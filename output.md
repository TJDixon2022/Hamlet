## 1. What Claude did

Surface: Claude Code on the development machine. The prompt opened `PROJECT: Hamlet`, and the tree confirmed it: `SHACK_FACTS.md`, `docs\cw-scoreboard.md` and `src\Hamlet.RadioEngine\Cw\CwLaneBank.cs` exist, and neither `CoreHMI.sln` nor `MURC.sln` does. Nothing in this report is evidence about the radio. Branch `main`.

This was a hand run:
- `SESSION.lock` was taken through `tools\arbiter\lock.bat take` and released at the end.
- Nothing was written to `RUN_LEDGER.md` or under `tools\arbiter\`, and nothing in `PHASE_PLAN.md` was ticked.
- Nothing keyed and nothing was written to the radio. No recording was added, removed or promoted.

**Built:** `CwRules.HeldLaneMarksAreItsStations`.
- A mark a bank lane calls while it holds a station, and every mark it hands on as that station's, now carries `CwMark.HeldLane`.
- With the rule on, neither the lane's pattern gate nor the sender gate judges that mark's level. Its pitch still places it, and its kind is judged by length as before.
- The grid's marks keep the level test and the flat-top test. Lane marks never met the flat-top test, which is the grid's own.
- **Confirmed:** while a lane holds, the grid's marks within 50 Hz are already kept from printing (`HeldByLane`).

**The trace** (`TheFadeTraceTests.TheHeldLanesMarks`, the two fading fists, as shipped and with both rules; tables in section 3). A test hook, `CwSenderGate.SenderBegunTrace`, records each new sender.
- **At 10 dB, both readings are identical.**
  - No lane at the note is ever held: the grid stands the station and hands on 147 of 173 marks.
  - The only hold is 675 Hz at a 40 ms dit, let go as a shadow. So the rule never acts.
  - Only five new senders began near the note in 37 s. The station's rejected marks are dropped rather than dealt among many senders.
  - One letter still prints per fade cycle.
- **At 20 dB with both rules on:**
  - the 625 Hz lane at a 60 ms dit took the station at 15.41 s with the grid stepping aside;
  - it handed on 20 marks, and all 20 printed as the station's: `TKENNICETO`, its `KEN NICE TO` right;
  - at 20.85 s the lane was let go in a pause. Once the grid stands near it again, the lane's kept marks are dropped unless `HoldWaitsAndFits` holds it aside, so it handed on nothing;
  - the fist printed 42 letters against 27 as shipped.

**Measured once, with `GridStepsAside` chosen on.** It **fails section 3**, and both rules stay off: `CwRules.OffByDefault`, chosen only by a test.

| condition | needed | both on | result |
|---|---|---|---|
| the fading half | 1,200 or better, from 439 | **529** | short by 671 |
| the flat half | 2,129 or better | 2250 | holds |
| the bank's score; invented | rises from 1,891; 50 or fewer | 2077; 34 | holds |
| the working fixtures | rise from 4 of 81 | 6 of 81 | holds |
| the twelve-plus-three | 297, wrong and invented not rising, spaces not worse | 297, 20 wrong, 1 invented, 86 of 110 with 3 added | holds |
| W1AW | 2111 | 2111 | holds |
| the four 5 dB CQs | 71 of 84 or better | **68** (`cq-12wpm-5db` 11 of 21, `FQ N QCQ DE ■■AE EL D06AL I K`) | short by 3 |
| the 20 s of noise after them | nothing | nothing | holds |
| the short-dah rows; the nine hands | their guards | held | holds |
| the random carrier | two seeds at most | **three**: 5195, 5204 (`TRSAESHEEE EEESEAT EENT EW`), 5207 | over by one |
| the other hard limits | noise nothing, steady carrier none, first recording whole, no capture over 6 strays per 10 s | held; highest 3.3 | holds; summed strays 37.1 from 26.8 |
| the easy fixtures | 77 of 90 or better | 79 | holds |

The fading half is under 900, so nothing goes under *For the owner's ruling*. As shipped, every reading is as at HEAD: 297 and 2111.

**Recorded:** HM-DEC-287, in `DECISIONS.md` and the `CLAUDE.md` §1 index. Also:
- `PHASE_OUTCOME.md`, both copies: `## UNIT 583 - STEP 12`.
- `PHASE_STATUS.md`, both copies: names 583.
- The 583 rows on both scoreboard tables.
- The survey's Rerun line.
- The version, 1.13.268 to 1.13.269.

`CW_SPEC.md` is untouched because nothing shipped. No ruling was added beyond HM-DEC-287, and nothing was recorded under §12.1.

