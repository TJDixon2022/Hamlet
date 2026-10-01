```
UNIT: 515 - complete - 2026-09-30
UNIT GOAL: the shape is found wherever it appears; the watched bin retires
NUMBER: pitches read whole through a 500 Hz filter on 600, nothing pointed: 5 of 5 (was 4 of 5; 700 Hz read nothing)
```

## 1. What Claude did

Claude Code on the development machine, branch `main`. The prompt claimed `PROJECT: Hamlet`; the order's gate held: `SHACK_FACTS.md`, `CwRunReader.cs` and `CW_REQUIREMENTS.md` exist, there is no `CoreHMI.sln` or `MURC.sln`, the root is `C:\Source\HamLet`, and `PROJECT_CARD.md` says Hamlet. Nothing in this report is evidence about the radio. Unit 515 and HM-DEC-219 were free.

SESSION.lock was taken through `tools\arbiter\lock.bat take` and released at the end. Nothing was written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` was touched, no box was ticked, and R114 was appended to both plans in the owner's words. Nothing keys, transmits or writes to the radio. Scratch probes are under `.run-unit\` and not committed. The change is commit `7d27cca3`.

**`src/Hamlet.RadioEngine/Cw/CwEnvelopeDetector.cs`.**
- **The verdict.** `Keying` is true when a sequence the pattern gate stands has had a mark within that sequence's own hold. `PitchHz` is that sequence's pitch, on the bin grid, the loudest where several stand.
- **A mark that stands is keyed.** Before, it was keyed only where the bins round its peak paired their bars. Through the filter they didn't at 700 Hz, so 65 marks stood, none was keyed, and the reader printed nothing.
- **Gone:** `Follow`, `PointAt`, `WatchedHz`, `PointedHz`, the followed and pointed bins, the old held-station logic (`_holdBin` and its kin), and the 496 "station's own bin" choice for the verdict.
- **Kept:** every bin's bar and pairing evaluation, `CallMarks`, and the 496 peak walk that puts a mark on its lobe's peak, which is attribution, not pointing.
- **The scope's bin** is the one nearest the standing pitch: derived from it, never steered, left where it was when nothing stands. `History()` marks a hop where a mark that stood covers it, carrying that mark's shape score; it no longer uses the bin's own paired bars, which the station's exact bin forms rarely (unit 496).
- **`MarksLast4s`** counts the marks that stood in four seconds: at the standing pitch while one stands, at any pitch otherwise.
- **`CwEnvelopeReading`** loses `Pointed`.

**`src/Hamlet.RadioEngine/Cw/CwPatternGate.cs`.** Read-only additions; the gate's rules are unchanged.
- `Standing(now, hold)` lists the standing sequences.
- Each sequence has `PitchHz`, `LevelDb` (leaving out unit 511's quieter marks) and `HoldSeconds(hold)`.
- **The hold is the sender's own:** the longer of the detector's one-second hold and the sequence's longest gap between recent marks, plus its longest mark. A mark reaches the gate only once it has ended, so a 9 WPM word gap and dah is 1.33 s.
- How I got there:
  - a one-second hold dropped keying mid-call at the first word gap;
  - a two-second hold lingered past the test's limit after the call;
  - a hold of the longest gap seen plus the longest mark still dropped at the first word gap, before any word gap had been seen.

**`src/Hamlet.App/ViewModels/MainWindowViewModel.cs`.**
- The scope tick no longer calls `PointAt` or `Follow`, and unit 514's `FollowPitch` is gone.
- `Tracker.FollowScope` and `Tracker.FollowMeter` are no longer called.
- The radio's scope pointer is still observed, for `scopePeakHz`, `scopePeakLevel` and `scopeFramesLast4s` on the row and the sheet.
- "scope quiet, sweeping" no longer leads the tone line, since nothing sweeps.

**`src/Hamlet.App/ViewModels/CwHearingViewModel.cs`.** The verdict row drops `trackerHz`, `trackerHasPitch` and `trackerHasKeying`. It keeps `scopePitchHz`, `scopeMarksLast4s` and `mixingHz`.

**Tests.**
- New: `Cw/TheShapeIsFoundWhereverItAppearsTests.cs`, with the five pitches through the filter, the drifting station, and the two stations.
- Rewritten because they drove the retired pointing, each saying why in its own text:
  - `TheDetectorFollowsTheMeterTests`: unit 514's two cases are retired. It now proves the meter does not steer the detector.
  - `TheRadioPointsTheDetectorTests`: keeps the scope pointer's own peak and quiet; the detector-pointing assertions are gone.
  - `ThePatternIsTheGateTests.TheRowDescribesTheSenderBeingPrinted`: no `Follow`. The reading's pitch equals the printed pitch on its own, which is case 5.
  - `TheScrollKeepsItsBlocksTests`: no `Follow`.
  - `NoDetectionNoLettersTests` and `PrintedStaysPrintedTests`: `WatchedHz` becomes the reading's pitch.
  - `WhichGateTurnsAwayW1awTests`: its "pointed" rows are no longer pointed.
- The row key-set tests (`TheOwnersVerdictIsARowTests`, `TheVerdictCarriesTheScopeTests`, `TheOwnersPressLandsInTheFileTests`) drop the tracker fields and name `mixingHz` at last. That fixes the seven reds they had carried since unit 488.
- **The scope-row and picture tests keyed too little to stand.** `TheVerdictCarriesTheScopeTests`, `TheScopeIsTheMiddlePictureTests` and `TheBarsCarryTheirLettersTests` keyed a single C, or C and part of a Q, which never stands under the five-mark pattern rule. They now key enough to stand: a C, Q and C; a Q prepended, with every examined time shifted by 0.96 s; a Q 3.5 s before the C. The assertions carry the new meaning: the gap keeps the standing pitch, and the mark counts are the marks that stood.

**What the meter and the tracker still touch, for unit 512.**

| Piece | What still constructs it | What still reads it |
|---|---|---|
| `CwKeyingMeter` | `MainWindowViewModel` (line ~11589) | `PublishKeying`: the keying word; the verdict row's `meter*` fields; the capture sheet's `KeyingLine` and `KeyingRecordLine` |
| `CwToneTracker` | `CwDecoder`'s constructor, which runs it in its hop loop | the decode report's pitch and proof (the sheet); `AutoCallViewModel` (`Tracker.Follows`); `MainWindowViewModel`'s `CoarseCandidates()` for the hearing state; `CwDecoder.DetectorPitch` hands it the detector's pitch |
| — | — | `FollowMeter` and `FollowScope` stay on the tracker, called by nothing in the app; `CwHearingState` still carries `TrackerHz`, `TrackerHasPitch` and `TrackerHasKeying`, which no row reads |

**Build and tests.**
- Build `Hamlet.sln` with warnings as errors: RC=0.
- App carry-forward line: 276 of 278. The two losses, `TheFavoritesAreChipsTests` and `BindingHealthTests` in 1 ms to *"You've caused dispatcher loop"*, pass alone, 4 of 4 and 1 of 1.
- Engine reading set: 76 of 80.
- App scope, scroll, row and reading cases: green.

**Records.**
- Version 1.13.200 to 1.13.201.
- `PHASE_OUTCOME.md` (both copies): `## UNIT 515 - STEP 12`.
- `PHASE_STATUS.md` (both copies) names 515.
- R114 appended to both plans, with no checkbox touched.
- `CLAUDE.md` §1 index row.
- `DECISIONS.md` HM-DEC-219, in full:

