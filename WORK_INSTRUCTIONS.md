# Work instruction 250 - the gate set exists, and the slow tests are named

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  MURC.sln

If all four are not as stated, you are in the wrong repository.
REFUSE. Do not read the rest of this file, do not summarise it, do not
adapt it to whatever project you are actually in, and change nothing.
Reply with only: the path you are in, which checks failed, and
"wrong project - nothing done."

If all four hold, say "Hamlet confirmed" and continue.
```

---

## THE TOOL RULE

**This session's shell may refuse calls.** `RUN_LEDGER.md` records 5 to 28
denials a unit for a fortnight, though unit 249 saw none. **The file-editing
tools have been unaffected throughout.**

- **A refused shell call is a signal to reach for the other tool, not to stop.**
  Only building and testing genuinely need the shell.
- **Record every refusal verbatim.**
- **Nothing in this unit halts the loop.** This unit's whole subject is what the
  test tooling costs, so a refusal is data rather than an obstacle.

---

## Why this unit exists

**The phase layer is already installed** - Tim ran `install-phase.bat` before
launching. **This unit moves no phase files.**

**Step 0 is already closed by work instruction 249**, which ran under the previous
phase's paused sequence and met every exit. Hamlet decodes through
`Ft8Sharp.Deep` with fine sync and ordered statistics on, 261 ms a slot against a
15,000 ms budget, every capture naming its decoder. `PHASE_STATUS.md` and
`PHASE_OUTCOME.md` carry it. **Do not redo it.**

**Tests in this tree have grown faster than their value.**

- `Ft8Sharp.Tests`: **609 tests in about 7 minutes 44 seconds.**
- `Hamlet.RadioEngine.Tests`: **2,157 tests, and it has never once completed a
  whole-project run** - started alone at 08:15 on 2026-09-01, cut off at 09:16.
- **Nobody knows which tests are expensive**, because no run has ever finished.
- The engine project has had no total in four consecutive reports.

**Every unit after this one is cheaper for this one existing**, which is why it
comes second and not last.

```
PHASE GOAL:   Everything this project has built reaches the operator's screen,
              and the decoder is taken as far as it will go.
UNIT GOAL:    A short named list of tests every unit runs, each entry naming the
              breakage it would have caught, running in under three minutes.
