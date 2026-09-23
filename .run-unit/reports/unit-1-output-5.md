```
READ IN THIS ORDER.

A. The phase goal - CW decodes again. Steps 0, 1 and 2 are ticked on every
   criterion; the outcome file holds 0 and 2 at not started, a layer
   mismatch reported here; step 3 is partial at 3.1 and 3.3 with 21 reds
   open and nothing retirable; step 4 is this unit's, opened at 4.1; step 5
   is Tim's. After this unit step 4 is in progress with 4.1 met and 9 of
   45 pieces judged.
B. The criteria, one line each, met or not: 4.1 met - the list of every Cw
   commit from 07f0397a to 2026-09-03 with files and claim, 84 commits, the
   45 pieces numbered, in docs/phase-cw/unit395-rework.md section 1; 4.2 9
   of 45 judged, 0 kept on a named number, not ticked; 4.3 9 out, each one
   applied was reverted in the next commit, not ticked because 36 pieces
   are unjudged; 4.4 no kept piece, so no floor rose and none was touched,
   not ticked; 4.5 the last report's, not this one; 4.6 floors and both
   lines green at exit with the synthetics red at both ends under R53,
   not ticked.
C. The report last. Section 4 raises 0 items, and nothing is in the way of
   a criterion in B; everything carried from before this phase and every
   finding that blocks nothing is in docs/phase-cw/PARKED.md under R54,
   not here.
```

```
UNIT:       395 - complete at task 3 of 4, pieces 10 to 45 not started on the clock rule - 2026-09-23 00:26
PHASE GOAL: Bring the CW decoder back to the one that read on the air, keep it from breaking silently, clear the inherited reds, put back only the parts of the August rework that measurably help, and finish with Tim reading CW off the air.
UNIT GOAL:  Write the list of every Cw commit step 4 works from, then start putting the rework's pieces back in oldest first, one commit each, keeping one only if a number Tim named moves and reverting it if nothing does.
ADVANCED:   yes - 4.1 is met on a committed doc, and 9 pieces are judged on measured runs with each out piece reverted
NUMBER:     pieces judged 0 -> 9 of 45, kept 0, out 9; 021410 47 -> 47 characters, WEEKEND at distance 5 -> 5; 013637 ABOVE at 2 -> 2; captures type 92 s -> 92 s; engine line 375 s -> 372 s of 480
DRIFT:      0
```

## 1. What Claude did

**Complete at task 3 of 4.** Tasks 0 to 3 all ran. Task 2 judged pieces 1 to 9, and the clock
rule (decision 10) fired at 00:11 after piece 9 was reverted. Pieces 10 to 45 were not started.
That is the drop candidate the instruction named, so it is not my sizing decision. The next unit
starts at **piece 10, `4786c7e7`**, on a tree where every piece so far is out. Provenance: the
shack machine, `C:\Source\HamLet`, branch `main`. The gate passed: SHACK_FACTS.md and
`CwProbabilisticDecoder.cs` exist, and neither `.sln` is present.

**Task 0, `55ae0e93`.** Version 1.13.81 to 1.13.82. `PHASE_STATUS.md` read `CURRENT_STEP: 0` and
`WORK_INSTRUCTION: 394`. I set it to `4` and `395 - the rework goes back one piece at a time, on
numbers`. I created `docs\phase-cw\PARKED.md` under R54 with one line per carried item: unit
389's item 1, 389's rest and the older queue by reference, unit 390's nine, 391's items 2 to 6,
392's items 2 to 5, 393's five (item 1 marked as a self-ruling Tim may overrule, with the type
left where 393 put it), and 394's six. Entry round:
- The app line was 276 of 278 twice, with 4 different names lost to the dispatcher loop and none
  lost twice. There was no red on an assertion.
- The engine line was 176 of 176 in 375 s.
- Floors: captures 37 of 37 in 92 s, adjudicated 13 of 13 in 29 s, and clean synthetics 0 of 2 as
  R53 expects.
- The transmit files were silent against `7e209cb4`.

`PHASE_OUTCOME.md` and `PHASE_STATUS.md` rode in whole, with the layer's uncommitted lines, as in
units 393 and 394.

**Task 1, `6eb97b4c` and `5688a8a5`.**
- **The list** (`.run-unit\unit395-list.sh`) has 84 Cw commits from `07f0397a` to 2026-09-03:
  - **39** are in the tree by R53's restore, marked *not a piece*.
  - **45** are the pieces, numbered oldest first, each with its date, the files it touched under
    `Cw`, its files outside `Cw` (named, not taken) and a clause of its claim.
  - The same log over the eleven transmit files printed nothing, over both ranges.
  - 4.1 is ticked in the root `PHASE_PLAN.md`.
- **The printer**, `Cw\TheReworkNumbersPrinterTests`, decodes the two captures exactly as
  `EachStillProducesWhatItDid` does, prints text, counts and five distances, and asserts nothing.
  At entry: WEEKEND 5, THINKING 5, FLEX 1 on 021410; ABOVE 2, BREEZE 2 on 013637.