> **The shape is found wherever it appears; the watched bin retires.** Tim, 2026-09-30, R114: *"I'm wondering why we're so focused on pitch. Pitch almost doesn't matter. It's shape. If you can identify height, flat top, period, then you know it's a dot or a dash. The pitch doesn't matter."* And: *"You keep talking about 350, 400, 500, 600. Those are pitches. I just care about shape."*
>
> **What it ends.** Since unit 476 the detector kept one watched bin for its keying verdict, its light, its blocks and its mark count, and a chain of rules chose which: 476's survey, 496's station's own bin for the verdict, 507's follow-the-reader, 514's follow-the-meter. Every pointing fault of the week was that bin being where the station was not. All four rules are superseded; the radio's scope peak is still read for the row and the sheet, and points nothing.
>
> **What is built.** The verdict is a sequence the pattern gate stands, at any pitch, with a mark within the sender's own hold - the longer of the detector's one-second hold and its own longest gap, plus its longest mark, since a mark reaches the gate only once it has ended. Its pitch is that sequence's, the loudest where several stand. A mark that stands is keyed. The scope's bin is the one nearest the standing pitch, derived and never steered, and its hops are marked by the marks that stood. The meter and the tracker steer nothing on the screen's path; the verdict row drops trackerHz, trackerHasPitch and trackerHasKeying. The 496 peak walk that puts a mark on its lobe's peak stays.
>
> **What it showed.** Through a 500 Hz filter on 600 with nothing pointed, 700 Hz had stood 65 marks and keyed none, so nothing printed; now 425 to 775 Hz all read whole with their pitch named, and a station drifting from 500 to 560 Hz is followed by its shape. Every synthetic reading is as before, and noise prints nothing. A test of the timing decoder's gate, which the app has not used since unit 493, now opens on a noise sequence that stands and is left red.

