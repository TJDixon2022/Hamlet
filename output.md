# Work instruction 572 - The handover ships on

## 1. What Claude did

- **Surface and gate:** Claude Code on the development computer. The prompt claimed `PROJECT: Hamlet`. The tree confirmed it: `PROJECT_CARD.md` says Hamlet, `Hamlet.sln` is at the root, `SHACK_FACTS.md`, `docs\cw-scoreboard.md` and `src\Hamlet.RadioEngine\Cw\CwShadow.cs` exist, and neither `CoreHMI.sln` nor `MURC.sln` does. Nothing in this report is evidence about the radio.
- **Branch:** `main`, one commit, pushed.
- **Run by hand:** `SESSION.lock` was taken through `tools\arbiter\lock.bat take` and released at the end. Nothing was written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` was touched, and no box in `PHASE_PLAN.md` was ticked. Nothing keys or transmits, nothing was written to the radio, and no recording was added, removed or promoted.
- **The ship commit had not landed.** Nothing after `f42a2cc2` set `LaneHandover` on.
- **Applied from the patch.** `unit571-ship-unshipped.patch` was still in unit 571's session scratchpad and applied cleanly, unchanged. It covers items 1 to 5 of the list:
  1. `CwEnvelopeDetector.LaneHandover` defaults on.
  2. `Weak stations` (on, the default, or off) is the last row of the scan's settings popover, under `AGC in CW`. It is kept as `WeakStations` in `settings.json` and takes effect at once.
  3. Both capture sheets carry a `weak` line beside the AGC line, and each `cw_listen` row carries `weakStations`.
  4. The story line says `Weak station at <pitch> Hz` once when a lane's station prints.
  5. `TheWeakEndTests.TheWeakCqsReadAtHalfOrBetter`. It is added to the engine carry-forward line in `docs\carry-forward-tests.txt`, so it guards.
- **Written in this unit:**
  - 6. **The strays bar, restated** in the scoreboard test's hard-limit remarks, in `docs\cw-scoreboard.md` and in `CW_SPEC.md` §2. The survey now prints it: the highest capture against the line of six, and the summed figure over the old captures, reported and not a condition.
  - 7. **Both scoreboard guards** get a 572 row at 297 and 2111. The twelve-plus-three row carries the handover's spaces, 86 of 110 with 3 added.
- **Records:**
  - Version 1.13.255 to 1.13.256.
  - HM-DEC-276 in `DECISIONS.md`, with the three standing rules, and its `CLAUDE.md` §1 row.
  - `PHASE_OUTCOME.md` and `PHASE_STATUS.md`, both copies.
  - `CW_SPEC.md` §2.
  - The survey document rewritten by its own run, with a new `Rerun` line.
- **Recorded under §12.1:** nothing. HM-DEC-276 is the owner's ruling, recorded at his order.

## 2. What the owner should expect

- **Rebuild.** Then weak stations read in the app. A station too weak for the grid to stand on its own now prints once a lane has heard it keying steadily for three seconds with nothing louder beside it.
- **The switch:** the `⋯` beside Scan opens the scan's settings. The last row, under `AGC in CW`, is `Weak stations`, on. Set it to off and Hamlet reads exactly as it did last week. It takes effect at once and is kept across restarts.
- **The story line** says `Weak station at 640 Hz` (or whatever the pitch is) once, when such a station starts printing. It says it again only for another station, or after the lane lets the first one go.
- **Tonight on W1AW:**
  - Look for a `Weak station at … Hz` line on anything real.
  - The 550 Hz sender is still not reached. It is exactly 50 Hz from W1AW, and the shadow rule only judges stations more than 50 Hz away.
- **Capture sheets** carry a new `weak on` line under the AGC.
- **Suite:** build clean, 0 warnings. App line 278 of 278. Engine line 146 of 146. Survey line 1 of 1. Pushed to `main`.
- **What will look wrong but is not:**
  - The survey's summed strays read 22.3 when added from unrounded figures. The bar's 21.9 is the same 65 rows added as the table prints them. The survey line now adds them that way (see section 4).

## 3. What you should see

- **The four 5 dB CQs**, through the chain as shipped:
  - `cq-12wpm-5db`: `FQ N QCQ DE N0 IL N0N L I K`, 13 of 21.
  - `cq-18wpm-5db`: `RQKQCQ DE N0C DL N0K DL K`, 14 of 21.
  - `cq-18wpm-5db-char5`: `RQCQN DEN0KALLN0KALLK`, 16 of 21.
  - `cq-25wpm-5db`: `NQ CQ CQ DE N0CAL I N0CALL K`, 19 of 21.
- **The carrier at twenty seeds:** it prints at one, seed 5195 (`NTTTTNTE TEAETOTTNEAIT TAN`), as recorded since unit 552. It prints at no other seed.
- **The survey's groups** (score):

  | Group | Score |
  |---|---|
  | Synthetic fixtures | 44 |
  | Receiver fixtures, easy | 75 |
  | Receiver fixtures, working | -4 |
  | Receiver fixtures, edge | 0 |
  | Receiver fixtures, ungraded | 63 |
  | Synthetic CQs, 15 dB | 72 |
  | Synthetic CQs, 5 dB | 46 |
  | Synthetic CQs, 0 dB | 0 |
  | The boards | 297 and 2111 |

  Every expected figure matched.
- **The strays bar:** the highest capture is `cw-2026-08-25-012748`, at 3.3 per 10 s, under the line of six. The summed figure over the 65 old captures is 21.9.
- **The boards' rows:**
  - Twelve-plus-three: `| 572 | 2026-10-14 | 318 of 353 | 20 | 1 | **297** | 0 | 86 of 110, 3 added | …`. Nothing printed in a silence. The first recording reads whole. Noise prints nothing.
  - W1AW: `| 572 | 2026-10-14 | **2111** | 2131 of 2151 | 20 | 0 | 438 of 440, 11 added | …`.
- **After the rows were added,** `TheScoreboardGuardsCwTests` (4 of 4) and `DecisionLogOrderTests` (2 of 2) were run once more, filtered, and passed.

## 4. What's blocking us

- **Choice taken, the summed strays:** the survey's new bar line first added the unrounded figures (22.3), and the order's 21.9 is the sum of the one-decimal figures the table shows. Neither choice moves either board. The line now adds the figures as each row shows them, and the one generated line in `docs\cw-survey-2026-10-09.md` was corrected to 21.9 rather than running the survey a second time. The next survey run writes the same figure from the code.
- **Choice taken, the guard on the line:** `TheWeakCqsReadAtHalfOrBetter` was added to the engine carry-forward line so it runs every unit. Neither board moves.
- **The 550 Hz sender beside W1AW** is still out of reach of the shadow rule and the handover.
- **Nothing blocks the next step.**

### Asks still outstanding

- **Unit 562, 2026-10-08:** what next for the junk before a weak call, given the 0.4 start costs the start of every real call. Waiting on the owner. No change for it sits in the tree.
- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by unit 524 and by the shadow rule. Still open before a sender stands, and for a sender 50 Hz or nearer.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.

Unit 571's ask (what to do about the strays) was answered by the owner's ruling of 2026-10-13, ship the handover on, and is dropped.
