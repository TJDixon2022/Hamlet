using Hamlet.RadioEngine.Cw;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **THE WORDS OF THE AIR** (work instruction 595, section 2, under HM-DEC-296): what the vocabulary recognizes, and the recognized share the validity judge
/// reads. Proves HM-DEC-299 and HM-DEC-300.
/// </summary>
public sealed class TheCwVocabularyTests
{
    private static string[] Words(string text) => text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

    [Fact]
    public void AContestExchangeIsAllWords()
        => Assert.All(Words("TEST DE TEST"), w => Assert.True(CwVocabulary.IsWord(w), w));

    [Fact]
    public void ARagchewIsAllButAtMostTwo()
        => Assert.True(Words("GM KEN NICE TO MEET U ES TNX FER RPRT UR RST 579 579").Count(w => !CwVocabulary.IsWord(w)) <= 2);

    [Fact]
    public void RunsOfEAndTAreNoWords()
        => Assert.All(Words("TETE TTET ETETTE TTTTTE"), w => Assert.False(CwVocabulary.IsWord(w), w));

    [Theory]
    [InlineData("N3AFS")]
    [InlineData("KB2UZK/M")]
    [InlineData("W1AW")]
    [InlineData("KC4ZGP/QRP")]
    [InlineData("W1AW/4")]
    public void CallsignsAreAShape(string call)
        => Assert.True(CwVocabulary.IsCallsign(call), call);

    [Theory]
    [InlineData("TEST")]
    [InlineData("W1")]
    [InlineData("ABC1D")]
    [InlineData("W1ABCD")]
    [InlineData("W1AW/")]
    public void WhatIsNoCallsign(string word)
        => Assert.False(CwVocabulary.IsCallsign(word), word);

    [Theory]
    [InlineData("5NN")]
    [InlineData("599")]
    [InlineData("73")]
    [InlineData("5N9")]
    public void ReportsAndNumbersAreShapes(string word)
        => Assert.True(CwVocabulary.IsWord(word), word);

    [Fact]
    public void AQCodeMayAsk()
    {
        Assert.True(CwVocabulary.IsWord("QTH"));
        Assert.True(CwVocabulary.IsWord("QTH?"));
        Assert.False(CwVocabulary.IsWord("TEST?"));
    }

    [Theory]
    [InlineData("E")]
    [InlineData("T")]
    [InlineData("TE")]
    [InlineData("ET")]
    [InlineData("TT")]
    [InlineData("EE")]
    public void AShortWordIsOnlyFromTheShortLists(string word)
        => Assert.False(CwVocabulary.IsWord(word), word);

    [Theory]
    [InlineData("TONITE")]
    [InlineData("TONIGHT")]
    [InlineData("2NITE")]
    [InlineData("WKD")]
    [InlineData("WRKD")]
    [InlineData("SKED")]
    [InlineData("CONGRATS")]
    [InlineData("GUD")]
    [InlineData("GD")]
    [InlineData("NITE")]
    [InlineData("LID")]
    [InlineData("CUL")]
    [InlineData("TKS")]
    [InlineData("VY")]
    [InlineData("FINE")]
    public void TheWordsTheTracesNamedAreWords(string word)
        => Assert.True(CwVocabulary.IsWord(word), word);

    [Theory]
    [InlineData("40M", true)]
    [InlineData("20M", true)]
    [InlineData("160M", true)]
    [InlineData("1600M", false)]
    [InlineData("M", false)]
    [InlineData("4M0", false)]
    public void ABandIsAShape(string word, bool band)
        => Assert.Equal(band, CwVocabulary.IsBand(word));

    [Theory]
    [InlineData("7.040", true)]
    [InlineData("14.058", true)]
    [InlineData("7.", false)]
    [InlineData(".040", false)]
    [InlineData("7.0.4", false)]
    [InlineData("E.T", false)]
    public void AFrequencyIsAShape(string word, bool frequency)
        => Assert.Equal(frequency, CwVocabulary.IsFrequency(word));

    [Fact]
    public void TheListHoldsAtLeastTheFewThousandAsked()
        => Assert.True(CwVocabulary.Size >= 3000, CwVocabulary.Size.ToString(System.Globalization.CultureInfo.InvariantCulture));

