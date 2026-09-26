# Work instruction 448 - the decoder listens for keying from cold, and the opening is measured

**Authored by the arbiter against step 4's open criterion 4.5**, HM-REQ-102 and HM-REQ-103 on the
7.052 opening, measured before and after one change to how the tracker points its filter before
anything is confirmed. That change works toward HM-REQ-091 on the path where the requirement
actually fails. By the plan's checkboxes, steps 0 and 1 are done, step 2 stands at 3 of 5, step 3
at 3 of 6, step 4 at 2 of 7, and steps 5 to 8 are at 0.

**Why step 4, and not step 2, which the launcher named.** Nothing has changed since 446 and 447:
- **2.5** is red on 17:37's named floor (banked 46, reads 38). 443's DECIDED (3) keeps it red, and
  only the owner lifts that. The one repair route, 444's, was recorded `no`.
- **2.4** is a closing rule. It is met by a third step-2 unit in a row that keeps nothing, and
  authoring a unit to keep nothing is not work toward HM-REQ-010.
- Step 3 is closed the same way: 3.4's recording is not in the tree, 3.5 is a closing rule, and 3.6
  is red on 17:37.
- 5.1 was recorded `no` two units ago.

**Why 4.5, and why this change.** Section K's HM-REQ-102 and 103 are about acquisition. The one
place the tracker chooses a pitch *before* acquisition is the from-cold move at
`CwToneTracker.cs` about lines 995 to 1025: *"FROM COLD, POINT AT THE LOUDEST THING AND LET THE
DECODER LOOK."* That is a choice by level alone, which HM-REQ-091 forbids. It is also the move that
decides what the decoder hears while it acquires, which is exactly what 102 and 103 measure. **One
change there, measured on the opening before and after, moves 4.5 and works toward 091.** It is the
first time this phase touches the acquisition path.

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

The count today: **step 4 at 2 of 7** (4.1 and 4.2), step 2 at 3 of 5, step 3 at 3 of 6, step 5 at
0 of 6. At HEAD `c2cae86d`, on real recordings with inferred keys (unit 447's figures after its kept
tracker change):

| metric | count | value |
|---|---|---|
| MET-INVENTED | 40 over 473 | 0.0846 |
| MET-CER-SURE | 40 of 433 sure | 0.0924 |
| sure-and-right coverage (R82) | 393 over 473 | 0.8309 |
| MET-WBE | 47 over 113 | 0.4159 |
| files with MET-PITCH-ERR over 25 Hz | 9 of 69 | |

**Task 0 re-measures all of these, and its numbers win over this table.**
Floors, as 447 reported its exit: captures 51 of 51, adjudicated 13 of 13, named 10 of 13
(17:37 at 38, `032113` at 43, `032129` at 42).

```
PHASE GOAL: Hamlet meets the CW requirements.
UNIT GOAL:  Before the decoder has found anybody, it points its filter at the tone that is
            being keyed, not the loudest tone. The 7.052 opening is measured against
            HM-REQ-102 and 103 before and after, as text, so the owner can read what the
            decoder prints while it is still acquiring.
ADVANCES:   step 4 criterion 5 - the plan's line 4.5
```

`step 4 criterion 5` is the launcher's form. It means the plan's line `- [ ] 4.5`.

**Read `CW_REQUIREMENTS.md` first, then `CW_SPEC.md`**, including section K and §11 (MET-PITCH-ERR,
and MET-TACQ if it is defined there). Where they differ from this instruction, the documents win.
**Quote each id below from the document in section 1 of your report, and report any difference as
a mismatch.**

- **HM-REQ-102:** while acquiring, emit no sure character.
- **HM-REQ-103:** when a run-up precedes the message, do not lose the opening characters of the
  message to acquisition.
- **HM-REQ-091:** choose the tracked pitch by keying quality, never by level alone or by the
  configured pitch. This is the clause the change works toward.
