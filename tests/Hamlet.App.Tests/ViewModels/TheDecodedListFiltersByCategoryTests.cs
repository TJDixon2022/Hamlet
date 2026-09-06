using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 251, task 6: the decoded list filters by category, and a
/// filter can never make the band look quiet.
/// </summary>
/// <remarks>
/// <para>**DIMMED AND NOT REMOVED, WHICH WAS THE ARBITER'S TO CHOOSE** (§12.1;
/// Tim has not ruled it). The band's texture stays visible - an evening on 20 m
/// is mostly other people's contacts, and a list showing only the CQs makes a
/// busy band look like a quiet one - and rows do not jump while he is reading
/// them at four slots a minute.</para>
/// <para>**THE COUNTS ARE WHAT MAKE THE CHOICE SAFE.** Opacity is no better than
/// colour for somebody who cannot see it well, so the summary says both numbers
/// in words and a collapsed panel carries them (§0.5, §0.6).</para>
/// </remarks>
public sealed class TheDecodedListFiltersByCategoryTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the rows and the summary are printed.</param>
    public TheDecodedListFiltersByCategoryTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>`CQ only` dims everything that is not a call to anyone.</summary>
    [Fact]
    public void CqOnlyDimsEverythingThatIsNotACallToAnyone()
    {
        var model = WithRows(out var rows, mine: "KD9ABC");

        model.DigitalFilter = DecodedFilter.CqOnly;

        Print(model);

        // **THE ROWS ARE ALL STILL THERE.** This is the assertion that separates
        // dimming from removing, and it is first because it is the choice.
        Assert.Equal(rows.Length, model.DigitalDecodes.Count);

        Assert.False(Row(model, "CQ TA3MPK KM39").IsDimmed);
        Assert.False(Row(model, "CQ DX EA3QQ JN11").IsDimmed);

        Assert.True(Row(model, "KE9COB N5CH R+14").IsDimmed);
        Assert.True(Row(model, "KD9ABC W4WTM -07").IsDimmed);
        Assert.True(Row(model, "TNX FER QSO OM").IsDimmed);

        Assert.Equal(2, model.DigitalShownCount);
        Assert.Equal(3, model.DigitalDimmedCount);
    }

    /// <summary>`mine` dims everything not addressed to the operator.</summary>
    [Fact]
    public void MineDimsEverythingNotAddressedToTheOperator()
    {
        var model = WithRows(out var rows, mine: "KD9ABC");

        model.DigitalFilter = DecodedFilter.Mine;

        Print(model);

        Assert.Equal(rows.Length, model.DigitalDecodes.Count);

        Assert.False(Row(model, "KD9ABC W4WTM -07").IsDimmed);

        Assert.True(Row(model, "CQ TA3MPK KM39").IsDimmed);
        Assert.True(Row(model, "KE9COB N5CH R+14").IsDimmed);

        Assert.Equal(1, model.DigitalShownCount);
        Assert.Equal(4, model.DigitalDimmedCount);

        // Hamlet knows his callsign here, so the filter has nothing to explain.
        Assert.False(model.HasDigitalFilterNote);
    }

    /// <summary>`everything` dims nothing, and is where a fresh file starts.</summary>
    [Fact]
    public void EverythingDimsNothingAndIsTheFreshDefault()
    {
        var model = WithRows(out var rows, mine: "KD9ABC");

        Assert.Equal(DecodedFilter.Everything, model.DigitalFilter);
        Assert.True(model.ShowsEverything);

        model.DigitalFilter = DecodedFilter.CqOnly;
        model.DigitalFilter = DecodedFilter.Everything;

        Print(model);

        Assert.All(model.DigitalDecodes, r => Assert.False(r.IsDimmed));
        Assert.Equal(rows.Length, model.DigitalShownCount);
        Assert.Equal(0, model.DigitalDimmedCount);
    }

    /// <summary>
    /// The summary counts what is shown and what is hidden, so a filter can never
    /// make the band look quiet.
    /// </summary>
    /// <remarks>
    /// **IT IS THE SUMMARY AND NOT A LINE IN THE TABLE**, because a collapsed
    /// panel is exactly where the mistake would be made: the rows are not on
    /// screen at all and the one sentence left has to carry it (§0.5).
    /// </remarks>
    [Fact]
    public void TheSummaryCountsBothHalves()
    {
        var model = WithRows(out _, mine: "KD9ABC");

        var unfiltered = model.DigitalDecodedSummary;

        model.DigitalFilter = DecodedFilter.CqOnly;

        var filtered = model.DigitalDecodedSummary;

        _output.WriteLine("everything : " + unfiltered);
        _output.WriteLine("CQ only    : " + filtered);

        Assert.Contains("5 shown", unfiltered, StringComparison.Ordinal);
        Assert.DoesNotContain("dimmed", unfiltered, StringComparison.Ordinal);

        Assert.Contains("2 shown", filtered, StringComparison.Ordinal);
        Assert.Contains("3 dimmed", filtered, StringComparison.Ordinal);

        // And it names the filter that did it, so the number is not a mystery.
        Assert.Contains("CQ only", filtered, StringComparison.Ordinal);
    }

    /// <summary>
    /// With no callsign on file, `mine` says it has nothing to match on rather
    /// than silently showing nothing.
    /// </summary>
    [Fact]
    public void MineWithNoCallsignSaysSoAndDimsNothing()
    {
        var model = WithRows(out var rows, mine: "");

        model.DigitalFilter = DecodedFilter.Mine;

        Print(model);

        // **NOTHING IS DIMMED**, which is the ruling: `mine` is offered, and
        // where there is nothing to match on it does not quietly empty the band.
        Assert.All(model.DigitalDecodes, r => Assert.False(r.IsDimmed));
        Assert.Equal(rows.Length, model.DigitalShownCount);
        Assert.Equal(0, model.DigitalDimmedCount);

        Assert.True(model.HasDigitalFilterNote);
        Assert.Contains(
            "nothing to match on", model.DigitalFilterNote, StringComparison.Ordinal);

        // The choice is still available and still selected - it is not disabled
        // and it did not silently fall back to something else.
        Assert.True(model.ShowsMine);
    }

    /// <summary>The filter is remembered between evenings.</summary>
    [Fact]
    public void TheFilterSurvivesTheSettingsFile()
    {
        var settings = new AppSettings();
        var model = new MainWindowViewModel(settings, null);

        model.SetDecodedFilterCommand.Execute("CqOnly");

        Assert.Equal("CqOnly", settings.DecodedFilter);

        var path = Path.Combine(
            Path.GetTempPath(),
            "hamlet-unit251-filter-" + Guid.NewGuid().ToString("N") + ".json");

        try
        {
            SettingsStore.SaveTo(settings, path);

            var reloaded = SettingsStore.LoadFrom(path);
            var reopened = new MainWindowViewModel(reloaded, null);

            _output.WriteLine("reopened on : " + reopened.DigitalFilter);

            Assert.Equal(DecodedFilter.CqOnly, reopened.DigitalFilter);
            Assert.True(reopened.ShowsCqOnly);
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>A settings file naming a filter that does not exist shows everything.</summary>
    [Fact]
    public void AFilterThatDoesNotExistShowsEverything()
    {
        var model = new MainWindowViewModel(
            new AppSettings { DecodedFilter = "OnlyTheInterestingOnes" }, null);

        Assert.Equal(DecodedFilter.Everything, model.DigitalFilter);

        // And a command parameter it cannot read changes nothing, rather than
        // resetting the filter behind the operator's back.
        model.DigitalFilter = DecodedFilter.Mine;
        model.SetDecodedFilterCommand.Execute("NotAFilter");

        Assert.Equal(DecodedFilter.Mine, model.DigitalFilter);
    }

    /// <summary>
    /// A row that arrives while a filter is on is dimmed before it is drawn.
    /// </summary>
    /// <remarks>
    /// **OTHERWISE IT FLASHES.** A row put in at full strength and dimmed a
    /// moment later catches the eye at exactly the moment the movement already
    /// has, which is the opposite of what the filter is for.
    /// </remarks>
    [Fact]
    public void ARowArrivingUnderAFilterIsDimmedBeforeItIsDrawn()
    {
        var model = WithRows(out _, mine: "KD9ABC");

        model.DigitalFilter = DecodedFilter.CqOnly;

        var arriving = model.AddDecodeRowForTests(
            "214150", "-13", "0.3", "1620", "W1ABC K4XYZ RR73");

        Assert.True(arriving.IsDimmed);

        var alsoArriving = model.AddDecodeRowForTests(
            "214150", "-08", "0.2", "980", "CQ K4XYZ FM18");

        Assert.False(alsoArriving.IsDimmed);
    }

    /// <summary>The predicate itself, over the shapes a to-field takes.</summary>
    [Theory]
    [InlineData(DecodedFilter.CqOnly, "CQ", true)]
    [InlineData(DecodedFilter.CqOnly, "CQ DX", true)]
    [InlineData(DecodedFilter.CqOnly, "CQ EU", true)]
    [InlineData(DecodedFilter.CqOnly, "cq", true)]
    [InlineData(DecodedFilter.CqOnly, "KD9ABC", false)]
    [InlineData(DecodedFilter.CqOnly, "", false)]
    [InlineData(DecodedFilter.Mine, "KD9ABC", true)]
    [InlineData(DecodedFilter.Mine, "kd9abc", true)]
    [InlineData(DecodedFilter.Mine, "CQ", false)]
    [InlineData(DecodedFilter.Mine, "", false)]
    [InlineData(DecodedFilter.Everything, "", true)]
    [InlineData(DecodedFilter.Everything, "CQ", true)]
    [InlineData(DecodedFilter.Everything, "KE9COB", true)]
    public void ThePredicateReadsTheToField(
        DecodedFilter filter, string to, bool wanted)
    {
        var got = DecodedFilterRule.Wants(filter, to, "KD9ABC");

        _output.WriteLine(
            filter + " / [" + to + "] -> " + (got ? "shown" : "dimmed"));

        Assert.Equal(wanted, got);
    }

    private static DigitalDecodeRow Row(MainWindowViewModel model, string message)
        => model.DigitalDecodes.Single(r => r.Message == message);

    private void Print(MainWindowViewModel model)
    {
        foreach (var row in model.DigitalDecodes)
        {
            _output.WriteLine(
                (row.IsDimmed ? "  dim  " : "  show ") + row.Message
                + "   opacity " + row.RowOpacity.ToString("0.00"));
        }

        _output.WriteLine("summary : " + model.DigitalDecodedSummary);
    }

    /// <summary>Five rows covering every shape the predicate has to read.</summary>
    private static MainWindowViewModel WithRows(
        out string[] messages, string mine)
    {
        var settings = new AppSettings();

        settings.Operator.Callsign = mine;

        var model = new MainWindowViewModel(settings, null);

        messages = new[]
        {
            // A call to anyone, and a call to anyone with a direction on it.
            "CQ TA3MPK KM39",
            "CQ DX EA3QQ JN11",

            // Two other stations, which is neither a CQ nor his.
            "KE9COB N5CH R+14",

            // Addressed to the operator.
            "KD9ABC W4WTM -07",

            // Free text: no three fields at all, so no addressee to match.
            "TNX FER QSO OM",
        };

        foreach (var message in messages)
        {
            model.AddDecodeRowForTests("214135", "-12", "0.4", "1240", message);
        }

        return model;
    }
}
