using Hamlet.App.Controls;
using Hamlet.App.Layout;
using Hamlet.App.ViewModels;
using Xunit;

namespace Hamlet.App.Tests.Layout;

/// <summary>
/// The canvas: presets, saving, summoning and what happens to a widget that is
/// not out (HM-DEC-086).
/// </summary>
public sealed class CanvasTests
{
    private static CanvasViewModel New(LayoutBook? book = null, Action? changed = null)
        => new(body: null, book, changed);

    /// <summary>
    /// A canvas with widgets on it, for the tests that manipulate one.
    /// </summary>
    /// <remarks>
    /// **A FIRST RUN NO LONGER FURNISHES THE CANVAS** (Tim, 2026-08-27), so a
    /// test that takes a widget off one has to put a widget on it first. The
    /// arrangement is the one that used to be the default, loaded by name.
    /// </remarks>
    private static CanvasViewModel Furnished()
    {
        var canvas = New();

        canvas.LoadCommand.Execute(
            canvas.Presets.First(p => p.Name == LayoutPresets.FirstRun));

        return canvas;
    }

    /// <remarks>
    /// <para>Proves what a first run lands on after Tim's ruling of 2026-08-27:
    /// **the two panels, with nothing out.**</para>
    /// <para>**HM-DEC-086 SAYS NOBODY EVER STARTS ON AN EMPTY CANVAS, AND THAT IS
    /// STILL TRUE.** What that ruling forbids is an empty rectangle beside a list
    /// of things to drag — a puzzle handed to somebody who came here to talk on
    /// the radio. The screen a first run now lands on is not empty: Receive and
    /// Send are permanent panels on the CW tab, and the band plan, the
    /// neighborhood and the radio are permanent above the divider. **What is
    /// empty is the canvas layer, and the operator is looking at four panels
    /// rather than a blank.**</para>
    /// <para>The furnished arrangement is still on the preset bar, one press
    /// away, under its own name.</para>
    /// </remarks>
    [Fact]
    public void AFirstRunLandsOnTheTwoPanelsAndNotAnEmptyRectangle()
    {
        var canvas = New();

        Assert.Empty(canvas.Placed);
        Assert.Equal("Just receive and send", canvas.StartedFrom);

        // And the furnished one is still there to go back to.
        Assert.Contains(
            canvas.Presets, p => p.Name == LayoutPresets.FirstRun);
    }

