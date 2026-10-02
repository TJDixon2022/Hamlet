using Avalonia;
using Avalonia.Headless.XUnit;
using Hamlet.App.Controls;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// **THE LETTER SITS OVER THE BARS THAT MADE IT** (work instruction 480, the letter over the
/// bars, task 2, step 12 criterion 12.2, R94, §0.0).
/// </summary>
/// <remarks>
/// <para>**TIM, 2026-09-28**: *"This is a training tool. Over time I would start to recognize
/// those patterns."*</para>
/// <para>**THE LIVE PATH** of <see cref="TheScopeDrawsLiveTests"/>: the training radio's CQ at
/// twenty words a minute into a real decoder and a real detector, ticked every 50 ms of audio.
/// The span each letter is drawn over is the one the decoder settled it with - its end on the
/// decoder's audio clock and its length in the decoder's hops - and nothing moves it. No
/// recording is read (R88).</para>
/// </remarks>
public sealed class TheLetterSitsOverItsBarsTests
{
    private const double Width = 800;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the canvas is printed.</param>
    public TheLetterSitsOverItsBarsTests(ITestOutputHelper output) => _output = output;

    /// <remarks>
    /// Proves the picture: C over its four bars, over the span the decoder settled it with, and
    /// nothing over the gap after them. **The Q is not drawn, and that is the ruled cost**
    /// (work instruction 486, R97): it is the last letter of each short call, it settles after
    /// the detector has let go, and nothing reaches the screen while the panel says no keying.
    /// </remarks>
    [Fact]
    public void COverFourBarsAndNothingOverTheGap()
    {
        var run = TheScopeDrawsLiveTests.Listen(seconds: 12);
        var items = CwScopeControl.Items(run.Keyed, Width);

        new TheScopeDrawsLiveTests(_output).Print(run, items);

        var bars = items.Where(i => i.Kind == CwScopeItemKind.Bar).ToList();
        var letters = items.Where(i => i.Kind == CwScopeItemKind.Letter).ToList();

        // The newest C.
        var c = letters.LastOrDefault(l => l.Text == "C");
        Assert.True(c is not null, "no C is drawn over the scope");

        List<CwScopeItem> Under(CwScopeItem letter)
            => bars.Where(b => (b.X + b.X2) / 2 >= letter.X - 1 && (b.X + b.X2) / 2 <= letter.X2 + 1).ToList();

        var underC = Under(c!);

        _output.WriteLine($"C {c!.X:0.0}-{c.X2:0.0} over {underC.Count} bars");

        Assert.Equal(new[] { "dah", "dit", "dah", "dit" }, underC.Select(b => b.Text[..3]).ToArray());

        // The gap after C's last bar, up to the next bar, holds no letter.
        var gapFrom = underC[^1].X2;
        var gapTo = bars.Where(b => b.X > gapFrom).Select(b => b.X).DefaultIfEmpty(gapFrom).Min();

        Assert.DoesNotContain(letters, l => gapTo > gapFrom && l.X < gapTo - 1 && l.X2 > gapFrom + 1);

        // Where the decoder settled it: the span it gave, on its own clock.
        var now = run.Keyed.Training.NowUtc;
        var settled = run.Settled
            .Select(s => s.Character)
            .Where(ch => ch.Text == "C")
            .Select(ch => (End: run.Start + ch.At, Start: run.Start + ch.At - TimeSpan.FromMilliseconds(ch.SpanHops * CwProbabilisticDecoder.HopMilliseconds)))
            .Select(s => (Left: CwScopeControl.XOfTime(s.Start, now, Width), Right: CwScopeControl.XOfTime(s.End, now, Width)))
            .ToList();

        Assert.Contains(settled, s => Math.Abs(s.Left - c.X) < 0.01 && Math.Abs(s.Right - c.X2) < 0.01);

        // Every letter drawn stands over blocks, and a word gap draws nothing.
        Assert.All(letters, l => Assert.False(string.IsNullOrWhiteSpace(l.Text)));
        Assert.All(letters, l => Assert.NotEmpty(Under(l)));
    }

