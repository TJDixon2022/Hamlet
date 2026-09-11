using System;
using System.Globalization;
using Hamlet.App.ViewModels;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 306 task 5: **a line between the two markers, and a hover on
/// each.**
/// </summary>
/// <remarks>
/// <para>**IT IS DRAWN ONLY WHEN BOTH ENDS ARE PLACED.** A line from a marker that
/// does not exist would be a claim about a position nobody has (§0.0), and on this
/// asset either end can be missing for three separate reasons.</para>
/// <para>**AND IT IS NOT LABELLED AS A PATH, A ROUTE OR A BEARING ANYWHERE.** Tim's
/// own words license the roughness: *"I'm not looking for perfect lines. I'm looking
/// for general - this is where you kind of are, and this is where the guy you're
/// talking to kind of is."*</para>
/// <para>**NO BEARING ON THE FACE OF ANYTHING** (*"what am I, some sort of submarine
/// captain?"*). The degree ring printed round the asset is part of the photograph
/// and is not Hamlet asserting anything; nothing here adds a tick, a label or a
/// readout on top of it.</para>
/// </remarks>
public sealed class TheGlobeLineTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the findings are printed.</param>
    public TheGlobeLineTests(ITestOutputHelper output) => _output = output;

    /// <summary>**The line is drawn only when both stations are placed.**</summary>
    [Fact]
    public void TheLineIsDrawnOnlyWhenBothStationsArePlaced()
    {
        foreach (var (mine, theirs, expected, why) in new[]
        {
            ("FN00DJ", "IO63", true, "both on the picture"),
            ("FN00DJ", null, false, "he put no grid on the air"),
            (null, "IO63", false, "no grid in Settings"),
            // **SYDNEY IS ON THE PICTURE SINCE UNIT 308.** What has nowhere now is
            // Antarctica and a strip of ocean west of Hawaii.
            ("FN00DJ", "QF56", true, "he is on the flat map now"),
            ("FN00DJ", "RB32", false, "he is in Antarctica, below the file"),
        })
        {
            var plot = new Ft8GlobePlot(mine, theirs, "TEST", "");

            _output.WriteLine(
                (mine ?? "(none)").PadRight(8) + " -> "
                + (theirs ?? "(none)").PadRight(8)
                + " line " + (plot.HasPath ? "drawn    " : "not drawn")
                + "  (" + why + ")");

            Assert.Equal(expected, plot.HasPath);
        }
    }

    /// <summary>**Each marker's hover carries its distance and compass word.**</summary>
    /// <remarks>
    /// **NOTHING ON THE FACE** (*show, do not tell*). The distance and the direction
    /// are there for a deliberate look and nowhere else.
    /// </remarks>
    [Fact]
    public void EachMarkerHoverCarriesItsDistanceAndCompassWord()
    {
        var plot = new Ft8GlobePlot("FN00DJ", "IO63", "EI4GNB", "Ireland");

        _output.WriteLine("operator: " + plot.OperatorTip);
        _output.WriteLine("station : " + plot.StationTip);

        Assert.Contains("You", plot.OperatorTip, StringComparison.Ordinal);
        Assert.Contains("FN00DJ", plot.OperatorTip, StringComparison.Ordinal);

        Assert.Contains("EI4GNB", plot.StationTip, StringComparison.Ordinal);
        Assert.Contains("miles", plot.StationTip, StringComparison.Ordinal);

        // **A COMPASS WORD, NOT A NUMBER** (HM-DEC-038).
        Assert.DoesNotContain("degrees", plot.StationTip, StringComparison.Ordinal);

        Assert.Contains(
            new[] { "north", "south", "east", "west" },
            point => plot.StationTip.Contains(point, StringComparison.Ordinal));
    }

    /// <summary>**No bearing is on the face of the globe.**</summary>
    /// <remarks>
    /// **THE CAPTION IS THE FACE HERE**, and it names the place, the grid and the
    /// distance. A direction on it would be exactly the submarine-captain readout
    /// Tim took off the cards.
    /// </remarks>
    [Fact]
    public void NoBearingIsOnTheFaceOfTheGlobe()
    {
        var plot = new Ft8GlobePlot("FN00DJ", "IO63", "EI4GNB", "Ireland");

        _output.WriteLine("caption: " + plot.Caption);

        Assert.DoesNotContain("degrees", plot.Caption, StringComparison.Ordinal);
        Assert.DoesNotContain("bearing", plot.Caption, StringComparison.Ordinal);

        foreach (var point in new[]
        {
            "northeast", "northwest", "southeast", "southwest",
        })
        {
            Assert.DoesNotContain(point, plot.Caption, StringComparison.Ordinal);
        }

        // **AND THE DISTANCE IS STILL ON IT**, which is what he asked to keep.
        Assert.Contains("miles", plot.Caption, StringComparison.Ordinal);
    }
}
