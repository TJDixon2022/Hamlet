@echo off
REM ===========================================================================
REM  update-graph.bat -- refresh the Graphify graph for SharedRepo
REM  Version 5 (2026-08-06)
REM
REM  Usage:   update-graph.bat  [repo-root]
REM           update-graph.bat                 uses C:\Source\SharedRepo
REM           update-graph.bat D:\Other\Repo   overrides the root
REM
REM  Set GRAPH_NO_PAUSE=1 to skip the final pause (for scheduled runs).
REM
REM  v5 change: the staleness check compares graph.json against the files
REM  named in manifest.json -- the files graphify actually scanned -- instead
REM  of every file in the tree. v4 scanned everything and reported
REM  update-graph.bat itself as the newest source, so editing this tool
REM  claimed the graph was stale. Anything graphify does not scan cannot
REM  make its graph stale. The manifest is rewritten by the update that just
REM  ran, so it is current at the moment of comparison.
REM
REM  v4 finding, retained: 'graphify update' does NOT restamp the report's
REM  "Built from commit" line. Observed across three HEADs (2df71498,
REM  a1507902, b63077d0) while the report held at 2df71498. That line records
REM  the last FULL build. A difference from HEAD is not staleness.
REM
REM  Uses only the invocation documented in GRAPH_REPORT.md:  graphify update .
REM  No flags are passed. Exclusions belong in .graphifyignore or a verified
REM  graphify option -- not guessed here.
REM ===========================================================================

setlocal EnableExtensions EnableDelayedExpansion

set "REPO=C:\Source\SharedRepo"
if not "%~1"=="" set "REPO=%~1"

set "RC=0"

REM --- 0. UTF-8 console, remembering what to put back ------------------------
set "OLDCP="
for /f "tokens=2 delims=:" %%C in ('chcp') do set "OLDCP=%%C"
if defined OLDCP set "OLDCP=!OLDCP: =!"
if defined OLDCP set "OLDCP=!OLDCP:.=!"
chcp 65001 >nul 2>&1

echo.
echo === Graph update: %REPO%
echo.

REM --- 1. Sanity: is this actually the repo root? -----------------------------
if not exist "%REPO%\CLAUDE.md" goto :no_repo
if not exist "%REPO%\.git" goto :no_git

REM --- 2. Is graphify on PATH? ------------------------------------------------
where graphify >nul 2>&1
if errorlevel 1 goto :no_graphify

pushd "%REPO%" || goto :no_pushd

REM --- 3. Current HEAD, for the log ------------------------------------------
set "HEAD_SHA="
for /f "usebackq delims=" %%H in (`git rev-parse HEAD 2^>nul`) do set "HEAD_SHA=%%H"
if not defined HEAD_SHA goto :no_head
set "HEAD_SHORT=!HEAD_SHA:~0,8!"
echo HEAD                : !HEAD_SHORT!

set "DIRTY="
for /f "usebackq delims=" %%D in (`git status --porcelain 2^>nul`) do set "DIRTY=1"
if defined DIRTY (
  echo Working tree        : UNCOMMITTED CHANGES
) else (
  echo Working tree        : clean
)

REM --- 4. Run the update -----------------------------------------------------
echo.
echo --- graphify update . ---
echo.
call graphify update .
set "RC=!ERRORLEVEL!"
echo.
if not "!RC!"=="0" goto :graphify_failed

REM --- 5. Show what landed and when ------------------------------------------
echo --- artifacts ---
set "FOUND="
for /f "usebackq delims=" %%F in (`dir /s /b /a-d graph.json manifest.json GRAPH_REPORT.md graph.html 2^>nul ^| findstr /v /i "node_modules \\bin\\ \\obj\\"`) do (
  set "FOUND=1"
  for %%T in ("%%F") do echo   %%~tT   %%F
)
if not defined FOUND echo   NONE FOUND -- graphify reported success but wrote nothing here.
echo.
echo   Only manifest.json moving is normal: graphify re-scanned, found no
echo   topology change, and left the graph alone. It says so above.

REM --- 6. Resolve the output directory ----------------------------------------
set "OUTDIR=graphify-out"
set "REPORT="
if exist "!OUTDIR!\GRAPH_REPORT.md" set "REPORT=!OUTDIR!\GRAPH_REPORT.md"
if not defined REPORT if exist "GRAPH_REPORT.md" set "REPORT=GRAPH_REPORT.md" & set "OUTDIR=."

