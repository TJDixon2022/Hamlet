# Work instruction 388 - the band governs the map, and favorites go back where Tim had them

**Step 10 of the hardening phase, its third unit, and both of tonight's criteria are Tim's
words from this afternoon.** At `752a9b62` (rev7, 2026-09-22 13:07) he read unit 387's report
and did two things: he **reworded 10.3** - *vertical space is the concern, not horizontal;
unit 387's reading that the map governs the band is superseded - the band governs the map* -
and he **added 10.6** - favorites are a drop-down under the green zone, the way Hamlet had
them before 2026-08-27, and the caret unit 387 put beside the star comes off. Four tasks and
an exit. **This unit writes code under `src/Hamlet.App/Views` and `Controls` and nowhere near
a send path.**

**The step you were handed was step 0. This unit is step 10, and section 4 says why in one
table.** In one line: 0.1 is closed to units by work instruction 386 section 6 ruling 1, and no
arbiter may overrule an earlier arbiter's ruling - only Tim can, and he has not.

**Status.** `sh tools/status.sh`, real clock, after every commit and every task. **The `sh` is
part of the command - read section 2 before you type it.**

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

*All four were checked against the tree at authoring time and all four hold: `SHACK_FACTS.md`
and `src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs` are present, neither `CoreHMI.sln`
nor `MURC.sln` exists at the root, and the root is `C:\Source\HamLet`.*

---

## 1. The rules that killed sessions

**HM-DEC-155.** No suite; only `docs\carry-forward-tests.txt` as its own top comment says -
**two invocations, one build each**, status written immediately before each. Never background
and poll. **Unit 323 ran one `dotnet test` per name - forty-one builds - wrote one status
line, and the watchdog killed it at twelve minutes of silence.**

**Write a status line immediately before every test invocation**, naming the task and what is
running. Silence is what kills a unit.

**The app invocation is the flaky one and you must expect it.** The headless
`InvalidProgramException: You've caused dispatcher loop` kills at about **1 ms, before any
assertion**, moves between names, and **has never once touched the engine invocation**. Unit
387 met it **five times in five app invocations**. **It is not a red; it is a lost invocation.
Re-run it once, record both attempts, and do not chase it.** *About 1 ms and before any
assertion* is the session fault; **anything that ran and disagreed is a red and is yours.**

**A loop goes in a script file** (`;` is refused in a compound command). `Bash(sh:*)` is
granted, so `sh .run-unit/unit388-<name>.sh` works. **Write the script with the editor.**

---

## 2. The tool facts

- **The status line is `sh tools/status.sh ...`, with the `sh`.** A permission rule is a
  literal prefix match over the whole command string; `tools/status.sh ...` and
  `./tools/status.sh ...` are both refused.
- **`git status` with no `-C`.** You are already standing in the root.
- **`git log`, `git log -S`, `git show`, `git diff` and `git rev-parse` are granted.** Task 1
  uses `git show a51bc2a6^:src/Hamlet.App/Views/MainWindow.axaml` to read what the favorites
  control looked like before it went - **read it; do not reconstruct it from memory.**
- **`git stash`, `git worktree add` and `git checkout <ref> -- <paths>` are refused** (unit 387
  measured all three). You cannot run the pre-unit tree. Plan for that.
- **Shell output redirection (`>`) is refused to every path.** Write files with the editor. A
  test run's output is read from the console, or piped to `grep` in the same command.
- **A compound command with `;`, `&&` or a second operation is refused**, and so is
  `cd X && Y`.
- **Apostrophes in quoted heredocs break; doubled backslashes collapse; `rm` is refused; `-m`
  more than once for a multi-line commit.**
- **Write this repository's files as UTF-8. `PHASE_OUTCOME.md` is BOM+CRLF - edit it with the
  editor and do not normalise it.**
- **`validate-output.bat` is at `tools/arbiter/validate-output.bat`**: `./tools/arbiter/validate-output.bat output.md`,
  forward slashes, a leading `./`, one command, no `cd`, no `cmd /c`. **Run it before you say
  the report is written.** If it answers *This command requires approval*, that is the
  permission mode: hand-check against the script's own header, say in section 4 it was a
  hand-check and not a run, and move on. Eight units running have hit this.

---

## 3. Asks still outstanding

Carried per HM-DEC-139, **verbatim in section 4**.

**The queue stands at sixty and you answer none of them** - the fifty-three unit 387 carried
plus **unit 387's own seven**. Unit 387's report is `output.md` at the root at `752a9b62`; its
section 4 is the list. **Read it; do not reconstruct the queue from memory.**

Five of them touch tonight and each gets one line in your section 4:

