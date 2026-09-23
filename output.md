READ IN THIS ORDER.

A. The phase goal - CW decodes again. Steps 0, 1 and 2 are ticked on every
   criterion; the outcome file holds 0 and 2 at not started, a layer
   mismatch reported here; step 3 is partial at 3.1 and 3.3 with 21 reds
   open and nothing retirable; step 4 is this unit's, partial at 4.1 at
   entry with 9 of 45 pieces judged; step 5 is Tim's. After this unit
   step 4 is partial with 4.1 and 4.3 met and 33 of 45 pieces judged.
B. The criteria, one line each, met or not: 4.1 met by unit 395; 4.2 33
   of 45 judged, 0 kept on a named number, not ticked, ticked only at 45;
   4.3 33 out across units 395 and 396, the 20 applied each reverted in
   the next commit and the 13 never applied, ticked 33 of 33; 4.4 no piece
   kept, no floor moved, not ticked, ticked only at 45; 4.5 the last
   report's, not ticked, ticked only at 45; 4.6 floors and both lines
   green at exit, the synthetics red at both ends under R53, not ticked,
   ticked only at 45.
C. The report last. Section 4 raises 0 items and none is in the way of a
   criterion in B; everything carried from before this phase and every
   finding that blocks nothing is in docs/phase-cw/PARKED.md under R54,
   not here - this unit added 396 items 1 to 3 there.

UNIT:       396 - complete at task 3 of 4, tasks 0 to 3, pieces 34 to 45 not started on the clock rule - 2026-09-23 01:34
PHASE GOAL: get the CW decoder reading on the air again, from the restored 08-25 decoder, with every August rework piece let back in only on a measured number
UNIT GOAL:  judge the rework pieces after piece 9 one commit at a time on the three floor tests and the printer, keep one only if a named number improves, revert the rest in the next commit, and tick 4.3 on that record
ADVANCED:   yes - 24 more pieces judged, 9 to 33 of 45, and 4.3 met on 33 of 33 out pieces each reverted in the next commit or never applied
NUMBER:     pieces judged 9 -> 33 of 45, kept 0, out 33; 021410 47 -> 47 characters, WEEKEND at distance 5 -> 5; 013637 ABOVE at 2 -> 2; captures type 92 s -> 92 s; engine line 372 s -> 374 s of 480
DRIFT:      0

## 1. What Claude did

**Complete at task 3 of 4. Tasks 0 to 3 were all done and none was dropped. Pieces 34 to 45 were
not started because the clock rule fired, and those pieces are the named drop candidate.** Windows
11, `C:\Source\HamLet`, Hamlet confirmed by the section 0 gate, branch `main`. The unit started
00:37 and task 3 was committed at 01:34.

**Task 0, the entry round.** Commit `a0fc4faf`. Version 1.13.82 to 1.13.83. `PHASE_STATUS.md`
read `CURRENT_STEP: 0` and `WORK_INSTRUCTION: 395`, as section 5 predicted. It is now `CURRENT_STEP:
4` and `396 - pieces 10 onward, on the same numbers`. `PHASE_OUTCOME.md` and `PHASE_STATUS.md`
rode whole in task 0's commit, the layer's uncommitted edits included, as in units 393 to 395. I
appended `## UNIT 396 - STEP 4` and added one line per task. I copied unit 395's scripts to
`unit396-*`. **Decision 12 applied**: `git diff --stat 259eba0a HEAD -- src tests
docs/carry-forward-tests.txt` printed nothing, so unit 395's exit runs stand as this unit's entry
for the lines: app 277 of 278 twice, engine 176 of 176 in 372 s. The floors ran in full after one
build. Captures were 37 of 37 in 93 s, every capture identical to unit 395's exit. Adjudicated was
13 of 13 in 29 s, and the clean synthetics 0 of 2. The printer gave WEEKEND 5, THINKING 5, FLEX 1,
ABOVE 2, BREEZE 2. The transmit files printed nothing against `7e209cb4`, and `src` printed nothing
against `5688a8a5`.

**Task 1, piece 10.** Commit `94b5dff6`. `4786c7e7` did not apply. Decision 16's sequence found
that it applies only after pieces 7 and 9 together, so it is *dependent, out*, with nothing applied.

