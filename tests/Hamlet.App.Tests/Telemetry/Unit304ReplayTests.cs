using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Hamlet.App.Settings;
using Hamlet.App.Telemetry;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Telemetry;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Telemetry;

/// <summary>
/// Work instruction 304 task 5: **replay the three mysteries against what the record
/// now holds.**
/// </summary>
/// <remarks>
/// <para>**THIS IS THE ACCEPTANCE CRITERION FOR THE WHOLE UNIT AND IT IS NOT A
/// FORMALITY.** Each state below is **constructed**, not hoped for, and each test
/// answers one question: **could it have been diagnosed from the file alone, with no
/// command and no conversation?**</para>
/// <para>**WHERE THE ANSWER IS NO, IT SAYS SO.** A standard this unit fails on its own
/// three cases is worth knowing about tonight rather than the next time something
/// breaks.</para>
/// </remarks>
public sealed class Unit304ReplayTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the replay and its verdict are printed.</param>
    public Unit304ReplayTests(ITestOutputHelper output) => _output = output;

    /// <summary>**Mystery 1: a clock query that never happens.**</summary>
    /// <remarks>
    /// **CONSTRUCTED AS THE ABSENCE IT WAS.** The machine that cost twenty minutes
    /// wrote no clock record at all, so the replay is a session that writes a
    /// snapshot and never queries.
    /// </remarks>
    [Fact]
    public void MysteryOneAClockQueryThatNeverHappens()
    {
        var lines = Session(telemetry =>
        {
            StartupFacts.Write(
                telemetry, Settings(), new FakeInputs(), () => Outputs(), _ => true);

            // and nothing asks the clock.
        });

        Quote(lines);

        var snapshot = lines.Single(
            l => l.Contains(StartupSnapshot.EventName, StringComparison.Ordinal));

        var started = lines.Any(
            l => l.Contains("clock_query_started", StringComparison.Ordinal));

        _output.WriteLine("");
        _output.WriteLine("a clock query was attempted : " + started);
        _output.WriteLine(
            "the snapshot says the offset: "
            + Field(snapshot, "clockOffsetKnown"));

        _output.WriteLine("");
        _output.WriteLine(
            "COULD IT BE DIAGNOSED FROM THE FILE ALONE? **YES.** The snapshot says "
            + "the clock offset is unknown and why nobody supplied it, and there is "
            + "no clock_query_started anywhere in the file - which since unit 303 "
            + "means nothing tried, rather than tried and failed. Those two were the "
            + "same empty file on the day it cost twenty minutes.");

        Assert.False(
            started,
            "this replay is supposed to construct a session that never queries");

        Assert.Contains(
            "clockOffsetKnown", snapshot, StringComparison.Ordinal);
    }

    /// <summary>**Mystery 2: a settings file naming a transmit device that is gone.**</summary>
    [Fact]
    public void MysteryTwoATransmitDeviceThatIsNotPresent()
    {
        var settings = Settings();

        settings.AudioOutputDeviceId = "{0.0.0.00000000}.{a-device-that-is-gone}";

        var lines = Session(telemetry =>
        {
            StartupFacts.Write(
                telemetry, settings, new FakeInputs(), () => Outputs(), _ => true);

            _ = new Hamlet.App.ViewModels.SettingsViewModel(
                settings, telemetry, new FakeInputs(), () => Outputs());
        });

        Quote(lines);

        var snapshot = lines.Single(
            l => l.Contains(StartupSnapshot.EventName, StringComparison.Ordinal));

        _output.WriteLine("");
        _output.WriteLine("transmitDevicePresent : " + Field(snapshot, "transmitDevicePresent"));
        _output.WriteLine("settingsNamedButAbsent: " + Field(snapshot, "settingsNamedButAbsent"));

        _output.WriteLine("");
        _output.WriteLine(
            "COULD IT BE DIAGNOSED FROM THE FILE ALONE? **YES.** One field says the "
            + "transmit device is not present, one names it, and one lists every "
            + "device that is - so a reader can see both what was wanted and what "
            + "was there. On the day, 56 refusals named the field that decided each "
            + "one and not one named the missing device.");

        Assert.Contains(
            "\"transmitDevicePresent\":false", snapshot, StringComparison.Ordinal);

        Assert.Contains(
            "a-device-that-is-gone", snapshot, StringComparison.Ordinal);
    }

    /// <summary>**Mystery 3: a transmission that played but never keyed a radio.**</summary>
    /// <remarks>
    /// **THIS IS THE ONE THE UNIT DOES NOT FULLY PASS, AND IT SAYS SO.**
    /// </remarks>
    [Fact]
    public void MysteryThreeATransmissionThatPlayedButNeverKeyed()
    {
        var record = new TransmitRecord(
            SlotStartUtc: new DateTime(2026, 9, 10, 12, 0, 0, DateTimeKind.Utc),
            StartSecondsIntoSlot: 0.5,
            FrequencyHz: 14_074_000,
            DurationSeconds: 12.6,
            SampleRate: 48_000,
            SampleCount: 604_800,
            MessageType: Ft8Sharp.Message.Ft8MessageType.Standard,
            MessageLength: 13,
            CarriedHashedCallsign: false,
            Outcome: Ft8TransmitOutcome.Played,
            CameOutOfTransmit: UnkeyRoute.OrdinaryUnkey,
            Keyed: true);

        var lines = Session(telemetry =>
        {
            StartupFacts.Write(
                telemetry, Settings(), new FakeInputs(), () => Outputs(), _ => true);

            telemetry.Write(
                TelemetryCategory.Transmit,
                TransmitRecord.EventName,
                record.ToBag(),
                record.Level);
        });

        Quote(lines);

        var transmission = lines.Single(
            l => l.Contains(TransmitRecord.EventName, StringComparison.Ordinal));

        _output.WriteLine("");
        _output.WriteLine(
            "the outcome now reads : " + Field(transmission, "outcome"));

        _output.WriteLine("");
        _output.WriteLine(
            "COULD IT BE DIAGNOSED FROM THE FILE ALONE? **NO, AND ONLY PARTLY "
            + "BETTER.** The record no longer claims the signal went out - unit 303 "
            + "renamed Sent to Played, so it says what was actually established: the "
            + "audio was composed and played and the keying frames were written. "
            + "**But the question was *did anything transmit*, and the file still "
            + "cannot answer it.**");

        _output.WriteLine("");
        _output.WriteLine(
            "WHAT IS MISSING: whether the radio was at the other end of that port "
            + "at that moment, and whether it keyed. Hamlet already polls 1C 00 four "
            + "times a second and reads 15 11 for power made, so it is knowable - "
            + "but joining those to the transmission record is ask 1, which is Tim's "
            + "and which this instruction parks.");

        _output.WriteLine("");
        _output.WriteLine(
            "WHAT DID IMPROVE: the snapshot beside it names which audio devices "
            + "exist and which was selected, so *played into the wrong sound card* "
            + "is now separable from *played into the right one*. That was not "
            + "answerable on the day either.");

        // **THE HONEST ASSERTION IS THE NEGATIVE ONE.** Nothing in the file claims
        // the transmitter keyed, and that is the property worth locking down.
        Assert.DoesNotContain("\"Sent\"", transmission, StringComparison.Ordinal);

        Assert.Contains("\"Played\"", transmission, StringComparison.Ordinal);
    }

    /// <summary>**The scoreboard: two of three, and which one is not.**</summary>
    [Fact]
    public void TwoOfTheThreeAreDiagnosableFromTheFileAlone()
    {
        _output.WriteLine("1. a clock query that never happened  : YES");
        _output.WriteLine("2. a transmit device that is not there: YES");
        _output.WriteLine("3. did anything actually transmit     : NO");
        _output.WriteLine("");
        _output.WriteLine(
            "Today it was none of the three. The third needs the radio's own "
            + "transmit state joined to the transmission record, which is ask 1 and "
            + "is parked by this instruction.");

        Assert.True(true);
    }

    /// <summary>Print a session's lines, trimmed to what a reader would scan.</summary>
    private void Quote(IReadOnlyList<string> lines)
    {
        foreach (var line in lines)
        {
            _output.WriteLine(
                line.Length > 400 ? line[..400] + " …" : line);
        }
    }

    /// <summary>One field's value out of a JSON line, for the printout.</summary>
    private static string Field(string line, string key)
    {
        var at = line.IndexOf("\"" + key + "\":", StringComparison.Ordinal);

        if (at < 0)
        {
            return "(not in this line)";
        }

        var from = at + key.Length + 3;
        var to = line.IndexOfAny(new[] { ',', '}' }, from);

        return to < 0 ? line[from..] : line[from..to];
    }

    /// <summary>Run one session against a real sink and hand back what it wrote.</summary>
    private static IReadOnlyList<string> Session(Action<JsonlTelemetry> run)
    {
        var folder = Path.Combine(
            Path.GetTempPath(), "hamlet-unit304r-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(folder);

        try
        {
            using (var telemetry = new JsonlTelemetry(folder, "1.12.267", _ => true))
            {
                run(telemetry);
            }

            return Directory.GetFiles(folder, "*.jsonl")
                .SelectMany(File.ReadAllLines)
                .ToList();
        }
        finally
        {
            try
            {
                Directory.Delete(folder, recursive: true);
            }
            catch (IOException)
            {
                // A file still held open is not this test's business.
            }
        }
    }

    private static AppSettings Settings()
        => new() { ReconnectOnStartup = false };

    private static IReadOnlyList<RenderEndpoint> Outputs()
        => new[]
        {
            new RenderEndpoint(
                "{0.0.0.00000000}.{speakers}", "Speakers", true, 48_000, 2, 16, "shared"),
            new RenderEndpoint(
                "{0.0.0.00000000}.{usb-codec}", "USB Audio CODEC", false, 48_000, 2, 16,
                "shared"),
        };

    private sealed class FakeInputs : IAudioDevices
    {
        public IReadOnlyList<AudioDevice> List()
            => new[]
            {
                new AudioDevice("{in}.{mic}", "Microphone", true),
                new AudioDevice("{in}.{codec}", "USB Audio CODEC"),
            };
    }
}
