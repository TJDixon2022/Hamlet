# Unit 263 - what the operator's stop reaches, and what it does not

**Every measurement below was taken on the development machine**, from the
working tree at `HEAD 5186820`, by reading source and running `grep`. **No radio
has ever been attached to this machine** (`SHACK_FACTS.md` FACT-004), no serial
port was opened, nothing was keyed and no audio endpoint was played into for this
document. Where a claim comes from Microsoft's documentation rather than from
this tree it is marked as such, and task 5 is the only place anything is measured
against a real sound card.

**No product code was written for this task.**

---

## Q1. What token does the application hand to `AtBoundaryAsync`, and where does it go?

**It hands over no token at all**, so the whole chain runs on
`CancellationToken.None`.

The call site, `src/Hamlet.App/ViewModels/MainWindowViewModel.cs:8258`:

```csharp
var result = await _armedSend.AtBoundaryAsync(boundaryUtc).ConfigureAwait(false);
```

There is one argument and the parameter list takes two. The second is
`CancellationToken cancellationToken = default`
(`Ft8ArmedSend.cs:265-266`), and `default(CancellationToken)` is
`CancellationToken.None` - a token whose `CanBeCanceled` is false and which no
source can ever cancel.

The method that contains that line is itself token-free:

```csharp
internal async Task<Ft8BoundaryResult?> AtSlotBoundaryAsync(DateTime boundaryUtc)
```

`MainWindowViewModel.cs:8250`. Its one caller is
`_ = AtSlotBoundaryAsync(boundary);` at `MainWindowViewModel.cs:8237`, fired from
the slot clock and not awaited.

### Every hop, from the operator's thumb to the sound card

| # | Where | The line | What it does to the token |
|---|---|---|---|
| 0 | `MainWindowViewModel.cs:8237` | `_ = AtSlotBoundaryAsync(boundary);` | none exists yet |
| 1 | `MainWindowViewModel.cs:8250` | `internal async Task<Ft8BoundaryResult?> AtSlotBoundaryAsync(DateTime boundaryUtc)` | **takes no token parameter** |
| 2 | `MainWindowViewModel.cs:8258` | `await _armedSend.AtBoundaryAsync(boundaryUtc)` | **supplies none - the default binds `CancellationToken.None`** |
| 3 | `Ft8ArmedSend.cs:265-266` | `AtBoundaryAsync(DateTime boundaryUtc, CancellationToken cancellationToken = default)` | receives `None` |
| 4 | `Ft8ArmedSend.cs:297` | `await _sequence.RunAsync(send, cancellationToken)` | passes it straight through |
| 5 | `Ft8TransmitSequence.cs:249-250` | `RunAsync(OperatorSend send, CancellationToken cancellationToken = default)` | receives it |
| 6 | `Ft8TransmitSequence.cs:283` | `await _port.WriteAsync(Frame(CivConstants.PttOn), cancellationToken)` | passes it to the keying write |
| 7 | `Ft8TransmitSequence.cs:287-289` | `await _sink.PlayAsync(samples, send.Transmission.SampleRate, cancellationToken)` | passes it straight through |
| 8 | `WasapiTransmitSink.cs:298-300` | `PlayAsync(ReadOnlyMemory<float> samples, int sampleRate, CancellationToken cancellationToken)` | polls it - see Q2 |

**The rope is strung the whole way and nothing is tied to the far end.** Hops 3
through 8 all forward the token correctly; hop 2 is where it dies. Every
`IsCancellationRequested` read at hop 8 is reading a token that is structurally
incapable of ever being true.

### Two line numbers where the instruction and the tree disagree

Work instruction 263's table cites `Ft8TransmitSequence.cs:288` and
`Ft8ArmedSend.cs:296` for hops 7 and 4. The tree has the `PlayAsync` call
spanning `:287-289` with the token argument on `:288`, which agrees; and the
`RunAsync` call on **`:297`**, one line later than stated. The instruction also
cites `StopNow` at `Ft8ArmedSend.cs:235-251`; in the tree it is **`:235-247`**.
The tree wins on both; neither changes anything about the trace.

