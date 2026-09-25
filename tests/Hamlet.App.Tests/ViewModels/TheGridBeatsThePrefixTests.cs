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
/// Work instruction 427 task 1: **a station's grid beats his prefix's entity** (HM-DEC-180,
/// the owner's UI list item 14).
/// </summary>
/// <remarks>
/// <para>**TIM, 2026-09-23**: when a station's grid contradicts his prefix's entity, the grid
/// wins; the card says both - *WL7E, an Alaska callsign, operating from CM98 in California* -
/// and there is no new-entity quill for him. The word is *entity*, with a note that DXCC
/// counts Alaska apart from the lower 48.</para>
/// <para>**WHERE THE GRID IS ABSENT OR UNREADABLE, NOTHING CHANGES.** The prefix's entity
/// stands and the card says what it always said.</para>
/// </remarks>
public sealed class TheGridBeatsThePrefixTests
{
    private const string HisCall = "KC3QIS";

    /// <summary>The owner's own sentence.</summary>
    private const string HisForm = "WL7E, an Alaska callsign, operating from CM98 in California";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the card's words are printed.</param>
    public TheGridBeatsThePrefixTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// **WL7E from CM98: the card states both, in his form, and says where he is.**
    /// </summary>
    [Fact]
    public void WL7EFromCM98IsInCaliforniaAndTheCardSaysBoth()
    {
        var card = Card("WL7E", "CM98");

        Print(card);

        // **THE FACE SAYS BOTH, IN THE OWNER'S FORM.**
        Assert.StartsWith(HisForm, card.EntityLine, StringComparison.Ordinal);
        Assert.True(card.HasEntityLine);
        Assert.Contains("entity", card.EntityLine, StringComparison.Ordinal);

        // **AND THE PLACE IS WHERE HE IS, NOT WHERE HIS CALLSIGN WAS ISSUED.**
        Assert.StartsWith("California", card.Place, StringComparison.Ordinal);
        Assert.DoesNotContain("Alaska", card.Place, StringComparison.Ordinal);

        // **THE CARD EXPLAINS ITSELF, WITH THE DXCC NOTE.**
        Assert.Contains(HisForm, card.DetailHover, StringComparison.Ordinal);
        Assert.Contains("DXCC counts Alaska", card.DetailHover, StringComparison.Ordinal);
        Assert.Contains("lower 48", card.DetailHover, StringComparison.Ordinal);

        // **THE WORD IS ENTITY** (HM-DEC-180).
        foreach (var said in new[] { card.EntityLine, card.Place, card.DetailHover })
        {
            Assert.DoesNotContain("country", said, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>**No new-entity quill for him on the list**, and one for a station whose grid agrees.</summary>
    [Fact]
    public void TheRowEarnsNoQuillOnThePrefix()
    {
        var model = Panel();

        Heard(model, "CQ WL7E CM98", 14_074_000);
        Heard(model, "CQ KL7AB BP51", 14_074_100);
        Heard(model, "CQ AL7CD", 14_074_200);

        foreach (var row in model.DigitalDecodes)
        {
            _output.WriteLine(row.Sender + " -> " + row.Nudge);
        }

        // **THE CONTRADICTED PREFIX OPENS NOTHING.**
        Assert.Equal(NudgeKind.None, Row(model, "WL7E").Nudge);

        // **AND THE TEST IS NOT VACUOUS**: an Alaska call from an Alaska grid, and one with no
        // grid at all, still earn the quill Alaska would open.
        Assert.Equal(NudgeKind.Visible, Row(model, "KL7AB").Nudge);
        Assert.Equal(NudgeKind.Visible, Row(model, "AL7CD").Nudge);
    }

    /// <summary>**No new-entity quill on his card either.**</summary>
    [Fact]
    public void TheCardEarnsNoQuillOnThePrefix()
    {
        var model = Panel();

        model.AddSentRowForTests("WL7E " + HisCall + " FN00", Slot("02:11:00"));
        model.RecordSentForTests("WL7E " + HisCall + " FN00", Slot("02:11:00"));

        model.AddDecodeRowForTests(
            "021115", "-09", "0.2", "1240", HisCall + " WL7E CM98",
            Slot("02:11:15"), 14_074_000);

        model.CardsNowForTests = Slot("02:12:00");
        model.RebuildCardsForTests();

        var card = model.DigitalCards.Single(c => c.Callsign == "WL7E");

        Print(card);
        _output.WriteLine("quill     : " + card.NudgePreview.HasPreview);

        Assert.False(card.NudgePreview.HasPreview);
        Assert.StartsWith(HisForm, card.EntityLine, StringComparison.Ordinal);
    }

    /// <summary>**No grid, an unreadable grid, or one the table does not place: nothing changes.**</summary>
    [Fact]
    public void WithoutAGridThatSettlesItThePrefixStands()
    {
        foreach (var grid in new[] { null, "", "ZZ99", "CM9", "JN88", "BP51" })
        {
            var card = Card("WL7E", grid);

            _output.WriteLine("[" + (grid ?? "null") + "] place: " + card.Place
                + " | line: [" + card.EntityLine + "]");

            Assert.StartsWith("Alaska", card.Place, StringComparison.Ordinal);
            Assert.False(card.HasEntityLine);
            Assert.Empty(card.EntityLine);
            Assert.Null(GridPlaces.Contradiction("WL7E", grid));
        }
    }

    private void Print(Ft8ContactCard card)
    {
        _output.WriteLine("place     : " + card.Place);
        _output.WriteLine("line      : " + card.EntityLine);
        _output.WriteLine("hover     :");
        _output.WriteLine(card.DetailHover);
    }

    /// <summary>One conversation card, the recipe `TheQuillPopupTests` uses.</summary>
    private static Ft8ContactCard Card(string callsign, string? grid)
    {
        var at = new DateTime(2026, 9, 24, 11, 2, 0, DateTimeKind.Utc);

        var facts = new Ft8CardFacts(
            Callsign: callsign,
            State: Ft8ContactState.YourMove,
            Slots: 1,
            LastAtUtc: at,
            FirstAtUtc: at,
            YouCalledHim: false,
            HeCameBack: false,
            HisMessages: 1,
            YourMessages: 0,
            HeardInAll: 1,
            ReportFromHim: null,
            ReportToHim: null,
            HeRogered: false,
            YouRogered: false,
            HeSignedOff: false,
            YouSignedOff: false,
            Grid: grid,
            HisLastPayload: null,
            YourLastMessage: null);

        return new Ft8ContactCard(
            facts, "FN00", Ft8CardActionKind.None, "", "", at.AddSeconds(10));
    }

    /// <summary>A panel whose log holds the United States and nothing else.</summary>
    private static MainWindowViewModel Panel()
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = HisCall;
        settings.Operator.GridSquare = "FN00";

        var model = new MainWindowViewModel(settings, null)
        {
            DigitalNewestFirst = false,
            OperatingMode = "Digital",
            DigitalDecodedExpanded = true,
        };

        var entities = new[] { DxccPrefixes.EntityOf("W9ZZZ")! };

        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>());
        model.UseNudgeSetForTests(
            new NudgeSet(entities, entities.Select(DxccContinents.Of)));

        return model;
    }

    private static void Heard(MainWindowViewModel model, string text, long hz)
        => model.AddDecodeRowForTests(
            "021115", "-09", "0.2", "1240", text, Slot("02:11:15"), heardOnHz: hz);

    private static DigitalDecodeRow Row(MainWindowViewModel model, string who)
        => model.DigitalDecodes.Single(r => r.Sender == who);

    private static DateTime Slot(string at)
        => DateTime.ParseExact(
            "2026-09-24 " + at,
            "yyyy-MM-dd HH:mm:ss",
            CultureInfo.InvariantCulture,
            DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
}
