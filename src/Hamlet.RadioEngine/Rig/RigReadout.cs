namespace Hamlet.RadioEngine.Rig;

/// <summary>How current a reading is.</summary>
public enum RigFreshness
{
    /// <summary>There is no reading.</summary>
    None,

    /// <summary>Read recently enough to show as what the radio is doing now.</summary>
    Fresh,

    /// <summary>
    /// Old enough that showing it as current would be a claim about now that is
    /// really a claim about then.
    /// </summary>
    Stale,
}

/// <summary>
/// One reading, ready to be shown: what it says, how old it is, and whether it
/// is still worth believing.
/// </summary>
/// <param name="Value">The reading.</param>
/// <param name="Freshness">How current it is.</param>
/// <param name="Text">
/// What the operator reads. Never a number standing in for an absent one.
/// </param>
/// <param name="AgeText">How long ago it was read, in words, or empty.</param>
public sealed record RigReadout(
    RigValue Value, RigFreshness Freshness, string Text, string AgeText)
{
    /// <summary>True when this is a real reading that is still current.</summary>
    public bool IsCurrent => Freshness == RigFreshness.Fresh;
}

/// <summary>
/// Turns what Hamlet knows into what the operator reads, and refuses to make
/// anything up on the way.
/// </summary>
/// <remarks>
/// <para>THE RULE THIS CLASS EXISTS TO HOLD (§0.0, HM-DEC-009). There is no
/// default that quietly stands in for a real reading. An unread S-meter reads
/// "unknown" and not "S0", because a needle at rest looks exactly like a
/// measurement of a quiet band, and an operator acting on it would be wrong
/// because the app was more confident than its input justified.</para>
/// <para>Four things are said differently on purpose. "Unknown" means nobody
/// has asked yet or the asking failed. "This radio does not have it" means
/// nothing is ever coming, so the screen can stop waiting (HM-DEC-030). "No
/// documented command" means the gap is in Hamlet rather than in the radio,
/// which is the one of the four somebody could go and fix. And a stale reading
/// still shows its number with its age attached, because "S7, read four minutes
/// ago" is useful and honest where a bare "S7" would be a lie about now.</para>
/// <para>Ages are spoken rather than counted (§0.7). "A moment ago" is what a
/// person says; "1.4 seconds ago" is what a stopwatch says, and nobody reading
/// a diagnostics screen at two in the morning wants the stopwatch.</para>
/// </remarks>
/// <summary>Presents a <see cref="RigValue"/> for reading.</summary>
public static class RigDisplay
{
    /// <summary>Prepare one reading for the screen.</summary>
    /// <param name="value">The reading.</param>
    /// <param name="nowUtc">Reference time.</param>
    /// <returns>What to show.</returns>
    public static RigReadout Describe(RigValue value, DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (!value.IsKnown)
        {
            return new RigReadout(value, RigFreshness.None, value.Text, "");
        }

        var freshFor = RigPollPlan.FreshFor(value.Field);
        var stale = value.IsStale(nowUtc, freshFor);
        var age = Age(value.Age(nowUtc));

        return new RigReadout(
            value,
            stale ? RigFreshness.Stale : RigFreshness.Fresh,
            value.Text,
            age);
    }

    /// <summary>
    /// How long ago, in the words a person uses.
    /// </summary>
    /// <param name="age">How old the reading is, or null.</param>
    /// <returns>e.g. "a moment ago", "about 4 minutes ago", or "".</returns>
    /// <remarks>
    /// Numbers are spoken rather than counted (§0.7). Under a couple of seconds
    /// there is no number worth saying at all, because a reading that fresh is
    /// simply what the radio is doing.
    /// </remarks>
    public static string Age(TimeSpan? age)
    {
        if (age is not { } elapsed || elapsed < TimeSpan.Zero)
        {
            return "";
        }

        if (elapsed < TimeSpan.FromSeconds(2))
        {
            return "a moment ago";
        }

        if (elapsed < TimeSpan.FromMinutes(1))
        {
            return $"{(int)elapsed.TotalSeconds} seconds ago";
        }

        if (elapsed < TimeSpan.FromMinutes(2))
        {
            return "about a minute ago";
        }

        if (elapsed < TimeSpan.FromHours(1))
        {
            return $"about {(int)elapsed.TotalMinutes} minutes ago";
        }

        return elapsed < TimeSpan.FromHours(2)
            ? "over an hour ago"
            : $"over {(int)elapsed.TotalHours} hours ago";
    }

    /// <summary>What to call a field on screen.</summary>
    /// <param name="field">The field.</param>
    /// <returns>Its label, in ordinary words rather than an identifier.</returns>
    public static string Label(RigField field) => field switch
    {
        RigField.Frequency => "Frequency",
        RigField.Mode => "Mode",
        RigField.DataMode => "Data mode",
        RigField.FilterSelection => "Filter",
        RigField.FilterBandwidth => "Filter width",
        RigField.SMeter => "S-meter",
        RigField.TransmitStatus => "Transmitting",
        RigField.Overflow => "Front end overload",
        RigField.RfPower => "Transmit power",
        RigField.RfGain => "RF gain",
        RigField.Squelch => "Squelch level",
        RigField.SquelchStatus => "Squelch",
        RigField.Agc => "AGC",
        RigField.Preamp => "Preamp",
        RigField.Attenuator => "Attenuator",
        RigField.NoiseBlanker => "Noise blanker",
        RigField.NoiseBlankerLevel => "Noise blanker level",
        RigField.NoiseReduction => "Noise reduction",
        RigField.NoiseReductionLevel => "Noise reduction level",
        RigField.AutoNotch => "Auto notch",
        RigField.ManualNotch => "Manual notch",
        RigField.BreakIn => "Break-in",
        RigField.KeyerSpeed => "Keyer speed",
        RigField.CwPitch => "CW pitch",
        RigField.AccUsbOutputSelect => "Audio to the computer",
        RigField.AccUsbAfLevel => "Audio output level",
        RigField.AccUsbSquelch => "Audio squelch gate",
        RigField.Split => "Split",
        _ => "VFO",
    };
}
