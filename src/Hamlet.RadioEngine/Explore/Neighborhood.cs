using Hamlet.RadioEngine.Bands;

namespace Hamlet.RadioEngine.Explore;

/// <summary>One named region of a band — a neighborhood on the map.</summary>
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
public sealed record Neighborhood(
    string Name, string ShortName, long LowHz, long HighHz,
    string Vibe, string Blurb, long JumpHz, ModeFamily Family)
{
    /// <summary>True when the frequency lies inside this neighborhood.</summary>
    public bool Contains(long hz) => hz >= LowHz && hz <= HighHz;
}

/// <summary>
/// The neighborhood map: the tribal knowledge of where things live on each
/// band, written down. This is editorial radio culture, not regulation —
/// marked [extrapolated] like the band plan (§4); the frequencies themselves
/// follow US conventions and HM-OPEN-005's cited-data move covers both.
/// </summary>
public static class NeighborhoodPlan
{
    private static readonly IReadOnlyList<Neighborhood> FortyMeters = new[]
    {
        new Neighborhood("CW fast lane", "CW DX", 7_000_000, 7_025_000,
            "Rush hour",
            "Where the contest and DX crowd runs Morse at 25+ WPM. Thrilling "
            + "to watch on the waterfall, brutal to copy by ear. Visit later; "
            + "listen now.",
            7_005_000, ModeFamily.Cw),
        new Neighborhood("CW main street", "CW", 7_025_000, 7_060_000,
            "Friendly, unhurried",
            "Everyday Morse. 7.030 is the QRP watering hole, where tiny "
            + "five-watt rigs say hello across oceans. 7.055 is the slow-speed "
            + "club, and calling there as a beginner is the whole point of the "
            + "place. This is your Morse home.",
            7_030_000, ModeFamily.Cw),
        new Neighborhood("Digital corner", "RTTY+", 7_060_000, 7_070_000,
            "Quirky neighbors",
            "The old digital modes: RTTY chirping on twin rails, PSK31 "
            + "whispering in 31 Hz ribbons. Quiet most days, packed during "
            + "digital contests.",
            7_062_000, ModeFamily.Digital),
        new Neighborhood("FT8 city", "FT8", 7_070_000, 7_080_000,
            "Always open, 24/7",
            "The busiest 3 kHz in all of radio. Everyone transmits in "
            + "synchronized 15-second bursts, so the waterfall looks like rain. "
            + "Contacts are structured handshakes rather than conversations, "
            + "and the mode is built to dig a callsign out of noise you can "
            + "barely hear. That's why people reach so far here on so little "
            + "power, and with antennas they're faintly embarrassed about.",
            7_074_000, ModeFamily.Digital),
        new Neighborhood("Quiet blocks", "", 7_080_000, 7_125_000,
            "Off the beaten path",
            "Sparser territory: JS8 keyboard chats, occasional digital nets, "
            + "and open space. Good hunting ground once you can identify "
            + "signals by their waterfall shape.",
            7_090_000, ModeFamily.Open),
        new Neighborhood("Phone downtown", "SSB DX", 7_125_000, 7_175_000,
            "Cocktail party",
            "Voice at last. Single sideband sounds like ducks until you tune it "
            + "just right, and then suddenly it is a person in Portugal. DX "
            + "chasers work the low end.",
            7_150_000, ModeFamily.Phone),
        new Neighborhood("Ragchew boulevard", "SSB", 7_175_000, 7_300_000,
            "Front porch",
            "Long, friendly voice conversations, and evening nets. A net is an "
            + "organized round-table where checking in as a newcomer is "
            + "expected and welcomed. The most welcoming street in radio.",
            7_188_000, ModeFamily.Phone),
    };

    /// <summary>
    /// Neighborhoods for a band. 40 m carries the full editorial map; other
    /// bands get an honest minimal map (CW side, phone side) until their
    /// stories are written — a thin map is truthful, an invented one is not.
    /// </summary>
    public static IReadOnlyList<Neighborhood> ForBand(CwBand band)
    {
        if (band.Name == "40 m")
        {
            return FortyMeters;
        }

        var hoods = new List<Neighborhood>
        {
            new("CW side", "CW", band.CwLowHz, band.CwHighHz,
                "Beeps live here",
                $"The CW and digital end of {band.Name}. Its full neighborhood "
                + "map hasn't been written yet. The 40 m map shows what one "
                + "looks like.",
                band.JumpHz, ModeFamily.Cw),
        };

        if (band.CwHighHz < band.HighHz)
        {
            hoods.Add(new Neighborhood("Phone side", "SSB",
                band.CwHighHz, band.HighHz,
                "Voices live here",
                $"The voice end of {band.Name}. Neighborhood stories to come.",
                (band.CwHighHz + band.HighHz) / 2, ModeFamily.Phone));
        }

        return hoods;
    }
}
