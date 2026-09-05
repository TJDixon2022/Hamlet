# Test baseline — the failing set, by name

> ## SUPERSEDED IN PART, 2026-09-05, by work instruction 250
>
> **Read this box before you use any number below it.** Everything in this file
> from *Counts* down was measured on 2026-09-01 and three of its central claims
> are now false. The 2026-09-01 text is kept intact — a measurement is not
> deleted because a later one disagreed with it — but it is no longer current.
>
> **What has changed, measured on 2026-09-05 from TRX files rather than from
> console text:**
>
> | | 2026-09-01 said | 2026-09-05 measured |
> |---|---|---|
> | `Ft8Sharp.Tests` | 38 discovered, 38 run, 42 ms | **610 discovered, 610 run, 856 s wall clock**, 609 passed, 1 skipped |
> | `Ft8Sharp.Deep.Tests` | did not exist | **69 discovered, 69 run, 10.4 s**, 69 passed |
> | `Hamlet.RadioEngine.Tests` | 2157 discovered, **never completed**, no per-test time ever recorded | **2,281 discovered**; **1,541 run with per-test durations**, in four namespace batches, 42 red |
> | `Hamlet.App.Tests` | 523 discovered, not run | **not re-measured on 2026-09-05**; the 523 is carried, not confirmed |
> | Total discovered | 2718 | **2,960 across the three projects re-measured**, plus whatever `Hamlet.App.Tests` now holds |
>
> **The 38 for `Ft8Sharp.Tests` was never wrong — it has grown.** Units 216
> through 249 added the ladder, the scoreboard, the placement traces and the
> identity comparison to that project. It is now the second-slowest thing in
> this repository.
>
> **The engine project has been timed for the first time.** It still has not been
> run whole in one invocation, and this file's caveat 1 stands on that point.
> What has changed is that a partial run in namespace batches leaves per-test
> durations behind, so *which* engine tests are expensive is no longer unknown:
> `Cw.Fixtures.TheIntegratorBandwidthTable.Write` alone is **362 s**, and the
> twelve slowest outside `Cw` and `Rig` are 83 per cent of that batch's measured
> test time. The 740 tests not reached are all inside
> `Hamlet.RadioEngine.Tests.Cw`.
>
> **Caveat 2 is now a solved problem and the solution is not the one it
> proposed.** It recommended the unfiltered console logger. That is wrong here:
> **the console logs in this tree are UTF-16**, so filtering them as UTF-8 finds
> nothing and reports zero — which is the mechanism by which a suite came to have
> no total in four consecutive reports. **The correct instrument is a TRX logger
> and `tools\arbiter\trx-rank.py`**, which carries a duration per test and is the
> only place the per-test cost is written down.
>
> **What replaces the advice at the bottom of this file.** *Run everything* has
> stopped being affordable and is **not what a unit does**. Tim ruled on
> 2026-09-05 that **a unit runs no test suite at all** — only the test it
> constructs in its own work instruction, filtered by exact name, foregrounded,
> with a stated timeout — and that **the suites are his, by hand, uncontended,
> once, at the end of the phase**. **`docs/gate-set.md` is the short list he
> runs**, with the breakage each entry would have caught and the unit number
> where it happened, and `tools\arbiter\gate-set.bat` is the command. This file
> remains the name-by-name record of the inherited failing set; **it is not a
> list of what to run**.
>
> **One failing name is new here.**
> `Hamlet.RadioEngine.Tests.Scan.ScannerEndToEndTests.ADwellReachesTheDecoderAndTheVerdictCarriesItsConfidence`
> was the single red outside `Cw` and `Rig` on 2026-09-05, in a batch of 988 that
> was otherwise wholly green. It is not in unit 204's list below and not in
> `docs/unit239-failing-set.txt`. **Nobody knows when it went red**, because no
> unit had ever run that namespace to completion before, so there is no earlier
> reading to diff it against. It is recorded as newly *observed*, not newly
> caused.

---

**Rewritten 2026-09-01 by work unit 205, at `HEAD` `fb4d9af` on `main`, on Tim's
Windows 11 Pro machine (10.0.26200).** Written for somebody who has never seen unit 204
or unit 205 and needs to know what is red in this repository and what that means.

**This file supersedes unit 204's version of it.** Unit 204's measurement is not deleted —
it is kept below, clearly labelled as a prior reading taken under CPU contention, because a
measurement is not thrown away because a later one disagreed with it. But it is no longer
the top of the file, and it should not be diffed against without reading the caveat that
sits on it.

**Read the two caveats before you use this file for anything.** They are not hedging; they
change how much weight each number will bear.

---

## Caveat 1 — the discovered census is complete, the run is not

For the first time in this phase, **every test project has been enumerated**. All three
answered `--list-tests` in seconds, including `Hamlet.App.Tests`, which unit 204 could not
get a single byte out of.

