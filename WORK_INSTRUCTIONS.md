# Work instruction 440 - the decoder stops being sure and wrong

**Runs either way.** If `tools\arbiter\run-phase.bat` will launch, seed it with `--seed`. If it
still halts on the stale stop 4, run it by hand in Claude Code exactly as unit 439 was run -
section 1 says what changes. **Four tasks, drop from the back.**

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

If all six are not as stated, you are in the wrong repository or the
specification is missing.
REFUSE. Do not read the rest of this file, do not summarise it, do not
adapt it to whatever project you are actually in, and change nothing.
Reply with only: the path you are in, which checks failed, and
"wrong project - nothing done."

If all six hold, say "Hamlet confirmed" and continue.
```

---

## 1. If this runs by hand

No arbiter, no judge. Take `SESSION.lock` at the start through `tools\arbiter\lock.bat take`
and release it at the end. **Write nothing to `RUN_LEDGER.md` and touch nothing under
`tools\arbiter\`.** Tick criteria only as section 6 licenses. Write `output.md` at the root in
the four headings, and say in section 1 that the unit ran by hand.

## 2. The rules that killed sessions

**HM-DEC-155.** No suite. Only this unit's names and `docs\carry-forward-tests.txt`, run as
its top comment says. **Never background and poll.** One type per invocation, each with its own
`timeout`. Captures is 51 rows at about 125 s; give it 600 s. **`WhatTheOpeningHeardTests` no
longer fits 600 s - give it 900 s or run its printers separately** (unit 439's finding 3).

Apostrophes in quoted heredocs break; doubled backslashes collapse; `;` is refused; `rm` is
refused; Python cannot run here; `-m` more than once for a multi-line commit. Multi-step
commands go into `.run-unit\unit440-<name>.sh` and run with `sh`.

**The four report headings are exactly these, character for character:**

```
## 1. What Claude did
## 2. What the owner should expect
## 3. What you should see
## 4. What's blocking us
```

**The `UNIT:` line carries no parentheses**, and no `&`, `|`, `<`, `>`, `^`.
**`ADVANCES` reads exactly `step 2 criterion 1`.** **WHY cites a line of the plan.**

---

## 3. Why this unit exists

```
PHASE GOAL: Hamlet meets the CW requirements.
UNIT GOAL:  The decoder stops printing the wrong letter and believing it.
ADVANCES:   step 2 criterion 1
```

**Read `CW_REQUIREMENTS.md` and `CW_SPEC.md` first.** They are the specification. Where they
and this instruction differ, **the documents win** and the difference goes in the report.

**What unit 439 measured, by hand, on 2026-09-25.** MET-INVENTED is **67** against
HM-REQ-011's zero, over 473 characters sent, inferred keys, 23 of 23 recordings. And it is
**not** what the tree had been counting:

- **13 sure characters added** - the stray `E`, `T` and `<BT>` litter. Step 3's.
- **54 sure characters wrong** - **this unit's**. The decoder emitted a character as sure, and
  the key says a different character was sent.

The tree's old figure was 17 added letters. **The 54 wrong-when-sure are the larger half and
had never been counted at all.** They are what makes `W1AW/8 OHIO` read `W1 TW /8 W H I O`:
not junk between words, but the wrong letter in the middle of a right one, printed with
confidence.

**Why this is first.** R81. It is the biggest measured pile of errors in the tree, it is
measurable today with the metrics unit 439 built, and it changes what the operator reads.

---

## 4. Verify this instruction against the tree

Check, report any mismatch, repair nothing:

- **Unit 439's `output.md`** at the root, and where it put the metrics and `traceability.md`.
  **Take the 54 from its measurement**, not from a fresh count, unless the count differs - in
  which case say so and use the fresh one.
- **`CW_SPEC.md`'s definition of MET-CER-SURE**, quoted into the report, and the denominator
  unit 439 took (the sure characters emitted, not characters sent) - **and unit 439's finding 4,
  that the spec reads two ways here.** Use unit 439's reading and say so.
- The four metrics exist and are reachable by the CW test types; name the type that calls each.
- The keyed recordings, their key kinds and counts.
- `CwProbabilisticDecoder.Judged`, `CharacterMargin`, and how a character becomes **sure**
  rather than a placeholder.

## 5. Rulings in force - do not re-argue

`docs\phase-requirements\PHASE_PLAN.md` R77 to R81 and §6.

**R78 is the keep rule**: a change is kept when **MET-CER-SURE falls, MET-INVENTED does not
rise, MET-COVERAGE does not fall**, the three adjudicated readings are unchanged or move onto
their own adjudicated text, and **V-11 holds** - no capture is reddened to green a newer one.
**The capture floors are V-11's overfitting guard only. A capture row's character count falling
is reported, not rejected**, when no requirement's metric got worse.
**R80** the record and the tests come last. **This unit writes no traceability, no test
inventory and no decision-log repair.** It records no ruling: none is given here.
**R72 / HM-DEC-175** no word, dictionary or callsign prior, in any form.
**R61 / V-13** an inferred key is labelled inferred everywhere, and **disagreement with it is
not by itself proof the decoder is wrong** - so where a wrong character sits against an
inferred key, say so, and where the key itself is doubtful, **report it and do not count it**.
**§3.1** a number is always its metric, its condition, its count and the key's kind. Never a
bare percentage.
**§0.0** never state as known what is not known. **§0.2** nothing that keys or transmits is
touched. **HM-DEC-091**, **HM-DEC-155**, **HM-DEC-165**, **FACT-004**, **FACT-006**.

---

## 6. The tasks

### Task 0 - the record, and the two ticks

Short. `PHASE_OUTCOME.md` gets `## UNIT 440 - STEP 2` from the decision block at the foot of
this file. `PHASE_STATUS.md` names unit 440 and `CURRENT_STEP: 2`. Patch-bump
`Directory.Build.props`.

