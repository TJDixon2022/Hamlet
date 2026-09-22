using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.VisualTree;
using Hamlet.App.Controls;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Explore;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// **The favorites row is chips** - work instruction 390 task 2, refining criterion 10.6 (Tim,
/// 2026-09-22: *"Favorites looks boring! Sex it up"*).
/// </summary>
/// <remarks>
/// <para>The drop-down unit 388 built was a grey button with a caret. The row under the green zone
/// is now one chip per saved spot, in the band pills' style: the spot's frequency and mode in its
/// family's ink, the name beside it, one click tunes, a ✕ on hover forgets it, and an empty row
/// says how to fill it.</para>
/// <para>**EVERY PRESS IS A REAL PRESS**, with a headless pointer where the window laid the chip
/// out; nothing writes a property a control owns. **NOTHING IS KEYED AND NO PORT IS OPENED**
/// (FACT-004).</para>
/// </remarks>
public sealed class TheFavoritesAreChipsTests
{
    /// <summary>The empty row's sentence.</summary>
    public const string EmptySentence = "no spots saved yet - press ☆ to keep this one";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where every chip is printed.</param>
    public TheFavoritesAreChipsTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// **Three saved spots render as three chips with the right words and colors**, under the green
    /// block and inside the neighborhood card.
    /// </summary>
    [AvaloniaFact]
    public void ThreeSavedSpotsAreThreeChipsWithTheirWordsAndTheirFamilysInk()
    {
        var settings = WithThree();
        var window = TheTopRowTests.Realized(1920, TheTopRowTests.WindowHeight, settings);

        try
        {
            var chips = Chips(window);

            Assert.Equal(3, chips.Count);
            Assert.False(Empty(window).IsEffectivelyVisible, "the empty sentence shows with three spots saved");

            var expected = new[]
            {
                ("14.074 USB-D", "the FT8 watering hole", ModeFamily.Digital),
                ("14.050 CW", "slow CW on Sunday", ModeFamily.Cw),
                ("14.250 USB", "the Tuesday net", ModeFamily.Phone),
            };

            for (var i = 0; i < 3; i++)
            {
                var spot = Part(chips[i], "FavoriteSpot");
                var name = Part(chips[i], "FavoriteName");

                _output.WriteLine(
                    "chip " + i + ": [" + spot.Text + "] [" + name.Text + "] ink " + spot.Foreground
                    + " at " + Box(TheTopRowTests.RectIn(chips[i], window)));

                Assert.Equal(expected[i].Item1, spot.Text);
                Assert.Equal(expected[i].Item2, name.Text);
                Assert.Same(ModePalette.For(expected[i].Item3).InkBrush, spot.Foreground);
                Assert.Equal(new Cursor(StandardCursorType.Hand).ToString(), chips[i].Cursor?.ToString());
            }

            // **WHERE THEY ARE**: in one row under the green block, the row's window on them wholly
            // inside the neighborhood card, and the first chip wholly in view. Chips past the row's
            // right end scroll sideways and are clipped, never drawn over the card's edge; the
            // headless font runs about 10 px a character, so fewer fit here than on the screen.
            var card = TheTopRowTests.RectIn(TheTopRowTests.Card(window), window);
            var block = TheTopRowTests.RectIn(TheTopRowTests.Named<Border>(window, "GreenZoneBlock"), window);
            var view = TheTopRowTests.RectIn(
                TheTopRowTests.Named<Grid>(window, "GreenZoneFavoritesRow").GetVisualDescendants().OfType<ScrollViewer>().First(),
                window);

            _output.WriteLine("the row's window on the chips " + Box(view) + " in the card " + Box(card));

            Assert.True(card.Contains(view), "the chip row " + Box(view) + " is not inside the neighborhood card " + Box(card));
            Assert.True(view.Contains(TheTopRowTests.RectIn(chips[0], window)), "the first chip is not wholly in view");

            foreach (var chip in chips)
            {
                var at = TheTopRowTests.RectIn(chip, window);

                Assert.True(at.Top >= block.Bottom - 0.5, "a chip " + Box(at) + " is not under the green block " + Box(block));
                Assert.True(at.Top >= view.Top - 0.5 && at.Bottom <= view.Bottom + 0.5, "a chip " + Box(at) + " is not in the one row " + Box(view));
            }
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>
    /// **A click tunes, and the star on the rig face fills on a saved spot**; the ✕ forgets it
    /// and the list persists without it.
    /// </summary>
    [AvaloniaFact]
    public void AClickTunesTheStarFillsAndTheCrossForgetsAndThatPersists()
    {
        var settings = WithThree();
        var window = TheTopRowTests.Realized(1920, TheTopRowTests.WindowHeight, settings);

        try
        {
            var model = (MainWindowViewModel)window.DataContext!;
            var rig = window.GetVisualDescendants().OfType<RigDisplayControl>().Single();

            // The fixture opens on 14.074, which is the first saved spot, so the dial is moved off
            // every saved spot before the star is read.
            model.FrequencyHz = 14_060_000;
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            Assert.False(rig.IsFavorite, "the star is filled before the dial is on a saved spot");

            Click(window, Chips(window)[1]);

            _output.WriteLine("after one click on the CW chip the dial reads "
                + model.FrequencyHz.ToString("#,0", CultureInfo.InvariantCulture) + " Hz, star filled " + rig.IsFavorite);

            Assert.Equal(14_050_000, model.FrequencyHz);
            Assert.True(rig.IsFavorite, "the star on the rig face is not filled on a saved spot");

            // **THE ✕ IS ON HOVER**: hidden at rest, shown while the pointer is on its chip.
            var third = Chips(window)[2];
            var cross = Part<Button>(third, "FavoriteForget");

            Assert.Equal(0, cross.Opacity);

            Hover(window, third);

            _output.WriteLine("hovering the net's chip at " + Box(TheTopRowTests.RectIn(third, window))
                + ": pointer over it " + third.IsPointerOver + ", the cross's opacity " + cross.Opacity);

            Assert.Equal(1, cross.Opacity);

            Click(window, cross);

            var left = Chips(window).Select(c => Part(c, "FavoriteName").Text).ToList();

            _output.WriteLine("after the cross on the net: " + string.Join(" | ", left)
                + "; settings hold " + settings.Favorites.Count);

            Assert.Equal(new[] { "the FT8 watering hole", "slow CW on Sunday" }, left);
            Assert.Equal(2, model.Favorites.Count);

            // **AND IT PERSISTS**: the settings the window was built from hold two, and a fresh
            // window built from them shows two chips.
            Assert.Equal(new long[] { 14_074_000, 14_050_000 }, settings.Favorites.Select(f => f.FrequencyHz));
            Assert.Equal(14_050_000, model.FrequencyHz);

            var again = TheTopRowTests.Realized(1920, TheTopRowTests.WindowHeight, settings);

            try
            {
                Assert.Equal(2, Chips(again).Count);
            }
            finally
            {
                again.Close();
            }
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>**Empty, the row says how to fill it**, and no chip and no drop-down is drawn.</summary>
    [AvaloniaFact]
    public void EmptyTheRowSaysHowToKeepASpot()
    {
        var window = TheTopRowTests.Realized(1920, TheTopRowTests.WindowHeight, TheTopRowTests.FixtureSettings());

        try
        {
            var empty = Empty(window);

            _output.WriteLine("empty row reads [" + empty.Text + "], visible " + empty.IsEffectivelyVisible);

            Assert.Empty(Chips(window));
            Assert.True(empty.IsEffectivelyVisible);
            Assert.Equal(EmptySentence, empty.Text);
            Assert.Empty(window.GetVisualDescendants().OfType<FavoritesDropDownControl>());
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>**The band no taller with three chips** - 214 stands, at 1920 and at 1400.</summary>
    [AvaloniaFact]
    public void ThreeChipsCostTheTopBandNothing()
    {
        var misses = new List<string>();

        foreach (var width in new[] { 1920.0, 1400.0 })
        {
            var window = TheTopRowTests.Realized(width, TheTopRowTests.WindowHeight, WithThree());

            try
            {
                Unit376TheTopBandTests.Pump(window);

                var band = Unit376TheTopBandTests.Band(window);

                _output.WriteLine(Px(width) + ": the band is " + Px(band.WithPills) + " px with the pills, three chips saved");

                if (band.WithPills > Unit376TheTopBandTests.BandReachedWithThePills + 0.5)
                {
                    misses.Add(Px(width) + ": the band is " + Px(band.WithPills) + " px, over "
                        + Px(Unit376TheTopBandTests.BandReachedWithThePills));
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

    private static AppSettings WithThree()
    {
        var settings = TheTopRowTests.FixtureSettings();
        var at = new DateTime(2026, 9, 22, 20, 0, 0, DateTimeKind.Utc);

        settings.Favorites.Add(new SavedFavorite { FrequencyHz = 14_074_000, Name = "the FT8 watering hole", Mode = "USB-D", BandName = "20 m", SavedUtc = at });
        settings.Favorites.Add(new SavedFavorite { FrequencyHz = 14_050_000, Name = "slow CW on Sunday", Mode = "CW", BandName = "20 m", SavedUtc = at });
        settings.Favorites.Add(new SavedFavorite { FrequencyHz = 14_250_000, Name = "the Tuesday net", Mode = "USB", BandName = "20 m", SavedUtc = at });

        return settings;
    }

    private static List<Button> Chips(Window window)
        => TheTopRowTests.Named<ItemsControl>(window, "GreenZoneFavorites")
            .GetVisualDescendants().OfType<Button>()
            .Where(b => b.Classes.Contains("hm-favchip"))
            .ToList();

    private static TextBlock Empty(Window window)
        => TheTopRowTests.Named<TextBlock>(window, "GreenZoneFavoritesEmpty");

    private static TextBlock Part(Control chip, string name) => Part<TextBlock>(chip, name);

    private static T Part<T>(Control chip, string name)
        where T : Control
        => chip.GetVisualDescendants().OfType<T>().Single(c => c.Name == name);

    private static void Hover(Window window, Control control)
    {
        InView(window, control);

        var at = TheTopRowTests.RectIn(control, window);

        // **OFF, THEN ON**: the headless mouse is one device across every window of the session, and
        // a move to where it already is from the last window raises no enter.
        Avalonia.Headless.HeadlessWindowExtensions.MouseMove(window, new Point(1, 1));
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        Avalonia.Headless.HeadlessWindowExtensions.MouseMove(window, at.Center);
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
    }

    /// <summary>
    /// Scrolls the row until the control is in view, as the operator would before pressing it -
    /// a request to the row's scroller, not a write to anything the control owns.
    /// </summary>
    private static void InView(Window window, Control control)
    {
        var chip = control as Button is { } b && b.Classes.Contains("hm-favchip")
            ? control
            : control.GetVisualAncestors().OfType<Button>().FirstOrDefault(a => a.Classes.Contains("hm-favchip")) ?? control;

        chip.BringIntoView();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
    }

    private static void Click(Window window, Control control)
    {
        InView(window, control);

        var at = TheTopRowTests.RectIn(control, window).Center;

        Avalonia.Headless.HeadlessWindowExtensions.MouseMove(window, at);
        Avalonia.Headless.HeadlessWindowExtensions.MouseDown(window, at, MouseButton.Left);
        Avalonia.Headless.HeadlessWindowExtensions.MouseUp(window, at, MouseButton.Left);

        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
    }

    private static string Px(double value)
        => value.ToString("0.#", CultureInfo.InvariantCulture);

    private static string Box(Rect r)
        => "[" + Px(r.X) + "," + Px(r.Y) + " " + Px(r.Width) + "x" + Px(r.Height) + "]";
}
