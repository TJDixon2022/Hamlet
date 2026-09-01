@echo off
rem ============================================================
rem  watchdog.bat  -  is that session still reporting?
rem
rem  Reads PROJECT_STATUS.md, compares UPDATED to the clock, and
rem  reports staleness by exit code, because a loop reads codes
rem  and not prose.
rem
rem      watchdog.bat [root] [--minutes N] [--since ISO]
rem
rem      0  FRESH  - UPDATED is within the threshold, OR the run
rem                  named by --since is younger than the threshold
rem                  and has not had time to write yet
rem      1  STALE  - UPDATED is past the threshold, or the run named
rem                  by --since is past it and has never written
rem      2  UNKNOWN - the file is absent or unreadable, or
rem                   UPDATED is missing or will not parse
rem
rem  --since NAMES THE MOMENT A RUN BEGAN, and the watchdog then
rem  judges only writes after it. Without it, behaviour is exactly
rem  as before.
rem
rem  WHY IT EXISTS. UPDATED at launch belongs to the PREVIOUS unit.
rem  Unit 039 measured the consequence: a unit queued against a root
rem  whose last run ended twenty minutes ago is reported STALE at
rem  t=0, so a watchdog polling a healthy one-second-old run returns
rem  a kill signal. On a queue - which is the whole phase - that
rem  kills every run at second zero. 039 refused to build the kill
rem  on that reading and was right to.
rem
rem  IT CHANGES WHEN THE WATCHDOG LOOKS, NOT WHAT IT LOOKS AT. The
rem  threshold is untouched and staleness still means the same thing
rem  it means on the panel, so the two cannot disagree about one
rem  file. Rejected by the owner: seeding UPDATED at launch, which
rem  puts a told field into a tree the launcher is about to hand to
rem  someone else - CPS-DEC-030. Rejected: watching the file mtime
rem  instead, a different reading from the panel's. Rejected: a
rem  grace period, a second absolute threshold, which is what
rem  CPS-DEC-026 removed after it produced a false STOPPED.
rem
rem  THE THREE CASES --since ADDS:
rem    run younger than the threshold, no write since --since  -> 0
rem      it has not had time. Not stale, and NOT unknown: the
rem      panel's rule is that a reading you could not take is
rem      unknown, and this is a reading that is not due yet.
rem    run older than the threshold, no write since --since     -> 1
rem      THE REAL STALL. The session started and never wrote.
rem    a write after --since                                    -> the
rem      ordinary comparison, unchanged.
rem
rem  2 IS NOT 1, AND THAT IS THE POINT. Cannot read and has not
rem  moved are different facts and only one of them is evidence
rem  about the session. The panel's prime directive is that a
rem  reading which is absent, unparseable or refused is shown as
rem  unknown - never as healthy, never as failed - and a watchdog
rem  that collapsed the two would hand a loop a "kill it" verdict
rem  built on a file it never read. A caller that treats 2 as 1
rem  has thrown away the distinction this exit code exists for.
rem
rem  NO KILL. Terminating a process on this reading is phase 3's
rem  sharpest edge and is deliberately not built here: this script
rem  reports and something else decides. GROKBOT.md section 4
rem  task 11 names a watchdog "with a kill" and that half is not
rem  this unit's.
rem
rem  THE THRESHOLD, and why 12. Default 12 minutes, matching the
rem  panel's CFG.staleRunMin, so the two cannot disagree about the
rem  same file - which is the whole risk of a second reader.
rem  CLAUDE_CODE.md section 7 makes the write rule every ten
rem  minutes and the panel adds two minutes of grace, because a
rem  threshold set at exactly the write interval fires on every
rem  write that is a few seconds late. GROKBOT.md section 3 stop
rem  condition 5 says "stale past ten minutes"; ten is the write
rem  rule rather than the threshold, and that difference is
rem  reported rather than quietly resolved. Override with
rem  --minutes N.
rem
rem  UPDATED IS READ FROM THE FILE. THE CLOCK IS READ FROM THE
rem  SYSTEM at the moment of comparison, never composed -
rem  CLAUDE_CODE.md section 11 records two typed timestamps, one
rem  of them thirty-nine seconds into the future.
rem
rem  IT PRINTS ITS WORKING. The parsed UPDATED, the clock it
rem  compared against, the age in minutes and the threshold. A
rem  watchdog that prints only a verdict cannot be debugged on the
rem  morning it is wrong.
rem
rem  Batch, not PowerShell: a .ps1 will not run on this machine,
rem  unsigned scripts are blocked by execution policy. The inline
rem  powershell -NoProfile -Command calls are not script files and
rem  are used only where cmd genuinely cannot do the job - parsing
rem  a timestamp and differencing two of them.
rem
rem  Generated 2026-08-28 for: work instructions 038 task 3
rem  Amended  2026-08-28 for: work instructions 040 task 2 - --since,
rem           licensed by the owner's ruling of that date and by nothing
rem           else. All six arms 038 demonstrated were re-run afterwards.
rem ============================================================

