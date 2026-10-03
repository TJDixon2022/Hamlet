```
UNIT: 531 - tasks 1 and 2 done; tasks 3 and 4 dropped - 2026-10-03
UNIT GOAL: the recording reads whole
NUMBER: every letter right; one space off, the sender's own 579 ms between C and H
```

## 1. What Claude did

Claude Code on the development machine, branch `main`. The prompt claimed `PROJECT: Hamlet`, and the order's gate held: `SHACK_FACTS.md`, `CwSenderLane.cs` and the recording exist, there is no `CoreHMI.sln` or `MURC.sln`, and the root is `C:\Source\HamLet`. Hamlet confirmed. Nothing in this report is evidence about the radio beyond the owner's one recording. HM-DEC-235 was free.

**How the session ran:**
- It took SESSION.lock and released it at the end.
- It wrote nothing to `RUN_LEDGER.md`, touched nothing under `tools\arbiter\`, and ticked no box.
- R88 stayed lifted for the one recording. Nothing keys, transmits or writes to the radio.

**Task 1: the line inside a letter sits at Morse's midpoint.** Commit `c778efd5`.
- **The change:** the line between a gap inside a letter and one between letters is now the geometric midpoint of the two clusters, each side weighed equally. That is √3 for Morse's 1:3. It applies in the sender's own lines and in the check of a gap against its neighbours.
- **What it replaces:** the spread-weighted boundary of HM-DEC-217, for these gaps only. The dit-or-dah line keeps the spread-weighted boundary.
- **Result:** the owner's 7 now reads `7V`. Every other case reads as before. Only noise readings in two random-carrier diagnostic rows moved.

**Task 2: the first seconds of a transmission.** Commit `93fb73d1`.
- **The change:** until a sender's word cluster is trusted (three word gaps), its word line is never under √21 = 4.58 of its element gaps, Morse's midpoint between a 3-unit letter gap and a 7-unit word gap.
  - The element gap is the centre of the sender's inside-letter gaps once three show, and its gap dits before that.
  - The five-dit floor stays.
- **Result:** `FER` now reads together; KC4ZGP holds; the straight key still reads `SKCC DE`.
- **What it costs:** the 27 WPM fist that tightens from 30% scatter now reads `CQCQ DE` (was `CQ CQ DE`). Its first word gap is 240 ms, 4.4 of its element gaps, under the 249 ms floor. Before, the line drawn from its two letter gaps sat at 202 ms and caught it. That is the order's rule working as written, so the report names it.
- **The recording test now asserts the whole text, spaces included.** It reads `FER C HAT<BT> BEST 7V 73 <SK> KC4ZGP DEWA`: every letter right, one space off. The sender left 579 ms between the C and the H, about 7.4 of his gap dits, longer than a 7-unit word gap. No line drawn from 1:3:7 can read that as inside a word. The assertion was not loosened, so the test stays red.

**Task 3: dropped.**
- **The cause:** the gate accepts a mark into a sequence within one bin spacing either side of its mean pitch, a window 50 Hz wide. While pitches were bin centres that took in three bins; measured to the hertz, noise marks fill the whole window.
- **The fix tried:** half a bin either side (one bin's width). Loud noise then stood nothing at all, in 30 s and in three minutes.
- **Why it was reverted:** three tests measure the single-mark gates by counting the noise that stands. With none standing their figures can't be taken, and two more of them went red.

**Task 4: dropped.** The random carrier still prints at 775 and 825 Hz.

**Records:**
- HM-DEC-235 in `DECISIONS.md` (dated 2026-10-03, as the session crossed midnight), naming what it supersedes.
- The `CLAUDE.md` row.
- `PHASE_OUTCOME` (both copies) has `## UNIT 531 - STEP 12`.
- `PHASE_STATUS` (both copies) names 531.
- Version 1.13.215 to 1.13.216.

**Build and app line:** build 0 warnings, 0 errors. App carry-forward 278 of 278.

## 2. What the owner should expect

- **Rebuild.**
- **Your recording now reads:**
  - sent: `FER CHAT<BT> BEST 7V 73 <SK> KC4ZGP DEWA`
  - now: `FER C HAT<BT> BEST 7V 73 <SK> KC4ZGP DEWA`
