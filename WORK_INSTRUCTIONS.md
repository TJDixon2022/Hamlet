# Work instruction 283 - every mode's status bar, not the one the author named

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

**Ten units have paid for these two tool facts:** this shell will not carry a
quoted heredoc containing an apostrophe, and it collapses a doubled backslash
inside one. Use script files and the file-editing tools.

---

## `ADVANCED` and `DRIFT`

**Every bench step of this phase is closed.** Steps D and E are Tim at his own
radio. **Every unit from here reports `ADVANCED: no` by construction.**

```
DRIFT:  5 consecutive units without advance, carried from unit 282.
```

---

## Asks still outstanding

Carried inbound per HM-DEC-139. **Read unit 282's outbound queue and carry
anything it added; the numbering below is this order's.**

1. **Two issues of one work-instruction number.** Unit 271, and 252 before it.
   **The author's error.** No unit action.
2. **`PM95` reads *southern Japan***, unit 271. **Not a defect.**
3. **`HM-OPEN-083` and `HM-OPEN-084`.** In `OPEN_ISSUES.md` by HM-DEC-140.
4. **Three pixels.** **Waiting on Tim.**
5. **`dt` and `hz` were never on the mine list.** **Waiting on Tim.**
6. **Where an outcome entry goes when the unit it corrects has none.** **Tim's.**
7. **`PHASE_OUTCOME.md` is written by a tool no unit is told to run.** Named in
   task 6; **the general question is the author's.**
8. **Where the repeat fold stops**, unit 277. **Still open.**
9. **Whether a faded row needs a second carrier of its meaning**, unit 279.
   **Tim's.**
10. **Whether counting subjects is a new assertion**, unit 279. **Tim's.**
11. **Whether the fade is still obvious now the row is a bubble**, unit 280.
12. **`HM-OPEN-087`** - thirteen unreferenced `widget.*` templates. Unit 282
    disabled the button pointing at them and touched no template. **Still Tim's.**
13. **Whether a 14-pixel ring at the left of the status bar is findable**, unit
    282. **Not seen by anybody on a real screen** and it now carries the whole
    receive narration.
14. **Whether `AboutWindow` is in scope for the terseness ruling**, unit 281. Unit
    282 capped it and did not sweep.

---

## Why this unit exists

**The paragraph is still on the operator's screen, and unit 282 did exactly what
it was asked.**

**282 measured, capped and cleared the CW tab.** 884 characters to 0, from *the
nine rows the shipped `mode-receiver-conditions.json` states for CW*. **Its own
report says, one line later: `FT8 composes 448 the same way`.**

**He operates on the Digital tab.** The FT8 rows go onto the bar exactly as they
always did.

**The root cause is the author's, and it is the same one twice.** Task 1 of unit
282 named **one** call site, `MainWindowViewModel.cs:8179`. The session found a
**second at 8137**, reported it as a mismatch, and fixed the one it was given.
**Naming a line number instead of a behaviour is how a fix reaches one surface out
of two** - the same error as naming one tab in unit 280 when the ruling was
app-wide.

**So this order names no line numbers.** It names the behaviour and asks for the
count.

**And the earlier misses have a different cause worth keeping in view**: the text
is composed at runtime from parts whose only literal form is XML doc comments, so
**every source sweep returned empty and read as clean.**

```
PHASE GOAL:   Hamlet works stations on the air.
UNIT GOAL:    No mode puts a paragraph on the status bar, and no surface in the
              application can grow one without a test failing.
ADVANCES:     nothing. Every bench step is closed and this unit says so plainly.
```

---

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim and report mismatches.
Report them; do not repair the instruction.

- **Unit 282 reported `MainWindowViewModel.cs:8207`** as the assignment that puts
  the narration on the bar, **and a second at `8137` for mode-follow.** The lines
  the author gave it - `OwnedSettings.cs:65`, `ReceiverConditions.cs:102` - are
  **class declarations, not the composition.**
- **`mode-receiver-conditions.json`** ships the rows. **CW states nine; FT8
  composes 448 characters.** Confirm both figures.
- **`ReceiverSetupVoice.Say`** composes the narration and
  **`ReceiverSetupVoice.Admissions`** filters the same clauses out of the same
  results - **a setting the radio would not confirm, one Hamlet could not read, one
  it cannot reach at all.** **Those go on the bar unhovered, deliberately, because
  they are faults.**
- **`ReceiverConditions.ForMode`** was added by unit 282; before it the only way to
  ask what a mode needs was `ForBlock`, which wants a whole `Neighborhood`.
- **`HowMuchTheApplicationSaysTests`** holds per-surface ceilings. **The CW tab
  measures 611 against 750** and would have measured 1,495 with the paragraph.
  **Find out which surfaces have a ceiling and which do not.**
- Root version after unit 282 was **1.12.189**. **Read it, do not assume.**

Known reds, inherited, **never chased**:
`CwAdjudicationTests.ASpeedChangeInRealisticAudio`; the 51 CW cases in
`docs/unit239-failing-set.txt`; the `Ft8Sharp.Deep.Tests` whole-type-list
tripwire.

---

## Rulings in force

**Tim's, 2026-09-08:**