REM --- 7. Staleness, scoped to what graphify actually scans -------------------
echo.
echo --- staleness ---
powershell -NoProfile -ExecutionPolicy Bypass -Command "$ErrorActionPreference='SilentlyContinue'; $g=Get-Item 'graphify-out\graph.json'; if(-not $g){$g=Get-Item 'graph.json'}; if(-not $g){Write-Output '  graph.json not found - cannot compare'; exit}; $mf='graphify-out\manifest.json'; if(-not (Test-Path $mf)){$mf='manifest.json'}; if(-not (Test-Path $mf)){Write-Output '  manifest.json not found - cannot compare'; exit}; $m=Get-Content $mf -Raw | ConvertFrom-Json; $names=$m.PSObject.Properties.Name; $newest=$null; $missing=0; foreach($p in $names){ $f=Get-Item $p; if(-not $f){$missing=$missing+1; continue}; if($newest -eq $null -or $f.LastWriteTime -gt $newest.LastWriteTime){$newest=$f} }; if(-not $newest){Write-Output '  no scanned files found on disk'; exit}; Write-Output ('  scanned files : ' + $names.Count); Write-Output ('  graph.json    : ' + $g.LastWriteTime); Write-Output ('  newest scanned: ' + $newest.LastWriteTime + '   ' + $newest.Name); if($missing -gt 0){Write-Output ('  NOTE          : ' + $missing + ' manifest entries no longer on disk - deleted or moved')}; if($newest.LastWriteTime -gt $g.LastWriteTime){Write-Output '  VERDICT       : STALE - a scanned file changed after the graph was built'}else{Write-Output '  VERDICT       : CURRENT - no scanned file is newer than the graph'}"
echo.
echo   Scope note: only files graphify scans can make its graph stale.
echo   Editing this batch file, or anything .graphifyignore excludes, cannot.

REM --- 8. Commit line, correctly labeled ------------------------------------
if defined REPORT (
  echo.
  echo --- last full build ---
  for /f "usebackq tokens=* delims=" %%L in (`findstr /C:"Built from commit" "!REPORT!" 2^>nul`) do echo   %%L
  echo   Git HEAD is       : !HEAD_SHORT!
  echo   These differ after any commit. 'update' does not restamp the line;
  echo   only a full build does. A difference is NOT staleness -- see above.
)

REM --- 9. Scale -- summary line only ------------------------------------------
echo.
echo --- scale ---
set "SCALE="
if defined REPORT for /f "usebackq tokens=* delims=" %%L in (`findstr /R /C:"^- [0-9][0-9]* nodes" "!REPORT!" 2^>nul`) do set "SCALE=%%L"
if defined SCALE (
  echo   !SCALE!
  echo   Compare with the previous run. A large drop means an ignore rule
  echo   swallowed a tree; a large jump means something new was scanned.
) else (
  echo   Summary line not found.
)

REM ===========================================================================
echo.
echo ==========================================================================
echo  UPLOAD THESE THREE TO CLAUDE
echo ==========================================================================
echo.
echo   %REPO%\!OUTDIR!\GRAPH_REPORT.md
echo   %REPO%\!OUTDIR!\graph.json
echo   %REPO%\!OUTDIR!\manifest.json
echo.
echo   GRAPH_REPORT.md   summary: scale, god nodes, cohesion, gaps
echo   graph.json        the nodes and edges themselves, for checking claims
echo   manifest.json     which files were scanned at all
echo.
echo   NOT needed: graph.html -- that viewer is for you, not Claude.
echo.
echo   Send them when something structural changed: a new project, a moved
echo   folder, a large delivery landing. Not after every run.
echo.

echo Reminders:
echo   - Static-member calls are a known blind spot. Verify any "orphan" or
echo     "isolated" claim with a source grep before acting on it.
echo   - Absence from the graph is weak evidence, not proof.
echo   - semantic_hash empty in manifest.json means semantic extraction did
echo     not run (no GEMINI_API_KEY / GOOGLE_API_KEY). Structure only.
echo   - appsettings*.json produce zero nodes (graphify issue #1666).
echo.
echo Done. Exit code 0.
popd
goto :end

REM ===========================================================================
:no_repo
echo ERROR: CLAUDE.md not found under "%REPO%".
echo        That path is not the repository root. Pass the correct root:
echo            update-graph.bat C:\Source\SharedRepo
set "RC=2"
goto :end

:no_git
echo ERROR: no .git directory under "%REPO%".
set "RC=2"
goto :end

:no_graphify
echo ERROR: 'graphify' is not on PATH.
echo        Open a shell where it works, run:  where graphify
echo        then add that folder to PATH, or call this file from that shell.
set "RC=3"
goto :end

:no_pushd
echo ERROR: could not change directory to "%REPO%".
set "RC=2"
goto :end

:no_head
echo ERROR: git rev-parse HEAD returned nothing. Is there at least one commit?
popd
set "RC=4"
goto :end

:graphify_failed
echo ERROR: graphify exited with code !RC!. Graph NOT updated.
echo        The previous graph files, if any, are unchanged and now stale.
popd
goto :end

:end
if defined OLDCP chcp !OLDCP! >nul 2>&1
echo.
if not defined GRAPH_NO_PAUSE pause
exit /b %RC%
