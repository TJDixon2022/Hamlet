# Work instruction 447 - a pitch instrument, then the tracker hears the sender's own note

**Authored by the arbiter against step 4's open criterion 4.1**, with one tracker change under
R75 so that the unit changes what the operator reads (R83). By the plan's checkboxes, steps 0 and 1
are done, step 2 stands at 3 of 5, step 3 at 3 of 6, and steps 4 to 8 are at 0.

**Why step 4, and not step 2, which the launcher named.** Step 2's two open lines have no route a
unit can take this pass:
- **2.5** is red for one reason: 17:37's named floor, banked 46 and reading 38. 443's DECIDED (3)
  says no floor is re-banked while 17:37's boundaries are worse than before G1. That is an arbiter
  ruling, and only the owner lifts it. The one repair route, restoring the boundaries, was recorded
  `no` by 444.
- **2.4** is a closing rule. It is met by a third step-2 unit in a row that keeps nothing. Authoring
  a unit to keep nothing is not work toward HM-REQ-010.

Step 3's open lines are closed the same way:
- 3.4 needs the 7.052 traffic-net recording, which is not in the tree.
- 3.5 is a closing rule.
- 3.6 is red on 17:37.

**Why step 4, and not step 5 again.** 5.1 was tried last unit and recorded `no`. **Step 4 has never
been worked.** It sits under the other steps:
- R75 records that on the 7.052 opening at 30.54 s, **the decoder's own envelope ranks 525 Hz
  highest while the sender is at 625 Hz.**
- R76 holds `CwToneTracker` shut until a pitch instrument is built and proved (4.1).

Until 4.1 is met, no unit may change the tracker, so no unit can fix the note the decoder listens
to. **4.1 is the key to every pitch fix, and the tracker change after it is the first chance in this
phase to change the text at the opening.**

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

The count today: **step 2 at 3 of 5, step 3 at 3 of 6, step 4 at 0 of 7, step 5 at 0 of 6.** At HEAD
`ae2d6b00`, on real recordings with inferred keys (unit 446's exit, which is its entry, because
`src` did not change):

| metric | count | value |
|---|---|---|
| MET-INVENTED | 45 over 473 | 0.0951 |
| MET-CER-SURE | 45 of 419 sure | 0.1074 |
| sure-and-right coverage (R82) | 374 over 473 | 0.7907 |
| MET-WBE | 52 over 113 | 0.4602 |

**Task 0 re-measures all of these, and its numbers win over this table.**
Floors: captures 51 of 51, adjudicated 13 of 13, named 12 of 13 (17:37 banked 46, reads 38).

```
PHASE GOAL: Hamlet meets the CW requirements.
UNIT GOAL:  Hamlet gets an instrument that measures the note a station sends to better than
            25 Hz, independent of the tracker. It is proved on tones whose pitch is known.
            Then the tracker is changed once, under the keep rule, so that it listens where
            the instrument says the sender is.
ADVANCES:   step 4 criterion 1 - the plan's line 4.1
```

`step 4 criterion 1` is the launcher's form. It means the plan's line `- [ ] 4.1`.

**Read `CW_REQUIREMENTS.md` first, then `CW_SPEC.md`, including §11 for MET-PITCH-ERR.** Where they
differ from this instruction, the documents win. **Quote each id below from the document in section
1 of your report, and report any difference as a mismatch.**

- **HM-REQ-092:** report the pitch being demodulated within N Hz of true, measured as
  MET-PITCH-ERR ≤ N.
  - N is **TBD, needs ruling** (5 Hz recommended). **Measure it and report it. Do not judge it.**
  - This is what the instrument exists to measure.
- **HM-REQ-090:** acquire and track a keyed tone anywhere from 300 to 900 Hz. The instrument's cases
  span that range.
- **HM-REQ-091:** choose the tracked pitch by keying quality, never by level alone or by the
  configured pitch. This is the clause the tracker change works toward. 4.3 is not ticked here
  unless its test is written and green.
- **HM-REQ-102 and HM-REQ-103:** no sure character while acquiring, and no opening characters lost
  to acquisition. Print the 7.052 opening before and after the change (the material for 4.5; 4.5 is
  not ticked here).
- **HM-REQ-010, 011 and 012, and MET-WBE:** the guards under R78.
- **V-11:** no change may redden an earlier capture to green a newer one.

## 2. Verify this instruction against the tree