- **Unit 387's item 2 - 214 against 220.** *Answered by Tim, not by you*: rev7's 10.3 reads
  *the band itself does not grow, 214 px stands.* Say so in one line and carry it closed.
- **Unit 387's item 3 - at 1100 x 780 there is height beside the map and spending it costs
  width.** Still Tim's. **Tonight's layout will change what is true at 1100 x 780, so re-measure
  it and report the new numbers beside the old ones** - do not rule on it.
- **Unit 387's item 4 - eight names in `TheMenuIsUnderTheMouseTests` red on a scene
  precondition, not proved inherited.** Not on the carry-forward list. **If tonight's layout
  change touches `DigitalDecodedRows` or `DigitalMineRows` being realized, it is yours.** It
  should not - the top band is above the working panels - so run the type once at the entry
  and once at the exit and report the count both times. **Do not repair it.**
- **Unit 387's items 1, 5, 6 and 7** - the nameless card, the 10.4 gate's missing telemetry,
  unit 386's launcher and 0.1 items, and the `RULES_AT` split (now **the eleventh unit
  running**: `PROJECT_STATUS.md` reads `HM-DEC-165 (2026-09-19)`, `CLAUDE.md` section 1 holds
  `CPS-DEC-0165`, `tools\status.sh` writes it as a literal and **you may not edit `tools\`**).
  **None is yours. Do not re-record them beyond the verbatim carry.**
- **Unit 383's inherited reds** - `TheOliviaMoveUpTests.ItIsNotOfferedOnAGuessedYourTurn`,
  `ViewTestsActThroughControlsTests.NoViewTestWritesAPropertyAControlOwns` (one offender,
  `TheStopIsAlwaysOnScreenTests.cs:102`) and `TheOliviaSendTests.StopMidPlayAbortsAnOliviaSend`.
  None on the list. **Do not go hunting them.** Tonight you write view tests beside the second
  one's trap - **act through the control and read bounds; never write a property a control
  owns.** Run `ViewTestsActThroughControlsTests` at the exit and say the offender count is
  still one.

---

## 4. Why this unit exists

```
PHASE GOAL: Hamlet holds what it has - the hardening phase: screen, record and test
            work banked in the PSK31 and Olivia threads that needs neither the radio
            nor the owner, judged by tests that ran and, at the end, by Tim at his
            window.
UNIT GOAL:  Do the two things Tim marked on his own screen this afternoon: give the
            sun map the top band's whole height by taking it out from under the
            card's chrome, so the band governs the map and not the other way round;
            and put favorites back as the one drop-down Hamlet had under the green
            zone before 2026-08-27, taking unit 387's caret off the rig face.
ADVANCES:   step 10, criterion 10.6 and criterion 10.3, with 10.3's second stage -
            the map beside the pills row - the named drop candidate.
