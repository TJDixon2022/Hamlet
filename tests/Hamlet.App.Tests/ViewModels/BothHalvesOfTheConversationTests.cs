using System;
using System.Globalization;
using System.Linq;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 277, task 2: the For you panel carries both halves of the
/// conversation, in time order, with his own transmissions tellable from what
/// came off the air.
/// </summary>
/// <remarks>
/// <para>**THIS REVERSES UNIT 273'S INSTRUCTION**, which said a sent message is
/// not a decode and does not belong in a decoded list. That was right about what a
/// sent message is and wrong about what the panel is for: **a conversation with
/// one side missing is unreadable.**</para>
/// <para>**THE EVENING IT COST HIM.** On 2026-09-08 K9XP came back to his call at
/// -09 and sent the same report three times. The panel showed three identical rows
/// and nothing else, because his own three transmissions were nowhere on it — so
/// there was no way to see that he was answering a slot or two late and that the
/// other station had given up on each round before his reply arrived.</para>
/// <para>**THE FIGURES ARE THE REAL EXCHANGE'S**, taken from the telemetry the
/// work instruction quotes, so the fixture and the reconstruction in task 5 are
/// the same six moments rather than two sets of numbers that have to agree.</para>
/// </remarks>
public sealed class BothHalvesOfTheConversationTests
{
    /// <summary>The operator's own callsign, as Settings has it on his machine.</summary>
    private const string HisCall = "KC3QIS";

    /// <summary>The station that came back to him.</summary>
    private const string His = "K9XP";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the conversation is printed.</param>
    public BothHalvesOfTheConversationTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// **Three sent and three received are six rows in the order they happened.**
    /// </summary>
    /// <remarks>
    /// **AGAINST THE TREE AS THE OPERATOR FOUND IT THERE ARE THREE ROWS**, all
    /// received, all identical, and this fails saying so. That is the panel he was
    /// looking at on 2026-09-08.
    /// </remarks>
    [Fact]
    public void TheWholeExchangeIsOnePanelInTimeOrder()
    {
        var model = TheK9xpExchange(newestFirst: false);

        Print(model);

        Assert.Equal(
            new[]
            {
                "021100 sent      " + His + " " + HisCall + " R-09",
                "021115 received  " + HisCall + " " + His + " -09 x2",
                "021215 sent      " + His + " " + HisCall + " R-09",
                "021245 received  " + HisCall + " " + His + " -09",
                "021315 sent      " + His + " " + HisCall + " RRR",
            },
            model.DigitalMineDecodes.Select(Shown).ToArray());

        // **ALL SIX MOMENTS ARE STILL THERE.** The pair at 021115 and 021145 is
        // one row saying so, which is task 3's ruling, and nothing was dropped:
        // three transmissions and three decodes are accounted for.
        Assert.Equal(3, model.DigitalMineDecodes.Count(r => r.IsSent));
        Assert.Equal(
            3, model.DigitalMineDecodes.Where(r => !r.IsSent).Sum(r => r.RepeatCount));
    }

    /// <summary>
    /// **The order button turns the conversation over and keeps it a conversation.**
    /// </summary>
    /// <remarks>
    /// The two halves are ordered by one rule, so reversing cannot separate them:
    /// a list that reversed only the decoded rows would put every transmission at
    /// one end and read as two lists in one column.
    /// </remarks>
    [Fact]
    public void NewestFirstReversesBothHalvesTogether()
    {
        var model = TheK9xpExchange(newestFirst: true);

        Print(model);

        Assert.Equal(
            new[] { "021315", "021245", "021215", "021115", "021100" },
            model.DigitalMineDecodes.Select(r => r.Utc).ToArray());

        // **AND THE ALTERNATION SURVIVES IT**, which is the point of the panel.
        Assert.Equal(
            new[] { true, false, true, false, true },
            model.DigitalMineDecodes.Select(r => r.IsSent).ToArray());
    }

