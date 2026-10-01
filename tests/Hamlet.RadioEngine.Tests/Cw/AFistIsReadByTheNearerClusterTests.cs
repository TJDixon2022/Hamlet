using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **A fist is read by the nearer cluster, not a hard line** (work instruction 513, R113, HM-DEC-217).
/// </summary>
/// <remarks>
/// <para>**THE 21:44 STATION**: a hand-sent station on 7.0299 at about 27 WPM, a clean stream of marks,
/// sorted into the wrong letters. No case on the bench had been both fast and human.</para>
/// <para>**A FIST KEYER WRITTEN HERE, NOTHING READ FROM DISK** (R96): a 625 Hz tone with 4 ms raised
/// cosine edges, 24 dB over the noise on unit 502's scale, in seeded Gaussian noise. Every element and
/// every gap is its ideal length times one plus a share drawn uniformly from minus to plus the stated
/// scatter: at 20%, dahs run 2.4 to 3.6 dits and gaps likewise.</para>
/// </remarks>
public sealed class AFistIsReadByTheNearerClusterTests
{
    private const int Rate = 8000;
    internal const double Pitch = 625;
    private const string Call = "CQ CQ DE N0CALL N0CALL K";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the readings and clusters are printed.</param>
    public AFistIsReadByTheNearerClusterTests(ITestOutputHelper output) => _output = output;

    /// <summary>The true lengths a fist sent, in seconds, by kind.</summary>
    internal sealed record Truth(List<double> Dits, List<double> Dahs, List<double> ElementGaps, List<double> LetterGaps, List<double> WordGaps);

    /// <summary>A hand-sent message: the scatter for each letter from the function, by the letter's index.</summary>
    internal static (float[] Samples, Truth Truth) Fist(string text, int wpm, Func<int, double> scatter, int seed, double wordGapDits = 7)
    {
        var random = new Random(seed);
        var dit = 1.2 / wpm;
        var truth = new Truth(new(), new(), new(), new(), new());
        var keyed = new List<(double Seconds, bool On)> { (3.0, false) };
        var letter = 0;

        double Draw(double ideal, double s) => ideal * (1 + (s * ((2 * random.NextDouble()) - 1)));

        var words = text.Split(' ');

        for (var w = 0; w < words.Length; w++)
        {
            for (var c = 0; c < words[w].Length; c++)
            {
                var s = scatter(letter++);
                var code = MorseCode(words[w][c]);

                for (var e = 0; e < code.Length; e++)
                {
                    var length = Draw(code[e] == '-' ? 3 * dit : dit, s);

                    (code[e] == '-' ? truth.Dahs : truth.Dits).Add(length);
                    keyed.Add((length, true));

                    if (e + 1 < code.Length)
                    {
                        var gap = Draw(dit, s);

                        truth.ElementGaps.Add(gap);
                        keyed.Add((gap, false));
                    }
                }

                if (c + 1 < words[w].Length)
                {
                    var gap = Draw(3 * dit, s);

                    truth.LetterGaps.Add(gap);
                    keyed.Add((gap, false));
                }
            }

            if (w + 1 < words.Length)
            {
                var gap = Draw(wordGapDits * dit, scatter(letter));

                truth.WordGaps.Add(gap);
                keyed.Add((gap, false));
            }
        }

        keyed.Add((3.0, false));

        var total = (int)(keyed.Sum(k => k.Seconds) * Rate) + Rate;
        var samples = new float[total];
        var amplitude = ThePatternIsTheGateTests.Over(24);
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

                    samples[at + i] = (float)(amplitude * shape * Math.Sin(2 * Math.PI * Pitch * (at + i) / Rate));
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

        return (samples, truth);
    }

    private static string MorseCode(char c) => c switch
    {
        'A' => ".-", 'C' => "-.-.", 'D' => "-..", 'E' => ".", 'K' => "-.-", 'L' => ".-..", 'N' => "-.",
        'Q' => "--.-", '0' => "-----",
        'B' => "-...", 'H' => "....", 'I' => "..", 'J' => ".---", 'M' => "--", 'S' => "...", 'T' => "-", 'U' => "..-",
        'V' => "...-", 'W' => ".--", '1' => ".----", '2' => "..---",
        _ => throw new ArgumentOutOfRangeException(nameof(c), c, "not in the call"),
    };

    private static string Stats(IEnumerable<double> seconds)
    {
        var logs = seconds.Select(s => Math.Log(s)).ToList();

        if (logs.Count == 0)
        {
            return "none";
        }

        var mean = logs.Average();
        var sd = Math.Sqrt(logs.Sum(l => (l - mean) * (l - mean)) / logs.Count);

        return $"{Math.Exp(mean) * 1000:0} ms ±{sd:0.00}";
    }

    private string Case(string name, int wpm, Func<int, double> scatter, int seed)
    {
        var (samples, truth) = Fist(Call, wpm, scatter, seed);
        var r = ThePatternIsTheGateTests.Read(samples, Pitch);

        _output.WriteLine($"{name}: {r.Candidates} candidates, {r.Stood} stood, {r.Printed} printed; reads `{r.Text}`");
        _output.WriteLine($"   true   : dit {Stats(truth.Dits)}, dah {Stats(truth.Dahs)}; gaps element {Stats(truth.ElementGaps)}, letter {Stats(truth.LetterGaps)}, word {Stats(truth.WordGaps)} (centre as a geometric mean, spread as the SD of the log)");
        _output.WriteLine($"   reader : {CwRunReader.LastClusters}");

        return r.Text;
    }

    /// <remarks>Case 1: the call at 27 WPM with a fist scattered by 20%.</remarks>
    [Fact]
    public void At27WordsAFistScatteredByAFifthReadsWhole()
        => Assert.Equal(Call, Case("27 WPM, 20% scatter", 27, _ => 0.2, 5130));

    /// <remarks>Case 2: the call at 27 WPM with a fist scattered by 30%, unit 504's scatter.</remarks>
    [Fact]
    public void At27WordsAFistScatteredByThreeTenthsReadsWhole()
        => Assert.Equal(Call, Case("27 WPM, 30% scatter", 27, _ => 0.3, 5131));

    /// <remarks>Case 3: the call at 12 and at 35 WPM with a fist scattered by 20%.</remarks>
    /// <param name="wpm">Speed.</param>
    [Theory]
    [InlineData(12)]
    [InlineData(35)]
    public void AtTwelveAndThirtyFiveAFistScatteredByAFifthReadsWhole(int wpm)
        => Assert.Equal(Call, Case($"{wpm} WPM, 20% scatter", wpm, _ => 0.2, 5132 + wpm));

    /// <remarks>Case 4: a fist that tightens mid-transmission, 30% for ten letters then 10%.</remarks>
    [Fact]
    public void AFistThatTightensIsFollowed()
        => Assert.Equal(Call, Case("27 WPM, 30% then 10%", 27, letter => letter < 10 ? 0.3 : 0.1, 5140));
}
