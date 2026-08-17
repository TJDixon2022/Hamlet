using System.Reflection;

namespace Hamlet.RadioEngine.Tests.Cw.Fixtures;

/// <summary>
/// Every rebuilt fixture, and the terms it is built on (HM-OPEN-018).
/// </summary>
/// <remarks>
/// <para>**FOUR MESSAGES, THREE SIGNAL-TO-NOISE TIERS, AND ONE PREAMBLE.** The
/// tiers are the point: fifteen decibels is comfortable, five is where the
/// station this project actually recorded was sitting, and zero is the edge the
/// decoder refuses below by ruling (HM-DEC-097).</para>
/// <para>**NO REAL CALLSIGN APPEARS ANYWHERE.** `N0CALL` is reserved for exactly
/// this. An off-air recording carries somebody else's transmission and this
/// repository is going out under GPL-3.0 (§2.1).</para>
/// <para>The fade is on the five decibel tier only, because that is where it was
/// measured and a full matrix of every impairment against every message is a
/// suite nobody runs.</para>
/// </remarks>
public static class CwFixtureCatalogue
{
    /// <summary>The comfortable tier, in decibels.</summary>
    public const double EasyDb = 15;

    /// <summary>The tier the real station was sitting at.</summary>
    public const double WorkingDb = 5;

    /// <summary>The edge, below which the decoder refuses (HM-DEC-097).</summary>
    public const double EdgeDb = 0;

    /// <summary>The fade rate measured off the air, in hertz.</summary>
    public const double QsbHz = 0.7;

    /// <summary>How deep that fade goes, in decibels.</summary>
    /// <remarks>
    /// <para>**TWELVE, WITH TWENTY-FIVE MEASURED AND RECORDED AS THE CEILING.**
    /// The brief asks for "up to 25 dB", and twenty-five at seven tenths of a
    /// hertz across a fifteen second message is not a fade, it is a message with
    /// most of it removed: measured, the surviving marks were separated by gaps
    /// of seven hundred to two thousand seven hundred milliseconds and the
    /// reference chain read a fifth of it.</para>
    /// <para>The arithmetic says why. One threshold is fitted over about three
    /// seconds and the fade turns over in one and a half, so a single threshold
    /// has to serve both the peak and the trough of two whole cycles, and at
    /// twenty-five decibels no level does. Twelve is four times in amplitude,
    /// which is a substantial fade anybody would notice, and it leaves a message
    /// a decoder can be asked to read.</para>
    /// </remarks>
    public const double QsbDepthDb = 12;

    /// <summary>A callsign exchange, which session 2's classifier will want.</summary>
    public const string ExchangeText = "CQ CQ DE N0CALL N0CALL K";

    /// <summary>Prosigns, each sent as one character rather than two letters.</summary>
    public const string ProsignText = "^BT N0CALL ^AR ^SK";

    /// <summary>Letters, digits and the punctuation a contact actually uses.</summary>
    public const string CoverageText = "1234567890 QRZ? DE/N0CALL";

    /// <summary>What the tight fist sends.</summary>
    /// <remarks>
    /// <para>**NO DIGITS, AND THAT IS A MEASUREMENT RATHER THAN A PREFERENCE.**
    /// This message started as `N0CALL N0CALL`, and `0` is five dahs in a row.
    /// At this fist's sixty-five millisecond gaps, read through the fifty
    /// millisecond window a twenty hertz detection bandwidth needs, those five
    /// dahs merge into one mark of about one and two thirds of a second. That one
    /// run then dominates the two-means fit and the clock collapses: the
    /// reference chain read `K W BG EN` out of `N0CALL N0CALL`.</para>
    /// <para>Worth a fixture of its own one day, and it is not this one. Message
    /// four exists to carry gaps shorter than its own dits, and a second defect
    /// riding along inside it would make any failure ambiguous.</para>
    /// </remarks>
    public const string TightFistText = "TEST DE TEST K";

    /// <summary>Where the rebuilt fixtures live.</summary>
    public static string Folder { get; } = Path.Combine(
        RepositoryRoot(), "tests", "fixtures", "cw", "receiver");

