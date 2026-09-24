# Where the words break - the trace behind 3.1

**Work instruction 413, task 1, 2026-09-24, at HEAD `af541f66`, `src` byte-identical to
`8a140c79`.** Printed by `WhereTheWordsBreakTests` (two facts, neither asserts anything); raw
output in `.run-unit/unit413-trace-t1.txt`. Every key is inferred (R61), so every "inserted"
below is a space where the **inferred** key has none.

## What decides a space

`CwProbabilisticDecoder.DecodeAt`, lines 1175 to 1177 and 1204 to 1206. The gap between
characters (kind 3) and the gap between words (kind 4) carry the **same evidence**, the key-up
likelihood over the same span, so which one the path takes is decided by the length penalty
alone: `want` for each kind, and the two penalties cost the same at the geometric mean of the
two wants. That mean is the **word-from** boundary printed below. The wants are one of two
things:

- **textbook**, `Kinds` lines 498 and 499, three and seven units at the stream's speed, word
  from 4.58 units - whenever `CwProbabilisticStream._structureHeld` is false;
- **the sender's own**, `CwProbabilisticStream._heldGaps` from `CwUnitEstimator.MeasureGaps`,
  once twelve reads running have come back separated.

## The inserted spaces

122 boundaries between two named characters inside the scored regions: the ten keyed
captures' 14 stretches and 17:37's region.

| | boundaries | inserted | missing | word kept | joined |
|---|---|---|---|---|---|
| the ten | 95 | **29** | 1 | 18 | 47 |
| 17:37 | 27 | **14** | 0 | 5 | 8 |

**On the ten, 27 of the 29 inserted spaces were read with the textbook wants in force**:
unit 55 ms at 21.8 wpm, character 165 ms, word 385 ms, word from 252 ms. The gaps that
split run 260 to 430 ms, one 510 and one 740, **4.7 to 7.8 units, median 5.6**. The
boundaries read right as joined sit at median 4.1 units, and the word gaps kept at median
11.6, lowest 5.0. **This sender's character gap is near five units, not three**, and the
textbook boundary at 4.58 falls inside it.

**On 17:37, 8 of the 14 were read under held gaps out of order** - character 552 or 828 ms
over word 295 or 250 - exactly unit 405's G1 finding, and the other 6 under held gaps whose
word boundary is clipped to its floor of 3.5 units.

## Why the sender's own gaps are not held on the ten

For every read (56 per capture), what `MeasureGaps` finds in that window:

| capture | held | separated | too few gaps | heaps too close | no trough | of those, at the word boundary only | out of order | longest run | refused centroids, units |
|---|---|---|---|---|---|---|---|---|---|
| 004108 | 0 | 16 | 0 | 1 | 39 | 28 | 1 | 6 | 1.1 / 3.2 / 8.8 |
| 004133 | 28 | 23 | 0 | 2 | 31 | 24 | 11 | 13 | 1.1 / 5.4 / 13.0 |
| 004205 | 0 | 4 | 3 | 0 | 49 | 48 | 1 | 1 | 1.1 / 4.9 / 10.7 |
| 004234 | 44 | 52 | 1 | 0 | 3 | 3 | 0 | 29 | 1.1 / 5.0 / 14.6 |
| 004322 | 4 | 26 | 0 | 0 | 30 | 24 | 0 | 15 | 1.1 / 4.7 / 11.2 |
| 004347 | 44 | 51 | 0 | 0 | 5 | 3 | 8 | 45 | 1.0 / 4.8 / 11.0 |
| 004405 | 0 | 16 | 2 | 0 | 38 | 31 | 0 | 5 | 1.1 / 4.8 / 10.7 |
| 004427 | 0 | 13 | 1 | 0 | 42 | 41 | 0 | 10 | 1.0 / 3.5 / 6.3 |
| 004510 | 0 | 25 | 2 | 0 | 29 | 27 | 0 | 8 | 1.0 / 4.8 / 19.6 |
| 004550 | 0 | 14 | 0 | 4 | 38 | 31 | 0 | 6 | 1.0 / 3.1 / 5.9 |
| 17:37 | 18 | 30 | 1 | 1 | 23 | 10 | 4 | 16 | 0.8 / 2.1 / 4.3 |

