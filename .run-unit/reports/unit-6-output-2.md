READ IN THIS ORDER.

A. Hamlet reads a CQ call correctly. Steps 0 to 2 done; 3 partial and
   closed under 3.4 with 3.6 open; 4 not started; 5 the owner's; 6 partial
   with 6.5 open; 7 partial with 7.1, 7.2, 7.4, 7.6 and 7.8 open.
B. Step 7, criterion 7.4: the mix follows a move of more than 25 Hz only
   when the decoder's envelope keys harder at the target than at the mix.
   The replay answered yes on 032113's 650 Hz move and no on the opening's
   525 Hz move. Nothing was built, so nothing was kept or taken back out.
   032113 stays at 47 above the bar with 102 elements, and 031905 at 36
   with 108. The opening is unchanged beside EANQNID. 7.4 is closed
   partial with P48 and is not ticked.
C. This report adds one finding that bears on B. At 30.54 s the decoder's
   own envelope keys harder at 525 Hz than at the sender's 625 Hz, so the
   envelope agrees with the tracker's wrong move. Section 4 raises 2
   items. Neither stands in the way of 7.4 as it now stands, closed
   partial. Nothing halts the phase.

```
UNIT:       433 - complete at task 3 of 3, none dropped - 2026-09-25 05:45
PHASE GOAL: Hamlet reads a CW CQ call as the text that was sent, measured against keys, and Tim confirms it at the radio
UNIT GOAL:  stop the mixdown following the tracker off the sender in the 7.052 opening by comparing keying at the two pitches on the decoder's own envelope, without costing 032113 or any other row, gated on a replay and kept only under 3.2's four tests
ADVANCED:   no - the replay gate answered no on the opening's 525 Hz move, so no rule was built, and 7.4 closes partial without a tick
NUMBER:     keyed 165 -> 165 over 565, inferred keys; 032113 above-bar 47 -> 47; opening stream 30 to 46.2 s 22 -> 22 named, UIEH EE E E T I NIEEE E E ET N ■IK -> unchanged
DRIFT:      1
```

## 1. What Claude did

**Complete, at task 3 of 3, with none dropped.** Task 2 built nothing because section 6's gate said to build nothing if either replay answer was no. That is the gate's instruction, not a drop. The session ran on QUIVERFULL, in the Hamlet project on main, and the project gate passed. Entry HEAD was `f1e751ea`.

**Task 0, the record (`f5271938`).**
- Bumped the version from 1.13.119 to 1.13.120.
- `PHASE_STATUS.md` now names unit 433 and CURRENT_STEP 7. The launcher had left CURRENT_STEP at 3 and WORK_INSTRUCTION at 432.
- Added `## UNIT 433 - STEP 7` to `PHASE_OUTCOME.md` from the decision block, with an ENTRY line.
- Parked unit 432's section 4 items 1 and 2 verbatim as P46 and P47, each with its note from section 3.
- Committed the launcher's four root files as they stood.

**Section 5 against the tree: every check holds.**
- `CwDecoder.cs` lines 600 to 603 and 617 to 621 are as stated, and the file is identical to `a7e6e2f2`.
- `Envelope(samples, sampleRate, toneHz)` is public and static and passes `IntegratorBandwidthHz` (45.0). The overload that takes a bandwidth is beside it.
- `CwToneSurvey` defaults to `seconds = 3.0`.
- Both printers are present, and `WhenEachRuleFollows` came in at `f0845a92`.
- The 032113 row is 47 and 102, and its keyed floor is 47.
- The version was 1.13.119.

**Entry round and exit round, one type per invocation.** Apart from timing, every output at exit is identical to entry. At entry, every output was also identical to unit 432's exit.

