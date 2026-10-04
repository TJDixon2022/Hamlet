using System.Globalization;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **A GREEN LIGHT SAYS HOLD STILL** (work instruction 521, task 2, HM-DEC-225).
/// </summary>
/// <remarks>
/// <para>**THE OWNER, 2026-10-01**: *"I want a green light whenever the first shape is being detected so that I
/// know to hold on that frequency and not adjust, because you're not hearing it."*</para>
/// <para>**ON THE LIVE PATH, SYNTHETIC AUDIO WRITTEN HERE** (R96): the detector and the run reader wired as the app
/// wires them, the reading following what the reader prints.</para>
/// </remarks>
public sealed class TheLightSaysHoldStillTests
{
    private const int Rate = 8000;
    private const int Chunk = 80;
    private const string Call = "CQ CQ DE N0CALL N0CALL K";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the light's sequence is printed.</param>
    public TheLightSaysHoldStillTests(ITestOutputHelper output) => _output = output;

    /// <summary>One step of the light: when, what it showed, and its count.</summary>
    internal sealed record Step(double Seconds, CwShapeLight Light, int Forming, double Fill = 0);

    internal static (List<Step> Steps, List<double> Letters, List<CwMark> Candidates) Run(float[] samples)
    {
        var detector = new CwEnvelopeDetector(Rate);
        var reader = new CwSenderGate();
        var steps = new List<Step>();
        var letters = new List<double>();
        var sequence = 0L;

        detector.PrintedPitch = () => reader.StationPitchHz;
        detector.WaitingPitch = () => reader.WaitingPitchHz;
        reader.CharacterRead += c =>
        {
            if (!c.IsWordGap)
            {
                letters.Add(steps.Count > 0 ? steps[^1].Seconds : 0);
            }
        };

        for (var at = 0; at + Chunk <= samples.Length; at += Chunk)
        {
            detector.Process(samples.AsSpan(at, Chunk));

            var batch = detector.MarksSince(sequence);

            sequence = batch.Marks.Count > 0 ? batch.Marks.Max(m => m.Sequence) : sequence;
            reader.Read(batch);

            var reading = detector.Reading;

            steps.Add(new Step((at + Chunk) / (double)Rate, reading.ShapeLight, reading.ShapeForming, reading.ShapeFill));
        }

        return (steps, letters, detector.CandidatesKept.ToList());
    }


    /// <remarks>
    /// A clean call at 20 WPM, 24 dB: dark before it; amber with its count climbing to four; green `shape found` at the
    /// fifth mark and before the first letter; green `reading` from the first letter; dark within two seconds and a
    /// half of the last mark.
    /// </remarks>
    [Fact]
    public void ACleanCallLightsInOrder()
    {
        var signal = CwSignal.Generate(new CwSignalRequest(
            Call, WordsPerMinute: 20, ToneHz: 625, SampleRate: Rate, Amplitude: ThePatternIsTheGateTests.Over(24),
            NoiseAmplitude: 0.04, LeadInSeconds: 3, TailSeconds: 4, Seed: 5213)).Samples;
        var (steps, letters, candidates) = Run(signal);
        var marks = candidates.Where(m => Math.Abs(m.PitchHz - 625) <= 50 && m.FromSeconds >= 2.9).OrderBy(m => m.FromSeconds).ToList();
        var lastMark = marks[^1].ToSeconds;
        var changes = steps.Where((s, i) => i == 0 || s.Light != steps[i - 1].Light || s.Forming != steps[i - 1].Forming).ToList();

        foreach (var s in changes.Where(c => c.Seconds > 2.5 && c.Seconds < 7 || c.Seconds > lastMark - 0.5))
        {
            _output.WriteLine(string.Create(CultureInfo.InvariantCulture, $"{s.Seconds:0.000} s  {CwShapeLights.Words(s.Light, s.Forming)}"));
        }

        _output.WriteLine(string.Create(CultureInfo.InvariantCulture, $"fifth mark of the call ends at {marks[4].ToSeconds:0.000} s; first letter printed at {letters[0]:0.000} s; last mark ends at {lastMark:0.000} s"));

        var firstFound = steps.First(s => s.Seconds > 3 && s.Light == CwShapeLight.Found).Seconds;
        var firstReading = steps.First(s => s.Light == CwShapeLight.Reading).Seconds;
        var forming = steps.Where(s => s.Seconds > 3 && s.Seconds < firstFound && s.Light == CwShapeLight.Forming).Select(s => s.Forming).ToList();

        Assert.Equal(CwShapeLight.Listening, steps.Last(s => s.Seconds < 2.9).Light);
        Assert.Contains(4, forming);
        Assert.True(firstFound < letters[0], "green before the first letter");
        Assert.True(firstReading >= letters[0], "reading from the first letter");
        Assert.Equal(CwShapeLight.Listening, steps.First(s => s.Seconds > lastMark + 2.5).Light);
    }

