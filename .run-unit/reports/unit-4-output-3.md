```
READ IN THIS ORDER.

A. Hamlet reads a CQ call correctly. Steps 0 to 2 done; 3 partial with 3.4
   and 3.6 open; 4 not started; 5 the owner's; 6 partial with 6.5 open;
   7 partial with 7.1, 7.2, 7.4, 7.6 and 7.8 open.
B. Step 3, criterion 3.6: G1 - the out-of-order held gap reading refused -
   re-judged under 3.2's four tests as R71 and R73 now read them; kept or
   taken back out, which test decided it, the added letters and the 8
   single-element ones before and after; and, if nothing was kept, 3.4
   closing step 3 partial.
C. The rest, weighed against A and B. Section 4 raises 2 items; neither
   stands in the way of 3.6 as the rules now read, and item 1 is the one
   thing that would let G1 be judged again.
```

```
UNIT:       431 - complete at task 4 of 4, none dropped - 2026-09-25 03:40
PHASE GOAL: Hamlet reads a CQ call off the air as it was sent, judged by edit distance against keys and at the end by Tim at the radio.
UNIT GOAL:  Join the stray E and T that are pieces of a split letter back into the letter the sender keyed, kept only if 3.2's four tests pass.
ADVANCED:   no - G1 cut keyed edits 165 to 154 and brought back WB6 on 17:37, but failed tests 2 and 4 on 17:37 and 004133 and was taken back out, so 3.6 is unmet; 3.4 is ticked instead and step 3 closes partial
NUMBER:     keyed 165 -> 165 over 565, inferred keys; 17:37 19 -> 19 over its scored region; added letters 17 -> 17, single-element 8 -> 8
DRIFT:      0
```

## 1. What Claude did

**Complete at task 4 of 4, nothing dropped. No change is kept.** Local Windows machine, branch `main`. The gate's four checks confirmed Hamlet at `C:\Source\HamLet`. Every commit was pushed, with push rc 0 each time.

**Task 0, the record** (`75aaeae3`).
- `PHASE_OUTCOME.md` gained `## UNIT 431 - STEP 3` from the decision block, with the entry round.
- `PHASE_STATUS.md` names unit 431. The launcher had already written `CURRENT_STEP: 3`, and it stands.
- The version went from 1.13.117 to 1.13.118.
- Unit 430's section 4 items 1 to 4 are parked verbatim as P39 to P42.
- The launcher's uncommitted writes to `PHASE_OUTCOME.md`, `PHASE_STATUS.md`, `RUN_LEDGER.md` and `WORK_INSTRUCTIONS.md` went into this commit as they stood.

**Entry round**, one type per invocation. These are the numbers to hold:
- `Hamlet.sln` builds with warnings as errors, 0 errors.
- Engine carry-forward: 178 of 178 in 371 s.
- App carry-forward: 275 of 278 in 164 s. The three losses were `TheRecordNamesTheSubModePressedTests` (Olivia, FT4) and `TheChipSaysTheChosenModeTests` (FT8), all on "You've caused dispatcher loop" in 1 ms. Alone, the two types pass 12 of 12 and 6 of 6.
- Floors: captures 51 of 51, adjudicated 13 of 13, keyed floors 13 of 13.
- All keyed: **165 edits over 565**. The ten: 35 over 156 at the bench. Baseline: 22 over 46. Outside: 108 over 363.
- 17:37: **19 edits over its 25-character scored region**.
- `WhatTheStrayLettersRestOnTests` 2 of 2: **17 added, 8 single-element**.
- All output was identical to unit 430's exit, apart from timing lines.

**Section 5 against the tree.** Every item matched:
- `687aab1a`'s check sat after `word = wordBoundary * wordBoundary / character` and before the `return`.
- `MeasureGaps` has not changed between `687aab1a~1` and HEAD. The 57 added lines are a new method after it, `MeasureCharacterGap` (unit 415).
- So the check goes back at the same place, `CwUnitEstimator.cs` between lines 237 and 239, and the reading it refuses is computed exactly as before.
- The four test types exist. Both recordings carry a `.key.md` with a scored region.

**Task 1, which strays a split made** (`13f2e763`).
- I added `WhatTheStrayLettersRestOnTests.WhichStraysASplitMade`, a printer that asserts nothing.
- For each added letter, it reads the stream's `_structureHeld`, `_heldGaps` and `_troughRun` by reflection at the moment the letter settles, and prints the verdict: held, separated or textbook.
- Per keyed recording, it prints each run of characters read under an out-of-order held reading, with three characters either side.
- In task 2 (`dc4fd8b1`) it also gained one line per named character, with time and label. That is how each character that left is named below.

