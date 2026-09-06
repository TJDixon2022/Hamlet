using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 252, task 2: hovering a grid says the square, how far away
/// it is and which way, and never where it is.
/// </summary>
/// <remarks>
/// <para>**A GRID IS A BOX AND NOT A PLACE** (Tim's ruling, 2026-09-05). A
/// four-character square is roughly 70 by 100 miles, so *Texas* is fair and
/// *Houston* is a lie, and the ruling shows neither. What is left that the four
/// characters honestly support is arithmetic: a distance and a bearing.</para>
/// <para>**THE PLACE-NAME SWEEP BELOW IS THE REAL GUARD.** Unit 241 kept the
/// tooltip safe by never repeating the four characters, on the reasoning that a
/// sentence with a grid in it could grow a country on the end. That instinct was
/// right and it is no longer available, because the sentence now carries a
/// distance and the operator has to see which square it was measured from. A test
/// that sweeps every output for a place name is the guard that replaces
/// it.</para>
/// </remarks>
public sealed class TheGridTooltipSaysHowFarTests
{
    /// <summary>The operator's own square, as Settings has it on his machine.</summary>
    private const string HisGrid = "FN00DJ";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the tooltips are printed.</param>
    public TheGridTooltipSaysHowFarTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>With his grid set, a grid payload carries distance and bearing.</summary>
    [Fact]
    public void AGridPayloadSaysHowFarAndWhichWay()
    {
        var help = Help("CQ HA1BF JN86", HisGrid);

        _output.WriteLine(help);

        Assert.Contains("JN86", help, StringComparison.Ordinal);
        // 4,551 miles to the nearest hundred, which is the rounding task 2 chose.
        Assert.Contains("4,600 miles", help, StringComparison.Ordinal);
        Assert.Contains("49 degrees", help, StringComparison.Ordinal);
        Assert.Contains("HA1BF", help, StringComparison.Ordinal);
    }

    /// <summary>A grid sent to a named station reads as a report to that station.</summary>
    /// <remarks>
    /// **THE EXISTING SHAPE SURVIVES** (unit 251). A payload between two other
    /// stations is worded as being between them, and the operator is not in it —
    /// the distance is the one thing here measured from where he is, and it says
    /// *from you* so that it cannot be read as a distance between the two of them.
    /// </remarks>
    [Fact]
    public void AGridToANamedStationStillNamesBothStations()
    {
        var help = Help("KE9COB N5CH EM12", HisGrid);

        _output.WriteLine(help);

        Assert.Contains("N5CH", help, StringComparison.Ordinal);
        Assert.Contains("KE9COB", help, StringComparison.Ordinal);
        Assert.Contains("EM12", help, StringComparison.Ordinal);
        Assert.Contains("from you", help, StringComparison.Ordinal);
        Assert.DoesNotContain(" you are ", help, StringComparison.Ordinal);
    }

    /// <summary>
    /// With no grid of his own, it says the square and names Settings.
    /// </summary>
    /// <remarks>
    /// **IT DOES NOT FALL SILENT AND IT DOES NOT GUESS.** Going quiet would read
    /// as the grid meaning nothing, and a location taken from a callsign prefix or
    /// anything else is the §0.0 fault this unit is most exposed to. So the
    /// tooltip says what the square is, that Hamlet cannot measure from it yet,
    /// and where the missing fact goes.
    /// </remarks>
    [Fact]
    public void WithNoGridOfHisOwnItNamesSettings()
    {
        var help = Help("CQ HA1BF JN86", "");

        _output.WriteLine(help);

        Assert.Contains("JN86", help, StringComparison.Ordinal);
        Assert.Contains("Settings", help, StringComparison.Ordinal);
        Assert.Contains("your own grid square", help, StringComparison.Ordinal);

        // No number is offered, because none can be measured.
        Assert.DoesNotContain("miles", help, StringComparison.Ordinal);
        Assert.DoesNotContain("degrees", help, StringComparison.Ordinal);
    }

