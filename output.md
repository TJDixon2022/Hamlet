# Work instruction 575 - The split between a sender's letter gaps and word gaps; and a held lane keeps reading

## 1. What Claude did

- **Surface and gate:** Claude Code on the development computer. The prompt claimed `PROJECT: Hamlet`, and the tree at `C:\Source\HamLet` confirmed it:
  - `SHACK_FACTS.md`, `docs\cw-scoreboard.md` and `src\Hamlet.RadioEngine\Cw\CwLaneBank.cs` exist.
  - Neither `CoreHMI.sln` nor `MURC.sln` exists.

  Nothing in this report is evidence about the radio.
- **Branch:** `main`, three commits, pushed.
- **Run by hand:**
  - `SESSION.lock` was taken through `tools\arbiter\lock.bat take` and released at the end.
  - Nothing was written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` was touched, and no box in `PHASE_PLAN.md` was ticked.
  - Nothing keys or transmits, nothing was written to the radio, and no recording was added, removed or promoted.
- **Task 1, the split** (`021537b8`):
  - **The trace** (`TheSplitTraceTests`): for each of the seventeen gaps, at the moment the letter after it printed:
    - the gaps the split had in hand;
    - the clusters its walk found, the step it looked for and the largest step between neighbours;
    - the path it took and the boundary it drew;
    - the side the gap landed on.

    Then the same for twenty word gaps W1AW reads. The table is in section 3, and the full rows, with every gap in hand, are in `.run-unit\reports\unit575-trace.txt`.
  - **The rule the table names: a jump demanded that a hand never produces.**
    - The walk parts letter gaps from word gaps where two neighbouring gaps, sorted, differ by √(7/3), which is 1.53. Every W1AW split shows that step, at about 2.3.
    - A hand's gaps rise smoothly. Its largest step between neighbours is 1.09 to 1.66, so the walk finds no step at all (nine of the fourteen misses), or finds one only up to a pause, which then becomes the "word cluster" (five).
    - Either way, the letter cluster runs from about two gap dits to past six and holds the word gaps themselves.
  - **Built:** `CwRules.StraddleSplit`.
    - A letter cluster with gaps both under and over √21 of the sender's gap dits is two kinds. √21 is Morse's own midpoint between a letter gap of three and a word gap of seven.
    - Its gaps from the midpoint up become word gaps, and the word line is the midpoint.
    - A clean sender's letter cluster sits wholly under the midpoint (W1AW's at 2.7 to 2.9 gap dits), and a Farnsworth sender's wholly over it, so neither is touched.
  - **Measured once, and it failed.** It is **off**, a rule only a test can choose (`CwRules.OffByDefault`).
- **Task 2, a held lane keeps reading** (`a171d211`):
  - **Built:** `CwRules.HeldLaneReads` and `CwLaneBank.Held`.
    - The detector tells the bank which lane holds the station it has handed on.
    - That lane goes on calling marks while its contrast dips under `CwLaneBank.MinContrastDb`, 12 dB. The floor governs standing, not holding.
    - No constant was needed.
  - **What ends the hold:** nothing new. As before, it ends when:
    - the lane's sequence stops standing;
    - the grid takes the station back within 50 Hz;
    - or the lane is judged a carrier or a shadow.

    The sender gate also lets the sender go under shape 0.1.
  - **Measured once, and it failed on W1AW.** It is **off**, a rule only a test can choose. `TheWeakEndTests.TheWeakCqsWithAHeldLaneReading` keeps the CQ figures both ways.
- **A fault found on the way, fixed in task 1's commit:** the scoreboards read in parallel, and the rule switches they carry to their worker threads (`CwRules.Current`, `Use`) carried only the rules a test turned off.
  - A rule that ships off and was chosen on for a test was therefore applied only on the calling thread.
  - Both now travel. Unit 565's "on" figure for the line between the clusters may have been measured in part.
- **The lines:**
  - **The engine line,** run alone. `dotnet build-server shutdown` went first, and the one other dotnet process left (Avalonia's build-services collector) was ended.
    - It stopped with "Test host process crashed" after 147 passed, **exit code 124**. That code is the shell's `timeout 480` firing, not the host: the line takes longer than 480 s.
    - The last test before the stop could not be named, because the minimal console log names none.
    - Run once more alone with a 1500 s limit, it passed **155 of 155 in 9 min 05 s**.
  - **The app line** first stopped the same way: exit 124 at 480 s with 101 passed.
    - That run built again without `--no-build`, behind eight leftover dotnet processes.
    - With those ended and `--no-build`, it passed **278 of 278 in 2 min 14 s**.
  - **The survey line,** once: stopped once by Claude Code for low memory, as HM-DEC-276 allows, and run once more alone: **1 of 1 in 7 min 17 s**, every group unchanged and only the `Rerun` line rewritten.
- **Records** (the closing commit):
  - Version 1.13.259 to 1.13.260.
  - HM-DEC-279 at the top of `DECISIONS.md`, and its `CLAUDE.md` §1 row.
  - A 575 row in each scoreboard table.
  - `PHASE_OUTCOME.md` and `PHASE_STATUS.md`, both copies.
  - The survey's new `Rerun` line.
  - `CW_SPEC.md` §2 is unchanged, since no rule shipped.
- **Recorded under §12.1:** nothing.

## 2. What the owner should expect

- **Word spaces on hand senders are not found yet, and nothing changed.** The rule the trace named finds some, but costs more than it finds.
  - As shipped, the twelve-plus-three still reads 86 of 110 spaces with 3 added.
  - With the rule on, it read 91 of 110, but with **20 added**, every one a letter gap spaced as a word.
  - It also cost 2 letters (318 to 316 of 353) and broke your first recording: `FER C HA T <BT> BEST 7 V 73 <SK> KC 4 Z GP DEWA`. That hand's letter gaps run 358 to 430 ms, just over the new line at 354 to 359.
  - W1AW was untouched by it: 2111, 438 of 440.
  - So the SKCC line still reads `QTHWVWVNAMEIS JOEL`. A hand's letter gaps and word gaps overlap in length, and Morse's own midpoint is not where these hands put the break.
- **Weak stations would print nearly whole, but the rule printed junk on W1AW, so it is off.**
  - With the held lane reading, the four 5 dB CQs read **71 of 84** (14, 17, 20 and 20 of 21), from 62 (13, 14, 16 and 19).
  - Noise printed nothing, the bank stood nothing on loud noise, the carrier still printed at its one seed, and the twelve-plus-three held at 297.
  - But on W1AW the held lane went on reading after the station paused, and printed `M T TAT TM E` in the gap after `W1AW <AS>` on piece 2. W1AW fell from 2111 to 2101, with 8 letters invented where it had none.
  - As shipped, the CQs read 62 of 84, as before.
- **Each rule in a sentence:**
  - **The split at the word midpoint:** a sender's letter cluster that runs across √21 of its gap dits is cut there into letter and word gaps.
  - **The held lane:** the lane reading a weak station keeps reading it while its contrast dips under 12 dB.
- **The engine line is not crashing.** The "crash" of the last three units was our own 480 s timeout. The line takes nine minutes and passes 155 of 155.
- **Suite:**
  - Build clean, 0 warnings.
  - App 278 of 278, engine 155 of 155, survey 1 of 1 on its second run (the first was stopped for memory).
  - Both scoreboards as at HEAD. The scoreboard row guards passed 4 of 4 and `DecisionLogOrderTests` 2 of 2 after the records were written.
- **Pushed to `main`.** Left uncommitted, as found: `RUN_LEDGER.md`, `PARKED.md`, `.run-unit\denials.txt`, the `.run-unit\reports` files and the `cases-*.txt` files.
- **What will look wrong but is not:** the scoreboard test is red on the random carrier at seed 5195. That is the known baseline since unit 552.

## 3. What you should see

**The seventeen,** with the line as it ships and with the split at the word midpoint, in ms. *Walk clusters* is the sizes of the clusters the walk found, the letter cluster first. *Then* is the side the gap lands on with the midpoint split.

| recording | around | kind | gap | walk clusters | largest step | path | letter / word gaps | the line | gap dit | gap in gap dits | the midpoint split's line | then |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `03-221530` | `6C\|HAM` | letter gap spaced | 212 | 1+1+1 | 2.93 | jump, settled | 1 / 1 | 211 | 67 | 3.2 | 211 (unmoved) | word |
| `03-221530` | `6CHA\|MPI` | letter gap spaced | 541 | 3+2 | 2.55 | jump, settled | 3 / 2 | 269 | 66 | 8.1 | 269 (unmoved) | word |
| `03-221530` | `SAGE\|12I` | word gap missed | 252 | 14+1 | 1.57 | jump, settled | 12 / 3 | 254 | 63 | 4.0 | 254 (unmoved) | letter |
| `03-221548` | `EEMY\|SCO` | word gap missed | 283 | 6+2 | 1.63 | jump, settled | 6 / 2 | 313 | 62 | 4.6 | 313 (unmoved) | letter |
| `03-221548` | `SCOU\|TMA` | letter gap spaced | 452 | 14 | 1.42 | no jump, 2-means | 10 / 4 | 313 | 62 | 7.3 | 313 (unmoved) | word |
| `03-221745` | `E\|EDA` | word gap missed | 380 | 3 | 1.42 | no jump, 2-means refused | 3 / - | 460 | 64 | 5.9 | 293 | word |
| `03-221745` | `EE\|DAN` | word gap missed | 305 | 3 | 1.42 | no jump, 2-means refused | 3 / - | 460 | 64 | 4.8 | 293 | word |
| `03-221745` | `NDON\|40M` | word gap missed | 405 | 3 | 1.42 | no jump, 2-means refused | 3 / - | 460 | 64 | 6.3 | 293 | word |
| `03-221805` | `DAYS\|.TN` | word gap missed | 356 | 6 | 1.40 | no jump, 2-means refused | 6 / - | 399 | 66 | 5.4 | 301 | word |
| `03-221805` | `XFER\|ANO` | word gap missed | 372 | 17+3 | 1.66 | jump, settled | 17 / 3 | 508 | 69 | 5.4 | 314 | word |
| `03-221805` | `THER\|FT` | word gap missed | 424 | 24+3 | 1.57 | jump, settled | 24 / 3 | 508 | 72 | 5.9 | 328 | word |
| `03-221828` | `EDES\|BES` | word gap missed | 286 | 34 | 1.15 | no jump, 2-means | 24 / 10 | 300 | 71 | 4.0 | 300 (unmoved) | letter |
| `08-121324` | `UKKQ\|4PA` | word gap missed | 248 | 3 | 1.11 | no jump, 2-means refused | 3 / - | 324 | 75 | 3.3 | 324 (unmoved) | letter |
| `08-121324` | `Q4PA\|K` | word gap missed | 232 | 5 | 1.09 | no jump, 2-means refused | 5 / - | 334 | 75 | 3.1 | 334 (unmoved) | letter |
| `08-121414` | `BKBK\|TUR` | word gap missed | 288 | 2+1 | 1.66 | jump, settled | 2 / 1 | 301 | 50 | 5.8 | 301 (unmoved) | letter |
| `08-121414` | `BKTU\|RON` | word gap missed | 212 | 3 | 1.36 | no jump, 2-means refused | 3 / - | 343 | 50 | 4.2 | 230 | letter |
| `08-121414` | `ES72\|KM3` | word gap missed | 312 | 26 | 1.41 | no jump, 2-means | 18 / 8 | 315 | 51 | 6.1 | 233 | word |

On the seventeen, the midpoint split puts seven of the fourteen word gaps on the word side and leaves the three letter gaps where they are. The seven are `221745`'s three, `221805`'s three and `121414`'s `ES72|KM3`. On the whole board it found five more spaces and added seventeen.

**Twenty of W1AW's 438 word gaps read: what a working split looks like.** Every one shows a step of about 2.3 between its letter and word clusters, and its letter cluster sits under the midpoint, so none moves.

| recording | around | gap | walk clusters | largest step | letter / word gaps | the line | gap dit | gap in gap dits |
|---|---|---|---|---|---|---|---|---|
| `piece-01` | `QST\|QST` | 556 | 5+2 | 2.33 | 5 / 2 | 361 | 83 | 6.7 |
| `piece-01` | `W1AW\|SCH` | 556 | 29+10+1 | 2.30 | 29 / 10 | 361 | 86 | 6.4 |
| `piece-01` | `0WPM\|FOL` | 553 | 25+12+3 | 2.32 | 25 / 12 | 360 | 87 | 6.4 |
| `piece-02` | `T,\|AND` | 557 | 1+2 | 1.69 | 1 / 2 | 369 | 82 | 6.8 |
| `piece-02` | `9PM\|PDT` | 558 | 30+10 | 2.30 | 30 / 10 | 362 | 82 | 6.8 |
| `piece-02` | `2024\|QST` | 556 | 28+10+1+1 | 5.14 | 28 / 10 | 363 | 84 | 6.6 |
| `piece-02` | `NDAT\|LEA` | 237 | 2+2 | 2.41 | 2 / 2 | 151 | 38 | 6.3 |
| `piece-02` | `MPOR\|WEL` | 235 | 31+9 | 2.27 | 31 / 9 | 152 | 38 | 6.2 |
| `piece-03` | `NGET\|GEM` | 237 | 30+9+1 | 2.26 | 30 / 9 | 152 | 36 | 6.5 |
| `piece-03` | `TION\|GRO` | 231 | 30+10 | 2.30 | 30 / 10 | 152 | 37 | 6.2 |
| `piece-03` | `WAVE\|TRA` | 238 | 35+5 | 2.35 | 35 / 5 | 155 | 37 | 6.4 |
| `piece-03` | `R.AT\|SOM` | 235 | 32+7+1 | 2.30 | 32 / 7 | 153 | 37 | 6.3 |
| `piece-03` | `.THE\|FIR` | 238 | 31+8+1 | 2.31 | 31 / 8 | 152 | 37 | 6.4 |
| `piece-03` | `THEN\|AFE` | 235 | 30+10 | 2.28 | 30 / 10 | 144 | 37 | 6.3 |
| `piece-03` | `OW30\|WPM` | 277 | 23+15+1 | 6.54 | 23 / 15 | 142 | 41 | 6.8 |
| `piece-04` | `ORTH\|EAS` | 276 | 23+6+1 | 1.65 | 23 / 6 | 166 | 43 | 6.3 |
| `piece-04` | `PARE\|MIN` | 276 | 31+8+1 | 2.28 | 31 / 8 | 178 | 43 | 6.4 |
| `piece-04` | `THAT\|MOS` | 285 | 33+7 | 2.27 | 33 / 7 | 180 | 42 | 6.8 |
| `piece-04` | `ETTY\|STR` | 276 | 33+7 | 2.31 | 33 / 7 | 178 | 44 | 6.3 |
| `piece-04` | `YZER\|AND` | 274 | 33+7 | 2.24 | 33 / 7 | 178 | 43 | 6.3 |

**The four 5 dB CQs:**

| CQ | as shipped | held lane reading |
|---|---|---|
| `cq-12wpm-5db` | 13 of 21 | 14 of 21 |
| `cq-18wpm-5db` | 14 of 21 | 17 of 21 |
| `cq-18wpm-5db-char5` | 16 of 21 | 20 of 21 |
| `cq-25wpm-5db` | 19 of 21 | 20 of 21 |
| total | 62 of 84 | 71 of 84 |

**The synthetic hands,** word spaces of 14 with the added in brackets, as shipped and with the midpoint split:

| hand | seeds | as shipped | midpoint split |
|---|---|---|---|
| SKCC, 15 WPM, letter gaps 2-4 dits, word 4-6 | 5401, 5402, 5403 | 10 (2), 12 (7), 12 (3) | 10 (2), 13 (7), 12 (3) |
| touching, 20 WPM, letter 3-5.5, word 6-10 | 5381, 5382, 5383 | 12 (1), 14 (0), 12 (0) | 12 (7), 14 (8), 14 (1) |
| slow spacing, 18 WPM, letter 4-5, word 8-12 | 5391, 5392, 5393 | 14 (0), 14 (0), 14 (0) | 14 (6), 14 (7), 14 (2) |

**The survey's groups, before and after** (score):

| group | before | after |
|---|---|---|
| synthetic fixtures (noisy-18wpm 0 both) | 44 | 44 |
| receiver fixtures: easy | 75 | 75 |
| receiver fixtures: working | -4 | -4 |
| receiver fixtures: edge | 0 | 0 |
| receiver fixtures: ungraded | 63 | 63 |
| synthetic CQs: 15 dB | 72 | 72 |
| synthetic CQs: 5 dB | 46 | 46 |
| synthetic CQs: 0 dB | 0 | 0 |
| the boards: twelve-plus-three | 297 | 297 |
| W1AW pieces | 2111 | 2111 |
| strays: highest old capture per 10 s | 3.3 | 3.3 |
| strays: summed over the 65 old captures | 21.9 | 21.9 |


**The scoreboards' 575 rows:**

| unit | date | right | wrong | invented | score | in silences | spaces | what |
|---|---|---|---|---|---|---|---|---|
| 575 | 2026-10-19 | 318 of 353 | 20 | 1 | **297** | 0 | 86 of 110, 3 added | the split and a held lane (work instruction 575, HM-DEC-279): a letter cluster across √21 gap dits split there read 316 of 353 and 91 of 110 spaces with 20 added and broke the first recording; a held lane reading under its floor read 297 and 86 of 110 here; both ship off and this row is the board as shipped, as at HEAD |

| unit | date | score | right | wrong | invented | spaces | what |
|---|---|---|---|---|---|---|---|
| 575 | 2026-10-19 | **2111** | 2131 of 2151 | 20 | 0 | 438 of 440, 11 added | the split at the word midpoint read 2111 here, unchanged; a held lane reading under its floor read 2101 (2129 of 2151, 20 wrong, 8 invented, 437 of 440), printing `M T TAT TM E` in the pause after `W1AW <AS>` on piece 2; both off (HM-DEC-279); as shipped |

## 4. What's blocking us

- **Task 1 failed: the midpoint split added 17 spaces and broke the first recording.** It is off.
  - A hand's letter gaps and word gaps overlap in length: your first recording's letter gaps reach 430 ms, where `221745`'s word gaps start at 305.
  - So no line drawn from the sender's own dit alone parts every hand. What else could part them is a question for a ruling.
- **Task 2 failed on W1AW: the held lane read noise in a pause.** It is off.
  - The hold ends only when the lane's sequence stops standing, and noise marks keep a sequence standing for a while.
  - Ending it sooner needs a rule of its own, for a ruling.
- **Choices taken:**
  - **The seventeen** are the rows of `TheSpacesByCauseTests.TheGapsAgainstTheLines`, the gaps under or over the line, as the order describes them.
  - **The twenty W1AW rows** are every twenty-second word gap W1AW reads on its medium-or-better stretches, 438 in all.
  - **The engine line ran a second time with a 1500 s limit,** because its stop was our own timeout (exit 124), not a crash or a memory stop.
  - **The app line ran a second time alone with `--no-build`,** after the first run timed out behind eight leftover dotnet processes.
  - **The scoreboards' thread switches now carry the rules a test chose.** Without that, a rule a test chose on was measured only in part.
- **The survey's first run was stopped by Claude Code for low memory** while it ran in the background beside ten leftover dotnet processes. Those were ended and it was run once more alone, under HM-DEC-276, and passed.
- **The line scripts' 480 s limit is too short for the engine line,** which needs about nine minutes. The scripts sit under `.run-unit\` and are not committed.

### Asks still outstanding

- **Unit 562, 2026-10-08:** what next for the junk before a weak call, given the 0.4 start costs the start of every real call. Waiting on the owner. No change for it sits in the tree.
- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by unit 524 and by the shadow rule. Still open before a sender stands, and for a sender 50 Hz or nearer.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
