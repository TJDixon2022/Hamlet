# Unit 261, task 1 - the stop trace

**What this is.** Six questions answered against the tree at `HEAD ed324bf`,
every answer carrying a file and a line number, written before anything was
built. It exists so that the concurrency decision task 2 makes is recorded as
measured rather than assumed.

**Nothing here opened a port, opened a device or played a sound**
(`SHACK_FACTS.md` FACT-004). Every answer below is read off source, and where an
answer would need a radio to establish it, this file says so instead of guessing.

---

## 1. Where is the port the operator's stop would write to?

**Confirmed, all three lines, exactly as the instruction states them.**

| What | Where | Line, verbatim |
|---|---|---|
| declared | `src/Hamlet.App/ViewModels/MainWindowViewModel.cs:238` | `private ISerialPort? _rigPort;` |
| assigned | `src/Hamlet.App/ViewModels/MainWindowViewModel.cs:6566` | `_rigPort = rigPort;` |
| nulled | `src/Hamlet.App/ViewModels/MainWindowViewModel.cs:9969` | `_rigPort = null;` |

The assignment at `:6566` sits past the connect method's early returns, under the
comment *THE PORT IS KEPT BESIDE THE RADIO, AND THE SEND PATH IS BUILT FROM IT*,
and `BuildTheArmedSend(rigPort)` is the next line. The null at `:9969` is inside
the disconnect `finally`, one line above `_armedSend = null;` at `:9970` -
**the port and the armed send are cleared together, so there is no state in which
one exists without the other.**

**Is it reachable from a command on the view model while a transmission is
running? Yes, directly and with nothing in the way.** `_rigPort` is a private
field of `MainWindowViewModel`, and a `[RelayCommand]` method generated on that
same class reads a private field of its own type with no accessor, no lock and no
lookup. It is non-null for the whole span between connect and disconnect, which
includes the arm window and the 12.64 s the radio is keyed: nothing on the send
path writes the field.

**The one state to be honest about is null.** With no radio connected, or with
the training radio (which `CreateRig` at `:9984`'s remark says returns no port on
purpose), `_rigPort` is null and there is no wire to write a stop onto. That is
not an error condition to be hidden; it is task 4's *nothing to stop* answer.

---

## 2. What is running while the radio is keyed, and on which thread?

**The chain, with line numbers:**

```
OnDecodeTick            MainWindowViewModel.cs:4898   -> OnSlotTick()
OnSlotTick              MainWindowViewModel.cs:7613   -> DriveTheArmedSend()   (:7618)
DriveTheArmedSend       MainWindowViewModel.cs:8175   -> _ = AtSlotBoundaryAsync(boundary)  (:8194)
AtSlotBoundaryAsync     MainWindowViewModel.cs:8205   -> await _armedSend.AtBoundaryAsync(boundaryUtc).ConfigureAwait(false)  (:8213)
Ft8ArmedSend.AtBoundaryAsync  Ft8ArmedSend.cs:132     -> await _sequence.RunAsync(send, ct).ConfigureAwait(false)  (:164)
Ft8TransmitSequence.RunAsync  Ft8TransmitSequence.cs:249
```

**Which thread the operator's click handler runs on: the Avalonia UI thread.**
`SendMessage` at `MainWindowViewModel.cs:8110` is a `[RelayCommand]`; it is
invoked from the right-click flyout's `MenuItem` and from `DigitalSendCqButton`
in `MainWindow.axaml:3095`, both of which raise their command on the dispatcher
thread. Any new stop command will be raised the same way.

**Is `RunAsync` occupying that thread? No - and this is the answer the rest of
the unit depends on.** Three things say so, all read from source:

1. **`_decodeTimer` is a `DispatcherTimer`** - `MainWindowViewModel.cs:3272`,
   `DispatcherPriority.Background`, 250 ms. So `OnSlotTick` and
   `DriveTheArmedSend` *begin* on the UI thread.
2. **`DriveTheArmedSend` discards the task**: `_ = AtSlotBoundaryAsync(boundary);`
   at `:8194`. There is no `await` and no `.Wait()`. The tick callback returns as
   soon as the async method yields, and the dispatcher goes back to pumping.
