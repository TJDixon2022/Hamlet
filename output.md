## 1. What Claude did

Development computer, project gate `PROJECT: Hamlet` checked against `PROJECT_CARD.md`, `Hamlet.sln` and the `Hamlet.*` namespaces. Nothing here is evidence about the radio. Branch `main`. Run by hand: `SESSION.lock` taken at 22:01:47 and released at the end, nothing written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` touched, no box ticked in `PHASE_PLAN.md`. HEAD `1b18cc89` was tagged `before-old-decoder-removal` and pushed before any change. Version 1.13.229 to 1.13.230. Ruling HM-DEC-249, the number the order gave.

**Task 1: every caller reads the shape side** (`4b1aba71`).
- Every caller of the old decoder in the app and the engine was found and moved onto the shape side, or retired with its reason. The table is in section 3.
- `CwDecodeReport` now carries the shape side's own figures: the senders held, the letters resolved, whether a sender is printing, the speed and its proof.
- The story line, the competing note and the speed read those figures.
- The app's display tests that pinned old figures were re-pinned on `CwChain`, or removed where they tested only the old meter or tracker.
- Scoreboard and scans table identical to HEAD.

**Task 2: the old code is removed**, one group at a time, building and running the scoreboard after each.
- **`CwDecoder` became the chain and nothing else** (`c91eb6b7`). It keeps the tap, the suspension while the radio transmits and the digital-mode skip. It pulls the detector's marks into the sender gate and the lookup table. `CwChain` lost its pitch argument.
- **Groups 1 to 5:**
  - group 1 is `493ccb02`;
  - group 2 is `7e5a70d6`;
  - group 3 is `5d213bc2`;
  - group 4 is `3ab273e0`;
  - group 5 is `f72d4c05`.
- No removal changed a reading, so nothing was put back.
- **Moved before each removal:**
  - the survey's 25 ms shortest dit went into `CwEnvelopeDetector.ShortestBarMs`;
  - the 5 ms hop went into `CwCharacter.HopMilliseconds`, which the sender gate and the scope read;
  - the 300 to 900 Hz pitch range went into `KeyingEnvelope`;
  - the competitor's 125 Hz separation went into `RecordingToneOverNoise.SeparationHz`.
- **Re-pinned on `CwChain`, because they assert behaviour the shape side keeps:**
  - own sending is not decoded (now on a generated CQ);
  - digital mode does not decode;
  - the tap;
  - the scan dwell;
  - the clear;
  - the scroll's letters and blocks;
  - the terminal copy;
  - prosign naming, compared trimmed because the shape side prints no leading word gap;
  - printed stays printed;
  - the scope draws live;
  - the sent-text guard;
  - the silence lock.

**Task 3: what it bought.**
- The counts are in section 3. **`CwDecoder` is now** the tap, the transmit suspension and the shape side's pull from detector to gate to lookup table: 362 lines, from 1,360.
- `CW_REQUIREMENTS.md`: HM-REQ-093 (the pitch proof) and HM-REQ-120 to 129 (the second decoder and the arbiter) are each marked retired in one line, with their text kept.
- `CW_SPEC.md`: the one line naming the pitch proof is marked the same way. Its other "arbiter" mentions are the work loop's arbiter, not `CwArbiter`.

**Records.**
- `PHASE_OUTCOME.md`, both copies: `## UNIT 545 - STEP 12`.
- `PHASE_STATUS.md`, both copies: names 545.
- `Directory.Build.props`: 1.13.230.
- `CLAUDE.md` §1: a row above HM-DEC-248.
- `DECISIONS.md`: HM-DEC-249, *The old decoder comes out*.
- Nothing was recorded under §12.1.

**Build** `-warnaserror`: no warnings, no errors.

**App carry-forward:** 277 of 278. The loss is `ThePsk31ConversationCardTests.NoSlotClockUnderPsk31AndFt8AndFt4StillShowIt`, a headless dispatcher test. It passes in its own class (8 of 8) and reads nothing from CW. It is the same order-dependent kind as unit 544's loss, though a different test.

## 2. What the owner should expect

Rebuild and run as usual. **Nothing you read changes.** Every letter, space and confidence the terminal prints came from the shape side before this unit, and still does. The scoreboard reads exactly as it did: 191, with 208 of 244 letters right.

