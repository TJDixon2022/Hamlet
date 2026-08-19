namespace Hamlet.RadioEngine.Rig;

/// <summary>How often a field is worth asking about.</summary>
/// <remarks>
/// CI-V IS A SLOW SERIAL BUS SHARED WITH THE TRANSCEIVE STREAM, and hammering
/// it makes the radio sluggish and the app unreliable. So nothing is polled at
/// one rate: the S-meter moves constantly and everything else barely moves at
/// all, and treating them alike would either waste the bus or leave the meter
/// crawling.
/// </remarks>
public enum RigPollRate
{
    /// <summary>
    /// Never polled. Either the radio volunteers it, or nothing does.
    /// </summary>
    /// <remarks>
    /// The VFO selection is here. **The frequency used to be and is not any
    /// more** (HM-DEC-109): the radio does broadcast a change, and a broadcast
    /// missed while the app was starting left the model holding a frequency the
    /// radio was not on with nothing to correct it.
    /// </remarks>
    Never,

    /// <summary>
    /// A few times a second, and only while something is showing it.
    /// </summary>
    Live,

    /// <summary>
    /// Once on connect and then occasionally, because it changes when somebody
    /// reaches over and changes it.
    /// </summary>
    Session,

    /// <summary>Only when somebody asks, such as opening the diagnostics screen.</summary>
    OnDemand,
}

/// <summary>
/// Which fields are polled, how often, and how long a reading of each kind
/// stays current.
/// </summary>
/// <remarks>
/// <para>The plan is data rather than code so the whole polling policy can be
/// read in one screen and argued with. Getting it wrong is not a crash, it is a
/// radio that feels sluggish and an app that seems flaky, which is the hardest
/// kind of defect to attribute.</para>
/// <para>THE STALENESS WINDOW IS NOT THE POLL INTERVAL. A reading is stale when
/// it is old enough that showing it as current would be a claim about now that
/// is really a claim about then. For the S-meter that is about a second; for
/// the AGC setting it is minutes, because nobody changes their AGC twice a
/// minute. The window is deliberately a few times the interval, so an ordinary
/// missed poll does not make the screen flicker between fresh and stale.</para>
/// </remarks>
public static class RigPollPlan
{
    /// <summary>How often a live value is asked for.</summary>
    /// <remarks>
    /// Four times a second. Fast enough that the meter moves the way the
    /// radio's own does, slow enough to leave the bus alone: at 19200 baud a
    /// short read and its reply is under two milliseconds of wire time, so this
    /// is well under one per cent of the bus.
    /// </remarks>
    public static TimeSpan LiveInterval { get; } = TimeSpan.FromMilliseconds(250);

    /// <summary>How often the settings are swept again.</summary>
    /// <remarks>
    /// Half a minute. These change when somebody reaches over and turns a knob,
    /// which is rare, and the sweep is two dozen commands so it is not something
    /// to do often. Anything that needs to be current sooner than this is asked
    /// for on demand.
    /// </remarks>
    public static TimeSpan SessionInterval { get; } = TimeSpan.FromSeconds(30);

    /// <summary>How long a live reading stays current.</summary>
    public static TimeSpan LiveFreshFor { get; } = TimeSpan.FromSeconds(1.5);

    /// <summary>How long a setting stays current.</summary>
    public static TimeSpan SessionFreshFor { get; } = TimeSpan.FromMinutes(2);

