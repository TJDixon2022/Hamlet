using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Tests.Cw.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **THE WILD BANK IS WHAT ITS RECIPES SAY** (work instruction 592, HM-DEC-296): forty-eight hands, each named and seeded once, each
/// keying the text it claims, and the control the fist bank's own keyer bit for bit. There is no score guard: the survey's figures are
/// the baseline, and these tests prove only that the measure measures what it says.
/// </summary>
public sealed class TheWildBankTests(ITestOutputHelper output)
{
    /// <summary>The ninety-six fists' edges, hashed with the keyer as it stood at 1.13.278, before it gained a gap recipe.</summary>
    private const string BankEdges = "5671ABEF7EF49455465D7B05F54764CA393187F6773BC13247E0C32AD2635C75";

    /// <summary>The ninety-six fists' audio, hashed at the same commit.</summary>
    private const string BankAudio = "2CA8BE5E6346836722F47213582394E738581E6714BCB160AB881DED646DAAEF";

    /// <remarks>
    /// Every hand is generated once, and what it keys spells the text it claims: its marks and boundaries read through Morse's own table,
    /// a hand's habits moving only what a gap's length is drawn from.
    /// </remarks>
    [Fact]
    public void TheBankIsMade()
    {
        Assert.Equal(48, CwWildBank.All.Count);
        Assert.Equal(48, CwWildBank.All.Select(h => h.Name).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(48, CwWildBank.All.Select(h => h.Seed).Distinct().Count());

        foreach (var hand in CwWildBank.All)
        {
            var elements = CwFistBank.Walk(hand.Text, hand.Recipe.Gaps).ToList();
            var edges = hand.Edges();
            var text = new System.Text.StringBuilder();
            var letter = new System.Text.StringBuilder();

            foreach (var e in elements)
            {
                letter.Append(e.Dah ? '-' : '.');

                if (e.Boundary != CwGapClass.Inside)
                {
                    text.Append(MorseAlphabet.Lookup(letter.ToString()));
                    letter.Clear();

                    if (e.Boundary == CwGapClass.Word)
                    {
                        text.Append(' ');
                    }
                }
            }

            Assert.Equal(hand.Text, text.ToString());
            Assert.Equal(2 * elements.Count, edges.Length);
            Assert.All(edges.Zip(edges.Skip(1)), p => Assert.True(p.Second > p.First, $"{hand.Name}: an edge out of order"));
        }
    }

    /// <remarks>
    /// The recipes are what is sent: every class of every hand with a recipe sits near the recipe's median, the habits draw what they
    /// say, and a hand with none draws its digits and short words from the classes they are.
    /// </remarks>
    [Fact]
    public void TheHandsSendTheirRecipes()
    {
        foreach (var hand in CwWildBank.All.Where(h => h.Recipe.Gaps is not null && h.Qso))
        {
            var gaps = hand.SentGaps();
            var (inside, letter, word) = hand.Recipe.Medians;

            double Median(CwGapClass c)
            {
                var these = gaps.Where(g => g.Drawn == c).Select(g => g.Seconds).Order().ToList();

                return these[these.Count / 2] / hand.Dit;
            }

            Assert.InRange(Median(CwGapClass.Inside) / inside, 0.85, 1.18);
            Assert.InRange(Median(CwGapClass.Letter) / letter, 0.75, 1.33);
            Assert.InRange(Median(CwGapClass.Word) / word, 0.65, 1.55);
        }

        var mobile = CwFistBank.Walk(CwWildBank.QsoText, CwWildBank.Recipes[0].Gaps).Where(e => e.Boundary != e.Drawn).ToList();
        var bug = CwFistBank.Walk(CwWildBank.QsoText, CwWildBank.Recipes[1].Gaps).Where(e => e.Boundary != e.Drawn).ToList();

        // `579 579` holds four gaps between digits; `GM`, `TO`, `U`, `ES`, `UR`, `I`, `AM`, `IN`, `HR`, `ES`, `IS` are the short words.
        Assert.Equal(4, mobile.Count(e => e.Boundary == CwGapClass.Letter && e.Drawn == CwGapClass.Word));
        Assert.Equal(11, mobile.Count(e => e.Boundary == CwGapClass.Word && e.Drawn == CwGapClass.Letter));
        Assert.Empty(bug);
    }

    /// <remarks>
    /// **THE CONTROL IS THE BANK, BIT FOR BIT.** The hashes were taken from the keyer at 1.13.278 before it was touched: every edge and every
    /// audio sample of all ninety-six fists hash the same through the keyer as it stands, and each machine hand sending the bank's text
    /// is that fist's edges and audio exactly.
    /// </remarks>
    [Fact]
    public void TheMachineRecipeIsTheBank()
    {
        using var edges = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        using var audio = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

        foreach (var fist in CwFistBank.All)
        {
            edges.AppendData(MemoryMarshal.AsBytes(CwFistBank.Edges(fist).AsSpan()));
            audio.AppendData(MemoryMarshal.AsBytes(CwFistBank.Audio(fist).Samples.AsSpan()));
        }

        Assert.Equal(BankEdges, Convert.ToHexString(edges.GetHashAndReset()));
        Assert.Equal(BankAudio, Convert.ToHexString(audio.GetHashAndReset()));

        var machine = CwWildBank.All.Where(h => h.Recipe.Gaps is null && !h.Qso).ToList();

        Assert.Equal(4, machine.Count);

        foreach (var hand in machine)
        {
            var fist = CwFistBank.All.Single(f => f.Name == hand.Fist!.Name);

            Assert.Equal(CwFistBank.Edges(fist), hand.Edges());
            Assert.Equal(CwFistBank.Audio(fist).Samples, hand.Audio().Samples);
        }
    }

    /// <remarks>The alignment: a `?` takes any run at no cost and counts no letter; everything else scores as the board's scorer does.</remarks>
    [Fact]
    public void AnUnknownMatchesAnything()
    {
        var none = new HashSet<int>();

        foreach (var (decode, key) in new[] { ("CQDEW1AW", "CQDEW1AW"), ("XCQDEW1AWY", "CQDEW1AW"), ("CQDXW1AW", "CQDEW1AW"), ("CQW1AW", "CQDEW1AW"), ("CQDDEW1AW", "CQDEW1AW") })
        {
            Assert.Equal(CwScorer.Within(decode, key, CwKeyKind.Exact).Edits, CwWildHands.Align(decode, key, none).Edits);
        }

        var (kinds, _, edits, unknowns) = CwWildHands.Align("ABXYZCD", "ABCD", new HashSet<int> { 2 });

        Assert.Equal(0, edits);
        Assert.Equal(new[] { CwWildHands.LetterIs.Same, CwWildHands.LetterIs.Same, CwWildHands.LetterIs.Unknown, CwWildHands.LetterIs.Unknown, CwWildHands.LetterIs.Unknown, CwWildHands.LetterIs.Same, CwWildHands.LetterIs.Same }, kinds);
        Assert.Equal(["XYZ"], unknowns);
    }

    /// <remarks>The truth file parses as the two stretches the work instruction names, the notes left out.</remarks>
    [Fact]
    public void TheDraftTruthIsBesideTheCapture()
    {
        var truths = CwWildHands.Truths();

        Assert.Equal(2, truths.Count);
        Assert.Equal((0.0, 120.0), (truths[0].From, truths[0].To));
        Assert.Equal((120.0, 300.0), (truths[1].From, truths[1].To));
        Assert.All(truths, t => Assert.DoesNotContain("(", t.Text, StringComparison.Ordinal));
        Assert.EndsWith("HW? KB2UZK/M DE N3AFS K", truths[0].Text, StringComparison.Ordinal);
        Assert.EndsWith("KEN N3AFS DE KB2UZK/M K", truths[1].Text, StringComparison.Ordinal);
        Assert.StartsWith("# DRAFT - the owner corrects by ear", File.ReadLines(Path.ChangeExtension(TheOwnersRecordingReadsTests.Wav(CwWildHands.Recording), ".truth.md")).First(), StringComparison.Ordinal);
    }

    /// <remarks>
    /// **THE BASELINE TRACE** (work instruction 592, section 4): for the two real hands and the `mobile` and `bug` recipes at 20 dB and their
    /// own speed, what was sent against what the reader learned. What the next unit is measured against. Asserts nothing; never on a line.
    /// </remarks>
    [Fact]
    [Trait("Category", "Survey")]
    public void TheBaselineTrace()
    {
        var inv = CultureInfo.InvariantCulture;
        var wav = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(CwWildHands.Recording));
        var (pitch, width) = TheRecordingsScoreboardTests.RadioState(CwWildHands.Recording);
        var truths = CwWildHands.Truths();
        double[] moments = [10, 30, 120, 130, 150, 280];
        var (letters, held) = CwWildHands.Read(wav.Samples, wav.SampleRate, pitch, width, moments);
        var map = TheKeyingMapTests.Read(CwWildHands.Recording, wav.Samples, wav.SampleRate);

        string Cell(CwWildHands.Held h) => h.Learned is { } l
            ? string.Create(inv, $"{h.AtSeconds:0} s, {h.PrintedHz:0} Hz: {l.InsideGap * CwSequenceReader.HopMs:0} / {l.LetterGap * CwSequenceReader.HopMs:0} / {l.WordGap * CwSequenceReader.HopMs:0}, dit {l.Dit * CwSequenceReader.HopMs:0} dah {l.Dah * CwSequenceReader.HopMs:0}")
            : string.Create(inv, $"{h.AtSeconds:0} s: nobody printing");

        output.WriteLine("| hand | sent or measured, inside / letter / word, dit, dah (ms) | the reader's learned, at 10 s, 30 s and the end, inside / letter / word, dit, dah (ms) | E, T or the placeholder | printed |");
        output.WriteLine("|---|---|---|---|---|");

        foreach (var (truth, recipe, at) in new[] { (truths[0], CwWildBank.Recipes[1], new[] { 0, 1, 2 }), (truths[1], CwWildBank.Recipes[0], new[] { 3, 4, 5 }) })
        {
            var score = CwWildHands.ScoreOf(truth, letters, map);
            var (i, l, w) = recipe.Medians;
            var d = recipe.DitMs;

            output.WriteLine(string.Create(inv, $"| {truth.Name} (measured by the analysis) | {i * d:0} / {l * d:0} / {w * d:0}, dit {d:0}, dah {recipe.Ratio * d:0} | {string.Join("; ", at.Select(k => Cell(held[k])))} | {CwWildHands.EtShare(score.Letters):0%} | `{score.Printed}` |"));
        }

        foreach (var recipe in new[] { CwWildBank.Recipes[0], CwWildBank.Recipes[1] })
        {
            var hand = new CwWildHand(recipe, OwnSpeed: true, Db: 20, Qso: true);
            var audio = hand.Audio();
            var seconds = audio.Samples.Length / (double)audio.SampleRate;
            var (read, marks) = CwWildHands.Read(audio.Samples, audio.SampleRate, 600, 500, [10, 30, seconds]);
            var gaps = hand.SentGaps();
            var sent = hand.SentMarks();

            double Median(IEnumerable<double> v)
            {
                var s = v.Order().ToList();

                return s[s.Count / 2] * 1000;
            }

            double Gap(CwGapClass c) => Median(gaps.Where(g => g.Drawn == c).Select(g => g.Seconds));

            output.WriteLine(string.Create(inv, $"| recipe `{recipe.Name}`, 20 dB, own speed, QSO text (as sent) | {Gap(CwGapClass.Inside):0} / {Gap(CwGapClass.Letter):0} / {Gap(CwGapClass.Word):0}, dit {Median(sent.Where(m => !m.Dah).Select(m => m.Seconds)):0}, dah {Median(sent.Where(m => m.Dah).Select(m => m.Seconds)):0} | {string.Join("; ", marks.Select(Cell))} | {CwWildHands.EtShare(read):0%} | `{TheRecordingsScoreboardTests.Text(read)}` |"));
        }

        // Where the reader printed on the capture, every twenty seconds: which lanes, how many letters.
        foreach (var group in letters.GroupBy(l => Math.Floor(l.Seconds / 20) * 20).OrderBy(g => g.Key))
        {
            output.WriteLine(string.Create(inv, $"{group.Key:0}-{group.Key + 20:0} s: {string.Join(", ", group.GroupBy(l => Math.Round(l.PitchHz / 25) * 25).OrderBy(g => g.Key).Select(g => $"{g.Count()} at {g.Key:0} Hz"))}"));
        }
    }
}