**What has still never been produced is a completed whole-project run of either Hamlet test
project.** `Hamlet.RadioEngine.Tests` was started alone at 08:15:26 and had not returned at
09:16:10, when unit 205's 60-minute bound stopped the wait. `Hamlet.App.Tests` was dropped
as the named drop candidate rather than started, because the RadioEngine run was still
alive and this loop's standing rule is that two test projects never run at once.

**So the counts table below has a discovered column that is complete and a run column that
is mostly empty, and the difference between those two columns is the honest state of this
repository's knowledge of itself.**

## Caveat 2 — unit 205 blinded its own instrument, and says so

Unit 205 ran `dotnet test tests/Hamlet.RadioEngine.Tests --logger "console;verbosity=detailed"`
and piped it through `grep` to select the failure lines. **`grep` block-buffers when its
output is not a terminal.** The consequence is that when the run was still going at the
60-minute bound, the captured stream held one line — the run's own start stamp — and not
one test result, passing or failing.

That is a defect in unit 205's method, not a property of the suite. **The instruction it
was working from said in terms that only the streamed console text survives an early stop,
and putting a buffering filter in the pipe threw exactly that away.** It is recorded here
so the next unit does not repeat it.

**The correct instrument for a run that may not finish is the unfiltered console logger,
with the filtering done after the run ends or on a stream that is not block-buffered.**

---

## Counts

Discovered by `dotnet test <project> --list-tests`, one project at a time, 2026-09-01.

| Project | Discovered | Total run | Passed | Failed | Skipped | Wall clock | Completed? |
|---|---|---|---|---|---|---|---|
| `Ft8Sharp.Tests` | 38 | 38 | 37 | 0 | 1 | 42 ms | yes |
| `Hamlet.RadioEngine.Tests` | 2157 | not read | not read | not read | not read | > 60 min, cut off | **no** |
| `Hamlet.App.Tests` | 523 | not run | not run | not run | not run | — | **not started** |
| **Total discovered** | **2718** | | | | | | |

**There is no summed run total, because two of the three projects produced no summary
line.** Nothing in this table is extrapolated and nothing is carried forward from a prior
unit's report as though it had been measured on 2026-09-01.

**On the census figure.** `PHASE_PLAN.md` contains two mutually incompatible statements
about the size of this suite: a table headed *The baseline, measured* giving 1049 tests
across the three projects with 0 failed, and a sentence two paragraphs earlier saying the
suite is 2682 tests. **Tonight's discovery says 2718.** The larger figure is the one the
tree supports. `PHASE_PLAN.md` is the owner's file and was not edited; the disagreement is
reported and nothing more.

**No failing test is inside `Ft8Sharp`.** `Ft8Sharp.Tests` ran to completion as its own
invocation and reported `Failed: 0` — 38 discovered, 38 run, 37 passed, 1 skipped.

---

## The tests that were run to completion on 2026-09-01

### `Ft8Sharp.Tests` — 38 discovered, 37 passed, 0 failed, 1 skipped

Green in full. The classes that passed: `Ft8SharpBoundaryTests`, `Ft8TableGeometryTests`,
`Ft8TableGenerationTests`, `CSourceParserTests`, `ReferenceCloneProbeTests`,
`Ldpc.Ft8LdpcParityTests`, `Ldpc.Ft8LdpcLayoutTests`, `Ldpc.Ft8LdpcRefusalTests`,
`Ldpc.Ft8LdpcSecondOpinionTests`, `Ldpc.UpstreamEncoderProvenanceTests`.

### The three channels — 68 tests, 0 failed

These are the only tests in the Hamlet projects that read anything the FT8 phase changed.
They are named and run individually in the attribution section below. All 68 passed.

---

## Attribution — what this phase changed, and every test that reads it

**This is the part of the file that answers "has the FT8 phase broken Hamlet", and it does
not depend on the suite completing.**

### The phase boundary

`2828ab6` is the parent of `6f58a76`, the first commit of the FT8 phase; `git rev-parse
6f58a76^` confirms it. Everything the phase has done is `2828ab6..HEAD`, which at
`f743fc2` was **24 commits and 37 changed paths**.

### The 37 paths

Under `src/Ft8Sharp/` (8): `Directory.Build.props`, `Ft8Sharp.csproj`,
`Ft8SharpAssembly.cs`, `LICENSE`, `Ldpc/LdpcEncoder.cs`, `NOTICE`,
`Tables/Ft8Tables.g.cs`, `porting-notes.md`.

