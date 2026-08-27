@echo off
rem ============================================================
rem  get-files.bat  -  collect the files Claude asked for
rem
rem  THIS FILE IS THE CANONICAL SCRIPT. CLAUDE.md 9.4 requires it
rem  to be copied VERBATIM. Change only the marked file-list block
rem  and the Generated line below.
rem  Do not rewrite it from memory and do not substitute another
rem  mechanism - the subroutines took several rounds to get right
rem  on cmd.exe and Claude cannot execute Windows batch to test a
rem  replacement.
rem
rem  Put the generated copy in your Downloads folder and
rem  double-click it, or run it from a prompt. It copies the files
rem  listed below out of the repo, preserving their relative
rem  paths, and zips them into Downloads.
rem
rem  Repo root defaults to C:\Source\ClaudeProjectStatus.
rem  Override it:
rem      get-files.bat D:\Some\Other\Repo
rem  A trailing backslash on the argument is accepted.
rem
rem  The excluded-folder list below is kept identical to the one
rem  in tools\repo-listing\get-listing.bat. They must agree, or
rem  verifying the zip against repo_listing.txt compares two
rem  different views of the tree and reports differences that are
rem  not real.
rem
rem  Generated <date> for: <the row this pull is for>
rem ============================================================

setlocal

set "REPO=%~1"
if "%REPO%"=="" set "REPO=C:\Source\ClaudeProjectStatus"

rem --- normalise: drop a trailing backslash -------------------------
rem  Drag-and-drop and tab completion both produce one.
if "%REPO:~-1%"=="\" set "REPO=%REPO:~0,-1%"

set "OUTDIR=%USERPROFILE%\Downloads"
set "STAGE=%OUTDIR%\_claude_stage"
set "OUT=%OUTDIR%\for_claude.zip"

rem --- excluded folders, identical to get-listing.bat ---------------
set "XD=/XD node_modules bin obj .git .vs packages TestResults coverage dist graphify-out"

set /a MISSING=0
set /a FOUND=0

echo.
echo Repo root : %REPO%
echo Output    : %OUT%
echo.

if not exist "%REPO%\" (
  echo ERROR: repo root not found: %REPO%
  echo Pass the correct path as the first argument.
  goto :done
)

rem --- clean any previous run -------------------------------------
if exist "%STAGE%" rd /s /q "%STAGE%"
if exist "%OUT%" del /q "%OUT%"
mkdir "%STAGE%"

rem --- the files ---------------------------------------------------
rem  <<< REPLACE THIS BLOCK EVERY TIME >>>
rem  :adddir for a folder (recursive), :add for a single file.
rem  Paths are RELATIVE to the repo root and use BACKSLASHES.
call :add    "CLAUDE.md"
call :add    "OPEN_ISSUES.md"
call :add    "DECISIONS.md"
call :add    ".gitignore"

rem --- zip ---------------------------------------------------------
echo.
if %FOUND%==0 (
  echo Nothing was copied. Not creating a zip.
  goto :done
)

echo Zipping...
powershell -NoProfile -Command "Compress-Archive -Path '%STAGE%\*' -DestinationPath '%OUT%' -Force"

if not exist "%OUT%" (
  echo.
  echo ERROR: no zip was produced. The files are staged here:
  echo   %STAGE%
  echo Right-click that folder, Send to ^> Compressed folder, and upload it.
  goto :done
)

rd /s /q "%STAGE%"

echo.
echo Copied  : %FOUND%
echo Missing : %MISSING%
for %%Z in ("%OUT%") do echo Zip size: %%~zZ bytes
echo.
echo Created: %OUT%
echo Open it in Explorer and confirm you can SEE the files before uploading.
if not %MISSING%==0 echo NOTE: some files were missing - see the list above.

:done
echo.
pause
endlocal
exit /b

rem ============================================================
:adddir
set "REL=%~1"
if not exist "%REPO%\%REL%\" (
  echo   MISSING  %REL%\ ^(folder^)
  set /a MISSING+=1
  goto :eof
)
echo   dir      %REL%\
robocopy "%REPO%\%REL%" "%STAGE%\%REL%" /E /NJH /NJS /NDL /NFL /NP %XD% >nul
rem robocopy exit codes below 8 are success; 8 and above are real failures.
if errorlevel 8 (
  echo   FAILED   %REL%\
  set /a MISSING+=1
  goto :eof
)
for /f %%C in ('dir /b /s /a-d "%STAGE%\%REL%" 2^>nul ^| find /c /v ""') do set /a FOUND+=%%C
goto :eof

rem ============================================================
:add
set "REL=%~1"
if not exist "%REPO%\%REL%" (
  echo   MISSING  %REL%
  set /a MISSING+=1
  goto :eof
)
for %%F in ("%STAGE%\%REL%") do (
  if not exist "%%~dpF" mkdir "%%~dpF"
)
copy /y "%REPO%\%REL%" "%STAGE%\%REL%" >nul
if errorlevel 1 (
  echo   FAILED   %REL%
  set /a MISSING+=1
  goto :eof
)
echo   ok       %REL%
set /a FOUND+=1
goto :eof
