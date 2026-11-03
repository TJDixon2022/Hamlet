using System.Globalization;
using Hamlet.RadioEngine.Audio;

namespace Hamlet.RadioEngine.Tests.Cw.Fixtures;

/// <summary>The three kinds of gap a keyer sends.</summary>
public enum CwGapClass
{
    /// <summary>Between two marks of one letter.</summary>
    Inside,

    /// <summary>Between two letters of one word.</summary>
    Letter,

    /// <summary>Between two words.</summary>
    Word,
}

/// <summary>One mark of a text, and the gap after it: the boundary Morse puts there and the class its length is drawn from.</summary>
/// <param name="Dah">Whether the mark is a dah.</param>
/// <param name="Boundary">What the gap is in the text, or null after the last mark.</param>
/// <param name="Drawn">The class its length is drawn from, which a hand's habit may make another.</param>
public readonly record struct CwKeyedElement(bool Dah, CwGapClass? Boundary, CwGapClass? Drawn);

/// <summary>How a hand keys, each median in its own dits and each spread the standard deviation of the log (work instruction 592).</summary>
/// <param name="DitSpread">The spread of its dits.</param>
/// <param name="DahSpread">The spread of its dahs.</param>
/// <param name="Inside">Its inside gap.</param>
/// <param name="InsideSpread">That gap's spread.</param>
/// <param name="Letter">Its letter gap.</param>
/// <param name="LetterSpread">That gap's spread.</param>
/// <param name="Word">Its word gap.</param>
/// <param name="WordSpread">That gap's spread.</param>
/// <param name="DigitsAsWords">A gap between two digits is drawn from its word gaps.</param>
/// <param name="ShortWordsTogether">A gap after a one- or two-letter word is drawn from its letter gaps.</param>
public sealed record CwGapRecipe(
    double DitSpread, double DahSpread, double Inside, double InsideSpread, double Letter, double LetterSpread, double Word, double WordSpread,
    bool DigitsAsWords = false, bool ShortWordsTogether = false);

/// <summary>A wild bank recipe: a hand's name, whom it is measured from, its dit, its dah over its dit, and how it keys; <see cref="Gaps"/> null is the fist bank's own keyer.</summary>
public sealed record CwWildRecipe(string Name, string From, double DitMs, double Ratio, CwGapRecipe? Gaps)
{
    /// <summary>The recipe's gap medians in its own dits. The bank's keyer draws its letter gaps uniformly over 2 to 3.5 dits and its word gaps over 4 to 6, so its medians are 2.75 and 5.</summary>
    public (double Inside, double Letter, double Word) Medians => Gaps is { } g ? (g.Inside, g.Letter, g.Word) : (1, 2.75, 5);
}

/// <summary>One hand of the wild bank.</summary>
/// <param name="Recipe">How it keys.</param>
/// <param name="OwnSpeed">At the recipe's own dit, or scaled to 20 WPM.</param>
/// <param name="Db">The signal over the noise in the receiver's 520 Hz passband.</param>
/// <param name="Qso">Whether it sends the QSO text, or the fist bank's.</param>
public sealed record CwWildHand(CwWildRecipe Recipe, bool OwnSpeed, double Db, bool Qso)
{
    /// <summary>Its dit in seconds.</summary>
    public double Dit => (OwnSpeed ? Recipe.DitMs : CwWildBank.TwentyWpmDitMs) / 1000;

    /// <summary>Its speed, PARIS.</summary>
    public double Wpm => 1.2 / Dit;

    /// <summary>What it sends.</summary>
    public string Text => Qso ? CwWildBank.QsoText : CwFistBank.Text;

    /// <summary>Its name: <c>wild-{recipe}-{own|20wpm}-{db}db-{qso|bank}</c>.</summary>
    public string Name => string.Create(CultureInfo.InvariantCulture, $"wild-{Recipe.Name}-{(OwnSpeed ? "own" : "20wpm")}-{Db:0}db-{(Qso ? "qso" : "bank")}");

    /// <summary>The fist of the bank this hand is, where it is the bank's keyer sending the bank's text; else null.</summary>
    public CwFist? Fist => Recipe.Gaps is null && !Qso ? new CwFist(Recipe.Ratio, Wpm, Db, false) : null;

    /// <summary>Its seed: a hash of its name, as the bank's; a hand that is a fist of the bank has that fist's.</summary>
    public int Seed
    {
        get
        {
            if (Fist is { } fist)
            {
                return fist.Seed;
            }

            var hash = 2166136261u;

            foreach (var c in Name)
            {
                hash = (hash ^ c) * 16777619u;
            }

            return (int)(hash & 0x7FFFFFFF);
        }
    }

    /// <summary>Its key-down and key-up times.</summary>
    public double[] Edges() => Fist is { } fist ? CwFistBank.Edges(fist) : CwFistBank.Key(Text, Dit, Recipe.Ratio, Seed, Recipe.Gaps);

    /// <summary>Its audio, over the band as the bank's, flat.</summary>
    public MonoAudio Audio()
    {
        if (Fist is { } fist)
        {
            return CwFistBank.Audio(fist);
        }

        var recipe = new CwFixtureRecipe(
            Name, Text, DitMilliseconds: Dit * 1000, DahMilliseconds: Recipe.Ratio * Dit * 1000,
            SignalToNoiseDb: Db, ToneHz: CwFistBank.ToneHz, DriftHz: CwFistBank.DriftHz, QsbHz: 0, QsbDepthDb: 0, Seed: Seed);

        return CwFixtureGenerator.GenerateFromEdges(recipe, Edges());
    }

