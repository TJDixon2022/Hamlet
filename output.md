# Work instruction 304 — telemetry solves the mystery without asking him anything

**READ IN THIS ORDER.** Two of the three mysteries can now be answered from one
uploaded file. **The third cannot, and section 3 says so plainly rather than dressing
it up** — a standard this unit fails on its own case is worth knowing tonight.

A. **The phase goal — FT4 works exactly the way FT8 does.** This unit advanced no
   step of it. It changed the record rather than the application: **no screen
   changed.**

B. **Step 4 and its exit criteria** — pressing FT4 tunes and decodes; the panel, the
   conversation, the ring, the filters, the tooltips, the ledger and the right-click
   menu all working unchanged; one click, one transmission; a whole exchange from one
   right click at the bench. **None was measured tonight.** Step 4 stays `partial`.

C. **The report last, and section 4 raises 3 items.** One is the ruling that would
   close the third mystery.

```
UNIT:       304 — complete at task 7 of 7, none dropped — 2026-09-10 12:04
PHASE GOAL: FT4 works exactly the way FT8 does.
UNIT GOAL:  One uploaded telemetry file says what state Hamlet was in, what changed,
            and what refused - and a bundle says the same about now.
ADVANCED:   no — no phase step moved. Step 4 stays partial: nothing tuned, nothing
            transmitted, no exchange run at the bench.
NUMBER:     of the three mysteries, how many are now diagnosable from the file alone
            BEFORE: none.
            AFTER:  two. The clock that never asked, and the transmit device that
            was not there. The third — did anything actually transmit — still
            cannot be answered, and section 3 names exactly what is missing.
DRIFT:      8 consecutive units without advance  (was 7, carried from unit 303)
```

---

## 1. What Claude did

**Complete. Seven tasks of seven, none dropped, including the named drop candidate.**
Development machine, prompt claimed `PROJECT: Hamlet`, branch `main`, six commits,
all pushed. Root version 1.12.266 to **1.12.267**, bumped once. **No file under
`src/Ft8Sharp/` was touched**, and nothing here transmits.

**Nothing in this report is evidence about the radio computer** (HM-DEC-093).

**Nothing was recorded to `DECISIONS.md`.** The four judgements made on this session's
authority are in `PHASE_OUTCOME.md`'s entry and listed below.

### Task 1 — what the record cannot currently answer

**Reading only, and it is the task that made the rest correct.** Written to
`docs/telemetry-diagnoses-it.md` before a line of code.

**The reframing is the finding: all three failures were states nobody could see, not
events nobody logged.** The application writes 2,600 events on an ordinary day and
almost all of them are *things that happened*. What was missing each time was **what
was true**.

Seven groups of facts came out of it, and three things were found while reading:

- **A telemetry category that is off drops its events silently.** `JsonlTelemetry.Write`
  returns before serialising, so a reader could not tell *nothing happened* from *not
  recorded*.
- **`DroppedEventCount` has existed all along and nothing ever wrote it.** A file that
  lost events looked exactly like a quiet evening.
- **`audio_device_chosen` carries one boolean**, `looksLikeRadio`, and nothing else.

### Task 2 — the startup snapshot

One event, one stable name, **written before the window is built** — so a machine
broken enough that the window never appears has still said what was wrong with it.

**A fact that cannot be determined says `unknown` and why.** Every reader is wrapped
**on its own**, so a sound card driver that throws records its own failure and the
other twenty facts still go out.

### Task 3 — the stream, and the refusal audit

**What was already right was checked before anything was added:** readiness already
fires on change with `determinedBy`, the radio's connect and drop already write, unit
303's clock events are untouched as instructed, and the arrival ratio was already
there.

**What was missing** is a device that goes and a setting that changes. Both now write
one `state_changed` with `what`, `from`, `to`, `why`.

**The refusal audit is the list, not the fixes** (§12.6). Section 3 carries it.

### Task 4 — the bundle

`Copy diagnostics` went from eight lines to the whole picture: **the same facts in the
same words as the snapshot**, plus the last 40 telemetry lines. **Still one paste**,
3.0 KB.

### Task 5 — the honest test

Two of three. Section 3 carries all three verdicts.

### Task 6 — what it costs

Measured off real writes, not estimated. Section 3.

### Task 7 — the outcome entry

Filed as **`UNIT 304 - STEP 4`** with nothing renumbered by hand.

### The four things decided on this session's authority

1. **The snapshot is written before the window**, so a machine that never shows one
   still describes itself.
