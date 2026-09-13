# Work instruction 340 - step 0, the power offer drawn

Step 0 of `PHASE_PLAN.md`, **fourth unit on it**. Units 337 and 338 built the layout. Unit 339 put a
named test behind every criterion, and the state reader still answered `partial` on one clause. The
power offer was never drawn in any test, so nothing measured it sitting under the S-meter. This unit
draws it and measures it at both widths. If the top row cannot hold it, the unit fits it. It also
asserts the one clause unit 339 left printed rather than asserted. **Three tasks, 0 to 2.**

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

## 1. Why this unit exists

**The number: step 0's six must-pass carry a green named test at both widths, but two sub-clauses
are held by containment or by a print, not by a measured rectangle.** Unit 339 said so itself
(section 4, item 4). The state reader that read its report answered `partial`, naming one:

> criterion 3 is not fully shown, because the power offer is not drawn on the FT8 fixture and is
> checked only as being inside the rig panel, so nothing measures it sitting under the S-meter at
> either width.

**What the tree says, read by the arbiter on 2026-09-12:**

- `MainWindowViewModel.HasPsk31PowerOffer` is `IsPsk31Chosen && !_psk31PowerSettled`. The offer
  draws when PSK31 is the chosen digital mode and the operator has not answered it. **No transmission
  and no radio is needed to draw it.**
- `TheTopRowTests.Realized` sets `ChosenDigitalMode = "FT8"`. So every top-row test to date has
  measured the rig panel **without the offer in it**.
- The offer is a `Border` inside `RigDriveAndPower`, holding three things: a wrapped sentence of
  about 190 characters (`MaxWidth` 496), the accept and decline buttons, and the ALC reference line.
  **Whether the rig panel, and so the top row, stays at the card's height and about 190 px with that
  block drawn has never been measured.** The mockup draws the offer as one short line: *Transmit
  drive 30 % · RF power 50 % offered*. The tree's offer is far taller than that.
- `TheWorkingPanelsTests.TheThreePanelsShareOneTopAndOneBottom` realizes 1400 and 1920. It skips
  every assertion below 1900 (`if (width < 1900) continue;`). So *full to the status bar* at 1400 is
  printed, not asserted. Unit 339 printed y 953 against a floor of y 953.

**So the risk is real, not clerical.** In PSK31 the top row may be taller than the mockup, and no test
would say so. **Every figure above comes from the source and from unit 339's report. The arbiter ran
none of it.**

```
PHASE GOAL: The screen, done right - the main window as the approved mockup,
            one short top row and the working panels given the height; then
            the achievements category pages as trading cards; then what the
            last phase left; then Tim at his window says it passed.
UNIT GOAL:  The power offer drawn in the rig panel and measured under the
            S-meter at 1920 and 1400, with the top row still the mockup's
            height while it shows - fitted in markup if it is not - and the
            panels asserted full to the status bar at 1400.
ADVANCES:   step 0, must-pass 3 (the power offer under the S-meter, drawn,
            at both widths) and must-pass 1 and 5 as they hold while the
            offer shows; must-pass 5's "full to the status bar" at 1400.
            Task 2 advances no step 0 criterion; it runs step 1's own tests
            and is the drop candidate.
DRIFT:      0
```

---

## 2. Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim against the files and report any
mismatch. **Report the mismatch; do not repair the instruction.** Mismatches go in the report even
when the work succeeds anyway.

Check:

- `PHASE_STATUS.md` names *The screen, done right*, with step 0 `partial`.
- `HasPsk31PowerOffer` reads as §1 says (near line 14874 of `MainWindowViewModel.cs`).
  `OnChosenDigitalModeChanged` raises it (near line 380).
- In `MainWindow.axaml`, `RigDriveAndPower` holds `DigitalTransmitDriveBox` and a `Border` bound to
  `HasPsk31PowerOffer`. That border holds `DigitalPsk31PowerOffer`, `DigitalPsk31PowerAccept`,
  `DigitalPsk31PowerDecline` and `DigitalPsk31AlcReference`.
- `TheTopRowTests.Realized` sets FT8 (near line 647).
  `DriveAndThePowerOfferAreUnderTheRigAndTheSendAreaKeepsCqAndStop` asserts the offer by
  containment only (near line 328).
