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
    /// The frequency is here. The radio broadcasts a change as the operator
    /// makes it, and asking as well would be spending bus traffic on a fact
    /// already in hand (HM-DEC-050).
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
        // The radio broadcasts these, so asking would be spending bus traffic
        // on a fact already in hand.
        RigField.Frequency => RigPollRate.Never,

        // The needle, and whether the radio is keying. Both move constantly and
        // both are on screen while receiving.
        RigField.SMeter => RigPollRate.Live,
        RigField.TransmitStatus => RigPollRate.Live,
        RigField.SquelchStatus => RigPollRate.Live,
        RigField.Overflow => RigPollRate.Live,

        // Mode and filter arrive by broadcast when the operator changes them,
        // but they are swept anyway: a broadcast missed while the app was
        // starting would otherwise leave the badge wrong until the next change.
        RigField.Mode => RigPollRate.Session,
        RigField.FilterSelection => RigPollRate.Session,
        RigField.FilterBandwidth => RigPollRate.Session,

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