## 2. What the owner should expect

- **Rebuild.**
- **A station at any pitch inside the filter is found the moment it keys.** The shape is looked for in every bin at once, so nothing has to be pointed at it or catch up with it. On the bench, through a stand-in for your 500 Hz filter on 600, the call reads whole at 425, 500, 600, 700 and 775 Hz. A station drifting from 500 to 560 Hz reads whole, with the pitch shown rising with it.
- **The light, the blocks and the row describe whatever is standing.** The tone line names the standing station's pitch. Keying holds through that sender's own gaps and lets go about a second after its last mark, longer for a slow sender.
- **Nothing about how letters are read changed.** Every synthetic call reads exactly as before, and noise still prints nothing.
- **If a station still reads nothing,** report it against the pitch table in section 3.
- **One test is newly red, deliberately.** `NoDetectionNoLettersTests.ALetterReadFromNoiseDoesNotReachTheScreen(blocks: True)` drives the old timing decoder's gate, which the app hasn't used since unit 493. Under the new verdict, a noise sequence that stands opens that gate. The reader on the screen's path still prints nothing on noise.
- **Still red, as before:** unit 507's three strength cases, the decision-log index gaps, and unit 450's two "centre"s. The seven verdict-row reds since unit 488 are fixed.

## 3. What you should see

**The pitch table.** The call at 20 WPM, 24 dB, through two band-pass sections at 600 Hz, Q 1.2, with the detector told the passband and nothing pointed or followed.

| Pitch | HEAD: keyed / reads | Now: keyed / pitch named / reads |
|---|---|---|
| 425 Hz | 28 of 65 / whole | 65 of 65 / 425 Hz / `CQ CQ DE N0CALL N0CALL K` |
| 500 Hz | 59 of 65 / whole | 65 / 500 Hz / whole |
| 600 Hz | 50 of 65 / whole | 65 / 600 Hz / whole |
| **700 Hz** | **0 of 65 / nothing** | **65 / 700 Hz / whole** |
| 775 Hz | 49 of 65 / whole | 65 / 775 Hz / whole |

**The drifting station,** 500 to 560 Hz over the call, through the same filter: 65 stood, 65 keyed. The pitch named over the first quarter averaged 512 Hz and over the last quarter 550 Hz, and the call reads whole. At HEAD it also read whole, with 16 keyed.

**Two stations,** 625 Hz at 24 dB and 825 Hz at 10 dB: the loud one reads whole (65 stood) and the quiet one stands (12), as at HEAD.

**The verdict row on a driven station:** while the reader prints and the bars say keying, `scopePitchHz` is the printed pitch on every reading, with nothing followed. The key set has no tracker fields.

**The existing cases.** Every printed reading of the eight synthetic reader classes is identical to HEAD:
- the calls at every speed, both Farnsworth cases and the speed change;
- the fists;
- the bursts, the hesitation, `TEST DE W1AW K`, `DE DE`, the lone and stray marks;
- the strength table: 16 and 24 dB whole; 8 dB `N ET A EI A DE N0CALL NTJCE AEL K`; 12 dB `CT A CQ DE N0CALL N0CALL K`;
- unit 511's quiet dit and dah, whole.

Both noise tests print nothing (30 s: 0 stood; 180 s: 80 stood). In `WhichGateTurnsAwayW1aw`, only its diagnostic counts moved: keying hops, and "pointed 600" now counting 18 marks rather than 0.

## 4. What's blocking us

Nothing blocks. The items:

1. **The old timing-path noise test is red.** `ALetterReadFromNoiseDoesNotReachTheScreen(blocks: True)` exercises a gate the app has not used since 493. Unit 512's cleanup should retire it with the timing decoder, or a ruling should say the verdict must stay closed on noise sequences that stand.
2. **For 512:** the meter and tracker table in section 1. Neither steers anything on the screen's path any more.
3. **The filter in the tests is a stand-in** (two band-pass sections), not the IC-7300's own shape. Your station at the radio is the test.
4. **Unit 512 is still unrun.** This unit numbered past it as the order said; the next order should be 516 or later, with ruling id HM-DEC-220 or later.

### Asks still outstanding

- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25, waiting on
  the owner; no change sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8
  record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, so nothing on it is ever
  revised, at the cost of seconds of lag. On the run path, now the only path to the screen, the
  terminal shows only settled text. The ask stands only for the timing-only path, which no longer
  reaches the screen; no change for it sits in the tree.
