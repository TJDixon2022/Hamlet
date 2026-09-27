# Arbitration: the arbitrated transcript, ours alone and the port alone (HM-REQ-128)

Written by `TheArbitrationEarnsItsPlaceFact.TheThreeWayTable` (work instruction 466, task 2; PHASE_PLAN.md 9.7). The metric list and the loss, better-decoder and path rules were fixed in that fact's header at `9eef6850`, before any three-way figure was computed (V-14), and are `CwSwitchTable.Choose`. Every figure goes through `TheRequirementsAreMeasuredTests.Measure` and the same `CwMetrics` calls, each with its key's kind (V-11, V-13). The port was run on 35 of 35 recordings in the harness.

## 1. The metrics and the rules

- **011** MET-INVENTED, **010** MET-CER-SURE, **012** MET-COVERAGE (sure and right over sent), **081** MET-WBE: all three outputs. Lower is better except coverage.
- **013** sent characters not emitted sure and correct, over sent: the 15 dB rows only (synthetic; not shown to be CH-AWGN).
- **014** dim right over dim emitted, higher better: not defined for the port, which emits no dim (parity.md section 1).
- **083** live against settled boundaries, over distinct consecutive pairs the leading edge showed: live path only; not defined for the port, which has one rendering.
- **084** named spans read exactly (the word, a boundary each side, none inside, every letter sure), over the 3 of 6 whose audio is in the tree: rows real HF, all and real HF, sender not stated.
- **015 and 082** are properties of all three (the class travels with each character when it is emitted; boundary errors are scored apart by `CwMetrics.WordBoundaries`).
- **Loses**: strictly worse than either decoder alone on any metric defined for both, a rate compared as a rate. **Better alone**: no worse on every metric defined for both, else the first difference in the order 011, 010, 013, 012, 014, 081, 083, 084, ours on a full tie. **Switch**: no loss, arbitrate; else the better alone. **Path**: real HF, all from the live path (the product runs under it); every other row from the harness.

## 2. The switch per condition

