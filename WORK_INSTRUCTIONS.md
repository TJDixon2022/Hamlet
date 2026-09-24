# Work instruction 416 - the relabel goes back in

**Seed under `--seed`.** Unit 415 built a space-only relabel that took every keyed recording
from 217 edits to 185 and the ten bench recordings from 60 to 36, moving no letter, no
element and no placeholder anywhere. It went out on one clause, which R66 has now amended.
**This unit re-applies it and keeps it.** Four tasks, drop from the back.

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
own `timeout`. The captures type is 51 rows; give it 600 s.

**The report's four top-level headings are exactly these, character for character:**

```
## 1. What Claude did
## 2. What the owner should expect
## 3. What you should see
## 4. What's blocking us
```

**The `UNIT:` line carries no parentheses**, and no `&`, `|`, `<`, `>`, `^`.

**Nothing in section 4 halts this phase** (R65). A question is parked in
`docs\phase-correctness\PARKED.md` and the loop goes on. **Do not carry an ask forward as
blocking** unless the criterion this unit was authored for cannot be met without it.

## 2. The tool facts

Apostrophes in quoted heredocs break; doubled backslashes collapse; `;` is refused; `rm` is
refused; Python cannot run here; `-m` more than once for a multi-line commit. A bare
`git worktree`, `git checkout` and `git show` are refused at the prompt. Multi-step commands
go into `.run-unit\unit416-<name>.sh` and run with `sh`. Unit 415's scripts can be copied.

## 3. Asks still outstanding

Carried per HM-DEC-139, verbatim in section 4. **Unit 415's item 1 is answered by R66 and
leaves the carried list.** Unit 415's fifth self-decision asked whether 3.5 should wait for a
kept change; it should not, and 3.5 stays ticked.

---

## 4. Why this unit exists

```
PHASE GOAL: Hamlet reads a CQ call correctly.
UNIT GOAL:  Re-apply unit 415's narrowed relabel b68be0dd under R66 and keep
            it, with all four of 3.2's tests printed as numbers.
ADVANCES:   step 3 criterion 2
DRIFT:      0
```

**What unit 415 measured**, and what this unit must reproduce before keeping anything:

| 3.2 test | entry | with `b68be0dd` |
|---|---|---|
| 1. total edits, all keyed | 217 over 565, inferred keys | **185 over 565** |
| - the ten bench recordings | 60 over 156 | **36 over 156** |
| - 17:37 | 29 over 25 | 28 over 25 |
| 2. named floors | 13 of 13 | 13 of 13, every count identical |
| 3. adjudicated readings | `VA3VRR`, `N4 `, `EETMP/4 QNIK` | `VA3VRR`, **`N4L`**, `EETMP/4 QNIK` |
| 4. capture rows' named counts | 51 rows | identical |

**R66, Tim, 2026-09-24:** test 3 is met when a reading is unchanged **or changed to exactly
its own adjudicated text**. `N4L` is HM-DEC-144's text, 1 edit to 0. **Any other movement of
an adjudicated reading still fails**, and the report prints any reading that moves, before
and after.

**Why the change is safe, from 415's trace:** it re-decides only whether a gap the decoder
already read between letters is announced as a space. The path is untouched, so no letter can
move; 415 measured 0 letters moved on 52 of 52 recordings with the relabel placed either side
of the settle dedupe, and built the safe placement.

---

## 5. Verify this instruction against the tree

Check, report any mismatch, repair nothing:

- Commit `b68be0dd` exists and its take-out `040a4ae0` follows it; `git diff 040a4ae0 HEAD --
  src` prints nothing, so the tree is at the pre-change decoder.
- `docs\phase-correctness\unit415-trace.md` describes the relabel, the centroid guard, and
  the `sqrt(7/3)` boundary.
- The keyed totals at HEAD are 217 over 565, the ten 60 over 156, 17:37 29 over 25.
- `PHASE_PLAN.md`'s 3.2 carries R66's clause.

