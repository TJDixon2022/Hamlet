# Work instruction 372 - Hamlet opens whole, and the send area is never the thing that leaves

**Step 1 of the hardening phase.** Five tasks. Touches layout only - no decoder, no
modulator, nothing that keys. Measure before you build: most of criterion 1.1 is already
in the tree and task 1 exists to find out exactly how much.

**Status.** `tools/status.sh`, real clock, after every commit and every task.

---

## 0. The project gate

```
SHACK_FACTS.md                                          must exist
src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs     must exist
CoreHMI.sln                                             must not exist
MURC.sln                                                must not exist
root                                                    C:\Source\HamLet
```

**If any of the four is wrong, stop and say so in `output.md` section 4. Write nothing
else.** The refusal text: *This is not Hamlet. Nothing was changed.*

---

## 1. The rules that killed sessions

**HM-DEC-155.** No suite; only this unit's names and `docs\carry-forward-tests.txt` as its
top comment says - **two invocations, one build each**, status written immediately before
each. Never background and poll.

## 2. The tool facts

Apostrophes in quoted heredocs break; doubled backslashes collapse; `;` is refused; `rm` is
refused; `-m` more than once for a multi-line commit. **Write this repository's files as
UTF-8; a PowerShell `>` redirect writes UTF-16 and the launcher cannot read it.**

**Corrected from work instruction 371's list, on unit 371's measurement (its section 4 item
6):** *Python runs here.* Scripts written to the scratchpad and run as `python file.py`
worked throughout units 361, 362 and 371. The old line said it could not. Use it if it
helps; it is not needed for this unit.

## 3. Asks still outstanding

Carried per HM-DEC-139, **verbatim in section 4**: unit 371's six items, then unit 369's
five, which 371 carried unanswered. **Two of them are answered by §6 below and come off the
queue when you carry it** - 369's item 1 and 371's item 1. Carry the rest as they stand and
answer none of them.

---

## 4. Why this unit exists

```
PHASE GOAL: Hamlet holds what it has - the hardening phase, five steps of banked
            screen, record and test work that needs neither the radio nor the owner.
UNIT GOAL:  Hamlet opens whole and stays whole as the window shrinks: the working
            panels give up height first, then the top row, and the send area is
            never the thing that leaves - asserted by measuring, not described.
ADVANCES:   step 1, criteria 1.2 and 1.3, and 1.1 and 1.4 confirmed by measurement.
DRIFT:      none.
```

**The count today.** Step 0 `done` under §6's first ruling below. Step 1 `not started`,
**0 units spent**. Steps 2, 3, 4 and 5 `not started`. Steps 1-5 are one pipeline
(`PHASE_PLAN.md` §5), so this unit is the one that gets the pipeline moving.

**R34 - Tim, 2026-09-14.** At 1100×780, the size Hamlet opens at, CQ was drawn at y 800 and
the mode tabs at y 847, below a window 780 tall: a new operator opened Hamlet and could not
see the button that calls CQ (units 354, 355). Eight of the nine sizes were unaffected;
that one was not.

**What is already there, and it is most of 1.1.** Unit 356 capped `TopRow` at
`MaxHeight="300"` and wrote the rule into `MainWindow.axaml` as a comment, and
`TheStopIsAlwaysOnScreenTests` holds two nine-size assertions:
`AtEachOf354sNineSizesStopIsInTheStatusBarAndOnTheWindow` and
`AtEachOfTheNineSizesTheSendAreaIsWholeOnTheWindow`, the second asserting
`DigitalSendCqButton`, `DigitalModeChipStrip`, `DigitalStopButton` and
`DigitalTransmitDriveNote` whole on the window at each size, and the send area's drawn
height equal at all nine.

**So what is left is the part the tree states and does not assert.** The rule is written in
a comment at `src\Hamlet.App\Views\MainWindow.axaml` line 2946 - *the working panels give up
height first, then the top row, and the send area is never the thing that leaves* - and a
comment is not a test. **Nothing measures the order in which height is surrendered, and
nothing measures what happens under 900×620 at all.** That is 1.2's first half and the whole
of 1.3, and it is this unit's work.

