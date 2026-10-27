## 1. What Claude did

Surface: Claude Code on the development computer, branch `main`. The gate: the prompt said `PROJECT: Hamlet`; `SHACK_FACTS.md`, `docs\cw-scoreboard.md` and `src\Hamlet.RadioEngine\Cw\CwSequenceReader.cs` exist, `CoreHMI.sln` and `MURC.sln` do not, the root is `C:\Source\HamLet`. Hamlet confirmed. Nothing in this report is evidence about the radio.

Hand run:
- `SESSION.lock` was taken through `tools\arbiter\lock.bat take` and released at the end.
- Nothing was written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` was touched, and no box was ticked in `PHASE_PLAN.md`.
- Nothing keyed or transmitted, nothing was written to the radio, and no recording was added, removed or promoted.
- The tag `before-sequence-reader` stands. Version 1.13.275 to 1.13.276. HM-DEC-294 was free and is recorded. Nothing was recorded under §12.1.
- Round three's survey was stopped once by Claude Code's low-memory reaper, with about 1.7 GB of 16 GB free. As the order says, its leftover test processes were stopped and it was run once more, alone.

**The trace, first.** Two probes were added to `TheSequenceReaderTests`:
- **`TheColdStartsByPattern`.** Each start's pattern, gaps and likelihood on W1AW piece-02's first 36 s, the first recording's opening and `121414`.
- **`SilenceAgainstThePath`.** On the band alone and on three weak CQs, what one nobody-keying stretch scores against the decoded path, under several noise emissions.

They found:
- **The cold read's start was not the fault.** Likelihood and pattern take the same start in 26 of 27 windows. At 600 Hz piece-02's opening reads `A, AND OCTOBER` and `WBER 8 AT 10 PM`.
- **An off lane held the terminal.** Piece-02's loss was a lane 25 Hz off W1AW, at 575 Hz, holding the terminal on `dit 50 dah 185 ig 5 lg 80 ms`. That 5 ms inside gap passed a pattern test that looks only at marks.
- **The bank's invented letters were not noise.** They were not the band alone after the text ends. 60 of 101 were one hand, `fist-r1.6-20wpm-20db-flat`, printed from a lane at 675 Hz, 60 Hz off the station.
- **`144020` was lane choice, not change 3.** No lane held two stations; the second station was lost to which lane printed.

**Round one** (`16357053`):
- **Change 1: a decode is Morse only where its learned gaps are in order.** The inside gap must be at least `InsideGapLeast` 0.4 of the dit, and each gap after it at least `GapStep` 1.4 over the last. This is `PatternStats.GapsInOrder`. The cold read keeps the likeliest start whose pattern is Morse, and a lane's 10 s pattern is judged with the rhythm it learned.
- **Change 3: a warm lane whose figures fall reads cold as well.** The trigger is separation down by `PatternDrop` 0.25 of the kept figure, or dah share moving 0.25. The cold read is taken where it is Morse and better.
- **Change 2 was built and taken out before the commit.** The nobody-keying state was emitted as the lane's own noise. As the order's Gaussian, it cannot make the band alone silence: the marks explain the band alone 0.08 to 0.13 nats a hop better. As narrowband noise's exact log-Rayleigh density, it came within 0.01 to 0.03 of the marks on the band alone. But it took the fade's troughs: the first proof's 10 dB fading case read 19 to 42 of 60 and the 5 dB fading case 19 to 31, and the band alone still decoded 28 to 37 marks.
- Measured: 275, 2104, the bank 3842.

**Round two** (`16b7267e`):
- **The trace.** On `144020` at 20 s the terminal went to 625 Hz, 25 Hz off the second station, at separation 1.64. From 22 s the 600 Hz lane read him at 2.25. It was dropped as the printed lane's shadow, because the printed lane was listed first.
- **The fix.** Of one station's lanes the best separated prints. The printed lane leads only while no lane reading its sequence stands `SeparationMargin` 0.25 better.
- Measured: 285, 2104, the bank 3922 with invented 64 to 18, and the nine hands whole.

**Round three** (`2f1a2ddf`):
- **The trace.** On `144020` from 12.5 s the printed 500 Hz lane's sender had stopped. Reading warm, it relearned its levels on the second station's leakage at −33.6 dB, against the −20.2 it kept.
- **The fix.** A warm lane whose learned key-down falls more than `LevelDropDb` 10 dB under its sender's commits nothing from that window, and keeps its sender's rhythm.
- Measured: 293, 2104, the bank 3925.

**Builds and runs.**
- Each round was built once with `SequenceReader` out of `OffByDefault`. That change was never committed and was restored with `git checkout`.
- The build servers were shut down after each build. Then, one at a time: the survey, the guard line, CPU on piece-04, and in round three the board's noise lines and the first proof.
- The record build was the solution whole with the rule off. The app line, the engine line (1500 s) and the survey line then ran once each with `--no-build`.

**Recorded**, not a ruling: HM-DEC-294 in `DECISIONS.md` and its `CLAUDE.md` §1 row; `docs\cw-scoreboard.md` 590 rows; `PHASE_OUTCOME.md`, both copies, `## UNIT 590 - STEP 12`; `PHASE_STATUS.md`, both copies, naming 590; `Directory.Build.props`; the survey's `Rerun` line in `TheCwSurveyTests` and the doc.

