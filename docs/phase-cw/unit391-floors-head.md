# Unit 391 - the three CW floor tests at HEAD (step 0, criterion 0.1)

Work instruction 391 task 1, 2026-09-22, at 3d6a2c12 (src unchanged from 8a4d1d77). One type
per invocation, filtered, foregrounded in a script, `--logger "console;verbosity=detailed"`,
a status line before each. Raw output: `.run-unit/unit391-floors-head-{1,2,3}.txt` (not committed).

**These are counts, not correctness** (CLAUDE.md 0.0). A green case says the decoder produced at
least as many characters and elements as on the day the floor was set, and nothing about whether
they are right.

## Totals

| Type | Cases | Green | Red | Wall |
|---|---|---|---|---|
| `TheCapturesThatDecodeKeepDecodingTests.EachStillProducesWhatItDid` | 37 | 19 | **18** | 1995 s |
| `TheAdjudicatedReadingsKeepReadingTests` (12 readings and 1 fact) | 13 | 13 | 0 | 364 s |
| `CwFixtureTests.TheCleanRecordingsDecodeExactly` | 2 | 0 | **2** | 10 s |
| **All three** | **52** | **32** | **20** | |

No run was lost. The first captures run under the instruction's `timeout 900` was cut off by the
timeout at 18 of 37 cases (8 red, every one matching the full run); the type was re-run whole
under 2700 s, and that run is the one tabled.

## TheCapturesThatDecodeKeepDecodingTests - every case

Characters and elements measured at HEAD against their floors; diff is measured minus floor.
Unsure is printed and never asserted (the class's own remarks); the number in brackets is what was
marked when the floor was set. Where the count floor is retired under Tim's 2026-08-25 ruling (an
adjudicated anchor covers the recording) only the element floor is asserted.

| Capture | Result | Chars | Floor | Diff | Elements | Floor | Diff | Unsure (then) | Tone Hz |
|---|---|---|---|---|---|---|---|---|---|
| `cw-2026-08-17-013347` | green (count floor retired, anchor) | 58 | 59 | -1 | 108 | 108 | 0 | 21 (2) | 614 |
| `cw-2026-08-17-134712` | red (count floor retired, anchor) | 54 | 63 | -9 | 81 | 98 | -17 | 36 (10) | 501 |
| `cw-2026-08-18-004507` | green (count floor retired, anchor) | 50 | 50 | 0 | 119 | 118 | 1 | 2 (1) | 501 |
| `unadjudicated/cw-2026-08-24-012403` | green (count floor retired, anchor) | 24 | 22 | 2 | 71 | 65 | 6 | 4 (0) | 440 |
| `unadjudicated/cw-2026-08-22-031838` | red (count floor retired, anchor) | 33 | 57 | -24 | 116 | 126 | -10 | 6 (3) | 500 |
| `unadjudicated/cw-2026-08-22-031905` | green (count floor retired, anchor) | 37 | 42 | -5 | 120 | 118 | 2 | 5 (6) | 500 |
| `unadjudicated/cw-2026-08-22-031948` | green (count floor retired, anchor) | 31 | 34 | -3 | 119 | 114 | 5 | 0 (3) | 500 |
| `unadjudicated/cw-2026-08-22-032012` | red (count floor retired, anchor) | 43 | 44 | -1 | 119 | 120 | -1 | 5 (1) | 500 |
| `unadjudicated/cw-2026-08-22-032050` | red (count floor retired, anchor) | 49 | 53 | -4 | 115 | 123 | -8 | 9 (9) | 500 |
| `unadjudicated/cw-2026-08-22-032113` | green (count floor retired, anchor) | 48 | 55 | -7 | 126 | 118 | 8 | 11 (8) | 500 |
| `unadjudicated/cw-2026-08-22-032129` | green (count floor retired, anchor) | 43 | 66 | -23 | 123 | 119 | 4 | 8 (1) | 500 |
| `cw-2026-08-17-013622` | red | 53 | 55 | -2 | 86 | 84 | 2 | 23 (0) | 601 |
| `unadjudicated/cw-2026-08-18-003016` | red | 54 | 57 | -3 | 146 | 149 | -3 | 1 (3) | 669 |
| `unadjudicated/cw-2026-08-18-003126` | red | 53 | 54 | -1 | 142 | 144 | -2 | 8 (6) | 669 |
| `unadjudicated/cw-2026-08-18-003758` | green (count floor retired, anchor) | 61 | 63 | -2 | 123 | 121 | 2 | 19 (10) | 498 |
| `unadjudicated/cw-2026-08-23-001520` | red | 7 | 5 | 2 | 39 | 45 | -6 | 6 (1) | 600 |
| `unadjudicated/cw-2026-08-23-001831` | red | 53 | 55 | -2 | 124 | 124 | 0 | 18 (10) | 527 |
| `unadjudicated/cw-2026-08-23-001952` | red | 60 | 75 | -15 | 120 | 142 | -22 | 28 (13) | 521 |
| `unadjudicated/cw-2026-08-23-002016` | green | 75 | 75 | 0 | 136 | 136 | 0 | 34 (17) | 521 |
| `unadjudicated/cw-2026-08-25-011552` | green | 32 | 30 | 2 | 89 | 89 | 0 | 10 (8) | 500 |
| `unadjudicated/cw-2026-08-25-012748` | green | 2 | 2 | 0 | 4 | 4 | 0 | 0 (0) | 400 |
| `unadjudicated/cw-2026-08-25-012823` | red | 35 | 41 | -6 | 57 | 62 | -5 | 27 (15) | 500 |
| `unadjudicated/cw-2026-08-25-012922` | red | 44 | 50 | -6 | 111 | 112 | -1 | 11 (5) | 492 |
| `unadjudicated/cw-2026-08-25-013010` | green | 56 | 54 | 2 | 132 | 131 | 1 | 10 (6) | 501 |
| `unadjudicated/cw-2026-08-25-013150` | red | 61 | 58 | 3 | 132 | 139 | -7 | 23 (7) | 501 |
| `unadjudicated/cw-2026-08-25-013303` | red | 52 | 54 | -2 | 141 | 146 | -5 | 14 (10) | 501 |
| `unadjudicated/cw-2026-08-25-013402` | red | 59 | 61 | -2 | 154 | 161 | -7 | 10 (5) | 536 |
| `unadjudicated/cw-2026-08-25-013520` | green | 62 | 60 | 2 | 155 | 153 | 2 | 8 (5) | 536 |
| `unadjudicated/cw-2026-08-25-013637` | red | 62 | 63 | -1 | 158 | 164 | -6 | 13 (3) | 536 |
| `unadjudicated/cw-2026-08-25-021410` | red | 40 | 47 | -7 | 97 | 99 | -2 | 6 (11) | 540 |
| `unadjudicated/cw-2026-08-25-021629` | red | 26 | 47 | -21 | 71 | 96 | -25 | 10 (20) | 504 |
| `unadjudicated/cw-2026-08-25-021825` | green | 61 | 41 | 20 | 94 | 74 | 20 | 39 (16) | 394 |
| `unadjudicated/cw-2026-08-26-125941` | green | 0 | 0 | 0 | 0 | 0 | 0 | 0 (0) | 404 |
| `unadjudicated/cw-2026-08-20-014854` | green | 0 | 0 | 0 | 0 | 0 | 0 | 0 (0) | 608 |
| `unadjudicated/cw-2026-08-20-014935` | green | 0 | 0 | 0 | 0 | 0 | 0 | 0 (0) | 742 |
| `unadjudicated/cw-2026-08-22-014113` | green | 0 | 0 | 0 | 0 | 0 | 0 | 0 (0) | 607 |
| `unadjudicated/cw-2026-08-22-014308` | green | 0 | 0 | 0 | 0 | 0 | 0 | 0 (0) | 606 |