## 2. What the owner should expect

**Fading stations still do not read: as shipped, nothing changed.** With the grid stepping aside and the held lane's marks taken as its station's, a fading hand read a little more: the fading half went from 439 to 529 of 2,880. That is far short of the 1,200 bar. It also cost:
- three letters of the weak CQs (68 of 84);
- a third random-carrier seed.

Both rules stay off. Everything else improved with them on:
- the steady hands, the flat half from 2173 to 2250;
- the bank's score, from 1891 to 2077;
- invented letters, down from 46 to 34;
- the working fixtures, 4 to 6 of 81;
- the easy fixtures, 77 to 79 of 90.

Both boards held at 297 and 2111, and nothing printed in the noise after a weak CQ.

| the 2.5 hand at 20 WPM | flat, as shipped | fading, as shipped | fading, both on |
|---|---|---|---|
| 5 dB | 53 | 0 | 13 |
| 10 dB | 59 | 14 | 14 |
| 20 dB | 60 | 13 | 15 |

**Why so little:** the lane reads a fading station almost whole, and when it holds one every mark it hands on prints. But it is seldom allowed to hold:
- at 10 dB the grid keeps standing the station in pieces, so no lane is ever held;
- at 20 dB the grid's return ended the only hold after about five seconds.

The rule in a sentence: a mark a held lane calls belongs to its station, and no gate re-judges it by level.

**The lines** (one build, the build servers shut down, each line once with `--no-build`):
- **app:** 278 of 278 in 2 min 27 s;
- **engine, at 1500 s:** 159 of 159 in 12 min 50 s;
- **survey, at 1800 s:** 1 of 1 in 12 min. It is identical line for line to the shipped reading apart from its Rerun line: the bank 2612 of 5760, score 1891, 46 invented; the boards 297 and 2111; the easy fixtures 77 of 90; the working 4 of 81.


Two commits are pushed to `main`, the code and the record. The tree is clean apart from `RUN_LEDGER.md`, `PARKED.md`, `.run-unit/denials.txt`, `.run-unit/reports/*` and the `cases-*.txt` files, which stay uncommitted as ruled, and `PROJECT_STATUS.md`, which holds the panel's status.

## 3. What you should see

**The two fists' cycle tables**, one row per 1.4 s fade cycle:
- *held lane handed on* counts the marks handed to the sender gate from a lane holding the station;
- *grid handed on* counts the grid's marks within 50 Hz;
- *held marks printed* counts the held lane's marks inside printed letters;
- *new senders* counts the senders the gate began within 50 Hz.

**`fist-r2.5-20wpm-10db-fade, as shipped`**

Printed `SNAKKBAKSATEROEARRRHNBITSION`.

| cycle | from (s) | keyed | held lane handed on | grid handed on | held marks printed | new senders | letters |
|---|---|---|---|---|---|---|---|
| 0 | 0.0 | 3 | 0 | 2 | 0 | 1 | `` |
| 1 | 1.4 | 6 | 0 | 4 | 0 | 2 | `S` |
| 2 | 2.9 | 7 | 0 | 3 | 0 | 0 | `NA` |
| 3 | 4.3 | 6 | 0 | 2 | 0 | 0 | `` |
| 4 | 5.7 | 6 | 0 | 6 | 0 | 0 | `K` |
| 5 | 7.1 | 8 | 0 | 8 | 0 | 0 | `K` |
| 6 | 8.6 | 7 | 0 | 7 | 0 | 0 | `B` |
| 7 | 10.0 | 7 | 0 | 7 | 0 | 0 | `A` |
| 8 | 11.4 | 8 | 0 | 9 | 0 | 0 | `K` |
| 9 | 12.9 | 7 | 0 | 7 | 0 | 0 | `S` |
| 10 | 14.3 | 5 | 0 | 4 | 0 | 0 | `AT` |
| 11 | 15.7 | 6 | 0 | 4 | 0 | 0 | `E` |
| 12 | 17.1 | 7 | 0 | 5 | 0 | 0 | `` |
| 13 | 18.6 | 5 | 0 | 6 | 0 | 1 | `R` |
| 14 | 20.0 | 5 | 0 | 5 | 0 | 0 | `OE` |
| 15 | 21.4 | 6 | 0 | 6 | 0 | 0 | `A` |
| 16 | 22.9 | 7 | 0 | 7 | 0 | 0 | `R` |
| 17 | 24.3 | 6 | 0 | 5 | 0 | 0 | `R` |
| 18 | 25.7 | 8 | 0 | 6 | 0 | 1 | `R` |
| 19 | 27.1 | 9 | 0 | 9 | 0 | 0 | `H` |
| 20 | 28.6 | 6 | 0 | 5 | 0 | 0 | `N` |
| 21 | 30.0 | 8 | 0 | 5 | 0 | 0 | `BI` |
| 22 | 31.4 | 8 | 0 | 7 | 0 | 0 | `TS` |
| 23 | 32.9 | 8 | 0 | 6 | 0 | 0 | `I` |
| 24 | 34.3 | 6 | 0 | 5 | 0 | 0 | `O` |
| 25 | 35.7 | 6 | 0 | 5 | 0 | 0 | `` |
| 26 | 37.1 | 2 | 0 | 2 | 0 | 0 | `N` |

