```
READ IN THIS ORDER.

A. The phase goal - CW decodes again. Step 0 is ticked on all four criteria
   and its state line still reads not started, a layer mismatch reported
   here; step 1 was partial at entry on 1.5 alone; step 2 is this unit's
   second half; steps 3 to 5 are not started. After this unit step 1 is
   done and step 2 is done.
B. The criteria, one line each, met or not:
   1.5 met - engine 150 of 150 at task 0; app line after the repair 277 and
       275 of 278, every loss the dispatcher loop and green in the other run,
       no red on an assertion; TheOliviaRowsTests green in every run.
   2.1 met - line 9 carries the adjudicated type and the thirteen 08-25
       captures, and the guard table has a CW row naming them and unit 393.
   2.2 met - 20 of 26 red against a CwDecoder that hands nothing on, every
       case that asserts a decode; 26 of 26 green with the file put back.
   2.3 met - engine line 301 s before, 372 s and 373 s after, of 480.
   2.4 met - app 278 of 278, engine 176 of 176 at exit, nothing red that was
       green at task 0.
C. The report last. Section 4 raises 23 items - unit 390's nine, unit 391's
   five and unit 392's four carried, and this unit's own five - and none is
   in the way of a criterion in B. Item 1 is a regression the guard caused
   and this unit repaired by a self-ruling you may overrule.
```

```
UNIT:       393 - complete at task 4 of 5, tasks 0 to 4, none dropped - 2026-09-22 21:47
PHASE GOAL: get the CW decoder reading again from the last code that read, make sure no later unit can break it without the list going red, clear the inherited reds, judge the August rework on numbers, and end with Tim hearing it read at the radio
UNIT GOAL:  get both carry-forward lines green again by fixing the Olivia rows test's file read, which closes step 1, then put a CW read guard on the engine line, show it refusing a broken decoder, and show the line still fits its 480 s, which closes step 2
ADVANCED:   yes - step 1's last criterion and all four of step 2's are ticked on measured runs, and CW now has a read guard on the list every unit runs
NUMBER:     app line red on an assertion-free IOException: 1 -> 0; CW names on the engine line: 0 -> 2 terms, 26 cases; engine line wall time 301 s -> 373 s of 480
DRIFT:      0
```

## 1. What Claude did

**Complete at task 4 of 5: tasks 0 to 4 done, none dropped.** Claude Code on the dev machine,
Hamlet confirmed at the gate, branch `main`. Started 21:00, the last task committed at 21:46.
Five commits, all pushed: `05b729eb`, `d6dbe8e2`, `bbefa9d7`, `f180cdb6`, `e09608c4`.

**Task 0, the record and the entry round.** Version 1.13.79 to 1.13.80. `PHASE_STATUS.md` read
`CURRENT_STEP: 0` with `WORK_INSTRUCTION: 392 - the decoder reads again`. I set it to
`CURRENT_STEP: 1` and `393 - the list is green, and CW is on it`. I appended `## UNIT 393 - STEP
1` to `PHASE_OUTCOME.md` in unit 392's shape, with a line per task added as each finished. At
entry the app line was 278 of 278 in 170 s. The race did not land, and nothing was lost. The
engine line was 150 of 150 in 301 s. The floors were 37 of 37 in 97 s, 13 of 13 in 33 s, and 0
of 2 in 7 s, the synthetics reading `■` placeholders. The eleven transmit files printed nothing
against `7e209cb4`.

**Task 1, the trace, then the repair (1.5).** The two share modes, as the sources have them:
- **The writer:** `JsonlTelemetry.cs:188`, `File.AppendAllText(path, line + Environment.NewLine);`,
  inside `WriteLoop` on the background thread started in the constructor. It opens the file for
  writing and shares read.
- **The reader:** `TheOliviaRowsTests.cs:675`,
  `Directory.GetFiles(folder, "*.jsonl").SelectMany(File.ReadAllLines)`. It is called at line 306,
  `var before = Lines(folder);`, inside the `using` of line 279 that owns the writer. It opens for
  reading and shares read only. That refuses a handle already open for writing.

