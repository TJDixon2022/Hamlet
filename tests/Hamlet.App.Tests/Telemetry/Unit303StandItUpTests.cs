using System;
using System.IO;
using System.Linq;
using Avalonia.Headless.XUnit;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Telemetry;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Telemetry;

/// <summary>
/// Work instruction 303 task 2: **stand the application up and read its own new
/// record.**
/// </summary>
/// <remarks>
/// <para>**THIS IS THE POINT OF TASK 1.** The query was made unrecorded, so nobody
/// could tell whether it happened. Now it is recorded, so the way to find out is to
/// build the real view model with a real telemetry sink and read the file it
/// writes - which is the constructor the application runs, not a paraphrase of
/// it.</para>
/// <para>**IT REPORTS AND DOES NOT JUDGE THE NETWORK.** What it asserts is that the
/// attempt is written and that an outcome follows it, because those are Hamlet's
/// behaviour; whether a time server answered is the machine's business.</para>
/// </remarks>
public sealed class Unit303StandItUpTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the written record is printed.</param>
    public Unit303StandItUpTests(ITestOutputHelper output) => _output = output;

    /// <summary>**Build the real view model and read what it wrote.**</summary>
    [AvaloniaFact]
    public void TheRealViewModelWritesAClockRecord()
    {
        var folder = Path.Combine(
            Path.GetTempPath(),
            "hamlet-unit303-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(folder);

        try
        {
            using var telemetry = new JsonlTelemetry(
                folder, "1.12.266", _ => true);

            var settings = new AppSettings { ReconnectOnStartup = false };

            settings.Operator.Callsign = "KC3QIS";

            // **THE CONSTRUCTOR THE APPLICATION RUNS.** The clock query is started
            // from it, fire and forget.
            var panel = new MainWindowViewModel(settings, telemetry);

            // **THE QUERY IS NOT AWAITED ANYWHERE**, which is deliberate in the
            // application and is what makes this a wait rather than an assertion on
            // a returned value. Its own timeout is 3 s; five is comfortably past it
            // and nowhere near the watchdog.
            var until = DateTime.UtcNow.AddSeconds(5);

            while (DateTime.UtcNow < until)
            {
                Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            }

            telemetry.Dispose();

            var lines = Directory.GetFiles(folder, "*.jsonl")
                .SelectMany(File.ReadAllLines)
                .Where(l => l.Contains("clock_query", StringComparison.Ordinal))
                .ToList();

            _output.WriteLine("clock records written: " + lines.Count);

            foreach (var line in lines)
            {
                _output.WriteLine("  " + line);
            }

            _output.WriteLine("");
            _output.WriteLine("what the panel believes now:");
            _output.WriteLine("  ClockOffset.IsKnown = " + panel.ClockOffset.IsKnown);
            _output.WriteLine("  the line on screen  = " + panel.ClockOffsetLine);

            // **THE ATTEMPT IS ALWAYS WRITTEN**, whatever the network did. That is
            // the whole repair: an absence now means nothing was tried.
            Assert.Contains(
                lines, l => l.Contains("clock_query_started", StringComparison.Ordinal));

            // **AND AN OUTCOME FOLLOWS IT.** Before this unit there was neither.
            Assert.Contains(
                lines,
                l => l.Contains("clock_query_finished", StringComparison.Ordinal));
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
}