---

## Q2. Does the sink poll the token or register a callback on it?

**It polls. There is not one registration anywhere on the transmit path.**

The write loop, `WasapiTransmitSink.cs:339`:

```csharp
while (written < total && !cancellationToken.IsCancellationRequested)
```

The drain loop, `WasapiTransmitSink.cs:372-374`:

```csharp
while (!cancellationToken.IsCancellationRequested
    && _client.CurrentPadding > 0
    && deadline.Elapsed < bound)
```

The `finally`, `WasapiTransmitSink.cs:379-403` - the endpoint is stopped and
flushed on every way out of the block, cancellation included:

```csharp
finally
{
    var stranded = 0;
    ...
    written = Math.Max(0, written - stranded);

    lock (_gate)
    {
        _client.Stop();
        _client.Reset();
    }

    clock.Stop();
}
```

Note also `WasapiTransmitSink.cs:348` and `:376`: both waits inside the loops are
`Task.Delay(WaitMilliseconds, CancellationToken.None)`. **Cancellation ends the
loop; it never throws out of `PlayAsync`.** The method's contract is to say how
much went out, and a throw cannot.

### The grep

```
$ grep -rn "Register(" src/Hamlet.RadioEngine/Audio/WasapiTransmitSink.cs \
    src/Hamlet.RadioEngine/Transmit/
(no output)
```

**Count: zero.** That covers `WasapiTransmitSink.cs`, `Ft8ArmedSend.cs`,
`Ft8Composer.cs`, `Ft8TransmitSequence.cs`, `ITransmitAudioSink.cs` and every
other file in `Transmit/`. Not one `Register(`, `UnsafeRegister(` or
`CancellationTokenRegistration` on the whole path a transmission takes.

---

## Q3. What survives `Stop()` and `Reset()`, and how much audio can still leave the card?

### What the two calls do

`_client` is NAudio's `AudioClient` (`WasapiTransmitSink.cs:4`,
`using NAudio.CoreAudioApi;`), a thin wrapper over Windows Core Audio's
`IAudioClient`. **Microsoft's documentation** - not a measurement from this tree -
says `IAudioClient::Stop` halts the stream without discarding what is queued, and
`IAudioClient::Reset` "resets the audio stream", flushing all pending data and
setting the stream position back to zero. So the pair is *halt, then discard*:
samples handed to the endpoint but not yet played are **dropped, not played out**.

**This was documentation when this section was first written, and task 5 then
measured it.** Work instruction 263 named it as one of the two things the
instruction expected to be told it got wrong, and said to measure it rather than
reason about it.

### Measured, task 5, on the development machine

`TheStopStopsARealEndpointTests`, a full FT8 slot into a real render endpoint
through the real `WasapiTransmitSink`, stopped a third of the way in. **No serial
port was opened and nothing was keyed; the transport is `FakeSerialPort`.**

```
endpoint        : S34J55x (3- HD Audio Driver for Display Audio)  [development machine]
endpoint format : 48000 Hz, 2 ch, Extensible 32-bit float          [development machine]
buffer          : 9600 frames (200 ms)                             [development machine]
the slot        : 606720 samples, 12.64 s
stop pressed at : 4.228 s in
StopNow took    : 1.3 ms (bound 250 ms)
samples played  : 203040 of 606720, short by 403680
audio that went : 4230 ms of 12640 ms
CARD WENT QUIET : 20 ms after the stop                             [development machine]
sink says took  : 4241 ms
audio vs wall   : -11 ms
```

**`Reset()` discards; it does not play the buffer out.** That is what the last
line settles. The endpoint granted a 9600-frame buffer, which is exactly the 200
ms the arithmetic below predicts, so a buffer played out at the cancel would show
wall time exceeding audio time by about 200 ms. It exceeds it by **11 ms** -
start latency and one poll. The samples inside the endpoint at the moment of the
cancel went nowhere.

