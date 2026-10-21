## 1. What Claude did

Surface: Claude Code on the development machine. The prompt opened `PROJECT: Hamlet`, and the tree confirmed it: `SHACK_FACTS.md`, `docs\cw-scoreboard.md` and `src\Hamlet.RadioEngine\Cw\CwLaneBank.cs` exist, and neither `CoreHMI.sln` nor `MURC.sln` does. Nothing in this report is evidence about the radio. Branch `main`.

This was a hand run:
- `SESSION.lock` was taken through `tools\arbiter\lock.bat take` and released at the end.
- Nothing was written to `RUN_LEDGER.md` or under `tools\arbiter\`, and nothing in `PHASE_PLAN.md` was ticked.
- Nothing keyed and nothing was written to the radio. No recording was added, removed or promoted.

**The RSID test, in the first commit.** `TheRsidDetectorTests.TheDetectorKeepsUpWithRealTime` now times its own thread through Windows' `GetThreadTimes`. Off Windows it falls back to the process's time, as before. Run alone it read 0.078 and 0.079 of real time. It passed on the engine line beside the parallel guards.

**The trace** (`TheFadeTraceTests.TheFadingFists`, the 2.5 hand at 20 WPM, fading and flat, at 5, 10 and 20 dB). Rows every 250 ms are in `.run-unit\reports\unit582-trace-*.tsv`, and the cycle tables are in section 3. It does not confirm the order's likely shape:
- **The lane's lines lag, but they are not what breaks the station.**
  - The bank lane at 625 Hz and a 60 ms dit read the 20 dB fading fist 172 of 173 marks, and the 10 dB fading fist 143 called, 10 short and 20 missed.
  - Its key-down line (the 90th percentile of three seconds) swings only 3.3 to 5.3 dB against the fade's 12. So it sits at the peaks.
  - At 10 dB the lane missed 29 of the 85 marks keyed in a trough. At 20 dB it missed none.
  - It called no noise mark at a peak, because the generator's noise does not fade.
  - So a word gap's history, or lines set on the marks' own tops, would win back part of the 10 dB troughs, and nothing at 20 dB.
- **The fault is the level tolerance.**
  - Each mark's level follows the fade to within about a decibel: at 10 dB, -18 dB at a peak and -30 dB in a trough. From one mark to the next it moves by up to 7 dB.
  - The sender gate and the pattern gate hold a mark to 3 dB of the mean of the sender's last three marks. That is twice a flat top's wobble.
  - Outside that tolerance fell 101 of 139 marks on the 10 dB fist and 53 of 96 on the 20 dB fist, against none on either flat twin.
  - So one station was dealt among new senders, and **one letter printed per fade cycle**, every 1.4 s: `S N A K K B A K …` at 1.61, 3.05, 4.28, 5.98, 7.47 s.
- **At 20 dB the grid also loses the fade's slopes.** A mark's top must be flat to 1.5 dB, and a dah on the slope tilts 4 dB. The grid handed on 99 of 173 marks, losing 42 of 54 on a slope and 30 of 59 in a trough. The lane that read them is not handed on while the grid stands within 50 Hz.
- **The one change named:** the gates' level tolerance follows the sender through the fade.

**Built:** `CwRules.FollowsTheFade`, in both gates. A sender's level tolerance grows by its own pace:
- the median rate its level moved between its last `FollowMarks` marks (eight);
- times `FollowPace` (1.5);
- times the time since the marks its level is read from;
- never more than the swing those marks span.

Before settling on it I tried three other forms on the trace and on sixteen fists:
- the last mark as the reference. It cost the weak flat fists, as HM-DEC-229 had found.
- the net rate across five marks;
- the signed median rate.

In either gate alone the rule reads the fading fists as without it.

**Measured once, the rule on.** It **fails section 4** and ships off: `CwRules.OffByDefault`, chosen only by a test.

| condition | needed | on | result |
|---|---|---|---|
| the fading half | 1,200 or better, from 439 | **960** | short by 240 |
| the flat half | 2,129 or better (2,173 less 2%) | **2105** | short by 24; the 5 dB flat column 41 and 31 at 2.5 and 3.0, from 53 and 16 |
| the bank's score; invented | rises; invented 50 or fewer | 2270 from 1891; **202** | invented over by 152, 184 of them on fading fists, the worst `fist-r2.0-10wpm-20db-fade` at -79 |
| the working fixtures | rise from 4 of 81 | 6 of 81 | holds |
| the twelve-plus-three | 297, wrong and invented not rising, spaces not worse | 298, wrong 19, **invented 4 from 1**, spaces 86 of 110 | fails on invented |
| W1AW | 2111 | **2038**, 25 wrong; piece 3 read 737 of 815 from 804 | fails by 73 |
| the four 5 dB CQs | 71 of 84 or better | 71 | holds |
| the 20 s of noise after them | nothing | **32 letters** after `cq-25wpm-5db` | fails |
| the short-dah rows; the nine hands | their guards | held | holds |
| the hard limits | noise nothing; carrier ≤ 2 seeds; steady none; first recording whole; no capture over 6 strays per 10 s | held: carrier at 5195 and 5207, steady none, first recording whole, highest 3.0 | holds; summed strays 34.3 from 26.8 |
| the easy fixtures | 77 of 90 or better | 77 | holds |

Conditions beyond the fading half failed too, so nothing goes under *For the owner's ruling*. As shipped, every reading is as at HEAD: 297 and 2111.

**Recorded:** HM-DEC-286, in `DECISIONS.md` and the `CLAUDE.md` §1 index. Also:
- `PHASE_OUTCOME.md`, both copies: `## UNIT 582 - STEP 12`.
- `PHASE_STATUS.md`, both copies: names 582.
- The 582 rows on both scoreboard tables.
- The survey's Rerun line.
- The version, 1.13.267 to 1.13.268.

