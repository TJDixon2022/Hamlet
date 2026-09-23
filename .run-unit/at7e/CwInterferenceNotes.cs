namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// One thing the operator could do about something sitting in the passband.
/// </summary>
/// <param name="Name">What it is called on the radio.</param>
/// <param name="Explanation">What it does and what it costs, in the app's voice.</param>
/// <param name="Command">
/// The CI-V command that would do it, cited so nobody has to recall it.
/// </param>
/// <remarks>
/// **THIS DESCRIBES A FIX AND DOES NOT PERFORM ONE.** Every one of these is a
/// receive-side setting Hamlet could write under HM-DEC-084, and none of them is
/// written from here. What the decoder has measured is that something is in the
/// passband; whether it is worth moving the dial or reaching for a notch is the
/// operator's call, and he is sitting in front of the radio.
/// </remarks>
public readonly record struct InterferenceFix(
    string Name, string Explanation, string Command);

/// <summary>
/// Says what is sitting in the passband, and what could be done about it
/// (HM-DEC-096, phase 5).
/// </summary>
/// <remarks>
/// <para>**A STEADY SIGNAL INSIDE THE FILTER IS AN OPERATIONAL PROBLEM RATHER
/// THAN A CURIOSITY.** The receiver's automatic gain control follows the loudest
/// thing it can hear, so a carrier in the passband sets the gain for everything
/// else in there and quietly holds the station being read down with it. On a fast
/// AGC setting it does that afresh several times a second, which is why a weak
/// signal can be perfectly readable one moment and gone the next while nothing
/// about the weak signal has changed at all.</para>
/// <para>**WHAT IS SAID IS WHAT WAS MEASURED** (§0.0). A frequency and a
/// strength are facts. Whose carrier it is, what equipment is making it, and
/// whether taking it out will make the station readable are all things Hamlet has
/// no way to know, and the operator has a receiver and years of listening. So the
/// copy names the measurement, explains why it matters, and stops.</para>
/// </remarks>
public static class CwInterferenceNotes
{
    /// <summary>
    /// What is in the passband, in the app's voice.
    /// </summary>
    /// <param name="interference">What the survey measured, or null.</param>
    /// <param name="agc">The AGC setting as the rig reported it, or "".</param>
    /// <returns>One passage, or "" when there is nothing to say.</returns>
    public static string Describe(ToneInterference? interference, string agc = "")
    {
        if (interference is not { } found)
        {
            return "";
        }

        var pitch = (int)Math.Round(found.ToneHz / 5) * 5;
        var lift = (int)Math.Round(found.LiftDb);
        var share = (int)Math.Round(found.PresentFraction * 100);

        var passage =
            $"There is something at about {pitch} hertz sitting {lift} decibels "
            + "over the rest of the band, and it is not being keyed, so it is not "
            + $"somebody sending. It was there for about {share} percent of the "
            + "time Hamlet has been listening. That matters more than it sounds "
            + "like it should, because the receiver sets its gain from the loudest "
            + "thing inside the filter, so anything steady in there is quietly "
            + "holding down every weaker signal beside it.";

        if (agc.Contains("FAST", StringComparison.OrdinalIgnoreCase))
        {
            passage +=
                " The gain control is on its fast setting at the moment, which "
                + "makes it chase that carrier several times a second and takes "
                + "the station you are trying to read up and down with it.";
        }

        return passage;
    }

    /// <summary>
    /// The short version, for a panel summary.
    /// </summary>
    /// <param name="interference">What the survey measured, or null.</param>
    /// <returns>A short line, or "".</returns>
    public static string Summarize(ToneInterference? interference)
        => interference is { } found
            ? $"a steady signal at {(int)Math.Round(found.ToneHz / 5) * 5} Hz, "
              + $"{(int)Math.Round(found.LiftDb)} dB over the band"
            : "";

    /// <summary>
    /// What the operator could do about it, with the commands cited.
    /// </summary>
    /// <param name="interference">What was measured, or null.</param>
    /// <returns>The options, or an empty list.</returns>
    /// <remarks>
    /// <para>**AUTOMATIC NOTCH IS DELIBERATELY NOT OFFERED.** It hunts for
    /// whatever is steadiest in the passband and a slow fist looks steady to it,
    /// so on a quiet CW signal it eats the Morse along with the carrier. The
    /// manual notch is pointed where the operator points it and stays there.
    /// Auto notch is `16 41` and this is the one place worth naming it in order
    /// to say why it is the wrong tool (Full Manual p. 19-3).</para>
    /// </remarks>
    public static IReadOnlyList<InterferenceFix> Fixes(ToneInterference? interference)
    {
        if (interference is not { } found)
        {
            return Array.Empty<InterferenceFix>();
        }

        var pitch = (int)Math.Round(found.ToneHz / 5) * 5;

        return new[]
        {
            new InterferenceFix(
                "Move the dial",
                $"Shifting a couple of hundred hertz puts the {pitch} hertz "
                + "carrier outside the filter altogether, and the station you are "
                + "reading moves by the same amount so you follow it with the "
                + "pitch control. It costs nothing and it is the one fix that "
                + "cannot go wrong.",
                "no command; the operator turns the dial"),

            new InterferenceFix(
                "Manual notch",
                "A narrow notch aimed at the carrier takes it out and leaves the "
                + "rest of the passband alone. It wants pointing by hand, which is "
                + "the whole reason to prefer it: the automatic notch hunts for "
                + "whatever is steadiest in there, and on a slow fist that is the "
                + "Morse.",
                "16 48 on or off, 14 0D for where it sits "
                + "(00 00 fully counter-clockwise, 01 28 center, 02 55 fully "
                + "clockwise), Full Manual p. 19-3"),

            new InterferenceFix(
                "Twin passband tuning",
                "Sliding the passband moves the carrier out of the edge of the "
                + "filter while keeping the station inside it, which is worth "
                + "reaching for when the two are close enough that a notch would "
                + "catch both.",
                "14 07 inner and 14 08 outer, both 00 00 to 02 55, "
                + "Full Manual p. 19-3"),
        };
    }
}
