# Unit 268, task 1 - the chain trace

**Measured before anything was built.** Work instruction 268 task 1: six questions,
each answered with a file, a line and a quotation. Every line below was read off the
tree on 2026-09-07, at `79edaa9`, before a line of task 2 existed.

`docs/unit267-what-step-c-needs.md` did the survey of *which pieces exist*. **This is
not a repeat of it.** It is only what that document left open: where the clicked
string actually comes from, whether the two hosts can join, what the telemetry
harness costs, what the port records, what the Stop button reaches, and what figure
says *silence*.

Nothing here was run. No port was opened, nothing was keyed, no sound was made
(`SHACK_FACTS.md` FACT-004).

---

## 1. The click's own text

**The string that reaches `SendMessageCommand` is `Ft8SendOption.Text`, exactly, with
nothing added and nothing built again on the way.**

`src/Hamlet.App/Views/MainWindow.axaml.cs:182-191`, inside `SendFlyoutFor`:

```csharp
foreach (var option in menu.Options)
{
    flyout.Items.Add(new MenuItem
    {
        Header = HeaderFor(option),

        // THE ONE ENTRY POINT THAT ARMS, AND THE ONLY THING THIS DOES.
        Command = vm!.SendMessageCommand,
        CommandParameter = option.Text,
    });
}
```

**The header is decorated and the command parameter is not**, and the instruction was
right to ask which is which. `HeaderFor` at `MainWindow.axaml.cs:216`:

```csharp
var said = option.Text + "   " + option.Label;

if (option.SentBefore > 0)
{
    said += ", " + Ordinal(option.SentBefore + 1) + " time";
}

return option.IsExpected ? said + " - the one that comes next" : said;
```

So the item whose header reads
`W1ABC KC3QIS RRR   acknowledge, 2nd time - the one that comes next` carries the
`CommandParameter` `"W1ABC KC3QIS RRR"`.

**The property that is the message is `Ft8SendOption.Text`** -
`src/Hamlet.RadioEngine/Contacts/Ft8SendOptions.cs:30-31`:

```csharp
public sealed record Ft8SendOption(
    Ft8SendShape Shape, string Text, string Label, bool IsExpected, int SentBefore);
```

**What criterion 2 must compare against, and how a test gets at it without
re-deriving it:** the clicked `MenuItem`'s own `CommandParameter`, cast to `string`.
Reading `option.Text` off `vm.SendMenuFor(row)` would be a second read of the same
source; reading `CommandParameter` off the realized item is a read of **what the
markup's own handler put on the thing the mouse hits**, which is the seam this unit
exists to measure. The item is then invoked through `item.Command.Execute(item.CommandParameter)`,
which is what a click on a `MenuItem` does.

**Notes carry no command** (`MainWindow.axaml.cs:245`,
`private static MenuItem Note(string text) => new() { Header = text, IsHitTestVisible = false };`),
so the clickable options are exactly `flyout.Items.OfType<MenuItem>().Where(i => i.Command is not null)`,
which is the helper `TheMenuIsUnderTheMouseTests.cs:541` already uses.

---

## 2. The host question, on paper

**What `[AvaloniaFact]` does to the thread.** `Avalonia.Headless.XUnit` 11.3.0 -
`tests/Hamlet.App.Tests/Hamlet.App.Tests.csproj:12`,
`<PackageReference Include="Avalonia.Headless.XUnit" Version="11.3.0" />`, **confirmed,
and NAudio is named nowhere in that file** - it arrives transitively through
`..\..\src\Hamlet.App\Hamlet.App.csproj` at `:26`, which is how
`TheLoopbackThroughTheApplicationsSendPathTests.cs:8` already compiles
`using NAudio.CoreAudioApi;` in this project. **No project-file edit is needed.**

The test body runs **on the Avalonia UI thread of a headless session**, with the
dispatcher's synchronization context installed, so every `await` inside the body
resumes on that same dispatcher thread rather than on a thread-pool thread.

**What pumps it in this tree, and it is done by hand rather than by the framework** -
`tests/Hamlet.App.Tests/Views/TheMenuIsUnderTheMouseTests.cs:551-558`:

```csharp
private static void Pump(Window window)
{
    for (var i = 0; i < 5; i++)
    {
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
    }
}
```

`TheOperatorCanStopItTests.cs` carries the same helper and calls it around every
click.

**What would have to be true for a `WasapiLoopbackCapture` callback and a
12.64-second `await` to coexist with it - and all three already hold somewhere in the
tree:**

