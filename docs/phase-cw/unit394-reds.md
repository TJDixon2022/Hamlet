# Unit 394 - the 51 inherited CW reds, run at HEAD by type

Work instruction 394, step 3 criterion 3.1. Every name in `docs\unit239-failing-set.txt` run
against the restored decoder at HEAD, one invocation per type, and classified under R49. Every
result here is an indication (FACT-004); *green* means the assertion held, not that CW works
(`CLAUDE.md` 0.0).

## 1. The runs

Thirteen invocations against `tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj`,
each `--filter "FullyQualifiedName~<Type>"` (the two `Fixtures` types and `CwFixtureTests` by
full namespace), `--no-build` after task 0's engine-line build (decision 5), `timeout 600`,
`--logger "console;verbosity=detailed"`, raw output under `.run-unit\unit394-set-<Type>.txt`,
not committed. The floor type's invocation is task 0's captures run. Wall time is the script's
clock around `dotnet test`. No run died before an assertion; nothing was re-run.

| Type | File added | Cases in the type | Cases in the set | Green | Red | Lost | Wall time |
|---|---|---|---|---|---|---|---|
| `ABlipDoesNotShiftEverythingAfterItTests` | 2026-08-29 | 3 | 1 | - | - | - | not run, excluded from compilation |
| `ARecordingWithKeyingInItIsReadTests` | 2026-08-20 | 5 | 3 | 5 | 0 | 0 | 14 s |
| `CapturedSignalTests` | 2026-08-16 | 13 | 1 | 13 | 0 | 0 | 42 s |
| `CwAcquisitionWindowTests` | 2026-08-18 | 12 | 12 | 10 | 2 | 0 | 18 s |
| `CwDisplacementFloorTests` | 2026-08-18 | 6 | 6 | 0 | 6 | 0 | 13 s |
| `CwEmissionGateTests` | 2026-08-16 | 8 | 1 | 7 | 1 | 0 | 8 s |
| `CwFixtureTests` | 2026-08-14 | 23 | 9 | 14 | 9 | 0 | 14 s |
| `CwLowDutyTests` | 2026-08-16 | 4 | 3 | 4 | 0 | 0 | 21 s |
| `CwRefiningRetuneTests` | 2026-08-18 | 3 | 3 | 3 | 0 | 0 | 6 s |
| `CwSurveyThresholdPinTests` | 2026-08-17 | 3 | 1 | 3 | 0 | 0 | 7 s |
| `Fixtures.CwAdjudicationTests` | 2026-08-17 | 11 | 1 | 10 | 1 | 0 | 13 s |
| `Fixtures.CwReceiverFixtureTests` | 2026-08-17 | 27 | 4 | 23 | 4 | 0 | 14 s |
| `OneDecoderNotTwoTests` | 2026-08-25 | 106 | 3 | 100 | 0 | 0 | 601 s, timed out; split 580 s, timed out |
| `TheCapturesThatDecodeKeepDecodingTests` | before 08-25 | 37 | 2 | 37 | 0 | 0 | 97 s, task 0 |
| `ThePitchCanBeHeldTests` | 2026-08-24 | 5 | 1 | 5 | 0 | 0 | 3 s |

**`OneDecoderNotTwoTests` did not finish inside 600 s** (decision 5). The whole-type run printed
81 green and no red before `timeout` killed it at 601 s, exit 124: all 53 cases of
`ListeningAndFeedingReadTheSame`, the set's three among them, and 28 of 53 of
`TheBufferSizeChangesNothing`. That method was then run alone, split by method, at
`timeout 580` rather than 600, so the call stayed inside the harness's 600 s foreground cap. The
first call, at `timeout 600` plus the status line, passed the cap by about a second and the
harness moved it to the background; it was not polled, and the notice of its end was the only
read. The split run printed 47 green and no red before it too was killed, at 580 s. Across the
two runs 47 of the 53 `TheBufferSizeChangesNothing` cases are green and **six are unmeasured at
the cap**: `cw-2026-08-17-134712`, `unadjudicated/cw-2026-08-23-001831`,
`unadjudicated/cw-2026-08-25-011552`, `unadjudicated/cw-2026-08-25-021410`,
`unadjudicated/cw-2026-08-31-003212`, `unadjudicated/cw-2026-08-31-003408`. **None of the six is
in the set**, so 3.1 does not depend on them. The *Test host process crashed* line in both
outputs is the kill, not HM-OPEN-063.

