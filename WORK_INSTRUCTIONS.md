# Work instruction 376 - the top row comes down to one measured band, and the working panels take every pixel it gives up

**Step 6 of the hardening phase, the first unit aimed at it, and the first aimed at any of
R39's three UI steps.** Five tasks. **Task 1 builds nothing**: the band is measured band by
band before one pixel moves, because 6.1's number has to be proved against a before, and
because which of the top row's two columns is the taller decides which of R39's four moves
is worth making.

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

*All four were checked against the tree at authoring time and all four hold: both files are
present, neither solution exists, and the root is `C:\Source\HamLet`.*

---

## 1. The rules that killed sessions

**HM-DEC-155.** No suite; only this unit's names and `docs\carry-forward-tests.txt` as its
top comment says - **two invocations, one build each**, status written immediately before
each. Never background and poll.

**This unit's own runs are filtered and foregrounded too.** Task 1's measurement runs are
`[AvaloniaFact]` traces in the app project, run by name. Task 3's evidence is four named
types. That is not a suite and it is not a poll.

**A loop goes in a script file** (`;` is refused in a compound command); `.run-unit\unit372-flake.sh`
is the shape if you need one, and this unit probably does not.

## 2. The tool facts

Apostrophes in quoted heredocs break; doubled backslashes collapse; `;` is refused; `rm` is
refused; `-m` more than once for a multi-line commit. **Write this repository's files as
UTF-8; a PowerShell `>` redirect writes UTF-16 and the launcher cannot read it.**

- **`git worktree add` is refused**, inside the repository root and outside it. No task here
  asks for one.
- **Shell output redirection (`>`) is refused to every path**, including the scratchpad.
  Write files with the editor, not with a heredoc or a redirect. **A test run's output is
  read from the console**, or piped to `grep` in the same command.
- **A compound command with `;` or a second operation is refused.** **Python runs here** -
  `python file.py` from the scratchpad worked in units 361, 362, 371 and 374.
- **An `[AvaloniaFact]` trace is how this repository measures a layout**, and
  `TheTopRowTests` already carries the helpers: `Realized(width)`, `Settle(window)`,
  `Measure(window)` and `Named<T>(window, name)`. Use them; do not write a second way to
  measure the same window.

## 3. Asks still outstanding

Carried per HM-DEC-139, **verbatim in section 4**. Unit 375 raised four. **Two of them have
been answered by the owner since**, in `PHASE_PLAN.md` R38 at HEAD `c92bd2e1`:

- **Unit 375's item 2 - the duplicate abort pair - is ANSWERED and comes off the queue.**
  R38 (b): *the transmit sequence's teardown abort pair, sent after the click's own, is
  accepted: a duplicate unkey is the safe direction and nothing keys on it; logged as
  tidy-up, not a stop.* **Nothing in this unit touches it and nothing is to be done about
  it.** Say in section 4 that it is answered and closed.
- **Unit 375's item 1 - criterion 2.2, the RSID tone sequences - was answered by R38 (a),
  and the arbiter has REOPENED it**, because the tree contradicts the fact R38 (a) rests on.
  **Section 9 carries the measurement and section 12 tells you where it goes in your report.
  You do not touch 2.2 and you do not chase it.**
- **Unit 375's items 3 and 4 stay** - the headless dispatcher loop, and
  `TheStopIsAlwaysOnScreenTests.WithNothingKeyedItSaysStopAndIsStillPressable` having gone
  red once in seventeen invocations. Both are findings, neither wants a ruling, and this unit
  answers neither. **If either shows itself in your carry-forward runs, record it with the
  failure verbatim** - unit 375 could not capture that name's message and said so.

**With unit 375's item 2 closed, the carried queue is fifteen:** unit 375's items 3 and 4,
unit 374's item 3, unit 373's item 2, unit 372's items 4 and 7, unit 371's five and unit
369's four. **This unit answers none of them.**

---

## 4. Why this unit exists

```
PHASE GOAL: Hamlet holds what it has - the hardening phase, screen, record and test
            work banked in the PSK31 and Olivia threads that needs neither the radio
            nor the owner, judged by tests that ran and, at the end, by Tim at his
            window.
UNIT GOAL:  The top row stops taking a third of the window. Pills, neighborhood
            strip, green zone and rig display come down to one band of about 180 px
            at 1920 and at 1400, nothing in them is lost - the strip's legend and the
            rule-of-thumb line go to a hover and keep their words - the sun map keeps
            its size, and every pixel given up goes to the three working panels.
ADVANCES:   step 6, criteria 6.1, 6.2, 6.3 and 6.4 - all four must-pass, all four
            untouched by any unit.
DRIFT:      none.
```

**The count today.** Step 0 `partial`, 2 units spent (0.1 met on unit 369's completed
negative, ruled in work instruction 372 section 6; 0.2 to 0.5 met). **Step 1 `done`**, 6
units, closed by unit 374. **Step 2 `partial`**, 2 units: 2.1 met by unit 374, 2.3 met by
unit 375, **2.4 not met** - the best streak was one completed green round of both
invocations against a target of five - and **2.2 not attempted**. Steps 3, 4 and 5 `not
started`, 0 units each. **Steps 6, 7 and 8 are new in `PHASE_PLAN.md` rev 2 of 2026-09-21
and have 0 units each; this is the first unit aimed at any of them.**

