# Unit 392 - the seams at 7e209cb4, and what the restore keeps

Written by work instruction 392 task 1, 2026-09-22, from the tree at 10512248 (src and tests
identical to 3d6a2c12 and to a1fd388c), before anything under src was restored. By grep and
`git grep` over 7e209cb4, never a compiler: task 2's build is the real check, and its results
are written under their own headings below when task 2 and task 4 add them.

Scripts, not committed: `.run-unit/unit392-seams-at.sh` (unit 391's `unit391-seams-at.sh`
copied and pointed at 7e209cb4, reading the same 145 rows of `.run-unit/unit391-members.txt`),
`unit392-headonly.sh`, `unit392-keepdeps.sh`, `unit392-tap.sh`, `unit392-tool.sh`.

## 1. The Cw folder at the two commits

`git diff --name-status 7e209cb4 HEAD -- src/Hamlet.RadioEngine/Cw`: 20 rows, **10 `M` and
10 `A`**, no `D`. The ten `A` rows are exactly the ten HEAD-only files the instruction names.
**None of the eleven transmit files is in either list**, and
`git diff --stat 7e209cb4 HEAD -- <the eleven>` printed nothing: they are byte-identical at the
two commits and all eleven exist at HEAD.

| | File |
|---|---|
| A | `src/Hamlet.RadioEngine/Cw/CwAccuracy.cs` |
| M | `src/Hamlet.RadioEngine/Cw/CwCharacter.cs` |
| M | `src/Hamlet.RadioEngine/Cw/CwCounterTrail.cs` |
| M | `src/Hamlet.RadioEngine/Cw/CwDecodeReport.cs` |
| M | `src/Hamlet.RadioEngine/Cw/CwDecoder.cs` |
| A | `src/Hamlet.RadioEngine/Cw/CwElementPitch.cs` |
| A | `src/Hamlet.RadioEngine/Cw/CwJointCutter.cs` |
| M | `src/Hamlet.RadioEngine/Cw/CwKeyingMeter.cs` |
| A | `src/Hamlet.RadioEngine/Cw/CwPitchChoice.cs` |
| A | `src/Hamlet.RadioEngine/Cw/CwPitchRanking.cs` |
| A | `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.Posterior.cs` |
| M | `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` |
| M | `src/Hamlet.RadioEngine/Cw/CwProbabilisticStream.cs` |
| A | `src/Hamlet.RadioEngine/Cw/CwReferenceDecoder.cs` |
| A | `src/Hamlet.RadioEngine/Cw/CwSpectralPeak.cs` |
| A | `src/Hamlet.RadioEngine/Cw/CwStreamSplit.cs` |
| A | `src/Hamlet.RadioEngine/Cw/CwSwingSurvey.cs` |
| M | `src/Hamlet.RadioEngine/Cw/CwToneSurvey.cs` |
| M | `src/Hamlet.RadioEngine/Cw/CwToneTracker.cs` |
| M | `src/Hamlet.RadioEngine/Cw/CwUnitEstimator.cs` |

`git log 7e209cb4..HEAD -- src/Hamlet.RadioEngine/Cw`: **45 commits**, `1a84188e` (2026-09-03)
down to `2068f868` (2026-08-25), as the instruction says.

## 2. The app and app-test seams at 7e209cb4 (0.4 at the commit R53 named)

Unit 391's 145 file-and-type rows, each re-checked at 7e209cb4: the type looked for by
declaration under `src/Hamlet.RadioEngine/Cw` at that commit, each member by whole word in the
file that declares it there. **136 rows have the type and every member; 9 rows name a type
absent at 7e209cb4, and 10 rows have something absent.**

Read against the tree, six of the nine absent-type rows are not seams, and three are:

- **`Outcome`** (AppEvents.cs, CwTransmitViewModel.cs, MainWindowViewModel.cs,
  CallsignPrivacyTests.cs, DecisionEmissionTests.cs): the `Outcome` those files mean is
  `Hamlet.RadioEngine.Telemetry.Outcome` (`src/Hamlet.RadioEngine/Telemetry/Outcome.cs`), not
  the enum nested in HEAD-only `CwAccuracy.cs`. Unit 391 already flagged the name as common.
