# The full-suite run — what to type and what to read

**For Tim, at the keyboard, on the morning he closes the FT8 phase.**

`PHASE_PLAN.md` reserves one run of the whole test suite for you: *by hand, by
Tim, uncontended, once, before he looks at the screen and closes the phase.* This
file is that run. It is not a status report and it is not a unit's story.

**Every line below is marked `[measured 2026-09-03]` or `[inherited]`.** A runbook
that quietly predicts is the same instrument that once recorded 1049 as a
baseline. Where a figure is inherited, it says where from.

**Written by work unit 230.** The measurements come from ten `dotnet test`
invocations on `QUIVERFULL`, Windows 11 Pro 10.0.26200, at commit `71947c5` on
`main`.

---

## The one rule that outranks everything else here

**Never run two `dotnet test` invocations at once.** Not in two terminals, not one
in the background. Contention in this repository once turned one standing failure
into five in `Hamlet.App.Tests` — four tests that build a real window headless,
failing because another project was eating the machine. It has also blinded one
unit outright and cost this phase four units in total.

One at a time, top to bottom. `[inherited — PHASE_PLAN.md, ruling of 2026-09-01]`

---

## 1. What to type

Run these **from `C:\Source\HamLet`**, in this order, **one at a time**, waiting
for each prompt to come back before starting the next.

The `-p:OutputPath=bin/unit230/` is optional but recommended: it keeps these runs
out of the normal `bin` tree, so if anything does stall and hold a file lock, your
next ordinary build is unaffected. See section 6.

### Leg 1 — Ft8Sharp

```
dotnet test tests\Ft8Sharp.Tests\Ft8Sharp.Tests.csproj ^
  --logger "trx;LogFileName=ft8sharp.trx" ^
  --results-directory TestResults-close
```

No hang bound is needed: this project has never stalled. `[measured 2026-09-03]`

### Leg 2 — Hamlet.App.Tests, in four parts

**This project cannot be run whole. It stalls.** Section 5 explains what that
looks like; section 6 explains what it costs you. These four commands are the way
to get a complete, readable count out of it, and all four were measured tonight.

```
dotnet test tests\Hamlet.App.Tests\Hamlet.App.Tests.csproj ^
  -p:OutputPath=bin/unit230/ ^
  --filter "FullyQualifiedName!~Hamlet.App.Tests.Views" ^
  --logger "trx;LogFileName=app-1-not-views.trx" ^
  --results-directory TestResults-close ^
  --blame-hang-timeout 2m --blame-hang-dump-type none
```

```
dotnet test tests\Hamlet.App.Tests\Hamlet.App.Tests.csproj ^
  -p:OutputPath=bin/unit230/ ^
  --filter "FullyQualifiedName~Views.BindingHealthTests|FullyQualifiedName~Views.ClippingTests|FullyQualifiedName~Views.EveryBandCanBeClickedTests|FullyQualifiedName~Views.EveryResourceKeyResolvesTests|FullyQualifiedName~Views.HistoryRecedesAndCurrentCopyDoesNotTests|FullyQualifiedName~Views.ReturningToCwShowsCwTests|FullyQualifiedName~Views.TheBandRowIsWhereItWasRuledTests|FullyQualifiedName~Views.TheCapturePressIsOnTheScreenTests" ^
  --logger "trx;LogFileName=app-2-views-a.trx" ^
  --results-directory TestResults-close ^
  --blame-hang-timeout 90s --blame-hang-dump-type none
```

```
dotnet test tests\Hamlet.App.Tests\Hamlet.App.Tests.csproj ^
  -p:OutputPath=bin/unit230/ ^
  --filter "FullyQualifiedName~Views.TheFollowedSentenceReachesTheScreenTests|FullyQualifiedName~Views.TheHeaderSaysEachThingOnceTests|FullyQualifiedName~Views.TheOperatingScreenIsLaidOutAsRuledTests|FullyQualifiedName~Views.ThePitchControlsAreOffThePanelTests" ^
  --logger "trx;LogFileName=app-3-views-b.trx" ^
  --results-directory TestResults-close ^
  --blame-hang-timeout 90s --blame-hang-dump-type none
```