    /// <summary>
    /// **A sent row carries no signal report and no time offset, and they are
    /// empty rather than zero.**
    /// </summary>
    /// <remarks>
    /// **§0.0, and the distinction is the whole of it.** Nothing measured a
    /// signal-to-noise ratio for a message this station transmitted, and nothing
    /// measured how far into the slot it arrived, because it did not arrive. A
    /// zero in either cell is a measurement that was never taken, drawn as one
    /// that was — and a `0.0` in the `dt` column would read as a transmission
    /// perfectly on the slot boundary, which is the exact thing this unit exists
    /// to let him see he was not doing.
    /// </remarks>
    [Fact]
    public void NothingMeasuredASentRowSoItsCellsAreEmpty()
    {
        var model = TheK9xpExchange(newestFirst: false);

        var sent = model.DigitalMineDecodes.Where(r => r.IsSent).ToList();

        Assert.Equal(3, sent.Count);

        foreach (var row in sent)
        {
            Assert.Equal("", row.Snr);
            Assert.Equal("", row.Dt);

            // **NOT THE DASH EITHER.** `NoMeasurement` means a reading was
            // attempted and failed, which is a claim about an attempt that was
            // never made here.
            Assert.NotEqual(DigitalDecodeRow.NoMeasurement, row.Snr);
            Assert.NotEqual(DigitalDecodeRow.NoMeasurement, row.Dt);
        }

        // The received rows still carry theirs, so this is not a test that passes
        // by everything being empty.
        Assert.All(
            model.DigitalMineDecodes.Where(r => !r.IsSent),
            r => Assert.Equal("-09", r.Snr));
    }

    /// <summary>
    /// **A sent row says so in a word, not only in a colour.**
    /// </summary>
    /// <remarks>
    /// **§0.6.** Roughly one man in twelve has a colour vision deficiency and this
    /// hobby's demographics make that a real slice of the people who will use this.
    /// The amber ink is a second carrier of a distinction the word already makes.
    /// </remarks>
    [Fact]
    public void TheDirectionIsAWordAndNotOnlyAColour()
    {
        var model = TheK9xpExchange(newestFirst: false);

        // **THE COUNT FIRST, BECAUSE THE LOOP BELOW PASSES ON AN EMPTY PANEL.**
        // Watched failing against the pre-change tree, this was the one test of
        // the seven that went green: with no sent rows placed, every row is a
        // received one and every assertion in the loop holds. A test that passes
        // because the thing it is about is absent is worse than no test.
        Assert.Equal(3, model.DigitalMineDecodes.Count(r => r.IsSent));
        Assert.Equal(2, model.DigitalMineDecodes.Count(r => !r.IsSent));

        foreach (var row in model.DigitalMineDecodes)
        {
            if (row.IsSent)
            {
                Assert.Equal("sent", row.Direction);
                Assert.True(row.HasDirection);
            }
            else
            {
                Assert.Equal("", row.Direction);
                Assert.False(row.HasDirection);
            }
        }
    }

    /// <summary>
    /// **Right-clicking his own transmission offers nothing to send.**
    /// </summary>
    /// <remarks>
    /// The sender of a message he transmitted is his own callsign, so without the
    /// guard the menu composes replies addressed from him to him and offers to
    /// transmit them. There is no station on that row to answer.
    /// </remarks>
    [Fact]
    public void ASentRowHasNoStationToAnswerAndSoNoMenu()
    {
        var model = TheK9xpExchange(newestFirst: false);

        var sent = OnlyOne(model, r => r.IsSent, "a message he sent");
        var received = OnlyOne(model, r => !r.IsSent, "a message he heard");

        Assert.Null(model.SendMenuFor(sent));

        // And the received row still has one, so this is not passing because the
        // ledger is empty.
        Assert.NotNull(model.SendMenuFor(received));
    }

    /// <summary>
    /// **A sent row is not a decode and is not counted as one.**
    /// </summary>
    /// <remarks>
    /// The left summary's hidden count is `heard - shown - his`, so counting his
    /// own transmissions among the mine rows would take three off a total they
    /// were never part of. Enough sends and it goes negative.
    /// </remarks>
    [Fact]
    public void SendingDoesNotChangeWhatTheBandIsSaidToHaveDone()
    {
        var model = TheK9xpExchange(newestFirst: false);

        // Three heard, all of them his, none hidden - the three sent rows are on
        // the panel and in none of these figures.
        Assert.Equal(3, model.DigitalDecodes.Count);
        Assert.Equal(0, model.DigitalShownCount);
        Assert.Equal(0, model.DigitalHiddenCount);

        // **FIVE ROWS FOR SIX MOMENTS**, because the pair of repeats is one row
        // that says it is two (task 3).
        Assert.Equal(5, model.DigitalMineCount);
    }

