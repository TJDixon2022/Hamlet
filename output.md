READ IN THIS ORDER.

A. Hamlet meets the CW requirements: step 2 at 3 of 5 by the plan's checkboxes (2.1, 2.2, 2.3), steps 0 and 1 done,
   3 to 8 not started. Four recordings' text is unchanged at HEAD; section 3 shows it with the letters the not-kept change dimmed marked.
B. Step 2, HM-REQ-010: 2.1 ticked, from unit 440's committed trace of all 54; 2.2 ticked, by unit 441's two kept changes (G1 and the marks' speed),
   because this unit's change was not kept; 2.3 ticked, after metrics.md gained the key's kind beside each per-condition number;
   2.5 not ticked, because 17:37's named floor is red at the exit of every commit after task 1 (V-11 left it red).
   The confidence change was not kept. R78's five: MET-CER-SURE 0.1116 -> 0.1005 falls; MET-INVENTED 47 -> 40 does not rise;
   sure-and-right coverage 374 -> 358 FALLS; adjudicated 13 of 13 hold; V-11 fails on 8 recordings. MET-CER-SURE at HEAD 0.1116 -> 0.1116.
C. This report adds a rival-margin trace of all 421 sure letters. It shows that how far a letter's reading beats its nearest rival
   does not separate right letters from wrong ones: 13 of the 47 wrong-or-added letters sit above 128 nats. So dimming by margin cannot
   move MET-CER-SURE without taking coverage with it. Section 4 raises 3 items; item 1 is in the way of criterion 2.5.

UNIT:       442 - complete at task 3 of 3, second lever dropped - 2026-09-25 21:48
PHASE GOAL: Hamlet's CW decoder meets CW_REQUIREMENTS.md, judged on its metrics. This step: a letter printed as sure is wrong less than 1 % of the time.
UNIT GOAL:  Stop printing close calls as sure by setting each letter's class from how far its reading beats the nearest rival, and get the floors green.
ADVANCED:   yes - 2.1, 2.2 and 2.3 flipped in PHASE_PLAN.md, all from work already in the tree; no decoder change was kept this unit
NUMBER:     MET-CER-SURE 0.1116 -> 0.1116 (0.1005 under the not-kept change); sure-and-right coverage 374 -> 374 over 473 (358 under it); dim precision none at HEAD, which emits no dim letter (0.6957, 16 of 23, under it)
DRIFT:      1 consecutive unit without a kept change (was 0)

## 1. What Claude did

**Complete, at task 3 of 3.** The drop candidate, the second lever, was dropped whole. Run by `run-phase.bat` on QUIVERFULL,
project Hamlet (the gate's six checks held), branch `main`. All five commits were pushed to `origin/main`: `83a652d9`, `3cf9f9ea`,
`d5d53501`, `cdeaac36` and the closing commit. `SESSION.lock` was not touched, and nothing was written to `RUN_LEDGER.md` or `tools\arbiter\`.

**Task 0, entry and ticks.** Version 1.13.128 to 1.13.129, and PHASE_STATUS.md names 442 in both copies. PHASE_OUTCOME.md has
`## UNIT 442 - STEP 2` in both copies. The root copy also carried the runner's uncommitted `UNIT 1 - STEP 2` entry, which went
into the same commit. Entry round, one type per invocation:
- build: 0 errors
- engine carry-forward: 175 of 178, with 013010, 012823 and 012922 red on counts
- app carry-forward: 277 of 278; BindingHealthTests failed and is green alone, 1 of 1
- floors: captures 38 of 51, adjudicated 13 of 13, named 7 of 13
- real recordings, inferred keys: MET-CER-SURE 47 of 421 (0.1116), MET-INVENTED 47 over 473 (0.0994), sure-and-right coverage 374 over 473 (0.7907), MET-WBE 52 over 113 (0.4602)
- synthetic: MET-CER-SURE 14 of 173 (0.0809)

Ticks:
- **2.1**: `.run-unit/unit440-trace.txt` (committed) prints all 54 with the recording, what was sent, what was emitted, the span, the wpm and the pitch at the hop, and the envelope's marks, then the groups.
- **2.3**: metrics.md had before and after for both kept changes, but its per-condition lines carried no key kind. So I wrote out a per-condition table for each change, with the key's kind in its own column, from 441's committed printouts. Nothing was re-measured. That was my own decision, made by analogy with 2.1's "add the missing field, then tick".

**Task 1, the floors.** The 13 red capture rows and 5 of the 6 red named floors were re-banked at their counts at `0439a8e7`, in one
commit. The text each row read before (441's entry decoder, `f14b2453`, run by checking its two `src` files out temporarily) and
after is in `.run-unit/unit442-floorrows.txt`. The unkeyed rows' text is also in the commit message.
- **V-11 per keyed recording**, across 441's two changes, on MET-CER-SURE, wrong-or-added, sure-and-right and boundaries wrong: 17:37's boundaries wrong went 5 to 7, so **its named floor (46 banked, 38 now) was left red.** `032050` also got worse (wrong-or-added 4 to 5), but it has no red row.
- Exit of the commit: captures 51 of 51, adjudicated 13 of 13, named 12 of 13 (17:37), engine carry-forward 178 of 178. App carry-forward was 273 of 278: TheFavoritesAreChipsTests (4) and TheFavoritesAreUnderTheGreenZoneTests (1), which are green alone, 4 of 4 and 3 of 3. **Five is more than the "up to 3" §2 expects.**

**Task 2, the confidence.**

*The margin.* `CwProbabilisticDecoder.RivalMargin` works on the path's own lattice, over each letter's own span. It finds the two
best distinct readings, where a reading is the dits, the dahs and where the letters split. Segments are scored exactly as `DecodeAt`
scores them. The margin is the emitted reading's score less the best different reading's, in nats. Keeping the top two readings per
hop is exact, because a prefix that two others beat into a hop cannot finish ahead of both. The stream carries the margin into
`CwCharacter.MarginLlr`, which was the sidecar's never-set seam. The captures floor took 130 s where it took 125 s.

*The trace.* `EachSureLetterAgainstItsNearestRival` prints all 421 sure letters (374 right, 43 wrong, 4 added). For each one it
gives the margin, the span ratio against silence, the margin over the span, and the window marks' unit over the path's unit.
I fixed the bins before reading any number:

| margin, nats | wrong or added | right |
|---|---|---|
| below 0 | 0 | 0 |
| 0 to 0.5 | 4 | 13 |
| 0.5 to 1 | 3 | 3 |
| 1 to 2 | 7 | 28 |
| 2 to 4 | 8 | 84 |
| 4 to 8 | 1 | 35 |
| 8 to 16 | 3 | 54 |
| 16 to 32 | 5 | 60 |
| 32 to 64 | 1 | 32 |
| 64 to 128 | 2 | 25 |
| 128 and over | 13 | 40 |

**No edge.** Below every edge, right letters outnumber wrong or added, and no bin holds only wrong letters.

*The change.* §5 says to build the change anyway at the edge where wrong outnumbers right. No such edge exists, so I took 1 nat:
the top of the only bin where wrong is not outnumbered (3 to 3). This was my own decision, made before R78 was read. The change
set `Low` below 1 nat. Judged under R78:

| part of R78 | before | under the change | verdict |
|---|---|---|---|
| MET-CER-SURE, real | 47 of 421, 0.1116 | 40 of 398, 0.1005 | falls |
| MET-CER-SURE, synthetic | 14 of 173, 0.0809 | 11 of 166, 0.0663 | falls |
| MET-INVENTED, real | 47 over 473 | 40 over 473 | does not rise |
| sure-and-right coverage, real | 374 over 473 | 358 over 473 | falls |
| sure-and-right coverage, synthetic | 159 | 155 | falls |
| adjudicated readings | 13 of 13 | 13 of 13 | hold |
| V-11 | - | 8 recordings lose sure-and-right, `032113` 21 to 12 | fails |

**Not kept:** 16 right letters dimmed for 7 wrong or added, so coverage falls. The change was taken back out of `src`. Its diff
is `.run-unit/unit442-dim-notkept.diff`, and it is recorded in metrics.md. Dim precision under it was 16 of 23, 0.6957.

*The second lever* is the window marks' unit over the path's unit. From the same letters, in bins fixed beforehand:

| window marks over the path's unit | wrong or added | right |
|---|---|---|
| under 0.33 | 1 | 0 |
| 0.33 to 0.50 | 5 | 17 |
| 0.50 to 0.67 | 3 | 39 |
| 0.67 to 0.80 | 5 | 43 |
| 0.80 to 1.25 | 33 | 275 |

Only the first bin separates, and it holds one letter. **Dropped whole** because the unit was at its hour; it is the named drop candidate.

**Task 3, exit round at HEAD, one type per invocation.**
- build: 0 errors
- engine carry-forward: 178 of 178 in 369 s
- app carry-forward: 276 of 278. TheCarrierHoldsTheButtonsTests is green alone, 8 of 8, and TheFavoritesAreUnderTheGreenZoneTests is green alone, 3 of 3.
- floors: captures 51 of 51, adjudicated 13 of 13, named 12 of 13 (17:37)
- metrics: unchanged from entry
- touched types: WhereTheSureWrongLettersComeFromTests 3 of 3, TheProbabilisticDecoderTests 11 of 11, TheSpanRatioReachesTheSidecarTests 5 of 5. WhatTheFloorRowsReadTests passed at `3cf9f9ea` and under the not-kept build; it is a printer and was not re-run at exit.

**`src`, file by file, all pushed:**
- `CwProbabilisticDecoder.cs`: `RivalMargin`, `WithRivals`, and `CwProbabilisticCharacter.RivalMargin`.
- `CwProbabilisticStream.cs`: one line, setting `MarginLlr`.
- `CwCharacter.cs`: the `MarginLlr` doc.

What prints is unchanged. PHASE_PLAN.md: 2.2 ticked in both copies, 2.4 not ticked, 2.5 not ticked.

**The transmit files.** This unit's diff touches none of them: the diff against entry prints nothing. The instruction names no file list,
so I used the 22 files under `src` whose names say transmit, PTT, keyer or sender. Against `7e209cb4`, 13 of those differ, every one
a file created after that commit. So I cannot repeat 441's "prints nothing" on this list (section 4, item 3).

**`validate-output.bat` was not run.** The session's permissions refused the call, and I did not route around them. Its six rules,
as the script prints them, were checked with `.run-unit/unit442-check-output.sh` over the same patterns, and all six hold. A
reading of the rules is not the validator's verdict.

**Mismatches against the tree** (§2):
- `134712` is keyed in the metrics (baseline, 3 sent, 0 wrong before and after), not unkeyed. It was re-banked either way.
- The trace types assert that their count equals the metric's count. That is not nothing.
- `CwMetrics` counts `Low` as NotSure: never sure and never wrong.
- The app line lost 5 at task 1, more than the stated 3.
- **Known and not edited:** PHASE_OUTCOME.md's header lists the old titles for steps 2, 3 and 8, and CW_SPEC.md §11 still defines MET-COVERAGE as sure over sent, where R82 has it.

## 2. What the owner should expect

A letter printed as sure is no more often right than it was this morning. MET-CER-SURE is 0.1116 at HEAD, as it was at entry.
The change that would have moved it to 0.1005 was measured and not kept, because it dimmed 16 right letters to catch 7 wrong
ones, and sure-and-right coverage would have fallen from 374 to 358 over 473. All of that is on inferred keys (V-13).

What will look wrong but is not:
- The capture sheet's margin column now carries a number where it printed "unmeasured". That is `MarginLlr`, the letter's margin over its rival.
- The capture floors read lower numbers. They were re-banked under R78.
- 17:37's named floor is red on purpose.

The trace's real finding is this: 13 of the 47 wrong-or-added letters beat their nearest rival by more than 128 nats. The model is
confidently wrong about them, so their class cannot fix them. The error is upstream, in the evidence the path is given (pitch,
speed, envelope), not in how sure the path is of its choice.

## 3. What you should see

**What the operator reads is unchanged at HEAD.** No letter prints dim yet. Here are the four recordings as they read now, and in
brackets the letters the not-kept change would have dimmed:

| recording | at HEAD, and after this unit | under the not-kept change, dim in brackets |
|---|---|---|
| `003758` | `■R L T U   I AN EAND E A ET EEEETMP/4 QNIKK` | `■R L T (U)   I (A)N EAND E A ET EEEETMP/4 QNIKK` |
| `031838` | `A 3, AT3 , 2TT 2, AND  ■ W IAH A MEAN OF 2 TT` | `A 3, (A)T3 , 2TT (2), AND  ■ W (I)AH A MEAN OF 2 TT` |
| `032129` | `■ MTMTJ26 PGOPAGATION E EE EIIEE ...` | `■ (M)T(M)(T)J26 P(G)OPAGATION E EE EIIEE ... (S) ... (I) ...` |
| 17:37 | `T EABNIREDWBZ WB6RED CQ CQ CQ DEWB6 RE D W B 7E E I` | `(T) EA(B)NIRE(D)WBZ WB6RED CQ CQ CQ DEWB6 RE D W B (7)E E I` |

**Is a wrong letter the operator used to see as sure now dimmed? No.** The change was not kept. Under it, some would have been:
17:37's `7` where `6` was sent, `032129`'s `G` in `PGOPAGATION`, and 5 more. But right letters beside them would have dimmed too,
16 in all.

No other visible change. The floors are green again, except 17:37's named floor.

## 4. What's blocking us

1. **17:37's named floor: leave it red, or re-bank it at 38?** Ruling wanted: whether MET-WBE counts as a "requirement metric got worse" for a re-bank under task 1. Reasoning: 17:37's boundaries wrong went 5 to 7 under G1. That is HM-REQ-080/081's metric, but R78's keep rule does not list MET-WBE. The instruction says to re-bank only where no requirement metric got worse, so the floor was left red. While it is red, 2.5 cannot be ticked. Rejected: re-banking anyway, which would read V-11 more loosely than the instruction wrote it.
2. **Should the rival margin stay in `src` as the trace's instrument?** Ruling wanted: keep `RivalMargin` and the `MarginLlr` it fills, or take them out. Reasoning: the instruction says to leave a not-kept change out of `src` and keep the trace. The trace needs the margin, and the margin now reaches the capture sheet, while the class and every printed letter are unchanged. It costs about 4 % decode time on the captures floor. Rejected: moving the lattice into the test through reflection, which would copy the decoder's private scoring and could drift from it.
3. **Which files are "the transmit files" for the check against `7e209cb4`?** Ruling wanted: the list, so the check can be repeated. Reasoning: nothing in CLAUDE.md or the instruction names it. On a 22-file list by name, 13 differ from `7e209cb4`, all created since. This unit touched none of them. Rejected: asserting "prints nothing" without knowing the list.
