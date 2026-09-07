using Hamlet.RadioEngine.Contacts;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Contacts;

/// <summary>
/// **The ledger, fed the band scene corpus, station by station.**
/// </summary>
/// <remarks>
/// <para>**THE BREAKAGE THESE WOULD HAVE CAUGHT, AND IT WAS WATCHED GOING RED.**
/// A ledger holding only the *last* message per station. It looks right on a
/// station heard once, it looks right at the moment of the last transmission,
/// and it is wrong about every exchange that took more than one turn:
/// <c>K9RST</c> sent his grid at slot 2, his roger at slot 4 and his <c>73</c> at
/// slot 6, and a ledger holding one message reads as though the first two never
/// happened. It also makes a repeat impossible to count, which is exactly what
/// step 5's *grid, 2nd time* will need.</para>
/// <para>**THE HISTORY IS THE POINT.** Step 4's criterion 1 is *which messages
/// passed each way, when, and how many slots ago* - plural, in order, each with
/// its slot.</para>
/// <para>**THE CORPUS IS FED THROUGH TWO DOORS.** A line whose sender is the
/// operator goes to <c>RecordSent</c> and every other line to <c>RecordHeard</c>,
/// which is what step 5's send path will do: it will not decode its own
/// transmission, it will say what it sent.</para>
/// </remarks>
public sealed class TheLedgerHoldsWhatPassedEachWayTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the fixture.</summary>
    /// <param name="output">Where the station table is printed.</param>
    public TheLedgerHoldsWhatPassedEachWayTests(ITestOutputHelper output) =>
        _output = output;

    /// <summary>The scene, read back off disk and fed to a ledger.</summary>
    /// <returns>The ledger and the corpus it was fed.</returns>
    internal static (Ft8ContactLedger Ledger, Ft8SceneCorpus Corpus) FedFromTheScene()
    {
        var corpus = Ft8SceneCorpus.Read(Ft8SceneCorpus.PathInTree);
        var ledger = new Ft8ContactLedger(corpus.OperatorCallsign);

        foreach (var line in corpus.Lines)
        {
            var fields = Ft8MessageSplit.Split(line.Message);
            var utc = corpus.SlotUtc(line.Slot);

            var mine = fields is not null && string.Equals(
                fields.From, corpus.OperatorCallsign, StringComparison.OrdinalIgnoreCase);

            if (mine)
            {
                ledger.RecordSent(line.Message, utc);
            }
            else
            {
                ledger.RecordHeard(line.Message, utc);
            }
        }

        return (ledger, corpus);
    }

    /// <summary>The five stations the scene puts in front of the operator, and no others.</summary>
    /// <remarks>
    /// <c>DL1QQ</c> and <c>JA1ZZ</c> are addressees of <c>G4XYZ</c>'s traffic and
    /// never transmit, so nothing books them - and <c>CQ</c> is an addressee and
    /// not a station, so the operator's own CQ at slot 1 books nobody either.
    /// </remarks>
    [Fact]
    public void TheSceneBooksFiveStationsAndNeitherCqNorSomebodyElsesAddressee()
    {
        var (ledger, _) = FedFromTheScene();

        _output.WriteLine("booked: " + string.Join(", ", ledger.Stations));

        Assert.Equal(
            new[] { "G4XYZ", "VK2PQ", "K9RST", "W1ABC", "N5TT" }.Order().ToArray(),
            ledger.Stations.Order().ToArray());

        Assert.Null(ledger.For("DL1QQ"));
        Assert.Null(ledger.For("JA1ZZ"));
        Assert.Null(ledger.For("CQ"));
    }

    /// <summary>
    /// **THE RED.** The whole of what passed with <c>K9RST</c>, both ways, in
    /// order, each with its slot.
    /// </summary>
    [Fact]
    public void TheWholeK9rstExchangeIsHeldAndNotJustItsLastMessage()
    {
        var (ledger, corpus) = FedFromTheScene();
        var k9rst = ledger.For("K9RST");

        Assert.NotNull(k9rst);

        foreach (var message in k9rst!.Heard)
        {
            _output.WriteLine($"heard  {message.SlotStartUtc:HH:mm:ss}  {message.Message}");
        }

        foreach (var message in k9rst.Sent)
        {
            _output.WriteLine($"sent   {message.SlotStartUtc:HH:mm:ss}  {message.Message}");
        }

        Assert.Equal(
            new[] { "KC3QIS K9RST EM12", "KC3QIS K9RST R-09", "KC3QIS K9RST 73" },
            k9rst.Heard.Select(m => m.Message).ToArray());

        Assert.Equal(
            new[] { "K9RST KC3QIS -13", "K9RST KC3QIS RRR" },
            k9rst.Sent.Select(m => m.Message).ToArray());

        Assert.Equal(corpus.SlotUtc(2), k9rst.Heard[0].SlotStartUtc);
        Assert.Equal(corpus.SlotUtc(4), k9rst.Heard[1].SlotStartUtc);
        Assert.Equal(corpus.SlotUtc(6), k9rst.Heard[2].SlotStartUtc);
        Assert.Equal(corpus.SlotUtc(3), k9rst.Sent[0].SlotStartUtc);
        Assert.Equal(corpus.SlotUtc(5), k9rst.Sent[1].SlotStartUtc);
    }

    /// <summary>
    /// **THE RED, THE SECOND HALF.** Every transmission of the station working
    /// three others at once, whoever it was addressed to.
    /// </summary>
    /// <remarks>
    /// Six transmissions across slots 0 to 10, only one of which was to us. A
    /// ledger holding the last message alone holds one of the six and cannot
    /// answer criterion 5 at all.
    /// </remarks>
    [Fact]
    public void EveryTransmissionOfTheThreeAtOnceStationIsHeldWhoeverItWasTo()
    {
        var (ledger, _) = FedFromTheScene();
        var g4xyz = ledger.For("G4XYZ");

        Assert.NotNull(g4xyz);

        foreach (var message in g4xyz!.Heard)
        {
            _output.WriteLine($"heard  {message.SlotStartUtc:HH:mm:ss}  {message.Message}");
        }

        Assert.Equal(
            new[]
            {
                "JA1ZZ G4XYZ -08",
                "KC3QIS G4XYZ IO91",
                "DL1QQ G4XYZ -12",
                "JA1ZZ G4XYZ RR73",
                "DL1QQ G4XYZ RRR",
                "CQ G4XYZ IO91",
            },
            g4xyz.Heard.Select(m => m.Message).ToArray());

        // One of the six was addressed to us, and it is not the last one.
        Assert.Equal("KC3QIS G4XYZ IO91", g4xyz.LastHeardToUs!.Message);
        Assert.Equal("CQ G4XYZ IO91", g4xyz.LastHeard!.Message);
        Assert.Single(g4xyz.Sent);
        Assert.Equal("G4XYZ KC3QIS -14", g4xyz.Sent[0].Message);
    }

    /// <summary>
    /// **THE RED, AND WHAT STEP 5 NEEDS.** A repeat is counted rather than
    /// replacing what it repeats.
    /// </summary>
    /// <remarks>
    /// `PHASE_PLAN.md`: *a repeat is correct behaviour and shows its count*. FT8
    /// loses transmissions constantly, so sending the grid a second time is
    /// correct behaviour and not a mistake to be greyed out - and a ledger that
    /// overwrites cannot show the count that ruling asks for.
    /// </remarks>
    [Fact]
    public void ARepeatIsCountedAndDoesNotReplaceWhatItRepeats()
    {
        var slotZero = new DateTime(2026, 9, 6, 18, 0, 0, DateTimeKind.Utc);
        var ledger = new Ft8ContactLedger("KC3QIS");

        ledger.RecordSent("W1ABC KC3QIS FN00", slotZero);
        ledger.RecordHeard("KC3QIS W1ABC FN42", slotZero.AddSeconds(15));
        ledger.RecordSent("W1ABC KC3QIS FN00", slotZero.AddSeconds(30));

        var w1abc = ledger.For("W1ABC");

        Assert.NotNull(w1abc);
        Assert.Equal(2, w1abc!.Sent.Count);

        Assert.Equal(
            2,
            w1abc.Sent.Count(m => string.Equals(
                m.Message, "W1ABC KC3QIS FN00", StringComparison.Ordinal)));
    }

    /// <summary>How many slots ago, by the engine's own boundary arithmetic.</summary>
    /// <remarks>
    /// The scene's last slot is 11 and the moment read at is the boundary of slot
    /// 13. <c>N5TT</c> last spoke in slot 8, which is five boundaries back.
    /// </remarks>
    [Fact]
    public void HowManySlotsAgoIsCountedInBoundariesCrossed()
    {
        var (ledger, corpus) = FedFromTheScene();
        var at13 = corpus.SlotUtc(13);

        Assert.Equal(5, ledger.For("N5TT")!.SlotsSinceHeard(at13));
        Assert.Equal(11, ledger.For("VK2PQ")!.SlotsSinceHeard(at13));
        Assert.Equal(3, ledger.For("G4XYZ")!.SlotsSinceHeard(at13));
        Assert.Equal(2, ledger.For("G4XYZ")!.SlotsSinceSent(at13));
        Assert.Equal(5, ledger.For("W1ABC")!.SlotsSinceHeard(at13));
        Assert.Equal(4, ledger.For("W1ABC")!.SlotsSinceSent(at13));

        // Never sent to, so there is nothing to count rather than a zero.
        Assert.Null(ledger.For("N5TT")!.SlotsSinceSent(at13));

        // The same slot is nought slots ago, not one.
        Assert.Equal(
            0, ledger.For("G4XYZ")!.SlotsSinceHeard(corpus.SlotUtc(10)));
    }

    /// <summary>
    /// **The two messages the splitter refuses do not crash it and book nobody.**
    /// </summary>
    /// <remarks>
    /// The scene puts <c>TNX BOB 73 GL</c> in slot 4 and <c>ABCDEFGHIJKLM</c> in
    /// slot 10 for exactly this. Both are already inside
    /// <see cref="FedFromTheScene"/>; they are fed again here by name, along with
    /// null and whitespace, so the refusal is asserted rather than implied.
    /// </remarks>
    [Fact]
    public void TheMessagesTheSplitterRefusesBookNobodyAndThrowNothing()
    {
        var slotZero = new DateTime(2026, 9, 6, 18, 0, 0, DateTimeKind.Utc);
        var ledger = new Ft8ContactLedger("KC3QIS");

        ledger.RecordHeard("TNX BOB 73 GL", slotZero);
        ledger.RecordHeard("ABCDEFGHIJKLM", slotZero);
        ledger.RecordHeard(null, slotZero);
        ledger.RecordHeard("   ", slotZero);
        ledger.RecordSent("TNX BOB 73 GL", slotZero);
        ledger.RecordSent(null, slotZero);

        Assert.Empty(ledger.Stations);

        // And the operator's own CQ books nobody, because CQ is an addressee.
        ledger.RecordSent("CQ KC3QIS FN00", slotZero);

        Assert.Empty(ledger.Stations);
    }

    /// <summary>The operator's callsign arrives as a parameter and nowhere else.</summary>
    [Fact]
    public void TheOperatorsCallsignIsAConstructorParameter()
    {
        var ledger = new Ft8ContactLedger("W1XYZ");

        Assert.Equal("W1XYZ", ledger.OperatorCallsign);
        Assert.Throws<ArgumentException>(() => new Ft8ContactLedger("  "));
    }
}