- **Text only where he intentionally hovers.** App-wide, every screen, **every
  mode.**
- **A fault speaks unasked.** The only exception, and unit 282 built it correctly.

**Standing:**

- **§0.0 and HM-DEC-092.** **Removing words must not remove a fact.** Advice moved
  to hover is preserved; advice deleted is a fact lost.
- **§12.5.** A fixture built from the same assumption as the code proves nothing
  about the code. **A source search for a runtime-composed string is that fault**,
  and it is why three sweeps missed this.
- **One click, one transmission.** Nothing here transmits, arms or cancels.
- **§0.6.** Colour is never the only carrier of meaning.
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

### Task 1 - every path that puts narration on the status bar

**Reading only. Fix nothing in this task. No line numbers are given on purpose.**

- **Find every call site that composes receiver-conditions narration onto the
  status bar**, for **every mode and every tab** - CW, Digital, Voice, and anything
  else that reaches that surface.
- **Report the count**, with file and line for each. **Unit 282 was given one and
  found two. Say how many there actually are.**
- **For each, say which mode or tab it feeds** and how many characters it composes
  from the shipped rows.
- **Say which of them unit 282 already cleared** and which it did not.
- **Do not stop at the ones this order's background section mentions.** The author
  has now been wrong twice about where this text lives.

### Task 2 - every mode's bar, not one

**This is the goal task.**

- **Apply unit 282's change to every path task 1 found.** Advice goes behind the
  hover; **the bar keeps the count, the belt and the glyphs.**
- **A fault still speaks unasked** - a setting the radio would not confirm, one
  Hamlet could not read, one it cannot reach at all. **Unit 282 built that
  correctly through `Admissions` and it is not to be undone.**
- **Nothing is deleted.** Every sentence still exists, one hover away.
- **The Digital tab is the one the operator uses**, and it is where the paragraph
  is today. **If any path resists the change, say which and why** rather than
  clearing the others and reporting success.

### Task 3 - the ceiling covers every surface

- **Every surface in `HowMuchTheApplicationSaysTests` has a ceiling**, not the ones
  a previous order happened to name.
- **The Digital tab's ceiling is set from what it holds after task 2**, with a
  stated margin. Unit 282 chose its margin against the fault rather than the noise
  - a paragraph is 448 to 884 characters, so a margin of 100 cannot hide one.
  **Use the same reasoning and say so.**
- **A surface with no ceiling is a surface this can come back on.** Report any that
  cannot be capped and why.
- Test, watched failing first: adding a sentence to the Digital tab turns it red.

### Task 4 - the same fault, looked for once more

**Named drop candidate, and the reason the last three units each fixed one
instance.**

**Runtime-composed text is invisible to a source search.** Look for the shape
rather than the string:

- **Anything that builds a user-visible sentence from parts** - a `Say`, a
  narration, a description assembled from several sources - **and reaches a
  permanently-visible surface.**
- **Report what you looked at, not only what you found**, as unit 276's
  context-menu sweep and unit 279's finder sweep both did.
- **Do not fix what you find.** Report it. **This unit is already fixing one
  instance and taking on more is how a unit stops finishing.**

### Task 5 - what moved, listed and measured

- **Every string this unit moved**, with where it went, appended to the removal log
  units 280, 281 and 282 keep.
- **If a fact was lost rather than a sentence moved, say so under its own
  heading.**
- **Measure the Digital tab before and after**, and the whole application against
  unit 282's figure.

### Task 6 - the outcome entry

- **Append this unit's entry to `PHASE_OUTCOME.md`** through
  `tools\arbiter\outcome-append.bat`. **If the shell refuses, append with the
  file-editing tools in the format the existing entries use** and say so.
- **Do not back-fill units 273, 274 or 275.**

---

## Parked - do not touch, do not raise

- **The thirteen unreferenced `widget.*` templates.** `HM-OPEN-087`, Tim's.
- **Settings' 6,845 characters** and **`AboutWindow`.** Capped, not swept.
- **The three pixels**, **`dt` and `hz` on the mine list**, **where the repeat fold
  stops**, **whether the fade needs a glyph**, **whether the fade reads on a
  bubble**, **whether the hover ring is findable**. All Tim's.
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

- **Do not fix only the paths this order mentions.** Find them all.
- **Do not delete a sentence.** Move it to hover.
- **Do not undo `Admissions`.** Faults speak unasked.
- **Do not search source to prove a surface is clean.** Measure the window.
- **Do not fix what task 4 finds.** Report it.
- **Do not sweep Settings or `AboutWindow`.**
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

**NUMBER: the Digital tab's rendered characters, before and after** - the surface
the operator actually uses, and the one three units have not touched.

**Section 3 leads with four things:**

1. **How many paths put narration on the status bar**, which mode each feeds, and
   which unit 282 had already cleared.
2. **The Digital tab as it now renders**, and the paragraph quoted from its hover.
3. **The ceilings, per surface**, and which surfaces still have none.
4. **What task 4 found**, and what it looked at.

**Section 2 says what he will see change on his screen**, in his own terms: the
paragraph is off the bar on the tab he actually uses.

**Carry the asks queue outbound, and carry `DRIFT` forward.**

Write `output.md`, then stop. Do not start the next unit.
