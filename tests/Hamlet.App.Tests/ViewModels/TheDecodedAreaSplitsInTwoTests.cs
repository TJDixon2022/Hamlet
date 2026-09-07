using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 273, tasks 1, 2 and 4: the decoded area is two lists, and
/// anything addressed to the operator is on the right and can never be buried.
/// </summary>
/// <remarks>
/// <para>**WHY THE SPLIT** (Tim's ruling, 2026-09-07: *that way I am always sure
/// that I am seeing what is for me*). `mine` and `CQ` competed for one list, so
/// choosing either lost sight of the other, and on a busy band a message addressed
/// to him landed among fifty that were not.</para>
/// <para>**A MESSAGE IS ON EXACTLY ONE SIDE**, which is the assertion this file
/// exists for. Two copies of one message is how a table starts disagreeing with
/// itself, and each summary would then be counting something that overlaps the
/// other.</para>
/// </remarks>
public sealed class TheDecodedAreaSplitsInTwoTests
{
    /// <summary>The operator's own callsign, as Settings has it on his machine.</summary>
    private const string HisCall = "KC3QIS";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the two sides are printed.</param>
    public TheDecodedAreaSplitsInTwoTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// A slot with a CQ, a third-party exchange and a message to the operator
    /// puts exactly one row on the right and the other two on the left.
    /// </summary>
    /// <remarks>
    /// **THE INSTRUCTION'S OWN THREE**, and the shape unit 271 used for the
    /// contact column, because the split asks the same question that column does.
    /// </remarks>
    [Fact]
    public void OneSlotGoesToBothSidesAndNoMessageToBoth()
    {
        var model = WithRows(
            "CQ VP2MAA FK52",
            "K9TC KJ6IX RRR",
            HisCall + " W1ABC -12");

        Print(model);

        Assert.Equal(
            new[] { "CQ VP2MAA FK52", "K9TC KJ6IX RRR" },
            model.DigitalVisibleDecodes.Select(r => r.Message).OrderBy(m => m).ToArray());

        Assert.Equal(
            new[] { HisCall + " W1ABC -12" },
            model.DigitalMineDecodes.Select(r => r.Message).ToArray());

        // **NOTHING ON BOTH**, asserted as an intersection rather than inferred
        // from the two counts agreeing, because two counts can agree while the
        // same row sits in both lists and a third has gone missing.
        Assert.Empty(
            model.DigitalVisibleDecodes.Intersect(model.DigitalMineDecodes));

        // And every row that was heard is on one side or the other.
        Assert.Equal(3, model.DigitalShownCount + model.DigitalMineCount);
        Assert.Equal(0, model.DigitalHiddenCount);
    }

    /// <summary>A CQ is never on the mine side, whoever sent it.</summary>
    /// <remarks>
    /// **A CQ IS AN INVITATION AND NOT A MESSAGE TO HIM.** It is the same ruling
    /// the contact column follows, and it is asked of the same predicate.
    /// </remarks>
    [Theory]
    [InlineData("CQ VP2MAA FK52")]
    [InlineData("CQ DX VP2MAA FK52")]
    [InlineData("CQ POTA W5LST EM33")]
    public void ACqIsNeverOnTheMineSide(string message)
    {
        var model = WithRows(message);

        Assert.Empty(model.DigitalMineDecodes);
        Assert.Single(model.DigitalVisibleDecodes);
    }

    /// <summary>His portable and compound calls are addressed to him.</summary>
    [Theory]
    [InlineData("KC3QIS W1ABC -12")]
    [InlineData("KC3QIS/P W1ABC -12")]
    [InlineData("W4/KC3QIS W1ABC -12")]
    public void HisCompoundFormsAreHis(string message)
    {
        var model = WithRows(message);

        _output.WriteLine(message + " -> mine: " + model.DigitalMineCount);

        Assert.Single(model.DigitalMineDecodes);
        Assert.Empty(model.DigitalVisibleDecodes);
    }

    /// <summary>A message he sent is not on the mine side.</summary>
    /// <remarks>
    /// **THE SIDE IS WHAT IS ADDRESSED TO HIM** (Tim's ruling), which is narrower
    /// than unit 252's `mine` toggle: that matched either field, because he asked
    /// there for *his traffic* and a contact is two sides. A message he sent is
    /// still on the left, where it belongs among what was heard.
    /// </remarks>
    [Fact]
    public void AMessageHeSentIsNotOnTheMineSide()
    {
        var model = WithRows("W1ABC " + HisCall + " R-11");

        Assert.Empty(model.DigitalMineDecodes);
        Assert.Single(model.DigitalVisibleDecodes);
    }