`CW_SPEC.md` is untouched because nothing shipped. No ruling was added beyond HM-DEC-286, and nothing was recorded under §12.1.

## 2. What the owner should expect

**Fading stations still do not read: nothing changed as shipped.** The fault is now found. A 12 dB fade at 0.7 Hz moves a station up to 7 dB between marks, and Hamlet decided a mark 3 dB off was someone else. So it printed one letter per fade cycle.

The rule that follows a station through the fade would read about twice as much of a fading hand: the fading half from 439 to 960 of 2,880. Its cost was too high to ship:
- quiet, steady hands lost letters;
- invented letters went from 46 to 202;
- W1AW fell from 2111 to 2038;
- 32 letters printed in the noise after a weak CQ.

So it waits in the tree, off.

| the 2.5 hand at 20 WPM | flat, as shipped | fading, as shipped | fading, rule on |
|---|---|---|---|
| 5 dB | 53 | 0 | 12 |
| 10 dB | 59 | 14 | 48 |
| 20 dB | 60 | 13 | 22 |

- **The bank, as shipped:** 2612 of 5760, score 1891, fading half 439, flat half 2173.
- **The working fixtures:** 4 of 81 as shipped, 6 with the rule on.
- **The boards:** 297 and 2111, unchanged.
- **What will look wrong but is not:** `TheFadeTraceTests` is in the tree and on no line. It asserts nothing, and its `TheBankByFade` takes about six minutes.

The rule in a sentence: a sender's level tolerance grows by how fast its own last eight marks have been moving, up to the swing they span.

**The lines** (one build, the build servers shut down, each line once with `--no-build`): the app line 278 of 278 in 2 min 26 s; the engine line at 1500 s, 159 of 159 in 12 min 28 s, the RSID real-time test green on its own thread's time; the survey line at 1800 s, 1 of 1 in 11 min, identical line for line to the shipped reading apart from its Rerun line (the bank 2612 of 5760, score 1891, 46 invented; the boards 297 and 2111; the easy fixtures 77 of 90; the working 4 of 81).

