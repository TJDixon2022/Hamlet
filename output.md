## 1. What Claude did

Claude Code on the development computer, branch `main`. The prompt named `PROJECT: Hamlet`, and the order's gate and `PROJECT_CARD.md` confirmed it. Nothing in this report is evidence about the radio. Version 1.13.281 to 1.13.282. Work instruction 595, ruling HM-DEC-299. The tag `before-gap-learning` stands. `SESSION.lock` was taken and released. Nothing was written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` was touched, and no box was ticked in `PHASE_PLAN.md`. The draft truth was not edited. Nothing keys or transmits, nothing was written to the radio, and no recording was added, removed or promoted.

**Round one** (`5c943867`): `CwVocabulary`, the word judge at all three places, the four traces, and the full measure. The twelve-plus-three 269, the wild bank 3024, `bug` 339.

**Round two** (`93c58007`): the largest board loss was `221745`, which printed nothing in 30 s where the shipped reader read 27 of 29. A refused lane now takes the terminal only where no unrefused lane has marks to give. The twelve-plus-three 308, the wild bank 3015, `bug` 321.

**Round three** (`c10b64ed`): the largest wild bank loss. On `mobile`, a 40 ms start at 0.09 on one lucky `U` beat a real rhythm at 0.00 whose spaces were off, and took the lane to `TTTTT TETE` for 8 s. Where no cold start is valid, the choice is now the most letters inside words whatever the spaces. The twelve-plus-three 286, the wild bank 2989, `bug` 280. This was worse, and it was the last round the order allows.

**Departures, said plainly:**
- **Round two departs from section 3's sentence** that a refused lane does not take the terminal. Section 4 allows a fix inside the judge, and I took it as covering this.
- **Round three departs from section 3's "the most recognized":** among starts that are not valid, it ranks by words found across the lane's spaces. The rule's verdict still uses the spaces.
- **"Printed letters" is read as the letters of the lane's current decode**, since a lane off the terminal prints nothing. Any lane's window is judged on its own text.
- **A prosign counts as a word of its own**, so `HAT<BT>` is two words. The order did not say.
- **The word judge's first trace ran before rounds two and three.** The traces below are from round one, the judge as ordered. The printed lines were read again at the end and are named where they moved.
- **Round two was measured by the quick measure only**, the boards and wild bank without the survey or the guards.
- **Round three's code stays in the tree** though round two read better on the boards. Both are off.

**Recorded:** HM-DEC-299, the order's own ruling record, in the tree in full.

**Lines:** engine 168 of 168, app 278 of 278, `DecisionLogOrderTests` 2 of 2, `TheCwVocabularyTests` 32 of 32, the survey once with the rule off. The harness reported wrappers stopped for low memory on three runs. Each time the script went on running and finished, so nothing was run twice for it. One socket error aborted the first vocabulary run, and it passed when run again alone.

## 2. What the owner should expect

**It does not ship, and it is not for your ruling.** Words as the judge fixed what they were brought in to fix, `tightfist-easy` and the station who began mid-window, but they cost more than they gave: the twelve-plus-three is 22 under, the wild bank fell below its bar and `bug` fell 60 under. What you run reads exactly as it did: with the rule off, the survey is byte-identical to the last one but for its `Rerun` line.

**The whole picture against the shipped reader:**

| | as shipped | rule on, last unit | rule on, now | bar | |
|---|---|---|---|---|---|
| twelve-plus-three | 308, 13 wrong, 4 invented, 92 of 110 spaces | 291 | **286**, 21 wrong, 10 invented, 93 of 110, 16 added | 308, wrong 13, invented 4 | **22 under, wrong and invented over** |
| W1AW | 2101, 24 wrong | 2098, 26 wrong | 2103, 23 wrong | 2101, wrong 24 | met |
| fist bank | 3898 | 4268 | 4157 | 3,898 | met, its guard passes |
| fading half, right | 1771 | 1993 | 1961 | as guarded | better |
| flat half, right | 2690 | 2720 | 2722 | as guarded | better |
| easy fixtures | 90 of 90 | 74 | **90 of 90** | 90 | met |
| working fixtures, reported | 19 | 5 | 5 | | worse |
| edge fixtures, reported | 35 | 21 | 31 | | worse |
| wild bank score | 2859 | 3091 | **2989** | 3,050 | **61 under** |
| spaces added | 400 | 263 | **273** | 270 | **3 over** |
| invented | 0 | 0 | 0 | 5 | met |
| `mobile` | 487 | 529 | **505** | 519 | **14 under** |
| `bug` | 345 | 344 | **280** | 340 | **60 under, and 64 under last unit** |
| `farnsworth` | 555 | 565 | 572 | 555 | met |
| `tight` | 414 | 499 | 499 | 489 | met |
| `steady` | 494 | 570 | 562 | 560 | met |
| `machine` | 564 | 584 | **571** | 574 | **3 under** |
| first recording | `FF C HAT<BT> BEST 7V 73 <SK> KC4ZGP ■` | whole | `FER C HAT<BT> BEST 7V 73 <SK> KC4ZGP DEWA` | whole | met |
| 5 dB CQs | 84 of 84 | 84 | 84 of 84 | 84 | met |
| `two-station` | 27 of 27 | 27 | 27 of 27 | 27 | met |
| noise, steady carrier | nothing | nothing | nothing | nothing | met |
| highest strays per 10 s | 4.3 | 2.0 | 2.0 | 6 | met |
| summed strays, reported | 46.5 | 29.7 | 31.2 | | better |
| random carrier, reported | 20 seeds of 20 | 6 | **20 seeds of 20** | | the last unit's gain is gone |
| letters in silences | 1 | 1 | 1 | as guarded | met |
| CPU, W1AW piece-04, 283 s | 124.5 s | +9% | 127.4 s, +2% | under real time | met |

