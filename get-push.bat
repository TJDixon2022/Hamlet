@echo off
rem ============================================================
rem  get-push.bat  -  say what is unpushed, push it, verify
rem
rem  Two ways in:
rem    1. Double-click the copy in a repository root. It works on
rem       the folder it sits in.
rem    2. get-push.bat C:\Some\Repo  - works on that folder.
rem       This is how get-push-all.bat drives it.
rem  A trailing backslash on the argument is accepted.
rem
rem  Set GETPUSH_NOPAUSE to skip the pause at the end. The master
rem  sets it so a twelve-repo run does not need twelve keypresses.
rem
rem  Exit codes - get-push-all.bat counts these, do not renumber:
rem    0  nothing to push
rem    1  pushed and verified
rem    2  no upstream, needs one manual push
rem    3  push refused by the remote
rem    4  not usable - no git, no folder, not a work tree,
rem       detached HEAD
rem    5  push reported success but commits remain
rem
rem  DO NOT EDIT A DISTRIBUTED COPY. It is overwritten by
rem  distribute-get-push.bat. Edit the master in Downloads and
rem  re-run the distributor.
rem ============================================================

setlocal

set "RC=4"

set "REPO=%~1"
if "%REPO%"=="" set "REPO=%~dp0"

rem --- normalise: drop a trailing backslash -------------------------
rem  %~dp0 always ends in one; drag-and-drop often does.
if "%REPO:~-1%"=="\" set "REPO=%REPO:~0,-1%"

echo.
echo Repo: %REPO%
echo.

where git >nul 2>&1
if errorlevel 1 (
  echo ERROR: git is not on PATH. Nothing done.
  goto :done
)

if not exist "%REPO%\" (
  echo ERROR: folder not found: %REPO%
  goto :done
)

pushd "%REPO%"

git rev-parse --is-inside-work-tree >nul 2>&1
if errorlevel 1 (
  echo ERROR: this folder is not a git working tree. Nothing done.
  goto :popdone
)

rem --- branch ------------------------------------------------------
set "BRANCH="
for /f "usebackq delims=" %%B in (`git rev-parse --abbrev-ref HEAD 2^>nul`) do set "BRANCH=%%B"

if "%BRANCH%"=="" (
  echo ERROR: could not read the current branch. Nothing done.
  goto :popdone
)
if "%BRANCH%"=="HEAD" (
  echo ERROR: detached HEAD - there is no branch to push.
  echo Check out a branch first. Nothing done.
  goto :popdone
)

rem --- upstream ----------------------------------------------------
set "UPSTREAM="
for /f "usebackq delims=" %%U in (`git rev-parse --abbrev-ref --symbolic-full-name "@{u}" 2^>nul`) do set "UPSTREAM=%%U"

if "%UPSTREAM%"=="" (
  set "RC=2"
  echo Branch   : %BRANCH%
  echo Upstream : NONE - this branch has never been pushed.
  echo.
  echo Not guessing a remote. Run this once, by hand:
  echo     git push -u origin %BRANCH%
  echo Then this script works on it from now on.
  goto :popdone
)

rem --- uncommitted work --------------------------------------------
rem  Reported, never acted on. A push does not carry it and the
rem  count is the only warning that it is being left behind.
set "DIRTY=0"
for /f %%C in ('git status --porcelain 2^>nul ^| find /c /v ""') do set "DIRTY=%%C"

rem --- refresh the remote-tracking refs -----------------------------
rem  Without this the ahead/behind counts are whatever was true the
rem  last time anything talked to the remote.
echo Fetching...
git fetch --quiet
if errorlevel 1 echo   WARNING: fetch failed - counts below may be stale.

rem --- counts ------------------------------------------------------
set "BEHIND=0"
set "AHEAD=0"
for /f "usebackq tokens=1,2" %%A in (`git rev-list --left-right --count "@{u}...HEAD" 2^>nul`) do (
  set "BEHIND=%%A"
  set "AHEAD=%%B"
)

echo.
echo   Branch      : %BRANCH%
echo   Upstream    : %UPSTREAM%
echo   To push     : %AHEAD% commits
echo   Behind      : %BEHIND% commits
echo   Uncommitted : %DIRTY% files
echo.

if not "%DIRTY%"=="0" echo NOTE: uncommitted work is NOT included in a push.
if not "%BEHIND%"=="0" echo NOTE: upstream is ahead. If the push is rejected, pull first.

if "%AHEAD%"=="0" (
  set "RC=0"
  echo Nothing to push.
  goto :popdone
)

rem --- push --------------------------------------------------------
echo Pushing %AHEAD% commits to %UPSTREAM% ...
echo.
git push
if errorlevel 1 (
  set "RC=3"
  echo.
  echo PUSH FAILED - see the message above. Nothing was sent.
  echo The commits are still local. Do not assume they are safe.
  goto :popdone
)

rem --- verify ------------------------------------------------------
rem  The exit code is not the check. The remaining count is.
set "LEFT=1"
for /f %%A in ('git rev-list --count "@{u}..HEAD" 2^>nul') do set "LEFT=%%A"

echo.
if "%LEFT%"=="0" (
  set "RC=1"
  echo PUSHED OK - %AHEAD% commits are now on %UPSTREAM%.
) else (
  set "RC=5"
  echo WARNING: push reported success but %LEFT% commits are still
  echo unpushed. Check the branch and the remote by hand.
)

:popdone
popd

:done
echo.
if not defined GETPUSH_NOPAUSE pause
endlocal & exit /b %RC%
