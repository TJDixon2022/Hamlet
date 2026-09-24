# Work instruction 413 - the words stop shattering

**Seed under `--seed`.** Step 3. The decoder reads `R I C H ARD D J UE G E L` where
`RICHARD DJUEGEL` was sent. Every letter is there and the gaps fall in the wrong places.
This unit traces why and attacks it, judged on edits against ten keyed recordings.
**Five tasks, drop from the back.**

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
own `timeout`. **The captures type is 51 rows now; give it 600 s.**

**The report's four top-level headings are exactly these, character for character:**

```
## 1. What Claude did
## 2. What the owner should expect
## 3. What you should see
## 4. What's blocking us
```

**The `UNIT:` line carries no parentheses**, and no `&`, `|`, `<`, `>`, `^`.

**Nothing in section 4 halts this phase.** Under R65 a stop 3 is legitimate only when a
criterion **this unit was authored for** cannot be met without a ruling. Every other
question - carried, noticed, or raised on the way past - is written to
`docs\phase-correctness\PARKED.md` and the report says so. **Do not carry an ask forward as
blocking.** The loop must run through the night.

## 2. The tool facts

Apostrophes in quoted heredocs break; doubled backslashes collapse; `;` is refused; `rm` is
refused; Python cannot run here; `-m` more than once for a multi-line commit. A bare
`git worktree`, `git checkout` and `git show` are refused at the prompt. Multi-step commands
go into `.run-unit\unit413-<name>.sh` and run with `sh`. Unit 412's runner scripts can be
copied under this unit's name.

## 3. Asks still outstanding

Carried per HM-DEC-139, verbatim in section 4. **Unit 412's item 1, the RF gain scale, is
answered by R65 and is now criterion 6.8; it leaves the carried list.** Nothing else is this
unit's to answer.

---

## 4. Why this unit exists

```
PHASE GOAL: Hamlet reads a CQ call correctly.
UNIT GOAL:  Find why the gaps fall in the wrong places and repair it,
            judged on edits over every keyed recording.
ADVANCES:   step 3 criterion 1
DRIFT:      0
```

**Tim, 2026-09-24:** *"I also want to work on this spacing. We're translating, but it's just
hard for me to read when it's spaced this way."*

**What the tree holds now**, after unit 412: **51 capture floor rows**, fourteen of them
from the 7.052 MHz session; **ten keyed recordings** from that session with inferred keys;
the guard, proved by dropping `E` and `T`, which took edits from 33 to 21 and broke 13 of 13
named floors - exactly the cheat the guard exists to catch.

**The fault, from the sidecars:** `R I C H ARD D J UE G E L`, `OP ERA T I ON`,
`S T M W O H`. Letters correct, word gaps inserted inside words. On 17:37 the scorer counted
**15 spaces added and 0 missing**. The decoder is splitting where it should join.

