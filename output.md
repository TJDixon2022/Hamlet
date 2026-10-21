## 1. What Claude did

Surface: Claude Code on the development machine. The prompt opened `PROJECT: Hamlet`, and the tree confirmed it: `SHACK_FACTS.md`, `docs\cw-scoreboard.md` and `src\Hamlet.RadioEngine\Cw\CwLaneBank.cs` exist, and neither `CoreHMI.sln` nor `MURC.sln` does. Nothing in this report is evidence about the radio. Branch `main`.

This was a hand run:
- `SESSION.lock` was taken through `tools\arbiter\lock.bat take` and released at the end.
- Nothing was written to `RUN_LEDGER.md` or under `tools\arbiter\`, and nothing in `PHASE_PLAN.md` was ticked.
- Nothing keyed and nothing was written to the radio. No recording was added, removed or promoted.
- The tag `before-one-finder` was set and pushed before the first change.

**Round one: the bank as the finder, behind `CwRules.BankFinds`** (commit `088fec81`). With the rule on:
- **The gates take only lane marks.** The grid's marks are no longer offered to the pattern gate or the sender gate; the grid stays for the scope. The verdict, the light and the scope read the bank's standing lanes. The catch ear, the capture triggers and the sheet's `senders` line already read the sender gate, which now holds only lane stations.
- **One station per pitch.** Of the lanes within 50 Hz standing at shape 0.4, the station is the one whose dit is nearest the dit their two kinds give, or the best-shaped while they are not two. The others there feed nothing while it holds. A held lane moves to a lane at its pitch that fits better by more than 1.2. `CwLaneBank.HeldAll` holds every held lane at once, each reading under its floor.
- **Standing and holding.** A lane standing at 0.4, not a carrier and not a shadow, is held at once and hands on what it stood on and every mark after. Its marks carry `HeldLane`, and neither gate judges their level. The hold ends when the lane's sequence stops standing, when it is judged a carrier or a shadow, or in a pause of three of its sender's word gaps. After a pause, a lane there is held again only once it has stood on a new mark of its own. There is no fallback clock.
- **The station's window.** Neither the station's own window nor a provisional window opens; the held lane is the window.
- **The seam's rules, with `BankFinds` on:**
  - `LaneLeadSeconds` gates nothing. It is still the 3 s window of the shadow-backlog rule, which stays.
  - The grid-first rule does nothing: it lives only in the old handover, which does not run.
  - The take-back does nothing: it lives in the handover, and the grid's marks reach no gate.
  - `GridStepsAside` does nothing: handover only.
  - `HoldWaitsAndFits` does nothing: handover only. The finder has its own refit, always on.
  - `HeldLanePrintsWithinSeconds` does nothing: it is read only under `HoldWaitsAndFits`.
  - All are left in the code for the off path.
- **Measured:** the bank's fading half rose to 1094, but the recordings fell, the twelve-plus-three to 229 and W1AW to 2006. The largest single loss was W1AW `piece-02`, 388 to 326.

**Round two** (commit `2102242a`).
- **The trace** (`TheOneFinderTests.TheLargestLossTraced`): on `piece-02` the 600 Hz station was held at 7.59 s with no backlog, and on the first recording the 675 Hz station at 3.03 s likewise. A lane found within 3 s of a shadow let go hands on no backlog. With one finder, a station's own shadows stand beside it, are held, and are let go as the station stands, so that rule removed every station's opening: `T, AT D OCT` and `FER C`.
- **The fix:** the rule spares the lane within 50 Hz of the station the shadow shadowed.
- **Measured:** W1AW 2026, `piece-02` 344. The twelve-plus-three was unchanged at 229. On the first recording the shadows were judged against one another before the station stood, so the exception never fired there.

**Round three** (commit `1f805c48`).
- **The trace,** on W1AW `piece-01`, which kept its opening and lost 20 letters wrong:
  - at 129.17 s the station's own held lane, 600 Hz, was judged a shadow and let go;
  - it was re-held at 625 Hz with no backlog, so the `1` of `W1AW` lost a dah and printed `J`;
  - it then refitted to the 100 ms lane, slower than W1AW's dit, and the letters after it broke (`SCHEDUR TPPEARS`).
- **Why:** the shadow test takes the stronger of two stations by contrast. Each bank lane's contrast is over its own key-up, so it says how the lane sits over its own noise, not which lane carries the station. A station's leakage into the lanes beside it is always quieter by level than the station on its own pitch.
- **The fix:** with one finder, lane is judged against lane by level (`CwShadow.IsLaneShadowOf`).
- **Measured:** W1AW 2111, `piece-01` 242 of 242. The twelve-plus-three rose to 239.

**The result: it fails section 4, and `BankFinds` stays off**, in `CwRules.OffByDefault` and chosen only by a test. Failed conditions:

| condition | needed | round three | by how much, and where |
|---|---|---|---|
| the twelve-plus-three | 297, wrong and invented not rising, spaces not worse | **239**, 33 wrong, 7 invented, 82 of 110 spaces with 14 added | 58 under. `cw-2026-10-03-221530` reads `SA TTP IM` for `6 CHAMPION BK` and 17 of 34 in its second stretch; `cw-2026-10-03-221502` and `221745` lose letters; the first recording reads `ER C H AT…` |
| W1AW | 2111, wrong and invented not rising | 2111, 10 wrong, **3 invented** | invented from 0: `piece-02` 1, `piece-03` 2 |
| the fading half | 1,200 | **1111** | 89 under |
| the easy fixtures | 77 of 90 | **71** | `tightfist-easy` 0 of 14 from 13 |
| the random carrier | two seeds at most | **four**: 5195, 5201, 5207, 5208 | two over |
| nothing in silences | nothing | **3 letters** | `cw-2026-10-03-144020` 2, `cw-2026-10-03-221502` 1 |

The twelve-plus-three is 239, well under the 294 of the owner's-ruling band, so nothing goes under *For the owner's ruling*. Every round's figures, the trace and the tag stand.

**Recorded:** HM-DEC-288, in `DECISIONS.md` and the `CLAUDE.md` §1 index. Also:
- `PHASE_OUTCOME.md`, both copies: `## UNIT 584 - STEP 12`;
- `PHASE_STATUS.md`, both copies: names 584;
- the 584 rows on both scoreboard tables, at the figures as shipped;
- the survey's Rerun line;
- the version, 1.13.269 to 1.13.270.

