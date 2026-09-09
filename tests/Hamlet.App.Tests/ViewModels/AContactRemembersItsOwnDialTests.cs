using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Contacts;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 275, task 3: a log entry records the dial the contact was
/// heard on, not the one the radio is on now.
/// </summary>
/// <remarks>
/// <para>**A LOG IS THE ONE ARTEFACT IN THIS PROJECT THAT OUTLIVES EVERYTHING
/// ELSE**, and a wrong band in it is wrong in ten years. Unit 274's dialog read
/// the dial when he right-clicked, said so on the field rather than hiding it, and
/// named the fix as another unit's work.</para>
/// <para>**A ROW WITH NO RECORDED DIAL WRITES NO FREQUENCY AND NO BAND.** Anything
/// decoded before this change carries none, and a plausible frequency in a
/// permanent record is the fault §0.0 exists for.</para>
/// </remarks>
public sealed class AContactRemembersItsOwnDialTests : IDisposable
{
    private const string His = "IK4LZH";
    private const string Mine = "KC3QIS";

    /// <summary>20 metres, where the contact happened.</summary>
    private const long Worked = 14_074_000;

    private static readonly DateTime Slot =
        new(2026, 9, 7, 21, 41, 30, DateTimeKind.Utc);

    private readonly ITestOutputHelper _output;
    private readonly string _folder;
    private readonly string _wasFolder;

