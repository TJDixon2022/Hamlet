## 1. What Claude did

**Gate.** The work was done on the development computer under the prompt's `PROJECT: Hamlet`. The tree confirmed it: `PROJECT_CARD.md` says `PROJECT: Hamlet`, `Hamlet.sln` is at the root, `SHACK_FACTS.md`, `docs\cw-scoreboard.md` and `src\Hamlet.RadioEngine\Cw\CwLaneBank.cs` exist, and neither `CoreHMI.sln` nor `MURC.sln` does. Nothing in this report is evidence about the radio.

**The hand run.**
- I took `SESSION.lock` through `tools\arbiter\lock.bat take` and released it at the end.
- Nothing was written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` was touched, and nothing was ticked in `PHASE_PLAN.md`.
- Nothing keyed or transmitted, and nothing was written to the radio.
- No recording was added, removed or promoted.
- Everything is on branch `main`. The tag `before-one-finder` stands as unit 584 left it.

**Measuring.** Each round's rules were measured by taking `BankFinds` out of `CwRules.OffByDefault` for that round's build, then putting it back before the commit. One test run at a time: the survey, then the guards, then traces.

**Round one: loudest first, and the grid's tests on lane marks** (`574dfbb8`).
- `CwRules.LoudestFirst`: before a lane is held, it is judged for shadow against every lane standing or forming at 0.3 (`LoudestFirstShape`), held or not, by `CwShadow.IsLaneShadowOf`. New stations are held loudest first, by level.
- `CwRules.LaneMeetsTheGrid`: a mark a lane calls while it holds nothing is judged by the tests a grid mark meets in `CallMarks` before the pattern gate (`GridTestsOf`). They are read on the grid bin at the mark's pitch, by reference to the same methods: `HasEdges` at half the mark's own height, `IsNarrow`, and the mark shape at `ShapeThreshold`. `IsToneHere` is not one of the grid's tests (the sender's window applies it), so it is reported and gates nothing. In round one, a failing mark was not offered to the lane's gate.
- **The carrier** (`TheCarrierSeedsAndTheGridsTests`, one finder):
  - with the tests off, it printed at 5195, 5196, 5204, 5208 and 5210; with all three, at none;
  - shape alone stops every seed;
  - 5201 printed only with the tone test alone;
  - 5208 is stopped by edges, by shape and by tone, and not by narrowness.
- **`tightfist-easy`** (`TheTightFistTraced`):
  - two finders read `I VVTESTDETEST K`;
  - one finder as it stood read nothing, its hold refitted four times between 600 and 625 Hz;
  - round one read `A■TESTDENSTK`;
  - the `VVV` is lost because the hold opens on the 625 Hz, 40 ms lane at 2.42 s and refits to the 100 ms lane at 2.84 s, after the lane at the fist's own dit had stood. That is not a one-line cause, so it is reported, not fixed.
- **Measured:** the twelve-plus-three 240, W1AW 2100. The full figures are in section 3.
- **Traced** (`TheLossTracedByRule`, `TheLanesProbed`). The largest loss was `cw-2026-10-03-221530` (31 to 19), then `221548` (11 to 0).
  - On both, loudest first changed nothing.
  - On `221530`, a noise lane at 800 Hz on the 40 ms dit was held at 8.73 s. It was calling marks at 10 to 13 dB of contrast, its level −31 dB against the station's −20, and was not in step with the station.
  - With its failing marks dropped, the rest of its sequence stood at shape 0.61. It printed `TENTESIS`.
  - The station at 598 Hz began near 9 s, and its own 600 Hz lane called no mark until 10.2 s, when its contrast first cleared the lanes' 12 dB floor.
  - On `221548`, a noise lane at 450 Hz was held from 11.57 s and refitted onto the station at 500 Hz at 19.4 s, taking its opening.

**Round two: the tests judge the sequence, not the marks** (`2b60d8c8`).
- No lane mark is dropped. Each carries its verdict (`CwMark.MeetsGrid`).
- A lane is held only where two thirds (`LaneGridShare`, the shadow rule's share) of its last ten marks met the tests.
- **Measured:** the twelve-plus-three 254, W1AW 2098. The largest loss was still `221530`, at 15.

**Round three: a station is heard in two lanes** (`05272c34`).
- The trace showed the 800 Hz lane standing alone at its pitch, while a real station forms on several lanes beside its pitch and on more than one dit.
- `CwRules.TwoLanes`: a lane is held only where another lane within 50 Hz forms at 0.3.
- **Measured:** the twelve-plus-three 253, W1AW 2098, `221530` unchanged at 15.

**It does not ship.** It failed section 4 in every round, outside the owner's-ruling band (294 and 2090). By section 4, this is the last unit on one finder for now. `BankFinds` is back in `CwRules.OffByDefault`, and the three new rules take effect only under it. As shipped, every reading is as at HEAD: 297 and 2111.

**The record**, in one commit after the rounds:
- `DECISIONS.md`: HM-DEC-289, *One finder, loudest first*, and the `CLAUDE.md` §1 index row;
- `docs\cw-scoreboard.md`: a 585 row in each table, 297 and 2111;
- `PHASE_OUTCOME.md`, both copies: `## UNIT 585 - STEP 12`;
- `PHASE_STATUS.md`, both copies: names 585;
- `Directory.Build.props`: 1.13.270 to 1.13.271;
- the survey's `Rerun` line, and `docs\cw-survey-2026-10-09.md` regenerated.

