@echo off
rem ============================================================
rem  return-package.bat  -  the return trip
rem
rem  GROKBOT.md 2 phase 2 and 4 task 7: package output.md,
rem  PROJECT_STATUS.md and the git log for the unit into one zip
rem  in Downloads, for upload.
rem
rem      return-package.bat <since-commit> [root]
rem
rem  Contains: output.md, PROJECT_STATUS.md, git-log.txt, HEAD.txt,
rem  reload.txt and MANIFEST.txt.
rem
rem  <since-commit> is where the unit began. The log packaged is
rem  <since-commit>..HEAD.
rem
rem  THE EXIT CODE IS THE POINT:
rem
rem      0 = zip written
rem      2 = usage, bad root, or not a git repository
rem      3 = A REQUIRED FILE IS MISSING - no zip written
rem      4 = git could not resolve <since-commit>
rem      5 = no zip was produced
rem
rem  ONE EXIT POINT, as lock.bat and extract-verify.bat: every
rem  path sets RC and jumps to :end. lock.bat's header records
rem  why - exit /b from inside a nested block is lost under
rem  cmd /c script.bat, the form a double-click uses.
rem
rem  A MISSING FILE ABORTS, and it aborts BEFORE the zip is
rem  built. This is the same rule extract-verify.bat enforces on
rem  the way in, applied on the way out: a return package
rem  missing output.md is a unit that reported nothing, and
rem  shipping a zip anyway is exactly the failure work
rem  instructions 023 was written about. Nothing here is
rem  advisory.
rem
rem  It ships its own MANIFEST.txt, in the format
rem  extract-verify.bat reads. The sender declares what it sent
rem  in both directions, so a truncated return trip is
rem  detectable by whoever opens it rather than assumed away.
rem
rem  The unit number in the filename is READ from
rem  PROJECT_STATUS.md's WORK_INSTRUCTION field, not typed.
rem  CLAUDE_CODE.md 5 makes the sequence number mandatory and
rem  monotonic; a number that is typed is a number that can be
rem  typed wrong, and one delivery named -tonight- beside one
rem  named -456- is what that clause was written after. Where
rem  the field cannot be read the filename says unknown rather
rem  than inventing a number.
rem
rem  The date comes from PowerShell Get-Date. NEVER wmic - it is
rem  gone on current Windows and a dead call there silently
rem  named every backup the same thing once already. Never a
rem  composed timestamp: CLAUDE_CODE.md 11 records one written
rem  into the future twice.
rem
rem  Compress-Archive, NEVER tar: tools\get-files\README.md
rem  records that tar prefixes every entry ./ and Explorer
rem  renders the result as an empty folder.
rem
rem  No pause. A loop runs this one, not a double-click.
rem
rem  Amended  2026-08-29 for: work instructions 042 task 4 - the
rem           reload is run at packaging time and included, and is
rem           named in the manifest when it exists. Licensed by that
rem           task and by nothing else.
rem
rem  Generated 2026-08-27 for: work instructions 023 task 6
rem ============================================================

setlocal

set "RC=0"
set "SINCE=%~1"
set "REPO=%~2"

if "%SINCE%"=="" goto :usage
if "%REPO%"=="" set "REPO=C:\Source\HamLet"
if "%REPO:~-1%"=="\" set "REPO=%REPO:~0,-1%"

if not exist "%REPO%\" (
  echo ERROR: repo root not found: %REPO%
  set "RC=2"
  goto :end
)
if not exist "%REPO%\.git\" (
  echo ERROR: not a git repository: %REPO%
  set "RC=2"
  goto :end
)

set "OUTDIR=%USERPROFILE%\Downloads"
set "STAGE=%OUTDIR%\_arbiter_return_stage"

echo.
echo ============================================================
echo  return-package
echo    root  : %REPO%
echo    since : %SINCE%
echo ============================================================

rem --- the unit number, read rather than typed -----------------
set "UNIT=unknown"
for /f "usebackq tokens=2 delims=: " %%U in (`findstr /b /c:"WORK_INSTRUCTION:" "%REPO%\PROJECT_STATUS.md" 2^>nul`) do set "UNIT=%%U"

rem --- the date, measured --------------------------------------
set "TODAY="
for /f "usebackq delims=" %%D in (`powershell -NoProfile -Command "Get-Date -Format yyyy-MM-dd"`) do set "TODAY=%%D"
if defined TODAY goto :havedate
echo ERROR: could not read the clock. Refusing to compose a date.
set "RC=2"
goto :end

