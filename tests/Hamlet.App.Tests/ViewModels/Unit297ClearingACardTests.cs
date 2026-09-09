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
/// Work instruction 297, task 4: **clearing a card does not lose a contact.**
/// </summary>
/// <remarks>
/// <para>**THE BREAKAGE THIS WOULD HAVE CAUGHT**, and it is the one the instruction
/// names outright: the X taking a *Finished* card off the panel while that contact
/// was not in the log. The card is the only surface that says a contact is finished
/// and ready to write down, so it is the card he will tidy away, and a contact
/// dropped that way is gone - the ledger is in memory, `OUTPUT.md` is overwritten,
/// and nothing else in the application remembers a QSO that was never logged.</para>
/// <para>**AND THE SECOND BREAKAGE, WHICH IS SUBTLER.** The obvious way to ask
/// *has this been logged* is to look the callsign up in the log, and that answers a
/// different question: `_workedBefore` holds the newest entry per callsign, so a
/// contact with a station worked last month would read as logged and today's would
/// go without a word. The check is against the entry's own start.</para>
/// <para>**IT WAS WATCHED FAILING.** Against the tree with the two-step clear
/// removed, `TheFirstPressSaysWhatIsAboutToBeLost` reports the card gone after one
/// press and no warning shown.</para>
/// </remarks>
public sealed class Unit297ClearingACardTests
{
    /// <summary>The operator's own callsign, as Settings has it on his machine.</summary>
    private const string HisCall = "KC3QIS";

    /// <summary>The station he finished a contact with.</summary>
    private const string Station = "K9XP";

    /// <summary>The evening the fixture is set on.</summary>
    private static readonly DateTime Evening =
        new(2026, 9, 8, 2, 10, 0, DateTimeKind.Utc);

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the cards are printed.</param>
    public Unit297ClearingACardTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// **The first press on a finished, unlogged card says what is being dropped.**
    /// </summary>
    [Fact]
    public void TheFirstPressSaysWhatIsAboutToBeLost()
    {
        var model = AFinishedContact();

        var before = Only(model);

        Assert.Equal("Finished", before.StateWord);
        Assert.False(before.WarnsBeforeClearing);
        Assert.Equal("", before.ClearWarning);

        model.ClearCardCommand.Execute(Station);

        var after = Only(model);

        _output.WriteLine(after.ClearWarning);

        Assert.True(
            after.WarnsBeforeClearing,
            "the card cleared on the first press, so a finished contact that is "
            + "not in the log was discarded with nothing said about it");

        Assert.Contains(Station, after.ClearWarning, StringComparison.Ordinal);
        Assert.Contains("not in your log", after.ClearWarning, StringComparison.Ordinal);
    }

    /// <summary>**The second press clears it. The warning refuses nothing.**</summary>
    [Fact]
    public void TheSecondPressClearsItAnyway()
    {
        var model = AFinishedContact();

        model.ClearCardCommand.Execute(Station);
        model.ClearCardCommand.Execute(Station);

        Assert.Empty(model.DigitalCards);
        Assert.False(model.HasDigitalCards);
    }

    /// <summary>**A card that is not the hazard clears on one press.**</summary>
    /// <remarks>
    /// **THE WARNING IS FOR ONE CASE AND NOT FOR TIDYING UP.** A card he is still
    /// waiting on has no contact to lose, and asking twice for every card would
    /// teach him to press twice without reading, which is how the one warning that
    /// matters stops being read.
    /// </remarks>
    [Fact]
    public void AnUnfinishedCardClearsOnTheFirstPress()
    {
        var model = AStationStillCalling();

        Assert.Equal("Your turn", Only(model).StateWord);

        model.ClearCardCommand.Execute(Station);

        Assert.Empty(model.DigitalCards);
    }

    /// <summary>**A finished contact already in the log clears on one press.**</summary>
    [Fact]
    public void AFinishedContactAlreadyLoggedClearsOnTheFirstPress()
    {
        var model = AFinishedContact();

        model.UseWorkedBeforeForTests(LoggedAt(Evening.AddMinutes(1)));

        model.ClearCardCommand.Execute(Station);

        Assert.Empty(model.DigitalCards);
    }

    /// <summary>
    /// **A contact with a station worked on another day still warns.**
    /// </summary>
    /// <remarks>
    /// **THIS IS THE CALLSIGN LOOKUP FAILING, AND IT IS WHY THE CHECK IS AGAINST
    /// THE ENTRY'S OWN START.** The log holds a contact with K9XP from a month
    /// earlier. A card asking only *is this callsign in the log* would clear
    /// tonight's finished contact without a word.
    /// </remarks>
    [Fact]
    public void AnOlderContactWithTheSameStationDoesNotCountAsThisOne()
    {
        var model = AFinishedContact();

        model.UseWorkedBeforeForTests(LoggedAt(Evening.AddDays(-30)));

        model.ClearCardCommand.Execute(Station);

        var after = Only(model);

        Assert.True(
            after.WarnsBeforeClearing,
            "a contact with the same station from a month ago was taken for this "
            + "one, and tonight's finished contact was discarded silently");
    }

    /// <summary>**Clearing writes nothing to the log.**</summary>
    /// <remarks>
    /// **THE X REMOVES A CARD AND NOTHING ELSE** (Tim's ruling, 2026-09-08). It is
    /// asserted against the count the application itself reports, which is read from
    /// the log file, so a write would move it.
    /// </remarks>
    [Fact]
    public void ClearingWritesNothingToTheLog()
    {
        var model = AFinishedContact();

        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>());

