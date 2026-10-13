## 1. What Claude did

Development computer; nothing in this report is evidence about the radio.

**The gate:** the prompt and `WORK_INSTRUCTIONS.md` both say `PROJECT: Hamlet`. The tree agrees: `PROJECT_CARD.md`, `Hamlet.sln`, and the order's five checks (`SHACK_FACTS.md`, `docs\cw-scoreboard.md` and `CwLaneBank.cs` present; no `CoreHMI.sln`, no `MURC.sln`). Hamlet confirmed.

**How the unit was run:**
- Branch `main`, run by hand. `SESSION.lock` was taken through `tools\arbiter\lock.bat take` and released at the end.
- Nothing was written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` was touched, and no box was ticked in `PHASE_PLAN.md`.
- Nothing keys, transmits or writes to the radio. No recording was added, removed or promoted.
- Version 1.13.253 to **1.13.254**. Ruling **HM-DEC-274**; HM-DEC-269 stays held. Nothing was recorded under §12.1.

**The order was defective in three ways:**
- It carries no status-write instruction.
- It states no phase count.
- It carries no `Asks still outstanding` list.

I wrote the status on §13.2's triggers anyway, and rebuilt the queue in section 4 from the last report.

**Task 1, the grid goes first** (`d49db406`). Two rules in the handover, each with a name.
- **A lane waits.** `LaneLeadSeconds` is 3 s, the bank's own history. A lane is offered only once its sequence has stood that long with nothing of the grid's (standing, candidate or window) within 50 Hz. It then hands on what it stood on while it waited, oldest first.
  - Its three dits' lanes at one pitch all stand on the same station. Each pitch therefore remembers how far it has handed on, so nothing is handed twice. Before that fix, the 12 WPM CQ printed its first `CQ CQ DE N0` twice.
- **The grid takes it back** (`GridTakesBack`). While a lane holds, the grid's marks near it still reach the grid's own gate, but are not printed.
  - Once the grid stands a station within 50 Hz, or the station's own window is there, the lane is let go at once and the grid's marks print from then on.
  - A provisional window near the held lane is offered and not printed, so the station is never read twice.
- **Measured one at a time, handover on, against HEAD's 297:**
  - The plain handover in this tree reads 290. HM-DEC-273 measured 291; this tree keeps the marks a lane stood on.
  - The wait alone reads 294.
  - The takeback alone reads 296, with invented up from 1 to 2.
  - **Both read 297:** 317 right, 19 wrong, 1 invented.
  - Leads of 2 s and 4 s read the same 297 and the same four CQs as 3 s. W1AW reads 2111 under every variant.

**Task 2, a lane knows a carrier** (`a72eddb5`). A lane whose tone is key-up less than `CwCatchScan.CarrierKeyUpShare`, 0.15, of the last `CwCatchScan.CarrierListen`, 8 s, hands nothing on. A held lane that turns out to be a carrier is let go.
- The rule is the scan's own, called by reference through `CwCatchEar.KeyUpShareOf` over the 8 s of audio the detector already keeps. It is judged at most once a second per pitch.
- **It does not silence seed 5209, and it cannot.** The scoreboard's "carrier" is keyed at random: marks of 30 to 300 ms at gaps of 30 to 400 ms, key-up **0.32** of its time. The order called it unkeyed.
- **What prints at 5209** is a lane at 700 Hz, 75 Hz from the carrier at 625 and so outside the 50 Hz "same station" distance. That lane reads the carrier's keying edges: each of its marks starts within 10 to 30 ms of one of the carrier's own (`TheWeakEndTests.TheCarrierAtTheLane`).
- `cq-18wpm-5db` still stands on its lane and reads 14 of 21.
- **Measured and not built:** a shadow rule that keeps a lane mark back when a grid mark more than 50 Hz away starts within 30 ms of it. It silenced 5209, but the board fell to 291 and `cq-12wpm-5db` to 9 of 21, under half.

**The measure and the ship** (`3afbcf12`).
- **The handover does not ship.** The carrier prints at **2 of 20 seeds** (5195, as at HEAD, and 5209) against a bar of one. File: the scoreboard's `NoiseRuns`, carrier at 625 Hz, 24 dB.
- **Every other condition held:** the four 5 dB CQs at half or better, both boards, and the stray figure.
- It stays off. The two rules and the carrier check stay in the tree.
- No `Weak stations` setting was added and no CQ guard was written, since both were for a handover that ships.
- The survey ran twice: on, to measure, then off, as shipped. The off run writes `docs\cw-survey-2026-10-09.md` row for row as before, with a new `rerun` line.

**Records:**
- `PHASE_OUTCOME.md` (both copies): `## UNIT 570 - STEP 12`.
- `PHASE_STATUS.md` (both copies): names 570.
- `Directory.Build.props`: 1.13.254.
- `CLAUDE.md` §1: a row.
- `DECISIONS.md`: HM-DEC-274.
- `docs\cw-scoreboard.md`: a 570 row in each table.
- `CW_SPEC.md` §2: the handover paragraph, with the three rules and why it is off.