- **HM-REQ-090:** acquire and track a keyed tone anywhere from 300 to 900 Hz.
- **HM-REQ-092:** MET-PITCH-ERR ≤ N, N **TBD, needs ruling**. Measure and report it. Do not judge it.
- **HM-REQ-010, 011 and 012, and MET-WBE:** the guards under R78.
- **V-11:** no change may redden an earlier capture to green a newer one.

## 2. Verify this instruction against the tree

Check each of these. **Where one is wrong, report it as a mismatch in section 1 and carry on. Do not
repair it.**

- **Unit 447 has no exit commit.** HEAD is `c2cae86d`, its task 3 record. Its `output.md` is
  written and uncommitted, and it says 4.1, 4.2 and 4.3 were ticked. **In the tree, 4.1 and 4.2 are
  ticked and 4.3 is not**, in both copies of `PHASE_PLAN.md`. Confirm this.
- The from-cold move is in `CwToneTracker.cs`, guarded by `double.IsNaN(_lastKeyedHz)` and
  `coarse.Strongest`. Confirm the lines, and name what `Strongest` ranks by.
- `WhatTheTrackerWeighedWhereItMoved` exists and logs every move of more than 15 Hz. 447 counted 48
  from-cold moves in 9 of the 11 files more than 25 Hz off. Confirm it can be run over the opening
  and the keyed 23.
- `TheTrackedPitchIsChosenByKeyingTests` names HM-REQ-091. Name the cases it holds, and say whether
  any of them starts cold with a louder unkeyed tone present.
- The 7.052 opening is `cw-2026-09-24-003901` through `-004234`, spliced as `WhatTheOpeningHeardTests`
  names them. **30.54 s is in `003919`.**
- **Expected failures at entry, and they are not yours to fix:**
  - The named floors: 17:37 (banked 46, reads 38), `032113` (45, reads 43), `032129` (64, reads 42).
  - `TheFiveToEightDecibelPlateauHolds` is the correctness phase's recorded red. It is in neither
    carry-forward line.
  - The app line loses up to 5 to the dispatcher loop. Each of those types is green alone.
- **Known and not yours. Report each once and edit none of them:**
  - `PHASE_OUTCOME.md`'s header still carries the old titles for steps 2, 3 and 8.
  - `CW_SPEC.md` §11 still defines MET-COVERAGE as sure over sent.
  - `PROJECT_STATUS.md` RULES_AT says HM-DEC-165, while `CLAUDE.md` §1 holds CPS-DEC-0183.
  - R75 names the sender at 625 Hz at 30.54 s. 447's instrument finds 599 to 601 Hz there, and 625
    only from 76 s on. Report the disagreement. Do not correct R75.

## 3. Rulings in force - do not re-argue

- **`PHASE_PLAN.md` R77 to R81 and §6, and R82 and R83** as work instruction 441 recorded them:
  - **R82:** MET-COVERAGE is sure-and-right over sent.
  - **R83:** a unit of this phase changes what the operator reads, or it is not authored.
- **R75, the tone tracker is open.** HM-DEC-095 and HM-DEC-127 are amended only that far, and their
  reasoning stands. **A unit that changes the tracker states which clause it works against, and
  why.** Here the thing changed is the from-cold fallback's comment and code, *"point at the loudest
  thing"*. Say whether that is a clause of HM-DEC-095 or HM-DEC-127, or neither. If neither, say
  what records it.
- **R76 is met.** 4.1 is ticked, so the tracker may be changed.
- **R78, the keep rule, for task 2's change.** Every item is a number:
  - MET-CER-SURE does not rise, real or synthetic;
  - MET-INVENTED does not rise, real or synthetic;
  - sure-and-right coverage does not fall, real or synthetic;
  - MET-WBE does not rise on the real set;
  - the adjudicated readings hold, or move onto their own adjudicated text;
  - V-11 holds per keyed recording on all four metrics;
  - **and the change does what it is for:** the count of from-cold moves made by level alone
    falls, the count of files more than 25 Hz off does not rise, and on the opening **no sure
    character is added while acquiring (HM-REQ-102) and no sure-right opening character is lost
    (HM-REQ-103).**

  **A capture row's character count falling is reported, not rejected.**
