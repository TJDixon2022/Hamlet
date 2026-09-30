```
UNIT: 511 - complete at task 3 of 3, none dropped; task 3 removed nothing - 2026-09-30
UNIT GOAL: the blocks stay, the sender's own dits count, and the old decoder retires
LANDED: 1, 2, and 3 as far as its own rules allow (tag and section M; no file removed)
```

## 1. What Claude did

Claude Code on the development machine, branch `main`. The prompt claimed `PROJECT: Hamlet`; the order's gate held: `SHACK_FACTS.md`, `CwProbabilisticDecoder.cs` and `CW_REQUIREMENTS.md` exist, there is no `CoreHMI.sln` or `MURC.sln`, the root is `C:\Source\HamLet`, and `PROJECT_CARD.md` says Hamlet. Nothing in this report is evidence about the radio. Unit 511 and HM-DEC-215 were both free.

SESSION.lock was taken through `tools\arbiter\lock.bat take` and released at the end. Nothing was written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` was touched, no box was ticked, and no ruling was added to either plan. Scratch probes are under `.run-unit\` and not committed.

**Task 1: a block on the scroll stays on the scroll** (`e58435b1`).
- **Before.** The scroll's blocks were the watched bin's paired bars. `CwTrainingGraph.Update` threw away and rebuilt the last four seconds of them on every tick, and the watched bin and its pairing move. So a block drawn on one frame could be gone on the next.
- **`ViewModels/CwTrainingGraph.cs`.**
  - New `Stand(mark, heard, now)` keeps a mark as a block once, by its sequence number.
  - New `Trim(now)` takes blocks and letters away only by time.
  - The kept sequence numbers outlive a Clear, so a cleared block does not come back while it is still inside the window.
- **`ViewModels/CwScopeFeed.cs`.** Each tick offers every mark that stood at the pitch the decoder is printing and is still inside the window. So a sender's first marks, which stood before the reader started printing it, appear the moment it prints. When nobody is printed, nothing is drawn. `Update` stays for the control-level tests that build frames with it, but the live path no longer calls it.
- **`Hamlet.App.Tests/ViewModels/TheScrollKeepsItsBlocksTests.cs`**, sampled every 50 ms, with a block counted as still present when a later frame covers any of its time:
  - The 20 WPM call was already whole before the change (the order expected it red): 0 blocks went early.
  - The 5 WPM Farnsworth call was red: 10 blocks went before the edge. Now 0.
  - Loud noise draws no block.
- **Side effect.** In unit 509's Farnsworth case, every letter now sits over a block. The letters with no block under them that 509 reported are gone.
- The scope tests beside it pass, 25 of 25.

**Task 2: the sender's own dits count** (`3b6329bc`).
- **`Cw/CwPatternGate.cs`.** Once a sender stands, a candidate at its pitch passes if all of these hold:
  - its length is within √2 of the sender's dit or dah;
  - it is quieter than the sender's marks of the same kind (dits against dits, dahs against dahs) by more than the level tolerance and no more than twice it;
  - a gap to the mark before it or after it is inside a letter: under two dits and not under half a dit.
- **Timing.** Where the gap before already places it inside a letter, it stands at once. The first mark of a letter waits for the next mark.
- **Constants.** `QuieterShare` = 2 (the order's figure). `InsideLetterShare` = 2 and `LengthRatio` = √2 are the author's, overrulable.
- **Why "of the same kind".** This detector reads a dit a tenth or two of a decibel under a dah. The order's case measures 6.0x dB under against dits and dahs mixed, and 5.9 dB against dits.
- **`Cw/CwMark.cs`.** New `BySendersPattern` flag on a mark admitted this way.
- **`Cw/CwRunReader.cs`.** A mark with the flag is matched to its sender by pitch alone, because the gate has already judged its level against that sender. The sender's reference level leaves such marks out. Otherwise the quiet mark pulled the reference about 3 dB down, the sender's next ordinary mark missed it, and a new sender took over, which stopped the printing at `N0CAA`.
- **Correction to unit 510's measurement.** Its test helper started `quietRun` at `int.MaxValue`, so the counter overflowed on the first element and counted it as none. Its "first dit of the first L" was in fact the L's dah. Fixed.
- **`Cw/TheSendersPatternFindsItsMarksTests.cs`**, measured red first:
  - first dit of the first L 6 dB down: `CQ CQ DE N0CA DL N0CALL K`, now the call whole;
  - the L's dah 6 dB down (what unit 510 actually measured): `N0CAE IL`, now whole.
- **Every reading case against HEAD.** Eight synthetic reader classes: 51 of 54, the same three reds as HEAD (unit 507's 8 dB, 12 dB and 10 dB rows). Their printed readings match HEAD's except one: unit 502's weak call, `CGE N EQ DE N0CALL NT ON EAE IL A` → `... NT ON EALL A`, where one more mark stands (61 against 60). The two-station case and both noise cases are unchanged (section 3).
- The app cases through the reader pass, 16 of 16.

**Task 3: the old decoder retires** (`6ad21ceb`).
- **The tag.** HEAD is tagged **`before-cw-cleanup`** at `3b6329bc` and pushed.
- **Section M.** `CW_REQUIREMENTS.md` section M is marked superseded at its head, in one paragraph. The rows HM-REQ-120 to 129 are kept. No test parses the file.
- **Nothing was removed.** Rule 3 of the order is "if a retired piece is still called by something live, say so and leave it — do not refactor around it", and every piece the order names is still called:

| Piece | Built by | Superseded by | Still called by |
|---|---|---|---|
| `CwProbabilisticDecoder` (lattice, speed grid, emission gate), and `CwProbabilisticStream` around it | 1xx and after; the stream by the second-pass design (HM-DEC-096) | 493 (`ReadsRuns`) | `CwDecoder` builds and feeds it on every hop; `CwTrainingGraph`, `CwRunReader`, `CwCharacter` and `MainWindowViewModel` read its constants and members |
| `CwUnitEstimator` | the streaming estimator, before 493 | 493 | `CwProbabilisticStream` |
| `CwToneTracker` | 48 and after | 496, 507 | `CwDecoder`'s hop loop is driven by it (`Process`, `HopSamples`); the verdict row reads `trackerHz`; the decode report and capture sheet read its pitch and proof |
| `CwToneSurvey` | 95 and after | 496, 507 | `CwToneTracker`, `CwEnvelopeDetector` (its `ShortestDitMs` sets the shortest bar), `CwCompetitor` |
| `CwKeyingMeter` swing test, `ConfidentSwingDb` | 474 to 479 | 485 to 507 | `MainWindowViewModel` builds it and publishes its reading to the verdict row and the tracker |
| Unit 489's three switches (`DetectorSteersPitch`, `DetectorGatesKeying`, `DetectorGatesBlocks`) | 489 | 493 | `CwDecoder`; `TheScopeDrawsLiveTests` sets `DetectorGatesKeying` |
| The second decoder, `Cw/Second/Fldigi*`, `CwSecondReader`, `CwSecondReading`, `FldigiConfidence` | 456 to 461 | the shape approach (493) | **it runs live**: the app builds `CwDecoder` with `secondReader: true` (`MainWindowViewModel.cs` line 11571) |
| `CwArbiter`, `CwVoteTable`, `CwSwitchTable` (arbitration and calibration) | 462 to 467 | the same | `CwDecoder`; the capture sheet's `arbiter` line reads `CwArbitrationCase` |
| The mixdown path the tracker fed | before 496 | 496 | inside `CwDecoder`'s hop loop |

- **What could go but can't be deleted here.** `CwInterferenceNotes` (with `InterferenceFix`) is the one CW type that no source file calls. It came from `2fdfb349`, "name what is sitting in the passband", under HM-DEC-096 phase 5. It and `CwInterferenceNotesTests.cs` are listed for you to delete, because this session's `rm` and `git rm` are refused.

**Build and records.**
- Build `Hamlet.sln` with warnings as errors: RC=0.
- App carry-forward line: 278 of 278.
- Version 1.13.197 to 1.13.198.
- `PHASE_OUTCOME.md` (both copies): `## UNIT 511 - STEP 12`.
- `PHASE_STATUS.md` (both copies) names 511.
- `CLAUDE.md` §1 index row.
- `DECISIONS.md` HM-DEC-215, in full below. Its headline says the old decoder is *tagged for retirement* rather than *retires*, because nothing was removed; the entry says so.

