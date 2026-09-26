# Parity: Hamlet's decoder and the fldigi port on the same audio (HM-REQ-123)

Written by `BothDecodersAreScoredAlikeTests.TheParityTableIsWritten` (work instruction 458, PHASE_PLAN.md 9.2). Both decoders start cold at sample 0 of each file with nothing added or removed. Ours is fed hop by hop from 600 Hz, as `TheRequirementsAreMeasuredTests` feeds it; the port is given the file at 8000 Hz through `FldigiRateAdapter` and the pitch instrument's median over the file's keyed windows, with fldigi's shipped defaults at `61b97f41` and the squelch off. Both texts go through `TheRequirementsAreMeasuredTests.Measure` and the same `CwMetrics` calls, each stretch located in each decoder's own text by the same rule. Neither decoder was changed. Every number carries its key's kind (V-13).

## 1. The mapping of the port's unclassed output

458 DECIDED (2), the author's, overrulable: every non-space character the port prints is scored as sure, because fldigi shows every character alike and an operator reads it as asserted (CLAUDE.md 0.0), and mapping it to dim would hide its errors from MET-CER-SURE; its no-match output at `rx_lookup`'s caller is scored as a placeholder, as ours is; its spaces are word boundaries as emitted, 456's 5-unit finding included.

- **No-match output:** `rx_lookup` returns `""` when the table has no entry (`src/cw_rtty/morse.cxx:254`); its caller then prints `CW_noise` (`src/cw_rtty/cw.cxx:892-898`), `*` by default (`src/include/configuration.h:249-251`). `*` is not in fldigi's table, so it is only ever this. Scored as a placeholder: never wrong, never coverage.
- **Characters:** the table's printed form, `<BT>` for a prosign with `CW_prosign_display` off, accented letters and `_` included, each scored as itself at sure. None is mapped to another character or to dim.
- **Spaces:** printed once after more than 4 dot lengths of silence (`cw.cxx:909-914`), each a word boundary.

Counts over everything the port printed on the 35 files: 613 sure, 24 placeholder (`*`), 181 word boundaries, 0 not sure. Inside the scored stretches: 407 sure, 13 placeholder, 114 word boundaries between scored characters.

## 2. Per recording

MET-CER-SURE is sure characters wrong or added of sure emitted; MET-INVENTED is sure added plus sure wrong over characters sent; coverage is sure and right over characters sent (R82); MET-WBE is word boundaries inserted plus deleted over words sent. Summed over a recording's stretches.

