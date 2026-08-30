using Hamlet.RadioEngine.Civ;

namespace Hamlet.RadioEngine.Explore;

/// <summary>
/// Why mode-follow last declined, in a sentence, for the diagnostics screen.
/// </summary>
/// <remarks>
/// <para>**THE SILENCE WAS DEFENSIBLE AND IT COST WEEKS** (work instruction 051,
/// task 5). Mode-follow declines without saying anything, and that is right for
/// the status line: the operator has not asked for anything, and a commentary on
/// writes that nearly happened is noise on the one line he reads. But it meant
/// nothing anywhere recorded that a refusal had occurred, so **"Hamlet refused",
/// "Hamlet is broken" and "nobody tuned anywhere" were the same picture** — which
/// is exactly the fault §8.1 was written against, and it is how a guard that
/// silenced twenty-eight digital blocks survived from one ruling to the next.</para>
/// <para>So the reason goes where somebody hunting for why nothing happened will
/// look. It names three things, because any one of them alone leaves the reader
/// guessing: **what the map called for, what the radio is in, and which test
/// declined.**</para>
/// </remarks>
public static class ModeFollowNote
{
    /// <summary>Describe the last mode-follow decision.</summary>
    /// <param name="target">What the map called for, or null.</param>
    /// <param name="currentMode">What the radio was in, or null where unread.</param>
    /// <param name="currentDataMode">The data flag, or null where unread.</param>
    /// <param name="because">
    /// The refusal token from <see cref="ModeFollowRefusal"/>, or "" where the
    /// decision was to write.
    /// </param>
    /// <returns>One sentence, or "" where there is nothing to say.</returns>
    /// <remarks>
    /// **UNREAD IS SAID AS UNREAD** (§0.0). A radio nobody has asked and a radio
    /// in plain CW are different facts, and this screen exists to prove what
    /// Hamlet holds rather than to tidy it.
    /// </remarks>
    public static string Describe(
        ModeTarget? target, CivMode? currentMode, bool? currentDataMode,
        string because)
    {
        if (string.IsNullOrEmpty(because))
        {
            return "";
        }

        var wanted = target is null ? "nothing" : target.Name;
        var isIn = currentMode is { } mode
            ? currentDataMode switch
            {
                true => CivValues.Name(mode) + "-D",
                false => CivValues.Name(mode),
                null => CivValues.Name(mode) + ", data flag unread",
            }
            : "a mode nobody has read";

        var declined = because switch
        {
            ModeFollowRefusal.NotArmed =>
                "the automation is off, or your own hand suspended it until the "
                + "next band change",
            ModeFollowRefusal.NoTarget =>
                "the map says nothing about this stretch of band",
            ModeFollowRefusal.WorkingMorse =>
                "you are working Morse here, and that beats what the map says "
                + "lives at this frequency",
            ModeFollowRefusal.AlreadyThere =>
                "the radio is already in it",
            ModeFollowRefusal.AlreadyWritten =>
                "Hamlet set this here already and nothing since says the radio "
                + "left it",
            _ => because,
        };

        return $"Mode-follow wrote nothing. The map called for {wanted}, the "
               + $"radio is in {isIn}, and {declined}.";
    }
}
