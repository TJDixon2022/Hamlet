@echo off
rem ============================================================
rem  status-check.bat  -  are the two status files telling the truth
rem
rem      status-check.bat [root]
rem
rem  Validates PROJECT_STATUS.md and PHASE_STATUS.md and PRINTS
rem  EVERY CHECK, passed or failed. Root defaults to C:\Source\HamLet.
rem
rem  Exit codes:
rem      0 = every check passed
rem      1 = at least one check failed
rem      2 = usage, or the root does not exist
rem
rem  A FAILING CHECK DOES NOT HALT THE PHASE. It is recorded and
rem  named in the ledger and the outcome entry so the fault reaches
rem  the owner attached to the unit that caused it. A status file is
rem  a report about work and not the work; refusing to continue
rem  because a caption is wrong throws away a good unit.
rem
rem  WHY IT EXISTS. Every fault these files have had in this project
rem  came from a different session writing prose by hand, so there is
rem  no single writer to harden and the guard has to sit downstream
rem  of all of them. The faults it is built against are all real in
rem  this tree and are listed in status-check.py's own header.
rem
rem  THE WORK IS IN PYTHON, and that is deliberate. The checks are
rem  date arithmetic against the clock, a byte scan for mojibake and
rem  a BOM, and a field-by-field parse - all of which cmd does badly
rem  and one of which (composing content inside nested shell quoting)
rem  is a named recurring corruption in this repository. The batch
rem  wrapper exists so the launcher calls it the way it calls
rem  everything else, and so the exit code is a batch exit code.
rem ============================================================

setlocal

rem  THIS SCRIPT'S OWN DIRECTORY, TAKEN BEFORE ANYTHING SHIFTS.
set "HERE=%~dp0"

set "REPO=%~1"
if "%REPO%"=="" set "REPO=C:\Source\HamLet"
if "%REPO:~-1%"=="\" set "REPO=%REPO:~0,-1%"

if not exist "%REPO%\" (
  echo ERROR: no such repository root: %REPO%
  endlocal & exit /b 2
)

python "%HERE%status-check.py" "%REPO%"
set "RC=%ERRORLEVEL%"

endlocal & exit /b %RC%
