PHASE: Hamlet holds what it has
PHASE_SET: 2026-09-20
DESCRIPTION: A hardening phase, run unattended while Tim is away. Everything banked in the PSK31 and Olivia threads that is screen, record or test and needs neither the radio nor the owner. Judged by tests that ran and, at the end, by Tim at his window.
STEP: 0 | done | Settings survive an upgrade, and a send that cannot go names the true fault - no device chosen is its own refusal with a Settings link; a device that will not open carries its name, rate and OS error. 0.2 to 0.5 met and measured; 0.1's search is complete and its answer is a NEGATIVE - no commit in that window touched the loader, and the line that dropped the device is named in the report and predates 1.13.30.
STEP: 1 | not started | Hamlet opens whole - at the size it opens at and every size measured by unit 354, CQ, the mode tabs, the send area and Stop are on the window; the panels give up height before the send area ever does.
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