## 6. Rulings in force

`PHASE_PLAN.md` R59 to R66, §3 and §6.

**R66** an adjudicated reading may move onto its own adjudicated text and nowhere else.
**3.2** is otherwise unchanged and is not negotiable: total edits over all keyed recordings
must fall, no named floor breaks, no capture row's named count falls.
**§10 of unit 415 still binds:** no key, scored region or floor is edited to make a number
better; no boundary or guard is chosen from a score.
**R65** nothing carried halts the loop. **§0.0** the keys are inferred and no decode is called
what was sent. **§0.2** nothing that keys is touched. **HM-DEC-091**, **HM-DEC-155**,
**HM-DEC-165**, **FACT-004**.

**Record this in `DECISIONS.md`, newest first, and one row at the top of `CLAUDE.md` §1's
table dated 2026-09-24, headline **A reading that moves onto its own adjudicated text has not
been damaged**, ref HM-DEC-173:**

```
---
id: HM-DEC-173
date: 2026-09-24
refs: PHASE_PLAN.md R66 and criterion 3.2, unit 415 output.md section 4 item 1, HM-DEC-144, work instruction 416
---

**A reading that changes to exactly its own adjudicated text has not been damaged, and
3.2's third test allows it.** Tim, 2026-09-24.

**What was at stake.** Unit 415 built a space-only relabel that moved no letter, no element
and no placeholder on any of the 51 capture rows, held all 13 named floors identical, and
took every keyed recording from 217 edits to 185 over 565 characters and the ten bench
recordings from 60 to 36 over 156. It was taken out on one clause: on `cw-2026-08-17-134712`
the reading moved from `N4 ` to `N4L`, which is the text HM-DEC-144 adjudicated.

**What is ruled.** The third test of 3.2 reads: the three adjudicated readings are unchanged
character for character, or changed to exactly their own adjudicated text. Any other
movement of an adjudicated reading still fails it, and a report invoking the clause prints
the reading before and after so the owner can see which happened.

**Why.** The test exists so that a change cannot buy total edits by damaging a reading
somebody ruled on. A reading that becomes the ruled text has not been damaged.

**Whose words are whose.** The ruling is Tim's; the wording is work instruction 416's record
of it. Rejected: leaving the test as written; narrowing the relabel until `134712` does not
move, which would choose the boundary from a score rather than from a trace.
```

## 7. Status cadence

As the header says.

---

## 8. The tasks

### Task 0 - the record

`PHASE_OUTCOME.md` gets its `## UNIT 416 - STEP 3` entry from the decision block at the foot
of this file. `PHASE_STATUS.md` names unit 416 and `CURRENT_STEP: 3`. Patch-bump
`Directory.Build.props`. `DECISIONS.md` HM-DEC-173 and the `CLAUDE.md` row. **Entry round:**
both carry-forward lines, the three floor tests, the keyed totals, the named floors, recorded
as the numbers to beat.

**Drop candidate:** none.

### Task 1 - the relabel goes back in (3.2, 3.3)

Re-apply `b68be0dd` in its own commit - `git diff 3ddca565 b68be0dd` gives it, or rebuild it
from `unit415-trace.md` if the diff will not apply cleanly, and say which. **Do not change
it.** Not the boundary, not the guard, not the placement.

Then run all four of 3.2's tests and print every one as a number beside 415's figures above:

1. total edits over all keyed recordings, and the three sub-rows;
2. all 13 named floors, each count;
3. the three adjudicated readings, quoted before and after, with R66 invoked explicitly for
   any that moved;
4. all 51 capture rows' named, element and placeholder counts, diffed against entry.

**If every test passes, the change is kept and 3.2 is ticked.** If any fails, it goes out in
the next commit and the report prints why - **and the failure must be a number, not a
judgment.**

**Drop candidate:** none.

### Task 2 - what it looks like (3.3)

Print, from the kept build, the settled text of `cw-2026-09-24-004322` and `-004405` beside
what they read before, as 415 did:

```
now:        P O N S ORED A M ER I CA 2 5 9 OP ERA T I ON X ALL L O G S
narrowed:   P O N S ORED AMERICA 25 9 OPERATION X ALL LOGS
```

Update `docs\phase-correctness\baseline.md` with the new running total. **This is what Tim
reads in the morning, so it goes at the top of section 3.**

**Drop candidate:** the baseline update only; the printed lines stay.

### Task 3 - the next kind (3.2 again, if the clock allows)

With the relabel kept, re-run 415's trace to see what the largest remaining kind is now, and
build one change against it under the same four tests. **Trace first, then build.** If the
clock is short, print the trace and say the build was dropped.

**Drop candidate:** whole task.

### Task 4 - the exit round (3.5)

Both carry-forward lines, the three floor tests with captures at 51, `TheNumberCannotBeGamedTests`,
`TheBaselineIsScoredTests`, `TheBenchmarkIsKeyedTests`, and every type touched. The transmit
files print nothing against `7e209cb4`.

---

## 9. Parked - do not touch, do not raise

- **The acquisition failure** - the first two minutes of the 7.052 session.
- **6.3 the reflow, 6.4 hover text, 6.5 the dead button, 6.7 tonePeak, 6.8 the RF gain
  scale.** Step 6's, and the arbiter's to author next.
- **Step 4, the pitch judge.**
- **P6, P7, P8** in `PARKED.md`.
- **Any key, scored region or floor.** Fixed.

## 10. What not to do

- **Do not modify the relabel to improve a number.** It goes back in as it was built.
- **Do not edit a key, a scored region or a floor.**
- **Do not let an adjudicated reading move anywhere but onto its own adjudicated text.**
- **Do not touch what keys or transmits.**
- **Do not halt for a question.** Park it.
- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**
- **Report mismatches; repair nothing. American spelling. UTF-8. The four headings exactly.**

## 11. Committing and pushing

Commit per task. Push at the end and say whether it succeeded.

---

## 12. Reporting

`output.md` at the root, the four headings exactly as section 1 gives them.

```
READ IN THIS ORDER.

A. Whether the relabel was kept, and the total: 217 -> <n> over 565.
B. Step 3's criteria: 3.2 kept or out with all four tests as numbers,
   3.3 the running total, 3.4 the count, 3.5 the exits.
C. The rest. Section 4 raises <n> items, none blocking.
```

```
UNIT:       416 - <complete|stopped> at task N of 4, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   yes | no - <why, on the line>
NUMBER:     all keyed: 217 -> <n> over 565; the ten bench: 60 -> <n> over 156
DRIFT:      <0 if a criterion moved>
```

**Section 2 tells the owner in one paragraph** that the words no longer break apart
mid-word, with one line of real text before and after, and that no letter changed.

---

```
ARBITER-DECISION
STEP: 3
APPROACH: re-apply unit 415's narrowed space-only relabel b68be0dd unchanged, run all four of 3.2's tests and print each as a number, and keep it under R66's amended third test
MOVE: continue
WHY: PHASE_PLAN.md step 3 criterion 3.2 asks that each change be kept when the total edit count over all keyed recordings falls, no named floor breaks, the adjudicated readings are unchanged or changed to exactly their own adjudicated text, and no capture row's named count falls
STATE: partial
DECIDED: whether the diff is cherry-picked or rebuilt from the trace, and the per-type timeouts, are the author's, overrulable
LICENCE: PHASE_PLAN.md R63, R65, R66, section 3, section 6; HM-DEC-173; HM-DEC-144; HM-DEC-091; HM-DEC-155; CLAUDE.md 0.0 and 0.2
ACCOMPLISHED: the words on the CW tab stop breaking apart mid-word, at no cost to a single letter, and the phase's first kept correctness gain is in the tree
ADVANCES: step 3 criterion 2
END-ARBITER-DECISION
```
