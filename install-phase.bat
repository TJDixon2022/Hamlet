@echo off
setlocal EnableExtensions

rem  install-phase.bat
rem  Generated 2026-09-04 for: hamlet-phase-sensitivity-2026-09-04.zip
rem
rem  Installs the new phase layer at the repository root.
rem  RUN THIS YOURSELF, BEFORE THE LOOP. It is not a work unit's job:
rem  the loop's shell has refused every write for the last several units
rem  and the phase must not depend on it.
rem
rem  Usage:  install-phase.bat [repo root]
rem  Default root: C:\Source\HamLet
rem
rem  It archives the closing phase, installs the new one, and refuses
rem  rather than guessing. Nothing is deleted; everything is moved.

set "REPO=%~1"
if "%REPO%"=="" set "REPO=C:\Source\HamLet"
if "%REPO:~-1%"=="\" set "REPO=%REPO:~0,-1%"

set "STAGE=%REPO%\docs\phase-sensitivity"
set "ARCHIVE=%REPO%\docs\phase-ft8"

echo.
echo   Repo root : %REPO%
echo   Staged in : %STAGE%
echo   Archive to: %ARCHIVE%
echo.

rem --- the project gate ---------------------------------------------
if not exist "%REPO%\" goto :nodir
if not exist "%REPO%\SHACK_FACTS.md" goto :wrongproject
if not exist "%REPO%\src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs" goto :wrongproject
if exist "%REPO%\CoreHMI.sln" goto :wrongproject
if exist "%REPO%\MURC.sln" goto :wrongproject
echo   Hamlet confirmed.

rem --- the staged files must all be present --------------------------
set "MISSING="
if not exist "%STAGE%\PHASE_PLAN.md"    set "MISSING=1"
if not exist "%STAGE%\PHASE_STATUS.md"  set "MISSING=1"
if not exist "%STAGE%\PHASE_OUTCOME.md" set "MISSING=1"
if defined MISSING goto :nostage

rem --- refuse to run twice -------------------------------------------
if exist "%ARCHIVE%\PHASE_OUTCOME.md" goto :already

rem --- the outcome file is the thing worth protecting -----------------
if not exist "%REPO%\PHASE_OUTCOME.md" goto :nooutcome
for %%F in ("%REPO%\PHASE_OUTCOME.md") do set "OUTSIZE=%%~zF"
echo   Closing phase's PHASE_OUTCOME.md is %OUTSIZE% bytes.
if %OUTSIZE% LSS 50000 goto :suspectoutcome

rem --- archive -------------------------------------------------------
if not exist "%REPO%\docs\" mkdir "%REPO%\docs"
mkdir "%ARCHIVE%" 2>nul
call :moveone "PHASE_OUTCOME.md"
call :moveone "PHASE_PLAN.md"
call :moveone "PHASE_STATUS.md"

rem --- verify the archive landed before installing anything -----------
if not exist "%ARCHIVE%\PHASE_OUTCOME.md" goto :archivefailed
for %%F in ("%ARCHIVE%\PHASE_OUTCOME.md") do set "NEWSIZE=%%~zF"
if not "%OUTSIZE%"=="%NEWSIZE%" goto :archivefailed
echo   Archived intact: %NEWSIZE% bytes.

rem --- install -------------------------------------------------------
copy /y "%STAGE%\PHASE_PLAN.md"    "%REPO%\PHASE_PLAN.md"    >nul
copy /y "%STAGE%\PHASE_STATUS.md"  "%REPO%\PHASE_STATUS.md"  >nul
copy /y "%STAGE%\PHASE_OUTCOME.md" "%REPO%\PHASE_OUTCOME.md" >nul

if not exist "%REPO%\PHASE_PLAN.md"    goto :installfailed
if not exist "%REPO%\PHASE_STATUS.md"  goto :installfailed
if not exist "%REPO%\PHASE_OUTCOME.md" goto :installfailed

echo.
echo   ================================================================
echo    PHASE INSTALLED
echo   ================================================================
echo    Archived : docs\phase-ft8\  (plan, status, outcome)
echo    Installed: PHASE_PLAN.md, PHASE_STATUS.md, PHASE_OUTCOME.md
echo.
echo    STILL TO DO, and unit 243 does it with its file tools:
echo      PROJECT_CARD.md  PHASE and PHASE_SET
echo      DECISIONS.md     the ruling that set them
echo.
echo    Commit this before launching the loop:
echo      git add -A ^&^& git commit -m "phase: sensitivity" ^&^& git push
echo   ================================================================
echo.
goto :end

rem ===================================================================
rem  subroutines and exits, after the main flow has ended
rem ===================================================================

:moveone
if exist "%REPO%\%~1" (
    move /y "%REPO%\%~1" "%ARCHIVE%\%~1" >nul
    echo   moved    %~1
) else (
    echo   ABSENT   %~1  ^(nothing to archive^)
)
exit /b 0

:nodir
echo   REFUSED: no such directory.
goto :end

:wrongproject
echo.
echo   REFUSED: this is not Hamlet.
echo   Expected SHACK_FACTS.md and src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs,
echo   and neither CoreHMI.sln nor MURC.sln.
echo   Nothing was changed.
goto :end

:nostage
echo.
echo   REFUSED: the staged phase files are not all in %STAGE%.
echo   Run the extraction gate first. Nothing was changed.
goto :end

:already
echo.
echo   REFUSED: docs\phase-ft8\PHASE_OUTCOME.md already exists,
echo   so this has already run. Nothing was changed.
goto :end

:nooutcome
echo.
echo   REFUSED: there is no PHASE_OUTCOME.md at the root to archive.
echo   That is not the tree this script expects. Nothing was changed.
goto :end

:suspectoutcome
echo.
echo   REFUSED: PHASE_OUTCOME.md is only %OUTSIZE% bytes.
echo   The closing phase's memory should be well over 100 KB.
echo   Check what that file is before archiving it. Nothing was changed.
goto :end

:archivefailed
echo.
echo   REFUSED: the archive copy did not match the original.
echo   The new phase was NOT installed. Check docs\phase-ft8\.
goto :end

:installfailed
echo.
echo   FAILED: the archive succeeded but the install did not.
echo   The old files are in docs\phase-ft8\. Copy the three files from
echo   %STAGE% to the root by hand.
goto :end

:end
endlocal
pause
