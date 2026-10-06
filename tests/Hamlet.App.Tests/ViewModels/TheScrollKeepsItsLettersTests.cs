using Hamlet.App.Controls;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// **A letter on the scroll stays on the scroll** (work instruction numbered 508, run as unit 509,
/// R100, HM-DEC-213).
/// </summary>
/// <remarks>
/// <para>**TIM, 2026-09-30**: *"I don't like how the scrolling letters seem to blink in and out
/// depending on your confidence."*</para>
/// <para>**SYNTHETIC AUDIO WRITTEN HERE, NOTHING READ FROM DISK** (R96). Wired as the tab wires it:
/// the decoder given the detector's marks, every settled character to the terminal and to the
/// scope's feed, and the scope ticked every fifty milliseconds on the audio's clock, twenty a
/// second as the tab's timer ticks. Every frame is sampled, so a letter that blinks out for one
/// frame is seen.</para>
/// </remarks>
public sealed class TheScrollKeepsItsLettersTests
{
    private const int Rate = 8000;
    private const int Chunk = 80;
    private const int ChunksPerTick = 5;
    private const double Width = 800;
    private const string Call = "CQ CQ DE N0CALL N0CALL K";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where each case's terminal and scroll are printed.</param>
    public TheScrollKeepsItsLettersTests(ITestOutputHelper output) => _output = output;

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

        var samples = new float[pieces.Sum(p => p.Length)];
        var at = 0;

        foreach (var piece in pieces)
        {
            piece.CopyTo(samples, at);
            at += piece.Length;
        }

