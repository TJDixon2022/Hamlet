using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **A sender's gaps are its own dit times one, three and seven** (work instruction 500,
/// HM-DEC-204). W1AW's slow code practice read as single-element letters; every synthetic case from
/// unit 490 to unit 498 was built between 9 and 23 WPM.
/// </summary>
/// <remarks>
/// **SYNTHETIC AUDIO WRITTEN HERE, NOTHING READ FROM DISK** (R96). Each case prints the text, the
/// sender's true dit and the dit the reader measured, so a failure says whether the measurement or
/// the arithmetic was at fault.
/// </remarks>
public sealed class TheGapsBelongToTheSendersOwnDitTests
{
    private const int Rate = 8000;
    private const int Chunk = 80;
    private const string Call = "CQ CQ DE N0CALL N0CALL K";
    private const string Answer = "TEST DE W1AW K";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where each speed's reading is printed.</param>
    public TheGapsBelongToTheSendersOwnDitTests(ITestOutputHelper output) => _output = output;

    private static float[] Station(string text, int wpm, int seed, double lead = 3, double tail = 3)
        => CwSignal.Generate(new CwSignalRequest(
            text, WordsPerMinute: wpm, ToneHz: 625, SampleRate: Rate, Amplitude: 0.5,
            NoiseAmplitude: 0.04, LeadInSeconds: lead, TailSeconds: tail, Seed: seed)).Samples;

    /// <summary>What the run reader prints, and the dit it measured for the sender it printed.</summary>
    internal static (string Text, double DitSeconds, double GapDitSeconds) Read(float[] samples)
    {
        var detector = new CwEnvelopeDetector(Rate);
        var reader = new CwRunReader();
        var characters = new List<CwCharacter>();
        var sequence = 0L;
        var dit = double.NaN;
        var gapDit = double.NaN;

        reader.CharacterRead += characters.Add;

        for (var at = 0; at + Chunk <= samples.Length; at += Chunk)
        {
            detector.Process(samples.AsSpan(at, Chunk));

            var batch = detector.MarksSince(sequence);

            sequence = batch.Marks.Count > 0 ? batch.Marks.Max(m => m.Sequence) : sequence;
            reader.Read(batch);

            if (!double.IsNaN(reader.StationDitSeconds))
            {
                dit = reader.StationDitSeconds;
                gapDit = reader.StationGapDitSeconds;
            }
        }

        reader.Flush();

        var text = string.Join(' ', string.Concat(characters.Select(c => c.Text)).Split(' ', StringSplitOptions.RemoveEmptyEntries));

        return (text, dit, gapDit);
    }

    private string Reads(string sent, int wpm, float[] samples)
    {
        var (text, dit, gapDit) = Read(samples);

        _output.WriteLine($"{wpm,2} WPM  true dit {1200.0 / wpm,5:0} ms, measured {dit * 1000,5:0} ms on the marks, {gapDit * 1000,5:0} ms on the gaps  sent `{sent}`  read `{text}`");

        return text;
    }

    /// <remarks>Case 1: the call reads whole at 5, 10, 18 and 35 WPM.</remarks>
    [Theory]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(18)]
    [InlineData(35)]
    public void TheCallReadsWholeAtEverySpeed(int wpm)
        => Assert.Equal(Call, Reads(Call, wpm, Station(Call, wpm, 500 + wpm)));

    /// <remarks>Case 2: TEST DE W1AW K, unit 498's case at 23 WPM, at 5 and 10.</remarks>
    [Theory]
    [InlineData(5)]
    [InlineData(10)]
    public void TestDeW1awKReadsWholeWhenSlow(int wpm)
        => Assert.Equal(Answer, Reads(Answer, wpm, Station(Answer, wpm, 600 + wpm)));

    /// <remarks>
    /// Case 3: one sender that steps from 10 WPM to 20 WPM mid-transmission, as code practice does:
    /// the call at 10, a word gap at 10, and the answer at 20, at one pitch and one level.
    /// </remarks>
    [Fact]
    public void ASenderWhoSpeedsUpReadsWhole()
    {
        var slow = Station(Call, 10, 710, lead: 3, tail: 7 * 0.12);
        var fast = Station(Answer, 20, 720, lead: 0, tail: 3);
        var samples = new float[slow.Length + fast.Length];

        slow.CopyTo(samples, 0);
        fast.CopyTo(samples, slow.Length);

        var sent = Call + " " + Answer;
        var (text, dit, gapDit) = Read(samples);

        _output.WriteLine($"10 then 20 WPM  true dits 120 then 60 ms, measured at the end {dit * 1000:0} ms on the marks, {gapDit * 1000:0} ms on the gaps  sent `{sent}`  read `{text}`");

        Assert.Equal(sent, text);
    }
}
