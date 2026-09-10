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
/// Work instruction 305 task 4: **the FT8 decoder says it exists, every slot writes
/// a line, and the record says which mode the application was on.**
/// </summary>
/// <remarks>
/// <para>**THE FT8 DECODER ANNOUNCED NOTHING, SO ITS SILENCE WAS UNREADABLE.** The
/// file from 2026-09-10 carried `decoder_started` with `pitchHz 600` and two
/// `decode_quality` events counting elements and characters - **all three of those
/// are the CW decoder.**</para>
/// <para>**AND A SLOT THAT WAS NEVER CUT LEFT NO TRACE AT ALL.** The only route to
/// an `ft8_slot` event was a slot that had already been cut and decoded, so a
/// session that cut none wrote nothing, which is exactly what *no `ft8_slot` events*
/// meant in that file.</para>
/// <para>**THE MODE IS THE FIELD EVERY OTHER FIELD IS READ THROUGH.** Nothing in
/// seventeen events said whether the operator was on CW or on the Digital tab, and
/// an absence of slots means one thing on one and nothing at all on the other.</para>
/// </remarks>
public sealed class Unit305DecoderTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the record is printed.</param>
    public Unit305DecoderTests(ITestOutputHelper output) => _output = output;

    /// <summary>**A slot that is never cut leaves a line saying why.**</summary>
    [AvaloniaFact]
    public void ASlotThatIsNeverCutLeavesALineSayingWhy()
    {
        var lines = Session(panel =>
        {
            // **NO MEASURED OFFSET AND A TAP WITH AUDIO IN IT**, which is the state
            // the cutter refuses in, and the state the operator was in.
            panel.ClockOffset = ClockOffset.Unknown;
            panel.LookForASlotForTests();
        });

        var slots = lines
            .Where(l => l.Contains("\"event\":\"ft8_slot\"", StringComparison.Ordinal))
            .ToList();

        foreach (var line in slots)
        {
            _output.WriteLine("  " + line);
        }

        _output.WriteLine("");
        _output.WriteLine("ft8_slot lines written: " + slots.Count);

        // **THE ABSENCE IS NOW A LINE.** Before this unit a session that cut no
        // slot wrote no `ft8_slot` at all, and nothing separated that from a
        // decoder that was never running.
        Assert.NotEmpty(slots);

        Assert.Contains(
            slots, l => l.Contains("\"outcome\":\"refused\"", StringComparison.Ordinal));

        // **AND THE REFUSAL CARRIES THE REASON IN THE OPERATOR'S OWN WORDS.**
        Assert.Contains(
            slots,
            l => l.Contains("clock offset has not been measured", StringComparison.Ordinal));
    }

    /// <summary>**The digital decoder announces itself, once.**</summary>
    [AvaloniaFact]
    public void TheDigitalDecoderAnnouncesItselfOnce()
    {
        var lines = Session(panel =>
        {
            panel.LookForASlotForTests();
            panel.LookForASlotForTests();
            panel.LookForASlotForTests();
        });

        var starts = lines
            .Where(l => l.Contains("digital_decoder_started", StringComparison.Ordinal))
            .ToList();

        foreach (var line in starts)
        {
            _output.WriteLine("  " + line);
        }

        // **IT SAYS IT EXISTS**, which the FT8 side never did.
        var only = Assert.Single(starts);

        Assert.Contains("\"mode\":\"Ft8\"", only, StringComparison.Ordinal);
        Assert.Contains("\"slotSeconds\":15", only, StringComparison.Ordinal);

        // **AND ONCE PER SHAPE, NOT ONCE PER TICK.** Three looks, one line.
        Assert.Single(starts);
    }

    /// <summary>**The snapshot says which mode the application was on.**</summary>
    [AvaloniaFact]
    public void TheSnapshotSaysWhichModeTheApplicationWasOn()
    {
        var lines = Session(panel => panel.ClockOffset = new ClockOffset(
            0.033, DateTime.UtcNow), settle: true);

        var settled = lines.Single(
            l => l.Contains(StartupSnapshot.EventName, StringComparison.Ordinal)
                 && l.Contains(
                     "\"" + StartupSnapshot.WhenField + "\":\""
                     + StartupSnapshot.Settled + "\"",
                     StringComparison.Ordinal));

        _output.WriteLine("  " + settled);

        Assert.Contains(
            "\"appOperatingMode\":\"Digital\"", settled, StringComparison.Ordinal);

        Assert.Contains("\"appDigitalMode\":\"Ft8\"", settled, StringComparison.Ordinal);

        // **AND SO DOES A STATE CHANGE.**
        var changed = lines.Where(
            l => l.Contains("state_changed", StringComparison.Ordinal)).ToList();

        foreach (var line in changed)
        {
            _output.WriteLine("  " + line);
        }

        Assert.Contains(
            changed, l => l.Contains("\"mode\":\"Digital\"", StringComparison.Ordinal));
    }

    /// <summary>One session, one act, its telemetry read back.</summary>
    private static List<string> Session(
        Action<MainWindowViewModel> act, bool settle = false)
    {
        var folder = Path.Combine(
            Path.GetTempPath(), "hamlet-unit305d-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(folder);

        try
        {
            using var telemetry = new JsonlTelemetry(folder, "1.12.269", _ => true);

            var settings = new AppSettings { ReconnectOnStartup = false };

            settings.Operator.Callsign = "KC3QIS";

            StartupFacts.Write(
                telemetry, settings, categoriesOn: settings.IsTelemetryEnabled);

            // **A REAL TAP WITH REAL AUDIO IN IT**, because the slot look needs
            // one and a sound card is what this machine does not have.
            var tap = new AudioTap();

            tap.Take(new float[12_000], 12_000);

            var panel = new MainWindowViewModel(settings, telemetry)
            {
                OperatingMode = "Digital",
                TapForTests = tap,
            };

            act(panel);

            if (settle)
            {
                var until = DateTime.UtcNow
                    .Add(MainWindowViewModel.SettledSnapshotAfter)
                    .AddSeconds(2);

                while (DateTime.UtcNow < until)
                {
                    Avalonia.Threading.Dispatcher.UIThread.RunJobs();
                }
            }

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
