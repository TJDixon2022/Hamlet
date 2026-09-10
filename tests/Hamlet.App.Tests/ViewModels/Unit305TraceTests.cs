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
/// Work instruction 305 task 1: **the before-numbers, measured rather than quoted.**
/// </summary>
/// <remarks>
/// <para>**THE INSTRUCTION'S FIGURES CAME OUT OF A REPORT AND REPORTS GO STALE.**
/// This measures the three the unit is aimed at, on the tree as it stands, so the
/// after-numbers have something real to be compared against.</para>
/// <para>**IT ASSERTS THE FAULTS RATHER THAN DESCRIBING THEM.** A CQ press produces
/// no card, and the hover is over budget: both are stated as assertions here so that
/// the tasks which follow have something to turn red.</para>
/// </remarks>
public sealed class Unit305TraceTests
{
    private const string HisCall = "KC3QIS";
    private const string Station = "K9XP";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the measurements are printed.</param>
    public Unit305TraceTests(ITestOutputHelper output) => _output = output;

    /// <summary>**What a CQ press puts on the screen.**</summary>
    /// <remarks>
    /// **MEASURED BEFORE TASK 2 AND AGAIN AFTER IT: 0 cards, then 1.** The
    /// explanation printed below is the trace that found the cause, and it is kept
    /// because the cause is what makes the number mean something: the ledger is
    /// keyed by callsign and a CQ is addressed to nobody, so nothing was booked and
    /// the card list rebuilt from an empty ledger.
    /// </remarks>
    [Fact]
    public void WhatACqPressPutsOnTheScreenToday()
    {
        var model = Panel();

        model.RecordSentForTests("CQ " + HisCall + " FN00", Slot("02:11:00"));

        var after = At(model, "02:11:15");

        _output.WriteLine("cards after a CQ nobody has answered : "
            + after.DigitalCards.Count);

        _output.WriteLine("rows on the decoded table            : "
            + after.DigitalDecodes.Count);

        _output.WriteLine("");
        _output.WriteLine(
            "WHY, CONFIRMED RATHER THAN ASSUMED. `Ft8ContactLedger.RecordSent` "
            + "returns without booking anything when the message is addressed to "
            + "nobody - `Ft8MessageSplit.IsCallToAnyone(fields.To)` - and the "
            + "ledger is keyed by callsign, so a CQ has no station to key on. The "
            + "card list was rebuilt from that ledger, so it stayed empty. It is "
            + "now booked under its own key and the first station to answer takes "
            + "the record over.");

        Assert.Single(after.DigitalCards);
    }

    /// <summary>**How long the `i` hover is today, and how many facts it carries.**</summary>
    [Fact]
    public void HowLongTheHoverIsToday()
    {
        var card = AFullExchange().DigitalCards.Single();

        var detail = card.Detail;

        var sentences = detail
            .Split('.', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => s.Length > 0)
            .ToList();

        _output.WriteLine("MEASURED ON A FULL EXCHANGE");
        _output.WriteLine("");
        _output.WriteLine(detail);
        _output.WriteLine("");
        _output.WriteLine("characters : " + detail.Length);
        _output.WriteLine("sentences  : " + sentences.Count);

        foreach (var sentence in sentences)
        {
            _output.WriteLine("  - " + sentence + ".");
        }

        // **OVER THE BUDGET THE INSTRUCTION SETS**, which is the fault task 4
        // exists to fix. Stated as an assertion so it turns red when it is fixed.
        Assert.True(
            detail.Length > 300,
            "the hover is already inside the budget at " + detail.Length);
    }

    /// <summary>An exchange with every fact the ledger can hold.</summary>
    private static MainWindowViewModel AFullExchange()
    {
        var model = Panel();

        Sent(model, "02:11:00", Station + " " + HisCall + " FN00");
        Heard(model, "02:11:15", HisCall + " " + Station + " EN52");
        Sent(model, "02:11:30", Station + " " + HisCall + " R-12");
        Heard(model, "02:11:45", HisCall + " " + Station + " -09");
        Heard(model, "02:12:00", HisCall + " " + Station + " RR73");

        return At(model, "02:12:15");
    }

    /// <summary>Read the panel at a moment.</summary>
    private static MainWindowViewModel At(MainWindowViewModel model, string at)
    {
        model.CardsNowForTests = Slot(at);
        model.RebuildCardsForTests();

        return model;
    }

    /// <summary>An empty panel with his callsign and no log.</summary>
    private static MainWindowViewModel Panel()
    {
        var settings = new AppSettings();

        settings.Operator.Callsign = HisCall;
        settings.Operator.GridSquare = "FN00";

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
            System.Globalization.DateTimeStyles.AssumeUniversal
            | System.Globalization.DateTimeStyles.AdjustToUniversal);
}
