using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Licensing;

namespace Hamlet.RadioEngine.Explore;

/// <summary>One named region of a band, a neighborhood on the map.</summary>
/// <param name="Name">Full name, e.g. "FT8 city".</param>
/// <param name="ShortName">Map label; empty string for unlabeled space.</param>
/// <param name="LowHz">Lower edge in hertz.</param>
/// <param name="HighHz">Upper edge in hertz.</param>
/// <param name="Vibe">Two-or-three-word character tag.</param>
/// <param name="Blurb">The plain-language story a newcomer needs.</param>
/// <param name="JumpHz">Where "take me there" lands.</param>
/// <param name="Family">Which mode family lives here. The map colors from
/// this rather than from a per-neighborhood literal, so every surface that
/// shows a mode family agrees without being told twice (HM-DEC-032).</param>
/// <param name="Cite">
/// The id of the source this block's frequencies came from, or empty for a
/// stretch filled in from the band's own structure (HM-DEC-054).
/// </param>
/// <param name="Caution">
/// What sending Morse here would actually do, or null where there is nothing
/// worth saying. A consequence and never an instruction: the card may explain
/// that the software in this block cannot hear Morse, and may not tell anybody
/// what to do about it.
/// </param>
/// <param name="SignalsAreAudioOffsets">
/// True where every signal in the block is an audio tone above the dial, so the
/// receiver must pass the whole block to hear all of it.
/// </param>
/// <remarks>
/// **THE REQUIREMENT IS A FACT ABOUT THE RADIO, NOT ABOUT THE BAND** (work
/// instruction 040). Every signal in an FT8 block sits as an audio tone between
/// nought and three kilohertz above the dial, so a receiver has to pass all
/// three kilohertz to hear the block at all. The operator lost an hour to a
/// radio correctly tuned to 14.074 through a 500 Hz window, which is a state
/// this map could describe and could not check.
/// </remarks>
public sealed record Neighborhood(
    string Name, string ShortName, long LowHz, long HighHz,
    string Vibe, string Blurb, long JumpHz, ModeFamily Family,
    string Cite = "", string? Caution = null,
    bool SignalsAreAudioOffsets = false)
{
    /// <summary>True when the frequency lies inside this neighborhood.</summary>
    public bool Contains(long hz) => hz >= LowHz && hz <= HighHz;

    /// <summary>
    /// Where the signals actually are, for somebody who has just tuned here.
    /// </summary>
    /// <returns>One sentence, or "" where the block does not work this way.</returns>
    /// <remarks>
    /// <para>**NOBODY TRANSMITS ON THE PUBLISHED FREQUENCY, AND THAT HAS NOW
    /// COST THE OPERATOR TWO EVENINGS** (work instruction 040). You tune to
    /// 14.074 and every station sits as an audio tone somewhere in the three
    /// kilohertz above it. **A dead dial and a live band is the correct
    /// behaviour of a correctly tuned radio**, and an operator not told that
    /// concludes the band is empty or the rig is broken.</para>
    /// <para>**THE NUMBERS ARE THE ROW'S OWN.** The dial is the jump frequency
    /// and the block is the row's span; nothing here is typed, so a block that
    /// is two kilohertz on 80 m and three on 20 m says so on each.</para>
    /// </remarks>
    public string WhereTheSignalsAre()
    {
        if (!SignalsAreAudioOffsets)
        {
            return "";
        }

        var wideHz = HighHz - LowHz;

        var wide = wideHz >= 1000
            ? $"{wideHz / 1000.0:0.#} kHz"
            : $"{wideHz} Hz";

        return $"Tune to {JumpHz / 1_000_000.0:0.000}, and expect the dial "
            + $"itself to sound dead. Everybody here transmits somewhere in the "
            + $"{wide} above it, arriving as audio tones rather than on the "
            + "frequency you set, so the band comes alive across the whole "
            + "block at once.";
    }

    /// <summary>How wide a passband this block needs, or null.</summary>
    /// <returns>
    /// True when it is wide enough, false when it is not, and **null when
    /// nobody knows** — either the row states no requirement or the radio has
    /// not been read.
    /// </returns>
    /// <remarks>
    /// **THE REQUIREMENT IS THE BLOCK'S OWN WIDTH AND IS NOT WRITTEN DOWN
    /// TWICE** (§0). The first version of this stored three kilohertz against
    /// every FT8 row, and the map walk caught it immediately: the 80 m block is
    /// two kilohertz wide, so the file was claiming a radio needed more passband
    /// than the block occupies. Deriving it cannot be wrong that way.
    /// </remarks>
    /// <remarks>
    /// <para>**THREE ANSWERS, NOT TWO** (§0.0). A radio whose filter has not been
    /// read is not a radio that is set correctly, and returning false for it
    /// would say the operator has a problem he may not have. Unknown is the
    /// honest third answer and the caller has to carry it.</para>
    /// <para>**THE REQUIREMENT COMES FROM THE ROW, NOT FROM CODE.** The file
    /// states it per neighborhood with its own reason; a constant here would be
    /// a second copy of a fact the data already carries (§0).</para>
    /// </remarks>
    public long? PassbandHz
        => SignalsAreAudioOffsets ? HighHz - LowHz : null;

    /// <summary>
    /// True when a receiver passband of this width can hear the whole block.
    /// </summary>
    /// <param name="hertz">The passband the radio reports, or null if unread.</param>
    /// <returns>Wide enough, not wide enough, or null when nobody knows.</returns>
    public bool? PassbandIsWideEnough(double? hertz)
        => PassbandHz is not { } needed || hertz is not { } have
            ? null
            : have >= needed;
}

