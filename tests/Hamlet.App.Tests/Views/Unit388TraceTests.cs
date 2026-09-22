using System;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Controls;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// **Work instruction 388 task 1: the band, the width budget and the pills row, measured before
/// one pixel moves.**
/// </summary>
/// <remarks>
/// <para>**THIS TRACE ASSERTS NOTHING ABOUT THE LAYOUT**, in the shape of unit 376's and unit
/// 387's traces. It is the before that task 3's arithmetic is computed against, and it is
/// deliberately not on the carry-forward list, for the reason <c>Unit376TheTopBandTests</c>'
/// trace is not: a name that cannot fail teaches nothing by being run.</para>
/// <para>**COMPUTED, NOT SEEN** (§0.0, FACT-004). Every number is read off a realized headless
/// window. No port is opened and nothing is keyed.</para>
/// </remarks>
public sealed class Unit388TraceTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the trace.</summary>
    /// <param name="output">Where every rectangle is printed.</param>
    public Unit388TraceTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// **The band's parts, the width budget and the pills' appetite at 1920, 1400 and 1100 x 780.**
    /// Asserts nothing.
    /// </summary>
    [AvaloniaFact]
    public void Unit388TraceTheBandTheWidthAndThePillsBeforeOnePixelMoves()
    {
        foreach (var (width, height) in Unit376TheTopBandTests.Sizes)
        {
            var window = TheTopRowTests.Realized(width, height, null, null);

            try
            {
                Unit376TheTopBandTests.Pump(window);
                Print(width, height, window);
            }
            finally
            {
                window.Close();
            }
        }
    }

    private void Print(double width, double height, Window window)
    {
        var band = Unit376TheTopBandTests.Band(window);
        var pills = Unit376TheTopBandTests.Pills(window);
        var card = TheTopRowTests.Card(window);
        var body = Named(window, "NeighborhoodCardBody");
        var block = Named(window, "GreenZoneBlock");
        var left = Named(window, "GreenZoneLeft");
        var right = Named(window, "GreenZoneRight");
        var mapColumn = Named(window, "GreenZoneMap");
        var map = Named(window, "GreenZoneGrayLine");
        var caption = Named(window, "GreenZoneClockCaption");
        var strip = window.GetVisualDescendants().OfType<NeighborhoodMapControl>().First();
        var drive = Named(window, "RigDriveAndPower");
        var topRow = Unit376TheTopBandTests.TopRow(window);

        var cardRect = In(card, window);
        var bodyRect = In(body, window);
        var mapRect = In(map, window);
        var captionRect = In(caption, window);
        var pillsRect = In(pills, window);
        var blockRect = In(block, window);
        var leftStack = block.GetVisualParent() as Control;
        var leftStackRect = leftStack is null ? default : In(leftStack, window);

        _output.WriteLine("WINDOW " + Px(width) + " x " + Px(height));
        _output.WriteLine("  BAND with pills " + Px(band.WithPills) + ", without " + Px(band.WithoutPills)
            + " | top " + Px(band.TopWithPills) + " bottom " + Px(band.Bottom) + " | " + band.Governs);
        _output.WriteLine("  TopRow            : " + Box(topRow));
        _output.WriteLine("  pills row         : " + Box(pillsRect) + ", margin " + pills.Margin
            + ", desired " + Px(pills.DesiredSize.Width) + " x " + Px(pills.DesiredSize.Height));

        var buttons = pills.GetVisualDescendants().OfType<Button>().Where(b => b.Classes.Contains("hm-band")).ToList();
        var buttonsWidth = buttons.Sum(b => b.Bounds.Width) + 8 * Math.Max(0, buttons.Count - 1);

        _output.WriteLine("  pills             : " + buttons.Count + " buttons, their widths plus 8 px spacing = "
            + Px(buttonsWidth) + " of a row " + Px(pillsRect.Width) + " wide, spare " + Px(pillsRect.Width - buttonsWidth)
            + "; rightmost pill ends at x " + Px(buttons.Max(b => In(b, window).Right)));
        _output.WriteLine("  card              : " + Box(cardRect) + ", header and top padding "
            + Px(bodyRect.Top - cardRect.Top) + ", bottom padding " + Px(cardRect.Bottom - bodyRect.Bottom)
            + ", left padding " + Px(bodyRect.Left - cardRect.Left) + ", right padding " + Px(cardRect.Right - bodyRect.Right));
        _output.WriteLine("  card body         : " + Box(bodyRect));
        _output.WriteLine("  left stack        : " + Box(leftStackRect) + ", desired " + Px(leftStack?.DesiredSize.Height ?? 0));
        _output.WriteLine("    strip           : " + Box(In(strip, window)));
        _output.WriteLine("    GreenZoneBlock  : " + Box(blockRect) + ", desired " + Px(block.DesiredSize.Height));
        _output.WriteLine("      GreenZoneLeft : " + Box(In(left, window)) + ", desired " + Px(left.DesiredSize.Width) + " x " + Px(left.DesiredSize.Height));
        _output.WriteLine("      GreenZoneRight: " + Box(In(right, window)) + ", desired " + Px(right.DesiredSize.Width) + " x " + Px(right.DesiredSize.Height));
        _output.WriteLine("    EMPTY UNDER THE GREEN ZONE: y " + Px(blockRect.Bottom) + " to " + Px(bodyRect.Bottom)
            + " = " + Px(bodyRect.Bottom - blockRect.Bottom) + " px tall, " + Px(blockRect.Width) + " px wide");
        _output.WriteLine("  map column        : " + Box(In(mapColumn, window)) + ", desired " + Px(mapColumn.DesiredSize.Height));
        _output.WriteLine("    map             : " + Box(mapRect) + ", aspect " + (mapRect.Width / mapRect.Height).ToString("0.0000", CultureInfo.InvariantCulture));
        _output.WriteLine("    caption         : " + Box(captionRect) + ", gap " + Px(captionRect.Top - mapRect.Bottom)
            + ", width wanted " + Px(caption.DesiredSize.Width));
        var favorites = window.GetVisualDescendants().OfType<FavoritesDropDownControl>().FirstOrDefault();

        if (favorites is not null)
        {
            _output.WriteLine("  favorites         : " + Box(In(favorites, window)) + ", desired "
                + Px(favorites.DesiredSize.Width) + " x " + Px(favorites.DesiredSize.Height)
                + ", padding " + favorites.Padding + ", border " + favorites.BorderThickness);

            foreach (var part in favorites.GetVisualDescendants().OfType<Control>())
            {
                _output.WriteLine("    " + part.GetType().Name.PadRight(18) + ": " + Box(In(part, window))
                    + ", desired " + Px(part.DesiredSize.Width) + " x " + Px(part.DesiredSize.Height)
                    + (part is TextBlock t ? " [" + t.Text + "]" : ""));
            }
        }

        _output.WriteLine("  rig border        : " + Box(band.Rig) + ", wants " + Px(band.RigWants));
        _output.WriteLine("  RigDriveAndPower  : " + Box(In(drive, window)));

        var aspect = mapRect.Width / mapRect.Height;

        foreach (var tall in new[] { band.WithPills, band.WithoutPills, topRow.Height })
        {
            _output.WriteLine("  a map " + Px(tall) + " tall at its own proportions is " + Px(tall * aspect)
                + " wide - " + Px(tall * aspect - mapRect.Width) + " more than today");
        }

        _output.WriteLine("  the width the card gives the left stack today " + Px(leftStackRect.Width)
            + "; minus the extra for a band-tall map leaves " + Px(leftStackRect.Width - (band.WithPills * aspect - mapRect.Width))
            + ", against GreenZoneLeft desired " + Px(left.DesiredSize.Width) + " + GreenZoneRight desired " + Px(right.DesiredSize.Width)
            + " + block padding and border 22");
        _output.WriteLine("");
    }

    private static Control Named(Window window, string name)
        => TheTopRowTests.Named<Control>(window, name);

    private static Rect In(Control control, Window window)
        => TheTopRowTests.RectIn(control, window);

    private static string Px(double value)
        => value.ToString("0.#", CultureInfo.InvariantCulture);

    private static string Box(Rect r)
        => Px(r.X) + "," + Px(r.Y) + " " + Px(r.Width) + " x " + Px(r.Height);
}
