# Unit 398 - the excluded files, classified and retired under R49

Step 3 of *CW decodes again*. Every result here is an indication (FACT-004); this machine has no
radio (FACT-006). *Green* means the assertion held and nothing more.

## 1. Entry

Decision 4's diff, run in `.run-unit/unit398-verify.sh`:

```
$ git diff --stat 3af36501 HEAD -- src tests docs/carry-forward-tests.txt
(nothing printed)
```

Empty, so decision 4 applies: the carry-forward lines at entry are unit 397's exit runs -
**app 278 of 278 in 167 s, engine 176 of 176 in 374 s** (unit 397 report section 3).

Floors at entry, HEAD `7e2abb7e` plus the version bump, one `dotnet build Hamlet.sln -warnaserror`
(exit 0, 14 s), then `--no-build`, one type per invocation:

| Type | Result | Wall |
|---|---|---|
| `TheCapturesThatDecodeKeepDecodingTests` | 37 of 37 green | 94 s |
| `TheAdjudicatedReadingsKeepReadingTests` | 13 of 13 green | 29 s |
| `CwFixtureTests.TheCleanRecordingsDecodeExactly` | 0 of 2, red under R53 | 3 s |

Every capture row is identical to unit 397's exit table (`unit398-cmp.sh` against
`unit397-floors-exit-1.txt`: 37 rows each side, `diff` rc 0). The two synthetics' texts:
`clean-12wpm` `■ ■ ■ ■ ■  ■ ■ ■ ■■`, `clean-18wpm` `■ ■ ■  ■■■`, as unit 397's exit.

The eleven transmit files printed nothing against `7e209cb4`; `git diff --stat 5688a8a5 HEAD -- src`
printed nothing.

## 2. The classification

