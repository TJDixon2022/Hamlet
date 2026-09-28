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
/// **THE PITCH STRIP SHOWS WHERE THE DETECTOR LOOKS, AND LABELS EVERY MARK IN WORDS**
/// (work instruction 474 task 2, step 11 criterion 11.2, HM-DEC-184).
/// </summary>
/// <remarks>
/// <para>**NO AUDIO** (R88). The view model is handed a tracker pitch, admitted bins and a
/// meter reading directly, as the decode tick hands them over.</para>
/// <para>**IT IS DRAWN WIDER THAN THE DETECTOR LOOKS**, 200 to 1200 Hz against the
/// tracker's 300 to 900, so the owner sees the edges: a station beating at 1000 Hz is
/// invisible to the tracker by design, and the strip is where that shows.</para>
/// </remarks>
public sealed class TheStripShowsWhereTheDetectorLooksTests
{
    /// <remarks>Proves a driven tracker pitch is marked with its number.</remarks>
    [Fact]
    public void ADrivenTrackerPitchIsMarkedWithItsNumber()
    {
        var hearing = new CwHearingViewModel();

        hearing.Observe(State(tracker: 612, measured: true));

        Assert.Equal(612, hearing.Strip.TrackerHz);
        Assert.Equal("mixing 612 Hz", hearing.Strip.TrackerLabel);
    }

    /// <remarks>Proves a pitch the tracker fell back to says it was not measured.</remarks>
    [Fact]
    public void AnAssumedTrackerPitchSaysItWasNotMeasured()
    {
        var hearing = new CwHearingViewModel();

        hearing.Observe(State(tracker: 600, measured: false));

        Assert.Equal(600, hearing.Strip.TrackerHz);
        Assert.Equal("mixing 600 Hz, not measured", hearing.Strip.TrackerLabel);
    }

    /// <remarks>Proves the drawn range and the searched band, with the band's numbers.</remarks>
    [Fact]
    public void TheWholeRangeIsDrawnAndTheSearchedBandIsLabelledWithItsNumbers()
    {
        var hearing = new CwHearingViewModel();

        hearing.Observe(State(tracker: 612, measured: true));

        Assert.Equal(200, hearing.Strip.LowHz);
        Assert.Equal(1200, hearing.Strip.HighHz);
        Assert.Equal(CwToneTracker.MinimumToneHz, hearing.Strip.SearchLowHz);
        Assert.Equal(CwToneTracker.MaximumToneHz, hearing.Strip.SearchHighHz);
        Assert.Equal("searched 300 to 900 Hz", hearing.Strip.SearchLabel);
    }

    /// <remarks>Proves every admitted bin is marked and named.</remarks>
    [Fact]
    public void EveryAdmittedBinIsMarkedAndNamed()
    {
        var hearing = new CwHearingViewModel();

        hearing.Observe(State(612, true, KeyingVerdict.Listening, Bin(575), Bin(612.5)));

        Assert.Equal(new[] { 575.0, 612.5 }, hearing.Strip.AdmittedHz);
        Assert.Equal("survey admitted 575, 613 Hz", hearing.Strip.AdmittedLabel);
    }

    /// <remarks>Proves the meter's pitch is marked and its four figures are in the hover.</remarks>
    [Fact]
    public void TheMeterPitchIsMarkedWithItsFourFiguresInTheHover()
    {
        var hearing = new CwHearingViewModel();

        hearing.Observe(State(612, true, KeyingVerdict.Keying));

        Assert.Equal(610, hearing.Strip.MeterHz);
        Assert.Equal("meter 610 Hz", hearing.Strip.MeterLabel);
        Assert.Contains("score 0.21", hearing.StripTip, StringComparison.Ordinal);
        Assert.Contains("median 70 ms", hearing.StripTip, StringComparison.Ordinal);
        Assert.Contains("swing 22 dB", hearing.StripTip, StringComparison.Ordinal);
        Assert.Contains("verdict keying", hearing.StripTip, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves every mark the control draws carries words, never color alone (§0.6), and
    /// the tracker's mark lands inside the shaded band.
    /// </remarks>
    [Fact]
    public void EveryDrawnMarkCarriesWords()
    {
        var hearing = new CwHearingViewModel();

        hearing.Observe(State(612, true, KeyingVerdict.Keying, Bin(575)));

        var marks = CwPitchStripControl.Marks(hearing.Strip, 500);

        Assert.Contains(marks, m => m.Kind == CwPitchMarkKind.Searched);
        Assert.Contains(marks, m => m.Kind == CwPitchMarkKind.Admitted);
        Assert.Contains(marks, m => m.Kind == CwPitchMarkKind.Tracker);
        Assert.Contains(marks, m => m.Kind == CwPitchMarkKind.Meter);
        Assert.All(marks, m => Assert.False(string.IsNullOrWhiteSpace(m.Label)));

        var band = marks.Single(m => m.Kind == CwPitchMarkKind.Searched);
        var tracker = marks.Single(m => m.Kind == CwPitchMarkKind.Tracker);

        Assert.InRange(tracker.X, band.X, band.X2);
        Assert.Equal(CwPitchStripControl.XOf(612, hearing.Strip, 500), tracker.X, 6);
    }

    /// <remarks>
    /// Proves the strip is on the CW tab with its hover and the admitted line, and that it
    /// paints with a driven tracker pitch without throwing.
    /// </remarks>
    [AvaloniaFact]
    public void TheStripIsOnTheCwTabAndPaintsADrivenPitch()
    {
        var (window, panel) = TheControlsTimCanPress.Open();

        try
        {
            panel.CwHearing.Observe(State(612, true, KeyingVerdict.Keying, Bin(575)));
            TheControlsTimCanPress.Settle(window);

            var cw = TheTopRowTests.Named<Grid>(window, "CwWorkspace");
            var strip = cw.GetVisualDescendants().OfType<CwPitchStripControl>().Single();
            var tip = ToolTip.GetTip(strip) as string;

            Assert.True(strip.IsEffectivelyVisible && strip.Bounds.Width > 100, "the strip is not drawn on the CW tab");
            Assert.Equal(612, strip.Strip!.TrackerHz);
            Assert.Contains("Solid line", tip, StringComparison.Ordinal);
            Assert.Contains(
                cw.GetVisualDescendants().OfType<TextBlock>(),
                t => t.Text == "survey admitted 575 Hz");

            window.UpdateLayout();

            var size = new PixelSize((int)Math.Ceiling(strip.Bounds.Width), (int)Math.Ceiling(strip.Bounds.Height));

            using var surface = new Avalonia.Media.Imaging.RenderTargetBitmap(size);
            using var context = surface.CreateDrawingContext();

            strip.Render(context);
        }
        finally
        {
            window.Close();
        }
    }

    private static KeyingCandidate Bin(double hz) => new(hz, 60, 180, 3, 6, 18, 12, -40);

    private static CwHearingState State(
        double tracker, bool measured, KeyingVerdict verdict = KeyingVerdict.Listening,
        params KeyingCandidate[] admitted)
        => new(
            new KeyingReading(verdict, verdict == KeyingVerdict.Keying ? 610 : 0, 70, 22, 30, 0.21, false),
            tracker,
            measured,
            false,
            admitted);
}
