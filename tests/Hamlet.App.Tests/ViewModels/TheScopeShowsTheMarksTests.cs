using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Controls;
using Hamlet.App.Tests.Views;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Cw;
using Xunit;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// **THE OSCILLOSCOPE ON THE CW TAB: TRACE, MARKS AS BARS, THE TONE LINE** (work instruction
/// 476 task 2, step 12 criterion 12.2, R90, HM-DEC-185; the floor, threshold and passband
/// removed by work instruction 478, R92).
/// </summary>
/// <remarks>
/// <para>**A DRIVEN DETECTOR, NO RECORDING** (R88). The detector is fed a keyed tone made
/// here - C and Q at twenty words a minute over seeded noise - so the bars the scope must
/// draw are known: eight marks, the dahs three times the dits.</para>
/// <para>**WORDS ON EVERY MARK** (§0.6): every bar and every line the control draws
/// carries a label, and the hover says what each is.</para>
/// </remarks>
public sealed class TheScopeShowsTheMarksTests
{
    private const int Rate = 48_000;

    /// <remarks>
    /// Proves the eight keyed marks are eight bars along the bottom, each labelled with its
    /// length, the dahs about three times as wide as the dits.
    /// </remarks>
    [Fact]
    public void EveryKeyedMarkIsABarWithItsLength()
    {
        var hearing = new CwHearingViewModel();
        var detector = Keyed(742, 500, KeyingSeconds);

        hearing.ObserveScope(CwScopeFrame.From(detector.History(), detector.HopMs, detector.Reading));

        var bars = CwScopeControl.Bars(hearing.Scope, 800);

        Assert.Equal(8, bars.Count);
        Assert.All(bars, b => Assert.Matches(@"^mark \d+ ms$", b.Label));

        var widths = bars.Select(b => b.X2 - b.X).ToList();
        var dit = widths.Min();
        var dah = widths.Max();

        Assert.InRange(dah / dit, 2.4, 3.6);
    }

    /// <remarks>Proves the four seconds span the width, newest at the right.</remarks>
    [Fact]
    public void FourSecondsSpanTheWidthNewestAtTheRight()
    {
        var hearing = new CwHearingViewModel();
        var detector = Keyed(742, 500, 5.0);

        hearing.ObserveScope(CwScopeFrame.From(detector.History(), detector.HopMs, detector.Reading));

        Assert.Equal(4.0, hearing.Scope.Hops.Count * hearing.Scope.HopMs / 1000, 2);
        Assert.Equal(CwScopeControl.Pad, CwScopeControl.XOfHop(0, hearing.Scope, 800), 6);
        Assert.Equal(800 - CwScopeControl.Pad, CwScopeControl.XOfHop(hearing.Scope.Hops.Count, hearing.Scope, 800), 6);
    }

    /// <remarks>
    /// Proves the tone line reads the pitch while a mark is up, and says "no keying" before
    /// any keying (work instruction 478: the pitch alone, no contrast).
    /// </remarks>
    [Fact]
    public void TheToneLineReadsThePitchWhileAMarkIsUp()
    {
        var hearing = new CwHearingViewModel();

        // Stopped 90 ms into Q's first dah. **Not C's first dah any more** (work instruction
        // 477, R91): the first element of a transmission is a bar with no partner yet, and is
        // marked when the next element pairs with it across the gap.
        var mid = Keyed(742, 500, 1.73);
        hearing.ObserveScope(CwScopeFrame.From(mid.History(), mid.HopMs, mid.Reading));

        Assert.True(hearing.Scope.Reading.Mark);
        Assert.Matches(@"^tone 7[3-5]\d Hz heard$", hearing.Scope.ToneLine);

        var quiet = Keyed(742, 500, 0.7);
        hearing.ObserveScope(CwScopeFrame.From(quiet.History(), quiet.HopMs, quiet.Reading));

        Assert.False(hearing.Scope.Reading.Mark);
        Assert.Equal("no keying", hearing.Scope.ToneLine);
    }

