@echo off
rem ============================================================
rem  write-status.bat  -  write PROJECT_STATUS.md, CLAUDE.md 13.1
rem
rem  Why this exists: PROJECT_STATUS.md was found at ZERO BYTES on
rem  2026-08-21 while the session was committing and pushing
rem  normally. A write truncated the file and produced no content,
rem  silently. That is CLAUDE_CODE.md 11's first named failure,
rem  composing file content inside nested shell quoting.
rem
rem  Three things this makes impossible:
rem    1. A zero-byte result. Content is built in a temp file and
rem       only moved over the real one after it is verified
rem       non-empty. A failed write leaves the previous status
rem       standing, which is stale - but stale is legible and
rem       empty is not.
rem    2. A typed timestamp. UPDATED is read from the clock here.
rem       It is not an argument and cannot be passed in.
rem    3. An invented STATE or BALL. Both are checked against the
rem       lists in CLAUDE.md 13.1 and the script refuses.
rem
rem  Usage, from the repository root:
rem    write-status.bat STATE PHASE BALL NEXT_PASTE "NOTE"
rem
rem  Example:
rem    write-status.bat EXECUTING "2 of 5" code none ^
rem      "Phase 2 - rebuilding AR/IR fixtures, 4 of 11"
rem
rem  Quote any argument containing spaces. NOTE must not contain
rem  & | < > ^ - cmd will eat them. Keep it a caption.
rem ============================================================

setlocal

set "STATE=%~1"
set "PHASE=%~2"
set "BALL=%~3"
set "NEXT_PASTE=%~4"
set "NOTE=%~5"

set "OUT=PROJECT_STATUS.md"
set "TMP=PROJECT_STATUS.md.tmp"

rem --- all six must be present -------------------------------------
if "%STATE%"=="" goto :usage
if "%PHASE%"=="" goto :usage
if "%BALL%"=="" goto :usage
if "%NEXT_PASTE%"=="" goto :usage
if "%NOTE%"=="" goto :usage

rem --- STATE: one of five words and nothing else -------------------
set "OK="
if /i "%STATE%"=="PREPARING_PROMPT"   set "OK=1"
if /i "%STATE%"=="ANSWERING_QUESTIONS" set "OK=1"
if /i "%STATE%"=="EXECUTING"          set "OK=1"
if /i "%STATE%"=="COMPLETED"          set "OK=1"
if /i "%STATE%"=="BLOCKED"            set "OK=1"
if not defined OK (
  echo ERROR: STATE was "%STATE%".
  echo Must be one of: PREPARING_PROMPT ANSWERING_QUESTIONS EXECUTING COMPLETED BLOCKED
  echo Nothing written. The previous status still stands.
  goto :fail
)

rem --- BALL: one of four -------------------------------------------
set "OK="
if /i "%BALL%"=="code"       set "OK=1"
if /i "%BALL%"=="web"        set "OK=1"
if /i "%BALL%"=="tim"        set "OK=1"
if /i "%BALL%"=="unassigned" set "OK=1"
if not defined OK (
  echo ERROR: BALL was "%BALL%".
  echo Must be one of: code web tim unassigned
  echo Nothing written. The previous status still stands.
  goto :fail
)

rem --- UPDATED, read from the clock, never typed -------------------
rem  Get-Date -Format o is ISO 8601 with a UTC offset.
set "UPDATED="
for /f "usebackq delims=" %%T in (`powershell -NoProfile -Command "Get-Date -Format o"`) do set "UPDATED=%%T"
if not defined UPDATED (
  echo ERROR: could not read the clock. Nothing written.
  echo A status file with no measured timestamp is worse than a stale one.
  goto :fail
)

rem --- build in a temp file ----------------------------------------
if exist "%TMP%" del /q "%TMP%"

echo STATE: %STATE%>>"%TMP%"
echo PHASE: %PHASE%>>"%TMP%"
echo BALL: %BALL%>>"%TMP%"
echo NEXT_PASTE: %NEXT_PASTE%>>"%TMP%"
echo UPDATED: %UPDATED%>>"%TMP%"
echo NOTE: %NOTE%>>"%TMP%"

rem --- verify non-empty BEFORE touching the real file --------------
if not exist "%TMP%" (
  echo ERROR: temp file was not created. %OUT% is untouched.
  goto :fail
)
for %%Z in ("%TMP%") do set "SIZE=%%~zZ"
if "%SIZE%"=="0" (
  echo ERROR: temp file is zero bytes. %OUT% is untouched.
  del /q "%TMP%"
  goto :fail
)

move /y "%TMP%" "%OUT%" >nul
if errorlevel 1 (
  echo ERROR: could not move temp over %OUT%. Staged content is in %TMP%.
  goto :fail
)

echo Wrote %OUT% ^(%SIZE% bytes^)
type "%OUT%"
endlocal
exit /b 0

:usage
echo.
echo Usage: write-status.bat STATE PHASE BALL NEXT_PASTE "NOTE"
echo.
echo   STATE       PREPARING_PROMPT ANSWERING_QUESTIONS EXECUTING COMPLETED BLOCKED
echo   PHASE       "n of m", or "-" when no work order is running
echo   BALL        code web tim unassigned
echo   NEXT_PASTE  none, or "what -^> where"
echo   NOTE        one line, a caption
echo.
echo UPDATED is read from the clock and is not an argument.
echo.

:fail
endlocal
exit /b 1
