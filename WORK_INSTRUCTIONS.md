# Work instruction 446 - the speed search reaches 5 and 45 words a minute

**Authored by the arbiter against step 5's open criterion 5.1.** By the plan's checkboxes, step 2
stands at 3 of 5, step 3 at 3 of 6, steps 0 and 1 are done, and steps 4 to 8 are not started.

**Why step 5, and not step 2, which the launcher named.** Every route to step 2's two open lines
is closed this pass:
- **2.5** is red for one reason, 17:37's named floor (banked 46, reads 38). 443's DECIDED (3) says
  no floor is re-banked while 17:37's boundaries are worse than before G1. It is an arbiter ruling,
  and only the owner lifts it. The one route that did not need a re-bank was restoring the two
  boundaries. 444 measured it: it needs a duration threshold between 320 and 330 ms that fits one
  sender. It is recorded `no` against 2.5.
- **2.4** is a closing rule. It is met by a third consecutive step-2 unit that keeps no change.
  Authoring a unit so that it keeps nothing is not work toward HM-REQ-010. This arbiter does not
  author one.

Step 3's open lines are closed too:
- 3.4 needs the 7.052 traffic-net recording, which is not in the tree (443).
- 3.5 is a closing rule.
- 3.6 is red on 17:37.

The plan's §5 lets any of steps 2 to 7 go: *"six independent places to route and the loop need
never stall for want of work."* Between steps 4 and 5:
- **Step 4 opens on 4.1**, an instrument. That changes nothing the operator reads, and R83 says a
  unit of this phase changes what the operator reads or it is not authored.
- **Step 5 opens on 5.1**, the speed search. That does change what the operator reads.
  `cw-2026-09-24-135641` was sent at about 44 WPM and **reads nothing at HEAD**, because the search
  stops at 40. Any station sending 5 to 7 WPM, the slow end of HM-REQ-030, is outside the search
  as well.

**The owner's standing order, 2026-09-25:** *"Write something that materially advances CW in the
most significant way we can handle."* A station the decoder cannot hear at all is the largest
failure an operator meets.

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

