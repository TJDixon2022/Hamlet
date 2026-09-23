# Unit 407 - the station still keying, and #45's tail on the air's terms

Every number here is an indication (FACT-004). Nothing here says a change makes CW read.

## 1. The entry round

HEAD at entry `9c8198b1`. `git diff --stat 24d14e9c HEAD -- src tests docs/carry-forward-tests.txt`
prints nothing, so both carry-forward lines at entry are unit 406's exit: ENGINE 178 of 178,
APP 278 of 278 (decision 8). Built at 1.13.94 with warnings as errors, RC 0.

Every list below is identical, name by name, to unit 406's exit list of the same type.

| type | entry |
|---|---|
| `TheCapturesThatDecodeKeepDecodingTests` | 37 of 37 in 94 s, every row identical to unit 406's exit table |
| `TheAdjudicatedReadingsKeepReadingTests` | 13 of 13 in 29 s |
| `CwFixtureTests.TheCleanRecordingsDecodeExactly` | 2 of 2 in 2 s |
| `CwAcquisitionWindowTests` | 11 of 12: #6 0.82 green, #15 0.54 |
| `CwReceiverFixtureTests` | 24 of 27: #42 green, #43 5 + 37, #44 3 + 21, #45 1 + 3 |
| `CwFixtureTests` | 22 of 23, `fading-18wpm` parked |
| `CwAdjudicationTests` | 11 of 11 |
| `CwEmissionGateTests` | 8 of 8 |
| `CwDisplacementFloorTests` | 6 of 6 |
| `HamletDoesNotDecodeYourOwnSendingTests` | 6 of 6 |
| `CapturedSignalTests` | 13 of 13 |
| `CwSpeedSilenceTests` | 4 of 4 |
| `WhyTheGateDidNotFireTests` | 2 of 2 |
| `CwTwoStationTests` | 5 of 5 |

**The tracker readers**, unit 406's list:

| type | entry |
|---|---|
| `ThePitchCanBeHeldTests` | 5 of 5 |
| `CwSurveyThresholdPinTests` | 3 of 3 |
| `CwToneSurveyTests` | 5 of 5 |
| `CwTrackerSwitchTests` | 2 of 2 |
| `TheKeyingWitnessSaysNothingImpossibleTests` | 62 of 62 |
| `TheOperatorIsToldAboutASecondStationTests` | 5 of 5 |
| `TheSurveyAlreadyUsesAShortWindowTests` | 2 of 2 |
| `WhatDecodeScoringCostsTests` | 2 of 2 |
| `WhatBandwidthTheDecoderListensThroughTests` | 4 of 6 in 153 s: `MostRealRecordingsSitInTheWidestWindow` and `HoldingTheWindowLongInTimeReadsMore(cw-2026-08-18-004507.wav)` red at entry, as unit 406's exit; neither is in the 51-name set |
| `TheGateHasItsOwnWindowNowTests` | its two short methods, 4 of 4 in 1 s; `EveryWidthLeavesTheEmptyRecordingsSilent` unmeasured (section 3 item 5) |

The eleven transmit files print nothing against `7e209cb4`. `src\Hamlet.App` prints nothing
against `9c8198b1`.

**Mismatches against section 5 of the instruction**, reported and not repaired:

- H2's hold in `CwToneTracker.cs` is lines 958 to 978, its comment 958 to 965 and its `if` 966
  to 978 with `Switch(_heldSwitchHz)` at 974. The instruction says 957 to 966. Line 212, 1077
  to 1080 and 1091 are as stated.
- `PHASE_STATUS.md` as the launcher left it read `CURRENT_STEP: 0`, and at HEAD it names steps
  0 and 2 `not started` while the instruction says steps 1, 2 and 4 are done. Task 0 set
  `CURRENT_STEP: 3`; the step lines are not touched.
- The reload disagreement the instruction names does not hold as written: `PROJECT_STATUS.md`
  RULES_AT says HM-DEC-165 of 2026-09-19, and `CLAUDE.md` section 1 holds HM-DEC-165 dated
  2026-09-19 at line 361. A grep for `CPS-DEC-0167` in `CLAUDE.md` finds nothing.
