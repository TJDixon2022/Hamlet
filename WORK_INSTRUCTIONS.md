# Work instruction 338 - the three working panels take the height

Step 0 of `PHASE_PLAN.md`, **second unit on it**. Unit 337 met step 0 at 1920 on its own
reading of *the working panels*. This unit holds the step to the mockup's reading, and it
closes the 1400 top row. **Four tasks, 0 to 3.**

**Status.** Write status before every `dotnet` command and after every task. `tools/status.sh`
has been refused as *requires approval* in units 332, 334 and 337. Try it once. If it is
refused, take a `date` reading and paste it whole. **Never compose a time.** The watchdog
polls the process and kills a session only when its process tree has used no CPU for ten
minutes.

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

**HM-DEC-155**, transcribed: *A unit runs no test suite. It may run only the test it
constructs in that work instruction, filtered by exact name, in the foreground, with a stated
timeout, and it never backgrounds a command and polls for it.* Three sessions died on
2026-09-05, each sitting in an `until grep … sleep 15` loop. **The suite was incidental; the
poll was fatal.** Run the carry-forward list as `docs\carry-forward-tests.txt`'s top comment
says: two invocations, one build each.

## 2. The tool fact

- Apostrophes inside quoted heredocs break.
- Doubled backslashes collapse.
- `;` is refused, and so are `rm` and `git stash`.
- Python cannot run here.
- A multi-line commit message needs more than one `-m`.
- The validator runs as `dotnet build tools/arbiter/validate-output.proj -p:Report=output.md`.
  The `.bat` spelling gets mangled by Git Bash.

## 3. Asks still outstanding

Per HM-DEC-139, unit 337's section 4 is carried **verbatim** into this unit's section 4. That
includes unit 336's queue, which 337 already carries inside it. **This unit answers two of
337's items, and the answers are rulings below:**

- **Item 1** is ruled in §6 (the rule of thumb).
- **Item 2** is ruled in §6 (what *the working panels* means).

Mark both items `ANSWERED by the arbiter's ruling in work instruction 338`, and say what was
built.

---

## 4. Why this unit exists

**The number: at 1920 the three working panels are 363 px, which is 0.399 of the 910 px below
the band pills. The mockup's panels are 382 of 710, or 0.538.** At 1400 the panels are 307 to
374 px (0.337 to 0.411), and the licensed operator's top row is 273 px (0.300, against the
mockup's 0.262). Every figure is unit 337's, computed headless at 1040 px tall and never seen.
The 1400 panel figure moves with run order. **Unit 337 did not find out why, so that spread
is contested until task 0 measures it.**

**Where the height went:**

| | Mockup | Built |
|---|---|---|
| Rows inside the working card above the panels | about 40 px: one row with the mode chips on the left and *5 messages out of one slot* on the right | about 197 px at 1920 |
| What is in those rows | the one row above | mode strip 32 px + margin 10; readiness strip 43; send area 54 with CQ and Stop; filter bar 34 with the slot clock, the filter and `clear` |
| Where the filter chips sit | in the *Decoded text* panel's header (`everything`, `CQ`, `newest first`, `clear`) | in the filter bar |

Unit 337 read *the working panels* as the working card, and it said a ruling was wanted only
if the panels themselves were meant. **They were.** Tim's complaint on unit 334's window was
that the waterfall, the decoded list and For You were squeezed, and the mockup gives the
panels themselves more than half.

```
PHASE GOAL: The screen, done right - the main window as the approved mockup,
            one short top row and the working panels given the height; then
            the achievements category pages as trading cards; then what the
            last phase left; then Tim at his window says it passed.
UNIT GOAL:  The waterfall, the decoded list and For You themselves take at
            least half the height below the band pills at 1920 and at 1400,
            and the 1400 top row on a licensed operator comes back to the
            mockup's proportion.
ADVANCES:   task 1 - step 0, must-pass 1 and 5 (the panels take the rest;
            at 1400 the same shape). Task 2 - step 0, must-pass 3 and 5 (the
            1400 top row; the rig at the card's height at 1400).
DRIFT:      0
```

---

## 5. Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim against the files and report any
mismatch. **Report the mismatch; do not repair the instruction.** Mismatches go in the report
even when the work succeeds anyway.

Check:

