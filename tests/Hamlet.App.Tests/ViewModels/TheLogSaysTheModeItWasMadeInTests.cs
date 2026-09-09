using System.Globalization;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Licensing;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// **The permanent log says the mode the contact was actually made in.** Work
/// instruction 293, task 4.
/// </summary>
/// <remarks>
/// <para>**THE BREAKAGE THIS CATCHES** (`CLAUDE.md`: a unit may not add a test
/// without naming it): **Tim's first FT4 contact written into the permanent record
/// as FT8.** `MainWindowViewModel`'s one write path handed in
/// `ContactModes.Named("FT8")` unconditionally. It was unreachable on FT4 only
/// because `CanLogRow` needs a station to have addressed the operator, and nothing
/// could address him on FT4 until Hamlet could transmit on it - **which this unit's
/// tasks 2 and 3 made possible.** It is the §0.0 fault in the one artefact
/// `PHASE_PLAN.md` says outlives everything else, and **no later unit can repair
/// it**, because by then nobody knows which records were wrong.</para>
/// <para>**AND THE OTHER DIRECTION IS AS IMPORTANT.** The failure mode of a shared
/// source is that FT8 quietly grows a submode. An FT8 contact must come out
/// byte-identical to what it was before this unit, with `SUBMODE` absent rather
/// than empty.</para>
/// <para>**NOTHING HERE OPENS A WINDOW OR TOUCHES THE LOG FILE.**
/// <c>ContactLogEntryFor</c> builds the record and writes nothing; the file is
/// touched by <c>LogContactAsync</c> and only where the operator pressed Save.
/// </para>
/// </remarks>
public sealed class TheLogSaysTheModeItWasMadeInTests
{
    /// <summary>The operator.</summary>
    private const string Mine = "KC3QIS";

    /// <summary>The station that addressed him.</summary>
    private const string His = "W1ABC";

    /// <summary>A message addressed to the operator, which is what `CanLogRow` needs.</summary>
    private const string HeardFromHim = Mine + " " + His + " R-15";

    /// <summary>FT4's watering hole on 20 m, from the cited band data.</summary>
    private const long Ft4On20m = 14_080_000;

    /// <summary>FT8's watering hole on 20 m.</summary>
    private const long Ft8On20m = 14_074_000;

    /// <summary>The slot the seeded row sits in.</summary>
    private static readonly DateTime RowSlot =
        new(2026, 9, 9, 18, 0, 0, DateTimeKind.Utc);

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where every record is printed.</param>
    public TheLogSaysTheModeItWasMadeInTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// **A contact made with FT4 chosen writes `MODE=MFSK` and `SUBMODE=FT4`.**
    /// </summary>
    /// <remarks>
    /// **THE MODE TRAVELS AS ONE OBJECT** - unit 291 built that - so the two tags
    /// cannot be set to disagree. Nothing here sets `Submode` by hand; it comes out
    /// of the `ContactMode` the conditions carried in.
    /// </remarks>
    [Fact]
    public void AContactMadeOnFt4IsLoggedAsMfskWithTheSubmode()
    {
        var contact = Logged(DigitalMode.Ft4, Ft4On20m);

        var record = AdifLog.Record(contact);

        _output.WriteLine("mode chosen  : FT4");
        _output.WriteLine("MODE         : " + contact.Mode);
        _output.WriteLine("SUBMODE      : " + contact.Submode);
        _output.WriteLine(record);

        Assert.Equal("MFSK", contact.Mode);
        Assert.Equal("FT4", contact.Submode);

        Assert.Contains("<MODE:4>MFSK", record, StringComparison.Ordinal);
        Assert.Contains("<SUBMODE:3>FT4", record, StringComparison.Ordinal);

        // **AND IT NEVER SAYS FT8**, which is what it said before this unit.
        Assert.DoesNotContain("FT8", record, StringComparison.Ordinal);
    }

    /// <summary>
    /// **THE CONTROL: a contact made with FT8 chosen is what it was before this
    /// unit, with no `SUBMODE` in the record at all.**
    /// </summary>
    /// <remarks>
    /// **ABSENT, NOT EMPTY.** Unit 291 pinned the FT8 record as a single string
    /// comparison and that pin is this test's control; what is asserted here is the
    /// application's own write path, which is the half unit 291 could not reach.
    /// </remarks>
    [Fact]
    public void AContactMadeOnFt8IsLoggedExactlyAsItAlwaysWas()
    {
        var contact = Logged(DigitalMode.Ft8, Ft8On20m);

        var record = AdifLog.Record(contact);

        _output.WriteLine("mode chosen  : FT8");
        _output.WriteLine("MODE         : " + contact.Mode);
        _output.WriteLine("SUBMODE      : " + (contact.Submode ?? "(absent)"));
        _output.WriteLine(record);

        Assert.Equal("FT8", contact.Mode);
        Assert.Null(contact.Submode);

        Assert.Contains("<MODE:3>FT8", record, StringComparison.Ordinal);
        Assert.DoesNotContain("SUBMODE", record, StringComparison.Ordinal);
        Assert.DoesNotContain("MFSK", record, StringComparison.Ordinal);
    }

