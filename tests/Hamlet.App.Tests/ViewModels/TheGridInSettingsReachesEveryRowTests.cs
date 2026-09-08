using System;
using System.Linq;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 281, task 6: the grid that is there and was not found.
/// </summary>
/// <remarks>
/// <para>**OBSERVED 2026-09-08**, on his own CQ: *…and Hamlet needs your own grid
/// square in Settings before it can say how far away that is.* **It is `FN00DJ`,
/// marked verified in Settings from callook.info on 2026-08-14.**</para>
/// <para>**THE CAUSE, WITH FILE AND LINE**: `MainWindowViewModel.KeepSentRow` builds
/// its row through `DigitalDecodeRow.Sent`, which passes `ObserverGrid: ""`, and it
/// does not call `PlaceRow` — which is where the grid was read. `PlaceRow`'s own
/// remarks have called themselves *the one place the operator's own grid reaches a
/// row* since unit 252, and that stopped being true the moment a second row builder
/// was added.</para>
/// <para>**THIS IS THE THIRD OF THAT SHAPE.** The menu on the wrong list, the Log
/// item gated where it could never fire, and now this: a second construction site
/// added later that does not go through the door the first one uses, with the value
/// plainly present the whole time.</para>
/// <para>**AND IT ASSERTS BOTH SIDES.** A received row is checked as well as a sent
/// one, because the point of the repair is that there is one door and not that one
/// more caller remembered.</para>
/// </remarks>
public sealed class TheGridInSettingsReachesEveryRowTests
{
    private const string HisGrid = "FN00DJ";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the tooltips are printed.</param>
    public TheGridInSettingsReachesEveryRowTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>**A received message's tooltip carries a distance and a bearing.**</summary>
    /// <remarks>
    /// The instruction's own criterion: with a grid in Settings the tooltip does not
    /// mention Settings at all.
    /// </remarks>
    [Fact]
    public void AReceivedMessageCarriesADistanceAndNeverMentionsSettings()
    {
        var panel = Panel(HisGrid);

        var row = panel.AddDecodeRowForTests(
            "214130", "-11", "0.2", "1240", "CQ W3YNI FN20",
            new DateTime(2026, 9, 8, 21, 41, 30, DateTimeKind.Utc), 14_074_000);

        _output.WriteLine("grid on the row : [" + row.ObserverGrid + "]");
        _output.WriteLine("the tooltip     : " + row.PayloadHelp);

        Assert.Equal(HisGrid, row.ObserverGrid);

        Assert.Contains("miles away", row.PayloadHelp, StringComparison.Ordinal);
        Assert.Contains("bearing of", row.PayloadHelp, StringComparison.Ordinal);

        // **AND NOT A WORD ABOUT A SETTING HE HAS ALREADY SET.**
        Assert.DoesNotContain("Settings", row.PayloadHelp, StringComparison.Ordinal);
        Assert.DoesNotContain("grid square in", row.PayloadHelp, StringComparison.Ordinal);
    }

    /// <summary>**A sent row gets the grid too, which is where the fault was.**</summary>
    /// <remarks>
    /// Watched failing first: `DigitalDecodeRow.Sent` passes `ObserverGrid: ""` and
    /// `KeepSentRow` does not call `PlaceRow`, so this row carried a blank grid while
    /// Settings held one. Task 5 gives a sent row no tooltip, so the symptom is out
    /// of sight either way — **this asserts the cause**, because a row that carries a
    /// blank where a value exists is the fault, and the next thing to read the field
    /// would inherit it.
    /// </remarks>
    [Fact]
    public void ASentRowCarriesTheOperatorsGridAsWell()
    {
        var panel = Panel(HisGrid);

        var row = panel.AddSentRowForTests(
            "CQ KC3QIS FN00", new DateTime(2026, 9, 8, 21, 41, 45, DateTimeKind.Utc));

        _output.WriteLine("grid on the sent row : [" + row.ObserverGrid + "]");

        Assert.Equal(HisGrid, row.ObserverGrid);
    }

    /// <summary>**With no grid set, the sentence still asks for one.**</summary>
    /// <remarks>
    /// §0.0 the other way round. The repair must not make Hamlet claim a distance it
    /// cannot measure; the sentence that asks for a grid is right whenever there is
    /// genuinely no grid, and it is only wrong when there is one.
    /// </remarks>
    [Fact]
    public void WithNoGridTheSentenceStillAsksForOne()
    {
        var panel = Panel("");

        var row = panel.AddDecodeRowForTests(
            "214130", "-11", "0.2", "1240", "CQ W3YNI FN20",
            new DateTime(2026, 9, 8, 21, 41, 30, DateTimeKind.Utc), 14_074_000);

        _output.WriteLine(row.PayloadHelp);

        Assert.Equal("", row.ObserverGrid);
        Assert.Contains("grid square in Settings", row.PayloadHelp, StringComparison.Ordinal);
        Assert.DoesNotContain("miles away", row.PayloadHelp, StringComparison.Ordinal);
    }

