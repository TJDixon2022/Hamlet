namespace Hamlet.RadioEngine.Explore;

/// <summary>
/// What each thing about a spot is worth, in one place (HM-DEC-058).
/// </summary>
/// <remarks>
/// <para>ONE NAMED PLACE, so changing what Hamlet thinks makes a good first
/// contact is one edit and a test change rather than a hunt through the scoring
/// code. There is deliberately no control for these in the app: they are a
/// judgment about what a beginner can work, and a slider would ask the operator
/// to make that judgment before they have the experience to make it.</para>
/// <para>WHAT IS NOT HERE IS THE POINT. There is no distance weight. Distance
/// does not run in a straight line with workability on HF: there is a skip zone,
/// so on 20 m a station two hundred miles off is often unreachable while
/// somebody two thousand miles out is easy, and on 40 m at night the close-in
/// stations come back in loud. Sorting nearest-first would put the hardest
/// contacts at the top and call them the best chance, which is a guess presented
/// as a decode. Distance stays on the card, where it teaches what ranges are
/// plausible on which band, and earns a vote when FG-007 lands and Hamlet can
/// say which bands are open to where.</para>
/// <para>THE OPERATOR NOW STATES A COPY SPEED AND THE RANKING READS IT, and the
/// line it may not cross is unchanged (HM-DEC-066, HM-OPEN-006). A stated speed
/// is a preference and not a measured ability, so Hamlet may say a station is
/// sending faster than the speed somebody named, because they named it, and it
/// still may not say they can or cannot copy it. Offered, never asserted. The
/// scale below is a fact about Morse rather than about this person: slower
/// sending is easier for anybody still learning, which is why the slow-speed
/// clubs exist.</para>
/// </remarks>
public static class SpotRankWeights
{
    /// <summary>
    /// Somebody who carried a radio somewhere on purpose to be called.
    /// </summary>
    /// <remarks>
    /// The strongest single thing on the list. That operator needs contacts for
    /// the activation to count and will slow down for an obvious beginner, which
    /// makes them the friendliest contact on the band.
    /// </remarks>
    public const int Activation = 30;

    /// <summary>An open invitation to anybody.</summary>
    public const int CallingCq = 25;

    /// <summary>Reported as DX activity without a call type.</summary>
    public const int Dx = 5;

    /// <summary>A closed conversation running at speed.</summary>
    public const int ContestExchange = -10;

    /// <summary>
    /// A beacon, which answers nobody.
    /// </summary>
    /// <remarks>
    /// Decisive rather than merely heavy, and larger than every positive here
    /// added together. A beacon is strong, close, steady and permanently useless
    /// for making a contact, so it scores well on every other axis and would
    /// otherwise float above real stations. It is pushed to the bottom rather
    /// than hidden: a band's beacons are worth seeing on the map.
    /// </remarks>
    public const int Beacon = -200;

    /// <summary>
    /// Whether that person is probably still there, scaled by liveness.
    /// </summary>
    /// <remarks>
    /// <para>ONE CLOCK, SHARED WITH THE DISPLAY (HM-DEC-057). This multiplies
    /// <see cref="SpotLensView.Liveness"/>, which runs across the spot's own
    /// source lifetime, so an hour-old park activation and an hour-old skimmer
    /// report are not the same number. Before this the rank had its own buckets
    /// in flat minutes and the fade followed the source lifetimes, so a card
    /// could look faded and rank fresh on the same screen.</para>
    /// <para>Set level with an activation on purpose. A live skimmer report of
    /// somebody calling CQ can then beat a nearly-spent activation, and a fresh
    /// activation beats both, which is the order a person would put them in.
    /// </para>
    /// </remarks>
    public const int Liveness = 30;

    /// <summary>
    /// What a spot at the very end of its lifetime is worth, which is less than
    /// nothing.
    /// </summary>
    /// <remarks>
    /// A FINISHED ACTIVATION MUST NEVER OUTRANK A LIVE STATION CALLING CQ
    /// (HM-DEC-058), and the absence of a bonus was not enough to guarantee it:
    /// an activation calling CQ in Morse at a relaxed pace scores well over
    /// seventy before liveness is counted at all, so somebody who packed up an
    /// hour ago would still have led the list. So liveness runs from a penalty
    /// to a bonus rather than from nothing to a bonus, and the score slides
    /// across the source's own ruled lifetime rather than stepping at a
    /// threshold nobody could see coming.
    /// </remarks>
    public const int Expired = -25;

