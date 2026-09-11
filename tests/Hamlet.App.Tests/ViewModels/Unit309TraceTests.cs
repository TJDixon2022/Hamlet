using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Explore;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 309 task 1: **two screenshots turned into two mechanisms.**
/// </summary>
/// <remarks>
/// <para>**A FIX WRITTEN BEFORE THIS FINISHES IS A GUESS.** Both faults were asserted
/// green by unit 308 and neither is on the operator's screen, which is inbound ask 2
/// arriving with a bill: nothing in this repository can look at a picture, so a
/// control placed in a visual tree and a property returning the right words were both
/// asserted correctly while the screen showed neither.</para>
/// <para>**THIS TYPE MEASURES AND ASSERTS NOTHING IT HOPES FOR.** Where a value is
/// wrong it says so; where it is right it says that too, because *right in the model
/// and absent on the screen* is a different fault from *never computed*.</para>
/// </remarks>
public sealed class Unit309TraceTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the measurements are printed.</param>
    public Unit309TraceTests(ITestOutputHelper output) => _output = output;

    /// <summary>**The card on his screen: RD6OB in KN98, from FN00DJ.**</summary>
    [Fact]
    public void WhatTheCardOnHisScreenComputes()
    {
        var plot = new Ft8GlobePlot("FN00DJ", "KN98", "RD6OB", "European Russia");

        _output.WriteLine("HasOperator     : " + plot.HasOperator);
        _output.WriteLine("HasStation      : " + plot.HasStation);
        _output.WriteLine("HasPath         : " + plot.HasPath);
        _output.WriteLine("operator at     : " + At(plot.OperatorX, plot.OperatorY));
        _output.WriteLine("station at      : " + At(plot.StationX, plot.StationY));
        _output.WriteLine("path runs       : " + plot.Path.Count);
        _output.WriteLine(
            "path vertices   : " + plot.Path.Sum(r => r.Count));
        _output.WriteLine("caption         : " + plot.Caption);

        // **THE STATION IS WHERE THE INSTRUCTION SAYS IT IS**, which confirms the
        // projection and the marker drawing both work.
        Assert.True(plot.HasStation);
        Assert.Equal(399.2, plot.StationX, 1);
        Assert.Equal(113.9, plot.StationY, 1);

        // **AND SO IS THE OPERATOR, IN THE MODEL.** If this passes, the marker and
        // the path are computed and the fault is downstream of the view model.
        Assert.True(plot.HasOperator);
        Assert.Equal(178.3, plot.OperatorX, 1);
        Assert.Equal(133.1, plot.OperatorY, 1);

        Assert.True(plot.HasPath);
        Assert.NotEmpty(plot.Path);
    }

    /// <summary>**What the card actually hands the control.**</summary>
    /// <remarks>
    /// **THE CARD IS BUILT THE WAY THE PANEL BUILDS IT**, from a ledger and the
    /// operator's own settings, rather than by calling the plot constructor directly.
    /// A fault between the settings and the plot would be invisible to the test
    /// above and is exactly the shape of what reached his screen.
    /// </remarks>
    [Fact]
    public void WhatTheCardHandsTheControl()
    {
        var model = Panel("FN00DJ");

        Sent(model, "02:11:00", "RD6OB KC3QIS FN00");
        Heard(model, "02:11:15", "KC3QIS RD6OB KN98");

        model.CardsNowForTests = Slot("02:11:30");
        model.RebuildCardsForTests();

        var card = model.DigitalCards.Single();
        var globe = card.Globe;

        _output.WriteLine("card            : " + card.Callsign);
        _output.WriteLine("HasOperator     : " + globe.HasOperator);
        _output.WriteLine("HasStation      : " + globe.HasStation);
        _output.WriteLine("HasPath         : " + globe.HasPath);
        _output.WriteLine("operator at     : " + At(globe.OperatorX, globe.OperatorY));
        _output.WriteLine("station at      : " + At(globe.StationX, globe.StationY));
        _output.WriteLine("path runs       : " + globe.Path.Count);
        _output.WriteLine("caption         : " + globe.Caption);

        Assert.True(globe.HasStation);
        Assert.True(globe.HasOperator);
        Assert.True(globe.HasPath);
    }

    /// <summary>**What a short grid does, which is what Settings usually holds.**</summary>
    /// <remarks>
    /// **THE OPERATOR'S GRID IN SETTINGS IS OFTEN FOUR CHARACTERS**, not six. If the
    /// four-character form fails to resolve where the six-character one succeeds,
    /// that is the difference between the caption - which came from a different
    /// path - and the marker.
    /// </remarks>
    [Theory]
    [InlineData("FN00DJ")]
    [InlineData("FN00")]
    [InlineData("fn00dj")]
    [InlineData(" FN00DJ ")]
    public void WhatEachShapeOfOperatorGridDoes(string grid)
    {
        var here = OperatorLocation.FromGrid(grid);
        var placed = here is { } a
            ? FlatWorldMap.Relief.Place(a.Latitude, a.Longitude)
            : null;

        var plot = new Ft8GlobePlot(grid, "KN98", "RD6OB", "European Russia");

        _output.WriteLine(
            "[" + grid + "] resolves: " + (here is not null)
            + ", places: " + (placed is not null)
            + ", HasOperator: " + plot.HasOperator
            + ", HasPath: " + plot.HasPath);

        Assert.True(plot.HasOperator, "[" + grid + "] did not place the operator");
    }

    /// <summary>**What the marks return with an empty log, on his fourteen rows.**</summary>
    /// <remarks>
    /// **THE COUNTS FROM A RUN, NOT FROM READING THE CODE** (task 1d). The fourteen
    /// callsigns are the ones on `assets/screenshot-cq-no-marks.png`.
    /// </remarks>
    [Fact]
    public void WhatTheMarksReturnOnHisFourteenRows()
    {
        var empty = new NudgeSet(Array.Empty<string>(), Array.Empty<string>());

        var worked = new NudgeSet(
            new[] { DxccPrefixes.EntityOf("W1ABC"), DxccPrefixes.EntityOf("VE3ZZZ") },
            new[]
            {
                DxccContinents.Of(DxccPrefixes.EntityOf("W1ABC")),
                DxccContinents.Of(DxccPrefixes.EntityOf("VE3ZZZ")),
            });

        var rows = new[]
        {
            "TI2AIM", "J38DX", "RD6OB", "VE3XN", "KJ3LLY", "W4JNC", "KD5USA",
            "KF9UG", "KS1WK", "N4ZEK", "KD8WYT", "K1ABC", "W9ZZZ", "VE7AA",
        };

        _output.WriteLine("callsign   entity                    empty log      worked NA");
        _output.WriteLine("");

        var emptyMarked = 0;
        var workedMarked = 0;

        foreach (var call in rows)
        {
            var entity = DxccPrefixes.EntityOf(call) ?? "(the table declines)";
            var (emptyKind, _) = empty.WouldOpen(call);
            var (workedKind, workedEntity) = worked.WouldOpen(call);

            if (emptyKind != NudgeKind.None)
            {
                emptyMarked++;
            }

            if (workedKind != NudgeKind.None)
            {
                workedMarked++;
            }

            _output.WriteLine(
                call.PadRight(11) + entity.PadRight(26)
                + emptyKind.ToString().PadRight(15)
                + workedKind + (workedEntity.Length > 0 ? " " + workedEntity : ""));
        }

        _output.WriteLine("");
        _output.WriteLine("marked with an empty log        : " + emptyMarked + " of 14");
        _output.WriteLine("marked having worked NA         : " + workedMarked + " of 14");

        // **TWO OF THE THREE THE INSTRUCTION NAMES.**
        Assert.NotEqual(NudgeKind.None, worked.WouldOpen("TI2AIM").Kind);
        Assert.NotEqual(NudgeKind.None, worked.WouldOpen("J38DX").Kind);

        // **AND THE THIRD IS A MISMATCH AGAINST THE INSTRUCTION, MEASURED.**
        // `DxccPrefixes.EntityOf("RD6OB")` returns nothing: the cited table declines
        // the `RD6` prefix. Unit 252's rule is that Hamlet is **silent where the
        // table declines**, so this station cannot be highlighted by any version of
        // this feature - not because the mark is broken but because Hamlet does not
        // know where he is. Reported rather than repaired: adding a prefix to a
        // cited table is a data change nobody asked for.
        Assert.Null(DxccPrefixes.EntityOf("RD6OB"));
        Assert.Equal(NudgeKind.None, worked.WouldOpen("RD6OB").Kind);
    }

    private static string At(double x, double y)
        => x.ToString("0.0", CultureInfo.InvariantCulture) + ", "
           + y.ToString("0.0", CultureInfo.InvariantCulture);

    private static MainWindowViewModel Panel(string grid)
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = "KC3QIS";
        settings.Operator.GridSquare = grid;

        var model = new MainWindowViewModel(settings, null)
        {
            DigitalNewestFirst = false,
        };

        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>());

        return model;
    }

    private static void Sent(MainWindowViewModel model, string at, string message)
        => model.RecordSentForTests(message, Slot(at));

    private static void Heard(MainWindowViewModel model, string at, string message)
        => model.AddDecodeRowForTests(
            at.Replace(":", "", StringComparison.Ordinal),
            "-09", "0.2", "1240", message, Slot(at), heardOnHz: 14_074_000);

    private static DateTime Slot(string at)
        => DateTime.ParseExact(
            "2026-09-10 " + at, "yyyy-MM-dd HH:mm:ss",
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
}
