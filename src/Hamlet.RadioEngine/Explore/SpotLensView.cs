namespace Hamlet.RadioEngine.Explore;

/// <summary>Which of the two questions the happening-now list is answering.</summary>
/// <remarks>
/// TWO QUESTIONS, AND THEY ARE NOT THE SAME ONE (HM-DEC-057). A refresh button
/// answers neither, because it conflates "show me the good ones" with "show me
/// the fresh ones" and the answer to those is different on almost every band.
/// </remarks>
public enum SpotLens
{
    /// <summary>
    /// The arrival question: where is my best shot, over everything alive.
    /// </summary>
    BestChance,

    /// <summary>
    /// The between-contacts question: what has turned up since I last looked.
    /// </summary>
    WhatsNew,
}

/// <summary>
/// What the operator has already done about the spots.
/// </summary>
/// <param name="LastLookedUtc">
/// When they last finished looking at "what's new". Null when they never have.
/// </param>
/// <param name="ActedOn">
/// Keys of spots they tuned to. Never re-offered under "what's new", because
/// somebody who has already been there does not need telling about it again.
/// </param>
/// <remarks>
/// A VALUE, NOT A STORE. The whole point of keeping this as a plain record is
/// that "what's new" becomes a pure function of the history and this, which
/// makes the case that matters testable: the operator who worked a station and
/// came back a minute later.
/// </remarks>
public sealed record SpotAttention(DateTime? LastLookedUtc, IReadOnlySet<string> ActedOn)
{
    /// <summary>Somebody who has never looked and never worked anybody.</summary>
    public static SpotAttention Fresh { get; } =
        new(null, new HashSet<string>(StringComparer.OrdinalIgnoreCase));
}

/// <summary>One spot as a lens presents it.</summary>
/// <param name="Spot">The spot.</param>
/// <param name="FirstSeenUtc">When Hamlet first recorded it.</param>
/// <param name="Liveness">
/// How much of this spot's ruled lifetime is left, 1 down to 0 (HM-DEC-045).
/// </param>
/// <param name="Prominence">
/// How strongly to draw it, 1 down to a floor. The eye finds what is current
/// without anybody reading a timestamp.
/// </param>
/// <param name="IsUnseen">
/// True when this arrived after the operator last looked and they have not
/// acted on it.
/// </param>
public sealed record LensedSpot(
    ActivitySpot Spot,
    DateTime FirstSeenUtc,
    double Liveness,
    double Prominence,
    bool IsUnseen);

/// <summary>
/// The two lenses over the spot history (HM-DEC-057).
/// </summary>
/// <remarks>
/// <para>NOTHING IS DELETED, EVER. This is a view over the store, which is
/// exactly what HM-DEC-045 built the store for. Every method here filters and
/// orders what it is given and removes nothing from anywhere, so a hard refresh
/// cannot re-create the failure that ruling ended: throwing away good
/// invitations at ten minutes and then saying "nothing here" while holding
/// them.</para>
/// <para>ONE CLOCK. <see cref="Liveness"/> is the single measure of whether
/// somebody is probably still there, and both the display fade and the
/// workability rank read it (HM-DEC-058). Before this there were two: the fade
/// followed each source's ruled lifetime and the rank had its own buckets in
/// minutes, so a spot could look faded and rank fresh on the same screen.</para>
/// <para>Pure: spots, attention and a moment in, a view out. No clock read
/// here, so every threshold is testable exactly (§5).</para>
/// </remarks>
public static class SpotLensView
{
    /// <summary>
    /// How dim a spot at the very end of its lifetime is still drawn.
    /// </summary>
    /// <remarks>
    /// Never zero. A card faded to nothing is a card removed, and removing one
    /// is what this whole design exists not to do. It is also a readability
    /// floor: the text on a card still has to clear its background (§0.6).
    /// </remarks>
    public const double FadeFloor = 0.45;

    /// <summary>
    /// The floor under "best chance", where the fade is a softer signal.
    /// </summary>
    /// <remarks>
    /// Liveness is one input to the rank rather than the whole answer there, so
    /// an old park activation that still ranks high is still drawn plainly. A
    /// card that argued with its own position would be the screen contradicting
    /// itself.
    /// </remarks>
    public const double SoftFadeFloor = 0.75;