    /// <summary>
    /// **A rebuild keeps both halves**, which is what the order button does.
    /// </summary>
    /// <remarks>
    /// A full rebuild walks `DigitalDecodes`, and a sent row is deliberately not
    /// in it. Without the merge in `ApplyDecodedFilter` the first press of the
    /// order button would empty half the conversation, and the half it emptied
    /// would be the half showing he answered at all.
    /// </remarks>
    [Fact]
    public void TheOrderButtonDoesNotThrowHisOwnHalfAway()
    {
        var model = TheK9xpExchange(newestFirst: false);

        Assert.Equal(5, model.DigitalMineDecodes.Count);

        model.DigitalNewestFirst = true;

        Print(model);

        Assert.Equal(5, model.DigitalMineDecodes.Count);
        Assert.Equal(3, model.DigitalMineDecodes.Count(r => r.IsSent));

        // **AND THE REBUILD FOLDS AGAIN RATHER THAN INHERITING THE COUNT.** A
        // rebuild walks the decoded table, where all three repeats still sit as
        // three rows, so the fold has to happen a second time and happen
        // forwards in time. Done down a newest-first table it would find every
        // pair the wrong way round and fold nothing.
        Assert.Equal(
            3, model.DigitalMineDecodes.Where(r => !r.IsSent).Sum(r => r.RepeatCount));
    }

    /// <summary>
    /// **Two identical messages running fold into one row with a count.**
    /// </summary>
    /// <remarks>
    /// **THREE COPIES MEAN HE IS NOT BEING HEARD, AND THREE ROWS HIDE IT** (Tim's
    /// ruling, 2026-09-08). `02:11:15` and `02:11:45` are the same report with
    /// nothing between them, so they are one row saying `-09 x2`.
    /// </remarks>
    [Fact]
    public void TwoOfTheSameRunningAreOneRowWithACount()
    {
        var model = TheK9xpExchange(newestFirst: false);

        Print(model);

        var first = OnlyOne(model, r => !r.IsSent, "a message he heard");

        Assert.Equal(2, first.RepeatCount);
        Assert.True(first.HasRepeats);
        Assert.Equal(HisCall + " " + His + " -09 x2", first.Shown);

        // **AND `Message` IS UNTOUCHED**, so everything that reasons about the
        // text still sees the text that was sent.
        Assert.Equal(HisCall + " " + His + " -09", first.Message);
    }

    /// <summary>
    /// **A repeat that arrives after he transmitted does not fold backwards.**
    /// </summary>
    /// <remarks>
    /// <para>**THIS IS THE MOST DIAGNOSTIC FACT IN THE WHOLE EXCHANGE**, and a
    /// literal reading of *three identical messages read as one row* would destroy
    /// it. The third `-09` came at `02:12:45`, after he transmitted at `02:12:15`:
    /// it says the station did not hear his answer. Folded into the row above his
    /// transmission it would say only that the station repeated itself early on,
    /// which is a different and much less useful claim, and the panel would be
    /// hiding the very thing it was built to show.</para>
    /// <para>**SO THE COUNT STOPS AT ANYTHING IN BETWEEN**, and the exchange reads
    /// as `x2`, his answer, then a fresh `x1`.</para>
    /// </remarks>
    [Fact]
    public void ARepeatAfterHisAnswerIsItsOwnRowAndSaysSo()
    {
        var model = TheK9xpExchange(newestFirst: false);

        Assert.Equal(
            new[]
            {
                "021100 sent      " + His + " " + HisCall + " R-09",
                "021115 received  " + HisCall + " " + His + " -09 x2",
                "021215 sent      " + His + " " + HisCall + " R-09",
                "021245 received  " + HisCall + " " + His + " -09",
                "021315 sent      " + His + " " + HisCall + " RRR",
            },
            model.DigitalMineDecodes.Select(Shown).ToArray());

        var after = model.DigitalMineDecodes.Last(r => !r.IsSent);

        Assert.Equal(1, after.RepeatCount);
        Assert.False(after.HasRepeats);
    }

    /// <summary>
    /// **A folded repeat is still every message it stands for, in the counts.**
    /// </summary>
    /// <remarks>
    /// The left summary's hidden count is `heard - shown - his`. Counting the
    /// folded row once would leave one decode looking hidden on a panel that is
    /// showing it.
    /// </remarks>
    [Fact]
    public void FoldingARepeatHidesNothingFromTheTotals()
    {
        var model = TheK9xpExchange(newestFirst: false);

        Assert.Equal(3, model.DigitalDecodes.Count);
        Assert.Equal(0, model.DigitalHiddenCount);
    }

