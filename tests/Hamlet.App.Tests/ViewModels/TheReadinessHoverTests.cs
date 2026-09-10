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
/// Work instruction 305 task 4: **the `i` hover is facts, not lessons.**
/// </summary>
/// <remarks>
/// <para>**THE OPERATOR HOVERED IT AND COULD NOT READ IT.** Measured on this tree
/// before the cut: **1015 characters over 13 sentences** where a dial was known, and
/// 773 over 10 where it was not. Unit 299 was told to give every fact context that
/// teaches and did exactly that; it is withdrawn here because *show, do not tell* -
/// a fact the operator hovers for is a fact, not a lesson.</para>
/// <para>**THE FACTS ARE NOT REDUCED, THE SENTENCES AROUND THEM ARE.** Every figure
/// the long form carried is still here: both reports, the decoder's floor, the grid,
/// the distance, the bearing, the tone, the dial, the time offset, the span, the slot
/// count and what he last sent. What goes is the explanation attached to each.</para>
/// <para>**AND NOTHING IS DELETED FROM THE REPOSITORY.** The long form stands in git
/// history where it already is; whether it survives anywhere is Tim's question, asked
/// separately.</para>
/// </remarks>
public sealed class TheReadinessHoverTests
{
    private const string HisCall = "KC3QIS";
    private const string Station = "K9XP";

    /// <summary>The budget the instruction sets for twelve short facts.</summary>
    private const int Budget = 300;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the hover is printed and measured.</param>
    public TheReadinessHoverTests(ITestOutputHelper output) => _output = output;

    /// <summary>**The hover is facts only.**</summary>
    [Fact]
    public void TheHoverIsFactsOnly()
    {
        var card = AFullExchange().DigitalCards.Single();

        var detail = card.Detail;

        _output.WriteLine(detail);
        _output.WriteLine("");
        _output.WriteLine("characters : " + detail.Length);

        var facts = detail
            .Split('·', StringSplitOptions.RemoveEmptyEntries)
            .Select(f => f.Trim())
            .Where(f => f.Length > 0)
            .ToList();

        _output.WriteLine("facts      : " + facts.Count);

        foreach (var fact in facts)
        {
            _output.WriteLine("  - " + fact);
        }

        // **INSIDE THE BUDGET**, which is the measurement the task is judged on.
        Assert.True(
            detail.Length <= Budget,
            "the hover is " + detail.Length + " characters, over the "
            + Budget + " the instruction budgets");

        // **AND NO FACT CARRIES A SECOND SENTENCE.** One full stop inside a fact
        // is a decimal point or an abbreviation; a second clause explaining the
        // first is what this task removes.
        foreach (var fact in facts)
        {
            Assert.DoesNotContain(", so ", fact, StringComparison.Ordinal);
            Assert.DoesNotContain(", which is ", fact, StringComparison.Ordinal);
            Assert.DoesNotContain(" rather than ", fact, StringComparison.Ordinal);
        }
    }

    /// <summary>**Every fact the long form carried is still here.**</summary>
    /// <remarks>
    /// **CUTTING THE PROSE IS NOT CUTTING THE FACTS** (§0.0 by omission). A hover
    /// that lost a figure while getting shorter would have solved the wrong problem.
    /// </remarks>
    [Fact]
    public void EveryFactTheLongFormCarriedIsStillHere()
    {
        var detail = AFullExchange().DigitalCards.Single().Detail;

        _output.WriteLine(detail);
        _output.WriteLine("");

        foreach (var (fact, expected) in new[]
        {
            ("the report he gave", "-9"),
            ("the report you gave", "-12"),
            ("the decoder's floor", "-21"),
            ("his grid", "EN52"),
            ("the distance", "miles"),
            ("the bearing", "288"),
            ("his tone", "1240"),
            ("the dial", "14.074000"),
            ("the time offset", "0.2"),
            ("the slot count", "slot"),
            ("what he last sent", "RR73"),
        })
        {
            _output.WriteLine(
                fact.PadRight(22)
                + (detail.Contains(expected, StringComparison.Ordinal)
                    ? "present" : "MISSING"));

            Assert.Contains(expected, detail, StringComparison.Ordinal);
        }
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

    private static MainWindowViewModel At(MainWindowViewModel model, string at)
    {
        model.CardsNowForTests = Slot(at);
        model.RebuildCardsForTests();

        return model;
    }

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
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
}