setlocal

set "RC=0"
set "MINUTES=12"
set "REPO="
set "SINCE="

:parse
if "%~1"=="" goto :parsed
if /i "%~1"=="--minutes" goto :parsemin
if /i "%~1"=="--since" goto :parsesince
if defined REPO goto :badarg
set "REPO=%~1"
shift
goto :parse

:parsemin
if "%~2"=="" echo ERROR: --minutes needs a number. & goto :usage
set "MINUTES=%~2"
shift
shift
goto :parse

:parsesince
if "%~2"=="" echo ERROR: --since needs a timestamp. & goto :usage
set "SINCE=%~2"
shift
shift
goto :parse

:badarg
echo ERROR: unexpected argument: %~1
goto :usage

:parsed
if "%REPO%"=="" set "REPO=C:\Source\HamLet"
if "%REPO:~-1%"=="\" set "REPO=%REPO:~0,-1%"

echo %MINUTES%| findstr /r "^[0-9][0-9]*$" >nul
if errorlevel 1 (
  echo ERROR: --minutes must be a whole number of minutes: %MINUTES%
  goto :usage
)

rem  --since is validated HERE, before the status file is even read.
rem  A launcher that passes an unparseable moment must be told so
rem  loudly rather than have its kill silently fall back to the old
rem  behaviour, which is the one that kills healthy runs.
set "SINCEAGE="
if defined SINCE (
  for /f "usebackq delims=" %%M in (`powershell -NoProfile -Command "try{ $s=[datetime]::Parse('%SINCE%'); [int][math]::Floor(((Get-Date) - $s).TotalMinutes) }catch{ '' }"`) do set "SINCEAGE=%%M"
  if not defined SINCEAGE (
    echo ERROR: --since will not parse: "%SINCE%"
    echo Refusing to judge a run against a moment this script cannot read.
    goto :usage
  )
)

set "STATUS=%REPO%\PROJECT_STATUS.md"

echo.
echo ============================================================
echo  watchdog
echo    status    : %STATUS%
echo    threshold : %MINUTES% min
if defined SINCE echo    since     : %SINCE%  ^(the run is %SINCEAGE% min old^)
echo ============================================================
echo.

if not exist "%REPO%\" (
  echo   UNKNOWN - repo root not found: %REPO%
  echo   Not a verdict about the session. The panel shows a reading
  echo   it could not take as unknown, and so does this.
  set "RC=2"
  goto :end
)
if not exist "%STATUS%" (
  echo   UNKNOWN - no PROJECT_STATUS.md at %REPO%
  echo   ABSENT is not STALE: nothing was read, so nothing is known
  echo   about whether that session is reporting.
  set "RC=2"
  goto :end
)

