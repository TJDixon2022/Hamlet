# Work instruction 305 — the snapshot lands after the facts, and the Morse gate stops judging FT8

**READ IN THIS ORDER.** The unit did five things and reported a sixth as already
done. **Task 4 was shipped by unit 303 the same day**, so it is confirmed here rather
than rebuilt, and section 1 shows the measurement that settled it.

A. **The phase goal — FT4 works exactly the way FT8 does.** This unit advanced no
   step of it. It changed the record and one recording gate: **no screen changed.**

B. **Step 4 and its exit criteria** — pressing FT4 tunes and decodes; the panel, the
   conversation, the ring, the filters, the tooltips, the ledger and the right-click
   menu all working unchanged; one click, one transmission; a whole exchange from one
   right click at the bench. **None was measured tonight.** Step 4 stays `partial`.

C. **The report last, and section 4 raises 2 items** on top of a queue of five that
   you have not yet ruled on.

```
UNIT:       305 — complete at task 6 of 6, none dropped — 2026-09-10 14:01
PHASE GOAL: FT4 works exactly the way FT8 does.
UNIT GOAL:  The startup snapshot lands after the facts arrive, and the Morse gate
            stops writing a verdict about a transmitter nobody is operating.
ADVANCED:   no — no phase step moved. Step 4 stays partial: nothing tuned, nothing
            transmitted, no exchange run at the bench.
NUMBER:     how many of the snapshot's fields read `unknown` on a normal start
            BEFORE: 16
            AFTER:  9
            Of the snapshot's 50 data fields, 56 counting the event envelope. The
            nine that remain are the audio input, the transmit device, and the
            radio's port, baud, CI-V address and model — every one of them a fact
            nothing on this machine supplies, said with its reason.
DRIFT:      9 consecutive units without advance  (was 8, carried from unit 304)
```

---

## 1. What Claude did

**Complete. Six tasks of six, none dropped, including the named drop candidate.**
Development machine, prompt claimed `PROJECT: Hamlet`, and the tree confirmed it:
`SHACK_FACTS.md` and `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` both
present, `CoreHMI.sln` and `MURC.sln` both absent. Branch `main`, three commits, all
pushed. Root version 1.12.267 to **1.12.268**, bumped once. **No file under
`src/Ft8Sharp/` was touched**, and its own version did not move.

**Nothing in this report is evidence about the radio** (HM-DEC-093). This machine has
no IC-7300 on it. Where a measurement needed a connected radio, it says so instead of
approximating one.

**Nothing was recorded to `DECISIONS.md`.** The four judgments below were made inside
the order's own licence and are named here so you can overturn any of them.

**Task 1 — when each fact actually arrives.** Measured on a real start rather than
reasoned about. The early snapshot is written at **246 ms**; the clock query answers
**264 ms after it**; on your machine this morning the radio, the clock and readiness
all landed **1.0 to 1.3 s** after it. **Sixteen of the snapshot's fifty data fields
read `unknown`** in that early write, and every one of them was a fact that arrives
on its own a moment later.

**Task 2 — the snapshot is written twice.** Same event name, a `when` field carrying
`at_start` or `settled`, five seconds apart, **each readable entirely on its own**
rather than the second being a diff against the first. The engine holds the shape
(`StartupSnapshot`), the app does the gathering (`StartupFacts`), which is §0.1.

**Judgment 1: write twice rather than wait once.** Waiting would have traded the
early snapshot away, and the early one is exactly what a machine that dies during
startup leaves behind — which is the machine somebody needs a record of.

**Judgment 2: five seconds, and it is a wait rather than a condition.** Past both the
264 ms measured here and the 1.3 s measured at the shack. A condition would have
waited forever for a radio nobody plugged in; this writes regardless, and what has
not arrived is `unknown` with its reason.

**Task 3 — which of the two Morse-gate faults it was, established before touching
it.** The order named two possibilities and it is **the second**:
`TransmitReadiness.Check` has **exactly one production caller**, so it was never
blocking an FT8 send. What it was doing was writing `not_in_morse` into the record
about a keyer nobody was using — 56 times in the file you read this morning, which
is what sent you after a broken send chain when the fault was a missing sound card.

**Judgment 3: the scope is narrowed at the recording point and not in the check.** A
CW send still asks the gate and still needs the radio in CW. A test asserts exactly
that: asked about USB-D on 14.074000 the gate still answers `NotInMorse`, and it is
right to.