Alone before the edit: 7 of 7 in 99 s. The edit: `Lines` now opens each file with `new
FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)` under a `StreamReader`
and reads line by line to the end. No assertion or case moved. `git diff --stat` showed that
one file under `tests` and nothing under `src`. The app line then ran 277 of 278 in 160 s. One
name was lost to the dispatcher loop: `TheRecordNamesTheSubModePressedTests.TheCqPressWritesTheLabelTheOperatorPressed`
Olivia, the same name unit 392 lost. I re-ran it once: 275 of 278 in 167 s, with three
`TheFavoritesAreUnderTheGreenZoneTests` names lost the same way. Each lost name was green in the
other run. No run had a red on an assertion, and `TheOliviaRowsTests` was green in both. 1.5 is
ticked.

**Task 2, the guard on line 9 (2.1, 2.3).** Decision 2's clause was appended, taking line 9 from
25 terms to 27. The guard table has its CW row, and the foot has a `WHAT UNIT 393 ADDED`
paragraph. **The first engine run was 175 of 176 in 300 s, and the red was not CW.**
`TheOliviaDemodulatorTests.TheMinusTenDecibelFixtureDecodes` failed its CPU ceiling:
`demodulator cpu 26.563 s` against 20. The decode itself was perfect: CER 0, 251 of 251
characters. The ceiling reads process CPU, and that type is not in the `CpuMeasuredAlone`
collection, so it measured the CW cases running beside it. It was green at task 0 and red here
on an assertion. **By HM-DEC-165 that is a regression the guard caused.** Per the rule, it was
not re-run. Self-ruling 1 below repaired its wiring. The line was then measured again as a
changed tree: 176 of 176 in 372 s, under decision 3's 420 s. 2.1 and 2.3 are ticked. All 26 CW
cases on the line passed. The line runs at normal verbosity, so the 26 are counted as 176 less
150, and each is named in task 3's runs.

**Task 3, the guard watched (2.2).** I inserted one line into `CwDecoder.Process` after the tap
takes the chunk: `if (chunk.Samples.Length >= 0) return;`. It is written as a condition because
the build treats warnings as errors and would refuse a bare `return` above reachable code. It
touches no transmit file. `git diff --stat HEAD -- src` showed `CwDecoder.cs` alone. Red run: 20
of 26 red in 6 s. Green run, with the file put back by `git checkout HEAD --`: 26 of 26 in 32 s.
`git status --short src` printed nothing after the put-back and after the green run, and the
break was never committed. The six that stayed green in the red run cannot go red on any
decoder. Five are readings Tim retired on 2026-08-30, which the test prints as `RETIRED` and does
not assert. The sixth is `TheShortfallIsPrintedRatherThanPapered`. Everything else is in
`docs/phase-cw/unit393-guard.md`.

**Task 4, the exit round (2.4).** The app line was 278 of 278 in 159 s, and the engine line 176
of 176 in 373 s, nothing lost. The floors were 37 of 37 (runner 98 s), 13 of 13 (runner 31 s)
and 0 of 2 (5 s). The transmit files printed nothing against `7e209cb4`, and `git diff --stat
HEAD -- src` printed nothing. 2.4 is ticked.

**Regressions:** one, named. `TheOliviaDemodulatorTests.TheMinusTenDecibelFixtureDecodes`, red
in task 2's first engine run on its CPU ceiling, green at task 0. It was repaired in the same
task, and it is green in the second task 2 run and at exit. Nothing else was red that had been
green at task 0.

**Author's decisions applied, all seven:**
1. The repair is the reader's share mode, in the test only, as written.
2. The guard is the adjudicated type whole plus the thirteen 08-25 captures by display name, as
   written.
3. Fit is 420 s. The line measured 372 s and 373 s, so the 08-25 clause stayed on.
4. The break is one hunk in `CwDecoder.cs`, never committed, watched by the guard's own filter.
5. Step 2's entry was taken as satisfied at task 1's tick, with the floors green at task 0.
6. 1.5 was ticked at task 1 and re-confirmed at task 4.
7. The timeouts were as given. Every run finished inside its own.

