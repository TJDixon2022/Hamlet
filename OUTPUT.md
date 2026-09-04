READ IN THIS ORDER

A. THE PHASE GOAL. Hamlet hears FT8 off the radio and displays the decoded text
on screen. Nothing on screen is closer tonight. What is closer is that the thing
which was eating three quarters of the audio has been found, measured, and taken
out of the way.

B. THIS STEP AND ITS EXIT CRITERIA. Arrival on the shack machine was 76%, with
zero callback overruns, zero queue drops and zero callback failures, and 554 of
561 callbacks running past half their budget. Unit 238 moved the CW decode off
the device callback and arrival went 13% to 76%; unit 239 fixed the tap's readers
and correctly reported that the spectrum source does not read the tap. This unit
went after the second subscriber to the same event.

C. WHAT THIS REPORT ADDS, AND WHETHER IT BEARS ON A OR B. It bears hard on B. The
device callback was spending **62,271 microseconds** on a single 100 ms buffer,
and **99.5% of it was one line**: the waterfall's ring shifting 16,383 floats
down one place, once per sample. It is now **270 microseconds**, and with the
transform moved off the thread as well a stalled picture costs the callback
**92 microseconds** where it used to cost 522,895. **Whether that takes the shack
machine from 76% to 99% is the operator's reading to take** (`SHACK_FACTS.md`
FACT-004). Nothing below was measured on a computer with a radio attached.

---

UNIT:       240 — complete at task 4 of 4 — 2026-09-03 23:35
PHASE GOAL: Hamlet hears FT8 off the radio and displays the decoded text on screen.
UNIT GOAL:  No subscriber to SamplesReady does more than copy on the device callback thread, and the waterfall's transform work happens somewhere a slow frame cannot cost the radio audio.
ADVANCED:   **yes** — the goal task. The callback's work fell by three orders of magnitude and the picture is provably unchanged.
NUMBER:     **the device callback with a stalled picture: 522,895 µs to 92 µs**; `Push` alone 62,271 µs to 270 µs; 43 pinned frames byte-identical.
DRIFT:      0 consecutive units without advance.

## 1. What Claude did

**Complete. Four tasks of four.** Task 4 was the named drop candidate and was
taken rather than dropped, because tasks 2 and 3 left room.

Hamlet confirmed against all four gate checks before the instruction was read:
`SHACK_FACTS.md` and `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` both
present, `CoreHMI.sln` and `MURC.sln` both absent.

Development machine, branch **`main`**, four commits, **every push succeeded**.
Root version **1.12.43 to 1.12.44** per HM-DEC-150; `Ft8Sharp` did not move and
nothing in `src/Ft8Sharp/` was touched.

**Nothing in this report is evidence about the radio** (FACT-004).

### Task 1 — the measurement, before anything was built

**Two subscribers to `IAudioSource.SamplesReady`, and no third.**

| Subscriber | Where | What it does synchronously |
|---|---|---|
| `AudioSpectrumSource.OnSamples` | `AudioSpectrumSource.cs:225` | `Push(chunk.Samples)`, in full |
| `CwDecoder.OnSamples` | `CwDecoder.cs:797` | enqueues onto `AudioHandoff` and returns |

`MainWindowViewModel` holds an `AudioSpectrumSource` but does not subscribe to
the event itself.

**One 100 ms buffer of 4,800 samples, ring full, `FrameReady` attached:**

| | Median | Worst |
|---|---|---|
| `AudioSpectrumSource.Push` | **62,271 µs** | 65,585 µs |
| `AudioHandoff.Offer` | **1 µs** | 60 µs |

The spectrum source is 100.0% of the callback's time to one decimal place, and
62 ms against a 100,000 µs period sits exactly where the shack machine's 50 to
86 ms does.

**And `Push`'s three parts, each timed at the size `Push` runs it at:**