    /// <summary>
    /// **The mode comes off `DigitalMode` and follows a press of the strip.**
    /// </summary>
    /// <remarks>
    /// **NOT THE CHIP'S LIT STATE AND NOT A STRING.** `IsLit` is *the dial is inside
    /// this mode's block*, which is a reading of where the radio is; what goes in
    /// the log is what Hamlet was actually running, which is the operator's own
    /// instruction. The two are different facts and only the second may reach a
    /// permanent record.
    /// </remarks>
    [Fact]
    public void TheModeInTheRecordFollowsTheOneValueTheTabRunsOn()
    {
        var (panel, _) = Panel(Ft4On20m);

        var row = panel.AddDecodeRowForTests(
            RowSlot.ToString("HHmmss", CultureInfo.InvariantCulture),
            DigitalDecodeRow.NoMeasurement, "0.2", "1240", HeardFromHim, RowSlot);

        panel.UseModeForTests(DigitalMode.Ft8);

        var asFt8 = panel.ContactLogEntryFor(row);

        panel.UseModeForTests(DigitalMode.Ft4);

        var asFt4 = panel.ContactLogEntryFor(row);

        _output.WriteLine("same row, FT8 chosen : " + asFt8!.Mode
            + " / " + (asFt8.Submode ?? "(absent)"));
        _output.WriteLine("same row, FT4 chosen : " + asFt4!.Mode
            + " / " + (asFt4.Submode ?? "(absent)"));

        Assert.Equal("FT8", asFt8.Mode);
        Assert.Null(asFt8.Submode);
        Assert.Equal("MFSK", asFt4.Mode);
        Assert.Equal("FT4", asFt4.Submode);
    }

    /// <summary>
    /// **The achievements FT4 row lights off a record the write path produced.**
    /// </summary>
    /// <remarks>
    /// <para>**IT IS NOT FORCED.** The record below is the one
    /// <c>ContactLogEntryFor</c> builds with FT4 chosen, handed to the same reader
    /// unit 291's own test uses. **Unit 292 was told not to light it early because
    /// Hamlet could not transmit on FT4; after tasks 2 and 3 it can**, and this is
    /// the first unit in which a real FT4 contact is reachable.</para>
    /// <para>**A BARE `MODE=MFSK` IS STILL NOT FT4** - unit 291's rule, and the
    /// reason the row reads the submode rather than the mode.</para>
    /// </remarks>
    [Fact]
    public void TheFt4AchievementLightsFromARecordTheWritePathMade()
    {
        var contact = Logged(DigitalMode.Ft4, Ft4On20m);
        var ft4 = ContactModes.Named("FT4")!;

        _output.WriteLine("record       : " + contact.Mode + " / " + contact.Submode);
        _output.WriteLine("the row wants: " + ft4.AdifSpelling);
        _output.WriteLine("matches      : " + ft4.Matches(contact.Mode, contact.Submode));

        Assert.True(ft4.Matches(contact.Mode, contact.Submode));

        // **AND FT8 IS NOT LIT BY IT**, which is the other half of naming the mode.
        Assert.False(
            ContactModes.Named("FT8")!.Matches(contact.Mode, contact.Submode));
    }

    // -------------------------------------------------------------------------

    /// <summary>The record one seeded row produces in one mode.</summary>
    private AdifContact Logged(DigitalMode mode, long frequencyHz)
    {
        var (panel, _) = Panel(frequencyHz);

        panel.UseModeForTests(mode);

        var row = panel.AddDecodeRowForTests(
            RowSlot.ToString("HHmmss", CultureInfo.InvariantCulture),
            DigitalDecodeRow.NoMeasurement, "0.2", "1240", HeardFromHim, RowSlot);

        var entry = panel.ContactLogEntryFor(row);

        Assert.True(entry is not null, "the seeded row could not be logged");

        return entry!;
    }

    /// <summary>A panel on one band, with an operator who can be addressed.</summary>
    private static (MainWindowViewModel Panel, AppSettings Settings) Panel(long frequencyHz)
    {
        var settings = new AppSettings();

        settings.Operator.Callsign = Mine;
        settings.Operator.GridSquare = "FN00";
        settings.Operator.LicenseClass = LicenseClass.General;

        var panel = new MainWindowViewModel(settings, null) { OperatingMode = "Digital" };

        panel.SelectedBand = panel.Bands.First(
            b => b.Band.LowHz <= frequencyHz && b.Band.HighHz >= frequencyHz);
        panel.FrequencyHz = frequencyHz;

        return (panel, settings);
    }
}
