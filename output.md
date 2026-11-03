## 1. What Claude did

Claude Code on the development computer, branch `main`. The prompt named `PROJECT: Hamlet`, and the order's gate and `PROJECT_CARD.md` confirmed it. Nothing in this report is evidence about the radio. Version 1.13.279 to 1.13.280. Work instruction 593, ruling HM-DEC-297. Tag `before-gap-learning` at `460831e1`. `SESSION.lock` was taken and released. Nothing was written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` was touched, and no box was ticked in `PHASE_PLAN.md`. The draft truth was not edited. Nothing keys or transmits, nothing was written to the radio, and no recording was added, removed or promoted.

**Round one** (`29ae7471`): the trace, then the wider cold ranges, the start chosen by validity, the ordered fit, and a warm lane that goes invalid read cold. **Section 3.2's line starts were left out, because they raised letters right on none of the twelve windows.** The wild bank 2846, `bug` 166.

**Round two** (`2431cab9`): on `bug`'s `5 7 9 5 7 9`, a window nearly all inside gaps, the fit parted his inside gaps in two and took his letter gaps for word gaps, the letter centre going 331, 256, 231, 168 ms. A fit's letter centre must now stand 2.0 over its inside centre (`FitLetterStep`). The wild bank 2911, `bug` 226.

**Round three** (`4108eb22`): word gaps laid as nobody keying were left out of the fit, so `farnsworth`, sending 397 and 1575 ms, learned 190 and 369. A stretch of nobody keying up to 30 dits, not cut by the window's edge, is now fitted as a word gap (`FitWordGapMost`). The wild bank 2944, spaces added 267, `bug` 254.

**Three of my own slips, all corrected:** the first survey's saved document was overwritten by a hand copy of the restored file, and I recovered it whole from the run log. One new test, `TheFitFindsAHandsClasses`, failed on a poor start of mine; I corrected the start and it passes, but only after the engine line had run, and that test is not on the line. I also said the final chain would stop after a memory notice, and it did not.

**Recorded:** HM-DEC-297, which is the order's own ruling record and in the tree in full.

**Lines:** engine 168 of 168, app 278 of 278, the survey once with the rule off. The harness reported memory stops on the first survey and the final chain; both kept running to the end, and nothing was run twice.

## 2. What the owner should expect

**It does not ship.** The change stays in the tree behind `CwRules.GapLearning`, off, so what you run reads exactly as it did. With it off, the survey is byte-identical to the last one but for its `Rerun` line. It missed the bar and is not in the band for your ruling: the wild bank ended at 2944, under 3,000.

**The whole picture against the shipped reader:**

| | as shipped | the change on | bar | |
|---|---|---|---|---|
| wild bank score | 2859 | **2944** | 3,150 | missed by 206 |
| spaces added | 400 | 267 | 250 | missed by 17 |
| invented | 0 | 0 | 5 or fewer | met |
| `mobile` | 487, 110 added | 486, 73 added | 50 added or fewer | missed by 23 |
| `bug` | 345, 459 right | **254, 410 right** | 510 | missed, and 91 worse |
| `farnsworth` | 555, 175 added | 538, 85 added | within 10 of shipped | 17 worse |
| `tight` | 414 | 505 | within 10 of shipped | met |
| `steady` | 494 | 573 | within 10 of shipped | met |
| `machine` | 564 | 588, every letter | within 10 of shipped | met |
| twelve-plus-three | 308, 13 wrong, 4 invented | 313, 15 wrong, 0 invented | 308, wrong not rising | wrong rose by 2 |
| W1AW | 2101, 24 wrong | 2098, 26 wrong | 2101 | 3 under, wrong rose by 2 |
| fist bank | 3898 | 4634 | 3,898 | met |
| fading half, right | 1771 | 2088 | as guarded | better |
| flat half, right | 2690 | 2820 | as guarded | better |
| first recording | `FF C HAT<BT> BEST 7V 73 <SK> KC4ZGP ■` | `FER C HAT<BT> BEST 7V 73 <SK> KC4ZGP DEWA`, whole | whole | better; the guard is pinned to the old read |
| 5 dB CQs | 84 of 84 | 84 of 84 | 84 | met |
| easy fixtures | 90 of 90 | 90 of 90 | 90 | met |
| `two-station` | 27 of 27 | 27 of 27 | 27 | met |
| noise, steady carrier | nothing | nothing | nothing | met |
| highest strays per 10 s | under 6 | 5.3 | 6 | met |
| letters in silences | 1 | 1 | as guarded | met |
| CPU, W1AW piece-04, 283 s | 107.3 s | 123.4 s, +15% | under real time | met |

