# Work instruction 441 - keep the fix, then fix the speed it was hiding

**Seed under `--seed`, or by hand as unit 440 was run.** Unit 440 built the first change in a
week that makes the CW text better on the requirements' own numbers, and one metric definition
kept it out. **This unit keeps it, and then goes after the cause underneath it.** Four tasks.

**The owner's standing order for this unit, 2026-09-25:** *"No bookkeeping, no record keeping,
no stopping because some mundane point that doesn't matter, some number you made up that we
didn't quite achieve, some silly reason not to progress. Write something that materially
advances CW in the most significant way we can handle."* **Every task below is judged against
that sentence.** A task that changes no character the operator reads is a task this unit does
not have.

---

## 0. The project gate

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs
  MUST EXIST:      CW_REQUIREMENTS.md
  MUST EXIST:      CW_SPEC.md
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  MURC.sln
  root             C:\Source\HamLet

If all six are not as stated, refuse: reply with only the path you are in,
which checks failed, and "wrong project - nothing done."

If all six hold, say "Hamlet confirmed" and continue.
```

---

## 1. Rules, short

**HM-DEC-155.** No suite. Named types only, one per invocation, each with its own `timeout`.
Captures 600 s. `WhatTheOpeningHeardTests` 900 s. **Never background and poll.**

Apostrophes in quoted heredocs break; doubled backslashes collapse; `;` is refused; `rm` is
refused; Python cannot run here; `-m` more than once for a multi-line commit. Scripts go in
`.run-unit\unit441-<name>.sh`, run with `sh`.

**By hand:** take `SESSION.lock` through `tools\arbiter\lock.bat take`, release it at the end,
write nothing to `RUN_LEDGER.md`, touch nothing under `tools\arbiter\`.

**The four report headings, exactly:** `## 1. What Claude did`, `## 2. What the owner should
expect`, `## 3. What you should see`, `## 4. What's blocking us`. `UNIT:` line without brackets.
`ADVANCES: step 2 criterion 2`. WHY cites the plan.

**Nothing halts this unit.** A question goes in one line in section 4 and the work goes on.

---

## 2. Why this unit exists

```
PHASE GOAL: Hamlet meets the CW requirements.
UNIT GOAL:  The decoder prints fewer wrong letters, on the air, tonight.
ADVANCES:   step 2 criterion 2
```

**Read `CW_REQUIREMENTS.md` and `CW_SPEC.md` first. They win over this instruction.**

**Two rulings from the owner, 2026-09-25, recorded here and nowhere else in this unit:**

**R82 - MET-COVERAGE counts sure-and-right characters over characters sent.** Unit 440 found
the metric as written counts a wrong sure letter as coverage, so removing wrong letters lowered
it and kept the fix out. A metric that rewards wrong letters is not what HM-REQ-012 means. The
guard still catches dimming everything: coverage cannot rise by withholding.

**R83 - a unit of this phase changes what the operator reads, or it is not authored.** The
owner's sentence at the top of this file. No task for the record, no task for a test that
proves nothing the unit is changing, no halt on a figure that is not a requirement.

**What unit 440 measured.** 54 sure-but-wrong characters. **27 are one letter broken in two**:
the decoder reads a character gap in the middle of an `L`, `W` or `A` and prints the first
piece as `E` or `T`. **46 of the 54 were read on textbook spacing at a speed the envelope
contradicts** - `003758` read at 40 WPM with 60 ms dits, `031838` with dahs of 4.7 to 6.2 read
units, `032129` read at 8 to 12 WPM with marks of 0.2 to 0.8 units. **The decoder is timing the
sender at the wrong speed, so its clock says a letter has ended when it has not.**

**And the root under that:** `CwProbabilisticStream` emits any pattern that spells a letter as
*sure*. There is no confidence beyond "it is in the alphabet." A wrong pattern that happens to
be a letter prints with full confidence.

**G1**, built and measured by unit 440: refuse a gap reading where the character gap stands
past the word gap. MET-CER-SURE 67 to 56, MET-INVENTED 67 to 56, adjudicated readings hold,
V-11 holds, no recording loses a right character. 17:37 goes from
`CQ CQ CQ DEWTEETEEERE D ETTTB` to `CQ CQ CQ DEWB6 RE D W B`. **It is in the tree's history at
`cce7985d`, taken out at `5aa9c167`.**

---

## 3. Verify against the tree, briefly

- `cce7985d` is G1 and `5aa9c167` its take-out; `git diff 5aa9c167 HEAD -- src` is empty.
- `CwMetrics` holds the four; which type computes MET-COVERAGE and where its denominator is.
- Unit 440's `WhereTheSureWrongLettersComeFromTests` and its grouping.
- Where the path is given its speed: the grid, the held reading, and what chooses between them.
- The numbers at HEAD: MET-CER-SURE 67 of 426, MET-INVENTED 67 over 473, coverage 426 over 473
  with 359 right.

