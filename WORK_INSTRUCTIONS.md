# Work instruction 431 - the split letters joined back, judged under the rules that now allow it

**Seed under `--seed`.** On `cw-2026-09-23-173723` the sender keyed `WB6RED` and the decoder
reads `W T E E T E  E ERE D`. The stray `E` and `T` here are not noise between words. They are
pieces of real letters, split by a gap the decoder read as a character gap. Unit 413 traced 8
of 17:37's splits to one cause: a held gap reading whose character gap stands past its word
gap. It built G1, `687aab1a`, which refuses that reading. G1 took all keyed recordings from
217 to 193 edits and 17:37 from 29 to 11. It went back out only because joining the pieces
lowered two named counts, 17:37 46 to 38 and `004133` 30 to 25. **Both are keyed
recordings.** R71 and R73 did not exist then. Under R73, a character the key aligns as added,
inside a scored stretch, may now leave a floor. This unit asks whether G1 passes 3.2's tests
under the rules as they now stand, and is kept if it does. If nothing is kept, this is the
third consecutive step 3 unit with no kept change, and 3.4 closes the step partial. Five
tasks, drop from the back.

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

**HM-DEC-155.** No suite. Run only this unit's named types and `docs\carry-forward-tests.txt`,
as its top comment says. **Never background and poll.** One type per invocation, each with its
own `timeout`. The engine carry-forward line ran 375 s at unit 430's entry; give it 600 s.
The captures type is 51 rows and ran 120 s; give it 600 s. Give `WhereTheWordsBreakTests` and
`WhatTheStrayLettersRestOnTests` 600 s each. A run lost before any assertion counts neither
way and is re-run once, alone. That means the test host crash inside `Cw` (HM-OPEN-063) or the
headless dispatcher loop.

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

Put multi-step commands in `.run-unit\unit431-<name>.sh` and run them with `sh`. Unit 430's
scripts can be copied. **To take a change back out, commit a revert** (`git revert --no-edit
<sha>` in a script). Never reset or rewrite history.

## 3. Asks still outstanding

Carried per HM-DEC-139. **None is this unit's.** P27 stays the owner's, and P29 to P38 stay
parked. Task 0 parks unit 430's four section 4 items verbatim as P39 to P42, with their
proposed rulings:
- P39: what the second 7.4 unit builds.
- P40: `AHeldPitchDoesNotOutliveItsEvidenceTests` red at entry, 1 of 4.
- P41: the named pitch test.
- P42: the opening's figure.

This unit answers nothing but its own criterion.

---

## 4. Why this unit exists

```
PHASE GOAL: Hamlet reads a CQ call correctly.
UNIT GOAL:  Join the stray E and T that are pieces of split letters back into
            the letters the sender keyed, kept only under 3.2's four tests.
ADVANCES:   step 3 criterion 6
DRIFT:      0
```

**The count today.** Steps 0, 1 and 2 are done. Step 3 is partial, with 3.4 and 3.6 open.
Step 4 is not started. Step 5 is the owner's verdict. Step 6 is partial, with 6.5 open. Step
7 is partial, with 7.1, 7.2, 7.4, 7.6 and 7.8 open. **Unit 430 did not advance.** If this
unit also does not flip 3.6, that is two in a row.

**Why step 3, and why 3.6.** R64 puts the spacing first. 3.6 is step 3's open work, and 3.4
exists only to close it. Units 421, 425 and 428 each looked for a figure that tells an
emitted stray apart from a right letter. They tried raw span, per-hop span, span over
neighbors, the Gate score, gaps in units, standing alone, pitch and energy. None separated
them (P36). **A sixth figure would be a loop, so this unit does not look for one.** It
attacks the strays where unit 413 found they are made: the gap reading that splits a letter.

**Why this is not a loop.** G1 was tried once, under 3.1, and it cut edits more than any
change since. It went out on 3.2's second and fourth tests alone. Since then the owner has
changed both tests on purpose:
- **R71:** a floor counts at or above a raw span bar.
- **R73:** a key-aligned added character inside a scored stretch may leave a floor.

Unit 416 is the precedent: it re-applied a change taken out under a test that R66 later
amended, and kept it. Judging G1 again under the amended tests is new evidence, not a repeat.
The loop test found no entry for this approach.

**Why not 7.4 again.** The prompt's step is 3. Unit 430's item 1 is a real 7.4 route and is
parked as P39 for step 7's next unit. Unit 430 also measured that its mixdown change left
the 8 single-element added letters where they were, so it is not 3.6's route.

---

## 5. Verify this instruction against the tree

Check each of these. Report any mismatch and repair nothing:

- `687aab1a` added one check to `CwUnitEstimator.MeasureGaps`. After `word` is computed, it
  returns `textbook` when `character >= word`. `b4ccab9e` took it out. `CwUnitEstimator.cs`
  has changed since: 57 lines were added between `687aab1a~1` and HEAD. Say where the check
  now goes, and whether the reading it refuses is still computed the same way.
- The following types are in `tests\Hamlet.RadioEngine.Tests\Cw`:
  - `WhereTheWordsBreakTests`, which is unit 413's trace;
  - `WhatTheStrayLettersRestOnTests`;
  - `WhatTheNeighborsSayTests`;
  - `TheBenchmarkIsKeyedTests`.
- `cw-2026-09-23-173723` and `cw-2026-09-24-004133` each carry a `.key.md` with a scored
  region.
- The keyed totals at HEAD are:
  - all keyed: **165 edits over 565**, against inferred keys;
  - added letters: **17**, of which 8 are single-element;
  - captures 51 of 51, adjudicated 13 of 13, keyed floors 13 of 13.
- The launcher writes to `PHASE_OUTCOME.md`, `PHASE_STATUS.md`, `RUN_LEDGER.md` and
  `WORK_INSTRUCTIONS.md` at the root, and to `.run-unit` files. The reload will show them as
  modified. Task 0 commits the root files with its record as they stand, and says so.

**Expected failures.** Report each of these and repair none of them:
- the dispatcher-loop losses on the app carry-forward line, each re-run alone;
- `AHeldPitchDoesNotOutliveItsEvidenceTests` at 1 of 4. It was red at unit 430's entry, and
  it is P40.

## 6. Rulings in force - do not re-argue

`PHASE_PLAN.md` R59 to R74, with §3 and §6.

**3.2's four tests are the keep rule.** A change is kept only if all four hold:
1. The total edit count over all keyed recordings **falls** below 165 over 565, against
   inferred keys.
2. No named floor from 2.2 is broken.
3. The three adjudicated readings are unchanged character for character, or changed to
   exactly their own adjudicated text (R66). A reading that moves is printed before and after.
4. No capture row's above-bar named count falls (R71, raw span 13.0), except as R73 allows.

**R73:** a character the inferred key aligns as added, inside a scored stretch of a keyed
recording, may leave a floor, and nowhere else. The 29 unkeyed rows keep their floors as they
are. **Every character a change removes is listed by name in the report, with its recording.**

**Other rulings:**
- **R66:** an adjudicated reading may move onto its own adjudicated text.
- **R69:** the strays are 3.6.
- **R72:** no word, dictionary or callsign prior, in any form.
- **R63:** the fourteen captures of 2026-09-24 are the benchmark, and the older rows are not
  retired.
- **R61:** no key is invented, and no scored region is changed.
- **§0.0:** no decode is called what was sent.
- **§0.2:** nothing that keys or transmits is touched.
- **§12.5:** a fixture built from the same misunderstanding as the code proves nothing. No
  synthetic case is the sole evidence for keeping a change (1.4).
- **HM-DEC-091:** a change that reads one recording and costs another is not a fix.
- Also in force: **HM-DEC-155**, **HM-DEC-165**, **FACT-004** and **FACT-006**.

**The author's decisions, overrulable, recorded as decisions and not as rulings:**

- **How R73 reads for a join.** A join removes fragments and leaves a letter, so the
  alignment decides which characters "left". On a keyed recording, the above-bar named count
  may fall only if all three of these hold:
  - the whole fall is inside scored stretches;
  - the fall is no larger than the fall in key-aligned added characters there;
  - no key-aligned right character is lost anywhere.

  Outside scored stretches, nothing may fall. On an unkeyed row, nothing may fall at all.
  This is stricter than counting characters one by one, and a change that needs a looser
  reading fails.
- **A keyed floor under 2.2 moved by R73 is lowered in the judging commit.** Each removed
  character is named beside the floor, as unit 425 was told.
- **3.6 ticks only when a kept change removes at least one of the 8 single-element added
  letters.** The criterion's words also need the total to fall and the added letters to be
  reported before and after. A kept change that lowers the total but leaves all 8 in place
  meets 3.2 and is reported, but it is not 3.6.
- **Only one attempt at G1, plus one narrower variant.** The variant is allowed only if the
  first attempt fails on a single row or test and task 1's trace says why. There is no third
  attempt.
- **3.4's count.** Units 425 and 428 were step 3 units that kept nothing. If this unit keeps
  nothing, it is the third, and task 3 closes 3.4.

## 7. Status cadence

As the header says.

---

## 8. The tasks

### Task 0 - the record

- Add `## UNIT 431 - STEP 3` to `PHASE_OUTCOME.md`, from the decision block at the foot of
  this file.