1. **The capture callback must not need the UI thread.** It does not.
   `TheLoopbackThroughTheApplicationsSendPathTests.cs:122-141` handles
   `capture.DataAvailable` on NAudio's own capture thread and touches only
   `WasapiAudioSource.Downmix` and `AudioTap.Take`, neither of which is a UI type;
   `AudioTap`'s own remark at `AudioTap.cs:96` calls the device callback "one writer"
   and the tap is a seqlock built for exactly this.
2. **A long real-time `await` must be legal inside an `[AvaloniaFact]`.** It already
   is, in this project, today: `TheOperatorCanStopItTests.cs:241`
   `AClickWhileTheToneIsPlayingStopsTheSoundAndNotJustTheCarrier` is an
   `[AvaloniaFact]` that awaits `scene.Panel.AtSlotBoundaryAsync(slot)` across a
   two-second `FakeSink { PlaysOver = TimeSpan.FromSeconds(2) }`, blocks on
   `playing.Entered.Wait(TimeSpan.FromSeconds(10))`, and loops on
   `await Task.Delay(2, CancellationToken.None)` for up to thirty seconds. **A
   multi-second wall-clock wait with another thread making progress underneath it is
   proved inside this host.** 12.64 seconds is the same shape, longer.
3. **The transmission itself must not be posted to the UI thread.** It is not:
   `Ft8ArmedSend.AtBoundaryAsync` reaches `Ft8TransmitSequence.RunAsync` and the sink
   writes on whatever thread it is on; the only UI-thread work is the Send-area line
   the panel sets afterwards, and `Pump` runs it.

**What is genuinely untried, and it is one thing:** opening a `WasapiTransmitSink` -
a real WASAPI render client - from inside the headless session's thread. COM
apartment state is the risk: the headless session's thread is not the xUnit thread
`[Fact]` runs on. **That is what task 2 measures**, and the fallback in the
instruction is written for exactly that failure.

---

## 3. The telemetry line

**What `TheWholeContactWalksThroughTheApplicationTests` does, in three parts.**

**The settings and the writer**, built the way `App.axaml.cs` builds it -
`tests/Hamlet.App.Tests/ViewModels/TheWholeContactWalksThroughTheApplicationTests.cs:486-491`:

```csharp
var telemetry = new JsonlTelemetry(
    _folder,
    "1.12.90",
    category => settings.IsTelemetryEnabled(category),
    settings.TelemetryMaxMegabytes * 1024L * 1024L);

var panel = new MainWindowViewModel(settings, telemetry);
```

**The path** - a fresh temporary folder per test instance, deleted in `Dispose` -
`:67-68`:

```csharp
private readonly string _folder = Path.Combine(
    Path.GetTempPath(), "hamlet-unit264-" + Guid.NewGuid().ToString("N")[..8]);
```

**The predicate** is `settings.IsTelemetryEnabled(category)` off a real `AppSettings`
with nothing switched on by hand, which is the whole point - unit 264 measured that
Transmit telemetry survives the application's own switches rather than assuming it.

**The read-back**, off disk and never off a double - `:449-455`:

```csharp
private List<string> TransmitLines()
    => !Directory.Exists(_folder)
        ? []
        : Directory.GetFiles(_folder, "*.jsonl")
            .OrderBy(f => f, StringComparer.Ordinal)
            .SelectMany(File.ReadAllLines)
            .Where(l => l.Contains("\"" + TransmitRecord.EventName + "\"", StringComparison.Ordinal))
            .ToList();
```

**What the chain test has to set up, and it is four lines:** a temp folder; a
`JsonlTelemetry` built with those four arguments; the panel constructed as
`new MainWindowViewModel(settings, telemetry)` rather than `(settings, null)` - which
is the one difference from `TheLoopbackThroughTheApplicationsSendPathTests.cs:291`,
where it is `null` and no line is ever written; and **`telemetry.Dispose()` before
reading**, `:274`, because `JsonlTelemetry.Write` appends on a background thread and
returns `void`. The writer reaches the sequence through
`MainWindowViewModel.cs:8067-8068`, `new Ft8TransmitSequence(port, sink, _sendLicence, _telemetry)`,
so the line is written by the application's own path and not by the test.

---

## 4. The unkey and the frames

**The property is `TransmitRun.CameOutOfTransmit`**, of type `UnkeyRoute`
(`src/Hamlet.RadioEngine/Transmit/Ft8TransmitSequence.cs:42-61`; `OrdinaryUnkey` at
`:48` is "The ordinary `1C 00 00` write, on the path where all was well").

