using Hamlet.RadioEngine.Contacts;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Contacts;

/// <summary>
/// **The contact column speaks only about contacts the operator is in**
/// (work instruction 271, task 4; Tim's ruling of 2026-09-07).
/// </summary>
/// <remarks>
/// <para>*"The your move text is obnoxious."* Unit 266 put a state on every row
/// that had a sender, so `K9TC KJ6IX RRR` - KJ6IX telling K9TC he received, with
/// Tim in none of it - read `your move, 0 slots` on his own screen.</para>
/// <para>**THE LEDGER IS NOT WHAT CHANGES.** Every one of these messages is booked
/// exactly as unit 266 booked it, and the assertions below check that: the
/// stations are still on file and their records still hold what was heard. **This
/// is what the column shows, not what is tracked.**</para>
/// </remarks>
public sealed class TheContactColumnSpeaksOnlyAboutHisContactsTests
{
    private readonly ITestOutputHelper _output;

    public TheContactColumnSpeaksOnlyAboutHisContactsTests(ITestOutputHelper output)
        => _output = output;

    private const string OperatorCall = "KC3QIS";

    private static readonly DateTime Slot =
        new(2026, 9, 7, 17, 12, 30, DateTimeKind.Utc);

    /// <summary>
    /// **THE ONE. A slot holding a CQ, a third-party exchange and a message
    /// addressed to the operator puts a state on exactly one row.**
    /// </summary>
    [Fact]
    public void ASlotOfThreeMessagesPutsAContactStateOnExactlyOneRow()
    {
        var slot = new[]
        {
            "CQ VP2MAA FK52",     // an invitation, addressed to anyone
            "K9TC KJ6IX RRR",     // two other stations, and Tim is in none of it
            "KC3QIS W1ABC -12",   // addressed to the operator
        };

        var ledger = new Ft8ContactLedger(OperatorCall);

        foreach (var message in slot)
        {
            ledger.RecordHeard(message, Slot);
        }

        var spoke = new List<string>();

        foreach (var message in slot)
        {
            var sender = Ft8MessageSplit.Split(message)?.From;
            var text = Ft8ContactStates.ColumnTextFor(
                message, OperatorCall, ledger.For(sender), Slot);

            _output.WriteLine(
                $"\"{message,-20}\" -> contact column: "
                + (text.Length == 0 ? "(nothing)" : "\"" + text + "\""));

            if (text.Length > 0)
            {
                spoke.Add(message);
            }
        }

        Assert.Single(spoke);
        Assert.Equal("KC3QIS W1ABC -12", spoke[0]);

        // **AND THE LEDGER IS UNCHANGED.** All three senders are still on file and
        // still hold what was heard - unit 266's ledger is not what this task
        // touches.
        _output.WriteLine(string.Empty);
        _output.WriteLine("stations still booked: " + string.Join(", ", ledger.Stations));

        Assert.Equal(3, ledger.Stations.Count);
        Assert.NotNull(ledger.For("VP2MAA"));
        Assert.NotNull(ledger.For("KJ6IX"));
        Assert.NotNull(ledger.For("W1ABC"));
        Assert.Single(ledger.For("KJ6IX")!.Heard);
        Assert.Empty(ledger.For("KJ6IX")!.HeardToUs);

        // **AND THIS IS THE DEFECT, STILL REACHABLE AND QUOTED RATHER THAN
        // DESCRIBED.** `Ft8ContactStates.Read` is what unit 266 put on every row
        // that had a sender. Asked about KJ6IX - who was telling K9TC he received,
        // with the operator in none of it - it still answers, and what it answers
        // is what was on Tim's screen.
        var whatUnit266Showed = Ft8ContactStates.Read(ledger.For("KJ6IX")!, Slot).Text;

        _output.WriteLine(string.Empty);
        _output.WriteLine(
            "what the ungated read still says about \"K9TC KJ6IX RRR\": "
            + $"\"{whatUnit266Showed}\"");
        _output.WriteLine("what the column says about it now                : (nothing)");

        Assert.Equal("your move, 0 slots", whatUnit266Showed);
    }