| Part | Cost | Share |
|---|---|---|
| 4,800 uncontended lock acquisitions | 69 µs | 0.1% |
| **4,800 full-ring `Array.Copy` of 16,383 floats** | **63,120 µs** | **99.5%** |
| One 16,384-point FFT with taper | 244 µs | 0.4% |

**The shift owns it.** Not the lock, and not the transform. 78 million floats
moved per 100 ms of audio.

**That changed what the remaining tasks were each worth, and the report says so
rather than crediting the fix to whichever landed last.** Task 2 is the repair.
Task 3 is worth 244 µs of the 62,271 and is insurance against variance. Both
were built as instructed.

Committed as `98286cd`.

### Task 2 — the ring stops moving 78 million floats a buffer

The ring gets a write cursor. A run of samples is written where the cursor points
in at most two `Array.Copy` calls and usually one, and nothing is shifted, which
is what `AudioTap` has done a few files away all along. The lock is taken per run
rather than per sample: `Push` cuts the incoming buffer at hop boundaries and
takes the lock once per piece, so a 4,800-sample buffer takes it once or twice
instead of 4,800 times.

The cut is not thrift. A frame has to be raised at exactly the sample the old
code raised it at, or every frame carries the wrong time, and on a mode whose
whole geometry is fifteen-second slots that is not a small error.

`Emit` pays instead, walking the ring from the oldest sample rather than from
index zero. That runs once a hop rather than once a sample, a factor of 4,096.

**`Push`: 62,271 µs to 270 µs median, 556 µs worst.**

**The picture is unchanged, and that is pinned rather than asserted.** The 43
frames a fixture produces, timestamps and every bin, were written to
`tests/fixtures/spectrum/waterfall-frames.txt` **from the old implementation,
before a line of it was touched**, and committed in that state. The rewrite
reproduces them byte for byte. A test that captured its own expectation after
the change would have passed whatever the change did (§12.5).

The fixture is pushed in 4,800-sample buffers on purpose: a device does not hand
over whole hops, and 4,800 does not divide the 4,096-sample hop, so a ring that
only worked when the buffer divided the hop would pass a tidier fixture and fail
on the radio.

Committed as `cefa179`.

### Task 3 — the transform leaves the callback thread

`OnSamples` copies onto a queue and returns. The ring write, the taper, the
transform, the decibel floor and `FrameReady` all happen on a worker.

**It reuses unit 238's `AudioHandoff` and does not grow a second mechanism.** It
fits without argument: bounded, ordered, oldest-dropped, one consumer, samples
copied before the call returns, never blocking, never throwing, counting what it
drops.

`Push` itself is unchanged and still synchronous, because the class's determinism
lives in it. What moved is who calls it, which is why task 2's pinned frames are
still byte-identical.

**Both designs measured in one run on one machine, with a `FrameReady` handler
sleeping 250 ms on every frame:**

| | Worst callback |
|---|---|
| Before, `Push` inline | **522,895 µs**, five whole buffer periods |
| After, offer and return | **92 µs** |

**A full queue drops rows and counts them while the audio survives:** 86 rows
dropped, 412,800 samples' worth, and a second subscriber feeding an `AudioTap`
on the same callback held **576,000 of 576,000 samples. Not one lost.** That is
the whole design in one measurement.

The worker runs `BelowNormal` on purpose. Detaching closes the queue discarding
rather than draining, because a row arriving after the source was detached has
nowhere to be drawn, and the join is bounded so a stalled handler cannot hold a
shutdown open.

The drop count and the worst frame duration join unit 239's numbers on all three
surfaces: `ft8_slot` telemetry on both paths, the sidecar's `audioPathDrops`
line, and the census line, which speaks only when rows were actually dropped and
says plainly that it cost the picture and not the audio.

Committed as `5ea9c71`.

### Task 4 — nothing is computed while nobody is drawing

**What `IsRunning` was actually driven by, which the instruction said it did not
know: `StartDecoding` and `StopDecoding` in `MainWindowViewModel`, lines 3964 and
4062, the CW decoder's own lifetime.** It has never had anything to do with
whether the Digital tab is showing, so before this the transform ran for an
entire evening whether or not the picture was on screen.

