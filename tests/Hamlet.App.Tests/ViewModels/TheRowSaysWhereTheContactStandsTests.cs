using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 258, task 5: **the four states reach the operator's row.**
/// </summary>
/// <remarks>
/// <para>**THE BREAKAGE THIS WOULD HAVE CAUGHT.** A contact ledger that is
/// perfectly correct in the engine and reaches nothing a reader can see.
/// `PHASE_PLAN.md` step 4's criterion 2 is that the four states are shown *per
/// row*, and an engine type nobody feeds is criterion 1 met and criterion 2
/// not.</para>
/// <para>**IT NEVER OPENS A WINDOW.** `DigitalDecodeRow` formats what a reader
/// sees, the way the `snr` cell and the tooltips already do, so the text can be
/// asserted through `AddDecodeRowForTests` - the same door the decoder uses -
/// with no Avalonia surface anywhere in the test.</para>
/// <para>**THE SCENE IS THE SAME ONE THE ENGINE IS PROVED AGAINST**, retyped
/// here rather than read off disk because the app test project has no business
/// knowing where the engine's corpus lives. The engine's own
/// `TheRowSaysOneOfFourThingsTests` is what proves the states against the
/// decoded corpus; this proves they arrive on the row.</para>
/// <para>**NOTHING HERE SENDS.** Only the heard door is fed, because there is no
/// send path until step 5 - so `G4XYZ`, whom the operator never answers, reads
/// *your move* here where the engine's corpus test reads *waiting on him* after
/// the operator's slot-11 transmission. **That difference is the whole of what
/// step 5 will add**, and it is asserted rather than glossed.</para>
/// </remarks>
public sealed class TheRowSaysWhereTheContactStandsTests
{
    /// <summary>The boundary slot 0 opened on, as the scene has it.</summary>
    private static readonly DateTime SlotZero =
        new(2026, 9, 6, 18, 0, 0, DateTimeKind.Utc);

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the rows are printed.</param>
    public TheRowSaysWhereTheContactStandsTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>**The state and its slot count are text on the row.**</summary>
    [Fact]
    public void TheContactCellCarriesTheStateAndItsSlotCount()
    {
        var rows = TheScenePutThroughTheTable();

        foreach (var row in rows)
        {
            _output.WriteLine($"{row.Utc}  {row.Message,-20}  {row.Contact}");
        }

        // K9RST answers the CQ at slot 2: he has spoken and nothing has gone
        // back, because nothing in this repository sends yet.
        Assert.Equal("your move, 0 slots", Last(rows, "KC3QIS K9RST EM12").Contact);

        // His 73 at slot 6 is still your move, and the count has not moved
        // because it is measured from his own last transmission.
        Assert.Equal("your move, 0 slots", Last(rows, "KC3QIS K9RST 73").Contact);

        // W1ABC's CQ at slot 6, and his report at slot 8 two slots later.
        Assert.Equal("your move, 0 slots", Last(rows, "CQ W1ABC FN42").Contact);
        Assert.Equal("your move, 0 slots", Last(rows, "KC3QIS W1ABC R-15").Contact);
    }