- **Unit 447's section 4, item 2, is ruled here, author's, overrulable:** a MET-PITCH-ERR movement
  inside the instrument's 0.5 Hz bin, or a change of sign whose size moves by less than one 5 Hz
  fine-bank step, is not a rise. The same reading applies to this unit's change.
- **Work instruction 447's tick rule for 4.3:** *"Tick 4.3 only if a test naming HM-REQ-091 was
  written and is green."* 447 met it and stated HM-REQ-091 unmet on the real set, but its exit
  commit never landed. **Task 0 applies that ruling.** It does not re-argue it.
- **Arbiter rulings carried, not re-argued:**
  - **443's DECIDED (3):** no floor is re-banked while 17:37's boundaries are worse than before G1.
    So `032113` and `032129` are not re-banked either. They are reported with their text.
  - **442's DECIDED (2):** a floors line is read as green at the exit of every commit from the
    unit's working task on.
- **HM-DEC-095's guard against a carrier**, 447's under-10 dB guard in `ReadSurvey`, and
  **HM-DEC-127's rule not to abandon a confirmed station for a candidate far below it** all stay as
  they are.
- **V-04 and V-14:** no fixture is admitted by lowering a gate. No separation limit, confirmation
  rule or plausibility bound is loosened to pass a fixture. **The three-second confirmation stays.**
  This change is about where the filter points before it, not how soon it confirms.
- **V-06:** no digital silence in a synthetic case. **V-13:** an inferred key is not proof by
  itself.
- **`CLAUDE.md` §12.5:** a fixture built from the same misunderstanding as the code proves nothing.
  The instrument measures the change. The tracker does not call it.
- **R72:** no word, dictionary or callsign prior.
- **`CLAUDE.md` §0.0:** never present a guess as a decode. **§0.2:** nothing that keys or
  transmits.
- **HM-DEC-155:** no suite. Named types only, one per invocation, each with its own `timeout`.
  - Captures get 600 s.
  - `WhatTheOpeningHeardTests` gets 900 s.
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

Scripts go in `.run-unit\unit448-<name>.sh` and run with `sh`. **Never compose a timestamp. Read the
clock.**

---

## 5. The tasks

### Task 0 - the entry, and 447's lost tick

- Add `## UNIT 448 - STEP 4` to `PHASE_OUTCOME.md`, from the decision block at the foot.
- `PHASE_STATUS.md` names 448, with `CURRENT_STEP: 4`. Patch-bump the version.
- Commit the root's uncommitted `PHASE_OUTCOME.md`, `PHASE_STATUS.md`, `PROJECT_STATUS.md`,
  `RUN_LEDGER.md` and 447's `output.md` with the entry. **These are the runner's and 447's writes.**
  Commit them as they are, without editing them.
- **4.3.** Run `TheTrackedPitchIsChosenByKeyingTests` alone. If it is green, **tick 4.3 in both
  copies of `PHASE_PLAN.md`**, under 447's tick rule. Its statement is 447's: HM-REQ-091 met on the
  synthetic cases and **not met on the real set**. If it is red, do not tick it, and say why.
- Run the entry round and print each result as a number:
  - build;
  - both carry-forward lines, **each with its wall time**;
  - the three floor tests, with the captures type's wall time;
  - the four metrics, per condition, with the key's kind beside each number;
  - the count of files more than 25 Hz off, from 447's printer.

### Task 1 - the trace: what the decoder does while it is acquiring

**First, define "acquiring" and state the definition.** Take it from `CW_SPEC.md` if the spec gives
one. If it does not, use: from the start of the recording, or of keyed signal after silence, to the
tracker's first keyed verdict, meaning HM-DEC-095's twice-confirmation. State the time of that
verdict in each file.