Two commits are pushed to `main`, the code and the record. The tree is clean apart from `RUN_LEDGER.md`, `PARKED.md`, `.run-unit/denials.txt`, `.run-unit/reports/*` and the `cases-*.txt` files, which stay uncommitted as ruled.

## 3. What you should see

**The two fists' cycle tables**, one row per 1.4 s fade cycle, as shipped. The lane is the bank's 625 Hz lane at a 60 ms dit. The gate column is the marks the detector handed to the sender gate within 50 Hz.

`fist-r2.5-20wpm-10db-fade` (14 of 60 in the survey):

| cycle | from (s) | lane key-down (dB) | lane contrast (dB) | keyed | lane called / short / missed | gate called / short / missed |
|---|---|---|---|---|---|---|
| 0 | 0.0 | -40.1 to -24.2 | 7.9 to 22.4 | 3 | 2 / 0 / 1 | 1 / 1 / 1 |
| 1 | 1.4 | -24.2 to -20.3 | 22.4 to 25.6 | 6 | 6 / 0 / 0 | 2 / 2 / 2 |
| 2 | 2.9 | -22.8 to -18.8 | 21.4 to 25.3 | 7 | 6 / 1 / 0 | 3 / 0 / 4 |
| 3 | 4.3 | -19.8 to -18.8 | 21.3 to 22.2 | 6 | 4 / 0 / 2 | 2 / 0 / 4 |
| 4 | 5.7 | -20.3 to -19.1 | 20.5 to 22.1 | 6 | 4 / 2 / 0 | 6 / 0 / 0 |
| 5 | 7.1 | -20.3 to -19.9 | 20.3 to 21.9 | 8 | 7 / 0 / 1 | 8 / 0 / 0 |
| 6 | 8.6 | -20.2 to -19.9 | 20.8 to 22.5 | 7 | 5 / 0 / 2 | 7 / 0 / 0 |
| 7 | 10.0 | -20.4 to -20.0 | 20.4 to 22.5 | 7 | 7 / 0 / 0 | 7 / 0 / 0 |
| 8 | 11.4 | -20.3 to -19.4 | 19.8 to 21.2 | 8 | 6 / 2 / 0 | 7 / 1 / 0 |
| 9 | 12.9 | -19.8 to -19.3 | 19.5 to 21.9 | 7 | 5 / 0 / 2 | 7 / 0 / 0 |
| 10 | 14.3 | -21.1 to -19.7 | 21.0 to 22.1 | 5 | 4 / 0 / 1 | 4 / 0 / 1 |
| 11 | 15.7 | -21.0 to -19.5 | 20.8 to 22.5 | 6 | 6 / 0 / 0 | 4 / 0 / 2 |
| 12 | 17.1 | -21.0 to -19.9 | 20.9 to 22.2 | 7 | 7 / 0 / 0 | 5 / 0 / 2 |
| 13 | 18.6 | -20.7 to -20.6 | 22.0 to 23.3 | 5 | 5 / 0 / 0 | 4 / 1 / 0 |
| 14 | 20.0 | -20.6 to -19.2 | 23.0 to 23.9 | 5 | 3 / 1 / 1 | 4 / 1 / 0 |
| 15 | 21.4 | -20.2 to -18.8 | 21.4 to 23.7 | 6 | 5 / 0 / 1 | 6 / 0 / 0 |
| 16 | 22.9 | -21.1 to -19.3 | 20.8 to 22.5 | 7 | 6 / 1 / 0 | 6 / 1 / 0 |
| 17 | 24.3 | -19.4 to -19.1 | 22.4 to 23.2 | 6 | 5 / 0 / 1 | 5 / 0 / 1 |
| 18 | 25.7 | -19.2 to -19.0 | 22.8 to 23.7 | 8 | 5 / 1 / 2 | 6 / 0 / 2 |
| 19 | 27.1 | -20.2 to -19.0 | 21.7 to 23.9 | 9 | 7 / 2 / 0 | 8 / 1 / 0 |
| 20 | 28.6 | -21.0 to -19.8 | 19.3 to 21.9 | 6 | 5 / 0 / 1 | 5 / 0 / 1 |
| 21 | 30.0 | -21.1 to -20.3 | 18.9 to 21.3 | 8 | 6 / 0 / 2 | 5 / 0 / 3 |
| 22 | 31.4 | -22.1 to -21.1 | 20.9 to 21.5 | 8 | 8 / 0 / 0 | 7 / 0 / 1 |
| 23 | 32.9 | -22.1 to -21.0 | 19.8 to 21.5 | 8 | 7 / 0 / 1 | 6 / 0 / 2 |
| 24 | 34.3 | -21.0 to -19.9 | 20.6 to 21.4 | 6 | 5 / 0 / 1 | 5 / 0 / 1 |
| 25 | 35.7 | -20.5 to -19.9 | 20.8 to 21.6 | 6 | 5 / 0 / 1 | 5 / 0 / 1 |
| 26 | 37.1 | -23.0 to -20.5 | 19.2 to 21.6 | 2 | 2 / 0 / 0 | 2 / 0 / 0 |