**The line that decides it is `CwUnitEstimator.cs` 216, `!IsTrough(gaps, centroids[1],
centroids[2])`.** The three heaps are found, in order and far apart - about 1.1, 4.8 and 11
units - and the element and character heaps have their trough. What fails is the trough
between the character and word heaps: 12 s of this sender holds a handful of word gaps, and
the test asks the word heap to outnumber the gaps standing near its boundary. On the ten, 304
of 320 refusals are no-trough and 260 of those fail at the word boundary only. The window is
then textbook, the run of twelve never completes, and `DecodeAt` splits at 4.58 units.

## The ten worst in the ten keyed captures

Ranked by the gap over the word-from boundary in force, smallest first: the splits made
closest to or inside the character side.

| capture | at | gap | thresholds in force | before | decode around, `_` a space | key so far |
|---|---|---|---|---|---|---|
| 004347 | 7.650 s | 210 ms, 3.7 units, 0.64 of word-from | unit 58 ms, held element 15 character 373 word 291, character clipped, word from 329 | `U` ends dah | `_W_ILL_BE_U_PLOADED_TO` | `ILL_BE_UP` |
| 004322 | 2.965 s | 260 ms, 4.7 units, 1.03 | unit 55 ms, textbook 165 and 385, word from 252 | `O` ends dah | `__P_O_N_S_ORED_A` | `THE_ARRL` |
| 004322 | 8.250 s | 260 ms, 4.7 units, 1.03 | textbook, word from 252 | `M` ends dah | `_S_ORED_A_M_ER_I_CA_2_` | `PONSORED_AME` |
| 004322 | 9.595 s | 260 ms, 4.7 units, 1.03 | textbook, word from 252 | `I` ends dit | `ED_A_M_ER_I_CA_2_5_9_O` | `SORED_AMERIC` |
| 004550 | 18.640 s | 260 ms, 4.7 units, 1.03 | textbook, word from 252 | `2` ends dah | `_3HB_DE_KA2_GJV_HW_ROB` | `DE_KA2G` |
| 004322 | 9.185 s | 265 ms, 4.8 units, 1.05 | textbook, word from 252 | `R` ends dit | `ORED_A_M_ER_I_CA_2_5_9` | `NSORED_AMERI` |
| 004322 | 18.915 s | 270 ms, 4.9 units, 1.07 | textbook, word from 252 | `T` ends dah | `_9_OP_ERA_T_I_ON_X_ALL` | `OPERATI` |
| 004322 | 19.345 s | 275 ms, 5.0 units, 1.09 | textbook, word from 252 | `I` ends dit | `_OP_ERA_T_I_ON_X_ALL_L` | `OPERATIO` |
| 004322 | 26.305 s | 275 ms, 5.0 units, 1.09 | textbook, word from 252 | `O` ends dah | `N_X_ALL_L_O_G_S_WILA` | `ALL_LOG` |
| 004510 | 19.835 s | 275 ms, 5.2 units, 1.14 | unit 53 ms, textbook 158 and 368, word from 241 | `2` ends dah | `__EEE__KA2__GJV_T_QNV_` | `KA2G` |

Every inserted line, 17:37's included, is in the raw output with its settled time. The element
before is a dah on 16 of the ten's 29 and a dit on 13: no lean either way.

## What it names for task 2

The dominant cause on the ten is not G1. It is the word-boundary trough refusing a sender
whose character gap is near five units. The trace names a change that keeps the element and
character trough as the evidence and, where the word heap is too sparse to show a trough of
its own, places the word gap at seven thirds of the measured character gap. Farnsworth
spacing stretches character and word gaps by the same factor, which keeps three to seven.
G1, the out-of-order held reading, is 17:37's cause and is the second candidate.
