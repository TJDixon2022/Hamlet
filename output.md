## 1. What Claude did

**Gate.** The work was done on the development computer under the prompt's `PROJECT: Hamlet`. The tree confirmed it: `PROJECT_CARD.md` says `PROJECT: Hamlet`, `Hamlet.sln` is at the root, `SHACK_FACTS.md`, `docs\cw-scoreboard.md` and `src\Hamlet.RadioEngine\Cw\CwLaneBank.cs` exist, and neither `CoreHMI.sln` nor `MURC.sln` does. Nothing in this report is evidence about the radio.

**The hand run.**
- I took `SESSION.lock` through `tools\arbiter\lock.bat take` and released it at the end.
- Nothing was written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` was touched, and nothing was ticked in `PHASE_PLAN.md`.
- Nothing keyed or transmitted, and nothing was written to the radio.
- No recording was added, removed or promoted.
- Everything is on branch `main`. The tag `before-one-finder` stands.

**Measuring.** Each round's rule was measured by taking `BankFinds` out of `CwRules.OffByDefault` for that round's build and putting it back before the commit. One test run at a time: the survey, then the guards with the board, then traces.

**The trace, first** (`TheOpeningTests.TheOpeningTraced`, with `BankFinds` on as it stood; the tables are in section 3).
- **At the start of audio** (the first recording's `FER`, `144045`'s `N TEMP`, `221828`'s `FER`), a bank lane had no key-up or key-down at all for about its first second. It takes them only from a third of its three-second history. The grid's first candidate was at the station's first mark.
- **After quiet** (`221530`'s second station, keying from 9.17 s):
  - the lane's key-up held near −40 dB;
  - its key-down was −32.3 dB at 9.17 s and still −31.1 at 9.93 s, against the station's −18 to −20;
  - its contrast first cleared 12 dB at 10.2 s, where the lane called its first mark, 1.03 s after the station's first;
  - the floor was right; **the ceiling was the delay**, and with it, at the start of audio, the wait for any lines at all.
- `144020`'s second station and W1AW `piece-02`'s `QST` after its pause already read their openings: contrast 14 to 20 dB on the first mark.
- **The one change named:** a lane takes its lines once it holds a dah of hops, and lifts its key-down to the top of its last dah while it holds nothing.

**Round one: `CwRules.LaneHearsTheOpening`** (`74ad1d74`), under `BankFinds` only, in `CwLaneBank`:
- a lane takes its key-up and key-down once it holds `OpeningDits`, three of its dits in hops, rather than a second;
- while it holds nothing, its key-down is the greater of the 90th percentile and the median of the loudest quarter of its last dah of hops;
- a mark begins where its level rose, as a grid mark does.

Measured: the first recording read whole, the twelve-plus-three 267 with 22 invented, W1AW 2087.

Traced: the largest loss was the invented letters, 22 against 1. On `144020`, `IED■` at 575 Hz in a silence was noise the 575 Hz lane called against its lifted ceiling in the quiet before the second station. It joined the sequence that stood when the station began, and the hold handed it on as backlog. (By recording, `221745` fell most, 14 to 6, but that is not an opening: its station's lanes stand under 0.4 for its first 13 s.)

**Round two** (`827e38f8`). The ceiling is lifted only where the top of the last dah stands `OpeningOverDb`, 6 dB (half amplitude, `EdgeDepthDb`), over the 90th percentile.
- **Measured:** the twelve-plus-three 275 with 12 invented, W1AW 2077 with 14 invented.
- **Traced:** the largest loss was W1AW `piece-04`, 688 to 674. A probe of its held lane (600 Hz, 40 ms, held from 2.1 s) found a 25 ms mark at 178.97 s at −25.2 dB, 8 dB under the station's −17. It sat in the word gap after `REASONABLE`, under the line a mark rises at (−24.4). The 60 ms lane beside it called nothing there, and the held lane printed it as `E`.

**Round three** (`447a45e3`). A mark whose mean level falls under the line it rose at is not called.
- **Measured:** W1AW 2124 with 1 invented, the twelve-plus-three 275.

**The hard stop.** The twelve-plus-three is 275 at the end of the last round, under 294. `BankFinds` is back in `CwRules.OffByDefault`, every one-finder rule stays in the tree behind it, the tag stands, and the one-finder line ends. As shipped, every reading is as at HEAD: 297 and 2111.

**CPU** (`TheOneFinderCost`, W1AW `piece-04`, 300 s of audio): two finders 182 s, one finder 396 s, 117% more. The survey ran 20.5 to 23.7 minutes with one finder on, against 10.9 minutes with two (unit 585's survey line).

**The record**, in one commit after the rounds:
- `DECISIONS.md`: HM-DEC-290, *One finder, the opening*, with the owner's ruling and the hard stop quoted, and the `CLAUDE.md` §1 index row;
- `docs\cw-scoreboard.md`: a 586 row in each table, 297 and 2111;
- `PHASE_OUTCOME.md`, both copies: `## UNIT 586 - STEP 12`;
- `PHASE_STATUS.md`, both copies: names 586;
- `Directory.Build.props`: 1.13.271 to 1.13.272;
- the survey's `Rerun` line, and `docs\cw-survey-2026-10-09.md` regenerated.

