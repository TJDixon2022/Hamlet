@echo off
setlocal
rem ============================================================
rem  run-fixture.bat  -  prove --seed runs the shipped instruction
rem
rem      run-fixture.bat <seed | noseed>
rem
rem      0  iteration 1 did what it must
rem      1  it did not - the verdict line says what differed
rem      2  usage
rem
rem  Runs the real loop - run-phase.bat - for one iteration against a
rem  fresh git root under %TEMP%, with every claude call answered by the
rem  stand-in in this folder, first on PATH. No model is called. The
rem  second iteration stops on the --max-iterations 1 backstop, so each
rem  arm takes about a minute and a half: the watchdog's first look is at
rem  sixty seconds.
rem
rem    seed    --seed. Iteration 1 MUST run the unit and MUST NOT call the
rem            arbiter, and the outcome entry must carry the shipped
rem            instruction's APPROACH.
rem    noseed  no flag. Iteration 1 MUST call the arbiter before the unit.
rem            This is the control: it shows the stand-in can see an
rem            arbiter call, so a seed arm with none is a real absence.
rem
rem  NOT AGAINST THIS REPOSITORY. The root is under %TEMP%.
rem
rem  Generated 2026-09-13 for: work instructions 063 task 3
rem ============================================================

set "HERE=%~dp0"
set "ARM=%~1"
set "FLAG="
set "OK="
if /i "%ARM%"=="seed" set "OK=1"
if /i "%ARM%"=="seed" set "FLAG=--seed"
if /i "%ARM%"=="noseed" set "OK=1"
if not defined OK goto :usage
for %%I in ("%HERE%..\..") do set "ARB=%%~fI\"

set "FROOT=%TEMP%\cps-seed-fixture-%ARM%"
if exist "%FROOT%" rd /s /q "%FROOT%"
mkdir "%FROOT%"
git init -q "%FROOT%"
>"%FROOT%\PROJECT_CARD.md" echo PROJECT: seed-fixture-%ARM%
copy /y "%HERE%PHASE_PLAN.md" "%FROOT%\PHASE_PLAN.md" >nul
copy /y "%HERE%WORK_INSTRUCTIONS.md" "%FROOT%\WORK_INSTRUCTIONS.md" >nul
copy /y "%HERE%ARBITER.md" "%FROOT%\ARBITER.md" >nul
rem  reload.bat refuses a root with no CLAUDE.md, and the loop then stops at
rem  stage 2 before --seed is ever decided. Found by this fixture's first run.
copy /y "%HERE%CLAUDE.md" "%FROOT%\CLAUDE.md" >nul

set "PATH=%HERE%;%PATH%"

echo.
echo ============================================================
echo  seed fixture : %ARM%
echo  root         : %FROOT%
echo  flag         : %FLAG%
echo  claude resolves to the stand-in:
where claude
echo ============================================================

call "%ARB%run-phase.bat" "%FROOT%" %FLAG% --max-iterations 1
set "RRC=%ERRORLEVEL%"

echo.
echo ============================================================
echo  RESULT : %ARM%
echo ============================================================
powershell -NoProfile -Command "$cf='%FROOT%\calls.txt'; $c=@(); if(Test-Path -LiteralPath $cf){ $c=@(Get-Content -LiteralPath $cf) }; $arb=@($c | Where-Object { $_ -eq 'ARBITER' }).Count; $unit=@($c | Where-Object { $_ -eq 'UNIT' }).Count; $o='%FROOT%\PHASE_OUTCOME.md'; $appr=''; if(Test-Path -LiteralPath $o){ $m=Select-String -Path $o -Pattern '^APPROACH: ' | Select-Object -Last 1; if($m){ $appr=[string]$m.Line } }; 'run-phase exit : %RRC%  (1 is the backstop at iteration 2, expected)'; 'calls, in order: ' + ($c -join ', '); 'arbiter calls  : ' + $arb; 'unit calls     : ' + $unit; 'outcome entry  : ' + $appr; if('%ARM%' -eq 'seed'){ $ok=($unit -ge 1) -and ($arb -eq 0) -and ($appr -match 'the seed instruction as shipped'); 'must           : the unit ran, the arbiter was never called, the entry carries the shipped APPROACH' } else { $ok=($arb -ge 1) -and ($unit -ge 1) -and ($c.Count -gt 0) -and ($c[0] -eq 'ARBITER'); 'must           : the arbiter was called, and before the unit' }; ''; 'verdict        : ' + $(if($ok){ 'PASS' } else { 'FAIL' }); if($ok){ exit 0 } else { exit 1 }"
set "RC=%ERRORLEVEL%"
goto :end

:usage
echo.
echo   run-fixture.bat ^<seed ^| noseed^>
echo.
set "RC=2"

:end
endlocal & exit /b %RC%