Check each of these. **Where one is wrong, report it as a mismatch in section 1 and carry on. Do not
repair it.**

- `src\Hamlet.RadioEngine\Cw\` holds `CwToneTracker.cs`, `CwToneSurvey.cs` and `CwPitchChoice.cs`.
  - Name the bin width the tracker and the survey work on. The plan says 25 Hz.
  - Name whatever else they call to find a pitch: FFT, Goertzel, window functions.
- The 7.052 opening is `cw-2026-09-24-003901` through `-004234`, as `WhatTheOpeningHeardTests`
  names them. Confirm which file holds 30.54 s, where R75 measured 525 Hz ranked above the sender's
  625 Hz.
- `cw-2026-09-24-135641` and `-152135`, the two known 75 Hz cases, are **not in the tree**. Confirm
  that. **They are not yours to add**, so measure 4.2 on what the tree has.
- The synthetic generator can make a keyed tone at a stated pitch, with a shaped noise band (V-06).
  Name it.
- **Expected failures at entry, and they are not yours to fix:**
  - 17:37's named floor is red (banked 46, reads 38).
  - `TheFiveToEightDecibelPlateauHolds` is the correctness phase's recorded red. It is in neither
    carry-forward line.
  - The app line loses up to 5 to the dispatcher loop. Each of those types is green alone.
- **Known and not yours. Report each once and edit none of them:**
  - `PHASE_OUTCOME.md`'s header still carries the old titles for steps 2, 3 and 8.
  - `CW_SPEC.md` §11 still defines MET-COVERAGE as sure over sent.
  - `PROJECT_STATUS.md` RULES_AT says HM-DEC-165, while `CLAUDE.md` §1 holds CPS-DEC-0183.

## 3. Rulings in force - do not re-argue

- **`PHASE_PLAN.md` R77 to R81 and §6, and R82 and R83** as work instruction 441 recorded them:
  - **R82:** MET-COVERAGE is sure-and-right over sent.
  - **R83:** a unit of this phase changes what the operator reads, or it is not authored.
- **R75, the tone tracker is open.** HM-DEC-095 and HM-DEC-127 are amended only that far. Their
  reasoning stands. **A unit that changes the tracker states which clause it works against, and
  why.**
- **R76, the instrument first.** *"4.1 is built and proved against tones known by construction
  first; no unit may change `CwToneTracker` until 4.1 is met."*
- **R78, the keep rule, for task 3's change.** Every item is a number:
  - MET-CER-SURE does not rise, real or synthetic;
  - MET-INVENTED does not rise, real or synthetic;
  - sure-and-right coverage does not fall, real or synthetic;
  - MET-WBE does not rise on the real set;
  - the adjudicated readings hold, or move onto their own adjudicated text;
  - V-11 holds per keyed recording on all four metrics;
  - **and the change does what it is for:** MET-PITCH-ERR, measured by the instrument, falls on the
    cases it targets and rises on none.

  **A capture row's character count falling is reported, not rejected.**
- **Arbiter rulings carried, not re-argued:**
  - **443's DECIDED (3):** no floor is re-banked while 17:37's boundaries are worse than before G1.
  - **442's DECIDED (2):** a floors line is read as green at the exit of every commit from the
    unit's working task on.
- **HM-DEC-095's guard against a carrier**, and **HM-DEC-127's rule not to abandon a confirmed
  station for a candidate far below it**, stay in force except for the one clause task 3 names.
- **V-04 and V-14:** no fixture is admitted by lowering a gate. No separation limit, confirmation
  rule or plausibility bound is loosened to pass a fixture.
- **V-06:** no digital silence in a synthetic case. **V-13:** an inferred key is not proof by
  itself.
- **`CLAUDE.md` §12.5:** a fixture built from the same misunderstanding as the code proves nothing.
  This is why the instrument shares no code with the tracker.
- **R72:** no word, dictionary or callsign prior.
- **`CLAUDE.md` §0.0:** never present a guess as a decode. **§0.2:** nothing that keys or
  transmits.
- **HM-DEC-155:** no suite. Named types only, one per invocation, each with its own `timeout`.
  - Captures get 600 s.
  - `WhatTheOpeningHeardTests` gets 900 s.
  - **Never background and poll.**
- **HM-DEC-165, FACT-004.** **A package is needed: `MOVE: stop` (§6).** Build any transform from the
  BCL or by hand.

## 4. Status cadence

At the start of each task, run `sh tools/status.sh EXECUTING "<n> of 4" code none "<one line>"`. At
the end, run `COMPLETED`. `SESSION.lock` belongs to the runner, so do not take or release it. Write
nothing to `RUN_LEDGER.md`, and touch nothing under `tools\arbiter\`.

What breaks in this shell:
- Apostrophes in quoted heredocs break, and doubled backslashes collapse.
- `;` is refused, `rm` is refused, and Python cannot run here.
- A multi-line commit uses `-m` more than once.

Scripts go in `.run-unit\unit447-<name>.sh` and run with `sh`. **Never compose a timestamp. Read the
clock.**

---

## 5. The tasks

### Task 0 - the entry

- Add `## UNIT 447 - STEP 4` to `PHASE_OUTCOME.md`, from the decision block at the foot.
- `PHASE_STATUS.md` names 447, with `CURRENT_STEP: 4`. Patch-bump the version.
- Run the entry round and print each result as a number:
  - build;
  - both carry-forward lines, **each with its wall time**;
  - the three floor tests, with the captures type's wall time;
  - the four metrics, per condition, with the key's kind beside each number.
