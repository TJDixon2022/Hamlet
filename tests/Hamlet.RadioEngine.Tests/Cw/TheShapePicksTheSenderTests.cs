using System.Globalization;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **SHAPE PICKS THE SENDER; LOUDNESS PICKS NOTHING** (work instruction 519, R116, HM-DEC-223).
/// </summary>
/// <remarks>
/// <para>**THE OWNER, R116**: *"I don't care what the pitch is. You should find the shape in the noise.
/// It's there. It was audible. Let's defocus pitch and emphasize shape."*</para>
/// <para>**SYNTHETIC AUDIO WRITTEN HERE, NOTHING READ FROM DISK** (R96): keyed tones built from a
/// timeline in this file, with the band's noise added once, on unit 502's scale.</para>
/// </remarks>
public sealed class TheShapePicksTheSenderTests
{
    private const int Rate = 8000;
    private const int Chunk = 80;
    private const string Call = "CQ CQ DE N0CALL N0CALL K";

    private static readonly Dictionary<char, string> Code = new()
    {
        ['A'] = ".-", ['B'] = "-...", ['C'] = "-.-.", ['D'] = "-..", ['E'] = ".", ['F'] = "..-.", ['G'] = "--.",
        ['H'] = "....", ['I'] = "..", ['J'] = ".---", ['K'] = "-.-", ['L'] = ".-..", ['M'] = "--", ['N'] = "-.",
        ['O'] = "---", ['P'] = ".--.", ['Q'] = "--.-", ['R'] = ".-.", ['S'] = "...", ['T'] = "-", ['U'] = "..-",
        ['V'] = "...-", ['W'] = ".--", ['X'] = "-..-", ['Y'] = "-.--", ['Z'] = "--..",
        ['0'] = "-----", ['1'] = ".----", ['2'] = "..---", ['3'] = "...--", ['4'] = "....-",
        ['5'] = ".....", ['6'] = "-....", ['7'] = "--...", ['8'] = "---..", ['9'] = "----.",
    };

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the scores and readings are printed.</param>
    public TheShapePicksTheSenderTests(ITestOutputHelper output) => _output = output;

    /// <summary>A Morse timeline from a start time: key-down and key-up spans, each scattered by up to the share either way.</summary>
    private static List<(double From, double To)> Morse(string text, int wpm, double scatter, double start, int seed)
    {
        var random = new Random(seed);
        var dit = 1.2 / wpm;
        var marks = new List<(double, double)>();
        var at = start;

        double Draw(double ideal) => ideal * (1 + (scatter * ((2 * random.NextDouble()) - 1)));

        var words = text.Split(' ');

        for (var w = 0; w < words.Length; w++)
        {
            for (var c = 0; c < words[w].Length; c++)
            {
                var code = Code[words[w][c]];

                for (var e = 0; e < code.Length; e++)
                {
                    var length = Draw(code[e] == '-' ? 3 * dit : dit);

                    marks.Add((at, at + length));
                    at += length + (e + 1 < code.Length ? Draw(dit) : 0);
                }

                at += c + 1 < words[w].Length ? Draw(3 * dit) : 0;
            }

            at += w + 1 < words.Length ? Draw(7 * dit) : 0;
        }

        return marks;
    }

    /// <summary>A carrier keyed at random: marks of 30 to 300 ms at gaps of 30 to 400 ms, no rhythm.</summary>
    private static List<(double From, double To)> RandomKeying(double start, double end, int seed)
    {
        var random = new Random(seed);
        var marks = new List<(double, double)>();
        var at = start;

        while (at < end)
        {
            var length = 0.03 + (0.27 * random.NextDouble());

            marks.Add((at, at + length));
            at += length + 0.03 + (0.37 * random.NextDouble());
        }

        return marks;
    }

    /// <summary>Add a keyed tone to some audio, with four millisecond raised-cosine edges.</summary>
    private static void Key(float[] samples, IEnumerable<(double From, double To)> marks, double pitch, double db)
    {
        var amplitude = ThePatternIsTheGateTests.Over(db);
        var edge = 0.004 * Rate;

        foreach (var (from, to) in marks)
        {
            var a = (int)Math.Round(from * Rate);
            var b = Math.Min(samples.Length, (int)Math.Round(to * Rate));

            for (var i = a; i < b; i++)
            {
                var shape = i - a < edge ? 0.5 - (0.5 * Math.Cos(Math.PI * (i - a) / edge))
                    : b - i < edge ? 0.5 - (0.5 * Math.Cos(Math.PI * (b - i) / edge))
                    : 1.0;

                samples[i] += (float)(amplitude * shape * Math.Sin(2 * Math.PI * pitch * i / Rate));
            }
        }
    }

