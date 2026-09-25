READ IN THIS ORDER.

A. Hamlet reads a CQ call correctly. Steps 0 to 2 done; 3 partial and
   closed under 3.4 with 3.6 open; 4 not started; 5 the owner's; 6 partial
   with 6.5 open; 7 partial with 7.1, 7.2, 7.4, 7.6 and 7.8 open.
B. Step 7, criterion 7.4: the mixdown follows a move once any survey read
   since it was set has confirmed it (P39). The replay's three answers were
   yes, yes and no. The third failed on 031905, so nothing was built and
   nothing was kept or taken back out. 032113 stays at 47 above the bar.
   The opening is unchanged beside EANQNID, and 7.4 is not ticked.
C. This report adds one finding that bears on B. On these three stretches,
   P39 differs from the entry only in the opening. The gate stopped it
   because it differs from 5b6b704c on 031905, and it matches the entry
   there. Section 4 raises 2 items. Item 1 decides whether 7.4 can move
   again by this route. Nothing halts the phase.

```
UNIT:       432 - complete at task 3 of 3, none dropped - 2026-09-25 04:42
PHASE GOAL: Hamlet reads a CW CQ call as the text that was sent, measured against keys, and Tim confirms it at the radio
UNIT GOAL:  stop the mixdown following the tracker off the sender in the 7.052 opening, without costing 032113 its letters, using P39's release rule, gated on a replay, kept only under 3.2's four tests
ADVANCED:   no - the replay gate answered no on its third condition, 031905, so no rule was built and 7.4 stays open
NUMBER:     keyed 165 -> 165 over 565, inferred keys; 032113 above-bar 47 -> 47; opening stream 30 to 46.2 s 22 -> 22 named, UIEH EE E E T I NIEEE E E ET N ■IK -> unchanged
DRIFT:      1
```

## 1. What Claude did

**Complete, at task 3 of 3, with none dropped.** Task 2 built nothing. That was the gate's instruction and not a drop: section 6 says to build nothing if any replay answer is no, and to go to task 3. The session ran on QUIVERFULL, in the Hamlet project on main, and the gate passed. Entry HEAD was `cec344ad`.

**Task 0, the record (`134b715e`).**
- Bumped the version from 1.13.118 to 1.13.119.
- `PHASE_STATUS.md` now names unit 432 and CURRENT_STEP 7. The launcher had left CURRENT_STEP at 3.
- Added `## UNIT 432 - STEP 7` to `PHASE_OUTCOME.md`, with an ENTRY line.
- Parked unit 431's items 1 and 2 verbatim as P44 and P45, and marked P39 as taken up.
- Committed the launcher's four root files as they stood.

**Section 5 against the tree.** All of it holds except one small point, noted and not repaired:
- `CwDecoder.cs` lines 600 to 603 and 617 to 621 are as stated.
- The four commits exist as unit 430 describes them.
- `5b6b704c` put its pending move inside the `if (_tracker.HasMeasuredPitch)` block, with three fields and `SameStationHz` after `_lastMeasuredToneHz`. It re-applies cleanly: `git apply --check` passes.
- `CwDecoder.cs` is identical to `a7e6e2f2`.
- The 032113 row is 47 named, 102 elements, with a keyed floor of 47.
- The opening's text is as stated.
- The small point: `WhyTheMixMoved` prints the verdict recomputed by `CwToneSurvey.Analyze`, not the `Tracker.Verdict` property itself.

**Entry round, one type per invocation.**

| Check | Result |
|---|---|
| Build, warnings as errors | 0 errors |
| Engine carry-forward | 178 of 178, 372 s |
| App carry-forward | 276 of 278. The two dispatcher-loop losses, `TheRecordNamesTheSubModePressedTests` (Olivia) and `TheWindowHoldsBelowItsMinimumTests`, pass alone at 12 of 12 and 3 of 3 (P45) |
| Captures | 51 of 51 |
| Adjudicated | 13 of 13 |
| Keyed floors | 13 of 13 |
| All keyed | 165 over 565 |
| 17:37 | 19 over 25 |
| Added letters | 17, 8 of them single-element |
| `WhatTheOpeningHeardTests` | 9 of 9 |
| Pitch types | `AHeldPitchDoesNotOutliveItsEvidenceTests` 1 of 4 (P40). `EveryElementCarriesItsOwnPitchTests` and `ThePeakFindsThePitchTheTrackerMissedTests` run 0 (P41). The other ten are all green. |

