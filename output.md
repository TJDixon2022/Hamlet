```
READ IN THIS ORDER.

A. The phase goal - CW decodes again. Steps 0, 1 and 2 are ticked on every
   criterion; the outcome file holds 0 and 2 at not started, a layer
   mismatch reported here, and the launcher named step 0 for this unit on
   that reading; step 3 is partial at 3.1 and 3.3 with 21 reds open and
   nothing retirable; step 4 was this unit's, partial at 4.1 and 4.3 at
   entry with 33 of 45 pieces judged and none kept; step 5 is Tim's. After
   this unit step 4 has every criterion ticked with 0 kept and 14 never
   applied for want of a chain.
B. The criteria, one line each, met or not: 4.1 met by unit 395; 4.2 45
   of 45 judged, 27 applied in their own or a pair commit, 0 kept on a
   named number, 14 listed dependent and never applied, 4 with nothing to
   apply - ticked per decision 22 with all three counts and the 14 hashes;
   4.3 45 out across units 395 to 397, each applied one reverted in the
   next commit, re-confirmed at 45 of 45; 4.4 no piece kept and no floor
   moved, the floor table byte-identical to the step's entry - ticked; 4.5
   this report's section 3 leads with the 45-row table - ticked; 4.6
   floors and both lines green at exit, the synthetics red at both ends
   under R53 - ticked.
C. The report last. Section 4 raises 0 items in the way of a criterion in
   B; everything carried from before this phase and every finding that
   blocks nothing is in docs/phase-cw/PARKED.md under R54, not here.
```

```
UNIT:       397 - complete at task 2 of 3, none dropped - 2026-09-23 02:18
PHASE GOAL: Get the CW decoder reading real on-air audio again, by putting it back where it last worked and then deciding, on measured numbers, which of the August changes earn their way back in.
UNIT GOAL:  Finish step 4 - put pieces 34 to 45 of the August rework back one at a time on the restored decoder, keep only one that moves a named number with the floors green, and tick the step's last four criteria at piece 45.
ADVANCED:   yes - the last 12 pieces were judged, so all 45 are, and 4.2, 4.4, 4.5 and 4.6 are ticked; nothing was kept, and the decoder is unchanged.
NUMBER:     pieces judged 33 -> 45 of 45, kept 0, out 45, never applied for want of a chain 14; 021410 47 -> 47 characters, WEEKEND at distance 5 -> 5; 013637 ABOVE at 2 -> 2; captures type 92 s -> 92 s; engine line 374 s -> 374 s of 480
DRIFT:      0
```

## 1. What Claude did

**Complete, at task 2 of 3 - tasks 0, 1 and 2 all done, nothing dropped.** The clock rule never
fired: piece 45 was judged at 02:04, minute 21 of the hour that began with the first status line
at 01:43. Windows 11 machine, Hamlet confirmed at `C:\Source\HamLet` by the gate's four checks,
branch `main`. Every commit pushed; the last push succeeded.

**Task 0, entry** (`5e6dd4e8`). Version 1.13.83 to 1.13.84. `PHASE_STATUS.md` read `CURRENT_STEP: 0`
and `WORK_INSTRUCTION: 396 - pieces 10 onward, on the same numbers`; set to `CURRENT_STEP: 4` and
`397 - pieces 34 to 45, the list to its end`. `PHASE_OUTCOME.md` and `PHASE_STATUS.md` went into task
0's commit as whole files, as units 393 to 396's did. Unit 396's scripts copied to `unit397-*` with the
unit number and task labels changed. **Decision 20 applied**: `git diff --stat e2b31a40 HEAD -- src
tests docs/carry-forward-tests.txt` printed nothing, so unit 396's exit runs are the entry numbers
for both lines. Floors run in full: 37 of 37, 13 of 13, synthetics 0 of 2, every capture row and the
printer identical to unit 396's exit.

**Task 1, pieces 34 to 45**, one row each in `docs\phase-cw\unit395-rework.md` section 2 under
`### Judged by unit 397`, one paragraph each in `docs\phase-cw\unit397-rework.md` section 2:

