```
READ IN THIS ORDER.

A. The phase goal - CW decodes again. Steps 0, 1, 2 and 4 are ticked on
   every criterion; the outcome file holds 0 and 2 at not started and 4 at
   partial, a layer mismatch reported here, and the launcher named step 0
   for this unit on that reading; step 3 is this unit's, at 3.1, 3.2, 3.3
   and 3.5 met at entry with 3.4 open and 14 reds open in the set, every
   one a decode result, and all three floor tests green at entry; step 5
   is Tim's. After this unit the set stands at 43 green and 8 red-open,
   the known-reds block names no CW test, the failing set carries its
   closing line, the third floor test is on the engine line, and 3.4 is
   met on its letter with the step partial under R49.
B. The criteria, one line each, met or not: 3.1 met by unit 394, its
   sentence gaining #18 to #23; 3.2 met by unit 398, nothing retired here;
   3.3 met, nothing retired here; 3.4 met - the block's CW line names no
   test, the closing line reads 31/12/0/8, the engine line 178 of 178 in
   374 s; 3.5 re-confirmed - floors 37 of 37, 13 of 13 and 2 of 2 at
   task 4, lines app 278 of 278, engine 178 of 178 in 374 s, no file under
   src changed. The unit named 3.4 in ADVANCES as step 3 criterion 4; it
   flipped.
C. The report last. Section 4 raises 0 items and none is in the way of a
   criterion in B; everything carried from before this phase and every
   finding that blocks nothing is in docs/phase-cw/PARKED.md under R54,
   not here. Every non-owner criterion of the plan is now ticked; 5.1 is
   Tim's and no unit is authored toward it.
```

```
UNIT:       401 - complete at task 4 of 5, none dropped - 2026-09-23 05:27
PHASE GOAL: get the CW decoder that worked on the air in August working again, proved by three named floor tests, kept from breaking silently by a guard every unit runs, with the old pile of CW reds dealt with, and in the end confirmed by Tim at the radio
UNIT GOAL:  measure the six displacement reds and repair them in the test where the numbers allow, then close out the CW pile on the plan's own wording - the known-reds block names no CW test, the failing set carries its count each way, and the clean synthetics join the engine guard - with the floors unmoved and nothing under src changed
ADVANCED:   yes - step 3 criterion 4, the known-reds block names no CW test and the set carries its closing line
NUMBER:     displacement 0 of 6 -> 6 of 6 on both the settled event and band 0.005; set names green 37 -> 43, red-open 14 -> 8; known-reds CW lines 2 -> 0 naming a test; engine line 176 in 374 s -> 178 in 374 s of 480; floors 37 of 37, 13 of 13, 2 of 2 at both ends
DRIFT:      0
```

## 1. What Claude did

**Complete at task 4 of 5, tasks 0 to 4, none dropped.** Claude Code on Tim's Windows machine,
Hamlet confirmed at the gate, branch `main`. The unit's hour started at 04:57:08, the first status
line of task 0. Task 2 started at minute 7, task 3 at minute 11 and task 4 at minute 18, all inside
decision 11's clock. Every commit was pushed, and each push returned 0.

**Commits:**

| hash | what it changed |
|---|---|
| `bb854ac8` | task 0: `PHASE_OUTCOME.md` entry `## UNIT 401 - STEP 3` appended, `PHASE_STATUS.md` whole, version 1.13.87 to 1.13.88, `unit401-closeout.md` section 1 |
| `034bb82e` | task 1: `TheDisplacementFloorFourWaysTests.cs`, a printer asserting nothing; doc section 2 |
| `8ed230ad` | step 3 repair (a): `CwDisplacementFloorTests` line 45 reads `CharacterSettled`, 1 of 6 |
| `377eb7b3` | step 3 repair (b): the five silent cases at a band of 0.005, class-remark paragraph, 6 of 6; doc section 3 |
| `29ede63d` | task 3: the known-reds block, the closing line, line 9's term, the guard-table row, the `WHAT UNIT 401 CHANGED` paragraph; doc section 4 |
| `dd4d1d07` | ticks: 3.4 ticked, 2.3 and 3.1 clauses; doc section 6 |
| `162f0259` | task 4: exit round into doc section 5, `PARKED.md` 401 items 1 to 3, the outcome entry's task lines |