ADVANCES:     step 1
```

---

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim and report mismatches.
Report them; do not repair the instruction.

- `PHASE_PLAN.md` at the root is the **on-air** phase's, with step 0 done and
  step 1 current, and `docs/phase-sensitivity-run/` holds the previous phase.
  **If the root files still name the sensitivity phase, `install-phase.bat` was
  not run** - say so first, skip task 5, carry on with the rest.
- `docs/full-suite-run.md` holds four filtered commands for `Hamlet.App.Tests`,
  which stops partway when run unfiltered.
- `docs/test-baseline.md` holds what was known about suite sizes as of
  2026-09-01.
- `tools/arbiter/validate-output.bat` holds seven shape rules, rule 7 added by
  unit 249.
- Root `1.12.51` after unit 249. `Ft8Sharp` `0.10.7`, `Ft8Sharp.Deep` `0.3.0`.

Expected to fail, **never chased**:
`CwAdjudicationTests.ASpeedChangeInRealisticAudio`; the 51 CW cases in
`docs/unit239-failing-set.txt`, which fail at the baseline `d541fc8` too; the
`Ft8Sharp.Deep.Tests` whole-type-list tripwire that reddens whenever types are
added to Deep.

---

## Rulings in force

Transcribed from `PHASE_PLAN.md`. **Not to be re-argued.**

**`Ft8Sharp` is a faithful MIT port and nothing in this phase changes a line of
it.** Its value is that it cannot drift.

**`Ft8Sharp.Deep` is GPL-3.0.** Settled 2026-09-04.

**Hamlet decodes through Deep, not the port**, and both of the port's gates stay
in the path.

**A wrong decode is counted separately from a missed one, everywhere.**

**Targets are waypoints, not gates.** A step closes on the figure it reached.

**No unit assumes WSJT-X on this machine.**

**A unit may not add a test without naming the breakage it would have caught.**
This unit writes that rule down and is the first bound by it.

---

## What this unit runs

**This unit runs tests in order to time them**, which is the exception to running
as little as possible. It still never runs anything concurrently, and it never
runs `Hamlet.App.Tests` unfiltered.

---

## Status cadence

After each task, before starting the next, update `PROJECT_STATUS.md` per
`CLAUDE.md` - `STATE`, `TASK: n of m`, `BALL`, `UPDATED` from the clock, and
`NOTE` saying what is moving inside the task. The same every ten minutes while a
task is running. **Use the file-editing tools if the shell refuses.**

---

## Tasks

### Task 1 - what the tests actually cost

- Run `Ft8Sharp.Tests` and `Ft8Sharp.Deep.Tests` **with a TRX logger**, and read
  **per-test durations** out of the TRX rather than off the console. Console logs
  in this tree are UTF-16 and grepping them as UTF-8 finds nothing and reports
  zero.
- **Rank them. Name the twenty slowest with their times**, and report what
  fraction of the wall clock the top twenty account for.
- Say **why** the expensive ones are expensive where the TRX makes it visible -
  a ladder rung inside a test, a long synthetic recording, a real-time wait.
- **Attempt the same for `Hamlet.RadioEngine.Tests`.** A cut-off run still times
  the tests it reached, so run it with a TRX logger and a wall-clock cap, and
  rank whatever it got to. **Report how far it got.** *This part is
  nice-to-pass; the two FT8 projects are the must.*

### Task 2 - the gate set

Write **`docs/gate-set.md`**. For every entry: the test's full name, the property
it guards, and **the breakage it would have caught** - a real one, with the unit
number where it happened where you can find it.

**An entry that cannot name a breakage does not belong in the gate set.** That is
the whole rule, and it is what stops this list growing back into the suite it
replaces.

The properties the gate set must cover, at minimum:

- **Deep is a superset of the port** - whole-result identity, the thing unit 246
  established over 69 reference recordings and 801 messages.
- **The port's parity and CRC-14 gates are in the decode path** - unit 249's
  test that re-checks every returned message rather than assuming.
- **`Ft8Sharp` references nothing outside itself** - the boundary test that keeps
  the port separately publishable.
- **The ladder reports zero wrong** - not the rate, only that nothing is returned
  that was not sent.
- **The census reaches all three surfaces** - telemetry, sidecar, census line.
- **A decoder's identity is recorded** - unit 249's, because an unattributed
  capture cannot be read six months later.

Add others if task 1's ranking or the outcome record shows a property that has
actually broken. **Do not pad it.** A gate set of a dozen tests that each earned
their place is worth more than fifty.

### Task 3 - it runs, and it runs fast

- **A command that runs exactly the gate set and nothing else**, in
  `tools/arbiter/` beside the others, in the manner of the existing scripts.
- **Measure its wall clock.** The target is **under three minutes**.
- **If it cannot reach three minutes, say what the floor is and what is holding
  it there.** Do not drop a test that earned its place to hit a number - the
  number is a waypoint, not a gate, and a slow gate set that guards the right
  things beats a fast one that does not.
- Watch it fail: break one guarded property deliberately in a scratch change,
  confirm the gate set reddens, revert. **A gate set nobody has seen fail is a
  list, not a gate.**

### Task 4 - write the rule down where the next unit will read it

- Record in `docs/gate-set.md` the standing rule this phase runs under: **a unit
  runs the gate set, plus the channels it touched, and nothing else**; the full
  engine suite is Tim's, by hand, uncontended, once; and **no test is added
  without naming the breakage it would have caught.**
- Note the known-red inherited tests there too, so a session finds them in one
  place instead of rediscovering them.
- **If `docs/test-baseline.md` now says something false**, say so and update it
  rather than leaving two documents disagreeing about the same suites.

### Task 5 - the phase's bookkeeping

**File edits only. No shell needed.**

- `PROJECT_CARD.md` gains the new `PHASE` and `PHASE_SET` from
  `PHASE_STATUS.md`'s header. **This file changes only by ruling** (§13.3), and
  the ruling is Tim's approval of `PHASE_PLAN.md` on 2026-09-05.
- Append that ruling to `DECISIONS.md`, next id in sequence, naming the phase and
  citing the plan.
- **If `install-phase.bat` was not run, do neither** - say so and leave both
  files alone.
- Append this unit's entry to `PHASE_OUTCOME.md` through
  `tools\arbiter\outcome-append.bat`. **If the shell refuses, append with the
  file-editing tools in the format the existing entries use** and say so.

### Task 6 - the OSD re-encoding count, measured not fixed

**Named drop candidate.**

Unit 249 raised it: ordered statistics re-encoded **192,602 times** on one slot of
clean synthetic audio, and nothing bounds that number. It used 210 ms of 15,000,
so it is not a defect today.

- **Measure how it varies with candidate count.** Feed slots with few candidates
  and slots with many and report re-encodings against candidates, so the shape is
  known rather than guessed.
- **Do not cap it.** A cap would make the decoder's reach depend on how busy the
  band is, silently, which is the §0.0 fault in a new place.
- Report the worst case seen and the budget it would leave.
- If dropped, say so; it is already logged and the shack machine is where it
  would matter.

---

## Parked - do not touch, do not raise

- **Step 0.** Closed by unit 249. Do not redo it.
- **Measuring or displaying SNR.** Step 2. The `snr` column keeps its dash.
- **OSD tuning, subtraction, cross-slot combining.** Steps 3, 4 and 5.
- **Anything in `src/Ft8Sharp/`.**
- **The panel** - columns, tooltips, sort, trim; and the decoded panel's family
  colour, which unit 249 recorded as `Lavender` in the markup against a green
  sender field.
- **The CW decoder**, the 419 dropped chunks, the 51 inherited reds, the
  waterfall's late first row, `ReusableWindow`, `ProcessDelayForTests`, the tap's
  owner, unit 237's Extensible conclusion, work instruction 231's four tree
  items, `validate-output.bat`'s permitted-spellings bug, the 101.33 ms pulse
  above 6 kHz.

---

## What not to do

- **Do not stop because the shell refused something.**
- **Do not redo step 0.**
- **Do not pad the gate set.** Every entry names a breakage or it is not in it.
- **Do not drop a test that earned its place to hit three minutes.**
- **Do not cap the OSD re-encoding count.**
- **Do not change `src/Ft8Sharp/`.**
- **Do not run `Hamlet.App.Tests` unfiltered.** One project at a time, never
  concurrently - contention once turned one standing failure into five.
- **Do not ship a placeholder token in a reported number.** Rule 7 now catches
  it; do not be the report that finds out.

---

## Committing and pushing

Commit and push each task before starting the next. Root `1.12.51` to `1.12.52`.
**`Ft8Sharp` does not move.** If `git` is refused, say so and carry on.

---

## Reporting

`output.md` at the repository root, overwritten, four sections per
`CLAUDE_CODE.md` §8 with the header block above section 1.

**Section 3 leads with three things:**

1. **The twenty slowest tests** with their times, and what fraction of the wall
   clock they account for.
2. **The gate set's size and its measured wall clock**, and confirmation it was
   watched failing.
3. **How far `Hamlet.RadioEngine.Tests` got** before its cap, and the slowest it
   reached - the first time anybody has had a number for that project.

Write `output.md`, then stop. Do not start the next unit.
