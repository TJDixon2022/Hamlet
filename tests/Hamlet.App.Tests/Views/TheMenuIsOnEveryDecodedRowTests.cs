using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Controls;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Olivia;
using Hamlet.RadioEngine.Psk31;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// **Criterion 10.2: a right-click opens a menu on every decoded row** - R46(b), work
/// instruction 387 task 4, under section 6 ruling 2(a).
/// </summary>
/// <remarks>
/// <para>**WHAT WAS WRONG.** `Psk31CannedMenuFor` answered null on a row the parser read no
/// station out of, `SendMenuFor` answered null on the same row, and `SendFlyoutFor` then answered
/// null - so **the rows Tim most wants a capture from were the only rows with nothing on them at
/// all.** Unit 387 task 1 measured it over this same fixture: **3 of 9 rows offered a menu.**
/// </para>
/// <para>**WHAT THE RULING ALLOWS AND WHAT IT DOES NOT** (section 6 ruling 2(a)). R46(b) is later
/// than the 2026-09-06 *nothing is greyed, hidden, sorted away or disabled* rule and wins **only
/// where it speaks**: a line that cannot be sent because Hamlet does not know the operator's own
/// callsign is drawn grey with the word saying so, and **every other line keeps the note**. So
/// this guard asserts both halves, and it asserts the one that protects the operator hardest:
/// **no line that could be sent before this unit became unsendable.**</para>
/// <para>**NOTHING HERE ARMS A TRANSMISSION** (§0.2, §10). `Capture` records audio and decodes
/// nothing; *make a card anyway* opens a card. Every send on this panel still goes through one
/// click on a named button, and no test in this file presses one.</para>
/// <para>**NOTHING IS KEYED AND NO PORT IS OPENED** (FACT-004).</para>
/// </remarks>
public sealed class TheMenuIsOnEveryDecodedRowTests
{
    /// <summary>The station the fixture rows are framed for.</summary>
    private const string His = "W1AW";

    /// <summary>The operator on the fixture panel.</summary>
    private const string Mine = "KC3QIS";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the guard.</summary>
    /// <param name="output">Where every row's menu is printed.</param>
    public TheMenuIsOnEveryDecodedRowTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// **Every decoded row opens a menu - callsign read or not, live or ended, PSK31 or Olivia -
    /// and `Capture` and *make a card anyway* are on every one of them.**
    /// </summary>
    [Fact]
    public void EveryRowOpensAMenuAndCaptureAndTheCardLineAreOnAllOfThem()
    {
        var misses = new List<string>();
        var looked = 0;
        var withAMenu = 0;

        foreach (var (what, model, rows) in Panels())
        {
            _output.WriteLine("-- " + what + " -------------------------------------");

            foreach (var row in rows)
            {
                var station = model.Psk31StationOn(row);
                var flyout = MainWindow.SendFlyoutFor(model, row);
                var where = "[" + Short(row.Message) + "]";

                looked++;

                if (flyout is null)
                {
                    misses.Add(where + ": the right-click offers nothing at all (10.2).");
                    continue;
                }

                withAMenu++;

                var headers = flyout.Items.OfType<MenuItem>()
                    .Select(i => i.Header?.ToString() ?? "")
                    .ToList();

                _output.WriteLine(
                    "  " + where.PadRight(44)
                    + " station " + (station ?? "-").PadRight(8)
                    + " | " + headers.Count + " items");

                foreach (var header in headers)
                {
                    _output.WriteLine("      " + Short(header));
                }

                if (!headers.Any(h => h.StartsWith("Capture", StringComparison.Ordinal)))
                {
                    misses.Add(where + ": Capture is not on the menu, and R46(b) says it always is.");
                }

                if (!headers.Any(h => h.StartsWith("Make a card anyway", StringComparison.Ordinal)))
                {
                    misses.Add(where + ": *make a card anyway* is not on the menu, and R46(b) says it always is.");
                }
            }

            _output.WriteLine("");
        }

        _output.WriteLine("ROWS OFFERING A MENU: " + withAMenu + " of " + looked
            + " (unit 387 task 1 measured 3 of 9 before this unit)");

        Assert.True(misses.Count == 0, string.Join(Environment.NewLine, misses));
        Assert.Equal(looked, withAMenu);
    }