    /// <summary>
    /// How recently the operator has to have looked for "what's new" to be the
    /// question they are probably asking.
    /// </summary>
    /// <remarks>
    /// Twenty minutes, which is the skimmer lifetime, and picked for the same
    /// reason: past it, what was on the air when they last looked has stopped
    /// being a useful delta and they are arriving rather than returning.
    /// </remarks>
    public static readonly TimeSpan RecentLook = TimeSpan.FromMinutes(20);

    /// <summary>What the control calls this lens.</summary>
    /// <param name="lens">The lens.</param>
    /// <returns>Two or three words.</returns>
    public static string Name(SpotLens lens)
        => lens == SpotLens.WhatsNew ? "What's new" : "Best chance";

    /// <summary>
    /// The question this lens answers, for the control's tooltip.
    /// </summary>
    /// <param name="lens">The lens.</param>
    /// <returns>One sentence.</returns>
    /// <remarks>
    /// TWO WORDS ON SCREEN TEACH MORE THAN ANY INFERENCE (HM-DEC-057). A
    /// newcomer who reads that hunting again after a contact is a normal thing
    /// people do has learned something about this hobby that nobody tells them,
    /// and that is worth the control by itself.
    /// </remarks>
    public static string Question(SpotLens lens)
        => lens == SpotLens.WhatsNew
            ? "What has turned up since you last looked. This is the one to use "
              + "after a contact, because going hunting again straight away is "
              + "what everybody does."
            : "Your best shot right now, over everything still going on. Somebody "
              + "who has been standing in a park for an hour is still a fine "
              + "contact, so age is not the whole of it.";

    /// <summary>
    /// How much of a spot's ruled lifetime is left, 1 down to 0.
    /// </summary>
    /// <param name="spot">The spot.</param>
    /// <param name="age">How long since it was reported.</param>
    /// <param name="lifetimes">The configured source lifetimes.</param>
    /// <returns>1 at the moment it was heard, 0 once its lifetime is spent.</returns>
    /// <remarks>
    /// The lifetime is the source's own (HM-DEC-045), so an hour-old park
    /// activation and an hour-old skimmer report are not the same number, which
    /// is the whole point: an activator is still standing there and a skimmer
    /// only ever said somebody called once.
    /// </remarks>
    public static double Liveness(
        ActivitySpot spot, TimeSpan age, SpotLifetimeSettings? lifetimes = null)
    {
        ArgumentNullException.ThrowIfNull(spot);

        var lifetime = SpotLifetime.For(spot, lifetimes).TotalMinutes;
        if (lifetime <= 0)
        {
            return 0;
        }

        var spent = Math.Max(0, age.TotalMinutes) / lifetime;
        return Math.Clamp(1.0 - spent, 0.0, 1.0);
    }

    /// <summary>How strongly to draw a spot under a lens.</summary>
    /// <param name="lens">The lens.</param>
    /// <param name="liveness">How much lifetime is left.</param>
    /// <returns>1 down to the lens's floor.</returns>
    public static double Prominence(SpotLens lens, double liveness)
    {
        var floor = lens == SpotLens.WhatsNew ? FadeFloor : SoftFadeFloor;
        return floor + ((1.0 - floor) * Math.Clamp(liveness, 0.0, 1.0));
    }

    /// <summary>
    /// Everything a lens shows, newest first.
    /// </summary>
    /// <param name="lens">Which question is being asked.</param>
    /// <param name="history">Everything the store holds.</param>
    /// <param name="attention">What the operator has already done.</param>
    /// <param name="nowUtc">Reference time.</param>
    /// <param name="lifetimes">The configured source lifetimes.</param>
    /// <returns>The spots this lens shows, with their fade.</returns>
    /// <remarks>
    /// Both lenses start from the same live set. "What's new" then keeps only
    /// what arrived after the operator last looked and that they have not
    /// already tuned to, which is the delta the ruling asks for. It removes
    /// nothing from the history and the other lens still shows all of it.
    /// </remarks>
    public static IReadOnlyList<LensedSpot> Apply(
        SpotLens lens,
        IEnumerable<StoredSpot> history,
        SpotAttention attention,
        DateTime nowUtc,
        SpotLifetimeSettings? lifetimes = null)
    {
        ArgumentNullException.ThrowIfNull(history);
        ArgumentNullException.ThrowIfNull(attention);

        var shown = new List<LensedSpot>();

        foreach (var stored in history)
        {
            if (!SpotLifetime.IsLive(stored.Spot, nowUtc, lifetimes))
            {
                continue;
            }

            var unseen = IsUnseen(stored, attention);

            if (lens == SpotLens.WhatsNew && !unseen)
            {
                continue;
            }

            var liveness = Liveness(stored.Spot, nowUtc - stored.Spot.HeardAtUtc, lifetimes);

            shown.Add(new LensedSpot(
                stored.Spot,
                stored.FirstSeenUtc,
                liveness,
                Prominence(lens, liveness),
                unseen));
        }

        shown.Sort((a, b) => b.Spot.HeardAtUtc.CompareTo(a.Spot.HeardAtUtc));
        return shown;
    }