DRIFT:      none.
```

**The count today**, from `PHASE_OUTCOME.md`, `PHASE_PLAN.md` at `752a9b62` and the reload of
2026-09-22 13:08.

| Step | State | Units spent | Where it stands |
|---|---|---|---|
| 0 | `partial` | 369, 371's carried repair | 0.2 to 0.5 met. **0.1 cut down by instruction 386 ruling 1, closed to units** |
| 1 | **`done`** | 372, 373, 374 | judging session `done` |
| 2 | **`done`** | 375, 377, 384, 386 | judging session `done` |
| 3 | **`done`** | 380, 381 | judging session `done` |
| 4 | `partial` | 382, 383 | 4.3 cut down by instruction 384, logged to the owner |
| 5 | `not started` | 0 | **Tim's own, and it ends the run** |
| 6 | **`done`** | 376 | closed by R42 at the measured floor |
| 7 | `partial` | 378, 383 | 7.2 met on unit 383's own account, **unjudged** |
| 8 | **`done`** | 379 | judging session `done` |
| 9 | ungraded | 385 | work landed at `e5e4bee0`; 9.1 to 9.5 unticked |
| 10 | `partial` | **387** | 10.1, 10.2, 10.4, 10.5 met - judging session `partial` at `PHASE_OUTCOME.md:559`, on 10.3 alone. **Rev7 reworded 10.3 and added 10.6. This unit** |

**Step 10's entry** was ruled open by instruction 387 section 6 ruling 1 and step 10 has had a
unit since; it is open.

### Why step 10, when the step handed to this arbiter was step 0

| # | What was tried at 0.1 | What it hit |
|---|---|---|
| A | Unit 369 searched the 119 commits of `681d45c8..ec4b466e` over the four settings paths | **zero** commits; a completed negative |
| B | Instruction 372 ruled 0.1 met on that negative | judging session `partial`: the report *"names none"* |
| C | Instruction 386 re-measured wider - any path whose name contains `settings` | **zero** again; **cut down, closed to units, remedy logged to Tim** |

**Instruction 386's ruling stands and I may not overrule it** - no self-ruling overrules an
earlier arbiter's. Instruction 387 logged the one shape nobody has searched (a saved device
name lost where it is resolved against the devices the OS enumerates); **it is still Tim's to
open, and he has not.** Of everything else open, 4.3 is cut down, 5.1 is Tim's verdict, and
7.2 and step 9 wait on a grading pass. **10.3 and 10.6 are the only criteria in this phase a
unit can move, and Tim wrote both today.**

### What this unit is worth, in Tim's terms

**He has 214 px of band and a map using 134 of it**, and he said plainly which way round he
wants the trade: the band stays, the map grows into it, and the neighborhood strip beside it
gives up width, which he has plenty of. And **the favorites list is back, but not where his
hand goes for it** - he marked the empty row under the green zone as where it lived and where
it should be.

### Why this is not unit 387's approach again

Unit 387 let the map take the height **its own row** offered, inside the card, under the
card's header and over its caption - and the card and the band grew with it, to 460 px. **That
approach is not repeated.** Tonight the map **stops being inside the card's chrome**: it
becomes its own column in the band, as tall as the band, so the card's header and padding
(32 px), the caption and its gap (11 px) and - at the second stage - the pills row (36 px) sit
**beside** the map instead of stacked on it. The loop test returned *NOT FOUND*, and I judge it
a different mechanism rather than a rewording.

---

## 5. Verify this instruction against the tree

Every line below was read at authoring time. **Report every mismatch in sections 1 and 4 of
your report; repair nothing but this unit's.** Line numbers move under an edit - the name is
the thing that matters. **Where your measurement disagrees with mine, yours wins and it is a
finding.**

### The record and the versions

`Directory.Build.props` line 1211 reads `<Version>1.13.74</Version>`. `PHASE_STATUS.md` reads
`CURRENT_STEP: 0` and `WORK_INSTRUCTION: 387 - ...`, both stale. `HEAD` is `752a9b62`.
`PHASE_PLAN.md` leaves exactly **eleven** criteria unticked - 0.1, 4.3, 5.1, 7.2, 9.1 to 9.5,
**10.3 and 10.6** - and 36 ticked.

**The reload's two disagreements**, both reported and neither yours: the `RULES_AT` split
(section 3), and *PHASE_OUTCOME header says step 6 is done; the last entry for it says
partial*. That second one is **R42** - Tim closed step 6 by ruling after the judging session's
`partial` - so the header is right and the entry is history. One line in section 4.

### What a round should come back as

Unit 387's exit round: **app 265 of 265, engine 150 of 150**. Line 7 of
`docs/carry-forward-tests.txt` carried **62** filter terms at authoring time and line 9 **25**;
both begin `timeout 480 dotnet test`. **Used unedited, except where section 6 ruling 2 edits a
name in the same commit as the test it names.**

**One mismatch I found and you confirm, not repair:** 10.5's ticked text says `TheTopRowTests`
is on the carry-forward line; **I could not find it on line 7** - only
`Unit376TheTopBandTests.TheTopBandIsOneShortRowAndThePanelsAreTallerByTheDifference`,
`BindingHealthTests.TheMainWindowBindsWithoutOneComplaint` and
`TheFavoritesAreBackOnTheRigDisplayTests`. Count it and say what you found.

### For 10.3 - the map and the band

| Where | What is there |
|---|---|
| `MainWindow.axaml:2978` | the outer grid, `RowDefinitions="Auto,Auto,Auto,Auto,*"` |
| `MainWindow.axaml:3388` | the band-pills `ItemsControl`, `Grid.Row="0"` - a **full-width row above** `TopRow` |
| `MainWindow.axaml:3038` | `TopRow`, `Grid.Row="1"`, `ColumnDefinitions="*,Auto"`, `MaxHeight="300"`: the card scroller in column 0, the rig border in column 1 at `:3072` |
| `MainWindow.axaml:609` | `NeighborhoodCardBody`, `ColumnDefinitions="*,Auto"`, **inside** the `widget.map` `CollapsiblePanel` - so the map sits under the card's header and padding |
| `MainWindow.axaml:1020-1035` | `GreenZoneMap` stack: `GreenZoneGrayLine` at **`Height="134"`** and `GreenZoneClockCaption` *where the sun is · you* |
| `MainWindow.axaml:992-1019` | unit 387's comment: *the map already governs this card*; the 80 px it does not have are pills 36, card header and padding 32, caption and gap 11 |
| `tests/.../Unit376TheTopBandTests.cs:110` | `BandReachedWithThePills = 214` - the ratchet |
| `Unit376TheTopBandTests.cs:616-632` | `TopBand`: **the band runs from the top of the pills to the lowest of card, rig and `RigDriveAndPower`** |
| `Unit376TheTopBandTests.cs:449` | `TheSunMapIsTheSizeItWasAndStillCarriesHisGrid`, rewritten by unit 387 to assert *never smaller than 246 x 134, never a pixel of its row unused* |

**Read the `TopBand` row twice.** The pills are inside the band the criterion measures and in a
different grid row from the card. **A map as tall as the band has to stand beside the pills row
as well as beside the card**, which is why section 6 ruling 1 splits 10.3 in two.

### For 10.6 - the drop-down and the caret

| Where | What is there |
|---|---|
| `a51bc2a6`, 2026-08-27, `MainWindow.axaml` | the commit that took the old control out: `ComboBox ItemsSource="{Binding Favorites}" PlaceholderText="places you chose"`, a *favorites* label, `FavoriteHere` and `FavoriteNote` - **read the old markup with `git show a51bc2a6^:...` before you write the new one** |
| `MainWindow.axaml:3094-3098` | the rig face's bindings: `IsFavorite`, `FavoriteLabel`, `ToggleFavoriteCommand` (**stay**) and `Favorites="{Binding FavoriteMenu}"`, `ManageFavoritesCommand` (**unit 387's, go with the caret**) |
| `MainWindow.axaml:3100-3112` | unit 387's comment, *the list comes back on the rig face and not in a strip* - **superseded by 10.6; rewrite it** |
| `RigDisplayControl.cs:190, 381-422, 634-638` | the caret `▾`, `_listRect`, the bail that zeroes it, and `SavedListUnderThePointer` for tests |
| `MainWindow.axaml:2768` | the Radio menu's `ItemsSource="{Binding FavoriteMenu}"` - **stays**; two ways in is not a defect |
| `MainWindowViewModel.cs` | `FavoriteMenu`, `TuneToFavoriteCommand` with `FavoriteTuned`, `ManageFavoritesCommand` - **wire them, do not rewrite them** (R14) |
| `tests/.../Views/TheFavoritesAreBackOnTheRigDisplayTests.cs` | **five names, the whole type on the carry-forward line**: `...CaretAreDrawnAndHittableAtAllNineSizes...` (67), `PressingTheStarSaves...` (120), `TheListOpensFromTheRigDisplayAndOneClickTunes...` (171), `WithNothingSavedTheListSaysSo...` (252), `TheWayBackInCostTheTopBandNothing` (298) |
| `TheFavoritesAreBackOnTheRigDisplayTests.cs:277-281` | the empty list today: **two items - a note mentioning the star, not hittable, and `Manage favorites…`**. That note is the empty case's sentence; **reuse it, invent none** |

### What will be red before you start

Unit 383's three inherited reds and unit 387's eight `TheMenuIsUnderTheMouseTests` names -
**none on the carry-forward list** - plus the known-red block in
`docs/carry-forward-tests.txt`, likewise never on the list.

---

## 6. Rulings in force

**Transcribed in full. Do not re-argue any of them.** Two are mine, author's and overrulable
under R31, which allows two a unit. The rest are the plan's and the project's.

### The first is mine: what *the top band's existing height* means, and the two stages

**Author's and overrulable.** `PHASE_PLAN.md` section 6: *a layout, a number ... decide, mark
author's, continue.*

1. **The target is the band as `Unit376TheTopBandTests.Band` measures it - `WithPills`, 214 px
   at 1920 and at 1400.** 10.3 is **met** when `GreenZoneGrayLine`'s drawn height equals that
   band's height within **the band's own outer margins only** (report each margin by name and
   in px), at both widths, with the band still at 214, the map at its own proportions
   (HM-DEC-092), and its width taken from the neighborhood side - the strip and the green
   block - and **never from the rig column**.
2. **Stage A - the map out of the card's chrome.** The map becomes its own column in the band,
   beside the card and not inside it, as tall as `TopRow`: the card's header and padding stop
   standing over it and the caption stops standing under it. **Stage A alone is 10.3 partial,
   reported with both numbers.**
3. **Stage B - the map beside the pills. THE NAMED DROP CANDIDATE.** The pills row gives up
   the map's width so the map runs from the top of the pills to the bottom of the band. **If
   the pills then wrap and the band grows, stage B fails: revert it in the same task, report
   the band and the pills' row count, and stop at stage A.**
4. **The caption is kept, its words unchanged** - `GreenZoneClockCaption` stays in the tree,
   **wholly drawn and wholly on the window**, beside the map or inside the map's own bounds
   where it overlays no land and no grid marker. **Where it goes is yours; report it.**
5. **At every one of unit 354's nine sizes, the map is never smaller than 246 x 134** - the
   floor unit 387's guard holds - **and step 1's rules hold**: CQ, the mode tabs, the send
   area and Stop on the window; the panels give up height first. Below 1400 the map may be
   whatever the row allows above that floor. **1100 x 780 is re-measured and reported beside
   unit 387's item 3, not ruled on.**
6. **`TheSunMapIsTheSizeItWasAndStillCarriesHisGrid` is rewritten under R12 a second time**
   to assert rev7's rule - *the map's height is the band's, at 1920 and 1400, the band still
   214* - keeping the 246 x 134 floor and every assertion about the grid marker and the
   caption. **That is not loosening a test: Tim replaced what it asserted at `752a9b62`.**

### The second is mine: unit 387's caret guards are rewritten, not deleted and not skipped

**Author's and overrulable.** 10.6 says *the caret unit 387 added beside the star is removed*.
`TheFavoritesAreBackOnTheRigDisplayTests` is on the carry-forward line and three of its five
names assert the caret. **The owner replaced what they assert**, so under R12:

1. **`PressingTheStarSavesTheDialAndTheModeWithANameAndPressingItAgainRemovesIt` is not
   edited** - 10.6 says the star keeps saving.
2. **The caret's hit rectangle and drawing are removed** from `RigDisplayControl`, with the
   `Favorites` and `ManageFavoritesCommand` properties it used. **The star's own drawing, its
   bail and `_starRect` are untouched.**
3. **The other four names are rewritten to assert 10.6**, one for one: the star drawn and
   hittable at all nine sizes **and no caret drawn or hittable anywhere on the rig face**; the
   drop-down opening the named spots and one click on one tuning; the empty case offering the
   same note and `Manage favorites…` it offers today; and the band not growing by a pixel for
   it. **Rename the type if its name is now false**, and change the carry-forward line **in
   the same commit**, by name.
4. **No name leaves the carry-forward line without its replacement going on in the same
   commit.** The line's count after this unit is at least what it is tonight.

### The plan's own, which tonight is built on

**10.3, rev7, verbatim:** *The sun map grows to the top band's existing height at 1920 and at
1400 - the band itself does not grow, 214 px stands - keeping its aspect; the width it needs
comes from the neighborhood strip beside it; its dot and caption kept. (Tim, 2026-09-22:
vertical space is the concern, not horizontal; unit 387's reading that the map governs the band
is superseded - the band governs the map.)*

**10.6, rev7, verbatim:** *Favorites are a drop-down in the empty row under the green zone,
inside the neighborhood card, the way Hamlet had them before 2026-08-27: one control reading*
Favorites*, opening the named spots, one click tunes; the star on the rig face keeps saving;
the caret unit 387 added beside the star is removed (Tim, 2026-09-22, marked on his screen).*

**R46 (c)** the sun map takes the top band's full height. **R42** 6.1 met at the measured
floor; 214 stands. **R34** the send area never leaves the window. **HM-DEC-092** the map keeps
its projection's proportions. **HM-DEC-070** the star is inside the black. **§0.5.1 and the
2026-09-06 rule** - nothing is greyed or disabled that can be used; an entry with no command is
a note.

**R11** nothing at the radio. **R12** a session rewrites its own tests. **R13** telemetry on
every stage. **R14** nothing beyond the criterion. **R19** American. **R31** unattended -
criteria by id, a done step closed, the owner's step ends the run, **two rulings a unit**.
**HM-DEC-018 §2.1** nothing personal in an event. **§0.0** a sentence on the screen is a claim.

**`PHASE_PLAN.md` section 6:** *Three stops only: keying, transmit or the radio's safety; money
past the budget; a fact the product states to the operator about the radio, a contact or a
send. A hint, a label, a number, a layout, a test's shape, a mechanism arithmetic will not
allow: decide, mark author's, continue. The later ruling wins. A done step is closed. A
must-pass missed by a little: ship, report, `partial`, move on. **Never loosen a test.***
**Nothing tonight is near a stop.** A drop-down that tunes is a tune, not a send.

---

## 7. Status cadence

`sh tools/status.sh`, real clock, **after every commit, after every task, and immediately
before every test invocation**. Name the task and what is running. **The watchdog kills at
twelve minutes of silence and the app invocation alone runs about 2 m 40 s.**

---

## 8. The tasks

**Five tasks. The trace is task 1 and builds nothing under `src`. The named drop candidate is
task 3's stage B** - the map beside the pills row.

**10.6 goes first because it is certain and small, and because 10.3 then has to leave it
standing** - the drop-down lives in the card whose shape task 3 changes.

### Task 0 - the record and the entry run

Version **1.13.74 -> 1.13.75** with its line in the version log. `PHASE_STATUS.md` to **step 10
and unit 388**. **`PHASE_PLAN.md` ticks nothing at this task.** Append `## UNIT 388 - STEP 10`
to `PHASE_OUTCOME.md` in the house shape, carrying this instruction's `STEP`, `ADVANCED`,
`APPROACH`, `MOVE`, `WHY`, `DECIDED`, `LICENCE`, `COST`, `ACCOMPLISHED` and an `ENTRY:` line
with the entry round's real numbers.