| recording | key | CER-SURE ours | CER-SURE port | INVENTED ours | INVENTED port | coverage ours | coverage port | WBE ours | WBE port |
|---|---|---|---|---|---|---|---|---|---|
| unadjudicated/cw-2026-09-23-173723 | inferred | 3 of 20 (0.150) | 1 of 8 (0.125) | 3 / 20 (0.150) | 1 / 20 (0.050) | 17 / 20 (0.850) | 7 / 20 (0.350) | 7 (6 ins, 1 del) / 6 (1.167) | 6 (1 ins, 5 del) / 6 (1.000) |
| cw-2026-08-17-013347 | inferred | 0 of 6 (0.000) | 2 of 3 (0.667) | 0 / 6 (0.000) | 2 / 6 (0.333) | 6 / 6 (1.000) | 1 / 6 (0.167) | 0 (0 ins, 0 del) / 1 (0.000) | 0 (0 ins, 0 del) / 1 (0.000) |
| cw-2026-08-17-134712 | inferred | 0 of 3 (0.000) | 0 of 2 (0.000) | 0 / 3 (0.000) | 0 / 3 (0.000) | 3 / 3 (1.000) | 2 / 3 (0.667) | 0 (0 ins, 0 del) / 1 (0.000) | 0 (0 ins, 0 del) / 1 (0.000) |
| unadjudicated/cw-2026-08-18-003758 | inferred | 3 of 11 (0.273) | 2 of 10 (0.200) | 3 / 11 (0.273) | 2 / 11 (0.182) | 8 / 11 (0.727) | 8 / 11 (0.727) | 0 (0 ins, 0 del) / 2 (0.000) | 2 (1 ins, 1 del) / 2 (1.000) |
| unadjudicated/cw-2026-08-24-012403 | inferred | 0 of 13 (0.000) | 1 of 6 (0.167) | 0 / 13 (0.000) | 1 / 13 (0.077) | 13 / 13 (1.000) | 5 / 13 (0.385) | 0 (0 ins, 0 del) / 4 (0.000) | 3 (0 ins, 3 del) / 4 (0.750) |
| cw-2026-08-18-004507 | inferred | 0 of 43 (0.000) | 3 of 33 (0.091) | 0 / 44 (0.000) | 3 / 44 (0.068) | 43 / 44 (0.977) | 30 / 44 (0.682) | 5 (5 ins, 0 del) / 11 (0.455) | 4 (2 ins, 2 del) / 11 (0.364) |
| unadjudicated/cw-2026-08-22-031838 | inferred | 5 of 21 (0.238) | 3 of 13 (0.231) | 5 / 26 (0.192) | 3 / 26 (0.115) | 16 / 26 (0.615) | 10 / 26 (0.385) | 1 (0 ins, 1 del) / 10 (0.100) | 6 (0 ins, 6 del) / 10 (0.600) |
| unadjudicated/cw-2026-08-22-031905 | inferred | 2 of 32 (0.063) | 4 of 20 (0.200) | 2 / 33 (0.061) | 4 / 33 (0.121) | 30 / 33 (0.909) | 16 / 33 (0.485) | 1 (1 ins, 0 del) / 7 (0.143) | 7 (1 ins, 6 del) / 7 (1.000) |
| unadjudicated/cw-2026-08-22-031948 | inferred | 1 of 27 (0.037) | 2 of 15 (0.133) | 1 / 28 (0.036) | 2 / 28 (0.071) | 26 / 28 (0.929) | 13 / 28 (0.464) | 0 (0 ins, 0 del) / 9 (0.000) | 5 (0 ins, 5 del) / 9 (0.556) |
| unadjudicated/cw-2026-08-22-032012 | inferred | 3 of 43 (0.070) | 3 of 16 (0.188) | 3 / 42 (0.071) | 3 / 42 (0.071) | 40 / 42 (0.952) | 13 / 42 (0.310) | 2 (1 ins, 1 del) / 10 (0.200) | 8 (0 ins, 8 del) / 10 (0.800) |
| unadjudicated/cw-2026-08-22-032050 | inferred | 5 of 40 (0.125) | 5 of 15 (0.333) | 5 / 50 (0.100) | 5 / 50 (0.100) | 35 / 50 (0.700) | 10 / 50 (0.200) | 2 (1 ins, 1 del) / 10 (0.200) | 9 (1 ins, 8 del) / 10 (0.900) |
| unadjudicated/cw-2026-08-22-032113 | inferred | 1 of 22 (0.045) | 2 of 9 (0.222) | 1 / 25 (0.040) | 2 / 25 (0.080) | 21 / 25 (0.840) | 7 / 25 (0.280) | 3 (3 ins, 0 del) / 4 (0.750) | 3 (1 ins, 2 del) / 4 (0.750) |
| unadjudicated/cw-2026-08-22-032129 | inferred | 5 of 39 (0.128) | 3 of 14 (0.214) | 5 / 38 (0.132) | 3 / 38 (0.079) | 34 / 38 (0.895) | 11 / 38 (0.289) | 0 (0 ins, 0 del) / 5 (0.000) | 3 (0 ins, 3 del) / 5 (0.600) |
| unadjudicated/cw-2026-09-24-004108 | inferred | 2 of 4 (0.500) | 0 of 1 (0.000) | 2 / 8 (0.250) | 0 / 8 (0.000) | 2 / 8 (0.250) | 1 / 8 (0.125) | 1 (1 ins, 0 del) / 2 (0.500) | 1 (0 ins, 1 del) / 2 (0.500) |
| unadjudicated/cw-2026-09-24-004133 | inferred | 0 of 5 (0.000) | 3 of 4 (0.750) | 0 / 6 (0.000) | 3 / 6 (0.500) | 5 / 6 (0.833) | 1 / 6 (0.167) | 2 (2 ins, 0 del) / 1 (2.000) | 1 (1 ins, 0 del) / 1 (1.000) |
| unadjudicated/cw-2026-09-24-004205 | inferred | 0 of 8 (0.000) | 1 of 5 (0.200) | 0 / 8 (0.000) | 1 / 8 (0.125) | 8 / 8 (1.000) | 4 / 8 (0.500) | 1 (1 ins, 0 del) / 3 (0.333) | 1 (1 ins, 0 del) / 3 (0.333) |
| unadjudicated/cw-2026-09-24-004234 | inferred | 2 of 9 (0.222) | 1 of 6 (0.167) | 2 / 14 (0.143) | 1 / 14 (0.071) | 7 / 14 (0.500) | 5 / 14 (0.357) | 4 (2 ins, 2 del) / 5 (0.800) | 5 (2 ins, 3 del) / 5 (1.000) |
| unadjudicated/cw-2026-09-24-004322 | inferred | 0 of 32 (0.000) | 10 of 21 (0.476) | 0 / 40 (0.000) | 10 / 40 (0.250) | 32 / 40 (0.800) | 11 / 40 (0.275) | 6 (4 ins, 2 del) / 8 (0.750) | 9 (5 ins, 4 del) / 8 (1.125) |
| unadjudicated/cw-2026-09-24-004347 | inferred | 1 of 24 (0.042) | 9 of 15 (0.600) | 1 / 23 (0.043) | 9 / 23 (0.391) | 23 / 23 (1.000) | 6 / 23 (0.261) | 1 (1 ins, 0 del) / 6 (0.167) | 7 (4 ins, 3 del) / 6 (1.167) |
| unadjudicated/cw-2026-09-24-004405 | inferred | 0 of 6 (0.000) | 0 of 3 (0.000) | 0 / 6 (0.000) | 0 / 6 (0.000) | 6 / 6 (1.000) | 3 / 6 (0.500) | 0 (0 ins, 0 del) / 1 (0.000) | 2 (2 ins, 0 del) / 1 (2.000) |
| unadjudicated/cw-2026-09-24-004427 | inferred | 0 of 9 (0.000) | 5 of 8 (0.625) | 0 / 10 (0.000) | 5 / 10 (0.500) | 9 / 10 (0.900) | 3 / 10 (0.300) | 0 (0 ins, 0 del) / 3 (0.000) | 1 (1 ins, 0 del) / 3 (0.333) |
| unadjudicated/cw-2026-09-24-004510 | inferred | 0 of 11 (0.000) | 2 of 7 (0.286) | 0 / 11 (0.000) | 2 / 11 (0.182) | 11 / 11 (1.000) | 5 / 11 (0.455) | 0 (0 ins, 0 del) / 2 (0.000) | 1 (1 ins, 0 del) / 2 (0.500) |
| unadjudicated/cw-2026-09-24-004550 | inferred | 0 of 8 (0.000) | 0 of 5 (0.000) | 0 / 8 (0.000) | 0 / 8 (0.000) | 8 / 8 (1.000) | 5 / 8 (0.625) | 1 (1 ins, 0 del) / 2 (0.500) | 2 (1 ins, 1 del) / 2 (1.000) |
| cq-12wpm-15db | exact | 0 of 21 (0.000) | 0 of 19 (0.000) | 0 / 21 (0.000) | 0 / 21 (0.000) | 21 / 21 (1.000) | 19 / 21 (0.905) | 0 (0 ins, 0 del) / 7 (0.000) | 1 (0 ins, 1 del) / 7 (0.143) |
| cq-12wpm-5db | exact | 0 of 21 (0.000) | 1 of 15 (0.067) | 0 / 21 (0.000) | 1 / 21 (0.048) | 21 / 21 (1.000) | 14 / 21 (0.667) | 0 (0 ins, 0 del) / 7 (0.000) | 1 (0 ins, 1 del) / 7 (0.143) |
| cq-12wpm-0db | exact | 0 of 0 (no number) | 11 of 13 (0.846) | 0 / 21 (0.000) | 11 / 21 (0.524) | 0 / 21 (0.000) | 2 / 21 (0.095) | 6 (0 ins, 6 del) / 7 (0.857) | 4 (1 ins, 3 del) / 7 (0.571) |
| cq-18wpm-15db | exact | 0 of 21 (0.000) | 1 of 19 (0.053) | 0 / 21 (0.000) | 1 / 21 (0.048) | 21 / 21 (1.000) | 18 / 21 (0.857) | 0 (0 ins, 0 del) / 7 (0.000) | 1 (0 ins, 1 del) / 7 (0.143) |
| cq-18wpm-5db | exact | 0 of 20 (0.000) | 2 of 17 (0.118) | 0 / 21 (0.000) | 2 / 21 (0.095) | 20 / 21 (0.952) | 15 / 21 (0.714) | 0 (0 ins, 0 del) / 7 (0.000) | 3 (0 ins, 3 del) / 7 (0.429) |
| cq-18wpm-0db | exact | 0 of 0 (no number) | 5 of 7 (0.714) | 0 / 21 (0.000) | 5 / 21 (0.238) | 0 / 21 (0.000) | 2 / 21 (0.095) | 6 (0 ins, 6 del) / 7 (0.857) | 5 (0 ins, 5 del) / 7 (0.714) |
| cq-25wpm-15db | exact | 1 of 22 (0.045) | 1 of 18 (0.056) | 1 / 21 (0.048) | 1 / 21 (0.048) | 21 / 21 (1.000) | 17 / 21 (0.810) | 0 (0 ins, 0 del) / 7 (0.000) | 1 (0 ins, 1 del) / 7 (0.143) |
| cq-25wpm-5db | exact | 1 of 22 (0.045) | 2 of 15 (0.133) | 1 / 21 (0.048) | 2 / 21 (0.095) | 21 / 21 (1.000) | 13 / 21 (0.619) | 0 (0 ins, 0 del) / 7 (0.000) | 2 (0 ins, 2 del) / 7 (0.286) |
| cq-25wpm-0db | exact | 0 of 0 (no number) | 5 of 7 (0.714) | 0 / 21 (0.000) | 5 / 21 (0.238) | 0 / 21 (0.000) | 2 / 21 (0.095) | 6 (0 ins, 6 del) / 7 (0.857) | 6 (0 ins, 6 del) / 7 (0.857) |
| cq-18wpm-15db-char5 | exact | 1 of 21 (0.048) | 1 of 19 (0.053) | 1 / 21 (0.048) | 1 / 21 (0.048) | 20 / 21 (0.952) | 18 / 21 (0.857) | 12 (7 ins, 5 del) / 7 (1.714) | 14 (13 ins, 1 del) / 7 (2.000) |
| cq-18wpm-5db-char5 | exact | 11 of 25 (0.440) | 3 of 16 (0.188) | 11 / 21 (0.524) | 3 / 21 (0.143) | 14 / 21 (0.667) | 13 / 21 (0.619) | 8 (6 ins, 2 del) / 7 (1.143) | 12 (10 ins, 2 del) / 7 (1.714) |
| cq-18wpm-0db-char5 | exact | 0 of 0 (no number) | 2 of 3 (0.667) | 0 / 21 (0.000) | 2 / 21 (0.095) | 0 / 21 (0.000) | 1 / 21 (0.048) | 6 (0 ins, 6 del) / 7 (0.857) | 6 (0 ins, 6 del) / 7 (0.857) |