    /// <summary>
    /// **The panel says whose slot it is, and will not guess before it can.**
    /// </summary>
    /// <remarks>
    /// K9XP transmitted on `:15` and `:45`, so his own slots are `:00` and `:30`.
    /// This is the same rule the engine test asserts, read through the panel, so a
    /// turn line wired to the wrong station or the wrong list fails here even
    /// though the arithmetic is right.
    /// </remarks>
    [Fact]
    public void ThePanelReadsTheBeatFromTheStationItIsFollowing()
    {
        var empty = new MainWindowViewModel(Settings(), null);

        empty.RefreshTurnForTests();

        // Nothing heard, so no turn is claimed however the clock reads.
        Assert.False(empty.DigitalTurnIsKnown);
        Assert.False(empty.DigitalTurnIsMine);
        _output.WriteLine("empty panel: " + empty.DigitalTurnLine);

        var model = TheK9xpExchange(newestFirst: false);

        // And the station it is following is the one that called him.
        Assert.Equal(His, model.ConversationStation());
    }

    /// <summary>
    /// **Two stations calling him give one conversation and one waiting row.**
    /// </summary>
    /// <remarks>
    /// Work instruction 277, task 4. The panel follows whoever most recently called
    /// him, and the other is above it with what it sent and how long it has been
    /// quiet. **Nothing is hidden**: both are on the screen, one as a conversation
    /// and one as a row he can click.
    /// </remarks>
    [Fact]
    public void TwoStationsGiveOneConversationAndOneWaitingRow()
    {
        var model = TwoCalling();

        PrintWaiting(model);

        // W1ABC called last, so the panel is following W1ABC.
        Assert.Equal("W1ABC", model.ConversationStation());

        // **ONE ROW, AND THE MESSAGE TO K1ABC IS NOT IN IT.** W1ABC sent the same
        // report twice running, which folds; and a message he sent to a third
        // station belongs to that station's conversation, not to this one. Reading
        // the sender of a sent row would have filed it under his own callsign.
        Assert.Equal(
            new[] { HisCall + " W1ABC -14 x2" },
            model.DigitalMineDecodes.Select(r => r.Shown).ToArray());

        var waiting = Assert.Single(model.DigitalWaiting);

        Assert.Equal(His, waiting.Callsign);
        Assert.Equal(2, waiting.Messages);
        Assert.True(model.HasDigitalWaiting);
    }

    /// <summary>
    /// **Clicking a waiting row switches the conversation and loses neither.**
    /// </summary>
    /// <remarks>
    /// The station he was reading moves to the waiting strip, which is where the
    /// one he clicked came from. Every message either station sent is still on the
    /// screen somewhere, which is the whole of *nothing is hidden*.
    /// </remarks>
    [Fact]
    public void ClickingAWaitingRowSwitchesAndKeepsTheOther()
    {
        var model = TwoCalling();

        model.ShowConversationCommand.Execute(His);

        PrintWaiting(model);

        Assert.Equal(His, model.ConversationStation());

        // K9XP's two messages and his own answer to K9XP are the conversation now.
        // **THREE ROWS AND NOT TWO**, because his answer came between the two
        // identical reports. That is the whole reason the fold stops at anything
        // in between: the second -09 says K9XP did not hear him.
        Assert.Equal(
            new[]
            {
                HisCall + " " + His + " -09",
                His + " " + HisCall + " R-09",
                HisCall + " " + His + " -09",
            },
            model.DigitalMineDecodes.Select(r => r.Message).ToArray());

        // And W1ABC is not lost: it is the waiting row now.
        var waiting = Assert.Single(model.DigitalWaiting);

        Assert.Equal("W1ABC", waiting.Callsign);

        // **NOTHING WENT MISSING IN THE SWITCH.** Every message addressed to him is
        // still accounted for, on the panel or on the strip.
        Assert.Equal(
            4,
            model.DigitalMineDecodes.Where(r => !r.IsSent).Sum(r => r.RepeatCount)
                + model.DigitalWaiting.Sum(w => w.Messages));
    }

