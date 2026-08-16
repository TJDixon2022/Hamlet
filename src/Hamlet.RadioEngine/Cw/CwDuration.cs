using Hamlet.RadioEngine.Training;

namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// How long a message will take to send, and how much silence means it is over
/// (HM-DEC-085).
/// </summary>
/// <remarks>
/// <para>**HANDING THE MESSAGE OVER IS NOT THE TRANSMISSION.** Command `17`
/// gives up to thirty characters to the radio's own keyer and returns as soon as
/// the bytes are accepted, about thirteen milliseconds later. The radio then keys
/// on its own for the next eighteen seconds. A latch that released when the send
/// call returned released at 13 ms and handed the panel straight back to the
/// flapping transmit line, which is why the send buttons blinked through two
/// shipped attempts to stop them.</para>
/// <para>So the end of a transmission has to be predicted before it starts, and
/// Morse timing is arithmetic rather than a mystery. The dit is the unit, and
/// everything else follows from counting elements.</para>
/// <para>**The counting is not done here.** <see cref="MorseCode"/> already
/// holds the character table, the PARIS dit and the element counter, and it
/// already knows about the radio's `^` run-together character, which a second
/// copy written for the transmit path got wrong on its first attempt. One table,
/// so the waterfall's keying rhythm, the field guide's audio and this clock
/// cannot disagree about how long a message is (§0).</para>
/// </remarks>
public static class CwDuration
{
    /// <summary>The speed assumed when the radio has not been read.</summary>
    /// <remarks>
    /// Twenty, which is what this radio was observed sending at. It sizes a
    /// progress bar and never claims a speed: the keyer speed is read over
    /// `14 0C`, and the reading wins whenever there is one (§0.0).
    /// </remarks>
    public const int DefaultWpm = 20;

    /// <summary>How long a dit lasts at a given speed.</summary>
    /// <param name="wordsPerMinute">The keyer speed.</param>
    /// <returns>The dit length.</returns>
    public static TimeSpan Dit(int wordsPerMinute)
        => MorseCode.Dit(Math.Clamp(Speed(wordsPerMinute), 1, 60));

    /// <summary>
    /// How long the transmit line must stay quiet before the message is over.
    /// </summary>
    /// <param name="wordsPerMinute">The keyer speed.</param>
    /// <returns>The hold-off.</returns>
    /// <remarks>
    /// <para>**A HOLD-OFF, NOT EDGE DETECTION** (HM-DEC-085). Under full break-in
    /// the transmit line drops between every dit and dah, so waiting for it to go
    /// low would end the transmission somewhere inside the first letter. What ends
    /// it is silence longer than any silence the message itself contains.</para>
    /// <para>The longest legitimate gap is a word space, seven dit lengths, and
    /// this is twice that with a floor of three quarters of a second. Twice,
    /// because the transmit line is not watched continuously: it is sampled with
    /// everything else about four times a second, and a hold-off only a little
    /// longer than a word gap can be cleared by three unlucky samples landing in
    /// the quiet parts of real keying. The floor is there because at forty words a
    /// minute two word spaces is a quarter of a second, which is one sample.</para>
    /// <para>The cost of being generous is that the panel stays busy for an extra
    /// half second after the last dah, and the cost of being tight is the blinking
    /// this exists to stop.</para>
    /// </remarks>
    public static TimeSpan Silence(int wordsPerMinute)
    {
        var twoWordGaps = Dit(wordsPerMinute) * 14;

        return twoWordGaps < MinimumSilence ? MinimumSilence : twoWordGaps;
    }

    /// <summary>The shortest quiet that may ever be read as finished.</summary>
    /// <remarks>
    /// Three quarters of a second, which is three samples of the transmit line at
    /// the rate the rig is polled.
    /// </remarks>
    public static readonly TimeSpan MinimumSilence = TimeSpan.FromMilliseconds(750);

    /// <summary>
    /// How long this message will take at this speed.
    /// </summary>
    /// <param name="message">Exactly what will go out.</param>
    /// <param name="wordsPerMinute">The keyer speed, or 0 when it was not read.</param>
    /// <returns>The expected duration, and zero when there is nothing to send.</returns>
    /// <remarks>
    /// A character the table does not carry contributes nothing rather than a
    /// guessed average, because the radio will not send it either.
    /// </remarks>
    public static TimeSpan Of(string? message, int wordsPerMinute)
        => string.IsNullOrWhiteSpace(message)
            ? TimeSpan.Zero
            : Dit(wordsPerMinute) * MorseCode.LengthInDits(message);

    /// <summary>The speed to use, falling back when the radio was not read.</summary>
    private static int Speed(int wordsPerMinute)
        => wordsPerMinute <= 0 ? DefaultWpm : wordsPerMinute;
}