    /// <summary>The band's noise, unit 502's level, for some seconds.</summary>
    private static float[] Noise(double seconds, int seed)
    {
        var noise = CwSignal.Generate(new CwSignalRequest(
            " ", SampleRate: Rate, Amplitude: 0, NoiseAmplitude: 0.04, LeadInSeconds: seconds / 2, TailSeconds: seconds / 2, Seed: seed)).Samples;

        return noise.Take((int)(seconds * Rate)).ToArray();
    }

    /// <summary>What a read gives: the text, each character with its pitch, the reader's senders at the end, and the detector's figures.</summary>
    internal sealed record Run(
        string Text,
        List<(double Seconds, double PitchHz, string Text)> Letters,
        IReadOnlyList<(double PitchHz, CwSequenceShape Shape, bool Printed, int Marks)> Senders,
        double HighestStandingShape,
        CwSequenceShape? HighestStandingShapeOf = null);

    private static Run Read(float[] samples, bool shape, Action<CwEnvelopeDetector>? setUp = null)
    {
        var detector = new CwEnvelopeDetector(Rate) { ShapePicks = shape };

        setUp?.Invoke(detector);

        var reader = new CwRunReader { ShapePicks = shape };
        var characters = new List<CwCharacter>();
        var letters = new List<(double, double, string)>();
        var sequence = 0L;
        IReadOnlyList<(double, CwSequenceShape, bool, int)> senders = Array.Empty<(double, CwSequenceShape, bool, int)>();

        reader.CharacterRead += characters.Add;
        reader.RunRead += (c, run) => letters.Add((run[^1].ToSeconds, run.Average(m => m.PitchHz), c.Text));

        for (var at = 0; at + Chunk <= samples.Length; at += Chunk)
        {
            detector.Process(samples.AsSpan(at, Chunk));

            var batch = detector.MarksSince(sequence);

            sequence = batch.Marks.Count > 0 ? batch.Marks.Max(m => m.Sequence) : sequence;
            reader.Read(batch);

            // The senders as they stood while sending, before any is forgotten.
            var now = reader.SenderShapes;

            if (now.Count > 0)
            {
                senders = now;
            }
        }

        reader.Flush();

        var text = string.Join(' ', string.Concat(characters.Select(c => c.Text)).Split(' ', StringSplitOptions.RemoveEmptyEntries));

        return new Run(text, letters, senders, detector.HighestStandingShape, detector.HighestStandingShapeOf);
    }

    private void Report(string what, Run run)
    {
        _output.WriteLine($"{what}: reads `{run.Text}`; highest standing sequence {run.HighestStandingShapeOf?.ToString() ?? "none"}");

        foreach (var group in run.Letters.GroupBy(l => Math.Round(l.PitchHz / 25) * 25))
        {
            _output.WriteLine($"  letters at {group.Key:0} Hz: `{string.Concat(group.Select(l => l.Text))}`");
        }

        foreach (var (pitch, shape, printed, marks) in run.Senders.Where(s => s.Marks >= CwPatternGate.MarksToStand).OrderByDescending(s => s.Shape.Score))
        {
            _output.WriteLine(string.Create(CultureInfo.InvariantCulture, $"  sender at {pitch:0} Hz, {marks} marks{(printed ? ", printed" : string.Empty)}: {shape}"));
        }
    }

    /// <remarks>
    /// Case 1: a clean 20 WPM machine sender at 12 dB at 625 Hz beside a 24 dB carrier keyed at random, 200
    /// and 400 Hz away. At 400 Hz the clean sender prints, whole. At 200 Hz it is printed for the report: the
    /// carrier's keying costs the clean sender most of its marks at the per-mark gates, before and after.
    /// </remarks>
    /// <param name="carrierHz">The carrier's pitch.</param>
    [Theory]
    [InlineData(825.0)]
    [InlineData(1025.0)]
    public void ACleanSenderBeatsALoudRandomCarrier(double carrierHz)
    {
        var clean = Morse(Call, 20, 0, 3, 5191);
        var end = clean[^1].To + 3;
        var samples = Noise(end, 5192);

        Key(samples, clean, 625, 12);
        Key(samples, RandomKeying(2.5, end - 2, 5193), carrierHz, 24);

        Report("  before, loudness and marks pick", Read(samples, shape: false));

        var run = Read(samples, shape: true);

        Report($"case 1, clean 12 dB at 625 beside a random 24 dB carrier at {carrierHz:0}", run);

        if (carrierHz > 900)
        {
            Assert.Equal(Call, run.Text);
        }
    }