## TheAdjudicatedReadingsKeepReadingTests - every case, 13 of 13 green

| Reading | Anchor | State | Found |
|---|---|---|---|
| `cw-2026-08-17-013347` | `VA3VRR` 6 of 6 | required | yes |
| `cw-2026-08-17-134712` | `N4` | retired (Tim 2026-08-30) | no |
| `unadjudicated/cw-2026-08-18-003758` | `MP/4 QNIK` 9 of 12 | required | yes, reads `AA4MP/4 QNIK` whole |
| `unadjudicated/cw-2026-08-24-012403` | `DE KD0UN KD0UN K` 16 of 16 | required | yes |
| `cw-2026-08-18-004507` | `N HANDLING THIS MESSAG` 22 of 57 | required | yes |
| `unadjudicated/cw-2026-08-22-031838` | `, AND` 5 of 35 | required | yes, reads `2, 2, AND 2 WITH A MEAN OF 2.` |
| `unadjudicated/cw-2026-08-22-031905` | `DICTED 10.7` | retired (squelch) | no |
| `unadjudicated/cw-2026-08-22-031948` | `110, AND 110 WITH A MEAN OF 117` 31 of 36 | required | yes |
| `unadjudicated/cw-2026-08-22-032012` | `R OTHER WEBSITES MENTI` 22 of 51 | required | yes |
| `unadjudicated/cw-2026-08-22-032050` | `ULLETIN CAN BE FO` | retired (squelch) | no |
| `unadjudicated/cw-2026-08-22-032113` | `INT` | retired (squelch) | yes |
| `unadjudicated/cw-2026-08-22-032129` | `OPAGATION` | retired (squelch) | yes |
| `TheShortfallIsPrintedRatherThanPapered` | - | fact | green |

## CwFixtureTests.TheCleanRecordingsDecodeExactly - 0 of 2

| Fixture | Expected | Read at HEAD |
|---|---|---|
| `clean-12wpm` | `CQ DE W1AW K` | `""` (nothing) |
| `clean-18wpm` | `CQ DE W1AW K` | `""` (nothing) |

## Against docs/unit239-failing-set.txt (2026-09-03)

That list carries 2 cases of `EachStillProducesWhatItDid` (`unadjudicated/cw-2026-08-23-001520`,
`unadjudicated/cw-2026-08-25-013637`) and both clean synthetics, and no case of
`TheAdjudicatedReadingsKeepReadingTests`. All four are red here. **The other 16 red capture
cases are not on that list** and are red at HEAD: a finding, not chased (section 9).
