# Unit 391 - the seams into src/Hamlet.RadioEngine/Cw (step 0, criterion 0.4)

Written by work instruction 391 tasks 2 and 3, 2026-09-22, from the tree at 3d6a2c12, by grep.
Scripts: .run-unit/unit391-seams.sh, unit391-members.sh, unit391-seams-at.sh (not committed).

**How the rows were found.** Every .cs and .xaml file under src/Hamlet.App and
tests/Hamlet.App.Tests from git ls-files, kept when it has a using of Hamlet.RadioEngine.Cw.
The type column is every one of the 102 type names declared under src/Hamlet.RadioEngine/Cw
that the file names as a whole word. 30 files are kept, 9 under src/Hamlet.App and 21 under tests/Hamlet.App.Tests, 145 file-and-type rows. Dropped as name-only matches: 27 files that name only
Outcome (a nested enum in CwAccuracy.cs, a common word) with no Cw using, and
AchievementsViewModel.cs, which names CwTransmitter and KeyerCwSender in a comment only.

**How the members column was found, and what it is not.** A member is listed when the file
has .Member for a Member declared public in the file that declares the type. Types declared
in the same file (CwCharacter, CwConfidence and CwReadingStage in CwCharacter.cs, for
instance) therefore show the same members. It is a grep, not a compiler: it can over-list
a member that another type in the file shares, and cannot see a member reached through var
or a lambda parameter if the member name is not written with a dot. Step 1's build is the
real check.

**At 07f0397a** (the commit named for 0.2): the type is looked for by declaration under
src/Hamlet.RadioEngine/Cw at that commit, and each member by whole word in the file that
declares the type there. Outcome rows in files that also have a Cw using are kept, but the
Outcome those files mean may be another type of that name.

