@echo off
rem ============================================================
rem  FIXTURE - NOT CLAUDE. A stand-in the launcher runs as if it were
rem  claude -p, found first on PATH by run-fixture.bat.
rem
rem  HUNG. Writes PROJECT_STATUS.md once, then sleeps for 15 minutes
rem  accruing no CPU, then would write a report. It MUST BE KILLED at
rem  ten minutes of no CPU, with the new ledger sentence and the lock
rem  released - and it must never reach the report.
rem
rem  THE SLEEP IS Start-Sleep, AND THAT WAS MEASURED. Unit 061 sampled
rem  four sleepers for 150 seconds each: PowerShell Start-Sleep, a
rem  Thread.Sleep, waitfor and cmd running PowerShell all accrued 0.0
rem  ms; ping accrued 218.8 ms. A hung fixture that ticks is a fixture
rem  that cannot be killed by a CPU rule, and would prove nothing.
rem
rem  THE STATUS WRITE IS HERE SO THE FIXTURE IS NOT SILENT. It is the
rem  case the old rule got right, and it proves the new rule does not
rem  need the status file to reach the same verdict.
rem
rem  Generated 2026-09-12 for: work instructions 061 task 1
rem ============================================================
set "NOW="
for /f "usebackq delims=" %%D in (`powershell -NoProfile -Command "(Get-Date).ToString('yyyy-MM-dd HH:mm')"`) do set "NOW=%%D"
>"%CD%\PROJECT_STATUS.md" echo PROTOCOL: 2
>>"%CD%\PROJECT_STATUS.md" echo PROJECT: watchdog-fixture-hung
>>"%CD%\PROJECT_STATUS.md" echo STATE: EXECUTING
>>"%CD%\PROJECT_STATUS.md" echo TASK: 1 of 1
>>"%CD%\PROJECT_STATUS.md" echo BALL: code
>>"%CD%\PROJECT_STATUS.md" echo NEXT_PASTE: none
>>"%CD%\PROJECT_STATUS.md" echo UPDATED: %NOW%
>>"%CD%\PROJECT_STATUS.md" echo NOTE: fixture - written once, then the session hangs
powershell -NoProfile -Command "Start-Sleep -Seconds 900"
copy /y "%~dp0..\fixture-output.md" "%CD%\output.md" >nul
echo {"type":"result","subtype":"success","is_error":false,"terminal_reason":"completed","num_turns":1,"permission_denials":[],"result":"fixture hung: THIS LINE MEANS THE WATCHDOG DID NOT FIRE"}
exit /b 0
