# The running figure - step 2 and step 3

Every number is its metric, its condition, its count and the key's kind (§3.1). Real keyed
recordings: 23, every key inferred (V-13), no CH-* profile and no SNR in the 2500 Hz reference,
so no row is yet a requirement's own condition (unit 439's finding 1). Synthetic CQ set: 12,
exact keys, never sole evidence. Measured by `TheRequirementsAreMeasuredTests`. MET-CER-SURE's
denominator is the sure characters emitted, unit 439's reading of `CW_SPEC.md` 11.

**From unit 441, MET-COVERAGE is sure and right over sent (R82).** The figures below under unit
440 are as the spec wrote it then.

## Unit 446 - the speed search runs 5 to 45 WPM, not kept

Step 5, 5.1, HM-REQ-030 with HM-REQ-010, 011 and 012 as guards. The trace
(`TheSpeedSearchReachesBothEndsTests.EveryBoundOnTheSpeedSearch`) named eight bounds that stop an
end: stopping 5, `SlowestWpm` (`CwProbabilisticDecoder.cs` 466), the loop's start (763), the
stream's measured-speed range (`CwProbabilisticStream.cs` 432), the marks' re-read range (510) and
`CwDecoder.SlowestPlausibleWpm` (`CwDecoder.cs` 388); stopping 45, `FastestWpm` (487), the loop's
end (764) and the stream's range (433). `WpmStep` (509) puts neither end on a grid point. No window,
delay or span sized from a speed stops either end. The change: `SlowestWpm` 8 to 5, `FastestWpm` 40
to 45, the search tries `SpeedGrid` - the even speeds 6 to 44 and the two ends - and
`SlowestPlausibleWpm` 6 to 5. The diff is `.run-unit/unit446-speed-notkept.diff`, and `src` does
not carry it.

| part of R78 | before (HEAD `7e9f3161`) | under the change | verdict |
|---|---|---|---|
| MET-CER-SURE, real, inferred | 45 of 419, 0.1074 | 50 of 419, 0.1193 | **rises** |
| MET-CER-SURE, synthetic, exact | 14 of 173, 0.0809 | 14 of 173, 0.0809 | unchanged |
| MET-INVENTED, real, inferred | 45 over 473 (4 added, 41 wrong), 0.0951 | 50 over 473 (4 added, 46 wrong), 0.1057 | **rises** |
| MET-INVENTED, synthetic, exact | 14 over 252 (6 added, 8 wrong) | 14 over 252 (6 added, 8 wrong) | unchanged |
| sure-and-right coverage, real, inferred | 374 over 473, 0.7907 | 369 over 473, 0.7801 | **falls** |
| sure-and-right coverage, synthetic, exact | 159 over 252, 0.6310 | 159 over 252, 0.6310 | holds |
| MET-WBE, real, inferred | 52 over 113, 0.4602 | 53 over 113 (43 inserted, 10 deleted), 0.4690 | **rises** |
| adjudicated readings | 13 of 13 | 13 of 13 | hold |
| V-11, 35 recordings, four metrics | - | 2 worse: `134712` wrong-or-added 0 to 1, right 3 to 1; `031838` wrong-or-added 5 to 9, right 16 to 13, boundaries wrong 1 to 2 | **fails** |
| 5.1: 5 WPM case, exact | shown 8, MET-CER-SURE 35 of 51, MET-INVENTED 35 over 21, coverage 16 over 21 | shown 5, 10 of 29, 10 over 21, 19 over 21 | does what 5.1 asks |
| 5.1: 45 WPM case, exact | shown 36, 20 % slow; 0 of 21, 0 over 21, 21 over 21 | shown 44, 2.2 % slow; the same | does what 5.1 asks |
| capture rows | 51 of 51 | 48 of 51; `134712` elements 22 to 18, `012823` 36 to 35, `003126` 131 to 127 | reported |
| named floors | 12 of 13, 17:37 at 38 | 11 of 13; `134712` 8 to 7, 17:37 at 38 | reported |