The noise run's "off" row was taken with the rule on by default, so it is no comparison. The 20 for as shipped is the last unit's measure.

**Ken, against the draft truth, 43 of 76 both ways:**

| stretch | right, shipped | right, on | prints, shipped | prints, on |
|---|---|---|---|---|
| 0-20 s | 2 | 2 | `U UE N DEALL<BT> W A NST SI LG` | `U UE N DEA L<BT> W A NST SI ZG` |
| 20-40 s | 11 | 11 | `N 58E XTHI I N W N ENSEN` | `N 58E XTHI I N W D ENS EN` |
| 40-60 s | 10 | 11 | `G TOI ,P T ■ NR GITTSNI` | `G TON,P T PA NR GITTSNI` |
| 60-80 s | 13 | 12 | `■GH TU NAME ES KEN KEN UG` | `■GH TU N TME ES KEN KEN UG` |
| 80-100 s | 0 | 0 | `V ■LE UTI2UM WEEENIWRNS` | `V ■LE UTI2UM W EEENIWRNS` |
| 100-120 s | 7 | 7 | `A N3AFS DE KB2UZY / TE` | `ET N3AFS DE KB2UZY / TE` |

**Bryan, against the draft truth, 118 to 123 of 144:**

| stretch | right, shipped | right, on | prints, shipped | prints, on |
|---|---|---|---|---|
| 120-140 s | 16 | 17 | `E T■O GM KEN NICE TO M EE` | `E TELO GM KEN NICE TO M E EE` |
| 140-160 s | 16 | 15 | `T S TNXFER RPRT UR I 5 7` | `T S TNXFER RPRT UR I 5 7` |
| 160-180 s | 8 | 10 | `9 5 7 N LIL T IAM F M O` | `9 5 7 N LIL T I IAM IN MO` |
| 180-200 s | 16 | 16 | `BILE SNN T A MY NAME BRYAN B` | `BILE SNN TET IY NAME BRYAN B` |
| 200-220 s | 15 | 16 | `RYAN ES Q TH E S E CHICOP` | `RYAN ES Q TH IS FHICOP` |
| 220-240 s | 14 | 14 | `E , MA C H I C OP E E MW MA P` | `E , MA CHICOP E E MW MA P` |
| 240-260 s | 18 | 18 | `ARKED IN MY MOBILE <BT> SO TIT` | `ARKED IN MY MOBILE <BT> SO TIT` |
| 260-280 s | 15 | 17 | `H KEN N3AFS DE KB■ZY/ M` | `H KEN N3AFS DE KB2UZY/ M` |
| 280-300 s | 0 | 0 | `II` | `EEE` |

**What the four traces found:**
- **`tightfist-easy`:** the shipped judge called it invalid at 0.50. The word judge gives no verdict, because it was laid without spaces as `VVVTESTDETESTK`, so it prints, 14 of 14. It printed for lack of a verdict, not because the words were recognized.
- **`221548`'s second station:** the old share is mostly invalid and the word share is 0.00 to 0.25 from 22 to 28 s, reaching 0.50 at 30 s, when the terminal goes to them. The backlog then prints 16 of 18, so this loss is gone.
- **`bug`, 20 WPM, 20 dB, 6 to 20 s:** the lanes 50 Hz off read `TETE TNTN AETE`, old share 0.55 to 1.00, words 0.00. They never take the terminal, with the word judge alone or with rounds two and three in.
- **`221828`:** this loss is neither the judge's refusal nor the keep rule's. The keep rule refused no window of the lane's cold read, near-lines 0.02 to 0.17. A warm read laid `AMEN` where the cold read laid `AGN`, and the word judge's re-reads did not put it right.

