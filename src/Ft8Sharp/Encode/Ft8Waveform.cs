using System;

namespace Ft8Sharp.Encode;

/// <summary>
/// Turns the seventy-nine channel symbols of an FT8 transmission into the audio that transmission
/// actually is.
/// </summary>
/// <remarks>
/// <para>
/// <b>This is the last of the three things step 3 delivers</b> — LDPC encode, the symbol sequence,
/// and audio synthesis from it. It takes <see cref="Ft8SymbolEncoder"/>'s output and nothing else:
/// it does not take message text, it does not pack, it does not re-derive a tone. That boundary is
/// upstream's own and <see cref="Ft8SymbolEncoder"/> already draws it.
/// </para>
/// <para>
/// <b>It returns a buffer and that is the whole of what it does.</b> No sound device, no stream, no
/// port, no file. Nothing in this library plays, transmits or keys anything, and nothing in this
/// library ever will — the audio built here exists so that a decoder can be tested against a signal
/// whose contents are known. See <c>CLAUDE.md</c> §0.2 and the phase plan's first named boundary.
/// </para>
/// <para>
/// <b>The modulation, in the terms the published description uses.</b> FT8 sends 79 channel symbols
/// at 6.25 symbols per second — a symbol period of 0.16 s and a slot of 15 s — each symbol one of
/// eight tones spaced by the reciprocal of the symbol period, so 6.25 Hz apart. The frequency is
/// shifted between tones through a Gaussian-shaped pulse rather than stepped, and the phase runs
/// continuously across symbol boundaries, which is what makes the emission narrow. Those facts are
/// public and are in the QEX paper the NOTICE cites.
/// </para>
/// <para>
/// <b>Continuous phase is the thing a plausible port gets wrong.</b> A synthesizer that restarts
/// phase at each symbol produces a waveform of exactly the right length carrying exactly the right
/// frequencies, and it is not FT8 — the discontinuities splatter energy across the band and no
/// decoder built for the real thing will read it cleanly. Nothing about the length or the recovered
/// tones catches that, so <c>Ft8WaveformTests</c> measures the step at every symbol boundary
/// directly.
/// </para>
/// <para>
/// <b>Ported from the pin at
/// <c>9fec6ca39886edbf96f4f5e71edc76da5074e871</c>: <c>demo/gen_ft8.c</c> for the synthesis and the
/// slot layout, <c>common/wave.c</c> for the conversion to sixteen-bit samples, <c>ft8/constants.h</c>
/// for the timing.</b> The arithmetic is deliberately kept in <see langword="float"/> in the same
/// order upstream evaluates it, and the two places where upstream computes in <see langword="double"/>
/// and narrows — the peak phase step, the frequency offset and the sample conversion — do the same
/// here. That is not fussiness: <c>Ft8WaveformComparisonTests</c> holds every sample of this against
/// the WAV upstream's own generator writes for the same message, and an evaluation order that drifts
/// shows up there as a difference nobody can localise afterwards.
/// </para>
/// </remarks>
public static class Ft8Waveform
{
    /// <summary>How many channel symbols one FT8 transmission carries.</summary>
    public const int SymbolCount = Ft8SymbolEncoder.SymbolCount;

    /// <summary>How many tones the modulation has.</summary>
    public const int ToneCount = Ft8SymbolEncoder.ToneCount;

    /// <summary>The sample rate upstream's generator writes, and this library's default.</summary>
    public const int DefaultSampleRate = 12000;

    /// <summary>The audio frequency of tone 0 unless another is asked for, in hertz.</summary>
    public const float DefaultBaseFrequency = 1000.0f;

    /// <summary>How long one channel symbol lasts, in seconds.</summary>
    /// <remarks>Published: 6.25 symbols per second.</remarks>
    public const float SymbolPeriodSeconds = 0.160f;

    /// <summary>How long the transmission slot is, in seconds, signal and silence together.</summary>
    public const float SlotSeconds = 15.0f;

    /// <summary>The spacing between adjacent tones, in hertz — the reciprocal of the symbol period.</summary>
    public const float ToneSpacingHz = 1.0f / SymbolPeriodSeconds;

    /// <summary>
    /// The bandwidth-time product of the Gaussian smoothing filter.
    /// </summary>
    /// <remarks>
    /// Upstream's value for FT8, carried here because the port needs it. It is the single parameter
    /// the sample comparison is most sensitive to after the phase, which is why
    /// <c>Ft8WaveformComparisonTests</c> is watched refusing a waveform built with it moved.
    /// </remarks>
    private const float SymbolSmoothing = 2.0f;

