namespace Hamlet.RadioEngine.Audio;

/// <summary>
/// What Windows is doing to the audio before Hamlet ever sees it (HM-DEC-088).
/// </summary>
/// <param name="Name">The device this is about.</param>
/// <param name="Gain">
/// The capture level Windows is applying, from zero to one, or null when it
/// could not be read.
/// </param>
/// <param name="Muted">True when the device is muted, or null when unread.</param>
public readonly record struct CaptureHealth(string Name, double? Gain, bool? Muted)
{
    /// <summary>Below this the operating system is holding the signal down.</summary>
    /// <remarks>
    /// Half. Windows sets a capture level per device and remembers it, and a
    /// device left at a fifth is a fifth of the signal before anything in this
    /// application gets a look at it.
    /// </remarks>
    public const double LowGain = 0.5;

    /// <summary>Nothing read.</summary>
    public static CaptureHealth Unknown { get; } = new("", null, null);

    /// <summary>True when the gain is low enough to be worth saying.</summary>
    public bool GainIsLow => Gain is { } gain && gain < LowGain;

    /// <summary>True when something is definitely wrong here.</summary>
    public bool IsAProblem => Muted == true || GainIsLow;
}

/// <summary>
/// What can be said about the Windows side of the audio path (HM-DEC-088).
/// </summary>
/// <remarks>
/// <para>**THREE THINGS SIT BETWEEN THE RADIO AND THE DECODER AND THE OPERATOR
/// CAN SEE NONE OF THEM.** The radio's own USB output level, the capture level
/// Windows keeps per device, and the "audio enhancements" Windows applies to
/// capture devices by default. Turning up the AF knob moves none of them,
/// because that knob feeds the speaker and the speaker is a different path
/// entirely.</para>
/// <para>**HAMLET READS WHAT IT CAN AND NAMES WHAT IT CANNOT** (§0.0). The
/// capture level and the mute state are readable and are reported as
/// measurements. The enhancements are not reliably readable from a normal
/// application, so they are described and located rather than diagnosed, and the
/// wording never claims to have looked. An unread setting reported as "off"
/// would be worse than not mentioning it.</para>
/// </remarks>
public static class CaptureAdvice
{
    /// <summary>
    /// What Windows is doing, in the app's voice.
    /// </summary>
    /// <param name="health">What was read.</param>
    /// <returns>One passage, or "" when there is nothing to say.</returns>
    public static string Describe(CaptureHealth health)
    {
        if (health.Muted == true)
        {
            return "Windows has this input muted, so nothing is reaching Hamlet at "
                + "all no matter what the radio is doing.";
        }

        if (health.GainIsLow)
        {
            var percent = (int)Math.Round((health.Gain ?? 0) * 100);

            return $"Windows is holding this input at {percent} percent, which is "
                + "applied before Hamlet sees anything. That level is separate "
                + "from the radio's own and separate again from the volume in "
                + "your headphones.";
        }

        return "";
    }

    /// <summary>
    /// The standing note about enhancements, which Hamlet cannot read.
    /// </summary>
    /// <remarks>
    /// **NAMED, NOT DIAGNOSED.** This says what the setting does to Morse and
    /// where it lives, and it does not say whether it is on, because Hamlet has
    /// not looked and cannot. Saying "your enhancements are on" from a guess
    /// would be the prime directive broken about somebody's operating system
    /// instead of about a signal.
    /// </remarks>
    public const string EnhancementsNote =
        "Windows applies what it calls audio enhancements to microphone inputs, "
        + "and noise suppression and automatic gain are usually among them. They "
        + "are tuned for voice calls, where a steady tone is something to be "
        + "removed, and a Morse note is a steady tone switching on and off. "
        + "Hamlet cannot read whether they are switched on for this device. They "
        + "live under Sound settings, in the recording device's own properties.";
}
