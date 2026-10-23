## 1. What Claude did

Surface: Claude Code on the development computer, branch `main`. The gate: the prompt said `PROJECT: Hamlet`; the tree has `SHACK_FACTS.md`, `docs\cw-scoreboard.md` and `src\Hamlet.RadioEngine\Cw\CwLaneBank.cs`, and neither `CoreHMI.sln` nor `MURC.sln`; the root is `C:\Source\HamLet`. Nothing in this report is evidence about the radio. Hand run: `SESSION.lock` taken through `tools\arbiter\lock.bat take` and released at the end; nothing written to `RUN_LEDGER.md`; nothing under `tools\arbiter\` touched; no box ticked in `PHASE_PLAN.md`. Nothing keyed or transmitted, nothing written to the radio, no recording added, removed or promoted. The tag `before-columns` was set before the first change and pushed. Version 1.13.272 to 1.13.273. HM-DEC-291 recorded.

**The generator's edges, checked first.** The fixtures (the CQs, the fist bank, the receiver fixtures) are keyed by `CwFixtureGenerator.Gate`, a raised cosine of 5 ms (`EdgeSeconds`); the carrier tests and the hands key a raised cosine of 4 ms. The radio's shape, a quarter longer than the manual's default. The fixtures were **not regenerated**: 36 committed receiver fixtures carry reference scores bound to their bytes (HM-DEC-101, `CwFixtureCommitTests`) that only `tools/score-fixtures`, a Python tool, can renew, and Python does not run here. So no re-baseline was taken; the baseline is the shipped survey, and the first proof shows the 4 ms template finding 5 ms edges at 20 and 10 dB.

**`KeyingEdgeMs` is 4 ms**: the IC-7300's CW-KEY SET rise time offers 2, 4, 6 and 8 ms (`A7292-4EX-6` p. 4-21, cited in `CLAUDE.md` §4). Four is the manual's default as the author recalls it; the page was not re-read in this session, and the owner's own setting is on no sheet in the tree. Marked as such in the code and carried as an ask in section 4.

**The finder, `CwColumnFinder`**, behind `CwRules.Columns`, off by default:
- a lane every 50 Hz across the passband, the audio mixed to the pitch and low-passed on both quadratures by a fourth-order Butterworth at 150 Hz, the amplitude read every millisecond;
- the template is the radio's raised-cosine edge put through the lane's own low-pass, so the filter's delay is inside it; a rise is where the normalized correlation over four edge-lengths peaks at `EdgeMatch` 0.90 or better, stands `ColumnHeightDb` 6 dB over the chaos just before it (at most six edge-lengths, never past the last edge judged), and whose 20 to 80 per cent transition, read outward from the edge's centre, is no slower than `EdgeSlowest` 2.5 times the template's own; a drop is the template upside down, judged against the chaos three to five edge-lengths after it;
- a column is a rise paired with the next drop with no rise between, its top (from two edge-lengths after the rise to the drop) at least `TopLeastEdges` three edge-lengths and deviating about a straight line no more than `TopFlatness` 0.45 of the chaos's deviation before the rise;
- two lanes holding one column are resolved by height alone; the pitch is the slope of the tallest lane's phase over the top.
- With the rule on, the columns are the only marks both gates see (`CwEnvelopeDetector`): the grid's marks reach neither gate and the bank's lanes are not run; neither gate judges a column's level (`CwMark.Column`, `CwPatternGate.Agrees`, `CwSenderGate.TakeMark`); the shadow rule does not judge it. Off, nothing changes.
- No level crossing, percentile floor or contrast line decides whether a column exists. Every figure is about the shape and is named: `KeyingEdgeMs`, `LaneBandwidthHz`, `LaneSpacingHz`, `StepMs`, `ColumnHeightDb`, `EdgeMatch`, `EdgeSlowest`, `TopFlatness`, `TopLeastEdges`, `EdgePairDb`, `RiseLeastDb`, `ChaosEdges`.

**The first proof** (`TheColumnTests.TheFirstProof`), run before the finder went near the survey, its figures chosen from a sweep (`TheFiguresSwept`) rather than guessed: at the end of round one, dits and dahs found 20, 19 and 6 and 20, 19 and 2 of 20 at 20, 10 and 5 dB; bursts stood 0 of 20 at every level, both the slow-edged burst (edges five keying-edges long, no keyed edge) and the keyed-edge burst (a noise top between two keyed edges); the band alone 2, 3 and 0 columns a minute. Section 3 has the tables.

**Round one** (`128a672c`): the finder as above, with the drop held to within 6 dB of the rise either way. With `Columns` on: the twelve-plus-three 22 against 297, W1AW -52 against 2111, the fist bank 2406 from 1891, the easy fixtures 90 of 90, the 15 dB CQs 83 of 84, the 5 dB CQs 4 of 84, the carrier at 3 seeds, CPU 28 s against 181 s on `piece-04`.

**Round two** (`f3a2b3bc`): the trace (`TheColumnsTraced`) on W1AW `piece-04` and the first recording named 35 of 54 closings in the station's lane failing as *edges of two heights*, rises 6 to 9 dB and drops 21 to 24 dB. The receiver's AGC releases over a gap, so the floor before a rise sits far above the floor after a drop; while the key is down it can only pull the gain down. The pair test became one-sided (a drop at least the rise less 6 dB, any deeper allowed; a shallower drop is a dip inside something else, which is how a loud noise burst had stood on its own dips), and the drop's chaos moved past the lane's decay tail, which at two edge-lengths capped every strong drop near 24 dB. The boards 91 and 1468.

**Round three** (`c713900d`): the elements still missed were the first after a letter or word gap. The lane probe (`TheLaneProbed`) at 99.35 to 99.80 s of `piece-04` shows the floor at -21 to -24 dB before a dit whose top is -17 dB, and -35 to -55 dB after it. The column's height is now proved by its drop, and the rise is held to `RiseLeastDb` 3 dB, matched and steep like any edge. `DEDICATED` read whole and the first recording's `F` and `<BT>` came back. The boards 153 and 1758; but the lower rise let short noise tops stand: at 20 dB one slow-edged and three keyed-edge bursts of 20, at 10 dB one.

**Builds and runs.** The finder was built up over several builds and proof runs before round one's survey, and each round was then built once for its measure with `Columns` taken out of `OffByDefault` for that build and put back before its commit. The record build, the solution whole with `Columns` off, was built once at the end and the build servers shut down; the app line, the engine line and the survey line ran once each with `--no-build`, one at a time.

**Recorded** under HM-DEC-276 and HM-DEC-284, not a ruling: HM-DEC-291 in `DECISIONS.md` and the `CLAUDE.md` §1 row; `docs\cw-scoreboard.md` 587 rows; `PHASE_OUTCOME.md`, both copies, `## UNIT 587 - STEP 12`; `PHASE_STATUS.md`, both copies, naming 587; `Directory.Build.props`; the survey's `Rerun` line. Nothing was recorded under §12.1.

