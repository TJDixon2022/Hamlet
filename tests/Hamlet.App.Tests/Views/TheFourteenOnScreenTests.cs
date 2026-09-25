using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Controls;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Olivia;
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Telemetry;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Work instruction 427 task 2: **the owner's items 1 to 13, driven, and what is actually there
/// printed.** Asserts nothing.
/// </summary>
/// <remarks>
/// <para>**TICKED AND RIGHT HAVE COME APART TWICE THIS WEEK**, so each item is driven through the
/// view model or a realized headless window and its property, text and numbers printed. A fact
/// that throws prints the exception as UNVERIFIABLE rather than failing, because a finding is not
/// a failure here.</para>
/// <para>**NOTHING IS KEYED AND NO PORT IS OPENED** (FACT-004, §0.2). The two sends driven here
/// go over `FakePort` and `FakeSink`.</para>
/// </remarks>
public sealed class TheFourteenOnScreenTests : IDisposable
{
    private const string Mine = "KC3QIS";
    private const string His = "W1AW";
    private const string Him = "W1ABC";

    private const string HisOver = Mine + " de " + Him + " GM TNX CALL UR 599 599 BTU " + Mine + " de " + Him + " K\n";
    private const string MidOver = HisOver + Mine + " de " + Him + " R R NAME BOB QTH ERIE AND THE RIG HERE IS";