---

## 5. Verify this instruction against the tree

Every line below was read at authoring time. **Report every mismatch in section 4 and
section 1; repair nothing but this unit's.**

- `src\Hamlet.App\Views\MainWindow.axaml` line 12: `Width="1100" Height="780"`; line 13:
  `MinWidth="900" MinHeight="620"`.
- Line 2658: the root grid, `RowDefinitions="Auto,*,Auto"`, header pinned, status bar
  pinned (HM-DEC-051).
- Line 2896: `RowDefinitions="Auto,Auto,Auto,Auto,*"` inside root row 1.
- Line 2957: `x:Name="TopRow"`, `MaxHeight="300"`, unit 356's cap; the rule stated in the
  comment above it.
- Line 3385: `<Grid Grid.Row="4" RowDefinitions="Auto,*">` - the tab row carries the send
  area (`ModeTabs` 3394, `DigitalSendReserved` 3464, `DigitalSendCqButton` 3480).
- `SendPanel` 3745 and `ReceivePanel` 3818; `DigitalTransmitDriveNote` 3104;
  `DigitalStopButton` 6640, in the status bar since unit 355.
- **No `ScrollViewer` stands on root row 1.** Every scroller in the file is inside a panel
  or a card. If that is wrong, say so - 1.3 turns on it.
- `tests\Hamlet.App.Tests\Views\TheStopIsAlwaysOnScreenTests.cs`, 499 lines, the two
  nine-size names above plus `WithNothingKeyedItSaysStopAndIsStillPressable`.
- The nine sizes, from `TheTopRowTests.Unit354TraceTheMainWindowAtTheSizesTimCanOpen`:
  **1920×1040, 900×620, 1100×780, 1280×720, 1366×728, 1536×824, 1400×1040, 1920×1017,
  2560×1400**.
- Helpers you will want rather than rewrite: `TheTopRowTests.Realized`,
  `TheTopRowTests.Named<T>`, `TheTopRowTests.RectIn`, `TheWorkingPanelsTests.Realized`,
  `TheWorkingPanelsTests.Panels`, `TheWorkingPanelsTests.Placement`.
- **1.4's "the sheet's layout tests"** are the layout tests `docs\unit349-what-tim-looks-at.md`
  cites as its sources: **`TheTopRowTests`, `TheWorkingPanelsTests` and
  `TheStopIsAlwaysOnScreenTests`**. That reading is the author's and is in §6; if the sheet
  names others you find, run those too and say so.

**Expected failures, so you can tell them from yours.** `docs\carry-forward-tests.txt`'s
known-red block: `CwAdjudicationTests.ASpeedChangeInRealisticAudio`, the 51 CW cases in
`docs\unit239-failing-set.txt`, the `Ft8Sharp.Deep.Tests` tripwire, HM-OPEN-088's ten, the
two `TheAchievementsScreenTests` names, `TheFitGuardAsksAboutTheGridTheSendIsOnTests`
(engine, not app), and unit 320's item 46. **None of those is yours.**

**Two things the reload measured that are not yours to repair, and say so in section 1:**
`PROJECT_STATUS.md` `RULES_AT` reads `HM-DEC-165 (2026-09-19)` while `CLAUDE.md` §1 holds
`CPS-DEC-0165` - the id-scheme split, carried in `PHASE_PLAN.md` §7. And at authoring time
`WORK_INSTRUCTIONS.md` and `output.md` were **deleted** in the working tree and
`RUN_LEDGER.md` modified, all uncommitted, against HEAD `723142cb`.

## 6. Rulings in force

Transcribed in full. **Do not re-argue any of these.**

