namespace Hamlet.RadioEngine.Psk31;

/// <summary>What one candidate looked like on one search pass.</summary>
/// <param name="OffsetHz">Where in the passband it sits.</param>
/// <param name="StrengthDb">How far above the noise floor, in decibels.</param>
/// <param name="Coherence">How BPSK-shaped the keying is, 0 to 1.</param>
/// <param name="Crossed">True where it met every test to become a carrier.</param>
/// <remarks>
/// **A CANDIDATE IS NOT A STATION** (§0.0). It is a place in the passband that had enough
/// energy to be worth measuring, and most of them are noise. `Crossed` is the whole
/// difference, and a reader of the record has to be able to see the ones that did not.
/// </remarks>
public readonly record struct Psk31Candidate(
    double OffsetHz, double StrengthDb, double Coherence, bool Crossed);

/// <summary>One pass of the search over the passband.</summary>
/// <param name="Milliseconds">How long the pass took.</param>
/// <param name="Candidates">What it measured, strongest first.</param>
/// <param name="CarriersHeld">How many carriers were listed when it finished.</param>
/// <param name="SetChanged">True where a carrier appeared or was retired on this pass.</param>
/// <remarks>
/// **THIS IS THE ANSWER TO *WAS THE BAND EMPTY OR WAS THE SQUELCH SHUT*.** A pass with no
/// candidates at all says the band was quiet. A pass with candidates that all failed to
/// cross says the opposite: there was something there and Hamlet would not take it.
/// **Those two look identical on the screen and must never look identical in the file.**
/// </remarks>
public sealed record Psk31Pass(
    double Milliseconds,
    IReadOnlyList<Psk31Candidate> Candidates,
    int CarriersHeld,
    bool SetChanged);

/// <summary>Why a carrier stopped being listed.</summary>
/// <remarks>
/// <para>**THERE ARE TWO WAYS OUT OF THE LIST AND NEITHER OF THEM IS SILENCE** (work
/// instruction 324 task 3). The signal went, or the operator left. **A carrier that is
/// still on the air and saying nothing is a live carrier**, not a dead one: a PSK31
/// station idling between words keys continuous reversals for seconds at a time, and
/// retiring him for it is what made the operator's evening of 2026-09-11 a list of rows
/// that appeared for two seconds and vanished.</para>
/// <para>**WHAT WENT WITH UNIT 324.** `Silence`, `LostLock` and `OneWay` were three ways
/// to fail one test, and that test asked whether the carrier was *readable* rather than
/// whether it was *there*. Not being readable is now a state of a held carrier - the
/// panel says so in words and the record says so in `psk31_reading` - and not a cause of
/// death.</para>
/// </remarks>
public enum Psk31Retirement
{
    /// <summary>**The search stopped finding the signal at its offset.**</summary>
    /// <remarks>
    /// **THE ENERGY WENT, NOT THE CHARACTERS.** The place the carrier sits stopped
    /// standing <see cref="Psk31CarrierSearch.CandidateRatio"/> over the floor, on
    /// <see cref="Psk31CarrierSearch.RetireAfterPasses"/> passes in a row.
    /// </remarks>
    SignalGone,

    /// <summary>It never crossed at all and its trial ran out.</summary>
    /// <remarks>
    /// **NOT WRITTEN ONE BY ONE.** On a noisy band the search makes and drops a great
    /// many candidates, and an event each would bury the ones that matter; they are in
    /// every pass's candidate list with `crossed: false`. The value is here so that a
    /// reader of the record knows what that list is.
    /// </remarks>
    NeverCrossed,

    /// <summary>It was still being heard when the operator left the tab.</summary>
    /// <remarks>
    /// **A CARRIER THAT WAS STILL THERE MUST NOT VANISH FROM THE RECORD** (§0.0, work
    /// instruction 322 task 2, found by its own test). Measured: the four-signal fixture
    /// produced four appearances and **three** retirements, because the weakest station
    /// was still being heard when the audio ran out - so the file said a carrier appeared
    /// and never said what became of it. Every appearance is now accounted for.
    /// </remarks>
    ListeningStopped,
}

/// <summary>A carrier appearing or being retired.</summary>
/// <param name="Id">The carrier's own id, stable while it is held.</param>
/// <param name="Appeared">True on appearance, false on retirement.</param>
/// <param name="OffsetHz">Where it sits in the passband.</param>
/// <param name="StrengthDb">How far above the floor when this happened.</param>
/// <param name="Coherence">How BPSK-shaped it was when this happened.</param>
/// <param name="Passes">How many passes it was a candidate before it crossed.</param>
/// <param name="LifetimeSeconds">How long it was held, on retirement.</param>
/// <param name="Why">Why it was retired, or null on appearance.</param>
public sealed record Psk31CarrierChange(
    int Id,
    bool Appeared,
    double OffsetHz,
    double StrengthDb,
    double Coherence,
    int Passes,
    double LifetimeSeconds,
    Psk31Retirement? Why);

