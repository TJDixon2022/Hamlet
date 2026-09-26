READ IN THIS ORDER.

A. Hamlet meets the CW requirements: step 3 at 3 of 6, step 2 at 3 of 5 by the plan's
   checkboxes, steps 0 and 1 done, 4 to 8 not started.
B. Step 3, criterion 3.2, HM-REQ-011 with HM-REQ-010 and 012 as guards: the trace named an edge
   at 6.5 on the worst inner gap, catching 2 wrong for 0 right; the change kept. MET-INVENTED
   47 -> 45 over 473; MET-CER-SURE 0.1116 -> 0.1074; coverage 374 -> 374; 3.2 ticked, and 3.3
   with it; 3.6 red on 17:37.
C. What this report adds: the first mark-shape trace of every sure letter, one kept change that
   dims two wrong letters and no right one, and a finding that the edge is narrow.
   Section 4 raises 1 item; none is in the way of a criterion in B.

UNIT:       445 - complete at task 3 of 3, none dropped - 2026-09-26 01:07
PHASE GOAL: Hamlet's CW decoder meets its written requirements, above all that it never prints a guessed letter as a sure one.
UNIT GOAL:  Stop printing as sure a letter whose marks or gaps do not fit the speed in force, but only past a line no right letter crosses, so fewer wrong letters look certain and none of the right ones dim.
ADVANCED:   yes - 3.2 and 3.3 flipped in both copies of PHASE_PLAN.md
NUMBER:     MET-INVENTED 47 -> 45 over 473; MET-CER-SURE 0.1116 -> 0.1074; coverage 374 -> 374; edge 6.5 (worst inner gap)
DRIFT:      step 3 0; step 2 2

## 1. What Claude did

**Complete, 3 of 3 tasks, none dropped.** QUIVERFULL, `C:\Source\HamLet`, Hamlet confirmed by
the six-file gate, branch `main`, every commit pushed to `origin/main`: `3dc62a7f` (task 0),
`5513d7cf` (task 1), `72c2c75c` (the change, on its own), `73a4e08a` (its record), then this
closing commit.

**Task 0, the entry.** `PHASE_OUTCOME.md` has `## UNIT 445 - STEP 3` from the decision block,
in both copies. `PHASE_STATUS.md` names 445 at `CURRENT_STEP: 3` in both copies. Version went
1.13.131 to 1.13.132. The runner's `PHASE_OUTCOME.md`, `PHASE_STATUS.md`, `RUN_LEDGER.md` and
`WORK_INSTRUCTIONS.md` were committed as the runner wrote them.

Entry round, one type per invocation:
- build: 0 errors;
- engine carry-forward: 178 of 178;
- app carry-forward: 278 of 278;
- captures: 51 of 51;
- adjudicated: 13 of 13;
- named: 12 of 13, with 17:37 red (banked 46, reads 38).

MET-INVENTED at entry, per condition, with the key's kind:
- real, sender not stated: 47 over 410 (4 added, 43 substituted), inferred;
- real, TX-FARNS, TX-ITU and TX-TIGHT: 0 each, inferred;
- synthetic: 14 over 252 (6 added, 8 substituted), exact. ITU 5 dB and ITU 15 dB have 1 added
  each. Char-gap-5 15 dB has 1 substituted, and char-gap-5 5 dB has 4 added and 7 substituted.

The other metrics at entry, real and inferred:
- MET-CER-SURE: 47 of 421, 0.1116;
- coverage: 374 over 473, 0.7907;
- MET-WBE: 52 over 113, 0.4602.

**Task 1, the trace.** `WhatTheSureLettersMarksLookLikeTests` asserts nothing. It covers every sure
letter on the real keyed recordings and on the synthetic set: 421 real (374 right, 43 wrong,
4 added) and 173 synthetic (159, 8, 6). Those counts reconcile with the metric. For each letter it
prints:
- the recording, time, key and emission;
- the unit in force, taken from `Stream.Last`, with its source (estimator 369, grid 34, the marks'
  speed 18);
- every whole envelope mark and inner gap, in ms, in units and as a distance;
- the worst mark and worst gap distances;
- the number of marks against the pattern's elements.

The marks come from the letter's span, one unit either side, cut the way `Elements` cuts. Only whole
marks count, and only the gaps between two of them. A distance is a ratio of one or more, taken
against the nearer of 1 and 3 units for a mark and against 1 unit for a gap. The bins were fixed
before any number was read: 1.25, 1.5, 2, 3 and 5.

Findings:
- **Worst mark: no edge.** Right letters lie in every bin: 2 at 5 and over, and 8 with no whole
  mark at all.
- **Worst inner gap: edge 6.5**, exactly. Past it lie 0 right letters and 2 wrong ones, both on
  `032129`: 17.400 s `T` read `E` (distance 10.0) and 20.660 s `0` read `E` (15.0).
