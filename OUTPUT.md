READ IN THIS ORDER

A. The phase goal. Hamlet hears FT8 off the radio and displays the decoded text on
   screen. Steps 1 to 5 are done - the library and its tables, 77-bit messages,
   encode and synthesis, candidate search, decode. Step 6 is BLOCKED at 14 of 306,
   4.6 per cent against a 40 per cent band, on a ruling only Tim can give: four
   instruments were pointed at it and the fourth ran upstream's own decode_ft8.exe
   over the identical audio and got the identical messages on all 918 slots, so the
   shortfall came in with the code and what remains is whether this port may
   deliberately diverge from the pin. Nothing tonight touched it. Step 7 is BLOCKED
   with all five of its must-pass criteria evidenced by units 224 to 228, and its
   closing line is a bench check at 14.074 that no unit performs.

B. This step and its exit criteria. Step 7 - Hamlet displays decoded FT8. THIS UNIT
   CLAIMS NONE OF ITS FIVE CRITERIA AND WAS NOT AUTHORED TO. It clears a blocker
   sitting in front of the one whole-suite run PHASE_PLAN.md places between this
   tree and Tim closing the phase - "run by hand, by Tim, uncontended, once, before
   he looks at the screen". The two standing lines were re-taken and are reported
   with their numbers: Ft8Sharp 524 total, 523 passed, 0 failed, 1 skipped;
   attribution 231 paths from 2828ab6..HEAD with 18 under src/Hamlet.* or
   tests/Hamlet.*, SO THE PLAN'S REDUCTION DOES NOT APPLY AND IS NOT CLAIMED; the
   channel tests 9 of 9 in the App project after the version bump and 38 of 38 in
   the Engine project in 13 m 38 s.

C. This report. The answer to the question this unit was commissioned to ask:
   Hamlet.App.Tests STOPS PARTWAY. It does not finish and refuse to exit, which is
   what unit 228 concluded. Six recorded runs read 92, 251, 170, 49, 41 and 34
   results out of 557 declared, each naming a different test as in flight, each
   putting exactly the inactivity bound between its last result and the abort, and
   none printing "All tests finished running". Complete runs of this project
   recorded in this tree before tonight: 0 of 557. After tonight: 557 OF 557, ALL
   GREEN, in four invocations rather than one - 495 outside the Views namespace,
   then 25, 20 and 17 across Views split three ways. The stall is INHERITED, not
   this phase's: it reproduces with all four classes this phase added excluded and
   with Views running alone. Section 4 raises 3 items. NONE OF THE THREE IS IN THE
   WAY OF A CRITERION IN B.

UNIT:       230 - complete at task 5 of 5, nothing dropped: the named drop candidate was task 5 and it was kept - 2026-09-03 08:55
PHASE GOAL: Hamlet hears FT8 off the radio and shows the decoded text on screen. Steps 1-5 done, step 6 blocked on an inherited limit and an owner ruling, step 7 evidenced and closing on a bench check at 14.074.
UNIT GOAL:  Settle what Hamlet.App.Tests actually does, fix it if the seam is nameable, and leave Tim a written procedure for the one full-suite run that stands whether or not the fix landed.
ADVANCED:   no - this unit removed a blocker; no decoder path, no threshold and no panel was touched, and no step 7 criterion is claimed.
NUMBER:     whole-project runs of Hamlet.App.Tests that recorded every declared test and returned on their own: 0 -> 0. The instrument is nonetheless readable: 557 of 557 across four invocations, which is 0 -> 557 on tests recorded in one sitting.
DRIFT:      1 consecutive units without advance  (was 0)

## 1. What Claude did

**Complete, at task 5 of 5. Nothing was dropped.** The named drop candidate was
task 5, `HM-OPEN-069`, and it was kept - the night ran short of trouble, not short
of time. Task 2 went to its own stop rule, which is described below and is not a
dropped task.

QUIVERFULL, Windows 11 Pro 10.0.26200, `Hamlet` confirmed by the four gate checks,
branch `main`, four commits from `90c184c` to `b55a46f`, all pushed.

