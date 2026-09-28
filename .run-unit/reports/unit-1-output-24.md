READ IN THIS ORDER.

A. What the owner will see on the CW tab: directly under the transcript there is a new row.
   On the left is a light. Lit, it reads "● I think I hear CW" on green. Dark, it reads
   "○ I don't think I hear CW" on slate. Its hover says it is lit when the keying meter calls
   it keying or the survey admits a pitch, and that this is Hamlet's guess, not a fact. On the
   right are two buttons, "I agree with you" and "You're an idiot". They are live while Hamlet
   is listening, and each press writes one telemetry row. Under the row is a strip drawn from
   200 to 1200 Hz. The band the tracker actually searches, 300 to 900 Hz, is shaded and
   labelled with those numbers. The pitch Hamlet is mixing at is a solid line labelled
   "mixing 612 Hz" ("..., not measured" where it fell back). The keying meter's best pitch is
   a dashed line labelled "meter 610 Hz". Every pitch the survey admitted is a short tick, and
   the line under the strip names them: "survey admitted 575 Hz" or "survey admitted
   nothing". The strip's hover explains each mark and gives the meter's score, median ms,
   swing dB and verdict. Nothing about where the detector looks has changed.
B. Step 11 is the owner's ear teaching the detector where the entry is.
   - 11.1, the light: built. Watched red 3 of 4 on a stub, now green 5 of 5, including the
     built window.
   - 11.2, the strip: built. Watched red 6 of 6 on stubs, now green 7 of 7, including a
     paint in the built window with a driven tracker pitch.
   - 11.3, the buttons and the row: built. Watched red 5 of 5 on stubs, now green 6 of 6,
     including the row going through the real `.jsonl` writer's serializer.
   `PHASE_PLAN.md` holds step 11 as a single summary line with no 11.x checkboxes, so there
   was nothing to tick.
C. The rest: build 0 errors with warnings as errors. App carry-forward line 278/278 at entry.
   At exit it lost 1 then 6 names, and every lost type passed alone. The touched types are
   48/48 across nine. The transmit files and `src\Hamlet.RadioEngine\Cw` both print nothing.
   An owner's `STOP` file appeared at the root after launch and was left in place. No
   recording was read.

