using Hamlet.App.Layout;
using Hamlet.App.ViewModels;
using Xunit;

namespace Hamlet.App.Tests.Layout;

/// <summary>
/// What happens when a widget arrives, is taken hold of, or speaks from off the
/// canvas (HM-DEC-087).
/// </summary>
public sealed class CanvasArrivalTests
{
    /// <summary>
    /// A canvas with widgets on it, for the tests that manipulate one.
    /// </summary>
    /// <remarks>
    /// **A FIRST RUN NO LONGER FURNISHES THE CANVAS** (Tim, 2026-08-27: "Remove
    /// all widgets for now. Leave them on the far left side."), so a test that
    /// takes a widget off one has to put a widget on it first.
    /// </remarks>
    private static CanvasViewModel Furnished()
    {
        var canvas = new CanvasViewModel(null);

        canvas.LoadCommand.Execute(
            canvas.Presets.First(p => p.Name == LayoutPresets.FirstRun));

        return canvas;
    }

    /// <remarks>
    /// <para>Proves HM-DEC-087: **a widget arrives showing its contents.**
    /// Everything used to arrive shut, so pulling three things out of the tray
    /// gave three title bars and an empty canvas, and somebody who reaches for a
    /// panel is reaching for what is in it.</para>
    /// <para>The panel still owns whether it is open, so what is asserted is that
    /// the canvas asks for it to be opened rather than that it keeps a second
    /// copy of the answer.</para>
    /// </remarks>
    [Fact]
    public void AWidgetFromTheTrayArrivesOpen()
    {
        var opened = new List<string>();
        var canvas = new CanvasViewModel(null, null, null, opened.Add);

        var arriving = canvas.Tray.First();
        canvas.Add(arriving);

        Assert.Equal(new[] { arriving.Id }, opened);
    }

    /// <remarks>
    /// Proves HM-DEC-087: a summoned widget arrives open too. The phrasebook
    /// appearing as a shut title bar at the moment a contact starts would be the
    /// worst of both, since it takes up room and says nothing.
    /// </remarks>
    [Fact]
    public void ASummonedWidgetArrivesOpen()
    {
        var opened = new List<string>();
        var canvas = new CanvasViewModel(null, null, null, opened.Add);

        canvas.Remove(canvas.Placed.FirstOrDefault(p => p.Id == Widgets.Phrasebook)!);
        opened.Clear();

        canvas.Summon(Widgets.Phrasebook);

        Assert.Equal(new[] { Widgets.Phrasebook }, opened);
    }

    /// <remarks>
    /// Proves HM-DEC-087: taking hold of a widget brings it to the front.
    /// Dragging one over another used to slide it underneath, so it vanished
    /// behind the thing it was being moved beside, which reads as the drag having
    /// failed. Last in the list is drawn last and therefore on top.
    /// </remarks>
    [Fact]
    public void TakingHoldOfAWidgetBringsItToTheFront()
    {
        var canvas = Furnished();
        var first = canvas.Placed.First();

        Assert.NotSame(first, canvas.Placed.Last());

        canvas.Raise(first);

        Assert.Same(first, canvas.Placed.Last());

        // Raising the one already in front leaves the order alone.
        var order = canvas.Placed.Select(p => p.Id).ToList();
        canvas.Raise(first);
        Assert.Equal(order, canvas.Placed.Select(p => p.Id));
    }

    /// <remarks>
    /// <para>Proves HM-DEC-087: **the two kinds of news do not look the same.**
    /// Morse arriving right now and a tally that will keep are different facts,
    /// and a notice that draws them identically teaches the operator to read past
    /// both.</para>
    /// <para>Color is not the only carrier, per §0.6: a live note also has a mark
    /// beside it that a quiet one does not.</para>
    /// </remarks>
    [Fact]
    public void LiveNewsAndNewsThatWillKeepAreToldApart()
    {
        var canvas = Furnished();

        canvas.Remove(canvas.Placed.First(p => p.Id == Widgets.Spots));
        canvas.Remove(canvas.Placed.First(p => p.Id == Widgets.Guide));

        canvas.News(Widgets.Spots, "Somebody is calling now.", AbsentUrgency.Live);
        canvas.News(Widgets.Guide, "Six modes are described here.", AbsentUrgency.Quiet);

        var live = canvas.Absent.Single(a => a.Id == Widgets.Spots);
        var quiet = canvas.Absent.Single(a => a.Id == Widgets.Guide);

        Assert.True(live.IsLive);
        Assert.True(quiet.IsQuiet);
        Assert.False(live.IsQuiet);

        // Drawn differently, and the mark is what survives a grayscale print.
        Assert.NotEqual(live.Look, quiet.Look);
        Assert.NotEqual(live.IsLive, quiet.IsLive);
    }

    /// <remarks>
    /// Proves HM-DEC-087: a note whose urgency changes is replaced rather than
    /// left showing the old one. It used to compare only the words, so news that
    /// became urgent went on being drawn quietly.
    /// </remarks>
    [Fact]
    public void ANoteThatBecomesUrgentIsRedrawn()
    {
        var canvas = Furnished();
        canvas.Remove(canvas.Placed.First(p => p.Id == Widgets.Spots));

        canvas.News(Widgets.Spots, "Somebody is calling.", AbsentUrgency.Quiet);
        Assert.True(canvas.Absent.Single().IsQuiet);

        canvas.News(Widgets.Spots, "Somebody is calling.", AbsentUrgency.Live);

        Assert.True(canvas.Absent.Single().IsLive);
        Assert.Single(canvas.Absent);
    }
}
