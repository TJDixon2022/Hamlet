# Work instruction 449 - the forty sure-wrong letters left, one more change, or step 2 closes

**Authored by the arbiter against step 2's open criterion 2.4**, the step the launcher named.
Steps 0 and 1 are done. Step 2 is at 3 of 5 by the plan's checkboxes, step 3 at 3 of 6, and step 4
at 4 of 7. Steps 5 to 8 are at 0.

**Why step 2, after five units spent elsewhere.** Units 443 to 448 left step 2 because 2.5 is held
red and 2.4 was "a closing rule no honest unit is aimed at". That was true while step 2's count of
units with no kept change was 0 or 1. **It stands at 2 of 3** (unit 6's judge: 442 and 444 each kept
nothing). So one unit now settles step 2 either way:
- **It keeps a change.** MET-CER-SURE falls toward HM-REQ-010's one in a hundred, which is what the
  step is for, and the count goes back to 0.
- **It keeps nothing.** Then it is the third unit in a row with no kept change. Under 2.4 the trace
  goes to `PARKED.md` and step 2 closes partial, which frees the loop from a step it keeps being
  sent back to.

**This unit is aimed at the first of those.** The second is its honest fallback, and it is written
in advance so that nobody has to argue about it afterwards.

**Why a new trace.** Unit 440 traced the 54 before any change and before the pitch instrument existed.
Since then, four changes have been kept: G1, the marks' speed, the mark-shape dim edge and 447's
tracker change. What is left is 40 sure-but-wrong letters on real recordings (5 added, 35
substituted) and 14 on the synthetic set. **Nobody has traced those 40**, and nobody has put the
instrument's pitch beside them.

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

## 1. Why this unit exists

The count today: **step 2 at 3 of 5** (2.1, 2.2, 2.3), step 3 at 3 of 6, step 4 at 4 of 7, step 5
at 0 of 6. At HEAD `6b872543`, as unit 448's entry and exit measured them:

| metric | condition, key | count | value |
|---|---|---|---|
| MET-CER-SURE | real, inferred | 40 of 433 sure (5 added, 35 substituted) | 0.0924 |
| MET-CER-SURE | synthetic, exact | 14 of 173 | 0.0809 |
| MET-INVENTED | real, inferred | 40 over 473 | 0.0846 |
| MET-INVENTED | synthetic, exact | 14 over 252 | |
| sure-and-right coverage (R82) | real, inferred | 393 over 473 | 0.8309 |
| sure-and-right coverage (R82) | synthetic, exact | 159 over 252 | 0.6310 |
| MET-WBE | real, inferred | 47 over 113 | 0.4159 |
| MET-WBE | synthetic, exact | 48 over 84 | 0.5714 |
| MET-PITCH-ERR | files more than 25 Hz off; windows | 9 of 69; 256 of 1688 | |

Floors at the same HEAD: captures 51 of 51, adjudicated 13 of 13, named 10 of 13 (17:37 at 38
against 46, `032113` at 43 against 45, `032129` at 42 against 64). **Task 0 re-measures all of
these, and its numbers win over this table.**

```
PHASE GOAL: Hamlet meets the CW requirements.
UNIT GOAL:  Fewer letters printed wrong with confidence. The forty the decoder still prints
            sure and wrong are traced with the pitch the instrument hears beside each, and
            one change is built against the largest cause no unit has tried. If R78 keeps
            it, MET-CER-SURE falls. If not, step 2 has had three units with no kept change,
            and it closes partial with its trace parked.
ADVANCES:   step 2 criterion 4 - the plan's line 2.4
```

`step 2 criterion 4` is the launcher's form. It means the plan's line `- [ ] 2.4`. **If task 2's
change is kept, 2.4 does not flip, and the report says so plainly.** The unit will then have moved
HM-REQ-010's number and reset the count. By the launcher's count it did not advance, and that is
the true answer.