The random carrier printed at 20 seeds of 20 with the change on. I did not measure it this unit with the change off, so I can't give you a before figure. Earlier sequence-reader rounds recorded the same 20. The working fixtures fell from 19 to 5, and those are reported, not guarded.

**Bryan, against the draft truth, 118 to 123 of 144:**

| stretch | right, shipped | right, on | prints now, shipped | prints with the change on |
|---|---|---|---|---|
| 120-140 s | 16 | 17 | `E T■O GM KEN NICE TO M EE` | `D EELO GM KEN NICE TO ME ES` |
| 140-160 s | 16 | 15 | `T S TNXFER RPRT UR I 5 7` | `T S TNXFER RPRT UR I 5 7` |
| 160-180 s | 8 | 10 | `9 5 7 N LIL T IAM F M O` | `9 5 7 N LIL T IAM IN MO` |
| 180-200 s | 16 | 16 | `BILE SNN T A MY NAME BRYAN B` | `BILE SNN TEA MIW NAME BRYAN B` |
| 200-220 s | 15 | 16 | `RYAN ES Q TH E S E CHICOP` | `RYAN ES Q TH IS I CHICOP` |
| 220-240 s | 14 | 14 | `E , MA C H I C OP E E MW MA P` | `E , MA CHICOP E E MW MA P` |
| 240-260 s | 18 | 18 | `ARKED IN MY MOBILE <BT> SO TIT` | `ARKED IN MY MOBILE <BT> SO TIT` |
| 260-280 s | 15 | 17 | `H KEN N3AFS DE KB■ZY/ M` | `H KEN N3AFS DE KB2UZY/ M` |

**Ken, against the draft truth, 43 to 37 of 76. The change reads him worse:**

| stretch | right, shipped | right, on | prints now, shipped | prints with the change on |
|---|---|---|---|---|
| 0-20 s | 2 | 2 | `U UE N DEALL<BT> W A NST SI LG` | `U UE N DEALL<BT> W M MST SI LG` |
| 20-40 s | 11 | 11 | `N 58E XTHI I N W N ENSEN` | `N 58T QTVI I N W D ENS EN` |
| 40-60 s | 10 | 9 | `G TOI ,P T ■ NR GITTSNI` | `G TOI ■ P T PA NR GITTSNI` |
| 60-80 s | 13 | 8 | `■GH TU NAME ES KEN KEN UG` | `■BJ TU N TME AU KEM KEN UG` |
| 80-100 s | 0 | 0 | `V ■LE UTI2UM WEEENIWRNS` | `V ■LT KTI2UME ■ TEENI M A NS` |
| 100-120 s | 7 | 7 | `A N3AFS DE KB2UZY / TE` | `EETN3AFS DE KB2UZY / D` |

**What the trace found, a sentence per hand:**

- **Ken:** partly the cold range. At 34 to 46 s the likeliest start as built laid 79% `E`, `T` and the placeholder against 25% with the wider range, yet his live read still fell, and I did not trace why.
- **Bryan:** the learning. His letters improve a little, and most of his remaining losses are not gap-line faults.
- **`mobile`:** the learning. The fit read 36 of 37 letters across three windows where the medians read 28 of 33, but his digits and short words are sent spaced like words and run together, and no line can part those.
- **`bug`:** at first not the gaps at all, since every start read alike. The fit then broke him twice, and what he still loses is in his marks.

**What the wild bank still loses is the learning, and now in the marks.** Nearly all the shortfall is `bug`: with a dit spread of 0.37, his dah centre falls from 250 to 165 ms between 40 and 66 s and his dah share climbs to 0.6, so long dits read as dahs. The gap fit is what moved five recipes the right way. The mark fit went in beside it and may be what hurts him, but that is a reading of the trace, not a measurement, and no round was left to test it.

