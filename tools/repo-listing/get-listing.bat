@echo off
rem ============================================================
rem  get-listing.bat  -  produce repo_listing.txt
rem
rem  Run this BEFORE anything else. Claude cannot ask for a file
rem  it does not know exists, and cannot deliver into a folder
rem  structure it has not read. This is the bootstrap.
rem
rem  Put this in your Downloads folder and double-click it, or run
rem  it from a prompt. It writes repo_listing.txt to Downloads.
rem  Upload that file to the session.
rem
rem  Repo root defaults to C:\Source\Hamlet.
rem  Override it:
rem      get-listing.bat D:\Some\Other\Repo
rem  A trailing backslash on the argument is accepted.
rem
rem  Re-run whenever files are added, removed or moved. A stale
rem  listing is worse than none - it produces confident requests
rem  for paths that no longer exist.
rem ============================================================

setlocal enabledelayedexpansion

set "REPO=%~1"
if "%REPO%"=="" set "REPO=C:\Source\Hamlet"

rem --- normalise: drop a trailing backslash -------------------------
rem  Drag-and-drop and tab completion both produce one, and it makes
rem  every path in the listing come out absolute instead of relative.
if "%REPO:~-1%"=="\" set "REPO=%REPO:~0,-1%"

set "OUTDIR=%USERPROFILE%\Downloads"
set "OUT=%OUTDIR%\repo_listing.txt"

echo.
echo Repo root : %REPO%
echo Output    : %OUT%
echo.

if not exist "%REPO%\" (
  echo ERROR: repo root not found: %REPO%
  echo Pass the correct path as the first argument.
  goto :done
)

if exist "%OUT%" del /q "%OUT%"

rem --- header ------------------------------------------------------
echo repo_listing.txt>>"%OUT%"
echo Generated: %DATE% %TIME%>>"%OUT%"
echo Repo root: %REPO%>>"%OUT%"
echo.>>"%OUT%"

rem --- git commit, if this is a working tree -----------------------
pushd "%REPO%"
git rev-parse HEAD >nul 2>&1
if not errorlevel 1 (
  for /f %%H in ('git rev-parse HEAD') do echo Commit: %%H>>"%OUT%"
  for /f "tokens=*" %%B in ('git rev-parse --abbrev-ref HEAD') do echo Branch: %%B>>"%OUT%"
  for /f %%D in ('git status --porcelain ^| find /c /v ""') do echo Dirty files: %%D>>"%OUT%"
) else (
  echo Commit: not a git working tree>>"%OUT%"
)
popd

echo.>>"%OUT%"
echo ============================================================>>"%OUT%"
echo FILES  ^(size in bytes, modified, path relative to repo root^)>>"%OUT%"
echo ============================================================>>"%OUT%"
echo.>>"%OUT%"

rem --- the listing -------------------------------------------------
rem  Excluded: build output, package caches, IDE state, and the
rem  graph artifacts. Everything else is listed.
rem
rem  Exclusion is a plain substring test on the relative path with a
rem  leading backslash prepended, so a folder at the repo root is
rem  caught as well as a nested one. cmd's substring replacement is
rem  case-insensitive, which is what /i gave before.
rem
rem  findstr is deliberately NOT used here. Its handling of
rem  backslashes inside /c:"..." depends on how the C runtime strips
rem  them before findstr ever sees the argument, so a filter that
rem  looks correct can silently match nothing and let every build
rem  artifact into the listing. The Excluded count below is the
rem  evidence that the filters actually fired.
set /a COUNT=0
set /a BYTES=0
set /a EXCLUDED=0

pushd "%REPO%"
for /f "delims=" %%F in ('dir /b /s /a-d 2^>nul') do (
  set "FULL=%%F"
  set "REL=!FULL:%REPO%\=!"
  set "TEST=\!REL!"
  set "SKIP="
  if not "!TEST:\node_modules\=!"=="!TEST!" set SKIP=1
  if not "!TEST:\bin\=!"=="!TEST!"          set SKIP=1
  if not "!TEST:\obj\=!"=="!TEST!"          set SKIP=1
  if not "!TEST:\.git\=!"=="!TEST!"         set SKIP=1
  if not "!TEST:\.vs\=!"=="!TEST!"          set SKIP=1
  if not "!TEST:\packages\=!"=="!TEST!"     set SKIP=1
  if not "!TEST:\TestResults\=!"=="!TEST!"  set SKIP=1
  if not "!TEST:\coverage\=!"=="!TEST!"     set SKIP=1
  if not "!TEST:\dist\=!"=="!TEST!"         set SKIP=1
  if not "!TEST:\graphify-out\=!"=="!TEST!" set SKIP=1
  if defined SKIP (
    set /a EXCLUDED+=1
  ) else (
    for %%S in ("%%F") do (
      echo %%~zS^	%%~tS^	!REL!>>"%OUT%"
      set /a COUNT+=1
      set /a BYTES+=%%~zS
    )
  )
)
popd

echo.>>"%OUT%"
echo ============================================================>>"%OUT%"
echo Files listed  : %COUNT%>>"%OUT%"
echo Files excluded: %EXCLUDED%>>"%OUT%"
echo Total bytes   : %BYTES%>>"%OUT%"
echo ============================================================>>"%OUT%"

echo.
echo Files listed  : %COUNT%
echo Files excluded: %EXCLUDED%
echo.
echo Created: %OUT%
echo Upload that file to the session.

:done
echo.
pause
endlocal
exit /b