- **Together: no edge**, because of the same 8 right letters with no whole mark.
- **Synthetic, exact keys: no edge on any measure.** No wrong-or-added letter lies past the
  farthest right one (mark 1.676, gap 3.375). At 6.5 the synthetic set has nothing on either
  side. The two sets therefore do not confirm each other. They also do not conflict.

**Task 2, the one change, kept.** In `CwProbabilisticStream.Character`, a known letter is now
emitted `Low` if any gap between two of its whole marks is more than 6.5 times longer or shorter
than one unit at the path's speed. The gap is read from the window the stream holds at emission
(HM-REQ-015). It never reads the neighbours (R72). The edge was taken from the trace and not moved.
`CwUnitEstimator.InnerElements` is the trace's segmentation moved into `src`, and the trace now
calls it, so the rule and the trace run the same code. `CallsignResolver`, `ContactTracker`, G1,
the marks' speed, `RivalMargin` and `MarginLlr` are untouched.

Judged against §3, item by item:
- MET-INVENTED, real: 47 -> 45. It falls.
- MET-CER-SURE: real 47 of 421 -> 45 of 419 (0.1074); synthetic 14 of 173, unchanged.
- Coverage: real 374 over 473 and synthetic 159 over 252, both held.
- Adjudicated readings: 13 of 13.
- V-11: 0 of 35 recordings worse on any of the four metrics.
- Dim precision (HM-REQ-014), real, keyed stretches: 0 right of 2 dim, 0.0000. It is not a keep
  condition. Dimming only wrong letters puts it at zero by construction, and the number says so.
- Floors under the change: captures 51 of 51; named 12 of 13 with 17:37 at 38, as at entry.

`metrics.md` carries the table and the per-condition before and after, with the key's kind. The
re-run trace under the change shows 419 sure, 0 wrong past 6.5, and the same 374 right.

**Task 3, the exit round.**
- build: 0 errors;
- engine carry-forward: 178 of 178;
- app carry-forward: 278 of 278;
- captures: 51 of 51;
- adjudicated: 13 of 13;
- named: 12 of 13, with 17:37 red at 38 as at entry;
- metrics: as under the change.

Types touched or exercising the touched code, one invocation each:
- `EachCharacterAnswersForItselfTests` 6 of 6;
- `TheShortRunFilterDropsWithoutMergingTests` 2 of 2;
- `CallsignResolverTests` 29 of 29;
- `WhereTheSureAddedLettersComeFromTests` 2 of 2;
- `WhatTheSureLettersMarksLookLikeTests` 1 of 1;
- `WhereTheSureWrongLettersComeFromTests` 3 of 3;
- `TheUnitIsMeasuredNotSearchedTests` 4 of 5. The one failure is `TheFiveToEightDecibelPlateauHolds`,
  the correctness phase's recorded red.

**`src`, file by file, against entry `0528afbe`:**
- `src/Hamlet.RadioEngine/Cw/CwProbabilisticStream.cs`: `InnerGapEdge = 6.5`, `GapsFitTheUnit`,
  and `Character` taking the window and choosing `Low`.
- `src/Hamlet.RadioEngine/Cw/CwUnitEstimator.cs`: `InnerElements`.

Neither file keys or transmits. The only "key" words in the diff are `keyDown`, the envelope's
key-up/key-down state on the receive side.

**3.2 and 3.3 ticked** in both copies. 3.4, 3.5, 3.6, 2.4 and 2.5 were not ticked.
**DRIFT:** step 3 is 0, because this unit's change was kept. Step 2 is 2, unchanged.

**Mismatches against the instruction:**
1. V-11 is in `CW_REQUIREMENTS.md` line 254, not in `CW_SPEC.md`. Its text there: "No change may
   make an earlier capture or the synthetic corpus go red to make a newer one green."
2. `CwProbabilisticStream.cs` sets `known ? High : Unreadable` at line 685 at entry, as stated.
   `CallsignResolver` acts only on `High` at line 257, and `ContactTracker` at line 216. Confirmed.
3. §2 says `CLAUDE.md` §1 holds CPS-DEC-0183. The string does not occur anywhere in `CLAUDE.md`.
4. The quoted requirements, from `CW_REQUIREMENTS.md`:
   - HM-REQ-010: "keep MET-CER-SURE below 1 %".
   - HM-REQ-011: "keep MET-INVENTED at zero".
   - HM-REQ-012: "emit at least 90 % of sent characters as sure (MET-COVERAGE ≥ 0.90) ... Without a
     floor, dimming everything satisfies HM-REQ-010". Its wording is sure over sent, not R82's
     sure-and-right. This is the same known item as `CW_SPEC.md` §11.
   - HM-REQ-014: "characters the decoder emits as dim shall be correct at least 70 % of the time".
   - HM-REQ-015: "make each character's confidence class available at the moment the character
     is emitted".

   HM-REQ-010, 011 and 012 speak of must-tier conditions at or above the sensitivity floor. As
   `metrics.md` already records, no real row is such a condition.
