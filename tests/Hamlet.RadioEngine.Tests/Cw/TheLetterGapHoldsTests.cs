using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **One odd gap does not move a sender's letter gap** (work instruction 504). The owner's screen,
/// 2026-09-30, a hand-sent QSO on 40 m: twenty letters of clean English, then every mark its own
/// letter.
/// </summary>
/// <remarks>
/// **SYNTHETIC AUDIO WRITTEN HERE, NOTHING READ FROM DISK** (R96). A 20 WPM sender, 625 Hz, about
/// 22 dB over the noise, built in pieces so one gap can be stretched and one dit dropped.
/// </remarks>
public sealed class TheLetterGapHoldsTests
{
    private const int Rate = 8000;
    private const int Chunk = 80;
    private const int Wpm = 20;
    private const double Dit = 1.2 / Wpm;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where each case prints what it read.</param>
    public TheLetterGapHoldsTests(ITestOutputHelper output) => _output = output;

    /// <summary>The pieces sent one after another, each followed by its own tail in seconds.</summary>
    private static float[] Send(int seed, params (string Text, double Tail)[] pieces)
    {
        var parts = new List<float[]>();

        for (var i = 0; i < pieces.Length; i++)
        {
            parts.Add(CwSignal.Generate(new CwSignalRequest(
                pieces[i].Text, WordsPerMinute: Wpm, ToneHz: 625, SampleRate: Rate, Amplitude: 0.5,
                NoiseAmplitude: 0.04, LeadInSeconds: i == 0 ? 3 : 0, TailSeconds: pieces[i].Tail,
                Seed: seed + i)).Samples);
        }

        var samples = new float[parts.Sum(p => p.Length)];
        var at = 0;

        foreach (var part in parts)
        {
            part.CopyTo(samples, at);
            at += part.Length;
        }

        return samples;
    }