## 4. Rulings in force

`PHASE_PLAN.md` R77 to R83 and §6. **R78** is the keep rule, now with R82's coverage: a change
is kept when MET-CER-SURE falls, MET-INVENTED does not rise, **sure-and-right coverage does not
fall**, the adjudicated readings hold or move onto their own text, and V-11 holds. **A capture
row's character count falling is reported, not rejected.** **R72** no word or callsign prior.
**V-13** an inferred key is not proof by itself. **§0.2** nothing that keys or transmits.
**HM-DEC-155, HM-DEC-165, FACT-004.**

---

## 5. The tasks

### Task 0 - the change to the metric, and the entry numbers

`PHASE_OUTCOME.md` gets `## UNIT 441 - STEP 2` from the block at the foot. `PHASE_STATUS.md`
names 441. Patch-bump. **That is all the record this unit writes.**

**Then R82:** change `CwMetrics`' coverage to sure-and-right over sent, watched failing first on
a case known by construction where a wrong sure letter used to count. Print the real and
synthetic coverage before and after the definition change - the decoder is untouched, so the
number moves only because the definition did.

Entry round: both carry-forward lines, the floor tests, the four metrics.

### Task 1 - G1 goes in and stays (2.2, 2.3)

Cherry-pick `cce7985d`. **Change nothing in it.** Judge under R78 with R82's coverage, every
part a number, and keep it. Print 17:37 before and after. Write the figure to `metrics.md`.

**If some test in R78 fails that unit 440 did not see, print the number and keep going to task
2 anyway with G1 out** - do not stop, and do not narrow G1.

### Task 2 - the speed the path is given (2.2 again, and the cause under the 27)

**This is the unit's real work.** 46 of 54 wrong letters were read at a speed the envelope
contradicts. Trace it with a fact that asserts nothing: for each of the 54 with G1 in, print
the speed the path was given, where it came from (grid, held reading, estimator), and **the
speed the envelope's own marks imply** - dit and dah lengths over the character's span, one
unit either side, in milliseconds. Print the two speeds side by side and the ratio.

Then build **one change** against what that trace shows: the path takes its speed from the
envelope's marks where the grid's speed and the marks disagree by more than a ratio the trace
justifies. **Build what the trace names; do not tune after reading it.** Judge under R78 with
R82, every part a number, and keep it if it passes.

**Then print, before and after, the three recordings the trace named as worst:** `003758`,
`031838`, `032129`. One line of text each. **That is what the owner reads.**

### Task 3 - the exit round

Both carry-forward lines, the floor tests, the four metrics, and every type touched. Transmit
files print nothing against `7e209cb4`. **Report what changed in `src`**, file by file, and
that it was pushed.

---

## 6. Do not

- Do not spend a task on the record, a trace table for its own sake, or a test that proves
  nothing this unit changes.
- Do not reject a change because a capture row's character count fell. Report it.
- Do not halt on a question. One line in section 4, and go on.
- Do not narrow G1 or tune a threshold after reading a trace.
- Do not add a word or callsign prior. Do not touch what keys or transmits.
- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**

## 7. Report

`output.md` at the root, four headings exactly.

```
READ IN THIS ORDER.

A. Three lines of real CW text before and after, and the metric numbers.
B. Step 2: G1 kept or not, the speed change kept or not, each with R78's
   numbers.
C. The rest, in as few lines as it takes.
```

```
UNIT:       441 - <complete|stopped> at task N of 3, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
NUMBER:     sure-but-wrong: 54 -> <n>; MET-CER-SURE <n> -> <n>; changes kept <n> of 2
```

**Section 2, one paragraph:** does the decoder print fewer wrong letters than it did this
morning, by how many, on what evidence.

---

```
ARBITER-DECISION
STEP: 2
APPROACH: change MET-COVERAGE to sure-and-right over sent under R82, cherry-pick G1 unchanged and keep it under R78, then trace the speed each of the 54 was read at against the speed the envelope's marks imply and build one change that takes the path's speed from the marks where they disagree
MOVE: continue
WHY: PHASE_PLAN.md step 2 criterion 2.2 asks that each change be kept under R78 when MET-CER-SURE falls and nothing else in the keep rule gets worse, and criterion 2.3 that MET-CER-SURE be reported before and after every kept change
STATE: partial
DECIDED: the ratio the speed change uses is taken from the trace and stated; the per-type timeouts are the author's
LICENCE: PHASE_PLAN.md R78, R80, R81, R82, R83, section 6; V-11; V-13; R72; HM-DEC-155; CLAUDE.md 0.2
ACCOMPLISHED: the first kept reduction in confidently-wrong letters, and an attack on the speed error that produces half of them
ADVANCES: step 2 criterion 2
END-ARBITER-DECISION
```
