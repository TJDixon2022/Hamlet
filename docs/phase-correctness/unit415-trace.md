# Where the space is decided - the trace behind unit 415's relabel

**Work instruction 415, task 1, 2026-09-24, at HEAD `26cd4ade`, `src` byte-identical to
`61d8b58f`.** Printed by `WhereTheSpaceIsDecidedTests` (two facts, neither asserts anything).
Raw output: `.run-unit/unit415-relabel-t1.txt` (first run, no centroid guard) and
`.run-unit/unit415-relabel-t1b.txt` (the run task 2 builds from). Every key is inferred (R61).

## Where kind 3 against kind 4 is read downstream of the path

Section 5 asked for everything after `DecodeAt` that reads the difference. What the tree holds:

| where | what it does with kind 3 against kind 4 | can a relabel move a letter there? |
|---|---|---|
| `CwProbabilisticDecoder.Spell`, 1321 to 1347 | both close the letter before them identically: same `EndHop` (the gap's start), same span ratio, same `SpanHops`. Kind 4 then adds one `" "` whose `EndHop` is the gap's **end** | no - the letters are built before the kind is looked at |
| `Judged`, 1247 to 1263 (the `CharacterMargin` gate, 1256) | passes every space unconditionally; judges each letter on its own marks | no - a letter's margin does not read the gap after it |
| `Decode`, 756 (`EndsInsideCharacter`) | kinds 3 and 4 alike count as "between characters" | no |
| `Decode`, speed choice, 738 to 750 | compares path scores | no - a relabel after the path changes no score |
| `CwProbabilisticStream.Read`, 525 to 537, the settle dedupe | a settled item advances `_settledThrough` to its own `EndHop`, so a settled **space** pushes it to the gap's end where a letter pushes it only to the key-up; any later read's item at or before it is never announced | **yes, in principle** - a space added or removed before this loop moves the mark a later letter is tested against |
| `CwProbabilisticStream.Skip`, 295 | advances the same mark | no, but the relabel's own mark must advance with it |
| `CwDecoder`, 114 to 133, the counters | skip `IsWordGap` | no |
| `CwReading` (tests, `CwScorer.cs` 19) unsure flags | one flag per character, a space is never unsure | no - spaces add or drop a `false` |
| word or dictionary prior | none exists in `src/Hamlet.RadioEngine/Cw`; nothing re-reads letters by word | no |
| `CwTranscript.Settle` (app) | appends what settled | no - it receives what the stream announces |

**So there is a place where letters cannot move: after the settle dedupe.** The stream keeps its
bookkeeping on the path's own characters exactly as today, and only the announcement of a
space is re-decided. A space the relabel removes still advances `_settledThrough`, and it is
simply not announced. A space the relabel adds is announced against a mark of its own, and it
never touches `_settledThrough`. Task 2 builds it there.

## Measured, not argued

The fact replays every recording through `CwDecoder` hop by hop, as the floors do. After each
read it re-reads the stream's window, speed and held gaps by reflection, reads the path ungated
at the same speed and gaps (the same path before `Judged`), and emulates the settle loop three
ways: as it is, with the relabel fed in before the loop, and with the relabel after it.

| | 52 recordings: 51 capture rows and 17:37 |
|---|---|
| emulation "as it is" equals what really settled | **52 of 52**, text and count |
| letters moved, relabel before the settle loop | **0** |
| letters moved, relabel after it | **0** |
| spaces added / removed at the starting boundary, guarded | 31 / 136 |

Letters are compared by moment and text, over named characters and placeholders. **The
premise holds on every recording in the tree. On these recordings even the placement before
the loop moves nothing, and the placement after it cannot by construction.**

## The centroid, and the one guard added

The relabel uses the character centroid `MeasureGaps` finds in that window when the element
and character heaps are 1.5 apart and have their trough, whether or not the word trough holds.

**The first run found the element heap standing in for it.** On `cw-2026-08-17-013347` the
"character centroid" measured 1.1 units in 7 of 9 reads, so every character gap read as a word
gap and `VA3VRR` became `VA 3V RRT`, 6 spaces added there and 142 added over all 52. The per-read
centroid runs continuously from 0.3 to 9 units across the tree, with a clump near 1 unit on
`013347`, `031838`, `032129`, `003901` and the low reads of the 09-24 captures.

**Guard, from `MeasureGaps`' own constants:** the boundary between the element and character
centroids, `sqrt(c0 * c1)`, must fall where `MeasureGaps` itself would take it without clipping,
1.3 to 2.6 units (`CwUnitEstimator.cs` 228 and 229). The guard adds no new number. With it,
reads with a centroid go from 1,903 to 1,449 of 2,912 over the 52, and `013347`'s region no longer moves.

## The boundary, from the heaps

Every gap between two marks at the read it settled, on the ten and 17:37, as a share of the
measured character centroid, with no key (guarded run):

```
0.8 to 1.3   42 31 31 32 29      the character heap
1.3 to 1.6   18 16 13            its shoulder
1.6 to 2.1    7  8  4  6  5      low and flat
2.1 to 2.4    7 10  8
2.4 to 2.5    0
2.5 to 3.0    5  8 12 11  3      word gaps
```

Split against the inferred keys, as unit 413 split them (the ten):

| kind | n with a centroid | span over centroid, quartile / median / quartile | span in units, median |
|---|---|---|---|
| inserted | 21 | 1.08 / 1.23 / 1.55 (first run) | 5.6 |
| joined | 40 | 0.86 / 0.97 / 1.27 (first run) | 4.2 |
| word kept | 10 | 2.70 / 2.99 / 4.93 (first run) | 12.0 |

**The starting boundary, `sqrt(7/3)` = 1.528 of the centroid, stands.** It sits on the heap's
falling shoulder, above three quarters of the joined boundaries and below every kept word gap
on the ten. Past it, 1.6 to 2.1 is a low flat plateau with no single emptiest place, so no
boundary is plainly better from the distributions. The keyed "at" rows in the raw output were
printed beside the choice. They were not used to make it.

## What would change, at 1.528, guarded

Per recording, in each direction, the flips are in `unit415-relabel-t1b.txt` as `flip |` lines,
each with its span, the centroid and the boundary. The largest movers are the 09-24 run:
`004405` 16 removed, `004322` 12, `004205` and `004427` 11, `004108` 8 added 1.
For example, on `004405`, `W 1 A W / 88 ■ O OR D IN A T` becomes `W1AW/88 ■OORDINAT`.

**What the trace predicts task 2 will meet.** On `cw-2026-08-17-134712` (HM-DEC-144, `N4L`) the
path reads `N4 L ZT`. The relabel removes the two spaces after `4` and `L`, and it adds one
after `N` from a centroid of 1.0 unit by the decode's speed that passed the guard by the
estimator's unit. The reading becomes `N 4LZT`. That region moves, so 3.2's third test is
expected to fail there. The prediction is recorded before the build and was not tuned away.
`013347`'s `VA3VRR` and `003758`'s `EETMP/4 QNIK` do not move under the guard.

## Task 2 - the relabel, built and judged under 3.2

Built as `4a0487b0`, the change alone: `CwUnitEstimator.MeasureCharacterGap`, `CwPathGap` and
`CwProbabilisticResult.Gaps` read off `Spell`'s output, and the stream's settle loop announcing
spaces after its dedupe. Taken out as `83e2dc7f`. Every key inferred.

| 3.2 test | entry | with the relabel | |
|---|---|---|---|
| 1. all keyed recordings, bench | 217 edits over 565 | **191 over 565** | pass |
| - baseline | 33 over 46 | 32 over 46 | |
| - 17:37 | 29 over 25 | 28 over 25 | |
| - outside | 124 over 363 | 122 over 363 | |
| - the ten, bench | 60 over 156 | 37 over 156 | |
| 2. named floors | 13 of 13 | 13 of 13, every count identical | pass |
| 3. the three adjudicated readings | `VA3VRR`, `N4 `, `EETMP/4 QNIK` | `VA3VRR`, ` 4L`, `EETMP/4 QNIK` | **fail**, `134712` |
| 4. all 51 capture rows | - | identical in named, elements and placeholders | pass |

**Task 1's claim held.** No named count, element count or placeholder count moved on any
row. The letters are the path's letters. The one failure is a space, as the trace predicted:
`134712`'s path reads `N4 L`, and the relabel takes the space out after `4` and puts one in
after `N`.

Beside it, never evidence (P8, 1.4), exact keys: the grid 102 to 100 over 243, the five-unit
row 63 to 64 over 81.

3.5 at both commits: captures 51 of 51, adjudicated 13 of 13, clean 2 of 2, engine
carry-forward 178 of 178. App carry-forward 276 and 277 of 278, with the rest lost to the
dispatcher loop and green alone.

## Task 3 - the narrowed relabel, which only removes spaces

It ran because task 2 failed only the third test. Built as `b68be0dd`: the same change, except
that a space the path read is left unannounced when its gap runs under the boundary, and no
space is ever added. Taken out as `040a4ae0`.

| 3.2 test | entry | narrowed | |
|---|---|---|---|
| 1. all keyed recordings, bench | 217 over 565 | **185 over 565** | pass |
| - baseline | 33 over 46 | 31 over 46 | |
| - 17:37 | 29 over 25 | 28 over 25 | |
| - outside | 124 over 363 | 118 over 363 | |
| - the ten, bench | 60 over 156 | 36 over 156 | |
| 2. named floors | 13 of 13 | 13 of 13, every count identical | pass |
| 3. the three adjudicated readings | `VA3VRR`, `N4 `, `EETMP/4 QNIK` | `VA3VRR`, **`N4L`**, `EETMP/4 QNIK` | **fail as written** |
| 4. all 51 capture rows | - | identical in named, elements and placeholders | pass |

**`134712` now reads `N4L`, which is exactly what HM-DEC-144 adjudicated, 1 edit to 0.** 3.2's
third test asks for the adjudicated readings to be unchanged character for character, so a
reading that moves onto the adjudicated text fails it as written. The change is out, and the
question of whether "unchanged" was meant to forbid that is in the report's section 4.

Beside it, never evidence, exact keys: the grid 100 over 243, the five-unit row 64 over 81.
3.5 at both commits: captures 51 of 51, adjudicated 13 of 13, clean 2 of 2, engine 178 of 178,
app 278 of 278 at the change. The take-out's round is task 4's exit round.
