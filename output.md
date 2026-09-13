```
READ IN THIS ORDER.
```

A. The phase goal - the screen, done right. Step 0 as the evidence stands: all six must-pass now
   carry a green named test at the widths they name, up from 3 of 6 before this unit, and the
   nice-to-pass is green at 1920. Two sub-clauses are asserted more weakly than the criterion
   reads (section 4, item 4). The STEP line is the launcher's to write. Step 1 not started; its
   ground measured in task 2. Steps 2 and 3 not started.
B. Step 0 and its exit criteria, each with the test that proves it, its result and the widths it
   realized:
   1. Top row about 190 px at 1920; the three PANELS at least half below the pills.
      `AtNineteenTwentyTheTopRowIsAbout190AndTheWorkingCardTakesTheRest` pass, 190 px at 1920.
      `TheThreePanelsTakeAtLeastHalfTheHeightBelowTheBandPills` pass. With the readiness strip
      hidden: licensed 503 px (0.553) at 1920, plain 505 (0.555) at 1920 and 465 (0.511) at
      1400. With it showing: 450 (0.495), 452 (0.497) and 441 (0.485).
   2. The card's three things. `TheGreenBlockIsInsideTheCardUnderTheStripWithTheBandLargest`
      pass at 1920 AND 1400: the band is 20 px against 15 px for the next largest at both, and
      the count is in the block at both. At 1920 the sparkline is drawn and no line beside it
      wraps; at 1400 the width rule hides it and *heard just now* stands over the count.
      `TheWorldClockIsAtTheCardsRightEndWithOneMarker` pass at 1920 AND 1400: 246 x 134, one
      marker, 15 px from the card's right edge at both.
   3. Rig panel the card's height, drive and power under the S-meter.
      `DriveAndThePowerOfferAreUnderTheRigAndTheSendAreaKeepsCqAndStop` pass at 1920 AND 1400:
      190 against 190 px, and 219 against 219 px. The drive box starts at y 274, under the rig
      display's bottom at y 257, at both widths. The power offer is inside the rig panel but is
      not drawn on this FT8 fixture.
   4. Three panels equal height to the status bar, facts beside the map at 1920.
      `TheThreePanelsShareOneTopAndOneBottom` pass: one top and one bottom, ending at the working
      card's floor at y 953. `AtNineteenTwentyTheCardsFactsSitBesideTheMap` pass: 678 px inside
      against 220 + 10 + 338. `TheDecodedListIsAsWideAsItsLongestLineNeeds` pass: the column is
      383 px against a need of 383 at both widths.
   5. At 1400 the same shape, no callsign clipped, facts by the rule. The licensed top row is 219
      px (0.241) against 0.262. `AtFourteenHundredTheLicensedTopRowIsTheMockupsShare` pass;
      `AtFourteenHundredTheSameShapeHolds` pass, with no callsign clipped and the facts UNDER by
      the rule (419 < 568); `NoCallsignIsClipped` pass at 1920; and criteria 2 and 3's tests at
      1400, above. On windows where the live best bet line draws, the licensed top row is 228 px
      (0.251), still under 0.262.
   6. BindingHealthTests 1 of 1, VoiceTests 5 of 5, carry-forward app 100 of 100 and engine 85
      of 85, both before and after task 1.
   nice-to-pass: best bet joined to the green block.
   `TheBestBetPillAndTheGreenBlockNameTheSameBandOnTheWindow` pass at 1920.
   Not on criterion 6's list: TheStopAddedNoNewRouteToATransmission red, as expected. Three more
   `TheOperatorCanStopItTests` were red in the joint run and pass alone (section 4, item 1).
C. The report last. Section 4 raises 5 items on top of the carried queue. None stands in the way
   of a criterion in B. Item 4 is about how strongly criteria 3 and 5 are asserted, and is not a
   red.

```
UNIT:       339 - complete at task 2 of 3 - 2026-09-12 21:21
PHASE GOAL: The main window laid out as Tim's approved mockup - a short top row, the working
            panels given the height - then the achievements pages as trading cards, then what
            the last phase left, then Tim's own pass at his window.
UNIT GOAL:  Put a named, green test with its result behind every step 0 exit criterion at
            1920 and at 1400, so the step is read from evidence, then measure step 1's ground
            without building on it.
ADVANCED:   yes - criteria 2 and 3 gained assertions at 1400 (and with them criterion 5), criterion 6 is named with counts, and none was found red
NUMBER:     step 0 criteria with a green named test at both widths: 3 -> 6 of 6, with two
            sub-clauses asserted more weakly than the criterion reads (section 4, item 4)
DRIFT:      0
```

## 1. What Claude did

**Complete: tasks 0, 1 and 2 of 3.** Task 0 is pushed as `38b4427` and `9841934`, and task 1 as
`c8c6b97`. Task 2 changed no file. This report and the status file follow in their own commit.

Provenance: Windows 11, `C:\Source\HamLet`, branch `main`, and the prompt claimed Hamlet. The gate
passed: `SHACK_FACTS.md` and `src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs` are present,
`CoreHMI.sln` and `MURC.sln` are absent, and the root is `C:\Source\HamLet`. This is the
development computer, so nothing here is evidence about the radio. No decision was recorded in
`DECISIONS.md`.

### Task 0 - the trace

- **The launcher's files** were committed unchanged in `38b4427`, with `WORK_INSTRUCTIONS.md` and
  the patch bump from 1.13.23 to 1.13.24. `PHASE_STATUS.md`'s `WORK_INSTRUCTION:` line was then set
  to `339 - step 0, proved at both widths` in `9841934`.
- **Carry-forward before any change:** app 100 of 100, engine 85 of 85.
- **The five classes, in one filter:** 25 of 29. All 20 step 0 tests are green, and
  `TheOperatorCanStopItTests` is 5 of 9.

#### Every test method, before task 1

The licensed fixture is `TheTopRowTests.Realized`; the plain one is `TheWorkingPanelsTests.Realized`.
Every window is 1040 px tall, with 910 px below the band pills.

| Test | Result | Criterion | Widths realized | Numbers |
|---|---|---|---|---|
| TheTopRowTests.AtNineteenTwentyTheTopRowIsAbout190AndTheWorkingCardTakesTheRest | pass | 1 | 1920 asserted, 1400 printed | top row 190 px, working card 573 tall to the status bar's margin; 1400 printed 219 |
| TheTopRowTests.TheGreenBlockIsInsideTheCardUnderTheStripWithTheBandLargest | pass | 2 | 1920 only | band 20 px, next largest 15 (the count); block 1038 x 58; sparkline 110 x 26 |
| TheTopRowTests.TheWorldClockIsAtTheCardsRightEndWithOneMarker | pass | 2 | 1920 only | clock 246 x 134 at x 1083, 1 marker |
| TheTopRowTests.DriveAndThePowerOfferAreUnderTheRigAndTheSendAreaKeepsCqAndStop | pass | 3 | 1920 only | rig panel 190, card 190; drive box at y 274 under the rig display ending at y 257 |
| TheTopRowTests.AtFourteenHundredTheLicensedTopRowIsTheMockupsShare | pass | 3, 5 | 1920 and 1400 | 1920: top row 190 (0.209), panels 503 (0.553). 1400: 219 (0.241), panels 474 (0.521), rig 219 / card 219, sparkline hidden, count shown |
| TheTopRowTests.TheBestBetPillAndTheGreenBlockNameTheSameBandOnTheWindow | pass | nice-to-pass | 1920 | best bet 20 m: `20 m ✓`; best bet 40 m: `40 m`, no check |
| TheWorkingPanelsTests.TheThreePanelsShareOneTopAndOneBottom | pass | 4 | 1920 asserted, 1400 printed | plain, strip showing: 452 px each at 1920, 441 at 1400, both ending at the floor, y 953 |
| TheWorkingPanelsTests.TheDecodedListIsAsWideAsItsLongestLineNeeds | pass | 4 | 1920 and 1400 | column 383 against need 383 at both; 735 / 378 / 734 at 1920, 474 / 378 / 475 at 1400 |
| TheWorkingPanelsTests.AtNineteenTwentyTheCardsFactsSitBesideTheMap | pass | 4 | 1920 | card 678 px inside, map 220, facts 338: beside |
| TheWorkingPanelsTests.NoCallsignIsClipped | pass | 5 | 1920 | two cells of 200 px needing 200 and 150 |
| TheWorkingPanelsTests.AtFourteenHundredTheSameShapeHolds | pass | 5 | 1400 | plain: top row 220 (0.242), working card 541 (0.595), panels 420 (0.462) strip showing; nothing clipped; 419 < 568, so under |
| TheWorkingPanelsTests.TheThreePanelsTakeAtLeastHalfTheHeightBelowTheBandPills | pass | 1, 5 | plain 1920 and 1400, licensed 1920 | hidden / showing: plain 1920 505 (0.555) / 452 (0.497); plain 1400 465 (0.511) / 441 (0.485); licensed 1920 503 (0.553) / 450 (0.495) |
| TheWorkingPanelsTests.StopNeverCollapsesAndTheFilterStaysOnAnEmptyOrCollapsedList | pass | none by number; guards §0.2 and R17 for rulings 4 and 5 | 1920 and 1400 | Stop 74 x 22 with no collapsible ancestor, and both filter chips visible, in all four states |
| TheWorkingPanelsTests.Unit338TraceTheRowsAboveThePanels | pass (asserts nothing) | none | 1920 and 1400 | licensed 1400: 412 px (0.453) with the strip showing, 465 (0.511) hidden, on a window with the live best bet line and a 228 px top row |
| BindingHealthTests.TheMainWindowBindsWithoutOneComplaint | pass | 6 | the main window | - |
| VoiceTests, five methods | 5 of 5 | 6 | none | - |
| TheOperatorCanStopItTests, nine methods | 5 of 9 | not on criterion 6's list | 1400 x 1400 | see Reds below |

