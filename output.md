```
READ IN THIS ORDER.

A. The phase goal - CW decodes again. Steps 0, 1, 2 and 4 are ticked on
   every criterion; the outcome file holds 0 and 2 at not started and 4 at
   partial, a layer mismatch reported here, and the launcher named step 0
   for this unit on that reading; step 3 is this unit's, partial at 3.1 and
   3.3 at entry with 21 reds open in the set and 22 test files excluded
   from compilation, nothing yet retired; step 5 is Tim's. After this unit
   step 3 is partial at 3.1, 3.2 and 3.3 with 20 tests in 7 files retired
   under R49, 31 facts left excluded and red-open because they assert a
   decode result, 7 files re-included, and 3.4 and 3.5 waiting on the
   decode repairs.
B. The criteria, one line each, met or not: 3.1 met by unit 394, its #1
   now green, 3 of 3, re-included by this unit, so the set stands at 31
   green and 20 red-open; 3.2 20 retirements each quoting its missing name
   in this report and carrying its four fields in docs/cw-retired-tests.txt,
   ticked after a green build following the last retirement; 3.3 nothing
   that reads audio and asserts characters, elements, a tone or a speed
   retired, 31 such facts listed red-open, its clause added; 3.4 not met,
   the known-reds block and the set's closing line untouched; 3.5 not
   ticked, the step not at its exit, floors green at task 0 and task 4
   with no file under src changed.
C. The report last. Section 4 raises 0 items and none is in the way of a
   criterion in B; everything carried from before this phase and every
   finding that blocks nothing is in docs/phase-cw/PARKED.md under R54,
   not here.
```

```
UNIT:       398 - complete at task 4 of 5, tasks 0 to 4, none dropped - 2026-09-23 03:00
PHASE GOAL: Get the CW decoder reading real on-air audio again - put it back where it last worked, judge the August changes on numbers, and clear the inherited reds by repair or by a recorded retirement, until Tim hears it read at the radio.
UNIT GOAL:  Sort the 22 test files that could not compile against the restored decoder, one fact at a time, under Tim's R49 rule - retire each fact that tests a mechanism no longer in the decoder and asserts no decode result, leave every fact that asserts what the decoder reads excluded and named as a repair owed, and put back and run every fact that still compiles.
ADVANCED:   yes - 3.2 flipped from open to ticked on 20 retirements that each quote the missing name and carry all four fields, after a green build; #1 of the set now has a measurement, green.
NUMBER:     excluded files 22 -> 12; tests retired 0 -> 20 in 7 files; facts red-open outside the set 32, 31 still excluded and 1 re-included red; set names green 30 -> 31, red-open 21 -> 20; floors 37 of 37, 13 of 13, 0 of 2 at both ends; engine line 374 s -> 374 s of 480
DRIFT:      0
```

## 1. What Claude did

**Complete, at task 4 of 5 - tasks 0, 1, 2, 3 and 4 all done, nothing dropped.** Neither clock rule
fired: the last re-include ended at 02:47, minute 16 of the hour that began with the first status
line at 02:31, and task 4 started at minute 17. Windows 11 machine, Hamlet confirmed at
`C:\Source\HamLet` by the gate's four checks, branch `main`. Every commit pushed; the last push,
`4d4e552f`, succeeded; this report rides in one more.

**Task 0, entry** (`305bec69`). Version 1.13.84 to 1.13.85. `PHASE_STATUS.md` read `CURRENT_STEP: 0`
and `WORK_INSTRUCTION: 397 - pieces 34 to 45, the list to its end`; set to `CURRENT_STEP: 3` and
`398 - the excluded files, classified and retired under R49`. `PHASE_OUTCOME.md` and
`PHASE_STATUS.md` went into task 0's commit as whole files, as units 393 to 397's did. Unit 397's
floors, carry, build, commit, transmit, cmp and nums scripts copied to `unit398-*` with the unit number
and task labels changed. **Decision 4 applied**: `git diff --stat 3af36501 HEAD -- src tests
docs/carry-forward-tests.txt` printed nothing, so unit 397's exit runs are the entry numbers for
both lines. Floors run in full after one build: 37 of 37 in 94 s, every capture row identical to unit
397's exit; 13 of 13 in 29 s; synthetics 0 of 2 in 3 s. Transmit files and `src` silent.

