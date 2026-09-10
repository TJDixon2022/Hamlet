using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Transmit;
using Avalonia.Headless.XUnit;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Telemetry;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Telemetry;

/// <summary>
/// Work instruction 305 task 3: **every stage of the send path records that it was
/// entered.**
/// </summary>
/// <remarks>
/// <para>**A TRANSMISSION HE WATCHED THE RADIO MAKE PRODUCED NO RECORD AT ALL.**
/// Only outcomes were written, and this path produced none, so *the path was never
/// entered* and *the path died at its last step* left the same empty file.</para>
/// <para>**THE ASSERTION IS ABOUT THE ORDER AND THE LAST LINE.** Entry before
/// outcome means the last stage in the file is the stage it stopped at, and that is
/// the whole diagnostic value.</para>
/// <para>**NOTHING HERE TRANSMITS** (§0.2). There is no radio and no sound card, so
/// this session stops at the stage that needs one - which is exactly the case being
/// asserted.</para>
/// </remarks>
public sealed class Unit305StageTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the stages are printed.</param>
    public Unit305StageTests(ITestOutputHelper output) => _output = output;

    /// <summary>**A send that stops is readable at the stage it stopped.**</summary>
    [AvaloniaFact]
    public void ASendThatStopsIsReadableAtTheStageItStopped()
    {
        var lines = Session(panel => panel.SendCallToAnyoneCommand.Execute(null));

        var stages = Stages(lines);

        _output.WriteLine("THE STAGES THIS SEND ENTERED, IN ORDER");

        foreach (var stage in stages)
        {
            _output.WriteLine("  " + stage);
        }

        _output.WriteLine("");
        _output.WriteLine(
            "IT STOPPED AT [" + (stages.Count == 0 ? "(none)" : stages[^1])
            + "], WHICH IS WHAT A MACHINE WITH NO RADIO AND NO SOUND CARD SHOULD "
            + "DO. Before this unit the file said nothing at all here, so a send "
            + "that never started and one that died at the sound card were the "
            + "same absence.");

        // **THE PATH IS ENTERED AND THE ENTRY IS WRITTEN.**
        Assert.Contains(SendStage.Composed, stages);

        // **AND IT GOT AS FAR AS IT COULD.** With no armed send there is nothing
        // to arm, so the last thing written is the read-back check.
        Assert.Equal(SendStage.ReadBack, stages[^1]);

        // **NOTHING KEYED**, which is the readable half.
        Assert.DoesNotContain(SendStage.Keyed, stages);
    }

    /// <summary>**The stages come before the outcome, never after it.**</summary>
    [AvaloniaFact]
    public void TheStagesComeBeforeTheOutcome()
    {
        var lines = Session(panel => panel.SendCallToAnyoneCommand.Execute(null));

        var firstStage = lines.FindIndex(
            l => l.Contains(SendStage.EventName, StringComparison.Ordinal));

        var firstOutcome = lines.FindIndex(
            l => l.Contains(TransmitRecord.EventName, StringComparison.Ordinal));

        _output.WriteLine("first stage line   : " + firstStage);
        _output.WriteLine("first outcome line : "
            + (firstOutcome < 0 ? "(none written)" : firstOutcome.ToString()));

        Assert.True(firstStage >= 0, "no stage was written at all");

        // **THERE IS NO OUTCOME HERE**, because nothing reached a radio - and that
        // absence is now readable rather than indistinguishable from nothing
        // having been attempted.
        Assert.True(
            firstOutcome < 0 || firstOutcome > firstStage,
            "an outcome was written before the stage that produced it");
    }

    /// <summary>**A send that reaches a radio writes every stage of the path.**</summary>
    /// <remarks>
    /// <para>**THE PORT AND THE SOUND CARD ARE FAKES AND NOTHING IS ON THE AIR**
    /// (§0.2, `SHACK_FACTS.md` FACT-004). What is asserted is what the sequence
    /// writes down, which is the half that was missing; whether a real radio keys is
    /// not a claim this machine can make (HM-DEC-093).</para>
    /// <para>**THE ORDER IS THE ASSERTION.** Gate, key, hand over, unkey - and each
    /// written before the thing it names is attempted, so the file's last stage is
    /// where a broken chain stopped.</para>
    /// </remarks>
    [Fact]
    public async Task ASendThatReachesARadioWritesEveryStage()
    {
        var composed = Ft8Composer.ComposeSignal(
            "CQ K1ABC FN42", 48_000, Ft8Composer.DefaultBaseFrequencyHz, 0.5f);

        Assert.True(composed.Composed, composed.Explanation);

        var folder = Path.Combine(
            Path.GetTempPath(), "hamlet-unit305e-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(folder);

        try
        {
            using var telemetry = new JsonlTelemetry(folder, "1.12.269", _ => true);

            var port = new FakePort();
            var sink = new FakeSink();

            var send = new OperatorSend(
                composed.Transmission!,
                14_074_000,
                LicenseClass.General,
                true,
                new DateTime(2026, 9, 6, 23, 45, 0, DateTimeKind.Utc),
                0.5);

            var run = await new Ft8TransmitSequence(
                    port, sink, guard: null, telemetry: telemetry)
                .RunAsync(send)
                .ConfigureAwait(true);

            telemetry.Dispose();

            var lines = Directory.GetFiles(folder, "*.jsonl")
                .SelectMany(File.ReadAllLines)
                .ToList();

            var stages = Stages(lines);

            _output.WriteLine("THE STAGES A SEND THAT REACHES A RADIO ENTERS");

            foreach (var stage in stages)
            {
                _output.WriteLine("  " + stage);
            }

            _output.WriteLine("");
            _output.WriteLine("outcome : " + run.Outcome);

            Assert.Equal(
                new[]
                {
                    SendStage.GateAsked,
                    SendStage.Keyed,
                    SendStage.HandedToTheSoundCard,
                    SendStage.Unkeyed,
                },
                stages.ToArray());

            // **AND THE EXISTING RECORD GAINED THEM RATHER THAN BEING REPLACED.**
            Assert.Contains(
                lines,
                l => l.Contains(TransmitRecord.EventName, StringComparison.Ordinal)
                     && l.Contains("stagesEntered", StringComparison.Ordinal));
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

    private static List<string> Stages(IReadOnlyList<string> lines)
        => lines
            .Where(l => l.Contains(SendStage.EventName, StringComparison.Ordinal))
            .Select(Stage)
            .ToList();

    private static string Stage(string line)
    {
        const string key = "\"stage\":\"";
        var at = line.IndexOf(key, StringComparison.Ordinal);

        if (at < 0)
        {
            return "(no stage)";
        }

        var from = at + key.Length;

        return line[from..line.IndexOf('"', from)];
    }

    /// <summary>One session, one act, its telemetry read back.</summary>
    private static List<string> Session(Action<MainWindowViewModel> act)
    {
        var folder = Path.Combine(
            Path.GetTempPath(), "hamlet-unit305s-" + Guid.NewGuid().ToString("N"));

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