**And the card went quiet 20 ms after the stop**, measured as `PlayAsync`
returning, which happens after its `finally` has called `_client.Stop()` and
`_client.Reset()`. **Nothing here listened to the room** - there is no microphone
in this measurement and it does not claim one.

**FACT-004 applies to every line of it.** This is one display-audio endpoint on
the development machine. **No radio has ever been attached to this machine**, and
none of these numbers says anything about the IC-7300's USB codec.

### The arithmetic

Three quantities, all from this tree:

- `DefaultBufferMilliseconds = 200` (`WasapiTransmitSink.cs:87`) - what the
  endpoint is asked for.
- `BufferFrames = _client.BufferSize` (`WasapiTransmitSink.cs:166`) - what it
  actually granted, which the endpoint may round up. **It is a runtime value and
  is not knowable from source**; task 5 read it as **9600 frames** on the endpoint
  above, which is the 200 ms asked for, granted exactly.
- `WaitMilliseconds = 5` (`WasapiTransmitSink.cs:90`) - the poll interval.

At 48000 Hz, a 200 ms buffer is `0.200 x 48000 = 9600 frames`.

How much can still leave the card after the token is cancelled, worst case:

```
detection latency   the loops test the token once per iteration, and an
                    iteration is either a buffer fill or Task.Delay(5).
                    The class's own remark at WasapiTransmitSink.cs:82-87
                    says the real granularity of that timer is about 15 ms.
                                                        <= ~15 ms

still inside the    CurrentPadding at the moment of the cancel, bounded by
endpoint            BufferFrames = 9600 frames at 48000 Hz
                                       = 9600 / 48000 = 200 ms
                    - and Reset() discards it, per the documentation above.

TOTAL, if Reset() discards          <= ~15 ms of audio (the detection gap)
TOTAL, if Reset() played it out     <= ~215 ms
```

**Measured: 20 ms**, on the endpoint above - the first row, as predicted, since
`Reset()` was measured to discard. The fake at real time gives 15 ms for the same
thing, and the extra five is the real endpoint's own poll and teardown.

**Both numbers are small, and the number they are being compared against is not.**
A full FT8 transmission is 12.64 s. On the tree as it stands the token cannot be
cancelled at all (Q1), so what actually leaves the card after the operator presses
stop is **everything remaining in the slot** - up to 12,640 ms, and 12.64 s x 48000
= 606,720 samples at the endpoint's rate.

`written = Math.Max(0, written - stranded);` at `WasapiTransmitSink.cs:396`
matters here too: whatever is stranded in the endpoint at the cancel is
**subtracted from `SamplesPlayed` rather than counted**, so a cancelled play
reports what the card consumed and not what it was handed. That is what makes
`SamplesPlayed` a usable measure of the fix.

---

## Q4. Do the two fakes read the token?

**Neither of them reads it. Neither takes any time. Both are more permissive than
the thing they stand for**, which is the exact shape of the fault unit 262 spent a
task learning.

### `FakeTransmitAudioSink` - `tests/Hamlet.RadioEngine.Tests/Transmit/FakeTransmitAudioSink.cs:71-95`

```csharp
public Task<PlayedAudio> PlayAsync(
    ReadOnlyMemory<float> samples, int sampleRate, CancellationToken cancellationToken)
{
    TimesCalled++;
    ...
    return Task.FromResult(new PlayedAudio(PlaysOnly ?? samples.Length, Took));
}
```

The parameter is named and never mentioned again. **Cancelled mid-play it does
nothing**, because there is no mid-play: it is synchronous, returns a completed
task, and reports `samples.Length` played. It can be told to play short via
`PlaysOnly`, but only by a test deciding so in advance - never by a cancel.

### `FakeSink` - `tests/Hamlet.App.Tests/FakeTransmitParts.cs:115-134`

```csharp
public Task<PlayedAudio> PlayAsync(
    ReadOnlyMemory<float> samples, int sampleRate, CancellationToken cancellationToken)
{
    TimesCalled++;
    ...
    return Task.FromResult(
        new PlayedAudio(samples.Length, TimeSpan.FromSeconds(12.64)));
}
```

