using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Contacts;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// The left-hand decoded list: everything, or the calls to anyone.
/// </summary>
/// <remarks>
/// <para>**THIS FILE LOST HALF ITS TESTS ON 2026-09-07 AND THEY WERE NOT
/// DELETED, THEY MOVED.** Unit 252 built `CQ` and `mine` as two independent
/// toggles over one list, and Tim's ruling of 2026-09-07 makes `mine` a **side of
/// the panel** instead: always on, no toggle, nothing that could hide it. So
/// every test here that pressed a `mine` toggle is answered by
/// <see cref="TheDecodedAreaSplitsInTwoTests"/>, which asks the same questions of
/// the side.</para>
/// <para>**WHAT STAYED IS WHAT IS STILL TRUE**: the `CQ` toggle, `everything`
/// being the absence of it, the counts, the order, and the compound-callsign rule
/// — which is still live, and is asked of `Ft8MessageSplit` by both sides of the
/// split.</para>
/// </remarks>
public sealed class TheDecodedListFiltersByCategoryTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the rows and the summary are printed.</param>
    public TheDecodedListFiltersByCategoryTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>`CQ` keeps the calls to anyone and nothing else.</summary>
    [Fact]
    public void CqKeepsEveryCallToAnyone()
    {
        var model = WithRows(mine: "KD9ABC");

        model.ShowsCqOnly = true;

        Print(model);

        Assert.True(OnTheLeft(model, "CQ TA3MPK KM39"));
        Assert.True(OnTheLeft(model, "CQ DX EA3QQ JN11"));

        Assert.False(OnTheLeft(model, "KE9COB N5CH R+14"));
        Assert.False(OnTheLeft(model, "TNX FER QSO OM"));
    }

    /// <summary>The toggle off is `everything`, and it is the fresh-file state.</summary>
    [Fact]
    public void TheToggleOffIsEverything()
    {
        var model = WithRows(mine: "KD9ABC");

        Assert.True(model.ShowsEverything);
        Assert.False(model.ShowsCqOnly);

        // Everything except what is addressed to him, which is on the other side.
        Assert.True(OnTheLeft(model, "CQ TA3MPK KM39"));
        Assert.True(OnTheLeft(model, "KE9COB N5CH R+14"));
        Assert.True(OnTheLeft(model, "TNX FER QSO OM"));
        Assert.True(OnTheLeft(model, "W4WTM KD9ABC R-11"));

        Assert.False(OnTheLeft(model, "KD9ABC W4WTM -07"));
    }

    /// <summary>Pressing `everything` clears the toggle.</summary>
    /// <remarks>
    /// **IT IS NOT A SECOND CHOICE THAT COULD DISAGREE.** `everything` is what the
    /// toggle being off already is, so `ShowsEverything` is derived rather than
    /// stored, and the button that appears to select it clears the toggle.
    /// </remarks>
    [Fact]
    public void EverythingClearsTheToggle()
    {
        var model = WithRows(mine: "KD9ABC");

        model.ShowsCqOnly = true;

        model.ShowEveryDecodeCommand.Execute(null);

        Assert.False(model.ShowsCqOnly);
        Assert.True(model.ShowsEverything);

        // And again is a no-op rather than an error, because the control is never
        // disabled (§0.5.1).
        model.ShowEveryDecodeCommand.Execute(null);

        Assert.True(model.ShowsEverything);
    }

    /// <summary>The toggle is remembered between evenings.</summary>
    [Fact]
    public void TheToggleSurvivesTheSettingsFile()
    {
        var settings = new AppSettings();
        var model = new MainWindowViewModel(settings, null);

        model.ShowsCqOnly = true;

        Assert.True(settings.DecodedShowCq);

        var path = Path.Combine(
            Path.GetTempPath(),
            "hamlet-unit273-filter-" + Guid.NewGuid().ToString("N") + ".json");

        try
        {
            SettingsStore.SaveTo(settings, path);

            var reopened = new MainWindowViewModel(SettingsStore.LoadFrom(path), null);

            _output.WriteLine("reopened on CQ=" + reopened.ShowsCqOnly);

            Assert.True(reopened.ShowsCqOnly);
            Assert.False(reopened.ShowsEverything);
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>A unit 251 settings file keeps the filter it was left on.</summary>
    /// <remarks>
    /// **§6.1's SECOND EXCEPTION, STILL STANDING.** Unit 251 shipped
    /// `DecodedFilter` as one string and unit 252 migrated it into two keys. The
    /// `mine` half of that migration no longer restores a filter — there is no
    /// filter left for it to restore — but a file naming `CqOnly` must still come
    /// back on `CQ`, or an operator's stored answer is silently reset.
    /// </remarks>
    [Theory]
    [InlineData("CqOnly", true)]
    [InlineData("Mine", false)]
    [InlineData("Everything", false)]
    [InlineData("OnlyTheInterestingOnes", false)]
    public void AUnit251FileKeepsTheChoiceItWasLeftOn(string legacy, bool cq)
    {
        var path = Path.Combine(
            Path.GetTempPath(),
            "hamlet-unit251-legacy-" + Guid.NewGuid().ToString("N") + ".json");

        try
        {
            File.WriteAllText(path, "{ \"DecodedFilter\": \"" + legacy + "\" }");

            var reopened = new MainWindowViewModel(SettingsStore.LoadFrom(path), null);

            _output.WriteLine(legacy + " -> CQ=" + reopened.ShowsCqOnly);

            Assert.Equal(cq, reopened.ShowsCqOnly);
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>The summary counts what is shown and what is hidden.</summary>
    /// <remarks>
    /// **A ROW ON THE MINE SIDE IS NOT HIDDEN.** It is on the screen in its own
    /// column, so it is counted out of both — which is why the two shown counts
    /// and the hidden count add up to what was heard, and never overlap.
    /// </remarks>
    [Fact]
    public void TheSummaryCountsWhatIsShownAndWhatIsHidden()
    {
        var model = WithRows(mine: "KD9ABC");

        // Six heard, one of them addressed to him.
        Assert.Equal(6, model.DigitalDecodes.Count);
        Assert.Equal(1, model.DigitalMineCount);
        Assert.Equal(5, model.DigitalShownCount);
        Assert.Equal(0, model.DigitalHiddenCount);
        Assert.DoesNotContain("hidden", model.DigitalDecodedSummary);
        _output.WriteLine("everything : " + model.DigitalDecodedSummary);

        model.ShowsCqOnly = true;

        Assert.Equal(2, model.DigitalShownCount);
        Assert.Equal(3, model.DigitalHiddenCount);
        Assert.Equal(1, model.DigitalMineCount);
        Assert.Contains("2 shown", model.DigitalDecodedSummary);
        Assert.Contains("3 hidden by CQ", model.DigitalDecodedSummary);
        _output.WriteLine("CQ         : " + model.DigitalDecodedSummary);

        // The three counts account for every row that was heard.
        Assert.Equal(
            model.DigitalDecodes.Count,
            model.DigitalShownCount + model.DigitalHiddenCount + model.DigitalMineCount);
    }

    /// <summary>A row the toggle does not want never disturbs the left list.</summary>
    [Fact]
    public void AFilteredRowArrivingDoesNotDisturbTheList()
    {
        var model = WithRows(mine: "KD9ABC");

        model.ShowsCqOnly = true;

        var before = model.DigitalVisibleDecodes.ToArray();
        var changed = 0;

        model.DigitalVisibleDecodes.CollectionChanged += (_, _) => changed++;

        model.AddDecodeRowForTests(
            "214150", "-13", "0.3", "1620", "W1ABC K4XYZ RR73");

        Assert.Equal(0, changed);
        Assert.Equal(before, model.DigitalVisibleDecodes);

        // Still heard, and the summary says so rather than swallowing it.
        Assert.Equal(7, model.DigitalDecodes.Count);
        Assert.Equal(4, model.DigitalHiddenCount);

        model.AddDecodeRowForTests(
            "214150", "-08", "0.2", "980", "CQ K4XYZ FM18");

        Assert.Equal(1, changed);
    }

    /// <summary>The left list keeps the whole list's order, both ways round.</summary>
    [Fact]
    public void TheVisibleListKeepsTheWholeListsOrder()
    {
        foreach (var newestFirst in new[] { true, false })
        {
            var model = WithRows(mine: "KD9ABC");

            model.DigitalNewestFirst = newestFirst;
            model.ShowsCqOnly = true;

            model.AddDecodeRowForTests(
                "214150", "-08", "0.2", "980", "CQ K4XYZ FM18");
            model.AddDecodeRowForTests(
                "214150", "-13", "0.3", "1620", "W1ABC K4XYZ RR73");
            model.AddDecodeRowForTests(
                "214150", "-15", "0.1", "1100", "CQ DX VK3ABC QF22");

            var expected = model.DigitalDecodes
                .Where(r => DecodedFilterRule.IsCallToAnyone(r.Addressee))
                .ToArray();

            _output.WriteLine(
                "newest first " + newestFirst + " : "
                + string.Join(" | ", model.DigitalVisibleDecodes.Select(r => r.Message)));

            Assert.Equal(expected, model.DigitalVisibleDecodes);
        }
    }

    /// <summary>The predicate itself, over the shapes a to-field takes.</summary>
    /// <remarks>
    /// **THE `CQ POTA` ROW IS THE ONE THAT MATTERS**, and it takes two files to
    /// answer: `Ft8MessageSplit.Split` joins `CQ POTA W5LST EM33` into the single
    /// addressee `CQ POTA`, and `IsCallToAnyone` tests what that produced.
    /// </remarks>
    [Theory]
    [InlineData(true, "CQ", true)]
    [InlineData(true, "CQ DX", true)]
    [InlineData(true, "CQ POTA", true)]
    [InlineData(true, "cq", true)]
    [InlineData(true, "KD9ABC", false)]
    [InlineData(true, "", false)]
    [InlineData(false, "", true)]
    [InlineData(false, "KE9COB", true)]
    public void ThePredicateReadsTheToField(bool cq, string to, bool wanted)
    {
        var got = DecodedFilterRule.Wants(cq, to);

        _output.WriteLine(
            "CQ=" + cq + " / [" + to + "] -> " + (got ? "shown" : "held back"));

        Assert.Equal(wanted, got);
    }

    /// <summary>His portable and compound calls are his.</summary>
    /// <remarks>
    /// <para>**STILL LIVE, AND ASKED OF THE ENGINE NOW.** The rule moved to
    /// `Ft8MessageSplit.IsSameStation` in unit 271 and both sides of the split ask
    /// it — the mine side through `IsAddressedTo`, and the contact column through
    /// the same. `DecodedFilterRule.IsSameStation` is the one-line forward the app
    /// still calls it by.</para>
    /// <para>**`W1ABCD` IS THE CASE THAT DECIDES IT.** A prefix match would claim
    /// another station's traffic as his, which is §0.0's fault wearing a helpful
    /// face.</para>
    /// </remarks>
    [Theory]
    [InlineData("W1ABC", "W1ABC", true)]
    [InlineData("W1ABC", "w1abc", true)]
    [InlineData("W1ABC", "W1ABC/P", true)]
    [InlineData("W1ABC", "W4/W1ABC", true)]
    [InlineData("W4/W1ABC", "W1ABC", true)]
    [InlineData("W1ABC", "W1ABCD", false)]
    [InlineData("W1ABC", "W1AB", false)]
    [InlineData("W1ABC", "KD9ABC", false)]
    [InlineData("W1ABC", "", false)]
    public void HisPortableAndCompoundCallsAreHis(
        string stored, string heard, bool his)
    {
        var got = DecodedFilterRule.IsSameStation(heard, stored);

        _output.WriteLine(
            "stored [" + stored + "] heard [" + heard + "] -> "
            + (got ? "his" : "somebody else"));

        Assert.Equal(his, got);

        // And the engine agrees, because the app is a forward to it.
        Assert.Equal(his, Ft8MessageSplit.IsSameStation(heard, stored));
    }

    private static bool OnTheLeft(MainWindowViewModel model, string message)
    {
        Assert.Contains(model.DigitalDecodes, r => r.Message == message);

        return model.DigitalVisibleDecodes.Any(r => r.Message == message);
    }

    private void Print(MainWindowViewModel model)
    {
        foreach (var row in model.DigitalDecodes)
        {
            var where = model.DigitalMineDecodes.Contains(row)
                ? "  mine  "
                : model.DigitalVisibleDecodes.Contains(row) ? "  left  " : "  hidden ";

            _output.WriteLine(where + row.Message);
        }

        _output.WriteLine("summary : " + model.DigitalDecodedSummary);
    }

    /// <summary>Six rows covering every shape the predicate has to read.</summary>
    private static MainWindowViewModel WithRows(string mine)
    {
        var settings = new AppSettings();

        settings.Operator.Callsign = mine;

        var model = new MainWindowViewModel(settings, null);

        var rows = new[]
        {
            ("CQ TA3MPK KM39", "-11"),
            ("CQ DX EA3QQ JN11", "-17"),

            // Two other stations: neither a CQ nor his.
            ("KE9COB N5CH R+14", "-04"),

            // Addressed to him: the one row that belongs on the other side.
            ("KD9ABC W4WTM -07", "-09"),

            // Sent BY him, which is not addressed to him and stays on the left.
            ("W4WTM KD9ABC R-11", "-13"),

            // Free text: no three fields at all.
            ("TNX FER QSO OM", "-06"),
        };

        foreach (var (message, snr) in rows)
        {
            model.AddDecodeRowForTests("214135", snr, "0.2", "1240", message);
        }

        return model;
    }
}