    /// <summary>A malformed grid of his own is the same as none.</summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("nonsense")]
    [InlineData("ZZ99")]
    public void AGridHamletCannotReadIsTheSameAsNoGrid(string his)
    {
        var help = Help("CQ HA1BF JN86", his);

        _output.WriteLine("[" + his + "] -> " + help);

        Assert.Contains("Settings", help, StringComparison.Ordinal);
        Assert.DoesNotContain("miles", help, StringComparison.Ordinal);
    }

    /// <summary>
    /// No grid tooltip, in any state, contains the name of a place.
    /// </summary>
    /// <remarks>
    /// **THE GUARD THAT REPLACES WITHHOLDING THE CHARACTERS.** The list below is
    /// deliberately made of the names these very squares would attract if anybody
    /// ever wired a lookup in: `JN86` is Hungary, `EM12` is Texas, `IO91` is
    /// England, `PM95` is Japan. If one of them ever appears here, the ruling has
    /// been broken and this fails.
    /// </remarks>
    [Fact]
    public void NoGridTooltipEverNamesAPlace()
    {
        string[] squares = { "JN86", "EM12", "IO91", "PM95", "FN00", "OF88" };

        string[] placeNames =
        {
            "Hungary", "Budapest", "Texas", "Houston", "England", "London",
            "Japan", "Tokyo", "Pennsylvania", "Australia", "Perth", "Europe",
            "Asia", "America", "United States", "Africa",
        };

        foreach (var square in squares)
        {
            foreach (var his in new[] { HisGrid, "" })
            {
                var help = Help("CQ HA1BF " + square, his);

                _output.WriteLine(square + " / [" + his + "] -> " + help);

                foreach (var name in placeNames)
                {
                    Assert.DoesNotContain(
                        name, help, StringComparison.OrdinalIgnoreCase);
                }
            }
        }
    }

    /// <summary>
    /// The rest of the closed table is untouched by the grid work.
    /// </summary>
    /// <remarks>
    /// **NOTHING ELSE MOVED** (task 4's rule, checked here because this is the
    /// task that changed `Explain`). Reports, the courtesies and the calls keep
    /// exactly the wording unit 251 left, and a payload off the table still gets
    /// nothing at all.
    /// </remarks>
    [Theory]
    [InlineData("KE9COB N5CH R+14", "N5CH has KE9COB's message")]
    [InlineData("KE9COB N5CH -07", "N5CH hears KE9COB at -7 dB")]
    [InlineData("KE9COB N5CH RR73", "everything came through")]
    [InlineData("KE9COB N5CH 73", "signing off")]
    [InlineData("CQ HA1BF CQ", "is calling anyone")]
    public void TheRestOfTheClosedTableIsUnchanged(string message, string expected)
    {
        var help = Help(message, HisGrid);

        _output.WriteLine(message + " -> " + help);

        Assert.Contains(expected, help, StringComparison.Ordinal);
        Assert.DoesNotContain("miles", help, StringComparison.Ordinal);
    }

    /// <summary>A payload off the closed table still gets nothing.</summary>
    [Theory]
    [InlineData("TNX FER QSO OM")]
    [InlineData("KE9COB N5CH QRZ")]
    [InlineData("KE9COB N5CH 599")]
    public void APayloadOffTheTableStillGetsNothing(string message)
    {
        Assert.Equal("", Help(message, HisGrid));
    }

    /// <summary>The tooltip a row would show, built the way the panel builds it.</summary>
    /// <remarks>
    /// **THROUGH THE ROW AND NOT THROUGH `Explain` DIRECTLY**, so what is asserted
    /// is what the operator would actually hover, including the plumbing that
    /// carries his grid onto the row.
    /// </remarks>
    private static string Help(string message, string hisGrid)
    {
        var settings = new AppSettings();

        settings.Operator.GridSquare = hisGrid;

        var model = new MainWindowViewModel(settings, null);

        var row = model.AddDecodeRowForTests(
            "214135", "-11", "0.2", "1240", message);

        return row.PayloadHelp;
    }
}
