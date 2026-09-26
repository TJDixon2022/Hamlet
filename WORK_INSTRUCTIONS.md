# Work instruction 443 - the letters the decoder adds between words

**Seeded by `run-phase.bat`, redirected once.** By the plan's checkboxes, step 2 stands at 3 of 5:
2.1, 2.2 and 2.3 are ticked. 2.2 is parked for this pass because its question is waiting on the owner
in `PARKED.md`. What is left in step 2 is 2.4, a closing rule that no single unit can build, and 2.5,
which is held red by 17:37's named floor. **`PHASE_PLAN.md` step 3 says: "Independent of step 2: when
one blocks the arbiter works the other."** So this unit opens step 3.

**Step 3 is HM-REQ-011: MET-INVENTED at zero.** Unit 439 measured **13 sure added characters** over
473 sent. After 441's two kept changes, the tree measures **4** (unit 442's entry). Those added
characters are the litter between real words that the operator reads as text, such as the 7.052
traffic net's `EETTTEETTTTTTTTETTETETKTETEE` between `GRAY KC` and `LIVER VIA`. Nobody has traced
them yet. This unit traces them the way 2.1 traced the wrong ones, then builds one change against
what the trace shows.

**The owner's standing order still holds, 2026-09-25:** *"Write something that materially
advances CW in the most significant way we can handle."*

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

