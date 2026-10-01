using Hamlet.App.Controls;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// **A block on the scroll stays on the scroll** (work instruction 511, task 1, HM-DEC-215).
/// </summary>
/// <remarks>
/// <para>**TIM, 2026-09-30**: *"The letters are solid, but the bars, the dashes and dots bars, tend
/// to come and go."*</para>
/// <para>**SYNTHETIC AUDIO WRITTEN HERE, NOTHING READ FROM DISK** (R96), wired as the tab wires it and
/// sampled every fifty milliseconds, twenty a second as the tab's timer ticks. A block is still there
/// on a later frame when a block on that frame covers any of the same time: a block the eye follows
/// as it slides left, whether or not its ends moved.</para>
/// </remarks>
public sealed class TheScrollKeepsItsBlocksTests
{
    private const int Rate = 8000;
    private const int Chunk = 80;
    private const int ChunksPerTick = 5;
    private const string Call = "CQ CQ DE N0CALL N0CALL K";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where each case's blocks are printed.</param>
    public TheScrollKeepsItsBlocksTests(ITestOutputHelper output) => _output = output;

    private static float[] Station(string text, int wpm, int seed)
        => CwSignal.Generate(new CwSignalRequest(
            text, WordsPerMinute: wpm, ToneHz: 625, SampleRate: Rate, Amplitude: 0.5,
            NoiseAmplitude: 0.04, LeadInSeconds: 3, TailSeconds: 3, Seed: seed)).Samples;

    /// <summary>18 WPM letters with the spaces stretched by PARIS to the overall speed, as unit 507 built it.</summary>
    private static float[] Farnsworth(string text, double overallWpm, int seed)
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
                    words[w][c].ToString(), WordsPerMinute: letterWpm, ToneHz: 625, SampleRate: Rate,
                    Amplitude: 0.5, NoiseAmplitude: 0.04,
                    LeadInSeconds: pieces.Count == 0 ? 3 : 0, TailSeconds: tail,
                    Seed: seed + pieces.Count)).Samples);
            }
        }

        return pieces.SelectMany(p => p).ToArray();
    }

    private static float[] NoiseAlone() => CwSignal.Generate(new CwSignalRequest(
        " ", SampleRate: Rate, Amplitude: 0, NoiseAmplitude: 0.3, LeadInSeconds: 15, TailSeconds: 15, Seed: 491)).Samples;

    private sealed record Run(IReadOnlyList<(DateTime NowUtc, IReadOnlyList<CwGraphBar> Bars)> Frames, string Terminal);

    /// <summary>The tab's decoder, detector and scope timer on the audio's clock, every frame kept.</summary>
    private static Run Listen(float[] samples)
    {
        var detector = new CwEnvelopeDetector(Rate);
        var decoder = new CwDecoder(Rate, 600) { DetectorMarks = detector.MarksSince };
        var feed = new CwScopeFeed();
        var start = new DateTime(2026, 9, 30, 21, 0, 0, DateTimeKind.Utc);
        var printed = new List<CwCharacter>();
        var frames = new List<(DateTime, IReadOnlyList<CwGraphBar>)>();
        var frame = CwScopeFrame.Empty;

        DateTime Now() => start + decoder.Heard;

        decoder.CharacterSettled += c =>
        {
            printed.Add(c);
            feed.Settle(c, decoder.Heard, Now());
        };

        void Tick()
        {
            frame = feed.Tick(detector, detector.Reading, decoder.PrintingHz, frame, scopeQuiet: false, Now());
            frames.Add((frame.Training.NowUtc, frame.Training.Bars));
        }

        for (var at = 0; at + Chunk <= samples.Length; at += Chunk)
        {
            decoder.Process(new AudioChunk(at, Rate, samples.AsSpan(at, Chunk)));
            detector.Process(samples.AsSpan(at, Chunk));

            if (at / Chunk % ChunksPerTick == 0)
            {
                Tick();
            }
        }

        decoder.Flush();
        Tick();

        return new Run(frames, string.Join(' ', string.Concat(printed.Select(c => c.Text)).Split(' ', StringSplitOptions.RemoveEmptyEntries)));
    }

    /// <summary>Every block that was drawn and then went before time carried it off the left, one line each.</summary>
    private List<string> Gone(string name, Run run)
    {
        var window = TimeSpan.FromSeconds(CwTrainingGraph.WindowSeconds);
        var gone = new List<string>();
        var drawn = 0;

        for (var t = 0; t + 1 < run.Frames.Count; t++)
        {
            foreach (var bar in run.Frames[t].Bars)
            {
                var next = run.Frames[t + 1];

                if (bar.EndUtc < next.NowUtc - window)
                {
                    continue;
                }

                if (!next.Bars.Any(b => b.StartUtc <= bar.EndUtc && b.EndUtc >= bar.StartUtc))
                {
                    gone.Add($"block {bar.StartUtc:ss.fff}-{bar.EndUtc:ss.fff} on frame {t}, gone on frame {t + 1}");
                }
            }
        }

        foreach (var f in run.Frames)
        {
            drawn = Math.Max(drawn, f.Bars.Count);
        }

        _output.WriteLine($"{name}: terminal `{run.Terminal}`; {run.Frames.Count} frames; up to {drawn} blocks on a frame; {gone.Count} went before the edge");

        foreach (var line in gone.Take(12))
        {
            _output.WriteLine("   " + line);
        }

        return gone;
    }

    /// <remarks>Task 1: the call at 20 WPM, every frame; a block once drawn is on every later frame until it scrolls off.</remarks>
    [Fact]
    public void AtTwentyWordsABlockDrawnStaysUntilTheEdge()
    {
        var run = Listen(Station(Call, 20, 5090));

        Assert.Empty(Gone("the call at 20 WPM", run));
        Assert.Contains(run.Frames, f => f.Bars.Count > 0);
    }

    /// <remarks>Task 1: the call at 5 WPM Farnsworth, the same.</remarks>
    [Fact]
    public void AtFiveWordsFarnsworthABlockDrawnStaysUntilTheEdge()
    {
        var run = Listen(Farnsworth(Call, 5, 5080));

        Assert.Empty(Gone("the call at 5 WPM Farnsworth", run));
        Assert.Contains(run.Frames, f => f.Bars.Count > 0);
    }

    /// <remarks>Task 1: loud noise and no station draws no block.</remarks>
    [Fact]
    public void LoudNoiseDrawsNoBlock()
    {
        var run = Listen(NoiseAlone());

        Gone("loud noise", run);

        Assert.All(run.Frames, f => Assert.Empty(f.Bars));
    }
}