    /// <summary>The pulse's scale factor, which is pi times the square root of two over log two.</summary>
    private const float GfskConstant = 5.336446f;

    /// <summary>How many symbol periods the truncated smoothing pulse spans.</summary>
    private const int PulseSymbolSpan = 3;

    /// <summary>The fraction of a symbol over which the ends of the signal are ramped up and down.</summary>
    private const int RampDivisor = 8;

    /// <summary>How many samples one channel symbol occupies at the given rate.</summary>
    public static int SamplesPerSymbol(int sampleRate)
    {
        RequireSampleRate(sampleRate);
        return (int)(0.5f + sampleRate * SymbolPeriodSeconds);
    }

    /// <summary>How many samples the signal itself occupies, silence excluded.</summary>
    public static int SampleCount(int sampleRate)
    {
        RequireSampleRate(sampleRate);
        return (int)(0.5f + (SymbolCount * SymbolPeriodSeconds * sampleRate));
    }

    /// <summary>
    /// How many samples of silence lead the signal inside a full slot, and how many follow it.
    /// </summary>
    /// <remarks>
    /// The signal is a little over half a second short of the slot, and the remainder is split
    /// evenly across the two ends. This is the number a comparison against upstream's own file
    /// aligns on, and it is computed from the timing rather than found by searching the file for
    /// where the signal starts — an alignment discovered by search is weaker evidence and would
    /// have to be reported as such.
    /// </remarks>
    public static int PaddingSampleCount(int sampleRate)
    {
        RequireSampleRate(sampleRate);
        return (int)((SlotSeconds * sampleRate - SampleCount(sampleRate)) / 2);
    }

    /// <summary>How many samples a whole slot occupies: silence, signal, silence.</summary>
    public static int SlotSampleCount(int sampleRate) =>
        SampleCount(sampleRate) + (2 * PaddingSampleCount(sampleRate));

    /// <summary>
    /// The signal for one message: <paramref name="symbols"/> rendered as samples in the range
    /// -1 to +1, with no leading or trailing silence.
    /// </summary>
    /// <param name="symbols">The 79 channel symbols, each in the eight-tone alphabet.</param>
    /// <param name="sampleRate">Samples per second.</param>
    /// <param name="baseFrequency">The audio frequency of tone 0, in hertz.</param>
    public static float[] Synthesize(
        ReadOnlySpan<byte> symbols,
        int sampleRate = DefaultSampleRate,
        float baseFrequency = DefaultBaseFrequency)
    {
        RequireSymbols(symbols);
        RequireSampleRate(sampleRate);
        RequireBaseFrequency(baseFrequency, sampleRate);

        var samplesPerSymbol = SamplesPerSymbol(sampleRate);
        var sampleCount = SymbolCount * samplesPerSymbol;

        // The instantaneous phase step at each sample, one symbol of margin at each end so that the
        // first and last symbols are shaped by a pulse that runs off both sides of them.
        var phaseStep = new float[sampleCount + (2 * samplesPerSymbol)];

        // Upstream computes the two scale factors in double and narrows, and so does this: the
        // comparison is sample-for-sample and an evaluation that differs here differs everywhere.
        var peakStep = (float)(2.0 * Math.PI * 1.0 / samplesPerSymbol);
        var offsetStep = (float)(2.0 * Math.PI * baseFrequency / sampleRate);
        for (var i = 0; i < phaseStep.Length; i++)
        {
            phaseStep[i] = offsetStep;
        }

        var pulse = GfskPulse(samplesPerSymbol);

        // Each symbol contributes its tone through the pulse, over the three symbol periods the
        // pulse spans — so neighbouring symbols overlap and the frequency slides rather than steps.
        for (var i = 0; i < SymbolCount; i++)
        {
            var at = i * samplesPerSymbol;
            for (var j = 0; j < PulseSymbolSpan * samplesPerSymbol; j++)
            {
                phaseStep[j + at] += peakStep * symbols[i] * pulse[j];
            }
        }

        // Dummy symbols before the first and after the last, each repeating its neighbour's tone,
        // so the ends are shaped by the same filter as the middle rather than starting abruptly.
        // Note the multiplication order differs from the loop above; upstream's does too, and float
        // multiplication is not associative, so it is kept.
        for (var j = 0; j < 2 * samplesPerSymbol; j++)
        {
            phaseStep[j] += peakStep * pulse[j + samplesPerSymbol] * symbols[0];
            phaseStep[j + (SymbolCount * samplesPerSymbol)] +=
                peakStep * pulse[j] * symbols[SymbolCount - 1];
        }

        // The waveform itself. Phase accumulates across every symbol boundary and is never reset,
        // which is the whole point of the modulation.
        var signal = new float[sampleCount];
        var phase = 0.0f;
        var twoPi = (float)(2.0 * Math.PI);
        for (var k = 0; k < sampleCount; k++)
        {
            signal[k] = MathF.Sin(phase);
            phase = (phase + phaseStep[k + samplesPerSymbol]) % twoPi;
        }

        // A raised-cosine ramp on the first and last eighth of a symbol, so the transmission does
        // not begin or end on a step.
        var ramp = samplesPerSymbol / RampDivisor;
        for (var i = 0; i < ramp; i++)
        {
            var envelope = (1 - MathF.Cos((float)(2.0 * Math.PI * i / (2 * ramp)))) / 2;
            signal[i] *= envelope;
            signal[sampleCount - 1 - i] *= envelope;
        }

        return signal;
    }