### The measurement, which is the unit

Ten `dotnet test` invocations, one at a time, never two at once. **Every count
below is the TRX `ResultSummary.Counters` element. Not one is a console line.**

| Run | Filter | Recorded | Outcome |
|---|---|---|---|
| A | whole project | 251 / 557 | stalled, bound fired |
| B | whole project | 170 / 557 | stalled, bound fired |
| C | this phase's four classes excluded | 49 / 553 | stalled, bound fired |
| D | `Views` only | 41 / 62 | stalled, bound fired |
| E | one `Views` class | 5 / 5 | **returned, exit 0, 3.2 s** |
| F | everything except `Views` | 495 / 495 | **returned, exit 0, 50 s** |
| G1 | `Views` classes 1-8 | 25 / 25 | **returned, exit 0, 4.4 s** |
| G2 | `Views` classes 9-16 | 34 / 37 | stalled, bound fired |
| Q3 | `Views` classes 9-12 | 20 / 20 | **returned, exit 0, 4.6 s** |
| Q4 | `Views` classes 13-16 | 17 / 17 | **returned, exit 0, 2.7 s** |

**Nothing failed in any of them. Every test that ran, passed.**

`495 + 25 + 20 + 17 = 557`, which is exactly what `--list-tests` declares, so the
split loses nothing.

### 1a - what unit 229 left on disk, which nobody had read

`TestResults229/app-full-run1.trx`: `total="92" executed="92" passed="92"
failed="0"`, `start` 00:36:44.822, `finish` 00:42:38.294 - **5 m 53 s**. The
`Sequence_07195e38b88648d0bd931baacbb33324.xml` carries **93 `Test` elements, 92
`Completed="True"` and exactly one `Completed="False"`**:
`ViewModels.TuningDoesNotSnapBackTests.AReadingFromAfterTheTuneStillMovesIt`.
**The run ended by the hang collector, not by returning.** The hang dump beside it
measures **396,924,352 bytes**. Nothing was committed or deleted from that folder.

### 1b - the census, by discovery

`--list-tests`, one project at a time: **Ft8Sharp 524, Hamlet.App.Tests 557,
Hamlet.RadioEngine.Tests 2179, total 3260.** That reproduces unit 229's figures
exactly. **`PHASE_PLAN.md`'s 2157 / 523 / 38 and total 2718 are stale in all four
figures**, and its 38 for `Ft8Sharp` was never a census - it is the size of a
RadioEngine channel-test filter that got written into a census column.

### 1c - the decisive question, answered in the instruction's own words

- **How many results out of the declared count?** 251 of 557 on run A, 170 of 557
  on run B.
- **Did the collector print `All tests finished running`?** **No.** It printed
  `The specified inactivity time of 3 minutes has elapsed` and `Test Run Aborted`.
- **How long between the last recorded result and the bound firing?** Run A: last
  `endTime` 08:12:59.639, `finish` 08:15:59.725 - **3 m 00.1 s.** Run B: last
  `endTime` 08:17:18.519, `finish` 08:20:18.609 - **3 m 00.1 s.** Both are exactly
  the bound. **Nothing was slow. The run stopped dead.**
- **So which is it?** **IT STOPS PARTWAY.** Unit 228's conclusion - passes
  everything and then will not exit - is wrong, and this report says so plainly
  rather than splitting the difference.
- **Deterministic?** **No. A different test every time**, with everything green
  behind it, which is the shutdown-or-dispatcher signature the instruction named.

### 1d - whose stall is it

**Inherited. It is not this phase's.** Run C excluded all four classes this phase
added - `TheDecodedTableIsRealTests`, `TheTabHearsEverySlotTests`,
`TheTabHearsARealBandTests`, `TheTabSaysWhyNothingIsDecodingTests` - and stalled at
49. Run D ran the `Views` namespace with none of the other five hundred tests
before it and stalled after **six seconds of work**. Nobody had excluded all four
before; the answer is that it makes no difference.

### 1e - the seam, from reading

