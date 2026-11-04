## 1. What Claude did

Claude Code on the development computer, branch `main`. The prompt named `PROJECT: Hamlet`, and the order's gate and `PROJECT_CARD.md` confirmed it. Nothing in this report is evidence about the radio. Version 1.13.280 to 1.13.281. Work instruction 594, ruling HM-DEC-298. The tag `before-gap-learning` stands. `SESSION.lock` was taken and released. Nothing was written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` was touched, and no box was ticked in `PHASE_PLAN.md`. The draft truth was not edited. Nothing keys or transmits, nothing was written to the radio, and no recording was added, removed or promoted.

**Round one** (`77e19a3d`): the two-cluster mark fit removed from `LearnByFit`, so dit, dah and the line between them are the medians' own; the two traces; the noise read both ways. The wild bank 2888, `bug` 221, the twelve-plus-three 244.

**Round two** (`4154ce43`): traced on `bug` at 20 WPM, 20 dB, a lane 50 Hz off him held the terminal from 6 to 20 s with his inside gaps laid as letter gaps, so `CQ CQ` printed `TETE TTET`. A lane whose decode is not valid no longer takes the terminal; the printed lane is not judged so. The wild bank 3028, `bug` 278, the twelve-plus-three 300.

**Round three** (`f426947f`): `bug` is the largest loss. Taking each part of the rule out in turn on his eight hands, he read 340 with the gap fit out and at most 294 with any other part out, against 278 with everything in; taking out the terminal test dropped him to 221. His gaps spread across any line. A fit whose lines sit among more than 0.25 of his gaps (`FitNearMost`) now keeps the medians' classes. The wild bank 3091, `bug` 344, the twelve-plus-three 291.

**Departures, said plainly:**
- The attribution runs in round three and the two figures for `FitNearMost` used a temporary switch in the engine. It was taken out before the round was committed and never reached `main`.
- I took 0.25 over 0.2 on the measured bank figures (3091 against 3064, `bug` 344 against 342, the boards alike). 0.2 was the figure the trace alone suggested.
- I added one trace-only line to the reader, and the rule stays off, so it changes no reading.

**Recorded:** HM-DEC-298, the order's own ruling record, in the tree in full.

**Lines:** engine 168 of 168, app 278 of 278, `DecisionLogOrderTests` 2 of 2, the survey once with the rule off. The harness stopped three wrappers for low memory, and each time the test hosts kept running to the end. Nothing was run twice for it. The first stop left the rule's temporary "on" in place during the guards of round one, and I put it back when they finished; it was never committed on.

## 2. What the owner should expect

**It does not ship, and it is not for your ruling.** The wild bank cleared its bar at **3091**, but `bug` is 1 under, the twelve-plus-three is 14 under the ruling's band, W1AW is 3 under, wrong rose on both boards and the easy fixtures fell. What you run reads exactly as it did: with the rule off, the survey is byte-identical to the last one but for its `Rerun` line.

**The whole picture against the shipped reader:**

| | as shipped | rule on | bar | |
|---|---|---|---|---|
| wild bank score | 2859 | **3091** | 3,050 | met |
| spaces added | 400 | 263 | 270 | met |
| invented | 0 | 0 | 5 | met |
| `mobile` | 487, 110 added | 529, 72 added | within 10 of shipped | met |
| `bug` | 345, 114 wrong | **344**, 106 wrong | 345 | **1 under** |
| `farnsworth` | 555, 175 added | 565, 85 added | 545 | met |
| `tight` | 414 | 499 | within 10 of shipped | met |
| `steady` | 494 | 570 | within 10 of shipped | met |
| `machine` | 564 | 584 | within 10 of shipped | met |
| twelve-plus-three | 308, 13 wrong, 4 invented, 92 of 110 spaces | **291**, 15 wrong, 3 invented, 89 of 110 | 308, wrong 13 | **17 under, wrong up 2, spaces worse** |
| W1AW | 2101, 24 wrong | **2098**, 26 wrong | 2101, wrong 24 | **3 under, wrong up 2** |
| fist bank | 3898 | 4268 | 3,898 | met, and its guard passes |
| fading half, right | 1771 | 1993 | as guarded | better |
| flat half, right | 2690 | 2720 | as guarded | better |
| first recording | `FF C HAT<BT> BEST 7V 73 <SK> KC4ZGP ■` | `FER C HAT<BT> BEST 7V 73 <SK> KC4ZGP DEWA` | whole | met |
| 5 dB CQs | 84 of 84 | 84 of 84 | 84 | met |
| easy fixtures | 90 of 90 | **74 of 90** | 90 | **`tightfist-easy` 14 to 0** |
| `two-station` | 27 of 27 | 27 of 27 | 27 | met |
| noise, steady carrier | nothing | nothing | nothing | met |
| highest strays per 10 s | 4.3 | 2.0 | 6 | met |
| summed strays, reported | 46.5 | 29.7 | | better |
| random carrier, reported | 20 seeds of 20 | **6 seeds of 20** | | better, and measured both ways this time |
| letters in silences | 1 | 1 | as guarded | met |
| CPU, W1AW piece-04, 283 s | 120.8 s | 132.2 s, +9% | under real time | met |

