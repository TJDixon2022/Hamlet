using System.Reflection;
using Hamlet.RadioEngine.Contacts;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Contacts;

/// <summary>
/// **Every message that is valid toward a station, with the expected one marked
/// and a repeat carrying its count.**
/// </summary>
/// <remarks>
/// <para>**THE BREAKAGE THESE WOULD HAVE CAUGHT**, and it was watched happening
/// before the list was widened: an option list that returns only the message
/// which conventionally comes next. <c>K9RST</c>'s exchange is complete at slot
/// 5, so nothing conventionally comes next, and the list handed the operator
/// **nothing at all** - no `73`, and no way to send his grid a second time when
/// the first was lost. **A contact is never closed by the app** and *nothing is
/// forbidden in the menu*, both ruled 2026-09-06, and that version broke both.
/// </para>
/// <para>**AND THE SECOND BREAKAGE: a repeat that does not say it is one.** FT8
/// loses transmissions constantly, so `RRR` a second time is correct operating.
/// A menu that shows it identically to a first send tells the operator nothing;
/// <c>W1ABC</c> and <c>K9RST</c> have both had an `RRR` already and both must
/// say so.</para>
/// <para>**THE MOMENT IS SLOT 13**, the same boundary unit 258 read the four
/// states at, and the predicted sets were written into
/// <c>docs/unit259-send-path-trace.md</c> and committed at
/// <c>495a499</c> - **before this file existed**. That is what makes the score a
/// measurement.</para>
/// <para>**NOTHING HERE INTERPRETS A MESSAGE** (12.1). A label names which field
/// shape a payload is, the way the splitter does; the expected one is what
/// conventionally answers the station's last message to us, which is format
/// arithmetic. Neither says what anybody meant.</para>
/// </remarks>
public sealed class TheMenuOffersEveryValidMessageTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the fixture.</summary>
    /// <param name="output">Where the menus are printed.</param>
    public TheMenuOffersEveryValidMessageTests(ITestOutputHelper output) =>
        _output = output;

    /// <summary>The boundary every menu is read at.</summary>
    private const int ReadAtSlot = 13;

    /// <summary>The grid the trace predicted with, and Tim's own square.</summary>
    private const string Grid = "FN00";

    /// <summary>The report the trace predicted with, uniformly across the five.</summary>
    private const int Report = -10;

    /// <summary>
    /// **The five stations, against what the trace wrote down before this ran.**
    /// </summary>
    /// <remarks>
    /// The whole of <c>NUMBER:</c>. Each block is quoted into the test output so
    /// the report can carry the menus rather than describe them.
    /// </remarks>
    [Fact]
    public void EveryStationIsOfferedTheMessagesTaskOnePredicted()
    {
        var predicted = new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["G4XYZ"] =
            [
                "G4XYZ KC3QIS FN00 | grid | - | 0",
                "G4XYZ KC3QIS -10 | report | expected | 0",
                "G4XYZ KC3QIS R-10 | roger and report | - | 0",
                "G4XYZ KC3QIS RRR | acknowledge | - | 0",
                "G4XYZ KC3QIS 73 | 73 | - | 0",
            ],
            ["VK2PQ"] =
            [
                "VK2PQ KC3QIS FN00 | grid | - | 0",
                "VK2PQ KC3QIS -10 | report | expected | 0",
                "VK2PQ KC3QIS R-10 | roger and report | - | 0",
                "VK2PQ KC3QIS RRR | acknowledge | - | 0",
                "VK2PQ KC3QIS 73 | 73 | - | 0",
            ],
            ["K9RST"] =
            [
                "K9RST KC3QIS FN00 | grid | - | 0",
                "K9RST KC3QIS -10 | report | - | 0",
                "K9RST KC3QIS R-10 | roger and report | - | 0",
                "K9RST KC3QIS RRR | acknowledge | - | 1",
                "K9RST KC3QIS 73 | 73 | expected | 0",
            ],
            ["W1ABC"] =
            [
                "W1ABC KC3QIS FN00 | grid | - | 0",
                "W1ABC KC3QIS -10 | report | - | 0",
                "W1ABC KC3QIS R-10 | roger and report | - | 0",
                "W1ABC KC3QIS RRR | acknowledge | expected | 1",
                "W1ABC KC3QIS 73 | 73 | - | 0",
            ],
            ["N5TT"] =
            [
                "N5TT KC3QIS FN00 | grid | - | 0",
                "N5TT KC3QIS -10 | report | expected | 0",
                "N5TT KC3QIS R-10 | roger and report | - | 0",
                "N5TT KC3QIS RRR | acknowledge | - | 0",
                "N5TT KC3QIS 73 | 73 | - | 0",
            ],
        };

        var matched = 0;

        foreach (var (callsign, want) in predicted)
        {
            var menu = MenuFor(callsign);
            var got = menu.Options.Select(Line).ToArray();

            _output.WriteLine(callsign + " - " + State(callsign));

            foreach (var line in got)
            {
                _output.WriteLine("    " + line);
            }

            if (want.SequenceEqual(got, StringComparer.Ordinal))
            {
                matched++;
            }
            else
            {
                _output.WriteLine("  PREDICTED:");

                foreach (var line in want)
                {
                    _output.WriteLine("    " + line);
                }
            }
        }

        _output.WriteLine("NUMBER: " + matched + " of " + predicted.Count);

        foreach (var (callsign, want) in predicted)
        {
            Assert.Equal(want, MenuFor(callsign).Options.Select(Line).ToArray());
        }

        Assert.Equal(predicted.Count, matched);
    }

    /// <summary>
    /// **The complete contact loses nothing, `73` included.**
    /// </summary>
    /// <remarks>
    /// <c>K9RST</c>'s exchange has both calls, both reports and both
    /// acknowledgements, and his `73` arrived a slot after it was complete.
    /// **This is the assertion the expected-only list failed**, and it is here
    /// because a contact is never closed by the app.
    /// </remarks>
    [Fact]
    public void TheCompleteContactStillOffersEverythingIncluding73()
    {
        var record = Ledger().For("K9RST")!;

        Assert.True(Ft8ContactStates.IsComplete(record));

        var menu = Ft8SendOptions.For(record, "KC3QIS", Grid, Report);

        _output.WriteLine("K9RST reads complete, and is offered:");

        foreach (var option in menu.Options)
        {
            _output.WriteLine("    " + Line(option));
        }

        Assert.Equal(5, menu.Options.Count);

        Assert.Contains(menu.Options, o => o.Text == "K9RST KC3QIS 73");
        Assert.Contains(menu.Options, o => o.Text == "K9RST KC3QIS FN00");
        Assert.Contains(menu.Options, o => o.Text == "K9RST KC3QIS RRR");

        // AND THE GONE-QUIET ONE TOO. Eleven silent slots withhold nothing.
        var quiet = Ledger().For("VK2PQ")!;

        Assert.Equal(
            Ft8ContactState.GoneQuiet,
            Ft8ContactStates.Read(quiet, Corpus().SlotUtc(ReadAtSlot)).State);

        Assert.Equal(
            5, Ft8SendOptions.For(quiet, "KC3QIS", Grid, Report).Options.Count);
    }

    /// <summary>
    /// **There is no member on this type that could withhold an option.**
    /// </summary>
    /// <remarks>
    /// The same reflection unit 258 used on the ledger and the states. A menu
    /// that can hide a message is a menu that will, and the ruling is that
    /// nothing is forbidden.
    /// </remarks>
    [Fact]
    public void NothingOnTheseTypesCanWithholdAnOption()
    {
        string[] verbs =
        [
            "hide", "close", "forbid", "deny", "block", "grey", "dim",
            "disable", "exclude",
        ];

        Type[] types =
        [
            typeof(Ft8SendOptions), typeof(Ft8SendOption), typeof(Ft8SendMenu),
            typeof(Ft8SendShape),
        ];

        foreach (var type in types)
        {
            foreach (var member in type.GetMembers(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static
                | BindingFlags.DeclaredOnly))
            {
                foreach (var verb in verbs)
                {
                    Assert.False(
                        member.Name.Contains(verb, StringComparison.OrdinalIgnoreCase),
                        type.Name + "." + member.Name + " reads like it can "
                        + verb + " an option, and nothing here may");
                }
            }
        }

        _output.WriteLine(
            "checked " + types.Length + " types against " + verbs.Length + " verbs");
    }

    /// <summary>**A repeat says which time it is.**</summary>
    /// <remarks>
    /// The breakage: a menu that shows a second `RRR` identically to a first one.
    /// </remarks>
    [Fact]
    public void ARepeatCarriesTheCountOfWhatAlreadyWent()
    {
        foreach (var callsign in new[] { "K9RST", "W1ABC" })
        {
            var repeat = MenuFor(callsign).Options
                .Single(o => o.Shape == Ft8SendShape.Acknowledge);

            _output.WriteLine(callsign + ": " + repeat.Text + " sent before "
                + repeat.SentBefore);

            Assert.Equal(1, repeat.SentBefore);
        }

        // AND A MESSAGE THAT HAS NEVER GONE SAYS ZERO, not "1st time".
        Assert.Equal(
            0,
            MenuFor("N5TT").Options
                .Single(o => o.Shape == Ft8SendShape.Acknowledge).SentBefore);
    }

    /// <summary>**The CQ needs no station and no typing.**</summary>
    [Fact]
    public void TheCallToAnyoneComesFromTheOperatorsOwnSettings()
    {
        Assert.Equal("CQ KC3QIS FN00", Ft8SendOptions.CallToAnyone("KC3QIS", "FN00"));

        // NO GRID IN SETTINGS IS A LEGAL FT8 MESSAGE, not an invented square.
        Assert.Equal("CQ KC3QIS", Ft8SendOptions.CallToAnyone("KC3QIS", ""));
        Assert.Equal("CQ KC3QIS", Ft8SendOptions.CallToAnyone("KC3QIS", null));
    }

    /// <summary>
    /// **A missing grid removes the messages that carry one, and says why.**
    /// </summary>
    /// <remarks>
    /// The breakage: defaulting to a square. `FN00` is Tim's own and is never a
    /// fallback (0.0). Absent is not the same as forbidden - there is no message
    /// to send, rather than one withheld.
    /// </remarks>
    [Fact]
    public void WithNoGridTheGridMessagesAreAbsentAndTheReasonIsSaidOutLoud()
    {
        var menu = Ft8SendOptions.For(Ledger().For("N5TT")!, "KC3QIS", "", Report);

        _output.WriteLine("with no grid: "
            + string.Join(", ", menu.Options.Select(o => o.Text)));
        _output.WriteLine("absent: " + string.Join(" / ", menu.Absent));

        Assert.DoesNotContain(menu.Options, o => o.Shape == Ft8SendShape.Grid);
        Assert.Equal(4, menu.Options.Count);
        Assert.Contains(menu.Absent, r => r.Contains("grid", StringComparison.Ordinal));
        Assert.DoesNotContain(menu.Options, o => o.Text.Contains("FN00", StringComparison.Ordinal));
    }

    /// <summary>
    /// **A report never measured is absent, for the same reason a grid is.**
    /// </summary>
    /// <remarks>
    /// The breakage: making up a signal report. A report is a measurement of a
    /// received signal and the option list has not made one.
    /// </remarks>
    [Fact]
    public void WithNoReportMeasuredTheReportMessagesAreAbsentAndTheReasonIsSaidOutLoud()
    {
        var menu = Ft8SendOptions.For(Ledger().For("N5TT")!, "KC3QIS", Grid, null);

        _output.WriteLine("with no report: "
            + string.Join(", ", menu.Options.Select(o => o.Text)));
        _output.WriteLine("absent: " + string.Join(" / ", menu.Absent));

        Assert.DoesNotContain(
            menu.Options,
            o => o.Shape is Ft8SendShape.Report or Ft8SendShape.RogerAndReport);

        Assert.Equal(3, menu.Options.Count);
        Assert.Contains(menu.Absent, r => r.Contains("report", StringComparison.Ordinal));
    }

    /// <summary>
    /// **The two messages the splitter refuses reach nothing, and nothing crashes.**
    /// </summary>
    /// <remarks>
    /// `TNX BOB 73 GL` and `ABCDEFGHIJKLM` are never booked, so no record exists
    /// for them; and a record whose fields are all null still answers.
    /// </remarks>
    [Fact]
    public void TheMessagesTheSplitterRefusedNeverReachTheOptionList()
    {
        var ledger = Ledger();

        Assert.Null(ledger.For("TNX"));
        Assert.Null(ledger.For("BOB"));
        Assert.Null(ledger.For("ABCDEFGHIJKLM"));

        // A STATION HEARD ONLY IN FREE TEXT. Nothing splits, nothing is booked,
        // and a station booked from an ordinary message and then sent free text
        // still answers rather than throwing.
        var loose = new Ft8ContactLedger("KC3QIS");

        loose.RecordHeard("KC3QIS ZZ9ZZ AA00", Corpus().SlotUtc(0));
        loose.RecordHeard("ABCDEFGHIJKLM", Corpus().SlotUtc(2));

        var menu = Ft8SendOptions.For(loose.For("ZZ9ZZ")!, "KC3QIS", Grid, Report);

        _output.WriteLine("ZZ9ZZ: " + string.Join(", ", menu.Options.Select(o => o.Text)));

        Assert.Equal(5, menu.Options.Count);
    }

    /// <summary>One option as the predicted blocks in the trace write one.</summary>
    /// <remarks>
    /// Text, label, whether it is the expected one, and the repeat count -
    /// the four things work instruction 259 requires each option to carry.
    /// </remarks>
    private static string Line(Ft8SendOption option)
        => option.Text + " | " + option.Label + " | "
            + (option.IsExpected ? "expected" : "-") + " | " + option.SentBefore;

    private static Ft8SendMenu MenuFor(string callsign)
        => Ft8SendOptions.For(Ledger().For(callsign)!, "KC3QIS", Grid, Report);

    private static string State(string callsign)
        => Ft8ContactStates
            .Read(Ledger().For(callsign)!, Corpus().SlotUtc(ReadAtSlot))
            .Text;

    private static Ft8ContactLedger Ledger()
        => TheLedgerHoldsWhatPassedEachWayTests.FedFromTheScene(ReadAtSlot).Ledger;

    private static Ft8SceneCorpus Corpus()
        => TheLedgerHoldsWhatPassedEachWayTests.FedFromTheScene(ReadAtSlot).Corpus;
}