**What will look wrong but is not:** `CwRules.GapLearning` and `TheGapLearningTests` are in the tree with the rule off. The guards still hold the old first-recording read, which is correct while the change is off.

## 3. What you should see

**The trace, section 2.** Three 12 s windows a hand, one lane. Before is the start as built, read by medians; after is the start validity takes, read by the fit. Each cell gives the start, its share of `E`, `T` and `■`, its share of gaps near a boundary, then after three passes: letters matching the truth of letters printed, spaces added inside words, the learned inside / letter / word gaps in ms, and the two lines. The full tables, with every start, are in `.run-unit\reports\unit593-trace-r0.txt` and `unit593-on-measures.txt`.

| hand, window | before | after |
|---|---|---|
| Ken, sent 136 / 344 / 608, 20-32 s | 120 ms, 20%, 7%: 5 of 9, 0+, 135/413/700 (275, 555) | 120 ms, 20%, 7%: 5 of 9, 0+, 133/437/979 (240, 655) |
| 34-46 s | 70 ms, 79%, 43%: 6 of 22, 2+, 125/163/360 (145, 260) | 120 ms, 25%, 3%: 8 of 12, 3+, 141/348/559 (220, 440) |
| 48-60 s | 120 ms, 23%, 13%: 6 of 12, 0+, 130/330/543 (230, 435) | 120 ms, 23%, 13%: 8 of 13, 0+, 129/320/698 (205, 470) |
| Bryan, measured 104 / 286 / 891, 140-152 s | 70 ms, 29%, 11%: 12 of 12, 1+, 105/235/805 (170, 520) | 70 ms, 23%, 11%: 11 of 12, 1+, 105/243/904 (160, 470) |
| 154-166 s | 70 ms, 20%, 49%: 6 of 8, 3+, 110/180/565 (145, 370) | 55 ms, 17%, 11%: 6 of 9, 3+, 104/180/679 (140, 360) |
| 168-180 s | 70 ms, 21%, 23%: 7 of 10, 1+, 105/210/855 (160, 530) | 70 ms, 15%, 14%: 7 of 10, 0+, 106/314/915 (185, 535) |
| `bug`, sent 136 / 344 / 608, 16-28 s | 100 ms, 25%, 27%: 11 of 12, 0+, 140/358/560 (250, 460) | 100 ms, 31%, 23%: 11 of 12, 2+, 138/329/466 (215, 390) |
| 40-52 s | 110 ms, 8%, 43%: 10 of 12, 0+, 135/330/665 (230, 500) | 120 ms, 8%, 23%: 10 of 14, 0+, 129/290/513 (195, 385) |
| 64-76 s | 120 ms, 18%, 11%: 8 of 12, 0+, 135/423/805 (280, 615) | 120 ms, 18%, 7%: 7 of 11, 1+, 137/376/595 (225, 475) |
| `mobile`, sent 104 / 286 / 891, 13-25 s | 90 ms, 29%, 15%: 12 of 13, 3+, 105/245/400 (175, 320) | 90 ms, 29%, 14%: 14 of 14, 3+, 105/241/494 (160, 345) |
| 34-46 s | 70 ms, 0%, 9%: 9 of 10, 3+, 105/253/703 (180, 480) | 70 ms, 0%, 6%: 11 of 11, 2+, 108/230/666 (155, 390) |
| 55-67 s | 70 ms, 15%, 3%: 7 of 10, 0+, 100/300/1040 (200, 670) | 70 ms, 15%, 6%: 11 of 12, 1+, 102/269/832 (165, 475) |

**The line starts' test.** Best letters right on any start of a kind, across the twelve windows: the wider range and the line starts tie on all twelve. On spaces they differ twice on those best rows: Ken at 48-60 s, where the line starts add 4 against none, and Bryan at 168-180 s, where they add 1 against 2 on a read of mostly `E` and `T`.

**The wild bank by recipe, right / score / spaces added:**