- **Every letter is right.** The 7 is a 7, and FER is one word.
- **The one space Hamlet adds, after the C, is in your sender's own timing.** He paused 579 ms there, longer than he leaves between some of his words.
- **A station tuned into mid-sentence is now spaced from its first letters by Morse's own proportions,** not by a guess drawn from one or two gaps. The cost: a very rough fist whose first word gap is sent short can run its first two words together.

## 3. What you should see

**The recording's text:**

| | text |
|---|---|
| sent | `FER CHAT<BT> BEST 7V 73 <SK> KC4ZGP DEWA` |
| before 531 | `F ER C H AT<BT> BEST MSV 73 <SK> KC4ZGP DEWA` |
| after task 1 | `F ER C H AT<BT> BEST 7V 73 <SK> KC4ZGP DEWA` |
| now | `FER C HAT<BT> BEST 7V 73 <SK> KC4ZGP DEWA` |

**Task 1, the 7:** its inside gap is 120 ms. The line sat at 100–114 ms; it now sits at the midpoint of the sender's 74 ms and roughly 300 ms clusters.

**Task 2:**

| case | before | now |
|---|---|---|
| the recording's start | `F ER C H AT` | `FER C HAT` |
| straight key, six conditions | `CQ CQ SKCC DE N0CALL N0CALLK` | same |
| 4-dit word gaps | `KI1MMDEVE2JDNAMEISJEANQTHQUEBECHW` | same |
| Farnsworth 5 WPM, six conditions | `TEXT IS FROM SEPTEMBER 2024` | same |
| 27 WPM fist, 30% then 10% | `CQ CQ DE N0CALL N0CALL K` | `CQCQ DE N0CALL N0CALL K` |

**Task 3 (reverted):** with half a bin either side, noise stood 0 marks in 30 s (44 now) and 0 in three minutes (12 now). Then `MostNoiseBarsHaveNoEdges` read 0 against 0, and `RealMarksScoreInsideTheShapeAndNoiseOutside` and `TheShapeTurnsAwayNoiseThatPassedFiveLines` had nothing to measure.

**The existing cases:**
- Sets a, b1, b3, b4 and b5 read as at HEAD, except the 27 WPM fist above and two random-carrier diagnostic rows whose noise reading moved.
- Noise prints nothing.
- The strength table through the filter reads `CTU NIG DE N0CALL N0CALL K` at 8 dB and `CTU CQ DE N0CALL N0CALL K` at 10 dB, and whole at 12, 16 and 24 dB.
- The between-bin cases read whole.
- **Reds:**
  - the 8 dB call;
  - Farnsworth and fast at 10 dB;
  - a letter from noise (blocks);
  - the random carrier at 775 and 825 Hz;
  - the clean sender at the edge;
  - the radio strength table at 8 and 10 dB;
  - `MostNoiseBarsHaveNoEdges`;
  - `AFistThatTightensIsFollowed` (new);
  - `TheOwnersRecordingReads` (the C HAT space).

## 4. What's blocking us

1. **`C HAT`:** the sender's 579 ms is a word gap by 1:3:7. Reading it as one word would take knowing English, not Morse timing.
2. **The 27 WPM fist that tightens reads `CQCQ`:** its first word gap, sent at 4.4 element gaps, falls under Morse's midpoint.
3. **Noise standing:**
   - The gate's pitch agreement spans two bins.
   - At one bin, noise stands nothing, but three tests that count standing noise to measure a single-mark gate then have nothing to count.
   - Ruling asked: *the gate agrees a mark with its sequence within half a bin of its pitch, and the three tests count candidates rather than marks that stood.* Rejected: leaving the agreement as it is, which stands 44 noise marks in 30 s.
4. **The random carrier prints at 775 and 825 Hz** (task 4 not worked).
5. **Pre-existing app reds** outside the line are untouched: the scope's tone-line tests, two `centre` spellings, `DecisionLogOrderTests`' gaps check.

### Asks still outstanding

- **Unit 531, 2026-10-03:** the gate's pitch agreement and the noise-count tests (item 3 above). No change for it sits in the tree.
- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by unit 524; still open before a sender stands.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
