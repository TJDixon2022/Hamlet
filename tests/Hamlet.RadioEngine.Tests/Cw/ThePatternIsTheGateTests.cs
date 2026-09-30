using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **The pattern across marks is the gate; a single-mark test is proportional, never a decibel figure**
/// (work instruction 507, R112, HM-DEC-210). The owner: *"Chaos and noise have no patterns. All we have
/// to do is clearly identify when we have a pattern, a shape, and the translation is easy."*
/// </summary>
/// <remarks>
/// <para>**SYNTHETIC AUDIO WRITTEN HERE, NOTHING READ FROM DISK** (R96).</para>
/// <para>**DECIBELS OVER THE NOISE ON ONE SCALE, UNIT 502'S**: the call at amplitude 0.5 over noise of
/// amplitude 0.04 is about 22 dB over the noise as the detector's own bin measures it, so a station
/// X dB over is that call at 0.5 times 10^((X - 22)/20), and the noise is the same in every case.</para>
/// <para>**EVERY SYNTHETIC CASE BEFORE THIS UNIT WAS 20 dB OR MORE OVER THE NOISE**, and the gates were
/// set with those in front of them. The weak cases are the point.</para>
/// </remarks>
public sealed class ThePatternIsTheGateTests
{
    private const int Rate = 8000;
    private const int Chunk = 80;
    private const double Pitch = 625;
    private const string Call = "CQ CQ DE N0CALL N0CALL K";
    private const double NoiseAmplitude = 0.04;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the tables are printed.</param>
    public ThePatternIsTheGateTests(ITestOutputHelper output) => _output = output;

    /// <summary>The amplitude of a station this many dB over the noise, on unit 502's scale.</summary>
    internal static double Over(double db) => 0.5 * Math.Pow(10, (db - 22) / 20);

    private static float[] Standard(string text, int wpm, double db, int seed, double pitch = Pitch, double noise = NoiseAmplitude)
        => CwSignal.Generate(new CwSignalRequest(
            text, WordsPerMinute: wpm, ToneHz: pitch, SampleRate: Rate, Amplitude: Over(db),
            NoiseAmplitude: noise, LeadInSeconds: 3, TailSeconds: 3, Seed: seed)).Samples;

    /// <summary>18 WPM letters with the spaces stretched by PARIS to the overall speed (unit 501's build).</summary>
    private static float[] Farnsworth(string text, double overallWpm, double db, int seed)
    {
        const int letterWpm = 18;
        var unit = ((60 / overallWpm) - (31 * 1.2 / letterWpm)) / 19;
        var words = text.Split(' ');
        var pieces = new List<float[]>();

        for (var w = 0; w < words.Length; w++)
        {
            for (var c = 0; c < words[w].Length; c++)
            {
                var lastInWord = c == words[w].Length - 1;
                var tail = !lastInWord ? 3 * unit : w < words.Length - 1 ? 7 * unit : 3;

                pieces.Add(CwSignal.Generate(new CwSignalRequest(
                    words[w][c].ToString(), WordsPerMinute: letterWpm, ToneHz: Pitch, SampleRate: Rate,
                    Amplitude: Over(db), NoiseAmplitude: NoiseAmplitude,
                    LeadInSeconds: pieces.Count == 0 ? 3 : 0, TailSeconds: tail,
                    Seed: seed + pieces.Count)).Samples);
            }
        }

        return Join(pieces);
    }

    private static float[] Join(IEnumerable<float[]> pieces)
    {
        var list = pieces.ToList();
        var samples = new float[list.Sum(p => p.Length)];
        var at = 0;

        foreach (var piece in list)
        {
            piece.CopyTo(samples, at);
            at += piece.Length;
        }

        return samples;
    }

    /// <summary>What one run of the detector and the reader over the audio gives.</summary>
    internal sealed record Reading(string Text, int Candidates, int Stood, int Printed, IReadOnlyList<CwCharacter> Characters, int MostMarks4s);