The assertion the loopback test makes -
`TheLoopbackThroughTheApplicationsSendPathTests.cs:186-189`:

```csharp
// **THE SEND PATH RAN WHOLE.**
Assert.Equal(Ft8ArmOutcome.Ran, result.Outcome);
Assert.True(run!.Sent, run.Reason);
Assert.Equal(UnkeyRoute.OrdinaryUnkey, run.CameOutOfTransmit);
```

**What `FakePort` records: whole frames, in order, as `byte[]` - so both frames and
byte counts, the second by arithmetic on the first.**
`tests/Hamlet.App.Tests/FakeTransmitParts.cs:19-25`:

```csharp
private readonly List<byte[]> _written = [];

/// <summary>Every frame that was written, in order.</summary>
public IReadOnlyList<byte[]> Written => _written;

/// <summary>True where nothing was ever written to it.</summary>
public bool WasNeverWrittenTo => _written.Count == 0;
```

**What the send-path test asserts about them** -
`tests/Hamlet.App.Tests/ViewModels/TheSendPathReachesARealRadioTests.cs:199-201`:

```csharp
// KEYED AND UNKEYED, ON THE PORT THIS CODE KEPT.
Assert.Equal(2, port.Written.Count);
```

It counts and does not read the bytes. `TheOperatorCanStopItTests.cs:695-696` is the
one in this project that reads them - `Frames(scene)` is
`scene.Port.Written.Select(Hex).ToArray()` - and asserts
`Assert.Equal(new[] { KeyOn, CwStop, PttOff }, whileRunning)` at `:196` and `:286`. **The chain
test should assert the hex and not only the count**, because a count of two is
satisfied by two of anything.

---

## 5. The stop, from the button

**In order, from the press:**

1. The realized `Button` named `DigitalStopButton`, found on the window at
   `tests/Hamlet.App.Tests/Views/TheOperatorCanStopItTests.cs:650-652`
   (`.FirstOrDefault(b => b.Name == "DigitalStopButton")`), pressed with real
   headless pointer input at `:687-689` -
   `scene.Window.MouseMove(...)`, `MouseDown(..., MouseButton.Left)`, `MouseUp(...)`.
2. Its `Command`, which `TheOperatorCanStopItTests.cs:467` asserts is
   `Assert.Same(scene.Panel.StopSendingCommand, button.Command)`.
3. `MainWindowViewModel.StopSending`, `src/Hamlet.App/ViewModels/MainWindowViewModel.cs:8562-8567`:

```csharp
private void StopSending()
{
    var stop = _armedSend?.StopNow(_rigPort);

    DigitalSendLine = StopLine(_armedText, stop, _transmitRefusal);
}
```

4. `Ft8ArmedSend.StopNow`, `src/Hamlet.RadioEngine/Transmit/Ft8ArmedSend.cs:339-353`:

```csharp
var unarmed = Cancel();

var abort = port is null
    ? null
    : TransmitAbort.Fire(port, radioAddress, controllerAddress);

var audioToldToStop = StopTheAudio();

return new Ft8StopResult(unarmed, abort, audioToldToStop);
```

**THE TRAP TASK 4 MUST NOT WALK INTO, and it is not the one unit 267 named.** The
abort's frames go to `_rigPort`, **not** to the port handed to `BuildTheArmedSend`.
`BuildTheArmedSend` (`MainWindowViewModel.cs:7989-8086`) never assigns `_rigPort`; it
is set only by `UseRigPortForTests` at `:8646` or by the real connect at `:6567`.
`TheLoopbackThroughTheApplicationsSendPathTests` calls `BuildTheArmedSend(port)` and
nothing else, so **a Stop pressed in that harness would fire `StopNow(null)`, take no
abort at all, and criterion 3's carrier half would silently measure nothing.** The
chain test calls **both** `panel.UseRigPortForTests(port)` and
`panel.BuildTheArmedSend(port)` on the same `FakePort`.

**What the run reports afterwards.** Two different objects say two different things:

- **The run's outcome is `Ft8TransmitOutcome.Cancelled`** - asserted at
  `TheOperatorCanStopItTests.cs:297` and again on a real card at
  `tests/Hamlet.RadioEngine.Tests/Audio/TheStopStopsARealEndpointTests.cs:196`. The
  boundary's own `Ft8ArmOutcome` is still `Ran`, which is unit 267's recorded trap.
- **How much went out is `TransmitRun.Played.SamplesPlayed`**, not a field of
  `Ft8StopResult`.

