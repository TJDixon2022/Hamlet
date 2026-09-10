using System;
using System.Collections.Generic;
using System.Linq;
using Hamlet.App.Settings;
using Hamlet.App.Telemetry;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Telemetry;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Telemetry;

/// <summary>
/// Work instruction 304 task 2: **the startup snapshot.**
/// </summary>
/// <remarks>
/// <para>**TIM'S RULING, 2026-09-10**: *telemetry should be able to diagnose any
/// issue.* On that one day it failed three times and the author spent the day asking
/// him to type commands instead.</para>
/// <para>**THE BREAKAGE THIS CATCHES IS A FACT QUIETLY MISSING.** Every one of the
/// three failures was a state nobody could see: which sound card was selected,
/// whether a clock reading existed, whether a port write reached a radio. **So a
/// field that cannot be determined says `unknown` and why, and the test that matters
/// most is that a snapshot still goes out when the machine is broken enough to throw
/// while being described.**</para>
/// </remarks>
public sealed class Unit304SnapshotTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the snapshot is quoted.</param>
    public Unit304SnapshotTests(ITestOutputHelper output) => _output = output;

    /// <summary>**The snapshot carries every fact task 1 asked for.**</summary>
    [Fact]
    public void TheSnapshotCarriesEveryFactTaskOneAskedFor()
    {
        var sink = new Recording();

        StartupFacts.Write(
            sink, Settings(), new FakeInputs(), () => Outputs(), _ => true);

        var snapshot = sink.Only();

        foreach (var (key, value) in snapshot.OrderBy(k => k.Key, StringComparer.Ordinal))
        {
            _output.WriteLine("  " + key.PadRight(30) + value);
        }

        // The seven groups from `docs/telemetry-diagnoses-it.md`.
        foreach (var required in new[]
        {
            "appVersion", "ft8SharpVersion", "framework", "osBuild",
            "audioInputs", "audioInputSelected", "audioInputPresent",
            "audioOutputs", "transmitDeviceSelected", "transmitDevicePresent",
            "settingsLoaded", "settingsNamedButAbsent",
            "telemetryCategoriesOn", "telemetryCategoriesOff",
            "telemetryEventsDropped",
        })
        {
            Assert.True(
                snapshot.ContainsKey(required),
                "the snapshot does not carry " + required);
        }
    }

    /// <summary>**A missing transmit device is visible in one field.**</summary>
    /// <remarks>
    /// **THIS IS THE CQ MYSTERY, AND IT IS THE FIELD THAT WOULD HAVE ANSWERED IT.**
    /// 56 refusals were written, each naming the field that decided it, and not one
    /// named the missing device, because it was upstream of what readiness looks at.
    /// </remarks>
    [Fact]
    public void AMissingTransmitDeviceIsVisibleInOneField()
    {
        var settings = Settings();

        settings.AudioOutputDeviceId = "{0.0.0.00000000}.{a-device-that-is-gone}";

        var sink = new Recording();

        StartupFacts.Write(
            sink, settings, new FakeInputs(), () => Outputs(), _ => true);

        var snapshot = sink.Only();

        _output.WriteLine("transmitDeviceSelected : " + snapshot["transmitDeviceSelected"]);
        _output.WriteLine("transmitDevicePresent  : " + snapshot["transmitDevicePresent"]);
        _output.WriteLine("settingsNamedButAbsent : " + snapshot["settingsNamedButAbsent"]);
        _output.WriteLine("audioOutputs           : " + snapshot["audioOutputs"]);

        Assert.Equal(false, snapshot["transmitDevicePresent"]);

        Assert.Contains(
            "a-device-that-is-gone",
            snapshot["settingsNamedButAbsent"]?.ToString() ?? "",
            StringComparison.Ordinal);
    }

    /// <summary>**A fact that cannot be determined says unknown, and why.**</summary>
    /// <remarks>
    /// **NOT ABSENT, NOT A PLAUSIBLE DEFAULT** - today's own lesson. A wrong
    /// diagnostic cost an hour; a missing one would have cost a question.
    /// </remarks>
    [Fact]
    public void AFactThatCannotBeDeterminedSaysUnknownAndWhy()
    {
        var sink = new Recording();

        // A machine whose sound cards cannot be enumerated at all.
        StartupFacts.Write(
            sink, Settings(), new ThrowingInputs(), () => Outputs(), _ => true);

        var snapshot = sink.Only();

        _output.WriteLine("audioInputs    : " + snapshot["audioInputs"]);
        _output.WriteLine("audioInputsWhy : " + snapshot["audioInputsWhy"]);

        Assert.Equal(StartupSnapshot.Unknown, snapshot["audioInputs"]);

        Assert.Contains(
            "the sound card driver fell over",
            snapshot["audioInputsWhy"]?.ToString() ?? "",
            StringComparison.Ordinal);

        // **AND THE REST OF THE SNAPSHOT STILL WENT OUT**, which is the whole
        // argument for wrapping each reader on its own.
        Assert.NotEqual(StartupSnapshot.Unknown, snapshot["framework"]);
    }

    /// <summary>**Which categories are off, so a reader can read an absence.**</summary>
    /// <remarks>
    /// **MEASURED IN `JsonlTelemetry.Write`: A CATEGORY THAT IS OFF RETURNS BEFORE
    /// SERIALISING**, so its events leave no trace whatever. Without this field a
    /// reader cannot tell *nothing happened* from *not recorded* - which is the exact
    /// confusion that cost twenty minutes on the clock.
    /// </remarks>
    [Fact]
    public void TheSnapshotSaysWhichCategoriesAreOff()
    {
        var sink = new Recording();

        StartupFacts.Write(
            sink, Settings(), new FakeInputs(), () => Outputs(),
            c => c != TelemetryCategory.Transmit);

        var snapshot = sink.Only();

        _output.WriteLine("on  : " + snapshot["telemetryCategoriesOn"]);
        _output.WriteLine("off : " + snapshot["telemetryCategoriesOff"]);

        Assert.Contains(
            "Transmit", snapshot["telemetryCategoriesOff"]?.ToString() ?? "",
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "Transmit", snapshot["telemetryCategoriesOn"]?.ToString() ?? "",
            StringComparison.Ordinal);
    }

    /// <summary>**Nothing personal reaches the snapshot.**</summary>
    /// <remarks>
    /// **§2.1, AND IT IS STRUCTURAL RATHER THAN REMEMBERED**: `StartupFacts` never
    /// reads the operator's profile at all, so there is nothing to leak. The settings
    /// handed in below carry a full profile and none of it can appear.
    /// </remarks>
    [Fact]
    public void NothingPersonalReachesTheSnapshot()
    {
        var settings = Settings();

        settings.Operator.Callsign = "KC3QIS";
        settings.Operator.OperatorName = "Timothy Dixon";
        settings.Operator.GridSquare = "FN00DJ";
        settings.Operator.Location = "Trafford, PA";

        var sink = new Recording();

        StartupFacts.Write(
            sink, settings, new FakeInputs(), () => Outputs(), _ => true);

        var everything = string.Join(" ", sink.Only().Select(kv => kv.Key + "=" + kv.Value));

        _output.WriteLine(everything);

        foreach (var personal in new[]
        {
            "KC3QIS", "Timothy", "Dixon", "FN00DJ", "Trafford",
        })
        {
            Assert.DoesNotContain(
                personal, everything, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>**It is written even when the gathering itself falls over.**</summary>
    /// <remarks>
    /// **A SNAPSHOT THAT FAILS TO WRITE ON A BROKEN MACHINE IS WORTHLESS**, and the
    /// broken machine is the one somebody needs described. Handing in settings that
    /// throw on every read is the harshest case available.
    /// </remarks>
    [Fact]
    public void ItIsWrittenEvenWhenTheGatheringFallsOver()
    {
        var sink = new Recording();

        StartupFacts.Write(
            sink,
            Settings(),
            new ThrowingInputs(),
            () => throw new InvalidOperationException("no audio subsystem at all"),
            _ => throw new InvalidOperationException("settings unreadable"));

        var snapshot = sink.Only();

        _output.WriteLine("still wrote " + snapshot.Count + " fields");
        _output.WriteLine("audioOutputs    : " + snapshot["audioOutputs"]);
        _output.WriteLine("audioOutputsWhy : " + snapshot["audioOutputsWhy"]);

        Assert.True(
            snapshot.Count > 0,
            "nothing was written at all for a machine that could not describe itself");

        Assert.Equal(StartupSnapshot.Unknown, snapshot["audioOutputs"]);
    }

    /// <summary>**One stable event name, so a reader has one line to find.**</summary>
    [Fact]
    public void ThereIsOneStableEventName()
    {
        var sink = new Recording();

        StartupFacts.Write(
            sink, Settings(), new FakeInputs(), () => Outputs(), _ => true);

        _output.WriteLine("events written: " + sink.Events.Count);
        _output.WriteLine("name          : " + sink.Events[0].Name);
        _output.WriteLine("category      : " + sink.Events[0].Category);

        Assert.Single(sink.Events);
        Assert.Equal(StartupSnapshot.EventName, sink.Events[0].Name);
        Assert.Equal(TelemetryCategory.Diagnostics, sink.Events[0].Category);
    }

    /// <summary>**A device that has gone is a state change in the file.**</summary>
    /// <remarks>
    /// **THE SNAPSHOT IS A PICTURE OF STARTUP AND TODAY'S FAULTS HAPPENED LATER**
    /// (task 3). The transmit device did not go missing while Hamlet was starting; it
    /// went missing across a reboot, so the file has to be able to say when a fact
    /// stopped being what the snapshot said.
    /// </remarks>
    [Fact]
    public void ADeviceThatHasGoneIsAStateChange()
    {
        var settings = Settings();

        settings.AudioOutputDeviceId = "{0.0.0.00000000}.{a-device-that-is-gone}";

        // **THROUGH A REAL SINK, WHICH IS THE STRONGER MEASUREMENT.** Settings holds
        // a `JsonlTelemetry` because it genuinely needs `ClearAll` and `TotalBytes`,
        // so this reads the lines it actually wrote to disk rather than a stand-in.
        var lines = WhatSettingsWrote(settings);

        foreach (var line in lines)
        {
            _output.WriteLine("  " + line);
        }

        Assert.Contains(
            lines,
            l => l.Contains("state_changed", StringComparison.Ordinal)
                 && l.Contains("transmitDevicePresent", StringComparison.Ordinal));

        // **A LOSS IS A WARNING**, because a send will refuse and he will press CQ
        // and see nothing happen.
        Assert.Contains(
            lines,
            l => l.Contains("transmitDevicePresent", StringComparison.Ordinal)
                 && l.Contains("\"level\":\"warn\"", StringComparison.Ordinal));
    }

    /// <summary>**A device that is present raises no alarm.**</summary>
    [Fact]
    public void ADeviceThatIsPresentRaisesNoAlarm()
    {
        var settings = Settings();

        settings.AudioOutputDeviceId = Outputs()[1].Id;

        var lines = WhatSettingsWrote(settings);

        var warnings = lines
            .Where(l => l.Contains("\"level\":\"warn\"", StringComparison.Ordinal))
            .ToList();

        _output.WriteLine("lines written   : " + lines.Count);
        _output.WriteLine("warnings written: " + warnings.Count);

        Assert.Empty(warnings);
    }

    /// <summary>Build the Settings screen over a real sink and read what it wrote.</summary>
    private static IReadOnlyList<string> WhatSettingsWrote(AppSettings settings)
    {
        var folder = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            "hamlet-unit304-" + Guid.NewGuid().ToString("N"));

        System.IO.Directory.CreateDirectory(folder);

        try
        {
            using (var telemetry = new JsonlTelemetry(folder, "1.12.267", _ => true))
            {
                _ = new Hamlet.App.ViewModels.SettingsViewModel(
                    settings, telemetry, new FakeInputs(), () => Outputs());
            }

            return System.IO.Directory.GetFiles(folder, "*.jsonl")
                .SelectMany(System.IO.File.ReadAllLines)
                .ToList();
        }
        finally
        {
            try
            {
                System.IO.Directory.Delete(folder, recursive: true);
            }
            catch (System.IO.IOException)
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

    private sealed class ThrowingInputs : IAudioDevices
    {
        public IReadOnlyList<AudioDevice> List()
            => throw new InvalidOperationException("the sound card driver fell over");
    }

    /// <summary>A sink that remembers what was written.</summary>
    private sealed class Recording : ITelemetry
    {
        public List<(TelemetryCategory Category, string Name,
            IReadOnlyDictionary<string, object?> Data, TelemetryLevel Level)> Events
        { get; } = new();

        /// <inheritdoc/>
        public long DroppedEventCount => 0;

        /// <summary>The one snapshot that was written.</summary>
        public IReadOnlyDictionary<string, object?> Only()
        {
            Assert.NotEmpty(Events);

            return Events[0].Data;
        }

        /// <inheritdoc/>
        public void Write(
            TelemetryCategory category,
            string eventName,
            IReadOnlyDictionary<string, object?>? data = null,
            TelemetryLevel level = TelemetryLevel.Info)
            => Events.Add((
                category, eventName,
                data ?? new Dictionary<string, object?>(), level));
    }
}
