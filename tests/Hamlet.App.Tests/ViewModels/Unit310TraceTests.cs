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
/// Work instruction 310 task 1: **what the CQ card actually says, whether a second
/// conversation destroys the first, and every place `CQ` is treated as a station.**
/// </summary>
/// <remarks>
/// <para>**`CQ` IS A REAL PORTUGUESE PREFIX.** CT, CR and CQ all belong to Portugal,
/// so a DXCC lookup on the literal string `CQ` matches, and the operator own general
/// call is labelled a station in Portugal. **A picture binds as hard as a sentence**
/// (§0.0, HM-DEC-092), and so does a header.</para>
/// <para>**THIS TASK WRITES NOTHING INTO `src/`.** It prints what is true so the four
/// tasks after it are fixes rather than guesses.</para>
/// </remarks>
public sealed class Unit310TraceTests
{
    private const string HisCall = "KC3QIS";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the measurements are printed.</param>
    public Unit310TraceTests(ITestOutputHelper output) => _output = output;

    /// <summary>**What `CQ` resolves to, at every site that asks.**</summary>
    [Fact]
    public void WhatCqResolvesToAtEverySiteThatAsks()
    {
        _output.WriteLine("DxccPrefixes.EntityOf(CQ)     : "
            + (DxccPrefixes.EntityOf("CQ") ?? "(null)"));

        _output.WriteLine("DxccContinents.ForCallsign(CQ): "
            + (DxccContinents.ForCallsign("CQ") ?? "(null)"));

        var nudges = new NudgeSet(Array.Empty<string>(), Array.Empty<string>());

        _output.WriteLine("NudgeSet.WouldOpen(CQ)        : "
            + nudges.WouldOpen("CQ").Kind);

        // **THE LOOKUP MATCHES, WHICH IS THE WHOLE FAULT.**
        Assert.Equal("Portugal", DxccPrefixes.EntityOf("CQ"));

        // **AND IT REACHES THE NUDGE TOO**, so the operator own call would be
        // highlighted as a country to chase.
        Assert.NotEqual(NudgeKind.None, nudges.WouldOpen("CQ").Kind);
    }

    /// <summary>**What the CQ card says today, word for word.**</summary>
    [Fact]
    public void WhatTheCqCardSaysToday()
    {
        var model = Panel();

        model.SendCallToAnyoneCommand.Execute(null);

        var card = Assert.Single(model.DigitalCards);

        _output.WriteLine("callsign     : " + card.Callsign);
        _output.WriteLine("place        : " + card.Place);
        _output.WriteLine("state word   : " + card.StateWord);
        _output.WriteLine("time line    : " + card.TimeLine);
        _output.WriteLine("action label : " + card.ActionLabel);
        _output.WriteLine("sentence     : " + card.Sentence);
        _output.WriteLine("shows globe  : " + card.ShowsGlobe);
        _output.WriteLine("globe caption: " + card.Globe.Caption);
        _output.WriteLine("detail       : " + card.Detail);

        // **THE HEADER CARRIES A COUNTRY THE OPERATOR NEVER CALLED.**
        Assert.Equal(Ft8ContactLedger.CallToAnyone, card.Callsign);
        Assert.Contains("Portugal", card.Place, StringComparison.Ordinal);

        // **AND A MAP ROW WITH A CAPTION ABOUT WHERE NOTHING IS.**
        Assert.True(card.ShowsGlobe);
        Assert.Contains(
            "does not know where", card.Globe.Caption, StringComparison.Ordinal);
    }