**Named, and named as a class of thing rather than an object, because that is as
far as reading got.** `TestParallelism.cs` already records the mechanism in its own
words - *an Avalonia headless test runs on one process-wide dispatcher* - and
serializes the assembly for it. **Every one of the six named in-flight tests
carries `[AvaloniaFact]`.** The stall is confined to `Hamlet.App.Tests.Views`, 62
tests in 16 classes, and scales with how many share one host: 8 classes and 25
tests returned, 8 classes and 37 tests stalled at 34, the namespace stalled at 41.

**The nearest production object a reading can name, offered as the first place to
look and not as a finding:** `MainWindowViewModel`'s constructor starts a 250 ms
`DispatcherTimer` and a clock timer and fires `_ = QueryTheClockAsync()`, an
un-awaited SNTP UDP query with 3 s timeouts and `ConfigureAwait(true)`. **The class
is not `IDisposable` and has no `Shutdown`, so nothing can stop them**, and 38
sites in this test project construct one. **I did not read the
`Avalonia.Headless.XUnit` 11.3.0 source** - it lives outside the working directory
and the harness would not list it - so the object that actually holds the
dispatcher is not named, and I am saying that rather than guessing at it.

### Task 2 - the stop rule, invoked deliberately

**No code change was attempted and none is committed.** Task 2 permits touching
`src/Hamlet.App` only if the trace names a production object as the thing holding
the process open. The trace named a seam class, not an object. Moving
`MainWindowViewModel`'s timer lifetime on a hypothesis, the night before Tim
measures the decoder at the radio, is exactly the trade that task's own
prohibitions exist to prevent.

**Task 2's literal assertion - two consecutive WHOLE-PROJECT runs that record every
declared test and return on their own - is NOT MET and is not claimed.** What was
delivered instead is the four-command route that reads all 557.

### Task 3 - `docs/full-suite-run.md`, committed

Seven sections, **every line tagged `[measured 2026-09-03]` or `[inherited]`**.
What to type per project with the reason attached, where to read the answer with
the UTF-16 console trap named, the expected counts with the plan's 2718 marked
stale, how long each leg takes, what a hang looks like beside a failure with
tonight's text quoted verbatim, what to do when a host stalls, and what the run is
read for with `docs/test-baseline.md`'s own two caveats quoted honestly.
**`Hamlet.RadioEngine.Tests` was not run tonight and the file says so in those
words** - its timing is inherited and labelled.

### Task 4 - the standing gates, taken at the end

- **`Ft8Sharp`: 524 total, 523 executed, 523 passed, 0 failed, 1 skipped**, from the
  TRX `Counters` element, in **5 m 09 s** - unit 229 read the same figures in
  5 m 11 s. **The failing set is EMPTY.** The one skip is
  `Ft8TableGenerationTests.RewriteTheCheckedInTablesFile`, the table-write gate,
  and is expected.
- **Attribution: `git diff --name-only 2828ab6..HEAD` gives 231 paths, 18 of them
  under `src/Hamlet.*` or `tests/Hamlet.*`.** **The plan's reduction therefore does
  not apply to this unit by its own terms, and no clean attribution diff is
  claimed.** The criterion is answered by those named paths plus tests over the
  changed code. Worth adding, as a fact and not as a laundering: **this unit itself
  changed none of those 18** - it committed `PROJECT_STATUS.md`, `PHASE_STATUS.md`,
  `Directory.Build.props`, `OPEN_ISSUES.md`, `docs/full-suite-run.md` and
  `tools/unit230/census.sh` and nothing else.
- **Channel tests: App 9 of 9 in 2.6 s, re-run AFTER the 1.12.36 bump** because the
  root version is one of the three channels and it is what moved. **Engine 38 of 38
  in 13 m 38 s**, backgrounded, alone, with statuses written across it.
  **Both failing sets EMPTY.**

### Task 5 - the drop candidate, kept

