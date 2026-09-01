using System.Collections.Concurrent;

namespace Ft8Sharp.Tests.Encode;

/// <summary>
/// A second synthesis of the same waveform, computed a different way from the library's, and the
/// audio counterpart of what <see cref="SymbolCheck"/> is for the tones.
/// </summary>
/// <remarks>
/// <para>
/// <b>Deliberately different arithmetic, not a paraphrase.</b> A second implementation that reaches
/// the same answer by the same route proves only that the route was walked twice. So:
/// </para>
/// <list type="bullet">
/// <item><b>The pulse comes from numerical integration</b> — Simpson's rule over the Gaussian
/// itself — where the library evaluates a series for the error function. The two share no term.</item>
/// <item><b>The phase comes from direct summation.</b> This one forms the whole phase at each
/// sample as a running total in double precision and never wraps it; the library accumulates in
/// single precision and takes a remainder every sample, which is what upstream does. Those two
/// differ in their rounding by construction, and the size of that difference is itself the
/// finding this file reports.</item>
/// <item><b>Everything is in double until the last step.</b> The library is in single throughout,
/// in upstream's own evaluation order.</item>
/// </list>
/// <para>
/// <b>It also takes the parameters the library holds fixed</b> — the smoothing factor, and whether
/// phase is restarted at each symbol. That is not generality for its own sake: the comparison
/// against upstream's WAV is required to be watched refusing a waveform built with the smoothing
/// parameter moved, and there is no way to build one without an implementation that takes it.
/// </para>
/// <para>
/// <b>This is a test fixture and it never moves into the library.</b>
/// </para>
/// </remarks>
internal static class Ft8WaveformSecondOpinion
{
    /// <summary>The smoothing factor the modulation uses, as the library holds it.</summary>
    /// <remarks>
    /// Named here rather than reached for through the library, so that the second opinion cannot be
    /// made to agree with the port by construction — the same reasoning that keeps
    /// <see cref="Ft8Oracle.ToneSequenceLength"/> out of the library.
    /// </remarks>
    public const double Smoothing = 2.0;

    /// <summary>How many symbol periods the pulse is truncated to.</summary>
    private const int PulseSpan = 3;

    private static readonly ConcurrentDictionary<(int PerSymbol, double Bt), double[]> PulseCache = new();

    /// <summary>
    /// The signal for one message, in the range -1 to +1, computed independently of the library.
    /// </summary>
    /// <param name="symbols">The 79 channel symbols.</param>
    /// <param name="sampleRate">Samples per second.</param>
    /// <param name="baseFrequency">The audio frequency of tone 0.</param>
    /// <param name="smoothing">The smoothing factor, so a wrong one can be built on purpose.</param>
    /// <param name="restartPhaseEachSymbol">
    /// Builds the waveform a port with a plausible defect would build, so that the tests which claim
    /// to catch that defect can be shown catching it.
    /// </param>
    public static float[] Synthesize(
        ReadOnlySpan<byte> symbols,
        int sampleRate = 12000,
        double baseFrequency = 1000.0,
        double smoothing = Smoothing,
        bool restartPhaseEachSymbol = false)
    {
        var perSymbol = (int)(0.5 + (sampleRate * 0.160));
        var count = symbols.Length * perSymbol;
        var pulse = Pulse(perSymbol, smoothing);

        // The frequency at every sample, as a phase step. Laid out with a symbol of margin at each
        // end, as upstream's is, but built by adding each symbol's whole contribution rather than
        // by walking the pulse.
        var step = new double[count + (2 * perSymbol)];
        var offset = 2.0 * Math.PI * baseFrequency / sampleRate;
        var peak = 2.0 * Math.PI / perSymbol;
        for (var i = 0; i < step.Length; i++)
        {
            step[i] = offset;
        }

        for (var s = 0; s < symbols.Length; s++)
        {
            for (var j = 0; j < PulseSpan * perSymbol; j++)
            {
                step[(s * perSymbol) + j] += peak * symbols[s] * pulse[j];
            }
        }

        // The dummy symbols at each end, which repeat the neighbouring tone so the ends are shaped
        // by a whole pulse rather than a truncated one.
        for (var j = 0; j < 2 * perSymbol; j++)
        {
            step[j] += peak * symbols[0] * pulse[j + perSymbol];
            step[j + (symbols.Length * perSymbol)] +=
                peak * symbols[^1] * pulse[j];
        }

        // Direct summation. The whole phase at sample k is the total of every step before it, in
        // double, never wrapped — where the library adds one step at a time in single and takes a
        // remainder each time, because upstream does.
        var signal = new float[count];
        var phase = 0.0;
        for (var k = 0; k < count; k++)
        {
            if (restartPhaseEachSymbol && k % perSymbol == 0)
            {
                phase = 0.0;
            }

            signal[k] = (float)Math.Sin(phase);
            phase += step[k + perSymbol];
        }

        // The ramp on the ends, a raised cosine over an eighth of a symbol.
        var ramp = perSymbol / 8;
        for (var i = 0; i < ramp; i++)
        {
            var envelope = (1 - Math.Cos(2.0 * Math.PI * i / (2 * ramp))) / 2;
            signal[i] = (float)(signal[i] * envelope);
            signal[count - 1 - i] = (float)(signal[count - 1 - i] * envelope);
        }

        return signal;
    }