> **The blocks stay, the sender's own dits count, and the old decoder is tagged for retirement.** The order named the headline *...and the old decoder retires*; nothing was removed, so the headline says what was done. Tim, 2026-09-30: *"The letters are solid, but the bars, the dashes and dots bars, tend to come and go."* And: *"We're running two decoders. We really don't need them both."* Task 2 answers unit 510's question from the pattern (R85), as the order directs.
>
> **The blocks.** A block that was drawn stays drawn until time carries it off the left. The scroll's blocks are the marks that stood at the pitch being printed, each kept once by its sequence, never reworked from the detector's live state; nobody printed, nothing is drawn.
>
> **The sender's own dits.** Once a sender stands, a candidate at its pitch, of its dit or dah length within √2, quieter than its marks of that kind by more than the level tolerance and no more than twice it, stands as its mark where a gap to the mark before or after it is inside a letter, under two dits; a letter's first mark waits for the next. The reader matches such a mark on pitch and leaves it out of its reference level. Between letters, between senders and at any other pitch the tolerance stands. The √2, the two dits and the kind-by-kind comparison are the author's, overrulable.
>
> **The old decoder.** HEAD is tagged `before-cw-cleanup` and section M of `CW_REQUIREMENTS.md` is superseded, its rows kept. **Nothing was removed**: every piece the order names is still called by live code - the decoder's hop loop runs on `CwToneTracker` and feeds `CwProbabilisticStream`, the app builds its decoder with the second reader on, the verdict row reads the tracker, and the capture sheet's arbiter line reads the arbitration types - and the order says to leave such pieces rather than refactor around them. Retiring them is a refactor of `CwDecoder` and its callers, and waits on an order that says so.