`HM-OPEN-069` asked in its own words for somebody to run the Views classes with a
TRX logger until one does not return. **Three units in a row recorded that they had
not done it.** Tonight it was done, and the entry is rewritten with the ten-run
table, the two things it had wrong, and a `blocks:` line that is finally true: it
blocked Tim's one full-suite run until today, and since `docs/full-suite-run.md` it
does not. The original text is kept below, labelled, rather than overwritten.

### Decisions I made for myself

1. **I re-ordered the tasks.** Task 5 and the version bump were committed before
   task 4's gates finished, because the Engine channel leg is 13 minutes and
   leaving it to last would have put the whole unit's evidence behind one long run.
   The gates were still taken at the end, as the instruction directs.
2. **I ran four diagnostic invocations the instruction did not name** - E, F, G1
   and Q3/Q4. They are what turned "this project cannot be read" into "here is how
   you read it", and without them task 3 would have been a runbook for an
   instrument that does not work.
3. **I used a 3-minute hang bound, not unit 229's longer one**, and
   `--blame-hang-dump-type none` throughout. No dump was written tonight.

### Mismatches found, reported and NOT repaired

- **The instruction's channel filter is the App one.** Task 4 names
  `DecisionLogOrderTests`, `VersionTests`, `EveryResourceKeyResolvesTests`,
  `ViewTestsActThroughControlsTests` for both projects. In `Hamlet.RadioEngine.Tests`
  that filter **matches nothing** - I ran it and got `total="0"` in 1.5 s. The
  Engine channel set is **eleven different classes**, recorded in
  `src/Ft8Sharp/porting-notes.md`, and that is what the 38 of 38 above was run with.
- **`PHASE_OUTCOME.md`'s last entry records unit 229 as `FATE: executed`,
  `STATE_AFTER: blocked`, `COST: 24.5806055` - unit 228's cost - and unit 228's
  `STATE_WHY` verbatim. `RUN_LEDGER.md` records the same run as `killed by the
  watchdog: no status write within 25 min of the launch clock`.** Confirmed in both
  files. Neither was changed.
- `PROJECT_STATUS.md` `RULES_AT` reads `HM-DEC-152 (2026-08-31)`; the highest entry
  in `CLAUDE.md` section 1 reads `CPS-DEC-0152`. Unchanged.
- `PHASE_OUTCOME.md`'s header says steps 1 and 3 are `done` while the last entry for
  each says `partial`. Unchanged.
- `PHASE_STATUS.md` read `CURRENT_STEP: 6` and `WORK_INSTRUCTION: 229`. I set the
  `WORK_INSTRUCTION:` line to `230 - the closing instrument, and the night that
  survives its own test runs`, which is mine to write. **`CURRENT_STEP:` belongs to
  the launcher and still reads 6 while this unit worked step 7.** Not mine to touch.
- **The "2026-09-02 ruling that steps 6 and 7 do not depend on each other", cited by
  units 224, 225 and 228, is not in `PHASE_PLAN.md`.** It is not repeated here. What
  licenses step 7 work while step 6 waits is the plan's Branching section.
- **Untracked debris, unchanged and uncommitted**: eight `.obj` files at the root,
  `unit215-section.md`, `unit216-section.md`, `unit217-status.py`,
  `toolsarbitervalidate-output.bat`, `tools/unit217/`, `tools/unit228/`, and
  `TestResults223/`, `TestResults228/`, `TestResults229/`. **`TestResults229/`
  contains a 397 MB hang dump** at
  `TimDi_QUIVERFULL_2026-09-03_00_36_47/In/QUIVERFULL/testhost_47244_20260903T004236_hangdump.dmp`.
  I added `TestResults230/` to that pile and did not commit it either.

### The validator - and this report is NOT claimed as an exit 0

**The permitted spellings were attempted once and refused, for the seventh unit
running.** `tools\arbiter\validate-output.bat output.md` reached the shell as
`toolsarbitervalidate-output.bat: command not found` - Git Bash removes a
backslash before an ordinary letter, exactly as unit 228 diagnosed. The
forward-slash form and a `cmd //c` wrapper were both refused by the harness. **The
remedy is a permission change, which is Tim's under `ARBITER.md` section 6.** It is
not campaigned for here.

