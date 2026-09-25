# Work instruction 432 - the mixdown follows a move once any read since it was set has confirmed it

**Seed under `--seed`.** The 7.052 session opens with `UIEH EE E E T I NIEEE E` on the
spliced stream, and then reads a whole QSO. Unit 429 traced this to `CwDecoder.cs` 617 to 621.
There, the mixdown followed the tracker from 600 to 525 Hz while the sender stood at 625, and
the speed then collapsed. Unit 430 built one change against that line and one narrower
variant:
- **The change** (`5b6b704c`) holds a move of more than 25 Hz as pending until a later survey
  read confirms keying at the new pitch.
- **The variant** (`ec76051e`) follows a move at once if the read just before it confirmed it.

**Both cured the opening.** The stream read `EANQNID EAN■IK`, the group the cold decode reads.
Both lowered the keyed total, from 165 to 149 and to 153. **Both went out on one recording
only**: `cw-2026-08-22-032113` fell from 47 to 44 named, outside its scored stretch. Unit 430
measured why the variant still cost it. The tracker's 650 Hz move was confirmed at 25.54 s,
the next read at 26.04 s admitted nothing, and the move was made at 26.54 s. A look-back of one
read missed the confirmation, and the mix stayed at 600 from 26.54 to 29.54 s.

This unit tests a third release rule, P39. **A pending move is released as soon as any survey
read since the mix's pitch was last set has confirmed keying there.** Before building it, the
unit replays the reads to check that the rule follows both of `032113`'s moves and still holds
the opening's wrong one. It is built only if the replay says so, and kept only under 3.2's four
tests. Four tasks, drop from the back.

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

**HM-DEC-155.** Never run the whole suite. Run only this unit's named types and
`docs\carry-forward-tests.txt`, as its top comment says. **Never background and poll.** Run one
type per invocation, each with its own `timeout`:
- the engine carry-forward line ran 371 s at unit 431's entry; give it 600 s;
- the captures type is 51 rows and ran 118 s; give it 600 s;
- `WhatTheOpeningHeardTests` ran 162 s at unit 430's exit; give it 600 s;
- give `WhatTheStrayLettersRestOnTests` 600 s.

A run lost before any assertion counts neither way and is re-run once, alone. That means the
test host crash inside `Cw` (HM-OPEN-063) or the headless dispatcher loop.

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

These are known limits of the shell here:
- Apostrophes in quoted heredocs break, and doubled backslashes collapse.
- `;` and `rm` are refused, and Python cannot run.
- A multi-line commit needs `-m` more than once.
- A bare `git worktree`, `git checkout` or `git show` is refused at the prompt.

Put multi-step commands in `.run-unit\unit432-<name>.sh` and run them with `sh`. You can copy
the scripts from units 430 and 431. **To take a change back out, commit a revert**
(`git revert --no-edit <sha>` in a script). Never reset or rewrite history.

## 3. Asks still outstanding

These are carried under HM-DEC-139. **None of them belongs to this unit.** P27 stays the
owner's. P29 to P38 and P40 to P43 stay parked. This unit takes up **P39**, and only that one.

Task 0 parks unit 431's two section 4 items verbatim, with their proposed rulings:
- **P44:** whether a join may be judged by the letters it leaves.
- **P45:** the app carry-forward line loses one to three types to the dispatcher loop. This is
  for the record.

This unit answers nothing except its own criterion.

---

## 4. Why this unit exists

```
PHASE GOAL: Hamlet reads a CQ call correctly.
UNIT GOAL:  Stop the mixdown walking off the sender in a session's opening,
            without costing 032113 the letters unit 430's two attempts cost it,
            kept only under 3.2's four tests.
ADVANCES:   step 7 criterion 4
DRIFT:      0
```

**The count today:**
- Steps 0, 1 and 2 are done.
- Step 3 is partial and closed under 3.4 by unit 431 (P43), with 3.6 open.
- Step 4 is not started.
- Step 5 is the owner's verdict.
- Step 6 is partial, with 6.5 open.
- Step 7 is partial, with 7.1, 7.2, 7.4, 7.6 and 7.8 open.

**Units 430 and 431 did not advance.** The loop has sent this instruction back to 7.4 or 3.6.
It must use an approach the record does not show failing.

**Why 7.4 and not 3.6.** Every route recorded at 3.6 has failed:
- a span bar (unit 421);
- features other than span (unit 425);
- span over neighbors (unit 428);
- G1's join (unit 431).

G1's only way back is P44, which is the owner's to rule. An arbiter's reading of R73 cannot be
loosened by another arbiter. At 7.4, by contrast, both of unit 430's attempts passed tests 1
and 3, cured the opening, and failed on the same three characters of one recording. The cause
of that failure was measured: a hold that spanned two reads.