**The first candidate is already in the record.** Unit 405 of the restore phase traced red
`#44` to held gaps of 15, 1127 and 323 ms putting the character gap past the word gap, and
built **G1**: an out-of-order gap reading is not separated. **G1 cost nothing on any floor
or type and was thrown out only because no test turned green with it.** Under this phase it
is judged on edits instead. Its description is in the restore phase's material under
`docs\phase-cw-run\` and in `docs\phase-cw\unit405-reds.md` if that survived; 405's diffs
are not in the tree, so rebuild from the description as 405 rebuilt 402's.

---

## 5. Verify this instruction against the tree

Check, report any mismatch, repair nothing:

- The capture floor table holds 51 rows and the ten keyed recordings have key files.
- `docs\phase-correctness\baseline.md` holds the keyed table with the unsure-per-named
  column, and what the totals are.
- `CwScorer.Kinds` counts spaces added and missing from the alignment.
- Where the decoder decides a gap is a character gap or a word gap: `MeasureGaps`, and the
  unit estimator that scales them.
- Whether unit 405's G1 description survives anywhere in the tree. **If it does not, trace
  the fault yourself in task 1 and build from your own trace** - do not skip the task.

## 6. Rulings in force

`PHASE_PLAN.md` R59 to R65, §3 and §6.

**3.2 is the keep rule** and it is not negotiable: a change is kept only if **the total edit
count over all keyed recordings falls**, no named floor from 2.2 breaks, the three
adjudicated readings are unchanged character for character, and no capture row's named count
falls. Anything failing one of those goes back out in the next commit and the report says so.
**3.4**: after three consecutive units with no kept change the step closes partial - this is
unit one of three.
**R65** nothing carried halts the loop. **R57** placeholders are free, named counts are not.
**§0.0** no decode is called what was sent; the keys are inferred.
**§0.2** nothing that keys or transmits is touched. **HM-DEC-091**, **HM-DEC-155**,
**HM-DEC-165**, **FACT-004**, **FACT-006**.

**Record this in `DECISIONS.md`, newest first, and one row at the top of `CLAUDE.md` §1's
table dated 2026-09-24, headline **The RF gain scale is licensed, and nothing carried ever
halts the loop**, ref HM-DEC-172:**

```
---
id: HM-DEC-172
date: 2026-09-24
refs: PHASE_PLAN.md R65, criterion 6.8, work instruction 413, HM-DEC-139, HM-DEC-056, unit 411 section 4 item 1
---

**The RF gain condition may be compared with its read-back on one scale, and no carried ask
ever halts the loop.** Tim, 2026-09-24.

**The scale.** The CW receive condition asks for RF gain 255 on the radio's scale; the radio
reads it back as 100 percent. They never compare equal, so every CW tune-in writes the gain
and files the result unconfirmed, and the memory never records it. `ReceiverSetup` and
`Ic7300Rig.SetSettingAsync` may compare on a single scale so that a gain already at the
wanted value is recognized. The change makes the app write less to the radio, not more.
Nothing about keying, transmitting or power is touched.

**The loop.** A stop 3 is legitimate only when a criterion of the step a unit is working
cannot be met without a ruling on keying, transmit and safety, money, or a fact the product
states. A carried ask, a question raised in a report's section 4, a parked item, or a
finding noticed on the way past is parked and the loop goes on, however squarely it touches
one of the three. A unit does not carry an ask forward as blocking unless the criterion it
was authored for is the one that cannot be met.

**Why.** Tim asked for a night of unattended work. Unit 412 completed its work and the loop
halted on an ask 412 had carried rather than on anything blocking a criterion, which is the
second time in a night that a pile item stopped the work.

**Whose words are whose.** The rulings are Tim's; the wording is work instruction 413's
record of them. Rejected: leaving the RF gain ask parked and unanswered.
```

## 7. Status cadence

As the header says.

---

## 8. The tasks

### Task 0 - the record

`PHASE_OUTCOME.md` gets its `## UNIT 413 - STEP 3` entry from the decision block at the foot
of this file. `PHASE_STATUS.md` names unit 413 and `CURRENT_STEP: 3`. Patch-bump
`Directory.Build.props`. `DECISIONS.md` HM-DEC-172 and the `CLAUDE.md` row. **Entry round:**
both carry-forward lines, the floor tests, and **the keyed table's totals**, recorded as the
number this unit must beat.

**Drop candidate:** none.

### Task 1 - the trace (3.1)

A fact that asserts nothing, printing for every inserted space in the ten keyed recordings:
the gap in milliseconds that caused the split, the unit the estimator was using at that
moment, the character gap and word gap thresholds then in force, and the element that
preceded it. **Print the ten worst offenders in full** - the gap, the thresholds, the text
around it.

**Name the line or property that decides it**, in `src\Hamlet.RadioEngine\Cw`. That name is
what 3.1 asks for, and everything after depends on it.

**Drop candidate:** none.

### Task 2 - G1, or what the trace names (3.2, 3.3)

Build the first candidate in its own commit and judge it by 3.2's four tests, all four
reported as numbers:

1. total edits over all ten keyed recordings, before and after;
2. every named floor from 2.2, before and after;
3. the three adjudicated readings, character for character;
4. every capture row's named count, all 51.

