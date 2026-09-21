using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Transmit;
using Hamlet.RadioEngine.Transport;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Work instruction 375 task 1 item 4: **whose second abort pair is it?** Printed, not asserted -
/// this task builds nothing, changes no source file and repairs nothing.
/// </summary>
/// <remarks>
/// <para>**THE QUESTION, AND WHY ONLY A MEASUREMENT CAN ANSWER IT.**
/// <c>TheStopIsAlwaysOnScreenTests.KeyedAtTheOpeningSizeAClickOnTheBarFiresTheAbortWhileItRuns</c>
/// reads the wire immediately after <c>Click</c> and asserts exactly
/// <c>KeyOn, CwStop, PttOff</c>. It was red in 3 of 7 runs for unit 372, 1 of 4 for unit 373, 0 of 5
/// for unit 374 and 4 of 10 here, always the same way: the abort pair is on the wire **twice**.
/// Unit 372 named two candidates and chased neither - the click's own <c>StopNow</c>, and the
/// transmit sequence's unkey as it comes off the token. Work instruction 375 section 6 item 2 forks
/// the whole unit on which of the two it is: if the **click alone** puts two pairs on the wire, that
/// is the application sending its abort twice and the unit stops; if the second pair arrives only
/// once the sequence has torn down, the test is reading too early and the repair is the test's
/// under R12.</para>
/// <para>**HOW IT IS SPLIT.** <c>Click</c> is <c>MouseMove</c>, <c>MouseDown</c>, <c>MouseUp</c> and
/// then <c>Pump</c>, and <c>Pump</c> runs five <c>Dispatcher.RunJobs</c> passes. The abort is
/// same-thread and no-await (CLAUDE.md section 0.2), so whatever the click itself writes is on the
/// wire the instant <c>MouseUp</c> returns, **before any dispatcher job has been pumped**. The
/// sequence's teardown is a continuation and cannot run until something pumps it. This trace
/// therefore reads the wire at four moments in the same run - before the click, the instant
/// <c>MouseUp</c> returns, after the click's own pump, and after <c>await running</c> - and records
/// **the stack of every write**, so the writer is named rather than inferred.</para>
/// <para>**COMPUTED, NOT SEEN** (<c>SHACK_FACTS.md</c> FACT-004). Every frame below is a byte array
/// handed to a <see cref="TracingPort"/> that opens nothing, and the audio ends at a
/// <see cref="FakeSink"/> that makes no sound. **Nothing reaches a device** and nothing is keyed;
/// none of this is evidence about the radio.</para>
/// </remarks>
public sealed class Unit375TraceTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the trace.</summary>
    /// <param name="output">Where the four reads and the writers are printed.</param>
    public Unit375TraceTests(ITestOutputHelper output) => _output = output;

    /// <summary>FT8's watering hole on 20 m, where a General may send data.</summary>
    private const long Ft8On20m = 14_074_000;

    /// <summary>The abort's first frame.</summary>
    private const string CwStop = "FE FE 94 E0 17 FF FD";

    /// <summary>The abort's second frame, and the ordinary unkey.</summary>
    private const string PttOff = "FE FE 94 E0 1C 00 00 FD";

    /// <summary>What the radio sees when it is keyed.</summary>
    private const string KeyOn = "FE FE 94 E0 1C 00 01 FD";

    /// <summary>
    /// **The wire at four moments of the same run, eight runs over, with every write's caller named.**
    /// </summary>
    /// <remarks>
    /// Eight runs because the fault is a race and one run proves nothing either way: the flake was
    /// 4 of 10 this session, so a single green pass would have said the click writes one pair when
    /// what it means is that this pass was lucky. The point of interest is the third column, the
    /// instant <c>MouseUp</c> returns.
    /// </remarks>
    [AvaloniaFact]
    public async Task WhoseSecondAbortPairIsIt()
    {
        for (var run = 1; run <= 8; run++)
        {
            var scene = Scene(1100, 780);

            try
            {
                scene.Sink.PlaysOver = TimeSpan.FromSeconds(20);
                scene.Panel.SendMessageCommand.Execute("W1ABC KC3QIS -10");

                var slot = scene.Panel.ArmedForSlotUtc!.Value;
                var running = scene.Panel.AtSlotBoundaryAsync(slot);

                Assert.True(scene.Sink.Entered.Wait(TimeSpan.FromSeconds(10)), "the sink was never reached");

                var stop = StopButton(scene.Window);
                var keyed = Frames(scene.Port);

                // The click, taken apart. Nothing is pumped between MouseUp and the read after it,
                // so that read is the click's own writes and no continuation's.
                Pump(scene.Window);

                var centre = TheTopRowTests.RectIn(stop, scene.Window).Center;

                scene.Window.MouseMove(centre);
                scene.Window.MouseDown(centre, MouseButton.Left);
                scene.Window.MouseUp(centre, MouseButton.Left);

                var atMouseUp = Frames(scene.Port);
                var writersAtMouseUp = scene.Port.Writers.ToArray();

                Pump(scene.Window);

                var whileRunning = Frames(scene.Port);

                await running;
                Pump(scene.Window);

                var afterTeardown = Frames(scene.Port);

                _output.WriteLine("RUN " + run);
                _output.WriteLine("  1 keyed, before the click      " + keyed.Length + ": " + Join(keyed));
                _output.WriteLine("  2 the instant MouseUp returns  " + atMouseUp.Length + ": " + Join(atMouseUp));
                _output.WriteLine("  3 after the click's own pump   " + whileRunning.Length + ": " + Join(whileRunning) + "   <- line 231 reads here");
                _output.WriteLine("  4 after await running          " + afterTeardown.Length + ": " + Join(afterTeardown));
                _output.WriteLine("  the click alone wrote " + (atMouseUp.Length - keyed.Length) + " frame(s); the pump added "
                    + (whileRunning.Length - atMouseUp.Length) + "; the teardown added " + (afterTeardown.Length - whileRunning.Length));
                _output.WriteLine("  line 231 would have " + (whileRunning.SequenceEqual(new[] { KeyOn, CwStop, PttOff }) ? "PASSED" : "FAILED") + " on this run");

                for (var i = 0; i < scene.Port.Writers.Count; i++)
                {
                    _output.WriteLine("  frame " + i + "  " + Frames(scene.Port)[i]
                        + (i < writersAtMouseUp.Length ? "  [by MouseUp]" : "  [after MouseUp]")
                        + "  <- " + scene.Port.Writers[i]);
                }
            }
            finally
            {
                scene.Window.Close();
            }
        }
    }

    private static string Join(string[] frames) => string.Join(" | ", frames);

    /// <summary>The window, the panel and the fakes behind them.</summary>
    private sealed record Built(MainWindow Window, MainWindowViewModel Panel, TracingPort Port, FakeSink Sink);

    /// <summary>
    /// <c>TheStopIsAlwaysOnScreenTests.Scene</c>, frame for frame, with the port swapped for one that
    /// also records who wrote.
    /// </summary>
    private static Built Scene(double width, double height)
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = "KC3QIS";
        settings.Operator.GridSquare = "FN00";
        settings.Operator.LicenseClass = LicenseClass.General;

        foreach (var name in TheTopRowTests.NetworkSources)
        {
            settings.SetSourceEnabled(name, false);
        }

        var panel = new MainWindowViewModel(settings, null)
        {
            OperatingMode = "Digital",
            DigitalDecodedExpanded = true,
        };

        var window = new MainWindow { DataContext = panel, Width = width, Height = height };

        window.Show();
        Pump(window);

        panel.SelectedBand = panel.Bands.First(b => b.Band.LowHz <= Ft8On20m && b.Band.HighHz >= Ft8On20m);
        panel.FrequencyHz = Ft8On20m;

        var port = new TracingPort();
        var sink = new FakeSink();

        panel.UseRigPortForTests(port);
        panel.UseArmedSendForTests(new Ft8ArmedSend(new Ft8TransmitSequence(port, sink)));
        Pump(window);

        return new Built(window, panel, port, sink);
    }

    /// <summary>The realized stop button.</summary>
    private static Button StopButton(Window window)
    {
        Pump(window);

        return TheTopRowTests.Named<Button>(window, "DigitalStopButton");
    }

    private static string[] Frames(TracingPort port)
        => port.Written.Select(f => string.Join(' ', f.Select(b => b.ToString("X2", CultureInfo.InvariantCulture)))).ToArray();

    /// <summary>Runs the dispatcher and lays the window out.</summary>
    private static void Pump(Window window)
    {
        for (var i = 0; i < 5; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }

    /// <summary>
    /// <see cref="FakePort"/> with a stack behind every frame, so the trace can name the writer
    /// instead of guessing between two candidates as unit 372 had to.
    /// </summary>
    /// <remarks>
    /// It is here and not in <c>FakeTransmitParts.cs</c> because it exists for one measurement in
    /// one task; <see cref="FakePort"/> is what every other test uses and is not touched.
    /// </remarks>
    private sealed class TracingPort : ISerialPort
    {
        private readonly List<byte[]> _written = [];
        private readonly List<string> _writers = [];

        /// <summary>Every frame that was written, in order.</summary>
        public IReadOnlyList<byte[]> Written => _written;

        /// <summary>The Hamlet frames of the stack that wrote each one, innermost first.</summary>
        public IReadOnlyList<string> Writers => _writers;

        /// <inheritdoc/>
        public bool IsOpen { get; private set; } = true;

        /// <inheritdoc/>
        public string PortName => "FAKE1";

        /// <inheritdoc/>
        public int BaudRate => 115_200;

        /// <inheritdoc/>
        public void Open() => IsOpen = true;

        /// <inheritdoc/>
        public void Close() => IsOpen = false;

        /// <inheritdoc/>
        public ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken)
            => ValueTask.FromResult(0);

        /// <inheritdoc/>
        public ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)
        {
            Record(buffer.ToArray());

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public void Write(ReadOnlySpan<byte> buffer) => Record(buffer.ToArray());

        /// <inheritdoc/>
        public void Dispose() => IsOpen = false;

        private void Record(byte[] frame)
        {
            _written.Add(frame);
            _writers.Add(Caller());
        }

        /// <summary>The Hamlet methods on the stack, nearest first, which is enough to name a writer.</summary>
        private static string Caller()
        {
            var names = new StackTrace(false).GetFrames()
                .Select(f => f.GetMethod())
                .Where(m => m?.DeclaringType?.FullName is not null && m.DeclaringType.FullName.StartsWith("Hamlet.", StringComparison.Ordinal))
                .Select(m => m!.DeclaringType!.Name + "." + m.Name)
                .Take(8)
                .ToArray();

            return names.Length == 0 ? "(no Hamlet frame on the stack)" : string.Join(" <- ", names);
        }
    }
}