    /// <summary>The whole slot, silence and signal, as the library's counterpart produces it.</summary>
    public static float[] SynthesizeSlot(
        ReadOnlySpan<byte> symbols,
        int sampleRate = 12000,
        double baseFrequency = 1000.0,
        double smoothing = Smoothing,
        bool restartPhaseEachSymbol = false)
    {
        var signal = Synthesize(symbols, sampleRate, baseFrequency, smoothing, restartPhaseEachSymbol);
        var total = sampleRate * 15;
        var padding = (total - signal.Length) / 2;
        var slot = new float[total];
        signal.CopyTo(slot.AsSpan(padding));
        return slot;
    }

    /// <summary>
    /// The same as sixteen-bit samples. The conversion itself is deliberately <em>not</em>
    /// independent — it is the definition of the output format rather than a step of the algorithm,
    /// and a second opinion that rounded differently would report the rounding as a disagreement
    /// about the waveform.
    /// </summary>
    public static short[] SynthesizeSlotPcm16(
        ReadOnlySpan<byte> symbols,
        int sampleRate = 12000,
        double baseFrequency = 1000.0,
        double smoothing = Smoothing,
        bool restartPhaseEachSymbol = false) =>
        Ft8Sharp.Encode.Ft8Waveform.ToPcm16(
            SynthesizeSlot(symbols, sampleRate, baseFrequency, smoothing, restartPhaseEachSymbol));

    /// <summary>
    /// The smoothing pulse, from the integral it is defined as rather than from a series.
    /// </summary>
    /// <remarks>
    /// The pulse is the difference of two error functions half a symbol apart, and an error function
    /// is the area under a Gaussian — so this integrates the Gaussian directly, by Simpson's rule
    /// over enough intervals that the rule's own error is far below the resolution of the samples it
    /// will shape. Cached per shape, because it is the same pulse for every message and the
    /// integration is the expensive part.
    /// </remarks>
    private static double[] Pulse(int perSymbol, double smoothing) =>
        PulseCache.GetOrAdd((perSymbol, smoothing), key =>
        {
            // pi * sqrt(2 / log 2), computed here rather than written down, so that the second
            // opinion does not inherit the library's rounding of it.
            var k = Math.PI * Math.Sqrt(2.0 / Math.Log(2.0));

            var pulse = new double[PulseSpan * key.PerSymbol];
            for (var i = 0; i < pulse.Length; i++)
            {
                var t = ((double)i / key.PerSymbol) - 1.5;
                var lower = k * key.Bt * (t + 0.5);
                var upper = k * key.Bt * (t - 0.5);

                // erf(a) - erf(b) is one integral of the Gaussian from b to a, so it is taken as
                // one rather than as two differences of a saturating quantity — which also keeps
                // all of the precision when both ends are far out in the tail.
                pulse[i] = GaussianIntegral(upper, lower) / 2;
            }

            return pulse;
        });

    /// <summary>
    /// The error function's difference, erf(<paramref name="to"/>) - erf(<paramref name="from"/>),
    /// by Simpson's rule over the Gaussian between them.
    /// </summary>
    private static double GaussianIntegral(double from, double to)
    {
        // Beyond this the Gaussian is below the resolution of a double and the interval contributes
        // nothing; clamping keeps the interval count bounded without changing the answer.
        const double Far = 6.0;
        var a = Math.Clamp(from, -Far, Far);
        var b = Math.Clamp(to, -Far, Far);
        if (a == b)
        {
            return 0.0;
        }

        // Enough intervals that Simpson's own fourth-order error is around a part in ten to the
        // eleventh, which is far below anything a sixteen-bit sample can show.
        var n = Math.Max(256, (int)Math.Ceiling(Math.Abs(b - a) * 256));
        if (n % 2 == 1)
        {
            n++;
        }

        var h = (b - a) / n;
        var sum = Gaussian(a) + Gaussian(b);
        for (var i = 1; i < n; i++)
        {
            sum += (i % 2 == 0 ? 2 : 4) * Gaussian(a + (i * h));
        }

        // The 2/sqrt(pi) that turns the area under the Gaussian into the error function.
        return sum * h / 3 * (2.0 / Math.Sqrt(Math.PI));
    }

    private static double Gaussian(double t) => Math.Exp(-t * t);
}