| condition | set from | arbitration loses on | better alone | by | switch |
|---|---|---|---|---|---|
| real HF, all | live | none | ours | dominance | **arbitrate** |
| real HF, no CH-* profile, SNR_2500 not measured, sender TX-FARNS (CW_SPEC.md 6.4 and 10, HM-DEC-115's traffic net) | harness | 081 to port | ours | order, at 011 | **ours alone** |
| real HF, no CH-* profile, SNR_2500 not measured, sender TX-ITU (CW_SPEC.md 10, the KD0UN capture) | harness | none | ours | dominance | **arbitrate** |
| real HF, no CH-* profile, SNR_2500 not measured, sender TX-TIGHT (CW_SPEC.md 10, HM-DEC-101) | harness | none | ours | dominance | **arbitrate** |
| real HF, no CH-* profile, SNR_2500 not measured, sender not stated in CW_SPEC.md | harness | none | ours | dominance | **arbitrate** |
| synthetic, all | harness | none | ours | dominance | **arbitrate** |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 0 dB in the passband (not restated in the 2500 Hz reference) | harness | 012 to port, 081 to port | ours | order, at 011 | **ours alone** |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 15 dB in the passband (not restated in the 2500 Hz reference) | harness | none | ours | dominance | **arbitrate** |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 5 dB in the passband (not restated in the 2500 Hz reference) | harness | none | ours | dominance | **arbitrate** |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 0 dB in the passband (not restated in the 2500 Hz reference) | harness | 012 to port | ours | order, at 011 | **ours alone** |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 15 dB in the passband (not restated in the 2500 Hz reference) | harness | none | ours | dominance | **arbitrate** |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 5 dB in the passband (not restated in the 2500 Hz reference) | harness | 011 to port, 010 to port | port | order, at 011 | **port alone** |

## 3. The harness, each recording under its own row

| condition | key | recordings | metric | arbitrated | ours alone | port alone | arbitrated against ours | against the port |
|---|---|---|---|---|---|---|---|---|
| real HF, all | inferred | 23 | 011 | 33 / 473 (0.070) | 33 / 473 (0.070) | 62 / 473 (0.131) | equal | better |
| real HF, all | inferred | 23 | 010 | 33 / 436 (0.076) | 33 / 436 (0.076) | 62 / 239 (0.259) | equal | better |
| real HF, all | inferred | 23 | 012 | 403 / 473 (0.852) | 403 / 473 (0.852) | 177 / 473 (0.374) | equal | better |
| real HF, all | inferred | 23 | 014 | 0 / 0 (not defined) | 0 / 0 (not defined) | not compared | not compared | not compared |
| real HF, all | inferred | 23 | 081 | 37 / 113 (0.327) | 37 / 113 (0.327) | 86 / 113 (0.761) | equal | better |
| real HF, all | inferred | 23 | 084 | 0 / 3 (0.000) | 0 / 3 (0.000) | 0 / 3 (0.000) | equal | equal |
| real HF, all | | | **verdict** | does not lose | better alone: ours by dominance | | switch on this path: arbitrate | |
| real HF, no CH-* profile, SNR_2500 not measured, sender TX-FARNS (CW_SPEC.md 6.4 and 10, HM-DEC-115's traffic net) | inferred | 1 | 011 | 0 / 44 (0.000) | 0 / 44 (0.000) | 3 / 44 (0.068) | equal | better |
| real HF, no CH-* profile, SNR_2500 not measured, sender TX-FARNS (CW_SPEC.md 6.4 and 10, HM-DEC-115's traffic net) | inferred | 1 | 010 | 0 / 43 (0.000) | 0 / 43 (0.000) | 3 / 33 (0.091) | equal | better |
| real HF, no CH-* profile, SNR_2500 not measured, sender TX-FARNS (CW_SPEC.md 6.4 and 10, HM-DEC-115's traffic net) | inferred | 1 | 012 | 43 / 44 (0.977) | 43 / 44 (0.977) | 30 / 44 (0.682) | equal | better |
| real HF, no CH-* profile, SNR_2500 not measured, sender TX-FARNS (CW_SPEC.md 6.4 and 10, HM-DEC-115's traffic net) | inferred | 1 | 014 | 0 / 0 (not defined) | 0 / 0 (not defined) | not compared | not compared | not compared |
| real HF, no CH-* profile, SNR_2500 not measured, sender TX-FARNS (CW_SPEC.md 6.4 and 10, HM-DEC-115's traffic net) | inferred | 1 | 081 | 5 / 11 (0.455) | 5 / 11 (0.455) | 4 / 11 (0.364) | equal | WORSE |
| real HF, no CH-* profile, SNR_2500 not measured, sender TX-FARNS (CW_SPEC.md 6.4 and 10, HM-DEC-115's traffic net) | | | **verdict** | loses on 081 to port | better alone: ours by order, at 011 | | switch on this path: ours alone | |
| real HF, no CH-* profile, SNR_2500 not measured, sender TX-ITU (CW_SPEC.md 10, the KD0UN capture) | inferred | 1 | 011 | 0 / 13 (0.000) | 0 / 13 (0.000) | 1 / 13 (0.077) | equal | better |
| real HF, no CH-* profile, SNR_2500 not measured, sender TX-ITU (CW_SPEC.md 10, the KD0UN capture) | inferred | 1 | 010 | 0 / 13 (0.000) | 0 / 13 (0.000) | 1 / 6 (0.167) | equal | better |
| real HF, no CH-* profile, SNR_2500 not measured, sender TX-ITU (CW_SPEC.md 10, the KD0UN capture) | inferred | 1 | 012 | 13 / 13 (1.000) | 13 / 13 (1.000) | 5 / 13 (0.385) | equal | better |
| real HF, no CH-* profile, SNR_2500 not measured, sender TX-ITU (CW_SPEC.md 10, the KD0UN capture) | inferred | 1 | 014 | 0 / 0 (not defined) | 0 / 0 (not defined) | not compared | not compared | not compared |
| real HF, no CH-* profile, SNR_2500 not measured, sender TX-ITU (CW_SPEC.md 10, the KD0UN capture) | inferred | 1 | 081 | 0 / 4 (0.000) | 0 / 4 (0.000) | 3 / 4 (0.750) | equal | better |
| real HF, no CH-* profile, SNR_2500 not measured, sender TX-ITU (CW_SPEC.md 10, the KD0UN capture) | | | **verdict** | does not lose | better alone: ours by dominance | | switch on this path: arbitrate | |
| real HF, no CH-* profile, SNR_2500 not measured, sender TX-TIGHT (CW_SPEC.md 10, HM-DEC-101) | inferred | 1 | 011 | 0 / 6 (0.000) | 0 / 6 (0.000) | 2 / 6 (0.333) | equal | better |
| real HF, no CH-* profile, SNR_2500 not measured, sender TX-TIGHT (CW_SPEC.md 10, HM-DEC-101) | inferred | 1 | 010 | 0 / 6 (0.000) | 0 / 6 (0.000) | 2 / 3 (0.667) | equal | better |
| real HF, no CH-* profile, SNR_2500 not measured, sender TX-TIGHT (CW_SPEC.md 10, HM-DEC-101) | inferred | 1 | 012 | 6 / 6 (1.000) | 6 / 6 (1.000) | 1 / 6 (0.167) | equal | better |
| real HF, no CH-* profile, SNR_2500 not measured, sender TX-TIGHT (CW_SPEC.md 10, HM-DEC-101) | inferred | 1 | 014 | 0 / 0 (not defined) | 0 / 0 (not defined) | not compared | not compared | not compared |
| real HF, no CH-* profile, SNR_2500 not measured, sender TX-TIGHT (CW_SPEC.md 10, HM-DEC-101) | inferred | 1 | 081 | 0 / 1 (0.000) | 0 / 1 (0.000) | 0 / 1 (0.000) | equal | equal |
| real HF, no CH-* profile, SNR_2500 not measured, sender TX-TIGHT (CW_SPEC.md 10, HM-DEC-101) | | | **verdict** | does not lose | better alone: ours by dominance | | switch on this path: arbitrate | |
| real HF, no CH-* profile, SNR_2500 not measured, sender not stated in CW_SPEC.md | inferred | 20 | 011 | 33 / 410 (0.080) | 33 / 410 (0.080) | 56 / 410 (0.137) | equal | better |
| real HF, no CH-* profile, SNR_2500 not measured, sender not stated in CW_SPEC.md | inferred | 20 | 010 | 33 / 374 (0.088) | 33 / 374 (0.088) | 56 / 197 (0.284) | equal | better |
| real HF, no CH-* profile, SNR_2500 not measured, sender not stated in CW_SPEC.md | inferred | 20 | 012 | 341 / 410 (0.832) | 341 / 410 (0.832) | 141 / 410 (0.344) | equal | better |
| real HF, no CH-* profile, SNR_2500 not measured, sender not stated in CW_SPEC.md | inferred | 20 | 014 | 0 / 0 (not defined) | 0 / 0 (not defined) | not compared | not compared | not compared |
| real HF, no CH-* profile, SNR_2500 not measured, sender not stated in CW_SPEC.md | inferred | 20 | 081 | 32 / 97 (0.330) | 32 / 97 (0.330) | 79 / 97 (0.814) | equal | better |
| real HF, no CH-* profile, SNR_2500 not measured, sender not stated in CW_SPEC.md | inferred | 20 | 084 | 0 / 3 (0.000) | 0 / 3 (0.000) | 0 / 3 (0.000) | equal | equal |
| real HF, no CH-* profile, SNR_2500 not measured, sender not stated in CW_SPEC.md | | | **verdict** | does not lose | better alone: ours by dominance | | switch on this path: arbitrate | |
| synthetic, all | exact | 12 | 011 | 14 / 252 (0.056) | 14 / 252 (0.056) | 34 / 252 (0.135) | equal | better |
| synthetic, all | exact | 12 | 010 | 14 / 173 (0.081) | 14 / 173 (0.081) | 34 / 168 (0.202) | equal | better |
| synthetic, all | exact | 12 | 012 | 159 / 252 (0.631) | 159 / 252 (0.631) | 134 / 252 (0.532) | equal | better |
| synthetic, all | exact | 12 | 014 | 0 / 0 (not defined) | 0 / 0 (not defined) | not compared | not compared | not compared |
| synthetic, all | exact | 12 | 081 | 44 / 84 (0.524) | 44 / 84 (0.524) | 56 / 84 (0.667) | equal | better |
| synthetic, all | | | **verdict** | does not lose | better alone: ours by dominance | | switch on this path: arbitrate | |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 0 dB in the passband (not restated in the 2500 Hz reference) | exact | 3 | 011 | 0 / 63 (0.000) | 0 / 63 (0.000) | 21 / 63 (0.333) | equal | better |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 0 dB in the passband (not restated in the 2500 Hz reference) | exact | 3 | 010 | 0 / 0 (not defined) | 0 / 0 (not defined) | 21 / 27 (0.778) | not compared | not compared |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 0 dB in the passband (not restated in the 2500 Hz reference) | exact | 3 | 012 | 0 / 63 (0.000) | 0 / 63 (0.000) | 6 / 63 (0.095) | equal | WORSE |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 0 dB in the passband (not restated in the 2500 Hz reference) | exact | 3 | 014 | 0 / 0 (not defined) | 0 / 0 (not defined) | not compared | not compared | not compared |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 0 dB in the passband (not restated in the 2500 Hz reference) | exact | 3 | 081 | 18 / 21 (0.857) | 18 / 21 (0.857) | 15 / 21 (0.714) | equal | WORSE |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 0 dB in the passband (not restated in the 2500 Hz reference) | | | **verdict** | loses on 012 to port, 081 to port | better alone: ours by order, at 011 | | switch on this path: ours alone | |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 15 dB in the passband (not restated in the 2500 Hz reference) | exact | 3 | 011 | 1 / 63 (0.016) | 1 / 63 (0.016) | 2 / 63 (0.032) | equal | better |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 15 dB in the passband (not restated in the 2500 Hz reference) | exact | 3 | 010 | 1 / 64 (0.016) | 1 / 64 (0.016) | 2 / 56 (0.036) | equal | better |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 15 dB in the passband (not restated in the 2500 Hz reference) | exact | 3 | 013 | 0 / 63 (0.000) | 0 / 63 (0.000) | 9 / 63 (0.143) | equal | better |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 15 dB in the passband (not restated in the 2500 Hz reference) | exact | 3 | 012 | 63 / 63 (1.000) | 63 / 63 (1.000) | 54 / 63 (0.857) | equal | better |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 15 dB in the passband (not restated in the 2500 Hz reference) | exact | 3 | 014 | 0 / 0 (not defined) | 0 / 0 (not defined) | not compared | not compared | not compared |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 15 dB in the passband (not restated in the 2500 Hz reference) | exact | 3 | 081 | 0 / 21 (0.000) | 0 / 21 (0.000) | 3 / 21 (0.143) | equal | better |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 15 dB in the passband (not restated in the 2500 Hz reference) | | | **verdict** | does not lose | better alone: ours by dominance | | switch on this path: arbitrate | |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 5 dB in the passband (not restated in the 2500 Hz reference) | exact | 3 | 011 | 1 / 63 (0.016) | 1 / 63 (0.016) | 5 / 63 (0.079) | equal | better |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 5 dB in the passband (not restated in the 2500 Hz reference) | exact | 3 | 010 | 1 / 63 (0.016) | 1 / 63 (0.016) | 5 / 47 (0.106) | equal | better |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 5 dB in the passband (not restated in the 2500 Hz reference) | exact | 3 | 012 | 62 / 63 (0.984) | 62 / 63 (0.984) | 42 / 63 (0.667) | equal | better |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 5 dB in the passband (not restated in the 2500 Hz reference) | exact | 3 | 014 | 0 / 0 (not defined) | 0 / 0 (not defined) | not compared | not compared | not compared |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 5 dB in the passband (not restated in the 2500 Hz reference) | exact | 3 | 081 | 0 / 21 (0.000) | 0 / 21 (0.000) | 6 / 21 (0.286) | equal | better |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 5 dB in the passband (not restated in the 2500 Hz reference) | | | **verdict** | does not lose | better alone: ours by dominance | | switch on this path: arbitrate | |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 0 dB in the passband (not restated in the 2500 Hz reference) | exact | 1 | 011 | 0 / 21 (0.000) | 0 / 21 (0.000) | 2 / 21 (0.095) | equal | better |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 0 dB in the passband (not restated in the 2500 Hz reference) | exact | 1 | 010 | 0 / 0 (not defined) | 0 / 0 (not defined) | 2 / 3 (0.667) | not compared | not compared |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 0 dB in the passband (not restated in the 2500 Hz reference) | exact | 1 | 012 | 0 / 21 (0.000) | 0 / 21 (0.000) | 1 / 21 (0.048) | equal | WORSE |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 0 dB in the passband (not restated in the 2500 Hz reference) | exact | 1 | 014 | 0 / 0 (not defined) | 0 / 0 (not defined) | not compared | not compared | not compared |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 0 dB in the passband (not restated in the 2500 Hz reference) | exact | 1 | 081 | 6 / 7 (0.857) | 6 / 7 (0.857) | 6 / 7 (0.857) | equal | equal |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 0 dB in the passband (not restated in the 2500 Hz reference) | | | **verdict** | loses on 012 to port | better alone: ours by order, at 011 | | switch on this path: ours alone | |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 15 dB in the passband (not restated in the 2500 Hz reference) | exact | 1 | 011 | 1 / 21 (0.048) | 1 / 21 (0.048) | 1 / 21 (0.048) | equal | equal |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 15 dB in the passband (not restated in the 2500 Hz reference) | exact | 1 | 010 | 1 / 21 (0.048) | 1 / 21 (0.048) | 1 / 19 (0.053) | equal | better |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 15 dB in the passband (not restated in the 2500 Hz reference) | exact | 1 | 013 | 1 / 21 (0.048) | 1 / 21 (0.048) | 3 / 21 (0.143) | equal | better |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 15 dB in the passband (not restated in the 2500 Hz reference) | exact | 1 | 012 | 20 / 21 (0.952) | 20 / 21 (0.952) | 18 / 21 (0.857) | equal | better |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 15 dB in the passband (not restated in the 2500 Hz reference) | exact | 1 | 014 | 0 / 0 (not defined) | 0 / 0 (not defined) | not compared | not compared | not compared |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 15 dB in the passband (not restated in the 2500 Hz reference) | exact | 1 | 081 | 12 / 7 (1.714) | 12 / 7 (1.714) | 14 / 7 (2.000) | equal | better |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 15 dB in the passband (not restated in the 2500 Hz reference) | | | **verdict** | does not lose | better alone: ours by dominance | | switch on this path: arbitrate | |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 5 dB in the passband (not restated in the 2500 Hz reference) | exact | 1 | 011 | 11 / 21 (0.524) | 11 / 21 (0.524) | 3 / 21 (0.143) | equal | WORSE |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 5 dB in the passband (not restated in the 2500 Hz reference) | exact | 1 | 010 | 11 / 25 (0.440) | 11 / 25 (0.440) | 3 / 16 (0.188) | equal | WORSE |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 5 dB in the passband (not restated in the 2500 Hz reference) | exact | 1 | 012 | 14 / 21 (0.667) | 14 / 21 (0.667) | 13 / 21 (0.619) | equal | better |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 5 dB in the passband (not restated in the 2500 Hz reference) | exact | 1 | 014 | 0 / 0 (not defined) | 0 / 0 (not defined) | not compared | not compared | not compared |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 5 dB in the passband (not restated in the 2500 Hz reference) | exact | 1 | 081 | 8 / 7 (1.143) | 8 / 7 (1.143) | 12 / 7 (1.714) | equal | better |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 5 dB in the passband (not restated in the 2500 Hz reference) | | | **verdict** | loses on 011 to port, 010 to port | better alone: port by order, at 011 | | switch on this path: port alone | |

## 4. The live path, CwDecoder with the second reader, grouped by the same rows

| condition | key | recordings | metric | arbitrated | ours alone | port alone | arbitrated against ours | against the port |
|---|---|---|---|---|---|---|---|---|
| real HF, all | inferred | 23 | 011 | 33 / 473 (0.070) | 33 / 473 (0.070) | 64 / 453 (0.141) | equal | better |
| real HF, all | inferred | 23 | 010 | 33 / 436 (0.076) | 33 / 436 (0.076) | 64 / 221 (0.290) | equal | better |
| real HF, all | inferred | 23 | 012 | 403 / 473 (0.852) | 403 / 473 (0.852) | 157 / 453 (0.347) | equal | better |
| real HF, all | inferred | 23 | 014 | 0 / 0 (not defined) | 0 / 0 (not defined) | not compared | not compared | not compared |
| real HF, all | inferred | 23 | 081 | 37 / 113 (0.327) | 37 / 113 (0.327) | 78 / 107 (0.729) | equal | better |
| real HF, all | inferred | 23 | 083 | 10 / 331 (0.030) | 10 / 331 (0.030) | not compared | equal | not compared |
| real HF, all | inferred | 23 | 084 | 0 / 3 (0.000) | 0 / 3 (0.000) | 0 / 3 (0.000) | equal | equal |
| real HF, all | | | **verdict** | does not lose | better alone: ours by dominance | | switch on this path: arbitrate | |
| real HF, no CH-* profile, SNR_2500 not measured, sender TX-FARNS (CW_SPEC.md 6.4 and 10, HM-DEC-115's traffic net) | inferred | 1 | 011 | 0 / 44 (0.000) | 0 / 44 (0.000) | 5 / 44 (0.114) | equal | better |
| real HF, no CH-* profile, SNR_2500 not measured, sender TX-FARNS (CW_SPEC.md 6.4 and 10, HM-DEC-115's traffic net) | inferred | 1 | 010 | 0 / 43 (0.000) | 0 / 43 (0.000) | 5 / 26 (0.192) | equal | better |
| real HF, no CH-* profile, SNR_2500 not measured, sender TX-FARNS (CW_SPEC.md 6.4 and 10, HM-DEC-115's traffic net) | inferred | 1 | 012 | 43 / 44 (0.977) | 43 / 44 (0.977) | 21 / 44 (0.477) | equal | better |
| real HF, no CH-* profile, SNR_2500 not measured, sender TX-FARNS (CW_SPEC.md 6.4 and 10, HM-DEC-115's traffic net) | inferred | 1 | 014 | 0 / 0 (not defined) | 0 / 0 (not defined) | not compared | not compared | not compared |
| real HF, no CH-* profile, SNR_2500 not measured, sender TX-FARNS (CW_SPEC.md 6.4 and 10, HM-DEC-115's traffic net) | inferred | 1 | 081 | 5 / 11 (0.455) | 5 / 11 (0.455) | 7 / 11 (0.636) | equal | better |
| real HF, no CH-* profile, SNR_2500 not measured, sender TX-FARNS (CW_SPEC.md 6.4 and 10, HM-DEC-115's traffic net) | inferred | 1 | 083 | 0 / 22 (0.000) | 0 / 22 (0.000) | not compared | equal | not compared |
| real HF, no CH-* profile, SNR_2500 not measured, sender TX-FARNS (CW_SPEC.md 6.4 and 10, HM-DEC-115's traffic net) | | | **verdict** | does not lose | better alone: ours by dominance | | switch on this path: arbitrate | |
| real HF, no CH-* profile, SNR_2500 not measured, sender TX-ITU (CW_SPEC.md 10, the KD0UN capture) | inferred | 1 | 011 | 0 / 13 (0.000) | 0 / 13 (0.000) | 1 / 13 (0.077) | equal | better |
| real HF, no CH-* profile, SNR_2500 not measured, sender TX-ITU (CW_SPEC.md 10, the KD0UN capture) | inferred | 1 | 010 | 0 / 13 (0.000) | 0 / 13 (0.000) | 1 / 5 (0.200) | equal | better |
| real HF, no CH-* profile, SNR_2500 not measured, sender TX-ITU (CW_SPEC.md 10, the KD0UN capture) | inferred | 1 | 012 | 13 / 13 (1.000) | 13 / 13 (1.000) | 4 / 13 (0.308) | equal | better |
| real HF, no CH-* profile, SNR_2500 not measured, sender TX-ITU (CW_SPEC.md 10, the KD0UN capture) | inferred | 1 | 014 | 0 / 0 (not defined) | 0 / 0 (not defined) | not compared | not compared | not compared |
| real HF, no CH-* profile, SNR_2500 not measured, sender TX-ITU (CW_SPEC.md 10, the KD0UN capture) | inferred | 1 | 081 | 0 / 4 (0.000) | 0 / 4 (0.000) | 3 / 4 (0.750) | equal | better |
| real HF, no CH-* profile, SNR_2500 not measured, sender TX-ITU (CW_SPEC.md 10, the KD0UN capture) | inferred | 1 | 083 | 0 / 2 (0.000) | 0 / 2 (0.000) | not compared | equal | not compared |
| real HF, no CH-* profile, SNR_2500 not measured, sender TX-ITU (CW_SPEC.md 10, the KD0UN capture) | | | **verdict** | does not lose | better alone: ours by dominance | | switch on this path: arbitrate | |
| real HF, no CH-* profile, SNR_2500 not measured, sender TX-TIGHT (CW_SPEC.md 10, HM-DEC-101) | inferred | 1 | 011 | 0 / 6 (0.000) | 0 / 6 (0.000) | 2 / 6 (0.333) | equal | better |
| real HF, no CH-* profile, SNR_2500 not measured, sender TX-TIGHT (CW_SPEC.md 10, HM-DEC-101) | inferred | 1 | 010 | 0 / 6 (0.000) | 0 / 6 (0.000) | 2 / 4 (0.500) | equal | better |
| real HF, no CH-* profile, SNR_2500 not measured, sender TX-TIGHT (CW_SPEC.md 10, HM-DEC-101) | inferred | 1 | 012 | 6 / 6 (1.000) | 6 / 6 (1.000) | 2 / 6 (0.333) | equal | better |
| real HF, no CH-* profile, SNR_2500 not measured, sender TX-TIGHT (CW_SPEC.md 10, HM-DEC-101) | inferred | 1 | 014 | 0 / 0 (not defined) | 0 / 0 (not defined) | not compared | not compared | not compared |
| real HF, no CH-* profile, SNR_2500 not measured, sender TX-TIGHT (CW_SPEC.md 10, HM-DEC-101) | inferred | 1 | 081 | 0 / 1 (0.000) | 0 / 1 (0.000) | 0 / 1 (0.000) | equal | equal |
| real HF, no CH-* profile, SNR_2500 not measured, sender TX-TIGHT (CW_SPEC.md 10, HM-DEC-101) | inferred | 1 | 083 | 1 / 31 (0.032) | 1 / 31 (0.032) | not compared | equal | not compared |
| real HF, no CH-* profile, SNR_2500 not measured, sender TX-TIGHT (CW_SPEC.md 10, HM-DEC-101) | | | **verdict** | does not lose | better alone: ours by dominance | | switch on this path: arbitrate | |
| real HF, no CH-* profile, SNR_2500 not measured, sender not stated in CW_SPEC.md | inferred | 20 | 011 | 33 / 410 (0.080) | 33 / 410 (0.080) | 56 / 390 (0.144) | equal | better |
| real HF, no CH-* profile, SNR_2500 not measured, sender not stated in CW_SPEC.md | inferred | 20 | 010 | 33 / 374 (0.088) | 33 / 374 (0.088) | 56 / 186 (0.301) | equal | better |
| real HF, no CH-* profile, SNR_2500 not measured, sender not stated in CW_SPEC.md | inferred | 20 | 012 | 341 / 410 (0.832) | 341 / 410 (0.832) | 130 / 390 (0.333) | equal | better |
| real HF, no CH-* profile, SNR_2500 not measured, sender not stated in CW_SPEC.md | inferred | 20 | 014 | 0 / 0 (not defined) | 0 / 0 (not defined) | not compared | not compared | not compared |
| real HF, no CH-* profile, SNR_2500 not measured, sender not stated in CW_SPEC.md | inferred | 20 | 081 | 32 / 97 (0.330) | 32 / 97 (0.330) | 68 / 91 (0.747) | equal | better |
| real HF, no CH-* profile, SNR_2500 not measured, sender not stated in CW_SPEC.md | inferred | 20 | 083 | 9 / 276 (0.033) | 9 / 276 (0.033) | not compared | equal | not compared |
| real HF, no CH-* profile, SNR_2500 not measured, sender not stated in CW_SPEC.md | inferred | 20 | 084 | 0 / 3 (0.000) | 0 / 3 (0.000) | 0 / 3 (0.000) | equal | equal |
| real HF, no CH-* profile, SNR_2500 not measured, sender not stated in CW_SPEC.md | | | **verdict** | does not lose | better alone: ours by dominance | | switch on this path: arbitrate | |
| synthetic, all | exact | 12 | 011 | 14 / 252 (0.056) | 14 / 252 (0.056) | 29 / 252 (0.115) | equal | better |
| synthetic, all | exact | 12 | 010 | 14 / 173 (0.081) | 14 / 173 (0.081) | 29 / 154 (0.188) | equal | better |
| synthetic, all | exact | 12 | 012 | 159 / 252 (0.631) | 159 / 252 (0.631) | 125 / 252 (0.496) | equal | better |
| synthetic, all | exact | 12 | 014 | 0 / 0 (not defined) | 0 / 0 (not defined) | not compared | not compared | not compared |
| synthetic, all | exact | 12 | 081 | 44 / 84 (0.524) | 44 / 84 (0.524) | 52 / 84 (0.619) | equal | better |
| synthetic, all | exact | 12 | 083 | 1 / 40 (0.025) | 1 / 40 (0.025) | not compared | equal | not compared |
| synthetic, all | | | **verdict** | does not lose | better alone: ours by dominance | | switch on this path: arbitrate | |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 0 dB in the passband (not restated in the 2500 Hz reference) | exact | 3 | 011 | 0 / 63 (0.000) | 0 / 63 (0.000) | 13 / 63 (0.206) | equal | better |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 0 dB in the passband (not restated in the 2500 Hz reference) | exact | 3 | 010 | 0 / 0 (not defined) | 0 / 0 (not defined) | 13 / 18 (0.722) | not compared | not compared |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 0 dB in the passband (not restated in the 2500 Hz reference) | exact | 3 | 012 | 0 / 63 (0.000) | 0 / 63 (0.000) | 5 / 63 (0.079) | equal | WORSE |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 0 dB in the passband (not restated in the 2500 Hz reference) | exact | 3 | 014 | 0 / 0 (not defined) | 0 / 0 (not defined) | not compared | not compared | not compared |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 0 dB in the passband (not restated in the 2500 Hz reference) | exact | 3 | 081 | 18 / 21 (0.857) | 18 / 21 (0.857) | 17 / 21 (0.810) | equal | WORSE |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 0 dB in the passband (not restated in the 2500 Hz reference) | exact | 3 | 083 | 0 / 0 (not defined) | 0 / 0 (not defined) | not compared | not compared | not compared |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 0 dB in the passband (not restated in the 2500 Hz reference) | | | **verdict** | loses on 012 to port, 081 to port | better alone: ours by order, at 011 | | switch on this path: ours alone | |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 15 dB in the passband (not restated in the 2500 Hz reference) | exact | 3 | 011 | 1 / 63 (0.016) | 1 / 63 (0.016) | 2 / 63 (0.032) | equal | better |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 15 dB in the passband (not restated in the 2500 Hz reference) | exact | 3 | 010 | 1 / 64 (0.016) | 1 / 64 (0.016) | 2 / 56 (0.036) | equal | better |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 15 dB in the passband (not restated in the 2500 Hz reference) | exact | 3 | 013 | 0 / 63 (0.000) | 0 / 63 (0.000) | 9 / 63 (0.143) | equal | better |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 15 dB in the passband (not restated in the 2500 Hz reference) | exact | 3 | 012 | 63 / 63 (1.000) | 63 / 63 (1.000) | 54 / 63 (0.857) | equal | better |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 15 dB in the passband (not restated in the 2500 Hz reference) | exact | 3 | 014 | 0 / 0 (not defined) | 0 / 0 (not defined) | not compared | not compared | not compared |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 15 dB in the passband (not restated in the 2500 Hz reference) | exact | 3 | 081 | 0 / 21 (0.000) | 0 / 21 (0.000) | 3 / 21 (0.143) | equal | better |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 15 dB in the passband (not restated in the 2500 Hz reference) | exact | 3 | 083 | 0 / 15 (0.000) | 0 / 15 (0.000) | not compared | equal | not compared |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 15 dB in the passband (not restated in the 2500 Hz reference) | | | **verdict** | does not lose | better alone: ours by dominance | | switch on this path: arbitrate | |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 5 dB in the passband (not restated in the 2500 Hz reference) | exact | 3 | 011 | 1 / 63 (0.016) | 1 / 63 (0.016) | 5 / 63 (0.079) | equal | better |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 5 dB in the passband (not restated in the 2500 Hz reference) | exact | 3 | 010 | 1 / 63 (0.016) | 1 / 63 (0.016) | 5 / 47 (0.106) | equal | better |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 5 dB in the passband (not restated in the 2500 Hz reference) | exact | 3 | 012 | 62 / 63 (0.984) | 62 / 63 (0.984) | 42 / 63 (0.667) | equal | better |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 5 dB in the passband (not restated in the 2500 Hz reference) | exact | 3 | 014 | 0 / 0 (not defined) | 0 / 0 (not defined) | not compared | not compared | not compared |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 5 dB in the passband (not restated in the 2500 Hz reference) | exact | 3 | 081 | 0 / 21 (0.000) | 0 / 21 (0.000) | 6 / 21 (0.286) | equal | better |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 5 dB in the passband (not restated in the 2500 Hz reference) | exact | 3 | 083 | 0 / 13 (0.000) | 0 / 13 (0.000) | not compared | equal | not compared |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 5 dB in the passband (not restated in the 2500 Hz reference) | | | **verdict** | does not lose | better alone: ours by dominance | | switch on this path: arbitrate | |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 0 dB in the passband (not restated in the 2500 Hz reference) | exact | 1 | 011 | 0 / 21 (0.000) | 0 / 21 (0.000) | 4 / 21 (0.190) | equal | better |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 0 dB in the passband (not restated in the 2500 Hz reference) | exact | 1 | 010 | 0 / 0 (not defined) | 0 / 0 (not defined) | 4 / 5 (0.800) | not compared | not compared |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 0 dB in the passband (not restated in the 2500 Hz reference) | exact | 1 | 012 | 0 / 21 (0.000) | 0 / 21 (0.000) | 1 / 21 (0.048) | equal | WORSE |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 0 dB in the passband (not restated in the 2500 Hz reference) | exact | 1 | 014 | 0 / 0 (not defined) | 0 / 0 (not defined) | not compared | not compared | not compared |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 0 dB in the passband (not restated in the 2500 Hz reference) | exact | 1 | 081 | 6 / 7 (0.857) | 6 / 7 (0.857) | 7 / 7 (1.000) | equal | better |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 0 dB in the passband (not restated in the 2500 Hz reference) | exact | 1 | 083 | 0 / 0 (not defined) | 0 / 0 (not defined) | not compared | not compared | not compared |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 0 dB in the passband (not restated in the 2500 Hz reference) | | | **verdict** | loses on 012 to port | better alone: ours by order, at 011 | | switch on this path: ours alone | |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 15 dB in the passband (not restated in the 2500 Hz reference) | exact | 1 | 011 | 1 / 21 (0.048) | 1 / 21 (0.048) | 3 / 21 (0.143) | equal | better |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 15 dB in the passband (not restated in the 2500 Hz reference) | exact | 1 | 010 | 1 / 21 (0.048) | 1 / 21 (0.048) | 3 / 13 (0.231) | equal | better |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 15 dB in the passband (not restated in the 2500 Hz reference) | exact | 1 | 013 | 1 / 21 (0.048) | 1 / 21 (0.048) | 11 / 21 (0.524) | equal | better |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 15 dB in the passband (not restated in the 2500 Hz reference) | exact | 1 | 012 | 20 / 21 (0.952) | 20 / 21 (0.952) | 10 / 21 (0.476) | equal | better |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 15 dB in the passband (not restated in the 2500 Hz reference) | exact | 1 | 014 | 0 / 0 (not defined) | 0 / 0 (not defined) | not compared | not compared | not compared |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 15 dB in the passband (not restated in the 2500 Hz reference) | exact | 1 | 081 | 12 / 7 (1.714) | 12 / 7 (1.714) | 8 / 7 (1.143) | equal | WORSE |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 15 dB in the passband (not restated in the 2500 Hz reference) | exact | 1 | 083 | 0 / 4 (0.000) | 0 / 4 (0.000) | not compared | equal | not compared |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 15 dB in the passband (not restated in the 2500 Hz reference) | | | **verdict** | loses on 081 to port | better alone: ours by order, at 011 | | switch on this path: ours alone | |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 5 dB in the passband (not restated in the 2500 Hz reference) | exact | 1 | 011 | 11 / 21 (0.524) | 11 / 21 (0.524) | 2 / 21 (0.095) | equal | WORSE |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 5 dB in the passband (not restated in the 2500 Hz reference) | exact | 1 | 010 | 11 / 25 (0.440) | 11 / 25 (0.440) | 2 / 15 (0.133) | equal | WORSE |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 5 dB in the passband (not restated in the 2500 Hz reference) | exact | 1 | 012 | 14 / 21 (0.667) | 14 / 21 (0.667) | 13 / 21 (0.619) | equal | better |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 5 dB in the passband (not restated in the 2500 Hz reference) | exact | 1 | 014 | 0 / 0 (not defined) | 0 / 0 (not defined) | not compared | not compared | not compared |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 5 dB in the passband (not restated in the 2500 Hz reference) | exact | 1 | 081 | 8 / 7 (1.143) | 8 / 7 (1.143) | 11 / 7 (1.571) | equal | better |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 5 dB in the passband (not restated in the 2500 Hz reference) | exact | 1 | 083 | 1 / 8 (0.125) | 1 / 8 (0.125) | not compared | equal | not compared |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 5 dB in the passband (not restated in the 2500 Hz reference) | | | **verdict** | loses on 011 to port, 010 to port | better alone: port by order, at 011 | | switch on this path: port alone | |

## 5. HM-REQ-084's named spans as each reads them

- harness | arbitrated | ABOVE | unadjudicated/cw-2026-08-25-013637 | reads ` AB OVE ` | not met
- harness | ours alone | ABOVE | unadjudicated/cw-2026-08-25-013637 | reads ` AB OVE ` | not met
- harness | port alone | ABOVE | unadjudicated/cw-2026-08-25-013637 | reads ` JE` | not met
- live | arbitrated | ABOVE | unadjudicated/cw-2026-08-25-013637 | reads ` AB OVE ` | not met
- live | ours alone | ABOVE | unadjudicated/cw-2026-08-25-013637 | reads ` AB OVE ` | not met
- live | port alone | ABOVE | unadjudicated/cw-2026-08-25-013637 | reads `E` | not met
- harness | arbitrated | BREEZE | unadjudicated/cw-2026-08-25-013637 | reads ` BR EEZE ` | not met
- harness | ours alone | BREEZE | unadjudicated/cw-2026-08-25-013637 | reads ` BR EEZE ` | not met
- harness | port alone | BREEZE | unadjudicated/cw-2026-08-25-013637 | reads `BREEZ` | not met
- live | arbitrated | BREEZE | unadjudicated/cw-2026-08-25-013637 | reads ` BR EEZE ` | not met
- live | ours alone | BREEZE | unadjudicated/cw-2026-08-25-013637 | reads ` BR EEZE ` | not met
- live | port alone | BREEZE | unadjudicated/cw-2026-08-25-013637 | reads `BREEZ` | not met
- harness | arbitrated | FLEX | unadjudicated/cw-2026-08-25-021410 | reads ` FLENT ` | not met
- harness | ours alone | FLEX | unadjudicated/cw-2026-08-25-021410 | reads ` FLENT ` | not met
- harness | port alone | FLEX | unadjudicated/cw-2026-08-25-021410 | reads `ZA ` | not met
- live | arbitrated | FLEX | unadjudicated/cw-2026-08-25-021410 | reads ` FLENT ` | not met
- live | ours alone | FLEX | unadjudicated/cw-2026-08-25-021410 | reads ` FLENT ` | not met
- live | port alone | FLEX | unadjudicated/cw-2026-08-25-021410 | reads `L ` | not met
- both | all three | WEEKEND | unadjudicated/cw-2026-08-25-021410 | not measurable here - span not in the recording's audio: the file is the last 30 s the application held; its sidecar credits 37 of the line's characters to this file, and `WEEKEND` stands 60 characters back from the line's end, `THINKING` 47; the file's first keying is the `A` after `NG`
- both | all three | THINKING | unadjudicated/cw-2026-08-25-021410 | not measurable here - span not in the recording's audio, as `WEEKEND`
- both | all three | USED TO USE A FIRM | 011447 / 011514 | not measurable here - recording not in the tree

## 6. HM-REQ-013, 080 and 014 per row

- harness | real HF, all | arbitrated | 014 no dim emitted
- harness | real HF, all | ours alone | 014 no dim emitted
- harness | real HF, all | port alone | 014 not defined (emits no dim)
- harness | real HF, no CH-* profile, SNR_2500 not measured, sender TX-FARNS (CW_SPEC.md 6.4 and 10, HM-DEC-115's traffic net) | arbitrated | 014 no dim emitted
- harness | real HF, no CH-* profile, SNR_2500 not measured, sender TX-FARNS (CW_SPEC.md 6.4 and 10, HM-DEC-115's traffic net) | ours alone | 014 no dim emitted
- harness | real HF, no CH-* profile, SNR_2500 not measured, sender TX-FARNS (CW_SPEC.md 6.4 and 10, HM-DEC-115's traffic net) | port alone | 014 not defined (emits no dim)
- harness | real HF, no CH-* profile, SNR_2500 not measured, sender TX-ITU (CW_SPEC.md 10, the KD0UN capture) | arbitrated | 014 no dim emitted
- harness | real HF, no CH-* profile, SNR_2500 not measured, sender TX-ITU (CW_SPEC.md 10, the KD0UN capture) | ours alone | 014 no dim emitted
- harness | real HF, no CH-* profile, SNR_2500 not measured, sender TX-ITU (CW_SPEC.md 10, the KD0UN capture) | port alone | 014 not defined (emits no dim)
- harness | real HF, no CH-* profile, SNR_2500 not measured, sender TX-TIGHT (CW_SPEC.md 10, HM-DEC-101) | arbitrated | 014 no dim emitted
- harness | real HF, no CH-* profile, SNR_2500 not measured, sender TX-TIGHT (CW_SPEC.md 10, HM-DEC-101) | ours alone | 014 no dim emitted
- harness | real HF, no CH-* profile, SNR_2500 not measured, sender TX-TIGHT (CW_SPEC.md 10, HM-DEC-101) | port alone | 014 not defined (emits no dim)
- harness | real HF, no CH-* profile, SNR_2500 not measured, sender not stated in CW_SPEC.md | arbitrated | 014 no dim emitted
- harness | real HF, no CH-* profile, SNR_2500 not measured, sender not stated in CW_SPEC.md | ours alone | 014 no dim emitted
- harness | real HF, no CH-* profile, SNR_2500 not measured, sender not stated in CW_SPEC.md | port alone | 014 not defined (emits no dim)
- harness | synthetic, all | arbitrated | 014 no dim emitted
- harness | synthetic, all | ours alone | 014 no dim emitted
- harness | synthetic, all | port alone | 014 not defined (emits no dim)
- harness | synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 0 dB in the passband (not restated in the 2500 Hz reference) | arbitrated | 014 no dim emitted
- harness | synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 0 dB in the passband (not restated in the 2500 Hz reference) | ours alone | 014 no dim emitted
- harness | synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 0 dB in the passband (not restated in the 2500 Hz reference) | port alone | 014 not defined (emits no dim)
- harness | synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 15 dB in the passband (not restated in the 2500 Hz reference) | arbitrated | 013 not met | 080 met | 014 no dim emitted
- harness | synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 15 dB in the passband (not restated in the 2500 Hz reference) | ours alone | 013 not met | 080 met | 014 no dim emitted
- harness | synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 15 dB in the passband (not restated in the 2500 Hz reference) | port alone | 013 not met | 080 not met | 014 not defined (emits no dim)
- harness | synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 5 dB in the passband (not restated in the 2500 Hz reference) | arbitrated | 014 no dim emitted
- harness | synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 5 dB in the passband (not restated in the 2500 Hz reference) | ours alone | 014 no dim emitted
- harness | synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 5 dB in the passband (not restated in the 2500 Hz reference) | port alone | 014 not defined (emits no dim)
- harness | synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 0 dB in the passband (not restated in the 2500 Hz reference) | arbitrated | 014 no dim emitted
- harness | synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 0 dB in the passband (not restated in the 2500 Hz reference) | ours alone | 014 no dim emitted
- harness | synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 0 dB in the passband (not restated in the 2500 Hz reference) | port alone | 014 not defined (emits no dim)
- harness | synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 15 dB in the passband (not restated in the 2500 Hz reference) | arbitrated | 013 not met | 080 not met | 014 no dim emitted
- harness | synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 15 dB in the passband (not restated in the 2500 Hz reference) | ours alone | 013 not met | 080 not met | 014 no dim emitted
- harness | synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 15 dB in the passband (not restated in the 2500 Hz reference) | port alone | 013 not met | 080 not met | 014 not defined (emits no dim)
- harness | synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 5 dB in the passband (not restated in the 2500 Hz reference) | arbitrated | 014 no dim emitted
- harness | synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 5 dB in the passband (not restated in the 2500 Hz reference) | ours alone | 014 no dim emitted
- harness | synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 5 dB in the passband (not restated in the 2500 Hz reference) | port alone | 014 not defined (emits no dim)
- live | real HF, all | arbitrated | 014 no dim emitted
- live | real HF, all | ours alone | 014 no dim emitted
- live | real HF, all | port alone | 014 not defined (emits no dim)
- live | real HF, no CH-* profile, SNR_2500 not measured, sender TX-FARNS (CW_SPEC.md 6.4 and 10, HM-DEC-115's traffic net) | arbitrated | 014 no dim emitted
- live | real HF, no CH-* profile, SNR_2500 not measured, sender TX-FARNS (CW_SPEC.md 6.4 and 10, HM-DEC-115's traffic net) | ours alone | 014 no dim emitted
- live | real HF, no CH-* profile, SNR_2500 not measured, sender TX-FARNS (CW_SPEC.md 6.4 and 10, HM-DEC-115's traffic net) | port alone | 014 not defined (emits no dim)
- live | real HF, no CH-* profile, SNR_2500 not measured, sender TX-ITU (CW_SPEC.md 10, the KD0UN capture) | arbitrated | 014 no dim emitted
- live | real HF, no CH-* profile, SNR_2500 not measured, sender TX-ITU (CW_SPEC.md 10, the KD0UN capture) | ours alone | 014 no dim emitted
- live | real HF, no CH-* profile, SNR_2500 not measured, sender TX-ITU (CW_SPEC.md 10, the KD0UN capture) | port alone | 014 not defined (emits no dim)
- live | real HF, no CH-* profile, SNR_2500 not measured, sender TX-TIGHT (CW_SPEC.md 10, HM-DEC-101) | arbitrated | 014 no dim emitted
- live | real HF, no CH-* profile, SNR_2500 not measured, sender TX-TIGHT (CW_SPEC.md 10, HM-DEC-101) | ours alone | 014 no dim emitted
- live | real HF, no CH-* profile, SNR_2500 not measured, sender TX-TIGHT (CW_SPEC.md 10, HM-DEC-101) | port alone | 014 not defined (emits no dim)
- live | real HF, no CH-* profile, SNR_2500 not measured, sender not stated in CW_SPEC.md | arbitrated | 014 no dim emitted
- live | real HF, no CH-* profile, SNR_2500 not measured, sender not stated in CW_SPEC.md | ours alone | 014 no dim emitted
- live | real HF, no CH-* profile, SNR_2500 not measured, sender not stated in CW_SPEC.md | port alone | 014 not defined (emits no dim)
- live | synthetic, all | arbitrated | 014 no dim emitted
- live | synthetic, all | ours alone | 014 no dim emitted
- live | synthetic, all | port alone | 014 not defined (emits no dim)
- live | synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 0 dB in the passband (not restated in the 2500 Hz reference) | arbitrated | 014 no dim emitted
- live | synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 0 dB in the passband (not restated in the 2500 Hz reference) | ours alone | 014 no dim emitted
- live | synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 0 dB in the passband (not restated in the 2500 Hz reference) | port alone | 014 not defined (emits no dim)
- live | synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 15 dB in the passband (not restated in the 2500 Hz reference) | arbitrated | 013 not met | 080 met | 014 no dim emitted
- live | synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 15 dB in the passband (not restated in the 2500 Hz reference) | ours alone | 013 not met | 080 met | 014 no dim emitted
- live | synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 15 dB in the passband (not restated in the 2500 Hz reference) | port alone | 013 not met | 080 not met | 014 not defined (emits no dim)
- live | synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 5 dB in the passband (not restated in the 2500 Hz reference) | arbitrated | 014 no dim emitted
- live | synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 5 dB in the passband (not restated in the 2500 Hz reference) | ours alone | 014 no dim emitted
- live | synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 5 dB in the passband (not restated in the 2500 Hz reference) | port alone | 014 not defined (emits no dim)
- live | synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 0 dB in the passband (not restated in the 2500 Hz reference) | arbitrated | 014 no dim emitted
- live | synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 0 dB in the passband (not restated in the 2500 Hz reference) | ours alone | 014 no dim emitted
- live | synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 0 dB in the passband (not restated in the 2500 Hz reference) | port alone | 014 not defined (emits no dim)
- live | synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 15 dB in the passband (not restated in the 2500 Hz reference) | arbitrated | 013 not met | 080 not met | 014 no dim emitted
- live | synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 15 dB in the passband (not restated in the 2500 Hz reference) | ours alone | 013 not met | 080 not met | 014 no dim emitted
- live | synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 15 dB in the passband (not restated in the 2500 Hz reference) | port alone | 013 not met | 080 not met | 014 not defined (emits no dim)
- live | synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 5 dB in the passband (not restated in the 2500 Hz reference) | arbitrated | 014 no dim emitted
- live | synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 5 dB in the passband (not restated in the 2500 Hz reference) | ours alone | 014 no dim emitted
- live | synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 5 dB in the passband (not restated in the 2500 Hz reference) | port alone | 014 not defined (emits no dim)

## 7. What the table does not prove

- **Every real key is inferred** (V-13), and **the synthetic set is never sole evidence** (CLAUDE.md 12.5).
- **No row is a CH-* condition**, so no must-tier verdict here is the requirement's own; each is the tree's nearest row, labelled.
- **The port's figures are at parity.md section 1's all-sure mapping**; its MET-CER-SURE is its whole character error.
- **Who votes is `CwVoteTable`'s**, calibration.md section 2's held-out verdicts, unchanged here; the switch sits beside it and does not edit it.