## 2. What the owner should expect

**The first proof, by shape alone:** at 20 and 10 dB the finder stands the dit and the dah 20 and 18 of 20 times and stands no burst at 10 dB; at 20 dB, after round three, four short noise tops of 120 stood as columns (one in round two, none in round one); at 5 dB it stands 9 dits and 4 dahs of 20 and no burst. Under 12 dB of fade: 20 and 20, 13 and 13, 8 and 7. The band alone stands 0 to 3 columns a minute. **So a 150 Hz lane sees the shape cleanly down to 10 dB and only half the time at 5 dB**, where the tone is 7 dB over the lane's own noise.

**Columns against two finders, row by row, at the end of round three with `Columns` on:**

| | two finders (shipped) | columns |
|---|---|---|
| W1AW | 2111 (2131 right, 20 wrong) | 1758 (1927 right, 169 wrong, 0 invented) |
| the hand-sent recordings | 297 (318 right, 20 wrong, 1 invented) | 153 (226 right, 73 wrong, 0 invented) |
| the first recording | whole | `FET C IEAT<BT> BES IU TEI SA KCVZGM DEWA` |
| short-dah rows, dah 1.6 / 2.0 | 569 / 624 of 1440 | 798 / 792 |
| the fist bank | 1891 (2612 right, 46 invented) | 2428 (3180 right, 0 invented) |
| a fading station: the fading half, letters right | 439 of 2880 | 1447 |
| the flat half | 2173 of 2880 | 1733 |
| a weak CQ: the four 5 dB CQs | 71 of 84 | 0 of 84, nothing after them |
| the 15 dB CQs | 78 of 84 | 83 of 84 |
| the far side: `two-station` | 18 of 27 | 8 of 27 |
| the working fixtures | 4 of 81 | 9 of 81 |
| the easy fixtures | 77 of 90 | 90 of 90 |
| the random carrier | 2 seeds of 20 | 3 seeds (5195, 5203, 5204) |
| letters in silences, letters invented | 0, 1 | 0, 0 |
| strays summed over the 65 old captures | 26.8, highest 3.3 | 24.8, highest 4.0 |
| CPU on `piece-04`, 283 s of audio | 181 to 185 s | 28 to 31 s |