## 3. Per condition

The two conditions the tree has: real recordings with inferred keys, and the synthetic set with exact keys (458 DECIDED (4)). **Neither is a `CH-*` condition, and no real capture is counted toward one (PHASE_PLAN.md 7.4).** Under each, the finer rows `TheRequirementsAreMeasuredTests` states: for a real recording the sender `CW_SPEC.md` section 10 names, for a synthetic case its character gap and in-passband level.

| condition | key | recordings | CER-SURE ours | CER-SURE port | INVENTED ours | INVENTED port | coverage ours | coverage port | WBE ours | WBE port |
|---|---|---|---|---|---|---|---|---|---|---|
| **real HF, all** | inferred | ours 23 of 23, port 23 of 23 | 33 of 436 (0.076) | 62 of 239 (0.259) | 33 / 473 (0.070) | 62 / 473 (0.131) | 403 / 473 (0.852) | 177 / 473 (0.374) | 37 (29 ins, 8 del) / 113 (0.327) | 86 (25 ins, 61 del) / 113 (0.761) |
| real HF, no CH-* profile, SNR_2500 not measured, sender TX-FARNS (CW_SPEC.md 6.4 and 10, HM-DEC-115's traffic net) | inferred | ours 1 of 1, port 1 of 1 | 0 of 43 (0.000) | 3 of 33 (0.091) | 0 / 44 (0.000) | 3 / 44 (0.068) | 43 / 44 (0.977) | 30 / 44 (0.682) | 5 (5 ins, 0 del) / 11 (0.455) | 4 (2 ins, 2 del) / 11 (0.364) |
| real HF, no CH-* profile, SNR_2500 not measured, sender TX-ITU (CW_SPEC.md 10, the KD0UN capture) | inferred | ours 1 of 1, port 1 of 1 | 0 of 13 (0.000) | 1 of 6 (0.167) | 0 / 13 (0.000) | 1 / 13 (0.077) | 13 / 13 (1.000) | 5 / 13 (0.385) | 0 (0 ins, 0 del) / 4 (0.000) | 3 (0 ins, 3 del) / 4 (0.750) |
| real HF, no CH-* profile, SNR_2500 not measured, sender TX-TIGHT (CW_SPEC.md 10, HM-DEC-101) | inferred | ours 1 of 1, port 1 of 1 | 0 of 6 (0.000) | 2 of 3 (0.667) | 0 / 6 (0.000) | 2 / 6 (0.333) | 6 / 6 (1.000) | 1 / 6 (0.167) | 0 (0 ins, 0 del) / 1 (0.000) | 0 (0 ins, 0 del) / 1 (0.000) |
| real HF, no CH-* profile, SNR_2500 not measured, sender not stated in CW_SPEC.md | inferred | ours 20 of 20, port 20 of 20 | 33 of 374 (0.088) | 56 of 197 (0.284) | 33 / 410 (0.080) | 56 / 410 (0.137) | 341 / 410 (0.832) | 141 / 410 (0.344) | 32 (24 ins, 8 del) / 97 (0.330) | 79 (23 ins, 56 del) / 97 (0.814) |
| **synthetic, all** | exact | ours 12 of 12, port 12 of 12 | 14 of 173 (0.081) | 34 of 168 (0.202) | 14 / 252 (0.056) | 34 / 252 (0.135) | 159 / 252 (0.631) | 134 / 252 (0.532) | 44 (13 ins, 31 del) / 84 (0.524) | 56 (24 ins, 32 del) / 84 (0.667) |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 0 dB in the passband (not restated in the 2500 Hz reference) | exact | ours 3 of 3, port 3 of 3 | 0 of 0 (no number) | 21 of 27 (0.778) | 0 / 63 (0.000) | 21 / 63 (0.333) | 0 / 63 (0.000) | 6 / 63 (0.095) | 18 (0 ins, 18 del) / 21 (0.857) | 15 (1 ins, 14 del) / 21 (0.714) |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 15 dB in the passband (not restated in the 2500 Hz reference) | exact | ours 3 of 3, port 3 of 3 | 1 of 64 (0.016) | 2 of 56 (0.036) | 1 / 63 (0.016) | 2 / 63 (0.032) | 63 / 63 (1.000) | 54 / 63 (0.857) | 0 (0 ins, 0 del) / 21 (0.000) | 3 (0 ins, 3 del) / 21 (0.143) |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 5 dB in the passband (not restated in the 2500 Hz reference) | exact | ours 3 of 3, port 3 of 3 | 1 of 63 (0.016) | 5 of 47 (0.106) | 1 / 63 (0.016) | 5 / 63 (0.079) | 62 / 63 (0.984) | 42 / 63 (0.667) | 0 (0 ins, 0 del) / 21 (0.000) | 6 (0 ins, 6 del) / 21 (0.286) |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 0 dB in the passband (not restated in the 2500 Hz reference) | exact | ours 1 of 1, port 1 of 1 | 0 of 0 (no number) | 2 of 3 (0.667) | 0 / 21 (0.000) | 2 / 21 (0.095) | 0 / 21 (0.000) | 1 / 21 (0.048) | 6 (0 ins, 6 del) / 7 (0.857) | 6 (0 ins, 6 del) / 7 (0.857) |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 15 dB in the passband (not restated in the 2500 Hz reference) | exact | ours 1 of 1, port 1 of 1 | 1 of 21 (0.048) | 1 of 19 (0.053) | 1 / 21 (0.048) | 1 / 21 (0.048) | 20 / 21 (0.952) | 18 / 21 (0.857) | 12 (7 ins, 5 del) / 7 (1.714) | 14 (13 ins, 1 del) / 7 (2.000) |
| synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 5 dB in the passband (not restated in the 2500 Hz reference) | exact | ours 1 of 1, port 1 of 1 | 11 of 25 (0.440) | 3 of 16 (0.188) | 11 / 21 (0.524) | 3 / 21 (0.143) | 14 / 21 (0.667) | 13 / 21 (0.619) | 8 (6 ins, 2 del) / 7 (1.143) | 12 (10 ins, 2 del) / 7 (1.714) |

## 4. Decode time

Wall time inside each decoder on this machine, summed per condition: ours from the first hop to `Flush`; the port inside `rx_process`, with `FldigiRateAdapter`'s resampling to 8000 Hz given apart. The pitch instrument is in neither. These figures move from run to run; nothing asserts them.

| condition | recordings | audio s | ours s | port s | resampling s |
|---|---|---|---|---|---|
| real HF, inferred keys | 23 | 690.0 | 52.60 | 3.70 | 8.94 |
| synthetic, exact keys | 12 | 279.5 | 8.38 | 1.48 | 0.00 |

## 5. What the table does not prove

- **The keys on the real rows are inferred** (V-13): reasoned from the form of a call, adjudicated, or differenced from consecutive transcripts, never transcribed. A disagreement with one is not by itself proof either decoder is wrong.
- **The synthetic set is never sole evidence** (CLAUDE.md 12.5): one generator, one text, shaped band noise not shown to be `CH-AWGN`, and a 1.0 s lead-in of noise before the first mark, where the port loses its first element (457's verdict (b), fldigi's own); that loss is scored here, not excused.
- **Neither decoder has a calibrated confidence yet** (HM-REQ-124, 9.5). The port has none at all and every character it prints is counted sure by the mapping above, so its MET-CER-SURE is its whole character error; ours marks some characters as placeholders, which are never wrong. The two MET-CER-SURE columns therefore do not measure the same kind of restraint.
- **The port runs on fldigi's shipped defaults at 18 WPM with tracking on**, given the instrument's pitch; fldigi at the radio would take its pitch from the operator's cursor. Nothing here says how fldigi performs with other settings.
- **The stretches are located in each decoder's own text by the same rule.** `CwScorer.Within` fits each key to the text with free ends, independently, so on a text far from its keys two stretches can fall on overlapping characters, and a text that reads little can be fitted where it happens to resemble the key. It is the rule ours is scored by, unchanged.