`CW_SPEC.md` is untouched because nothing shipped. No ruling was added beyond HM-DEC-288, and nothing was recorded under §12.1.

## 2. What the owner should expect

**With one finder, Hamlet reads far more of the hands and the weak stations, and less of the real recordings. So it stays off, and as shipped nothing reads differently.**

What one finder gained, round three against as shipped:
- the fist bank: 3772 of 5760 letters from 2612, score **2853** from 1891, invented 7 from 46;
- the fading half: **1111** from 439, and the flat half 2661 from 2173;
- the four 5 dB CQs: **80 of 84** from 71; even the 0 dB CQs read 32 of 84, from nothing;
- the working fixtures: 20 of 81 from 4; `two-station` 24 of 27 from 18;
- the strays on the old captures: 23.7 summed from 26.8, the highest 2.3 per 10 s.

What it cost:
- **the twelve-plus-three: 239** against 297. The two-station recording `221530` and the first recording's opening are the largest losses;
- **W1AW: 2111**, as shipped, but with 3 invented letters where there were none;
- **the random carrier: four seeds** against the limit of two;
- the easy fixtures 71 against 77, where `tightfist-easy` now reads nothing;
- 3 letters printed in silences on the twelve-plus-three;
- 5% more CPU.

The design in three sentences. The grid's marks no longer reach either gate, and every station is found, stood, held and read on a bank lane, one lane per pitch. A lane standing at the green line is held at once, with no wait and no handover, and its marks are its station's. The seam's rules do nothing with it on and are left for the off path.

**Why it read the recordings worse.** The bank finds a strong station's shadows as readily as the station: lanes 100 to 250 Hz off, keyed in step. With two finders the grid stood the station first and the shadows never mattered. With one, each shadow is held until the station stands, and then the shadow rule has to tell them apart. Rounds two and three fixed the two ways it told them apart wrongly: the shadow-backlog rule, and judging by contrast where level is what says which lane carries the station. Each fix recovered part of W1AW, but the twelve-plus-three's hand-sent recordings still lose their openings and pick up junk.