    /// <summary>Runs the detector and the reader, and counts the marks at a pitch before and after the pattern.</summary>
    internal static Reading Read(float[] samples, double pitch = Pitch, Action<CwEnvelopeDetector>? setUp = null)
    {
        var detector = new CwEnvelopeDetector(Rate);

        setUp?.Invoke(detector);

        var reader = new CwRunReader();
        var characters = new List<CwCharacter>();
        var printedMarks = 0;
        var sequence = 0L;
        var most = 0;

        reader.CharacterRead += characters.Add;
        reader.RunRead += (_, run) => printedMarks += run.Count;

        for (var at = 0; at + Chunk <= samples.Length; at += Chunk)
        {
            detector.Process(samples.AsSpan(at, Chunk));

            var batch = detector.MarksSince(sequence);

            sequence = batch.Marks.Count > 0 ? batch.Marks.Max(m => m.Sequence) : sequence;
            reader.Read(batch);
            most = Math.Max(most, detector.Reading.MarksLast4s);
        }

        reader.Flush();

        var all = detector.MarksSince(0).Marks;
        var candidates = all.Count(m => Math.Abs(m.PitchHz - pitch) <= CwRunReader.PitchToleranceHz);
        var stood = all.Count(m => Math.Abs(m.PitchHz - pitch) <= CwRunReader.PitchToleranceHz && m.Stood);
        var text = string.Join(' ', string.Concat(characters.Select(c => c.Text)).Split(' ', StringSplitOptions.RemoveEmptyEntries));

        return new Reading(text, candidates, stood, printedMarks, characters, most);
    }

    private string Report(string what, Reading r)
    {
        var line = what + ": " + r.Candidates + " candidates at the pitch, " + r.Stood + " stood, " + r.Printed + " printed; reads `" + r.Text + "`";

        _output.WriteLine(line);

        return r.Text;
    }

    [Theory]
    [InlineData(8.0, true)]
    [InlineData(8.0, false)]
    [InlineData(16.0, true)]
    [InlineData(16.0, false)]
    public void DiagWhatHappensToEachMark(double db, bool gates)
    {
        var samples = Standard(Call, 20, db, 5070 + (int)db);
        var detector = new CwEnvelopeDetector(Rate) { MarksNeedEdges = gates, MarksNeedNarrowness = gates, MarksNeedShape = gates };

        for (var at = 0; at + Chunk <= samples.Length; at += Chunk)
        {
            detector.Process(samples.AsSpan(at, Chunk));
        }

        var marks = detector.MarksSince(0).Marks.Where(m => Math.Abs(m.PitchHz - Pitch) <= CwRunReader.PitchToleranceHz).OrderBy(m => m.FromSeconds).ToList();
        var pattern = MorseCode.KeyPattern(Call);
        var dit = MorseCode.Dit(20).TotalSeconds;
        var t = 3.0;
        var truth = new List<(double From, double To)>();

        for (var i = 0; i < pattern.Count; i++)
        {
            if (i % 2 == 0)
            {
                truth.Add((t, t + (pattern[i] * dit)));
            }

            t += pattern[i] * dit;
        }

        var used = new HashSet<CwMark>();
        var lost = 0;
        var split = 0;

        foreach (var (from, to) in truth)
        {
            var over = marks.Where(m => m.FromSeconds < to && m.ToSeconds > from).ToList();

            used.UnionWith(over);

            if (over.Count == 0)
            {
                lost++;
                _output.WriteLine("LOST  " + from.ToString("0.000") + " " + ((to - from) * 1000).ToString("0") + " ms");
            }
            else if (over.Count > 1)
            {
                split++;
                _output.WriteLine("SPLIT " + from.ToString("0.000") + " " + ((to - from) * 1000).ToString("0") + " ms into " + string.Join(", ", over.Select(m => m.LengthMs.ToString("0") + " ms " + m.Shape)));
            }
            else
            {
                var m = over[0];
                var off = m.LengthMs - ((to - from) * 1000);

                if (Math.Abs(off) > 25)
                {
                    _output.WriteLine("LONG/SHORT " + from.ToString("0.000") + " true " + ((to - from) * 1000).ToString("0") + " read " + m.LengthMs.ToString("0") + " " + m.Shape + " keyed " + m.Keyed);
                }
            }
        }

        var extra = marks.Where(m => !used.Contains(m)).ToList();

        foreach (var m in extra)
        {
            _output.WriteLine("EXTRA " + m.FromSeconds.ToString("0.000") + " " + m.LengthMs.ToString("0") + " ms " + m.Shape);
        }

        _output.WriteLine(db + " dB gates " + gates + ": " + truth.Count + " true, " + marks.Count + " at pitch, lost " + lost + ", split " + split + ", extra " + extra.Count
            + "; contrast median " + marks.Select(m => m.ContrastDb).Where(c => !double.IsNaN(c)).OrderBy(c => c).ElementAtOrDefault(marks.Count / 2).ToString("0.0"));
    }

