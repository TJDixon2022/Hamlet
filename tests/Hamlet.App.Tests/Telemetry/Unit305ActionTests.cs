using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Headless.XUnit;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Telemetry;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Telemetry;

/// <summary>
/// Work instruction 305 task 2: **an operator action is recorded when it is taken.**
/// </summary>
/// <remarks>
/// <para>**HE PRESSED CQ AND NOTHING RECORDED THE PRESS.** Seventeen events in the
/// file from his own machine and not one of them is a person doing something, so
/// *he did not press* and *he pressed and nothing happened* were the same file and
/// the difference cost a session.</para>
/// <para>**THE ORDERING IS THE WHOLE POINT.** The press is written before anything
/// it triggers, so a press with no outcome after it reads as a press that went
/// nowhere - which is exactly the case this test constructs, because a machine with
/// no transmit device cannot send and the record has to say so.</para>
/// <para>**NOTHING HERE TRANSMITS** (§0.2). There is no sound card and no radio, and
/// what is asserted is what was written down.</para>
/// </remarks>
public sealed class Unit305ActionTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the record is printed.</param>
    public Unit305ActionTests(ITestOutputHelper output) => _output = output;

    /// <summary>**A CQ press that goes nowhere still leaves a line.**</summary>
    [AvaloniaFact]
    public void ACqPressThatGoesNowhereStillLeavesALine()
    {
        var lines = Session(panel => panel.SendCallToAnyoneCommand.Execute(null));

        Print(lines);

        var actions = Actions(lines);

        // **THE PRESS IS THERE.**
        Assert.Contains(
            actions, l => l.Contains("\"action\":\"cq_pressed\"", StringComparison.Ordinal));

        // **AND SO IS THE DOOR IT WENT THROUGH**, so a send that stopped inside the
        // send path is separable from one that never entered it.
        Assert.Contains(
            actions,
            l => l.Contains("\"action\":\"send_requested\"", StringComparison.Ordinal));

        // **AND NOTHING WENT OUT**, which is the readable absence: an action with
        // no transmission after it.
        Assert.DoesNotContain(
            lines, l => l.Contains("ft8_transmission", StringComparison.Ordinal));
    }

    /// <summary>**The stop is recorded even when there is nothing to stop.**</summary>
    /// <remarks>
    /// **THE CONTROL IS PRESSABLE AT EVERY INSTANT** (HM-DEC-158's stop), so a press
    /// of it when nothing is armed is the ordinary case and is exactly the one that
    /// used to leave no trace.
    /// </remarks>
    [AvaloniaFact]
    public void TheStopIsRecordedEvenWithNothingArmed()
    {
        var lines = Session(panel => panel.StopSendingCommand.Execute(null));

        Print(lines);

        Assert.Contains(
            Actions(lines),
            l => l.Contains("\"action\":\"stop_pressed\"", StringComparison.Ordinal)
                 && l.Contains("\"detail\":\"nothing_armed\"", StringComparison.Ordinal));
    }

    /// <summary>**No callsign reaches the record** (§2.1).</summary>
    [AvaloniaFact]
    public void NoCallsignReachesTheRecord()
    {
        var lines = Session(panel =>
        {
            panel.SendCallToAnyoneCommand.Execute(null);
            panel.ClearCardCommand.Execute("K1ABC");
            panel.StopSendingCommand.Execute(null);
        });

        foreach (var line in Actions(lines))
        {
            _output.WriteLine("  " + line);
        }

        Assert.DoesNotContain(
            lines, l => l.Contains("K1ABC", StringComparison.OrdinalIgnoreCase));

        Assert.DoesNotContain(
            lines, l => l.Contains("KC3QIS", StringComparison.OrdinalIgnoreCase));
    }

    private static IReadOnlyList<string> Actions(IReadOnlyList<string> lines)
        => lines
            .Where(l => l.Contains("operator_action", StringComparison.Ordinal))
            .ToList();

    private void Print(IReadOnlyList<string> lines)
    {
        _output.WriteLine("WHAT THE OPERATOR DID, IN THE ORDER IT WAS WRITTEN");

        foreach (var line in Actions(lines))
        {
            _output.WriteLine("  " + line);
        }

        _output.WriteLine("");
        _output.WriteLine("events in the file: " + lines.Count);
    }

    /// <summary>One session, one act, its telemetry read back.</summary>
    private static IReadOnlyList<string> Session(Action<MainWindowViewModel> act)
    {
        var folder = Path.Combine(
            Path.GetTempPath(), "hamlet-unit305a-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(folder);

        try
        {
            using var telemetry = new JsonlTelemetry(folder, "1.12.269", _ => true);

            var settings = new AppSettings { ReconnectOnStartup = false };

            settings.Operator.Callsign = "KC3QIS";

            var panel = new MainWindowViewModel(settings, telemetry)
            {
                OperatingMode = "Digital",
            };

            act(panel);

            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            telemetry.Dispose();

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
                // Not this test's business.
            }
        }
    }
}