    /// <remarks>
    /// **THE LIGHT READS THE SCORE** (work instruction 522, task 2): a short sloppy keyed sequence - five and then a few
    /// more marks of scattered lengths and gaps - never turns the light green; it stays amber. **And since work
    /// instruction 524 it never stands at all** (HM-DEC-228): a sequence stands only on a shape of 0.2 or better, the line
    /// the light uses, so the sloppy sequence that stood under the line in unit 522 now holds its marks, and the gauge
    /// never fills past the mark.
    /// </remarks>
    [Fact]
    public void ALowScoringSequenceNeverTurnsTheLightGreen()
    {
        var noise = CwSignal.Generate(new CwSignalRequest(
            " ", SampleRate: Rate, Amplitude: 0, NoiseAmplitude: 0.04, LeadInSeconds: 4, TailSeconds: 4, Seed: 5218)).Samples;
        var samples = noise.ToArray();
        var amplitude = ThePatternIsTheGateTests.Over(24);
        var marks = new (double Ms, double GapMs)[] { (40, 60), (280, 40), (75, 150), (140, 30), (55, 100), (230, 0) };
        var at = 3.0;

        foreach (var (ms, gap) in marks)
        {
            for (var i = (int)(at * Rate); i < (int)((at + (ms / 1000)) * Rate); i++)
            {
                samples[i] += (float)(amplitude * Math.Sin(2 * Math.PI * 625 * i / Rate));
            }

            at += (ms + gap) / 1000;
        }

        var detector = new CwEnvelopeDetector(Rate);
        var steps = new List<(double Seconds, CwShapeLight Light, double Score, double Fill)>();

        for (var k = 0; k + Chunk <= samples.Length; k += Chunk)
        {
            detector.Process(samples.AsSpan(k, Chunk));
            steps.Add(((k + Chunk) / (double)Rate, detector.Reading.ShapeLight, detector.Reading.ShapeScore, detector.Reading.ShapeFill));
        }

        var standing = steps.Where(s => double.IsFinite(s.Score)).ToList();

        _output.WriteLine(string.Create(CultureInfo.InvariantCulture, $"a sloppy sequence: stood with shape {(standing.Count > 0 ? standing.Max(s => s.Score) : double.NaN):0.000} at best; green {steps.Count(s => s.Light is CwShapeLight.Found or CwShapeLight.Reading)} steps, amber {steps.Count(s => s.Light == CwShapeLight.Forming)}, fullest gauge {steps.Max(s => s.Fill):0.00}"));

        Assert.Empty(standing);
        Assert.DoesNotContain(steps, s => s.Light is CwShapeLight.Found or CwShapeLight.Reading);
        Assert.DoesNotContain(steps, s => s.Fill > CwShapeLights.Mark);
    }

