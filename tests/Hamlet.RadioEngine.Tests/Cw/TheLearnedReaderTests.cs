using System.Globalization;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **THE PORT AGAINST THE SHIPPED VECTOR** (work instruction 597, HM-DEC-302): the engine's learned reader computes what
/// <c>CWNET_SPEC.md</c> says, on a 12 s synthetic scene that is not a recording. Until the first of these passes nothing downstream is judged.
/// </summary>
public sealed class TheLearnedReaderTests
{
    private static string Vector(string file)
        => Path.Combine(CwFixtures.Folder, "..", "cwnet", file);

    private static float[] VectorAudio()
        => WavAudio.Read(Vector("cwnet-vector.wav")).Samples;

    /// <remarks>The spec's section 2: every cell within 1e-3, and a failure names the first that differs.</remarks>
    [Fact]
    public void FeaturesMatchTheVector()
    {
        var (features, frames, _, _) = CwLearnedReader.Features(VectorAudio());
        var lines = File.ReadAllLines(Vector("cwnet-vector-feat.csv")).Where(l => l.Length > 0).ToList();

        Assert.Equal(lines.Count, frames);

        for (var row = 0; row < frames; row++)
        {
            var cells = lines[row].Split(',');

            for (var column = 0; column < cells.Length; column++)
            {
                var want = double.Parse(cells[column], CultureInfo.InvariantCulture);
                var have = features[(row * cells.Length) + column];

                Assert.True(Math.Abs(want - have) <= 1e-3, string.Create(CultureInfo.InvariantCulture, $"row {row}, column {column}: the engine has {have}, the vector {want}"));
            }
        }
    }

    /// <remarks>The spec's section 3: the argmax per output frame, all 599.</remarks>
    [Fact]
    public void TheNetMatchesTheVector()
    {
        var (features, frames, _, _) = CwLearnedReader.Features(VectorAudio());
        var (logits, outputs) = CwLearnedReader.Logits(features, frames);
        var want = File.ReadAllText(Vector("cwnet-vector-ids.txt")).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
        var have = Enumerable.Range(0, outputs).Select(j => CwLearnedReader.Best(logits, j)).ToArray();

        Assert.Equal(want, have);
    }

    /// <remarks>The spec's section 4, through the chain: the audio fed in 100 ms pieces, as the radio would, not one call on the whole file.</remarks>
    [Fact]
    public void ReadsTheVector()
    {
        var audio = WavAudio.Read(Vector("cwnet-vector.wav"));
        using var on = CwRules.Choose(CwRules.LearnedReader);
        using var chain = new CwChain(audio.SampleRate);
        var text = new System.Text.StringBuilder();

        chain.Decoder.CharacterSettled += c => text.Append(c.Text);
        chain.Detector.SetPassband(600, 500);

        var piece = audio.SampleRate / 10;

        for (var at = 0; at < audio.Samples.Length; at += piece)
        {
            var length = Math.Min(piece, audio.Samples.Length - at);

            chain.Process(new AudioChunk(at, audio.SampleRate, audio.Samples.AsSpan(at, length)));
        }

        chain.Decoder.Flush();

        Assert.NotNull(chain.Detector.Learned);
        Assert.Null(chain.Detector.Sequence);
        Assert.Equal(File.ReadAllText(Vector("cwnet-vector-text.txt")).Trim(), text.ToString());
    }

    /// <summary>What the panel reads after each 100 ms piece of the vector, the learned reader on (work instruction 598, task 3).</summary>
    private static List<(double Seconds, double PitchHz, int? Wpm, CwSpeedProof Proof, bool Lit, CwShapeLight Light)> Panel()
    {
        var audio = WavAudio.Read(Vector("cwnet-vector.wav"));
        using var on = CwRules.Choose(CwRules.LearnedReader);
        using var chain = new CwChain(audio.SampleRate);
        var seen = new List<(double, double, int?, CwSpeedProof, bool, CwShapeLight)>();

        chain.Detector.SetPassband(600, 500);

        var piece = audio.SampleRate / 10;

        for (var at = 0; at + piece <= audio.Samples.Length; at += piece)
        {
            chain.Process(new AudioChunk(at, audio.SampleRate, audio.Samples.AsSpan(at, piece)));
            seen.Add(((at + piece) / (double)audio.SampleRate, chain.Decoder.PanelPitchHz, chain.Decoder.WordsPerMinute, chain.Decoder.SpeedProof, chain.Detector.Learned!.Lit, chain.Detector.Reading.ShapeLight));
        }

        return seen;
    }

    /// <remarks>The panel's pitch: the median of the learned letters' marks, within one bin of the vector's 579 Hz, and blank before three letters.</remarks>
    [Fact]
    public void ThePanelPitchIsTheLettersMedian()
    {
        var panel = Panel();

        Assert.True(double.IsNaN(panel[0].PitchHz));
        Assert.InRange(panel[^1].PitchHz, 579 - 31.25, 579 + 31.25);
    }

    /// <remarks>The panel's speed: a letter rate between 15 and 30 WPM on the vector's 21, said to be a hypothesis, never proved, and blank at the start.</remarks>
    [Fact]
    public void ThePanelSpeedIsALetterRate()
    {
        var panel = Panel();

        Assert.Null(panel[0].Wpm);
        Assert.InRange(panel[^1].Wpm ?? 0, 15, 30);
        Assert.Equal(CwSpeedProof.Hypothesis, panel[^1].Proof);
    }

    /// <remarks>The panel's light: lit while the first ROGER is sent, from the net's uncommitted frames, and dark before 0.2 s; the light the panel shows follows it.</remarks>
    [Fact]
    public void ThePanelLightFollowsTheNet()
    {
        var panel = Panel();

        Assert.All(panel.Where(p => p.Seconds < 0.2), p => Assert.False(p.Lit));
        Assert.Contains(panel, p => p.Seconds >= 0.26 && p.Seconds <= 3.2 && p.Lit);
        // Where the net's light held across two pieces, so no run fell between the detector's last hop and the look.
        Assert.All(panel.Zip(panel.Skip(1)).Where(p => p.First.Lit == p.Second.Lit), p => Assert.Equal(p.Second.Lit ? CwShapeLight.Reading : CwShapeLight.Listening, p.Second.Light));
    }

    /// <remarks>
    /// Off, nothing of this unit runs: the reader is never built, and the sequence reader reads as it did. Work instruction 599, HM-DEC-304: the
    /// switch ships on, so the test turns it off itself, and first asserts that it is on by default.
    /// </remarks>
    [Fact]
    public void TheSwitchOffIsSilent()
    {
        var audio = WavAudio.Read(Vector("cwnet-vector.wav"));

        Assert.DoesNotContain(CwRules.LearnedReader, CwRules.OffByDefault);
        Assert.True(CwRules.On(CwRules.LearnedReader), "the learned reader is not on by default");

        using var off = CwRules.Off(CwRules.LearnedReader);
        using var chain = new CwChain(audio.SampleRate);

        chain.Detector.SetPassband(600, 500);

        for (var at = 0; at + 800 <= audio.Samples.Length; at += 800)
        {
            chain.Process(new AudioChunk(at, audio.SampleRate, audio.Samples.AsSpan(at, 800)));
        }

        chain.Decoder.Flush();

        Assert.Null(chain.Detector.Learned);
        Assert.NotNull(chain.Detector.Sequence);
    }
}