**The first is mine, and it unblocks this step.** Step 1's entry is *step 0 done*, and step 0
stood `partial` on criterion 0.1 alone. **0.1 is met.** Unit 369 searched 119 commits between
1.13.30 (`681d45c8`) and 1.13.48 (`ec4b466e`), found that not one names a file under
`src\Hamlet.App\Settings\`, and measured `git diff` over the settings model, its loader,
`SettingsViewModel.cs`, `SettingsWindow.axaml` and `App.axaml.cs` as **empty** across that
window. The criterion's purpose is to know what broke it before trusting the fix, and the
mechanism is named, reproduced and repaired - `LoadFrom` answered every exception with
`return new AppSettings()` and `App.axaml.cs` saved those defaults over his file on exit.
0.1's wording presupposed a commit in a window the measurement shows contains none, and a
criterion that cannot be satisfied by a complete and correct search is a wording fault, not
an unmet criterion. **A negative, fully searched and named, satisfies 0.1.** *Author's,
overrulable* - this answers unit 369's section 4 item 1. It is not one of `PHASE_PLAN.md`
§6's three stops: it touches no keying, no transmit, no money, and no fact the product
states to the operator.

**The second is mine, and it parks a question.** Unit 371's section 4 item 1 asks whether the
CQ filter should hide a PSK31 row addressed to somebody else. **R9 stands unchanged.** A
PSK31 row has no addressee until a turnover has been read; hiding by latest parse reopens
unit 337's fault, where a carrier emitted 262 characters with no turnover and was held off
the list. It is also outside this phase. *Author's, overrulable; logged, not chased*
(ARBITER.md §2). **Do not change the filter in this unit.**

**§R34 - Tim, 2026-09-14: Hamlet opens with every control on the window.** At 1100×780, the
size it opens at, CQ and the mode tabs were below the window (units 354, 355). The send area
never leaves the window; the panels give up height first, then the top row.

**§R11 - nothing at the radio.** *No unit asks the operator to set a level, read a meter, or
know what ALC is.*

**§R12 - Tim, 2026-09-11: a session fixes its own tests and never asks the owner to approve
it.** A test a session wrote while a door was shut, that later blocks the unit told to open
the door, is **the session's to rewrite in its own commit** so it guards the rule and not the
shut door - and that is not a ruling, not an ask, and not a stop. **The arbiter never puts
the wording of a test to the owner.**

**§R13 - telemetry is a must-pass on every remaining step.** Every stage a step adds writes an
event that lets a person diagnose that stage from the file alone, proved by assertion against
a fixture, with nothing personal in it (HM-DEC-018 §2.1). **Step 1 adds no stage and therefore
no event** - if you find yourself adding one, you have left the criterion.

**§R14 - Tim, 2026-09-11: eyes on the prize.** *"We don't focus too much on pointless testing.
We remember what the phase goal is."* A test exists to prove an exit criterion. A unit writes
the tests its criteria need and no others; it does not add guards for doors it is not
building, pins against changes it is not making, or tests of a test.

**§R19 - American spelling** in every operator-facing string and every instruction.

**§R31 - this phase runs unattended.** Progress is counted in criteria by id. A done step is
closed. The owner's step ends the run. **Two rulings per unit at most.** A question about
layout, wording, a number or a mechanism is the arbiter's to answer, mark and continue - and
under §R32's precedent, a layout number is never a stop.

**§0.0** a sentence on the screen is a claim; a refusal says the true reason. **§0.2** one
click, one transmission - **nothing in this unit sends anything**. **§0.5** hiding detail is
allowed; hiding information is not - a panel that scrolls hides neither. **§0.6** a word
survives grayscale where punctuation does not.

**HM-DEC-051** the header and the status bar are pinned and everything else scrolls.
**HM-DEC-155**, **HM-DEC-139**, **HM-DEC-165** (no name green before a unit is red after it),
**FACT-004** (nothing here is evidence about the radio; every appearance claim is computed,
not seen), **FACT-006**.

## 7. Status cadence

`tools/status.sh`, real clock, **after every task and every commit**, and immediately before
each of the two carry-forward invocations. Never compose a timestamp.

---

## 8. The tasks

### Task 0 - the record

Append `UNIT 372 - STEP 1` to `PHASE_OUTCOME.md` with `ADVANCED: step 1`. Patch-bump
**1.13.58 → 1.13.59** in `Directory.Build.props` with its line in the version log. **Run the
carry-forward list, both invocations, before anything changes**, and write the two counts in
the outcome entry's `ENTRY:` line. A red here is not yours; name it and go on.

**Drop candidate:** none.

### Task 1 - the trace: measure what the tree already does

**This task builds nothing.** `Unit372TraceTheWindowAsItShrinks`, app, a trace in
`TheStopIsAlwaysOnScreenTests` or its own file, printing and asserting nothing about new
behavior:

1. **At the nine sizes**, print for each: the window box; `TopRow`, `DigitalSendReserved`,
   `ModeTabs`, `DigitalSendCqButton`, `DigitalTransmitDriveNote`, `DigitalStopButton` boxes;
   and each of `TheWorkingPanelsTests.Panels(window)`. Say for each control whether it is
   whole on the window.
2. **Then a descending sweep at width 1100**: heights **780, 740, 700, 660, 620, 580, 540,
   500**. The last three are below `MinHeight`; realize them anyway - this is a headless
   window and nothing stops you, and if something does, say so, that is a finding. Print the
   same boxes at each height.
3. **Run `TheStopIsAlwaysOnScreenTests` three times** and record whether the count was the
   same each time. **This is evidence for step 2's criterion 2.3, which names it a flake. It
   is not step 2's work and you do not fix it here** (§R14) - you record what you saw, so the
   unit that does step 2 starts from a measurement rather than a rumor. If it flakes while
   you are working, say which name and how often, and re-run rather than chase it.

**What this task must answer in the report, by id:** whether **1.1** is already met by the
tree as it stands, with the numbers; and what the sweep shows about **1.2** and **1.3** -
specifically, at which height each thing first shrinks, and what happens to the send area and
the panels below 620.

**Drop candidate:** none. If the unit runs long, everything else goes before this does.

### Task 2 - criterion 1.2: the order in which height is given up

**The rule, stated, and this is the tree's and unit 356's, not new:** *when the window is too
short, the working panels give up height first, then the top row; the send area's height is
constant across sizes and it is never the thing that leaves.*

`TheWindowGivesUpHeightInOneOrderTests`, app, **watched failing first** against the tree at
task 0's commit - copy it into a worktree there and record which of its names were red and
why. Assert, by measuring at task 1's descending sweep from 780 down to 620:

- **The send area's drawn height is the same at every height in the sweep**, within a pixel.
  It is the number that says *never the thing that leaves*.
- **The working panels lose height before `TopRow` loses any.** Between the first two heights
  at which anything shrinks, the panels' total is strictly smaller and `TopRow` is unchanged.
- **`TopRow` gives up its share only after the panels have**, and never exceeds its 300 px
  cap at any height in the sweep.
- **At every height in the sweep, `DigitalSendCqButton`, `ModeTabs`, `DigitalSendReserved`,
  `DigitalTransmitDriveNote` and `DigitalStopButton` are whole on the window.**

**If the measurement says the tree already holds all four, the test still gets written and
committed** - 1.2 says *the rule is stated and holds*, and a rule that holds with nothing
asserting it is the state R34 was ruled about. Say plainly in the report that no source file
changed, as unit 371 did for its task 2.

**If it does not hold, fix the layout, not the test.** Never loosen an assertion
(`PHASE_PLAN.md` §6). Any test that asserted the old order is yours to rewrite under **§R12,
in its own commit**.

**Drop candidate:** none.

### Task 3 - criterion 1.3: below the sum of the minimums

**Below the sum of the minimums, the panels scroll inside themselves and the send area stays
put.** At 1100×580, 1100×540 and 1100×500 - and at 900×620, the smallest size Hamlet will
open at - assert:

- **The send area is whole on the window and the same height it is at 780.** It does not
  shrink and it does not go off the bottom.
- **The working panels scroll inside themselves**: each has a scroller whose extent exceeds
  its viewport, and scrolling it to the end shows its last content. `ThePanelScrollsTests`
  and `TheDecodedPanelScrollsItselfTests` are the shape to copy; read them before writing.
- **Nothing is hidden, only scrolled** (§0.5). A panel that has collapsed to nothing with its
  content unreachable fails this.
- **The status bar and the header stay pinned** (HM-DEC-051).

`TheWindowHoldsBelowItsMinimumTests`, app, **watched failing first** the same way.

**This is the task most likely to need a layout change**, and the change belongs on root row
1's contents or on the panels, **never on the send area's row**. If a change to the send area
looks like the only way, **stop the task, do not make it, and write the reason in section 4**
- the send area leaving is the exact thing R34 forbids, and choosing to let it leave would be
a change to what the product promises the operator.

**Drop candidate: this task's repair, not its measurement.** If the measurement shows a
change is needed and it cannot be made without moving the send area or without a package,
**keep the measuring test, mark 1.3 `partial` with the numbers in section 3, and go to task
4.** A measured `partial` with the cause named is worth more than an unmeasured criterion.

### Task 4 - criterion 1.4, the list, and the report

- **`BindingHealthTests` green**, whole type.
- **The sheet's layout tests green**: `TheTopRowTests`, `TheWorkingPanelsTests`,
  `TheStopIsAlwaysOnScreenTests`, each with its count in the report.
- **The carry-forward list, both invocations, after the last change.** Compare name by name
  against task 0's run. **A red after that was green before is a regression** and section 1
  and section 4 both name it as one (HM-DEC-165).
- **`docs\carry-forward-tests.txt`:** add **nothing** unless this unit wrote a test that
  guards a rule the whole product depends on, and if you do, add it **by type and method**
  with a paragraph saying which unit added it and what it costs in seconds. The app
  invocation already runs about 38 s. **The default is to add nothing** - say which you chose
  and why.
- **`PHASE_STATUS.md` and `PHASE_OUTCOME.md`:** step 0 `done`, step 1 to whatever your
  measurements support, criterion by criterion by id (§R31). Do not round up.
- Write `output.md` per §12.

**Drop candidate: task 1's three repeat runs of `TheStopIsAlwaysOnScreenTests`** (task 1 item
3). It is step 2's evidence, not step 1's criterion. If the unit is running long, drop it and
say in section 4 that step 2 still has no flake measurement.

---

## 9. Parked - do not touch, do not raise

- **Steps 2, 3, 4 and 5.** The RSID codes, the sub-mode on `cq_pressed`, the flake repair,
  the visibility events, the radio sheet. **Not this unit**, however close the flake feels.
- **The CQ filter and R9.** Ruled in §6. Do not change it, do not re-argue it.
- **The archived Olivia phase** at `docs\phase-olivia-run\`. A phase is never reopened
  (`PHASE_CONTROL.md` §6); 369's item 4 stays on the queue as a carried ask.
- **`LearnedAlcReference.Ago()`** and its minutes. 369's item 3, outside this phase.
- **Anything about what keys. Any package.** A package is `MOVE: stop` (`PHASE_PLAN.md` §6).
- **The `RULES_AT` id-scheme split.** Report it, carry it, do not repair it.

## 10. What not to do

- **No unfiltered `dotnet test`** (HM-DEC-155). Two invocations, one build each. **Never
  background and poll. Never compose a timestamp.**
- **Do not loosen a test to make a criterion pass.** `PHASE_PLAN.md` §6: ship, report,
  `partial`, move on.
- **Do not let the send area shrink, move off the window, or give up height** to make
  anything else fit. That is the fault R34 was ruled about.
- **Do not add an event.** Step 1 adds no stage (§R13).
- **Do not write a test for a door you are not building** (§R14). No pins, no tests of tests.
- **Do not delete a file.** Empty it, comment it, list it (`PHASE_PLAN.md` §6).
- **Report mismatches; repair nothing but this unit's. Write American. Write files as UTF-8.**

## 11. Committing and pushing

**One commit per task**, on `main`, message naming the unit and the task. **§R12 rewrites go
in their own commit**, separate from the change that made them necessary. Push once, at the
end, after task 4's carry-forward run is green or its reds are named.

---

## 12. Reporting

`output.md` at the root. **Canonical headings:** `## 1. What Claude did`, `## 2. What the
owner should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