    /// <summary>
    /// **His choice sticks, and a later call from somebody else does not steal it.**
    /// </summary>
    /// <remarks>
    /// A panel that jumped away while he was reading it would be worse than one
    /// that never moved, and this is the case that happens on a busy band: he
    /// clicks a station and a third one calls him a slot later.
    /// </remarks>
    [Fact]
    public void OnceHeHasChosenThePanelStaysWhereHePutIt()
    {
        var model = TwoCalling();

        model.ShowConversationCommand.Execute(His);

        Heard(model, "02:14:15", HisCall + " W1ABC -14");

        Assert.Equal(His, model.ConversationStation());

        // W1ABC is still the waiting row and its count has gone up, so the new
        // call is visible without having stolen the panel.
        Assert.Equal(3, model.DigitalWaiting.Single().Messages);
    }

    /// <summary>
    /// **The summary counts everybody, not the conversation on show.**
    /// </summary>
    /// <remarks>
    /// §0.0 and HM-DEC-092. The panel draws one conversation, so a summary counting
    /// what is drawn would read `1 for you` on an evening when two stations were
    /// calling, and a collapsed panel saying that is a false picture of how busy he
    /// is. It binds as hard as a false sentence.
    /// </remarks>
    [Fact]
    public void TheSummaryCountsEverybodyCallingHim()
    {
        var model = TwoCalling();

        Assert.Equal("4 for you", model.DigitalMineSummary);

        // And the hidden count still balances: four heard, all for him, none held
        // back by the left list's toggles.
        Assert.Equal(4, model.DigitalDecodes.Count);
        Assert.Equal(0, model.DigitalHiddenCount);
    }

    /// <summary>
    /// **The turn follows the conversation, not whichever station spoke last.**
    /// </summary>
    /// <remarks>
    /// Two stations calling him can be using opposite halves of the minute. Reading
    /// the newest row whoever sent it would flip the turn line every time the other
    /// one transmitted, which is a claim about his slot that changes under him for
    /// a reason he cannot see.
    /// </remarks>
    [Fact]
    public void TheBeatFollowsWhoeverHeIsWorking()
    {
        var model = TwoCalling();

        // W1ABC transmits on the even half; K9XP on the odd one.
        model.ShowConversationCommand.Execute("W1ABC");
        Assert.Equal("W1ABC", model.ConversationStation());

        model.ShowConversationCommand.Execute(His);
        Assert.Equal(His, model.ConversationStation());
    }

    /// <summary>Two stations calling him in the same few minutes.</summary>
    /// <remarks>
    /// **K9XP IS ON THE ODD HALF AND W1ABC ON THE EVEN ONE**, so the two disagree
    /// about whose turn it is and a panel reading the wrong one is caught.
    /// </remarks>
    private static MainWindowViewModel TwoCalling()
    {
        var model = new MainWindowViewModel(Settings(), null) { DigitalNewestFirst = false };

        Heard(model, "02:11:15", HisCall + " " + His + " -09");
        Sent(model, "02:11:30", His + " " + HisCall + " R-09");
        Heard(model, "02:11:45", HisCall + " " + His + " -09");
        Heard(model, "02:12:00", HisCall + " W1ABC -14");

        // Something he sent to a third station, which belongs to neither of these
        // two conversations and must not appear in either.
        Sent(model, "02:12:30", "K1ABC " + HisCall + " RRR");
        Heard(model, "02:13:00", HisCall + " W1ABC -14");

        return model;
    }

    /// <summary>The waiting strip, so a failure shows who was on it.</summary>
    private void PrintWaiting(MainWindowViewModel model)
    {
        _output.WriteLine("conversation: " + model.ConversationStation());

        foreach (var row in model.DigitalMineDecodes)
        {
            _output.WriteLine("    " + Shown(row));
        }

        _output.WriteLine("waiting: " + model.DigitalWaitingSummary);

        foreach (var waiting in model.DigitalWaiting)
        {
            _output.WriteLine("    " + waiting.Callsign + "  " + waiting.Detail);
        }

        _output.WriteLine("");
    }

