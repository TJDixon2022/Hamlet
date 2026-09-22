using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Hamlet.App.Controls;
using Hamlet.App.ViewModels;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// **Criterion 10.3's report: the sun map as unit 389 left it** - work instruction 390 task 5.
/// </summary>
/// <remarks>
/// <para>**STATED, THEN ASSERTED.** At the band's left edge the map is 393 x 214 - the band's
/// whole height, pills row included - at 1920 on FT8, PSK31 and Olivia and at 1400 on FT8 and
/// Olivia. Where the card beside it has to say *PSK31 lives at 14.070; you are at 14.074* (the
/// strayed line, at 1400 on PSK31 with this fixture's dial on 14.074), it stands between the card
/// and the rig face at 327 x 178, the card's height. The band is 214 in every case at 1920 and
/// 1400. At 1100 x 780 the top reflows and the map keeps the mockup's 246 x 134, asserted
/// separately below.</para>
/// <para>`TheTopRowTests.TheWorldClockIsAtTheCardsRightEndWithOneMarker` asserts where it stands
/// and how tall; this asserts the two sizes the criterion's report states, and that tonight's
/// favorites chips (task 2) moved neither. **NOTHING IS KEYED AND NO PORT IS OPENED.**</para>
/// </remarks>
public sealed class TheSunMapStandsWhereItWasLeftTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the test.</summary>
    /// <param name="output">Where every size is printed.</param>
    public TheSunMapStandsWhereItWasLeftTests(ITestOutputHelper output) => _output = output;

    /// <summary>393 x 214 at the left edge; 327 x 178 beside the card; the band 214.</summary>
    [AvaloniaFact]
    public void ItIs393By214AtTheLeftEdgeAnd327By178BesideTheCard()
    {
        var cases = new (double Width, double Height, string Mode, bool LeftEdge)[]
        {
            (1920, TheTopRowTests.WindowHeight, "FT8", true),
            (1920, TheTopRowTests.WindowHeight, "PSK31", true),
            (1920, TheTopRowTests.WindowHeight, "Olivia", true),
            (1400, TheTopRowTests.WindowHeight, "FT8", true),
            (1400, TheTopRowTests.WindowHeight, "Olivia", true),
            (1400, TheTopRowTests.WindowHeight, "PSK31", false),
        };

        var misses = new List<string>();

        foreach (var (width, height, mode, leftEdge) in cases)
        {
            var window = TheTopRowTests.Realized(width, height, null, null);
            var model = (MainWindowViewModel)window.DataContext!;

            try
            {
                model.ChosenDigitalMode = mode;
                Unit376TheTopBandTests.Pump(window);

                var map = TheTopRowTests.RectIn(TheTopRowTests.Named<GrayLineMapControl>(window, "GreenZoneGrayLine"), window);
                var row = TheTopRowTests.Named<BandGovernsTheMapPanel>(window, "BandRow");
                var band = Unit376TheTopBandTests.Band(window);
                var (w, h) = leftEdge ? (393.0, 214.0) : (327.0, 178.0);
                var at = Px(width) + " x " + Px(height) + " on " + mode;

                _output.WriteLine(
                    at.PadRight(22) + " map " + Px(map.Width) + " x " + Px(map.Height) + ", left edge "
                    + row.MapIsAtTheLeftEdge + ", band " + Px(band.WithPills));

                if (row.MapIsAtTheLeftEdge != leftEdge)
                {
                    misses.Add(at + ": the map is " + (row.MapIsAtTheLeftEdge ? "" : "not ") + "at the band's left edge");
                }

                if (Math.Abs(map.Width - w) > 1 || Math.Abs(map.Height - h) > 1)
                {
                    misses.Add(at + ": the map is " + Px(map.Width) + " x " + Px(map.Height) + " where 10.3 states " + Px(w) + " x " + Px(h));
                }

                if (width >= 1400 && Math.Abs(band.WithPills - Unit376TheTopBandTests.BandReachedWithThePills) > 0.5)
                {
                    misses.Add(at + ": the band is " + Px(band.WithPills) + " where it stands at 214");
                }
            }
            finally
            {
                model.ChosenDigitalMode = "FT8";
                window.Close();
            }
        }

        Assert.True(misses.Count == 0, string.Join(Environment.NewLine, misses));
    }

    /// <summary>
    /// **At 1100 x 780, the size Hamlet opens at, the map keeps the mockup's 246 x 134 and never
    /// less** - measured, and not the 327 x 178 work instruction 390 stated for *under 1400 wide*.
    /// </summary>
    /// <remarks>
    /// The top reflows there (the band measured 402 tonight) and 10.3 names only 1920 and 1400, so
    /// this holds the floor the map has always kept rather than a size the criterion never asked for.
    /// </remarks>
    [AvaloniaFact]
    public void AtTheSizeItOpensAtItKeepsTheMockupsSize()
    {
        var window = TheTopRowTests.Realized(1100, 780, null, null);

        try
        {
            Unit376TheTopBandTests.Pump(window);

            var map = TheTopRowTests.RectIn(TheTopRowTests.Named<GrayLineMapControl>(window, "GreenZoneGrayLine"), window);
            var row = TheTopRowTests.Named<BandGovernsTheMapPanel>(window, "BandRow");

            _output.WriteLine("1100 x 780 on FT8: map " + Px(map.Width) + " x " + Px(map.Height) + ", left edge " + row.MapIsAtTheLeftEdge);

            Assert.False(row.MapIsAtTheLeftEdge);
            Assert.InRange(map.Width, 245, 247);
            Assert.InRange(map.Height, 133, 135);
        }
        finally
        {
            window.Close();
        }
    }

    private static string Px(double value) => value.ToString("0.#", CultureInfo.InvariantCulture);
}