Nothing was recorded under §12.1.

**The lines** each ran once after one build, with the build servers shut down, using `--no-build`:
- the app line: 278 of 278;
- the engine line: 159 of 159 in 12.4 minutes;
- the survey line: 1 of 1 in 10.8 minutes. `docs\cw-survey-2026-10-09.md` is identical to the shipped reading apart from its `Rerun` line.

## 2. What the owner should expect

**The hard stop: the twelve-plus-three reads 275 with one finder, under 294, so `BankFinds` stays off and the one-finder line ends.**

**One finder against two, row by row** (round three, with it on, against two finders as shipped):
- **W1AW:** 2124 against 2111. One finder now reads W1AW better than two, with 1 invented letter against none.
- **The hand-sent recordings** (the twelve-plus-three): 275 against 297. The openings are mostly back, but junk prints in a noise recording, and one QSO's slow station reads worse.
- **A short-dah fist:** the dah-1.6 rows 823 of 1440 against 569, dah 2.0 982 against 624.
- **A weak CQ:** the four 5 dB CQs read 83 of 84 against 71, with nothing after them. The 0 dB CQs read nothing, as with two.
- **A fading station:** the fist bank's fading half reads 1195 letters right against 439.
- **The far side** (the working fixtures): 14 of 81 against 4, though 27 wrong. `two-station` reads 27 of 27 against 18.
- **The carrier** prints at one seed of twenty against two. The steady carrier prints nothing.
- **The cost:** one finder takes a little over twice the CPU of two.

**What you run is unchanged:** 297, 2111, and the same `Weak stations` row. Version 1.13.272. By the hard stop, the next units go to what you see on the air with two finders.

**The rule, in a sentence:** with one finder, a lane takes its noise floor from its first dah rather than its first second. Its ceiling rises to a station's first dah where that dah stands well over the lane's loudest tenth, and a mark is a mark only where it stands at the line it rose at.

**What the openings read now** (round three):
- the first recording: `FER C HAT<BT>…`, whole, against `ER C H AT` in unit 585;
- `221828`: `FER ANOTHER…`, against `IER`;
- `144020`'s second station: `WX IN NETAGIT…`;
- `221530`'s second station: 35 of 45, against 15 in unit 585 and 34 with two finders;
- `144045`: `TEMP…`, the `N` still lost;
- `121357`: `EENR` for `RR` still.

**What looks wrong but is not:** the new test classes `TheOpeningTests` and the traces in `TheOneFinderTests` assert nothing.

## 3. What you should see