**Tests:**
- **Build** `Hamlet.sln -warnaserror`: 0 warnings, 0 errors.
- **App carry-forward:** 278 of 278.
- **Engine line:** 153 of 154. The one red is `TheRsidDetectorTests.TheDetectorKeepsUpWithRealTime`, red before this unit too.
- **Survey line:** passed, 2 min 24 s off and 5 min 44 s on.
- **Guards** (`TheScoreboardGuardsCwTests`, `TheW1awTableFallsNowhere`, the bank tests): pass.
- **Decision-log and voice tests:** 7 of 7.

## 2. What the owner should expect

**Weak stations still do not read in the app.** The handover that would let them is off, because a randomly keyed carrier at one test seed still prints letters.

**Switched on, it now costs your recordings nothing:**

| what | handover on (this unit) | before (switched on last unit) | as shipped (off) |
|---|---|---|---|
| your twelve plus KM3STU's three | **297**, 19 wrong | 291 | 297, 20 wrong |
| W1AW | **2111** | 2111 | 2111 |
| `cq-12wpm-5db` | **13 of 21** | 15 | nothing |
| `cq-18wpm-5db` | **14 of 21** | 16 | nothing |
| `cq-18wpm-5db-char5` | **16 of 21** | 16 | nothing |
| `cq-25wpm-5db` | **19 of 21** | 17 | nothing |
| random carrier, 20 seeds | **prints at 2** | prints at 2 | prints at 1 |

**The rules in plain words:**
- **A lane waits:** a weak station found by the lanes is handed on only after three seconds with nothing found by the grid nearby. A strong station's opening is therefore the grid's.
- **The grid takes it back:** the moment the grid finds the same station, it takes over.
- **A lane knows a carrier:** a lane whose tone barely ever drops out, under the scan's own carrier line, is never handed on.

**Why it is still off:** the test carrier isn't a steady tone but one keyed at random, so the third rule cannot see it. What prints is a lane 75 Hz beside it, catching the clicks of its keying.

**There is no `Weak stations` setting:** the order asked for one only if the handover shipped. Nothing on screen changes. Pushed to `main`.

## 3. What you should see

**The six letters.** HEAD itself does not print `EN` on `221530` or `ET ON 40T S` on `221805`. What the switched-on handover lost against HEAD was row points, and this is which rule returned them (right / wrong in each stretch):

| recording | HEAD (off) | handover as it was | the wait alone | the takeback alone | both (this unit) |
|---|---|---|---|---|---|
| `221805` 5-30 s | `E 40T U THESE…`, 28 / 1 | `T OONN 4 4T■ U…`, 26 / 6 | 28 / 1, as HEAD | `T ON EU0T S…`, 29 / 2 | as HEAD |
| `144020` 9.5-30 s | `NETAGIT`, 17 / 1 | `NETAGET`, 16 / 2 | 16 / 2 | as HEAD | as HEAD |
| `221530` 9-30 s | `IHES MHENI■S`, 27 / 3 | `FB WHENI■S`, 28 / 2 | 26 / 3 | `THENI■S`, 26 / 2 | 26 / 2, the same row score |
| `143951` 14.5-30 s, low, not counted | 14 of 16 | 8 of 16 | as HEAD | 12 of 16 | as HEAD |

So `221805`'s seven points came back under either rule, `144020`'s two under the takeback only, and `143951` under the wait.