The working fixtures fell from 19 to 5 and the edge fixtures from 35 to 21; both are reported, not guarded.

**Ken, against the draft truth, 43 of 76 both ways.** The loss on HM-DEC-297 is gone:

| stretch | right, shipped | right, on | prints, shipped | prints, on |
|---|---|---|---|---|
| 0-20 s | 2 | 2 | `U UE N DEALL<BT> W A NST SI LG` | `U UE N DEA L<BT> W A NST SI LG` |
| 20-40 s | 11 | 11 | `N 58E XTHI I N W N ENSEN` | `N 58T XTHI I N W N ENS EN` |
| 40-60 s | 10 | 10 | `G TOI ,P T ■ NR GITTSNI` | `G TOI,P T PA NR GITTSNI` |
| 60-80 s | 13 | 13 | `■GH TU NAME ES KEN KEN UG` | `■GH EU NAME ES KEN KEN UG` |
| 80-100 s | 0 | 0 | `V ■LE UTI2UM WEEENIWRNS` | `V ■LE UTI2UM W EEENIWRNS` |
| 100-120 s | 7 | 7 | `A N3AFS DE KB2UZY / TE` | `ET N3AFS DE KB2UZY / TE` |

**Bryan, against the draft truth, 118 to 122 of 144:**

| stretch | right, shipped | right, on | prints, shipped | prints, on |
|---|---|---|---|---|
| 120-140 s | 16 | 17 | `E T■O GM KEN NICE TO M EE` | `E TELO GM KEN NICE TO ME E I` |
| 140-160 s | 16 | 15 | `T S TNXFER RPRT UR I 5 7` | `T S TNXFER RPRT UR I 5 7` |
| 160-180 s | 8 | 10 | `9 5 7 N LIL T IAM F M O` | `9 5 7 N LIL T IAM IN MO` |
| 180-200 s | 16 | 16 | `BILE SNN T A MY NAME BRYAN B` | `BILE SNN TET IY NAME BRYAN B` |
| 200-220 s | 15 | 15 | `RYAN ES Q TH E S E CHICOP` | `RYAN ES Q TH IS I FHICOP` |
| 220-240 s | 14 | 14 | `E , MA C H I C OP E E MW MA P` | `E , MA CHICOP E E MW MA P` |
| 240-260 s | 18 | 18 | `ARKED IN MY MOBILE <BT> SO TIT` | `ARKED IN MY MOBILE <BT> SO TIT` |
| 260-280 s | 15 | 17 | `H KEN N3AFS DE KB■ZY/ M` | `H KEN N3AFS DE KB2UZY/ M` |
| 280-300 s | 0 | 0 | `II` | `IEE` |

**What the two traces found:**
- **`bug`: the slide is not gone, and the mark fit did not cause it.** With the medians his dah still falls from 238 ms at 46 s to 165 ms at 62 and 64 s, about as it did with the fit replayed. My reading in the last report, that the mark fit was hurting him, was wrong: removing it alone took him from 254 to 221. His 344 came from the two later rounds.
- **Ken: the mark fit was his loss.** With it out he reads 13 at 60-80 s and 43 of 76 whole, and his classes come out near his measured gaps. The one letter that differs in that span is a mark fault, the same with the rule on or off.

**Why it fails, by path:**
- **The twelve-plus-three:** net 16 letters lost, and `221548`'s second station alone is 18, printing nothing at 18 to 30 s and 498 Hz. He began mid-window, so his 12 s windows still hold noise read as `E` and `T`, and the round two test refuses him. Round three cost a further 9 points, 8 of them on `221828`, and one invented letter on `121324`.
- **`tightfist-easy`:** prints nothing, 14 down. It sends `TEST DE TEST`, 7 letters of 11 `E` or `T`, which would fail the same test on honest text. I did not trace that, so treat it as likely rather than found.
- **W1AW:** the same 2098 and 26 wrong as with the mark fit in, so neither this unit's change nor its rounds moved it.

