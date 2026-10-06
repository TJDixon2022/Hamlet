using System.Globalization;
using System.Text;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **FARNSWORTH: LONG LETTER GAPS ARE NOT WORDS** (work instruction 546, task 2): W1AW's slow code practice, letters at
/// 18 WPM with the gaps stretched to 5 and 7.5 WPM overall, read through the app's chain, reads as words.
/// </summary>
/// <remarks>
/// <para>**ON THE AIR, 2026-10-05**: the 5 and 7.5 WPM sections printed every letter spaced, `E X T F OLLOWS`, while
/// 10, 13 and 15 read whole sentences.</para>
/// <para>**SYNTHETIC AUDIO WRITTEN HERE** (R88): built one letter at a time, each at 18 WPM, followed by the stretched gap
/// PARIS gives for the overall speed, one stretched unit being (60/overall - 31 × 1.2/18) / 19 seconds, a letter gap three
/// of them and a word gap seven, as <see cref="FarnsworthAndLoneLettersTests"/> builds it.</para>
/// </remarks>
public sealed class AFarnsworthBulletinReadsAsWordsTests
{
    /// <summary>W1AW's own opening for a slow section.</summary>
    internal const string Bulletin = "TEXT IS FROM OCTOBER 2024 QST PAGE 50 5 WPM TEXT FOLLOWS";

    private const int Rate = 8000;
    private const int LetterWpm = 18;

    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where each case prints its text and the sender's gap lines.</param>
    public AFarnsworthBulletinReadsAsWordsTests(ITestOutputHelper output) => _output = output;

    /// <summary>One stretched spacing unit at an overall speed, with the letters at 18 WPM, in seconds.</summary>
    private static double StretchedUnit(double overallWpm) => ((60 / overallWpm) - (31 * 1.2 / LetterWpm)) / 19;

    /// <summary>The text sent Farnsworth-style: letters at 18 WPM, spaces stretched to the overall speed.</summary>
    internal static float[] Farnsworth(string text, double overallWpm, int seed)
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

    /// <summary>What the app's chain prints, one space between words, and the printed sender's gap lines at the end.</summary>
    internal static (string Text, string Lines) Read(float[] samples)
    {
        using var chain = new CwChain(Rate);
        var text = new StringBuilder();
        var lines = "none";

        chain.Decoder.CharacterSettled += c => text.Append(c.Text);

        for (var at = 0; at + 80 <= samples.Length; at += 80)
        {
            chain.Process(new AudioChunk(at, Rate, samples.AsSpan(at, 80)));

            if (chain.Decoder.Runs.StationLines is { } l)
            {
                lines = string.Create(Invariant,
                    $"gap dit {l.GapDitSeconds * 1000:0} ms; inside {l.InsideGaps.Count} gaps, median {(l.InsideGaps.Count > 0 ? l.InsideGaps.Order().ElementAt(l.InsideGaps.Count / 2) * 1000 : double.NaN):0} ms; letter cluster {(l.LetterGaps is { } lg ? $"{lg.Count} at {lg.Average() * 1000:0} ms" : "none")}; word cluster {(l.WordGaps is { } wg ? $"{wg.Count} at {wg.Average() * 1000:0} ms" : "none")}; letter line {l.CharacterSeconds * 1000:0} ms, word line {l.WordSeconds * 1000:0} ms");
            }
        }

        chain.Decoder.Flush();

        return (string.Join(' ', text.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries)), lines);
    }

    /// <remarks>
    /// Task 2: W1AW's form at 5 and 7.5 WPM overall reads as words: the text printed is the bulletin, every space where it
    /// was sent and none inside a word.
    /// </remarks>
    /// <param name="overallWpm">The overall speed.</param>
    [Theory]
    [InlineData(5.0)]
    [InlineData(7.5)]
    public void TheSlowSectionsReadAsWords(double overallWpm)
    {
        var unit = StretchedUnit(overallWpm);
        var (text, lines) = Read(Farnsworth(Bulletin, overallWpm, 5460 + (int)(overallWpm * 10)));

        _output.WriteLine(string.Create(Invariant, $"18/{overallWpm:0.0}: true letter gap {3 * unit * 1000:0} ms, word gap {7 * unit * 1000:0} ms, gap inside a letter {1.2 / LetterWpm * 1000:0} ms"));
        _output.WriteLine($"  lines at the end: {lines}");
        _output.WriteLine($"  sent `{Bulletin}`");
        _output.WriteLine($"  read `{text}`");

        Assert.Equal(Bulletin, text);
    }
}

/// <summary>Task 2's probe: the slow section heard after other sending, as on the air.</summary>
public sealed class AFarnsworthBulletinAfterOtherSendingTests
{
    private const int Rate = 8000;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where each case prints its text and the sender's gap lines.</param>
    public AFarnsworthBulletinAfterOtherSendingTests(ITestOutputHelper output) => _output = output;

    /// <remarks>
    /// Task 2: the bulletin at 5 and 7.5 WPM overall after a preamble: <c>vvv</c>, the same text at the same Farnsworth
    /// spacing; <c>fast</c>, a sentence at 18 WPM with ordinary spacing just before, the sender's lines learned on it.
    /// Since the rule shipped (work instruction 547, task 2, HM-DEC-251), the slow section reads as words after either
    /// preamble: everything after its first word, whose opening letters are sent before three gaps have shown the new spacing.
    /// </remarks>
    /// <param name="preamble">Which preamble.</param>
    /// <param name="overallWpm">The overall speed.</param>
    [Theory]
    [InlineData("vvv", 5.0)]
    [InlineData("vvv", 7.5)]
    [InlineData("fast", 5.0)]
    [InlineData("fast", 7.5)]
    public void TheSlowSectionAfterOtherSending(string preamble, double overallWpm)
    {
        var lead = preamble == "vvv"
            ? AFarnsworthBulletinReadsAsWordsTests.Farnsworth("VVV VVV QST DE W1AW", overallWpm, 5470)
            : CwSignal.Generate(new CwSignalRequest("QST QST DE W1AW W1AW NOW SLOW CODE", WordsPerMinute: 18, ToneHz: 625, SampleRate: Rate, Amplitude: 0.5, NoiseAmplitude: 0.04, LeadInSeconds: 3, TailSeconds: 2, Seed: 5480)).Samples;
        var body = AFarnsworthBulletinReadsAsWordsTests.Farnsworth(AFarnsworthBulletinReadsAsWordsTests.Bulletin, overallWpm, 5490);
        var samples = lead.Concat(body).ToArray();
        var (text, lines) = AFarnsworthBulletinReadsAsWordsTests.Read(samples);

        _output.WriteLine($"{preamble} 18/{overallWpm}: lines at the end: {lines}");
        _output.WriteLine($"  read `{text}`");

        Assert.EndsWith("IS FROM OCTOBER 2024 QST PAGE 50 5 WPM TEXT FOLLOWS", text, StringComparison.Ordinal);
    }
}
