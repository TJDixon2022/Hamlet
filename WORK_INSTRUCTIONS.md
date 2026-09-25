# Work instruction 439 - every CW test is traced to a requirement

**Seed under `--seed`.** The first unit of *Hamlet meets the CW requirements*, step 0. It
builds nothing, repairs nothing and changes no test. It reads every CW test in the tree and
says which requirement each one proves, and it says which requirements have no test at all.
**Four tasks, drop from the back.**

**Status.** `sh tools/status.sh`, real clock, after every commit and every task, and
immediately before every `dotnet test`. **Write files as UTF-8.**

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

## 1. The rules that killed sessions

**HM-DEC-155.** No suite. Only this unit's names and `docs\carry-forward-tests.txt`, run as
its top comment says. **Never background and poll.** One type per invocation, each with its
own `timeout`. The captures type is 51 rows and runs about 120 s; give it 600 s.

**This unit runs almost no tests.** It reads source. The entry and exit rounds are the only
runs it needs.

**The report's four top-level headings are exactly these, character for character:**

```
## 1. What Claude did
## 2. What the owner should expect
## 3. What you should see
## 4. What's blocking us
```

**The `UNIT:` line carries no parentheses**, and no `&`, `|`, `<`, `>`, `^`.
**`ADVANCES` reads exactly `step 0 criterion 2`.** **WHY cites a line of the plan.**
**Write `output.md` at the root before the session ends, whatever else happened.**
**Nothing in section 4 halts this phase.** Park it and go on.

## 2. The tool facts

Apostrophes in quoted heredocs break; doubled backslashes collapse; `;` is refused; `rm` is
refused; Python cannot run here; `-m` more than once for a multi-line commit. A bare
`git worktree`, `git checkout` and `git show` are refused at the prompt. Multi-step commands
go into `.run-unit\unit439-<name>.sh` and run with `sh`. **A denial parks the criterion and is
recorded; no session widens its own `allowed.txt`.**

## 3. Asks still outstanding

Carried per HM-DEC-139, verbatim in section 4. **None is this unit's to answer.** The
correctness phase's parked items travel with it in `docs\phase-correctness-run\`.

---

## 4. Why this unit exists

```
PHASE GOAL: Hamlet meets the CW requirements.
UNIT GOAL:  Say which requirement each CW test proves, and which
            requirements no test proves.
ADVANCES:   step 0 criterion 2
DRIFT:      0 - the first unit of the phase
```

**Tim, 2026-09-25:** *"We will now focus moving forward on finishing CW based on the
specifications and requirements. I want the arbiter to recenter itself during every iteration
on these two documents."* And: *"This should also inform our policy on unit tests. We should
be tracing to requirements."*

**`CW_REQUIREMENTS.md` and `CW_SPEC.md` are at the repository root and are the specification.**
Sixty-eight requirements, sixty-five must-tier. **Read them before anything else in this unit**,
and where this instruction and those documents differ, **the documents win** and the difference
goes in the report.

**Why this comes first.** Section T of the requirements already maps fifteen rows and marks
fourteen requirement groups *none*. That table, finished, is what every later unit is aimed
with: which requirements have a test, which have none, and which have a test that measures
something other than what the requirement states. Without it every later unit guesses what it
is duplicating.

**What the phase before learned, so it is not repeated.** Six units on 2026-09-25 built changes
that read the 7.052 opening correctly - 24 WPM, `EANQNID EAN■IK` where junk had stood - and
every one was rejected because capture rows' character counts fell. **Character counts are not
a requirement.** Under R78 the keep rule is now the requirements' own metrics, and the capture
floors stay only as V-11's overfitting guard.

---

## 5. Verify this instruction against the tree

Check, report any mismatch, repair nothing:

- **`CW_REQUIREMENTS.md` and `CW_SPEC.md` are at the repository root.** If either is absent,
  **stop and say so** - the phase cannot proceed without them.
- The requirement count: sixty-eight, sixty-five must-tier, and section T's existing rows.
- Every CW test file under `tests\Hamlet.RadioEngine.Tests\Cw` and any CW test elsewhere,
  including the app project's CW tests - **name where you looked**.
- `docs\unit239-failing-set.txt`, the known-reds block of `docs\carry-forward-tests.txt`, and
  `docs\cw-retired-tests.txt` all exist and how many names each holds.

## 6. Rulings in force - do not re-argue

`PHASE_PLAN.md` R77, R78, R79 and §6, and every ruling §2 carries forward.

