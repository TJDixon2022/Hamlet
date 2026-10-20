# Work instruction 579 - The hold's clock waits for the station, and the lane that fits is handed on

## 1. What Claude did

- **Surface and gate:** Claude Code on the development computer. The prompt claimed `PROJECT: Hamlet`, and the tree at `C:\Source\HamLet` confirmed it:
  - `SHACK_FACTS.md`, `docs\cw-scoreboard.md` and `tests\fixtures\cw\captured\cw-2026-10-19-141708.wav` exist.
  - Neither `CoreHMI.sln` nor `MURC.sln` exists.

  Nothing in this report is evidence about the radio.
- **Branch:** `main`, two commits (the change and its tests, then the record), pushed.
- **Run by hand:**
  - `SESSION.lock` was taken through `tools\arbiter\lock.bat take` and released at the end.
  - Nothing was written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` was touched, and no box in `PHASE_PLAN.md` was ticked.
  - Nothing keys or transmits, nothing was written to the radio, and no recording was added, removed or promoted.
- **The trace, first** (`TheHoldWaitsForTheStationTests`, new). It runs with `GridStepsAside` chosen, as shipped and with the change.
  - **`EveryHold`:** every hold on the five working fixtures and Ken's two overs. For each: the lane's pitch and dit against the station's, when it was held, when the station first printed near it, and when and why it was let go.
  - **`HowLongAStationTakesToPrint`:** on the four 5 dB CQs and the working fixtures, how long after a lane was held its station printed. This is the bound's figure.
  - **`WhatTheGateMakesOfTheHeldLane`:** every half second, the marks handed on near the pitch, and each sender near it with its qualifying tests.
  - **The fixtures and Ken, read:** `TheGridStepsAsideTests.TheWorkingFixturesAndKenRead` now also reads with all three rules on.
- **The change, one named change to the handover** (`CwRules.HoldWaitsAndFits`):
  - **The clock waits for the station.**
    - Until the station a lane holds has printed within 50 Hz, the end of the hold in a pause does not run. The fallback to seven of the lane's own dit is removed.
    - The hold then ends only when the lane's sequence stops standing, when the grid takes the station back, by the carrier or shadow checks, or at **`HeldLanePrintsWithinSeconds`, 12 s**. That is five times the longest a station took to print once held: 1.4 s on the 5 dB CQs, 2.4 s on the working fixtures.
    - Once the station has printed, the clock is three of the printed sender's word gaps, as before.
  - **The lane that fits is handed on.**
    - Of the lanes standing at 0.4 or better at the chosen pitch, the one picked is the one whose dit is nearest the station's dit, compared as `|ln(lane dit / station dit)|`.
    - The station's dit is the median of the dits each lane's two kinds give there (`CwPatternGate.KindsDitSeconds`). Where no lane shows two kinds yet, the best-shaped lane is picked, as now.
    - The pick is revisited while the lane holds. Another lane takes the station only where it fits better by a ratio over 1.2 (`RefitRatio`).
  - **With the grid stepping aside:** a lane handed on stays aside while it holds, and loses that in the hop it is let go.
- **Measured once,** with all three rules on. A board failed (section 3), so the change ships off. `HoldWaitsAndFits` joins `GridStepsAside` in `CwRules.OffByDefault`, chosen only by a test, and as shipped every reading is as at HEAD.
  - The working fixtures have not joined the weak-end guard.
  - The capture sheet's `bank` line is unchanged.
- **The lines,** after one build, with the build servers shut down:
  - the app line passed 278 of 278 in 2 min 19 s;
  - the engine line, at a 1500 s limit, passed 156 of 156 in 9 min 41 s;
  - the survey line passed 1 of 1 in 8 min 19 s, with `--no-build`.
- **Records:**
  - HM-DEC-283 at the top of `DECISIONS.md`, and its `CLAUDE.md` §1 index row.
  - A 579 row in each table of `docs\cw-scoreboard.md`, at the figures that ship, 297 and 2111.
  - `PHASE_OUTCOME.md` and `PHASE_STATUS.md`, both copies.
  - The survey's `Rerun` line, in the test and in `docs\cw-survey-2026-10-09.md`.
  - No `CW_SPEC.md` change, since nothing shipped.
  - Version 1.13.263 to **1.13.264**.
- **Recorded under §12.1:** nothing. HM-DEC-283 was ordered by work instruction 579.

## 2. What the owner should expect

- **Weak and far-side stations still do not print, and the change ships off. It would have cost W1AW eight invented letters.**
  - **The working fixtures** read 6 of 81 with all three rules on, from 4. That gain is the grid stepping aside alone; the two corrections add nothing to it.
  - **Ken's reply** prints nothing at his pitch, where as shipped it prints `2UZKT/ M` (KB2UZK's call).
  - **The twelve-plus-three** holds at 297.
  - **The four 5 dB CQs** rise to 74 of 84 from 71, and `two-station` to 24 of 27 from 18.
  - **W1AW falls to 2103 with 8 invented letters.** They are `M T TAT TM E`, back in the pause after `W1AW <AS>` on piece 2.
  - **The easy fixtures** fall to 77 of 90, under the bar of 78.
- **The two corrections, a sentence each:**
  - **The hold's clock waits for the station:** a lane holding a weak station is not let go for a pause until that station has printed. It is let go after 12 s if the station never does.
  - **The lane that fits is handed on:** the lane whose dit matches the station's is the one that holds it, not the best-shaped one.
- **Both worked as built.** On `coverage-working` the lane was refitted from the 40 ms dit to the 100 ms dit at once and held 10.8 s, against 0.9 s before.
- **The station still did not print, and the fade is most of why.** While held, the lane handed on almost only dahs (285–320 ms). The station's 100 ms dits were lost in the 12 dB fade. The marks started a second sender in the gate, which never showed two kinds. The backlog's own sender qualified at a shape of 0.63 and printed nothing. **So what is left on the working fixtures is the fade, and the gate splitting the station, not the hold or the lane.**
- **Why it failed on W1AW:** a lane held at 800 Hz into W1AW's pause waits for a station that never prints at its pitch, so nothing lets it go, and the gate prints its noise within seconds. The 12 s bound does not reach it.
- **In the app nothing has changed.** Both rules are off, and nothing on screen moves.
- **The suite:** the app line 278 of 278, the engine line 156 of 156, and the survey 1 of 1 with every group as before.
- **What will look wrong but is not:**
  - `TheRecordingsScoreboard` is red on the random carrier at seed 5195 alone, the known baseline red since unit 552.
  - The lock tool reports its own lock as `STALE` while held, because the `cmd` that took it has exited. It was taken and released.
- **Pushed:** two commits on `main`. The pre-existing uncommitted files are left out: `RUN_LEDGER.md`, `PARKED.md`, `.run-unit\denials.txt`, `.run-unit\reports\*`, and the three `cases-*.txt` files.

## 3. What you should see

**Every hold, with the grid stepping aside: as shipped, then with the change.**

| file | as shipped: held, lane dit / station's, first letter, let go by | with the change |
|---|---|---|
| `coverage-working` | 11.72 s, 40 ms / 100 (0.40), none, 12.57 s in a pause | 11.72 s on 40 ms, refitted to 100 ms at 11.73 s, none, 22.56 s (sequence stopped standing or the grid took it back), **10.8 s held** |
| `exchange-working` | 6.70 s, 100 ms (1.00), none, 7.13 s; 10.73 s, 60 ms (0.60), none, 12.77 s in a pause; 20.57 s, 100 ms, letter at 21.71 s, 26.34 s | the same three holds, the second let go at 12.93 s, not in a pause |
| `fast-working` | no hold | no hold |
| `prosigns-working` | 11.76 s, 100 ms (1.00), none, held to the end at 16 s | the same |
| `tightfist-working` | no hold | no hold |
| Ken, 80 to 100 s | no hold near him | no hold near him |
| Ken, 272 to 300 s | 293.86 s at 375 Hz, 40 ms, none, held to the end | 292.46 s at 425 Hz, 40 ms, refitted to 100 ms (1.08 of his 93) at 296.25 s, none, 299.25 s |

**How long a station took to print once held, with the change:**

| file | holds | first letter after |
|---|---|---|
| `cq-12wpm-5db` | 3 | 1.4 s |
| `cq-18wpm-5db` | 3 | 0.8 s |
| `cq-18wpm-5db-char5` | 4 | 1.0 and 0.9 s |
| `cq-25wpm-5db` | 1 | 0.4 s |
| `exchange-working` | 3 | 1.1 s |
| `coverage-working`, `prosigns-working` | 2 and 1 | never, held 10.8 and 4.2 s |

The longest is 2.4 s, from a run before the refit was damped. The bound, 12 s, is five times that.

**What the gate made of `coverage-working`'s held lane:**

- **At 12.0 s** the lane handed on its backlog of 26 marks. A sender at 600 Hz qualified on 21 of them: keyed runs 7, two kinds, kinds held, sure, shape 0.63. It printed nothing.
- **From 12.5 to 22.5 s** the lane handed on 11 more marks: 320, 315, 125, 305, 285, 210, 265, 165, 275, 280 and 305 ms. They are nearly all dahs. They went to a second 600 Hz sender, 22 to 31 marks, with no two kinds and shape 0.00.

**The working fixtures, read:**

| fixture | as shipped | the grid steps aside | all three rules |
|---|---|---|---|
| `coverage-working` | `ITTEIIITTTTED`, 1 of 23 | `■NMNAA`, 2 | `■NMNAA`, 2 |
| `exchange-working` | nothing, 0 of 19 | `TMNEE`, 1 | `TMNEE`, 1 |
| `fast-working` | `EOXDOTAN`, 3 of 19 | 3 | 3 |
| `prosigns-working` | nothing, 0 of 9 | 0 | 0 |
| `tightfist-working` | nothing, 0 of 11 | 0 | 0 |
| **total** | **4 of 81** | **6 of 81** | **6 of 81** |

**Ken, as printed:**

| over | as shipped | all three rules |
|---|---|---|
| 80 to 100 s | `U`, nothing at his pitch | nothing |
| 272 to 300 s | `2UZKT/ M`, at 475 Hz | `EN WTN M E/`, nothing at his pitch |

**The ship condition, all three rules on:**

| condition | bar | all three rules | as shipped | met |
|---|---|---|---|---|
| working fixtures | rise from 4, two at half | 6 of 81, none at half | 4 of 81 | **no**, and the fade clause does not apply since a board fails |
| Ken's reply, words at his pitch | 3 | 0 | 0 | **no** |
| the twelve-plus-three | 297 | 297 (318 of 353, 20 wrong, 1 invented, 86 of 110, 3 added) | 297 | yes |
| W1AW | 2111 | **2103** (2131 of 2151, 20 wrong, **8 invented**, 438 of 440, 11 added) | 2111, 0 invented | **no** |
| the four 5 dB CQs | 71 of 84 | **74** (17, 17, 20, 20) | 71 | yes |
| nothing in the 20 s after them | nothing | nothing | nothing | yes |
| loud noise | nothing | nothing | nothing | yes |
| the carrier keyed at random | one seed | one, 5195 | one, 5195 | yes |
| the first recording | whole | whole | whole | yes |
| the easy fixtures | 78 of 90 | **77** | 78 | **no** |
| strays, old captures | none above six | not measured | highest 3.3, summed 22.3 | — |
| `two-station`, handover on | reported | **24 of 27** | 18 of 27 | — |

**The survey, by group, as shipped:** every group as before, since no reading rule changed; only the `Rerun` line of `docs\cw-survey-2026-10-09.md` moved. The working fixtures are 4 of 81 and the easy fixtures 78 of 90. The strays bar: the highest old capture 3.3 per 10 s, summed 22.3.

**The scoreboard rows that ship:**

| unit | right | wrong | invented | score | spaces |
|---|---|---|---|---|---|
| 579, the twelve-plus-three | 318 of 353 | 20 | 1 | **297** | 86 of 110, 3 added |
| 579, W1AW | 2131 of 2151 | 20 | 0 | **2111** | 438 of 440, 11 added |

## 4. What's blocking us

- **The pause again.** With the clock waiting for a station, a lane held into W1AW's pause after `<AS>` has nothing to let it go, and prints `M T TAT TM E`. The wait would need an end that a lane on noise reaches inside 5 s; neither the order nor the table gives one.
- **The fade loses a weak station's dits.** At 5 dB with 12 dB of QSB at 0.7 Hz, the held lane handed on almost only dahs. The order names this a separate unit.
- **The gate splits a handed-on station.** The backlog's sender qualified at a shape of 0.63 and printed nothing. The marks after it began a second sender at the same pitch. Neither was traced further. This is the next unit's to name.
- **The gains under the change are real and not yet kept:** the 12 WPM 5 dB CQ from 14 to 17 of 21, and `two-station` from 18 to 24 of 27. They come with W1AW's eight invented letters.
- **Choices taken, named under HM-DEC-276:**
  - **The station's dit is the median over every lane's two kinds at the pitch,** and a refit needs a ratio over 1.2. The first build took the chosen lane's own two kinds and swapped lanes back and forth 600 times on `coverage-working`.
  - **A lane handed on with the grid stepped aside stays aside while it holds.** Without that, its own station printing near it stopped its marks being kept, and nothing was handed on after the backlog. The grid stepping aside was built last unit and is measured on here, so that fault was this change's to fix.
  - **That lane loses its stepped-aside mark in the hop it is let go.** Without that, it was picked again in the same hop, 600 times at 22.56 s.
  - **The bound is 12 s, five times the table's longest wait.** It was the figure measured, and no bound the table supports reaches the W1AW fault.
  - **The strays on the old captures were not measured with the rules on,** since a board had already failed. The survey ran once, as shipped.

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