Under `tests/Ft8Sharp.Tests/` (19): `CSourceParserTests.cs`, `Ft8Sharp.Tests.csproj`,
`Ft8SharpBoundaryTests.cs`, `Ft8TableGenerationTests.cs`, `Ft8TableGeometryTests.cs`,
`ReferenceCloneProbeTests.cs`, `Ldpc/BasisProof.cs`, `Ldpc/Ft8LdpcLayoutTests.cs`,
`Ldpc/Ft8LdpcParityTests.cs`, `Ldpc/Ft8LdpcRefusalTests.cs`,
`Ldpc/Ft8LdpcSecondOpinionTests.cs`, `Ldpc/LdpcCheck.cs`, `Ldpc/Payloads.cs`,
`Ldpc/UpstreamEncoderProvenanceTests.cs`, `TableGen/CSourceParser.cs`,
`TableGen/ExpressionEvaluator.cs`, `TableGen/Ft8TableConverter.cs`,
`TableGen/RepositoryTree.cs`, `TableGen/TableComparison.cs`.

The loop's own machinery (4): `tools/arbiter/outcome-append.bat`,
`tools/arbiter/outcome-entry.py`, `tools/write-status.py`, `write-status.bat`.

Everything else (6): `CLAUDE.md`, `Directory.Build.props`, `Hamlet.sln`, `OUTPUT.md`,
`PROJECT_STATUS.md`, `docs/test-baseline.md`.

**Not one path is under `src/Hamlet.App/`, `src/Hamlet.RadioEngine/`,
`tests/Hamlet.App.Tests/` or `tests/Hamlet.RadioEngine.Tests/`.** The FT8 phase has not
touched a line of Hamlet's source or a line of Hamlet's tests.

**No `.csproj` outside the FT8 project references `Ft8Sharp`.** Grepping every `.csproj` in
the tree finds `Ft8Sharp` only in its own project file's comments and in
`Ft8Sharp.Tests.csproj`'s single `ProjectReference`. `Hamlet.sln` membership is the only
link between this library and the application, and nothing in Hamlet consumes it yet.

### The three channels, and their verdicts

That leaves exactly three ways the phase could have reached a Hamlet test at all. Each was
run by name on 2026-09-01.

**Channel 1 — the two rows added to `CLAUDE.md` §1.** `DecisionLogOrderTests` parses that
table with a regex and asserts it is newest-first. **This is the single most likely way the
phase could have reddened a Hamlet test, and before tonight no unit had ever run it.**

| Test | Verdict |
|---|---|
| `Hamlet.App.Tests.DecisionLogOrderTests.TheDecisionLogIsNewestFirst` | **passed** |
| `Hamlet.App.Tests.DecisionLogOrderTests.EveryRulingAppearsOnceAndTheGapsAreTheKnownOnes` | **passed** |
| `Hamlet.App.Tests.Telemetry.DecisionEmissionTests.TheDecisionLogKeepsTransitionsAndDropsRepeats` | **passed** |
| `Hamlet.App.Tests.Telemetry.DecisionEmissionTests.AnUnchangedVerdictIsNotWrittenAgain` | **passed** |
| `Hamlet.App.Tests.Telemetry.DecisionEmissionTests.NothingTheDecisionLogCopiesCanIdentifyAnybody` | **passed** |
| `Hamlet.App.Tests.Telemetry.DecisionEmissionTests.AStateArrivingAfterConnectReEvaluatesAndEmits` | **passed** |
| `Hamlet.App.Tests.Telemetry.DecisionEmissionTests.ARefusalWithNoButtonPressStillEmits` | **passed** |
| `Hamlet.App.Tests.VoiceTests.TheSweepIsActuallyReadingTheCopy` | **passed** |
| `Hamlet.App.Tests.VoiceTests.NoPassageOfCopyCarriesTwoEmDashes` | **passed** |
| `Hamlet.App.Tests.VoiceTests.CommentsAreNotMistakenForCopy` | **passed** |

**Channel 2 — the root version, moved 1.12.8 → 1.12.11 across the phase.**
`ViewModels/VersionTests` holds the chain from `Directory.Build.props` to the About box.

| Test | Verdict |
|---|---|
| `Hamlet.App.Tests.ViewModels.VersionTests.TheShellAndTheEngineCarryTheSameVersion` | **passed** |
| `Hamlet.App.Tests.ViewModels.VersionTests.TheAboutBoxReportsWhateverTheAssemblySays` | **passed** |
| `Hamlet.App.Tests.ViewModels.VersionTests.TheVersionIsAtLeastTheReleaseThatSetIt` | **passed** |

**Channel 3 — solution membership**, two new projects in `Hamlet.sln`. The engine-side
tests that read the seam and the licensing data: `Hamlet.RadioEngine.Tests.Audio.AudioSeamTests`
(12 cases) and `Hamlet.RadioEngine.Tests.Licensing.PrivilegeTests` (32 cases), plus
`Hamlet.RadioEngine.Tests.Cw.TransmitPrivilegeTests` (11 cases) which the same filter
caught. **55 tests, 55 passed, 0 failed**, in 3 seconds.

