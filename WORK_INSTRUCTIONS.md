# Work instruction 341 - step 0, the power offer as the mockup's one line

Step 0 of `PHASE_PLAN.md`, **fifth unit on it**. Units 337 and 338 built the layout. Unit 339 put a
named test behind every criterion. Unit 340 drew the PSK31 power offer and measured it: the top row
goes to 305 px while the offer shows. No arrangement inside the rig column brings that back, and the
unit asked for a ruling. **The ruling that held it was the arbiter's own ruling 7, not Tim's. This
instruction withdraws it and rules the shape instead.** Following Tim's mockup and §R11, the offer
under the S-meter is one line. The full offer opens from that line, with its words unchanged.
**Three tasks, 0 to 2.**

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

**The number: with the PSK31 power offer drawn, the top row is 305 px, against 209 at 1920 (96 px
over) and 238 at 1400 (67 px over). The panels are 388 px against 455.** So criteria 1 and 5 are red on
PSK31, and the state reader answered `blocked`:

> With the PSK31 power offer drawn the top row measures 305 px against 209 at 1920 and 0.335 against
> 0.262 at 1400, so criteria 1 and 5 are red, and the unit shows that no layout change inside the rig
> column fixes this without changing the offer's words, which ruling 7 forbids, so a ruling on
> section 4 item 1 is needed first.

**What the tree and the plan say, read by the arbiter on 2026-09-12:**

- **The height budget does not hold a 112 px offer anywhere on the main window at 1040 px tall.**
  Below the pills there are 910 px. The top row gets at most 209 and the panels at least 455. On FT8
  the panels have 48 px to spare (503). A block 112 px tall costs the panels that height wherever it
  is drawn: in the rig column (option d), beside CQ (option c), or as a strip above the panels. The
  one arrangement that avoids the cost is one that is not in the layout, which is a popup.
- **`assets/main-screen-mockup.png`, Tim's approved picture, draws the offer as one line** under the
  S-meter: *Transmit drive 30 % · RF power 50 % offered*.
- **§R11 (Tim, 2026-09-11)** says RF power for PSK31 *defaults to half and is offered as a percentage
  beside the drive, never written silently*.
- **HM-DEC-084's tier two**: power *is offered rather than simply done*. The code's own comment on
  `Psk31PowerOffer` says the offer *says what would change and what would not*, and that it says so
  before the press that writes.
- **The offer today is one `Border` in `RigDriveAndPower`** (`MainWindow.axaml` near line 3079). It
  holds `DigitalPsk31PowerOffer` (195 characters, 5 lines on the host), `DigitalPsk31PowerAccept`,
  `DigitalPsk31PowerDecline` and `DigitalPsk31AlcReference` (4 lines). The border is 520 x 112 after
  unit 340's fit.
- **A second, smaller miss that is not the offer:** on PSK31 at 1400 the neighborhood card alone
  makes the row 240 px (0.264), 2 px over 0.262. The green block is 103 px on PSK31 against 91 on
  FT8. Which line adds the 12 px was not read (unit 340 item 2).
