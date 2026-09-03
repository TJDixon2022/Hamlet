PROTOCOL: 2
PROJECT: Hamlet
STATE: EXECUTING
TASK: 2 of 5
WORK_INSTRUCTION: 230
BALL: code
NEXT_PASTE: none
RULES_AT: HM-DEC-152 (2026-08-31)
UPDATED: 2026-09-03T08:38:00-04:00
NOTE: The stall is CONFINED TO Hamlet.App.Tests.Views and the rest of the project is readable tonight for the first time in this repository. Run F, everything except that one namespace: 495 of 495 passed, All tests finished running, exit 0, 50 s. 557 declared minus 62 Views is exactly 495, so nothing was skipped. Views split in half: first eight classes 25 of 25, exit 0, 4.4 s; second eight stall at 34 of 37 inside ThePitchControlsAreOffThePanelTests. So the owner can already read 520 of his 557, and I am splitting the second half once more to close the last 37 before I stop. This is procedure, not a code fix - task 2 stays at the stop rule and nothing of an attempt is committed. Previous note: Task 2 is going to the stop rule and I am saying so before I spend anything on it. Run E is the reason it is the right call rather than a shrug: TheTabsAndTheWorkspacesTests ALONE ran 5 of 5, printed All tests finished running, and exited 0 in 3.2 s - the same class inside which the Views-only run D stalled. One class returns, eighteen classes together do not, and no individual test is at fault. That is cross-class state on the one process-wide headless dispatcher, inherited, and the only production object my reading can name near it is MainWindowViewModel's 250 ms dwell timer, which the constructor starts and which nothing can stop because the class is not IDisposable and has no Shutdown. Naming it is not proving it, and moving the application's timer lifetime the night before the owner measures at the radio is what task 2 forbids. So: nothing committed of an attempt, and the night's deliverable is task 3. Measuring one more thing for the runbook first - everything except Views - because if that returns the owner has a procedure that reads the project instead of one that cannot. 170 of 557, all passed, in flight Views.ReturningToCwShowsCwTests.ExactlyOneTabIsCheckedAndItIsTheOneShowing - a THIRD stopping point, after run A 251 at TheTabOwnsTheWorkspaceTests and unit 229 92 at TuningDoesNotSnapBackTests. Both of tonight's runs put exactly 3 m 00 s between the last recorded result and the bound firing, which is the inactivity bound itself and not a slow test. Every one of the three named in-flight tests carries AvaloniaFact, so the thing that stops is the process-wide headless dispatcher, not any one test. Nothing has been changed yet. Run A read total=251 executed=251 passed=251 failed=0, and neither run printed All tests finished running - both printed the inactivity message and Test Run Aborted. So the answer to 1c is STOPS PARTWAY, which overturns unit 228's finishes-and-will-not-exit. Starting 1d, all four phase-added classes excluded by filter, same bound, to settle whether this phase put the stall there or inherited it.

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
