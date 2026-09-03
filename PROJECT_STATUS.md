PROTOCOL: 2
PROJECT: Hamlet
STATE: EXECUTING
TASK: 1 of 5
WORK_INSTRUCTION: 230
BALL: code
NEXT_PASTE: none
RULES_AT: HM-DEC-152 (2026-08-31)
UPDATED: 2026-09-03T08:27:00-04:00
NOTE: 1d is answered and the answer is INHERITED, not this phase's. With all four phase-added classes excluded by filter the project still stalls - 49 of about 553 recorded, all passed, in flight Views.TheTabOwnsTheWorkspaceTests.TheBoundaryMeetsTheStripAndEnclosesTheWorkspace, three minutes of silence before the bound. That is a fourth distinct stopping point and every one of the four is an AvaloniaFact. Localising further before I touch anything: a Views-only run is up with a 2 m bound, which decides whether the stall needs the earlier 500 tests to have run first or lives inside Views on its own. Previous note: Run B settles the determinism question. 170 of 557, all passed, in flight Views.ReturningToCwShowsCwTests.ExactlyOneTabIsCheckedAndItIsTheOneShowing - a THIRD stopping point, after run A 251 at TheTabOwnsTheWorkspaceTests and unit 229 92 at TuningDoesNotSnapBackTests. Both of tonight's runs put exactly 3 m 00 s between the last recorded result and the bound firing, which is the inactivity bound itself and not a slow test. Every one of the three named in-flight tests carries AvaloniaFact, so the thing that stops is the process-wide headless dispatcher, not any one test. Nothing has been changed yet. Run A read total=251 executed=251 passed=251 failed=0, and neither run printed All tests finished running - both printed the inactivity message and Test Run Aborted. So the answer to 1c is STOPS PARTWAY, which overturns unit 228's finishes-and-will-not-exit. Starting 1d, all four phase-added classes excluded by filter, same bound, to settle whether this phase put the stall there or inherited it.

---

## What this file is

Volatile state, overwritten whole at each write. `PROTOCOL: 2` names
`STATUS_PROTOCOL.md`, which is not in this repository - the header is read as
which protocol this is written against, not as conformance anything here can
check.

Branch, commit and working-tree state are never reported here. The panel reads
those from `.git` itself.

Prose below the `---` is free. Nothing reads it, and no key of this format's own
may appear below the rule.
