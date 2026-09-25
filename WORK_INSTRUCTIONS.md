# Work instruction - none authored: the phase is stopped for the owner

**This file carries no unit.** The arbiter was invoked for *Hamlet reads a CQ call correctly*,
redirected (3 of this run) to criterion 7.4, and stops the phase under ARBITER.md section 6
instead of authoring. **No session should execute anything from this file.**

**The owner's seed that stood here is not lost.** The file this one replaced is the owner's
*Work instruction 439 - every CW test is traced to a requirement*, the seed of *Hamlet meets
the CW requirements*, committed at `d925a633`. It is restored with:

```
git checkout d925a633 -- WORK_INSTRUCTIONS.md
```

---

## 0. The project gate

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  MURC.sln
  root             C:\Source\HamLet

If all four are not as stated, you are in the wrong repository.
REFUSE. Do not read the rest of this file, do not summarise it, do not
adapt it to whatever project you are actually in, and change nothing.
Reply with only: the path you are in, which checks failed, and
"wrong project - nothing done."

If all four hold, say "Hamlet confirmed" and continue.
```

---

## 1. Why no unit exists

**The count today.** Step 3: 6 of 7 criteria met (3.6 open). Step 7: 7.3, 7.5 and 7.7 met,
7.4 open, closed partial under its own three-unit rule by unit 433 (P48). Steps 4 and 5 not
started. Step 6: 6.5 open.

- **PHASE GOAL:** Hamlet reads a CQ call correctly.
- **UNIT GOAL:** none. The phase is stopped for the owner.
- **ADVANCES:** none. No unit is authored.

**What the tree shows, measured at 13:31 on 2026-09-25:**

1. **The owner archived this phase before this arbiter ran.** HEAD is `d925a633`, *phase:
   requirements - Hamlet meets the CW requirements; correctness phase archived with 5.1 open;
   seed unit 439*, committed 13:27:37. It adds `CW_REQUIREMENTS.md`, `CW_SPEC.md`, and
   `docs/phase-requirements/PHASE_PLAN.md`, `PHASE_STATUS.md` and `PHASE_OUTCOME.md`. It also
   commits the new phase's seed as the root `WORK_INSTRUCTIONS.md`, with `ADVANCES: step 0
   criterion 2`.
2. **The root phase files were not switched.** The root `PHASE_PLAN.md`, `PHASE_STATUS.md` and
   `PHASE_OUTCOME.md` still name *Hamlet reads a CQ call correctly*, and the launcher built
   this arbiter's prompt from them at 13:28:06.
3. **A unit of the archived phase is live in the tree.** `PROJECT_STATUS.md`, updated
   13:30:36: *STATE: EXECUTING, TASK 1 of 3, WORK_INSTRUCTION 439 - the marks are joined
   across gaps no sender makes*. Its note reads *Trace: replaying the bridge over the opening,
   the locked stretch, 003901 and 003919 cold*. It has written 361 lines into
   `tests/Hamlet.RadioEngine.Tests/Cw/WhatTheOpeningHeardTests.cs`, uncommitted, the file last
   written 13:30:12. Its approach, bridging key-up dips of 15 ms or less in `CwUnitEstimator`,
   is the one route to 7.4 this arbiter's loop test found untried. It is running now, so the
   attempt list this prompt carries is missing it.
4. **Two numbers named 439 now exist.** One is the live correctness unit. The other is the
   requirements seed.

**Why this is a stop and not a work-around.** Authoring a 7.4 unit here would overwrite the
owner's committed seed. It would author into a phase the owner has archived, and no arbiter
may overrule the owner. It would also put a second session into the tree while unit 439
writes the same test file. Three questions are the owner's: which phase the loop runs, whether
the live unit finishes, and what may be killed. They are section 6's *what the project is for*
and *what may be killed*. None is a section 4 carry.

## 2. The question for the owner

1. **Which phase does the loop run?** If it is *Hamlet meets the CW requirements*, as
   `d925a633` says, then the root phase files still need to be switched, and the launcher
   pointed at them, before the seed runs. Both are the owner's to do or order. The seed is
   restored with the command at the top of this file.
2. **Does the live correctness unit 439 finish?** It is mid-trace on 7.4's bridging route.
   Letting it finish costs nothing the new phase needs, and it records its measurement in
   `PARKED.md` P48. Stopping it leaves 361 uncommitted lines in `WhatTheOpeningHeardTests.cs`.
   The arbiter kills nothing and decides neither.
3. **The launcher overlap**, carried from units 437 to 439 as the owner's: the launcher built
   an arbiter prompt and redirected while a unit of the same chain was running.

**Author's recommendation, overrulable:** let unit 439 finish its trace and report. Then run
the requirements seed as `d925a633` intends, and add 7.4's measurement to the record the new
phase's step 4 (pitch) and step 5 (speed) inherit.

## 3. Parked

Everything in `docs/phase-correctness/PARKED.md`, P1 to P48. Nothing is raised from it.

## 4. What not to do

- Execute nothing from this file. It is a stop.
- No session may edit `WhatTheOpeningHeardTests.cs` or `CwUnitEstimator.cs` while unit 439 is
  live.

## 5. Committing and reporting

The arbiter commits nothing (ARBITER.md section 5). There is no unit, so there is no
`output.md`.

```
ARBITER-DECISION
STEP: 7
APPROACH: stop - the owner archived the correctness phase at d925a633 and seeded Hamlet meets the CW requirements while correctness unit 439 (bridging key-up dips in CwUnitEstimator) was still live; no unit authored
MOVE: stop
WHY: HEAD d925a633 archives this phase and commits the requirements phase's seed as WORK_INSTRUCTIONS.md, while the root phase files still name the correctness phase and its unit 439 is executing now; authoring 7.4 would overrule the owner's phase decision and put a second session into a tree a live unit is writing. Which phase runs, and whether the live unit finishes or is killed, are the owner's under ARBITER.md section 6.
STATE: partial
DECIDED: author's, overrulable - to stop rather than author a new 7.4 route, because the only untried route this arbiter found (bridging sub-15 ms key-up dips) is the one live unit 439 is already tracing; recommendation that 439 finish and the requirements seed then run as d925a633 intends
LICENCE: ARBITER.md sections 5 and 6; PHASE_PLAN.md section 6 (the later ruling wins); owner's commit d925a633; PROJECT_STATUS.md at 13:30:36
ACCOMPLISHED: nothing is built; the owner is told the new phase they committed is not yet the one the loop runs, and that a unit of the old phase is still working in the tree
ADVANCES: none - no unit is authored; the stop hands the owner the question of which phase the loop runs
END-ARBITER-DECISION
```
