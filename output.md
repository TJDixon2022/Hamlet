```
UNIT: 524 - partial (the rule and cases 1 to 4 done; case 5 and the carrier's first marks open; the switch stays off) - 2026-10-01
UNIT GOAL: a sequence stands only on its shape, the last five cases, and the switch
NUMBER: with shape-first on, 2 cases still read worse than today's path, down from 5, so shape-first stays an experiment at the tag
```

## 1. What Claude did

Claude Code on the development machine, branch `main`. The prompt claimed `PROJECT: Hamlet`, and the tree confirms it:
- `WORK_INSTRUCTIONS.md` opens with work instruction 524;
- `SHACK_FACTS.md` and `CwSequenceShape.cs` exist;
- the root is `C:\Source\HamLet`.

Nothing in this report is evidence about the radio.

**How the session ran:**
- It took SESSION.lock through `tools\arbiter\lock.bat take` and released it at the end.
- It wrote nothing to `RUN_LEDGER.md`, touched nothing under `tools\arbiter\`, and ticked no box.
- It read no recording, fixture or telemetry.
- Nothing keys, transmits or writes to the radio.
- **Shape-first was measured on through a switch set only for the measurement**: a script set the default to on and back to off around each run. No commit carries it on.

**The rule: a sequence stands only on its shape** (commit `03bed1de`).
- **The gate stands a sequence only at a shape of 0.2 or better.** On its own this did not keep the random carrier out. Over its first twelve to fifteen marks the carrier scores 0.21 to 0.24 and stands. With more marks it falls to about 0.1.
- **So the reader prints a sender only while it holds the same line.**
- **A sender's shape is judged over the marks its dit and dah come from (unit 523), with those marks' own evidence.**
  - Over forty marks spanning two speeds, the per-bin speed change fell under the line and stopped after `K`.
  - With forty marks' evidence, a carrier's newest handful kept printing.
- **The low-scoring light test is re-pinned.** Its sloppy sequence no longer stands at all. The test now asserts that it never stands, never turns green and never fills past the mark.

**Case 1: the speed change's first word gap.** Green, commit `f3994cc3`.
- **Cause:** a 10 WPM letter gap kept among a 20 WPM sender's gaps put the word line at 555 ms, above the new 420 ms word gap.
- **Fix:** when the newer-speed retry fires, the letter and word gap clusters come from the gaps between runs that ended after the newer marks began.
- **Result:** reads `… K TEST DE W1AW K`.
- Gaps inside letters are not re-taken. Taking them again merged `TEST` into one letter.

**Case 2: the quiet dit inside a letter.** Green, commit `54d59aee`.
- **Cause:** the fit's height read the dit 6.012 dB under the sender's dits. The line is 6 dB and the dit was taken 6.02 dB down.
- **Fix:** the quieter mark's own wobble at its contrast (unit 479's formula) is taken off before the line.
- **Result:** reads whole, and so does 511's dah case.

**Case 3: a clean sender beside a carrier 400 Hz away.** Not whole, commit `7ea54614`.
- **Cause:** every clean mark that was found was found exactly. Every missing one was cut by a carrier mark that started or ended inside it, where the whole band's span and excess are the carrier's.
- **Fix:** each hop, a standing sender's own bin is fitted at the whole band's lengths, and a rectangle at its best end is offered as its mark.
- **At 400 Hz:** 63 of 65 marks stood (41 before), reading `E EQ CQ DE N0CALL N0CALL K`. The first C's two dahs were cut before anyone stood.
- **At 200 Hz:** reads `KDEN0CALL N0CALL K`, and the carrier no longer prints.

**Case 4: unit 519's switch case.** Covered by case 3, so it needs no commit of its own. The fist reads `CQCQDEN0CALLN0CALL`, then the clean sender takes over with `W1AWKTESTDEW1AWK`.

**Case 5: unit 519's edge case.** Red, not fixed.
- **Scores:** the clean sender 0.782 and the fist 0.152, at the end.
- **Cause:** the fist's skirt covers most of the 500 Hz passband in the bins. That lifts the median excess to about -21 dB, above the clean sender's own -26.
- **Effect:** the clean sender has no marks until 10 s. The fist qualifies alone at 4.9 s and prints from 5.2 s, until its shape falls under the line at 11.4 s. It reads `TEST DE W1AW TES ALL N0CALL K`.

**The switch stays off.** With shape-first on, two cases still read worse than the per-bin path:
- the edge case;
- the first CQ beside the 400 Hz carrier.

**Records:**
- HM-DEC-228 in `DECISIONS.md`. Headline: *A sequence stands only on its shape; shape-first stays an experiment*.
- The `CLAUDE.md` index row.
- `PHASE_OUTCOME` (both copies) has `## UNIT 524 - STEP 12`.
- `PHASE_STATUS` (both copies) names 524.
- Version 1.13.208 to 1.13.209.

**Build and app line:** build 0 warnings, 0 errors. App carry-forward 278 of 278.

## 2. What the owner should expect

- **Rebuild before you run it.**
- **What reads on your radio is the same as today.** The new front end, which reads by shape first, is still switched off.
- **With it on, it now reads:**
  - a sender who speeds up, word for word;
  - a weak dit inside a letter;
  - a fist handing over to a clean station.