**Task 1, the classification** (`10a6b715`), committed before any test file was touched. All 22
files read; every name each file's code uses under `Cw` grepped by `.run-unit/unit398-grep.sh`,
`unit398-grep2.sh` and `unit398-grep3.sh`; 92 facts, a `[Theory]` counted once, one row each in
`docs\phase-cw\unit398-excluded.md` section 2. Retired 20, stays red-open 31, re-include 19,
retirable but left with a stays-whole file 15, naming nothing absent but held with the file 4, the
app's 3 left under decision 6. The finding that moved the plan: **unit 392's `CwAccuracy` is doc
prose only in `TheCleanReadsStayCleanTests` and `TheProbabilisticDecoderTests`**, so both were
re-include whole, not stays whole, the same finding unit 394 made for #1.

**Task 2, the retirements**, one file per commit, then the tick:

| Commit | File | What went |
|---|---|---|
| `ceb06af9` | `Cw/TheScoreSaysWhatItIsMeasuringTests.cs` | deleted, 8 tests, csproj line out |
| `30176620` | `Cw/ThePosteriorSurvivesItsOwnArithmeticTests.cs` | deleted, 6 tests, csproj line out |
| `fa7b8911` | `Cw/FittingKeyUpAgainstAssumingItTests.cs` | deleted, 1 test, csproj line out |
| `732051f2` | `Audio/TheReadPathDoesNotAllocateTests.cs` | trimmed, 1 fact out, 3 kept |
| `a1dff1ad` | `Audio/TheTapIsNotBehindTheDecoderTests.cs` | trimmed, 2 facts out, 1 kept |
| `60a2fa36` | `Cw/NothingActsOnTheAdmissionVerdictTests.cs` | trimmed, 1 fact out, 1 kept |
| `41497e5a` | `Cw/WhatDecodeScoringCostsTests.cs` | trimmed, 1 fact out, 2 kept |
| `48b420c9` | `PHASE_PLAN.md`, the doc's section 3 | 3.2 ticked, 3.3's clause |

`dotnet build Hamlet.sln -warnaserror` after the last retirement: exit 0, 15 s.

**Task 3, the re-includes**, in task 1's order, every one built:

| Commit | File | Result |
|---|---|---|
| `ba998d84` | `Cw/ABlipDoesNotShiftEverythingAfterItTests.cs`, the set's #1 | 3 green; 3.1 gains its clause |
| `fd2e7258` | `Audio/TheTapIsNotBehindTheDecoderTests.cs` | 1 green |
| `447b939c` | `Cw/NothingActsOnTheAdmissionVerdictTests.cs` | 1 green |
| `6657865e` | `Cw/WhatDecodeScoringCostsTests.cs` | 2 green |
| `6b8e19a9` | `Audio/TheReadPathDoesNotAllocateTests.cs` | 3 green |
| `997029fd` | `Cw/TheCleanReadsStayCleanTests.cs` | 2 facts green, 1 red-open on one case |
| `7a297abf` | `Cw/TheProbabilisticDecoderTests.cs` | 6 green, 11 cases |

**Task 4, the exit round** (`4d4e552f`): app 278 of 278 in 160 s, engine 176 of 176 in 374 s; floors
37 of 37, 13 of 13, 0 of 2, every capture identical; transmit files, `src`, the carry-forward list and
the failing set unchanged. **No regression.** `PARKED.md` gains `398 item 1` to `398 item 4`.

**Decisions of the instruction applied:** 1, step 3 resumed on the 22 files; 2, per-fact retirement
under both R49 sentences, doubt resolved toward staying; 3, `CoarseSpacingHz` private at HEAD, used
for one retirement, `WhatDecodeScoringCostsTests.AShortScoringWindowBreaksTheSilenceProperty`;
4, the lines at entry from unit 397's exit; 5, nothing under `src` changed and 3.5 not ticked; 6, the
app file left and listed; 7, #1 re-included first; 8, whole-file deletions only where every fact
retired, trims where survivors name nothing absent, stays-whole files left unedited; 9, the retired
file's header and line shape; 10, the doc; 11, 3.2 ticked after the green build, 3.3 and 3.1 clauses,
nothing else ticked; 12 and 13, clock and timeouts, neither rule reached.

**Decisions I made for myself, about how to carry out assigned tasks, none authorizing work
outside them - no self-ruling was used:**

1. **The 5-argument `CwProbabilisticDecoder.Decode` is private at HEAD** (`CwProbabilisticDecoder.cs:706`),
   treated like `CoarseSpacingHz` under decision 3. It changes no verdict: every fact that calls it
   also asserts elements on audio and stays.
