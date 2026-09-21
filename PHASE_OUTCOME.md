PHASE: Hamlet holds what it has
PHASE_SET: 2026-09-20
DESCRIPTION: A hardening phase, run unattended while Tim is away. Everything banked in the PSK31 and Olivia threads that is screen, record or test and needs neither the radio nor the owner. Judged by tests that ran and, at the end, by Tim at his window.
STEP: 0 | partial | Settings survive an upgrade, and a send that cannot go names the true fault - no device chosen is its own refusal with a Settings link; a device that will not open carries its name, rate and OS error. 0.2 to 0.5 met and measured; 0.1's search is complete and its answer is a NEGATIVE - no commit in that window touched the loader, and the line that dropped the device is named in the report and predates 1.13.30.
STEP: 1 | partial | Hamlet opens whole - at the size it opens at and every size measured by unit 354, CQ, the mode tabs, the send area and Stop are on the window; the panels give up height before the send area ever does.
STEP: 2 | not started | The record says what was true - the send press records the mode it was pressed under; every RSID code fldigi knows is in the data file with its tone sequence; the two headless flakes are made deterministic or named as environment and quarantined off the carry-forward list.
STEP: 3 | not started | The record says what was on screen - for every decoded row and every card, whether it was drawn, filtered, scrolled away or folded, so an empty-looking screen can be diagnosed from the file without a screenshot.
STEP: 4 | not started | The radio sheet - one page Tim reads at the radio for PSK31 and Olivia: what to press, what he should see at each step, what each refusal sentence means, where the capture goes, and what to send back if it fails.
STEP: 5 | not started | Tim looks - at his window and, when he has time, at the radio, and says it passed.

## UNIT 369 - STEP 0

STEP: 0
APPROACH: find the commit that dropped the transmit device on load, make the loader keep every value an old file carries, and split the refusal into no-device-chosen with a Settings link and device-would-not-open with the name, rate and OS error
MOVE: continue
WHY: the transmit device setting was lost on upgrade and FT8 could not send for five days; the refusal named the wrong fault
DECIDED: nothing beyond the fault. Task 2's Settings link is the drop candidate; tasks 0 and 1 have none.
LICENCE: CLAUDE.md 0.0, 0.2; PSK31 plan R11, R12, R13, R14, R19; PHASE_PLAN.md R33; HM-DEC-155, HM-DEC-139, HM-DEC-165
COST: one session, three tasks (0 to 2), each committed on its own.
ACCOMPLISHED: written in output.md at the end of the unit and not claimed here at task 0.
ENTRY: PHASE_STATUS.md line 1 names this phase. Carry-forward before any change: app 190 of 190, engine 146 of 146, with TheSendReachesTheAirTests among them and green.
FOUND AT TASK 0: criterion 0.1 is a negative, and the evidence is in the tree. No commit between 1.13.30 (681d45c8) and 1.13.48 (ec4b466e) touched the settings model, its loader, its migrations or the transmit-device picker - 119 commits in that window and not one of them names a file under src/Hamlet.App/Settings/. git diff over src/Hamlet.App/Settings/, ViewModels/SettingsViewModel.cs, Views/SettingsWindow.axaml and App.axaml.cs from 681d45c8 to HEAD is empty. The loader did not drop the value, so the drop is not in the load path.

## UNIT 1 - STEP 0

STEP: 0
APPROACH: find the commit that dropped the transmit device on load, make the loader keep every value an old file carries, and split the refusal into no-device-chosen with a Settings link and device-would-not-open with the name, rate and OS error
HIT: section 4 asked nothing inside the three stops - author's, overrulable, the loop continued - Every item asks about investigation status, a criterion's bookkeeping, a time display's wording, a checkbox count, and fixture provenance, none of which decides what the product can make happen outside the machine, spends past budget, or changes a fact the product states to the operator, and both device faults were already repaired. - STATUS-CHECK FAILED - a field this unit wrote is wrong, see the run output
MOVE: continue
WHY: the transmit device setting was lost on upgrade and FT8 could not send for five days; the refusal named the wrong fault
DECIDED: nothing beyond the fault
LICENCE: CLAUDE.md 0.0, 0.2; PSK31 plan R11, R12, R13, R14, R19
COST: 19.234482500000006
ACCOMPLISHED: an upgrade never again forgets what Tim chose, and a send that cannot go says the true reason
FATE: executed
STATE_AFTER: partial
STATE_WHY: Criteria 0.2 to 0.5 are met with quoted sentences, counts and green unedited suites, but 0.1 as written requires naming a commit between 1.13.30 and 1.13.48 and the report names none, offering instead a line that it measures as predating that window, and the open question about whether a negative satisfies 0.1 is a plan wording matter the unit was to rule on itself, not a stop.

## UNIT 371 - CARRIED REPAIR (THE MAN WHO ANSWERED THE CQ)

