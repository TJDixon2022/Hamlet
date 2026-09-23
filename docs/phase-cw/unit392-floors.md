# Unit 392 - the three floor tests and the app CW tests at the restored decoder (1.3, 1.4)

Written by work instruction 392 task 3, 2026-09-22, at `9fbb4728` (task 2's commit:
`src\Hamlet.RadioEngine\Cw` is `7e209cb4`'s with the seams of `unit392-seams.md`). One
filtered, foregrounded invocation per type with `--logger "console;verbosity=detailed"`, a
status line before each; output kept in `.run-unit\unit392-floors-{1,2,3}.txt` and
`.run-unit\unit392-appcw-1.txt` (not committed). The HEAD column is unit 391's run of the same
case on the same tables, `docs/phase-cw/unit391-floors-head.md`. **Every number here is a count
against a floor, an indication and not a reading (FACT-004, §0.0).**

## Summary

| Type | Cases | Green | Red | Wall | Unit 391 at HEAD |
|---|---|---|---|---|---|
| `TheCapturesThatDecodeKeepDecodingTests.EachStillProducesWhatItDid` | 37 | **37** | 0 | 97 s, timeout 900 | 19 green, 18 red, 1995 s |
| `TheAdjudicatedReadingsKeepReadingTests` (12 readings and 1 fact) | 13 | **13** | 0 | 33 s, timeout 600 | 13 green, 364 s |
| `CwFixtureTests.TheCleanRecordingsDecodeExactly` | 2 | 0 | **2** | 7 s, timeout 300 | 0 green, 2 red, 10 s |
| **The three** | **52** | **50** | **2** | | 32 green, 20 red |

**Captures and adjudicated: 18 of 50 red at HEAD, 0 of 50 red at the restored decoder.** The
two clean synthetics are red at entry and at exit, as R53 says; they now read placeholders
where HEAD read the empty string. No floor, case or anchor was changed.

## Captures, every case

Result, measured characters, floor, difference, measured elements, floor, difference, unsure
now (unsure when the floor was set), the pitch the test decoded at, and unit 391's characters
and elements at HEAD with its result there.

| Capture | Result | Chars | Floor | Diff | Elements | Floor | Diff | Unsure (then) | Tone Hz | HEAD, unit 391 |
|---|---|---|---|---|---|---|---|---|---|---|
| `cw-2026-08-17-013347` | green | 59 | 59 | 0 | 108 | 108 | 0 | 2 (2) | 625 | 58 / 108, green (count floor retired, anchor) |
| `cw-2026-08-17-013622` | green | 55 | 55 | 0 | 84 | 84 | 0 | 4 (0) | 600 | 53 / 86, red |
| `cw-2026-08-17-134712` | green | 63 | 63 | 0 | 98 | 98 | 0 | 42 (10) | 500 | 54 / 81, red (count floor retired, anchor) |
| `cw-2026-08-18-004507` | green | 50 | 50 | 0 | 118 | 118 | 0 | 1 (1) | 500 | 50 / 119, green (count floor retired, anchor) |
| `unadjudicated/cw-2026-08-18-003016` | green | 57 | 57 | 0 | 149 | 149 | 0 | 3 (3) | 670 | 54 / 146, red |
| `unadjudicated/cw-2026-08-18-003126` | green | 54 | 54 | 0 | 144 | 144 | 0 | 6 (6) | 665 | 53 / 142, red |
| `unadjudicated/cw-2026-08-18-003758` | green | 63 | 63 | 0 | 121 | 121 | 0 | 19 (10) | 500 | 61 / 123, green (count floor retired, anchor) |
| `unadjudicated/cw-2026-08-20-014854` | green | 0 | 0 | 0 | 0 | 0 | 0 | 0 (0) | 600 | 0 / 0, green |
| `unadjudicated/cw-2026-08-20-014935` | green | 0 | 0 | 0 | 0 | 0 | 0 | 0 (0) | 825 | 0 / 0, green |
| `unadjudicated/cw-2026-08-22-014113` | green | 0 | 0 | 0 | 0 | 0 | 0 | 0 (0) | 600 | 0 / 0, green |
| `unadjudicated/cw-2026-08-22-014308` | green | 0 | 0 | 0 | 0 | 0 | 0 | 0 (0) | 575 | 0 / 0, green |
| `unadjudicated/cw-2026-08-22-031838` | green | 57 | 57 | 0 | 126 | 126 | 0 | 15 (3) | 525 | 33 / 116, red (count floor retired, anchor) |
| `unadjudicated/cw-2026-08-22-031905` | green | 42 | 42 | 0 | 118 | 118 | 0 | 6 (6) | 300 | 37 / 120, green (count floor retired, anchor) |
| `unadjudicated/cw-2026-08-22-031948` | green | 34 | 34 | 0 | 114 | 114 | 0 | 3 (3) | 500 | 31 / 119, green (count floor retired, anchor) |
| `unadjudicated/cw-2026-08-22-032012` | green | 44 | 44 | 0 | 120 | 120 | 0 | 1 (1) | 500 | 43 / 119, red (count floor retired, anchor) |
| `unadjudicated/cw-2026-08-22-032050` | green | 53 | 53 | 0 | 123 | 123 | 0 | 9 (9) | 325 | 49 / 115, red (count floor retired, anchor) |
| `unadjudicated/cw-2026-08-22-032113` | green | 55 | 55 | 0 | 118 | 118 | 0 | 8 (8) | 650 | 48 / 126, green (count floor retired, anchor) |
| `unadjudicated/cw-2026-08-22-032129` | green | 66 | 66 | 0 | 119 | 119 | 0 | 1 (1) | 650 | 43 / 123, green (count floor retired, anchor) |
| `unadjudicated/cw-2026-08-23-001520` | green | 5 | 5 | 0 | 45 | 45 | 0 | 4 (1) | 600 | 7 / 39, red |
| `unadjudicated/cw-2026-08-23-001831` | green | 55 | 55 | 0 | 124 | 124 | 0 | 11 (10) | 525 | 53 / 124, red |
| `unadjudicated/cw-2026-08-23-001952` | green | 75 | 75 | 0 | 142 | 142 | 0 | 19 (13) | 525 | 60 / 120, red |
| `unadjudicated/cw-2026-08-23-002016` | green | 75 | 75 | 0 | 136 | 136 | 0 | 31 (17) | 525 | 75 / 136, green |
| `unadjudicated/cw-2026-08-24-012403` | green | 22 | 22 | 0 | 65 | 65 | 0 | 1 (0) | 440 | 24 / 71, green (count floor retired, anchor) |
| `unadjudicated/cw-2026-08-25-011552` | green | 30 | 30 | 0 | 89 | 89 | 0 | 8 (8) | 500 | 32 / 89, green |
| `unadjudicated/cw-2026-08-25-012748` | green | 4 | 2 | 2 | 16 | 4 | 12 | 2 (0) | 395 | 2 / 4, green |
| `unadjudicated/cw-2026-08-25-012823` | green | 41 | 41 | 0 | 62 | 62 | 0 | 15 (15) | 450 | 35 / 57, red |
| `unadjudicated/cw-2026-08-25-012922` | green | 50 | 50 | 0 | 112 | 112 | 0 | 5 (5) | 475 | 44 / 111, red |
| `unadjudicated/cw-2026-08-25-013010` | green | 54 | 54 | 0 | 131 | 131 | 0 | 6 (6) | 475 | 56 / 132, green |
| `unadjudicated/cw-2026-08-25-013150` | green | 58 | 58 | 0 | 139 | 139 | 0 | 7 (7) | 495 | 61 / 132, red |
| `unadjudicated/cw-2026-08-25-013303` | green | 54 | 54 | 0 | 146 | 146 | 0 | 10 (10) | 500 | 52 / 141, red |
| `unadjudicated/cw-2026-08-25-013402` | green | 61 | 61 | 0 | 161 | 161 | 0 | 5 (5) | 525 | 59 / 154, red |
| `unadjudicated/cw-2026-08-25-013520` | green | 60 | 60 | 0 | 153 | 153 | 0 | 5 (5) | 540 | 62 / 155, green |
| `unadjudicated/cw-2026-08-25-013637` | green | 63 | 63 | 0 | 164 | 164 | 0 | 3 (3) | 550 | 62 / 158, red |
| `unadjudicated/cw-2026-08-25-021410` | green | 47 | 47 | 0 | 99 | 99 | 0 | 11 (11) | 550 | 40 / 97, red |
| `unadjudicated/cw-2026-08-25-021629` | green | 47 | 47 | 0 | 96 | 96 | 0 | 20 (20) | 500 | 26 / 71, red |
| `unadjudicated/cw-2026-08-25-021825` | green | 41 | 41 | 0 | 74 | 74 | 0 | 16 (16) | 400 | 61 / 94, green |
| `unadjudicated/cw-2026-08-26-125941` | green | 0 | 0 | 0 | 0 | 0 | 0 | 0 (0) | 400 | 0 / 0, green |

`unadjudicated/cw-2026-08-25-012748` is the one case above its floor: 4 characters and 16
elements against 2 and 4, the floor Tim lowered on 2026-08-25 at `2068f868`. Every other case
is exactly on its floor. The tone column is what the test decoded at, and it differs from
unit 391's HEAD column on several recordings (`031905` at 300 Hz here, 500 at HEAD); nothing in
this unit judges which is right.

## Adjudicated, every case

| Reading | Anchor | State | Found | Adjudicated characters read | HEAD, unit 391 |
|---|---|---|---|---|---|
| `cw-2026-08-17-013347` | `VA3VRR` | required | yes | 6 of 6 | green, found |
| `cw-2026-08-17-134712` | `N4` | retired, Tim 2026-08-30 | yes, would come back | 2 of 3 | green, not found |
| `unadjudicated/cw-2026-08-18-003758` | `MP/4 QNIK` | required | yes | 9 of 12 | green, found |
| `unadjudicated/cw-2026-08-24-012403` | `DE KD0UN KD0UN K` | required | yes | 16 of 16 | green, found |
| `cw-2026-08-18-004507` | `N HANDLING THIS MESSAG` | required | yes | 22 of 57 | green, found |
| `unadjudicated/cw-2026-08-22-031838` | `, AND` | required | yes | 5 of 35 | green, found |
| `unadjudicated/cw-2026-08-22-031905` | `DICTED 10.7` | retired, squelch | yes, would come back | 11 of 39 | green, not found |
| `unadjudicated/cw-2026-08-22-031948` | `110, AND 110 WITH A MEAN OF 117` | required | yes | 31 of 36 | green, found |
| `unadjudicated/cw-2026-08-22-032012` | `R OTHER WEBSITES MENTI` | required | yes | 22 of 51 | green, found |
| `unadjudicated/cw-2026-08-22-032050` | `ULLETIN CAN BE FO` | retired, squelch | yes, would come back | 17 of 59 | green, not found |
| `unadjudicated/cw-2026-08-22-032113` | `INT` | retired, squelch | yes, would come back | 3 of 28 | green, found |
| `unadjudicated/cw-2026-08-22-032129` | `OPAGATION` | retired, squelch | yes, would come back | 9 of 42 | green, found |
| `TheShortfallIsPrintedRatherThanPapered` | - | fact | - | 153 of 384 across all, 40 % | green |

The retired anchors are reported and not required; the test prints *it would come back if the
anchor appeared: True* for all five. No retired anchor was un-retired and no required one
added.

## The clean synthetics (R53: step 3's, reported not required)

| Fixture | Expected | Read at the restored decoder | Read at HEAD, unit 391 |
|---|---|---|---|
| `clean-12wpm` | `CQ DE W1AW K` | `■ ■ ■ ■ ■  ■ ■ ■ ■■` | `""` (nothing) |
| `clean-18wpm` | `CQ DE W1AW K` | `■ ■ ■  ■■■` | `""` (nothing) |

Red on the assertion both times, not a lost run. Not re-run, not chased (section 9).

## The app's CW tests (1.4)

One invocation, `timeout 480`, 13 s, **13 of 13 green**:

| Test | Result |
|---|---|
| `TheSheetSaysWhatEachElementWasSentAtTests.AnUnmeasuredPitchSaysSoRatherThanPrintingNumbers` | green |
| `TheSheetSaysWhatEachElementWasSentAtTests.SilenceProducesNoSpread` | green |
| `TheSheetSaysWhatEachElementWasSentAtTests.TheLineNeverSaysTwoOperators(toneHz: 500)` | green |
| `TheSheetSaysWhatEachElementWasSentAtTests.TheLineNeverSaysTwoOperators(toneHz: 600)` | green |
| `ReturningToCwShowsCwTests.EachTabShowsItsOwnWorkspace` | green |
| `ReturningToCwShowsCwTests.ExactlyOneTabIsCheckedAndItIsTheOneShowing` | green |
| `ReturningToCwShowsCwTests.TheRoundTripComesBackToSendAndReceive` | green |
| `BindingHealthTests.TheMainWindowBindsWithoutOneComplaint` | green |
| `VoiceTests.TheStaleCannotWorkSentenceIsNowhereInTheSource` | green |
| `VoiceTests.TheSweepIsActuallyReadingTheCopy` | green |
| `VoiceTests.NoOperatorFacingStringUsesABritishSpelling` | green |
| `VoiceTests.NoPassageOfCopyCarriesTwoEmDashes` | green |
| `VoiceTests.CommentsAreNotMistakenForCopy` | green |

The element-pitch line the first file tests is one of task 2's app hunks. On keyed audio it
now prints *not measured (the decoder in this build does not say where each element began and
ended, so no element's own pitch was measured)*; on silence, *nothing was read, so no element
was measured, which is too few to say anything about how they spread*. Both branches ran in
this invocation.