Every name below was grepped over `src/Hamlet.RadioEngine/Cw` at HEAD `305bec69` by
`.run-unit/unit398-grep.sh`, `unit398-grep2.sh` and `unit398-grep3.sh`, `grep -rnw <name>`;
what grep printed is quoted. *nothing* means grep printed no line at all. Names in doc-comment
prose are not counted (unit 394's finding). *Audio* is decision 2's: a `.wav`,
`CwSignal.Generate`, `CwFixtureGenerator.Generate`, or a source pumped into a decoder.

**The grep, for every name a verdict rests on:**

| Name | What grep printed under `Cw` |
|---|---|
| `CwAccuracy` | nothing |
| `CwSpectralPeak` | nothing |
| `CwSwingSurvey` | nothing |
| `CwStreamSplit` | nothing |
| `CwElementPitch` | nothing |
| `CwElement`, `IsMark`, `PitchHz` | nothing |
| `CwPitchRanking` | nothing |
| `CwReferenceDecoder` | nothing |
| `LogSum` | nothing |
| `Posterior` | nothing |
| `FittedLogLikelihoods` | nothing |
| `ReReads` | nothing |
| `WindowSizings` | nothing |
| `ProcessDelayForTests` | nothing |
| `RankThePitch`, `Rankings`, `Rank` | nothing |
| `Ranked` | `CwPitchChoice.cs:63: Ranked,` - an enum member only; no `CwDecoder.Ranked` |
| `AssertAt`, `AssertStation` | nothing |
| `PitchWasAsserted` | `CwDecodeReport.cs:72: public bool PitchWasAsserted => false;` - on the report only; nothing in `CwDecoder.cs` |
| `CoarseSpacingHz` | `CwToneTracker.cs:131: private const double CoarseSpacingHz = 25;` - **private at HEAD** |
| `Elements` on `CwProbabilisticResult` | the record is `CwProbabilisticDecoder.cs:35`, parameters `LikelihoodRatio, WordsPerMinute, Text, ToneHz, Characters, EndsInsideCharacter`; no `Elements` |
| 5-argument `Decode` | `CwProbabilisticDecoder.cs:706: private static CwProbabilisticResult Decode(` with `bool ungated` - **private at HEAD** |
| `CwUnitEstimator.Elements` with `out dropped` | `CwUnitEstimator.cs:334: public static (IReadOnlyList<double> Marks, IReadOnlyList<double> Gaps) Elements(` - no `out` overload |
| `PitchChoice` as a `CwDecodeReport` constructor parameter | the record's parameters, `CwDecodeReport.cs:47` to `61`, end at `bool PitchWasMeasured = false)`; `PitchChoice` is `CwDecodeReport.cs:83: public CwPitchChoice PitchChoice`, a property, not a parameter |
| present, for the re-include verdicts | `Reading` `CwDecoder.cs:209`; `Tap` `:232`; `Listen` `:463`; `Process` `:493`; `Flush` `:605`; `Retuned` `:385`; `Report` `:242`; `Tracker` `:212`; `HopSamples` `CwToneTracker.cs:392`; `Envelope` `CwProbabilisticDecoder.cs:807`; `LogLikelihoods` `:901`; `Gate` `:214`; `CharacterMargin` `:302`; `HopMilliseconds` `:427`; `SlowestWpm` `:438`; `FastestWpm` `:459`; `None` `:60`; `DecodeUngated` `:683`; `ReadEverySeconds` `CwProbabilisticStream.cs:34`; `WindowSeconds` `:31`; `CharacterSettled` `:225`; `LeadingEdgeChanged` `:235`; `Last` `:185`; `MorseAlphabet.Unreadable` `MorseAlphabet.cs:43`; `CwPitchChoice.StrongestBin` `CwPitchChoice.cs:44`; `ReusableWindow.Sizings`, `AudioTap.ArrivalRatio`, `BufferedAudioSource.PumpAll` under `src/Hamlet.RadioEngine/Audio` |

**Two of unit 392's quoted names are prose only.** `TheCleanReadsStayCleanTests.cs` and
`TheProbabilisticDecoderTests.cs` were excluded on `CwAccuracy`; in both files `CwAccuracy` appears
only inside `///` doc comments, and neither file's code names anything grep printed as absent.
Both are therefore **re-include whole**, the same finding unit 394 made for #1.

**Verdicts.** *retired* - names an absent or private name and asserts none of the four from audio.
*stays red-open* - asserts characters, elements, a tone or a speed, on audio or, where the
buffer is a hand-built sine or noise, in doubt, which decision 2 resolves toward staying.
*re-include* - names nothing absent. In a file that stays whole, a fact that would be retired is
*retirable, left with the file* and a fact that names nothing absent is *names nothing absent,
held with the file* (decision 8).

| # | File | Fact | Absent at HEAD, as grep printed | Audio | Asserts | Verdict |
|---|---|---|---|---|---|---|
| 1 | `Audio/TheReadPathDoesNotAllocateTests` | `ARepeatingReaderAllocatesNothingAfterTheFirstCall` | none | no | bytes allocated, `ReusableWindow.Sizings` | re-include |
| 2 | | `AFixedSpanReadRepeatedlyAllocatesNothing` | none | no | bytes allocated, sizings | re-include |
| 3 | | `TheKeyingMeterSizesItsWindowOnceAndReusesIt` | `CwKeyingMeter.WindowSizings` - nothing | no | a sizing count | **retired** |
| 4 | | `ReadingTheArrivalRatioAllocatesNothing` | none | no | bytes allocated | re-include |
| 5 | `Audio/TheTapIsNotBehindTheDecoderTests` | `TheTapIsWholeWhileTheDecoderCrawls` | `CwDecoder.ProcessDelayForTests` - nothing | zeros pumped | tap sample count, callback time | **retired** |
| 6 | | `AFullQueueDropsAndCounts` | `CwDecoder.ProcessDelayForTests` - nothing | zeros pumped | tap sample count, drop counts | **retired** |
| 7 | | `TheTapIsFedOnceWhicheverWayTheAudioArrives` | none | zeros pumped | tap sample count | re-include |
| 8 | `Cw/ABlipDoesNotShiftEverythingAfterItTests` - the set's #1 | `ASubMinimumBlipInAGapChangesNothingAfterIt` | none | `CwSignal.Generate` | text equal | re-include |
| 9 | | `ThreeBlipsChangeNothingEither` | none | yes | text equal | re-include |
| 10 | | `ADitLongInjectionIsNoticed` | none | yes | text differs | re-include |
| 11 | `Cw/AMoveStartsTheDecoderFreshTests` | `NothingTheDecoderLearnedSurvivesTheMove` | none | `CwFixtureGenerator` | characters emitted | names nothing absent, held with the file |
| 12 | | `ARetunedDecoderMatchesOneThatHasNeverListened` | `CwDecoder.PitchWasAsserted`, `CwDecoder.Ranked` - not on `CwDecoder` | yes | character and element counts | stays red-open |
| 13 | | `TheDecoderReadsTheNewStationAfterTheMove` | none | yes | characters emitted | names nothing absent, held with the file |
| 14 | `Cw/AStationIsABinThatSwingsTests` | `TheRefusedCqIsFoundNearFiveEightyThree` | `CwSwingSurvey` - nothing | `.wav` | a hertz | stays red-open |
| 15 | | `TheNoisePickNoLongerWins` | `CwSwingSurvey` | `.wav` | a hertz | stays red-open |
| 16 | | `TheMarginBetweenSilenceAndAStation` | `CwSwingSurvey` | `.wav` | swing in dB | retirable, left with the file |
| 17 | | `EveryCaptureOfTheEveningRanks`, 7 cases | `CwSwingSurvey` | `.wav` | a candidate pitch exists; doubt | stays red-open |
| 18 | `Cw/EveryElementCarriesItsOwnPitchTests` | `ADahResolvesToAboutFiveHertz`, 4 cases | `CwElementPitch` - nothing | hand-built sine; doubt | a hertz | stays red-open |
| 19 | | `ADitResolvesToAboutEighteenHertz`, 3 cases | `CwElementPitch` | sine; doubt | a hertz | stays red-open |
| 20 | | `TooShortToMeasureSaysSoRatherThanGuessing` | `CwElementPitch` | sine; doubt | no hertz | stays red-open |
| 21 | | `AGapIsNotMeasured` | `CwElementPitch`, `CwElement` - nothing | sine; doubt | a hertz per element | stays red-open |
| 22 | | `TwoSendersThirteenHertzApartAreSeparableOnTheirDahs` | `CwElementPitch` | sine; doubt | hertz apart | stays red-open |
| 23 | | `TheElementStreamComesOutWithTheText` | `CwProbabilisticResult.Elements`, private 5-arg `Decode` | `clean-18wpm.wav` | elements against patterns | stays red-open |
| 24 | `Cw/FittingKeyUpAgainstAssumingItTests` | `WhatFittingKeyUpDoesToEveryRecording` | `CwProbabilisticDecoder.FittedLogLikelihoods` - nothing | `.wav` | `Cases.Length` is 9 | **retired** |
| 25 | `Cw/IsTheHertzABiasOrAFloorTests` | `TheErrorAcrossCarriersSpeedsAndDuties` | `CwSpectralPeak` - nothing | `CwSignal.Generate` | a pitch was found | stays red-open |
| 26 | | `AShortBurstInALongRecordingIsFoundBetterOverTheLoudestStretch` | `CwSpectralPeak` | yes | a pitch was found | stays red-open |
| 27 | | `TheCarrierThatRetiredN4L` | `CwSpectralPeak` | yes | a pitch was found | stays red-open |
| 28 | `Cw/NoSenderIsSplitInTwoTests` | `ASenderHamletAlreadyReadsIsNeverDivided`, 4 cases | `CwStreamSplit`, `CwElementPitch`, `CwProbabilisticResult.Elements` | `.wav` | a sender not divided by element pitch; doubt | stays red-open |
| 29 | | `TheTwoSenderCaptureIsNotYetDividedEither` | same | `.wav` | hertz apart above 5 | stays red-open |
| 30 | | `TooFewLongMarksIsReportedAndNotResolved` | `CwStreamSplit`, `CwElement` | no | a division refused | retirable, left with the file |
| 31 | `Cw/NothingActsOnTheAdmissionVerdictTests` | `TheDecoderExposesTheVerdictAndTheEmitPathDoesNotConsultIt` | none - the 8-argument constructor matches `CwDecodeReport.cs:47`; `PitchChoice` read as the property | no | `PitchWasMeasured` false, `Gate` 1.40, `CharacterMargin` 1.0 | re-include |
| 32 | | `AnUnmeasuredPitchIsStillReportedAndSaysSo` | `PitchChoice` as a constructor parameter - not in `CwDecodeReport.cs:47` to `61` | **no** - a hand-built report with the literal `CharactersEmitted: 61` | a literal it set | **retired** |
| 33 | `Cw/TheCleanReadsStayCleanTests` | `EachCleanCaptureStillContainsItsTruth`, 3 cases | none - `CwAccuracy` in prose only | `.wav` | text | re-include |
| 34 | | `EachCleanCaptureStillNamesAsManyCharacters`, 3 cases | none | `.wav` | named characters | re-include |
| 35 | | `EveryFloorWasMeasuredAndNotHopedFor` | none | `.wav` | named characters | re-include |
| 36 | `Cw/TheFirstSecondsAreReadAgainTests` | `AnEmptyBandIsNeverReadAgain`, 4 cases | `CwProbabilisticStream.ReReads` - nothing | `.wav` | re-reads and characters emitted | stays red-open |
| 37 | | `TheReplayFiresTheSameWhateverTheBufferSize` | `ReReads` | `.wav` | re-read count equal | retirable, left with the file |
| 38 | | `NothingIsSaidTwice` | `ReReads` | `.wav` | settled characters in order | stays red-open |
| 39 | | `TheCallsignTheReReadRecovers` | none | `.wav` | text | names nothing absent, held with the file |
| 40 | `Cw/ThePeakAgainstASecondSignalTests` | `WhereThePeakSwitchesFromOneStationToTheOther` | `CwSpectralPeak` | `.wav` | `Assert.True(true)` | retirable, left with the file |
| 41 | | `WhetherThePeakWalksBetweenThemWithinOneRecording` | `CwSpectralPeak` | `.wav` | pitches were found | stays red-open |
| 42 | | `WhatTheOldTrackerDoesOnTheSameMix` | none | `.wav` | no assertion | names nothing absent, held with the file |
| 43 | `Cw/ThePeakFindsThePitchTheTrackerMissedTests` | `AGeneratedToneIsFoundToWithinAHertz`, 4 cases | `CwSpectralPeak` | `CwSignal.Generate` | a hertz | stays red-open |
| 44 | | `ThePeakAgreesWithTheKeyedBin`, 6 cases | `CwSpectralPeak` | `.wav` | a hertz | stays red-open |
| 45 | | `TooLittleAudioReturnsNothing` | `CwSpectralPeak` | no - a short zero buffer | no peak | retirable, left with the file |
| 46 | | `NoiseHasAPeakToo` | `CwSpectralPeak` | random noise; doubt | a pitch was found | stays red-open |
| 47 | `Cw/ThePosteriorSurvivesItsOwnArithmeticTests` | `LogSumStaysFinite`, 6 cases | `CwProbabilisticDecoder.LogSum` - nothing | no | finite, not NaN | **retired** |
| 48 | | `TwoEqualTermsDoubleTheEvidence` | `LogSum` | no | arithmetic | **retired** |
| 49 | | `NegativeInfinityIsTheIdentity` | `LogSum` | no | arithmetic | **retired** |
| 50 | | `EveryPosteriorIsAProbability`, 3 cases | `CwProbabilisticDecoder.Posterior` - nothing | `.wav` | posterior in 0 to 1, not all zero | **retired** |
| 51 | | `DigitalSilenceProducesNothingRatherThanANumber` | `Posterior` | no | posterior in 0 to 1 | **retired** |
| 52 | | `NoHopsProduceNoPosterior` | `Posterior` | no | null | **retired** |
| 53 | `Cw/TheProbabilisticDecoderTests` | `ItReadsWhatTheReferenceReads` | none - `CwAccuracy` in prose only | `.wav` | text and a speed | re-include |
| 54 | | `TheSpeedIsFoundAndNotTold`, 4 cases | none | `.wav` | text and a speed | re-include |
| 55 | | `ARecordingWithNoStationInItSaysNothing`, 2 cases | none | `.wav` | no characters | re-include |
| 56 | | `TheGateSitsInAWideGap` | none | `.wav` | likelihood ratios | re-include |
| 57 | | `ItKeepsUpWithLiveAudio` | none | `.wav` | text | re-include |
| 58 | | `NothingIsSettledFromAnEmptyBandLive`, 2 cases | none | `.wav` | no characters | re-include |
| 59 | `Cw/TheQuietestBinNoLongerWinsTests` | `AnEmptyBinScoresNearNothing` | `CwPitchRanking` - nothing | `CwFixtureGenerator` | which bin scores; doubt | stays red-open |
| 60 | | `TheStationWinsTheBand` | `CwPitchRanking`, private `CoarseSpacingHz` | yes | a hertz | stays red-open |
| 61 | | `OnRealAudioTheBareScorePicksAnEmptyBin` | same | `.wav` | a hertz | stays red-open |
| 62 | | `TheRankingDoesNotYetDriveTheDecode` | `CwDecoder.RankThePitch` - nothing | no | a switch is off | retirable, left with the file |
| 63 | | `WithTheRankingOffTheSheetReportsTheTrackersPitch` | `CwDecoder.Rankings`, `CwDecoder.Ranked`, `CwDecodeReport.Rank` - nothing | yes | ranking counters | retirable, left with the file |
| 64 | | `NothingIsRankedFromTooLittleAudio` | `CwPitchRanking` | yes | no pitch ranked; doubt | stays red-open |
| 65 | | `DigitalSilenceIsNotRanked` | `CwPitchRanking` | no - zeros | not ranked | retirable, left with the file |
| 66 | | `TheFloorIsAddedInPower` | `CwPitchRanking` | no | arithmetic | retirable, left with the file |
| 67 | | `TheCandidatesAreTheTrackersOwnBins` | `CwPitchRanking`, private `CoarseSpacingHz` | no | the candidate grid | retirable, left with the file |
| 68 | `Cw/TheReferenceDecoderIsPortedFaithfullyTests` | `AcquisitionFindsTheNetAndTheTrackerRefinesIt` | `CwReferenceDecoder` - nothing | `.wav` | a hertz | stays red-open |
| 69 | | `TheNetReadsExactlyAsTheReferenceReadsIt` | `CwReferenceDecoder` | `.wav` | text, dit and dah lengths | stays red-open |
| 70 | | `NoClockFitsNoise` | `CwReferenceDecoder` | no - numbers | no clock | retirable, left with the file |
| 71 | | `EightMarksAreNeededBeforeAClockIsFittedAtAll` | `CwReferenceDecoder` | no | no clock | retirable, left with the file |
| 72 | | `ATextbookFistFitsAndAnImpossibleSpeedDoesNot` | `CwReferenceDecoder` | no - numbers | a fitted dit from numbers | retirable, left with the file |
| 73 | | `AHeavyFistIsAdmittedByScatterWhereTheRatioBandRefusesIt` | `CwReferenceDecoder` | no | a clock fits | retirable, left with the file |
| 74 | | `TheGateRefusesAWindowWithTooLittleContrast` | `CwReferenceDecoder` | no - arrays | keyed hops | retirable, left with the file |
| 75 | | `TheSilenceControlsBehaveAsMeasured`, 2 cases | `CwReferenceDecoder` | `.wav` | a character count | stays red-open |
| 76 | | `AnAllZeroBufferIsRefused` | `CwReferenceDecoder` | zeros into a decoder | no characters | stays red-open |
| 77 | `Cw/TheScoreSaysWhatItIsMeasuringTests` | `APerfectReadScoresOne` | `CwAccuracy` - nothing | no | a score of two strings | **retired** |
| 78 | | `ABlockIsADeletionAndNotASubstitution` | `CwAccuracy` | no | same | **retired** |
| 79 | | `RefusingEverythingYieldsNothing` | `CwAccuracy` | no | same | **retired** |
| 80 | | `OnlyTheSpanWithTruthIsScored` | `CwAccuracy` | no | same | **retired** |
| 81 | | `InsertionsAndDeletionsAreCountedApart` | `CwAccuracy` | no | same | **retired** |
| 82 | | `NothingReadIsScoredAsEveryCharacterLost` | `CwAccuracy` | no | same | **retired** |
| 83 | | `SpacingIsNormalizedRatherThanScored` | `CwAccuracy` | no | same | **retired** |
| 84 | | `TheScoreIsDeterministic` | `CwAccuracy` | no | same | **retired** |
| 85 | `Cw/WhatDecodeScoringCostsTests` | `OneDecodeAtOnePitchCostsThis` | none | `.wav` | characters above 0 | re-include |
| 86 | | `APitchWithNothingOnItIsTheCheapCase` | none | `.wav` | three rows ran | re-include |
| 87 | | `AShortScoringWindowBreaksTheSilenceProperty` | `CwToneTracker.CoarseSpacingHz` - **private at HEAD**, `CwToneTracker.cs:131` | `.wav` | `broken.Count >= 0` | **retired** |
| 88 | `Cw/WhereHamletAndTheReferenceDivergeTests` | `TheElementStreamsAgreeOverTheCallsign` | `CwProbabilisticResult.Elements`, private 5-arg `Decode` | `.wav` | elements, mark and gap lengths | stays red-open |
| 89 | | `TheShortRunFloorDiscardsAlmostNothing`, 3 cases | `CwUnitEstimator.Elements` with `out` | `.wav` | dropped element runs; doubt | stays red-open |
| 90 | app `Views/ThePitchControlsAreOffThePanelTests` | `HoldThisPitchIsNotOnThePanel` | none | no | a screen fact | left, decision 6 |
| 91 | | `ACaptureCannotReportAnAssertedPitch` | none - `CwDecodeReport.PitchWasAsserted` is at line 72 | no | a screen fact | left, decision 6 |
| 92 | | `TheEngineStillCarriesTheCapability` | `CwDecoder.AssertAt` - nothing, `CwDecoder.PitchWasAsserted` - not on `CwDecoder` | no | a lock | left, decision 6; retirable on its letter, parked |

**Per file:**

| Verdict | Files |
|---|---|
| **delete** - every fact retired | `TheScoreSaysWhatItIsMeasuringTests` 8, `ThePosteriorSurvivesItsOwnArithmeticTests` 6, `FittingKeyUpAgainstAssumingItTests` 1 |
| **trim and re-include** | `Audio/TheReadPathDoesNotAllocateTests` 1 retired 3 kept, `Audio/TheTapIsNotBehindTheDecoderTests` 2 retired 1 kept, `NothingActsOnTheAdmissionVerdictTests` 1 retired 1 kept, `WhatDecodeScoringCostsTests` 1 retired 2 kept |
| **re-include whole** | `ABlipDoesNotShiftEverythingAfterItTests` 3, `TheCleanReadsStayCleanTests` 3, `TheProbabilisticDecoderTests` 6 |
| **stays whole** | `AMoveStartsTheDecoderFreshTests`, `AStationIsABinThatSwingsTests`, `EveryElementCarriesItsOwnPitchTests`, `IsTheHertzABiasOrAFloorTests`, `NoSenderIsSplitInTwoTests`, `TheFirstSecondsAreReadAgainTests`, `ThePeakAgainstASecondSignalTests`, `ThePeakFindsThePitchTheTrackerMissedTests`, `TheQuietestBinNoLongerWinsTests`, `TheReferenceDecoderIsPortedFaithfullyTests`, `WhereHamletAndTheReferenceDivergeTests`, and the app's `ThePitchControlsAreOffThePanelTests` under decision 6 |

**Counts.** Files 22. Facts 92, a `[Theory]` case set counted once: 89 engine, 3 app. **Retired 20**
in 7 files. **Stays red-open 31**, in 11 engine files, each asserting characters, elements, a tone
or a speed. **Re-include 19**, in 7 files. Retirable, left with the file 15, in 7 stays-whole
engine files; names nothing absent, held with the file 4; the app's 3 left under decision 6.
20 + 31 + 19 + 15 + 4 = 89.

**Order for task 2:** `TheScoreSaysWhatItIsMeasuringTests`, `ThePosteriorSurvivesItsOwnArithmeticTests`,
`FittingKeyUpAgainstAssumingItTests` deleted; then the four trims.
**Order for task 3:** `ABlipDoesNotShiftEverythingAfterItTests` first; then the trims, fewest survivors
first - `TheTapIsNotBehindTheDecoderTests` 1, `NothingActsOnTheAdmissionVerdictTests` 1,
`WhatDecodeScoringCostsTests` 2, `TheReadPathDoesNotAllocateTests` 3; then the two whole files the
grep freed, `TheCleanReadsStayCleanTests` 3, `TheProbabilisticDecoderTests` 6.