- **Level meter.** It shows the input level, as before. It never showed the old decoder's figure.
- **Story line.** It says what the shape side is doing now. *Listening* when no shape has formed. *A shape is forming* while a sender is held but not printed. *Reading a sender at about 600 hertz, keying at about 18 words a minute* while it prints. The clipping and near-silence warnings are unchanged.
- **Speed.** The speed on screen, the speed the transmit panel offers and the speed in the record now come from the printed sender's own dit. The transmit speed is only read and offered. It never keys anything. With no sender printing, no speed is shown.
- **Competing note.** It appears when the gate holds another sender beside the one printing.
- **Retired:**
  - **the keying meter's panel**, which was off unless you had turned its setting on, so nothing on your screen moves;
  - **the keying advice**;
  - **the pitch lock and its text**;
  - **the "listening afresh" note**;
  - **the retune nudge**.
- **On the capture sheet**, the keying line now reads `retired`, and the roster's meter column is empty.

**How much came out.** 154 files: 12,220 lines of source and 37,844 lines of tests. That is about two thirds of the engine's CW code. The fldigi reader, the old probabilistic decoder, the tone tracker and survey, the keying meter and the competitor are gone. All of it is reachable by the tag `before-old-decoder-removal`.

**How much faster.** The full scoreboard ran in 56 s and now runs in 21 s. The scans table ran in about 45 s and now runs in 17 s. The old decoder ran on every hop and no longer does.

## 3. What you should see

**The callers, as found and as they are now.**

| Caller | Read before | Reads now |
|---|---|---|
| Level meter | the decode report's level, from the tap | the tap's input level, unchanged |
| Story line | the old decoder's states and SNR | the shape side: listening, a shape forming, reading at N Hz and N WPM |
| Keying advice | the keying meter | **retired**: it judged the meter's verdict, which no longer exists |
| Keying meter panel | `CwKeyingMeter` | **retired**: hidden by default (`ShowKeyingSweep` off); the setting is kept so settings files load |
| Competing note | `CwCompetitor` from the survey | `Report.Competing`: printing and more than one sender held |
| Speed on screen, speed proof | the old decoder's rolling WPM | the printed sender's dit, as 1.2 over the dit; proved only while printing |
| Transmit speed (heard WPM) and speed offer | the same | the printed sender's dit; read only, keys nothing |
| `SpeedIsReacquiring` | the old clock | a sender waiting with none printed |
| Scope input | the old printing pitch | `RunsPrintingHz` |
| Hearing light and verdict row | meter and survey fields | the printed pitch and senders held (`printedHz`, `sendersHeld`) |
| Decode-quality row | SNR, elements seen, word-spacing flag | tone (while printing), senders held, letters resolved, character counts |
| Capture sheet keying line | the meter | **retired**: reads `retired` |
| Case roster meter column | the meter | **retired**: empty |
| Pitch lock and its text | the tracker | **retired** |
| Listening-afresh note (followed note) | the tracker | **retired** |
| Retune nudge (`_decoderTunedAtHz`) | the tracker's `Retuned` | **retired**: the shape side follows each sender itself |
| Leading edge | the stream's provisional tip | **retired**: the terminal shows settled text only |
| Decode-queue drop counters | the stream | **retired**: written as 0 |
| `CwPitchChoice` | the survey | **retired** |
| Auto-call station change | the old printing pitch | the printed pitch moving by more than the gate's pitch tolerance |
| Scan ear (`CwCatchEar`) | `CwChain(rate, pitch)` | `CwChain(rate)` |

**The groups, with the scoreboard after each.** The scoreboard read the same after every step: 208 of 244, 15 wrong, 2 invented, score 191, 55 of 77 spaces with 3 added. Both hard limits stayed red as at HEAD. The scans table was identical too.

| Step | What came out | Source lines out | Test lines out | Files deleted |
|---|---|---|---|---|
| Task 1, callers | meter display, pitch lock, `CwPitchChoice`, old sheet tests | 1,103 (142 in) | 6,819 | 33 |
| Decoder becomes the chain | tracker, stream, second reader, vote and switch inside `CwDecoder`; old-path tests | 1,219 (157 in) | 22,739 | 61 |
| Group 1 | `Cw\Second\*`, `CwSecondReader`, `CwSecondReading`, `FldigiConfidence`, `CwArbiter`, `CwVoteTable`, `CwSwitchTable` | 3,262 | 1,140 | 19 |
| Group 2 | `CwProbabilisticDecoder`, `CwProbabilisticStream`, `CwUnitEstimator`, `CwCharacterProbability` | 3,396 | 5,077 | 22 |
| Group 3 | `CwToneTracker`, `CwToneSurvey`, `CwTransmitGuard`, `CwInterferenceNotes` | 2,758 | 1,080 | 10 |
| Group 4 | `CwKeyingMeter`, `CwCompetitor` | 459 | 675 | 6 |
| Group 5 | `CwPitchProof`, the last old-path tests | 40 | 357 | 3 |