**Tick steps 0 and 1 in `PHASE_PLAN.md` from unit 439's report** - 0.1 to 0.4 and 1.1 to 1.5 -
**without re-measuring**, as R80 licenses. Name the report's figures beside each tick in the
outcome entry: 5 of 68 traced, 63 none, 43 mismeasuring, and the four metrics built.

**Entry round:** both carry-forward lines, the three floor tests, MET-CER-SURE, MET-INVENTED
and MET-COVERAGE, recorded as the numbers to beat.

**Drop candidate:** none.

### Task 1 - the 54, traced (2.1)

A fact that asserts nothing. For **every sure-but-wrong character**, print:

- the recording and the time within it;
- what the key says was sent, and what the decoder emitted;
- its span, and the spans of the characters either side;
- the speed and the pitch in force at that hop;
- **the marks and gaps the character rested on**, in milliseconds and in units.

**Then group them by what they have in common** and print the groups with their counts. The
grouping is the author's - by what was sent, by what came out, by the element pattern, by
whether the character sat beside a space the decoder inserted, by pitch error at that hop -
and the reason goes in the report.

**This grouping is the whole point of the task.** 54 individual mistakes are not actionable; a
group of 30 that share one cause is.

**If a wrong character's key is itself doubtful** (V-13), say so and leave it out of the groups,
counting it separately.

**Drop candidate:** none.

### Task 2 - the change (2.2, 2.3)

Build against **the largest group task 1 found**, in its own commit, and judge under R78's
keep rule with every part a number:

1. MET-CER-SURE before and after, per condition, key kind beside each;
2. MET-INVENTED before and after;
3. MET-COVERAGE before and after;
4. the three adjudicated readings, quoted;
5. all 51 capture rows - **report what falls, do not reject on it**, and state plainly whether
   V-11 is broken: was an earlier capture reddened to green a newer one.

