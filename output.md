UNIT:       600 - complete at task 5 of 5, task 4 dropped by the run budget as the order allowed (the trace, the plumbing, the measure, the end) - 2026-11-07 13:11
PHASE GOAL: Hamlet meets its CW requirements.
UNIT GOAL:  The net's per-letter probability reaches the scoreboard, the dimming line is measured at one quarter, one half and three quarters, and the counts are recorded; the line stays where it is and nothing Hamlet prints changes.
ADVANCED:   yes, task 3 (the measure). The dimming line has a number for the first time: on the twelve-plus-three, under one half 3 right / 2 wrong, at or over 301 right / 11 wrong.
NUMBER:     the dimming line at one half, none -> under 3 right / 2 wrong, over 301 right / 11 wrong; the twelve-plus-three 286 -> 286
DRIFT:      0 - this unit advanced the task it named

## 1. What Claude did

**Complete. Tasks 1, 2, 3 and 5 are done. Task 4 (the off-line set) was dropped whole: the order said to drop it if the run budget reads as forbidding it, and `UNIT_RUN_BUDGET.md` says an order that asks for more runs is refused (section 4).**
- **Where this ran:** Claude Code on the development computer, branch `main`, every push accepted. Nothing here is evidence about the radio.
- **The gate:** the prompt named `PROJECT: Hamlet`, and all four checks passed at `C:\Source\HamLet` (`SHACK_FACTS.md` and `src\Hamlet.RadioEngine\Cw\CwLearnedReader.cs` exist, `CoreHMI.sln` and `MURC.sln` don't). Hamlet confirmed.
- **Version and ruling:** 1.13.286 to 1.13.287, HM-DEC-305 as numbered.
- **The lock** was taken with the root named and released. Nothing was written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` was touched, no box was ticked in `PHASE_PLAN.md`.
- **Not touched:** the model, alphabet, feature recipe and vector files; the sequence reader, gap learning, marks, lanes, keyer, wild bank and boards; the draft truth; the dimming line; the panel. Nothing keys or transmits, nothing was written to the radio, no recording was added, removed or promoted, and no process I didn't start was stopped.
- **Commits:** `ffdf345a` task 1, `9c1eb6cf` task 2, `82545a69` task 3, and the end commit with the records.

**Task 1, the trace (nothing built):**
- **Where the probability is born.** `CwLearnedReader.cs:500`, `Probability(logits, frame, index)` (`:566`): the softmax over the net's output scores, read at the letter's index, at **the first output frame of the run** the greedy collapse emitted the letter on (`Commit`, `:448`, when the argmax changes to a letter). It is not the best or the mean of the run. It is the same figure the dimming line compares: `:526`, `score >= 0.5 ? High : Low`.
- **Where it dies.** It already rides the letter as `CwCharacter.Score`: `CwLearnedReader.Letter` -> `CwSymbol.Reading` -> `CwEnvelopeDetector.SequenceSymbols` (wired at `CwChain.cs:77`) -> `CwDecoder.PullRuns` -> `CwSenderGate.PrintSequence` -> `CwRunReader.Letter` (its `reading with { ... }` keeps it) -> `CharacterRead` / `RunRead`. **The first type that drops it is the scoreboard's `Printed`.** `Score` can't carry it alone, though: the sequence reader writes its own figure there (`CwSequenceReader.cs:1474`, the most doubtful mark's level between midpoint and key-down), so a reader of `Score` can't tell the two apart.
- **How the board scores.** `CwScorer.Within(decode, key, Exact)`, free ends on the printed side. `WrongAt` walks the alignment and returns the indices of substituted and inserted letters; only the counts were kept.
- **The silences.** `Board.SilencePrints`: every letter read from a board recording (not only stretch letters) whose span lies in a silence of 2 s or more on its keying map. Same `Printed` record.
- **The off-line set.** 46 engine classes whole, 3 in part, and 5 app classes; listed in section 4 with the filter.
- **Quick measure as shipped:** the twelve-plus-three 286 (302 of 353, 13 wrong, 3 invented), KM3STU 55 of 65, letters in silences 11, W1AW 2128, the first recording 26 of 27, the wild bank 3271. **Matches.**

**Task 2, the plumbing:**
- `src\Hamlet.RadioEngine\Cw\CwCharacter.cs`: `double? NetProbability { get; init; }`, null by default.
- `src\Hamlet.RadioEngine\Cw\CwLearnedReader.cs`: sets `NetProbability = score` on the letter's reading. `Score`, the confidence and the dimming are unchanged. The sequence reader is not touched, and its letters carry none.
- `CwRunReader` needed no change: its `with` keeps the field.
- `TheRecordingsScoreboardTests`: `Printed` gains `double? Probability`, filled from `NetProbability` in `ReadLive`. New `LetterMark` (Right, Wrong, Added, Free) and `MarksOf(printed, reference)`, the same alignment `WrongAt` reads. `Scored` gains `Marks`, set by the board.
- `TheW1awTableTests.Score`: its stretches carry `Marks` too.
- `CwWildHands`: `Read` fills `Probability`; `Score` gains `Kinds` and `Of`, the alignment it already computed.
- **Test added:** `TheLearnedReaderTests.TheProbabilityReachesTheScoreboard`, on the engine line. It reads the first recording through the chain: every learned letter carries a probability in [0, 1], and with `CwRules.Off(CwRules.LearnedReader)` every sequence-reader letter carries none.
- **Quick measure after:** 286, 11, 2128, 26 of 27, KM3STU 55 of 65, the wild bank 3271. **Identical to task 1.**

**Task 3, the measure:** `TheLearnedReaderTraces.TheDimmingLine`, Survey category, on no line, run once. Figures in section 3.

**Task 4: dropped** (section 4). No test was restated, pinned or left red, because none was run.

**Task 5, the end:**
- The survey, the engine line and the app line, once each. The survey is byte-identical to last unit's, so no Rerun line was added: the survey keeps one Rerun line, rewritten by a unit that changes its figures, not one per unit.
- Engine line **176 of 176** (175 and the new test), app line **278 of 278**, `DecisionLogOrderTests` 2 of 2.
- Records: `PHASE_OUTCOME.md` both copies (`## UNIT 600 - STEP 13`), `PHASE_STATUS.md` both copies, `Directory.Build.props` 1.13.287, the `CLAUDE.md` §1 row, `DECISIONS.md` HM-DEC-305, `docs\cw-scoreboard.md` (a `600` row in the board, W1AW, wild hands and wild bank tables, and a new table *The dimming line*), and a `CW_SPEC.md` §14 paragraph.

**Recorded: HM-DEC-305, in full as written to `DECISIONS.md`:**

> **HM-DEC-305 - The dimming line, measured.** The net's probability for each learned letter travels with the letter to the scoreboard, which now keeps it beside the board's right/wrong; nothing printed, dimmed or shown changed, the twelve-plus-three at 286 before and after. On the twelve-plus-three as the board scores it, letters under one half are wrong 2 of 5 and letters at or over one half are right 301 of 312; at one quarter, under 0 wrong of 0, over 304 right of 317; at three quarters, under 6 wrong of 28, over 282 right of 289. On the fist bank rows: under one half 99 wrong of 196, at or over 5213 right of 5317. On W1AW: under one half 0 wrong of 2, at or over 2131 right of 2135. Of the 11 letters printed in the board's silences, 0 sit under one half and are already dim. The line stays at one half: where a letter dims is what the display asserts, and that is the owner's, with these numbers in front of him. *Rejected:* moving the line in this unit to the best of the three - a line chosen by the unit that measured it is a line chosen on one board, and the owner has not seen it. *Rejected:* dimming by the board's right/wrong instead of the net's probability - the board knows the truth and the operator's radio does not; the display may only use what the net knows at the time.

The record also carries what the probability is, how the counts are taken, and the fuller figures in section 3.

## 2. What the owner should expect

- **Nothing looks different.** No letter Hamlet prints, no letter it dims, nothing on the panel changed. The twelve-plus-three is 286, W1AW 2128, the wild bank 3271, the survey identical.
- **The bright and dim letters are now counted.** Every learned letter now carries the net's probability to the scoreboard beside the board's own right or wrong for it, so the dimming line has numbers.
- **The suite:** builds clean; engine line 176 of 176, app line 278 of 278. Everything is pushed to `main`.
- **What will look wrong but isn't:** `TheDimmingLine` is on no line by design. The plain `600` rows on the scoreboard aren't `ruled` or `task` rows, so the guards still read the `599 ruled` rows, at the same figures.
- **The end run was interrupted, not lost.** Claude Code reported the run's shell stopped for low memory during the engine line (another project's tests were running on this machine). The shell carried on anyway and finished all three: the survey, the engine line, then the app line. Nothing was run twice.
- **Left as found:** untracked `model\`, `CwSequenceReader.cs.new` and `cases-*.txt`; uncommitted `PARKED.md`, `RUN_LEDGER.md`, `tools\go-claude\GoClaude.bat` and `.run-unit\denials.txt`. Not committed: the unit's scripts and notes under `.run-unit\` (`unit600-*.sh`, `unit600-*.ps1`, `unit600-task1.md`, `unit600-offline.txt`, `f600\`).

## 3. What you should see

**No change on the air or on any board; the twelve-plus-three 286 before and after.**

**The twelve-plus-three** (324 letters, 7 on free ends, 317 scored: 304 right, 13 wrong; every letter carried a probability):

| line | under: right / wrong | at or over: right / wrong |
|---|---|---|
| 0.25 | 0 / 0 | 304 / 13 |
| **0.50** | **3 / 2** | **301 / 11** |
| 0.75 | 22 / 6 | 282 / 7 |

**At one half, elsewhere:**

| set | scored | under: right / wrong | at or over: right / wrong |
|---|---|---|---|
| the fist bank, every row | 5513 | 97 / 99 | 5213 / 104 |
| row 1.6 | 1335 | 65 / 65 | 1160 / 45 |
| row 2.0 | 1391 | 11 / 17 | 1349 / 14 |
| row 2.5 | 1399 | 13 / 5 | 1361 / 20 |
| row 3.0 | 1388 | 8 / 12 | 1343 / 25 |
| W1AW | 2137 | 2 / 0 | 2131 / 4 |
| Ken N3AFS, 0-120 s | 73 | 2 / 4 | 52 / 15 |
| Bryan KB2UZK/M, 120-300 s | 138 | 1 / 3 | 125 / 9 |
| the first recording | 26 | 0 / 0 | 26 / 0 |

The fist bank at the other lines: at one quarter, under 2 / 11 and over 5308 / 192; at three quarters, under 553 / 174 and over 4757 / 29. W1AW at three quarters: under 76 / 2, over 2057 / 2.

**The 11 letters printed in the board's silences: none under one half, all bright.**

| recording | silence | letter | probability |
|---|---|---|---|
| `cw-2026-10-02-200157` | 7.3-9.6 s | `B` | 0.989 |
| `cw-2026-10-03-143906` | 7.9-13.7 s | `Q` | 0.983 |
| `cw-2026-10-03-143906` | 24.5-26.8 s | `E` | 0.791 |
| `cw-2026-10-03-143906` | 24.5-26.8 s | `E` | 0.642 |
| `cw-2026-10-03-143951` | 6.4-13.8 s | `I` | 0.945 |
| `cw-2026-10-03-143951` | 6.4-13.8 s | `C` | 0.514 |
| `cw-2026-10-03-143951` | 6.4-13.8 s | `E` | 0.985 |
| `cw-2026-10-03-144020` | 11.3-13.6 s | `W` | 0.999 |
| `cw-2026-10-03-221502` | 21.4-24.5 s | `I` | 0.699 |
| `cw-2026-10-08-121414` | 25.1-27.6 s | `E` | 0.891 |
| `cw-2026-10-08-121414` | 25.1-27.6 s | `C` | 0.986 |

**The twelve-plus-three by probability** (right / wrong): 0.0-0.3 none; 0.3-0.4 1 / 1; 0.4-0.5 2 / 1; 0.5-0.6 6 / 2; 0.6-0.7 7 / 2; 0.7-0.8 14 / 0; 0.8-0.9 30 / 2; 0.9-1.0 244 / 5.

**What this says about the letters on the panel:**
- **On real recordings almost nothing is dim, and the dim letters are not mostly the wrong ones.** On your own recordings 5 letters in 317 fall under the line, and 11 of the 13 wrong letters are shown bright. Most of the net's mistakes are confident ones: 5 of the wrong letters sit above 0.9.
- **The letters you saw between transmissions are bright.** All 11 silence letters, the `E I EE` kind, sit over one half, most over 0.8. The display does not mark them as doubtful today.
- **On a rough hand the line does its job.** On the fist bank half of what dims is wrong (97 right, 99 wrong), and on the 1.6 row exactly half. Bright letters there are right 98% of the time.
- **Raising the line buys little on your boards.** At three quarters, 4 more wrong letters would dim on the twelve-plus-three, at the cost of 19 more right ones. Where the line sits is yours to rule (parked, not proposed).

**Task 4: not run.** None of the off-line set passed, failed, or was restated, pinned or left red, because it was not run (section 4).

## 4. What's blocking us

Nothing blocks the next unit. No ruling is asked for: the dimming line is parked for the owner, with these numbers.

- **Task 4, the off-line set, was dropped by the run budget.** `UNIT_RUN_BUDGET.md` (HM-DEC-301): "A work instruction that asks for more is wrong and Claude Code says so in `output.md` section 4 instead of running it." A fourth filtered run of 54 classes is more than one survey and one run of each line, so it was refused, as the order allowed. The set, for whoever runs it:
  - Engine, whole classes: `ABarIsALevelThatHoldsTests`, `ACandidateGetsAWindowTests`, `ACharacterIsARunOfMarksThatAgreeTests`, `AHandIsReadAgainstItselfTests`, `AMarkIsTheEnvelopeOverAThresholdTests`, `APassbandIsHeldTests`, `AReplyBeginsWhereTheOtherEndsTests`, `AStationPrintsOnceSureTests`, `AStationTunedInColdTests`, `AStationsShadowIsNotAStationTests`, `AWordLineIsWhereTheGapsCrossTests`, `EAndTInsideAWordTests`, `Km3stuReadsFromHisCqTests`, `NarrownessReadsTheFiltersBandTests`, `TheBacklogStartsWhereSureTests`, `TheColumnTests`, `TheDahThatComesApartTests`, `TheFadeTraceTests`, `TheFistBankTests`, `TheFoundWeakStationTests`, `TheGridStepsAsideTests`, `TheHAt35WpmTests`, `TheHeldLanePauseTests`, `TheHoldWaitsForTheStationTests`, `TheKeyingMapTests`, `TheLiveFeedIsCountedTests`, `TheLiveFeedTests`, `TheOneFinderTests`, `TheOwnersRecordingReadsTests`, `TheRecordingsScoreboardTests`, `TheShadowReachesTheGridTests`, `TheShapeIsFoundWhereverItAppearsTests`, `TheShapePicksTheSenderTests`, `TheShapeScoreTakenApartTests`, `TheShortDahCostsTests`, `TheSpacesThatSlippedTests`, `TheW1awSessionReplayTests`, `TheW1awTableTests`, `TheWeakFastStationTracedTests`, `TheWindowsOwnViewTests`, `WhichGateTurnsAwayW1awTests`; Scan: `ACarrierIsACarrierTests`, `TheBrokenDahsTests`, `TheCatchScanTests`, `TheEarHearsAsTheAppDoesTests`, `TheStrongStationsOverTests`.
  - Engine, in part: `TheWeakEndTests` (all but its two line methods), `TheStraightKeyReadsTests` (all but `EveryHandReadsFortyEight`), `TheSequenceReaderTests` (all but `TheFirstProof`; its theories are pinned off).
  - App: `TheBarsCarryTheirLettersTests`, `TheScopeDrawsLiveTests`, `TheScopeIsTheMiddlePictureTests`, `TheScopeShowsTheMarksTests`, `TheVerdictCarriesTheScopeTests`.
  - What makes a class a rider: it calls `CwEnvelopeDetector.SetPassband` with a rig passband, directly or through `ReadLive`, `CwWildHands.Read`, `CwCatchEar` or the survey's helpers, is not pinned off, and is on neither line.
  - How to filter them: by class name (`FullyQualifiedName~.<Class>.`), the line methods excluded with `FullyQualifiedName!~`, and `Category!=Survey`. The whole command is in `.run-unit\unit600-offline.txt` (not committed).
  - Not settled: app tests that build the main window's view model might reach the learned reader through the scope tick with a CW rig state; they were not counted.
- **Order claims checked, mismatches:**
  - "The printed-letter record carries the letter and its time but no probability": true of `Printed`, but the letter's `CwCharacter` already carried the probability as `Score`, shared with the sequence reader's own figure. So the plumbing was one optional field and the scoreboard's record.
  - "Some fifty-five test files ride the default": 54 classes by this unit's rule, three of them only in part.
  - The order's §3 transcription of HM-DEC-304 says the sequence reader's tests "pin it on"; they pin the learned reader off, as the order's own §2 notes.
  - Every other claim in §2 matched the tree, including the line at `CwLearnedReader.cs:526`.
- **The end run's shell was reported stopped for memory** and in fact finished. If a later unit gets the same notice, the test host may still be running; check before starting anything again.

### Asks still outstanding

- **Unit 587, 2026-10-22:** the IC-7300's CW rise time, the owner's setting and the manual's default. `KeyingEdgeMs` is 4 ms, the default as the author recalls it from `A7292-4EX-6` p. 4-21, not re-read; the owner's setting is on no sheet. Waiting on the owner. The constant sits in `CwColumnFinder`, marked.
- **Unit 575, 2026-10-19:** what could part a hand's letter gaps from his word gaps, since they overlap in length and no line drawn from his dit alone parts every hand. Waiting on the owner. `CwRules.StraddleSplit` sits in the tree, off.
- **Unit 562, 2026-10-08:** what next for the junk before a weak call, given the 0.4 start costs the start of every real call. Waiting on the owner. No change for it sits in the tree.
- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by unit 524 and by the shadow rule. Still open before a sender stands, and for a sender 50 Hz or nearer.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
- **Unit 597, 2026-11-05:** a learned letter is dimmed under the net's probability of one half, a line not measured against its errors. Waiting on the owner. The line sits at `CwLearnedReader.cs:526`.
  - Measured this unit (HM-DEC-305): under one half 3 right / 2 wrong, at or over 301 / 11 on the twelve-plus-three. Where the line sits is still the owner's to rule.
