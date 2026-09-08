using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Contacts;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 278, task 5: what the log costs at a thousand records and at
/// ten thousand.
/// </summary>
/// <remarks>
/// <para>**MEASURE AND REPORT. DO NOT OPTIMISE**, and do not add paging on the
/// strength of a number nobody has looked at yet. Unit 274 measured 265 bytes an
/// entry and 0.217 ms to read a hundred; the badges go to ten thousand, so somebody
/// should look.</para>
/// <para>**NEVER HIS REAL LOG.** Everything here is synthesised into a temporary
/// folder through `SettingsStore.DataFolder`, the seam unit 235 added, and the
/// folder goes away in `Dispose`.</para>
/// <para>**THE FIGURES ARE PRINTED RATHER THAN ASSERTED TIGHTLY.** A wall-clock
/// threshold in a test is a test that fails on somebody else's machine for a reason
/// that is not a defect. What is asserted is what must be true regardless of speed:
/// every record comes back, and the ordering holds.</para>
/// </remarks>
public sealed class WhatTheLogCostsAtTenThousandTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly string _folder;
    private readonly string _wasFolder;

    /// <summary>Creates the tests and redirects the data folder.</summary>
    /// <param name="output">Where the measurements are printed.</param>
    public WhatTheLogCostsAtTenThousandTests(ITestOutputHelper output)
    {
        _output = output;

        _wasFolder = SettingsStore.DataFolder;
        _folder = Path.Combine(
            Path.GetTempPath(), "hamlet-unit278-cost-" + Guid.NewGuid().ToString("N"));

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
            // Not worth failing a measurement over.
        }
    }

    /// <summary>**A thousand records, and ten thousand: time and size.**</summary>
    [Theory]
    [InlineData(1_000)]
    [InlineData(10_000)]
    public void TheReadIsMeasuredAtSize(int records)
    {
        Synthesise(records);

        var bytes = new FileInfo(ContactLogStore.LogPath).Length;

        // **WARMED FIRST**, because the first read of the run pays for the file
        // system and the parser's own JIT, and reporting that as the cost of ten
        // thousand records would be measuring the wrong thing.
        _ = ContactLogStore.ReadRecords();

        var readClock = Stopwatch.StartNew();
        var read = ContactLogStore.ReadRecords();
        readClock.Stop();

        var viewClock = Stopwatch.StartNew();
        var view = new ContactLogViewModel(read, ContactLogStore.LogPath);
        viewClock.Stop();

        _output.WriteLine(records.ToString("N0") + " records");
        _output.WriteLine("  file        " + bytes.ToString("N0") + " bytes ("
            + (bytes / records) + " an entry)");
        _output.WriteLine("  read        " + readClock.Elapsed.TotalMilliseconds.ToString("F2") + " ms");
        _output.WriteLine("  build view  " + viewClock.Elapsed.TotalMilliseconds.ToString("F2") + " ms");
        _output.WriteLine("  total       "
            + (readClock.Elapsed.TotalMilliseconds + viewClock.Elapsed.TotalMilliseconds)
                .ToString("F2") + " ms");
        _output.WriteLine("  rows        " + view.Contacts.Count.ToString("N0"));
        _output.WriteLine("  badge       " + view.Milestones.Line);
        _output.WriteLine("");

        // **WHAT IS ASSERTED IS CORRECTNESS, NOT SPEED.** Every record came back,
        // nothing was dropped, and the newest is still first.
        Assert.Equal(records, read.Count);
        Assert.Equal(records, view.Count);
        Assert.Equal(records, view.Contacts.Count);
        Assert.Equal(0, view.Damaged);
        Assert.True(
            view.Contacts[0].StartedUtc >= view.Contacts[^1].StartedUtc,
            "the newest row is not first");
    }

    /// <summary>
    /// **The count the status bar shows costs one read, whatever the size.**
    /// </summary>
    /// <remarks>
    /// The count and the *worked* mark are two derivations of one read (task 3), so
    /// this is the figure that matters on the main screen: it is paid once, lazily,
    /// and again only when he logs something.
    /// </remarks>
    [Fact]
    public void TheCountOnTheMainScreenIsOneReadAtTenThousand()
    {
        Synthesise(10_000);

        var settings = new AppSettings();

        settings.Operator.Callsign = "KC3QIS";

        var model = new MainWindowViewModel(settings, null);

        var clock = Stopwatch.StartNew();
        var count = model.LoggedContacts;
        clock.Stop();

        _output.WriteLine("first ask of the count at 10,000 records: "
            + clock.Elapsed.TotalMilliseconds.ToString("F2") + " ms");

        var again = Stopwatch.StartNew();
        _ = model.LoggedContacts;
        again.Stop();

        _output.WriteLine("second ask: "
            + again.Elapsed.TotalMilliseconds.ToString("F3") + " ms");
        _output.WriteLine("badge line: [" + model.ContactBadgeLine + "]");

        Assert.Equal(10_000, count);
        Assert.Equal("10,000", model.ContactCountLine);
        Assert.Equal("contacts", model.ContactCountWord);

        // **AND HE IS NOT CONGRATULATED FOR NINE BADGES AT ONCE**, which is what
        // this measurement found: installed beside a log of ten thousand it read
        // *That is 10 and 25 and 50 and 100 and 500 and 1,000 and 2,000 and 5,000
        // and 10,000 contacts logged*, in one line, in the status bar. The first
        // look seeds where he stands and says nothing, because an acknowledgement
        // is for a milestone he has just passed and Hamlet was not there for the
        // others.
        Assert.Equal("", model.ContactBadgeLine);
        Assert.False(model.HasContactBadge);
        Assert.Equal(10_000, model.Milestones.Highest);

        // **THE SECOND ASK DOES NOT RE-READ THE FILE.** That is what makes the
        // status bar's number free once the mark has paid for it.
        Assert.True(
            again.Elapsed < clock.Elapsed,
            "the second ask re-read the file");
    }

    /// <summary>Write a log of this many well-formed records.</summary>
    /// <remarks>
    /// **THROUGH `AdifLog.Record`** (§12.5), so the synthesised file is the file
    /// the application actually writes rather than a fixture that agrees with a
    /// misunderstanding. The callsigns repeat on purpose: every logged contact
    /// counts, so a thousand records from two hundred stations is a thousand.
    /// </remarks>
    private static void Synthesise(int records)
    {
        var text = new StringBuilder(AdifLog.Header("1.12.153"));
        var start = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        for (var i = 0; i < records; i++)
        {
            text.Append(AdifLog.Record(new AdifContact
            {
                Call = "K" + (i % 200).ToString("000") + "AB",
                StationCallsign = "KC3QIS",
                StartedUtc = start.AddMinutes(i),
                Band = "20m",
                Mode = "FT8",
                ReportSent = "-09",
                ReportReceived = "-14",
                GridSquare = "EM10",
                MyGridSquare = "FN00",
                Comment = "synthesised for work instruction 278 task 5",
            }));
        }

        File.WriteAllText(ContactLogStore.LogPath, text.ToString());
    }
}