**R77** the requirements are the specification, and a CW test names the requirement it proves.
**R78** the keep rule is the requirements' metrics, not character counts - **not used in this
unit, which changes nothing**, but it is what the traceability is for.
**R79** the loop runs unattended and recenters on the two documents every iteration.
**§0.0** never state as known what is not known: **a test whose requirement is unclear is
listed as unclear, never assigned a requirement to fill the column.**
**§0.2** nothing that keys or transmits is touched. **§12.6** repair nothing on the way past.
**HM-DEC-103** a fixture retires by ruling, one at a time - **so nothing retires here**.
**HM-DEC-155**, **HM-DEC-165**, **FACT-004**, **FACT-006**.

**Record this in `DECISIONS.md`, newest first, and one row at the top of `CLAUDE.md` §1's table
dated 2026-09-25, headline **The CW requirements are the specification, and tests trace to
them**, ref HM-DEC-183:**

```
---
id: HM-DEC-183
date: 2026-09-25
refs: CW_REQUIREMENTS.md, CW_SPEC.md, PHASE_PLAN.md R77 R78 R79, docs/phase-correctness-run/, PROJECT_CARD.md, work instruction 439 task 0, HM-DEC-175
---

**`CW_REQUIREMENTS.md` and `CW_SPEC.md` are the specification for CW, and every CW test names
the requirement it proves.** Tim, 2026-09-25.

**What is archived.** *Hamlet reads a CQ call correctly*, set 2026-09-23, closes with its
spacing repair kept (all keyed 217 edits to 167 over 565), its emission gate kept (placeholders
299 to 28 with no named character lost), the receiver conditions set from the radio's manual,
the screen sentences made true, and 5.1 - Tim's verdict - open and still his. Its 3.6, 6.5,
7.1, 7.2, 7.4, 7.6 and 7.8 are unmet and each reappears in the new phase as a requirement id.

**Why the shape changes.** On 2026-09-25 six units built changes that read the 7.052 opening
correctly and every one was rejected because capture rows' character counts fell. Character
counts are not a requirement anywhere in `CW_REQUIREMENTS.md`. What is required is MET-INVENTED
at zero, MET-CER-SURE below 1 per cent, coverage at or above 90 per cent and MET-WBE at or
below 5 per cent. The keep rule was measuring the wrong thing.

**What is set.** *Hamlet meets the CW requirements*: trace every CW test to a requirement,
build the metrics the requirements are written in, then meet them group by group - honesty,
confidence, pitch, speed, text, and the generated conditions - and Tim at the radio. A change
is kept on a requirement's metric; the capture floors stay as V-11's overfitting guard and stop
being the keep rule. Every unit reads the two documents first, and where they differ from a
plan the documents win.

**Already answered.** Section R's first row, the knowledge rule HM-REQ-004, was ruled on
2026-09-24 as HM-DEC-175: no word, dictionary or callsign prior, in any form.

**Whose words are whose.** The ruling is Tim's; the wording is work instruction 439 task 0's
record of it. Rejected: finishing the correctness phase's remaining criteria first, because
they are written against counts the requirements have superseded.
```

## 7. Status cadence

As the header says.

---

## 8. The tasks

### Task 0 - the record

`PHASE_OUTCOME.md` gets its `## UNIT 439 - STEP 0` entry from the decision block at the foot of
this file. `PHASE_STATUS.md` names unit 439 and `CURRENT_STEP: 0`. Patch-bump
`Directory.Build.props`. `PROJECT_CARD.md` `PHASE: Hamlet meets the CW requirements` and
`PHASE_SET: 2026-09-25`. `DECISIONS.md` HM-DEC-183 and the `CLAUDE.md` row. **Entry round:**
both carry-forward lines and the three floor tests, recorded.

**Drop candidate:** none.

### Task 1 - every test, and what it proves (0.1)

Walk every CW test in the tree - the engine's `Cw` folder, the app project's CW tests, and any
CW test anywhere else - and write `docs\phase-requirements\traceability.md`: one row per test
**method**, with its file, its type, its method, and **the requirement id it proves**, or the
word `none`.

**Rules for the column:**

- A test proves a requirement when what it asserts is what the requirement states. A test that
  asserts a mechanism the requirement does not name proves `none`, however useful it is.
- **Where it is unclear, write `unclear` and one line saying why** (§0.0). Do not assign a
  requirement to fill the column.
- A test may prove more than one; name them all.
- For `none` and `unclear`, add one line saying what the test does assert.