**The openings, as traced before the rule** (one row per keyed mark, from the board's keying map; each lane gives level, key-up, key-down, contrast and whether it called the mark; the full 50 ms rows are in `.run-unit\reports\unit586-openings-r0.txt`):

*First recording, station 663 Hz, lanes at 650 Hz. First keyed mark 0.156 s, first letter `E` at 1.19 s; the grid's first candidate 0.165 s.*

| keyed mark | ms | lane 100 ms | lane 60 ms | lane 40 ms | grid: from, floor, contrast |
|---|---|---|---|---|---|
| 0.156 | 102 | −20.0, no lines, no | −17.6, no lines, no | −17.8, no lines, no | 0.165 |
| 0.311 | 89 | −19.7, no lines, no | −17.9, no lines, no | −18.0, no lines, no | 0.315, −27.2, 9.7 |
| 0.460 | 225 | −19.0, no lines, no | −17.7, no lines, no | −17.8, no lines, no | 0.465, −28.0, 10.2 |
| 0.744 | 90 | −20.3, no lines, no | −18.2, no lines, no | −18.0, no lines, no | 0.750, −27.9, 10.1 |
| 1.108 | 89 | −20.8 −33.5 −18.9 14.7 yes | −17.6 −33.6 −17.7 15.9 yes | −17.5 −32.0 −17.7 14.3 yes | 1.110, −29.3, 11.7 |

*`144045`, station 598 Hz. First keyed mark 0.085 s, first letter `E` at 2.16 s; the grid's first candidate 0.095 s.* Every lane had no lines until the mark at 1.369 s (contrast 20.7, 18.8 and 16.7 dB, all called).

*`221828`, station 606 Hz. First keyed mark 0.000 s, first letter `I` at 1.17 s; the grid's first candidate 0.570 s.* No lines until 1.07 s; the mark at 1.073 s was called at 15.3, 13.4 and 12.1 dB.

*`221530`'s second station, 597 Hz, lanes at 600 Hz. First keyed mark 9.167 s, first letter `I` at 11.39 s; the grid's first candidate 9.815 s.*

| keyed mark | ms | lane 100 ms | lane 60 ms | lane 40 ms |
|---|---|---|---|---|
| 9.167 | 25 | −41.8 −43.2 −32.3 11.0 no | −37.2 −41.3 −30.4 10.9 no | −27.8 −40.1 −28.5 11.6 no |
| 9.441 | 59 | −21.6 −42.4 −32.1 10.4 no | −18.5 −40.8 −30.3 10.5 no | −19.4 −39.5 −28.4 11.1 no |
| 9.704 | 179 | −18.8 −40.6 −31.1 9.5 no | −19.0 −38.9 −29.2 9.7 no | −18.8 −38.0 −27.4 10.6 no |
| 9.928 | 68 | −20.3 −40.6 −31.1 9.5 no | −17.6 −38.9 −29.2 9.7 no | −18.7 −38.0 −27.4 10.6 no |
| 10.202 | 60 | −23.3 −40.6 −24.9 15.7 yes | −17.6 −38.8 −24.2 14.6 yes | −19.0 −38.2 −23.9 14.2 yes |
| 10.435 | 162 | −20.2 −40.6 −19.6 21.0 yes | −22.5 −38.7 −19.3 19.4 yes | −25.2 −37.8 −19.1 18.7 yes |

*`121357`, station 698 Hz.* From 12.77 s, contrast 8.5 to 11.9 dB for its first five marks, none called, the key-down −30 to −25 against the station's −20 to −22. First called at 13.42 s; the grid's first candidate 13.52 s.

*`144020`'s second station, 598 Hz, from 13.57 s, and W1AW `piece-02` after its pause, from 219.38 s:* every mark was called from the first, at 12 to 20 dB of contrast.

**The rounds, every condition:**

| condition | the bar | two finders | 585, round three | round one | round two | round three |
|---|---|---|---|---|---|---|
| twelve-plus-three score | 297 (294 for a ruling) | 297 | 253 | 267 | 275 | 275 |
| right, wrong, invented | not rising | 318, 20, 1 | 293, 32, 8 | 306, 17, 22 | 306, 19, 12 | 306, 19, 12 |
| spaces | not worse | 86 of 110, 3 added | 85, 14 added | 80, 5 added | 84, 5 added | 86, 5 added |
| W1AW score | 2111 | 2111 | 2098 | 2087 | 2077 | 2124 |
| W1AW wrong, invented | not rising | 20, 0 | 16, 8 | 25, 8 | 25, 14 | 11, 1 |
| fist bank score | 2,500 | 1891 | 2915 | 3034 | 3007 | 2885 |
| fist bank invented | 50 or fewer | 46 | 20 | 8 | 8 | 8 |
| fading half, right | 1,000 | 439 | 1166 | 1209 | 1205 | 1195 |
| flat half, right | 2,129 | 2173 | 2678 | 2703 | 2714 | 2721 |
| the four 5 dB CQs | 71 of 84 | 71 | 80 | 84 | 83 | 83 |
| letters after them | none | none | none | none | none | none |
| short-dah rows 1.6, 2.0 | over the floors | 569, 624 | 832, 886 | 828, 939 | 838, 914 | 823, 982 |
| straight-key hands | 48 of 60 each | held | held | held | held | held |
| loud noise | nothing | nothing | nothing | nothing | nothing | nothing |
| random carrier | two seeds | 2 | 0 | 1 (5195) | 1 (5195) | 1 (5195) |
| steady carrier | none | none | none | none | none | none |
| first recording | whole | whole | `ER C H AT…` | whole | whole | whole |
| letters in silences | none | 0 | 3 | 9 | 7 | 7 |
| strays, highest capture | 6 per 10 s | 3.3 | 1.7 | 1.7 | 1.7 | 1.3 |
| strays, summed | reported | 26.8 | 12.8 | 13.4 | 12.4 | 13.6 |
| easy fixtures | 77 of 90 | 77 | 71 | 87 | 85 | 85 |
| working fixtures | reported | 4 | 17 | 12 | 15 | 14 |
| `two-station` | reported | 18 of 27 | 24 | 27 | 27 | 27 |
| 0 dB CQs | reported | 0 | 0 | 41 | 0 | 0 |
| survey time | reported | 10.9 min | 20.8 min | 23.7 min | 21.6 min | 20.5 min |

**The fist bank's three matrices**, two finders / one finder at round three.

*Dah over dit by speed, flat, 20 dB:*

| | 10 WPM | 15 WPM | 20 WPM | 25 WPM | row |
|---|---|---|---|---|---|
| dah 1.6 | 60 / 46 | 60 / 46 | 60 / 48 | 60 / 49 | 240 / 189 |
| dah 2.0 | 59 / 60 | 60 / 58 | 60 / 60 | 22 / 59 | 201 / 237 |
| dah 2.5 | 59 / 59 | 59 / 59 | 60 / 60 | 60 / 60 | 238 / 238 |
| dah 3.0 | 60 / 60 | 60 / 60 | 60 / 60 | 60 / 60 | 240 / 240 |

*Dah over dit by signal, flat, 20 WPM:*

| | 5 dB | 10 dB | 20 dB | row |
|---|---|---|---|---|
| dah 1.6 | 45 / 52 | 36 / 57 | 60 / 48 | 141 / 157 |
| dah 2.0 | 43 / 58 | 59 / 60 | 60 / 60 | 162 / 178 |
| dah 2.5 | 53 / 60 | 59 / 60 | 60 / 60 | 172 / 180 |
| dah 3.0 | 16 / 59 | 59 / 60 | 60 / 60 | 135 / 179 |

*Signal by fade, dah 2.5, 20 WPM:*

| | flat | 12 dB of QSB at 0.7 Hz | row |
|---|---|---|---|
| 5 dB | 53 / 60 | 0 / 24 | 53 / 84 |
| 10 dB | 59 / 60 | 14 / 22 | 73 / 82 |
| 20 dB | 60 / 60 | 13 / 57 | 73 / 117 |

**The bank's totals:**
- two finders: 2612 of 5760, score 1891;
- round one: 3912, score 3034;
- round two: 3919, score 3007;
- round three: 3916, score 2885, with 1023 wrong and 8 invented.

**The easy and working fixtures**, right of sent:

| fixture | two finders | round one | round two | round three |
|---|---|---|---|---|
| `coverage-easy` | 24 of 26 | 25 | 25 | 25 |
| `exchange-easy` | 22 of 22 | 22 | 22 | 22 |
| `fast-easy` | 18 of 19 | 19 | 19 | 19 |
| `prosigns-easy` | 0 of 9 | 9 | 9 | 9 |
| `tightfist-easy` | 13 of 14 | 12 | 10 | 10 |
| `coverage-working` | 1 of 23 | 4 | 4 | 3 |
| `exchange-working` | 0 of 19 | 3 | 3 | 4 |
| `fast-working` | 3 of 19 | 5 | 5 | 5 |
| `prosigns-working` | 0 of 9 | 0 | 1 | 0 |
| `tightfist-working` | 0 of 11 | 0 | 2 | 2 |

**Both scoreboards' rows:**
- the twelve-plus-three: `| 586 | 2026-10-21 | 318 of 353 | 20 | 1 | **297** | 0 | 86 of 110, 3 added | one finder, the opening (work instruction 586, HM-DEC-290): three rounds, 275 with it on, under 294, so the hard stop: left off and the one-finder line ended; the board unchanged |`
- W1AW: `| 586 | 2026-10-21 | **2111** | 2131 of 2151 | 20 | 0 | 438 of 440 | one finder, the opening (HM-DEC-290): 2124 with 1 invented on, left off by the hard stop; unchanged |`

## 4. What's blocking us

**Left, one line each:**
- **The one-finder line is ended by the hard stop.** A further one-finder unit needs a new ruling. `BankFinds` and the four rules under it (`LoudestFirst`, `LaneMeetsTheGrid`, `TwoLanes` and `LaneHearsTheOpening`) sit in the tree, off.
- **What one finder still lost at the end:**
  - `221745`, 14 to 3: its station's lanes stand under 0.4 for its first 13 s;
  - `143906`, a noise-only recording: 7 invented, 6 in silences, from a 500 Hz lane;
  - `121357`'s `RR`: in the trace before the rule its first marks sat 8.5 to 11.9 dB over the lane's floor, under the 12 dB a lane calls at.
- **CPU:** one finder costs a little over twice the chain. In the test build it reads 300 s of W1AW in 396 s of CPU, slower than real time.
- **Choices taken, named under HM-DEC-276:**
  - "a dah of hops" is three of the lane's own dits, at least four hops;
  - "the top" is the median of the loudest quarter of those hops;
  - 6 dB is the grid's edge depth, half amplitude;
  - the stand-at-the-line test compares the mark's mean with the rise line at its end;
  - all three apply only while the lane holds nothing, except the stand-at-the-line test, which applies to every lane under the rule.

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
