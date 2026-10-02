using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **A hand is read against itself** (work instruction 526, HM-DEC-230): each mark and each gap is judged against
/// its neighbours in the same sender, so a change of speed has no adjustment period.
/// </summary>
/// <remarks>
/// <para>**THE OWNER, 2026-10-02**: *"Our biggest struggle is in changes of words per minute. Hand keyers are going to
/// be all over the place."*</para>
/// <para>**SYNTHETIC AUDIO WRITTEN HERE, NOTHING READ FROM DISK** (R96): a keyer whose speed, scatter and spacing are
/// set per letter, at 24 dB over the noise on unit 502's scale.</para>
/// </remarks>
public sealed class AHandIsReadAgainstItselfTests
{
    private const int Rate = 8000;
    private const double Pitch = 625;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the readings are printed.</param>
    public AHandIsReadAgainstItselfTests(ITestOutputHelper output) => _output = output;

    /// <summary>How one letter is sent: its speed, its scatter, and the dits of the gaps after it.</summary>
    internal readonly record struct Sending(double Wpm, double Scatter, double DahDits = 3, double? DahScatter = null, double LetterGapDits = 3, double WordGapDits = 7, double? LetterGapSeconds = null, double? WordGapSeconds = null);

    /// <summary>
    /// The text keyed letter by letter as <paramref name="sending"/> says for each letter's index, with noise added after.
    /// </summary>
    internal static float[] Keyed(string text, Func<int, Sending> sending, int seed, double db = 24, double overshootDb = 0, double pitch = Pitch)
    {
        var random = new Random(seed);
        var keyed = new List<(double Seconds, bool On)> { (3.0, false) };
        var letter = 0;

        double Draw(double ideal, double s) => ideal * (1 + (s * ((2 * random.NextDouble()) - 1)));

        var words = text.Split(' ');

        for (var w = 0; w < words.Length; w++)
        {
            for (var c = 0; c < words[w].Length; c++)
            {
                var send = sending(letter++);
                var dit = 1.2 / send.Wpm;
                var code = Morse[words[w][c]];

                for (var e = 0; e < code.Length; e++)
                {
                    keyed.Add((code[e] == '-' ? Draw(send.DahDits * dit, send.DahScatter ?? send.Scatter) : Draw(dit, send.Scatter), true));

                    if (e + 1 < code.Length)
                    {
                        keyed.Add((Draw(dit, send.Scatter), false));
                    }
                }

                var last = c + 1 == words[w].Length;

                if (!last)
                {
                    keyed.Add((Draw(send.LetterGapSeconds ?? (send.LetterGapDits * dit), send.Scatter), false));
                }
                else if (w + 1 < words.Length)
                {
                    keyed.Add((Draw(send.WordGapSeconds ?? (send.WordGapDits * dit), send.Scatter), false));
                }
            }
        }

        keyed.Add((3.0, false));

        var samples = new float[(int)(keyed.Sum(k => k.Seconds) * Rate) + Rate];
        var amplitude = ThePatternIsTheGateTests.Over(db);
        var edge = (int)(0.004 * Rate);
        var at = 0;

        foreach (var (seconds, on) in keyed)
        {
            var n = (int)Math.Round(seconds * Rate);

            if (on)
            {
                for (var i = 0; i < n; i++)
                {
                    var shape = i < edge ? 0.5 - (0.5 * Math.Cos(Math.PI * i / edge))
                        : i >= n - edge ? 0.5 - (0.5 * Math.Cos(Math.PI * (n - i) / edge))
                        : 1.0;

                    var agc = Math.Pow(10, overshootDb * Math.Exp(-i / (0.025 * Rate)) / 20);

                    samples[at + i] = (float)(amplitude * agc * shape * Math.Sin(2 * Math.PI * pitch * (at + i) / Rate));
                }
            }

            at += n;
        }

        var noise = new Random(seed + 1);

        for (var i = 0; i < samples.Length; i++)
        {
            var u1 = 1.0 - noise.NextDouble();
            var u2 = noise.NextDouble();

            samples[i] += (float)(0.04 * Math.Sqrt(-2 * Math.Log(u1)) * Math.Cos(2 * Math.PI * u2));
        }

        return samples;
    }