- `PHASE_STATUS.md` names *The screen, done right*, with step 0 `partial`.
- **In `MainWindow.axaml`:**
  - `DigitalWorkspace` has rows `Auto,Auto,Auto,*`.
  - `DigitalModeStrip` is in row 0 with a comment saying it does not collapse.
  - `DigitalReadinessStrip` is in row 1.
  - `DigitalPanes` is in row 3, `*,383,*` over `Auto,*`.
  - `DigitalSendReserved` holds `DigitalStopButton`.
  - The filter buttons are `DigitalFilterEverything` and `DigitalFilterCq`.
  - **Row 2 is not named here. Say what it holds.**
- `GreenZone.RuleOfThumb` in `src\Hamlet.App\ViewModels\GreenZone.cs` reads *Rule of thumb:
  20 m and up want daylight along the path; 40 m and down want dark; the gray edge is where
  both happen.*
- These test files exist under `tests\Hamlet.App.Tests\Views`: `TheTopRowTests`,
  `TheWorkingPanelsTests`, `TheFilterIsAlwaysThereTests` and `TheOperatorCanStopItTests`.
- **The reload's disagreements:**
  - `PROJECT_STATUS.md` says `RULES_AT: HM-DEC-161`, but `DECISIONS.md` carries HM-DEC-163.
    `PROJECT_STATUS.md` is the unit's own file, so write `RULES_AT` from the highest
    `HM-DEC` in `DECISIONS.md`.
  - `CLAUDE.md` §1's `CPS-DEC-0163` belongs to the id-scheme split. That is parked; report it
    and do not resolve it.
  - `PHASE_OUTCOME.md`, `PHASE_STATUS.md` and `RUN_LEDGER.md` are uncommitted. They are the
    launcher's writes. Commit them **unchanged** in task 0's commit, as units 336 and 337 did.
    Do not commit `.run-unit\`.

**Reds expected, older than this unit. Name them and do not chase them:**

- `TheOperatorCanStopItTests.TheStopAddedNoNewRouteToATransmission`: two `_armedSend.Arm(`
  lines in `MainWindowViewModel.cs`.
- `TheWholeChainRunsFromOneRightClickTests`, two tests: *realized row roots: 0*.
- `TheMenuIsUnderTheMouseTests`, eight tests: `DigitalMineRows` behind *show the messages*.
- `ThePsk31RecordsAppearTests.WithNoPsk31ContactNoPsk31CardIsOnTheScreenAndNoneIsDimmed`.
- `TheTotalMilesTests.TheBadgeSaysZeroMilesOnAnEmptyLogAndTheLowestTierIsNext`.

**If a red turns green or a new red appears, say which.**

---

## 6. Rulings in force - do not re-argue

**R26, Tim, 2026-09-12 (`PHASE_PLAN.md`), in full:**

> - **The top row is one band, about 190 px tall at 1920**, and the working panels below the
>   tabs take the rest of the window. At no window size do the working panels get less than
>   half the height below the band pills.
> - **The neighborhood card carries three things**: the band strip with the legend and the
>   *you · mode* marker; **the green block** - the band in the largest text, the frequency,
>   mode and *yours to use*, the license line small, the rule-of-thumb line small, and
>   heard-just-now with its count and sparkline; and **the world's clock** at its right end,
>   about 246 px wide, with the operator's dot only.
> - **The rig display is the same height as the neighborhood card**, and carries under the
>   frequency and S-meter the transmit drive and the RF power offer, so no empty column stands
>   under it.
> - **The band pills stay where they are** and are not repeated anywhere.
> - **Below the tabs**: waterfall, decoded text, For You, all the same height, full to the
>   status bar. The decoded list is as wide as its longest line needs and no wider; For You
>   takes the rest, wide enough that the card's facts sit beside its map.
> - **At 1400**: the same shape; where the card's facts cannot sit beside the map they go under
>   it; no callsign is ever clipped in the decoded list. **No mechanism is prescribed; the unit
>   measures and chooses, marks the choice as its own, and reports the numbers at both
>   widths.**

**`PHASE_PLAN.md` §6, the lines that bite here:**

- *A must-pass is missed by a little: ship, report, `partial`, move on. Never loosen a test.*
- *A string will not fit: shorten and say which, or widen; never clip.*
- *Anything touches the radio, a decoder, a parser or the transmit chain: `MOVE: stop`.*
- *A package is needed: `MOVE: stop`.*
- *A file must be deleted: empty it, comment it, list it.*

**The arbiter's rulings for this unit.** They are the author's, marked for Tim and overrulable.
`PHASE_PLAN.md` §R and §6 let the arbiter decide layout and continue.

1. **"The working panels" in R26 means the waterfall, decoded text and For You panels
   themselves**, measured from the top of the three panels to their shared bottom. It does not
   mean the working card they sit in.
   - *Why:* the mockup gives the panels 382 of the 710 px below the pills (0.538), and puts one
     row of about 40 px between the card's top and the panels.
   - *Rejected:* the working card, unit 337's reading 7. Under it, 197 px of strips count as
     working panels, and Tim's complaint was about the panels.
   - **This unit's measure:** the three panels are at least 0.5 of the height below the band
     pills, at 1920 and 1400, at 1040 px tall. It is measured with `DigitalReadinessStrip`
     hidden, which is the connected state the mockup draws. Report it with the strip showing
     as well.
2. **The rule of thumb is shortened to the mockup's own words:** *20 m and up want daylight
   along the path; 40 m and down want dark.* The *Rule of thumb:* prefix and *the gray edge
   is where both happen* both come off.
   - *Why:* it is unit 337's item 1, option (a). The mockup already draws that sentence, and
     §6 says shorten and name it.
   - *Rejected:* rewording the license line, which is `PrivilegeStatus.Detail`, the regulation's
     own sentence. Also rejected: hiding the rule behind a mark, and shrinking the world clock.
     Unit 337 gave the reasons, and they stand.
   - Update the `<remarks>` on `RuleOfThumb` to name work instruction 338 and the mockup.
3. **The sparkline may hide at widths where the green block's text column would otherwise
   wrap.** The count stays at every width. This is a fallback for task 2 and is used only if
   ruling 2 alone leaves the 1400 top row above 0.262. If it is used, the width rule is the
   unit's own; state it.

**Standing, transcribed:**

- **§0.0**: *Never present a guess as a decode… This binds pictures as hard as sentences.*
  Every appearance claim in this unit is computed, not seen, and says so once.
- **§0.2**, *transmit safety, ABSOLUTE*: *One operator action, one transmission.* What stays
  absolute is the abort and the one-click rule.
  - **For this unit:** CQ and Stop may move in markup only, on the same commands.
  - **Stop is never inside anything that collapses.** It is visible whenever the Digital tab
    is, at both widths.
- **§0.5**: *every panel is collapsible, and a collapsed panel still carries its summary.
  Collapsing hides detail, never information.*
  - Whatever leaves the send area or the filter bar keeps saying what it said. That includes
    `DigitalSendReservedLine`, *what went out*.
- **§R17, Tim, 2026-09-11**: *A collapsed Decoded text or For you header carries a live count
  and a sentence saying what is in it and that a click shows it… The CQ / Everything filter is
  visible on an empty list, because it is a choice about what to see, not a report of what is
  there.*
  - **If the filter moves into the decoded panel's header, it stays visible while that panel
    is collapsed.**
- **Tim, 2026-08-28, in the markup**: *the mode strip runs the full width and does not
  collapse. A row, not a column.*
- **§0.6**: every ink clears 4.5:1 against its fill; color is never the only carrier.
- **R12**: *a session fixes its own tests and never asks the owner to approve it.* A test that
  guards a layout this unit changes is rewritten in this unit's commit, and the test is named.
- **R13**: telemetry on every new stage. This unit adds no stage. If it adds one, it adds the
  event.
- **R14**: *a test exists to prove an exit criterion.* Write the tests this unit's criteria
  need and no others.
- **R19**: American spelling.

---

## 7. Status cadence

As the header says: before every `dotnet` command, after every task, and never composed.

---

## 8. The tasks

### Task 0 - the trace, and the before-numbers

Measure before anything is built. **Say what you find rather than confirming this
instruction's table.**

1. **Commit the launcher's root files unchanged** (see §5) with a patch bump, as
   `chore(unit338): step 0 again - the trace before a line is built`. Run the carry-forward
   list, status first.
2. **The rows between the tabs and the panels, top to bottom.** For each row, at 1920 and 1400
   and 1040 px tall, give:
   - its name, height and margin;
   - what it holds;
   - whether it can collapse;
   - whether it is conditional, and on what.

   This includes row 2, which §5 does not name.
3. **The three panels' height and share** at both widths, with the readiness strip shown and
   hidden, on the licensed fixture and on the plain one.
4. **The run-order spread.** Unit 337 measured the 1400 top row at 208 px alone and 229 px after
   the layout set, and the panels at 374 and 307. **Find the row that grows and why.** If the
   cause is state a test leaves behind, it is the test's to fix under R12; say so. A criterion
   whose number depends on run order proves nothing.
5. **The rig panel's height against the neighborhood card at 1400**, on the licensed fixture.
   Unit 337 did not measure it again after its band line changed.
6. **Where the band pills' *best bet now* check is drawn, and whether the green block joins it
   on the realized window.** This is the nice-to-pass.

Report the numbers in section 1 before task 1 starts.

**Drop candidate:** none.

### Task 1 - the three panels take the height

Win back the height between the tabs and the panels. **The target outcome is ruling 1: at
least half the height below the band pills, for the panels themselves, at both widths.**

The mockup's arrangement is the reference, not a prescription:

- one full-width row that does not collapse, with the mode chips on the left and a status on
  the right;
- the filter chips in the decoded panel's header;
- nothing else above the panels.

Where CQ, Stop, the slot clock and `DigitalSendReservedLine` go is **the unit's choice, marked
as its own**, within §6:

- Stop is never inside anything that collapses.
- The filter is visible on an empty list and while its panel is collapsed.
- Nothing that was said on screen stops being said.
- The mode strip does not collapse.

**Test watched failing first:** extend `TheWorkingPanelsTests`.

- At 1920 and 1400 (1040 tall, readiness strip hidden), the three panels share one top and one
  bottom, and their height is at least 0.5 of the height below the band pills.
- `DigitalStopButton` is visible and has no collapsible panel among its ancestors.
- The CQ / Everything filter is visible with the decoded list empty and with *Decoded text*
  collapsed.
- The decoded column still equals its stated need within 10 px, and no callsign is clipped.

Rewrite under R12, and name each, any of these that guard the old rows:

- `ThePanelsMakeRoomTests`
- `TheDigitalTabIsTwoColumnsTests`
- `TheFilterIsAlwaysThereTests`
- `ThePanelScrollsTests`
- `TheCardsRightColumnTests`

Run `TheOperatorCanStopItTests`, `BindingHealthTests` and `VoiceTests`. The known red named
in §5 stays exactly the one red.

**Drop candidate:** none. This is the step's largest remaining miss.

### Task 2 - the 1400 top row

Apply ruling 2. Measure the licensed fixture's top row at 1400. **If it is still above 0.262
of the height below the band pills, apply ruling 3** and state the width rule. If it is still
above after both, report the number and the next candidate. **Do not hide or shrink anything
R26 names.**

**Test watched failing first:** extend `TheTopRowTests`.

- At 1400 on the licensed fixture, the top row is at most 0.262 of the height below the band
  pills.
- The rig panel's height equals the neighborhood card's at 1920 and 1400, within 2 px.
- The rule-of-thumb line reads the mockup's sentence.
- The count is present at both widths.

Update `TheGreenZoneTests` under R12 if it asserts the old sentence.

**Drop candidate:** none.

### Task 3 - the best bet, on the window

**Drop candidate: this whole task.** It is the only nice-to-pass left on step 0.

Assert, on the realized window at 1920, that the band pill carrying *best bet now* and the
green block's band are the same band, and that the check is drawn as before. If task 0 found
it already holds, the test is the task. If it does not hold, fix the join in the view and say
what was broken.

### Then - the report

Section 3 puts the mockup's numbers beside the built numbers at both widths, region by region.
This is required, not a task, and it is not dropped.

---

## 9. Parked - do not touch, do not raise

- **Steps 1, 2 and 3.** The category pages, the States wording, the Modes test, the
  undeletable files and the points file. Each has its own step, and the arbiter authors them.
- **The old reds in §5.** Step 2 owns the small reds, and `TheOperatorCanStopItTests` touches
  the transmit side.
- **The width split `*,383,*` and the facts-under rule at 1400.** Both meet their criteria.
  Change them only if task 1 cannot win the height otherwise, and say so if it cannot.
- **The license line's wording.** It is the regulation's sentence (ruling 2, rejected).
- **The two id schemes, real flags on country cards, PSK31 step 6, real PSK31 audio, the ALC
  margin, the map bitmap's license.** They are Tim's, carried in `PHASE_PLAN.md` §7.
- **Anything touching the radio, a decoder, a parser or the transmit chain. Any package.**

## 10. What not to do

- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**
  (HM-DEC-155: these killed three sessions.)
- **Do not put Stop inside anything that collapses, and do not bind CQ or Stop to anything
  new.** §0.2 is the one rule on this screen that is the owner's.
- **Do not win height by hiding information.** §0.5: a row that leaves keeps its words
  somewhere on screen.
- **Do not loosen a threshold in this instruction to make a test pass.** A small miss ships
  as `partial` with its number (§6).
- **No image assets. No package. Report mismatches; repair nothing. Write American.**

## 11. Committing and pushing

Commit and push each task on its own, on `main`. The report names the commits and says
whether each push succeeded; **a refused push is reported as refused, with the reason.**

---

## 12. Reporting

Write `output.md` at the root, then stop. Do not start the next unit. **Every exit writes
it**: finished, blocked, failed or stopped early. Canonical headings: `## 1. What Claude did`,
`## 2. What the owner should expect`, `## 3. What you should see`, `## 4. What's blocking us`.
Validate it with `dotnet build tools/arbiter/validate-output.proj -p:Report=output.md`.

**The ordering block comes first.** `validate-output` refuses a report without it.

```
READ IN THIS ORDER.

A. The phase goal - the screen, done right. Step 0 <state after this unit>:
   <how many of its six must-pass met, and the nice-to-pass>. Steps 1 to 3
   not started.
B. Step 0 and its exit criteria, each met or not, with the number:
   1. top row about 190 px at 1920, and the three PANELS take at least half
      below the band pills - <panels px / share at 1920>
   2. the neighborhood card's three things - <unchanged, or what moved>
   3. rig panel at the card's height, at 1920 AND 1400 - <px against px>
   4. three panels equal height to the status bar, facts beside the map at 1920
   5. at 1400 the same shape: panels <px / share>, licensed top row
      <px / share against 0.262>, no callsign clipped, facts by the rule
   6. BindingHealthTests, VoiceTests, carry-forward - <counts>
   nice-to-pass: best bet joined to the green block on the window - <met or dropped>
C. The report last. Section 4 raises N items on top of the carried queue;
   say which, if any, stands in the way of a criterion in B.
```

**Every line specific to this unit.** If a criterion was not measured, say *not measured*.
Do not fill the shape.

```
UNIT:       338 - <complete|stopped> at task N of 4 - <date time, read from the clock>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   <yes|no> - <which step 0 criteria moved>
NUMBER:     three panels <before> -> <after> px (<share>) at 1920 and
            <before> -> <after> px (<share>) at 1400; licensed top row at 1400
            <before> -> <after> px (<share>)
DRIFT:      0
```

**Section 3 leads with the answer:** do the three panels themselves take at least half the
height below the band pills at both widths, and what is the licensed top row at 1400? Then
put the mockup's numbers beside the built ones, region by region, at both widths. Include
the rows between the tabs and the panels, before and after. **Every appearance claim is
computed, not seen. Say so once.**

**Section 4:** unit 337's section 4 verbatim, per HM-DEC-139, with items 1 and 2 marked as
answered (§3). Then anything this unit raises. **A ruling is wanted only where Tim must
decide; everything else is a finding and says so.**

---

```
ARBITER-DECISION
STEP: 0
APPROACH: fold the strips above the three working panels into one row as the mockup draws (filter into the decoded panel header, Stop never collapsible) so the panels themselves take half the height, and shorten the green block's rule of thumb to the mockup's words so the 1400 licensed top row returns to 0.262
MOVE: work around
WHY: Unit 337 met step 0 at 1920 by counting the working card as the working panels; measured against the mockup the three panels are 0.40 of the height where the picture gives them 0.54, and the 1400 top row is 0.300 against 0.262 - both are reachable by moving strips and shortening one string, so this is a different approach to the same step, not a loop and not a cut.
STATE: partial
DECIDED: R26's "working panels" means the three panels themselves, at least half the height below the band pills at 1920 and 1400 with the readiness strip hidden (overrides unit 337's reading 7); the rule of thumb becomes the mockup's sentence "20 m and up want daylight along the path; 40 m and down want dark."; the sparkline may hide at narrow widths only if that alone does not bring the 1400 top row to 0.262 - all the author's, marked for Tim, overrulable
LICENCE: PHASE_PLAN.md R26 and section 6 (shorten a string and name it; the arbiter decides layout and continues); assets/main-screen-mockup.png (panels 382 of 710 px below the pills); unit 337 output.md section 4 items 1 and 2; CLAUDE.md 0.2, 0.5; psk31 R12, R14, R17
ACCOMPLISHED: the waterfall, the decoded list and For You get more than half of the window below the band pills at both of Tim's widths, as in the picture he approved, and the top band stays short at 1400 too
ADVANCES: step 0 - must-pass 1 (the working panels take the rest), must-pass 3 (rig at the card's height, now at 1400) and must-pass 5 (the same shape at 1400), and the nice-to-pass if task 3 is not dropped
END-ARBITER-DECISION
```