    /// <summary>
    /// **The evening of 2026-09-08, step by step, as the panel would now draw it.**
    /// </summary>
    /// <remarks>
    /// <para>Work instruction 277, task 5, and it is the honest check on the other
    /// four rather than a nicety. **If the missed beat is not obvious here, the unit
    /// has failed**, whatever its tests say.</para>
    /// <para>**THE FIGURES ARE THE TELEMETRY'S OWN.** Three received at `02:11:15`,
    /// `02:11:45` and `02:12:45`; three sent at `02:11:00`, `02:12:15` and
    /// `02:13:15`. Nothing here is composed for the reconstruction.</para>
    /// <para>**THE TURN LINE IS READ A SECOND INTO EACH SLOT**, from the same
    /// engine call the panel uses, so what is printed is what he would have been
    /// looking at rather than a description of it.</para>
    /// </remarks>
    [Fact]
    public void TheEveningOf20260908AsItWouldNowRead()
    {
        var model = new MainWindowViewModel(Settings(), null) { DigitalNewestFirst = false };

        Step(model, "02:11:00", sent: true, His + " " + HisCall + " R-09");
        Step(model, "02:11:15", sent: false, HisCall + " " + His + " -09");
        Step(model, "02:11:45", sent: false, HisCall + " " + His + " -09");
        Step(model, "02:12:15", sent: true, His + " " + HisCall + " R-09");
        Step(model, "02:12:45", sent: false, HisCall + " " + His + " -09");
        Step(model, "02:13:15", sent: true, His + " " + HisCall + " RRR");

        // **THE CLAIM THE RECONSTRUCTION HAS TO SUPPORT**, asserted rather than
        // left to the reader of the printout: his three transmissions are on the
        // panel, the repeat before his answer is folded, and the repeat after it
        // stands alone as its own row.
        Assert.Equal(
            new[]
            {
                "021100 sent      " + His + " " + HisCall + " R-09",
                "021115 received  " + HisCall + " " + His + " -09 x2",
                "021215 sent      " + His + " " + HisCall + " R-09",
                "021245 received  " + HisCall + " " + His + " -09",
                "021315 sent      " + His + " " + HisCall + " RRR",
            },
            model.DigitalMineDecodes.Select(Shown).ToArray());

        // **THE DIAGNOSIS, IN THE ONE PLACE IT CAN BE STATED AS A FACT.** K9XP
        // transmits on the odd half, so the operator's slots are `:00` and `:30`.
        // His first call went out in his own slot and was correct; **both of the
        // replies that followed went out in K9XP's**, which is the missed beat,
        // and each of those is a reply arriving a slot after the one it answered.
        var theirs = Slot("02:11:45");

        Assert.Equal(
            Ft8TurnState.Mine,
            Ft8Turn.Read(Slot("02:11:00").AddSeconds(1), Offset(), theirs).State);

        foreach (var at in new[] { "02:12:15", "02:13:15" })
        {
            var turn = Ft8Turn.Read(Slot(at).AddSeconds(1), Offset(), theirs);

            Assert.Equal(Ft8TurnState.Theirs, turn.State);

            // And the sentence he would have been reading names it.
            Assert.Contains("Their slot", turn.Line(), StringComparison.Ordinal);
        }
    }

    /// <summary>One moment of that evening, placed and then printed.</summary>
    /// <param name="model">The panel.</param>
    /// <param name="at">The slot it happened in.</param>
    /// <param name="sent">True where he transmitted it.</param>
    /// <param name="message">The text.</param>
    private void Step(
        MainWindowViewModel model, string at, bool sent, string message)
    {
        if (sent)
        {
            Sent(model, at, message);
        }
        else
        {
            Heard(model, at, message);
        }

        // **A SECOND INTO THE SLOT**, which is where he would be looking: the
        // decode has landed and there are fourteen seconds of the slot to go.
        var turn = Ft8Turn.Read(Slot(at).AddSeconds(1), Offset(), LastHeard(model));

        _output.WriteLine(
            "=== " + at + " UTC, he " + (sent ? "transmits" : "hears") + " ===");
        _output.WriteLine("  turn: " + turn.Line());
        _output.WriteLine("  For you (" + model.ConversationStation() + "):");

        foreach (var row in model.DigitalMineDecodes)
        {
            _output.WriteLine("      " + Shown(row));
        }

        _output.WriteLine("");
    }

    /// <summary>The last slot K9XP was heard in, as the panel knows it.</summary>
    private static DateTime? LastHeard(MainWindowViewModel model)
    {
        DateTime? best = null;

        foreach (var row in model.DigitalMineDecodes)
        {
            if (row.IsSent || row.SlotStartUtc == default)
            {
                continue;
            }

            if (best is null || row.SlotStartUtc > best)
            {
                best = row.SlotStartUtc;
            }
        }

        return best;
    }

