```
READ IN THIS ORDER.

A. Hamlet reads a CQ call correctly. Steps 0 to 2 done; 3 partial with 3.4
   and 3.6 open; 4 not started; 5 the owner's; 6 partial with 6.5 open;
   7 partial with 7.1, 7.2, 7.4, 7.6 and 7.8 open.
B. Step 7, criterion 7.4: the change against the mixdown pitch that 7.3
   named, judged under 3.2's four tests - kept or taken back out, which
   test decided it, and the opening's named characters before and after.
C. The rest, weighed against A and B. Section 4 raises 4 items; none stands
   in the way of 7.4, and item 1 is the next 7.4 unit's starting point.
```

```
UNIT:       430 - complete at task 3 of 3, none dropped - 2026-09-25 02:36
PHASE GOAL: Hamlet reads a CQ call off the air as it was sent, judged by edit distance against keys and at the end by Tim at the radio.
UNIT GOAL:  Stop the mixdown leaving the sender for a weak pitch the survey admits in his pause, so the opening of a session reads like the rest of it.
ADVANCED:   no - one change and one narrower variant built, both judged and both taken back out on the same floor, cw-2026-08-22-032113, so 7.4 is not ticked
NUMBER:     keyed 165 -> 165 over 565, inferred keys; opening stream 0 to 46.2 s 31 -> 31 named; added letters 17 -> 17
DRIFT:      1
```

## 1. What Claude did

**Complete at task 3 of 3, nothing dropped. No change is kept.** Local Windows machine, Hamlet confirmed at `C:\Source\HamLet` by the gate's four checks, branch `main`. Every commit pushed, push rc 0 each time.

**Task 0, the record** (`cacb8ec7`). `PHASE_OUTCOME.md` gained `## UNIT 430 - STEP 7` from the decision block, with the entry round. `PHASE_STATUS.md` now names unit 430 and `CURRENT_STEP: 7`; the launcher had written `CURRENT_STEP: 3` there. The version went from 1.13.116 to 1.13.117. Unit 429's items 3 and 4 are parked verbatim as P37 and P38. The launcher's uncommitted writes to `PHASE_OUTCOME.md`, `PHASE_STATUS.md` and `RUN_LEDGER.md` went into this commit as they stood.

**Entry round**, one type per invocation:
- the build: 0 errors
- engine carry-forward: 178 of 178 in 375 s
- app carry-forward: 277 of 278. The one loss was `TheFavoritesAreChipsTests` on the dispatcher loop, and that type ran 4 of 4 alone.
- captures: 51 of 51 in 120 s
- adjudicated: 13 of 13
- keyed floors: 13 of 13
- all keyed: 165 edits over 565
- added letters: 17, of which 8 single-element
- `WhatTheOpeningHeardTests`: 4 of 4 in 141 s

Every output was identical to unit 429's exit apart from timing lines.

**Section 5 against the tree:**
- `CwDecoder.cs:600` to 603 and 617 to 621 match the instruction.
- The two recordings overlap by 13.800 s, and 662400 of 662400 samples are identical.
- The totals match.
- **One mismatch:** no test method named `ASignalAtTheWrongPitchIsStillFound` exists. The name appears only in comments. Its adjudicator is `CwAdjudicationTests.ASignalOffTheExpectedPitchIsFoundInRealisticAudio`, and I ran that type and `CwDisplacementFloorTests`, which also cites the name.

**Task 1, why the mix moved** (`d3fb3e82`). I added `WhatTheOpeningHeardTests.WhyTheMixMoved`, a theory that asserts nothing. For every survey read, it prints:
- what the survey admitted, bin by bin;
- which branch of `CwToneTracker.ReadSurvey` acted, with file and line;
- `HasMeasuredPitch`, `_lastMeasuredToneHz`, and the pitch written at `CwDecoder.cs:617`.

It reads seven private tracker fields and one decoder field by reflection and writes none of them. It also calls the tracker's private `_survey.Analyze()` again after each read. Like the public `CoarseCandidates()`, that call recomputes the band beside each bin and decides nothing.

It runs on four stretches:
- the stream, 28 to 38 s;
- the locked stretch warm, stream 90 to 106.2 s;
- the same stretch cold, `004108` 13.8 to 30 s;
- beyond the list, as the author's call: `003919` 11.8 to 21.8 s cold.

