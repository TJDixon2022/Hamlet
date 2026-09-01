@echo off
rem ============================================================
rem  ledger.bat  -  what the owner reads instead of watching
rem
rem      ledger.bat <unit> <start> <end> <exit-state> <answer>
rem                 [cost] [root]
rem
rem  Appends ONE line to RUN_LEDGER.md at the repository root.
rem
rem      0  the line was appended
rem      1  the line could not be appended
rem      2  usage, or bad root
rem
rem  APPEND-ONLY, AND THAT IS THE WHOLE DESIGN. It never rewrites
rem  an existing line, never sorts, never de-duplicates and never
rem  corrects. GROKBOT.md section 3: this is what the owner reads
rem  instead of watching, and a record that can be rewritten is a
rem  record he has to verify rather than read. The same reason
rem  CLAUDE.md section 1 says a ruling is never edited - it is
rem  superseded by a later row.
rem
rem  A run that went wrong therefore gets a SECOND line saying so,
rem  not a corrected first one.
rem
rem  ARGUMENTS. All five are required and none is invented:
rem    <unit>        the work instruction number, e.g. 038
rem    <start>       when the unit began, ISO
rem    <end>         when it ended, ISO
rem    <exit-state>  complete / blocked / failed / stopped -
rem                  CLAUDE_CODE.md section 8's four, and the
rem                  script does not check the spelling because a
rem                  fifth word in the ledger is better than a
rem                  refusal that loses the line
rem    <answer>      what section 3 leads with, in the owner's
rem                  terms. Quote it.
rem    [cost]        total_cost_usd from the run's JSON, or the
rem                  word unknown. OPTIONAL, and it defaults to
rem                  unknown rather than to a number.
rem
rem  ON THE COST FIELD, added 2026-08-28 by work instructions 040
rem  task 6, which licensed this one change. Unit 039 had nowhere
rem  to put a cost and carried it inside the answer text, where
rem  nothing can sum it or sort by it. The three historical lines
rem  written by 039 keep unknown and are NOT backfilled: AN
rem  INVENTED COST IS WORSE THAN AN ABSENT ONE, because the ledger
rem  is what the owner reads instead of watching and a fabricated
rem  number in it poisons everything read from it afterwards.
rem
rem  It defaults to unknown rather than to 0. A zero would be a
rem  claim that the run was free.
rem
rem  Nothing here is read from the clock, deliberately: the caller
rem  knows when the unit began and this script does not, and a
rem  start time invented at append time would be a composed
rem  timestamp - CLAUDE_CODE.md section 11, twice.
rem
rem  THE HEADER IS WRITTEN ONCE. If RUN_LEDGER.md does not exist
rem  it is created with a heading and a table header, then the
rem  line is appended. If it does exist, nothing above the new
rem  line is touched.
rem
rem  Batch, not PowerShell: a .ps1 will not run on this machine.
rem
rem  Generated 2026-08-28 for: work instructions 038 task 5
rem  Amended  2026-08-28 for: work instructions 040 task 6 - the cost
rem           field, licensed by that task and by nothing else.
rem ============================================================

setlocal

set "RC=0"
set "UNIT=%~1"
set "STARTED=%~2"
set "ENDED=%~3"
set "STATE=%~4"
set "ANSWER=%~5"
set "COST=%~6"
set "REPO=%~7"

if "%UNIT%"==""    goto :usage
if "%STARTED%"=="" goto :usage
if "%ENDED%"==""   goto :usage
if "%STATE%"==""   goto :usage
if "%ANSWER%"==""  goto :usage
if "%COST%"=="" set "COST=unknown"

if "%REPO%"=="" set "REPO=C:\Source\HamLet"
if "%REPO:~-1%"=="\" set "REPO=%REPO:~0,-1%"

if not exist "%REPO%\" (
  echo ERROR: repo root not found: %REPO%
  set "RC=2"
  goto :end
)

set "LEDGER=%REPO%\RUN_LEDGER.md"

echo.
echo ============================================================
echo  ledger
echo    file : %LEDGER%
echo ============================================================
echo.

rem --- the header, written once and never again ------------------
if not exist "%LEDGER%" (
  echo   RUN_LEDGER.md does not exist yet. Creating it with a header.
  >"%LEDGER%" echo # RUN_LEDGER.md
  >>"%LEDGER%" echo.
  >>"%LEDGER%" echo Append-only. One line per unit. **No line is ever rewritten** - a run
  >>"%LEDGER%" echo that went wrong gets a second line saying so, not a corrected first one.
  >>"%LEDGER%" echo Written by `tools\arbiter\ledger.bat`. GROKBOT.md section 3: this is what
  >>"%LEDGER%" echo the owner reads instead of watching.
  >>"%LEDGER%" echo.
  >>"%LEDGER%" echo ^| Unit ^| Started ^| Ended ^| Exit ^| Cost ^| What section 3 led with ^|
  >>"%LEDGER%" echo ^|---^|---^|---^|---^|---^|---^|
  if not exist "%LEDGER%" (
    echo   ERROR: could not create %LEDGER%
    set "RC=1"
    goto :end
  )
)

rem --- count the lines before, so the append can be proven -------
set "BEFORE=0"
for /f "usebackq delims=" %%N in (`powershell -NoProfile -Command "(Get-Content -LiteralPath '%LEDGER%').Count"`) do set "BEFORE=%%N"

>>"%LEDGER%" echo ^| %UNIT% ^| %STARTED% ^| %ENDED% ^| %STATE% ^| %COST% ^| %ANSWER% ^|

set "AFTER=0"
for /f "usebackq delims=" %%N in (`powershell -NoProfile -Command "(Get-Content -LiteralPath '%LEDGER%').Count"`) do set "AFTER=%%N"

echo   lines before : %BEFORE%
echo   lines after  : %AFTER%
echo.

if "%BEFORE%"=="%AFTER%" (
  echo   ERROR: the line was not appended.
  set "RC=1"
  goto :end
)

echo   Appended:
echo     ^| %UNIT% ^| %STARTED% ^| %ENDED% ^| %STATE% ^| %COST% ^| %ANSWER% ^|
echo.
echo   Nothing above it was touched. This script only ever appends.
set "RC=0"
goto :end

rem ============================================================
:usage
echo.
echo   ledger.bat ^<unit^> ^<start^> ^<end^> ^<exit-state^> ^<answer^> [cost] [root]
echo.
echo   All five are required. Quote the answer.
echo   root defaults to C:\Source\HamLet
echo.
echo   Example:
echo     ledger.bat 040 2026-08-28T19:16 2026-08-28T19:40 complete "the kill fires" 0.42
echo.
echo   0 appended, 1 could not append, 2 usage or bad root
echo.
set "RC=2"
goto :end

rem ============================================================
:end
echo.
echo ledger exit %RC%
endlocal & exit /b %RC%