**Step 6's entry, checked:** *step 2 done, or partial with 2.1 and 2.3 met.* Step 2 is
partial with 2.1 met and 2.3 met. **The entry is open and it is open by the plan's own
words**, which anticipated exactly this state.

**Why step 6 and not the rest of step 2.** R39, from Tim on 2026-09-21, puts the UI steps
first, and 2.2 is held for the owner for the reason in section 9 - so the reachable work in
step 2 is 2.4 alone, five consecutive green rounds of a list that has to be run again after
2.2 lands in any case. **Step 6 is a whole step nobody has spent an hour on, it is what Tim
sees when he looks at his window in step 5, and it touches no port, no event and nothing
that keys.**

**What 6.1 is worth, in pixels.** At width 1100 the top row draws 300 px - its cap - at every
window height from 1040 down to 620 (unit 374's ladder). At 1920 it draws about 190. The band
is capped at 300 and the panel row at width 1100 is *window height minus 707*, so every pixel
the band gives up is a pixel the three working panels get. **At the size Hamlet opens at, this
is the difference between panels that scroll and panels that do not.**

---

## 5. Verify this instruction against the tree

Every line below was read at authoring time. **Report every mismatch in section 4 and
section 1; repair nothing but this unit's.**

**Where the band is.** `src\Hamlet.App\Views\MainWindow.axaml`.

- **The strip grid** opens at **line 2896**: `<Grid Grid.Row="1" RowDefinitions="Auto,Auto,Auto,Auto,*">`.
  **Its row 0 is the band row - the pills** - and the comment above it, from Tim's ruling of
  2026-08-26, is *from the top it is the Hamlet title, the line of Shakespeare, then the
  bands*. **Task 1 names that element; this instruction does not, because it was not read by
  name.**
- **`TopRow`** at **line 2956**: `ColumnDefinitions="*,Auto"`, `RowDefinitions="Auto,Auto"`,
  **`MaxHeight="300"`**, `Margin="0,0,0,10"`. The comment above it is unit 356's, and it
  states the 300 as measured: *across unit 354's nine sizes this row draws 198, 204, 224, 250,
  278 and 293 px everywhere the layout is sound.*
- **Column 0, row 0: `TopRowCardScroller`** (line 2966), a `ScrollViewer` holding the
  `widget.map` template (line **551**), which is a `CollapsiblePanel` titled *Neighborhood
  map* with a `HintMarkControl` already on its header (line **564**) - **that mark is the
  hover pattern 6.2 asks for, and it is already in the file.** Inside it, `NeighborhoodCardBody`
  (line **577**), two columns:
  - **left:** `NeighborhoodMapControl Height="44"` (the strip itself), then
    **`MapLegendControl`** (line **596**), then **`GreenZoneBlock`** (line **605**) carrying
    `GreenZoneBand`, `GreenZoneFrequency`, `GreenZoneModeLine`, `GreenZoneLicenseLine`,
    **`GreenZoneRuleOfThumb`** (line **713**), `GreenZoneBestBetRow` with `GreenZoneBestBet`,
    `GreenZoneHeardGrid` with `GreenZoneSparkline`, `GreenZoneHeard` and
    `GreenZoneHeardWindow`, and `GreenZoneStrayedLine`.
  - **right:** `GreenZoneMap` (line **938**) holding `GreenZoneGrayLine` and
    `GreenZoneClockCaption` - **this is the sun map, and 6.3 says it keeps its size and its
    dot.**
- **Column 1, row 0:** the amber `Border` holding `ctl:RigDisplayControl` - the rig's own
  face. **Its comment cites HM-DEC-021 (deliberately not collapsible) and HM-DEC-070 (the
  star is inside the black).** Read both before you touch it.
- **Row 1: `RigDriveAndPower`** (line **3079**) - `DigitalTransmitDriveBox`,
  `DigitalTransmitDriveNote`, `DigitalTransmitDriveTip`, `DigitalPsk31PowerLine` and its
  popup. **`DigitalTransmitDriveNote` is one of the five controls criterion 1.1 asserts is on
  the window at all nine sizes. Step 1 is done and closed; do not make it false.**

**How the top row is measured today, and why that is not what 6.1 asks for.**
`TheTopRowTests.Measured` (line **2081**) computes `TopRowTop` as `Math.Min(Card.Top, Rig.Top)`
and `TopRowBottom` as `Math.Max(Card.Bottom, Rig.Bottom)`, from two rectangles: the
neighborhood card and the rig panel. **The band row - the pills - is not in it, and neither
is `RigDriveAndPower` if it falls below both.** Criterion 6.1 names *pills, neighborhood
strip, green zone, rig display*. **Section 6's first ruling settles what is measured and how.**

**What the existing tests assert, so you know what you are about to turn red.**
`tests\Hamlet.App.Tests\Views\TheTopRowTests.cs`, **15 names**, and `TopRowTarget = 190` at
line **45**, read at lines **102**, **728**, **793** and **2545**:

- `AtNineteenTwentyTheTopRowIsAbout190AndTheWorkingCardTakesTheRest` (line 79) - asserts the
  band is within **10% of 190**, so **171 to 209**, and only at widths above 1900; 1400 is
  swept and printed but not asserted. **180 is inside that band.**
- `AtFourteenHundredTheLicensedTopRowIsTheMockupsShare` (583),
  `TheTopRowAndThePanelShareHoldWithTheBestBetPinnedBothWays` (694) and
  `TheTopRowAndThePanelShareHoldWithTheBestBetOnAnotherBand` (760) - **the share between the
  top row and the panels, which this unit changes by design.**
- `TheGreenBlockIsInsideTheCardUnderTheStripWithTheBandLargest` (144) - **the green zone going
  to one line is most likely to land here.**
- `DriveAndThePowerOfferAreUnderTheRigAndTheSendAreaKeepsCqAndStop` (345) - **its name is the
  arrangement R39 would change.** See the drop candidate in task 2.
- `TheWorldClockIsAtTheCardsRightEndWithOneMarker` (260) - **the sun map. This one going red
  means you broke 6.3, not that it needs rewriting.**
- `TheBestBetPillAndTheGreenBlockNameTheSameBandOnTheWindow` (858) - rewritten by unit 373 to
  assert `BandRanking.BadgeLabel`'s rule rather than one of two ruled labels. **Leave it
  asserting the rule.**

**The other three types 6.4 names.** `TheWorkingPanelsTests` **8 names**,
`TheStopIsAlwaysOnScreenTests` **5 names**, `BindingHealthTests` **1 name**. Unit 374 ran all
four together at **33 of 33**.

**Unit 354's nine sizes**, from `TheStopIsAlwaysOnScreenTests` line **61**:
`(1920,1040) (900,620) (1100,780) (1280,720) (1366,728) (1536,824) (1400,1040) (1920,1017)
(2560,1400)`. **The panel row at those nine, from unit 374: 452, 73, 73, 90, 90, 230, 426,
429, 829.** That is 6.4's before, and after this unit every one of those nine should be the
same or larger.

**The carry-forward lists.** `docs\carry-forward-tests.txt` - two command lines at the top,
one per project, then a human-readable list by project and a paragraph per addition.
**`TheStopIsAlwaysOnScreenTests` and `BindingHealthTests.TheMainWindowBindsWithoutOneComplaint`
are on the app line; `TheTopRowTests` and `TheWorkingPanelsTests` are not.** The known-red
block is at line **139** and **none of it is yours**. `docs\carry-forward-dropped.txt` is not
a quarantine list. `docs\quarantined-tests.txt` exists, created by unit 375, and carries one
quarantined name, one recorded-but-not-quarantined name, and the dispatcher loop as an
environmental cause with no name attached.

**The counts to expect.** Unit 375's exit run: **app 210 of 210, engine 146 of 146**. The app
invocation takes about 2 m 20 s; the engine about 4 m 42 s.

**The version.** `Directory.Build.props` line **973**: `<Version>1.13.62</Version>`.

**The record's own state, and three of these are yours to repair in task 0.**

- **Yours.** `PHASE_STATUS.md` and `PHASE_OUTCOME.md` both stop at `STEP: 5`, while
  `PHASE_PLAN.md` at HEAD carries **steps 0 to 8**. `tools\arbiter\outcome-read.bat` therefore
  prints a six-step phase and **the next arbiter cannot see steps 6, 7 and 8 at all.** Add
  all three to both files as `not started`, transcribing the plan's one-line summaries.
- **Yours.** `PHASE_STATUS.md` `CURRENT_STEP: 0` and `WORK_INSTRUCTION: 374 - ...` - both
  stale by two units. Set them to `6` and to this instruction.
- **Yours.** `PROJECT_STATUS.md` reads `STATE: blocked`, `BALL: tim`, `TASK: TASK 4 of 4`,
  `WORK_INSTRUCTION: 374` and a `NOTE` saying two questions wait on Tim. **R38 answered both**
  and the ball is back; your status writes bring it forward.
- **Not yours.** `PROJECT_STATUS.md` `RULES_AT` reads `HM-DEC-165 (2026-09-19)` while
  `CLAUDE.md` section 1 holds `CPS-DEC-0165` - the id-scheme split, carried in
  `PHASE_PLAN.md` section 7. **Report it, do not repair it.**
- At authoring time HEAD was **c92bd2e1** on `main`, with `PHASE_STATUS.md` carrying an
  uncommitted `HEARTBEAT:` line and `.run-unit\reload.txt` modified. **Two uncommitted at the
  root, neither under `src\`.**

**What failures are expected, and what they mean.** Task 2 changes a layout four named tests
were written against. **Expect `TheGreenBlockIsInsideTheCardUnderTheStripWithTheBandLargest`,
`AtFourteenHundredTheLicensedTopRowIsTheMockupsShare` and the two share names to go red, and
expect `TopRowTarget = 190` to be the wrong number for a row ruled to 180.** All of those are
**this unit's to bring to the new rule under R12, in their own commit, each asserting more
than it replaced and never less** - unit 373's badge rewrite is the model. **What is not
expected and is not a rewrite:** `TheWorldClockIsAtTheCardsRightEndWithOneMarker` going red
(that is 6.3 broken), `BindingHealthTests` complaining (that is a binding you removed and did
not clean up), or any name in `TheStopIsAlwaysOnScreenTests` or `TheWorkingPanelsTests` going
red (those are step 1's closed criteria, and making them false is a regression under
HM-DEC-165).

---

## 6. Rulings in force

Transcribed in full. **Do not re-argue any of these.**

### The first is mine: what "the top row" is, and how the 180 is measured

Criterion 6.1 says *the top row - pills, neighborhood strip, green zone, rig display -
measures at or under 180 px at 1920 and at 1400, from 300*. `TheTopRowTests.Measured` measures
something narrower than that list: two rectangles, the card and the rig, and not the band row
above them. **A criterion met against the narrower thing would not be the criterion.**

**So, for 6.1 and for every number this unit reports:**

1. **The band is everything above the divider and below the header.** From the **top of the
   band row** (`Grid.Row="0"` of the strip grid at line 2896 - task 1 names the element) to
   the **bottom of whichever of the neighborhood card, the rig face and `RigDriveAndPower`
   ends lowest**, measured in the window's own frame by **one helper**, used everywhere. The
   Hamlet title, the byline and the status bar are not in it.
2. **Report both numbers, always: the band with the pills and the band without.** The
   criterion is met on the band **with** them. The band without is reported beside it so the
   owner can see which reading the 180 was reached on, and so the existing `TopRowTarget`
   names can be compared against the thing they actually measure.
3. **Three widths, not two.** 6.1 names 1920 and 1400; **also measure and report 1100 x 780**,
   the size Hamlet opens at, where the band is at its 300 px cap and where the pixels matter
   most to Tim. **1100 is reported, not asserted** - the criterion names two widths and this
   unit does not invent a third must-pass.
4. **On every mode the strip can be in.** `TheTopRowTests` sweeps FT8 and PSK31; Olivia is a
   third. **The band that must be at or under 180 is the tallest of them**, and the report
   says which mode that was.
5. **The panel row is measured at the same three sizes, before and after**, and *the working
   panels are taller by the difference* is asserted as arithmetic against task 1's before
   table - **not against a constant written in task 3.**

*Author's, overrulable.* It is none of `PHASE_PLAN.md` section 6's three stops: it defines
what is measured, spends nothing, and changes no sentence the operator reads.

### The second is mine: nothing leaves the window, it moves to a hover that keeps its words

6.2 says *nothing is lost*, and it names what must still be there: every pill, the strip's
segments, the band and frequency, the best bet, the heard count, the drive and power offer -
**and it sends the strip's legend and the rule-of-thumb line to a hover.**

1. **A thing that comes off the card goes onto a hover that carries the same words.**
   `MapLegendControl` (line 596) and `GreenZoneRuleOfThumb` (line 713) are the two, by R39.
   **The pattern is already in the file** - `HintMarkControl` on the card's header at line
   564 - and it is the pattern to use. **A test asserts the moved text still exists in an
   operator-facing string**, so *hiding detail* stays on the right side of CLAUDE.md 0.5 and
   never becomes hiding information.
2. **Everything else 6.2 names stays drawn, and is asserted present by name** at both widths
   and on every mode. **Nothing is made smaller by clipping**, nothing is `Collapsed` to buy a
   pixel, and no line is dropped because it was long.
3. **The sun map keeps its size and its dot** (6.3), asserted by comparing its rectangle to
   task 1's before. **If a move would shrink it, that move does not happen.** The clock column
   is `Auto` at a fixed picture height and the left column takes the rest, which is the
   mechanism already in the template's comment - **do not fight it, work with it.**
4. **`DigitalTransmitDriveNote` stays on the window at all nine of unit 354's sizes.** Step 1
   is closed and criterion 1.1 names that control. **Making it false is a regression, not a
   trade.**

*Author's, overrulable.* A layout, a number and a mechanism are the arbiter's by
`PHASE_PLAN.md` section 6 and never a stop.

**That is two rulings, which is R31's limit.** The fifteen carried asks get none.

### The phase's standing rulings

**R39 - Tim, 2026-09-21: the UI comes first.** *Steps 6, 7 and 8 are worked before steps 3
and 4; step 3 depends on step 8. Ruled A on the top row: tighten everything to one band of
about 180 px - pills half height, the neighborhood strip thinner with its legend on hover, the
green zone one line, the rig display shorter with drive and power beside the frequency - and
the sun map keeps its size.* **This is the owner's, it is the shape of task 2, and the hover
is his call and not a session's.**

**R34 - Tim, 2026-09-14: Hamlet opens with every control on the window.** The send area never
leaves the window; the panels give up height first, then the top row. **Step 1 closed on
that and this unit does not reopen it.**

**R38 (b) - Tim, 2026-09-21.** The transmit sequence's teardown abort pair is accepted, logged
as tidy-up, not a stop. **Closed. Nothing here touches it.**

**R11 - nothing at the radio.** No unit asks the operator to set a level, read a meter, or
know what ALC is.

**R12 - Tim, 2026-09-11: a session fixes its own tests and never asks the owner to approve
it.** A test that blocks the unit told to do the work is **the session's to rewrite in its own
commit** so it guards the rule and not the accident. **The arbiter never puts the wording of a
test to the owner.** Section 5 names the four this unit should expect to meet.

**R13 - telemetry is a must-pass on every remaining step.** **This unit adds no stage and
therefore no event.** If you find yourself adding one, you have left the criterion. The
visibility events are step 3's and step 3 waits on step 8.

**R14 - Tim, 2026-09-11: eyes on the prize.** *"We don't focus too much on pointless testing.
We remember what the phase goal is."* A unit writes the tests its criteria need and no others.

**R19 - American spelling** in every operator-facing string and every instruction.

**R31 - this phase runs unattended.** Criteria by id. A done step is closed. The owner's step
ends the run. **Two rulings per unit at most.**

**HM-DEC-021** the rig display is deliberately not collapsible; **HM-DEC-070** the star is
inside the black; **HM-DEC-086** the whole strip the rig display sits in is exempt from the
canvas; **HM-DEC-051** everything but the header and the status bar scrolls; **HM-DEC-032**
the legend is the key to the map's colors. **Section 0.0** a sentence on the screen is a
claim. **Section 0.5** hiding detail is allowed and hiding information is not. **HM-DEC-155**,
**HM-DEC-139** (the carried queue verbatim), **HM-DEC-165** (no name green before a unit is
red after it), **FACT-004** (nothing here is evidence about the radio), **HM-DEC-018 section
2.1** (nothing personal in an event).

## 7. Status cadence

`tools/status.sh`, real clock, **after every task and every commit**, and **immediately before
each carry-forward invocation** - task 0's two and task 4's two. **Task 1's measurement runs
are one task**: status before the first and after the last, not between every run. Never
compose a timestamp.

---

## 8. The tasks

### Task 0 - the record, the missing three steps, and the entry run

Append `UNIT 376 - STEP 6` to `PHASE_OUTCOME.md` with `ADVANCED: step 6`. Patch-bump
**1.13.62 -> 1.13.63** in `Directory.Build.props` with its line in the version log.

**Add steps 6, 7 and 8 to `PHASE_STATUS.md` and `PHASE_OUTCOME.md`, `not started`, with the
plan's one-line summaries** (section 5: both files stop at step 5 while the plan carries
nine, so `outcome-read.bat` prints a phase that is three steps short). **Set
`CURRENT_STEP: 6` and `WORK_INSTRUCTION: 376 - ...`** in `PHASE_STATUS.md`, both of which are
two units stale. One line in the report saying you did and why.

**Run the carry-forward list, both invocations, before anything changes**, status written
immediately before each, and put the two counts in the outcome entry's `ENTRY:` line. Unit
375 left it at **app 210 of 210, engine 146 of 146**. A red here is not yours; name it and go
on. **If either invocation dies of `InvalidProgramException: You have caused dispatcher loop`
before any assertion, that is unit 375's item 3 - re-run it once and record both attempts.**

**Drop candidate:** none.

### Task 1 - the trace: what the band is made of, before one pixel moves

**This task builds nothing, changes no source file and repairs nothing.** It is what makes
6.1's number a measurement instead of a target, and it decides which of R39's four moves is
worth making.

One `[AvaloniaFact]` trace, named for this unit, in `tests\Hamlet.App.Tests\Views\`, using
`TheTopRowTests`' own helpers. **At 1920 x 1040, 1400 x 1040 and 1100 x 780, on FT8, PSK31
and Olivia**, print in the window's own frame:

1. **The band, both readings** - with the pills and without - per section 6's first ruling,
   and **name the element that is `Grid.Row="0"` of the strip grid at line 2896**, which this
   instruction could not name.
2. **Every component's rectangle**: the band row; the neighborhood card and, inside it,
   `NeighborhoodMapControl`, `MapLegendControl`, `GreenZoneBlock` and each of its lines by
   name; `GreenZoneMap` with `GreenZoneGrayLine` and `GreenZoneClockCaption`; the rig face
   `Border` and `RigDisplayControl` inside it; `RigDriveAndPower`.
3. **Which of the two columns governs the band's height at each width** - the card stack or
   the rig column. **This is the question the task exists for.** The row is `Auto` and both
   halves stretch to it, so **shrinking the shorter column buys nothing**, and R39's four
   moves are not worth the same number of pixels.
4. **The panel row's height** at the same three sizes, and **at all nine of unit 354's
   sizes** - the before for 6.1's *taller by the difference* and for 6.4.
5. **`TheTopRowTests`, `TheWorkingPanelsTests`, `TheStopIsAlwaysOnScreenTests` and
   `BindingHealthTests`, run once, filtered, one build**, and the counts recorded. Unit 374
   had 33 of 33.

**What this task must answer in the report, by id.** For **6.1**: the band's height at 1920,
1400 and 1100, both readings, on the tallest mode, and how many pixels short of 180 each is.
For **6.2**: every named component's height, so the reader can see where the pixels are. For
**6.3**: the sun map's rectangle. For **6.4**: the nine-size panel row and the four types'
counts.

**Drop candidate: none, and if the unit runs long everything else goes before this does.** A
measured band handed to the next unit beats an unmeasured repair.

### Task 2 - the band comes down, R39's four moves, in its own commit

Act on task 1's numbers. **Take the moves in the order the measurement says pays**, and
re-measure after each so the report can say what each one gave up:

- **The pills at half height.**
- **The neighborhood strip thinner, its legend to a hover** (`MapLegendControl`, line 596).
- **The green zone on one line**, with `GreenZoneRuleOfThumb` (line 713) to a hover.
- **The rig display shorter**, and **drive and power beside the frequency rather than under
  it**.

**The sun map keeps its size and its dot (6.3): it is not a source of pixels.**

**Never by clipping and never by dropping a line.** Section 6's second ruling is the rule: a
thing that comes off the card goes to a hover that keeps its words, and everything 6.2 names
stays drawn.

**Drop candidate: the rig display's rearrangement - drive and power moved beside the
frequency - and only that.** 6.2 requires the drive and the power offer to be **present**, not
to be in any particular place, and *beside the frequency* is R39's mechanism, which
`PHASE_PLAN.md` section 6 makes the arbiter's. **Drop it if, and only if, task 1 measured the
neighborhood card as the governing column at both asserted widths and the band reached 180
without it.** It is the move that costs the most and buys the least: it touches the rig face
under HM-DEC-021 and HM-DEC-070, and it is the move that turns
`DriveAndThePowerOfferAreUnderTheRigAndTheSendAreaKeepsCqAndStop` red by its very name.
**Shortening the rig display itself is not the drop candidate and is not dropped** - if the
rig column governs the band, that is the work.

### Task 3 - the evidence: 6.1, 6.2, 6.3 and 6.4, and the lists

In its own commit or commits. **An R12 rewrite of an existing name goes in a commit of its
own**, separate from the new tests.

- **6.1** - a name asserting the band, measured section 6's way, is **at or under 180 px at
  1920 and at 1400** on the tallest mode, and that **the panel row is taller by the
  difference**, the difference computed against task 1's before table and quoted in the
  failure message.
- **6.2** - a name asserting every component 6.2 lists is present, and that the legend's words
  and the rule-of-thumb's words **still exist in an operator-facing string** now that they
  live on a hover.
- **6.3** - a name asserting the sun map's rectangle and its dot are what task 1 measured.
- **6.4** - unit 354's nine sizes hold, and **`TheTopRowTests`, `TheWorkingPanelsTests`,
  `TheStopIsAlwaysOnScreenTests` and `BindingHealthTests` green**. Where one of the four that
  section 5 names goes red, **rewrite it under R12 to assert the new rule** and say in the
  report, name for name, **what it asserts now that it did not assert before**. **Never loosen
  one to make a criterion pass**; if the only assertion that survives is weaker than today's,
  leave it red, report it, and mark the criterion `partial`.
- **`TopRowTarget = 190`** at line 45 and its four readers are this unit's under R12. **It may
  need to become two numbers** - the band with the pills and the band without - because the
  existing name measures the narrower thing. Say which you chose and why.
- **`docs\carry-forward-tests.txt`**: add the new 6.1 name by type and method, with its
  paragraph and its time, **and change the human-readable list underneath to match** - a
  mismatch there has been a finding in two of the last three units. **Add nothing that is
  knowingly red.**

**Drop candidate:** none. This is where the criteria are proved.

### Task 4 - the exit run, the record and the report

- **Run the carry-forward list, both invocations, after the last change.** Compare name by
  name against task 0. **A red after that was green before is a regression** and sections 1
  and 4 both name it as one (HM-DEC-165).
- **`PHASE_STATUS.md` and `PHASE_OUTCOME.md`:** step 6 criterion by criterion by id (R31).
  **Step 6 is `done` only if all four are met**; any one short and it is `partial` with the
  number that was reached. **Do not round up.** Leave step 2's line as unit 375 wrote it and
  add nothing to it - 2.4 is not this unit's and 2.2 is the owner's.
- Write `output.md` per section 12.

**Drop candidate:** none.

---

## 9. Parked - do not touch, do not raise

- **Criterion 2.2 - the RSID tone sequences - stays parked, and the arbiter has sent the
  question back to Tim.** R38 (a) permits the change on a stated fact: *Hamlet's modulator
  makes only 8/250, 16/500 and 32/1000, so nothing announces a variant it cannot send.*
  **Read at authoring time, line by line, the tree does not support that fact:**
  `data\olivia\format.json` lines 61 to 67 carry **all seven variants** - 4/250, 4/500, 8/250,
  8/500, 16/500, 16/1000, 32/1000 - and `OliviaModulator` is driven from that table, so the
  engine can modulate every one of them. **What keeps four of them off the air today is
  exactly the gate this change removes**: `OliviaModulator.Compose` line 149 throws where
  `RsidBurst.TonesFor` is null. And **the send variant is not picked by the operator from a
  list of three**: `MainWindowViewModel` line 16009 takes `_oliviaSendVariant`, set at lines
  6027, 16533, 16789 and 16930 **from the variant of a decoded row or card**, and
  `OliviaListener` line 280 starts a channel **from an RSID detection's own variant**. So an
  8/500 burst on the air would become detectable, start a channel, make a card, and an answer
  to that station would be composed and transmitted at 8/500. **That is a change to what goes
  on the air.** *This is the arbiter's reading and no unit has confirmed it.* **You do not
  confirm it, you do not act on it, and you do not touch `assets\data\rsid-codes.json`,
  `data\rsid\rsid-codes.json`, `RsidCodes`, `RsidBurst`, `RsidDetector`, `OliviaModulator` or
  the csproj's embedded resource.** Section 12 says where it goes in your report.
- **Criterion 2.4 - the five green rounds.** Not this unit's. Run the carry-forward list twice,
  at task 0 and task 4, and no more.
- **Steps 7 and 8** - the canned list, the hover on a PSK31 row, the achievements. **The hover
  this unit builds is the top row's legend and rule of thumb, and nothing to do with a decoded
  row.** Do not build toward step 7.
- **Steps 3, 4 and 5.** The visibility events, the radio sheet, Tim's verdict.
- **`PHASE_PLAN.md`'s criterion checkboxes** (unit 372's item 7). Do not tick them; do not
  raise it again. The record this phase maintains is `PHASE_STATUS.md` and `PHASE_OUTCOME.md`.
- **`tools\run-carry-forward.sh`** (unit 374's item 3). It still does not match the list. **Run
  the two command lines from `docs\carry-forward-tests.txt` itself.**
- **`ApplyBestBet`'s stale `BestBetLabel`** (unit 373's item 2), **`LearnedAlcReference.Ago()`**
  (unit 369's item 3), **the archived Olivia phase** at `docs\phase-olivia-run\` - a phase is
  never reopened - and **the `RULES_AT` id-scheme split.** Carry them; do not repair them.
- **Anything about what keys. Any package.** A package is `MOVE: stop`.

## 10. What not to do

- **No unfiltered `dotnet test`** (HM-DEC-155). Every invocation filtered, foregrounded, one
  build. **Never background and poll. Never compose a timestamp.**
- **Do not buy a pixel by hiding information** (CLAUDE.md 0.5, section 6's second ruling).
  Not by clipping, not by `Collapsed`, not by dropping a line that has nowhere else to go.
- **Do not fund the band from the send area, the mode tabs, the status bar or the header.**
  Step 1 is closed on R34 and the send area's 22 px is constant at every height.
- **Do not raise `TopRow`'s `MaxHeight`, and do not lower it to fake the criterion.** 6.1 is
  met by the content measuring less, not by a cap clipping more - a cap that binds is the card
  scrolling inside it, which is a smaller card and not a shorter one.
- **Do not shrink the sun map** (6.3).
- **Do not loosen a test to make a criterion pass** (`PHASE_PLAN.md` section 6). Rewrite it
  under R12 to assert the new rule, asserting more and never less, or leave it red and report.
- **Do not add an event, and do not add a test for a door you are not building** (R13, R14).
- **Do not delete a file.** Empty it, comment it, list it (`PHASE_PLAN.md` section 6).
- **Do not chase a red on the known-red block at line 139**, and do not chase the dispatcher
  loop - it is unit 375's item 3 and nobody's criterion yet. **Record it; do not repair it.**
- **Report mismatches; repair nothing but this unit's. Write American. Write files as UTF-8.**

## 11. Committing and pushing

**One commit per task**, on `main`, message naming the unit and the task. **An R12 rewrite of
an existing test goes in its own commit**, separate from the new tests and from any list
change it causes. **A carry-forward line edit goes in the same commit as the test it names**,
so the two never disagree in the history. Push once, at the end, after task 4's runs are green
or their reds are named.

---

## 12. Reporting

`output.md` at the root. **Canonical headings:** `## 1. What Claude did`, `## 2. What the
owner should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

**The ordering block first. `validate-output.bat` refuses a report without it.** Fill every
line from what you measured - a line that is the same every unit is furniture.

```
READ IN THIS ORDER.

A. The phase goal - Hamlet holds what it has. Step 0 partial, step 1 done, step 2
   partial with 2.1 and 2.3 banked and 2.4 short at one round of five, steps 3, 4
   and 5 not started, and steps 6, 7, 8 added to the plan on 2026-09-21 by R39 with
   the UI worked before the record. This unit is the first spent on any of them.
B. Step 6 - the top gives back height. 6.1 <met|partial|not>: the band measured
   <n> px at 1920 and <n> px at 1400 against 180, from <n> and <n> before, and the
   panel row is <n> px taller. 6.2 <met|partial|not>: <what moved to a hover, and
   what is still drawn>. 6.3 <met|partial|not>: the sun map is <n> x <n>, before
   <n> x <n>. 6.4 <met|partial|not>: the nine sizes and the four types, <n> of <n>.
C. The report last. Section 4 raises <N> items on top of the carried fifteen, and
   <none of them is | item <k> is> in the way of a criterion in B. The first item
   is not this unit's work at all: it is criterion 2.2, sent back to Tim.
```

```
UNIT:       376 - <complete|stopped> at task N of 5 - <date time>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   step 6, criteria <the ids you actually moved>
NUMBER:     the band at 1920, with the pills: <before> px -> <after> px, against 180
DRIFT:      none
```

**Section 3 must lead with task 1's component table** - three widths, three modes, every
named rectangle, both readings of the band, and which column governed - **because that table
is what makes 6.1 a measurement**, and it comes before any prose about what was moved. Then
the same table after task 2, beside it. Then the panel row at unit 354's nine sizes, before
and after. Then the four types' counts. Then the carry-forward counts before and after.

**Section 2 tells Tim in one paragraph what is different**, in his terms: not that a grid row
was retuned, but that **the top of his window has stopped taking a third of it and the
waterfall, the decoded text and For You are all taller by the same amount** - and that
nothing he read up there is gone, only that two lines now wait under the pointer. Every
appearance claim is computed, not seen (FACT-004).

**Section 4:** your own items first, most-blocking first, each saying plainly whether it wants
a ruling or is a finding - a note is not a ruling request.

**Your first item is criterion 2.2, and it wants a ruling.** Transcribe section 9's
measurement in full - the seven variants in `data\olivia\format.json`, the gate at
`OliviaModulator.Compose` line 149, `_oliviaSendVariant` taking its value from a decoded row,
and `OliviaListener` line 280 starting a channel from a detection's variant. Say plainly:
**R38 (a) permits the change on the ground that Hamlet's modulator makes only three variants,
and the arbiter reads the tree as saying otherwise; the arbiter has not confirmed it with a
run and neither has this unit; step 2 cannot close until Tim rules again, and no unit should
take 2.2 until he does.** Say also that **R38 (b) answered unit 375's item 2 and it is
closed.**

Then the carried queue verbatim per HM-DEC-139: **unit 375's items 3 and 4, unit 374's item 3,
unit 373's item 2, unit 372's items 4 and 7, unit 371's five and unit 369's four - fifteen**,
with one line saying that unit 375's item 2 came off it by R38 (b).

---

```
ARBITER-DECISION
STEP: 6
APPROACH: tighten the top row to one measured band of 180 px at 1920 and 1400 - pills at half height, the neighborhood strip thinner with its legend on hover, the green zone on one line, the rig display shorter with drive and power beside the frequency - and give every pixel it gives up to the working panels while the sun map keeps its size
MOVE: continue
WHY: step 6 is new in the plan of 2026-09-21, has zero units spent, and its entry - step 2 done, or partial with 2.1 and 2.3 met - is exactly where the phase stands; R39 of the same date puts the UI steps before the record steps, the top row draws 300 px at its cap at the size Hamlet opens at and about 190 at 1920, and every pixel it gives up is a pixel the three working panels get.
STATE: not started
DECIDED: author's, overrulable, three - two given to the unit in section 6, and one scoping decision that sends a question back to the owner. (1) What the top row is for criterion 6.1, and how the 180 is measured: the band runs from the top of the band-pills row to the bottom of whichever of the neighborhood card, the rig face and RigDriveAndPower ends lowest, measured by one helper in the window's own frame, because TheTopRowTests.Measured takes only the card and the rig rectangles and the criterion names the pills as well; both readings are reported every time, the criterion is met on the band with the pills, 1100 x 780 is measured and reported but not asserted since the criterion names two widths, the band that must be at or under 180 is the tallest of FT8, PSK31 and Olivia, and the working panels being taller by the difference is asserted as arithmetic against task 1's before table rather than against a constant chosen in the test. (2) Nothing leaves the window - a thing that comes off the card goes to a hover carrying the same words, which for R39 is the map legend and the rule-of-thumb line, with a test asserting the moved text still exists in an operator-facing string so hiding detail never becomes hiding information under CLAUDE.md 0.5; everything criterion 6.2 names stays drawn and is asserted present by name, nothing is bought by clipping or by collapsing, the sun map is not a source of pixels, and DigitalTransmitDriveNote stays on the window at all nine sizes because step 1 is closed on it. (3) THE SCOPING DECISION, AND IT GOES BACK TO THE OWNER: criterion 2.2 stays parked notwithstanding R38 (a), because the tree contradicts the fact R38 (a) rests on. R38 (a) permits codes 72-75 their tone sequences on the ground that Hamlet's modulator makes only 8/250, 16/500 and 32/1000 so nothing announces a variant it cannot send. Read at authoring time: data/olivia/format.json lines 61 to 67 carry all seven variants and OliviaModulator is driven from that table, so the engine can modulate every one; what keeps four of them off the air today is exactly the gate the change removes, OliviaModulator.Compose line 149 throwing where RsidBurst.TonesFor is null; and the send variant is not chosen from a list of three but taken from a decoded row - MainWindowViewModel line 16009 reads _oliviaSendVariant, set at lines 6027, 16533, 16789 and 16930 from a row's or a card's variant, and OliviaListener line 280 starts a channel from an RSID detection's own variant. So an 8/500 burst would become detectable, start a channel, make a card, and an answer to that station would be composed and transmitted at 8/500 - a change to what goes on the air, which PHASE_PLAN.md section 6 makes the owner's. This is the arbiter's reading of the tree and no unit has confirmed it with a run. The phase is NOT halted for it: the unit authored here touches no port, no event and nothing that keys, step 6 is a whole step nobody has spent an hour on, and halting would spend the night Tim asked to use. The question is carried to him as the first item of this unit's section 4, and no unit takes 2.2 until he rules again.
LICENCE: PHASE_PLAN.md R39, R34, R31 and section 6 - a layout, a number, a mechanism and a test's shape are the arbiter's and never a stop; PSK31 plan R11, R12, R13, R14, R19; CLAUDE.md 0.0, 0.2, 0.5, 0.6; HM-DEC-021, HM-DEC-032, HM-DEC-046, HM-DEC-051, HM-DEC-070, HM-DEC-086, HM-DEC-139, HM-DEC-155, HM-DEC-165, HM-DEC-018 section 2.1; FACT-004
ACCOMPLISHED: The top of Tim's window stops taking a third of it. The bands, the neighborhood strip, the green block and the radio's face come down to one short band, and every pixel they give up goes to the three panels he actually works in - the waterfall, the decoded text and For You - at the size Hamlet opens at as well as at the size his monitor is. Nothing he read up there is gone: the map's color key and the rule-of-thumb line wait under the pointer instead of standing on the card, and the sun map is exactly the size it was.
ADVANCES: step 6, criteria 6.1, 6.2, 6.3 and 6.4 - all four must-pass, all four untouched by any unit, and 6.1 is the one the other three are built around. It is also the first of R39's three UI steps, which steps 7 and 8 depend on in turn and which step 3 waits behind.
END-ARBITER-DECISION
```
