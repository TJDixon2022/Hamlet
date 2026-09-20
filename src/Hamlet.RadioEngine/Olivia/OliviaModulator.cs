namespace Hamlet.RadioEngine.Olivia;

/// <summary>**Hamlet's own Olivia voice: text in, audio out.**</summary>
/// <remarks>
/// <para>**THE DEMODULATOR'S INVERSE, AND EVERY FORMAT FACT IS THE FILE'S** (work instruction 365
/// decision AQ, PSK31 plan §R5). The characters per block, the mask, the upper half negated, the
/// Walsh butterfly, the scrambling code and its shift of 13, the rotation of a character's bit
/// across the tone's bits, which sign sets a bit, the Gray table, and each variant's tones,
/// spacing, symbol length and first tone all come from <see cref="OliviaFormat"/>, the file
/// <see cref="OliviaDemodulator"/> reads. Nothing of the mode author's code was taken.</para>
/// <para>**WHAT `pj_mfsk.h` WAS READ FOR, AND ONLY THAT** (decision AQ): the transmitter's
/// structure, which `format.json` does not carry. Each symbol is its tone under a raised cosine
/// two symbol periods long, the next starting one period later (`pj_mfsk.h:121`, `:223-236`); a
/// symbol's tone starts a quarter turn either way from where the last one's phase had run to, the
/// way chosen at random (`:174-187`); a block with fewer characters than it carries is filled
/// with the null character (`:1819-1826`); there is no start-up preamble - the first block's
/// first symbol begins the signal (`:1813-1834`) - and the signal ends when the last symbol's
/// shape has fallen. **Written here as arithmetic per sample, in hertz and seconds, not as the
/// author's tap buffer and cosine table.** The author's transmitter also divides each period of
/// its output by that period's own peak (`:1839-1844`); Hamlet does not, because a raised cosine
/// overlapped by half already sums to the peak asked for, and the drive level is the
/// operator's (§R4).</para>
/// <para>**IT KNOWS NOTHING ABOUT TABS, RADIOS OR SLOTS** (§0.1) **AND KEYS NOTHING** (§0.2).
/// Text, a variant, a center, a rate and a peak go in; samples come out.</para>
/// </remarks>
public static class OliviaModulator
{
    /// <summary>The seed of the quarter turns' directions, so the same text makes the same audio.</summary>
    /// <remarks>**THIS UNIT'S NUMBER.** The author chooses each direction with the C library's
    /// <c>rand()</c>; any fair choice serves a receiver that does not read phase, and a fixed seed
    /// makes a send reproducible to the sample.</remarks>
    public const int PhaseSeed = 365;

    /// <summary>The audio for this text at one variant and center.</summary>
    /// <param name="text">What to send.</param>
    /// <param name="variant">The variant, as the air names it, e.g. "8/250".</param>
    /// <param name="centerHz">Where the tones are centered.</param>
    /// <param name="sampleRate">Samples a second.</param>
    /// <param name="peak">The largest sample value asked for, above zero and at most one.</param>
    /// <returns>Samples in -1 to +1, starting and ending on silence.</returns>
    /// <exception cref="ArgumentNullException">There is no text.</exception>
    /// <exception cref="ArgumentException">The variant is not the file's, or a character cannot be sent as itself.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The rate, center or peak is unusable.</exception>
    /// <exception cref="InvalidOperationException">The format file could not be read.</exception>
    public static float[] Modulate(string text, string variant, double centerHz, int sampleRate, float peak)
    {
        var format = OliviaData.Current.Format
            ?? throw new InvalidOperationException("the Olivia format could not be read: " + OliviaData.Current.Problem);

        return Modulate(format, text, variant, centerHz, sampleRate, peak);
    }