2. **`PitchChoice` as a `CwDecodeReport` constructor parameter counts as an absent member.** The
   record's positional parameters end at `PitchWasMeasured` (`CwDecodeReport.cs:47` to `61`);
   `PitchChoice` exists only as a property at line 83, and the fact names it as a parameter. On that
   I retired `NothingActsOnTheAdmissionVerdictTests.AnUnmeasuredPitchIsStillReportedAndSaysSo`.
3. **That fact reads no audio.** The instruction's table says it asserts `CharactersEmitted > 0` on
   audio. The fact builds the report by hand with the literal `CharactersEmitted: 61`, and no `.wav`,
   generator or decoder is involved, so R49's second sentence does not reach it. Decision 2's "a count
   of characters emitted is characters" governs a count *obtained from audio*; this one is a
   constant the test set itself. If Tim reads it the other way, the fact comes back from `60a2fa36`'s
   parent and its line comes out of `docs\cw-retired-tests.txt`.
4. **Doubt went toward staying for facts on buffers outside decision 2's list of audio** - hand-built
   sines in `EveryElementCarriesItsOwnPitchTests` and random noise in `NoiseHasAPeakToo` - when they
   assert a hertz. Every such fact is in a file that stays whole anyway, so no retirement rests on it.
5. **A fact in a stays-whole file that names nothing absent** is marked *names nothing absent, held
   with the file* rather than *re-include*, because decision 8 keeps the file whole and unedited.

## 2. What the owner should expect

Nothing changes on the CW tab, and nothing under `src` changed: the decoder that read on the air is
byte-identical to the one unit 397 left, and its floors are green at both ends. **Twenty dead tests
are gone**, each with a line in `docs\cw-retired-tests.txt` naming what it tested that no longer
exists: `CwAccuracy` (8, a text-scoring harness), `LogSum` and `Posterior` (6, arithmetic of a
decoder path the restore removed), `FittedLogLikelihoods` (1), `CwKeyingMeter.WindowSizings` (1),
`CwDecoder.ProcessDelayForTests` (2), a `PitchChoice` constructor parameter (1), and the private
`CoarseSpacingHz` (1). **Thirty-one tests stay out and red** because they assert what the decoder
reads - text, a pitch, a speed, elements - through types the August rework brought and the restore
removed; those are the repairs still ahead, listed by name in the doc. Seven files compile and run
again. **The set's #1 now has a measurement: green.** One re-included test is red on real audio -
`003758` no longer contains `AA4MP/4 QNIK` in `TheCleanReadsStayCleanTests`, though the anchored
`TheAdjudicatedReadingsKeepReadingTests` stays green - and it is parked as a repair.

**What will look wrong but is not:** `PHASE_OUTCOME.md` still says step 0 and step 2 are *not started*
and step 4 *partial* with every criterion ticked in the plan - the layer's lines, not edited.
`PROJECT_STATUS.md` says `RULES_AT: HM-DEC-165` while `CLAUDE.md`'s top row is HM-DEC-167 - the
status script writes it as a literal, parked since 390. Twelve files are still excluded from
compilation; that is correct under R49, not unfinished work.

## 3. What you should see

**No visible change - this unit sorts the test pile so the dead tests are gone on the record and the
live ones are named as repairs.** The answer the unit was commissioned for: 3.2 is ticked on 20
retirements, and not one retired test asserts a decode result.

**The classification, all 22 files, as `docs\phase-cw\unit398-excluded.md` section 2 carries it.**
*Absent* is what `grep -rnw` over `src/Hamlet.RadioEngine/Cw` at HEAD printed: *nothing*, or the line.

