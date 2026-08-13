using Hamlet.RadioEngine.Solar;

namespace Hamlet.RadioEngine.Explore;

/// <summary>When a band is in its element.</summary>
public enum BandElement
{
    /// <summary>A daytime band: it wants the sun up.</summary>
    Day,

    /// <summary>A night band: it wants the sun down.</summary>
    Night,

    /// <summary>An all-rounder: useful either side of sunset, differently.</summary>
    Both,
}

/// <summary>The four seasons, from the calendar and the hemisphere.</summary>
public enum Season
{
    /// <summary>Unknown hemisphere, so unknown season.</summary>
    Unknown,

    /// <summary>Spring.</summary>
    Spring,

    /// <summary>Summer.</summary>
    Summer,

    /// <summary>Autumn.</summary>
    Autumn,

    /// <summary>Winter.</summary>
    Winter,
}

/// <summary>
/// What each band is like, in plain words: what the sun is doing to it right
/// now, and what the season tends to do to it.
/// </summary>
/// <remarks>
/// <para>SOURCE MARKING (§0, §4): editorial radio culture and the standard
/// plain-language account of daytime absorption, carried from general
/// knowledge — [extrapolated], the same status as
/// <see cref="NeighborhoodPlan"/> and <see cref="ModeGuide"/>. The physics
/// summarized here is textbook and stable; nothing in it is measured, and
/// nothing in it is about today.</para>
/// <para>WHAT MAY BE SAID, AND WHAT MAY NOT (HM-DEC-033). Where the sun is and
/// what month it is are computable facts and are stated plainly. Whether a
/// band is actually open today is propagation, which Hamlet cannot see
/// (FG-007), so nothing here says a band is open, closed or working, and
/// nothing tells the operator what they will or will not reach. "20 m lives on
/// sunlight and right now it's got it" is a claim about sunlight and is fine.
/// "20 m is open" is a claim about the ionosphere and is not.</para>
/// <para>THE VOICE (HM-DEC-034). This is the most important prose in the app.
/// It is written as a patient friend with forty years on the air would explain
/// it while you were both looking at the radio: connected thoughts that run
/// into one another, ordinary words, and the reason attached to the fact. A
/// stack of short declarative sentences reads as machine-written, and a
/// newcomer who has owned a radio for six years without using it has had
/// enough of being told things by machines.</para>
/// <para>Pure: a band, a solar snapshot and a date in, prose out. No clock read
/// (§5).</para>
/// </remarks>
public static class BandCharacter
{
    /// <summary>When each band is in its element.</summary>
    /// <param name="bandName">Band name, e.g. "40 m".</param>
    /// <returns>Day, night, or both.</returns>
    public static BandElement ElementOf(string bandName) => bandName switch
    {
        "80 m" => BandElement.Night,
        "40 m" or "30 m" => BandElement.Both,
        _ => BandElement.Day,
    };

    /// <summary>
    /// The season at a latitude, from the month.
    /// </summary>
    /// <param name="month">Calendar month, 1 to 12.</param>
    /// <param name="latitude">Degrees north; null when unknown.</param>
    /// <returns>The season, or Unknown without a hemisphere.</returns>
    /// <remarks>
    /// Meteorological seasons rather than astronomical ones: whole months are
    /// what somebody means by "August", and the three-day difference from the
    /// solstice matters to nobody reading this.
    /// </remarks>
    public static Season SeasonAt(int month, double? latitude)
    {
        if (latitude is null)
        {
            return Season.Unknown;
        }

        var northern = new[]
        {
            Season.Winter, Season.Winter, Season.Spring, Season.Spring, Season.Spring,
            Season.Summer, Season.Summer, Season.Summer, Season.Autumn, Season.Autumn,
            Season.Autumn, Season.Winter,
        };

        var season = northern[Math.Clamp(month, 1, 12) - 1];

        if (latitude >= 0)
        {
            return season;
        }

        return season switch
        {
            Season.Winter => Season.Summer,
            Season.Spring => Season.Autumn,
            Season.Summer => Season.Winter,
            Season.Autumn => Season.Spring,
            _ => Season.Unknown,
        };
    }

    /// <summary>
    /// Describe a band: what the sun is doing to it, and what the season tends
    /// to do, as one flowing passage.
    /// </summary>
    /// <param name="bandName">Band name, e.g. "40 m".</param>
    /// <param name="sun">Where the sun is, or <see cref="SolarSnapshot.Unknown"/>.</param>
    /// <param name="month">Calendar month, 1 to 12.</param>
    /// <param name="latitude">Degrees north; null when unknown.</param>
    /// <returns>One or two sentences of plain-language character.</returns>
    public static string Describe(
        string bandName, SolarSnapshot sun, int month, double? latitude)
    {
        var now = DescribeNow(bandName, sun);
        var season = DescribeSeason(bandName, SeasonAt(month, latitude));

        return season.Length == 0 ? now : now + " " + season;
    }

