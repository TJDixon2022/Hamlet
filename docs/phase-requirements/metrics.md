# The running figure - step 2 and step 3

Every number is its metric, its condition, its count and the key's kind (§3.1). Real keyed
recordings: 23, every key inferred (V-13), no CH-* profile and no SNR in the 2500 Hz reference,
so no row is yet a requirement's own condition (unit 439's finding 1). Synthetic CQ set: 12,
exact keys, never sole evidence. Measured by `TheRequirementsAreMeasuredTests`. MET-CER-SURE's
denominator is the sure characters emitted, unit 439's reading of `CW_SPEC.md` 11.

**From unit 441, MET-COVERAGE is sure and right over sent (R82).** The figures below under unit
440 are as the spec wrote it then.

## Unit 443 - a letter starting inside one already said is not announced, not kept

Step 3, HM-REQ-011. The trace (`WhereTheSureAddedLettersComeFromTests`) put 439's 13 real sure
added at 1fb0bad6, 5 under G1 alone (42d5dbb9), 4 at HEAD: G1 removed 8, all on 17:37; the marks'
speed removed 1, `031838` at 21.355 s. The 4 left share no cause. The one cause shared by more than
one added letter, real or synthetic, is the same marks read twice: a letter whose span begins 8.9
to 10.6 units inside the letter settled before it (`004347` `L O OTW`, both 25 WPM synthetic `KK`).
`CwProbabilisticStream.Read` checks only where a re-read letter ends. The change dropped a letter
whose first mark began before the last settled letter's last mark ended. The diff is
`.run-unit/unit443-added-notkept.diff`, and `src` does not carry it.

| part of R78 | before | under the change | verdict |
|---|---|---|---|
| MET-INVENTED, real, inferred | 47 over 473 (4 added, 43 wrong), 0.0994 | 48 over 473 (3 added, 45 wrong), 0.1015 | **rises** |
| MET-INVENTED, synthetic, exact | 14 over 252 (6 added, 8 wrong) | 12 over 252 (4 added, 8 wrong) | falls |
| MET-CER-SURE, real, inferred | 47 of 421, 0.1116 | 48 of 420, 0.1143 | **rises** |
| MET-CER-SURE, synthetic, exact | 14 of 173, 0.0809 | 12 of 171, 0.0702 | falls |
| sure-and-right coverage, real, inferred | 374 over 473, 0.7907 | 372 over 473, 0.7865 | **falls** |
| sure-and-right coverage, synthetic, exact | 159 over 252, 0.6310 | 159 over 252, 0.6310 | holds |
| adjudicated readings | 13 of 13 | 13 of 13 | hold |
| V-11, 35 recordings | - | `032050` and `032129` each lose one sure-and-right | **fails** |
| MET-WBE, real, inferred | 52 over 113, 0.4602 | 53 over 113, 0.4690 | rises by one |
| capture rows | 51 of 51 | 31 of 51, 20 rows fall on counts | reported |

Per condition, real, inferred keys, MET-INVENTED before -> after: sender not stated (20
recordings) 47 over 410 (4 added, 43 wrong) -> 48 over 410 (3 added, 45 wrong); TX-FARNS 0 over 44
-> 0; TX-ITU 0 over 13 -> 0; TX-TIGHT 0 over 6 -> 0. MET-CER-SURE: not stated 47 of 359, 0.1309 ->
48 of 358, 0.1341; TX-FARNS, TX-ITU and TX-TIGHT 0 before and after.

**Not kept: MET-INVENTED rises on the real set.** It took out the doubled letters the operator
sees: `VA3VRRR` to `VA3VRR`, `NNOTT` to `NOT`, `QNIKK` to `QNIK`, `W1AW/88` to `W1AW/8` and
`OOTW` to `OTW`. It also took out letters whose first mark was shared but which carried new marks
after it: `200J6` to `20J6`, `MCON` to `MON`, `TN6TBRE` to `TN6TRE`.

