```
READ IN THIS ORDER.

A. Total edits over the ten keyed recordings, before and after, and whether
   any change was kept.
B. Step 3's criteria: 3.1 the trace, 3.2 the keep rule, 3.3 the running
   total, 3.5 the exit round.
C. The rest. Section 4 raises 3 items, none of them blocking.
```

**A.** The ten keyed captures on the bench: **60 edits over 156 characters against inferred
keys, before and after.** Across every keyed recording: **217 over 565 before and after.**
**No change was kept.** Two were built, each in its own commit, and each came back out in the
next. Candidate 1 took the ten to 46 and all keyed recordings to 207, but five capture rows
lost named characters. G1 took 17:37 from 29 to 11 and all keyed recordings to 193, but it
broke 17:37's named floor and one capture row.

**B.** 3.1 is met and ticked: the fault is traced to `CwUnitEstimator.cs` 216 by a fact that
asserts nothing. 3.2 was applied as written, and both changes are out. 3.3: 17:37 stays at 29
edits over 25, and the running total is in `baseline.md`. It stays open because nothing was
kept. 3.5 is green at exit, but see section 4 item 2 about the candidate commits.

**C.** Everything below.

```
UNIT:       413 - complete at task 4 of 4, none dropped - 2026-09-24 00:03
PHASE GOAL: Hamlet reads a CQ call off the air correctly, measured as edit distance against inferred keys, with a guard so going quiet does not count as reading well
UNIT GOAL:  Find why the decoder puts word gaps inside words, name the line, and build a repair judged on edits over every keyed recording without costing a named count
ADVANCED:   yes - 3.1 is met, the split traced to CwUnitEstimator.cs 216 and printed by WhereTheWordsBreakTests; no repair kept
NUMBER:     total edits over the keyed recordings: 217 -> 217; the ten keyed captures on the bench 60 -> 60 over 156, inferred keys
DRIFT:      0
```

## 1. What Claude did

**Complete at task 4 of 4, none dropped.** QUIVERFULL, Hamlet confirmed by the gate, branch
`main`. Six commits, each pushed with rc 0: `af541f66`, `6d4545ea`, `27c75b77`,
`99701fbe`, `687aab1a` and `b4ccab9e`. The report went in at `68313acc`, pushed with rc 0.

**Task 0, the record.** HM-DEC-172 is in `DECISIONS.md` with its row at the top of `CLAUDE.md`
§1. The version goes from 1.13.99 to 1.13.100. `PHASE_STATUS.md` names unit 413 and
`CURRENT_STEP: 3`. The `## UNIT 413 - STEP 3` entry is in `PHASE_OUTCOME.md`.

Section 5 against the tree:
- 51 capture floor rows.
- Eleven key files for the run. 004535's has nothing to score, so ten captures are scored.
- `baseline.md` carries the unsure-per-named column.
- `CwScorer.Kinds` counts spaces added and missing.
- `MeasureGaps` is `CwUnitEstimator.cs` 174. `CwProbabilisticStream.cs` 453 calls it with the unit from `CwUnitEstimator.Measure`.
- G1 survives as prose only, in `docs/phase-cw/unit405-reds.md` 245 and 80 to 85.

No mismatch with the instruction.

The entry round, with the build first and each type run alone:

| type | result | time |
|---|---|---|
| engine carry-forward | 178 of 178 | 394 s |
| app carry-forward | 276 of 278; two lost to the dispatcher loop before any assertion, 2 of 2 alone | 155 s |
| captures | 51 of 51 | 123 s |
| adjudicated | 13 of 13 | 29 s |
| synthetics | 2 of 2 | 2 s |
| named floors | 13 of 13 | |

**The number to beat**, every key inferred:

| set | edits | characters |
|---|---|---|
| baseline | 33 | 46 |
| 17:37 alone | 29 | 25 |
| outside the baseline | 124 | 363 |
| the ten captures on the bench | 60 | 156 |
| **every keyed recording** | **217** | **565** |

**Task 1, the trace (3.1).** `WhereTheWordsBreakTests` has two facts and asserts nothing. It
reads the stream's held gaps by reflection and changes nothing in `src`. `CwScore` now carries
the start of its scored region so each alignment step maps back to the character that settled.
Full write-up: `docs/phase-correctness/unit413-trace.md`.