## 2. The 51 names

In the order of `docs\unit239-failing-set.txt`, the prefix `Hamlet.RadioEngine.Tests.Cw.`
dropped. Numbers are verbatim from the console. **30 green, 0 red-repaired, 0 red-retired, 21
red-open, 0 unmeasured.** Every red-open asserts a decode result on audio (characters, share or
speed), so each is repaired one at a time in a later unit under R49 and none can be retired
(R49's second sentence, 3.3).

| # | Name | Type | Class | Number | Reason |
|---|---|---|---|---|---|
| 1 | `ASubMinimumBlipInAGapChangesNothingAfterIt` | `ABlipDoesNotShiftEverythingAfterItTests` | red-open | not run: file excluded by `<Compile Remove>` | asserts **characters**: generates `CQ DE W1AW K` at 18 wpm and asserts the text read with a 10 ms blip equals the text read without; not retirable under R49's second sentence; see the note below the table |
| 2 | `ARecordingWithKeyingInItIsReadTests.WhereTheTrackerStartsDoesNotDecideThis(startHz: 500)` | `ARecordingWithKeyingInItIsReadTests` | green | passed | - |
| 3 | `...WhereTheTrackerStartsDoesNotDecideThis(startHz: 550)` | `ARecordingWithKeyingInItIsReadTests` | green | passed | - |
| 4 | `...WhereTheTrackerStartsDoesNotDecideThis(startHz: 600)` | `ARecordingWithKeyingInItIsReadTests` | green | passed | - |
| 5 | `CapturedSignalTests.TheSignalReadsAsStrongAsItIs(name: "cw-2026-08-17-134712")` | `CapturedSignalTests` | green | passed | - |
| 6 | `CwAcquisitionWindowTests.AFastFistIsReadWithoutARunUp(wordsPerMinute: 25, floor: 0.79)` | `CwAcquisitionWindowTests` | red-open | *25 words a minute tuned onto mid-transmission came back 0.75 of the message against a bar of 0.79* | asserts **share** of the message |
| 7 | `...AFastFistIsReadWithoutARunUp(wordsPerMinute: 28, floor: 0.79)` | `CwAcquisitionWindowTests` | green | bare 0.95 | - |
| 8 | `...AFastFistIsReadWithoutARunUp(wordsPerMinute: 30, floor: 0.79)` | `CwAcquisitionWindowTests` | green | bare 0.88 | - |
| 9 | `...AFastFistIsReadWithoutARunUp(wordsPerMinute: 35, floor: 0.78)` | `CwAcquisitionWindowTests` | green | bare 0.89 | - |
| 10 | `...TheSameFistWithARunUpDoesNot(wordsPerMinute: 25)` | `CwAcquisitionWindowTests` | green | run-up 0.95 | - |
| 11 | `...TheSameFistWithARunUpDoesNot(wordsPerMinute: 28)` | `CwAcquisitionWindowTests` | green | run-up 0.84 | - |
| 12 | `...TheSameFistWithARunUpDoesNot(wordsPerMinute: 30)` | `CwAcquisitionWindowTests` | green | run-up 0.88 | - |
| 13 | `...TheSlowEndReadsTheMessage(wordsPerMinute: 10, snrDb: 18)` | `CwAcquisitionWindowTests` | green | run-up 1.00 | - |
| 14 | `...TheSlowEndReadsTheMessage(wordsPerMinute: 10, snrDb: 3)` | `CwAcquisitionWindowTests` | green | run-up 0.98 | - |
| 15 | `...TheSlowEndReadsTheMessage(wordsPerMinute: 12, snrDb: 18)` | `CwAcquisitionWindowTests` | red-open | *12 words a minute at 18 dB came back 0.63 of the message*, bar 0.66 | asserts **share** of the message; the same speed at 6 dB and 3 dB is green at 0.96 and 1.00 |
| 16 | `...TheSlowEndReadsTheMessage(wordsPerMinute: 12, snrDb: 3)` | `CwAcquisitionWindowTests` | green | run-up 1.00 | - |
| 17 | `...TheSlowEndReadsTheMessage(wordsPerMinute: 12, snrDb: 6)` | `CwAcquisitionWindowTests` | green | run-up 0.96 | - |
| 18 | `CwDisplacementFloorTests.AStationElsewhereIsStillFound(toneHz: 400)` | `CwDisplacementFloorTests` | red-open | read `■ ■■ ■`, expected to end `CQ DE W1AW K` | asserts **characters** |
| 19 | `...AStationElsewhereIsStillFound(toneHz: 500)` | `CwDisplacementFloorTests` | red-open | read `■ ■■ ■`, expected to end `CQ DE W1AW K` | asserts **characters** |
| 20 | `...AStationElsewhereIsStillFound(toneHz: 750)` | `CwDisplacementFloorTests` | red-open | read `■ ■■ ■`, expected to end `CQ DE W1AW K` | asserts **characters** |
| 21 | `...AStationElsewhereIsStillFound(toneHz: 875)` | `CwDisplacementFloorTests` | red-open | read `■ ■■ ■`, expected to end `CQ DE W1AW K` | asserts **characters** |
| 22 | `CwDisplacementFloorTests.NothingIsRefusedBeforeAnythingIsBeingRead` | `CwDisplacementFloorTests` | red-open | read ending `VIVVV E KCTCGQQ N DEDE E WWAJ11AARW W N K`, expected to end `CQ DE W1AW K` | asserts **characters** |
| 23 | `CwDisplacementFloorTests.TheTrackerDoesNotLeaveAStationForItsOwnImage` | `CwDisplacementFloorTests` | red-open | read `■ ■■ ■`, expected to end `CQ DE W1AW K` | asserts **characters** |
| 24 | `CwEmissionGateTests.NoSpeedIsNamedWithoutCharactersToNameItFrom` | `CwEmissionGateTests` | red-open | *real signal reported none wpm*; `Assert.NotNull` on the speed, then 14 to 24 | asserts **speed** on an 18 wpm generated signal; the noise half, no speed from noise, held |
| 25 | `CwFixtureTests.EveryRecordingGivesBackTheShareItShould(name: "clean-12wpm")` | `CwFixtureTests` | red-open | *clean-12wpm gave back 0 of 9, short of the 100% it has to manage* | asserts **share**; R53's synthetic |
| 26 | `...EveryRecordingGivesBackTheShareItShould(name: "clean-18wpm")` | `CwFixtureTests` | red-open | *clean-18wpm gave back 0 of 9, short of the 100% it has to manage* | asserts **share**; R53's synthetic |
| 27 | `...EveryRecordingGivesBackTheShareItShould(name: "fading-18wpm")` | `CwFixtureTests` | green | passed | - |
| 28 | `...EveryRecordingGivesBackTheShareItShould(name: "interference-18wpm")` | `CwFixtureTests` | green | passed | - |
| 29 | `...EveryRecordingGivesBackTheShareItShould(name: "noisy-18wpm")` | `CwFixtureTests` | green | passed | - |
| 30 | `...EveryRecordingGivesBackTheShareItShould(name: "prosigns-18wpm")` | `CwFixtureTests` | red-open | *prosigns-18wpm gave back 0 of 16, short of the 100% it has to manage* | asserts **share** |
| 31 | `CwFixtureTests.TheCleanRecordingsDecodeExactly(name: "clean-12wpm")` | `CwFixtureTests` | red-open | read `■ ■ ■ ■ ■  ■ ■ ■ ■■` against `CQ DE W1AW K` | asserts **characters**; R53, the floor synthetic, step 3's repair, not retirable |
| 32 | `CwFixtureTests.TheCleanRecordingsDecodeExactly(name: "clean-18wpm")` | `CwFixtureTests` | red-open | read `■ ■ ■  ■■■` against `CQ DE W1AW K` | asserts **characters**; R53, as above |
| 33 | `CwFixtureTests.TheProsignRecordingDecodesItsProsigns` | `CwFixtureTests` | red-open | `<BT>` not found in `■ ■ ■■ ■■■ ■■ ■ ■■■ ■■■ ■■ ■ ■■■ ■■■■■■ ■`··· | asserts **characters** |
| 34 | `CwLowDutyTests.AStationKeyedForAMomentReadsAsAStrongStation` | `CwLowDutyTests` | green | passed | - |
| 35 | `CwLowDutyTests.TheHeldFigureLetsGoWhenTheStationStops` | `CwLowDutyTests` | green | passed | - |
| 36 | `CwLowDutyTests.TheToneIsFoundWhereItActuallyIs` | `CwLowDutyTests` | green | passed | - |
| 37 | `CwRefiningRetuneTests.AHandoverToAnotherStationStillResets` | `CwRefiningRetuneTests` | green | passed | - |
| 38 | `CwRefiningRetuneTests.AMoveBeforeAnythingHasBeenReadIsAFollow` | `CwRefiningRetuneTests` | green | passed | - |
| 39 | `CwRefiningRetuneTests.TheSurveySettlingBetweenTwoBinsIsNotAStationChange` | `CwRefiningRetuneTests` | green | passed | - |
| 40 | `CwSurveyThresholdPinTests.TheToneInTheInterferenceCaptureIsStillFound` | `CwSurveyThresholdPinTests` | green | passed | - |
| 41 | `Fixtures.CwAdjudicationTests.ASpeedChangeInRealisticAudio` | `Fixtures.CwAdjudicationTests` | red-open | *no speed was ever named*; `Assert.NotEmpty` on the speeds | asserts **speed** across the two-station recording; also the known-reds block's line 158 |
| 42 | `Fixtures.CwReceiverFixtureTests.NothingIsEmittedDuringTheOperatorsOwnTransmission` | `Fixtures.CwReceiverFixtureTests` | red-open | *70 characters during the preamble, 134 in all* against 0; *own transmit measured: 9.1 s*, its over-3 s assertion held | asserts **characters** emitted inside the operator's own transmission, on `qsk-preamble.wav` |
| 43 | `Fixtures.CwReceiverFixtureTests.TheEasyTierIsReadWhole(name: "coverage-easy")` | `Fixtures.CwReceiverFixtureTests` | red-open | 5 characters unreadable, 37 not in the message, reads `VVEVSVVAW■11S■22E2H33E3S44E4H55NB6E6MZ77NO88T8MO99T9MO900■■SKRYNTTTTMEQUTOOA?TDDEENXITE/TTEENMO■00TKCCAAEARLLALL` against `1234567890QRZ?DE/N0CALL` | asserts **characters** |
| 44 | `...TheEasyTierIsReadWhole(name: "exchange-easy")` | `Fixtures.CwReceiverFixtureTests` | red-open | 3 unreadable, 21 not in the message, reads `VVEVSVVNKCECMZQQENCCTCMQQQ■■TNENMO■00TKCCAAEARLLALLTNENMOTTT00TKCCAAEARTEELAETEILNKKK` against `CQCQDEN0CALLN0CALLK` | asserts **characters** |
| 45 | `...TheEasyTierIsReadWhole(name: "tightfist-easy")` | `Fixtures.CwReceiverFixtureTests` | red-open | 1 unreadable, 3 not in the message (H, I, I), reads `VEVHVVTETEIESTSTTTDDEEETETEISTSTTTKKK■` against `TESTDETESTK` | asserts **characters** |
| 46 | `OneDecoderNotTwoTests.ListeningAndFeedingReadTheSame(name: "unadjudicated/cw-2026-08-23-001952")` | `OneDecoderNotTwoTests` | green | passed | - |
| 47 | `...ListeningAndFeedingReadTheSame(name: "unadjudicated/cw-2026-08-25-012922")` | `OneDecoderNotTwoTests` | green | passed | - |
| 48 | `...ListeningAndFeedingReadTheSame(name: "unadjudicated/cw-2026-08-28-005051")` | `OneDecoderNotTwoTests` | green | passed | - |
| 49 | `TheCapturesThatDecodeKeepDecodingTests.EachStillProducesWhatItDid(name: "unadjudicated/cw-2026-08-23-001520", ...)` | floor type | green | passed, task 0 | - |
| 50 | `TheCapturesThatDecodeKeepDecodingTests.EachStillProducesWhatItDid(name: "unadjudicated/cw-2026-08-25-013637", ...)` | floor type | green | passed, task 0 | - |
| 51 | `ThePitchCanBeHeldTests.UnlockingLetsTheTrackerSteerAgain` | `ThePitchCanBeHeldTests` | green | passed | - |

**By what the red-open ones assert:** characters 14 (#1, 18 to 23, 31 to 33, 42 to 45), share 5
(#6, 15, 25, 26, 30), speed 2 (#24, 41). Of the 21, **the two clean synthetics are also red on
share (#25, 26)** and the six `CwDisplacementFloorTests` cases and #24 decode generated audio, the
same kind of audio as the two synthetics; #42 to 45 decode the receiver tier's recordings.

**#1, the excluded file, read from source (decision 4).** All three facts of
`ABlipDoesNotShiftEverythingAfterItTests.cs` generate `CQ DE W1AW K` at 18 wpm through
`CwSignal.Generate`, decode it through `new CwDecoder(audio.SampleRate, 600)`, and compare
`decoder.Reading.Text`: `ASubMinimumBlipInAGapChangesNothingAfterIt` and
`ThreeBlipsChangeNothingEither` assert the text with blips equals the text without,
`ADitLongInjectionIsNoticed` asserts it differs. **Each reads audio and asserts characters, so the
file is red-open, not retired**, whatever it names. **Mismatch with the instruction and with
unit 392's table:** `CwReferenceDecoder` occurs in the file once, at line 30, inside a
`<para>` of doc-comment prose (*The other filter in the tree, `CwReferenceDecoder.Deglitch`...*),
not in code; prose in backticks does not bind. So the name unit 392 quoted is not what stops the
file compiling, or not alone. The code names `CwSignal.Generate`, `CwSignalRequest`,
`CwSignal.DefaultToneHz`, `BufferedAudioSource.PumpAll`, `CwDecoder.Listen`, `Flush` and
`Reading.Text`; every one but `Reading.Text` is used by a compiled engine test, and `Reading.Text`
is used elsewhere only by `AMoveStartsTheDecoderFreshTests.cs`, itself excluded. This unit opened
no file under `src` and did not build the file, so which name fails is **not measured**; the
repair is a rewire if the file compiles once that name is found, or waits on step 4.

## 3. The other cases the thirteen types carry

Step 3's context, not its criterion.

| Type | Cases not in the set | Green | Red | Unmeasured | Red with its number |
|---|---|---|---|---|---|
| `ARecordingWithKeyingInItIsReadTests` | 2 | 2 | 0 | 0 | - |
| `CapturedSignalTests` | 12 | 12 | 0 | 0 | - |
| `CwAcquisitionWindowTests` | 0 | - | - | - | - |
| `CwDisplacementFloorTests` | 0 | - | - | - | - |
| `CwEmissionGateTests` | 7 | 7 | 0 | 0 | - |
| `CwFixtureTests` | 14 | 11 | 3 | 0 | `NothingTheDecoderWasSureOfIsWrong` on `fading-18wpm` (*invented 'R', pattern [.-.], score 922.11*; *invented 'N'*, *'M'*; *said 'Q' where 'C' was sent*; *said 'E' where 'Q' was sent*; more), `noisy-18wpm` (*invented 'N', pattern [-.], score 17.94*; *'D'*, *'E'*, *'E'*, *'W'*; more), `interference-18wpm` (*invented 'N', pattern [-.], score 634.07*; *'D'*, *'E'*, *'E'*, *'W'*; more) - each asserts no confident character is wrong; the same method is green on `clean-12wpm`, `clean-18wpm` and `prosigns-18wpm` |
| `CwLowDutyTests` | 1 | 1 | 0 | 0 | - |
| `CwRefiningRetuneTests` | 0 | - | - | - | - |
| `CwSurveyThresholdPinTests` | 2 | 2 | 0 | 0 | - |
| `Fixtures.CwAdjudicationTests` | 10 | 10 | 0 | 0 | - |
| `Fixtures.CwReceiverFixtureTests` | 23 | 23 | 0 | 0 | - |
| `OneDecoderNotTwoTests` | 103 | 97 | 0 | 6 | none red; six `TheBufferSizeChangesNothing` cases unmeasured at the cap, section 1 |
| `TheCapturesThatDecodeKeepDecodingTests` | 35 | 35 | 0 | 0 | - |
| `ThePitchCanBeHeldTests` | 4 | 4 | 0 | 0 | - |

**The set's reds are not all of the types' reds:** `CwFixtureTests` carries three more, on
confident characters that are wrong, outside `docs\unit239-failing-set.txt`. Nothing else outside
the set is red.

## 4. The excluded files not in the set

From `docs\phase-cw\unit392-seams.md` section 9, each with the missing name unit 392 quoted.
**Not in 3.1's set; parked, section 9 of work instruction 394.** The instruction says twenty;
the table has **twenty-one** outside the set (twenty engine files and the one app file, twenty-two
less `ABlipDoesNotShiftEverythingAfterItTests.cs`), which is a mismatch reported, not repaired.

| Project | File | Missing name quoted by unit 392 |
|---|---|---|
| engine | `Audio/TheReadPathDoesNotAllocateTests.cs` | `CwKeyingMeter.WindowSizings` |
| engine | `Audio/TheTapIsNotBehindTheDecoderTests.cs` | `CwDecoder.ProcessDelayForTests` |
| engine | `Cw/AMoveStartsTheDecoderFreshTests.cs` | `CwDecoder.PitchWasAsserted`, `CwDecoder.Ranked` |
| engine | `Cw/AStationIsABinThatSwingsTests.cs` | `CwSwingSurvey` |
| engine | `Cw/EveryElementCarriesItsOwnPitchTests.cs` | `CwProbabilisticResult.Elements`, `CwElementPitch` |
| engine | `Cw/FittingKeyUpAgainstAssumingItTests.cs` | `CwProbabilisticDecoder.FittedLogLikelihoods` |
| engine | `Cw/IsTheHertzABiasOrAFloorTests.cs` | `CwSpectralPeak` |
| engine | `Cw/NoSenderIsSplitInTwoTests.cs` | `CwStreamSplit`, `CwProbabilisticResult.Elements` |
| engine | `Cw/NothingActsOnTheAdmissionVerdictTests.cs` | `CwDecodeReport` constructor parameter `PitchChoice` |
| engine | `Cw/TheCleanReadsStayCleanTests.cs` | `CwAccuracy` |
| engine | `Cw/TheFirstSecondsAreReadAgainTests.cs` | `CwProbabilisticStream.ReReads` |
| engine | `Cw/ThePeakAgainstASecondSignalTests.cs` | `CwSpectralPeak` |
| engine | `Cw/ThePeakFindsThePitchTheTrackerMissedTests.cs` | `CwSpectralPeak` |
| engine | `Cw/ThePosteriorSurvivesItsOwnArithmeticTests.cs` | `CwProbabilisticDecoder.Posterior`, `LogSum` |
| engine | `Cw/TheProbabilisticDecoderTests.cs` | `CwAccuracy` |
| engine | `Cw/TheQuietestBinNoLongerWinsTests.cs` | `CwPitchRanking`, `CwDecoder.RankThePitch` |
| engine | `Cw/TheReferenceDecoderIsPortedFaithfullyTests.cs` | `CwReferenceDecoder` |
| engine | `Cw/TheScoreSaysWhatItIsMeasuringTests.cs` | `CwAccuracy` |
| engine | `Cw/WhatDecodeScoringCostsTests.cs` | `CwToneTracker.CoarseSpacingHz` |
| engine | `Cw/WhereHamletAndTheReferenceDivergeTests.cs` | `CwProbabilisticResult.Elements`, 5-argument `Decode` |
| app | `Views/ThePitchControlsAreOffThePanelTests.cs` | `CwDecoder.AssertAt`, `CwDecoder.PitchWasAsserted` |

## Repairs and retirements

**None of either kind was made.** No red in the set has a wiring cause: every one fails on a
decode result the console printed, and repairing a decode result is a later unit's under R49. The
one test that cannot compile reads audio and asserts characters, so R49's second sentence forbids
its retirement. `docs\cw-retired-tests.txt` was not created.

## 5. Entry and exit

Both lines of `docs\carry-forward-tests.txt` as its comment says, one build each, a status line
before each; the three floor tests one invocation per type. Nothing under `src` changed between
them (`git diff --stat 02c64ae2 HEAD -- src` prints nothing), so the two floor runs cover every
commit of the unit (decision 8).

| Run | Entry, task 0 | Exit, task 4 |
|---|---|---|
| App line | 278 of 278 in 170 s | 277 of 278 in 172 s, 1 lost to the dispatcher loop; re-run once, 275 of 278 in 169 s, 3 lost the same way; each lost name green in the other run, no red on an assertion |
| Engine line, `timeout 480` | 176 of 176 in 375 s, 26 of them CW | 176 of 176 in 371 s |
| `TheCapturesThatDecodeKeepDecodingTests`, `timeout 900` | 37 of 37 in 97 s | 37 of 37, 92 s test time |
| `TheAdjudicatedReadingsKeepReadingTests`, `timeout 600` | 13 of 13, 30 s test time | 13 of 13, 30 s test time |
| `CwFixtureTests.TheCleanRecordingsDecodeExactly`, `timeout 300` | 0 of 2 in 5 s, `■ ■ ■ ■ ■  ■ ■ ■ ■■` and `■ ■ ■  ■■■` | 0 of 2 in 7 s, the same two readings; R53 |
| Eleven transmit files against `7e209cb4` | nothing | nothing |

The dispatcher-loop names at exit: `TheTestsStayOffTheNetworkTests.ThePlainFixtureTakesGeneralFromTheFixedAnswer`
in the first run; `ThePowerIsOfferedTests.TheOfferRendersAtHalfAndNothingMirrorsTheUsbModLevel`,
`ThePsk31OfferTests.TheOfferIsOneButtonAndItIsTheOneTheEngineNamed` and
`TheWindowHoldsBelowItsMinimumTests.TheWorkingPanelsScrollInsideThemselvesRatherThanCollapsing` in
the second; each threw *You've caused dispatcher loop* from `Dispatcher.ResetForUnitTests` before
any assertion. **No regression:** nothing green at task 0 is red at task 4. 3.5 is not ticked; it
is the step's exit.