**Task 2, the pieces.** Every piece was measured with its own build, the three floor types and the
printer, and a transmit check after each one.

| n | Hash | Kept or out | Piece commit | Revert commit |
|---|---|---|---|---|
| 1 | 2068f868 | out: nothing named moved; 4 captures fell | 5cf8f9c8 | 4fa4b593 |
| 2 | 6fc36a1e | out: nothing moved | 99db35fc, seam drop 0246f209 | 0d3e08a6 |
| 3 | 3e84ac74 | out: nothing moved | 8d454ab9, seam met 4b8a89a3 | c1868852 |
| 4 | 39a42c3f | out: already in the tree, nothing applied | none | none needed; row in 37a322aa |
| 5 | 9de394da | out: nothing moved | fa26b78b | 37a322aa |
| 6 | 3d4694e5 | out: doc comment only, judged with piece 5 | none | none needed; row in 248c6465 |
| 7 | 7fb89d5e | out: nothing moved | 1e72c135 | 248c6465 |
| 8 | 1bf4372d | out: nothing moved, as a pair with piece 7 | 433180c4 | cd9da33c |
| 9 | 44cf3fc8 | out: nothing moved, as a pair with piece 7 | 11ae2b56 | ed22ff7d |

**Task 3, `45b26f65`.** Exit round:
- The app line was 277 of 278 twice, with 1 different name lost to the dispatcher loop each run
  and no red on an assertion.
- The engine line was 176 of 176 in 372 s.
- Floors: captures 37 of 37 in 92 s with every case identical to entry, 13 of 13, and 0 of 2.
- The transmit files were silent. `git diff --stat 5688a8a5 HEAD -- src` printed nothing.

**No regression.** This report goes in one more commit. Every commit was pushed without refusal.

**Author's decisions applied:**
- **1**: step 4's entry was taken as satisfied.
- **2**: the list's two parts.
- **3**: Cw-only patches with transmit excluded by path. Hunks already in the tree were dropped
  for pieces 2 and 4.
- **4**: the seam was met once, on piece 3.
- **5**: pairs for pieces 8 and 9, and piece 6 judged with piece 5.
- **6**: judged against the numbers before the piece.
- **7**: every captures run was 92 to 95 s, far under 240 s.
- **8**: the printer.
- **9**: commits.
- **10**: the clock.
- **11**: timeouts as given.
- **12**: `PARKED.md`.

**Self-rulings: none.** No work outside the instruction's tasks was authorized.