**Verdict on attribution: nothing the FT8 phase did has made a Hamlet test red.** 68 tests
on the three channels, every one green, and no Hamlet source or test file changed at all.

---

## The failing set — not measured on 2026-09-01

**This section is empty on purpose, and its emptiness is a fact rather than a claim of
zero.** The `Hamlet.RadioEngine.Tests` run did not return inside its bound, and the stream
it was writing to was block-buffered by unit 205's own filter (caveat 2), so **not one
failing name was read tonight**. `Hamlet.App.Tests` was not run.

**Do not read this as "nothing is red."** The prior reading below says a great deal is.
Read it as: this repository still has no failing set it has measured in isolation, and
producing one is the first job of the next unit that touches this file.

---

## PRIOR READING — unit 204, 2026-08-31, taken under CPU contention

**Everything from here to the skipped set is unit 204's measurement, kept intact. It was
taken at `HEAD` `a3a5c90`, and unit 204 said in its own report that it may over-count.**
Unit 205 did not reproduce it, did not confirm it and did not refute it.

**Why it may over-count.** `Hamlet.RadioEngine.Tests` was run *concurrently with* a stalled
`Hamlet.App.Tests` run, because the alternative was measuring nothing before time ran out.
The two projects wrote to disjoint output directories so nothing was corrupted, but they
shared a CPU. A number of the failures are timeout-shaped: all seventeen `RigReadTests`
failures fail inside the same `ConnectAsync()` helper at `RigReadTests.cs:37`, with
individual cases taking one to four seconds against a *fake* link, and
`AnOrdinaryDisconnectIsImmediate`, `ARigWhoseReadLoopIsStuckStillDisconnects` and
`ItKeepsUpWithLiveAudio` assert on timing by their own names. Against that, the CW fixture
failures decode recorded audio and assert on decoded characters, and those are unlikely to
be contention.

**Tonight's diff against these 84: 0 in both, 0 new, 0 shown green — because tonight
produced no set to diff.** That is the whole of the comparison and it is reported as
nothing rather than as agreement.

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

**Two of these are transmit-safety tests** —
`Cw.AutoCallSafetyTests.NoTestInThisSuiteCanReachARealTransmitter` and
`Scan.BandScannerSafetyTests.AScanNeverKeysTheTransmitter`. They were red before the FT8
phase existed and nothing in it goes anywhere near transmit. They are named here and not
investigated, which is what the phase plan says to do with them.

---

## The skipped set

Kept deliberately separate from the failing set.

### `Ft8Sharp.Tests` (1)

- `Ft8Sharp.Tests.Ft8TableGenerationTests.RewriteTheCheckedInTablesFile`

`Hamlet.RadioEngine.Tests`' and `Hamlet.App.Tests`' skip counts have never been read,
because neither run has ever reached its summary line.

**A skip is not a failure here.** Reference material — off-air recordings and the pinned
upstream `ft8_lib` clone at `C:\Source\ft8_lib` — is never committed to this repository, so
tests that need it report skipped when it is absent rather than red. On a machine without
the clone, six `Ft8Sharp` tests skip instead of one. That is what keeps a fresh clone green
for somebody who has just downloaded the project, and it is a deliberate design rather than
a gap.

---

## How to use this file

A later unit re-runs the suite and compares **name by name** against the lists above. Any
name that has appeared, and any name that has disappeared, is the finding — and it is
reported as a finding whichever direction it went, because a red that has quietly gone green
is as much a change to the inherited set as a green that has gone red.

**A matching count is not a match.** This is the whole reason HM-DEC-151 asks for names
rather than a number: one inherited red going green while a green goes red leaves the total
exactly where it was, and a count-only comparison sees nothing. Compare the sets, not their
sizes.

**The one list in here that is measured and current is the attribution section**, and it is
measured in a way that does not need the suite to finish: the paths the phase changed, and
the verdict of every test in the tree that reads any of them.

**The correct next use of this file is still to replace its failing set** — run
`Hamlet.RadioEngine.Tests` alone, with an unfiltered console logger so the stream survives an
early stop, and rewrite the list from that run before anyone diffs against it. Unit 204's 84
are a first draft with a known upward bias, and unit 205 did not manage to confirm or refute
them.

## What this file is not

**It is not a list of bugs to fix.** Nobody is assigned to any name in it by its presence
here.

**It is not a ratchet the FT8 phase owns.** The CW work and the rig work predate this phase,
and this phase neither created these failures nor took responsibility for them. Its
obligation is narrower: do not add to the set, and do not silently change it. The attribution
section is the evidence that it has done neither.

**And it is not a licence to leave the CW reds alone forever.** HM-DEC-151's own words — it
says only that fixing them is not this phase's work. Somebody's night, eventually.