    /// <summary>**Does a second conversation destroy the first, or hide it?**</summary>
    /// <remarks>
    /// **TWO DIFFERENT SEVERITIES BEHIND ONE SCREEN.** If the ledger still holds the
    /// first exchange, the history survives and this is a display fault. If it does
    /// not, a contact part-way through logging can be lost.
    /// </remarks>
    [Fact]
    public void DoesASecondConversationDestroyTheFirst()
    {
        var model = Panel();

        Heard(model, "02:11:15", HisCall + " K9XP EN52");
        At(model, "02:11:30");

        var afterFirst = model.DigitalCards.Select(c => c.Callsign).ToList();

        Heard(model, "02:11:45", HisCall + " W1ABC FN42");
        At(model, "02:12:00");

        var afterSecond = model.DigitalCards.Select(c => c.Callsign).ToList();

        _output.WriteLine("cards after the first  : " + Join(afterFirst));
        _output.WriteLine("cards after the second : " + Join(afterSecond));

        var ledger = model.LedgerForTests;

        _output.WriteLine("ledger stations        : " + Join(ledger?.Stations));
        _output.WriteLine(
            "K9XP heard in ledger   : " + (ledger?.For("K9XP")?.Heard.Count ?? -1));

        // **THE LEDGER KEEPS BOTH**, whatever the panel does.
        Assert.NotNull(ledger?.For("K9XP"));
        Assert.NotNull(ledger?.For("W1ABC"));

        // **AND SO, MEASURED HERE, DOES THE PANEL.** If this passes, what the owner
        // saw is not reproduced by this path and the cause is elsewhere - which is a
        // finding, not a pass.
        _output.WriteLine("");
        _output.WriteLine(
            afterSecond.Count >= 2
                ? "THE PANEL HELD BOTH. The single-card fault is not reproduced by "
                  + "two decoded answers, so whatever produced it is a different "
                  + "path - reported rather than guessed at."
                : "THE PANEL DROPPED ONE. The second conversation replaced the "
                  + "first on screen while the ledger kept both, so the history "
                  + "survives and this is a display fault.");

        Assert.Equal(2, ledger!.Stations.Count);
    }

    /// <summary>**What sizes the map row, and what sizes the image in it.**</summary>
    [Fact]
    public void WhatSizesTheMapRow()
    {
        var bitmap = (double)FlatWorldMap.Relief.WidthPixels
            / FlatWorldMap.Relief.HeightPixels;

        _output.WriteLine(
            "bitmap        : " + FlatWorldMap.Relief.WidthPixels + " x "
            + FlatWorldMap.Relief.HeightPixels
            + ", aspect " + bitmap.ToString("0.0000", CultureInfo.InvariantCulture));

        // **WHAT THE MARKUP ASKS FOR TODAY**, read from the card template: a fixed
        // Height of 120 with the width left to the card, so the row is as wide as
        // the card and the image fits inside it by height.
        const double rowWidth = 240;
        const double rowHeight = 120;

        var scale = Math.Min(rowWidth / FlatWorldMap.Relief.WidthPixels,
            rowHeight / FlatWorldMap.Relief.HeightPixels);

        var drawnWidth = FlatWorldMap.Relief.WidthPixels * scale;
        var drawnHeight = FlatWorldMap.Relief.HeightPixels * scale;

        _output.WriteLine(
            "row           : " + rowWidth + " x " + rowHeight
            + ", aspect " + (rowWidth / rowHeight).ToString("0.0000", CultureInfo.InvariantCulture));

        _output.WriteLine(
            "image drawn   : " + drawnWidth.ToString("0.0", CultureInfo.InvariantCulture)
            + " x " + drawnHeight.ToString("0.0", CultureInfo.InvariantCulture)
            + ", aspect " + (drawnWidth / drawnHeight).ToString("0.0000", CultureInfo.InvariantCulture));

        _output.WriteLine(
            "empty to the right : "
            + (rowWidth - drawnWidth).ToString("0.0", CultureInfo.InvariantCulture)
            + " px, which is "
            + ((rowWidth - drawnWidth) / rowWidth * 100).ToString("0", CultureInfo.InvariantCulture)
            + "% of the row");

        // **THE GREY IS THE ROW BEING WIDER THAN THE PICTURE IT HOLDS.**
        Assert.True(
            drawnWidth < rowWidth,
            "the image already fills the row, so the grey is something else");

        Assert.Equal(1.8320, bitmap, 3);
    }

    private static string Join(IEnumerable<string>? what)
        => what is null ? "(none)" : "[" + string.Join(", ", what) + "]";

    private static MainWindowViewModel At(MainWindowViewModel model, string at)
    {
        model.CardsNowForTests = Slot(at);
        model.RebuildCardsForTests();

        return model;
    }

    private static MainWindowViewModel Panel()
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = HisCall;
        settings.Operator.GridSquare = "FN00DJ";

        var model = new MainWindowViewModel(settings, null)
        {
            OperatingMode = "Digital",
            DigitalNewestFirst = false,
        };

        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>());

        model.ClockOffset = new Hamlet.RadioEngine.Audio.ClockOffset(
            0.033, DateTime.UtcNow);

        return model;
    }

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
