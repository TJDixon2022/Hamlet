# Work instruction 373 - the panels keep a floor and scroll inside themselves, and the last red comes off 1.4

**Step 1 of the hardening phase, second unit.** Five tasks. Touches layout and one test
only - no decoder, no modulator, nothing that keys. **Unit 372 measured this step and did
not repair it**; this unit repairs what it measured. Measure first anyway: task 1 builds
nothing.

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

**Corrected on unit 372's measurement, and these three come off the carried queue because
they are answered here rather than carried:**

- **`git worktree add` is refused**, inside the repository root and outside it (372's item
  5). **No task in this unit asks for a worktree.** Where a test must be watched failing on
  an earlier tree, do it the way unit 372 did: write the test, run it, and prove the
  application was unchanged at that moment with `git diff <task 0 commit> -- src/` reading
  empty. Say in the report which commit you diffed against.
- **Shell output redirection (`>`) is refused to every path**, including the scratchpad
  (372's item 6). Write files with the editor, not with a heredoc or a redirect.
- **A compound command with `;` or a second operation is refused** (372's item 6). A loop
  goes into a script file first. **Python runs here** - `python file.py` from the
  scratchpad worked in units 361, 362 and 371. It is not needed for this unit.

## 3. Asks still outstanding

Carried per HM-DEC-139, **verbatim in section 4**. Unit 372 raised seven. **Two are answered
by §6 below** - its items 1 and 2 - and **three are absorbed into §2 and §5 of this
instruction** - its items 3, 5 and 6. Those five come off the queue when you carry it, with
one line saying so. **372's items 4 and 7 stay**, and so do unit 371's five and unit 369's
four: **eleven carried, and this unit answers none of them.**

---

## 4. Why this unit exists

```
PHASE GOAL: Hamlet holds what it has - the hardening phase, five steps of banked
            screen, record and test work that needs neither the radio nor the owner.
UNIT GOAL:  Finish step 1. The three working panels never draw zero: below the sum
            of the minimums they keep a measured floor and scroll inside themselves,
            the send area still never moves, and 1.4's last red comes off.
ADVANCES:   step 1, criteria 1.3 and 1.4 - the two that are open, both must-pass.
DRIFT:      none.
```

**The count today.** Step 0 `done`. **Step 1 `partial`, 2 units spent** - 1.1 met and
measured at all nine sizes, 1.2 met with four green names and no source file changed, **1.3
partial** and **1.4 not met**. Steps 2, 3, 4 and 5 `not started`, 0 units each. Steps 1-5
are one pipeline (`PHASE_PLAN.md` §5): **nothing downstream starts until 1.3 and 1.4 close.**

**What 1.3 is, and what the tree does instead.** *Below the sum of the minimums, the panels
scroll inside themselves and the send area stays put.* Unit 372 measured that **all three
working panels draw 0 px at 900x620 and at every height from 700 down**, with their content
still in them and unreachable - the decoded scroller reads **extent 36 in a viewport of 0**,
For You **extent 360 in a viewport of 0**. That is hiding information rather than detail,
which §0.5 forbids. `TheWindowHoldsBelowItsMinimumTests` is **committed red** on exactly that
name and was not loosened. It is the one thing standing between step 1 and done.

**What 1.4 is, and what is in its way.** `BindingHealthTests` 1 of 1, `TheWorkingPanelsTests`
8 of 8, `TheStopIsAlwaysOnScreenTests` 5 of 5 - and **`TheTopRowTests` 14 of 15**, failing
alone at `TheBestBetPillAndTheGreenBlockNameTheSameBandOnTheWindow`, unrelated to height and
byte-identical to unit 371's tip. Unit 372 correctly left it alone, because its §5 said repair
nothing but its own. **§6 below rules that it is this unit's**, and says why.

---

## 5. Verify this instruction against the tree

Every line below was read at authoring time. **Report every mismatch in section 4 and
section 1; repair nothing but this unit's.**

**The layout, and this is where the work is.**

- `src\Hamlet.App\Views\MainWindow.axaml` line 12: `Width="1100" Height="780"`; line 13:
  `MinWidth="900" MinHeight="620"`.
- Line **2658**: the root grid, `RowDefinitions="Auto,*,Auto"`, header pinned, status bar
  pinned (HM-DEC-051).
- Line **2896**: `<Grid Grid.Row="1" RowDefinitions="Auto,Auto,Auto,Auto,*">` inside root
  row 1.
- Line **2956**: `<Grid Grid.Row="1" x:Name="TopRow"`, with `MaxHeight="300"` at line
  **2958** - unit 356's cap, the rule stated in the comment above it. **372's §5 said 2957;
  it is 2956, and that correction is taken.** `TopRowCardScroller` is already inside it at
  row 0, column 0, so the top row's card already scrolls within the cap.
- Line **3385**: `<Grid Grid.Row="4" RowDefinitions="Auto,*">`. **Read this one carefully -
  the whole repair turns on it.** Its row 0 is the tab row and **carries the send area**
  (`ModeTabs` 3394, `DigitalSendReserved` 3464). Its row 1 is `WorkspaceBoundary`.
- Line **3728**: `<Border Grid.Row="1" x:Name="WorkspaceBoundary"`, `BorderThickness="1"`,
  `Padding="12"` - the 26 px unit 372 measured - wrapping a `<Panel>` that holds the mode
  workspaces. **The send area is not inside it. It is a sibling, one row above.**
- The three panels unit 372 measured: `DigitalWaterfallPanel` **4340**,
  `DigitalDecodedPanel` **4517**, `DigitalMinePanel` **5199**, all inside
  `WorkspaceBoundary`. `DigitalReadinessStrip` **4197**.
- `DigitalTransmitDriveNote` 3104; `DigitalStopButton` 6640, in the status bar since 355.
- **No `ScrollViewer` stands on root row 1** - unit 372 confirmed it. Every scroller in the
  file is inside a panel, a card, or `TopRow`. If that is wrong, say so.

**The numbers unit 372 measured, which you may start from and must re-measure.**

- At 1100x620 the workspace region is given **51 px** and needs **138 px** before the panels
  get their first pixel: **26 px** of `WorkspaceBoundary`'s border and padding, **112 px** of
  the mode strip and the row beneath it. **The deficit is 87 px.**
- At width 1100 the panel row's height is **window height minus 707** - 71 at 780, 31 at 740,
  **0 at 700 and below**.
- `TopRow` draws **300 px at every height from 780 to 620** and never passes its cap. The
  radio's own face inside it measures **110 px**.
- At the nine sizes the panel row drew **450, 0, 71, 50, 86, 228, 424, 427 and 827**. The
  zero is 900x620.
- **`MinHeight` binds on a headless window** (372's item 3): 1100x580, 540 and 500 all draw
  as **1100x620**, so they are one size reached three ways, and 620 is the worst case Tim can
  reach by dragging. Expect this; it is not a finding again.

**The tests, and their state.**

- `tests\Hamlet.App.Tests\Views\TheWindowHoldsBelowItsMinimumTests.cs` - **2 of 3, committed
  red** on the panels-draw-zero name. **This is the criterion. Turn it green by changing the
  layout, never by changing the assertion.**
- `TheWindowGivesUpHeightInOneOrderTests.cs` - **4 of 4, and it is 1.2's guard.** It asserts
  the send area is 22 px at every height 780 to 620, that the panels give up all the height
  while `TopRow` gives up none, and that `TopRow` never passes 300. **It must still be 4 of 4
  when you are done, unmodified.** It is how we know the repair did not reorder the surrender.
- `Unit372TraceTests.cs` - the sweep, asserts nothing, not on the list. Extend or copy it.
- Helpers rather than rewrites: `TheTopRowTests.Realized`, `TheTopRowTests.Named<T>`,
  `TheTopRowTests.RectIn`, `TheWorkingPanelsTests.Realized`, `TheWorkingPanelsTests.Panels`
  (waterfall, decoded, For You), `TheWorkingPanelsTests.Placement`.
- The nine sizes: **1920x1040, 900x620, 1100x780, 1280x720, 1366x728, 1536x824, 1400x1040,
  1920x1017, 2560x1400**.
- **1.4's "the sheet's layout tests"** are `TheTopRowTests`, `TheWorkingPanelsTests` and
  `TheStopIsAlwaysOnScreenTests` - unit 372's author's reading, unchanged and not re-argued.

**The red on 1.4, located.**

- `tests\Hamlet.App.Tests\Views\TheTopRowTests.cs` line **828**,
  `TheBestBetPillAndTheGreenBlockNameTheSameBandOnTheWindow`. It loops widths 1920 and 1400,
  and inside each loops `bestName` over **"20 m"** then **"40 m"**, setting `IsBestBet` on
  the matching band by hand. At line **854** it collects every visible `TextBlock` whose text
  is **exactly `"best bet now"`**, and at line **867** asserts `Assert.Single(badged)`.
- It failed with *`Assert.Single()` Failure: The collection was empty*, printing *at 1920.0
  best bet 40 m: pills wearing the badge []* - so it passed for 20 m and failed for 40 m.
- **What the tree says the badge wears.** `MainWindow.axaml` line **3313** is the badge
  `Border`, `IsVisible="{Binding IsBestBet}"`, and line **3318** is its
  `TextBlock Text="{Binding BestBetLabel}"`. `MainWindowViewModel.cs` line **19275** sets
  `button.BestBetLabel = ranking.BadgeLabel`, and `BandOpportunity.cs` line **240** is
  `BadgeLabel => FromObservation ? "best bet now" : "likely, going on the hour"`, with the
  comment above line 3316 saying *the label travels with the ranking, so a clock guess can
  never wear the same words as an observation* (**HM-DEC-046**).
- **The hypothesis, and you must confirm or refute it by measuring, not adopt it.** If the
  fixture's 40 m band is a clock guess rather than an observation, its badge is visible and
  wearing *likely, going on the hour*, the test's text filter misses it, and **the test is
  asserting one of two ruled labels instead of the rule**. If instead **no** badge is visible
  for 40 m at all, that is a defect in the application and the application is what you fix.
  Task 1 decides which, in print, before task 3 changes anything.

**The version.** `Directory.Build.props` line **931**: `<Version>1.13.59</Version>`.

**Expected failures, so you can tell them from yours.** `docs\carry-forward-tests.txt`'s
known-red block: `CwAdjudicationTests.ASpeedChangeInRealisticAudio`, the 51 CW cases in
`docs\unit239-failing-set.txt`, the `Ft8Sharp.Deep.Tests` whole-type-list tripwire,
HM-OPEN-088's ten, `TheAchievementsScreenTests.WsprIsNotAFirstAnybodyCanEarnAndTheCardSaysSo`,
`TheAchievementsScreenTests.TheWindowDrawsEverySixRows`,
`TheFitGuardAsksAboutTheGridTheSendIsOnTests` (engine, not app), and unit 320's item 46.
**None of those is yours.** `TheWindowHoldsBelowItsMinimumTests`'s one red **is** yours and
is the point of task 2.

**Known flake, and it is not yours to fix** (§R14, and it is step 2's criterion 2.3):
`TheStopIsAlwaysOnScreenTests.KeyedAtTheOpeningSizeAClickOnTheBarFiresTheAbortWhileItRuns`
was red in 3 of 7 runs for unit 372, always the same way - the abort pair on the wire twice.
The numbers are in `docs\unit372-flake-measurement.md`. **If it flakes at you, re-run it, say
so and how often, and do not chase it.**

**Two things the reload measured that are not yours to repair, and say so in section 1:**
`PROJECT_STATUS.md` `RULES_AT` reads `HM-DEC-165 (2026-09-19)` while `CLAUDE.md` §1 holds
`CPS-DEC-0165` - the id-scheme split, carried in `PHASE_PLAN.md` §7. And at authoring time
`PHASE_OUTCOME.md`, `PHASE_STATUS.md` and `RUN_LEDGER.md` were **modified and uncommitted**
against HEAD `9a6ba2de`, and `.run-unit\unit372-flake.sh` and `.run-unit\unit372-validate.sh`
were untracked.

## 6. Rulings in force

Transcribed in full. **Do not re-argue any of these.**

**The first is mine, and it is the repair 1.3 has been waiting for.** Unit 372's section 4
item 1 asks for a ruling: *criterion 1.3 cannot be met without redesigning how root row 1
allocates height, and that is past a layout number.* **`PHASE_PLAN.md` §6 says a layout and a
mechanism are the arbiter's to decide, marked author's, and never a stop**, so here is the
decision.

**The panel canvas gets a measured minimum height and a scroller of its own, and the pool it
draws on is its own overflow - not the top row's 300 px.** Concretely: the floor and the
scroller belong **inside `WorkspaceBoundary` (line 3728), around the `<Panel>` it wraps, or
on the row that holds it** - and **never** on the tab row above it, which carries the send
area. Unit 372 rejected *a `ScrollViewer` on root row 1* because it would enclose the send
area, and that rejection was right; **this is one grid lower**, at `Grid.Row="4"`'s row 1,
where the send area is a **sibling** in row 0 and cannot be enclosed. Unit 372 did not
measure that site. That is why this is a work-around and not a repetition.

**Three things this ruling fixes, and they are the whole of it.**

1. **The panels never draw zero.** Each of the three keeps a viewport big enough that its own
   scroller works - `extent > viewport > 0` - so its content is scrolled rather than hidden
   (§0.5).
2. **The order of surrender does not change.** `TopRow` keeps its `MaxHeight="300"` and is not
   touched. The panels still give up height first and the top row second, which is 1.2, and
   `TheWindowGivesUpHeightInOneOrderTests` stays 4 of 4 unmodified as the proof of it.
3. **The send area is not in the scroller and does not move.** It stays 22 px at every height,
   in its own row, above the boundary.

**The cost, stated, because it is real.** A floor means that at short windows the canvas is
taller than the room it has, so **the canvas scrolls** - and that is a visible change at
sizes that are not broken today, including **1280x720 and 1366x728**, where the panel row
draws 50 and 86 px. *That is accepted.* A 50 px panel is not a working panel; the five
controls 1.1 is about do not move; and HM-DEC-051 is the ruling that says the header and the
status bar are pinned **and everything else scrolls**. **Rejected**, and do not revisit:
capping `TopRow` off a window-height converter - it buys too little and misfires across
widths, as 372 measured; and taking height from the send area's row in any form, which is
exactly what R34 forbids. **The floor's number is yours to measure and to state**, not to
choose from the air: the smallest floor at which all three panels report a working scroller
at 900x620, with 372's 138 px of chrome as the starting arithmetic. *Author's, overrulable.*
It is none of `PHASE_PLAN.md` §6's three stops: no keying, no transmit, no money, no package,
and no fact the product states to the operator.

**The second is mine, and it makes 1.4's red this unit's work.** Unit 372's section 4 item 2
reports `TheTopRowTests.TheBestBetPillAndTheGreenBlockNameTheSameBandOnTheWindow` red and
inherited, and left it under *repair nothing but this unit's*. **That clause is about work a
unit stumbles over, and it does not exempt a must-pass criterion of the step the unit is
on.** 1.4 says the sheet's layout tests are green; this name is one of them; step 1 cannot
close while it is red, and **no other unit is coming for it** - it has survived units 371 and
372 already. **So it is this unit's, and it is task 3.** How it is repaired is not a ruling at
all: **if the cause is that the test asserts the badge's particular words rather than the
rule, rewriting it is §R12 work** - the session's own, in its own commit, guarding *one pill
wears the badge and its words match its ranking* rather than guarding one of the two labels
HM-DEC-046 allows. **If the cause is that no badge is drawn, the application is the thing
that is wrong and the application is what you fix.** *Author's, overrulable.* **The one guard:
if the only available repair would change which band Hamlet names as best bet, or change what
the badge says to the operator, do not make it** - write the numbers in section 4, leave the
test red, and report 1.4 not met. That is a fact the product states about the radio and it is
the owner's (`PHASE_PLAN.md` §6).

**That is two rulings, which is §R31's limit.** Unit 372's item 7 - whether `PHASE_PLAN.md`'s
criterion boxes are meant to be the record - **stays on the carried queue, unanswered**, and
§9 parks it. Both rulings this unit gets are spent on must-pass criteria.

**§R34 - Tim, 2026-09-14: Hamlet opens with every control on the window.** At 1100x780, the
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
click, one transmission - **nothing in this unit sends anything**, and `GreenZoneBestBet`
moves the operator's band, so it is read and never clicked. **§0.5** hiding detail is allowed;
hiding information is not - a panel that scrolls hides neither, **and a panel drawn 0 px tall
with content in it hides information**. **§0.6** a word survives grayscale where punctuation
does not.

**HM-DEC-046** the badge's label travels with the ranking: an observation and a clock guess
never wear the same words. **HM-DEC-051** the header and the status bar are pinned and
everything else scrolls. **HM-DEC-155**, **HM-DEC-139**, **HM-DEC-165** (no name green before
a unit is red after it), **FACT-004** (nothing here is evidence about the radio; every
appearance claim is computed, not seen), **FACT-006**.

## 7. Status cadence

`tools/status.sh`, real clock, **after every task and every commit**, and immediately before
each of the two carry-forward invocations. Never compose a timestamp.

---

## 8. The tasks

### Task 0 - the record

Append `UNIT 373 - STEP 1` to `PHASE_OUTCOME.md` with `ADVANCED: step 1`. Patch-bump
**1.13.59 -> 1.13.60** in `Directory.Build.props` with its line in the version log. **Run the
carry-forward list, both invocations, before anything changes**, and write the two counts in
the outcome entry's `ENTRY:` line. Unit 372 left it at **app 206 of 206, engine 146 of 146**;
if yours differs, that is a finding and it goes in section 1. A red here is not yours; name it
and go on.

**Drop candidate:** none.

### Task 1 - the trace: measure both repairs before you make either

**This task builds nothing and changes no source file.** One trace type, or an extension of
`Unit372TraceTests`, printing and asserting nothing about new behavior.

**1. The panel canvas, at three widths.** Unit 372 swept width 1100 only, and its stated
reason for calling the repair a redesign was that *the rest of the canvas varies with width as
well as height*. **Measure that claim.** At widths **900, 1100 and 1920**, heights **780, 740,
700, 660 and 620**, print for each: the window box; `WorkspaceBoundary`'s box; the box of the
`Panel` inside it; each of `TheWorkingPanelsTests.Panels(window)`; and for the decoded panel
and For You, **their scroller's extent and viewport**. Then print, per width, **how many
pixels are consumed between `WorkspaceBoundary`'s outer edge and the first panel pixel** -
372 measured 138 at width 1100 and it is the number the floor is built on. **Say plainly
whether that chrome is constant across the three widths or varies, and by how much.** That
single answer decides how the floor is written.

**2. The best-bet red, printed rather than guessed.** At 1920 and 1400, for both `bestName`
values the failing test uses, print for **every** band: its name, `IsBestBet`,
`BestBetLabel`, and whether its badge `Border` is `IsEffectivelyVisible`. **Answer in the
report, in one sentence: is a badge drawn for 40 m, and what words is it wearing?** That
decides whether task 3 rewrites the test under §R12 or repairs the application. **Nothing is
pressed** (§0.2).

**What this task must answer in the report, by id:** for **1.3**, the chrome-per-width number
and the smallest floor that gives all three panels a working scroller at 900x620; for **1.4**,
the one sentence about the 40 m badge.

**Drop candidate:** none. If the unit runs long, everything else goes before this does - a
measured cause handed to the next unit is worth more than an unmeasured repair.

### Task 2 - criterion 1.3: the floor and the scroller

**The criterion:** *below the sum of the minimums, the panels scroll inside themselves and the
send area stays put.* Build it at the site §6 names - **inside `WorkspaceBoundary` (3728), on
the `Panel` it wraps or the row that holds it, never on the tab row at 3385 row 0.**

Turn `TheWindowHoldsBelowItsMinimumTests`'s red name green **by changing the layout**, and add
what the criterion needs if it is not already asserted there, at **900x620** and at **1100x620**
(which is where 580, 540 and 500 all land):

- **Each of the three panels has a scroller with `viewport > 0` and `extent > viewport` where
  its content is longer, and scrolling it to the end shows its last content.**
  `ThePanelScrollsTests` and `TheDecodedPanelScrollsItselfTests` are the shape to copy; read
  them before writing.
- **The send area is whole on the window and the same height it is at 780** - 22 px. It does
  not shrink, it does not move, and it is not inside any scroller you add.
- **Nothing is hidden, only scrolled** (§0.5). A panel collapsed to nothing with content in it
  fails this, and that is the exact red you inherited.
- **The header and the status bar stay pinned** (HM-DEC-051).

**Then re-run `TheWindowGivesUpHeightInOneOrderTests` and it must still be 4 of 4, unmodified.**
If the floor changed the order in which height is surrendered, the floor is wrong, not the
test. **Never loosen an assertion** (`PHASE_PLAN.md` §6).

**Re-run the nine sizes too** (`TheStopIsAlwaysOnScreenTests`, `TheWorkingPanelsTests`) and put
the new panel-row heights beside unit 372's **450, 0, 71, 50, 86, 228, 424, 427, 827** in
section 3. **The change at 1280x720 and 1366x728 is expected and ruled** - report the numbers,
do not apologize for them.

**If a change to the send area's row looks like the only way, stop the task, do not make it,
and write the reason in section 4.** The send area leaving is what R34 forbids.

**Drop candidate:** none. This is the criterion the unit exists for.

### Task 3 - criterion 1.4: the last red

Take task 1's answer about the 40 m badge and act on it, **per §6's second ruling**:

- **If a badge is drawn wearing *likely, going on the hour*:** the test is asserting one of
  two ruled labels instead of the rule. **Rewrite it under §R12, in its own commit**, so it
  asserts what HM-DEC-046 actually says - exactly one pill wears the badge, it is the band
  `GreenZoneBand` names, and its words are the ones its ranking earns. **This is not a
  loosening and say why in the report**: it asserts more than it did, not less.
- **If no badge is drawn for 40 m:** the application is wrong. Fix the application, and leave
  the test exactly as it is.
- **If the only repair would change which band is named or what the badge says to the
  operator:** make nothing, report the numbers, leave it red, 1.4 not met. That one is the
  owner's.

Then run all four of 1.4's types and put each count in the report: **`BindingHealthTests`,
`TheTopRowTests`, `TheWorkingPanelsTests`, `TheStopIsAlwaysOnScreenTests`.**

**Drop candidate: this task.** If the unit is running long, drop it, leave 1.4 not met with
task 1's measurement of the cause in section 4, and say that the next unit starts from a
diagnosis rather than a rumor. **Task 2 is never dropped for this one.**

### Task 4 - the list, the record, and the report

- **The carry-forward list, both invocations, after the last change.** Compare name by name
  against task 0's run. **A red after that was green before is a regression** and section 1
  and section 4 both name it as one (HM-DEC-165).
- **`docs\carry-forward-tests.txt`:** the default is to **add nothing**, and unit 372 chose
  that and said why. **If task 2's floor is the thing that keeps the panels reachable, one
  name may be worth adding** - `TheWindowHoldsBelowItsMinimumTests`, by type and method, with
  a paragraph naming this unit and its cost in seconds. **Say which you chose and why**, and
  **add nothing that is knowingly red.**
- **`PHASE_STATUS.md` and `PHASE_OUTCOME.md`:** step 1 to whatever your measurements support,
  **criterion by criterion by id** (§R31). If 1.3 and 1.4 are both met, step 1 is `done` and
  say so; if either is not, step 1 is `partial` and name which. **Do not round up.**
- Write `output.md` per §12.

**Drop candidate:** none. The record is how the next unit starts.

---

## 9. Parked - do not touch, do not raise

- **Steps 2, 3, 4 and 5.** The RSID codes, the sub-mode on `cq_pressed`, the flake repair, the
  visibility events, the radio sheet. **Not this unit**, however close the flake feels - and it
  will feel close, because `TheStopIsAlwaysOnScreenTests` is on your list and is one of the two
  flakes. Re-run it; do not fix it.
- **`PHASE_PLAN.md`'s criterion checkboxes** (372's item 7). **Do not tick them and do not
  raise it again in this unit** - it is carried verbatim on the queue and §6 says why it gets no
  ruling here. The record this phase maintains is `PHASE_STATUS.md` and `PHASE_OUTCOME.md`.
- **The CQ filter and R9.** Ruled in work instruction 372 §6. Do not change it, do not
  re-argue it.
- **`TopRow`'s 300 px cap.** §6 rules that it is not the pool. Do not cap it, do not bind it,
  do not convert it.
- **The archived Olivia phase** at `docs\phase-olivia-run\`. A phase is never reopened
  (`PHASE_CONTROL.md` §6); 369's item 4 stays a carried ask.
- **`LearnedAlcReference.Ago()`** and its minutes. 369's item 3, outside this phase.
- **Anything about what keys. Any package.** A package is `MOVE: stop` (`PHASE_PLAN.md` §6).
- **The `RULES_AT` id-scheme split.** Report it, carry it, do not repair it.

## 10. What not to do

- **No unfiltered `dotnet test`** (HM-DEC-155). Two invocations, one build each. **Never
  background and poll. Never compose a timestamp.**
- **Do not loosen a test to make a criterion pass.** `PHASE_PLAN.md` §6: ship, report,
  `partial`, move on. `TheWindowHoldsBelowItsMinimumTests`'s red is turned green by the
  layout or not at all.
- **Do not modify `TheWindowGivesUpHeightInOneOrderTests`.** It is 1.2's evidence and your
  proof that the order of surrender survived the floor.
- **Do not let the send area shrink, move off the window, give up height, or end up inside a
  scroller** to make anything else fit. That is the fault R34 was ruled about.
- **Do not add an event.** Step 1 adds no stage (§R13).
- **Do not write a test for a door you are not building** (§R14). No pins, no tests of tests.
- **Do not click `GreenZoneBestBet`.** It moves the operator's band (§0.2). Read it.
- **Do not delete a file.** Empty it, comment it, list it (`PHASE_PLAN.md` §6).
- **Report mismatches; repair nothing but this unit's. Write American. Write files as UTF-8.**

## 11. Committing and pushing

**One commit per task**, on `main`, message naming the unit and the task. **A §R12 rewrite
goes in its own commit**, separate from any change that made it necessary. Push once, at the
end, after task 4's carry-forward run is green or its reds are named.

---

## 12. Reporting

`output.md` at the root. **Canonical headings:** `## 1. What Claude did`, `## 2. What the
owner should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

**The ordering block first. `validate-output.bat` refuses a report without it.** Fill every
line from what you measured - a line that is the same every unit is furniture.

```
READ IN THIS ORDER.

A. The phase goal - Hamlet holds what it has. Step 0 done. Step 1 <state>, two
   units spent, 1.1 and 1.2 banked by unit 372. Steps 2, 3, 4, 5 not started.
B. Step 1 - Hamlet opens whole. 1.3 <met|partial|not> - the panels drew 0 px
   before this unit and <what they draw now> at 900x620. 1.4 <met|not> - the
   best-bet red <off|still there>. Both must-pass; 1.1 and 1.2 carried met and
   re-checked <green|not>.
C. The report last. Section 4 raises <N> items on top of the carried eleven, and
   <none of them is | item <k> is> in the way of a criterion in B - say which,
   and say whether step 1 closes.
```

```
UNIT:       373 - <complete|stopped> at task N of 5 - <date time>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   step 1, criteria <the ids you actually moved>
NUMBER:     the panel row's height at 900x620: 0 px -> <what it is now>
DRIFT:      none
```

**Section 3 must lead with task 1's chrome-per-width table** - widths 900, 1100 and 1920
against heights 780 to 620, with `WorkspaceBoundary`, the panel boxes, and each panel's
extent and viewport - **because that table is what says whether the floor is one number or
three**, and it comes before any prose about 1.3. Then the before-and-after panel-row heights
at the nine sizes beside unit 372's row. Then the badge table from task 1 item 2. Then the
test counts, including `TheWindowGivesUpHeightInOneOrderTests` at 4 of 4. Then the
carry-forward counts before and after.

**Section 2 tells Tim in one paragraph what is different when he drags the window small** -
that the panels stop vanishing, what he scrolls to reach what does not fit, and what still
never moves. Unit 372's section 2 promised him this fault would be fixed; say whether it is.
**Every appearance claim is computed, not seen** (FACT-004).

**Section 4:** your own items first, most-blocking first, each saying plainly whether it wants
a ruling or is a finding - a note is not a ruling request. Then the carried queue verbatim per
HM-DEC-139: **unit 372's items 4 and 7, unit 371's five, unit 369's four - eleven**, with one
line saying that 372's items 1 and 2 were answered in §6 and its items 3, 5 and 6 were
absorbed into this instruction's §2 and §5.

---

```
ARBITER-DECISION
STEP: 1
APPROACH: give the panel canvas inside WorkspaceBoundary a measured minimum height and a scroller of its own, so the three working panels keep a viewport and scroll inside themselves at 620 while the send area stays put in its sibling row
MOVE: work around
WHY: unit 372 measured 1.3 and declined the repair, having rejected a scroller on root row 1 because it would enclose the send area - but the send area is a sibling of the panel canvas one grid lower, at Grid.Row 4 row 0, so a floor and a scroller on WorkspaceBoundary at line 3728 reach the criterion without touching the send area or TopRow's cap, and that site was never measured.
STATE: partial
DECIDED: author's, overrulable, two. (1) Criterion 1.3's repair is a measured minimum height and a scroller on the panel canvas inside WorkspaceBoundary, never on the tab row that carries the send area and never funded from TopRow's 300 px cap; the canvas scrolling at 1280x720 and 1366x728, where the panels draw 50 and 86 px today, is the accepted cost, since a 50 px panel is not a working panel and HM-DEC-051 is the ruling that everything but the header and the status bar scrolls. This answers unit 372's section 4 item 1. (2) The inherited red TheTopRowTests.TheBestBetPillAndTheGreenBlockNameTheSameBandOnTheWindow is this unit's to repair, because repair-nothing-but-your-own does not exempt a must-pass criterion of the step the unit is on and no other unit is coming for it; if the cause is that the test asserts the badge's words rather than HM-DEC-046's rule, the rewrite is R12 work and not a ruling at all, and if the only repair would change which band Hamlet names as best bet, it is made nothing of and reported. This answers unit 372's section 4 item 2. Unit 372's item 7, the plan checkboxes, is left on the carried queue unanswered because R31 allows two rulings a unit and both are spent on must-pass criteria.
LICENCE: PHASE_PLAN.md R34, R31 and section 6 - a layout, a number and a mechanism are the arbiter's and never a stop; PSK31 plan R11, R12, R13, R14, R19; CLAUDE.md 0.0, 0.2, 0.5, 0.6; HM-DEC-046, HM-DEC-051, HM-DEC-139, HM-DEC-155, HM-DEC-165; FACT-004
ACCOMPLISHED: Hamlet's three working panels stop disappearing when the window gets short - at the smallest size Tim can open or drag to, the waterfall, the decoded text and For You are still there at a size he can read, with what does not fit reachable by scrolling instead of gone, while the CQ button and the area around it stay exactly where they have always been.
ADVANCES: step 1, criteria 1.3 and 1.4 - the only two of the four still open, both must-pass, and closing them closes step 1 and opens the entry to step 2, which the remaining pipeline waits on.
END-ARBITER-DECISION
```