**Worse than its engine-side neighbour, which is what unit 262 recorded.** It has
no `PlaysOnly` at all, so it cannot report a short play by any route: it always
claims the full `samples.Length` went out in a hard-coded 12.64 s. Cancelled
mid-play it does nothing, for the same reason - there is no mid.

### There is a third, and it is the one that matters most here

`ParkingSink`, a private class inside
`tests/Hamlet.RadioEngine.Tests/Transmit/TheOperatorsStopFiresFromEveryStateTests.cs:754-791`.
**This is the fake the mid-transmission stop tests actually run against**, and it
is the one that holds a transmission open long enough for a stop to land in the
middle of it:

```csharp
return await Task.Run(
    () =>
    {
        Entered.Set();

        Assert.True(
            _release.Wait(TimeSpan.FromSeconds(30)),
            "the parked transmission was never released");

        return new PlayedAudio(count, TimeSpan.FromSeconds(12.64));
    },
    CancellationToken.None).ConfigureAwait(false);
```

It ignores `cancellationToken` and passes `CancellationToken.None` to `Task.Run`.
**It waits for the test's own `Release()` and for nothing else, and then reports
the full `count` played.** So every existing mid-transmission stop test is proved
against a sink that cannot stop, which is why the current green says nothing about
the audio (Q7).

### Which fake does each test get?

| Test type | Sink it gets | Where |
|---|---|---|
| `TheOperatorsStopFiresFromEveryStateTests`, the non-transmitting states | `FakeTransmitAudioSink` | `Armed()` helper, `:615-621` |
| `TheOperatorsStopFiresFromEveryStateTests`, the mid-transmission states | **`ParkingSink`** | built inline, `:226` and `:283` |
| `Hamlet.App.Tests` - the whole application send path | **`FakeSink`** | `FakeTransmitParts.cs` |

**Unit 262's finding stands and is confirmed**: the application's send path runs
against `FakeSink`, the worse of the two, and `FakeSink` is where a defect in what
the operator's click actually causes would hide.

---

## Q5. Does threading a cancellation source into `RunAsync` run WASAPI registrations on the UI thread?

**No. Unit 261's stated reason for not doing this work does not hold, and the
evidence is Q2's grep.**

Unit 261 recorded, in its own `PHASE_OUTCOME.md` entry:

> *"Threading a cancellation source from `Ft8ArmedSend` into `RunAsync` would run
> WASAPI's registrations synchronously on the operator's UI thread, which is the
> one property the abort may not have, so it is reported as a finding rather than
> repaired here."*

The concern is a real one about `CancellationTokenSource.Cancel()`: it runs every
callback registered on the source **synchronously, on the thread that called
`Cancel()`**. If `WasapiTransmitSink` registered a callback that stopped the
endpoint, `Cancel()` from a UI click would run a COM call to a sound card on the
UI thread, and the abort would inherit whatever that took.

**But there is no such registration.** `grep -rn "Register("` over
`WasapiTransmitSink.cs` and the whole of `src/Hamlet.RadioEngine/Transmit/`
returns nothing (Q2). The sink stops itself by *reading a flag at the top of a
loop it is already in*, on its own thread - the loops at `:339` and `:372` - and
by the `finally` at `:379`, which also runs on the sink's thread and not the
canceller's. **The endpoint is never touched by the thread that cancels.**

**This is a correction to the phase's memory and it belongs in this unit's report,
not in an edit to unit 261's entry.** Unit 258's ruling stands: a unit does not
rewrite an entry that is not its own. Unit 261's finding was correct as a general
worry about cancellation sources and wrong about this particular path, and the
tree is what settles it.

---

## Q6. What does `Cancel()` do with nothing registered on it, and what can it throw?

**With no registrations it sets a flag and returns**, which is the whole of the
work. `CancellationTokenSource.Cancel()` transitions the source to the cancelled
state and then invokes each registered callback in turn on the calling thread;
with zero callbacks there is nothing to invoke and the method's cost is the state
transition. **Source: Microsoft's documented behaviour for
`CancellationTokenSource.Cancel`**, read as documentation and not measured from
this tree.