    /// <summary>What the sun is doing to this band at this moment.</summary>
    /// <param name="bandName">Band name.</param>
    /// <param name="sun">Where the sun is.</param>
    /// <returns>A sentence or two, never empty.</returns>
    public static string DescribeNow(string bandName, SolarSnapshot sun)
    {
        if (!sun.IsKnown)
        {
            // No coordinates, so nothing about the sun can be said. The band's
            // standing character still can.
            return WithoutTheSun(bandName);
        }

        return sun.IsDaylight ? InDaylight(bandName, sun) : AfterDark(bandName, sun);
    }

    private static string InDaylight(string bandName, SolarSnapshot sun) => bandName switch
    {
        "80 m" =>
            "Right now the sun's up, and that's bad news for 80 m — daylight thickens a "
            + "layer of the atmosphere that soaks up low frequencies before they get "
            + "anywhere. Come back after dark and this band turns into something else "
            + "entirely.",

        "40 m" =>
            "With the sun up, 40 m pulls in close: that same daylight layer absorbs part "
            + "of what you send, so what survives tends to come back down within a few "
            + "hundred miles. That's its own kind of useful — this is when you'll find "
            + "the regional nets and the people a couple of states over.",

        "30 m" =>
            "30 m sits in a narrow slot where voice isn't allowed at all, so it never "
            + "gets crowded and never gets loud. Daylight costs it something, but less "
            + "than it costs the bands below, which leaves it one of the steadier places "
            + "to sit in the afternoon.",

        "20 m" =>
            "20 m lives on sunlight — it needs the sun overhead to bounce a signal around "
            + "the world, and right now it's got it. If you're going to talk to someone on "
            + "the far side of an ocean today, the odds are it happens here.",

        "17 m" =>
            "17 m is 20 m's quieter neighbor, a narrow band with no contests allowed on "
            + "it, so the crowding never quite arrives. It leans on the sun the same way "
            + "20 m does, and the sun is up.",

        "15 m" =>
            "15 m wants a strong sun rather than merely a present one, and it rewards the "
            + "years when the sun is busy. Daylight is the first thing it needs, and "
            + "that much it has right now.",

        "10 m" =>
            "The sun's up, which is the first condition 10 m asks for — it's the highest "
            + "band Hamlet knows and the one that leans hardest on the sun being active.",

        _ =>
            "The sun's up, which generally suits the higher bands more than the lower "
            + "ones.",
    };

    private static string AfterDark(string bandName, SolarSnapshot sun)
    {
        var since = sun.SinceSunset is { } elapsed ? Roughly(elapsed) : null;
        var sunset = since is null
            ? "The sun's down"
            : $"The sun went down {since} ago";

        return bandName switch
        {
            "80 m" =>
                $"{sunset}, and the layer that spends all day swallowing low frequencies "
                + "is thinning away. This is 80 m at its best — a big, slow, sociable band "
                + "where the voices come from a few hundred miles off and nobody's in a "
                + "hurry.",

            "40 m" =>
                $"{sunset}, and the layer that was absorbing your signal all day is "
                + "thinning out. This is 40 m at its best, reaching across the country in "
                + "a way it simply can't at noon.",

            "30 m" =>
                $"{sunset}, which suits 30 m — no voice is allowed here, so it stays "
                + "quiet, and the loss of the daylight layer lets it stretch out further "
                + "than it manages in the afternoon.",

            "20 m" =>
                $"{sunset}, and 20 m tends to fold up as the light goes: the sunlight that "
                + "does the work of carrying a signal around the world isn't there to do "
                + "it. It can hang on for a while after sunset, particularly in summer, "
                + "but this isn't its hour.",

            "17 m" =>
                $"{sunset}, and 17 m leans on sunlight much as 20 m does, so the evening "
                + "is not usually when it has much to offer.",

            "15 m" =>
                $"{sunset}, and 15 m needs daylight and an active sun together. Without "
                + "the first of those, the evening tends to be a thin time here.",

            "10 m" =>
                $"{sunset}, and 10 m asks more of the sun than any band below it, so after "
                + "dark it usually goes quiet.",

            _ => $"{sunset}, which generally favors the lower bands over the higher ones.",
        };
    }

