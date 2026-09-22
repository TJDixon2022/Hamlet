PHASE: CW decodes again
PHASE_SET: 2026-09-22
DESCRIPTION: A restore phase. The CW decoder read on the air on 2026-08-25 and reads nothing now; the floors that recorded what it produced have been red since 2026-08-31 and nobody has run them since. The engine's CW code goes back to the last commit that read, adapted so today's app builds; the floors go green and stay green under a guard every unit runs; the inherited reds are repaired or retired with reasons; the August rework goes back in one piece at a time on numbers. Judged by three named tests and, at the end, by Tim at the radio.
STEP: 0 | not started | The break is measured and named - the three floor tests run at HEAD case by case with their numbers, the newest commit on main where all three were green is named, and the first commit after it that turned one red is named.
STEP: 1 | not started | The decoder reads again - src\Hamlet.RadioEngine\Cw restored to the named commit and adapted only where today's app or tests would not build; the solution builds; the three floor tests green; the app's CW tests green; the carry-forward list green; nothing under src\Hamlet.App changed without a listed reason.
STEP: 2 | not started | CW cannot break silently again - a CW read guard on the engine carry-forward line beside FT8, FT4, PSK31 and Olivia, watched red against a broken decoder before it was trusted, and measured to fit the line's timeout.
STEP: 3 | not started | The inherited reds are gone - every name in docs\unit239-failing-set.txt and the known-reds block is green, repaired, or retired under R49 with its reason in docs\cw-retired-tests.txt; the known-reds block of the carry-forward list names no CW test.
STEP: 4 | not started | The August rework is judged on numbers - each piece of the 2026-08-28 to 08-31 rework re-applied in its own commit and kept only if the three floor tests stay green and a named number moves; a piece that moves nothing goes back out and the report says so.
STEP: 5 | not started | Tim at the radio - CW on 40 m, text on the CW tab that reads as what was sent, and he says it read.

## UNIT 391 - STEP 0

STEP: 0
APPROACH: run the three CW floor tests at HEAD case by case with their numbers, then walk main backward in a detached worktree to the newest commit where all three are green and name the commit after it that turned one red
MOVE: continue
WHY: PHASE_PLAN.md step 0 criterion 0.2 asks for the newest commit on main at which all three floor tests are green, and criterion 0.1 for every case at HEAD with its measured characters and elements beside its floor
DECIDED: the worktree path C:/Source/HamLet-wt391 and the 900 s timeout per type are the author's, overrulable
LICENCE: PHASE_PLAN.md R47, R48, section 6; HM-DEC-155; HM-DEC-139; HM-DEC-165; FACT-004
COST: one session, four tasks (0 to 3), committed per task; drop from the back.
ACCOMPLISHED: written in output.md at the end of the unit and not claimed here at task 0.
ENTRY: Version 1.13.77 to 1.13.78. PROJECT_CARD.md PHASE to CW decodes again and PHASE_SET to 2026-09-22 under HM-DEC-167, recorded in DECISIONS.md and CLAUDE.md section 1. PHASE_STATUS.md already named unit 391. No file under src changed. Entry round, both command lines as docs/carry-forward-tests.txt prints them at lines 7 and 9, unedited, one build each, a status line immediately before each: APP 278 of 278 in 2 m 46 s and ENGINE 150 of 150 in 5 m, both green on the first attempt.
TASK 1, THE FLOORS AT HEAD (0.1): 20 of 52 cases red - captures 18 of 37 (19 green), adjudicated 0 of 13, clean synthetics 2 of 2 reading nothing. The captures type needed 1995 s; the first run under the instruction's 900 s was cut off at 18 cases and re-run whole under 2700 s. Table in docs/phase-cw/unit391-floors-head.md.
TASK 2, THE SEAMS (0.4): 30 files, 9 under src/Hamlet.App and 21 under tests/Hamlet.App.Tests, 145 file-and-type rows with members by grep, in docs/phase-cw/unit391-seams.md.
TASK 3, THE WALK (0.2, 0.3): no commit back to 2026-08-24 is green on all three - the clean synthetics are red at all 51. Walked on per section 4 to the 85th Cw commit: 0.2 is 07f0397a (2026-08-21), green on all three as it had them (adjudicated not yet written); 0.3 is 8e3ee277, its child, the likelihood decoder, which turned both clean synthetics red. 84 Cw commits between 07f0397a and HEAD. Probe: 7e209cb4 (the 08-25 floors) is 36 of 36 on captures and 13 of 13 adjudicated. Seams checked at 07f0397a: 12 file-and-type rows name a type absent there. Worktree removed. docs/phase-cw/unit391-walk.md.
FATE: executed, complete at task 3 of 4 (tasks 0 to 3, none dropped); step 0's four criteria each have their answer in the report.