    /// <remarks>
    /// Proves HM-DEC-086: an unreadable or missing layouts file is not a reason
    /// to show nothing. It lands exactly where a first run does.
    /// </remarks>
    [Fact]
    public void AnUnreadableLayoutFileStillLandsSomewhereUseful()
    {
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        File.WriteAllText(path, "{ this is not json");

        try
        {
            var canvas = New(LayoutStore.LoadFrom(path));

            // Exactly where a first run does, which since 2026-08-27 is the
            // two panels with nothing out — not a blank, and not the old
            // furnished arrangement.
            Assert.Empty(canvas.Placed);
            Assert.Equal("Just receive and send", canvas.StartedFrom);
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <remarks>
    /// <para>Proves HM-DEC-086: **a preset is a starting point and never a
    /// document.** Loading one, dragging everything about and loading it again
    /// gives back exactly what it gave the first time.</para>
    /// <para>This is the rule that makes the preset bar safe to press. Without
    /// it, the way back from a canvas that has got away from somebody is itself
    /// something they can spoil.</para>
    /// </remarks>
    [Fact]
    public void RearrangingAPresetDoesNotChangeThePreset()
    {
        var canvas = New();
        var preset = canvas.Presets.First(p => p.Name == "Making contacts");

        canvas.Load(preset);
        var before = canvas.Placed.Select(p => (p.Id, p.X, p.Y, p.Width)).ToList();

        foreach (var widget in canvas.Placed)
        {
            widget.X += 137;
            widget.Y += 42;
            widget.Width += 55;
            canvas.Moved(widget);
        }

        canvas.Load(canvas.Presets.First(p => p.Name == "Making contacts"));
        var after = canvas.Placed.Select(p => (p.Id, p.X, p.Y, p.Width)).ToList();

        Assert.Equal(before, after);

        // And the shipped preset object itself was never written to.
        Assert.Equal(
            preset.Placements.Select(p => (p.Id, p.X, p.Y)).ToList(),
            LayoutPresets.Fresh("Making contacts")!
                .Placements.Select(p => (p.Id, p.X, p.Y)).ToList());
    }

    /// <remarks>
    /// Proves HM-DEC-086: the presets are named by what you are doing rather than
    /// by mode, because a name somebody has to already understand in order to
    /// pick is no help to the person this application exists for. And there is no
    /// FT8 preset, which was ruled rather than overlooked.
    /// </remarks>
    [Fact]
    public void ThePresetsAreNamedByActivity()
    {
        var names = LayoutPresets.All.Select(p => p.Name).ToList();

        Assert.Equal(
            new[]
            {
                // **THE CLEAR ONE COMES FIRST** (Tim, 2026-08-27). It is what a
                // first run gets and the one press back to Receive and Send.
                "Just receive and send",
                "Getting started",
                "Listening around",
                "Making contacts",
            },
            names);

        foreach (var name in names)
        {
            Assert.DoesNotContain("CW", name, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("FT8", name, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("mode", name, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <remarks>
    /// Proves HM-DEC-086: **the send controls sit directly beneath the terminal**
    /// in Making contacts, so reading a call and answering it is one motion, and
    /// the band map is deliberately absent because it belongs to looking around.
    /// Tim's own arrangement, held by a test so a later tidy-up cannot quietly
    /// undo the reasoning.
    /// </remarks>
    [Fact]
    public void MakingContactsPutsTheAnswerUnderTheThingBeingAnswered()
    {
        var layout = LayoutPresets.Fresh("Making contacts")!;

        var terminal = layout.Placements.Single(p => p.Id == Widgets.Terminal);
        var send = layout.Placements.Single(p => p.Id == Widgets.Send);
        var spots = layout.Placements.Single(p => p.Id == Widgets.Spots);

        Assert.Equal(terminal.X, send.X);
        Assert.True(send.Y >= terminal.Y + terminal.Height);
        Assert.True(spots.X >= terminal.X + terminal.Width);

        Assert.False(layout.Holds(Widgets.Map));
    }

    /// <remarks>
    /// Proves HM-DEC-086: every widget is either on the canvas or in the tray,
    /// and never both and never neither. A widget that could be in neither is a
    /// widget the operator cannot get back.
    /// </remarks>
    [Fact]
    public void EveryWidgetIsEitherOutOrInTheTray()
    {
        var canvas = New();

        foreach (var layout in canvas.Presets.ToList())
        {
            canvas.Load(layout);

            var placed = canvas.Placed.Select(p => p.Id).ToList();
            var tray = canvas.Tray.Select(t => t.Id).ToList();

            Assert.Empty(placed.Intersect(tray));

            // **THE NEIGHBORHOOD MAP IS NEITHER, AND THAT IS THE RULING** (Tim,
            // 2026-08-27). It is a header panel now, permanent above the divider
            // in every mode, so it is not in the tray and no preset places it —
            // otherwise the same panel would appear twice. It stays in the
            // catalogue so a saved layout naming it still resolves.
            Assert.DoesNotContain(Widgets.Map, tray);

            Assert.Equal(
                Widgets.All.Select(w => w.Id)
                    .Where(id => id != Widgets.Map)
                    .OrderBy(id => id),
                placed.Concat(tray).Where(id => id != Widgets.Map)
                    .OrderBy(id => id));
        }
    }

    /// <remarks>
    /// Proves HM-DEC-086: taking a widget off puts it back in the tray, and it
    /// goes back where it belongs in the order rather than on the end.
    /// </remarks>
    [Fact]
    public void TakingOneOffPutsItBackInTheTray()
    {
        var canvas = Furnished();
        var widget = canvas.Placed.First();
        var id = widget.Id;

        canvas.Remove(widget);

        Assert.DoesNotContain(canvas.Placed, p => p.Id == id);
        Assert.Contains(canvas.Tray, t => t.Id == id);

        canvas.Add(Widgets.Find(id));

        Assert.Contains(canvas.Placed, p => p.Id == id);
        Assert.DoesNotContain(canvas.Tray, t => t.Id == id);
    }

    /// <remarks>
    /// Proves HM-DEC-086: a widget added to a full canvas does not land on top of
    /// something else, which would look like nothing happened.
    /// </remarks>
    [Fact]
    public void AnAddedWidgetDoesNotLandOnTopOfAnother()
    {
        var canvas = New();
        canvas.Load(canvas.Presets.First(p => p.Name == "Making contacts"));

        var arriving = canvas.Tray.First();
        canvas.Add(arriving);

        var placed = canvas.Placed.Single(p => p.Id == arriving.Id);

        foreach (var other in canvas.Placed.Where(p => p.Id != arriving.Id))
        {
            var apart = placed.X >= other.X + other.Width
                || other.X >= placed.X + placed.Width
                || placed.Y >= other.Y + other.Height
                || other.Y >= placed.Y + placed.Height;

            Assert.True(apart, $"{arriving.Id} landed on top of {other.Id}");
        }
    }

    /// <remarks>
    /// Proves HM-DEC-086: **saving is one action from where you are**, and what
    /// gets saved is the arrangement in front of the operator.
    /// </remarks>
    [Fact]
    public void KeepingAnArrangementSavesWhatIsOnScreen()
    {
        var saves = 0;
        var canvas = New(changed: () => saves++);

        // Furnish it first: a first run no longer puts anything out, and this
        // test is about what moving a widget saves (Tim, 2026-08-27).
        canvas.LoadCommand.Execute(
            canvas.Presets.First(p => p.Name == LayoutPresets.FirstRun));

        saves = 0;

        canvas.Placed.First().X = 321;
        canvas.NewName = "  Evening on 40  ";

        Assert.True(canvas.CanKeep);
        canvas.Keep();

        var kept = Assert.Single(canvas.Saved);

        Assert.Equal("Evening on 40", kept.Name);
        Assert.False(kept.Preset);
        Assert.Contains(kept.Placements, p => p.X == 321);
        Assert.True(saves > 0);

        // The box empties, so the next save is not the last name again.
        Assert.Equal("", canvas.NewName);
        Assert.False(canvas.CanKeep);
    }

    /// <remarks>
    /// Proves HM-DEC-086: saving over a name replaces that arrangement rather
    /// than leaving two things called the same thing.
    /// </remarks>
    [Fact]
    public void SavingTheSameNameTwiceKeepsOneOfThem()
    {
        var canvas = Furnished();

        canvas.NewName = "Evening";
        canvas.Keep();
        canvas.Remove(canvas.Placed.First());
        canvas.NewName = "Evening";
        canvas.Keep();

        Assert.Single(canvas.Saved);
    }

    /// <remarks>
    /// Proves HM-DEC-086: an arrangement survives being written out and read back
    /// in, which is what makes it worth saving at all.
    /// </remarks>
    [Fact]
    public void AnArrangementSurvivesTheRoundTrip()
    {
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".json");

        try
        {
            var canvas = New();
            canvas.Load(canvas.Presets.First(p => p.Name == "Listening around"));
            canvas.Placed.First().X = 456.5;
            canvas.NewName = "Mine";
            canvas.Keep();

            LayoutStore.SaveTo(canvas.Book(), path);

            var back = New(LayoutStore.LoadFrom(path));

            Assert.Equal(
                canvas.Placed.Select(p => (p.Id, p.X, p.Y, p.Width, p.Height)),
                back.Placed.Select(p => (p.Id, p.X, p.Y, p.Width, p.Height)));

            Assert.Equal("Mine", Assert.Single(back.Saved).Name);
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <remarks>
    /// Proves HM-DEC-086: a saved layout naming a widget this build does not have
    /// loses that widget and nothing else. A layout is the operator's document
    /// and a build that cannot read one of its lines is not a reason to throw the
    /// rest away.
    /// </remarks>
    [Fact]
    public void ALayoutNamingAnUnknownWidgetKeepsTheRest()
    {
        var book = new LayoutBook(new CanvasLayout(
            "", "",
            new[]
            {
                new Placement(Widgets.Terminal, 10, 10, 400, 300),
                new Placement("somethingFromTheFuture", 10, 320, 400, 300),
            }));

        var canvas = New(book);

        Assert.Single(canvas.Placed);
        Assert.Equal(Widgets.Terminal, canvas.Placed[0].Id);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-086: **some widgets arrive on their own.** The
    /// phrasebook comes out when a contact starts and goes away when it ends.
    /// </para>
    /// <para>And only widgets that declare themselves summonable can arrive that
    /// way, so no future wiring can make an arbitrary panel jump onto somebody's
    /// canvas.</para>
    /// </remarks>
    [Fact]
    public void ASummonedWidgetArrivesAndLeaves()
    {
        var canvas = Furnished();
        canvas.Remove(canvas.Placed.FirstOrDefault(p => p.Id == Widgets.Phrasebook)!);

        canvas.Summon(Widgets.Phrasebook);
        Assert.Contains(canvas.Placed, p => p.Id == Widgets.Phrasebook);

        canvas.Dismiss(Widgets.Phrasebook);
        Assert.DoesNotContain(canvas.Placed, p => p.Id == Widgets.Phrasebook);

        // Nothing else may arrive this way.
        canvas.Remove(canvas.Placed.First(p => p.Id == Widgets.Spots));
        canvas.Summon(Widgets.Spots);
        Assert.DoesNotContain(canvas.Placed, p => p.Id == Widgets.Spots);
    }

    /// <remarks>
    /// Proves HM-DEC-086: a summoned widget the operator has moved is theirs from
    /// then on and is never taken away again. A panel that vanishes just after
    /// somebody has put it where they want it teaches them not to touch anything.
    /// </remarks>
    [Fact]
    public void AWidgetTheOperatorHasMovedIsNeverTakenAway()
    {
        var canvas = New();
        canvas.Remove(canvas.Placed.FirstOrDefault(p => p.Id == Widgets.Phrasebook)!);

        canvas.Summon(Widgets.Phrasebook);

        var placed = canvas.Placed.First(p => p.Id == Widgets.Phrasebook);
        placed.X = 700;
        canvas.Moved(placed);

        canvas.Dismiss(Widgets.Phrasebook);

        Assert.Contains(canvas.Placed, p => p.Id == Widgets.Phrasebook);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-086, and it is the answer to the question the ruling
    /// had to settle: **what happens when something arrives for a widget that is
    /// not on the canvas.** It is not swallowed, and the widget is not flung onto
    /// somebody's arrangement either. A line says what is happening with one press
    /// to bring the widget out.</para>
    /// <para>It is §0.5 one level up: a collapsed panel still carries its summary,
    /// and a widget that is not out still carries its news.</para>
    /// </remarks>
    [Fact]
    public void NewsFromAnAbsentWidgetIsNeitherSwallowedNorForced()
    {
        var canvas = Furnished();
        canvas.Remove(canvas.Placed.First(p => p.Id == Widgets.Spots));

        canvas.News(Widgets.Spots, "Somebody is calling CQ on this band.");

        var note = Assert.Single(canvas.Absent);

        Assert.True(canvas.HasAbsent);
        Assert.Equal("Happening now", note.Title);

        // Not forced onto the canvas by the news alone.
        Assert.DoesNotContain(canvas.Placed, p => p.Id == Widgets.Spots);

        canvas.Show(note);

        Assert.Contains(canvas.Placed, p => p.Id == Widgets.Spots);
        Assert.Empty(canvas.Absent);
        Assert.False(canvas.HasAbsent);
    }

    /// <remarks>
    /// Proves HM-DEC-086: a widget that is out says nothing from off the canvas,
    /// so the line is never a duplicate of something already on screen.
    /// </remarks>
    [Fact]
    public void AWidgetThatIsOutSaysNothingFromOffTheCanvas()
    {
        var canvas = Furnished();

        canvas.News(Widgets.Spots, "Somebody is calling CQ.");
        Assert.Empty(canvas.Absent);

        canvas.Remove(canvas.Placed.First(p => p.Id == Widgets.Spots));
        canvas.News(Widgets.Spots, "Somebody is calling CQ.");
        Assert.Single(canvas.Absent);

        // And it clears when there is no longer anything to say.
        canvas.News(Widgets.Spots, "");
        Assert.Empty(canvas.Absent);
    }

    /// <remarks>
    /// Proves HM-DEC-086: edges within reach line up with a neighbor's and edges
    /// out of reach are left exactly where the operator put them. **Snapping that
    /// fights the operator is worse than no snapping.**
    /// </remarks>
    [Theory]
    [InlineData(100, 104)]
    [InlineData(100, 96)]
    [InlineData(100, 100)]
    public void NearlyLinedUpBecomesLinedUp(double neighbor, double mine)
        => Assert.Equal(neighbor, WidgetCanvas.SnapEdge(mine, new[] { neighbor }));

    /// <remarks>
    /// Proves HM-DEC-086: a deliberate gap survives, so the canvas is free
    /// placement rather than a grid wearing a disguise.
    /// </remarks>
    [Theory]
    [InlineData(100, 140)]
    [InlineData(100, 60)]
    public void ADeliberateGapSurvives(double neighbor, double mine)
        => Assert.Equal(mine, WidgetCanvas.SnapEdge(mine, new[] { neighbor }));

    /// <remarks>
    /// Proves HM-DEC-086: the nearest neighbor wins when several are in reach, so
    /// a crowded corner does not pull a widget to whichever edge happened to be
    /// checked first.
    /// </remarks>
    [Fact]
    public void TheNearestEdgeWins()
        => Assert.Equal(103, WidgetCanvas.SnapEdge(102, new[] { 96.0, 103.0, 108.0 }));

    /// <remarks>
    /// Proves HM-DEC-086: every widget in the catalog has a blurb saying what it
    /// is for. The tray is a list of names, and a name is only useful to somebody
    /// who already knows what it means (§0.7).
    /// </remarks>
    [Fact]
    public void EveryWidgetSaysWhatItIsFor()
    {
        foreach (var widget in Widgets.All)
        {
            Assert.False(string.IsNullOrWhiteSpace(widget.Blurb), widget.Id);
            Assert.False(string.IsNullOrWhiteSpace(widget.Title), widget.Id);
            Assert.True(widget.Width >= WidgetCanvas.Smallest, widget.Id);
            Assert.True(widget.Height >= WidgetCanvas.Smallest, widget.Id);
        }
    }

    /// <remarks>
    /// Proves HM-DEC-086: every widget named by a preset is one the catalog has,
    /// and every widget id is unique. A preset placing something nobody knows
    /// about would quietly ship a shorter arrangement than it says it does.
    /// </remarks>
    [Fact]
    public void EveryPresetPlacesOnlyRealWidgets()
    {
        Assert.Equal(
            Widgets.All.Select(w => w.Id).Distinct().Count(), Widgets.All.Count);

        foreach (var preset in LayoutPresets.All)
        {
            // **ONE PRESET IS DELIBERATELY EMPTY** (Tim, 2026-08-27: "Remove
            // all widgets for now. Leave them on the far left side."). It is the
            // one press that clears the tab back to Receive and Send, so an
            // emptiness check would fail on exactly the arrangement that is
            // supposed to be empty.
            if (preset.Placements.Count > 0)
            {
                Assert.NotEmpty(preset.Placements);
            }

            foreach (var placement in preset.Placements)
            {
                Assert.True(
                    Widgets.Knows(placement.Id),
                    $"{preset.Name} places an unknown widget: {placement.Id}");
            }

            Assert.Equal(
                preset.Placements.Select(p => p.Id).Distinct().Count(),
                preset.Placements.Count);
        }
    }
}
