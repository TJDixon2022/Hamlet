READ IN THIS ORDER.

A. The phase goal, Hamlet reads a CQ call correctly, and where each step
   stands: 0 done, 1 done, 2 done, 3 partial, 4 not started, 5 Tim's,
   6 partial.
B. Step 3: 3.2 the relabel kept or out on each of its four tests, 3.3
   17:37 before and after and the running total, 3.4 the count of units
   without a kept change, 3.5 every commit's exit - each met or not,
   with the number.
   - 3.2 not met. Both changes passed tests 1, 2 and 4 and failed test 3
     at `cw-2026-08-17-134712`, so both are out. The relabel: all keyed 191
     edits over 565 characters against inferred keys, from 217. The
     narrowed relabel: 185 over 565.
   - 3.3 not met, nothing kept. 17:37 was 29 over 25 against an inferred
     key, would have been 28 over 25 under either change, and stays 29. The
     running total stays 217 over 565.
   - 3.4 open. This is the second of three step 3 units without a kept
     change.
   - 3.5 met and ticked. All four commits' exits were green.
C. Whether moving only the spaces left every letter where it was, weighed
   against A and B: **it did.** No named, element or placeholder count
   moved on any of the 51 capture rows under either change, and the named
   floors are 13 of 13 with every count identical. So the mechanism is
   sound: it removes the P6 wall, and it takes a quarter of the bench's
   edits off with no letter cost. Section 4 raises 1 item, and it is in
   the way of 3.2: the narrowed relabel failed only because
   `134712` moved onto its adjudicated text, `N4L`.

UNIT:       415 - complete at task 4 of 4, none dropped - 2026-09-24 02:28
PHASE GOAL: Hamlet prints what a station calling CQ actually sent, and the correctness number says so.
UNIT GOAL:  Re-decide only whether each gap the decoder already read between letters is a space, so no letter can move, and keep it under 3.2 or take it out with the measurement.
ADVANCED:   no - 3.2 not met, both changes out on its third test at 134712; 3.5 ticked on four green commit exits
NUMBER:     all keyed 217 edits over 565 characters against inferred keys, from 217; 17:37 29 over 25, from 29 - unchanged, nothing kept; built and out at 191 and 185 over 565, 17:37 28 over 25
DRIFT:      0 - 3.5 moved (was 0)

## 1. What Claude did

**Complete at task 4 of 4. None dropped: task 3 applied, because task 2 failed only the
third test.** On QUIVERFULL, `C:\Source\HamLet`, project Hamlet confirmed by the four gate
checks, branch `main`, every commit pushed. Nothing kept. `src` prints nothing against entry
`61d8b58f`.

**3.2's four tests, for each change built. Every key is inferred and every total is over the
bench readings.**

The relabel, `4a0487b0`, taken out at `83e2dc7f`. It removes and adds spaces at `sqrt(7/3)`
of the sender's character gap.

| 3.2 test | entry | after | |
|---|---|---|---|
| 1. total edits, all keyed recordings | 217 edits over 565 characters, inferred keys | **191 over 565**, inferred keys | pass |
| - baseline | 33 over 46 | 32 over 46 | |
| - 17:37 | 29 over 25 | 28 over 25 | |
| - outside | 124 over 363 | 122 over 363 | |
| - the ten, bench | 60 over 156 | 37 over 156 | |
| 2. named floors | 13 of 13 | 13 of 13, every count identical | pass |
| 3. the three adjudicated readings | `VA3VRR`, `N4 `, `EETMP/4 QNIK` | `VA3VRR`, ` 4L`, `EETMP/4 QNIK` | **fail** |
| 4. capture rows' named counts | 51 rows | identical | pass |

The narrowed relabel, `b68be0dd`, taken out at `040a4ae0`. It only removes spaces.

| 3.2 test | entry | after | |
|---|---|---|---|
| 1. total edits, all keyed recordings | 217 edits over 565 characters, inferred keys | **185 over 565**, inferred keys | pass |
| - baseline | 33 over 46 | 31 over 46 | |
| - 17:37 | 29 over 25 | 28 over 25 | |
| - outside | 124 over 363 | 118 over 363 | |
| - the ten, bench | 60 over 156 | 36 over 156 | |
| 2. named floors | 13 of 13 | 13 of 13, every count identical | pass |
| 3. the three adjudicated readings | `VA3VRR`, `N4 `, `EETMP/4 QNIK` | `VA3VRR`, **`N4L`**, `EETMP/4 QNIK` | **fail as written** |
| 4. capture rows' named counts | 51 rows | identical | pass |

**The named-count row for all 51 captures: identical** under both changes, in named
characters, named elements and placeholders (`.run-unit/unit415-captures-c1.norm` and
`-c2.norm` against `-entry.norm`, no diff).