    private readonly ITestOutputHelper _output;
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "hamlet-427-" + Guid.NewGuid().ToString("N"));

    /// <summary>Creates the sweep.</summary>
    /// <param name="output">Where every item is printed.</param>
    public TheFourteenOnScreenTests(ITestOutputHelper output)
    {
        _output = output;
        Directory.CreateDirectory(_folder);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        try
        {
            Directory.Delete(_folder, true);
        }
        catch (IOException)
        {
        }
    }

    /// <summary>**1: the right-click menu, on a row with a callsign and on one without.**</summary>
    [AvaloniaFact]
    public void Item01TheMenu() => Item("1 the right-click menu", () =>
    {
        var model = Panel("PSK31");

        model.ShowPsk31ChannelsForTests(new[]
        {
            new Psk31Channel(1, 500, 10, "CQ CQ CQ de " + His + " " + His + " pse K\n", true),
            new Psk31Channel(6, 1750, 15, "e5 ttt tu ee\n", true),
        }.ToList());

        foreach (var row in model.DigitalDecodes.Where(r => r.IsTextOnly).ToList())
        {
            var flyout = MainWindow.SendFlyoutFor(model, row);

            Say("row [" + Short(row.Message) + "] station " + (model.Psk31StationOn(row) ?? "none")
                + ": menu " + (flyout is null ? "DOES NOT OPEN" : "opens, " + flyout.Items.Count + " items"));

            foreach (var item in flyout?.Items.OfType<MenuItem>() ?? Enumerable.Empty<MenuItem>())
            {
                Say("    " + (item.IsEnabled ? "enabled " : "DISABLED") + " [" + Short(item.Header?.ToString()) + "]");
            }

            var always = model.RowMenuAlwaysFor(row);
            var card = always[1];
            var before = model.DigitalCards.Count;

            if (card.Command is { } open && open.CanExecute(card.Parameter))
            {
                open.Execute(card.Parameter);
            }

            Say("    make a card anyway: " + (card.IsLive ? "a command" : card.IsNote ? "A NOTE" : "grey")
                + "; pressing it: cards " + before + " -> " + model.DigitalCards.Count
                + (model.DigitalCards.Count > before ? " (a CARD)" : " (no card)"));
        }

        var noCall = Panel("PSK31", callsign: "");

        noCall.ShowPsk31ChannelsForTests(new[]
        {
            new Psk31Channel(1, 500, 10, "CQ CQ CQ de " + His + " " + His + " pse K\n", true),
        }.ToList());

        var entries = noCall.Psk31CannedMenuFor(noCall.DigitalDecodes.First(r => r.IsTextOnly)) ?? Array.Empty<Psk31CannedEntry>();

        Say("with no callsign in Settings, the canned list:");

        foreach (var entry in entries)
        {
            Say("    " + (entry.Disabled ? "DISABLED" : entry.IsNote ? "note    " : "live    ") + " [" + Short(entry.Label) + "]");
        }
    });

    /// <summary>**2: the row hover, whole, and whether the text is in it.**</summary>
    [AvaloniaFact]
    public void Item02TheRowHover() => Item("2 the row hover", () =>
    {
        var model = Panel("PSK31");

        model.ShowPsk31ChannelsForTests(new[]
        {
            new Psk31Channel(1, 1500, 12, "CQ CQ CQ de VE3YX VE3YX FN03 pse K\n", true),
            new Psk31Channel(2, 1000, 10, "CQ CQ CQ de " + His + " " + His + " pse K\n", true),
            new Psk31Channel(3, 750, 11, Mine + " de K3ABC  RST 599 599 QTH FN10 BTU " + Mine + " de K3ABC K\n", true),
        }.ToList());

        foreach (var row in model.DigitalDecodes.Where(r => r.IsTextOnly))
        {
            Say("row [" + Short(row.Message) + "], hover whole:");

            foreach (var line in row.RowFacts.Split('\n'))
            {
                Say("    | " + line);
            }

            Say("    the transmission's text in it: " + (row.RowFacts.Contains("pse K", StringComparison.Ordinal)
                || row.RowFacts.Contains("CQ CQ", StringComparison.Ordinal)
                || row.RowFacts.Contains("RST 599", StringComparison.Ordinal) ? "YES" : "no"));
        }
    });

    /// <summary>**3 and 4: the band, the pills and the sun map at 1600 and 1300 wide.**</summary>
    [AvaloniaFact]
    public void Item03And04TheTopAndTheSunMap() => Item("3 and 4 the top band and the sun map", () =>
    {
        foreach (var width in new[] { 1600.0, 1300.0 })
        {
            var window = TheTopRowTests.Realized(width, TheTopRowTests.WindowHeight, null, null);

            try
            {
                Unit376TheTopBandTests.Pump(window);

                var band = Unit376TheTopBandTests.Band(window);
                var pill = window.GetVisualDescendants().OfType<Button>().First(b => b.Classes.Contains("hm-band"));
                var map = window.GetVisualDescendants().OfType<Control>().FirstOrDefault(c => c.Name == "GreenZoneMap");
                var drive = band.Drive;

                Say("at " + Px(width) + " x " + Px(TheTopRowTests.WindowHeight) + ":");
                Say("    band with pills " + Px(band.WithPills) + " px, without " + Px(band.WithoutPills)
                    + " px; pills row " + Px(band.Pills.Height) + " px, one pill " + Px(pill.Bounds.Height) + " px");
                Say("    rig face " + Box(band.Rig) + ", drive and power " + Box(drive)
                    + (drive.Top < band.Rig.Bottom && drive.Left >= band.Rig.Left - 0.5 ? " (beside/inside the rig column)" : ""));

                if (map is null)
                {
                    Say("    sun map: absent from the visual tree");
                }
                else
                {
                    var at = TheTopRowTests.RectIn(map, window);

                    Say("    sun map " + (map.IsEffectivelyVisible ? "drawn" : "NOT DRAWN") + " at " + Box(at)
                        + ", " + Px(at.Width) + " x " + Px(at.Height)
                        + "; its left edge " + Px(at.Left) + " against the band's " + Px(Math.Min(band.Card.Left, band.Pills.Left))
                        + "; the band's height " + Px(band.WithoutPills));
                }
            }
            finally
            {
                window.Close();
            }
        }
    });

    /// <summary>**5: PSK31 and Olivia against the Modes badge, the Hall of Fame, the records and the quill.**</summary>
    [AvaloniaFact]
    public void Item05TheKeyboardModesEarn() => Item("5 the keyboard modes earn", () =>
    {
        foreach (var name in new[] { "FT8", "PSK31", ContactModes.OliviaName })
        {
            var mode = ContactModes.Logged.First(m => string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase));
            var log = new AchievementLog(new[] { Fixture(mode, "VE3YX", "FN03") }, "FN00");

            Say(name.PadRight(7) + " " + string.Join(", ", AchievementKinds.All.Select(k => k + " " + AchievementScores.WorkedIn(k, log)))
                + " | firsts: " + string.Join(", ", AchievementScores.FirstsEarned(log)));
        }

        Say("per-contact records: not re-driven here; TheKeyboardModesEarnWhatFt8EarnsTests.TheRecordsBehindTheCountsAreTheSameRecordsInEveryMode drives them through the log and was green at entry");

        foreach (var name in new[] { "PSK31", "Olivia" })
        {
            var model = Panel(name);
            var worked = new[] { DxccPrefixes.EntityOf("W9ZZZ")! };

            model.UseNudgeSetForTests(new NudgeSet(worked, worked.Select(DxccContinents.Of)));

            var said = "CQ CQ CQ de VE3YX VE3YX pse K\n";

            if (name == "PSK31")
            {
                model.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(1, 1000, 10, said, true) }.ToList());
            }
            else
            {
                model.ShowOliviaChannelsForTests(new[] { new OliviaChannel(1, "8/250", 1000, OliviaListener.FoundByRsid, 0, said, 9, 0, false) });
            }

            var row = model.DigitalDecodes.First(r => r.IsTextOnly);

            Say(name + " row for VE3YX (Canada, not worked; North America worked): quill " + row.Nudge + " [" + row.NudgeTip + "]");
        }
    });

    /// <summary>**6: under Olivia, which chip is filled, and the send line after a CQ.**</summary>
    [AvaloniaFact]
    public void Item06TheChipAndTheSendLine() => Item("6 the mode chip and the send line", () =>
    {
        using var telemetry = new JsonlTelemetry(_folder, "427-6", _ => true);

        var model = Panel("Olivia", telemetry: telemetry, dialHz: 14_071_500);

        foreach (var chip in model.DigitalModeChips)
        {
            Say("    chip " + chip.Label.PadRight(7) + " filled " + chip.IsFilled + ", chosen " + chip.IsChosen + ", dial in its block " + chip.IsLit);
        }

        model.TapForTests = new Hamlet.RadioEngine.Audio.AudioTap();

        var port = new FakePort();
        var sink = new FakeSink();

        model.UseRigPortForTests(port);
        model.UseArmedSendForTests(new Ft8ArmedSend(new Ft8TransmitSequence(port, sink, guard: null, telemetry)));
        model.SendCallToAnyoneCommand.Execute(null);

        for (var tries = 0; tries < 2000 && model.HasSomethingToStop; tries++)
        {
            Thread.Sleep(10);
        }

        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        Say("an Olivia CQ over the fakes: sound-card calls " + sink.TimesCalled + "; the send line reads [" + model.DigitalSendLine + "]");
    });

    /// <summary>**7: the dismiss X, and whether the press is recorded.**</summary>
    [AvaloniaFact]
    public void Item07TheX() => Item("7 the dismiss X", () =>
    {
        var window = new MainWindow { DataContext = PanelWithHisCard(out var telemetry), Width = 1600, Height = 1040 };
        var model = (MainWindowViewModel)window.DataContext!;

        window.Show();
        Unit376TheTopBandTests.Pump(window);

        try
        {
            var xs = window.GetVisualDescendants().OfType<Button>().Where(b => b.Classes.Contains("hm-cardx")).ToList();

            Say("cards on the panel " + model.DigitalCards.Count + "; X buttons drawn " + xs.Count(b => b.IsEffectivelyVisible)
                + (xs.Count > 0 ? ", reading [" + xs[0].Content + "], hover [" + ToolTip.GetTip(xs[0]) + "]" : ""));

            model.ClearCardCommand.Execute(model.DigitalCards[0].Callsign);
            model.FlushOnScreenForTests();
            telemetry.Dispose();

            var lines = Directory.GetFiles(_folder, "*.jsonl").SelectMany(File.ReadAllLines).ToList();

            Say("after the X: cards " + model.DigitalCards.Count
                + "; record carries card_dismissed " + lines.Count(l => l.Contains("\"card_dismissed\"", StringComparison.Ordinal))
                + " time(s), a card removed by dismissed " + lines.Count(l => l.Contains("\"removed\"", StringComparison.Ordinal) && l.Contains("\"dismissed\"", StringComparison.Ordinal)));
        }
        finally
        {
            window.Close();
        }
    });

    /// <summary>**8: a live carrier - the row, the card, the word, and the send buttons.**</summary>
    [AvaloniaFact]
    public void Item08TheLiveCarrier() => Item("8 his live carrier", () =>
    {
        using var telemetry = new JsonlTelemetry(_folder, "427-8", _ => true);

        var model = Panel("PSK31", telemetry: telemetry, dialHz: 14_070_000);
        var port = new FakePort();
        var sink = new FakeSink();

        model.UseRigPortForTests(port);
        model.UseArmedSendForTests(new Ft8ArmedSend(new Ft8TransmitSequence(port, sink, guard: null, telemetry)));

        foreach (var (what, text) in new[] { ("mid-over", MidOver), ("after his K", HisOver + Mine + " de " + Him + " R R NAME BOB BTU " + Mine + " de " + Him + " K\n") })
        {
            model.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(5, 1500, 10.0, text) });

            var row = model.DigitalDecodes.First(r => r.IsTextOnly);
            var card = model.DigitalCards.First(c => c.Callsign == Him);

            Say(what + ": row ended " + row.Ended + ", row word [" + row.SendingWord + "]; card live " + card.CarrierIsLive
                + ", card word [" + card.SendingWord + "], turn [" + card.TurnWord + "], offer note [" + card.OfferNote
                + "], action pressable " + card.CanPressAction + ", hold " + model.HisCarrierIsLive(Him));

            card.TypedText = "HELLO OM";
            model.SendTypedPsk31Command.Execute(card);

            for (var tries = 0; tries < 200 && model.HasSomethingToStop; tries++)
            {
                Thread.Sleep(10);
            }

            Say("    a typed send pressed: sound-card calls so far " + sink.TimesCalled + ", send line [" + model.DigitalSendLine + "]");
        }

        model.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(5, 1500, 10.0, MidOver) });
        model.ShowPsk31ChannelsForTests(Array.Empty<Psk31Channel>());

        Say("mid-over and then his carrier drops, with no K: hold " + model.HisCarrierIsLive(Him)
            + " - THE HOLD ALSO RELEASES ON CARRIER DROP, not only on K or BTU");
    });

    /// <summary>**9: Log on the card, and which of its fields are editable and how they are marked.**</summary>
    [AvaloniaFact]
    public void Item09TheLog() => Item("9 Log and the RST fields", () =>
    {
        var model = PanelWithHisCard(out var telemetry);
        var card = model.DigitalCards[0];

        Say("half an exchange in: Log on the card " + card.ShowsLogLink + " [" + card.LogLabel + "]");
        telemetry.Dispose();

        var dialog = new LogContactViewModel(new AdifContact
        {
            Call = Him, StationCallsign = Mine, Band = "20m", Mode = "PSK", Submode = "PSK31",
            StartedUtc = new DateTime(2026, 9, 24, 17, 44, 0, DateTimeKind.Utc), RstSent = null, RstReceived = "599",
        });

        foreach (var field in dialog.Fields)
        {
            Say("    " + field.AdifField.PadRight(14) + (field.IsReport ? "EDITABLE  [" + field.Report!.Text + "] " + field.Report.Mark : "read-only [" + field.Value + "]"));
        }

        var typed = dialog.Fields.First(f => f.AdifField == "RST_SENT").Report!;

        typed.Text = "579";

        Say("    RST_SENT typed 579: [" + typed.Text + "] " + typed.Mark);
    });

    /// <summary>**10: what the turn does on a certain hand-back and on a guessed one.**</summary>
    [Fact]
    public void Item10TheTurn() => Item("10 the turn on every hand-back", () =>
    {
        var model = PanelWithHisCard(out var telemetry);
        var first = model.DigitalCards[0];

        Say("certain hand-back: [" + first.TurnWord + "] guess " + first.TurnIsGuess + ", offered " + first.Offered);

        model.CardActionCommand.Execute(first);

        for (var tries = 0; tries < 200 && model.HasSomethingToStop; tries++)
        {
            Thread.Sleep(10);
        }

        Say("after Hamlet answered: [" + model.DigitalCards[0].TurnWord + "]");

        model.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(5, 1500, 10.0, HisOver + Mine + " de " + Him + " R R NAME BOB 5#9 QTH ERIE BTU " + Mine + " de " + Him + " K\n") });

        var second = model.DigitalCards[0];

        Say("guessed hand-back: [" + second.TurnWord + "] guess " + second.TurnIsGuess + ", offered " + second.Offered + ", note [" + second.OfferNote + "]");
        telemetry.Dispose();
    });

    /// <summary>**11: the favorites - chips, where they sit, their colors, the star, and the empty words.**</summary>
    [AvaloniaFact]
    public void Item11TheFavorites() => Item("11 favorites", () =>
    {
        var settings = TheTopRowTests.FixtureSettings();

        settings.Favorites.Add(new SavedFavorite { FrequencyHz = 14_074_000, Name = "the FT8 watering hole", Mode = "USB-D", BandName = "20 m" });
        settings.Favorites.Add(new SavedFavorite { FrequencyHz = 14_050_000, Name = "slow CW on Sunday", Mode = "CW", BandName = "20 m" });

        var window = TheTopRowTests.Realized(1920, TheTopRowTests.WindowHeight, settings);

        try
        {
            var chips = TheTopRowTests.Named<ItemsControl>(window, "GreenZoneFavorites").GetVisualDescendants().OfType<Button>()
                .Where(b => b.Classes.Contains("hm-favchip")).ToList();
            var block = TheTopRowTests.RectIn(TheTopRowTests.Named<Border>(window, "GreenZoneBlock"), window);
            var model = (MainWindowViewModel)window.DataContext!;
            var rig = window.GetVisualDescendants().OfType<RigDisplayControl>().Single();

            Say("green block " + Box(block) + "; " + chips.Count + " chips (buttons of class hm-favchip)");

            foreach (var chip in chips)
            {
                var texts = chip.GetVisualDescendants().OfType<TextBlock>().Where(t => t.IsEffectivelyVisible).ToList();

                Say("    chip at " + Box(TheTopRowTests.RectIn(chip, window)) + (TheTopRowTests.RectIn(chip, window).Top >= block.Bottom - 0.5 ? " (under the green block)" : " (NOT under it)")
                    + ": " + string.Join(" ", texts.Select(t => "[" + t.Text + "] ink " + t.Foreground)));
            }

            model.FrequencyHz = 14_060_000;
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            var off = rig.IsFavorite;

            model.FrequencyHz = 14_050_000;
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            Say("star filled off a saved spot " + off + ", on a saved spot " + rig.IsFavorite);
        }
        finally
        {
            window.Close();
        }

        var empty = TheTopRowTests.Realized(1920, TheTopRowTests.WindowHeight, TheTopRowTests.FixtureSettings());

        try
        {
            var words = TheTopRowTests.Named<TextBlock>(empty, "GreenZoneFavoritesEmpty");

            Say("empty, verbatim: [" + words.Text + "] drawn " + words.IsEffectivelyVisible);
        }
        finally
        {
            empty.Close();
        }
    });

    /// <summary>**12: a blind-found carrier before the decoder is confident.**</summary>
    [AvaloniaFact]
    public void Item12TheBlindCarrier() => Item("12 a blind-found carrier", () =>
    {
        var olivia = Panel("Olivia");

        olivia.ShowOliviaChannelsForTests(new[]
        {
            new OliviaChannel(1, "4/500", 1500, OliviaListener.FoundBlind, 0, "Hk7DYYYzfzYXTDYY", 1, 11, false),
            new OliviaChannel(2, "32/1000", 2400, OliviaListener.FoundBlind, 0, "", 0, 4, false),
        });

        foreach (var row in olivia.DigitalDecodes.Where(r => r.IsTextOnly))
        {
            Say("Olivia blind " + row.Variant + ": [" + row.Message + "] heard-not-readable " + row.HeardNotReadable);
        }

        var psk31 = Panel("PSK31");

        psk31.ShowPsk31ChannelsForTests(new[]
        {
            new Psk31Channel(1, 1000, 3, "e5 ttt tu ee\n", false),
            new Psk31Channel(2, 1250, 3, "", false),
        }.ToList());

        foreach (var row in psk31.DigitalDecodes.Where(r => r.IsTextOnly))
        {
            Say("PSK31 carrier not yet readable: [" + row.Message + "] heard-not-readable " + row.HeardNotReadable);
        }
    });

    /// <summary>**13: KC3QIS de VE3YX, mid-over - does a card open, how is it marked, and the buttons.**</summary>
    [AvaloniaFact]
    public void Item13HisCardWhileHeTalks() => Item("13 his card while he is talking", () =>
    {
        var text = Mine + " de VE3YX GM OM TNX FER THE CALL NAME HERE IS";

        foreach (var name in new[] { "PSK31", "Olivia" })
        {
            var model = Panel(name);

            if (name == "PSK31")
            {
                model.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(5, 1500, 10.0, text, true) });
            }
            else
            {
                model.ShowOliviaChannelsForTests(new[] { new OliviaChannel(1, "8/250", 1000, OliviaListener.FoundByRsid, 0, text, 9, 0, false) });
            }

            var row = model.DigitalDecodes.First(r => r.IsTextOnly);
            var card = model.DigitalCards.FirstOrDefault(c => c.Callsign == "VE3YX");

            Say(name + ": card for VE3YX mid-over " + (card is null ? "DOES NOT OPEN" : "opens"));

            if (card is not null)
            {
                Say("    turn [" + card.TurnWord + "], word [" + card.SendingWord + "], offer note [" + card.OfferNote
                    + "], action [" + card.ActionLabel + "] pressable " + card.CanPressAction + ", can type " + card.CanType);
            }

            var flyout = MainWindow.SendFlyoutFor(model, row);

            Say("    right-click names " + (model.Psk31StationOn(row) ?? "nobody") + "; first items: "
                + string.Join(" | ", (flyout?.Items.OfType<MenuItem>() ?? Enumerable.Empty<MenuItem>()).Take(3).Select(i => Short(i.Header?.ToString()))));
        }
    });

    // -------------------------------------------------------------------------------------

    private void Item(string name, Action drive)
    {
        _output.WriteLine("=== ITEM " + name);

        try
        {
            drive();
        }
        catch (Exception ex)
        {
            _output.WriteLine("UNVERIFIABLE here: " + ex.GetType().Name + ": " + ex.Message);
        }
    }

    private void Say(string line) => _output.WriteLine(line);

    private MainWindowViewModel PanelWithHisCard(out JsonlTelemetry telemetry)
    {
        telemetry = new JsonlTelemetry(_folder, "427", _ => true);

        var model = Panel("PSK31", telemetry: telemetry, dialHz: 14_070_000);
        var port = new FakePort();

        model.UseRigPortForTests(port);
        model.UseArmedSendForTests(new Ft8ArmedSend(new Ft8TransmitSequence(port, new FakeSink(), guard: null, telemetry)));
        model.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(5, 1500, 10.0, HisOver) });

        return model;
    }

    private static MainWindowViewModel Panel(string mode, string? callsign = null, JsonlTelemetry? telemetry = null, long dialHz = 0)
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = callsign ?? Mine;
        settings.Operator.GridSquare = callsign is null ? "FN00DJ" : "";
        settings.Operator.OperatorName = callsign is null ? "Tim" : "";
        settings.Operator.Location = callsign is null ? "Pennsylvania" : "";
        settings.Operator.LicenseClass = LicenseClass.General;

        var model = new MainWindowViewModel(settings, telemetry)
        {
            OperatingMode = "Digital",
            DigitalNewestFirst = false,
        };

        if (dialHz > 0)
        {
            model.SelectedBand = model.Bands.First(b => b.Band.LowHz <= dialHz && b.Band.HighHz >= dialHz);
            model.FrequencyHz = dialHz;
        }

        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>(StringComparer.OrdinalIgnoreCase));
        model.ChooseDigitalModeCommand.Execute(mode);

        return model;
    }

    private static AdifLogRecord Fixture(ContactMode mode, string call, string grid)
        => new(
            new AdifContact
            {
                Call = call,
                StationCallsign = Mine,
                Band = "20m",
                Mode = mode.AdifMode,
                Submode = mode.AdifSubmode,
                GridSquare = grid,
                MyGridSquare = "FN00",
                StartedUtc = new DateTime(2026, 9, 19, 21, 0, 0, DateTimeKind.Utc),
                EndedUtc = new DateTime(2026, 9, 19, 21, 12, 0, DateTimeKind.Utc),
            },
            [],
            true);

    private static string Short(string? text)
    {
        var one = (text ?? "").Replace("\n", " / ", StringComparison.Ordinal).Trim();

        return one.Length <= 110 ? one : one[..107] + "...";
    }

    private static string Px(double value) => value.ToString("0.#", CultureInfo.InvariantCulture);

    private static string Box(Rect r) => "[" + Px(r.X) + "," + Px(r.Y) + " " + Px(r.Width) + "x" + Px(r.Height) + "]";
}
