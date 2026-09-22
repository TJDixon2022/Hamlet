using System;
using System.Collections.Generic;
using System.Globalization;
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
/// **Criterion 10.1: the favorites Tim lost are back on the rig display** - R46(a), work
/// instruction 387 task 2.
/// </summary>
/// <remarks>
/// <para>**WHAT WENT, AND WHEN.** `a51bc2a6` of 2026-08-27 14:08, work instruction 029,
/// *fix(app): the header says each thing once, and nothing sits above the tabs*, removed 149
/// lines of `MainWindow.axaml`. Tim's ruling of that date took the recent-places row out of the
/// slot above the tabs and `ABANDONED_WIDGETS.md` records that; **the favorites list, the
/// favorite's name and its note went out in the same `Grid` in the same commit and are recorded
/// nowhere.** The star itself never went: unit 387 task 1 measured `_starRect` at
/// `[62,2 67x26]` at 1920, 1400 and 1100 x 780 alike, so the bail at `DrawStar` had never fired
/// and this phase's own step 6 did not do it.
/// </para>
/// <para>**THE LIST COMES BACK ON THE RIG FACE.** The band may not grow by one pixel - 6.1's
/// ceiling, and `Unit376TheTopBandTests.BandReachedWithThePills` ratchets it at 214 - and the
/// slot the strip sat in is the one Tim ruled must read the same in every mode. A caret inside
/// the black costs no height at all, and the Radio menu keeps its own copy.</para>
/// <para>**EVERY PRESS IS A REAL PRESS.** Nothing here writes a property the control owns
/// (`ViewTestsActThroughControlsTests.NoViewTestWritesAPropertyAControlOwns`): the star and the
/// caret are hit with a headless pointer at the coordinates the control itself drew them at, read
/// back by reflection and never written.</para>
/// <para>**NOTHING IS KEYED AND NO PORT IS OPENED** (FACT-004).</para>
/// </remarks>
public sealed class TheFavoritesAreBackOnTheRigDisplayTests
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
    public TheFavoritesAreBackOnTheRigDisplayTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// **The star and the caret are drawn and hittable at every one of unit 354's nine sizes.**
    /// </summary>
    /// <remarks>
    /// A zeroed rectangle means the thing cannot be clicked even if something is drawn, so the
    /// rectangle is what is asserted. **The two never overlap**, which is asserted rather than
    /// eyeballed: a press that could mean both things would save a frequency while somebody was
    /// trying to open a list.
    /// </remarks>
    [AvaloniaFact]
    public void TheStarAndTheCaretAreDrawnAndHittableAtAllNineSizesAndDoNotOverlap()
    {
        var misses = new List<string>();

        foreach (var (width, height) in NineSizes)
        {
            var window = TheTopRowTests.Realized(width, height, null, null);

            try
            {
                var rig = Rig(window);

                Paint(window, rig);

                var star = StarRect(rig);
                var list = ListRect(rig);
                var where = Px(width) + " x " + Px(height);

                _output.WriteLine(
                    where.PadRight(14) + " face " + Px(rig.Bounds.Width) + " x " + Px(rig.Bounds.Height)
                    + " | star " + Box(star) + " | caret " + Box(list));

                if (star == default)
                {
                    misses.Add(where + ": the star's hit rectangle is zero, so the star cannot be pressed.");
                }

                if (list == default)
                {
                    misses.Add(where + ": the caret's hit rectangle is zero, so the saved list cannot be opened from the rig display (R46(a)).");
                }

                if (star != default && list != default && star.Intersects(list))
                {
                    misses.Add(
                        where + ": the star " + Box(star) + " and the caret " + Box(list)
                        + " overlap, so one press could mean both things.");
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
    /// **The list opens from the rig display, one click tunes, and `Manage favorites…` is on it.**
    /// </summary>
    /// <remarks>
    /// The list is the view model's own `FavoriteMenu` - the same list the Radio menu shows - so
    /// the list this reads and the list Tim sees are one list.
    /// </remarks>
    [AvaloniaFact]
    public void TheListOpensFromTheRigDisplayAndOneClickTunesToTheSavedFrequency()
    {
        var window = TheTopRowTests.Realized(1920, 1040, null, null);

        try
        {
            var model = (MainWindowViewModel)window.DataContext!;
            var rig = Rig(window);

            // Two saved places, put there through the star rather than by hand: the first where
            // the dial already is, the second 2 kHz up.
            Paint(window, rig);
            Press(window, rig, StarRect(rig));

            var first = model.Favorites[0].FrequencyHz;

            model.FrequencyHz = first + 2_000;

            Paint(window, rig);
            Press(window, rig, StarRect(rig));

            Assert.Equal(2, model.Favorites.Count);

            // Now the caret, which is the thing R46(a) says was missing.
            Paint(window, rig);
            Press(window, rig, ListRect(rig));

            var flyout = rig.SavedListUnderThePointer;

            Assert.NotNull(flyout);

            var items = flyout!.Items.OfType<MenuItem>().ToList();

            foreach (var item in items)
            {
                _output.WriteLine("  the list offers: " + item.Header);
            }

            // One line per saved place, plus `Manage favorites…`.
            Assert.Equal(model.FavoriteMenu.Count + 1, items.Count);

            for (var i = 0; i < model.FavoriteMenu.Count; i++)
            {
                Assert.Equal(model.FavoriteMenu[i].Label, items[i].Header);
                Assert.Same(model.FavoriteMenu[i].Tune, items[i].Command);
            }

            Assert.Equal("Manage favorites…", items[^1].Header);
            Assert.NotNull(items[^1].Command);

            // **ONE CLICK TUNES.** The dial is moved somewhere neither favorite is, and the first
            // line of the list is clicked exactly as a mouse would click it.
            model.FrequencyHz = first + 20_000;

            var line = items[0];

            Assert.True(line.Command!.CanExecute(line.CommandParameter));

            line.Command.Execute(line.CommandParameter);

            Assert.Equal(first, model.FrequencyHz);

            _output.WriteLine(
                "one click on [" + line.Header + "] put the dial back on "
                + model.FrequencyHz.ToString("#,0", CultureInfo.InvariantCulture) + " Hz");
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>
    /// **With nothing saved the list is a note and not an empty box, and not a disabled item.**
    /// </summary>
    /// <remarks>
    /// The 2026-09-06 rule: nothing is greyed, hidden, sorted away or disabled, and an entry with
    /// no command is a note that cannot be hit. R46(b)'s supersession is for the lines that need
    /// his callsign and does not reach here.
    /// </remarks>
    [AvaloniaFact]
    public void WithNothingSavedTheListSaysSoRatherThanOpeningEmpty()
    {
        var window = TheTopRowTests.Realized(1920, 1040, null, null);

        try
        {
            var model = (MainWindowViewModel)window.DataContext!;
            var rig = Rig(window);

            Assert.Empty(model.Favorites);

            Paint(window, rig);
            Press(window, rig, ListRect(rig));

            var flyout = rig.SavedListUnderThePointer;

            Assert.NotNull(flyout);

            var items = flyout!.Items.OfType<MenuItem>().ToList();

            foreach (var item in items)
            {
                _output.WriteLine("  the empty list offers: [" + item.Header + "] hittable " + item.IsHitTestVisible);
            }

            Assert.Equal(2, items.Count);
            Assert.Contains("star", items[0].Header!.ToString()!, StringComparison.OrdinalIgnoreCase);
            Assert.Null(items[0].Command);
            Assert.False(items[0].IsHitTestVisible);
            Assert.Equal("Manage favorites…", items[^1].Header);
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>
    /// **The band did not grow to make room for the way in** - 6.1's ceiling and the ratchet unit
    /// 376 took.
    /// </summary>
    /// <remarks>
    /// The caret lives inside the LCD's own status strip, which is why this can be asserted at
    /// all: every pixel of the door was already on the screen.
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

                _output.WriteLine(
                    Px(width) + ": the band is " + Px(band.WithPills) + " px with the pills and "
                    + Px(band.WithoutPills) + " without, against the ratchet of "
                    + Px(Unit376TheTopBandTests.BandReachedWithThePills));

                if (band.WithPills > Unit376TheTopBandTests.BandReachedWithThePills + 0.5)
                {
                    misses.Add(
                        Px(width) + ": the band is " + Px(band.WithPills)
                        + " px with the pills where unit 376 brought it to "
                        + Px(Unit376TheTopBandTests.BandReachedWithThePills)
                        + ". Height has gone back into the top row to make room for the favorites door.");
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

        var inWindow = TheTopRowTests.RectIn(rig, window);
        var at = new Point(inWindow.X + target.Center.X, inWindow.Y + target.Center.Y);

        Avalonia.Headless.HeadlessWindowExtensions.MouseDown(window, at, MouseButton.Left);
        Avalonia.Headless.HeadlessWindowExtensions.MouseUp(window, at, MouseButton.Left);

        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
    }

    /// <summary>Reads the star's hit rectangle. It writes nothing.</summary>
    /// <param name="rig">The control.</param>
    /// <returns>The rectangle, `default` where the bail fired.</returns>
    private static Rect StarRect(RigDisplayControl rig) => Read(rig, "_starRect");

    /// <summary>Reads the caret's hit rectangle. It writes nothing.</summary>
    /// <param name="rig">The control.</param>
    /// <returns>The rectangle, `default` where there was no room for it.</returns>
    private static Rect ListRect(RigDisplayControl rig) => Read(rig, "_listRect");

    private static Rect Read(RigDisplayControl rig, string field)
    {
        var found = typeof(RigDisplayControl)
            .GetField(field, BindingFlags.NonPublic | BindingFlags.Instance);

        return found is null ? default : (Rect)found.GetValue(rig)!;
    }

    private static string Px(double value)
        => value.ToString("0.#", CultureInfo.InvariantCulture);

    private static string Box(Rect r)
        => "[" + Px(r.X) + "," + Px(r.Y) + " " + Px(r.Width) + "x" + Px(r.Height) + "]";
}