/// <summary>A held carrier going idle, or starting to type again.</summary>
/// <param name="Id">The carrier's own id, stable while it is held.</param>
/// <param name="Idling">True where he has stopped typing, false where characters resumed.</param>
/// <param name="OffsetHz">Where it sits in the passband.</param>
/// <param name="Quality">What its demodulator's squelch measure read at that moment.</param>
/// <param name="Seconds">How long the state it is leaving lasted, in audio seconds.</param>
/// <remarks>
/// <para>**AN IDLE GAP HAS TO SHOW UP IN THE FILE AS AN IDLE GAP** (work instruction 327
/// task 1). Until this existed, a station who stopped typing for eight seconds and started
/// again left the same trace in the record as a station who went off the air and came back:
/// characters, then no characters, then characters. **Those are two different evenings and
/// the operator was reading the record to tell them apart.**</para>
/// <para>**IT IS A STATE CHANGE AND NOT A HEARTBEAT.** One event when the typing stops and
/// one when it resumes, so an hour of a quiet station is two lines rather than fourteen
/// thousand.</para>
/// <para>**AND NOTHING PERSONAL IS IN IT** (HM-DEC-018, §2.1). An id, a frequency, a
/// number and a duration. Not one character of what he was typing.</para>
/// </remarks>
public sealed record Psk31CarrierActivity(
    int Id, bool Idling, double OffsetHz, double Quality, double Seconds);

/// <summary>What one held channel's demodulator is doing right now.</summary>
/// <param name="Id">The carrier's id.</param>
/// <param name="OffsetHz">Where it is being read.</param>
/// <param name="Open">True while the squelch is letting characters through.</param>
/// <param name="Quality">How BPSK-shaped the last few symbols were.</param>
/// <param name="AfcHz">How far the AFC has pulled from the first estimate.</param>
/// <param name="Characters">How many characters it has emitted.</param>
public readonly record struct Psk31ChannelState(
    int Id,
    double OffsetHz,
    bool Open,
    double Quality,
    double AfcHz,
    int Characters);

/// <summary>
/// **What happened inside the listener, for something outside it to write down.**
/// </summary>
/// <remarks>
/// <para>**THE ENGINE REPORTS FACTS AND KNOWS NOTHING ABOUT A RECORD** (§0.1). Nothing in
/// this file mentions telemetry, a category, a session or a file. It is a drained list of
/// things that happened, and what the shell does with them - write them, show them, ignore
/// them - is the shell's business.</para>
/// <para>**DRAINED RATHER THAN SUBSCRIBED.** An event the engine raised while the shell was
/// on another tab would either queue for ever or be dropped silently; asking for what has
/// happened since the last ask does neither, and it keeps the engine free of a callback
/// that could re-enter it.</para>
/// <para>**AND IT IS CAPPED.** A search pass can nominate a great many candidates on a
/// noisy band, and a list nobody drained could grow without bound. What is kept is the
/// most recent <see cref="Keep"/> of each kind, and the count of what was dropped is
/// reported so the record never quietly loses events without saying so (§0.0).</para>
/// </remarks>
public sealed class Psk31Watch
{
    /// <summary>How many of each kind are kept if nobody drains them.</summary>
    /// <remarks>
    /// **ENOUGH FOR A MINUTE OF A BUSY BAND.** Passes come about once a second and the
    /// shell drains on every tick, four times a second, so this is only ever reached when
    /// something has stopped asking - and then the drop count is the interesting number.
    /// </remarks>
    public const int Keep = 256;

    private readonly List<Psk31Pass> _passes = new();
    private readonly List<Psk31CarrierChange> _changes = new();
    private readonly List<Psk31CarrierActivity> _activity = new();

    /// <summary>How many passes were dropped because nobody drained them.</summary>
    public int PassesDropped { get; private set; }

    /// <summary>How many carrier changes were dropped for the same reason.</summary>
    public int ChangesDropped { get; private set; }

    /// <summary>How many idling and typing events were dropped for the same reason.</summary>
    public int ActivityDropped { get; private set; }

    /// <summary>Record one pass.</summary>
    /// <param name="pass">What the pass measured.</param>
    public void Add(Psk31Pass pass)
    {
        if (_passes.Count >= Keep)
        {
            _passes.RemoveAt(0);
            PassesDropped++;
        }

        _passes.Add(pass);
    }

    /// <summary>Record one carrier appearing or retiring.</summary>
    /// <param name="change">What happened.</param>
    public void Add(Psk31CarrierChange change)
    {
        if (_changes.Count >= Keep)
        {
            _changes.RemoveAt(0);
            ChangesDropped++;
        }

        _changes.Add(change);
    }

    /// <summary>Record one carrier going idle or starting to type again.</summary>
    /// <param name="activity">What happened.</param>
    public void Add(Psk31CarrierActivity activity)
    {
        if (_activity.Count >= Keep)
        {
            _activity.RemoveAt(0);
            ActivityDropped++;
        }

        _activity.Add(activity);
    }

    /// <summary>Take the idling and typing events since the last ask.</summary>
    /// <returns>What has happened, oldest first.</returns>
    public IReadOnlyList<Psk31CarrierActivity> DrainActivity()
    {
        if (_activity.Count == 0)
        {
            return Array.Empty<Psk31CarrierActivity>();
        }

        var taken = _activity.ToArray();

        _activity.Clear();

        return taken;
    }

    /// <summary>Take the passes since the last ask.</summary>
    /// <returns>What has happened, oldest first.</returns>
    public IReadOnlyList<Psk31Pass> DrainPasses()
    {
        if (_passes.Count == 0)
        {
            return Array.Empty<Psk31Pass>();
        }

        var taken = _passes.ToArray();

        _passes.Clear();

        return taken;
    }

    /// <summary>Take the carrier changes since the last ask.</summary>
    /// <returns>What has happened, oldest first.</returns>
    public IReadOnlyList<Psk31CarrierChange> DrainChanges()
    {
        if (_changes.Count == 0)
        {
            return Array.Empty<Psk31CarrierChange>();
        }

        var taken = _changes.ToArray();

        _changes.Clear();

        return taken;
    }
}