Apart from timing lines, every output matched unit 431's exit.

**Task 1, the replay (`f0845a92`).** Added `WhatTheOpeningHeardTests.WhenEachRuleFollows`. It asserts nothing and writes nothing.
- It drives the decoder a hop at a time, as `WhyTheMixMoved` does. After each hop it keeps the tracker's pitch, whether that pitch is measured, its `Verdict`, and whether the hop read the survey.
- It then runs four rules over that one record: the entry, `5b6b704c`, `ec76051e` and P39.
- The entry rule reproduces the entry decoder's mix on every measured hop of all three runs, with 0 differences.

**A decision I made for myself, reproduced in full.** P39 says "since the mix's pitch was last set", which needed a meaning in code. I took it as the last time the mix's pitch value changed. I fixed that before running the replay:
- The literal assignment runs on every hop, which would give a look-back of zero reads.
- The rule does not count the read that made the move. That is P39's clause about a `Switch` made on the read that confirmed it.
- Like `5b6b704c`, the rule tells a new read from the last by its verdict. On the stream, 57 reads have a verdict equal to the read before; nearly all of them are empty. A read that repeats the previous keyed pitch adds nothing to P39's look-back, so this cannot change P39's answer.

**Task 2 built nothing** because the gate's third answer was no. `src` is unchanged.

**Task 3, the exit round.** Every run is identical to entry apart from timing:

| Check | Result |
|---|---|
| Build | 0 errors |
| Engine carry-forward | 178 of 178, 370 s |
| App carry-forward | 278 of 278, no dispatcher-loop loss this time |
| Captures | 51 of 51, every row the same as entry |
| Adjudicated | 13 of 13 |
| Keyed floors | 13 of 13 |
| Keyed totals | 165 over 565 |
| Added letters | 17, 8 of them single-element |
| `WhatTheOpeningHeardTests` | 12 of 12 |

Against the tree:
- The eleven transmit files are unchanged from `7e209cb4`.
- `src` and `data` are unchanged from `cec344ad`.

## 2. What the owner should expect

**Nothing changes in the app.** The decoder is byte for byte what it was at entry. The 7.052 opening still reads `UIEH EE E E T I NIEEE E` on the spliced stream.

**One addition that looks like work but decides nothing.** `WhatTheOpeningHeardTests` now has 12 members. The new printer, `WhenEachRuleFollows`, adds about 10 s to that type.

**The app line lost nothing to the dispatcher loop at exit.** That is luck, not a fix (P45).

**7.4's count.** This is the second 7.4 unit with no kept change, after 430.

## 3. What you should see

No visible change. This unit measured a rule before building it, and the measurement stopped it.

**The gate's three answers**, with the print lines from `.run-unit/unit432-replay-t1.txt`:

1. **Does P39 follow 032113's 650 Hz and 500 Hz moves when the tracker made them? Yes.**
   - **650 Hz:** `replay follows | 26.54 | 26.54 | never | never | 26.54`. The columns are the entry, `5b6b704c`, `ec76051e` and P39. The move was released by `replay read | 25.54 | 650.0 | yes`, a read after the mix was set at 21.04.
   - **500 Hz:** the entry tracker never makes this move. It is still held when the 30 s file ends: `survey | 29.54 | nothing admitted; the tracker stays`, held move 500.0. The tracker made the move only under unit 430's decoders, at 29.54, after `survey | 29.04 | 500.0 confirmed outside the bank's reach of 650.0; held`. P39 counts that read, and `ec76051e` followed the move there at 29.54. P39 looks back over every read `ec76051e` does, and more.
   - **This half of the answer is argued from unit 430's trace, not replayed**, because the move is not on the entry record.
2. **Does P39 hold the opening's 525 Hz move? Yes.**
   - The move: `replay move | 30.54 | 600.0 -> 525.0 | yes`. The move was a `Switch` on its own confirming read.
   - P39 holds it: `replay follows | 30.54 | 30.54 | not before the next move at 36.04 | not before the next move at 36.04 | not before the next move at 36.04`.
   - All four rules then follow 625 at 36.04.
3. **Does P39 make no other difference from `5b6b704c`? No.**
   - On `031905`: `replay differs | 13.04 | 18.03 | 500.0 | 300.0` and `replay differs | 26.54 | 30.00 | 500.0 | 300.0`.
   - P39 follows the tracker to 300 Hz, on reads that confirmed 300.0 at 12.04 and 26.04, where `5b6b704c` held 500.
   - Under `5b6b704c`, `031905` went from 108 to 115 elements with 36 named. P39 gives that up.
   - On the stream, P39 does not differ from `5b6b704c`. On 032113 it differs only by the intended 650 follow.

**The four tests.** None was run on a change, because none was built:

| Test | Before | After | Result | Kept |
|---|---|---|---|---|
| 1. All keyed edits do not rise | 165 over 565 | 165 over 565, no change built | not judged | nothing built |
| 2. No named floor broken | 13 of 13 | 13 of 13 | not judged | nothing built |
| 3. Adjudicated unchanged | 13 of 13 | 13 of 13 | not judged | nothing built |
| 4. No capture row's above-bar count falls | 51 of 51 | 51 of 51 | not judged | nothing built |

**The opening, beside the cold group `EANQNID`:**
- Before and after, on the spliced stream from 30 to 46.2 s: 22 named, `UIEH EE E E T I NIEEE E E ET N ■IK`.
- 003919 cold reads `EITEETNXNIK EANQNID EANQNIK`.
- Nothing changed.

**032113:** 47 above the bar before and after. The mix follows the 650 Hz move at 26.54 s, the same moment the tracker made it, as at entry. At entry the tracker does not make the 500 Hz move before the file ends.

## 4. What's blocking us

Nothing blocks the phase (R65). Two items, most useful first.

**1. Whether the replay gate's third clause is judged against the entry, not against `5b6b704c`.**
- **Proposed ruling:** the owner or author decides whether P39 may be built and judged under 3.2's four tests as a new 7.4 unit, with the third clause read as "no difference from the entry except the opening". Without that ruling, the P39 route at 7.4 is closed.
- **Reasoning:**
  - On the follow times the replay prints, P39 matches the entry on 032113 and 031905. It differs only on the opening, where it holds the 525 Hz move.
  - The gate stopped P39 because `5b6b704c` held 031905's moves to 300 Hz, and P39 does not. Those holds are what gave 031905 seven elements under unit 430's change.
  - The measured shapes make it plain that no release rule based on confirmation alone can tell the two cases apart. 031905's move at 13.04 was confirmed two reads before it, at 12.04. That is the same shape as 032113's 650 Hz move, confirmed at 25.54 and made at 26.54.
  - So "follow 032113" and "keep 031905's gain" cannot both hold under a rule of this kind.
  - A caveat: the replay runs on the entry decoder's tracker, and no other capture row was replayed. The four tests would decide.
- **Rejected:**
  - Building P39 in this unit, because section 6 made the gate binding and no self-ruling authorizes work outside the tasks.
  - A variant that also holds 300 Hz moves, because section 6 forbids a second variant, and a rule keyed to 031905's pitch would be tuned to one recording.

**2. `WhyTheMixMoved` prints `CwToneSurvey.Analyze`'s verdict, not `Tracker.Verdict`.** For the record.
- **Proposed ruling:** none. The two differ where the tracker refuses a candidate the survey admits, and the printer's rule column names each refusal. The new printer reads `Tracker.Verdict` directly.
- **Reasoning:** section 5 describes the printer as reading the tracker's `Verdict`. It recomputes that verdict from the same history.
- **Rejected:** repairing it, because section 5 says to repair nothing.