| # | File | Fact | Absent at HEAD, as grep printed | Audio | Asserts | Verdict |
|---|---|---|---|---|---|---|
| 1 | `Audio/TheReadPathDoesNotAllocateTests` | `ARepeatingReaderAllocatesNothingAfterTheFirstCall` | none | no | bytes allocated | re-include |
| 2 | | `AFixedSpanReadRepeatedlyAllocatesNothing` | none | no | bytes allocated | re-include |
| 3 | | `TheKeyingMeterSizesItsWindowOnceAndReusesIt` | `CwKeyingMeter.WindowSizings` - nothing | no | a sizing count | **retired** |
| 4 | | `ReadingTheArrivalRatioAllocatesNothing` | none | no | bytes allocated | re-include |
| 5 | `Audio/TheTapIsNotBehindTheDecoderTests` | `TheTapIsWholeWhileTheDecoderCrawls` | `CwDecoder.ProcessDelayForTests` - nothing | zeros pumped | sample count, callback time | **retired** |
| 6 | | `AFullQueueDropsAndCounts` | `CwDecoder.ProcessDelayForTests` - nothing | zeros pumped | sample and drop counts | **retired** |
| 7 | | `TheTapIsFedOnceWhicheverWayTheAudioArrives` | none | zeros pumped | sample count | re-include |
| 8 | `Cw/ABlipDoesNotShiftEverythingAfterItTests`, #1 | `ASubMinimumBlipInAGapChangesNothingAfterIt` | none | yes | text | re-include |
| 9 | | `ThreeBlipsChangeNothingEither` | none | yes | text | re-include |
| 10 | | `ADitLongInjectionIsNoticed` | none | yes | text | re-include |
| 11 | `Cw/AMoveStartsTheDecoderFreshTests` | `NothingTheDecoderLearnedSurvivesTheMove` | none | yes | characters | held with the file |
| 12 | | `ARetunedDecoderMatchesOneThatHasNeverListened` | `CwDecoder.PitchWasAsserted`, `CwDecoder.Ranked` - not on `CwDecoder` | yes | character counts | stays red-open |
| 13 | | `TheDecoderReadsTheNewStationAfterTheMove` | none | yes | characters | held with the file |
| 14 | `Cw/AStationIsABinThatSwingsTests` | `TheRefusedCqIsFoundNearFiveEightyThree` | `CwSwingSurvey` - nothing | yes | a hertz | stays red-open |
| 15 | | `TheNoisePickNoLongerWins` | `CwSwingSurvey` | yes | a hertz | stays red-open |
| 16 | | `TheMarginBetweenSilenceAndAStation` | `CwSwingSurvey` | yes | swing in dB | retirable, left with the file |
| 17 | | `EveryCaptureOfTheEveningRanks` | `CwSwingSurvey` | yes | a candidate pitch; doubt | stays red-open |
| 18 | `Cw/EveryElementCarriesItsOwnPitchTests` | `ADahResolvesToAboutFiveHertz` | `CwElementPitch` - nothing | sine; doubt | a hertz | stays red-open |
| 19 | | `ADitResolvesToAboutEighteenHertz` | `CwElementPitch` | sine; doubt | a hertz | stays red-open |
| 20 | | `TooShortToMeasureSaysSoRatherThanGuessing` | `CwElementPitch` | sine; doubt | no hertz | stays red-open |
| 21 | | `AGapIsNotMeasured` | `CwElementPitch`, `CwElement` - nothing | sine; doubt | hertz per element | stays red-open |
| 22 | | `TwoSendersThirteenHertzApartAreSeparableOnTheirDahs` | `CwElementPitch` | sine; doubt | hertz apart | stays red-open |
| 23 | | `TheElementStreamComesOutWithTheText` | `CwProbabilisticResult.Elements`, private 5-arg `Decode` | `.wav` | elements | stays red-open |
| 24 | `Cw/FittingKeyUpAgainstAssumingItTests` | `WhatFittingKeyUpDoesToEveryRecording` | `CwProbabilisticDecoder.FittedLogLikelihoods` - nothing | yes | `Cases.Length` is 9 | **retired** |
| 25 | `Cw/IsTheHertzABiasOrAFloorTests` | `TheErrorAcrossCarriersSpeedsAndDuties` | `CwSpectralPeak` - nothing | yes | a pitch found | stays red-open |
| 26 | | `AShortBurstInALongRecordingIsFoundBetterOverTheLoudestStretch` | `CwSpectralPeak` | yes | a pitch found | stays red-open |
| 27 | | `TheCarrierThatRetiredN4L` | `CwSpectralPeak` | yes | a pitch found | stays red-open |
| 28 | `Cw/NoSenderIsSplitInTwoTests` | `ASenderHamletAlreadyReadsIsNeverDivided` | `CwStreamSplit`, `CwElementPitch`, `Elements` | yes | no division by pitch; doubt | stays red-open |
| 29 | | `TheTwoSenderCaptureIsNotYetDividedEither` | same | yes | hertz apart | stays red-open |
| 30 | | `TooFewLongMarksIsReportedAndNotResolved` | `CwStreamSplit`, `CwElement` | no | a refusal | retirable, left with the file |
| 31 | `Cw/NothingActsOnTheAdmissionVerdictTests` | `TheDecoderExposesTheVerdictAndTheEmitPathDoesNotConsultIt` | none | no | constants and a flag | re-include |
| 32 | | `AnUnmeasuredPitchIsStillReportedAndSaysSo` | `PitchChoice` as a constructor parameter - not in `CwDecodeReport.cs:47` to `61` | no, a literal 61 | a literal it set | **retired** |
| 33 | `Cw/TheCleanReadsStayCleanTests` | `EachCleanCaptureStillContainsItsTruth` | none, `CwAccuracy` prose only | yes | text | re-include |
| 34 | | `EachCleanCaptureStillNamesAsManyCharacters` | none | yes | named characters | re-include |
| 35 | | `EveryFloorWasMeasuredAndNotHopedFor` | none | yes | named characters | re-include |
| 36 | `Cw/TheFirstSecondsAreReadAgainTests` | `AnEmptyBandIsNeverReadAgain` | `CwProbabilisticStream.ReReads` - nothing | yes | characters | stays red-open |
| 37 | | `TheReplayFiresTheSameWhateverTheBufferSize` | `ReReads` | yes | a re-read count | retirable, left with the file |
| 38 | | `NothingIsSaidTwice` | `ReReads` | yes | settled characters | stays red-open |
| 39 | | `TheCallsignTheReReadRecovers` | none | yes | text | held with the file |
| 40 | `Cw/ThePeakAgainstASecondSignalTests` | `WhereThePeakSwitchesFromOneStationToTheOther` | `CwSpectralPeak` | yes | `Assert.True(true)` | retirable, left with the file |
| 41 | | `WhetherThePeakWalksBetweenThemWithinOneRecording` | `CwSpectralPeak` | yes | pitches found | stays red-open |
| 42 | | `WhatTheOldTrackerDoesOnTheSameMix` | none | yes | nothing | held with the file |
| 43 | `Cw/ThePeakFindsThePitchTheTrackerMissedTests` | `AGeneratedToneIsFoundToWithinAHertz` | `CwSpectralPeak` | yes | a hertz | stays red-open |
| 44 | | `ThePeakAgreesWithTheKeyedBin` | `CwSpectralPeak` | yes | a hertz | stays red-open |
| 45 | | `TooLittleAudioReturnsNothing` | `CwSpectralPeak` | no | no peak | retirable, left with the file |
| 46 | | `NoiseHasAPeakToo` | `CwSpectralPeak` | noise; doubt | a pitch found | stays red-open |
| 47 | `Cw/ThePosteriorSurvivesItsOwnArithmeticTests` | `LogSumStaysFinite` | `CwProbabilisticDecoder.LogSum` - nothing | no | finite | **retired** |
| 48 | | `TwoEqualTermsDoubleTheEvidence` | `LogSum` | no | arithmetic | **retired** |
| 49 | | `NegativeInfinityIsTheIdentity` | `LogSum` | no | arithmetic | **retired** |
| 50 | | `EveryPosteriorIsAProbability` | `CwProbabilisticDecoder.Posterior` - nothing | `.wav` | range 0 to 1 | **retired** |
| 51 | | `DigitalSilenceProducesNothingRatherThanANumber` | `Posterior` | no | range 0 to 1 | **retired** |
| 52 | | `NoHopsProduceNoPosterior` | `Posterior` | no | null | **retired** |
| 53 | `Cw/TheProbabilisticDecoderTests` | `ItReadsWhatTheReferenceReads` | none, `CwAccuracy` prose only | yes | text, speed | re-include |
| 54 | | `TheSpeedIsFoundAndNotTold` | none | yes | text, speed | re-include |
| 55 | | `ARecordingWithNoStationInItSaysNothing` | none | yes | no characters | re-include |
| 56 | | `TheGateSitsInAWideGap` | none | yes | ratios | re-include |
| 57 | | `ItKeepsUpWithLiveAudio` | none | yes | text | re-include |
| 58 | | `NothingIsSettledFromAnEmptyBandLive` | none | yes | no characters | re-include |
| 59 | `Cw/TheQuietestBinNoLongerWinsTests` | `AnEmptyBinScoresNearNothing` | `CwPitchRanking` - nothing | yes | bin scores; doubt | stays red-open |
| 60 | | `TheStationWinsTheBand` | `CwPitchRanking`, private `CoarseSpacingHz` | yes | a hertz | stays red-open |
| 61 | | `OnRealAudioTheBareScorePicksAnEmptyBin` | same | yes | a hertz | stays red-open |
| 62 | | `TheRankingDoesNotYetDriveTheDecode` | `CwDecoder.RankThePitch` - nothing | no | a switch | retirable, left with the file |
| 63 | | `WithTheRankingOffTheSheetReportsTheTrackersPitch` | `Rankings`, `CwDecoder.Ranked`, `Rank` - nothing | yes | counters | retirable, left with the file |
| 64 | | `NothingIsRankedFromTooLittleAudio` | `CwPitchRanking` | yes | no pitch; doubt | stays red-open |
| 65 | | `DigitalSilenceIsNotRanked` | `CwPitchRanking` | no | not ranked | retirable, left with the file |
| 66 | | `TheFloorIsAddedInPower` | `CwPitchRanking` | no | arithmetic | retirable, left with the file |
| 67 | | `TheCandidatesAreTheTrackersOwnBins` | `CwPitchRanking`, private `CoarseSpacingHz` | no | the grid | retirable, left with the file |
| 68 | `Cw/TheReferenceDecoderIsPortedFaithfullyTests` | `AcquisitionFindsTheNetAndTheTrackerRefinesIt` | `CwReferenceDecoder` - nothing | yes | a hertz | stays red-open |
| 69 | | `TheNetReadsExactlyAsTheReferenceReadsIt` | `CwReferenceDecoder` | yes | text, dit, dah | stays red-open |
| 70 | | `NoClockFitsNoise` | `CwReferenceDecoder` | no | no clock | retirable, left with the file |
| 71 | | `EightMarksAreNeededBeforeAClockIsFittedAtAll` | `CwReferenceDecoder` | no | no clock | retirable, left with the file |
| 72 | | `ATextbookFistFitsAndAnImpossibleSpeedDoesNot` | `CwReferenceDecoder` | no | a dit from numbers | retirable, left with the file |
| 73 | | `AHeavyFistIsAdmittedByScatterWhereTheRatioBandRefusesIt` | `CwReferenceDecoder` | no | a clock | retirable, left with the file |
| 74 | | `TheGateRefusesAWindowWithTooLittleContrast` | `CwReferenceDecoder` | no | keyed hops | retirable, left with the file |
| 75 | | `TheSilenceControlsBehaveAsMeasured` | `CwReferenceDecoder` | yes | a character count | stays red-open |
| 76 | | `AnAllZeroBufferIsRefused` | `CwReferenceDecoder` | zeros into a decoder | no characters | stays red-open |
| 77 | `Cw/TheScoreSaysWhatItIsMeasuringTests` | `APerfectReadScoresOne` | `CwAccuracy` - nothing | no | a score of two strings | **retired** |
| 78 | | `ABlockIsADeletionAndNotASubstitution` | `CwAccuracy` | no | same | **retired** |
| 79 | | `RefusingEverythingYieldsNothing` | `CwAccuracy` | no | same | **retired** |
| 80 | | `OnlyTheSpanWithTruthIsScored` | `CwAccuracy` | no | same | **retired** |
| 81 | | `InsertionsAndDeletionsAreCountedApart` | `CwAccuracy` | no | same | **retired** |
| 82 | | `NothingReadIsScoredAsEveryCharacterLost` | `CwAccuracy` | no | same | **retired** |
| 83 | | `SpacingIsNormalizedRatherThanScored` | `CwAccuracy` | no | same | **retired** |
| 84 | | `TheScoreIsDeterministic` | `CwAccuracy` | no | same | **retired** |
| 85 | `Cw/WhatDecodeScoringCostsTests` | `OneDecodeAtOnePitchCostsThis` | none | yes | characters above 0 | re-include |
| 86 | | `APitchWithNothingOnItIsTheCheapCase` | none | yes | rows ran | re-include |
| 87 | | `AShortScoringWindowBreaksTheSilenceProperty` | `CwToneTracker.CoarseSpacingHz` - `CwToneTracker.cs:131: private const double CoarseSpacingHz = 25;`, private at HEAD | yes | `broken.Count >= 0` | **retired** |
| 88 | `Cw/WhereHamletAndTheReferenceDivergeTests` | `TheElementStreamsAgreeOverTheCallsign` | `CwProbabilisticResult.Elements`, private 5-arg `Decode` | yes | elements | stays red-open |
| 89 | | `TheShortRunFloorDiscardsAlmostNothing` | `CwUnitEstimator.Elements` with `out` - only the 3-argument form at `CwUnitEstimator.cs:334` | yes | dropped runs; doubt | stays red-open |
| 90 | app `Views/ThePitchControlsAreOffThePanelTests` | `HoldThisPitchIsNotOnThePanel` | none | no | a screen fact | left, decision 6 |
| 91 | | `ACaptureCannotReportAnAssertedPitch` | none | no | a screen fact | left, decision 6 |
| 92 | | `TheEngineStillCarriesTheCapability` | `CwDecoder.AssertAt` - nothing | no | a lock | left, decision 6, parked |

