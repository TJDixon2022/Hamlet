READ IN THIS ORDER.

A. Whether the relabel was kept, and the total: 217 -> 185 over 565 with the relabel
   kept, and 217 -> 167 over 565 with task 3's second change also kept.
B. Step 3's criteria: 3.2 kept, all four tests passed as numbers, ticked; 3.3 17:37
   29 -> 28 -> 19 over 25 and the running total in baseline.md, ticked; 3.4 the count
   resets, this unit kept two changes; 3.5 every commit's exit green, stays ticked.
C. The rest. Section 4 raises 1 item, none blocking.

```
UNIT:       416 - complete at task 4 of 4, none dropped - 2026-09-24 09:08
PHASE GOAL: Hamlet reads a CQ call off the air as it was sent, judged by an edit count against keys and finally by Tim at the radio
UNIT GOAL:  Put unit 415's space-only relabel back in unchanged, keep it under R66 with all four of 3.2's tests printed as numbers, and attack the next error kind if time allows
ADVANCED:   yes - 3.2 and 3.3 ticked, the phase's first two kept correctness gains, all keyed 217 to 167 over 565
NUMBER:     all keyed: 217 -> 167 over 565; the ten bench: 60 -> 35 over 156
DRIFT:      0
```

## 1. What Claude did

**Complete, task 4 of 4, none dropped.** Claude Code on QUIVERFULL, the development computer.
Project Hamlet claimed, confirmed by `SHACK_FACTS.md` and `CwProbabilisticDecoder.cs` present
and `CoreHMI.sln` and `MURC.sln` absent. Branch `main`. Nothing in this report is evidence
about the radio. Every key is inferred (R61, §0.0).

**Section 5 checks: no mismatch.** `b68be0dd` exists, and `040a4ae0` is its child and its
take-out. `git diff 040a4ae0 HEAD -- src` printed nothing. `unit415-trace.md` describes the
relabel, the centroid guard and the `sqrt(7/3)` boundary. `PHASE_PLAN.md` 3.2 carries R66's
clause. Entry reproduced 217 over 565, the ten 60 over 156, and 17:37 29 over 25.

**Task 0 (`c4353dbf`).** Version 1.13.102 to 1.13.103. `PHASE_STATUS.md` names unit 416 at
step 3. The `PHASE_OUTCOME.md` entry comes from the decision block. HM-DEC-173 is in
`DECISIONS.md`, and its row is at the top of `CLAUDE.md` §1. Entry round: engine
carry-forward 178 of 178. App 274 of 278: four were lost to the known dispatcher loop, and
their two types passed 11 of 11 alone. Captures 51 of 51, adjudicated 13 of 13, clean 2 of 2,
named floors 13 of 13.

**Task 1: the relabel went back in, and it was kept (`38158be6`, record `04fba4d3`).**
`git diff 3ddca565 b68be0dd` applied cleanly. It was committed unchanged, and `src` is
byte-identical to `b68be0dd`. It was cherry-picked, not rebuilt.

| 3.2 test | entry | unit 415 with `b68be0dd` | unit 416, `38158be6` | |
|---|---|---|---|---|
| 1. all keyed | 217 over 565 | 185 over 565 | **185 over 565** | pass |
| - baseline | 33 over 46 | 31 over 46 | 31 over 46 | |
| - the ten, bench | 60 over 156 | 36 over 156 | **36 over 156** | |
| - 17:37 | 29 over 25 | 28 over 25 | **28 over 25** | |
| - outside | 124 over 363 | 118 over 363 | 118 over 363 | |
| 2. named floors | 13 of 13 | 13 of 13, identical | 13 of 13, every count identical | pass |
| 3. adjudicated | `VA3VRR`, `N4 `, `EETMP/4 QNIK` | `VA3VRR`, `N4L`, `EETMP/4 QNIK` | `VA3VRR`, `N4L`, `EETMP/4 QNIK` | pass under R66 |
| 4. capture rows | 51 rows | identical | 51 of 51 identical in named, elements and placeholders | pass |

**R66 applies to one reading.** `cw-2026-08-17-134712` read `N4 ` before and reads `N4L`
after. That is exactly HM-DEC-144's adjudicated text, 1 edit to 0. `VA3VRR` and
`EETMP/4 QNIK` did not move. The 13 named floors: 013347 57, 134712 21, 004507 49, 003758 44,
031838 43, 031905 36, 031948 31, 032012 43, 032050 44, 032113 47, 032129 65, 012403 21,
173723 46. Every one is equal to its floor and identical to entry. Adjudicated 13 of 13 and
clean 2 of 2 at the change. **3.2 ticked.**

**Task 2 (`11a48b13`).** From the kept build, text in section 3. On all 52 recordings, the
text that really settled with spaces stripped is identical to entry's. baseline.md carries
the running total.