```
dotnet test tests\Hamlet.App.Tests\Hamlet.App.Tests.csproj ^
  -p:OutputPath=bin/unit230/ ^
  --filter "FullyQualifiedName~Views.TheSendPanelComposesAndDoesNotKeyTests|FullyQualifiedName~Views.TheTabOwnsTheWorkspaceTests|FullyQualifiedName~Views.TheTabsAndTheWorkspacesTests|FullyQualifiedName~Views.ViewTestsActThroughControlsTests" ^
  --logger "trx;LogFileName=app-4-views-c.trx" ^
  --results-directory TestResults-close ^
  --blame-hang-timeout 90s --blame-hang-dump-type none
```

**Why `--blame-hang-dump-type none`.** Without it the collector writes a **397 MB**
dump beside the results when it fires. One such dump is sitting in
`TestResults229/` right now and nobody has ever needed it. `[measured 2026-09-03]`

**Why the split is where it is.** The stall is confined to the
`Hamlet.App.Tests.Views` namespace — 62 tests in 16 classes — and it depends on
how many of them share one test host, not on which one. Eight of those classes
with 25 tests between them returned cleanly; eight with 37 tests stalled at 34.
Four commands is what it took to keep every host under the ceiling.
`[measured 2026-09-03]`

### Leg 3 — Hamlet.RadioEngine.Tests

```
dotnet test tests\Hamlet.RadioEngine.Tests\Hamlet.RadioEngine.Tests.csproj ^
  --logger "trx;LogFileName=engine.trx" ^
  --results-directory TestResults-close
```

**This is the long one and it was NOT run tonight** — no unit of this phase is
permitted to run it in full. Everything this file says about its duration and its
outcome is inherited, and section 4 says so again in the same words. `[inherited]`

---

## 2. Where to read the answer

**Not from the console.** No project in this tree prints a run summary you can
trust, and there are two specific traps:

- **The console logs are UTF-16.** Anything that greps them as UTF-8 reports
  zero — that is how one unit produced a completely empty result set from a run
  that was working fine. `[inherited — PHASE_PLAN.md, 2026-09-01]`
- **A count from the console once read 1049** when what actually happened was two
  projects each stopping partway. It was written into the phase plan as a
  baseline. `[inherited — PHASE_PLAN.md, 2026-09-01]`

**Read the TRX.** Each command above writes one into `TestResults-close\`. Open it
and find the single `Counters` element near the top:

```xml
<Counters total="495" executed="495" passed="495" failed="0" error="0"
          timeout="0" aborted="0" inconclusive="0" passedButRunAborted="0"
          notRunnable="0" notExecuted="0" disconnected="0" warning="0"
          completed="0" inProgress="0" pending="0" />
```

That is the real one from tonight's `app-1-not-views.trx`. `total` is what ran,
not what exists — compare it against the declared count in section 3.

The `Times` element sits beside it and gives `start` and `finish`, which is how
you tell a slow run from a stalled one:

```xml
<Times start="2026-09-03T08:30:15.4260279-04:00"
       finish="2026-09-03T08:31:05.6499710-04:00" />
