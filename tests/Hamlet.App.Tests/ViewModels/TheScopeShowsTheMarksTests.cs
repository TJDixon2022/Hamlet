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
/// **THE OSCILLOSCOPE ON THE CW TAB: TRACE, FLOOR, THRESHOLD, MARKS AS BARS, THE TONE LINE**
/// (work instruction 476 task 2, step 12 criterion 12.2, R90, HM-DEC-185).
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
    /// Proves the tone line reads the pitch and its contrast while a mark is up, and says
    /// "no tone" when none is.
    /// </remarks>
    [Fact]
    public void TheToneLineReadsThePitchWhileAMarkIsUp()
    {
        var hearing = new CwHearingViewModel();

        // 0.8 s of noise then the first dah, stopped 90 ms into it.
        var mid = Keyed(742, 500, 0.89);
        hearing.ObserveScope(CwScopeFrame.From(mid.History(), mid.HopMs, mid.Reading));

        Assert.True(hearing.Scope.Reading.Mark);
        Assert.Matches(@"^tone 7[3-5]\d Hz, \d+ dB over the band$", hearing.Scope.ToneLine);

        var quiet = Keyed(742, 500, 0.7);
        hearing.ObserveScope(CwScopeFrame.From(quiet.History(), quiet.HopMs, quiet.Reading));

        Assert.False(hearing.Scope.Reading.Mark);
        Assert.Equal("no tone", hearing.Scope.ToneLine);
    }

    /// <remarks>
    /// Proves the threshold is labelled with its margin, and the passband with the radio's
    /// filter width and pitch - or says the rig's filter is unknown and the whole band is used.
    /// </remarks>
    [Fact]
    public void TheThresholdAndThePassbandAreLabelled()
    {
        var hearing = new CwHearingViewModel();
        var detector = Keyed(742, 500, 1.0);

        hearing.ObserveScope(CwScopeFrame.From(detector.History(), detector.HopMs, detector.Reading));

        Assert.Equal("threshold: floor + 9 dB", hearing.Scope.ThresholdLabel);
        Assert.Equal("filter 500 Hz at pitch 742 Hz: 492 to 992 Hz", hearing.Scope.PassbandLabel);

        var unknown = Keyed(null, null, 1.0);
        hearing.ObserveScope(CwScopeFrame.From(unknown.History(), unknown.HopMs, unknown.Reading));

        Assert.Equal("rig filter unknown: whole band 100 to 3000 Hz", hearing.Scope.PassbandLabel);

        var lines = CwScopeControl.Lines(hearing.Scope);

        Assert.Contains(lines, l => l.Kind == CwScopeLineKind.Envelope && l.Label == "envelope");
        Assert.Contains(lines, l => l.Kind == CwScopeLineKind.Floor && l.Label == "floor");
        Assert.Contains(lines, l => l.Kind == CwScopeLineKind.Threshold && l.Label == "threshold: floor + 9 dB");
        Assert.Contains(lines, l => l.Kind == CwScopeLineKind.Passband && l.Label == hearing.Scope.PassbandLabel);
        Assert.All(lines, l => Assert.False(string.IsNullOrWhiteSpace(l.Label)));
    }

    /// <remarks>
    /// Proves the hover says what each line is, that the threshold is the one number that
    /// decides a mark, and how often the scope is fed.
    /// </remarks>
    [Fact]
    public void TheHoverSaysWhatEachLineIsAndWhatDecidesAMark()
    {
        var tip = CwHearingViewModel.ScopeTip;

        Assert.Contains("Trace", tip, StringComparison.Ordinal);
        Assert.Contains("Dashed line", tip, StringComparison.Ordinal);
        Assert.Contains("Solid line", tip, StringComparison.Ordinal);
        Assert.Contains("Bars", tip, StringComparison.Ordinal);
        Assert.Contains("the one number that decides a mark", tip, StringComparison.Ordinal);
        Assert.Contains("20 times a second", tip, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves the scope is on the CW tab under 474's strip, with its hover and tone line,
    /// and paints a driven detector's frame without throwing.
    /// </remarks>
    [AvaloniaFact]
    public void TheScopeIsOnTheCwTabUnderTheStripAndPaints()
    {
        var (window, panel) = TheControlsTimCanPress.Open();

        try
        {
            var detector = Keyed(742, 500, 0.89);

            panel.CwHearing.ObserveScope(CwScopeFrame.From(detector.History(), detector.HopMs, detector.Reading));
            TheControlsTimCanPress.Settle(window);

            var cw = TheTopRowTests.Named<Grid>(window, "CwWorkspace");
            var scope = cw.GetVisualDescendants().OfType<CwScopeControl>().Single();
            var strip = cw.GetVisualDescendants().OfType<CwPitchStripControl>().Single();

            Assert.True(scope.IsEffectivelyVisible && scope.Bounds.Width > 100, "the scope is not drawn on the CW tab");
            Assert.True(
                scope.TranslatePoint(default, window)!.Value.Y > strip.TranslatePoint(default, window)!.Value.Y,
                "the scope is not under the strip");
            Assert.Equal(CwHearingViewModel.ScopeTip, ToolTip.GetTip(scope));
            Assert.Contains(
                cw.GetVisualDescendants().OfType<TextBlock>(),
                t => t.Text is { } text && text.StartsWith("tone 7", StringComparison.Ordinal));

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