**Self-rulings, one of two used:**
1. **`TheOliviaDemodulatorTests` into the `CpuMeasuredAlone` collection.** The change is one
   `[Collection(CpuMeasuredAlone.Name)]` attribute and a remark saying why, in
   `tests/Hamlet.RadioEngine.Tests/Olivia/TheOliviaDemodulatorTests.cs`. No ceiling, case or
   assertion moved. It cites `PHASE_PLAN.md` 2.4 and R12, and the collection's own remark at
   `TheOliviaBlindSearchTests.cs:186-192`: *a ceiling read off a shared clock measures the
   neighbors*. It rode in the task 2 commit, so that commit is not the carry-forward file alone,
   as the instruction asked. It costs wall time: the type now runs after every parallel
   collection, alone. The 372 s includes that.

**Decisions on how to carry out tasks, reported:**
- I ran each carry-forward line in its own tool call, because the harness caps one call at
  600 s.
- `PHASE_OUTCOME.md` and `PHASE_STATUS.md` were committed whole at task 0. See section 4, item 2.
- I added one line per task to this unit's `PHASE_OUTCOME.md` entry, as unit 392 did.

## 2. What the owner should expect

Nothing changes on the CW tab or anywhere else you can see. The decoder is the same one unit 392
restored, byte for byte. A test that read its own log while the log was still being written now
reads it in a way the writer allows, so it no longer fails for no reason on a busy machine. From
this unit on, every unit that runs the list will see CW go red if the decoder stops producing
what it produced on the evening of 2026-08-25, or loses one of the anchors you confirmed. That
is a count, not a claim that it reads, and saying it reads is yours at step 5. **What will look
wrong but is not:** the engine line now takes about 6 minutes instead of 5, and one Olivia test
type now runs by itself at the end of that line, because its CPU ceiling was reading CW's work
as its own. The two clean synthetics are still red. They are step 3's, and they stay off the
list until they are green.

## 3. What you should see

**No visible change. This unit makes the list catch a CW regression from here on.**

**Every carry-forward run this unit made:**

| When | Line | Count | Wall time | Lost before an assertion | Red on an assertion |
|---|---|---|---|---|---|
| Task 0 | app | 278 of 278 | 170 s | none | none |
| Task 0 | engine, 25 terms | 150 of 150 | 301 s | none | none |
| Task 1 | app | 277 of 278 | 160 s | `TheRecordNamesTheSubModePressedTests.TheCqPressWritesTheLabelTheOperatorPressed` Olivia | none |
| Task 1, re-run once | app | 275 of 278 | 167 s | `TheFavoritesAreUnderTheGreenZoneTests`: `TheStarIsDrawnAndHittable...`, `PressingTheStarSaves...`, `TheWayBackInCostTheTopBandNothing` | none |
| Task 2 | engine, 27 terms | 175 of 176 | 300 s | none | `TheOliviaDemodulatorTests.TheMinusTenDecibelFixtureDecodes`, CPU 26.6 s of 20 |
| Task 2, after self-ruling 1 | engine, 27 terms | 176 of 176 | 372 s | none | none |
| Task 4 | app | 278 of 278 | 159 s | none | none |
| Task 4 | engine, 27 terms | 176 of 176 | 373 s | none | none |

**Task 3, the guard alone,** `timeout 300`:

| Case | Floor or anchor | Broken decoder | Put back |
|---|---|---|---|
| 08-25-013520 | 60 chars, 153 elements | red, 60 to 0 | green |
| 08-25-013637 | 63, 164 | red, 63 to 0 | green |
| 08-25-012922 | 50, 112 | red, 50 to 0 | green |
| 08-25-013402 | 61, 161 | red, 61 to 0 | green |
| 08-25-013150 | 58, 139 | red, 58 to 0 | green |
| 08-25-013010 | 54, 131 | red, 54 to 0 | green |
| 08-25-021825 | 41, 74 | red, 41 to 0 | green |
| 08-25-012748 | 2, 4 | red, 2 to 0 | green |
| 08-25-012823 | 41, 62 | red, 41 to 0 | green |
| 08-25-013303 | 54, 146 | red, 54 to 0 | green |
| 08-25-011552 | 30, 89 | red, 30 to 0 | green |
| 08-25-021410 | 47, 99 | red, 47 to 0 | green |
| 08-25-021629 | 47, 96 | red, 47 to 0 | green |
| 08-17-013347 | `VA3VRR` | red, not found in "" | green |
| 08-18-003758 | `MP/4 QNIK` | red, not found in "" | green |
| 08-24-012403 | `DE KD0UN KD0UN K` | red, not found in "" | green |
| 08-18-004507 | `N HANDLING THIS MESSAG` | red, not found in "" | green |
| 08-22-031838 | `, AND` | red, not found in "" | green |
| 08-22-031948 | `110, AND 110 W...` | red, not found in "" | green |
| 08-22-032012 | `R OTHER WEBSITES MENTI` | red, not found in "" | green |
| 08-17-134712 | `N4`, retired 2026-08-30 | green, not asserted | green |
| 08-22-031905, 032050, 032113, 032129 | retired 2026-08-30, squelch | green, not asserted | green |
| `TheShortfallIsPrintedRatherThanPapered` | none | green | green |
| **Total** | | **20 of 26 red, 6 s** | **26 of 26 green, 32 s** |

**The three floor tests:**

| Type | Entry | Exit |
|---|---|---|
| `TheCapturesThatDecodeKeepDecodingTests` | 37 of 37, 97 s | 37 of 37, runner 98 s |
| `TheAdjudicatedReadingsKeepReadingTests` | 13 of 13, 33 s | 13 of 13, runner 31 s |
| `CwFixtureTests.TheCleanRecordingsDecodeExactly` | 0 of 2, 7 s, reading `■` placeholders | 0 of 2, 5 s, the same |

Entry times are wall times measured by the script. Exit times for the first two are the test
runner's totals, because the script's wall-time line was cut from the kept console.

## 4. What's blocking us

**Nothing blocks a criterion. Item 1 is a regression this unit caused and repaired by a
self-ruling; the rest are findings. Unit 392's item 1 is answered by this instruction, its option
A in the order of its option C, and is dropped from the queue. The rest of the queue is carried
per HM-DEC-139, below.**

**1. Putting the CW guard on the engine line turned an Olivia test red, and I moved that test's
type into `CpuMeasuredAlone` to repair it.** *Self-ruling 1, author's, overrulable.*
`TheOliviaDemodulatorTests.TheMinusTenDecibelFixtureDecodes` asserts the demodulator uses under
20 s of CPU, measured as process CPU. Beside the CW cases it read 26.563 s with every character
correct. It was not re-run. With the type in the collection the list's own Olivia types already
use, it is green at 372 s and 373 s.

| | Ruling | For | Against |
|---|---|---|---|
| **A** | Keep it: the type runs alone | Same fix and same reason as its three Olivia neighbors; no ceiling moved | The line went from 300 s to 372 s between the two task 2 runs, most of it this |
| **B** | Put the CW guard types in a collection of their own instead | Olivia stays parallel | Changes wiring on the guard, and still costs time |
| **C** | Measure the demodulator's own thread CPU rather than the process's | The ceiling then means what it says | A test-shape change beyond wiring, and a larger edit |

**Industry standard:** C, since a per-thread measure is the honest one. **My recommendation:** A,
which is already in and matches the file's precedent. The instruction's section 5 check on
`CpuMeasuredAlone` covered `TheOliviaBlindSearchTests`, `TheOliviaDriftTests` and
`TheOliviaBelowTheNoiseTests` but not this type. So decision 2's claim that the CPU-ceilinged
Olivia names run after the parallel collections was true for three types and not for the fourth.

**2. Section 5 mismatches:**
- **`docs\carry-forward-tests.txt` ended at line 897,** not 898. The last line is unit 390's
  `TheRstIsYoursToCorrectTests` paragraph, as stated.
- **`TheOliviaMoveUpTests` calls `Lines()` at more sites than listed.** There are also calls at
  837, 1019, 1114 and 1253, and a direct `File.ReadAllLines` at 1230. `Lines()` is defined at
  1367.