Totals: keyed 173; the held lane handed on 0, the grid 147; held marks printed 0; new senders 5; letters 28.

**`fist-r2.5-20wpm-10db-fade, the held lane's marks its station's, the grid stepping aside`**

Printed `SNAKKBAKSATEROEARRRHNBITSION`.

| cycle | from (s) | keyed | held lane handed on | grid handed on | held marks printed | new senders | letters |
|---|---|---|---|---|---|---|---|
| 0 | 0.0 | 3 | 0 | 2 | 0 | 1 | `` |
| 1 | 1.4 | 6 | 0 | 4 | 0 | 2 | `S` |
| 2 | 2.9 | 7 | 0 | 3 | 0 | 0 | `NA` |
| 3 | 4.3 | 6 | 0 | 2 | 0 | 0 | `` |
| 4 | 5.7 | 6 | 0 | 6 | 0 | 0 | `K` |
| 5 | 7.1 | 8 | 0 | 8 | 0 | 0 | `K` |
| 6 | 8.6 | 7 | 0 | 7 | 0 | 0 | `B` |
| 7 | 10.0 | 7 | 0 | 7 | 0 | 0 | `A` |
| 8 | 11.4 | 8 | 0 | 9 | 0 | 0 | `K` |
| 9 | 12.9 | 7 | 0 | 7 | 0 | 0 | `S` |
| 10 | 14.3 | 5 | 0 | 4 | 0 | 0 | `AT` |
| 11 | 15.7 | 6 | 0 | 4 | 0 | 0 | `E` |
| 12 | 17.1 | 7 | 0 | 5 | 0 | 0 | `` |
| 13 | 18.6 | 5 | 0 | 6 | 0 | 1 | `R` |
| 14 | 20.0 | 5 | 0 | 5 | 0 | 0 | `OE` |
| 15 | 21.4 | 6 | 0 | 6 | 0 | 0 | `A` |
| 16 | 22.9 | 7 | 0 | 7 | 0 | 0 | `R` |
| 17 | 24.3 | 6 | 0 | 5 | 0 | 0 | `R` |
| 18 | 25.7 | 8 | 0 | 6 | 0 | 1 | `R` |
| 19 | 27.1 | 9 | 0 | 9 | 0 | 0 | `H` |
| 20 | 28.6 | 6 | 0 | 5 | 0 | 0 | `N` |
| 21 | 30.0 | 8 | 0 | 5 | 0 | 0 | `BI` |
| 22 | 31.4 | 8 | 0 | 7 | 0 | 0 | `TS` |
| 23 | 32.9 | 8 | 0 | 6 | 0 | 0 | `I` |
| 24 | 34.3 | 6 | 0 | 5 | 0 | 0 | `O` |
| 25 | 35.7 | 6 | 0 | 5 | 0 | 0 | `` |
| 26 | 37.1 | 2 | 0 | 2 | 0 | 0 | `N` |

Totals: keyed 173; the held lane handed on 0, the grid 147; held marks printed 0; new senders 5; letters 28.

**`fist-r2.5-20wpm-20db-fade, as shipped`**

Printed `FNANNLKKSAMNRMTDANHMHEUGETK`.