Totals: the lane called 143, short 10, missed 20 of 173; the gate 137, 8, 28. Printed `SNAKKBAKSATEROEARRRHNBITSION`.

`fist-r2.5-20wpm-20db-fade` (13 of 60):

| cycle | from (s) | lane key-down (dB) | lane contrast (dB) | keyed | lane called / short / missed | gate called / short / missed |
|---|---|---|---|---|---|---|
| 0 | 0.0 | -39.2 to -15.1 | 7.9 to 30.9 | 3 | 2 / 0 / 1 | 2 / 1 / 0 |
| 1 | 1.4 | -15.1 to -9.9 | 30.4 to 35.3 | 6 | 6 / 0 / 0 | 4 / 0 / 2 |
| 2 | 2.9 | -13.1 to -8.9 | 27.3 to 33.7 | 7 | 7 / 0 / 0 | 5 / 0 / 2 |
| 3 | 4.3 | -10.7 to -8.9 | 28.4 to 31.1 | 5 | 5 / 0 / 0 | 4 / 0 / 1 |
| 4 | 5.7 | -10.7 to -8.9 | 29.5 to 32.1 | 7 | 7 / 0 / 0 | 5 / 0 / 2 |
| 5 | 7.1 | -10.7 to -9.1 | 28.3 to 31.0 | 7 | 7 / 0 / 0 | 3 / 3 / 1 |
| 6 | 8.6 | -9.8 to -9.1 | 26.0 to 28.9 | 8 | 8 / 0 / 0 | 6 / 1 / 1 |
| 7 | 10.0 | -9.8 to -9.2 | 28.2 to 29.9 | 6 | 6 / 0 / 0 | 2 / 0 / 4 |
| 8 | 11.4 | -9.6 to -9.1 | 27.5 to 29.3 | 9 | 9 / 0 / 0 | 4 / 0 / 5 |
| 9 | 12.9 | -10.0 to -9.5 | 27.2 to 29.8 | 7 | 7 / 0 / 0 | 3 / 0 / 4 |
| 10 | 14.3 | -10.4 to -9.6 | 29.2 to 30.2 | 5 | 5 / 0 / 0 | 2 / 0 / 3 |
| 11 | 15.7 | -10.4 to -8.7 | 29.7 to 31.5 | 5 | 5 / 0 / 0 | 1 / 0 / 4 |
| 12 | 17.1 | -9.4 to -8.8 | 30.9 to 31.8 | 8 | 8 / 0 / 0 | 3 / 0 / 5 |
| 13 | 18.6 | -9.5 to -9.4 | 30.7 to 31.7 | 6 | 6 / 0 / 0 | 3 / 0 / 3 |
| 14 | 20.0 | -9.7 to -9.3 | 29.6 to 30.9 | 5 | 5 / 0 / 0 | 1 / 0 / 4 |
| 15 | 21.4 | -10.0 to -9.2 | 30.4 to 31.7 | 6 | 6 / 0 / 0 | 2 / 0 / 4 |
| 16 | 22.9 | -11.3 to -9.9 | 30.4 to 31.7 | 7 | 7 / 0 / 0 | 2 / 0 / 5 |
| 17 | 24.3 | -10.8 to -10.8 | 29.3 to 30.5 | 7 | 7 / 0 / 0 | 3 / 0 / 4 |
| 18 | 25.7 | -10.8 to -10.0 | 29.3 to 30.4 | 9 | 9 / 0 / 0 | 3 / 0 / 6 |
| 19 | 27.1 | -10.8 to -9.8 | 28.5 to 30.9 | 7 | 7 / 0 / 0 | 5 / 0 / 2 |
| 20 | 28.6 | -12.0 to -10.8 | 26.9 to 29.1 | 7 | 7 / 0 / 0 | 6 / 0 / 1 |
| 21 | 30.0 | -10.8 to -9.9 | 26.9 to 31.0 | 7 | 7 / 0 / 0 | 5 / 0 / 2 |
| 22 | 31.4 | -10.7 to -8.8 | 31.0 to 32.6 | 9 | 9 / 0 / 0 | 7 / 0 / 2 |
| 23 | 32.9 | -10.1 to -9.4 | 28.8 to 32.3 | 7 | 7 / 0 / 0 | 6 / 0 / 1 |
| 24 | 34.3 | -10.4 to -9.4 | 27.9 to 28.8 | 6 | 6 / 0 / 0 | 5 / 0 / 1 |
| 25 | 35.7 | -10.9 to -10.4 | 28.6 to 30.5 | 5 | 5 / 0 / 0 | 5 / 0 / 0 |
| 26 | 37.1 | -10.8 to -10.0 | 29.7 to 33.9 | 2 | 2 / 0 / 0 | 2 / 0 / 0 |