**What the wild bank still loses:** it reads as the marks rather than the gap fit. `bug` holds 106 of the bank's 186 wrong letters and his dah still slides. That is a reading of the trace, not a count of faults, and marks were off limits. **What the boards lose is a third thing:** round two's test at the terminal judges a whole window, so it refuses a station who has just begun and may refuse real text heavy in `E` and `T`. Those figures are measured; that a narrower test would recover them is not.

**What will look wrong but is not:** `CwRules.GapLearning`, `FitNearMost` and the terminal test are in the tree with the rule off. The guards still hold the old first-recording read, which is correct while the rule is off. `TheGapLearningTests` is not on the carry-forward list.

## 3. What you should see

**The `bug` trace, 20 dB, own speed, QSO text**, a lane's warm chain with the first window cold, the third pass of each window, medians driving. Each pass of every window, and the same chain with the fit replayed driving, are in `.run-unit\reports\unit594-final-measures.txt`.

| window ends, s | medians dit / dah, ms | fit dit / dah, ms | dah share | letters right |
|---|---|---|---|---|
| 40 | 75 / 225 | 70 / 204 | 0.32 | 7 of 10 |
| 42 | 75 / 218 | 69 / 204 | 0.40 | 7 of 10 |
| 44 | 70 / 230 | 70 / 206 | 0.40 | 7 of 10 |
| 46 | 85 / 238 | 75 / 211 | 0.36 | 6 of 11 |
| 48 | 88 / 243 | 80 / 235 | 0.47 | 8 of 12 |
| 50 | 85 / 235 | 81 / 234 | 0.50 | 9 of 11 |
| 52 | 95 / 230 | 86 / 225 | 0.45 | 11 of 12 |
| 54 | 98 / 223 | 91 / 219 | 0.40 | 11 of 13 |
| 56 | 95 / 223 | 90 / 212 | 0.41 | 10 of 13 |
| 58 | 85 / 225 | 82 / 207 | 0.35 | 10 of 12 |
| 60 | 75 / 170 | 75 / 178 | 0.42 | 8 of 12 |
| 62 | 70 / 165 | 72 / 165 | 0.43 | 10 of 13 |
| 64 | 85 / 165 | 75 / 165 | 0.46 | 7 of 11 |
| 66 | 88 / 190 | 78 / 177 | 0.38 | 5 of 10 |

With the fit replayed driving, the same windows at the third pass: 46 s 85 / 223, dah share 0.48, 6 of 11; 52 s 88 / 220, 0.48, 12 of 12; 58 s 78 / 205, 0.42, 10 of 12; 60 s 75 / 170, 0.45, 7 of 12; 62 s 70 / 165, 0.50, 9 of 13; 64 s 78 / 158, 0.57, 6 of 11; 66 s 73 / 165, 0.52, 7 of 10.

**The Ken trace, 60-80 s, rule on.** Measured gaps 136 / 344 / 608 ms. The full start tables and each pass are in `.run-unit\reports\unit594-final-ken.txt`.

| window, s | start taken | Morse | E, T, ■ | near lines | validity | pass 1 ig/lg/wg | pass 2 | pass 3 |
|---|---|---|---|---|---|---|---|---|
| 50-62 | 120 ms | yes | 25% | 3% | 0.25 | 118/312/700 | 133/319/630 | 132/322/578 |
| 54-66 | 120 ms | yes | 38% | 13% | 0.38 | 117/311/805 | 121/323/735 | 125/328/735 |
| 58-70 | 120 ms | yes | 13% | 19% | 0.19 | 123/294/678 | 130/328/770 | 131/318/664 |
| 62-74 | 120 ms | yes | 0% | 19% | 0.19 | 122/331/763 | 131/349/771 | 131/357/729 |
| 66-78 | 120 ms | no | 10% | 12% | 0.12 | 117/326/795 | 131/341/843 | 135/330/706 |
| 70-82 | 120 ms | yes | 22% | 13% | 0.22 | 125/344/768 | 123/359/829 | 135/361/858 |

Letters that differ from the draft truth at 60-80 s, rule on and off alike. Letters under a `?` are not scored, so `■GH` and `EU` are not rows.

| second | sent | printed | sent as | laid as | fault |
|---|---|---|---|---|---|
| 69.10 | I | E | `..` | `.` | mark: 2 sent, 1 laid |

**The wild bank, right / score / spaces added:**