    /// <remarks>
    /// Proves the hover says what the blocks and the letters are and what is in the corner, that
    /// nothing new is drawn with nobody keying and what is drawn stays, and names no trace, dashed or solid line (work
    /// instruction 485).
    /// </remarks>
    [Fact]
    public void TheHoverSaysWhatTheBlocksAndTheLettersAre()
    {
        // Since work instruction 485 (R97) there is no trace; the hover names what is drawn: the
        // blocks, the letters over them, the empty panel, and the two lines.
        var tip = CwHearingViewModel.ScopeTip;

        Assert.DoesNotContain("Trace", tip, StringComparison.Ordinal);
        Assert.Contains("Blocks", tip, StringComparison.Ordinal);
        Assert.Contains("nothing new is drawn", tip, StringComparison.Ordinal);
        Assert.Contains("flat tops", tip, StringComparison.Ordinal);
        Assert.Contains("Letters", tip, StringComparison.Ordinal);
        Assert.Contains("a gap is empty space as long as the gap was", tip, StringComparison.Ordinal);
        Assert.Contains("mixing at", tip, StringComparison.Ordinal);
        Assert.Contains("eight seconds", tip, StringComparison.Ordinal);
        Assert.DoesNotContain("Dashed line", tip, StringComparison.Ordinal);
        Assert.DoesNotContain("Solid line", tip, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves the scope is on the CW tab with the two verdict buttons beside it and no light
    /// or pitch strip left (work instruction 478 task 2), with its hover and tone line, and
    /// paints a driven detector's frame without throwing.
    /// </remarks>
    [AvaloniaFact]
    public void TheScopeIsOnTheCwTabBesideTheButtonsAndPaints()
    {
        var (window, panel) = TheControlsTimCanPress.Open();

        try
        {
            // Mid-way through Q's first dah, where a mark is up (work instruction 477).
            var detector = Keyed(742, 500, 1.73);

            panel.CwHearing.ObserveScope(CwScopeFrame.From(detector.History(), detector.HopMs, detector.Reading));
            TheControlsTimCanPress.Settle(window);

            var cw = TheTopRowTests.Named<Grid>(window, "CwWorkspace");
            var scope = cw.GetVisualDescendants().OfType<CwScopeControl>().Single();
            var buttons = cw.GetVisualDescendants().OfType<Button>()
                .Where(b => b.Content is "I agree with you" or "You're an idiot")
                .ToList();

            Assert.True(scope.IsEffectivelyVisible && scope.Bounds.Width > 100, "the scope is not drawn on the CW tab");
            Assert.Equal(2, buttons.Count);

            var scopeAt = scope.TranslatePoint(default, window)!.Value;

            foreach (var button in buttons)
            {
                var at = button.TranslatePoint(default, window)!.Value;

                Assert.True(at.X >= scopeAt.X + scope.Bounds.Width, $"{button.Content} is not to the right of the scope");
                Assert.True(
                    at.Y >= scopeAt.Y && at.Y + button.Bounds.Height <= scopeAt.Y + scope.Bounds.Height + 20,
                    $"{button.Content} is not beside the scope");
            }

            Assert.DoesNotContain(
                cw.GetVisualDescendants(),
                v => v.GetType().Name.Contains("PitchStrip", StringComparison.Ordinal)
                     || v.Name is "CwHearingLight" or "CwHearingDark");
            Assert.Equal(CwHearingViewModel.ScopeTip, ToolTip.GetTip(scope));
            Assert.Contains(
                cw.GetVisualDescendants().OfType<TextBlock>(),
                t => (t.Inlines?.Text ?? t.Text) is { } text
                     && text.StartsWith("tone 7", StringComparison.Ordinal)
                     && text.Contains("no station", StringComparison.Ordinal));

            window.UpdateLayout();

            var size = new PixelSize((int)Math.Ceiling(scope.Bounds.Width), (int)Math.Ceiling(scope.Bounds.Height));

            using var surface = new Avalonia.Media.Imaging.RenderTargetBitmap(size);
            using var context = surface.CreateDrawingContext();

            scope.Render(context);
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>How long the keyed pattern below runs, in seconds.</summary>
    private const double KeyingSeconds = 2.58;

    /// <summary>C, Q at 20 words a minute after 0.8 s of noise, then quiet.</summary>
    private static readonly (bool On, double Ms)[] Keying =
    {
        (false, 800),
        (true, 180), (false, 60), (true, 60), (false, 60), (true, 180), (false, 60), (true, 60),
        (false, 180),
        (true, 180), (false, 60), (true, 180), (false, 60), (true, 60), (false, 60), (true, 180),
        (false, 3000),
    };

    /// <summary>A detector fed the keyed pattern for the first <paramref name="seconds"/>.</summary>
    private static CwEnvelopeDetector Keyed(double? pitchHz, double? widthHz, double seconds)
    {
        var detector = new CwEnvelopeDetector(Rate);
        detector.SetPassband(pitchHz, widthHz);

        var random = new Random(476);
        var total = (int)(seconds * Rate);
        var audio = new float[total];
        var n = 0;

        foreach (var (on, ms) in Keying)
        {
            for (var i = 0; i < (int)Math.Round(ms * Rate / 1000) && n < total; i++, n++)
            {
                var u1 = 1.0 - random.NextDouble();
                var u2 = random.NextDouble();
                var noise = 0.06 * Math.Sqrt(-2 * Math.Log(u1)) * Math.Cos(2 * Math.PI * u2);
                var tone = on ? 0.3 * Math.Sin(2 * Math.PI * 742 * n / Rate) : 0;

                audio[n] = (float)(tone + noise);
            }
        }

        for (var offset = 0; offset < total; offset += 960)
        {
            detector.Process(audio.AsSpan(offset, Math.Min(960, total - offset)));
        }

        return detector;
    }
}
