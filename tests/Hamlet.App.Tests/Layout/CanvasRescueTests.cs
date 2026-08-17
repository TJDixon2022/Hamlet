using Hamlet.App.Layout;
using Hamlet.App.ViewModels;
using Xunit;

namespace Hamlet.App.Tests.Layout;

/// <summary>
/// The restore leaves the fold alone and names what it moved (HM-DEC-094).
/// </summary>
public sealed class CanvasRescueTests
{
    private static CanvasViewModel Loaded(params Placement[] placements)
        => new(
            body: null,
            new LayoutBook(new CanvasLayout("", "", placements)),
            changed: null);

    /// <remarks>
    /// <para>Proves HM-DEC-094, and it is the reported fault. **A widget below
    /// the fold is not off the edge of anything.** The canvas scrolls and has
    /// bars for exactly that purpose, and the rescue compared every widget
    /// against the visible viewport, so opening a saved layout on a smaller
    /// window scrambled it and announced that it had been saved.</para>
    /// </remarks>
    [Fact]
    public void AWidgetBelowTheFoldIsLeftWhereItIs()
    {
        var canvas = Loaded(
            new Placement(Widgets.Terminal, 12, 12, 600, 300),
            new Placement(Widgets.Phrasebook, 12, 1400, 380, 340),
            new Placement(Widgets.Spots, 1800, 12, 420, 520));

        // A window far smaller than the arrangement.
        var moved = canvas.FitInto(900, 640);

        Assert.False(moved);
        Assert.Equal("", canvas.RestoreNote);

        Assert.Equal(1400, canvas.Placed.Single(p => p.Id == Widgets.Phrasebook).Y);
        Assert.Equal(1800, canvas.Placed.Single(p => p.Id == Widgets.Spots).X);
    }

    /// <remarks>
    /// Proves HM-DEC-094: a negative coordinate is genuinely unreachable, because
    /// no scrollbar goes there, so that one is rescued.
    /// </remarks>
    [Fact]
    public void ANegativeCoordinateIsRescued()
    {
        var canvas = Loaded(
            new Placement(Widgets.Terminal, 12, 12, 600, 300),
            new Placement(Widgets.Spots, -900, 40, 420, 520));

        Assert.True(canvas.FitInto(900, 640));

        var rescued = canvas.Placed.Single(p => p.Id == Widgets.Spots);

        Assert.True(rescued.X >= 0);
        Assert.True(rescued.Y >= 0);

        // And the one that was fine did not move.
        Assert.Equal(12, canvas.Placed.Single(p => p.Id == Widgets.Terminal).X);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-094: **the notice names what moved.** "Everything else
    /// is where you left it" was doing a great deal of work while several widgets
    /// had been shifted, and a layout opened on a small monitor was scrambled with
    /// no way to tell how much had survived.</para>
    /// </remarks>
    [Fact]
    public void TheNoticeNamesWhatMovedAndHowMany()
    {
        var canvas = Loaded(
            new Placement(Widgets.Terminal, 12, 12, 600, 300),
            new Placement(Widgets.Spots, -900, 40, 420, 520),
            new Placement(Widgets.Guide, 40, -700, 420, 400));

        Assert.True(canvas.FitInto(900, 640));

        var note = canvas.RestoreNote;

        Assert.Contains("2 widgets", note, StringComparison.Ordinal);
        Assert.Contains("Happening now", note, StringComparison.Ordinal);
        Assert.Contains("Field guide", note, StringComparison.Ordinal);

        // The one that stayed is not named as having moved.
        Assert.DoesNotContain("CW terminal", note, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves HM-DEC-094: a coordinate no drag could have produced is rescued
    /// too, because a corrupt or truncated file is the other way a widget ends up
    /// somewhere no scrollbar reaches.
    /// </remarks>
    [Fact]
    public void ACoordinateNoDragCouldHaveProducedIsRescued()
    {
        var canvas = Loaded(
            new Placement(Widgets.Terminal, 12, 12, 600, 300),
            new Placement(Widgets.Heard, 9_000_000, 12, 420, 260));

        Assert.True(canvas.FitInto(900, 640));
        Assert.True(canvas.Placed.Single(p => p.Id == Widgets.Heard).X < 20_000);
    }

    /// <remarks>
    /// Proves HM-DEC-094: an arrangement that fits is left completely alone and
    /// says nothing, so the notice means something when it does appear.
    /// </remarks>
    [Fact]
    public void AnArrangementThatFitsIsSilent()
    {
        var canvas = Loaded(
            new Placement(Widgets.Terminal, 12, 12, 600, 300),
            new Placement(Widgets.Spots, 640, 12, 420, 520));

        Assert.False(canvas.FitInto(1920, 1080));
        Assert.Equal("", canvas.RestoreNote);
        Assert.False(canvas.HasRestoreNote);
    }
}
