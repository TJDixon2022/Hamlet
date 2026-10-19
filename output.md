# Work instruction 574 - A found weak station prints dits: Ken at 425 Hz

## 1. What Claude did

- **Surface and gate:** Claude Code on the development computer. The prompt claimed `PROJECT: Hamlet`, and the tree confirmed it:
  - `PROJECT_CARD.md` says Hamlet.
  - `SHACK_FACTS.md`, `docs\cw-scoreboard.md` and `tests\fixtures\cw\captured\cw-2026-10-19-141708.wav` exist.
  - Neither `CoreHMI.sln` nor `MURC.sln` exists.

  Nothing in this report is evidence about the radio.
- **Branch:** `main`, five commits, pushed.
- **Run by hand:**
  - `SESSION.lock` was taken through `tools\arbiter\lock.bat take` and released at the end.
  - Nothing was written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` was touched, and no box in `PHASE_PLAN.md` was ticked.
  - Nothing keys or transmits, and nothing was written to the radio.
- **The recording, first** (`2842bb46`):
  - `cw-2026-10-19-141708.wav` and its sheet were committed by the owner's order (*"put it in"*), before any code. No other recording was added, removed or promoted.
  - `.gitattributes` holds no entry for any capture, only the `*.bat` rule, so matching the other captures meant adding nothing (section 4).
- **Task 1, the trace** (`16a9da38`): `TheFoundWeakStationTests`, which asserts nothing. It holds:
  - `KensOverTraced` and `TheCqTraced`: the four rows for each.
  - `TheLanesAgainstTheMarks`: the bank's own lanes, driven by reflection, against the CQ's sent marks and Ken's by-hand marks. Each missed mark is put down to the 12 dB contrast floor, a level never over the start line, or a mark cut or merged.
  - `WhyKenIsNotHandedOn`: every second of his over, what the bank and the grid hold near 425 Hz.

  The survey now has a list of trace recordings, read in the unadjudicated group and kept out of the old captures' summed strays. Its strays line names the recording's figure on its own.
- **Task 2: not built.** The table names none of the three rules offered (section 3). The rule it does name is not on the list, and the order builds only a named candidate.
- **Task 3, splinters are not senders** (`d80e73d7`):
  - `CwShapeSideReading.HeldSenders` and `Splinters` split the gate's senders at its release line, 0.1, keeping any printed sender as a sender.
  - The four-senders capture trigger counts `HeldSenders` only.
  - The sheet's `senders` line lists splinters after the senders, as `splinters at 500 Hz, 445 marks`.
  - `TheSplintersAreNotSendersTests` holds the capture's own four: two senders and two splinters.
- **Records** (the closing commit):
  - Version 1.13.257 to 1.13.258.
  - HM-DEC-278 and its `CLAUDE.md` §1 row.
  - A 574 row in each scoreboard table, and a line under them naming the trace recording.
  - `PHASE_OUTCOME.md` and `PHASE_STATUS.md`, both copies.
  - The survey rerun, with a new `Rerun` line.
  - `CW_SPEC.md` unchanged, since no reading rule shipped.
- **Recorded under §12.1:** nothing. HM-DEC-278 records the work order's own outcome.

## 2. What the owner should expect

- **A weak station the bank finds still prints as it did. Nothing about reading changed, so it cost nothing:**
  - Ken's over replayed prints nothing at his pitch, before and after.
  - The four 5 dB CQs read 13, 14, 16 and 19 of 21 (62 of 84).
  - The boards read 297 and 2111.
- **Where the letters are lost, in two sentences.** The CQ loses whole marks in the lane, because the lane's running contrast reading dips under its 12 dB floor and the lane stops reading while the marks are plainly there. Ken never reaches the lane at all: the grid stands him first, 15 Hz off, so the lane is never handed on, and his own hand has no clean gap between dit and dah for the gate to split.
- **The burst you saw live is not in this file.** Replayed, from 272 s only `ENWTNM` prints, at KB2UZK's 500 Hz. The burst fired another capture, which is not in the tree, so it came after this file ends.
- **What changes tonight:** a capture sheet's `senders` line will list KB2UZK's pieces as `splinters at 500 Hz, … marks`. They no longer count toward "four or more senders held", so this capture's trigger would have counted two, not four.
- **Suite:**
  - Build clean, 0 warnings.
  - App line 278 of 278, and the new splinter tests 2 of 2. Survey line 1 of 1.
  - **The engine line passed 146 of 146 and then its test host crashed.** The one retry, with nothing else running, crashed the same way after 145 passed. It is reported as not measured whole. This is the same crash as unit 573.
  - The two scoreboard row checks and `DecisionLogOrderTests` passed after the records were written.
- **Pushed to `main`.** Left uncommitted, as found: `RUN_LEDGER.md`, `PARKED.md`, `.run-unit\denials.txt`, the `.run-unit\reports` files and the `cases-*.txt` files.
- **What will look wrong but is not:**
  - The survey's unadjudicated group reads 66 files. The strays bar still sums the 65 old captures (21.9), and names the trace recording apart (0.8).

## 3. What you should see

**The trace, side by side** (as shipped):

| | `cq-18wpm-5db` (truth: `CQ CQ CQ DE N0CALL N0CALL K`, 615 Hz) | Ken, `cw-2026-10-19-141708`, 272 to 300 s |
|---|---|---|
| 1. His lane | 625 Hz (10 Hz off), held at a 40 ms dit from 5.42 s, then 60 ms from 18.14 s; shapes 0.79 to 0.89. Handed on: 64 marks, contrast median 13.2 dB, p10 12.3 dB | **Never handed on.** The bank stands 425 Hz at 100, 60 and 40 ms dits from 286 s, shapes 0.68 to 0.72, but the grid stands him at 437 to 441 Hz from 285 s, so the lane is never clear. The only lane held in his over is 375 Hz at a 40 ms dit, 293.9 to 296.9 s, shape at most 0.24 |
| 2. His marks | 64 of 73 found. Sorted: 55 to 65, then 150, 185 to 210 ms; jump 2.31 between 65 and 150 | The grid's: 55 220 70 81 244 215 81 113 73 103 206 113, then from 292 s pieces of 20 to 47 ms. Widest jump 1.70 (121 to 206); widest gap jump 1.69 |
| 2. As kinded | dits 27 at 62 ms, dahs 37 at 194 ms (ratio 3.15); gaps inside 71 ms, letter 264 ms, 3.69 times, past √3. Gate: dit 61, split 109, letter line 121, word line 306 ms | Nothing kinded: the gate never qualified him. His grid sequence shows a jump of 1.82 at most (113 to 206), under the gate's two, and its shape falls from 0.68 to 0.00 to 0.19 once the pieces arrive |
| 3. Printed | `RQKQCQ DE N0C DL N0K DL K`. `R` = dit dah dit, the C's first dah missing (lost at 1.0 s). `K` = the C with its last dit missing. `N0C DL` for the first `N0CALL`: ten marks lost from 17.0 to 18.6 s, where the lane contrast fell under 12 dB | Nothing at his pitch. From 272 s, `ENWTNM` at 500 to 501 Hz, KB2UZK's |
| 4. By hand | 615 Hz, 100 ms dit, 16.7 dB: all 73 marks, 55 to 75 then 190 to 210 ms, jump 2.53 | 430 Hz, 100 ms dit, 19.4 dB: 48 marks, sorted 50 50 55 75 … 150 150 175 175 200 … 280; widest jump 1.36. **The tree's lane sees what the owner saw by hand: a smear, no clean gap between dits and dahs** |

**The bank's own lanes against those marks:**

| Lane | Against | Whole | Missed |
|---|---|---|---|
| CQ, 625 Hz, 100 ms | the 73 sent | 62 | 11, each where lane contrast < 12 dB (contrast under 12 for 11% of hops) |
| CQ, 625 Hz, 60 ms | the 73 sent | 71 | 2, each where lane contrast < 12 dB |
| CQ, 625 Hz, 40 ms | the 73 sent | 63 | 10, each where lane contrast < 12 dB |
| Ken, 425 Hz, 100 ms | 48 by hand | 45 | 2 under 12 dB, 1 merged |
| Ken, 425 Hz, 60 ms | 48 by hand | 45 | 3 under 12 dB |
| Ken, 425 Hz, 40 ms | 48 by hand | 44 | 3 under 12 dB, 1 in pieces |

Every missed CQ mark stood 1.9 to 17.7 dB over the lane's start line.

**The paragraph.** The CQ fails in the lane: its marks are there and cross the level lines, but the lane's running contrast, its key-down percentile less its key-up percentile over 3 s, dips under the 12 dB floor (`CwLaneBank.MinContrastDb`). The lane then reads nothing, and the gate, which kinds the marks it gets correctly, prints letters with elements missing. Ken fails earlier and elsewhere: the bank's lane finds his marks whole, but the grid stands him first within 50 Hz, so the lane is never handed on. The grid's own reading of him never qualifies, because his hand shows no jump of two between dit and dah, and once the grid's 20 to 47 ms pieces arrive his shape collapses. They do not fail at the same place. **None of the three rules the order offered is supported:**
- **the end hold:** no dah was cut, the hold at 0.4 already applies, and the CQ's marks are whole;
- **the lane's own clusters:** the CQ's clusters are clean, and Ken is never handed from a lane;
- **lines on the noise:** every missed mark crosses the lines.

The rule the table supports is **that a lane already holding a station is not dropped by its own 12 dB contrast floor**. It is not built here.

**Ken's over, before and after:** `ENWTNM` at 500 Hz, nothing at his pitch, 0.0 lone words per 10 s, both. The survey's whole-file figure for the recording is 0.8.

**The four 5 dB CQs:** 13, 14, 16 and 19 of 21, before and after, as shipped.

**The survey's groups:** every group as at unit 573. The only change is that the unadjudicated group holds 66 files.

**The scoreboards' rows:**
- Twelve-plus-three: `| 574 | 2026-10-19 | 318 of 353 | 20 | 1 | **297** | 0 | 86 of 110, 3 added | …`
- W1AW: `| 574 | 2026-10-19 | **2111** | 2131 of 2151 | 20 | 0 | 438 of 440, 11 added | …`
- Under them: the trace recording, not scored.

## 4. What's blocking us

- **Choice taken: no rule built in task 2.** The rule the table names (a held lane is not dropped by its own contrast floor) is not one of the three offered, and the order builds only a named one. Both boards keep as shipped.
- **Choice taken: nothing added to `.gitattributes`.** No other capture has an entry there.
- **Choice taken: the trace recording stays out of the strays bar's summed figure.** It is not an old capture, so the 21.9 stays comparable, and its 0.8 is named beside it.
- **Choice taken: a printed sender is never a splinter**, whatever its shape.
- **The live burst for Ken is not in the tree.** It fell in the capture it fired, after this file ends.
- **Ken is never handed from a lane**, because the grid stands him first. A lane rule cannot reach him while that holds.
- **The engine line's test host crashes after its last test**, in this unit and the last, with no reason in the log. It was not traced.

### Asks still outstanding

- **Unit 562, 2026-10-08:** what next for the junk before a weak call, given the 0.4 start costs the start of every real call. Waiting on the owner. No change for it sits in the tree.
- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by unit 524 and by the shadow rule. Still open before a sender stands, and for a sender 50 Hz or nearer.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