Nothing was recorded under §12.1.

**The lines** each ran once after one build, with the build servers shut down, using `--no-build`:
- the app line: 278 of 278;
- the engine line: 159 of 159 in 12.5 minutes;
- the survey line: 1 of 1 in 10.9 minutes. `docs\cw-survey-2026-10-09.md` is identical to the shipped reading apart from its `Rerun` line.

## 2. What the owner should expect

**One finder against two, as it stands after this unit.** With one finder on, round three's figures:
- **W1AW:** 2098 against 2111, with 8 invented letters where two finders invent none. Most fall in the third piece.
- **The hand-sent recordings** (the twelve-plus-three): 253 against 297. Each station loses its opening: `FER` reads `ER` on the first recording, `N TEMP` reads `E MP`, `RR` reads `EENR`. On `221530` the second station loses its first words to a noise lane, 31 against 15.
- **A short-dah fist:** much better. The dah-1.6 rows read 832 of 1440 against 569, and dah 2.0 886 against 624.
- **A weak CQ:** the four 5 dB CQs read 80 of 84 against 71. The 0 dB CQs read nothing, as with two finders.
- **A fading station:** the fist bank's fading half reads 1166 letters right against 439.
- **The far side** (the working fixtures): 17 of 81 against 4, though 31 wrong.
- **The random carrier** prints at no seed, against two with two finders.

**It does not ship.** `BankFinds` stays off. Section 4 says this ends the one-finder line for now, so the evidence for going back to two finders is in section 4.

**What you run is unchanged:** 297, 2111, the carrier at two seeds, and the same `Weak stations` row. Version 1.13.271.

**The rules, one sentence each.** All are under `BankFinds` only.
- **Loudest first:** a lane in step with a louder lane, standing or forming, is a shadow and is not held, and stations are held loudest first.
- **The grid's tests on lanes:** a lane is held only where two thirds of its last ten marks rise and fall like a key, stand narrow, and score as a mark on the grid's bin.
- **Two lanes:** a lane is held only where a second lane within 50 Hz hears it too.

**What looks wrong but is not:**
- The new tests in `TheOneFinderTests` assert nothing. They are traces.
- Rounds two and three made the chain much slower with one finder on: the survey took 21 minutes against round one's 8, and the W1AW guard 13 minutes against 3. With `BankFinds` off, nothing runs that cost.

## 3. What you should see

**The rounds, every condition.** "As shipped" is two finders at HEAD; rounds are with `BankFinds` on.