**Why this is not a loop.** Unit 430's rules were:
- "wait for a later read";
- "follow at once if the read just before confirmed".

The second rule's failure was traced to its look-back of one read. P39's rule looks back over
every read since the mix's pitch was set. That is a different release condition, aimed at the
measured failure. Task 1 checks it against the recorded reads before anything is built, so it
cannot be a guess tuned by score. The loop test found no entry for this approach.

---

## 5. Verify this instruction against the tree

Check each of these. Report any mismatch and repair nothing:

- `CwDecoder.cs` takes `_lastMeasuredToneHz` from `_tracker.ToneHz` at lines 600 to 603. It
  writes `_probabilistic.ToneHz` at 617 to 621.
- The following four commits exist and are as unit 430 describes them:
  - `5b6b704c`, the change;
  - `a7e6e2f2`, its revert;
  - `ec76051e`, the variant;
  - `fa64edc5`, its revert.

  Say where the first change's pending-move logic sat, and whether it re-applies cleanly.
- `WhatTheOpeningHeardTests.WhyTheMixMoved` is in `tests\Hamlet.RadioEngine.Tests\Cw`, and
  prints the tracker's `Verdict` per survey read.
- `cw-2026-08-22-032113` has a row in the captures type, with named 47 at or above the bar and
  elements 102. It also has a keyed floor of 47.
- The numbers at HEAD are:
  - all keyed: **165 edits over 565**, against inferred keys;
  - 17 added letters, of which 8 are single-element;
  - captures 51 of 51, adjudicated 13 of 13, keyed floors 13 of 13.
- The opening at HEAD, on the spliced stream from 0 to 46.2 s, reads `EII E T NHHK` then
  `UIEH EE E E T I NIEEE E E ET N ■IK`.
- `Directory.Build.props` is at 1.13.118.
- The launcher writes the following at the root, and the reload shows them as modified:
  - `PHASE_OUTCOME.md`;
  - `PHASE_STATUS.md`;
  - `RUN_LEDGER.md`;
  - `WORK_INSTRUCTIONS.md`.

  It also writes `.run-unit` files. Task 0 commits the root files with its record as they
  stand, and says so.

**Expected failures.** Report each of these and repair none of them:
- the dispatcher-loop losses on the app carry-forward line, each re-run alone (P45);
- `AHeldPitchDoesNotOutliveItsEvidenceTests` at 1 of 4 (P40);
- `EveryElementCarriesItsOwnPitchTests` and `ThePeakFindsThePitchTheTrackerMissedTests`, which
  run 0 tests because they are `Compile Remove`d (P41).

## 6. Rulings in force - do not re-argue

`PHASE_PLAN.md` R59 to R74, with §3 and §6.

**3.2's four tests are the keep rule, as unit 430 applied them to 7.4:**
1. The total edit count over all keyed recordings **does not rise** above 165 over 565,
   against inferred keys. No key scores the opening, so 7.4 cannot be required to lower the
   total. This is unit 430's arbiter decision, and it stands.
2. No named floor from 2.2 is broken.
3. The three adjudicated readings are unchanged character for character, or changed to
   exactly their own adjudicated text (R66). Print any reading that moves, before and after.
4. No capture row's above-bar named count falls (R71, raw span 13.0), except as R73 allows.
   Under unit 431's arbiter reading, R73 does not reach a fall outside a scored stretch, or
   any fall on an unkeyed row.

**Other rulings:**
- **R68:** the acquisition failure is step 7.
- **R73:** only a key-aligned added character inside a scored stretch may leave a floor.
- **R66**, **R71**, and **R72**: no word, dictionary or callsign prior.
- **R61:** no key is invented, and no scored region is changed.
- **HM-DEC-095** and **HM-DEC-127:** the tracker's choice of candidate is the tracker's. This
  unit changes only when the mixdown follows it.
- **HM-DEC-091:** a change that reads one recording and costs another is not a fix.
- **§0.0:** no decode is called what was sent.
- **§0.2:** nothing that keys or transmits is touched.
- **§12.5:** no synthetic case is the sole evidence for keeping a change.
- Also in force: **HM-DEC-155**, **HM-DEC-165**, **FACT-004** and **FACT-006**.

**The author's decisions, overrulable, recorded as decisions and not as rulings:**

- **Routing to 7.4 over 3.6.** Section 4 gives the reason.
- **The rule.** A move of up to 25 Hz is followed at once, as in `5b6b704c`. A move further
  than that is followed at once if any survey read since the mix's own pitch was last set
  carried confirmed keying within 25 Hz of the new pitch. Otherwise it is held pending until
  a read does. The 25 Hz is unit 430's same-station distance, kept unchanged. P39's clause
  about a `Switch` made on the very read that confirmed it still waits for a later read, and
  task 1 says whether it matters on the four stretches.