    /// <remarks>
    /// Proves the three odd cases: an unreadable character settles as the placeholder glyph, a
    /// prosign as its bracketed name, and a word gap draws nothing.
    /// </remarks>
    [Fact]
    public void APlaceholderIsItsGlyphAProsignItsNameAndAWordGapNothing()
    {
        var graph = new CwTrainingGraph();
        var now = new DateTime(2026, 9, 28, 18, 30, 0, DateTimeKind.Utc);

        graph.Settle(Character("X", CwConfidence.Unreadable, TimeSpan.FromSeconds(7)), TimeSpan.FromSeconds(10), now);
        graph.Settle(Character("<AR>", CwConfidence.High, TimeSpan.FromSeconds(8)), TimeSpan.FromSeconds(10), now);
        graph.Settle(Character(MorseAlphabet.WordGap, CwConfidence.High, TimeSpan.FromSeconds(9)), TimeSpan.FromSeconds(10), now);

        // Keying, and one block under all three, so only the glyphs decide what is drawn (R97).
        var frame = CwScopeFrame.Empty with
        {
            Reading = CwEnvelopeReading.None with { Keying = true },
            Training = graph.Frame(now) with
            {
                Bars = new[] { new CwGraphBar(now.AddSeconds(-3.6), now.AddSeconds(-0.9), 2700, true) },
            },
        };
        var letters = CwScopeControl.Items(frame, Width).Where(i => i.Kind == CwScopeItemKind.Letter).ToList();

        Assert.Equal(new[] { MorseAlphabet.Unreadable, "<AR>" }, letters.Select(l => l.Text).ToArray());
    }

    /// <remarks>
    /// Proves the control paints the live frame - blocks and letters, sure, unsure and
    /// unreadable - without throwing, at the height it asks for.
    /// </remarks>
    [AvaloniaFact]
    public void TheLiveFramePaintsWithItsLetters()
    {
        var run = TheScopeDrawsLiveTests.Listen(seconds: 12);
        var now = run.Keyed.Training.NowUtc;
        var graph = new CwTrainingGraph();

        graph.Settle(Character("X", CwConfidence.Unreadable, TimeSpan.FromSeconds(8)), TimeSpan.FromSeconds(10), now);
        graph.Settle(Character("E", CwConfidence.Low, TimeSpan.FromSeconds(9)), TimeSpan.FromSeconds(10), now);

        // A block under the two letters settled by hand, so they are drawn (R97): a letter is
        // drawn only over blocks.
        var under = new CwGraphBar(now.AddSeconds(-2.6), now.AddSeconds(-0.9), 1700, true);
        var frame = run.Keyed with
        {
            Training = run.Keyed.Training with
            {
                Bars = run.Keyed.Training.Bars.Append(under).OrderBy(b => b.StartUtc).ToList(),
                Letters = run.Keyed.Training.Letters.Concat(graph.Frame(now).Letters).OrderBy(l => l.StartUtc).ToList(),
            },
        };

        var scope = new CwScopeControl { Frame = frame };
        scope.Measure(new Size(Width, double.PositiveInfinity));
        scope.Arrange(new Rect(scope.DesiredSize));

        using var surface = new Avalonia.Media.Imaging.RenderTargetBitmap(
            new PixelSize((int)Width, (int)Math.Ceiling(scope.Bounds.Height)));
        using var context = surface.CreateDrawingContext();

        scope.Render(context);

        Assert.Equal(CwScopeControl.BarTop + CwScopeControl.TrainingBarHeight + 12, scope.DesiredSize.Height, 6);
        Assert.Contains(CwScopeControl.Items(frame, Width), i => i.Kind == CwScopeItemKind.Letter && i.Text == MorseAlphabet.Unreadable);
    }

    private static CwCharacter Character(string text, CwConfidence confidence, TimeSpan at)
        => new(text, confidence, 0.5, ".-.-.", 20, 20, at) { SpanHops = 100, Probability = 0.5 };
}