    /// <remarks>
    /// **THE GAUGE** (work instruction 522, task 3): on a clean call the bar fills a fifth per mark toward the mark at
    /// four-fifths, crosses it at the fifth mark, climbs with the shape score, and is full from the first letter.
    /// </remarks>
    [Fact]
    public void TheGaugeFillsOnACleanCall()
    {
        var signal = CwSignal.Generate(new CwSignalRequest(
            Call, WordsPerMinute: 20, ToneHz: 625, SampleRate: Rate, Amplitude: ThePatternIsTheGateTests.Over(24),
            NoiseAmplitude: 0.04, LeadInSeconds: 3, TailSeconds: 4, Seed: 5213)).Samples;
        var (steps, letters, _) = Run(signal);
        var changes = steps.Where((s, i) => i > 0 && (Math.Abs(s.Fill - steps[i - 1].Fill) >= 0.01 || s.Light != steps[i - 1].Light || (s.Light == CwShapeLight.Found && i % 50 == 0))).ToList();

        foreach (var s in changes.Where(c => c.Seconds < letters[0] + 0.1))
        {
            _output.WriteLine(string.Create(CultureInfo.InvariantCulture, $"{s.Seconds:0.000} s  fill {s.Fill:0.00}  {CwShapeLights.Words(s.Light, s.Forming)}"));
        }

        var crossed = steps.First(s => s.Fill > CwShapeLights.Mark).Seconds;
        var before = steps.Where(s => s.Seconds > 3 && s.Seconds < crossed).Select(s => s.Fill).ToList();

        Assert.Contains(before, f => Math.Abs(f - 0.8) < 0.001);
        Assert.True(crossed < letters[0], "the gauge crosses the mark before the first letter");
        Assert.All(steps.Where(s => s.Seconds >= letters[0] + 0.05 && s.Seconds < letters[0] + 1), s => Assert.Equal(1, s.Fill));
    }
    /// <remarks>
    /// Loud noise alone, thirty seconds: never green, and the share of the time a forming sequence holds one, two,
    /// three and four marks, which is where amber should begin.
    /// </remarks>
    [Fact]
    public void LoudNoiseIsNeverGreen()
    {
        var noise = CwSignal.Generate(new CwSignalRequest(
            " ", SampleRate: Rate, Amplitude: 0, NoiseAmplitude: 0.3, LeadInSeconds: 15, TailSeconds: 15, Seed: 5212)).Samples;
        var (steps, _, _) = Run(noise);

        string Share(int least) => string.Create(CultureInfo.InvariantCulture, $"{100.0 * steps.Count(s => s.Forming >= least) / steps.Count:0.0}%");

        _output.WriteLine($"loud noise, 30 s: forming at 1 or more {Share(1)}, 2 or more {Share(2)}, 3 or more {Share(3)}, 4 {Share(4)}; green {steps.Count(s => s.Light is CwShapeLight.Found or CwShapeLight.Reading)} steps");

        Assert.DoesNotContain(steps, s => s.Light is CwShapeLight.Found or CwShapeLight.Reading);
        Assert.DoesNotContain(steps, s => s.Fill > CwShapeLights.Mark);
    }

    /// <remarks>
    /// **THE GAUGE CANNOT COUNT PAST FIVE** (work instruction 526, task 6): an SKCC straight key read `7 of 5` and `9 of 5`.
    /// Four marks forming say four of five; five or more without standing say not yet, amber, at the mark, and never
    /// hold here.
    /// </remarks>
    [Fact]
    public void TheGaugeCannotCountPastFive()
    {
        Assert.Equal("shape forming · 4 of 5", CwShapeLights.Words(CwShapeLight.Forming, 4));

        foreach (var forming in new[] { 5, 7, 9 })
        {
            Assert.Equal(CwShapeLights.NotYetWords, CwShapeLights.Words(CwShapeLight.Forming, forming));
            Assert.Equal(CwShapeLights.Mark, CwShapeLights.Fill(CwShapeLight.Forming, forming, double.NaN));
            Assert.DoesNotContain("hold", CwShapeLights.Words(CwShapeLight.Forming, forming), StringComparison.Ordinal);
        }
    }
}