**Then the entry round:** both command lines, unedited, one build each, status before each.
**Expect app 265 of 265 and engine 150 of 150.** A lost app attempt is re-run once. Then
`TheMenuIsUnderTheMouseTests` once, its count recorded (section 3). **A carry-forward name red
on an assertion here is inherited, named, and does not stop the night.**

**Commit.**

### Task 1 - the trace: measure before you move a pixel

**No file under `src` changes.** Every *before* number in your section 3 comes from here.

1. **The band, at 1920 and 1400** - `WithPills`, `WithoutPills`, and its parts in px: the pills
   row and its margins, the card's header and padding, the map, the caption and its gap, the
   rig column's wanted height, `RigDriveAndPower`. **Say which column governs and by how
   much.** Then the same at **1100 x 780**.
2. **The width budget at 1400** - the card's width, the strip's, `GreenZoneLeft`'s and
   `GreenZoneRight`'s, and **the width a 214-tall map at its own proportions needs** (unit 387's
   246 x 134 gives the aspect). **Say by arithmetic, before building, whether the green block's
   text still fits in what is left without making the card taller than 214.** If it cannot,
   say so here - that is the honest end of stage A and B, and it is reported, not argued.
3. **The pills row** - how wide it is and how wide its buttons need at 1920 and 1400, so stage
   B's cost is known before stage B is tried.
