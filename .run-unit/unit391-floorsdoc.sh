#!/bin/sh
# Unit 391: write docs/phase-cw/unit391-floors-head.md.
cd /c/Source/HamLet || exit 1
doc=docs/phase-cw/unit391-floors-head.md
{
cat <<'EOF'
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
EOF
sh .run-unit/unit391-table.sh
cat <<'EOF'

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
EOF
} > $doc
wc -l $doc
