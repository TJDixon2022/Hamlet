# Work instruction 444 - WB6RED read as one callsign again

**Authored by the arbiter against step 2's open criterion 2.5.** By the plan's checkboxes, step 2
stands at 3 of 5: 2.1, 2.2 and 2.3 are ticked. 2.4 is a closing rule that fires after three
consecutive step-2 units with no kept change. No unit can build it, so this unit does not aim at it.
**2.5 is open for one reason: 17:37's named floor is red.** It was banked at 46 and now reads 38.

17:37 went red under G1 (`42d5dbb9`), one of the two changes 441 kept for 2.2. On 17:37, G1 took
the sure letters that were wrong or added from 14 to 3, and the right ones from 14 to 17. It also
made the **word boundaries worse, from 5 wrong to 7**. The key, inferred, is
`CQ CQ CQ DE WB6RED WB6RED`. Before G1 the decoder read `CQ CQ CQ DEWTEETEEERE D ETTTB 7E E I`.
Under G1 it reads **`CQ CQ CQ DEWB6 RE D W B 7E E I`**. The letters are much closer, but the operator
reads a callsign cut into pieces and `DE` glued to it.

Units 442 and 443 did not re-bank the floor, because R78 names MET-WBE among its metrics and
17:37's boundaries got worse (443's DECIDED 3, in force). **The only honest way to turn 17:37
green is a change that gives the boundaries back.** A lowered floor does not count. This unit
traces the gaps G1 re-classified on 17:37 and builds one change that restores them. If the change
is kept and every requirement metric on 17:37 is no worse than before G1, the floor is re-banked
and 2.5 can be ticked.

**The owner's standing order still holds, 2026-09-25:** *"Write something that materially
advances CW in the most significant way we can handle."* A CQ whose callsign reads as one word is
the thing the operator answers.

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

