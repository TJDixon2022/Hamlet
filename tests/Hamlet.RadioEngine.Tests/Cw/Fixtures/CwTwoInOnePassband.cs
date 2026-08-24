using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;

namespace Hamlet.RadioEngine.Tests.Cw.Fixtures;

/// <summary>What one offset and one level did to the wanted station's text.</summary>
/// <param name="OffsetHz">How far the competing station sat from the wanted one.</param>
/// <param name="LevelDb">How loud it was relative to the wanted one.</param>
/// <param name="Correct">Characters read as sent, of the wanted station's text.</param>
/// <param name="Wrong">Characters read where a different one was sent.</param>
/// <param name="Invented">Characters read where nothing was sent at all.</param>
/// <param name="Emitted">How many characters came out, word gaps excluded.</param>
/// <param name="EShare">The share of those that are <c>E</c>.</param>
/// <param name="Text">What was read, for a person to look at.</param>
internal readonly record struct CwTwoStationReading(
    double OffsetHz,
    double LevelDb,
    int Correct,
    int Wrong,
    int Invented,
    int Emitted,
    double EShare,
    string Text);

/// <summary>
/// Two senders in one passband, swept across offset and level.
/// </summary>
/// <remarks>
/// <para>**THE MEASUREMENT COMES BEFORE THE CHANGE**, so that a change to the
/// front end is judged rather than illustrated. This runs identically before and
/// after, and the only thing between the two runs is the integrator.</para>
/// <para>**BOTH PATHS ARE MEASURED AND THEY ANSWER DIFFERENT QUESTIONS.** The
/// production path includes the tone tracker, so if the tracker walks off to the
/// competing station the text collapses for a reason that has nothing to do with
/// the filter. The offline path is told where the wanted station is and never
/// moves, which isolates what the integrator rejects. A front-end change is
/// judged on the second and lived with on the first.</para>
/// <para>**NO REAL CALLSIGNS.** `N0CALL` and `N0AAA` are the reserved example
/// calls; the older `CwFixtures` set uses `W1AW`, which is a real station's, and
/// that is noted rather than changed here (§12.6).</para>
/// </remarks>
internal static class CwTwoInOnePassband
{
    /// <summary>The wanted station's text, and the answer key.</summary>
    public const string WantedText = "CQ DE N0CALL K";

    /// <summary>What the competing station sends.</summary>
    /// <remarks>
    /// Different text, a different speed and a different lead-in, so the two are
    /// not accidentally keying in step with one another.
    /// </remarks>
    public const string OtherText = "DE N0AAA UP";

    /// <summary>Where the wanted station sits.</summary>
    public const double WantedToneHz = 600;

    /// <summary>The offsets swept, in hertz.</summary>
    public static IReadOnlyList<double> Offsets { get; }
        = new[] { 40.0, 80.0, 120.0, 200.0, 300.0 };

    /// <summary>The competing station's levels, in decibels against the wanted one.</summary>
    public static IReadOnlyList<double> Levels { get; }
        = new[] { 0.0, -6.0, -12.0 };

    /// <summary>The wanted station on its own, as the control.</summary>
    /// <returns>The audio.</returns>
    /// <remarks>
    /// **THE ROW THAT DECIDES WHAT THIS UNIT IS ABOUT.** Without it, soup on a
    /// two-station recording reads as the second station's doing, and the whole
    /// question is begged. It is the same recipe with the competing station left
    /// out and the same seed, so the band underneath is the same band.
    /// </remarks>
    public static MonoAudio Alone()
        => CwFixtureGenerator.Generate(Wanted()).Audio;

    /// <summary>How long both stations have their keys down at once.</summary>
    /// <param name="offsetHz">The offset.</param>
    /// <param name="levelDb">The level.</param>
    /// <returns>Seconds keyed together, and seconds each is keyed alone.</returns>
    /// <remarks>
    /// **A FIXTURE WHERE THE TWO NEVER COLLIDE PROVES NOTHING ABOUT REJECTION,
    /// AND LOOKS EXACTLY LIKE ONE THAT DOES** (§12.5). Measured through the
    /// decoder's own front end pointed at each station in turn, each envelope cut
    /// at its own midpoint. A crude tone measurement over a short window cannot
    /// resolve two notes forty hertz apart and reads both as keyed throughout,
    /// which is a way to certify a fixture that is not testing anything.
    /// </remarks>
    public static (double Both, double WantedAlone, double OtherAlone) Overlap(
        double offsetHz, double levelDb)
    {
        var audio = Audio(offsetHz, levelDb);

        var wanted = CwProbabilisticDecoder.Envelope(
            audio.Samples, audio.SampleRate, WantedToneHz);

        var other = CwProbabilisticDecoder.Envelope(
            audio.Samples, audio.SampleRate, WantedToneHz + offsetHz);

        var wantedCut = (wanted.Max() + wanted.Min()) / 2;
        var otherCut = (other.Max() + other.Min()) / 2;
        var hop = CwProbabilisticDecoder.HopMilliseconds / 1000.0;

        double both = 0, wantedOnly = 0, otherOnly = 0;

        for (var n = 0; n < Math.Min(wanted.Length, other.Length); n++)
        {
            var w = wanted[n] > wantedCut;
            var o = other[n] > otherCut;

            if (w && o)
            {
                both += hop;
            }
            else if (w)
            {
                wantedOnly += hop;
            }
            else if (o)
            {
                otherOnly += hop;
            }
        }

        return (both, wantedOnly, otherOnly);
    }

