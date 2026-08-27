@echo off
rem ============================================================
rem  extract-gate.bat  -  gate the root, then extract
rem
rem  THIS FILE IS THE CANONICAL SCRIPT. CLAUDE_CODE.md section 5 requires
rem  it to be copied VERBATIM. Change only the four checks, the
rem  repository root, the zip name and the Generated line.
rem  The stamping step below is part of the canonical script and
rem  is not one of the four edits.
rem
rem  It ships OUTSIDE the zip because it runs before there is
rem  anything extracted. Put it in Downloads beside the zip and
rem  double-click it.
rem
rem  The four checks are the work instruction's own gate, run
rem  against your hands instead of against the session's
rem  attention. The gate inside WORK_INSTRUCTIONS.md cannot
rem  catch a zip extracted into the wrong repository, because by
rem  the time a session reads it the files have already landed.
rem
rem  Override the root:
rem      extract-gate.bat D:\Some\Other\Repo
rem  A trailing backslash on the argument is accepted.
rem
rem  After extracting, every file that came out of the zip is
rem  stamped with THIS machine's clock. Zip entries carry the
rem  timestamps of the machine that built them, and a delivery
rem  built on a machine running ahead lands with a future
rem  mtime. WORK_INSTRUCTIONS.md dated tomorrow is permanently
rem  newer than OUTPUT.md, which left the annunciator showing a
rem  delivered card whose review button never came alive.
rem  The clock is read once, here, on the machine that owns the
rem  tree - never composed and never taken from the zip.
rem
rem  Generated <date> for: <zip name>
rem ============================================================

setlocal enabledelayedexpansion

rem  <<< REPLACE: repo root, zip name, and the four checks >>>
set "REPO=%~1"
if "%REPO%"=="" set "REPO=C:\Source\ClaudeProjectStatus"

set "ZIP=%~dp0<project>-work-instructions-<nnn>-<date>.zip"
set "PROJECT=<name>"

set "MUST1=CLAUDE.md"
set "MUST2=<file>"
set "NOT1=<file>"
set "NOT2=<file>"
rem  <<< END REPLACE >>>

if "%REPO:~-1%"=="\" set "REPO=%REPO:~0,-1%"

set /a FAIL=0

echo.
echo Project : %PROJECT%
echo Root    : %REPO%
echo Zip     : %ZIP%
echo.

if not exist "%ZIP%" (
  echo GATE FAIL - the zip is not beside this script.
  echo Put both in the same folder and run it again.
  goto :done
)

if not exist "%REPO%\" (
  echo GATE FAIL - repository root not found.
  goto :done
)

echo Checking the root...
call :must    "%MUST1%"
call :must    "%MUST2%"
call :mustnot "%NOT1%"
call :mustnot "%NOT2%"

echo.
if not %FAIL%==0 (
  echo ============================================================
  echo GATE FAIL - %FAIL% of 4 checks failed.
  echo   %REPO%
  echo is not %PROJECT%. NOTHING WAS EXTRACTED.
  echo ============================================================
  goto :done
)

echo   %PROJECT% confirmed.
echo.
echo ONE WRITER AT A TIME. If a Claude Code session is running in
echo this tree, close this window now.
echo.
pause
echo.

echo Extracting...
powershell -NoProfile -Command "Expand-Archive -LiteralPath '%ZIP%' -DestinationPath '%REPO%' -Force"
if errorlevel 1 (
  echo.
  echo ERROR: extraction failed. Check the zip.
  goto :done
)

echo.
echo Stamping extracted files with this machine's clock...
powershell -NoProfile -Command "$now = Get-Date; Add-Type -AssemblyName System.IO.Compression.FileSystem; $z = [System.IO.Compression.ZipFile]::OpenRead('%ZIP%'); $n = 0; foreach ($e in $z.Entries) { if ($e.Name -ne '') { $f = Join-Path '%REPO%' $e.FullName; if (Test-Path -LiteralPath $f) { (Get-Item -LiteralPath $f).LastWriteTime = $now; $n++ } } }; $z.Dispose(); Write-Host ('  stamped ' + $n + ' files')"
if errorlevel 1 echo   WARNING: could not stamp the files. Check their dates by hand.

echo.
echo Extracted into %REPO%
echo Paste the prompt into a Claude Code session in that folder.

:done
echo.
pause
endlocal
exit /b

rem ============================================================
:must
if not exist "%REPO%\%~1" (
  echo   FAIL  MUST EXIST      %~1
  set /a FAIL+=1
  goto :eof
)
echo   ok    MUST EXIST      %~1
goto :eof

rem ============================================================
:mustnot
if exist "%REPO%\%~1" (
  echo   FAIL  MUST NOT EXIST  %~1
  set /a FAIL+=1
  goto :eof
)
echo   ok    MUST NOT EXIST  %~1
goto :eof
