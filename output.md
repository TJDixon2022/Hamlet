# Work instruction 576 - A held lane keeps reading, and the hold ends in a pause

## 1. What Claude did

- **Surface and gate:** Claude Code on the development computer. The prompt claimed `PROJECT: Hamlet`, and the tree at `C:\Source\HamLet` confirmed it:
  - `SHACK_FACTS.md`, `docs\cw-scoreboard.md` and `src\Hamlet.RadioEngine\Cw\CwLaneBank.cs` exist.
  - Neither `CoreHMI.sln` nor `MURC.sln` exists.

  Nothing in this report is evidence about the radio.
- **Branch:** `main`, two commits (the rule, then the record), pushed.
- **Run by hand:**
  - `SESSION.lock` was taken through `tools\arbiter\lock.bat take` and released at the end.
  - Nothing was written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` was touched, and no box in `PHASE_PLAN.md` was ticked.
  - Nothing keys or transmits, nothing was written to the radio, and no recording was added, removed or promoted.
- **The pause on piece 2, traced first** (`TheHeldLanePauseTests`, new). It covers 224 to 246 s of `w1aw-2026-10-07/piece-02`, with a held lane reading, and with and without the end of the hold. For every mark a held lane called, it lists:
  - the lane's pitch, the mark's start and length, and its level against the lane's key-down and key-up;
  - the lane's contrast at that moment;
  - beside it, the printed sender's dit, letter gap, word gap and word line, and the longest gap W1AW had left in the piece before.

  A hook for it, `CwLaneBank.HeldTrace`, is set only by tests. What it found:
  - W1AW's last mark before the pause ends at 232.43 s and his next starts at 237.70 s, so the pause is 5.3 s. His dit is 84 ms, his letter gap 236 ms and his word gap 556 ms.
  - **The invented letters were not at his pitch.** A lane at 800 Hz was held from 234.15 s and printed `M T T A T T M E G`.
  - Its contrast fell from 12.8 to 7.6 dB, and its noise marks, 30 to 290 ms long, came every 0.1 to 0.5 s.
- **The rule, one rule** (`c02b1d59`, `CwRules.HeldLanePause`, `CwEnvelopeDetector.HeldLanePauseGaps` = 3):
  - **When the hold ends:** a held lane is let go once it has handed on no mark of half its sender's dit or longer for more than three of its sender's word gaps.
  - **Whose dit and word gap:** the printed sender's, where he is printed within 50 Hz of the lane (`CwDecoder.RunsPrintedTiming`, wired by `CwChain`). Otherwise the lane's own dit and seven of it.
  - **After the hold ends:** the lane is an ordinary lane again. Its floor governs, its sequence must stand on its own, and every lane at that pitch waits its 3 s (`LaneLeadSeconds`) again before it can be handed on.
  - **What does not count:** a piece under half a dit does not restart the clock.
- **`HeldLaneReads` ships on**, with the end of its hold. It was built and left off in the last unit.
- **Measured once, against section 2's ship condition.** Every condition held (section 3), so both rules are on by default:
  - the guard `TheWeakCqsReadAtHalfOrBetter` now asserts 71 of 84 or better;
  - a new guard, `NothingPrintsInTheSilenceAfterAWeakStation`, plays each 5 dB CQ followed by 20 s of its own noise and asserts that nothing prints in the 20 s;
  - `TheWeakCqsWithAHeldLaneReading` now compares the rule on, as shipped, with the rule off.
- **The lines,** after one build, with the build servers shut down and leftover dotnet processes ended:
  - the app line passed 278 of 278 in 2 min 14 s;
  - the engine line, at a 1500 s limit, passed 156 of 156 in 9 min (the new guard is the 156th);
  - the survey line passed 1 of 1 in 7 min 18 s, with `--no-build`.
- **Records:**
  - HM-DEC-280 at the top of `DECISIONS.md`, and its `CLAUDE.md` §1 index row.
  - A 576 row in each table of `docs\cw-scoreboard.md`.
  - `PHASE_OUTCOME.md` and `PHASE_STATUS.md`, both copies.
  - The survey's `Rerun` line, in the test and in `docs\cw-survey-2026-10-09.md`.
  - A `CW_SPEC.md` §2 sentence.
  - Version 1.13.260 to **1.13.261**.
- **Recorded under §12.1:** nothing. HM-DEC-280 was ordered by work instruction 576.

## 2. What the owner should expect

- **Weak stations now print almost whole, and it cost nothing on either board.**
  - **The four 5 dB CQs:** **71 of 84** letters, up from 62. That is 14, 17, 20 and 20 of 21: `NQ CQ CQ DE N0CALL N0CALL K` and `RQCQCQDEN0CALLN0CALLK` read clean.
  - **W1AW:** 2111, **0 invented**, 438 of 440 spaces with 11 added, the same as before the unit. The rule alone had printed eight junk letters in a pause.
  - **The twelve-plus-three:** 297, unchanged.
  - **Noise:** 20 s of plain noise after a weak CQ prints nothing.
- **The rule in a sentence:** once Hamlet is reading a weak station through a lane, it keeps reading while the signal fades. It lets go once that lane has gone three of the sender's word gaps with no real dit or dah.
- **What the pause on piece 2 looked like.** W1AW stopped for about five seconds after `QST DE W1AW <AS>`, before his 35 WPM practice.
  - The junk letters came from a lane at 800 Hz, not from W1AW's 600 Hz. That lane kept reading noise through the pause.
  - Now that lane is let go during `<AS>`, because it heard nothing real for over a second. It has to wait again, so it is not picked up for the pause, and the pause prints nothing.
  - Hamlet now reads `<BT> NOW 35 WPM` there. With the held lane and no end to its hold, it read `O W 35 WPM` after the junk.
- **What will look wrong but is not:**
  - The scoreboard test is red on the random carrier at seed 5195. That is the known baseline since unit 552.
  - The survey's summed strays rose from 21.9 to 22.3, mostly on `cw-2026-09-23-173723`, which now prints a run of `E`s. It is reported, not a condition, and the highest capture is still 3.3 per 10 s against the bar of six.
- **Pushed:** `main`, `c02b1d59` and the record commit after it.
- **Left uncommitted, as they were before the session:** `RUN_LEDGER.md`, `PARKED.md`, `.run-unit\denials.txt`, `.run-unit\reports\*` and `tests\fixtures\cw\captured\cases-*.txt`.

## 3. What you should see

**The pause on piece 2, the held lane reading and no end to its hold**, the 800 Hz lane that printed, in dB and ms. W1AW is printed at 600 Hz until his `<AS>` ends at 232.43 s, and nobody after that until 237 s, so the sender's column says none there. His figures at the pause are a dit of 84 ms, a letter gap of 236, a word gap of 556 and a word line of 362. The longest gap he had left in the piece before 224 s is 4502 ms. *Below key-down* is how far each mark's level sits under the lane's key-down, as a share of its contrast.

| lane | called at | start | length | level | key-down | key-up | below key-down | contrast | sender printed |
|---|---|---|---|---|---|---|---|---|---|
| 800 | 234.165 | 233.922 | 215 | -31.1 | -28.4 | -41.2 | 0.21 | 12.8 | none |
| 800 | 234.325 | 234.162 | 135 | -31.3 | -28.4 | -41.2 | 0.23 | 12.8 | none |
| 800 | 234.475 | 234.317 | 130 | -31.0 | -28.4 | -41.2 | 0.20 | 12.8 | none |
| 800 | 234.580 | 234.477 | 75 | -30.6 | -28.6 | -41.4 | 0.16 | 12.8 | none |
| 800 | 234.890 | 234.572 | 290 | -30.2 | -28.3 | -40.0 | 0.17 | 11.7 | none |
| 800 | 235.050 | 234.952 | 70 | -29.7 | -28.2 | -37.6 | 0.16 | 9.5 | none |
| 800 | 235.310 | 235.207 | 75 | -29.9 | -28.1 | -36.9 | 0.21 | 8.8 | none |
| 800 | 235.385 | 235.307 | 50 | -30.5 | -28.1 | -36.9 | 0.27 | 8.8 | none |
| 800 | 235.445 | 235.387 | 30 | -30.5 | -28.1 | -36.9 | 0.27 | 8.8 | none |
| 800 | 235.530 | 235.432 | 70 | -29.0 | -28.0 | -35.6 | 0.13 | 7.6 | none |
| 800 | 235.670 | 235.567 | 75 | -29.3 | -28.0 | -35.6 | 0.18 | 7.6 | none |
| 800 | 236.215 | 236.092 | 95 | -29.4 | -28.2 | -35.9 | 0.16 | 7.7 | none |
| 800 | 236.400 | 236.267 | 105 | -30.3 | -28.2 | -35.9 | 0.27 | 7.7 | none |
| 800 | 236.515 | 236.407 | 80 | -29.2 | -28.2 | -35.9 | 0.12 | 7.7 | none |
| 800 | 236.675 | 236.612 | 35 | -28.8 | -28.2 | -35.9 | 0.08 | 7.7 | none |
| 800 | 236.880 | 236.772 | 80 | -29.7 | -28.2 | -36.1 | 0.20 | 7.9 | none |
| 800 | 236.975 | 236.872 | 75 | -30.9 | -28.2 | -36.1 | 0.34 | 7.9 | none |
| 800 | 237.040 | 236.977 | 35 | -31.0 | -28.2 | -36.1 | 0.36 | 7.9 | 800 Hz: dit 35, letter 80, word 125 |
| 800 | 237.215 | 237.102 | 85 | -28.7 | -28.2 | -36.1 | 0.07 | 7.9 | 800 Hz: dit 35, letter 80, word 125 |
| 800 | 237.290 | 237.222 | 40 | -29.0 | -28.1 | -36.1 | 0.11 | 8.0 | 800 Hz: dit 35, letter 105 |
| 800 | 237.425 | 237.337 | 60 | -29.5 | -28.1 | -36.1 | 0.17 | 8.0 | none |
| 800 | 237.555 | 237.437 | 90 | -28.3 | -27.9 | -36.2 | 0.04 | 8.2 | none |
| 800 | 237.710 | 237.647 | 35 | -30.6 | -27.9 | -36.2 | 0.32 | 8.2 | none |

- **Printed with no end to the hold:** `<AS>`@232.43, then from 800 Hz `M T T A T T M E G` from 234.45 to 237.01 s, then W1AW's `O W 3 5 W P M <BT> 8 F E E T` from 239.18 s.
- **The 800 Hz lane's first mark** came 1.5 s into the pause, and each one after it within 0.1 to 0.55 s of the last, so no count of word gaps ends a hold inside the pause.
- **The holds, with the end of the hold:**
  - 800 Hz held at 230.15 s and **let go in a pause at 231.41 s**, 1.26 s with nothing kinded, three of seven 60 ms dits;
  - 375 Hz held at 233.16 s and let go in a pause at 234.00 s;
  - 725 Hz held from 234.00 s to 241.38 s. It handed on noise at 7.3 to 10.8 dB contrast, a mark every 0.1 to 0.8 s, and **the gate printed none of it**.
- **Printed with the end of the hold:** `<AS>`@232.43, then `<BT> N O W 3 5 W P M <BT> 8 F E E T` from 238.15 s. Nothing was printed in the pause.

**The four 5 dB CQs:**

| CQ | before (held lane off) | as shipped |
|---|---|---|
| `cq-12wpm-5db` | 13 of 21 | 14 of 21, `FQ N QCQ DE N0/AE EL N06AL I K` |
| `cq-18wpm-5db` | 14 of 21 | 17 of 21, `RQKQCQ DE N0C DL N0CALL K` |
| `cq-18wpm-5db-char5` | 16 of 21 | 20 of 21, `RQCQCQDEN0CALLN0CALLK` |
| `cq-25wpm-5db` | 19 of 21 | 20 of 21, `NQ CQ CQ DE N0CALL N0CALL K` |
| total | 62 of 84 | **71 of 84** |

**The silence after a weak station:** each 5 dB CQ followed by 20 s of its own noise, at its own level. In the 20 s, `cq-12wpm-5db`, `cq-18wpm-5db`, `cq-18wpm-5db-char5` and `cq-25wpm-5db` each print **0 letters**.

**The survey's groups, before and after:**

| group | before | after |
|---|---|---|
| synthetic fixtures | 46 of 67, score 44 | unchanged |
| receiver fixtures: easy | 78 of 90, 75 | unchanged |
| receiver fixtures: working | 4 of 81, -4 | unchanged |
| receiver fixtures: edge | 0 of 81, 0 | unchanged |
| receiver fixtures: ungraded | 77 of 98, 14 wrong, **63** | 73 of 98, 8 wrong, **65** |
| synthetic CQs: 15 dB | 78 of 84, 72 | unchanged |
| synthetic CQs: 5 dB | 62 of 84, 16 wrong, **46** | 71 of 84, 12 wrong, **59** |
| synthetic CQs: 0 dB | 0 of 84, 0 | unchanged |
| the boards: twelve-plus-three | 297 | 297 |
| W1AW pieces | 2111 | 2111 |
| strays, highest old capture per 10 s | 3.3 | 3.3 |
| strays, summed over the 65 old captures | 21.9 | 22.3 |

**The scoreboards' new rows:**

| unit | date | right | wrong | invented | score | in silence | spaces |
|---|---|---|---|---|---|---|---|
| 576 | 2026-10-20 | 318 of 353 | 20 | 1 | **297** | 0 | 86 of 110, 3 added |

| unit | date | score | right | wrong | invented | spaces |
|---|---|---|---|---|---|---|
| 576 | 2026-10-20 | **2111** | 2131 of 2151 | 20 | 0 | 438 of 440, 11 added |

The hard limits on the twelve-plus-three as shipped:
- loud noise at its four seeds prints nothing;
- nothing prints in a silence;
- the first recording reads `FER C HAT<BT> BEST 7V 73 <SK> KC4ZGP DEWA`;
- the random carrier prints at one seed of twenty, 5195.

## 4. What's blocking us

- **The end of the hold did not end a hold inside the pause, and the pause is clean for another reason.**
  - Noise marks long enough to count kept coming every 0.1 to 0.8 s, so a lane holding noise never goes three word gaps without one.
  - What kept piece 2 clean: the 800 Hz lane was let go during `<AS>` and had to wait again. The 725 Hz lane held through the pause handed on noise that the gate happened not to print.
  - A held lane that is reading only noise is still not let go by anything the rule measures. Telling noise marks from a station's needs a rule of its own.
- **Choices taken, named under HM-DEC-276:**
  - **Kinded means half the sender's dit or longer.** The order's examples of marks the gate does not kind are a splinter and a piece under half a dit. The lane already drops marks under half its own dit.
  - **Whose word gap:** the printed sender's, where he is printed within 50 Hz of the lane. Otherwise, seven of the lane's own dit, since the lane's own sequence keeps only ten marks and has no word gaps.
  - **Three word gaps** was the order's starting figure and the only one measured. The table supports it because it keeps W1AW. It cannot support a figure that ends a hold inside the pause, as the first item says.
  - **After a hold ends in a pause,** every dit's lane within 50 Hz of the pitch waits its 3 s again, so the same station is not handed straight back on through another dit.
- **`cw-2026-09-23-173723` now prints a run of `E`s** (`WEEEE E EEEEEI E EE HEEII`), 0.0 to 0.7 strays per 10 s. It is under the bar and not a condition.

### Asks still outstanding

- **Unit 575, 2026-10-19:** what could part a hand's letter gaps from his word gaps, since they overlap in length and no line drawn from his dit alone parts every hand. Waiting on the owner. `CwRules.StraddleSplit` sits in the tree, off.
- **Unit 562, 2026-10-08:** what next for the junk before a weak call, given the 0.4 start costs the start of every real call. Waiting on the owner. No change for it sits in the tree.
- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by unit 524 and by the shadow rule. Still open before a sender stands, and for a sender 50 Hz or nearer.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
