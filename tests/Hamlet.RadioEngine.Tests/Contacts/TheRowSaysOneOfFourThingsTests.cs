using System.Reflection;
using Hamlet.RadioEngine.Contacts;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Contacts;

/// <summary>
/// **The four states, derived from the ledger and read against the band scene.**
/// </summary>
/// <remarks>
/// <para>**THE BREAKAGE THESE WOULD HAVE CAUGHT**, and it is one breakage per
/// assertion. A completeness rule that waited for `73` - <c>W1ABC</c>'s whole
/// exchange has none. A completeness rule that could be undone afterwards -
/// <c>K9RST</c>'s `73` arrives a slot later. **And the one that is easiest to
/// write wrongly: a gone-quiet clock counting silence from the operator rather
/// than from the station.** <c>G4XYZ</c> transmits in six of the twelve slots and
/// answers us in one of them; a clock run on his replies to us would call him
/// gone quiet while he was on the air, and it would pass every other case in this
/// corpus while doing it.</para>
/// <para>**THE MOMENT EVERY ROW IS READ AT IS SLOT 13**, and it is stated rather
/// than implied. The scene's last transmission is the operator's, in slot 11; he
/// transmits in odd slots, so slot 13 is his next opportunity - which is the
/// moment the row actually matters, because it is the moment he is looking at the
/// list deciding what to do. Slot 11 is read as well, and quoted, so that the
/// threshold can be seen to be a count of slots rather than a fact about a
/// station.</para>
/// </remarks>
public sealed class TheRowSaysOneOfFourThingsTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the fixture.</summary>
    /// <param name="output">Where the station table is printed.</param>
    public TheRowSaysOneOfFourThingsTests(ITestOutputHelper output) =>
        _output = output;

    /// <summary>The slot the whole scene is read at.</summary>
    internal const int ReadAtSlot = 13;

    /// <summary>Every station in the corpus, with the state its row shows.</summary>
    /// <returns>The reads, in the order the ledger booked them.</returns>
    internal static IReadOnlyList<Ft8ContactRead> ReadTheWholeScene(int atSlot)
    {
        // Fed only as far as the moment being read at, so no row is answered by a
        // transmission that has not happened yet.
        var (ledger, corpus) =
            TheLedgerHoldsWhatPassedEachWayTests.FedFromTheScene(atSlot);

        var now = corpus.SlotUtc(atSlot);

        return ledger.Stations
            .Select(call => Ft8ContactStates.Read(ledger.For(call)!, now))
            .ToList();
    }

    /// <summary>**The station table, and it is the whole of `NUMBER:`.**</summary>
    [Fact]
    public void EveryStationInTheSceneReadsTheStateTaskOnePredicted()
    {
        // Predicted in docs/unit258-inherited-scene-trace.md section 4, committed
        // at 62898b6, BEFORE a line of the ledger existed.
        var predicted = new Dictionary<string, Ft8ContactState>(
            StringComparer.OrdinalIgnoreCase)
        {
            ["W1ABC"] = Ft8ContactState.Complete,
            ["K9RST"] = Ft8ContactState.Complete,
            ["G4XYZ"] = Ft8ContactState.WaitingOnHim,
            ["N5TT"] = Ft8ContactState.GoneQuiet,
            ["VK2PQ"] = Ft8ContactState.GoneQuiet,
        };

        var read = ReadTheWholeScene(ReadAtSlot);
        var atEleven = ReadTheWholeScene(11).ToDictionary(r => r.Callsign);

        _output.WriteLine(
            $"read at the boundary of slot {ReadAtSlot}; the scene ends at slot 11");
        _output.WriteLine(
            $"gone quiet after {Ft8ContactStates.GoneQuietAfterSlots} slots "
            + $"= {Ft8ContactStates.GoneQuietAfterSeconds:F0} s (a choice)");
        _output.WriteLine(string.Empty);
        _output.WriteLine(
            $"{"station",-8}  {"predicted",-16}  {"shown",-24}  at slot 11");

        foreach (var row in read)
        {
            _output.WriteLine(
                $"{row.Callsign,-8}  {predicted[row.Callsign].ToString(),-16}  "
                + $"{row.Text,-24}  {atEleven[row.Callsign].Text}");
        }

        Assert.Equal(predicted.Count, read.Count);

        foreach (var row in read)
        {
            Assert.Equal(predicted[row.Callsign], row.State);
        }
    }

    /// <summary>
    /// **Criterion 3. Complete on an exchange with no `73` anywhere in it.**
    /// </summary>
    [Fact]
    public void TheW1abcExchangeIsCompleteAndHasNoSeventyThreeInIt()
    {
        var (ledger, corpus) = TheLedgerHoldsWhatPassedEachWayTests.FedFromTheScene();
        var w1abc = ledger.For("W1ABC")!;

        var everything = w1abc.Heard.Concat(w1abc.Sent)
            .OrderBy(m => m.SlotStartUtc)
            .ToList();

        foreach (var message in everything)
        {
            _output.WriteLine(message.Message);
        }

        // The whole exchange, message by message, and not a 73 in it.
        Assert.DoesNotContain(
            everything, m => m.Message.Contains("73", StringComparison.Ordinal));

        Assert.True(Ft8ContactStates.IsComplete(w1abc));

        Assert.Equal(
            Ft8ContactState.Complete,
            Ft8ContactStates.Read(w1abc, corpus.SlotUtc(ReadAtSlot)).State);

        // And it was complete the moment the operator's RRR went out, in slot 9.
        Assert.Equal(
            Ft8ContactState.Complete,
            Ft8ContactStates.Read(w1abc, corpus.SlotUtc(9)).State);
    }

    /// <summary>
    /// **Criterion 3, the other half. `73` after complete changes nothing.**
    /// </summary>
    [Fact]
    public void K9rstIsCompleteAtSlotFiveAndHisSeventyThreeAtSlotSixChangesNothing()
    {
        var (ledger, corpus) = TheLedgerHoldsWhatPassedEachWayTests.FedFromTheScene();
        var k9rst = ledger.For("K9RST")!;

        var atFive = Ft8ContactStates.Read(k9rst, corpus.SlotUtc(5));
        var atSix = Ft8ContactStates.Read(k9rst, corpus.SlotUtc(6));

        _output.WriteLine("at slot 5: " + atFive.Text);
        _output.WriteLine("at slot 6: " + atSix.Text + "   (his 73 arrived here)");

        Assert.Equal(Ft8ContactState.Complete, atFive.State);
        Assert.Equal(Ft8ContactState.Complete, atSix.State);

        // The 73 is in the ledger and it is the only thing that changed.
        Assert.Equal("KC3QIS K9RST 73", k9rst.Heard[^1].Message);
    }

    /// <summary>
    /// **Criterion 5. The station working three others reads as gaps and is
    /// never gone quiet while it is transmitting in those slots.**
    /// </summary>
    [Fact]
    public void G4xyzReadsAsGapsAndIsNeverGoneQuietInAnySlotOfTheScene()
    {
        _output.WriteLine("he transmits in slots 0, 2, 4, 6, 8 and 10;");
        _output.WriteLine("only the one in slot 2 is addressed to us.");
        _output.WriteLine(string.Empty);

        // **FED SLOT BY SLOT.** Reading a fully-fed ledger at an earlier moment
        // would let the operator's slot-11 transmission answer for slot 0, and
        // every row would read waiting on him for the wrong reason.
        for (var slot = 0; slot <= 12; slot++)
        {
            var (ledger, sofar) =
                TheLedgerHoldsWhatPassedEachWayTests.FedFromTheScene(slot);

            var him = ledger.For("G4XYZ")!;
            var read = Ft8ContactStates.Read(him, sofar.SlotUtc(slot));

            _output.WriteLine(
                $"slot {slot,2}  {read.Text,-24}  heard from him "
                + $"{him.Heard.Count}, of which to us {him.HeardToUs.Count}");

            Assert.NotEqual(Ft8ContactState.GoneQuiet, read.State);
        }

        var (whole, corpus) = TheLedgerHoldsWhatPassedEachWayTests.FedFromTheScene();
        var g4xyz = whole.For("G4XYZ")!;

        // The gaps are real: he answered us once, at slot 2, and the operator did
        // not come back until slot 11 - nine slots later - and in between he was
        // transmitting to JA1ZZ and DL1QQ the whole time.
        Assert.Single(g4xyz.HeardToUs);
        Assert.Equal(6, g4xyz.Heard.Count);
        Assert.Equal(corpus.SlotUtc(2), g4xyz.HeardToUs[0].SlotStartUtc);
        Assert.Equal(corpus.SlotUtc(11), g4xyz.LastSent!.SlotStartUtc);

        // And at slot 11 his silence towards us is nine slots long while his
        // silence on the air is one slot long. The row shows the second.
        Assert.Equal(1, g4xyz.SlotsSinceHeard(corpus.SlotUtc(11)));
    }

    /// <summary>
    /// **Criterion 4's other half. Gone quiet is a count of slots, stated as one,
    /// and is never a reason.**
    /// </summary>
    [Fact]
    public void GoneQuietIsAStatedCountOfSlotsAndNeverAVerdict()
    {
        var (ledger, corpus) = TheLedgerHoldsWhatPassedEachWayTests.FedFromTheScene();
        var n5tt = ledger.For("N5TT")!;
        var vk2pq = ledger.For("VK2PQ")!;

        var n5ttAtEleven = Ft8ContactStates.Read(n5tt, corpus.SlotUtc(11));
        var n5ttAtThirteen = Ft8ContactStates.Read(n5tt, corpus.SlotUtc(13));

        _output.WriteLine("N5TT  last spoke in slot 8");
        _output.WriteLine("  at slot 11: " + n5ttAtEleven.Text);
        _output.WriteLine("  at slot 13: " + n5ttAtThirteen.Text);
        _output.WriteLine("VK2PQ last spoke in slot 2");
        _output.WriteLine(
            "  at slot 13: " + Ft8ContactStates.Read(vk2pq, corpus.SlotUtc(13)).Text);

        // THE THRESHOLD IS A COUNT OF SLOTS AND NOTHING ELSE, which is why the
        // same station reads two ways two slots apart.
        Assert.Equal(Ft8ContactState.YourMove, n5ttAtEleven.State);
        Assert.Equal(3, n5ttAtEleven.Slots);

        Assert.Equal(Ft8ContactState.GoneQuiet, n5ttAtThirteen.State);
        Assert.Equal(5, n5ttAtThirteen.Slots);
        Assert.Equal("gone quiet, 5 slots", n5ttAtThirteen.Text);

        Assert.Equal("gone quiet, 11 slots",
            Ft8ContactStates.Read(vk2pq, corpus.SlotUtc(13)).Text);

        // The arithmetic behind the threshold, written down beside it.
        Assert.Equal(4, Ft8ContactStates.GoneQuietAfterSlots);
        Assert.Equal(60.0, Ft8ContactStates.GoneQuietAfterSeconds);
    }

    /// <summary>
    /// **Criterion 4. Nothing is closed, hidden or forbidden - and this type
    /// exposes no member that could withhold anything.**
    /// </summary>
    /// <remarks>
    /// The list of what may be sent is step 5's and does not exist yet, so there
    /// is nothing to assert stays clickable. What is assertable today is that
    /// **no member of the ledger or of the states offers a way to withhold**: no
    /// hiding, no closing, no forbidding, no greying out, no permission to ask
    /// for and no verdict to read. A row that is complete and a row that is gone
    /// quiet expose exactly the same members as any other.
    /// </remarks>
    [Fact]
    public void NoMemberOfTheLedgerOrTheStatesCouldWithholdAnything()
    {
        string[] verbs =
        [
            "hide", "hidden", "close", "closed", "forbid", "forbidden", "deny",
            "block", "grey", "gray", "dim", "suppress", "disable", "allow",
            "permit", "may", "can", "should", "must", "eligible", "valid",
            "verdict", "judge", "fault", "remove", "delete", "clear", "filter",
            "exclude", "worked",
        ];

        Type[] types =
        [
            typeof(Ft8ContactLedger), typeof(Ft8StationRecord),
            typeof(Ft8ContactStates), typeof(Ft8ContactRead),
            typeof(Ft8LedgerMessage),
        ];

        foreach (var type in types)
        {
            foreach (var member in type.GetMembers(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
            {
                var name = member.Name;

                _output.WriteLine($"{type.Name}.{name}");

                foreach (var verb in verbs)
                {
                    Assert.False(
                        name.Contains(verb, StringComparison.OrdinalIgnoreCase),
                        $"{type.Name}.{name} reads as though it could withhold "
                        + $"something (\"{verb}\"). The ledger reports; it does "
                        + "not rule.");
                }
            }
        }
    }

    /// <summary>There are four states and the words are the plan's own.</summary>
    [Fact]
    public void ThereAreFourStatesAndTheyAreNamedTheWayThePlanNamesThem()
    {
        var all = Enum.GetValues<Ft8ContactState>();

        Assert.Equal(4, all.Length);

        Assert.Equal(
            new[] { "complete", "gone quiet", "waiting on him", "your move" },
            all.Select(s => new Ft8ContactRead("W1ABC", s, 1).Words).Order().ToArray());

        // The count is always shown beside the state, and it is a count of slots.
        Assert.Equal(
            "waiting on him, 1 slot",
            new Ft8ContactRead("W1ABC", Ft8ContactState.WaitingOnHim, 1).Text);

        Assert.Equal(
            "complete, 0 slots",
            new Ft8ContactRead("W1ABC", Ft8ContactState.Complete, 0).Text);
    }
}