**Task 2, G1 again** (`856d226e`, taken out by `4701969d`).
- The check is `687aab1a`'s, unchanged, at the same place. Only the comment names this unit.
- Nothing else was touched: not the trough, the path boundary, the relabel, the gates, the bar, the tracker or the mixdown.
- **It failed test 2 on 17:37 and test 4 on `004133`. R73, read for a join as section 6 states, excused neither, so it was reverted.**
- **The narrower variant was not built.** It was allowed only if the first attempt failed on a single row or test. G1 failed on two rows and two tests.

**Task 3, 3.4** (`b6384763`).
- `P43 - step 3 closes partial` is in `docs/phase-correctness/PARKED.md`. It covers the three units (425, 428, 431), what each attempted and what stopped it, task 1's count, task 2's four tests as numbers, and the numbers left standing.
- 3.4 is ticked in `PHASE_PLAN.md`. Step 3 stays `partial` in `PHASE_STATUS.md`, with a note at the end of the line that it closed under 3.4 by unit 431 (P43).
- 3.6 is not ticked.

**Task 4, the exit round** (in this commit):
- The build is clean.
- Engine carry-forward: 178 of 178 in 370 s.
- App carry-forward: 277 of 278. The one loss was `TheCarrierHoldsTheButtonsTests.HisCardIsDrawnInTheSendingGreenWithTheWord` on the dispatcher loop in 1 ms, and the type passes 8 of 8 alone. It is not a regression.
- Floors: captures 51 of 51, adjudicated 13 of 13, keyed floors 13 of 13.
- Keyed totals: 165 over 565, with 17 added and 8 single-element.
- `WhatTheStrayLettersRestOnTests` 3 of 3.
- Keyed, baseline, adjudicated and captures output is identical to entry apart from timing lines.
- None of the eleven transmit files differs from `7e209cb4`. `src` and `data` show no diff against entry `242168fc`.

**Decisions made for myself, author's and overrulable:**
- **The "separated" verdict** in task 1 means the stream did not yet hold the sender's gaps, but the latest read had found them (`_troughRun > 0`).
- **The step 3 line in `PHASE_STATUS.md` keeps `partial` as its second field.** The closure note goes at the end, because the launcher parses that field.
- **Test 3 counts as a pass.** `032012`'s reading moved only by the space in `ARTICLES OR`, which is in its adjudicated text. The rest of its reading is unchanged. This is the unit 416 reading of R66.
- **On 17:37, the right `E` at 22.990 s counts as lost.** It became part of the right `B` settled at the same moment. Section 6 says a looser reading fails, so I did not count the join by the letters it left. The fall of 8 against an added fall of 7 fails on its own anyway.

## 2. What the owner should expect

Nothing changes on screen or on the air. The decoder is exactly as it was at entry.

Step 3 is now closed partial under 3.4. Units 425, 428 and 431 kept nothing, so the loop stops spending on 3.6 unless the owner reopens it. P43 holds the record.

**What will look wrong but is not:**
- `PHASE_PLAN.md` has 3.4 ticked and 3.6 unticked. That is how it should read: 3.4 is the criterion that closes a step without its work.
- The app carry-forward line drops one type to the dispatcher loop on most runs, and it is a different type each time. Each one passes alone.

## 3. What you should see

**3.2's four tests, G1 (`856d226e`) against entry.** The change was not kept.

| Test | Before | After | Result | Kept |
|---|---|---|---|---|
| 1. All keyed edits, inferred keys | 165 over 565 | 154 over 565 | pass | no |
| 2. Named floors from 2.2 | 13 of 13 | 12 of 13; 17:37 46 to 38 | **fail** | no |
| 3. Adjudicated readings | 13 of 13 | 13 of 13; `032012` moves onto its own text | pass | no |
| 4. Capture rows, above-bar named count | 51 of 51 | 50 of 51; `004133` 28 to 25 | **fail** | no |

Under G1:
- Added letters on the keyed recordings went from 17 to 10.
- Single-element added letters went from 8 to 2. The two that remained are the textbook `T`s on `031838` and `032050`.
- The ten on the bench went from 35 to 34 over 156.
- Baseline went from 22 to 13 over 46.
- Outside went from 108 to 107 over 363.

At exit, after the revert, every figure is back at entry.

