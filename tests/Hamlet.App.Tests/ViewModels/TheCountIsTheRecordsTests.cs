using System;
using System.IO;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Contacts;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 278, task 3: the count is the number of records in the file,
/// and it comes off the read the *worked* mark already does.
/// </summary>
/// <remarks>
/// <para>**THE EXPOSURE IS THE COUNT** (§0.0). A number of contacts that is not the
/// number of records is a false claim about his own operating, and **it is the kind
/// of number a person repeats to other people.**</para>
/// <para>**THE OBVIOUS THING TO REACH FOR WAS THE WRONG NUMBER.** The mark keeps a
/// dictionary keyed by callsign and drops any record with no `CALL`, so its `Count`
/// is distinct stations. Working one station on three bands would have read as one
/// contact, and Tim's ruling of 2026-09-08 is that every logged contact counts.</para>
/// <para>**THE STATED RULE FOR A DAMAGED RECORD, WHICH THE INSTRUCTION ASKS FOR
/// EXPLICITLY: it is counted.** He made that contact; the file was cut off or a
/// field went bad afterwards. Leaving it out would make the status bar disagree with
/// the log window standing beside it, which lists it, and the window says separately
/// how many could not be read whole.</para>
/// </remarks>
public sealed class TheCountIsTheRecordsTests : IDisposable
{
    private const string HisCall = "KC3QIS";

    private readonly ITestOutputHelper _output;
    private readonly string _folder;
    private readonly string _wasFolder;