| n | Hash | Piece or pair commit | Revert or row commit | Outcome |
|---|---|---|---|---|
| 34 | e6b1ece7 | `9627bc0b` | `4c932f96` | measured; 14 captures and 1 adjudicated anchor red; out |
| 35 | dfb357ef | `2669b5f9`, pair with 28 | `49cd3ed9` | measured; nothing moved; out |
| 36 | efc33267 | `c08f766d` | `356ab900` | measured; nothing moved; out |
| 37 | aeea24f2 | none | `e19042ae` | dependent on 19, 29, 30; not applied |
| 38 | a37cfcff | none | `6b4d4b16` | dependent on 13, 34 and more; not applied |
| 39 | a09b36a7 | `2b9c5d10` | `a95fc8fb` | did not build, `CwElement`; out |
| 40 | ee2cba8d | none | `a2f4618e` | dependent on 31, 33, 34; not applied |
| 41 | 2828ab69 | `548d7007`, pair with 39 | `85b97b15` | did not build, `CwElement`; out |
| 42 | 43efc525 | `ec9dec4a` | `6d484e97` | measured; captures identical, synthetics emptied; out |
| 43 | 865e66d8 | none | `0f659396` | already in the tree as unit 392's seam; empty; out |
| 44 | 9c2a7f99 | none | `f860651d` | dependent on 1, 19, 29, 37; not applied |
| 45 | 1a84188e | `25bc3bcf` | `a902cdf8` | did not build, `_window`; out |

Task 1's record commit is `3af36501`.

**Task 2, exit round** (`2164449d`): both carry-forward lines, the three floor types, the printer,
the transmit and `src` checks, decision 14's log check. 4.2, 4.4, 4.5 and 4.6 ticked in the root
`PHASE_PLAN.md`; 4.3's sentence updated to 45 of 45. `PARKED.md` got `397 item 1` to `397 item 4`.