    /// <summary>The band's standing character when the sun's position is unknown.</summary>
    private static string WithoutTheSun(string bandName) => bandName switch
    {
        "80 m" =>
            "80 m is a night band: daylight thickens a layer of the atmosphere that soaks "
            + "up low frequencies, and after dark that layer thins and the band opens out "
            + "into something big and sociable. Set your location in Settings and Hamlet "
            + "can tell you where the sun is from here.",

        "40 m" =>
            "40 m works either side of sunset, differently — close to home in the "
            + "afternoon, reaching much further once the daylight absorption fades. Set "
            + "your location in Settings and Hamlet can tell you which of those you're in.",

        "30 m" =>
            "30 m is a narrow slot with no voice allowed on it, which keeps it quiet at "
            + "any hour. Set your location in Settings and Hamlet can tell you where the "
            + "sun is from here.",

        "20 m" =>
            "20 m lives on sunlight and is the steadiest of the long-distance bands. Set "
            + "your location in Settings and Hamlet can tell you whether the sun is up.",

        "17 m" =>
            "17 m is 20 m's quieter neighbor — narrow, no contests, and dependent on the "
            + "sun. Set your location in Settings and Hamlet can tell you whether the sun "
            + "is up.",

        "15 m" =>
            "15 m wants a strong sun as well as a present one. Set your location in "
            + "Settings and Hamlet can tell you whether the sun is up.",

        "10 m" =>
            "10 m is the lottery ticket of the bands, following the sun's eleven-year "
            + "cycle. Set your location in Settings and Hamlet can tell you whether the "
            + "sun is up.",

        _ =>
            "Set your location in Settings and Hamlet can tell you what the sun is doing "
            + "from here.",
    };

    /// <summary>What the season tends to do to this band.</summary>
    /// <param name="bandName">Band name.</param>
    /// <param name="season">The season where the operator is.</param>
    /// <returns>A sentence, or "" when there is nothing worth adding.</returns>
    /// <remarks>
    /// Always a tendency, never a measurement. Summer static on the low bands
    /// and the eleven-year cycle on the high ones are the two seasonal facts a
    /// newcomer most needs and least often hears.
    /// </remarks>
    public static string DescribeSeason(string bandName, Season season)
    {
        if (season == Season.Unknown)
        {
            return "";
        }

        var summer = season == Season.Summer;
        var winter = season == Season.Winter;

        return bandName switch
        {
            "80 m" when summer =>
                "Summer isn't kind to it, though — thunderstorms hundreds of miles away "
                + "crackle across this band all season and arrive as a wash of static. "
                + "Come the winter the nights are long and that noise falls away.",

            "80 m" when winter =>
                "Winter is its season, too: the nights run long and the summer storm "
                + "static is gone, which is why people say the low bands come alive once "
                + "the leaves are down.",

            "80 m" =>
                "It's at its best in the colder half of the year, when the nights are "
                + "longer and the storm static of summer has died back.",

            "40 m" when summer =>
                "Summer costs it something, mind — distant thunderstorms wash static "
                + "across the low bands from June onward, and it's quieter here in winter.",

            "40 m" when winter =>
                "Winter suits it: long nights and none of the summer storm static that "
                + "washes across the low bands from June onward.",

            "40 m" => "It's a little quieter in the colder months, when there's less "
                      + "storm static about.",

            "30 m" =>
                "It changes less with the seasons than its neighbors do, which is part of "
                + "why people who like it tend to keep going back.",

            "20 m" =>
                "It's the steadiest of the long-distance bands and works in some form for "
                + "most of the year, which is why it's where a great many people make "
                + "their first contact overseas.",

            "17 m" =>
                "Like the rest of the high bands it does better in the years when the sun "
                + "is busy, and it's usually worth a listen when 20 m gets crowded.",

            "15 m" =>
                "It follows the sun's eleven-year cycle closely, so what it does this year "
                + "and what it did five years ago can be very different things.",

            "10 m" =>
                "This one's the lottery ticket. It follows an eleven-year cycle in the "
                + "sun's activity, so it can sit quiet for years and then, one afternoon, "
                + "hand somebody the other side of the world on five watts and a piece of "
                + "wire. Always worth thirty seconds of listening.",

            _ => "",
        };
    }

    /// <summary>
    /// A human interval — "about an hour and a half", "twenty minutes".
    /// </summary>
    /// <param name="elapsed">How long.</param>
    /// <returns>Plain words, no digits where a word will do.</returns>
    /// <remarks>
    /// Nobody says "1.6 hours". The prose around this is written to sound like
    /// a person, and a decimal in the middle of it would give the game away.
    /// </remarks>
    public static string Roughly(TimeSpan elapsed)
    {
        var minutes = (int)Math.Round(elapsed.TotalMinutes);

        if (minutes < 5)
        {
            return "a few minutes";
        }

        if (minutes < 25)
        {
            return "about twenty minutes";
        }

        if (minutes < 50)
        {
            return "about half an hour";
        }

        if (minutes < 80)
        {
            return "about an hour";
        }

        if (minutes < 110)
        {
            return "about an hour and a half";
        }

        var hours = (int)Math.Round(elapsed.TotalHours);

        if (hours < 12)
        {
            return $"about {Spell(hours)} hours";
        }

        return "most of the night";
    }

    private static string Spell(int n) => n switch
    {
        2 => "two",
        3 => "three",
        4 => "four",
        5 => "five",
        6 => "six",
        7 => "seven",
        8 => "eight",
        9 => "nine",
        10 => "ten",
        11 => "eleven",
        _ => n.ToString(System.Globalization.CultureInfo.InvariantCulture),
    };
}
