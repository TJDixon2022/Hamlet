# Work instruction 428 - a letter is junk when its neighbors are ten times surer

**Seed under `--seed`.** The owner banked a nine-minute traffic net on 7.052 that read 664
characters with 20 unsure, and its span figures separate the litter from the text cleanly -
**but only against the text beside it, not against any fixed number.** That is what this unit
builds. Four tasks, drop from the back.

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
own `timeout`. The captures type is 51 rows and ran 119 s in unit 427; give it 600 s.

**The report's four top-level headings are exactly these, character for character:**

```
## 1. What Claude did
## 2. What the owner should expect
## 3. What you should see
## 4. What's blocking us
```

**The `UNIT:` line carries no parentheses**, and no `&`, `|`, `<`, `>`, `^`.

**Write `output.md` at the root before the session ends, whatever else happened.**

**Nothing in section 4 halts this phase** (R65). Park it and go on.

## 2. The tool facts

Apostrophes in quoted heredocs break; doubled backslashes collapse; `;` is refused; `rm` is
refused; Python cannot run here; `-m` more than once for a multi-line commit. A bare
`git worktree`, `git checkout` and `git show` are refused at the prompt. Multi-step commands
go into `.run-unit\unit428-<name>.sh` and run with `sh`. Unit 427's scripts can be copied.

## 3. Asks still outstanding

Carried per HM-DEC-139, verbatim in section 4. **None of unit 427's six is this unit's**, and
P27 stays the owner's. This unit answers nothing but its own criterion.

---

## 4. Why this unit exists

```
PHASE GOAL: Hamlet reads a CQ call correctly.
UNIT GOAL:  Stop printing a letter whose own confidence is a small fraction
            of the confidence of the letters around it.
ADVANCES:   step 3 criterion 6
DRIFT:      0
```

**What the owner reads today**, from the 7.052 traffic net, 2026-09-25, nine minutes, 664
characters, 20 unsure. Real text is in there: `YOUR MESSAGE NUMBER 9 211`, `LOCAL TRAFFIC NET
<AR>`, `KA2GJV`, `N0SM`, `W5KU`, `K0WRZ`, `800 HARRISON ST`, `ANTHONY LUSCRE`, `W1AW/8 OHIO
COORDINATOR`. Between them: `EETTTEETTTTTTTTETTETETKTETEE`.

**The measurement that matters.** `OPERETTEETTTTED`, where `OPERATION` was sent, carries these
spans in the sheet's own `spanLlr` line:

```
O:629.8  P:518.0  E:74.5  R:368.4  E:48.7  T:256.6  T:255.8
E:33.8   E:16.3   T:174.3 T:189.4  T:198.0 T:185.0  E:22.7  D:373.3
```

And in `ALL LOGS WILL BE UPLOADED`, real letters run 300 to 1358 while the two intruded `E`s
sit at 64.1 and 67.2. **The litter is an order of magnitude below its neighbors.**

**Why unit 421's conclusion does not settle this.** 421 measured eight added characters and
found them at raw 33 and above, over a right `E` at 30.8, and concluded span cannot separate
them. **On this recording they separate cleanly at roughly 100** - but 100 is meaningless as a
fixed number, because a weak passage's real letters sit near 40. **The separation is relative,
not absolute.** 421 was right about an absolute bar and its sample was eight characters; this
is hundreds.

**The idea to test, and it may still be wrong.** A character whose span is a small fraction of
the median span of the characters around it is a fragment, whatever its own score. **Whether
it separates on this corpus is this unit's question, not its assumption.**

---

## 5. Verify this instruction against the tree

Check, report any mismatch, repair nothing:

- Whether the 2026-09-25 captures from 7.052 are in `tests\fixtures\cw\captured\unadjudicated`.
  **The owner banked them.** If they are not there, say so - task 1 then runs on the keyed
  recordings alone and the report says which.
- The span the sheet prints (`spanLlr`) and the one the floors and R71's bar of 13.0 use: are
  they the same number, and where is it computed?
- `CwProbabilisticDecoder.Judged` and `CharacterMargin` 1.0, the gate that admits a character
  today.
- The keyed totals at HEAD, all keyed over 565, the ten, 17:37.
- R71's bar of 13.0, and R73's clause: a character the inferred key aligns as **added**, inside
  a scored stretch of a keyed recording, may leave a floor.

