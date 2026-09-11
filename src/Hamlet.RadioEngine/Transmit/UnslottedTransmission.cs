namespace Hamlet.RadioEngine.Transmit;

/// <summary>Which mode a send with no slot is in.</summary>
/// <remarks>
/// **ONLY THE MODES THAT HAVE A NO-SLOT PATH.** FT8 and FT4 are slotted and are not here;
/// a member for a mode nothing can send would be a type asserting a capability the
/// application does not have (§0.0).
/// </remarks>
public enum UnslottedMode
{
    /// <summary>PSK31: a continuous carrier for exactly as long as its text takes.</summary>
    Psk31 = 0,
}

/// <summary>Whether a send with no slot may go, measured against the cap.</summary>
public enum UnslottedFit
{
    /// <summary>There is audio and it is no longer than the cap.</summary>
    Fits,

    /// <summary>There is nothing to send.</summary>
    NoAudio,

    /// <summary>
    /// **Longer than <see cref="OperatorSend.LongestUnslottedSeconds"/>.** Refused before it
    /// is armed, and again inside the sequence if it is handed there directly.
    /// </summary>
    LongerThanTheCap,
}

/// <summary>
/// **The audio of a send with no slot, and the shape of what it says - never the words.**
/// </summary>
/// <param name="Mode">Which mode it is.</param>
/// <param name="Samples">The audio, in -1 to +1.</param>
/// <param name="SampleRate">Samples a second.</param>
/// <param name="MessageLength">How many characters the text was. A count, which names nobody.</param>
/// <remarks>
/// <para>**NO FT8 MESSAGE TYPE IS INVENTED FOR IT** (work instruction 318 task 3). An
/// <see cref="Ft8Transmission"/> carries an FT8 message type, a read-back and a hashed
/// callsign flag, and none of the three means anything for a text mode; putting PSK31 audio
/// in one would make the evidence record assert an FT8 format that never went out.</para>
/// <para>**IT HOLDS NO TEXT** (HM-DEC-018). The chain records the length, the duration, the
/// frequency and the mode, and a value that never held the words cannot leak them. Whoever
/// composed it already has the text.</para>
/// </remarks>
public sealed record UnslottedTransmission(
    UnslottedMode Mode, float[] Samples, int SampleRate, int MessageLength)
{
    /// <summary>How long the audio is, from the array and the rate.</summary>
    public double Seconds => SampleRate > 0 ? Samples.Length / (double)SampleRate : 0;

    /// <summary>Whether it may go, against the cap.</summary>
    public UnslottedFit Fit => Samples.Length == 0 || SampleRate <= 0
        ? UnslottedFit.NoAudio
        : Seconds > OperatorSend.LongestUnslottedSeconds
            ? UnslottedFit.LongerThanTheCap
            : UnslottedFit.Fits;
}
