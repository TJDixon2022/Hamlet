# Work instruction 445 - a letter whose marks do not fit the speed is not printed as sure

**Authored by the arbiter against step 3's open criterion 3.2.** By the plan's checkboxes, step 3
stands at 1 of 6 and step 2 at 3 of 5.

**Why step 3 and not step 2.** Step 2 has two open lines, and neither can be moved by a unit
this pass:
- **2.5** is red for one reason, 17:37's named floor. Unit 444 measured the cause. 17:37's sender
  sends letter spaces of up to 320 ms and word spaces from 245 ms, so the two overlap. To give back
  the two boundaries G1 lost, a duration rule would need a threshold between 320 and 330 ms that
  fits this one sender. R72 and V-11 refuse that. The not-kept rule 444 built took 17:37 from 7 to
  8. 443's DECIDED (3) is still in force: no floor is re-banked while 17:37's boundaries are worse
  than before G1. It is an arbiter ruling, and only the owner can lift it.
- **2.4** is a closing rule, and no single unit can build it.

The plan's step-3 dependency line says: *"Independent of step 2: when one blocks the arbiter works
the other."* R81 puts steps 2 and 3 ahead of pitch and speed. So this unit works step 3.

**Where MET-INVENTED stands.** At HEAD it is 47 over 473. Only **4 are added letters**. The other
**43 are sure letters that are wrong**. Those 43 are the same letters step 2's MET-CER-SURE
counts, so a change that removes them moves both numbers.

441's exit trace (`.run-unit/unit441-trace-exit.txt`) prints the marks each of the 43 rested on.
Many rest on marks that are neither a dit nor a dah at the unit in force:
- `4` sent `....-` and read `T` on a single mark of 5.3 units;
- `A` read `E` twice on single marks of 2.0 units;
- `,` read `T` on a mark of 5.1 units;
- `T` read `A` on a mark of 7.8 units;
- the 650 Hz recording's `L`, `R`, `3`, `P` and `0` rest on marks of 0.1 to 0.5 units.

The decoder calls every known letter `High` (`CwProbabilisticStream.cs:685`). It never asks
whether the marks it read fit the letter it printed.

**This unit measures first.** For every sure letter, right and wrong, it traces how far its marks
and gaps sit from a dit, a dah and an element gap at the unit in force. **Only if that trace finds
an edge that no right letter crosses** does the unit dim the letters past it. 442 dimmed on
the rival margin at an edge the trace had not found. It lost 16 right letters for 7 wrong ones,
and it was refused. This unit does not repeat that.

**The owner's standing order still holds, 2026-09-25:** *"Write something that materially
advances CW in the most significant way we can handle."* A wrong letter printed as sure is
the thing an operator acts on.

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
are not started. At HEAD `0528afbe`, on real recordings with inferred keys (unit 444's exit, `src`
unchanged since 443):

| metric | count | value |
|---|---|---|
| MET-INVENTED | 47 over 473 (4 added, 43 substituted) | 0.0994 |
| MET-CER-SURE | 47 of 421 sure | 0.1116 |
| sure-and-right coverage (R82) | 374 over 473 | 0.7907 |
| MET-WBE | 52 over 113 | 0.4602 |

Synthetic, exact keys: MET-CER-SURE 14 of 173, 0.0809. MET-INVENTED 14 over 252.
Floors: captures 51 of 51, adjudicated 13 of 13, named 12 of 13 (17:37 banked 46, reads 38).

```
PHASE GOAL: Hamlet meets the CW requirements.
UNIT GOAL:  A letter whose marks are neither a dit nor a dah at the speed in force stops being
            printed as sure, where the trace shows no right letter is lost by it, so MET-INVENTED
            and MET-CER-SURE fall with coverage held.
ADVANCES:   step 3 criterion 2 - the plan's line 3.2
```

`step 3 criterion 2` is the launcher's form, and it means the plan's line `- [ ] 3.2`.

**Read `CW_REQUIREMENTS.md` first, then `CW_SPEC.md`**, including §11 for the metrics'
definitions. Where they differ from this instruction, they win. Here are the requirement ids this
unit works toward. **Quote each from the document in section 1, and report any difference as a
mismatch.**

- **HM-REQ-011:** MET-INVENTED at zero. This is the target.
- **HM-REQ-010:** MET-CER-SURE below 1 %. This is a target too, since the 43 are shared.
- **HM-REQ-012:** coverage at or above 90 %. This is the guard. *"Without a floor, dimming
  everything satisfies HM-REQ-010."*
- **HM-REQ-014:** dim letters correct at least 70 % of the time. Measure it and report it. It is not
  a keep condition this unit, because dimming wrong letters lowers it by construction. Say so with
  the number.
- **HM-REQ-015:** the class is available when the letter is emitted. The rule must decide at
  emission, from what the stream already holds.
