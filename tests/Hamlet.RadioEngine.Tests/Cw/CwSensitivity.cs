using Hamlet.RadioEngine.Training;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>What the decoder managed at one signal-to-noise ratio.</summary>
/// <param name="SnrDb">The ratio the audio was generated at.</param>
/// <param name="Correct">Share of the sent characters that came back right.</param>
/// <param name="Wrong">Share that came back as a different character.</param>
/// <param name="Emitted">How many characters were produced at all.</param>
internal readonly record struct SensitivityPoint(
    double SnrDb, double Correct, double Wrong, int Emitted);

/// <summary>
/// How far down into the noise the decoder can still read (HM-DEC-088).
/// </summary>
/// <remarks>
/// <para>**THE MEASUREMENT EXISTS BEFORE THE CHANGE DOES.** The operator can
/// copy signals by ear that produce nothing on screen, and every explanation
/// offered for that is a hypothesis until something puts a number on it. "It
/// seems better" is not a result (§0).</para>
/// <para>**THE RATIO IS DEFINED HERE AND NOWHERE ELSE**, because a sensitivity
/// figure without its definition is not comparable to anything. It is the tone's
/// RMS amplitude over the noise's RMS amplitude, across the whole audio
/// bandwidth of the fixture, in decibels. The synthesizer's noise is Gaussian
/// with a standard deviation equal to its amplitude parameter, so the noise RMS
/// is that parameter, and a tone of peak amplitude A has an RMS of A over root
/// two.</para>
/// <para>That is a **wideband** ratio and it is deliberately not corrected to a
/// reference bandwidth. Correcting it would make the number look like a
/// published receiver sensitivity, which it is not, and the thing being measured
/// is whether one build reads further into the noise than another. A number that
/// is honest about being relative is worth more than one that borrows authority
/// it has not earned (§0.0).</para>
/// </remarks>
internal static class CwSensitivity
{
    /// <summary>The message used for every sweep.</summary>
    /// <remarks>
    /// A real call rather than a pangram. It is what the decoder is for, it
    /// contains both a long dah run and a single dit, and it is short enough that
    /// a sweep of forty levels across four seeds finishes in seconds.
    /// </remarks>
    public const string Message = "CQ DE W1AW K";

    /// <summary>The speed used for every sweep.</summary>
    public const int WordsPerMinute = 18;

    /// <summary>The tone the sweep is generated at.</summary>
    /// <remarks>
    /// Deliberately not the decoder's own starting pitch. A station is rarely
    /// tuned to exactly the pitch the operator set, and a benchmark that put the
    /// signal exactly where the decoder was already looking would flatter it.
    /// </remarks>
    public const double ToneHz = 640;

    /// <summary>Peak amplitude of the tone in every sweep.</summary>
    private const double Amplitude = 0.5;

    /// <summary>How many different noise draws each level is tried with.</summary>
    /// <remarks>
    /// Four. One draw of noise decides a marginal decode as much as the decoder
    /// does, and a threshold measured from a single seed moves a couple of
    /// decibels between runs for no reason at all.
    /// </remarks>
    public const int Seeds = 4;

    /// <summary>The noise amplitude that produces a given ratio.</summary>
    /// <param name="snrDb">The wanted ratio.</param>
    /// <returns>The standard deviation to generate noise at.</returns>
    public static double NoiseFor(double snrDb)
        => Amplitude / Math.Sqrt(2) / Math.Pow(10, snrDb / 20);

    /// <summary>Run one level, averaged over the seeds.</summary>
    /// <param name="snrDb">The ratio to generate at.</param>
    /// <returns>What the decoder managed.</returns>
    public static SensitivityPoint At(double snrDb)
    {
        var correct = 0.0;
        var wrong = 0.0;
        var emitted = 0;
        var expected = CwAlignment.SymbolCount(Message);

        for (var seed = 1; seed <= Seeds; seed++)
        {
            var result = CwDecodeHarness.Decode(
                new CwSignalRequest(
                    Message,
                    WordsPerMinute: WordsPerMinute,
                    ToneHz: ToneHz,
                    Amplitude: Amplitude,
                    NoiseAmplitude: NoiseFor(snrDb),
                    Seed: seed * 7919));

            var matches = CwAlignment.Align(result.Characters, Message);

            correct += (double)matches.Count(
                m => m.Kind == CwMatchKind.Correct && !m.Decoded.IsWordGap) / expected;

            wrong += (double)matches.Count(
                m => m.Kind == CwMatchKind.Wrong && !m.Decoded.IsWordGap) / expected;
            emitted += result.Letters.Count;
        }

        return new SensitivityPoint(
            snrDb, correct / Seeds, wrong / Seeds, emitted / Seeds);
    }

    /// <summary>
    /// Sweep from easy to impossible and report every level.
    /// </summary>
    /// <param name="fromDb">Where to start, which should be comfortable.</param>
    /// <param name="toDb">Where to stop, which should be hopeless.</param>
    /// <param name="stepDb">How big a step.</param>
    /// <returns>Every level, in order.</returns>
    public static IReadOnlyList<SensitivityPoint> Sweep(
        double fromDb = 18, double toDb = -12, double stepDb = 1)
    {
        var points = new List<SensitivityPoint>();

        for (var snr = fromDb; snr >= toDb - 1e-9; snr -= stepDb)
        {
            points.Add(At(snr));
        }

        return points;
    }

    /// <summary>
    /// The lowest ratio at which the decoder still reads most of the message.
    /// </summary>
    /// <param name="points">A sweep.</param>
    /// <param name="share">How much of it has to come back right.</param>
    /// <returns>The ratio, or null when it never managed it.</returns>
    /// <remarks>
    /// The **lowest** level that clears the bar, walking down from the top and
    /// stopping at the first level that fails after one has passed. Taking the
    /// global minimum instead would let a single lucky draw far down in the noise
    /// set the figure, and a threshold that a rerun does not reproduce is not a
    /// threshold.
    /// </remarks>
    public static double? Threshold(
        IReadOnlyList<SensitivityPoint> points, double share = 0.8)
    {
        double? best = null;

        foreach (var point in points)
        {
            if (point.Correct >= share)
            {
                best = point.SnrDb;
            }
            else if (best is not null)
            {
                break;
            }
        }

        return best;
    }

    /// <summary>A sweep as a table, for the record.</summary>
    /// <param name="points">The sweep.</param>
    /// <returns>One line per level.</returns>
    public static string Report(IReadOnlyList<SensitivityPoint> points)
        => string.Join(
            Environment.NewLine,
            points.Select(p =>
                $"{p.SnrDb,6:0.0} dB  right {p.Correct,5:0.00}  "
                + $"wrong {p.Wrong,5:0.00}  emitted {p.Emitted,3}"));
}
