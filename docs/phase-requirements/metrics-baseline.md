# The requirement metrics, baseline at unit 439

Work instruction 439, tasks 3 and 4 (PHASE_PLAN.md 1.1 to 1.3), run by hand outside the loop on
2026-09-25 at HEAD `efdad2bc`, which carries no change under `src` from the entry `7734ecbe`.
Printed by `TheRequirementsAreMeasuredTests` (engine, 2 facts, 61 s) and copied here from its
output, `.run-unit/unit439h-metrics-measure.txt`. **Every real key is inferred** and disagreement
with one is not by itself proof the decoder is wrong (V-13). **Every synthetic key is exact** and no
synthetic case is ever sole evidence (12.5).

## The definitions, quoted from `CW_SPEC.md` section 11

- **MET-INVENTED** - (sure insertions + sure substitutions) / characters sent.
- **MET-CER-SURE** - MET-CER over characters emitted as sure; dim and placeholders excluded, not counted wrong.
- **MET-COVERAGE** - Sure characters emitted / characters sent.
- **MET-WBE** - Word gaps inserted or deleted relative to truth / words sent. Scored on boundaries alone, separately from MET-CER.

How `CwMetrics` reads them: a character is one symbol, a prosign included, and characters sent
exclude spaces. The character alignment has the spaces taken out of both sides and is `CwScorer`'s
Levenshtein over the symbols. MET-CER-SURE's denominator is the sure characters emitted, which is
how HM-REQ-010 reads it ("one in a hundred sure characters wrong"), and its numerator is the sure
ones wrong or added. MET-COVERAGE is the ratio as written, which exceeds one where sure characters
are added; its sure-and-right count is printed beside it. The decoder emits two classes, sure
(`High`) and placeholder (`Unreadable`). A named character below high would be counted apart and
never as sure; there are none today, and no dim class is invented.

## Conditions

`CW_SPEC.md` section 4 states a condition as a channel profile, a sender profile and an SNR in the
2500 Hz reference. **No real recording here has all three.** None was received on a named `CH-*`
channel, and none carries an SNR in the reference bandwidth. Each is grouped by its sender where
section 10 names the capture (013347 TX-TIGHT, 012403 TX-ITU, 004507 TX-FARNS), and by "sender not
stated" otherwise. The synthetic set is labeled as generated: no fading, a shaped noise band that is
not shown to be `CH-AWGN`, a known sender and an in-passband level that is not restated in the
reference. **So no number below is yet a measurement of a requirement at its own condition.** It is
the metric over the corpus the tree has.

## Totals

    total | real | MET-CER-SURE 67 wrong or added of 426 sure (54 substituted, 13 added), 0.1573 | MET-COVERAGE 426 sure over 473 sent (359 right), 0.9006 | MET-WBE 57 (42 inserted, 15 deleted) over 113 words, 0.5044 | inferred keys
    total | synthetic | MET-CER-SURE 24 wrong or added of 180 sure (11 substituted, 13 added), 0.1333 | MET-COVERAGE 180 sure over 252 sent (156 right), 0.7143 | MET-WBE 49 (19 inserted, 30 deleted) over 84 words, 0.5833 | exact keys
    total | real keyed recordings | 67 invented (13 sure added, 54 sure wrong) over 473 sent | inferred keys | 23 of 23 recordings measured | share 0.1416
    total | old count | 17 added named characters, 8 single-element, over the same recordings and stretches
    total | synthetic CQ set | 24 invented (13 sure added, 11 sure wrong) over 252 sent | exact keys | never sole evidence

## Per condition