    /// <summary>
    /// A whole transmission slot: silence, the signal, silence — which is what fifteen seconds of
    /// FT8 on the air is, and what upstream's generator writes to a file.
    /// </summary>
    public static float[] SynthesizeSlot(
        ReadOnlySpan<byte> symbols,
        int sampleRate = DefaultSampleRate,
        float baseFrequency = DefaultBaseFrequency)
    {
        var signal = Synthesize(symbols, sampleRate, baseFrequency);
        var padding = PaddingSampleCount(sampleRate);
        var slot = new float[signal.Length + (2 * padding)];
        signal.CopyTo(slot.AsSpan(padding));
        return slot;
    }

    /// <summary>
    /// The same slot as sixteen-bit samples, by upstream's own clipping, scaling and rounding.
    /// </summary>
    /// <remarks>
    /// A separate entry point rather than a flag, so that a comparison against upstream's own file
    /// can be made in the units that file is written in and neither side has to be re-scaled to
    /// meet the other.
    /// </remarks>
    public static short[] SynthesizeSlotPcm16(
        ReadOnlySpan<byte> symbols,
        int sampleRate = DefaultSampleRate,
        float baseFrequency = DefaultBaseFrequency) =>
        ToPcm16(SynthesizeSlot(symbols, sampleRate, baseFrequency));

    /// <summary>
    /// Converts samples in the range -1 to +1 to sixteen-bit samples the way upstream does.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Three things happen here and all three are upstream's.</b> Values outside the range are
    /// clipped to it; the scale is the largest positive sixteen-bit value rather than the magnitude
    /// of the most negative one; and the rounding is a half added before a truncation toward zero,
    /// which is <em>not</em> symmetric about zero and is <em>not</em> what any of the framework's
    /// rounding modes does. Getting that last one wrong costs one count on roughly half the samples
    /// of the file, which is exactly the size of difference a tolerance would swallow.
    /// </para>
    /// <para>
    /// <b>The clipping never wraps.</b> The largest magnitude that can reach the truncation is
    /// 0.5 + 32767, which is inside the range of a sixteen-bit sample; the most negative is
    /// 0.5 - 32767. <c>Ft8WaveformTests</c> feeds it values outside the range in both directions
    /// and checks the ends rather than trusting this paragraph.
    /// </para>
    /// </remarks>
    public static short[] ToPcm16(ReadOnlySpan<float> samples)
    {
        var converted = new short[samples.Length];
        for (var i = 0; i < samples.Length; i++)
        {
            var x = samples[i];
            if (x > 1.0)
            {
                x = 1.0f;
            }
            else if (x < -1.0)
            {
                x = -1.0f;
            }

            converted[i] = (short)(int)(0.5 + (x * 32767.0));
        }

        return converted;
    }

    /// <summary>
    /// The Gaussian smoothing pulse, truncated to the symbol periods it meaningfully spans.
    /// </summary>
    /// <remarks>
    /// The pulse is the difference of two error functions half a symbol either side of the sample,
    /// which is the integral of a Gaussian over one symbol — so a symbol's tone is applied to the
    /// frequency gradually, and the sum of all the overlapping pulses at any instant is one tone's
    /// worth. It is theoretically infinite and is cut off at three symbol periods, where it is far
    /// below the resolution of the samples it shapes.
    /// </remarks>
    private static float[] GfskPulse(int samplesPerSymbol)
    {
        var pulse = new float[PulseSymbolSpan * samplesPerSymbol];
        for (var i = 0; i < pulse.Length; i++)
        {
            var t = (i / (float)samplesPerSymbol) - 1.5f;
            var lower = GfskConstant * SymbolSmoothing * (t + 0.5f);
            var upper = GfskConstant * SymbolSmoothing * (t - 0.5f);
            pulse[i] = (Erf(lower) - Erf(upper)) / 2;
        }

        return pulse;
    }