    /// <summary>
    /// **Capture is live on every row, and it is the panel's own press and not a second one.**
    /// </summary>
    /// <remarks>
    /// The audio is the band's rather than the row's, so what Hamlet could not read is exactly
    /// what is worth keeping - which is why this line is never a note.
    /// </remarks>
    [Fact]
    public void CaptureIsLiveOnEveryRowAndItIsThePanelsOwnCommand()
    {
        var misses = new List<string>();

        foreach (var (what, model, rows) in Panels())
        {
            foreach (var row in rows)
            {
                var always = model.RowMenuAlwaysFor(row);
                var capture = always[0];
                var where = what + " [" + Short(row.Message) + "]";

                if (!capture.IsLive)
                {
                    misses.Add(where + ": Capture carries no command, so it cannot be pressed.");
                }

                if (!ReferenceEquals(capture.Command, model.CapturePsk31Command))
                {
                    misses.Add(
                        where + ": Capture is not the panel's own CapturePsk31Command, which"
                        + " would be a second way to start a capture.");
                }

                if (capture.Disabled)
                {
                    misses.Add(where + ": Capture is drawn grey, and it can always be pressed.");
                }
            }
        }

        Assert.True(misses.Count == 0, string.Join(Environment.NewLine, misses));
    }

    /// <summary>
    /// **A line that needs the operator's own callsign is drawn grey and says why; everything
    /// else keeps the 2026-09-06 note.**
    /// </summary>
    [Fact]
    public void OnlyTheLinesThatNeedHisCallsignAreDrawnGreyAndTheyCarryTheWord()
    {
        // The same panel, with nothing in Settings - which is the state R46(b) is about.
        var model = Panel("PSK31", callsign: "");

        model.ShowPsk31ChannelsForTests(new[]
        {
            new Psk31Channel(1, 500, 10, "CQ CQ CQ de " + His + " " + His + " pse K\n", true),
        }.ToList());

        var row = model.DigitalDecodes.First(r => r.IsTextOnly);
        var entries = model.Psk31CannedMenuFor(row);

        Assert.NotNull(entries);

        var grey = new List<string>();

        foreach (var entry in entries!)
        {
            _output.WriteLine(
                (entry.Disabled ? "GREY " : entry.IsNote ? "note " : "live ") + Short(entry.Label));

            if (entry.Disabled)
            {
                grey.Add(entry.Label);
            }
        }

        // **AT LEAST ONE LINE IS GREY, AND EVERY GREY LINE SAYS WHY IN A WORD.**
        Assert.NotEmpty(grey);

        foreach (var label in grey)
        {
            Assert.Contains("callsign", label, StringComparison.OrdinalIgnoreCase);
        }

        // **AND A GREY LINE CARRIES NO COMMAND**, so nothing can be sent off it by accident.
        Assert.All(entries.Where(e => e.Disabled), e => Assert.Null(e.Command));

        // **NOTHING ELSE IS GREY**: every other line is either live or a note, which is the
        // 2026-09-06 rule and ruling 2(a) item 2.
        Assert.All(entries.Where(e => !e.Disabled), e => Assert.True(e.IsLive || e.IsNote));
    }

    /// <summary>
    /// **No line that could be sent before this unit became unsendable** - ruling 2(a) item 2, and
    /// the thing that would mean the change had gone too far.
    /// </summary>
    /// <remarks>
    /// Measured on a fully filled-in operator, which is the case where every line that can exist
    /// does: with his callsign, name, location and grid in Settings nothing needs a grey line at
    /// all, so **not one entry on any row may be disabled**.
    /// </remarks>
    [Fact]
    public void WithSettingsFilledInNotOneLineIsDrawnGreyAndTheSendableOnesStillSend()
    {
        var misses = new List<string>();
        var live = 0;

        foreach (var (what, model, rows) in Panels())
        {
            foreach (var row in rows)
            {
                var entries = model.Psk31CannedMenuFor(row);

                if (entries is null)
                {
                    continue;
                }

                foreach (var entry in entries)
                {
                    if (entry.Disabled)
                    {
                        misses.Add(
                            what + " [" + Short(row.Message) + "]: [" + Short(entry.Label)
                            + "] is drawn grey with his callsign, name, location and grid all set."
                            + " A sendable line has become unsendable.");
                    }

                    if (entry.IsLive)
                    {
                        live++;
                    }
                }
            }
        }

        _output.WriteLine("live send lines across the fixture: " + live);

        Assert.True(misses.Count == 0, string.Join(Environment.NewLine, misses));
        Assert.True(live > 0, "the fixture produced no sendable lines at all, so this proves nothing");
    }