**On the 7.052 opening, and on each of the keyed 23**, print:
- the acquiring span, start and end in seconds;
- every from-cold move in that span: the time, from where to where, the level and the survey's
  keying score of the bin moved to, and the same for the bin with the best keying score;
- whether the move went to the best-keyed bin, or to a louder bin that was not;
- the instrument's pitch over the same span;
- **HM-REQ-102:** every sure character the decoder emitted inside the span, with its time, and
  whether it is right against the key or against `003919`'s own decoding for the opening;
- **HM-REQ-103:** the opening characters sent, against what was read sure and right. For the
  opening, the sent text is the stretch from 0 to 46.2 s as the decoder reads each file alone.
  Say that this is inferred.

Then count, over the 11 files 447 named plus the opening: the **from-cold moves made by level
alone**, meaning a move to a bin whose keying score is not the best available.

Commit the printer as a fact that asserts nothing, with its output.

**Drop candidate:** the keyed 23 outside the opening and the 11 named files. If the unit runs long,
trace only the opening and the 11, say what was skipped, and go on. **Task 2 is not dropped. It is
what R83 asks of this unit.**

### Task 2 - one change to the from-cold move, under R75 and R78

**Write the test first.** One test that names **HM-REQ-091 and HM-REQ-102**, on a synthetic case with
an exact key, 15 dB, shaped noise band (V-06):
- A weaker keyed tone, and a louder unkeyed carrier 75 to 100 Hz away. The configured pitch is on
  neither.
- It asserts that **before the first keyed verdict** the filter points at the keyed tone, not the
  carrier, and that no sure character is emitted before that verdict.
- **Watch it fail at HEAD.** If it passes at HEAD, the from-cold move is not choosing by level in
  this case. Say so, rebuild the case from task 1's trace so it reproduces what the real files show,
  and watch that fail.

**Then build one change to the from-cold move in its own commit.** The filter is pointed, before
confirmation, by **keying quality**: the survey's own measure of keyed structure, not level. Where
no bin shows keyed structure, the filter stays where it is. The method is yours within that. State
it.
- **This is a replacement, not a removal.** The tracker's own comment records that not moving until
  confirmation leaves the decoder deaf through the opening. HM-REQ-103 is the guard against that.
- Do not shorten the confirmation, and do not loosen HM-DEC-095's carrier guard, 447's under-10 dB
  guard or HM-DEC-127's abandon rule.
- The instrument is not called by the tracker.
- Leave G1, the marks' speed, the mark-shape dim edge, `RivalMargin`, `MarginLlr` and the speed
  bounds as they are.

**Judge it under §3's R78 list, every item a number**, with the instrument's MET-PITCH-ERR table
beside it before and after, and 447's 30 instrument cases rerun through the tracker. Print:
- task 1's trace again after the change, for the opening and the 11 files;
- every recording whose text changes, before and after, with its key;
- **the 7.052 opening's text before and after**, from 0 to 46.2 s, with the acquiring span, every
  sure character inside it, and the opening characters read sure and right, marked on the text.

**If it is kept:**
- Commit it on its own, with the test.
- Write the before and after to `docs/phase-requirements/metrics.md`, per condition, with the key's
  kind beside each number and the MET-PITCH-ERR column.
- Do not re-bank any floor. A row whose count moved is reported with its text.

**If it is not kept:**
- Leave it out of `src`.
- Commit its diff, with the test, as `.run-unit/unit448-coldmove-notkept.diff`.
- Record it in `metrics.md` with the table.
- Name the §3 item that refused it. **Do not build a second change to rescue it.**

**Tick 4.5 in both copies of `PHASE_PLAN.md`** when HM-REQ-102 and HM-REQ-103 have both been
measured on the opening at HEAD and after this change, printed as text before and after, with the
report stating for each whether it is met. **Kept or not, the measurement is what 4.5 asks.** If the
change was not built, 4.5 is not ticked.

