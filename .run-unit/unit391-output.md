READ IN THIS ORDER.

A. The phase goal - CW decodes again. Step 0 is this unit's; steps 1 to 5 are
   not started. After this unit step 0 is done: all four criteria have their
   answer, and none was dropped.
B. Step 0's criteria, one line each, met or not:
   0.1 the table at HEAD - met: 52 cases, 20 red, every case with its numbers.
   0.2 the green commit - met: 07f0397a, 2026-08-21, 84 Cw commits before HEAD.
       It predates the 08-25 floors; see section 4 item 1.
   0.3 the red commit - met: 8e3ee277, 2026-08-21, both clean synthetics.
   0.4 the seams - met: 30 files, 145 file-and-type rows, each checked at 07f0397a.
C. The report last. Section 4 raises 15 items: unit 390's nine, carried, and
   six of this unit's own. None of them is in the way of a criterion in B.
   Item 1 decides where step 1 restores to, so it bears on step 1, not on step 0.

UNIT:       391 - complete at task 3 of 4 (tasks 0 to 3, none dropped) - 2026-09-22 19:17
PHASE GOAL: bring Hamlet's CW receive back to the last point where it produced what its guards
            recorded, prove it with the floor tests, keep it guarded, and let Tim confirm it on the air.
UNIT GOAL:  put numbers on the break at HEAD, name the newest commit where all three floor tests
            were green and the commit that first turned one red, and list the seams step 1 crosses.
ADVANCED:   yes - step 0's four criteria each have a measured answer; no file under src changed.
NUMBER:     floor cases red at HEAD: unknown -> 20 of 52
DRIFT:      0

## 1. What Claude did

**Complete: tasks 0 to 3 of 4, none dropped.** Claude Code on QUIVERFULL, `C:\Source\HamLet`, on
`main`. The gate passed: `SHACK_FACTS.md` and `CwProbabilisticDecoder.cs` present, no
`CoreHMI.sln`, no `MURC.sln`. Commits `3d6a2c12`, `52789b1a`, `3c5d7e84`, `356eb86b` and the
report commit, each pushed; `main...origin/main` showed nothing ahead after each push.

- **Task 0, the record.** Version 1.13.77 to 1.13.78. `PROJECT_CARD.md` changed to
  `PHASE: CW decodes again` and `PHASE_SET: 2026-09-22`. HM-DEC-167 was written into
  `DECISIONS.md` verbatim above HM-DEC-166. The `CLAUDE.md` section 1 row went above HM-DEC-165,
  and `PHASE_OUTCOME.md` got `## UNIT 391 - STEP 0`. **Entry round:** both
  carry-forward lines ran unedited, one build each, with a status line before each: **app 278 of 278
  (2 m 46 s), engine 150 of 150 (5 m)**, both green on the first attempt.
- **Task 1, the floors at HEAD.** One type per invocation, filtered, with detailed console output
  and a status line before each: **captures 19 green / 18 red of 37, adjudicated 13 of 13 green,
  clean synthetics 0 of 2 (both read the empty string)**. No run was lost.
