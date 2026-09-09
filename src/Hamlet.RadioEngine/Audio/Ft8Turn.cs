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

    /// <summary>**His own transmission is going out right now.**</summary>
    /// <remarks>
    /// It is his slot and he is using it, which is a different sentence from his
    /// slot being open. Telling him it is open while his own carrier is on it is
    /// the §0.0 exposure this state exists to close.
    /// </remarks>
    Transmitting,

    /// <summary>A transmission was stopped before its slot ran out.</summary>
    /// <remarks>
    /// **NOT THE SAME AS THE SLOT HAVING RUN ITS COURSE.** Part of a message went
    /// out and the rest did not, so whoever was listening heard something
    /// incomplete, and a line saying the slot simply ended would hide that.
    /// </remarks>
    Stopped,
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
/// The second-of-minute the other station transmits on, `0` or `15` on FT8's grid,
/// or null where it is not known. It is the parity rather than one boundary: a
/// station on `15` transmits on `15` and `45` alike.
/// **On a grid whose slot is not a whole number of seconds this is a rounding and
/// <see cref="TheirSlotSecondExact"/> is the measurement.** Nothing on screen reads
/// it today; anything that starts to must read the exact one.
/// </param>
/// <param name="SecondsLeftExact">
/// **How much of the slot is actually left, unrounded**, or null where no slot could
/// be placed.
/// </param>
/// <remarks>
/// <para>**WHY THERE IS AN EXACT ONE AT ALL** (work instruction 290 task 4). FT4's
/// slot is 7.5 seconds and does not divide into whole seconds, so a countdown that
/// rounds up reads *8* for the first half-second of every slot - which asserts a
/// slot length the application is not cutting on, on the one screen the operator
/// times his transmission by. That is arithmetic meeting a type rather than a fault
/// in the beat, and the answer is to keep the measurement and let the display round
/// it, rather than to round it on the way in and have nothing left to check
/// against.</para>
/// <para>**<see cref="SecondsLeft"/> IS UNCHANGED AND FT8'S SCREEN IS UNCHANGED.**
/// It is still the ceiling in whole seconds, still clamped never to reach zero, and
/// on a fifteen-second grid <see cref="CountText"/> renders exactly the digits it
/// always did.</para>
/// </remarks>
/// <param name="TheirSlotSecondExact">
/// The second-of-minute the other station transmits on, unrounded, or null.
/// </param>
/// <param name="Grid">
/// The grid this reading was taken on, or null meaning FT8's. **It is carried so the
/// display can tell a whole-second slot from a fractional one** without a second
/// copy of the mode arriving from somewhere else and disagreeing.
/// </param>
public readonly record struct Ft8Turn(
    Ft8TurnState State,
    int? SecondsLeft,
    int? TheirSlotSecond,
    double? SecondsLeftExact = null,
    double? TheirSlotSecondExact = null,
    SlotGrid? Grid = null)
{
    /// <summary>The grid this reading was taken on, FT8's where none was given.</summary>
    public SlotGrid On => Grid ?? SlotGrid.Ft8;

    /// <summary>True where the slot divides into whole seconds, as FT8's does.</summary>
    /// <remarks>
    /// **THE FORMAT IS DECIDED BY THE SLOT AND NOT BY THE TRANSMISSION.** FT8's
    /// transmission is 12.64 s and its countdown has always read whole seconds while
    /// the carrier is up; keeping the rule on the slot is what makes FT8's every
    /// state come out exactly as it did.
    /// </remarks>
    private bool CountsInWholeSeconds
        => Math.Abs(On.SlotSeconds - Math.Round(On.SlotSeconds)) < 1e-9;

    /// <summary>**The count as the operator reads it**, or "" where there is none.</summary>
    /// <remarks>
    /// <para>**WHOLE SECONDS ON A WHOLE-SECOND SLOT, ONE DECIMAL OTHERWISE, AND THE
    /// RULE IS THE SLOT'S RATHER THAN THE MOMENT'S.** On FT8 this renders the same
    /// digits <see cref="SecondsLeft"/> always rendered, in every state including
    /// the one where the carrier is up. On FT4 it reads <c>7.5</c> at the top of the
    /// slot, where rounding up would have read <c>8</c> - **and 8 is a slot length
    /// nothing in this application is cutting on** (§0.0). One decimal rather than
    /// two because the tick that drives it runs four times a second, so a hundredth
    /// would be a digit the reading cannot support.</para>
    /// <para>**AND IT ROUNDS UP TO ITS OWN LAST DIGIT, NEVER TO ZERO**, for the same
    /// reason <see cref="SecondsLeft"/> does: a zero says the slot is over while a
    /// station is still transmitting in it. The smallest it shows is <c>0.1</c>.</para>
    /// </remarks>
    public string CountText
    {
        get
        {
            if (SecondsLeftExact is not { } exact)
            {
                return SecondsLeft is { } rounded
                    ? rounded.ToString(CultureInfo.InvariantCulture)
                    : string.Empty;
            }

            if (CountsInWholeSeconds)
            {
                return (SecondsLeft ?? (int)Math.Ceiling(exact))
                    .ToString(CultureInfo.InvariantCulture);
            }

            // **THE EPSILON IS FOR FLOATING DUST AND NOTHING ELSE.** A remaining
            // time that is 7.4 by arithmetic and 7.4000000000000004 by binary would
            // otherwise ceil to 7.5 and put the slot's full length on the screen a
            // tenth of a second after it started. FT8's whole-second path above is
            // untouched and still has no epsilon, because it never had one.
            var tenths = Math.Max(1, (int)Math.Ceiling((exact * 10) - 1e-9));

            return (tenths / 10.0).ToString("0.0", CultureInfo.InvariantCulture);
        }
    }

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
    /// <param name="sendingSlotUtc">
    /// The slot his own transmission is going out in, or null where none is.
    /// </param>
    /// <param name="stopped">
    /// True where that transmission was stopped before its slot ran out.
    /// </param>
    /// <param name="grid">
    /// Which grid the beat runs on. **FT8's when nothing is asked for.**
    /// </param>
    public static Ft8Turn Read(
        DateTime pcUtc,
        ClockOffset offset,
        DateTime? theirLastSlotUtc,
        DateTime? sendingSlotUtc = null,
        bool stopped = false,
        SlotGrid? grid = null)
    {
        var on = grid ?? SlotGrid.Ft8;

        if (Ft8Slots.TrueUtc(pcUtc, offset) is not { } trueUtc)
        {
            return new Ft8Turn(Ft8TurnState.NoClock, null, null, null, null, grid);
        }

        // **HIS OWN CARRIER OUTRANKS EVERY OTHER READING.** Whose slot it is by
        // parity is still true and is no longer the useful sentence: he is
        // transmitting, and a line saying his slot is open while it is his own
        // signal filling it is the one thing this must not say (§0.0).
        // **THE SLOT CLOCK IS NOT THE TURN AND IS COMPUTED FIRST** (work
        // instruction 286 task 1). Boundaries land on :00, :15, :30 and :45 whatever
        // anybody is doing, so the count is available in every state a corrected
        // clock exists in - including the two where whose turn it is cannot be
        // derived. **Only `NoClock` has no count**, because with no measured offset
        // there is no boundary to count to.
        //
        // **AND THE EXACT FIGURE TRAVELS BESIDE THE ROUNDED ONE.** On FT4's grid
        // the rounded one reads 8 for the first half-second of a 7.5-second slot,
        // which is a countdown asserting a slot the cutter is not cutting.
        var exactLeft = on.SlotSeconds - on.IntoSlot(trueUtc);
        var left = Left(exactLeft, on.SlotSeconds);

        if (sendingSlotUtc is { } slot)
        {
            // **STOPPED KEEPS ITS NULL, AND THAT IS UNIT 277'S RULING RATHER THAN
            // AN OVERSIGHT.** `StoppedEarlySaysSoRatherThanPretendingItRan` states
            // it in as many words: there is nothing left running to count. Work
            // instruction 286 extends the count to the state where nothing has been
            // heard, which is the case Tim ruled on and the case its test names;
            // whether a stop should also show the next boundary is the same question
            // one state along and it is his, not this unit's to answer.
            if (stopped)
            {
                return new Ft8Turn(
                    Ft8TurnState.Stopped,
                    null,
                    TheirHalf(theirLastSlotUtc, on),
                    null,
                    TheirHalfExact(theirLastSlotUtc, on),
                    grid);
            }

            var goneSeconds = (trueUtc - slot).TotalSeconds;

            if (goneSeconds >= 0 && goneSeconds < on.TransmissionSeconds)
            {
                var exactOver = on.TransmissionSeconds - goneSeconds;
                var over = (int)Math.Ceiling(exactOver);

                return new Ft8Turn(
                    Ft8TurnState.Transmitting,
                    Math.Clamp(over, 1, (int)Math.Ceiling(on.TransmissionSeconds)),
                    TheirHalf(theirLastSlotUtc, on),
                    exactOver,
                    TheirHalfExact(theirLastSlotUtc, on),
                    grid);
            }
        }

        if (theirLastSlotUtc is not { } theirs || theirs == default)
        {
            return new Ft8Turn(
                Ft8TurnState.NoStationYet, left, null, exactLeft, null, grid);
        }

        var theirParity = ParityOf(theirs, on);
        var nowParity = ParityOf(on.SlotStart(trueUtc), on);

        return new Ft8Turn(
            theirParity == nowParity ? Ft8TurnState.Theirs : Ft8TurnState.Mine,
            left,
            theirParity * (int)on.SlotSeconds,
            exactLeft,
            theirParity * on.SlotSeconds,
            grid);
    }

    /// <summary>The half the other station uses, rounded, or null.</summary>
    private static int? TheirHalf(DateTime? theirLastSlotUtc, SlotGrid on)
        => theirLastSlotUtc is { } theirs && theirs != default
            ? ParityOf(theirs, on) * (int)on.SlotSeconds
            : null;

    /// <summary>The half the other station uses, unrounded, or null.</summary>
    private static double? TheirHalfExact(DateTime? theirLastSlotUtc, SlotGrid on)
        => theirLastSlotUtc is { } theirs && theirs != default
            ? ParityOf(theirs, on) * on.SlotSeconds
            : null;

    /// <summary>Which of the two alternating halves a slot belongs to.</summary>
    /// <param name="slotStartUtc">A slot boundary, corrected.</param>
    /// <param name="grid">The grid it was placed on, or null meaning FT8's.</param>
    /// <returns>0 for the slots at `:00` and `:30`, 1 for those at `:15` and `:45`.</returns>
    /// <remarks>
    /// <para>**UNIT 277'S RULE IS UNCHANGED IN SHAPE AND UNCHANGED IN FORCE**: the
    /// parity comes from what the other station actually sent, and nothing guesses
    /// it before a station has spoken. What changed is only how a boundary is turned
    /// into an index - it counts whole slots from the top of the minute rather than
    /// dividing the second-of-minute by fifteen, because on a 7.5-second grid four
    /// of the eight boundaries are not on a whole second at all.</para>
    /// <para>**AND IT STILL DOES NOT SHIFT AT A MINUTE OR AN HOUR.** That held on
    /// FT8 because four slots to the minute is an even number; it holds on FT4
    /// because eight is. <c>SlotGrid.SlotsPerMinute</c> is where that is checked.</para>
    /// </remarks>
    public static int ParityOf(DateTime slotStartUtc, SlotGrid? grid = null)
    {
        var on = grid ?? SlotGrid.Ft8;
        var sinceMinute = slotStartUtc.Ticks % TimeSpan.TicksPerMinute;
        var slotTicks = TimeSpan.TicksPerMinute / on.SlotsPerMinute;

        return (int)(sinceMinute / slotTicks % 2);
    }

    /// <summary>Whole seconds left of a slot, rounded up and never to zero.</summary>
    /// <param name="exactLeft">How much is really left.</param>
    /// <param name="slotSeconds">How long the whole slot is.</param>
    /// <returns>1 up to the ceiling of the slot.</returns>
    /// <remarks>
    /// **ROUNDED UP AND NEVER TO ZERO.** Half a second before the boundary the
    /// honest reading is *one second left*, and a zero on the display says the slot
    /// is over while the station is still transmitting in it.
    /// </remarks>
    private static int Left(double exactLeft, double slotSeconds)
        => Math.Clamp(
            (int)Math.Ceiling(exactLeft), 1, (int)Math.Ceiling(slotSeconds));

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
        // **THE WORDS COME OFF `CountText`, WHICH IS WHAT THE RING SHOWS.** A
        // sentence saying eight seconds beside a ring reading 7.5 would be the same
        // grid described two ways on one panel.
        var count = CountText;

        var seconds = count.Length == 0
            ? ""
            : count + (CountsInWholeSeconds && SecondsLeft == 1
                ? " second"
                : " seconds");

        return State switch
        {
            Ft8TurnState.Transmitting =>
                "You are transmitting, with " + seconds + " of it left. This slot is "
                + "yours and you are using it, so there is nothing to send into "
                + "until it finishes.",

            Ft8TurnState.Stopped =>
                "You stopped that transmission partway through its slot, so only "
                + "part of the message went out and whoever was listening heard "
                + "something incomplete.",

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
