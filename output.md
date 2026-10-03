```
UNIT: 530 - tasks 1 and 2 done; task 3 not built (premise false); task 4 dropped - 2026-10-02
UNIT GOAL: one front end, and the last four faults on the owner's recording
NUMBER: one front end, the grid; the recording reads every letter but the 7 before V
```

## 1. What Claude did

Claude Code on the development machine, branch `main`. The prompt claimed `PROJECT: Hamlet`, and the order's gate held: `SHACK_FACTS.md`, `CwSenderLane.cs` and `tests\fixtures\cw\captured\cw-2026-10-02-200157.wav` exist, there is no `CoreHMI.sln` or `MURC.sln`, and the root is `C:\Source\HamLet`. Hamlet confirmed. Nothing in this report is evidence about the radio beyond the owner's one recording. HM-DEC-234 was free.

**How the session ran:**
- It took SESSION.lock and released it at the end.
- It wrote nothing to `RUN_LEDGER.md`, touched nothing under `tools\arbiter\`, and ticked no box.
- **HEAD was tagged `before-one-front-end` (053b8fb6) and pushed** before any engine change.
- R88 stayed lifted for the one recording. Nothing keys, transmits or writes to the radio.

**Task 1: one front end, pitch as a result.** Commit `23937fc7`.
- **Pitch:** a grid mark's pitch is now its energy's centroid over its own samples: the strongest excess peak within two bins of the bin its bar peaked in. The walk to the louder neighbour still picks the bin a mark's level is read in, but no longer names its pitch.
- **The comparison:** both paths were run with the sender's window on (it was hooked into shape-first for this).
  - **Shape-first read the owner's recording as nothing.** It joined this hand's dits across their 75 ms gaps into 120 ms spans, so the sender's shape scored nought and nothing printed.
  - It did read the radio strength table whole at 8 and 10 dB, which the grid doesn't.
  - **The deciding case was the recording, so shape-first came out.** The grid is the only front end, and the tag holds shape-first.
- **Tests:**
  - Five tests that only compared shape-first with the grid were removed with it.
  - Two moved onto the grid: a random carrier never prints, and a real sender stands.
- **Results:** every reading case reads as at HEAD. One new red, `MostNoiseBarsHaveNoEdges`:
  - 30 s of loud noise now stands 44 marks with the edge test on (30 before) and none with it off (38 before).
  - Three minutes of noise stands 12 (31 before).
  - Nothing prints in either. The centroid spreads noise marks over continuous pitches, so the gate groups them differently.

**Task 2: a sender is held to its last word.** Commit `153ddf45`.
- A sender's letter-gap tightness is now scored on its longer gaps under its own word line, walked as before.
- The owner's hand had all its longer gaps (2 to 5.7 dits, no jump to its words) walked as one cluster. It scored nought, and the reader let it go before DEWA.
- **Why the straight key moved:** I traced it here. Scoring the word line's letter cluster whole holds the SKCC key, its gaps scattered by two fifths, to 0.25 tightness from 8.3 s. Its shape falls under 0.1 and it's let go after `CQ CQ S`. Walking only the gaps under the line keeps its first jump where it was.
- **Green:** DEWA prints, and the straight key reads `SKCC DE` in all six conditions.

**Task 3: not built, the premise is false.**
- The letter line is already the spread-weighted boundary, the same function as the dit-or-dah line.
- It leans toward the gaps inside letters because, through the sender's window, they measure at the 0.1 spread floor (70 to 82 ms), while this hand's letter gaps are wide.
- The 7's 120 ms gap sits fewer spreads from the letter cluster, so it reads as a letter. A fix is a ruling (section 4).

**Task 4: dropped.** Its first rule (a gap under three of the sender's inside-letter gaps is inside the letter) is about 225 ms here. The owner's E and R are 170 ms apart, so they would merge into one letter. The bench helpers were not rewired either.

**Records:**
- HM-DEC-234 in `DECISIONS.md`, and the `CLAUDE.md` row.
- `PHASE_OUTCOME` (both copies) has `## UNIT 530 - STEP 12`.
- `PHASE_STATUS` (both copies) names 530.
- Version 1.13.214 to 1.13.215.

**Build and app line:** build 0 warnings, 0 errors. App carry-forward 276 of 278. The two losses are `TheChipSaysTheChosenModeTests` FT8 and Olivia, at 1 ms ("You've caused dispatcher loop"); they pass alone.