**It does not ship.** `Columns` stays off. The twelve-plus-three is 153 against 297 and W1AW 1758 against 2111, far outside the 1 per cent band for a ruling; the flat half of the fist bank is 1733 against 2173; the 5 dB CQs read nothing against 71 of 84; the random carrier prints at three seeds against two; the first recording is not whole; and the proof's burst condition broke at 20 and 10 dB in round three.

**The rule in a sentence:** a mark is a rise shaped like the radio's keying edge, a flat top of at least three edge-lengths, and a drop shaped like the edge, each edge proved against the chaos on its own side, the drop at least as deep as the rise; nothing else reaches the gate.

**What reads now with it on.** Every clean fixture reads whole: the synthetic fixtures 67 of 67 from 46, the easy fixtures 90 of 90 from 77, every 15 dB CQ but one letter, the fading hands 1447 letters from 439, the short-dah hands 798 and 792 from 569 and 624, the nine straight-key hands 60 of 60, with nothing invented anywhere and a sixth of the CPU. The owner's own recordings and W1AW read worse: on W1AW the columns delivered are the right lengths (42 ms dits and 122 ms dahs at 30 WPM) but first elements after a gap are still lost where the AGC had lifted the floor to within 3 dB of the top, the floor's own climb sometimes stands as a rise, and the station's leakage 50 to 70 Hz off is delivered as columns of its own. Those recordings were made with AGC FAST, before HM-DEC-268 set SLOW.