- **`Envelope`** (MainWindowViewModel.cs): the app calls the static method
  `CwProbabilisticDecoder.Envelope(...)` at line 12402, which **exists at 7e209cb4**
  (`CwProbabilisticDecoder.cs:807` there). The absent `Envelope` is the record struct in
  HEAD-only `CwReferenceDecoder.cs`, which the app does not use.

The real seams at 7e209cb4, all in the app's CW code:

| App file | Engine type | Absent at 7e209cb4 | What task 2 expects to need |
|---|---|---|---|
| `MainWindowViewModel.cs` | `CwElementPitch` (HEAD-only) | the type; `Measure`, `MeasureAll` | keep the file (decision 4) |
| `MainWindowViewModel.cs` | `CwStreamSplit` (HEAD-only) | the type; `Divide`, `LeastTrustedMarks`, `None` | keep the file (decision 4) |
| `MainWindowViewModel.cs` | `CwPitchChoice` (HEAD-only) | the type; `Keying`, `OperatorAssertion`, `Ranked`, `StrongestBin` | keep the file (decision 4) |
| `MainWindowViewModel.cs` | `CwCharacter` | `MarginLlr`, `MarginShareForRecord`, `WidestRecordedLlr` | thin members, absent value |
| `MainWindowViewModel.cs` | `CwDecoder` | `AssertStation`, `DecodeQueueDroppedChunks`, `DecodeQueueDroppedSamples`, `DigitalMode`, `PitchWasAsserted`, `Ranked`, `Retuned` | thin members; `DigitalMode` with its gate |
| `MainWindowViewModel.cs` | `CwProbabilisticStream` | `SamplesSeen` | forward or absent value |
| `ScanViewModel.cs` | `CwDecoder` | `Ranked` | thin member |
| `ThePitchControlsAreOffThePanelTests.cs` (app test) | `CwDecoder` | `AssertAt`, `AssertStation`, `PitchWasAsserted` | thin members, or exclusion if the test asserts the mechanism |

**Present at 7e209cb4, which the instruction asked to be checked:** `ListeningAfresh`,
`Tap`, `IsLocked`, `Unlock`, `LockedToneHz`, `LeadingEdge`, `DecodingSuspended` on
`CwDecoder`; `SpanLogLikelihoodRatio` and `WordsPerMinute` on the reading record in
`CwProbabilisticDecoder.cs`; `CwProbabilisticDecoder.Envelope`. The grep sees the member names
only; whether each has the same shape is task 2's build.

## 3. The engine-test seams (1.2's evidence, before the build)

Every `.cs` under `tests/Hamlet.RadioEngine.Tests/Cw` that names a type declared only in a
HEAD-only Cw file. `Outcome`, `Candidate` and `Cutting` are left out: each is also a type
outside Cw and every hit was checked to be that other type or a transmit file's.

| Engine test file | HEAD-only name it quotes |
|---|---|
| `TheCleanReadsStayCleanTests.cs` | `CwAccuracy` |
| `TheProbabilisticDecoderTests.cs` | `CwAccuracy` |
| `TheScoreSaysWhatItIsMeasuringTests.cs` | `CwAccuracy` |
| `EveryElementCarriesItsOwnPitchTests.cs` | `CwElementPitch`, `CwElement` |
| `NoSenderIsSplitInTwoTests.cs` | `CwElementPitch`, `CwElement`, `CwStreamSplit`, `CwStreamDivision` |
| `NothingActsOnTheAdmissionVerdictTests.cs` | `CwPitchChoice` |
| `TheQuietestBinNoLongerWinsTests.cs` | `CwPitchChoice`, `CwPitchRanking` |
| `ABlipDoesNotShiftEverythingAfterItTests.cs` | `CwReferenceDecoder` |
| `TheReferenceDecoderIsPortedFaithfullyTests.cs` | `CwReferenceDecoder` |
| `IsTheHertzABiasOrAFloorTests.cs` | `CwSpectralPeak` |
| `ThePeakAgainstASecondSignalTests.cs` | `CwSpectralPeak` |
| `ThePeakFindsThePitchTheTrackerMissedTests.cs` | `CwSpectralPeak` |
| `AStationIsABinThatSwingsTests.cs` | `CwSwingSurvey` |