    /// <summary>The mine side is never filtered by the CQ toggle.</summary>
    /// <remarks>
    /// **NO CONTROL CAN HIDE IT**, which is the whole point of the ruling. Turning
    /// `CQ` on takes the third-party rows off the left and leaves the right
    /// exactly as it was.
    /// </remarks>
    [Fact]
    public void TheCqToggleNeverTouchesTheMineSide()
    {
        var model = WithRows(
            "CQ VP2MAA FK52",
            "K9TC KJ6IX RRR",
            HisCall + " W1ABC -12");

        Assert.Single(model.DigitalMineDecodes);

        model.ShowsCqOnly = true;

        Print(model);

        Assert.Single(model.DigitalMineDecodes);
        Assert.Equal(
            new[] { "CQ VP2MAA FK52" },
            model.DigitalVisibleDecodes.Select(r => r.Message).ToArray());

        model.ShowEveryDecodeCommand.Execute(null);

        Assert.Single(model.DigitalMineDecodes);
        Assert.Equal(2, model.DigitalShownCount);
    }

    /// <summary>Both sides keep the whole list's order, both ways round.</summary>
    [Fact]
    public void BothSidesKeepTheWholeListsOrder()
    {
        foreach (var newestFirst in new[] { true, false })
        {
            var model = WithRows(
                "CQ VP2MAA FK52",
                HisCall + " W1ABC -12",
                "K9TC KJ6IX RRR");

            model.DigitalNewestFirst = newestFirst;

            model.AddDecodeRowForTests(
                "214150", "-09", "0.2", "1300", HisCall + " N5CH RRR");
            model.AddDecodeRowForTests(
                "214150", "-14", "0.3", "1450", "CQ K4XYZ FM18");

            var expectedMine = model.DigitalDecodes
                .Where(r => model.DigitalMineDecodes.Contains(r)).ToArray();
            var expectedLeft = model.DigitalDecodes
                .Where(r => model.DigitalVisibleDecodes.Contains(r)).ToArray();

            _output.WriteLine(
                "newest first " + newestFirst + " mine: "
                + string.Join(" | ", model.DigitalMineDecodes.Select(r => r.Message)));

            Assert.Equal(expectedMine, model.DigitalMineDecodes);
            Assert.Equal(expectedLeft, model.DigitalVisibleDecodes);
        }
    }

    /// <summary>
    /// Task 2: with nothing addressed to him, the right side carries its line
    /// and no rows.
    /// </summary>
    /// <remarks>
    /// **EMPTY, WITH A LINE SAYING SO, AND NEVER BLANK.** A blank column says
    /// nothing about whether it is working, and this is the column he will be
    /// watching hardest.
    /// </remarks>
    [Fact]
    public void TheMineSideSaysSoWhenNobodyHasCalled()
    {
        var model = WithRows("CQ VP2MAA FK52", "K9TC KJ6IX RRR");

        _output.WriteLine("idle    : " + model.DigitalMineIdle);
        _output.WriteLine("summary : " + model.DigitalMineSummary);

        Assert.Empty(model.DigitalMineDecodes);
        Assert.False(model.HasDigitalMineDecodes);

        Assert.NotEqual("", model.DigitalMineIdle);
        Assert.Contains("Nothing addressed to you", model.DigitalMineIdle);

        // The header says it too, because a collapsed panel still carries its
        // summary (§0.5, HM-DEC-021).
        Assert.Equal("nothing for you yet", model.DigitalMineSummary);
    }

    /// <summary>With no callsign on file it says that, rather than blaming the band.</summary>
    /// <remarks>
    /// **NOTHING CAN BE ADDRESSED TO A CALLSIGN THE APP HAS NEVER BEEN TOLD**, so
    /// *nobody has called you* would be a claim about the band when the truth is a
    /// gap in Settings (§0.0).
    /// </remarks>
    [Fact]
    public void WithNoCallsignItNamesSettingsRatherThanTheBand()
    {
        var settings = new AppSettings();

        // **CLEARED ON PURPOSE, BECAUSE A FRESH PROFILE IS NOT EMPTY.**
        // `OperatorProfile.Callsign` defaults to `KC3QIS`, so the no-callsign path
        // is reached only where he has cleared the field himself. The first draft
        // of this test assumed a blank default and was wrong about the tree rather
        // than about the code.
        settings.Operator.Callsign = "";

        var model = new MainWindowViewModel(settings, null);

        model.AddDecodeRowForTests("214135", "-11", "0.2", "1240", "CQ VP2MAA FK52");

        _output.WriteLine(model.DigitalMineIdle);

        Assert.Empty(model.DigitalMineDecodes);
        Assert.Contains("does not know your callsign", model.DigitalMineIdle);
        Assert.Contains("Settings", model.DigitalMineIdle);
        Assert.DoesNotContain("Nothing addressed to you", model.DigitalMineIdle);
    }