- Commit the root's uncommitted `PHASE_OUTCOME.md`, `PHASE_STATUS.md` and `RUN_LEDGER.md` with the
  entry. **These are the runner's writes.** Commit them as they are, without editing them.

### Task 1 - the instrument (4.1)

**Build a pitch estimator that is finer than 25 Hz**, in a new type under
`src\Hamlet.RadioEngine\Cw\`, or under the tests if it is used only for measuring. Say which, and
why.

**Independence (R76, §12.5).** The estimator shares no line of code with any of these:
- `CwToneTracker`;
- `CwToneSurvey`;
- `CwPitchChoice`;
- anything those three call to find a pitch.

It does not read their state. **Show this** by listing every type the estimator references.

**The method is yours.** A long FFT with interpolation between bins, a zoom transform, or a phase-
or zero-crossing estimate on keyed marks are all acceptable. **State the method and its bin in
hertz.** It estimates only where keying is present. Say how it decides that, and that the decision
does not come from the tracker.

**The cases, known by construction.** Build them with the generator at 15 dB with a shaped noise
band (V-06), each with an exact key:
- **Pitches:** 300, 400, 523, 600, 625, 640, 675, 750 and 900 Hz. That is off-grid values, both ends
  of HM-REQ-090, and the 615/675 and 625/525 pairs the decision log names.
- **Speeds:** 12, 20 and 30 WPM.
- **At 5 dB:** 625 Hz at 20 WPM.
- **Two stations:** 625 Hz keyed, with a louder unkeyed carrier at 525 Hz. Here the instrument
  reports the keyed tone.
- **Slow drift:** a keyed tone moving 20 Hz over 30 s.

**Write one test that names HM-REQ-092 and 4.1.** It asserts that every case lands within one of the
instrument's own bins of the truth. **Watch it fail first** against a deliberately wrong truth, then
pass.

**Table it**, per case:
- the truth;
- the estimate;
- the error in Hz;
- the bin;
- whether it is inside one bin.

Give the count of cases the instrument admits at all, and **its cost per hop in µs**: the median of
three runs over one fixed recording.

**Tick 4.1 in both copies of `PHASE_PLAN.md` in this task's commit, and only if** every case is
within one bin, the table and the cost are printed, and the reference list shows no shared code. If
any case misses, do not tick it. **Report which cases missed, and do not build task 3** (R76). Go to
task 2, then task 4.

### Task 2 - MET-PITCH-ERR on what the tree holds (4.2)

Run the instrument over **every capture in the tree**: the 51 capture rows, the keyed recordings and
the 7.052 opening files. For each, print:
- its file;
- the pitch the decoder demodulated at, from the tracker's own output, sampled where the instrument
  found keying;
- the pitch the instrument found;
- MET-PITCH-ERR in Hz;
- **the flag `>25`** where the decoder was more than 25 Hz off.

Name every `>25` case. State plainly that `135641` and `152135` are not in the tree, so they are
unmeasured.

**Then print the 7.052 opening at HEAD.** Give the text, the pitch the decoder was on, the pitch the
instrument found, and when the first sure character came (HM-REQ-102, 103).

- Commit the printer, which is a fact that asserts nothing, and the table.
- **Tick 4.2 only if** every capture in the tree was measured and every `>25` case is named. The
  absent two are named as absent.

**Drop candidate:** the capture rows outside the keyed recordings and the opening. If the unit runs
long, measure only the keyed 23 and the opening files, say what was skipped, and do not tick 4.2.
**Task 3 is not dropped. It is what R83 asks of this unit.**

### Task 3 - one tracker change, under R75 and R78

Only if 4.1 was ticked in task 1.

Take the largest group of `>25` cases from task 2. **The opening at 30.54 s is expected among
them.** Say what the tracker weighed there to pick the wrong note: level, the survey's score, the
confirmation rule, or the abandon rule.

**Build one change to `CwToneTracker` in its own commit.** It moves the tracked pitch toward keying
quality under HM-REQ-091.
- **Name the clause of HM-DEC-095 or HM-DEC-127 it works against, and why** (R75).
- **The instrument is not called by the tracker.** The instrument measures the change. It is not
  part of it, because §12.5 would make the measurement circular.
- Leave G1, the marks' speed, the mark-shape dim edge, `RivalMargin`, `MarginLlr` and the speed
  bounds as they are.

**Judge it under §3's R78 list, every item a number**, with the instrument's MET-PITCH-ERR table
beside it, before and after, and the task 1 cases rerun. Print:
- every recording whose text changes, before and after, with its key;
- the 7.052 opening's text before and after.

**If it is kept:**
- Commit it on its own.
- Write the before and after to `docs/phase-requirements/metrics.md`, per condition, with the key's
  kind beside each number and the MET-PITCH-ERR column.
- **Do not re-bank any floor.** A row whose count moved is reported with its text.

**If it is not kept:**
- Leave it out of `src`.
- Commit its diff as `.run-unit/unit447-tracker-notkept.diff`.
- Record it in `metrics.md` with the table.
- Name the §3 item that refused it. **Do not build a second change to rescue it.**

If task 2 found no `>25` case in the tree, do not build a change. Say so. R83 is then unmet by this
unit, and that is a finding.

### Task 4 - the exit round

- Build, both carry-forward lines, the three floor tests, the four metrics and every type touched.
  Print each as a number, with the wall times.
- **Report what changed in `src`, file by file.** Say that none of it keys or transmits, and that
  it was pushed.
- **Ticks:**
  - Confirm 4.1 and 4.2 as ticked or not, per tasks 1 and 2.
  - **Tick 4.3** only if a test naming HM-REQ-091 was written and is green.
  - **Do not tick 4.4 to 4.7**, or anything in steps 2, 3 or 5. 4.7 stays red while 17:37 does.
- Report DRIFT:
  - step 4: 0 if task 3's change was kept, 1 if not;
  - step 2: 2, unchanged;
  - step 3: 0, unchanged;
  - step 5: 1, unchanged.

---

## 6. Parked - do not touch, do not raise

- Everything in `PARKED.md`, including step 2's question on which files the transmit check covers.
  **This unit neither defines nor runs a transmit-file list.** It reports its `src` diff file by
  file, and that is all.
- 17:37's floor and 444's section 4 question. Both stay with the owner.
- 446's two section 4 items:
  - how 5.1 is split, answered in DECIDED (5) below for when 5.1 is next worked;
  - putting `135641` in the tree, which is the owner's recording to supply.
- 445's section 4 question, and 444's half-unit dropout rule.
- The speed search, 5.1 to 5.6.
- The correctness phase's 5.1, which is Tim's, and all of `PHASE_PLAN.md` §7 (Carried).
- The decision log, the traceability table, and the 43 tests that measure something else. These are
  step 8 (R80).

## 7. What not to do

- Do not touch `CwToneTracker` before 4.1 is ticked (R76).
- Do not let the instrument share code with the tracker, the survey or `CwPitchChoice`.
- Do not wire the instrument into the decode path.
- Do not loosen HM-DEC-095's carrier guard or any confirmation rule beyond the one clause task 3
  names (V-14).
- Do not tune the change to one recording.
- Do not add a second change to rescue the first.
- Do not re-bank any floor.
- Do not revert G1, the marks' speed or the dim edge.
- Do not add a package. If one is needed, stop and report it.
- Do not halt on a question. Write one line in section 4 and go on.
- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**

## 8. Committing and pushing

Make one commit per task, and **one commit for the tracker change on its own**. Each message names
the criterion it serves: `unit447 task N: <what> (4.1)`. Push to `origin/main` after each commit.
End every commit message with:

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
B. Step 4, criterion 4.1 (HM-REQ-092 measured, N TBD): the instrument's bin <n> Hz, <n> of <n>
   cases within one bin, cost <n> us per hop, 4.1 <ticked | not, and which cases missed>;
   4.2 <n> captures measured, <n> more than 25 Hz off; the tracker change <kept | not kept |
   not built>, against <clause>; MET-CER-SURE <n> -> <n>; MET-INVENTED <n> -> <n>; coverage
   <n> -> <n>; 4.7 red on 17:37.
C. What this report adds, and whether it stands in the way of a criterion in B.
   Section 4 raises <N> items; <which, if any, is in the way of a criterion in B>.
```

