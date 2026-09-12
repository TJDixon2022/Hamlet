using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.VisualTree;
using Hamlet.App.Controls;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Solar;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Work instruction 332 task 5: **the screen at 1400 and at 1920, computed and reported.**
/// </summary>
/// <remarks>
/// <para>**NOTHING HERE LOOKS AT A PIXEL.** The windows are stood up on the headless host
/// and their layouts are read back; every figure is a measurement of that layout. **The host
/// advances a flat ten pixels a character**, wider than any face on the glass, so a line that
/// wraps here may be one line on the glass and a line that fits here fits there.</para>
/// <para>**THE ASSERTIONS ARE ONLY THE ONES THAT MUST HOLD AT BOTH WIDTHS**: nothing on the
/// green zone runs past its panel, and the achievements page fits its window. Everything
/// else is printed for the report.</para>
/// </remarks>
public sealed class Unit332TwoWidthsTests
{
    private const string HisGrid = "FN00";

    private static readonly DateTime TwoPmEdt = new(2026, 9, 12, 18, 0, 0, DateTimeKind.Utc);

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the measurements are printed.</param>
    public Unit332TwoWidthsTests(ITestOutputHelper output) => _output = output;

    /// <summary>**The green zone at 1400 and at 1920.**</summary>
    [AvaloniaFact]
    public void TheGreenZoneAtFourteenHundredAndNineteenTwenty()
    {
        foreach (var width in new[] { 1400.0, 1920.0 })
        {
            var window = Realized(width);

            try
            {
                var panel = Named<TextBlock>(window, "GreenZoneLicenseLine").GetVisualAncestors()
                    .OfType<Border>()
                    .First(b => b.BorderThickness.Left > 0);

                _output.WriteLine("WINDOW " + F(width) + " - green zone panel "
                    + F(panel.Bounds.Width) + " x " + F(panel.Bounds.Height));

                foreach (var name in new[] { "GreenZoneLeft", "GreenZoneMap", "GreenZoneRight", "GreenZoneRuleOfThumb" })
                {
                    var region = Named<Control>(window, name);
                    var at = region.TranslatePoint(new Point(0, 0), panel) ?? default;

                    _output.WriteLine("  " + name.PadRight(22) + " x " + F(at.X).PadLeft(7)
                        + "  y " + F(at.Y).PadLeft(6) + "  " + F(region.Bounds.Width) + " x "
                        + F(region.Bounds.Height));
                }

                foreach (var name in new[] { "GreenZoneBand", "GreenZoneModeLine", "GreenZoneLicenseLine", "GreenZoneRuleOfThumb" })
                {
                    var text = Named<TextBlock>(window, name);

                    _output.WriteLine("  " + name.PadRight(22) + " " + Lines(text) + " line(s): " + text.Text);
                }

                // **THE PILLS CAME OFF IN WORK INSTRUCTION 334** (R21); what is left to count is
                // whether any chip is still on the panel.
                _output.WriteLine("  chips on the panel: " + panel.GetVisualDescendants().OfType<Control>()
                    .Count(c => c.Classes.Contains("hm-chip")));

                var map = Named<GrayLineMapControl>(window, "GreenZoneGrayLine");

                _output.WriteLine("  map " + F(map.Bounds.Width) + " x " + F(map.Bounds.Height));

                var (left, right) = Ink(panel);
                var start = panel.Padding.Left + panel.BorderThickness.Left;
                var inside = panel.Bounds.Width - (2 * start);

                _output.WriteLine("  ink " + F(left) + " to " + F(right) + " of " + F(start) + " to "
                    + F(start + inside) + " = " + F((right - left) / inside * 100) + "%");
                _output.WriteLine("");

                Assert.True(
                    right <= start + inside + 0.5,
                    "at " + width + " the green zone runs past its panel: " + F(right));
            }
            finally
            {
                window.Close();
            }
        }
    }

