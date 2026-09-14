namespace Hamlet.RadioEngine.Rsid;

/// <summary>**Hamlet's own RSID burst: a code in, samples out.**</summary>
/// <remarks>
/// <para>**EVERY RSID NUMBER IS THE FILE'S** (`PHASE_PLAN.md` R27). The silence before the
/// tones, the symbol count, the symbol rate - which is also the tone spacing - where tone 0 sits
/// against the center, and the tone sequence all come from <see cref="RsidCodes"/>. A code the
/// file carries no sequence for makes nothing, and no sequence is derived (the arbiter's
/// decision C).</para>
/// <para>**HAMLET'S OWN ARITHMETIC** (PSK31 plan §R5). fldigi's encoder could not be read from
/// the session that wrote this. What is checked is the result: the strongest tone in each
/// symbol of this burst is the strongest tone in each symbol of fldigi's own, in front of the
/// mode author's fixtures (`TheRsidBurstTests`).</para>
/// <para>**THE PHASE RUNS ON FROM TONE TO TONE**, so a change of tone is a change of frequency
/// and not a jump in the wave, and the first and last eighth of a symbol rise from and fall to
/// silence along half a cosine, because a burst that switched on at full amplitude would be a
/// click across the band - the same reason <c>Psk31Modulator</c> starts and ends on silence.</para>
/// <para>**IT KEYS NOTHING, READS NO CLOCK, AND CHOOSES NO LEVEL OF ITS OWN** (§0.2). The peak is
/// the caller's.</para>
/// </remarks>
public static class RsidBurst
{
    /// <summary>The rise and fall at either end is this fraction of a symbol: one eighth.</summary>
    public const int RampFractionOfASymbol = 8;

    /// <summary>The tone sequence a code is announced with, or null where the file has none.</summary>
    /// <param name="codes">The codes and tone sequences.</param>
    /// <param name="code">The mode code.</param>
    /// <returns>The tones, or null.</returns>
    public static IReadOnlyList<int>? TonesFor(RsidCodes codes, int code)
        => NameFor(codes, code) is { } name && codes.ToneSequences.TryGetValue(name, out var tones)
            ? tones
            : null;

    /// <summary>fldigi's name for a code, or null where the file does not carry the code.</summary>
    /// <param name="codes">The codes.</param>
    /// <param name="code">The mode code.</param>
    /// <returns>The name, or null.</returns>
    public static string? NameFor(RsidCodes codes, int code)
    {
        ArgumentNullException.ThrowIfNull(codes);

        foreach (var (name, value) in codes.Codes)
        {
            if (value == code)
            {
                return name;
            }
        }

        return null;
    }

    /// <summary>How long a burst is, the silence before it included.</summary>
    /// <param name="codes">The codes.</param>
    /// <returns>Seconds.</returns>
    public static double Seconds(RsidCodes codes)
    {
        ArgumentNullException.ThrowIfNull(codes);

        return (codes.SilenceSymbolsBefore + codes.Symbols) / codes.SymbolRateHz;
    }

    /// <summary>Where the first tone starts, in samples from the start of the burst.</summary>
    /// <param name="codes">The codes.</param>
    /// <param name="sampleRate">Samples a second.</param>
    /// <returns>The sample.</returns>
    public static int ToneStartSample(RsidCodes codes, int sampleRate)
    {
        ArgumentNullException.ThrowIfNull(codes);

        return (int)Math.Round(codes.SilenceSymbolsBefore * sampleRate / codes.SymbolRateHz);
    }

    /// <summary>How many samples a burst is: the silence, then the symbols over the rate.</summary>
    /// <param name="codes">The codes.</param>
    /// <param name="sampleRate">Samples a second.</param>
    /// <returns>The count.</returns>
    public static int LengthInSamples(RsidCodes codes, int sampleRate)
        => ToneStartSample(codes, sampleRate) + ToneSamples(codes, sampleRate);

    /// <summary>The burst's samples.</summary>
    /// <param name="codes">The codes and tone sequences.</param>
    /// <param name="code">The mode code.</param>
    /// <param name="centerHz">Where the burst is centered.</param>
    /// <param name="sampleRate">Samples a second.</param>
    /// <param name="peak">The largest sample value asked for, above zero and at most one.</param>
    /// <returns>Silence, then the tones, in -1 to +1.</returns>
    /// <exception cref="ArgumentNullException">There are no codes.</exception>
    /// <exception cref="ArgumentException">The file carries no tone sequence for the code.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The rate, center or peak is unusable.</exception>
    public static float[] Samples(RsidCodes codes, int code, double centerHz, int sampleRate, float peak)
    {
        ArgumentNullException.ThrowIfNull(codes);

        var tones = TonesFor(codes, code)
            ?? throw new ArgumentException(
                "the RSID file carries no tone sequence for code " + code + ", and none is derived.",
                nameof(code));

        if (sampleRate <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sampleRate));
        }

        if (!(peak > 0) || peak > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(peak));
        }

        var lowestHz = centerHz + ((codes.FirstToneOffsetSymbols + tones.Min()) * codes.SymbolRateHz);
        var highestHz = centerHz + ((codes.FirstToneOffsetSymbols + tones.Max()) * codes.SymbolRateHz);

        if (lowestHz <= 0 || highestHz >= sampleRate / 2.0)
        {
            throw new ArgumentOutOfRangeException(nameof(centerHz));
        }

        var perSymbol = sampleRate / codes.SymbolRateHz;
        var toneStart = ToneStartSample(codes, sampleRate);
        var count = ToneSamples(codes, sampleRate);
        var ramp = Math.Max(1, (int)Math.Round(perSymbol / RampFractionOfASymbol));
        var samples = new float[toneStart + count];
        var phase = 0.0;

        for (var n = 0; n < count; n++)
        {
            var symbol = Math.Min(tones.Count - 1, (int)(n / perSymbol));
            var hz = centerHz + ((codes.FirstToneOffsetSymbols + tones[symbol]) * codes.SymbolRateHz);

            var envelope = n < ramp
                ? Rise((n + 0.5) / ramp)
                : n >= count - ramp
                    ? Rise((count - n - 0.5) / ramp)
                    : 1.0;

            samples[toneStart + n] = (float)(peak * envelope * Math.Sin(phase));

            phase += 2 * Math.PI * hz / sampleRate;

            if (phase > 2 * Math.PI)
            {
                phase -= 2 * Math.PI;
            }
        }

        return samples;
    }

    private static int ToneSamples(RsidCodes codes, int sampleRate)
    {
        ArgumentNullException.ThrowIfNull(codes);

        return (int)Math.Round(codes.Symbols * sampleRate / codes.SymbolRateHz);
    }

    /// <summary>Half a cosine from 0 to 1 as its argument goes from 0 to 1.</summary>
    private static double Rise(double along) => (1 - Math.Cos(Math.PI * Math.Clamp(along, 0, 1))) / 2;
}