    /// <summary>The summary counts what is for him.</summary>
    [Fact]
    public void TheMineSummaryCountsWhatIsForHim()
    {
        var model = WithRows(HisCall + " W1ABC -12");

        Assert.Equal("1 for you", model.DigitalMineSummary);

        model.AddDecodeRowForTests(
            "214150", "-09", "0.2", "1300", HisCall + " N5CH RRR");

        _output.WriteLine(model.DigitalMineSummary);

        Assert.Equal("2 for you", model.DigitalMineSummary);
    }

    /// <summary>Task 4: one clear empties both sides.</summary>
    /// <remarks>
    /// **ONE EVENING, ONE CLEAR** (the instruction's own words). A clear that left
    /// one side populated would leave the panel describing two different evenings.
    /// </remarks>
    [Fact]
    public void ClearEmptiesBothSides()
    {
        var model = WithRows(
            "CQ VP2MAA FK52",
            "K9TC KJ6IX RRR",
            HisCall + " W1ABC -12");

        Assert.NotEmpty(model.DigitalVisibleDecodes);
        Assert.NotEmpty(model.DigitalMineDecodes);

        model.ClearDigitalDecodesCommand.Execute(null);

        Assert.Empty(model.DigitalDecodes);
        Assert.Empty(model.DigitalVisibleDecodes);
        Assert.Empty(model.DigitalMineDecodes);

        Assert.Equal(0, model.DigitalShownCount);
        Assert.Equal(0, model.DigitalMineCount);
        Assert.Equal(0, model.DigitalHiddenCount);

        // And the right side says it is empty again rather than going blank.
        Assert.Equal("nothing for you yet", model.DigitalMineSummary);
    }

    /// <summary>The contact column has something to say on the mine side.</summary>
    /// <remarks>
    /// **WHICH IS WHY THE COLUMN MOVED THERE** (task 1). Unit 271 made it speak
    /// only about contacts he is in, so on the left it was blank on nearly every
    /// row; these are the only rows it has anything to say about.
    /// </remarks>
    [Fact]
    public void TheContactColumnSpeaksOnTheMineSide()
    {
        var settings = new AppSettings();

        settings.Operator.Callsign = HisCall;

        var model = new MainWindowViewModel(settings, null);
        var slot = new DateTime(2026, 9, 7, 21, 41, 30, DateTimeKind.Utc);

        model.AddDecodeRowForTests(
            "214130", "-12", "0.2", "1240", HisCall + " W1ABC -12", slot);
        model.AddDecodeRowForTests(
            "214130", "-11", "0.2", "1290", "K9TC KJ6IX RRR", slot);

        var mine = Assert.Single(model.DigitalMineDecodes);
        var left = Assert.Single(model.DigitalVisibleDecodes);

        _output.WriteLine("mine : [" + mine.Contact + "]");
        _output.WriteLine("left : [" + left.Contact + "]");

        Assert.NotEqual("", mine.Contact);
        Assert.Equal("", left.Contact);
    }

    private void Print(MainWindowViewModel model)
    {
        foreach (var row in model.DigitalDecodes)
        {
            var where = model.DigitalMineDecodes.Contains(row)
                ? "  RIGHT "
                : model.DigitalVisibleDecodes.Contains(row) ? "  left  " : "  hidden";

            _output.WriteLine(where + "  " + row.Message);
        }

        _output.WriteLine("left summary : " + model.DigitalDecodedSummary);
        _output.WriteLine("mine summary : " + model.DigitalMineSummary);
    }

    private static MainWindowViewModel WithRows(params string[] messages)
    {
        var settings = new AppSettings();

        settings.Operator.Callsign = HisCall;

        var model = new MainWindowViewModel(settings, null);

        foreach (var message in messages)
        {
            model.AddDecodeRowForTests("214135", "-11", "0.2", "1240", message);
        }

        return model;
    }
}
