# Work instruction 282 - the composed paragraph, and the log shows his contacts

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  MURC.sln

If all four are not as stated, you are in the wrong repository.
REFUSE. Do not read the rest of this file, do not summarise it, do not
adapt it to whatever project you are actually in, and change nothing.
Reply with only: the path you are in, which checks failed, and
"wrong project - nothing done."

If all four hold, say "Hamlet confirmed" and continue.
```

---

## THE TWO RULES THAT KILLED SESSIONS

**Tim's rulings of 2026-09-05, HM-DEC-155. Not this unit's to weigh.**

**1. A unit runs no test suite.** **A unit may run only the unit test it
constructs or rewrites in that work instruction**, filtered by exact name, in the
foreground, with a stated timeout of a few minutes. **An unfiltered `dotnet test`
on any project is forbidden.**

**2. Never background a command and poll for it.** Sessions were killed by the
watchdog sitting in `until grep -q "exited with code" ...; do sleep 15; done`
against a twelve-minute watchdog.

`dotnet build` is allowed, foregrounded, with a timeout.

---

## THE TOOL RULE

**This session's shell may refuse calls.** A refused shell call is a signal to
reach for the other tool, not to stop. Record every refusal verbatim. **Nothing in
this unit halts the loop.**

**Nine units have paid for these two tool facts:** this shell will not carry a
quoted heredoc containing an apostrophe, and it collapses a doubled backslash
inside one. Use script files and the file-editing tools.

---

## `ADVANCED` and `DRIFT`

**Every bench step of this phase is closed.** Steps D and E are Tim at his own
radio. **Every unit from here reports `ADVANCED: no` by construction.**

```
DRIFT:  4 consecutive units without advance, carried from unit 281.
```

---

## Asks still outstanding

Carried inbound per HM-DEC-139.

1. **Two issues of one work-instruction number.** Unit 271, and 252 before it.
   **The author's error.** No unit action.
2. **`PM95` reads *southern Japan***, unit 271. **Not a defect.**
3. **`HM-OPEN-083` and `HM-OPEN-084`.** In `OPEN_ISSUES.md` by HM-DEC-140.
4. **Three pixels.** **Waiting on Tim.**
5. **`dt` and `hz` were never on the mine list.** **Waiting on Tim.**
6. **Where an outcome entry goes when the unit it corrects has none.** **Tim's.**
7. **`PHASE_OUTCOME.md` is written by a tool no unit is told to run.** Named in
   task 8; **the general question is the author's.**
8. **Where the repeat fold stops**, unit 277. **Still open.**
9. **Whether a faded row needs a second carrier of its meaning**, unit 279.
   **Tim's.**
10. **Whether counting subjects is a new assertion**, unit 279. **Tim's.**
11. **Whether the fade is still obvious now the row is a bubble**, unit 280. **Not
    measured.**
12. **`HM-OPEN-087`** - the CW tab's *Have a look* button runs
    `OpenReceiveHelpCommand`, which expands a panel that is not in the tree, and
    **thirteen of fifteen `widget.*` templates are referenced by nothing.** Raised
    by unit 281. **Whether to rehome, delete or fold the advice is a ruling and is
    Tim's.** **Task 4 touches the button only, not the templates.**

---

## Why this unit exists

**Three units have swept for permanently-visible text and walked past the
paragraph the operator keeps pointing at.** It is still on his status bar, full
width, with his contact count beside it.

**It is not a string anybody can grep for.** It is composed at runtime from parts
whose only literal text is XML doc comments, so **a source search returns empty and
looks exactly like a clean sweep.** Located by the operator, 2026-09-08:

```
src/Hamlet.App/ViewModels/MainWindowViewModel.cs:8179      puts it on screen
src/Hamlet.RadioEngine/Explore/OwnedSettings.cs:65         builds part of it
src/Hamlet.RadioEngine/Explore/ReceiverConditions.cs:102   builds part of it
```

**Unit 281 built the instrument that would have caught it** -
`HowMuchTheApplicationSaysTests` walks realized windows and measures what is
actually on them. **It measured the status bar honestly all along. Nobody asked it
to hold a ceiling.**

**And the contact log shows the wrong grid.** The window has a `my grid` column
carrying `FN00DJ` correctly and **no column for the station's grid at all**, though
`AdifLog` writes both `MY_GRIDSQUARE` and `GRIDSQUARE`. `sent` reads `not recorded`
on four of five records and `rcvd` on one, and the columns truncate mid-word -
`not recorc`.

```
PHASE GOAL:   Hamlet works stations on the air.
UNIT GOAL:    No surface can grow a paragraph again without a test failing, and
              his log shows who he worked and where they were.