3. **Every await on the path carries `ConfigureAwait(false)`** -
   `MainWindowViewModel.cs:8213`, `Ft8ArmedSend.cs:164`,
   `Ft8TransmitSequence.cs:284`, `:289`, `:327`. So no continuation is posted
   back to the dispatcher, and the 12.64 s of `_sink.PlayAsync` at
   `Ft8TransmitSequence.cs:287-289` resumes on a thread-pool thread.

**How this was determined:** by reading the five call sites above, not by
running the application. `RunAsync` is *entered* synchronously on the UI thread
and runs there only as far as its first genuinely asynchronous await - in
practice `_port.WriteAsync` at `:283` or `_sink.PlayAsync` at `:287`. After that
the UI thread is free, which is why the dispatcher can deliver a stop click at
all. **Task 3 watches this rather than trusting it**: the fake sink is made to
block until the test releases it, and the stop is called from the test's own
thread while `RunAsync` is parked inside `PlayAsync`.

**The corollary, which is the whole reason the button can work:** the UI thread
is not held by a transmission, so a stop control on the window is clickable
during all 12.64 seconds. Had `DriveTheArmedSend` awaited, or had any of the five
awaits captured the context, the operator's stop would have been queued behind
the transmission it was trying to stop.

---

## 3. What `Ft8TransmitSequence` does to `_port` between key and unkey

`Ft8TransmitSequence.cs:249-353`. **Three writes, and no others** - the type
names `CivConstants.PttOn` once and reaches the port from nowhere else.

| # | Line | Write | Where | Method |
|---|---|---|---|---|
| 1 | `:283-284` | `Frame(CivConstants.PttOn)` -> `1C 00 01` | **inside the `try`** | `await _port.WriteAsync(..., cancellationToken)` |
| 2 | `:326-327` | `Frame(CivConstants.PttOff)` -> `1C 00 00` | **inside the `finally`**, guarded by `if (outcome == Ft8TransmitOutcome.Sent)` at `:319` | `await _port.WriteAsync(..., CancellationToken.None)` |
| 3 | `:344` | `TransmitAbort.Fire(_port, _radioAddress, _controllerAddress)` -> `17 FF` then `1C 00 00` | **inside the `finally`**, guarded by `if (!unkeyedNormally)` at `:339` | synchronous `port.Write` twice, `TransmitAbort.cs:81-82` |

Between writes 1 and 2 the only thing touching hardware is
`_sink.PlayAsync(samples, rate, ct)` at `:287-289`. **The port is idle for the
whole 12.64 seconds of audio.**

Three details that matter to task 2:

- **Write 2 passes `CancellationToken.None`** (`:326`, with its own comment:
  *whoever cancelled wanted the transmission stopped, which is the opposite of
  wanting this write skipped*). The unkey cannot be cancelled away.
- **Write 3 is the only existing call site of `TransmitAbort.Fire` in `src/`**,
  and it is reached only when `unkeyedNormally` is false - that is, only when
  something has already gone wrong. **Confirmed by grep across `src/`: one hit,
  `Ft8TransmitSequence.cs:344`.**
- **`RunAsync` never throws.** Every exception is caught at `:303` or `:308`, and
  the `finally` swallows its own at `:330`. So a stop firing underneath it cannot
  make it propagate.

---

## 4. Two writers, one `ISerialPort` - is `Write` safe beside an in-flight `WriteAsync`?

**Not obviously. That is the finding, and it is recorded as one rather than
resolved.**

What the seam says, `ISerialPort.cs:39-54`:

```
/// <summary>Write the whole buffer to the port.</summary>
ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken);

/// <summary>Write the whole buffer, on this thread, awaiting nothing.</summary>
/// THE ABORT PATH AND NOTHING ELSE (§0.2, HM-DEC-059).
void Write(ReadOnlySpan<byte> buffer);
```

**The interface states no thread-safety contract at all.** It says what `Write`
is *for* - the abort, because "a stop that waits its turn behind the send it is
stopping is not a stop" - and says nothing about what happens if both are in
flight.

What the implementation does, `SystemSerialPort.cs:50-60`:

```
public async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken ct)
    => await _port.BaseStream.WriteAsync(buffer, ct).ConfigureAwait(false);

public void Write(ReadOnlySpan<byte> buffer)
    => _port.Write(buffer.ToArray(), 0, buffer.Length);
```

