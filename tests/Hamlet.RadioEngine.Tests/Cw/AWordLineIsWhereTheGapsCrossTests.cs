using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **A WORD LINE IS WHERE THE SENDER'S OWN LETTER AND WORD GAPS CROSS** (work instruction 538, HM-DEC-242): a hand whose
/// letter and word gaps overlap still has its words read where it put them, and a slow-spacing sender is not split at
/// every letter.
/// </summary>
/// <remarks>
/// Synthetic hands keyed on band noise at 20 dB: every dit and dah scattered a little, every letter gap and word gap drawn
/// at random from its own range in dits. Nothing here is a recording.
/// </remarks>
public sealed class AWordLineIsWhereTheGapsCrossTests
{
    private const int Rate = 8000;

    private const string Message = "GE OM TNX FER CALL UR RST 579 NAME TIM QTH OHIO HW CPY BK";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where each reading is printed.</param>
    public AWordLineIsWhereTheGapsCrossTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// A hand: the message at a speed, dits and dahs scattered by a tenth, gaps inside letters one dit, letter gaps and
    /// word gaps drawn evenly from their ranges in dits.
    /// </summary>
    internal static List<(double From, double To)> Hand(string text, int wpm, (double Lo, double Hi) letter, (double Lo, double Hi) word, int seed)
    {
        var patterns = MorseAlphabet.All.Where(p => p.Value.Length == 1).ToDictionary(p => p.Value[0], p => p.Key);
        var random = new Random(seed);
        var dit = 1.2 / wpm;
        var marks = new List<(double, double)>();
        var at = 2.0;

        double Scatter(double length) => length * (1 + (0.1 * ((2 * random.NextDouble()) - 1)));

        double Between((double Lo, double Hi) range) => dit * (range.Lo + ((range.Hi - range.Lo) * random.NextDouble()));

        var words = text.Split(' ');

        for (var w = 0; w < words.Length; w++)
        {
            for (var l = 0; l < words[w].Length; l++)
            {
                var pattern = patterns[words[w][l]];

                for (var e = 0; e < pattern.Length; e++)
                {
                    var length = Scatter(pattern[e] == '-' ? 3 * dit : dit);

                    marks.Add((at, at + length));
                    at += length + (e + 1 < pattern.Length ? Scatter(dit) : 0);
                }

                at += l + 1 < words[w].Length ? Between(letter) : 0;
            }

            at += w + 1 < words.Length ? Between(word) : 0;
        }

        return marks;
    }

    private string Read(int wpm, (double Lo, double Hi) letter, (double Lo, double Hi) word, int seed)
    {
        var marks = Hand(Message, wpm, letter, word, seed);
        var audio = TheShapePicksTheSenderTests.Noise(marks[^1].To + 3, seed);

        TheShapePicksTheSenderTests.Key(audio, marks, 650, 20);

        var text = TheRecordingsScoreboardTests.ReadLive(audio, Rate).Text;
        var spaces = TheRecordingsScoreboardTests.SpacesOf(text, Message);

        _output.WriteLine($"{wpm} WPM, letter gaps {letter.Lo}-{letter.Hi} dits, word gaps {word.Lo}-{word.Hi}, seed {seed}: `{text}`; spaces {spaces.Right} of {spaces.OfReference}, {spaces.Missing} missing, {spaces.Added} added; letters {TheRecordingsScoreboardTests.Right(text, Message)} of {TheRecordingsScoreboardTests.Letters(Message).Length}");

        return text;
    }

    /// <remarks>
    /// Work instruction 538, task 2: a hand whose letter gaps reach 5.5 dits and word gaps start at 6, at 20 WPM. The two
    /// clusters touch and no two neighbouring gaps differ by √(7/3), so before this unit every gap was one letter cluster and
    /// the words ran together: 4, 7 and 7 of 14 word spaces read. Now 12, 14 and 13, and at most one space added: where the
    /// two ranges touch, a letter gap drawn at 5.4 dits sits past the equal-error line, as an ear would hear it too.
    /// </remarks>
    [Theory]
    [InlineData(5381)]
    [InlineData(5382)]
    [InlineData(5383)]
    public void AHandWhoseLetterAndWordGapsTouchKeepsItsWords(int seed)
    {
        var text = Read(20, (3, 5.5), (6, 10), seed);
        var spaces = TheRecordingsScoreboardTests.SpacesOf(text, Message);

        Assert.True(spaces.Added <= 1, $"{spaces.Added} spaces added");
        Assert.True(spaces.Right >= spaces.OfReference - 2, $"{spaces.Right} of {spaces.OfReference} word spaces read");
    }

    /// <remarks>
    /// Work instruction 538, task 3: a hand at 18 WPM whose letter gaps run 4 to 5 dits and word gaps 8 to 12. Its letters
    /// must not be split one from another: at three seeds, no space is added inside a word, and its words are read. This
    /// hand read whole before the unit too, so it is a guard, not the fault: the fault was found on the owner's 22:15:30
    /// station, whose word cluster is wide (see <c>TheCrossingIsWhereEitherClusterIsAsLikely</c>).
    /// </remarks>
    [Theory]
    [InlineData(5391)]
    [InlineData(5392)]
    [InlineData(5393)]
    public void ASlowSpacingSenderIsNotSplitAtEveryLetter(int seed)
    {
        var text = Read(18, (4, 5), (8, 12), seed);
        var spaces = TheRecordingsScoreboardTests.SpacesOf(text, Message);

        Assert.Equal(0, spaces.Added);
        Assert.True(spaces.Right >= spaces.OfReference - 2, $"{spaces.Right} of {spaces.OfReference} word spaces read");
    }

    /// <remarks>
    /// Both hands with the two word-line rules off, as they read before this unit. Asserts nothing; the readings are the result.
    /// </remarks>
    [Fact]
    public void BothHandsBeforeThisUnit()
    {
        using var _ = CwRules.Off(CwRules.OverlapSplit, CwRules.GapCrossing);

        foreach (var seed in new[] { 5381, 5382, 5383 })
        {
            Read(20, (3, 5.5), (6, 10), seed);
        }

        foreach (var seed in new[] { 5391, 5392, 5393 })
        {
            Read(18, (4, 5), (8, 12), seed);
        }
    }

    /// <remarks>
    /// The crossing on its own figures: two clusters of equal spread cross at their geometric middle, and a wide word
    /// cluster no longer pulls the line onto a tight letter cluster.
    /// </remarks>
    [Fact]
    public void TheCrossingIsWhereEitherClusterIsAsLikely()
    {
        var even = CwPatternGate.Crossing([0.27, 0.30, 0.33], [0.63, 0.70, 0.77]);

        Assert.InRange(even, Math.Sqrt(0.30 * 0.70) * 0.97, Math.Sqrt(0.30 * 0.70) * 1.03);

        var letters = new[] { 0.111, 0.125, 0.141, 0.150, 0.160, 0.171 };
        var words = new[] { 0.184, 0.250, 0.400, 0.700, 1.000, 1.361 };
        var crossing = CwPatternGate.Crossing(letters, words);
        var boundary = CwSenderGate.Boundary(CwSenderGate.LogStats(letters), CwSenderGate.LogStats(words));

        _output.WriteLine($"crossing {crossing * 1000:0} ms, the spread-count boundary {boundary * 1000:0} ms");

        Assert.True(crossing > letters.Max(), $"crossing {crossing * 1000:0} ms");
        Assert.True(crossing > boundary);
    }
}