4. **The favorites control before 2026-08-27** - `git show a51bc2a6^:src/Hamlet.App/Views/MainWindow.axaml`,
   the control, its label, where it sat relative to the green block, and what selecting did.
   **Quote the lines.** And the empty row under the green zone today: **where it is and how tall,
   at 1920 and 1400.**

**Commit** (the trace's record only).

### Task 2 - 10.6, the drop-down under the green zone and the caret off the rig face

- **One control reading *Favorites*** in the empty row under the green zone, **inside the
  neighborhood card**, shaped after what task 1 item 4 found - a drop-down whose list is
  `FavoriteMenu`, **the same list the Radio menu shows**, so nothing about a favorite is
  decided in two places.
- **One click on a named spot tunes** through `TuneToFavoriteCommand` with its own
  `FavoriteTuned`. **No second click, no confirm.** The control goes back to reading
  *Favorites* afterwards rather than keeping a selection that stops being true the moment the
  dial moves (§0.0).
- **Empty is a note, never a greyed control** - the same note and `Manage favorites…` the rig
  face's list offers today.
- **The star keeps saving**; the caret and its hit rectangle go (section 6 ruling 2). The
  comment at `MainWindow.axaml:3100` is rewritten to say where the list went and why - rev7,
  Tim, 2026-09-22.
- **The band does not grow by one pixel**, measured at 1920 and 1400 against 214. If the row
  under the green zone is not empty at some width, report the width and what the control cost.
- **No new event writer** - `FavoriteTuned` exists. If you add one, `CallsignPrivacyTests`'
  walk grows in the same commit.

Section 6 ruling 2's test rewrites and the carry-forward edit go **in this commit**.

**Commit.**

### Task 3 - 10.3, the band governs the map

**Stage A** (section 6 ruling 1 item 2): the map out of the card's chrome and as tall as
`TopRow`, its width from the neighborhood side, the caption kept, the drop-down from task 2
still in the card and still working. **Measure and commit stage A before you try stage B.**

**Stage B - THE DROP CANDIDATE** (ruling 1 item 3): the map beside the pills row, so it is the
band's full 214. **If the pills wrap or the band grows, revert stage B in the same task** and
report the numbers. If the night is short, **drop stage B, say so in one line, and go to task
4.** 10.3 then stays unticked and reported partial with the map's height against 214. **That is
a permitted outcome. A band at 215 is not.**

Either way: `TheSunMapIsTheSizeItWasAndStillCarriesHisGrid` rewritten under ruling 1 item 6;
`Unit376TheTopBandTests.TheTopBandIsOneShortRowAndThePanelsAreTallerByTheDifference` **green**;
**unit 354's nine sizes measured and reported** against unit 381's `483, 71, 92, 163, 171, 267,
483, 460, 860`; and the map, the card and the caption at **1100 x 780** reported beside unit
387's item 3.

**Commit** (stage A, then stage B if it holds).

### Task 4 - the exit run, the record and the report

The exit round of both invocations, one build each, status before each, **name for name**
against task 0. Then `TheMenuIsUnderTheMouseTests` and `ViewTestsActThroughControlsTests` once
each (section 3). Then `PHASE_PLAN.md`: **tick only what tonight earned**, each with tonight's
numbers beside it in the plan's own text - **10.6 and, only if stage B held, 10.3.** Tick
nothing of steps 0, 4, 7 or 9. Then `PHASE_STATUS.md`'s `STEP: 10` line, then `output.md`, then
`validate-output.bat`, then push.

**Leave step 10's state word at `partial`** - closing a step is a judging session's and not a
unit's, even if every criterion is met.

---

## 9. Parked - do not touch, do not raise

- **Criterion 0.1 and the 1.13.30-to-1.13.48 window.** Instruction 386 ruling 1.
- **10.1, 10.2, 10.4, 10.5** - met, ticked. **10.2's nameless-row card** (unit 387 item 1) is
  Tim's.
- **`docs/RADIO_SHEET.md` and every string it quotes.** If a string you change is quoted there,
  do not change the string; section 4 item.
- **7.2, step 9's code and criteria.**
- **The dispatcher loop's cause.** Record every occurrence; chase none.
- **`tools\`, including the `RULES_AT` split.**
- **The two false outcome rows** (the Olivia phase's `## UNIT 2 - STEP 1`, this phase's
  `## UNIT 7 - STEP 2`). Both stay exactly as they are.