    /// <summary>
    /// True when this arrived after the operator last looked and they have not
    /// acted on it.
    /// </summary>
    /// <param name="stored">The spot as the store holds it.</param>
    /// <param name="attention">What the operator has already done.</param>
    /// <returns>True when it is genuinely new to them.</returns>
    /// <remarks>
    /// FIRST SEEN, NEVER LAST SEEN. A station spotted again twenty minutes later
    /// did not start calling twenty minutes later, and treating a re-sighting as
    /// an arrival is the "presented as if it just arrived" failure HM-DEC-045
    /// forbids, arriving by a different door.
    /// </remarks>
    public static bool IsUnseen(StoredSpot stored, SpotAttention attention)
    {
        ArgumentNullException.ThrowIfNull(stored);
        ArgumentNullException.ThrowIfNull(attention);

        // Two records of the same fact, and both are consulted. The store's own
        // mark survives a restart; the set covers what the operator did in this
        // session before the next write reached the disk.
        if (stored.ActedOnUtc is not null
            || attention.ActedOn.Contains(SpotIdentity.KeyFor(stored.Spot)))
        {
            return false;
        }

        return attention.LastLookedUtc is not { } looked || stored.FirstSeenUtc > looked;
    }

    /// <summary>
    /// Which lens to open on, when the operator has not chosen one.
    /// </summary>
    /// <param name="lastLookedUtc">When they last finished with "what's new".</param>
    /// <param name="nowUtc">Reference time.</param>
    /// <param name="unseenCount">How many live spots they have not seen.</param>
    /// <returns>The lens to open on.</returns>
    /// <remarks>
    /// <para>INFERENCE MAY CHOOSE WHICH LENS OPENS AND MAY NEVER OVERRIDE THE
    /// OPERATOR AFTERWARD (HM-DEC-057). Guessing which question somebody is
    /// asking is a reasonable thing to do once. Guessing again, after they have
    /// answered it by clicking, is the app arguing with them, so the caller only
    /// asks this when there is no stored choice.</para>
    /// <para>It guesses "what's new" from two facts and not from a hunch: they
    /// were here within the last twenty minutes, so this is a return rather than
    /// an arrival, and something has actually turned up since. Absent either,
    /// it opens on "best chance" and leaves it.</para>
    /// </remarks>
    public static SpotLens OpeningLens(
        DateTime? lastLookedUtc, DateTime nowUtc, int unseenCount)
        => lastLookedUtc is { } looked
           && nowUtc - looked <= RecentLook
           && nowUtc >= looked
           && unseenCount > 0
            ? SpotLens.WhatsNew
            : SpotLens.BestChance;

    /// <summary>
    /// What a shut panel says about itself.
    /// </summary>
    /// <param name="lens">The active lens.</param>
    /// <param name="shown">How many cards it is showing.</param>
    /// <returns>The lens and the count, e.g. "Best chance · 7 spots".</returns>
    /// <remarks>
    /// A COLLAPSED PANEL STILL CARRIES ITS SUMMARY (§0.5), and it has to name
    /// the lens. A shut panel that has silently changed which question it is
    /// answering is the prime directive broken by omission: the operator would
    /// read a count and take it for a count of everything.
    /// </remarks>
    public static string Summary(SpotLens lens, int shown)
    {
        var what = shown switch
        {
            0 when lens == SpotLens.WhatsNew => "nothing new yet",
            0 => "nothing on this band",
            1 => "1 spot",
            _ => $"{shown} spots",
        };

        return $"{Name(lens)} · {what}";
    }
}