**Hand-checked against the script's own body, which I read in full:**

1. `UNIT:` line present and parseable, **line 38**, inside the 60-line window the
   script reads. **PASS.**
2. The four `## ` headings, in order, exact names: lines 45, 267, 297, 355, and
   the joined string matches the script's `WANT` exactly. **PASS.**
3. No fifth `## ` heading - `grep -n '^## '` returns those four and nothing else.
   **PASS.**
4. `## 4. What's blocking us` present, and its apostrophe verified as ASCII `0x27`
   by `od -c` so `findstr /b /c:` will match it. **PASS.**
5. Section 3 non-empty - 56 lines between the section 3 and section 4 headings.
   **PASS.**
6. Ordering block: `READ IN THIS ORDER` at line 1, `A.` at 3, `B.` at 14, `C.` at
   25, and `raises 3 items` at line 35 - all inside the 60-line window, and C
   commits to a count. **PASS.**

**Six of six by hand. No exit 0 was obtained and none is claimed.**

## 2. What the owner should expect

**Nothing about the application has changed.** No file under `src/` moved. No
threshold moved. No panel says anything different. If you start Hamlet it behaves
exactly as it did after unit 228. **That is deliberate** - the decoder you are
about to measure at 14.074 must not move the night before you measure it.

**What changed is that the test suite can be read.** Before tonight, no run of
`Hamlet.App.Tests` recorded in this repository had ever completed - the best was
167 of 557, and unit 229's was 92. Tonight all 557 were recorded, all passing, in
four commands. Those four commands are written down in `docs/full-suite-run.md`.

**What will look wrong but is not:**

- **`Hamlet.App.Tests` still cannot be run whole, and running it whole still looks
  like a pass.** It prints `Passed!` and `Failed: 0` and then `Test Run Aborted`,
  after running fewer than half its tests. **That is the single most misleading
  thing in this repository** and section 5 of the runbook exists for it.
- **The version went to 1.12.36 with no product change in it.** Under HM-DEC-150
  every unit that touches the tree takes a patch; this one bought measurement, not
  behaviour.
- **`HM-OPEN-069` is still `status: open`.** Its `blocks:` line now says nothing,
  which is true - the operational problem is solved by procedure. What is still
  open is the underlying object, which nobody has named.
- **`docs/test-baseline.md` has not been updated** to say that `Hamlet.App.Tests`
  now has a complete run column. That was not this unit's file to rewrite;
  `docs/full-suite-run.md` says so beside it.
- **A 397 MB hang dump is sitting in `TestResults229/`.** It is untracked, `rm` is
  out of scope, and it is yours to delete whenever you like.

## 3. What you should see

**The first line of `docs/full-suite-run.md` for `Hamlet.App.Tests` is this, and on
this machine it comes back:**

```
dotnet test tests\Hamlet.App.Tests\Hamlet.App.Tests.csproj ^
  -p:OutputPath=bin/unit230/ ^
  --filter "FullyQualifiedName!~Hamlet.App.Tests.Views" ^
  --logger "trx;LogFileName=app-1-not-views.trx" ^
  --results-directory TestResults-close ^
  --blame-hang-timeout 2m --blame-hang-dump-type none
```

**The prompt comes back on its own in about 50 seconds**, exit code 0, and the last
two lines are:

```
Data collector 'Blame' message: All tests finished running, Sequence file will not be generated.
Passed!  - Failed:     0, Passed:   495, Skipped:     0, Total:   495, Duration: 48 s
```

and the TRX reads:

```xml
<Counters total="495" executed="495" passed="495" failed="0" ... />
```

**Three more commands, 4.4 s, 4.6 s and 2.7 s, add 25, 20 and 17. That is 557 of
557 with nothing red - the first time this project has ever been counted whole.**

**And the thing to recognise rather than diagnose at seven in the morning.** If you
run that project WITHOUT a filter, this is what the screen does. Nothing appears
for about 10 to 50 seconds while it works, then **nothing appears at all** - no
output, no progress, no cursor movement. **You wait exactly as long as your
`--blame-hang-timeout` says**, three minutes on the runs above, and then it ends
itself with:

```
The active test run was aborted. Reason: Test host process crashed
Data collector 'Blame' message: The specified inactivity time of 3 minutes has elapsed.

Passed!  - Failed:     0, Passed:   251, Skipped:     0, Total:   251, Duration: 7 s
Test Run Aborted.

The test running when the crash occurred:
Hamlet.App.Tests.Views.TheTabOwnsTheWorkspaceTests.TheSelectedTabMergesIntoTheBoundary
```

**Read it twice. It says `Passed!` and `Failed: 0`, and it ran 251 of 557.** The
named test is not the culprit - six runs named six different tests. **If you run it
without a hang bound at all, it never ends**, and it holds a write lock that fails
your next build with `MSB3027`.

**Nothing you can see in the application changed tonight.** This unit only makes
the suite countable so that the run you make before closing the phase means
something.

## 4. What's blocking us

**Three items. None of them blocks a step 7 criterion, and none needs an answer
before you make the full-suite run.**

**1. The loop's own record of unit 229 disagrees with itself, and I was told not to
repair it.**

- **Ruling wanted:** whether `PHASE_OUTCOME.md`'s last entry should be corrected.
- **Reasoning:** it records unit 229 as `FATE: executed` with `STATE_AFTER:
  blocked`, carrying unit 228's `COST` of `24.5806055` and unit 228's `STATE_WHY`
  word for word. `RUN_LEDGER.md` records the same run as killed by the watchdog at
  00:51. The tree agrees with the ledger: no commit after `90c184c`, no `output.md`
  newer than unit 228's, `PROJECT_STATUS.md` frozen at `TASK: 1 of 4`. A phase
  outcome file that reports a killed run as executed, with another unit's cost,
  makes the loop's own drift and spend unreadable.
- **Rejected:** repairing it silently, which the instruction forbids and which
  would teach nobody; and treating it as a one-off, when the same shape would
  recur on the next watchdog kill.

**2. Two files now carry stale numbers that a reader would trust.**

- **Ruling wanted:** whether `PHASE_PLAN.md`'s census and `docs/test-baseline.md`
  should be brought up to what is now measured, and by whom.
- **Reasoning:** the plan records 2157 / 523 / 38 and a total of 2718; discovery
  reads 2179 / 557 / 524 and 3260. Its 38 is a filter size in a census column.
  Separately, `docs/test-baseline.md` says in its own voice that no completed
  whole-project run of either Hamlet project has ever been produced - that is no
  longer true of `Hamlet.App.Tests`, which now reads 557 of 557. Both files are the
  ones a future unit will diff against.
- **Rejected:** editing either tonight. `docs/full-suite-run.md` names both as
  stale and gives the current figures beside them, which is the honest holding
  position but is not a fix.

**3. The Views stall is worked around, not fixed, and nobody has named the object.**

- **Ruling wanted:** whether a unit should be spent naming what holds the Avalonia
  headless dispatcher open, or whether the four-command workaround is enough.
- **Reasoning:** the workaround is real and measured - 557 of 557 tonight - so this
  is genuinely optional. Against that: the fault has now cost this phase parts of
  five units, the suite cannot be run in one command by anyone or anything, and a
  fix probably needs a debugger on the hang dump plus reading
  `Avalonia.Headless.XUnit` 11.3.0, neither of which was reachable from inside this
  session's permissions. **My reading names `MainWindowViewModel`'s undisposable
  250 ms `DispatcherTimer` as the first place to look and explicitly not as a
  finding.**
- **Rejected:** attempting it tonight. Task 2's stop rule and the prohibition on
  moving application behaviour before the bench check both point the same way, and
  a speculative fix to inherited test infrastructure is how a night disappears.

**On `validate-output.bat`:** the permitted spellings were attempted once, per the
instruction, and refused for the seventh unit running. The six rules were
hand-checked against the script's own body and all six pass; **this report is not
claimed as an exit 0.** The details are at the end of section 1. The scope defect
is parked and is not campaigned for here.