    /// <summary>The audio for this text, from a format handed in.</summary>
    /// <param name="format">The format's facts.</param>
    /// <param name="text">What to send.</param>
    /// <param name="variant">The variant, as the air names it.</param>
    /// <param name="centerHz">Where the tones are centered.</param>
    /// <param name="sampleRate">Samples a second.</param>
    /// <param name="peak">The largest sample value asked for.</param>
    /// <returns>Samples in -1 to +1.</returns>
    public static float[] Modulate(OliviaFormat format, string text, string variant, double centerHz, int sampleRate, float peak)
    {
        ArgumentNullException.ThrowIfNull(format);
        ArgumentNullException.ThrowIfNull(text);

        var v = format.Variant(variant)
            ?? throw new ArgumentException("the Olivia format carries no variant \"" + variant + "\"", nameof(variant));

        if (sampleRate <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sampleRate));
        }

        var lowest = centerHz + v.FirstToneOffsetHz;
        var highest = lowest + ((v.Tones - 1) * v.ToneSpacingHz);

        if (lowest - v.ToneSpacingHz <= 0 || highest + v.ToneSpacingHz >= sampleRate / 2.0)
        {
            throw new ArgumentOutOfRangeException(nameof(centerHz));
        }

        if (!(peak > 0) || peak > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(peak));
        }

        return Synthesize(Tones(format, v, Characters(format, text)), v, centerHz, sampleRate, peak);
    }

    /// <summary>The name a send at this variant is announced under, as `rsid-codes.json` carries it.</summary>
    /// <param name="variant">The variant, as the air names it, e.g. "8/250".</param>
    /// <returns>The name, e.g. "OLIVIA_8_250".</returns>
    /// <remarks>**A NAME TO LOOK THE CODE UP BY, NOT A CODE** (R27), spelled the way
    /// <see cref="Rsid.RsidCodes.VariantOf"/> reads it back.</remarks>
    public static string AnnouncedAs(string variant) => "OLIVIA_" + (variant ?? "").Replace('/', '_');

    /// <summary>The audio for this text, as a send with no slot carries it: its announcement, a pause, then the text.</summary>
    /// <param name="text">What to send.</param>
    /// <param name="variant">The variant, as the air names it.</param>
    /// <param name="centerHz">Where the tones, and the burst, are centered.</param>
    /// <param name="sampleRate">Samples a second - the transmit endpoint's.</param>
    /// <param name="peak">The drive level the operator set.</param>
    /// <param name="kind">Which of PSK31's two caps this send is held to.</param>
    /// <returns>The samples, the rate, the mode, the text's length and the code announced - and not the text.</returns>
    /// <exception cref="InvalidOperationException">The format or the RSID codes could not be read, or carry no burst for the variant.</exception>
    /// <remarks>
    /// <para>**EVERY OLIVIA SEND BEGINS WITH ITS VARIANT'S RSID BURST** (`PHASE_PLAN.md` R27), from
    /// <see cref="Rsid.RsidBurst"/>, centered on the send's own center at the same drive level.
    /// <see cref="Transmit.UnslottedTransmission.AnnouncedCode"/> and
    /// <see cref="Transmit.UnslottedTransmission.AnnouncementSamples"/> are the burst actually placed,
    /// so the cap measures what follows it (R32 (a)) and <c>Fit</c> excuses that burst and no more.</para>
    /// <para>**THEN THE BURST'S OWN SILENCE AGAIN, BEFORE THE FIRST SYMBOL** (work instruction 365
    /// decision AS). The author's fixtures, made the way fldigi sends RSID, carry five RSID symbols
    /// of silence after the tones as before them, and `Unit365Trace` measured 0.467 s from the last
    /// tone to the first symbol on all three. The file states the silence before
    /// (<see cref="Rsid.RsidCodes.SilenceSymbolsBefore"/>) and Hamlet's burst carries it; the same
    /// count after is this unit's reading of the fixtures. It is outside the announcement, so the
    /// cap counts it.</para>
    /// <para>**THE CAP IS THIS VARIANT'S, IN CHARACTERS** (decision AV, criterion 4.4):
    /// <see cref="OliviaTiming.CapSeconds"/> - PSK31's thirty or sixty seconds counted in PSK31
    /// characters, charged at this variant's seconds per character. <c>OperatorSend</c>'s thirty
    /// seconds does not move and still bounds every PSK31 send.</para>
    /// <para>**NO BURST, NO SEND** (§0.0). An Olivia signal names its variant only by its burst, and
    /// a receiver that must guess the variant reads nothing, so where the codes carry no burst for
    /// the variant nothing is composed. This unit's choice, overrulable; PSK31's send, whose mode a
    /// receiver needs no announcement to read, goes unannounced instead.</para>
    /// </remarks>
    public static Transmit.UnslottedTransmission Compose(
        string text,
        string variant,
        double centerHz,
        int sampleRate,
        float peak,
        OliviaSendKind kind = OliviaSendKind.Macro)
    {
        var said = Modulate(text, variant, centerHz, sampleRate, peak);

        // **THE CAP IS THE TIMING TABLE'S COUNT AT THIS VARIANT'S RATE** (`PHASE_PLAN.md` §3.2,
        // criterion 4.4, decision AV). Where the table could not be read there is no cap of this
        // variant's own, and the send falls back to the thirty seconds every unslotted send has
        // always been held to rather than to a guess.
        var longestSeconds = OliviaData.Current.Timing?.CapSeconds(variant, kind) is { } cap && !double.IsNaN(cap)
            ? cap
            : Transmit.OperatorSend.LongestUnslottedSeconds;

        var codes = OliviaData.Current.Rsid
            ?? throw new InvalidOperationException("the RSID codes could not be read, so the send cannot be announced: " + OliviaData.Current.Problem);

        if (codes.CodeOf(AnnouncedAs(variant)) is not { } code || Rsid.RsidBurst.TonesFor(codes, code) is null)
        {
            throw new InvalidOperationException("the RSID codes carry no burst for Olivia " + variant + ", so the send cannot be announced");
        }

        var burst = Rsid.RsidBurst.Samples(codes, code, centerHz, sampleRate, peak);
        var pause = Rsid.RsidBurst.ToneStartSample(codes, sampleRate);
        var samples = new float[burst.Length + pause + said.Length];

        burst.CopyTo(samples, 0);
        said.CopyTo(samples, burst.Length + pause);

        return new(Transmit.UnslottedMode.Olivia, samples, sampleRate, text.Length, longestSeconds)
        {
            AnnouncedCode = code,
            AnnouncementSamples = burst.Length,
        };
    }

    /// <summary>**How long this many characters take on the air at this variant, before the burst.**</summary>
    /// <param name="variant">The variant, as the air names it.</param>
    /// <param name="characters">How many characters the text is.</param>
    /// <returns>The seconds, or NaN where the format carries no such variant.</returns>
    /// <remarks>
    /// <para>**THE SAME ARITHMETIC <c>Modulate</c> USES, WITHOUT MAKING THE AUDIO** (work
    /// instruction 366 task 4, decision BD). <see cref="SymbolsFor"/> rounds the text up to whole
    /// blocks, the last symbol's raised cosine runs a period past the last symbol, and the length
    /// is that count of periods. A card that says what a line would cost is asked on every
    /// keystroke, so it asks this and not the modulator.</para>
    /// <para>**IT IS NOT THE TIMING TABLE'S SLOPE.** `timing.json`'s seconds per character is the
    /// per-character rate measured across a long text and is what the caps are counted in; this is
    /// what a particular text actually takes, which is longer because the air sends whole blocks.
    /// **The difference is why 121 characters at 8/250 takes 84.46 s against an 82.60 s cap**, and
    /// saying the slope figure on a card would be telling the operator a length he will not get.</para>
    /// <para>**NEVER A FIGURE IN SECONDS** (`PHASE_PLAN.md` §3.2): every number in it is the
    /// variant's own.</para>
    /// </remarks>
    public static double TextSeconds(string variant, int characters)
    {
        if (OliviaData.Current.Format is not { } format || format.Variant(variant) is not { } v)
        {
            return double.NaN;
        }

        return (SymbolsFor(format, v, characters) + 1) * v.SymbolSeconds;
    }

    /// <summary>How many symbols this text takes at this variant: whole blocks.</summary>
    /// <param name="format">The format's facts.</param>
    /// <param name="variant">The variant.</param>
    /// <param name="characters">How many characters.</param>
    /// <returns>The symbol count.</returns>
    public static int SymbolsFor(OliviaFormat format, OliviaVariant variant, int characters)
    {
        ArgumentNullException.ThrowIfNull(format);
        ArgumentNullException.ThrowIfNull(variant);

        var blocks = (Math.Max(0, characters) + variant.BitsPerSymbol - 1) / variant.BitsPerSymbol;

        return blocks * format.SymbolsPerBlock;
    }

    /// <summary>The text as characters the format carries, each checked.</summary>
    private static int[] Characters(OliviaFormat format, string text)
    {
        var characters = new int[text.Length];

        for (var i = 0; i < text.Length; i++)
        {
            int c = text[i];

            // **A CHARACTER THAT WOULD ARRIVE AS ANOTHER IS REFUSED, NOT SENT** (§0.0). The mask
            // would fold anything above it onto something else, and the null character is idle.
            if (c > format.CharacterMask || c == format.NullCharacter)
            {
                throw new ArgumentException(
                    $"the text holds U+{c:X4}, which Olivia cannot send as itself", nameof(text));
            }

            characters[i] = c;
        }

        return characters;
    }

    /// <summary>The tone of every symbol: each block encoded as the demodulator decodes it, backwards.</summary>
    private static int[] Tones(OliviaFormat format, OliviaVariant variant, int[] characters)
    {
        var perBlock = variant.BitsPerSymbol;
        var length = format.SymbolsPerBlock;
        var walsh = WalshFunctions(format);
        var tones = new int[SymbolsFor(format, variant, characters.Length)];
        var symbol = new int[length];

        for (var block = 0; block * length < tones.Length; block++)
        {
            Array.Clear(symbol);

            for (var c = 0; c < perBlock; c++)
            {
                var at = (block * perBlock) + c;
                var character = (at < characters.Length ? characters[at] : format.NullCharacter) & format.CharacterMask;

                // The character picks a Walsh function; the upper half is the lower one negated.
                var index = character;
                var sign = 1;

                if (character >= length)
                {
                    index = character - length;
                    sign = format.UpperHalfNegated ? -1 : 1;
                }

                for (var t = 0; t < length; t++)
                {
                    var value = sign * walsh[index, t];

                    // Scrambled: the code's bit for this character and symbol negates the value.
                    var codeBit = ((c * format.ScramblingShiftPerCharacter) + t) & (length - 1);

                    if (((format.ScramblingCode >> codeBit) & 1) == 1)
                    {
                        value = -value;
                    }

                    // Interleaved: the character's bit moves across the tone's bits symbol by symbol.
                    var bit = (c + (t * format.ToneBitRotationPerSymbol)) % perBlock;

                    if (format.NegativeSetsTheBit ? value < 0 : value > 0)
                    {
                        symbol[t] |= 1 << bit;
                    }
                }
            }

            for (var t = 0; t < length; t++)
            {
                tones[(block * length) + t] = format.SymbolToTone[symbol[t]];
            }
        }

        return tones;
    }

    /// <summary>Every Walsh function: the file's inverse butterfly applied to each index alone.</summary>
    private static int[,] WalshFunctions(OliviaFormat format)
    {
        var length = format.SymbolsPerBlock;
        var k = format.WalshInverseKernel;
        var table = new int[length, length];
        var column = new int[length];

        for (var index = 0; index < length; index++)
        {
            Array.Clear(column);
            column[index] = 1;

            for (var step = length / 2; step >= 1; step /= 2)
            {
                for (var at = 0; at < length; at += 2 * step)
                {
                    for (var i = at; i < at + step; i++)
                    {
                        var lower = column[i];
                        var upper = column[i + step];

                        column[i] = (k[0, 0] * lower) + (k[0, 1] * upper);
                        column[i + step] = (k[1, 0] * lower) + (k[1, 1] * upper);
                    }
                }
            }

            for (var t = 0; t < length; t++)
            {
                table[index, t] = column[t];
            }
        }

        return table;
    }

    /// <summary>The samples: each symbol's tone under a raised cosine two periods long, one period apart.</summary>
    private static float[] Synthesize(int[] tones, OliviaVariant variant, double centerHz, int sampleRate, float peak)
    {
        if (tones.Length == 0)
        {
            return [];
        }

        var period = variant.SymbolSeconds;
        var hz = new double[tones.Length];
        var phase = new double[tones.Length];
        var directions = new Random(PhaseSeed);

        for (var s = 0; s < tones.Length; s++)
        {
            hz[s] = centerHz + variant.FirstToneOffsetHz + (tones[s] * variant.ToneSpacingHz);

            // Where the last symbol's tone had run to by this symbol's start, a quarter turn either way.
            phase[s] = s == 0
                ? 0
                : phase[s - 1] + (2 * Math.PI * hz[s - 1] * period) + (directions.Next(2) == 0 ? Math.PI / 2 : -Math.PI / 2);
            phase[s] %= 2 * Math.PI;
        }

        // **THE LAST SYMBOL'S SHAPE IS TWO PERIODS LONG**, so the audio is one period longer than
        // the symbols, and it ends on silence as it began.
        var count = (int)Math.Round((tones.Length + 1) * period * sampleRate);
        var samples = new float[count];

        for (var n = 0; n < count; n++)
        {
            var t = n / (double)sampleRate;
            var newest = Math.Min(tones.Length - 1, (int)Math.Floor(t / period));
            var sum = 0.0;

            for (var s = Math.Max(0, newest - 1); s <= newest; s++)
            {
                var into = t - (s * period);

                if (into < 0 || into >= 2 * period)
                {
                    continue;
                }

                // Two raised cosines half overlapped sum to one, so no sample passes the peak.
                var shape = 0.5 * (1 - Math.Cos(Math.PI * into / period));

                sum += shape * Math.Cos((2 * Math.PI * hz[s] * into) + phase[s]);
            }

            samples[n] = (float)(peak * sum);
        }

        return samples;
    }
}