    /// <summary>**The night side at 2 pm and at 10 pm EDT, and where it falls.**</summary>
    [AvaloniaFact]
    public void TheNightSideAtTwoAndTenPm()
    {
        var places = new (string Name, double Latitude, double Longitude)[]
        {
            ("his grid FN00", 40.5, -79.0),
            ("Los Angeles", 34.05, -118.24),
            ("London", 51.51, -0.13),
            ("Moscow", 55.75, 37.62),
            ("Johannesburg", -26.2, 28.05),
            ("Tokyo", 35.68, 139.69),
            ("Sydney", -33.87, 151.21),
        };

        foreach (var (label, utc) in new[] { ("2 pm EDT", TwoPmEdt), ("10 pm EDT", TwoPmEdt.AddHours(8)) })
        {
            var sun = SolarTerminator.At(utc);
            var drawn = GrayLineMapControl.WhatWouldBeDrawn(utc, HisGrid, 202, 110);

            _output.WriteLine(label + " (" + utc.ToString("u", CultureInfo.InvariantCulture)
                + "): sun overhead at " + F(sun.Latitude) + " N, " + F(sun.Longitude) + " E; "
                + drawn.NightCells + " night cells, " + drawn.TwilightCells + " gray-edge cells, "
                + drawn.Markers + " marker at " + drawn.MarkerAt);

            foreach (var (name, latitude, longitude) in places)
            {
                var elevation = SolarTerminator.ElevationDegrees(sun, latitude, longitude);
                var darkness = SolarTerminator.Darkness(sun, latitude, longitude);
                var said = darkness <= 0 ? "day" : darkness >= 1 ? "night" : "gray edge";

                _output.WriteLine("  " + name.PadRight(14) + " sun " + F(elevation).PadLeft(7)
                    + " deg  " + said);
            }

            _output.WriteLine("");
        }
    }

    /// <summary>**The achievements window: the page, a category, and the continents.**</summary>
    [AvaloniaFact]
    public void TheAchievementsWindowAtItsOwnSize()
    {
        var screen = new AchievementsViewModel(
            TheAchievementsPageTests.TwelveContacts(), HisGrid,
            AchievementPoints.Parse(AchievementPoints.Shipped()));
        var window = new AchievementsWindow { DataContext = screen };

        window.Show();
        Settle(window);

        try
        {
            _output.WriteLine("ACHIEVEMENTS WINDOW " + F(window.Bounds.Width) + " x " + F(window.Bounds.Height));

            var legend = Named<TextBlock>(window, "AchievementsLegend");
            var bottom = (legend.TranslatePoint(new Point(0, 0), window)?.Y ?? 0) + legend.Bounds.Height;

            _output.WriteLine("  page: legend ends at " + F(bottom) + " of " + F(window.Bounds.Height));

            var badge = Named<ItemsControl>(window, "AchievementsBadges")
                .GetVisualDescendants().OfType<Button>().First();

            _output.WriteLine("  badge " + F(badge.Bounds.Width) + " x " + F(badge.Bounds.Height));

            foreach (var slot in new[] { "badge-name", "badge-meaning", "badge-next", "badge-corner" })
            {
                var text = window.GetVisualDescendants().OfType<TextBlock>()
                    .First(t => t.IsEffectivelyVisible && t.Classes.Contains(slot));

                _output.WriteLine("  " + slot.PadRight(14) + " slot " + F(text.Bounds.Width) + " px");
            }

            Assert.True(bottom <= window.Bounds.Height, "the page runs past its window");

            screen.OpenCategoryCommand.Execute(AchievementKinds.Countries);
            Settle(window);

            var cards = Named<ItemsControl>(window, "AchievementsCategoryCards");
            var title = cards.GetVisualDescendants().OfType<TextBlock>()
                .First(t => t.Classes.Contains("card-title"));

            _output.WriteLine("  Countries: " + cards.ItemCount + " cards, title slot " + F(title.Bounds.Width) + " px");

            screen.BackCommand.Execute(null);
            screen.OpenCategoryCommand.Execute(AchievementKinds.Continents);
            Settle(window);

            var seven = Named<ItemsControl>(window, "AchievementsSubBadges")
                .GetVisualDescendants().OfType<Button>().ToList();
            // **ROWS BY WHERE A BADGE'S MIDDLE FALLS, NOT ITS TOP.** A badge with a gap line
            // is taller and centered in its cell, so its top sits a few pixels above its
            // neighbors'; counted by tops, two rows read as four.
            var rows = seven.Select(b => Math.Round(
                    ((b.TranslatePoint(new Point(0, 0), window)?.Y ?? 0) + (b.Bounds.Height / 2)) / 20))
                .Distinct().Count();
            var last = seven.Max(b => (b.TranslatePoint(new Point(0, 0), window)?.Y ?? 0) + b.Bounds.Height);

            _output.WriteLine("  Continents: " + seven.Count + " badges in " + rows + " rows, ending at " + F(last));

            foreach (var one in seven)
            {
                var at = one.TranslatePoint(new Point(0, 0), window) ?? default;

                _output.WriteLine("    " + (one.CommandParameter as string ?? "").PadRight(14)
                    + " x " + F(at.X).PadLeft(6) + "  y " + F(at.Y).PadLeft(6) + "  "
                    + F(one.Bounds.Width) + " x " + F(one.Bounds.Height));
            }

            Assert.True(last <= window.Bounds.Height, "the seven continents run past the window");
        }
        finally
        {
            window.Close();
        }
    }