## 2. What the owner should expect

**The whole picture: the sequence reader against two finders, at the end of round three with it on.**

| | two finders (shipped) | sequence reader |
|---|---|---|
| W1AW | 2111 (2131 right, 20 wrong) | 2104 (2126 right, 22 wrong, 0 invented) |
| the hand-sent recordings, the twelve-plus-three | 297 (318 right, 20 wrong, 1 invented) | 293 (316 right, 20 wrong, 3 invented) |
| the first recording | whole | `RF C HA T <BT> BEST 7V 73 <SK> KC4ZGP ■` |
| a short-dah fist: the bank's 1.6 and 2.0 rows | 1193 of 2880 | 2180 |
| the fist bank | 1891 (2612 right, 46 invented) | 3925 (4475 right, 18 invented) |
| a weak CQ at 5 dB: the four 5 dB CQs | 71 of 84 | 84 of 84 |
| a weak CQ at 0 dB | 0 of 84 | 5 of 84 |
| a fading station: the fading half, letters right | 439 of 2880 | 1763 |
| the flat half, letters right | 2173 | 2712 |
| the far side: `two-station` | 18 of 27 | 27 of 27 |
| the nine straight-key hands at 48 of 60 | 9 | 9 |
| the easy / working fixtures | 77 of 90 / 4 of 81 | 90 / 35 |
| letters in silences | 0 | 4 |
| the band alone | not Morse | 284 to 298 marks, not Morse, not silence |
| loud noise, the steady carrier | nothing | nothing |
| the random carrier | 2 seeds of 20 | 20 seeds of 20 |
| strays summed over the 65 old captures, highest | 26.8, 3.3 | 48.8, 5.3 |
| CPU on W1AW `piece-04`, 283 s of audio | 185 s | 100 s |

**It does not ship.** `SequenceReader` stays off. Where it misses the bar:
- **The twelve-plus-three** is 293 against 297, one letter under the 294 that would be inside the 1 per cent band.
- **W1AW** is inside the band at 2104, but its wrong letters rise from 20 to 22.
- **The bank** invents 18 against a bar of 10.
- **The band alone** is not silence.
- **The hard limits:** 4 letters print in silences, and the first recording is not whole.
- **Strong flat hands at 20 dB** read a little under shipped at 10, 15 and 20 WPM (dah 3.0 at 20 WPM: 56 against 60).

Everything else holds: the fist bank's score, both halves, the CQs, the noise after a weak CQ, the short-dah rows, the nine hands, the easy fixtures, noise, the steady carrier, the strays line and CPU.

The *For the owner's ruling* case does not apply. It covers one board just under, or the bank's invented letters between 10 and 30, but only with everything else holding, and here the board, the invented letters and the hard limits all miss together.

**The three changes, a sentence each:**
- **The cold read by pattern.** A reading counts as Morse only when its gaps step up in order, which stopped a lane 25 Hz off W1AW holding the terminal on a 5 ms inside gap; his opening reads 37 of 39 from 13.
- **Silence as noise.** It could not be made to work. Noise and a faded station sit too close in level for the model to call one silence without losing the other, so it was measured and taken out.
- **A second station.** The cold read on a fall never fired. What lost the far side was lane choice, fixed in rounds two and three.