**Count them** and give the totals in the report: tests traced, `none`, `unclear`.

**Drop candidate:** none.

### Task 2 - every requirement, and what proves it (0.2)

The other direction. Extend section T's table to **every requirement id** in
`CW_REQUIREMENTS.md`, in `traceability.md`, each row naming the test that proves it or `none`.

Then state, as numbers:

- how many requirements have at least one test;
- how many have none;
- **how many have a test that measures something other than what the requirement states** -
  that third number is the one nobody has ever counted, and it is what 0.2 exists for.

**Group the `none` rows by section** so the next unit knows which of steps 2 to 7 is emptiest.

**Drop candidate:** none. This is the map.

### Task 3 - the reds and the retired (0.3)

Place every name in `docs\unit239-failing-set.txt`, the known-reds block of
`docs\carry-forward-tests.txt`, and `docs\cw-retired-tests.txt` into the same table: the
requirement it was evidence for, or `none`.

**Retire nothing, repair nothing, restore nothing** (HM-DEC-103, §12.6). A retired test that
turns out to prove a must-tier requirement is a finding for the report, not a restoration.

**Drop candidate:** whole task, with the counts stated.

### Task 4 - the exit round (0.4)

Both carry-forward lines and the three floor tests. **`git diff` over `src` and over every test
file between entry and exit prints nothing** - this unit changed no code and no test - and the
transmit files print nothing against `7e209cb4`.

---

## 9. Parked - do not touch, do not raise

- **Every requirement's substance.** Steps 1 to 7's; this unit measures nothing about the
  decoder.
- **The correctness phase's 5.1 and its parked items.** They travel with it.
- **The window reflow on a radio-announced frequency change**, and the stale sentence saying
  the operator's license covers Morse on an FT8 frequency. Banked 2026-09-25, outside this
  phase.
- **The seven rulings of section R.** The owner's; the first is already answered by HM-DEC-175.
- **Any test's fate.** Listing is not retiring.

## 10. What not to do

- **Do not change a file under `src` or any test file.** Not one line.
- **Do not retire, repair or restore any test**, red or green.
- **Do not assign a requirement to a test to fill the column.** `unclear` is an answer.
- **Do not write a requirement that is not in `CW_REQUIREMENTS.md`.** If a test proves
  something the documents do not require, that is a finding for section 4.
- **Do not touch what keys or transmits.**
- **Do not halt for a question.** Park it.
- **Write `output.md` before the session ends.**
- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**
- **Report mismatches; repair nothing. American spelling. UTF-8. The four headings exactly.**

## 11. Committing and pushing

Commit per task. Push at the end and say whether it succeeded.

---

## 12. Reporting

`output.md` at the root, the four headings exactly as section 1 gives them.

```
READ IN THIS ORDER.

A. How many of the sixty-eight requirements have a test, how many have
   none, and how many have a test that measures something else.
B. Step 0's criteria 0.1 to 0.4.
C. The rest. Section 4 raises <n> items, none blocking.
```

```
UNIT:       439 - <complete|stopped> at task N of 4, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   yes | no - <why, on the line>
NUMBER:     requirements with a test <n> of 68; with none <n>; with a test that measures something else <n>
DRIFT:      0
```

**Section 3 leads with the `none` rows grouped by section** - that is the map the next six
units are aimed with.

**Section 2 tells the owner in one paragraph** that nothing in the app changed, and how much of
the specification currently has a test behind it.

---

```
ARBITER-DECISION
STEP: 0
APPROACH: walk every CW test in the tree and name the requirement it proves or none, then extend section T to every requirement id and count how many have a test, how many have none, and how many have a test that measures something other than what the requirement states
MOVE: continue
WHY: PHASE_PLAN.md step 0 criterion 0.2 asks that section T's table be extended to every requirement id in CW_REQUIREMENTS.md, each row naming the test that proves it or none, with the counts of requirements tested, untested, and tested by something that does not measure what the requirement states
STATE: not started
DECIDED: the form of the traceability file, how a test's requirement is judged, and the per-type timeouts are the author's, overrulable
LICENCE: PHASE_PLAN.md R77, R78, R79, section 6; HM-DEC-183; HM-DEC-175; HM-DEC-103; HM-DEC-155; CLAUDE.md 0.0 and 12.6; FACT-006
ACCOMPLISHED: the project knows which parts of its own specification are tested and which are not, so every later unit is aimed rather than guessed
ADVANCES: step 0 criterion 2
END-ARBITER-DECISION
```