    // -------------------------------------------------------------------------------------

    private IEnumerable<(string What, MainWindowViewModel Model, List<DigitalDecodeRow> Rows)> Panels()
    {
        var psk31 = Psk31Panel(out var psk31Rows);

        yield return ("PSK31, six carriers - three naming a station, three not", psk31, psk31Rows);

        var olivia = OliviaPanel(out var oliviaRows);

        yield return ("OLIVIA, four channels - two naming a station, two not", olivia, oliviaRows);
    }

    private static MainWindowViewModel Psk31Panel(out List<DigitalDecodeRow> rows)
    {
        var model = Panel("PSK31");

        model.ShowPsk31ChannelsForTests(new[]
        {
            new Psk31Channel(1, 500, 10, "CQ CQ CQ de " + His + " " + His + " pse K\n", true),
            new Psk31Channel(2, 750, 11, Mine + " de K3ABC  RST 599 599  BTU " + Mine + " de K3ABC K\n", true),
            new Psk31Channel(3, 1000, 12, "K9XYZ de N2DEF  R R TNX 73 73 SK\n", true),
            new Psk31Channel(4, 1250, 13, "", false),
            new Psk31Channel(5, 1500, 14, "...-.- the band is long tonight and the qsb is deep\n", true),
            new Psk31Channel(6, 1750, 15, "e5 ttt tu ee\n", true),
        }.ToList());

        rows = model.DigitalDecodes.Where(r => r.IsTextOnly).ToList();

        return model;
    }

    private static MainWindowViewModel OliviaPanel(out List<DigitalDecodeRow> rows)
    {
        var model = Panel("Olivia");

        model.ShowOliviaChannelsForTests(new[]
        {
            new OliviaChannel(1, "8/250", 1000, OliviaListener.FoundByRsid, 0, "CQ CQ CQ de " + His + " " + His + " pse K\n", 9, 0, false),
            new OliviaChannel(2, "16/500", 1500, OliviaListener.FoundByRsid, 0, "K9XYZ de N2DEF  R R TNX 73 73 SK\n", 9, 0, true),
            new OliviaChannel(3, "4/500", 2000, OliviaListener.FoundBlind, 0, "Hk7DYYYzfzYXTDYY", 1, 11, false),
            new OliviaChannel(4, "32/1000", 2400, OliviaListener.FoundBlind, 0, "", 0, 4, false),
        });

        rows = model.DigitalDecodes.Where(r => r.IsTextOnly).ToList();

        return model;
    }

    private static MainWindowViewModel Panel(string mode, string? callsign = null)
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = callsign ?? Mine;
        settings.Operator.GridSquare = callsign is null ? "FN00DJ" : "";
        settings.Operator.OperatorName = callsign is null ? "Tim" : "";
        settings.Operator.Location = callsign is null ? "Pennsylvania" : "";
        settings.Operator.LicenseClass = Hamlet.RadioEngine.Licensing.LicenseClass.General;

        var model = new MainWindowViewModel(settings, null)
        {
            OperatingMode = "Digital",
            DigitalNewestFirst = false,
        };

        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>(StringComparer.OrdinalIgnoreCase));
        model.ChooseDigitalModeCommand.Execute(mode);

        return model;
    }

    private static string Short(string? text)
    {
        var one = (text ?? "").Replace("\n", " / ", StringComparison.Ordinal).Trim();

        return one.Length <= 76 ? one : one[..73] + "...";
    }
}
