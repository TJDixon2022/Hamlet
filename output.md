# Work instruction 580 - The fist bank: a yardstick with a truth for every hand on the band

## 1. What Claude did

- **Surface and gate:** Claude Code on the development computer. The prompt claimed `PROJECT: Hamlet`, and the tree at `C:\Source\HamLet` confirmed it:
  - `SHACK_FACTS.md`, `docs\cw-scoreboard.md` and `docs\cw-survey-2026-10-09.md` exist.
  - Neither `CoreHMI.sln` nor `MURC.sln` exists.

  Nothing in this report is evidence about the radio.
- **Branch:** `main`, two commits (the bank and the survey, then the record), pushed.
- **Run by hand:**
  - `SESSION.lock` was taken through `tools\arbiter\lock.bat take` and released at the end.
  - Nothing was written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` was touched, and no box in `PHASE_PLAN.md` was ticked.
  - Nothing keys or transmits, nothing was written to the radio, and no recording was added, removed or promoted. Every fist is generated.
- **Unit 579 was not withdrawn unrun in this tree.** It was run before this order arrived and recorded as HM-DEC-283 (`762967e1`, `4ea5720b`); its change was built, measured and left off.
- **No engine change.** Nothing under `src\` moved, and both boards are exactly as at HEAD: 297 and 2111.
- **The fist bank** (`tests\...\Cw\Fixtures\CwFistBank.cs`, new):
  - **Ninety-six fists:** every dah over dit of 1.6, 2.0, 2.5 and 3.0, at 10, 15, 20 and 25 WPM, at 5, 10 and 20 dB in the 520 Hz passband, flat and with 12 dB of QSB at 0.7 Hz.
  - **Every fist is a hand:** dits and dahs each spread ±15%, gaps inside letters one dit ±20%, letter gaps 2 to 3.5 dits and word gaps 4 to 6, at 615 Hz drifting 3 Hz.
  - **What each sends:** the 60-letter ragchew `CQ CQ DE N0CALL N0CALL K GM KEN NICE TO MEET U RPRT UR 559 QTH IS CHICOPEE MA K`.
  - **Names and seeds:** each is named `fist-r{ratio}-{wpm}wpm-{db}db-{fade|flat}`, and its seed is a hash of its name, so it never changes.
  - **Each has a recipe line,** which the survey prints.
- **Generated, not stored.** Each fist is made at test time by the receiver fixtures' own band, drift and fade, through a new `CwFixtureGenerator.GenerateFromEdges`. That renders keying edges the caller built, so every element and gap can have its own length. Nothing binary was added.
- **The survey reads them** (`TheCwSurveyTests`) as a group of its own, `the fist bank`:
  - **Scoring:** by the boards' yardstick. Invented letters are counted against each fist's own key-downs within 50 Hz of its pitch, and silence against the stretches before its first and after its last key-down, both taken from the recipe, not read off 5 dB audio.
  - **The doc** gains `The fist bank`: three matrices, the bank's total and the worst ten, plus the group's table of every fist.
- **Checks** (`TheFistBankTests`, new):
  - `TheBankIsNinetySixFists`: 96 names and 96 seeds, all distinct.
  - `EachFistKeysItsText`: every mark of the text keyed, each within its spread.
  - `TheCornersRead`: the four corners read, asserting nothing.
- **The survey was not split.** With the bank it ran in 569 s, under its 1800 s.
- **The lines,** after one build, with the build servers shut down:
  - the app line passed 278 of 278 in 2 min 19 s;
  - the engine line, at a 1500 s limit, passed 156 of 156 in 9 min 18 s;
  - the survey line passed 1 of 1 in 9 min 29 s, with `--no-build`.
- **Records:**
  - HM-DEC-284 at the top of `DECISIONS.md`, and its `CLAUDE.md` §1 index row.
  - A 580 row in each table of `docs\cw-scoreboard.md`, unchanged, and a line naming the fist bank and where it lives.
  - `PHASE_OUTCOME.md` and `PHASE_STATUS.md`, both copies.
  - The survey's `Rerun` line.
  - Version 1.13.264 to **1.13.265**.
- **Recorded under §12.1:** nothing. HM-DEC-284 was ordered by work instruction 580.

## 2. What the owner should expect

- **What Hamlet reads today, across every hand.** The bank reads **1,538 of 5,760 letters, 27%**.
  - **By dah over dit** (1,440 letters each): the 1.6 hand reads 56, the 2.0 hand 79, the 2.5 hand 711, the 3.0 hand 692.
  - **By fade** (2,880 each): flat 1,239, fading 299.
  - **By signal** (1,920 each): 5 dB 385, 10 dB 546, 20 dB 607.
- **Three facts the matrices show:**
  - **Reading stops when the dah is shorter than about two and a half dits.**
    - At 20 dB with no fade, the 2.5 and 3.0 hands read every letter at every speed.
    - The 2.0 hand reads nothing at any speed, and the 1.6 hand 7 letters at 10 WPM.
    - The 2.0 hand's one good reading, 44 of 60 at 20 WPM and 10 dB, is luck: there the noise spread its marks wide enough that two of them happened to differ by two.
  - **As the signal falls, a clean fist holds up to 5 dB.**
    - At 20 WPM the 2.5 hand reads 60, 59 and 53 at 20, 10 and 5 dB.
    - The 3.0 hand falls to 16 at 5 dB, which this unit did not trace.
  - **The fade costs nearly everything.**
    - The 2.5 hand at 20 WPM reads 53, 59 and 60 flat at 5, 10 and 20 dB, and 0, 12 and 0 under 12 dB of QSB at 0.7 Hz.
    - Across the whole bank, fading reads a quarter of what flat reads.
- **In the app nothing has changed.** There is no engine change, the boards are 297 and 2111, and nothing on screen moves.
- **The bank is a yardstick, not yet a guard.** Nothing fails if a fist falls; the next units name which fists must rise and which must not fall.
- **The suite:** the app line 278 of 278, the engine line 156 of 156, and the survey 1 of 1, every other group as before.
- **What will look wrong but is not:**
  - `TheRecordingsScoreboard` is red on the random carrier at seed 5195 alone, the known baseline red since unit 552.
  - The order said unit 579 was withdrawn unrun. In this tree it ran and is HM-DEC-283.
  - The lock tool reports its own lock as `STALE` while held, because the `cmd` that took it has exited.
- **Pushed:** two commits on `main`. The pre-existing uncommitted files are left out: `RUN_LEDGER.md`, `PARKED.md`, `.run-unit\denials.txt`, `.run-unit\reports\*`, and the three `cases-*.txt` files.

## 3. What you should see

**Dah over dit by speed,** flat, 20 dB, letters right of 60:

| | 10 WPM | 15 WPM | 20 WPM | 25 WPM | row |
|---|---|---|---|---|---|
| dah 1.6 | 7 | 0 | 0 | 0 | 7 |
| dah 2.0 | 0 | 0 | 0 | 0 | 0 |
| dah 2.5 | 60 | 60 | 60 | 60 | 240 |
| dah 3.0 | 60 | 60 | 60 | 60 | 240 |

**Dah over dit by signal,** flat, 20 WPM:

| | 5 dB | 10 dB | 20 dB | row |
|---|---|---|---|---|
| dah 1.6 | 0 | 2 | 0 | 2 |
| dah 2.0 | 0 | 44 | 0 | 44 |
| dah 2.5 | 53 | 59 | 60 | 172 |
| dah 3.0 | 16 | 59 | 60 | 135 |

**Signal by fade,** dah 2.5, 20 WPM:

| | flat | 12 dB of QSB at 0.7 Hz | row |
|---|---|---|---|
| 5 dB | 53 | 0 | 53 |
| 10 dB | 59 | 12 | 71 |
| 20 dB | 60 | 0 | 60 |

**The bank's total:** 1,538 of 5,760 letters right, 413 wrong, 94 invented, 0 in silence, score **1,031**.

**The worst ten,** by letters right and then score. All read 0 of 60 with nothing printed, and all are 1.6 hands:

- `fist-r1.6-10wpm-10db-fade`, `fist-r1.6-10wpm-10db-flat` and `fist-r1.6-10wpm-5db-flat`;
- `fist-r1.6-15wpm-10db-fade`, `fist-r1.6-15wpm-20db-fade`, `fist-r1.6-15wpm-20db-flat` and `fist-r1.6-15wpm-5db-fade`;
- `fist-r1.6-20wpm-10db-fade`, `fist-r1.6-20wpm-20db-flat` and `fist-r1.6-20wpm-5db-fade`.

**The worst three by score** (letters right less wrong less invented) are fists that print junk: `fist-r1.6-10wpm-20db-fade` at -29, `fist-r2.0-20wpm-20db-fade` at -11, and `fist-r3.0-15wpm-5db-fade` at -10.

**The run time:** the survey with the bank ran in 569 s, against 499 s without it at the last unit, inside its 1800 s. It stays one line, and the bank's section is in `docs\cw-survey-2026-10-09.md`.

**The survey's other groups:** unchanged, since no reading rule changed; only the `Rerun` line and the new fist bank rows and section moved.

**The scoreboard rows, unchanged:**

| unit | right | wrong | invented | score | spaces |
|---|---|---|---|---|---|
| 580, the twelve-plus-three | 318 of 353 | 20 | 1 | **297** | 86 of 110, 3 added |
| 580, W1AW | 2131 of 2151 | 20 | 0 | **2111** | 438 of 440, 11 added |

## 4. What's blocking us

- **Unit 579 ran in this tree.** It is HM-DEC-283, and its change sits off by default as `CwRules.HoldWaitsAndFits`, beside `CwRules.GridStepsAside` and `CwRules.ShortDah`. If the order meant it as withdrawn, those three rules are in the tree for the next units to measure against the bank, or to take out.
- **The short dah is the largest hole.** The 1.6 and 2.0 hands read 135 of 2,880 between them. `CwRules.ShortDah`, built two units ago and off, read the 1.7 and 2.0 straight-key hands whole. It is the obvious first rule to measure against the bank.
- **The fade is the second.** Fading fists read 299 of 2,880 against 1,239 flat.
- **The matrices are one seed a cell.** A cell like the 2.0 hand's 44 at 10 dB is one draw of the noise. A guard built on the bank may want three seeds a cell; that is the next units' to name.
- **Choices taken, named under HM-DEC-276:**
  - **Invented and silence are counted against each fist's recipe,** its own key-downs within 50 Hz, rather than a keying map read off the audio, since the recipe is the truth and a map read at 5 dB under fading would misjudge.
  - **The seed is the FNV-1a hash of the fist's name,** so it never changes while the name does not.

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