/// <summary>
/// The neighborhood map: the tribal knowledge of where things live on each
/// band, written down and cited.
/// </summary>
/// <remarks>
/// <para>The conventions come from <c>data/bands/us-neighborhoods.json</c>,
/// each row carrying the source its frequencies were read from (HM-DEC-054).
/// Nothing on this map is written from recollection.</para>
/// <para>WHAT THIS CLASS ADDS to the file is only the space between the cited
/// rows. Nobody publishes a convention for every kilohertz of a band, and the
/// gaps are real rather than missing data, so they are filled from the band's
/// own structure: the stretch below the phone segment is where Morse and the
/// data modes live, and the stretch above it is where voices do. That boundary
/// is derived from the cited Part 97 data rather than carried here as a second
/// copy (§0).</para>
/// </remarks>
public static class NeighborhoodPlan
{
    /// <summary>
    /// Neighborhoods for a band, gap-free and lowest first.
    /// </summary>
    /// <param name="band">The band.</param>
    /// <returns>The map.</returns>
    public static IReadOnlyList<Neighborhood> ForBand(CwBand band)
    {
        ArgumentNullException.ThrowIfNull(band);

        return Build(band, NeighborhoodData.Current, PrivilegeData.Current);
    }

    /// <summary>
    /// How far past each band edge the map shows, so the wall is visible.
    /// </summary>
    /// <param name="band">The band.</param>
    /// <returns>The margin in hertz.</returns>
    /// <remarks>
    /// A twelfth of the band, and never less than five kilohertz, so 30 m gets a
    /// visible sliver rather than a hairline. It is a fraction rather than a
    /// fixed figure because the map is drawn to a width and what matters is how
    /// much of that width the edge takes up.
    /// </remarks>
    public static long MarginHz(CwBand band)
    {
        ArgumentNullException.ThrowIfNull(band);

        return Math.Max((band.HighHz - band.LowHz) / 12, 5_000);
    }

    /// <summary>
    /// The band's map with the spectrum either side of it shown as what it is.
    /// </summary>
    /// <param name="band">The band.</param>
    /// <returns>The map, running from a margin below the band to one above.</returns>
    /// <remarks>
    /// THE EDGE HAS TO BE VISIBLE TO BE LEARNED (HM-DEC-055). A map that stops
    /// exactly at the band edge shows a wall as the end of the picture, which
    /// teaches nothing at all, and the operator who tuned to the very top of
    /// 20 m had no way to see that a little further up there is no amateur
    /// spectrum. So the map runs past both edges and says what is out there.
    /// </remarks>
    public static IReadOnlyList<Neighborhood> WithEdges(CwBand band)
    {
        ArgumentNullException.ThrowIfNull(band);

        var margin = MarginHz(band);
        var map = new List<Neighborhood> { Beyond(band, band.LowHz - margin, band.LowHz - 1) };

        map.AddRange(ForBand(band));
        map.Add(Beyond(band, band.HighHz + 1, band.HighHz + margin));

        return map;
    }

    /// <summary>One stretch of spectrum that is not amateur at all.</summary>
    private static Neighborhood Beyond(CwBand band, long lowHz, long highHz)
        => new(
            "Not a ham band", "not ham", lowHz, highHz,
            "Somebody else's spectrum",
            $"This is past the edge of {band.Name} and it is not amateur "
            + "spectrum at all. Broadcasters, maritime and aeronautical "
            + "services and a good deal else live up and down the dial from "
            + "every ham band, and no amateur license permits transmitting on "
            + "any of it. " + AmateurSpectrum.ListeningIsStillFine,
            lowHz + ((highHz - lowHz) / 2),
            ModeFamily.OutsideTheBand,
            Cite: "cfr-97.301",
            Caution: "There is no amateur license that permits transmitting "
                   + "here, so a call in any mode would be going out on "
                   + "somebody else's allocation.");

    /// <summary>Build a band's map from given data, for the tests.</summary>
    /// <param name="band">The band.</param>
    /// <param name="data">The cited conventions.</param>
    /// <param name="privileges">The cited regulation, for the phone boundary.</param>
    /// <returns>The map.</returns>
    public static IReadOnlyList<Neighborhood> Build(
        CwBand band, NeighborhoodData data, PrivilegeData privileges)
    {
        ArgumentNullException.ThrowIfNull(band);
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(privileges);

        var cited = data.ForBand(band.Name)
            .Where(n => n.HighHz > band.LowHz && n.LowHz < band.HighHz)
            .OrderBy(n => n.LowHz)
            .ToList();

        var phoneStartHz = PhoneStartHz(band, privileges);
        var map = new List<Neighborhood>();
        var at = band.LowHz;

        foreach (var hood in cited)
        {
            AddFill(map, at, hood.LowHz, band, phoneStartHz);
            map.Add(hood);
            at = Math.Max(at, hood.HighHz);
        }

        AddFill(map, at, band.HighHz, band, phoneStartHz);

        return Separate(map);
    }