**Task 3: the next kind, traced and then built, and kept (trace `99342296`, change
`90840b1f`, record `3c796321`).** On the kept build, of 128 boundaries on the ten and 17:37:
joined 80, inserted 24, word kept 23, missing 1. **21 of the 24 remaining inserted spaces sat
in windows where no character gap was measured**, so the relabel left the path's label alone
there. All 13 on 17:37 were under held structure, at a median of 0.66 of the word boundary the
path itself was given. The change: where no character gap was measured, a space the path read
is not announced when its gap is shorter than the path's own word boundary. That boundary is
the geometric mean of the character and word gaps the path was given, held or textbook. It
adds no space, and the stream's bookkeeping is as before.

| 3.2 test | kept relabel | with `90840b1f` | |
|---|---|---|---|
| 1. all keyed | 185 over 565 | **167 over 565** | pass |
| - baseline | 31 over 46 | 22 over 46 | |
| - the ten, bench | 36 over 156 | 35 over 156 | |
| - 17:37 | 28 over 25 | **19 over 25** | |
| - outside | 118 over 363 | 110 over 363 | |
| 2. named floors | 13 of 13 | 13 of 13, every count identical to entry | pass |
| 3. adjudicated | `VA3VRR`, `N4L`, `EETMP/4 QNIK` | the same, none moved | pass |
| 4. capture rows | identical | 51 of 51 identical to entry | pass |

**Task 4, exit round (3.5).** `Hamlet.sln` builds with warnings as errors. Engine
carry-forward 178 of 178. App 277 of 278: `TheFavoritesAreChipsTests.ThreeChipsCostTheTopBandNothing`
was lost to the dispatcher loop, and it passed 4 of 4 alone. Captures 51 of 51, adjudicated
13 of 13, clean 2 of 2, `TheNumberCannotBeGamedTests` 13 of 13, `TheBaselineIsScoredTests`
2 of 2, `TheBenchmarkIsKeyedTests` 1 of 1, `WhereTheSpaceIsDecidedTests` 2 of 2. The transmit
files print nothing against `7e209cb4`, and `src/Hamlet.App` prints nothing against entry.
Only three files under `src/Hamlet.RadioEngine/Cw` changed.

**Decisions made for itself (author's, overrulable):**
1. **Cherry-picked, not rebuilt.** The diff applied cleanly and was proven byte-identical.
2. **`WhereTheSpaceIsDecidedTests` changed, test only.** It now prints the text that really
   settled. Its boundary split now walks that text instead of the old loop's list, because
   once the relabel was in, the two no longer lined up and the split would have been wrong.
   It also prints the boundaries that had no centroid.
3. **Task 3's change was built, not dropped.** The clock allowed it: the unit ran from 08:08 to
   09:08.
4. **3.3 ticked.** 17:37 is reported before and after both kept changes, and the running
   total is in baseline.md. If 3.3 is meant to stay open as a standing duty, untick it.

## 2. What the owner should expect

Words on the CW tab should stop breaking apart mid-word. `A M ER I CA` now reads `AMERICA`,
and `W 1 A W / 88` reads `W1AW/88`. No letter changed on any recording. The letters are the
same letters at the same moments, and only the spaces between them differ. Over every keyed
recording, errors went from 217 to 167 in 565 characters.

What will look wrong but is not. 17:37 still reads poorly: what is left there is wrong
letters, not spaces. One real word gap there, after `DE`, went with the inserted spaces
(`DEWTEE...`). The change was still kept, because 3.2 judges the total. The app line lost one
test to the known Avalonia dispatcher loop, and that test is green alone. The baseline's
unsure-per-named goes from 0 / 47 to 0 / 48 only because `134712`'s region now holds three
named letters, not two. Everything was pushed to `main`, and every push returned 0.

## 3. What you should see

Before and after, from the kept build, 00:43:22 on 7.052:

```
before:  P O N S ORED A M ER I CA 2 5 9 OP ERA T I ON X ALL L O G S WILA
now:     P O N S ORED AMERICA 25 9 OPERATION X ALL LOGS WILA
```

And 00:44:05:

```
before:  A N T H ONY L U S C RE K 8 Z T W 1 A W / 88 ■ O OR D IN A T
now:     A N T HONY LUSCRE K 8 Z T W1AW/88 ■OORDINAT
```

On the CW tab, words that came out as scattered letters now come out as words. Across every
keyed recording, errors fall from 217 to 167 in 565 characters. The 17:37 CQ call goes from
29 errors to 19 in its 25 characters.

## 4. What's blocking us

Nothing blocks. One item, parked as P9 in `docs/phase-correctness/PARKED.md`, and the loop
goes on:

**1. Whether step 3 closes.** 3.1, 3.2, 3.3 and 3.5 are ticked. 3.4 only applies after three
units with nothing kept. What is left on 17:37 is letters. At exit, the boundaries on the ten
and 17:37 are 90 joined, 22 word kept, 14 inserted and 2 missing. The arbiter decides whether
step 3 is marked done or another unit takes on the joined gaps.

### Asks still outstanding

None. Unit 415's one ask, whether 3.2's third test forbids a reading moving onto its own
adjudicated text, was answered by R66 (HM-DEC-173) and is dropped.
