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
    /// Proves the picture: C over its four bars and Q over its four, each over the span the
    /// decoder settled it with, and nothing over the gap between them.
    /// </remarks>
    [Fact]
    public void COverFourBarsQOverFourAndNothingOverTheGap()
    {
        var run = TheScopeDrawsLiveTests.Listen(seconds: 12);
        var items = CwScopeControl.Items(run.Last, Width);

        new TheScopeDrawsLiveTests(_output).Print(run, items);

        var bars = items.Where(i => i.Kind == CwScopeItemKind.Bar).ToList();
        var letters = items.Where(i => i.Kind == CwScopeItemKind.Letter).ToList();

        // The newest C with a Q after it.
        var c = letters.LastOrDefault(l => l.Text == "C" && letters.Any(q => q.Text == "Q" && q.X > l.X2));
        Assert.True(c is not null, "no C with a Q after it is drawn over the scope");
        var q = letters.First(l => l.Text == "Q" && l.X > c!.X2);

        List<CwScopeItem> Under(CwScopeItem letter)
            => bars.Where(b => (b.X + b.X2) / 2 >= letter.X - 1 && (b.X + b.X2) / 2 <= letter.X2 + 1).ToList();

        var underC = Under(c!);
        var underQ = Under(q);

        _output.WriteLine($"C {c!.X:0.0}-{c.X2:0.0} over {underC.Count} bars; Q {q.X:0.0}-{q.X2:0.0} over {underQ.Count} bars");

        Assert.Equal(new[] { "dah", "dit", "dah", "dit" }, underC.Select(b => b.Text[..3]).ToArray());
        Assert.Equal(new[] { "dah", "dah", "dit", "dah" }, underQ.Select(b => b.Text[..3]).ToArray());

        // The gap between C's last bar and Q's first holds no letter.
        var gapFrom = underC[^1].X2;
        var gapTo = underQ[0].X;

        Assert.True(gapTo > gapFrom, "C's bars and Q's bars overlap");
        Assert.DoesNotContain(letters, l => l.X < gapTo - 1 && l.X2 > gapFrom + 1);

        // Where the decoder settled them: the span it gave, on its own clock.
        var now = run.Last.Training.NowUtc;
        foreach (var (item, text) in new[] { (c, "C"), (q, "Q") })
        {
            var settled = run.Settled
                .Select(s => s.Character)
                .Where(ch => ch.Text == text)
                .Select(ch => (End: run.Start + ch.At, Start: run.Start + ch.At - TimeSpan.FromMilliseconds(ch.SpanHops * CwProbabilisticDecoder.HopMilliseconds)))
                .Select(s => (Left: CwScopeControl.XOfTime(s.Start, now, Width), Right: CwScopeControl.XOfTime(s.End, now, Width)))
                .ToList();

            Assert.Contains(settled, s => Math.Abs(s.Left - item.X) < 0.01 && Math.Abs(s.Right - item.X2) < 0.01);
        }

        // A word gap draws nothing: every letter drawn is a settled character with text.
        Assert.All(letters, l => Assert.False(string.IsNullOrWhiteSpace(l.Text)));
    }

    /// <remarks>
    /// Proves the hover over a letter says where it came from and how sure the decoder was.
    /// </remarks>
    [Fact]
    public void TheHoverOverALetterSaysTheDecoderMadeItFromTheBarsBeneathIt()
    {
        var run = TheScopeDrawsLiveTests.Listen(seconds: 12);
        var letter = CwScopeControl.Items(run.Last, Width).Last(i => i.Kind == CwScopeItemKind.Letter);

        var tip = CwScopeControl.TipAt(run.Last, Width, new Point((letter.X + letter.X2) / 2, CwScopeControl.LetterTop + 6));

        _output.WriteLine(tip);

        Assert.StartsWith(letter.Text + ": " + CwScopeControl.LetterTipWords, tip, StringComparison.Ordinal);
        Assert.Contains("sure", tip, StringComparison.Ordinal);
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

        var frame = CwScopeFrame.Empty with { Training = graph.Frame(now) };
        var letters = CwScopeControl.Items(frame, Width).Where(i => i.Kind == CwScopeItemKind.Letter).ToList();

        Assert.Equal(new[] { MorseAlphabet.Unreadable, "<AR>" }, letters.Select(l => l.Text).ToArray());
    }

    /// <remarks>
    /// Proves the control paints the live frame - trace, bars and letters, sure, unsure and
    /// unreadable - without throwing, at the height it asks for.
    /// </remarks>
    [AvaloniaFact]
    public void TheLiveFramePaintsWithItsLetters()
    {
        var run = TheScopeDrawsLiveTests.Listen(seconds: 12);
        var now = run.Last.Training.NowUtc;
        var graph = new CwTrainingGraph();

        graph.Settle(Character("X", CwConfidence.Unreadable, TimeSpan.FromSeconds(8)), TimeSpan.FromSeconds(10), now);
        graph.Settle(Character("E", CwConfidence.Low, TimeSpan.FromSeconds(9)), TimeSpan.FromSeconds(10), now);

        var frame = run.Last with
        {
            Training = run.Last.Training with
            {
                Letters = run.Last.Training.Letters.Concat(graph.Frame(now).Letters).OrderBy(l => l.StartUtc).ToList(),
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