- **What decides a space.** `CwProbabilisticDecoder.DecodeAt` 1175 to 1177. The gap between
  characters and the gap between words carry the same evidence, so the length penalty alone
  decides between them. A word starts at the geometric mean of the two expected gaps.
- **The count.** 122 boundaries between named characters. 29 inserted spaces on the ten and
  14 on 17:37.
- **The ten.** 27 of the 29 were read under the textbook 3 and 7 units: word from 4.58 units,
  252 ms at 21.8 wpm. The gaps they split on ran **4.7 to 7.8 units, median 5.6**. For
  comparison, correctly joined letters sat at median 4.1 and real word gaps at median 11.6.
  This sender's character gap is near five units.
- **Why the sender's own gaps were not used.** Every read's `MeasureGaps` verdict was printed.
  The three heaps are found in order at about 1.1, 4.8 and 11 units. On the ten, 304 of 320
  refusals are no-trough, and 260 of those fail only at the word boundary. **Named:
  `CwUnitEstimator.cs` 216, `!IsTrough(gaps, centroids[1], centroids[2])`.** Twelve seconds of
  this sender hold too few word gaps for that heap to show a trough, so the window falls back
  to textbook spacing.
- **17:37.** 8 of its 14 were read under held gaps out of order, character 552 or 828 ms
  against word 295 or 250. That is exactly G1's case.

**Task 2, candidate 1: kept or out? Out.** Built from what the trace names. `MeasureGaps`
keeps the element-and-character trough as its evidence. Where the word heap shows no trough,
it puts the word gap at seven thirds of the measured character gap, since Farnsworth spacing
stretches both gaps alike. Committed alone at `27c75b77` and taken out at `99701fbe`.
3.2's four tests:

1. **Total edits: 217 to 207** over every keyed recording. The ten went 60 to 46, the
   baseline 33 to 33, outside 124 to 128 and 17:37 29 to 29. Inserted spaces on the ten went
   29 to 16. `OPERATION`, `ALL LOGS W`, `OORDIN`, `KA2GJV` and `AA3SB` all read clean.
2. **Named floors: 13 of 13** before and after.
3. **Adjudicated readings:** `VA3VRR`, `N4 ` and `EETMP/4 QNIK`, identical character for
   character.
4. **Capture rows: 41 of 51.** Named characters fell on five rows: 021629 27 to 25, 002016 44
   to 41, 004027 40 to 39, 004133 30 to 28 and 004510 38 to 37. Named elements fell on five
   more. All 51 rows old beside new are in `.run-unit/unit413-rows-cmp-c1.txt`.

It fails the fourth test, so it is out.

**Task 3, G1: kept or out? Out.** A different mechanism, not a narrowing of candidate 1.
`MeasureGaps` refuses a clipped reading whose character gap lies past its word gap, so the
stream keeps the last gaps it stood behind. Committed alone at `687aab1a` and taken out at
`b4ccab9e`.

1. **Total edits: 217 to 193.** The baseline went 33 to 15, with 17:37 29 to 11 reading
   `CQ CQ CQ DEW B6 RE D W B 7E E I`. Outside went 124 to 115, with 032012 13 to 4. The ten
   went 60 to 63.
2. **Named floors: 12 of 13.** 17:37 fell from 46 named to 38 as its stray `E` and `T` joined
   into letters.
3. **Adjudicated readings:** identical.
4. **Capture rows: 50 of 51.** 004133 fell from 30 named to 25, while its named elements rose
   87 to 88. Rows in `.run-unit/unit413-rows-cmp-g1.txt`.

It fails the second and fourth tests, so it is out.

**Both candidates hit the same wall**, and it is parked as P6. Joining a split letter back
together lowers the named-character count the floors were set on.

**Task 4, the exit round.** `Hamlet.sln` builds with warnings as errors.