**What the boards still lose, in one paragraph.** It is the judge, the word share itself. A real hand's read is off by a letter or a space often enough that it scores near nought, and so does a wrong read, so the words cannot part them: `TMN 40M TONITE . EUROREE` and `TTTTT TETE` both score 0.00, and a 40 ms noise read beats the true rhythm at 0.09. Round one showed that as honest text refused, and round three as wrong text chosen. The `E`, `T` and `■` share this order removed was parting them at the cold start, and the random carrier went back to 20 seeds without it. That is a reading of the traces; I did not measure a build with both judges together. Round three's own costs were not traced, there being no round left: `221745` now takes the terminal at 475 Hz at 8 s with 8 invented, and `machine`'s 20 WPM, 20 dB QSO fell from 87 to 70.

**What will look wrong but is not:** `CwVocabulary`, `data\cw\` and the changed rule are in the tree with the rule off. `TheCwVocabularyTests` is not on the carry-forward list, since it ships only if the rule does. The guards still hold the old first-recording read, which is correct while the rule is off.

## 3. What you should see

**The four traces, round one, the judge as ordered.** Full lines, every decode, are in `.run-unit\reports\unit595-r1-traces.txt`, and the state at the end in `unit595-final-traces.txt`.

`tightfist-easy`, the 600 Hz lane; the 625 Hz lane reads the same, and it holds the terminal from 6 s:

| second | old share | near lines | shipped judge | words | read |
|---|---|---|---|---|---|
| 6 | 0.50 | 0.00 | invalid | 0.00 of 0 | ` VVVTESTE` |
| 8 | 0.50 | 0.00 | invalid | 0.00 of 0 | ` VVVTESTDETEI` |
| 10 | 0.50 | 0.00 | invalid | 0.00 of 0 | ` VVVTESTDETESTK` |

`221548`, the second station's 500 Hz lane, the terminal at 30 s:

| second | letters/s | old share | shipped judge | words | read |
|---|---|---|---|---|---|
| 18 | 3.8 | 0.65 | invalid | 0.00 of 8 | noise |
| 22 | 0.7 | 0.50 | invalid | 0.00 of 6 | ` EYRHEE MT` |
| 24 | 0.8 | 0.30 | valid | 0.00 of 6 | ` EYRHEE MYSK` |
| 26 | 1.2 | 0.43 | invalid | 0.20 of 10 | ` EYR HEE MY SCOET T` |
| 28 | 1.5 | 0.39 | invalid | 0.25 of 8 | ` EYR HEE MY SCOET T MAST` |
| 30 | 1.6 | 0.37 | invalid | 0.50 of 12 | ` YR HEE MY SCOET T MASTER ` |

`bug`, 20 WPM, 20 dB, bank text, the terminal on 625 Hz at 6 s and on 600 Hz at 8 s, the lane 50 Hz under:

| second | old share | words | read |
|---|---|---|---|
| 6 | 0.68 | 0.00 of 8 | ` TETE TNTN AETE TNEA ATE` |
| 10 | 0.70 | 0.00 of 9 | ` TETE TNTN AETE TNEN ATE T TE NNTNT AETE ET` |
| 14 | 0.58 | 0.00 of 6 | `TNTM NETT AMEM ATE T TE NMTNT MEAE EA TMTE ETTT N E ANNE` |
| 20 | 1.00 | 0.00 of 10 | `TT TETE ET ETTE ETEE T E TTTTT TETE ET ETEE ETEE TET TTE ` |

`221828`, the 600 Hz lane's cold read at each window:

| window ends, s | fit centres, ms | near lines | the fit | read |
|---|---|---|---|---|
| 6 | 44 / 186 / 317 | 0.17 | kept | ` T FER ANOTH EE` |
| 12 | 45 / 188 / 334 | 0.13 | kept | ` T FER ANOTH ER FB QSO ES ` |
| 18 | 44 / 222 / 453 | 0.10 | kept | `N FB QSOES HOPE U HAS` |
| 22 | 44 / 217 / 498 | 0.07 | kept | ` OES HOPE U HAVE AGN ED` |
| 26 | 47 / 226 / 506 | 0.07 | kept | `RE U HAVE AGN ED ESBEST ` |
| 30 | 47 / 210 / 490 | 0.02 | kept | ` TE AGN ED ESBESTE SV <AR>W■` |

The live lane printed `T FER ANOTH ER FB QSO ES HOP E U HAVE AMEN ED ESBESTE SV CW■`, its warm read at 22 s laying `AMEN`.

**The wild bank, right / score / spaces added:**

| recipe | as shipped | last unit | rule on, now |
|---|---|---|---|
| `mobile` | 522 / 487 / 110 | 550 / 529 / 72 | 537 / 505 / 75 |
| `bug` | 459 / 345 / 47 | 450 / 344 / 43 | 425 / 280 / 47 |
| `farnsworth` | 567 / 555 / 175 | 573 / 565 / 85 | 579 / 572 / 88 |
| `tight` | 489 / 414 / 31 | 542 / 499 / 29 | 542 / 499 / 33 |
| `steady` | 524 / 494 / 34 | 576 / 570 / 22 | 571 / 562 / 19 |
| `machine` | 572 / 564 / 3 | 586 / 584 / 12 | 571 / 571 / 11 |
| total | 3133 / 2859 / 400 | 3277 / 3091 / 263 | 3225 / 2989 / 273 |

**Every hand's score**, in the order 20 WPM 10 dB bank and QSO, 20 WPM 20 dB bank and QSO, own 10 dB bank and QSO, own 20 dB bank and QSO:

| recipe | as shipped | rule on, now |
|---|---|---|
| `mobile` | 46, 75, 51, 63, 55, 74, 54, 69 | 52, 81, 54, 76, 34, 74, 57, 77 |
| `bug` | 40, 57, 34, 48, 30, 62, 32, 42 | 38, 30, 40, 50, 23, 32, 18, 49 |
| `farnsworth` | 54, 81, 60, 81, 60, 85, 58, 76 | 57, 85, 60, 82, 60, 85, 58, 85 |
| `tight` | 45, 62, 15, 84, 38, 69, 31, 70 | 50, 73, 47, 83, 41, 76, 50, 79 |
| `steady` | 57, 67, 46, 69, 54, 75, 45, 81 | 60, 87, 57, 79, 57, 81, 54, 87 |
| `machine` | 51, 81, 57, 87, 60, 87, 57, 84 | 60, 87, 60, 70, 60, 87, 60, 87 |

**The two real hands' stretches** are in section 2.

**The rounds:**

| round | wild bank | `bug` | spaces added | twelve-plus-three | wrong | invented | W1AW | fist bank |
|---|---|---|---|---|---|---|---|---|
| shipped | 2859 | 345 | 400 | 308 | 13 | 4 | 2101 | 3898 |
| one | 3024 | 339 | 251 | 269 | 15 | 1 | 2103 | 4256 |
| two (quick measure) | 3015 | 321 | not taken | 308 | 15 | 2 | 2103 | not taken |
| three | 2989 | 280 | 273 | 286 | 21 | 10 | 2103 | 4157 |

**The survey's groups, score:**

| group | before, and with the rule off now | rule on |
|---|---|---|
| synthetic fixtures | 42 | 64 |
| receiver fixtures: easy | 90 | 90 |
| receiver fixtures: working | 19 | 5 |
| receiver fixtures: edge | 35 | 31 |
| receiver fixtures: ungraded | 45 | 73 |
| synthetic CQs: 15 dB | 84 | 84 |
| synthetic CQs: 5 dB | 84 | 84 |
| synthetic CQs: 0 dB | 2 | -3 |
| twelve-plus-three | 308 | 286 |
| W1AW pieces | 2101 | 2103 |
| the fist bank | 3898 | 4157 |
| wild bank | 2859 | 2989 |
| wild hands | 119 | 126 |

**CPU and the vocabulary.** W1AW piece-04, 283 s: 124.5 s to 127.4 s, +2%. The vocabulary's own share of that was not measured apart. The list holds 3,045 English words and 593 first names, 3,573 distinct between them, plus the listed codes, abbreviations and states. The survey took 22 m 41 s off and about 26 m on.

## 4. What's blocking us

Nothing blocks the next unit. Left, a line each:

- **Worth testing next, not measured here:** the word share and the `E`, `T` and `■` share together, since each failed where the other held.
- **Round two against round three:** round two is the better form on the boards, 308 with 15 wrong. Its wild bank was 3015 and `bug` 321. Taking round three out is one revert of `c10b64ed`.
- **The vocabulary is short of words the traces named:** `TONITE` and `40M` among them. None was added, because round three's trace did not name them.
- **State codes make short noise read as words:** `MT`, `ME`, `IN`. On `bug`, a 40 ms start read 0.50 on such words. The order lists them, so they stand.
- **Not traced, no round left:** `221745`'s 475 Hz lane and `machine`'s 20 WPM, 20 dB QSO.
- **The memory notices:** three wrappers were reported stopped, with about 3 GB free. Each kept running to the end.
- **A stray file:** `src\Hamlet.RadioEngine\Cw\CwSequenceReader.cs.new`, 0 bytes, untracked. Not from this unit; left alone and not committed.
- **Not committed, as before:** the earlier changes to `PARKED.md`, `RUN_LEDGER.md`, `PROJECT_STATUS.md` and `.run-unit\`, and the `cases-*.txt` fixtures.

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