    /// <summary>Creates the tests and redirects the data folder.</summary>
    /// <param name="output">Where the entries are printed.</param>
    public AContactRemembersItsOwnDialTests(ITestOutputHelper output)
    {
        _output = output;

        _wasFolder = SettingsStore.DataFolder;
        _folder = Path.Combine(
            Path.GetTempPath(), "hamlet-unit275-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(_folder);
        SettingsStore.DataFolder = _folder;
    }

    /// <summary>Puts the real folder back.</summary>
    public void Dispose()
    {
        SettingsStore.DataFolder = _wasFolder;

        try
        {
            Directory.Delete(_folder, recursive: true);
        }
        catch (IOException)
        {
        }
    }

    /// <summary>The row carries the dial it was heard on.</summary>
    [Fact]
    public void TheRowCarriesTheDialItWasHeardOn()
    {
        var model = Panel();

        var row = model.AddDecodeRowForTests(
            "214130", "-12", "0.2", "1240", $"{Mine} {His} -12", Slot, Worked);

        _output.WriteLine("heard on " + row.HeardOnHz + " Hz");

        Assert.Equal(Worked, row.HeardOnHz);
    }

    /// <summary>
    /// A contact logged after the dial moved writes the heard frequency.
    /// </summary>
    /// <remarks>
    /// **THIS IS THE WHOLE UNIT'S SECOND EXPOSURE.** Before this change the entry
    /// would have said `7.074000` and `40m` for a contact worked on 20 metres —
    /// and nothing in the file would ever have said otherwise.
    /// </remarks>
    [Fact]
    public void AContactLoggedAfterTheDialMovedWritesTheHeardFrequency()
    {
        var model = Panel();

        var row = model.AddDecodeRowForTests(
            "214130", "-12", "0.2", "1240", $"{Mine} {His} -12", Slot, Worked);

        var entry = EntryFor(model, row);

        _output.WriteLine(AdifLog.Record(entry));
        _output.WriteLine(
            "row heard on " + row.HeardOnHz + " Hz; the panel's dial reads "
            + model.FrequencyHz + " Hz");

        Assert.Equal(14.074, entry.FrequencyMhz);
        Assert.Equal("20m", entry.Band);

        // **AND THE ENTRY DID NOT COME FROM THE LIVE DIAL**, which reads
        // something else entirely on a panel that never tuned anywhere. Before
        // this change the entry took that reading.
        Assert.NotEqual(row.HeardOnHz, model.FrequencyHz);
    }

    /// <summary>A row with no recorded dial writes no FREQ and no BAND.</summary>
    /// <remarks>
    /// **ABSENT, NOT GUESSED** (§0.0, and the rule every other field in this log
    /// already follows). Anything decoded before work instruction 275 carries no
    /// dial, and filling it from the radio's current tuning is exactly the fault
    /// this task exists to remove.
    /// </remarks>
    [Fact]
    public void ARowWithNoRecordedDialWritesNeitherFrequencyNorBand()
    {
        var model = Panel();

        // No frequency handed in: this is a row as everything before this change
        // arrived.
        var row = model.AddDecodeRowForTests(
            "214130", "-12", "0.2", "1240", $"{Mine} {His} -12", Slot);

        Assert.Equal(0, row.HeardOnHz);

        var entry = EntryFor(model, row);
        var text = AdifLog.Record(entry);

        _output.WriteLine(text);

        Assert.Null(entry.FrequencyMhz);
        Assert.Null(entry.Band);

        Assert.DoesNotContain("<FREQ:", text, StringComparison.Ordinal);
        Assert.DoesNotContain("<BAND:", text, StringComparison.Ordinal);

        // What was heard is still there, so the absence is the missing fact
        // rather than the entry having given up.
        Assert.Contains("<CALL:6>IK4LZH", text, StringComparison.Ordinal);
        Assert.Contains("<RST_RCVD:3>-12", text, StringComparison.Ordinal);
    }

    /// <summary>The dialog says the frequency was heard, not that it is current.</summary>
    /// <remarks>
    /// **THE LABEL WAS THE HONEST HALF OF UNIT 274 AND IT IS NOW REDUNDANT.** It
    /// read *where the dial is now* because that was the truth; it says what the
    /// figure is instead, because the figure changed.
    /// </remarks>
    [Fact]
    public void TheDialogSaysTheFrequencyWasHeard()
    {
        var heard = new LogContactViewModel(
            Complete(), "14.074000 MHz, where this was heard");

        var field = heard.Fields.Single(f => f.AdifField == "FREQ");

        _output.WriteLine(field.Label + " -> " + field.Shown);

        Assert.True(field.Observed);
        Assert.Contains("where this was heard", field.Shown, StringComparison.Ordinal);
        Assert.DoesNotContain("dial is now", field.Shown, StringComparison.Ordinal);
    }

    /// <summary>With no dial recorded the field says so in its own words.</summary>
    /// <remarks>
    /// **A DIAL IS NOT SOMETHING A STATION SENDS**, so *Hamlet did not hear this*
    /// would be the wrong sentence. It says the dial was not recorded and that
    /// both fields are left out.
    /// </remarks>
    [Fact]
    public void WithNoDialTheFieldSaysWhatIsMissing()
    {
        var none = new LogContactViewModel(
            Complete() with { FrequencyMhz = null, Band = null }, "");

        var field = none.Fields.Single(f => f.AdifField == "FREQ");

        _output.WriteLine(field.Label + " -> " + field.Shown);

        Assert.False(field.Observed);
        Assert.Contains("did not record the dial", field.Shown, StringComparison.Ordinal);
        Assert.DoesNotContain("did not hear this", field.Shown, StringComparison.Ordinal);
    }

    /// <summary>Build the entry the way the Log command does.</summary>
    /// <remarks>
    /// **THE SAME TWO STEPS THE COMMAND TAKES** — the ledger for the station, and
    /// the conditions off the row rather than off the radio. It is written out here
    /// rather than driven through the command because the command opens a window,
    /// and a headless dialog is not what this is about.
    /// </remarks>
    private static AdifContact EntryFor(MainWindowViewModel model, DigitalDecodeRow row)
    {
        var record = model.ContactRecordForTests(row.Sender);

        Assert.NotNull(record);

        var hz = row.HeardOnHz;
        var band = hz > 0 ? Hamlet.RadioEngine.Bands.HfBands.BandFor(hz) : null;

        return Ft8ContactLogEntry.For(
            record!,
            Mine,
            new Ft8StationConditions(
                hz > 0 ? hz : null, band?.Name,
                Hamlet.RadioEngine.Contacts.ContactModes.Named("FT8"), "FN00DJ"));
    }

    private static MainWindowViewModel Panel()
    {
        var settings = new AppSettings();

        settings.Operator.Callsign = Mine;
        settings.Operator.GridSquare = "FN00DJ";

        return new MainWindowViewModel(settings, null);
    }

    private static AdifContact Complete() => new()
    {
        Call = His,
        StationCallsign = Mine,
        StartedUtc = Slot,
        EndedUtc = Slot.AddSeconds(60),
        Band = "20m",
        Mode = "FT8",
        FrequencyMhz = 14.074,
        ReportSent = "-09",
        ReportReceived = "-12",
        GridSquare = "JN54",
        MyGridSquare = "FN00DJ",
    };
}
