using System;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// The two sentences unit 288 named: they say the grid that is running rather than
/// fifteen seconds.
/// </summary>
/// <remarks>
/// <para>**WORK INSTRUCTION 290, TASK 7 - THE NAMED DROP CANDIDATE, TAKEN.**
/// <c>DigitalIdleText</c> said *FT8 runs in fifteen second slots* and the waterfall
/// summary said *15 s slots*, both as literals, and both are §0.0 breaches on a
/// 7.5-second grid: the first tells the operator to wait twice as long as he needs
/// to before deciding a band is empty, and the second is a caption under a picture
/// asserting a boundary spacing the waterfall is not drawing (HM-DEC-092 binds
/// pictures as hard as sentences).</para>
/// <para>**BOTH OR NEITHER.** A screen that says two different things about the same
/// grid is worse than one that says the same wrong thing twice, so both read the one
/// <c>DigitalGrid</c> property and this test asserts they agree.</para>
/// <para>**THE BREAKAGE THIS CATCHES.** A future unit threading a mode into one of
/// the two and not the other, which produces a Digital tab captioned `7.5 s slots`
/// beside an idle line telling him to give a fifteen-second slot or two - and the
/// operator has no way to tell which of the two is the application and which is a
/// leftover.</para>
/// </remarks>
public sealed class TheScreenSaysWhichGridIsRunningTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the sentences are printed.</param>
    public TheScreenSaysWhichGridIsRunningTests(ITestOutputHelper output)
        => _output = output;

    private static MainWindowViewModel Panel()
        => new(new AppSettings(), null);

    /// <remarks>
    /// **FT8'S TWO SENTENCES, AND THE WATERFALL'S IS BYTE-IDENTICAL.** Only the idle
    /// line's wording moved, and it moved because it named a mode the tab has no way
    /// to know it is running.
    /// </remarks>
    [Fact]
    public void OnFt8TheSentencesStillSayFifteenSeconds()
    {
        var panel = Panel();

        var idle = panel.DigitalModeStripLine;

        _output.WriteLine("  idle : " + idle);

        Assert.Equal(SlotGrid.Ft8, panel.DigitalGrid);
        Assert.Contains("Slots here run 15 seconds", idle, StringComparison.Ordinal);
        Assert.DoesNotContain("fifteen", idle, StringComparison.OrdinalIgnoreCase);

        Assert.Equal(
            DigitalIdleText.ModeStripFor(SlotGrid.Ft8), idle);
    }

    /// <remarks>
    /// **AND ON A 7.5-SECOND GRID NEITHER OF THEM SAYS FIFTEEN.** The idle line
    /// names the length rather than the mode, for the same reason the capture
    /// sidecar does - the length is what was measured and the mode name is a label.
    /// </remarks>
    [Fact]
    public void OnAnFt4GridNeitherSentenceSaysFifteen()
    {
        var panel = Panel();

        panel.UseGridForTests(SlotGrid.Ft4);

        var idle = panel.DigitalModeStripLine;

        _output.WriteLine("  idle : " + idle);

        Assert.Contains("Slots here run 7.5 seconds", idle, StringComparison.Ordinal);
        Assert.DoesNotContain("fifteen", idle, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("15", idle, StringComparison.Ordinal);

        // The sentence still does its job: it still tells him to wait before
        // deciding the band is empty, which is the whole reason the line exists.
        Assert.Contains(
            "before deciding the band is empty", idle, StringComparison.Ordinal);
    }

    /// <remarks>
    /// **THE WATERFALL SUMMARY, WHICH IS THE ONE THAT IS A PICTURE'S CAPTION.** Its
    /// FT8 rendering is character for character what it was; its FT4 rendering says
    /// 7.5.
    /// </remarks>
    [Fact]
    public void TheWaterfallSummaryNamesTheGridItIsDrawing()
    {
        // The unknown-clock case first, because it is the one that must not change
        // on either grid: with no offset there is no grid to name (HM-DEC-009).
        var unknown = Panel();

        unknown.UseGridForTests(SlotGrid.Ft4);
        unknown.DigitalSpectrum = new AudioSpectrumSource(48000, simulated: false);

        _output.WriteLine("  no clock  : " + Summary(unknown));

        Assert.Contains(
            "no slot grid until the clock is checked",
            Summary(unknown),
            StringComparison.Ordinal);
        Assert.DoesNotContain("7.5 s slots", Summary(unknown), StringComparison.Ordinal);

        // And with a measured clock, each grid names itself.
        var ft8 = WithClock(SlotGrid.Ft8);
        var ft4 = WithClock(SlotGrid.Ft4);

        _output.WriteLine("  FT8       : " + Summary(ft8));
        _output.WriteLine("  FT4       : " + Summary(ft4));

        Assert.Contains("15 s slots", Summary(ft8), StringComparison.Ordinal);
        Assert.Contains("7.5 s slots", Summary(ft4), StringComparison.Ordinal);
        Assert.DoesNotContain("15 s slots", Summary(ft4), StringComparison.Ordinal);
    }

    /// <remarks>
    /// **BOTH OR NEITHER, ASSERTED RATHER THAN ARRANGED.** The two sentences are
    /// read off one property, so a unit that threads a mode into one of them threads
    /// it into both or fails here.
    /// </remarks>
    [Fact]
    public void TheTwoSentencesNeverDisagreeAboutTheSameGrid()
    {
        foreach (var grid in new[] { SlotGrid.Ft8, SlotGrid.Ft4 })
        {
            var panel = WithClock(grid);

            var length = grid.SlotSeconds.ToString(
                "0.##", System.Globalization.CultureInfo.InvariantCulture);

            _output.WriteLine($"  {length,-4} idle : {panel.DigitalModeStripLine}");
            _output.WriteLine($"  {length,-4} wfall: {Summary(panel)}");

            Assert.Contains(
                length + " seconds",
                panel.DigitalModeStripLine,
                StringComparison.Ordinal);
            Assert.Contains(length + " s slots", Summary(panel), StringComparison.Ordinal);
        }
    }

    /// <summary>A panel on a grid, with a measured clock so the summary names it.</summary>
    private static MainWindowViewModel WithClock(SlotGrid grid)
    {
        var panel = Panel();

        panel.UseGridForTests(grid);
        panel.DigitalSpectrum = new AudioSpectrumSource(48000, simulated: false);
        panel.ClockOffset = new ClockOffset(0, DateTime.UtcNow);

        return panel;
    }

    private static string Summary(MainWindowViewModel panel)
        => panel.DigitalWaterfallSummary;
}
