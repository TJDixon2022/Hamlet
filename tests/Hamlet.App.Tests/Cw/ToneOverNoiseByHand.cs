using Hamlet.RadioEngine.Audio;

namespace Hamlet.App.Tests.Cw;

/// <summary>
/// How far one pitch stood above the noise beside it over one recording, measured
/// the long way round (work instruction 417).
/// </summary>
/// <remarks>
/// <para>**THE SAME KIND OF QUANTITY AS THE HELD FIGURE, OVER THIS FILE ONLY.**
/// The decoder's held peak takes, every five milliseconds, the power at the tracked
/// pitch against the median of the band's 25 Hz grid from 300 to 900 Hz, leaving out
/// every grid pitch within 125 Hz of the tone, and it counts a reading only when the
/// middle of five in a row agrees. This does exactly that over the samples in one
/// file and keeps the highest, with nothing held from before the file began and
/// nothing decaying.</para>
/// <para>**WRITTEN OUT BY HAND AND SHARING NO CODE WITH THE SHEET'S OWN
/// MEASUREMENT**, so a test comparing the two is comparing two implementations of
/// one stated method rather than one implementation with itself (§12.5).</para>
/// </remarks>
internal static class ToneOverNoiseByHand
{
    /// <summary>The window, in seconds.</summary>
    public const double WindowSeconds = 0.040;

    /// <summary>How far the window moves, in seconds: the tracker's own hop.</summary>
    public const double HopSeconds = 0.005;

    /// <summary>How many readings in a row must agree before one counts.</summary>
    public const int Agreeing = 5;

    /// <summary>The highest the pitch stood over the noise beside it, in dB.</summary>
    /// <param name="audio">The recording.</param>
    /// <param name="toneHz">The pitch.</param>
    /// <returns>The figure, or NaN where the file is too short to give one.</returns>
    public static double Peak(MonoAudio audio, double toneHz)
    {
        var rate = audio.SampleRate;
        var window = (int)Math.Round(rate * WindowSeconds);
        var hop = Math.Max(4, rate / 200);

        var hann = new double[window];

        for (var i = 0; i < window; i++)
        {
            hann[i] = 0.5 - (0.5 * Math.Cos(2 * Math.PI * i / (window - 1)));
        }

        var neighbors = new List<double>();

        for (var hz = 300.0; hz <= 900; hz += 25)
        {
            if (Math.Abs(hz - toneHz) >= 125)
            {
                neighbors.Add(hz);
            }
        }

        var readings = new List<double>();

        for (var start = 0; start + window <= audio.Samples.Length; start += hop)
        {
            var tone = Power(audio.Samples, start, hann, toneHz, rate);
            var noise = neighbors
                .Select(hz => Power(audio.Samples, start, hann, hz, rate))
                .OrderBy(p => p)
                .ToList();

            readings.Add(Db(tone) - Db(noise[noise.Count / 2]));
        }

        if (readings.Count < Agreeing)
        {
            return double.NaN;
        }

        var best = double.NegativeInfinity;

        for (var i = Agreeing - 1; i < readings.Count; i++)
        {
            var five = readings.Skip(i - Agreeing + 1).Take(Agreeing).OrderBy(r => r).ToList();

            best = Math.Max(best, five[Agreeing / 2]);
        }

        return best;
    }

    /// <summary>
    /// The highest the pitch stood above its own quietest fifth over the file, in dB.
    /// </summary>
    /// <param name="audio">The recording.</param>
    /// <param name="toneHz">The pitch.</param>
    /// <returns>The figure, or NaN where the file is too short to give one.</returns>
    public static double PeakOverOwnFloor(MonoAudio audio, double toneHz)
    {
        var readings = ToneReadings(audio, toneHz);

        if (readings.Count < Agreeing)
        {
            return double.NaN;
        }

        var floor = readings.OrderBy(r => r).ElementAt(readings.Count / 5);
        var best = double.NegativeInfinity;

        for (var i = Agreeing - 1; i < readings.Count; i++)
        {
            var five = readings.Skip(i - Agreeing + 1).Take(Agreeing).OrderBy(r => r).ToList();

            best = Math.Max(best, five[Agreeing / 2]);
        }

        return best - floor;
    }