:havedate
set "OUT=%OUTDIR%\Hamlet-return-%UNIT%-%TODAY%.zip"

rem --- required files, checked BEFORE anything is built ---------
echo.
echo Required files:
set /a MISSING=0
call :needs "output.md"
call :needs "PROJECT_STATUS.md"
if %MISSING%==0 goto :collect
echo.
echo ABORTING: %MISSING% required file^(s^) missing. No zip written.
echo A return package without them reports nothing, and shipping one
echo anyway is the failure this tool exists to stop.
set "RC=3"
goto :end

:collect
if exist "%STAGE%" rd /s /q "%STAGE%"
if exist "%OUT%" del /q "%OUT%"
mkdir "%STAGE%"

rem --- the reload, RUN NOW rather than inherited ----------------
rem  042 task 4. The arbiter receives its reload and the unit's
rem  report in the same upload, so a reload the owner has to
rem  remember to run cannot be the reload that did not happen -
rem  the argument 041 made about outcome-append.bat, one artifact
rem  over.
rem
rem  RUN AT PACKAGING TIME. A reload copied from an earlier run is
rem  a picture, and acting on a picture rather than a measurement
rem  is the failure this whole unit exists to remove.
echo.
echo Running the reload, so the picture is measured now...
call "%~dp0reload.bat" "%REPO%" --out "%STAGE%\reload.txt" >nul
if not exist "%STAGE%\reload.txt" (
  echo   NOTE: the reload produced nothing. The package still goes, and
  echo   this line says the arbiter will be reading without one.
) else (
  echo   reload.txt written into the package.
)

copy /y "%REPO%\output.md" "%STAGE%\output.md" >nul
copy /y "%REPO%\PROJECT_STATUS.md" "%STAGE%\PROJECT_STATUS.md" >nul

rem --- the log for the unit -------------------------------------
echo.
echo Writing git-log.txt for %SINCE%..HEAD
pushd "%REPO%"
git rev-parse --verify "%SINCE%" >nul 2>&1
if errorlevel 1 goto :badsince
git log --stat "%SINCE%..HEAD" > "%STAGE%\git-log.txt" 2>nul
if errorlevel 1 goto :badsince
git rev-parse HEAD > "%STAGE%\HEAD.txt" 2>nul
popd

rem --- declare what is being sent -------------------------------
>"%STAGE%\MANIFEST.txt" echo rem  MANIFEST.txt - the return package for unit %UNIT%
>>"%STAGE%\MANIFEST.txt" echo MANIFEST.txt
>>"%STAGE%\MANIFEST.txt" echo output.md
>>"%STAGE%\MANIFEST.txt" echo PROJECT_STATUS.md
>>"%STAGE%\MANIFEST.txt" echo git-log.txt
>>"%STAGE%\MANIFEST.txt" echo HEAD.txt
if exist "%STAGE%\reload.txt" >>"%STAGE%\MANIFEST.txt" echo reload.txt

echo.
echo Zipping...
powershell -NoProfile -Command "Compress-Archive -Path '%STAGE%\*' -DestinationPath '%OUT%' -Force"

if exist "%OUT%" goto :zipped
echo.
echo ERROR: no zip was produced. The files are staged here:
echo   %STAGE%
set "RC=5"
goto :end

:zipped
rd /s /q "%STAGE%"
echo.
echo Created: %OUT%
for %%Z in ("%OUT%") do echo Zip size: %%~zZ bytes
set "RC=0"
goto :end

:badsince
popd
echo.
echo ERROR: git could not resolve %SINCE% in %REPO%
echo Pass the commit the unit began at.
if exist "%STAGE%" rd /s /q "%STAGE%"
set "RC=4"
goto :end

rem ============================================================
:needs
if exist "%REPO%\%~1" (
  echo   ok       %~1
  goto :eof
)
echo   MISSING  %~1
set /a MISSING+=1
goto :eof

rem ============================================================
:usage
echo.
echo   return-package.bat ^<since-commit^> [root]
echo.
echo   root defaults to C:\Source\HamLet
echo   0 zip written, 2 usage/bad root, 3 required file missing,
echo   4 bad since-commit, 5 no zip produced
echo.
set "RC=2"
goto :end

rem ============================================================
:end
echo.
echo return-package exit %RC%
endlocal & exit /b %RC%
