namespace Hamlet.RadioEngine.Explore;

/// <summary>One chip: a family, whether it is on, and how many there are.</summary>
/// <param name="Family">Which family.</param>
/// <param name="Label">What the chip says.</param>
/// <param name="IsOn">Whether spots of this family are shown.</param>
/// <param name="Count">
/// How many live spots there are of it, whether or not it is on.
/// </param>
public sealed record FamilyChip(ModeFamily Family, string Label, bool IsOn, int Count);

/// <summary>
/// Filtering the happening-now list by mode family (HM-DEC-061).
/// </summary>
/// <remarks>
/// <para>THE COUNT SHOWS EVEN WHEN THE FAMILY IS SWITCHED OFF, and that is the
/// teaching part rather than a detail. Somebody who filters to Morse and still
/// sees forty-one voice stations learns that the band is full of people they
/// could talk to, which is the fact this whole app exists to reveal. A
/// filtered-out family that went silent would teach the opposite: that turning
/// something off makes it stop existing.</para>
/// <para>THEY FILTER AND THEY NEVER DELETE. This is one more view over the same
/// store the lenses read (HM-DEC-045, HM-DEC-057), so a chip changes what is on
/// screen and changes nothing about what Hamlet holds. It composes with the
/// lenses rather than fighting them: the lens decides what is in play and the
/// chips decide which families of it are drawn.</para>
/// <para>Pure: spots and a set of families in, a filtered list and the counts
/// out. No clock, no store (§5).</para>
/// </remarks>
public static class FamilyFilter
{
    /// <summary>The three families the chips offer, in order.</summary>
    /// <remarks>
    /// Three rather than four. Open is not a family anybody tunes for: it is the
    /// space between the families, so a chip for it would be a chip for
    /// "whatever is left" and nobody wants that filter. Spots whose mode nothing
    /// recognizes are shown whenever any chip is on, rather than being hidden by
    /// a control that does not name them.
    /// </remarks>
    public static IReadOnlyList<ModeFamily> Offered { get; } = new[]
    {
        ModeFamily.Cw, ModeFamily.Digital, ModeFamily.Phone,
    };

    /// <summary>Everything on, which is where a fresh profile starts.</summary>
    public static IReadOnlySet<ModeFamily> All { get; } =
        new HashSet<ModeFamily>(Offered);

    /// <summary>What a chip is called.</summary>
    /// <param name="family">The family.</param>
    /// <returns>Its label, matching the map's legend word for word (§0.6).</returns>
    public static string Label(ModeFamily family) => family switch
    {
        ModeFamily.Cw => "Morse",
        ModeFamily.Digital => "Digital",
        ModeFamily.Phone => "Voice",
        ModeFamily.OutsideTheBand => "Not a ham band",
        _ => "Open",
    };

    /// <summary>The chips, with their counts, for a set of spots.</summary>
    /// <param name="spots">Everything the lens is showing before filtering.</param>
    /// <param name="on">Which families are switched on.</param>
    /// <returns>One chip per offered family.</returns>
    /// <remarks>
    /// The count is over everything the lens has, not over what survives the
    /// filter, which is exactly the point: a chip that read zero because it was
    /// switched off would be telling the operator there is nothing there.
    /// </remarks>
    public static IReadOnlyList<FamilyChip> Chips(
        IEnumerable<ActivitySpot> spots, IReadOnlySet<ModeFamily> on)
    {
        ArgumentNullException.ThrowIfNull(spots);
        ArgumentNullException.ThrowIfNull(on);

        var counts = new Dictionary<ModeFamily, int>();

        foreach (var spot in spots)
        {
            var family = ModeGuide.FamilyFor(spot.Mode);
            counts[family] = counts.GetValueOrDefault(family) + 1;
        }

        return Offered
            .Select(f => new FamilyChip(
                f, Label(f), on.Contains(f), counts.GetValueOrDefault(f)))
            .ToList();
    }

    /// <summary>Keep only the spots whose family is switched on.</summary>
    /// <param name="spots">What the lens is showing.</param>
    /// <param name="on">Which families are switched on.</param>
    /// <returns>The ones to draw.</returns>
    /// <remarks>
    /// EVERY CHIP OFF SHOWS EVERYTHING, rather than an empty panel. Somebody who
    /// has switched all three off has not asked to see nothing; they have
    /// wandered into a state with no meaning, and an app that answered with a
    /// blank panel would look broken (§0.7).
    /// </remarks>
    public static IReadOnlyList<ActivitySpot> Apply(
        IEnumerable<ActivitySpot> spots, IReadOnlySet<ModeFamily> on)
    {
        ArgumentNullException.ThrowIfNull(spots);
        ArgumentNullException.ThrowIfNull(on);

        var list = spots.ToList();

        if (on.Count == 0 || Offered.All(on.Contains))
        {
            return list;
        }

        return list
            .Where(s =>
            {
                var family = ModeGuide.FamilyFor(s.Mode);

                // A mode nothing recognizes is not hidden by a control that does
                // not name it.
                return !Offered.Contains(family) || on.Contains(family);
            })
            .ToList();
    }

    /// <summary>
    /// What a shut panel says about the filtering (§0.5).
    /// </summary>
    /// <param name="on">Which families are switched on.</param>
    /// <returns>e.g. "Morse only", or "" when nothing is filtered.</returns>
    /// <remarks>
    /// A COLLAPSED PANEL NEVER HIDES THAT IT IS FILTERING. Somebody who shut the
    /// panel with two families switched off and later read a count would take it
    /// for a count of everything, which is the prime directive broken by
    /// omission.
    /// </remarks>
    public static string Summary(IReadOnlySet<ModeFamily> on)
    {
        ArgumentNullException.ThrowIfNull(on);

        if (on.Count == 0 || Offered.All(on.Contains))
        {
            return "";
        }

        var kept = Offered.Where(on.Contains).Select(Label).ToList();

        return kept.Count switch
        {
            0 => "",
            1 => $"{kept[0]} only",
            _ => string.Join(" and ", kept) + " only",
        };
    }

    /// <summary>Read a stored set of family names.</summary>
    /// <param name="names">What settings.json holds, or null.</param>
    /// <returns>The families, or all of them when nothing was stored.</returns>
    public static IReadOnlySet<ModeFamily> Parse(IEnumerable<string>? names)
    {
        if (names is null)
        {
            return All;
        }

        var parsed = new HashSet<ModeFamily>();

        foreach (var name in names)
        {
            if (Enum.TryParse<ModeFamily>(name, out var family)
                && Offered.Contains(family))
            {
                parsed.Add(family);
            }
        }

        return parsed.Count == 0 ? All : parsed;
    }
}