    /// <remarks>
    /// Case 2: a clean 20 WPM sender at 10 dB at the passband's edge, 825 Hz through 500 Hz on 600, beside a
    /// 20 dB fist scattered by a third at the centre. The clean one prints, whole (work instruction 520): the first
    /// pick waits one word gap, and its shape ranks above the fist's.
    /// </remarks>
    [Fact]
    public void ACleanSenderAtTheEdgeOutranksALouderFistAtTheCentre()
    {
        var clean = Morse(Call, 20, 0, 3, 5194);
        var fist = Morse("TEST DE W1AW TEST DE W1AW K", 20, 1.0 / 3, 3, 5195);
        var end = Math.Max(clean[^1].To, fist[^1].To) + 3;
        var samples = Noise(end, 5196);

        Key(samples, clean, 825, 10);
        var alone = Noise(end, 5196);

        Key(alone, clean, 825, 10);
        Report("  the clean sender alone through the filter", Read(NarrownessReadsTheFiltersBandTests.ThroughTheFilter(alone), shape: true, d => d.SetPassband(600, 500)));

        Key(samples, fist, 600, 20);

        var filtered = NarrownessReadsTheFiltersBandTests.ThroughTheFilter(samples);

        Report("  before, loudness and marks pick", Read(filtered, shape: false, d => d.SetPassband(600, 500)));

        var run = Read(filtered, shape: true, d => d.SetPassband(600, 500));

        Report("case 2, clean 10 dB at 825 through the filter beside a 20 dB fist at 600", run);

        var fistLetters = run.Letters.Where(l => Math.Abs(l.PitchHz - 600) <= 50).ToList();
        var cleanLetters = run.Letters.Where(l => Math.Abs(l.PitchHz - 825) <= 50).ToList();

        // **MET BY UNIT 520** (task 2): the first pick waits one word gap, and the clean sender's shape is the higher.
        var shapes = run.Senders.Where(s => s.Marks >= CwPatternGate.MarksToStand).ToList();
        var cleanShape = shapes.Where(s => Math.Abs(s.PitchHz - 825) <= 50).Select(s => s.Shape.Score).DefaultIfEmpty(0).Max();
        var fistShape = shapes.Where(s => Math.Abs(s.PitchHz - 600) <= 50).Select(s => s.Shape.Score).DefaultIfEmpty(0).Max();

        _output.WriteLine($"  printed at 600: `{string.Concat(fistLetters.Select(l => l.Text))}`, at 825: `{string.Concat(cleanLetters.Select(l => l.Text))}`");

        Assert.True(cleanShape > fistShape, $"the clean sender's shape {cleanShape:0.000} is above the fist's {fistShape:0.000}");
        Assert.Equal(Call, run.Text);
    }

    /// <remarks>
    /// Case 3: the two-station case, 24 dB at 625 and 10 dB at 825, both clean. The loud one prints; both
    /// scores are printed.
    /// </remarks>
    [Fact]
    public void TwoCleanStationsBothScore()
    {
        var loud = Morse(Call, 20, 0, 3, 5197);
        var quiet = Morse("TEST DE W1AW K TEST DE W1AW K", 20, 0, 3, 5198);
        var end = Math.Max(loud[^1].To, quiet[^1].To) + 3;
        var samples = Noise(end, 5199);

        Key(samples, loud, 625, 24);
        Key(samples, quiet, 825, 10);

        Report("  before, loudness and marks pick", Read(samples, shape: false));

        var run = Read(samples, shape: true);

        Report("case 3, clean 24 dB at 625 and clean 10 dB at 825", run);

        Assert.Equal(Call, run.Text);
    }