**Decisions applied.** Unit 395's 1 to 11 and unit 396's 12 to 19 as the step's rules; this unit's 20
(entry lines from unit 396's exit), 21 (exit round run in full), 22 (the ticks, each worded to its
criterion), 24 (the app and Audio halves of 36, 42, 43, 44, 45 not taken), 25 (the docs), 26 (section
3's table), 27 (timeouts, nothing backgrounded, minute 42). Decision 23 changed no row. **No
self-ruling** authorized work outside the tasks.

**How-to decisions made during the tasks, each overrulable:**

1. **Piece 35 went in as a pair with 28, with 35 merged under `--3way`.** `--check` cleared only
   after 28 and 32. The only extra need was piece 32's context lines, not any name the piece uses.
   Unit 396 merged piece 33 on piece 31's context the same way, so this is one pair, not a chain of
   three.
2. **Piece 42's `OnSamples` conflict was resolved to the piece's own lines.** About 580 lines of
   out pieces' bodies came in as "theirs" context; they were not taken. Its `DecodeQueueDroppedChunks`
   and `DecodeQueueDroppedSamples` were dropped as already in the tree, because unit 392's seam
   declares both and decision 3 keeps the seam. 175 of its 181 lines went in.
3. **Piece 43 is out as empty under decision 17.** Both of its code lines are unit 392's seam; what
   remained was a doc comment and a blank line.
4. **Piece 44 was listed dependent as a whole.** Its `CwKeyingMeter` half was not split off and
   measured alone, because decision 5 judges the whole piece. Parked as 397 item 2.
5. **Piece 45 was applied and built rather than listed dependent**, because it applied clean. The
   build error names the dependency, piece 44's `_window`.

**Regressions: none.** Every capture, adjudicated reading, line and printer number at exit is what it
was at entry.

**Section 5 mismatches.** HEAD, `Directory.Build.props` line 1254, `PHASE_STATUS.md`, `PHASE_OUTCOME.md`,
the five root files, `CLAUDE.md` line 360 (HM-DEC-167; the reload reads `CPS-DEC-0167`),
`PARKED.md`, `unit395-rework.md`, both diffs, the printer, lines 107 and 108, the carry-forward file,
the 21 `Compile Remove` items and the worktrees all matched the instruction. **One mismatch:**
`PROJECT_STATUS.md` at HEAD reads unit 390, `STATE: COMPLETED`, `TASK: TASK 5 of 6`, not unit 396,
`TASK 3 of 4`. **One slip of my own:** my first command ran `tools/status.sh` with no arguments. That
blanked the working copy of `PROJECT_STATUS.md` before I had read it, so I can't say what the layer
had written there. The next call, 17 seconds later, wrote a proper `EXECUTING` status. Both are
parked as 397 item 4.

## 2. What the owner should expect

Nothing changes on the CW tab. All 45 pieces of the August rework have now been judged one at a
time against the decoder that read on the air on 2026-08-25, and none of them stayed. 27 went back
in and came straight out in the next commit. 14 were never run because each needed two or more
other pieces that were themselves out, and the plan allows only one pair. 4 had nothing to apply.
None of the 27 improved a named number. The twelve this unit took split like this:
- Piece 34 turned 14 captures red.
- Piece 42, the decode worker thread, emptied the two clean synthetics' text.
- Three did not compile without the pieces they depend on.
- The rest moved nothing.

`021410` is still 47 characters, WEEKEND at distance 5 in the settled text. `013637` still has `AB OV E`,
ABOVE at distance 2. Every floor is green at its old number, both carry-forward lines are green, and
the two clean synthetics are red as they have been since step 1 (R53). **What will look wrong but is
not:** 45 revert commits titled *moved nothing*, some of them on pieces that moved numbers the wrong
way or did not build. Decision 9 fixes that title, and each commit body and doc row gives the real
reason. The next step on the plan is step 3's repairs under R49, now that step 4's verdict is in.

## 3. What you should see

**Step 4, all 45 pieces.** E is the kept state before every piece: 021410 47 characters, WEEKEND 5,
THINKING 5, FLEX 1; 013637 63 characters, ABOVE 2, BREEZE 2; synthetics 0 of 2 reading placeholders.
*Identical* means all 37 captures identical in characters, elements, unsure and tone, the five
distances identical, and synthetics 0 of 2.

| n | Number before | Number after | Kept or out | Hash | Captures wall |
|---|---|---|---|---|---|
| 1 | E; 004507 50, 003758 63, 031948 34, 012748 4 | 021410 and 013637 unchanged; 004507 49, 003758 58, 031948 31, 012748 2 | out | 2068f868 | 94 s |
| 2 | E | identical | out | 6fc36a1e | 95 s |
| 3 | E | identical | out | 3e84ac74 | 94 s |
| 4 | E | not run, nothing to apply, already in the tree | out | 39a42c3f | 92 s, task 0's run |
| 5 | E | identical | out | 9de394da | 94 s |
| 6 | E | pair with 5, comment only, piece 5's run: identical | out | 3d4694e5 | 94 s, piece 5's run |
| 7 | E | identical | out | 7fb89d5e | 94 s |
| 8 | E | pair with 7: identical | out | 1bf4372d | 94 s |
| 9 | E | pair with 7: identical | out | 44cf3fc8 | 94 s |
| 10 | E | not run, dependent on 7 and 9 | out | 4786c7e7 | none |
| 11 | E | not run, dependent on 7 and 9 | out | f2e1db7a | none |
| 12 | E | identical | out | f27174b5 | 97 s |
| 13 | E | not run, dependent on 2, 5 and more | out | 386fdb5d | none |
| 14 | E | not run, did not build | out | 8ca6a633 | none |
| 15 | E | not run, dependent on 3 and 12 | out | 4c6e4321 | none |
| 16 | E | not run, empty | out | 501e8e2d | none |
| 17 | E | identical | out | f9c11989 | 94 s |
| 18 | E | not run, did not build | out | b48d1158 | none |
| 19 | E | not run, dependent on 15, 18 and their chain | out | 0f2089f3 | none |
| 20 | E | identical | out | ac1d56da | 93 s |
| 21 | E | not run, dependent on 1 and 3 at least | out | 62262b94 | none |
| 22 | E | identical | out | 0f48c33e | 94 s |
| 23 | E | pair with 22: identical | out | b8cad1f9 | 98 s |
| 24 | E | not run, dependent on 2 and 13 at least | out | fc1ee77f | none |
| 25 | E | identical | out | 71b4f044 | 94 s |
| 26 | E | not run, dependent on 2, 25 and 24 | out | 68a18d66 | none |
| 27 | E | not run, dependent on 25, 26 and their chain | out | a91d8fe7 | none |
| 28 | E | identical | out | ade52536 | 94 s |
| 29 | E | not run, dependent on 19, 3 and their chains | out | efcd5242 | none |
| 30 | E; 021410 11 unsure, 013637 3 unsure | characters identical; unsure up on 25 of 37; **ABOVE 2 to 3** | out | 95a5e063 | 93 s |
| 31 | E | identical | out | b7147b1f | 94 s |
| 32 | E | pair with 28: identical | out | c8685e4d | 93 s |
| 33 | E | identical | out | 4935a4f8 | 93 s |
| 34 | E | **captures 23 of 37, 14 red; adjudicated 12 of 13**; 021410 47 to 45, WEEKEND 5; ABOVE 2; 004507 50 to 53, 002016 75 to 77 | out | e6b1ece7 | 82 s |
| 35 | E | pair with 28: identical | out | dfb357ef | 94 s |
| 36 | E | identical | out | efc33267 | 94 s |
| 37 | E | not run, dependent on 19, 29 and 30 | out | aeea24f2 | none |
| 38 | E | not run, dependent on 13, 34 and more | out | a37cfcff | none |
| 39 | E | not run, did not build, `CwElement` | out | a09b36a7 | none |
| 40 | E | not run, dependent on 31, 33 and 34 | out | ee2cba8d | none |
| 41 | E | pair with 39: not run, did not build, `CwElement` | out | 2828ab69 | none |
| 42 | E | captures and distances identical; **synthetics 0 of 2, both empty** | out | 43efc525 | 94 s |
| 43 | E | not run, already in the tree, empty | out | 865e66d8 | none |
| 44 | E | not run, dependent on 1, 19, 29 and 37 | out | 9c2a7f99 | none |
| 45 | E | not run, did not build, `_window` | out | 1a84188e | none |

**The captures at entry and at exit.** Entry is the numbers before piece 34; exit is the kept state
on `3af36501`'s tree. The compare printed no difference in 37 rows.

| Capture | Characters | Elements | Unsure | Tone | Exit |
|---|---|---|---|---|---|
| cw-2026-08-17-013347 | 59 | 108 | 2 | 625 | same |
| cw-2026-08-17-013622 | 55 | 84 | 4 | 600 | same |
| cw-2026-08-17-134712 | 63 | 98 | 42 | 500 | same |
| cw-2026-08-18-004507 | 50 | 118 | 1 | 500 | same |
| unadjudicated/cw-2026-08-18-003016 | 57 | 149 | 3 | 670 | same |
| unadjudicated/cw-2026-08-18-003126 | 54 | 144 | 6 | 665 | same |
| unadjudicated/cw-2026-08-18-003758 | 63 | 121 | 19 | 500 | same |
| unadjudicated/cw-2026-08-20-014854 | 0 | 0 | 0 | 600 | same |
| unadjudicated/cw-2026-08-20-014935 | 0 | 0 | 0 | 825 | same |
| unadjudicated/cw-2026-08-22-014113 | 0 | 0 | 0 | 600 | same |
| unadjudicated/cw-2026-08-22-014308 | 0 | 0 | 0 | 575 | same |
| unadjudicated/cw-2026-08-22-031838 | 57 | 126 | 15 | 525 | same |
| unadjudicated/cw-2026-08-22-031905 | 42 | 118 | 6 | 300 | same |
| unadjudicated/cw-2026-08-22-031948 | 34 | 114 | 3 | 500 | same |
| unadjudicated/cw-2026-08-22-032012 | 44 | 120 | 1 | 500 | same |
| unadjudicated/cw-2026-08-22-032050 | 53 | 123 | 9 | 325 | same |
| unadjudicated/cw-2026-08-22-032113 | 55 | 118 | 8 | 650 | same |
| unadjudicated/cw-2026-08-22-032129 | 66 | 119 | 1 | 650 | same |
| unadjudicated/cw-2026-08-23-001520 | 5 | 45 | 4 | 600 | same |
| unadjudicated/cw-2026-08-23-001831 | 55 | 124 | 11 | 525 | same |
| unadjudicated/cw-2026-08-23-001952 | 75 | 142 | 19 | 525 | same |
| unadjudicated/cw-2026-08-23-002016 | 75 | 136 | 31 | 525 | same |
| unadjudicated/cw-2026-08-24-012403 | 22 | 65 | 1 | 440 | same |
| unadjudicated/cw-2026-08-25-011552 | 30 | 89 | 8 | 500 | same |
| unadjudicated/cw-2026-08-25-012748 | 4 | 16 | 2 | 395 | same |
| unadjudicated/cw-2026-08-25-012823 | 41 | 62 | 15 | 450 | same |
| unadjudicated/cw-2026-08-25-012922 | 50 | 112 | 5 | 475 | same |
| unadjudicated/cw-2026-08-25-013010 | 54 | 131 | 6 | 475 | same |
| unadjudicated/cw-2026-08-25-013150 | 58 | 139 | 7 | 495 | same |
| unadjudicated/cw-2026-08-25-013303 | 54 | 146 | 10 | 500 | same |
| unadjudicated/cw-2026-08-25-013402 | 61 | 161 | 5 | 525 | same |
| unadjudicated/cw-2026-08-25-013520 | 60 | 153 | 5 | 540 | same |
| unadjudicated/cw-2026-08-25-013637 | 63 | 164 | 3 | 550 | same |
| unadjudicated/cw-2026-08-25-021410 | 47 | 99 | 11 | 550 | same |
| unadjudicated/cw-2026-08-25-021629 | 47 | 96 | 20 | 500 | same |
| unadjudicated/cw-2026-08-25-021825 | 41 | 74 | 16 | 400 | same |
| unadjudicated/cw-2026-08-26-125941 | 0 | 0 | 0 | 400 | same |

**The printer**, identical at entry and at exit:

| Capture | Settled text | Word | Distance |
|---|---|---|---|
| 021410 | `■ ■ ■ M ■ ■ ■ ■ T O MTT T  Y M TT ■ ■ O AO IHI DT ■RIGHR IS ■ FLENT 66OAM` | WEEKEND / THINKING / FLEX | 5 / 5 / 1 |
| 013637 | `TE MP NEVEN T REV■R G O T AB OV E ■7 5 F ES ■CLEAR S KY LI TE BR EE Z E ALL DAY JUST AWE SO` | ABOVE / BREEZE | 2 / 2 |

**The runs.**

| Run | Entry | Exit |
|---|---|---|
| App carry-forward line | 278 of 278 in 155 s, unit 396's exit under decision 20 | 278 of 278 in 167 s |
| Engine carry-forward line | 176 of 176 in 374 s of 480, unit 396's exit under decision 20 | 176 of 176 in 374 s of 480 |
| Captures type | 37 of 37 in 95 s | 37 of 37 in 92 s |
| Adjudicated type | 13 of 13 in 29 s | 13 of 13 in 29 s |
| Clean synthetics | 0 of 2 in 3 s, placeholders | 0 of 2 in 4 s, the same placeholders |
| Printer | 2 of 2 in 5 s | 2 of 2 in 6 s |

Decision 20's diff, `git diff --stat e2b31a40 HEAD -- src tests docs/carry-forward-tests.txt`, output:
*(empty)*. At exit the eleven transmit files printed nothing against `7e209cb4`, `src` printed nothing
against `5688a8a5`, and the two floor test files printed nothing against `ee0ea0dc`. `git worktree
list` shows the root and the three preflight trees.

## 4. What's blocking us

Nothing blocks a criterion of step 4; every one is ticked. The unit's findings that block nothing
are `397 item 1` to `397 item 4` in `docs\phase-cw\PARKED.md` under R54:
- item 1: the 14 unrun chains, the step's total.
- item 2: piece 44's meter half.
- item 3: what pieces 34 and 42 moved.
- item 4: the `PROJECT_STATUS.md` mismatch and the status file I blanked.

The layer mismatch persists and is reported, not repaired: `PHASE_OUTCOME.md` and `PHASE_STATUS.md`
hold steps 0 and 2 at `not started` with every criterion of both `[x]` in the plan.
