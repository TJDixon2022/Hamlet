using Hamlet.App.Controls;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// **THE SCOPE DRAWS FROM THE LIVE PATH** (work instruction 480, the letter over the bars, task 1,
/// step 12 criterion 12.2, R94).
/// </summary>
/// <remarks>
/// <para>**TIM, 2026-09-28**: *"I want to see the flat oscilloscope shapes with the letter over
/// top. I need a visual of dit and dash."* His screenshot showed a blank panel and a large
/// <c>T T</c> under it.</para>
/// <para>**THE LIVE PATH, NOT A DRIVEN DETECTOR**: the training radio's own audio source, keyed
/// with a CQ at twenty words a minute in its own noise, handed to a real <see cref="CwDecoder"/>
/// and a real <see cref="CwEnvelopeDetector"/> the way the tab's listen hands them the sound card,
/// and the scope ticked twenty times a second of that audio through <see cref="CwScopeFeed"/>, the
/// scope timer's own work. The clock is the audio's, so the run takes no real time. No recording
/// is read (R88).</para>
/// </remarks>
public sealed class TheScopeDrawsLiveTests
{
    private const double Width = 800;
    private const double TickMs = 50;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the canvas is printed.</param>
    public TheScopeDrawsLiveTests(ITestOutputHelper output) => _output = output;

    /// <remarks>
    /// Proves the tab's picture (work instruction 485, R97): every tick's frame carries the detector's
    /// hops, and the canvas draws blocks for the marks, with every letter over a block, and no line.
    /// </remarks>
    [Fact]
    public void OnTheLiveTabTheBlocksAndTheirLettersDraw()
    {
        var run = Listen(seconds: 12);
        var items = CwScopeControl.Items(run.Keyed, Width);

        Print(run, items);

        Assert.Equal(20, run.FramesWithHopsInTheLastSecond);
        Assert.NotEmpty(run.Last.Hops);
        Assert.Contains(items, i => i.Kind == CwScopeItemKind.Bar);
        Assert.All(
            items.Where(i => i.Kind == CwScopeItemKind.Letter),
            l => Assert.Contains(items, b => b.Kind == CwScopeItemKind.Bar && b.X <= l.X2 && b.X2 >= l.X));
    }

    /// <summary>What one run of the live path left.</summary>
    /// <param name="Last">The last frame the scope was handed.</param>
    /// <param name="Keyed">The last frame handed while the detector said keying; the last frame where none was.</param>
    /// <param name="FramesWithHopsInTheLastSecond">Ticks in the last second of audio whose frame carried hops.</param>
    /// <param name="Settled">Every character the decoder settled, with when on the audio clock.</param>
    /// <param name="Start">The wall time the audio clock's zero stands for.</param>
    internal sealed record Run(
        CwScopeFrame Last,
        CwScopeFrame Keyed,
        int FramesWithHopsInTheLastSecond,
        IReadOnlyList<(CwCharacter Character, TimeSpan Heard)> Settled,
        DateTime Start);

    /// <summary>The tab's listen and its scope timer, on the audio's clock.</summary>
    /// <param name="seconds">How much audio to hear.</param>
    /// <returns>What the scope was handed.</returns>
    internal static Run Listen(double seconds)
    {
        using var source = new TrainingAudioSource("CQ", wordsPerMinute: 20, toneHz: 600, noiseAmplitude: 0.02);
        var decoder = new CwDecoder(source.SampleRate, 600, secondReader: true);
        var envelope = new CwEnvelopeDetector(source.SampleRate);

        // Gated as the tab gates it: no detection, no letters (R97).
        decoder.KeyingGate = () => envelope.Reading.Keying;
        var feed = new CwScopeFeed();
        var start = new DateTime(2026, 9, 28, 18, 30, 0, DateTimeKind.Utc);
        var settled = new List<(CwCharacter, TimeSpan)>();

        DateTime Now() => start + decoder.Heard;

        decoder.CharacterSettled += c =>
        {
            settled.Add((c, decoder.Heard));
            feed.Settle(c, decoder.Heard, Now());
        };

        decoder.Listen(source);
        envelope.Listen(source);

        // The radio in CW at a 600 Hz pitch with a 500 Hz filter, as the tick reads it.
        envelope.SetPassband(600, 500);

        var perTick = (int)(source.SampleRate * TickMs / 1000);
        var ticks = (int)(seconds * 1000 / TickMs);
        var frame = CwScopeFrame.Empty;
        var keyed = CwScopeFrame.Empty;
        var recent = 0;

        for (var t = 0; t < ticks; t++)
        {
            source.PumpOnce(perTick);
            frame = feed.Tick(envelope, envelope.Reading, 600, frame, scopeQuiet: false, Now());

            if (frame.Reading.Keying)
            {
                keyed = frame;
            }

            if (t >= ticks - (1000 / TickMs) && frame.Hops.Count > 0)
            {
                recent++;
            }
        }

        decoder.Listen(null);
        envelope.Listen(null);

        return new Run(frame, keyed.Hops.Count > 0 ? keyed : frame, recent, settled, start);
    }

    internal void Print(Run run, IReadOnlyList<CwScopeItem> items)
    {
        _output.WriteLine($"hops {run.Last.Hops.Count} at {run.Last.HopMs} ms; frames with hops in the last second {run.FramesWithHopsInTheLastSecond}");
        _output.WriteLine($"marked hops {run.Last.Hops.Count(h => h.Mark)}; {run.Last.ToneLine} · {run.Last.MixingLine}");

        foreach (var (c, heard) in run.Settled)
        {
            _output.WriteLine($"settled [{c.Text}] {c.Confidence} at {c.At.TotalMilliseconds:0} ms span {c.SpanHops} hops, heard {heard.TotalMilliseconds:0} ms");
        }

        foreach (var item in items)
        {
            _output.WriteLine($"{item.Kind,-9} {item.X,7:0.0} {item.X2,7:0.0}  {item.Text}");
        }
    }
}