**The retirements, as `docs\cw-retired-tests.txt` carries them** below its four-line header:

```
Hamlet.RadioEngine.Tests.Cw.TheScoreSaysWhatItIsMeasuringTests.APerfectReadScoresOne | missing: CwAccuracy | unit 398 | 2026-09-23
Hamlet.RadioEngine.Tests.Cw.TheScoreSaysWhatItIsMeasuringTests.ABlockIsADeletionAndNotASubstitution | missing: CwAccuracy | unit 398 | 2026-09-23
Hamlet.RadioEngine.Tests.Cw.TheScoreSaysWhatItIsMeasuringTests.RefusingEverythingYieldsNothing | missing: CwAccuracy | unit 398 | 2026-09-23
Hamlet.RadioEngine.Tests.Cw.TheScoreSaysWhatItIsMeasuringTests.OnlyTheSpanWithTruthIsScored | missing: CwAccuracy | unit 398 | 2026-09-23
Hamlet.RadioEngine.Tests.Cw.TheScoreSaysWhatItIsMeasuringTests.InsertionsAndDeletionsAreCountedApart | missing: CwAccuracy | unit 398 | 2026-09-23
Hamlet.RadioEngine.Tests.Cw.TheScoreSaysWhatItIsMeasuringTests.NothingReadIsScoredAsEveryCharacterLost | missing: CwAccuracy | unit 398 | 2026-09-23
Hamlet.RadioEngine.Tests.Cw.TheScoreSaysWhatItIsMeasuringTests.SpacingIsNormalizedRatherThanScored | missing: CwAccuracy | unit 398 | 2026-09-23
Hamlet.RadioEngine.Tests.Cw.TheScoreSaysWhatItIsMeasuringTests.TheScoreIsDeterministic | missing: CwAccuracy | unit 398 | 2026-09-23
Hamlet.RadioEngine.Tests.Cw.ThePosteriorSurvivesItsOwnArithmeticTests.LogSumStaysFinite | missing: CwProbabilisticDecoder.LogSum | unit 398 | 2026-09-23
Hamlet.RadioEngine.Tests.Cw.ThePosteriorSurvivesItsOwnArithmeticTests.TwoEqualTermsDoubleTheEvidence | missing: CwProbabilisticDecoder.LogSum | unit 398 | 2026-09-23
Hamlet.RadioEngine.Tests.Cw.ThePosteriorSurvivesItsOwnArithmeticTests.NegativeInfinityIsTheIdentity | missing: CwProbabilisticDecoder.LogSum | unit 398 | 2026-09-23
Hamlet.RadioEngine.Tests.Cw.ThePosteriorSurvivesItsOwnArithmeticTests.EveryPosteriorIsAProbability | missing: CwProbabilisticDecoder.Posterior | unit 398 | 2026-09-23
Hamlet.RadioEngine.Tests.Cw.ThePosteriorSurvivesItsOwnArithmeticTests.DigitalSilenceProducesNothingRatherThanANumber | missing: CwProbabilisticDecoder.Posterior | unit 398 | 2026-09-23
Hamlet.RadioEngine.Tests.Cw.ThePosteriorSurvivesItsOwnArithmeticTests.NoHopsProduceNoPosterior | missing: CwProbabilisticDecoder.Posterior | unit 398 | 2026-09-23
Hamlet.RadioEngine.Tests.Cw.FittingKeyUpAgainstAssumingItTests.WhatFittingKeyUpDoesToEveryRecording | missing: CwProbabilisticDecoder.FittedLogLikelihoods | unit 398 | 2026-09-23
Hamlet.RadioEngine.Tests.Audio.TheReadPathDoesNotAllocateTests.TheKeyingMeterSizesItsWindowOnceAndReusesIt | missing: CwKeyingMeter.WindowSizings | unit 398 | 2026-09-23
Hamlet.RadioEngine.Tests.Audio.TheTapIsNotBehindTheDecoderTests.TheTapIsWholeWhileTheDecoderCrawls | missing: CwDecoder.ProcessDelayForTests | unit 398 | 2026-09-23
Hamlet.RadioEngine.Tests.Audio.TheTapIsNotBehindTheDecoderTests.AFullQueueDropsAndCounts | missing: CwDecoder.ProcessDelayForTests | unit 398 | 2026-09-23
Hamlet.RadioEngine.Tests.Cw.NothingActsOnTheAdmissionVerdictTests.AnUnmeasuredPitchIsStillReportedAndSaysSo | missing: CwDecodeReport constructor parameter PitchChoice | unit 398 | 2026-09-23
Hamlet.RadioEngine.Tests.Cw.WhatDecodeScoringCostsTests.AShortScoringWindowBreaksTheSilenceProperty | missing: CwToneTracker.CoarseSpacingHz, private at HEAD | unit 398 | 2026-09-23
```