Totals: the lane called 172, short 0, missed 1 of 173; the gate 99, 5, 69. Printed `FNANNLKKSAMNRMTDANHMHEUGETK`.

**The three matrices**, letters right of 60, as shipped (before) and with the rule on (after).

Dah over dit by speed, flat, 20 dB:

| | 10 WPM | 15 WPM | 20 WPM | 25 WPM | row, before | row, after |
|---|---|---|---|---|---|---|
| dah 1.6 | 60 / 60 | 60 / 60 | 60 / 60 | 60 / 60 | 240 | 240 |
| dah 2.0 | 59 / 59 | 60 / 60 | 60 / 60 | 22 / 14 | 201 | 193 |
| dah 2.5 | 59 / 59 | 59 / 59 | 60 / 60 | 60 / 60 | 238 | 238 |
| dah 3.0 | 60 / 60 | 60 / 60 | 60 / 60 | 60 / 60 | 240 | 240 |

Dah over dit by signal, flat, 20 WPM:

| | 5 dB | 10 dB | 20 dB | row, before | row, after |
|---|---|---|---|---|---|
| dah 1.6 | 45 / 45 | 36 / 39 | 60 / 60 | 141 | 144 |
| dah 2.0 | 43 / 43 | 59 / 59 | 60 / 60 | 162 | 162 |
| dah 2.5 | 53 / 41 | 59 / 59 | 60 / 60 | 172 | 160 |
| dah 3.0 | 16 / 31 | 59 / 59 | 60 / 60 | 135 | 150 |

Signal by fade, dah 2.5, 20 WPM:

| | flat, before / after | 12 dB of QSB at 0.7 Hz, before / after | row, before | row, after |
|---|---|---|---|---|
| 5 dB | 53 / 41 | 0 / 12 | 53 | 53 |
| 10 dB | 59 / 59 | 14 / 48 | 73 | 107 |
| 20 dB | 60 / 60 | 13 / 22 | 73 | 82 |

**The bank's halves, across every ratio and speed**, from `TheBankByFade`:

| | 5 dB flat / fade | 10 dB flat / fade | 20 dB flat / fade |
|---|---|---|---|
| dah 1.6, as shipped | 91 / 0 | 146 / 38 | 240 / 54 |
| dah 2.0, as shipped | 124 / 21 | 210 / 27 | 201 / 41 |
| dah 2.5, as shipped | 154 / 22 | 211 / 50 | 238 / 51 |
| dah 3.0, as shipped | 140 / 32 | 178 / 54 | 240 / 49 |

With the rule on, the survey's totals are flat 2105 and fading 960.

**The working receiver fixtures**, letters right, as shipped / with the rule on:

| fixture | as shipped | rule on |
|---|---|---|
| `coverage-working` | 1 of 23, `ITTEIIITTTTED` | 2 of 23, 9 wrong, `IT EIIITT ETA EN MTE` |
| `exchange-working` | 0 of 19 | 0 of 19 |
| `fast-working` | 3 of 19, `EOXDOTAN` | 3 of 19, `O NA DOAAEN` |
| `prosigns-working` | 0 of 9 | 1 of 9, `A A E EN` |
| `tightfist-working` | 0 of 11 | 0 of 11 |
| total | 4 of 81 | 6 of 81 |

**Both scoreboards' rows, as shipped:**

| unit | right | wrong | invented | score | spaces |
|---|---|---|---|---|---|
| 582, the twelve-plus-three | 318 of 353 | 20 | 1 | **297** | 86 of 110, 3 added |
| 582, W1AW | 2131 of 2151 | 20 | 0 | **2111** | 438 of 440 |

With the rule on: the twelve-plus-three 321 of 353, 19 wrong, 4 invented, 298; W1AW 2066 of 2151, 25 wrong, 2038.

## 4. What's blocking us

- **The fade, still the largest hole.** Three things are needed together, and the rule was the first:
  - the gates follow a sender's level through the fade;
  - the follow does not admit noise into weak steady senders, where the 202 invented and the 5 dB flat losses came from;
  - at 20 dB, the grid's flat-top test lets a dah tilt with the fade, or the lane that reads it whole is handed on.
- **Where the 202 invented come from** is not yet traced. 184 of them are on fading fists, and `fist-r2.0-10wpm-20db-fade` alone scored -79.
- **The trace hook:** `CwLaneBank.Probe` traces one lane whether or not it is held. It does nothing while no test sets it.
- **Choices taken, named under HM-DEC-276:**
  - of the forms tried, the one that read the fading fists best is the one left in the tree;
  - `FollowMarks` 8 and `FollowPace` 1.5 are the author's figures, from the trace;
  - the measurement of the other variants is in `.run-unit\unit582-bank*.txt`, `unit582-exp*.txt` and `unit582-trace-on.txt`.

### Asks still outstanding

- **The RSID real-time test, 2026-10-20:** answered by the owner's ruling of 2026-10-21. Done in this unit and dropped.
- **Unit 575, 2026-10-19:** what could part a hand's letter gaps from his word gaps, since they overlap in length and no line drawn from his dit alone parts every hand. Waiting on the owner. `CwRules.StraddleSplit` sits in the tree, off.
- **Unit 562, 2026-10-08:** what next for the junk before a weak call, given the 0.4 start costs the start of every real call. Waiting on the owner. No change for it sits in the tree.
- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by unit 524 and by the shadow rule. Still open before a sender stands, and for a sender 50 Hz or nearer.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