The honest test of whether anything is consuming frames is `FrameReady` being
non-null, and `WaterfallControl` already unsubscribes when it leaves the visual
tree. So the engine learns it without being told that tabs exist, which §0.1
requires. No UI signal was added and none was needed.

**The window is dropped rather than left sitting, and that is §0.0 rather than
thrift.** Skipping the work alone would leave the ring holding whatever was in it
when the tab closed, and the first frame after it reopened would be part old
audio and part new: a picture asserting a signal was present at a time it was
not, which HM-DEC-092 binds exactly as hard as a sentence. The cost is that the
first frame after somebody looks again waits one full window, about a third of a
second.

Measured: with nobody subscribed, forty buffers produce zero frames, zero drops
and a worst frame duration of zero, so the worker is never woken. Subscribe,
deliver the same forty, and 36 frames arrive.

Committed as `39d37ec`.

### Where the instruction did not match the tree

**Every claim in its verification list held exactly**: the lock inside the
`foreach`, the per-sample `Array.Copy`, `OnSamples` subscribed to a multicast
event, `WindowAt48K` 16,384 and `HopDivisor` 4, unit 238's hand-off, unit 239's
buffer length and `CallbackBudget`, version 1.12.43.

**One item in the parked list is stale.** It says the 51 red CW cases are ones
unit 239 could not compare against the baseline worktree at `d541fc8`. That
comparison did complete, in unit 239's closing minutes: every one of the 51 fails
at `d541fc8` as well, none is red at `HEAD` and green before, and the names are
checked in at `docs/unit239-failing-set.txt`. Nothing was done about them here,
and they remain parked, but the next instruction should not be written believing
the question is open.

### Recorded under §12.1

**Nothing.** Every conclusion here is a measurement.

## 2. What Tim should expect

**The waterfall looks exactly the same.** Same source, same window, same hop,
same bins. Tim's ruling of 2026-08-28 is untouched, and the 43 pinned frames
prove it rather than asserting it. The one visible change is that after opening
the Digital tab the first row now takes about a third of a second to appear,
because the window is deliberately started clean rather than drawn from audio
that arrived while nobody was looking.

**What is new that he can see on a shack-machine run:**