On `134712` the unguarded path reads `N4 L ZT`. The relabel takes out the space after `4`,
and it adds one after `N` from a window whose centroid passed the guard. The narrowed relabel
only takes out, so the region reads `N4L`, which is the text HM-DEC-144 adjudicated: 1 edit
to 0. 3.2 asks for the adjudicated readings unchanged character for character, and this one
changed. See section 4.

**3.3, never evidence, beside it.** 17:37 over its scored region, against an inferred key:
29 over 25 at entry, 28 over 25 under both changes, 29 at exit. The synthetics, exact keys
(P8, 1.4): the grid was 102 over 243 at entry and 100 under both changes. The five-unit row
was 63 over 81 and 64 under both.

**3.5.** Every commit's exit was green:
- at `4a0487b0`: engine carry-forward 178 of 178; app 276 of 278 with 2 lost to the
  dispatcher loop, 2 of 2 alone; captures 51 of 51, adjudicated 13 of 13, clean 2 of 2;
- at `83e2dc7f`: engine 178; app 277 of 278 with 1 lost to the dispatcher loop and green
  alone; captures 51, adjudicated 13, clean 2;
- at `b68be0dd`: engine 178; app 278 of 278; captures 51, adjudicated 13, clean 2;
- at `040a4ae0`, task 4's exit round, below.

**Task 0.** Version 1.13.101 to 1.13.102. The `PHASE_OUTCOME.md` entry was written and
`PHASE_STATUS.md` names 415. The entry round matched section 5 exactly: 217 over 565, the
baseline 33 over 46, 17:37 29 over 25, outside 124 over 363, the ten 60 over 156, named
floors 13 of 13, captures 51 of 51. Engine 178 of 178. App 275 of 278, with three lost to
the dispatcher loop and 3 of 3 alone.

**Section 5 against the tree.**
- **No mismatch** in the stated lines. `Kinds` is at 493 to 500 and `want` at 1175 to 1177.
  The path is kept in `fromHop`/`kindAt`. `MeasureGaps` is at 174 and the word trough at 216.
- **Where kind 3 or kind 4 becomes text:** `Spell`, `CwProbabilisticDecoder.cs` 1321 to 1347.
  Both close the letter identically, and kind 4 adds a `" "` that ends at the gap's end.
- **Downstream readers:**
  - `Judged` (the `CharacterMargin` gate, 1256) passes every space.
  - `EndsInsideCharacter` (756) treats 3 and 4 alike.
  - Speed choice compares path scores.
  - `CwDecoder`'s counters skip word gaps.
  - `CwReading`'s unsure flag is never set on a space.
  - `CwTranscript.Settle` appends.
  - No word or dictionary prior exists.
- **The one reader that can reach a letter** is the stream's settle dedupe,
  `CwProbabilisticStream.cs` 525 to 537, plus `Skip` at 295. A settled space pushes
  `_settledThrough` to the gap's end, where a letter pushes it only to its key-up.
- **One small mismatch:** section 5 expected the app line to lose one or two names at entry.
  It lost three.

**Task 1, the trace.** `WhereTheSpaceIsDecidedTests` has two facts and asserts nothing. It
replays the 51 capture rows and 17:37, and after every read it emulates the settle loop three
ways from the stream's own window, speed and held gaps.
- The emulation of the loop as it is equals what really settled on **52 of 52** recordings.
- **Letters moved: 0 with the relabel placed before the dedupe, and 0 after it.** On this
  corpus even the unsafe placement moves nothing. The build uses the safe one: bookkeeping
  stays on the path's characters, and only a space's announcement moves.
- Raw output is in `.run-unit/unit415-relabel-t1.txt` and `-t1b.txt`, and the write-up in
  `docs/phase-correctness/unit415-trace.md`.

**Decisions I made myself (author's latitude, overrulable):**
1. **The centroid guard.** The first run found the element heap standing in for the
   character centroid: 1.1 units on `013347`, and `VA3VRR` read `VA 3V RRT`. I required the
   boundary between the element and character centroids to fall inside `MeasureGaps`' own
   clip range, 1.3 to 2.6 units (`CwUnitEstimator.cs` 228 and 229). That adds no new number.
   Reads with a centroid went from 1,903 to 1,449 of 2,912.
2. **The boundary stays at `sqrt(7/3)`**, 1.528 of the centroid. It sits on the character
   heap's falling shoulder. From 1.6 to 2.1 the heap is a low, flat plateau (7, 8, 4, 6, 5
   per 0.1) with no single trough, so no boundary is plainly better. The keyed "at" rows
   were printed beside the choice and were not used to make it.
3. **The centroid is measured per window**, from the same window the stream reads. The path's
   own label stands where none is measured.
