using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Headless.XUnit;
using Ft8Sharp.Encode;
using Ft8Sharp.Message;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Telemetry;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Telemetry;

/// <summary>
/// Work instruction 305 task 1: **follow the measured offset from the query to
/// every reader, on a running application, and prove a slot is cut afterwards.**
/// </summary>
/// <remarks>
/// <para>**THE DIAGNOSIS IS GIVEN AND THIS IS THE TRACE.** One telemetry file from
/// the operator's machine holds `clock_query_finished` with `offsetSeconds 0.033`
/// and a settled `startup_snapshot` carrying the same figure, while the panel beside
/// it said the clock had never been checked. **Both of those readers are on this one
/// object**, so the divergence was never two copies of the value - it was five
/// surfaces being told and a sixth not.</para>
/// <para>**UNIT 284'S METHOD.** The application is stood up rather than reasoned
/// about, because the author has been wrong about where this lives all day.</para>
/// <para>**MEASURED BEFORE THE MEND** (2026-09-10): setting the property raised
/// `ClockOffset`, `ClockOffsetLine`, `ClockIsConcerning`, `DigitalWaterfallSummary`,
/// `DigitalReadinessLine` and `HasDigitalReadiness`, and **never `DigitalCardsIdle`**
/// - which is the panel that carried the sentence he read.</para>
/// </remarks>
public sealed class Unit305WireTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the trace is printed.</param>
    public Unit305WireTests(ITestOutputHelper output) => _output = output;

    /// <summary>The rate the band is simulated at.</summary>
    private const int Rate = 12000;

    /// <summary>Where the transmission is put, in the passband.</summary>
    private const float PlacedAtHz = 1240;

    /// <summary>**The measured offset reaches the panel that said it had none.**</summary>
    [AvaloniaFact]
    public void TheMeasuredOffsetReachesTheCardsPanel()
    {
        var folder = Path.Combine(
            Path.GetTempPath(), "hamlet-unit305w-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(folder);

        try
        {
            using var telemetry = new JsonlTelemetry(folder, "1.12.269", _ => true);

            var settings = new AppSettings { ReconnectOnStartup = false };

            settings.Operator.Callsign = "KC3QIS";

            var panel = new MainWindowViewModel(settings, telemetry)
            {
                OperatingMode = "Digital",
            };

            var raised = new List<string>();

            panel.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName is { } name)
                {
                    raised.Add(name);
                }
            };

            _output.WriteLine("BEFORE THE OFFSET ARRIVES");
            _output.WriteLine("  ClockOffset.IsKnown : " + panel.ClockOffset.IsKnown);
            _output.WriteLine("  ClockOffsetLine     : " + panel.ClockOffsetLine);
            _output.WriteLine("  DigitalCardsIdle    : " + Short(panel.DigitalCardsIdle));
            _output.WriteLine("");

            // **THE MEASUREMENT THE OPERATOR'S OWN FILE CARRIES**, set the way the
            // query sets it: this property, on the object the window is bound to.
            var measured = new ClockOffset(0.033, DateTime.UtcNow);

            panel.ClockOffset = measured;

            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            _output.WriteLine("AFTER THE OFFSET ARRIVES");
            _output.WriteLine("  ClockOffset.IsKnown : " + panel.ClockOffset.IsKnown);
            _output.WriteLine("  ClockOffsetLine     : " + panel.ClockOffsetLine);
            _output.WriteLine("  DigitalCardsIdle    : " + Short(panel.DigitalCardsIdle));
            _output.WriteLine("");

            _output.WriteLine("WHAT THE OBJECT TOLD THE SCREEN");

            foreach (var name in raised.Distinct().OrderBy(n => n, StringComparer.Ordinal))
            {
                _output.WriteLine("  raised: " + name);
            }

            telemetry.Dispose();

            var lines = Directory.GetFiles(folder, "*.jsonl")
                .SelectMany(File.ReadAllLines)
                .Where(l => l.Contains("clock_offset", StringComparison.Ordinal))
                .ToList();

            _output.WriteLine("");
            _output.WriteLine("AND IT IS IN THE RECORD");

            foreach (var line in lines)
            {
                _output.WriteLine("  " + line);
            }

            // **THE VALUE ARRIVED**, which was never in doubt.
            Assert.True(panel.ClockOffset.IsKnown);

            // **AND THE PANEL THAT SAID THE CLOCK WAS UNCHECKED IS NOW TOLD.**
            // This is the wire, asserted rather than described. Before the mend
            // this assertion failed and its opposite held.
            Assert.Contains("DigitalCardsIdle", raised);

            Assert.DoesNotContain(
                "has not been able to check the clock",
                panel.DigitalCardsIdle,
                StringComparison.Ordinal);

            // **AND THE CROSSING IS IN THE FILE**, so this class of fault is
            // findable next time rather than guessed at.
            Assert.Contains(
                lines,
                l => l.Contains("\"to\":\"known\"", StringComparison.Ordinal));
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

    /// <summary>**A slot is cut from a measured offset, and it decodes.**</summary>
    /// <remarks>
    /// **A DECODE IS THE POINT, NOT A LINE OF TEXT** (the instruction's own words).
    /// The same watch is driven twice over the same audio and the same clock: once
    /// with the offset unknown, which is the state the panel was stuck describing,
    /// and once with it measured.
    /// </remarks>
    [Fact]
    public void ASlotIsCutFromAMeasuredOffsetAndDecodes()
    {
        var sentAt = new DateTime(2026, 9, 2, 14, 22, 15, DateTimeKind.Utc);
        var from = new DateTime(2026, 9, 2, 14, 22, 10, DateTimeKind.Utc);
        var until = new DateTime(2026, 9, 2, 14, 22, 40, DateTimeKind.Utc);

        var measured = new ClockOffset(
            0.033, new DateTime(2026, 9, 2, 14, 20, 0, DateTimeKind.Utc));

        var (withNone, refusal, textWithNone) =
            RunTheBand(sentAt, from, until, ClockOffset.Unknown);

        var (withOne, _, textWithOne) =
            RunTheBand(sentAt, from, until, measured);

        _output.WriteLine("WITH NO MEASURED OFFSET");
        _output.WriteLine("  slots cut : " + withNone);
        _output.WriteLine("  refusal   : " + refusal);
        _output.WriteLine("  decoded   : " + (textWithNone.Count == 0
            ? "(nothing)" : string.Join(" | ", textWithNone)));
        _output.WriteLine("");
        _output.WriteLine("WITH THE MEASURED OFFSET");
        _output.WriteLine("  slots cut : " + withOne);
        _output.WriteLine("  decoded   : " + (textWithOne.Count == 0
            ? "(nothing)" : string.Join(" | ", textWithOne)));

        // **NO OFFSET, NO SLOT** — which is correct, and is what the record showed.
        Assert.Equal(0, withNone);
        Assert.Equal(Ft8SlotCutter.NoOffset, refusal);

        // **AND WITH ONE, SLOTS ARE CUT AND A MESSAGE COMES OUT OF THEM.**
        Assert.True(withOne > 0, "no slot was cut from a measured offset");
        Assert.Contains("CQ K1ABC FN42", textWithOne);
    }

    /// <summary>Drive the watch over one simulated transmission.</summary>
    /// <param name="sentAt">The slot the transmission goes out in.</param>
    /// <param name="from">When the looking starts.</param>
    /// <param name="until">When it stops.</param>
    /// <param name="offset">The clock the watch is given.</param>
    /// <returns>How many slots closed, the last refusal, and what decoded.</returns>
    private static (int Slots, string Refusal, List<string> Text) RunTheBand(
        DateTime sentAt, DateTime from, DateTime until, ClockOffset offset)
    {
        var origin = new DateTime(2026, 9, 2, 14, 22, 0, DateTimeKind.Utc);
        var band = new float[Rate * 120];

        var packed = new byte[Ft8StandardMessage.MessageBytes];

        Assert.Equal(
            Ft8PackResult.Ok,
            Ft8StandardMessage.TryPack("CQ", "K1ABC", "FN42", packed));

        Ft8Waveform
            .SynthesizeSlot(Ft8SymbolEncoder.Encode(packed), Rate, PlacedAtHz)
            .CopyTo(band.AsSpan(
                (int)Math.Round((sentAt - origin).TotalSeconds * Rate)));

        var tap = new AudioTap();
        var watch = new Ft8SlotWatch();
        var slots = 0;
        var refusal = "";
        var text = new List<string>();
        long delivered = (long)Math.Round((from - origin).TotalSeconds * Rate);

        for (var at = from; at <= until; at = at.AddSeconds(0.25))
        {
            var wanted = (long)Math.Round((at - origin).TotalSeconds * Rate);

            if (wanted > delivered)
            {
                tap.Take(
                    band.AsSpan((int)delivered, (int)(wanted - delivered)), Rate);

                delivered = wanted;
            }

            var look = watch.Look(tap, at, offset);

            if (look.Refusal.Length > 0)
            {
                refusal = look.Refusal;
            }

            if (look.Ready is not { } ready)
            {
                continue;
            }

            slots++;

            var heard = Ft8Reader.Read(ready.Audio, ready.EndedAtPcUtc, offset);

            text.AddRange(heard.Decodes.Select(d => d.Message));
        }

        return (slots, refusal, text);
    }

    private static string Short(string s)
        => s.Length <= 90 ? s : s[..90] + "...";
}