The count today: **step 2 at 3 of 5, step 3 at 1 of 6.** Steps 0 and 1 are done, and steps 4 to 8
are not started. At HEAD `1f6a5789`, on real recordings with inferred keys (unit 443's exit):

| metric | count | value |
|---|---|---|
| MET-CER-SURE | 47 of 421 sure (43 substituted, 4 added) | 0.1116 |
| MET-INVENTED | 47 over 473 | 0.0994 |
| sure-and-right coverage (R82) | 374 over 473 | 0.7907 |
| MET-WBE | 52 over 113 | 0.4602 |

Floors: captures 51 of 51, adjudicated 13 of 13, named 12 of 13 (17:37 banked 46, reads 38).

```
PHASE GOAL: Hamlet meets the CW requirements.
UNIT GOAL:  17:37's CQ reads DE and WB6RED as the words that were sent, with G1's letters kept,
            so that no requirement metric on 17:37 is worse than before G1, its named floor
            is honestly green, and step 2's floors and carry-forward lines are green at exit.
ADVANCES:   step 2 criterion 5 - the plan's line 2.5
```

**A note on the form of ADVANCES.** The launcher reads `step N criterion k`, where k is the number
after the step's dot. So `step 2 criterion 5` means the plan's line `- [ ] 2.5`. This arbiter's
first draft wrote `criterion 2.5`. The launcher read that as criterion 2, took it for the parked
2.2, and redirected it. The target has not changed. It is 2.5 throughout this instruction, and in
the commit messages as well.

**Read `CW_REQUIREMENTS.md` first, then `CW_SPEC.md`, including §11 for MET-WBE's definition.**
Where they differ from this instruction, they win. Here are the requirement ids this unit works
toward. **Quote each from the document in section 1, and report any difference as a mismatch.**

- **HM-REQ-010:** MET-CER-SURE below 1 %. This is the guard: the boundary change may not make a sure
  letter wrong.
- **HM-REQ-011:** MET-INVENTED at zero. It is a guard too: a boundary change may not add a letter.
- **HM-REQ-080, 081, 082:** word boundaries and MET-WBE, measured separately from character errors.
  Quote them.
- **V-11** from `CW_SPEC.md`: no change may redden an earlier capture to green a newer one.

## 2. Verify this instruction against the tree

Check each of these. **Where one is wrong, report it as a mismatch in section 1 and carry on. Do not
repair it.**

- `42d5dbb9` (G1) and `14f515bd` (the marks' speed) are in HEAD's ancestry. G1 is in
  `CwUnitEstimator.MeasureGaps`: it returns textbook gaps when the clipped character gap is at or
  past the word gap.
- 17:37's named floor reads 38 against a banked 46. Name the test, the file and the row that holds
  it, and **say what the floor counts**.
- `docs/phase-requirements/metrics.md` gives 17:37 as `CQ CQ CQ DEWB6 RE D W B 7E E I` under G1,
  with boundaries wrong 5 before G1 and 7 after. Confirm both at HEAD.
- **Expected failures at entry, and they are not yours to fix:** 17:37's named floor is red.
  `TheFiveToEightDecibelPlateauHolds` is the correctness phase's recorded red and is in neither
  line. The app line loses up to 5 to the dispatcher loop, and each of those types is green alone.
- **Known and not yours:** `PHASE_OUTCOME.md`'s header still lists the old titles for steps 2, 3
  and 8. `CW_SPEC.md` §11 still defines MET-COVERAGE as sure over sent, where R82 makes it
  sure-and-right. The reload also reports that `PROJECT_STATUS.md` RULES_AT says HM-DEC-165 while
  `CLAUDE.md` §1 holds CPS-DEC-0183. Report each once and do not edit any of them.

## 3. Rulings in force - do not re-argue

- **`PHASE_PLAN.md` R77 to R81, §6, and R82 and R83** as work instruction 441 recorded them:
  - **R82:** MET-COVERAGE is sure-and-right over sent.
  - **R83:** a unit of this phase changes what the operator reads, or it is not authored.
- **R78, the keep rule.** A change is kept when it moves a requirement's metric the right way and
  breaks no other requirement. **For this unit's change:**
  - MET-WBE on 17:37 falls to 5 or fewer, which is its count before G1;
  - MET-WBE over the real set does not rise;
  - MET-CER-SURE does not rise;
  - MET-INVENTED does not rise;
  - sure-and-right coverage does not fall;
  - the adjudicated readings hold, or move onto their own adjudicated text;
  - V-11 holds per keyed recording on all four metrics.

  **A capture row's character count falling is reported, not rejected.**
- **Arbiter rulings carried, not re-argued:** 443's DECIDED (3): no floor is re-banked while
  17:37's boundaries are worse than before G1. 442's DECIDED (2): 2.5 is read as green at the exit
  of every commit from the unit's working task on. This unit applies it from its task 2 on.
- **V-13:** an inferred key is not proof by itself. **V-04 and V-14:** no fixture is admitted by
  lowering a gate, and no plausibility bound is loosened to pass one.
- **R72:** no word, dictionary or callsign prior, in any form. **A gap is called a word space or a
  letter space from its duration against the unit and from the marks around it, never from the
  letters on either side or the word they would make.** Knowing that `WB6RED` is a callsign may not
  enter the rule in any form.
- **`CLAUDE.md` §0.0:** never present a guess as a decode. **§0.2:** nothing that keys or transmits.
- **HM-DEC-155:** no suite. Named types only, one per invocation, each with its own `timeout`.
  Captures get 600 s and `WhatTheOpeningHeardTests` gets 900 s. **Never background and poll.**
- **HM-DEC-165, FACT-004.**

## 4. Status cadence

At the start of each task, run `sh tools/status.sh EXECUTING "<n> of 3" code none "<one line>"`. At the
end, run `COMPLETED`. `SESSION.lock` belongs to the runner, so do not take or release it. Write nothing
to `RUN_LEDGER.md` and touch nothing under `tools\arbiter\`.

Apostrophes in quoted heredocs break. Doubled backslashes collapse. `;` is refused, `rm` is refused,
and Python cannot run here. A multi-line commit uses `-m` more than once. Scripts go in
`.run-unit\unit444-<name>.sh` and run with `sh`. **Never compose a timestamp. Read the clock.**

---

## 5. The tasks

### Task 0 - the entry

- `PHASE_OUTCOME.md` gets `## UNIT 444 - STEP 2` from the decision block at the foot.
- `PHASE_STATUS.md` names 444 and `CURRENT_STEP: 2`. Patch-bump.
- Entry round: build, both carry-forward lines, the three floor tests, and the four metrics, each
  printed as a number. **Print MET-WBE for 17:37 on its own, and 17:37's text at HEAD.**
- Commit the root's uncommitted `PHASE_OUTCOME.md`, `PHASE_STATUS.md` and `RUN_LEDGER.md` with the
  entry. **These are the runner's writes.** Commit them as they are and do not edit them.

### Task 1 - the gap trace on 17:37 (the measurement 2.5's change is built from)

**A fact that asserts nothing**, beside `WhereTheSureAddedLettersComeFromTests`. For **every gap
between marks from `CQ CQ CQ` to the end of 17:37**, print:

- the time in seconds, and its length in ms and in units;
- the unit in force and where it came from: G1's textbook gaps, or the measured ones;
- the word-gap and letter-gap thresholds in force at that hop;
- how the decoder called the gap: element, letter or word;
- how the key calls it, where the alignment can say. Where it cannot, say so;
- the marks either side, in ms and in units.

**Run it twice:** at HEAD, and with `src/Hamlet.RadioEngine/Cw` checked out at `f14b2453`, which is
the commit before G1. Restore `src` afterwards, as 443 did. **Line the two runs up gap by gap.**
Mark every gap whose call changed under G1, and say whether the key agrees with the old call or the
new one. **Name the cause in one sentence.** For example, G1's textbook gaps set the word threshold
below gaps that are letter spaces on this sender, or the reverse. Then check whether the same cause
reaches any other keyed recording whose MET-WBE rose under G1. Name each one.

**The drop candidate is the `f14b2453` half.** If the unit runs long, trace HEAD only. Give G1's
call from `CwUnitEstimator.MeasureGaps`'s own logic, and say that the before-G1 column is reasoned,
not measured.

Commit the fact and its printout.

### Task 2 - one change that gives the boundaries back (2.5)

**This is the unit's real work.** Build **one** change, aimed at the cause task 1 named. **Take the
rule from the trace. Do not tune it after you have seen R78's numbers.** The rule works on gap
durations, units and marks only (R72).

- G1 and the marks' speed stay. Do not revert or weaken either one. **If the cause is G1's
  condition itself, narrow the condition. Do not remove it.** Show that the 11 sure-wrong-or-added
  letters G1 removed on 17:37 stay removed.
- `RivalMargin` and `MarginLlr` stay as they are.

**Judge the change under §3's list, every item as a number.** Print every recording whose text
changes, before and after. Keep it or do not keep it, and give the reason in one line.

**If it is kept:**
- Commit the change on its own.
- Write MET-WBE and MET-CER-SURE before and after to `docs/phase-requirements/metrics.md`, per
  condition, with the key's kind beside each number.
- Then look at 17:37's named floor. **Re-bank it only if every requirement metric on 17:37 is no
  worse at HEAD than at `f14b2453`:** wrong-or-added, sure-and-right, and boundaries wrong. If so,
  re-bank it at its HEAD count in its own commit. Print the rule's line and the three numbers before
  and after in the commit message. If any one of them is worse, leave the floor red and say which.
- **From that commit on, every commit ends with the three floor tests and both carry-forward lines
  green**, apart from §2's recorded reds.

**If it is not kept:** leave it out of `src`, commit its diff as
`.run-unit/unit444-wbe-notkept.diff`, and record it in `metrics.md`. 17:37 stays red.

### Task 3 - the exit round

- Build, both carry-forward lines, the three floor tests, the four metrics, and every type touched,
  each printed as a number.
- **Report what changed in `src`, file by file**, that none of it keys or transmits, and that it was
  pushed.
- **Tick 2.5** in both copies of `PHASE_PLAN.md` only if all three hold:
  - the change was kept;
  - 17:37 was re-banked under task 2's rule;
  - the three floor tests (named 13 of 13) and both carry-forward lines were green at the exit of
    the re-bank commit and of every later commit of this unit.

  Print each of those commits with its floor and carry-forward numbers as the evidence line. Say
  plainly that the commits of 442 and 443 exited with 17:37 red.
- **Do not tick 2.4.**
- Report DRIFT for step 2, the consecutive step-2 units without a kept change: 441 kept, 442 did
  not. So it is 0 if this unit's change was kept, and 2 if not.

---

## 6. Parked - do not touch, do not raise

- Everything in `PARKED.md`, including step 2's question on which files the transmit check covers.
  **This unit does not define or run a transmit-file list.** It reports its `src` diff file by file,
  and that is all.
- 443's section 4. The 7.052 traffic-net recording is not in the tree, and whether `032012` sent
  `117.1.` is the owner's to hear. Neither bears on 2.5.
- The correctness phase's 5.1, which is Tim's, and everything in `PHASE_PLAN.md` §7 (Carried).
- The decision log, the traceability table, and the 43 tests that measure something else. These
  are step 8 (R80).
- `PHASE_OUTCOME.md`'s stale header, and `CW_SPEC.md` §11's coverage text.
- MET-WBE over the corpus as a target. That is step 6. This unit moves boundaries only where G1 moved
  them.

## 7. What not to do

- Do not call a gap a word space or a letter space because of the letters beside it or the word it
  would make (R72).
- Do not re-bank 17:37, or any floor, except under task 2's rule. Do not re-bank any other row.
- Do not move the rule after reading R78's numbers. Do not add a second rule to rescue a recording.
- Do not loosen a gate or a plausibility bound to pass a fixture (V-04, V-14).
- Do not revert G1 or the marks' speed.
- Do not spend a task on the record beyond task 0's entry and the ticks.
- Do not halt on a question. Write one line in section 4 and go on.
- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**

## 8. Committing and pushing

One commit per task, **one commit for the change on its own, and one for the re-bank on its own**.
Each message names the criterion it serves: `unit444 task N: <what> (2.5)`. Push to `origin/main`
after each commit. End every commit message with:

```
Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>
```

## 9. Reporting

Write `output.md` at the root. **`validate-output.bat` refuses it unless every one of these holds:**

- The ordering block comes first, within the first 60 lines. It has a `READ IN THIS ORDER.` line,
  and then lines that begin `A.`, `B.` and `C.`.
- **Line C contains the words `Section 4 raises N items`**, where N is the number.
- Then the `UNIT:` line, without brackets.
- Then the four headings exactly, and no fifth: `## 1. What Claude did`,
  `## 2. What the owner should expect`, `## 3. What you should see`, `## 4. What's blocking us`.
- Section 3 is not empty.

**Run `./tools/arbiter/validate-output.bat output.md` before you finish. Do not finish on INVALID.** If
your permissions refuse the call, say so, and check the rules by hand.

```
READ IN THIS ORDER.

A. Hamlet meets the CW requirements: step 2 at <n> of 5, step 3 at 1 of 6 by the plan's
   checkboxes, steps 0 and 1 done, 4 to 8 not started.
B. Step 2, criterion 2.5, with HM-REQ-080/081 on 17:37 and HM-REQ-010/011 as guards: the change
   <kept or not>, with §3's numbers. 17:37 boundaries wrong <7> -> <n>; MET-WBE <52> -> <n> over
   113; MET-CER-SURE <before> -> <after>; 17:37's floor <re-banked at n, or red at 38>; 2.5
   <ticked, or what is missing>.
C. What this report adds, and whether it stands in the way of a criterion in B.
   Section 4 raises <N> items; <which, if any, is in the way of a criterion in B>.
```

```
UNIT:       444 - <complete|stopped> at task N of 3, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   <yes|no> - <which criteria flipped in PHASE_PLAN.md>
NUMBER:     17:37 boundaries wrong <n> -> <n>; MET-WBE <n> -> <n>; MET-CER-SURE <n> -> <n>
DRIFT:      <consecutive step-2 units without a kept change>
```

**Section 3 opens with what the operator reads.** Give 17:37 before G1, at entry and at exit:
`CQ CQ CQ DEWTEETEEERE D ETTTB 7E E I`, `CQ CQ CQ DEWB6 RE D W B 7E E I`, and the exit text, beside
the key `CQ CQ CQ DE WB6RED WB6RED`. Then give every other recording whose text changed, before and
after.

**Section 2, in one paragraph:** Does the operator now read the callsign in 17:37's CQ as one word,
with no letter turned wrong anywhere? Say on what evidence (V-13), and whether the floors are green.

---

```
ARBITER-DECISION
STEP: 2
APPROACH: trace every gap in 17:37's CQ at HEAD and before G1 with its length, unit, thresholds and call, name the cause of the two boundaries G1 lost, build one gap-duration change that gives them back with G1 kept, and re-bank 17:37's named floor only if no requirement metric on it is worse than before G1
MOVE: work around
WHY: PHASE_PLAN.md line 2.5 asks that the three floor tests and both carry-forward lines be green at exit, and the one thing holding it red is 17:37's named floor, which 443's ruling keeps red while G1's two lost word boundaries stand; restoring those boundaries under R78 is the only route to 2.5 that does not loosen V-11. Redirect 1 was the launcher reading "criterion 2.5" as criterion 2, the parked 2.2 - the target was always 2.5, never parked, with no attempt recorded against it and this approach not found by the loop test.
STATE: partial
DECIDED: author's, overrulable - (1) 2.5 is worked by repairing the cause of 17:37's red floor rather than re-banking it, keeping 443's DECIDED (3); (2) 442's DECIDED (2), 2.5 green from the unit's working task on, is applied to this unit from task 2, and the report states that 442's and 443's commits exited red; (3) the keep rule for a boundary change adds 17:37's MET-WBE falling to 5 or fewer and the real set's MET-WBE not rising to 3.2's guards; (4) 2.4 is not authored, because no single unit can build it; (5) 443's section 4, the missing traffic-net recording and the 032012 key, is logged and not chased, since neither bears on step 2; (6) ADVANCES is written in the launcher's form "step 2 criterion 5", meaning plan line 2.5, because run-phase.bat takes the digits after "criterion" up to the first non-digit
LICENCE: PHASE_PLAN.md step 2 line 2.5, R78, R80, R81, section 6; R82 and R83 as recorded in work instruction 441; arbiter rulings 442 DECIDED (2) and 443 DECIDED (3); V-11; V-13; V-04; V-14; R72; HM-REQ-010, 011, 080, 081, 082; HM-DEC-155; HM-DEC-165; CLAUDE.md 0.0 and 0.2
ACCOMPLISHED: a CQ call whose callsign the confident-letter fix broke into pieces reads as the words that were sent again, with the fix kept, and every floor test is honestly green so step 2's exit holds
ADVANCES: step 2 criterion 5
END-ARBITER-DECISION
```