**Kept only if MET-CER-SURE falls and nothing else in that list gets worse.**

**Do not build against a group the trace does not support**, and **do not tune a threshold
after reading the trace to make a group pass** - that is fitting, and it is how thirty-four
changes were thrown away this week.

Write the running figure to `docs\phase-requirements\metrics.md`.

**Drop candidate:** none.

### Task 3 - a second group

If task 2 kept a change, re-trace and build against the next largest group under the same rule.
If it kept nothing, build against a **different** group - not a narrowing of the first.

**Drop candidate:** whole task, with what was measured stated.

### Task 4 - the exit round (2.5)

Both carry-forward lines, the three floor tests with captures at 51, the three metrics, and
every type touched. The transmit files print nothing against `7e209cb4`.

---

## 7. Parked - do not touch, do not raise

- **The 13 added characters.** Step 3's, next.
- **The pitch and the tracker** (step 4), **the speed** (step 5), **the boundaries** (step 6),
  **the generator** (step 7).
- **The decision log's gaps, HM-DEC-182, the version scheme, the 43 mismeasuring tests, the 63
  untested requirements.** All step 8's, last (R80).
- **The launcher's stop 4.** The other thread's; do not touch `tools\arbiter\`.
- **MET-COVERAGE reading above 1.0** on one synthetic (unit 439's finding 4). Report it if it
  appears; do not redefine the metric here.

## 8. What not to do

- **Do not spend a task on the record or on tests** (R80).
- **Do not reject a change because a capture row's character count fell.** R78: report it.
- **Do not tune after the trace.** Build what the trace names.
- **Do not treat disagreement with an inferred key as proof the decoder is wrong** (V-13).
- **Do not add a word, dictionary or callsign prior** (R72).
- **Do not touch what keys or transmits.**
- **Do not halt for a question.** Park it.
- **Write `output.md` before the session ends.**
- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**
- **Report mismatches; repair nothing. American spelling. UTF-8. The four headings exactly.**

## 9. Committing and pushing

Commit per task. Push at the end and say whether it succeeded.

---

## 10. Reporting

`output.md` at the root, the four headings exactly as section 2 gives them.

```
READ IN THIS ORDER.

A. MET-CER-SURE before and after, and whether a change was kept.
B. The 54, grouped by cause, with each group's count.
C. The rest. Section 4 raises <n> items, none blocking.
```

```
UNIT:       440 - <complete|stopped> at task N of 4, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
NUMBER:     sure-but-wrong characters: 54 -> <n>; MET-CER-SURE <n> -> <n>
```

**Section 3 leads with the groups.** Then one line of real text before and after, from a
recording a group touched - that is what the owner reads.

**Section 2 tells the owner in one paragraph** whether the decoder still prints wrong letters
with confidence, and on what evidence.

---

```
ARBITER-DECISION
STEP: 2
APPROACH: trace every sure-but-wrong character with its recording, key, span, speed, pitch and the marks it rested on, group the 54 by cause, then build against the largest group and keep it only if MET-CER-SURE falls with nothing else worse
MOVE: continue
WHY: PHASE_PLAN.md step 2 criterion 2.1 asks that the 54 sure-but-wrong characters be traced by a fact that asserts nothing, each with its recording, what was sent, what was emitted, its span, the speed and pitch in force and the marks it rested on, grouped by what they have in common
STATE: not started
DECIDED: how the 54 are grouped, which group is attacked first, and the per-type timeouts are the author's, overrulable
LICENCE: PHASE_PLAN.md R77, R78, R80, R81, section 6; V-11; V-13; R72; HM-DEC-155; CLAUDE.md 0.0 and 0.2; FACT-004
ACCOMPLISHED: the largest measured pile of errors in the tree - the decoder printing the wrong letter and believing it - is traced to causes and attacked on the requirement's own metric
ADVANCES: step 2 criterion 1
END-ARBITER-DECISION
```