UNIT:       474 - complete at task 4 of 4, none dropped - 2026-09-27 20:55
PHASE GOAL: Hamlet's CW receive meets CW_REQUIREMENTS.md. Under R88 the owner's ear at the radio, not the corpus, is what judges whether it hears CW at all.
UNIT GOAL:  Put the detector's existing opinion on the CW tab as a light, with a strip showing where it looks, and let the owner tell it with one press whether it was right. Each press writes a row the next unit can read.
ADVANCED:   yes - step 11 criterion 1. The light is on the CW tab, driven only by the meter and the survey, in words and color, and a headless test drives it. Whether it is right on the air is the owner's call, and the buttons exist for that.
NUMBER:     recordings read: 0; controls added: 4 (light, strip, two buttons); telemetry event: owner_verdict
DRIFT:      step 11 0 (first unit on it); step 3 0; step 4 1; step 5 1; step 6 0; step 7 0; step 9 0 (carried from 472's report; steps 2 to 10 are closed under R88)

## 1. What Claude did

**Complete: all 4 tasks of 4, none dropped.** This was a Claude Code session on QUIVERFULL.
The gate was confirmed as Hamlet (SHACK_FACTS.md, CwProbabilisticDecoder.cs and
CW_REQUIREMENTS.md present, and no CoreHMI.sln or MURC.sln). Branch `main`. Five commits,
each pushed before the next task began: 9c84ad50, 805840a4, ba3d0ddc, cdf9edca, and the
task 4 commit that carries this report.

**Task 0, the record.**
- `PHASE_OUTCOME.md` got `## UNIT 474 - STEP 11`, written from the instruction's block, in
  both copies.
- `PHASE_STATUS.md` now names 474 and `CURRENT_STEP: 11`, in both copies.
- The version went from 1.13.159 to 1.13.160.
- HM-DEC-184 went into `DECISIONS.md`, and its row went at the top of `CLAUDE.md` §1's table,
  dated 2026-09-27.
- The runner's writes to `.run-unit` were committed as they stood.
- Entry round: build 0 errors, and the app carry-forward line 278/278 on the first run. The
  engine line and the three floors were not run (R88).

**Task 1, the light.** `CwHearingViewModel` (new) holds the light.
- It is lit when `KeyingReading.Verdict == Keying` or the survey's admitted list is not empty.
  No new detector was added.
- It is fed once a second, on the keying meter's own cadence, from `ObserveHearing` in the
  decode tick.
- The seams it reads, all of which already existed:
  - the meter reading `PublishKeying` already holds;
  - `CwDecoder.Report`, for `ToneHz`, `PitchWasMeasured` and `HasKeying`;
  - `CwToneTracker.CoarseCandidates()`, for the admitted bins.

**Task 2, the strip.** `CwPitchStripControl` (new) draws `CwHearingViewModel.Strip`.
`Marks()` and `XOf()` are public, so a test can see exactly what is drawn and what words each
mark carries.

**Task 3, the buttons and the row.** "I agree with you" and "You're an idiot" run
`AgreeCommand` and `IdiotCommand`.
- Each writes one row through the app's existing `JsonlTelemetry`, category `cw`, event
  `owner_verdict`.
- Its fields are exactly: `verdict`, `light`, `trackerHz`, `trackerHasPitch`,
  `trackerHasKeying`, `meterVerdict`, `meterHz`, `meterScore`, `meterMedianMs`,
  `meterSwingDb`, `survey` (one `{hz, levelDb}` per admitted bin), `frequency`, `mode`,
  `agc`, `preamp`, `inputPeakDb`, `inputFloorDb`, `sinceVerdictMs`.
- The test asserts that key set closed both ways.
- No audio is captured and no sidecar is written.
- The CW tab's closed control list (`EveryControlSaysWhatItDoesTests`) now names both buttons.

**Task 4, the exit round.**
- `Hamlet.sln` builds with 0 errors under warnings as errors.
- App line, first run: 277/278. The one loss was `TheCarrierHoldsTheButtonsTests`, to
  "You've caused dispatcher loop".
- App line, the one rerun: 272/278.
  - `TheFavoritesAreChipsTests` lost 2, both to the dispatcher loop.
  - `TheStopIsAlwaysOnScreenTests` lost 3, and `ThePsk31ConversationCardTests` lost 1. These
    were assertion failures, not dispatcher loops.
- Run alone, all four types passed: TheStopIsAlwaysOnScreenTests 5/5,
  TheCarrierHoldsTheButtonsTests 8/8, TheFavoritesAreChipsTests 4/4,
  ThePsk31ConversationCardTests 8/8.
- The stop test and the PSK31 card test have both failed in the full line before this unit,
  all before any of this unit's code existed.
  - The stop test is in the saved line outputs of units 410, 413, 425, 427, 441 and 460
    (entry), 412 and 443 (exit), 415 and 466.
  - The PSK31 card test is in those of units 405 (exit), 408 and 412 (entry).
- Touched types at exit: 48/48 across nine (the three new types, the two control-inventory
  types, BindingHealthTests, TheCapturePressIsOnTheScreenTests, SettingsRoundTripTests and
  Unit303ClockRecordTests).
- `git diff 7e209cb4` over the eleven transmit files prints nothing.
- `git diff f8fc3921 -- src/Hamlet.RadioEngine/Cw` prints nothing. No seam had to be exposed.
- No file under `tests/fixtures` changed.

**No recording was read.** The unit ran no floor, no engine line, no metric and no keyed set.

**Decisions I made myself, all overrulable:**
1. **Wording.** "I think I hear CW" and "I don't think I hear CW". The hover is the
   instruction's sentence exactly. The strip labels are "searched 300 to 900 Hz",
   "mixing N Hz", "mixing N Hz, not measured", "meter N Hz" and "survey admitted …". Each
   button's hover says it writes one row, what the row carries, and that no audio is kept and
   nothing on the radio changes.
2. **Where the controls sit.** Directly under the transcript, above the advisory region, in a
   row that is always present so it never reflows the screen.
3. **The drawn range** is 200 to 1200 Hz, as the instruction asked.
4. **Only while listening.** The buttons are enabled only while Hamlet is decoding, like
   "I hear a station". The input levels in the row are null when nothing is listening.
5. **What "level" means in `survey`.** It is `KeyingCandidate.KeyedDb`, the bin's loudness
   while the key is down, written as `levelDb`. `LiftDb` is not written, because the
   instruction says "Hz and level" and "nothing else".
6. **The `light` field** is the light's own words at the press.
7. **`sinceVerdictMs`** is measured from the light's last change. If the light has never
   changed, it is measured from when the view model was made.
8. **A new telemetry category.** `TelemetryCategory.Cw` was added to the engine's telemetry
   enum, whose own comment requires any addition to be deliberate. It is on by default and
   has no row on the Settings screen, as `Psk31` has none.
9. **A figure not measured is written as null, never NaN.** A test checks the row through
   `JsonlTelemetry.Serialize` itself.
10. **An existing call on the UI thread.** The UI thread calls `CoarseCandidates()` once a
    second. That call is not new: the UI thread already calls it every tick, through
    `CwDecoder.Report`, then `Tracker.Competitor`, then `_survey.Candidates()`, whenever a
    keyed verdict exists. The light adds one more call a second through the same seam.

**Mismatches, reported for the record:**
- **An owner's `STOP` file appeared at the root after launch.** It was empty and dated 20:29,
  and was not in the first root listing. `run-phase.bat` (089) reads it only between units,
  never mid-unit. Unit 472 handled the same file the same way: I finished this unit, started
  nothing after it, and left the file in place, unstaged, for the runner. `SESSION.lock` was
  the launcher's and was not touched.
- **The app carry-forward line reads WAV files** under `assets/fixtures/`: generated PSK31 and
  Olivia fixtures. None of them is CW and none is under `tests/fixtures/cw`. The instruction
  orders that line run, and bars only a CW floor, so it ran whole. If R88's "any test whose
  input is recorded audio" is meant to cover these too, the line needs trimming, and that is
  the owner's ruling.
- **`PHASE_PLAN.md` holds step 11 as one line and has no 11.1 to 11.3 criteria.** The
  instruction's WHY cites "criterion 11.1". Nothing was ticked and no criteria were invented.
- `docs/phase-requirements/PHASE_STATUS.md` has long been stale against the root copy. Its
  step lines were left as they were.
- `tools/status.sh` hardcodes `RULES_AT: HM-DEC-165`. This session wrote the status through
  `.run-unit/unit474-status.sh` with `RULES_AT: HM-DEC-184`.

## 2. What the owner should expect

**At the radio:**
1. Start listening on the CW tab.
2. Tune a station.
3. Look at the light, then at the strip: is the solid line where you hear the station? Is it
   inside the shaded band?
4. Press "I agree with you" if the light is right about what you hear, or "You're an idiot"
   if it is wrong. That covers a station it says it doesn't hear, and a light that is lit on
   nothing.
5. Tune the next one and repeat.

Each press is one line in today's file under `%AppData%\Hamlet\telemetry\`, category `cw`,
event `owner_verdict`. The next unit reads those rows, and nothing else, to decide where the
detector should look and when it should start.

**What will look wrong but is not:**
- **A station you can hear beating above 900 Hz or below 300 Hz will leave the light dark.**
  No solid line will land on it, and the survey will admit nothing there, because the tracker
  does not search there. The strip is drawn wider precisely so you can see that. It is the
  thing to press "You're an idiot" on.
- **The solid line always has a number, even on an empty band.** With nothing measured, the
  decoder mixes at the last pitch, the bank's centre or your CW pitch, and the label then
  says "not measured".
- **The dashed meter line can sit apart from the solid line.** The meter sweeps on its own
  and shares nothing with the tracker. The two disagreeing is information, not a fault.
- **The light can be lit by the survey while the meter still says "listening"**, and the
  reverse. Either one lights it.
- **The light and strip update once a second, not instantly.**
- **The buttons are greyed out until listening starts.**
- **Nothing about decoding has changed.** The same stations will still give no characters.
  This unit shows what the detector thinks; it does not fix it.

## 3. What you should see

**The answer: the light, the strip and the two buttons exist on the CW tab.** They are driven
by the detector already in the tree, and every press writes the `owner_verdict` row with the
eighteen fields the instruction names and no others. Nothing was measured against a
recording.

- `TheLightSaysWhatHamletThinksItHearsTests` 5/5. A meter verdict of keying changes the words
  to "I think I hear CW". An admitted pitch lights it while the meter is listening. It goes
  dark in words. The hover sentence is exact, and the dark light is drawn on the CW tab of
  the built window.
- `TheStripShowsWhereTheDetectorLooksTests` 7/7. A driven tracker pitch of 612 is marked
  "mixing 612 Hz", and an assumed 600 reads "mixing 600 Hz, not measured". The strip spans
  200 to 1200 Hz with a searched band of `CwToneTracker.MinimumToneHz` to `MaximumToneHz`,
  labelled "searched 300 to 900 Hz". Admitted bins 575 and 612.5 read
  "survey admitted 575, 613 Hz". The meter reads "meter 610 Hz", and the hover carries
  score 0.21, median 70 ms, swing 22 dB and verdict keying. Every drawn mark carries words.
  The strip paints in the window.
- `TheOwnersVerdictIsARowTests` 6/6. Each button writes exactly one row, category `Cw`, event
  `owner_verdict`, and every field's value is asserted. Unmeasured figures come out null. The
  real writer serializes the row as one line with `"category":"cw"`. Both buttons are on the
  CW tab with their hover and command.
- Build: 0 errors, warnings as errors.
- App line: 278/278 at entry, then 277 and 272 of 278 at exit. Every lost type passed alone
  (section 1).
- Diffs: the transmit files and the `Cw` folder both print nothing.
- Commits: 9c84ad50 (task 0), 805840a4 (task 1), ba3d0ddc (task 2), cdf9edca (task 3), and
  the task 4 commit.

## 4. What's blocking us

`data\bands\mode-receiver-conditions.json` sets the CW receive condition's AGC to **fast**
(`"field": "agc"`, `"wanted": 1`, `"wantedText": "fast"`, `"confirmed": true`). The keying
meter's swing threshold, `CwKeyingThresholds.ConfidentSwingDb = 20`, was fitted to
recordings: the 2026-08-20 captures and seven repository recordings, per its own remarks at
`src\Hamlet.RadioEngine\Cw\CwKeyingMeter.cs:113-125`. Those recordings were taken under that
AGC FAST condition, which every one of the owner's sidecars shows (work instruction 474
section 2). This session did not open the sidecars, under R88.