The count today: **step 2 at 3 of 5, step 3 at 3 of 6, step 5 at 0 of 6.** At HEAD `83d3897c`, on
real recordings with inferred keys (unit 445's exit):

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
UNIT GOAL:  The decoder finds a station sending anywhere from 5 to 45 words a minute, where
            today it searches only 8 to 40, so a fast or slow sender is read instead of
            ignored, and nothing it reads today gets worse.
ADVANCES:   step 5 criterion 1 - the plan's line 5.1
```

`step 5 criterion 1` is the launcher's form, and it means the plan's line `- [ ] 5.1`.

**Read `CW_REQUIREMENTS.md` first, then `CW_SPEC.md`**, including §11 for the metrics'
definitions. Where they differ from this instruction, they win. Here are the requirement ids this
unit works toward. **Quote each from the document in section 1, and report any difference as a
mismatch.**

- **HM-REQ-030:** every must-tier requirement at any speed from 5 to 45 WPM inclusive, both ends
  hard. Above 30.6 WPM is synthetic-only until a capture exists. **This is the target.**
  - 5.1 asks that the search reach both ends.
  - Meeting every must-tier requirement at every speed is more than one unit's work. Measure it
    at 5 and 45 and report it. Do not claim it.
- **HM-REQ-033:** speed acquired without a prior speed. The 5 and 45 cases start cold, and the
  report says whether they acquired.
- **HM-REQ-031:** speed within 10 % of true after acquisition. Measure it at 5 and 45 and report it
  (5.3 is not ticked here).
- **HM-REQ-010, 011, 012, and MET-WBE:** the guards, under R78.
- **V-11** from `CW_SPEC.md`: no change may redden an earlier capture to green a newer one.

## 2. Verify this instruction against the tree

Check each of these. **Where one is wrong, report it as a mismatch in section 1 and carry on. Do not
repair it.**

- The plan says the speed search runs **8 to 40 WPM at HEAD**. Find every place that bounds it:
  - constants;
  - the speed grid or its candidate list;
  - the unit estimator's clamps;
  - any window, history length or timeout sized from a speed.

  Name each one with its file and line. If the bounds are not 8 and 40, report what they are.
- `cw-2026-09-24-135641` is in the tree and reads nothing at HEAD. Give its length, and how the
  captures test reads it: whole, or a clip.
- The synthetic generator can make a send at a stated WPM with an exact key. Name it. If it cannot
  make 5 or 45, say why.
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
- **R78, the keep rule, for this unit's change:**
  - MET-CER-SURE does not rise, real or synthetic;
  - MET-INVENTED does not rise, real or synthetic;
  - sure-and-right coverage does not fall, real or synthetic;
  - MET-WBE does not rise on the real set;
  - the adjudicated readings hold, or move onto their own adjudicated text;
  - V-11 holds per keyed recording on all four metrics;
  - **and the change does what 5.1 asks:** the 5 WPM and 45 WPM synthetic cases each emit sure
    characters, acquired cold, where HEAD emits none or reads them at the wrong speed.

  **A capture row's character count falling is reported, not rejected.**
- **Decode time is reported before and after, as 5.1 says, and it is not a keep condition.** But
  §6 is binding: no CW test costing more than 300 s goes on a carry-forward line. If the change
  pushes a carry-forward type past that, report it as a finding.
- **Arbiter rulings carried, not re-argued:**
  - **443's DECIDED (3):** no floor is re-banked while 17:37's boundaries are worse than before G1.
  - **442's DECIDED (2):** a floors line is read as green at the exit of every commit from the
    unit's working task on.
- **V-13:** an inferred key is not proof by itself. **V-04 and V-14:** no fixture is admitted by
  lowering a gate, and no plausibility bound is loosened to pass one.
  - Widening the speed search is what HM-REQ-030 requires. It is not the loosening of a bound.
  - A separation limit or confirmation rule that is not a speed bound stays as it is.
- **`CLAUDE.md` §12.5:** a fixture built from the same misunderstanding as the code proves nothing.
  The 5 and 45 WPM cases are built by the generator at PARIS timing. **The report states what they
  do not prove**, including: no real sender, no real channel, no Farnsworth.
- **R72:** no word, dictionary or callsign prior, in any form.
- **`CLAUDE.md` §0.0:** never present a guess as a decode. **§0.2:** nothing that keys or transmits.
  The speed search is receive-only. If any bound you find is shared with the keyer or transmit
  path, **do not change it. Stop at that bound and report it.**
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

Scripts go in `.run-unit\unit446-<name>.sh` and run with `sh`. **Never compose a timestamp. Read
the clock.**

---

## 5. The tasks

### Task 0 - the entry

- `PHASE_OUTCOME.md` gets `## UNIT 446 - STEP 5` from the decision block at the foot.
- `PHASE_STATUS.md` names 446 and `CURRENT_STEP: 5`. Patch-bump.
- Entry round, each printed as a number:
  - build;
  - both carry-forward lines, **each with its wall time**;
  - the three floor tests, with the captures type's wall time;
  - the four metrics, per condition, with the key's kind beside each number.
- Commit the root's uncommitted `PHASE_OUTCOME.md`, `PHASE_STATUS.md` and `RUN_LEDGER.md` with the
  entry. **These are the runner's writes.** Commit them as they are and do not edit them.

### Task 1 - the speed trace, and both ends watched failing first

**A fact that asserts nothing, first.** It prints every bound the speed search has, with its
file, line and value:
- the grid or candidate list;
- the estimator's clamps;
- every window sized from a speed.

For each one, give what 5 WPM and 45 WPM need of it, as numbers:
- the unit is 240 ms at 5 WPM and 26.7 ms at 45 WPM;
- the longest dah is 720 ms at 5 WPM;
- the word gap is 1680 ms at 5 WPM;
- the shortest element is 26.7 ms at 45 WPM, which is how many envelope hops?

Say which bounds would stop each end, and which would not.

**Then the cases.** Build synthetic sends with the generator at **5 WPM and 45 WPM**, plus
**8 WPM and 40 WPM** as controls:
- PARIS timing, a CQ-shaped text with an exact key;
- 15 dB with the generator's usual noise, and never digital silence (V-06);
- no prior speed given to the decoder.

Write one test per end that names **HM-REQ-030**. It asserts sure characters emitted, and speed
within 10 % of true once acquired. **Watch both fail at HEAD. Watch the 8 and 40 controls pass.**
If a control fails at HEAD, report it, and treat that end's failure as not caused by the bound.

Print, per case at HEAD:
- the text;
- the speed reported, and when;
- MET-CER-SURE, sure-and-right coverage and MET-INVENTED, with the exact key.

Also print `cw-2026-09-24-135641` at HEAD: its text, or `nothing read`, and the speed and pitch in
force. **This print is the drop candidate.** If the unit runs long, skip it, and say 5.2 is
unmeasured.

Commit the fact, the tests and the printout.

### Task 2 - one change: the search reaches 5 and 45 (5.1)

Build **one** change, in its own commit: every bound task 1 named as stopping an end is moved so
that the search covers 5 to 45 WPM. Include any window that is sized from a speed and must grow to
hold a 5 WPM element or gap.
- **Move nothing else.**
- Leave G1, the marks' speed, the mark-shape dim edge, `RivalMargin`, `MarginLlr` and the tone
  tracker as they are.
- If one end needs more than moving bounds, for example a new detector, then build the other end
  alone. **Say which end is left, and why.**

**Judge it under §3's list, every item as a number:**
- the four metrics, real and synthetic, per condition with the key's kind;
- the adjudicated readings;
- V-11 per recording;
- the 5, 8, 40 and 45 cases' text, speed and metrics, before and after;
- **decode time, before and after:**
  - each carry-forward line's wall time;
  - the captures type's wall time;
  - and one fixed recording's decode in ms, per minute of audio, run three times with the median
    given.

Print every recording whose text changes, before and after, with its key. If `135641` was printed
in task 1, print it after too, beside `nothing read`.

**If it is kept:**
- Commit the change on its own.
- Write the before and after to `docs/phase-requirements/metrics.md`, per condition, with the key's
  kind beside each number, and with the decode times.
- Every later commit ends with the three floor tests and both carry-forward lines as green as they
  were at entry. 17:37 stays red under 443's DECIDED (3).
- **Do not re-bank any floor.** A named or capture row whose count moved is reported with its text.

**If it is not kept:**
- Leave it out of `src`.
- Commit its diff as `.run-unit/unit446-speed-notkept.diff`.
- Record it in `metrics.md` with the table.
- Name the item of §3 that refused it. Do not build a second change to rescue it.

### Task 3 - the exit round

- Build, both carry-forward lines, the three floor tests, the four metrics and every type touched,
  each printed as a number, with the wall times.
- **Report what changed in `src`, file by file**, that none of it keys or transmits, and that it was
  pushed.
- **Tick 5.1** in both copies of `PHASE_PLAN.md` only if:
  - the change was kept under §3's list;
  - both the 5 and 45 WPM tests pass;
  - the decode time is reported before and after.

  If only one end was reached, do not tick it, and say which end is missing.
- **Tick 5.2** only if `135641` now emits sure characters and the report prints them beside
  `nothing read`.
- **Do not tick 5.3 to 5.6, or anything in steps 2 or 3.** 5.6 stays red while 17:37 does.
- Report DRIFT for step 5 (0 if kept, 1 if not), and for steps 2 and 3 unchanged (step 2 at 2;
  step 3 at 0).

---

## 6. Parked - do not touch, do not raise

- Everything in `PARKED.md`, including step 2's question on which files the transmit check covers.
  **This unit does not define or run a transmit-file list.** It reports its `src` diff file by file,
  and that is all.
- 17:37's floor and 444's section 4 question. Both stay with the owner.
- 445's section 4 question: whether a later edge needs a minimum number of wrong letters.
- 444's half-unit dropout rule. It is step 6's material.
- 443's section 4: the traffic-net recording, and the `032012` key.
- **Pitch.**
  - `135641` and `152135` sit 75 Hz off, and that is step 4's, under R76.
  - If `135641` still reads nothing after the speed change, report whether the pitch is the likely
    cause. **Do not touch `CwToneTracker` or `CwToneSurvey`.**
- Speed tracking through a change (HM-REQ-032), the proof state (034), and state kept across a
  clear (035) or a pitch refinement (036). These are 5.3 to 5.5.
- The correctness phase's 5.1, which is Tim's, and everything in `PHASE_PLAN.md` §7 (Carried).
- The decision log, the traceability table, and the 43 tests that measure something else. These
  are step 8 (R80).

## 7. What not to do

- Do not change any bound that the keyer or transmit path shares. Stop there and report it (§0.2).
- Do not loosen a confirmation rule, separation limit or plausibility bound that is not a speed
  bound (V-14).
- Do not tune the change to `135641`. It is one recording. The two ends are judged on the
  synthetic cases and guarded on the whole real set.
- Do not add a second change to rescue a recording that R78 refuses.
- Do not re-bank any floor.
- Do not revert G1, the marks' speed or the dim edge.
- Do not spend a task on the record beyond task 0's entry and the ticks.
- Do not halt on a question. Write one line in section 4 and go on.
- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**

## 8. Committing and pushing

Make one commit per task, and **one commit for the change on its own**. Each message names the
criterion it serves: `unit446 task N: <what> (5.1)`. Push to `origin/main` after each commit. End
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

A. Hamlet meets the CW requirements: step 5 at <n> of 6, step 2 at 3 of 5 and step 3 at 3 of 6 by
   the plan's checkboxes, steps 0 and 1 done, 4, 6, 7 and 8 not started.
B. Step 5, criterion 5.1, HM-REQ-030 with HM-REQ-010, 011 and 012 as guards: the search ran
   <8 to 40> and now runs <n to n>; 5 WPM <reads | reads nothing>, 45 WPM <reads | reads
   nothing>; the change <kept | not kept>; MET-CER-SURE <n> -> <n>; MET-INVENTED <n> -> <n>;
   coverage <n> -> <n>; decode time <n> -> <n>; 5.1 <ticked, or what is missing>; 5.6 red on
   17:37.
C. What this report adds, and whether it stands in the way of a criterion in B.
   Section 4 raises <N> items; <which, if any, is in the way of a criterion in B>.
```

```
UNIT:       446 - <complete|stopped> at task N of 3, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   <yes|no> - <which criteria flipped in PHASE_PLAN.md>
NUMBER:     range <8-40> -> <n-n>; MET-CER-SURE <n> -> <n>; MET-INVENTED <n> -> <n>; decode <n> -> <n> ms/min
DRIFT:      step 5 <n>; step 2 2; step 3 0
```

**Section 3 opens with the four speed cases.** For 5, 8, 40 and 45 WPM, give each case's text
before and after, beside its exact key, with the speed the decoder reported. Then:
- `135641` before and after, beside `nothing read`, or `unmeasured`;
- every real recording whose text changed, before and after, with its key;
- the decode-time table.

**Section 2, in one paragraph:** Will the operator now see text from a station sending at 5 or at
45 words a minute, and is anything read today worse? Give the evidence (V-13), and say what the
synthetic cases do not prove (§12.5).

---

```
ARBITER-DECISION
STEP: 5
APPROACH: trace where the speed search is bounded at 8 and 40 WPM, widen it to 5 and 45, prove both ends on synthetic sends of known speed, keep under R78 with decode time before and after
MOVE: work around
WHY: PHASE_PLAN.md step 5 line 5.1 asks that HM-REQ-030 be met at both ends, with the speed search reaching 5 and 45 WPM where it runs 8 to 40, judged by R78 with decode time before and after. Step 2's 2.5 is held red on 17:37 by 443's DECIDED (3), which only the owner lifts, and 444's route is recorded no. 2.4 is a closing rule no honest unit is aimed at. Step 3's open lines are blocked the same way, and plan section 5 routes to any of steps 2 to 7.
STATE: not started
DECIDED: author's, overrulable - (1) step 5 is worked instead of the launcher's step 2, because step 2's routes are closed this pass and the plan's section 5 makes steps 2 to 7 independent; step 5 is chosen over step 4 because 4.1 is an instrument that changes nothing the operator reads (R83), while 5.1 does; (2) 5.1 is read as the search reaching both ends, proven by one HM-REQ-030 test per end on synthetic sends with exact keys, and meeting every must-tier requirement at every speed is measured at the ends and reported, not claimed; (3) decode time is reported and is not a keep condition, but the 300 s carry-forward cap binds; (4) 444's section 4 question on 17:37 stays with the owner, and while it stands every step's floors-green line stays red; (5) 445's section 4 question is logged and not chased.
LICENCE: PHASE_PLAN.md step 5 line 5.1, section 5's independence line, R78, R80, R81, section 6; R82 and R83 as recorded in work instruction 441; arbiter rulings 442 DECIDED (2) and 443 DECIDED (3); V-04; V-06; V-11; V-13; V-14; R72; HM-REQ-010, 011, 012, 030, 031, 033; HM-DEC-155; HM-DEC-165; CLAUDE.md 0.0, 0.2 and 12.5
ACCOMPLISHED: a station sending as slowly as 5 or as fast as 45 words a minute is found and read, where today the decoder only searches 8 to 40 and a 44 WPM station reads nothing, with nothing read today made worse
ADVANCES: step 5 criterion 1
END-ARBITER-DECISION
```