| Check | Entry | Exit |
|---|---|---|
| Build, warnings as errors | 0 errors | 0 errors |
| Engine carry-forward | 178 of 178, 371 s | 178 of 178, 370 s |
| App carry-forward | 278 of 278, no dispatcher-loop loss | 278 of 278, no dispatcher-loop loss |
| Captures | 51 of 51, 120 s | 51 of 51, 119 s, every row the same |
| Adjudicated | 13 of 13 | 13 of 13 |
| Keyed floors | 13 of 13 | 13 of 13 |
| All keyed | 165 over 565 | 165 over 565 |
| 17:37 | 19 over 25 | 19 over 25 |
| Added letters | 17, 8 of them single-element | 17, 8 of them single-element |
| `WhatTheOpeningHeardTests` | 12 of 12, 170 s | 15 of 15, 181 s |
| Pitch types, 600 s each | P40 1 of 4; P41's two run 0; the other ten green | not run: task 2 built nothing |

Against the tree at exit:
- The eleven transmit files are unchanged from `7e209cb4`.
- `src` and `data` are unchanged from `f1e751ea`.

**Task 1, the replay (`1b5d75ae`).** I added `WhatTheOpeningHeardTests.WhereTheSenderKeysHarder`. It asserts nothing and writes nothing, and it drives the decoder a hop at a time, as `WhenEachRuleFollows` does. It keeps the same tracker record and runs two rules over it:
- the entry rule;
- section 6's rule, exactly as fixed there.

The entry rule reproduces the entry decoder's mix on every measured hop of all three runs, with 0 differences. The output is in `.run-unit/unit433-replay-t1.txt`.

**Decisions I made for myself, reproduced in full.** Each was fixed before the first run unless I say otherwise:
- **Percentiles.** I took them by nearest rank, `sorted[round(q × (n − 1))]`, as the file's own `Spread` does.
- **dB.** I used 20·log10 of the ratio, because the envelope is a magnitude. The comparison is plain greater-than, so the factor cannot change which pitch wins.
- **The window.** "The last 3.0 s" is the 3.0 s of audio ending at the hop just processed.
- **When a figure is missing.** A figure counts as not computable when under 3.0 s has been heard, or when the 10th percentile is 0. In both cases the rule follows as at entry. The second case did not arise.
- **What counts as a new move.** A move is new when the tracker moves more than 25 Hz from the pending pitch. While a move is pending, each comparison uses the tracker's current pitch.
- **Added after the first run.** I added one column to the move line: the contrast at each pitch the survey marked best in the stretch. It does not feed the rule, and it did not change either gate answer. I added it so a reader can see where the sender's 625 Hz stands, which is what section 8 asks the sender's pitch to show.

**Task 2 built nothing** because the gate's second answer was no. `src` is unchanged.

**Task 3.** I ran the exit round and checked the tree, with the results above. I wrote P48 in `PARKED.md` and recorded 7.4 as closed partial on step 7's line in `PHASE_STATUS.md`. 7.4 is not ticked. Tasks 0 and 1 were pushed with git's return code 0. This file goes out in task 3's commit, and that push was checked after this was written.

## 2. What the owner should expect

**Nothing changes in the app.** The decoder is byte for byte what it was at entry. The 7.052 opening still reads `UIEH EE E E T I NIEEE E` on the spliced stream.

**7.4 is closed partial, not met.** This was its third unit in a row with no kept change (430, 432 and 433). Step 7's line in `PHASE_STATUS.md` now says so, and P48 holds the trace. The criterion is still unmet.

**One addition that looks like work but decides nothing.** `WhatTheOpeningHeardTests` now has 15 members. The new printer adds about 13 s to that type.

**The app line lost nothing to the dispatcher loop, at entry or at exit.** That is luck, not a fix (P45).

## 3. What you should see

No visible change. This unit measured a rule before building it, and the measurement stopped it.

**The gate's two answers.** Each print line reads: time, target Hz, target dB, mix Hz, mix dB, which is greater.

1. **Does the rule follow 032113's 650 Hz move by 27.04 s? Yes.**
   - `contrast read | 26.54 | 650.0 | 28.75 | 600.0 | 24.49 | target | at the move`
   - `contrast follows | 26.54 | 650.0 | 26.54 | 26.54` (the entry, then this rule)
   - The contrast is 28.75 dB at 650 against 24.49 dB at 600.
