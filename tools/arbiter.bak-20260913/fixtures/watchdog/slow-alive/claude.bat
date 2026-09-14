@echo off
rem ============================================================
rem  FIXTURE - NOT CLAUDE. A stand-in the launcher runs as if it were
rem  claude -p, found first on PATH by run-fixture.bat.
rem
rem  SLOW BUT ALIVE. Sleeps 9 minutes, does two seconds of CPU, sleeps
rem  9 minutes, does two seconds, and exits with a valid output.md. It
rem  MUST SURVIVE: nine quiet minutes is under the ten, and two seconds
rem  of work resets the count.
rem
rem  THIS IS THE EDGE OF THE RULE, deliberately. At one look a minute a
rem  nine-minute sleep is at most nine quiet looks, and the kill is at
rem  ten. A watchdog whose quiet count did not reset on work - or whose
rem  interval drifted short - fails here first.
rem
rem  Generated 2026-09-12 for: work instructions 061 task 1
rem ============================================================
powershell -NoProfile -Command "Start-Sleep -Seconds 540"
powershell -NoProfile -Command "$sw=[Diagnostics.Stopwatch]::StartNew(); $n=0; while($sw.Elapsed.TotalSeconds -lt 2){ $n++ }"
powershell -NoProfile -Command "Start-Sleep -Seconds 540"
powershell -NoProfile -Command "$sw=[Diagnostics.Stopwatch]::StartNew(); $n=0; while($sw.Elapsed.TotalSeconds -lt 2){ $n++ }"
copy /y "%~dp0..\fixture-output.md" "%CD%\output.md" >nul
echo {"type":"result","subtype":"success","is_error":false,"terminal_reason":"completed","num_turns":1,"permission_denials":[],"result":"fixture slow-alive: two nine-minute sleeps, two seconds of work after each"}
exit /b 0