`TheAdjudicatedReadingsKeepReadingTests.cs` names `CwSpectralPeak` in a comment only (line
116), not in code. Twenty more files name the word `Envelope`; by the reading in section 2
most of those are `CwProbabilisticDecoder.Envelope(...)`, which exists at 7e209cb4. Members
absent from the 7e209cb4 copy of a restored file are not listed by grep here: 31 of the 34
files under the folder were added since 7e209cb4, and the build lists them exactly.

## 4. The keep list (decision 4), written before the restore

None of 7e209cb4's `CwDecoder.cs`, `CwProbabilisticDecoder.cs`, `CwProbabilisticStream.cs`,
`CwToneSurvey.cs`, `CwToneTracker.cs`, `CwUnitEstimator.cs` or `CwKeyingMeter.cs` - nor any
other file of the 33 under Cw at that commit - names a type declared in a HEAD-only file
(grep exit 1). So no kept file is on the restored decode path.

| HEAD-only file | Kept or deleted | Why |
|---|---|---|
| `CwElementPitch.cs` | **kept** | `MainWindowViewModel.ElementPitchLine` calls `CwElementPitch.MeasureAll` (line 12408), and the 1.4 test `TheSheetSaysWhatEachElementWasSentAtTests` reads that line |
| `CwStreamSplit.cs` | **kept** | `MainWindowViewModel.cs` 12412-12414 calls `CwStreamSplit.Divide` and `LeastTrustedMarks` |
| `CwPitchChoice.cs` | **kept** | `MainWindowViewModel.cs` 12515-12558 reads `CwPitchChoice` |
| `CwJointCutter.cs` | **kept** | declares `CwElement`, which `CwElementPitch` and `CwStreamSplit` take and return; not called by the restored decoder |
| `CwAccuracy.cs` | deleted | outside Cw only `tools/Hamlet.PitchRank` names it; engine tests excluded instead |
| `CwPitchRanking.cs` | deleted | the same |
| `CwReferenceDecoder.cs` | deleted | the same |
| `CwSpectralPeak.cs` | deleted | the same; the adjudicated floor test names it in a comment only |
| `CwSwingSurvey.cs` | deleted | nothing outside Cw names it |
| `CwProbabilisticDecoder.Posterior.cs` | deleted | a part of the decoder class itself, the rework's posterior; it is the decode path |

**`tools/Hamlet.PitchRank`** is in `Hamlet.sln` and names `CwAccuracy`, `CwPitchRanking`,
`CwReferenceDecoder`, `CwSpectralPeak`, `CwElementPitch`, `CwStreamSplit` and
`CwProbabilisticDecoder.DecodeUngated`. It was added on 2026-08-28 (`0fc14965`, *measure what
a decode costs at one pitch*), after 7e209cb4: it is a bench instrument of the rework. Decision
4 names the app, the 1.4 tests and the carry-forward names, not tools. **Author's, overrulable:**
it is treated as decision 3 treats a test - left in the tree unedited and taken out of the
solution's build, so step 4 brings it back with the pieces it measures. Task 2 records how.

## 5. The two guarded seams

**`CwDecoder.Tap`.** At 7e209cb4 `Process` feeds the tap first, `Tap.Take(chunk.Samples,
chunk.SampleRate)` at line 466, on the samples as they arrived from the source and at their
own rate, before any decode or suspension check; `OnSamples` calls `Process` directly. At HEAD
the callback thread feeds the tap synchronously and queues the chunk for a worker, which calls
`Process(..., takeIntoTap: false)`; `Process(chunk)` still feeds it (line 888), again at the
chunk's own rate. Neither commit feeds the tap from below a resampler: there is no resampler
in `CwDecoder.cs` at either commit. `TheCaptureButtonTests` asserts the written file's
`SampleRate` equals the device rate (lines 119 and 314) and the sidecar's `deviceSampleRate`
(196, 200, 327). **Expected adaptation: none**; the restored feed is at device rate, and the
carry-forward app line carries `TheCaptureButtonTests` to prove it.

