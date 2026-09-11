using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Contacts;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 310 task 3: **one receipt, as many conversations as he likes, and
/// nothing replacing anything.**
/// </summary>
/// <remarks>
/// <para>**R3, R5 AND R6** (Tim, 2026-09-11): *"If I do another CQ it is not a new
/// card, just a refresh of the time slot"*; *"No, the placeholder goes away and two
/// conversation card appear"*; *"THE NUMBER OF CONVERSATION IS UNLIMITED WE HAVE
/// VERTICAL SCROLL"*.</para>
/// <para>**R5 WITHDRAWS THE UNIT 305 ADOPT PROPOSAL.** That proposal had the first
/// answering station inherit the CQ card so the call stayed inside the exchange. The
/// ruling is the opposite: **the receipt retires and each answering station gets its
/// own card**, neither inheriting it.</para>
/// <para>**AND THE SINGLE-CARD FAULT DID NOT REPRODUCE** (task 1d, measured): two
/// stations answering produced two cards with the ledger holding both. So these tests
/// **assert the invariant so it cannot regress** rather than fixing a loss this trace
/// could find.</para>
/// </remarks>
public sealed class ThePanelHoldsThemAllTests
{
    private const string HisCall = "KC3QIS";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the panel is printed.</param>
    public ThePanelHoldsThemAllTests(ITestOutputHelper output) => _output = output;

    /// <summary>**Two stations answering one CQ give two cards and no receipt.**</summary>
    [Fact]
    public void TwoStationsAnsweringOneCqGiveTwoCardsAndNoReceipt()
    {
        var model = Panel();

        model.SendCallToAnyoneCommand.Execute(null);

        Assert.Single(model.DigitalCards);

        Heard(model, "02:11:15", HisCall + " K9XP EN52");
        Heard(model, "02:11:15", HisCall + " W1ABC FN42");
        At(model, "02:11:30");

        Print(model);

        // **TWO CONVERSATIONS.**
        Assert.Contains(model.DigitalCards, c => c.Callsign == "K9XP");
        Assert.Contains(model.DigitalCards, c => c.Callsign == "W1ABC");

        // **AND NO RECEIPT** - it retired when somebody answered.
        Assert.DoesNotContain(model.DigitalCards, c => c.IsCallToAnyone);

        // **AND NEITHER INHERITED IT** (R5 withdraws the unit 305 adopt proposal).
        foreach (var who in new[] { "K9XP", "W1ABC" })
        {
            Assert.DoesNotContain(
                model.LedgerForTests!.For(who)!.Sent,
                m => m.Message.StartsWith("CQ ", StringComparison.Ordinal));
        }
    }

    /// <summary>**A second conversation does not replace the first.**</summary>
    [Fact]
    public void ASecondConversationDoesNotReplaceTheFirst()
    {
        var model = Panel();

        Heard(model, "02:11:15", HisCall + " K9XP EN52");
        At(model, "02:11:30");

        Heard(model, "02:11:45", HisCall + " W1ABC FN42");
        At(model, "02:12:00");

        Print(model);

        // **BOTH CARDS.**
        Assert.Contains(model.DigitalCards, c => c.Callsign == "K9XP");
        Assert.Contains(model.DigitalCards, c => c.Callsign == "W1ABC");

        // **AND THE FIRST EXCHANGE'S HISTORY SURVIVED IN THE LEDGER**, which is the
        // half that decides whether a lost card is a display fault or a lost contact.
        Assert.NotEmpty(model.LedgerForTests!.For("K9XP")!.Heard);
    }

    /// <summary>**Six conversations produce six cards.**</summary>
    [Fact]
    public void SixConversationsProduceSixCards()
    {
        var model = Panel();

        var callers = new[] { "K9XP", "W1ABC", "VE3XN", "G0ABC", "JA1ABC", "VK2ABC" };

        for (var i = 0; i < callers.Length; i++)
        {
            Heard(model, "02:1" + i + ":15", HisCall + " " + callers[i] + " EN52");
        }

        At(model, "02:16:00");

        Print(model);

        foreach (var who in callers)
        {
            Assert.Contains(model.DigitalCards, c => c.Callsign == who);
        }

        Assert.Equal(callers.Length, model.DigitalCards.Count);
    }

    /// <summary>**Pressing CQ five times produces one receipt, on the fifth slot.**</summary>
    [Fact]
    public void PressingCqFiveTimesProducesOneReceipt()
    {
        var model = Panel();

        for (var press = 0; press < 5; press++)
        {
            model.SendCallToAnyoneCommand.Execute(null);
        }

        Print(model);

        var receipt = Assert.Single(model.DigitalCards);

        Assert.True(receipt.IsCallToAnyone);

        // **THE LAST CALL ONLY** (R4). What it shows is the newest, and nothing
        // counts how many there have been.
        var booked = model.LedgerForTests!.For(Ft8ContactLedger.CallToAnyone)!;

        _output.WriteLine("bookings held : " + booked.Sent.Count);
        _output.WriteLine("newest slot   : " + booked.LastSent!.SlotStartUtc);

        // **NO TALLY** - and asserted on the phrase rather than the digit,
        // because a timestamp is full of digits and my first attempt matched
        // the 5 in 01:40:45.
        foreach (var counted in new[] { "times", "5 calls", "Sent once" })
        {
            Assert.DoesNotContain(
                counted, receipt.Detail, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>**The panel never holds two receipts.**</summary>
    [Fact]
    public void ThePanelNeverHoldsTwoReceipts()
    {
        var model = Panel();

        for (var press = 0; press < 3; press++)
        {
            model.SendCallToAnyoneCommand.Execute(null);

            var receipts = model.DigitalCards.Count(c => c.IsCallToAnyone);

            _output.WriteLine("after press " + (press + 1) + ": " + receipts + " receipt(s)");

            Assert.True(receipts <= 1, "the panel held " + receipts + " receipts");
        }
    }

    /// <summary>**Dismissing the receipt touches no conversation card.**</summary>
    [Fact]
    public void DismissingTheReceiptTouchesNoConversationCard()
    {
        var model = Panel();

        Heard(model, "02:11:15", HisCall + " K9XP EN52");
        At(model, "02:11:30");

        model.SendCallToAnyoneCommand.Execute(null);

        _output.WriteLine("before: " + Join(model));

        Assert.Contains(model.DigitalCards, c => c.IsCallToAnyone);

        model.ClearCardCommand.Execute(Ft8ContactLedger.CallToAnyone);
        At(model, "02:11:45");

        _output.WriteLine("after : " + Join(model));

        // **THE RECEIPT IS GONE.**
        Assert.DoesNotContain(model.DigitalCards, c => c.IsCallToAnyone);

        // **AND THE CONVERSATION IS NOT.**
        Assert.Contains(model.DigitalCards, c => c.Callsign == "K9XP");
    }

    private static string Join(MainWindowViewModel model)
        => "[" + string.Join(", ", model.DigitalCards.Select(c => c.Callsign)) + "]";

    private void Print(MainWindowViewModel model)
    {
        _output.WriteLine("cards: " + Join(model));

        foreach (var card in model.DigitalCards)
        {
            _output.WriteLine(
                "  " + card.Callsign.PadRight(8)
                + (card.IsCallToAnyone ? "receipt     " : "conversation")
                + "  " + card.StateWord);
        }

        _output.WriteLine("");
    }

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