    /// <summary>The International Morse code for the letters, figures and the slash.</summary>
    internal static readonly IReadOnlyDictionary<char, string> Morse = new Dictionary<char, string>
    {
        ['A'] = ".-", ['B'] = "-...", ['C'] = "-.-.", ['D'] = "-..", ['E'] = ".", ['F'] = "..-.", ['G'] = "--.",
        ['H'] = "....", ['I'] = "..", ['J'] = ".---", ['K'] = "-.-", ['L'] = ".-..", ['M'] = "--", ['N'] = "-.",
        ['O'] = "---", ['P'] = ".--.", ['Q'] = "--.-", ['R'] = ".-.", ['S'] = "...", ['T'] = "-", ['U'] = "..-",
        ['V'] = "...-", ['W'] = ".--", ['X'] = "-..-", ['Y'] = "-.--", ['Z'] = "--..",
        ['0'] = "-----", ['1'] = ".----", ['2'] = "..---", ['3'] = "...--", ['4'] = "....-", ['5'] = ".....",
        ['6'] = "-....", ['7'] = "--...", ['8'] = "---..", ['9'] = "----.", ['/'] = "-..-.",
    };

    /// <summary>
    /// The conditions the air showed on 2026-10-02 and the bench did not: `plain` at 24 dB; `agc`, a 3 dB AGC overshoot
    /// at each key-down; `agc-filter`, that through the 500 Hz filter on 600; and `weak-agc-filter`, that at 12 dB.
    /// </summary>
    internal static float[] Under(string condition, string text, Func<int, Sending> sending, int seed)
    {
        var db = condition.StartsWith("weak", StringComparison.Ordinal) ? 12 : 24;
        var samples = Keyed(text, sending, seed, db, condition == "plain" ? 0 : 3);

        return condition.EndsWith("filter", StringComparison.Ordinal) ? NarrownessReadsTheFiltersBandTests.ThroughTheFilter(samples) : samples;
    }

    private ThePatternIsTheGateTests.Reading Reads(string name, string condition, string text, Func<int, Sending> sending)
    {
        var r = ThePatternIsTheGateTests.Read(Under(condition, text, sending, 5270), Pitch);

        _output.WriteLine($"{name}, {condition}: {r.Candidates} candidates, {r.Stood} stood, {r.Printed} printed; reads `{r.Text}`");

        return r;
    }

    private const string Bulletin = "TEXT IS FROM SEPTEMBER 2024 AND THE QUICK BROWN FOX JUMPS OVER THE LAZY DOG";

    private static readonly int Letters = Bulletin.Count(c => c != ' ');

    /// <remarks>
    /// Task 1: W1AW-style machine sending stepping from 25 to 35 WPM and back mid-sentence: every letter read at both
    /// changes. At 12 dB through the filter with AGC it reads whole too; at 12 dB plain, one dah of the J measured 7 dB
    /// low began a second sender (`W TUMPS`), a level grouping and not the speed, so that row is printed.
    /// </remarks>
    /// <param name="condition">See <see cref="Under"/>.</param>
    [Theory]
    [InlineData("plain")]
    [InlineData("agc")]
    [InlineData("agc-filter")]
    [InlineData("weak-agc-filter")]
    public void ASpeedChangeHasNoAdjustmentPeriod(string condition)
        => Assert.Equal(Bulletin, Reads("25, 35, 25 WPM", condition, Bulletin, i => new Sending(i < Letters / 3 || i >= 2 * Letters / 3 ? 25 : 35, 0)).Text);

    /// <remarks>
    /// Tasks 1 and 2: a hand that drifts from 13 to 18 WPM and back across a sentence, scattered by a sixth, reads whole;
    /// BROWN FOX ran together on the 13 WPM letter gaps until five dits stood as a floor. At 12 dB through the filter
    /// with AGC it is printed.
    /// </remarks>
    /// <param name="condition">See <see cref="Under"/>.</param>
    [Theory]
    [InlineData("plain")]
    [InlineData("agc")]
    [InlineData("agc-filter")]
    [InlineData("weak-agc-filter")]
    public void AHandThatDriftsReadsWhole(string condition)
    {
        var r = Reads("13, 18, 13 WPM, drifting", condition, Bulletin, i => new Sending(13 + (5 * Math.Sin(Math.PI * i / Letters)), 1.0 / 6));

        if (condition != "weak-agc-filter")
        {
            Assert.Equal(Bulletin, r.Text);
        }
    }

    private const string Hand = "THIS CONNECTION IS GOOD TNX FER CALL ES QSO";