    /// <summary>Morse, which is what Hamlet is built to decode.</summary>
    public const int Morse = 12;

    /// <summary>
    /// A digital mode Hamlet cannot read until phase 3.
    /// </summary>
    /// <remarks>
    /// The one weighting that is about Hamlet rather than about the air. FT8 is
    /// the busiest thing on most bands and against live feeds it swamped the top
    /// of this list, and recommending it as somebody's next ten minutes is
    /// recommending they watch a waterfall. The card says so, in those words, so
    /// the preference is stated rather than smuggled.
    /// </remarks>
    public const int DigitalHamletCannotRead = -20;

    /// <summary>
    /// A receiver in the operator's own corner of the country decoded it.
    /// </summary>
    /// <remarks>
    /// NOT A DISTANCE, AND THE DIFFERENCE IS THE WHOLE REASON THIS IS ALLOWED TO
    /// VOTE (HM-DEC-038). What a skimmer report states is where a receiver that
    /// heard the signal is standing, which is the closest thing to "your
    /// receiver will hear it too" that any spot network can honestly offer. It
    /// says nothing about where the transmitter is, and it is only read for
    /// sources that report a receiver.
    /// </remarks>
    public const int ReceiverNearby = 18;

    /// <summary>A receiver on this continent decoded it.</summary>
    public const int ReceiverOnContinent = 8;

    /// <summary>Somebody measured it and it is loud.</summary>
    public const int StrongSignal = 12;

    /// <summary>Somebody measured it and it is comfortable.</summary>
    public const int FairSignal = 7;

    /// <summary>Somebody measured it and it is thin.</summary>
    public const int WeakSignal = 2;

    /// <summary>Somebody measured it and it is barely above the noise.</summary>
    public const int BarelyThere = -2;

    /// <summary>Ten or more receivers report the same transmission.</summary>
    /// <remarks>
    /// Agreement between receivers is evidence rather than inference. A station
    /// a dozen skimmers all decoded is being heard widely, and that is the best
    /// answer available to "will I hear it".
    /// </remarks>
    public const int ManyReceivers = 10;

    /// <summary>Five or more receivers report it.</summary>
    public const int SeveralReceivers = 6;

    /// <summary>More than one receiver reports it.</summary>
    public const int AFewReceivers = 3;

    /// <summary>Morse at a relaxed pace.</summary>
    public const int RelaxedSpeed = 12;

    /// <summary>Morse at an ordinary pace.</summary>
    public const int ModerateSpeed = 8;

    /// <summary>Morse at a brisk pace: neither help nor hindrance here.</summary>
    public const int QuickSpeed = 0;

    /// <summary>Morse fast enough that the letters run together.</summary>
    public const int VeryFastSpeed = -6;

    /// <summary>At or under this, Morse is a relaxed pace.</summary>
    /// <remarks>
    /// Also the copy speed a fresh install starts at (HM-DEC-066). One number
    /// rather than two: the pace the slow-speed clubs run at is the same pace
    /// this scale calls relaxed, and two constants that mean the same thing
    /// drift apart the first time somebody edits one of them (§0).
    /// </remarks>
    public const int RelaxedWpm = 13;

    /// <summary>At or under this, Morse is an ordinary pace.</summary>
    public const int ModerateWpm = 18;

    /// <summary>Above this, Morse is very fast.</summary>
    public const int QuickWpm = 24;

    /// <summary>
    /// How far above a stated copy speed still counts as a little over.
    /// </summary>
    /// <remarks>
    /// Derived from the scale above rather than typed again, so a stated speed
    /// of 13 reproduces exactly the bands this ranking has always used and any
    /// other stated speed carries the same shape up or down with it
    /// (HM-DEC-066).
    /// </remarks>
    public const int ALittleOverWpm = ModerateWpm - RelaxedWpm;

    /// <summary>How far above a stated copy speed counts as well over.</summary>
    public const int WellOverWpm = QuickWpm - RelaxedWpm;

    /// <summary>Signal in dB at or above which RBN reports read as strong.</summary>
    public const int StrongSignalDb = 20;

    /// <summary>How many phrases a card's reason may carry.</summary>
    /// <remarks>
    /// Four rather than three, so the ordering is explainable from the screen
    /// (HM-DEC-058). A card that is ranked highly without saying why is a guess
    /// presented as a decode, and three phrases were losing the liveness clause
    /// on exactly the cards where it mattered most.
    /// </remarks>
    public const int MaxReasonParts = 4;
}