## Unit 442 - sure only where the reading beat its rival, not kept

`CwProbabilisticStream.Character` set `Low` where the letter's margin over its nearest rival
reading (`CwProbabilisticDecoder.RivalMargin`) was under 1 nat. The trace set no edge: right
letters outnumber wrong or added below every edge (0 to 0.5 nats 4 to 13, 0.5 to 1 3 to 3), and
13 of the 47 sit above 128 nats. Built at 1 nat anyway, the top of the one bin not outnumbered.
The diff is `.run-unit/unit442-dim-notkept.diff`; `src` does not carry it.

| part of R78 | before | under the change | verdict |
|---|---|---|---|
| MET-CER-SURE, real, inferred | 47 of 421, 0.1116 | 40 of 398, 0.1005 | falls |
| MET-CER-SURE, synthetic, exact | 14 of 173, 0.0809 | 11 of 166, 0.0663 | falls |
| MET-INVENTED, real, inferred | 47 over 473 | 40 over 473, 0.0846 | falls |
| MET-COVERAGE, real, inferred | 374 over 473, 0.7907 | 358 over 473, 0.7569 | **falls** |
| MET-COVERAGE, synthetic, exact | 159 over 252, 0.6310 | 155 over 252, 0.6151 | **falls** |
| adjudicated readings | 13 of 13 | 13 of 13 | hold |
| V-11, 35 recordings | - | 8 lose sure-and-right, `032113` 21 to 12 | **fails** |
| dim precision (HM-REQ-014), real | - | 16 right of 23 dim, 0.6957 | reported |

**Not kept: coverage falls.** Every right letter dimmed costs one, and 16 right were dimmed for 7
wrong or added.

## Unit 441 - the marks' speed, kept

`CwProbabilisticStream.Read` re-reads a window at the speed its marks alone imply
(`CwUnitEstimator.MarkUnit`) when that unit is more than 1.25 times the path's. The ratio is the
bin edge in the trace where sure-wrong outnumber sure-right, 8 to 2, against 28 to 263 between
0.80 and 1.25.

| part of R78, R82's coverage | G1 in | under the change | verdict |
|---|---|---|---|
| MET-CER-SURE, real, inferred | 56 of 419, 0.1337 | 47 of 421, 0.1116 | falls |
| MET-CER-SURE, synthetic, exact | 14 of 173, 0.0809 | 14 of 173, 0.0809 | unchanged |
| MET-INVENTED, real, inferred | 56 over 473 (5 added, 51 wrong) | 47 over 473 (4 added, 43 wrong), 0.0994 | falls |
| MET-COVERAGE, real, inferred | 363 over 473, 0.7674 | 374 over 473, 0.7907 | rises |
| MET-COVERAGE, synthetic, exact | 0.6310 | 0.6310 | unchanged |
| adjudicated readings | 13 of 13 | 13 of 13 | hold |
| V-11, 35 recordings | - | none goes red; `032050` 4 to 5 wrong-or-added with 33 to 35 right, edits 21 to 20 | holds, reported |
| capture rows | 50 of 51 | 38 of 51; elements lost on unkeyed rows, `001952` 103 to 86, `134712` 31 to 22 | reported |
| named floors | 12 of 13 | 7 of 13 | reported |
| MET-WBE, real | 58 over 113 | 52 over 113, 0.4602 | falls |

Per condition, real: TX-FARNS (`004507`) MET-CER-SURE 0.0233 to 0.0000, coverage 0.9545 to
0.9773; sender not stated 0.1541 to 0.1309, coverage 0.7366 to 0.7610. No condition gets worse.

Every condition, MET-CER-SURE G1 in to under the change, with the key's kind (unit 442 wrote this
out from `.run-unit/unit441-metrics-g1.txt` and `-speed.txt`; nothing re-measured):