**What the boards still lose:**
- **The twelve-plus-three:**
  - 8 on `121414`.
  - 6 on the first recording's opening.
  - 5 on `144020`.
  - The letters in silences on `200157`, `143906` and `221502`.
- **W1AW:** 7, spread over pieces 02 to 04.
- **Not change 1's.** The openings read the same under either cold-read rule: the reader's opening, a fourth thing. The letters in silences are not traced.

As shipped, nothing changes: 297 and 2111. The app line 278 of 278. The engine line 159 of 159 in 11.1 minutes, run once more alone after Claude Code stopped it for low memory. The survey line 1 of 1 in 10.4 minutes, every group as before (297, 2111, the bank 1891), only its `Rerun` line changed. Pushed to `main`: rounds one to three and the record.

## 3. What you should see

**The cold-read trace** (`TheColdStartsByPattern`; one pass per start in the 15 Hz width under the shared levels; score is likelihood less prior per hop; full rows in `.run-unit\reports\unit590-coldstarts.txt`):

| window | start | score | pattern | gaps | read |
|---|---|---|---|---|---|
| piece-02 to 12 s | 90 ms | -2.545 | 37 marks, dah share 0.57, separation 2.53, Morse | climb | `A, AND OCTOBER G` |
| | 40 ms | -2.765 | 56 marks, dah share 0.64, separation 2.34 | crossed (lg 195, wg 230) | `T ■ TA ME J ■ ■ A . 1 T Y <AR>` |
| | **70 ms**, taken by both | -2.524 | 37 marks, dah share 0.57, separation 2.54, Morse | climb | `A, AND OCTOBER G` |
| | 120 ms | -2.597 | the same as 70 ms | climb | `A,ANDOCTOBERG` |
| first recording to 8 s | 25 ms | -2.633 | 36 marks, dah share 0.89 | ig 10 against dit 28 | `TTMT T TMT E MTMT E TTTT` |
| | 40 ms | -2.671 | dah share 0.67 | climb | `■ TY E ■ E■ K A E■ E` |
| | **70 ms**, taken by both | -2.607 | 25 marks, dah share 0.32, separation 2.35, Morse | climb | `RER E C EHA T <BT>` |
| | 120 ms | -2.809 | Morse | climb | `■F CHAT<BT>` |
| `121414` to 10 s | 85 ms, likelihood's pick | -2.697 | dah share 0.20, not Morse | climb | `TR 55N 55N VA VA E` |
| | 40 ms | -2.819 | dah share 0.84 | climb | `AQ 00W 00W ■K ■G T` |
| | **70 ms**, the pattern's pick | -2.700 | dah share 0.22, Morse | climb | `TR 55N 55N VA VA T` |
| | 120 ms | -2.770 | not Morse | crossed (lg 475, wg 595) | `ER■■<SK><SK>E` |

Over the 27 windows, likelihood and pattern differ once (`121414` to 10 s). The live loss on piece-02 was the 575 Hz lane on a 5 ms inside gap; with the gaps in the pattern test, that stretch reads 37 of 39 from 13.

**The band alone** (`SilenceAgainstThePath`, nats a hop, every normaliser counted):

| window | the decoded path | one silence, key-up Gaussian | gaps' Gaussian | window's Gaussian | log-Rayleigh at the window's power |
|---|---|---|---|---|---|
| band alone, seed 31 | -2.133 | -2.353 | -2.255 | -2.239 | -2.142 |
| band alone, seed 32 | -2.080 | -2.330 | -2.231 | -2.211 | -2.105 |
| band alone, seed 33 | -2.123 | -2.415 | -2.269 | -2.231 | -2.144 |
| `cq-18wpm-0db` | -2.206 | -2.791 | -2.499 | -2.421 | -2.248 |
| `cq-18wpm-5db` | -2.085 | -3.174 | -2.902 | -2.631 | -2.571 |
| `cq-12wpm-5db` | -1.993 | -3.289 | -3.083 | -2.592 | -2.640 |