5. Known, not mine, reported once and not edited: `PHASE_OUTCOME.md`'s stale step titles,
   `CW_SPEC.md` §11's coverage text, and RULES_AT HM-DEC-165 (for which see 3).
6. A finding from the trace: 441's printer passes `CwUnitEstimator.Elements` over the letter's
   span, one unit either side. `Runs` records the leading run, so every gap list that printer gave
   includes the padding's partial gap before the letter. This unit's trace keeps only inner gaps.

**A slip of my own, corrected.** At task 1's start I ran `tools/status.sh` from `.run-unit`, and it
overwrote the tracked `.run-unit/PROJECT_STATUS.md`. I restored it with `git checkout` to its
committed content. It was never committed changed. Every later status call went through
`.run-unit/unit445-st.sh`, which runs from the root.

## 2. What the owner should expect

Yes, the operator now sees fewer wrong letters printed as sure, and no right letter turned dim.
The evidence, on inferred keys (V-13):
- Across the 23 real keyed recordings, 45 wrong letters remain sure instead of 47.
- On `032129`, the key's `T` and `0` that were read as `E` now print dim.
- All 374 right letters stay sure.
- No recording is worse on any metric (V-11, 35 of 35).
- No callsign is gained or lost on any capture or synthetic case.
- The synthetic set is untouched.
- The floors are as green as at entry: captures 51 of 51, adjudicated 13 of 13, and named 12 of 13
  with 17:37 red at 38, as recorded. Both carry-forward lines are green.

Two things will look wrong but are not:
- Dim precision reads 0 of 2. That is by construction, because the only keyed letters dimmed are
  the wrong ones.
- A third letter on `032129`, an `I`, also prints dim. It sits outside the keyed stretch, so no key
  can say whether it was right.

The gain is small, 2 of 47. The mark distance itself separated nothing, because right letters sit
at every distance from a dit or a dah.

## 3. What you should see

The trace's table on the real set, inferred keys. Right against wrong-or-added, by worst mark
distance. **No edge:** right letters lie past every distance.

| worst mark distance | right | wrong or added |
|---|---|---|
| 1.00 to 1.25 | 194 | 15 |
| 1.25 to 1.50 | 98 | 8 |
| 1.50 to 2.00 | 55 | 10 |
| 2.00 to 3.00 | 7 | 4 |
| 3.00 to 5.00 | 10 | 3 |
| 5.00 and over | 2 | 4 |
| no whole mark | 8 | 3 |

By worst inner gap distance. **The edge is 6.5, inside the 5.00-and-over bin.** Past it lie 0 right
and 2 wrong.

| worst inner gap distance | right | wrong or added |
|---|---|---|
| 1.00 to 1.25 | 218 | 31 |
| 1.25 to 1.50 | 84 | 6 |
| 1.50 to 2.00 | 26 | 2 |
| 2.00 to 3.00 | 22 | 1 |
| 3.00 to 5.00 | 21 | 3 |
| 5.00 to 6.50 | 3 | 2 |
| **past 6.50 (edge)** | **0** | **2** |

What the operator reads, for the one recording whose text or dimming changed, `032129`. Brackets
mark a dim letter.

- before: `MTMTJ26 PGOPAGATION E EE EIIEE I E EE IEEEE I HE EE I ...`
- after:  `MTMTJ26 PGOPAGATION E EE [E]IIEE I [E] EE [I]EEEE I HE EE I ...`
- `[E]` at 17.400 s: key `T` (inferred).
- `[E]` at 20.660 s: key `0` (inferred).
- `[I]`: outside the keyed stretch, no key.

No other capture and no synthetic case changed text or dimming. Callsigns gained: none. Callsigns
lost: none.

## 4. What's blocking us

1. **The edge is two letters wide, and it is set by the unit as much as by the shape.** The two
   letters it caught both sit on `032129`. Each rests on a 10 ms key-up, the envelope's 2-hop
   minimum, read against a slow grid unit of 100 and 150 ms. The two right letters at exactly 6.5
   are a 10 ms key-up at a 65 ms unit. A rule of the form "an inner gap past N units" therefore
   partly reads "the grid chose a slow unit". Ruling wanted: should a later step-3 edge need a
   minimum number of wrong letters past it, or agreement from the synthetic set, before it is
   built? Reasoning: the instruction's definition, an edge no right letter crosses, was met and
   the change was kept under R78 as written. But one more right letter at 7 would have erased
   this edge. Rejected: moving or widening the edge in this unit, because the instruction forbids
   it. Not in the way of any criterion in B.