Per condition, key's kind beside each, before -> after. Real, sender not stated (20 recordings),
inferred: MET-CER-SURE 45 of 357, 0.1261 -> 50 of 357, 0.1401; MET-WBE 45 of 97 -> 46 of 97.
TX-FARNS, TX-ITU and TX-TIGHT, inferred: 0 of 43, 0 of 13 and 0 of 6 -> the same; MET-WBE 7 of 11,
0 of 4 and 0 of 1 -> the same. Every synthetic condition, exact: unchanged on all four metrics.

The four speed cases, `SyntheticCq`'s CQ at PARIS timing, 15 dB, exact key `CQ CQ CQ DE N0CALL
N0CALL K`, cold, speed shown after the first sure letter as a median:

| case | before: text, speed | after: text, speed |
|---|---|---|
| 5 WPM | `TTC Q CTQ TTCTTTQ DE TT MMET■EATTTT TA TTTT TTTL TNTTEIZI■■CALL K`, 8 | `TTCQ CQ CQ DE NNN■ACALL NNIBIL■CALL K`, 5 |
| 8 WPM | `C Q CQ CQ DE NGWCALL NGGWCALL K`, 8 | `CQ CQ CQ DE NGWCALL NGGWCALL K`, 8 |
| 40 WPM | `CQ CQ CQ DE N0CALL N0CALL K`, 40 | the same, 40 |
| 45 WPM | `CQ CQ CQ DE N0CALL N0CALL K`, 36 | the same, 44 |

Decode time: the engine carry-forward line 376 s -> 373 s, the app line 166 s -> 166 s, the
captures type 131 s -> 130 s; `cw-2026-08-18-004507`, 0.50 min, three runs 5721, 5618, 5831 ->
4849, 4785, 4777 ms per minute of audio, median 5721 -> 4785.

**Not kept: real MET-CER-SURE and MET-INVENTED rise 45 to 50, coverage falls 374 to 369, MET-WBE
rises 52 to 53, and V-11 fails on two recordings.** Thirteen recordings' texts change (`.run-unit/unit446-text-change.sorted.txt` against
`unit446-text-before.sorted.txt`); `134712`
loses its callsign, `N4LQ K` -> `K ■ LQ K`. The synthetic set is unmoved, so what reddened the real
ones is where a real recording's measured or marks' speed fell between 5 and 8 or between 40 and
45 and is now taken, or where 5, 6, 42, 44 or 45 won the grid; which of those it is was not
measured.

## Unit 445 - a letter with an inner gap past 6.5 units either way prints dim, kept

Step 3, 3.2, HM-REQ-011 with HM-REQ-010 and 012 as guards. The trace
(`WhatTheSureLettersMarksLookLikeTests`) read every sure letter's whole envelope marks and the gaps
between them over its span one unit either side, each as a ratio of one or more from the nearer of
1 and 3 units (gaps from 1 unit), at the unit the path was read with. Real, inferred, 421 sure (374
right, 43 wrong, 4 added): worst mark, no edge (right letters in every bin, 8 with no whole mark);
worst inner gap, edge 6.5, past it 0 right and 2 wrong, both `032129` (17.400 s `T` read `E`, 10.0;
20.660 s `0` read `E`, 15.0), each on a 10 ms key-up at a grid unit of 100 and 150 ms; together, no
edge. Synthetic, exact, 173 sure: no wrong-or-added letter past the farthest right one on any
measure (mark 1.676, gap 3.375), so no edge there and nothing either side of 6.5. The change:
`CwProbabilisticStream.Character` emits `Low` where a known letter's worst inner gap is past 6.5.

| part of R78 | before (HEAD `0528afbe`) | under the change | verdict |
|---|---|---|---|
| MET-INVENTED, real, inferred | 47 over 473 (4 added, 43 wrong), 0.0994 | 45 over 473 (4 added, 41 wrong), 0.0951 | falls |
| MET-INVENTED, synthetic, exact | 14 over 252 (6 added, 8 wrong) | 14 over 252 (6 added, 8 wrong) | unchanged |
| MET-CER-SURE, real, inferred | 47 of 421, 0.1116 | 45 of 419, 0.1074 | falls |
| MET-CER-SURE, synthetic, exact | 14 of 173, 0.0809 | 14 of 173, 0.0809 | unchanged |
| sure-and-right coverage, real, inferred | 374 over 473, 0.7907 | 374 over 473, 0.7907 | holds |
| sure-and-right coverage, synthetic, exact | 159 over 252, 0.6310 | 159 over 252, 0.6310 | holds |
| adjudicated readings | 13 of 13 | 13 of 13 | hold |
| V-11, 35 recordings, four metrics | - | 0 worse; `032129` wrong-or-added 12 to 10, right 15 held | holds |
| MET-WBE, real, inferred | 52 over 113, 0.4602 | 52 over 113, 0.4602 | unchanged |
| dim precision (HM-REQ-014), real, keyed stretches | no dim letter | 0 right of 2 dim, 0.0000 | reported |
| capture rows | 51 of 51 | 51 of 51 | hold |
| named floors | 12 of 13, 17:37 at 38 | 12 of 13, 17:37 at 38 | as at entry |

Per condition, key's kind beside each, before -> after:

| condition | key | MET-INVENTED | MET-CER-SURE |
|---|---|---|---|
| real, sender not stated (20 recordings) | inferred | 47 over 410 (4 added, 43 wrong) -> 45 over 410 (4 added, 41 wrong) | 47 of 359, 0.1309 -> 45 of 357, 0.1261 |
| real, TX-FARNS | inferred | 0 over 44 -> 0 over 44 | 0 of 43 -> 0 of 43 |
| real, TX-ITU | inferred | 0 over 13 -> 0 over 13 | 0 of 13 -> 0 of 13 |
| real, TX-TIGHT | inferred | 0 over 6 -> 0 over 6 | 0 of 6 -> 0 of 6 |
| synthetic, ITU 0 dB | exact | 0 over 63 -> 0 over 63 | no sure letter -> no sure letter |
| synthetic, ITU 5 dB | exact | 1 over 63 (1 added) -> the same | 1 of 63, 0.0159 -> the same |
| synthetic, ITU 15 dB | exact | 1 over 63 (1 added) -> the same | 1 of 64, 0.0156 -> the same |
| synthetic, char gap 5, 0 dB | exact | 0 over 21 -> 0 over 21 | no sure letter -> no sure letter |
| synthetic, char gap 5, 5 dB | exact | 11 over 21 (4 added, 7 wrong) -> the same | 11 of 25, 0.4400 -> the same |
| synthetic, char gap 5, 15 dB | exact | 1 over 21 (1 wrong) -> the same | 1 of 21, 0.0476 -> the same |

**Kept: MET-INVENTED falls and nothing in R78 gets worse.** One recording's text changes, `032129`:
`E EE EIIEE I E EE IEEEE` -> `E EE [E]IIEE I [E] EE [I]EEEE`. The two dim `E`s are the key's `T` and
`0`; the dim `I` is outside the keyed stretch and has no key. No callsign is gained or lost on any
capture or synthetic case. The edge is narrow: two letters, both on 10 ms key-ups at slow grid
units, so it says as much about the grid's unit as about the letter's shape.

## Unit 444 - key-ups under half a dit left out of the gap clustering, not kept

Step 2, 2.5, HM-REQ-080/081 on 17:37 with HM-REQ-010/011 as guards. The trace
(`HowSeventeenThirtySevensGapsAreCalledTests`, at f14b2453 and at HEAD, both measured) found G1
lost two letter spaces on 17:37, 6|R at 24.030 s (310 ms, 4.43 u) and W|B at 26.830 s (320 ms,
4.57 u). In every window where G1's condition held, a single key-up of 10 to 15 ms (0.14 to 0.21 u,
1 of 44 gaps) took the shortest cluster, so no character gap was measured and the stream stood on
a word threshold of 303 ms or 254 ms. The change: `CwUnitEstimator.MeasureGaps` and
`MeasureCharacterGap` cluster only key-ups of at least half the unit. G1 is untouched. The diff is
`.run-unit/unit444-wbe-notkept.diff`, and `src` does not carry it.

| part of R78 | before (HEAD `1f6a5789`) | under the change | verdict |
|---|---|---|---|
| MET-WBE, 17:37, inferred | 7 over 6 words (6 inserted, 1 deleted) | 8 over 6 words (7 inserted, 1 deleted) | **rises; 5 or fewer needed** |
| MET-WBE, real, inferred | 52 over 113 (43 inserted, 9 deleted), 0.4602 | 43 over 113 (36 inserted, 7 deleted), 0.3805 | falls |
| MET-CER-SURE, real, inferred | 47 of 421, 0.1116 | 46 of 425, 0.1082 | falls |
| MET-INVENTED, real, inferred | 47 over 473 (4 added, 43 wrong) | 46 over 473 (4 added, 42 wrong) | falls |
| sure-and-right coverage, real, inferred | 374 over 473, 0.7907 | 379 over 473, 0.8013 | rises |
| MET-CER-SURE, synthetic, exact | 14 of 173, 0.0809 | 4 of 170, 0.0235 | falls |
| MET-WBE, synthetic, exact | 48 over 84, 0.5714 | 52 over 84, 0.6190 | rises |
| adjudicated readings | 13 of 13 | 13 of 13 | hold |
| V-11, 35 recordings | - | 17:37 boundaries 7 to 8, `031905` 2 to 3, `cq-18wpm-5db-char5` 10 to 14 | **fails** |
| 17:37 wrong-or-added, sure-and-right | 3, 17 | 3, 17 | G1's letters stay removed |
| capture rows | 51 of 51 | 49 of 51; `004234` 36 to 34, `004427` 42 to 41 | reported |
| named floors | 12 of 13 | 12 of 13; 17:37 at 38 | reported |

Per condition, real, inferred keys, before -> after. Sender not stated (20 recordings): MET-CER-SURE
47 of 359, 0.1309 -> 46 of 363, 0.1267; MET-WBE 45 of 97, 0.4639 -> 36 of 97, 0.3711. TX-FARNS:
MET-CER-SURE 0 of 43 -> 0 of 43; MET-WBE 7 of 11 -> 7 of 11. TX-ITU: 0 of 13 -> 0 of 13; 0 of 4 ->
0 of 4. TX-TIGHT: 0 of 6 -> 0 of 6; 0 of 1 -> 0 of 1. Synthetic, exact keys: character gap 5 units
at 5 dB MET-CER-SURE 11 of 25 -> 1 of 22, MET-WBE 10 of 7 -> 14 of 7; every other synthetic
condition unchanged on both.

**Not kept: 17:37's boundaries wrong rise 7 to 8, and V-11 fails on three recordings.** 17:37 reads
`CQ CQ CQ DEWB 6 RE D W B 7E E I`: 6|R and W|B are still read as word spaces, and B|6 at 23.005 s
(250 ms, 3.57 u) is now one too. The rule is recorded here as the trace set it, and it was not moved
after these numbers. Elsewhere it read `004234`'s `T HANTT TK■ET FOR` as `THANK YOU FOR`.

17:37 before G1 `CQ CQ CQ DEWTEETEEERE D ETTTB 7E E I`, at HEAD `CQ CQ CQ DEWB6 RE D W B 7E E I`,
under the change `CQ CQ CQ DEWB 6 RE D W B 7E E I`; key `CQ CQ CQ DE WB6RED WB6RED`, inferred.

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