The lines, after one build, with the build servers shut down, each run once with `--no-build`:
- **app:** 278 of 278 in 2 min 26 s;
- **engine, at 1500 s:** 159 of 159 in 12 min 31 s;
- **survey, at 1800 s:** 1 of 1 in 10 min 55 s. It is identical line for line to the shipped reading apart from its Rerun line: the bank 2612 of 5760, score 1891, 46 invented; the boards 297 and 2111; the easy fixtures 77 of 90; the working 4 of 81.

Three commits are pushed to `main`, one per round, and the record is one more. The tree is clean apart from `RUN_LEDGER.md`, `PARKED.md`, `.run-unit/denials.txt`, `.run-unit/reports/*` and the `cases-*.txt` files, which stay uncommitted as ruled, and `PROJECT_STATUS.md`, which holds the panel's status.

**What may look wrong but is not:**
- The survey line ran on a second build, made after the engine line. That build changed only the version and the survey's `Rerun` string; the code was identical to the one the app and engine lines ran.
- Round two's survey was stopped partway by Claude Code because the machine ran critically low on memory. The test run underneath it was never killed and finished on its own; its figures are the ones reported. Nothing was retried, and every run after it was run one at a time.

## 3. What you should see

**The rounds**, every condition, with `BankFinds` on:

| condition | as shipped (HEAD) | round one | round two | round three |
|---|---|---|---|---|
| the twelve-plus-three | **297**, 20 wrong, 1 invented, 86 of 110 spaces, 3 added | 229, 36 wrong, 10 invented, 79 of 110, 14 added | 229, 36, 10, 79 of 110, 14 added | **239**, 33, 7, 82 of 110, 14 added |
| W1AW | **2111**, 20 wrong, 0 invented | 2006, 43, 7 | 2026, 46, 7 | **2111**, 10, 3 |
| the fist bank: letters, score, invented | 2612, **1891**, 46 | 3759, 2844, 9 | 3766, 2861, 9 | 3772, **2853**, 7 |
| the fading half | 439 | 1094 | 1097 | **1111** |
| the flat half | 2173 | 2665 | 2669 | 2661 |
| the four 5 dB CQs | 71 of 84 | 80 | 80 | **80** |
| the 20 s of noise after them | nothing | nothing | not run | not run |
| the short-dah rows; the nine hands | held | held | not run | not run |
| noise | nothing | nothing | nothing | nothing |
| the random carrier | 2 seeds (5195, 5207) | 4 (5196, 5201, 5207, 5208) | 5 (5195 added) | **4** (5195, 5201, 5207, 5208) |
| the steady carrier | none | none | none | none |
| the first recording | whole | `RHAT<BT>…` | `RHAT<BT>…` | `ER C H AT<BT> BEST 7V 73 <SK> KC4ZGP DEWA` |
| letters in silences | none | 6 | 6 | 3 |
| strays, highest; summed | 3.3; 26.8 | 2.0; 26.2 | 2.0; 25.9 | 2.3; 23.7 |
| the easy fixtures | 77 of 90 | 71 | 71 | **71** |
| the working fixtures | 4 of 81 | 20 | 20 | 20 |
| `two-station` | 18 of 27 | 24 | 24 | 24 |

The weak-end, short-dah and straight-key guards were run with the rule on in round one only. Rounds two and three each ran the survey and the scoreboard.

**The three matrices**, letters right of 60, as shipped / round three:

Dah over dit by speed, flat, 20 dB:

| | 10 WPM | 15 WPM | 20 WPM | 25 WPM | row, before | row, after |
|---|---|---|---|---|---|---|
| dah 1.6 | 60 / 57 | 60 / 58 | 60 / 47 | 60 / 48 | 240 | 210 |
| dah 2.0 | 59 / 59 | 60 / 55 | 60 / 58 | 22 / 58 | 201 | 230 |
| dah 2.5 | 59 / 58 | 59 / 58 | 60 / 58 | 60 / 59 | 238 | 233 |
| dah 3.0 | 60 / 59 | 60 / 59 | 60 / 59 | 60 / 59 | 240 | 236 |

Dah over dit by signal, flat, 20 WPM:

| | 5 dB | 10 dB | 20 dB | row, before | row, after |
|---|---|---|---|---|---|
| dah 1.6 | 45 / 45 | 36 / 56 | 60 / 47 | 141 | 148 |
| dah 2.0 | 43 / 56 | 59 / 59 | 60 / 58 | 162 | 173 |
| dah 2.5 | 53 / 59 | 59 / 59 | 60 / 58 | 172 | 176 |
| dah 3.0 | 16 / 59 | 59 / 59 | 60 / 59 | 135 | 177 |

Signal by fade, dah 2.5, 20 WPM:

| | flat, before / after | 12 dB of QSB at 0.7 Hz, before / after | row, before | row, after |
|---|---|---|---|---|
| 5 dB | 53 / 59 | 0 / 21 | 53 | 80 |
| 10 dB | 59 / 59 | 14 / 28 | 73 | 87 |
| 20 dB | 60 / 58 | 13 / 58 | 73 | 116 |

**The easy and working receiver fixtures**, letters right, as shipped / round three:

| fixture | as shipped | round three |
|---|---|---|
| `coverage-easy` | 24 of 26 | 24 of 26 |
| `exchange-easy` | 22 of 22 | 21 of 22 |
| `fast-easy` | 18 of 19 | 18 of 19 |
| `prosigns-easy` | 0 of 9 | 8 of 9, `4 N0CALL <AR> <SK>` |
| `tightfist-easy` | 13 of 14 | **0 of 14** |
| easy, total | 77 of 90 | 71 of 90 |
| `coverage-working` | 1 of 23 | 4 of 23, 12 wrong |
| `exchange-working` | 0 of 19 | 4 of 19, 8 wrong |
| `fast-working` | 3 of 19 | 6 of 19, 9 wrong |
| `prosigns-working` | 0 of 9 | 1 of 9, 4 wrong |
| `tightfist-working` | 0 of 11 | 5 of 11, 1 wrong |
| working, total | 4 of 81 | 20 of 81 |

**W1AW by piece**, as shipped / round three: `piece-01` 242 / 242, `piece-02` 388 / 368 (6 wrong, 1 invented), `piece-03` 793 / 801 (3 wrong, 2 invented), `piece-04` 688 / 700.

**CPU**, W1AW `piece-04`, timed alone, the first read thrown away: two finders 181.9 s, one finder 191.0 s, **5% more**. The grid's marks are no longer gated, but holding and judging every standing lane against every other costs more than that saves.

**Both scoreboards' rows, as shipped:**

| unit | right | wrong | invented | score | spaces |
|---|---|---|---|---|---|
| 584, the twelve-plus-three | 318 of 353 | 20 | 1 | **297** | 86 of 110, 3 added |
| 584, W1AW | 2131 of 2151 | 20 | 0 | **2111** | 438 of 440 |

The traces are in `.run-unit\reports\unit584-trace-r2.txt` and `unit584-trace-piece01.txt`.

## 4. What's blocking us

- **Shadows, with one finder.** The bank stands a strong station's shadows before the station itself, and holds them. Rounds two and three moved the shadow rule onto level and spared the shadowed station's backlog. On hand-sent recordings, the station's openings are still lost and junk prints. A lane is not yet asked whether a louder lane keyed in step with it might still stand, before it is held.
- **The random carrier.** It prints at four seeds through lanes. Its lanes stand like a hand's, and the grid's narrowness and edge tests, which never ran on lanes, no longer stand in front of them.
- **`tightfist-easy`** reads nothing with one finder, where the grid read 13 of 14. Not traced: it was not the largest loss in any round.
- **The `Weak stations` setting** (`LaneHandover`) does nothing with `BankFinds` on, since the bank always finds. The story line's `Weak station at … Hz` is never shown with it on.
- **Without a rig passband there is no bank,** so with `BankFinds` on the grid still finds there.
- **Choices taken, named under HM-DEC-276:**
  - "held" means a standing lane at shape 0.4, the light's green line;
  - after a pause, a lane is held again only once it has stood on a new mark;
  - the shadow-backlog rule keeps its 3 s window, `LaneLeadSeconds`;
  - the rule was measured by setting it on by default for each round's build and survey, then set back before each commit.

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
