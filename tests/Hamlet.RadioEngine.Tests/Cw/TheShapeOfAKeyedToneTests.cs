using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **A mark is judged by its whole shape, not by crossing five lines** (work instruction 502, R110,
/// HM-DEC-206). The owner: *"CW is not any order. It's a particular shape and size and width."*
/// </summary>
/// <remarks>
/// **SYNTHETIC AUDIO WRITTEN HERE, NOTHING READ FROM DISK** (R96). The clean call and the loud
/// noise are unit 498's, built the same way; the weak station is the clean call twelve decibels
/// quieter, about 10 dB over the noise where the clean call is about 22.
/// </remarks>
public sealed class TheShapeOfAKeyedToneTests
{
    private const int Rate = 8000;
    private const int Chunk = 80;
    private const double Pitch = 625;
    private const string Call = "CQ CQ DE N0CALL N0CALL K";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the tables and scores are printed.</param>
    public TheShapeOfAKeyedToneTests(ITestOutputHelper output) => _output = output;

    private static float[] CleanCall(double amplitude = 0.5) => CwSignal.Generate(new CwSignalRequest(
        Call, WordsPerMinute: 23, ToneHz: Pitch, SampleRate: Rate, Amplitude: amplitude,
        NoiseAmplitude: 0.04, LeadInSeconds: 3, TailSeconds: 3, Seed: 490)).Samples;

    /// <summary>The clean call twelve decibels quieter: about 10 dB over the noise.</summary>
    private static float[] WeakCall() => CleanCall(0.5 * Math.Pow(10, -12 / 20.0));

    private static float[] NoiseAlone() => CwSignal.Generate(new CwSignalRequest(
        " ", SampleRate: Rate, Amplitude: 0, NoiseAmplitude: 0.3, LeadInSeconds: 15, TailSeconds: 15, Seed: 491)).Samples;

    private static IReadOnlyList<CwMark> Marks(float[] samples, bool edges = true, bool narrow = true, bool shape = true)
    {
        var detector = new CwEnvelopeDetector(Rate) { MarksNeedEdges = edges, MarksNeedNarrowness = narrow, MarksNeedShape = shape };

        for (var at = 0; at + Chunk <= samples.Length; at += Chunk)
        {
            detector.Process(samples.AsSpan(at, Chunk));
        }

        return detector.MarksSince(0).Marks;
    }

    /// <summary>The call's own marks: within a bin of its pitch and within 6 dB of the loudest there, as unit 498 picks them.</summary>
    private static List<CwMark> CallsOwn(IReadOnlyList<CwMark> marks)
    {
        var near = marks.Where(m => Math.Abs(m.PitchHz - Pitch) <= CwRunReader.PitchToleranceHz).ToList();
        var loudest = near.Max(m => m.LevelDb);

        return near.Where(m => m.LevelDb >= loudest - 6).ToList();
    }

    private static double Score(CwMark m) => m.Shape?.Score ?? double.NaN;

    /// <remarks>
    /// Case 1: thirty seconds of loud noise - unit 498's table with the shape score as its last
    /// column. Asserts that the shape turns away some of what passed all five.
    /// </remarks>
    [Fact]
    public void TheShapeTurnsAwayNoiseThatPassedFiveLines()
    {
        var samples = NoiseAlone();
        var seconds = samples.Length / (double)Rate;
        var older = Marks(samples, edges: false, narrow: false, shape: false).Count;
        var edged = Marks(samples, narrow: false, shape: false).Count;
        var narrow = Marks(samples, shape: false).Count;
        var shaped = Marks(samples).Count;

        _output.WriteLine($"thirty seconds of loud noise: passing the older tests {older}, with edges {edged}, narrow {narrow}, inside the shape {shaped}");
        _output.WriteLine($"marks handed out a second: {narrow / seconds:0.0} before, {shaped / seconds:0.0} after");

        Assert.True(shaped < narrow, "the shape turned away no noise bar that passed all five");
    }

    /// <remarks>
    /// Case 2: the scores of the clean call's own marks and of the noise bars that passed all five
    /// tests, with the shape off so none is missing; the lowest real score, the highest noise
    /// score, and how many of each sit on the other's side. Asserts every real mark clears the
    /// threshold.
    /// </remarks>
    [Fact]
    public void RealMarksScoreInsideTheShapeAndNoiseOutside()
    {
        var real = CallsOwn(Marks(CleanCall(), shape: false));
        var weak = CallsOwn(Marks(WeakCall(), shape: false));
        var noise = Marks(NoiseAlone(), shape: false);

        void Print(string name, IReadOnlyList<CwMark> marks)
        {
            var scores = marks.Select(Score).OrderBy(s => s).ToList();

            _output.WriteLine($"{name}: {scores.Count} marks, lowest {scores.First():0.000}, median {scores[scores.Count / 2]:0.000}, highest {scores.Last():0.000}");

            foreach (var m in marks.OrderBy(Score).Take(3))
            {
                _output.WriteLine($"  lowest: {m.FromSeconds:0.000} s {m.LengthMs:0} ms {m.Shape}");
            }

            foreach (var m in marks.OrderByDescending(Score).Take(3))
            {
                _output.WriteLine($"  highest: {m.FromSeconds:0.000} s {m.LengthMs:0} ms {m.Shape}");
            }
        }

        Print("clean call", real);
        Print("weak call", weak);
        Print("noise passing all five", noise);

        var realFloor = real.Concat(weak).Min(Score);
        var noiseTop = noise.Max(Score);

        _output.WriteLine($"lowest real {realFloor:0.000}, highest noise {noiseTop:0.000}; noise at or over the real floor {noise.Count(m => Score(m) >= realFloor)}, real at or under the noise top {real.Concat(weak).Count(m => Score(m) <= noiseTop)}; threshold {CwEnvelopeDetector.ShapeThreshold:0.000}");

        Assert.All(real.Concat(weak), m => Assert.True(Score(m) >= CwEnvelopeDetector.ShapeThreshold, $"a real mark at {m.FromSeconds:0.000} s scores {Score(m):0.000}"));
    }

    /// <remarks>
    /// The mark carries its score to the scope: six seconds into the clean call, the hops the scope
    /// draws under a called mark carry that mark's score, and the scores are the marks' own.
    /// </remarks>
    [Fact]
    public void TheScopeHopsCarryTheScore()
    {
        var samples = CleanCall();
        var detector = new CwEnvelopeDetector(Rate);

        for (var at = 0; at + Chunk <= Rate * 6; at += Chunk)
        {
            detector.Process(samples.AsSpan(at, Chunk));
        }

        var scored = detector.History().Where(h => !double.IsNaN(h.ShapeScore)).Select(h => h.ShapeScore).Distinct().ToList();
        var marks = detector.MarksSince(0).Marks.Select(m => m.Shape!.Score).ToList();

        _output.WriteLine($"six seconds in: {scored.Count} distinct scores on the scope's hops, {string.Join(", ", scored.Select(s => s.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)))}");

        Assert.NotEmpty(scored);
        Assert.All(scored, s => Assert.Contains(s, marks));
    }

    /// <remarks>
    /// Case 4: the weak station, about 10 dB over the noise, loses no mark to the shape: with the
    /// shape on it has every mark it has with the shape off.
    /// </remarks>
    [Fact]
    public void AWeakStationKeepsItsMarks()
    {
        var off = CallsOwn(Marks(WeakCall(), shape: false)).Count;
        var on = CallsOwn(Marks(WeakCall())).Count;
        var text = ACharacterIsARunOfMarksThatAgreeTests.Read(WeakCall(), runs: true);

        _output.WriteLine($"weak call: marks {off} with the shape off, {on} with it on; reads `{text}`");

        Assert.Equal(off, on);
    }
}