**Task 2, pieces 11 to 33.** Every piece with its commit and its revert:

| n | Hash | Piece commit | Revert commit | How judged |
|---|---|---|---|---|
| 10 | 4786c7e7 | none | none | dependent on 7, 9 |
| 11 | f2e1db7a | none, row `3b73c3b1` | none | dependent on 7, 9 |
| 12 | f27174b5 | `ad5fa332` + seam `14155613` | `f5ef1ff8` | measured, nothing moved |
| 13 | 386fdb5d | none, rows `ddb40581`, `56a90616` | none | dependent on 2, 5 and more |
| 14 | 8ca6a633 | `952fb690` | `3a74cf45` | did not build |
| 15 | 4c6e4321 | none, row `2ac93165` | none | dependent on 3, 12 |
| 16 | 501e8e2d | none, row `ac25f1d0` | none | empty |
| 17 | f9c11989 | `0bec4dd6` | `8d8a37f4` | measured, nothing moved |
| 18 | b48d1158 | `5dd24810` | `306419f2` | did not build |
| 19 | 0f2089f3 | none, row `d6854975` | none | dependent on 15, 18 and their chain |
| 20 | ac1d56da | `304ec791` | `c9681461` | measured, nothing moved |
| 21 | 62262b94 | none, row `53806d19` | none | dependent on 1, 3 at least |
| 22 | 0f48c33e | `b381460b` | `60f4453a` | measured, nothing moved |
| 23 | b8cad1f9 | pair with 22 `c89bdd44` | `35102716` | measured, nothing moved |
| 24 | fc1ee77f | none, row `cf222086` | none | dependent on 2, 13 at least |
| 25 | 71b4f044 | `e91ed185` | `8cae3b66` | measured, nothing moved |
| 26 | 68a18d66 | none, row `7b2f677d` | none | dependent on 2, 25, 24 |
| 27 | a91d8fe7 | none, row `d1c31c82` | none | dependent on 25, 26 and chain |
| 28 | ade52536 | `d14c9970` | `c1a7dfc6` | measured, nothing moved |
| 29 | efcd5242 | none, row `93f0f1bd` | none | dependent on 19, 3 and chains |
| 30 | 95a5e063 | `9c70a121` | `9302155c` | measured, **moved ABOVE 2 to 3** |
| 31 | b7147b1f | `ee3dcca8` | `e14f42e0` | measured, nothing moved |
| 32 | c8685e4d | pair with 28 `832a3db3` | `38610a35` | measured, nothing moved |
| 33 | 4935a4f8 | `8b709643` | `e9449d0a` | measured, nothing moved |

Task 2 closed with `e2b31a40`. **Task 3, the exit round**, is commit `14add170`. Every commit was
pushed to `origin main`, and every push returned rc 0.

**Decisions applied.** Unit 395's decisions 1 to 11 were applied as written. The author's decisions:

- **Decision 12 applied.** The diff was empty, so the lines were not run at entry.
- **Decision 13 applied.** The exit round ran whole.
- **Decision 14 applied.** I checked it on the log, `ee0ea0dc..HEAD`. 4.3 is ticked, 33 of 33.
- **Decision 15.** Piece 45 was not reached, so 4.2, 4.4, 4.5 and 4.6 are not ticked. The next
  piece is 34, `e6b1ece7`.
- **Decision 16** was applied to every dependent row, with each chain tested on a clean `Cw` and
  never committed.
- **Decision 17** was applied to piece 16.
- **Decision 18.** The rows are in `unit395-rework.md` section 2 under `### Judged by unit 396`.
  This unit's numbers are in `docs\phase-cw\unit396-rework.md`.
- **Decision 19.** The timeouts were as decision 11. Nothing was backgrounded, and no `dotnet test`
  ran without a filter.

**How-to decisions I made, uncapped and reported in full:**

1. **Piece 12's `Retuned` conflict.** The conflict block was piece 3's `Retuned` body. Piece 12's
   only change inside it was an `Unlock()` call, which unit 392's seam `Retuned() => Unlock()`
   already makes. I resolved it to ours and kept the seam, as a hunk already in the tree under
   decision 3.
