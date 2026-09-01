@echo off
rem ============================================================
rem  run-unit-watched.bat  -  a run you can walk away from
rem
rem      run-unit-watched.bat <unit> <root> [--minutes N]
rem                          [--poll SECONDS] [--tools-file F]
rem
rem      0  the run finished on its own and run-unit.bat said 0
rem      1  THE WATCHDOG KILLED IT - the run stalled
rem      2  usage, or bad root
rem      other  whatever run-unit.bat exited with, passed through
rem
rem  Launches run-unit.bat in the background, polls watchdog.bat on
rem  an interval with --since set to the launch clock, and
rem  TERMINATES THE RUN BY PID if the watchdog returns stale.
rem
rem  ---------------------------------------------------------------
rem  BY PID, NEVER BY NAME. taskkill /IM claude.exe would take the
rem  owner's own interactive sessions with it - unit 039's listing
rem  showed five running on this machine at the time. The tree it is
rem  killing is a fixture; the sessions it must not touch are the
rem  owner's work. So this tracks the process it started and kills
rem  that one, with its children, and nothing else.
rem
rem  EXIT 2 FROM THE WATCHDOG IS NOT A KILL. Unreadable is not
rem  stalled. The panel's prime directive is that a reading which is
rem  absent, unparseable or refused is shown as unknown - never as
rem  healthy and never as failed - and a kill holds the same line. A
rem  2 is reported and polling continues: terminating a process on a
rem  reading you could not take is the worst thing in this file.
rem
rem  --since IS WHY THIS CAN EXIST AT ALL. Unit 039 measured that a
rem  watchdog polling a freshly launched run reads the PREVIOUS
rem  unit's UPDATED and returns stale at t=0, which on a queue kills
rem  every run at second zero. 039 refused to build the kill on that
rem  reading. Unit 040 task 2 gave watchdog.bat --since, so the
rem  watchdog now judges only writes after the launch clock, and a
rem  run that has not had time to report reads FRESH.
rem
rem  THE LOCK IS RELEASED ON EVERY PATH OUT, INCLUDING THE KILL.
rem  run-unit.bat takes and releases its own lock; where this script
rem  kills that process the release never runs, so this script
rem  releases it instead. A lock left behind after a kill blocks the
rem  queue this whole phase exists to enable - which would make the
rem  kill worse than no kill.
rem
rem  A KILLED RUN GETS ITS OWN LEDGER LINE, naming the watchdog. A
rem  run that was killed and a run that failed are different facts
rem  and the ledger is what the owner reads instead of watching.
rem
rem  THE DEFAULTS, and why. --minutes 12, matching the panel's
rem  CFG.staleRunMin and watchdog.bat's own default, so the panel,
rem  the watchdog and the kill cannot disagree about one file.
rem  CLAUDE_CODE.md section 7 makes the write rule every ten and the
rem  panel adds two minutes of grace. --poll 30 seconds: fast enough
rem  that a stall is caught within a twenty-fifth of the threshold,
rem  slow enough that a 90-second run costs three polls rather than
rem  ninety. Neither number is measured against a long run, because
rem  no long run has been watched yet - they are starting points and
rem  this line says so.
rem
rem  Batch, not PowerShell: a .ps1 will not run on this machine.
rem  The inline powershell -NoProfile -Command calls are not script
rem  files, and are used where cmd cannot start a process and keep
rem  its pid, sleep, or read a clock.
rem
rem  Generated 2026-08-28 for: work instructions 040 task 4
rem ============================================================

setlocal

set "HERE=%~dp0"
set "RC=0"
set "UNIT=%~1"
set "ROOT=%~2"
set "MINUTES=12"
set "POLL=30"
set "TOOLSARG="

if "%UNIT%"=="" goto :usage
if "%ROOT%"=="" goto :usage
shift
shift

:parse
if "%~1"=="" goto :parsed
if /i "%~1"=="--minutes"    set "MINUTES=%~2" & shift & shift & goto :parse
if /i "%~1"=="--poll"       set "POLL=%~2" & shift & shift & goto :parse
if /i "%~1"=="--tools-file" set "TOOLSARG=--tools-file "%~2"" & shift & shift & goto :parse
echo ERROR: unexpected argument: %~1
goto :usage

:parsed
if "%ROOT:~-1%"=="\" set "ROOT=%ROOT:~0,-1%"
if not exist "%ROOT%\" (
  echo ERROR: repo root not found: %ROOT%
  set "RC=2"
  goto :end
)

set "WORK=%ROOT%\.run-unit"
if not exist "%WORK%" mkdir "%WORK%"
set "PIDFILE=%WORK%\watched.pid"
set "RCFILE=%WORK%\watched.rc"
set "LOGFILE=%WORK%\watched.log"
set "RUNNER=%WORK%\watched-runner.bat"

rem --- the launch clock, read from the system, never composed ----
set "SINCE="
for /f "usebackq delims=" %%D in (`powershell -NoProfile -Command "(Get-Date).ToString('yyyy-MM-ddTHH:mm:sszzz')"`) do set "SINCE=%%D"
set "STARTED="
for /f "usebackq delims=" %%D in (`powershell -NoProfile -Command "(Get-Date).ToString('yyyy-MM-ddTHH:mm')"`) do set "STARTED=%%D"

echo.
echo ============================================================
echo  run-unit-watched
echo    unit      : %UNIT%
echo    root      : %ROOT%
echo    since     : %SINCE%
echo    threshold : %MINUTES% min
echo    poll      : every %POLL%s
echo ============================================================
echo.