| type | result | time |
|---|---|---|
| engine carry-forward | 178 of 178 | 386 s |
| app carry-forward | 276 of 278; two lost to the dispatcher loop before any assertion, 5 of 5 alone counting the theory's cases | 178 s |
| captures | 51 of 51, every row identical to entry | 130 s |
| adjudicated | 13 of 13 | |
| synthetics | 2 of 2 | |
| TheNumberCannotBeGamedTests | 13 of 13 | |
| TheScorerCountsWhatAHandCountsTests | 20 of 20 | |
| TheBaselineIsScoredTests | 2 of 2, at 33 over 46 and 124 over 363 | |
| TheBenchmarkIsKeyedTests | 1 of 1, at 60 over 156 | |
| WhereTheWordsBreakTests | 2 of 2 | |

`git diff` over the transmit files against `7e209cb4` prints nothing. All of `src` prints
nothing against `8a140c79`.

**Decisions made for myself**, author's and overrulable:
- **The keep test's total is the sum of every keyed recording**: the baseline, the nine
  outside it and the ten on the bench, 217. PARKED P3 had tabled the ten outside the total.
  The sum is the stricter reading of "all keyed recordings", and the verdict on both
  candidates is the same either way.
- **The ten worst** are ranked by the gap over the word boundary in force, smallest first.
- **The trace also covers 17:37**, because 3.3 names it.

## 2. What the owner should expect

**The words are not less shattered yet.** The CW tab reads exactly as it did yesterday,
because both repairs went back out. Before and after on the bench: `OP ERA T I ON`. The trace
now shows why. Your 7.052 MHz contact spaced its letters about five dits apart. The decoder
calls anything longer than about four and a half dits a word gap unless it has measured the
sender's spacing, and twelve seconds of that sender never held enough word gaps to measure
it. The first repair read that stretch as `OPERATION` and cut the edits on the ten captures
from 60 to 46, but it was not kept. That is not a regression and not lost work. It is the
keep rule working: five older recordings lost a letter or more of what they read. **What will
look wrong but is not:** the two repair commits in the history are red on the capture floor.
3.2 requires each change in its own commit and its removal in the next, so red commits between
two green ones are expected.

## 3. What you should see

**No visible change.** This unit found where the words break and measured two repairs. It kept
neither, so the CW tab reads as it did before the unit. The answer to the question it was commissioned to ask: **the split
is decided at `CwUnitEstimator.cs` 216.** There the decoder refuses the sender's own
word-gap measurement because a 12-second window holds too few word gaps. It then falls back to
textbook spacing, and that puts a space inside every five-dit gap between letters.

## 4. What's blocking us

Nothing blocks this phase. All three items are parked in `docs/phase-correctness/PARKED.md`
under R65, and the loop goes on.

1. **The named floors refuse a repair that joins split letters (P6).** A split letter prints
   as several `E` and `T`. The floors were set on those readings, so reading the right letter
   lowers the named-character count, even where named elements hold or rise (004133 went
   87 to 88). Both of this unit's candidates cut edits, 217 to 207 and 217 to 193, and both
   fell on named characters for exactly this reason.
   - **Ruling, if wanted:** for a change that joins, let the cost check be the named-element
     count together with edits and unsure per named, rather than the named-character count.
   - **Reasoning:** the element count still catches a decoder that goes quiet, and it does
     not punish reading `B6` as two letters instead of seven.
   - **Rejected:** lowering any floor, and editing any key. Both are forbidden, and neither
     is asked for.
   - **Without a ruling:** 3.2 stands as written. This is unit one of three under 3.4.
2. **3.5 against 3.2's own-commit rule.** 3.5 asks for the floors green "at the exit of every
   commit of the step". 3.2 asks for each change in its own commit and its removal in the
   next, so a failing change is necessarily a red commit.
   - **Ruling, if wanted:** 3.5 reads as green at each unit's exit and after every kept
     change.
   - **Reasoning:** that is the only reading both criteria can meet together.
   - **Rejected:** measuring a candidate uncommitted. That would break 3.2's "own commit".
3. **Which total 3.2 judges.** This unit summed all three keyed sets, 217. PARKED P3 had kept
   the ten outside the baseline total.
   - **Ruling, if wanted:** confirm the sum.
   - **Reasoning:** the sum is the plain reading of "all keyed recordings", and it did not
     change either verdict.
   - **Rejected:** judging on the ten alone. Candidate 1 improves the ten and costs five
     other rows, and a ten-only total would hide that cost.