```
UNIT:       447 - <complete|stopped> at task N of 4, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   <yes|no> - <which criteria flipped in PHASE_PLAN.md>
NUMBER:     instrument bin <n> Hz, worst case <n> Hz; >25 Hz cases <n>; MET-CER-SURE <n> -> <n>; MET-INVENTED <n> -> <n>
DRIFT:      step 4 <n>; step 2 2; step 3 0; step 5 1
```

**Section 3 opens with the instrument's table,** one row per case with the truth, the estimate, the
error and the bin, and its cost per hop. Then:
- the MET-PITCH-ERR table over the tree, with the `>25` cases first;
- the 7.052 opening's text before and after, beside the pitch the decoder was on and the pitch the
  instrument found;
- every recording whose text changed, before and after, with its key.

**Section 2, in one paragraph:** Does the decoder now listen at the note the sender is on, and does
the operator read more of the opening? Say what reads worse, if anything. Give the evidence (V-13),
and say what the synthetic cases do not prove (§12.5).

---

```
ARBITER-DECISION
STEP: 4
APPROACH: build a pitch instrument finer than 25 Hz sharing no code with the tracker, prove it on tones known by construction, measure MET-PITCH-ERR on every capture in the tree, then one CwToneTracker change toward keying quality judged under R78 with the instrument table beside it
MOVE: work around
WHY: PHASE_PLAN.md step 4 line 4.1 asks for a pitch instrument finer than 25 Hz, proved within one bin on tones known by construction and sharing no code with CwToneTracker or CwToneSurvey, and R76 keeps the tracker shut until it is met. Step 2's 2.5 is held red on 17:37 by 443's DECIDED (3), which only the owner lifts, and 2.4 is a closing rule no honest unit is aimed at; step 3 is blocked the same way; 5.1 was just recorded no.
STATE: not started
DECIDED: author's, overrulable - (1) step 4 is worked instead of the launcher's step 2, for the same reasons 446's DECIDED (1) gave, which still stand, and ahead of step 5 because 5.1 was just refused and step 4 has never been worked; R83 is met by task 3's tracker change, which follows the instrument in the same unit; (2) R76's "until 4.1 is met" is read as 4.1 ticked in task 1's own commit, with the tracker untouched before it and task 3 not built if 4.1 misses; (3) R76's independence is read as no code shared with CwToneTracker, CwToneSurvey, CwPitchChoice or anything they call, and the instrument never enters the decode path; (4) HM-REQ-092's N is TBD, so MET-PITCH-ERR is measured and reported, never judged; (5) 446's section 4 is answered for when 5.1 is next worked: 5.1 may be met by two kept changes, one per end, ticked after the second, and 135641 is the owner's recording to supply and never halts a unit; (6) 17:37 stays under 443's DECIDED (3), so 4.7 stays red.
LICENCE: PHASE_PLAN.md step 4 lines 4.1 to 4.4 and section 5's independence line; R75 and R76 as carried from the correctness phase's plan; R78, R80, R81, section 6; R82 and R83 as recorded in work instruction 441; arbiter rulings 442 DECIDED (2) and 443 DECIDED (3); HM-DEC-095; HM-DEC-127; V-04; V-06; V-11; V-13; V-14; R72; HM-REQ-010, 011, 012, 090, 091, 092, 102, 103; HM-DEC-155; HM-DEC-165; CLAUDE.md 0.0, 0.2 and 12.5
ACCOMPLISHED: Hamlet can measure the note a station is sending to better than 25 Hz, independently of the part that chooses it, and uses that measurement to make the tracker listen where the sender is, so the opening of a call the tracker used to hear at the wrong pitch reads more of what was sent
ADVANCES: step 4 criterion 1
END-ARBITER-DECISION
```