**Task 4 — reported as a mismatch against the tree rather than redone.** The order
presents the achievement mark's option B as outstanding. **Unit 303 task 5 shipped
it**, and it was confirmed by measurement rather than taken on trust:
`HmStatusMarkSize` is **27**, the mark draws `var ink = Edge` with
`var fill = line ? null : Green`, greyscale gives **2 shapes at rest against 4 lit**
so colour is not the only carrier (§0.6), the belt matches off the same resource, and
all four MainWindow ceiling rows sit exactly at their set-from figures — CW 426,
Digital 1194, working 1193, Voice 468.

**Judgment 4: confirming beats rebuilding.** Redoing shipped work to make an
instruction come true would have put a second answer in the tree for one question.

**Task 5 — this morning, constructed and replayed.** Section 3 carries it whole.

**Task 6 — the outcome entry.** Appended through `tools\arbiter\outcome-append.bat`
as `UNIT 305 - STEP 4`, state `partial`, fate `executed`, with the state's reason
saying plainly that nothing in step 4 was measured tonight.

**What was watched failing, twice.** A `DispatcherTimer` **does not tick under the
headless harness's `RunJobs`**, so the settled snapshot was written by a timer that
never fired and the whole task would have shipped looking green. It was caught only
because the test asserts *two* snapshots and found one; the timer was replaced with
`Task.Delay(...).ContinueWith(_ => Dispatcher.UIThread.Post(...))`. And
`NothingRecordsAMorseRefusalOnADigitalMode` **passed with the guard removed** —
because `CwTransmitViewModel.Refresh` returns early while `_transmitter` is null.
The test was strengthened to drive `ApplyRigState`, still could not reach the path,
and **now prints plainly what it cannot reach** rather than passing for the wrong
reason.

**No test suite was run** (HM-DEC-155). Seven tests were constructed across three
classes and every run was filtered by exact name, foregrounded, with a 300 to 500 s
timeout. Every `dotnet build` was foregrounded. Nothing was backgrounded and polled.

---

## 2. What the owner should expect

**Two things you will notice, and one of them is not this unit's doing.**

**The feather is noticeable.** It is 27 px, filled green at rest, and reads as two
distinct shapes against four when the colour is taken away, so it survives a
greyscale print. **It arrived in 1.12.266 with unit 303**, not tonight — this unit
measured it and left it alone.

**And the next telemetry file says what your machine was actually doing.** Upload one
after a start and you will find two `startup_snapshot` lines rather than one: the
first at about a quarter of a second, the second five seconds in with the radio, the
clock and the readiness filled in. **The number that matters is 16 down to 9**, and
the nine that remain each say why.

**What will look wrong and is not.** `transmit_readiness` events **no longer appear
at all while you are on a digital mode.** That is deliberate and it is the whole of
task 3. Their absence on FT8 is not the panel going quiet; a later session reading a
telemetry file for evidence about the panel needs to know those events are now
CW-only by design.

**Build clean, no new warnings.** Three inherited reds are unchanged and unnamed by
any order — `TheAchievementsScreenTests`' two and
`TheFitGuardAsksAboutTheGridTheSendIsOnTests`' one. **Nothing was run beyond the
seven tests this instruction constructed**, so this report claims nothing about the
rest of the suite.

---

## 3. What you should see

### 1. The snapshot, whole, from a start where the radio answered

The rig state below is the one a real poll delivers — USB-D on 14.074000, receiving —
pushed through `ApplyRigState`. **The radio fields the settled write fills are the
ones it fills at the shack.** What this machine cannot supply is a connected port, so
`radioConnected` reads `false` and the port, baud and CI-V address stay `unknown`
with their reasons; that is the honest edge of what a bench with no radio can show.