---

## 10. What not to do

- **Do not touch a send path, a modulator, an `Arm` site, a `PttOn` site or anything that
  keys.** Nothing tonight needs one.
- **Do not let the band grow by one pixel past 214** at 1920 or 1400 - rev7 says *214 px
  stands*, and the carry-forward ratchet asserts it.
- **Do not take the map's width from the rig column**, and do not shrink the rig face.
- **Do not stretch or crop the map** - its proportions are its projection's (HM-DEC-092).
- **Do not lose anything 6.2 protects** - every pill, the strip's segments, band and frequency,
  best bet, heard count, drive and power offer - to make room. A wrap is not a loss; a hidden
  element is.
- **Do not delete a carry-forward name or skip one.** Ruling 2 rewrites, in the same commit.
- **Do not write a property a control owns from a view test.**
- **Do not grey the Favorites control when the list is empty.**
- **Do not put anything personal in an event** (HM-DEC-018 §2.1), and do not open Tim's own
  settings file.
- **Do not run a suite.** HM-DEC-155.
- **Do not call a red environmental because you would like it to be.**

---

## 11. Committing and pushing

**One commit per task**, with task 3 allowed two - **stage A, then stage B** - so a reverted
stage B leaves stage A standing in the history. **A carry-forward line edit goes in the same
commit as the name it is about.** **Push once, at the end.**