        return samples;
    }

    private static float[] NoiseAlone() => CwSignal.Generate(new CwSignalRequest(
        " ", SampleRate: Rate, Amplitude: 0, NoiseAmplitude: 0.3, LeadInSeconds: 15, TailSeconds: 15, Seed: 491)).Samples;

    /// <summary>One printed letter: what the terminal showed, and where the graph keeps it.</summary>
    private sealed record Printed(string Text, DateTime EndUtc, int SettledTick);

    /// <summary>One frame the scroll drew: its now, and each letter it drew with whether a block stands under it.</summary>
    private sealed record Drawn(DateTime NowUtc, IReadOnlyList<(string Text, DateTime StartUtc, DateTime EndUtc, bool OverABlock)> Letters);

    private sealed record Run(IReadOnlyList<Printed> Terminal, IReadOnlyList<Drawn> Frames, string Shown);

    /// <summary>The tab's decoder, detector and scope timer on the audio's clock, every frame kept.</summary>
    private static Run Listen(float[] samples)
    {
        var detector = new CwEnvelopeDetector(Rate);
        var decoder = new CwDecoder(Rate) { DetectorMarks = detector.MarksSince };
        var feed = new CwScopeFeed();
        var start = new DateTime(2026, 9, 30, 20, 10, 0, DateTimeKind.Utc);
        var terminal = new List<Printed>();
        var shown = new List<CwCharacter>();
        var frames = new List<Drawn>();
        var frame = CwScopeFrame.Empty;

        DateTime Now() => start + decoder.Heard;

        decoder.CharacterSettled += c =>
        {
            shown.Add(c);

            if (!c.IsWordGap && !string.IsNullOrWhiteSpace(c.Text))
            {
                var heard = decoder.Heard;
                var end = Now() - (heard > c.At ? heard - c.At : TimeSpan.Zero);

                terminal.Add(new Printed(Shown(c.Text, c.Confidence), end, frames.Count));
            }

            feed.Settle(c, decoder.Heard, Now());
        };

        void Tick()
        {
            frame = feed.Tick(detector, detector.Reading, decoder.RunsPrintingHz, frame, scopeQuiet: false, Now());

            var items = CwScopeControl.Items(frame, Width);
            var bars = items.Where(i => i.Kind == CwScopeItemKind.Bar).ToList();
            var letterItems = items.Where(i => i.Kind == CwScopeItemKind.Letter).ToList();
            var letters = CwScopeControl.DrawnLetters(frame);

            frames.Add(new Drawn(
                frame.Training.NowUtc,
                letters.Select((l, k) => (
                    Shown(l.Text, l.Confidence),
                    l.StartUtc,
                    l.EndUtc,
                    bars.Any(b => b.X <= letterItems[k].X2 && b.X2 >= letterItems[k].X))).ToList()));
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

        return new Run(
            terminal,
            frames,
            string.Join(' ', string.Concat(shown.Select(c => c.Text)).Split(' ', StringSplitOptions.RemoveEmptyEntries)));
    }

    private static string Shown(string text, CwConfidence confidence)
        => confidence == CwConfidence.Unreadable ? MorseAlphabet.Unreadable : text;

    /// <summary>What went wrong on the scroll, one line each; empty when it kept faith with the terminal.</summary>
    private List<string> Faults(string name, Run run, bool blocks)
    {
        var faults = new List<string>();
        var window = TimeSpan.FromSeconds(CwTrainingGraph.WindowSeconds);

        foreach (var p in run.Terminal)
        {
            bool On(Drawn f) => f.Letters.Any(l => l.Text == p.Text && l.EndUtc == p.EndUtc);

            // A letter that prints after the blocks it was read from have slid off the left has no
            // place on the scroll left to be drawn at: it is past the edge the moment it prints.
            var printedAt = run.Frames[Math.Min(p.SettledTick, run.Frames.Count - 1)].NowUtc;

            if (p.EndUtc < printedAt - window)
            {
                _output.WriteLine(
                    $"   [{p.Text}] at {p.EndUtc:ss.fff} printed at {printedAt:ss.fff}, "
                    + $"{(printedAt - p.EndUtc).TotalSeconds:0.0} s after it ended: past the left edge when it printed");
                continue;
            }

            var first = -1;

            for (var t = p.SettledTick; t < run.Frames.Count; t++)
            {
                if (On(run.Frames[t]))
                {
                    first = t;
                    break;
                }
            }

            if (first < 0)
            {
                faults.Add($"[{p.Text}] at {p.EndUtc:ss.fff} printed and never drawn");
                continue;
            }

            if (first != p.SettledTick)
            {
                faults.Add($"[{p.Text}] at {p.EndUtc:ss.fff} printed at frame {p.SettledTick}, first drawn at frame {first}");
            }

            if (!run.Frames[first].Letters.First(l => l.Text == p.Text && l.EndUtc == p.EndUtc).OverABlock)
            {
                var shown = run.Frames.Skip(first).Select(f => f.Letters.FirstOrDefault(l => l.Text == p.Text && l.EndUtc == p.EndUtc))
                    .Where(l => l.Text is not null).ToList();
                var bars = run.Frames[first].Letters.First(l => l.Text == p.Text && l.EndUtc == p.EndUtc);

                var bare = $"[{p.Text}] at {p.EndUtc:ss.fff} drawn with no block under it "
                    + $"(span {(bars.EndUtc - bars.StartUtc).TotalMilliseconds:0} ms; a block under it on {shown.Count(l => l.OverABlock)} of {shown.Count} frames it is shown)";

                // Where the case asks for its blocks it is a fault; elsewhere it is the detector's
                // blocks, which this unit leaves as they are, and it is printed for the report.
                if (blocks)
                {
                    faults.Add(bare);
                }
                else
                {
                    _output.WriteLine("   noted: " + bare);
                }
            }

            for (var t = first + 1; t < run.Frames.Count; t++)
            {
                var edge = run.Frames[t].NowUtc - window;

                if (p.EndUtc < edge)
                {
                    break;
                }

                if (!On(run.Frames[t]))
                {
                    faults.Add($"[{p.Text}] at {p.EndUtc:ss.fff} drawn at frame {first}, gone at frame {t} before the edge");
                    break;
                }
            }
        }

        foreach (var f in run.Frames)
        {
            foreach (var l in f.Letters)
            {
                if (!run.Terminal.Any(p => p.Text == l.Text && p.EndUtc == l.EndUtc))
                {
                    faults.Add($"[{l.Text}] at {l.EndUtc:ss.fff} drawn and never printed");
                }
            }
        }

        faults = faults.Distinct().ToList();

        _output.WriteLine($"{name}: terminal `{run.Shown}`; {run.Terminal.Count} letters printed, {run.Frames.Count} frames");

        foreach (var fault in faults)
        {
            _output.WriteLine("   " + fault);
        }

        return faults;
    }

    /// <remarks>
    /// Case 1: the call at 20 WPM. Every letter the terminal printed is drawn on the scroll, in the
    /// terminal's order, over its own blocks, and nothing else is.
    /// </remarks>
    [Fact]
    public void EveryPrintedLetterIsDrawnOverItsBlocks()
    {
        var run = Listen(Station(Call, 20, 5090));
        var faults = Faults("the call at 20 WPM", run, blocks: true);

        Assert.NotEmpty(run.Terminal);
        Assert.DoesNotContain(faults, f => f.Contains("never drawn") || f.Contains("never printed") || f.Contains("no block"));
    }

    /// <remarks>
    /// Case 2: the same call, frame by frame. A letter drawn is on every later frame until it slides
    /// off the left, and it is drawn on the frame after it prints.
    /// </remarks>
    [Fact]
    public void ALetterDrawnStaysUntilTheEdge()
    {
        var run = Listen(Station(Call, 20, 5090));
        var faults = Faults("the call at 20 WPM, frame by frame", run, blocks: false);

        Assert.NotEmpty(run.Terminal);
        Assert.Empty(faults);
    }

    /// <remarks>Case 3: the banked T and E of `TEST DE W1AW K` draw the moment they print, over their blocks, and stay.</remarks>
    [Fact]
    public void TheBankedTAndEDrawWhenTheyPrint()
    {
        var run = Listen(Station("TEST DE W1AW K", 20, 5100));
        var faults = Faults("TEST DE W1AW K", run, blocks: true);

        Assert.Contains(run.Terminal, p => p.Text == "T");
        Assert.Contains(run.Terminal, p => p.Text == "E");
        Assert.Empty(faults);
    }

    /// <remarks>Case 4: the call at 5 WPM Farnsworth. Letters that release late draw late, in place, and stay.</remarks>
    [Fact]
    public void FarnsworthLettersDrawLateInPlace()
    {
        var run = Listen(Farnsworth(Call, 5, 5080));
        var faults = Faults("the call at 5 WPM Farnsworth", run, blocks: false);

        Assert.NotEmpty(run.Terminal);
        Assert.Empty(faults);
    }

    /// <remarks>Case 5: loud noise and no station. No letter in the terminal and none on the scroll.</remarks>
    [Fact]
    public void LoudNoiseDrawsNoLetter()
    {
        var run = Listen(NoiseAlone());
        var faults = Faults("loud noise", run, blocks: false);

        Assert.Empty(run.Terminal);
        Assert.All(run.Frames, f => Assert.Empty(f.Letters));
        Assert.Empty(faults);
    }
}
