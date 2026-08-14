namespace Hamlet.RadioEngine.Explore;

/// <summary>
/// How long each kind of spot stays a live invitation, and how to say its age
/// out loud.
/// </summary>
/// <remarks>
/// <para>THE HONEST UNIT IS NOT "WHEN WAS THIS POSTED" (HM-DEC-045). The
/// question an operator is actually asking is whether that person is probably
/// still on that frequency, and the answer genuinely differs by source. Hamlet
/// already knows the source of every spot, so it can answer the real question
/// instead of the easy one.</para>
/// <para>A park or summit activator hauled gear somewhere on purpose and stays
/// put working whoever calls, often for well over an hour, so a spot from
/// twenty minutes ago is very likely still good. A skimmer report means
/// somebody called CQ at that moment, which is much weaker evidence they are
/// still calling now. Contest stations sit on one frequency for the whole
/// event and outlast both.</para>
/// <para>THE LIKELIHOOD LANGUAGE TRACKS THE SOURCE, never a flat rule. "A park
/// activator spotted twenty minutes ago is probably still working the pileup"
/// is defensible because that is what activators do. The same sentence about a
/// skimmer report is not, so it is never written (§0.0).</para>
/// <para>Pure: a source, a kind and an elapsed time in, a phrase out. No clock
/// (§5).</para>
/// </remarks>
public static class SpotLifetime
{
    /// <summary>How long an activation spot stays a live invitation.</summary>
    public static readonly TimeSpan ActivationDefault = TimeSpan.FromMinutes(60);

    /// <summary>How long a skimmer report stays a live invitation.</summary>
    public static readonly TimeSpan SkimmerDefault = TimeSpan.FromMinutes(20);

    /// <summary>How long contest activity stays a live invitation.</summary>
    public static readonly TimeSpan ContestDefault = TimeSpan.FromMinutes(180);

    /// <summary>The fallback for a source with no rule of its own.</summary>
    public static readonly TimeSpan UnknownDefault = TimeSpan.FromMinutes(30);

    /// <summary>
    /// The lifetime for one spot, given the operator's configured values.
    /// </summary>
    /// <param name="spot">The spot.</param>
    /// <param name="settings">The configured lifetimes.</param>
    /// <returns>How long this spot stays a live invitation.</returns>
    public static TimeSpan For(ActivitySpot spot, SpotLifetimeSettings? settings = null)
    {
        var s = settings ?? SpotLifetimeSettings.Defaults;

        // Contest first: a contest run outlasts everything, and the app only
        // claims it where the source actually said so rather than guessing
        // from a busy band.
        if (spot.CallType == SpotCallType.Contest)
        {
            return s.Contest;
        }

        // An activation is the source stating that somebody carried a radio
        // somewhere to be found. That is a fact about intent, not a guess.
        if (spot.IsActivation)
        {
            return s.Activation;
        }

        return IsSkimmer(spot) ? s.Skimmer : s.Unknown;
    }

    /// <summary>True when a spot is still inside its lifetime.</summary>
    /// <param name="spot">The spot.</param>
    /// <param name="nowUtc">The moment to judge against.</param>
    /// <param name="settings">The configured lifetimes.</param>
    /// <returns>True when the spot is still a live invitation.</returns>
    public static bool IsLive(
        ActivitySpot spot, DateTime nowUtc, SpotLifetimeSettings? settings = null)
        => nowUtc - spot.HeardAtUtc <= For(spot, settings);

    /// <summary>
    /// How old a spot is, in the words a person would use.
    /// </summary>
    /// <param name="elapsed">How long since it was reported.</param>
    /// <returns>e.g. "a few minutes ago", "about twenty minutes back".</returns>
    /// <remarks>
    /// Nobody says "17 min ago" out loud. The exact figure stays available on
    /// hover for anybody who wants it (HM-DEC-045); this is what the card
    /// reads like at a glance (§0.7).
    /// </remarks>
    public static string DescribeAge(TimeSpan elapsed)
    {
        var minutes = Math.Max(0, elapsed.TotalMinutes);

        return minutes switch
        {
            < 2 => "just now",
            < 6 => "a few minutes ago",
            < 12 => "about ten minutes ago",
            < 25 => "about twenty minutes back",
            < 40 => "half an hour ago now",
            < 75 => "about an hour ago",
            < 150 => "a couple of hours back",
            < 360 => "earlier today",
            _ => "a while ago",
        };
    }

