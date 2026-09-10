using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Hamlet.App.Settings;
using Hamlet.App.Telemetry;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Telemetry;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Telemetry;

/// <summary>
/// Work instruction 304 task 6: **what it costs.**
/// </summary>
/// <remarks>
/// **MEASURE AND REPORT, DO NOT OPTIMISE, AND DO NOT DROP A FACT TO SAVE BYTES** (the
/// instruction). Today cost an hour and the file is measured in kilobytes. Everything
/// here prints rather than asserting a threshold: a byte count that failed a test on
/// a machine with more sound cards would be a false red about nothing.
/// </remarks>
public sealed class Unit304CostTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the figures are printed.</param>
    public Unit304CostTests(ITestOutputHelper output) => _output = output;

    /// <summary>**What the snapshot and the bundle cost.**</summary>
    [Fact]
    public void WhatTheSnapshotAndTheBundleCost()
    {
        var folder = Path.Combine(
            Path.GetTempPath(), "hamlet-unit304c-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(folder);

        try
        {
            long snapshotBytes;

            using (var telemetry = new JsonlTelemetry(folder, "1.12.267", _ => true))
            {
                StartupFacts.Write(
                    telemetry, Settings(), new FakeInputs(), () => Outputs(),
                    _ => true);
            }

            var lines = Directory.GetFiles(folder, "*.jsonl")
                .SelectMany(File.ReadAllLines)
                .ToList();

            snapshotBytes = lines
                .Where(l => l.Contains(
                    StartupSnapshot.EventName, StringComparison.Ordinal))
                .Sum(l => (long)l.Length);

            var bundle = new AboutViewModel(Settings(), null).DiagnosticsText;

            _output.WriteLine("THE SNAPSHOT");
            _output.WriteLine(
                "  one line, " + snapshotBytes + " bytes, written once per session");
            _output.WriteLine(
                "  against an ordinary day of about 2,600 events, that is "
                + (snapshotBytes / 1024.0).ToString("0.00", CultureInfo.InvariantCulture)
                + " KB added per start");

            _output.WriteLine("");
            _output.WriteLine("THE BUNDLE");
            _output.WriteLine(
                "  " + bundle.Length + " characters, "
                + bundle.Split('\n').Length + " lines, "
                + (bundle.Length / 1024.0).ToString("0.0", CultureInfo.InvariantCulture)
                + " KB");
            _output.WriteLine(
                "  it is a paste rather than a file, and a chat window takes it");

            Assert.True(snapshotBytes > 0);
        }
        finally
        {
            try
            {
                Directory.Delete(folder, recursive: true);
            }
            catch (IOException)
            {
                // Not this test's business.
            }
        }
    }

    /// <summary>**How much a busy evening grows by, against today.**</summary>
    /// <remarks>
    /// **THE NEW STATE-CHANGE EVENTS ARE THE THING TO COUNT**, and the honest answer
    /// is that they fire on change rather than on a tick - so a busy evening on a
    /// machine where nothing goes wrong writes **none of them**.
    /// </remarks>
    [Fact]
    public void HowMuchABusyEveningGrowsBy()
    {
        var folder = Path.Combine(
            Path.GetTempPath(), "hamlet-unit304c2-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(folder);

        try
        {
            var settings = Settings();

            // An evening where the machine is healthy: the device is there.
            settings.AudioOutputDeviceId = Outputs()[1].Id;

            using (var telemetry = new JsonlTelemetry(folder, "1.12.267", _ => true))
            {
                StartupFacts.Write(
                    telemetry, settings, new FakeInputs(), () => Outputs(), _ => true);

                // Settings opened a dozen times over the evening.
                for (var i = 0; i < 12; i++)
                {
                    _ = new SettingsViewModel(
                        settings, telemetry, new FakeInputs(), () => Outputs());
                }
            }

            var lines = Directory.GetFiles(folder, "*.jsonl")
                .SelectMany(File.ReadAllLines)
                .ToList();

            var changes = lines.Count(
                l => l.Contains("state_changed", StringComparison.Ordinal));

            var bytes = lines.Sum(l => (long)l.Length);

            _output.WriteLine(
                "a healthy evening, Settings opened 12 times:");
            _output.WriteLine("  total lines written : " + lines.Count);
            _output.WriteLine("  state_changed lines : " + changes);
            _output.WriteLine("  bytes               : " + bytes);
            _output.WriteLine("");
            _output.WriteLine(
                "THE STATE-CHANGE EVENTS FIRE ON CHANGE, NOT ON A TICK. On a machine "
                + "where nothing goes wrong the evening's growth is one snapshot "
                + "line and nothing else. On the machine that lost its transmit "
                + "device it would be one line per time Settings was opened, which "
                + "is exactly when somebody is looking for the answer.");

            Assert.Equal(0, changes);
        }
        finally
        {
            try
            {
                Directory.Delete(folder, recursive: true);
            }
            catch (IOException)
            {
                // Not this test's business.
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
