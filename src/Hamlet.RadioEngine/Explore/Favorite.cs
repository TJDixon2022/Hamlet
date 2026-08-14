using Hamlet.RadioEngine.Bands;

namespace Hamlet.RadioEngine.Explore;

/// <summary>A frequency worth coming back to, and why (HM-DEC-060).</summary>
/// <param name="FrequencyHz">Where it is.</param>
/// <param name="Name">
/// What the operator calls it. Filled in from the neighborhood when it was
/// saved, and renameable.
/// </param>
/// <param name="Mode">The mode it was saved in, or "".</param>
/// <param name="BandName">Which band, e.g. "40 m".</param>
/// <param name="Neighborhood">
/// What lives there, as the map called it when it was saved, or "".
/// </param>
/// <param name="SavedUtc">When it was saved.</param>
public sealed record Favorite(
    long FrequencyHz,
    string Name,
    string Mode,
    string BandName,
    string Neighborhood,
    DateTime SavedUtc)
{
    /// <summary>The frequency as the app writes it, e.g. "14.074".</summary>
    public string FrequencyLabel
        => (FrequencyHz / 1_000_000.0).ToString(
            "0.000", System.Globalization.CultureInfo.InvariantCulture);
}

/// <summary>
/// Saving a frequency, with the reason attached (HM-DEC-060).
/// </summary>
/// <remarks>
/// <para>THE RADIO'S OWN MEMORY CHANNELS ARE THE PROBLEM RATHER THAN THE ANSWER.
/// They are numbered slots whose meaning you have to remember, and remembering
/// what channel 7 was for is the same work as remembering the number. Hamlet
/// knows why somebody was on a frequency, because the neighborhood map already
/// says what lives there, so its favorites carry the reason.</para>
/// <para>SAVING CAPTURES CONTEXT AUTOMATICALLY. Frequency, mode, band and
/// neighborhood, so a favorite reads "14.074, where the digital modes gather"
/// with nothing typed. The operator may rename it and nobody has to.</para>
/// <para>Pure: a frequency and what the map says about it in, a favorite out.
/// No clock is read here; the moment is passed in (§5).</para>
/// </remarks>
public static class Favorites
{
    /// <summary>How many favorites are kept.</summary>
    /// <remarks>
    /// Generous rather than unlimited. A list nobody can scan is a list nobody
    /// uses, and the radio's own memory holds ninety-nine.
    /// </remarks>
    public const int Maximum = 99;

    /// <summary>Build a favorite from where the operator is standing.</summary>
    /// <param name="frequencyHz">The frequency.</param>
    /// <param name="mode">The mode, or "".</param>
    /// <param name="here">The neighborhood, or null.</param>
    /// <param name="nowUtc">The moment.</param>
    /// <returns>The favorite, named from its context.</returns>
    public static Favorite From(
        long frequencyHz, string? mode, Neighborhood? here, DateTime nowUtc)
    {
        var band = BandPlan.BandFor(frequencyHz)?.Name ?? "";
        var hood = here?.Name ?? "";

        return new Favorite(
            frequencyHz, NameFor(frequencyHz, here), (mode ?? "").Trim(),
            band, hood, nowUtc);
    }

    /// <summary>
    /// What to call a favorite nobody has renamed.
    /// </summary>
    /// <param name="frequencyHz">The frequency.</param>
    /// <param name="here">The neighborhood, or null.</param>
    /// <returns>e.g. "14.074, where the digital modes gather".</returns>
    /// <remarks>
    /// NOTHING TYPED, AND NOTHING INVENTED. Where the map has a name for the
    /// block, the favorite borrows it, because that is the reason somebody was
    /// there. Where it does not, the favorite is the frequency and its band and
    /// says no more, rather than making something up about a stretch nobody
    /// published a convention for (§0.0).
    /// </remarks>
    public static string NameFor(long frequencyHz, Neighborhood? here)
    {
        var label = (frequencyHz / 1_000_000.0).ToString(
            "0.000", System.Globalization.CultureInfo.InvariantCulture);

        if (here is null || here.Name.Length == 0)
        {
            var band = BandPlan.BandFor(frequencyHz)?.Name;
            return band is null ? label : $"{label} on {band}";
        }

        return $"{label}, {Lowercase(here.Name)}";
    }

    /// <summary>The favorite at a frequency, or null.</summary>
    /// <param name="favorites">The list.</param>
    /// <param name="frequencyHz">Where the dial is.</param>
    /// <returns>The one saved here, or null.</returns>
    /// <remarks>
    /// Exact rather than near. A star that lit up a hundred hertz away would
    /// make un-saving unpredictable, and the operator would learn not to trust
    /// it.
    /// </remarks>
    public static Favorite? At(IEnumerable<Favorite> favorites, long frequencyHz)
    {
        ArgumentNullException.ThrowIfNull(favorites);

        return favorites.FirstOrDefault(f => f.FrequencyHz == frequencyHz);
    }

    /// <summary>
    /// What the star says, given where the dial is.
    /// </summary>
    /// <param name="saved">The favorite here, or null.</param>
    /// <returns>Its name, or the invitation to save.</returns>
    public static string StarLabel(Favorite? saved)
        => saved is null ? "save this spot" : saved.Name;

    private static string Lowercase(string name)
        => name.Length > 1 && char.IsUpper(name[0]) && !char.IsUpper(name[1])
            ? char.ToLowerInvariant(name[0]) + name[1..]
            : name;
}
