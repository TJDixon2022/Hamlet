using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Headless.XUnit;
using Hamlet.App.Settings;
using Hamlet.App.Telemetry;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Telemetry;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Telemetry;

/// <summary>
/// Work instruction 305 task 6: **the honest test.**
/// </summary>
/// <remarks>
/// <para>**FIVE FAULTS, EACH CONSTRUCTED, EACH ASKED THE SAME QUESTION**: would the
/// file alone diagnose this, with no command and no conversation? **Five for five or
/// the standard is not met, and saying so is worth more than claiming it.**</para>
/// <para>**NOTHING HERE IS ARGUED FROM THE CODE.** Each case is built, run, and the
/// file it wrote is read back and quoted, because a standard checked by reading the
/// source is a standard checked by the person who wrote it.</para>
/// <para>**AND WHERE A CASE CANNOT BE STAGED ON THIS MACHINE IT SAYS SO** rather
/// than passing on a substitute (HM-DEC-093, §0.0).</para>
/// </remarks>
public sealed class Unit305HonestTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where each verdict is printed.</param>
    public Unit305HonestTests(ITestOutputHelper output) => _output = output;

    /// <summary>**Case 1: an offset that reaches the snapshot and no other reader.**</summary>
    [AvaloniaFact]
    public void CaseOneAnOffsetThatReachesOnlyTheSnapshot()
    {
        var lines = Session(panel => panel.ClockOffset = new ClockOffset(
            0.033, DateTime.UtcNow));

        var crossing = lines.Where(
            l => l.Contains("\"what\":\"clock_offset\"", StringComparison.Ordinal))
            .ToList();

        Quote("CASE 1 - today's own fault", crossing);

        _output.WriteLine(
            "YES. The crossing from unknown to known is its own line, with the "
            + "mode beside it. A file with `clock_query_finished` and no "
            + "`state_changed` for `clock_offset` says the measurement was taken "
            + "and never delivered, which is the exact shape of today's fault and "
            + "took a day to find by guessing.");

        Assert.NotEmpty(crossing);
        Assert.Contains(
            crossing, l => l.Contains("\"to\":\"known\"", StringComparison.Ordinal));
    }

    /// <summary>**Case 2: a CQ press that produces no transmission.**</summary>
    [AvaloniaFact]
    public void CaseTwoACqPressThatProducesNoTransmission()
    {
        var lines = Session(panel => panel.SendCallToAnyoneCommand.Execute(null));

        var trail = lines
            .Where(l => l.Contains("operator_action", StringComparison.Ordinal)
                        || l.Contains(SendStage.EventName, StringComparison.Ordinal)
                        || l.Contains(TransmitRecord.EventName, StringComparison.Ordinal))
            .ToList();

        Quote("CASE 2 - he pressed CQ and nothing went out", trail);

        _output.WriteLine(
            "YES. The press is a line, the send path's entry is a line, and each "
            + "stage it reached is a line - so the file names the last stage the "
            + "path got to. Here that is `read_back`, and nothing after it, which "
            + "reads as composed and never armed.");

        Assert.Contains(
            trail, l => l.Contains("cq_pressed", StringComparison.Ordinal));

        Assert.Contains(
            trail, l => l.Contains("\"stage\":\"composed\"", StringComparison.Ordinal));

        Assert.DoesNotContain(
            trail, l => l.Contains("\"stage\":\"keyed\"", StringComparison.Ordinal));
    }

    /// <summary>**Case 3: a slot that is never cut.**</summary>
    [AvaloniaFact]
    public void CaseThreeASlotThatIsNeverCut()
    {
        var lines = Session(
            panel =>
            {
                panel.ClockOffset = ClockOffset.Unknown;
                panel.LookForASlotForTests();
            },
            withTap: true);

        var slots = lines
            .Where(l => l.Contains("\"event\":\"ft8_slot\"", StringComparison.Ordinal)
                        || l.Contains("digital_decoder_started", StringComparison.Ordinal))
            .ToList();

        Quote("CASE 3 - no slot was ever cut", slots);

        _output.WriteLine(
            "YES. The decoder says it exists, and every refusal to cut is its own "
            + "`ft8_slot` line carrying the reason in the operator's own words. A "
            + "file with a decoder start and no slot at all now means the tab was "
            + "never looked at; a file with refusals means it was looking and could "
            + "not cut, and says why.");

        Assert.Contains(
            slots, l => l.Contains("digital_decoder_started", StringComparison.Ordinal));

        Assert.Contains(
            slots, l => l.Contains("\"outcome\":\"refused\"", StringComparison.Ordinal));
    }

    /// <summary>**Case 4: a decode that reaches no panel.**</summary>
    [AvaloniaFact]
    public void CaseFourADecodeThatReachesNoPanel()
    {
        var slot = new DateTime(2026, 9, 2, 14, 22, 15, DateTimeKind.Utc);

        var heard = new Ft8Reception(
            new[] { new Ft8Decode(slot, 0.2, 1240, 24, "CQ K1ABC FN42") }, 1, 1, "");

        var lines = Session(panel =>
        {
            panel.NoteSlot(heard);
            panel.NoteSlot(heard);
        });

        var drawn = lines
            .Where(l => l.Contains("decodes_drawn", StringComparison.Ordinal))
            .ToList();

        Quote("CASE 4 - it decoded and nothing appeared", drawn);

        _output.WriteLine(
            "YES. One line per slot says how many messages came out and how many "
            + "reached the table, and the two disagreeing is a warning rather than "
            + "a number somebody has to notice.");

        Assert.Contains(
            drawn,
            l => l.Contains("decoded_but_no_row_added", StringComparison.Ordinal)
                 && l.Contains("\"level\":\"warn\"", StringComparison.Ordinal));
    }

    /// <summary>**Case 5: receive audio collapsing while FT8 is running.**</summary>
    [AvaloniaFact]
    public void CaseFiveReceiveAudioCollapsingOnFt8()
    {
        var lines = Session(
            panel =>
            {
                panel.ClockOffset = ClockOffset.Unknown;
                panel.LookForASlotForTests();
            },
            withTap: true);

        var slots = lines
            .Where(l => l.Contains("\"event\":\"ft8_slot\"", StringComparison.Ordinal))
            .ToList();

        Quote("CASE 5 - the sound card went quiet on FT8", slots);

        _output.WriteLine(
            "YES, WITH ONE HONEST LIMIT. The tap's own peak, floor and "
            + "`nearlySilent` now ride every FT8 slot line, so the collapse from "
            + "-12.9 dB to -67.9 dB would be caught on the digital side rather "
            + "than only where the CW decoder happened to be sampling. **The limit "
            + "is that a slot line is written when a slot is cut or refused**; a "
            + "tab nobody is on writes none, and audio health while the operator "
            + "is on Voice is still nowhere.");

        Assert.Contains(
            slots, l => l.Contains("audioPeakDb", StringComparison.Ordinal));

        Assert.Contains(
            slots, l => l.Contains("\"nearlySilent\":true", StringComparison.Ordinal));
    }

    private void Quote(string title, IReadOnlyList<string> lines)
    {
        _output.WriteLine(title.ToUpperInvariant());
        _output.WriteLine("");

        foreach (var line in lines)
        {
            _output.WriteLine("  " + line);
        }

        _output.WriteLine("");
        _output.WriteLine("WOULD THE FILE ALONE DIAGNOSE IT?");
        _output.WriteLine("");
    }

    /// <summary>One session, one act, its telemetry read back.</summary>
    private static List<string> Session(
        Action<MainWindowViewModel> act, bool withTap = false)
    {
        var folder = Path.Combine(
            Path.GetTempPath(), "hamlet-unit305h-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(folder);

        try
        {
            using var telemetry = new JsonlTelemetry(folder, "1.12.269", _ => true);

            var settings = new AppSettings { ReconnectOnStartup = false };

            settings.Operator.Callsign = "KC3QIS";

            StartupFacts.Write(
                telemetry, settings, categoriesOn: settings.IsTelemetryEnabled);

            var panel = new MainWindowViewModel(settings, telemetry)
            {
                OperatingMode = "Digital",
            };

            if (withTap)
            {
                var tap = new AudioTap();

                tap.Take(new float[12_000], 12_000);
                panel.TapForTests = tap;
            }

            act(panel);

            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            telemetry.Dispose();

            return Directory.GetFiles(folder, "*.jsonl")
                .SelectMany(File.ReadAllLines)
                .ToList();
        }
        finally
        {
            try
            {
                Directory.Delete(folder, recursive: true);
            }
            catch (IOException)
            {
                // Not this test's business.
            }
        }
    }
}
