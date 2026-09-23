PHASE: CW decodes again
PHASE_SET: 2026-09-22
DESCRIPTION: A restore phase. The CW decoder read on the air on 2026-08-25 and reads nothing now; the floors that recorded what it produced have been red since 2026-08-31 and nobody has run them since. The engine's CW code goes back to the last commit that read, adapted so today's app builds; the floors go green and stay green under a guard every unit runs; the inherited reds are repaired or retired with reasons; the August rework goes back in one piece at a time on numbers. Judged by three named tests and, at the end, by Tim at the radio.
CURRENT_STEP: 3
WORK_INSTRUCTION: 402 - the eight reds, first attempt
HEARTBEAT: 2026-09-23 08:10:11
STEP: 0 | not started | The break is measured and named - the three floor tests run at HEAD case by case with their numbers, the newest commit on main where all three were green is named, and the first commit after it that turned one red is named.
STEP: 1 | done | The decoder reads again - src\Hamlet.RadioEngine\Cw restored to the named commit and adapted only where today's app or tests would not build; the solution builds; the three floor tests green; the app's CW tests green; the carry-forward list green; nothing under src\Hamlet.App changed without a listed reason.
STEP: 2 | not started | CW cannot break silently again - a CW read guard on the engine carry-forward line beside FT8, FT4, PSK31 and Olivia, watched red against a broken decoder before it was trusted, and measured to fit the line's timeout.
STEP: 3 | partial | The inherited reds are gone - every name in docs\unit239-failing-set.txt and the known-reds block is green, repaired, or retired under R49 with its reason in docs\cw-retired-tests.txt; the known-reds block of the carry-forward list names no CW test.
STEP: 4 | partial | The August rework is judged on numbers - each piece of the 2026-08-28 to 08-31 rework re-applied in its own commit and kept only if the three floor tests stay green and a named number moves; a piece that moves nothing goes back out and the report says so.
STEP: 5 | not started | Tim at the radio - CW on 40 m, text on the CW tab that reads as what was sent, and he says it read.
