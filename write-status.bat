@echo off
rem ============================================================
rem  write-status.bat  -  write PROJECT_STATUS.md, CLAUDE.md 13.1
rem
rem  THIS IS NOW A SHIM. It forwards to tools\write-status.py and
rem  keeps no copy of the answer.
rem
rem  WHY, AND IT IS A MEASUREMENT. This script used to write the
rem  file itself, and it wrote SIX lines - STATE, PHASE, BALL,
rem  NEXT_PASTE, UPDATED, NOTE. ANNUNCIATOR.md's contract is eight
rem  and names PROTOCOL, TASK, WORK_INSTRUCTION and PROMPT among
rem  them. So running it DROPPED fields the panel reads, and on
rem  2026-08-31 PROJECT_STATUS.md was missing PROTOCOL and
rem  WORK_INSTRUCTION while return-package.bat was already reading
rem  WORK_INSTRUCTION out of it and silently getting nothing.
rem
rem  Two writers with two different field sets is exactly the drift
rem  CLAUDE.md section 0 forbids - where something can be generated
rem  from one source of truth, generate it. The Python writer is
rem  that source: it validates STATE and BALL against the panel's
rem  own vocabularies, reads UPDATED from the clock, writes ASCII
rem  with no byte-order mark, and stages through a temp file so a
rem  failed write leaves the previous status standing.
rem
rem  THE ARGUMENT LIST CHANGED WITH THE FIELD SET, so the old
rem  five-argument form is REFUSED rather than silently reinterpreted
rem  - a shim that guesses which contract it was called under is how
rem  a wrong value gets written confidently.
rem
rem      write-status.bat STATE TASK WORK_INSTRUCTION BALL NEXT_PASTE "NOTE"
rem
rem  Example:
rem      write-status.bat EXECUTING "2 of 6" 204 code none ^
rem        "Task 2 - measuring against the corpus"
rem
rem  Exit codes are the Python writer's: 0 written, 1 refused or
rem  the write failed, 2 usage.
rem ============================================================

setlocal

set "HERE=%~dp0"

if "%~1"=="" goto :usage
if "%~2"=="" goto :usage
if "%~3"=="" goto :usage
if "%~4"=="" goto :usage
if "%~5"=="" goto :usage
if "%~6"=="" goto :usage

python "%HERE%tools\write-status.py" %1 %2 %3 %4 %5 %6 %7 %8 %9
set "RC=%errorlevel%"
endlocal & exit /b %RC%

:usage
echo.
echo Usage: write-status.bat STATE TASK WORK_INSTRUCTION BALL NEXT_PASTE "NOTE"
echo.
echo   STATE            PREPARING_PROMPT ANSWERING_QUESTIONS EXECUTING
echo                    COMPLETED BLOCKED
echo   TASK             "n of m", or "-" when no work instruction is running
echo   WORK_INSTRUCTION what is running, or none. Never absent.
echo   BALL             code web tim unassigned
echo   NEXT_PASTE       none, or "what -^> where"
echo   NOTE             one line, a caption
echo.
echo Optional, after NOTE:  --prompt N  --rules-at "HM-DEC-nnn (date)"
echo.
echo UPDATED is read from the clock and is not an argument.
echo.
echo NOTE: this script's OLD five-argument form
echo   write-status.bat STATE PHASE BALL NEXT_PASTE "NOTE"
echo wrote six fields and dropped PROTOCOL, TASK, WORK_INSTRUCTION
echo and PROMPT. It is refused rather than reinterpreted.
echo.
endlocal & exit /b 2