| condition | the bar | as shipped | 584 r3 | round one | round two | round three |
|---|---|---|---|---|---|---|
| twelve-plus-three score | 297 | 297 | 239 | 240 | 254 | 253 |
| twelve-plus-three right, wrong, invented | not rising | 318, 20, 1 | 279, 33, 7 | 285, 25, 20 | 293, 32, 7 | 293, 32, 8 |
| twelve-plus-three spaces | not worse | 86 of 110, 3 added | 82, 14 added | 79, 13 added | 85, 14 added | 85, 14 added |
| W1AW score | 2111 | 2111 | 2111 | 2100 | 2098 | 2098 |
| W1AW wrong, invented | not rising | 20, 0 | 10, 3 | 15, 8 | 16, 8 | 16, 8 |
| fist bank score | 2,500 | 1891 | 2853 | 2884 | 2935 | 2915 |
| fist bank invented | 50 or fewer | 46 | 7 | 2 | 20 | 20 |
| fading half, right | 1,000 | 439 | 1111 | 1226 | 1173 | 1166 |
| flat half, right | 2,129 | 2173 | 2661 | 2628 | 2683 | 2678 |
| the four 5 dB CQs | 71 of 84 | 71 | 80 | 76 | 80 | 80 |
| letters in the 20 s after them | none | none | none | 3 (`I E E`, 18 WPM) | none | none |
| short-dah rows 1.6 and 2.0, guard | over the floors | 569, 624 | held | 803, 931 | 837, 893 | 832, 886 |
| straight-key hands | 48 of 60 each | held | held | held | held | held |
| loud noise | nothing | nothing | nothing | nothing | nothing | nothing |
| random carrier | two seeds | 2 | 4 | 0 | 0 | 0 |
| steady carrier | none | none | none | not read | not read | none |
| first recording whole | `FER C HAT…` | whole | `ER C H AT…` | `ER C H AT…` | `ER C H AT…` | `ER C H AT…` |
| letters in silences | none | 0 | 3 | 1 | 3 | 3 |
| strays, highest capture | 6 per 10 s | 3.3 | 2.3 | 1.7 | 1.7 | 1.7 |
| strays, summed | reported | 26.8 | 23.7 | 15.2 | 14.5 | 12.8 |
| easy fixtures, right | 77 of 90 | 77 | 71 | 81 | 71 | 71 |
| working fixtures, right | reported | 4 | 20 | 11 | 17 | 17 |
| `two-station`, right | reported | 18 of 27 | 24 | 22 | 24 | 24 |
| 0 dB CQs | reported | 0 | 32 | 19 | 0 | 0 |

In rounds one and two the board guard failed on its score before it checked loud noise and the steady carrier, so the steady carrier was read only in round three, from the board's own run. That run also read loud noise as nothing.

**The fist bank's three matrices**, as shipped and with one finder at round three.

*Dah over dit by speed, flat, 20 dB:*

| | 10 WPM | 15 WPM | 20 WPM | 25 WPM | row |
|---|---|---|---|---|---|
| dah 1.6 | 60 / 57 | 60 / 58 | 60 / 47 | 60 / 48 | 240 / 210 |
| dah 2.0 | 59 / 59 | 60 / 55 | 60 / 58 | 22 / 58 | 201 / 230 |
| dah 2.5 | 59 / 58 | 59 / 58 | 60 / 59 | 60 / 59 | 238 / 234 |
| dah 3.0 | 60 / 59 | 60 / 59 | 60 / 59 | 60 / 59 | 240 / 236 |

*Dah over dit by signal, flat, 20 WPM:*

| | 5 dB | 10 dB | 20 dB | row |
|---|---|---|---|---|
| dah 1.6 | 45 / 45 | 36 / 56 | 60 / 47 | 141 / 148 |
| dah 2.0 | 43 / 59 | 59 / 59 | 60 / 58 | 162 / 176 |
| dah 2.5 | 53 / 59 | 59 / 59 | 60 / 59 | 172 / 177 |
| dah 3.0 | 16 / 59 | 59 / 59 | 60 / 59 | 135 / 177 |

*Signal by fade, dah 2.5, 20 WPM:*

| | flat | 12 dB of QSB at 0.7 Hz | row |
|---|---|---|---|
| 5 dB | 53 / 59 | 0 / 22 | 53 / 81 |
| 10 dB | 59 / 59 | 14 / 31 | 73 / 90 |
| 20 dB | 60 / 59 | 13 / 58 | 73 / 117 |

**The bank's totals:**
- as shipped: 2612 of 5760, score 1891;
- round one: 3854, score 2884;
- round two: 3856, score 2935;
- round three: 3844, score 2915, with 909 wrong and 20 invented.

**The easy and working fixtures**, right of sent, as shipped then rounds one to three:

| fixture | as shipped | round one | round two | round three |
|---|---|---|---|---|
| `coverage-easy` | 24 of 26 | 25 | 24 | 24 |
| `exchange-easy` | 22 of 22 | 21 | 21 | 21 |
| `fast-easy` | 18 of 19 | 18 | 18 | 18 |
| `prosigns-easy` | 0 of 9 | 8 | 8 | 8 |
| `tightfist-easy` | 13 of 14 | 9 | 0 | 0 |
| `coverage-working` | 1 of 23 | 2 | 4 | 4 |
| `exchange-working` | 0 of 19 | 4 | 4 | 4 |
| `fast-working` | 3 of 19 | 4 | 5 | 5 |
| `prosigns-working` | 0 of 9 | 1 | 0 | 0 |
| `tightfist-working` | 0 of 11 | 0 | 4 | 4 |

