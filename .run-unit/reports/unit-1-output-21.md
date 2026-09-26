READ IN THIS ORDER.

A. The phase goal, and what the operator now reads. Hamlet meets the CW requirements. Three
   recordings, before this unit and after it:
   - `031838`: `D  ■T TTT TEAH A MEAN TOF 2 TT` becomes `TT 2, AND  ■ W IAH A MEAN OF 2 TT`.
   - 17:37: `CQ CQ CQ DEWTEETEEERE D ETTTB 7E E I` becomes `CQ CQ CQ DEWB6 RE D W B 7E E I`.
   - `032129`: `TJ26 PGOPAGATION EE EE EIIEE I E EE` becomes `TJ26 PGOPAGATION E EE EIIEE I E EE`.
   - `003758`: `EETMP/4 QNIK` is unchanged.
   On the 23 real keyed recordings, with inferred keys:
   - MET-CER-SURE goes from 67 of 426 (0.1573) to 47 of 421 (0.1116).
   - MET-INVENTED goes from 67 to 47 over 473.
   - Sure-and-right coverage goes from 359 to 374 over 473 (0.7590 to 0.7907).
B. This step and its exit criteria. Step 2: the decoder stops printing wrong letters with
   confidence, under HM-REQ-010. Criterion 2.2 keeps a change under R78. Criterion 2.3 reports
   MET-CER-SURE before and after every kept change. Both changes were kept:
   - G1: MET-CER-SURE 0.1573 to 0.1337.
   - The marks' speed: 0.1337 to 0.1116.
   Criterion 2.5 is not met at exit: the floor tests and the engine carry-forward line are
   red on character counts. R78 says a falling count is reported and not rejected. Section 4
   item 1.
C. What this report adds. The remaining 43 sure-wrong letters no longer include any read in a
   window whose marks say the sender is slower than the path was timed. Of the rest, 8 were
   read at a grid speed the marks call far too slow, and 30 where the path and the marks
   agree. That bears on B: the next change is not another speed change of this kind.

```
UNIT:       441 - complete at task 3 of 3, none dropped - 2026-09-25 20:27
PHASE GOAL: Hamlet decodes CW to the numbers CW_REQUIREMENTS.md sets, judged on its metrics.
UNIT GOAL:  Fewer letters printed sure and wrong: keep G1, then stop the path being timed
            faster than the sender.
ADVANCED:   yes - two changes kept, 20 fewer sure-wrong-or-added letters on the keyed recordings
NUMBER:     sure-but-wrong: 54 -> 43; MET-CER-SURE 0.1573 -> 0.1116; changes kept 2 of 2
DRIFT:      0 consecutive units without advance (the instruction carries no prior count)
```

## 1. What Claude did