**Read `CW_REQUIREMENTS.md` first, then `CW_SPEC.md`**, including section A, section B and §11.
Where they differ from this instruction, the documents win. **Quote each id below from the document
in section 1 of your report, and report any difference as a mismatch.**

- **HM-REQ-010:** MET-CER-SURE below one in a hundred. This is the requirement the unit works
  toward.
- **HM-REQ-011:** MET-INVENTED at zero. **HM-REQ-012:** coverage. **MET-WBE.** These are the guards
  under R78.
- **HM-REQ-014 and 015:** the dim class's precision and rate. Measure and report them. They are not
  keep conditions, because dimming a wrong letter lowers dim precision by construction (445's
  DECIDED (4)).
- **V-11:** no change may redden an earlier capture to green a newer one.

## 2. Verify this instruction against the tree

Check each of these. **Where one is wrong, report it as a mismatch in section 1 and carry on. Do not
repair it.**

- HEAD is `6b872543`, unit 448's exit commit. In both copies of `PHASE_PLAN.md`, **2.1, 2.2 and 2.3
  are ticked, and 2.4 and 2.5 are not.**
- `tests/Hamlet.RadioEngine.Tests/Cw/WhereTheSureWrongLettersComeFromTests.cs` is 440's and 441's
  printer, and `CwMetrics.cs` computes the four metrics. `Cw/Instruments/CwPitchInstrument.cs` is
  447's instrument, and it lives under `tests`. Confirm all three.
- Step 2's count of units with no kept change is **2 of 3**: 442 (unit 2) and 444 (unit 6), with no
  step-2 unit since. Confirm it from `PHASE_OUTCOME.md`. **If it is not 2, report that, and do not
  tick 2.4 in task 2** whatever happens.
- `PARKED.md`'s header says it is written by `run-phase.bat`, never by a session, while 2.4 says the
  trace goes to `PARKED.md`. **Report this as a mismatch.** §3 says how the unit writes the line.
- **Expected failures at entry, and they are not yours to fix:**
  - The named floors: 17:37 (banked 46, reads 38), `032113` (45, reads 43), `032129` (64, reads 42).
  - `TheFiveToEightDecibelPlateauHolds` is the correctness phase's recorded red. It is in neither
    carry-forward line.
  - The app line loses up to 5 to the dispatcher loop. Each of those types is green alone. 448's
    entry saw the host hang once at 480 s. **If it hangs again, report it as the second
    occurrence**, rerun it once, and do not raise the timeout.
- **Known and not yours. Report each once and edit none of them:**
  - `PHASE_OUTCOME.md`'s header still carries the old titles for steps 2, 3 and 8.
  - `CW_SPEC.md` §11 still defines MET-COVERAGE as sure over sent.
  - `PROJECT_STATUS.md` RULES_AT says HM-DEC-165, while `CLAUDE.md` §1 holds CPS-DEC-0183.

## 3. Rulings in force - do not re-argue

- **`PHASE_PLAN.md` R77 to R81 and §6, and R82 and R83** as work instruction 441 recorded them:
  - **R82:** MET-COVERAGE is sure-and-right over sent.
  - **R83:** a unit of this phase changes what the operator reads, or it is not authored.
- **R78, the keep rule, for task 2's change.** Every item is a number:
  - MET-CER-SURE **falls** on the real set and does not rise on the synthetic set;
  - MET-INVENTED does not rise, real or synthetic;
  - sure-and-right coverage does not fall, real or synthetic;
  - MET-WBE does not rise on the real set;
  - the adjudicated readings hold, or move onto their own adjudicated text;
  - V-11 holds per keyed recording on all four metrics;
  - the count of files more than 25 Hz off does not rise. 448's reading of a MET-PITCH-ERR move
    applies: a move inside the 0.5 Hz bin is not a rise.

  **A capture row's character count falling is reported, not rejected.**
