# Work instruction 339 - step 0, proved at both widths

Step 0 of `PHASE_PLAN.md`, **third unit on it**. Units 337 and 338 built the layout. This unit
proves what they built, criterion by criterion, at 1920 and at 1400, and names every test with its
result. Then it measures step 1's ground without building on it. **Three tasks, 0 to 2.**

**Status.** Write status before every `dotnet` command and after every task. `tools/status.sh`
has been refused as *requires approval* in units 332, 334, 337 and 338. Try it once. If it is
refused, take a `date` reading and paste it whole. **Never compose a time.** The watchdog polls
the process and kills a session only when its process tree has used no CPU for ten minutes.

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

**The named classes this instruction asks for are named by this instruction**, so each may
run filtered by its class name: `TheTopRowTests`, `TheWorkingPanelsTests`, `BindingHealthTests`,
`VoiceTests`, `TheOperatorCanStopItTests` and `TheAchievementsPageClicksInTests`. Join them in
one filter per project where you can, so there is one build, not six.

## 2. The tool facts

- Apostrophes inside quoted heredocs break.
- Doubled backslashes collapse.
- `;` is refused, and so are `rm` and `git stash`.
- Python cannot run here.
- A multi-line commit message needs more than one `-m`.
- A `sed` move of markup was refused in unit 338. Use exact-text edits.
- The validator runs as `dotnet build tools/arbiter/validate-output.proj -p:Report=output.md`.
  The `.bat` spelling gets mangled by Git Bash.

## 3. Asks still outstanding

Per HM-DEC-139, unit 338's section 4 is carried **verbatim** into this unit's section 4. That
includes the queue 338 already carries inside it. **This unit answers two of 338's items, and the
answers are rulings 4 and 5 in §6:**

- **Item 2**, the filter chips in the mode strip, is answered by ruling 4.
- **Item 3**, CQ and Stop right of the tabs, is answered by ruling 5.

Mark both items `ANSWERED by the arbiter's ruling in work instruction 339`. Nothing is built for
either one. What unit 338 built stands.

---

## 4. Why this unit exists

**The number: step 0 has six must-pass, and a separate reading of unit 338's report found three
of them without evidence.** Unit 338's own block B says all six are met. The session that read
that report against the exit criteria answered `partial`:

> for criterion 2 it only says the band strip and world clock are unchanged and shows nothing for
> the green block's band being its largest text, the clock's single dot or drive and power sitting
> under the S-meter, and BindingHealthTests and VoiceTests are not named with results while the
> layout set still has one red.

**What the tree says, read by the arbiter on 2026-09-12:**

- The tests exist, in `tests\Hamlet.App.Tests\Views\TheTopRowTests.cs`:
  - `TheGreenBlockIsInsideTheCardUnderTheStripWithTheBandLargest`
  - `TheWorldClockIsAtTheCardsRightEndWithOneMarker`
  - `DriveAndThePowerOfferAreUnderTheRigAndTheSendAreaKeepsCqAndStop`
- **All three realize the window at 1920 only.** Criterion 5 says *at 1400 the same shape holds*,
  and at 1400 unit 338 changed the green block: the sparkline hides and *heard just now* stands
  over the count. No test asserts the band is still the largest text, the clock still has one
  marker, or drive and power are still under the S-meter at 1400.
- Unit 338's report names the results of none of the three, and does not give
  `BindingHealthTests` or `VoiceTests` as counts of their own. Its *layout set 90 of 91* does not
  say which classes it holds.
- Unit 338 moved the send area to the tab row. The drive-and-power test also asserts that CQ and
  Stop are inside `DigitalSendReserved`. **Whether that test still passes after the move is not
  in the report.**

So the gap is evidence, not layout. **Every figure above comes from reports and from reading
the source. None was run by the arbiter.**