```json
{"ts":"2026-09-10T18:00:27.203Z","sessionId":"75596026","level":"info","appVersion":"1.12.268","category":"diagnostics","event":"startup_snapshot","data":{"when":"settled","appVersion":"1.12.268+a7db557","ft8SharpVersion":"unknown","ft8SharpVersionWhy":"Ft8Sharp is not loaded in this process yet, which is ordinary before anything has decoded","ft8SharpDeepVersion":"unknown","ft8SharpDeepVersionWhy":"Ft8Sharp.Deep is not loaded in this process yet, which is ordinary before anything has decoded","framework":".NET 8.0.24","osBuild":"Microsoft Windows 10.0.26200","processBits":64,"audioInputCount":2,"audioInputs":"... Microphone (EMEET SmartCam C960) | ... Microphone (USB Audio)","audioInputSelected":"(none named, so Hamlet preselects and never claims)","audioInputPresent":"unknown","audioInputPresentWhy":"nothing supplied this fact","audioInputChosenBy":"not remembered","audioOutputCount":4,"audioOutputs":"... S34J55x (3- HD Audio Driver for Display Audio) | ... S34J55x -2 (HD Audio Driver for Display Audio) | ... Ball Speaker (USBAudio2.0) | ... Speakers (USB Audio)","transmitDeviceSelected":"(none named, so a send refuses)","transmitDevicePresent":"unknown","transmitDevicePresentWhy":"nothing supplied this fact","radioConnected":false,"radioPort":"unknown","radioPortWhy":"nothing supplied this fact","radioBaud":"unknown","radioBaudWhy":"nothing supplied this fact","radioCivAddress":"unknown","radioCivAddressWhy":"nothing supplied this fact","radioModel":"mode USB","radioLastAnsweredSecondsAgo":5.1,"clockOffsetKnown":true,"clockOffsetSeconds":2.115,"clockOffsetAgeSeconds":5.2,"clockLastQueryReason":"measured","transmitReadiness":"Digital","transmitReadinessDecidedBy":"the operating mode the panel is on; the Morse gate is asked only in CW","settingsLoaded":true,"settingsPathExists":true,"settingsNamedButAbsent":"(none)","telemetryCategoriesOn":"Diagnostics | Rig | Tuning | Explore | Decode | Transmit | Performance","telemetryCategoriesOff":"(none)","telemetryEventsDropped":0}}
```

**The four fields to read**: `radioModel` says `mode USB` because the radio answered,
`radioLastAnsweredSecondsAgo` says `5.1` so its age travels with it, `clockOffsetKnown`
is `true` with the offset, its age and the query's own token beside it, and
`transmitReadiness` says `Digital` rather than repeating a Morse verdict.

*(The audio device ids are elided above only to keep the line readable. The real line
carries them in full, and no callsign, name, grid or location is in it — §2.1.)*

### 2. The same start, with no radio at all — it still writes

Both snapshots from one session, quoted whole, so you can see what the second write
buys. **16 unknown fields become 9.**

```
at_start    unknown fields: 16
settled     unknown fields: 9
```

```json
{"ts":"2026-09-10T17:59:33.367Z","sessionId":"6db52e44","level":"info","appVersion":"1.12.268","category":"diagnostics","event":"startup_snapshot","data":{"when":"at_start","appVersion":"1.12.268+a7db557","ft8SharpVersion":"unknown","ft8SharpVersionWhy":"Ft8Sharp is not loaded in this process yet, which is ordinary before anything has decoded","ft8SharpDeepVersion":"unknown","ft8SharpDeepVersionWhy":"Ft8Sharp.Deep is not loaded in this process yet, which is ordinary before anything has decoded","framework":".NET 8.0.24","osBuild":"Microsoft Windows 10.0.26200","processBits":64,"audioInputCount":2,"audioInputs":"... Microphone (EMEET SmartCam C960) | ... Microphone (USB Audio)","audioInputSelected":"(none named, so Hamlet preselects and never claims)","audioInputPresent":"unknown","audioInputPresentWhy":"nothing supplied this fact","audioInputChosenBy":"not remembered","audioOutputCount":4,"audioOutputs":"... S34J55x | ... S34J55x -2 | ... Ball Speaker (USBAudio2.0) | ... Speakers (USB Audio)","transmitDeviceSelected":"(none named, so a send refuses)","transmitDevicePresent":"unknown","transmitDevicePresentWhy":"nothing supplied this fact","radioConnected":"unknown","radioConnectedWhy":"nothing supplied this fact","radioPort":"unknown","radioPortWhy":"nothing supplied this fact","radioBaud":"unknown","radioBaudWhy":"nothing supplied this fact","radioCivAddress":"unknown","radioCivAddressWhy":"nothing supplied this fact","radioModel":"unknown","radioModelWhy":"nothing supplied this fact","radioLastAnsweredSecondsAgo":"unknown","radioLastAnsweredSecondsAgoWhy":"nothing supplied this fact","clockOffsetKnown":"unknown","clockOffsetKnownWhy":"nothing supplied this fact","clockOffsetSeconds":"unknown","clockOffsetSecondsWhy":"nothing supplied this fact","clockOffsetAgeSeconds":"unknown","clockOffsetAgeSecondsWhy":"nothing supplied this fact","clockLastQueryReason":"unknown","clockLastQueryReasonWhy":"nothing supplied this fact","transmitReadiness":"unknown","transmitReadinessWhy":"nothing supplied this fact","transmitReadinessDecidedBy":"unknown","transmitReadinessDecidedByWhy":"nothing supplied this fact","settingsLoaded":true,"settingsPathExists":false,"settingsNamedButAbsent":"(none)","telemetryCategoriesOn":"Diagnostics | Rig | Tuning | Explore | Decode | Transmit | Performance","telemetryCategoriesOff":"(none)","telemetryEventsDropped":0}}
```