- **R75 and R76.** The tracker is open and the instrument is proved. **The instrument never enters
  the decode path** (447's DECIDED (3)). It is used by the trace and by the judgment, never by `src`.
  If the change touches `CwToneTracker`, name the clause of HM-DEC-095 or HM-DEC-127 it works
  against.
- **Arbiter rulings carried, not re-argued:**
  - **443's DECIDED (3), as 448's DECIDED (6) read it:** no floor is re-banked while 17:37's
    boundaries are worse than before G1. That covers `032113` and `032129` too. **So 2.5 stays red,
    and this unit does not tick it.**
  - **442's DECIDED (2):** a floors line is read as green at the exit of every commit from the
    unit's working task on.
- **This unit's own readings, author's, overrulable:**
  - **2.4's count.** The unit counts as one with no kept change if task 2's change is refused under
    R78, or if task 1 finds no group outside §3's excluded routes and nothing is built. Either way,
    it says which.
  - **2.4's "the trace goes to `PARKED.md`".** The trace itself stays committed under `.run-unit\`.
    The unit appends **one line** to `PARKED.md`, in the file's own form, with `drift` as the stop
    field:

    `PARKED: 2.4 | unit 449 | <date and time read from the clock> | drift | <one sentence naming the trace files and the largest group left>`

    It changes no other line, and it reports the file's header as the mismatch §2 names.
- **The routes already tried at step 2 are excluded from task 2.** Do not build any of these, or a
  re-tuning of them:
  - the rival-margin confidence (442);
  - the path's speed taken from the marks (441, kept, and not re-tuned);
  - the gap-duration change on 17:37 (444);
  - the mark-shape dim edge (445, kept, and not re-tuned);
  - the from-cold keying move (448);
  - the speed bounds (446).
- **Acquisition is parked with the owner.** The 4.5 question, what "acquiring" means for HM-REQ-102,
  is in `PARKED.md` as a promise question. **The trace may print, as a fact, whether each letter came
  before the tracker's first keyed verdict. No change may gate the sure class on acquisition or on
  the tracker's verdict.**
- **V-04 and V-14:** no fixture is admitted by lowering a gate. No separation limit, confirmation
  rule or plausibility bound is loosened. **V-06:** no digital silence in a synthetic case. **V-13:**
  an inferred key is not proof by itself.
- **`CLAUDE.md` §12.5:** a fixture built from the same misunderstanding as the code proves nothing.
- **R72:** no word, dictionary or callsign prior, in any form.
- **`CLAUDE.md` §0.0:** never present a guess as a decode. **§0.2:** nothing that keys or
  transmits.
- **HM-DEC-155:** no suite. Named types only, one per invocation, each with its own `timeout`.
  - Captures get 600 s.
  - **Never background and poll.**
- **HM-DEC-165, FACT-004.** **A package is needed: `MOVE: stop` (§6).**

## 4. Status cadence

At the start of each task, run `sh tools/status.sh EXECUTING "<n> of 3" code none "<one line>"`. At
the end, run `COMPLETED`. `SESSION.lock` belongs to the runner, so do not take or release it. Write
nothing to `RUN_LEDGER.md`, and touch nothing under `tools\arbiter\`.

What breaks in this shell:
- Apostrophes in quoted heredocs break, and doubled backslashes collapse.
- `;` is refused, `rm` is refused, and Python cannot run here.
- A multi-line commit uses `-m` more than once.

Scripts go in `.run-unit\unit449-<name>.sh` and run with `sh`. **Never compose a timestamp. Read the
clock.**

---

## 5. The tasks

### Task 0 - the entry

- Add `## UNIT 449 - STEP 2` to `PHASE_OUTCOME.md`, from the decision block at the foot.
- `PHASE_STATUS.md` names 449, with `CURRENT_STEP: 2`. Patch-bump the version.
- Commit the root's uncommitted `PHASE_OUTCOME.md`, `PHASE_STATUS.md`, `PARKED.md` and
  `RUN_LEDGER.md` with the entry. **These are the runner's writes.** Commit them as they are,
  without editing them.
- Run the entry round and print each result as a number:
  - build;
  - both carry-forward lines, **each with its wall time**;
  - the three floor tests, with the captures type's wall time;
  - the four metrics, per condition, with the key's kind beside each number, and HM-REQ-014's dim
    precision and HM-REQ-015's dim rate;
  - the count of files more than 25 Hz off, from 447's printer.

### Task 1 - the trace: the forty sure-wrong letters at HEAD

For **every sure-but-wrong letter at HEAD on the real set (inferred keys)**, print:
- the recording, what the key says was sent, what was emitted, and whether it was substituted or
  added;
- its span, the speed in force, and the speed the marks imply;
- **the decoder's pitch over the span, and the instrument's.** Give the difference in Hz, and say
  whether the tracker moved inside the span or within one second of it;
- the marks and inner gaps it rested on, with the mark-shape verdict 445's edge gives it;
- `RivalMargin` for the letter, and the rival reading;
- whether it came before the tracker's first keyed verdict in that file. **This is a fact only (§3).**
- **which of §3's excluded routes, if any, would have covered it.**

Do the same for **the 14 on the synthetic set** (exact keys). There, the pitch and speed are known by
construction.

Then **group the letters by what the decoder itself could have seen**: a signal it already computes,
not the instrument and not the key. Print the groups, largest first, with right letters counted
beside wrong ones for each group's signal. A group whose signal also covers many right letters is
not a lever, and the table has to show that. **Name the largest group that none of the excluded
routes covers.**

Commit the printer as a fact that asserts nothing, with its output as
`.run-unit/unit449-trace-head.txt`. Extend `WhereTheSureWrongLettersComeFromTests`, or add a sibling
beside it. The choice is yours; state it.

**Drop candidate:** matching the 40 against 440's 54 and 441's 43, to say which kept change removed
which letter. Do it only if time allows. If you skip it, say so. **Task 2 is not dropped.**

### Task 2 - one change against that group, under R78

**Write the test first.** One test that names **HM-REQ-010**, on a synthetic case with an exact key,
15 dB, and a shaped noise band (V-06). It must reproduce the largest group's cause, as the trace
shows it on the real files. It asserts that the letter is not printed sure and wrong. **Watch it fail
at HEAD.** If it passes at HEAD, the case does not reproduce the cause. Rebuild it from the trace, and
watch that fail.

**Then build one change in its own commit**, against that group, using only a signal the decoder
already has. The method is yours within §3. State it, and state the threshold and where in the trace
it comes from. **It is not one of §3's excluded routes, and it does not gate on acquisition.**

**Judge it under §3's R78 list, every item a number**, with the MET-PITCH-ERR column beside it
before and after. Print:
- task 1's group table again after the change;
- every recording whose text changes, before and after, with its key;
- HM-REQ-014 and 015, before and after, reported and not judged.

**If it is kept:**
- Commit it on its own, with its test.
- Write the before and after to `docs/phase-requirements/metrics.md`, per condition, with the key's
  kind beside each number. This is 2.3's running figure.
- Do not re-bank any floor. A row whose count moved is reported with its text.
- **Do not tick 2.4.** Step 2's count goes to 0.

**If it is not kept, or task 1 found no group outside the excluded routes:**
- Leave the change out of `src`, and commit its diff, with its test, as
  `.run-unit/unit449-notkept.diff`.
- Record it in `metrics.md` with the table. Name the §3 item that refused it. **Do not build a
  second change to rescue it.**
- **2.4:** if §2 confirmed the count at 2, this is the third unit in a row with no kept change.
  - Append the one `PARKED.md` line §3 gives. It names `.run-unit/unit440-trace.txt`,
    `.run-unit/unit441-trace-exit.txt` and `.run-unit/unit449-trace-head.txt`, and the largest group
    left.
  - **Tick 2.4 in both copies of `PHASE_PLAN.md`.**
  - Write in the report that **step 2 closes partial**, with 2.5 unmet and the reason: the floors
    held red under 443's DECIDED (3).

### Task 3 - the exit round

- Build, both carry-forward lines, the three floor tests, the four metrics and every type touched.
  Print each as a number, with the wall times.
- **Report what changed in `src`, file by file.** Say that none of it keys or transmits.
- **Ticks:** confirm 2.4 ticked or not, per task 2. **Do not tick 2.5**, or anything in steps 3, 4
  or 5.
- **Make the exit commit and push it.**
- Report DRIFT:
  - step 2: 0 if task 2's change was kept, 3 and closed partial if not;
  - step 3: 0, unchanged;
  - step 4: 1, unchanged;
  - step 5: 1, unchanged.

---

## 6. Parked - do not touch, do not raise

- Everything in `PARKED.md`:
  - step 2's question on which files the transmit check covers. **This unit neither defines nor
    runs a transmit-file list.** It reports its `src` diff file by file, and that is all;
  - 448's question on what "acquiring" means for HM-REQ-102, which is the owner's.
- 17:37's floor and 444's section 4 question. Both stay with the owner.
- 448's section 4 items:
  - item 1, and item 3, which waits on it, are parked with the owner;
  - item 2, the survey's cross-range ranking from cold, is step 4's (4.4), not this unit's;
  - item 4, the app host hang, is reported only if it repeats (§2).
- 446's, 445's and 443's section 4 items, and 444's half-unit dropout rule.
- The pitch proof state (4.6), the speed search (5.1 to 5.6), and the traffic-net print (3.4).
- The correctness phase's 5.1, which is Tim's, and all of `PHASE_PLAN.md` §7 (Carried).
- The decision log, the traceability table, and the 43 tests that measure something else. These are
  step 8 (R80).

## 7. What not to do

- Do not build any of §3's excluded routes, or re-tune a kept one.
- Do not gate the sure class on acquisition or on the tracker's verdict.
- Do not wire the instrument into the decode path.
- Do not tune the change to one recording.
- Do not add a second change to rescue the first.
- Do not re-bank any floor, and do not tick 2.5.
- Do not tick 2.4 if a change was kept, or if §2 found the count is not 2.
- Do not edit any line of `PARKED.md`. Append the one line, and only under task 2's rule.
- Do not correct any ruling. Report the disagreement.
- Do not add a package. If one is needed, stop and report it.
- Do not halt on a question. Write one line in section 4 and go on.
- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**

## 8. Committing and pushing

Make one commit per task, and **one commit for the change and its test on their own**. Each message
names the criterion it serves: `unit449 task N: <what> (2.4)`. Push to `origin/main` after each
commit. End every commit message with:

```
Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>
```

## 9. Reporting

Write `output.md` at the root. **`validate-output.bat` refuses it unless every one of these holds:**
- The ordering block comes first, within the first 60 lines. It has a `READ IN THIS ORDER.` line,
  then lines that begin `A.`, `B.` and `C.`.
- **Line C contains the words `Section 4 raises N items`.**
- The `UNIT:` line follows, without brackets.
- Then come these four headings exactly, and no fifth: `## 1. What Claude did`,
  `## 2. What the owner should expect`, `## 3. What you should see`, `## 4. What's blocking us`.
- Section 3 is not empty.

**Run `./tools/arbiter/validate-output.bat output.md` before you finish. Do not finish on INVALID.**

```
READ IN THIS ORDER.

A. Hamlet meets the CW requirements: step 2 at <n> of 5 <and closed partial | open>, step 3 at
   3 of 6, step 4 at 4 of 7, step 5 at 0 of 6 by the plan's checkboxes, steps 0 and 1 done,
   6, 7 and 8 not started.
B. Step 2, criterion 2.4 (HM-REQ-010): <n> sure-wrong letters at HEAD traced, largest open group
   <name> with <n>; the change <kept | not kept | not built>; MET-CER-SURE real <n> -> <n>,
   synthetic <n> -> <n>; MET-INVENTED <n> -> <n>; coverage <n> -> <n>; 2.4 <ticked, step 2
   closes partial | not ticked, count reset to 0>; 2.5 red on <floors>.
C. What this report adds, and whether it stands in the way of a criterion in B.
   Section 4 raises <N> items; <which, if any, is in the way of a criterion in B>.
```

```
UNIT:       449 - <complete|stopped> at task N of 3, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   <yes|no> - <which criteria flipped in PHASE_PLAN.md>
NUMBER:     MET-CER-SURE real <n> -> <n>; synthetic <n> -> <n>; MET-INVENTED <n> -> <n>; coverage <n> -> <n>; largest group <n>
DRIFT:      step 2 <0 | 3, closed>; step 3 0; step 4 1; step 5 1
```

**Section 3 opens with the group table**: the sure-wrong letters at HEAD by cause, largest first,
with right letters beside wrong ones for each signal, and the excluded route that covers each group,
if one does. Then:
- **three of the largest group's letters printed as text**, with the recording's words around them
  as sent and as read, before and after the change, so the owner reads the difference rather than
  the number;
- the R78 table, every item a number, with the MET-PITCH-ERR column;
- every recording whose text changed, before and after, with its key.

**Section 2, in one paragraph:** Does the operator now see fewer letters printed as certain that
were not what was sent, and which ones? Say what reads worse, if anything. Give the evidence (V-13),
and say what the synthetic case does not prove (§12.5). If step 2 closed, say in plain words what it
leaves undone: MET-CER-SURE at <n> against HM-REQ-010's one in a hundred.

---

```
ARBITER-DECISION
STEP: 2
APPROACH: re-trace the 40 sure-but-wrong letters at HEAD with the instrument pitch and tracker moves beside each, build one change against the largest group no earlier unit attacked, kept under R78, else park the trace and close step 2 partial
MOVE: work around
WHY: PHASE_PLAN.md step 2 line 2.4 closes the step after three consecutive units with no kept change, and step 2 stands at 2 of 3 (442 and 444), so one more real attempt at HM-REQ-010 either keeps a change that lowers MET-CER-SURE or closes the step partial. 2.5 stays held red by 443's DECIDED (3), and no attempt has been recorded against 2.4.
STATE: partial
DECIDED: author's, overrulable - (1) step 2 is worked as the launcher named it: 444's DECIDED (4), that no single unit can build 2.4, was true while the count was 0 or 1, and at 2 of 3 one unit settles it, so this applies a changed fact and overrules nothing; (2) the unit counts as one with no kept change if its change is refused under R78 or if the trace finds no group outside the excluded routes; (3) 2.4's trace going to PARKED.md is done by appending one line in the file's own form with drift as the stop field, naming the committed trace files, and the file header's never-by-a-session line is reported as a mismatch; (4) the routes already tried at step 2 (rival margin, marks' speed, gap duration, mark-shape edge, from-cold move, speed bounds) are excluded from the change, and no change gates the sure class on acquisition, which is parked with the owner; (5) 443's DECIDED (3) as 448's DECIDED (6) read it holds, so 2.5 is not ticked and no floor is re-banked; (6) 448's section 4 is logged and not chased: items 1 and 3 are parked, item 2 is step 4's, and item 4 is reported only if it repeats.
LICENCE: PHASE_PLAN.md step 2 lines 2.2 to 2.5; R75 and R76 as carried; R78, R80, R81, section 6; R82 and R83 as recorded in work instruction 441; arbiter rulings 442 DECIDED (2), 443 DECIDED (3), 447 DECIDED (3) and 448 DECIDED (3) and (6); V-04; V-06; V-11; V-13; V-14; R72; HM-REQ-010, 011, 012, 014, 015; HM-DEC-155; HM-DEC-165; CLAUDE.md 0.0, 0.2 and 12.5
ACCOMPLISHED: either fewer letters reach the operator printed as certain when they are not what was sent, or the owner is told plainly that step 2 has run out of routes, with the forty that remain traced to their causes and parked
ADVANCES: step 2 criterion 4
END-ARBITER-DECISION
```