**Task 0, entry.** `PHASE_STATUS.md` read `CURRENT_STEP: 0` and `WORK_INSTRUCTION: 400 - the harness
reads the transcript`. It is now `3` and `401 - the pile is closed out on R49's letter`. It rode
whole in task 0's commit, as did `PHASE_OUTCOME.md`. Decision 6's diff (`git diff --stat 5e70860d
HEAD -- src tests docs/carry-forward-tests.txt docs/unit239-failing-set.txt`) printed nothing, so
unit 400's exit runs were the entry numbers for the two lines. One build of 16 s, warnings as
errors. Captures 37 of 37 in 94 s, every row identical to unit 400's exit. Adjudicated 13 of 13 in
29 s. Clean synthetics 2 of 2. `CwFixtureTests` 22 of 23 (`fading-18wpm` confident-mistakes red),
`CwDisplacementFloorTests` 0 of 6, `CwEmissionGateTests` 7 of 8. Transmit files and `src` printed
nothing. `grep -rl unit239-failing-set tools .claude` printed nothing.

**Task 1, the trace (decision 7).** 33 decodes plus #24, in 33 s. **Finding (a):** the settled
event alone turns the sixth case (noise 0.06) to `TV VVV VVV CQ DE W1AW K` and leaves every silent
case in blocks. **Finding (b):** 0.005 is the smallest band where all five silent cases end with the
call and the image case moves once; at 0.002 the 750 Hz case ends `OT W1AW K`. **#24:** 9
characters, reading `CQ DE W1AW K` at 18.46 wpm, `WordsPerMinute` null because `SpeedIsReacquiring`
is true. The cause is under `src`, so it was written down and left.

**Task 2, the repair (decision 8).** Both changes were reached and both kept. Nothing was put back.
(a) 0 of 6 to 1 of 6, #23 red to green, no case green to red. (b) 6 of 6. Before (b) was committed:
captures 37 of 37 with every row identical, adjudicated 13 of 13, and `CwFixtureTests` and
`CwEmissionGateTests` identical by case to task 0. No assertion, `Retunes` count or expected text
moved. The file changed only at line 45, the two silent call sites (five cases) and the class
remark.

**Task 3, the close-out (decisions 2, 3, 4, 9).** Every edit was made as the instruction gave it,
and the engine line with the new term ran 178 of 178 in 374 s of 480.

**Task 4, exit.** Both lines are green with no red on an assertion. The floors are unmoved, and so
is every measured type. No regression. 3.5 stands.

**Decisions applied**, all 12 as written: 1 (3.4 on its letter, step partial under R49), 2 (one CW
line naming no test), 3 (the term on line 9, inside 480 s, kept), 4 (the closing line, counts
summing to 51), 5 (no `src` change), 6 (entry lines from unit 400's exit), 7 (the printer), 8 (both
changes, each on its own evidence), 9 (the ticks), 10 (the doc, six sections), 11 (clock) and 12
(timeouts; nothing backgrounded). **Decisions I made for myself about how to carry out tasks:**

- 2.3's clause gives the engine line's count, 178 of 178 in 374 s, not the synthetics' own 2 of 2.
  The line's count is the one 2.3 measures.
- The new known-reds CW line takes the place of line 158. Because the guard table gained a row, it
  is now line 159.
- The printer prints `image` and `elsewhere-400` as separate rows even though they generate the same
  audio. The decision names six cases.
- Task 0's checks of the lines against unit 400 used `unit401-cmp.sh` and `unit401-adjcmp.sh`,
  copies of unit 400's scripts with only the unit number changed.

**No self-ruling that authorizes work outside the tasks was used.**

**Mismatches against the instruction**, reported and not repaired:

- `docs\carry-forward-tests.txt` was 918 lines at entry, not 919.
- `CwDisplacementFloorTests.cs` was 113 lines, not 114. Every quoted line number matched.
- The `CharacterDecoded` grep also hits `TheReworkNumbersPrinterTests.cs` line 18, a remark. None
  of the listed files is excluded from compilation.
