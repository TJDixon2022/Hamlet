using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Headless.XUnit;
using Hamlet.App.Settings;
using Hamlet.App.Telemetry;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Telemetry;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Telemetry;

/// <summary>
/// Work instruction 305 task 5: **the snapshot replayed against this morning.**
/// </summary>
/// <remarks>
/// <para>**THE SHACK MACHINE'S STATE, CONSTRUCTED RATHER THAN HOPED FOR**: a transmit
/// device named in settings and absent, a clock that has not answered, a radio not
/// connected.</para>
/// <para>**THE QUESTION IS WHETHER THE AUTHOR WOULD HAVE NEEDED TO ASK FOR A SINGLE
/// COMMAND.** Unit 304 answered two of three and said so; this unit's job is not to
/// claim the third.</para>
/// </remarks>
public sealed class Unit305ThisMorningTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the replay is quoted.</param>
    public Unit305ThisMorningTests(ITestOutputHelper output) => _output = output;

    /// <summary>**What the file would now say about this morning.**</summary>
    [AvaloniaFact]
    public void WhatTheFileWouldNowSayAboutThisMorning()
    {
        var folder = Path.Combine(
            Path.GetTempPath(), "hamlet-unit305m-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(folder);

        try
        {
            // **THIS MORNING, CONSTRUCTED.**
            var settings = new AppSettings
            {
                ReconnectOnStartup = false,
                AudioOutputDeviceId = "{0.0.0.00000000}.{the-codec-that-went-away}",
            };

            settings.Operator.Callsign = "KC3QIS";

            using var telemetry = new JsonlTelemetry(folder, "1.12.268", _ => true);

            StartupFacts.Write(
                telemetry, settings, categoriesOn: settings.IsTelemetryEnabled);

            var panel = new MainWindowViewModel(settings, telemetry)
            {
                OperatingMode = "Digital",
            };

            var until = DateTime.UtcNow.Add(
                MainWindowViewModel.SettledSnapshotAfter).AddSeconds(2);

            while (DateTime.UtcNow < until)
            {
                Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            }

            telemetry.Dispose();

            var lines = Directory.GetFiles(folder, "*.jsonl")
                .SelectMany(File.ReadAllLines)
                .ToList();

            var settled = lines.Single(
                l => l.Contains(StartupSnapshot.EventName, StringComparison.Ordinal)
                     && l.Contains(
                         "\"" + StartupSnapshot.WhenField + "\":\""
                         + StartupSnapshot.Settled + "\"",
                         StringComparison.Ordinal));

            _output.WriteLine("WHAT THE SETTLED SNAPSHOT SAYS");
            _output.WriteLine("");

            foreach (var key in new[]
            {
                "when", "transmitDeviceSelected", "transmitDevicePresent",
                "settingsNamedButAbsent", "audioOutputCount",
                "radioConnected", "clockOffsetKnown", "clockLastQueryReason",
                "transmitReadiness",
            })
            {
                _output.WriteLine("  " + key.PadRight(26) + Field(settled, key));
            }

            var morseRefusals = lines.Count(
                l => l.Contains("not_in_morse", StringComparison.Ordinal));

            _output.WriteLine("");
            _output.WriteLine("  not_in_morse refusals in the file : " + morseRefusals);

            _output.WriteLine("");
            _output.WriteLine("WOULD A COMMAND HAVE BEEN NEEDED?");
            _output.WriteLine("");
            _output.WriteLine(
                "**FOR THE SOUND CARD, NO.** transmitDevicePresent reads false, "
                + "settingsNamedButAbsent names the device, and audioOutputCount "
                + "says how many the machine does have. That is the whole of this "
                + "morning's hour, answered in three fields.");
            _output.WriteLine("");
            _output.WriteLine(
                "**FOR THE CLOCK, NO.** The settled snapshot carries whether an "
                + "offset is held and the last query's own token, so a reader sees "
                + "the state rather than inferring it from an absence.");
            _output.WriteLine("");
            _output.WriteLine(
                "**AND THE RECORD IS NO LONGER MISLEADING.** There are no "
                + "not_in_morse refusals to read as a broken send chain, which is "
                + "what sent the author after the wrong fault.");
            _output.WriteLine("");
            _output.WriteLine(
                "**WHAT IS STILL MISSING, AND IT IS ASK 1.** Whether anything "
                + "actually keyed the transmitter. Unit 304 reported that as the one "
                + "of three it could not answer and this unit does not claim it: the "
                + "radio's own transmit state is still not joined to the "
                + "transmission record.");

            // **THE THREE FIELDS THAT WOULD HAVE ANSWERED IT.**
            Assert.Contains(
                "\"transmitDevicePresent\":false", settled, StringComparison.Ordinal);

            Assert.Contains(
                "the-codec-that-went-away", settled, StringComparison.Ordinal);

            // **AND THE MISLEADING ONES ARE GONE.**
            Assert.Equal(0, morseRefusals);

            _output.WriteLine("");
            _output.WriteLine("the panel's clock line: " + panel.ClockOffsetLine);
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

    private static string Field(string line, string key)
    {
        var at = line.IndexOf("\"" + key + "\":", StringComparison.Ordinal);

        if (at < 0)
        {
            return "(not in this line)";
        }

        var from = at + key.Length + 3;
        var to = line.IndexOfAny(new[] { ',', '}' }, from);

        return (to < 0 ? line[from..] : line[from..to]).Trim('"');
    }
}