## 6. Rulings in force - do not re-argue

`PHASE_PLAN.md` R59 to R74, §3 and §6.

**R73** a character the key aligns as added, inside a scored stretch of a keyed recording, may
leave a floor. **It applies nowhere else** - the unkeyed rows keep their floors - the three
adjudicated readings must be unchanged as the independent check, and **every character a
change removes is listed by name in the report, with its recording**.
**R71** the floors count at or above raw span 13.0; a row whose above-bar count falls is a
regression.
**R72** a stray letter is told by where it sits, not by how sure it is - **and no word,
dictionary or callsign prior may be added, in any form**. The relative-span rule this unit
builds is not a prior: it compares a character with its neighbors' confidence, and knows no
words.
**3.2's four tests** are the keep rule, with R66's clause on adjudicated readings.
**§0.0** no decode is called what was sent; **no word is completed or invented**.
**§0.2** nothing that keys or transmits is touched.
**HM-DEC-091**, **HM-DEC-155**, **HM-DEC-165**, **FACT-004**, **FACT-006**.

**Record this in `DECISIONS.md`, newest first, and one row at the top of `CLAUDE.md` §1's table
dated 2026-09-25, headline **A letter is judged against the confidence of the letters around
it**, ref HM-DEC-181:**

```
---
id: HM-DEC-181
date: 2026-09-25
refs: PHASE_PLAN.md R69 R71 R72 R73 and criterion 3.6, unit 421 output.md, the 2026-09-25 7.052 captures, work instruction 428
---

**A single-element character is judged against the confidence of the characters around it,
not against a fixed bar.** Tim, 2026-09-25.

**What was measured.** Unit 421 found eight added single-element characters at raw span 33 and
above, over a right `E` at 30.8, and concluded no fixed bar could separate them. On the
nine-minute traffic net captured from 7.052 on 2026-09-25, the same litter sits an order of
magnitude below its neighbors: in `OPERETTEETTTTED` the real letters run 174 to 630 and the
intruded ones 16 to 75; in `ALL LOGS WILL BE UPLOADED` the real letters run 300 to 1358 and
the two intruded `E`s sit at 64 and 67. A fixed bar fails because a weak passage's real
letters sit near 40; a relative one may not.

**What is ruled.** Criterion 3.6 is attacked by relative span: a character whose own span is a
small fraction of the median span of the characters around it is a fragment, whatever its own
score. The window and the fraction are measured, not assumed, and are chosen from the traced
distributions rather than from which changes they admit.

**What is not changed.** R72 stands: no word, dictionary or callsign prior, in any form. A
comparison with neighbors' confidence is not a prior - it knows no words.

**Whose words are whose.** The ruling is Tim's; the wording is work instruction 428's record
of it.
```

## 7. Status cadence

As the header says.

---

## 8. The tasks

### Task 0 - the record

`PHASE_OUTCOME.md` gets its `## UNIT 428 - STEP 3` entry from the decision block at the foot of
this file. `PHASE_STATUS.md` names unit 428 and `CURRENT_STEP: 3`. Patch-bump
`Directory.Build.props`. `DECISIONS.md` HM-DEC-181 and the `CLAUDE.md` row. **Entry round:**
both carry-forward lines, the three floor tests, the keyed totals, the named floors, and the
added-letter count, recorded as the numbers to beat.

**Drop candidate:** none.

### Task 1 - does it separate (3.6's trace)

A fact that asserts nothing. Over the keyed recordings **and the 2026-09-25 captures if they
are in the tree**, for every emitted character, print its span and **the median span of the
characters within a window around it** - the window is yours, say what it is and why. Then:

- the ratio of the character's span to that median;
- for characters the key aligns as **added**, the distribution of that ratio;
- for characters the key aligns as **right**, the same distribution;
- the same two for single-element characters alone (`E` and `T`), which is where the litter is.

**Print both distributions.** The question is whether a ratio exists that takes most of the
added and almost none of the right. **If they overlap, say so plainly and build nothing** -
that finding is worth more than a discarded change, and task 2 becomes a second trace on a
different window.

**Name the ratio you would use and why**, from the distributions and from nothing else.

**Drop candidate:** none.

### Task 2 - the change (3.6, 3.2)