With log-Rayleigh silence in the Viterbi, the first proof read:
- 10 dB fading: 30, 19 and 42 of 60.
- 5 dB fading: 20, 19 and 31.
- The band alone: 36, 28 and 37 marks.

Taken out. At the end the band alone decodes 284 to 298 marks (separation 1.06 to 1.10), refused by the pattern test, and prints nothing.

**`144020`'s second station at 600 Hz from 9.5 s** (`.run-unit\reports\unit590-trace-r0.txt`, `-r2-`, `-r3-`):
- **Round one: 10 of 19.**
  - At 18 to 20 s the 500 Hz lane's warm figures fell (separation 1.88 then 1.79 against a kept 1.97). Its cold read was no better (1.66; then crossed gaps), so it kept its rhythm.
  - At 20 s it let go for 625 Hz. 625 Hz read him at separation 1.64 to 1.85 and printed `INTE SSEA GIT IHIE5`, while 600 Hz read 2.25 and stood as its shadow.
- **Round two: 12 of 19.** At 24 s it let go of 625 for 600 Hz.
- **Round three: 14 of 19 on the trace, the row 20 against shipped 25.** From 12.5 s the 500 Hz lane hears someone else (key-down −33.6 against −20.2) and commits nothing.

**The rounds, with the rule on:**

| | shipped | 589's end | round 1 | round 2 | round 3 |
|---|---|---|---|---|---|
| twelve-plus-three (right, wrong, invented) | 297 (318, 20, 1) | 275 (312, 23, 14) | 275 (312, 23, 14) | 285 (314, 21, 8) | 293 (316, 20, 3) |
| spaces right, added | 86, 3 | 86, 12 | 87, 11 | 88, 10 | 89, 9 |
| W1AW (right, wrong, invented) | 2111 (2131, 20, 0) | 2070 (2099, 29, 0) | 2104 (2126, 22, 0) | 2104 | 2104 |
| W1AW pieces 01 / 02 / 03 / 04 | 242 / 388 / 793 / 688 | 242 / 358 / 790 / 680 | 242 / 386 / 790 / 686 | the same | the same |
| fist bank (right, wrong, invented) | 1891 (2612, 675, 46) | 3498 (4260, 661, 101) | 3842 (4451, 545, 64) | 3922 (4478, 538, 18) | 3925 (4475, 532, 18) |
| flat half, right / score | 2173 / 1896 | 2621 / 2396 | 2697 / 2514 | 2716 / 2575 | 2712 / 2574 |
| fading half, right / score | 439 / -5 | 1639 / 1102 | 1754 / 1328 | 1762 / 1347 | 1763 / 1351 |
| short-dah rows | 1193 | 2044 | 2148 | 2186 | 2180 |
| 15 / 5 / 0 dB CQs of 84 | 78 / 71 / 0 | 84 / 84 / 5 | 84 / 84 / 5 | 84 / 84 / 5 | 84 / 84 / 5 |
| nothing after the weak CQs | holds | holds | holds | holds | holds |
| easy / working fixtures | 77 / 4 | 90 / 17 | 90 / 35 | 90 / 35 | 90 / 35 |
| synthetic fixtures (invented) | 46 of 67 (0) | 38 (7) | 37 (20) | 50 (10) | 51 (3) |
| `two-station` | 18 of 27 | 27 | 27 | 27 | 27 |
| letters in silences | 0 | 4 | 4 | 4 | 4 |
| first recording | whole | not whole | not whole | not whole | not whole |
| random carrier, seeds of 20 | 2 | 20 | 20 | 20 | 20 |
| loud noise, steady carrier | nothing | nothing | - | - | nothing |
| strays summed, highest | 26.8, 3.3 | 51.3, 5.3 | 50.3, 5.7 | 48.6, 5.7 | 48.8, 5.3 |
| nine hands at 48 | 9 | 6 | 7 | 9 | 9 |
| short-dah rows' guard | holds | holds | holds | holds | holds |
| first proof | - | holds | - | - | holds |
| band alone | not Morse | marks | - | - | 284 to 298 marks |
| CPU, reader / two finders | - | 100 / 184 s | 100 / 183 s | 100 / 185 s | 100 / 185 s |