2. **Piece 12's seam.** The build error `CS8907 Parameter 'PitchWasAsserted' is unread` was unit
   392's seam property `PitchWasAsserted => false`. I removed it in `14155613` so the piece's record
   parameter supplies the value. This is decision 4's exception, the same shape as unit 395's piece 3.
3. **Priors for decision 16.** Piece 12's raw patch does not apply after piece 3. So for pieces 15,
   19 and 29 I used piece 12 *as committed*, `git diff 56a90616 14155613 -- Cw`, as the prior.
4. **Piece 30's second hunk.** Its first hunk applied with full context. The second adds two members
   whose only context was piece 19's out `_ranked` field. `-C2` and `-C1` refused it, and `-C0`
   placed it after the class's closing brace, so I moved the brace below the two members and changed
   nothing else. I took this as resolving a conflict inside `Cw` under decision 3 and not as a
   dependency, because every name the piece uses exists at HEAD. **Overrulable.** If the arbiter
   reads it as a dependency, piece 30's row becomes *dependent, out*, and it is out either way.
5. **Piece 33 started at 01:18:51**, which is minute 41 of an hour begun at 00:37:24, inside decision
   10's letter. Its revert landed after minute 42, and nothing was started after it.
6. **I ran the printer at exit although nothing was kept.** Task 3 asks for it only if a piece was
   kept. It costs 5 s and puts the exit distances on a measured run, not on inference.
7. **The scripts read the task label from `.run-unit\unit396-task.txt`.** A leading `TL=...`
   environment assignment was refused at the prompt. The status NOTE for piece 10 still said
   *task 2* while the TASK field said 1 of 4.

**Script faults of my own, caught and corrected before anything was written from them.** First,
the first dependency script printed `basename`'s exit code, not `git apply`'s. I fixed it and re-ran
it for piece 11. Piece 10's table came from unit 395's `dep.sh`, which was right. Second, piece
13's first dependency pass was contaminated by a file that `--3way` had added and left on disk. I
discarded that pass, and `deps.sh` now cleans `Cw` before and after each chain. Third, commit
`ddb40581` missed piece 13's row in `unit395-rework.md` because of an ambiguous edit. `56a90616`
added it before piece 14 was committed.

**Self-rulings: none.**

**Regressions: none.** Nothing that was green at task 0 or at unit 395's exit is red at task 3.

**Mismatches against section 5:**

- `docs\phase-cw\PARKED.md` measured 62 lines at entry, not 63. It did have two headings and 31
  items.
- The root has three untracked layer files the instruction does not list: `SESSION.lock`,
  `tools\arbiter\run-phase-opus.bat` and `tools\arbiter\validate-output.bat.bak-20260922`. I left
  them as found.
