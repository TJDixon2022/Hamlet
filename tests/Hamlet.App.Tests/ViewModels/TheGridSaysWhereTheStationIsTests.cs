using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Explore;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 271, task 6: a grid says where the station is, with the
/// country from the callsign and a compass word only where it means something.
/// </summary>
/// <remarks>
/// <para>**WHAT WAS WRONG WITH THE OLD SENTENCE.** It read *IK4LZH is calling
/// anyone from grid JN54, which is 4,400 miles away from you*. The word *which*
/// attaches to the grid, so the sentence says the **square** is four thousand
/// miles away and leaves open where IK4LZH is calling from. He is in it, and the
/// rewrite says so.</para>
/// <para>**THE COUNTRY COMES FROM THE CALLSIGN AND NEVER FROM THE GRID** (Tim,
/// 2026-09-07). A grid square straddles borders; a prefix does not. Where the two
/// disagree the callsign wins, and where the DXCC table declines there is no
/// country at all — his ruling of 2026-09-06 untouched.</para>
/// </remarks>
public sealed class TheGridSaysWhereTheStationIsTests
{
    /// <summary>The operator's own square, as Settings has it on his machine.</summary>
    private const string HisGrid = "FN00DJ";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the tooltips are printed.</param>
    public TheGridSaysWhereTheStationIsTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>The distance belongs to the station, not to the square.</summary>
    /// <remarks>
    /// **THE WORDS `from grid` ARE THE FAULT AND THE TEST NAMES THEM.** It is not
    /// enough that the new sentence is right; the old construction must be gone,
    /// or a later edit restores it and nothing notices.
    /// </remarks>
    [Fact]
    public void TheDistanceBelongsToTheStation()
    {
        var help = Help("CQ IK4LZH JN54", HisGrid);

        _output.WriteLine(help);

        // **`He`, BY TIM'S RULING OF 2026-09-07** (HM-DEC-159). Unit 271 wrote
        // `They` under a rule `Ft8Vocabulary` used to state; he ruled the other
        // way and the rule came out of the file with the same change.
        Assert.Equal(
            "IK4LZH is calling anyone. He is in northern Italy, in grid JN54, "
            + "4,400 miles away on a bearing of 53 degrees.",
            help);

        Assert.DoesNotContain("from grid", help, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>No tooltip in any state says `from grid` any more.</summary>
    [Theory]
    [InlineData("CQ IK4LZH JN54")]
    [InlineData("CQ ON4ABC JO20")]
    [InlineData("KE9COB N5CH EM12")]
    [InlineData("CQ VK9XYZ QG44")]
    public void NothingSaysFromGridAnyMore(string message)
    {
        foreach (var his in new[] { HisGrid, "" })
        {
            var help = Help(message, his);

            _output.WriteLine("[" + his + "] " + help);

            Assert.DoesNotContain(
                "from grid", help, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// The same Italian callsign reads northern or southern by its grid.
    /// </summary>
    /// <remarks>
    /// **THE COUNTRY IS THE CALLSIGN'S AND THE COMPASS WORD IS THE GRID'S.** The
    /// callsign does not move between these three rows; only the latitude does,
    /// which is what makes the qualifier a measurement rather than a second guess
    /// at the country.
    /// </remarks>
    [Theory]
    [InlineData("JN54", "northern Italy")]
    [InlineData("JM88", "southern Italy")]
    [InlineData("JN62", "Italy")]
    public void AnItalianCallsignReadsByItsLatitude(string grid, string expected)
    {
        var help = Help("CQ IK4LZH " + grid, HisGrid);

        _output.WriteLine(grid + " -> " + help);

        Assert.Contains("in " + expected + ",", help, StringComparison.Ordinal);
    }

    /// <summary>A Belgian callsign gets the country and no compass word.</summary>
    /// <remarks>
    /// **BELGIUM IS BARELY TWO GRID SQUARES TALL** — 1.95 degrees — so a compass
    /// word in front of it claims a precision the grid does not have. It is in the
    /// table's `declined` list with that reason beside it, so a reader can see it
    /// was considered rather than forgotten.
    /// </remarks>
    [Theory]
    [InlineData("JO20")]
    [InlineData("JO11")]
    public void ABelgianCallsignGetsNoCompassWord(string grid)
    {
        var help = Help("CQ ON4ABC " + grid, HisGrid);

        _output.WriteLine(grid + " -> " + help);

        Assert.Contains("in Belgium,", help, StringComparison.Ordinal);
        Assert.DoesNotContain("northern", help, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("southern", help, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// A callsign the DXCC table declines gets no country at all.
    /// </summary>
    /// <remarks>
    /// **CERTAIN OR SILENT, UNTOUCHED** (Tim, 2026-09-06). `VK9` is five different
    /// islands. The sentence still says where the square is and how far, because
    /// that is arithmetic and is not in doubt; what it does not do is name a
    /// country it cannot be sure of.
    /// </remarks>
    [Theory]
    [InlineData("CQ VK9XYZ QG44")]
    [InlineData("CQ 3D2AB RH91")]
    public void ADeclinedCallsignGetsNoCountry(string message)
    {
        var help = Help(message, HisGrid);

        _output.WriteLine(help);

        Assert.Contains("He is in grid", help, StringComparison.Ordinal);
        Assert.Contains("miles away", help, StringComparison.Ordinal);
        Assert.DoesNotContain("Australia", help, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Fiji", help, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Where the callsign and the grid disagree, the callsign wins.</summary>
    /// <remarks>
    /// **`W4/YV7AXM` IS IN THE UNITED STATES WHATEVER HIS GRID SAYS**, which is
    /// the instruction's own example and the reason the country never comes from
    /// the grid. Here he sends a Venezuelan grid and the sentence still names the
    /// United States, because that is where he is transmitting from.
    /// </remarks>
    [Fact]
    public void WhereTheyDisagreeTheCallsignWins()
    {
        var help = Help("CQ W4/YV7AXM FK60", HisGrid);

        _output.WriteLine(help);

        // **THE SPOKEN FORM** (work instruction 281 task 5). The claim under test
        // is unchanged - the country comes from the callsign and not the grid -
        // and only the way the name is said has moved.
        Assert.Contains(
            "in the United States,", help, StringComparison.Ordinal);
        Assert.DoesNotContain("Venezuela", help, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>No output names a place below the country.</summary>
    /// <remarks>
    /// **THE SWEEP THAT GUARDS THE RULING** (Tim, 2026-09-07: no cities, no
    /// provinces, no *Tuscany*). Every name below is one these very squares would
    /// attract if anybody ever wired a finer lookup in.
    /// </remarks>
    [Fact]
    public void NoOutputNamesAPlaceBelowTheCountry()
    {
        string[] messages =
        {
            "CQ IK4LZH JN54", "CQ IK4LZH JM88", "CQ ON4ABC JO20",
            "CQ W4/YV7AXM FK60", "CQ VK9XYZ QG44", "KE9COB N5CH EM12",
            "CQ JA1XYZ PM95", "CQ LA1ABC JP99",
        };

        string[] places =
        {
            "Tuscany", "Bologna", "Rome", "Milan", "Brussels", "Flanders",
            "Wallonia", "Texas", "Houston", "Florida", "Tokyo", "Oslo",
            "Sicily", "Lombardy", "Antwerp",
        };

        foreach (var message in messages)
        {
            var help = Help(message, HisGrid);

            _output.WriteLine(message + " -> " + help);

            foreach (var place in places)
            {
                Assert.DoesNotContain(
                    place, help, StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    /// <summary>With no grid of his own it still says where they are.</summary>
    [Fact]
    public void WithNoGridOfHisOwnItStillNamesTheCountryAndTheSquare()
    {
        var help = Help("CQ IK4LZH JN54", "");

        _output.WriteLine(help);

        Assert.Contains("northern Italy", help, StringComparison.Ordinal);
        Assert.Contains("JN54", help, StringComparison.Ordinal);
        Assert.Contains("Settings", help, StringComparison.Ordinal);
        Assert.DoesNotContain("miles", help, StringComparison.Ordinal);
    }

    /// <summary>The table records what it refused and why.</summary>
    /// <remarks>
    /// **A JUDGEMENT BURIED IN CODE IS ONE NOBODY CAN ARGUE WITH**, which is the
    /// instruction's own reason for asking for a table. This asserts the refusals
    /// are in it with their reasoning, not merely absent from the qualified list.
    /// </remarks>
    [Fact]
    public void TheTableSaysWhatItRefusedAndWhy()
    {
        _output.WriteLine(
            EntityQualifier.QualifiedCount + " entities take a qualifier, "
            + EntityQualifier.DeclinedCount + " were considered and refused one");

        Assert.True(EntityQualifier.QualifiedCount >= 5);
        Assert.True(EntityQualifier.DeclinedCount >= 3);

        var belgium = EntityQualifier.WhyDeclined("Belgium");

        _output.WriteLine("Belgium: " + belgium);

        Assert.NotNull(belgium);
        Assert.Contains("degree", belgium!, StringComparison.OrdinalIgnoreCase);

        // An entity nobody considered is not a refusal, and says so by being null.
        Assert.Null(EntityQualifier.WhyDeclined("Italy"));
    }

    /// <summary>The tooltip a row would show, built the way the panel builds it.</summary>
    private static string Help(string message, string hisGrid)
    {
        var settings = new AppSettings();

        settings.Operator.GridSquare = hisGrid;

        return new MainWindowViewModel(settings, null)
            .AddDecodeRowForTests("214135", "-11", "0.2", "1240", message)
            .PayloadHelp;
    }
}
