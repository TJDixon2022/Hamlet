@echo off
setlocal
rem ============================================================
rem  run-fixture.bat  -  prove the watchdog on a stand-in session
rem
rem      run-fixture.bat <busy-silent | hung | slow-alive>
rem
rem      0  the fixture ended the way it must
rem      1  it did not - the verdict line says what differed
rem      2  usage
rem
rem  NOT AGAINST THIS REPOSITORY, AND NOT AGAINST A REAL UNIT. Each
rem  fixture runs in a fresh git repository under %TEMP%, created here
rem  and emptied on the next run of the same fixture. The session is a
rem  claude.bat in the fixture's own folder, put first on PATH, so
rem  run-unit-watched.bat and run-unit.bat run UNCHANGED and launch it
rem  exactly as they would launch claude -p. No model is called and no
rem  money is spent.
rem
rem  WHAT EACH MUST DO, per work instructions 061 task 1:
rem    busy-silent  25 min of CPU, no file written  -> exit 0, complete
rem    hung         one status write, then no CPU   -> exit 1, killed at
rem                 ten quiet looks, "no CPU time in 10 min", lock free
rem    slow-alive   9 min idle, 2 s CPU, twice      -> exit 0, complete
rem
rem  They take 25, about 11 and about 18 minutes. Run them from a
rem  console you can leave; the verdict is printed at the end and the
rem  fixture root keeps the ledger and the log for reading afterwards.
rem
rem  Generated 2026-09-12 for: work instructions 061 task 1
rem ============================================================

set "HERE=%~dp0"
set "NAME=%~1"
if "%NAME%"=="" goto :usage
if not exist "%HERE%%NAME%\claude.bat" goto :usage
for %%I in ("%HERE%..\..") do set "ARB=%%~fI\"

set "FROOT=%TEMP%\cps-watchdog-fixture-%NAME%"
if exist "%FROOT%" rd /s /q "%FROOT%"
mkdir "%FROOT%"
git init -q "%FROOT%"
>"%FROOT%\PROJECT_CARD.md" echo PROJECT: watchdog-fixture-%NAME%

set "PATH=%HERE%%NAME%;%PATH%"

set "WANTRC=0"
set "WANTLINE=ran unattended"
if /i "%NAME%"=="hung" set "WANTRC=1"
if /i "%NAME%"=="hung" set "WANTLINE=killed by the watchdog: under 100 ms of CPU time in the process tree for 10 min"

echo.
echo ============================================================
echo  watchdog fixture : %NAME%
echo  fixture root     : %FROOT%
echo  must end with    : exit %WANTRC%, ledger line containing "%WANTLINE%"
echo  claude resolves to the stand-in:
where claude
echo ============================================================

set "T0="
for /f "usebackq delims=" %%D in (`powershell -NoProfile -Command "(Get-Date).ToString('yyyy-MM-ddTHH:mm:ss')"`) do set "T0=%%D"

call "%ARB%run-unit-watched.bat" fixture-%NAME% "%FROOT%"
set "RRC=%ERRORLEVEL%"

echo.
echo ============================================================
echo  RESULT : %NAME%
echo ============================================================
powershell -NoProfile -Command "$t0=[datetime]::Parse('%T0%'); $t1=Get-Date; $w=$t1 - $t0; 'started     : ' + $t0.ToString('yyyy-MM-dd HH:mm:ss'); 'ended       : ' + $t1.ToString('yyyy-MM-dd HH:mm:ss'); 'wall time   : ' + [int][math]::Floor($w.TotalMinutes) + ' min ' + $w.Seconds + ' s'; 'exit        : %RRC%   (must be %WANTRC%)'; $lf='%FROOT%\RUN_LEDGER.md'; $l=''; if(Test-Path -LiteralPath $lf){ $l=[string](Get-Content -LiteralPath $lf -Tail 1) }; 'ledger line : ' + $l; $lock=Test-Path -LiteralPath '%FROOT%\SESSION.lock'; 'lock        : ' + $(if($lock){ 'HELD - SESSION.lock is still in the fixture root' } else { 'free' }); $rep=Test-Path -LiteralPath '%FROOT%\output.md'; 'report      : ' + $(if($rep){ 'output.md written' } else { 'no output.md' }); $log='%FROOT%\.run-unit\watched.log'; $mk=(Test-Path -LiteralPath $log) -and ((Get-Content -LiteralPath $log -Raw) -match 'Terminating pid'); 'kill marker : ' + $(if($mk){ 'Terminating pid is in watched.log' } else { 'none in watched.log' }); $ok=('%RRC%' -eq '%WANTRC%') -and $l.Contains('%WANTLINE%') -and (-not $lock); if('%WANTRC%' -eq '1'){ $ok = $ok -and $mk -and (-not $rep) }; ''; 'verdict     : ' + $(if($ok){ 'PASS' } else { 'FAIL' }); if($ok){ exit 0 } else { exit 1 }"
set "RC=%ERRORLEVEL%"
goto :end

:usage
echo.
echo   run-fixture.bat ^<busy-silent ^| hung ^| slow-alive^>
echo.
set "RC=2"

:end
endlocal & exit /b %RC%
