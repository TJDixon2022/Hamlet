using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.VisualTree;
using Hamlet.App.Controls;
using Hamlet.App.ViewModels;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// **Criterion 10.6: favorites are one drop-down under the green zone, the star on the rig face
/// keeps saving, and the caret beside the star is gone** - PHASE_PLAN.md step 10 rev7, work
/// instruction 388 task 2.
/// </summary>
/// <remarks>
/// <para>**REWRITTEN UNDER R12 AND WORK INSTRUCTION 388 SECTION 6 RULING 2, AND RENAMED.** This
/// type was unit 387's <c>TheFavoritesAreBackOnTheRigDisplayTests</c>, asserting criterion 10.1's
/// caret beside the star. Tim replaced what it asserted at `752a9b62` (rev7, 2026-09-22, marked on
/// his screen): *favorites are a drop-down in the empty row under the green zone, inside the
/// neighborhood card, the way Hamlet had them before 2026-08-27 ... the caret unit 387 added
/// beside the star is removed.* So its name was false and it is renamed; the file keeps its path
/// because the session could not move a file (`git mv` and `rm` are refused), which is reported.
/// The carry-forward line changed by name in the same commit.</para>
/// <para>**ONE FOR ONE.** The star-saving name is **unedited**, because 10.6 says the star keeps
/// saving. The caret's nine-size name now asserts the star at nine sizes **and no caret anywhere
/// on the rig face**; the list-opening name asserts the drop-down opening the named spots and one
/// click tuning; the empty name asserts the same note and <c>Manage favorites…</c>; and the band
/// name asserts the band did not grow by a pixel for the drop-down.</para>
/// <para>**EVERY PRESS IS A REAL PRESS.** Nothing here writes a property a control owns
/// (`ViewTestsActThroughControlsTests.NoViewTestWritesAPropertyAControlOwns`): the star and the
/// drop-down are hit with a headless pointer where the window laid them out.</para>
/// <para>**NOTHING IS KEYED AND NO PORT IS OPENED** (FACT-004).</para>
/// </remarks>
public sealed class TheFavoritesAreUnderTheGreenZoneTests
{
    /// <summary>Unit 354's nine sizes, as `TheStopIsAlwaysOnScreenTests` lists them.</summary>
    public static readonly (double Width, double Height)[] NineSizes =
    {
        (1920, 1040), (900, 620), (1100, 780), (1280, 720), (1366, 728),
        (1536, 824), (1400, 1040), (1920, 1017), (2560, 1400),
    };

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the guard.</summary>
    /// <param name="output">Where every rectangle is printed.</param>
    public TheFavoritesAreUnderTheGreenZoneTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// **The star is drawn and hittable at every one of unit 354's nine sizes, and nothing beside
    /// it opens a list** - no caret drawn and none hittable anywhere on the rig face.
    /// </summary>
    /// <remarks>
    /// <para>**THE CARET IS ASSERTED ABSENT THREE WAYS.** The control carries no member that could
    /// hold a list or its hit rectangle; its source draws no caret glyph; and a press at every
    /// 6 px across the whole status strip, outside the star's own rectangle, saves nothing and
    /// removes nothing - so no target is left there whatever is painted.</para>
    /// </remarks>
    [AvaloniaFact]
    public void TheStarIsDrawnAndHittableAtAllNineSizesAndNoCaretIsAnywhereOnTheRigFace()
    {
        var misses = new List<string>();

        foreach (var member in new[] { "_listRect", "Favorites", "ManageFavoritesCommand", "SavedListUnderThePointer", "OpenTheSavedList" })
        {
            var found = typeof(RigDisplayControl).GetMember(
                member, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

            if (found.Length > 0)
            {
                misses.Add("RigDisplayControl still carries " + member + ", which only the caret needed.");
            }
        }

        var source = File.ReadAllText(Path.Combine(RepoRoot(), "src", "Hamlet.App", "Controls", "RigDisplayControl.cs"));

        if (source.Contains('▾'))
        {
            misses.Add("RigDisplayControl.cs still draws the caret glyph ▾.");
        }

        foreach (var (width, height) in NineSizes)
        {
            var window = TheTopRowTests.Realized(width, height, null, null);

            try
            {
                var model = (MainWindowViewModel)window.DataContext!;
                var rig = Rig(window);

                Paint(window, rig);

                var star = StarRect(rig);
                var where = Px(width) + " x " + Px(height);

                _output.WriteLine(
                    where.PadRight(14) + " face " + Px(rig.Bounds.Width) + " x " + Px(rig.Bounds.Height)
                    + " | star " + Box(star));

                if (star == default)
                {
                    misses.Add(where + ": the star's hit rectangle is zero, so the star cannot be pressed.");

                    continue;
                }

                // **THE SWEEP**: every 6 px across the status strip, at the star's own height,
                // everywhere the star is not.
                var pressed = 0;

                for (var x = 2.0; x < rig.Bounds.Width - 2; x += 6)
                {
                    var at = new Rect(x, star.Center.Y, 0.001, 0.001);

                    if (star.Contains(at.TopLeft))
                    {
                        continue;
                    }

                    PressAt(window, rig, at.TopLeft);
                    pressed++;
                }

                if (model.Favorites.Count != 0 || model.IsFavorite)
                {
                    misses.Add(
                        where + ": " + pressed + " presses across the strip outside the star changed the"
                        + " saved list to " + model.Favorites.Count + " - something beside the star is still a target.");
                }
            }
            finally
            {
                window.Close();
            }
        }

        Assert.True(misses.Count == 0, string.Join(Environment.NewLine, misses));
    }

    /// <summary>
    /// **Pressing the star saves the dial and the mode with a name, and pressing it again takes
    /// it off** - and the name is Hamlet's own, built from where the dial is.
    /// </summary>
    [AvaloniaFact]
    public void PressingTheStarSavesTheDialAndTheModeWithANameAndPressingItAgainRemovesIt()
    {
        var window = TheTopRowTests.Realized(1920, 1040, null, null);

        try
        {
            var model = (MainWindowViewModel)window.DataContext!;
            var rig = Rig(window);

            Paint(window, rig);

            Assert.Empty(model.Favorites);

            Press(window, rig, StarRect(rig));

            var saved = Assert.Single(model.Favorites);

            _output.WriteLine(
                "after one press on the star: " + saved.FrequencyHz.ToString("#,0", CultureInfo.InvariantCulture)
                + " Hz, mode [" + saved.Mode + "], band [" + saved.BandName + "], name [" + saved.Name + "]");

            Assert.Equal(model.FrequencyHz, saved.FrequencyHz);
            Assert.Equal(model.RigModeText, saved.Mode);
            Assert.False(string.IsNullOrWhiteSpace(saved.Name));
            Assert.True(model.IsFavorite);
            Assert.True(model.HasFavorites);
            Assert.Single(model.FavoriteMenu);

            Paint(window, rig);

            Press(window, rig, StarRect(rig));

            Assert.Empty(model.Favorites);
            Assert.False(model.IsFavorite);
            Assert.False(model.HasFavorites);
            Assert.Empty(model.FavoriteMenu);
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>
    /// **The drop-down under the green zone opens the named spots, one click tunes, and the
    /// control reads *Favorites* again afterwards** - at 1920 and at 1400.
    /// </summary>
    /// <remarks>
    /// <para>The list is the view model's own `FavoriteMenu` - the same list the Radio menu shows -
    /// asserted by identity, so nothing about a favorite is decided in two places.</para>
    /// <para>**WHERE IT IS**: inside the neighborhood card, under the green block, and wholly on
    /// the card - the row Tim marked.</para>
    /// </remarks>
    [AvaloniaFact]
    public void TheDropDownUnderTheGreenZoneOpensTheNamedSpotsAndOneClickTunes()
    {
        foreach (var width in new[] { 1920.0, 1400.0 })
        {
            var window = TheTopRowTests.Realized(width, TheTopRowTests.WindowHeight, null, null);

            try
            {
                var model = (MainWindowViewModel)window.DataContext!;
                var rig = Rig(window);
                var dropDown = DropDown(window);

                Assert.Same(model.FavoriteMenu, dropDown.Favorites);

                // Two saved places, put there through the star rather than by hand: the first
                // where the dial already is, the second 2 kHz up.
                Paint(window, rig);
                Press(window, rig, StarRect(rig));

                var first = model.Favorites[0].FrequencyHz;

                model.FrequencyHz = first + 2_000;

                Paint(window, rig);
                Press(window, rig, StarRect(rig));

                Assert.Equal(2, model.Favorites.Count);

                AssertItIsUnderTheGreenZone(window, dropDown, Px(width));

                Click(window, dropDown);

                var flyout = dropDown.ListShown;

                Assert.NotNull(flyout);

                var items = flyout!.Items.OfType<MenuItem>().ToList();

                foreach (var item in items)
                {
                    _output.WriteLine(Px(width) + ": the drop-down offers: " + item.Header);
                }

                // One line per saved place, plus `Manage favorites…`.
                Assert.Equal(model.FavoriteMenu.Count + 1, items.Count);

                for (var i = 0; i < model.FavoriteMenu.Count; i++)
                {
                    Assert.Equal(model.FavoriteMenu[i].Label, items[i].Header);
                    Assert.Same(model.FavoriteMenu[i].Tune, items[i].Command);
                }

                Assert.Equal("Manage favorites…", items[^1].Header);
                Assert.Same(model.ManageFavoritesCommand, items[^1].Command);

                // **ONE CLICK TUNES.** The dial is moved somewhere neither favorite is, and the
                // first line is clicked exactly as a mouse would click it - no second click, no
                // confirm.
                model.FrequencyHz = first + 20_000;

                var line = items[0];

                Assert.True(line.Command!.CanExecute(line.CommandParameter));

                line.Command.Execute(line.CommandParameter);

                Assert.Equal(first, model.FrequencyHz);

                // **AND IT READS FAVORITES AGAIN**, holding no selection that stops being true the
                // moment the dial moves (§0.0).
                Assert.Equal("Favorites", dropDown.Content);

                _output.WriteLine(
                    Px(width) + ": one click on [" + line.Header + "] put the dial back on "
                    + model.FrequencyHz.ToString("#,0", CultureInfo.InvariantCulture)
                    + " Hz from " + (first + 20_000).ToString("#,0", CultureInfo.InvariantCulture)
                    + ", and the control reads [" + dropDown.Content + "]");
            }
            finally
            {
                CloseWithTheListShut(window);
            }
        }
    }

    /// <summary>
    /// **With nothing saved the drop-down is still there, still enabled, and opens to a note** -
    /// not an empty box, not a disabled item and never a greyed control.
    /// </summary>
    /// <remarks>
    /// The 2026-09-06 rule and §0.5.1: nothing is greyed, hidden, sorted away or disabled, and an
    /// entry with no command is a note that cannot be hit. The note is the one the rig face's list
    /// offered, reused and not reworded.
    /// </remarks>
    [AvaloniaFact]
    public void WithNothingSavedTheListSaysSoRatherThanOpeningEmpty()
    {
        var window = TheTopRowTests.Realized(1920, 1040, null, null);

        try
        {
            var model = (MainWindowViewModel)window.DataContext!;
            var dropDown = DropDown(window);

            Assert.Empty(model.Favorites);
            Assert.True(dropDown.IsEffectivelyVisible, "the drop-down is hidden with nothing saved");
            Assert.True(dropDown.IsEffectivelyEnabled, "the drop-down is greyed with nothing saved");

            Click(window, dropDown);

            var flyout = dropDown.ListShown;

            Assert.NotNull(flyout);

            var items = flyout!.Items.OfType<MenuItem>().ToList();

            foreach (var item in items)
            {
                _output.WriteLine("  the empty list offers: [" + item.Header + "] hittable " + item.IsHitTestVisible);
            }

            Assert.Equal(2, items.Count);
            Assert.Equal("Nothing saved here yet - press the star to save where you are.", items[0].Header);
            Assert.Null(items[0].Command);
            Assert.False(items[0].IsHitTestVisible);
            Assert.Equal("Manage favorites…", items[^1].Header);
            Assert.NotNull(items[^1].Command);
            Assert.Equal("Favorites", dropDown.Content);
        }
        finally
        {
            CloseWithTheListShut(window);
        }
    }

    /// <summary>
    /// **The band did not grow to make room for the drop-down** - rev7's *214 px stands*, and the
    /// ratchet unit 376 took.
    /// </summary>
    /// <remarks>
    /// Task 1 measured the row under the green zone empty - 55 px at 1920 and 42 at 1400 - which is
    /// why the drop-down can sit there at no cost; this is where that is held.
    /// </remarks>
    [AvaloniaFact]
    public void TheWayBackInCostTheTopBandNothing()
    {
        var misses = new List<string>();

        foreach (var width in new[] { 1920.0, 1400.0 })
        {
            var window = TheTopRowTests.Realized(width, TheTopRowTests.WindowHeight, null, null);

            try
            {
                Unit376TheTopBandTests.Pump(window);

                var band = Unit376TheTopBandTests.Band(window);
                var dropDown = TheTopRowTests.RectIn(DropDown(window), window);

                _output.WriteLine(
                    Px(width) + ": the band is " + Px(band.WithPills) + " px with the pills and "
                    + Px(band.WithoutPills) + " without, against the ratchet of "
                    + Px(Unit376TheTopBandTests.BandReachedWithThePills) + "; the drop-down is at "
                    + Box(dropDown));

                if (band.WithPills > Unit376TheTopBandTests.BandReachedWithThePills + 0.5)
                {
                    misses.Add(
                        Px(width) + ": the band is " + Px(band.WithPills)
                        + " px with the pills where unit 376 brought it to "
                        + Px(Unit376TheTopBandTests.BandReachedWithThePills)
                        + ". Height has gone back into the top row to make room for the favorites drop-down.");
                }
            }
            finally
            {
                window.Close();
            }
        }

        Assert.True(misses.Count == 0, string.Join(Environment.NewLine, misses));
    }

    // -------------------------------------------------------------------------------------

    private static RigDisplayControl Rig(Window window)
        => window.GetVisualDescendants().OfType<RigDisplayControl>().Single();

    private static FavoritesDropDownControl DropDown(Window window)
        => window.GetVisualDescendants().OfType<FavoritesDropDownControl>().Single();

    /// <summary>
    /// Shuts the list a press opened, then closes the window - so no popup outlives the window it
    /// was opened from into the next test of the session.
    /// </summary>
    /// <param name="window">The window.</param>
    private static void CloseWithTheListShut(Window window)
    {
        foreach (var dropDown in window.GetVisualDescendants().OfType<FavoritesDropDownControl>())
        {
            dropDown.ListShown?.Hide();
        }

        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        window.Close();
    }

    private static void AssertItIsUnderTheGreenZone(Window window, Control dropDown, string where)
    {
        var card = TheTopRowTests.RectIn(TheTopRowTests.Card(window), window);
        var block = TheTopRowTests.RectIn(TheTopRowTests.Named<Border>(window, "GreenZoneBlock"), window);
        var at = TheTopRowTests.RectIn(dropDown, window);

        Assert.True(dropDown.IsEffectivelyVisible, where + ": the drop-down is not drawn");
        Assert.True(
            at.Top >= block.Bottom - 0.5 && at.Left >= block.Left - 0.5 && at.Right <= block.Right + 0.5,
            where + ": the drop-down " + Box(at) + " is not under the green block " + Box(block));
        Assert.True(
            card.Contains(at),
            where + ": the drop-down " + Box(at) + " is not wholly inside the neighborhood card " + Box(card));
    }

    /// <summary>
    /// Runs the control's own `Render` over its own laid-out bounds, so what it drew and the hit
    /// rectangles it left behind can be read.
    /// </summary>
    /// <remarks>
    /// **THE HEADLESS WINDOW IS NEVER PAINTED** - `CaptureRenderedFrame` refuses without Skia - so
    /// the control is drawn into an off-screen bitmap **at the size the window's own layout gave
    /// it**. The size is the product's, measured; only the surface is the test's.
    /// </remarks>
    /// <param name="window">The realized window.</param>
    /// <param name="control">The control being measured.</param>
    private static void Paint(Window window, Control control)
    {
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        var size = new PixelSize(
            Math.Max(1, (int)Math.Ceiling(control.Bounds.Width)),
            Math.Max(1, (int)Math.Ceiling(control.Bounds.Height)));

        using var surface = new Avalonia.Media.Imaging.RenderTargetBitmap(size);
        using var context = surface.CreateDrawingContext();

        control.Render(context);
    }

    /// <summary>
    /// **Presses the control where it drew the thing**, with a headless pointer, through the
    /// control's own `OnPointerPressed`. Nothing the control owns is written.
    /// </summary>
    /// <param name="window">The window, for the pointer's frame.</param>
    /// <param name="rig">The rig face.</param>
    /// <param name="target">The rectangle to press, in the control's own frame.</param>
    private static void Press(Window window, RigDisplayControl rig, Rect target)
    {
        Assert.True(target != default, "the target was not drawn, so there is nothing to press");

        PressAt(window, rig, target.Center);
    }

    private static void PressAt(Window window, Control control, Point inControl)
    {
        var inWindow = TheTopRowTests.RectIn(control, window);
        var at = new Point(inWindow.X + inControl.X, inWindow.Y + inControl.Y);

        // **THE POINTER ARRIVES BEFORE IT PRESSES**, as a mouse does and as
        // `TheStopIsAlwaysOnScreenTests.Press` does: the headless mouse is one device across every
        // window of the session, and without the move a press can land on the last window's
        // pointer-over state rather than this one's.
        Avalonia.Headless.HeadlessWindowExtensions.MouseMove(window, at);
        Avalonia.Headless.HeadlessWindowExtensions.MouseDown(window, at, MouseButton.Left);
        Avalonia.Headless.HeadlessWindowExtensions.MouseUp(window, at, MouseButton.Left);

        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
    }

    /// <summary>Clicks a control at its centre with a headless pointer, as a mouse would.</summary>
    /// <param name="window">The window, for the pointer's frame.</param>
    /// <param name="control">The control.</param>
    private static void Click(Window window, Control control)
    {
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        PressAt(window, control, new Point(control.Bounds.Width / 2, control.Bounds.Height / 2));
    }

    /// <summary>Reads the star's hit rectangle. It writes nothing.</summary>
    /// <param name="rig">The control.</param>
    /// <returns>The rectangle, `default` where the bail fired.</returns>
    private static Rect StarRect(RigDisplayControl rig) => Read(rig, "_starRect");

    private static Rect Read(RigDisplayControl rig, string field)
    {
        var found = typeof(RigDisplayControl)
            .GetField(field, BindingFlags.NonPublic | BindingFlags.Instance);

        return found is null ? default : (Rect)found.GetValue(rig)!;
    }

    private static string RepoRoot()
    {
        var at = new DirectoryInfo(AppContext.BaseDirectory);

        while (at is not null && !File.Exists(Path.Combine(at.FullName, "Hamlet.sln")))
        {
            at = at.Parent;
        }

        return at?.FullName
            ?? throw new InvalidOperationException("no Hamlet.sln above the test binary");
    }

    private static string Px(double value)
        => value.ToString("0.#", CultureInfo.InvariantCulture);

    private static string Box(Rect r)
        => "[" + Px(r.X) + "," + Px(r.Y) + " " + Px(r.Width) + "x" + Px(r.Height) + "]";
}