I kept the cold control, the drop candidate, because it costs seconds. Task 1's paragraph is in section 3.

**Task 2, the change** (`5b6b704c`). The change is in `CwDecoder.Step`, where `_lastMeasuredToneHz` is taken, feeding lines 617 to 621:
- The first measured pitch is still taken at once.
- A move of up to 25 Hz is also taken at once. 25 Hz is one coarse bin, the distance the tracker's own `Switch` calls refining.
- A move further than that is held as pending. The mix stays on its old pitch until a later survey's verdict carries confirmed keying within 25 Hz of the new one.

It does not touch the tracker, the survey's scoring, the speed search, the gap structure, the gates, `CharacterMargin` or `StrayElementSpan`.

**Result: fails tests 2 and 4 on one recording, and was taken back out** (`a7e6e2f2`). `cw-2026-08-22-032113` fell from 47 named at or above the bar to 44. Its scored region was unchanged, so R73 does not excuse it.

**The narrower variant** (`ec76051e`). The trace showed that `032113`'s two tracker moves, to 650 Hz and to 500 Hz, were both held until a character ended. Each was confirmed on a read before the move was made. So the variant follows a move at once when the verdict of the read just before it already carried the keying, and still waits after a `Switch` made on the very read that confirmed it.

**Result: fails tests 2 and 4 on the same recording** (44 named, and above-bar elements 102 to 100). **Taken back out** (`fa64edc5`). No third attempt.

It failed because it looked back one read, and the hold before the 650 Hz move spanned two:
- confirmed at 25.54 s;
- still held at 26.04 s, with nothing admitted;
- made at 26.54 s.

The variant followed the 500 Hz move at 29.54 s. It did not follow the 650 Hz one, and the mix stayed at 600 from 26.54 to 29.54 s.

**Task 3, the exit round** (in this commit):
- `Hamlet.sln`, non-incremental with warnings as errors: 0 warnings, 0 errors.
- engine carry-forward: 178 of 178 in 371 s.
- app carry-forward: 275 of 278. All three losses were `TheFavoritesAreChipsTests` on the dispatcher loop, and that type ran 4 of 4 alone.
- captures: 51 of 51, every row as at entry.
- adjudicated: 13 of 13.
- keyed floors: 13 of 13.
- all keyed: 165 over 565. Added letters 17, 8 single-element.
- `WhatTheOpeningHeardTests`: 9 of 9 in 162 s.

Keyed, baseline, adjudicated, captures, floors and the stray trace are identical to entry apart from timing lines. Nothing under `src` or `data` differs from entry `003f2c98`, and none of the eleven transmit files differs from `7e209cb4`. 7.4 is not ticked.

**The pitch types, one per invocation, at entry source, under each change, and at exit.** Every result was the same in all four rounds:

| Type | Result |
|---|---|
| `ContactTrackerTests` | 14 of 14 |
| `CwSurveyThresholdPinTests` | 3 of 3 |
| `CwToneSurveyTests` | 5 of 5 |
| `CwTrackerSwitchTests` | 2 of 2 |
| `ThePitchCanBeHeldTests` | 5 of 5 |
| `TheSurveyAlreadyUsesAShortWindowTests` | 2 of 2 |
| `TheTrackerSwitchTraceTests` | 10 of 10 |
| `TheTwoPitchesTableTests` | 2 of 2 |
| `CwAdjudicationTests` | 11 of 11 |
| `CwDisplacementFloorTests` | 6 of 6 |
| `AHeldPitchDoesNotOutliveItsEvidenceTests` | **1 of 4, the same three red at entry as with either change in** (section 4) |
| `EveryElementCarriesItsOwnPitchTests` | 0 tests: `Compile Remove`d in the test project |
| `ThePeakFindsThePitchTheTrackerMissedTests` | 0 tests: `Compile Remove`d in the test project |

The instruction had no entry run for these, so I took the baseline after the first revert, on source identical to entry.