if exist "%PIDFILE%" del /q "%PIDFILE%"
if exist "%RCFILE%" del /q "%RCFILE%"

rem --- the background runner, WRITTEN TO A FILE and then executed -
rem  CPS-DEC-021. The first cut built this as a cmd /c command line
rem  inside a PowerShell -ArgumentList, three layers of quoting deep,
rem  with a redirect and an ampersand in it. It launched NOTHING -
rem  no log, no exit code, no work done - and the poll loop then
rem  reported "the run finished on its own" because the pid it had
rem  captured was already gone. A launcher that silently launches
rem  nothing and calls it finished is worse than one that fails.
rem  So the command goes into a file, and the file is executed.
>"%RUNNER%" echo @echo off
>>"%RUNNER%" echo call "%HERE%run-unit.bat" %UNIT% "%ROOT%" %TOOLSARG%
rem  PARENTHESISED, and this is not style. `echo %ERRORLEVEL%> f`
rem  with an exit code of 0 reads as `echo 0> f` - a DIGIT before a
rem  redirect is a FILE HANDLE, so cmd redirected stdin and wrote an
rem  empty file. The watcher then read no code, called it unknown and
rem  returned 2 for a run that had exited 0. Measured 2026-08-28.
>>"%RUNNER%" echo ^(echo %%ERRORLEVEL%%^)^> "%RCFILE%"

echo Launching run-unit.bat in the background...
powershell -NoProfile -Command "$p = Start-Process -FilePath '%RUNNER%' -RedirectStandardOutput '%LOGFILE%' -PassThru -WindowStyle Hidden; $p.Id | Set-Content -LiteralPath '%PIDFILE%'"

set "CHILD="
if exist "%PIDFILE%" for /f "usebackq delims=" %%P in ("%PIDFILE%") do set "CHILD=%%P"
if not defined CHILD (
  echo ERROR: the run did not start - no pid was captured.
  set "RC=2"
  goto :end
)
echo   pid       : %CHILD%
echo   log       : %LOGFILE%
echo.

rem ============================================================
rem  THE POLL LOOP
rem ============================================================
:poll
powershell -NoProfile -Command "Start-Sleep -Seconds %POLL%"

rem  has it finished on its own?
set "ALIVE="
for /f "usebackq tokens=1,2 delims=," %%A in (`tasklist /NH /FO CSV /FI "PID eq %CHILD%" 2^>nul`) do if "%%~B"=="%CHILD%" set "ALIVE=1"
if not defined ALIVE goto :finished

rem  still running - ask the watchdog
call "%HERE%watchdog.bat" "%ROOT%" --minutes %MINUTES% --since "%SINCE%" >nul 2>&1
set "WD=%ERRORLEVEL%"

if "%WD%"=="0" (
  echo   [poll] watchdog 0 - fresh, still running. Leaving it alone.
  goto :poll
)
if "%WD%"=="2" (
  rem  UNREADABLE IS NOT STALLED. Report and keep polling: a kill
  rem  on a reading that was not taken is the one thing this file
  rem  must never do.
  echo   [poll] watchdog 2 - UNKNOWN, not a kill. The status file could
  echo          not be read, which is not evidence the run has stalled.
  echo          Reporting and continuing to poll.
  goto :poll
)

rem --- WD is 1: stale. Kill it. ---------------------------------
echo.
echo   [poll] watchdog 1 - STALE. Killing the run.
call "%HERE%watchdog.bat" "%ROOT%" --minutes %MINUTES% --since "%SINCE%"
echo.
echo   Terminating pid %CHILD% and its children, BY PID.
taskkill /PID %CHILD% /T /F
echo.

set "ENDED="
for /f "usebackq delims=" %%D in (`powershell -NoProfile -Command "(Get-Date).ToString('yyyy-MM-ddTHH:mm')"`) do set "ENDED=%%D"

rem  run-unit.bat's own release never ran, so release its lock here
echo   Releasing the lock the killed run was holding...
call "%HERE%lock.bat" release "%ROOT%"

rem  A KILLED RUN'S COST IS unknown, not 0. The JSON is never
rem  written because the process was terminated before claude
rem  could emit it, so the money spent is real and unmeasured -
rem  and a 0 there would be a claim that the run was free.
call "%HERE%ledger.bat" "%UNIT%" "%STARTED%" "%ENDED%" "killed" "killed by the watchdog: no status write within %MINUTES% min of the launch clock" "unknown" "%ROOT%" >nul
echo   Ledger line written, naming the watchdog.
set "RC=1"
goto :end

rem ============================================================
:finished
set "URC="
if exist "%RCFILE%" for /f "usebackq tokens=* delims=" %%R in ("%RCFILE%") do set "URC=%%R"
if not defined URC set "URC=unknown"
echo.
echo   The run finished on its own.
echo   run-unit.bat exit : %URC%
echo   its output is in  : %LOGFILE%
echo.
if "%URC%"=="unknown" (
  set "RC=2"
  goto :end
)
set "RC=%URC%"
goto :end

rem ============================================================
:usage
echo.
echo   run-unit-watched.bat ^<unit^> ^<root^> [--minutes N] [--poll SECONDS]
echo                        [--tools-file F]
echo.
echo   --minutes defaults to 12, matching CFG.staleRunMin
echo   --poll    defaults to 30 seconds
echo.
echo   0 finished and run-unit said 0, 1 KILLED BY THE WATCHDOG,
echo   2 usage or bad root, other = run-unit.bat's own code
echo.
set "RC=2"
goto :end

rem ============================================================
:end
echo.
echo run-unit-watched exit %RC%
endlocal & exit /b %RC%