```
PHASE GOAL: The screen, done right - the main window as the approved mockup,
            one short top row and the working panels given the height; then
            the achievements category pages as trading cards; then what the
            last phase left; then Tim at his window says it passed.
UNIT GOAL:  Every step 0 exit criterion carries a named test with its result,
            at 1920 and at 1400, so the step's state is read from evidence
            rather than from a summary - then step 1's ground measured.
ADVANCES:   task 1 - step 0, must-pass 2, 3 and 5 (the card's three things,
            the rig display's drive and power, at 1400 as well as 1920) and
            must-pass 6 (BindingHealthTests and VoiceTests named with counts).
            Task 2 - none for step 0; it measures step 1's entry and exit
            ground and builds nothing. Task 2 is also the drop candidate.
DRIFT:      0
```

---

## 5. Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim against the files and report any
mismatch. **Report the mismatch; do not repair the instruction.** Mismatches go in the report
even when the work succeeds anyway.

Check:

- `PHASE_STATUS.md` names *The screen, done right*, with step 0 `partial`.
- `TheTopRowTests.cs` holds the six tests named in §4 and in task 1. The first three realize only
  1920. `AtFourteenHundredTheLicensedTopRowIsTheMockupsShare` realizes both widths.
- `TheWorkingPanelsTests.cs` holds `TheThreePanelsShareOneTopAndOneBottom`,
  `TheDecodedListIsAsWideAsItsLongestLineNeeds`, `AtNineteenTwentyTheCardsFactsSitBesideTheMap`,
  `NoCallsignIsClipped`, `AtFourteenHundredTheSameShapeHolds`,
  `TheThreePanelsTakeAtLeastHalfTheHeightBelowTheBandPills`,
  `StopNeverCollapsesAndTheFilterStaysOnAnEmptyOrCollapsedList` and the trace
  `Unit338TraceTheRowsAboveThePanels`.
- `BindingHealthTests` is at `tests\Hamlet.App.Tests\Views\BindingHealthTests.cs`, with
  `TheMainWindowBindsWithoutOneComplaint`. `VoiceTests` is at `tests\Hamlet.App.Tests\VoiceTests.cs`,
  with five tests.
- In `MainWindow.axaml`: `RigDriveAndPower`, `GreenZoneBand`, `GreenZoneRegions`,
  `DigitalSendReserved`, `DigitalHeaderStrip` and `DigitalReadinessStrip` exist.
- `GreenZone.RuleOfThumb` reads *20 m and up want daylight along the path; 40 m and down want
  dark.*