    /// <summary>
    /// The age and, where the source can support one, what it means for
    /// whether that person is still there.
    /// </summary>
    /// <param name="spot">The spot.</param>
    /// <param name="elapsed">How long since it was reported.</param>
    /// <param name="settings">The configured lifetimes.</param>
    /// <returns>One phrase for the card.</returns>
    /// <remarks>
    /// The claim half is only ever attached where the source justifies it. An
    /// activation may say somebody is probably still working callers, because
    /// activators stay put. A skimmer report says only what it saw.
    /// </remarks>
    public static string DescribeOpportunity(
        ActivitySpot spot, TimeSpan elapsed, SpotLifetimeSettings? settings = null)
    {
        var age = DescribeAge(elapsed);
        var lifetime = For(spot, settings);
        var minutes = Math.Max(0, elapsed.TotalMinutes);

        // Inside a couple of minutes nothing needs adding, whatever the source.
        if (minutes < 2)
        {
            return age;
        }

        var fraction = lifetime.TotalMinutes <= 0 ? 1.0 : minutes / lifetime.TotalMinutes;

        if (spot.CallType == SpotCallType.Beacon)
        {
            // A beacon is a machine and will be there all night. That is not
            // encouragement, it is the reason not to bother.
            return $"{age}, and it will still be there, because nobody is listening";
        }

        if (spot.CallType == SpotCallType.Contest)
        {
            return fraction < 1.0
                ? $"{age}, and contest stations stay put, so they are very likely still there"
                : $"{age}, which is a while even for a contest run";
        }

        if (spot.IsActivation)
        {
            return fraction switch
            {
                < 0.4 => $"{age}, and activators stay a while, so they are probably still there",
                < 0.8 => $"{age}, which is well within a normal activation",
                < 1.0 => $"{age}, so they may be packing up",
                _ => $"{age}, so they have most likely finished",
            };
        }

        if (IsSkimmer(spot))
        {
            // What a skimmer saw is a fact. Whether the operator is still
            // calling is not, and no phrasing here may pretend otherwise.
            return fraction switch
            {
                < 0.5 => $"heard {age}, though a skimmer only says somebody called then",
                < 1.0 => $"heard {age}, so they may have moved on",
                _ => $"heard {age}, and that is old for a skimmer report",
            };
        }

        return fraction < 1.0 ? $"{age}" : $"{age}, which is getting old";
    }

    /// <summary>
    /// True when a spot came from an unattended receiver rather than a person.
    /// </summary>
    /// <param name="spot">The spot.</param>
    /// <returns>True for a skimmer report.</returns>
    /// <remarks>
    /// The Reverse Beacon Network is the only skimmer source Hamlet has, and
    /// it is named rather than inferred from the shape of the record. A source
    /// added later declares itself here rather than being guessed at.
    /// </remarks>
    public static bool IsSkimmer(ActivitySpot spot)
        => string.Equals(spot.Source, "RBN", StringComparison.OrdinalIgnoreCase);
}

/// <summary>
/// The lifetimes, as the operator can set them.
/// </summary>
/// <param name="Activation">How long a park or summit spot stays live.</param>
/// <param name="Skimmer">How long a skimmer report stays live.</param>
/// <param name="Contest">How long contest activity stays live.</param>
/// <param name="Unknown">The fallback for anything else.</param>
/// <remarks>
/// Generous defaults, and settable, because the right answer depends on how
/// somebody operates. Ten minutes was never a considered figure; it was the
/// window the band-conditions line happened to use, applied to a question it
/// was not asked (HM-DEC-045).
/// </remarks>
public sealed record SpotLifetimeSettings(
    TimeSpan Activation,
    TimeSpan Skimmer,
    TimeSpan Contest,
    TimeSpan Unknown)
{
    /// <summary>The shipped values.</summary>
    public static SpotLifetimeSettings Defaults { get; } = new(
        SpotLifetime.ActivationDefault,
        SpotLifetime.SkimmerDefault,
        SpotLifetime.ContestDefault,
        SpotLifetime.UnknownDefault);

    /// <summary>
    /// Build from minute counts, falling back to the default where a value is
    /// missing or absurd.
    /// </summary>
    /// <param name="activationMinutes">Activation lifetime.</param>
    /// <param name="skimmerMinutes">Skimmer lifetime.</param>
    /// <param name="contestMinutes">Contest lifetime.</param>
    /// <returns>The settings.</returns>
    /// <remarks>
    /// A zero or a negative would empty the panel permanently and look exactly
    /// like a broken feed, so it is refused rather than obeyed. The ceiling is
    /// a day, past which "still there" stops meaning anything.
    /// </remarks>
    public static SpotLifetimeSettings FromMinutes(
        int activationMinutes, int skimmerMinutes, int contestMinutes)
        => new(
            Sane(activationMinutes, SpotLifetime.ActivationDefault),
            Sane(skimmerMinutes, SpotLifetime.SkimmerDefault),
            Sane(contestMinutes, SpotLifetime.ContestDefault),
            SpotLifetime.UnknownDefault);

    /// <summary>The longest of the lifetimes, which bounds how far back to look.</summary>
    public TimeSpan Longest
    {
        get
        {
            var longest = Activation;
            if (Skimmer > longest)
            {
                longest = Skimmer;
            }

            if (Contest > longest)
            {
                longest = Contest;
            }

            return Unknown > longest ? Unknown : longest;
        }
    }

    private static TimeSpan Sane(int minutes, TimeSpan fallback)
        => minutes is > 0 and <= 1440 ? TimeSpan.FromMinutes(minutes) : fallback;
}