STEP: 0 (paused)
ADVANCED: blocker
APPROACH: open the station conversation card on any parsed line addressed to the operator with a hand-back, marked as a guess where the parser is not certain, retire the receipt, offer Report with the doubt word and send on the click, and never let the CQ filter hide a row that spoke to the operator
MOVE: continue
WHY: the owner record of 2026-09-20 shows a station answering his PSK31 CQ twice, read and identified as addressed to him, and no card appeared because the parser was not certain
DECIDED: the display side of R1 covers a card that appears and a button that waits for a click; the doubt word and the question mark are the author choices
LICENCE: PSK31 plan R1 read as strict-on-sends only; Tim 2026-09-14 on the typed line; card rulings R1 to R6; CLAUDE.md 0.0, 0.2
COST: one session, three tasks (0 to 2), each committed on its own.
ACCOMPLISHED: written in output.md at the end of the unit and not claimed here at task 0.
ENTRY: carry-forward before any change: engine 146 of 146, app 206 of 206.

## UNIT 372 - STEP 1

STEP: 1
ADVANCED: step 1
APPROACH: shrink the window below the minimums and assert by measuring that the working panels give up height first, then the top row, and the panels scroll inside themselves while the send area stays put
MOVE: continue
WHY: step 1 has zero units spent and its rule is stated only in an axaml comment - unit 356 capped the top row and asserted the nine sizes, so 1.1 is largely banked, but nothing measures the order height is surrendered in and nothing measures the window below 900x620 at all.
DECIDED: author's, overrulable, two, both transcribed in work instruction 372 section 6. (1) Criterion 0.1 is met by unit 369's completed negative - 119 commits searched, an empty diff over every settings path, the mechanism named and repaired - so step 0 is done and step 1's entry is open. (2) R9 stands: the CQ filter keeps showing a PSK31 row addressed to another station, because a PSK31 row has no addressee until a turnover has been read. Also settled as the author's: 1.4's "the sheet's layout tests" reads as TheTopRowTests, TheWorkingPanelsTests and TheStopIsAlwaysOnScreenTests.
LICENCE: PHASE_PLAN.md R34, R31 and section 6 - a layout number is the arbiter's to decide and never a stop; PSK31 plan R11, R12, R13, R14, R19; CLAUDE.md 0.0, 0.2, 0.5, 0.6; HM-DEC-051, HM-DEC-139, HM-DEC-155, HM-DEC-165; FACT-004
COST: one session, five tasks (0 to 4), each committed on its own.
ACCOMPLISHED: written in output.md at the end of the unit and not claimed here at task 0.
ENTRY: PHASE_STATUS.md line 1 names this phase. Carry-forward before any change, both invocations, one build each: app 206 of 206, engine 146 of 146. Both green - no red to name at this task.
EXIT: carry-forward after the last change, both invocations, one build each: app 206 of 206, engine 146 of 146. Name for name identical to the entry run - no regression. Nothing was added to docs/carry-forward-tests.txt.
FATE: executed, complete at task 4 of 5 (tasks 0 to 4).
STATE_AFTER: partial
STATE_WHY: 1.1 met and measured at all nine sizes - CQ, the mode tabs, the reserved send area, the drive note and Stop are whole on the window at every one of them, and it was already in the tree from unit 356. 1.2 met - TheWindowGivesUpHeightInOneOrderTests, four names, 4 of 4, asserting for the first time that the send area is 22 px at every height from 780 to 620, that the working panels give up all of it while TopRow gives up none, and that TopRow never passes its 300 px cap; no source file needed changing. 1.3 PARTIAL, measured - all three working panels draw 0 px at 900 x 620 and at every height from 700 down, with their content still there and unreachable (the decoded scroller reads extent 36 in a 0 viewport, For You extent 360 in a 0 viewport), which is hiding information rather than detail under CLAUDE.md 0.5; TheWindowHoldsBelowItsMinimumTests is committed red on that one name and was not loosened. 1.4 NOT MET on an inherited red - BindingHealthTests 1 of 1, TheWorkingPanelsTests 8 of 8 and TheStopIsAlwaysOnScreenTests 5 of 5, but TheTopRowTests 14 of 15 at TheBestBetPillAndTheGreenBlockNameTheSameBandOnTheWindow, which fails alone, is unrelated to height, and is byte-identical to unit 371's tip along with all of src.
MEASURED: MinHeight binds on a headless window, so 1100 x 580, 540 and 500 are one size - 1100 x 620 - reached three ways. At 1100 x 620 the workspace region gets 51 px and needs 138 before the panels' first pixel; the deficit is 87. At width 1100 the panel row's height is window height minus 707. The only pool above the panels is TopRow's 300 px, of which the radio's own face is 110.

## UNIT 1 - STEP 1