**If the trace names something other than an out-of-order gap, build what the trace names
instead** - G1 is a candidate, not an instruction.

**Kept or out, the report says which and prints all four.** If it is out, it goes out in the
next commit.

**Drop candidate:** none.

### Task 3 - a second candidate

If task 2's change was kept, build a second aimed at the largest remaining kind from the
trace. If it was not kept, build a **different** change against the same trace - not a
narrowing of the first, which would be fitting to the keys rather than tracing.

**Drop candidate:** whole task. Say it was dropped and what was measured.

### Task 4 - the exit round

Both carry-forward lines, the three floor tests with captures at 51, the keyed table
re-scored with its totals, and every type touched. `git diff` over the transmit files
against `7e209cb4` prints nothing.

---

## 9. Parked - do not touch, do not raise

- **The acquisition failure** - the first two minutes of the 7.052 session reading
  `E ET E E`. Real, and a later criterion's. Park it; do not chase it here.
- **6.3 the reflow, 6.4 hover text, 6.5 the dead button, 6.7 tonePeak, 6.8 the RF gain
  scale.** Step 6's, and the arbiter's to author next.
- **Steps 1 and 4.** The synthetic keys and the pitch judge.
- **The tone tracker.** Licensed by the restore phase's R56, but it is step 4's question
  here and not this unit's.
- **Any key file.** The keys are fixed; a change is judged against them, never the reverse.

## 10. What not to do

- **Do not edit a key, a scored region, or a floor to make a number better.** That is the
  one thing that would make this phase worthless.
- **Do not lower a named count anywhere.** Placeholders are free; named characters are not.
- **Do not keep a change that fails any of 3.2's four tests**, however good its edits look.
- **Do not narrow a failed change until it passes** - trace again instead.
- **Do not touch what keys or transmits.**
- **Do not halt for a question.** Park it (R65).
- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**
- **Report mismatches; repair nothing. American spelling. UTF-8. The four headings exactly.**

## 11. Committing and pushing

Commit per task. Push at the end and say whether it succeeded.

---

## 12. Reporting

`output.md` at the root, the four headings exactly as section 1 gives them.

```
READ IN THIS ORDER.

A. Total edits over the ten keyed recordings, before and after, and whether
   any change was kept.
B. Step 3's criteria: 3.1 the trace, 3.2 the keep rule, 3.3 the running
   total, 3.5 the exit round.
C. The rest. Section 4 raises <n> items, none of them blocking.
```

```
UNIT:       413 - <complete|stopped> at task N of 4, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   yes | no - <why, on the line>
NUMBER:     total edits over the keyed recordings: <n> -> <n>
DRIFT:      <0 if a criterion moved>
```

**Section 2 tells the owner in one paragraph** whether the words are less shattered than
they were, in plain words, with one line of text before and after.

---

```
ARBITER-DECISION
STEP: 3
APPROACH: trace every inserted space in the ten keyed recordings to the gap and the thresholds that caused it, name the line that decides it, then build the change that trace names - unit 405's G1 the first candidate - and judge it on total edits, the named floors, the adjudicated readings and all 51 capture rows
MOVE: continue
WHY: PHASE_PLAN.md step 3 criterion 3.1 asks that the dominant error kind named in 0.3 be traced to a named line or property in src/Hamlet.RadioEngine/Cw, printed by a fact that asserts nothing, before any change is built
STATE: not started
DECIDED: which candidate is built first, the form of the trace printer and the per-type timeouts are the author's, overrulable
LICENCE: PHASE_PLAN.md R57, R61, R63, R64, R65, section 3, section 6; HM-DEC-172; HM-DEC-091; HM-DEC-155; HM-DEC-139; CLAUDE.md 0.0 and 0.2
ACCOMPLISHED: the words Tim reads on the CW tab stop breaking apart mid-word, or the reason they cannot yet is measured on ten real recordings
ADVANCES: step 3 criterion 1
END-ARBITER-DECISION
```