**The ordering block first. `validate-output.bat` refuses a report without it.** Fill every
line from what you measured - a line that is the same every unit is furniture.

```
READ IN THIS ORDER.

A. The phase goal - Hamlet holds what it has. Step 0 done (0.1 ruled met on the
   negative, work instruction 372 section 6, author's). Step 1 <state>. Steps 2,
   3, 4, 5 not started.
B. Step 1 - Hamlet opens whole. 1.1 <met|not> - already in the tree / by this
   unit. 1.2 <met|not>. 1.3 <met|partial|not>. 1.4 <met|not>. All four must-pass.
C. The report last. Section 4 raises <N> items on top of the carried eleven, and
   <none of them is | item <k> is> in the way of a criterion in B - say which.
```

```
UNIT:       372 - <complete|stopped> at task N of 5 - <date time>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   step 1, criteria <the ids you actually moved>
NUMBER:     the heights at which Hamlet was measured: nine sizes -> nine sizes
            plus a sweep from 780 to 500
DRIFT:      none
```

**Section 3 must lead with the measurement table from task 1** - the descending sweep, one
row per height, columns for `TopRow`, the working panels' total, `DigitalSendReserved` and
whether each of the five controls was whole on the window. **That table is the evidence for
1.2 and 1.3 and it comes before any prose about them.** Then the test counts, then the
carry-forward counts before and after.