- `TheThreePanelsShareOneTopAndOneBottom` skips its assertions below 1900 (near line 93).
- `OnChosenDigitalModeChanged` calls `SettingsStore.Save`. **Say where that writes under the test
  host.** If it is the operator's real settings file, say so as a finding. Also restore FT8 before
  each PSK31 window closes (task 1). Whatever the older PSK31 tests on the carry-forward list already
  do there, do not chase it.
- `TheCategoryPagesAreTradingCardsTests` holds `EveryKindsBandCarriesCountScoreLevelAndABar`,
  `EveryEarnedCardIsTheContactThatEarnedIt`, `TheNextCardKnowsWhoIsCalling`,
  `TheOtherFiveKindsEachDrawTheirOwnCards`, `NoStringClipsAndNoCardIsWhiteAtFourteenHundredAndNineteenTwenty`
  and `StatesCountWhatTheLogsStateFieldSays`.
- **The reload's disagreements:**
  - **The reload says `CLAUDE.md` §1 holds `CPS-DEC-0163`.** Unit 339 counted `CPS-DEC` in
    `CLAUDE.md` and found it 0 times. The reload's check misreads the file. That is the harness's
    problem, and the id schemes are parked: report it again in one line and do not resolve it.
  - `PROJECT_STATUS.md` `RULES_AT: HM-DEC-163` agrees with `DECISIONS.md`. Keep it unless this unit
    records a decision.
  - `PHASE_OUTCOME.md`, `PHASE_STATUS.md` and `RUN_LEDGER.md` are uncommitted. They are the
    launcher's writes. Commit them **unchanged** in task 0's commit with `WORK_INSTRUCTIONS.md`, as
    units 336 to 339 did. Do not commit `.run-unit\`.

**Reds expected, older than this unit. Name them and do not chase them:**

- `TheOperatorCanStopItTests.TheStopAddedNoNewRouteToATransmission`: two `_armedSend.Arm(` lines.
  Not on criterion 6's list.
- Three `TheOperatorCanStopItTests` went red **only when joined** with the layout classes in unit 339,
  and passed alone: `AClickBeforeTheBoundaryUnarmsItAndNothingGoesOut`,
  `TheLineSaysWhatHappenedToTheCarrierAndToTheSound` and
  `AClickWhileTheToneIsPlayingStopsTheSoundAndNotJustTheCarrier`. **This unit runs that class in its
  own filter** (task 0), never joined.
- `TheWholeChainRunsFromOneRightClickTests` (2), `TheMenuIsUnderTheMouseTests` (8),
  `ThePsk31RecordsAppearTests.WithNoPsk31ContactNoPsk31CardIsOnTheScreenAndNoneIsDimmed`,
  `TheTotalMilesTests.TheBadgeSaysZeroMilesOnAnEmptyLogAndTheLowestTierIsNext`. Not run.

**If a red turns green or a new red appears, in what you ran, say which.**

---

## 3. Rulings in force - do not re-argue

**HM-DEC-155**, transcribed: *A unit runs no test suite. It may run only the test it constructs in
that work instruction, filtered by exact name, in the foreground, with a stated timeout, and it never
backgrounds a command and polls for it.* Run the carry-forward list as `docs\carry-forward-tests.txt`'s
top comment says: two invocations, one build each. **The classes this instruction names may each run
filtered by class name:** `TheTopRowTests`, `TheWorkingPanelsTests`, `BindingHealthTests` and
`VoiceTests` in one filter; `TheOperatorCanStopItTests` in a filter of its own;
`TheCategoryPagesAreTradingCardsTests` in a filter of its own.

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

**R22, Tim (`docs/phase-maintenance-run/PHASE_PLAN.md`):** the category pages are trading cards, as
`assets/category-page-countries.png`. Task 2 only runs its tests. It builds nothing.

**`PHASE_PLAN.md` §6, the lines that bite here:**

- *A must-pass is missed by a little: ship, report, `partial`, move on. Never loosen a test.*
- *A string will not fit: shorten and say which, or widen; never clip.* **Ruling 7 below narrows
  this for the offer's own words.**
- *Anything touches the radio, a decoder, a parser or the transmit chain: `MOVE: stop`.*
- *A package is needed: `MOVE: stop`.*
- *A file must be deleted: empty it, comment it, list it.*

**The arbiter's rulings, work instructions 338 and 339, still in force.** They are the author's,
marked for Tim and overrulable.

1. **"The working panels" in R26 means the waterfall, decoded text and For You panels themselves.**
   They take at least 0.5 of the height below the band pills, at 1920 and 1400, at 1040 px tall. This
   is measured with `DigitalReadinessStrip` hidden and also reported with it showing.
2. **The rule of thumb is the mockup's own words:** *20 m and up want daylight along the path; 40 m
   and down want dark.*
3. **The sparkline may hide at widths where the green block's text column would otherwise wrap.** The
   count stays at every width (`MainWindow.FitTheHeardCount`).
4. **The filter chips stay in the mode strip.**
5. **CQ and Stop stay right of the tabs.** If Tim wants them elsewhere, that is his call, at step 3.

**The arbiter's rulings for this unit.** They are the author's, marked for Tim and overrulable.
`PHASE_PLAN.md` §R and §6 let the arbiter decide layout and continue.

6. **Criterion 3's power offer is measured in the mode that offers it: PSK31.**
   - *Why:* §R11 puts the offer on PSK31 alone, because PSK31 is the continuous carrier.
     `OnChosenDigitalModeChanged` says so in its comment. On FT8 the drive fills the space under the
     S-meter, so no empty column stands there.
   - *Rejected:* offering power on FT8 because the mockup draws *RF power 50 % offered* on an FT8
     screen. That changes when Hamlet offers to write the radio's power, which is not a screen step.
     If Tim wants the offer on every mode, that is his call, at step 3.
7. **If the offer makes the top row taller than the criteria allow, fit it by arrangement, in markup,
   inside `RigDriveAndPower`.** Examples are the offer's column beside the drive, its `MaxWidth`, or
   its padding. The unit chooses, marks the choice as its own, and reports the numbers at both widths.
   - **The offer's words, the ALC reference line's words, both buttons' commands and bindings, and
     `HasPsk31PowerOffer` are not changed.** None of them goes behind a hover.
   - *Why:* HM-DEC-084 makes the offer say what would change and what would not, before the press.
     §R15 keeps the ALC reference never blank. Shortening a sentence about a write to the radio is
     not a layout choice.
   - **If arrangement alone cannot hold the criteria, ship what arrangement reaches, report the miss
     with its numbers, and name the options in section 4.** The arbiter decides next.

**Standing, transcribed:**

- **§0.0**: *Never present a guess as a decode… This binds pictures as hard as sentences.* Every
  appearance claim is computed, not seen, and says so once.
- **§0.2**, *transmit safety, ABSOLUTE*: *One operator action, one transmission.* **This unit presses
  nothing.** The tests read where the drive, the offer, CQ and Stop are drawn. They never execute
  `AcceptPsk31PowerCommand`, `DeclinePsk31PowerCommand`, CQ or Stop.
- **§R11 / HM-DEC-084** (as the markup's own comment states them): the power offer is *offered, never
  mirrored and never written silently*, and *pressing nothing changes nothing*.
- **§0.5**: *every panel is collapsible, and a collapsed panel still carries its summary. Collapsing
  hides detail, never information.*
- **§0.6**: every ink clears 4.5:1 against its fill; color is never the only carrier.
- **R12**: *a session fixes its own tests and never asks the owner to approve it.*
- **R13**: telemetry on every new stage. This unit adds no stage.
- **R14**: *a test exists to prove an exit criterion.* Extend the tests the criteria need and add no
  others. A trace method that asserts nothing may be added in task 0, as unit 338's was.
- **R19**: American spelling.

---

## 4. Status cadence

Write status before every `dotnet` command and after every task. `tools/status.sh` has been refused
as *requires approval* in five units. Try it once. If it is refused, take a `date` reading and paste
it whole. **Never compose a time.** The script also hard-codes a stale `RULES_AT`, so do not let it
overwrite `HM-DEC-163` (unit 339 item 3). The watchdog kills a session only when its process tree has
used no CPU for ten minutes.

---

## 5. The tasks

### Task 0 - the trace: the offer drawn, measured before anything moves

Measure before anything is built. **Say what you find rather than confirming §1.**

1. **Commit the launcher's root files unchanged** (§2) with `WORK_INSTRUCTIONS.md` and a patch
   bump, as `chore(unit340): step 0, the power offer drawn - the trace before a line is built`. Run
   the carry-forward list, status first.
2. **Add `Unit340TraceThePowerOfferOnPsk31` to `TheTopRowTests`.** It asserts nothing. It realizes
   the licensed fixture with `ChosenDigitalMode = "PSK31"` at 1920 and at 1400, and prints:
   - the rig display, `DigitalTransmitDriveBox` and the offer's border;
   - the offer's sentence (its line count), the accept and decline buttons, and the ALC reference
     line (its line count);
   - the rig panel, the card, and the top row's height and share of the height below the pills;
   - the three panels' px and share, with the readiness strip hidden and showing.

   Then print the same at FT8, from the same method, so the two modes sit side by side.
3. **Answer from the numbers, at each width:**
   - Is the offer drawn, visible and non-zero?
   - Is its top at or below the rig display's bottom?
   - Is the rig panel still the card's height?
   - Is the top row still about 190 px at 1920, and at or under 0.262 at 1400?
   - Are the panels still at least 0.5 below the pills?

   **If every answer is yes, task 1 is assertions only.** If any is no, give the miss in px and say
   which box made it.
4. **Run `TheOperatorCanStopItTests` in its own filter.** Give it as *n of 9*, naming each red. This
   says whether unit 339's three extra reds come only from the joint run.

Report the numbers in section 1 before task 1 starts.

**Drop candidate:** none.

### Task 1 - criterion 3 with the offer drawn, and criterion 5's floor at 1400, asserted

1. **If task 0 found a miss, fit it first, by ruling 7.** Markup only, inside `RigDriveAndPower`. Say
   what was arranged and the numbers before and after at both widths. Mark the choice as the unit's
   own.
2. **Extend `DriveAndThePowerOfferAreUnderTheRigAndTheSendAreaKeepsCqAndStop`** to realize the
   licensed fixture on **PSK31 as well as FT8**, at 1920 and 1400. On PSK31, assert:
   - the offer's border is effectively visible, with non-zero width and height;
   - it is inside the rig panel and not in the send area;
   - its top is at or below the rig display's bottom, under the S-meter, as the drive already is;
   - the rig panel is the card's height, within the tolerance the test already uses;
   - CQ and Stop are where ruling 5 leaves them.

   Keep every FT8 assertion as it is. **Restore FT8 on the model before each PSK31 window closes.**
   Press nothing.
3. **Criteria 1 and 5 while the offer shows.** Extend
   `AtNineteenTwentyTheTopRowIsAbout190AndTheWorkingCardTakesTheRest` and
   `AtFourteenHundredTheLicensedTopRowIsTheMockupsShare` to hold on PSK31 as well, **with the
   tolerances and thresholds they already assert**. If a threshold would have to move to pass, it
   does not move: that is a miss, reported with its number (§3, `PHASE_PLAN.md` §6).
4. **Remove the skip below 1900 in `TheThreePanelsShareOneTopAndOneBottom`**, so one top, one bottom
   and the floor are asserted at 1400 as well as 1920.
5. **Watched failing first, where possible.** An extension that passes on its first run says *not
   watched red*, and says why that is honest. **Do not break the view to watch a test fail.** If a
   fit was made in step 1, the PSK31 assertions should be watched red against the tree before the
   fit.
6. Then run `TheTopRowTests`, `TheWorkingPanelsTests`, `BindingHealthTests` and `VoiceTests` in one
   filter, and give each class as *n of n*. Run the carry-forward list again, status first.
7. **The trace method from task 0 stays**, asserting nothing, as unit 338's did. Say so.

**Drop candidate:** none. This is the evidence the step's state is waiting for.

### Task 2 - step 1's own tests, run and not built

**Drop candidate: this whole task.** Step 1's entry is *step 0 done*, and that verdict is not this
unit's to give. This task **changes no file under `src\` or `tests\`**. Unit 339 found most of step 1
already built by units 335 and 336, but did not run their tests. This task runs them, so step 1's
first unit is authored from results.

1. Run `TheCategoryPagesAreTradingCardsTests` in its own filter. Give **every method**:
   - its name and its result;
   - the step 1 exit criterion it proves;
   - the widths it realizes;
   - the numbers it prints;
   - for a red, the failure's first line.
2. **Say whether any step 1 must-pass has no test in that class**, beyond the nice-to-pass unit 339
   already named (the map opening in a popup on click).
3. Do not re-measure the Countries page against its mockup. Unit 339's section 3 stands. Do not
   propose an approach.

---

## 6. Parked - do not touch, do not raise

- **Every layout mechanism units 337 to 339 chose:** the `*,383,*` split, the facts-under rule at 1400,
  the send area on the tab row, the filter in the mode strip, the sparkline width rule. Rulings 1 to 5
  hold them. Change one only if task 1 finds a criterion red because of it, and say so.
- **The offer's behavior.** When it shows, what it says, what accepting writes, and whether FT8 should
  offer too (ruling 6).
- **The live license lookup and the live heard count** (unit 338 items 1 and 4), and **the best bet
  reading the real clock** (unit 339 item 2). If a task 1 assertion turns flaky on one of them, raise
  it once, and say it was parked.
- **The three joint-run stop reds** (unit 339 item 1). Task 0 runs the class alone and reports. It
  chases nothing, because it is the transmit side.
- **Steps 1, 2 and 3 as builds.** The States wording, the Modes test, the undeletable files, the points
  file and the small reds are step 2's.
- **The license line's wording.** It is the regulation's sentence.
- **The two id schemes, including the reload's `CPS-DEC-0163` misreading.** Also real flags on country
  cards, PSK31 step 6, real PSK31 audio, the ALC margin, the map bitmap's license and the status
  script's stale `RULES_AT`. All are Tim's or the harness's, carried in `PHASE_PLAN.md` §7.
- **Anything touching the radio, a decoder, a parser or the transmit chain. Any package.**

## 7. What not to do

- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.** (HM-DEC-155.)
  **Never join `TheOperatorCanStopItTests` with the layout classes.**
- **Never execute the accept or decline command, CQ or Stop in a test.** Read positions only (§0.2,
  HM-DEC-084).
- **Do not change the offer's words, the ALC line's words, a binding or a command to make it fit**
  (ruling 7). Arrangement only.
- **Do not loosen a threshold or tolerance to make a test pass, and do not break the view to watch one
  fail.** A small miss ships as `partial` with its number.
- **Do not build step 1 in task 2.** `CLAUDE.md` §12.6 covers the rest.
- **No image assets. No package. Report mismatches; repair nothing. Write American.**
- **The tool facts, from units 332 to 339:**
  - apostrophes inside quoted heredocs break;
  - doubled backslashes collapse;
  - `;`, `rm`, `git stash`, `sort` in a pipe, `git check-ignore` and `sed -E` are refused;
  - Python cannot run;
  - a multi-line commit message needs more than one `-m`;
  - use exact-text edits for markup;
  - the validator runs as `dotnet build tools/arbiter/validate-output.proj -p:Report=output.md`,
    because the `.bat` spelling is mangled by Git Bash.

## 8. Committing and pushing

Commit and push each task on its own, on `main`. Task 2 commits only if it leaves a file, such as the
report. The report names the commits and says whether each push succeeded. **A refused push is
reported as refused, with the reason.**

---

## 9. Reporting

Write `output.md` at the root, then stop. Do not start the next unit. **Every exit writes it**:
finished, blocked, failed or stopped early. Canonical headings: `## 1. What Claude did`,
`## 2. What the owner should expect`, `## 3. What you should see`, `## 4. What's blocking us`. Validate
it with `dotnet build tools/arbiter/validate-output.proj -p:Report=output.md`.

**The ordering block comes first.** `validate-output` refuses a report without it.

```
READ IN THIS ORDER.

A. The phase goal - the screen, done right. Step 0 <state as the evidence
   stands>: <how many of its six must-pass carry a green named test at
   both widths WITH THE POWER OFFER DRAWN, and how many sub-clauses are
   still held by containment or a print>. Step 1 not started; its tests
   <run in task 2, n of 6 | not run, task 2 dropped>. Steps 2 and 3 not
   started.
B. Step 0 and its exit criteria, each with the test that proves it, its
   result and the widths and modes it realized:
   1. top row about 190 px at 1920, panels at least half below the pills -
      <FT8 px / share; PSK31 px / share; at 1920 AND 1400>
   2. the card's three things - <test, result; unchanged by this unit or not>
   3. rig panel the card's height, drive AND THE DRAWN POWER OFFER under
      the S-meter - <test, result; offer box and rig display bottom, rig
      panel against card, PSK31 at 1920 AND 1400>
   4. three panels equal height to the status bar, facts beside the map
      at 1920 - <test, result>
   5. at 1400 the same shape - panels full to the status bar NOW ASSERTED
      <result, y against floor>; licensed top row <FT8 and PSK31 px /
      share against 0.262>; no callsign clipped <result>
   6. BindingHealthTests <n of n>, VoiceTests <n of n>, carry-forward app
      <n of n> and engine <n of n>
   nice-to-pass: best bet joined to the green block - <test, result>
   Not on criterion 6's list: TheOperatorCanStopItTests alone <n of 9>.
C. The report last. Section 4 raises N items on top of the carried queue;
   say which, if any, stands in the way of a criterion in B - in
   particular whether ruling 7's fit reached the criteria or left a miss.
```

**Every line specific to this unit.** If a criterion was not measured, say *not measured*. Do not
fill the shape.

```
UNIT:       340 - <complete|stopped> at task N of 3 - <date time, read from the clock>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   <yes|no> - <whether the drawn offer holds criterion 3 at both
            widths, whether a fit was needed, and whether 1400's floor is
            now asserted>
NUMBER:     step 0 sub-clauses held only by containment or a print:
            <before> -> <after>; top row with the offer drawn at 1920:
            <px>
DRIFT:      0
```

**Section 3 leads with the answer:** with the power offer drawn, does every step 0 exit criterion
hold with a named green test at the widths it names? If not, which one, at which width, in which
mode, and by how many px? Then give what was fitted, if anything, with before and after. Then the
step 0 table as it stands after task 1: test, criterion, widths, modes, result, numbers. Then, if task
2 ran, step 1's tests with their results. **Every appearance claim is computed, not seen. Say so
once.**

**Section 4:** unit 339's section 4 verbatim, per HM-DEC-139, including the queue it carries. Mark its
item 4 `TAKEN UP by work instruction 340 task 1`, with one line on what became of each sub-clause.
Mark its item 1 with task 0's alone-run result. Then anything this unit raises. **A ruling is wanted
only where Tim must decide; everything else is a finding and says so.** If ruling 7 left a miss, that
is a ruling request naming the options, each measured.

---

```
ARBITER-DECISION
STEP: 0
APPROACH: draw the PSK31 power offer in the rig panel on the licensed fixture and assert it under the S-meter at 1920 and 1400 with the top row and rig height holding while it shows, fitting it by arrangement in RigDriveAndPower if it does not; assert the three panels' floor at 1400; then run step 1's TheCategoryPagesAreTradingCardsTests without building
MOVE: work around
WHY: The state reader left step 0 partial on one clause: the power offer was only ever checked by containment on an FT8 fixture where it is not drawn, so its place under the S-meter - and whether the 190 px top row survives it - was never measured. This is a different approach from unit 339's (draw the thing on the mode that offers it, and fit it if it breaks the row, rather than extend existing assertions to a second width); the loop test found nothing like it.
STATE: partial
DECIDED: the power offer is measured on PSK31, the mode that offers it, and is not added to FT8 because the mockup draws it there (ruling 6); if it breaks the top row it is fitted by arrangement only, with its words, the ALC line, bindings and commands unchanged, and a miss beyond arrangement is reported rather than shortened (ruling 7); TheOperatorCanStopItTests runs alone, never joined; step 1's tests may run before step 0's verdict but nothing is built - all the author's, marked for Tim, overrulable
LICENCE: PHASE_PLAN.md R26, sections 4 (step 0 exit, step 1 entry) and 6 (the arbiter decides layout and continues; a miss by a little ships partial; never loosen a test); PHASE_OUTCOME.md unit 339 STATE_WHY; unit 339 output.md section 4 items 1 and 4; MainWindowViewModel.HasPsk31PowerOffer and OnChosenDigitalModeChanged (section R11 on PSK31); HM-DEC-084; R15; CLAUDE.md 0.2, 0.5; psk31 R12, R14
ACCOMPLISHED: the rig display is shown to hold the transmit drive and the power offer under the S-meter, with the offer actually on the screen, at both of Tim's widths, and the top row stays the mockup's height when PSK31 is chosen - so step 0 can close on evidence and step 1 starts from its own test results
ADVANCES: step 0 - must-pass 3 (the power offer drawn and under the S-meter at 1920 and 1400), must-pass 1 and 5 as they hold with the offer shown, and must-pass 5's full-to-the-status-bar at 1400, all in tasks 0 and 1; task 2 advances no step 0 criterion and is the drop candidate
END-ARBITER-DECISION
```
