using Hamlet.RadioEngine.Cw;

namespace Hamlet.RadioEngine.Audio;

/// <summary>
/// How far one pitch stood above the noise beside it over one recording, and
/// nothing from before the recording began (work instruction 417, R63).
/// </summary>
/// <remarks>
/// <para>**THE HELD FIGURE'S OWN QUANTITY, OVER THIS FILE ONLY.** The decoder's
/// held peak (<see cref="CwDecodeReport.SnrDb"/>) rises at once and falls about a
/// decibel a second across a whole evening, so the number beside a capture was the
/// highest the tracked tone had stood in the last several seconds of whatever came
/// before it: measured across this repository's captures it rated two recordings
/// holding keying at no pitch above the one the decoder reads a callsign out of.
/// This measures the same thing the same way over the samples in one file: every
/// five milliseconds, a forty millisecond Hann window, the power at the pitch against
/// the median of the 25 Hz grid across the tracker's range, leaving out every grid
/// pitch within <see cref="CwCompetitor.SeparationHz"/> of the tone, counted only
/// where the middle of five readings in a row agrees, and the highest of those.</para>
/// <para>**THE NOISE BESIDE THE TONE IS BESIDE IT IN FREQUENCY, AND THAT HAS A
/// LIMIT WORTH KNOWING.** Measured on `cw-2026-08-20-014854`, the grid below 550 Hz
/// sat 25 to 44 dB under the receiver's passband, so a pitch at the passband's edge
/// is being compared with the filter's stopband. The sheet never prints this figure
/// where the decoder did not measure the pitch, which is where that was seen.</para>
/// <para>**IT LIVES IN THE ENGINE BECAUSE IT IS SIGNAL PROCESSING** (§0.1), and
/// outside `Cw` because nothing in the decoder uses it: it reads the tracker's range
/// and the competitor separation and changes neither.</para>
/// </remarks>
public static class RecordingToneOverNoise
{
    /// <summary>The window, in milliseconds.</summary>
    public const double WindowMs = 40;

    /// <summary>How many readings in a row must agree before one counts.</summary>
    public const int Agreeing = 5;

    /// <summary>The noise grid's spacing, in hertz.</summary>
    public const double GridHz = 25;

    /// <summary>The highest the pitch stood above the noise beside it, in dB.</summary>
    /// <param name="audio">The recording, and nothing else.</param>
    /// <param name="toneHz">The pitch.</param>
    /// <returns>The figure, or NaN where the recording holds too little audio.</returns>
    /// <exception cref="ArgumentNullException">No audio.</exception>
    /// <remarks>
    /// Measured at 862 ms on thirty seconds at 48 kHz, so a caller on a UI thread
    /// runs it elsewhere.
    /// </remarks>
    public static double Peak(MonoAudio audio, double toneHz)
    {
        ArgumentNullException.ThrowIfNull(audio);

        var rate = audio.SampleRate;
        var window = (int)Math.Round(rate * WindowMs / 1000);

        // The tracker's own hop, so a reading here is spaced as one there is.
        var hop = Math.Max(4, rate / 200);

        var samples = audio.Samples;
        var windows = samples.Length < window ? 0 : ((samples.Length - window) / hop) + 1;

        if (window < 2 || windows < Agreeing)
        {
            return double.NaN;
        }

        var hann = new double[window];

        for (var i = 0; i < window; i++)
        {
            hann[i] = 0.5 - (0.5 * Math.Cos(2 * Math.PI * i / (window - 1)));
        }

        var neighbors = new List<double>();

        for (var hz = CwToneTracker.MinimumToneHz; hz <= CwToneTracker.MaximumToneHz; hz += GridHz)
        {
            if (Math.Abs(hz - toneHz) >= CwCompetitor.SeparationHz)
            {
                neighbors.Add(2 * Math.Cos(2 * Math.PI * hz / rate));
            }
        }

        var toneCoefficient = 2 * Math.Cos(2 * Math.PI * toneHz / rate);
        var scratch = new double[window];
        var noise = new double[neighbors.Count];
        var readings = new double[windows];

        for (var w = 0; w < windows; w++)
        {
            var start = w * hop;

            for (var i = 0; i < window; i++)
            {
                scratch[i] = samples[start + i] * hann[i];
            }

            for (var n = 0; n < noise.Length; n++)
            {
                noise[n] = Goertzel(scratch, neighbors[n]);
            }

            Array.Sort(noise);

            readings[w] = Db(Goertzel(scratch, toneCoefficient)) - Db(noise[noise.Length / 2]);
        }

        var five = new double[Agreeing];
        var best = double.NegativeInfinity;

        for (var w = Agreeing - 1; w < windows; w++)
        {
            Array.Copy(readings, w - Agreeing + 1, five, 0, Agreeing);
            Array.Sort(five);

            best = Math.Max(best, five[Agreeing / 2]);
        }

        return best;
    }

    private static double Goertzel(double[] windowed, double coefficient)
    {
        double s1 = 0;
        double s2 = 0;

        for (var i = 0; i < windowed.Length; i++)
        {
            var s0 = windowed[i] + (coefficient * s1) - s2;

            s2 = s1;
            s1 = s0;
        }

        return (s1 * s1) + (s2 * s2) - (coefficient * s1 * s2);
    }

    private static double Db(double power) => 10 * Math.Log10(power + 1e-14);
}
