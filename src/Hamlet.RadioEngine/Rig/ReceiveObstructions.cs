namespace Hamlet.RadioEngine.Rig;

/// <summary>
/// One receive-path setting standing between the operator and a readable
/// signal, named with the control that changes it.
/// </summary>
/// <param name="Setting">Which setting, in the radio's own words.</param>
/// <param name="Says">
/// What it is doing to what he is trying to read, and the control on the front
/// of the radio that changes it.
/// </param>
public readonly record struct ReceiveObstruction(string Setting, string Says);

/// <summary>
/// What is in the way on the receive side, read-only.
/// </summary>
/// <remarks>
/// <para>**HAMLET SHOWS THE RECEIVE PATH'S OWN SETTINGS AND SAYS WHAT THEY MEAN,
/// AND DOES NOT WRITE THEM.** HM-DEC-148 did this for the preamp and the
/// attenuator and stopped there. The noise blanker, the noise reduction and the
/// filter width are the same class of fault: Hamlet reads all three from the
/// radio, records them in the sidecar, and has mentioned none of them.</para>
/// <para>**THE OPERATOR DOES NOT KNOW THE RADIO, AND THAT IS THE PREMISE OF THE
/// APPLICATION RATHER THAN A GAP IN HIM.** Advice delivered in a chat window is
/// not the application, and a diagnosis in a text file read the next morning is
/// not help.</para>
/// <para>**THIS IS NOT <see cref="ReceiveAdvice"/> AND THE DIFFERENCE IS THE
/// RULING.** That one proposes writes and is HM-DEC-084's tier one: it exists so
/// one press can put four things right. This one writes nothing and offers
/// nothing. It names what is in the way and the knob that changes it, and the
/// operator turns it.</para>
/// <para>**A CONTROL ALREADY IN THE RIGHT POSITION IS NOT MENTIONED**, because
/// advice about a knob that is already right is noise and it teaches the reader
/// to look past the panel. **And a value that could not be read says so** rather
/// than being asserted as off (HM-DEC-009): Hamlet not having looked at
/// something and Hamlet having looked and found it harmless are different facts,
/// and only one of them is a reason to relax.</para>
/// <para>Pure: state in, sentences out (§5).</para>
/// </remarks>
public static class ReceiveObstructions
{
    /// <summary>
    /// Everything standing between the operator and a readable signal.
    /// </summary>
    /// <param name="state">What Hamlet has read from the radio.</param>
    /// <param name="inMorse">Whether the radio is in a Morse mode.</param>
    /// <param name="competitorInside">
    /// True when the survey has actually found somebody else keying inside the
    /// passband. The filter is mentioned on this rather than on a width, because
    /// a measurement is a fact and a threshold would be a judgement nobody ruled.
    /// </param>
    /// <returns>What to say, in reading order, never null and often empty.</returns>
    public static IReadOnlyList<ReceiveObstruction> For(
        RigState state, bool inMorse, bool competitorInside)
    {
        ArgumentNullException.ThrowIfNull(state);

        var found = new List<ReceiveObstruction>();

        // **THE TWO THAT RESHAPE THE ENVELOPE COME FIRST, AND THAT IS WHY THEY
        // ARE HERE AT ALL.** Amplitude is what this decoder measures: it scores
        // every hop against the key being down and the key being up, and a
        // setting that alters the shape of a mark alters the evidence directly.
        // Neither is wrong to use and neither is being called a mistake; what is
        // wrong is Hamlet knowing and not saying.
        Add(found, state, RigField.NoiseBlanker, "noise blanker",
            "The noise blanker is on. It works by cutting out the loudest short "
            + "spikes, and the start of every Morse mark is a short spike, so it "
            + "can take bites out of the very edges the decoder measures. NB on "
            + "the front of the radio turns it off.");

        Add(found, state, RigField.NoiseReduction, "noise reduction",
            "The noise reduction is on. It smooths the audio, and what it smooths "
            + "away are the sharp starts and stops that tell a dit from a dah. NR "
            + "on the front of the radio turns it off.");

        found.AddRange(Filter(state, inMorse, competitorInside));

        return found;
    }

    /// <summary>A setting that is only in the way when it is switched on.</summary>
    private static void Add(
        List<ReceiveObstruction> found,
        RigState state,
        RigField field,
        string setting,
        string says)
    {
        var value = state[field];

        if (!value.IsKnown)
        {
            // **UNREAD IS NOT OFF** (HM-DEC-009). Saying nothing here would let a
            // reader take the quiet panel for a clean receive path, and the whole
            // reason this exists is that a setting nobody mentioned cost an
            // evening.
            found.Add(new ReceiveObstruction(
                setting,
                $"Hamlet could not read the {setting}, so it cannot say whether "
                + "it is in the way."));

            return;
        }

        if (value.Number is 0 or null)
        {
            return;
        }

        found.Add(new ReceiveObstruction(setting, says));
    }

    /// <summary>
    /// The filter, mentioned on a measurement rather than on a width.
    /// </summary>
    /// <remarks>
    /// <para>**IT IS NAMED WHEN SOMEBODY ELSE IS ACTUALLY INSIDE IT, NOT WHEN IT
    /// IS WIDER THAN A NUMBER.** A threshold would have to assert that some width
    /// is too wide for a signal Hamlet has not measured, which is a judgement
    /// nobody has ruled and the kind of confident claim §0.0 exists to stop. A
    /// competing station the survey has actually found is a fact, and the filter
    /// is what lets it in.</para>
    /// <para>**AND A FILTER ALREADY NARROW IS NOT MENTIONED EVEN THEN.** At the
    /// narrow end of this radio's Morse range there is nothing left to take,
    /// and telling somebody to narrow a filter that is already at five hundred
    /// hertz is advice about a knob in the right place.</para>
    /// </remarks>
    private static IEnumerable<ReceiveObstruction> Filter(
        RigState state, bool inMorse, bool competitorInside)
    {
        if (!inMorse || !competitorInside)
        {
            yield break;
        }

        var value = state[RigField.FilterBandwidth];

        if (!value.IsKnown || value.Number is not { } index)
        {
            yield return new ReceiveObstruction(
                "filter width",
                "Somebody else is keying inside the passband, and Hamlet could "
                + "not read the filter width, so it cannot say whether there is "
                + "room to narrow it.");

            yield break;
        }

        if (index <= ReceiveAdvice.CwFilterIndex)
        {
            yield break;
        }

        yield return new ReceiveObstruction(
            "filter width",
            "Somebody else is keying inside the passband and the filter is wide "
            + "enough to let them in. Everything inside it arrives in the same "
            + "audio the decoder measures. Turning FILTER down toward five "
            + "hundred hertz will shut them out, and the TWIN PBT controls will "
            + "slide what is left off them.");
    }
}