```json
{"ts":"2026-09-10T17:59:38.676Z","sessionId":"6db52e44","level":"info","appVersion":"1.12.268","category":"diagnostics","event":"startup_snapshot","data":{"when":"settled","appVersion":"1.12.268+a7db557","ft8SharpVersion":"unknown","ft8SharpVersionWhy":"Ft8Sharp is not loaded in this process yet, which is ordinary before anything has decoded","ft8SharpDeepVersion":"unknown","ft8SharpDeepVersionWhy":"Ft8Sharp.Deep is not loaded in this process yet, which is ordinary before anything has decoded","framework":".NET 8.0.24","osBuild":"Microsoft Windows 10.0.26200","processBits":64,"audioInputCount":2,"audioInputs":"... Microphone (EMEET SmartCam C960) | ... Microphone (USB Audio)","audioInputSelected":"(none named, so Hamlet preselects and never claims)","audioInputPresent":"unknown","audioInputPresentWhy":"nothing supplied this fact","audioInputChosenBy":"not remembered","audioOutputCount":4,"audioOutputs":"... S34J55x | ... S34J55x -2 | ... Ball Speaker (USBAudio2.0) | ... Speakers (USB Audio)","transmitDeviceSelected":"(none named, so a send refuses)","transmitDevicePresent":"unknown","transmitDevicePresentWhy":"nothing supplied this fact","radioConnected":false,"radioPort":"unknown","radioPortWhy":"nothing supplied this fact","radioBaud":"unknown","radioBaudWhy":"nothing supplied this fact","radioCivAddress":"unknown","radioCivAddressWhy":"nothing supplied this fact","radioModel":"unknown","radioModelWhy":"nothing supplied this fact","radioLastAnsweredSecondsAgo":"unknown","radioLastAnsweredSecondsAgoWhy":"nothing supplied this fact","clockOffsetKnown":true,"clockOffsetSeconds":2.117,"clockOffsetAgeSeconds":5.2,"clockLastQueryReason":"measured","transmitReadiness":"Digital","transmitReadinessDecidedBy":"the operating mode the panel is on; the Morse gate is asked only in CW","settingsLoaded":true,"settingsPathExists":true,"settingsNamedButAbsent":"(none)","telemetryCategoriesOn":"Diagnostics | Rig | Tuning | Explore | Decode | Transmit | Performance","telemetryCategoriesOff":"(none)","telemetryEventsDropped":0}}
```

**`radioConnected` goes from `unknown` to `false`, and those are different facts.**
The first says nothing supplied it; the second says Hamlet looked and there is no
radio. The clock filled itself in between the two writes, which is the whole reason
the second one exists.

### 3. Which of the two Morse-gate faults it was, and what a digital send records now

**It was the second.** `TransmitReadiness.Check` has one production caller, so it
**never refused an FT8 send** — the send path does not consult it. What it did was
fill the record with a verdict about a keyer nobody was using, which is what made 56
`not_in_morse` lines look like a broken send chain.

Asked directly about your own state — USB-D, 14.074000, receiving — the gate is
unchanged and still refuses:

```
the CW gate, asked about USB-D:
  state  : NotInMorse
  reason : not_in_morse
```

**And on a digital session nothing records it:**

```
transmit_readiness events on the Digital tab: 0
```

**What that test cannot reach, said plainly rather than claimed.** The readiness
callback fires only once a transmitter exists, and a transmitter exists only once a
radio is connected — `CwTransmitViewModel.Refresh` returns early while `_transmitter`
is null. **So a headless session writes no `transmit_readiness` events with the guard
in or out**, measured both ways. Reproducing the 56 refusals needs a connected radio
in USB-D, which this machine cannot stage. What is proved here is the deterministic
half: the CW check still refuses USB-D, so nothing was weakened, and a digital session
records no Morse verdict.

