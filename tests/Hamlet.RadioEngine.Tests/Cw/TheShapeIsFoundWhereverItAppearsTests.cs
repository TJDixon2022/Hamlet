using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **The shape is found wherever it appears; the watched bin retires** (work instruction 515, R114,
/// HM-DEC-219).
/// </summary>
/// <remarks>
/// <para>**TIM, 2026-09-30**: *"I'm wondering why we're so focused on pitch. Pitch almost doesn't matter.
/// It's shape."*</para>
/// <para>**SYNTHETIC AUDIO WRITTEN HERE, NOTHING READ FROM DISK** (R96), read by the detector and the
/// reader with **nothing pointed and nothing followed**: the detector is told the passband, as the
/// application tells it from the rig state, and nothing else.</para>
/// </remarks>
public sealed class TheShapeIsFoundWhereverItAppearsTests
{
    private const int Rate = 8000;
    private const int Chunk = 80;
    private const string Call = "CQ CQ DE N0CALL N0CALL K";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the table is printed.</param>
    public TheShapeIsFoundWhereverItAppearsTests(ITestOutputHelper output) => _output = output;

    /// <summary>What one run gave: the text, the counts, and the pitch the reading named while it said keying.</summary>
    internal sealed record Run(string Text, int Stood, int Keyed, IReadOnlyList<double> Pitches, int KeyingReadings);

    /// <summary>The detector and the reader over the audio, nothing pointed or followed.</summary>
    internal static Run Read(float[] samples, double? passbandPitch = 600, double? passbandWidth = 500)
    {
        var detector = new CwEnvelopeDetector(Rate);

        detector.SetPassband(passbandPitch, passbandWidth);

        var reader = new CwRunReader();
        var characters = new List<CwCharacter>();
        var pitches = new List<double>();
        var keying = 0;
        var sequence = 0L;

        reader.CharacterRead += characters.Add;

        for (var at = 0; at + Chunk <= samples.Length; at += Chunk)
        {
            detector.Process(samples.AsSpan(at, Chunk));

            var reading = detector.Reading;

            if (reading.Keying)
            {
                keying++;
                pitches.Add(reading.PitchHz);
            }

            var batch = detector.MarksSince(sequence);

            sequence = batch.Marks.Count > 0 ? batch.Marks.Max(m => m.Sequence) : sequence;
            reader.Read(batch);
        }

        reader.Flush();

        var marks = detector.MarksSince(0).Marks;
        var text = string.Join(' ', string.Concat(characters.Select(c => c.Text)).Split(' ', StringSplitOptions.RemoveEmptyEntries));

        return new Run(text, marks.Count, marks.Count(m => m.Keyed), pitches, keying);
    }

    private static string Named(IReadOnlyList<double> pitches)
        => pitches.Count == 0 ? "none"
            : string.Join(", ", pitches.Where(double.IsFinite).GroupBy(p => p).OrderByDescending(g => g.Count()).Take(3)
                .Select(g => $"{g.Key:0} Hz x{g.Count()}"));

    /// <remarks>
    /// Case 1: the call at 425, 500, 600, 700 and 775 Hz through a 500 Hz passband on 600, nothing
    /// pointed or followed: it reads whole, and the reading's pitch while keying is the station's.
    /// </remarks>
    /// <param name="pitchHz">The station's pitch.</param>
    [Theory]
    [InlineData(425.0)]
    [InlineData(500.0)]
    [InlineData(600.0)]
    [InlineData(700.0)]
    [InlineData(775.0)]
    public void ACallAtAnyPitchInTheFilterIsFoundAndRead(double pitchHz)
    {
        var r = Read(NarrownessReadsTheFiltersBandTests.CallAt(pitchHz, 5150 + (int)pitchHz));
        var named = r.Pitches.Where(double.IsFinite).ToList();
        var mostly = named.Count == 0 ? double.NaN : named.GroupBy(p => p).OrderByDescending(g => g.Count()).First().Key;

        _output.WriteLine($"{pitchHz:0} Hz: {r.Stood} stood, {r.Keyed} keyed; keying on {r.KeyingReadings} readings, pitch named {Named(r.Pitches)}; reads `{r.Text}`");

        Assert.Equal(Call, r.Text);
        Assert.True(Math.Abs(mostly - pitchHz) <= CwEnvelopeDetector.BinSpacingHz, $"the reading named {mostly:0} Hz for a station at {pitchHz:0}");
    }

    /// <summary>The call keyed at a pitch that drifts linearly from one frequency to another over the whole message, as a hand VFO does.</summary>
    internal static float[] Drifting(double fromHz, double toHz, int wpm, int seed)
    {
        var code = new Dictionary<char, string>
        {
            ['A'] = ".-", ['C'] = "-.-.", ['D'] = "-..", ['E'] = ".", ['K'] = "-.-", ['L'] = ".-..",
            ['N'] = "-.", ['Q'] = "--.-", ['0'] = "-----",
        };
        var dit = 1.2 / wpm;
        var keyed = new List<(double Seconds, bool On)> { (3, false) };

        foreach (var word in Call.Split(' '))
        {
            foreach (var c in word)
            {
                foreach (var e in code[c])
                {
                    keyed.Add((e == '-' ? 3 * dit : dit, true));
                    keyed.Add((dit, false));
                }

                keyed.Add((2 * dit, false));
            }

            keyed.Add((4 * dit, false));
        }

        keyed.Add((3, false));

        var total = keyed.Sum(k => (int)Math.Round(k.Seconds * Rate));
        var samples = new float[total];
        var amplitude = ThePatternIsTheGateTests.Over(24);
        var edge = (int)(0.004 * Rate);
        var phase = 0.0;
        var at = 0;

        foreach (var (seconds, on) in keyed)
        {
            var n = (int)Math.Round(seconds * Rate);

            for (var i = 0; i < n; i++)
            {
                var hz = fromHz + ((toHz - fromHz) * (at + i) / (double)total);

                phase += 2 * Math.PI * hz / Rate;

                if (on)
                {
                    var shape = i < edge ? 0.5 - (0.5 * Math.Cos(Math.PI * i / edge))
                        : i >= n - edge ? 0.5 - (0.5 * Math.Cos(Math.PI * (n - i) / edge))
                        : 1.0;

                    samples[at + i] = (float)(amplitude * shape * Math.Sin(phase));
                }
            }

            at += n;
        }

        var noise = new Random(seed);

        for (var i = 0; i < samples.Length; i++)
        {
            var u1 = 1.0 - noise.NextDouble();
            var u2 = noise.NextDouble();

            samples[i] += (float)(0.04 * Math.Sqrt(-2 * Math.Log(u1)) * Math.Cos(2 * Math.PI * u2));
        }

        return samples;
    }

    /// <remarks>Case 2: a station that drifts from 500 to 560 Hz over the call reads whole, and the reading's pitch follows it.</remarks>
    [Fact]
    public void AStationThatDriftsIsFollowedByItsShape()
    {
        var r = Read(NarrownessReadsTheFiltersBandTests.ThroughTheFilter(Drifting(500, 560, 20, 5160)));
        var named = r.Pitches.Where(double.IsFinite).ToList();
        var early = named.Take(named.Count / 4).DefaultIfEmpty(double.NaN).Average();
        var late = named.Skip(3 * named.Count / 4).DefaultIfEmpty(double.NaN).Average();

        _output.WriteLine($"500 to 560 Hz: {r.Stood} stood, {r.Keyed} keyed; pitch named first quarter {early:0} Hz, last quarter {late:0} Hz; reads `{r.Text}`");

        Assert.Equal(Call, r.Text);
        Assert.True(late > early, $"the reading's pitch went from {early:0} to {late:0} Hz while the station rose");
    }

    /// <remarks>Case 3: two stations 200 Hz apart, as unit 507: the loud one prints whole and both stand.</remarks>
    [Fact]
    public void TwoStationsBothStandAndTheLoudPrints()
    {
        var loud = CwSignal.Generate(new CwSignalRequest(
            Call, WordsPerMinute: 20, ToneHz: 625, SampleRate: Rate, Amplitude: ThePatternIsTheGateTests.Over(24),
            NoiseAmplitude: 0.04, LeadInSeconds: 3, TailSeconds: 3, Seed: 5110)).Samples;
        var quiet = CwSignal.Generate(new CwSignalRequest(
            "TEST DE W1AW K TEST DE W1AW K", WordsPerMinute: 20, ToneHz: 825, SampleRate: Rate, Amplitude: ThePatternIsTheGateTests.Over(10),
            NoiseAmplitude: 0, LeadInSeconds: 3, TailSeconds: 3, Seed: 5111)).Samples;
        var mixed = new float[Math.Max(loud.Length, quiet.Length)];

        for (var i = 0; i < mixed.Length; i++)
        {
            mixed[i] = (i < loud.Length ? loud[i] : 0) + (i < quiet.Length ? quiet[i] : 0);
        }

        var atLoud = ThePatternIsTheGateTests.Read(mixed, 625);
        var atQuiet = ThePatternIsTheGateTests.Read(mixed, 825);

        _output.WriteLine($"two stations: 625 Hz {atLoud.Stood} stood, 825 Hz {atQuiet.Stood} stood; reads `{atLoud.Text}`");

        Assert.Equal(Call, atLoud.Text);
        Assert.True(atLoud.Stood > 0 && atQuiet.Stood > 0, "both stations stand");
    }
}
