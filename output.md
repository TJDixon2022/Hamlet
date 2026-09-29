## 1. What Claude did

**Surface and gate.** Claude Code on the development computer at `C:\Source\HamLet`, branch
`main`. The prompt carries `PROJECT: Hamlet`, and all five of section 0's checks hold. Hamlet
confirmed. Nothing in this report is evidence about the radio.

**Run by hand, outside the loop.**
- `SESSION.lock` was taken through `tools\arbiter\lock.bat take` and released the same way.
- Nothing was written to `RUN_LEDGER.md`, and nothing under `tools\arbiter\` was touched.
- No box was ticked; `PHASE_PLAN.md` has 69 before and after.
- No recording, fixture, floor or telemetry was read.
- Nothing under `.run-unit\` was committed.

**Task 0.** Unit 496 is in the tree: `ThatPitchIsTheStationsOwn` is green at HEAD, 0 of 1404.

**The changes, file by file** (all in `bda95c42`):
- **`src/Hamlet.RadioEngine/Cw/CwEnvelopeDetector.cs` - a mark rises and falls like a key.**
  - **The rule:** a completed bar is handed out as a mark only if the peak's level falls at least
    `EdgeDepthDb` (6 dB) below the mark's top within `EdgeHops` (4) hops before its first flat hop,
    and again within four hops after its last.
  - **The switch:** `MarksNeedEdges`, on, so the difference can be counted.
  - **What it gates:** only the marks. The pairing, the keying verdict, the light and the scope see
    every bar as before.
  - **Nothing loosened or re-weighted:** the flatness tolerance, the shortest bar, the pairing, the
    wander check, unit 491's and 492's rules and unit 496's bin choice are all untouched.
  - **The one cost:** a bar is handed out four hops after its end rather than two, so the falling
    edge's hops exist. That is still inside unit 492's three-window bound.
- **`src/Hamlet.RadioEngine/Bands/W1awMorseFrequencies.cs`:**
  - Reads the schedule and its named zone from the data file.
  - `At(utcNow)` gives the Morse run scheduled now or next, converting each run's start and end
    from US Central on its own date through the time zone database.
  - `FindZone` takes the IANA name, falling back through the Windows name.
- **`data/bands/w1aw-morse.json`:** the seven Morse rows of the ARRL schedule, the zone named as
  `America/Chicago`, and the ARRL cited. The file notes that legal holidays are not listed.
- **`src/Hamlet.App/ViewModels/W1awButton.cs`:** `ScheduleLine` formats the line in the operator's
  own clock, and `ScheduleTip` is the hover.
- **`src/Hamlet.App/ViewModels/MainWindowViewModel.cs`:**
  - `W1awScheduleLine`, `W1awScheduledNow` and `W1awScheduleWord`, kept current by a twenty-second
    timer.
  - `UpdateW1awSchedule`, which the tests drive with a chosen moment and zone.
- **`src/Hamlet.App/Views/MainWindow.axaml`:** a dot beside the W1AW button, filled or hollow, with
  the word *scheduled* or *quiet* beside it, and one line under the button. All of it is still the
  last thing in the send column.
- **Tests.**
  - The run-reader file gains four: the noise count, the real edge, and the two faded tones.
  - `W1awScheduleLineTests`, new, has four.
- **Records.**
  - R107 is in both `PHASE_PLAN.md` copies, and `DECISIONS.md` has HM-DEC-201.
  - Both outcome copies have `## UNIT 497 - STEP 12`, and both status copies name 497.
  - Version 1.13.183 → 1.13.184.

**The rise and fall bound, and its reason - the author's, overrulable, not fitted or tuned.**
- **Four hops, twenty milliseconds.** The level is read through a window two hops long, so the
  window alone spreads a keyed step across two hops. A keyer shapes its edge over a few
  milliseconds more, allowed another two hops.
- **6 dB deep.** Half amplitude, the point a keyed element's edge is conventionally timed at.
- **What a real keyed edge measures here:** on the clean call, every one of the call's 65 marks
  clears the bound. All 65 with the test off, all 65 with it on.

**Watched failing first.**
- The noise count and the faded tone were measured with the test off and on, in the same run.
- At HEAD, the W1AW line and dot and the schedule in the data file do not exist, so their cases
  are red by absence.

**Verification.**
- The build: 0 warnings, 0 errors.
- **The app carry-forward line: 278 of 278.**
- **The app one-truth, layout, voice, bindings, hover registry and W1AW types: 96 of 99.** Two
  top-row names failed in the long run and pass alone (15 of 15). The British spelling red is from
  2026-09-26, not this unit.