- **V-11** from `CW_SPEC.md`: no change may redden an earlier capture to green a newer one.

## 2. Verify this instruction against the tree

Check each of these. **Where one is wrong, report it as a mismatch in section 1 and carry on. Do not
repair it.**

- `CwProbabilisticStream.cs` near line 685 sets `known ? CwConfidence.High : CwConfidence.Unreadable`.
  `CwConfidence.Low` exists in `CwCharacter.cs` and is documented as shown dimmed.
- `CallsignResolver` and `ContactTracker` act only on `High` letters. Confirm this and name the
  lines.
- `.run-unit/unit441-trace-exit.txt` prints 43 sure-wrong letters with their envelope marks and gaps
  in units. `docs/phase-requirements/metrics.md` records 442's rival-margin change as not kept,
  with 16 right letters dimmed for 7 wrong.
- **Expected failures at entry, and they are not yours to fix:**
  - 17:37's named floor is red (banked 46, reads 38).
  - `TheFiveToEightDecibelPlateauHolds` is the correctness phase's recorded red, and it is in
    neither line.
  - The app line loses up to 5 to the dispatcher loop. Each of those types is green alone.
- **Known and not yours:**
  - `PHASE_OUTCOME.md`'s header still lists the old titles for steps 2, 3 and 8.
  - `CW_SPEC.md` §11 still defines MET-COVERAGE as sure over sent.
  - `PROJECT_STATUS.md` RULES_AT says HM-DEC-165, while `CLAUDE.md` §1 holds CPS-DEC-0183.

  Report each once and do not edit any of them.

## 3. Rulings in force - do not re-argue

- **`PHASE_PLAN.md` R77 to R81 and §6, and R82 and R83** as work instruction 441 recorded them:
  - **R82:** MET-COVERAGE is sure-and-right over sent.
  - **R83:** a unit of this phase changes what the operator reads, or it is not authored.
- **R78, the keep rule, as 3.2 states it.** For this unit's change:
  - MET-INVENTED falls on the real set;
  - MET-CER-SURE does not rise, real or synthetic;
  - sure-and-right coverage does not fall, real or synthetic;
  - the adjudicated readings hold, or move onto their own adjudicated text;
  - V-11 holds per keyed recording on all four metrics, so no recording loses a sure-and-right
    letter.

  **A capture row's character count falling is reported, not rejected.**
- **Arbiter rulings carried, not re-argued:**
  - **443's DECIDED (3):** no floor is re-banked while 17:37's boundaries are worse than before G1.
  - **442's DECIDED (2):** a floors line is read as green at the exit of every commit from the
    unit's working task on.
- **V-13:** an inferred key is not proof by itself. **V-04 and V-14:** no fixture is admitted by
  lowering a gate, and no plausibility bound is loosened to pass one.
- **R72:** no word, dictionary or callsign prior, in any form. The rule reads marks, gaps and the
  unit in force. **It never reads the letters beside the letter, or the word they make.**
- **`CLAUDE.md` §0.0:** never present a guess as a decode. **§0.2:** nothing that keys or transmits.
- **HM-DEC-155:** no suite. Named types only, one per invocation, each with its own `timeout`.
  Captures get 600 s and `WhatTheOpeningHeardTests` gets 900 s. **Never background and poll.**
- **HM-DEC-165, FACT-004.**

## 4. Status cadence

At the start of each task, run `sh tools/status.sh EXECUTING "<n> of 3" code none "<one line>"`. At
the end, run `COMPLETED`. `SESSION.lock` belongs to the runner, so do not take or release it. Write
nothing to `RUN_LEDGER.md` and touch nothing under `tools\arbiter\`.

Some shell things break here:
- Apostrophes in quoted heredocs break, and doubled backslashes collapse.
- `;` is refused, `rm` is refused, and Python cannot run here.
- A multi-line commit uses `-m` more than once.

Scripts go in `.run-unit\unit445-<name>.sh` and run with `sh`. **Never compose a timestamp. Read
the clock.**

---

## 5. The tasks

### Task 0 - the entry

- `PHASE_OUTCOME.md` gets `## UNIT 445 - STEP 3` from the decision block at the foot.
- `PHASE_STATUS.md` names 445 and `CURRENT_STEP: 3`. Patch-bump.
- Entry round: build, both carry-forward lines, the three floor tests, and the four metrics, each
  printed as a number. **Print MET-INVENTED split into added and substituted, per condition, with
  the key's kind beside each number.**
- Commit the root's uncommitted `PHASE_OUTCOME.md`, `PHASE_STATUS.md` and `RUN_LEDGER.md` with the
  entry. **These are the runner's writes.** Commit them as they are and do not edit them.

### Task 1 - the mark-shape trace (the measurement 3.2's change is built from)

