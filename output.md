# Work instruction 578 - The grid steps aside for a lane that reads a station whole

## 1. What Claude did

- **Surface and gate:** Claude Code on the development computer. The prompt claimed `PROJECT: Hamlet`, and the tree at `C:\Source\HamLet` confirmed it:
  - `SHACK_FACTS.md`, `docs\cw-scoreboard.md` and `tests\fixtures\cw\captured\cw-2026-10-19-141708.wav` exist.
  - Neither `CoreHMI.sln` nor `MURC.sln` exists.

  Nothing in this report is evidence about the radio.
- **Branch:** `main`, two commits (the rule and its tests, then the record), pushed.
- **Run by hand:**
  - `SESSION.lock` was taken through `tools\arbiter\lock.bat take` and released at the end.
  - Nothing was written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` was touched, and no box in `PHASE_PLAN.md` was ticked.
  - Nothing keys or transmits, nothing was written to the radio, and no recording was added, removed or promoted.
- **The trace, first** (`TheGridStepsAsideTests`, new). It runs on the five working receiver fixtures, on Ken's two overs, and on W1AW's piece 2 from 41 to 95 s as a grid sequence reading the station itself.
  - **Per second,** wherever a bank lane within 50 Hz of the true pitch stands at shape 0.4 or better, it lists:
    - the lane's pitch, dit and shape;
    - everything the grid holds within 50 Hz: standing sequences, candidates, windows and the station's own window, each with its pitch, its marks' median length against the lane's dit, its shape, whether a sender at its pitch has qualified, and how long it has stood without qualifying;
    - whether the lane is handed on.
  - **Per file,** the share of the station's keyed seconds in which a whole-reading lane was blocked, every grid sequence that stood, the handover trace, and the letters printed.
  - **A read-only hook for it:** `CwEnvelopeDetector.GridNear`.
- **What parts pieces from a station:** the grid sequence's shape against the lane's.
  - On pieces, the grid's sequence scored 0.00 to 0.3 for seconds at a time, against lanes at 0.7 to 0.87.
  - W1AW's own sequence scored 0.88 against its lane's 0.88.
  - **The order's two figures did not separate them:**
    - a sender at the pieces' pitch *qualified* within 3 to 8 s on four of the five fixtures;
    - the pieces' median mark wandered from 25 ms up to the station's own dahs, as the sequence took in whole marks with the pieces.
- **The rule, one rule** (`CwRules.GridStepsAside`):
  - **When it steps aside:** a bank lane at shape 0.4 or better (`StepAsideLaneShape`) is treated as clear of the grid once every sequence the grid stands within 50 Hz has scored under half the lane's shape (`GridStepsAsideShare`) for 3 s (`GridStandsOnPiecesSeconds`). This applies only where the terminal prints no sender within 50 Hz.
  - **Its backlog** is kept while it waits, and it then waits its 3 s (`LaneLeadSeconds`) like any lane.
  - **The grid takes it back** only where its own sequence there reaches the lane's shape.
  - **Unchanged:** the wait, the carrier check, the shadow rule and the end of the hold. The handover trace says `the grid stepped aside` when it happens.
- **Measured once, against section 4.** Two conditions failed (section 3), so the rule ships off. It is in `CwRules.OffByDefault`, chosen only by a test, and as shipped every reading is as at HEAD.
  - The working fixtures have not joined the weak-end guard.
  - The capture sheet's `bank` line and `cw_listen` are unchanged.
- **The lines,** after one build, with the build servers shut down:
  - the app line passed 278 of 278 in 2 min 22 s;
  - the engine line, at a 1500 s limit, passed 156 of 156 in 9 min 47 s;
  - the survey line passed 1 of 1 in 8 min 3 s, with `--no-build`.
- **Records:**
  - HM-DEC-282 at the top of `DECISIONS.md`, and its `CLAUDE.md` §1 index row.
  - A 578 row in each table of `docs\cw-scoreboard.md`, at the figures that ship, 297 and 2111.
  - `PHASE_OUTCOME.md` and `PHASE_STATUS.md`, both copies.
  - The survey's `Rerun` line, in the test and in `docs\cw-survey-2026-10-09.md`.
  - No `CW_SPEC.md` sentence, since nothing shipped.
  - Version 1.13.262 to **1.13.263**.
- **Recorded under §12.1:** nothing. HM-DEC-282 was ordered by work instruction 578.

## 2. What the owner should expect

- **The far side of a QSO still does not read, and the rule that lets the grid step aside ships off.**
  - **Ken's reply** prints `2UZKT/ M` either way. That is KB2UZK's call; with the rule on, nothing prints at Ken's pitch at all. His first over prints a lone `U`, away from his pitch, either way.
  - **The working fixtures** (5 dB with 12 dB of fading) read **6 of 81 with the rule, from 4**, and none reads half its letters.
- **It cost nothing.** Chosen, both boards read as now (297 and 2111), the four 5 dB CQs 71 of 84, and every hard limit held. The easy fixtures went from 78 to 80 of 90, because `prosigns-easy` printed `<AR> <SK>`. So the rule is harmless but nearly useless.
- **Why it does so little:**
  - **It acts late.** The grid steps aside about 6 s into the station: 3 s scoring under, then the handover's 3 s wait.
  - **The lane it hands on is often the wrong dit.** It is often the 40 ms dit lane on a 12 WPM station. The end of the hold counts three of that lane's own seven-dit word gaps, 0.84 s, so the lane is let go at the station's first letter gap.
  - **On `fast-working` and `tightfist-working` it never acts.** There the grid's pieces scored more than half the lane's shape.
- **The rule, in a sentence:** when a lane reads a station well and everything the grid holds near it scores under half as well for 3 s, the lane gets the station, and the grid gets it back only by reading it as well.
- **In the app nothing has changed.** The rule is off, and nothing on screen moves.
- **The suite:** the app line 278 of 278, the engine line 156 of 156, and the survey 1 of 1 with every group as before.
- **What will look wrong but is not:**
  - `TheRecordingsScoreboard` is red on the random carrier at seed 5195 alone, the known baseline red since unit 552.
  - The order says the working fixtures read 5 of 81. The survey and this unit read 4 of 81 as shipped.
- **Pushed:** two commits on `main`. The pre-existing uncommitted files are left out: `RUN_LEDGER.md`, `PARKED.md`, `.run-unit\denials.txt`, `.run-unit\reports\*`, and the three `cases-*.txt` files.

## 3. What you should see

**The trace in brief, as shipped.** For each file: the keyed seconds where a lane read the station whole, and how many of those the grid blocked.

| file | whole-reading lane, of the station's keyed seconds | blocked by the grid | grid's best shape near it while blocking | the grid's sequences that stood |
|---|---|---|---|---|
| `coverage-working` | 30 of 35 | **27 (77%)** | 0.00 to 0.25, once 0.68 | 619 Hz, median 30 ms, a sender qualified after 6 s |
| `exchange-working` | 24 of 27 | **20 (74%)** | 0.00 to 0.32 | 614 Hz, median 30 ms, qualified after 8 s; 597 Hz never |
| `fast-working` | 12 of 14 | **10 (71%)** | 0.05 to 0.46, then 0.80 to 0.88 (the station) | 617 Hz, median 25 ms, after 3 s; 613 Hz, median 80 ms, at once |
| `prosigns-working` | 12 of 15 | **12 (80%)** | 0.00 from 7 s | 619 Hz, median 34 ms, after 4 s |
| `tightfist-working` | 4 of 7 | **3 (43%)** | 0.60 to 0.63 on 25 ms pieces | 617 Hz, median 25 ms, never |
| Ken, 80 to 100 s | 9 of 12 | **8 (67%)** | 0.06 to 0.61, near 480 Hz | 483 Hz, median 30 to 40 ms |
| Ken, 272 to 300 s | 14 of 18 | **5 (28%)** | 0.07 to 0.60, near 438 Hz | 441 Hz, median 70 to 85 ms, after 5 s |
| W1AW, piece 2, 41 to 95 s | 54 of 55 | 32 | **0.83 to 0.89**, the station itself | 600 Hz, median 240 ms, after 3 s |

**The working fixtures, read:**

| fixture | as shipped | with the grid stepping aside |
|---|---|---|
| `coverage-working` | `ITTEIIITTTTED`, 1 of 23 | `■NMNAA`, 2 of 23 |
| `exchange-working` | nothing, 0 of 19 | `TMNEE`, 1 of 19 |
| `fast-working` | `EOXDOTAN`, 3 of 19 | `EOXDOTAN`, 3 of 19 |
| `prosigns-working` | nothing, 0 of 9 | nothing, 0 of 9 |
| `tightfist-working` | nothing, 0 of 11 | nothing, 0 of 11 |
| **total** | **4 of 81** | **6 of 81** |

**What the handover did with the rule on:**

- **`coverage-working`:** held 600 Hz at a 40 ms dit from 11.72 s, the grid stepped aside, and let it go in a pause at 12.57 s.
- **`exchange-working`:** held 625 Hz at a 60 ms dit from 10.73 s, stepped aside, and let it go in a pause at 12.77 s. It then held 600 Hz at a 100 ms dit from 20.57 to 26.34 s, stepped aside, and printed `T M N`.
- **`prosigns-working`:** held 625 Hz at a 100 ms dit from 11.76 s, stepped aside, and printed nothing before the file ends at 16 s.
- **`fast-working` and `tightfist-working`:** never stepped aside.

**Ken, as printed:**

| over | as shipped | with the rule |
|---|---|---|
| 80 to 100 s | `U`, nothing at his pitch | `U`, nothing at his pitch |
| 272 to 300 s | `2UZKT/ M` (KB2UZK's, at 475 Hz) | `2UZKT/ M`, nothing at his pitch |

**The ship condition, with the rule chosen:**

| condition | bar | with the rule | as shipped | met |
|---|---|---|---|---|
| working fixtures | rise from 4 of 81, two at half | 6 of 81, **none at half** | 4 of 81 | **no** |
| Ken's reply, new words at his pitch | 3 | **0** | — | **no** |
| Ken's first over | reported | nothing at his pitch | nothing | — |
| the twelve-plus-three | 297 | 297 (318 of 353, 20 wrong, 1 invented, 86 of 110 spaces, 3 added) | 297 | yes |
| W1AW | 2111 | 2111 (2131 of 2151, 20 wrong, 0 invented, 438 of 440, 11 added) | 2111 | yes |
| the four 5 dB CQs | 71 of 84 | 71 (14, 17, 20, 20) | 71 | yes |
| nothing in the 20 s after them | nothing | nothing | nothing | yes |
| loud noise | nothing | nothing | nothing | yes |
| the carrier keyed at random | one seed | one, 5195 | one, 5195 | yes |
| the first recording | whole | whole | whole | yes |
| the easy fixtures | 78 of 90 | **80 of 90** | 78 of 90 | yes |
| strays, old captures | none above six | not measured | highest 3.3, summed 22.3 | — |
| `two-station`, handover on | reported | 18 of 27 | 18 of 27 | — |

**The survey, by group, as shipped:** every group as before, since no reading rule changed; only the `Rerun` line of `docs\cw-survey-2026-10-09.md` moved. The working fixtures are 4 of 81 (score -4) and the easy fixtures 78 of 90 (75). The strays bar: the highest old capture 3.3 per 10 s, summed 22.3.

**The scoreboard rows that ship:**

| unit | right | wrong | invented | score | spaces |
|---|---|---|---|---|---|
| 578, the twelve-plus-three | 318 of 353 | 20 | 1 | **297** | 86 of 110, 3 added |
| 578, W1AW | 2131 of 2151 | 20 | 0 | **2111** | 438 of 440, 11 added |

## 4. What's blocking us

- **The lane handed on is let go at the station's first letter gap.** The handover picks the best-shaped lane, often a 40 ms dit lane on a 12 WPM station. The end of the hold then counts three of that lane's own word gaps, 0.84 s, when no printed sender is near. Either of two next rules would address it: hand on the lane whose dit fits the station, or end the hold on the station's own gaps. That is the next unit's to name.
- **The grid steps aside about 6 s into a station.** It takes 3 s scoring under, then the handover's 3 s wait, and the order said the wait does not move.
- **On `tightfist-working` the grid's 25 ms pieces score 0.60,** about the lane's own shape, so no share of the lane's shape parts them there. The median mark (0.25 of the lane's dit) would, but on the other four it wanders.
- **Choices taken, named under HM-DEC-276:**
  - **The grid sequence's shape against the lane's replaces "has not qualified" and "pieces by median length".** The trace showed neither of those separates pieces from a station. The order allowed "whatever figure the table shows".
  - **Taken back when the grid reaches the lane's shape,** in place of "when its own sequence qualifies", since the pieces qualify too.
  - **Not where the terminal prints a sender within 50 Hz.** A printed sender there is the grid reading a station, as W1AW's is.
  - **The lane's backlog is kept while it waits** under the grid, so a station stepped aside for is read from its marks before the wait.
  - **The strays on the old captures were not measured with the rule chosen,** since it had already failed two conditions. The survey ran once, as shipped.

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