4. **Only gaps with a mark on both sides are relabeled.** The gap before the window's first
   letter and after its last are cut by the window's edge.
5. **3.5 ticked.** The instruction says to tick it if every commit's exit was green, and all
   four of this unit's were. Unit 413 left it open with the reason "no change was kept". If
   3.5 is meant to wait for a kept change, it should be unticked.

**Task 4, the exit round at `040a4ae0`.**
- Hamlet.sln builds with warnings as errors.
- Engine carry-forward 178 of 178 and app 278 of 278.
- Every one of these is identical to entry: captures 51 of 51, adjudicated 13 of 13, clean
  synthetics 2 of 2, `TheNumberCannotBeGamedTests` 13 of 13, `TheBaselineIsScoredTests` at 33
  over 46 and 124 over 363, and `TheBenchmarkIsKeyedTests` at 60 over 156.
- `WhereTheSpaceIsDecidedTests` 2 of 2.
- The transmit files print nothing against `7e209cb4`. All of `src` prints nothing against
  `61d8b58f`.
- `PHASE_PLAN.md`: 3.5 ticked; 3.2, 3.3 and 3.4 open.
- `baseline.md`'s running total has unit 415's rows.

**Commits, all pushed to `origin main`, every push rc 0:**
- `26cd4ade` task 0;
- `413ea25c` task 1;
- `4a0487b0` the relabel;
- `83e2dc7f` its take-out;
- `3ddca565` task 2's record;
- `b68be0dd` the narrowed relabel;
- `040a4ae0` its take-out;
- `b3767d24` task 3's record;
- the exit commit carrying this report.

## 2. What the owner should expect

**The decoder reads exactly as it did.** Both changes are out, and `src` is byte-identical to
`61d8b58f`.

What is now known and was not before:
- **A spacing repair does not have to cost a letter.** Unit 413's two candidates changed the
  path, and the letters moved with the spaces (P6). This one leaves the path alone. It moved
  no named character, no element and no placeholder on any recording in the tree.
- **The narrowed version is the strongest measured spacing change so far.** All keyed
  recordings go from 217 to 185 edits over 565 against inferred keys, and the ten on the
  bench from 60 to 36 over 156. It went out on one reading only: it corrected `134712` to
  its adjudicated `N4L`, and 3.2 as written requires that reading unchanged.

What will look wrong but is not:
- 3.5 is ticked while 3.2 and 3.3 are open. 3.5 is about green exits, and every exit was
  green.
- The adjudicated floor type, `TheAdjudicatedReadingsKeepReadingTests`, stayed 13 of 13
  under both changes, yet 3.2's third test failed. That type asserts what each adjudicated
  reading must contain, and 3.2 asks for character-for-character identity. They are
  different tests.

## 3. What you should see

**The CW tab reads exactly as it did.** Nothing was kept.

This is what the narrowed relabel would have shown, from the capture at 00:43:22 on
2026-09-24. It is not on the tab now:

```
now:          P O N S ORED A M ER I CA 2 5 9 OP ERA T I ON X ALL L O G S
narrowed:     P O N S ORED AMERICA 25 9 OPERATION X ALL LOGS
```

And from 00:44:05: `W 1 A W / 88` would read `W1AW/88`. Every letter in both lines is the
same letter at the same moment. Only the spaces between them differ.

## 4. What's blocking us

**1. Does 3.2's third test forbid an adjudicated reading moving onto its own adjudicated
text?** This bears on 3.2. It is the only thing between the narrowed relabel and a kept
change, so it is raised here rather than parked. It does not halt the loop.

- **Ruling wanted:** whether "the three adjudicated readings are unchanged character for
  character" is met when a reading changes to exactly the text that was adjudicated. On
  `cw-2026-08-17-134712` that is `N4 ` becoming `N4L`, HM-DEC-144's text, 1 edit to 0.
- **Reasoning:** the third test exists so a change cannot buy total edits by damaging a
  reading somebody ruled on. A reading that becomes the ruled text has not been damaged. But
  the words say "unchanged", and this unit applied them as written. The baseline also notes
  `134712` as "retired as an anchor" while 3.2 still counts it as one of the three.
- **If yes:** the narrowed relabel, `b68be0dd`, passes all four tests on this unit's
  measurements: 185 over 565, 13 of 13 identical, adjudicated unchanged or onto their text,
  51 rows identical. A next unit could re-apply it as it stands.
- **If no:** it stays out. The next step 3 unit is the third of three under 3.4, and then the
  trace goes to `PARKED.md`.
- **Rejected:** narrowing the relabel further so `134712` does not move. That would choose
  the boundary or the guard from a score, which section 10 forbids. And no letter reason
  exists to treat that recording differently.

Nothing was written to `PARKED.md` this unit. P6, P7 and P8 were not touched.