| File | Type | Members touched (grep) | At 07f0397a |
|---|---|---|---|
| `src/Hamlet.App/Controls/CwTerminalControl.cs` | CwCharacter | High Low Unreadable | all there |
| `src/Hamlet.App/Controls/CwTerminalControl.cs` | CwConfidence | High Low Unreadable | all there |
| `src/Hamlet.App/Controls/CwTerminalControl.cs` | MorseAlphabet | Unreadable | all there |
| `src/Hamlet.App/Controls/ModePalette.cs` | CwConfidence | High Low Unreadable | all there |
| `src/Hamlet.App/Telemetry/AppEvents.cs` | CwDecodeReport | Clipping NearlySilent | all there |
| `src/Hamlet.App/Telemetry/AppEvents.cs` | CwReadiness | AsEvent Reason | all there |
| `src/Hamlet.App/Telemetry/AppEvents.cs` | Outcome | - | **type absent** |
| `src/Hamlet.App/Telemetry/AppEvents.cs` | TransmitChain | - | all there |
| `src/Hamlet.App/ViewModels/AutoCallViewModel.cs` | AutoCallOutcome | Answered Cause FrequencyLabel IsUsable OperatorStopped Refusal Round RoundLimit RunAsync Sentence Stop Transmitted | all there |
| `src/Hamlet.App/ViewModels/AutoCallViewModel.cs` | AutoCallSettings | Answered Cause FrequencyLabel IsUsable OperatorStopped Refusal Round RoundLimit RunAsync Sentence Stop Transmitted | all there |
| `src/Hamlet.App/ViewModels/AutoCallViewModel.cs` | AutoCallStop | Answered Cause FrequencyLabel IsUsable OperatorStopped Refusal Round RoundLimit RunAsync Sentence Stop Transmitted | all there |
| `src/Hamlet.App/ViewModels/AutoCallViewModel.cs` | AutoCallTransmission | Answered Cause FrequencyLabel IsUsable OperatorStopped Refusal Round RoundLimit RunAsync Sentence Stop Transmitted | all there |
| `src/Hamlet.App/ViewModels/AutoCallViewModel.cs` | AutoCallWindow | Empty | all there |
| `src/Hamlet.App/ViewModels/AutoCallViewModel.cs` | AutoCaller | Answered Cause FrequencyLabel IsUsable OperatorStopped Refusal Round RoundLimit RunAsync Sentence Stop Transmitted | all there |
| `src/Hamlet.App/ViewModels/AutoCallViewModel.cs` | CwCharacter | - | all there |
| `src/Hamlet.App/ViewModels/AutoCallViewModel.cs` | CwDecoder | CharacterSettled Tracker | all there |
| `src/Hamlet.App/ViewModels/AutoCallViewModel.cs` | CwMessage | Clean | all there |
| `src/Hamlet.App/ViewModels/AutoCallViewModel.cs` | KeyerCwSender | - | all there |
| `src/Hamlet.App/ViewModels/AutoCallViewModel.cs` | TransmitReadiness | - | all there |
| `src/Hamlet.App/ViewModels/CwTranscript.cs` | CwCharacter | IsUnstable Unstable | all there |
| `src/Hamlet.App/ViewModels/CwTranscript.cs` | CwReadingStage | IsUnstable Unstable | all there |
| `src/Hamlet.App/ViewModels/CwTransmitViewModel.cs` | ContactScript | Answering Calling Confirming Exchanging Offer Pieces | all there |
| `src/Hamlet.App/ViewModels/CwTransmitViewModel.cs` | ContactStage | Answering Calling Confirming Exchanging Offer Pieces | all there |
| `src/Hamlet.App/ViewModels/CwTransmitViewModel.cs` | CwDuration | Of | all there |
| `src/Hamlet.App/ViewModels/CwTransmitViewModel.cs` | CwMessage | Clean MaximumLength PieceCount | all there |
| `src/Hamlet.App/ViewModels/CwTransmitViewModel.cs` | CwReadiness | Check Outcome Ready | all there |
| `src/Hamlet.App/ViewModels/CwTransmitViewModel.cs` | CwReadyState | Check Outcome Ready | all there |
| `src/Hamlet.App/ViewModels/CwTransmitViewModel.cs` | CwTransmitter | Abort Check SendAsync SupportsCharacterSpacing | all there |
| `src/Hamlet.App/ViewModels/CwTransmitViewModel.cs` | Outcome | - | **type absent** |
| `src/Hamlet.App/ViewModels/CwTransmitViewModel.cs` | SendOption | Answering Calling Confirming Exchanging Offer Pieces | all there |
| `src/Hamlet.App/ViewModels/CwTransmitViewModel.cs` | SwrReport | Citation Describe For IsHigh | all there |
| `src/Hamlet.App/ViewModels/CwTransmitViewModel.cs` | TransmissionEnd | Begin Elapsed Expected Keyed Message Observe Outcome Progress Remaining Stop Stopped | all there |
| `src/Hamlet.App/ViewModels/CwTransmitViewModel.cs` | TransmissionWatch | Begin Elapsed Expected Keyed Message Observe Outcome Progress Remaining Stop Stopped | all there |
| `src/Hamlet.App/ViewModels/CwTransmitViewModel.cs` | TransmitChain | Describe | all there |
| `src/Hamlet.App/ViewModels/CwTransmitViewModel.cs` | TransmitContext | Abort Check SendAsync SupportsCharacterSpacing | all there |
| `src/Hamlet.App/ViewModels/CwTransmitViewModel.cs` | TransmitEvidence | Describe | all there |
| `src/Hamlet.App/ViewModels/CwTransmitViewModel.cs` | TransmitNotes | Citation Describe For IsHigh | all there |
| `src/Hamlet.App/ViewModels/CwTransmitViewModel.cs` | TransmitOutcome | Abort Check SendAsync SupportsCharacterSpacing | all there |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | CallsignResolver | From Sender StationHeard | all there |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | ContactStage | Calling | all there |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | CwCase | Append FileName NoRecording Readable Recording Row Session | all there |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | CwCaseRoster | Append FileName NoRecording Readable Recording Row Session | all there |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | CwCharacter | IsWordGap MarginLlr MarginShareForRecord Settled SpanLogLikelihoodRatio Stage WidestRecordedLlr | type there; **missing: MarginLlr MarginShareForRecord SpanLogLikelihoodRatio WidestRecordedLlr** |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | CwCounterDelta | At Count Note Over | all there |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | CwCounterSample | At Count Note Over | all there |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | CwCounterTrail | At Count Note Over | all there |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | CwCountsCover | Append FileName NoRecording Readable Recording Row Session | all there |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | CwDecodeReport | Clipping Describe NearlySilent None Summarize | all there |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | CwDecodeStory | Clipping Describe NearlySilent None Summarize | all there |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | CwDecoder | AssertStation CharacterDecoded CharacterSettled DecodeQueueDroppedChunks DecodeQueueDroppedSamples DecodingSuspended DigitalMode IsLocked LeadingEdge Listen ListeningAfresh Lock LockedToneHz PitchWasAsserted RadioIsTransmitting Ranked Reading Report Retuned SampleRate SpeedIsReacquiring Tap Unlock WordsPerMinute | type there; **missing: AssertStation DecodeQueueDroppedChunks DecodeQueueDroppedSamples DecodingSuspended DigitalMode IsLocked LeadingEdge ListeningAfresh Lock LockedToneHz PitchWasAsserted RadioIsTransmitting Ranked Reading Retuned Unlock** |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | CwElementPitch | Measure MeasureAll | **type absent** |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | CwKeyingMeter | Keying Listening NoKeying None Reading Reset Update Window | all there |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | CwMessage | PieceCount Split | all there |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | CwPitchChoice | Keying OperatorAssertion Ranked StrongestBin | **type absent** |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | CwProbabilisticDecoder | - | **type absent** |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | CwProbabilisticStream | CharacterSettled SamplesSeen ToneHz WindowSeconds | **type absent** |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | CwReadiness | Check ListenOnly Outcome Ready Reason TransmitReadiness | all there |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | CwStreamSplit | Divide LeastTrustedMarks None | **type absent** |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | CwTransmitter | Abort Check | all there |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | Envelope | Decode None Run Runs Text WordsPerMinute | **type absent** |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | KeyerCwSender | Abort IsSending | all there |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | KeyingEnvelope | Measure Score | all there |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | KeyingReading | Keying Listening NoKeying None Reading Reset Update Window | all there |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | KeyingVerdict | Keying Listening NoKeying None Reading Reset Update Window | all there |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | Outcome | Block None Score | **type absent** |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | SendOption | Calling | all there |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | TransmissionEnd | Expected IsSending Keyed Message Observe Outcome Progress Stop Stopped | all there |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | TransmitChain | BrokeAt Describe | all there |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | TransmitContext | Abort Check | all there |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | TransmitEvidence | BrokeAt Describe TransmitChain | all there |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | TransmitOutcome | Abort Check | all there |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | TransmitReadiness | Check ListenOnly Outcome Ready Reason | all there |
| `src/Hamlet.App/ViewModels/PhrasebookViewModel.cs` | CwPhrase | Heading NewOperator NewOperatorNote OfKind Summary | all there |
| `src/Hamlet.App/ViewModels/PhrasebookViewModel.cs` | CwPhrasebook | Heading NewOperator NewOperatorNote OfKind Summary | all there |
| `src/Hamlet.App/ViewModels/PhrasebookViewModel.cs` | PhraseKind | Heading NewOperator NewOperatorNote OfKind Summary | all there |
| `src/Hamlet.App/ViewModels/ScanViewModel.cs` | CwCharacter | - | all there |
| `src/Hamlet.App/ViewModels/ScanViewModel.cs` | CwDecoder | CharacterSettled Ranked | type there; **missing: Ranked** |
| `tests/Hamlet.App.Tests/Cw/TheSheetSaysWhatEachElementWasSentAtTests.cs` | CwDecodeReport | - | all there |
| `tests/Hamlet.App.Tests/Telemetry/ARefusedPressLeavesALineTests.cs` | CwDecoder | Tap | all there |
| `tests/Hamlet.App.Tests/Telemetry/CallsignPrivacyTests.cs` | CwConfidence | High | all there |
| `tests/Hamlet.App.Tests/Telemetry/CallsignPrivacyTests.cs` | CwDecodeReport | - | all there |
| `tests/Hamlet.App.Tests/Telemetry/CallsignPrivacyTests.cs` | Outcome | - | **type absent** |
| `tests/Hamlet.App.Tests/Telemetry/CallsignPrivacyTests.cs` | TransmitChain | - | all there |
| `tests/Hamlet.App.Tests/Telemetry/CallsignPrivacyTests.cs` | TransmitReadiness | Check Outcome | all there |
| `tests/Hamlet.App.Tests/Telemetry/DecisionEmissionTests.cs` | CwMessage | MaximumLength | all there |
| `tests/Hamlet.App.Tests/Telemetry/DecisionEmissionTests.cs` | CwReadiness | BreakInOff ModeUnknown Outcome Ready Reason | all there |
| `tests/Hamlet.App.Tests/Telemetry/DecisionEmissionTests.cs` | CwReadyState | BreakInOff ModeUnknown Outcome Ready Reason | all there |
| `tests/Hamlet.App.Tests/Telemetry/DecisionEmissionTests.cs` | CwSendOutcome | Refused Sent | all there |
| `tests/Hamlet.App.Tests/Telemetry/DecisionEmissionTests.cs` | CwSendResult | Refused Sent | all there |
| `tests/Hamlet.App.Tests/Telemetry/DecisionEmissionTests.cs` | CwTransmitter | - | all there |
| `tests/Hamlet.App.Tests/Telemetry/DecisionEmissionTests.cs` | ICwSender | Refused Sent | all there |
| `tests/Hamlet.App.Tests/Telemetry/DecisionEmissionTests.cs` | Outcome | - | **type absent** |
| `tests/Hamlet.App.Tests/Telemetry/DecisionEmissionTests.cs` | TransmitContext | - | all there |
| `tests/Hamlet.App.Tests/Telemetry/ThePressActuallyWritesItsCaptureTests.cs` | CwDecoder | SampleRate Tap | all there |
| `tests/Hamlet.App.Tests/Telemetry/Unit305SettledTests.cs` | CwReadyState | Check NotInMorse Reason | all there |
| `tests/Hamlet.App.Tests/Telemetry/Unit305SettledTests.cs` | TransmitReadiness | Check NotInMorse Reason | all there |
| `tests/Hamlet.App.Tests/ViewModels/AHeldVerdictPrintsNoMeasurementsTests.cs` | KeyingReading | Keying None | all there |
| `tests/Hamlet.App.Tests/ViewModels/AHeldVerdictPrintsNoMeasurementsTests.cs` | KeyingVerdict | Keying None | all there |
| `tests/Hamlet.App.Tests/ViewModels/ASheetSaysWhichInstrumentSpokeTests.cs` | CwCase | Header Recording Row | all there |
| `tests/Hamlet.App.Tests/ViewModels/ASheetSaysWhichInstrumentSpokeTests.cs` | CwCaseRoster | Header Recording Row | all there |
| `tests/Hamlet.App.Tests/ViewModels/ASheetSaysWhichInstrumentSpokeTests.cs` | CwCountsCover | Header Recording Row | all there |
| `tests/Hamlet.App.Tests/ViewModels/AutoCallFaceTests.cs` | CwMessage | Clean | all there |
| `tests/Hamlet.App.Tests/ViewModels/CaseRosterSurvivesAnEveningTests.cs` | CwCase | Append Header NoRecording Readable Recording Row | all there |
| `tests/Hamlet.App.Tests/ViewModels/CaseRosterSurvivesAnEveningTests.cs` | CwCaseRoster | Append Header NoRecording Readable Recording Row | all there |
| `tests/Hamlet.App.Tests/ViewModels/CaseRosterSurvivesAnEveningTests.cs` | CwCharacter | - | all there |
| `tests/Hamlet.App.Tests/ViewModels/CaseRosterSurvivesAnEveningTests.cs` | CwCountsCover | Append Header NoRecording Readable Recording Row | all there |
| `tests/Hamlet.App.Tests/ViewModels/CaseRosterSurvivesAnEveningTests.cs` | CwDecoder | CharacterSettled Flush Listen Report SampleRate Tap WordsPerMinute | all there |
| `tests/Hamlet.App.Tests/ViewModels/DummyLoadNoticeTests.cs` | TransmitNotes | For | all there |
| `tests/Hamlet.App.Tests/ViewModels/ScannerFaceTests.cs` | CwCharacter | High Low | all there |
| `tests/Hamlet.App.Tests/ViewModels/ScannerFaceTests.cs` | CwConfidence | High Low | all there |
| `tests/Hamlet.App.Tests/ViewModels/ScannerFaceTests.cs` | MorseAlphabet | WordGap | all there |
| `tests/Hamlet.App.Tests/ViewModels/SendButtonEnablementTests.cs` | CwMessage | MaximumLength | all there |
| `tests/Hamlet.App.Tests/ViewModels/SendButtonEnablementTests.cs` | CwReadiness | Ready Reason | all there |
| `tests/Hamlet.App.Tests/ViewModels/SendButtonEnablementTests.cs` | CwReadyState | Ready Reason | all there |
| `tests/Hamlet.App.Tests/ViewModels/SendButtonEnablementTests.cs` | CwSendOutcome | Sent | all there |
| `tests/Hamlet.App.Tests/ViewModels/SendButtonEnablementTests.cs` | CwSendResult | Sent | all there |
| `tests/Hamlet.App.Tests/ViewModels/SendButtonEnablementTests.cs` | CwTransmitter | - | all there |
| `tests/Hamlet.App.Tests/ViewModels/SendButtonEnablementTests.cs` | ICwSender | Sent | all there |
| `tests/Hamlet.App.Tests/ViewModels/SendButtonEnablementTests.cs` | TransmitContext | - | all there |
| `tests/Hamlet.App.Tests/ViewModels/SendGuardTests.cs` | ContactStage | Calling | all there |
| `tests/Hamlet.App.Tests/ViewModels/SendGuardTests.cs` | CwDuration | DefaultWpm Dit Of | all there |
| `tests/Hamlet.App.Tests/ViewModels/SendGuardTests.cs` | CwMessage | MaximumLength | all there |
| `tests/Hamlet.App.Tests/ViewModels/SendGuardTests.cs` | CwSendOutcome | Refused Sent | all there |
| `tests/Hamlet.App.Tests/ViewModels/SendGuardTests.cs` | CwSendResult | Refused Sent | all there |
| `tests/Hamlet.App.Tests/ViewModels/SendGuardTests.cs` | CwTransmitter | - | all there |
| `tests/Hamlet.App.Tests/ViewModels/SendGuardTests.cs` | ICwSender | Refused Sent | all there |
| `tests/Hamlet.App.Tests/ViewModels/SendGuardTests.cs` | SendOption | Calling | all there |
| `tests/Hamlet.App.Tests/ViewModels/SendGuardTests.cs` | TransmitContext | - | all there |
| `tests/Hamlet.App.Tests/ViewModels/SendLengthIsLegibleTests.cs` | ContactStage | Calling | all there |
| `tests/Hamlet.App.Tests/ViewModels/SendLengthIsLegibleTests.cs` | CwDuration | Of | all there |
| `tests/Hamlet.App.Tests/ViewModels/SendLengthIsLegibleTests.cs` | SendOption | Calling | all there |
| `tests/Hamlet.App.Tests/ViewModels/SentTextNeverEntersTheReceivedStreamTests.cs` | CwCharacter | - | all there |
| `tests/Hamlet.App.Tests/ViewModels/SentTextNeverEntersTheReceivedStreamTests.cs` | CwDecoder | CharacterSettled Flush Process | all there |
| `tests/Hamlet.App.Tests/ViewModels/SentTextNeverEntersTheReceivedStreamTests.cs` | CwMessage | - | all there |
| `tests/Hamlet.App.Tests/ViewModels/SentTextNeverEntersTheReceivedStreamTests.cs` | CwProbabilisticStream | CharacterSettled Flush Process | **type absent** |
| `tests/Hamlet.App.Tests/ViewModels/TheSpanRatioReachesTheSidecarTests.cs` | CwCharacter | High | all there |
| `tests/Hamlet.App.Tests/ViewModels/TheSpanRatioReachesTheSidecarTests.cs` | CwConfidence | High | all there |
| `tests/Hamlet.App.Tests/ViewModels/TheSpanRatioReachesTheSidecarTests.cs` | MorseAlphabet | WordGap | all there |
| `tests/Hamlet.App.Tests/ViewModels/TwoStageTranscriptTests.cs` | CwCharacter | High IsUnstable Provisional Settled Unstable | all there |
| `tests/Hamlet.App.Tests/ViewModels/TwoStageTranscriptTests.cs` | CwConfidence | High IsUnstable Provisional Settled Unstable | all there |
| `tests/Hamlet.App.Tests/ViewModels/TwoStageTranscriptTests.cs` | CwReadingStage | High IsUnstable Provisional Settled Unstable | all there |
| `tests/Hamlet.App.Tests/ViewModels/UnresolvedLicenseTests.cs` | TransmitContext | Check | all there |
| `tests/Hamlet.App.Tests/Views/HistoryRecedesAndCurrentCopyDoesNotTests.cs` | CwCharacter | High Low Unreadable | all there |
| `tests/Hamlet.App.Tests/Views/HistoryRecedesAndCurrentCopyDoesNotTests.cs` | CwConfidence | High Low Unreadable | all there |
| `tests/Hamlet.App.Tests/Views/ThePitchControlsAreOffThePanelTests.cs` | CwDecoder | AssertAt AssertStation LockedToneHz PitchWasAsserted Unlock | type there; **missing: AssertAt AssertStation LockedToneHz PitchWasAsserted Unlock** |