    /// <summary>How often a field is worth asking about.</summary>
    /// <param name="field">The field.</param>
    /// <returns>Its rate.</returns>
    public static RigPollRate RateFor(RigField field) => field switch
    {
        // **SWEPT WITH THE MODE AND THE FILTER, FOR THE REASON ALREADY WRITTEN
        // BESIDE THEM** (HM-DEC-109, amending HM-DEC-050 for a third field).
        // That ruling says nothing the radio volunteers is polled for, and the
        // frequency was the last field it still covered. A broadcast missed
        // while the app is starting leaves the model holding a frequency the
        // radio is not on, with nothing to correct it until the dial is next
        // turned, which is exactly why Mode and FilterSelection below are swept
        // despite being broadcast too.
        //
        // It is worth more than a tidier diagnostics screen. The band on screen
        // derives from this reading, and the band scopes what RBN is filtered to
        // and what the skimmer watch listens for (HM-DEC-024, HM-DEC-075), so a
        // wrong one makes "nobody heard you" a defect wearing the clothes of an
        // answer.
        //
        // Its age also used to mean something different from every other
        // field's: with nobody touching the dial the last broadcast receded
        // without limit, reading as a link going quiet when it was a link with
        // nothing to report. Polled, it means what everything else's means.
        //
        // **AND SESSION CADENCE WAS THE WRONG ONE, MEASURED ON THE OPERATOR'S OWN
        // RADIO.** He turned the dial by hand and Hamlet took thirty seconds to
        // follow, repeatedly, because the sweep runs every thirty seconds and the
        // broadcast that was supposed to make it instant is not arriving. The
        // screen repaints four times a second holding a value a minute old, which
        // is the display being current about a number that is not (§0.0).
        //
        // A frequency read is six bytes out and eleven back. Four times a second
        // that is under seventy bytes on a cable carrying eleven thousand, so
        // this is not what HM-DEC-050 was rationing the bus against: a frequency
        // a minute old is the failure that ruling exists to prevent rather than
        // an acceptable cost of it. And it is asked for only when the radio is
        // not announcing, per SkipLiveRead below.
        RigField.Frequency => RigPollRate.Live,

        // The needle, and whether the radio is keying. Both move constantly and
        // both are on screen while receiving.
        RigField.SMeter => RigPollRate.Live,
        RigField.TransmitStatus => RigPollRate.Live,

        // ONLY WORTH ASKING WHILE KEYING (HM-DEC-081). SWR comes from reflected
        // power, so a resting radio has nothing to measure and the answer is not
        // a reading of now. It is polled live so a send has fresh numbers, and
        // what makes it honest is that it is marked unknown the moment the
        // transmitter stops rather than being left to look current.
        RigField.Swr => RigPollRate.Live,
        RigField.PowerOut => RigPollRate.Live,
        RigField.SquelchStatus => RigPollRate.Live,
        RigField.Overflow => RigPollRate.Live,

        // Mode and filter arrive by broadcast when the operator changes them,
        // but they are swept anyway: a broadcast missed while the app was
        // starting would otherwise leave the badge wrong until the next change.
        RigField.Mode => RigPollRate.Session,
        RigField.FilterSelection => RigPollRate.Session,
        RigField.FilterBandwidth => RigPollRate.Session,

        // Read on connect and when somebody opens the diagnostics screen, and
        // not in the loop: the mode itself is broadcast as it changes, and the
        // one command that carries the data flag carries the mode with it, so
        // sweeping for it would ask the same question twice a minute.
        RigField.DataMode => RigPollRate.OnDemand,

        // The scope stream proves these far better than a command would, and it
        // shares the bus with everything else, so they are read on connect and
        // when somebody asks and at no other time (HM-DEC-050, HM-DEC-062).
        RigField.ScopeOn => RigPollRate.OnDemand,
        RigField.ScopeOutput => RigPollRate.OnDemand,

        // **ON CONNECT AND WHEN SOMEBODY ASKS, WHICH IS ENOUGH FOR A SETTING
        // NOBODY CHANGES MID-EVENING** (HM-OPEN-043). Whether the radio announces
        // its own changes decides whether the dial is followed in a tenth of a
        // second or by asking four times a second, and Hamlet has to know which
        // world it is in before it can say so. It never changes without a hand on
        // the radio, so the connect read is the one that matters.
        RigField.CivTransceive => RigPollRate.OnDemand,

        // Nothing here changes without somebody's hand on the radio.
        RigField.Vfo => RigPollRate.Never,

        _ => RigPollRate.Session,
    };

    /// <summary>How long a reading of this field stays current.</summary>
    /// <param name="field">The field.</param>
    /// <returns>The window past which the UI must say it is stale.</returns>
    public static TimeSpan FreshFor(RigField field)
        => RateFor(field) == RigPollRate.Live ? LiveFreshFor : SessionFreshFor;

    /// <summary>
    /// How recently the radio must have announced a field for Hamlet to stay
    /// quiet about it.
    /// </summary>
    /// <remarks>
    /// A second and a half, which is <see cref="LiveFreshFor"/> and deliberately
    /// the same number: the window in which a reading is still current is exactly
    /// the window in which asking again would be waste. Six live intervals, so an
    /// ordinary dropped frame does not start a poll.
    /// </remarks>
    public static TimeSpan BroadcastCoversFor => LiveFreshFor;

    /// <summary>
    /// Whether a live read can be skipped because the radio just volunteered it.
    /// </summary>
    /// <param name="field">The field about to be asked for.</param>
    /// <param name="current">What the model holds, from
    /// <see cref="RigState"/>.</param>
    /// <param name="nowUtc">The moment.</param>
    /// <returns>True to stay quiet.</returns>
    /// <remarks>
    /// <para>**THE RADIO ANNOUNCING IS BETTER THAN HAMLET ASKING, AND SILENCE IS
    /// NOT A REASON TO WAIT** (HM-DEC-050, HM-DEC-109). Where transceive is on,
    /// the dial's own pushes keep this value younger than the window and nothing
    /// goes on the bus at all — the behavior that ruling wanted. Where it is off,
    /// or the announcements stop, the poll takes over within a second and a half
    /// and the screen follows the dial four times a second.</para>
    /// <para>It is deliberately not a switch or a setting. Hamlet cannot ask the
    /// operator whether his radio announces, and a preference would be a way to
    /// configure the app into the failure it just spent two builds in.</para>
    /// </remarks>
    public static bool SkipLiveRead(RigField field, RigValue? current, DateTime nowUtc)
    {
        if (field != RigField.Frequency || current is null || !current.IsBroadcast)
        {
            return false;
        }

        return current.Age(nowUtc) is { } age && age < BroadcastCoversFor;
    }

    /// <summary>The fields polled at a given rate, in read order.</summary>
    /// <param name="rate">The rate.</param>
    /// <returns>The fields.</returns>
    /// <remarks>
    /// The filter selection is deliberately absent from the session sweep even
    /// though it is a session field: reading the mode answers it too, and asking
    /// separately would double the traffic for the same two bytes.
    /// </remarks>
    public static IReadOnlyList<RigField> At(RigPollRate rate)
        => Enum.GetValues<RigField>()
            .Where(f => RateFor(f) == rate)
            .Where(f => f != RigField.FilterSelection)
            .ToList();
}