**Section 2 tells Tim in one paragraph what happens now when he drags the window small** -
what stays, what scrolls, and what he will never lose. **Every appearance claim is computed,
not seen** (FACT-004).

**Section 4:** your own items first, most-blocking first, each saying plainly whether it
wants a ruling or is a finding - a note is not a ruling request. Then the carried queue
verbatim per HM-DEC-139, **minus 369's item 1 and 371's item 1, both answered in §6**, with
one line saying so.

---

```
ARBITER-DECISION
STEP: 1
APPROACH: shrink the window below the minimums and assert by measuring that the working panels give up height first, then the top row, and the panels scroll inside themselves while the send area stays put
MOVE: continue
WHY: step 1 has zero units spent and its rule is stated only in an axaml comment - unit 356 capped the top row and asserted the nine sizes, so 1.1 is largely banked, but nothing measures the order height is surrendered in and nothing measures the window below 900x620 at all.
STATE: not started
DECIDED: author's, overrulable, two. (1) Criterion 0.1 is met by unit 369's completed negative - 119 commits searched, an empty diff over every settings path, the mechanism named and repaired - so step 0 is done and step 1's entry is open; the criterion's wording presupposed a commit the measurement shows does not exist, which is a wording fault and not an unmet criterion. This answers unit 369's section 4 item 1. (2) R9 stands: the CQ filter keeps showing a PSK31 row addressed to another station, because a PSK31 row has no addressee until a turnover has been read and hiding by latest parse reopens unit 337's fault. This answers unit 371's section 4 item 1 and is logged, not chased. Also settled as the author's: 1.4's "the sheet's layout tests" reads as TheTopRowTests, TheWorkingPanelsTests and TheStopIsAlwaysOnScreenTests.
LICENCE: PHASE_PLAN.md R34, R31 and section 6 - a layout number is the arbiter's to decide and never a stop; PSK31 plan R11, R12, R13, R14, R19; CLAUDE.md 0.0, 0.2, 0.5, 0.6; HM-DEC-051, HM-DEC-139, HM-DEC-155, HM-DEC-165; FACT-004
ACCOMPLISHED: Hamlet stays whole when the window is made small - the panels shrink and then scroll inside themselves, the top row gives up its share next, and the button that calls CQ and the area around it are still on the screen at every size Tim can drag the window to, including sizes smaller than Hamlet will open at.
ADVANCES: step 1, criteria 1.2 and 1.3 - the two nothing in the tree asserts - with 1.1 and 1.4 confirmed by measurement rather than assumed. It also clears step 1's entry condition, which was blocked on step 0 standing partial.
END-ARBITER-DECISION
```
