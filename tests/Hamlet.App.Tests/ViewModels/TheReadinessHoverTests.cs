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

    /// <summary>**The hover is a list of facts, not one string.**</summary>
    [Fact]
    public void TheHoverIsAListOfFactsAndNotOneString()
    {
        var card = AFullExchange().DigitalCards.Single();

        foreach (var row in card.DetailRows)
        {
            _output.WriteLine("  " + row);
        }

        _output.WriteLine("");
        _output.WriteLine("rows : " + card.DetailRows.Count);

        // **A LIST THE VIEW STACKS**, rather than a line it has to break.
        Assert.NotEmpty(card.DetailRows);

        // **FOUR GROUPS AND THE LAST THING HE SENT ON ITS OWN AT THE BOTTOM.**
        // Twelve bullets in one flat stack is the same wall, shorter.
        Assert.True(
            card.DetailRows.Count <= 6,
            "the hover is " + card.DetailRows.Count
            + " rows, which is a flat stack rather than groups");

        Assert.StartsWith(
            "He last sent", card.DetailRows[^1], StringComparison.Ordinal);
    }

    /// <summary>**Every row is one fact and carries no second sentence.**</summary>
    [Fact]
    public void EveryRowIsOneFactAndCarriesNoSecondSentence()
    {
        var card = AFullExchange().DigitalCards.Single();

        foreach (var row in card.DetailRows)
        {
            _output.WriteLine(
                row.Length.ToString(CultureInfo.InvariantCulture).PadLeft(3)
                + "  " + row);

            Assert.DoesNotContain(", so ", row, StringComparison.Ordinal);
            Assert.DoesNotContain(", which is ", row, StringComparison.Ordinal);
            Assert.DoesNotContain(" rather than ", row, StringComparison.Ordinal);

            // **A ROW IS READ AT A GLANCE OR IT IS NOT A BULLET.**
            Assert.True(
                row.Length <= 80,
                "a row is " + row.Length + " characters: " + row);
        }
    }

    /// <summary>**No row carries a bearing in degrees.**</summary>
    /// <remarks>
    /// **THE COMPASS WORD STAYS AND THE NUMBER GOES** - the author's proposal under
    /// the rulings block, reproduced in the report for Tim to overrule. HM-DEC-038
    /// already says a bearing is one of sixteen points and never degrees; the hover
    /// was the one place in the tree still printing the number.
    /// </remarks>
    [Fact]
    public void NoRowCarriesABearingInDegrees()
    {
        var card = AFullExchange().DigitalCards.Single();

        foreach (var row in card.DetailRows)
        {
            _output.WriteLine("  " + row);

            Assert.DoesNotContain("degrees", row, StringComparison.Ordinal);
        }

        // **AND THE DIRECTION IS STILL THERE, AS A WORD A PERSON CAN PICTURE.**
        Assert.Contains(
            card.DetailRows,
            r => r.Contains("miles", StringComparison.Ordinal)
                 && (r.Contains("west", StringComparison.Ordinal)
                     || r.Contains("east", StringComparison.Ordinal)
                     || r.Contains("north", StringComparison.Ordinal)
                     || r.Contains("south", StringComparison.Ordinal)));
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
            // **THE COMPASS WORD, NOT THE DEGREES** (work instruction 306
            // task 4). The fact is the direction and it is still required;
            // what went is the reading off an instrument (HM-DEC-038).
            ("the bearing", "west-northwest"),
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