The count today: **step 2 at 3 of 5 criteria met, and step 3 at 0 of 6.** Steps 0 and 1 are done,
and steps 4 to 8 are not started. At HEAD `1308f688`, on real recordings with inferred keys (unit
442's exit):

| metric | count | value |
|---|---|---|
| MET-CER-SURE | 47 of 421 sure (43 substituted, 4 added) | 0.1116 |
| MET-INVENTED | 47 over 473 | 0.0994 |
| sure-and-right coverage (R82) | 374 over 473 | 0.7907 |
| MET-WBE | 52 over 113 | 0.4602 |

Floors: captures 51 of 51, adjudicated 13 of 13, named 12 of 13 (17:37 is red).

```
PHASE GOAL: Hamlet meets the CW requirements.
UNIT GOAL:  Every sure character the decoder added where nothing was sent is traced, and it
            is shown which of unit 439's 13 are already gone and why. Then one change removes
            the largest group of what is left, without adding a wrong letter.
ADVANCES:   step 3 criterion 1
```

**Read `CW_REQUIREMENTS.md` first, then `CW_SPEC.md`, including §11 for MET-INVENTED's
definition.** Where they differ from this instruction, they win. Here are the requirement ids this
unit works toward. **Quote each from the document in section 1, and report any difference from this
instruction as a mismatch.**

- **HM-REQ-011:** MET-INVENTED at zero on the must-tier conditions.
- **HM-REQ-010:** MET-CER-SURE below 1 %. This is the guard: a change may not remove an added letter
  by making a sure letter wrong.
- **HM-REQ-012, 014, 015:** the confidence classes and what "sure" promises. Quote them. A letter
  that is not sent may go to unknown or dim. It may not be printed as sure.

## 2. Verify this instruction against the tree

Check each of these. **Where one is wrong, report it as a mismatch in section 1 and carry on. Do not
repair it.**

- Commits `1fb0bad6` (unit 440's entry, where 439's 13 added were measured), `f14b2453`, `42d5dbb9`
  (G1) and `14f515bd` (the marks' speed) are in HEAD's ancestry.
- `.run-unit/unit440-trace.txt` is the 2.1 trace and prints the sure-wrong letters only. Nothing in
  the tree traces the added ones yet.
- The string `EETTTEETTTTTTTTETTETETKTETEE` appears in no fixture or test, only in the plan. Find the
  7.052 traffic-net recording it comes from by its text and its `cases-*.txt` row, and name it. If
  it cannot be found, say so. Do not reconstruct it.
- **Expected failures at entry, and they are not yours to fix:** 17:37's named floor is red (banked
  46, reads 38). `TheFiveToEightDecibelPlateauHolds` is the correctness phase's recorded red. The
  app line loses up to 5 to the dispatcher loop, and each of those types is green alone.
- **Known and not yours:** `PHASE_OUTCOME.md`'s header still lists the old titles for steps 2, 3
  and 8. `CW_SPEC.md` §11 still defines MET-COVERAGE as sure over sent, where R82 makes it
  sure-and-right. Report each once and do not edit either.

## 3. Rulings in force - do not re-argue

- **`PHASE_PLAN.md` R77 to R81, §6, and R82 and R83** as work instruction 441 recorded them:
  - **R82:** MET-COVERAGE is sure-and-right over sent.
  - **R83:** a unit of this phase changes what the operator reads, or it is not authored.
- **R78, the keep rule.** A change is kept when it moves a requirement's metric the right way and
  breaks no other requirement. **For this unit's change, as 3.2 states it:**
  - MET-INVENTED falls;
  - MET-CER-SURE does not rise;
  - sure-and-right coverage does not fall;
  - the adjudicated readings hold, or move onto their own adjudicated text;
  - V-11 holds, per keyed recording.

  R78 lists **MET-WBE** among its four metrics, so report it before and after. **A capture row's
  character count falling is reported, not rejected.**
- **V-11:** no change may redden an earlier capture to green a newer one. It is judged on the
  requirements' metrics.
- **V-13:** an inferred key is not proof by itself. **V-04 and V-14:** no fixture is admitted by
  lowering a gate, and no plausibility bound is loosened to pass one.
- **R72:** no word, dictionary or callsign prior, in any form. **A character is dropped because of
  its marks, its gaps, its energy and its timing, never because of the letters around it or the word
  it would make.**
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
`.run-unit\unit443-<name>.sh` and run with `sh`. **Never compose a timestamp. Read the clock.**

---

## 5. The tasks

### Task 0 - the entry

- `PHASE_OUTCOME.md` gets `## UNIT 443 - STEP 3` from the decision block at the foot.
- `PHASE_STATUS.md` names 443 and `CURRENT_STEP: 3`. Patch-bump.
- Entry round: build, both carry-forward lines, the three floor tests, and the four metrics, each
  printed as a number. **MET-INVENTED is printed split into added and substituted, per condition,
  with the key's kind beside each number.**
- Commit the root's uncommitted `PHASE_OUTCOME.md`, `PHASE_STATUS.md`, `RUN_LEDGER.md` and
  `PARKED.md` with the entry. **These are the runner's writes.** Commit them as they are and do not
  edit them.

### Task 1 - the trace of the added characters (3.1, and 3.4's "before")

**A fact that asserts nothing**, in the form of `WhereTheSureWrongLettersComeFromTests`. Write it as a
new fact beside that one and do not change the old one. For **every sure character that MET-INVENTED
counts as added**, print:

- the recording, and the time in seconds;
- the text either side, as sent and as emitted;
- the character emitted, and its element pattern;
- its span, and the spans before and after it;
- the speed and the unit in milliseconds at the hop;
- the pitch at the hop, and the sender's pitch;
- the envelope's marks and gaps under it, in milliseconds and in units;
- whether it sits beside an inserted or a missed word space;
- its energy against the noise at the hop, if the decoder has that number. If it does not, say so.

**Run it twice.** Run it at `1fb0bad6`, where 439's 13 stand, by checking out only the decoder's
`src` files as 442 did and restoring them afterwards. Then run it at HEAD, where 4 stand. **Match
the two lists.** Mark each of the 13 as gone or still there, and each of the 4 as old or new. For
each one that went, name which of 441's two commits removed it. **Then group what is left at HEAD
by what the characters have in common**, and name the largest group.

**Then print 3.4's text at HEAD:** the traffic net recording from `GRAY KC` to `LIVER VIA`, as the
operator reads it. Print it beside the plan's `EETTTEETTTTTTTTETTETETKTETEE`, and say at which
commit that text was read, if the tree records it. **Say whether that recording has a key.** If it
has none, its characters are not counted by MET-INVENTED, and the report must say that plainly.

**The drop candidate is the `1fb0bad6` half of the run.** If the unit runs long, drop it, trace
HEAD only, and say that 3.1 is then met only against today's added characters and not against
439's 13.

Commit the fact and its printout.

### Task 2 - one change against the largest group (3.2)

**This is the unit's real work.** Build **one** change, aimed at the cause task 1 named for its
largest group. That group may be a single short element in a long gap, a mark below the energy the
others rest on, or a letter printed before acquisition. **If the added characters are few and
share no cause, aim at the untraced litter 3.4 shows instead.** Say which you chose. **Take the rule
from the trace. Do not tune it after you have seen R78's numbers.** A letter that is not sent may be
dropped, or it may be printed as unknown or dim. It may not be printed as sure.

- G1 and the marks' speed stay. Do not revert or weaken either one.
- `RivalMargin` and `MarginLlr` stay as they are. Do not remove them, and do not extend them.

**Judge the change under §3's list, every item as a number.** Print every recording whose text
changes, before and after. Keep it or do not keep it, and give the reason in one line.

**If it is kept:** commit the change on its own. Write MET-INVENTED and MET-CER-SURE before and
after to `docs/phase-requirements/metrics.md`, per condition, with the key's kind beside each
number (3.3). Print 3.4's text after the change. The three floor tests and both carry-forward lines
exit green, apart from §2's recorded reds.

**If it is not kept:** leave it out of `src`, commit its diff as
`.run-unit/unit443-added-notkept.diff`, and record it in `metrics.md`.

**Do not re-bank any floor.** 17:37 stays red at its recorded 38. Do not make it or any other row
worse.

### Task 3 - the exit round

- Build, both carry-forward lines, the three floor tests, the four metrics, and every type touched,
  each printed as a number.
- **Report what changed in `src`, file by file**, that none of it keys or transmits, and that it was
  pushed.
- **Tick 3.1** in both copies of `PHASE_PLAN.md` only if task 1's trace printed every added
  character with every field it names, at HEAD and at `1fb0bad6`, and grouped them. Print the one
  line of evidence. **Tick 3.2 and 3.3** only if the change was kept and every item of §3's list
  holds. **Do not tick 3.4:** it needs "before and after" on a kept change and a text the owner
  reads, and if task 2 kept one, report it for the next unit. **Do not tick 3.6** while 17:37 is
  red.
- Report DRIFT for step 3: 0 if the change was kept, 1 if not.

---

## 6. Parked - do not touch, do not raise

- Everything in `PARKED.md`, including step 2's question on which files the transmit check covers.
  **This unit does not define or run a transmit-file list.** It reports its `src` diff file by file,
  and that is all.
- Step 2's 2.2, 2.4 and 2.5, and 17:37's named floor.
- The correctness phase's 5.1, which is Tim's, and everything in `PHASE_PLAN.md` §7 (Carried).
- The decision log, the traceability table, and the 43 tests that measure something else. These
  are step 8 (R80).
- `PHASE_OUTCOME.md`'s stale header, and `CW_SPEC.md` §11's coverage text.
- MET-WBE as a target. That is step 6.

## 7. What not to do

- Do not drop or dim a character because of the letters beside it or the word it would make (R72).
- Do not move the rule after reading R78's numbers. Do not add a second rule to rescue a recording.
- Do not loosen a gate or a plausibility bound to pass a fixture (V-04, V-14).
- Do not reject a change because a character count fell. Report it (R78).
- Do not re-bank a floor. Do not revert G1 or the marks' speed.
- Do not spend a task on the record beyond task 0's entry and the ticks.
- Do not halt on a question. Write one line in section 4 and go on.
- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**

## 8. Committing and pushing

One commit per task, and **one commit for the change on its own**. Each message names the criterion it
serves: `unit443 task N: <what> (3.1)`. Push to `origin/main` after each commit. End every commit
message with:

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

A. Hamlet meets the CW requirements: step 2 at 3 of 5 (2.2 parked), step 3 at <n> of 6 by the
   plan's checkboxes, steps 0 and 1 done, 4 to 8 not started.
B. Step 3, HM-REQ-011 with HM-REQ-010 as the guard: 3.1 <ticked, or what is missing>; the
   change <kept or not>, with §3's numbers. Added <13 at 1fb0bad6> -> <n at entry> -> <n at exit>;
   MET-INVENTED <before> -> <after>; MET-CER-SURE <before> -> <after>.
C. What this report adds, and whether it stands in the way of a criterion in B.
   Section 4 raises <N> items; <which, if any, is in the way of a criterion in B>.
```

```
UNIT:       443 - <complete|stopped> at task N of 3, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   <yes|no> - <which criteria flipped in PHASE_PLAN.md>
NUMBER:     added <n> -> <n>; MET-INVENTED <n> -> <n>; MET-CER-SURE <n> -> <n>
DRIFT:      <consecutive units on step 3 without a kept change>
```

**Section 3 opens with what the operator reads.** Give the traffic net's text from `GRAY KC` to
`LIVER VIA` at entry, and at exit if it changed. Then give every recording whose text changed,
before and after, with each added character that went marked in brackets.

**Section 2, in one paragraph:** Does the operator now see fewer letters where nothing was sent,
without any letter that was sent turning wrong? On what evidence (V-13)?

---

```
ARBITER-DECISION
STEP: 3
APPROACH: trace every sure added character at 439's baseline and at HEAD with span, speed, pitch, marks, gaps and energy, match the two lists to show which 441 removed, group what is left, and build one change against the largest group, kept under R78 with MET-INVENTED falling
MOVE: work around
WHY: PHASE_PLAN.md step 2's 2.2 is parked this pass and its remaining 2.4 and 2.5 cannot be built by a unit, and the plan says step 3 is independent of step 2 and is worked when step 2 blocks; step 3 criterion 3.1 asks that the sure added characters be traced as 2.1 traced the wrong ones, which is the measurement HM-REQ-011's MET-INVENTED needs before any change.
STATE: not started
DECIDED: author's, overrulable - (1) step 3 is opened because step 2 is blocked this pass, on the plan's own dependency line; (2) 3.1's "13" is traced at 1fb0bad6 where 439 measured it and matched against HEAD's 4, with the baseline half as the drop candidate; (3) R78 names MET-WBE among its metrics, so no floor is re-banked while 17:37's boundaries are worse and 17:37 stays red, which keeps 3.6 unticked this unit; (4) RivalMargin and MarginLlr are left in src untouched; (5) the parked transmit-file list is neither defined nor ruled on here - the unit reports its src diff file by file and touches nothing that keys or transmits
LICENCE: PHASE_PLAN.md step 3 dependency line, R78, R80, R81, section 6; R82 and R83 as recorded in work instruction 441; V-11; V-13; V-04; V-14; R72; HM-REQ-010, 011, 012, 014, 015; HM-DEC-155; HM-DEC-165; CLAUDE.md 0.0 and 0.2
ACCOMPLISHED: every letter the decoder prints with confidence where nothing was sent is traced to its cause, and the commonest cause is attacked, so the operator reads less junk between real words
ADVANCES: step 3 criterion 1
END-ARBITER-DECISION
```