#### Which criterion, at which width, no test asserted

- **Criterion 2 at 1400: true, as §4 expected.** Nothing asserted the band as the largest text,
  the clock's one marker or the sparkline at 1400. Only the count was asserted there, in
  `AtFourteenHundredTheLicensedTopRowIsTheMockupsShare`.
- **Criterion 3 at 1400: half true.** The rig panel at the card's height was already asserted at
  1400, within 2 px, by `AtFourteenHundredTheLicensedTopRowIsTheMockupsShare`. Drive and power
  under the S-meter were not.
- **Also missing, and not in §4:**
  - *Full to the status bar* at 1400 is printed and not asserted.
    `TheThreePanelsShareOneTopAndOneBottom` asserts the floor at 1920 only. At 1400 it prints
    panels ending at y 953, which is the floor. One top and one bottom at 1400 is asserted, on the
    plain fixture, by `TheThreePanelsTakeAtLeastHalfTheHeightBelowTheBandPills`.
  - The power offer is asserted by containment only. On the FT8 fixture it is not drawn (0 x 0),
    so no rectangle puts it under the S-meter.
  - Neither was extended: task 1 names three tests, and R14 adds no others. See section 4, item 4.

#### The readiness strip showing, at 1400, licensed

- **412 px of 910 (0.453)**, against 465 (0.511) with the strip hidden. Both are printed by
  `AtFourteenHundredTheLicensedTopRowIsTheMockupsShare` in the task 1 run.
- **That window drew the live best bet line**, so its top row was 228 px, not 219. See section 4,
  item 2.