    /// <summary>Score any decode against the wanted station's text.</summary>
    /// <param name="characters">What came out.</param>
    /// <returns>The reading, with no offset or level attached.</returns>
    public static CwTwoStationReading ScoreAgainstWanted(
        IReadOnlyList<CwCharacter> characters)
        => Score(double.NaN, double.NaN, characters);

    /// <summary>Turn an offline result into characters the scorer can read.</summary>
    /// <param name="result">The offline decode.</param>
    /// <returns>The characters.</returns>
    public static IReadOnlyList<CwCharacter> AsCharacters(
        CwProbabilisticResult result)
        => result.Characters
            .Select(c => new CwCharacter(
                c.Text,
                c.Text == "#" ? CwConfidence.Unreadable : CwConfidence.High,
                result.LikelihoodRatio,
                c.Pattern,
                double.NaN,
                (int)Math.Round(result.WordsPerMinute),
                TimeSpan.Zero)
            {
                SpanLogLikelihoodRatio = c.SpanLogLikelihoodRatio,
            })
            .ToList();

    /// <summary>The wanted station's recipe.</summary>
    private static CwFixtureRecipe Wanted() => new(
        Name: "two-in-one",
        Text: WantedText,
        DitMilliseconds: 1200.0 / 18,
        DahMilliseconds: 3 * 1200.0 / 18,
        ElementGapMilliseconds: 1200.0 / 18,
        CharacterGapMilliseconds: 3 * 1200.0 / 18,
        WordGapMilliseconds: 7 * 1200.0 / 18,
        SignalToNoiseDb: 15,
        ToneHz: WantedToneHz,
        Seed: 20260823);

    /// <summary>Build one combination's audio.</summary>
    /// <param name="offsetHz">How far above the wanted station the other one sits.</param>
    /// <param name="levelDb">How loud it is relative to the wanted one.</param>
    /// <returns>The audio.</returns>
    public static MonoAudio Audio(double offsetHz, double levelDb)
    {
        var wanted = Wanted();

        // The competing station keys faster and starts later, so its marks land
        // inside the wanted station's rather than beside them. Its own
        // signal-to-noise figure is ignored: Together sets its level from the
        // wanted station instead.
        var other = wanted with
        {
            Text = OtherText,
            DitMilliseconds = 1200.0 / 24,
            DahMilliseconds = 3 * 1200.0 / 24,
            ElementGapMilliseconds = 1200.0 / 24,
            CharacterGapMilliseconds = 3 * 1200.0 / 24,
            WordGapMilliseconds = 7 * 1200.0 / 24,
            ToneHz = WantedToneHz + offsetHz,
            PreambleSeconds = 0.35,
            Seed = 20260823 ^ 0x5EED,
        };

        return CwFixtureGenerator.Together(wanted, other, levelDb).Audio;
    }

    /// <summary>Read one combination through the production path.</summary>
    /// <param name="offsetHz">The offset.</param>
    /// <param name="levelDb">The level.</param>
    /// <returns>What survived.</returns>
    /// <remarks>
    /// The tracker runs, so this is what the operator would get. It is started at
    /// the wanted station's pitch, which is the kindest honest starting point:
    /// telling it to look elsewhere would measure acquisition rather than
    /// rejection.
    /// </remarks>
    public static CwTwoStationReading Tracked(double offsetHz, double levelDb)
        => Score(
            offsetHz,
            levelDb,
            CwDecodeHarness.Decode(Audio(offsetHz, levelDb), WantedToneHz)
                .Characters);

    /// <summary>Read one combination at a fixed pitch, with no tracker.</summary>
    /// <param name="offsetHz">The offset.</param>
    /// <param name="levelDb">The level.</param>
    /// <returns>What survived.</returns>
    /// <remarks>
    /// **THIS IS THE ONE A FRONT-END CHANGE IS JUDGED ON.** Nothing moves the
    /// filter, so the only thing standing between the competing station and the
    /// envelope is the integrator.
    /// </remarks>
    public static CwTwoStationReading Fixed(double offsetHz, double levelDb)
    {
        var result = CwProbabilisticDecoder.Decode(
            Audio(offsetHz, levelDb), WantedToneHz);

        return Score(offsetHz, levelDb, AsCharacters(result));
    }

    private static CwTwoStationReading Score(
        double offsetHz, double levelDb, IReadOnlyList<CwCharacter> characters)
    {
        var matches = CwAlignment.Align(characters, WantedText);
        var letters = characters.Where(c => !c.IsWordGap).ToList();

        var eShare = letters.Count == 0
            ? double.NaN
            : (double)letters.Count(
                c => string.Equals(c.Text, "E", StringComparison.Ordinal))
              / letters.Count;

        return new CwTwoStationReading(
            offsetHz,
            levelDb,
            matches.Count(m => m.Kind == CwMatchKind.Correct && !m.Decoded.IsWordGap),
            matches.Count(m => m.Kind == CwMatchKind.Wrong && !m.Decoded.IsWordGap),
            matches.Count(m => m.Kind == CwMatchKind.Invented && !m.Decoded.IsWordGap),
            letters.Count,
            eShare,
            string.Concat(characters.Select(c => c.Text)));
    }
}