**The random carrier at twenty seeds**, one finder, in round one's trace. A dash means it printed nothing. "None" is the grid's tests off, with loudest first on.

| seed | none | all three | edges alone | narrowness alone | shape alone | tone alone |
|---|---|---|---|---|---|---|
| 5193 | - | - | - | `ETEM` | - | - |
| 5195 | `IDSEEINEEEIIESEIEEES EE ETU` | - | `WT M M A WMTN` | as none | - | `TEMETOTTNTAIT TAN` |
| 5196 | `AE TBS` | - | - | `TUE II` | - | `EE AEETNEST T IEET A` |
| 5201 | - | - | - | - | - | `EEAA E` |
| 5204 | `E TITT T TM NTT ETKIE IEEN TE E A` | - | - | - | - | - |
| 5208 | `SIEEEINE EIEEIE A` | - | - | `N T ITE` | - | - |
| 5210 | `TEM E SWT EDE EI AIN NE` | - | - | - | - | - |
| 5211 | - | - | `EII` | - | - | - |
| the other twelve | - | - | - | - | - | - |

- **Shape stops every seed.**
- 5201 and 5208, the two seeds unit 584 added, are stopped with all three tests on. 5201 does not print with the tests off, since round one's loudest first. 5208 is stopped by edges, by shape and by tone, and not by narrowness.
- In rounds two and three, with the tests judged on the sequence, the board's own carrier run printed at no seed.

**Both scoreboards' rows:**
- the twelve-plus-three: `| 585 | 2026-10-21 | 318 of 353 | 20 | 1 | **297** | 0 | 86 of 110, 3 added | one finder, loudest first (work instruction 585, HM-DEC-289): three rounds, 253 with it on, left off and the one-finder line ended for now; the board unchanged |`
- W1AW: `| 585 | 2026-10-21 | **2111** | 2131 of 2151 | 20 | 0 | 438 of 440 | one finder, loudest first (HM-DEC-289): 2098 with 8 invented on, left off; unchanged |`

**The traces** are kept under `.run-unit\reports\unit585-*`, uncommitted.

## 4. What's blocking us

**What the twelve-plus-three still loses with one finder.** It is several causes, not one, but one dominates: the station's opening.
- A bank lane calls nothing until its own contrast, taken over three seconds of its own history, clears 12 dB. A station that starts after quiet therefore loses its first letters, where the grid read them at once. Round three loses the `F` of `FER` on the first recording, the `N T` of `N TEMP`, reads `RR` as `EENR` and `FER` as `IER` on `221828`, and loses the second station's first words on `221530`, 31 to 15.
- Second come noise lanes at that same floor. They stand and are held while nothing louder is keying, and print junk: `TENTESIS` on `221530`, 8 invented letters across the board, and 3 letters in silences.
- Third, a few broken letters inside a call: `KM3STU` reads `KMSTSTU`.
- Loudest first moved none of it, since in each trace there was nothing louder yet to judge against.

That is the owner's evidence for going back to two finders: the grid reads an opening the lanes cannot, while the lanes read the short-dah, fading and weak stations the grid cannot.

**Other open items:**
- **`tightfist-easy`** reads nothing in rounds two and three, where two finders read 13 of 14 and round one read 9. The lane at the fist's own dit stands after a lane at another dit has been held.
- **CPU with one finder.** Judging every lane mark on the grid, and feeding failing marks into the gates, slowed the chain about two and a half times on the survey. This matters only if one finder is taken up again.
- **The 0 dB CQs** read 19 of 84 in round one and nothing after, once noise marks were kept in the gates.
- **Choices taken, named under HM-DEC-276:**
  - "forming" means a sequence of three marks or more at shape 0.3, within the 1 s hold;
  - the grid's tests are read on the grid bin nearest the lane's pitch, and a test whose hops are not held is not taken;
  - two thirds of the last ten marks is the shadow rule's own share over the same marks;
  - the tone test is reported and gates nothing, since the grid never applied it;
  - "two lanes" means another lane within 50 Hz, on another dit or at another pitch, forming at 0.3.

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