| recipe | as shipped | the change on |
|---|---|---|
| `mobile` | 522 / 487 / 110 | 528 / 486 / 73 |
| `bug` | 459 / 345 / 47 | 410 / 254 / 53 |
| `farnsworth` | 567 / 555 / 175 | 562 / 538 / 85 |
| `tight` | 489 / 414 / 31 | 541 / 505 / 25 |
| `steady` | 524 / 494 / 34 | 578 / 573 / 21 |
| `machine` | 572 / 564 / 3 | 588 / 588 / 10 |
| total | 3133 / 2859 / 400 | 3207 / 2944 / 267 |

Every hand's score with the change on, in the order own 10 dB bank and QSO, own 20 dB bank and QSO, 20 WPM 10 dB bank and QSO, 20 WPM 20 dB bank and QSO:

- `mobile`: 50, 77, 46, 72, 40, 70, 57, 74
- `bug`: 28, 37, 17, 39, 28, 65, 8, 32
- `farnsworth`: 51, 76, 60, 83, 56, 85, 56, 71
- `tight`: 50, 79, 53, 73, 55, 68, 58, 69
- `steady`: 60, 87, 57, 87, 60, 81, 54, 87
- `machine`: 60, 87, 60, 87, 60, 87, 60, 87

**The rounds:**

| round | wild bank | `bug` | spaces added | twelve-plus-three | W1AW | fist bank |
|---|---|---|---|---|---|---|
| shipped | 2859 | 345 | 400 | 308 | 2101 | 3898 |
| one | 2846 | 166 | 398 | 314 | 2098 | 4629 |
| two (quick measure) | 2911 | 226 | not taken | 314 | 2098 | not taken |
| three | 2944 | 254 | 267 | 313 | 2098 | 4634 |

**The survey's groups, score:**

| group | as shipped, and with the rule off now | the change on |
|---|---|---|
| synthetic fixtures | 42 | 67 |
| receiver fixtures: easy | 90 | 90 |
| receiver fixtures: working | 19 | 5 |
| receiver fixtures: edge | 35 | 31 |
| receiver fixtures: ungraded | 45 | 75 |
| synthetic CQs: 15 dB | 84 | 84 |
| synthetic CQs: 5 dB | 84 | 84 |
| synthetic CQs: 0 dB | 2 | -1 |
| twelve-plus-three | 308 | 313 |
| W1AW pieces | 2101 | 2098 |
| the fist bank | 3898 | 4634 |
| wild bank | 2859 | 2944 |
| wild hands | 119 | 113 |

**Cost.** W1AW piece-04, 283 s: 107.3 s to 123.4 s of CPU, +15%, under the order's 20%, so no gate was needed. The cold starts cost nothing extra because the line starts were left out; the 15% is the wider word range and the fit. The survey took 22 m 25 s with the rule off and 26 m 31 s on.

## 4. What's blocking us

Nothing blocks the next unit. Left, a line each:

- **For your eye, not a ruling the order asks for:** the change is a large gain on five recipes, on the fist bank and on the first recording, and its costs are concentrated in `bug`, W1AW's 3 letters and Ken. The change is in the tree if you want it looked at again.
- **Worth testing next:** the gap fit alone with the marks left to medians, since `bug`'s remaining loss is in his marks. Not measured here.
- **`bug` loses his opening too:** the 650 Hz lane held from 8 to 14 s and the printed text begins at about 20 s. This is the opening you ruled out of further units as a mark problem, so it was left.
- **Ken reads worse on, and I did not trace why.** He has a draft truth and is not guarded.
- **The first recording's guard** holds `FF C HAT<BT> BEST 7V 73 <SK> KC4ZGP ■`. If the change ever ships, that string must be restated to the whole read.
- **The random carrier** printed at 20 of 20 seeds with the change on. Reported, not a condition, and not measured with it off this unit.
- **`TheGapLearningTests`** is not on `docs\carry-forward-tests.txt`, as the rule is off. Its three plain tests pass, and its traces carry the `Survey` category.
- **The memory notices:** the harness reported memory stops twice with 1.6 to 2.5 GB free. Both runs finished, and nothing was run twice. The pressure came from elsewhere on the machine.
- **A stray file:** `src\Hamlet.RadioEngine\Cw\CwSequenceReader.cs.new`, 0 bytes, untracked. Not from this unit; left alone and not committed.
- **Not committed, as before:** the earlier changes to `PARKED.md`, `RUN_LEDGER.md` and `.run-unit\`.
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