**Decisions made for myself, author's and overrulable:**
- The 25 Hz same-station distance.
- Telling a later survey read from the one that made a move by comparing the tracker's `Verdict`. Every read writes one.
- Two stretches added to the printer beyond the list: `003919` cold, and `032113`, added in task 2 to find why the change cost it.
- Running the whole judging list on both attempts even though each failed test 2 first, so their numbers are on record.
- Counting `ContactTrackerTests` among the pitch types because "tracker" is in its name.
- The opening's "named" count: any settled character except a space or the placeholder.

## 2. What the owner should expect

Nothing the operator sees has changed. The decoder's source is byte-identical to the start of the unit. The opening of the 7.052 session still reads `UIEH EE E E T I NIEEE E` on the spliced stream, and this unit does not claim otherwise.

What is now true: the project knows why the mixdown walked off the sender, and has measured a repair that works on the opening. With either attempt in, the stream's opening read `EANQNID EAN■IK`, the group the cold decode reads, instead of the litter. It also lowered the keyed total from 165 to 149 edits (153 under the variant). It was taken out because it cost one other recording three characters outside its scored stretch. The floors forbid that, and they did their job.

**What will look wrong but is not:**
- The opening's named count falls when the repair is in, 31 to 21, because the litter stops being named. Here, fewer named characters is the better reading.
- `AHeldPitchDoesNotOutliveItsEvidenceTests` is red (1 of 4). It was red before this unit touched anything.

## 3. What you should see

**No visible change.** Both attempts were taken back out. What follows is what they did while they were in.

**3.2's four tests.** First change `5b6b704c`, narrower variant `ec76051e`. Before is the entry.

| Test | Before | First change | Pass | Variant | Pass |
|---|---|---|---|---|---|
| 1. keyed total, inferred keys, must not rise | 165 over 565 | 149 over 565 | pass | 153 over 565 | pass |
| 2. keyed floors, 2.2 | 13 of 13 | 12 of 13, `032113` 47 to 44 | **FAIL** | 12 of 13, `032113` 47 to 44 | **FAIL** |
| 3. adjudicated readings | 13 of 13 | 13 of 13, all unchanged | pass | 13 of 13, all unchanged | pass |
| 4. capture rows' above-bar counts, R71 | 51 rows | `032113` 47 to 44, outside its scored stretch | **FAIL** | `032113` 47 to 44, elements 102 to 100 | **FAIL** |
| **Kept** | | **no, reverted `a7e6e2f2`** | | **no, reverted `fa64edc5`** | |

Under both attempts, one row rose: `004234` went from 36 to 37 named. Under the first change, `031905`'s above-bar elements went from 108 to 115.

**The added letters, for 3.6 and not claimed for it.** They went from 17 to 14 under both attempts:
- `031905`: 2 to 0, and its right letters rose from 20 to 30 under the first change, 27 under the variant;
- `cw-2026-08-18-004507`: one added became one wrong.

The 8 single-element added letters did not move. They sit in stretches the mixdown change does not reach.

**The opening's text.** Entry and exit are identical. "With a change in" is the same under both attempts.

| Where | Entry and exit | With a change in |
|---|---|---|
| stream 0 to 30 s, `003901` | `EII E T NHHK`, 9 | `EII E T NHHK`, 9 |
| stream 30 to 46.2 s, `003919` | `UIEH EE E E T I  NIEEE E  E ET N ■IK`, 22 | `EANQNID  EAN■IK`, 12 |
| **stream 0 to 46.2 s** | **31 named** | **21 named** |
| `003901` cold, whole file | `EII E T NHHK`, 9 | unchanged, 9 |
| `003919` cold, whole file | `EITEETNXNIK  EANQNID  EANQNIK`, 25 | unchanged, 25 |
| live sidecar, `003901` whole | `E ET E E   E  E E  E  E E  E    E A TE E T N QNIK     EE`, 25 | - |
| live sidecar, `003919`'s addition | `EESIH S E E TEIE E RIEEE`, 18 | - |

With a change in, the stream read the sender's group where the live decoder and the unchanged stream read `E`, `I` and `T`. That is the difference 7.4 asks for, but a change is kept only on the four tests.

**Task 1: the rule that moved the mix.** The move to 525 Hz at stream 30.54 s was the tracker obeying its own rules. It happened in the sender's pause: the cold decode puts his `K` at stream 24.7 s and his next `E` at 30.6 s.