**`CwDecoder.DigitalMode`.** Absent at 7e209cb4. `865e66d8` (2026-09-03) added it: a settable
bool, and in `Process` `if (DecodingSuspended || DigitalMode)` the chunk is counted, the
stream is skipped by its length so the audio clock keeps running, and nothing is decoded; the
same commit set it from `MainWindowViewModel.cs` and added
`tests/Hamlet.RadioEngine.Tests/Audio/NoCwDecodeInDigitalModeTests.cs`. 7e209cb4's `Process`
already has the `DecodingSuspended` arm (line 468). **Expected adaptation: re-add the property
and put it beside `DecodingSuspended` in that arm**, with whatever the suspended arm does there
to keep time.

## 6. The seams at 7e209cb4, every row

| File | Type | At 7e209cb4 | Members absent there (grep) |
|---|---|---|---|
| `src/Hamlet.App/Controls/CwTerminalControl.cs` | CwCharacter | type there | none |
| `src/Hamlet.App/Controls/CwTerminalControl.cs` | CwConfidence | type there | none |
| `src/Hamlet.App/Controls/CwTerminalControl.cs` | MorseAlphabet | type there | none |
| `src/Hamlet.App/Controls/ModePalette.cs` | CwConfidence | type there | none |
| `src/Hamlet.App/Telemetry/AppEvents.cs` | CwDecodeReport | type there | none |
| `src/Hamlet.App/Telemetry/AppEvents.cs` | CwReadiness | type there | none |
| `src/Hamlet.App/Telemetry/AppEvents.cs` | Outcome | TYPE ABSENT | none |
| `src/Hamlet.App/Telemetry/AppEvents.cs` | TransmitChain | type there | none |
| `src/Hamlet.App/ViewModels/AutoCallViewModel.cs` | AutoCallOutcome | type there | none |
| `src/Hamlet.App/ViewModels/AutoCallViewModel.cs` | AutoCallSettings | type there | none |
| `src/Hamlet.App/ViewModels/AutoCallViewModel.cs` | AutoCallStop | type there | none |
| `src/Hamlet.App/ViewModels/AutoCallViewModel.cs` | AutoCallTransmission | type there | none |
| `src/Hamlet.App/ViewModels/AutoCallViewModel.cs` | AutoCallWindow | type there | none |
| `src/Hamlet.App/ViewModels/AutoCallViewModel.cs` | AutoCaller | type there | none |
| `src/Hamlet.App/ViewModels/AutoCallViewModel.cs` | CwCharacter | type there | none |
| `src/Hamlet.App/ViewModels/AutoCallViewModel.cs` | CwDecoder | type there | none |
| `src/Hamlet.App/ViewModels/AutoCallViewModel.cs` | CwMessage | type there | none |
| `src/Hamlet.App/ViewModels/AutoCallViewModel.cs` | KeyerCwSender | type there | none |
| `src/Hamlet.App/ViewModels/AutoCallViewModel.cs` | TransmitReadiness | type there | none |
| `src/Hamlet.App/ViewModels/CwTranscript.cs` | CwCharacter | type there | none |
| `src/Hamlet.App/ViewModels/CwTranscript.cs` | CwReadingStage | type there | none |
| `src/Hamlet.App/ViewModels/CwTransmitViewModel.cs` | ContactScript | type there | none |
| `src/Hamlet.App/ViewModels/CwTransmitViewModel.cs` | ContactStage | type there | none |
| `src/Hamlet.App/ViewModels/CwTransmitViewModel.cs` | CwDuration | type there | none |
| `src/Hamlet.App/ViewModels/CwTransmitViewModel.cs` | CwMessage | type there | none |
| `src/Hamlet.App/ViewModels/CwTransmitViewModel.cs` | CwReadiness | type there | none |
| `src/Hamlet.App/ViewModels/CwTransmitViewModel.cs` | CwReadyState | type there | none |
| `src/Hamlet.App/ViewModels/CwTransmitViewModel.cs` | CwTransmitter | type there | none |
| `src/Hamlet.App/ViewModels/CwTransmitViewModel.cs` | Outcome | TYPE ABSENT | none |
| `src/Hamlet.App/ViewModels/CwTransmitViewModel.cs` | SendOption | type there | none |
| `src/Hamlet.App/ViewModels/CwTransmitViewModel.cs` | SwrReport | type there | none |
| `src/Hamlet.App/ViewModels/CwTransmitViewModel.cs` | TransmissionEnd | type there | none |
| `src/Hamlet.App/ViewModels/CwTransmitViewModel.cs` | TransmissionWatch | type there | none |
| `src/Hamlet.App/ViewModels/CwTransmitViewModel.cs` | TransmitChain | type there | none |
| `src/Hamlet.App/ViewModels/CwTransmitViewModel.cs` | TransmitContext | type there | none |
| `src/Hamlet.App/ViewModels/CwTransmitViewModel.cs` | TransmitEvidence | type there | none |
| `src/Hamlet.App/ViewModels/CwTransmitViewModel.cs` | TransmitNotes | type there | none |
| `src/Hamlet.App/ViewModels/CwTransmitViewModel.cs` | TransmitOutcome | type there | none |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | CallsignResolver | type there | none |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | ContactStage | type there | none |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | CwCase | type there | none |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | CwCaseRoster | type there | none |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | CwCharacter | type there | MarginLlr MarginShareForRecord WidestRecordedLlr |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | CwCounterDelta | type there | none |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | CwCounterSample | type there | none |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | CwCounterTrail | type there | none |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | CwCountsCover | type there | none |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | CwDecodeReport | type there | none |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | CwDecodeStory | type there | none |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | CwDecoder | type there | AssertStation DecodeQueueDroppedChunks DecodeQueueDroppedSamples DigitalMode PitchWasAsserted Ranked Retuned |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | CwElementPitch | TYPE ABSENT | Measure MeasureAll |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | CwKeyingMeter | type there | none |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | CwMessage | type there | none |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | CwPitchChoice | TYPE ABSENT | Keying OperatorAssertion Ranked StrongestBin |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | CwProbabilisticDecoder | type there | none |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | CwProbabilisticStream | type there | SamplesSeen |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | CwReadiness | type there | none |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | CwStreamSplit | TYPE ABSENT | Divide LeastTrustedMarks None |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | CwTransmitter | type there | none |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | Envelope | TYPE ABSENT | Decode None Run Runs Text WordsPerMinute |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | KeyerCwSender | type there | none |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | KeyingEnvelope | type there | none |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | KeyingReading | type there | none |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | KeyingVerdict | type there | none |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | Outcome | TYPE ABSENT | Block None Score |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | SendOption | type there | none |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | TransmissionEnd | type there | none |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | TransmitChain | type there | none |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | TransmitContext | type there | none |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | TransmitEvidence | type there | none |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | TransmitOutcome | type there | none |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | TransmitReadiness | type there | none |
| `src/Hamlet.App/ViewModels/PhrasebookViewModel.cs` | CwPhrase | type there | none |
| `src/Hamlet.App/ViewModels/PhrasebookViewModel.cs` | CwPhrasebook | type there | none |
| `src/Hamlet.App/ViewModels/PhrasebookViewModel.cs` | PhraseKind | type there | none |
| `src/Hamlet.App/ViewModels/ScanViewModel.cs` | CwCharacter | type there | none |
| `src/Hamlet.App/ViewModels/ScanViewModel.cs` | CwDecoder | type there | Ranked |
| `tests/Hamlet.App.Tests/Cw/TheSheetSaysWhatEachElementWasSentAtTests.cs` | CwDecodeReport | type there | none |
| `tests/Hamlet.App.Tests/Telemetry/ARefusedPressLeavesALineTests.cs` | CwDecoder | type there | none |
| `tests/Hamlet.App.Tests/Telemetry/CallsignPrivacyTests.cs` | CwConfidence | type there | none |
| `tests/Hamlet.App.Tests/Telemetry/CallsignPrivacyTests.cs` | CwDecodeReport | type there | none |
| `tests/Hamlet.App.Tests/Telemetry/CallsignPrivacyTests.cs` | Outcome | TYPE ABSENT | none |
| `tests/Hamlet.App.Tests/Telemetry/CallsignPrivacyTests.cs` | TransmitChain | type there | none |
| `tests/Hamlet.App.Tests/Telemetry/CallsignPrivacyTests.cs` | TransmitReadiness | type there | none |
| `tests/Hamlet.App.Tests/Telemetry/DecisionEmissionTests.cs` | CwMessage | type there | none |
| `tests/Hamlet.App.Tests/Telemetry/DecisionEmissionTests.cs` | CwReadiness | type there | none |
| `tests/Hamlet.App.Tests/Telemetry/DecisionEmissionTests.cs` | CwReadyState | type there | none |
| `tests/Hamlet.App.Tests/Telemetry/DecisionEmissionTests.cs` | CwSendOutcome | type there | none |
| `tests/Hamlet.App.Tests/Telemetry/DecisionEmissionTests.cs` | CwSendResult | type there | none |
| `tests/Hamlet.App.Tests/Telemetry/DecisionEmissionTests.cs` | CwTransmitter | type there | none |
| `tests/Hamlet.App.Tests/Telemetry/DecisionEmissionTests.cs` | ICwSender | type there | none |
| `tests/Hamlet.App.Tests/Telemetry/DecisionEmissionTests.cs` | Outcome | TYPE ABSENT | none |
| `tests/Hamlet.App.Tests/Telemetry/DecisionEmissionTests.cs` | TransmitContext | type there | none |
| `tests/Hamlet.App.Tests/Telemetry/ThePressActuallyWritesItsCaptureTests.cs` | CwDecoder | type there | none |
| `tests/Hamlet.App.Tests/Telemetry/Unit305SettledTests.cs` | CwReadyState | type there | none |
| `tests/Hamlet.App.Tests/Telemetry/Unit305SettledTests.cs` | TransmitReadiness | type there | none |
| `tests/Hamlet.App.Tests/ViewModels/AHeldVerdictPrintsNoMeasurementsTests.cs` | KeyingReading | type there | none |
| `tests/Hamlet.App.Tests/ViewModels/AHeldVerdictPrintsNoMeasurementsTests.cs` | KeyingVerdict | type there | none |
| `tests/Hamlet.App.Tests/ViewModels/ASheetSaysWhichInstrumentSpokeTests.cs` | CwCase | type there | none |
| `tests/Hamlet.App.Tests/ViewModels/ASheetSaysWhichInstrumentSpokeTests.cs` | CwCaseRoster | type there | none |
| `tests/Hamlet.App.Tests/ViewModels/ASheetSaysWhichInstrumentSpokeTests.cs` | CwCountsCover | type there | none |
| `tests/Hamlet.App.Tests/ViewModels/AutoCallFaceTests.cs` | CwMessage | type there | none |
| `tests/Hamlet.App.Tests/ViewModels/CaseRosterSurvivesAnEveningTests.cs` | CwCase | type there | none |
| `tests/Hamlet.App.Tests/ViewModels/CaseRosterSurvivesAnEveningTests.cs` | CwCaseRoster | type there | none |
| `tests/Hamlet.App.Tests/ViewModels/CaseRosterSurvivesAnEveningTests.cs` | CwCharacter | type there | none |
| `tests/Hamlet.App.Tests/ViewModels/CaseRosterSurvivesAnEveningTests.cs` | CwCountsCover | type there | none |
| `tests/Hamlet.App.Tests/ViewModels/CaseRosterSurvivesAnEveningTests.cs` | CwDecoder | type there | none |
| `tests/Hamlet.App.Tests/ViewModels/DummyLoadNoticeTests.cs` | TransmitNotes | type there | none |
| `tests/Hamlet.App.Tests/ViewModels/ScannerFaceTests.cs` | CwCharacter | type there | none |
| `tests/Hamlet.App.Tests/ViewModels/ScannerFaceTests.cs` | CwConfidence | type there | none |
| `tests/Hamlet.App.Tests/ViewModels/ScannerFaceTests.cs` | MorseAlphabet | type there | none |
| `tests/Hamlet.App.Tests/ViewModels/SendButtonEnablementTests.cs` | CwMessage | type there | none |
| `tests/Hamlet.App.Tests/ViewModels/SendButtonEnablementTests.cs` | CwReadiness | type there | none |
| `tests/Hamlet.App.Tests/ViewModels/SendButtonEnablementTests.cs` | CwReadyState | type there | none |
| `tests/Hamlet.App.Tests/ViewModels/SendButtonEnablementTests.cs` | CwSendOutcome | type there | none |
| `tests/Hamlet.App.Tests/ViewModels/SendButtonEnablementTests.cs` | CwSendResult | type there | none |
| `tests/Hamlet.App.Tests/ViewModels/SendButtonEnablementTests.cs` | CwTransmitter | type there | none |
| `tests/Hamlet.App.Tests/ViewModels/SendButtonEnablementTests.cs` | ICwSender | type there | none |
| `tests/Hamlet.App.Tests/ViewModels/SendButtonEnablementTests.cs` | TransmitContext | type there | none |
| `tests/Hamlet.App.Tests/ViewModels/SendGuardTests.cs` | ContactStage | type there | none |
| `tests/Hamlet.App.Tests/ViewModels/SendGuardTests.cs` | CwDuration | type there | none |
| `tests/Hamlet.App.Tests/ViewModels/SendGuardTests.cs` | CwMessage | type there | none |
| `tests/Hamlet.App.Tests/ViewModels/SendGuardTests.cs` | CwSendOutcome | type there | none |
| `tests/Hamlet.App.Tests/ViewModels/SendGuardTests.cs` | CwSendResult | type there | none |
| `tests/Hamlet.App.Tests/ViewModels/SendGuardTests.cs` | CwTransmitter | type there | none |
| `tests/Hamlet.App.Tests/ViewModels/SendGuardTests.cs` | ICwSender | type there | none |
| `tests/Hamlet.App.Tests/ViewModels/SendGuardTests.cs` | SendOption | type there | none |
| `tests/Hamlet.App.Tests/ViewModels/SendGuardTests.cs` | TransmitContext | type there | none |
| `tests/Hamlet.App.Tests/ViewModels/SendLengthIsLegibleTests.cs` | ContactStage | type there | none |
| `tests/Hamlet.App.Tests/ViewModels/SendLengthIsLegibleTests.cs` | CwDuration | type there | none |
| `tests/Hamlet.App.Tests/ViewModels/SendLengthIsLegibleTests.cs` | SendOption | type there | none |
| `tests/Hamlet.App.Tests/ViewModels/SentTextNeverEntersTheReceivedStreamTests.cs` | CwCharacter | type there | none |
| `tests/Hamlet.App.Tests/ViewModels/SentTextNeverEntersTheReceivedStreamTests.cs` | CwDecoder | type there | none |
| `tests/Hamlet.App.Tests/ViewModels/SentTextNeverEntersTheReceivedStreamTests.cs` | CwMessage | type there | none |
| `tests/Hamlet.App.Tests/ViewModels/SentTextNeverEntersTheReceivedStreamTests.cs` | CwProbabilisticStream | type there | none |
| `tests/Hamlet.App.Tests/ViewModels/TheSpanRatioReachesTheSidecarTests.cs` | CwCharacter | type there | none |
| `tests/Hamlet.App.Tests/ViewModels/TheSpanRatioReachesTheSidecarTests.cs` | CwConfidence | type there | none |
| `tests/Hamlet.App.Tests/ViewModels/TheSpanRatioReachesTheSidecarTests.cs` | MorseAlphabet | type there | none |
| `tests/Hamlet.App.Tests/ViewModels/TwoStageTranscriptTests.cs` | CwCharacter | type there | none |
| `tests/Hamlet.App.Tests/ViewModels/TwoStageTranscriptTests.cs` | CwConfidence | type there | none |
| `tests/Hamlet.App.Tests/ViewModels/TwoStageTranscriptTests.cs` | CwReadingStage | type there | none |
| `tests/Hamlet.App.Tests/ViewModels/UnresolvedLicenseTests.cs` | TransmitContext | type there | none |
| `tests/Hamlet.App.Tests/Views/HistoryRecedesAndCurrentCopyDoesNotTests.cs` | CwCharacter | type there | none |
| `tests/Hamlet.App.Tests/Views/HistoryRecedesAndCurrentCopyDoesNotTests.cs` | CwConfidence | type there | none |
| `tests/Hamlet.App.Tests/Views/ThePitchControlsAreOffThePanelTests.cs` | CwDecoder | type there | AssertAt AssertStation PitchWasAsserted |