- `PHASE_STATUS.md` names unit 431 and `CURRENT_STEP: 3`.
- Patch-bump `Directory.Build.props` from 1.13.117 to 1.13.118.
- Park unit 430's section 4 items 1 to 4 verbatim in `docs\phase-correctness\PARKED.md` as P39
  to P42.

**Entry round.** Run each of these:
- both carry-forward lines;
- the three floor tests, with captures at 51;
- the keyed totals, per recording and all together;
- the added letters, split into single-element and not;
- `WhatTheStrayLettersRestOnTests`.

Record these as the numbers to hold.

**Drop candidate:** none.

### Task 1 - which strays a split made (3.6)

Build nothing in this task. Add one member to `WhereTheWordsBreakTests` or
`WhatTheStrayLettersRestOnTests`, whichever holds the pieces. It **asserts nothing**. For each
of the 17 added letters on the keyed recordings, it prints:
- the character, its recording, its settle time and its element count;
- the `MeasureGaps` verdict in force when it was read: held, separated or textbook;
- for a held reading, its character and word gaps, and whether the character gap stands at or
  past the word gap.

It also prints, per keyed recording, the characters around each such reading, so a reader
can see the split.

**State one count:** how many of the 8 single-element added letters were read under a held
reading with the character gap at or past the word gap. **If that count is 0, G1 does not
reach 3.6's strays.** In that case, build nothing in task 2, say so, and go to task 3.

**Drop candidate:** the neighbors print, if the count is plain without it.

### Task 2 - G1 again, judged under R71 and R73 (3.6)

Re-apply `687aab1a`'s check to `CwUnitEstimator.MeasureGaps` **in its own commit**. Adapt it
only as far as the tree has moved since, and say how. Do not touch any of these:
- the word-boundary trough;
- `90840b1f`'s path boundary;
- the relabel;
- the gates;
- `CharacterMargin`, `StrayElementSpan` or the span bar;
- the tracker or the mixdown.

Then run each of these, one type per invocation:
- the keyed totals, per recording, with 17:37's edits over its scored region;
- the added letters, single-element and not;
- the keyed floors;
- adjudicated;
- captures: all 51 rows, each with its old, above-bar and below-bar counts and its elements.

Judge the change under section 6's four tests, with R73 read for a join as section 6 states.
Print each test as a number. **For every row whose above-bar count falls, list every
character that left.** Give its recording and time, and say what the key aligned it as
before: added, wrong or right. Say whether the fall sits wholly inside a scored stretch.

**If the change fails any test, take it back out in the next commit** and say which test
failed.

**One narrower variant is allowed**, under section 6's condition. It gets its own commit, is
judged the same way, and is taken back out the same way.

**If a change is kept:**
- Lower each keyed floor it moved under R73 in the judging commit, naming each character.
- Add its row to `docs\phase-correctness\baseline.md`'s running total, with 17:37 before and
  after (3.3).
- Tick 3.6 in `PHASE_PLAN.md` only under section 6's condition.

**Drop candidate:** the narrower variant.

### Task 3 - 3.4, only if nothing was kept

If task 2 kept a change, skip this task and say so.

Otherwise, write a `P43` in `docs\phase-correctness\PARKED.md` titled *step 3 closes
partial*. It gives:
- the three step 3 units with no kept change: 425, 428 and 431;
- what each attempted, and the test that failed it or the trace that stopped it;
- the numbers step 3 leaves standing: 165 over 565 against inferred keys, 17:37's edits over
  its scored region, and 17 added letters with 8 single-element;
- task 1's count, and task 2's four tests as numbers.

Then tick 3.4 in `PHASE_PLAN.md`. Set step 3 to `partial` in `PHASE_STATUS.md`, marked closed
under 3.4. 3.6 stays unticked.

**Drop candidate:** none.

### Task 4 - the exit round

Run each of these:
- both carry-forward lines;
- the three floor tests, with captures at 51;
- the keyed totals and the added letters;
- `WhatTheStrayLettersRestOnTests`.

Then check two things against the tree:
- none of the eleven transmit files differs from `7e209cb4`;
- `data` is unchanged from entry.

If nothing was kept, `src` is also unchanged from entry.

**Drop candidate:** none.

---

## 9. Parked - do not touch, do not raise

- **7.4 and the mixdown** (P39). It is step 7's next unit, not this one.
- **`AHeldPitchDoesNotOutliveItsEvidenceTests`** (P40). Report its count and nothing more.
- **7.1, 7.2 and 7.8. Step 4. 6.5.**
- **The held gaps on `004535`** (P37). If G1 moves `004535`, report it as a finding and do not
  claim it for 7.4.
- **The 2026-09-25 traffic net** (P35). It stays unbanked.
- **P27, P29 to P42.**
- **Any key, scored region or span bar.** These are fixed. A keyed floor moves only under R73,
  as task 2 says.

