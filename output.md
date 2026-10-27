## 1. What Claude did

Surface: Claude Code on the development computer, branch `main`. The gate: the prompt said `PROJECT: Hamlet`; `SHACK_FACTS.md`, `docs\cw-scoreboard.md` and `src\Hamlet.RadioEngine\Cw\CwSequenceReader.cs` exist, `CoreHMI.sln` and `MURC.sln` do not, the root is `C:\Source\HamLet`. Hamlet confirmed. Nothing in this report is evidence about the radio.

Hand run: `SESSION.lock` was taken through `tools\arbiter\lock.bat take` and released at the end. Nothing was written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` was touched, and no box was ticked in `PHASE_PLAN.md`. Nothing keyed or transmitted, nothing was written to the radio, and no recording was added, removed or promoted. The tag `before-sequence-reader` stands. Version 1.13.274 to 1.13.275. HM-DEC-293 recorded; nothing recorded under §12.1.

The round two survey was stopped once by Claude Code's low-memory reaper. It was rerun at the owner's word ("rerun"), one test run at a time with everything else closed.

**The trace, first** (`TheReaderAgainstTheTruth`, one row per differing letter; table in section 3). It ran on the five worst stretches of the twelve-plus-three under the reader and on W1AW piece-03. It named three causes:
- **The lane was too wide for the speed.** The 16 Hz lane smeared 23 to 32 WPM gaps (key-up spread 9 to 10 dB, separation near 1.5). The lane let go and restarted cold on a crossed rhythm, with a letter gap over the word gap or a 5 ms inside gap. This falls under changes 2 and 3.
- **The printed lane never yielded.** It held while a second station keyed (`221548` at 498 Hz). This falls under change 3.
- **Marks were silenced.** The live midpoint filter dropped real marks (`121324`). This falls under change 1.

**Round one** (`ea79dcc9`) built the three changes:
- **Silence is a state.**
  - A sixth state, nobody keying: key-up level, from one word gap up with no upper bound, entered from a mark or the start and left to a mark.
  - It costs O(1) per hop, as a running maximum over the starts at least a word gap back. Its prior equals the word gap's, so a tie goes to the word gap.
  - The live midpoint and cut-letter filters are gone.
  - A one-level "nobody keying" override was tried, silenced nothing on the band alone, lost KM3STU's read, and was taken out.
- **The lane is sized to the speed.**
  - Lanes every 25 Hz, each filtered at 15, 24 and 40 Hz. `WidthFor(dit)` takes the width nearest `LaneHzPerDit` 1.5 over the dit.
  - A cold read tries every start dit (the guess, 40, 70 and 120 ms) in the 15 Hz width. Each start is scored by likelihood under the shared percentile levels less its prior cost.
  - Two first attempts failed. Scored each in its own width, or under its own learned levels, the starts that split marks won. That is why the starts share one width and one set of levels.
- **The live layer.**
  - A lane keeps its rhythm and levels for `KeepRhythmSeconds`, 20 s, after it last read Morse, and keeps them through a window that teaches nothing.
  - `ReadCold` is kept for a lane with no rhythm.
  - Hold-and-yield is gone.
  - The pattern is judged over the lane's last `PatternSeconds`, 10 s, of keying.
- The nine hands' guard now reads with the rig's passband set (`TheStraightKeyReadsTests`).
- Measured: the twelve-plus-three 210, W1AW 1882, the bank 2680, the easy fixtures 24 of 90.

**Round two** (`201ca655`). Traced on W1AW piece-02 and `cq-12wpm-15db`: a lane 25 Hz off the station took a junk rhythm in its 15 Hz cold width, where the station is 18 dB down. It then read the station at 40 Hz with every element a dah (dah share 1.00) and held the terminal, leaving the right lane as its shadow. The fix: the pattern test gains a ceiling, `DahShareMost` 0.65. Measured: 275, 1910, the bank 3376, the easy fixtures 90 of 90, both CQ rows at 15 and 5 dB 84 of 84.

**Round three** (`d3204159`). Traced on W1AW piece-02 at 237 s, where the text goes from 14 to 35 WPM. The lane read on at its kept 80 ms dit in the 15 Hz width, which smears a 35 ms dit. The decode never showed the new rhythm, so it never learned it, and 60 s printed placeholders. The fix: a warm lane whose window is not Morse reads that window cold, and takes the cold read where that is Morse. Piece-02 after 237 s read 165 of 168, from 3 and nothing. Measured: 275, 2070, the bank 3498.

**Builds and runs.**
- Each round was built once with `SequenceReader` taken out of `OffByDefault` for the measuring build. That change was never committed and was restored with `git checkout` at the end.
- After each build the build servers were shut down, then the survey, the guard line and the CPU measure ran one at a time.
- Round two's CPU run printed no figures under normal verbosity. Round three's was run with detailed output.
- The record build was the solution whole with the rule off. Then the app line, the engine line (1500 s) and the survey line ran once each with `--no-build`.

**Recorded**, not a ruling: HM-DEC-293 in `DECISIONS.md` and its `CLAUDE.md` §1 row; `docs\cw-scoreboard.md` 589 rows; `PHASE_OUTCOME.md`, both copies, `## UNIT 589 - STEP 12`; `PHASE_STATUS.md`, both copies, naming 589; `Directory.Build.props`; the survey's `Rerun` line in `TheCwSurveyTests` and the doc.