### Task 3 - the exit round

- Build, both carry-forward lines, the three floor tests, the four metrics and every type touched.
  Print each as a number, with the wall times.
- **Report what changed in `src`, file by file.** Say that none of it keys or transmits, and that
  it was pushed.
- **Ticks:**
  - Confirm 4.3 and 4.5 as ticked or not, per tasks 0 and 2.
  - State whether HM-REQ-091 is now met on the real set, with the level-alone move count before and
    after.
  - **Do not tick 4.4, 4.6 or 4.7**, or anything in steps 2, 3 or 5. 4.7 stays red while the named
    floors do.
- **Make the exit commit and push it.** 447's did not land, and its tick was lost.
- Report DRIFT:
  - step 4: 0 if task 2's change was kept, 1 if not;
  - step 2: 2, unchanged;
  - step 3: 0, unchanged;
  - step 5: 1, unchanged.

---

## 6. Parked - do not touch, do not raise

- Everything in `PARKED.md`, including step 2's question on which files the transmit check covers.
  **This unit neither defines nor runs a transmit-file list.** It reports its `src` diff file by
  file, and that is all.
- 17:37's floor and 444's section 4 question. Both stay with the owner.
- 447's section 4 items 1 and 2, answered in §3 above. Item 3, building group A, is this unit.
- 446's two section 4 items, 445's section 4 question, and 444's half-unit dropout rule.
- The speed search, 5.1 to 5.6. The pitch proof state, 4.6.
- The correctness phase's 5.1, which is Tim's, and all of `PHASE_PLAN.md` §7 (Carried).
- The decision log, the traceability table, and the 43 tests that measure something else. These are
  step 8 (R80).

## 7. What not to do

- Do not remove the from-cold move without a replacement. The opening is what HM-REQ-103 protects.
- Do not shorten the confirmation, or loosen any guard §3 names (V-14).
- Do not wire the instrument into the decode path.
- Do not tune the change to one recording.
- Do not add a second change to rescue the first.
- Do not re-bank any floor.
- Do not revert G1, the marks' speed, the dim edge or 447's under-10 dB guard.
- Do not correct R75 or any ruling. Report the disagreement.
- Do not add a package. If one is needed, stop and report it.
- Do not halt on a question. Write one line in section 4 and go on.
- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**

## 8. Committing and pushing

Make one commit per task, and **one commit for the tracker change and its test on their own**. Each
message names the criterion it serves: `unit448 task N: <what> (4.5)`. Push to `origin/main` after
each commit. End every commit message with:

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

A. Hamlet meets the CW requirements: step 4 at <n> of 7, step 2 at 3 of 5, step 3 at 3 of 6,
   step 5 at 0 of 6 by the plan's checkboxes, steps 0 and 1 done, 6, 7 and 8 not started.
B. Step 4, criterion 4.5 (HM-REQ-102 and 103 on the 7.052 opening): acquiring <start> to <end> s;
   sure characters while acquiring <n> -> <n>; opening characters read sure and right <n> -> <n>
   of <n>; 4.5 <ticked | not>. 4.3 <ticked at task 0 | not>. The from-cold change <kept | not
   kept | not built>; level-alone moves <n> -> <n>; HM-REQ-091 on the real set <met | not met>;
   MET-CER-SURE <n> -> <n>; MET-INVENTED <n> -> <n>; coverage <n> -> <n>; 4.7 red on <floors>.
C. What this report adds, and whether it stands in the way of a criterion in B.
   Section 4 raises <N> items; <which, if any, is in the way of a criterion in B>.
