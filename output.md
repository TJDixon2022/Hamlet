## 1. What Claude did

Claude Code on the development computer, branch `main`. The prompt named `PROJECT: Hamlet`, and the order's gate and `PROJECT_CARD.md` confirmed it. Nothing in this report is evidence about the radio. Version 1.13.278 to 1.13.279. Work instruction 592, ruling HM-DEC-296, both free.

**No file under `src\Hamlet.RadioEngine\Cw\` changed, and nothing was written to the radio.**

Built:

- **The keyer's gap recipe** (`CwFistBank.Key`, `Walk`): log-normal marks and gaps, and the two habits, digits as words and short words together.
- **`CwWildBank`:** 48 hands, with each hand's ambiguity. The survey group is `wild bank`.
- **`cw-2026-10-19-141708.truth.md`:** the order's draft, headed as it asks. The survey group is `wild hands`, scored per 20 s with a `?` alone matching anything (`CwWildHands`).
- **`TheWildBankTests`, 5 tests, on the engine line:** `TheBankIsMade`, `TheHandsSendTheirRecipes`, `TheMachineRecipeIsTheBank`, `AnUnknownMatchesAnything`, `TheDraftTruthIsBesideTheCapture`. None guards a score.
- **`TheBaselineTrace`:** the section 4 table, in the `Survey` category, off both lines.
- **The record:** HM-DEC-296, `CLAUDE.md` row, `CW_SPEC.md` section 2.1, both scoreboard tables, both `PHASE_OUTCOME.md` and `PHASE_STATUS.md` copies, the survey rewritten with its `Rerun` line.

Judgement calls the order left open:

- **`machine`'s own speed** is the bank's 15 WPM, since its other speed is 20.
- **A bare `?` is unknown; the `?` in `HW?` was sent.** A bracketed line in the truth is a note and is not scored, so Bryan's call at 100 s is not in Ken's truth.
- **The ambiguity midpoint is arithmetic**, as the order words it.
- **Real hands' sent figures** are the order's recipe table, not measured here. Bryan's word gap is 13.1 dits in its prose and 13.7 in its table; the table is used.
- **Bryan's "end" in the trace is 280 s**, where the truth says he stops.

## 2. What the owner should expect

**The existing boards did not move.** The twelve-plus-three is 308, W1AW 2101, the fist bank 3,898. Apart from the two new groups and the `Rerun` line, the survey document is byte-identical to the last commit. Nothing changes on screen or at the radio.

**The wild bank's baseline: 3133 of 3528 letters, nothing invented, but 400 spaces added.** The reader does better on these hands than the analysis led us to expect. Each recipe is out of 588:

- **`machine`, 572:** the control. Reads almost whole, 3 spaces added.
- **`farnsworth`, 567:** the letters read, every word gap but one is found, and it adds 175 spaces, the most of any hand.
- **`steady`, 524:** reads well, 34 spaces added.
- **`mobile`, 522:** the letters read, but it adds 110 spaces and misses 66, and digits come out spaced like `5 7 9`.
- **`tight`, 489:** weaker, mostly on the fist bank's text, 36 to 51 of 60 there.
- **`bug`, 459:** the hardest, 114 wrong. It also has the most ambiguous gaps.

**How much of each hand is readable by timing at all.** This is the share of sent gaps close to a boundary, so lower is easier:

| `farnsworth` | `steady` | `mobile` | `tight` | `machine` | `bug` |
|---|---|---|---|---|---|
| 7% | 9% | 12% | 13% | 20% | 30% |

`machine` scoring 20% is not a fault in the measure: its letter and word gaps, 2 to 3.5 and 4 to 6 dits, sit close together. It still reads best, so ambiguity alone doesn't predict the score.

**The two real hands, against the draft truth, per 20 s:**

| Ken N3AFS: 43 of 76 | right | letters | | Bryan KB2UZK/M: 118 of 144 | right | letters |
|---|---|---|---|---|---|---|
| 0-20 s | 2 | 19 | | 120-140 s | 16 | 18 |
| 20-40 s | 11 | 17 | | 140-160 s | 16 | 17 |
| 40-60 s | 10 | 17 | | 160-180 s | 8 | 14 |
| 60-80 s | 13 | 19 | | 180-200 s | 16 | 21 |
| 80-100 s | 0 | 20 | | 200-220 s | 15 | 18 |
| 100-120 s | 7 | 17 | | 220-240 s | 14 | 17 |
| | | | | 240-260 s | 18 | 21 |
| | | | | 260-280 s | 15 | 18 |
| | | | | 280-300 s | 0 | 2 |

**Two things will look different from what you were told.**

- **Ken does not read as `ETETTE TTTTTE` on this replay.** He reads `NR GITTSNI■GH TU NAME ES KEN KEN`, with a quarter of his letters `E`, `T` or `■`. The capture was made on 1.13.256, before the reader shipped. So this replay doesn't reproduce E's and T's on hands you copy by ear.
- **Ken's 43 is a little too high.** Of it, 7 come from 100-120 s, where the letters are really Bryan's `N3AFS DE KB2UZ`, aligned against the end of Ken's truth. His own sign-off at 80-100 s falls under the `?` and scores nothing. The truth needs your ear there.

## 3. What you should see

**The baseline trace.** Figures are inside / letter / word gaps, dit and dah, in ms. Each learned cell is the printed lane at that moment.

| hand | sent or measured | learned at 10 s | learned at 30 s | learned at the end | `E`, `T` or `■` |
|---|---|---|---|---|---|
| Ken N3AFS (analysis) | 136 / 344 / 608, dit 80, dah 232 | 500 Hz: 130 / 280 / 568, dit 75, dah 210 | 500 Hz: 148 / 385 / 490, dit 63, dah 223 | 120 s, 500 Hz: 115 / 290 / 780, dit 60, dah 220 | 25% |
| Bryan KB2UZK/M (analysis) | 104 / 286 / 891, dit 65, dah 221 | 130 s, 500 Hz: 103 / 278 / 845, dit 70, dah 218 | 150 s, 500 Hz: 108 / 235 / 800, dit 65, dah 225 | 280 s, 500 Hz: 110 / 278 / 665, dit 55, dah 210 | 21% |
| `mobile`, 20 dB, own speed, QSO text (sent) | 103 / 273 / 864, dit 65, dah 220 | 625 Hz: 110 / 323 / 490, dit 70, dah 228 | 625 Hz: 110 / 233 / 895, dit 65, dah 225 | 84 s, 625 Hz: 100 / 265 / 490, dit 70, dah 213 | 26% |
| `bug`, 20 dB, own speed, QSO text (sent) | 140 / 358 / 596, dit 82, dah 225 | nobody printing | 625 Hz: 145 / 370 / 478, dit 70, dah 223 | 94 s, 625 Hz: 140 / 388 / 630, dit 90, dah 240 | 32% |

What each printed:

- **Ken:** `U UE N DEALL<BT> W A NST SI LGN 58E XTHI I N W N ENSENG TOI ,P T ■ NR GITTSNI■GH TU NAME ES KEN KEN UGV ■LE UTI2UM WEEENIWRNS A N3AFS DE KB2UZY / TE`
- **Bryan:** `E T■O GM KEN NICE TO M EE T S TNXFER RPRT UR I 5 7 9 5 7 N LIL T IAM F M OBILE SNN T A MY NAME BRYAN BRYAN ES Q TH E S E CHICOP E , MA C H I C OP E E MW MA PARKED IN MY MOBILE <BT> SO TITH KEN N3AFS DE KB■ZY/ M II`
- **`mobile`:** `■KENNICET OMEET UES T N X FER RPRT U RRST 5 7 9 5 7 9 IAMIYO BILE S TN H <AR>A ME BRYAN BR )N ESQTH I ■ H■ OPE E, M A`
- **`bug`:** `GA UEN NICEEONEET UES TNX FER R PRT UR ■T 57■ HE79 IAM IN MOHILE STI HR NA M T NEE EN ■A N B R■A N E S MTTSE IS CHICOPEE■ MA`

**The wild bank matrix.** Letters right, QSO text of 87, then the fist bank text of 60:

| recipe | own dit, 20 dB | own dit, 10 dB | 20 WPM, 20 dB | 20 WPM, 10 dB |
|---|---|---|---|---|
| `mobile` | 75 / 56 | 79 / 57 | 71 / 54 | 79 / 51 |
| `bug` | 64 / 46 | 74 / 43 | 67 / 44 | 71 / 50 |
| `farnsworth` | 80 / 59 | 86 / 60 | 83 / 60 | 83 / 56 |
| `tight` | 77 / 44 | 76 / 48 | 85 / 36 | 72 / 51 |
| `steady` | 83 / 50 | 79 / 56 | 75 / 50 | 73 / 58 |
| `machine` | 85 / 58 | 87 / 60 | 87 / 58 | 83 / 54 |

| recipe | right | wrong | invented | score | spaces |
|---|---|---|---|---|---|
| `mobile` | 522 of 588 | 35 | 0 | 487 | 118 of 184, 110 added |
| `bug` | 459 of 588 | 114 | 0 | 345 | 144 of 184, 47 added |
| `farnsworth` | 567 of 588 | 12 | 0 | 555 | 183 of 184, 175 added |
| `tight` | 489 of 588 | 75 | 0 | 414 | 150 of 184, 31 added |
| `steady` | 524 of 588 | 30 | 0 | 494 | 176 of 184, 34 added |
| `machine` | 572 of 588 | 8 | 0 | 564 | 152 of 184, 3 added |
| **20 dB** | 1547 of 1764 | 153 | 0 | 1394 | 454 of 552, 175 added |
| **10 dB** | 1586 of 1764 | 121 | 0 | 1465 | 469 of 552, 225 added |
| **total** | 3133 of 3528 | 274 | 0 | 2859 | 923 of 1104, 400 added |

Nothing printed in a silence. The per-hand table and each hand's ambiguity are in `docs\cw-survey-2026-10-09.md`.

**The two real hands' stretches:**

| stretch | letters | right | wrong | invented | under a `?` | truth missing | spaces right / missing / added | printed |
|---|---|---|---|---|---|---|---|---|
| Ken 0-20 s | 19 | 2 | 6 | 0 | 2 | 0 | 1 / 1 / 1 | `U UE N DEALL<BT> W A NST SI LG` |
| Ken 20-40 s | 17 | 11 | 6 | 0 | 0 | 1 | 3 / 1 / 2 | `N 58E XTHI I N W N ENSEN` |
| Ken 40-60 s | 17 | 10 | 6 | 0 | 0 | 0 | 2 / 1 / 2 | `G TOI ,P T ■ NR GITTSNI` |
| Ken 60-80 s | 19 | 13 | 2 | 0 | 4 | 0 | 3 / 0 / 0 | `■GH TU NAME ES KEN KEN UG` |
| Ken 80-100 s | 20 | 0 | 0 | 0 | 20 | 0 | 0 / 0 / 0 | `V ■LE UTI2UM WEEENIWRNS` |
| Ken 100-120 s | 17 | 7 | 5 | 0 | 5 | 7 | 2 / 2 / 2 | `A N3AFS DE KB2UZY / TE` |
| **Ken total** | 109 | 43 of 76 | 25 | 0 | 31 | 8 | 11 of 16, 7 added | |
| Bryan 120-140 s | 18 | 16 | 2 | 0 | 0 | 1 | 5 / 0 / 3 | `E T■O GM KEN NICE TO M EE` |
| Bryan 140-160 s | 17 | 16 | 1 | 0 | 0 | 4 | 6 / 2 / 2 | `T S TNXFER RPRT UR I 5 7` |
| Bryan 160-180 s | 14 | 8 | 2 | 0 | 4 | 1 | 3 / 1 / 3 | `9 5 7 N LIL T IAM F M O` |
| Bryan 180-200 s | 21 | 16 | 3 | 0 | 2 | 0 | 4 / 0 / 1 | `BILE SNN T A MY NAME BRYAN B` |
| Bryan 200-220 s | 18 | 15 | 2 | 0 | 0 | 1 | 4 / 0 / 2 | `RYAN ES Q TH E S E CHICOP` |
| Bryan 220-240 s | 17 | 14 | 2 | 0 | 0 | 0 | 4 / 0 / 7 | `E , MA C H I C OP E E MW MA P` |
| Bryan 240-260 s | 21 | 18 | 2 | 0 | 0 | 0 | 6 / 0 / 0 | `ARKED IN MY MOBILE <BT> SO TIT` |
| Bryan 260-280 s | 18 | 15 | 3 | 0 | 0 | 2 | 4 / 1 / 1 | `H KEN N3AFS DE KB■ZY/ M` |
| Bryan 280-300 s | 2 | 0 | 0 | 0 | 0 | 0 | 0 / 0 / 0 | `II` |
| **Bryan total** | 146 | 118 of 144 | 17 | 0 | 6 | 9 | 36 of 40, 19 added | |

**The survey's groups, before and after.** Every old group's row is byte-identical, checked by a diff of the whole document against the last commit:

| group | before | after |
|---|---|---|
| synthetic fixtures | 42 | 42 |
| receiver fixtures: easy | 90 | 90 |
| receiver fixtures: working | 19 | 19 |
| receiver fixtures: edge | 35 | 35 |
| receiver fixtures: ungraded | 45 | 45 |
| synthetic CQs: 15 dB | 84 | 84 |
| synthetic CQs: 5 dB | 84 | 84 |
| synthetic CQs: 0 dB | 2 | 2 |
| the boards: twelve-plus-three | 308 | 308 |
| W1AW pieces | 2101 | 2101 |
| the fist bank | 3898 | 3898 |
| wild bank | - | 2859 |
| wild hands | - | 119 |

**The lines:** engine 168 of 168, the five new tests included; app 278 of 278; survey passed. Both lines ran before the record was written and the version bumped. After that, only `DecisionLogOrderTests` was run again.

**CPU.** The survey went from 16 m 23 s to 23 m 16 s, up 6 m 53 s, or 42%. Summed over each read's own time, `wild bank` is 899 s and `wild hands` 111 s, against 2269 s for the old groups, up 45%. That sum leaves out the boards' own reads. Process CPU for the new run is 4026 s; the old binary did not print that figure, so no before is given.

## 4. What's blocking us

Nothing blocks the next unit. Left, a line each:

- **`machine` reproduced the bank, bit for bit.** SHA-256 over all 96 fists' edges and audio, taken before the keyer changed, still matches and is pinned in `TheMachineRecipeIsTheBank`. The four machine hands sending the bank's text equal those fists exactly and read 60, 58, 54 and 58, as those fists do.
- **The order's premise is corrected:** the bank's keyer does not send 1 : 3 : 7. It sends letter gaps of 2 to 3.5 dits and word gaps of 4 to 6, uniform. `machine` keeps that, since bit for bit was the bar, and its ambiguity uses medians of 2.75 and 5.
- **Which of the truth's `?` the reader can fill:** two look real. **`MY`** in Bryan's `HR ? NAME`, and **`TU`** in Ken's `PITTSBURGH ? NAME`. The rest is not readable: `UUENDEALL` and `WA` in Ken's opening, nothing before `QTH`, `UGV■LEUTI2UM...` before `HW?`, and `LILT` in Bryan's `579 ? I AM`. The draft is unchanged; these are for your ear.
- **The truth at Ken's end needs your ear.** Bryan's `N3AFS DE KB2UZ...` at 100-120 s scored against Ken's closing line, as section 2 says.
- **After 280 s,** 17 letters printed at 425 Hz, outside Bryan's 60 Hz: `RRHE N3AFS ■ EIC TOME`. They look like another station and are not scored, as the truth says.
- **`tight` reads 89% of the QSO text and 75% of the fist bank's text.** `bug` is 79% and 76%; on the other four the two texts differ by 3 points or less. Not traced.
- **The score can flatter a poor read.** A free `?` lets any run go unscored, and the alignment finds chance matches. Treat Ken's 57% as an upper bound.
- **The analysis file `ANALYSIS-2026-11-03-wild-cw.md` is not in this tree,** so its measurements are quoted from the order and were not checked.
- **A stray file:** `src\Hamlet.RadioEngine\Cw\CwSequenceReader.cs.new`, 0 bytes, untracked, dated 2026-10-27. Not from this unit; left alone and not committed.
- **The survey's memory notice:** the harness reported the run stopped for low memory, but its test host kept running at 0.25 GB and finished. It was one run, not a retry. The pressure came from elsewhere on the machine, including another session's build.
- **Not committed, as before:** the earlier changes to `PARKED.md`, `RUN_LEDGER.md` and `.run-unit\`.
- **No hook was missing:** the reader's existing `LaneReadings` gave everything section 4 asked for.

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