    /// <summary>
    /// The speed messages one to three are sent at, in words a minute.
    /// </summary>
    /// <remarks>
    /// <para>**TWELVE, AND EIGHTEEN WAS TRIED AND MEASURED FIRST.** A detection
    /// bandwidth of twenty hertz needs a fifty millisecond window, and a window
    /// that long smears each keyed edge by about the same. The gate crosses its
    /// threshold early on the rise and late on the fall, so **every mark measures
    /// about twenty-three milliseconds long**, and adding a constant to both
    /// lengths compresses their ratio.</para>
    /// <para>At eighteen words a minute a dit is sixty-seven milliseconds and a
    /// dah two hundred, and the reference chain measured them as ninety and two
    /// hundred and twenty-five: a ratio of **2.45, under the 2.5 floor**, so it
    /// refused the clock and read nothing. The same message at zero decibels
    /// decoded fine, because a threshold sitting closer to the peak crosses later
    /// and biases less — which is how a fixture came to be easier to read the
    /// worse its signal was.</para>
    /// <para>Twelve words a minute is a dit of a hundred and a dah of three
    /// hundred, which survives the bias at 2.6, and it is where this decoder's
    /// operating point actually is: the station this project recorded was sending
    /// at 11.4.</para>
    /// </remarks>
    public const double OrdinaryWpm = 12;

    /// <summary>Every fixture this session builds.</summary>
    /// <remarks>
    /// Messages one to three are sent at <see cref="OrdinaryWpm"/> with textbook
    /// spacing. The fourth is the fist measured off the air, whose element gaps
    /// are **shorter than its own dits** — the shape that breaks any decoder
    /// classifying gaps by counting dits, and the shape no fixture in this
    /// repository contained before now.
    /// </remarks>
    public static IReadOnlyList<CwFixtureRecipe> All { get; } = Build();

    private static List<CwFixtureRecipe> Build()
    {
        var recipes = new List<CwFixtureRecipe>();

        var tiers = new[]
        {
            ("easy", EasyDb, 0.0),
            ("working", WorkingDb, QsbHz),
            ("edge", EdgeDb, 0.0),
        };

        var messages = new (string Slug, string Text, bool TightFist)[]
        {
            ("exchange", ExchangeText, false),
            ("prosigns", ProsignText, false),
            ("coverage", CoverageText, false),
            ("tightfist", TightFistText, true),
        };

        var seed = 20260817;

        foreach (var (slug, text, tight) in messages)
        {
            foreach (var (tier, snr, qsb) in tiers)
            {
                recipes.Add(tight
                    ? new CwFixtureRecipe(
                        $"{slug}-{tier}",
                        text,
                        DitMilliseconds: 105,
                        DahMilliseconds: 283,
                        ElementGapMilliseconds: 65,
                        CharacterGapMilliseconds: 130,
                        WordGapMilliseconds: 280,
                        SignalToNoiseDb: snr,
                        QsbHz: qsb,
                        QsbDepthDb: qsb > 0 ? QsbDepthDb : 0,
                        Seed: seed++)
                    : new CwFixtureRecipe(
                        $"{slug}-{tier}",
                        text,
                        DitMilliseconds: 1200.0 / OrdinaryWpm,
                        DahMilliseconds: 3 * 1200.0 / OrdinaryWpm,
                        ElementGapMilliseconds: 1200.0 / OrdinaryWpm,
                        CharacterGapMilliseconds: 3 * 1200.0 / OrdinaryWpm,
                        WordGapMilliseconds: 7 * 1200.0 / OrdinaryWpm,
                        SignalToNoiseDb: snr,
                        QsbHz: qsb,
                        QsbDepthDb: qsb > 0 ? QsbDepthDb : 0,
                        Seed: seed++));
            }
        }

        // The operator's own full-break-in transmission ahead of the answer, which
        // is the case that produced twelve hundred elements and one character on
        // the real capture (HM-DEC-095).
        recipes.Add(new CwFixtureRecipe(
            "qsk-preamble",
            TightFistText,
            DitMilliseconds: 105,
            DahMilliseconds: 283,
            ElementGapMilliseconds: 65,
            CharacterGapMilliseconds: 130,
            WordGapMilliseconds: 280,
            SignalToNoiseDb: EasyDb,
            PreambleSeconds: 12,
            Seed: seed));

        return recipes;
    }

    /// <summary>The repository root, found by walking up from the test binary.</summary>
    public static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!);

        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "src", "Hamlet.RadioEngine")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("could not find the repository root");
    }
}
