# The running figure - step 2 and step 3

Every number is its metric, its condition, its count and the key's kind (§3.1). Real keyed
recordings: 23, every key inferred (V-13), no CH-* profile and no SNR in the 2500 Hz reference,
so no row is yet a requirement's own condition (unit 439's finding 1). Synthetic CQ set: 12,
exact keys, never sole evidence. Measured by `TheRequirementsAreMeasuredTests`. MET-CER-SURE's
denominator is the sure characters emitted, unit 439's reading of `CW_SPEC.md` 11.

**From unit 441, MET-COVERAGE is sure and right over sent (R82).** The figures below under unit
440 are as the spec wrote it then.

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