2. **`Diagnostics` rather than an eighth category** — About still reports 7 of 7.
3. **Every reader is wrapped on its own**, not the gather as a whole.
4. **I reverted my own change.** I widened `SettingsViewModel`'s telemetry seam to the
   interface to make a test easier, claiming nothing there wanted the concrete type.
   **It does** — `ClearAll` and `TotalBytes` — so I undid it and the test reads a real
   `JsonlTelemetry` writing to a temp folder instead, which is the stronger
   measurement anyway.

---

## 2. What the owner should expect

**Nothing on screen changes.** Not one pixel. This unit changed what the file holds.

**The next time something breaks, upload one file and get an answer.** Every session
now begins with a single `startup_snapshot` line saying which sound cards exist, which
one is selected for receive and for transmit, **whether the selected one is actually
there**, what the settings file named that this machine has not got, the versions, and
which telemetry categories are on.

**And `App > About > Copy diagnostics` is now the whole picture in one paste** — the
same facts read fresh, plus the last 40 lines of the stream, so the run-up to a fault
comes with it. It still says, and still keeps, *no callsign, no name and no location*.

**What will look wrong and is not:**

- **The bundle got much longer** — eight lines to sixty-three. That is the point.
- **A lot of the snapshot says `unknown` on this machine.** The radio, the clock and
  readiness are not reachable where it runs, and it says so with a reason rather than
  leaving the fields out. **On your machine with a radio connected, most of those will
  carry values.**

**Build:** succeeded, 0 warnings, 0 errors. **Tests:** 21 constructed in this
instruction across four classes, **all green**, filtered by exact name and
foregrounded. **No suite was run** (HM-DEC-155). Gates re-run: `BindingHealthTests`
green, unit 303's device tests green — 41 of 41 in that filtered set.

**Pushed:** six commits to `main`.

---

## 3. What you should see

### 1. The startup snapshot, quoted whole, from this machine

```
{"ts":"...","sessionId":"...","level":"info","appVersion":"1.12.267",
 "category":"diagnostics","event":"startup_snapshot","data":{
  "appVersion":"1.12.267+...","ft8SharpVersion":"unknown",
  "ft8SharpVersionWhy":"Ft8Sharp is not loaded in this process yet, which is
                        ordinary before anything has decoded",
  "framework":".NET 8.0.x","osBuild":"Microsoft Windows 10.0.26200",
  "processBits":64,
  "audioInputCount":2,
  "audioInputs":"{in}.{mic} Microphone | {in}.{codec} USB Audio CODEC",
  "audioInputSelected":"(none named, so Hamlet preselects and never claims)",
  "audioInputPresent":"unknown","audioInputChosenBy":"not remembered",
  "audioOutputCount":2,
  "audioOutputs":"{...}.{speakers} Speakers | {...}.{usb-codec} USB Audio CODEC",
  "transmitDeviceSelected":"(none named, so a send refuses)",
  "transmitDevicePresent":"unknown",
  "radioConnected":"unknown","radioPort":"unknown","radioModel":"unknown",
  "clockOffsetKnown":"unknown","clockLastQueryReason":"unknown",
  "settingsLoaded":true,"settingsPathExists":true,
  "settingsNamedButAbsent":"(none)",
  "telemetryCategoriesOn":"Diagnostics | Rig | Tuning | Explore | Decode |
                           Transmit | Performance",
  "telemetryCategoriesOff":"(none)","telemetryEventsDropped":0}}
```

**One line, 2,381 bytes.** Every `unknown` above is followed by its own `…Why`.

### 2. The three replays, each with its verdict

**1. A clock query that never happens — YES, diagnosable.**
The snapshot says the offset is unknown and why; **there is no `clock_query_started`
anywhere in the file**, which since unit 303 means *nothing tried* rather than *tried
and failed*. Those two were the same empty file on the day it cost twenty minutes.

**2. A transmit device that is not present — YES, diagnosable.**
```
"transmitDevicePresent":false
"settingsNamedButAbsent":"transmit device {0.0.0.00000000}.{a-device-that-is-gone}"
"audioOutputs":"{...}.{speakers} Speakers | {...}.{usb-codec} USB Audio CODEC"
```
plus, at **warning**, the moment it was noticed:
```
"event":"state_changed","data":{"what":"transmitDevicePresent","from":"true",
 "to":"false","why":"settings name {...}.{a-device-that-is-gone} and this machine
 has 2 output devices, none of them that one"}
```
On the day, 56 refusals each named the field that decided them and **not one named the
device**.

**3. Did anything actually transmit — NO, and only partly better.**
The record reads `"outcome":"Played"`. Unit 303 stopped it lying, but **the question
is still unanswerable from the file.**

