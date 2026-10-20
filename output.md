# Work instruction 581 - The short dah, measured against the fist bank

## 1. What Claude did

- **Surface and gate:** Claude Code on the development computer. The prompt claimed `PROJECT: Hamlet`, and the tree at `C:\Source\HamLet` confirmed it:
  - `SHACK_FACTS.md`, `docs\cw-scoreboard.md` and `docs\cw-survey-2026-10-09.md` exist.
  - Neither `CoreHMI.sln` nor `MURC.sln` exists.

  Hamlet confirmed. Nothing in this report is evidence about the radio.
- **Branch:** `main`, two commits (the change, then the record), pushed.
- **Run by hand:**
  - `SESSION.lock` was taken through `tools\arbiter\lock.bat take` and released at the end.
  - Nothing was written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` was touched, and no box in `PHASE_PLAN.md` was ticked.
  - Nothing keys or transmits, nothing was written to the radio, and no recording was added, removed or promoted.
- **Measured (section 2):** `CwRules.ShortDah` chosen on as it stood. It ran through one survey (the bank, both boards, every fixture group and the strays), the scoreboard's hard limits with the carrier at twenty seeds, and the weak-end guard.
- **Traced (section 3):**
  - **The new test:** `TheShortDahCostsTests`, which asserts nothing. It reads each cost with the rule off and chosen.
  - **The hook:** each judgement the rule makes is caught at the moment it is made, through a test-only hook, `CwSenderGate.ShortDahJudged`. Nothing in the app sets it. Every caller now passes the pitch of the marks it judges.
  - **The table:** `WhereTheCarrierAndTheHandsStand` sets every judgement on the carrier at its twenty seeds against those on the bank's 48 fists of dah 1.6 and 2.0, condition by condition.
- **The tightening the table names, built:** `CwSenderGate.TwoKindsLeastDitSeconds`. The two means part only at a dit of Morse's fastest, 25 ms at 48 WPM, or longer. Two of the three costs were noise pieces parted at a dit of 20 and 21 ms. It was measured once more on the bank, the boards and every limit.
- **Not shipped:** the rule and its floor stay off by default (`CwRules.OffByDefault`). As shipped nothing reads differently, and both boards are 297 and 2111.
- **The lines,** after one build, with the build servers shut down:
  - the app line passed 278 of 278 in 2 min 15 s;
  - the engine line, at a 1500 s limit, passed 156 of 156 in 9 min 41 s;
  - the survey line passed 1 of 1 in 10 min 4 s, with `--no-build`.
- **Records:**
  - HM-DEC-285 at the top of `DECISIONS.md`, and its `CLAUDE.md` §1 index row.
  - A 581 row in each table of `docs\cw-scoreboard.md`, unchanged.
  - `PHASE_OUTCOME.md` and `PHASE_STATUS.md`, both copies.
  - The survey's `Rerun` line.
  - Version 1.13.265 to **1.13.266**.
  - `CW_SPEC.md` is untouched, since nothing shipped.
- **Recorded under §12.1:** nothing. HM-DEC-285 was ordered by work instruction 581.

## 2. What the owner should expect

- **Short-dah fists now read, with the rule chosen, but it doesn't ship.**
  - **The rows:** the bank's 1.6 and 2.0 rows rise from **135 to 1,193 of 2,880** with the rule and its floor.
  - **The bank:** it reads 2,612 of 5,760 from 1,538, scoring 1,891 from 1,031, with 46 invented from 94.
  - **The boards** hold at 297 and 2111.
  - **The cost:** a carrier keyed at random prints at a second seed, 5207 (`■HAE ■NII`), and the easy fixtures read 77 of 90 from 78. `coverage-easy` reads its `1` as `A M`.
  - **Left off.** In the app nothing changes.
- **The rule in a sentence:** where a sender's lengths show no jump of two, Hamlet splits them into a dit kind and a dah kind and accepts the split only if both kinds are tight, well apart and repeated, and now only if the dit is at least Morse's fastest, 25 ms.
- **The suite:** the app line 278 of 278, the engine line 156 of 156, the survey 1 of 1, every group as before.
- **What will look wrong but is not:**
  - `TheRecordingsScoreboard` is red on the random carrier at seed 5195 alone, the known baseline red since unit 552.
  - The order's twelve-plus-three cost was `RR UR 55N 55N` read `5 HN 55N`. In the trace no mark of that station was re-kinded. The station stopped printing for eight seconds while a noise run at his pitch stood as a sender.
  - The lock tool reports its own lock as `STALE` while held, because the `cmd` that took it has exited.
- **Pushed:** two commits on `main`. The pre-existing uncommitted files are left out: `RUN_LEDGER.md`, `PARKED.md`, `.run-unit\denials.txt`, `.run-unit\reports\*`, and the three `cases-*.txt` files.

### For the owner's ruling

**Ship `CwRules.ShortDah` with its dit floor on by default, at the cost below, or keep it off.**

| | as shipped | the rule as it stood | the rule with the floor | section 4 bar |
|---|---|---|---|---|
| the bank's 1.6 and 2.0 rows | 135 of 2,880 (56, 79) | 1,173 (556, 617) | **1,193** (569, 624) | 1,500 or better: **missed** |
| the bank's 2.5 and 3.0 rows | 1,403 (711, 692) | 1,389 (723, 666) | **1,419** (726, 693) | not falling: held |
| the bank's total | 1,538 of 5,760, score 1,031, 94 invented | 2,562, score 1,828, 44 invented | **2,612, score 1,891, 46 invented** | rising, invented 94 or under: held |
| the twelve-plus-three | 297 (318 of 353, 20 wrong, 1 invented, 86 of 110 spaces) | 293 (315, 21 wrong, 1 invented) | **297** (318, 20, 1, 86 of 110, 3 added) | held |
| W1AW | 2111 (20 wrong, 0 invented, 438 of 440 spaces) | 2111 | **2111** (20, 0, 438 of 440, 11 added) | held |
| the four 5 dB CQs | 71 of 84 | 69 | **71**, nothing in the noise after | held |
| the carrier at twenty seeds | 5195 | 5195, 5207 | **5195, 5207** | one seed: **missed** |
| the easy fixtures | 78 of 90 | 77 | **77** | 78 or better: **missed** |
| noise, the first recording, silences | nothing, whole, nothing | the same | the same | held |
| the strays, highest and summed | 3.3, 22.3 | 3.3, 27.1 | **3.3, 26.8** | none above 6: held |

**The cost if it ships:**
- **A random carrier prints at a second seed,** at 550 Hz, through marks whose dits and dahs stand like a hand's.
- **One easy fixture loses a figure.**
- **The strays rise,** from 22.3 to 26.8 summed, all under the line.

**The gain if it ships:** short-dah fists flat at 20 dB read 59 or 60 of 60 at every speed, but the 2.0 hand at 25 WPM, which reads 22. As shipped, the same fists read 7 letters between them.

**The tightening tried:** a dit floor at Morse's fastest. It took the twelve-plus-three back to 297 and the CQs back to 71. It could not touch the carrier, which parts at a dit of 68 ms.

**Why the carrier can't be separated by the rule's own figures:** its runs overlap the hands' on all five conditions. At every figure that keeps all 48 hands, the carrier is still taken at 19 or 20 seeds (section 3).

**The 1,500 bar can't be met by any tightening:** it sits above the 2.5 and 3.0 hands' own 1,419. In every row the fading half reads about a fifth of the flat half. Flat, the 1.6 and 2.0 hands read 477 and 535 of 720, against 603 and 558 for the 2.5 and 3.0 hands.

**Rejected:**
- **Shipping on the bank's gain alone:** section 4 sets the carrier at one seed.
- **A tighter figure on one of the five conditions:** none parts the carrier from the hands (section 3).

## 3. What you should see

**Dah over dit by speed,** flat, 20 dB, letters right of 60. The two rule columns are the same here:

| | as shipped | the rule as it stood, and with the floor |
|---|---|---|
| dah 1.6 | 7, 0, 0, 0 | 60, 60, 60, 60 |
| dah 2.0 | 0, 0, 0, 0 | 59, 60, 60, 22 |
| dah 2.5 | 60, 60, 60, 60 | 59, 59, 60, 60 |
| dah 3.0 | 60, 60, 60, 60 | 60, 60, 60, 60 |

(10, 15, 20 and 25 WPM, in order.)

**Dah over dit by signal,** flat, 20 WPM, at 5, 10 and 20 dB:

| | as shipped | the rule as it stood | with the floor |
|---|---|---|---|
| dah 1.6 | 0, 2, 0 | 39, 36, 60 | 45, 36, 60 |
| dah 2.0 | 0, 44, 0 | 43, 59, 60 | 43, 59, 60 |
| dah 2.5 | 53, 59, 60 | 53, 59, 60 | 53, 59, 60 |
| dah 3.0 | 16, 59, 60 | 16, 59, 60 | 16, 59, 60 |

**Signal by fade,** dah 2.5, 20 WPM, flat then 12 dB of QSB at 0.7 Hz:

| | as shipped | the rule, as it stood and with the floor |
|---|---|---|
| 5 dB | 53, 0 | 53, 0 |
| 10 dB | 59, 12 | 59, 14 |
| 20 dB | 60, 0 | 60, 13 |

**By fade across the bank,** of 2,880 each: as shipped, flat 1,239 and fading 299. As the rule stood, 2,123 and 439. With the floor, 2,173 and 439.

**The three costs' rows,** each at the moment of the loss:

| cost | where | the run the rule took | the truth | what printed |
|---|---|---|---|---|
| `cw-2026-10-08-121357` | 10.85 s, 799 Hz | 16 marks of 20 to 40 ms; 6 dits centre 21 ms, 10 dahs centre 33 ms; dah over dit 1.60, 4.2 spreads apart, widths 0.100 and 0.114 | the keying map's station at 798 Hz: 67 dits centre 77 ms, 16 dahs centre 207 ms, dah over dit **2.70** | off, `RR UR 55N 55N`; on, nothing for 10.7 to 18.4 s, then `5 HN 55N`. None of his marks was re-kinded; the noise run at his pitch stood while he sent `RR UR` |
| `cq-25wpm-5db` | 2.9 s, 619 to 625 Hz | 10 marks of 20 to 40 ms; 6 dits centre 20 ms, 4 dahs centre 35 ms; dah over dit 1.74, 5.4 spreads apart, widths 0.100 and 0.102 | the recipe: dit 48 ms, dah 144 ms, dah over dit **3.00** | off, `NQ CQ CQ DE N0CALL N0CALL K`; on, `S E E` from the pieces, then `R` for his `C`, so `S E ERQ CQ DE...`, 18 of 21 |
| the carrier, seed 5207 | 7.5 s, 550 Hz, 75 Hz off its tone | 12 marks of 55 to 180 ms, widest neighbour jump 1.81; 9 dits centre 68 ms, 3 dahs centre 158 ms; dah over dit 2.30, 7.3 spreads apart, widths 0.115 and 0.100 | random keying, marks 30 to 300 ms | `■HAE ■NII` |

**With the floor:** `121357` reads `RR UR 55N 55N VA VA BK BKT` and `cq-25wpm-5db` `NQ CQ CQ DE N0CALL N0CALL K`, both as shipped. The carrier at 5207 still reads `■HAE ■NII`.

**Where the carrier and the hands stand,** over every judgement the rule took with its floor (least, median, most):

| | dit centre | marks | widest cluster | dah over dit | spreads apart |
|---|---|---|---|---|---|
| the carrier, twenty seeds | 25.7, 66.6, 190.7 ms | 10, 16, 64 | 0.10, 0.11, 0.12 | 1.50, 1.75, 3.13 | 4.0, 5.1, 10.4 |
| the bank's 1.6 fists | 34.3, 81.6, 164.4 ms | 10, 16, 152 | 0.10, 0.10, 0.12 | 1.50, 1.64, 2.04 | 4.0, 4.8, 7.1 |
| the bank's 2.0 fists | 36.8, 82.0, 177.4 ms | 10, 16, 159 | 0.10, 0.10, 0.12 | 1.50, 2.00, 2.63 | 4.0, 6.8, 8.9 |

**Each condition tightened alone,** on top of the floor (the carrier's seeds still taken, and the 48 hands still taken):

| tightened to | carrier seeds | hands |
|---|---|---|
| marks 16 | 20 | 48 |
| marks 20 | 8 | 40 |
| each kind 4 | 20 | 48 |
| each kind 5 | 20 | 48 |
| widest 0.10 | 19 | 48 |
| widest 0.08 | 0 | 0 |
| dah over dit 1.55 | 20 | 48 |
| spreads 4.5 | 20 | 48 |
| spreads 5 | 20 | 48 |
| dah over dit under 2 | 20 | 48 |

The hand's margin above the carrier is nought on every condition. Every spread is floored at 0.10, so a widest cluster of 0.08 takes everything.

**The carrier at twenty seeds,** seeds 5193 to 5212:
- **As shipped:** prints at 5195 alone.
- **The rule as it stood, and with the floor:** prints at 5195 (`IDSEEIIEEEIIESEIEEES E ETU T EEI N`) and 5207 (`■HAE ■NII`), and nothing at the other eighteen.

**The scoreboard rows, as shipped:**

| unit | right | wrong | invented | score | spaces |
|---|---|---|---|---|---|
| 581, the twelve-plus-three | 318 of 353 | 20 | 1 | **297** | 86 of 110, 3 added |
| 581, W1AW | 2131 of 2151 | 20 | 0 | **2111** | 438 of 440, 11 added |

**The straight-key hands** (1.7, 2.0 and 2.5 at 12 WPM, three seeds each) read 60 of 60 at all nine with the rule and its floor.

## 4. What's blocking us

- **The ruling above:** ship the short dah with its floor on at the cost of a second carrier seed and one easy figure, or keep it off.
- **The 1,500 bar** for the short-dah rows sits above what the long-dah rows read, 1,419. A bar on the flat half, or one beside the long-dah rows, would be a bar the rule can meet.
- **The carrier at 5207** is read 75 Hz off its tone, through marks a hand could make. A carrier keyed at random from 30 to 300 ms makes dits and dahs that stand like a hand's within one pitch. What parts it from a hand is not in the five figures, and may be its key-down share or its gaps.
- **The fade** remains the largest hole: every row's fading half reads about a fifth of its flat half.
- **Choices taken, named under HM-DEC-276:**
  - **The floor is Morse's fastest dit,** `CwDecoder.FastestPlausibleWpm`. It is the tree's own ceiling and not a fitted figure.
  - **The trace hook** passes each caller's pitch through an optional parameter, and does nothing while no test sets it.

### Asks still outstanding

- **Unit 581, 2026-10-20:** ship `CwRules.ShortDah` with its dit floor on, at a second carrier seed and 77 of 90 easy, or keep it off. Waiting on the owner. The rule and its floor sit in the tree, off.
- **Unit 575, 2026-10-19:** what could part a hand's letter gaps from his word gaps, since they overlap in length and no line drawn from his dit alone parts every hand. Waiting on the owner. `CwRules.StraddleSplit` sits in the tree, off.
- **Unit 562, 2026-10-08:** what next for the junk before a weak call, given the 0.4 start costs the start of every real call. Waiting on the owner. No change for it sits in the tree.
- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by unit 524 and by the shadow rule. Still open before a sender stands, and for a sender 50 Hz or nearer.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