- **With it on, it still reads two things worse than today's path:**
  - a clean station at the filter's edge beside a louder fist;
  - the first letter of a call beside a loud random carrier.
- **A random carrier still prints a little, either way.** With shape-first on it prints its first four letters, `NOAM`, then nothing. On today's path it still prints junk. Its first dozen marks look enough like code to pass the line before the rest show they are not. Whether it fills the gauge was not measured on its own; the low-scoring light test shows a sloppy sequence never passes the mark.
- **One change on today's path:** noise stands fewer marks. One report-only row reads worse: a bulletin through a 6 dB AGC overshoot and the filter, with the fit on.

## 3. What you should see

**The carrier cases**, shape-first on:

| carrier away | carrier marks stood | clean marks stood | reads |
|---|---|---|---|
| 100 Hz | 53 | 19 of 65 | `NOAM■` |
| 150 Hz | 42 | 30 of 65 | `NOAM K` |
| 200 Hz | 42 | 56 of 65 | `KDEN0CALL N0CALL K` |
| 400 Hz | 42 | 63 of 65 | `E EQ CQ DE N0CALL N0CALL K` |

- **Noise:** stands nothing on shape-first in 30 s or 3 min. On today's path it stands 31 marks in 3 min (80 before).

**Real senders**, shape-first on, each read whole:

| sender | stood at | shape at standing |
|---|---|---|
| 12 WPM fist | 4.65 s | 0.203 |
| 27 WPM fist, 30% | 3.83 s | 0.230 |
| clean 20 WPM | 4.05 s | 0.224 |
| 27 WPM fist that tightens | 3.89 s | 0.230 |
| 5 WPM Farnsworth | 5.54 s | 0.244 |

**The five cases**, shape-first on:

| case | unit 523 | now |
|---|---|---|
| 1. speed change | `… K TESTDE W1AW K` | `… K TEST DE W1AW K` |
| 2. quiet dit | `CQ CQ DE N0CA DL N0CALL K` | whole, dah case whole |
| 3. carrier 400 Hz away | `E EG NQ N M D D EOT N I A` | `E EQ CQ DE N0CALL N0CALL K` |
| 4. switch case | `CQ CQ DE N0CTLD ATK W1AW K …` | fist whole, then the clean sender |
| 5. edge case | `TEST DE W1AW TESTDE W1AWK LK` | `TEST DE W1AW TES ALL N0CALL K` (clean 0.782, fist 0.152) |

**The strength table:**

| dB | per-bin | shape-first |
|---|---|---|
| 24 | whole | whole |
| 16 | whole | whole |
| 12 | whole | whole |
| 8 | `CQ NIQ DE N0CALL NTJCALL K` | whole |

**Every existing case, shape-first on, still red:**
- case 5 and case 3's first CQ;
- `ALetterReadFromNoiseDoesNotReachTheScreen(blocks: True)`, red at HEAD too;
- the per-bin gates' own tests (edges, narrowness, the shape gate), which test the per-bin path.

**Every existing case, as shipped (per-bin):**
- The same three reds as HEAD: `TheCallReadsAtEveryStrength(8)`, `FarnsworthAndFastReadAtTenDecibels` and `ALetterReadFromNoiseDoesNotReachTheScreen(blocks: True)`.
- The new carrier tests are red. They set shape-first themselves.
- Every asserted reading is as at HEAD. Rows that changed:
  - the random carrier's junk;
  - the W1AW all-gates-off rows, which now read nothing;
  - an 8 dB fit-off row, `N ■NIALL` → `N0NIALL`;
  - the report-only AGC 6 dB filter bulletin, fit on: `JAMP OVER THE LA DY DOG 0123IA56789` → `EIE I SI SE IE ES E S`.

## 4. What's blocking us

1. **The switch stays off.** Two cases remain for the next unit:
   - **The edge case.** A loud sender's skirt lifts the median excess over a weak sender's own, so the weak one's marks are never offered until it stands. The blob line needs a measure of the noise that a loud sender's skirt does not move.
   - **The first marks before anyone stands.** Marks cut by a louder carrier before a sender stands are never offered. Following begins only once someone stands.
2. **A random carrier still scores the line on its first marks.** It prints `NOAM` on shape-first. The 0.2 line is the order's, and the carrier crosses it at 12 to 15 marks.
3. **One report-only per-bin row reads worse**: the AGC 6 dB filter bulletin with the fit on. The gate's line costs it. The noise sequences at 1500 to 1875 Hz no longer stand, and the 625 Hz sender stands at the same moments, so the cause is not yet traced.
4. **`DecisionLogOrderTests` gaps check** is red as at HEAD, for HM-DEC-166, 182, 189, 220 and 222. The order check passes.

### Asks still outstanding

- **Unit 522, 2026-10-01:** whether to switch shape-first on before its failures are fixed. Still off. Two cases remain (item 1).
- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by following a standing sender on its own bin. Before it stands, it is still open (item 1).
- **Unit 517, 2026-10-01:** the hand sender's word-gap line, 5 dits recommended. Waiting on the owner. The measurement is in `TheSpacesComeFromTheShapeTests`, and no change sits in the reader's word line.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