- **The replay gate.** The rule is built only if task 1 shows that it does three things:
  - follows `032113`'s 650 Hz and 500 Hz moves when the tracker made them;
  - holds the opening's 600 to 525 Hz move;
  - makes no other difference from `5b6b704c` on the stretches task 1 prints.

  If it fails any of the three, build nothing, and report it.
- **One attempt, and no narrower variant.**
- **7.4 ticks only on a kept change.** 7.4's own three-unit count counts 7.4 units only, and a
  step 3 unit in between does not reset it. If this unit keeps nothing, it is the second
  (430 and 432).
- **P42's figure.** The opening is reported as its text beside the cold group `EANQNID`,
  together with the named count. The count alone is not the figure, because removing litter
  lowers it.
- **No self-ruling authorizes work outside these tasks.**

## 7. Status cadence

As the header says.

---

## 8. The tasks

### Task 0 - the record

- Add `## UNIT 432 - STEP 7` to `PHASE_OUTCOME.md`, from the decision block at the foot of
  this file.
- `PHASE_STATUS.md` names unit 432 and `CURRENT_STEP: 7`.
- Patch-bump `Directory.Build.props` from 1.13.118 to 1.13.119.
- Park unit 431's section 4 items 1 and 2 verbatim in `docs\phase-correctness\PARKED.md` as P44
  and P45. Mark P39 as taken up by unit 432.

**Entry round.** Run each of these, one type per invocation:
- both carry-forward lines;
- the three floor tests, with captures at 51;
- the keyed totals, per recording and all together;
- the added letters, split into single-element and not;
- `WhatTheOpeningHeardTests`, which gives the opening's text and named count, cold and on the
  spliced stream;
- unit 430's pitch types, as its report tables them.

Record these as the numbers to hold.

**Drop candidate:** none.

### Task 1 - the replay (7.4)

Build nothing in the decoder in this task. Add one member to `WhatTheOpeningHeardTests` that
**asserts nothing**. It runs on these stretches:
- the spliced stream from 28 to 38 s;
- `032113`, whole;
- `031905`, whole, because it gained under unit 430's change.

For every tracker move of more than 25 Hz on those stretches, it prints:
- the time of the move and its pitch from and to;
- every survey read since the mix's pitch was last set, each with its time, its `Verdict`,
  and whether it carried confirmed keying within 25 Hz of the target;
- whether the move was a `Switch` made on the read that confirmed it;
- the time at which each of four rules would move the mix: entry, `5b6b704c`, `ec76051e`,
  and this unit's rule.

It reads the tracker the way `WhyTheMixMoved` does, and writes nothing.

**State the gate's three answers** from section 6 as yes or no, each with the line of the
print that shows it. If any answer is no, build nothing in task 2. Say which answer was no,
and go to task 3.

**Drop candidate:** `031905`.

### Task 2 - the rule, judged (7.4)

Build section 6's rule in `CwDecoder.Step` **in its own commit**, on the shape of `5b6b704c`.
Do not touch any of these:
- the tracker and its survey scoring;
- the speed search;
- the gap estimator;
- the gates;
- `CharacterMargin`, `StrayElementSpan` or the span bar.

Then run each of these, one type per invocation:
- the keyed totals, per recording, with 17:37's edits over its scored region;
- the added letters, single-element and not;
- the keyed floors;
- adjudicated;
- captures: all 51 rows, each with its old, above-bar and below-bar counts and its elements;
  **`032113` is read first**;
- `WhatTheOpeningHeardTests`;
- unit 430's pitch types.

Judge the change under section 6's four tests, and print each test as a number. **For every
row whose above-bar count falls, list every character that left**, with its recording and its
time, and say whether it sits inside a scored stretch.

**If the change fails any test, take it back out in the next commit**, and say which test
failed.

**If it is kept:**
- report the opening's named characters in the opening 60 seconds of `003901` and `003919`,
  before and after, in two forms: cold per file, and on the spliced stream, beside
  `EANQNID`;
- tick 7.4 in `PHASE_PLAN.md`.

**Drop candidate:** the pitch types' after-round. The captures, floors and adjudicated tests
are not dropped.

### Task 3 - the exit round

Run each of these:
- both carry-forward lines;
- the three floor tests, with captures at 51;
- the keyed totals and the added letters;
- `WhatTheOpeningHeardTests`.

Then check two things against the tree:
- none of the eleven transmit files differs from `7e209cb4`;
- `data` is unchanged from entry.

If nothing was kept, `src` is also unchanged from entry.

**Drop candidate:** none.

---

## 9. Parked - do not touch, do not raise

- **3.6 and G1.** P43 and P44 hold them. If the rule moves any single-element added letter,
  report it as a finding and do not claim it for 3.6.
- **`AHeldPitchDoesNotOutliveItsEvidenceTests` and `Retuned()`** (P40). Report the count and
  nothing more.