- The sidecar's `audioPathDrops` line gains the picture's own cost beside the
  audio's: `... 0 over the 100000 us buffer period, 241 over half of it, in 248
  timed, 0 waterfall row(s) dropped, worst frame 634 us`.
- `ft8_slot` telemetry carries `droppedFrames` and `longestFrameMicroseconds` on
  both the refused and the decoded paths.
- The census line adds one sentence **only if rows were actually dropped**,
  saying it cost the picture and not the audio, so he does not chase the sound
  card over a stuttering display.

**Build:** clean, 0 errors, 0 warnings, both projects.

**Tests:**

| Project | Result |
|---|---|
| `Hamlet.App.Tests`, not-Views leg | **530 of 530** |
| `Hamlet.App.Tests`, Views leg | **62 of 62** |
| Engine: audio, spectrum, waterfall, FT8, capture-sheet | **245 of 246**, twice in a row |

**What will look wrong and is not:**

- **`CwAdjudicationTests.ASpeedChangeInRealisticAudio` is red.** Named
  pre-existing by units 238, 239 and this instruction. Not touched.
- **The engine project has no total again**, and for the same reason as unit
  239: the full leg takes over half an hour and was not run to a summary here.
  What was run is the five channels this unit could affect, whole, twice. **The
  51 inherited CW reds are unchanged and unexamined**, and they are on this
  instruction's parked list.
- **`DroppedFrames` climbing is not a fault.** It is the design working: the
  picture falls behind and the audio does not.
- **A worst frame duration of 0 with 0 drops is not a broken counter.** It is
  nobody looking at the Digital tab, which after task 4 costs exactly one null
  check per callback.

**Pushed to `main`,** four commits, working tree clean of everything this unit
touched.

## 3. What we should do next

**The three numbers this unit was asked for.**

| | Figure |
|---|---|
| **Device callback's worst duration, stalled frame consumer** | **522,895 µs to 92 µs.** Both arms measured in one run on one machine. `Push` alone, unstalled: 62,271 to 270 |
| **Which of `Push`'s three parts owned the time** | **The per-sample full-ring `Array.Copy`: 63,120 µs, 99.5%.** The lock was 0.1% and the FFT 0.4% |
| **Frame-drop count from task 3's test** | **86 rows dropped, 412,800 samples' worth, counted**, while the tap on the same callback held 576,000 of 576,000 |

**And the one thing only the operator can supply, named as outstanding rather
than answered: the arrival ratio and the `over half of it` count on a
shack-machine sidecar.**

Before this unit, 554 of 561 callbacks ran past half the period, on a machine
where nothing overran and nothing dropped. On this machine the work behind that
figure fell from 62,271 µs to 270, and to 92 with the picture stalled. **If the
over-half count collapses and arrival does not, the spectrum source was not the
whole of it and the next suspect is the device or the driver.** That is the
reading the next instruction should be written from, and it cannot be taken here.

Then, in order:

1. **Take one shack-machine sidecar and read four lines**: `arrival`, the
   over-half count, `droppedFrames`, and the worst frame duration. Those four
   separate the picture having been the problem from something else also being
   wrong.
2. **Finish one uncontended engine run to a summary line.** Unit 239 could not
   and neither could this one; the project's total has now been missing from two
   consecutive reports. Budget the 45 minutes `docs/full-suite-run.md` asks for.
3. **The 51 inherited CW reds**, which are now a named, checked-in set with a
   baseline behind them and no owner.

## 4. What's blocking us

**Nothing is blocking the next step.** The shack reading needs an evening at the
radio and not a ruling.

One question is handed back:

> **The waterfall's window is thrown away whenever nobody is drawing it, so the
> first row after the Digital tab is opened is about a third of a second late.**
>
> That was my call under §0.0 and I think it is right: the alternative is a first
> row built partly from audio that arrived while the tab was shut, which is a
> picture asserting a signal was present at a time it was not (HM-DEC-092). But
> it is a visible behavior change to a surface Tim has ruled on before, and it
> trades a third of a second of blank against never drawing a mixed row.
>
> The alternatives: keep feeding the ring while nobody looks, which restores the
> instant first row and costs the transform nothing but keeps the ring writes on
> the callback, cheap now and still the coupling this unit just removed; or mark
> the first row after a gap as incomplete rather than discarding it, which needs
> a way for a frame to say so and nothing on the drawing side reads such a flag
> today.
>
> **Rejected already:** drawing the mixed row silently.

### Asks still outstanding

1. **The waterfall's dropped window and its late first row**, raised above,
   2026-09-03. **New.** The code is in `AudioSpectrumSource.Idle()` and shipped
   with the discard.
2. **`ReusableWindow`'s borrowed buffer**, raised by unit 239, 2026-09-03. The
   code is in `src/Hamlet.RadioEngine/Audio/ReusableWindow.cs`, shipped with the
   documented-rule option. Not touched.
3. **`ProcessDelayForTests` as a hook or a seam**, raised by unit 238,
   2026-09-03. The code is in `CwDecoder`. Not touched.
4. **The tap's owner**, parked since work instruction 238, 2026-09-03.
5. **The divergence ruling on `Ft8Sharp` sensitivity**, owner's, open. Nothing
   in `src/Ft8Sharp/` was touched.
6. **Unit 237's Extensible-format conclusion**: the fix stands, the exoneration
   does not (FACT-004). Taken at the shack from the sidecar's `encoding` line.
7. **Work instruction 231's four tree items**: the `PHASE_OUTCOME.md` header,
   the `RULES_AT` mismatch, uncommitted root paths, the Views stall.
8. **`validate-output.bat`'s permitted-spellings bug**, which has refused for ten
   units. Not exercised this unit.