    /// <remarks>
    /// Case 1, the strength table: the call at 20 WPM, 65 marks sent, at 8, 12, 16 and 24 dB over the
    /// noise. It reads whole at every one.
    /// </remarks>
    [Theory]
    [InlineData(8.0)]
    [InlineData(12.0)]
    [InlineData(16.0)]
    [InlineData(24.0)]
    public void TheCallReadsAtEveryStrength(double db)
        => Assert.Equal(Call, Report("20 WPM at " + db + " dB", Read(Standard(Call, 20, db, 5070 + (int)db))));

    /// <remarks>Case 2: the call at 5 WPM Farnsworth and at 35 WPM, both at 10 dB over the noise, reads whole.</remarks>
    [Fact]
    public void FarnsworthAndFastReadAtTenDecibels()
    {
        var slow = Report("5 WPM Farnsworth at 10 dB", Read(Farnsworth(Call, 5, 10, 5080)));
        var fast = Report("35 WPM at 10 dB", Read(Standard(Call, 35, 10, 5090)));

        Assert.Equal(Call, slow);
        Assert.Equal(Call, fast);
    }

    /// <remarks>
    /// Case 3: thirty seconds and three minutes of loud noise. How many candidates the single-mark gates
    /// pass, how many stand after the pattern, and that nothing prints.
    /// </remarks>
    [Theory]
    [InlineData(30)]
    [InlineData(180)]
    public void NoiseMakesNoPattern(int seconds)
    {
        var noise = CwSignal.Generate(new CwSignalRequest(
            " ", SampleRate: Rate, Amplitude: 0, NoiseAmplitude: 0.3, LeadInSeconds: seconds / 2.0, TailSeconds: seconds / 2.0, Seed: 5100 + seconds)).Samples;
        var detector = new CwEnvelopeDetector(Rate);
        var reader = new CwRunReader();
        var printed = new List<CwCharacter>();
        var sequence = 0L;
        var candidates = 0;
        var stood = 0;

        reader.CharacterRead += printed.Add;

        for (var at = 0; at + Chunk <= noise.Length; at += Chunk)
        {
            detector.Process(noise.AsSpan(at, Chunk));

            var batch = detector.MarksSince(sequence);

            sequence = batch.Marks.Count > 0 ? batch.Marks.Max(m => m.Sequence) : sequence;
            candidates += batch.Marks.Count;
            reader.Read(batch);
        }

        reader.Flush();

        // Every mark kept still says whether it stood; the ones dropped from the minute kept are counted as they came.
        stood = detector.StoodCount;

        _output.WriteLine(seconds + " s of loud noise: " + candidates + " candidates passed the single-mark gates, " + stood + " stood in a pattern; printed `" + string.Concat(printed.Select(c => c.Text)) + "`");

        Assert.DoesNotContain(printed, c => c.Text.Trim().Length > 0);
    }

    /// <remarks>
    /// Case 4: two stations 200 Hz apart, the call at 24 dB at 625 Hz and TEST DE W1AW K at 10 dB at 825
    /// Hz. The loud one prints; whether the quiet one is found as a second sequence is reported.
    /// </remarks>
    [Fact]
    public void TheLoudOfTwoStationsPrintsAndTheQuietIsFound()
    {
        var loud = Standard(Call, 20, 24, 5110);
        var quiet = Standard("TEST DE W1AW K TEST DE W1AW K", 20, 10, 5111, pitch: Pitch + 200, noise: 0);
        var mixed = new float[Math.Max(loud.Length, quiet.Length)];

        for (var i = 0; i < mixed.Length; i++)
        {
            mixed[i] = (i < loud.Length ? loud[i] : 0) + (i < quiet.Length ? quiet[i] : 0);
        }

        var atLoud = Read(mixed, Pitch);
        var atQuiet = Read(mixed, Pitch + 200);

        Report("two stations, the loud one at 625 Hz", atLoud);
        Report("two stations, the quiet one at 825 Hz", atQuiet);

        Assert.Equal(Call, atLoud.Text);
    }
}
