using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
/// Work instruction 305 task 1: **when each snapshot fact actually becomes
/// knowable.**
/// </summary>
/// <remarks>
/// <para>**MEASURED FROM A REAL START RATHER THAN REASONED ABOUT** - the instruction's
/// own rule, and unit 284's. The shack machine's file shows the shape of the problem:
/// the snapshot at `17:23:59.156` and then the clock at `17:24:00.142`, readiness at
/// `.327` and `.455`. **The one event designed to say what state the machine is in
/// fires a second before the state exists.**</para>
/// <para>**THE OUTPUT DECIDES TASK 2**, so this measures and asserts almost nothing: a
/// figure that failed a threshold on a slower machine would be a false red about
/// nothing.</para>
/// </remarks>
public sealed class Unit305ArrivalTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the arrival times are printed.</param>
    public Unit305ArrivalTests(ITestOutputHelper output) => _output = output;

    /// <summary>**When every fact arrives, measured from a real start.**</summary>
    [AvaloniaFact]
    public void WhenEveryFactArrives()
    {
        var folder = Path.Combine(
            Path.GetTempPath(), "hamlet-unit305a-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(folder);

        var watch = Stopwatch.StartNew();

        try
        {
            var settings = new AppSettings { ReconnectOnStartup = false };

            settings.Operator.Callsign = "KC3QIS";

            using var telemetry = new JsonlTelemetry(folder, "1.12.268", _ => true);

            // The application's own order: snapshot, then the view model.
            StartupFacts.Write(
                telemetry, settings,
                categoriesOn: settings.IsTelemetryEnabled);

            var snapshotAt = watch.Elapsed;

            var panel = new MainWindowViewModel(settings, telemetry);

            var builtAt = watch.Elapsed;

            // Let whatever lands on its own, land.
            var until = DateTime.UtcNow.AddSeconds(4);

            while (DateTime.UtcNow < until)
            {
                Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            }

            telemetry.Dispose();

            var lines = Directory.GetFiles(folder, "*.jsonl")
                .SelectMany(File.ReadAllLines)
                .ToList();

            _output.WriteLine(
                "snapshot written at    : "
                + snapshotAt.TotalMilliseconds.ToString(
                    "0", CultureInfo.InvariantCulture) + " ms");
            _output.WriteLine(
                "view model built at    : "
                + builtAt.TotalMilliseconds.ToString(
                    "0", CultureInfo.InvariantCulture) + " ms");
            _output.WriteLine("");

            _output.WriteLine("EVENTS, IN THE ORDER THEY WERE WRITTEN");

            foreach (var line in lines)
            {
                var name = Between(line, "\"event\":\"", "\"");
                var stamp = Between(line, "\"ts\":\"", "\"");

                _output.WriteLine("  " + stamp + "  " + name);
            }

            _output.WriteLine("");
            _output.WriteLine("WHAT THE SNAPSHOT COULD SEE AT THE MOMENT IT FIRED");

            var snapshot = lines.FirstOrDefault(
                l => l.Contains(StartupSnapshot.EventName, StringComparison.Ordinal));

            if (snapshot is not null)
            {
                var unknown = CountOf(snapshot, "\"unknown\"");
                var fields = CountOf(snapshot, "\":");

                _output.WriteLine(
                    "  fields          : " + fields);
                _output.WriteLine(
                    "  reading unknown : " + unknown);
            }

            _output.WriteLine("");
            _output.WriteLine("the panel's own state after the wait:");
            _output.WriteLine(
                "  ClockOffset.IsKnown : " + panel.ClockOffset.IsKnown);
            _output.WriteLine(
                "  the clock line      : " + panel.ClockOffsetLine);

            // **THE ONLY ASSERTION IS THAT THE MEASUREMENT HAPPENED.** Everything
            // above is a reading, and task 2 is what acts on it.
            Assert.NotEmpty(lines);
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

    private static string Between(string line, string from, string to)
    {
        var start = line.IndexOf(from, StringComparison.Ordinal);

        if (start < 0)
        {
            return "(none)";
        }

        start += from.Length;

        var end = line.IndexOf(to, start, StringComparison.Ordinal);

        return end < 0 ? line[start..] : line[start..end];
    }

    private static int CountOf(string line, string needle)
    {
        var count = 0;
        var at = 0;

        while ((at = line.IndexOf(needle, at, StringComparison.Ordinal)) >= 0)
        {
            count++;
            at += needle.Length;
        }

        return count;
    }
}
