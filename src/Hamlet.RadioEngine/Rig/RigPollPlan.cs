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
        // nothing to report. Swept, it means what everything else's means, which
        // is part of why the sweep is the clean answer rather than a special
        // staleness rule.
        RigField.Frequency => RigPollRate.Session,

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

        // Nothing here changes without somebody's hand on the radio.
        RigField.Vfo => RigPollRate.Never,

        _ => RigPollRate.Session,
    };

    /// <summary>How long a reading of this field stays current.</summary>
    /// <param name="field">The field.</param>
    /// <returns>The window past which the UI must say it is stale.</returns>
    public static TimeSpan FreshFor(RigField field)
        => RateFor(field) == RigPollRate.Live ? LiveFreshFor : SessionFreshFor;

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