## 2. What the owner should expect

**The whole picture: the sequence reader against two finders, at the end of round three with it on.**

| | two finders (shipped) | sequence reader |
|---|---|---|
| W1AW | 2111 (2131 right, 20 wrong) | 2070 (2099 right, 29 wrong, 0 invented) |
| the hand-sent recordings, the twelve-plus-three | 297 (318 right, 20 wrong, 1 invented) | 275 (312 right, 23 wrong, 14 invented) |
| the first recording | whole | `RF C HA T <BT> BEST 7V 73 <SK> KC4ZGP ■` |
| a short-dah fist: the bank's 1.6 and 2.0 rows | 1193 of 2880 | 2044 |
| the fist bank | 1891 (2612 right, 46 invented) | 3498 (4260 right, 101 invented) |
| a weak CQ at 5 dB: the four 5 dB CQs | 71 of 84 | 84 of 84 |
| a weak CQ at 0 dB | 0 of 84 | 5 of 84 |
| a fading station: the fading half, letters right | 439 of 2880 | 1639 |
| the flat half, letters right | 2173 | 2621 |
| the far side: `two-station` | 18 of 27 | 27 of 27 |
| the easy fixtures | 77 of 90 | 90 of 90 |
| letters in silences | 0 | 4 |
| the random carrier | 2 seeds of 20 | 20 seeds of 20 |
| loud noise, the steady carrier | nothing | nothing |
| strays summed over the 65 old captures, highest | 26.8, 3.3 | 51.3, 5.3 |
| CPU on W1AW `piece-04`, 283 s of audio | 184 s | 100 s |

**It does not ship.** `SequenceReader` stays off. The bar is missed on:
- **The boards.** The twelve-plus-three is 275 against 297, and W1AW is 2070 against 2111 with wrong up from 20 to 29. Both are outside the 1 per cent band (294, 2090), so the *For the owner's ruling* case does not apply.
- **Invented letters on the bank.** 101 against a bar of 10.
- **Strong flat hands.** At 20 dB they read worse than shipped (dah 3.0 at 20 WPM: 48 against 60).
- **The band alone.** It decodes 284 to 298 marks, not silence. The pattern test refuses them, so nothing prints, but the model does not call it silence.
- **The nine straight-key hands.** Three read under 48.
- **The hard limits.** Four letters print in silences, and the first recording is not whole.

What it clears: the fist bank's score, both halves, the CQs, the easy fixtures, the short-dah rows' guard, nothing in the noise after a weak CQ, noise, the steady carrier, the strays line and CPU. The random carrier prints at every seed, reported and not a condition.

**The three changes, a sentence each:**
- A long pause is now explained as nobody keying instead of a mark laid in it.
- A lane's width follows the sender's speed: 15 Hz for a slow hand, 40 Hz for a fast one.
- A lane keeps its sender's rhythm for 20 s through a pause, and gives it up for a cold read when the rhythm stops reading him.

**What the boards still lose:**
- On the twelve-plus-three:
  - 17 on `144020`, where the second station reads 10 of 19.
  - 8 on `121414`.
  - 6 on the first recording's opening.
  - 7 invented in `221502`'s low stretch.
- On W1AW, 30 on piece-02's first 36 s, where a lane starts cold on a crossed rhythm.
- The openings fall under change 3, the cold read before a rhythm is known. Choosing between two stations is a fourth thing.

As shipped, nothing changes: 297 and 2111. The app line 278 of 278, the engine line 159 of 159. The survey line as shipped is in section 3. Pushed to `main`: rounds one to three and the record.

## 3. What you should see

**The trace, before round one**, as rows of differing letters per stretch (full rows in `.run-unit\reports\unit589-trace.txt`; 228 missing, 64 wrong, 2 added in all):

