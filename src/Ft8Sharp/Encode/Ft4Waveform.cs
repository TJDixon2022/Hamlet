using System;

namespace Ft8Sharp.Encode;

/// <summary>
/// Turns the hundred and five channel symbols of an FT4 transmission into the audio that
/// transmission actually is.
/// </summary>
/// <remarks>
/// <para>
/// <b>It returns a buffer and that is the whole of what it does.</b> No sound device, no stream, no
/// port, no file. Nothing in this library plays, transmits or keys anything. <c>CLAUDE.md</c> §0.2.
/// </para>
/// <para>
/// <b>Ported from the same pin and the same function as FT8's</b> — <c>demo/gen_ft8.c</c>'s
/// <c>synth_gfsk</c> for the synthesis and its <c>main</c> for the slot layout — with the four
/// parameters upstream hands that function differing: 105 symbols rather than 79, a symbol period of
/// <c>0.048f</c> rather than <c>0.160f</c>, a slot of 7.5 s rather than 15, and
/// <b>a smoothing bandwidth of 1.0 rather than 2.0</b> (<c>FT4_SYMBOL_BT</c> at
/// <c>demo/gen_ft8.c:17</c>, used at <c>:151</c>). The tone spacing is not a separate constant on
/// either side: it falls out of <c>dphi_peak</c>, which is one cycle per symbol period, so four
/// tones sit 20.833 Hz apart.
/// </para>
/// <para>
/// <b>Beside <see cref="Ft8Waveform"/> and not inside it.</b> That type's constants are FT8's and
/// its padding is aligned to upstream's own WAV, sample for sample, over fifty-one messages.
/// Parameterising it would put every FT4 change one edit away from that evidence. The arithmetic is
/// duplicated deliberately and the one thing that is <em>not</em> duplicated is the error function,
/// which is reached through <see cref="Ft8Waveform.ErrorFunction"/> — a second copy of a
/// numerically delicate series that a sample comparison depends on is the more expensive mistake.
/// </para>
/// <para>
/// <b>WHERE THE SIGNAL SITS IN THE SLOT, AND IT IS THE THING THAT BITES.</b> The padding here is
/// upstream's: the slot less the signal, split evenly across the two ends, which at 12 kHz is
/// <b>14760 samples — 1.23 seconds — of silence in front</b>. That is a deliberate choice rather
/// than an inheritance, and unit 289 task 1 measured what it costs: upstream's own candidate search
/// sweeps block offsets -10 to +19, which at a 0.048 s block is -0.48 s to +0.912 s, so
/// <b>a centred FT4 signal starts a third of a second past the last place upstream's own decoder
/// looks</b> — which is why upstream's decoder reads zero messages out of upstream's generator.
/// </para>
/// <para>
/// <b>The placement is kept and the sweep is widened instead.</b> A waveform that is not upstream's
/// waveform is not a faithful port, and the sweep bound is not a protocol constant: it is a
/// file-scope judgement in <c>demo/decode_ft8.c</c> about how much work to do, and it is already a
/// constructor parameter in this port. <c>Ft4SyncSearch</c> carries the widened default and states
/// the reasoning at the point of use.
/// </para>
/// <para>
/// <b>Every timing constant comes from <see cref="Ft4Timing"/>.</b> There is no <c>0.048f</c> and no
/// <c>7.5f</c> in this file, so a ruling on the 4.48-against-5.04 question is one edit and not a
/// hunt through five files.
/// </para>
/// </remarks>
public static class Ft4Waveform
{
    /// <summary>How many channel symbols one FT4 transmission carries.</summary>
    public const int SymbolCount = Ft4SymbolEncoder.SymbolCount;

    /// <summary>How many tones the modulation has.</summary>
    public const int ToneCount = Ft4SymbolEncoder.ToneCount;

    /// <summary>The sample rate upstream's generator writes, and this library's default.</summary>
    public const int DefaultSampleRate = 12000;

    /// <summary>The audio frequency of tone 0 unless another is asked for, in hertz.</summary>
    public const float DefaultBaseFrequency = 1000.0f;

    /// <summary>How long one channel symbol lasts, in seconds. From the one place it lives.</summary>
    public const float SymbolPeriodSeconds = Ft4Timing.SymbolPeriodSeconds;

    /// <summary>How long the transmission slot is, in seconds, signal and silence together.</summary>
    public const float SlotSeconds = Ft4Timing.SlotSeconds;

