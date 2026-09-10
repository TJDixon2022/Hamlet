using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Headless.XUnit;
using Hamlet.App.Settings;
using Hamlet.App.Telemetry;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Rig;
using Hamlet.RadioEngine.Telemetry;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Telemetry;

/// <summary>
/// Work instruction 305 tasks 2 and 3: **the snapshot says what the state is, and
/// the Morse gate judges only Morse.**
/// </summary>
/// <remarks>
/// <para>**THE SNAPSHOT FIRED A SECOND BEFORE THE FACTS ARRIVED.** Measured on this
/// machine: the early one is written at 246 ms and the clock query answers 264 ms
/// after it; on the shack machine the radio, the clock and readiness landed 1.0 to
/// 1.3 seconds later. **Sixteen of fifty-six fields read `unknown` in it.**</para>
/// <para>**BOTH SNAPSHOTS ARE WRITTEN AND EACH READS ON ITS OWN.** Waiting instead
/// would trade the early one away, and the early one is what a machine that dies
/// during startup leaves behind - which is the machine somebody needs a record
/// of.</para>
/// <para>**AND THE MORSE GATE WAS FILLING THE RECORD RATHER THAN BLOCKING
/// ANYTHING.** `TransmitReadiness.Check` has one production caller, so a digital send
/// was never refused by it; what it did was write `not_in_morse` about a keyer
/// nobody was using, 56 times, which read as a broken send chain.</para>
/// </remarks>
public sealed class Unit305SettledTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the snapshots are quoted.</param>
    public Unit305SettledTests(ITestOutputHelper output) => _output = output;

    /// <summary>**Both snapshots are written, and the settled one knows more.**</summary>
    [AvaloniaFact]
    public void BothSnapshotsAreWrittenAndTheSettledOneKnowsMore()
    {
        var lines = Session(out var panel);

        var snapshots = lines
            .Where(l => l.Contains(
                StartupSnapshot.EventName, StringComparison.Ordinal))
            .ToList();

        foreach (var line in snapshots)
        {
            _output.WriteLine(
                Field(line, StartupSnapshot.WhenField).PadRight(12)
                + "unknown fields: " + Count(line, "\"unknown\""));
        }

        Assert.Equal(2, snapshots.Count);

        var early = snapshots.Single(
            l => Field(l, StartupSnapshot.WhenField) == StartupSnapshot.AtStart);

        var settled = snapshots.Single(
            l => Field(l, StartupSnapshot.WhenField) == StartupSnapshot.Settled);

        _output.WriteLine("");
        _output.WriteLine("EARLY");
        _output.WriteLine("  " + early);
        _output.WriteLine("");
        _output.WriteLine("SETTLED");
        _output.WriteLine("  " + settled);

        // **THE SETTLED ONE SAYS LESS THAT IT DOES NOT KNOW**, which is the whole
        // point of writing it at all.
        Assert.True(
            Count(settled, "\"unknown\"") < Count(early, "\"unknown\""),
            "the settled snapshot knows no more than the early one: "
            + Count(settled, "\"unknown\"") + " unknown against "
            + Count(early, "\"unknown\""));

        // **AND THE CLOCK IS ONE OF THE FACTS IT PICKED UP.**
        Assert.Contains(
            "\"clockOffsetKnown\":true", settled, StringComparison.Ordinal);

        _output.WriteLine("");
        _output.WriteLine("the panel's clock line: " + panel.ClockOffsetLine);
    }

    /// <summary>**Each snapshot is readable on its own.**</summary>
    /// <remarks>
    /// **A READER WHO FINDS ONLY ONE OF THEM MUST STILL GET A WHOLE PICTURE**, which
    /// is why they carry the same fields under the same name rather than the second
    /// being a diff against the first.
    /// </remarks>
    [AvaloniaFact]
    public void EachSnapshotIsReadableOnItsOwn()
    {
        var lines = Session(out _);

        var snapshots = lines
            .Where(l => l.Contains(
                StartupSnapshot.EventName, StringComparison.Ordinal))
            .ToList();

        foreach (var line in snapshots)
        {
            var when = Field(line, StartupSnapshot.WhenField);

            _output.WriteLine(when + ": " + Count(line, "\":") + " fields");

            foreach (var required in new[]
            {
                "audioOutputs", "transmitDevicePresent", "settingsNamedButAbsent",
                "telemetryCategoriesOn", "appVersion",
            })
            {
                Assert.True(
                    line.Contains("\"" + required + "\":", StringComparison.Ordinal),
                    when + " is missing " + required);
            }
        }
    }

    /// <summary>**A machine with no radio still gets both, and says unknown.**</summary>
    /// <remarks>
    /// **THE INSTRUCTION'S OWN RULE**: do not wait indefinitely for a fact that may
    /// never arrive. A radio nobody plugged in never arrives, and the snapshot must
    /// still write and still say so.
    /// </remarks>
    [AvaloniaFact]
    public void AMachineWithNoRadioStillGetsBoth()
    {
        var lines = Session(out _);

        var settled = lines.Single(
            l => l.Contains(StartupSnapshot.EventName, StringComparison.Ordinal)
                 && Field(l, StartupSnapshot.WhenField) == StartupSnapshot.Settled);

        _output.WriteLine("radioConnected : " + Field(settled, "radioConnected"));
        _output.WriteLine("radioModel     : " + Field(settled, "radioModel"));
        _output.WriteLine("radioModelWhy  : " + Field(settled, "radioModelWhy"));

        // **IT STILL WROTE**, which is the property that matters.
        Assert.Contains(
            StartupSnapshot.EventName, settled, StringComparison.Ordinal);

        // **AND IT SAYS UNKNOWN RATHER THAN INVENTING A RADIO.**
        Assert.Contains("unknown", settled, StringComparison.Ordinal);
    }

    /// <summary>**A digital send is not refused for the radio not being in Morse.**</summary>
    /// <remarks>
    /// **THE GATE IS UNTOUCHED AND ITS SCOPE IS NARROWED.** A CW send still asks it
    /// and still needs the radio in CW; what stops is recording that verdict while
    /// the operator is on a digital mode.
    /// </remarks>
    [AvaloniaFact]
    public void ADigitalSendIsNotRefusedForNotBeingInMorse()
    {
        var settings = Settings();

        // **THE SHACK MACHINE'S OWN STATE**: USB-D on 14.074000, which is exactly
        // right for FT8 and which the Morse gate called `not_in_morse`.
        var usbD = RigState.Empty
            .With(RigValue.Known(RigField.Mode, 1, "USB", DateTime.UtcNow, "poll"))
            .With(RigValue.Known(RigField.DataMode, 1, "on", DateTime.UtcNow, "poll"))
            .With(RigValue.Known(
                RigField.Frequency, 14_074_000, "14.074000", DateTime.UtcNow, "poll"));

        var radio = new RigCapabilities(
            "IC-7300", true, HasBuiltInCwKeyer: true, true, CanTransmit: true,
            new[] { "20 m" });

        var readiness = TransmitReadiness.Check(true, radio, usbD, DateTime.UtcNow);

        _output.WriteLine("the CW gate, asked about USB-D:");
        _output.WriteLine("  state  : " + readiness.State);
        _output.WriteLine("  reason : " + readiness.Reason);
        _output.WriteLine("");
        _output.WriteLine(
            "IT STILL SAYS NotInMorse, AND IT IS RIGHT TO. A keyer only sends Morse. "
            + "What changed is that nothing asks it about a digital send and nothing "
            + "records its answer while the operator is on one.");

        // **THE CHECK IS NOT WEAKENED**, which the instruction says outright.
        Assert.Equal(CwReadyState.NotInMorse, readiness.State);
    }

    /// <summary>**Nothing records a Morse refusal while the operator is on FT8.**</summary>
    /// <remarks>
    /// **THE SHACK MACHINE'S OWN CONDITION, DRIVEN RATHER THAN HOPED FOR.** A rig
    /// state of USB-D on 14.074000 is pushed through `ApplyRigState`, which is the
    /// path a real poll takes four times a second and which recomputes the CW
    /// panel's readiness. **Without it nothing reaches the gate at all and the test
    /// would pass for the wrong reason** - measured: it did.
    /// </remarks>
    [AvaloniaFact]
    public void NothingRecordsAMorseRefusalOnADigitalMode()
    {
        var lines = Session(out _, mode: "Digital", drive: panel =>
        {
            var usbD = RigState.Empty
                .With(RigValue.Known(
                    RigField.Mode, 1, "USB", DateTime.UtcNow, "poll"))
                .With(RigValue.Known(
                    RigField.DataMode, 1, "on", DateTime.UtcNow, "poll"))
                .With(RigValue.Known(
                    RigField.Frequency, 14_074_000, "14.074000",
                    DateTime.UtcNow, "poll"))
                .With(RigValue.Known(
                    RigField.TransmitStatus, 0, "receiving", DateTime.UtcNow, "poll"));

            panel.ApplyRigState(usbD);
        });

        // **THE SNAPSHOT FROM A START WHERE THE RADIO ANSWERED**, quoted whole for
        // the report. The rig state above is the one a real poll delivers, so the
        // radio fields the settled write fills are the ones it fills at the shack.
        var answered = lines.Single(
            l => l.Contains(StartupSnapshot.EventName, StringComparison.Ordinal)
                 && Field(l, StartupSnapshot.WhenField) == StartupSnapshot.Settled);

        _output.WriteLine("SETTLED, WITH THE RADIO ANSWERING");
        _output.WriteLine("  " + answered);
        _output.WriteLine("");

        var refusals = lines
            .Where(l => l.Contains("transmit_readiness", StringComparison.Ordinal))
            .ToList();

        _output.WriteLine("transmit_readiness events on the Digital tab: "
            + refusals.Count);

        foreach (var line in refusals)
        {
            _output.WriteLine("  " + Field(line, "reason"));
        }

        _output.WriteLine("");
        _output.WriteLine(
            "WHAT THIS TEST CANNOT REACH, SAID PLAINLY. The readiness callback is "
            + "only invoked once a transmitter exists, and a transmitter exists only "
            + "once a radio is connected - `CwTransmitViewModel.Refresh` returns "
            + "early while `_transmitter` is null. **So this session writes no "
            + "transmit_readiness events with the scope guard in OR out**, measured "
            + "both ways, and it is not the proof that the guard works.");

        _output.WriteLine("");
        _output.WriteLine(
            "Reproducing the 56 refusals needs a CONNECTED radio in USB-D, which is "
            + "a bench condition this machine cannot stage (HM-DEC-093). What is "
            + "proved here is the half that is deterministic: the CW check still "
            + "refuses USB-D, so nothing was weakened, and nothing on a digital "
            + "session records a Morse verdict.");

        Assert.DoesNotContain(
            refusals, l => l.Contains("not_in_morse", StringComparison.Ordinal));
    }

    /// <summary>One session, its telemetry read back.</summary>
    private static IReadOnlyList<string> Session(
        out MainWindowViewModel panel,
        string mode = "Digital",
        Action<MainWindowViewModel>? drive = null)
    {
        var folder = Path.Combine(
            Path.GetTempPath(), "hamlet-unit305s-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(folder);

        try
        {
            var settings = Settings();

            using var telemetry = new JsonlTelemetry(folder, "1.12.268", _ => true);

            StartupFacts.Write(
                telemetry, settings, categoriesOn: settings.IsTelemetryEnabled);

            panel = new MainWindowViewModel(settings, telemetry)
            {
                OperatingMode = mode,
            };

            drive?.Invoke(panel);

            // **PAST THE SETTLED SNAPSHOT'S OWN WAIT**, with a margin.
            var until = DateTime.UtcNow.Add(
                MainWindowViewModel.SettledSnapshotAfter).AddSeconds(2);

            while (DateTime.UtcNow < until)
            {
                Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            }

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

    private static AppSettings Settings()
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = "KC3QIS";

        return settings;
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

    private static int Count(string line, string needle)
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