rem --- UPDATED, read from the file ------------------------------
set "RAW="
for /f "usebackq tokens=1,* delims=:" %%A in (`findstr /b /c:"UPDATED:" "%STATUS%"`) do set "RAW=%%B"
if not defined RAW (
  echo   UNKNOWN - no UPDATED line in PROJECT_STATUS.md
  echo   The field is required by STATUS_PROTOCOL.md section 3. Its
  echo   absence says the file was written wrong, not that the
  echo   session stopped.
  set "RC=2"
  goto :end
)
call :trim

rem --- the clock, and the age, both read at this moment ----------
set "AGE="
set "NOW="
for /f "usebackq delims=" %%D in (`powershell -NoProfile -Command "(Get-Date).ToString('yyyy-MM-ddTHH:mm:sszzz')"`) do set "NOW=%%D"
for /f "usebackq delims=" %%M in (`powershell -NoProfile -Command "try{ $u=[datetime]::Parse('%RAW%'); [int][math]::Floor(((Get-Date) - $u).TotalMinutes) }catch{ '' }"`) do set "AGE=%%M"

echo   UPDATED reads   : %RAW%
echo   clock now reads : %NOW%

if not defined AGE (
  echo   age             : could not be computed
  echo.
  echo   UNKNOWN - UPDATED will not parse: "%RAW%"
  echo   Written wrong is not the same as not written, and neither
  echo   is evidence that the session has stopped. CPS-DEC-029
  echo   makes the panel name such a value rather than act on it.
  set "RC=2"
  goto :end
)

echo   age             : %AGE% min
echo   threshold       : %MINUTES% min
if defined SINCE echo   run age         : %SINCEAGE% min ^(since %SINCE%^)
echo.

rem  --- the --since window ---------------------------------------
rem  AGE is how long ago UPDATED was written; SINCEAGE is how long
rem  the run has been going. If AGE is the LARGER, the write predates
rem  the run and belongs to the previous unit - so this run has not
rem  written at all, and the only honest question is whether it has
rem  had time to.
if not defined SINCE goto :verdict
if %AGE% LSS %SINCEAGE% goto :verdict

if %SINCEAGE% GEQ %MINUTES% (
  echo   STALE - this run has been going %SINCEAGE% minutes and has not
  echo   written PROJECT_STATUS.md once. The newest write is %AGE% minutes
  echo   old, which predates the run, so it belongs to the previous unit.
  echo   THE REAL STALL: the session started and never reported.
  echo   THIS SCRIPT DOES NOT KILL ANYTHING. It reports; something else decides.
  set "RC=1"
  goto :end
)

echo   FRESH - this run is %SINCEAGE% minutes old and has not written yet,
echo   which is inside the %MINUTES% minute threshold. The newest write is
echo   %AGE% minutes old and belongs to the PREVIOUS unit, so it is not
echo   evidence about this one. A run has not stalled until it has had
echo   time to report. THIS IS THE READING UNIT 039 MEASURED AS A FALSE
echo   KILL, and the whole reason --since exists.
set "RC=0"
goto :end

:verdict
if %AGE% GEQ %MINUTES% (
  echo   STALE - that status file has not been written for %AGE% minutes.
  echo   CLAUDE_CODE.md section 7 makes the write rule every ten while a
  echo   session is EXECUTING, so this is a rule broken rather than a guess.
  echo   THIS SCRIPT DOES NOT KILL ANYTHING. It reports; something else decides.
  set "RC=1"
  goto :end
)

echo   FRESH - written %AGE% minutes ago, inside the %MINUTES% minute threshold.
set "RC=0"
goto :end

rem ============================================================
:trim
if "%RAW:~0,1%"==" " set "RAW=%RAW:~1%" & goto :trim
goto :eof

rem ============================================================
:usage
echo.
echo   watchdog.bat [root] [--minutes N] [--since ISO]
echo.
echo   root defaults to C:\Source\HamLet
echo   --minutes defaults to 12, matching the panel's CFG.staleRunMin
echo.
echo   0 fresh, 1 stale, 2 unknown ^(absent, unreadable, unparseable^)
echo.
set "RC=2"
goto :end

rem ============================================================
:end
echo.
echo watchdog exit %RC%
endlocal & exit /b %RC%