## 2. What the owner should expect

- **Rebuild.**
- **Your recording now reads:**
  - sent: `FER CHAT<BT> BEST 7V 73 <SK> KC4ZGP DEWA`
  - now: `F ER C H AT<BT> BEST MSV 73 <SK> KC4ZGP DEWA`
  - Every letter is right except the 7 before V, which still shows as M S. The spacing at the very start is still loose.
- **Hamlet has one way of finding marks now:** the per-pitch path. A mark's pitch is measured from its own sound rather than snapped to the nearest 25 Hz. The other way is kept under the tag `before-one-front-end` in case it's ever wanted.
- **A station is no longer dropped before its last word.** DEWA now prints, and the straight-key test still reads `SKCC DE`.

## 3. What you should see

**The recording's text:**

| | text |
|---|---|
| sent | `FER CHAT<BT> BEST 7V 73 <SK> KC4ZGP DEWA` |
| before 530 | `F ER C H AT<BT> BEST MSV 73 <SK> KC4ZGP` |
| now | `F ER C H AT<BT> BEST MSV 73 <SK> KC4ZGP DEWA` |

**The front-end decision, both paths with the sender's window:**

| case | grid | shape-first |
|---|---|---|
| **owner's recording** | `F ER C H AT<BT> BEST MSV 73 <SK> KC4ZGP` | **nothing** (dits joined into 120 ms spans) |
| strength table 8 dB | `CTU NIG DE N0CALL N0CALL K` | `CQ CQ DE N0CALL N0CALL K` |
| strength table 10 dB | `CTU CQ DE N0CALL N0CALL K` | `CQ CQ DE N0CALL N0CALL K` |
| the other hand cases | whole | whole |

**The strength table now**, 20 WPM through the filter, 1 dB AGC:

| dB | reads |
|---|---|
| 8 | `CTU NIG DE N0CALL N0CALL K` |
| 10 | `CTU CQ DE N0CALL N0CALL K` |
| 12, 16, 24 | whole |

The between-bin cases (612.5, 637.5, 662.5 Hz) read whole, 70 stood. Noise, 30 s and three minutes, prints nothing.

**The existing cases:**
- **Same text as at HEAD** in every set: a, b1, b3, b4 and b5.
- **Changed:**
  - Mark pitches now read to the hertz: a 612 Hz station stands at 611–612, where it stood at 600 and 625.
  - The noise stand counts, given in section 1.
  - The gate table beside a carrier, a diagnostic that reads noise: `NOAM T N I T AE E N` became `NOAM N I AEEN E`.
- **The random carrier test, now on the grid,** prints at 775 and 825 Hz (shape-first printed at 725 and 775).
- **Reds:** 8 dB call, Farnsworth/fast at 10 dB, letter from noise (blocks), random carrier at 775 and 825, clean sender at the edge, radio strength 8 and 10 dB, `MostNoiseBarsHaveNoEdges` (new), `TheOwnersRecordingReads`.

## 4. What's blocking us

1. **The 7 before V.** Ruling asked: *the line between a gap inside a letter and one between letters is the midpoint of the two clusters in log-length, each side weighed alike, rather than by spread.*
   - Why: through the sender's window the gaps inside letters are as tight as the detector reads, so weighing by spread pulls the line to 1.4 dits. A 1.6-dit gap inside this hand's 7 then reads as a letter. Morse's own 1:3 midpoint is 1.73 dits.
   - It supersedes the spread rule (HM-DEC-217) for gaps only.
   - Rejected: leaving it, which keeps `M S`.
2. **`MostNoiseBarsHaveNoEdges` is red** since the centroid pitch: 44 noise marks stand in 30 s with the edge test, 30 before. Nothing prints.
3. **`F ER C H AT` at the start.** Task 4's rule needs rewording: as written it merges letters under three inside-letter gaps.
4. **Bench helpers that don't tell the detector what it prints** (`TheLetterGapHoldsTests`, `WhichGateTurnsAwayW1awTests` and others) still test the grid without the window.
5. **The random carrier prints at 775 and 825 Hz** on the grid.
6. **Pre-existing app reds** outside the line are untouched: the scope's tone-line tests, two `centre` spellings, `DecisionLogOrderTests`' gaps check.

### Asks still outstanding

- **Unit 530, 2026-10-02:** the letter line's weighting (item 1 above). No change for it sits in the tree.
- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by unit 524; still open before a sender stands.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