| condition | key | G1 in | under the change |
|---|---|---|---|
| real, TX-FARNS | inferred | 1 of 43, 0.0233 | 0 of 43, 0.0000 |
| real, TX-ITU | inferred | 0 of 13, 0.0000 | 0 of 13, 0.0000 |
| real, TX-TIGHT | inferred | 0 of 6, 0.0000 | 0 of 6, 0.0000 |
| real, sender not stated | inferred | 55 of 357, 0.1541 | 47 of 359, 0.1309 |
| synthetic, TX-ITU, 15 dB | exact | 1 of 64, 0.0156 | 1 of 64, 0.0156 |
| synthetic, TX-ITU, 5 dB | exact | 1 of 63, 0.0159 | 1 of 63, 0.0159 |
| synthetic, TX-ITU, 0 dB | exact | none sure, no number | none sure, no number |
| synthetic, character gap 5, 15 dB | exact | 1 of 21, 0.0476 | 1 of 21, 0.0476 |
| synthetic, character gap 5, 5 dB | exact | 11 of 25, 0.4400 | 11 of 25, 0.4400 |
| synthetic, character gap 5, 0 dB | exact | none sure, no number | none sure, no number |

| recording | G1 in | under the change |
|---|---|---|
| `031838` | `D  ■T TTT TEAH A MEAN TOF 2 TT` | `TT 2, AND  ■ W IAH A MEAN OF 2 TT` |
| `003758` | `EETMP/4 QNIK` | `EETMP/4 QNIK` (window ratio 0.88, not touched) |
| `032129` | `TJ26 PGOPAGATION EE EE EIIEE I E EE` | `TJ26 PGOPAGATION E EE EIIEE I E EE` |

What is left, 43 sure-wrong: none in a window whose marks say slower than the path; 8 read at a
grid speed the marks put two to four times too slow (`032129`), and 30 in windows where the
marks and the path agree within 1.25.

## Unit 441 - G1 kept (`cce7985d` re-applied unchanged)

| part of R78, R82's coverage | before | under G1 | verdict |
|---|---|---|---|
| MET-CER-SURE, real, inferred | 67 of 426, 0.1573 | 56 of 419, 0.1337 | falls |
| MET-CER-SURE, synthetic, exact | 24 of 180, 0.1333 | 14 of 173, 0.0809 | falls |
| MET-INVENTED, real, inferred | 67 over 473, 0.1416 | 56 over 473, 0.1184 | falls |
| MET-COVERAGE, real, inferred | 359 over 473, 0.7590 | 363 over 473, 0.7674 | rises |
| MET-COVERAGE, synthetic, exact | 156 over 252, 0.6190 | 159 over 252, 0.6310 | rises |
| adjudicated readings | 13 of 13 | 13 of 13 | hold |
| V-11, 35 recordings | - | none worse; 17:37 wrong-or-added 14 to 3, right 14 to 17 | holds |
| capture rows | 51 of 51 | 50 of 51; `004133` 28 to 25 named | reported |
| named floors | 13 of 13 | 12 of 13; 17:37 46 to 38 named | reported |
| MET-WBE, real (not in R78's list) | 57 over 113 | 58 over 113 | rises by one |

Per condition, real, sender not stated (20 recordings): MET-CER-SURE 0.1813 to 0.1541, coverage
0.7268 to 0.7366. Synthetic TX-ITU 15 dB: 0.1159 to 0.0156, coverage 0.9683 to 1.0000; 5 dB:
0.0615 to 0.0159, coverage 0.9683 to 0.9841. No condition gets worse.

Every condition, MET-CER-SURE before to under G1, with the key's kind (unit 442 wrote this out
from `.run-unit/unit441-metrics-r82.txt` and `-g1.txt`; nothing re-measured):

