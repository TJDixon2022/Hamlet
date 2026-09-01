# Test baseline — the failing set, by name

**Measured 2026-08-31, at `HEAD` `a3a5c90` on `main`, on Tim's Windows 11 machine, by
work unit 204.** Written for someone who has never seen that unit.

This is the first record in this repository that names the failing tests rather than
counting them. Before it, the only figure anywhere in the tree was "18 of 38 red" inside
the text of HM-DEC-151 itself, with no list behind it and nothing saying what the 38
counted.

**Read the two caveats below before you use this file for anything.** They are not
hedging; they materially change how much weight the numbers will bear.

---

## Caveat 1 — this baseline covers two of the three test projects

`Hamlet.sln` names three test projects: `tests/Ft8Sharp.Tests`,
`tests/Hamlet.RadioEngine.Tests` and `tests/Hamlet.App.Tests`.

**`tests/Hamlet.App.Tests` is missing from this baseline.** It is not missing because it
would not build — it builds clean, in about six seconds, with no warnings and no errors,
and the `MSB3027` file lock that earlier units reported is gone. It is missing because
`dotnet test` on it ran for over one hundred minutes without emitting a single line of
output and had still not returned when the measurement was cut off. Whether it is a
genuinely slow headless-UI suite or is wedged was not determined; process enumeration was
refused by the environment the measurement ran in.

**So this file describes the suite less `Hamlet.App.Tests`.** A partial baseline that says
it is partial is useful. Do not treat it as complete.

## Caveat 2 — this baseline was taken under CPU contention, and may over-count

`Hamlet.RadioEngine.Tests` was run **concurrently with** the stalled `Hamlet.App.Tests`
run, because the alternative was measuring nothing at all before time ran out. The two
projects write to disjoint output directories, so nothing was corrupted — but they shared
a CPU.

**A number of the failures below are timeout-shaped and may be starvation rather than
defects.** All seventeen `RigReadTests` failures fail inside the same `ConnectAsync()`
helper, with individual cases taking one to four seconds against a fake link.
`AnOrdinaryDisconnectIsImmediate`, `ARigWhoseReadLoopIsStuckStillDisconnects` and
`ItKeepsUpWithLiveAudio` assert on timing by their own names. Against that, the CW fixture
failures decode recorded audio and assert on decoded characters, and those are unlikely to
be contention.

**The set below was never re-run in isolation**, so real reds and starved ones are not
separated here. **The first thing the next unit to touch this file should do is re-run
these names alone on a quiet machine and rewrite this list from that run.** Until then,
this is a first draft with a known upward bias.

---

## Counts

| Project | Total | Passed | Failed | Skipped |
|---|---|---|---|---|
| `Ft8Sharp.Tests` | 38 | 37 | 0 | 1 |
| `Hamlet.RadioEngine.Tests` | not read | not read | **84** | not read |
| `Hamlet.App.Tests` | not run | not run | not run | not run |

**There is deliberately no summed total.** The `Hamlet.RadioEngine.Tests` run did not
print its summary line before the cut-off, so its total, passed and skipped figures were
never read. The 84 is a count of failure names observed one at a time on the run's own
output stream. Nothing here is extrapolated.

**No failing test is inside `Ft8Sharp`.** `Ft8Sharp.Tests` ran to completion as its own
invocation and reported `Failed: 0`. Every name below begins
`Hamlet.RadioEngine.Tests.`.

---

## The failing set — all 84, grouped by class

### `Hamlet.RadioEngine.Tests.Cw.AutoCallSafetyTests` (8)

- `NoTestInThisSuiteCanReachARealTransmitter`
- `AnUnansweredDeadManReadStopsTheCycle`
- `AStaleReadingStopsTheCycle`
- `TheDialMovedStopsTheCycle`
- `ASendTheRadioDidNotTakeStopsTheCycle`
- `ATransmitterStillOnAfterTheMessageStopsTheCycle`
- `TheLinkFailingMidCycleStopsTheCycleAndKeysTheStop`
- `EveryTransmissionIsLoggedWithWhereAndWhatAndWhen`

### `Hamlet.RadioEngine.Tests.Cw.CwDisplacementFloorTests` (6)

- `TheTrackerDoesNotLeaveAStationForItsOwnImage`
- `NothingIsRefusedBeforeAnythingIsBeingRead`
- `AStationElsewhereIsStillFound(toneHz: 400)`
- `AStationElsewhereIsStillFound(toneHz: 500)`
- `AStationElsewhereIsStillFound(toneHz: 750)`
- `AStationElsewhereIsStillFound(toneHz: 875)`

### `Hamlet.RadioEngine.Tests.Cw.CwFixtureTests` (9)

- `TheProsignRecordingDecodesItsProsigns`
- `NothingTheDecoderWasSureOfIsWrong(name: "fading-18wpm")`
- `NothingTheDecoderWasSureOfIsWrong(name: "noisy-18wpm")`
- `NothingTheDecoderWasSureOfIsWrong(name: "interference-18wpm")`
- `EveryRecordingGivesBackTheShareItShould(name: "prosigns-18wpm")`
- `EveryRecordingGivesBackTheShareItShould(name: "clean-12wpm")`
- `EveryRecordingGivesBackTheShareItShould(name: "clean-18wpm")`
- `TheCleanRecordingsDecodeExactly(name: "clean-12wpm")`
- `TheCleanRecordingsDecodeExactly(name: "clean-18wpm")`

