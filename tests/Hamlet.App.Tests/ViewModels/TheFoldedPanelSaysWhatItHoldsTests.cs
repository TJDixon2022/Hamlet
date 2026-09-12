using Hamlet.App.Controls;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// A collapsed digital panel says what is in it and that a click opens it
/// (`PHASE_PLAN.md` §R17, work instruction 325 task 1).
/// </summary>
/// <remarks>
/// <para>**THIS IS THE EVENING TIM DECIDED FT8 HAD STOPPED WORKING.** It had
/// not. Thirty-one rows were arriving into a panel he had collapsed two seconds
/// earlier, and the header said nothing that would tell him so. A picture binds
/// as hard as a sentence (§0.0, HM-DEC-092), and a folded panel that looks empty
/// is a false claim about the band.</para>
/// <para>**EVERY ASSERTION HERE IS COMPUTED, NOT SEEN.** These are ViewModel
/// strings and a brush, read headless. Nothing in this file proves a pixel.</para>
/// </remarks>
public sealed class TheFoldedPanelSaysWhatItHoldsTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the two headers are printed.</param>
    public TheFoldedPanelSaysWhatItHoldsTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>The folded decoded header counts the rows it is holding.</summary>
    [Fact]
    public void TheCountInTheHeaderMatchesTheRows()
    {
        var model = WithRows(3);

        model.DigitalDecodedExpanded = false;

        _output.WriteLine("folded : " + model.DigitalDecodedSummary);

        Assert.Equal(model.DigitalShownCount, RowsOnTheLeft(model));
        Assert.Contains(
            model.DigitalShownCount + " stations decoded", model.DigitalDecodedSummary);
        Assert.Contains("click to show them", model.DigitalDecodedSummary);
    }

    /// <summary>The count ticks as a slot lands, with the panel still shut.</summary>
    [Fact]
    public void ItChangesWhenASlotLands()
    {
        var model = WithRows(3);

        model.DigitalDecodedExpanded = false;

        Assert.Contains("3 stations decoded", model.DigitalDecodedSummary);

        model.AddDecodeRowForTests("214150", "-08", "0.2", "980", "CQ K4XYZ FM18");
        model.AddDecodeRowForTests("214150", "-12", "0.2", "1180", "CQ N0ABC EN34");

        _output.WriteLine("after a slot : " + model.DigitalDecodedSummary);

        Assert.Contains("5 stations decoded", model.DigitalDecodedSummary);
    }

    /// <summary>One row is *station*, not *stations*.</summary>
    [Fact]
    public void OneRowReadsSingular()
    {
        var model = WithRows(1);

        model.DigitalDecodedExpanded = false;

        _output.WriteLine("one : " + model.DigitalDecodedSummary);

        Assert.Contains("1 station decoded", model.DigitalDecodedSummary);
        Assert.DoesNotContain("1 stations", model.DigitalDecodedSummary);
    }

    /// <summary>The *For you* header names stations calling, in the same shape.</summary>
    [Fact]
    public void TheForYouHeaderSaysWhoIsCallingHim()
    {
        var model = WithRows(2, mine: "KD9ABC");

        model.AddDecodeRowForTests("214150", "-09", "0.2", "1240", "KD9ABC W4WTM -07");

        // **THE PANEL BEING ASKED ABOUT IS THE ONE THAT FOLDS, SINCE UNIT 327.** The
        // two panels shared `DigitalDecodedExpanded` until then, so this folded
        // *Decoded text* and read the header of *For you* - which is the fault unit
        // 325 item 6 raised, standing in this test as a coincidence that passed.
        model.DigitalMineExpanded = false;

        _output.WriteLine("for you : " + model.DigitalMineSummary);

        // **AND ITS NEIGHBOUR IS UNTOUCHED**, which is the whole point of the split.
        Assert.True(model.DigitalDecodedExpanded);
        Assert.DoesNotContain("click to show", model.DigitalDecodedSummary);

        Assert.Equal(1, model.DigitalMineStationCount);
        Assert.Contains("1 station calling you", model.DigitalMineSummary);
        Assert.Contains("click to show", model.DigitalMineSummary);

        // A second caller, and it is two stations rather than two messages.
        model.AddDecodeRowForTests("214205", "-14", "0.2", "1610", "KD9ABC VE3XYZ FN03");
        model.AddDecodeRowForTests("214205", "-14", "0.2", "1610", "KD9ABC VE3XYZ R-14");

        _output.WriteLine("for you : " + model.DigitalMineSummary);

        Assert.Equal(2, model.DigitalMineStationCount);
        Assert.Contains("2 stations calling you", model.DigitalMineSummary);
    }

    /// <summary>Opened, both headers go back to their working summaries.</summary>
    [Fact]
    public void OpenedItIsTheOrdinarySummaryAgain()
    {
        var model = WithRows(3, mine: "KD9ABC");

        model.AddDecodeRowForTests("214150", "-09", "0.2", "1240", "KD9ABC W4WTM -07");

        Assert.True(model.DigitalDecodedExpanded);

        _output.WriteLine("open : " + model.DigitalDecodedSummary);
        _output.WriteLine("open : " + model.DigitalMineSummary);

        Assert.DoesNotContain("click to show", model.DigitalDecodedSummary);
        Assert.DoesNotContain("click to show", model.DigitalMineSummary);
        Assert.Contains("shown", model.DigitalDecodedSummary);
        Assert.Contains("for you", model.DigitalMineSummary);
    }

    /// <summary>The filter's held-back count survives the folded wording.</summary>
    /// <remarks>
    /// **THIS IS THE §0.0 GUARD AND IT IS THE REASON THE SENTENCE IS NOT JUST A
    /// COUNT.** A filter that removes rows can make a busy band look like a
    /// quiet one, and a shut panel is exactly where that mistake gets made.
    /// </remarks>
    [Fact]
    public void TheFoldedHeaderStillSaysWhatTheFilterIsHolding()
    {
        var model = WithRows(3, mine: "KD9ABC");

        model.AddDecodeRowForTests("214150", "-04", "0.2", "1400", "KE9COB N5CH R+14");
        model.ShowsCqOnly = true;

        model.DigitalDecodedExpanded = false;

        _output.WriteLine("folded, filtered : " + model.DigitalDecodedSummary);

        Assert.True(model.DigitalHiddenCount > 0);
        Assert.Contains("hidden by CQ", model.DigitalDecodedSummary);
    }

    /// <summary>Folded with content, the summary takes the decode family's ink.</summary>
    /// <remarks>
    /// **SHAPE AND WORDS CARRY IT FIRST** (§0.6). The color is checked here only
    /// to prove it is the family's own value from `PanelPalette` and not a
    /// literal, and that it is gone the moment the panel opens.
    /// </remarks>
    [Fact]
    public void TheHeaderTakesTheFamilyColorOnlyWhileFoldedWithContent()
    {
        var model = WithRows(3);

        Assert.False(model.DigitalIsFoldedWithContent);

        model.DigitalDecodedExpanded = false;

        Assert.True(model.DigitalIsFoldedWithContent);
        Assert.Same(PanelPalette.Green.TitleBrush, model.DigitalFoldedInk);

        model.DigitalDecodedExpanded = true;

        Assert.False(model.DigitalIsFoldedWithContent);
        Assert.NotSame(PanelPalette.Green.TitleBrush, model.DigitalFoldedInk);
    }

    /// <summary>An empty folded panel does not take the family ink.</summary>
    [Fact]
    public void AnEmptyFoldedPanelIsNotColored()
    {
        var model = new MainWindowViewModel(new AppSettings(), null);

        model.DigitalDecodedExpanded = false;

        _output.WriteLine("empty, folded : " + model.DigitalDecodedSummary);

        Assert.False(model.DigitalIsFoldedWithContent);
        Assert.NotSame(PanelPalette.Green.TitleBrush, model.DigitalFoldedInk);
        Assert.DoesNotContain("click to show", model.DigitalDecodedSummary);
    }

    /// <summary>Neither panel opens collapsed, whatever was remembered.</summary>
    /// <remarks>
    /// **THE TWO PANELS SHARE ONE EXPANDED FLAG** in the window, so one
    /// assertion covers both. If they are ever split, this test splits with
    /// them.
    /// </remarks>
    [Fact]
    public void NeitherPanelOpensCollapsedWhateverWasRemembered()
    {
        var settings = new AppSettings();
        var model = new MainWindowViewModel(settings, null);

        model.DigitalDecodedExpanded = false;

        var path = Path.Combine(
            Path.GetTempPath(),
            "hamlet-unit325-folded-" + Guid.NewGuid().ToString("N") + ".json");

        try
        {
            SettingsStore.SaveTo(settings, path);

            var reopened = new MainWindowViewModel(SettingsStore.LoadFrom(path), null);

            _output.WriteLine(
                "remembered shut, reopened expanded=" + reopened.DigitalDecodedExpanded);

            Assert.True(reopened.DigitalDecodedExpanded);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static int RowsOnTheLeft(MainWindowViewModel model)
        => model.DigitalVisibleDecodes.Count;

    /// <summary>A window holding `count` calls to anyone.</summary>
    private static MainWindowViewModel WithRows(int count, string mine = "KD9ABC")
    {
        var settings = new AppSettings();

        settings.Operator.Callsign = mine;

        var model = new MainWindowViewModel(settings, null);

        var calls = new[] { "TA3MPK KM39", "EA3QQ JN11", "VK3ABC QF22", "G0XYZ IO91" };

        for (var i = 0; i < count; i++)
        {
            model.AddDecodeRowForTests(
                "2141" + (35 + i).ToString("00"),
                "-1" + i,
                "0.2",
                (1000 + (i * 120)).ToString(),
                "CQ " + calls[i % calls.Length]);
        }

        return model;
    }
}