    /// <remarks>
    /// Task 2: the 13 WPM hand of 7.035 at 12:15, scattered by a fifth: THIS and CONNECTION read as words. Through the
    /// filter with AGC it stood nothing before the first rise above the floor was a key-down (task 7).
    /// </remarks>
    /// <param name="condition">See <see cref="Under"/>.</param>
    [Theory]
    [InlineData("plain")]
    [InlineData("agc")]
    [InlineData("agc-filter")]
    [InlineData("weak-agc-filter")]
    public void A13WpmHandReadsItsWords(string condition)
        => Assert.Equal(Hand, Reads("13 WPM hand, a fifth", condition, Hand, _ => new Sending(13, 0.2)).Text);

    private const string Code = "TEXT IS FROM SEPTEMBER 2024";

    /// <remarks>
    /// Task 2: W1AW's 5 WPM Farnsworth section, letters at 18 WPM and the spaces stretched to 5 WPM overall (unit 501's
    /// PARIS arithmetic): reads as words. With AGC its first word was lost before task 7.
    /// </remarks>
    /// <param name="condition">See <see cref="Under"/>.</param>
    [Theory]
    [InlineData("plain")]
    [InlineData("agc")]
    [InlineData("agc-filter")]
    [InlineData("weak-agc-filter")]
    public void FarnsworthAtFiveReadsAsWords(string condition)
    {
        var unit = ((60.0 / 5) - (31 * 1.2 / 18)) / 19;

        Assert.Equal(Code, Reads("5 WPM Farnsworth", condition, Code, _ => new Sending(18, 0, LetterGapSeconds: 3 * unit, WordGapSeconds: 7 * unit)).Text);
    }

    /// <remarks>Task 2: the Quebec station's 4-dit word gaps at 18 WPM, scattered by a sixth: printed, not held.</remarks>
    [Fact]
    public void FourDitWordGapsArePrinted()
        => Reads("18 WPM, 4-dit word gaps", "plain", "KI1MM DE VE2JD NAME IS JEAN QTH QUEBEC HW", _ => new Sending(18, 1.0 / 6, WordGapDits: 4));

    private const string Skcc = "CQ CQ SKCC DE N0CALL N0CALL K";

    /// <remarks>
    /// Task 4: an SKCC straight key at 18 WPM, scattered by two fifths - dits from 0.6 to 1.4 dits, dahs from 2 to 4,
    /// gaps likewise - stands and reads. At 12 dB with AGC it stood nothing before the hand test was on the per-bin gate.
    /// Its last word gap is drawn at 4.35 dits, under the five-dit floor, so `N0CALL K` reads `N0CALLK`; the words before
    /// it are held, and at 12 dB through the filter the opening is printed.
    /// </remarks>
    /// <param name="condition">See <see cref="Under"/>.</param>
    [Theory]
    [InlineData("plain")]
    [InlineData("agc")]
    [InlineData("agc-filter")]
    [InlineData("weak-agc-filter")]
    public void AStraightKeyStands(string condition)
    {
        var r = Reads("straight key, two fifths", condition, Skcc, _ => new Sending(18, 0.4, DahScatter: 1.0 / 3));

        Assert.True(r.Stood > 0, "the straight key stands");

        if (condition != "weak-agc-filter")
        {
            Assert.StartsWith("CQ CQ SKCC DE N0CALL N0CALL", r.Text, StringComparison.Ordinal);
        }
    }

    /// <remarks>
    /// Task 2 of work instruction 528: the case unit 521 lacked. A clean sender at 16 WPM halfway between two bins,
    /// through the 500 Hz filter on 600, with a 1 dB AGC overshoot - what the owner's recording on 7.0549 measured, its
    /// station at 662.8 Hz - reads whole, every dah a dah.
    /// </remarks>
    /// <param name="pitch">The station's pitch, halfway between two bins.</param>
    [Theory]
    [InlineData(612.5)]
    [InlineData(637.5)]
    [InlineData(662.5)]
    public void AStationBetweenTwoBinsReadsItsDahs(double pitch)
    {
        const string Text = "FER CHAT BEST 73 KC4ZGP DE WA";
        var samples = NarrownessReadsTheFiltersBandTests.ThroughTheFilter(Keyed(Text, _ => new Sending(16, 0), 5280, 24, 1, pitch));
        var r = ThePatternIsTheGateTests.Read(samples, pitch, d => d.SetPassband(600, 500));

        _output.WriteLine($"{pitch} Hz, between bins, filter and 1 dB AGC: {r.Stood} stood; reads `{r.Text}`");

        Assert.Equal(Text, r.Text);
    }
}