**Finder or gate.** Columns stood in the chaos where marks did not, on every clean fixture and every fading hand. The losses are the finder's at the weak end and at the edges: at 5 dB in the passband a 150 Hz lane holds the tone 7 dB over its own noise and the finder stands half the elements, so the 5 dB CQs and the bank's 5 dB column read nothing (an edge or a flat misjudged against chaos of the column's own size); and on the AGC-shaped recordings the rise before a first element is misjudged against a lifted floor. The carrier at three seeds is the gate's: a carrier keyed at random with 4 ms edges makes true columns, delivered right, and the two-kinds test alone did not stop it where the level rules the columns bypass had. The next unit, if there is one, is aimed at the finder's lane (its bandwidth against the 5 dB signal) and at the AGC's floor, not at the gate.

## 3. What you should see

**The first proof, round one** (`TheFirstProof`, figures height 6 dB, match 0.90, slowest 2.5x, flatness 0.45, top 3 edges, pair 6 dB; the template through the lane `0.00 0.00 0.00 0.00 0.00 0.00 0.03 0.13 0.35 0.64 0.89 1.04 1.07 1.04 1.00 0.98`, transition 3 steps):

| dB | element | columns found of 20 | other columns stood | slow-edged bursts stood of 20 | keyed-edge bursts stood of 20 |
|---|---|---|---|---|---|
| 20 | dit | 20 | 2 | 0 | 0 |
| 20 | dah | 20 | 2 | 0 | 0 |
| 10 | dit | 19 | 2 | 0 | 0 |
| 10 | dah | 19 | 2 | 0 | 0 |
| 5 | dit | 6 | 2 | 0 | 0 |
| 5 | dah | 2 | 3 | 0 | 0 |

Under 12 dB of fade at 0.7 Hz, the element placed at twenty points of the cycle: 20 dB 20 and 20, 10 dB 14 and 13, 5 dB 8 and 8. The band alone, sixty seconds, three seeds: 2, 3 and 0 columns a minute.

**The first proof, rounds two and three:**

| dB | element | round two found | r2 slow / keyed bursts | round three found | r3 slow / keyed bursts |
|---|---|---|---|---|---|
| 20 | dit | 20 | 1 / 0 | 20 | 1 / 3 |
| 20 | dah | 20 | 0 / 0 | 20 | 0 / 0 |
| 10 | dit | 18 | 0 / 0 | 18 | 0 / 0 |
| 10 | dah | 18 | 0 / 0 | 18 | 1 / 0 |
| 5 | dit | 6 | 0 / 0 | 9 | 0 / 0 |
| 5 | dah | 2 | 0 / 0 | 4 | 0 / 0 |

Fade, round three: 20 and 20, 13 and 13, 8 and 7. The band alone: round two 1, 1 and 0; round three 0, 1 and 3 a minute.

**The sweep that chose the figures** (lane 150 Hz, top 3 edges, height 6 dB; dits and dahs found of 20 at 20/10/5 dB; bursts stood of 20 at 20, 10 and 5 dB as slow, keyed; the band alone per 30 s):

| match | flatness | dits | dahs | 20 dB bursts | 10 dB bursts | 5 dB bursts | band alone |
|---|---|---|---|---|---|---|---|
| 0.85 | 0.50 | 20/20/8 | 20/20/8 | 4, 1 | 5, 0 | 0, 0 | 3 |
| 0.875 | 0.50 | 20/20/6 | 20/20/6 | 3, 1 | 1, 0 | 0, 0 | 3 |
| 0.90 | 0.50 | 20/19/6 | 20/19/5 | 1, 0 | 0, 0 | 0, 0 | 2 |
| 0.875 | 0.45 | 20/20/6 | 20/20/3 | 2, 0 | 1, 0 | 0, 0 | 3 |
| **0.90** | **0.45** | 20/19/6 | 20/19/2 | 0, 0 | 0, 0 | 0, 0 | 2 |
| 0.90 | 0.40 | 20/19/3 | 20/18/0 | 0, 0 | 0, 0 | 0, 0 | 2 |

A 100 Hz lane found more at 5 dB (11 to 14 dits) and stood four times the noise columns (9 to 17 per 30 s) and more bursts; a top of one edge-length stood four times the noise columns of three.

**The rounds' table**, every condition, two finders as shipped then rounds one, two and three with `Columns` on:

| condition | two finders | round one | round two | round three |
|---|---|---|---|---|
| twelve-plus-three: score (right, wrong, invented) | 297 (318, 20, 1) | 22 (85, 63, 0) | 91 (186, 95, 0) | 153 (226, 73, 0) |
| spaces right of 110, added | 86, 3 | 27, 17 | 75, 20 | 76, 19 |
| letters in silences | 0 | 0 | 0 | 0 |
| W1AW: score (right, wrong, invented) | 2111 (2131, 20, 0) | -52 (512, 564, 0) | 1468 (1757, 289, 0) | 1758 (1927, 169, 0) |
| the first recording | whole | `U NE` | `RET C IETT V BS IU STKAUTETM IEWA` | `FET C IEAT<BT> BES IU TEI SA KCVZGM DEWA` |
| fist bank: score (right, wrong, invented) | 1891 (2612, 675, 46) | 2406 (3214, 808, 0) | 2459 (3252, 793, 0) | 2428 (3180, 752, 0) |
| flat half: right, score | 2173, 1896 | 1737, 1563 | 1780, 1638 | 1733, 1568 |
| fading half: right, score | 439, -5 | 1477, 843 | 1472, 821 | 1447, 860 |
| short-dah rows 1.6 / 2.0 (floors 540 / 592) | 569 / 624 | 805 / 809 | 827 / 821 | 798 / 792 |
| the nine hands at 60 | 9 of 9 | 9 of 9 | 9 of 9 | 9 of 9 |
| 5 dB CQs of 84; after them | 71; nothing | 0 (survey 4); nothing | 0; nothing | 0; nothing |
| 15 dB CQs of 84 | 78 | 83 | 83 | 83 |
| 0 dB CQs of 84 | 0 | 0 | 0 | 0 |
| random carrier, seeds of 20 | 2 | 3 | 3 | 3 |
| steady carrier; noise alone (`143906`) | none; nothing | none; nothing | none; nothing | none; nothing |
| strays summed; highest capture | 26.8; 3.3 | 34.6; 3.3 | 31.9; 3.3 | 24.8; 4.0 |
| easy fixtures of 90 | 77 | 90 | 90 | 90 |
| working fixtures of 81 | 4 | 6 | 10 | 9 |
| ungraded of 98; `two-station` of 27 | 73; 18 | 47; 7 | 53; 10 | 48; 8 |
| synthetic fixtures of 67 | 46 | 67 | 67 | 67 |
| CPU on `piece-04` (two finders in the same run) | 181 s | 28 s (181) | not rerun | 31 s (185) |
| the survey's wall clock | 10.8 min | 4.3 min | 4.3 min | 4.1 min |

**The three matrices, round three** (letters right of 60; two finders as shipped in brackets):

Dah over dit by speed, flat, 20 dB:

| | 10 WPM | 15 WPM | 20 WPM | 25 WPM | row |
|---|---|---|---|---|---|
| dah 1.6 | 60 (60) | 60 (60) | 60 (60) | 60 (60) | 240 (240) |
| dah 2.0 | 60 (59) | 60 (60) | 60 (60) | 59 (22) | 239 (201) |
| dah 2.5 | 59 (59) | 59 (59) | 60 (60) | 60 (60) | 238 (238) |
| dah 3.0 | 59 (60) | 60 (60) | 60 (60) | 60 (60) | 239 (240) |

Dah over dit by signal, flat, 20 WPM:

| | 5 dB | 10 dB | 20 dB | row |
|---|---|---|---|---|
| dah 1.6 | 0 (45) | 55 (36) | 60 (60) | 115 (141) |
| dah 2.0 | 0 (43) | 49 (59) | 60 (60) | 109 (162) |
| dah 2.5 | 0 (53) | 51 (59) | 60 (60) | 111 (172) |
| dah 3.0 | 0 (16) | 47 (59) | 60 (60) | 107 (135) |

Signal by fade, dah 2.5, 20 WPM:

| | flat | 12 dB of QSB at 0.7 Hz | row |
|---|---|---|---|
| 5 dB | 0 | 15 | 15 |
| 10 dB | 51 | 20 | 71 |
| 20 dB | 60 | 58 | 118 |

**The fixtures, round three** (two finders in brackets): easy `coverage` 26 of 26 (26), `exchange` 22 of 22, `fast` 19 of 19 (17), `prosigns` 9 of 9 (0), `tightfist` 14 of 14 (12); working `coverage` 2 of 23, `exchange` 3 of 19, `fast` 3 of 19, `prosigns` 1 of 9, `tightfist` 0 of 11; ungraded `farnsworth-heavy` 11 of 11 (10), `farnsworth-light` 1 of 11 (6), `qsk-preamble` 22 of 22 (22), `two-station` 8 of 27 (18), `two-station-first` 0 of 13 (6), `two-station-second` 6 of 14 (11).

**The boards' rows, round three** (right of sent, wrong; two finders in brackets): `200157` 15 of 27, 12 (27, 0); `144020` 24 of 28, 2 (26, 1); `144045` 18 of 23, 2 (23, 0); `221530` 19 of 45, 9 (34, 3); `221548` 11 of 18, 5 (14, 3); `221745` 15 of 29, 11 (21, 7); `221805` 24 of 33, 4 (28, 1); `221828` 22 of 41, 9 (39, 2); `212015` 32 of 44, 7 (42, 2); `121324` 11 of 19, 6 (18, 1); `121357` 12 of 14, 0 (14, 0); `121414` 23 of 32, 6 (32, 0); the four low or empty stretches 0. W1AW `piece-01` 205 of 242, 26 wrong (242, 0); `piece-02` 335 of 392, 41 (388 right); `piece-03` 749 of 815, 53; `piece-04` 638 of 702, 49 (688 right).

**The round-two trace** on `piece-04` 97 to 105 s, 600 Hz lane, verdicts at the closings: round one's finder 11 a column, 35 edges of two heights, 5 top not flat, 3 an edge too short; round two's 42 a column, 5 top not flat, 3 an edge too short. The lane probe, 99.35 to 99.80 s: -34 to -21 dB before the dit at 99.400, -17 dB on its top, -27 to -55 dB after its drop at 99.440.

**Both scoreboards' rows, as shipped:** the twelve-plus-three 297, 318 of 353 right, 20 wrong, 1 invented, 0 in silences, spaces 86 of 110 with 3 added; W1AW 2111, 2131 of 2151 right, 20 wrong, 0 invented, 438 of 440 spaces. Both unchanged by this unit.

**The lines**, once each at the end, `--no-build`, after one build of the solution with `Columns` off: the app line 278 of 278 in 2.5 minutes; the engine line 159 of 159 in 12.5 minutes; the survey line 1 of 1 in 11.2 minutes, the survey document identical to the shipped one apart from its `Rerun` line.

**Commits on `main`:** `128a672c` round one, `f3a2b3bc` round two, `c713900d` round three, and the record. Tag `before-columns` at `b275bc85`.

## 4. What's blocking us

**Left, one line each:**
- **The finder is not the gate's problem.** `Columns` sits in the tree, off. If the line continues, the next unit goes to the lane: a 150 Hz lane holds a 5 dB signal 7 dB over its own noise, and the finder stands half its elements there; a narrower lane found more at 5 dB in the sweep and stood four times the noise columns, so the shape tests, not the figures, would have to carry it.
- **The AGC.** The owner's recordings and W1AW were made with AGC FAST, before HM-DEC-268 set SLOW. On them the floor before a first element sits within 3 to 6 dB of its top and 15 to 30 dB above the floor after its drop. The boards remain the yardstick, so a shape finder has to read FAST-AGC audio; whether a recording made with SLOW reads differently is not known.
- **Shadows.** The station's leakage 50 to 70 Hz off its pitch is delivered as weak columns of its own (-33 dB against -17 on `piece-04`), at a pitch the gates treat as another sender. The resolution by height merges only columns at one pitch; across pitches it was not built.
- **The carrier.** A carrier keyed at random with 4 ms edges makes true columns; with the level rules bypassed the two-kinds test alone let three seeds print. That is the gate's.
- **The first proof's burst condition** broke in round three (four bursts of 120 at 20 dB, one at 10 dB): short noise tops of 12 to 22 ms pass flatness by chance under a 3 dB rise. Round two's finder stood one burst of 120 at 20 dB, round one's none.
- **The generator keys a 5 ms edge** where the radio's default is 4; not regenerated, for the reason in section 1.
- **CPU:** columns cost a sixth of two finders; the survey ran in 4.1 to 4.3 minutes against 10.8.

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
