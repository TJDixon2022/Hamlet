using System.Diagnostics;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Contacts;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 274, task 5: what the log costs after an evening, measured
/// rather than assumed.
/// </summary>
/// <remarks>
/// <para>**FOURTEEN MESSAGES A SLOT AND FOUR SLOTS A MINUTE IS FIFTY-SIX CHECKS A
/// MINUTE.** A file read behind each of them is the kind of thing that is free
/// tonight and slow in March, and nothing about reading the code would say
/// so.</para>
/// <para>**MEASURE AND REPORT. DO NOT OPTIMISE IT**, which is the instruction's
/// own rule, so nothing here changes a line. What it does is put a number on the
/// arrangement that already exists, and say plainly whether the check is done per
/// decode or once.</para>
/// </remarks>
public sealed class WhatTheLogCostsAfterAnEveningTests : IDisposable
{
    private const string Mine = "KC3QIS";

    private static readonly DateTime Slot =
        new(2026, 9, 7, 21, 41, 30, DateTimeKind.Utc);

    private readonly ITestOutputHelper _output;
    private readonly string _folder;
    private readonly string _wasFolder;

    /// <summary>Creates the tests and redirects the data folder.</summary>
    /// <param name="output">Where the measurements are printed.</param>
    public WhatTheLogCostsAfterAnEveningTests(ITestOutputHelper output)
    {
        _output = output;

        _wasFolder = SettingsStore.DataFolder;
        _folder = Path.Combine(
            Path.GetTempPath(), "hamlet-unit274-cost-" + Guid.NewGuid().ToString("N"));

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

    /// <summary>A hundred entries, and what they cost to carry.</summary>
    [Fact]
    public void AHundredEntriesAndWhatTheyCost()
    {
        for (var i = 0; i < 100; i++)
        {
            Assert.True(ContactLogStore.Append(Entry(i), "1.12.134"));
        }

        var bytes = new FileInfo(ContactLogStore.LogPath).Length;
        var entries = ContactLogStore.Read();

        _output.WriteLine("=== the file after a hundred contacts ===");
        _output.WriteLine("path         : " + ContactLogStore.LogPath);
        _output.WriteLine("entries      : " + entries.Count);
        _output.WriteLine("bytes        : " + bytes);
        _output.WriteLine(
            "per entry    : " + (bytes / 100) + " bytes");
        _output.WriteLine(
            "a year of it : " + (bytes * 10 / 1024) + " KB at a thousand contacts");
        _output.WriteLine("");

        Assert.Equal(100, entries.Count);

        // **HOW LONG A WHOLE RE-READ TAKES**, which is what happens when he logs a
        // contact — once, on a click, and never on the decode path.
        var reads = new List<double>();

        for (var i = 0; i < 20; i++)
        {
            var clock = Stopwatch.StartNew();
            var read = ContactLogStore.Read();
            clock.Stop();

            Assert.Equal(100, read.Count);
            reads.Add(clock.Elapsed.TotalMilliseconds);
        }

        reads.Sort();

        _output.WriteLine("=== reading the whole log, 20 times ===");
        _output.WriteLine("median  : " + reads[10].ToString("0.000") + " ms");
        _output.WriteLine("slowest : " + reads[^1].ToString("0.000") + " ms");
        _output.WriteLine("");

        // **AND HOW LONG THE CHECK ITSELF TAKES**, which is what runs per decode:
        // a dictionary lookup against the copy already in memory.
        var model = Panel();

        // The first row loads the log; everything after it is the check alone.
        model.AddDecodeRowForTests("214135", "-12", "0.2", "1240", "CQ K0000 FN31");

        var slotOfDecodes = new List<double>();

        for (var slot = 0; slot < 10; slot++)
        {
            var clock = Stopwatch.StartNew();

            // Fourteen messages, which is what the busiest band measured here
            // delivers in one fifteen-second slot.
            for (var i = 0; i < 14; i++)
            {
                model.AddDecodeRowForTests(
                    "2141" + slot.ToString("00"), "-12", "0.2", "1240",
                    "CQ K" + (slot * 14 + i).ToString("0000") + " FN31");
            }

            clock.Stop();
            slotOfDecodes.Add(clock.Elapsed.TotalMilliseconds);
        }

        slotOfDecodes.Sort();

        _output.WriteLine("=== fourteen decodes placed, one slot's worth, 10 times ===");
        _output.WriteLine("median  : " + slotOfDecodes[5].ToString("0.000") + " ms");
        _output.WriteLine("slowest : " + slotOfDecodes[^1].ToString("0.000") + " ms");
        _output.WriteLine(
            "against a slot of : 15000 ms");
        _output.WriteLine("");

        _output.WriteLine("=== per decode or once? ===");
        _output.WriteLine(
            "ONCE. The log is read into a dictionary the first time a row is");
        _output.WriteLine(
            "placed and kept, and re-read only when a contact is logged --");
        _output.WriteLine(
            "which is the one thing in the application that changes the file.");
        _output.WriteLine(
            "The per-decode cost is a case-insensitive dictionary lookup on the");
        _output.WriteLine(
            "sender's callsign. See MainWindowViewModel.PlaceRow.");

        // **NOT AN ASSERTION ABOUT THE TIMINGS.** The instruction says measure and
        // report; a threshold here would be this session deciding what is fast
        // enough on a machine that is not the operator's. What is asserted is that
        // the measurement happened and that the arrangement is the one claimed.
        Assert.True(bytes > 0, "the log measured no size");
        Assert.True(slotOfDecodes[^1] >= 0, "nothing was timed");
    }

    /// <summary>The check does not read the file per decode.</summary>
    /// <remarks>
    /// **THIS IS THE CLAIM, AND IT IS ASSERTED RATHER THAN TIMED.** A timing can
    /// be fast for the wrong reason — a warm cache, a small file — and would go on
    /// passing after somebody put a read back in. Deleting the file after the
    /// first row proves the later rows never touched it.
    /// </remarks>
    [Fact]
    public void TheCheckDoesNotReadTheFilePerDecode()
    {
        Assert.True(ContactLogStore.Append(Entry(0), "1.12.134"));

        var model = Panel();

        var first = model.AddDecodeRowForTests(
            "214135", "-12", "0.2", "1240", "CQ K0000 FN31");

        Assert.True(first.HasWorkedBefore);

        // **THE FILE GOES AWAY.** If the mark still lands, the log in memory is
        // what answered and no read happened.
        File.Delete(ContactLogStore.LogPath);

        var second = model.AddDecodeRowForTests(
            "214140", "-11", "0.2", "1290", "CQ K0000 FN31");

        _output.WriteLine(
            "with the file deleted, the mark is still ["
            + second.WorkedBefore + "]");

        Assert.True(second.HasWorkedBefore);
        Assert.Equal(first.WorkedBefore, second.WorkedBefore);
    }

    private static MainWindowViewModel Panel()
    {
        var settings = new AppSettings();

        settings.Operator.Callsign = Mine;
        settings.Operator.GridSquare = "FN00DJ";

        return new MainWindowViewModel(settings, null);
    }

    /// <summary>One entry, as an evening produces them.</summary>
    private static AdifContact Entry(int n) => new()
    {
        Call = "K" + n.ToString("0000"),
        StationCallsign = Mine,
        StartedUtc = Slot.AddMinutes(n),
        EndedUtc = Slot.AddMinutes(n).AddSeconds(60),
        Band = "20m",
        Mode = "FT8",
        FrequencyMhz = 14.074,
        ReportSent = "-09",
        ReportReceived = "-12",
        GridSquare = "JN54",
        MyGridSquare = "FN00DJ",
        Comment = "One of an evening's hundred.",
    };
}