    private static string F(double value) => value.ToString("0.0", CultureInfo.InvariantCulture);

    private static void Settle(Window window)
    {
        for (var i = 0; i < 6; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }

    private static T Named<T>(Window window, string name)
        where T : Control
    {
        var found = window.GetVisualDescendants().OfType<T>().FirstOrDefault(c => c.Name == name);

        Assert.True(found is not null, "there is no " + typeof(T).Name + " named " + name);

        return found!;
    }

    /// <summary>How many lines a run laid out to, from its height and one line's height.</summary>
    private static int Lines(TextBlock text)
    {
        var one = new TextLayout(
            "Xg", new Typeface(text.FontFamily, text.FontStyle, text.FontWeight), text.FontSize, null)
            .Height;

        return one <= 0 ? 0 : (int)Math.Round(text.Bounds.Height / one);
    }

    /// <summary>The leftmost and rightmost ink on the panel, text by its own laid-out width.</summary>
    private static (double Left, double Right) Ink(Border panel)
    {
        var left = double.MaxValue;
        var right = double.MinValue;

        foreach (var control in panel.GetVisualDescendants().OfType<Control>()
            .Where(c => c.IsEffectivelyVisible && c.Bounds.Width > 0))
        {
            if (control is not (TextBlock or GrayLineMapControl or SparklineControl))
            {
                continue;
            }

            if (control is TextBlock { Text: var said } && string.IsNullOrWhiteSpace(said))
            {
                continue;
            }

            var at = control.TranslatePoint(new Point(0, 0), panel)?.X ?? 0;
            var width = control.Bounds.Width;

            if (control is TextBlock run)
            {
                var ink = Math.Min(width, new TextLayout(
                    run.Text ?? "", new Typeface(run.FontFamily, run.FontStyle, run.FontWeight),
                    run.FontSize, null).Width);

                at += run.TextAlignment switch
                {
                    TextAlignment.Right => width - ink,
                    TextAlignment.Center => (width - ink) / 2,
                    _ => 0,
                };

                width = ink;
            }

            left = Math.Min(left, at);
            right = Math.Max(right, at + width);
        }

        return (left, right);
    }

    private static Window Realized(double width)
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.LicenseClass = LicenseClass.General;
        settings.Operator.Callsign = "KC3QIS";
        settings.Operator.GridSquare = HisGrid;

        var model = new MainWindowViewModel(settings, null) { OperatingMode = "Digital" };

        model.SelectBandCommand.Execute(model.Bands.First(b => b.Band.Name == "20 m"));
        model.FrequencyHz = 14_074_000;
        model.ChosenDigitalMode = "FT8";
        model.MapExpanded = true;
        model.GrayLineUtc = TwoPmEdt;
        model.HeardInTheLastMinute = 6;
        model.HeardSparkline = GreenZone.Sparkline(
            new[] { 3, 9, 14, 30, 44, 58 }.Select(s => TwoPmEdt.AddSeconds(-s)), TwoPmEdt);

        var window = new MainWindow { DataContext = model, Width = width, Height = 1200 };

        window.Show();
        Settle(window);

        return window;
    }
}