MET-INVENTED:

    condition | real | condition | key | invented | sure added | sure wrong | sent | share | recordings measured | recordings with no number
    condition | real | real HF, no CH-* profile, SNR_2500 not measured, sender TX-FARNS (CW_SPEC.md 6.4 and 10, HM-DEC-115's traffic net) | inferred | 1 | 0 | 1 | 44 | 0.0227 | 1 | 0
    condition | real | real HF, no CH-* profile, SNR_2500 not measured, sender TX-ITU (CW_SPEC.md 10, the KD0UN capture) | inferred | 0 | 0 | 0 | 13 | 0.0000 | 1 | 0
    condition | real | real HF, no CH-* profile, SNR_2500 not measured, sender TX-TIGHT (CW_SPEC.md 10, HM-DEC-101) | inferred | 0 | 0 | 0 | 6 | 0.0000 | 1 | 0
    condition | real | real HF, no CH-* profile, SNR_2500 not measured, sender not stated in CW_SPEC.md | inferred | 66 | 13 | 53 | 410 | 0.1610 | 20 | 0
    condition | synthetic | condition | key | invented | sure added | sure wrong | sent | share | recordings measured | recordings with no number
    condition | synthetic | synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 0 dB in the passband (not restated in the 2500 Hz reference) | exact | 0 | 0 | 0 | 63 | 0.0000 | 3 | 0
    condition | synthetic | synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 15 dB in the passband (not restated in the 2500 Hz reference) | exact | 8 | 6 | 2 | 63 | 0.1270 | 3 | 0
    condition | synthetic | synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 5 dB in the passband (not restated in the 2500 Hz reference) | exact | 4 | 3 | 1 | 63 | 0.0635 | 3 | 0
    condition | synthetic | synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 0 dB in the passband (not restated in the 2500 Hz reference) | exact | 0 | 0 | 0 | 21 | 0.0000 | 1 | 0
    condition | synthetic | synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 15 dB in the passband (not restated in the 2500 Hz reference) | exact | 1 | 0 | 1 | 21 | 0.0476 | 1 | 0
    condition | synthetic | synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 5 dB in the passband (not restated in the 2500 Hz reference) | exact | 11 | 4 | 7 | 21 | 0.5238 | 1 | 0

MET-CER-SURE, MET-COVERAGE, MET-WBE:

    condition | real | condition | key | sure emitted | sure wrong or added | MET-CER-SURE | sent | MET-COVERAGE | words sent | boundaries wrong | MET-WBE | recordings measured
    condition | real | real HF, no CH-* profile, SNR_2500 not measured, sender TX-FARNS (CW_SPEC.md 6.4 and 10, HM-DEC-115's traffic net) | inferred | 43 | 1 | 0.0233 | 44 | 0.9773 | 11 | 7 | 0.6364 | 1 of 1
    condition | real | real HF, no CH-* profile, SNR_2500 not measured, sender TX-ITU (CW_SPEC.md 10, the KD0UN capture) | inferred | 13 | 0 | 0.0000 | 13 | 1.0000 | 4 | 0 | 0.0000 | 1 of 1
    condition | real | real HF, no CH-* profile, SNR_2500 not measured, sender TX-TIGHT (CW_SPEC.md 10, HM-DEC-101) | inferred | 6 | 0 | 0.0000 | 6 | 1.0000 | 1 | 0 | 0.0000 | 1 of 1
    condition | real | real HF, no CH-* profile, SNR_2500 not measured, sender not stated in CW_SPEC.md | inferred | 364 | 66 | 0.1813 | 410 | 0.8878 | 97 | 50 | 0.5155 | 20 of 20
    condition | synthetic | condition | key | sure emitted | sure wrong or added | MET-CER-SURE | sent | MET-COVERAGE | words sent | boundaries wrong | MET-WBE | recordings measured
    condition | synthetic | synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 0 dB in the passband (not restated in the 2500 Hz reference) | exact | 0 | 0 | no number: nothing to divide by | 63 | 0.0000 | 21 | 18 | 0.8571 | 3 of 3
    condition | synthetic | synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 15 dB in the passband (not restated in the 2500 Hz reference) | exact | 69 | 8 | 0.1159 | 63 | 1.0952 | 21 | 1 | 0.0476 | 3 of 3
    condition | synthetic | synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 5 dB in the passband (not restated in the 2500 Hz reference) | exact | 65 | 4 | 0.0615 | 63 | 1.0317 | 21 | 0 | 0.0000 | 3 of 3
    condition | synthetic | synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 0 dB in the passband (not restated in the 2500 Hz reference) | exact | 0 | 0 | no number: nothing to divide by | 21 | 0.0000 | 7 | 6 | 0.8571 | 1 of 1
    condition | synthetic | synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 15 dB in the passband (not restated in the 2500 Hz reference) | exact | 21 | 1 | 0.0476 | 21 | 1.0000 | 7 | 14 | 2.0000 | 1 of 1
    condition | synthetic | synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 5 dB in the passband (not restated in the 2500 Hz reference) | exact | 25 | 11 | 0.4400 | 21 | 1.1905 | 7 | 10 | 1.4286 | 1 of 1

## Per recording

MET-INVENTED, with the stray trace's old count of added named characters beside it:


MET-CER-SURE, MET-COVERAGE, MET-WBE and `CwScorer.Kinds`' boundary edits on the same stretches:

    row | recording | set | key | sure emitted | sure wrong | sure added | MET-CER-SURE | sent | sure right | MET-COVERAGE | placeholders | named below high | words sent | inserted | deleted | MET-WBE | Kinds space added | Kinds space missing
    row | unadjudicated/cw-2026-09-23-173723 | 17:37 | inferred | 28 | 6 | 8 | 0.5000 | 20 | 14 | 1.4000 | 0 | 0 | 6 | 4 | 1 | 0.8333 | 4 | 1
    row | cw-2026-08-17-013347 | baseline | inferred | 6 | 0 | 0 | 0.0000 | 6 | 6 | 1.0000 | 0 | 0 | 1 | 0 | 0 | 0.0000 | 0 | 0
    row | cw-2026-08-17-134712 | baseline | inferred | 3 | 0 | 0 | 0.0000 | 3 | 3 | 1.0000 | 0 | 0 | 1 | 0 | 0 | 0.0000 | 0 | 0
    row | unadjudicated/cw-2026-08-18-003758 | baseline | inferred | 11 | 3 | 0 | 0.2727 | 11 | 8 | 1.0000 | 0 | 0 | 2 | 0 | 0 | 0.0000 | 0 | 0
    row | unadjudicated/cw-2026-08-24-012403 | outside | inferred | 13 | 0 | 0 | 0.0000 | 13 | 13 | 1.0000 | 0 | 0 | 4 | 0 | 0 | 0.0000 | 0 | 0
    row | cw-2026-08-18-004507 | outside | inferred | 43 | 1 | 0 | 0.0233 | 44 | 42 | 0.9773 | 0 | 0 | 11 | 7 | 0 | 0.6364 | 9 | 0
    row | unadjudicated/cw-2026-08-22-031838 | outside | inferred | 20 | 10 | 1 | 0.5500 | 26 | 9 | 0.7692 | 1 | 0 | 10 | 2 | 3 | 0.5000 | 5 | 1
    row | unadjudicated/cw-2026-08-22-031905 | outside | inferred | 30 | 10 | 0 | 0.3333 | 33 | 20 | 0.9091 | 0 | 0 | 7 | 1 | 1 | 0.2857 | 6 | 1
    row | unadjudicated/cw-2026-08-22-031948 | outside | inferred | 27 | 1 | 0 | 0.0370 | 28 | 26 | 0.9643 | 0 | 0 | 9 | 0 | 0 | 0.0000 | 0 | 0
    row | unadjudicated/cw-2026-08-22-032012 | outside | inferred | 43 | 1 | 2 | 0.0698 | 42 | 40 | 1.0238 | 0 | 0 | 10 | 1 | 2 | 0.3000 | 0 | 1
    row | unadjudicated/cw-2026-08-22-032050 | outside | inferred | 37 | 3 | 1 | 0.1081 | 50 | 33 | 0.7400 | 1 | 0 | 10 | 2 | 2 | 0.4000 | 4 | 0
    row | unadjudicated/cw-2026-08-22-032113 | outside | inferred | 22 | 2 | 0 | 0.0909 | 25 | 20 | 0.8800 | 2 | 0 | 4 | 7 | 0 | 1.7500 | 7 | 0
    row | unadjudicated/cw-2026-08-22-032129 | outside | inferred | 28 | 13 | 0 | 0.4643 | 38 | 15 | 0.7368 | 0 | 0 | 5 | 5 | 2 | 1.4000 | 4 | 0
    row | unadjudicated/cw-2026-09-24-004108 | the ten | inferred | 4 | 2 | 0 | 0.5000 | 8 | 2 | 0.5000 | 0 | 0 | 2 | 2 | 0 | 1.0000 | 4 | 0
    row | unadjudicated/cw-2026-09-24-004133 | the ten | inferred | 4 | 0 | 0 | 0.0000 | 6 | 4 | 0.6667 | 0 | 0 | 1 | 2 | 0 | 2.0000 | 3 | 0
    row | unadjudicated/cw-2026-09-24-004205 | the ten | inferred | 8 | 0 | 0 | 0.0000 | 8 | 8 | 1.0000 | 0 | 0 | 3 | 1 | 0 | 0.3333 | 1 | 0
    row | unadjudicated/cw-2026-09-24-004234 | the ten | inferred | 9 | 2 | 0 | 0.2222 | 14 | 7 | 0.6429 | 0 | 0 | 5 | 2 | 2 | 0.8000 | 2 | 1
    row | unadjudicated/cw-2026-09-24-004322 | the ten | inferred | 32 | 0 | 0 | 0.0000 | 40 | 32 | 0.8000 | 0 | 0 | 8 | 4 | 2 | 0.7500 | 4 | 0
    row | unadjudicated/cw-2026-09-24-004347 | the ten | inferred | 24 | 0 | 1 | 0.0417 | 23 | 23 | 1.0435 | 0 | 0 | 6 | 1 | 0 | 0.1667 | 1 | 0
    row | unadjudicated/cw-2026-09-24-004405 | the ten | inferred | 6 | 0 | 0 | 0.0000 | 6 | 6 | 1.0000 | 0 | 0 | 1 | 0 | 0 | 0.0000 | 0 | 0
    row | unadjudicated/cw-2026-09-24-004427 | the ten | inferred | 9 | 0 | 0 | 0.0000 | 10 | 9 | 0.9000 | 0 | 0 | 3 | 0 | 0 | 0.0000 | 0 | 0
    row | unadjudicated/cw-2026-09-24-004510 | the ten | inferred | 11 | 0 | 0 | 0.0000 | 11 | 11 | 1.0000 | 0 | 0 | 2 | 0 | 0 | 0.0000 | 0 | 0
    row | unadjudicated/cw-2026-09-24-004550 | the ten | inferred | 8 | 0 | 0 | 0.0000 | 8 | 8 | 1.0000 | 0 | 0 | 2 | 1 | 0 | 0.5000 | 1 | 0
    row | cq-12wpm-15db | synthetic | exact | 21 | 0 | 0 | 0.0000 | 21 | 21 | 1.0000 | 0 | 0 | 7 | 0 | 0 | 0.0000 | 0 | 0
    row | cq-12wpm-5db | synthetic | exact | 23 | 1 | 2 | 0.1304 | 21 | 20 | 1.0952 | 0 | 0 | 7 | 0 | 0 | 0.0000 | 0 | 0
    row | cq-12wpm-0db | synthetic | exact | 0 | 0 | 0 | no number: nothing to divide by | 21 | 0 | 0.0000 | 0 | 0 | 7 | 0 | 6 | 0.8571 | 0 | 6
    row | cq-18wpm-15db | synthetic | exact | 26 | 2 | 5 | 0.2692 | 21 | 19 | 1.2381 | 0 | 0 | 7 | 0 | 1 | 0.1429 | 0 | 1
    row | cq-18wpm-5db | synthetic | exact | 20 | 0 | 0 | 0.0000 | 21 | 20 | 0.9524 | 1 | 0 | 7 | 0 | 0 | 0.0000 | 0 | 0
    row | cq-18wpm-0db | synthetic | exact | 0 | 0 | 0 | no number: nothing to divide by | 21 | 0 | 0.0000 | 0 | 0 | 7 | 0 | 6 | 0.8571 | 0 | 6
    row | cq-25wpm-15db | synthetic | exact | 22 | 0 | 1 | 0.0455 | 21 | 21 | 1.0476 | 0 | 0 | 7 | 0 | 0 | 0.0000 | 0 | 0
    row | cq-25wpm-5db | synthetic | exact | 22 | 0 | 1 | 0.0455 | 21 | 21 | 1.0476 | 0 | 0 | 7 | 0 | 0 | 0.0000 | 0 | 0
    row | cq-25wpm-0db | synthetic | exact | 0 | 0 | 0 | no number: nothing to divide by | 21 | 0 | 0.0000 | 0 | 0 | 7 | 0 | 6 | 0.8571 | 0 | 6
    row | cq-18wpm-15db-char5 | synthetic | exact | 21 | 1 | 0 | 0.0476 | 21 | 20 | 1.0000 | 0 | 0 | 7 | 10 | 4 | 2.0000 | 10 | 4
    row | cq-18wpm-5db-char5 | synthetic | exact | 25 | 7 | 4 | 0.4400 | 21 | 14 | 1.1905 | 1 | 0 | 7 | 9 | 1 | 1.4286 | 9 | 1
    row | cq-18wpm-0db-char5 | synthetic | exact | 0 | 0 | 0 | no number: nothing to divide by | 21 | 0 | 0.0000 | 0 | 0 | 7 | 0 | 6 | 0.8571 | 0 | 6

## How MET-WBE relates to `CwScorer.Kinds`

`Kinds` counts space edits in the text alignment, where spaces are characters. There, the
tie-break can seat a letter against a space and count that one edit as a boundary. MET-WBE places
boundaries on the letters-only alignment, so no letter error can stand in for one. On the synthetic
set they agree exactly: 19 inserted and 30 deleted, against 19 spaces added and 30 missing. On the
real set MET-WBE counts 42 inserted and 15 deleted, while `Kinds` counts 55 spaces added and 5
missing. The requirement's number is MET-WBE (HM-REQ-082). `Kinds` remains the edit total's
breakdown.

## Reaching the metrics from another test type (1.4)

`CwMetrics` is a public static class in the engine test assembly, like `CwScorer`.
`CwMetrics.Symbols` takes any settled characters. `CwMetrics.Align` takes symbols, a key and its kind.
`Invented`, `SureErrors`, `Coverage` and `WordBoundaries` each take that alignment, and
`TheRequirementsAreMeasuredTests.Measure` cuts a decode into the baseline's scored stretches first.
Two types call all four: `TheMetricsCountWhatAHandCountsTests` on pairs known by construction, and
`TheRequirementsAreMeasuredTests` on every keyed recording.