### `Hamlet.RadioEngine.Tests.Cw.ARecordingWithKeyingInItIsReadTests` (3)

- `WhereTheTrackerStartsDoesNotDecideThis(startHz: 500)`
- `WhereTheTrackerStartsDoesNotDecideThis(startHz: 550)`
- `WhereTheTrackerStartsDoesNotDecideThis(startHz: 600)`

### `Hamlet.RadioEngine.Tests.Cw.TheCapturesThatDecodeKeepDecodingTests` (6)

- `EachStillProducesWhatItDid(name: "cw-2026-08-17-013622", characters: 55, elements: 84, unsure: 0)`
- `EachStillProducesWhatItDid(name: "unadjudicated/cw-2026-08-23-001520", characters: 5, elements: 45, unsure: 1)`
- `EachStillProducesWhatItDid(name: "unadjudicated/cw-2026-08-25-012922", characters: 50, elements: 112, unsure: 5)`
- `EachStillProducesWhatItDid(name: "unadjudicated/cw-2026-08-25-013150", characters: 58, elements: 139, unsure: 7)`
- `EachStillProducesWhatItDid(name: "unadjudicated/cw-2026-08-25-013402", characters: 61, elements: 161, unsure: 5)`
- `EachStillProducesWhatItDid(name: "unadjudicated/cw-2026-08-25-013637", characters: 63, elements: 164, unsure: 3)`

### `Hamlet.RadioEngine.Tests.Cw.Fixtures.CwReceiverFixtureTests` (4)

- `NothingIsEmittedDuringTheOperatorsOwnTransmission`
- `TheEasyTierIsReadWhole(name: "coverage-easy")`
- `TheEasyTierIsReadWhole(name: "exchange-easy")`
- `TheEasyTierIsReadWhole(name: "tightfist-easy")`

### `Hamlet.RadioEngine.Tests.Cw.Fixtures.CwAdjudicationTests` (1)

- `ASpeedChangeInRealisticAudio`

### `Hamlet.RadioEngine.Tests.Cw.TheUnitIsMeasuredNotSearchedTests` (1)

- `TheFiveToEightDecibelPlateauHolds`

### `Hamlet.RadioEngine.Tests.Cw.ThePitchCanBeHeldTests` (1)

- `UnlockingLetsTheTrackerSteerAgain`

### `Hamlet.RadioEngine.Tests.Cw.CwLowDutyTests` (1)

- `TheToneIsFoundWhereItActuallyIs`

### `Hamlet.RadioEngine.Tests.Cw.CwEmissionGateTests` (1)

- `NoSpeedIsNamedWithoutCharactersToNameItFrom`

### `Hamlet.RadioEngine.Tests.Cw.TheProbabilisticDecoderTests` (1)

- `ItKeepsUpWithLiveAudio`

### `Hamlet.RadioEngine.Tests.Scan.BandScannerSafetyTests` (1)

- `AScanNeverKeysTheTransmitter`

### `Hamlet.RadioEngine.Tests.Rig.RigReadTests` (17)

All seventeen fail inside the same `ConnectAsync()` helper at `RigReadTests.cs:37`.

- `AReadThatTimesOutMarksTheValueUnknownWithoutThrowing`
- `AModeChangeOnTheRadioArrivesWithoutBeingAskedFor`
- `TheFilterWidthComesBackInHertz`
- `AnUndocumentedFieldSendsNothingAndSaysSo`
- `AReplyToADifferentSubCommandDoesNotAnswerThisRequest`
- `EachSettingParsesToTheManualsOwnWords(field: SquelchStatus, command: 21, subCommand: 5, value: 1, expected: "open")`
- `EachSettingParsesToTheManualsOwnWords(field: Preamp, command: 22, subCommand: 2, value: 2, expected: "preamp 2")`
- `TheSMeterParsesAgainstTheManualsAnchors(high: 2, low: 65, expected: "S9+60")`
- `TheSMeterParsesAgainstTheManualsAnchors(high: 0, low: 0, expected: "S0")`
- `TheSMeterParsesAgainstTheManualsAnchors(high: 0, low: 103, expected: "S5")`
- `ReadingTheModeAlsoAnswersTheFilter(modeByte: 0, filterByte: 1, mode: "LSB", filter: "FIL1")`
- `ReadingTheModeAlsoAnswersTheFilter(modeByte: 5, filterByte: 1, mode: "FM", filter: "FIL1")`
- `ReadingTheModeAlsoAnswersTheFilter(modeByte: 3, filterByte: 3, mode: "CW", filter: "FIL3")`
- `TheLevelScalesLandOnTheManualsFigures(field: CwPitch, subCommand: 9, high: 0, low: 0, expected: 300)`
- `TheLevelScalesLandOnTheManualsFigures(field: CwPitch, subCommand: 9, high: 1, low: 40, expected: 600)`
- `TheLevelScalesLandOnTheManualsFigures(field: CwPitch, subCommand: 9, high: 2, low: 85, expected: 900)`
- `TheLevelScalesLandOnTheManualsFigures(field: KeyerSpeed, subCommand: 12, high: 0, low: 0, expected: 6)`