| cycle | from (s) | keyed | held lane handed on | grid handed on | held marks printed | new senders | letters |
|---|---|---|---|---|---|---|---|
| 0 | 0.0 | 3 | 0 | 3 | 0 | 1 | `` |
| 1 | 1.4 | 6 | 0 | 4 | 0 | 1 | `F` |
| 2 | 2.9 | 7 | 0 | 5 | 0 | 0 | `N` |
| 3 | 4.3 | 5 | 0 | 4 | 0 | 0 | `A` |
| 4 | 5.7 | 7 | 0 | 5 | 0 | 1 | `N` |
| 5 | 7.1 | 7 | 0 | 6 | 0 | 0 | `N` |
| 6 | 8.6 | 8 | 0 | 7 | 0 | 1 | `L` |
| 7 | 10.0 | 6 | 0 | 2 | 0 | 0 | `K` |
| 8 | 11.4 | 9 | 0 | 4 | 0 | 0 | `K` |
| 9 | 12.9 | 7 | 0 | 3 | 0 | 0 | `S` |
| 10 | 14.3 | 5 | 0 | 2 | 0 | 0 | `A` |
| 11 | 15.7 | 5 | 0 | 1 | 0 | 0 | `M` |
| 12 | 17.1 | 8 | 0 | 3 | 0 | 0 | `N` |
| 13 | 18.6 | 6 | 0 | 3 | 0 | 0 | `R` |
| 14 | 20.0 | 5 | 0 | 1 | 0 | 0 | `MT` |
| 15 | 21.4 | 6 | 0 | 2 | 0 | 0 | `` |
| 16 | 22.9 | 7 | 0 | 2 | 0 | 0 | `D` |
| 17 | 24.3 | 7 | 0 | 3 | 0 | 0 | `A` |
| 18 | 25.7 | 9 | 0 | 3 | 0 | 0 | `N` |
| 19 | 27.1 | 7 | 0 | 5 | 0 | 1 | `H` |
| 20 | 28.6 | 7 | 0 | 6 | 0 | 0 | `M` |
| 21 | 30.0 | 7 | 0 | 5 | 0 | 0 | `H` |
| 22 | 31.4 | 9 | 0 | 7 | 0 | 0 | `E` |
| 23 | 32.9 | 7 | 0 | 6 | 0 | 0 | `U` |
| 24 | 34.3 | 6 | 0 | 5 | 0 | 0 | `GE` |
| 25 | 35.7 | 5 | 0 | 5 | 0 | 0 | `T` |
| 26 | 37.1 | 2 | 0 | 2 | 0 | 0 | `K` |

Totals: keyed 173; the held lane handed on 0, the grid 104; held marks printed 0; new senders 5; letters 27.

**`fist-r2.5-20wpm-20db-fade, the held lane's marks its station's, the grid stepping aside`**

Printed `TKENNICETOFNANNLNTKIEAMNRMTNEETNHMHEITMETK`.

| cycle | from (s) | keyed | held lane handed on | grid handed on | held marks printed | new senders | letters |
|---|---|---|---|---|---|---|---|
| 0 | 0.0 | 3 | 0 | 3 | 0 | 0 | `` |
| 1 | 1.4 | 6 | 0 | 4 | 0 | 1 | `F` |
| 2 | 2.9 | 7 | 0 | 5 | 0 | 0 | `N` |
| 3 | 4.3 | 5 | 0 | 4 | 0 | 0 | `A` |
| 4 | 5.7 | 7 | 0 | 5 | 0 | 1 | `N` |
| 5 | 7.1 | 7 | 0 | 6 | 0 | 0 | `N` |
| 6 | 8.6 | 8 | 0 | 7 | 0 | 1 | `LN` |
| 7 | 10.0 | 6 | 0 | 2 | 0 | 0 | `T` |
| 8 | 11.4 | 9 | 0 | 4 | 0 | 0 | `K` |
| 9 | 12.9 | 7 | 0 | 3 | 0 | 0 | `IE` |
| 10 | 14.3 | 5 | 1 | 2 | 0 | 1 | `A` |
| 11 | 15.7 | 5 | 5 | 1 | 5 | 0 | `TKEM` |
| 12 | 17.1 | 8 | 8 | 3 | 6 | 0 | `NNIN` |
| 13 | 18.6 | 6 | 6 | 3 | 6 | 0 | `CETR` |
| 14 | 20.0 | 5 | 0 | 1 | 3 | 0 | `OMT` |
| 15 | 21.4 | 6 | 0 | 2 | 0 | 0 | `N` |
| 16 | 22.9 | 7 | 0 | 2 | 0 | 0 | `EE` |
| 17 | 24.3 | 7 | 0 | 3 | 0 | 0 | `T` |
| 18 | 25.7 | 9 | 0 | 3 | 0 | 0 | `N` |
| 19 | 27.1 | 7 | 0 | 5 | 0 | 1 | `H` |
| 20 | 28.6 | 7 | 0 | 6 | 0 | 0 | `M` |
| 21 | 30.0 | 7 | 0 | 5 | 0 | 0 | `H` |
| 22 | 31.4 | 9 | 0 | 7 | 0 | 0 | `EI` |
| 23 | 32.9 | 7 | 0 | 6 | 0 | 0 | `T` |
| 24 | 34.3 | 6 | 0 | 5 | 0 | 0 | `ME` |
| 25 | 35.7 | 5 | 0 | 5 | 0 | 0 | `T` |
| 26 | 37.1 | 2 | 0 | 2 | 0 | 0 | `K` |