```

---

## 3. The expected counts

**Re-taken tonight by `--list-tests` on each project, one at a time**, which
discovers what exists without running any of it.

| Project | Declared | Expected to pass | |
|---|---|---|---|
| `Ft8Sharp.Tests` | 524 | 523, with 1 skipped | `[measured 2026-09-03]` |
| `Hamlet.App.Tests` | 557 | 557 | `[measured 2026-09-03]` |
| `Hamlet.RadioEngine.Tests` | 2179 | 2177, with 2 inherited reds | `[inherited]` |
| **Total** | **3260** | | `[measured 2026-09-03]` |

**`PHASE_PLAN.md`'s figures of 2157 / 523 / 38 and a total of 2718 are stale and
should not be diffed against.** Every one of the four has moved. Its 38 for
`Ft8Sharp.Tests` was never a census at all — it is the size of a channel-test
filter that got written into a census column. `[measured 2026-09-03]`

**The four App legs sum exactly.** 495 + 25 + 20 + 17 = 557, which is the declared
count, so the split loses nothing. `[measured 2026-09-03]`

**The two RadioEngine reds are expected and inherited**: `WhereTheTrackerStarts`
`DoesNotDecideThis` and `AStationElsewhereIsStillFound`, both in
`Hamlet.RadioEngine.Tests.Cw`. They pre-date the FT8 phase, have nothing to do
with FT8, and are not this phase's to fix. `[inherited]`

**`Ft8Sharp`'s one skip is the table-write gate** — `Ft8TableGenerationTests.`
`RewriteTheCheckedInTablesFile` — and is expected, not a failure.
`[measured 2026-09-03]`

---

## 4. How long each leg takes

| Leg | Duration | |
|---|---|---|
| `Ft8Sharp.Tests` | 5 m 09 s | `[measured 2026-09-03]` — 5 m 11 s at unit 229 |
| App 1, not Views | 50 seconds | `[measured 2026-09-03]` |
| App 2, Views a | 4.4 seconds | `[measured 2026-09-03]` |
| App 3, Views b | 4.6 seconds | `[measured 2026-09-03]` |
| App 4, Views c | 2.7 seconds | `[measured 2026-09-03]` |
| `Hamlet.RadioEngine.Tests` | **over half an hour** | `[inherited]` |

**`Hamlet.RadioEngine.Tests` was not measured tonight.** Unit 230 was forbidden to
run it: it runs real signal processing over recorded audio, it takes well over
half an hour, and it has already cost this phase four units — one killed by a
watchdog mid-run, one blinded by contention, and two that read a truncated run as
a finished one and reported a criterion unmet on the strength of it. **The figure
above is inherited from `PHASE_PLAN.md` and is not a measurement.** One unit
started it alone at 08:15 and it had not returned when a 60-minute bound stopped
the wait, so *over half an hour* is a floor rather than an estimate. `[inherited]`

**So budget about 45 minutes of wall clock**, nearly all of it the engine leg, and
do not touch the machine while it runs.

---

## 5. What a hang looks like beside a failure

**On this machine they look identical while they are happening: a cursor that does
not come back.** Telling them apart afterwards is the whole point of using the TRX.

**A failure** looks like this. The prompt comes back on its own, the exit code is
1, and the TRX carries the failure:

```
Failed!  - Failed:     2, Passed:  2177, Skipped:     0, Total:  2179
```

`Counters` shows `failed="2"`, and `executed` equals `total`. Every declared test
ran. `[inherited]`

**A hang** looks like this, and this is the exact text from tonight:

```
The active test run was aborted. Reason: Test host process crashed
Data collector 'Blame' message: The specified inactivity time of 3 minutes has
elapsed. Collecting hang dumps from testhost and its child processes.

Passed!  - Failed:     0, Passed:   251, Skipped:     0, Total:   251, Duration: 7 s
Test Run Aborted.