**Complete, at task 3 of 3, nothing dropped.** Claude Code on QUIVERFULL, at
`C:\Source\HamLet`, on branch `main`. Hamlet was confirmed: the four `MUST EXIST` files are
present, and `CoreHMI.sln` and `MURC.sln` are absent. The unit was launched by `run-phase.bat`,
iteration 1. `SESSION.lock` belongs to the runner, so the session neither took it nor released
it. Nothing was written to `RUN_LEDGER.md` or under `tools\arbiter\`.

**Task 0 (`bd155ea3`).**
- The outcome entry for 441 is in both copies. `PHASE_STATUS.md` names 441, and the version
  went from 1.13.127 to 1.13.128.
- **R82.** `CwCoverage.Share` is now sure-and-right over sent. The new test was watched
  failing first: `CQ DI K` against `CQ DE K` scored 1.0 as written and scores 0.8 now. The
  decoder was not touched, so this move is the definition's alone:
  - real: 0.9006 to 0.7590 (359 over 473);
  - synthetic: 0.7143 to 0.6190 (156 over 252).
- **The entry round.**
  - build: 0 errors;
  - engine carry-forward: 178 of 178;
  - app carry-forward: 275 of 278, the three dispatcher-loop losses, each type green alone;
  - captures 51 of 51, adjudicated 13 of 13, named 13 of 13;
  - MET-CER-SURE 67 of 426.

**Task 1 (`42d5dbb9`): G1 kept.** `cce7985d` was cherry-picked, and the `src` diff is
byte-identical to it.
- MET-CER-SURE: real 67 of 426 to 56 of 419; synthetic 24 of 180 to 14 of 173.
- MET-INVENTED: 67 to 56.
- Coverage: real 0.7590 to 0.7674; synthetic 0.6190 to 0.6310.
- Adjudicated readings: 13 of 13.
- V-11: none of the 35 recordings gets worse.
- Reported rather than rejected: capture `004133` goes from 28 to 25 named, and 17:37's named
  floor from 46 to 38. Unit 440 saw both.
- Engine carry-forward: 178 of 178.

**Task 2, the trace (`57b1f095`).**
`EachSureWrongLetterAgainstTheSpeedItsMarksImply` is a printer. For each of the 51 it prints:
- the speed the path was given and where it came from: the estimator for 42, the grid for 9,
  and 5 under held gaps;
- the unit implied by the letter's own marks;
- the unit implied by the window's marks;
- each as a ratio over the path's unit.

It prints the same for the 363 right letters. By the window's marks:

| window marks over the path's unit | sure wrong | sure right |
|---|---|---|
| over 1.25 | 8 | 2 |
| 0.80 to 1.25 | 28 | 263 |
| under 0.80 | 15 | 98 |

**Task 2, the change (`14f515bd`): kept.**
- `CwUnitEstimator.MarkUnit` works out the unit from marks alone: dits and dahs are split at
  the geometric mean, and no gap is used.
- `CwProbabilisticStream.Read` re-reads the window at that speed when the marks' unit is more
  than 1.25 times the path's.
- The mechanism, from the trace: `031838`'s estimator averaged a 55 ms dit mark with a 15 ms
  gap caused by a dropout inside a mark. It read the sender at 34 WPM, against dahs of 165 to
  220 ms.

Judged against the tree with G1 in:
- MET-CER-SURE: 56 of 419 to 47 of 421. By condition, TX-FARNS goes from 0.0233 to 0.0000,
  and "sender not stated" from 0.1541 to 0.1309. Synthetic is unchanged.
- MET-INVENTED: 56 to 47.
- Coverage: 0.7674 to 0.7907.
- MET-WBE: 58 to 52.
- Adjudicated readings: 13 of 13.
- V-11: no recording goes red. On `032050`, wrong-or-added goes from 4 to 5 while right goes
  from 33 to 35 and edits from 21 to 20.
- Reported: captures 38 of 51 and named floors 7 of 13. Elements were lost on unkeyed rows:
  `001952` from 103 to 86, `134712` from 31 to 22 (its adjudicated `N4L` holds).
- The split group goes from 27 to 19.

**Task 3, the exit round.**
- build: 0 errors;
- the four metrics as judged;
- the hand pairs: 14 of 14;
- captures 38 of 51, adjudicated 13 of 13, named 7 of 13;
- engine carry-forward: 175 of 178. The three failures are capture rows `013010` (48 to 47),
  `012823` (23 to 22) and `012922` (43 to 40).
- app carry-forward: 275 of 278, the dispatcher-loop loss, each type 8 of 8 alone;
- touched types: 24 of 25. `TheFiveToEightDecibelPlateauHolds` is the correctness phase's
  recorded red with the same numbers, 165 against 117. It tests `Elements`, which this unit
  did not change.

The transmit files print nothing against `7e209cb4`. `src` changes in two files:
- `CwUnitEstimator.cs`: G1 and `MarkUnit`;
- `CwProbabilisticStream.cs`: the re-read at the marks' speed.

Everything is pushed to `origin/main`.

**Decisions the session made for itself** (author's, overrulable):
1. The ratio is 1.25, the one bin edge past which wrong letters outnumber right. It acts one
   way only, because the fast side did not separate (15 wrong against 98 right).
2. V-11 is read as its text says: a capture going red on a requirement's metric. On that
   reading, `032050`'s one extra wrong letter, which came with two more right letters, does
   not reject the change.
3. The change was kept although elements fell on unkeyed captures that no metric can judge.

## 2. What the owner should expect

Yes. The decoder prints fewer wrong letters with confidence than it did this morning. On the 23
keyed recordings, the letters it printed sure and wrong or added fell from 67 to 47. It printed
15 more right letters than it did after the definition change, 359 to 374. The evidence is
`TheRequirementsAreMeasuredTests` against inferred keys, 426 then 421 sure letters over 473
sent. That evidence is an indication rather than proof (V-13).

What will look wrong but is not: the capture floors and the named floors are red. They count
characters, and merged letters make fewer, longer characters. Where it looks wrong and may be:
`001952` and `134712` read fewer elements, and neither has a key to say whether they were
letters.

## 3. What you should see

Sender `031838` now reads `...A MEAN OF 2 TT` where it read `...TEAH A MEAN TOF 2 TT`. The
single `T`s it chopped out of an `A`, a `D` and a `W` are fewer, because the decoder now times
that sender at the speed its dahs show rather than 34 WPM. 17:37's `DEWTEETEEERE` is now
`DEWB6 RE`. `003758` still reads `EETMP/4` for `AA4MP/4`, which this change was never going to
fix. `032129` still reads mostly `E`s, because its speed comes from the grid and the grid is
wrong the other way.

## 4. What's blocking us

Nothing is blocking. Three questions:

1. **The floors, and the carry-forward line with them, are red on counts.** R78 reports a
   falling count and does not reject it, but 2.5 and HM-DEC-165 ask for green. Re-bank the 13
   rows at the new counts, or rule that an element loss on an unkeyed row is V-11 and take the
   speed change out?
2. **`001952` lost 17 of 103 elements, and there is no key** to say whether they were letters
   or litter. A key for it would settle it.
3. **R82 is written only in the work instruction.** `CW_SPEC.md` 11 still defines MET-COVERAGE
   as sure-emitted over sent, and `CwMetrics` now differs from it.
