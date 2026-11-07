UNIT:       599 - complete at task 4 of 4, task 3 dropped as the order allowed (the trace, the switch, the end) - 2026-11-06 19:16
PHASE GOAL: Hamlet meets its CW requirements.
UNIT GOAL:  The learned reader, second model, is what Hamlet reads with by default, every line green with it on, and the ruling is recorded.
ADVANCED:   yes, task 2 (the switch). The learned reader is on by default, the engine line 175 of 175 and the app line 278 of 278 with it on.
NUMBER:     as shipped, the twelve-plus-three 308 -> 286 with KM3STU 55 of 65; W1AW 2101 -> 2128
DRIFT:      0 - this unit advanced the task it named

## 1. What Claude did

**Complete. Tasks 1, 2 and 4 are done, and task 3 was dropped whole, as the order said to do if the probability isn't kept beside the board's scoring (it isn't).**
- **Where this ran:** Claude Code on the development computer, branch `main`, every push accepted. Nothing here is evidence about the radio.
- **The gate:** the prompt named `PROJECT: Hamlet` and all four checks passed at `C:\Source\HamLet` (`SHACK_FACTS.md` and `CwLearnedReader.cs` exist, `CoreHMI.sln` and `MURC.sln` don't). Hamlet confirmed.
- **Version and ruling:** 1.13.285 to 1.13.286, ruling HM-DEC-304 as numbered.
- **The lock** was taken with the root named and released.
- **What wasn't touched:**
  - Nothing was written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` was touched, and no box was ticked in `PHASE_PLAN.md`.
  - The model, the alphabet, the feature recipe, the vector files, the sequence reader, the marks, the lanes, the keyer, the wild bank and the draft truth were not touched.
  - Nothing keys or transmits, and nothing was written to the radio. No recording was added, removed or promoted.
  - Nothing on screen moved, and no process I didn't start was stopped.
- **The prompt came three times.** The first two arrived while `WORK_INSTRUCTIONS.md` still held work instruction 598, already done, so I ran nothing then and left 598's report in place. The third found 599.
- **Time:** about 50 minutes, 18:28 to 19:16.

**Task 1, the trace. Nothing was built except one trace** (`14903161`).
- **Where the switch is set, and who reads it:**
  - The default lived at `src\Hamlet.RadioEngine\Cw\CwRules.cs:214`, `LearnedReader` in `OffByDefault`.
  - It is read in one place, `CwEnvelopeDetector.SetPassband` (`CwEnvelopeDetector.cs:1751`). With a rig passband, the learned reader is built and the sequence reader isn't; without one, neither is.
  - Everything else reaches it through that call: the tab (`CwChain.SetPassband`, and `MainWindowViewModel.cs:12432`), the scan's ear (`CwCatchEar.cs:553` and `:557`), and the survey and scoreboard helpers (`ReadLive`, `Truthful`, `Fist`, `Wild`). The traces and the app probe use `CwRules.Choose`.
  - `CwRules.SequenceReader` is a separate switch, still on, and unchanged.
- **Tests on the engine line that would go red with the default flipped:**
  - `TheScoreboardGuardsCwTests.TheScoreboardFallsNowhere`: 286 under 308, the first recording's text, and 11 letters in silences against 1.
  - `TheSequenceReaderShipsTests.TheEasyFixturesReadWhole`: 87 of 90.
  - `TheWeakEndTests.NothingPrintsInTheSilenceAfterAWeakStation`: one `E` 25.1 s into the 18 WPM CQ's file.
  - `TheLearnedReaderTests.TheSwitchOffIsSilent`: it asserted the switch was in `OffByDefault`.
  - All except the last ride the default. The last names the switch.
- **Tests on the engine line that ride the default and stay green:**
  - `TheScoreboardGuardsCwTests.TheW1awTableFallsNowhere`: 2128, 4 wrong.
  - `TheWeakEndTests.TheWeakCqsReadAtHalfOrBetter`: 83 against its floor of 83.
  - `TheFistBankGuardsTests.TheBankHolds`: every row far above its floor.
  - `TheStraightKeyReadsTests.EveryHandReadsFortyEight`: 59 or 60 of 60.
  - `TheSequenceReaderShipsTests.TheFarSideReadsWhole`: 27 of 27.
- **Tests that name their reader, or read nothing, and are unaffected:** `TheSequenceReaderTests.TheFirstProof` calls `CwSequenceReader.Read` directly, and `TheSequenceReaderShipsTests.TheReaderIsOnByDefault` checks the sequence reader's own switch. The rest are `TheClustersAreOnTheSheetTests`, `TheWildBankTests` (generation only), and the learned reader's vector and panel tests.
- **The app line:** no test on it reads CW audio through a rig passband.
- **Off every line, riding the default and testing the sequence reader:**
  - `TheSequenceReaderTests`' live theories and traces. Their `Choose(SequenceReader)` became a no-op once the learned reader was on by default.
  - `TheSequenceReaderTests.TheSequenceReaderCost`, the `TheGapLearningTests` traces, `TheWildBankTests.TheBaselineTrace`, `TheLearnedReaderTraces.TheCost` (its shipped half), and `TheLearnedReaderOnThePanelProbe` (its switch-off half).
  - All of these were pinned in task 2.
- **Off every line, riding the default and testing the chain as it ships:** about 50 files under `tests\Hamlet.RadioEngine.Tests\Cw` and `\Scan`, and 5 under `tests\Hamlet.App.Tests`. Examples are `TheRecordingsScoreboardTests`, `TheW1awTableTests`, `Km3stuReadsFromHisCqTests`, `TheOwnersRecordingReadsTests` and the scan-ear tests. They weren't pinned or run, so their state with the switch on is not measured (section 4).
- **The quick measure, switch on, matches the last report on every figure it carried** (section 3). The new trace `TheLearnedReaderTraces.TheGuards` (Survey category, on no line) printed the guard figures the board trace doesn't.
- **The dimming line:**
  - It's at `CwLearnedReader.cs:526`: `score >= 0.5 ? High : Low`, with the net's probability carried on each `CwCharacter`.
  - The scoreboard's `Printed` record (`TheRecordingsScoreboardTests.cs:210`) doesn't keep that probability, and the board scores a stretch by edit distance, with no per-letter right or wrong. So it isn't kept beside the board's scoring, and task 3 was dropped.

**Task 2, the switch goes on** (`b54cdf3c`).
- **The switch:** `LearnedReader` left `CwRules.OffByDefault`, and its doc says so. Off, the sequence reader is built exactly as before.
- **The pin is `CwRules.Off(CwRules.LearnedReader)`, not `Choose`.** With the learned reader on by default, choosing the sequence reader's own switch changes nothing. The order allowed either: "or however the trace tests pin the learned reader on".
- **Pinned to the sequence reader:**
  - `TheSequenceReaderTests.ReadLiveTraced` and `TheSequenceReaderTests.Live`, the helpers behind `TheLiveReaderReads`, `TheLiveReaderReadsAFile`, `TheReaderAgainstTheTruth`, `TheOpeningTraced`, `TheSilencesTraced` and every `TheGapLearningTests` trace that uses them.
  - `TheSequenceReaderTests.TheSequenceReaderCost`.
  - `TheGapLearningTests`: `TheCost`, `TheBoardTraced`, `TheW1awTraced`, `TheNoiseBothWays` and `TheQuickMeasure`.
  - `TheWildBankTests.TheBaselineTrace`.
  - The shipped half of `TheLearnedReaderTraces.TheCost`, and the off half of `TheLearnedReaderOnThePanelProbe`.
  - Nothing was deleted, skipped or weakened.
- **Restated at the engine's measured figure**, old to new:
  - `TheScoreboardGuardsCwTests.TheScoreboardFallsNowhere`, through a new `599 ruled` row in the board table: 308 to **286**. Wrong stays 13, invented goes 4 to **3**, spaces right 92 to **70**, spaces added 13 to **4**, and letters in a silence 1 to **11**.
  - `TheScoreboardGuardsCwTests.TheW1awTableFallsNowhere`, through a `599 ruled` row in the w1aw table: 2101 to **2128**, wrong 24 to **4**, invented 0.
  - `TheRecordingsScoreboardTests.FirstRecordingReads`: `FF C HAT<BT> BEST 7V 73 <SK> KC4ZGP ■` to **`FER CHAT<BT> BEST 7V 73 <SK> KC4ZP DE WA`**. `SilencePrintsAllowed` goes 1 to **11**.
  - `TheFistBankGuardsTests.TheBankHolds`, each floor its row less 5%: the rows 990, 1182, 1143 and 1146 to **1212, 1359, 1372 and 1346**, and the fading half 1771 to **2544**.
  - `TheSequenceReaderShipsTests.TheEasyFixturesReadWhole`: 90 to **87** of 90, now a floor. `TheFarSideReadsWhole` stays at 27.
  - `TheWeakEndTests.TheWeakCqsReadAtHalfOrBetter`: 83, the floor it already had. Only its remark changed.
  - `TheWeakEndTests.NothingPrintsInTheSilenceAfterAWeakStation`: none to **at most 1** across the four.
  - `TheLearnedReaderTests.TheSwitchOffIsSilent`, not renamed: it now asserts the switch is on by default, turns it off itself, and still checks that no learned reader is built.
  - `TheStraightKeyReadsTests.EveryHandReadsFortyEight` was left at 48. That's a floor rule, not a shipped figure, and the hands read 59 or 60.
- **`docs\carry-forward-tests.txt`:** the guard rows were updated, and the silence test was named as the line already runs it.
- **The quick measure as it now ships:** the guards themselves, `TheScoreboardGuardsCwTests` 4 of 4 (286, spaces 70 right and 4 added, 11 in a silence, the first recording as above, the carrier at 20 seeds), `Cw.TheLearnedReaderTests` 7 of 7, `TheSequenceReaderShipsTests` 3 of 3, and the silence test 1 of 1. This equals task 1.

**Task 3, the dimming line, dropped whole** (see task 1). Nothing was plumbed and the line wasn't moved.

**Task 4, the end** (the records commit).
- **The survey**, once, as it ships: 9.5 minutes. `docs\cw-survey-2026-10-09.md` was rewritten. Apart from its new `Rerun` line, *learned reader on*, it is byte-identical to the last unit's switch-on survey.
- **The lines, once each, as it ships:** the engine line 175 of 175 and the app line 278 of 278. `DecisionLogOrderTests` was 2 of 2 after the records went in.
- **Records:**
  - **HM-DEC-304** went into `DECISIONS.md`, newest first: the order's text with figures filled, two departures stated in its own second paragraph, and what each figure is and which test holds it. Its text is in `DECISIONS.md` as written, and its first paragraph is the order's, reproduced except for those two departures.
  - The `CLAUDE.md` §1 index row.
  - `CW_SPEC.md` §14, one paragraph.
  - `PHASE_OUTCOME.md`, both copies, `## UNIT 599 - STEP 12`. `PHASE_STATUS.md`, both copies, naming 599.
  - `docs\cw-scoreboard.md`: a 599 row in each of the four tables the learned reader appears in (the board and w1aw as `599 ruled`, the wild hands and the wild bank as *learned reader on - shipped*), and the guard paragraph restated.
  - `Directory.Build.props` 1.13.286.
- **`CLAUDE.md` outside the index states none of 308, 2101, 3898 or 2859 and doesn't say the learned reader is off,** so nothing else there changed. The index rows that say so are records and are not edited.
- **Nothing was recorded under §12.1.** HM-DEC-304 is the owner's ruling, recorded as ordered.

**Where the instruction didn't match the tree:**
- **`TheScoreboardGuardsCwTests` doesn't guard 3898 or 2859.** Two of its four cases read audio, and they guard the board (308) and the w1aw table (2101). 3898 is the survey's fist bank score, which `TheFistBankGuardsTests` holds row by row at other figures. **No test guards the wild bank.** HM-DEC-304's text carries 5086 and 3271 as the order wrote it, and its second paragraph says which test holds what.
- **The order expected `TheScoreboardGuardsCwTests` red before task 2.** Only `TheScoreboardFallsNowhere` would be. `TheW1awTableFallsNowhere` stays green (2128 against 2101).
- **HM-DEC-304's text says the sequence reader's tests are pinned by `CwRules.Choose`.** They're pinned by `CwRules.Off`, for the reason above, and the decision's second paragraph says so.
- **HM-DEC-304's dimming counts can't be filled** (task 3). That sentence now says the line hasn't been measured.
- **The order lists `PROJECT_STATUS.md` as uncommitted.** It was clean at the start, committed by the last unit, and each task committed it.
- **KM3STU "0 of 65 as shipped"** was not re-measured. There's no switch-off survey (HM-DEC-301), and the last report left that cell blank. 55 of 65 is the figure as it now ships.
- **The other claims hold:**
  - The switch is wired as `GapLearning` is.
  - `cwnet.onnx` passes the four vector tests.
  - `TheLearnedReaderTests` was on the line.
  - The panel reads `PanelPitchHz`, the letter rate is marked `not proved`, and the light comes from the net.
  - The dimming line is at one half.
  - The survey's `Rerun` line and the 598 rows matched.
  - `model\` and `CwSequenceReader.cs.new` are untracked.

## 2. What the owner should expect

- **When you run Hamlet in CW, the learned reader is what reads.** There's no setting for it, as ruled. The sequence reader is still in the tree, off.
- **The panel:**
  - **`decoding at`** shows the pitch of the last letters the net read.
  - **The speed** shows a letter rate marked `not proved`, because the net names letters and never measures a dit.
  - **The light** goes green while the net hears keying and dark in the gaps.
- **The scan's ear reads with the learned reader too,** since it builds the same chain.
- **Three things will look wrong but aren't:**
  - **The speed says `not proved`.** It reads low for its first ten seconds, about 6 WPM at 5 s on the test signal.
  - **The pitch goes blank after ten seconds with nothing printed.**
  - **The light changes once a second rather than with each dit.**
  - All three are as the last unit built them and are parked.
- **Letters in silences** are now up to 11 on the board where the sequence reader printed one, and the random carrier prints at every seed as before. An `E` or a short run of letters between transmissions is what you'd expect from it.
- **Every line is green with the switch on:** engine 175 of 175, app 278 of 278. Everything is pushed to `main` at 1.13.286 in three commits.
- **Off every line, about 55 test files ride the default and were never run with the learned reader on.** Some will read differently or fail when someone runs them. The lines are what guard.
- **Left as they were:** `model\`, `CwSequenceReader.cs.new` and `SESSION.lock` are untracked. `PARKED.md`, `RUN_LEDGER.md`, `tools\go-claude\GoClaude.bat`, `.run-unit\denials.txt` and the `cases-*.txt` files are uncommitted, as they were at the start.

## 3. What you should see

**The shipped build now reads with the learned reader, second model.** As shipped now, against what shipped before (the sequence reader):

| | shipped before | shipped now |
|---|---|---|
| **twelve-plus-three** | 308 (325 of 353, 13 wrong, 4 invented) | **286** (302 of 353, 13 wrong, 3 invented) |
| **KM3STU's rows** | not measured here | **55 of 65**, 1 wrong |
| **W1AW** | 2101 (2125 of 2151, 24 wrong) | **2128** (2132 of 2151, 4 wrong) |
| Ken, 0 to 120 s | 43 of 76, 25 wrong | **51 of 76**, 19 wrong |
| Bryan, 120 to 300 s | 118 of 144, 17 wrong | **124 of 144**, 12 wrong |
| wild bank | 2859 | **3271** (3372 of 3528, 101 wrong) |
| fist bank | 3898 | **5086** (5289 of 5760, 203 wrong) |
| first recording | `FF C HAT<BT> BEST 7V 73 <SK> KC4ZGP ■` | `FER CHAT<BT> BEST 7V 73 <SK> KC4ZP DE WA`, 26 of 27 |
| letters in silences | 1 | 11 |
| easy fixtures | 90 of 90 | 87 of 90 |
| 15 dB CQs | - | 82 of 84 |
| 5 dB CQs | 84 of 84 | 83 of 84 |
| 0 dB CQs | 5 of 84 | 52 of 84 |
| `two-station` | 27 of 27 | 27 of 27 |
| loud noise, steady carrier | silent | silent |
| random carrier | 20 of 20 seeds | 20 of 20 seeds |
| CPU, W1AW piece-04, 283 s | 108.1 s | **45.9 s**, down 58% |

The per-20 s windows for Ken and Bryan are in `.run-unit\reports\unit599-t1-TheLearnedReaderTraces.TheHands.txt`.

**Each guard restated, old figure to new:**
- `TheScoreboardGuardsCwTests.TheScoreboardFallsNowhere`:
  - score 308 to 286
  - invented 4 to 3
  - spaces right 92 to 70
  - spaces added 13 to 4
  - letters in a silence 1 to 11
  - the first recording `FF C HAT<BT> BEST 7V 73 <SK> KC4ZGP ■` to `FER CHAT<BT> BEST 7V 73 <SK> KC4ZP DE WA`
- `TheScoreboardGuardsCwTests.TheW1awTableFallsNowhere`: 2101 to 2128, wrong 24 to 4.
- `TheFistBankGuardsTests.TheBankHolds`: the rows 990, 1182, 1143 and 1146 to 1212, 1359, 1372 and 1346; the fading half 1771 to 2544. Each is held at its figure less 5%.
- `TheSequenceReaderShipsTests.TheEasyFixturesReadWhole`: 90 to 87 of 90.
- `TheWeakEndTests.NothingPrintsInTheSilenceAfterAWeakStation`: none to at most 1.
- `TheWeakEndTests.TheWeakCqsReadAtHalfOrBetter`: 83, unchanged.
- `TheSequenceReaderShipsTests.TheFarSideReadsWhole`: 27, unchanged.
- `TheStraightKeyReadsTests.EveryHandReadsFortyEight`: 48, unchanged.

**The dimming counts at one quarter, one half and three quarters weren't measured: task 3 was dropped.** The net's probability travels on each letter, but the board keeps no per-letter record of it and scores whole stretches by edit distance, so no letter has a right or wrong to set beside it. The line stays at one half.

**What the panel shows when you run the app, in CW on a station:**
- **The scope's line:** `decoding at` and the pitch of the letters being read, for example `decoding at 579 Hz` on the test signal. It's blank until three letters have printed, and blank again after ten seconds with nothing.
- **The speed:** a figure from how many letters arrived in the last ten seconds, marked `not proved`, blank until five.
- **The light:** green `reading` while the net hears keying, dark `listening` in the gaps and in noise, updated once a second.

## 4. What's blocking us

**Nothing blocks the next unit.**

**1. The dimming line at one half is still unmeasured against the net's errors.** *Pending the owner; restated, not argued.* With the switch on, operators now see it. Measuring it needs the scoreboard to keep each printed letter's probability and to score letter by letter. That's plumbing this unit was told not to do. It could be a small unit of its own if you want the counts before you rule.

**Left, a line each:**
- **About 55 test files that ride the default are off every line and weren't run with the learned reader on.** Among them are `TheRecordingsScoreboardTests`, `TheW1awTableTests`, `Km3stuReadsFromHisCqTests`, `TheOwnersRecordingReadsTests` and the scan-ear tests. Some assert the sequence reader's old figures and will be red when run. Running or restating them would break the run budget.
- **`TheSequenceReaderShipsTests` keeps its name** while two of its three cases now hold the learned reader's figures. Renaming would change the carry-forward filter.
- **The run budget was kept:** one survey, one engine line, one app line, at the end. Around them I ran six quick-measure traces in task 1, four guard tests in task 2, and `DecisionLogOrderTests` once after the records went in.

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

The ask to turn the learned reader on is answered by HM-DEC-304 and dropped.