**The re-includes, each built and run by type**, `timeout 600`, `--no-build`:

| File | Build | Run | Facts green or red |
|---|---|---|---|
| `ABlipDoesNotShiftEverythingAfterItTests` | exit 0, 8 s | 3 of 3, 3 s | all green; clean, blipped and three-blip all `CQ DE W1AW K`, the dit control `CQ ME W1AW K` |
| `TheTapIsNotBehindTheDecoderTests` | exit 0, 6 s | 1 of 1, 2 s | green |
| `NothingActsOnTheAdmissionVerdictTests` | exit 0, 6 s | 1 of 1, 2 s | green |
| `WhatDecodeScoringCostsTests` | exit 0, 6 s | 2 of 2, 6 s | both green; 21 characters at the 12 s window |
| `TheReadPathDoesNotAllocateTests` | exit 0, 5 s | 3 of 3, 2 s | all green |
| `TheCleanReadsStayCleanTests` | exit 0, 5 s | 6 of 7 cases, 20 s | `EachCleanCaptureStillContainsItsTruth` **red-open** on `cw-2026-08-18-003758`: wants `AA4MP/4 QNIK`, reads `■ ■ ■R L T U ■ ■ I AN EAND E A ET EEEETMP/4 QNIKK ■ ■■■■ ■■ E AN EANQNIK ...`; green on `012403` and `013347`. Named-character counts green: 21 of floor 20, 44 of 42, 57 of 9 |
| `TheProbabilisticDecoderTests` | exit 0, 5 s | 11 of 11 cases, 9 s | all six facts green |