If task 1 found a separation, demote a character whose ratio falls below it. **Demote, not
delete**: the character becomes what the decoder shows when it is not sure, so the litter stops
reaching the transcript as confident text. Build it in its own commit and judge under 3.2's
four tests, every one a number:

1. total edits over all keyed recordings, before and after, **with added letters counted
   separately** - that is the number 3.6 exists for;
2. all 13 named floors, above-bar counts;
3. the three adjudicated readings, quoted, R66's clause invoked if any moves;
4. all 51 capture rows' above-bar counts, **and every character removed listed by name with
   its recording** (R73).

**Kept only if added letters fall and the total does not rise.** A change that lowers the total
by removing wrong letters while leaving added letters where they were is not what 3.6 asks for:
report it, take it out, say so.

**Drop candidate:** none.

### Task 3 - a second pass

If task 2 kept a change, re-trace and build once more under the same rule. If it kept nothing,
trace the second window and report.

**Drop candidate:** whole task, with what was measured stated.

### Task 4 - the exit round (3.5)

Both carry-forward lines, the three floor tests with captures at 51, the keyed totals, the
named floors, and every type touched. The transmit files print nothing against `7e209cb4`.

---

## 9. Parked - do not touch, do not raise

- **Every one of unit 427's six findings**, P29 to P34, and P27.
- **Step 4 the pitch judge; 7.1 to 7.4 the speed ceiling and acquisition; 6.5 the dead button.**
- **The spacing.** Kept as units 415 and 416 left it.
- **Any key, scored region or floor.** Fixed.
- **The acquisition junk at the start of a session.** Real, and 7.3's.

## 10. What not to do

- **Do not add a word, dictionary or callsign prior, or complete or invent any text** (R72).
- **Do not raise R71's bar of 13.0.** That is the floors' bar and 3.7 is met.
- **Do not build a change the trace does not support.**
- **Do not lower an above-bar count on any unkeyed row**, and remove a character from a keyed
  row only under R73, listing it by name.
- **Do not tick 3.6 unless added letters fell.**
- **Do not touch what keys or transmits.**
- **Do not halt for a question.** Park it.
- **Write `output.md` before the session ends.**
- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**
- **Report mismatches; repair nothing. American spelling. UTF-8. The four headings exactly.**

## 11. Committing and pushing

Commit per task, each change with its red quoted. Push at the end and say whether it succeeded.

---

## 12. Reporting

`output.md` at the root, the four headings exactly as section 1 gives them.

```
READ IN THIS ORDER.

A. Added letters over the keyed recordings, before and after, and whether
   the ratio separates the added from the right.
B. Step 3's criterion 3.6, and 3.5 at exit.
C. The rest. Section 4 raises <n> items, none blocking.
```

```
UNIT:       428 - <complete|stopped> at task N of 4, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   yes | no - <why, on the line>
NUMBER:     added letters over all keyed recordings: <n> -> <n>; all keyed <n> -> <n> over 565
DRIFT:      <0 if a criterion moved>
```

**Section 3 leads with the two distributions**, added against right, so the owner can see
whether they separate at all. **Then one line of the 7.052 traffic net before and after**, if
those captures are in the tree - that is what he reads.

---

```
ARBITER-DECISION
STEP: 3
APPROACH: trace each emitted character's span against the median span of the characters around it, over the keyed recordings and the 2026-09-25 traffic net, and demote a character whose ratio falls far below its neighbors only if the added and the right separate on that ratio
MOVE: continue
WHY: PHASE_PLAN.md step 3 criterion 3.6 asks that the stray single-element characters be traced and attacked with the count of added letters reported before and after, and unit 421 measured that no fixed bar separates them
STATE: partial
DECIDED: the window, the ratio, and the per-type timeouts are the author's, overrulable, and are chosen from the traced distributions rather than from which changes they admit
LICENCE: PHASE_PLAN.md R66, R69, R71, R72, R73, section 3, section 6; HM-DEC-181; HM-DEC-091; HM-DEC-155; CLAUDE.md 0.0 and 0.2
ACCOMPLISHED: the litter between real words stops reaching the transcript as confident text, or the project knows on measured distributions that relative span cannot find it either
ADVANCES: step 3 criterion 6
END-ARBITER-DECISION
```
