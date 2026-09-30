using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **Farnsworth gaps are the sender's own, and a lone letter must belong to something** (work
/// instruction 501, R109, HM-DEC-205). No case had been built Farnsworth-style, the way the ARRL
/// sends its slow code practice, and a lone letter had been confirming a lone letter.
/// </summary>
/// <remarks>
/// **SYNTHETIC AUDIO WRITTEN HERE, NOTHING READ FROM DISK** (R96). A Farnsworth signal is built one
/// letter at a time: each letter at 18 WPM, followed by the stretched gap PARIS gives for the
/// overall speed, one stretched unit being (60/overall - 31 × 1.2/18) / 19 seconds, a letter gap
/// three of them and a word gap seven.
/// </remarks>
public sealed class FarnsworthAndLoneLettersTests
{
    private const int Rate = 8000;
    private const int Chunk = 80;
    private const int LetterWpm = 18;
    private const string Call = "CQ CQ DE N0CALL N0CALL K";
    private const string Answer = "TEST DE W1AW K";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where each case prints its text and its gaps.</param>
    public FarnsworthAndLoneLettersTests(ITestOutputHelper output) => _output = output;

    /// <summary>One stretched spacing unit at an overall speed, with the letters at 18 WPM, in seconds.</summary>
    private static double StretchedUnit(double overallWpm) => ((60 / overallWpm) - (31 * 1.2 / LetterWpm)) / 19;

    /// <summary>The text sent Farnsworth-style: letters at 18 WPM, spaces stretched to the overall speed.</summary>
    private static float[] Farnsworth(string text, double overallWpm, int seed)
    {
        var unit = StretchedUnit(overallWpm);
        var words = text.Split(' ');
        var pieces = new List<float[]>();

        for (var w = 0; w < words.Length; w++)
        {
            for (var c = 0; c < words[w].Length; c++)
            {
                var lastInWord = c == words[w].Length - 1;
                var tail = !lastInWord ? 3 * unit : w < words.Length - 1 ? 7 * unit : 3;

                pieces.Add(CwSignal.Generate(new CwSignalRequest(
                    words[w][c].ToString(), WordsPerMinute: LetterWpm, ToneHz: 625, SampleRate: Rate,
                    Amplitude: 0.5, NoiseAmplitude: 0.04,
                    LeadInSeconds: pieces.Count == 0 ? 3 : 0, TailSeconds: tail,
                    Seed: seed + pieces.Count)).Samples);
            }
        }

        var samples = new float[pieces.Sum(p => p.Length)];
        var at = 0;

        foreach (var piece in pieces)
        {
            piece.CopyTo(samples, at);
            at += piece.Length;
        }

        return samples;
    }

    private static float[] Standard(string text, int wpm, int seed)
        => CwSignal.Generate(new CwSignalRequest(
            text, WordsPerMinute: wpm, ToneHz: 625, SampleRate: Rate, Amplitude: 0.5,
            NoiseAmplitude: 0.04, LeadInSeconds: 3, TailSeconds: 3, Seed: seed)).Samples;

    /// <summary>What the run reader prints, and the letter and word gaps it measured for the sender it printed.</summary>
    private static (string Text, double? Letter, double? Word) Read(float[] samples)
    {
        var detector = new CwEnvelopeDetector(Rate);
        var reader = new CwRunReader();
        var characters = new List<CwCharacter>();
        var sequence = 0L;
        (double? Letter, double? Word) gaps = (null, null);

        reader.CharacterRead += characters.Add;

        for (var at = 0; at + Chunk <= samples.Length; at += Chunk)
        {
            detector.Process(samples.AsSpan(at, Chunk));

            var batch = detector.MarksSince(sequence);

            sequence = batch.Marks.Count > 0 ? batch.Marks.Max(m => m.Sequence) : sequence;
            reader.Read(batch);

            if (reader.StationGaps.Letter is not null)
            {
                gaps = reader.StationGaps;
            }
        }

        reader.Flush();

        return (string.Join(' ', string.Concat(characters.Select(c => c.Text)).Split(' ', StringSplitOptions.RemoveEmptyEntries)), gaps.Letter, gaps.Word);
    }

    private static string Ms(double? seconds) => seconds is { } s ? $"{s * 1000:0}" : "none";

    private string FarnsworthReads(string sent, double overallWpm, int seed)
    {
        var unit = StretchedUnit(overallWpm);
        var (text, letter, word) = Read(Farnsworth(sent, overallWpm, seed));

        _output.WriteLine(
            $"Farnsworth 18/{overallWpm:0}: true letter gap {3 * unit * 1000:0} ms, word gap {7 * unit * 1000:0} ms; "
            + $"measured letter {Ms(letter)} ms, word {Ms(word)} ms; sent `{sent}` read `{text}`");

        return text;
    }

    /// <remarks>Case 1: the call sent Farnsworth-style, 18 WPM letters at 5, 10 and 13 WPM overall, reads whole.</remarks>
    [Theory]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(13)]
    public void AFarnsworthCallReadsWhole(int overallWpm)
        => Assert.Equal(Call, FarnsworthReads(Call, overallWpm, 5010 + overallWpm));

    /// <remarks>Case 2: TEST DE W1AW K sent Farnsworth-style at 5 WPM overall reads whole.</remarks>
    [Fact]
    public void AFarnsworthAnswerReadsWhole()
        => Assert.Equal(Answer, FarnsworthReads(Answer, 5, 5020));

    /// <remarks>
    /// Case 3: lone marks at a station's pitch and level, T E T T E, with nothing else, print nothing:
    /// no letter of two marks stands beside any of them, and five lone letters in a row are not
    /// sending.
    /// </remarks>
    [Fact]
    public void AStringOfLoneMarksPrintsNothing()
    {
        var (text, _, _) = Read(Standard("T E T T E", 18, 5030));

        _output.WriteLine($"lone marks `T E T T E` at 18 WPM: read `{text}`");

        Assert.Equal("", text);
    }

    /// <remarks>
    /// Case 3, from a sender already printed: the call, then T E T T E at the same pitch and level.
    /// Alone, lone marks never make a sender to print; after a call they are that sender's, and each
    /// lone letter used to confirm the next. The call prints and the string does not.
    /// <para>**SEED 5035, RESTORED** (work instruction 504). Unit 501 moved it to 5036 because the
    /// detector read the hop before hop zero while reaching a mark back to where its tone rose, and
    /// threw; the walk now stops at hop zero.</para>
    /// </remarks>
    [Fact]
    public void LoneMarksAfterACallPrintNothing()
    {
        var (text, _, _) = Read(Standard(Call + " T E T T E", 18, 5035));

        _output.WriteLine($"the call then `T E T T E` at 18 WPM: read `{text}`");

        Assert.Equal(Call, text);
    }

    /// <remarks>
    /// Case 4: at standard timing TEST DE W1AW K reads whole - its T and E confirmed by the S, the E
    /// of DE by the D - and so does DE DE, since a lone DE is one run of two marks and a sender makes
    /// two before it is printed.
    /// </remarks>
    [Theory]
    [InlineData(Answer, 18)]
    [InlineData("DE DE", 18)]
    public void TheLoneLettersInsideWordsStillPrint(string sent, int wpm)
    {
        var (text, _, _) = Read(Standard(sent, wpm, 5040 + sent.Length));

        _output.WriteLine($"`{sent}` at {wpm} WPM: read `{text}`");

        Assert.Equal(sent, text);
    }
}