- **`125941` is not an 08-25 case.** It is `cw-2026-08-26-125941`, so the guard's display-name
  match does not select it. Task 3's note that it "stays green" does not apply. All thirteen
  08-25 cases have floors above nought, and all thirteen went red.
- **`PHASE_STATUS.md` read `CURRENT_STEP: 0`,** as the instruction said, and is now 1. Its
  `STEP: 1` and `STEP: 2` lines still read `partial` and `not started`. Those are the layer's, and
  I did not edit them.
- **I committed `PHASE_OUTCOME.md` and `PHASE_STATUS.md` whole.** Task 0 asks the unit to write
  both, and git commits a file whole. So the layer's uncommitted `## UNIT 1 - STEP 1` entry and
  `HEARTBEAT` line went into `05b729eb` with this unit's changes. I edited neither.
  `RUN_LEDGER.md`, `WORK_INSTRUCTIONS.md` and the three `tools\arbiter\` entries are left as found
  and uncommitted. `PROJECT_STATUS.md` is written by `tools/status.sh` and stays uncommitted.
- **`PHASE_OUTCOME.md` holds step 0 at `not started`** with all four of its criteria `[x]`, and
  carries both `## UNIT 392 - STEP 1` and `## UNIT 1 - STEP 1`. As stated, reported, not edited.
- **`CLAUDE.md` §1's top row reads HM-DEC-167.** `PROJECT_STATUS.md` says HM-DEC-165, which
  `tools/status.sh` writes as a literal.