**Decisions on how to carry out a task (uncapped), each reported:**
1. **The printer judges on the `CharacterSettled` text, not the harness's `CharacterDecoded`
   text.** `CwDecoder.Reading` is the last window only. The harness's member re-emits the leading
   edge on every revision (`FFRLELETT`, `NEVVENEN`). The settled text has the sidecar's shape
   (`FLENT 66O` beside the sidecar's `FLENX 66O`, `AB OV E`, `BR EE Z E`). The printer prints both
   texts and both sets of distances. **It was built twice, not once:** the first run found this,
   and the second added the settled distances.
2. **A hunk already in the tree was dropped in a follow-up commit** (`0246f209`), not by rewriting
   the pushed piece commit, and the revert took both commits.
3. **Pieces 4 and 6 made no piece commit.** Piece 4's one hunk is already in the tree. Piece 6 is
   doc comment only on piece 5's paragraph. Its pair with piece 5 compiles to piece 5's code,
   measured minutes earlier, so the pair was not rebuilt. Piece 4's tree is byte-identical to
   task 0's, so task 0's run is its measurement.
4. **The printer was not re-run at exit.** `src` is byte-identical to the entry tree, and the last
   printer run, the 7 and 9 pair's, printed the entry numbers.
5. **Dependency for piece 9 was tested with `git apply --check`** after piece 7 alone, which
   applied clean. So piece 9 needed one out piece and was tried as a pair, not listed as
   dependent.

**Section 5 mismatches** (held as stated unless listed):
- **The first table ends at `ca252057`, not `7e209cb4`.** `7e209cb4` touches no file under `Cw`.
  There are 39 Cw commits in `07f0397a..7e209cb4`.
- **The floor table's `012748` stood at 4 and 16 against its floor of 2 and 4 at entry**. The
  other 36 cases sat exactly on their floors.
- **The printer's 021410 text does not carry `ATEEKEND` or `TTHINKING`.** The sidecar words are
  the app's reading on the evening. The harness's settled text carries only the file's tail. So
  WEEKEND and THINKING stand at 5 and can move only if a piece changes what the tail reads.
- **`PHASE_STATUS.md`** read 0 and 394, as stated; its `STEP: 0` and `STEP: 2` lines are the
  layer's and I did not edit them. **`PHASE_OUTCOME.md`** holds steps 0 and 2 at `not started`,
  with the paired entries as stated; I did not edit them.
- **`CLAUDE.md` §1's top row reads HM-DEC-167** at line 360. `PROJECT_STATUS.md` says HM-DEC-165.
- Held: HEAD `ee0ea0dc`; 1.13.81 at line 1254; the 45 by day as counted; the Cw diff of 4 files,
  165 and 1; the floor rows at 107 and 108; the sidecar words; 918 lines in the carry-forward
  list; 21 and 19 `<Compile Remove>`; no `PARKED.md` and no `cw-retired-tests.txt` at entry; three
  preflight worktrees.

## 2. What the owner should expect

Nothing changed on the CW tab. The decoder is exactly as unit 392 restored it, and `src` shows no
change since the printer commit. The August rework now has a list: all 84 commits since the
decoder that read, 45 of them rework pieces, each with its files and what its message claimed. The
list is in `docs\phase-cw\unit395-rework.md`. Nine pieces went back in one at a time, and all nine
came back out. No piece moved a number you named. `021410` still stands at 47 characters, and
`013637` still reads `AB OV E` and `BR EE Z E`, ABOVE and BREEZE each at distance 2. Only the first
piece, *read the first seconds again*, changed anything at all, and it lowered four captures. Most
of the first nine are instruments, records or switched-off options, so *moved nothing* is what
their own messages predicted. **What will look wrong but is not:** the git log has sixteen
commits tonight that add and then remove decoder code. That is 4.3 working. Each out piece's revert
is the next commit after it. The next unit starts at piece 10, `4786c7e7`.

## 3. What you should see

**The answer: 9 of 45 pieces judged, 0 kept, 9 out, every out piece reverted, 4.1 met.**

| Piece | Hash | Claim | Number before | Number after | Captures wall | Kept or out |
|---|---|---|---|---|---|---|
| 1 | 2068f868 | read the first seconds again | 004507 50, 003758 63, 031948 34, 012748 4; 5 distances 5, 5, 1, 2, 2 | 49, 58, 31, 2; distances unchanged | 94 s | out |
| 2 | 6fc36a1e | log how close the argument was | entry | identical | 95 s | out |
| 3 | 3e84ac74 | let go of a pitch on a frequency left | entry | identical | 94 s | out |
| 4 | 39a42c3f | margin's share on the sheet | entry | identical, nothing applied | 92 s, task 0 | out |
| 5 | 9de394da | open two constants for a sweep | entry | identical | 94 s | out |
| 6 | 3d4694e5 | confirmation window stays at two | entry | with piece 5: identical | 94 s, piece 5 | out |
| 7 | 7fb89d5e | record which test refused which bin | entry | identical | 94 s | out |
| 8 | 1bf4372d | separation bound must not move | entry | with piece 7: identical | 94 s | out |
| 9 | 44cf3fc8 | both gate derivations, off | entry | with piece 7: identical | 94 s | out |

**Numbers before piece 1 and at exit, every capture:** identical, 37 rows, in the doc's section
3.1. The anchored cases are 004507, 003758 and 031948. 021410 was 47, 99, 11 unsure at 550 Hz, and
013637 was 63, 164, 3 at 550 Hz. The four empty captures read 0.

**The printer, at entry and at exit:**
- 021410's settled text is `■ ■ ■ M ■ ■ ■ ■ T O MTT T  Y M TT ■ ■ O AO IHI DT ■RIGHR IS ■ FLENT
  66OAM`. WEEKEND 5 (` FLEN`), THINKING 5 (`T ■RIG`), FLEX 1 (`FLE`).
- 013637's is `TE MP NEVEN T REV■R G O T AB OV E ■7 5 F ES ■CLEAR S KY LI TE BR EE Z E ALL DAY JUST
  AWE SO`. ABOVE 2 (`AB OV`), BREEZE 2 (`BR EE`).

**Carry-forward and floors:**

| Run | Entry | Exit |
|---|---|---|
| App line | 276 of 278 in 171 s; re-run 276 of 278 in 169 s; 4 names lost to the loop, none twice | 277 of 278 in 162 s; re-run 277 of 278 in 147 s; 2 names lost to the loop, none twice |
| Engine line | 176 of 176 in 375 s | 176 of 176 in 372 s |
| Captures | 37 of 37, 92 s | 37 of 37, 92 s, identical |
| Adjudicated | 13 of 13, 29 s | 13 of 13, 29 s |
| Clean synthetics | 0 of 2, R53 | 0 of 2, R53 |

`git worktree list`: the root and the three preflight trees, nothing else. `git diff --stat
5688a8a5 HEAD -- src` at the end prints nothing, because no piece was kept.

## 4. What's blocking us

**Nothing blocks a criterion of step 4.** The next unit takes piece 10 onward from the same list
under the same rules. Every ask carried from before this phase, and every earlier finding that
blocks nothing, is in `docs\phase-cw\PARKED.md` under R54. This unit's own findings and
mismatches are in section 1. None touches keying, transmit, money or a product fact, and none
needed parking.

**`validate-output.bat`:** not run, since it asked for approval in earlier units. I hand-checked
this file against its rules:
- The ordering block and the `UNIT:` block are above section 1.
- The `UNIT:` line has no parentheses and none of `& | < > ^`.
- It has the four canonical sections in order and no fifth.
- Section 3 is not empty.