    /// <summary>
    /// Make sure no two neighborhoods claim the same hertz.
    /// </summary>
    /// <remarks>
    /// <see cref="Neighborhood.Contains"/> takes both edges, so two blocks that
    /// meet exactly would both answer for the frequency where they touch, and
    /// which one a caller got would depend on the order it happened to search
    /// in. That is fine for a tooltip and not fine for the card that says what
    /// is going on where the dial is pointing. So each block gives up its last
    /// hertz to the one above it and the answer becomes single-valued.
    /// </remarks>
    private static IReadOnlyList<Neighborhood> Separate(List<Neighborhood> map)
    {
        for (var i = 0; i < map.Count - 1; i++)
        {
            if (map[i].HighHz >= map[i + 1].LowHz)
            {
                map[i] = map[i] with { HighHz = map[i + 1].LowHz - 1 };
            }
        }

        return map;
    }

    /// <summary>
    /// Where the voice segment starts, from the cited regulation.
    /// </summary>
    /// <param name="band">The band.</param>
    /// <param name="privileges">The cited Part 97 data.</param>
    /// <returns>The lowest phone frequency, or the band's top when there is none.</returns>
    /// <remarks>
    /// <para>Read from 47 CFR 97.305(c) rather than carried here, because a
    /// second copy of a boundary is a second copy until the two disagree. 30 m
    /// has no phone segment at all and answers with the top of the band, which
    /// is the honest shape rather than a special case.</para>
    /// <para>THE LOWEST PHONE FREQUENCY IS THE WRONG ANSWER, which cost a round
    /// of this. 40 m has a phone allocation at 7.075 that belongs to stations in
    /// particular places rather than to the band generally, and taking the
    /// lowest range would have painted everything above 7.077 as the voice end,
    /// FT8 and the automatic stations included. What is wanted is the point
    /// above which the rest of the band really is voice, so the ranges are
    /// merged and the one that reaches the top of the band is the one that
    /// counts.</para>
    /// </remarks>
    private static long PhoneStartHz(CwBand band, PrivilegeData privileges)
    {
        var phone = privileges.EmissionRanges
            .Where(r => r.Emissions.Contains(TransmitMode.Phone))
            .Where(r => r.Range.HighHz > band.LowHz && r.Range.LowHz < band.HighHz)
            .Select(r => (r.Range.LowHz, r.Range.HighHz))
            .OrderBy(r => r.LowHz)
            .ToList();

        var start = band.HighHz;

        // Walk down from the top, joining ranges that touch, and stop at the
        // first gap. Where that walk ends is where the voice end begins.
        for (var i = phone.Count - 1; i >= 0; i--)
        {
            if (phone[i].HighHz < start)
            {
                break;
            }

            start = phone[i].LowHz;
        }

        return Math.Clamp(start, band.LowHz, band.HighHz);
    }

    /// <summary>Fill a stretch nobody published a convention for.</summary>
    private static void AddFill(
        List<Neighborhood> map, long fromHz, long toHz, CwBand band, long phoneStartHz)
    {
        if (toHz <= fromHz)
        {
            return;
        }

        // A stretch that straddles the phone boundary is two stretches, because
        // the character changes at exactly that line.
        if (fromHz < phoneStartHz && toHz > phoneStartHz)
        {
            AddFill(map, fromHz, phoneStartHz, band, phoneStartHz);
            AddFill(map, phoneStartHz, toHz, band, phoneStartHz);
            return;
        }

        var jump = fromHz + ((toHz - fromHz) / 2);

        map.Add(fromHz >= phoneStartHz
            ? new Neighborhood(
                "Phone side", "SSB", fromHz, toHz,
                "Voices live here",
                $"The voice end of {band.Name}. Single sideband sounds like ducks "
                + "until you tune it just right, and then it is suddenly a person "
                + "somewhere else. Nobody has written down a convention for this "
                + "stretch, so what you find here is whoever happens to be on it.",
                jump, ModeFamily.Phone,
                Caution: "This is the voice end of the band, so a Morse call here "
                       + "goes out under people holding conversations, and they "
                       + "will hear it as interference rather than as a call.")
            // OPEN AND NOT MORSE, which looks like a small thing and is not.
            // Below the phone segment the regulation allows Morse and the data
            // modes alike, and nobody has published a claim to this stretch, so
            // coloring it amber would say Morse owns ground it does not (§0.6).
            : new Neighborhood(
                "Open ground", "", fromHz, toHz,
                "Nobody's in particular",
                $"Morse and the data modes share this stretch of {band.Name} and "
                + "nobody has published a convention for it, so what you hear is "
                + "whoever happened to want the space. Most days it is quiet "
                + "enough to pick out one signal at a time, which makes it good "
                + "hunting ground once you can tell one note from another.",
                jump, ModeFamily.Open));
    }
}