---

## 12. Reporting

`output.md` at the root. **Canonical headings:** `## 1. What Claude did`, `## 2. What the owner
should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

**The ordering block first. `validate-output.bat` refuses a report without it.** Fill every line
from what you measured.

```
READ IN THIS ORDER.

A. The phase goal - Hamlet holds what it has. Steps 1, 2, 3, 6 and 8 done; step 0
   partial with 0.1 cut down and closed to units; step 4 partial with 4.3 logged to
   the owner; step 7 partial on an unjudged 7.2; step 9 landed and ungraded; step 5
   Tim's own and it ends the run. Step 10 partial after unit 387 - 10.1, 10.2, 10.4,
   10.5 met - and rev7 reworded 10.3 and added 10.6 this afternoon.
B. Step 10 - tonight's two. 10.6 <met|not>: <the control, where it sits, n favorites
   in the list, one click measured tuning from <dial> to <dial>>, the caret <gone|not>,
   the band <214|n> px at 1920 and 1400. 10.3 <met|partial>: the map <w> x <h> where
   it was 246 x 134, in a band of <h> px; stage A <held|not>, stage B <held|reverted,
   and why|dropped>.
C. The report last. Section 4 raises <N> items on top of the carried sixty, and
   <none of them is | item <k> is> in the way of a criterion in B. Say in one line
   what the pills row did when the map stood beside it - that is the night's finding
   whichever way it went.