    private static string Read(float[] samples)
    {
        var detector = new CwEnvelopeDetector(Rate);
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

        return string.Join(' ', string.Concat(characters.Select(c => c.Text)).Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    private static readonly Dictionary<char, string> Morse = new()
    {
        ['A'] = ".-", ['B'] = "-...", ['C'] = "-.-.", ['D'] = "-..", ['E'] = ".", ['F'] = "..-.",
        ['G'] = "--.", ['H'] = "....", ['I'] = "..", ['J'] = ".---", ['K'] = "-.-", ['L'] = ".-..",
        ['M'] = "--", ['N'] = "-.", ['O'] = "---", ['P'] = ".--.", ['Q'] = "--.-", ['R'] = ".-.",
        ['S'] = "...", ['T'] = "-", ['U'] = "..-", ['V'] = "...-", ['W'] = ".--", ['X'] = "-..-",
        ['Y'] = "-.--", ['Z'] = "--..",
    };

    /// <summary>
    /// A hand on a straight key: every mark and every gap its Morse length times one plus a scatter
    /// drawn evenly from minus to plus <paramref name="scatter"/>, five millisecond raised-cosine
    /// edges, and Gaussian noise; <paramref name="stretch"/> lengthens the gap after one word.
    /// </summary>
    private static float[] Hand(string text, double scatter, int seed, int stretchAfterWord = -1, double stretch = 1, double letterUnits = 3, double wordUnits = 7, int hesitateAtLetter = -1, double hesitateUnits = 1)
    {
        var random = new Random(seed);
        var samples = new List<float>();
        var phase = 0.0;

        double Jitter(double units) => units * Dit * (1 + (scatter * ((2 * random.NextDouble()) - 1)));

        void Tone(double seconds, bool on)
        {
            var n = (int)(seconds * Rate);
            var edge = (int)(0.005 * Rate);

            for (var i = 0; i < n; i++)
            {
                var shape = !on ? 0 : i < edge ? 0.5 - (0.5 * Math.Cos(Math.PI * i / edge)) : i >= n - edge ? 0.5 - (0.5 * Math.Cos(Math.PI * (n - i) / edge)) : 1;
                var u1 = 1 - random.NextDouble();
                var u2 = random.NextDouble();
                var noise = 0.04 * Math.Sqrt(-2 * Math.Log(u1)) * Math.Cos(2 * Math.PI * u2);

                samples.Add((float)((0.5 * shape * Math.Sin(phase)) + noise));
                phase += 2 * Math.PI * 625 / Rate;
            }
        }

        Tone(3, false);

        var words = text.Split(' ');
        var letter = -1;

        for (var w = 0; w < words.Length; w++)
        {
            for (var c = 0; c < words[w].Length; c++)
            {
                var code = Morse[words[w][c]];
                letter++;

                for (var e = 0; e < code.Length; e++)
                {
                    Tone(Jitter(code[e] == '.' ? 1 : 3), true);

                    if (e < code.Length - 1)
                    {
                        Tone(Jitter(letter == hesitateAtLetter && e == 0 ? hesitateUnits : 1), false);
                    }
                }

                if (c < words[w].Length - 1)
                {
                    Tone(Jitter(letterUnits), false);
                }
            }

            Tone(w == stretchAfterWord ? stretch * wordUnits * Dit : Jitter(wordUnits), false);
        }

        Tone(3, false);

        return samples.ToArray();
    }

    /// <remarks>
    /// A hand-sent QSO at 20 WPM, every mark and gap scattered by a fifth either way, with and without
    /// the gap after INTO at a word gap and a half: it reads whole.
    /// </remarks>
    [Theory]
    [InlineData(0.2, -1)]
    [InlineData(0.2, 1)]
    public void AHandSentQsoReadsWhole(double scatter, int stretchAfter)
    {
        var text = Read(Hand(Qso, scatter, 5043, stretchAfter, 1.5));

        _output.WriteLine($"hand-sent, scatter {scatter:0.0}, stretched after word {stretchAfter}: read `{text}`");

        Assert.Equal(Qso, text);
    }

    private const string Qso = "NICELY INTO MK TOYOTA PRIUS BOTH WITH AND WITHOUT THE HYBRID ENGINE RUNNING";

    /// <remarks>
    /// <para>**THE OWNER'S SCREEN, BUILT.** A hand sender at 20 WPM who leaves four dits between
    /// letters and nine between words, and hesitates once inside the Y of NICELY, two and a half
    /// dits where the dit belongs. The hesitation splits the Y into two letters, and the gap
    /// between them, about 150 ms against letter gaps of about 250, is the shortest gap between runs
    /// the sender has shown and far enough below the rest to be a cluster of its own.</para>
    /// <para>Read as the letter gap, it put the word boundary at 1.53 times 150 ms, under every real
    /// letter gap, and every letter after it printed as a word of its own - for forty gaps, until
    /// the odd one left the sender's memory. The Y still reads as two letters; everything after it
    /// reads whole.</para>
    /// </remarks>
    [Fact]
    public void OneHesitationInsideALetterDoesNotSpaceOutTheRest()
    {
        var text = Read(Hand(Qso, 0, 5044, letterUnits: 4, wordUnits: 9, hesitateAtLetter: 5, hesitateUnits: 2.5));

        _output.WriteLine($"hand sender, letter gap four dits, word gap nine, a hesitation of two and a half dits inside the Y: read `{text}`");

        Assert.EndsWith(Qso[Qso.IndexOf(" INTO", StringComparison.Ordinal)..], text);
    }

    /// <remarks>
    /// The owner's sentence at 20 WPM with the gap after INTO stretched to a word gap and a half,
    /// then the sending going on: it reads whole.
    /// </remarks>
    [Fact]
    public void OneLongGapDoesNotSplitTheLettersAfterIt()
    {
        var text = Read(Send(5041, ("NICELY INTO", 1.5 * 7 * Dit), ("MK TOYOTA PRIUS BOTH WITH", 3)));

        _output.WriteLine($"one gap of a word gap and a half after INTO: read `{text}`");

        Assert.Equal("NICELY INTO MK TOYOTA PRIUS BOTH WITH", text);
    }

    /// <remarks>
    /// The same with the third element of the L in NICELY dropped, so the L's dit-gap-dit becomes a
    /// gap three dits long: it is sent as A then E, and the rest reads whole.
    /// </remarks>
    [Fact]
    public void ADroppedDitDoesNotSplitTheLettersAfterIt()
    {
        var text = Read(Send(5042, ("NICEAEY INTO MK TOYOTA PRIUS BOTH WITH", 3)));

        _output.WriteLine($"a dit dropped from the L: read `{text}`");

        Assert.Equal("NICEAEY INTO MK TOYOTA PRIUS BOTH WITH", text);
    }
}