Totals: keyed 173; the held lane handed on 20, the grid 104; held marks printed 20; new senders 5; letters 42.


**The three matrices**, letters right of 60, as shipped / with both rules on.

Dah over dit by speed, flat, 20 dB:

| | 10 WPM | 15 WPM | 20 WPM | 25 WPM | row, before | row, after |
|---|---|---|---|---|---|---|
| dah 1.6 | 60 / 60 | 60 / 60 | 60 / 60 | 60 / 60 | 240 | 240 |
| dah 2.0 | 59 / 59 | 60 / 60 | 60 / 60 | 22 / 16 | 201 | 195 |
| dah 2.5 | 59 / 59 | 59 / 59 | 60 / 60 | 60 / 60 | 238 | 238 |
| dah 3.0 | 60 / 60 | 60 / 60 | 60 / 60 | 60 / 60 | 240 | 240 |

Dah over dit by signal, flat, 20 WPM:

| | 5 dB | 10 dB | 20 dB | row, before | row, after |
|---|---|---|---|---|---|
| dah 1.6 | 45 / 45 | 36 / 36 | 60 / 60 | 141 | 141 |
| dah 2.0 | 43 / 47 | 59 / 59 | 60 / 60 | 162 | 166 |
| dah 2.5 | 53 / 53 | 59 / 59 | 60 / 60 | 172 | 172 |
| dah 3.0 | 16 / 22 | 59 / 59 | 60 / 60 | 135 | 141 |

Signal by fade, dah 2.5, 20 WPM:

| | flat, before / after | 12 dB of QSB at 0.7 Hz, before / after | row, before | row, after |
|---|---|---|---|---|
| 5 dB | 53 / 53 | 0 / 13 | 53 | 66 |
| 10 dB | 59 / 59 | 14 / 14 | 73 | 73 |
| 20 dB | 60 / 60 | 13 / 15 | 73 | 75 |

The bank as shipped: 2612 of 5760, score 1891, 46 invented, flat 2173, fading 439. Both on: 2779, score 2077, 34 invented, flat 2250, fading 529.

**The working receiver fixtures**, letters right:

| fixture | as shipped | both on |
|---|---|---|
| `coverage-working` | 1 of 23, `ITTEIIITTTTED` | 1 of 23, `ITTEIIIT` |
| `exchange-working` | 0 of 19 | 2 of 19, 4 wrong, `T HE ■ D AESE I N` |
| `fast-working` | 3 of 19, `EOXDOTAN` | 3 of 19, `EOXDOTAN` |
| `prosigns-working` | 0 of 9 | 0 of 9 |
| `tightfist-working` | 0 of 11 | 0 of 11 |
| total | 4 of 81 | 6 of 81 |

**Both scoreboards' rows, as shipped** (and identical with both rules on):

| unit | right | wrong | invented | score | spaces |
|---|---|---|---|---|---|
| 583, the twelve-plus-three | 318 of 353 | 20 | 1 | **297** | 86 of 110, 3 added |
| 583, W1AW | 2131 of 2151 | 20 | 0 | **2111** | 438 of 440 |

## 4. What's blocking us

- **The verdict on the grid.**
  - On the fading half the lane reads the station almost whole: 172 of 173 marks at 20 dB, as the last unit traced.
  - Where it holds the station, the gates keep every mark it hands on: 20 of 20 here.
  - The rule still failed on the fading half, 529 against 1,200, because the lane is seldom allowed to hold. At 10 dB the grid stands the fading station in pieces and a lane at the note is never held. At 20 dB the grid's return ended the only hold within six seconds.
  - That is the evidence that the grid must retire as the finder, and the next unit is that rebuild.
- **The weak CQs and the carrier.** The 68 of 84 and the third carrier seed came with the pair on. Which of the two rules costs them was not measured apart.
- **The hold's end:** with the grid back near the lane, the lane's kept marks are dropped unless `HoldWaitsAndFits` is on, so a held fading station is let go in a pause it never made.
- **Choices taken, named under HM-DEC-276:**
  - `CwMark.HeldLane` says where a mark came from whether or not the rule is on, so the trace counts the held lane's marks as shipped too;
  - `CwSenderGate.SenderBegunTrace` does nothing while no test sets it;
  - both rules were measured by setting them on by default for the one survey and the guard run, then set back.

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