The test running when the crash occurred:
Hamlet.App.Tests.Views.TheTabOwnsTheWorkspaceTests.TheSelectedTabMergesIntoTheBoundary
```

**Read that carefully, because it is designed to mislead you.** It says
`Passed!` and it says `Failed: 0`. Nothing failed. It also ran 251 of 557 tests
and then stopped. `[measured 2026-09-03]`

**The three tells, in order of reliability:**

1. **`total` in the TRX is less than the declared count in section 3.** That is
   the one that never lies.
2. **`Test Run Aborted`** appears, and `All tests finished running` does **not**.
   When a run really finishes, the collector prints
   `Data collector 'Blame' message: All tests finished running, Sequence file will
   not be generated.` `[measured 2026-09-03]`
3. **The `Times` gap.** Subtract the last `endTime` in the TRX from `finish`. If
   that gap is exactly your `--blame-hang-timeout`, nothing was slow — the run
   stopped dead and the bound ended it. Tonight both whole-project runs put
   **exactly 3 m 00 s** in that gap. `[measured 2026-09-03]`

**"The test running when the crash occurred" is not the culprit and the tool says
so.** Six recorded runs of this project named six different tests there
— 92, 251, 170, 49, 41 and 34 results deep — with everything green behind each
one. It is whichever test happened to be next. `[measured 2026-09-03]`

---

## 6. What to do when a host does stall

**It holds a write lock on `tests\Hamlet.App.Tests\bin\...\Hamlet.App.dll` and the
lock outlives the run.** Your next build of anything that writes there fails with
`MSB3027`. That is `HM-OPEN-069`, and it is how the damage outlives the run.
`[inherited]`

**What to do:**

1. **Wait for the bound.** A run started with `--blame-hang-timeout` ends itself.
   If you started it without one, it will not.
2. **Do not kill a testhost you did not start**, and be aware that one left over
   from an earlier session is a common cause of a build that suddenly fails for no
   reason.
3. **Build to a different output path instead of fighting the lock:**
   `-p:OutputPath=bin/unit230/`. That is why every App command in section 1 already
   carries it, and it is why they can be re-run back to back. `[inherited — unit
   224's recorded workaround, confirmed in use 2026-09-03]`

**Unit 230 did not fix the stall and did not attempt a fix.** It established that
the stall is inherited — it reproduces with all four of this phase's test classes
excluded, and it reproduces with the `Views` namespace running entirely on its own
— and that it is cross-class state on the one process-wide Avalonia headless
dispatcher, which `TestParallelism.cs` already names as the reason that assembly
is serialized. **This section stays in the file as the recovery it still is.**
`[measured 2026-09-03]`

---

## 7. What the run is being read for

**The criterion is: no new red, and the inherited failing set unchanged.**

**Name the failing set and count it — do not count it alone.** A count on its own
lets a swap hide: one inherited red goes green, one green goes red, and the total
is identical. `[inherited — ruling of 2026-08-31]`

**Diff against `docs/test-baseline.md`**, which lists the failing set by name.

**Two caveats on that file, in its own words, because a baseline you over-trust is
worse than one you know the shape of:**

- **Its discovered census is complete; its run column is mostly empty.** When it
  was written, no completed whole-project run of either Hamlet test project had
  ever been produced. The difference between those two columns is, as the file
  itself puts it, *the honest state of this repository's knowledge of itself.*
- **The unit that wrote it blinded its own instrument and says so.** It piped a
  console logger through `grep`, `grep` block-buffers when its output is not a
  terminal, and the captured stream ended up holding one line — the run's start
  stamp — and not a single test result.

**One thing has changed since that file was written, and it is in your favour.**
`Hamlet.App.Tests` now has a complete run column: **557 of 557, all passing**,
across the four commands in leg 2. `docs/test-baseline.md` has not been rewritten
to say so — that was not this unit's to do — so read this paragraph beside it.
`[measured 2026-09-03]`

**What is NOT in scope for this run.** *Whole suite green* read strictly would mean
no step of this phase could ever close, however good the FT8 work is. The two
inherited CW reds are recorded, not dropped, and fixing them is a different
phase's job. `[inherited — ruling of 2026-08-31]`

---

## What tonight's numbers actually were

The complete set, so you can tell a change from a surprise. All counts are TRX
`Counters` elements. `[measured 2026-09-03]`

| Run | Filter | Recorded | Result |
|---|---|---|---|
| A | whole project | 251 / 557 | stalled, bound fired |
| B | whole project | 170 / 557 | stalled, bound fired |
| C | four phase classes excluded | 49 / 553 | stalled, bound fired |
| D | `Views` only | 41 / 62 | stalled, bound fired |
| E | one Views class | 5 / 5 | **returned, exit 0** |
| F | not `Views` | 495 / 495 | **returned, exit 0** |
| G1 | Views classes 1–8 | 25 / 25 | **returned, exit 0** |
| G2 | Views classes 9–16 | 34 / 37 | stalled, bound fired |
| Q3 | Views classes 9–12 | 20 / 20 | **returned, exit 0** |
| Q4 | Views classes 13–16 | 17 / 17 | **returned, exit 0** |

**Nothing failed in any of them.** Every test that ran, passed.