| recipe | as shipped | rule on |
|---|---|---|
| `mobile` | 522 / 487 / 110 | 550 / 529 / 72 |
| `bug` | 459 / 345 / 47 | 450 / 344 / 43 |
| `farnsworth` | 567 / 555 / 175 | 573 / 565 / 85 |
| `tight` | 489 / 414 / 31 | 542 / 499 / 29 |
| `steady` | 524 / 494 / 34 | 576 / 570 / 22 |
| `machine` | 572 / 564 / 3 | 586 / 584 / 12 |
| total | 3133 / 2859 / 400 | 3277 / 3091 / 263 |

**Every hand's score**, in the order 20 WPM 10 dB bank and QSO, 20 WPM 20 dB bank and QSO, own 10 dB bank and QSO, own 20 dB bank and QSO:

| recipe | as shipped | rule on |
|---|---|---|
| `mobile` | 46, 75, 51, 63, 55, 74, 54, 69 | 52, 81, 54, 76, 55, 74, 57, 80 |
| `bug` | 40, 57, 34, 48, 30, 62, 32, 42 | 38, 60, 43, 33, 25, 67, 32, 46 |
| `farnsworth` | 54, 81, 60, 81, 60, 85, 58, 76 | 57, 82, 60, 80, 60, 85, 58, 83 |
| `tight` | 45, 62, 15, 84, 38, 69, 31, 70 | 52, 73, 49, 87, 41, 76, 46, 75 |
| `steady` | 57, 67, 46, 69, 54, 75, 45, 81 | 60, 87, 57, 87, 57, 81, 54, 87 |
| `machine` | 51, 81, 57, 87, 60, 87, 57, 84 | 60, 87, 56, 87, 60, 87, 60, 87 |

**The two real hands' stretches** are in section 2.

**The rounds:**

| round | wild bank | `bug` | spaces added | twelve-plus-three | W1AW | fist bank |
|---|---|---|---|---|---|---|
| shipped | 2859 | 345 | 400 | 308 | 2101 | 3898 |
| one | 2888 | 221 | 278 | 244 | 2098 | 4195 |
| two (quick measure) | 3028 | 278 | not taken | 300 | 2098 | not taken |
| three | 3091 | 344 | 263 | 291 | 2098 | 4268 |

**`bug` with one part of the rule taken out, at round two** (278 with everything in): the gap fit out 340; validity at the start out 294; the wider cold range out 290; the warm lane's invalid re-read out 292; the terminal test out 221.

**The survey's groups, score:**

| group | before, and with the rule off now | rule on |
|---|---|---|
| synthetic fixtures | 42 | 60 |
| receiver fixtures: easy | 90 | 74 |
| receiver fixtures: working | 19 | 5 |
| receiver fixtures: edge | 35 | 21 |
| receiver fixtures: ungraded | 45 | 57 |
| synthetic CQs: 15 dB | 84 | 82 |
| synthetic CQs: 5 dB | 84 | 84 |
| synthetic CQs: 0 dB | 2 | -3 |
| twelve-plus-three | 308 | 291 |
| W1AW pieces | 2101 | 2098 |
| the fist bank | 3898 | 4268 |
| wild bank | 2859 | 3091 |
| wild hands | 119 | 124 |

**CPU.** W1AW piece-04, 283 s: 120.8 s to 132.2 s, +9%. The machine was busier than in the last unit, so compare the percentage rather than the seconds. The survey took 23 m 20 s off and 27 m 0 s on.

## 4. What's blocking us

Nothing blocks the next unit. Left, a line each:

- **Worth testing next, not measured here:** the terminal test judged on a lane's recent letters rather than its whole window, for `221548`'s second station, and without the `E` and `T` share, for `tightfist-easy`.
- **`bug`'s remaining wrong letters** read as his marks, which the order kept out of this unit. The dah slide is still there.
- **`tightfist-easy`, `tightfist-edge` and `fast-working`** print nothing with the rule on. None was traced.
- **The first recording's guard** holds `FF C HAT<BT> BEST 7V 73 <SK> KC4ZGP ■`. If the rule ever ships, that string must be restated to the whole read.
- **A correction to the last report:** its per-hand lists were labelled own speed first. The order is 20 WPM first, as above.
- **The memory notices:** three wrappers stopped this unit, with 1.0 to 4.5 GB free and other programs open. Every run finished.
- **A stray file:** `src\Hamlet.RadioEngine\Cw\CwSequenceReader.cs.new`, 0 bytes, untracked. Not from this unit; left alone and not committed.
- **Not committed, as before:** the earlier changes to `PARKED.md`, `RUN_LEDGER.md`, `PROJECT_STATUS.md` and `.run-unit\`, and the `cases-*.txt` fixtures.
- **The analysis file `ANALYSIS-2026-11-03-wild-cw.md` is still not in this tree.** Its figures were not checked.

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