1. The survey admitted 525 Hz, and nothing else, on two consecutive reads, 30.04 and 30.54 s. Its figures there: dit 78 and 84 ms, dah 280 ms, 18 marks, lift 2.4 and 2.5 dB, keyed level -41 dB, about 15 wpm.
2. The twice rule at `CwToneTracker.cs:1044` confirmed it.
3. HM-DEC-127's floor at 1077 did not refuse it. The candidate stood 12 dB under the -29 dB station being read, inside the 25 dB rejection.
4. `Switch` at 1107 moved the bank. That is the choice by keying HM-DEC-095 governs.
5. `CwDecoder.cs:600` to 603 took the new measured pitch in the same hop, and 617 wrote 525 into the mix.

The survey never admitted 525 again. It admitted 600 and 625 once each (32.54 and 33.54 s), and confirmed 625 at 34.04 s. That move was held mid-character (1097 to 1104) until 36.04 s, because the decoder reading at 525 was always inside a character. So the mix sat 100 Hz off the sender for 5.5 s.

**HM-DEC-095 governs the move and HM-DEC-127 does not bear on it.** Changing the tracker's choice would have meant ranking by loudness, or lowering HM-DEC-127's floor. So the line to touch is the mix's use of the tracker's figure, `CwDecoder.cs:600` to 603 feeding 617 to 621, not the tracker's choice.

The cold controls agree:
- `003919` cold never measured a pitch in 11.8 to 21.8 s, and mixed at the bank's 600 throughout.
- `004108`, warm and cold, locked on 625 with moves no larger than one bin, apart from one confirmed `Switch` from 625 to 650 cold at 23.04 s.

## 4. What's blocking us

Nothing blocks the phase (R65). Four items are parked, most useful first.

**1. What the second 7.4 unit builds.**
- **Proposed ruling:** the next 7.4 unit builds the variant's intent correctly. The mixdown follows at once any move that a survey read has confirmed since the mix's own pitch was last set, not only one confirmed on the read just before the move, and still waits after a `Switch` made on the read that confirmed it. It is judged on the same four tests, with `032113` watched first.
- **Reasoning:** both attempts cured the opening and lowered the keyed total. Both failed only on `032113`. The measured reason for the variant's failure is a hold that spanned two reads, which that rule covers. This unit could not build it: the instruction allows no third attempt.
- **Rejected:**
  - Moving the tracker's choice, because the trace shows HM-DEC-095 governing the move.
  - Excusing `032113` as outside the scored stretch, because R73 excuses only key-aligned added characters inside one.

**2. `AHeldPitchDoesNotOutliveItsEvidenceTests` is red at entry, 1 of 4.**
- **Proposed ruling:** open an issue and have a later unit find when `CwDecoder.Retuned()` stopped releasing on a QSY. The unit that finds it does not repair it without a ruling.
- **Reasoning:**
  - `Retuned()` is now only `Unlock()`.
  - The three red tests expect a move of the dial to release, in their words, the pitch measured before it, the held peak, and the speed.
  - The type is not on the carry-forward list, so no unit's round would have caught it.
  - This is the 2026-08-26 fault the file describes, a decoder still pointed at a pitch measured on another frequency.
- **Rejected:** repairing it here, which section 10 forbids.

**3. The instruction's named test.** For the record.
- **Proposed ruling:** future instructions name `CwAdjudicationTests.ASignalOffTheExpectedPitchIsFoundInRealisticAudio`.
- **Reasoning:** `ASignalAtTheWrongPitchIsStillFound` exists in the tree only in comments. Two pitch-named files, `EveryElementCarriesItsOwnPitchTests` and `ThePeakFindsThePitchTheTrackerMissedTests`, are `Compile Remove`d, so a "run every pitch type" line runs 0 tests on them.
- **Rejected:** nothing.

**4. The opening's figure.** For the record.
- **Proposed ruling:** 7.4's figure is the stream opening's text set beside the cold group `EANQNID`, and not its named count alone.
- **Reasoning:** a repair that removes litter lowers the named count, 31 to 21 here.
- **Rejected:** scoring the opening against a key, which no recording in the tree carries (R72 rules out inventing one).