    // Segments that read as the text, each letter its Morse, spaces word gaps; a window opening on a word gap unless asked otherwise, and closing on one.
    private static List<CwSequenceReader.Segment> Laid(string text, bool opensCut = false, bool closesCut = false)
    {
        var patterns = MorseAlphabet.All.GroupBy(p => p.Value, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.First().Key, StringComparer.Ordinal);
        var segs = new List<CwSequenceReader.Segment>();
        var at = 0;

        void Add(int state)
        {
            segs.Add(new CwSequenceReader.Segment(state, at, at + 1));
            at++;
        }

        if (!opensCut)
        {
            Add(CwSequenceReader.WordGap);
        }

        var words = Words(text);

        for (var w = 0; w < words.Length; w++)
        {
            for (var l = 0; l < words[w].Length; l++)
            {
                var pattern = patterns[words[w][l].ToString()];

                for (var m = 0; m < pattern.Length; m++)
                {
                    Add(pattern[m] == '-' ? CwSequenceReader.Dah : CwSequenceReader.Dit);

                    if (m < pattern.Length - 1)
                    {
                        Add(CwSequenceReader.InsideGap);
                    }
                }

                if (l < words[w].Length - 1)
                {
                    Add(CwSequenceReader.LetterGap);
                }
            }

            if (w < words.Length - 1 || !closesCut)
            {
                Add(CwSequenceReader.WordGap);
            }
        }

        return segs;
    }

    [Fact]
    public void HonestTextHeavyInEAndTReadsWhole()
    {
        var (share, counted, _) = CwSequenceReader.Recognized(Laid("TEST DE TEST"));

        Assert.Equal(1.0, share, 3);
        Assert.Equal(10, counted);
    }

    [Fact]
    public void APathOfEAndTReadsAsNothing()
    {
        var (share, counted, _) = CwSequenceReader.Recognized(Laid("TETE TTET"));

        Assert.Equal(0.0, share, 3);
        Assert.Equal(8, counted);
    }

    [Fact]
    public void AWordCutByTheWindowCountsNeitherWay()
    {
        var (share, counted, _) = CwSequenceReader.Recognized(Laid("TEE NAME TEE", opensCut: true, closesCut: true));

        Assert.Equal(1.0, share, 3);
        Assert.Equal(4, counted);
    }

    [Fact]
    public void OnlyTheLastTwelveLettersAreRead()
    {
        // The noise before a station who began mid-window falls out of the letters read.
        var (share, counted, _) = CwSequenceReader.Recognized(Laid("TETE TETTE TETE CQ CQ DE W1AW"));

        Assert.Equal(1.0, share, 3);
        Assert.Equal(10, counted);
    }

    [Fact]
    public void AProsignStandsAloneFromTheWordBesideIt()
    {
        // A prosign is a word of its own, so HAT<BT> is two words.
        var segs = Laid("HAT");

        segs.RemoveAt(segs.Count - 1);

        var at = segs[^1].To;

        segs.Add(new CwSequenceReader.Segment(CwSequenceReader.LetterGap, at, at + 1));
        at++;

        foreach (var c in "-...-")
        {
            segs.Add(new CwSequenceReader.Segment(c == '-' ? CwSequenceReader.Dah : CwSequenceReader.Dit, at, at + 1));
            at++;
            segs.Add(new CwSequenceReader.Segment(CwSequenceReader.InsideGap, at, at + 1));
            at++;
        }

        segs.Add(new CwSequenceReader.Segment(CwSequenceReader.WordGap, at, at + 1));

        var (share, counted, _) = CwSequenceReader.Recognized(segs);

        Assert.Equal(1.0, share, 3);
        Assert.Equal(4, counted);
    }

    private static (double Words, int Counted, double Letters, double Gaps, bool TextRefused, bool? Valid) Judged(string text)
        => CwSequenceReader.Validity(new CwSequenceReader.Reading(Laid(text), text, [], default, default));

    [Fact]
    public void HonestTextOfEAndTIsKeptByTheWords()
    {
        var v = Judged("TEST DE TEST");

        Assert.True(v.Letters > CwSequenceReader.ValidityShare);
        Assert.False(v.TextRefused);
    }

    [Fact]
    public void AHandALetterOffIsKeptByTheLetterCount()
    {
        var v = Judged("WAQK BROWN FOQ");

        Assert.True(v.Words < CwSequenceReader.ValidWords);
        Assert.True(v.Letters <= CwSequenceReader.ValidityShare);
        Assert.False(v.TextRefused);
    }

    [Fact]
    public void ARunOfEAndTIsRefusedByBoth()
    {
        var v = Judged("TETE TTET ETE");

        Assert.True(v.Letters > CwSequenceReader.ValidityShare);
        Assert.True(v.Words < CwSequenceReader.ValidWords);
        Assert.True(v.TextRefused);
    }

    [Fact]
    public void UnderTheLeastALaneHasNoVerdict()
    {
        var reading = new CwSequenceReader.Reading(Laid("TE"), "TE", [], default, default);

        Assert.Null(CwSequenceReader.Validity(reading).Valid);
    }
}