**17:37 beside its key.** `WB6` came back under G1, and went out with it.

```
key:    CQ CQ CQ DE WB6RED WB6RED
entry:  CQ CQ CQ DEWTEETEEERE D ETTTB 7E E I      19 edits over 25
G1:     CQ CQ CQ DEWB6 RE D W B 7E E I            10 edits over 25
exit:   CQ CQ CQ DEWTEETEEERE D ETTTB 7E E I      19 edits over 25
```

**`032012`, the reading that moved** (test 3):

```
adjudicated:  N OF 117. LINKS TO ARTICLES OR OTHER WEBSITES MENTI
entry:          T F 117.1. LINKS TO ARTICLESOR OTHER WEBSITES MENTI
G1:             T F 117.1. LINKS TO ARTICLES OR OTHER WEBSITES MENTI
```

**Every character that left under G1, by recording.**

17:37, the whole fall inside its scored region. 11 left and 3 came in (`B`, `6`, `W`), for a net fall of 8. Added characters there fell from 8 to 1, which is 7.

| Settled at | Character | Key aligned it as |
|---|---|---|
| 22.540 s | `T` | added |
| 22.720 s | `E` | added |
| 22.990 s | `E` | **right** (became part of the right `B` at 22.990 s) |
| 23.460 s | `T` | wrong |
| 23.740 s | `E` | wrong |
| 24.020 s | `E` | wrong |
| 24.395 s | `E` | wrong |
| 26.250 s | `E` | added |
| 26.515 s | `T` | added |
| 26.820 s | `T` | added |
| 27.350 s | `T` | wrong |

`004133`, the whole fall **outside** every scored stretch. 7 left and 4 came in (`D`, `D`, `J`, `U`). One `D` is right, inside the `I C H ARD` stretch, which went from 4 edits to 3.

| Settled at | Character | Key aligned it as |
|---|---|---|
| 7.040 s | `T` | outside every stretch |
| 7.260 s | `E` | outside every stretch |
| 8.265 s | `T` | outside every stretch |
| 8.485 s | `E` | outside every stretch |
| 10.855 s | `E` | outside every stretch |
| 14.510 s | `K` | outside every stretch; became a placeholder |
| 19.195 s | `A` | outside every stretch; read `U` |

**Task 1's count: 5 of the 8 single-element added letters were made by a split.** Each was read under a held reading whose character gap stood at or past its word gap:
- `T` 22.540 s and `E` 22.720 s, under character 552 ms and word 295 ms;
- `E` 26.250 s, `T` 26.515 s and `T` 26.820 s, under character 828 ms and word 250 ms.

All five are on 17:37, and G1 removed all five. The other three:
- `E` at 17:37 21.170 s was read under a held reading in order. Under G1 the key aligned it as right.
- `T` on `031838` at 21.355 s was read under textbook gaps.
- `T` on `032050` at 11.195 s was read under textbook gaps.

The same kind of reading also covered 9 key-right characters on `032012` (`RTICLESOR`, which G1 corrected to `RTICLES OR`) and 5 unscored characters on `004133`.

## 4. What's blocking us

Nothing blocks the phase (R65). Two items are parked, most useful first.

**1. Whether a join may be judged by the letters it leaves.**
- **Proposed ruling:** the owner decides one of two things:
  - key `004133` from 7.0 to 11.0 s, where it reads `ARTETE■E`;
  - or rule that on a keyed recording, a join is judged by the key-aligned right characters before and after, rather than by each character removed.

  Either would let G1 be judged a third time, as a new unit. Without one, G1 stays out.
- **Reasoning:**
  - G1 is the only change on record that restores 17:37's `WB6`, cuts keyed edits by 11, and removes 6 of the 8 single-element strays.
  - It is held off by two things. One is an unscored stretch on `004133`. The other is one right `E` on 17:37 that became part of a right `B`, after which 17:37's right count rose from 13 to 17.
  - Section 6 set a strict reading and said a looser one fails, so this unit did not take one.
- **Rejected:**
  - Inventing a key for `004133`, which R61 forbids.
  - Building a narrower variant, which section 6 did not allow for a two-row failure.

**2. The app carry-forward line loses one to three types to the dispatcher loop on every run, a different set each time.** For the record.
- **Proposed ruling:** none. It is HM-OPEN-063's neighbor and already known.
- **Reasoning:** this unit saw it on both runs: three types at entry and one at exit, all green alone.
- **Rejected:** nothing.