- **The layer's `ADVANCES` misrecording:** `PHASE_OUTCOME.md`'s `## UNIT 6 - STEP 3` entry carries
  `ADVANCED: no` and `ATTEMPT: 3.3 ...` for unit 400, whose `ADVANCES` read `step 3 criterion 3.5`
  and whose 3.5 flipped. The launcher's parse stopped at the `.`. This unit's entry writes
  `ADVANCES: step 3 criterion 4`. The existing entry was not edited, and `tools\arbiter\` was not
  touched.
- The outcome file still holds steps 0 and 2 at `not started` and step 4 at `partial`, with every
  criterion `[x]`. That is the layer's, and it was not edited.
- `CLAUDE.md` §1's top row is at line 360, the 2026-09-22 CW-phase row, which the instruction calls
  HM-DEC-167. `PROJECT_STATUS.md` writes HM-DEC-165 as a literal (390 item 9).

**Regressions:** none.

## 2. What the owner should expect

Nothing changed on the CW tab, and nothing changed under `src`. The decoder that read on the air is
the same code, and its three floor tests are green. The list every unit runs before and after its
work now also guards the third floor test, the two clean synthetics, so they cannot go red again
without a unit seeing it. The file that told every unit since September that CW's reds were
inherited and not chased now says something else: all 51 were run, 12 were repaired (six by unit
400 and six here), none was retired, and 8 are owed by number under your R49. The six displacement
cases went green, 6 of 6. It took two changes to the test: it now takes the transcript the CW tab
shows, and its five silent cases now carry a band of 0.005 in place of exact digital silence. No
assertion moved. With 3.4 ticked, every criterion the loop can move is done, and 5.1 is yours at
the radio.

**What will look wrong but is not:**

- Step 3 is still partial. R49's own clause says a red decode result stays red with its number and
  the step partial. The 8 are #6, #15, #24, #41 and #42 to #45.
- At 0.005 the 750 Hz displacement case moves twice and shows blocks before the call. It is green
  because the test asserts only the ending (401 item 3).

## 3. What you should see

**The answer: 3.4 met, and the displacement type went from 0 of 6 to 6 of 6.**

**The printer's rows** (`TheDisplacementFloorFourWaysTests`, asserting nothing). Way 1 is the
leading edge at the test's noise, way 2 the settled transcript at the test's noise, ways 3 to 6 the
settled transcript at the band shown:

| case | way | band | Retunes | ends with call | text |
|---|---|---|---|---|---|
| image / elsewhere-400 / 500 / 750 / 875 | 1 | 0 | 1 | no | `■ ■■ ■` |
| same five | 2 | 0 | 1 | no | `■ ■ ■ ■ ■ ■ ■A ■ ■ ■■` |
| image, 400, 875 | 3 | 0.002 | 1 | yes | `T TEEV VVV VVV CQ DE W1AW K` |
| 500 | 3 | 0.002 | 1 | yes | `■ ■ T TEET ETET ... E■ CQ DE W1AW K` |
| 750 | 3 | 0.002 | 1 | **no** | `■ ■ T TTTT ... ■■ OT W1AW K` |
| image, 400, 875 | 4 | 0.005 | 1 | yes | `T TEEV VVV VVV CQ DE W1AW K` |
| 500 | 4 | 0.005 | 1 | yes | `■ ■ ■T TEET ... EET■■ CQ DE W1AW K` |
| 750 | 4 | 0.005 | 2 | yes | `■ E ■ E I ■ ■ 5EE E ■E EE ■ VVV CQ DE W1AW K` |
| image, 400, 750, 875 | 5 | 0.01 | 1 | yes | `TV VVV VVV CQ DE W1AW K` |
| 500 | 5 | 0.01 | 1 | yes | `■ ■ ■ EEET VVV VVV CQ DE W1AW K` |
| image, 400, 750, 875 | 6 | 0.02 | 1 | yes | `TV VVV VVV CQ DE W1AW K` |
| 500 | 6 | 0.02 | 1 | yes | `T TEEV VVV VVV CQ DE W1AW K` |
| refused-before | 1 | 0.06 | 1 | no | `5V H VEVVVSVV I ... WWAJ11AARW W N K` |
| refused-before | 2 | 0.06 | 1 | yes | `TV VVV VVV CQ DE W1AW K` |
| refused-before | 6 | 0.02 | 1 | yes | `TV VVV VVV CQ DE W1AW K` |

Every row, case by case, is in `docs/phase-cw/unit401-closeout.md` section 2.

**`CwDisplacementFloorTests`:**

| case | task 0 | after (a) `8ed230ad` | after (b) `377eb7b3` |
|---|---|---|---|
| `TheTrackerDoesNotLeaveAStationForItsOwnImage` #18 | red | red | green |
| `AStationElsewhereIsStillFound(400)` | red | red | green |
| `AStationElsewhereIsStillFound(500)` | red | red | green |
| `AStationElsewhereIsStillFound(750)` | red | red | green |
| `AStationElsewhereIsStillFound(875)` | red | red | green |
| `NothingIsRefusedBeforeAnythingIsBeingRead` #23 | red | green | green |

**#24's four numbers:** `Report.CharactersEmitted` 9, `Reading.WordsPerMinute` 18.46,
`WordsPerMinute` null, `SpeedIsReacquiring` true. #24 stays red-open, and its cause is under `src`
(401 item 1).

**The known-reds block's CW lines, before:**

```
    CwAdjudicationTests.ASpeedChangeInRealisticAudio
    the 51 CW cases in docs/unit239-failing-set.txt
