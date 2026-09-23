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

The grep over-lists: `CwProbabilisticStream.SamplesSeen` and `ScanViewModel`'s
`CwDecoder.Ranked` are listed above as absent, and task 2's build raised neither, so both
names appear there only in comments or on another type.

---

Sections 7 to 10 were written by task 4, after task 2's commit `9fbb4728`, from the build and
from `git diff`.

## 7. What the restore kept and deleted, as built (1.1)

Task 2 ran `.run-unit/unit392-restore.sh`: the transmit diff printed nothing;
`git checkout 7e209cb4 -- src/Hamlet.RadioEngine/Cw` moved the ten `M` files; `git rm` took out
the six files section 4 marked deleted. **1.1's proof, pasted before a seam was touched:**

```
git diff --stat 7e209cb4 -- src/Hamlet.RadioEngine/Cw
 src/Hamlet.RadioEngine/Cw/CwElementPitch.cs | 266 ++++++++++++++++++
 src/Hamlet.RadioEngine/Cw/CwJointCutter.cs  | 344 +++++++++++++++++++++++
 src/Hamlet.RadioEngine/Cw/CwPitchChoice.cs  |  72 +++++
 src/Hamlet.RadioEngine/Cw/CwStreamSplit.cs  | 410 ++++++++++++++++++++++++++++
 4 files changed, 1092 insertions(+)
```

**The keep list changed during the build, and this is why.** `ElementPitchLine` calls
`CwElementPitch.MeasureAll(read.Elements, ...)`, and `read.Elements` is the list of marks and
gaps HEAD's decoder hands out from its winning path. 7e209cb4's `CwProbabilisticResult` has no
such member: its path is walked in the private `Spell` and only characters come out. Keeping
`CwElementPitch` therefore did not make the line buildable. The two ways to make it build were
to thread `CwElement`, a type from a HEAD-only file, back through 7e209cb4's decode function,
which section 9 of the instruction parks (*nothing from a HEAD-only file goes back into the
decode path here, however small*), or to change the line in the app. The line was changed
(section 9 below, hunk 2), after which nothing outside Cw named `CwElementPitch`,
`CwStreamSplit` or `CwJointCutter`, and decision 4 deleted all three. **Kept: `CwPitchChoice.cs`
only**, because `CwDecodeReport.PitchChoice` returns it and `MainWindowViewModel.cs` reads it.
Deleted: the other nine.

**`tools/Hamlet.PitchRank`** (section 4): its six `Build.0` lines were taken out of
`Hamlet.sln`, so the project stays in the solution and in the tree, unedited, and
`dotnet build Hamlet.sln` no longer builds it. The `ActiveCfg` lines stay. Restoring the six lines
puts it back. It would not compile against the restored engine: it names `CwAccuracy`,
`CwPitchRanking`, `CwReferenceDecoder`, `CwSpectralPeak`, `CwElementPitch`, `CwStreamSplit` and
`CwProbabilisticDecoder.DecodeUngated`'s HEAD overloads.

## 8. The 1.1 adaptation table - every hunk of `git diff 7e209cb4 HEAD -- src/Hamlet.RadioEngine/Cw`

`git diff --stat 7e209cb4 HEAD -- src/Hamlet.RadioEngine/Cw` at task 4: 4 files, 165
insertions, 1 deletion. **Nothing else is in that diff.**