- **Task 2, the seams.** The grep scripts are under `.run-unit\`. 30 files name a Cw type, 145
  file-and-type rows, with members found by grep. Written to `docs/phase-cw/unit391-seams.md`.
- **Task 3, the walk.** I walked 85 commits touching `src/Hamlet.RadioEngine/Cw`, newest first,
  each in `C:/Source/HamLet-wt391` and removed after. 0.2 is `07f0397a` and 0.3 is `8e3ee277`.
  I then checked every seam member at 07f0397a. Written to `docs/phase-cw/unit391-walk.md`.
  **`git worktree list` at the end:**

```
C:/Source/HamLet                       3c5d7e84 [main]
C:/Users/TimDi/preflight-trees/206bd90 263949d7 (detached HEAD)
C:/Users/TimDi/preflight-trees/8e3ee27 07f0397a (detached HEAD)
C:/Users/TimDi/preflight-trees/f595938 351784ae (detached HEAD)
```

  `HamLet-wt391` is gone. The three `preflight-trees` were there before this session. I didn't make
  them and didn't touch them (section 4, item 5).

**Decisions I made for myself, in full:**

1. **Timeout.** The captures type's timeout went from the instruction's 900 s to 2700 s. The
   first run under 900 s was cut off at 18 of 37 cases (8 red, each matching the full run).
   The whole type took 1995 s. The instruction marked 900 s as the author's and overrulable.
2. **Walk order.** At each commit the walk ran the cheapest type first (the clean synthetics,
   about 10 s) and stopped at the first red type. The captures and adjudicated types ran only
   where the synthetics were green. A commit red on one type is red on the three, so this changes
   no answer, only the cost.
3. **Walking past 2026-08-24.** The task 3 fallback's condition held: no commit back to 08-24 was
   green on all three. I kept walking under section 4's *"If it is not there, say so and keep
   walking back"* and found an all-three green 34 commits further down. I named that for 0.2
   rather than the fallback's captures-only commit, because it meets criterion 0.2 as written.
   Section 4 item 1 puts the choice to Tim.
4. **One probe outside the walk.** I ran captures and adjudicated at `7e209cb4`, the commit that
   set the 08-25 floors, so step 1 has a second restore point on numbers.
5. **Overlap with the long runs.** The harness caps a foreground call at 600 s. The two long
   captures runs and the probe were moved to the background by it, or started there. While they
   ran I did grep work and the synthetics-only walk, which runs `dotnet test` in a second tree. I
   waited for completion with one bounded loop in a single call rather than repeated checks. This
   bends HM-DEC-155's *never background and poll*, and I'm saying so (item 4).

## 2. What the owner should expect

**Today 20 of the 52 floor cases are red.** That's 18 of 37 captures and both clean synthetics.
The 13 adjudicated readings all pass, but that's a count and not a verdict on the decoder. **The
decoder last kept all three floor tests green on 2026-08-21, at `07f0397a`, an hour before
`8e3ee277` replaced threshold decoding with the likelihood decoder.** From that commit on, the
clean `CQ DE W1AW K` came out as the wrong letters. From 08-25 it came out as unsure marks only.
From 09-03 (`43efc525`, the decoder fed from a queue) it came out as nothing, and it still does.
On the evening of 08-25 (`7e209cb4`) both capture-based tests were fully green (36 of 36 and 13
of 13), while the synthetics were already red. So "read on the air on 08-25" and "the floors were
green" are true of the captures, not of the synthetics. **There's no visible change in the
application.** Nothing under `src` changed. What looks wrong but isn't: 07f0397a is older than
the 08-25 evening the phase description names. That's the measurement, not a slip, and item 1
asks which point step 1 goes back to.

## 3. What you should see

**0.2: `07f0397a`, 2026-08-21 10:23 -0400, "feat(engine): give the gate its own analysis window".**
Green on all three there, as that commit had them: clean synthetics 2 of 2 exact; captures 5 of 5
(five recordings, character floors only); adjudicated not yet written (added 08-25 at `f96b21fb`),
so it counts green. **84 commits between it and HEAD touch `src\Hamlet.RadioEngine\Cw`.**

**0.3: `8e3ee277`, 2026-08-21 11:44 -0400, "feat(engine): decode CW by likelihood instead of by
threshold"**, 07f0397a's direct child. It turned red **`CwFixtureTests.TheCleanRecordingsDecodeExactly`
`clean-12wpm`** (read `ENCCTCMQQQ T DDEDE  A WWEWRJ11E1AAAWW W T...`) and **`clean-18wpm`** (read
`E KCTCGQ Q N DEDE E WWAJ11AARW W N K`), both against `CQ DE W1AW K`. Captures stayed 5 of 5
there.

### 0.1 - every floor case at HEAD

**TheCapturesThatDecodeKeepDecodingTests.EachStillProducesWhatItDid - 19 green, 18 red of 37, 1995 s.**
Diff is measured minus floor. Where the count floor is retired (an adjudicated anchor covers the
recording, Tim 2026-08-25), only elements are asserted. Unsure is printed, never asserted; the
bracket is what was marked when the floor was set.

| Capture | Result | Chars | Floor | Diff | Elements | Floor | Diff | Unsure (then) | Tone Hz |
|---|---|---|---|---|---|---|---|---|---|
| `cw-2026-08-17-013347` | green (count floor retired, anchor) | 58 | 59 | -1 | 108 | 108 | 0 | 21 (2) | 614 |
| `cw-2026-08-17-134712` | red (count floor retired, anchor) | 54 | 63 | -9 | 81 | 98 | -17 | 36 (10) | 501 |
| `cw-2026-08-18-004507` | green (count floor retired, anchor) | 50 | 50 | 0 | 119 | 118 | 1 | 2 (1) | 501 |
| `unadjudicated/cw-2026-08-24-012403` | green (count floor retired, anchor) | 24 | 22 | 2 | 71 | 65 | 6 | 4 (0) | 440 |
| `unadjudicated/cw-2026-08-22-031838` | red (count floor retired, anchor) | 33 | 57 | -24 | 116 | 126 | -10 | 6 (3) | 500 |
| `unadjudicated/cw-2026-08-22-031905` | green (count floor retired, anchor) | 37 | 42 | -5 | 120 | 118 | 2 | 5 (6) | 500 |
| `unadjudicated/cw-2026-08-22-031948` | green (count floor retired, anchor) | 31 | 34 | -3 | 119 | 114 | 5 | 0 (3) | 500 |
| `unadjudicated/cw-2026-08-22-032012` | red (count floor retired, anchor) | 43 | 44 | -1 | 119 | 120 | -1 | 5 (1) | 500 |
| `unadjudicated/cw-2026-08-22-032050` | red (count floor retired, anchor) | 49 | 53 | -4 | 115 | 123 | -8 | 9 (9) | 500 |
| `unadjudicated/cw-2026-08-22-032113` | green (count floor retired, anchor) | 48 | 55 | -7 | 126 | 118 | 8 | 11 (8) | 500 |
| `unadjudicated/cw-2026-08-22-032129` | green (count floor retired, anchor) | 43 | 66 | -23 | 123 | 119 | 4 | 8 (1) | 500 |
| `cw-2026-08-17-013622` | red | 53 | 55 | -2 | 86 | 84 | 2 | 23 (0) | 601 |
| `unadjudicated/cw-2026-08-18-003016` | red | 54 | 57 | -3 | 146 | 149 | -3 | 1 (3) | 669 |
| `unadjudicated/cw-2026-08-18-003126` | red | 53 | 54 | -1 | 142 | 144 | -2 | 8 (6) | 669 |
| `unadjudicated/cw-2026-08-18-003758` | green (count floor retired, anchor) | 61 | 63 | -2 | 123 | 121 | 2 | 19 (10) | 498 |
| `unadjudicated/cw-2026-08-23-001520` | red | 7 | 5 | 2 | 39 | 45 | -6 | 6 (1) | 600 |
| `unadjudicated/cw-2026-08-23-001831` | red | 53 | 55 | -2 | 124 | 124 | 0 | 18 (10) | 527 |
| `unadjudicated/cw-2026-08-23-001952` | red | 60 | 75 | -15 | 120 | 142 | -22 | 28 (13) | 521 |
| `unadjudicated/cw-2026-08-23-002016` | green | 75 | 75 | 0 | 136 | 136 | 0 | 34 (17) | 521 |
| `unadjudicated/cw-2026-08-25-011552` | green | 32 | 30 | 2 | 89 | 89 | 0 | 10 (8) | 500 |
| `unadjudicated/cw-2026-08-25-012748` | green | 2 | 2 | 0 | 4 | 4 | 0 | 0 (0) | 400 |
| `unadjudicated/cw-2026-08-25-012823` | red | 35 | 41 | -6 | 57 | 62 | -5 | 27 (15) | 500 |
| `unadjudicated/cw-2026-08-25-012922` | red | 44 | 50 | -6 | 111 | 112 | -1 | 11 (5) | 492 |
| `unadjudicated/cw-2026-08-25-013010` | green | 56 | 54 | 2 | 132 | 131 | 1 | 10 (6) | 501 |
| `unadjudicated/cw-2026-08-25-013150` | red | 61 | 58 | 3 | 132 | 139 | -7 | 23 (7) | 501 |
| `unadjudicated/cw-2026-08-25-013303` | red | 52 | 54 | -2 | 141 | 146 | -5 | 14 (10) | 501 |
| `unadjudicated/cw-2026-08-25-013402` | red | 59 | 61 | -2 | 154 | 161 | -7 | 10 (5) | 536 |
| `unadjudicated/cw-2026-08-25-013520` | green | 62 | 60 | 2 | 155 | 153 | 2 | 8 (5) | 536 |
| `unadjudicated/cw-2026-08-25-013637` | red | 62 | 63 | -1 | 158 | 164 | -6 | 13 (3) | 536 |
| `unadjudicated/cw-2026-08-25-021410` | red | 40 | 47 | -7 | 97 | 99 | -2 | 6 (11) | 540 |
| `unadjudicated/cw-2026-08-25-021629` | red | 26 | 47 | -21 | 71 | 96 | -25 | 10 (20) | 504 |
| `unadjudicated/cw-2026-08-25-021825` | green | 61 | 41 | 20 | 94 | 74 | 20 | 39 (16) | 394 |
| `unadjudicated/cw-2026-08-26-125941` | green | 0 | 0 | 0 | 0 | 0 | 0 | 0 (0) | 404 |
| `unadjudicated/cw-2026-08-20-014854` | green | 0 | 0 | 0 | 0 | 0 | 0 | 0 (0) | 608 |
| `unadjudicated/cw-2026-08-20-014935` | green | 0 | 0 | 0 | 0 | 0 | 0 | 0 (0) | 742 |
| `unadjudicated/cw-2026-08-22-014113` | green | 0 | 0 | 0 | 0 | 0 | 0 | 0 (0) | 607 |
| `unadjudicated/cw-2026-08-22-014308` | green | 0 | 0 | 0 | 0 | 0 | 0 | 0 (0) | 606 |

**TheAdjudicatedReadingsKeepReadingTests - 13 of 13 green, 364 s.** The eight required anchors
were all found: `VA3VRR`; `MP/4 QNIK` (reads `AA4MP/4 QNIK` whole); `DE KD0UN KD0UN K`;
`N HANDLING THIS MESSAG`; `, AND`; `110, AND 110 WITH A MEAN OF 117`; `R OTHER WEBSITES MENTI`;
and the fact `TheShortfallIsPrintedRatherThanPapered`. The five retired readings passed as
retired; two of their anchors appear (`INT`, `OPAGATION`) and three don't (`N4`, `DICTED 10.7`,
`ULLETIN CAN BE FO`).

**CwFixtureTests.TheCleanRecordingsDecodeExactly - 0 of 2, 10 s.** `clean-12wpm` and `clean-18wpm`
both read `""` against `CQ DE W1AW K`.

**Against `docs\unit239-failing-set.txt` (2026-09-03):** it lists 2 capture cases (`001520`,
`013637`) and both synthetics, all four red here. **The other 16 red capture cases are not on
that list.** That's a finding, not chased (section 9).

### Every walked commit, in one line each

In `docs/phase-cw/unit391-walk.md`, 85 rows. The clean synthetics were red at all 84 commits
above 07f0397a, in three stages: wrong letters from `8e3ee277` (08-21), unsure marks only from
`07260a2a` (08-25), and nothing from `43efc525` (09-03). **Probe at `7e209cb4` (08-25 14:54, the
floors' own commit):** captures **36 of 36** in 97 s, adjudicated **13 of 13** in 37 s; the
synthetics are 0 of 2 at `ca252057`, the same decoder source. Whether any newer commit is green on
captures was not measured.

### 0.4 - the seams

30 files have a `using Hamlet.RadioEngine.Cw` and name one of the 102 types declared under
`src\Hamlet.RadioEngine\Cw`: 9 under `src\Hamlet.App` and 21 under `tests\Hamlet.App.Tests`.
Twenty-seven files matched only the word `Outcome` with no Cw using, and `AchievementsViewModel.cs`
named Cw types in a comment. I dropped those 28 as name-only.

**What doesn't exist at 07f0397a**, from the right-hand column below. 12 rows name a type absent
there, 4 of them the ambiguous `Outcome`. The real gaps are all in `MainWindowViewModel`,
`ScanViewModel` and two app tests:
- **Types absent:** `CwProbabilisticDecoder`, `CwProbabilisticStream`, `CwElementPitch`,
  `CwPitchChoice`, `CwStreamSplit`, `Envelope` and `CwAccuracy.Outcome`.
- **`CwDecoder` members absent:** `AssertStation`, `AssertAt`, `Lock`, `Unlock`, `IsLocked`,
  `LockedToneHz`, `PitchWasAsserted`, `Ranked`, `Reading`, `Retuned`, `LeadingEdge`,
  `ListeningAfresh`, `DigitalMode`, `DecodingSuspended`, `RadioIsTransmitting`,
  `DecodeQueueDroppedChunks` and `DecodeQueueDroppedSamples`.
- **`CwCharacter` members absent:** `MarginLlr`, `MarginShareForRecord`,
  `SpanLogLikelihoodRatio` and `WidestRecordedLlr`.

Every transmit-side type the app names (`CwTransmitter`, `KeyerCwSender`, `TransmitChain`,
`TransmitReadiness`, `TransmissionWatch`, `TransmitNotes`, `ICwSender`, `AutoCall*`) exists at
07f0397a with every member listed. The members column is a grep, not a compiler (method in the
doc's head); step 1's build is the real check.

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

## 4. What's blocking us

**Nothing blocks step 0. Item 1 decides where step 1 restores to, and step 1 can start on the
commit named here unless you rule otherwise.**

**1. Which commit step 1 restores `src\Hamlet.RadioEngine\Cw` to.** *A ruling request; not blocking.*
0.2 as written is `07f0397a` (08-21). It's the only commit green on all three, but it's from
before the 08-25 evening, the adjudicated anchors, and 32 of the 37 capture floors. Whether it
meets today's floor table is unknown until step 1 runs it. `PHASE_PLAN.md` §6 and task 3 give a
fallback for this case: the newest commit green on the captures type alone. I didn't walk for
that. One probe shows `7e209cb4` (08-25) green on captures 36 of 36 and adjudicated 13 of 13,
with the synthetics red.

| | Restore to | For | Against |
|---|---|---|---|
| **A** | `07f0397a` (08-21) | Green on all three; the synthetics decode exactly; the literal 0.2 | Loses 08-22 to 08-25; today's captures and adjudicated tables were never run against it; 7 app-facing types and 21 members are missing, so more seams |
| **B** | the newest commit green on captures alone, found by one more unit's walk from HEAD down (at least as new as `7e209cb4`) | Keeps the decoder the floors were set on; two of three tests green; fewer seams | Synthetics red from the start; R49 forbids retiring them, so step 1.3 can't be met on the synthetics without a repair |
| **C** | `7e209cb4` directly | As B, with no further walk | Not proven the newest green; commits after it may hold more |

**Industry standard:** A. You go back to the last build green on the whole guard set, and
re-apply from there (R48, R51). **My recommendation, author's:** A for step 1. The first thing
step 1 measures is today's three floor tests against 07f0397a's decoder, and if the captures
floors fail there, B's walk is the next unit.

**2. Section 5 mismatches:**
- **The failing set has 2 cases of `EachStillProducesWhatItDid`, not six** (`001520`, `013637`).
  The two clean synthetics are there as stated.
- **The floor table has 37 rows.** The class's own remarks say "thirty-six here".
- **The Cw source changes are not "2026-08-28 to 08-31 and once on 09-03".** `git log` since
  08-24 shows 51 commits on every day from 08-24 to 08-31, and **four** on 09-03 (`43efc525`,
  `865e66d8`, `9c2a7f99`, `1a84188e`).
- **The green commit isn't between 08-25 and 08-28.** Nothing since 08-24 is green on all three.
- **`PHASE_PLAN.md` §6's fallback reads "since 2026-08-25";** task 3's reads "back to 2026-08-24".
- **HM-DEC-166 has no row in `CLAUDE.md` §1.** The top row before this unit was HM-DEC-165. I
  added HM-DEC-167's row only.
- **Held as stated:** `PROJECT_STATUS.md` read unit 390; `Directory.Build.props` read 1.13.77;
  every one of the 37 captures is on disk; the three tests exist by those names with 37, 13 and 2
  cases. The known-reds block carries two CW entries: `CwAdjudicationTests.ASpeedChangeInRealisticAudio`
  and "the 51 CW cases in docs/unit239-failing-set.txt".

**3. The captures type takes 1995 s at HEAD, against 97 s at `7e209cb4`.** *An indication, one
run each; it bears on step 2.* Criterion 2.3 measures the guard against the engine line's
`timeout 480`. At today's speed the whole type can't go on that line. §6 already rules that
a test over 300 s never does.

**4. HM-DEC-155, bent and said so.** The harness caps a foreground call at 600 s, so the two
long captures runs and the probe ran in the background. I waited with one bounded loop per run.
The synthetics-only walk overlapped the second HEAD captures run in a second tree, which only
costs time: results were identical case for case against the first run. If the rule should bind
here as written, a type over 600 s needs a different runner.

**5. Three worktrees under `C:/Users/TimDi/preflight-trees/` were there before this session.**
One is at `07f0397a`, the commit named for 0.2. I didn't make them and didn't touch them. By the
instruction's own reasoning they're "a second tree the next unit can edit by mistake".

**6. `PHASE_PLAN.md` 0.1 to 0.4 are not ticked.** The instruction didn't ask me to; the judge
ticks them.

### Asks still outstanding

**Carried per HM-DEC-139, verbatim, from unit 390. None is CW, none is this phase's, and none is
this unit's to answer.**

**Nothing blocks. Items 1 to 3 are ticks you may want to reverse; the rest are findings.**

**1. 0.1 is ticked on this instruction's word, which overrules instruction 386 section 6 ruling 1 ("never ticked by a unit of this phase").** *A ruling request if you disagree.* The later instruction wins under `PHASE_PLAN.md` §6, and both were the author's and overrulable. The tick says what is true: no such commit exists (119 commits, 0 on a settings path). **Option A:** keep the tick as *met as a negative*. **Option B:** untick it and reword 0.1 so a completed negative meets it. **Industry standard:** B, because a criterion should be met by its own words. A was taken because the instruction said so.

**2. 9.2 and 9.4 are ticked "as built", and their words don't all match what was built.** *Findings; rewording is yours.*
- **9.2** says *Report, Confirm, the canned lines and the typed line are held - greyed with he is still sending - until his carrier drops*. As built, it would read: *the typed line is greyed with he is still sending, Report and Confirm are not offered and the canned lines give way to one note, until his hand-back or his carrier drops*.
- In 9.2 and 9.4, *replayed from the ... record* would read *replayed from a fixture built to that record's shape*. Your `2026-09-21.jsonl` is not on this machine.

**3. 10.3 is ticked with one case under the band's full height:** 327 × 178 at 1400 on PSK31 with the dial off 14.070, where the card also has to say *PSK31 lives at 14.070*. The instruction stated this case and asked for the tick. Untick it if the case matters to you.

**4. Tonight's ruling A and the 2026-09-07 read-only ruling are in no decision record.** *A mismatch against section 5, which says "HM-DEC- number: find it".* **The 2026-09-07 ruling has no HM-DEC number.** It lives only in `LogContactViewModel`'s remarks and the dialog's markup comment. Ruling A is now recorded in `PHASE_PLAN.md` 9.3, the code remarks and the commit. Neither is in `DECISIONS.md`, and I did not assign an id (§4.8).

**5. 10.6's ticked words (*one control reading Favorites*) now describe the drop-down this unit replaced.** Rewording is yours.

**6. `src\Hamlet.App\Controls\FavoritesDropDownControl.cs` should be deleted.** Nothing places it. It stays only because `Unit388TraceTests` names the type and `rm` is refused here. It is marked as off the window in its own remarks.

**7. Section 5 mismatches:**
- **7.5 was already `[x]`** although the instruction says it was never built. The tick was early (task 1's finding).
- **At 1100 × 780 the map is 246 × 134 in a band of 402**, not the 327 × 178 stated for "under 1400 wide". It is asserted as measured in `TheSunMapStandsWhereItWasLeftTests.AtTheSizeItOpensAtItKeepsTheMockupsSize`.
- **Unit 389's 9.2 finding holds:** 1 of 4 controls is greyed, and the hold ends at the hand-back.
- **The RST prefill and the words list:** `docs/RADIO_SHEET.md` quotes none of the strings changed tonight. I checked by running its test, which stayed green.

**8. The favorites row scrolls sideways with the bar hidden, and I haven't verified wheel scrolling.** *A finding.* At 1400 roughly one chip is in view beside the map's caption. If the hidden row doesn't respond to the wheel, a small arrow or a count is a one-unit follow-up.

**9. The `RULES_AT` split, the thirteenth unit running.** `PROJECT_STATUS.md` reads `HM-DEC-165 (2026-09-19)` because `tools/status.sh` writes it as a literal, while `CLAUDE.md` holds `CPS-DEC-0165`. `tools\` is not mine to edit.

**`validate-output.bat`:** not run; it asked for approval in earlier units. Hand-checked against its six rules: `UNIT:` above section 1; four sections in order with exact names; no fifth; section 4 present; section 3 not empty; the ordering block above `UNIT:` with A, B, C and a count.

#### Asks still outstanding

Carried per HM-DEC-139. **Unit 389's item 2, 9.3's conflict with the 2026-09-07 ruling, is answered by your ruling A of 2026-09-22 and dropped.** The instruction's section 3 also answers 7.5 and the favorites; neither was on the queue.

**Unit 389's own, verbatim (items 1 and 3 to 7):**

**1. 9.2 falls short in two places, and both are on the send path, so they were left.** *A finding with numbers. Changing either needs a licence, because each changes whether a send control is enabled.*
- **Only 1 of 4 controls is greyed.** Report and Confirm are withheld by R1 mid-over, and the canned lines become a note.
- **The hold ends at his hand-back, not when his carrier drops.** Unit 385 measured that holding to the drop would refuse every answer for 9 s after a PSK31 `K`, and 22.94 to 38.23 s after an Olivia one.

Unit 385's claim was *"3 of the four controls held with one sentence and the fourth already withheld by R1"*. Measured, all four are held, three carry the sentence, and one is greyed.

**3. Neither replay is the record, and 9.4's is not what you got.** *A finding. Its remedy needs your file, or a licence to open the engine.* Your `2026-09-21.jsonl` is not on this machine. Unit 385's item 2 measured that `Psk31MessageSplitter` completes nothing from a garbled over, and that site is under `src\Hamlet.RadioEngine\Psk31\`.

**4. 10.3 at 1400 on PSK31 with the dial off 14.070 falls back to stage A.** *A finding. Whether it blocks 10.3 is a judging session's reading.* The card also draws *PSK31 lives at 14.070; you are at 14.074*, and at 401 px wide that no longer fits in 178 px. Any fix would cost one of three things you ruled on: the map's full height, the rig's width, or the strayed line. On 14.070 the same window stands at the left edge. The fallback is what keeps that line on the screen.

**5. The card's green word and the row's *sending* can disagree for a few seconds.** *A finding.* The card follows the hold, which ends at his hand-back. The row follows his carrier. For the 9 s (PSK31) to 38 s (Olivia) tail after a `K`, his row still says *sending* while his card no longer does.

**6. *Your turn?* now stands beside the unchanged *His turn, a guess*.** *Author's. It is one line either way if you want them to match.* R14 held me to the one word 9.4 names.

**7. `validate-output.bat` - see the last line of this section.**

**The older queue, carried by reference as units 385 to 389 did:** the whole of *"Unit 388's section 4, carried per HM-DEC-139, verbatim"* and *"Asks still outstanding - carried per HM-DEC-139, verbatim (unit 388's)"* in `output.md` at commit `cd18cc6c`, lines 270 to 506. That covers unit 388's seven; the sixty (unit 387's seven, unit 386's six, unit 385's eight, unit 383's ten); and the twenty-nine carried by reference from units 369 to 382. **None of them is answered tonight** except as follows. Unit 383's item 1 (4.3 partial under the strictest reading) and unit 386's item 6 (0.1 stays unticked) are overtaken by tonight's ticks, and items 1 and 3 above put both back to you.