```

**After:**

```
    CW: none. The 51 names of docs/unit239-failing-set.txt were run, classified and closed out by the CW phase under R49 (units 394, 398, 400 and 401): its closing line carries the count each way, every red-open name stands with its number in docs/phase-cw/unit394-reds.md section 2, each is a repair owed under R49, not an inherited red, and none is on either line above.
```

The exit grep of `Cw\|CW` over the file hits lines 9, 27, 28, 159, 882, 900 to 916 and 920 to 929.
None of the hits names a CW test as a known red.

**The closing line**, line 52 of `docs/unit239-failing-set.txt`. It is the file's one added line:

```
# Closed out by the CW phase, CW decodes again, 2026-09-23, units 394 to 401: of the 51 names above, 31 green at HEAD without repair, 12 repaired (#25 #26 #30 #31 #32 #33 by unit 400, and #18 #19 #20 #21 #22 #23 by unit 401), 0 retired, 8 red-open with their numbers in docs/phase-cw/unit394-reds.md section 2 (#6 #15 #24 #41 #42 #43 #44 #45), each a repair owed under R49 and HM-DEC-151.
```

**Line 9's appended term**, the 28th: `|FullyQualifiedName~CwFixtureTests.TheCleanRecordingsDecodeExactly`
inside the closing quote. **The guard-table row**, line 28:
`CwFixtureTests.TheCleanRecordingsDecodeExactly, the two clean synthetics   engine   unit 401`.

**The runs:**

| run | entry | exit |
|---|---|---|
| app line | 277 of 278 twice on dispatcher losses, unit 400's exit via decision 6 | 278 of 278 in 156 s |
| engine line | 176 of 176 in 374 s, unit 400's exit via decision 6 | 178 of 178 in 374 s; also 178 of 178 in 374 s at task 3 |
| captures | 37 of 37 in 94 s | 37 of 37 in 92 s, every row identical |
| adjudicated | 13 of 13 in 29 s | 13 of 13 in 29 s |
| clean synthetics | 2 of 2 | 2 of 2 |
| `CwFixtureTests` | 22 of 23 | 22 of 23, identical by case |
| `CwEmissionGateTests` | 7 of 8 | 7 of 8, identical by case |

Decision 6's diff: `git diff --stat 5e70860d HEAD -- src tests docs/carry-forward-tests.txt
docs/unit239-failing-set.txt` printed nothing.

At exit:

- `git diff --stat 5688a8a5 HEAD -- src`: nothing.
- The eleven transmit files against `7e209cb4`: nothing.
- `git diff --stat bf7bae57 HEAD -- tests docs`: exactly `carry-forward-tests.txt`, `PARKED.md`,
  `unit401-closeout.md`, `unit239-failing-set.txt`, `CwDisplacementFloorTests.cs` and
  `TheDisplacementFloorFourWaysTests.cs`.
- `git status --short tests`: nothing.
- `git worktree list`: the root and the three preflight trees.

**The tick and clause sentences as written:**

- **3.4:** *Unit 401: lines 158 and 159 of the known-reds block replaced by one CW line naming no
  test and pointing at the set's closing line and `unit394-reds.md`; grep over the file at exit finds
  no CW test named as a known red; the closing line written - 51 names, 31 green at HEAD without
  repair, 12 repaired, 0 retired, 8 red-open by number (#6, #15, #24, #41, #42, #43, #44, #45); the
  step's goal sentence stays partial on the 8 red-open under R49's own clause, red with its number and
  the step partial, each a repair owed under HM-DEC-151; `CwFixtureTests.TheCleanRecordingsDecodeExactly`
  on the engine line, 178 of 178 in 374 s of 480.*
- **2.3 clause:** *the two clean synthetics joined the engine line by unit 401 once green, 178 of 178
  in 374 s.*
- **3.1 clause:** *#18 to #23 green by unit 401 on the settled transcript and a band of 0.005, the
  set at 43 green and 8 red-open.*
- **3.5:** re-confirmed at task 4 with nothing red that was green. Its sentence stands unedited.

## 4. What's blocking us

Nothing. No criterion of step 3 waits on a ruling. This unit's findings that block nothing are
`401 item 1` to `401 item 3` in `docs/phase-cw/PARKED.md`, under R54: #24's cause under `src`, two
line-count mismatches, and the 750 Hz case's two moves at 0.005.