    /// <remarks>
    /// Case 4: a fist at 24 dB at 625 is being printed when a clean sender at 825 begins mid-call. The fist
    /// is printed whole, nothing of the clean one before the fist falls silent, and then the clean one.
    /// The switch time is the clean one's first letter less the fist's last.
    /// </remarks>
    [Fact]
    public void ABetterShapeTakesTheTerminalAtTheNextSilence()
    {
        var fist = Morse(Call, 20, 0.2, 3, 5200);
        var clean = Morse("TEST DE W1AW K TEST DE W1AW K TEST DE W1AW K", 20, 0, 8, 5201);
        var end = Math.Max(fist[^1].To, clean[^1].To) + 3;
        var samples = Noise(end, 5202);

        Key(samples, fist, 625, 24);
        Key(samples, clean, 825, 20);

        Report("  before, loudness and marks pick", Read(samples, shape: false));

        var run = Read(samples, shape: true);

        Report("case 4, a fist at 625 printing when a clean sender at 825 begins at 8 s", run);

        var fistLetters = run.Letters.Where(l => Math.Abs(l.PitchHz - 625) <= 50).ToList();
        var cleanLetters = run.Letters.Where(l => Math.Abs(l.PitchHz - 825) <= 50).ToList();

        _output.WriteLine(string.Create(
            CultureInfo.InvariantCulture,
            $"  the fist's last mark ends at {fist[^1].To:0.00} s, its last letter printed at {fistLetters.Select(l => l.Seconds).DefaultIfEmpty(double.NaN).Max():0.00} s; the clean one's first letter printed ends at {cleanLetters.Select(l => l.Seconds).DefaultIfEmpty(double.NaN).Min():0.00} s"));

        // Never mid-word: what the fist printed is whole words of its call, in order, and the clean sender comes
        // only after the fist's last letter.
        var fistText = string.Concat(run.Letters.Where(l => Math.Abs(l.PitchHz - 625) <= 50).Select(l => l.Text));

        var words = Call.Split(' ');

        Assert.True(Enumerable.Range(1, words.Length).Any(k => string.Concat(words.Take(k)) == fistText), $"the fist printed `{fistText}`, not whole words of its call");
        Assert.True(cleanLetters.Count > 0, "the clean sender is printed after the fist falls silent");
        Assert.True(cleanLetters.Min(l => l.Seconds) > fistLetters.Max(l => l.Seconds), "the clean sender is printed only after the fist's last letter");
    }


    /// <remarks>
    /// **A FIST IS A SENDER** (work instruction 520, task 1): the call at 20 WPM, 24 dB, clean and scattered
    /// by a fifth and by a third, alone. Its shape from the last forty marks that stood at its pitch, on unit
    /// 519's machine scale and against a hand; both fists above noise's best (0.107 in unit 519) and the clean
    /// sender above both.
    /// </remarks>
    /// <param name="scatter">How far each length is scattered either way.</param>
    [Theory]
    [InlineData(0.0)]
    [InlineData(0.2)]
    [InlineData(1.0 / 3)]
    public void AFistScoresAboveNoise(double scatter)
    {
        var marks = Morse(Call, 20, scatter, 3, 5203);
        var samples = Noise(marks[^1].To + 3, 5204);

        Key(samples, marks, 625, 24);

        var detector = new CwEnvelopeDetector(Rate);

        for (var at = 0; at + Chunk <= samples.Length; at += Chunk)
        {
            detector.Process(samples.AsSpan(at, Chunk));
        }

        var stood = detector.MarksSince(0).Marks.Where(m => Math.Abs(m.PitchHz - 625) <= 2 * CwEnvelopeDetector.BinSpacingHz).ToList();
        var recent = stood.TakeLast(40).ToList();
        var before = CwSequenceShape.Of(recent, stood.Count, againstAHand: false);
        var after = CwSequenceShape.Of(recent, stood.Count);
        var run = Read(samples, shape: true);

        _output.WriteLine(string.Create(CultureInfo.InvariantCulture, $"scattered by {scatter:0.00}: {stood.Count} stood, reads `{run.Text}`"));
        _output.WriteLine($"  before, a machine's scale: {before}");
        _output.WriteLine($"  after, against a hand:     {after}");

        Assert.True(after.Score > 0.2, $"shape {after.Score:0.000} is clear of noise's best");
    }
    /// <remarks>
    /// Case 5: thirty seconds and three minutes of loud noise print nothing; the highest shape score any noise
    /// sequence earned is printed beside the real senders' of cases 1 to 3.
    /// </remarks>
    /// <param name="seconds">How long.</param>
    [Theory]
    [InlineData(30)]
    [InlineData(180)]
    public void NoiseShapesPrintNothing(int seconds)
    {
        var noise = CwSignal.Generate(new CwSignalRequest(
            " ", SampleRate: Rate, Amplitude: 0, NoiseAmplitude: 0.3, LeadInSeconds: seconds / 2.0, TailSeconds: seconds / 2.0, Seed: 5190 + seconds)).Samples;
        Report("  before, loudness and marks pick", Read(noise, shape: false));

        var run = Read(noise, shape: true);

        Report($"case 5, {seconds} s of loud noise", run);

        Assert.Equal(string.Empty, run.Text);
    }
}
