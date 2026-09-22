using System;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Settings;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>Work instruction 390 task 2's trace: what the favorites row lays out, part by part. Asserts nothing.</summary>
public sealed class Unit390TraceTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the trace.</summary>
    /// <param name="output">Where every rectangle is printed.</param>
    public Unit390TraceTests(ITestOutputHelper output) => _output = output;

    /// <summary>Prints the row under the green zone and every control in it, at 1920, 1400 and 900 x 620.</summary>
    [AvaloniaFact]
    public void TheFavoritesRowPartByPart()
    {
        foreach (var (width, height, three) in new[] { (1920.0, 1040.0, true), (1400.0, 1040.0, true), (900.0, 620.0, false), (900.0, 620.0, true) })
        {
            var settings = TheTopRowTests.FixtureSettings();

            if (three)
            {
                settings.Favorites.Add(new SavedFavorite { FrequencyHz = 14_074_000, Name = "the FT8 watering hole", Mode = "USB-D", BandName = "20 m" });
                settings.Favorites.Add(new SavedFavorite { FrequencyHz = 14_050_000, Name = "slow CW on Sunday", Mode = "CW", BandName = "20 m" });
                settings.Favorites.Add(new SavedFavorite { FrequencyHz = 14_250_000, Name = "the Tuesday net", Mode = "USB", BandName = "20 m" });
            }

            var window = TheTopRowTests.Realized(width, height, settings);

            try
            {
                var grid = TheTopRowTests.Named<Grid>(window, "GreenZoneFavoritesRow");
                var caption = TheTopRowTests.Named<TextBlock>(window, "GreenZoneClockCaption");
                var card = TheTopRowTests.Card(window);

                _output.WriteLine("=== " + width + " x " + height + (three ? ", three saved" : ", none saved"));
                _output.WriteLine("  card      " + Box(TheTopRowTests.RectIn(card, window)));
                _output.WriteLine("  row grid  " + Box(TheTopRowTests.RectIn(grid, window)) + " desired " + Size(grid.DesiredSize));
                _output.WriteLine("  caption   " + Box(TheTopRowTests.RectIn(caption, window)) + " desired " + Size(caption.DesiredSize));

                foreach (var part in grid.GetVisualDescendants().OfType<Control>().Where(c => c is TextBlock or Button or ScrollViewer or ItemsControl).Take(20))
                {
                    _output.WriteLine("    " + part.GetType().Name.PadRight(18) + (part.Name ?? "").PadRight(24)
                        + Box(TheTopRowTests.RectIn(part, window)) + " desired " + Size(part.DesiredSize)
                        + (part is TextBlock t ? " [" + t.Text + "]" : ""));
                }
            }
            finally
            {
                window.Close();
            }
        }
    }

    private static string Px(double value) => value.ToString("0.#", CultureInfo.InvariantCulture);

    private static string Size(Size s) => Px(s.Width) + "x" + Px(s.Height);

    private static string Box(Rect r) => "[" + Px(r.X) + "," + Px(r.Y) + " " + Px(r.Width) + "x" + Px(r.Height) + "]";
}