- Everything else held as stated:
  - HEAD `259eba0a`, and props line 1254 read 1.13.82.
  - `CLAUDE.md` line 360's top row is HM-DEC-167, `PROJECT_STATUS.md` writes HM-DEC-165, and the
    reload reads CPS-DEC-0167.
  - `PHASE_OUTCOME.md` holds steps 0 and 2 at not started, 1 done, and 3 and 4 partial, with the
    paired entries.
  - `cw-retired-tests.txt` is absent.
  - The `src` and `Cw` diff stats were as given, and the eleven transmit files were silent.
  - The printer, floor lines 107 and 108, the carry-forward list's 918 lines, and the 21 `<Compile
    Remove>` items were as stated.
  - The worktrees were the root and the three preflight trees.

`git worktree list` at exit shows the same four.

## 2. What the owner should expect

Nothing changes on the CW tab. No piece was kept, so the decoder in the build is still exactly the
08-25 decoder that unit 392 restored: `src` is byte-identical to `5688a8a5`. So far 33 of the 45
August pieces have been judged and none stayed in:

- 20 were put back and measured, or failed to build, and each was taken straight back out in the
  next commit. This unit accounts for 13 of them.
- 10 were never run, because each needs two or more other out pieces under it. The chains are the
  tone-survey work, the pitch ranking, the joint cutter and the posterior.
- 3 had nothing to apply.

`021410` and `013637` are where they were: WEEKEND at 5, THINKING 5, FLEX 1, ABOVE 2, BREEZE 2.
Their settled text still ends `FLENT 66OAM` and still has `AB OV E`. Only one piece moved anything,
and it moved it the wrong way. Piece 30 blanks every character decoded before the survey has
admitted a pitch. With it, 013637's opening went to blocks and ABOVE drifted from 2 to 3, so it
went out. 4.3 is now ticked on the record of every revert. The next unit starts at piece 34,
`e6b1ece7`, with 12 left to judge.

**What will look wrong but is not:**

- Every revert commit is titled *moved nothing*, including piece 30's, which moved ABOVE away. The
  title is decision 9's fixed form, and the commit body and the doc row say what it measured.
- Pieces 14 and 18 are titled *moved nothing* too, but they never built.
- The two clean synthetics are red, as they have been since step 1 (R53).

## 3. What you should see

No visible change. This unit judged 24 more pieces against the floors and kept none, so the
decoder you run is the one you ran before it.

**The step's table so far, in 4.5's shape.** *Entry* is the kept state before every piece: 021410
47 characters, WEEKEND 5, THINKING 5, FLEX 1; 013637 63 characters, ABOVE 2, BREEZE 2; synthetics 0
of 2. "Identical" means all 37 captures and the five distances are unchanged.

| n | Hash | Claim | Number before | Number after | Captures wall | Kept or out |
|---|---|---|---|---|---|---|
| 1 | 2068f868 | read the first seconds again | entry | named numbers identical; 004507 50 to 49, 003758 63 to 58, 031948 34 to 31, 012748 4 to 2 | 94 s | out |
| 2 | 6fc36a1e | log how close the argument was | entry | identical | 95 s | out |
| 3 | 3e84ac74 | let go of a pitch on a frequency the radio has left | entry | identical | 94 s | out |
| 4 | 39a42c3f | the margin's share on the sheet | entry | nothing applied | none | out |
| 5 | 9de394da | open two constants for a sweep | entry | identical | 94 s | out |
| 6 | 3d4694e5 | the confirmation window stays at two | entry | comment only, judged with 5 | 94 s, piece 5's | out |
| 7 | 7fb89d5e | record which admission test refused which bin | entry | identical | 94 s | out |
| 8 | 1bf4372d | the separation bound must not move | entry | pair with 7: identical | 94 s | out |
| 9 | 44cf3fc8 | both gate derivations, both off | entry | pair with 7: identical | 94 s | out |
| 10 | 4786c7e7 | both gate derivations measured, neither ships | entry | not run, dependent on 7, 9 | none | out |
| 11 | f2e1db7a | collect the raw run stream | entry | not run, dependent on 7, 9 | none | out |
| 12 | f27174b5 | the operator may assert a station | entry | identical | 97 s | out |
| 13 | 386fdb5d | cuts and characters together, behind a setting | entry | not run, dependent on 2, 5 and more | none | out |
| 14 | 8ca6a633 | rank candidate pitches; not wired | entry | did not build, CS0122 | none | out |
| 15 | 4c6e4321 | the strongest bin chooses the note | entry | not run, dependent on 3, 12 | none | out |
| 16 | 501e8e2d | delete CwPitchRanking | entry | nothing to apply | none | out |
| 17 | f9c11989 | fit the key-up state, ship nothing | entry | identical | 94 s | out |
| 18 | b48d1158 | rank against one noise floor | entry | did not build, CS0122 | none | out |
| 19 | 0f2089f3 | the ranking supplies the mixdown pitch, off | entry | not run, dependent on 15, 18 and chain | none | out |
| 20 | ac1d56da | port the reference decoder | entry | identical | 93 s | out |
| 21 | 62262b94 | start fresh when the dial moves | entry | not run, dependent on 1, 3 at least | none | out |
| 22 | 0f48c33e | score a decode against what was sent | entry | identical | 94 s | out |
| 23 | b8cad1f9 | test every confidence against correctness | entry | pair with 22: identical | 98 s | out |
| 24 | fc1ee77f | index the lattice by hop and kind | entry | not run, dependent on 2, 13 at least | none | out |
| 25 | 71b4f044 | a posterior over the lattice | entry | identical | 94 s | out |
| 26 | 68a18d66 | carry the posterior to each character | entry | not run, dependent on 2, 25, 24 | none | out |
| 27 | a91d8fe7 | a temperature on the path score | entry | not run, dependent on 25, 26 and chain | none | out |
| 28 | ade52536 | the bench's run-merging bug is not in Hamlet | entry | identical; adjudicated 47 s | 94 s | out |
| 29 | efcd5242 | find the pitch from the band | entry | not run, dependent on 19, 3 and chains | none | out |
| 30 | 95a5e063 | assert nothing from an unjudged pitch | entry | characters and elements identical; unsure up on 25 of 37, 013637 3 to 31; 013637 text blocked to `■■LEAR`, **ABOVE 3**; 021410 identical | 93 s | out |
| 31 | b7147b1f | the percentile threshold, refused | entry | identical | 94 s | out |
| 32 | c8685e4d | the peak window buys nothing | entry | pair with 28: identical | 93 s | out |
| 33 | 4935a4f8 | the peak-referenced threshold, refused | entry | identical | 93 s | out |

The full rows, with the reason for each one, are in `docs\phase-cw\unit395-rework.md` section 2.
Each piece's dependency chains and SETTLED lines are in `docs\phase-cw\unit396-rework.md` section 2.

**The numbers before piece 10 at entry, and the kept state at exit, every capture.** The entry and
exit columns are both measured, and the compare of the two outputs printed no difference in 37 rows.
Each cell reads characters / elements / unsure / tone in Hz.

| Capture | Entry, task 0 | Exit, task 3 |
|---|---|---|
| cw-2026-08-17-013347 | 59 / 108 / 2 / 625 | 59 / 108 / 2 / 625 |
| cw-2026-08-17-013622 | 55 / 84 / 4 / 600 | 55 / 84 / 4 / 600 |
| cw-2026-08-17-134712 | 63 / 98 / 42 / 500 | 63 / 98 / 42 / 500 |
| cw-2026-08-18-004507 | 50 / 118 / 1 / 500 | 50 / 118 / 1 / 500 |
| 08-18-003016 | 57 / 149 / 3 / 670 | 57 / 149 / 3 / 670 |
| 08-18-003126 | 54 / 144 / 6 / 665 | 54 / 144 / 6 / 665 |
| 08-18-003758 | 63 / 121 / 19 / 500 | 63 / 121 / 19 / 500 |
| 08-20-014854 | 0 / 0 / 0 / 600 | 0 / 0 / 0 / 600 |
| 08-20-014935 | 0 / 0 / 0 / 825 | 0 / 0 / 0 / 825 |
| 08-22-014113 | 0 / 0 / 0 / 600 | 0 / 0 / 0 / 600 |
| 08-22-014308 | 0 / 0 / 0 / 575 | 0 / 0 / 0 / 575 |
| 08-22-031838 | 57 / 126 / 15 / 525 | 57 / 126 / 15 / 525 |
| 08-22-031905 | 42 / 118 / 6 / 300 | 42 / 118 / 6 / 300 |
| 08-22-031948 | 34 / 114 / 3 / 500 | 34 / 114 / 3 / 500 |
| 08-22-032012 | 44 / 120 / 1 / 500 | 44 / 120 / 1 / 500 |
| 08-22-032050 | 53 / 123 / 9 / 325 | 53 / 123 / 9 / 325 |
| 08-22-032113 | 55 / 118 / 8 / 650 | 55 / 118 / 8 / 650 |
| 08-22-032129 | 66 / 119 / 1 / 650 | 66 / 119 / 1 / 650 |
| 08-23-001520 | 5 / 45 / 4 / 600 | 5 / 45 / 4 / 600 |
| 08-23-001831 | 55 / 124 / 11 / 525 | 55 / 124 / 11 / 525 |
| 08-23-001952 | 75 / 142 / 19 / 525 | 75 / 142 / 19 / 525 |
| 08-23-002016 | 75 / 136 / 31 / 525 | 75 / 136 / 31 / 525 |
| 08-24-012403 | 22 / 65 / 1 / 440 | 22 / 65 / 1 / 440 |
| 08-25-011552 | 30 / 89 / 8 / 500 | 30 / 89 / 8 / 500 |
| 08-25-012748 | 4 / 16 / 2 / 395 | 4 / 16 / 2 / 395 |
| 08-25-012823 | 41 / 62 / 15 / 450 | 41 / 62 / 15 / 450 |
| 08-25-012922 | 50 / 112 / 5 / 475 | 50 / 112 / 5 / 475 |
| 08-25-013010 | 54 / 131 / 6 / 475 | 54 / 131 / 6 / 475 |
| 08-25-013150 | 58 / 139 / 7 / 495 | 58 / 139 / 7 / 495 |
| 08-25-013303 | 54 / 146 / 10 / 500 | 54 / 146 / 10 / 500 |
| 08-25-013402 | 61 / 161 / 5 / 525 | 61 / 161 / 5 / 525 |
| 08-25-013520 | 60 / 153 / 5 / 540 | 60 / 153 / 5 / 540 |
| 08-25-013637 | 63 / 164 / 3 / 550 | 63 / 164 / 3 / 550 |
| 08-25-021410 | 47 / 99 / 11 / 550 | 47 / 99 / 11 / 550 |
| 08-25-021629 | 47 / 96 / 20 / 500 | 47 / 96 / 20 / 500 |
| 08-25-021825 | 41 / 74 / 16 / 400 | 41 / 74 / 16 / 400 |
| 08-26-125941 | 0 / 0 / 0 / 400 | 0 / 0 / 0 / 400 |

All captures other than the first four are under `unadjudicated/cw-2026-`. 36 rows sit on their
floor. `012748` is at 4 and 16 against a floor of 2 and 4.

**The printer, at entry and at exit, identical both times:**

| Capture | Settled text | Word | Distance |
|---|---|---|---|
| 021410 | `■ ■ ■ M ■ ■ ■ ■ T O MTT T  Y M TT ■ ■ O AO IHI DT ■RIGHR IS ■ FLENT 66OAM` | WEEKEND | 5 |
| | | THINKING | 5 |
| | | FLEX | 1 |
| 013637 | `TE MP NEVEN T REV■R G O T AB OV E ■7 5 F ES ■CLEAR S KY LI TE BR EE Z E ALL DAY JUST AWE SO` | ABOVE | 2 |
| | | BREEZE | 2 |

**The runs.**

- **Decision 12 at entry.** `git diff --stat 259eba0a HEAD -- src tests
  docs/carry-forward-tests.txt` printed nothing. The entry numbers for the lines are therefore
  unit 395's exit runs: app 277 of 278 twice, with 1 name lost to the dispatcher loop each run;
  engine 176 of 176 in 372 s.
- **App line at exit:** 278 of 278 in 155 s, nothing lost.
- **Engine line at exit:** 176 of 176 in 374 s of 480.
- **Captures type:** entry 37 of 37 in 93 s, exit 37 of 37 in 92 s. The piece runs took 93 to 98 s
  and none came near decision 7's 240 s.
- **Adjudicated type:** entry 13 of 13 in 29 s, exit 13 of 13 in 29 s. The piece runs took 28 to
  30 s, except piece 28's at 47 s.
- **Clean synthetics:** 0 of 2 at entry, after every measured piece, and at exit. They read `■ ■ ■
  ■ ■  ■ ■ ■ ■■` and `■ ■ ■  ■■■` against `CQ DE W1AW K`, red under R53.
- **Transmit files** against `7e209cb4`: nothing, at entry, after every piece, and at exit.
- **`src`** against `5688a8a5` at exit: nothing, because no piece was kept.

## 4. What's blocking us

Nothing blocks a criterion of step 4. The next unit starts at piece 34, `e6b1ece7`, on the same
list and under the same rules. This unit's non-blocking findings are in `docs\phase-cw\PARKED.md`
as 396 items 1 to 3, along with everything carried from before this phase, under R54:

1. Ten pieces could not be run because each needs a chain longer than a pair.
2. `CwPitchRanking` does not build on the restored tracker.
3. Piece 30 is the only piece that moved a number, and it moved it away from the word.
