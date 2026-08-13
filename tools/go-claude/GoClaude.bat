@echo off
rem ============================================================
rem  GoClaude.bat  -  launch Claude Code unattended in Hamlet
rem
rem  Starts Claude Code with permission prompts DISABLED so a
rem  session can extract, edit, delete, build, test and commit
rem  without stopping every few seconds to ask.
rem
rem  --dangerously-skip-permissions turns off the guardrails for
rem  the whole session. That includes file deletion and git
rem  operations. Only run this in a repository whose work is
rem  committed and pushed, so anything wrong is one git command
rem  away from being undone.
rem
rem  Repo root defaults to C:\Source\Hamlet, falling back to
rem  C:\Source\HamManager if the rename has not happened yet.
rem  Override it:
rem      GoClaude.bat D:\Some\Other\Repo
rem  A trailing backslash on the argument is accepted.
rem ============================================================

setlocal

set "REPO=%~1"

rem --- normalize: drop a trailing backslash -------------------------
if defined REPO if "%REPO:~-1%"=="\" set "REPO=%REPO:~0,-1%"

rem --- default, with pre-rename fallback ----------------------------
if not defined REPO (
  if exist "C:\Source\Hamlet\" (
    set "REPO=C:\Source\Hamlet"
  ) else (
    set "REPO=C:\Source\HamManager"
  )
)

echo.
echo  Repo : %REPO%
echo  Mode : permissions SKIPPED - Claude will not ask before acting
echo.

if not exist "%REPO%\" (
  echo ERROR: repo root not found: %REPO%
  echo Pass the correct path as the first argument.
  goto :done
)

rem --- verify claude is on PATH -------------------------------------
where claude >nul 2>&1
if errorlevel 1 (
  echo ERROR: 'claude' was not found on PATH.
  echo Install Claude Code, or open a new terminal so PATH refreshes.
  goto :done
)

cd /d "%REPO%"

rem --- warn if the tree is dirty ------------------------------------
rem  Unattended mode can commit over uncommitted work. Knowing the
rem  tree is dirty BEFORE starting is the difference between a clean
rem  undo and an archaeology session.
git rev-parse HEAD >nul 2>&1
if not errorlevel 1 (
  for /f %%D in ('git status --porcelain ^| find /c /v ""') do set "DIRTY=%%D"
  for /f "tokens=*" %%B in ('git rev-parse --abbrev-ref HEAD') do set "BRANCH=%%B"
  echo  Branch      : %BRANCH%
  echo  Dirty files : %DIRTY%
  echo.
  if not "%DIRTY%"=="0" (
    echo  NOTE: uncommitted changes are present. Commit or stash them
    echo        first if you want a clean point to roll back to.
    echo.
  )
)

echo Starting Claude Code. Ctrl+C to abort.
echo.

claude --dangerously-skip-permissions

:done
echo.
pause
endlocal
exit /b