| # | File | Hunk | What | Why, and why it is honest |
|---|---|---|---|---|
| 1 | `CwPitchChoice.cs` | `@@ -0,0 +1,72` | the whole file, HEAD's copy | kept HEAD-only file: `CwDecodeReport.PitchChoice` returns it and `MainWindowViewModel.ToneForTheRecord` and `EmittedWithoutKeying` read it (decision 4) |
| 2 | `CwCharacter.cs` | `@@ -144,0 +145,36` | `WidestRecordedLlr` const 1,000,000; `MarginLlr { get; init; } = NaN`; `MarginShareForRecord`, HEAD's arithmetic | `MainWindowViewModel.Clamped` and `SpanRatioLine` read all three; `TheSpanRatioReachesTheSidecarTests` sets `MarginLlr`. The const bounds what the sheet prints, not a measurement; the margin is NaN because this decoder never compares two paths and never sets it, and the sheet prints NaN as *unmeasured* |
| 3 | `CwDecodeReport.cs` | `@@ -62,0 +63,23` | `PitchWasAsserted => false`; `PitchChoice => PitchWasMeasured ? Keying : NotChosen` | `ToneForTheRecord` and `EmittedWithoutKeying` read both. Nothing in this build takes an operator's assertion, so false is true; at 7e209cb4 an unmeasured pitch is the middle of the bank (`CwToneTracker.cs` line 441 there), which is `NotChosen` |
| 4 | `CwDecoder.cs` | `@@ -350,0 +351,12` | `DigitalMode { get; set; }` | set from `MainWindowViewModel.cs:14155`; 865e66d8's gate, carried across (instruction, task 2) |
| 5 | `CwDecoder.cs` | `@@ -353,0 +366,21` | `DecodeQueueDroppedChunks => 0`, `DecodeQueueDroppedSamples => 0`, `Retuned() => Unlock()` | the counters feed `AudioArrival` at `MainWindowViewModel.cs:7480`. This decoder has no queue, so nothing is dropped from one; HEAD's own figure was nought whenever its queue was not running, and the app already writes nought when there is no decoder. **Author's, overrulable: nought, not an app hunk.** `Retuned` is called at 13275 when the dial moves 500 Hz; the lock is the part of HEAD's `Retuned` this decoder has state for |
| 6 | `CwDecoder.cs` | `@@ -468 +501` | `if (DecodingSuspended)` becomes `if (DecodingSuspended \|\| DigitalMode)` | the gate: in Digital the tap still takes the audio, the chunk is counted as suspended, the stream is skipped by its length, and nothing is decoded - 7e209cb4's own suspended arm |

The adaptation to `CwElementPitch.cs` in task 2 (two `<see cref="CwSpectralPeak"/>` turned to
`<c>`, because the cref no longer resolved under warnings as errors) went out with the file.

## 9. The exclusion table (1.2) - every `<Compile Remove>`

22 files, 21 engine and 1 app, each left in the tree unedited; the missing name is quoted from
task 2's build (`.run-unit/unit392-errors-2.txt` and `-3.txt`, not committed). **In
`docs\unit239-failing-set.txt`:** only `ABlipDoesNotShiftEverythingAfterItTests`
(`ASubMinimumBlipInAGapChangesNothingAfterIt`). None is on either carry-forward line and none
is a 1.4 test.

| Project | File | Missing name it quotes | In unit239 |
|---|---|---|---|
| engine | `Audio/TheReadPathDoesNotAllocateTests.cs` | `CwKeyingMeter.WindowSizings` | no |
| engine | `Audio/TheTapIsNotBehindTheDecoderTests.cs` | `CwDecoder.ProcessDelayForTests` | no |
| engine | `Cw/ABlipDoesNotShiftEverythingAfterItTests.cs` | `CwReferenceDecoder` | **yes** |
| engine | `Cw/AMoveStartsTheDecoderFreshTests.cs` | `CwDecoder.PitchWasAsserted`, `CwDecoder.Ranked` | no |
| engine | `Cw/AStationIsABinThatSwingsTests.cs` | `CwSwingSurvey` | no |
| engine | `Cw/EveryElementCarriesItsOwnPitchTests.cs` | `CwProbabilisticResult.Elements`, `CwElementPitch` | no |
| engine | `Cw/FittingKeyUpAgainstAssumingItTests.cs` | `CwProbabilisticDecoder.FittedLogLikelihoods` | no |
| engine | `Cw/IsTheHertzABiasOrAFloorTests.cs` | `CwSpectralPeak` | no |
| engine | `Cw/NoSenderIsSplitInTwoTests.cs` | `CwStreamSplit`, `CwProbabilisticResult.Elements` | no |
| engine | `Cw/NothingActsOnTheAdmissionVerdictTests.cs` | `CwDecodeReport` constructor parameter `PitchChoice` | no |
| engine | `Cw/TheCleanReadsStayCleanTests.cs` | `CwAccuracy` | no |
| engine | `Cw/TheFirstSecondsAreReadAgainTests.cs` | `CwProbabilisticStream.ReReads` | no |
| engine | `Cw/ThePeakAgainstASecondSignalTests.cs` | `CwSpectralPeak` | no |
| engine | `Cw/ThePeakFindsThePitchTheTrackerMissedTests.cs` | `CwSpectralPeak` | no |
| engine | `Cw/ThePosteriorSurvivesItsOwnArithmeticTests.cs` | `CwProbabilisticDecoder.Posterior`, `LogSum` | no |
| engine | `Cw/TheProbabilisticDecoderTests.cs` | `CwAccuracy` | no |
| engine | `Cw/TheQuietestBinNoLongerWinsTests.cs` | `CwPitchRanking`, `CwDecoder.RankThePitch` | no |
| engine | `Cw/TheReferenceDecoderIsPortedFaithfullyTests.cs` | `CwReferenceDecoder` | no |
| engine | `Cw/TheScoreSaysWhatItIsMeasuringTests.cs` | `CwAccuracy` | no |
| engine | `Cw/WhatDecodeScoringCostsTests.cs` | `CwToneTracker.CoarseSpacingHz` | no |
| engine | `Cw/WhereHamletAndTheReferenceDivergeTests.cs` | `CwProbabilisticResult.Elements`, 5-argument `Decode` | no |
| app | `Views/ThePitchControlsAreOffThePanelTests.cs` | `CwDecoder.AssertAt`, `CwDecoder.PitchWasAsserted` | no |