STEP: 1
APPROACH: shrink the window below the minimums and assert by measuring that the working panels give up height first, then the top row, and the panels scroll inside themselves while the send area stays put
HIT: section 4 asked nothing inside the three stops - author's, overrulable, the loop continued - The three explicit ruling requests concern a layout height mechanism, whether plan checkboxes are the record, whether to chase which refusal the operator saw, and a time phrasing's units, all of which fall outside keying or transmit reach, money past budget, and what the product asserts as true, and the rest are findings or notes the unit has already acted on.
MOVE: continue
WHY: step 1 has zero units spent and its rule is stated only in an axaml comment - unit 356 capped the top row and asserted the nine sizes, so 1.1 is largely banked, but nothing measures the order height is surrendered in and nothing measures the window below 900x620 at all.
DECIDED: author's, overrulable, two. (1) Criterion 0.1 is met by unit 369's completed negative - 119 commits searched, an empty diff over every settings path, the mechanism named and repaired - so step 0 is done and step 1's entry is open; the criterion's wording presupposed a commit the measurement shows does not exist, which is a wording fault and not an unmet criterion. This answers unit 369's section 4 item 1. (2) R9 stands: the CQ filter keeps showing a PSK31 row addressed to another station, because a PSK31 row has no addressee until a turnover has been read and hiding by latest parse reopens unit 337's fault. This answers unit 371's section 4 item 1 and is logged, not chased. Also settled as the author's: 1.4's 'the sheet's layout tests' reads as TheTopRowTests, TheWorkingPanelsTests and TheStopIsAlwaysOnScreenTests.
LICENCE: PHASE_PLAN.md R34, R31 and section 6 - a layout number is the arbiter's to decide and never a stop; PSK31 plan R11, R12, R13, R14, R19; CLAUDE.md 0.0, 0.2, 0.5, 0.6; HM-DEC-051, HM-DEC-139, HM-DEC-155, HM-DEC-165; FACT-004
COST: 16.45659
ACCOMPLISHED: Hamlet stays whole when the window is made small - the panels shrink and then scroll inside themselves, the top row gives up its share next, and the button that calls CQ and the area around it are still on the screen at every size Tim can drag the window to, including sizes smaller than Hamlet will open at.
FATE: executed
STATE_AFTER: partial
STATE_WHY: Criteria 1.1 and 1.2 are met with measured tables and green tests, while 1.3 is measured red with the panels drawing 0 px and 1.4 fails on an inherited red, and the open question is a layout allocation and a number, which is not one of the three things the phase stops for.

## UNIT 373 - STEP 1

STEP: 1
ADVANCED: step 1
APPROACH: give the panel canvas inside WorkspaceBoundary a measured minimum height and a scroller of its own, so the three working panels keep a viewport and scroll inside themselves at 620 while the send area stays put in its sibling row, and take the inherited best-bet red off 1.4
MOVE: work around
WHY: unit 372 measured 1.3 and declined the repair, having rejected a scroller on root row 1 because it would enclose the send area - but the send area is a sibling of the panel canvas one grid lower, at Grid.Row 4 row 0, so a floor and a scroller on WorkspaceBoundary at line 3728 reach the criterion without touching the send area or TopRow's cap, and that site was never measured.
DECIDED: author's, overrulable, two, both transcribed in work instruction 373 section 6. (1) Criterion 1.3's repair is a measured minimum height and a scroller on the panel canvas inside WorkspaceBoundary, never on the tab row that carries the send area and never funded from TopRow's 300 px cap; the canvas scrolling at 1280x720 and 1366x728 is the accepted cost, since a 50 px panel is not a working panel and HM-DEC-051 is the ruling that everything but the header and the status bar scrolls. This answers unit 372's section 4 item 1. (2) The inherited red TheTopRowTests.TheBestBetPillAndTheGreenBlockNameTheSameBandOnTheWindow is this unit's to repair, because repair-nothing-but-your-own does not exempt a must-pass criterion of the step the unit is on and no other unit is coming for it. This answers unit 372's section 4 item 2. Unit 372's item 7, the plan checkboxes, is left on the carried queue unanswered because R31 allows two rulings a unit and both are spent on must-pass criteria.
LICENCE: PHASE_PLAN.md R34, R31 and section 6 - a layout, a number and a mechanism are the arbiter's and never a stop; PSK31 plan R11, R12, R13, R14, R19; CLAUDE.md 0.0, 0.2, 0.5, 0.6; HM-DEC-046, HM-DEC-051, HM-DEC-139, HM-DEC-155, HM-DEC-165; FACT-004
COST: one session, five tasks (0 to 4), each committed on its own.
ACCOMPLISHED: written in output.md at the end of the unit and not claimed here at task 0.
ENTRY: PHASE_STATUS.md line 1 names this phase. Carry-forward before any change, both invocations, one build each: app 206 of 206, engine 146 of 146. Both green, and name for name identical to unit 372's entry and exit runs - no red to name at this task and no finding.