    /// <summary>The spacing between adjacent tones, in hertz.</summary>
    public const float ToneSpacingHz = Ft4Timing.ToneSpacingHz;

    /// <summary>
    /// The bandwidth-time product of the Gaussian smoothing filter. <b>Upstream's FT4 value, which
    /// is half FT8's</b> — <c>FT4_SYMBOL_BT</c> is 1.0 where <c>FT8_SYMBOL_BT</c> is 2.0.
    /// </summary>
    /// <remarks>
    /// A lower bandwidth-time product means a wider pulse in time and a narrower emission in
    /// frequency. It is the single parameter the sample comparison is most sensitive to after the
    /// phase, and getting it from FT8's file rather than from FT4's would produce a waveform of
    /// exactly the right length carrying exactly the right tones that no other station's decoder
    /// reads as well as it should.
    /// </remarks>
    private const float SymbolSmoothing = 1.0f;

    /// <summary>The pulse's scale factor, which is pi times the square root of two over log two.</summary>
    private const float GfskConstant = 5.336446f;

    /// <summary>How many symbol periods the truncated smoothing pulse spans.</summary>
    private const int PulseSymbolSpan = 3;

    /// <summary>The fraction of a symbol over which the ends of the signal are ramped up and down.</summary>
    private const int RampDivisor = 8;

    /// <summary>How many samples one channel symbol occupies at the given rate. 576 at 12 kHz.</summary>
    public static int SamplesPerSymbol(int sampleRate)
    {
        RequireSampleRate(sampleRate);
        return (int)(0.5f + (sampleRate * SymbolPeriodSeconds));
    }

    /// <summary>How many samples the signal itself occupies, silence excluded. 60480 at 12 kHz.</summary>
    /// <remarks>
    /// Upstream reaches this number twice by two different routes — once from the whole
    /// transmission's duration, which is what sizes the slot, and once as the symbol count times the
    /// samples per symbol, which is what the synthesis writes. At 12 kHz the two agree; the rates
    /// where they do not are refused rather than written past. <see cref="RequireConsistentGeometry"/>.
    /// </remarks>
    public static int SampleCount(int sampleRate)
    {
        RequireSampleRate(sampleRate);
        return (int)(0.5f + (SymbolCount * SymbolPeriodSeconds * sampleRate));
    }

    /// <summary>
    /// How many samples of silence lead the signal inside a full slot, and how many follow it.
    /// <b>14760 at 12 kHz, which is 1.23 seconds.</b>
    /// </summary>
    /// <remarks>
    /// Upstream's own expression from <c>demo/gen_ft8.c:175</c>: the slot less the signal, halved.
    /// Kept deliberately — see this type's remarks on where the signal sits.
    /// </remarks>
    public static int PaddingSampleCount(int sampleRate)
    {
        RequireSampleRate(sampleRate);
        return (int)(((SlotSeconds * sampleRate) - SampleCount(sampleRate)) / 2);
    }

    /// <summary>How many samples a whole slot occupies: silence, signal, silence.</summary>
    public static int SlotSampleCount(int sampleRate) =>
        SampleCount(sampleRate) + (2 * PaddingSampleCount(sampleRate));