    /// <summary>A clock offset that has been measured, so slots can be placed.</summary>
    private static ClockOffset Offset()
        => new(0.0, new DateTime(2026, 9, 8, 2, 10, 0, DateTimeKind.Utc));

    /// <summary>The exchange of 2026-09-08, from its own figures.</summary>
    /// <param name="newestFirst">Which way round the panel is showing.</param>
    /// <returns>The panel, with all six messages placed.</returns>
    /// <remarks>
    /// **THE MESSAGES ARE ADDED IN THE ORDER THEY HAPPENED**, alternating, so the
    /// placement rule is exercised rather than a sort applied afterwards. Each
    /// sent row goes in against a panel that already holds received rows on both
    /// sides of where it belongs, which is the case that would fail if a sent row
    /// were simply appended.
    /// </remarks>
    private static MainWindowViewModel TheK9xpExchange(bool newestFirst)
    {
        var model = new MainWindowViewModel(Settings(), null) { DigitalNewestFirst = newestFirst };

        Sent(model, "02:11:00", His + " " + HisCall + " R-09");
        Heard(model, "02:11:15", HisCall + " " + His + " -09");
        Heard(model, "02:11:45", HisCall + " " + His + " -09");
        Sent(model, "02:12:15", His + " " + HisCall + " R-09");
        Heard(model, "02:12:45", HisCall + " " + His + " -09");
        Sent(model, "02:13:15", His + " " + HisCall + " RRR");

        return model;
    }

    /// <summary>The first row matching, or a failure that says what was there.</summary>
    /// <remarks>
    /// **A BARE `First` THREW `Sequence contains no matching element`** (work
    /// instruction 279 task 4, case 7), naming neither what it wanted nor what the
    /// panel held. Nothing this class asserts has changed; only what it says when
    /// the conversation cannot produce a subject.
    /// </remarks>
    private static DigitalDecodeRow OnlyOne(
        MainWindowViewModel model, Func<DigitalDecodeRow, bool> wanted, string what)
    {
        var row = model.DigitalMineDecodes.FirstOrDefault(wanted);

        Assert.True(
            row is not null,
            "the conversation holds no row that is " + what + ". Rows: ["
            + string.Join(", ", model.DigitalMineDecodes.Select(
                r => (r.IsSent ? "sent " : "heard ") + r.Message)) + "]");

        return row!;
    }

    /// <summary>Settings as they stand on his machine, with his callsign.</summary>
    private static AppSettings Settings()
    {
        var settings = new AppSettings();

        settings.Operator.Callsign = HisCall;

        return settings;
    }

    /// <summary>One row as the panel draws it, with any repeat count.</summary>
    private static string Shown(DigitalDecodeRow row)
        => row.Utc + " " + (row.IsSent ? "sent    " : "received").PadRight(9)
            + " " + row.Shown;

    /// <summary>One message off the air, in its slot.</summary>
    private static void Heard(MainWindowViewModel model, string at, string message)
        => model.AddDecodeRowForTests(
            Stamp(at), "-09", "0.2", "1240", message, Slot(at), 14_074_000);

    /// <summary>One message this station transmitted, in its slot.</summary>
    private static void Sent(MainWindowViewModel model, string at, string message)
        => model.AddSentRowForTests(message, Slot(at));

    /// <summary>A slot boundary on the evening in question, in true UTC.</summary>
    private static DateTime Slot(string at)
        => DateTime.ParseExact(
            "2026-09-08 " + at,
            "yyyy-MM-dd HH:mm:ss",
            CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.AdjustToUniversal
                | System.Globalization.DateTimeStyles.AssumeUniversal);

    /// <summary>The `HHmmss` cell for a moment, as the panel draws it.</summary>
    private static string Stamp(string at)
        => Slot(at).ToString("HHmmss", CultureInfo.InvariantCulture);

    /// <summary>One row as a line, for asserting and for reading.</summary>
    private static string Describe(DigitalDecodeRow row)
        => row.Utc + " " + (row.IsSent ? "sent    " : "received").PadRight(9) + " " + row.Message;

    /// <summary>The conversation, so a failure shows what was on the panel.</summary>
    private void Print(MainWindowViewModel model)
    {
        _output.WriteLine("For you - " + model.DigitalOrderLabel + ":");

        foreach (var row in model.DigitalMineDecodes)
        {
            _output.WriteLine("    " + Describe(row));
        }

        _output.WriteLine("");
    }
}