## 2. What the owner should expect

- **Rebuild.**
- **The scroll's bars.** The dots and dashes on the scroll no longer come and go. A block stays until it slides off the left, as the letters already did.
  - The bars now start at the moment the terminal starts printing a station. Its first few dits and dahs appear then, all at once, in their right places.
  - While nobody is being printed, the scroll draws no bars.
- **Quieter dits.** A strong station's quieter dit or dah inside a letter is now kept. On the bench a dit 6 dB down inside `N0CALL`'s L reads right, where it read `N0CA DL`, so `SEPTEMBER` should read `SEPTEMBER`. Nothing else about what reads changed on the bench, except one weak test call that now reads one more letter right.
- **The old decoder** is still in the tree. It is tagged `before-cw-cleanup`, so it can be reached by name once it goes. Nothing about what reads changed because of it.
  - It couldn't come out under this order's rules: the running decoder still passes its audio through the old tracker, the app still runs the fldigi second decoder in the background, and the verdict row and the capture sheet still read pieces of it.
  - Taking it out means rebuilding `CwDecoder` around the new path, which needs an order that allows that refactor.
- **Two files to delete by hand:** `src\Hamlet.RadioEngine\Cw\CwInterferenceNotes.cs` and `tests\Hamlet.RadioEngine.Tests\Cw\CwInterferenceNotesTests.cs`. Nothing else uses them.
- **Still red, as before:**
  - `DecisionLogOrderTests.EveryRulingAppearsOnceAndTheGapsAreTheKnownOnes`, on the index gaps 166, 182 and 189 to 210;
  - `VoiceTests.NoOperatorFacingStringUsesABritishSpelling`, on unit 450's two "centre"s;
  - unit 507's three strength reds.

## 3. What you should see

Task 1, every frame at 50 ms:

| Case | Before | After |
|---|---|---|
| `CQ CQ DE N0CALL N0CALL K` at 20 WPM | 0 blocks went before the edge (up to 37 on a frame) | 0 |
| The call at 5 WPM Farnsworth | **10** went before the edge (e.g. 03.275–03.330 on frame 143, gone on 144) | 0 |
| Loud noise, 30 s | no block | no block |

Task 2:

| Case | Before | After |
|---|---|---|
| The call at 24 dB, as sent | 72 candidates, 65 stood, 65 printed, whole | the same |
| First dit of the first L 6 dB down | 73 / 64 / 64, `CQ CQ DE N0CA DL N0CALL K` | 73 / 65 / 65, whole |
| The L's dah 6 dB down (unit 510's actual case) | `CQ CQ DE N0CAE IL N0CALL K` | whole |
| Two stations, the loud one at 625 Hz | 67 candidates, 65 stood, whole | the same |
| Two stations, the quiet one at 825 Hz | 20 candidates, 12 stood | the same: its marks don't join the loud one |
| 30 s of loud noise | 635 candidates, 0 stood, nothing printed | the same |
| 180 s of loud noise | 3,932 candidates, 80 stood, nothing printed | the same |
| Unit 502's weak call | 60 marks, `... NT ON EAE IL A` | 61 marks, `... NT ON EALL A` |
| 35 WPM at 10 dB (unit 510's task 7) | `CQ CQ DE N0CALL N0CALL K` | the same |

## 4. What's blocking us

Nothing blocks. The items:

1. **Retiring the old decoder needs an order that allows refactoring `CwDecoder`.** The work would be:
   - drive its hop loop from the envelope detector rather than `CwToneTracker`;
   - stop building the second reader in the app;
   - drop the capture sheet's `arbiter` line and the tracker fields of the verdict row, or re-source them.

   The table in section 1 lists every piece and what still holds it.
2. **Two files to delete by hand:** `CwInterferenceNotes.cs` and `CwInterferenceNotesTests.cs`.
3. **Unit 510's weak-dit figure was measured on the dah, not the dit.** Its report's `N0CAE IL` is the dah case, and its test helper is corrected here.
4. **The 20 WPM case of task 1 was already whole before the change**, so the owner's flicker at the radio is more likely the Farnsworth kind: the watched bin moving or re-pairing on a slow or weak sender. Your report at the radio is the test.

### Asks still outstanding

- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25, waiting on
  the owner; no change sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8
  record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, so nothing on it is ever
  revised, at the cost of seconds of lag. On the run path, now the only path to the screen, the
  terminal shows only settled text. The ask stands only for the timing-only path, which no longer
  reaches the screen; no change for it sits in the tree.