| recording, stretch | read of sent | differing rows | what the path laid there | change |
|---|---|---|---|---|
| W1AW piece-03, 0-41.8 s | 90 of 116 | 26 | merged letters after a cold restart, crossed gaps | 2, 3 |
| W1AW piece-03, 58.1-112.6 s | 62 of 159 | 97 | the lane let go; cold starts on letter gap over word gap; placeholders | 2, 3 |
| W1AW piece-03, 112.7-177.3 s | 161 of 186 | 25 | letters merged at 23 to 32 WPM in the 16 Hz lane | 2 |
| W1AW piece-03, 194.4-258 s | 71 of 168 | 97 | the lane let go and restarted cold on an inside gap of 5 ms | 2, 3 |
| W1AW piece-03, 261.4-299.9 s | 84 of 87 | 3 | single wrong kinds | - |
| `221548` 0-18.5 s at 598 Hz | 21 of 26 | 5 | wrong kinds | 2 |
| `221548` 18-30 s at 498 Hz | 0 of 18 | 18 | nothing: the printed lane at 598 Hz never yielded | 3 |
| `121324` 14.6-30 s at 800 Hz | 5 of 19 | 14 | marks silenced by the midpoint filter | 1 |
| `200157` 0-30 s | 22 of 27 | 5 | the opening split before the rhythm was learned | 3 |
| `121414` 0.3-24.1 s | 17 of 32 | 15 | marks silenced; a cold restart | 1, 3 |
| `144020` 9.5-30 s at 600 Hz | 12 of 19 | 7 | the second station read in the first's rhythm | 3 |

Piece-03 as a whole went from 567 of 815 at the trace to 802 of 815 at round three.

**The rounds, with the rule on:**

| | shipped | round 1 | round 2 | round 3 |
|---|---|---|---|---|
| twelve-plus-three (right, wrong, invented) | 297 (318, 20, 1) | 210 (271, 43, 18) | 275 (312, 23, 14) | 275 (312, 23, 14) |
| spaces | 86 of 110, 3 added | 77, 18 added | 86, 12 added | 86, 12 added |
| W1AW (right, wrong, invented) | 2111 (2131, 20, 0) | 1882 (1932, 47, 3) | 1910 (1951, 41, 0) | 2070 (2099, 29, 0) |
| W1AW pieces 01 / 02 / 03 / 04 | 242 / 388 / 793 / 688 | 242 / 172 / 790 / 678 | 242 / 200 / 790 / 678 | 242 / 358 / 790 / 680 |
| fist bank (right, wrong, invented) | 1891 (2612, 675, 46) | 2680 (3738, 994, 64) | 3376 (4172, 695, 101) | 3498 (4260, 661, 101) |
| flat half, right / score | 2173 / 1896 | 2332 / 1985 | 2619 / 2394 | 2621 / 2396 |
| fading half, right / score | 439 / -5 | 1406 / 695 | 1553 / 982 | 1639 / 1102 |
| short-dah rows (1.6 and 2.0) | 1193 | 1703 | 1953 | 2044 |
| 15 / 5 / 0 dB CQs of 84 | 78 / 71 / 0 | 63 / 71 / 5 | 84 / 84 / 5 | 84 / 84 / 5 |
| nothing after the weak CQs | holds | holds | holds | holds |
| easy / working fixtures | 77 of 90 / 4 of 81 | 24 / 17 | 90 / 17 | 90 / 17 |
| synthetic fixtures | 46 of 67, 0 invented | 22, 15 invented | 32, 25 invented | 38, 7 invented |
| `two-station` | 18 of 27 | 27 | 27 | 27 |
| letters in silences | 0 | 4 | 3 | 4 |
| first recording | whole | not whole | not whole | not whole |
| random carrier, seeds of 20 | 2 | 20 | 20 | 20 |
| loud noise, steady carrier | nothing | - | - | nothing |
| strays summed, highest | 26.8, 3.3 | 51.6, 5.7 | 50.3, 5.7 | 51.3, 5.3 |
| nine hands at 48 of 60 | 9 | 8 | 6 | 6 |
| short-dah rows' guard | holds | holds | holds | holds |
| band alone | not Morse | - | - | 284 to 298 marks, not Morse, not silence |
| CPU on piece-04 against two finders | - | 98 s / 184 s | not printed | 100 s / 184 s |

**The three matrices, round three with the rule on** (shipped in brackets):

Dah over dit by speed, flat, 20 dB:

| | 10 WPM | 15 WPM | 20 WPM | 25 WPM | row |
|---|---|---|---|---|---|
| dah 1.6 | 59 (60) | 53 (60) | 36 (60) | 49 (60) | 197 (240) |
| dah 2.0 | 52 (59) | 60 (60) | 57 (60) | 60 (22) | 229 (201) |
| dah 2.5 | 57 (59) | 60 (59) | 56 (60) | 59 (60) | 232 (238) |
| dah 3.0 | 60 (60) | 58 (60) | 48 (60) | 55 (60) | 221 (240) |

Dah over dit by signal, flat, 20 WPM:

| | 5 dB | 10 dB | 20 dB | row |
|---|---|---|---|---|
| dah 1.6 | 51 (45) | 47 (36) | 36 (60) | 134 (141) |
| dah 2.0 | 57 (43) | 59 (59) | 57 (60) | 173 (162) |
| dah 2.5 | 59 (53) | 60 (59) | 56 (60) | 175 (172) |
| dah 3.0 | 60 (16) | 54 (59) | 48 (60) | 162 (135) |

Signal by fade, dah 2.5, 20 WPM:

| | flat | 12 dB of QSB at 0.7 Hz | row |
|---|---|---|---|
| 5 dB | 59 (53) | 20 (0) | 79 (53) |
| 10 dB | 60 (59) | 49 (14) | 109 (73) |
| 20 dB | 56 (60) | 27 (13) | 83 (73) |

**The fixtures, round three with the rule on:**

| group | read | wrong | invented | score |
|---|---|---|---|---|
| synthetic fixtures | 38 of 67 | 11 | 7 | 20 |
| receiver: easy | 90 of 90 | 0 | 0 | 90 |
| receiver: working | 17 of 81 | 7 | 6 | 4 |
| receiver: edge | 42 of 81 | 10 | 1 | 31 |
| receiver: ungraded | 74 of 98 | 3 | 41 | 30 |
| CQs 15 / 5 / 0 dB | 84 / 84 / 5 of 84 | 0 / 0 / 3 | 0 | 84 / 84 / 2 |

**The first proof, round three:** every prototype case at the prototype's reads, three seeds each:
- the 18 WPM ragchew at 10 and 5 dB flat: 60 of 60;
- at 0 dB: 60 of 60;
- at 10 dB fading: 60 of 60;
- at 5 dB fading: 60, 59 and 59;
- the 12 WPM fist with a dah of 1.7: 60, 60 and 59.

The band alone gives 284 to 298 marks, dah share 0.11 to 0.15, separation 1.06 to 1.10: not Morse, but not silence.

**Both scoreboards' rows**, as shipped:
- twelve-plus-three: `| 589 | 2026-10-27 | 318 of 353 | 20 | 1 | **297** | 0 | 86 of 110, 3 added | the sequence reader, finished ... 275 with SequenceReader on (210, 275, 275), left off; the board unchanged |`
- W1AW: `| 589 | 2026-10-27 | **2111** | 2131 of 2151 | 20 | 0 | 438 of 440 | the sequence reader, finished (HM-DEC-293): 2070 with SequenceReader on (1882, 1910, 2070), left off; unchanged |`

**The lines as shipped, `--no-build`:** the app line 278 of 278; the engine line 159 of 159 in 11.4 minutes; the survey line 1 of 1 in 10.3 minutes, every group as before (297, 2111, the bank 1891), only its `Rerun` line changed.

## 4. What's blocking us

**Left, one line each:**
- **Two stations in one window.** `144020`'s second station reads 10 of 19. Which lane prints when two share the window is none of the three changes.
- **The opening.** A lane starting cold on a crossed rhythm loses W1AW piece-02's first 36 s (30 letters) and the first recording's `FE` and `HAT`.
- **Invented letters on the bank,** 101 against 10, mostly in the flat half (60). The bank was 1 at unit 588's end.
- **The band alone is not silence.** Noise decodes as marks with the "nobody there" pattern. The silence state did not change that, because noise has two levels.
- **Strong flat hands.** At 20 dB with dah 1.6 and 3.0 they read under shipped at 20 WPM (36 and 48 against 60).
- **The 1.7 and 2.0 straight-key hands at seeds 5771 and 5772** read 30, 35 and 36 of 60.
- **The random carrier** prints at every seed. Only content would part it from a hand. The owner rules on whether that limit stands.
- **Letters in silences:** 4, on `200157`, `143906` and `221502`.
- **The working fixtures** read 17 of 81 in every round, unchanged by the three fixes.

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
