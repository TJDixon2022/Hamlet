@echo off
rem ============================================================
rem  FIXTURE - NOT CLAUDE. Stands in for every claude -p call
rem  run-phase.bat makes on the seed fixture, found first on PATH by
rem  run-fixture.bat, and records in calls.txt which kind it was:
rem
rem    ARBITER  the arbiter's prompt opens "Read ARBITER.md". Writes
rem             nothing, so WORK_INSTRUCTIONS.md stays the one shipped.
rem    UNIT     run-unit.bat's prompt opens with the PROJECT: gate line.
rem             Writes a valid output.md.
rem    JUDGE    a judge passes its prompt on stdin, so no prompt argument
rem             follows -p. Reads stdin to its end, answers a state.
rem
rem  The prompt arguments carry line breaks, and cmd keeps only the first
rem  line of a command line, so the first line is what is recorded and
rem  what the kind is told from. That is enough: the three first lines
rem  differ.
rem
rem  Generated 2026-09-13 for: work instructions 063 task 3
rem ============================================================
set "ARGS=%CD%\.seed-args.txt"
>"%ARGS%" echo %*
findstr /c:"Read ARBITER" "%ARGS%" >nul
if not errorlevel 1 goto :arbiter
findstr /c:"PROJECT:" "%ARGS%" >nul
if not errorlevel 1 goto :unit
goto :judge

:arbiter
>>"%CD%\calls.txt" echo ARBITER
echo {"type":"result","subtype":"success","is_error":false,"terminal_reason":"completed","num_turns":1,"permission_denials":[],"result":"fixture arbiter - wrote nothing"}
exit /b 0

:unit
>>"%CD%\calls.txt" echo UNIT
copy /y "%~dp0..\watchdog\fixture-output.md" "%CD%\output.md" >nul
echo {"type":"result","subtype":"success","is_error":false,"terminal_reason":"completed","num_turns":1,"permission_denials":[],"result":"fixture unit - ran the instruction in the tree"}
exit /b 0

:judge
findstr "^" >nul
>>"%CD%\calls.txt" echo JUDGE
echo {"type":"result","subtype":"success","is_error":false,"terminal_reason":"completed","num_turns":1,"permission_denials":[],"result":"STATE: in progress\nWHY: fixture judge - the seed fixture proves which instruction iteration 1 ran, not the step."}
exit /b 0
