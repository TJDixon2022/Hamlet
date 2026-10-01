using System.Globalization;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **A weak mark is a rectangle fitted as a whole, not checked hop by hop** (work instruction 516, R115,
/// HM-DEC-220).
/// </summary>
/// <remarks>
/// <para>**THE OWNER, R115**: *"Focus on shape. If you get the shape, the decode comes."*</para>
/// <para>**SYNTHETIC AUDIO WRITTEN HERE, NOTHING READ FROM DISK** (R96): unit 507's call at 20 WPM at 8,
/// 12, 16 and 24 dB over the noise on unit 502's scale with its seeds, read with the fit off and on;
/// a noiseless twin of each gives every element's true span, where the fit's score is read.</para>
/// </remarks>
public sealed class TheRectangleIsFittedTests
{
    private const int Rate = 8000;
    private const int Chunk = 80;
    private const double Pitch = 625;
    private const string Call = "CQ CQ DE N0CALL N0CALL K";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the tables are printed.</param>
    public TheRectangleIsFittedTests(ITestOutputHelper output) => _output = output;

    private static float[] Standard(double db, double noise, int seed)
        => CwSignal.Generate(new CwSignalRequest(
            Call, WordsPerMinute: 20, ToneHz: Pitch, SampleRate: Rate, Amplitude: ThePatternIsTheGateTests.Over(db),
            NoiseAmplitude: noise, LeadInSeconds: 3, TailSeconds: 3, Seed: seed)).Samples;

    /// <summary>Each element's true span in seconds, from the noiseless twin.</summary>
    private static List<(double From, double To)> Spans(float[] clean)
    {
        var spans = new List<(double, double)>();
        var start = -1;
        var quiet = Rate;

        for (var i = 0; i < clean.Length; i++)
        {
            var on = Math.Abs(clean[i]) > 1e-6;

            if (on && start < 0 && quiet > Rate / 500)
            {
                start = i;
            }

            if (on)
            {
                quiet = 0;
            }
            else
            {
                quiet++;

                if (start >= 0 && quiet == Rate / 500)
                {
                    spans.Add((start / (double)Rate, (i - quiet + 1) / (double)Rate));
                    start = -1;
                }
            }
        }

        return spans;
    }

    /// <summary>What a run gave: the text and the marks that stood at the pitch, and how many the fit found.</summary>
    internal sealed record Run(string Text, int Stood, int Fitted, int CandidatesAll, int FittedCandidatesAll, int StoodAll);

    internal static Run Read(float[] samples, bool fit)
    {
        var detector = new CwEnvelopeDetector(Rate) { MarksMayBeFitted = fit };
        var reader = new CwRunReader();
        var characters = new List<CwCharacter>();
        var sequence = 0L;

        reader.CharacterRead += characters.Add;

        for (var at = 0; at + Chunk <= samples.Length; at += Chunk)
        {
            detector.Process(samples.AsSpan(at, Chunk));

            var batch = detector.MarksSince(sequence);

            sequence = batch.Marks.Count > 0 ? batch.Marks.Max(m => m.Sequence) : sequence;
            reader.Read(batch);
        }

        reader.Flush();

        var marks = detector.MarksSince(0).Marks.Where(m => Math.Abs(m.PitchHz - Pitch) <= CwRunReader.PitchToleranceHz).ToList();
        var text = string.Join(' ', string.Concat(characters.Select(c => c.Text)).Split(' ', StringSplitOptions.RemoveEmptyEntries));

        return new Run(text, marks.Count, marks.Count(m => m.Fitted), detector.CandidateCount, detector.CandidatesKept.Count(m => m.Fitted), detector.StoodCount);
    }

    /// <summary>The fit's score at every element's true span, read just after each ends.</summary>
    private static List<double> RealScores(float[] samples, IReadOnlyList<(double From, double To)> spans)
    {
        var detector = new CwEnvelopeDetector(Rate) { MarksMayBeFitted = false };
        var scores = new List<double>();
        var next = 0;

        for (var at = 0; at + Chunk <= samples.Length && next < spans.Count; at += Chunk)
        {
            detector.Process(samples.AsSpan(at, Chunk));

            var heard = (at + Chunk) / (double)Rate;

            while (next < spans.Count && heard >= spans[next].To + 0.06)
            {
                var s = detector.FitScoreAt(Pitch, spans[next].From, spans[next].To);

                if (double.IsFinite(s))
                {
                    scores.Add(s);
                }

                next++;
            }
        }

        return scores;
    }

    /// <summary>The fit's score at dit and dah spans every 25 ms across noise alone.</summary>
    private static List<double> NoiseScores(float[] noise)
    {
        var detector = new CwEnvelopeDetector(Rate) { MarksMayBeFitted = false };
        var scores = new List<double>();

        for (var at = 0; at + Chunk <= noise.Length; at += Chunk)
        {
            detector.Process(noise.AsSpan(at, Chunk));

            var heard = (at + Chunk) / (double)Rate;

            if (heard > 1 && at % (Rate / 40) == 0)
            {
                foreach (var ms in new[] { 60.0, 180.0 })
                {
                    var to = heard - 0.06;
                    var s = detector.FitScoreAt(Pitch, to - (ms / 1000), to);

                    if (double.IsFinite(s))
                    {
                        scores.Add(s);
                    }
                }
            }
        }

        return scores;
    }

    private static string Describe(List<double> scores)
    {
        if (scores.Count == 0)
        {
            return "none";
        }

        var sorted = scores.OrderBy(s => s).ToList();

        string At(double q) => sorted[(int)Math.Clamp(Math.Floor(q * (sorted.Count - 1)), 0, sorted.Count - 1)].ToString("0.000", CultureInfo.InvariantCulture);

        return $"{sorted.Count} spans: lowest {At(0)}, 5% {At(0.05)}, median {At(0.5)}, 95% {At(0.95)}, highest {At(1)}";
    }

    /// <remarks>
    /// The distributions: the fit's score at every real mark of the call at 24, 16, 12 and 8 dB, and at dit and
    /// dah spans across noise at the call's own noise level and at the noise tests' loud level.
    /// </remarks>
    [Fact]
    public void TheFitScoresOfRealMarksAndOfNoise()
    {
        foreach (var db in new[] { 24.0, 16.0, 12.0, 8.0 })
        {
            var seed = 5070 + (int)db;
            var spans = Spans(Standard(db, 0, seed));
            var scores = RealScores(Standard(db, 0.04, seed), spans);

            _output.WriteLine($"real marks at {db} dB: {Describe(scores)}; under the threshold {scores.Count(s => s < CwEnvelopeDetector.FitThreshold)}");
        }

        foreach (var (noise, seed) in new[] { (0.04, 5200), (0.3, 5201) })
        {
            var samples = CwSignal.Generate(new CwSignalRequest(
                " ", SampleRate: Rate, Amplitude: 0, NoiseAmplitude: noise, LeadInSeconds: 15, TailSeconds: 15, Seed: seed)).Samples;
            var scores = NoiseScores(samples);

            _output.WriteLine($"noise at {noise}: {Describe(scores)}; at or over the threshold {scores.Count(s => s >= CwEnvelopeDetector.FitThreshold)}");
        }
    }

    /// <remarks>
    /// The strength table, unit 507's: the call at 8, 12, 16 and 24 dB, with the fit off and on - how many of
    /// the 65 marks stood, how many the fit found, and what reads.
    /// </remarks>
    /// <param name="db">Decibels over the noise.</param>
    [Theory]
    [InlineData(24.0)]
    [InlineData(16.0)]
    [InlineData(14.0)]
    [InlineData(12.0)]
    [InlineData(10.0)]
    [InlineData(8.0)]
    public void TheStrengthTableWithTheFitOffAndOn(double db)
    {
        var samples = Standard(db, 0.04, 5070 + (int)db);
        var off = Read(samples, fit: false);
        var on = Read(samples, fit: true);

        _output.WriteLine($"{db} dB, fit off: {off.Stood} stood; reads `{off.Text}`");
        _output.WriteLine($"{db} dB, fit on : {on.Stood} stood, {on.Fitted} of them fitted; reads `{on.Text}`");

        // The floor the fit reaches is asserted; below it the rows are printed for the report, not held.
        if (db >= 10)
        {
            Assert.Equal(Call, on.Text);
        }
    }

    private const string Bulletin = "THE QUICK BROWN FOX JUMPS OVER THE LAZY DOG 0123456789";

    /// <summary>
    /// A keyed call whose every mark opens 8 dB high and settles over 25 ms, as a receiver's AGC overshoots
    /// at key-down on a strong signal, with the band's noise added after: a dah whose top is not flat.
    /// </summary>
    private static float[] AgcOvershoot(string text, int wpm, double db, int seed, double overshootDb)
    {
        var clean = CwSignal.Generate(new CwSignalRequest(
            text, WordsPerMinute: wpm, ToneHz: Pitch, SampleRate: Rate, Amplitude: ThePatternIsTheGateTests.Over(db),
            NoiseAmplitude: 0, LeadInSeconds: 3, TailSeconds: 3, Seed: seed)).Samples;
        var noise = CwSignal.Generate(new CwSignalRequest(
            " ", SampleRate: Rate, Amplitude: 0, NoiseAmplitude: 0.04, LeadInSeconds: clean.Length / (double)Rate, TailSeconds: 1, Seed: seed + 1)).Samples;
        var output = new float[clean.Length];
        var quiet = Rate;
        var since = 0;

        for (var i = 0; i < clean.Length; i++)
        {
            if (Math.Abs(clean[i]) > 1e-6)
            {
                since = quiet > Rate / 500 ? 0 : since + 1;
                quiet = 0;
            }
            else
            {
                quiet++;
                since++;
            }

            var gain = Math.Pow(10, overshootDb * Math.Exp(-since / (0.025 * Rate)) / 20);

            output[i] = (float)((clean[i] * gain) + noise[i]);
        }

        return output;
    }

    /// <remarks>
    /// **THE GATE AGAINST WHAT HAPPENED ON THE AIR** (work instruction 517): a strong machine-sent bulletin
    /// at 18 WPM, 24 dB over the noise, reads identically with the fit on and off, every dah intact. Unit
    /// 516's fit on the air took W1AW at 18 WPM to dits only. Read plain, through the 500 Hz filter on 600,
    /// and fading slowly by 6 dB, the AGC's and the band's shapes on a dah top.
    /// </remarks>
    /// <param name="through">Which way the bulletin reaches the detector.</param>
    [Theory]
    [InlineData("plain")]
    [InlineData("filter")]
    [InlineData("fading")]
    [InlineData("agc-2")]
    [InlineData("agc-3")]
    [InlineData("agc-4")]
    [InlineData("agc-6")]
    [InlineData("agc-2-filter")]
    [InlineData("agc-3-filter")]
    [InlineData("agc-4-filter")]
    [InlineData("agc-6-filter")]
    public void AStrongBulletinReadsTheSameWithTheFitOnAndOff(string through)
    {
        var samples = CwSignal.Generate(new CwSignalRequest(
            Bulletin, WordsPerMinute: 18, ToneHz: Pitch, SampleRate: Rate, Amplitude: ThePatternIsTheGateTests.Over(24),
            NoiseAmplitude: 0.04, LeadInSeconds: 3, TailSeconds: 3, Seed: 5170)).Samples;

        if (through == "filter")
        {
            samples = NarrownessReadsTheFiltersBandTests.ThroughTheFilter(samples);
        }
        else if (through == "fading")
        {
            for (var i = 0; i < samples.Length; i++)
            {
                samples[i] *= (float)Math.Pow(10, -3 * (1 - Math.Cos(2 * Math.PI * i / (Rate * 4.0))) / 20);
            }
        }
        else if (through.StartsWith("agc", StringComparison.Ordinal))
        {
            samples = AgcOvershoot(Bulletin, 18, 24, 5170, double.Parse(through.Split('-')[1], System.Globalization.CultureInfo.InvariantCulture));

            if (through.EndsWith("-filter", StringComparison.Ordinal))
            {
                samples = NarrownessReadsTheFiltersBandTests.ThroughTheFilter(samples);
            }
        }

        var off = Read(samples, fit: false);
        var on = Read(samples, fit: true);

        _output.WriteLine($"bulletin, {through}, fit off: {off.Stood} stood; reads `{off.Text}`");
        _output.WriteLine($"bulletin, {through}, fit on : {on.Stood} stood, {on.Fitted} of them fitted; reads `{on.Text}`");

        // An AGC overshoot of 3 dB or more breaks the per-hop path itself, into dits; there the fit is meant to
        // fill, and the rows are printed for the report rather than held identical.
        if (through is "agc-3" or "agc-4" or "agc-6" or "agc-3-filter" or "agc-4-filter" or "agc-6-filter")
        {
            return;
        }

        Assert.Equal(off.Text, on.Text);
        Assert.Equal(off.Stood, on.Stood);
    }

    /// <remarks>Both noise cases print nothing with the fit on; the candidates the fit adds are counted.</remarks>
    /// <param name="seconds">How long.</param>
    [Theory]
    [InlineData(30)]
    [InlineData(180)]
    public void NoiseStillPrintsNothing(int seconds)
    {
        var noise = CwSignal.Generate(new CwSignalRequest(
            " ", SampleRate: Rate, Amplitude: 0, NoiseAmplitude: 0.3, LeadInSeconds: seconds / 2.0, TailSeconds: seconds / 2.0, Seed: 5100 + seconds)).Samples;
        var off = Read(noise, fit: false);
        var on = Read(noise, fit: true);

        _output.WriteLine($"{seconds} s of loud noise, fit off: {off.CandidatesAll} candidates, {off.StoodAll} stood; printed `{off.Text}`");
        _output.WriteLine($"{seconds} s of loud noise, fit on : {on.CandidatesAll} candidates, {on.FittedCandidatesAll} of them fitted, {on.StoodAll} stood; printed `{on.Text}`");

        Assert.Equal(string.Empty, on.Text);
    }

    /// <remarks>A fading station: the call at 24 dB fading to 10 dB and back over its length, as the Quebec station did.</remarks>
    [Fact]
    public void AFadingStationReads()
    {
        var keyed = Standard(24, 0, 5300);
        var noise = CwSignal.Generate(new CwSignalRequest(
            " ", SampleRate: Rate, Amplitude: 0, NoiseAmplitude: 0.04, LeadInSeconds: keyed.Length / (double)Rate, TailSeconds: 1, Seed: 5301)).Samples;
        var samples = new float[keyed.Length];

        for (var i = 0; i < keyed.Length; i++)
        {
            var down = 14 * 0.5 * (1 - Math.Cos(2 * Math.PI * i / keyed.Length));

            samples[i] = (float)((keyed[i] * Math.Pow(10, -down / 20)) + noise[i]);
        }

        var off = Read(samples, fit: false);
        var on = Read(samples, fit: true);

        _output.WriteLine($"24 to 10 dB and back, fit off: {off.Stood} stood; reads `{off.Text}`");
        _output.WriteLine($"24 to 10 dB and back, fit on : {on.Stood} stood, {on.Fitted} of them fitted; reads `{on.Text}`");

        Assert.Equal(Call, on.Text);
    }
}