What it can throw, and what this unit relies on:

| Throw | When | What task 3 does about it |
|---|---|---|
| `ObjectDisposedException` | the source has already been disposed | **the source is never disposed on the stop path**, and the cancel is wrapped anyway |
| `AggregateException` | one or more registered callbacks threw | **cannot arise - there are no callbacks** (Q2, Q5), and the cancel is wrapped anyway |

**What this unit relies on: that `Cancel()` cannot block.** That follows from the
registration count being zero and from nothing else, so if a registration ever
appears on this path the design must be revisited - and that is what makes Q2's
grep worth keeping as an assertion rather than a note.

**But "cannot block" is not "cannot throw", and task 3 does not rely on the
second.** Both rows above are exceptions, and step 1's criterion is that the abort
cannot be made conditional on anything. **So the cancel goes inside its own `try`
and the abort's frames go afterwards regardless**, and task 3 asserts that by
making the cancel path fail and reading the wire - it does not argue it.

---

## Q7. The stop tests, and which of them would still pass if the audio never stopped

Twelve tests in `TheOperatorsStopFiresFromEveryStateTests`:

| # | Test | Line | Would it still pass with the audio running on? |
|---|---|---|---|
| 1 | `NothingArmedAndNoRadioIsNothingToStop` | 70 | **yes** - nothing is playing; asserts `sink.WasNeverTouched` |
| 2 | `NothingArmedWithARadioStillTellsTheRadio` | 109 | **yes** - asserts the wire only |
| 3 | `ArmedBeforeTheBoundaryIsUnarmedAndTheBoundaryFindsNothing` | 137 | **yes** - the sink is never reached at all |
| 4 | `AboutToKeyTheAbortStillFires` | 178 | **yes** - asserts the wire only |
| 5 | `KeyedMidTransmissionTheAbortFiresWhileItIsStillRunning` | 223 | **yes** - and this is the one that hurts; see below |
| 6 | `StopNowUnarmsAndDoesNotAbort` | 280 | **yes** - it asserts the *defect*, and the audio is not part of it |
| 7 | `WaitingForTheUnkeyTheAbortStillFires` | 328 | **yes** - the audio is already finished by this state |
| 8 | `ADeadPortStillReturnsAndStillNeverThrows` | 369 | **yes** - about the port, not the sink |
| 9 | `ADisposedPortIsNotACrash` | 399 | **yes** - about the port, not the sink |
| 10 | `NothingOnTheStopPathWaitsForAnything` | 432 | **yes** - reflection and a source scan |
| 11 | `NoFlagCanTurnTheStopOffAndItAddsNoSecondWayToUnarm` | 486 | **yes** - a source scan |
| 12 | `TheStopAddsNoRouteToATransmission` | 529 | **yes** - a source scan |

**Twelve of twelve.** Not one assertion in the file reads `SamplesPlayed`,
`PlayedAudio`, or anything else about what the sink did after the stop. **The
whole green of the operator's stop is compatible with Hamlet feeding a full 12.64
second transmission into an unkeyed radio**, and that is the exact measure of what
the current green does not cover.

Test 5 is the sharpest case. It asserts:

```csharp
Assert.Equal(new[] { KeyOn, CwStop, PttOff }, wireWhileRunning);
```

- the carrier is off while the transmission is still inside `PlayAsync`, which is
a real and valuable property and stays true. Then, four lines later:

```csharp
sink.Release();
var boundary = await running;
```

**The test's own next act is to let the transmission finish playing.** It is
written that way because nothing at the time could have stopped it - and the
`ParkingSink` it runs against would have ignored the stop if anything had tried.

---

## What task 1 changed

Nothing. **No product code, no test, no fake.** This document was the whole of
task 1's output, and the two line-number mismatches in Q1 are reported rather than
repaired, per `ARBITER.md` §5 and work instruction 263's *What not to do* item 2.

**Q3's measured block was added by task 5**, which is the only part of this
document not written before any code was touched. It is marked as such where it
sits.