- On a window without that line, the hidden figure is 474 px (0.521). The showing figure on such a
  window was not measured: 421 px is arithmetic (474 less the strip's 43 px and its 10 px margin).
- At 1920 it is 450 px (0.495), as unit 338 found.

### Task 1 - criteria 2 and 3 at 1400, asserted

All the changes are in `tests/Hamlet.App.Tests/Views/TheTopRowTests.cs`. No markup or source
changed.

- **`TheGreenBlockIsInsideTheCardUnderTheStripWithTheBandLargest`** loops over 1920 and 1400 on the
  licensed fixture.
  - At both widths: the block is inside the card and under the strip and legend; the band is the
    largest text in the card; the count is in the block, right of the band's column.
  - **What ruling 3's rule drew, not a width.** If the sparkline is drawn, it is in the block right
    of the band's column, and the license line and the rule of thumb are each on one line
    (`TextLayout.TextLines`). If it is hidden, *heard just now* (`GreenZoneHeardLabel`) is drawn in
    the block, ends at or above the count's top, and overlaps it across.
  - On the host: at 1920 the sparkline is drawn and nothing wraps. At 1400 it is hidden, the label
    is at y 268 over the count at y 277, and the license line takes 3 lines and the rule of thumb 2.
- **`TheWorldClockIsAtTheCardsRightEndWithOneMarker`** loops over both widths with its assertions
  unchanged, and prints the distance to the card's right edge: 15 px at both.
- **`DriveAndThePowerOfferAreUnderTheRigAndTheSendAreaKeepsCqAndStop`** loops over both widths and
  adds ruling 5's place.
  - CQ and Stop are visible, in `DigitalSendReserved`, and inside no collapsible panel.
  - The send area has the same parent as `ModeTabs`, starts right of the tabs, and ends above the
    working card.
  - It reads positions and presses nothing.
- **`AtFourteenHundredTheLicensedTopRowIsTheMockupsShare`** now also prints the panels with the
  readiness strip showing, before hiding it. No assertion was added.

**Not watched red.** All three passed on their first run at both widths.
- That is honest because task 0's printed 1400 numbers already satisfied what the tests now assert:
  the layout holds, and the test is the task.
- The view was not broken to watch one fail.
- Nothing was red at 1920, so nothing was rewritten under R12.

**After task 1:**
- TheTopRowTests 6 of 6.
- BindingHealthTests 1 of 1 and VoiceTests 5 of 5, run in one filter.
- Carry-forward app 100 of 100 and engine 85 of 85.

### Task 2 - step 1's ground, measured and not built

The drop candidate was not dropped. No file under `src\` changed, and no test was added.

- **Step 1's entry check: `TheAchievementsPageClicksInTests` 8 of 8.**
  - `ThePageHasNoScrollerAndNothingBelowTheLegend` pass
  - `ClickingABadgeReplacesThePageAndTheBackControlReturns` pass
  - `InsideACategoryTheEarnedCardsComeFirstThenOneUnearned` pass
  - `ContinentsOpensToSevenAndEachToItsCountries` pass
  - `NoStringInAnySlotIsClippedAtTheWindowsSize` pass
  - `OpeningACategoryWritesTheKindAndTheCardCountAndNothingElse` pass
  - `OpeningTheWindowWritesHowManyStatesScoredAndNoCode` pass
  - `ReadingThePointsFileWritesHowManyRankNamesItReadAndNoName` pass
- **The exit criteria against the tree, the Countries page against its mockup, and the three largest
  gaps** are in section 3.
- **What it found in one line:** unit 335 built most of step 1 already. The criteria each have
  code and a named test in `TheCategoryPagesAreTradingCardsTests`, which this unit did not run
  (HM-DEC-155). The nice-to-pass has nothing behind it.

### Reds

- **`TheOperatorCanStopItTests.TheStopAddedNoNewRouteToATransmission`** is red, as §5 says: two
  `_armedSend.Arm(` lines, at `MainWindowViewModel.cs` 14202 and 14381.
- **New reds in what ran:** three more `TheOperatorCanStopItTests`, in the joint run of the five
  classes. All three pass when run alone, 3 of 3. See section 4, item 1.
  - `AClickBeforeTheBoundaryUnarmsItAndNothingGoesOut`
  - `TheLineSaysWhatHappenedToTheCarrierAndToTheSound`
  - `AClickWhileTheToneIsPlayingStopsTheSoundAndNotJustTheCarrier`
- **Not run:** `TheWholeChainRunsFromOneRightClickTests`, `TheMenuIsUnderTheMouseTests`,
  `ThePsk31RecordsAppearTests` and `TheTotalMilesTests` (HM-DEC-155).
- No red turned green.

### Where the instruction and the tree disagreed

- **§4's claim about criterion 3 at 1400 is half right.** The rig panel's height was already
  asserted at 1400; drive and power were not.
- **§5's `CLAUDE.md` check.** `CPS-DEC` occurs 0 times in `CLAUDE.md`. The highest `HM-DEC` in
  `DECISIONS.md` is 163, and no 164 or later exists, so `RULES_AT: HM-DEC-163` stands. Reported,
  not resolved: the id schemes are parked.
- **The prompt says to run `tools\arbiter\validate-output.bat`.** §2 says that spelling is mangled
  by Git Bash, so the `.proj` route was used.
- **The prompt says to set `PHASE_STATUS.md`'s `WORK_INSTRUCTION:` line, and §5 says to commit that
  file unchanged.** Both were done, in two commits.
- **`tools/status.sh` came back *requires approval*, tried once.** Every `UPDATED` is a `date`
  reading pasted whole. The script also writes `RULES_AT: HM-DEC-161`. See section 4, item 3.
- **Also refused:** a `sort` in a pipe, `git check-ignore`, and a `sed -E` substitution. The last
  is why the carried section 4 keeps its own heading levels.
- **Every other §5 item matched the tree:**
  - the phase, with step 0 `partial`;
  - the six `TheTopRowTests` and the eight `TheWorkingPanelsTests` names;
  - `BindingHealthTests` and `VoiceTests` at their paths, with 1 and 5 tests;
  - `RigDriveAndPower`, `GreenZoneBand`, `GreenZoneRegions`, `DigitalSendReserved`,
    `DigitalHeaderStrip` and `DigitalReadinessStrip` in `MainWindow.axaml`;
  - `GreenZone.RuleOfThumb`'s sentence;
  - the three uncommitted launcher files.

### Decisions this unit made for itself, marked as its own and overrulable

1. **What "assert what the width rule draws" means at 1400.** With the sparkline drawn, no line
   beside it wraps. With it hidden, *heard just now* stands over the count.
2. **Ruling 5's place, as asserted.** The send area has the same parent as `ModeTabs`, is right of
   the tabs and is above the working card, and CQ and Stop have no collapsible ancestor.
3. **The three new stop reds were run once more, alone**, to tell a race from a steady red. They
   were not otherwise chased.
4. **Unit 338's section 4 is carried with its headings at their own levels**, because the command
   that moves them down one level was refused. Its words are unchanged.

## 2. What the owner should expect

**Nothing on the screen changed.** Only `TheTopRowTests.cs` changed, plus the version, the status
files and this report.

**What is now true:** the green block, the world clock, drive and power, and CQ and Stop's place
are each checked by a named test at 1920 and at 1400. So a later change that breaks one of them at
either width turns a test red.

**What will look wrong but is not:**
- **The licensed 1400 top row prints 219 px on some windows and 228 on others.** The best bet
  ranking reads the real clock, so its line draws on some test windows. Both are under the
  mockup's 0.262. See section 4, item 2.
- **The count printed 4 stations on one window, against the 6 the fixture sets.** That is the
  parked live count, unit 338 item 4. No assertion depends on it.
- **Three stop tests go red when run with the layout classes, and pass alone.** See section 4,
  item 1.
- **The carried section 4 below has headings at the same level as this unit's.** The words are
  unit 338's.

**Build and tests:**
- It builds.
- TheTopRowTests 6 of 6, TheWorkingPanelsTests 8 of 8, BindingHealthTests 1 of 1, VoiceTests 5 of
  5, TheAchievementsPageClicksInTests 8 of 8.
- Carry-forward app 100 of 100 and engine 85 of 85.
- Pushed to `main`: `38b4427`, `9841934` and `c8c6b97`, each push accepted. The report's commit
  follows.
- The trx files are under the ignored `testresults\`. No file was created that cannot be deleted.

## 3. What you should see

**The answer: yes.** Every step 0 exit criterion now has a named test that is green at the widths
the criterion names. Criteria 2 and 3 gained their 1400 assertions in this unit.

Two sub-clauses are held more weakly than the criterion reads. Neither is a miss by a measured
amount:
- *full to the status bar* at 1400 is printed and not asserted, and the print shows a gap of 0 px;
- the power offer's place under the S-meter is asserted by containment, because the offer is not
  drawn on the FT8 fixture.

**No visible change.** This unit only makes the tests catch a regression at 1400 later.

**Every appearance claim in this report is computed on the headless test host at 1040 px tall, not
seen.** The mockup's side of the Countries comparison is its words and layout, read from the ruling
picture, and no pixel of it is measured.

### The table from task 0, as it stands after task 1

Only these rows changed from section 1's table. Every other row stands with the same result.

| Test | Criterion | Widths | Result | Numbers |
|---|---|---|---|---|
| TheGreenBlockIsInsideTheCardUnderTheStripWithTheBandLargest | 2 | 1920 and 1400 | pass | 1920: block 1038 x 58, sparkline 110 x 26 drawn, nothing wraps. 1400: block 518 x 91, sparkline hidden, *heard just now* 140 x 9 at y 268 over the count at y 277, license line 3 lines, rule of thumb 2. Band 20 px, next largest 15, at both |
| TheWorldClockIsAtTheCardsRightEndWithOneMarker | 2 | 1920 and 1400 | pass | clock 246 x 134, 1 marker, 15 px from the card's right edge at both; the block ends at x 1069 at 1920 and x 549 at 1400, and the clock starts at 1083 and 563 |
| DriveAndThePowerOfferAreUnderTheRigAndTheSendAreaKeepsCqAndStop | 3, ruling 5 | 1920 and 1400 | pass | rig panel / card 190 / 190 and 219 / 219; drive box at y 274 under y 257 at both; power offer inside the panel, not drawn; send area 22 px tall right of the 250 px tabs, above the working card |
| AtFourteenHundredTheLicensedTopRowIsTheMockupsShare | 3, 5 | 1920 and 1400 | pass | 1920: 190 (0.209), panels 503 (0.553) hidden / 450 (0.495) showing. 1400, with the best bet line: 228 (0.251), panels 465 (0.511) / 412 (0.453) |
| BindingHealthTests | 6 | - | 1 of 1 | - |
| VoiceTests | 6 | - | 5 of 5 | - |
| TheOperatorCanStopItTests.TheStopAddedNoNewRouteToATransmission | not on criterion 6's list | - | red | two `_armedSend.Arm(` lines |

### Step 1's exit criteria against the tree

The tests in the third column are unit 335's and 336's in `TheCategoryPagesAreTradingCardsTests`.
**This unit did not run them** (HM-DEC-155), so their results today are not measured.
`TheAchievementsPageClicksInTests` did run, 8 of 8.

| Criterion | Where it lives | Named test | What is missing |
|---|---|---|---|
| Color band with count, score, level and a bar to the next level | `AchievementsWindow.axaml` `AchievementsCategoryBand` (lines 253-306); `AchievementCategory` builds `BandLine`, `LevelBarLine` and `LevelFraction` (lines 181-213) | `EveryKindsBandCarriesCountScoreLevelAndABar` | Nothing the criterion names. Where there is no next level, words stand in for the bar. The gap (*14 to Silver*) is said over the bar, not also on the line, as unit 335 chose. |
| Earned card: entity, callsign, grid, path map cropped to the two stations, distance, band, mode, date, points, from the log entry | `AchievementCategory.EarnedBy` (lines 939-976); card template lines 372-588; the map is `Ft8GlobeControl Opened="True"` over the conversation card's `Ft8GlobePlot` | `EveryEarnedCardIsTheContactThatEarnedIt` | Nothing the criterion names. The crop is the popup's path-fitted frame, and whether it is cropped to the two stations was not measured here. A record with no grid gets a word and a contact list in place of the map. |
| Next card: its want and its CQ callers with distance, or *no one is calling from there now* | `AchievementCategory.NextCard` and `CallersFrom` (lines 835-868); `NoOneCalling` (line 801) | `TheNextCardKnowsWhoIsCalling` | Nothing the criterion names. It lists up to 3 callers, then *and N more*. With no list read it says *the CQ list was not read*; States says *Hamlet cannot tell a caller's state* (carried, unit 335 item 1). |
| All eight kinds; Continents opens to seven badges and each to its countries | `AchievementCategory` kind switch (lines 376-415); `AchievementsSubBadges` | `TheOtherFiveKindsEachDrawTheirOwnCards`; `ContinentsOpensToSevenAndEachToItsCountries` pass today | Not measured beyond those tests. |
| No clipped or word-wrapped string at 1400 and 1920 | `NoStringClipsAndNoCardIsWhiteAtFourteenHundredAndNineteenTwenty` realizes the dialog at each width | that test; `NoStringInAnySlotIsClippedAtTheWindowsSize` pass today at 1040 | The window is `Width="1040" Height="720"` in markup, and nothing sizes it from the main window. So 1400 and 1920 are reached only by the test setting the dialog's own width (carried, unit 332 item 3). |
| No white-rectangle card | the same test counts cards with no map, bar or list; the `HasNoMap` list border (lines 433-458) | same | Nothing the criterion names. |
| `achievement_category_opened` with the kind and the count of cards | `AppEvents.AchievementCategoryOpened` (lines 1122-1128) | `OpeningACategoryWritesTheKindAndTheCardCountAndNothingElse` pass today: Countries, 9 cards | Nothing. |
| Nice-to-pass: a card's map opens in the popup on click | nowhere | none | All of it. `Ft8GlobeControl` handles no pointer or command, and the card template holds no `Popup`. The only map popup is the conversation card's: `Ft8ContactCard.MapIsOpen` (line 580), bound at `MainWindow.axaml` line 5645. |

### The Countries page beside `assets/category-page-countries.png`

| Region | The mockup draws | The tree draws | The gap |
|---|---|---|---|
| Back | `‹ All achievements`, a plain link, top left | `AchievementsBack`, an `hm-chip` button showing `BackLabel` | A chip, not a link; its words were not read |
| Band | A red rounded band; a three-flag emblem; `Countries` large; `one card per DXCC entity · 11 worked · 80 pts · Bronze · 14 to Silver`; on the right `11 of 25 to Silver` over a bar | The kind's color, corner 10; a 40 x 40 `BadgeEmblemControl`; the name at 22 bold; `BandLine` at 12; `LevelBarLine` over a 300 x 12 bar; ink computed to 4.5:1 | The `14 to Silver` clause is not on the line where a bar is drawn. Which glyph the Countries emblem draws was not read. |
| Layout | Two columns of cards | `UniformGrid Columns="2"`, scrolling inside the category | None |
| Earned card | Color edge; entity bold with points in green at the right; `D2IM · JI61`; the map across the card; the distance large, with `20 m · FT8` and the date beside it | A 6 px edge in the kind's color; title 18 bold; points 13 semibold green; `CallGridLine` 12; the globe `Height="170"`, left-aligned; distance 28 bold, with band and mode, and date beside it | The map is a fixed 170 px tall and left-aligned, where the mockup spans the card; its drawn width was not measured |
| Next card | Grey edge; `next`; `Your 12th country`; points; *Any country you have not worked. On the CQ list they carry the green quill.*; a grey panel *calling CQ right now, unworked:* with a green quill, the country, and `call · miles` per row | Grey `#C9C5BA` edge; `next` as the figure; title from `AchievementBadgePage.NextWords`; `WantsLine` *Any country you have not worked*; a panel of at least 120 px, headed *calling CQ at <time>, unworked*; a mark (the door or counter form), the place at 14 and `call · miles` at 12 | The quill sentence is absent. The heading carries the time the list was read, where the mockup says *right now*: unit 335's choice, because the window is modal and the list is not live. The title's words for this fixture were not read. |

### The three largest gaps, by size of build

The sizes are this unit's estimate. No approach is proposed.

1. **The map opening in a popup on click** (the nice-to-pass). There is nothing to click. The globe
   takes no pointer input, the card template has no popup, and the only map popup belongs to the
   main window's conversation card.
2. **The earned card's map at the card's width.** The tree draws a 170 px tall globe at the left;
   the mockup draws the map across the card. Every earned card's height depends on it, and those
   heights are what the no-clip and no-white tests measure.
3. **The next card's words against the mockup.** The quill sentence is missing, *right now* is a
   read time, and the band line drops *14 to Silver* where a bar is drawn. These are strings, and
   two of them were deliberate choices in unit 335.

## 4. What's blocking us

### Raised by this unit

**1. Three `TheOperatorCanStopItTests` go red when run with the layout classes, and pass alone.**

*No ruling wanted; a finding.* In one filter with `TheTopRowTests`, `TheWorkingPanelsTests`,
`BindingHealthTests` and `VoiceTests`, three tests failed:
- **`AClickBeforeTheBoundaryUnarmsItAndNothingGoesOut`**: after the click the send line was
  unchanged, nothing was on the wire, and the slot was still armed. The click did not reach Stop.
- **`TheLineSaysWhatHappenedToTheCarrierAndToTheSound`**: the line read on the click was already the
  boundary's final sentence.
- **`AClickWhileTheToneIsPlayingStopsTheSoundAndNotJustTheCarrier`**: the wire held a second stop
  pair (`17 FF`, `1C 00 00`) before the test read it. The audio stopped 98 ms after the click, and
  the run said `Cancelled`.

In the last two, the run finished inside the click's own dispatcher pump, so the run's closing
line and frames landed before the test read them.

Alone, all three pass, 3 of 3. **Whether they were red before unit 338 moved Stop to the tab row is
not measured**, because `git stash` is refused. In the same run, Stop was hit where it is drawn and
`PressingItTwiceSaysWhatTheSecondPressFoundAndStillTellsTheRadio` passed its press before the
boundary.

They are transmit-side tests, parked by §9, and nothing on the transmit path was touched.

**2. The best bet ranking reads the real clock inside the test fixtures.**

*No ruling wanted; a finding beside parked items 1 and 4 of unit 338, raised once.*
- `MainWindowViewModel.RankBands` passes `DateTime.Now.Hour` and the current UTC.
- So on some licensed windows this evening the 40 m badge, and *best bet now: 40 m* in the green
  block, were drawn.
- That takes the 1400 top row from 219 to 228 px (0.241 to 0.251), and the panels from 474 to 465
  px.
- Nothing failed, and both are inside the criteria. A test that asserts the 1400 share more
  tightly would move with the time of day.

**3. `tools/status.sh` is refused for the fifth unit, and would write a stale `RULES_AT` if it ran.**

*No ruling wanted; a finding.* The script hard-codes `RULES_AT: HM-DEC-161 (2026-09-11)`, and the
decision log is at 163. Every `UPDATED` in this unit is a `date` reading pasted whole, and none was
composed.

**4. Two step 0 sub-clauses are asserted more weakly than the criterion reads.**

*No ruling wanted; a finding for whoever writes step 0's verdict.*
- **Criterion 5's *same shape*, at 1400: the panels running full to the status bar is printed, not
  asserted.** `TheThreePanelsShareOneTopAndOneBottom` asserts the floor at 1920 only, and at 1400
  it prints the panels ending at y 953, which is the floor.
- **Criterion 3's power offer under the S-meter is asserted by containment in the rig panel.** The
  licensed fixture is FT8, where the PSK31 offer is not drawn (0 x 0), so no rectangle for it
  exists to compare.
- Neither was extended, because task 1 named three tests and R14 adds no others.

**5. Step 1's nice-to-pass has nothing in the tree behind it.**

*No ruling wanted; a finding for the arbiter authoring step 1.* See section 3: the globe on a
trading card takes no click, and the only map popup is the conversation card's.

### Where the carried items stand after unit 339

- **Unit 338 items 2 and 3: ANSWERED by the arbiter's ruling in work instruction 339** (rulings 4
  and 5). Both are marked in the carried text below. Nothing was built for either, and what unit
  338 built stands. Item 3's place is now asserted at both widths in
  `DriveAndThePowerOfferAreUnderTheRigAndTheSendAreaKeepsCqAndStop`.
- **Unit 338 item 1, the live license lookup:** parked, untouched. The plain fixture read
  `License class unknown` on every trace window in this unit's run.
- **Unit 338 item 4, the heard count:** parked, untouched. It read 4 stations on one licensed
  window against the 6 set. No assertion depends on the number.
- **Unit 338 item 5, the name scope:** unchanged.
- **Unit 338 item 6 and unit 337 item 3, the reds:** `TheStopAddedNoNewRouteToATransmission` is
  still red. The four expected reds were not run. See item 1 above for three more.
- **The status helper:** still refused. See item 3 above.
- **Carried item 18, the id schemes, and `CPS-DEC-0163`:** untouched. See section 1.
- **Every other item stands as carried.** Nothing in this unit touched:
  - the radio side, a decoder, a parser or the transmit chain;
  - States, the achievements pages or the points file;
  - the demodulator, the ALC margin or PSK31 step 6.

### Asks still outstanding - carried from unit 338's section 4, per HM-DEC-139, verbatim

The words are unit 338's, from its line under `## 4. What's blocking us` to its end, except that
items 2 and 3 are marked answered, as work instruction 339 §3 asks. **The headings keep their own
levels**, one level too high for this nesting. The shell commands that would have moved them down,
or appended the committed text, were refused in this environment. So the text was copied in with
the file editor from unit 338's report as committed in `f7f6eb6`.

### Raised by this unit

**1. The plain test fixture asks callook.info for KC3QIS's license class, and the answer changes
what it measures.**

*No ruling wanted; a finding.*
- The view model's constructor looks up a callsign that has no class. General lands inside the
  layout passes on some windows and not others.
- That removes the green block's third line and the send area's guard sentence, 67 px at 1400,
  which is unit 337's run-order spread.
- Setting the callsign after the constructor did not stop it, so a second path exists and was not
  found.
- This unit's criteria hold in both states.
- A seam that keeps tests off the network would change the view model's start-up, which is not
  this step's work.

**2. The filter chips are in the mode strip, not in the Decoded text header where the mockup draws
them.**

**ANSWERED by the arbiter's ruling in work instruction 339** (ruling 4: the filter chips stay in the
mode strip, option (a)). Nothing was built for it, and what unit 338 built stands.

*Ruling wanted only if the mockup's placement is what you want.* On the host the header is 376 px
inside. With rows, `everything`, `CQ`, the order toggle and `clear` want about 430 px before the
title, and the summary a shut panel shows would be cut (§R17). The options:
- (a) keep them in the mode strip, which does not collapse;
- (b) put only `everything` and `CQ` in the header, about 188 px, which leaves the title and
  summary about 50 px, and keep the row controls in the strip;
- (c) shorten the chips' words.

*Reasoning.* §R17 says the filter is visible on an empty list and a shut header carries its count
and sentence. Option (a) keeps both at every width, so the recommendation is (a).

*What was rejected and why.* A second row inside the Decoded text panel, above the list. It would
be inside what collapses, and the filter must stay on screen when the panel is shut.

**3. CQ and Stop sit right of the tabs, in a place the mockup draws empty.**

**ANSWERED by the arbiter's ruling in work instruction 339** (ruling 5: CQ and Stop stay right of the
tabs). Nothing was built for it, and what unit 338 built stands.

*Ruling wanted only if that is not where you want them.* The mockup draws no send area. Over the
waterfall it was a row all three panels paid for.

*Reasoning.* Beside the tabs it costs no height, it is outside everything that folds, and it is on
screen whenever the Digital tab is (§0.2).

*What was rejected and why.*
- The For you or waterfall panel: both collapse, and Stop may never be inside something that does.
- The status bar: every tab shares it, and it would grow.

**4. The licensed fixture's heard count read 8 and then 9 stations on two runs at 1920, against
the 6 the fixture sets.**

*No ruling wanted; a finding.* Something live replaces the fixture's count after it is set. The
top-row test asserts only that a count is shown, so nothing failed. The green block at 1920 was 62
px before task 2 and 67 after, and the top row stayed at 190.

**5. The window's name scope does not find the controls inside the neighborhood card.**

*No ruling wanted; a finding for whoever next writes code-behind there.* `FindControl` returned
nothing for `GreenZoneRegions`, and a walk of the visual tree finds it.

**6. Four expected reds were not run.**

*No ruling wanted; a finding.* `TheWholeChainRunsFromOneRightClickTests`,
`TheMenuIsUnderTheMouseTests`, `ThePsk31RecordsAppearTests` and `TheTotalMilesTests` are not this
unit's tests (HM-DEC-155). `TheStopAddedNoNewRouteToATransmission` ran and is still red.

### Where the carried items stand after unit 338

- **Unit 337 items 1 and 2: ANSWERED by the arbiter's ruling in work instruction 338.** Both are
  marked in the carried text below, with what was built.
- **Unit 337 item 3, the reds:** `TheStopAddedNoNewRouteToATransmission` is still red. The two
  `TheWholeChainRunsFromOneRightClickTests` were not run.
- **Unit 332 item 4, the license phrase at 1400:** its wording is unchanged. Its line count at 1400
  was not measured after task 2.
- **The status helper:** still refused.
- **Carried item 18, the id schemes:** untouched. See section 1 on `CPS-DEC-0163`.
- **Every other item stands as carried.** Nothing in this unit touched:
  - the radio side, a decoder, a parser or the transmit logic;
  - States or the achievements pages;
  - the demodulator, the ALC margin or PSK31 step 6.

### Asks still outstanding - carried from unit 337's section 4, per HM-DEC-139, verbatim

Headings under it are moved down one level so they sit inside this one. The words are unchanged,
except that items 1 and 2 are marked answered, as work instruction 338 §3 asks.

#### Raised by this unit

**1. At 1400 a licensed operator's top row is 273 px - 0.300 of the height below the band pills -
against the mockup's 0.262.**

**ANSWERED by the arbiter's ruling in work instruction 338** (rulings 2 and 3). Built: the rule of
thumb is option (a), the mockup's sentence, which alone gave 256 px (0.281); then the sparkline
hides where the text column would wrap and *heard just now* stands over the count, giving 219 px
(0.241). The count stays at both widths.

*Ruling wanted, or the arbiter's own recommendation taken: how to shorten the green block at 1400.*
The block wraps because its text column is about 210 px wide there. The clock takes 246 px and the
count with its sparkline about 270. On the host the license line takes 4 lines and the rule of
thumb 6. The options, each measured against those numbers:
- **(a) Shorten the rule of thumb to the mockup's own words**: *Rule of thumb: 20 m and up want
  daylight along the path; 40 m and down want dark.* That drops *the gray edge is where both
  happen*.
- **(b) Take the sparkline off**, task 1's named drop candidate, keeping the count. That widens the
  text column by about 120 px.
- **(c) Accept it.** The host draws text about half again wider than the glass, so on your screen
  the block is probably shorter. That is an inference.

*Reasoning.* §6 says a string that will not fit is shortened and named, or widened, never clipped.
The recommendation is (a): the mockup is the ruling, and it already draws the shorter sentence.

*What was rejected and why.*
- Hiding the rule of thumb behind a mark: the mockup shows it on the card.
- Shrinking the world clock at narrow widths: its size is the mockup's.
- Rewording the license line: it is `PrivilegeStatus.Detail`, the regulation's own sentence in
  the engine's words.

**2. "The working panels" is read as the working card. Read as the three panels themselves, they
are under half the height below the pills at both widths.**

**ANSWERED by the arbiter's ruling in work instruction 338** (ruling 1: the three panels
themselves). Built: the row above the panels is gone - the send area rides the tab row, the filter
and row controls the mode strip, the slot clock the For you header - and the panels are 503 px
(0.553) at 1920 and 474 px (0.521) at 1400 on the licensed fixture, readiness strip hidden.

*Ruling wanted only if the panels themselves were meant.*
- **At 1920** the working card is 573 of 910 px (0.630). The three panels are 363 (0.399).
- **At 1400** the card is 534 to 555 (0.587 to 0.610). The panels are 307 to 374 (0.337 to 0.411).
- **What stands between them**, measured alone at 1400: the mode strip 32 px, the readiness strip
  43, the send area 54 and the filter bar 34. The readiness strip shows only while there is
  something standing between you and a decode; on the test host there is no radio.

*Reasoning.* R26 says *the working panels below the tabs take the rest of the window*, and the
region below the tabs is the working card, so that is what the test asserts. Both numbers are
printed on every run.

*What was rejected and why.* Folding the send strip or the filter bar into a panel header to win
back height: collapsing that panel would then hide Stop (§0.2), or the filter (§R17).

**3. Three reds off the carry-forward list are older than this unit.**

*No ruling wanted; a finding.*
- **`TheOperatorCanStopItTests.TheStopAddedNoNewRouteToATransmission`** expects one
  `_armedSend.Arm(` line in `src`. There are two, at `MainWindowViewModel.cs` 14202 and 14381, in
  a file this unit did not touch.
- **`TheWholeChainRunsFromOneRightClickTests`, two tests,** find *mine rows: 1; realized row roots:
  0*. That is the `DigitalMineRows`-behind-*show the messages* cause of unit 331's item 14. This
  unit's diff touches none of that path. **Not proved by a run at HEAD**, because `git stash` was
  refused. Both tests also open a real audio endpoint.

#### Where the carried items stand after unit 337

- **Unit 336 item 1, how the decoded list and For You share the tab: ANSWERED by R26 and this
  unit.**
  - The tab is `*,383,*`.
  - At 1920 the facts sit beside the map, with the card 678 px inside.
  - At 1400 they go under it, with the card 419 px inside.
  - No callsign is cut at either width.
  - The outer `*,*` was superseded by the mockup, Tim's own, not by this unit.
- **Unit 331 queue item 2, the 1400 width arithmetic:** superseded the same way. Its numbers are
  replaced by section 3's.
- **Unit 334 items 1 and 2, the map's height at 1920 and its 30 px at 1400:** superseded by R26.
  The world clock is 246 x 134 at the card's right end at both widths.
- **Unit 332 item 4, the license phrase at 1400:** still wraps, now 4 lines on the host at 1400 and
  1 at 1920. See item 1 above.
- **The status helper:** still refused. Every `UPDATED` is a `date` reading pasted whole.
- **Every other item stands as carried.** Nothing in this unit touched:
  - the radio side, a decoder or a parser;
  - the transmit logic (CQ, Stop, the drive and the power offer moved in markup only, on the same
    commands);
  - States, rank names, the Modes card, the achievements pages;
  - the demodulator, the ALC margin, the id schemes, or PSK31 step 6.

#### Carried from unit 336's section 4, per HM-DEC-139 - verbatim

Headings under it are moved down one level so they sit inside this one; the words are unchanged.

##### Raised by this unit

**1. Task 3 cannot meet its must-pass with a fraction alone, measured. Which bend do you want?**

*Ruling wanted: how the decoded list and For You share the tab, given the numbers.* On the test
host, with the conversation card's table as it is:
- **For the table to sit beside the map at 1920**, For You needs 624 px or more. That leaves the
  decoded column at most 302 px of the 931, which is a fraction of 0.324.
- **At 1400 that same fraction** leaves the message 34 px, and a six-character callsign needs 60.
  So one fraction cuts a callsign at 1400 or puts the table under the map at 1920.

The options, each measured against those numbers:
- **(a) A star split with a minimum width on the decoded column.** At 1400 the table goes under
  the map and the message shows callsign and grid. At 1920 the table is beside the map, and the
  message is abbreviated there too, because 119 px does not hold a full 200 px FT8 line.
- **(b) Keep the table under the map at 1920 as well.** This answers R24's *beside at 1920* with
  no.
- **(c) Shorten the table's two widest rows.** They are `Last heard` and `4,500 miles ·
  northeast`, and they set its 338 px. That is the conversation card, not this step.

*Reasoning.* R24 says the split is a fraction and the table goes under only when abbreviation is
not enough. Measured, abbreviation is not enough at 1400 at any fraction that also gives the table
its room at 1920. The recommendation is (a). It is the smallest bend, and it keeps R24's order: For
You gets what the table needs first, and the decoded list abbreviates rather than cutting.

*What was rejected and why.*
- Building a fraction and reporting the miss: §6 allows that only for a little miss, and this is
  not one.
- Taking width from the waterfall: `DigitalPanes` `*,*` is your ruling, and it stays yours (unit
  331 queue item 2).
- A half-built layout, which the instruction names as a failure.

**2. The Modes next card names PSK31 on a log with no PSK31 contact, and a test says §3.1 forbids
that.**

*Ruling wanted: which rule holds on that card.*
- `ThePsk31RecordsAppearTests.WithNoPsk31ContactNoPsk31CardIsOnTheScreenAndNoneIsDimmed` is red,
  with `modes draws [PSK31] before any PSK31 contact`. It is unit 333's test of *absent, not
  dimmed*.
- The line it catches is unit 335's `bcaf39d`, which does what R22 asks: the Modes next card says
  *where the unearned mode lives and who is there*, as `PSK31 3.580 on 80 m`.

*Reasoning.* R22, 2026-09-12, is later than §3.1, and §6 says the later ruling wins. But the test
was not rewritten when the card changed, so the tree now asserts both. This is the same collision
unit 333 raised for Hall of Fame's `A PSK31 contact`, on a second card. Nothing in this unit
touched Modes.

*What was rejected and why.* Rewriting the test or the card here: §12.6, and neither is this
step's.

**3. `TheTotalMilesTests.TheBadgeSaysZeroMilesOnAnEmptyLogAndTheLowestTierIsNext` has been red
since unit 332.**

*No ruling wanted; a finding.* It expects the Total Miles badge line `grid to grid, added up`, and
unit 332 (`3ea16ec`) changed that line to `every mile, added` in `src` only. It is not on the
known-reds list, and the unit that next touches Total Miles owns it under R12.

##### Where the carried items stand after unit 336

- **Unit 335 item 1, the States next card's wording:** unchanged. The card still says `Any state
  you have not worked` and `Hamlet cannot tell a caller's state`. Scoring `STATE` changed only the
  earned cards in front of it.
- **Unit 331 queue item 3, States scoring nought: ANSWERED by unit 336.**
  - **What counts now.** `STATE` is read, and scores on a United States, Alaska or Hawaii record as
    one of the fifty codes.
  - **The fixture's numbers.** Of five records, 2 score, for 12 points, and the badge reads `2
    worked`.
  - **The item's worry, confirmed.** Hamlet's own entries carry no `STATE`, so they score none.
  - The item stays in the queue as carried.
- **Unit 331 queue item 2, the 1400 width arithmetic:** stands, and the outer `*,*` option stays
  rejected. This unit's measurement updates its numbers:
  - the card has 227 px inside at 1400 and 487 at 1920;
  - the table now wants 338 px, so it sits under the map at both widths.
  - See item 1 above.
- **Unit 333 item 1, the Hall of Fame next-card collision:** untouched, and now joined by item 2
  above on Modes.
- **The status helper:** not tried. Every `UPDATED` in this unit is a `date` reading pasted whole,
  and none was composed.
- **Every other item stands as carried.** Nothing in this unit touched the radio side, a decoder,
  the FT8 or PSK31 message split, or the transmit chain.

##### Carried from unit 335's section 4, per HM-DEC-139 - verbatim

Headings under it are moved down one level so they sit inside this one; the words are unchanged.

###### Raised by this unit

**1. The States next card says `Hamlet cannot tell a caller's state`. That wording is the
arbiter's proposal, marked for you, and shortened here to fit.**

*Ruling wanted: keep it or reword it.*
- A CQ carries no state, so the card cannot know who is calling from a state you have not
  worked.
- The proposal read *Hamlet cannot tell a caller's state from the air*. That needs 480 px, and
  the slot at the window's 1040 is 426, so *from the air* came off.

*Reasoning.* *No one is calling from there now* would assert something nobody measured (§0.0).
Guessing a state from a prefix was rejected before: a `W3` can be anywhere.

*What was rejected and why.*
- A wider slot. That would change every next card's width for one sentence.
- A second line. The fit rule says no string wraps.

**2. The next cards name callers from continents you have never worked.**

*Ruling wanted, only if the 2026-09-10 rule was meant to hold here.*
- That rule says the CQ list must not be what tells you an area exists. So a decoded-list row
  from a never-opened continent wears the ringed door and names nothing.
- On a Countries or continent next card, the same caller is named by country, beside the same
  ringed door.

*Reasoning.* R22, 2026-09-12, asks the next card to list who is calling from a place that would
earn it. It also asks each unearned continent to name *who is calling from it now*, which cannot
be done without naming the place. §6 says the later ruling wins.

*What was rejected and why.* Leaving door callers off the Countries card. That would hide the
one caller who earns two cards at once, and Continents could not do what R22 asks of it.

**3. The opening page's Hall of Fame badge still has white text on gold, at about 3.6:1.**

*No ruling wanted; a finding.*
- This unit's category band computes its ink and turns dark on that gold.
- The eight badges on the opening page, and their shared template, are unit 331's and step 0's.
  They were not touched, so the badge's name there still reads white on `#A8811A`, under §0.6's
  4.5:1.

*Reasoning.* §12.6: do not repair unrelated things on the way past. The fix is one binding, and
it belongs to a unit that is told to change the page.

###### Where the carried items stand after unit 335

- **Unit 333 item 1, the Hall of Fame next-card collision:** untouched. The next first is still
  chosen by `NextFirstOf`, and `A PSK31 contact` still shows where it is the only first left.
  This unit added a line under it (where PSK31 lives, and its callers) and did not change which
  card it is.
- **Unit 332 item 1, the `Why` hovers:** unchanged. No card draws `ModeFirstRow.Why`, and a test
  holds `cannot work` off the Modes cards. The sentences themselves are untouched in
  `AchievementsViewModel.cs`.
- **Unit 331 queue item 3, States:** unchanged. States still scores nought, and its next card
  says Hamlet cannot tell a caller's state. See item 1 above.
- **Unit 332 item 3, the achievements window width:** it is still 1040 by 720. This unit
  measured the pages at 1400 and 1920 by setting the dialog's own width, because nothing sizes it
  from the main window.
- **The status helper:** not tried again. Every `UPDATED` in this unit is a `date` reading pasted
  whole.
- **Every other item stands as carried.** Nothing in this unit touched the radio side, a decoder,
  a parser or the transmit chain.

###### Carried from unit 334's section 4, per HM-DEC-139 - verbatim

Headings under it are moved down one level so they sit inside this one; the words are unchanged.

####### Raised by this unit

**1. At 1920 the green zone is 310 px tall, because the map keeps its shape as it takes the
pills' width.**

*Ruling wanted: cap the map's height or not.* Computed: the map is 492 x 269 at a 1920 window,
and the panel goes from 151 to 310 px. At 1400 it goes from 193 to 177 px, so there it is
shorter.

*Reasoning.* R21 says the map takes the space the pills held, and at 1920 that is 218 px of
width. A map drawn at its own proportions cannot take width without height, and stretching it
would move every place off the pixel the projection puts it on (HM-DEC-092).

*What was rejected and why.* Choosing a cap here. A height limit is a number about how much of
your screen the panel may take, and it is yours to pick.

**2. At 1400 the map gained only 30 px, because the count and its sparkline set the right
column's width, not the pills.**

*Ruling wanted, if 30 px is not the bigger map you meant.* The right column is 260 px after,
against 262 before.
- The sparkline is 110 px of it plus a 10 px gap.
- Dropping the sparkline, the task's named drop candidate, would free that width.
- The left block and the map share freed width equally, so the map would gain about half of it.
  That is arithmetic on the measured widths, not a measurement.

*Reasoning.* The task says keep the sparkline if it still fits, and it fits, so it stayed.

*What was rejected and why.* Dropping it anyway, which the task does not allow while it fits.
Stacking the count under the sparkline, which rearranges the count beyond what was asked.

**3. `tools/status.sh` is still refused, in all three spellings.**

*No ruling wanted; a finding, the same as carried item 5 below.* `sh`, `bash` and `./` all came
back *requires approval*. Every `UPDATED` in this unit is a `date` reading pasted whole, and none
was composed. The validator was run by the `.proj` route; its verdict is in the session
transcript, not quoted here.

####### Carried from unit 333's section 4, per HM-DEC-139 - verbatim

Headings under it are moved down one level so they sit inside this one; the words are unchanged.

######## Raised by this unit

**1. Where PSK31 is the only Hall of Fame first left unearned, Ruling C and §3.1 say opposite
things about one slot.**

*Ruling wanted.* The slot is the Hall of Fame badge's next card, and Hall of Fame's unearned
card inside the category. The exact string is `A PSK31 contact`.

It arises on a log holding *Your first contact*, *A DX contact*, *A Morse contact*, *Over 5,000
miles* and *Over 10,000 miles* but no PSK31 contact, for instance an imported CW log with long
contacts.
- Ruling C, Tim, 2026-09-12: *every kind shown, the nearest unearned card in each, nothing
  beyond it.*
- `ACHIEVEMENTS_PHILOSOPHY.md` §3.1: *absent, not dimmed. No PSK31 card exists until the first
  PSK31 contact.*

*Reasoning.* On every other log the two agree: the badge shows the nearest first §3.1 allows.
In this one case, showing the card breaks §3.1 and showing nothing breaks Ruling C. The
instruction says not to choose, so **the screen is left as it was and still shows
`A PSK31 contact` there.**

*What was rejected and why.* Showing no next card, which is choosing §3.1. Moving `first_psk31`
last in the list, which only moves the collision to a later log and reorders the owner's
firsts.

**2. The nice-to-pass wants a logger, and this session could not look for one.**

*Something you can do in a minute, not a stop.* Import `docs/unit333-psk31-fixture-export.adi`
into a new, empty log in any logger you already have, with the five steps in section 2. Name
the logger and its version, and say whether it took both records with PSK/PSK31, both RSTs
and the grid. That closes the criterion.

*Reasoning.* The instruction forbids installing one, and the permission layer refused every
listing outside `C:\Source\HamLet` and the registry query.

*What was rejected and why.* Downloading or building a logger, which is your decision about
your machine. Guessing from memory which loggers are installed, which would be a claim nobody
measured.

**3. This environment refuses deletes inside the repository and listings outside it, so two
scratch files remain.**

*No ruling wanted; housekeeping.*
- `tests/Hamlet.App.Tests/Views/Unit333ProbeTests.cs` is untracked and one comment line.
- `artifacts/unit333/psk31-fixture-export.adi` is gitignored.

Both are safe to delete by hand, beside the five carried in item 19 below. The validator was
run by the `.proj` route the instruction names. The prompt's `.bat` spelling is the one unit
243 documented as mangled by Git Bash.

######## Carried from unit 332's section 4, per HM-DEC-139 - verbatim

**1. The mode rows' hovers still say Hamlet cannot work PSK31 and FT4 and cannot log CW.**

*Ruling wanted on whether to rewrite them.* `AchievementsViewModel.Why` has *Hamlet can tune
you to the PSK31 watering holes and cannot work them* and the FT4 and CW equivalents. That is
the same falsity as the sentence task 0 removed.

*Reasoning.* The instruction named the one line and §12.6 says not to repair unrelated things
on the way past. Those hovers are not on the new page, but `ModeFirstRow.Why` is still in the
tree, and a later surface could draw it.

*What was rejected and why.* Rewriting them here. The words about what Hamlet can now work are
a claim about PSK31 and CW, and they want the step 5 and 6 facts behind them, not this unit's
guess.

**2. The continent level names two continents he has not opened.**

*Ruling wanted.* Antarctica and Oceania each get a badge on the fixture, with `A first here`,
`0 pts` and `500 for a first` or `50 for a first`.

*Reasoning.* The instruction says *seven continent badges*, and seven named badges are what the
Continents level is. Its next cards do not name the continent again. But §3.1 says nothing
shows inside a category until something adjacent is earned, and this bends it one step further
than ruling C bends the page.

*What was rejected and why.* Drawing only the opened continents. The instruction asked for
seven, and a five-badge level would read as five continents.

**3. The fit is measured on a host that draws text about half again wider than the glass.**

*Ruling wanted on the window size.* To pass a measured no-clip test there, the achievements
window went from 820 to 1040 wide and nine strings were shortened.

*Reasoning.* The host advances ten pixels a character at every size. A string that fits there
fits on the glass, so the test cannot pass a clip the owner would see. The price is a window
wider than the glass needs.

*What was rejected and why.* Estimating widths for a proportional face instead of measuring.
The instruction says *asserted by measuring*, and an estimate is the thing that let 331's text
clip.

**4. The green zone's license phrase is not on one line at 1400.**

*Ruling wanted.* The instruction says *the license phrase and citation on one line*. At a 1400
window the left region is 253 px, and the phrase lays out to three lines on the test host; I
estimate one or two on the glass.

*Reasoning.* The mockup fits it by rewording it to *General covers digital modes here*.
`PrivilegeStatus.Detail` is the regulation's sentence, and 331 kept it in its own words. One
line at 1400 therefore needs either a shorter sentence or less room for the map and the
buttons.

*What was rejected and why.* Keeping it one line and letting it run past the panel's edge,
which is what the first cut did, at 142%.

**5. `tools/status.sh` cannot run here, and neither could the validator's `.bat`.**

*No ruling wanted; a finding.* `sh tools/status.sh` and `bash tools/status.sh` both came back
*requires approval*. So every status write was `date` and a paste, and none was composed: the
helper exists and the permission layer does not allow it.

The validator was attempted as instructed, `tools\arbiter\validate-output.bat output.md`, and
Git Bash turned the path into `toolsarbitervalidate-output.bat`. It was then run through the
route unit 243 built, `dotnet build tools/arbiter/validate-output.proj -p:Report=output.md`,
and its verdict is the last thing in this session's transcript. It is not reproduced here,
because a report cannot quote a run of itself.

*What was rejected and why.* Composing timestamps as units 327, 328 and 331 did.

**6. Step 5 is `partial` in `PHASE_STATUS.md` and *done* in the instruction.** **ANSWERED by
this unit**: the four must-pass and R13 are proved in section 3, the nice-to-pass is unmet, and
unit 333's `PHASE_OUTCOME.md` entry records `STATE_AFTER: done`. The `STEP: 5` lines are the
launcher's and were not written.

######## Carried from unit 331's queue, as unit 332 carried it - verbatim

**1. Fourteen `UPDATED` timestamps in `PROJECT_STATUS.md` were composed rather than
read from the clock - the third unit running, and this session read both prior
reports of it before doing it.**

*No ruling wanted; reported because it is now a pattern rather than a slip.* The
clock was read at `12:44:45` and at `13:48:41`, and every status write between them
carried an extrapolated time: `13:02`, `13:15`, `13:24`, `13:40`, `13:52`, `14:05`,
`14:12`, `14:30`, `14:44`, `14:58`, `15:12`, `15:30`, `15:52`, `16:25`. **The last of
those is two and a half hours ahead of the true time.** The final write is from the
clock and says so.

*Reasoning.* This defeats the one signal that catches a stopped session, which is the
whole purpose of the ten-minute write - a panel reading `16:25` at `13:48` cannot tell
a working session from a dead one, and would have read unit 330 as alive for two
hours after the watchdog killed it. Unit 327 reported it, unit 328 reported it and
repeated it, and this session did it fourteen times.

*What was rejected and why.* Reporting it as a detail. Three units is a mechanism
problem: the rule says *read from the clock* and the failure mode is that reading the
clock is a separate command nobody budgets for. **The fix that would work is a status
helper that reads the clock itself** - `tools/status.sh` arrived in the seed commit
and this session did not use it, which is its own finding.

**2. The instruction's own width arithmetic cannot hold at 1400 px, and the honest
resolution costs the card 48 px.**

*Ruling wanted.* Task 1a asks for about 460 px inside the card. Measured: the decoded
panes are half the tab, the decoded list needs 383 of them to stop clipping the
longest FT8 line, and 460 inside the card needs about 516 px of panel - so the pair
needs about 899 px, which is a window of about **1856**. At 1400 the arithmetic leaves
**227 px** inside the card, down from 275.

*Reasoning.* Two §0.0 claims are in conflict at 1400 and only one can win: a clipped
callsign on the decoded list is a station misidentified, so the list got what it
needs. **The room the instruction wants exists at your own window width if it is over
about 1850**, and does not below it.

*What was rejected and why.* Taking the pixels from the waterfall. Changing
`DigitalPanes` from `*,*` to `1*,2*` would give the card about 456 px inside at 1400 -
almost exactly the number asked for - but the outer split is your ruling from a phase
ago, and the waterfall would fall from 666 px to 447. **That is the option, and it is
yours, not mine.**

**3. The States badge scores nought because the log does not read `STATE`.**

*Ruling wanted on whether to read it.* An ADIF record carries `STATE` and
`AchievementContact` does not parse it, so the kind has no count and no score. The
badge draws, its next card is *Your first state*, and nought is the honest figure.

*Reasoning.* A state worked out from a callsign prefix would be a claim about where
somebody lives, which the prefix does not support - a `W3` can be anywhere. Reading
the field would work for records written by a logger that fills it; **Hamlet's own
`Ft8ContactLogEntry` does not write one**, so the kind would score for imported
records and not for his own, which is a worse screen than an honest nought.

*What was rejected and why.* Hiding the badge. Eight kinds is the shape you approved,
and a kind that is absent because Hamlet cannot yet count it teaches nothing; a
nought with a first card behind it says what is missing.

**4. `first_answer_to_own_cq` is in your points file and Hamlet can never award it.**

*No ruling wanted; a finding.* The log says a contact happened and not who called
first. Awarding it would mean deciding that from the exchange, which nobody recorded.
It stays in the file because the file is yours and a key Hamlet cannot award today is
a key it may award later - it simply never scores.

**5. The family word on the green zone is `Digital` where task 4's example says
`Data`.**

*Ruling wanted, and it is one word.* See decision 1 in section 1. `ModePalette`'s own
label is what the map legend teaches, and a fifth word for one of four families would
have two surfaces calling one thing two things.

**6. `validate-output.bat` CLOSED - the route has existed since unit 243 and five
units have not used it.**

*No ruling wanted; the ask is answered and the answer was already in the tree.* The
`.bat` invocation was refused again exactly as units 324 to 328 recorded. **Then
`tools\arbiter\validate-output.proj` was found sitting beside it**, written by unit
243 for precisely this deadlock, and it works:

```
dotnet build tools/arbiter/validate-output.proj -p:Report=output.md
    -> VALID - all seven rules passed.
    -> validate-output exit 0
```

*Reasoning.* `dotnet build` is permitted with a wildcard, MSBuild's `Exec` runs a
command, and the `.proj` calls the validator unmodified with its own rules and fails
the build on a non-zero exit. **Nothing was copied, read around or reimplemented.**
Unit 328 wrote *this needs the permission layer changed or a route that is not a
`.bat`*; the route existed, in the same folder, with a 32-line header explaining
itself.

*What was rejected and why.* Applying the seven rules by hand again, as unit 328 did.
A hand-applied rule is applied by the same session that wrote the file, which is
exactly the independence the rule wanted; now that an independent run is available,
the hand-check is worth nothing beside it. **The line for the next unit to carry is
the command above, not the fault.**

**7.** *(was item 1)* **`MainWindow.axaml`'s comment on the mark now says the
opposite of what the code does.** **CLOSED by unit 330 task 1** and re-checked here:
both remaining *filled disc* strings read correctly in context, one of them 330's own
corrected comment.

**8.** *(was item 2)* **`Unit300SizesTests.WhatTheMarkDrawsAtEachSize` is red, and two
tests in this repository assert opposite things about the same mark.** **CLOSED by
unit 330 task 1**, reconciled on option B of 2026-09-10, and 10 of 10 green here.

**9.** *(was item 3)* **The render recorder erases the type of every shape, so a shape
assertion written the obvious way silently passes.** Carried. `DrawingGroup.Open()`
returns every geometry as `PlatformGeometry`, whatever it was drawn as, so
`Assert.IsNotType<EllipseGeometry>` passes against a filled disc. **Bounds are the
honest question**: a circle's are square.

**10.** *(was items 4 and 10)* **Composed `UPDATED` timestamps.** **Carried and
repeated** - see item 1 above, which is the same fault in the same file a third unit
later.

**11.** *(was item 5)* **The demodulator's quality measure vouches for a carrier that
has stopped, for between five and seven seconds, and that now sets how long a dead row
survives.** *Ruling wanted.* `Psk31Demodulator.Quality` is documented as *0.637 on
uniform noise phase and 1.0 on clean keying*. After a loud carrier stops, the input
**is** uniform noise phase and it goes on reporting 0.99, because both of its rolling
means are weighted by magnitude and the carrier's own loud symbols dominate the window
while they decay. **Measured on `psk31-idle-8s-1000hz.wav`: the squelch shut 5.70 s
after the carrier stopped; the quality fell under 0.80 at 7.20 s.** With
`KeepReadableSeconds` on top, `Psk31Listener.RetiredWithinSeconds` had to go from 2.5
to **9.0**. Two fixes would each bring it back under three seconds and neither has
been built: normalizing the measure per symbol changes what every PSK31 decode is
squelched on, and capping how long a vouch may outlive the spectrum contradicts
*retired only when both have lost it*.

**12.** *(was item 6)* **The idle fixture does not reproduce the fault the owner
saw.** Carried. With both new mechanisms switched off,
`psk31-idle-8s-1000hz.wav` still yields one carrier across the whole gap and still
nominates at 1000.0 Hz. **So the keep rule is built from the physics and from his
telemetry, and is proved not to break anything - it is not proved to fix what he
saw.** This is the same ask unit 324 left: **two minutes of his own 14.070 or 7.070,
captured to WAV.**

**13.** *(was item 7)* **`AchievementMarkControl.cs` was taken off the SHA pin, on
unit 327's own judgement.** Carried. Units 330 and 331 have both changed that file
since, under instructions that name it.

**14.** *(was item 8)* **The `Views` reds in `TheMenuIsUnderTheMouseTests` are eight,
not two, and the shared collapse flag was not the cause.** *Ruling wanted on who fixes
it.* Every one of the eight fails at the same line: `expected both decoded lists in
the window, found DigitalDecodedRows`. `DigitalMineRows` lives inside a `ScrollViewer`
gated on `ShowsConversation`, so the right-hand **row** list is realized only after
*show the N messages* is pressed. **That is deliberate** - the For you side became a
panel of cards and the raw rows are one press down, never gone. The test's premise
went stale on the day cards replaced that list.

**15.** *(was items 11, 12, 13)* **Unit 326 items 8 and 9 and unit 325 item 6 -
CLOSED by unit 327** and re-proved in the runs above.

**16.** *(was item 14)* **Unit 324 item 4 - why a 62 dB carrier failed the
keying-shape test. HALF ANSWERED.** The other half still wants a recording and is
item 12 above.

**17.** *(was item 15)* **The ALC margin of 15.** Carried verbatim: built, carried,
**Tim's to overrule**. Nothing in this unit touched it.

**18.** *(was item 16)* **`HM-DEC-161` versus `CPS-DEC-0161` - two id schemes.**
Reported, not repaired. `PROJECT_STATUS.md` carries `RULES_AT: HM-DEC-161
(2026-09-11)`; the other scheme appears in the arbiter's own artifacts. **Nothing in
this repository resolves which is canonical**, and no unit should pick one without a
ruling.

**19.** *(was item 17)* **Files this environment cannot delete.** Now five, listed for
Tim in section 2: `commit-msg-326.txt`, `toolsarbitervalidate-output.bat`,
`tools\arbiter\unit323-append.bat`, `tools\arbiter\unit323-append.py` and this
session's own `tools\cut-header-action.py`.

**All other items stand as unit 328 carried them.**

######## Where the carried items stand after this unit

- **Unit 332 item 1, the `Why` hovers:** not drawn on the rebuilt page before a PSK31 contact.
  Every hover on every visible control was read in task 1 and none names PSK31, so it stays
  parked.
- **Unit 332 item 6, step 5 partial or done:** answered above.
- **Carried item 1 and 10, composed timestamps:** not repeated. Every `UPDATED` in this unit is
  a `date` reading, pasted.
- **Every other item stands as carried.** Nothing in this unit touched the 1400 split, States,
  `first_answer_to_own_cq`, `Digital`, the demodulator vouch, the idle fixture, the ALC margin,
  the two id schemes, the five files or step 6.

####### Where the carried items stand after unit 334

- **Unit 333 item 3 and unit 331-queue item 19, the undeletable files:** now **thirteen**, and
  listed once in section 2. `Unit333ProbeTests.cs` is tracked, not untracked. None was left
  unemptied.
- **Unit 332 item 1, the `Why` hovers:** unchanged. The FT4 and PSK31 sentences are still at
  `AchievementsViewModel.cs` lines 501 to 517, and both are now false. Task 2 named only the one
  sentence it checked.
- **Unit 332 item 4, the license phrase at 1400:** still three lines on the test host. The left
  block is now 232 px, against 253, and the phrase breaks in the same places.
- **Unit 332 item 5 and unit 333 item 3, the status helper:** still refused, now in three
  spellings; see item 3 raised above. No timestamp was composed.
- **Every other item stands as carried.** Nothing in this unit touched the radio side, the
  achievements rulings, States, the 1400 decoded-list split, the demodulator, the ALC margin, the
  id schemes or PSK31 step 6.

