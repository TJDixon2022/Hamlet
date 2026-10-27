## 1. What Claude did

Surface: Claude Code on the development computer, branch `main`. The gate: the prompt said `PROJECT: Hamlet`; `SHACK_FACTS.md`, `docs\cw-scoreboard.md` and `src\Hamlet.RadioEngine\Cw\CwSequenceReader.cs` exist, `CoreHMI.sln` and `MURC.sln` do not, the root is `C:\Source\HamLet`. Hamlet confirmed. Nothing in this report is evidence about the radio.

Hand run:
- `SESSION.lock` was taken through `tools\arbiter\lock.bat take` and released at the end.
- Nothing was written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` was touched, and no box was ticked in `PHASE_PLAN.md`.
- Nothing keyed or transmitted, nothing was written to the radio, and no recording was added, removed or promoted.
- The tag `before-sequence-reader` stands. Version 1.13.276 to 1.13.277. HM-DEC-295 was free and is recorded. Nothing was recorded under §12.1.

**The traces, first.**
- **A watch on one lane.** The reader gained a watch (`WatchHz`). For one lane, each decode logs its window, where the 2 s commit lag stood, whether it read cold or warm, its levels and rhythm, and the letters its path laid with the second each starts. With a watch set, only that lane's own lines are traced.
- **`TheOpeningTraced`.** It sets that beside the station's keyed marks from the keying map, every letter committed, and the lane's level at each width every 50 ms. It ran on the first recording, `121414`, `144020`'s second station, and W1AW pieces 02 and 04.
- **`TheSilencesTraced`.** For each letter printed in a silence, it gives the lane and its rhythm, levels and window pattern, and every keyed mark near it.

**Round one** (`046d5c2b`):
- **The opening.** It is the window's left edge. A decode may open only on a mark, a word gap or nobody keying, and the last two need a word gap or more. So a station keying within a word gap of the window's start cannot have his first mark laid. Built: the quiet a window opens on is nobody keying of any length.
- **The silences.** Three of four were real letters the lane's filter drew 20 to 30 ms late. Built: a committed mark is timed less the lane's delay, 2.6131/(2π × width).
- Measured: 300, 2101, the bank 3944.

**Round two** (`6b47b1ac`):
- **The trace.** `121414` was the largest loss. From 11.8 s the sender's dah shortened from about 220 ms to 155 ms. The kept rhythm read on and printed `BK TU RON` as `HS T U RSI`, while the cold read saw dahs the warm read called dits (dah share 0.37 and 0.43 against 0.22 and 0.27).
- **The rule.** A warm lane reads every window cold as well. It takes the cold read where both are Morse, they disagree on the dah share by more than `DahShareDisagree` 0.1, and the cold read lays no more unreadable letters.
- **The tie-break.** It came from W1AW piece-03 at 256 s, where he slows for `<BT> QST DE W1AW` and the cold read was the wrong one. Without it that stretch read 1 of 11.
- Measured: 308, 2101, the bank 3898.

**No round three.** The largest loss left against shipped is W1AW piece-04, at −8. It is scattered single marks lost at 30 WPM in the 40 Hz lane, with no one cause. The rest are the first recording's `ER` merge and `144020`'s spaces, each where a hand's letter gap sits against the line the reader learns, and `221502`'s marks in a pause. None is a rule the trace names, so a third round would have been a guess. The order allows two rounds after the first at most.

**Builds and runs.**
- Each round was built once with `SequenceReader` out of `OffByDefault`. That change was never committed and was restored with `git checkout`.
- The build servers were shut down after each build. Then, one at a time: the survey, the guard line, CPU on piece-04, and in round two the board's noise lines and the first proof.
- The record build was the solution whole with the rule off. The app line, the engine line (1500 s) and the survey line then ran once each with `--no-build`.

**Recorded**, not a ruling: HM-DEC-295 in `DECISIONS.md`, quoting the owner's *concur*, and its `CLAUDE.md` §1 row; `docs\cw-scoreboard.md` 591 rows; `PHASE_OUTCOME.md`, both copies, `## UNIT 591 - STEP 12`; `PHASE_STATUS.md`, both copies, naming 591; `Directory.Build.props`; the survey's `Rerun` line in `TheCwSurveyTests` and the doc.
**The owner's ruling on HM-DEC-295, 2026-10-27: ship `CwRules.SequenceReader` on by default** (hand run, one commit, version 1.13.277 to 1.13.278). The prompt carried `PROJECT: Hamlet`; the tree is unit 591's. `SESSION.lock` was taken through `tools\arbiter\lock.bat take` and released. Nothing was written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` touched, no box ticked, nothing keyed or written to the radio, and no recording added, removed or promoted.
- **The rule:** `SequenceReader` comes out of `CwRules.OffByDefault`. With it on the reader is the only path to the printer; the grid's marks, the bank's handover and the column finder feed nothing, and the grid stays for the scope, as built.
- **The boards' guards** read rows labelled `591 ruled`: the twelve-plus-three at 308 (325 right, 13 wrong, 4 invented, 92 of 110 spaces, 13 added) and W1AW at 2101 (2125 right, 24 wrong, 0 invented), wrong and invented now held not above the row as well as the score.
- **The hard limits, restated** in `TheRecordingsScoreboardTests`, the board guard and the scoreboard's guard paragraph:
  - the first recording is guarded as it reads, `FF C HAT<BT> BEST 7V 73 <SK> KC4ZGP ■`, with the `ER` merge at 151 ms a known loss and `DEWA` one unreadable letter;
  - at most one letter in a silence, the one on `221502`;
  - loud noise and the steady carrier print nothing;
  - the random carrier is reported, not a condition.
- **New guards:**
  - the first proof on the engine line;
  - `TheFistBankGuardsTests.TheBankHolds`, every dah row at 990, 1182, 1143 and 1146 less 5% and the fading half at 1771 less 5%, replacing the 1.6 and 2.0 rows' guard;
  - the 5 dB CQs at 83 of 84;
  - `TheSequenceReaderShipsTests`, the reader on by default, `two-station` 27 of 27, the easy fixtures 90 of 90;
  - the nine hands at 48, as before.
  - `docs\carry-forward-tests.txt`'s engine line now names them all, at 1500 s.
- **The sheet and the screen:**
  - the capture sheet's `senders` line, where the reader reads, says the station it prints: `675 Hz, a 15 Hz lane, dit 80 ms, separation 2.16, printed  (the sequence reader)`, or `none printed`; `TheSendersLineNamesTheLaneTests` holds the wording;
  - the `Weak stations` row keeps its place, and its hover and the sheet's `weak` line say it no longer changes what prints.
- **The records:** HM-DEC-295 carries the ruling, the restated limits and the reason, and its `CLAUDE.md` row says so. `CW_SPEC.md` §2's finder history is replaced by one paragraph, the reader finds the sequence. The survey was rerun with a new `Rerun` line.
- One build after a doc-comment fix, the build servers shut down, then the lines one at a time with `--no-build`.


## 2. What the owner should expect

**The whole picture: the sequence reader against two finders, at the end of round two with it on.**

| | two finders (shipped) | sequence reader |
|---|---|---|
| W1AW | 2111 (2131 right, 20 wrong) | 2101 (2125 right, 24 wrong, 0 invented) |
| the hand-sent recordings, the twelve-plus-three | 297 (318 right, 20 wrong, 1 invented) | **308** (325 right, 13 wrong, 4 invented) |
| spaces on the twelve-plus-three | 86 of 110, 3 added | 92 of 110, 13 added |
| the first recording | whole | `FF C HAT<BT> BEST 7V 73 <SK> KC4ZGP ■` |
| a short-dah fist: the bank's 1.6 and 2.0 rows | 1193 of 2880 | 2172 |
| the fist bank | 1891 (2612 right, 46 invented) | 3898 (4461 right, 18 invented) |
| a weak CQ at 5 dB: the four 5 dB CQs | 71 of 84 | 84 of 84 |
| a weak CQ at 0 dB | 0 of 84 | 5 of 84 |
| a fading station: the fading half, letters right | 439 of 2880 | 1771 |
| the flat half, letters right | 2173 | 2690 |
| the far side: `two-station` | 18 of 27 | 27 of 27 |
| the nine straight-key hands at 48 of 60 | 9 | 9 |
| the easy / working / edge fixtures | 77 / 4 / 0 | 90 / 36 / 45 |
| letters in silences | 0 | 1 |
| loud noise, the steady carrier | nothing | nothing |
| the random carrier | 2 seeds of 20 | 20 seeds of 20 |
| strays summed over the 65 old captures, highest | 26.8, 3.3 | 46.5, 4.3 |
| CPU on W1AW `piece-04`, 283 s of audio | 183 s | 106 s |

**It does not ship.** `SequenceReader` stays off. For the first time the reader beats two finders on the owner's hand-sent recordings, 308 against 297, with fewer wrong letters. What fails the bar:
- **Invented letters** rise from 1 to 4.
- **Spaces added** rise from 3 to 13.
- **The first recording** reads `FF` for `FER`, two letters, so the one-letter ruling case doesn't cover it.
- **W1AW** is at 2101, inside the 1 per cent band, but its wrong letters rise from 20 to 24.

The one letter in silences would sit inside the ruling case on its own. But that case needs everything else to hold, and here several things fail together, so there is no *For the owner's ruling* heading.

What holds:
- the bank's score (3898) and invented letters (18, under 20);
- both halves, and strong flat hands within three letters of shipped;
- the first proof, the weak end, the nine hands and the easy fixtures;
- noise, the steady carrier and the strays line;
- CPU.

**The opening** was the window's left edge. A station who starts keying within a word gap of the window's start lost his first mark: the first recording's `F` read `R`, and W1AW's first `T` was lost. It is fixed.

**The silences** were mostly real letters drawn 20 to 30 ms late by the lane's filter. Three of the four are fixed. The fourth is two marks laid in a pause on `221502`.

**What the boards still lose** is neither the opening nor the silences. It is where a hand's letter gaps sit against the line the reader draws between its gap kinds:
- the first recording's `E` and `R` are 151 ms apart and merge into `F`;
- `144020`'s second hand spaces his letters 400 to 800 ms apart and gets spaces inside words.

Beyond that, single marks are lost on W1AW's 30 WPM piece, and four invented letters are untraced. A fifth thing.

As shipped, nothing changes: 297 and 2111. The app line 278 of 278. The engine line 159 of 159 in 11.1 minutes. The survey line 1 of 1 in 10.3 minutes, every group as before (297, 2111, the bank 1891), only its `Rerun` line changed. Pushed to `main`: rounds one and two and the record.

**Shipped on, 2026-10-27, the owner's ruling on HM-DEC-295.** Hamlet now reads CW with the sequence reader by default. What the lines read as shipped:
- **The app line:** 278 of 278.
- **The engine line:** 163 of 163 in 13.2 minutes. Every new guard passed: the reader on by default, the first proof, the bank's rows and fading half, the 5 dB CQs at 83 or better, `two-station` 27 of 27, the easy fixtures 90 of 90, the nine hands, and both boards at 308 and 2101.
- **The survey line:** 1 of 1 in 15.7 minutes, every group as round two read it:
  - the twelve-plus-three 308 (325 right, 13 wrong, 4 invented);
  - W1AW 2101 (2125 right, 24 wrong, 0 invented);
  - the fist bank 3898 (4461 right, 18 invented), the flat half 2690 right, the fading half 1771;
  - the 15 and 5 dB CQs 84 of 84, the 0 dB 5;
  - the easy fixtures 90 of 90, the working 36 of 81, the edge 45 of 81;
  - strays 46.5 summed, the highest 4.3.

What you will see on the radio:
- **Weak, fading and short-dah stations read where they did not.**
- **The first recording reads `FF C HAT`** where it read `FER C HAT`.
- **A carrier keyed at random can print**, at every seed of the test.
- **The `Weak stations` choice no longer does anything.**

## 3. What you should see

**The opening trace** (`TheOpeningTraced`; `.run-unit\reports\unit591-opening.txt`, before round one):

| opening | the station keys first | what the window and path did | printed | cause |
|---|---|---|---|---|
| the first recording, 663 Hz (lane 675) | 0.16 s | first decode at 6 s, cold, window 0 to 6 s; path `.-.@0.38 ..-.@1.14`: the first dit at 0.16 never laid, then `E` and `R` merged | `R F C HA T` | the window's left edge: 0.16 s of quiet before him, under a word gap, so no state could open on it; then a short letter gap |
| W1AW piece-02, 600 Hz | 0.09 s (a dah) | path `--..--@0.61 .-@2.69`: the `T` at 0.09 never laid | `, A T D O` for `T, AND O` | the window's left edge, 0.09 s in |
| W1AW piece-04, 600 Hz | 0.00 s | 24 Hz cold read; path `--@0.23`: the `W`'s dit at 0.00, inside the filter's start, never laid | `M O A` for `W O A` | the window's left edge, at the file's start |
| `121414`, 800 Hz | 0.00 s | cold read 15 Hz, dit 70 dah 210; path `.-.@0.42 .....@1.40 .....@2.25 -.@3.10` | `R 5 5 N` | none: the opening reads right; its loss is a speed change at 11.8 s (round two) |
| `144020`'s second station, 600 Hz | 13.57 s | the 600 Hz lane not yet Morse; the 625 Hz lane printed `W@13.58 X@14.52` | `WX` | lane choice, not the window |

After round two the same traces read:
- the first recording `F@0.17 F@1.11 C@2.80 H@4.18 A@5.00 T@5.76`;
- W1AW piece-02's stretch 38 of 39.

**The silences trace** (`TheSilencesTraced`, before round one):

| letter | lane | the truth near it | cause | after round one |
|---|---|---|---|---|
| `<BT>` at 6.43 to 7.38 s, first recording, silence from 7.35 s | 675 Hz, 15 Hz lane, separation 2.16, Morse | his last dah 7.13 to 7.35 s | a real letter, its last mark ended 30 ms late (the 15 Hz lane's 28 ms delay) | gone |
| `E` at 7.91 to 7.96 s, `143906`, silence from 7.94 s | 525 Hz, 24 Hz lane | his dit 7.89 to 7.94 s | real, 20 ms late (17 ms at 24 Hz) | gone |
| `E` at 24.50 to 24.56 s, `143906`, silence from 24.54 s | 525 Hz, 40 Hz lane | his dit 24.49 to 24.54 s | real, 20 ms late | gone |
| `■` (`..-....`) at 20.96 to 21.79 s, `221502`, silence from 21.45 s | 500 Hz, 24 Hz lane, separation 1.67, Morse | his last mark 21.40 to 21.45 s | two marks laid 340 ms into the pause | stays |

**`121414`, round two's trace** (the warm and cold figures for the 800 Hz lane):

| decode | warm read | cold read | before round two | after |
|---|---|---|---|---|
| 14 s | dah share 0.22, dah 210 ms | 0.22, dah 210 ms | keeps its rhythm | keeps |
| 16 s | 0.22 | **0.37, dah 145 ms** | keeps its rhythm | takes the cold read |
| 18 s | 0.27 | **0.43, dah 145 ms** | keeps its rhythm | takes the cold read |
| 20 s | 0.40 | 0.40, dah 145 ms | keeps | keeps |

The stretch read `BK HSTURSI` and now reads 32 of 32.

**The rounds, with the rule on:**

| | shipped | 590's end | round 1 | round 2 |
|---|---|---|---|---|
| twelve-plus-three (right, wrong, invented) | 297 (318, 20, 1) | 293 (316, 20, 3) | 300 (321, 17, 4) | 308 (325, 13, 4) |
| spaces right, added | 86, 3 | 89, 9 | 92, 13 | 92, 13 |
| W1AW (right, wrong, invented) | 2111 (2131, 20, 0) | 2104 (2126, 22, 0) | 2101 (2125, 24, 0) | 2101 (2125, 24, 0) |
| W1AW pieces 01 / 02 / 03 / 04 | 242 / 388 / 793 / 688 | 242 / 386 / 790 / 686 | 242 / 387 / 792 / 680 | 242 / 387 / 792 / 680 |
| fist bank (right, wrong, invented) | 1891 (2612, 675, 46) | 3925 (4475, 532, 18) | 3944 (4481, 519, 18) | 3898 (4461, 545, 18) |
| flat half, right / score | 2173 / 1896 | 2712 / 2574 | 2714 / 2576 | 2690 / 2525 |
| fading half, right / score | 439 / -5 | 1763 / 1351 | 1767 / 1368 | 1771 / 1373 |
| short-dah rows | 1193 | 2180 | 2192 | 2172 |
| 15 / 5 / 0 dB CQs of 84 | 78 / 71 / 0 | 84 / 84 / 5 | 84 / 84 / 5 | 84 / 84 / 5 |
| nothing after the weak CQs | holds | holds | holds | holds |
| easy / working / edge fixtures | 77 / 4 / 0 | 90 / 35 / 42 | 90 / 36 / 45 | 90 / 36 / 45 |
| synthetic fixtures (invented) | 46 of 67 (0) | 51 (3) | 51 (3) | 51 (3) |
| `two-station` | 18 of 27 | 27 | 27 | 27 |
| letters in silences | 0 | 4 | 1 | 1 |
| first recording | whole | `RF C HA T` | `FF C HAT` | `FF C HAT` |
| random carrier, seeds of 20 | 2 | 20 | - | 20 |
| loud noise, steady carrier | nothing | nothing | - | nothing |
| strays summed, highest | 26.8, 3.3 | 48.8, 5.3 | 46.5, 4.3 | 46.5, 4.3 |
| nine hands at 48 | 9 | 9 | 9 | 9 |
| short-dah rows' guard | holds | holds | holds | holds |
| first proof | - | holds | - | holds |
| CPU, reader / two finders | - | 100 / 185 s | 101 / 185 s | 106 / 183 s |

**The three matrices, round two with the rule on** (shipped in brackets):

Dah over dit by speed, flat, 20 dB:

| | 10 WPM | 15 WPM | 20 WPM | 25 WPM | row |
|---|---|---|---|---|---|
| dah 1.6 | 53 (60) | 50 (60) | 50 (60) | 54 (60) | 207 (240) |
| dah 2.0 | 52 (59) | 60 (60) | 57 (60) | 60 (22) | 229 (201) |
| dah 2.5 | 59 (59) | 60 (59) | 58 (60) | 60 (60) | 237 (238) |
| dah 3.0 | 60 (60) | 58 (60) | 58 (60) | 60 (60) | 236 (240) |

Dah over dit by signal, flat, 20 WPM:

| | 5 dB | 10 dB | 20 dB | row |
|---|---|---|---|---|
| dah 1.6 | 51 (45) | 49 (36) | 50 (60) | 150 (141) |
| dah 2.0 | 55 (43) | 58 (59) | 57 (60) | 170 (162) |
| dah 2.5 | 59 (53) | 60 (59) | 58 (60) | 177 (172) |
| dah 3.0 | 60 (16) | 54 (59) | 58 (60) | 172 (135) |

Signal by fade, dah 2.5, 20 WPM:

| | flat | 12 dB of QSB at 0.7 Hz | row |
|---|---|---|---|
| 5 dB | 59 (53) | 19 (0) | 78 (53) |
| 10 dB | 60 (59) | 49 (14) | 109 (73) |
| 20 dB | 58 (60) | 31 (13) | 89 (73) |

**The fixtures, round two with the rule on:**

| group | read | wrong | invented | score |
|---|---|---|---|---|
| synthetic fixtures | 51 of 67 | 6 | 3 | 42 |
| receiver: easy | 90 of 90 | 0 | 0 | 90 |
| receiver: working | 36 of 81 | 17 | 0 | 19 |
| receiver: edge | 45 of 81 | 9 | 1 | 35 |
| receiver: ungraded | 72 of 98 | 3 | 24 | 45 |
| CQs 15 / 5 / 0 dB | 84 / 84 / 5 of 84 | 0 / 0 / 3 | 0 | 84 / 84 / 2 |

**The first proof, round two:**
- the 18 WPM ragchew at 10, 5 and 0 dB flat: 60 of 60 at every seed;
- 10 dB fading: 60 at every seed;
- 5 dB fading: 60, 59 and 59;
- the 12 WPM fist with a dah of 1.7: 60, 60 and 59;
- the band alone: 288 to 299 marks, not Morse, nothing printed.

**Both scoreboards' rows**, as shipped:
- twelve-plus-three: `| 591 | 2026-10-27 | 318 of 353 | 20 | 1 | **297** | 0 | 86 of 110, 3 added | ... two rounds, 308 with SequenceReader on (300, 308), left off; the board unchanged |`
- W1AW: `| 591 | 2026-10-27 | **2111** | 2131 of 2151 | 20 | 0 | 438 of 440 | ... 2101 with SequenceReader on (2101, 2101), left off; unchanged |`

## 4. What's blocking us

**Left, one line each:**
- **A hand's letter gaps against the reader's gap lines.** The first recording's `E` and `R` sit 151 ms apart against a learned letter gap of 290 to 340 ms and merge into `F`. `144020`'s second hand spaces his letters 400 to 800 ms apart against a word gap near 500 and gets 7 spaces inside words.
- **Invented letters, 4:** `143906` at 2.0 s, `221502` at 25.7 s, and `121324` at 11.6 and 14.5 s. Not traced.
- **One letter in a silence:** `221502`, two marks laid 340 ms into a pause. Not traced.
- **W1AW piece-04** loses single marks at 30 WPM in the 40 Hz lane, 4 more wrong than shipped, with no one cause.
- **The random carrier** prints at every seed. Whether that limit stands is for the owner's ruling.
- **Listed for removal in a later unit, now that the reader is the only path to the printer** (the owner's ruling on HM-DEC-295):
  - **The mark finders:** the grid's mark path into the gates, kept for the scope only (`CwEnvelopeDetector`'s grid marks), and the lane bank's level-rule marks (`CwLaneBank`, `CwSenderLane`).
  - **The bank's handover** (`CwRules.LaneHandover`, `CwEnvelopeDetector.LaneHandover`, `HandoverHz`, `WeakStationPitchHz`, the `Weak stations` row and its `WeakStations` setting with a migration, the `Weak station at` story line).
  - **The column finder** (`CwColumnFinder`, `CwRules.Columns`).
  - **The seam's rules:** the shadow rule (`CwShadow`, `ShadowGrid`), the pattern and sender gates' level and shape rules (`CwPatternGate`, `CwSenderGate`, `CwRunReader` where only the old path calls it), and the rules in `CwRules.OffByDefault` and those on by default that only the old path reads: `LaneBank`, `HeldLaneReads`, `HeldLanePause`, `SureFirst`, `BankFinds`, `LoudestFirst`, `LaneMeetsTheGrid`, `TwoLanes`, `LaneHearsTheOpening`, `GridStepsAside`, `HoldWaitsAndFits`, `FollowsTheFade`, `HeldLaneMarksAreItsStations`, `ShortDah`, `BetweenClusters`, `StraddleSplit` and the gates' earlier rules.
  - Each to go with the tests that read only it, and the boards held through each removal.
- **`VoiceTests.NoOperatorFacingStringUsesABritishSpelling` is red,** on `centre` in two strings in `CwSenderGate.cs` (lines 331 and 334), last changed by unit 588. It is not on the app line, was not touched here, and goes with `CwSenderGate`'s removal or a one-word fix.

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