    /// <summary>The error function, which the framework does not provide.</summary>
    /// <remarks>
    /// <para>
    /// <b>Deliberately not a cheap approximation.</b> The pulse this shapes drives the phase, the
    /// phase accumulates across a hundred and fifty thousand samples, and an error in the pulse that
    /// leans one way integrates into a growing phase error — which would show up in the comparison
    /// against upstream's own waveform as a difference that grows with time, indistinguishable at a
    /// glance from a wrong sample rate. So this is computed in double precision and narrowed once,
    /// rather than evaluated in single throughout.
    /// </para>
    /// <para>
    /// The series is the all-positive one — erf(x) = (2x/√π)·e^(-x²)·Σ (2x²)ⁿ/(2n+1)!! — chosen over
    /// the Maclaurin series because that one alternates and loses most of its significant digits to
    /// cancellation by the time the argument reaches three, which is well inside the range the pulse
    /// asks for. Beyond six the result is one to within a part in ten to the seventeenth, which is
    /// below the resolution of a double, so it is returned as one rather than summed for.
    /// </para>
    /// </remarks>
    private static float Erf(float x)
    {
        var v = (double)x;
        if (double.IsNaN(v))
        {
            return float.NaN;
        }

        var sign = v < 0 ? -1.0 : 1.0;
        var a = Math.Abs(v);
        if (a >= 6.0)
        {
            return (float)sign;
        }

        var squared = a * a;
        var term = 1.0;
        var sum = 1.0;
        for (var n = 1; n < 512; n++)
        {
            term *= 2.0 * squared / ((2 * n) + 1);
            sum += term;
            if (term < sum * 1e-18)
            {
                break;
            }
        }

        const double TwoOverRootPi = 1.1283791670955126;
        return (float)(sign * TwoOverRootPi * a * Math.Exp(-squared) * sum);
    }

    private static void RequireSymbols(ReadOnlySpan<byte> symbols)
    {
        if (symbols.Length != SymbolCount)
        {
            throw new ArgumentException(
                $"an FT8 transmission is exactly {SymbolCount} channel symbols and this is "
                + $"{symbols.Length}. A waveform synthesized from the wrong number of them would be "
                + "the wrong length and would carry a message no decoder can frame.",
                nameof(symbols));
        }

        for (var i = 0; i < symbols.Length; i++)
        {
            if (symbols[i] >= ToneCount)
            {
                throw new ArgumentException(
                    $"symbol {i} is {symbols[i]} and the modulation has {ToneCount} tones, numbered 0 "
                    + $"to {ToneCount - 1}. A tone outside the alphabet would be synthesized as a "
                    + "frequency outside the channel rather than refused.",
                    nameof(symbols));
            }
        }
    }

    private static void RequireSampleRate(int sampleRate)
    {
        if (sampleRate <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sampleRate),
                sampleRate,
                "the sample rate must be a positive number of samples per second; a waveform cannot "
                + "be synthesized at zero or fewer.");
        }
    }

    private static void RequireBaseFrequency(float baseFrequency, int sampleRate)
    {
        if (float.IsNaN(baseFrequency) || baseFrequency <= 0.0f)
        {
            throw new ArgumentOutOfRangeException(
                nameof(baseFrequency),
                baseFrequency,
                "the base frequency is the audio frequency of tone 0 and must be above zero; a base "
                + "frequency at or below zero puts the bottom tone at or below DC, where it is not a "
                + "tone at all.");
        }

        var top = baseFrequency + ((ToneCount - 1) * ToneSpacingHz);
        var nyquist = sampleRate / 2.0f;
        if (top >= nyquist)
        {
            throw new ArgumentOutOfRangeException(
                nameof(baseFrequency),
                baseFrequency,
                $"tone {ToneCount - 1} would sit at {top} Hz, at or above the {nyquist} Hz Nyquist "
                + $"limit of a {sampleRate} Hz sample rate, where it would alias down into the "
                + "channel as a different tone instead of being synthesized.");
        }
    }
}