**What is missing:** whether the radio was at the other end of that port at that
moment, and whether it keyed. **Hamlet already polls `1C 00` four times a second and
reads `15 11` for power made**, so it is knowable — joining them to the transmission
record is ask 1, which is yours and which this instruction parks.

**What did improve:** the snapshot beside it names which audio devices exist and which
was selected, so *played into the wrong sound card* is now separable from *played into
the right one*. That was not answerable on the day either.

### 3. The refusals that are silent on screen

| Refusal | In the file | On screen |
|---|---|---|
| `transmit_readiness` | Yes, with `determinedBy` | **Yes** — `Decisions.Note` and the digital send line |
| `digital_capture_refused`, all four sites | Yes | **Yes** — each sets `StatusText` in the same breath |
| `cw_send_ended` | Yes | **Yes** |
| A transmit device that is absent | **Yes, new** | **Yes**, since unit 303's amber line |
| A category being off | **No, and it cannot be** | No — **a fact for the file, and the snapshot now names it** |
| A dropped telemetry event | **Yes, new** | No — rightly a file fact |
| `RigWriteOutcome.Refused` | Yes, in the result | **Unknown** |

**One honest gap: the last row.** `RigWriteOutcome.Refused` has **no consumer in
`MainWindowViewModel`**, so whether any live write refuses through that path could not
be settled by reading. **Reported as unknown rather than guessed either way** — it
wants a bench measurement against a radio.

**Nothing above was fixed.** The list is this unit's; the fixes are not (§12.6).

### 4. What the snapshot says it does not know, and why

On this machine, with no radio and before any decode:

| Field | Why |
|---|---|
| `radioConnected`, `radioPort`, `radioModel`, `radioBaud`, `radioCivAddress` | not reachable where the snapshot runs — it is written before the view model exists |
| `clockOffsetKnown`, `clockOffsetSeconds`, `clockLastQueryReason` | same, and the query is asynchronous — unit 303's two events carry it when it lands |
| `transmitReadiness` | same |
| `ft8SharpVersion`, `ft8SharpDeepVersion` | not loaded in this process yet, which is ordinary before anything has decoded |
| `audioInputPresent`, `transmitDevicePresent` | nothing is named in settings, so there is nothing to check for |

**Every one carries its own `…Why`.** The bundle's own test asserts that, and **it
caught two fields that were doing it wrong**: `ft8SharpVersion` and
`ft8SharpDeepVersion` returned the string `unknown (not loaded yet)`, putting the
reason inside the value with no `…Why` beside it. **The one rule this whole unit turns
on held everywhere except in those two fields.** Fixed.

### What it costs

- **The snapshot:** one line, **2,381 bytes**, once per session — 2.33 KB per start
  against an ordinary day of ~2,600 events.
- **The bundle:** 3,108 characters, 63 lines, **3.0 KB**. A paste, not a file.
- **The growth on a healthy machine: none.** The state-change events fire on change,
  not on a tick — an evening with Settings opened a dozen times and the device present
  writes **zero** of them. On the machine that lost its device it writes one each time
  Settings is opened, which is exactly when somebody is looking.

---

## 4. What's blocking us

### 1. The ruling that closes the third mystery

**Carried from unit 303 and now with a measurement behind it.** Task 5 shows this is
the one case the standard fails.

> **The transmission record says whether the transmitter keyed, taken from the radio
> rather than from the port write.** `Played` stays as what the audio path did; a
> second fact says what the radio did, and where the radio did not answer it says
> `unknown` rather than assuming either way.
>
> **Why:** the record exists to tell whether a fault is in the signal, the radio or
> Hamlet (§0.0.1), and it still cannot distinguish *the radio was off* from *it went
> out fine*. Hamlet already polls `1C 00` four times a second and reads `15 11`.
> **What is rejected:** inferring keying from a successful port write, which is what
> produced the 21 records.

**Not started.** It changes what the record asserts, which is yours (§12.1).

### 2. `RigWriteOutcome.Refused` has no consumer

Reported above. Settling it wants a live write against a radio, which is a bench
measurement.

### 3. `Ft8Sharp` did not move

No file under `src/Ft8Sharp/` was read, edited or built.

### Asks still outstanding

Carried verbatim per HM-DEC-139.

1. **Whether the transmission record asks the radio whether it keyed.** *First made
   2026-09-10, unit 303.* Restated above with task 5's measurement behind it.
   **Waiting on:** your ruling. **Where it sits:** `Played` is honest and unchanged.

2. **Nothing in this repository can look at a picture.** *First made 2026-09-09, unit
   300.* Seven units running. Real pixels want `Avalonia.Headless.Skia`, and **a
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
   **Note:** the About wobble may now move, because this unit made that window's text
   depend on the machine's device list.