    /// <summary>
    /// The whole file's mean power at each 25 Hz pitch from 300 to 900, in dB
    /// under the loudest of them.
    /// </summary>
    /// <param name="audio">The recording.</param>
    /// <returns>Pitch and level, in order.</returns>
    public static IReadOnlyList<(double Hz, double Db)> Spectrum(MonoAudio audio)
    {
        var rate = audio.SampleRate;
        var window = (int)Math.Round(rate * WindowSeconds);
        var hann = Hann(window);
        var levels = new List<(double Hz, double Db)>();

        for (var hz = 300.0; hz <= 900; hz += 25)
        {
            double sum = 0;
            var count = 0;

            for (var start = 0; start + window <= audio.Samples.Length; start += window)
            {
                sum += Power(audio.Samples, start, hann, hz, rate);
                count++;
            }

            levels.Add((hz, Db(sum / Math.Max(1, count))));
        }

        var top = levels.Max(l => l.Db);

        return levels.Select(l => (l.Hz, l.Db - top)).ToList();
    }

    /// <summary>
    /// Where the file's audio went quiet: per second, the broadband level in dBFS.
    /// </summary>
    /// <param name="audio">The recording.</param>
    /// <returns>One level per whole second.</returns>
    public static IReadOnlyList<double> SecondsLevel(MonoAudio audio)
    {
        var levels = new List<double>();

        for (var start = 0; start + audio.SampleRate <= audio.Samples.Length; start += audio.SampleRate)
        {
            double sum = 0;

            for (var i = 0; i < audio.SampleRate; i++)
            {
                sum += audio.Samples[start + i] * (double)audio.Samples[start + i];
            }

            levels.Add(10 * Math.Log10((sum / audio.SampleRate) + 1e-14));
        }

        return levels;
    }

    /// <summary>The tone bin's readings in dB, in percentiles.</summary>
    /// <param name="audio">The recording.</param>
    /// <param name="toneHz">The pitch.</param>
    /// <returns>The 5th, 20th, 50th and 95th percentiles.</returns>
    public static (double P5, double P20, double P50, double P95) Percentiles(MonoAudio audio, double toneHz)
    {
        var sorted = ToneReadings(audio, toneHz).OrderBy(r => r).ToList();

        return (sorted[sorted.Count / 20], sorted[sorted.Count / 5], sorted[sorted.Count / 2], sorted[sorted.Count * 19 / 20]);
    }

    private static List<double> ToneReadings(MonoAudio audio, double toneHz)
    {
        var rate = audio.SampleRate;
        var window = (int)Math.Round(rate * WindowSeconds);
        var hop = Math.Max(4, rate / 200);
        var hann = Hann(window);
        var readings = new List<double>();

        for (var start = 0; start + window <= audio.Samples.Length; start += hop)
        {
            readings.Add(Db(Power(audio.Samples, start, hann, toneHz, rate)));
        }

        return readings;
    }

    private static double[] Hann(int window)
    {
        var hann = new double[window];

        for (var i = 0; i < window; i++)
        {
            hann[i] = 0.5 - (0.5 * Math.Cos(2 * Math.PI * i / (window - 1)));
        }

        return hann;
    }

    private static double Power(float[] samples, int start, double[] hann, double hz, int rate)
    {
        var coefficient = 2 * Math.Cos(2 * Math.PI * hz / rate);
        double s1 = 0;
        double s2 = 0;

        for (var i = 0; i < hann.Length; i++)
        {
            var s0 = (samples[start + i] * hann[i]) + (coefficient * s1) - s2;

            s2 = s1;
            s1 = s0;
        }

        return (s1 * s1) + (s2 * s2) - (coefficient * s1 * s2);
    }

    private static double Db(double power) => 10 * Math.Log10(power + 1e-14);
}