| condition | key | before | under G1 |
|---|---|---|---|
| real, TX-FARNS | inferred | 1 of 43, 0.0233 | 1 of 43, 0.0233 |
| real, TX-ITU | inferred | 0 of 13, 0.0000 | 0 of 13, 0.0000 |
| real, TX-TIGHT | inferred | 0 of 6, 0.0000 | 0 of 6, 0.0000 |
| real, sender not stated | inferred | 66 of 364, 0.1813 | 55 of 357, 0.1541 |
| synthetic, TX-ITU, 15 dB | exact | 8 of 69, 0.1159 | 1 of 64, 0.0156 |
| synthetic, TX-ITU, 5 dB | exact | 4 of 65, 0.0615 | 1 of 63, 0.0159 |
| synthetic, TX-ITU, 0 dB | exact | none sure, no number | none sure, no number |
| synthetic, character gap 5, 15 dB | exact | 1 of 21, 0.0476 | 1 of 21, 0.0476 |
| synthetic, character gap 5, 5 dB | exact | 11 of 25, 0.4400 | 11 of 25, 0.4400 |
| synthetic, character gap 5, 0 dB | exact | none sure, no number | none sure, no number |

17:37 before `CQ CQ CQ DEWTEETEEERE D ETTTB 7E E I`, under G1 `CQ CQ CQ DEWB6 RE D W B 7E E I`.

## Standing figure before unit 441 - no change kept

| metric | set | count | share | key |
|---|---|---|---|---|
| MET-CER-SURE | real | 67 wrong or added of 426 sure (54 substituted, 13 added) | 0.1573 | inferred |
| MET-INVENTED | real | 67 over 473 sent | 0.1416 | inferred |
| MET-COVERAGE | real | 426 sure over 473 sent, 359 right | 0.9006 | inferred |
| MET-WBE | real | 57 over 113 words | 0.5044 | inferred |
| MET-CER-SURE | synthetic | 24 of 180 sure (11 substituted, 13 added) | 0.1333 | exact |
| MET-INVENTED | synthetic | 24 over 252 sent | 0.0952 | exact |
| MET-COVERAGE | synthetic | 180 sure over 252 sent, 156 right | 0.7143 | exact |

## Unit 440 - G1, built in `cce7985d`, not kept

G1: `CwUnitEstimator.MeasureGaps` returns textbook gaps when the clipped character gap stands at
or past the word gap. Built against the split group, 27 of the 54 sure-but-wrong.

| part of R78 | before | under G1 | verdict |
|---|---|---|---|
| MET-CER-SURE, real, inferred | 67 of 426, 0.1573 | 56 of 419, 0.1337 | falls |
| MET-CER-SURE, synthetic, exact | 24 of 180, 0.1333 | 14 of 173, 0.0809 | falls |
| MET-INVENTED, real, inferred | 67 over 473 | 56 over 473 (5 added, 51 wrong) | falls |
| MET-COVERAGE, real, inferred, as `CW_SPEC.md` 11 writes it | 426 over 473, 0.9006 | 419 over 473, 0.8858 | **falls** |
| sure and right, real | 359 | 363 | rises |
| MET-COVERAGE, synthetic, exact | 180 over 252, 0.7143 | 173 over 252, 0.6865 | **falls** |
| sure and right, synthetic | 156 | 159 | rises |
| adjudicated readings | 13 of 13 | 13 of 13; `032012` `ARTICLESOR` to `ARTICLES OR`, its own text | hold |
| capture rows | 51 of 51 | 50 of 51; `004133` 28 to 25 named, all outside its scored stretches | reported |
| V-11 on the metrics | - | no recording's sure-wrong-or-added rises or sure-right falls | holds |
| MET-WBE, real (not in R78's list for step 2) | 57 over 113 | 58 over 113; 17:37 5 to 7 | rises by one |

**Not kept: MET-COVERAGE falls as the spec writes it.** Every one of the seven sure characters
G1 stops emitting is one the key calls wrong or added; sure-and-right rises. That is unit 439's
finding 4, a coverage that counts wrong sure characters. The metric is not redefined here.

17:37 before: `CQ CQ CQ DEWTEETEEERE D ETTTB 7E E I`; under G1: `CQ CQ CQ DEWB6 RE D W B 7E E I`;
key `CQ CQ CQ DE WB6RED WB6RED`, inferred.