- **The reload's disagreements:**
  - **The reload says `CLAUDE.md` §1 holds `CPS-DEC-0163`.** The arbiter counted `CPS-DEC` in
    `CLAUDE.md` and found none. Its 2026-09-12 row carries HM-DEC-163, as unit 338 also found. So
    the reload's check is misreading the file. That is the harness's, and the id schemes are
    parked: report it and do not resolve it.
  - `PROJECT_STATUS.md` says `RULES_AT: HM-DEC-163`, and the highest `HM-DEC` in `DECISIONS.md`
    is 163. They agree. Keep it unless this unit records a decision.
  - `PHASE_OUTCOME.md`, `PHASE_STATUS.md` and `RUN_LEDGER.md` are uncommitted. They are the
    launcher's writes. Commit them **unchanged** in task 0's commit with `WORK_INSTRUCTIONS.md`,
    as units 336 to 338 did. Do not commit `.run-unit\`.

**Reds expected, older than this unit. Name them and do not chase them:**

- `TheOperatorCanStopItTests.TheStopAddedNoNewRouteToATransmission`: two `_armedSend.Arm(`
  lines in `MainWindowViewModel.cs`. **It is not on criterion 6's list.** It is a transmit-side
  test, and step 0 does not own it. Run it, and report it apart from criterion 6's counts.
- `TheWholeChainRunsFromOneRightClickTests`, two tests: *realized row roots: 0*. Not run.
- `TheMenuIsUnderTheMouseTests`, eight tests: `DigitalMineRows` behind *show the messages*. Not run.
- `ThePsk31RecordsAppearTests.WithNoPsk31ContactNoPsk31CardIsOnTheScreenAndNoneIsDimmed`. Not
  run.
- `TheTotalMilesTests.TheBadgeSaysZeroMilesOnAnEmptyLogAndTheLowestTierIsNext`. Not run.

**If a red turns green or a new red appears, in what you ran, say which.**

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

**R22, Tim (`docs/phase-maintenance-run/PHASE_PLAN.md`):** the category pages are trading cards,
as `assets/category-page-countries.png`. Task 2 only reads it; read R22 there in full before task 2.

**`PHASE_PLAN.md` §6, the lines that bite here:**

- *A must-pass is missed by a little: ship, report, `partial`, move on. Never loosen a test.*
- *A string will not fit: shorten and say which, or widen; never clip.*
- *Anything touches the radio, a decoder, a parser or the transmit chain: `MOVE: stop`.*
- *A package is needed: `MOVE: stop`.*
- *A file must be deleted: empty it, comment it, list it.*

**The arbiter's rulings, work instruction 338, still in force.** They are the author's, marked for
Tim and overrulable.

1. **"The working panels" in R26 means the waterfall, decoded text and For You panels
   themselves**, measured from the top of the three panels to their shared bottom. It does not
   mean the working card they sit in.
   - *Why:* the mockup gives the panels 382 of the 710 px below the pills (0.538).
   - *Rejected:* the working card, unit 337's reading 7. Under it, 197 px of strips counted as
     working panels, and Tim's complaint was about the panels.
   - **The measure:** the three panels are at least 0.5 of the height below the band pills, at 1920
     and 1400, at 1040 px tall. It is measured with `DigitalReadinessStrip` hidden, which is the
     connected state the mockup draws. It is reported with the strip showing as well.
2. **The rule of thumb is the mockup's own words:** *20 m and up want daylight along the path; 40
   m and down want dark.*
   - *Rejected:* rewording the license line, which is the regulation's own sentence. Also
     rejected: hiding the rule behind a mark, and shrinking the world clock.
3. **The sparkline may hide at widths where the green block's text column would otherwise wrap.**
   The count stays at every width. Unit 338 applied it and stated the width rule
   (`MainWindow.FitTheHeardCount`). It stands.

**The arbiter's rulings for this unit.** They are the author's, marked for Tim and overrulable.
`PHASE_PLAN.md` §R and §6 let the arbiter decide layout and continue.

4. **The filter chips stay in the mode strip** (unit 338 item 2, option (a)).
   - *Why:* §R17 wants the filter visible on an empty list and a shut header carrying its count
     and sentence. The mode strip does not collapse, so both hold at every width. On the host,
     the four controls want about 430 px against the Decoded text header's 376.
   - *Rejected:* (b) two chips in the header, which leaves the title and summary about 50 px.
     (c) Shortening the chips' words, which would rename a choice to win a placement the mockup
     only draws.
5. **CQ and Stop stay right of the tabs** (unit 338 item 3).
   - *Why:* there they cost no height, sit outside everything that folds, and show whenever the
     Digital tab does. They moved in markup only and bind the same commands, so §0.2's one-click
     rule and abort are untouched.
   - *Rejected:* the For you or waterfall panel, because both collapse. The status bar, because
     every tab shares it and it would grow.
   - **If Tim wants them elsewhere, that is his, at step 3.**

**Standing, transcribed:**

- **§0.0**: *Never present a guess as a decode… This binds pictures as hard as sentences.*
  Every appearance claim in this unit is computed, not seen, and says so once.
- **§0.2**, *transmit safety, ABSOLUTE*: *One operator action, one transmission.* **This unit
  moves nothing.** CQ, Stop, the drive and the power offer are read, never changed.
- **§0.5**: *every panel is collapsible, and a collapsed panel still carries its summary.
  Collapsing hides detail, never information.*
- **§0.6**: every ink clears 4.5:1 against its fill; color is never the only carrier.
- **R12**: *a session fixes its own tests and never asks the owner to approve it.* If a test this
  unit extends guards something 338 changed and is now wrong about it, rewrite it in this unit's
  commit and name it.
- **R13**: telemetry on every new stage. This unit adds no stage.
- **R14**: *a test exists to prove an exit criterion.* Extend the tests the criteria need and add
  no others.
- **R19**: American spelling.

---

## 7. Status cadence

As the header says: before every `dotnet` command, after every task, and never composed.

---

## 8. The tasks

### Task 0 - the trace: every step 0 test, run and named

Measure before anything is built. **Say what you find rather than confirming §4.**

1. **Commit the launcher's root files unchanged** (see §5) with `WORK_INSTRUCTIONS.md` and a patch
   bump, as `chore(unit339): step 0, proved - the trace before a line is built`. Run the
   carry-forward list, status first.
2. **Run `TheTopRowTests`, `TheWorkingPanelsTests`, `BindingHealthTests`, `VoiceTests` and
   `TheOperatorCanStopItTests`**, in one `Hamlet.App.Tests` filter. For **every test method**, give:
   - its name;
   - pass or fail;
   - the step 0 criterion it proves, 1 to 6 or the nice-to-pass;
   - the width or widths it realizes;
   - the numbers it prints.
3. **Say which criterion, at which width, no test asserts.** §4 expects criteria 2 and 3 at 1400.
   Say whether that is true, and whether anything else is missing.
4. **The readiness strip showing, at 1400, on the licensed fixture.** Give the three panels' px
   and share. Unit 338 left it *not measured*. At 1920 it was 450 px (0.495).

Report the table in section 1 before task 1 starts.

**Drop candidate:** none.

### Task 1 - criteria 2 and 3 at 1400, asserted

Extend the three tests in `TheTopRowTests` so each runs **at 1920 and at 1400** on the licensed
fixture, with the assertions it already makes:

- **`TheGreenBlockIsInsideTheCardUnderTheStripWithTheBandLargest`:**
  - the band is the largest text in the card at both widths;
  - the count is in the green block at both widths;
  - the sparkline is asserted where ruling 3 keeps it. At 1400, where ruling 3 hides it,
    *heard just now* and the count stand in its place. Assert what the width rule draws, not
    a fixed width.
- **`TheWorldClockIsAtTheCardsRightEndWithOneMarker`:** at both widths, the clock is at the card's
  right end, at the mockup's size, with one marker.
- **`DriveAndThePowerOfferAreUnderTheRigAndTheSendAreaKeepsCqAndStop`:**
  - drive and the power offer are in the rig panel under the S-meter at both widths;
  - the rig panel is the card's height at both widths;
  - CQ and Stop are where ruling 5 leaves them.

If task 0 found one of these red at 1920, **say why before you touch it**. A red caused by 338's
move is rewritten under R12 and named. A red in the view is a miss on criterion 2 or 3: fix it in
markup only, and say what was broken.

**Watched failing first, where possible.** If an extension passes on its first run, say *not
watched red*, and say why that is honest: the layout already holds, and the test is the task. **Do
not break the view to watch a test fail.**

Then run `BindingHealthTests` and `VoiceTests` and give each as *n of n*. Run the carry-forward
list again, status first.

**Drop candidate:** none. This is the evidence the step's state is waiting for.

### Task 2 - step 1's ground, measured and not built

**Drop candidate: this whole task.** Step 1's entry is *step 0 done*, and that verdict is not this
unit's to give. So this task **changes no file under `src\`** and adds no test. It measures only,
so step 1's first unit starts from numbers.

1. **Step 1's entry check:** run `TheAchievementsPageClicksInTests`. Give each method with its
   result.
2. **Step 1's exit criteria, each against the tree as it stands today**, with where in the code it
   lives and what is missing:
   - the color band with count, score, level and a bar to the next level;
   - an earned card's entity, callsign, grid, path map cropped to the two stations, distance, band,
     mode, date and points, from the log entry that earned it;
   - the next card's want and its CQ callers with distance, or *no one is calling from there now*;
   - all eight kinds, with Continents opening to seven badges and each to its countries;
   - no clipped or word-wrapped string at 1400 and 1920;
   - no white-rectangle card;
   - `achievement_category_opened` carrying the kind and the count of cards;
   - the nice-to-pass, the card's map opening in the popup on click.
3. **The Countries page beside `assets/category-page-countries.png`, region by region.** Give what
   the mockup draws, what the tree draws, and the gap, computed rather than seen.
4. **Name the three largest gaps, by size of build.** Do not propose the approach. The arbiter
   authors step 1.

---

## 9. Parked - do not touch, do not raise

- **Every layout mechanism units 337 and 338 chose:** the `*,383,*` split, the facts-under rule at
  1400, the send area on the tab row, the filter in the mode strip, the sparkline width rule. Each
  meets its criterion, and rulings 1 to 5 hold them. Change one only if task 1 finds a criterion
  red because of it, and say so.
- **The live callook.info lookup in the plain fixture** (338 item 1) and **the heard count reading
  8 or 9 against 6** (338 item 4). Both are real. Neither is in the way of a criterion, because the
  licensed fixture carries the 1400 assertions, and a seam for them changes the view model's
  start-up, which is not a screen step. If a task 1 assertion turns flaky on either, raise it
  once, and say it was parked.
- **Steps 1, 2 and 3 as builds.** Task 2 reads step 1 and writes nothing. The States wording, the
  Modes test, the undeletable files, the points file and the small reds are step 2's.
- **The old reds in §5.** Step 2 owns the small reds, and `TheOperatorCanStopItTests` touches the
  transmit side.
- **The license line's wording.** It is the regulation's sentence.
- **The two id schemes, including the reload's `CPS-DEC-0163` misreading.** Also real flags on
  country cards, PSK31 step 6, real PSK31 audio, the ALC margin and the map bitmap's license. All
  are Tim's, carried in `PHASE_PLAN.md` §7.
- **Anything touching the radio, a decoder, a parser or the transmit chain. Any package.**

## 10. What not to do

- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**
  (HM-DEC-155: these killed three sessions.)
- **Do not move CQ, Stop, the drive or the power offer, and bind nothing new to them.** This unit
  proves where they are. §0.2 is the owner's.
- **Do not loosen a threshold to make a test pass, and do not break the view to watch one fail.**
  A small miss ships as `partial` with its number (§6). A test that was never red says so.
- **Do not build step 1 in task 2.** A step built before its entry holds is built on a verdict
  nobody gave. `CLAUDE.md` §12.6 covers the rest.
- **No image assets. No package. Report mismatches; repair nothing. Write American.**

## 11. Committing and pushing

Commit and push each task on its own, on `main`. Task 2 commits only if it leaves a file, such as
the report. The report names the commits and says whether each push succeeded. **A refused push
is reported as refused, with the reason.**

---

## 12. Reporting

Write `output.md` at the root, then stop. Do not start the next unit. **Every exit writes
it**: finished, blocked, failed or stopped early. Canonical headings: `## 1. What Claude did`,
`## 2. What the owner should expect`, `## 3. What you should see`, `## 4. What's blocking us`.
Validate it with `dotnet build tools/arbiter/validate-output.proj -p:Report=output.md`.

**The ordering block comes first.** `validate-output` refuses a report without it.

```
READ IN THIS ORDER.

A. The phase goal - the screen, done right. Step 0 <state as the evidence
   stands>: <how many of its six must-pass carry a green named test at
   both widths, and the nice-to-pass>. Step 1 not started; its ground
   <measured in task 2 | not measured, task 2 dropped>. Steps 2 and 3
   not started.
B. Step 0 and its exit criteria, each with the test that proves it, its
   result and the widths it realized:
   1. top row about 190 px at 1920; the three PANELS at least half below
      the pills - <test, result, px / share; strip showing px / share>
   2. the card's three things - band largest text, clock one marker, the
      count (and the sparkline where ruling 3 keeps it) - <test, result,
      1920 AND 1400>
   3. rig panel the card's height, drive and power under the S-meter -
      <test, result, px against px at 1920 AND 1400>
   4. three panels equal height to the status bar, facts beside the map
      at 1920 - <test, result>
   5. at 1400 the same shape, no callsign clipped, facts by the rule,
      licensed top row <px / share against 0.262> - <tests, results>
   6. BindingHealthTests <n of n>, VoiceTests <n of n>, carry-forward app
      <n of n> and engine <n of n>
   nice-to-pass: best bet joined to the green block - <test, result>
   Not on criterion 6's list: TheStopAddedNoNewRouteToATransmission <result>.
C. The report last. Section 4 raises N items on top of the carried queue;
   say which, if any, stands in the way of a criterion in B.
```

**Every line specific to this unit.** If a criterion was not measured, say *not measured*.
Do not fill the shape.

```
UNIT:       339 - <complete|stopped> at task N of 3 - <date time, read from the clock>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   <yes|no> - <which step 0 criteria gained a named test at 1400,
            and whether any was found red>
NUMBER:     step 0 criteria with a green named test at both widths: <before>
            -> <after> of 6
DRIFT:      0
```

**Section 3 leads with the answer:** does every step 0 exit criterion now have a named test
that is green at the widths the criterion names? If not, which criterion, at which width, and
by how much? Then the table from task 0 as it stands after task 1: test, criterion, widths,
result, numbers. Then, if task 2 ran, step 1's criteria against the tree, and the Countries page
against its mockup. **Every appearance claim is computed, not seen. Say so once.**

**Section 4:** unit 338's section 4 verbatim, per HM-DEC-139. Mark items 2 and 3 as answered
(§3). Then anything this unit raises. **A ruling is wanted only where Tim must decide; everything
else is a finding and says so.**

---

```
ARBITER-DECISION
STEP: 0
APPROACH: prove step 0 criterion by criterion on the realized window - extend the green block, world clock and drive-and-power tests from 1920 to 1400, run and name every step 0 test with its result and BindingHealthTests and VoiceTests as counts - then measure step 1's category page against its mockup without building
MOVE: work around
WHY: Two units built the layout and the state reader still found step 0 partial on evidence, not on the screen: criteria 2 and 3 are asserted only at 1920, and criterion 6's classes were never named with counts. A different approach to the same step - proof instead of more layout - not a loop (the loop test found nothing like it) and not a cut, since nothing measured is missing.
STATE: partial
DECIDED: the filter chips stay in the mode strip (unit 338 item 2, option a) and CQ and Stop stay right of the tabs (unit 338 item 3); the readiness-strip-hidden measure of ruling 1 stands, with the strip-showing number reported at 1400; step 1 may be measured before step 0's verdict, but not built - all the author's, marked for Tim, overrulable
LICENCE: PHASE_PLAN.md R26, sections 4 (step 0 exit, step 1 entry) and 6 (the arbiter decides layout and continues); unit 338 output.md section 4 items 2 and 3 and their stated reasoning; PHASE_OUTCOME.md unit 338 STATE_WHY; CLAUDE.md 0.2, 0.5; R17; psk31 R12, R14
ACCOMPLISHED: every promise the approved mockup makes about the main window is checked by a named test at both of Tim's widths, so step 0 is closed on evidence and step 1 starts from measured numbers
ADVANCES: step 0 - must-pass 2 and 3 at 1400 (and so must-pass 5, the same shape at 1400) and must-pass 6 named with counts, all in task 1; task 2 advances no step 0 criterion and is the drop candidate
END-ARBITER-DECISION
```