## 10. What not to do

- **Do not look for another figure that separates strays from right letters.** Units 421,
  425 and 428 closed that route (P36).
- **Do not change more than the one check in `MeasureGaps`.** The trough, the path boundary,
  the relabel, the gates, the bar, the tracker and the mixdown stay as they are.
- **Do not excuse a fall outside a scored stretch, or on an unkeyed row.** R73 does not reach
  either one.
- **Do not lower any floor except a keyed floor moved under R73**, and name each character
  when you do.
- **Do not keep a change on 17:37's figures alone.** The four tests decide.
- **Do not add a word, dictionary or callsign prior** (R72).
- **Do not tick 3.6 and 3.4 together.** A kept change ticks 3.6 or nothing. No kept change
  ticks 3.4.
- **Do not touch what keys or transmits.**
- **Do not halt for a question.** Park it.
- **Write `output.md` before the session ends.**
- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**
- **Report mismatches and repair nothing. American spelling. UTF-8. Use the four headings
  exactly.**

## 11. Committing and pushing

Commit after each task. The change and any revert each go in their own commit. Push at the end
and say whether the push succeeded.

---

## 12. Reporting

Write `output.md` at the root, with the four headings exactly as section 1 gives them.

```
READ IN THIS ORDER.

A. Hamlet reads a CQ call correctly. Steps 0 to 2 done; 3 partial with 3.4
   and 3.6 open; 4 not started; 5 the owner's; 6 partial with 6.5 open;
   7 partial with 7.1, 7.2, 7.4, 7.6 and 7.8 open.
B. Step 3, criterion 3.6: G1 - the out-of-order held gap reading refused -
   re-judged under 3.2's four tests as R71 and R73 now read them; kept or
   taken back out, which test decided it, the added letters and the 8
   single-element ones before and after; and, if nothing was kept, 3.4
   closing step 3 partial.
C. The rest, weighed against A and B. Section 4 raises <n> items; say
   whether any stands in the way of 3.6.
```

```
UNIT:       431 - <complete|stopped> at task N of 4, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   yes | no - <why, on the line>
NUMBER:     keyed 165 -> <n> over 565, inferred keys; 17:37 <before> -> <after> over its scored region; added letters 17 -> <n>, single-element 8 -> <n>
DRIFT:      <0 if a criterion moved, else 1>
```

**Section 3 leads with the four tests as a table.** Give each test, the number before, the
number after, pass or fail, and whether the change was kept. **Then give 17:37's reading
before and after**, beside its key, so the owner can see whether `WB6RED` came back. **Then
list every character that left**, by recording, with how the key aligned it. **Then give task
1's count**, which is how many of the 8 single-element added letters a split made.

---

```
ARBITER-DECISION
STEP: 3
APPROACH: re-apply G1 - MeasureGaps refusing a held gap reading whose character gap stands past its word gap, so split letters join - and judge it under 3.2's four tests as R71's span bar and R73's key-aligned added exemption now read them, after tracing which of the 8 single-element added letters a split made
MOVE: work around
WHY: Units 421, 425 and 428 found no figure that separates strays from right letters, so a sixth would be a loop. Unit 413 traced 17:37's strays to split letters and G1 cut edits 217 to 193, going out only on floors of two keyed recordings, and R71 and R73 have since changed exactly those tests. If G1 still fails, this is step 3's third unit with no kept change, and 3.4 closes the step partial.
STATE: partial
DECIDED: author's, overrulable - R73 read for a join: a keyed row's above-bar count may fall only inside scored stretches, by no more than the fall in key-aligned added characters there, and with no key-aligned right character lost; unkeyed rows may not fall at all. Keyed floors moved under R73 are lowered in the judging commit, with each character named. 3.6 ticks only if a kept change removes at least one of the 8 single-element added letters. One narrower variant is allowed. If nothing is kept, this is 3.4's third unit (425, 428, 431), and task 3 parks P43 and ticks 3.4. Unit 430's four items are parked as P39 to P42. Per-type timeouts are the unit's. No self-ruling authorizes work outside the tasks.
LICENCE: PHASE_PLAN.md R69, R71, R73, R66, R64, R65, section 6 and criteria 3.2, 3.3, 3.4 and 3.6; PARKED.md P6, P19 and P36; unit 416's re-apply under R66 as precedent; HM-DEC-091; HM-DEC-155; HM-DEC-165; CLAUDE.md 0.0, 0.2 and 12.5
ACCOMPLISHED: the stray E and T that are pieces of a split letter join back into the letter the sender keyed, so 17:37 reads WB6 where it read W T E E T E, with no real letter lost - or step 3 closes partial with its measurements on record, so the loop stops spending on it
ADVANCES: step 3 criterion 6
END-ARBITER-DECISION
```