`TheCleanReadsStayCleanTests.cs` and `TheProbabilisticDecoderTests.cs` were excluded from
section 3's grep, which found `CwAccuracy` in both, not from a compiler error: the compiler
stops reporting binding errors in a file once its declarations fail, and build 2 had not yet
reached them. `TheProbabilisticDecoderTests.cs` exists at 7e209cb4; the name that excludes it
came with its 43-line change since. Build 4 and the non-incremental rebuild after it, with all 22
removed, were clean on both test projects. **Each is excluded, not retired: step 3
retires or repairs each under R49 with the quoted name as its evidence.**

Engine tests that task 2's build 2 raised and that were **not** excluded, because a seam in
section 8 gave them their name: `Audio/NoCwDecodeInDigitalModeTests.cs` (`DigitalMode`),
`Cw/AHeldPitchDoesNotOutliveItsEvidenceTests.cs` (`Retuned`), `Cw/WhereAcquisitionPointsTests.cs`
(`CwDecodeReport.PitchChoice`), and the app test `ViewModels/TheSpanRatioReachesTheSidecarTests.cs`
(`CwCharacter.MarginLlr` settable). They compile; whether they are green is task 5's measure.

## 10. The 1.6 app hunks - `git diff 10512248 HEAD -- src/Hamlet.App`

One file, `MainWindowViewModel.cs`, 21 insertions and 43 deletions, three changes in six diff
hunks. Every other file under `src/Hamlet.App` is byte-identical to the step's entry.

| # | Where | Diff hunks | What changed | The seam it serves |
|---|---|---|---|---|
| 1 | `StartListening`, line 11080 | `@@ -11080,5 +11080,5` | `new CwDecoder(...) { UseJointCutter = _settings.UseJointDecoder }` becomes `new CwDecoder(...)`, with a comment that the build has no joint cutter | the restored decoder has no joint cutter. A shim property that accepted the switch and did nothing would be a control that silently does nothing (task 2's rule), so the hunk wins. `AppSettings.UseJointDecoder` is kept, so the settings file round-trips; it ships off (Tim, 2026-08-27) and has no control on screen |
| 2 | `ElementPitchLine`, line 12405 | `@@ -12405,8`, `@@ -12414`, `@@ -12416,3`, `@@ -12421,11` | decode with 7e209cb4's public `Decode(envelope, toneHz)`; nothing read gives *nothing was read, so no element was measured, which is too few to say anything about how they spread*; anything read gives *not measured (the decoder in this build does not say where each element began and ended, so no element's own pitch was measured)* | `CwProbabilisticResult.Elements` does not exist at 7e209cb4 (section 7). An empty list would have made the sheet say *0 elements were long enough to measure a pitch from* on audio it had read, which is a measurement nobody made (§0.0). The no-tone branch is unchanged |
| 3 | `ToneForTheRecord`, line 12487 | `@@ -12487,15 +12476,4` | the `report.Rank is { } rank` branch and its *ranked* sentence removed, with a comment | `CwDecodeReport.Rank` would need `CwPitchRank` from deleted `CwPitchRanking.cs`; this decoder never ranks, so the branch could never be taken, and re-creating the type to return null from it would be a HEAD-only type put back to make a caller build |

**What the operator's capture sheet says differently:** the element-pitch line (hunk 2), and
the ranked sentence never appears (hunk 3). With the restored decoder the sheet's margin figures
print *unmeasured* (section 8, row 2), and the *emitted without keying* line can only name the
middle of the bank or keying, never the strongest bin, the ranking or an assertion (row 3).
