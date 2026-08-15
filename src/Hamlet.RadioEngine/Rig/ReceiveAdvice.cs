using Hamlet.RadioEngine.Civ;

namespace Hamlet.RadioEngine.Rig;

/// <summary>What Hamlet would change, and whether it needs to (HM-DEC-084).</summary>
/// <param name="Write">The setting, with its citation.</param>
/// <param name="Value">What it would be set to.</param>
/// <param name="Says">One line: what would change and why.</param>
/// <param name="AlreadyRight">True when there is nothing to do.</param>
/// <param name="Unreadable">
/// True when Hamlet could not read the setting, so it neither acts nor pretends.
/// </param>
public sealed record ReceiveSuggestion(
    CivWrite Write, int Value, string Says, bool AlreadyRight, bool Unreadable)
{
    /// <summary>True when pressing the button would actually do this one.</summary>
    public bool WouldChange => !AlreadyRight && !Unreadable;
}

/// <summary>
/// "I can hear it and Hamlet can't" (HM-DEC-084).
/// </summary>
/// <remarks>
/// <para>**SETTINGS ARE CONSEQUENCES OF INTENT, NEVER THINGS THE OPERATOR
/// OPERATES.** A rig control app gives somebody a Noise Blanker button and
/// expects them to know when to press it. Hamlet gives them one button that says
/// "I can hear it and you can't", does the handful of things that usually cause
/// that, says what it changed in plain words, and offers to put it back. Nobody
/// ever learns what auto notch is. They learn that pressing that button usually
/// helps.</para>
/// <para>**THE LIST IS COMPUTED FROM LIVE RIG STATE AND IS NEVER HARDCODED.**
/// What is already right says so and stays visible, because hiding it is tidier
/// and teaches nothing while showing it is the app proving what it checked.
/// **What could not be read says that**, rather than being guessed at or
/// silently dropped, which would leave somebody believing Hamlet had looked at
/// something it never saw (§0.0).</para>
/// <para>Nothing here can put a signal on the air. Every write it proposes is
/// tier one (<see cref="RigWriteTier.Receive"/>), which is what makes "do all
/// four" one press rather than four confirmations.</para>
/// <para>Pure: state in, suggestions out (§5).</para>
/// </remarks>
public static class ReceiveAdvice
{
    /// <summary>Below this the receive gain is holding signals down.</summary>
    /// <remarks>
    /// The evening this was written it sat at 42 percent and the receiver was
    /// deaf for two hours. Anything under most of the way open is worth saying,
    /// because a gain control is the one thing on the front panel that silently
    /// undoes everything else.
    /// </remarks>
    public const int OpenGainAbove = 240;

    /// <summary>
    /// Everything Hamlet would change to hear a faint signal better.
    /// </summary>
    /// <param name="state">What Hamlet knows.</param>
    /// <returns>The list, in the order it reads, never null.</returns>
    public static IReadOnlyList<ReceiveSuggestion> For(RigState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var inMorse = state.Mode is { } mode && CivValues.IsCw(mode);

        return new[]
        {
            AutoNotch(state, inMorse),
            NoiseBlanker(state),
            Preamp(state),
            Gain(state),
        };
    }

    /// <summary>
    /// The auto notch, which is the one that prompted the whole ruling.
    /// </summary>
    /// <remarks>
    /// A Morse note is a steady tone switching on and off, and the auto notch
    /// hunts steady tones and takes them out. In CW it removes the thing being
    /// decoded. Hamlet has printed that on its diagnostics screen for weeks and
    /// could not act on it, and that gap is the feature.
    /// </remarks>
    private static ReceiveSuggestion AutoNotch(RigState state, bool inMorse)
    {
        var value = state[RigField.AutoNotch];

        if (!value.IsKnown)
        {
            return Unread(
                CivWrites.AutoNotch, 0,
                "Hamlet could not read the automatic notch, so it is leaving it "
                + "alone.");
        }

        if (value.Number is 0)
        {
            return Fine(
                CivWrites.AutoNotch,
                "The automatic notch is already off, which is what Morse wants.");
        }

        return new ReceiveSuggestion(
            CivWrites.AutoNotch, 0,
            inMorse
                ? "Turn off the automatic notch. It removes steady tones, and a "
                  + "Morse note is a steady tone switching on and off, so it is "
                  + "hunting the thing you are trying to read."
                : "Turn off the automatic notch. It removes steady tones, which "
                  + "is what a Morse signal is.",
            AlreadyRight: false, Unreadable: false);
    }

    /// <summary>The noise blanker, which mistakes strong signals for clicks.</summary>
    private static ReceiveSuggestion NoiseBlanker(RigState state)
    {
        var value = state[RigField.NoiseBlanker];

        if (!value.IsKnown)
        {
            return Unread(
                CivWrites.NoiseBlanker, 0,
                "Hamlet could not read the noise blanker, so it is leaving it "
                + "alone.");
        }

        return value.Number is 0
            ? Fine(CivWrites.NoiseBlanker, "The noise blanker is already off.")
            : new ReceiveSuggestion(
                CivWrites.NoiseBlanker, 0,
                "Turn off the noise blanker. It mutes the instant a sharp tick "
                + "arrives, and on a busy band a strong nearby signal looks like "
                + "a tick and gets chopped along with the noise.",
                AlreadyRight: false, Unreadable: false);
    }

    /// <summary>The preamp, which is what a faint signal wants.</summary>
    private static ReceiveSuggestion Preamp(RigState state)
    {
        var value = state[RigField.Preamp];

        if (!value.IsKnown)
        {
            return Unread(
                CivWrites.Preamp, 1,
                "Hamlet could not read the preamp, so it is leaving it alone.");
        }

        return value.Number is > 0
            ? Fine(CivWrites.Preamp, "The preamp is already on.")
            : new ReceiveSuggestion(
                CivWrites.Preamp, 1,
                "Switch the preamp on. It is more gain at the front end, which is "
                + "what a faint signal needs.",
                AlreadyRight: false, Unreadable: false);
    }

    /// <summary>The receive gain, which cost two hours.</summary>
    private static ReceiveSuggestion Gain(RigState state)
    {
        var value = state[RigField.RfGain];

        if (!value.IsKnown)
        {
            return Unread(
                CivWrites.RfGain, 255,
                "Hamlet could not read the receive gain, so it is leaving it "
                + "alone.");
        }

        if (value.Number is { } level && level >= OpenGainAbove)
        {
            return Fine(
                CivWrites.RfGain,
                "The receive gain is already open all the way, so there is "
                + "nothing to do there.");
        }

        var percent = value.Number is { } n
            ? (int)Math.Round(n / 255.0 * 100)
            : 0;

        return new ReceiveSuggestion(
            CivWrites.RfGain, 255,
            $"Open the receive gain all the way. It is at about {percent} percent, "
            + "and a gain control turned down is the one thing that quietly "
            + "undoes everything else.",
            AlreadyRight: false, Unreadable: false);
    }

    private static ReceiveSuggestion Fine(CivWrite write, string says)
        => new(write, 0, says, AlreadyRight: true, Unreadable: false);

    private static ReceiveSuggestion Unread(CivWrite write, int value, string says)
        => new(write, value, says, AlreadyRight: false, Unreadable: true);
}