**A fact that asserts nothing**, beside `WhereTheSureWrongLettersComeFromTests`. For **every sure
letter at HEAD**, whether right, wrong or added, over the real keyed recordings, print:

- the recording, the time, what the key says was sent, what was emitted, and whether it is right,
  wrong or added;
- the unit in force at emission, and where it came from;
- each mark it rested on, in ms and in units, and **its distance from the nearer of 1 and 3
  units**, as a ratio;
- each gap inside the letter, in units, and its distance from 1 unit;
- **the letter's worst mark distance and worst gap distance**;
- how many marks it rested on, against the elements of the letter it emitted.

Then **tabulate right against wrong-or-added, bin by bin**, on the worst mark distance and on the
worst gap distance, each on its own and together. **Name the edge, if there is one:** the smallest
distance past which no right letter lies on the real set, and how many wrong-or-added letters lie
past it. Do the same on the synthetic set with exact keys. Say whether the two sets agree on the
edge.

**There is no edge if right letters lie past every distance that catches a wrong one.** If so,
say so in one line, print the table, and skip to task 3. Task 2 is then not built. That is
an allowed outcome, and it is recorded as the reason 3.2 did not move.

**The drop candidate is the synthetic half.** If the unit runs long, trace the real set only, and
say the edge is unchecked on exact keys.

Commit the fact and its printout.

### Task 2 - one change: past the edge, the letter is dim (3.2)

**Build this only if task 1 named an edge.** Build **one** change in `CwProbabilisticStream`: a
known letter whose worst distance is past task 1's edge is emitted as `CwConfidence.Low` instead of
`High`. Everything else is unchanged.

- **Take the edge from the trace, and do not move it after you have seen R78's numbers.** If R78
  refuses it, it is not kept. Do not build a second edge.
- The decision is made at emission, from the marks the stream holds (HM-REQ-015).
- Leave `CallsignResolver` and `ContactTracker` untouched. **Print every callsign the corpus
  resolves before and after.** A callsign that is no longer resolved because one of its letters
  went dim is reported, with the key beside it.
- G1, the marks' speed, `RivalMargin` and `MarginLlr` stay as they are.

**Judge the change under §3's list, every item as a number.** Also give:
- dim precision (HM-REQ-014) on the real set;
- the dim letters, each with its key.

Print every recording whose text or dimming changes, before and after. Keep the change or do not
keep it, and give the reason in one line.

**If it is kept:**
- Commit the change on its own.
- Write MET-INVENTED and MET-CER-SURE before and after to `docs/phase-requirements/metrics.md`, per
  condition, with the key's kind beside each number (3.3).
- Every later commit ends with the three floor tests and both carry-forward lines as green as they
  were at entry. 17:37 stays red under 443's DECIDED (3).
- **Do not re-bank any floor.** A named or capture row whose count moved is reported with its text.

**If it is not kept:** leave it out of `src`, commit its diff as
`.run-unit/unit445-shape-notkept.diff`, and record it in `metrics.md` with the table.

### Task 3 - the exit round

- Build, both carry-forward lines, the three floor tests, the four metrics, and every type touched,
  each printed as a number.
- **Report what changed in `src`, file by file**, that none of it keys or transmits, and that it was
  pushed.
- **Tick 3.2** in both copies of `PHASE_PLAN.md` only if the change was kept under §3's list.
  **Tick 3.3** as well only if `metrics.md` carries the per-condition before and after with the
  key's kind.
- **Do not tick 3.4, 3.5, 3.6, 2.4 or 2.5.**
  - 3.4 needs the traffic-net recording, which is not in the tree.
  - 3.6 and 2.5 stay red while 17:37 does.
- Report DRIFT for step 3: consecutive step-3 units without a kept change. 443 kept none, so it is
  0 if this unit's change was kept, and 2 if not. Report step 2's DRIFT as 2, unchanged.

---

## 6. Parked - do not touch, do not raise

- Everything in `PARKED.md`, including step 2's question on which files the transmit check covers.
  **This unit does not define or run a transmit-file list.** It reports its `src` diff file by file,
  and that is all.
- 17:37's floor and 444's section 4. Leave both to the owner, as this instruction's decision block
  says.
- 444's half-unit dropout rule, `.run-unit/unit444-wbe-notkept.diff`. It is step 6's material, and
  it is not rebuilt here.
- 443's section 4: the traffic-net recording, and the `032012` key.
- The correctness phase's 5.1, which is Tim's, and everything in `PHASE_PLAN.md` §7 (Carried).
- The decision log, the traceability table, and the 43 tests that measure something else. These
  are step 8 (R80).
- `PHASE_OUTCOME.md`'s stale header, and `CW_SPEC.md` §11's coverage text.
- Pitch. The 12 sure-wrong letters 25 Hz or more off the sender are step 4's, under R76. They are
  counted here, not attacked.

