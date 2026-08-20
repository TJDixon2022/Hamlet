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
    /// <summary>Keying ahead of a message, so the detector can acquire on it.</summary>
    /// <remarks>
    /// <para>**A FIXTURE SHORTER THAN THE DETECTOR'S ACQUISITION TESTS
    /// ACQUISITION, NOT DECODING** (§12.5). This detector wants keying before
    /// its tone tracker and its clock have both settled, and every one of these
    /// messages is under five seconds, so the part under test was competing for
    /// the seconds the decoder needed to find it at all. The characters lost
    /// without a run-up are always the opening ones.</para>
    /// <para>**ONE GROUP, MEASURED RATHER THAN PICKED.** Swept from nothing to
    /// eight groups: one is the least that satisfies the acquisition, and at
    /// twelve words a minute two or more make the decode worse and it never
    /// recovers. Choosing more would have buried a real decoder fault; that
    /// fault is in `OUTPUT.md` instead.</para>
    /// <para>`V` is what an operator actually sends to let somebody find him,
    /// which is the same problem and the same answer.</para>
    /// </remarks>
    public const string RunUp = "VVV ";

    public const string ExchangeText = "CQ CQ DE N0CALL N0CALL K";

    /// <summary>Prosigns, each sent as one character rather than two letters.</summary>
    public const string ProsignText = "^BT N0CALL ^AR ^SK";

    /// <summary>Letters, digits and the punctuation a contact actually uses.</summary>
    public const string CoverageText = "1234567890 QRZ? DE/N0CALL";

    /// <summary>What the two Farnsworth fixtures send.</summary>
    /// <remarks>
    /// An ordinary exchange with four words in it, because the point of these two
    /// is the spacing and a message with one word would exercise no word gap at
    /// all. The text is this session's; **the timing is not, and that is the whole
    /// difference** (HM-DEC-144, HM-DEC-145).
    /// </remarks>
    public const string FarnsworthText = "CQ DE N0CALL K";

    /// <summary>`N4L`'s dit, measured (HM-DEC-144).</summary>
    public const double HeavyFistDitMs = 56;

    /// <summary>`N4L`'s dah, measured: 4.24 dits (HM-DEC-144).</summary>
    public const double HeavyFistDahMs = 238;

    /// <summary>
    /// `N4L`'s element gap, measured at 36 ms against a 56 ms dit (HM-DEC-144).
    /// </summary>
    /// <remarks>
    /// **SHORTER THAN ITS OWN DIT**, which is the shape HM-DEC-115 measured off
    /// the air and the shape this pair of fixtures exists to put into the suite.
    /// </remarks>
    public const double HeavyFistElementGapMs = 36;

    /// <summary>`N4L`'s character gap, measured (HM-DEC-144).</summary>
    public const double HeavyFistCharacterGapMs = 165;

    /// <summary>
    /// What `N4L`'s word gap is taken to be, and it is the one figure here that
    /// was not measured.
    /// </summary>
    /// <remarks>
    /// **NEITHER ADJUDICATION CAUGHT A WORD GAP**, because both callsigns were
    /// read out of a single unbroken run of characters. Rather than invent one,
    /// this takes the ratio the generator's own default recipe already carries,
    /// 280 against 130, which was itself modelled on a real recording: 2.15 times
    /// the measured character gap. **It is an assumption and it says so here**,
    /// and a later adjudication that catches a word gap replaces it (§12.4).
    /// </remarks>
    public const double HeavyFistWordGapMs = 355;

    /// <summary>`VA3VRR`'s dit, measured (HM-DEC-145).</summary>
    public const double LightFistDitMs = 100;

    /// <summary>`VA3VRR`'s dah, measured: 2.73 dits (HM-DEC-145).</summary>
    public const double LightFistDahMs = 274;

    /// <summary>
    /// `VA3VRR`'s element gap, measured at 73 ms against a 100 ms dit
    /// (HM-DEC-145).
    /// </summary>
    public const double LightFistElementGapMs = 73;

    /// <summary>`VA3VRR`'s character gap, measured (HM-DEC-145).</summary>
    public const double LightFistCharacterGapMs = 150;

    /// <summary>
    /// What `VA3VRR`'s word gap is taken to be, on the same assumption as
    /// <see cref="HeavyFistWordGapMs"/> and for the same reason.
    /// </summary>
    public const double LightFistWordGapMs = 323;

    /// <summary>The tight fist's dit, in milliseconds.</summary>
    /// <remarks>
    /// <para>**NINETY-FOUR, AND A HUNDRED AND SIX WAS THE WINDOW'S ANSWER RATHER
    /// THAN THE STATION'S** (HM-DEC-101). This fixture was built from the figures
    /// published off capture 013347 — dit 106, dah 283 — and those figures are
    /// themselves a fifty millisecond window's measurement of that station. A
    /// window that long smears each keyed edge and the gate crosses its threshold
    /// early on the rise and late on the fall, so every mark reads about
    /// twenty-five milliseconds long and every gap about fifteen short.</para>
    /// <para>Building the fixture from the measurement and then measuring the
    /// fixture applies that bias **twice**, and the second application is what
    /// pushed it under the reference's own 2.5 ratio floor. Swept against window
    /// length, the real capture reads:</para>
    /// <list type="bullet">
    /// <item>50 ms window: dit 113, dah 287, ratio 2.54</item>
    /// <item>30 ms window: dit 102, dah 280, ratio 2.75</item>
    /// <item>10 ms window: dit **94**, dah **273**, ratio **2.92**</item>
    /// </list>
    /// <para>The generator was never wrong. It produced exactly what it was
    /// asked for, which a ten millisecond window confirms to the millisecond.
    /// **It was asked for the wrong numbers**, which is the same fault as a
    /// fixture built from the same misunderstanding as the code it certifies
    /// (§12.5), wearing different clothes.</para>
    /// </remarks>
    public const double TightFistDitMs = 94;

    /// <summary>The tight fist's dah.</summary>
    /// <remarks>A true ratio of 2.92, which survives the measurement bias at
    /// about 2.54 where 2.70 did not.</remarks>
    public const double TightFistDahMs = 273;

    /// <summary>
    /// The silence inside one of its characters, which is the whole point.
    /// </summary>
    /// <remarks>
    /// Eighty milliseconds against a ninety-four millisecond dit: **still shorter
    /// than its own dits**, which is the property this message exists to carry
    /// and which no textbook spacing produces. Measured as the median of
    /// thirty-two element gaps in the capture, range forty to a hundred.
    /// </remarks>
    public const double TightFistElementGapMs = 80;

    /// <summary>The silence between its characters.</summary>
    /// <remarks>Median of four, range a hundred and forty-five to two hundred
    /// and thirty.</remarks>
    public const double TightFistCharacterGapMs = 162;

    /// <summary>The silence between its words.</summary>
    public const double TightFistWordGapMs = 265;

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

    /// <summary>The station that calls, in the two-station fixture.</summary>
    /// <remarks>
    /// About eleven words a minute at 615 Hz, which is the fist and the pitch
    /// measured off capture 013347.
    /// </remarks>
    public static CwFixtureRecipe Caller { get; } = new(
        "two-station-first",
        "CQ CQ DE N0CALL K",
        DitMilliseconds: 1200.0 / 11,
        DahMilliseconds: 3 * 1200.0 / 11,
        ElementGapMilliseconds: 1200.0 / 11,
        CharacterGapMilliseconds: 3 * 1200.0 / 11,
        WordGapMilliseconds: 7 * 1200.0 / 11,
        SignalToNoiseDb: WorkingDb,
        ToneHz: 615,
        Seed: 7001);

    /// <summary>The station that answers, at a different speed and pitch.</summary>
    /// <remarks>
    /// Twenty-two words a minute at 730 Hz. **Twice the speed and a hundred and
    /// fifteen hertz away**, because that is what a different operator answering
    /// actually sounds like, and because it is the only way to exercise clock
    /// loss and tracker switching in one recording.
    /// </remarks>
    public static CwFixtureRecipe Answerer { get; } = new(
        "two-station-second",
        "N0CALL DE W1XYZ K",
        DitMilliseconds: 1200.0 / 22,
        DahMilliseconds: 3 * 1200.0 / 22,
        ElementGapMilliseconds: 1200.0 / 22,
        CharacterGapMilliseconds: 3 * 1200.0 / 22,
        WordGapMilliseconds: 7 * 1200.0 / 22,
        SignalToNoiseDb: WorkingDb,
        ToneHz: 730,
        Seed: 7002);

    /// <summary>What the joined two-station recording is called.</summary>
    public const string TwoStationName = "two-station";

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

    /// <summary>The speed the fast messages are sent at (HM-DEC-103).</summary>
    /// <remarks>
    /// <para>**TWENTY-FIVE, AND THIRTY-FIVE WAS REJECTED AS SCOPE INVENTED AT A
    /// TEST BENCH.** Nothing has yet been decoded above twenty on the air, so a
    /// claim about thirty-five would be a number with no evidence under it, which
    /// is the same defect as a decode with no signal under it.</para>
    /// <para>A dit at this speed is forty-eight milliseconds, which is where the
    /// settled pass's window stops being governed by its two-and-a-half second
    /// floor and starts being governed by its thirty-element one. **No test has
    /// ever exercised that path**, and new failures here are wanted rather than
    /// feared.</para>
    /// </remarks>
    public const double FastWpm = 25;

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

        var fastTiers = new[]
        {
            ("easy", EasyDb, 0.0),
            ("working", WorkingDb, QsbHz),
            ("edge", EdgeDb, 0.0),
        };

        var seed = 20260817;

        foreach (var (slug, text, tight) in messages)
        {
            foreach (var (tier, snr, qsb) in tiers)
            {
                // **THE RUN-UP GOES ON THE EASY TIER AND NOWHERE ELSE.** That is
                // where HM-DEC-114's bar applies — fifteen decibels, steady
                // fist, the whole message — and it is the tier whose failures
                // were all lost opening characters. The working and edge tiers
                // assert how the decoder degrades rather than that it reads
                // everything, and they were passing.
                //
                // Applying it to all three was tried and measured: the easy
                // tiers held or improved, and two working tiers fell through the
                // reference gate, coverage from 52% to 8% and tightfist from 73%
                // to 36%. Rebuilding a fixture that was not failing is churn
                // that invalidates a reference score for nothing (§12.5,
                // §12.6).
                // **AND NOT ON THE PROSIGNS FIXTURE, WHICH IT BREAKS.** With a
                // run-up in front of it the tone survey stops finding the tone
                // at all and the decode is empty: `TheToneIsFoundInRealisticAudio`
                // fails alongside the bar. A prosign is one long unbroken symbol,
                // so `VVV` in front of it gives the survey a run of short marks
                // followed by a run of very long ones, and the mark-length
                // clustering that separates keying from a carrier (HM-DEC-095)
                // sees one smear rather than two groups.
                //
                // That fixture's real fault is a wrong character rather than a
                // missing opening — it reads `IR` where `AR` was sent — and a
                // run-up would have hidden it behind an empty decode instead of
                // fixing it (§12.5).
                var message = tier == "easy" && slug != "prosigns"
                    ? RunUp + text
                    : text;

                recipes.Add(tight
                    ? new CwFixtureRecipe(
                        $"{slug}-{tier}",
                        message,
                        DitMilliseconds: TightFistDitMs,
                        DahMilliseconds: TightFistDahMs,
                        ElementGapMilliseconds: TightFistElementGapMs,
                        CharacterGapMilliseconds: TightFistCharacterGapMs,
                        WordGapMilliseconds: TightFistWordGapMs,
                        SignalToNoiseDb: snr,
                        QsbHz: qsb,
                        QsbDepthDb: qsb > 0 ? QsbDepthDb : 0,
                        Seed: seed++)
                    : new CwFixtureRecipe(
                        $"{slug}-{tier}",
                        message,
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

        // Twenty-five words a minute, all three tiers, replacing the one old
        // fixture that carries fast CW at all (HM-DEC-103).
        foreach (var (tier, snr, qsb) in fastTiers)
        {
            recipes.Add(new CwFixtureRecipe(
                $"fast-{tier}",
                ExchangeText,
                DitMilliseconds: 1200.0 / FastWpm,
                DahMilliseconds: 3 * 1200.0 / FastWpm,
                ElementGapMilliseconds: 1200.0 / FastWpm,
                CharacterGapMilliseconds: 3 * 1200.0 / FastWpm,
                WordGapMilliseconds: 7 * 1200.0 / FastWpm,
                SignalToNoiseDb: snr,
                QsbHz: qsb,
                QsbDepthDb: qsb > 0 ? QsbDepthDb : 0,
                Seed: seed++));
        }

        // **THE TWO-STATION FIXTURE IS GATED THROUGH ITS SEGMENTS** (HM-DEC-104).
        // The reference is a single-pass batch decoder with no notion of a second
        // station: handed the joined recording it acquires whichever one it
        // prefers and reads the whole file with that one clock, which says
        // nothing about either half. Each segment is therefore generated and
        // gated on its own, and the joined file is those two proved halves with a
        // stretch of band between them.
        recipes.Add(Caller);
        recipes.Add(Answerer);

        // The operator's own full-break-in transmission ahead of the answer, which
        // is the case that produced twelve hundred elements and one character on
        // the real capture (HM-DEC-095).
        // **THE MESSAGE HAS TO OUTWEIGH THE SLIVERS, AS IT DOES ON THE AIR.**
        // Band noise audible between the operator's own elements crosses the gate
        // now and then and arrives as a handful of short marks: the real capture
        // shows exactly this, as the six placeholders standing in front of
        // MVRRVA3VRR. It is harmless there because the answering station sends
        // about thirty-three marks against those six. A message of nineteen marks
        // behind the same preamble leaves four pieces of noise weighing a fifth of
        // the clock fit, which dragged the ratio to 2.49 against a floor of 2.50.
        recipes.Add(new CwFixtureRecipe(
            "qsk-preamble",
            TightFistText + " " + TightFistText,
            DitMilliseconds: TightFistDitMs,
            DahMilliseconds: TightFistDahMs,
            ElementGapMilliseconds: TightFistElementGapMs,
            CharacterGapMilliseconds: TightFistCharacterGapMs,
            WordGapMilliseconds: TightFistWordGapMs,
            SignalToNoiseDb: EasyDb,
            PreambleSeconds: 12,
            Seed: seed));

        // **TWO FISTS THIS PROJECT HAS PROVED ON THE AIR, SENT BY THE
        // GENERATOR** (HM-DEC-144, HM-DEC-145). Every other message above is
        // either textbook or the one tight fist, and until now the suite could
        // not catch a decoder that handled only one style of spacing. These two
        // are cut to the millisecond from the only two recordings whose timing is
        // adjudicated rather than estimated, and they are deliberately far apart:
        // `N4L` sends a dah of 4.24 dits at twenty-one words a minute, `VA3VRR`
        // 2.73 at twelve.
        recipes.Add(new CwFixtureRecipe(
            "farnsworth-heavy",
            FarnsworthText,
            DitMilliseconds: HeavyFistDitMs,
            DahMilliseconds: HeavyFistDahMs,
            ElementGapMilliseconds: HeavyFistElementGapMs,
            CharacterGapMilliseconds: HeavyFistCharacterGapMs,
            WordGapMilliseconds: HeavyFistWordGapMs,
            SignalToNoiseDb: EasyDb,
            Seed: seed + 1));

        recipes.Add(new CwFixtureRecipe(
            "farnsworth-light",
            FarnsworthText,
            DitMilliseconds: LightFistDitMs,
            DahMilliseconds: LightFistDahMs,
            ElementGapMilliseconds: LightFistElementGapMs,
            CharacterGapMilliseconds: LightFistCharacterGapMs,
            WordGapMilliseconds: LightFistWordGapMs,
            SignalToNoiseDb: EasyDb,
            Seed: seed + 2));

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
