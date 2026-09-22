@echo off
setlocal
rem ============================================================
rem  run-fixture.bat  -  prove the fate field takes one of three words
rem
rem      run-fixture.bat <clean | status-failed>
rem
rem      0  the append succeeded the way it must
rem      1  it did not - the verdict line says so
rem      2  usage
rem
rem  Runs run-phase.bat --fixture record: steps 4a to 5 of the loop, the
rem  SAME subroutine the loop calls, against a fresh root under %TEMP%.
rem  The state judge is claude.bat in this folder, first on PATH, and
rem  answers with a multi-sentence verdict. No model is called.
rem
rem  BOTH ARMS MUST END WITH the append succeeding, FATE: executed,
rem  STATE_AFTER: blocked, and BOTH of the judge's sentences in
rem  STATE_WHY beside the state - never in the fate.
rem
rem    clean          status-check passes.
rem    status-failed  status-check fails. UNTIL 061 THIS ARM WAS RED:
rem                   run-phase.bat wrote "executed - STATUS-CHECK
rem                   FAILED ..." into the fate, outcome-append refused
rem                   it at exit 5, and nothing was recorded. The
rem                   status verdict must now ride in HIT.
rem
rem  Generated 2026-09-12 for: work instructions 061 task 3
rem ============================================================

set "HERE=%~dp0"
set "ARM=%~1"
set "OK="
if /i "%ARM%"=="clean" set "OK=1"
if /i "%ARM%"=="status-failed" set "OK=1"
if not defined OK goto :usage
for %%I in ("%HERE%..\..") do set "ARB=%%~fI\"

set "FROOT=%TEMP%\cps-fate-fixture-%ARM%"
if exist "%FROOT%" rd /s /q "%FROOT%"
mkdir "%FROOT%\.run-unit"
copy /y "%HERE%PHASE_PLAN.md" "%FROOT%\PHASE_PLAN.md" >nul
copy /y "%HERE%WORK_INSTRUCTIONS.md" "%FROOT%\WORK_INSTRUCTIONS.md" >nul
copy /y "%HERE%..\watchdog\fixture-output.md" "%FROOT%\output.md" >nul
copy /y "%HERE%last-run.json" "%FROOT%\.run-unit\last-run.json" >nul
if /i "%ARM%"=="clean" copy /y "%HERE%PROJECT_STATUS-clean.md" "%FROOT%\PROJECT_STATUS.md" >nul
if /i "%ARM%"=="status-failed" copy /y "%HERE%PROJECT_STATUS-bad.md" "%FROOT%\PROJECT_STATUS.md" >nul

set "PATH=%HERE%;%PATH%"

echo.
echo ============================================================
echo  fate fixture : %ARM%
echo  root         : %FROOT%
echo  the state judge resolves to the stand-in:
where claude
echo ============================================================

call "%ARB%run-phase.bat" "%FROOT%" --fixture record
set "RRC=%ERRORLEVEL%"

echo.
echo ============================================================
echo  RESULT : %ARM%
echo ============================================================
powershell -NoProfile -Command "$f='%FROOT%\PHASE_OUTCOME.md'; if(-not (Test-Path -LiteralPath $f)){ 'no PHASE_OUTCOME.md - nothing was appended'; 'exit    : %RRC% (must be 0)'; 'verdict : FAIL'; exit 1 }; $t=@(Get-Content -LiteralPath $f); $i=-1; for($k=0;$k -lt $t.Count;$k++){ if($t[$k] -match '^## UNIT'){ $i=$k } }; $e=@(); if($i -ge 0){ $e=@($t[$i..($t.Count-1)]) }; 'the entry appended:'; $e | ForEach-Object { '    ' + $_ }; $fate=[string]($e | Where-Object { $_ -like 'FATE:*' } | Select-Object -First 1); $st=[string]($e | Where-Object { $_ -like 'STATE_AFTER:*' } | Select-Object -First 1); $why=[string]($e | Where-Object { $_ -like 'STATE_WHY:*' } | Select-Object -First 1); $hit=[string]($e | Where-Object { $_ -like 'HIT:*' } | Select-Object -First 1); $ok=('%RRC%' -eq '0') -and ($fate.Trim() -eq 'FATE: executed') -and ($st.Trim() -eq 'STATE_AFTER: blocked') -and ($why -match 'own verdict at the radio') -and ($why -match 'No unit can close it'); if('%ARM%' -eq 'status-failed'){ $ok = $ok -and ($hit -match 'STATUS-CHECK FAILED') }; ''; 'exit    : %RRC% (must be 0)'; 'verdict : ' + $(if($ok){ 'PASS' } else { 'FAIL' }); if($ok){ exit 0 } else { exit 1 }"
set "RC=%ERRORLEVEL%"
goto :end

:usage
echo.
echo   run-fixture.bat ^<clean ^| status-failed^>
echo.
set "RC=2"

:end
endlocal & exit /b %RC%
