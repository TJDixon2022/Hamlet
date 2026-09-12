namespace Hamlet.RadioEngine.Psk31;

/// <summary>
/// **Hamlet's own PSK31 voice: text in, audio out.**
/// </summary>
/// <remarks>
/// <para>**IT KNOWS NOTHING ABOUT TABS, RADIOS OR SLOTS** (§0.1). Text, a sample rate, an
/// offset in hertz and a peak go in; samples in -1 to +1 come out. Where they are played,
/// when, and whether anything keys is the transmit chain's business, and this type keys
/// nothing and reads no clock.</para>
/// <para>**THE CONVENTION IS THE ONE HAMLET'S EAR ALREADY READS**, taken from
/// <see cref="Psk31Demodulator"/> rather than from memory: BPSK at
/// <see cref="Psk31Demodulator.Baud"/>, **differential, a `0` bit a phase reversal and a `1`
/// bit no change**, each character its <see cref="Varicode"/> code followed by `00`, and
/// idle a run of `0`s. The fixtures that ear was proved against were made to the same
/// convention by `assets/reference-modem.py`, whose shaping is a raised-cosine pulse two
/// symbols wide centred on each symbol. **This is Hamlet's own arithmetic for that shape,
/// not a port of it** (§R5): per sample and in closed form rather than by convolution.
/// The pinned `fldigi` clone could not be read from this session and nothing was taken
/// from it.</para>
/// <para>**THE SHAPE, IN ONE SENTENCE.** The amplitude holds each symbol's sign at its
/// centre and crosses between neighbouring centres along half a cosine, so across a
/// boundary with no change it stays flat at the peak, and across a reversal it passes
/// through zero exactly on the boundary - which is the raised-cosine envelope that keeps the
/// signal about 31 Hz wide rather than splattering every time the phase flips.</para>
/// <para>**IT STARTS AND ENDS ON SILENCE.** The first half symbol rises from nothing and the
/// last half symbol falls to nothing along the same half cosine, because a carrier that
/// switched on at full amplitude would be a click across the band.</para>
/// <para>**THE DRIVE LEVEL IS THE CALLER'S** (§R4). No sample is larger than the peak handed
/// in, and this type chooses no level of its own.</para>
/// </remarks>
public static class Psk31Modulator
{
    /// <summary>**Idle bits before the text: 32, which is 1.02 s.**</summary>
    /// <remarks>
    /// <para>**LOCK AND SQUELCH.** A receiver has to find the symbol clock and then decide it
    /// is hearing keying rather than noise before the first character may be shown. Hamlet's
    /// own ear decides that over <see cref="Psk31Demodulator.QualityWindow"/> symbols, so
    /// the idle is exactly one of those windows of clean reversals: the squelch has measured
    /// a whole window of this signal before the first code arrives.</para>
    /// <para>**MEASURED, NOT ASSUMED.** Walked down one bit at a time on work instruction
    /// 318's bench, the shortest idle before which every macro still read back identically
    /// was **23 bits at both 48 000 and 8 000 Hz**. Thirty-two is nine bits, 0.29 s, over
    /// that. It is shorter than the fixtures' forty because every bit of idle is carrier on
    /// the air at full duty (§3.3).</para>
    /// </remarks>
    public const int IdleBitsBefore = Psk31Demodulator.QualityWindow;

    /// <summary>**Idle bits after the text: 16, which is 0.51 s.**</summary>
    /// <remarks>
    /// <para>**FLUSHING THE LAST CHARACTER.** A character is only read when the `00` after it
    /// is, and every code already carries that `00` (<see cref="Varicode.Encode"/>), so
    /// Hamlet's own ear needed **no idle after at all** - measured at 0 bits at both rates.
    /// The sixteen are margin for a receiver that is not Hamlet, whose clock or filter runs
    /// later than this one's and whose flush nobody here has measured. **That is this unit's
    /// choice and not a measurement**, and it is half of the idle before so the carrier does
    /// not run on longer than it has to.</para>
    /// </remarks>
    public const int IdleBitsAfter = 16;

    /// <summary>The bits that go on the air for this text, idle included.</summary>
    /// <param name="text">What to send.</param>
    /// <returns>A run of `0` and `1`, one symbol each after the reference symbol.</returns>
    public static string BitsFor(string text) => BitsFor(text, IdleBitsBefore, IdleBitsAfter);

    /// <summary>**How long this text takes on the air, idle and reference symbol included.**</summary>
    /// <param name="text">What would be sent.</param>
    /// <returns>Seconds of carrier.</returns>
    /// <exception cref="ArgumentNullException">There is no text.</exception>
    /// <remarks>
    /// <para>**COUNTED OFF THE BITS THAT WOULD ACTUALLY GO OUT** (§0.0). It encodes
    /// the text through <see cref="BitsFor(string)"/> and divides by the baud rate,
    /// so it is the same number the modulator would produce and cannot drift from
    /// it. Varicode is a variable-length code - `e` is two bits and `Q` is eleven -
    /// so a character count would not answer this question at all.</para>
    /// <para>**PLUS ONE SYMBOL FOR THE REFERENCE.** A differential mode needs
    /// something to differ from, so the wave carries one more symbol than there
    /// are bits, exactly as <see cref="Modulate(string, int, double, float)"/>
    /// builds it.</para>
    /// <para>**IT KEYS NOTHING AND COMPOSES NOTHING** (§0.2). It is arithmetic over
    /// a string.</para>
    /// </remarks>
    public static double SecondsFor(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        return (BitsFor(text).Length + 1) / Psk31Demodulator.Baud;
    }