- **The engine run, gate and detector types: 36 of 38.** The reds are
  `AMarkIsTheEnvelopeOverAThresholdTests`' two cases, at 70 and 208, unchanged.

## 2. What the owner should expect

1. Rebuild.
2. **Somewhat fewer false letters.** A stretch of noise that was flat and long enough to count as a
   dot or a dash now also has to rise and fall like a key.
   - On the test's loud noise this turned away about a quarter of such stretches, not most of them.
     That noise jumps from moment to moment rather than drifting, so much of it still has edges.
   - Real band noise that drifts should be turned away more often.
3. **A real station is unaffected.** Its keyer's edges sit well inside the bound, and no mark of the
   test call was lost.
4. **If real letters go missing, that is the bound being too tight**, and it is the one thing to
   report back.
5. **Beside the W1AW button there is now a dot and a line.**
   - The dot is filled with the word *scheduled* while the ARRL's schedule has a Morse run on, and
     hollow with *quiet* otherwise.
   - The line says what is on until when, or what is next and when, in your own clock.
   - It is the ARRL's schedule, not a sign that W1AW is on the air: the ARRL leaves out legal
     holidays, and the hover says so.

## 3. What you should see

**The noise counts** (case 1, thirty seconds of loud noise):

| | count |
|---|---|
| bars passing every existing test - height, duration, flatness | 1452 |
| of those, with edges | 1069 |
| turned away by the edge test | 383 |

- **Marks handed out on noise:** about 48 a second before, about 36 after.
- **Three minutes of noise:** 477 marks in the last minute before, 341 after.
- **Both noise tests stay green:** nothing printed, before or after.

**The five cases**, identical in the terminal and the scroll, before and after:

| case | before (HEAD) | after |
|---|---|---|
| clean call | `CQ CQ DE N0CALL N0CALL K` | `CQ CQ DE N0CALL N0CALL K` |
| call with bursts in every gap | `CQ CQ DE N0CALL N0CALL K` | `CQ CQ DE N0CALL N0CALL K` |
| the call plus a second station 200 Hz away | `CQ CQ DE N0CALL N0CALL K` | `CQ CQ DE N0CALL N0CALL K` |
| a lone dit | nothing | nothing |
| a lone dah | nothing | nothing |

**Case 4, the faded tones:**
- **Faded up 100 ms, held 200 ms, faded down 100 ms:** no mark with the edge test on. It was
  already no mark with the test off, because unit 492's rule that a mark ends where its tone ends
  turns away the slow fall.
- **Faded up 100 ms, held 200 ms, cut off sharply:** one mark with the test off, none with it on.
  Only the rising edge catches this one.

**The W1AW schedule, driven headless in named zones:**
- **Inside a bulletin**, 7:30 PM Central read in Eastern: `Sending now: code bulletin, 18 WPM, until
  9:00 PM`, dot filled, *scheduled*.
- **At a quiet time**, Wednesday 8:38 PM Central: `Next: code practice, fast, 9:00 PM - in 22
  minutes`, dot hollow, *quiet*.
- **Past the last run of a day**, Tuesday 11:30 PM Central: `Next: code practice, slow, tomorrow
  8:00 AM`.
- **Daylight saving:** the 7 PM Central bulletin starts 00:00 UTC in October and 01:00 UTC in
  December.

**The CW tab's text count:** 653, against a ceiling of 550. It was 597 after unit 495.

## 4. What's blocking us

Nothing blocks. What is left, a line each:
- **The edge test turns away a quarter of white noise's bars, not most.** The rest have edges by
  this bound. What still keeps noise off the screen is the run reader's keyed-mark and two-run
  rules.
- **A mark is now handed out 25 ms after it ends at worst**, from 15, to read its falling edge.
- **At the time of this run** (20:54 UTC, Tuesday, this machine on Eastern): `Sending now: code
  practice, slow, until 5:00 PM`, dot *scheduled*. **The schedule lives in
  `data/bands/w1aw-morse.json`**, beside the frequencies, cited to the ARRL.
- **The CW tab's text count is 653 against 550**; the ceiling is not this unit's to move.
- **Pre-existing reds, not this unit's:**
  - `VoiceTests`' British spelling.
  - `HowMuchTheApplicationSaysTests`.
  - `ModeFollowsTheMapAgainTests.NothingButTheModeIsEverWritten`.
  - `AMarkIsTheEnvelopeOverAThresholdTests`' two cases.

### Asks still outstanding

- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25, waiting on
  the owner; no change sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8
  record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, so nothing on it is ever
  revised, at the cost of seconds of lag. On the run path, now the only path to the screen, the
  terminal shows only settled text. The ask stands only for the timing-only path, which no longer
  reaches the screen; no change for it sits in the tree.