**`Ft8StopResult`'s three facts** - `Ft8ArmedSend.cs:123`,
`public sealed record Ft8StopResult(bool Unarmed, AbortRecord? Abort, bool AudioToldToStop)`.
**The one that says the sound was stopped is `AudioToldToStop`**, and its own remark
at `:377` is careful about what it claims: *"A cancel that would not take reports
false rather than claiming the sound stopped."* `Abort` is the carrier half;
`Unarmed` says a send was taken off the queue before it ever keyed. The six-way
`Outcome` at `:140-148` folds them: a mid-slot press on a keyed transmission reads
`Ft8StopOutcome.StoppedTheTransmissionAndToldTheRadio`.

**None of the three says how many seconds went out.** That comes off
`run.Played.SamplesPlayed` and, in this unit, off the capture as well.

---

## 6. What silence is

**Answer: "none" for the assertion, and *nearly none* for the measurement.**

**Nothing in the tree asserts on a quiet capture.** The one place a quiet capture is
measured at all is a printed line with no assertion behind it -
`tests/Hamlet.RadioEngine.Tests/Audio/WhatThisMachineCanPlayAndCaptureTests.cs:157`:

```csharp
_output.WriteLine($"LOOPBACK PEAK    : {peak:F9} (nothing was playing, so zero is expected)");
```

and its `peak` at `:105-121` is computed by hand over the raw 32-bit float buffer
inside that test's own `DataAvailable`, not by anything reusable. Its remark at
`:148-150` is the fact criterion 4 rests on: *"A silent machine still delivers
buffers of zeroes under WASAPI loopback, which is itself the answer."* **Buffers
keep arriving while nothing plays, so a silent boundary is measurable rather than
being an absence of data.**

**What the existing capture block in the loopback test gives, and it is less than the
question hoped:**

- `tap.SamplesSeen` (`AudioTap.cs:178`) - a running sample count. **Useless for
  silence**: it counts zeroes as eagerly as tones, which is exactly the "untouched
  counter" the criterion forbids.
- `tap.Snapshot()` (`AudioTap.cs:313`) - a `MonoAudio` carrying `SampleRate` and a
  `float[]` of the last thirty seconds. **This is the raw material and it is
  enough.**
- `tap.Level` (`AudioTap.cs:160`), an `AudioLevel` with `PeakDb`, `RmsDb`, `FloorDb`.
  **Not usable here as-is**: it is measured over a rolling `LevelSeconds = 0.2`
  window (`:84`), so it says what the last fifth of a second was, not what a whole
  15-second boundary was.

**The figure this unit names, and it uses the tree's own threshold rather than
inventing one: the peak absolute sample across the boundary window, in dBFS.**
`AudioLevel` already defines the word: `AudioTap.cs:44`,

```csharp
public bool NearlySilent => PeakDb <= TooQuietDb;
```

with `TooQuietDb = -60` at `:36` - "Sixty decibels below full scale is a
sixty-fourth of a percent of the available range". So:

- **not silent** (task 2, task 3's transmission) is `peak dBFS > -60`;
- **silent** (task 3's unclicked boundary) is `peak dBFS <= -60`, with the RMS in
  dBFS printed beside it.

**What it reads on this machine when nothing is playing: not yet measured, and this
unit measures it rather than quoting it.** No committed test asserts it and
`WhatThisMachineCanPlayAndCaptureTests` is outside the two tests task 2 is permitted
to run, so **the honest answer today is that the tree does not know.** The figure is
printed by what this unit builds and reported in section 3 of `output.md`. The
prediction, written here before the run so it can be wrong: a loopback capture on an
endpoint that nothing else is rendering to returns exact zeroes, and a peak of 0.0
prints as `-inf` dBFS - so the printout floors at `AudioLevel.SilenceDb = -90`
(`AudioTap.cs:24`) rather than showing an infinity.

---

## What this trace changed about the tasks that follow

- **Task 3's expected string comes off the realized `MenuItem.CommandParameter`**, not
  off `SendMenuFor` a second time. Q1.
- **Task 2 needs no project-file edit.** NAudio is already reachable. Q2.
- **The chain test's panel is constructed with a `JsonlTelemetry`, and disposes it
  before reading disk.** Q3.
- **The frames are asserted as hex, not as a count of two.** Q4.
- **`UseRigPortForTests` is called as well as `BuildTheArmedSend`, or criterion 3's
  carrier half measures nothing.** Q5 - and this is the finding of the trace.
- **Silence is peak dBFS against `AudioLevel.TooQuietDb`, computed over the window,
  and no committed test has ever measured it.** Q6.
