using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Contacts;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 274, tasks 3 and 4: the dialog says what was observed, the
/// notes overwrite nothing, cancel writes nothing, and a station already worked
/// is marked.
/// </summary>
/// <remarks>
/// <para>**THE LOG FILE IS REDIRECTED FOR THE WHOLE OF THIS FILE.**
/// `SettingsStore.DataFolder` has an internal setter for exactly this — the seam
/// unit 235 added after nine tests rewrote the operator's own settings — so
/// nothing here can append to his real log.</para>
/// <para>**CANCEL IS GUARANTEED BY SHAPE AND NOT BY CARE.** The dialog's model
/// never touches a file; the caller writes only where `Saved` is true. So the test
/// for cancel is that `Saved` is false and nothing was written, and there is no
/// third path.</para>
/// </remarks>
public sealed class TheLogDialogAndTheWorkedMarkTests : IDisposable
{
    private const string His = "IK4LZH";
    private const string Mine = "KC3QIS";

    private static readonly DateTime Slot =
        new(2026, 9, 7, 21, 41, 30, DateTimeKind.Utc);

    private readonly ITestOutputHelper _output;
    private readonly string _folder;
    private readonly string _wasFolder;

    /// <summary>Creates the tests and redirects the data folder.</summary>
    /// <param name="output">Where the entries are printed.</param>
    public TheLogDialogAndTheWorkedMarkTests(ITestOutputHelper output)
    {
        _output = output;

        _wasFolder = SettingsStore.DataFolder;
        _folder = Path.Combine(
            Path.GetTempPath(), "hamlet-unit274-" + Guid.NewGuid().ToString("N"));

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
            // A folder that will not delete is the operating system's business.
        }
    }

    /// <summary>A complete contact fills every observable field of the dialog.</summary>
    [Fact]
    public void ACompleteContactPopulatesEveryObservableField()
    {
        var model = new LogContactViewModel(CompleteEntry(), "14.074000 MHz");

        foreach (var field in model.Fields)
        {
            _output.WriteLine(
                (field.Observed ? "  heard  " : "  empty  ")
                + field.Label.PadRight(16) + field.Shown);
        }

        Assert.Equal(model.Fields.Count, model.ObservedCount);
        Assert.All(model.Fields, f => Assert.True(f.Observed));

        _output.WriteLine("summary : " + model.Summary);
        Assert.Contains("all", model.Summary, StringComparison.Ordinal);
    }

    /// <summary>
    /// An incomplete contact leaves the missing field empty and marked.
    /// </summary>
    /// <remarks>
    /// **AN EMPTY BOX BESIDE A LABEL READS AS ONE HE FORGOT TO FILL.** It says
    /// Hamlet did not hear it instead, which is a different fact and the true one.
    /// </remarks>
    [Fact]
    public void AnIncompleteContactLeavesTheFieldEmptyAndSaysSo()
    {
        var model = new LogContactViewModel(
            CompleteEntry() with { ReportSent = null }, "14.074000 MHz");

        var sent = model.Fields.Single(f => f.AdifField == "RST_SENT");

        _output.WriteLine(sent.Label + " -> " + sent.Shown);

        Assert.False(sent.Observed);
        Assert.Equal("", sent.Value);
        Assert.Contains("did not hear", sent.Shown, StringComparison.Ordinal);

        // The one beside it was heard, so the absence is the missing fact rather
        // than the dialog having given up on the pair.
        Assert.True(model.Fields.Single(f => f.AdifField == "RST_RCVD").Observed);

        Assert.Contains("did not hear them", model.Summary, StringComparison.Ordinal);

        // And it stays out of the file rather than being written blank.
        Assert.DoesNotContain(
            "RST_SENT", AdifLog.Record(model.Entry), StringComparison.Ordinal);
    }

    /// <summary>Nothing he types overwrites anything the radio heard.</summary>
    /// <remarks>
    /// **THE OBSERVED FIELDS ARE NOT EDITABLE AT ALL**, which is the surest way to
    /// keep this true: there is nowhere to type over them. This asserts the model
    /// as well, so a later edit that made them editable would have to break a test
    /// to do it.
    /// </remarks>
    [Fact]
    public void NothingTypedOverwritesAnythingObserved()
    {
        var observed = CompleteEntry();
        var model = new LogContactViewModel(observed, "14.074000 MHz")
        {
            Notes = "He was 20 dB over here. Worked him on the second call.",
        };

        var entry = model.Entry;

        _output.WriteLine(AdifLog.Record(entry));

        Assert.Equal(observed.Call, entry.Call);
        Assert.Equal(observed.ReportSent, entry.ReportSent);
        Assert.Equal(observed.ReportReceived, entry.ReportReceived);
        Assert.Equal(observed.GridSquare, entry.GridSquare);
        Assert.Equal(observed.StartedUtc, entry.StartedUtc);

        // The notes are the one field that moved, and they went to COMMENT.
        Assert.Equal(
            "He was 20 dB over here. Worked him on the second call.", entry.Comment);
        Assert.Equal(observed with { Comment = entry.Comment }, entry);
    }

    /// <summary>Cancel writes nothing.</summary>
    [Fact]
    public void CancelWritesNothing()
    {
        var model = new LogContactViewModel(CompleteEntry()) { Notes = "typed and abandoned" };

        // Save was never pressed.
        Assert.False(model.Saved);

        // The caller writes only where it was, so nothing reaches the file.
        Assert.False(File.Exists(ContactLogStore.LogPath));
        Assert.Empty(ContactLogStore.Read());
    }

    /// <summary>Save writes one record, and a second appends rather than replaces.</summary>
    [Fact]
    public void SaveWritesOneRecordAndTheNextAppends()
    {
        var first = new LogContactViewModel(CompleteEntry()) { Notes = "first" };
        first.SaveCommand.Execute(null);

        Assert.True(first.Saved);
        Assert.True(ContactLogStore.Append(first.Entry, "1.12.133"));

        var second = new LogContactViewModel(CompleteEntry() with { Call = "W1ABC" });
        second.SaveCommand.Execute(null);

        Assert.True(ContactLogStore.Append(second.Entry, "1.12.133"));

        var text = File.ReadAllText(ContactLogStore.LogPath);

        _output.WriteLine(text);

        // **ONE HEADER, TWO RECORDS.** A second header appended later would be
        // read as a record with a version number in it.
        Assert.Equal(1, Count(text, "<EOH>"));
        Assert.Equal(2, Count(text, "<EOR>"));

        var read = ContactLogStore.Read();

        Assert.Equal(2, read.Count);
        Assert.Equal(His, read[0].Call);
        Assert.Equal("first", read[0].Comment);
        Assert.Equal("W1ABC", read[1].Call);
        Assert.Null(read[1].Comment);
    }

    /// <summary>The Log item is offered on his traffic and on nothing else.</summary>
    /// <remarks>
    /// **A LOG ITEM ON A CQ WOULD OFFER TO WRITE DOWN A CONTACT THAT HAS NOT
    /// HAPPENED.** `CanLogRow` asks `Ft8MessageSplit.IsAddressedTo`, the same
    /// question the mine side and the contact column ask.
    /// </remarks>
    [Theory]
    [InlineData("KC3QIS IK4LZH -12", true)]
    [InlineData("KC3QIS/P IK4LZH -12", true)]
    [InlineData("CQ IK4LZH JN54", false)]
    [InlineData("K9TC KJ6IX RRR", false)]
    [InlineData("TNX FER QSO OM", false)]
    public void TheLogItemIsOfferedOnHisTrafficOnly(string message, bool offered)
    {
        var model = Panel();
        var row = model.AddDecodeRowForTests("214135", "-12", "0.2", "1240", message);

        _output.WriteLine(message + " -> " + (model.CanLogRow(row) ? "Log" : "no Log"));

        Assert.Equal(offered, model.CanLogRow(row));
    }

    /// <summary>A callsign already in the log is marked, and one not in it is not.</summary>
    [Fact]
    public void AStationAlreadyWorkedIsMarked()
    {
        Assert.True(ContactLogStore.Append(CompleteEntry(), "1.12.133"));

        var model = Panel();

        var worked = model.AddDecodeRowForTests(
            "214135", "-12", "0.2", "1240", $"CQ {His} JN54");
        var fresh = model.AddDecodeRowForTests(
            "214135", "-11", "0.2", "1290", "CQ W1ABC FN31");

        _output.WriteLine(His + "  -> [" + worked.WorkedBefore + "]");
        _output.WriteLine("W1ABC   -> [" + fresh.WorkedBefore + "]");

        Assert.True(worked.HasWorkedBefore);
        Assert.False(fresh.HasWorkedBefore);

        // **THE MARK SAYS WHEN AND ON WHAT BAND**, which is what makes it useful
        // rather than only a warning.
        // **TIM'S WORDING SINCE UNIT 280**, replacing unit 274's. The date is his
        // format rather than ISO, and **the band came out**: it was a third clause
        // on a hover that already had two, and a date is what he wants when a
        // callsign looks familiar.
        Assert.Contains("09/07/26", worked.WorkedBefore, StringComparison.Ordinal);
        Assert.Contains(His, worked.WorkedBefore, StringComparison.Ordinal);
        Assert.DoesNotContain("20m", worked.WorkedBefore, StringComparison.Ordinal);
    }

    /// <summary>A compound call is a different station, and that is deliberate.</summary>
    /// <remarks>
    /// **WHERE THE ANSWER IS NOT CERTAIN, TREAT THEM AS DIFFERENT** (the
    /// instruction's own rule). `W4/YV7AXM` and `YV7AXM` are the same licensee and
    /// arguably not the same contact: the second is a Venezuelan station at home
    /// and the first is that licensee transmitting from Florida, a different DXCC
    /// entity and a different contact to most award programmes. **The mark
    /// under-claims rather than over-claims**, so he is never told he has worked
    /// somebody he has not.
    /// </remarks>
    [Fact]
    public void ACompoundCallIsADifferentStation()
    {
        Assert.True(ContactLogStore.Append(
            CompleteEntry() with { Call = "YV7AXM" }, "1.12.133"));

        var model = Panel();

        var home = model.AddDecodeRowForTests(
            "214135", "-12", "0.2", "1240", "CQ YV7AXM FK60");
        var portable = model.AddDecodeRowForTests(
            "214135", "-12", "0.2", "1290", "CQ W4/YV7AXM EL96");

        _output.WriteLine("YV7AXM    -> [" + home.WorkedBefore + "]");
        _output.WriteLine("W4/YV7AXM -> [" + portable.WorkedBefore + "]");

        Assert.True(home.HasWorkedBefore);
        Assert.False(portable.HasWorkedBefore);
    }

    /// <summary>A row already on screen picks the mark up when he logs.</summary>
    /// <remarks>
    /// **OTHERWISE THE MARK WOULD APPEAR ONLY ON ROWS THAT ARRIVE AFTERWARDS**,
    /// and the station's other rows from the same evening would go on looking
    /// unworked while he is reading them.
    /// </remarks>
    [Fact]
    public void RowsAlreadyOnScreenPickUpTheMark()
    {
        var model = Panel();

        var row = model.AddDecodeRowForTests(
            "214135", "-12", "0.2", "1240", $"CQ {His} JN54");

        Assert.False(row.HasWorkedBefore);

        Assert.True(ContactLogStore.Append(CompleteEntry(), "1.12.133"));
        model.ReloadContactLogForTests();

        _output.WriteLine("after logging -> [" + row.WorkedBefore + "]");

        Assert.True(row.HasWorkedBefore);
    }

    private static int Count(string text, string needle)
    {
        var n = 0;
        var at = 0;

        while ((at = text.IndexOf(needle, at, StringComparison.Ordinal)) >= 0)
        {
            n++;
            at += needle.Length;
        }

        return n;
    }

    private static MainWindowViewModel Panel()
    {
        var settings = new AppSettings();

        settings.Operator.Callsign = Mine;
        settings.Operator.GridSquare = "FN00DJ";

        return new MainWindowViewModel(settings, null);
    }

    private static AdifContact CompleteEntry() => new()
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