**Carry-forward lines and floors:**

| Run | Entry | Exit |
|---|---|---|
| App line | 278 of 278, 167 s, unit 397's exit under decision 4 | 278 of 278, 160 s |
| Engine line | 176 of 176, 374 s, unit 397's exit under decision 4 | 176 of 176, 374 s of 480 |
| Captures type | 37 of 37, 94 s | 37 of 37, 91 s, every row identical |
| Adjudicated type | 13 of 13, 29 s | 13 of 13, 29 s |
| Clean synthetics | 0 of 2, 3 s, R53 | 0 of 2, `■ ■ ■ ■ ■  ■ ■ ■ ■■` and `■ ■ ■  ■■■` |

Decision 4's diff, `git diff --stat 3af36501 HEAD -- src tests docs/carry-forward-tests.txt`,
printed nothing. At exit: the eleven transmit files nothing against `7e209cb4`; `git diff --stat
5688a8a5 HEAD -- src` nothing; `git diff --stat 7e2abb7e HEAD -- tests` 8 files, 833 deletions -
exactly the 3 deleted, the 4 trimmed and the engine csproj; `git worktree list` the root and the
three preflight trees.

**Section 5 checks:** HEAD was `7e2abb7e` as named; `Directory.Build.props` line 1254 read
`1.13.84`; `PROJECT_STATUS.md` at HEAD read unit 390, `TASK 5 of 6`, the working copy unit 397
`COMPLETED`, `TASK 2 of 3` - a mismatch, `398 item 4`; `PHASE_STATUS.md` read `CURRENT_STEP: 0`,
`WORK_INSTRUCTION: 397`, set as section 5 asks; `PHASE_OUTCOME.md` as described, paired entries for
392 to 397; the modified and untracked root and `tools\arbiter\` files as listed, plus `SESSION.lock`,
left; `CLAUDE.md` line 360 is the top row, HM-DEC-167; `PARKED.md` two headings, 38 items;
`docs\cw-retired-tests.txt` absent at entry; 21 and 1 `<Compile Remove>` lines as stated; 34 files
under `Cw`, the grep as the table above; `Cw` against `7e209cb4` 4 files as stated; the carry-forward
lines, guard row and known-reds lines where stated; the printer and the two capture rows as stated.
Mismatches beyond `PROJECT_STATUS.md`: the instruction's table calls `NothingActsOnTheAdmissionVerdictTests`'
second fact an audio fact and it is not; unit 392's `CwAccuracy` is prose only in two files; the app
file carries 3 facts, not 1. All in `398 item 4`.

## 4. What's blocking us

Nothing. No criterion of step 3 that this unit could reach is waiting on a ruling. 3.4 and 3.5 wait on
the decode repairs R49 orders next, which are the next units' work, not a question. Everything this
unit found that blocks nothing is `398 item 1` to `398 item 4` in `docs\phase-cw\PARKED.md`.
