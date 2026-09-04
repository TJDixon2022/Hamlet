using System.Diagnostics;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Audio;

/// <summary>
/// Work instruction 239, task 5: a captured recording replayed through the tap
/// at wall-clock pace, with a reader running at the cadence the app reads it,
/// and the arrival ratio that comes out reported.
/// </summary>
/// <remarks>
/// <para>**THIS IS THE ONE TEST IN THE UNIT THAT ASSERTS NO NUMBER, AND THAT IS
/// DELIBERATE.** The shack machine's arrival was 13% before unit 238 and 76%
/// after, and this is the development machine (`SHACK_FACTS.md` FACT-004):
/// nothing measured here is evidence about the radio, the sound card, or the
/// machine the number 76% came from. A threshold here would be a claim about
/// this machine dressed as a claim about that one, which is exactly the shape
/// §0.0 forbids.</para>
/// <para>**WHAT IT IS FOR.** It exercises the whole shape - a writer paced like
/// a device, a reader at the app's own cadence, and the arrival ratio read off
/// the tap - so the ratio is produced by the same machinery on both machines and
/// the shack figure has something to be compared against. And it fails loudly if
/// the ratio ever stops being MEASURABLE, which is the failure mode that cost an
/// evening: not a bad number, but no number at all.</para>
/// <para>**THE AUDIO IS A REAL RECORDING** (HM-DEC-091). `cw-2026-08-17-013347`
/// is the capture holding `VA3VRR`, adjudicated in HM-DEC-145. Nothing here
/// decodes it; it is used because a real recording's sample rate and length are
/// facts rather than choices.</para>
/// </remarks>
public sealed class AReplayedCaptureArrivesWholeTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the replay.</summary>
    /// <param name="output">Where the figures are printed.</param>
    public AReplayedCaptureArrivesWholeTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>The device buffer period in force, set in WasapiAudioSource.</summary>
    private const double PeriodMs = WasapiAudioSource.BufferMilliseconds;

    /// <summary>How long the replay runs.</summary>
    /// <remarks>
    /// **LONGER THAN THE FIFTEEN-SECOND WINDOW THE RATIO IS TAKEN OVER**, or the
    /// ratio would be measuring a window that is partly empty because the replay
    /// had not started rather than because audio failed to arrive.
    /// </remarks>
    private const double ReplaySeconds = 18;

    /// <summary>The arrival ratio a paced replay produces with a reader running.</summary>
    [Fact]
    public void APacedReplayWithAReaderReportsItsArrivalRatio()
    {
        var path = Path.Combine(
            Root(), "tests", "fixtures", "cw", "captured",
            "cw-2026-08-17-013347.wav");

        Assert.True(File.Exists(path), "the capture is missing: " + path);

        var capture = WavAudio.Read(path);
        var rate = capture.SampleRate;
        var chunk = new float[(int)Math.Round(rate * PeriodMs / 1000.0)];

        var tap = new AudioTap();
        var stop = false;
        var readsMade = 0L;

        // **THE READER IS THE APP'S OWN CADENCE, NOT A HAMMER.** Task 2's test
        // already proves the hammer case. What this one asks is what the ratio
        // reads while the two readers that actually run are running: the keying
        // meter's six seconds once a second, and the slot watch's fifteen
        // seconds once a slot.
        var reader = new Thread(() =>
        {
            var meterWindow = new ReusableWindow();
            var slotWindow = new ReusableWindow();
            var lastSlot = Stopwatch.GetTimestamp();

            while (!Volatile.Read(ref stop))
            {
                if (meterWindow.Tail(tap, CwKeyingThresholds.Window) is not null)
                {
                    Interlocked.Increment(ref readsMade);
                }

                var sinceSlot = (Stopwatch.GetTimestamp() - lastSlot)
                    / (double)Stopwatch.Frequency;

                if (sinceSlot >= Ft8Slots.SlotSeconds)
                {
                    var span = (int)(Ft8Slots.SlotSeconds * rate);

                    if (slotWindow.From(tap, tap.SamplesSeen - span, span) is not null)
                    {
                        Interlocked.Increment(ref readsMade);
                    }

                    lastSlot = Stopwatch.GetTimestamp();
                }

                Thread.Sleep(1_000);
            }
        })
        { IsBackground = true, Name = "app-cadence-reader" };

        reader.Start();

        // **PACED LIKE THE DEVICE, ONE BUFFER PER PERIOD.** The arrival ratio is
        // samples delivered over samples a continuous stream would have
        // delivered in the same wall clock, so a replay that ran flat out would
        // report an arrival far over 100% and mean nothing.
        var pace = (long)(Stopwatch.Frequency * PeriodMs / 1000.0);
        var next = Stopwatch.GetTimestamp();
        var until = next + (long)(Stopwatch.Frequency * ReplaySeconds);
        var from = 0;
        var chunksWritten = 0;

        while (Stopwatch.GetTimestamp() < until)
        {
            while (Stopwatch.GetTimestamp() < next)
            {
                Thread.SpinWait(50);
            }

            next += pace;

            for (var i = 0; i < chunk.Length; i++)
            {
                // The recording loops, because it is shorter than the replay and
                // what is being measured is the pacing rather than the content.
                chunk[i] = capture.Samples[(from + i) % capture.Samples.Length];
            }

            from = (from + chunk.Length) % capture.Samples.Length;

            tap.Take(chunk, rate);
            chunksWritten++;
        }

        Volatile.Write(ref stop, true);
        reader.Join(TimeSpan.FromSeconds(3));

        var recent = tap.ArrivalRatio(TimeSpan.FromSeconds(15));
        var arrival = new AudioArrival(
            recent, recent, 0, 0, 0, 0, 0, tap.SamplesSeen,
            BufferPeriodMicroseconds: PeriodMs * 1000);

        _output.WriteLine("capture      : cw-2026-08-17-013347.wav, "
            + rate + " Hz, " + capture.Samples.Length + " samples");
        _output.WriteLine("replay       : " + ReplaySeconds + " s at "
            + PeriodMs + " ms a buffer, " + chunk.Length + " samples each");
        _output.WriteLine("chunks taken : " + chunksWritten);
        _output.WriteLine("reader calls : " + Interlocked.Read(ref readsMade)
            + " at the app's own cadence");
        _output.WriteLine("torn reads   : " + tap.TornReads
            + ", abandoned " + tap.AbandonedReads);
        _output.WriteLine("");
        _output.WriteLine("ARRIVAL      : " + arrival.RecentText);
        _output.WriteLine("as the sidecar would print it:");
        _output.WriteLine("  arrival  " + arrival.RecentText);
        _output.WriteLine("  budget   " + arrival.CallbackBudgetText);
        _output.WriteLine("");
        _output.WriteLine("**NO THRESHOLD IS ASSERTED.** This is the development "
            + "machine (SHACK_FACTS.md FACT-004), and nothing here is evidence "
            + "about the radio or about the machine that read 76%.");

        Assert.True(chunksWritten > 0, "the replay never wrote a chunk");

        Assert.True(
            Interlocked.Read(ref readsMade) > 0,
            "the reader never read the tap, so this measured a writer alone");

        // **THE ONE ASSERTION: THE RATIO WAS MEASURED.** NaN is *nobody
        // measured*, and a path that cannot produce a number is the failure that
        // cost an evening - not a bad ratio, but no ratio at all (HM-DEC-093).
        Assert.False(
            double.IsNaN(recent),
            "the arrival ratio came back NaN after " + chunksWritten
            + " chunks were written at wall-clock pace, so the tap cannot say "
            + "whether audio arrived");

        // And that it reports as a percentage rather than as the word for
        // nothing measured, which is what the sidecar and the census print.
        Assert.DoesNotContain("not measured", arrival.RecentText,
            StringComparison.Ordinal);
    }

    private static string Root()
    {
        var at = new DirectoryInfo(AppContext.BaseDirectory);

        while (at is not null && !File.Exists(Path.Combine(at.FullName, "Hamlet.sln")))
        {
            at = at.Parent;
        }

        return at?.FullName
            ?? throw new InvalidOperationException("no Hamlet.sln above the test binary");
    }
}