**The four 5 dB CQs**, handover on (ceiling 21 of 21, from the sender's own window opened by hand):

| CQ | this unit | switched on last unit | half |
|---|---|---|---|
| `cq-12wpm-5db` | `FQ N QCQ DE N0 IL N0N L I K`, **13** | 15 | 11 |
| `cq-18wpm-5db` | `RQKQCQ DE N0C DL N0K DL K`, **14** | 16 | 11 |
| `cq-18wpm-5db-char5` | `RQCQN DEN0KALLN0KALLK`, **16** | 16 | 11 |
| `cq-25wpm-5db` | `NQ CQ CQ DE N0CAL I N0CALL K`, **19** | 17 | 11 |

**The carrier at twenty seeds,** handover on:
- **5195:** `NTTTTNTE TEAETOTTNEAIT TAN`, as at HEAD.
- **5209:** `NE T E I EEEEI E E EI E T IE N E EI`. It prints under every variant measured, the key-up rule included, and is silent only under the shadow rule that was not built.
- The other 18 seeds print nothing. Off, only 5195 prints.

**The survey's summary,** before (as shipped, off) and with the handover on:

| group | off | on |
|---|---|---|
| synthetic fixtures | 46 of 67, 44 | 46 of 67, 44 |
| receiver easy | 74 of 90, 67 | 78 of 90, **75** |
| receiver working | 5 of 81, -4 | 4 of 81, -4 |
| receiver edge | 0 of 81, 0 | 0 of 81, 0 |
| receiver ungraded | 49 of 98, 44 | 77 of 98, **63** |
| CQs at 15 dB | 73 of 84, 62 | 78 of 84, **72** |
| CQs at 5 dB | 0 of 84, 0 | 62 of 84, **46** |
| CQs at 0 dB | 0 of 84, 0 | 0 of 84, 0 |
| twelve-plus-three | 318 of 353, **297** | 317 of 353, **297** |
| W1AW | 2131 of 2151, **2111** | 2131 of 2151, **2111** |
| strays on the 65 old captures, per 10 s summed | 21.6 | 20.9 |

**Both scoreboards' rows:**

| unit | right | wrong | invented | score | printed in silence | spaces |
|---|---|---|---|---|---|---|
| HEAD | 318 of 353 | 20 | 1 | **297** | 0 | 83 of 110, 3 added |
| handover on, as it was (not shipped) | 316 of 353 | 25 | 1 | 290 | 0 | 85 of 110, 4 added |
| on, the wait alone (not shipped) | 316 of 353 | 21 | 1 | 294 | 0 | 87 of 110, 3 added |
| on, the takeback alone (not shipped) | 318 of 353 | 20 | 2 | 296 | 0 | 85 of 110, 3 added |
| on, both, and the carrier check (not shipped) | 317 of 353 | 19 | 1 | 297 | 0 | 86 of 110, 3 added |
| on, with a shadow rule (not built) | 313 of 353 | 21 | 1 | 291 | 0 | 81 of 110, 5 added |
| 570 (handover off) | 318 of 353 | 20 | 1 | **297** (234 without) | 0 | 83 of 110, 3 added |

| w1aw | score | right | wrong | invented | spaces |
|---|---|---|---|---|---|
| HEAD | **2111** | 2131 of 2151 | 20 | 0 | 438 of 440, 11 added |
| every variant on | 2111 | 2131 of 2151 | 20 | 0 | 438 of 440, 11 added |
| 570 (handover off) | **2111** | 2131 of 2151 | 20 | 0 | 438 of 440, 11 added |

## 4. What's blocking us

- **The carrier at seed 5209** is the one condition the handover fails.
  - It prints through a lane 75 Hz from a station the grid already holds, reading that station's keying edges.
  - The obvious shadow rule costs 6 points and a 5 dB CQ, so a better one needs a ruling first.
- **The working fixtures** read -4 on and off. On at least two of them the grid stands, so the staged rule leaves them to it.
- **The fading fixtures** read as before, `fading-18wpm` at 8.
- **0 dB:** the lanes call nothing at 12 dB of lane contrast, and the four 0 dB CQs read nothing on or off.
- **`two-station`** reads `FQ CG DE N■ TLL K ■CALL DE W1XYZ K`, 20 of 27, with the handover on, and nothing off.
- **Reds:** `TheRsidDetectorTests.TheDetectorKeepsUpWithRealTime` is still red on the engine line, and the two reply tests (`TAW`, `IAN`) are red as at HEAD.

**Ask, this unit: what to do about the carrier at seed 5209.** Proposed ruling: the next unit judges a lane against the grid's own stations by when it keys, so a lane is not handed on where most of its marks start with a stronger station's more than 50 Hz away.

| option | for | against |
|---|---|---|
| A. A shadow rule judged over the lane's whole sequence, not mark by mark (proposed) | Targets the cause measured here, and leaves a lane alone when it shares only a few starts by chance | Not yet measured; the per-mark form cost 6 points |
| B. Ship the handover on now, carrier at 2 of 20 | The 5 dB CQs and `two-station` read today, and the boards hold | Breaks the owner's third hard limit at a second seed |
| C. Widen the "same station" distance from 50 Hz to 75 Hz or more | One number | A figure chosen to fit one seed; a louder carrier leaks further |

The industry answer is A: a shadow is a property of timing, not distance, and judging the whole sequence is what the pattern gate already does for the grid.

### Asks still outstanding

- **Unit 570, 2026-10-12:** what to do about the carrier at seed 5209 (above). Waiting on the owner. The change sits in the tree behind `CwEnvelopeDetector.LaneHandover`, off.
- **Unit 562, 2026-10-08:** what next for the junk before a weak call, given the 0.4 start costs the start of every real call. Waiting on the owner. No change for it sits in the tree.
- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by unit 524; still open before a sender stands.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.

Unit 569's ask (ship the handover at its cost, or narrow it first) was answered by work instruction 570, which narrowed it, and is dropped.