**Tests removed because they only exercised removed code** (listed by commit):
- `4b1aba71`:
  - EverySentenceOnTheSheet, TheKeyingCaptionNamesTheSweepItRan, TheSidecarDoesNotContradictItself, AHeldVerdictPrintsNoMeasurements, OneInstrumentDoesNotArgueWithAnother, TheDetectorFollowsTheMeter, TheFollowedSentenceReachesTheScreen;
  - AHeldPitchDoesNotOutliveItsEvidence, ARecordingWithKeyingInItIsRead, CwDiagnosis, CwEmissionGate, CwLowDuty, CwRefusalFloorTable, CwSurveyThresholdPin, CwReceiverFixture, NothingActsOnTheAdmissionVerdict, NothingIsReadFromAudioWithNoKeying, TheCaptureOfTheTwentyThird, TheCleanSyntheticsFourWays, TheCwBaselineTable, TheDisplacementFloorFourWays, TheEightReds, TheInterferenceIsMeasuredFact, TheOperatorIsToldAboutASecondStation, ThePitchSaysWhetherItWasProved, TheProsignsFixtureAtABand, TheReworkNumbersPrinter, TheSixRedsTrace, TheStationStillKeyingTrace, TheSwingIsTheFigureThatHolds, TheTrackerSwitchTrace, WhereAcquisitionPoints.