**The three matrices, round three with the rule on** (shipped in brackets):

Dah over dit by speed, flat, 20 dB:

| | 10 WPM | 15 WPM | 20 WPM | 25 WPM | row |
|---|---|---|---|---|---|
| dah 1.6 | 59 (60) | 55 (60) | 52 (60) | 50 (60) | 216 (240) |
| dah 2.0 | 52 (59) | 60 (60) | 57 (60) | 60 (22) | 229 (201) |
| dah 2.5 | 57 (59) | 60 (59) | 58 (60) | 60 (60) | 235 (238) |
| dah 3.0 | 60 (60) | 58 (60) | 56 (60) | 60 (60) | 234 (240) |

Dah over dit by signal, flat, 20 WPM:

| | 5 dB | 10 dB | 20 dB | row |
|---|---|---|---|---|
| dah 1.6 | 51 (45) | 47 (36) | 52 (60) | 150 (141) |
| dah 2.0 | 57 (43) | 58 (59) | 57 (60) | 172 (162) |
| dah 2.5 | 59 (53) | 60 (59) | 58 (60) | 177 (172) |
| dah 3.0 | 60 (16) | 54 (59) | 56 (60) | 170 (135) |

Signal by fade, dah 2.5, 20 WPM:

| | flat | 12 dB of QSB at 0.7 Hz | row |
|---|---|---|---|
| 5 dB | 59 (53) | 20 (0) | 79 (53) |
| 10 dB | 60 (59) | 49 (14) | 109 (73) |
| 20 dB | 58 (60) | 31 (13) | 89 (73) |

**The fixtures, round three with the rule on:**

| group | read | wrong | invented | score |
|---|---|---|---|---|
| synthetic fixtures | 51 of 67 | 6 | 3 | 42 |
| receiver: easy | 90 of 90 | 0 | 0 | 90 |
| receiver: working | 35 of 81 | 18 | 0 | 17 |
| receiver: edge | 42 of 81 | 10 | 1 | 31 |
| receiver: ungraded | 73 of 98 | 3 | 20 | 50 |
| CQs 15 / 5 / 0 dB | 84 / 84 / 5 of 84 | 0 / 0 / 3 | 0 | 84 / 84 / 2 |

**The first proof, round three:**
- the 18 WPM ragchew at 10, 5 and 0 dB flat: 60 of 60 at every seed;
- 10 dB fading: 60 at every seed;
- 5 dB fading: 60, 59 and 59;
- the 12 WPM fist with a dah of 1.7: 60, 60 and 59.

**Both scoreboards' rows**, as shipped:
- twelve-plus-three: `| 590 | 2026-10-27 | 318 of 353 | 20 | 1 | **297** | 0 | 86 of 110, 3 added | ... three rounds, 293 with SequenceReader on (275, 285, 293), left off; the board unchanged |`
- W1AW: `| 590 | 2026-10-27 | **2111** | 2131 of 2151 | 20 | 0 | 438 of 440 | ... 2104 with SequenceReader on (2104, 2104, 2104), left off; unchanged |`

## 4. What's blocking us

**Left, one line each:**
- **The reader's opening.** A lane's first window loses `FE` of the first recording, 8 letters on `121414` and 5 on `144020`; it reads the same under either cold-read rule, a fourth thing.
- **Letters in silences, 4,** on `200157`, `143906` and `221502`: not traced to a cause in this unit.
- **The band alone is not silence.** Narrowband noise in dB and a faded station's trough are within 0.01 to 0.04 nats a hop of each other, so no emission tried separates them. Only the pattern test does, and it does: nothing prints.
- **Invented on the bank, 18:** 14 on `fist-r1.6-20wpm-20db-flat`, now printed from 625 Hz, the lane nearest its 615 Hz station rather than one 60 Hz off; why they fall over none of its key-downs is not traced.
- **Strong flat hands at 20 dB** read 1 to 4 letters under shipped at 10, 15 and 20 WPM.
- **W1AW's wrong letters,** 22 against 20, spread over pieces 02 to 04.
- **The random carrier** prints at every seed. Only content would part it from a hand, and whether that limit stands is the owner's to rule.

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