**Two different doors onto one `SerialPort`.** `WriteAsync` goes at
`BaseStream`, which is `System.IO.Ports.SerialStream`; `Write` goes at
`SerialPort.Write(byte[], int, int)`, which internally reaches the same
`SerialStream`. `System.IO.Ports` documents its instance members as **not**
thread-safe, and neither `SerialPort` nor `SerialStream` publishes a guarantee
that a synchronous write may be interleaved with an outstanding overlapped one.
So the honest answer is: **there is no documented guarantee, and this repository
has never tested it, because `SHACK_FACTS.md` FACT-004 says no radio has ever
been attached to this machine.** No measurement taken here says anything about
what a real IC-7300 driver does with two concurrent writes.

**What is nevertheless known, from question 3:** the risk window is not the whole
transmission. It is only the moment the port is actually mid-`WriteAsync`, which
is write 1 (`1C 00 01`, 6 bytes) and write 2 (`1C 00 00`, 6 bytes). For the
12.64 seconds of audio in between - by far the largest part of the window in
which an operator would reach for a stop - **there is no in-flight `WriteAsync`
at all**, and the abort has the port to itself.

**Worst case if the two do collide:** interleaved bytes on the wire, so one or
both frames are malformed and the radio answers `FA` (NG) or ignores them. That
is a frame that did not land. It is not a crash, because
`TransmitAbort.Attempt` catches everything (`TransmitAbort.cs:90-102`) and
records the failure rather than throwing, and because the sequence's `finally`
runs regardless.

**The choice this forces on task 2** is set out there and in the report: the
abort is not serialised behind any lock the running transmission could hold.

---

## 5. What the sequence does after an abort has fired underneath it

**`RunAsync` still runs its `finally`, and it still writes `1C 00 00`.**

Trace it. An external `TransmitAbort.Fire` touches only the port. It does not
touch `outcome`, `keyed`, `unkeyedNormally` or any field of the sequence - it
cannot, because `TransmitAbort` is `static`, holds no state
(`TransmitAbort.cs:57-58`), and is handed nothing but an `ISerialPort` and two
address bytes. So:

- if the sink then returns normally, `outcome` is still `Ft8TransmitOutcome.Sent`
  from `:274`, the `if` at `:319` is true, and **write 2 goes out**:
  `1C 00 00` for the second time;
- `unkeyedNormally` is then true, so the `if` at `:339` is false and **the
  sequence's own abort does not fire**;
- if instead the sink throws or returns short, `outcome` moves off `Sent`,
  write 2 is skipped, and the sequence fires its own abort at `:344` - a third
  and fourth frame, `17 FF` and `1C 00 00` again.

**Is a second unkey harmful, or merely redundant? Merely redundant. The argument
is in the bytes.**

`1C 00 00` is CI-V *transceiver control, sub-command 00 (PTT), data 00 =
receive* (IC-7300 manual p. 19-7). It is a **set to an absolute state, not a
toggle.** Sending "be in receive" to a radio already in receive leaves it in
receive and returns `FB` (OK). There is no byte in the frame whose meaning
depends on what the radio was doing when it arrived. `17 FF` is the same shape:
the stop code for a keyer message the radio is sending itself (p. 19-11), which a
radio not sending one has nothing to apply it to - **that is `TransmitAbort`'s
own stated reasoning at `TransmitAbort.cs:32-38`, that the fallback is not a
retry but the other half of the answer, and it goes out whether or not the first
one landed.** The same reasoning covers a repeat.

**So the redundant frames are harmless. But there is a real finding here, and it
is not repaired by this unit.**

> **The abort unkeys the radio. It does not stop the audio.**

`_sink.PlayAsync` at `Ft8TransmitSequence.cs:287-289` is handed
`cancellationToken`, and `AtSlotBoundaryAsync` at `MainWindowViewModel.cs:8213`
calls `AtBoundaryAsync(boundaryUtc)` with **no token**, so the default
`CancellationToken.None` reaches the sink. Nothing an operator can do cancels it.
After a stop at second 3 of a 12.64 s transmission, the PTT is off within
two frames, **and the sink keeps playing the remaining ~9.6 s of tones into a
radio that is no longer keyed.**

