using System.Globalization;

namespace Hamlet.RadioEngine.Audio;

/// <summary>Whose slot the current one is, and why that is or is not known.</summary>
/// <remarks>
/// **THE FOUR ARE NOT THREE PLUS A FALLBACK.** Each unknown has a different cause
/// and a different thing the operator could do about it, and collapsing them into
/// one *unknown* would leave him unable to tell a clock he could fix from a station
/// he has simply not heard yet.
/// </remarks>
public enum Ft8TurnState
{
    /// <summary>Nothing has been heard from the station, so there is no parity.</summary>
    NoStationYet,

    /// <summary>The clock offset has never been measured, so no slot can be placed.</summary>
    NoClock,

    /// <summary>The current slot belongs to the other station.</summary>
    Theirs,

    /// <summary>The current slot belongs to the operator.</summary>
    Mine,
}

/// <summary>
/// **The beat: whose fifteen seconds this is, and how many are left of it.**
/// </summary>
/// <remarks>
/// <para>**IT COUNTS DOWN AND IT DOES NOTHING ELSE** (§0.2, and the standing rule
/// of one click for one transmission). Nothing in this file arms, queues, sends or
/// touches a transmitter, and reaching zero does nothing at all. A countdown that
/// transmits when it expires is automatic sequencing wearing a clock's face, and
/// it is the one thing this phase forbids outright.</para>
/// <para>**IT IS A READING AND SO IT MAY BE UNKNOWN** (§0.0). Telling the operator
/// it is his turn when it is not sends him into a slot the other station is using,
/// which is worse than telling him nothing: he would transmit on top of the very
/// station he is trying to work, and the collision would look to him like a station
/// that stopped answering.</para>
/// <para>**WHY IT EXISTS.** On 2026-09-08 a station came back at -09 and repeated
/// the same report three times while the operator answered a slot or two late each
/// round. Every fact needed to see that was already in the application and none of
/// it was on the screen.</para>
/// </remarks>
/// <param name="State">Whose slot it is, or which of the two reasons it is unknown.</param>
/// <param name="SecondsLeft">
/// Whole seconds until the current slot ends, 1 to 15, or null where no slot could
/// be placed. **Rounded up**, because a countdown showing 0 for the last part of a
/// second says the slot has ended while it has not.
/// </param>
/// <param name="TheirSlotSecond">
/// The second-of-minute the other station transmits on, `0` or `15`, or null where
/// it is not known. It is the parity rather than one boundary: a station on `15`
/// transmits on `15` and `45` alike.
/// </param>
public readonly record struct Ft8Turn(
    Ft8TurnState State, int? SecondsLeft, int? TheirSlotSecond)
{
    /// <summary>How long one slot is, in seconds.</summary>
    private const int Slot = (int)Ft8Slots.SlotSeconds;

    /// <summary>True where the turn is a fact rather than an absence.</summary>
    public bool IsKnown => State is Ft8TurnState.Theirs or Ft8TurnState.Mine;

    /// <summary>True where the operator's own slot is the one running.</summary>
    /// <remarks>
    /// **FALSE IS NOT "THEIRS".** It is false for both unknowns as well, which is
    /// correct for anything deciding whether to encourage him: not knowing is not
    /// his turn.
    /// </remarks>
    public bool MineNow => State == Ft8TurnState.Mine;

    /// <summary>
    /// **Read the beat from the corrected clock and what the station has sent.**
    /// </summary>
    /// <param name="pcUtc">What the machine's clock says.</param>
    /// <param name="offset">The measured offset, which may be unknown.</param>
    /// <param name="theirLastSlotUtc">
    /// The slot boundary of the most recent message heard from the station, or null
    /// where nothing has been heard from it.
    /// </param>
    /// <returns>The turn, complete with its reason where it is not known.</returns>
    /// <remarks>
    /// <para>**THE PARITY COMES FROM THE STATION'S OWN TRANSMISSION AND IS NOT
    /// GUESSED.** A slot boundary's second is always 0, 15, 30 or 45, so
    /// `(second / 15) % 2` says which of the two alternating halves the station
    /// uses, and a minute holds four slots, an even number, so that parity does not
    /// shift at a minute or an hour boundary.</para>
    /// <para>**THE MOST RECENT TRANSMISSION IS THE EVIDENCE**, rather than the
    /// first or a vote across all of them. A station that has changed which half it
    /// uses has told the operator so with its latest transmission, and an average
    /// over the evening would answer with where it used to be.</para>
    /// <para>**WITH NO OFFSET THERE IS NO ANSWER AT ALL**, and the PC clock is not
    /// substituted for one. The send path does substitute it, defensibly, because a
    /// boundary has to be picked for something the operator has already clicked. A
    /// turn line has no such obligation, and a countdown run off an uncorrected
    /// clock and presented as the beat is a measurement the application did not
    /// make.</para>
    /// </remarks>
    public static Ft8Turn Read(
        DateTime pcUtc, ClockOffset offset, DateTime? theirLastSlotUtc)
    {
        if (Ft8Slots.TrueUtc(pcUtc, offset) is not { } trueUtc)
        {
            return new Ft8Turn(Ft8TurnState.NoClock, null, null);
        }

        var left = Left(trueUtc);

        if (theirLastSlotUtc is not { } theirs || theirs == default)
        {
            return new Ft8Turn(Ft8TurnState.NoStationYet, left, null);
        }

        var theirParity = ParityOf(theirs);
        var nowParity = ParityOf(Ft8Slots.SlotStart(trueUtc));

        return new Ft8Turn(
            theirParity == nowParity ? Ft8TurnState.Theirs : Ft8TurnState.Mine,
            left,
            theirParity * Slot);
    }

    /// <summary>Which of the two alternating halves a slot belongs to.</summary>
    /// <param name="slotStartUtc">A slot boundary, corrected.</param>
    /// <returns>0 for the slots at `:00` and `:30`, 1 for those at `:15` and `:45`.</returns>
    public static int ParityOf(DateTime slotStartUtc)
        => (slotStartUtc.Second / Slot) % 2;

    /// <summary>Whole seconds left of the slot a moment falls in.</summary>
    /// <param name="trueUtc">A corrected moment.</param>
    /// <returns>1 to 15.</returns>
    /// <remarks>
    /// **ROUNDED UP AND NEVER TO ZERO.** Half a second before the boundary the
    /// honest reading is *one second left*, and a zero on the display says the slot
    /// is over while the station is still transmitting in it.
    /// </remarks>
    private static int Left(DateTime trueUtc)
    {
        var into = Ft8Slots.IntoSlot(trueUtc);
        var left = (int)Math.Ceiling(Ft8Slots.SlotSeconds - into);

        return Math.Clamp(left, 1, Slot);
    }

    /// <summary>What the panel says about the beat, in the app's own voice.</summary>
    /// <returns>One sentence, which never claims a turn it has not derived.</returns>
    /// <remarks>
    /// <para>**THE REASON IS ATTACHED TO THE FACT** (§0.7). Saying the turn is
    /// unknown teaches nothing; saying nobody has transmitted yet, so there is no
    /// pattern to read, tells him what would change it.</para>
    /// <para>**THE LATE ANSWER IS NAMED WHERE IT IS ABOUT TO HAPPEN.** A click in
    /// his own slot arms the following one, which belongs to the other station, and
    /// that is exactly the mistake that cost him the contact on 2026-09-08. The
    /// line says so and stops there. It does not refuse, disable, or delay
    /// anything: nothing is hidden, closed or forbidden by the application.</para>
    /// </remarks>
    public string Line()
    {
        var seconds = SecondsLeft is { } s
            ? s.ToString(CultureInfo.InvariantCulture) + (s == 1 ? " second" : " seconds")
            : "";

        return State switch
        {
            Ft8TurnState.NoClock =>
                "Hamlet has not measured the clock yet, so it cannot say where the "
                + "slot boundaries fall or whose turn this is.",

            Ft8TurnState.NoStationYet =>
                "Nobody has transmitted to you yet, so there is no pattern to read "
                + "and Hamlet will not guess whose turn it is. " + seconds
                + " left in this slot.",

            Ft8TurnState.Theirs =>
                "Their slot, with " + seconds + " left. Yours is next, so a message "
                + "you click now goes out at the top of it.",

            _ =>
                "Your slot, with " + seconds + " left of it. Anything you click now "
                + "waits for the slot after this one, which is theirs, and arriving "
                + "a slot late is how a station gives up and starts again.",
        };
    }
}