    /// <summary>
    /// The signal for one message: <paramref name="symbols"/> rendered as samples in the range -1 to
    /// +1, with no leading or trailing silence.
    /// </summary>
    /// <param name="symbols">The 105 channel symbols, each in the four-tone alphabet.</param>
    /// <param name="sampleRate">Samples per second.</param>
    /// <param name="baseFrequency">The audio frequency of tone 0, in hertz.</param>
    public static float[] Synthesize(
        ReadOnlySpan<byte> symbols,
        int sampleRate = DefaultSampleRate,
        float baseFrequency = DefaultBaseFrequency)
    {
        RequireSymbols(symbols);
        RequireSampleRate(sampleRate);
        RequireConsistentGeometry(sampleRate);
        RequireBaseFrequency(baseFrequency, sampleRate);

        var samplesPerSymbol = SamplesPerSymbol(sampleRate);
        var sampleCount = SymbolCount * samplesPerSymbol;

        // The instantaneous phase step at each sample, one symbol of margin at each end so that the
        // first and last symbols are shaped by a pulse that runs off both sides of them.
        var phaseStep = new float[sampleCount + (2 * samplesPerSymbol)];

        // Upstream computes the two scale factors in double and narrows, and so does this.
        var peakStep = (float)(2.0 * Math.PI * 1.0 / samplesPerSymbol);
        var offsetStep = (float)(2.0 * Math.PI * baseFrequency / sampleRate);
        for (var i = 0; i < phaseStep.Length; i++)
        {
            phaseStep[i] = offsetStep;
        }

        var pulse = GfskPulse(samplesPerSymbol);

        for (var i = 0; i < SymbolCount; i++)
        {
            var at = i * samplesPerSymbol;
            for (var j = 0; j < PulseSymbolSpan * samplesPerSymbol; j++)
            {
                phaseStep[j + at] += peakStep * symbols[i] * pulse[j];
            }
        }

        // Dummy symbols before the first and after the last, each repeating its neighbour's tone.
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

        // A raised-cosine ramp on the first and last eighth of a symbol, so the transmission does not
        // begin or end on a step. FT4 also carries two ramp SYMBOLS, at positions 0 and 104, and the
        // two are different things: those are tones carrying no payload, this is the envelope.
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
    /// A whole transmission slot: silence, the signal, silence — which is what seven and a half
    /// seconds of FT4 on the air is, and what upstream's generator writes to a file.
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

    /// <summary>The same slot as sixteen-bit samples, by upstream's own clipping, scaling and rounding.</summary>
    public static short[] SynthesizeSlotPcm16(
        ReadOnlySpan<byte> symbols,
        int sampleRate = DefaultSampleRate,
        float baseFrequency = DefaultBaseFrequency) =>
        Ft8Waveform.ToPcm16(SynthesizeSlot(symbols, sampleRate, baseFrequency));

    /// <summary>The Gaussian smoothing pulse, truncated to the symbol periods it meaningfully spans.</summary>
    private static float[] GfskPulse(int samplesPerSymbol)
    {
        var pulse = new float[PulseSymbolSpan * samplesPerSymbol];
        for (var i = 0; i < pulse.Length; i++)
        {
            var t = (i / (float)samplesPerSymbol) - 1.5f;
            var lower = GfskConstant * SymbolSmoothing * (t + 0.5f);
            var upper = GfskConstant * SymbolSmoothing * (t - 0.5f);
            pulse[i] = (Ft8Waveform.ErrorFunction(lower) - Ft8Waveform.ErrorFunction(upper)) / 2;
        }

        return pulse;
    }

    private static void RequireSymbols(ReadOnlySpan<byte> symbols)
    {
        if (symbols.Length != SymbolCount)
        {
            throw new ArgumentException(
                $"an FT4 transmission is exactly {SymbolCount} channel symbols and this is "
                + $"{symbols.Length}. A waveform synthesized from the wrong number of them would be "
                + "the wrong length and would carry a message no decoder can frame.",
                nameof(symbols));
        }

        for (var i = 0; i < symbols.Length; i++)
        {
            if (symbols[i] >= ToneCount)
            {
                throw new ArgumentException(
                    $"symbol {i} is {symbols[i]} and FT4 has {ToneCount} tones, numbered 0 to "
                    + $"{ToneCount - 1}. A tone outside the alphabet would be synthesized as a "
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

    /// <summary>Refuses a sample rate at which the signal's two lengths disagree.</summary>
    /// <remarks>
    /// The same divergence <see cref="Ft8Waveform"/> records: upstream sizes its slot from one of
    /// these numbers and writes the other, which is exactly right at the rate it uses and would run
    /// off the end of its own buffer where they differ.
    /// </remarks>
    private static void RequireConsistentGeometry(int sampleRate)
    {
        var fromDuration = SampleCount(sampleRate);
        var fromSymbols = SymbolCount * SamplesPerSymbol(sampleRate);
        if (fromDuration != fromSymbols)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sampleRate),
                sampleRate,
                $"at {sampleRate} samples per second an FT4 signal is {fromDuration} samples long "
                + $"measured from the transmission's duration and {fromSymbols} measured as "
                + $"{SymbolCount} symbols of {SamplesPerSymbol(sampleRate)} samples. The slot is laid "
                + "out from the first and the waveform is written from the second, so a rate where "
                + "they disagree puts every sample after the signal starts at the wrong offset. Use "
                + $"a rate at which a symbol is a whole number of samples — {DefaultSampleRate} is "
                + "the one FT4 is written and decoded at.");
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