        var before = model.LoggedContacts;

        model.ClearCardCommand.Execute(Station);
        model.ClearCardCommand.Execute(Station);

        Assert.Equal(before, model.LoggedContacts);
    }

    /// <summary>**Clearing leaves the ledger alone.**</summary>
    /// <remarks>
    /// The conversation is still there in full behind the card, which is what makes
    /// the card a view of a record rather than the record.
    /// </remarks>
    [Fact]
    public void ClearingLeavesEveryMessageWhereItWas()
    {
        var model = AFinishedContact();

        var messages = model.DigitalMineDecodes.Count;

        Assert.True(messages > 0, "the fixture put nothing on the panel");

        model.ClearCardCommand.Execute(Station);
        model.ClearCardCommand.Execute(Station);

        Assert.Empty(model.DigitalCards);
        Assert.Equal(messages, model.DigitalMineDecodes.Count);
    }

    /// <summary>**A cleared station that transmits again comes back.**</summary>
    /// <remarks>
    /// **HE CLEARED A CARD, NOT A STATION** (Tim's ruling, 2026-09-08). A station
    /// that calls him after he cleared it is news, and a panel that stayed silent
    /// about it would be hiding a caller - which is the one thing this panel may
    /// never do.
    /// </remarks>
    [Fact]
    public void AClearedStationComesBackWhenItTransmitsAgain()
    {
        var model = AFinishedContact();

        model.ClearCardCommand.Execute(Station);
        model.ClearCardCommand.Execute(Station);

        Assert.Empty(model.DigitalCards);

        Heard(model, "02:20:00", HisCall + " " + Station + " 73");
        model.CardsNowForTests = Slot("02:20:15");
        model.RebuildCardsForTests();

        Assert.Single(model.DigitalCards);
        Assert.Equal(Station, Only(model).Callsign);
    }

    /// <summary>The only card on the panel, or a failure saying what is there.</summary>
    private static Ft8ContactCard Only(MainWindowViewModel model)
    {
        Assert.True(
            model.DigitalCards.Count == 1,
            "expected one card and the panel holds ["
            + string.Join(", ", model.DigitalCards.Select(
                c => c.Callsign + " " + c.StateWord)) + "]");

        return model.DigitalCards[0];
    }

    /// <summary>A finished exchange with K9XP, nothing logged.</summary>
    /// <remarks>
    /// **BOTH CALLS, A REPORT EACH WAY AND A ROGER EACH WAY**, which is what
    /// `Ft8ContactStates.IsComplete` counts. It ends `RRR` on purpose: that is a
    /// finished contact with no goodbye in it, and it is the shape a card must not
    /// describe as a sign-off.
    /// </remarks>
    private MainWindowViewModel AFinishedContact()
    {
        var model = Panel();

        Sent(model, "02:11:00", Station + " " + HisCall + " FN00");
        Heard(model, "02:11:15", HisCall + " " + Station + " -09");
        Sent(model, "02:11:30", Station + " " + HisCall + " R-12");
        Heard(model, "02:11:45", HisCall + " " + Station + " RRR");

        model.CardsNowForTests = Slot("02:12:00");
        model.RebuildCardsForTests();

        return model;
    }

    /// <summary>A station calling him that he has not answered.</summary>
    private MainWindowViewModel AStationStillCalling()
    {
        var model = Panel();

        Heard(model, "02:11:15", HisCall + " " + Station + " -09");

        model.CardsNowForTests = Slot("02:11:30");
        model.RebuildCardsForTests();

        return model;
    }

    /// <summary>An empty panel with his callsign and no log.</summary>
    private static MainWindowViewModel Panel()
    {
        var settings = new AppSettings();

        settings.Operator.Callsign = HisCall;
        settings.Operator.GridSquare = "FN00";

        var model = new MainWindowViewModel(settings, null) { DigitalNewestFirst = false };

        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>());

        return model;
    }

    /// <summary>A log holding one contact with the station, started when it says.</summary>
    private static Dictionary<string, AdifContact> LoggedAt(DateTime startedUtc)
        => new(StringComparer.OrdinalIgnoreCase)
        {
            [Station] = new AdifContact { Call = Station, StartedUtc = startedUtc },
        };

    /// <summary>One message off the air, in its slot.</summary>
    private static void Heard(MainWindowViewModel model, string at, string message)
        => model.AddDecodeRowForTests(
            Stamp(at), "-09", "0.2", "1240", message, Slot(at), 14_074_000);

    /// <summary>One message this station transmitted, in its slot.</summary>
    /// <remarks>
    /// **BOTH HALVES, THE WAY THE SEND PATH DOES IT.** `AddSentRowForTests` keeps
    /// the panel row and `RecordSentForTests` tells the ledger, which are the two
    /// adjacent lines the real send runs. With only the first, `Ft8StationRecord.Sent`
    /// stays empty and no exchange can ever read as finished.
    /// </remarks>
    private static void Sent(MainWindowViewModel model, string at, string message)
    {
        model.AddSentRowForTests(message, Slot(at));
        model.RecordSentForTests(message, Slot(at));
    }

    /// <summary>A slot boundary on the evening in question, in true UTC.</summary>
    private static DateTime Slot(string at)
        => DateTime.ParseExact(
            "2026-09-08 " + at,
            "yyyy-MM-dd HH:mm:ss",
            CultureInfo.InvariantCulture,
            DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);

    /// <summary>The `HHmmss` cell for a moment, as the panel draws it.</summary>
    private static string Stamp(string at)
        => Slot(at).ToString("HHmmss", CultureInfo.InvariantCulture);
}