What that costs, and what it does not:

- **It does not put a signal on the air.** PTT is off. Audio into an unkeyed
  IC-7300's USB input modulates nothing.
- **It does cost the operator the rest of the slot.** `RunAsync` does not return
  until the sink does, and `Ft8ArmedSend` has consumed the send already
  (`Ft8ArmedSend.cs:148`), so nothing is stuck armed - but the boundary machinery
  is occupied until the audio finishes.
- **It means the stop is not instantaneous end-to-end, only instantaneous at the
  transmitter** - which is the half that matters, and the half
  `PHASE_PLAN.md` names.

**Reported, not repaired**, per work instruction 261 *What not to do* item 2:
*if task 1 question 5 shows the abort and the `finally` interact badly, report
it; do not repair it here.* Cutting the audio needs a cancellation source
threaded from `Ft8ArmedSend` through `AtBoundaryAsync` into `RunAsync`, and
`CancellationTokenSource.Cancel()` runs its registrations synchronously on the
calling thread - which on a real WASAPI sink would be the operator's UI thread.
**That is exactly the "the abort may never wait on the thing it is aborting"
property, and it deserves its own unit rather than a line in this one.**

---

## 6. What is already on screen in the FT8 send area

**The reserved Send area is the `Border` named `DigitalSendReserved`,
`src/Hamlet.App/Views/MainWindow.axaml:3081`**, `Grid.Row="1"`, `MinHeight="72"`,
beneath the waterfall. Its whole content is one vertical `StackPanel` with four
children:

| Line | Element | Binding |
|---|---|---|
| `:3095` | `Button x:Name="DigitalSendCqButton"`, content `CQ` | `Command={Binding SendCallToAnyoneCommand}`, `ToolTip.Tip={Binding CallToAnyoneText}` |
| `:3102` | `TextBlock x:Name="DigitalSendReservedLine"` | `Text={Binding DigitalSendLine}` |
| `:3111` | `TextBlock x:Name="DigitalSendLicenceText"` | `Text={Binding DigitalSendLicenceLine}`, `IsVisible={Binding HasDigitalSendLicenceLine}` |
| `:3118` | `TextBlock x:Name="DigitalSendUnsetText"` | `Text={Binding DigitalSendUnset}`, `IsVisible={Binding HasDigitalSendUnset}` |

**There is no stop, no abort and no cancel anywhere in it.** Every binding above
resolves on `MainWindowViewModel`, which is the window's `DataContext`.

**The "Stop sending" button at `MainWindow.axaml:1239-1244` is the CW path and is
not reachable from FT8. Confirmed.** It sits inside

```
1157:  <StackPanel Spacing="8" DataContext="{Binding Transmit}">
```

so its `Command="{Binding AbortCommand}"` at `:1242` resolves against
`MainWindowViewModel.Transmit`, which is a `CwTransmitViewModel`
(`src/Hamlet.App/ViewModels/CwTransmitViewModel.cs:304`), whose `Abort` is at
`:1195`. Its `IsVisible="{Binding IsSending}"` at `:1241` is
`CwTransmitViewModel.IsSending`, set true at `:1037` and false at `:1111`, both
inside the CW send. **An FT8 transmission never sets it, so the button is not
even on screen while the FT8 path has the radio keyed** - and if it were, its
command would abort a CW transmission that is not happening.

**That is the gap this unit exists to close: the operator has a stop button for
the mode that is parked, and none for the mode that keys his transmitter.**

---

## What this trace commits task 2 to

1. **The port is reachable** from a view-model command at both moments (Q1).
2. **The UI thread is free** during the 12.64 s, so a button click gets through
   (Q2).
3. **The abort takes the port to itself** for all of the audio and shares it only
   with two 6-byte frames (Q3, Q4).
4. **Concurrent safety is undocumented**, so the stop **must not** be put behind
   a lock the transmission holds; let both writes go and record what the
   sequence's `finally` did afterwards (Q4).
5. **Redundant frames are harmless**, so nothing needs suppressing (Q5).
6. **The audio does not stop, only the carrier** - reported, not repaired (Q5).
7. **There is nowhere on screen for it yet**, so task 4 adds one to
   `DigitalSendReserved` (Q6).