- **Held as stated:** HEAD `e7c036fc`; 1.13.79; `PROJECT_STATUS.md` at unit 392, `COMPLETED`,
  `TASK 4 of 6`; the five root files and the three `tools\arbiter\` entries; `TheOliviaRowsTests`
  lines 279, 306 and 674-675; `JsonlTelemetry` lines 30, 62, 146-159 and 188, and no `FileShare`;
  `TheOliviaExportSaysOliviaTests` at 582; line 7 with 65 terms, line 9 with 25 and `timeout
  480`; the guard table; the known-reds block; the thirteen 08-25 passes in
  `unit392-floors-1.txt`; `CpuMeasuredAlone` at 193-194 with `DisableParallelization = true`;
  the Cw diff of 4 files, 165 and 1; the transmit files silent; 21 and 1 `<Compile Remove>`;
  51 lines in the failing set; no `cw-retired-tests.txt`; three preflight worktrees.

**3. Two sibling helpers read the telemetry file the same way and are on neither line.**
`TheOliviaMoveUpTests.Lines()` and `TheOliviaExportSaysOliviaTests` at 582 both use
`File.ReadAllLines` on a file `JsonlTelemetry`'s writer may hold. They can fail the same way on a
busy machine. I did not edit them. The same one-method fix applies if either goes on a line.

**4. Six of the guard's 26 cases can never go red.** *A finding.* Five adjudicated readings carry
a `Retired` reason from your rulings of 2026-08-30 and are printed, not asserted, and
`TheShortfallIsPrintedRatherThanPapered` asserts no decode. They cost about a second. The 20
cases that do assert all refused the broken decoder.

**5. The headless dispatcher loop lost 4 names across the two task 1 app runs,** and none in
the entry or exit runs. *A finding, recorded and not chased (§6).* None was lost twice.

**`validate-output.bat`:** not run. It asked for approval in earlier units. I checked this file
against its rules by hand: the ordering block and `UNIT:` above section 1; a `UNIT:` line with no
parentheses and none of `& | < > ^`; four sections in order with the canonical names; section 4
present.

**`git worktree list`:** the root and the three preflight trees, nothing else.
**`git diff --stat HEAD -- src` at the end:** prints nothing. **Push:** all five commits pushed
without refusal.

### Asks still outstanding

**Carried per HM-DEC-139: unit 392's items 2 to 5, unit 391's items 2 to 6 and unit 390's nine,
verbatim. None is this unit's to answer. Unit 392's item 1 is answered by this instruction and
dropped.**

**Unit 392's items 2 to 5, verbatim:**

**2. Section 5 mismatches:**
- **HEAD at entry was `a1fd388c`,** one arbiter commit past the `4baf986c` the instruction names.
  `src` and `tests` were identical to `3d6a2c12` either way.
- **`CLAUDE.md` §1's top row reads HM-DEC-167.** That is neither `PROJECT_STATUS.md`'s
  HM-DEC-165, which `tools/status.sh` writes as a literal, nor the reload's `CPS-DEC-0167`. I did
  not edit `CLAUDE.md`.
- **`CwProbabilisticDecoder.Envelope` exists at `7e209cb4`.** The instruction lists it among the
  members to check. It was never a seam; the absent `Envelope` is a record in `CwReferenceDecoder`.
- **Keeping `CwElementPitch` could not make `ElementPitchLine` build.** Its input,
  `CwProbabilisticResult.Elements`, comes from HEAD's decoder (section 1, decision 4).
- **`tools/Hamlet.PitchRank` is in `Hamlet.sln` and uses seven HEAD-only names.** The
  instruction does not mention it.
- **The clean synthetics do not read the empty string at the restored decoder.** They read `■`
  placeholders.
- **Held as stated:** version 1.13.78; `PROJECT_STATUS.md` read unit 391; the 45 commits
  `1a84188e` down to `2068f868`; 20 files, 6684 insertions and 98 deletions; the ten `A` rows as
  named; the eleven transmit files identical; `tests/Hamlet.RadioEngine.Tests/Cw`, 34 files; the
  four 1.4 tests exist; `docs\unit239-failing-set.txt` has 51 lines;
  `docs\cw-retired-tests.txt` absent and not created; three preflight worktrees.

**3. Three of my own decisions that you may want to reverse.** *Author's, overrulable.* Each
is in section 1:
- `pitch-rank` taken out of the solution build.
- The queue counters return nought.
- The element-pitch line now says *not measured*.

The first two are one-line reversals. The third is step 4's to bring back with the rework's
elements if they earn their place.

**4. Task 5 dropped.** It was the named drop candidate, dropped on the clock rule. The failing
set at the restored decoder is unmeasured, and step 3 inherits it. Four previously
uncompilable tests now compile against thin seams: `NoCwDecodeInDigitalModeTests`,
`AHeldPitchDoesNotOutliveItsEvidenceTests`, `WhereAcquisitionPointsTests` and
`TheSpanRatioReachesTheSidecarTests`. None has been run.

**5. The headless dispatcher loop cost 8 lost runs across the four app-line invocations.** *A
finding.* The lost tests were never the same twice, and each passed in the other run. It is the
§6 lost run, recorded and not chased.

**Unit 391's items 2 to 6, verbatim:**

**2. Section 5 mismatches:**
- **The failing set has 2 cases of `EachStillProducesWhatItDid`, not six** (`001520`, `013637`).
  The two clean synthetics are there as stated.
- **The floor table has 37 rows.** The class's own remarks say "thirty-six here".
- **The Cw source changes are not "2026-08-28 to 08-31 and once on 09-03".** `git log` since
  08-24 shows 51 commits on every day from 08-24 to 08-31, and **four** on 09-03 (`43efc525`,
  `865e66d8`, `9c2a7f99`, `1a84188e`).
- **The green commit isn't between 08-25 and 08-28.** Nothing since 08-24 is green on all three.
- **`PHASE_PLAN.md` §6's fallback reads "since 2026-08-25";** task 3's reads "back to 2026-08-24".
- **HM-DEC-166 has no row in `CLAUDE.md` §1.** The top row before this unit was HM-DEC-165. I
  added HM-DEC-167's row only.
- **Held as stated:** `PROJECT_STATUS.md` read unit 390; `Directory.Build.props` read 1.13.77;
  every one of the 37 captures is on disk; the three tests exist by those names with 37, 13 and 2
  cases. The known-reds block carries two CW entries: `CwAdjudicationTests.ASpeedChangeInRealisticAudio`
  and "the 51 CW cases in docs/unit239-failing-set.txt".

**3. The captures type takes 1995 s at HEAD, against 97 s at `7e209cb4`.** *An indication, one
run each; it bears on step 2.* Criterion 2.3 measures the guard against the engine line's
`timeout 480`. At today's speed the whole type can't go on that line. §6 already rules that
a test over 300 s never does.

**4. HM-DEC-155, bent and said so.** The harness caps a foreground call at 600 s, so the two
long captures runs and the probe ran in the background. I waited with one bounded loop per run.
The synthetics-only walk overlapped the second HEAD captures run in a second tree, which only
costs time: results were identical case for case against the first run. If the rule should bind
here as written, a type over 600 s needs a different runner.

**5. Three worktrees under `C:/Users/TimDi/preflight-trees/` were there before this session.**
One is at `07f0397a`, the commit named for 0.2. I didn't make them and didn't touch them. By the
instruction's own reasoning they're "a second tree the next unit can edit by mistake".

**6. `PHASE_PLAN.md` 0.1 to 0.4 are not ticked.** The instruction didn't ask me to; the judge
ticks them.

**Unit 390's queue as unit 391 carried it, verbatim:**

**Carried per HM-DEC-139, verbatim, from unit 390. None is CW, none is this phase's, and none is
this unit's to answer.**

**Nothing blocks. Items 1 to 3 are ticks you may want to reverse; the rest are findings.**

**1. 0.1 is ticked on this instruction's word, which overrules instruction 386 section 6 ruling 1 ("never ticked by a unit of this phase").** *A ruling request if you disagree.* The later instruction wins under `PHASE_PLAN.md` §6, and both were the author's and overrulable. The tick says what is true: no such commit exists (119 commits, 0 on a settings path). **Option A:** keep the tick as *met as a negative*. **Option B:** untick it and reword 0.1 so a completed negative meets it. **Industry standard:** B, because a criterion should be met by its own words. A was taken because the instruction said so.

**2. 9.2 and 9.4 are ticked "as built", and their words don't all match what was built.** *Findings; rewording is yours.*
- **9.2** says *Report, Confirm, the canned lines and the typed line are held - greyed with he is still sending - until his carrier drops*. As built, it would read: *the typed line is greyed with he is still sending, Report and Confirm are not offered and the canned lines give way to one note, until his hand-back or his carrier drops*.
- In 9.2 and 9.4, *replayed from the ... record* would read *replayed from a fixture built to that record's shape*. Your `2026-09-21.jsonl` is not on this machine.

**3. 10.3 is ticked with one case under the band's full height:** 327 × 178 at 1400 on PSK31 with the dial off 14.070, where the card also has to say *PSK31 lives at 14.070*. The instruction stated this case and asked for the tick. Untick it if the case matters to you.

**4. Tonight's ruling A and the 2026-09-07 read-only ruling are in no decision record.** *A mismatch against section 5, which says "HM-DEC- number: find it".* **The 2026-09-07 ruling has no HM-DEC number.** It lives only in `LogContactViewModel`'s remarks and the dialog's markup comment. Ruling A is now recorded in `PHASE_PLAN.md` 9.3, the code remarks and the commit. Neither is in `DECISIONS.md`, and I did not assign an id (§4.8).

**5. 10.6's ticked words (*one control reading Favorites*) now describe the drop-down this unit replaced.** Rewording is yours.

**6. `src\Hamlet.App\Controls\FavoritesDropDownControl.cs` should be deleted.** Nothing places it. It stays only because `Unit388TraceTests` names the type and `rm` is refused here. It is marked as off the window in its own remarks.

**7. Section 5 mismatches:**
- **7.5 was already `[x]`** although the instruction says it was never built. The tick was early (task 1's finding).
- **At 1100 × 780 the map is 246 × 134 in a band of 402**, not the 327 × 178 stated for "under 1400 wide". It is asserted as measured in `TheSunMapStandsWhereItWasLeftTests.AtTheSizeItOpensAtItKeepsTheMockupsSize`.
- **Unit 389's 9.2 finding holds:** 1 of 4 controls is greyed, and the hold ends at the hand-back.
- **The RST prefill and the words list:** `docs/RADIO_SHEET.md` quotes none of the strings changed tonight. I checked by running its test, which stayed green.

**8. The favorites row scrolls sideways with the bar hidden, and I haven't verified wheel scrolling.** *A finding.* At 1400 roughly one chip is in view beside the map's caption. If the hidden row doesn't respond to the wheel, a small arrow or a count is a one-unit follow-up.

**9. The `RULES_AT` split, the thirteenth unit running.** `PROJECT_STATUS.md` reads `HM-DEC-165 (2026-09-19)` because `tools/status.sh` writes it as a literal, while `CLAUDE.md` holds `CPS-DEC-0165`. `tools\` is not mine to edit.

**`validate-output.bat`:** not run; it asked for approval in earlier units. Hand-checked against its six rules: `UNIT:` above section 1; four sections in order with exact names; no fifth; section 4 present; section 3 not empty; the ordering block above `UNIT:` with A, B, C and a count.

#### Asks still outstanding

Carried per HM-DEC-139. **Unit 389's item 2, 9.3's conflict with the 2026-09-07 ruling, is answered by your ruling A of 2026-09-22 and dropped.** The instruction's section 3 also answers 7.5 and the favorites; neither was on the queue.

**Unit 389's own, verbatim (items 1 and 3 to 7):**

**1. 9.2 falls short in two places, and both are on the send path, so they were left.** *A finding with numbers. Changing either needs a licence, because each changes whether a send control is enabled.*
- **Only 1 of 4 controls is greyed.** Report and Confirm are withheld by R1 mid-over, and the canned lines become a note.
- **The hold ends at his hand-back, not when his carrier drops.** Unit 385 measured that holding to the drop would refuse every answer for 9 s after a PSK31 `K`, and 22.94 to 38.23 s after an Olivia one.

Unit 385's claim was *"3 of the four controls held with one sentence and the fourth already withheld by R1"*. Measured, all four are held, three carry the sentence, and one is greyed.

**3. Neither replay is the record, and 9.4's is not what you got.** *A finding. Its remedy needs your file, or a licence to open the engine.* Your `2026-09-21.jsonl` is not on this machine. Unit 385's item 2 measured that `Psk31MessageSplitter` completes nothing from a garbled over, and that site is under `src\Hamlet.RadioEngine\Psk31\`.

**4. 10.3 at 1400 on PSK31 with the dial off 14.070 falls back to stage A.** *A finding. Whether it blocks 10.3 is a judging session's reading.* The card also draws *PSK31 lives at 14.070; you are at 14.074*, and at 401 px wide that no longer fits in 178 px. Any fix would cost one of three things you ruled on: the map's full height, the rig's width, or the strayed line. On 14.070 the same window stands at the left edge. The fallback is what keeps that line on the screen.

**5. The card's green word and the row's *sending* can disagree for a few seconds.** *A finding.* The card follows the hold, which ends at his hand-back. The row follows his carrier. For the 9 s (PSK31) to 38 s (Olivia) tail after a `K`, his row still says *sending* while his card no longer does.

**6. *Your turn?* now stands beside the unchanged *His turn, a guess*.** *Author's. It is one line either way if you want them to match.* R14 held me to the one word 9.4 names.

**7. `validate-output.bat` - see the last line of this section.**

**The older queue, carried by reference as units 385 to 389 did:** the whole of *"Unit 388's section 4, carried per HM-DEC-139, verbatim"* and *"Asks still outstanding - carried per HM-DEC-139, verbatim (unit 388's)"* in `output.md` at commit `cd18cc6c`, lines 270 to 506. That covers unit 388's seven; the sixty (unit 387's seven, unit 386's six, unit 385's eight, unit 383's ten); and the twenty-nine carried by reference from units 369 to 382. **None of them is answered tonight** except as follows. Unit 383's item 1 (4.3 partial under the strictest reading) and unit 386's item 6 (0.1 stays unticked) are overtaken by tonight's ticks, and items 1 and 3 above put both back to you.