- **Criteria 2 and 4 were never realized on PSK31** (unit 340 section 3's table). If the evidence
  is to close step 0 in both modes the offer draws in, they need to be.

**Every figure above comes from unit 340's report, from the source and from the picture. The arbiter
ran none of it.**

```
PHASE GOAL: The screen, done right - the main window as the approved mockup,
            one short top row and the working panels given the height; then
            the achievements category pages as trading cards; then what the
            last phase left; then Tim at his window says it passed.
UNIT GOAL:  The PSK31 power offer drawn as the mockup's one line under the
            S-meter, opening the full offer with its words, buttons and ALC
            line unchanged in a popup, so the top row holds 190 px at 1920
            and 0.262 at 1400 while it shows; the PSK31 green block's 2 px at
            1400 found and fitted; criteria 2 and 4 realized on PSK31.
ADVANCES:   step 0, must-pass 1 and 5 on PSK31 (red today at 305 px) and
            must-pass 3 with the offer in its new shape, in task 1;
            must-pass 2 and 4 realized on PSK31, in task 2. Task 2's
            criterion 4 half is the drop candidate.
DRIFT:      0
```

---

## 2. Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim against the files and report any
mismatch. **Report the mismatch; do not repair the instruction.** Mismatches go in the report even
when the work succeeds anyway.

Check:

- `PHASE_STATUS.md` names *The screen, done right*, with step 0 `blocked`.
- The offer's `Border` in `RigDriveAndPower` reads as §1 says, with unit 340's fit comment above it
  (near line 3073 of `MainWindow.axaml`).
- `Psk31PowerOffer`, `Psk31PowerAccept`, `Psk31AlcReferenceLine`, `HasPsk31PowerOffer`,
  `AcceptPsk31PowerAsync` and `DeclinePsk31Power` are near lines 14866 to 14931 of
  `MainWindowViewModel.cs`. Accepting sets `_psk31PowerSettled`, writes `psk31_power_accepted` and
  writes `CivWrites.RfPower` once. Declining writes `psk31_power_declined` and nothing to the radio.
- `MainWindow.axaml` already uses `<Popup IsOpen="{Binding ...}">` (the nudge near 4795, the map
  near 5651), so a popup needs no package.
- `TheTopRowTests` holds `Unit340TraceThePowerOfferOnPsk31`, and
  `DriveAndThePowerOfferAreUnderTheRigAndTheSendAreaKeepsCqAndStop` realizes FT8 and PSK31.
  `AtNineteenTwentyTheTopRowIsAbout190AndTheWorkingCardTakesTheRest` and
  `AtFourteenHundredTheLicensedTopRowIsTheMockupsShare` are red on PSK31 only.
- **The reload's disagreements:**
  - **The reload says `CLAUDE.md` §1 holds `CPS-DEC-0163`.** Units 339 and 340 found this is the
    reload misreading the file. The id schemes are parked: report it in one line and do not
    resolve it.
  - `PROJECT_STATUS.md` `RULES_AT: HM-DEC-163` agrees with `DECISIONS.md`. Keep it unless this unit
    records a decision.
  - `PHASE_OUTCOME.md`, `PHASE_STATUS.md` and `RUN_LEDGER.md` are uncommitted. They are the
    launcher's writes. Commit them **unchanged** in task 0's commit with `WORK_INSTRUCTIONS.md`, as
    units 336 to 340 did. Do not commit `.run-unit\`.

**Reds expected, older than this unit. Name them and do not chase them:**

- **`AtNineteenTwentyTheTopRowIsAbout190AndTheWorkingCardTakesTheRest` and
  `AtFourteenHundredTheLicensedTopRowIsTheMockupsShare`, on PSK31.** These are the reds this unit
  exists to turn green. They are already red against the tree, so they count as watched red.
- `TheOperatorCanStopItTests.TheStopAddedNoNewRouteToATransmission`: two `_armedSend.Arm(` lines.
- `TheOperatorCanStopItTests.AClickWhileTheToneIsPlayingStopsTheSoundAndNotJustTheCarrier`: red alone
  in unit 340, and a timing matter. Parked. **This unit does not run that class.**
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
`VoiceTests`, in one filter.

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

**§R11, Tim, 2026-09-11 (`docs/phase-psk31-run/PHASE_PLAN.md`), the sentence that bites:** *RF power
for PSK31 defaults to **half** and is **offered** as a percentage beside the drive, never written
silently (HM-DEC-084, HM-DEC-074).*

**HM-DEC-084, tier two, transcribed:** *Tier two changes what the operator sounds like and is offered
rather than simply done: power, keyer speed, break-in and its delay.*

**§R15, Tim, 2026-09-11, the sentence that bites:** *Until an FT8 send has been observed, there is no
reference and Hamlet reports the reading and judges nothing - it never invents one.*

**`PHASE_PLAN.md` §2 and §6, the lines that bite here:**

- *The arbiter stops for three things only: keying, transmit or the radio's safety; money past the
  budget; a decision that changes what the product promises the operator. On everything else it
  takes its own recommendation, marks it author's and overrulable, applies it, and continues.*
- *A must-pass is missed by a little: ship, report, `partial`, move on. Never loosen a test.*
- *A string will not fit: shorten and say which, or widen; never clip.*
- *Anything touches the radio, a decoder, a parser or the transmit chain: `MOVE: stop`.*
- *A package is needed: `MOVE: stop`.*
- *A file must be deleted: empty it, comment it, list it.*

**The arbiter's rulings from work instructions 338 to 340, still in force.** They are the author's,
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
6. **Criterion 3's power offer is measured on PSK31, the mode that offers it.** It is not added to
   FT8.

**Ruling 7 of work instruction 340 is withdrawn.** It said the offer was fitted by arrangement only,
with nothing behind a hover. Unit 340 measured that arrangement cannot hold R26, and the ruling was
the arbiter's, not Tim's.

**The arbiter's rulings for this unit.** They are the author's, marked for Tim and overrulable at step
3.

8. **Under the S-meter, the offer is one line. The full offer opens from it in a popup.**
   - **The line** shows while `HasPsk31PowerOffer` is true. It uses the mockup's words: *RF power
     {Psk31PowerPercent} % offered*. The number is bound to `Psk31PowerPercent`, never typed. It
     sits under the rig display with the drive: on the drive's row, on the drive note's row, or on
     its own line, whichever the unit measures to hold the row. It looks pressable, and pressing it
     opens the popup and **writes nothing**.
   - **The popup** holds the existing offer **unchanged**: `DigitalPsk31PowerOffer` with its words,
     `DigitalPsk31PowerAccept` and `DigitalPsk31PowerDecline` with their content, commands and
     bindings, and `DigitalPsk31AlcReference` with its words. **Accept and decline exist only inside
     the popup**, so the full sentence is on screen at every press that writes. Closing the popup
     without an answer leaves `HasPsk31PowerOffer` true and writes nothing. The popup opens on a
     click, never on a hover.
   - *Why:* this is the only shape measured or reasoned that holds R26's height and still shows
     every word before the press. §R11 and the mockup both draw the offer as a percentage beside the
     drive. HM-DEC-084 tier two asks that power be *offered*, and the popup keeps it offered, with
     what changes and what does not written beside the button. §0.5 holds, because the line carries
     the information (power, percentage, offered) and the popup carries the detail.
   - *Not a stop:* nothing in the transmit chain, the write, its tier, `HasPsk31PowerOffer` or the
     offer's words changes. What the product promises the operator is the same promise: offered,
     never silent, explained before the press. **What changes is one click more to reach the
     accept button.** That is marked for Tim at step 3.
   - *Rejected:*
     - (a) and (b) from unit 340: they take the sentence or the ALC line off the screen at the
       press.
     - (c), the offer beside CQ: it breaks R26's *under the S-meter*, and 112 px beside CQ grows
       the tab row and costs the panels the same height.
     - (d), a taller row while unanswered: it misses by 96 px, and the offer returns on every
       launch.
     - A hover: HM-DEC-084's words must be on screen at the press, not under a pointer.
9. **The PSK31 green block's extra 12 px at 1400 is fitted under the rulings already in force.**
   Task 0 finds the line. If it is the rule of thumb, the heard line or the sparkline, rulings 2 and 3
   and §6 apply: shorten a string and name it, or hide the sparkline. **If it is the license line, it
   is not reworded** (it is the regulation's sentence, parked). Then the 2 px ships as a little miss,
   with its number.
10. **Telemetry: the popup is not a new stage** (R13). It is a view of an existing offer, and
    `psk31_power_accepted` and `psk31_power_declined` already record the answer. No event is added.

**Standing, transcribed:**

- **§0.0**: *Never present a guess as a decode… This binds pictures as hard as sentences.* Every
  appearance claim is computed, not seen, and says so once.
- **§0.2**, *transmit safety, ABSOLUTE*: *One operator action, one transmission.* **Tests never execute
  `AcceptPsk31PowerCommand`, `DeclinePsk31PowerCommand`, CQ or Stop.** A test may open the popup by
  its line, or by the popup's own open state, and must show that doing so wrote no event and changed
  no setting.
- **§R11 / HM-DEC-084** (as the markup's own comment states them): the power offer is *offered, never
  mirrored and never written silently*, and *pressing nothing changes nothing*.
- **§0.5**: *every panel is collapsible, and a collapsed panel still carries its summary. Collapsing
  hides detail, never information.*
- **§0.6**: every ink clears 4.5:1 against its fill, including the new line against the rig display's
  fill; color is never the only carrier.
- **R12**: *a session fixes its own tests and never asks the owner to approve it.* The PSK31 half of
  `DriveAndThePowerOfferAreUnderTheRigAndTheSendAreaKeepsCqAndStop` asserts the old border's place.
  It is this unit's to rewrite for ruling 8.
- **R13**: telemetry on every new stage. Ruling 10 says the popup is not one.
- **R14**: *a test exists to prove an exit criterion.* Extend the tests the criteria need and add no
  others.
- **R19**: American spelling.

---

## 4. Status cadence

Write status before every `dotnet` command and after every task. `tools/status.sh` has been refused
as *requires approval* in six units. Try it once. If it is refused, take a `date` reading and paste it
whole. **Never compose a time.** The script hard-codes a stale `RULES_AT`, so do not let it overwrite
`HM-DEC-163`. The watchdog kills a session only when its process tree has used no CPU for ten minutes.

---

## 5. The tasks

### Task 0 - the trace: where the line fits, and which line makes the green block taller

Measure before anything is built. **Say what you find rather than confirming §1.**

1. **Commit the launcher's root files unchanged** (§2) with `WORK_INSTRUCTIONS.md` and a patch bump,
   as `chore(unit341): step 0, the power offer as one line - the trace before a line is built`. Set
   `PHASE_STATUS.md`'s `WORK_INSTRUCTION:` line to `341 - step 0, the power offer as one line`. Run the
   carry-forward list, status first.
2. **Add `Unit341TraceTheOneLineOfferAndThePsk31GreenBlock` to `TheTopRowTests`.** It asserts nothing
   and presses nothing. It realizes the licensed fixture on PSK31 and on FT8, at 1920 and 1400, and
   prints:
   - **the rig column's rows:** the rig display, the drive row with its three parts and their right
     edge, and the drive note row with its right edge, all against the 520 px column;
   - **the room for the line:** the px left on the drive row and on the note row, and the height the
     rig column has spare under the card at each width. Measure the width the line *RF power 50 %
     offered* needs on the host, set on the test window only, never in markup.
   - **the green block, line by line:** each line's text, width, height and line count on PSK31 against
     FT8, so the 12 px has a name;
   - the top row and the panels, readiness strip hidden and showing, as unit 340's trace does.
3. **Answer from the numbers, at each width:**
   - Where does the line fit without adding height: the drive row, the note row, or neither? If
     neither, how many px does its own line add, and does the row still hold 209 at 1920 and 238 at
     1400?
   - Which green block line makes PSK31 12 px taller, and which of ruling 9's paths applies?

Report the numbers in section 1 before task 1 starts.

**Drop candidate:** none.

### Task 1 - ruling 8 built, ruling 9 fitted, criteria 1, 3 and 5 asserted on PSK31

1. **Build ruling 8** in `MainWindow.axaml`, and in `MainWindowViewModel.cs` only if the line's words
   or the popup's open state need a property. **Mark the placement as the unit's own.** Replace unit
   340's fit comment with one that says what ruling 8 did and why. The offer's words, the ALC line's
   words, both buttons' content, commands and bindings, and `HasPsk31PowerOffer` are unchanged. Show
   that with `git diff` in the report.
2. **Fit ruling 9's line**, by the path task 0 named, or name the 2 px as the miss.
3. **Rewrite the PSK31 half of `DriveAndThePowerOfferAreUnderTheRigAndTheSendAreaKeepsCqAndStop`**
   (R12), at 1920 and 1400. On PSK31, assert:
   - the line is effectively visible, non-zero, inside the rig panel, not in the send area, with its
     top at or below the rig display's bottom, and not clipped (its desired width fits its bounds);
   - its text carries `Psk31PowerPercent`;
   - the popup is closed at start, and opening it writes no `psk31_power_*` event and leaves
     `HasPsk31PowerOffer` true;
   - with the popup open, `DigitalPsk31PowerOffer`, `DigitalPsk31PowerAccept`,
     `DigitalPsk31PowerDecline` and `DigitalPsk31AlcReference` are visible and non-zero **inside the
     popup**, and their text equals the view model's `Psk31PowerOffer`, `Psk31PowerAccept`, *I will
     set it myself* and `Psk31AlcReferenceLine`;
   - no accept or decline control is on the main window outside the popup;
   - the rig panel is the card's height, within the tolerance the test already uses, and CQ and Stop
     are where ruling 5 leaves them.

   Keep every FT8 assertion as it is. Close the popup and restore FT8 on the model before each
   PSK31 window closes. **Never execute accept or decline.**
4. **Criteria 1 and 5 on PSK31 go green with the thresholds they already assert.**
   `AtNineteenTwentyTheTopRowIsAbout190AndTheWorkingCardTakesTheRest` and
   `AtFourteenHundredTheLicensedTopRowIsTheMockupsShare` do not change a number. If the 1400 PSK31 row
   still misses by the green block's 2 px, it ships red with that number, as §6 says.
5. **Watched red.** Both top-row tests are red against the tree now. Say so with their failure lines
   before the build. Watch the rewritten drive test's popup assertions red against the tree before
   the build. The old border is not in a popup, so they should fail there. **Do not break the view
   to watch a test fail.**
6. Then run `TheTopRowTests`, `TheWorkingPanelsTests`, `BindingHealthTests` and `VoiceTests` in one
   filter, and give each class as *n of n*. Run the carry-forward list again, status first.
7. **Both trace methods stay**, asserting nothing. Say so.

**Drop candidate:** none. This is the evidence the step's state is waiting for.

### Task 2 - criteria 2 and 4 realized on PSK31

The offer only draws on PSK31, and criteria 2 and 4 have only been realized on FT8 and on the plain
fixture. This task closes that.

1. **Extend `TheGreenBlockIsInsideTheCardUnderTheStripWithTheBandLargest` and
   `TheWorldClockIsAtTheCardsRightEndWithOneMarker`** to realize the licensed fixture on PSK31 as well
   as FT8, at 1920 and 1400, with the assertions they already make. Restore FT8 before each window
   closes.
2. **Extend `TheThreePanelsShareOneTopAndOneBottom` and `AtNineteenTwentyTheCardsFactsSitBesideTheMap`**
   to hold with PSK31 chosen, at the widths they already realize. **This step is the drop candidate.**
3. Run the one filter again, then the carry-forward list, status first. Give each class as *n of n*.
   An extension that passes on its first run says *not watched red*, and why that is honest.

**Drop candidate: step 2 of this task** (criterion 4 on PSK31). The offer is inside the top row and
the panels are measured by the top-row tests. Criterion 2's half is not droppable, because the
green block is the part that changes on PSK31.

---

## 6. Parked - do not touch, do not raise

- **Every layout mechanism units 337 to 340 chose:** the `*,383,*` split, the facts-under rule at
  1400, the send area on the tab row, the filter in the mode strip, the sparkline width rule. Rulings 1
  to 5 hold them. Unit 340's padding fit may be undone if ruling 8 makes it moot; say so.
- **The offer's behavior beyond ruling 8:** when it shows, what it says, what accepting writes,
  that `_psk31PowerSettled` is not saved (so the line returns on every launch), and whether FT8
  should offer too.
- **Step 1.** Unit 340 ran its six tests, 6 of 6, and named what is held weakly (item 5). Do not run
  them again, and do not build.
- **`TheOperatorCanStopItTests`, all of it.** Do not run it. Unit 340 items 3 and 339 item 1 are
  transmit-side and parked.
- **The live license lookup, the live heard count and the best bet reading the real clock** (unit
  338 items 1 and 4, unit 339 item 2). If an assertion turns flaky on one of them, raise it once and
  say it was parked.
- **The license line's wording.** It is the regulation's sentence.
- **Steps 2 and 3.** The States wording, the Modes test, the undeletable files, the points file, the
  small reds.
- **The two id schemes, including the reload's `CPS-DEC-0163` misreading.** Also real flags on country
  cards, PSK31 step 6, real PSK31 audio, the ALC margin, the map bitmap's license and the status
  script's stale `RULES_AT`. All are Tim's or the harness's, carried in `PHASE_PLAN.md` §7.
- **Anything touching the radio, a decoder, a parser or the transmit chain. Any package.**

## 7. What not to do

- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.** (HM-DEC-155.)
- **Never execute the accept or decline command, CQ or Stop in a test.** Opening the popup is the
  only interaction allowed, and the test shows it wrote nothing (§0.2, HM-DEC-084).
- **Do not change the offer's sentence, the ALC line's words, either button's content, a command, a
  binding, `HasPsk31PowerOffer` or the write.** Ruling 8 moves them and adds one line. It rewrites
  nothing.
- **Do not put the offer, or any part of it, behind a hover.** Do not shorten the sentence to make
  the popup smaller.
- **Do not loosen a threshold or tolerance to make a test pass, and do not break the view to watch one
  fail.** A small miss ships as `partial` with its number.
- `CLAUDE.md` §12.6 covers the rest. **No image assets. No package. Report mismatches; repair
  nothing. Write American.**
- **The tool facts, from units 332 to 340:**
  - apostrophes inside quoted heredocs break, and an apostrophe in an argument breaks the `.bat`
    tools;
  - doubled backslashes collapse;
  - `;`, `rm`, `git stash`, `sort` in a pipe, `git check-ignore`, `sed -E`, a shell loop variable,
    `pwd -W`, `grep -v` in a pipe and a redirect into `testresults\` are refused;
  - Python cannot run;
  - a multi-line commit message needs more than one `-m`;
  - use exact-text edits for markup;
  - the validator runs as `dotnet build tools/arbiter/validate-output.proj -p:Report=output.md`,
    because the `.bat` spelling is mangled by Git Bash.

## 8. Committing and pushing

Commit and push each task on its own, on `main`. The report and status file follow in their own
commit. The report names the commits and says whether each push succeeded. **A refused push is
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
   both widths ON FT8 AND ON PSK31 with the offer showing as ruling 8
   draws it, and which do not>. Step 1 not started; its tests 6 of 6 in
   unit 340, not run here. Steps 2 and 3 not started.
B. Step 0 and its exit criteria, each with the test that proves it, its
   result and the widths and modes it realized:
   1. top row about 190 px at 1920, panels at least half below the pills -
      <FT8 and PSK31 px / share at 1920; was 305 on PSK31>
   2. the card's three things - <test, result; PSK31 and FT8 at 1920 AND
      1400; the green block's PSK31 height before and after ruling 9>
   3. rig panel the card's height, drive AND THE ONE-LINE OFFER under the
      S-meter, the full offer unchanged in its popup - <test, result;
      line box against rig display bottom; popup contents equal to the
      view model; nothing written on opening; PSK31 at 1920 AND 1400>
   4. three panels equal height to the status bar, facts beside the map
      at 1920 - <test, result; plain and PSK31, or PSK31 dropped>
   5. at 1400 the same shape - panels full to the status bar <result>;
      licensed top row <FT8 and PSK31 px / share against 0.262; was 305
      on PSK31>; no callsign clipped <result>
   6. BindingHealthTests <n of n>, VoiceTests <n of n>, carry-forward app
      <n of n> and engine <n of n>
   nice-to-pass: best bet joined to the green block - <test, result>
C. The report last. Section 4 raises N items on top of the carried queue;
   say which, if any, stands in the way of a criterion in B - in
   particular whether ruling 8 held the row at both widths and whether
   ruling 9 closed the 2 px or left it.
```

**Every line specific to this unit.** If a criterion was not measured, say *not measured*. Do not
fill the shape.

```
UNIT:       341 - <complete|stopped> at task N of 3 - <date time, read from the clock>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   <yes|no> - <whether the one-line offer holds criteria 1, 3 and 5
            on PSK31 at both widths, whether the 2 px closed, and whether
            criteria 2 and 4 now hold on PSK31>
NUMBER:     PSK31 top row with the offer showing: 1920 305 -> <px>;
            1400 305 -> <px> (<share>)
DRIFT:      0
```

**Section 3 leads with the answer:** with PSK31 chosen and the offer unanswered, does every step 0 exit
criterion hold with a named green test at the widths it names? If not, which one, at which width, and
by how many px? Then say where the line went and why, with the numbers before and after at both widths.
Then describe the popup in words: what is in it, where it opens, what closes it. Then the step 0 table
after task 2: test, criterion, widths, modes, result, numbers. **Every appearance claim is computed,
not seen. Say so once.**

**Section 4:** unit 340's section 4 verbatim, per HM-DEC-139, including the queue it carries. Mark its
item 1 `ANSWERED by the arbiter's ruling 8 in work instruction 341`, with what was built and the
numbers. Mark its item 2 `TAKEN UP by work instruction 341 ruling 9`, with what became of the 2 px.
Then anything this unit raises. **A ruling is wanted only where Tim must decide; everything else is a
finding and says so.** Ruling 8's extra click is already marked for Tim at step 3. Do not raise it
again unless the build found something that changes it.

---

```
ARBITER-DECISION
STEP: 0
APPROACH: the PSK31 power offer as the mockup one line (RF power N % offered) under the S-meter, opening the unchanged full offer - sentence, accept, decline, ALC line - in a popup on click; find and fit the PSK31 green block 2 px at 1400; realize criteria 2 and 4 on PSK31
MOVE: work around
WHY: Unit 340 measured that no arrangement of the full offer holds R26's row, and the ruling it asked for was the arbiter's own ruling 7, not one of the three things the plan stops for. The mockup and section R11 both draw the offer as a percentage beside the drive, so a one-line offer with the full words in a popup is a different approach to the same step. The loop test found nothing like it.
STATE: blocked
DECIDED: ruling 7 withdrawn; ruling 8 - the offer under the S-meter is the mockup line "RF power N % offered", opening a click popup that holds the unchanged sentence, both buttons and the ALC line, with accept and decline only inside it and nothing written on opening; unit 340 options (a), (b), (c) and (d) rejected with reasons; ruling 9 - the PSK31 green block's 12 px is fitted by rulings 2 and 3 unless it is the license line, which is not reworded and ships as a 2 px miss; ruling 10 - the popup is not a new telemetry stage - all the author's, marked for Tim at step 3, overrulable
LICENCE: PHASE_PLAN.md section 2 (the arbiter stops for three things only; otherwise decides, marks, applies, continues), R26, sections 4 and 6; assets/main-screen-mockup.png (the one-line offer under the S-meter); docs/phase-psk31-run/PHASE_PLAN.md R11 (offered as a percentage beside the drive), R13, R15; HM-DEC-084 tier two; unit 340 output.md section 4 items 1 and 2 with their measured options; PHASE_OUTCOME.md unit 340 STATE_WHY; CLAUDE.md 0.0, 0.2, 0.5, 0.6; psk31 R12, R14
ACCOMPLISHED: with PSK31 chosen, the top row stays the short band in Tim's picture, with "RF power 50 % offered" under the S-meter as the mockup draws it, and the full offer one click away with every word still in front of the button that writes - so step 0 can close on evidence in both modes
ADVANCES: step 0 - must-pass 1 and 5 on PSK31 (305 px today against 209 and 238) and must-pass 3 in the offer's new shape, in task 1; must-pass 2 and 4 realized on PSK31, in task 2, whose criterion 4 half is the drop candidate
END-ARBITER-DECISION
```