```

```
UNIT:       388 - <complete|stopped> at task N of 5 - <date time>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   step 10, criteria <the ids you actually moved>
NUMBER:     the sun map's share of its band: 62.6% -> <n>%; favorites reached from
            the neighborhood card: 0 -> <n> presses to tune
DRIFT:      none
```

**Section 3 leads with the band, in a table, before and after, at 1920, 1400 and 1100 x 780**:
the pills, the card's header and padding, the map, the caption, `WithPills`, `WithoutPills`.
Then the width budget at 1400. Then task 1 item 4's quoted pre-`a51bc2a6` markup beside what
shipped tonight. Then the nine sizes against unit 381's. Then the entry and exit rounds, name
for name, and the carry-forward line's count before and after with every renamed name listed.

**Section 2 tells Tim in one paragraph, in his terms**: the map is now as tall as the strip it
sits in and the neighborhood side gave up the width, or exactly how far short it stopped and
why; and his favorites are one drop-down under the green zone again, where he had them, with
the star still saving. Every claim computed, not seen (FACT-004), and one line saying no port
was opened, nothing was enumerated and nothing was keyed.

---

```
ARBITER-DECISION
STEP: 10
APPROACH: move the sun map out of the neighborhood card's chrome into its own band-height column beside the card and then beside the pills row, width from the neighborhood side, and put favorites back as one drop-down under the green zone in place of unit 387's rig-face caret
MOVE: work around
WHY: 10.3 and 10.6 are the only criteria in this phase a unit can reach and Tim wrote both at 752a9b62 today (PHASE_PLAN.md step 10, rev7); unit 387 grew the map inside its card and the band followed it to 460 px, so tonight's route to 10.3 is a different mechanism - the map leaves the card's chrome so the band governs it - while step 0 stays closed under instruction 386 ruling 1, which no arbiter may overrule.
STATE: partial
DECIDED: author's and overrulable, two rulings, transcribed in work instruction 388 section 6. (1) 10.3's target is the band as Unit376TheTopBandTests.Band measures it, WithPills, 214 at 1920 and 1400: met when the map's height equals it within the band's own outer margins, the width taken from the neighborhood side and never the rig column; worked in two stages - A, the map out of the card's chrome and as tall as TopRow, partial on its own; B, the map beside the pills row, the named drop candidate, reverted in the same task if the pills wrap or the band grows - with the caption kept wholly drawn, the 246 x 134 floor and step 1's rules held at all nine sizes, and TheSunMapIsTheSizeItWasAndStillCarriesHisGrid rewritten under R12 because Tim replaced what it asserts. (2) Unit 387's caret guards in TheFavoritesAreBackOnTheRigDisplayTests are rewritten under R12 to assert 10.6, not deleted or skipped: the star-saving name unedited, the other four retargeted to the drop-down and to no caret anywhere on the rig face, the type renamed if its name is false, and the carry-forward line changed by name in the same commit with its count not falling. Also decided and not a ruling: step 10 over the handed step 0, because 0.1 is closed by instruction 386 ruling 1 and its unsearched device-resolution shape remains logged to Tim.
LICENCE: PHASE_PLAN.md step 10 rev7 criteria 10.3 and 10.6, and R46(a) and (c); R42, 214 stands; R34 and step 1's rules; HM-DEC-092, HM-DEC-070; PHASE_PLAN.md section 6 - a layout and a test's shape are the arbiter's, the later ruling wins, a must-pass missed by a little ships partial, never loosen a test; R12, R14, R31 two rulings a unit; ARBITER.md section 3's work around for 10.3, section 4 whose reading returned NOT FOUND and which I judge not a loop because the mechanism differs from unit 387's, and section 6; work instruction 386 section 6 ruling 1, not reopened; HM-DEC-018 section 2.1, HM-DEC-139, HM-DEC-155, HM-DEC-165; FACT-004
ACCOMPLISHED: Tim's sun map stands as tall as the band it lives in, with the neighborhood side giving up the width he said he has plenty of and the band not a pixel taller - or, if the pills row will not give way, as tall as the card row with the numbers saying exactly what stood in the way. And his favorites are back where his hand went for them before 2026-08-27: one drop-down reading Favorites under the green zone, one click to a saved spot, with the star still saving and the extra caret gone from the radio face.
ADVANCES: step 10 criterion 6, and step 10 criterion 3, with 10.3's stage B - the map beside the pills row - the named drop candidate.
END-ARBITER-DECISION
```