    /// <summary>Creates the tests and redirects the data folder.</summary>
    /// <param name="output">Where the counts are printed.</param>
    public TheCountIsTheRecordsTests(ITestOutputHelper output)
    {
        _output = output;

        // **NEVER HIS REAL LOG.** `SettingsStore.DataFolder` is the seam unit 235
        // added for exactly this, and this unit reads and never writes anyway.
        _wasFolder = SettingsStore.DataFolder;
        _folder = Path.Combine(
            Path.GetTempPath(), "hamlet-unit278-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(_folder);
        SettingsStore.DataFolder = _folder;
    }

    /// <summary>Puts the real folder back and removes the temporary one.</summary>
    public void Dispose()
    {
        SettingsStore.DataFolder = _wasFolder;

        try
        {
            Directory.Delete(_folder, recursive: true);
        }
        catch (IOException)
        {
            // A temporary folder that will not delete is not worth failing a test
            // over; the operating system will have it in the end.
        }
    }

    /// <summary>**Three records count three.**</summary>
    [Fact]
    public void ThreeRecordsCountThree()
    {
        Write(
            Entry("K9XP", "20260908", "021315"),
            Entry("W1ABC", "20260908", "031500"),
            Entry("VP2MAA", "20260908", "041500"));

        var model = Panel();

        _output.WriteLine(model.ContactCountLine);

        Assert.Equal(3, model.LoggedContacts);
        // **THE COUNT IS A NUMBER SINCE UNIT 280** (Tim: the contacts are
        // understated). It is drawn large with the word small beside it, so the
        // line itself is the figure and the word is its own property.
        Assert.Equal("3", model.ContactCountLine);
        Assert.Equal("contacts", model.ContactCountWord);
        Assert.True(model.HasLoggedContacts);
    }

    /// <summary>
    /// **The same station three times counts three, not one.**
    /// </summary>
    /// <remarks>
    /// Tim's ruling, 2026-09-08: every logged contact counts, and no judgement is
    /// made about whether two records are the same station. **This is the test that
    /// fails if anybody ever reaches for the worked-mark dictionary**, whose whole
    /// purpose is to collapse exactly these three into one.
    /// </remarks>
    [Fact]
    public void TheSameStationThreeTimesCountsThree()
    {
        Write(
            Entry("K9XP", "20260908", "021315", "20m"),
            Entry("K9XP", "20260908", "031500", "40m"),
            Entry("K9XP", "20260908", "041500", "15m"));

        var model = Panel();

        _output.WriteLine(model.ContactCountLine);

        Assert.Equal(3, model.LoggedContacts);

        // **AND THE MARK STILL WORKS OFF THE SAME READ**, which is the other
        // question those three records answer. A row for K9XP says worked; one
        // for a station he has never worked does not.
        var worked = model.AddDecodeRowForTests(
            "021315", "-09", "0.2", "1240", HisCall + " K9XP -09");
        var fresh = model.AddDecodeRowForTests(
            "021315", "-09", "0.2", "1240", HisCall + " W1ABC -09");

        Assert.True(worked.HasWorkedBefore);
        Assert.False(fresh.HasWorkedBefore);
    }

    /// <summary>
    /// **A damaged record is counted, and that is the rule, stated.**
    /// </summary>
    /// <remarks>
    /// <para>**THE INSTRUCTION ASKS FOR A STATED RULE RATHER THAN WHATEVER THE
    /// PARSER HAPPENS TO DO**, and this is it: **every record the reader returns is
    /// counted, sound or not.**</para>
    /// <para>**WHY THAT WAY ROUND.** He made the contact; the file was cut off
    /// afterwards, or a field went bad. Not counting it would make the status bar
    /// say two where the log window standing beside it lists three, and of the two
    /// numbers the one that disagrees with what he can see is the wrong one.</para>
    /// </remarks>
    [Fact]
    public void ADamagedRecordIsCountedAndTheWindowAgrees()
    {
        // Two whole records and a third cut off mid-field, which is what a crash
        // during a write leaves behind.
        File.WriteAllText(
            ContactLogStore.LogPath,
            AdifLog.Header("1.12.151")
            + Entry("K9XP", "20260908", "021315")
            + Entry("W1ABC", "20260908", "031500")
            + "<CALL:6>VP2MAA<QSO_DATE:8>20260908<TIME_ON:6>0415");

        var model = Panel();
        var window = new ContactLogViewModel(
            ContactLogStore.ReadRecords(), ContactLogStore.LogPath);

        _output.WriteLine("status bar: " + model.ContactCountLine);
        _output.WriteLine("window:     " + window.CountLine);
        _output.WriteLine("damaged:    " + window.DamageLine);

        Assert.Equal(3, model.LoggedContacts);

        // **THE TWO NUMBERS ARE THE SAME NUMBER**, which is the point of counting
        // damage in rather than out.
        Assert.Equal(model.LoggedContacts, window.Count);
        Assert.Equal(1, window.Damaged);
        Assert.Equal(3, window.Contacts.Count);
    }

    /// <summary>
    /// **An empty log counts nothing and the line stays off the screen.**
    /// </summary>
    /// <remarks>
    /// `0 contacts logged` on a first launch is a scoreboard nobody asked for, and
    /// the instruction asked for this quietly.
    /// </remarks>
    [Fact]
    public void NothingLoggedShowsNothing()
    {
        var model = Panel();

        Assert.Equal(0, model.LoggedContacts);
        Assert.False(model.HasLoggedContacts);
    }

    /// <summary>
    /// **The count is read once, and logging a contact brings it up to date.**
    /// </summary>
    /// <remarks>
    /// The count and the mark are two derivations of one read of the file (task 3),
    /// so the re-read after a contact is logged has to move both. A count that went
    /// stale the moment he logged something would be the wrong number on screen at
    /// exactly the moment he looked at it.
    /// </remarks>
    [Fact]
    public void LoggingSomethingBringsTheCountUpToDate()
    {
        Write(Entry("K9XP", "20260908", "021315"));

        var model = Panel();

        Assert.Equal(1, model.LoggedContacts);

        Write(
            Entry("K9XP", "20260908", "021315"),
            Entry("W1ABC", "20260908", "031500"));

        model.ReloadContactLogForTests();

        _output.WriteLine(model.ContactCountLine);

        Assert.Equal(2, model.LoggedContacts);
    }

    /// <summary>A panel with his callsign, reading the redirected folder.</summary>
    private static MainWindowViewModel Panel()
    {
        var settings = new AppSettings();

        settings.Operator.Callsign = HisCall;

        return new MainWindowViewModel(settings, null);
    }

    /// <summary>One record, written by `AdifLog` rather than by hand.</summary>
    /// <remarks>
    /// **THROUGH THE WRITER** (§12.5). A fixture built from the same understanding
    /// as the code proves nothing, and a hand-written length is the exact mistake
    /// this session already made once in task 2.
    /// </remarks>
    private static string Entry(
        string call, string date, string time, string? band = null)
        => AdifLog.Record(new AdifContact
        {
            Call = call,
            StationCallsign = HisCall,
            StartedUtc = DateTime.ParseExact(
                date + time, "yyyyMMddHHmmss",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.AdjustToUniversal
                    | System.Globalization.DateTimeStyles.AssumeUniversal),
            Band = band,
            Mode = "FT8",
        });

    /// <summary>Put a log in the redirected folder.</summary>
    private static void Write(params string[] entries)
        => File.WriteAllText(
            ContactLogStore.LogPath,
            AdifLog.Header("1.12.151") + string.Concat(entries));
}
