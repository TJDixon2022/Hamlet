using Hamlet.RadioEngine.Audio;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The key-down run lengths in a recording, measured outside the decoder.
/// </summary>
/// <param name="RunsMs">Every key-down run, in milliseconds, in order.</param>
/// <param name="MedianMs">The middle one.</param>
/// <param name="SwingDb">
/// How far the envelope moves between its tenth and ninetieth percentile.
/// </param>
public readonly record struct KeyingProfile(
    IReadOnlyList<double> RunsMs, double MedianMs, double SwingDb);

/// <summary>
/// A second opinion about what is in a recording, built from nothing the decoder
/// owns (HM-DEC-091, §12.5).
/// </summary>
/// <remarks>
/// <para>**THIS EXISTS BECAUSE A DIAGNOSIS WAS DRAWN FROM COUNTERS THAT WERE
/// COUNTING THE WRONG THING.** Two recordings read as nothing, a session
/// concluded the speed tracker could not lock on a sloppy fist, and the
/// measurement that settled it was taken outside this repository and could not be
/// reproduced inside it. An analysis nobody can re-run is an argument, which is
/// the same thing §0.0.1 says about a decode with no audio behind it.</para>
/// <para>**IT SHARES NO CODE WITH THE DECODER ON PURPOSE.** A measurement taken
/// with the instrument under test cannot referee it, and this project has twice
/// certified a fault with a fixture built from the same misunderstanding as the
/// code (§12.5). Quadrature mixdown, a one-pole smoother, a threshold from the
/// envelope's own percentiles: no Goertzel bank, no gate, no tracker.</para>
/// <para>Fifteen hundred key-downs at a six millisecond median is a threshold
/// being crossed by noise. Two hundred at forty-eight is somebody sending.</para>
/// </remarks>
public static class KeyingEnvelope
{
    /// <summary>How often the envelope is read, in milliseconds.</summary>
    public const double StepMs = 1;

    /// <summary>Where the smoother rolls off, in hertz.</summary>
    public const double SmoothingHz = 100;

    /// <summary>Measure the keying in a recording at one pitch.</summary>
    /// <param name="audio">The recording.</param>
    /// <param name="toneHz">The pitch to listen at.</param>
    /// <returns>The profile.</returns>
    /// <exception cref="ArgumentNullException">No audio.</exception>
    public static KeyingProfile Measure(MonoAudio audio, double toneHz)
    {
        ArgumentNullException.ThrowIfNull(audio);

        var envelope = Envelope(audio, toneHz);

        if (envelope.Count == 0)
        {
            return new KeyingProfile(Array.Empty<double>(), 0, 0);
        }

        var sorted = envelope.OrderBy(v => v).ToArray();
        var low = sorted[(int)(sorted.Length * 0.10)];
        var high = sorted[(int)(sorted.Length * 0.90)];

        // **MIDWAY IN AMPLITUDE, WHICH IS NOT MIDWAY IN DECIBELS.** Halfway
        // between a quiet tenth and a loud tenth on a linear scale sits about six
        // decibels under the loud one; halfway on a logarithmic scale sits at
        // their geometric mean, which on this recording lands close enough to the
        // noise to be crossed seventeen hundred times by nothing at all. The
        // first is also where this project already takes an element's edge
        // (HM-DEC-105, HM-DEC-119), for the same reason.
        var threshold = (low + high) / 2;

        var runs = new List<double>();
        var run = 0;

        foreach (var value in envelope)
        {
            if (value >= threshold)
            {
                run++;
                continue;
            }

            if (run > 0)
            {
                runs.Add(run * StepMs);
            }

            run = 0;
        }

        if (run > 0)
        {
            runs.Add(run * StepMs);
        }

        var median = runs.Count == 0
            ? 0
            : runs.OrderBy(v => v).ElementAt(runs.Count / 2);

        return new KeyingProfile(
            runs, median, Decibels(high) - Decibels(low));
    }

    private static double Decibels(double magnitude)
        => 20 * Math.Log10(Math.Max(magnitude, 1e-12));

    private static List<double> Envelope(MonoAudio audio, double toneHz)
    {
        var rate = audio.SampleRate;
        var step = Math.Max(1, (int)Math.Round(rate * StepMs / 1000.0));

        // **A HUNDRED HERTZ OF SMOOTHING IS A TEN MILLISECOND WINDOW**, and a
        // boxcar of that length over the quadrature arms is exactly a Goertzel of
        // that bandwidth. A one-pole of the same nominal corner was tried first
        // and left nine hundred crossings under ten milliseconds on this
        // recording, which is the smoother's own ripple counted as keying: the
        // shape of a filter is part of what it measures (HM-DEC-119).
        var window = Math.Max(1, (int)Math.Round(rate / SmoothingHz));
        var omega = 2 * Math.PI * toneHz / rate;

        var cosine = new double[audio.Samples.Length];
        var sine = new double[audio.Samples.Length];

        for (var i = 0; i < audio.Samples.Length; i++)
        {
            var sample = audio.Samples[i];
            var angle = omega * i;

            cosine[i] = sample * Math.Cos(angle);
            sine[i] = sample * -Math.Sin(angle);
        }

        var envelope = new List<double>((audio.Samples.Length / step) + 1);

        double inPhase = 0;
        double quadrature = 0;

        for (var i = 0; i < audio.Samples.Length; i++)
        {
            inPhase += cosine[i];
            quadrature += sine[i];

            if (i >= window)
            {
                inPhase -= cosine[i - window];
                quadrature -= sine[i - window];
            }

            if (i % step != 0)
            {
                continue;
            }

            envelope.Add(
                Math.Sqrt((inPhase * inPhase) + (quadrature * quadrature)) / window);
        }

        return envelope;
    }
}
