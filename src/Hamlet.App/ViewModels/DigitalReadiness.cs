using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Explore;

namespace Hamlet.App.ViewModels;

/// <summary>
/// The first thing standing between the operator and a decode, in one sentence,
/// or nothing at all when there is no such thing.
/// </summary>
/// <remarks>
/// <para>**AN EMPTY PANEL IS INDISTINGUISHABLE FROM A BROKEN ONE** (Tim,
/// 2026-08-28), and that ruling is why every panel on this tab carries an idle
/// line. This is the case the idle lines cannot cover: the table is empty and
/// there is a reason. A blank decoded table looks the same whether the band is
/// quiet, the clock has never been checked, the radio is in plain USB or Hamlet
/// is listening to its own training radio, and finding out which by elimination
/// is how a morning gets spent.</para>
/// <para>**HAMLET ALREADY KNEW ALL OF THESE AND SAID NONE OF THEM IN ONE
/// PLACE.** The clock is measured and described on the mode strip, the waterfall
/// summary knows whether the source is simulated, the map knows what lives at
/// this frequency and the rig state knows the mode. An operator looking at an
/// empty table had to assemble that from three panels, and the one thing he
/// could not get from any of them is whether the band is quiet or the setup is
/// wrong.</para>
/// <para>**ORDERED, AND ONLY THE FIRST IS SAID.** The first wrong thing makes
/// the rest moot: a simulated source means the clock does not matter yet,
/// because no amount of clock accuracy will put a real station into audio Hamlet
/// generated itself.</para>
/// <para>**SILENCE IS THE CORRECT OUTPUT WHEN NOTHING IS WRONG**, and it is
/// asserted by a test. A readiness line that always says something is one the
/// operator stops reading, and a quiet band is not a fault.</para>
/// <para>**PURE, AND IT READS NO CLOCK.** Facts in, one sentence out, so every
/// branch is reachable from a test without a radio, a sound card or a
/// window.</para>
/// </remarks>
public static class DigitalReadiness
{
    /// <summary>What is said when nothing is wrong, which is nothing.</summary>
    public const string Nothing = "";

    /// <summary>
    /// The first thing that is wrong, or <see cref="Nothing"/>.
    /// </summary>
    /// <param name="listening">Whether any audio source is attached at all.</param>
    /// <param name="simulated">
    /// Whether that source is the training radio rather than the receiver.
    /// </param>
    /// <param name="clock">The measured clock offset, which may be unknown.</param>
    /// <param name="mode">The mode the radio reports, or null when unread.</param>
    /// <param name="dataVariant">
    /// Whether the radio is in the mode's data variant, or null when nobody has
    /// read the flag. Three answers because there are three states.
    /// </param>
    /// <param name="here">The neighborhood the dial is in, or null.</param>
    /// <returns>One sentence, or an empty string.</returns>
    public static string FirstProblem(
        bool listening,
        bool simulated,
        ClockOffset clock,
        CivMode? mode,
        bool? dataVariant,
        Neighborhood? here)
    {
        // 1. NOTHING IS LISTENING. Everything below it is a question about audio
        //    that does not exist.
        if (!listening)
        {
            return "nothing is listening yet, so there is no audio arriving "
                + "here at all and none of it can be cut into slots however "
                + "busy the band is. Hamlet opens the sound card when it starts "
                + "listening, and until it does this tab has nothing to work on.";
        }

        // 2. THE SOURCE IS THE TRAINING RADIO. Real FT8 will never appear, and
        //    the operator should know that before he waits a quarter of an hour
        //    to find out.
        if (simulated)
        {
            return "this is the training radio rather than the receiver, so "
                + "everything in the audio was made by Hamlet and nothing off "
                + "the air can reach the table below. Real signals start "
                + "arriving when the radio's own audio is the source.";
        }

        // 3. THE CLOCK. Unknown first, because an offset nobody has measured
        //    means no slots are cut at all, which is worse than slots cut
        //    against a clock that is known to be out.
        if (!clock.IsKnown)
        {
            return "the clock has not been checked against UTC yet, so where "
                + "the fifteen second boundaries fall is not known and nothing "
                + "is being cut into slots. It settles itself when the time "
                + "check answers, so this one is usually worth a moment before "
                + "you go looking anywhere else.";
        }

        if (ClockOffset.IsConcerning(clock))
        {
            // **WHAT IS CLAIMED HERE IS WHAT THE CODE ACTUALLY DOES** (§0.0).
            // The received wisdom is that a PC more than about a second off UTC
            // decodes nothing, and that is true of a decoder cutting slots on
            // the machine's own minute. Hamlet does not: `Ft8SlotCutter` cuts on
            // the measured offset, so it is still aligned to true UTC and a
            // sentence saying nothing will decode until the clock is fixed
            // would send the operator to fix the one thing that was already
            // handled.
            return $"the PC clock is {HowFarOut(clock)} against UTC, where FT8 "
                + "wants it inside about a second. Hamlet cuts its slots on the "
                + "offset it measured rather than on the machine's own minute, "
                + "so it is not lost, but a clock that far out drifts between "
                + "checks and the machine is worth putting back on a time "
                + "server.";
        }

        // 4. THE MODE. An unconfirmed read is not a fault (HM-DEC-056), so an
        //    unread flag says unknown rather than wrong.
        if (mode is null || dataVariant is null)
        {
            return "the radio has not said which mode it is in, so whether it "
                + "is on the data setting FT8 wants is unknown rather than "
                + "wrong. Nothing is being claimed about it until the radio "
                + "answers for itself.";
        }

        if (mode is not CivMode.Usb)
        {
            return $"the radio is in {CivValues.Name(mode.Value)}, and FT8 is "
                + "worked on the upper sideband through the computer, so what "
                + "is reaching this tab is not the audio these signals live in.";
        }

        if (dataVariant is false)
        {
            return "the radio is on the upper sideband but not on the data "
                + "setting, and USB-D is what takes the microphone out of the "
                + "path and hands the computer the receiver's own audio. It is "
                + "worth changing before you decide the band is quiet.";
        }

        // 5. THE DIAL. A frequency the map has no block for says nothing, the
        //    same way an unread mode does: not knowing where you are is not
        //    evidence that you are in the wrong place (HM-DEC-009).
        if (here is not null && here.Family is not ModeFamily.Digital)
        {
            return $"the dial is in {here.Name}, which the map does not have "
                + "the digital modes gathering in, so an FT8 signal turning up "
                + "here would be a stranger. The band map has the digital "
                + "blocks marked if you want to move to one.";
        }

        return Nothing;
    }

    /// <summary>How far the clock is out, spoken rather than counted.</summary>
    /// <param name="clock">A known offset.</param>
    /// <returns>A few words, e.g. "about 4 seconds slow".</returns>
    /// <remarks>
    /// **NUMBERS ARE SPOKEN, NOT COUNTED** (§0.7). The mode strip beside this
    /// already carries the figure to two decimal places, and repeating it here
    /// would be the same measurement twice in different words. What this line
    /// needs is the size of the problem.
    /// </remarks>
    private static string HowFarOut(ClockOffset clock)
    {
        var seconds = clock.OffsetSeconds ?? 0;
        var size = Math.Abs(seconds);

        // Positive means the PC is behind UTC, which is what `ClockOffset`
        // records and what its own describing line calls slow.
        var sign = seconds >= 0 ? "slow" : "fast";

        var howFar = size < 0.75
            ? "about half a second"
            : size < 1.25
                ? "about a second"
                : size < 1.75
                    ? "about a second and a half"
                    : $"about {size:0} seconds";

        return $"{howFar} {sign}";
    }
}