```

```
UNIT:       448 - <complete|stopped> at task N of 3, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   <yes|no> - <which criteria flipped in PHASE_PLAN.md>
NUMBER:     sure while acquiring <n> -> <n>; opening sure-right <n> -> <n>; level-alone moves <n> -> <n>; MET-CER-SURE <n> -> <n>; MET-INVENTED <n> -> <n>
DRIFT:      step 4 <n>; step 2 2; step 3 0; step 5 1
```

**Section 3 opens with the 7.052 opening's text before and after**, 0 to 46.2 s, with the acquiring
span marked, every sure character inside it marked, and the opening characters read sure and right
marked. Beside it, the pitch the decoder was on and the pitch the instrument found. Then:
- the from-cold move table before and after, per file, with level and keying score of where each
  move went and of the best-keyed bin;
- the R78 table, every item a number, with the MET-PITCH-ERR column;
- every recording whose text changed, before and after, with its key.

**Section 2, in one paragraph:** When the operator tunes onto a call, does the decoder now listen
to the station that is keying from the first seconds, rather than the loudest tone, and does the
opening read more of what was sent without letters printed as sure before the decoder has found
the station? Say what reads worse, if anything. Give the evidence (V-13), and say what the
synthetic case does not prove (§12.5).

---

```
ARBITER-DECISION
STEP: 4
APPROACH: measure HM-REQ-102 and 103 on the 7.052 opening before and after one change that replaces the tracker's from-cold point-at-the-loudest move with a choice by keying quality, with an HM-REQ-091 and 102 test on a cold start beside a louder unkeyed carrier watched failing first, kept under R78 with the instrument table beside it
MOVE: work around
WHY: PHASE_PLAN.md step 4 line 4.5 asks that HM-REQ-102 and HM-REQ-103 be measured on the 7.052 opening as text before and after, and the only pitch choice the tracker makes while acquiring is the from-cold move to the loudest bin, which is also a choice by level alone under HM-REQ-091 in line 4.3. Step 2's 2.5 is held red on 17:37 by 443's DECIDED (3), which only the owner lifts, and 2.4 is a closing rule no honest unit is aimed at; step 3 is blocked the same way; 5.1 was recorded no.
STATE: partial
DECIDED: author's, overrulable - (1) step 4 is worked instead of the launcher's step 2, for the reasons 446's DECIDED (1) gave, which still stand; (2) 4.3 is ticked at task 0 by applying work instruction 447's own tick rule, a green test naming HM-REQ-091, which 447 met and stated HM-REQ-091 unmet on the real set, and whose exit commit never landed - this applies an earlier ruling and overrules none; (3) 447's section 4 item 2 is ruled: a MET-PITCH-ERR move inside the 0.5 Hz bin, or a sign change moving by less than one 5 Hz fine step, is not a rise; (4) "acquiring" is taken from CW_SPEC.md, or else as the span up to HM-DEC-095's first twice-confirmed keyed verdict, and the opening's sent text is inferred from each file decoded alone; (5) 4.5 is ticked on the measurement before and after the change whether or not it is kept, since 4.5 is a measurement; (6) 443's DECIDED (3) is read as covering 032113 and 032129 too, so no floor is re-banked and 4.7 stays red.
LICENCE: PHASE_PLAN.md step 4 lines 4.3 and 4.5 and section 5's independence line; R75 and R76 as carried from the correctness phase's plan; R78, R80, R81, section 6; R82 and R83 as recorded in work instruction 441; work instruction 447's tick rule for 4.3; arbiter rulings 442 DECIDED (2) and 443 DECIDED (3); HM-DEC-095; HM-DEC-127; V-04; V-06; V-11; V-13; V-14; R72; HM-REQ-010, 011, 012, 090, 091, 092, 102, 103; HM-DEC-155; HM-DEC-165; CLAUDE.md 0.0, 0.2 and 12.5
ACCOMPLISHED: when the operator tunes onto a call, the decoder listens to the station that is keying from the first seconds instead of the loudest tone in the passband, and the owner can read, as text, what the opening of the 7.052 call prints while the decoder is still finding the station and what it prints after
ADVANCES: step 4 criterion 5
END-ARBITER-DECISION
```