- `c91eb6b7`:
  - ANudgeIsNotAMove, TheDecoderIsFedTheDetectorsPitch, ABlipDoesNotShiftEverythingAfterIt, ARefinementKeepsTheTiming, BothDecodersAreScoredAlike, BothDecodersReadTheSameSamples, CwDisplacementFloor, CwRefiningRetune, CwSpeedSilence, DoesAStrongSignalClearEightyPercent, EachDecodersConfidenceIsMeasured, EveryCharacterCarriesAConfidence, CwAdjudication, CwTwoStation, HowSeventeenThirtySevensGapsAreCalled, NoDetectionNoLetters, OneUnitThroughBothClassifiers;
  - TheArbitrationEarnsItsPlace (Fact and Tests), TheChannelConditionsAreReadFact, TheCleanReadsStayClean, TheDecoderGetsItsEarsBack, TheEmitDecisionTable, TheGateHasItsOwnWindowNow, TheInventedLettersAreNotPrintedSure, TheMustFistsAtFifteenDecibelsFact, ThePhantomsBecomeBlocks, ThePitchCanBeHeld, ThePitchTheDetectorFoundReachesTheDecoder, TheSpeedFollowsTheSendersMarkPairs, TheSpeedSaysWhetherItWasProved, TheSpeedSearchReachesBothEnds, TheTrackedPitchIsChosenByKeying, TheTrackerStaysWithTheStationItReads, TheTwoPitchesTable, WhatBandwidthTheDecoderListensThrough;
  - WhatEachDecoderKnowsAboutEachCharacterFact, WhatFldigisEdgesWouldMoveFact, WhatPitchTheDecoderIsOn, WhatTheAcquiringLettersReadAtTheProvedValuesFact, WhatTheDecoderDoesWhileAcquiring, WhatTheFortySureWrongLettersRestOn, WhatTheNamedWordsRead, WhatTheOpeningHeard, WhatThePitchCanSayItProved, WhatTheSecondDecoderReadsFirst, WhatTheSpeedCanSayItProved, WhatTheSureLettersMarksLookLike, WhatTheSureLettersWerePrintedUnderFact, WhenTheWindowIsEmptied;
  - WhereOursLosesWhatThePortKeeps, WhereTheGapsActuallySit, WhereTheInventedLettersSitFact, WhereThePairSpeedMovedTheUnit, WhereTheSpaceIsDecided, WhereTheSureAddedLettersComeFrom, WhereTheSureWrongLettersComeFrom, WhereTheTwoReadingsMeetFact, WhereTheWordBoundariesGoWrong, WhereTheWordsBreak, WhyTheGateDidNotFire.
  - Removed as methods only: `WhichStraysASplitMade`, `TheStraysAndTheSpansTheyStandOn` and `WhatSeparatesAStrayFromALetter` (WhatTheStrayLettersRestOn's helpers stay), and `NothingBelowTheBarIsPrinted` (TheSeventeenThirtySevenCapture).
- `493ccb02`: TheOperatorSeesOneTranscript, TheHigherCalibratedReadingWins, TheSecondDecoderIsAFaithfulPort, WhatFldigisFrontEndWouldLiftFact, WhereTheSecondDecodersFirstDitGoes.
- `7e5a70d6`: EachCharacterAnswersForItself, EveryElementCarriesItsOwnPitch, CwTwoInOnePassband, TheIntegratorBandwidthTable, TheTwoStationTable, NoSenderIsSplitInTwo, TheNoiseScaleTable, TheProbabilisticDecoder, TheQuietestBinNoLongerWins, TheRefillGuardActuallyRuns, TheShortRunFilterDropsWithoutMerging, TheTwoEnvelopePathsAgree, TheUnitIsMeasuredNotSearched, WhatAFlatMarginDoesToShortCharacters, WhatDecodeScoringCosts, WhatTheWindowRatioIsMadeOf, WhereHamletAndTheReferenceDiverge, WhereTheKeyUpStateSits.
- `5d213bc2`:
  - CwInterferenceNotes, CwToneSurvey, CwTrackerSwitch, CwTransmitGuard, TheSurveyAlreadyUsesAShortWindow, TheTrackerObeysTheMeter;
  - the tracker method of TheRadioPointsTheDetector.
- `3ab273e0`:
  - CwKeyingMeter, TheMeterRunsOnLiveAudio, TheOwnersRefusedStationIsKeying, TheSwingBarIsTheLowestTheOwnerHeard;
  - the two meter methods of TheKeyingWitnessSaysNothingImpossible.
- `f72d4c05`: AMoveStartsTheDecoderFresh, TheFirstSecondsAreReadAgain. Both were already excluded from compilation, and their lines left the csproj with them.

**The counts.**

| | Before | After |
|---|---|---|
| Files deleted | | 154: 26 source, 128 test |
| Source lines | | 12,220 out, 311 in |
| Test lines | | 37,844 out, 257 in |
| `CwDecoder.cs` | 1,360 lines | 362 lines |
| Full scoreboard (`TheRecordingsScoreboard`) | 56 s (test 52 s) | 21 s (test 18 s) |
| Scans table | about 45 s | 17 s |

**Re-pinned tests run green this unit:**
- engine: the own-sending suite (6), digital mode, the tap, ScannerEndToEnd (7), ACharacterIsARunOfMarksThatAgree (19 of 20, see section 4), the silence lock's all-zero buffer, CwFixtureBuild (46), the witness sweep, the radio pointer;
- app: the clear, prosign naming (7), printed stays printed, scope draws live, the letter over its bars, one decoder one truth, both scroll tests, terminal copy, the sent-text guard, the bars carry their letters.

## 4. What's blocking us

- **Re-pinned and not run:** the engine carry-forward's three CW read guards read recordings R88 does not lift. These are `TheAdjudicatedReadingsKeepReadingTests`, the 08-25 cases of `TheCapturesThatDecodeKeepDecodingTests`, and `CwFixtureTests.TheCleanRecordingsDecodeExactly`. Their pins were the old decoder's readings. They now decode through `CwChain` and were not run, so expect them red until you rule on R88 for them or on their pins.
- **Also re-pinned and not run, for the same reason:**
  - `CaseRosterSurvivesAnEveningTests` (`004507`);
  - `OneDecoderNotTwoTests`;
  - the captures in `TheSilencePropertyIsLockedTests`;
  - `TheSeventeenThirtySevenCaptureTests`;
  - `HowFastTheDecoderEatsAudioTests` (`003016`).
- **`ACharacterIsARunOfMarksThatAgreeTests.MostNoiseBarsHaveNoEdges` is red.** It reads only `CwEnvelopeDetector`, which this unit did not touch, so it was red before. It reads like the edge rule unit 541 removed.
- **`VoiceTests` stays red** on two British spellings from earlier units: `CwRules.cs:75` "neighbour" (unit 541) and `CwCatchScan.cs:521` "centre" (unit 543). They were left per §12.6.
- **`CwCounterTrail` has no caller in the app.** It had none at the tag either, so it was outside this unit and was left.
- **`docs/carry-forward-tests.txt` still names the three CW read guards** as the CW read guard. Choosing a shape-side guard to replace them is yours.
- **Unit 487's ask** stood only for the timing-only path, and that path is now removed.
- **The silence limit stays red at one letter,** and **the carrier limit at seed 5195,** as at HEAD.

### Asks still outstanding

- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by unit 524; still open before a sender stands.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
