# Unit 406 - the tracker switch, and #42 given the radio's word

Every number here is an indication (FACT-004). Nothing here says a change makes CW read.

## 1. The entry round

HEAD at entry `527b1659`. `git diff ef34d601 HEAD -- src tests docs/carry-forward-tests.txt`
prints nothing, so both carry-forward lines at entry are unit 405's exit: ENGINE 178 of 178,
APP 278 of 278 counting neither way (decision 7).

| type | entry |
|---|---|
| `TheCapturesThatDecodeKeepDecodingTests` | 37 of 37 in 94 s, every row identical to unit 405's exit table |
| `TheAdjudicatedReadingsKeepReadingTests` | 13 of 13 in 29 s |
| `CwFixtureTests.TheCleanRecordingsDecodeExactly` | 2 of 2 in 1 s |
| `CwAcquisitionWindowTests` | 10 of 12: #6 0.75, #15 0.54 |
| `CwReceiverFixtureTests` | 23 of 27: #42 70, #43 5 + 37, #44 3 + 21, #45 1 + 3 |
| `CwFixtureTests` | 22 of 23, `fading-18wpm` parked |
| `CwAdjudicationTests` | 11 of 11 |
| `CwEmissionGateTests` | 8 of 8 |
| `CwDisplacementFloorTests` | 6 of 6 |
| `CapturedSignalTests` | 13 of 13 |
| `CwSpeedSilenceTests` | 4 of 4 |
| `WhyTheGateDidNotFireTests` | 2 of 2 |
| `CwTwoStationTests` | 5 of 5 |

**The tracker readers.** Grep for `CwToneTracker` over `tests\Hamlet.RadioEngine.Tests`, plus
`ThePitchCanBeHeldTests`, compiled types only:

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
| `WhatBandwidthTheDecoderListensThroughTests` | 4 of 6 in 154 s: `MostRealRecordingsSitInTheWidestWindow` and `HoldingTheWindowLongInTimeReadsMore(cw-2026-08-18-004507.wav)`, 48 against 50, red at entry; neither is in the 51-name set |
| `TheGateHasItsOwnWindowNowTests` | **did not fit.** 300 s, then alone at 590 s: the test host was killed with no result either time. Its two short methods, run alone, 4 of 4 in 1 s. `EveryWidthLeavesTheEmptyRecordingsSilent` is unmeasured |

`ThePeakAgainstASecondSignalTests` and `TheQuietestBinNoLongerWinsTests` name the tracker but
are removed from compile in the test project, so they are not run.

The eleven transmit files print nothing against `7e209cb4`.
