using Hamlet.RadioEngine.Rig;

namespace Hamlet.RadioEngine.Explore;

/// <summary>
/// What the controls Hamlet cannot write are doing to the passband, and whether
/// it may claim the block is audible.
/// </summary>
/// <remarks>
/// <para>**THIS IS THE HALF OF "THE DATA SETTINGS ARE SET" THAT HAS NO WRITE
/// BEHIND IT** (work instruction 051, task 4). Hamlet sets the mode and what the
/// receive side needs, and then says so. The Twin PBT and RIT are not on that
/// list: there is no PBT write in the command table at all, and RIT is not this
/// unit's to touch. **A hand-set PBT closes the window regardless**, so a
/// sentence saying the radio is ready is a claim the app cannot support — and it
/// is the one sentence the operator acts on (§0.0).</para>
/// <para>**SO THE CLAIM IS SUPPRESSED BY UNCERTAINTY AND NOT ONLY BY A BAD
/// READING.** Away from centre and never read are different facts about the
/// radio and the same fact about what Hamlet may assert: it does not know the
/// passband is open. Unknown is not a quiet yes.</para>
/// <para>**AND TODAY IT CAN NEVER SAY YES, WHICH IS THE HONEST STATE RATHER THAN
/// A DEFECT.** Two of the three controls have no documented read in this
/// repository — the inner Twin PBT and RIT — so `CanClaimAudible` is false until
/// somebody reads p. 19-4 column-aware and closes them. Saying nothing is what
/// §0.0 asks for; saying "ready" would be the guess.</para>
/// </remarks>
public readonly record struct PassbandReport
{
    /// <summary>Where the Twin PBT sits when nobody has touched it.</summary>
    /// <remarks>
    /// The command table gives 00 00 to 02 55 for this family, so the centre is
    /// 128. Reading it as a number rather than a hertz figure is deliberate:
    /// nothing in this repository cites what one step is worth.
    /// </remarks>
    public const int PbtCentre = 128;

    /// <summary>
    /// How far from centre still counts as centred.
    /// </summary>
    /// <remarks>
    /// **ONE STEP, BECAUSE THE ALTERNATIVE IS A NUMBER NOBODY CITED.** A wider
    /// band would be Hamlet deciding how much passband shift is harmless, and it
    /// has no figure for what a step is worth in hertz.
    /// </remarks>
    public const int PbtSlack = 1;

    private PassbandReport(bool offCentre, bool unread, string sentence)
    {
        IsOffCentre = offCentre;
        SomethingWasNotRead = unread;
        Sentence = sentence;
    }

    /// <summary>Whether a control Hamlet read is away from neutral.</summary>
    public bool IsOffCentre { get; }

    /// <summary>Whether a control Hamlet needs was not read at all.</summary>
    public bool SomethingWasNotRead { get; }

    /// <summary>What to say, or empty where there is nothing to report.</summary>
    public string Sentence { get; } = "";

    /// <summary>
    /// Whether Hamlet may say the block is audible.
    /// </summary>
    /// <remarks>
    /// **THE WHOLE POINT OF THE TYPE.** False when something is off centre, and
    /// false when something was not read, because those are the same fact about
    /// what Hamlet knows.
    /// </remarks>
    public bool CanClaimAudible => !IsOffCentre && !SomethingWasNotRead;

    /// <summary>Read the passband controls out of the rig ledger.</summary>
    /// <param name="state">What Hamlet believes about the radio.</param>
    /// <returns>The report.</returns>
    /// <remarks>
    /// The remedy is named rather than described, because an operator staring at
    /// a silent radio needs the control on the front of it: **hold TWIN PBT CLR
    /// for a second, until the dot beside the width goes** (work instruction 051,
    /// citing p. 4-5).
    /// </remarks>
    public static PassbandReport ForState(RigState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var outer = state[RigField.TwinPbtOuter];
        var inner = state[RigField.TwinPbtInner];
        var rit = state[RigField.Rit];

        var offCentre = outer.IsKnown
                        && outer.Number is { } position
                        && Math.Abs(position - PbtCentre) > PbtSlack;

        var unread = !outer.IsKnown || !inner.IsKnown || !rit.IsKnown;

        if (offCentre)
        {
            return new PassbandReport(
                true, unread,
                "One of the passband controls has been moved off centre, which "
                + "narrows what you can hear whatever the mode says, and Hamlet "
                + "has no way to put it back. Hold TWIN PBT CLR for about a "
                + "second, until the little dot beside the filter width goes "
                + "away, and the receiver opens up again.");
        }

        if (unread)
        {
            // **NAMED, NOT SUMMARISED.** "Some settings are unknown" sends
            // somebody to check everything; naming the two sends them to the two.
            var missing = new List<string>();

            if (!outer.IsKnown)
            {
                missing.Add("the passband tuning");
            }

            if (!inner.IsKnown)
            {
                missing.Add("the inner passband control");
            }

            if (!rit.IsKnown)
            {
                missing.Add("the receive offset");
            }

            return new PassbandReport(
                false, true,
                $"Hamlet cannot read {Join(missing)} on this radio, so it will "
                + "not tell you the band is open when it has no way to know. If "
                + "you are hearing nothing here, hold TWIN PBT CLR for a second "
                + "and check that RIT is off, and that rules both of them out.");
        }

        return new PassbandReport(false, false, "");
    }

    /// <summary>A list a person would say out loud.</summary>
    private static string Join(IReadOnlyList<string> parts)
        => parts.Count switch
        {
            0 => "",
            1 => parts[0],
            2 => $"{parts[0]} or {parts[1]}",
            _ => string.Join(", ", parts.Take(parts.Count - 1))
                 + $" or {parts[^1]}",
        };
}