    /// <summary>The recipe line the survey prints.</summary>
    public string Line => string.Create(
        CultureInfo.InvariantCulture,
        $"{Recipe.Name}, from {Recipe.From}: dit {Dit * 1000:0} ms ({Wpm:0} WPM), dah {Recipe.Ratio:0.0} dits, {Db:0} dB in the passband, flat, {CwFistBank.ToneHz:0} Hz drifting {CwFistBank.DriftHz:0} Hz, seed {Seed}");

    /// <summary>What it keyed, gap by gap: each sent gap in seconds, with the class it was drawn from.</summary>
    public IReadOnlyList<(CwGapClass Drawn, double Seconds)> SentGaps()
    {
        var edges = Edges();
        var elements = CwFistBank.Walk(Text, Recipe.Gaps).ToList();
        var gaps = new List<(CwGapClass, double)>();

        for (var k = 0; k + 1 < elements.Count; k++)
        {
            gaps.Add((elements[k].Drawn!.Value, edges[(2 * k) + 2] - edges[(2 * k) + 1]));
        }

        return gaps;
    }

    /// <summary>What it keyed, mark by mark: whether each is a dah, and its seconds.</summary>
    public IReadOnlyList<(bool Dah, double Seconds)> SentMarks()
    {
        var edges = Edges();

        return CwFistBank.Walk(Text, Recipe.Gaps).Select((e, k) => (e.Dah, edges[(2 * k) + 1] - edges[2 * k])).ToList();
    }

    /// <summary>
    /// **HOW MUCH OF A HAND IS READABLE BY TIMING AT ALL** (work instruction 592, section 2): the share of its sent gaps within ±25% of
    /// the midpoint between its inside and letter medians, of the midpoint between its letter and word medians, and of either.
    /// </summary>
    public (double InsideLetter, double LetterWord, double Either) Ambiguity()
    {
        var (inside, letter, word) = Recipe.Medians;
        var low = (inside + letter) / 2 * Dit;
        var high = (letter + word) / 2 * Dit;
        var gaps = SentGaps();

        bool Near(double gap, double mid) => Math.Abs(gap - mid) <= 0.25 * mid;

        return (
            gaps.Count(g => Near(g.Seconds, low)) / (double)gaps.Count,
            gaps.Count(g => Near(g.Seconds, high)) / (double)gaps.Count,
            gaps.Count(g => Near(g.Seconds, low) || Near(g.Seconds, high)) / (double)gaps.Count);
    }
}

/// <summary>
/// **THE WILD BANK: HANDS WITH GAPS AS THEY ARE SENT** (work instruction 592, HM-DEC-296). The fist bank varies a hand's marks and keeps its
/// gaps close to Morse's own, so it holds no 1.6-dit inside gap, no digits spaced like words and no letter gaps spread from 2.5 to 5 dits, and
/// a reader that scores 3,898 on it reads Ken as <c>ETETTE TTTTTE</c>. Forty-eight hands generated at test time from the measured hands' recipes.
/// </summary>
/// <remarks>
/// The recipes' medians and spreads are the owner's analysis of 2026-11-03, as the work instruction's table gives them, not measured in this
/// tree. **<c>machine</c> is the control and is the fist bank's own keyer untouched**, so its letter gaps are 2 to 3.5 dits and its word gaps 4 to 6.
/// </remarks>
public static class CwWildBank
{
    /// <summary>The QSO text, in the air's own words.</summary>
    public const string QsoText = "GM KEN NICE TO MEET U ES TNX FER RPRT UR RST 579 579 I AM IN MOBILE STN HR NAME BRYAN BRYAN ES QTH IS CHICOPEE, MA";

    /// <summary>A dit at 20 WPM.</summary>
    public const double TwentyWpmDitMs = 60;

    /// <summary>The measured hands, and the control.</summary>
    public static readonly CwWildRecipe[] Recipes =
    [
        new("mobile", "Bryan", 65, 3.4, new CwGapRecipe(0.29, 0.12, 1.6, 0.10, 4.4, 0.30, 13.7, 0.40, DigitsAsWords: true, ShortWordsTogether: true)),
        new("bug", "Ken", 80, 2.9, new CwGapRecipe(0.37, 0.19, 1.7, 0.23, 4.3, 0.18, 7.6, 0.29)),
        new("farnsworth", "144020", 75, 2.9, new CwGapRecipe(0.28, 0.05, 0.9, 0.05, 5.3, 0.35, 21, 0.45)),
        new("tight", "121414", 58, 2.5, new CwGapRecipe(0.25, 0.16, 1.0, 0.08, 3.6, 0.25, 8.5, 0.40)),
        new("steady", "200157", 75, 2.8, new CwGapRecipe(0.10, 0.01, 1.1, 0.08, 4.0, 0.30, 11.8, 0.25)),

        // The control: the fist bank's own keyer. Its "own" dit is the bank's 15 WPM, since its other speed is 20.
        new("machine", "the fist bank", 80, 3.0, null),
    ];

    /// <summary>The levels.</summary>
    public static readonly double[] Levels = [20, 10];

    /// <summary>The forty-eight hands: every recipe, speed, level and text.</summary>
    public static IReadOnlyList<CwWildHand> All { get; } =
        (from recipe in Recipes
         from own in new[] { true, false }
         from db in Levels
         from qso in new[] { true, false }
         select new CwWildHand(recipe, own, db, qso)).ToList();
}