    /// <summary>**A CQ gets no state, whoever sends it and however busy he is.**</summary>
    [Fact]
    public void ACqIsAnInvitationAndGetsNoState()
    {
        var ledger = new Ft8ContactLedger(OperatorCall);

        foreach (var message in new[] { "CQ VP2MAA FK52", "CQ DX VP2MAA FK52", "CQ POTA W5LST EM33" })
        {
            ledger.RecordHeard(message, Slot);

            var sender = Ft8MessageSplit.Split(message)?.From;
            var text = Ft8ContactStates.ColumnTextFor(
                message, OperatorCall, ledger.For(sender), Slot);

            _output.WriteLine($"\"{message}\" -> \"{text}\"");
            Assert.Equal("", text);
        }
    }

    /// <summary>
    /// **Two other stations get no state, whatever they are saying to each other.**
    /// </summary>
    [Fact]
    public void TwoOtherStationsWorkingEachOtherGetNoState()
    {
        var ledger = new Ft8ContactLedger(OperatorCall);

        var exchange = new[]
        {
            "K9TC KJ6IX FN31",
            "KJ6IX K9TC -09",
            "K9TC KJ6IX R-11",
            "KJ6IX K9TC RRR",
            "K9TC KJ6IX 73",
        };

        foreach (var message in exchange)
        {
            ledger.RecordHeard(message, Slot);
        }

        foreach (var message in exchange)
        {
            var sender = Ft8MessageSplit.Split(message)?.From;
            var text = Ft8ContactStates.ColumnTextFor(
                message, OperatorCall, ledger.For(sender), Slot);

            _output.WriteLine($"\"{message}\" -> \"{text}\"");
            Assert.Equal("", text);
        }

        // A whole QSO between two strangers, complete, and the column said nothing
        // about any of it - while the ledger holds every message of it.
        Assert.Equal(5, ledger.For("KJ6IX")!.Heard.Count + ledger.For("K9TC")!.Heard.Count);
    }

    /// <summary>
    /// **A message addressed to the operator gets a state, including to his
    /// portable and compound calls.**
    /// </summary>
    [Fact]
    public void AMessageAddressedToTheOperatorGetsAStateIncludingHisPortableCalls()
    {
        foreach (var addressee in new[] { "KC3QIS", "KC3QIS/P", "W4/KC3QIS" })
        {
            var message = addressee + " W1ABC -12";
            var ledger = new Ft8ContactLedger(OperatorCall);
            ledger.RecordHeard(message, Slot);

            var text = Ft8ContactStates.ColumnTextFor(
                message, OperatorCall, ledger.For("W1ABC"), Slot);

            _output.WriteLine($"\"{message}\" -> \"{text}\"");
            Assert.NotEqual("", text);
        }
    }

    /// <summary>
    /// **Nothing says anything where there is nothing to say** - no callsign on
    /// file, a message the splitter refuses, or a sender with no record.
    /// </summary>
    [Fact]
    public void WithNothingToGoOnTheColumnSaysNothingRatherThanGuessing()
    {
        var ledger = new Ft8ContactLedger(OperatorCall);
        ledger.RecordHeard("KC3QIS W1ABC -12", Slot);
        var record = ledger.For("W1ABC");

        Assert.Equal("", Ft8ContactStates.ColumnTextFor("KC3QIS W1ABC -12", "", record, Slot));
        Assert.Equal("", Ft8ContactStates.ColumnTextFor("KC3QIS W1ABC -12", null, record, Slot));
        Assert.Equal("", Ft8ContactStates.ColumnTextFor("KC3QIS W1ABC -12", OperatorCall, null, Slot));
        Assert.Equal("", Ft8ContactStates.ColumnTextFor("GL IN TEST DE ME", OperatorCall, record, Slot));
        Assert.Equal("", Ft8ContactStates.ColumnTextFor(null, OperatorCall, record, Slot));
    }
}
