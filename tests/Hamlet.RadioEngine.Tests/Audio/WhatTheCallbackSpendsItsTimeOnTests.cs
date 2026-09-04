using System.Diagnostics;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Audio;

/// <summary>
/// Work instruction 240, task 1: what the device callback actually spends its
/// time on, subscriber by subscriber.
/// </summary>
/// <remarks>
/// <para>**THE BUILD TASKS ARE WORTH NOTHING AIMED AT THE WRONG SUBSCRIBER.**
/// The shack machine reports 554 of 561 callbacks running past half the buffer
/// period with zero overruns, zero queue drops and zero callback failures. After
/// unit 238 the callback should copy 4,800 samples and enqueue - microseconds of
/// work - and it is taking fifty to eighty-six milliseconds. This measures where
/// that goes before anything is changed.</para>
/// <para>**IT ASSERTS ALMOST NOTHING ON PURPOSE.** It is an instrument, not a
/// gate. The one thing it does assert is that the harness really ran, because a
/// measurement of nothing reads exactly like a measurement of something fast.
/// </para>
/// </remarks>
public sealed class WhatTheCallbackSpendsItsTimeOnTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the measurement.</summary>
    /// <param name="output">Where the figures are printed.</param>
    public WhatTheCallbackSpendsItsTimeOnTests(ITestOutputHelper output)
        => _output = output;

    private const int Rate = 48_000;

    /// <summary>One 100 ms device buffer at 48 kHz, the period unit 239 set.</summary>
    private const int Buffer = 4_800;

    /// <summary>How many buffers each arm pushes.</summary>
    private const int Buffers = 60;

    /// <summary>Where the callback's time goes, with each subscriber attached.</summary>
    [Fact]
    public void WhereTheCallbacksTimeGoesBySubscriber()
    {
        _output.WriteLine("=== SUBSCRIBERS TO IAudioSource.SamplesReady ===");
        _output.WriteLine("");
        _output.WriteLine("Two in production, and no third:");
        _output.WriteLine("  AudioSpectrumSource.OnSamples  "
            + "src/Hamlet.RadioEngine/Audio/AudioSpectrumSource.cs:225");
        _output.WriteLine("    -> Push(chunk.Samples), synchronously, in full");
        _output.WriteLine("  CwDecoder.OnSamples            "
            + "src/Hamlet.RadioEngine/Cw/CwDecoder.cs:797");
        _output.WriteLine("    -> since unit 238, enqueues onto AudioHandoff and returns");
        _output.WriteLine("");
        _output.WriteLine("MainWindowViewModel HOLDS an AudioSpectrumSource "
            + "(line 712) but does not subscribe to the event itself.");
        _output.WriteLine("");

        // **THE RING IS FILLED FIRST, BECAUSE THE EXPENSIVE BRANCH IS THE FULL
        // ONE.** Below `_ring.Length` samples, `Push` just writes and moves on;
        // the per-sample `Array.Copy` only starts once the ring is full, which
        // on a running radio it is within the first third of a second.
        var raised = 0;
        var spectrum = Warmed(() => raised++);

        var samples = new float[Buffer];

        for (var i = 0; i < samples.Length; i++)
        {
            samples[i] = MathF.Sin(i * 0.07f) * 0.25f;
        }

        var withSpectrum = Time(() => spectrum.Push(samples));

        _output.WriteLine("=== 1.2 PER SUBSCRIBER, ONE 100 ms BUFFER (4,800 samples) ===");
        _output.WriteLine("");
        Report("AudioSpectrumSource.Push, ring full, FrameReady attached",
            withSpectrum);
        _output.WriteLine("    frames raised in total: " + raised);

        var handoff = new AudioHandoff(Rate, Buffer);
        var seen = 0;
        var index = 0L;

        var worker = new Thread(() =>
        {
            var into = new float[Buffer];

            while (handoff.Take(ref into, out var count, out _, out _))
            {
                seen += count;
            }
        })
        { IsBackground = true, Name = "handoff-drain" };

        worker.Start();

        var withHandoff = Time(() =>
        {
            handoff.Offer(index, Rate, samples);
            index += samples.Length;
        });

        handoff.Completed();
        worker.Join(TimeSpan.FromSeconds(3));

        Report("AudioHandoff.Offer, which is all CwDecoder does since unit 238",
            withHandoff);
        _output.WriteLine("    samples drained by the worker: " + seen);

        _output.WriteLine("");
        _output.WriteLine("=== 1.4 THE CALLBACK, WITH AND WITHOUT THE SPECTRUM ===");
        _output.WriteLine("");

        var both = withSpectrum.Median + withHandoff.Median;

        _output.WriteLine("  app's own subscribers together (median) : "
            + both.ToString("0") + " us");
        _output.WriteLine("  with the spectrum source detached       : "
            + withHandoff.Median.ToString("0") + " us");
        _output.WriteLine("  so the spectrum source is               : "
            + (both <= 0 ? "not measurable"
                : (withSpectrum.Median / both).ToString("P1"))
            + " of it");
        _output.WriteLine("");
        _output.WriteLine("  the buffer period is 100,000 us "
            + "(WasapiAudioSource.BufferMilliseconds, unit 239)");
        _output.WriteLine("  half of it, the count the shack machine reports "
            + "554 of 561 callbacks past, is 50,000 us");

        Assert.True(raised > 0,
            "no frame was raised, so Emit never ran and this measured the cheap "
            + "path only");

        Assert.True(withSpectrum.Worst > 0, "the spectrum arm never ran");
        Assert.True(withHandoff.Worst > 0, "the hand-off arm never ran");
    }

    /// <summary>Which of `Push`'s three parts owns the time.</summary>
    /// <remarks>
    /// **THE REPORT HAS TO SAY WHICH, NOT THAT THE METHOD IS SLOW.** Three
    /// candidates: 4,800 lock acquisitions a buffer, up to 4,800 full-ring
    /// `Array.Copy` calls a buffer, and the FFT with its per-bin decibel work.
    /// Each is timed on its own, at exactly the size `Push` runs it at.
    /// </remarks>
    [Fact]
    public void WhichOfPushsThreePartsOwnsTheTime()
    {
        var ring = new float[AudioSpectrumSource.WindowAt48K];
        var gate = new object();
        var sink = 0f;

        // **PART ONE: THE PER-SAMPLE LOCK.** Uncontended, which is the kind
        // `Push` takes on a machine where nothing else is reading, so this is
        // the floor rather than the realistic cost.
        var locks = Time(() =>
        {
            for (var i = 0; i < Buffer; i++)
            {
                lock (gate)
                {
                    sink += ring[i & (ring.Length - 1)];
                }
            }
        });

        // **PART TWO: THE PER-SAMPLE FULL-RING SHIFT.** This is the branch
        // `Push` takes once `_fill` reaches `_ring.Length`, which on a running
        // radio is within the first third of a second and stays that way.
        var shifts = Time(() =>
        {
            for (var i = 0; i < Buffer; i++)
            {
                Array.Copy(ring, 1, ring, 0, ring.Length - 1);
                ring[^1] = 0f;
            }
        });

        // **PART THREE: THE TRANSFORM.** One 16,384-point real FFT plus the
        // taper, at the hop rate: a 4,800-sample buffer crosses a 4,096-sample
        // hop once, sometimes twice.
        var fft = new RealFft(AudioSpectrumSource.WindowAt48K);
        var window = new float[AudioSpectrumSource.WindowAt48K];
        var magnitudes = new double[fft.BinCount];
        var real = new double[fft.Size];
        var imaginary = new double[fft.Size];
        var taper = new double[ring.Length];

        for (var i = 0; i < taper.Length; i++)
        {
            taper[i] = 0.5 - (0.5 * Math.Cos(2 * Math.PI * i / (taper.Length - 1)));
        }

        var transform = Time(() =>
        {
            for (var i = 0; i < ring.Length; i++)
            {
                window[i] = (float)(ring[i] * taper[i]);
            }

            fft.Magnitudes(window, magnitudes, real, imaginary);
        });

        _output.WriteLine("=== 1.3 PUSH'S THREE PARTS, PER 100 ms BUFFER ===");
        _output.WriteLine("");
        Report("4,800 uncontended lock acquisitions", locks);
        Report("4,800 full-ring Array.Copy of "
            + (AudioSpectrumSource.WindowAt48K - 1) + " floats", shifts);
        Report("one 16,384-point FFT with taper (once or twice a buffer)",
            transform);

        var total = locks.Median + shifts.Median + transform.Median;

        _output.WriteLine("");
        _output.WriteLine("  share of the median total:");
        _output.WriteLine("    lock     " + Share(locks.Median, total));
        _output.WriteLine("    shift    " + Share(shifts.Median, total));
        _output.WriteLine("    transform " + Share(transform.Median, total));
        _output.WriteLine("");
        _output.WriteLine("  the shift moves "
            + ((long)Buffer * (AudioSpectrumSource.WindowAt48K - 1) / 1_000_000)
            + " million floats per 100 ms buffer");
        _output.WriteLine("  sink (kept so the lock loop is not optimised away): "
            + sink);

        Assert.True(locks.Worst > 0 && shifts.Worst > 0 && transform.Worst > 0,
            "one of the three parts did not run");
    }

    private static string Share(double part, double total)
        => total <= 0 ? "not measurable" : (part / total).ToString("P1");

    /// <summary>A running source whose ring is already full.</summary>
    /// <remarks>
    /// **THE RING IS FILLED BECAUSE THE EXPENSIVE BRANCH IS THE FULL ONE.**
    /// Below `_ring.Length` samples `Push` writes and moves on; the per-sample
    /// `Array.Copy` only begins once the ring is full, which on a running radio
    /// happens within the first third of a second and stays that way for the
    /// evening.
    /// </remarks>
    private static AudioSpectrumSource Warmed(Action onFrame)
    {
        var source = new AudioSpectrumSource(Rate);

        source.FrameReady += (in SpectrumFrame _) => onFrame();
        source.Start();

        var filler = new float[AudioSpectrumSource.WindowAt48K];

        for (var i = 0; i < filler.Length; i++)
        {
            filler[i] = MathF.Sin(i * 0.03f) * 0.2f;
        }

        source.Push(filler);

        return source;
    }

    private (double Median, double P99, double Worst) Time(Action work)
    {
        // One untimed run, so tiered compilation is not charged to the first
        // measurement.
        work();

        var costs = new double[Buffers];

        for (var i = 0; i < costs.Length; i++)
        {
            var started = Stopwatch.GetTimestamp();
            work();
            costs[i] = (Stopwatch.GetTimestamp() - started) * 1_000_000.0
                / Stopwatch.Frequency;
        }

        Array.Sort(costs);

        return (
            costs[costs.Length / 2],
            costs[(int)(costs.Length * 0.99)],
            costs[^1]);
    }

    private void Report(string what, (double Median, double P99, double Worst) t)
    {
        _output.WriteLine("  " + what);
        _output.WriteLine("    median " + t.Median.ToString("0").PadLeft(9)
            + " us   p99 " + t.P99.ToString("0").PadLeft(9)
            + " us   worst " + t.Worst.ToString("0").PadLeft(9) + " us");
    }
}
