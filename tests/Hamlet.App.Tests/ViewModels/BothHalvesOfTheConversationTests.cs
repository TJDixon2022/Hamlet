using System;
using System.Globalization;
using System.Linq;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
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
                "021115 received  " + HisCall + " " + His + " -09",
                "021145 received  " + HisCall + " " + His + " -09",
                "021215 sent      " + His + " " + HisCall + " R-09",
                "021245 received  " + HisCall + " " + His + " -09",
                "021315 sent      " + His + " " + HisCall + " RRR",
            },
            model.DigitalMineDecodes.Select(Describe).ToArray());
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
            new[] { "021315", "021245", "021215", "021145", "021115", "021100" },
            model.DigitalMineDecodes.Select(r => r.Utc).ToArray());

        // **AND THE ALTERNATION SURVIVES IT**, which is the point of the panel.
        Assert.Equal(
            new[] { true, false, true, false, false, true },
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
        Assert.Equal(3, model.DigitalMineDecodes.Count(r => !r.IsSent));

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

        var sent = model.DigitalMineDecodes.First(r => r.IsSent);
        var received = model.DigitalMineDecodes.First(r => !r.IsSent);

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
        Assert.Equal(6, model.DigitalMineCount);
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

        Assert.Equal(6, model.DigitalMineDecodes.Count);

        model.DigitalNewestFirst = true;

        Print(model);

        Assert.Equal(6, model.DigitalMineDecodes.Count);
        Assert.Equal(3, model.DigitalMineDecodes.Count(r => r.IsSent));
    }

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
        var settings = new AppSettings();

        settings.Operator.Callsign = HisCall;

        var model = new MainWindowViewModel(settings, null) { DigitalNewestFirst = newestFirst };

        Sent(model, "02:11:00", His + " " + HisCall + " R-09");
        Heard(model, "02:11:15", HisCall + " " + His + " -09");
        Heard(model, "02:11:45", HisCall + " " + His + " -09");
        Sent(model, "02:12:15", His + " " + HisCall + " R-09");
        Heard(model, "02:12:45", HisCall + " " + His + " -09");
        Sent(model, "02:13:15", His + " " + HisCall + " RRR");

        return model;
    }

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