- **The held gaps on `004535`** (P37).
- **The sidecar's counters** (P38).
- **7.1, 7.2 and 7.8. Step 4. 6.5.**
- **The 2026-09-25 traffic net** (P35).
- **P27, P29 to P38, P40 to P45.**
- **Any key, scored region, floor or span bar.** 7.4 moves no floor.

## 10. What not to do

- **Do not move the tracker's choice.** HM-DEC-095 governs it, and unit 430 traced the move
  to it.
- **Do not re-build `5b6b704c` or `ec76051e` as they were.** Both are recorded as failing.
- **Do not build a second variant**, whatever task 2 shows.
- **Do not excuse `032113`'s fall, or any fall outside a scored stretch.**
- **Do not keep a change on the opening's figures alone.** The four tests decide.
- **Do not add a word, dictionary or callsign prior** (R72).
- **Do not touch anything that keys or transmits.**
- **Do not halt for a question.** Park it.
- **Write `output.md` before the session ends.**
- **Never run an unfiltered `dotnet test`. Never background and poll. Never compose a
  timestamp.**
- **Report mismatches and repair nothing. Use American spelling and UTF-8. Use the four
  headings exactly.**

## 11. Committing and pushing

Commit after each task. The change and any revert each go in their own commit. Push at the end
and say whether the push succeeded.

---

## 12. Reporting

Write `output.md` at the root, with the four headings exactly as section 1 gives them.

```
READ IN THIS ORDER.

A. Hamlet reads a CQ call correctly. Steps 0 to 2 done; 3 partial and
   closed under 3.4 with 3.6 open; 4 not started; 5 the owner's; 6 partial
   with 6.5 open; 7 partial with 7.1, 7.2, 7.4, 7.6 and 7.8 open.
B. Step 7, criterion 7.4: the mixdown follows a move once any survey read
   since it was set has confirmed it (P39); the replay's three answers;
   built or not; kept or taken back out, and which test decided it;
   032113 before and after; the opening's text before and after beside
   EANQNID; 7.4 ticked or not.
C. The rest, weighed against A and B. Section 4 raises <n> items; say
   whether any stands in the way of 7.4.
```

```
UNIT:       432 - <complete|stopped> at task N of 3, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   yes | no - <why, on the line>
NUMBER:     keyed 165 -> <n> over 565, inferred keys; 032113 above-bar 47 -> <n>; opening stream 30 to 46.2 s <n> -> <n> named, <text before> -> <text after>
DRIFT:      <0 if a criterion moved, else 1>
```

**Section 3 leads with task 1's three gate answers**, each with the print line that shows it.
**Then give the four tests as a table**: each test, the number before, the number after, pass
or fail, and whether the change was kept. **Then give the opening's text before and after,
beside `EANQNID`**. **Then give `032113`'s above-bar count and the time the mix followed each
of its two moves.**

---

```
ARBITER-DECISION
STEP: 7
APPROACH: release a pending mixdown pitch move once any survey read since the mix was last set confirmed keying within 25 Hz of it (P39), checked first by replaying 032113's two held moves and the opening's 525 Hz move against four release rules, then built once and judged under 3.2's four tests
MOVE: work around
WHY: Every recorded 3.6 route has failed, and G1's way back (P44) is the owner's. At 7.4, unit 430's two release rules both cured the opening and failed on 032113 alone, traced to a hold spanning two reads. This rule looks back over every read since the mix was set, and it is gated on a replay of the recorded reads, not on a score.
STATE: partial
DECIDED: author's, overrulable - routing to 7.4 over 3.6; the release rule with unit 430's 25 Hz kept; the replay gate (follows 032113's 650 and 500 Hz moves, holds the opening's 525 Hz move, no other difference from 5b6b704c) or nothing is built; one attempt and no variant; test 1 reads does not rise, as unit 430's arbiter decided; 7.4 ticks only on a kept change, and its three-unit count counts 7.4 units only; the opening reported as text beside EANQNID with its count; unit 431's items parked as P44 and P45. No self-ruling authorizes work outside the tasks.
LICENCE: PHASE_PLAN.md R68, R64, R65, R66, R71, R72, R73, section 6 and criteria 7.3 and 7.4; PARKED.md P39 and P42; unit 430's arbiter decision on test 1; HM-DEC-095; HM-DEC-127; HM-DEC-091; HM-DEC-155; HM-DEC-165; CLAUDE.md 0.0, 0.2 and 12.5
ACCOMPLISHED: the first minute of a CW session reads the sender's letters instead of a run of E and T, because the decoder no longer follows its pitch tracker off the sender, and no recording loses a letter it read before - or the replay or the four tests show why this release rule does not do it, with the numbers on record
ADVANCES: step 7 criterion 4
END-ARBITER-DECISION
```