ADVANCES:     nothing. Every bench step is closed and this unit says so plainly.
```

---

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim and report mismatches.
Report them; do not repair the instruction.

- **The three file and line numbers above** were found by the operator with
  `Get-ChildItem src -Recurse -Include *.cs | Select-String`. **Confirm them before
  building; they may have moved.**
- **`HowMuchTheApplicationSaysTests`** in `tests/Hamlet.App.Tests/Views/` walks all
  ten screens and measured **14,670 -> 7,665 characters** across the application.
  **Settings is 47 per cent of the total and 92 per cent of it is teaching prose.**
- **`AdifLog` writes `GRIDSQUARE` and `MY_GRIDSQUARE`** as separate fields, built by
  unit 274 from ADIF Specification 3.1.4.
- **Unit 274 reads the report sent from the ledger's `Sent` and the report received
  from `HeardToUs`**, and a field Hamlet did not observe is **absent** from the
  record rather than empty.
- **Unit 279 recorded `FACT-006`**: this machine has no contact log. **Everything
  below is built against synthesised logs and that is correct.** His real log holds
  **5 records** as of 2026-09-08.
- Root version after unit 281 was **1.12.181**. **Read it, do not assume.**

Known reds, inherited, **never chased**:
`CwAdjudicationTests.ASpeedChangeInRealisticAudio`; the 51 CW cases in
`docs/unit239-failing-set.txt`; the `Ft8Sharp.Deep.Tests` whole-type-list
tripwire.

---

## Rulings in force

**Tim's, 2026-09-08:**

- **Text only where he intentionally hovers.** App-wide. **A fault speaks unasked**
  and is the only exception.

**Standing:**

- **§0.0 and HM-DEC-092.** **Removing words must not remove a fact.** Advice moved
  to hover is preserved; advice deleted is a fact lost.
- **§0.6.** Colour is never the only carrier of meaning.
- **§12.5.** A fixture built from the same assumption as the code proves nothing
  about the code. **A test that searches source for a string composed at runtime is
  that fault exactly**, and it is why three sweeps missed this.
- **One click, one transmission.** Nothing here transmits, arms or cancels.
- **HM-DEC-012 and §0.5.** Family colour is text colour only.
- **§0.1.** The engine is not told that tabs exist.
- **`Ft8Sharp` is a faithful MIT port and nothing changes a line of it.**

---

## Status cadence

After each task, before starting the next, update `PROJECT_STATUS.md` per
`CLAUDE.md` - `STATE`, `TASK: n of m`, `BALL`, `UPDATED` from the clock, and
`NOTE` saying what is moving inside the task. The same every ten minutes while a
task is running. **Use the file-editing tools if the shell refuses.**

---

## Tasks

### Task 1 - the composed paragraph, at the three lines that build it

**This is the goal task, and the lines are given so no session has to guess
again.**

- **`MainWindowViewModel.cs:8179`** puts it on the status bar. **The advice moves
  behind the tips icon**, with the whole text on hover.
- **`OwnedSettings.cs:65` and `ReceiverConditions.cs:102`** compose it. **Nothing
  they say is deleted** - every sentence still exists, one hover away.
- **The status bar keeps the count, the belt and glyphs.** Nothing else.
- **A fault still speaks unasked**: a clock that is wrong, a device that is not
  there, a slot that refused.
- **Confirm the three lines first** and say what you found if they have moved.

### Task 2 - a ceiling nothing can quietly exceed

**This is the fix that outlives the paragraph.**

- **`HowMuchTheApplicationSaysTests` gains a ceiling per surface**, asserted, so a
  surface that grows a paragraph again turns a test red.
- **The ceiling is measured off the realized window**, which is what that harness
  already does, **and never from source.** A source search for a string composed at
  runtime returns empty and reads as clean - that is §12.5 and it is why three
  sweeps missed this one.
- **Set each ceiling from what the surface holds after task 1**, with a stated
  margin, and say what margin you chose and why.
- **Settings is 47 per cent of the application and is not this unit's to sweep.**
  Give it a ceiling at its current size so it cannot grow, and **say plainly that
  it is capped where it stands rather than reduced.**
- Test, watched failing first: adding a sentence to a capped surface turns it red.

### Task 3 - the log shows the station's grid

- **A column for the station's grid**, from `GRIDSQUARE`, beside the existing
  `my grid` from `MY_GRIDSQUARE`. **Both are written; only one is shown.**
- **Label them so neither is mistaken for the other.**
- **`not recorded` still means absent**, not blank and not guessed.

### Task 4 - why `sent` is empty

- **Four of five real records carry no report sent, and one carries no report
  received.** Find out whether the sends were genuinely not recorded, or **recorded
  and not reaching the log entry.**
- **Report which, with file and line.** These are two different faults and only one
  of them is fixable here.
- **If it is the second, fix it.** If it is the first, **say exactly what is not
  recorded and where it would have to be recorded**, and leave it.
- **Do not back-fill or edit an existing record.** A log record is a statement the
  operator made and is not a unit's to revise.
- **Also**: the CW tab's *Have a look* button runs `OpenReceiveHelpCommand` and
  expands a panel that is not in the tree, so **pressing it does nothing while
  looking exactly like a button that works** (§0.5.1, HM-DEC-087). **Make it not
  look like a working button.** **Do not touch the thirteen unreferenced
  `widget.*` templates** - whether to rehome, delete or fold them is `HM-OPEN-087`
  and Tim's.

### Task 5 - the log window stops truncating

- **`not recorc`** is a column cut mid-word. **Size the columns to their content or
  let them wrap**; do not clip.
- **Report the widths before and after**, and **if something still does not fit,
  say so** rather than shrinking another column to hide it.

### Task 6 - the sender tooltip and the grid it cannot find

**Carried from unit 281's tasks 5 and 6. Confirm whether they landed before
building.**

- **A message he sent gets no sender tooltip.** He knows who sent it.
- **`United States of America` reads long** where every other entity reads short.
- **The tooltip claimed Hamlet needs his grid square while `FN00DJ` sits verified
  in Settings.** If that is still true, **find why the distance calculation cannot
  reach it, with file and line** - and say whether it is the same shape as the menu
  on the wrong list and the Log item gated where it could never fire.

### Task 7 - what was removed, listed and measured

**Named drop candidate.**

- **Every string this unit moved or deleted**, with where it went, appended to the
  removal log units 280 and 281 keep.
- **If a fact was lost rather than a sentence shortened, say so under its own
  heading.**
- **Measure the whole application again** against unit 281's 7,665.

### Task 8 - the outcome entry

- **Append this unit's entry to `PHASE_OUTCOME.md`** through
  `tools\arbiter\outcome-append.bat`. **If the shell refuses, append with the
  file-editing tools in the format the existing entries use** and say so.
- **Do not back-fill units 273, 274 or 275.**

---

## Parked - do not touch, do not raise

- **The thirteen unreferenced `widget.*` templates.** `HM-OPEN-087`, Tim's.
- **Settings' 6,845 characters.** Capped by task 2, not swept.
- **The three pixels**, **`dt` and `hz` on the mine list**, **where the repeat fold
  stops**, **whether the fade needs a glyph**, **whether the fade reads on a
  bubble**. All Tim's.
- **FT4.** On hold.
- **Editing or deleting a log record**, awards beyond the count, uploading
  anywhere.
- **The send path, the abort, the composer, the waiting-stations row.**
- **Anything in `src/Ft8Sharp/`.**
- The OSD re-encoding count, `ReusableWindow`, `ProcessDelayForTests`, the tap's
  owner, the waterfall's first row, unit 237's Extensible conclusion, work
  instruction 231's four tree items, `validate-output.bat`'s permitted-spellings
  bug, the 101.33 ms pulse above 6 kHz, the CW decoder and its inherited reds.

---

## What not to do

- **Do not delete a sentence.** Move it to hover.
- **Do not hide a fault behind a hover.**
- **Do not search source to prove a surface is clean.** Measure the window.
- **Do not sweep Settings.** Cap it.
- **Do not edit or back-fill a log record.**
- **Do not touch the unreferenced widget templates.**
- **Do not clip a column.** Fit it, wrap it, or report it.
- **Do not touch `src/Ft8Sharp/`.**
- **Do not run a test suite.** Only the tests you write or rewrite here, filtered,
  foregrounded, with a timeout.
- **Do not background a command and poll for it.**
- **Do not ship a placeholder token in a reported number.**
- **Do not report `ADVANCED: yes`.**

---

## Committing and pushing

Commit and push each task before starting the next. Bump the root version's patch
by one from whatever the tree carries. **`Ft8Sharp` does not move.**

---

## Reporting

`output.md` at the repository root, overwritten, four sections per
`CLAUDE_CODE.md` §8 with the header block above section 1.

**NUMBER: the status bar's rendered characters, before and after** - the surface
this unit exists for.

**Section 3 leads with four things:**

1. **The status bar as it now renders**, and the paragraph's whole text quoted from
   its hover.
2. **The ceilings**, per surface, with the margin chosen and why - and the red a
   capped surface gives when a sentence is added.
3. **A log row with both grids**, quoted, and the truncation gone.
4. **Why `sent` was empty** - not recorded, or recorded and not reaching the entry.

**Section 2 says what he will see change on his screen**, in his own terms: the
paragraph is gone from the bar and his log says who he worked and where they were.

**Carry the asks queue outbound, and carry `DRIFT` forward.**

Write `output.md`, then stop. Do not start the next unit.