2. **Does the rule hold the opening's 600 to 525 Hz move until 36.04 s? No.**
   - `contrast read | 30.54 | 525.0 | 15.02 | 600.0 | 12.69 | target | at the move`
   - `contrast follows | 30.54 | 525.0 | 30.54 | 30.54`
   - The contrast is 15.02 dB at 525 against 12.69 dB at 600, so the rule follows at 30.54, as the entry does.
   - Beside it, deciding nothing: 625.0 Hz, the sender, 13.67 dB, and 850.0 Hz 14.39 dB.
   - The envelope keys harder at the wrong pitch than at the sender's own pitch. No margin on this comparison would have held the move.

**The four tests.** None was run on a change, because none was built:

| Test | Before | After | Result | Kept |
|---|---|---|---|---|
| 1. All keyed edits do not rise | 165 over 565 | 165 over 565, no change built | not judged | nothing built |
| 2. No named floor broken | 13 of 13 | 13 of 13 | not judged | nothing built |
| 3. Adjudicated unchanged | 13 of 13 | 13 of 13 | not judged | nothing built |
| 4. No capture row's above-bar count falls | 51 of 51 | 51 of 51 | not judged | nothing built |

**The opening, beside the cold group `EANQNID`:**
- Before and after, on the spliced stream from 30 to 46.2 s: 22 named, `UIEH EE E E T I NIEEE E E ET N ■IK`.
- 003919 cold reads `EITEETNXNIK EANQNID EANQNIK`, 25 named.
- Nothing changed.

**032113 and 031905.**
- **032113:** 47 above the bar with 102 elements, before and after. At entry the mix follows the 650 Hz move at 26.54 s, and it would under this rule too.
- **031905:** 36 above the bar with 108 elements, before and after. It was printed but not gated. Follow times under the entry and under this rule:
  - 13.04 s, the move from 500 to 300 Hz: 13.04 and 13.04. The contrast was 26.50 dB at 300 against 23.05 at 500.
  - 18.04 s, the move back to 500 Hz: 18.04 and 25.04.
  - 26.54 s, the move from 500 to 300 Hz: 26.54 and 29.04. At the move the contrast was 30.22 dB at 300 against 30.36 at 500.

## 4. What's blocking us

Nothing blocks the phase (R65). There are two items, most useful first.

**1. What would reopen 7.4.**
- **Proposed ruling:** the owner decides whether a later unit may question the tracker's choice of 525 Hz at 30.54 s on the spliced stream. Without that decision, 7.4 stays closed partial under P48.
- **Reasoning:**
  - Four follow rules have now been measured against one line, 617 to 621.
  - Three of them asked whether the survey confirmed the new pitch. Unit 432 showed that confirmation looks the same for right moves and wrong ones.
  - The fourth compared the two pitches directly. At 30.54 s the decoder's own envelope ranks 525 (15.02 dB) above 850 (14.39), above the sender's 625 (13.67), and above the mix at 600 (12.69).
  - So the evidence the mixdown could weigh points the same way as the tracker's wrong move. Deciding when to follow cannot undo a choice that the evidence supports.
  - The tracker's choice belongs to the tracker under HM-DEC-095 and HM-DEC-127, and this unit did not touch it.
- **Rejected:**
  - A margin, another percentile, or another window. Section 6 forbids a variant, and no plain margin can invert a ranking where the target wins.
  - Combining this rule with `5b6b704c`, `ec76051e` or P39, which section 10 forbids.

**2. The window at 30.54 s starts before the splice.** For the record.
- **Proposed ruling:** none.
- **Reasoning:**
  - The 3.0 s ending at 30.54 on the spliced stream runs from 27.54 s. It starts inside `003901`'s tail, before `003919` begins at 30.0 s.
  - Both the rule and the survey's own 3.0 s history see that joined audio, and the live session's decoder saw its own continuous audio.
  - Whether the contrast would rank the pitches differently on unspliced audio was not measured.
- **Rejected:** measuring it in this unit, because no task asked for it and no self-ruling authorizes work outside the tasks.