### `Hamlet.RadioEngine.Tests.Rig.RigDisconnectTests` (3)

- `ARigWhoseReadLoopIsStuckStillDisconnects`
- `TheStateMonitorDoesNotHoldUpADisconnect`
- `AnOrdinaryDisconnectIsImmediate`

### `Hamlet.RadioEngine.Tests.Rig.ScopeStreamTests` (3)

- `AnOutOfRangeSweepDrawsNothing`
- `ListeningToTheScopeIssuesNoCommands`
- `ASweepIsAssembledFromItsPartsAndPublishedOnce`

### `Hamlet.RadioEngine.Tests.Rig.ScopeOutputWriteTests` (2)

- `TheCommandOnTheWireIs2711`
- `AnAcknowledgementWithTheSettingStillOffIsItsOwnFault`

### `Hamlet.RadioEngine.Tests.Rig.Ic7300RigTests` (3)

- `EchoedOwnFrame_IsIgnored`
- `SetFrequency_WiresDocumentedBytes_CompletesOnOk`
- `Connect_ProbeAnswered_Succeeds`

### `Hamlet.RadioEngine.Tests.Rig.TheRoundTripLandsInTheSamePlaceTests` (3)

- `AutoNotchLeftOnIsCorrectedOnEnteringMorse`
- `TenRoundTripsDoNotDrift`
- `ComingBackToMorseRestoresWhatMorseNeeds`

### `Hamlet.RadioEngine.Tests.Rig.TheTuneInSetsOnlyWhatIsInTheWayTests` (2)

- `WhatIsInTheWayIsChangedAndWhatIsAlreadyRightIsNot`
- `ArrivingAgainWithNothingChangedSendsNothing`

### `Hamlet.RadioEngine.Tests.Rig.RigBroadcastProvenanceTests` (3)

- `AskingForACompanionFieldLeavesWhatAnsweredItAlone`
- `NoFieldTheRadioActuallyHasIsEverCalledNotOnThisRadio`
- `ABroadcastFrequencySurvivesTheSweepWithItsProvenance`

### `Hamlet.RadioEngine.Tests.Rig.CwToDataAndBackTests` (1)

- `ArrivingOnTheDigitalBlockLeavesTheRadioAbleToHearIt`

### `Hamlet.RadioEngine.Tests.Rig.ModeFollowTests` (1)

- `AConfirmedModeWriteFoldsTheDataVariantTooAsync`

### `Hamlet.RadioEngine.Tests.Rig.TheDataWriteCarriesItsFlagTests` (1)

- `ARefusedWriteLeavesTheModeUnknown`

### `Hamlet.RadioEngine.Tests.Rig.AReadIsAnsweredOnlyByItsAnswerTests` (1)

- `AnAcknowledgementDoesNotCompleteAFrequencyRead`

### `Hamlet.RadioEngine.Tests.Rig.OneReadReturnsTheAnswerItAskedForTests` (1)

- `ATransceiveBroadcastDoesNotAnswerARead`

---

## The skipped set

Kept deliberately separate from the failing set.

### `Ft8Sharp.Tests` (1)

- `Ft8Sharp.Tests.Ft8TableGenerationTests.RewriteTheCheckedInTablesFile`

`Hamlet.RadioEngine.Tests`' skip count was not read, because that run did not reach its
summary line.

**A skip is not a failure here.** Reference material — off-air recordings and the pinned
upstream clone — is never committed to this repository, so tests that need it report
skipped when it is absent rather than red. That is what keeps a fresh clone green for
somebody who has just downloaded the project, and it is a deliberate design, not a gap.

---

## How to use this file

A later unit re-runs the suite and compares **name by name** against the lists above. Any
name that has appeared, and any name that has disappeared, is the finding — and it is
reported as a finding whichever direction it went, because a red that has quietly gone
green is as much a change to the inherited set as a green that has gone red.

**A matching count is not a match.** This is the whole reason HM-DEC-151 asks for names
rather than a number: one inherited red going green while a green goes red leaves the
total exactly where it was, and a count-only comparison sees nothing. Compare the sets,
not their sizes.

Given caveat 2, the correct first use of this file is to **replace it** — re-run these
names on a quiet machine, in isolation, and rewrite the lists from that run before anyone
starts diffing against them.

## What this file is not

**It is not a list of bugs to fix.** Nobody is assigned to any name in it by its presence
here.

**It is not a ratchet the FT8 phase owns.** The CW work and the rig work predate this
phase, and this phase neither created these failures nor took responsibility for them.
Its obligation is narrower: do not add to the set, and do not silently change it.

**And it is not a licence to leave the CW reds alone forever.** HM-DEC-151's own words —
it says only that fixing them is not this phase's work. Somebody's night, eventually.