    /// <summary>**A grid typed in Settings shows up on the next row.**</summary>
    /// <remarks>
    /// The read is fresh rather than captured at construction, which is what makes
    /// the setting take effect without a restart. Asserted because a fix that cached
    /// it would pass the tests above and fail the operator.
    /// </remarks>
    [Fact]
    public void AGridTypedAfterwardsReachesTheNextRow()
    {
        var settings = new AppSettings();

        settings.Operator.Callsign = "KC3QIS";
        settings.Operator.GridSquare = "";

        var panel = new MainWindowViewModel(settings, null)
        {
            OperatingMode = "Digital",
        };

        var before = panel.AddDecodeRowForTests(
            "214130", "-11", "0.2", "1240", "CQ W3YNI FN20",
            new DateTime(2026, 9, 8, 21, 41, 30, DateTimeKind.Utc), 14_074_000);

        settings.Operator.GridSquare = HisGrid;

        var after = panel.AddDecodeRowForTests(
            "214145", "-11", "0.2", "1240", "CQ W3YNI FN20",
            new DateTime(2026, 9, 8, 21, 41, 45, DateTimeKind.Utc), 14_074_000);

        _output.WriteLine("before : [" + before.ObserverGrid + "]");
        _output.WriteLine("after  : [" + after.ObserverGrid + "]");

        Assert.Equal("", before.ObserverGrid);
        Assert.Equal(HisGrid, after.ObserverGrid);
    }

    /// <summary>**No row on any bound list is missing the grid.**</summary>
    /// <remarks>
    /// <para>**THE GUARD THAT SURVIVES A FOURTH BUILDER** (work instruction 282 task
    /// 6). Unit 281 fixed the one construction site that bypassed the door, and
    /// counting call sites is exactly the check that passed for six units while
    /// `KeepSentRow` sat outside it. This asks the lists instead: whatever built a
    /// row, if it reached the screen without the operator's grid it is here.</para>
    /// <para>Confirmed 2026-09-08 against the tree: three sites build a row and all
    /// three go through `WithOperatorGrid`, so this is a ratchet rather than a
    /// repair.</para>
    /// </remarks>
    [Fact]
    public void NoRowOnAnyBoundListIsMissingTheGrid()
    {
        var panel = Panel(HisGrid);
        var slot = new DateTime(2026, 9, 8, 21, 41, 30, DateTimeKind.Utc);

        panel.AddDecodeRowForTests(
            "214130", "-11", "0.2", "1240", "CQ W3YNI FN20", slot, 14_074_000);
        panel.AddDecodeRowForTests(
            "214130", "-13", "0.3", "1310", "KC3QIS W3YNI +02", slot, 14_074_000);
        panel.AddSentRowForTests("KC3QIS W3YNI R-09", slot.AddSeconds(15));

        var lists = new (string Name, System.Collections.Generic.IEnumerable<DigitalDecodeRow> Rows)[]
        {
            ("DigitalDecodes", panel.DigitalDecodes),
            ("DigitalVisibleDecodes", panel.DigitalVisibleDecodes),
            ("DigitalMineDecodes", panel.DigitalMineDecodes),
        };

        var seen = 0;
        var blank = new System.Collections.Generic.List<string>();

        foreach (var (name, rows) in lists)
        {
            foreach (var row in rows)
            {
                seen++;

                _output.WriteLine(
                    name.PadRight(22) + "[" + row.ObserverGrid + "]  " + row.Message);

                if (row.ObserverGrid != HisGrid)
                {
                    blank.Add(name + ": " + row.Message);
                }
            }
        }

        // **A SWEEP THAT SWEPT NOTHING HAS NOT PASSED** (unit 279's rule).
        Assert.True(seen > 0, "no rows reached any bound list, so nothing was checked");

        Assert.True(
            blank.Count == 0,
            "these rows reached the screen without the operator's grid: "
            + string.Join(" | ", blank));
    }

    /// <summary>A panel with a grid in its settings.</summary>
    /// <param name="grid">What Settings holds.</param>
    /// <returns>The panel.</returns>
    private static MainWindowViewModel Panel(string grid)
    {
        var settings = new AppSettings();

        settings.Operator.Callsign = "KC3QIS";
        settings.Operator.GridSquare = grid;

        return new MainWindowViewModel(settings, null)
        {
            OperatingMode = "Digital",
        };
    }
}
