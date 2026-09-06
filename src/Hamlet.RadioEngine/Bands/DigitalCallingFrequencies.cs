using Hamlet.RadioEngine.Explore;

namespace Hamlet.RadioEngine.Bands;

/// <summary>
/// Where a digital mode is worked in a band, derived from the cited convention
/// data rather than written down here (work instruction 251, task 5).
/// </summary>
/// <remarks>
/// <para>**THERE IS NO FREQUENCY LITERAL IN THIS FILE**, which is the same rule
/// <see cref="HfBands"/> keeps and for the same reason. §0 requires data that can
/// be generated from a source of truth to be generated from it, and §0.2.1
/// forbids frequencies asserted from a model's memory. Every number here comes
/// out of <c>data/bands/us-neighborhoods.json</c>, whose digital rows cite the
/// WSJT-X default frequency table — *"the FT8 and FT4 dial frequencies the
/// software itself ships with, which is what the whole world is actually tuned
/// to"*.</para>
/// <para>**THE SAME BLOCKS THE MAP ALREADY DRAWS.** `FT8 city` on the
/// Neighborhood map, `DigitalModeChip.For`'s lit chip and `HfBands.Landing` all
/// read this file already. A press that landed somewhere else would put the dial
/// outside the block the map had just drawn under it.</para>
/// <para>**AND IT IS INCOMPLETE, WHICH IS REPORTED RATHER THAN FILLED IN.** As of
/// 2026-09-05 the data carries FT8 on all seven bands, FT4 on five, PSK31 on four
/// and **WSPR on none at all**. A band with no row for a mode answers null, and
/// the surface says so; it does not get a number typed in from somewhere nobody
/// can cite. Adding WSPR is a data change with a citation behind it, not a code
/// change.</para>
/// </remarks>
public static class DigitalCallingFrequencies
{
    /// <summary>
    /// Where a mode is worked in a band, or null where the data has no row.
    /// </summary>
    /// <param name="bandName">The band, as <see cref="HfBands.Names"/> spells it.</param>
    /// <param name="shortName">The mode's short name, e.g. "FT8".</param>
    /// <returns>The block, or null.</returns>
    public static Neighborhood? Find(string? bandName, string? shortName)
        => Find(NeighborhoodData.Current, bandName, shortName);

    /// <summary>
    /// Where a mode is worked in a band, or null where the data has no row.
    /// </summary>
    /// <param name="conventions">The convention data.</param>
    /// <param name="bandName">The band, as <see cref="HfBands.Names"/> spells it.</param>
    /// <param name="shortName">The mode's short name, e.g. "FT8".</param>
    /// <returns>The block, or null.</returns>
    /// <exception cref="ArgumentNullException">The conventions are null.</exception>
    /// <remarks>
    /// **MATCHED ON THE SHORT NAME AND ON THE DIGITAL FAMILY, BOTH.** The short
    /// name is what the mode strip already lights a chip from, so the strip and
    /// the press cannot disagree about which block is FT8's. The family check is
    /// what stops a Morse or phone row ever answering a digital press, however it
    /// happened to be labelled.
    /// </remarks>
    public static Neighborhood? Find(
        NeighborhoodData conventions, string? bandName, string? shortName)
    {
        ArgumentNullException.ThrowIfNull(conventions);

        if (string.IsNullOrWhiteSpace(bandName) || string.IsNullOrWhiteSpace(shortName))
        {
            return null;
        }

        var wanted = shortName.Trim();

        return conventions.ForBand(bandName.Trim())
            .FirstOrDefault(
                n => n.Family == ModeFamily.Digital
                     && string.Equals(
                         n.ShortName.Trim(), wanted,
                         StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Which bands have a row for this mode, lowest first.
    /// </summary>
    /// <param name="shortName">The mode's short name, e.g. "FT4".</param>
    /// <returns>The band names, possibly none.</returns>
    /// <remarks>
    /// **SO A SURFACE CAN SAY WHERE IT *IS* RATHER THAN ONLY THAT IT IS NOT
    /// HERE.** "This band has no FT4" is true and unhelpful; "40 m has one" is
    /// what the operator can act on, and it costs nothing because it comes out of
    /// the same rows.
    /// </remarks>
    public static IReadOnlyList<string> BandsWith(string? shortName)
        => BandsWith(NeighborhoodData.Current, shortName);

    /// <summary>
    /// Which bands have a row for this mode, lowest first.
    /// </summary>
    /// <param name="conventions">The convention data.</param>
    /// <param name="shortName">The mode's short name, e.g. "FT4".</param>
    /// <returns>The band names, possibly none.</returns>
    /// <exception cref="ArgumentNullException">The conventions are null.</exception>
    public static IReadOnlyList<string> BandsWith(
        NeighborhoodData conventions, string? shortName)
    {
        ArgumentNullException.ThrowIfNull(conventions);

        // **HfBands' ORDER AND NOT THE FILE'S**, so the answer is in the order
        // the band buttons are drawn in and the operator reads it as a row of
        // the same bands he already has on screen.
        return HfBands.Names
            .Where(band => Find(conventions, band, shortName) is not null)
            .ToList();
    }
}