    /// <summary>The audio for this text.</summary>
    /// <param name="text">What to send. A character with no varicode is skipped by the code.</param>
    /// <param name="sampleRate">Samples a second.</param>
    /// <param name="offsetHz">Where the carrier sits in the passband.</param>
    /// <param name="peak">The largest sample value asked for, above zero and at most one.</param>
    /// <returns>Samples in -1 to +1, starting and ending on silence.</returns>
    /// <exception cref="ArgumentNullException">There is no text.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The rate, offset or peak is unusable.</exception>
    public static float[] Modulate(string text, int sampleRate, double offsetHz, float peak)
        => Modulate(text, sampleRate, offsetHz, peak, IdleBitsBefore, IdleBitsAfter);

    /// <summary>The audio for this text, as a send with no slot carries it.</summary>
    /// <param name="text">What to send.</param>
    /// <param name="sampleRate">Samples a second - the transmit endpoint's.</param>
    /// <param name="offsetHz">Where the carrier sits in the passband.</param>
    /// <param name="peak">The drive level the operator set.</param>
    /// <returns>The samples, the rate, the mode and the text's length - and not the text.</returns>
    public static Transmit.UnslottedTransmission Compose(
        string text, int sampleRate, double offsetHz, float peak)
        => new(
            Transmit.UnslottedMode.Psk31,
            Modulate(text, sampleRate, offsetHz, peak),
            sampleRate,
            text.Length);

    /// <summary>The audio for this text, with the idle stated.</summary>
    /// <param name="text">What to send.</param>
    /// <param name="sampleRate">Samples a second.</param>
    /// <param name="offsetHz">Where the carrier sits in the passband.</param>
    /// <param name="peak">The largest sample value asked for.</param>
    /// <param name="idleBefore">Idle bits before the text.</param>
    /// <param name="idleAfter">Idle bits after the text.</param>
    /// <returns>Samples in -1 to +1.</returns>
    /// <remarks>
    /// **INTERNAL, FOR THE MEASUREMENT THAT CHOSE THE TWO CONSTANTS.** Nothing on the send
    /// path chooses its own idle.
    /// </remarks>
    internal static float[] Modulate(
        string text, int sampleRate, double offsetHz, float peak, int idleBefore, int idleAfter)
    {
        ArgumentNullException.ThrowIfNull(text);

        if (sampleRate <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sampleRate));
        }

        if (offsetHz <= 0 || offsetHz >= sampleRate / 2.0)
        {
            throw new ArgumentOutOfRangeException(nameof(offsetHz));
        }

        if (!(peak > 0) || peak > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(peak));
        }

        if (idleBefore < 0 || idleAfter < 0)
        {
            throw new ArgumentOutOfRangeException(idleBefore < 0 ? nameof(idleBefore) : nameof(idleAfter));
        }

        var bits = BitsFor(text, idleBefore, idleAfter);

        // SYMBOL 0 IS THE REFERENCE THE FIRST BIT IS KEYED AGAINST. A differential mode
        // needs something to differ from, so there is one more symbol than there are bits.
        var symbols = new int[bits.Length + 1];
        symbols[0] = 1;

        for (var i = 0; i < bits.Length; i++)
        {
            symbols[i + 1] = bits[i] == '0' ? -symbols[i] : symbols[i];
        }

        // DERIVED, NOT TYPED, AND NOT ROUNDED PER SYMBOL. At 44 100 Hz a symbol is 1411.2
        // samples; positions are taken off the exact figure so the rate stays 31.25 baud
        // over a whole macro rather than drifting a fifth of a sample every symbol.
        var perSymbol = sampleRate / Psk31Demodulator.Baud;
        var count = (int)Math.Round(symbols.Length * perSymbol);
        var step = 2 * Math.PI * offsetHz / sampleRate;
        var samples = new float[count];

        for (var n = 0; n < count; n++)
        {
            var position = n / perSymbol;
            var symbol = Math.Min(symbols.Length - 1, (int)position);

            samples[n] = (float)(peak * Amplitude(symbols, symbol, position - symbol) * Math.Sin(step * n));
        }

        return samples;
    }

    /// <summary>Idle, the text's codes each followed by `00`, then idle.</summary>
    private static string BitsFor(string text, int before, int after)
        => new string('0', before) + Varicode.Encode(text) + new string('0', after);

    /// <summary>The signed amplitude at a point inside one symbol.</summary>
    /// <param name="symbols">Every symbol's sign.</param>
    /// <param name="symbol">Which symbol the point is in.</param>
    /// <param name="into">How far into it, 0 at its start and 1 at its end.</param>
    /// <returns>A value from -1 to +1.</returns>
    /// <remarks>
    /// The first half of a symbol is the second half of the crossing from the one before;
    /// the second half is the first half of the crossing to the one after. The first and
    /// last symbols cross from and to silence instead, over their outer half.
    /// </remarks>
    private static double Amplitude(int[] symbols, int symbol, double into)
    {
        var last = symbols.Length - 1;

        if (into < 0.5)
        {
            return symbol == 0
                ? symbols[0] * Rise(2 * into)
                : Cross(symbols[symbol - 1], symbols[symbol], into + 0.5);
        }

        return symbol == last
            ? symbols[last] * Rise(2 * (1 - into))
            : Cross(symbols[symbol], symbols[symbol + 1], into - 0.5);
    }

    /// <summary>From one sign to the next along half a cosine; zero halfway on a reversal.</summary>
    private static double Cross(int from, int to, double along) => from + ((to - from) * Rise(along));

    /// <summary>Half a cosine from 0 to 1 as its argument goes from 0 to 1.</summary>
    private static double Rise(double along) => (1 - Math.Cos(Math.PI * Math.Clamp(along, 0, 1))) / 2;
}