### 4. Task 5 — this morning, replayed, and whether a command would have been needed

Your state constructed: a transmit device named in settings and absent from the
machine, a clock that had not answered, a radio not connected.

```
when                      settled
transmitDevicePresent     false
settingsNamedButAbsent    transmit device {0.0.0.00000000...
audioOutputCount          4
radioConnected            false
clockOffsetKnown          true
clockLastQueryReason      measured
transmitReadiness         Digital
not_in_morse refusals in the file : 0
```

**For the sound card, no command would have been needed.** `transmitDevicePresent`
reads false, `settingsNamedButAbsent` names the device that went away, and
`audioOutputCount` says how many the machine does have. That is the whole of this
morning's hour, answered in three fields of one uploaded file.

**For the clock, no.** The settled snapshot carries whether an offset is held, its
age, and the last query's own token, so a reader sees the state rather than inferring
it from an absence.

**And the record is no longer misleading.** There are no `not_in_morse` lines to read
as a broken send chain.

**What is still missing, and this unit does not claim it.** Whether anything actually
keyed the transmitter. That is ask 1 below, unchanged since unit 303: the radio's own
transmit state is still not joined to the transmission record.

---

## 4. What's blocking us

**Nothing blocks the next unit.** Two items want a ruling before the work they
concern can be done well.

**1. The one thing an uploaded file still cannot answer is whether anything keyed.**

> Hamlet's transmission record says a message was **played** and never that the radio
> **transmitted**, and it stays that way until the radio's own transmit state is
> joined to it.
>
> Why: `Ft8TransmitOutcome.Played` and `TransmitRun.AudioWentOut` were renamed in unit
> 303 precisely because the old names claimed something the code had not measured, and
> that honesty is worth keeping. But the radio broadcasts `1C 00` four times a second
> and the record does not consume it, so a send with the antenna disconnected and a
> send that put a hundred watts out look identical in the file. This morning that was
> the third of three mysteries and it is still the third.
>
> Rejected: inferring it from the Po meter, which is only meaningful while
> transmitting and so answers a question about the moment rather than about the send;
> and inferring it from the audio path, which is the same category error the rename
> was made to stop.

**2. Whether `transmit_readiness` should be recorded at all outside CW.**

> This unit narrowed the recording to CW and left the check untouched. **A weaker
> option exists and was not taken**: record it everywhere but at `debug` level.
>
> Why the narrow one was chosen: §8.1 says anything a person would want to find by
> scanning is not `info`, and a Morse verdict about a digital send is not a thing
> anybody wants to find at all — it is noise that already cost you an hour. But the
> absence is itself a fact a future reader has to know about, which is a real cost and
> is why this is here rather than settled.
>
> Rejected: removing the check's Morse condition, which would have weakened a gate
> that is right.

### Asks still outstanding

Carried verbatim per HM-DEC-139.

1. **Whether the transmission record asks the radio whether it keyed.** *First made
   2026-09-10, unit 303.* Restated above with task 5's measurement behind it.
   **Waiting on:** your ruling. **Where it sits:** `Played` is honest and unchanged.

2. **Nothing in this repository can look at a picture.** *First made 2026-09-09, unit
   300.* Eight units running. Real pixels want `Avalonia.Headless.Skia`, and **a
   package is a dependency decision** (§0.4). **Waiting on:** your ruling. **Where it
   sits:** nowhere.

3. **The map image.** *First made 2026-09-09, unit 301.* The projection is built and
   proved; the picture and its three numbers are missing. **Waiting on:** a file.
   **Where it sits:** `assets/azimuthal-map.md`.

4. **Three inherited reds** — `TheAchievementsScreenTests`' two and
   `TheFitGuardAsksAboutTheGridTheSendIsOnTests`' one. *First made 2026-09-10, units
   302 and 303.* All proved red with the previous unit's changes stashed, and named by
   no order. **Waiting on:** an order that takes them.

5. **Two ±2 wobbles** on `SettingsWindow` and `AboutWindow`, far under their ceilings.
   *First made 2026-09-10, unit 302.* **Waiting on:** an order that takes them.
   **Note:** unit 304 made the About window's text depend on the machine's device
   list, so that wobble may now move.