## 7. What not to do

- Do not build task 2 without an edge from task 1. Do not build at a best-available edge the way
  442 did.
- Do not move the edge after reading R78's numbers. Do not add a second rule to rescue a recording.
- Do not read the neighbouring letters, a word or a callsign in the rule (R72).
- Do not change `CallsignResolver`, `ContactTracker`, or what either does with a `Low` letter.
- Do not re-bank any floor. Do not loosen a gate or a plausibility bound (V-04, V-14).
- Do not revert G1 or the marks' speed.
- Do not spend a task on the record beyond task 0's entry and the ticks.
- Do not halt on a question. Write one line in section 4 and go on.
- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**

## 8. Committing and pushing

Make one commit per task, and **one commit for the change on its own**. Each message names the
criterion it serves: `unit445 task N: <what> (3.2)`. Push to `origin/main` after each commit. End
every commit message with:

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

A. Hamlet meets the CW requirements: step 3 at <n> of 6, step 2 at 3 of 5 by the plan's
   checkboxes, steps 0 and 1 done, 4 to 8 not started.
B. Step 3, criterion 3.2, HM-REQ-011 with HM-REQ-010 and 012 as guards: the trace <named an edge
   at <d>, catching <n> wrong for 0 right | found no edge>; the change <kept | not kept | not
   built>. MET-INVENTED <47> -> <n> over 473; MET-CER-SURE <0.1116> -> <n>; coverage <374> -> <n>;
   3.2 <ticked, or what is missing>; 3.6 red on 17:37.
C. What this report adds, and whether it stands in the way of a criterion in B.
   Section 4 raises <N> items; <which, if any, is in the way of a criterion in B>.
```

```
UNIT:       445 - <complete|stopped> at task N of 3, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   <yes|no> - <which criteria flipped in PHASE_PLAN.md>
NUMBER:     MET-INVENTED <n> -> <n>; MET-CER-SURE <n> -> <n>; coverage <n> -> <n>; edge <d or none>
DRIFT:      step 3 <n>; step 2 2
```

**Section 3 opens with the trace's table:** right against wrong-or-added by worst mark distance,
with the edge marked. Then comes what the operator reads, for every recording whose text or
dimming changed, before and after. Mark the dim letters, and put the key beside each. Then list
every callsign the resolver gained or lost.

**Section 2, in one paragraph:** Does the operator now see fewer wrong letters printed as sure,
with no right letter turned dim? Give the evidence (V-13), and say whether the floors are as green
as at entry.

---

```
ARBITER-DECISION
STEP: 3
APPROACH: trace every sure letter's marks and inner gaps against a dit, a dah and an element gap at the unit in force, tabulate right against wrong, and only if an edge exists that no right letter crosses, print letters past it dim, kept under R78 with MET-INVENTED falling and coverage held
MOVE: work around
WHY: PHASE_PLAN.md step 3 line 3.2 asks for a change kept under R78 with MET-INVENTED falling, and 43 of HEAD's 47 are sure letters resting on marks that are neither dit nor dah at the unit in force, which the decoder never checks; step 2's open 2.4 and 2.5 cannot be moved by a unit while 443's standing ruling holds 17:37's floor, and the plan's step-3 dependency line routes to step 3 when step 2 blocks.
STATE: partial
DECIDED: author's, overrulable - (1) step 3 is worked because step 2's 2.5 is held by 443's DECIDED (3), which this arbiter may not overrule, and 2.4 is a closing rule no unit builds; (2) 444's section 4 question on 17:37 - re-bank at 38 or restore to 5 - is left to the owner, not ruled here: it is not one of the two stops, but answering it would overrule 443's DECIDED (3), and while it stands every step's floors-green line (2.5, 3.6, 4.7, 5.6, 6.6, 7.5, 8.6) stays red on 17:37, which is reported as a mismatch for the owner; (3) task 2 is built only on an edge the trace finds with no right letter past it, so 442's refusal is not repeated; (4) HM-REQ-014's dim precision is measured and reported, not a keep condition, because dimming wrong letters lowers it by construction; (5) 444's dropout rule and 443's section 4 are logged and not chased.
LICENCE: PHASE_PLAN.md step 3 line 3.2 and its dependency line, R78, R80, R81, section 6; R82 and R83 as recorded in work instruction 441; arbiter rulings 442 DECIDED (2) and 443 DECIDED (3); V-11; V-13; V-04; V-14; R72; HM-REQ-010, 011, 012, 014, 015; HM-DEC-155; HM-DEC-165; CLAUDE.md 0.0 and 0.2
ACCOMPLISHED: a letter the decoder read from marks that do not fit the sending speed stops being printed as sure, so fewer wrong letters reach the operator looking certain, and no right letter is dimmed to do it
ADVANCES: step 3 criterion 2
END-ARBITER-DECISION
```