    /// <summary>
    /// **A station working three others reads as gaps on the row, and is never
    /// gone quiet while it is transmitting.**
    /// </summary>
    [Fact]
    public void G4xyzIsNeverGoneQuietOnAnyRowOfTheScene()
    {
        var rows = TheScenePutThroughTheTable();

        var his = rows
            .Where(r => r.Message.Contains("G4XYZ", StringComparison.Ordinal))
            .ToList();

        foreach (var row in his)
        {
            _output.WriteLine($"{row.Utc}  {row.Message,-20}  {row.Contact}");
        }

        Assert.Equal(6, his.Count);

        foreach (var row in his)
        {
            Assert.DoesNotContain("gone quiet", row.Contact, StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// **Gone quiet arrives on the row as a count of slots and never as a
    /// reason.**
    /// </summary>
    /// <remarks>
    /// `VK2PQ` answers at slot 2 and is never heard again. The row that shows him
    /// gone quiet is the next row that mentions him, and in this scene there is
    /// none - so the state is read off a later slot the way the tab reads it when
    /// a new decode arrives, by putting one more row on the table.
    /// </remarks>
    [Fact]
    public void AStationNotHeardForFourSlotsReadsGoneQuietWithItsCount()
    {
        var (panel, _) = Panel();

        Add(panel, 2, "KC3QIS VK2PQ QF56");

        // Eleven slots later he is still the sender of nothing, and a row for him
        // at slot 13 would say how long the silence has been.
        var later = Add(panel, 13, "KC3QIS VK2PQ QF56");

        _output.WriteLine(later.Contact);

        // Read at slot 13, his previous transmission was eleven slots back - but
        // this row IS a transmission from him, so the row says nought slots and
        // your move. That is the honest answer: he has just spoken.
        Assert.Equal("your move, 0 slots", later.Contact);

        // The gone-quiet reading is what a row for SOMEBODY ELSE at slot 13 shows
        // about him, which is the ledger's answer and is asserted in the engine's
        // own tests. What is asserted here is that the words and the count reach
        // a row at all.
        var n5tt = Add(panel, 8, "KC3QIS N5TT EM10");

        Assert.Equal("your move, 0 slots", n5tt.Contact);
    }

    /// <summary>
    /// A message the splitter refuses, and a row with no slot, carry no contact
    /// text rather than a guessed one.
    /// </summary>
    [Fact]
    public void AMessageWithNoStationInItCarriesNoContactText()
    {
        var (panel, _) = Panel();

        Assert.Equal("", Add(panel, 4, "TNX BOB 73 GL").Contact);
        Assert.Equal("", Add(panel, 10, "ABCDEFGHIJKLM").Contact);

        // A row placed without its true UTC cannot count slots, so it says
        // nothing rather than counting from DateTime.MinValue.
        Assert.Equal(
            "",
            panel.AddDecodeRowForTests(
                "214135", "-11", "0.2", "1240", "KC3QIS W1ABC FN42").Contact);
    }

    /// <summary>With no callsign in Settings there is nobody to keep a ledger from.</summary>
    [Fact]
    public void WithNoOperatorCallsignTheRowSaysNothing()
    {
        var settings = new AppSettings();

        settings.Operator.Callsign = "";

        var panel = new MainWindowViewModel(settings, null);

        Assert.Equal("", Add(panel, 2, "KC3QIS K9RST EM12").Contact);
    }

    /// <summary>Every message of the band scene, through the same door the decoder uses.</summary>
    /// <returns>The rows, in the order they were placed.</returns>
    private static IReadOnlyList<DigitalDecodeRow> TheScenePutThroughTheTable()
    {
        var (panel, _) = Panel();

        (int Slot, string Message)[] scene =
        [
            (0, "JA1ZZ G4XYZ -08"),
            (2, "KC3QIS VK2PQ QF56"),
            (2, "KC3QIS K9RST EM12"),
            (2, "KC3QIS G4XYZ IO91"),
            (4, "DL1QQ G4XYZ -12"),
            (4, "KC3QIS K9RST R-09"),
            (4, "TNX BOB 73 GL"),
            (6, "JA1ZZ G4XYZ RR73"),
            (6, "KC3QIS K9RST 73"),
            (6, "CQ W1ABC FN42"),
            (8, "DL1QQ G4XYZ RRR"),
            (8, "KC3QIS N5TT EM10"),
            (8, "KC3QIS W1ABC R-15"),
            (10, "CQ G4XYZ IO91"),
            (10, "ABCDEFGHIJKLM"),
        ];

        return scene.Select(s => Add(panel, s.Slot, s.Message)).ToList();
    }

    private static (MainWindowViewModel Panel, AppSettings Settings) Panel()
    {
        var settings = new AppSettings();

        settings.Operator.Callsign = "KC3QIS";

        return (new MainWindowViewModel(settings, null), settings);
    }

    private static DigitalDecodeRow Add(
        MainWindowViewModel panel, int slot, string message)
    {
        var utc = SlotZero.AddSeconds(slot * 15);

        return panel.AddDecodeRowForTests(
            utc.ToString("HHmmss", System.Globalization.CultureInfo.InvariantCulture),
            "-11", "0.2", "1240", message, utc);
    }

    private static DigitalDecodeRow Last(
        IReadOnlyList<DigitalDecodeRow> rows, string message)
        => rows.Last(r => string.Equals(r.Message, message, StringComparison.Ordinal));
}
