@echo off
rem ============================================================
rem  extract-verify.bat  -  extraction that refuses
rem
rem  CLAUDE_CODE.md 1's MISSING line, enforced rather than
rem  printed. GROKBOT.md 4 task 6: extract a delivered zip over
rem  the root and verify every manifest file landed - A MISSING
rem  FILE ABORTS.
rem
rem      extract-verify.bat <zip> [root]
rem
rem  THE EXIT CODE IS THE POINT:
rem
rem      0 = extracted, and every manifest entry is present
rem      1 = THE GATE REFUSED - the zip was not touched
rem      2 = usage, zip not found, or bad root
rem      3 = THE LOCK IS HELD - nothing was extracted
rem      4 = extraction itself failed
rem      5 = no MANIFEST.txt after extraction
rem      6 = ONE OR MORE MANIFEST ENTRIES MISSING
rem
rem  Root defaults to C:\Source\HamLet. A trailing
rem  backslash on the argument is accepted, as in
rem  tools\get-files\get-files.template.bat.
rem
rem  ONE EXIT POINT. Every path sets RC and jumps to :end, which
rem  releases the lock if this run took it. lock.bat records why:
rem  exit /b from inside a nested block is lost under
rem  cmd /c script.bat, which is the form a double-click uses.
rem
rem  WHERE THE FOUR CHECKS COME FROM, and why not the command
rem  line. They are the block below, edited per delivery, exactly
rem  as get-files.template.bat's file-list block is. The gate
rem  exists to catch a delivery extracted over the WRONG ROOT. If
rem  the checks were arguments, the same hand that types the
rem  wrong root types the checks that match it, and the gate
rem  passes every time it is most needed - it would be verifying
rem  the invocation against itself. In the block they travel with
rem  the delivery the sender generated, and the receiver supplies
rem  only the root. The four are printed before they are
rem  resolved, so a block left stale from the last delivery is
rem  visible on screen rather than silently enforcing the wrong
rem  repository.
rem
rem  Expand-Archive via inline powershell -NoProfile -Command,
rem  NEVER tar: tools\get-files\README.md records that tar
rem  prefixes every entry ./ and Explorer renders the result as
rem  an empty folder. A .ps1 will not run on this machine -
rem  unsigned scripts are blocked by execution policy - and an
rem  inline -Command is not a script file.
rem
rem  ON THE MANIFEST. It is MANIFEST.txt inside the zip: one
rem  relative path per line, backslashes, blank lines and rem
rem  lines ignored. The sender declares what it sent, so a
rem  truncated zip is DETECTED BY THE RECEIVER rather than
rem  assumed away. It is read after extraction, from the root,
rem  so a zip that lost the manifest itself aborts at 5 rather
rem  than reporting nothing missing out of nothing checked.
rem  MANIFEST.txt is left at the root afterwards on purpose: it
rem  is the receipt for what this delivery claimed to contain.
rem
rem  Generated 2026-08-27 for: work instructions 023 task 4
rem ============================================================

setlocal

set "RC=0"
set "TOOK="

rem  <<< THE FOUR CHECKS - REPLACE THIS BLOCK EVERY DELIVERY >>>
rem  Taken verbatim from the work instruction's gate block.
set "MUST_EXIST_1=app\PROJECT_ANNUNCIATOR.html"
set "MUST_EXIST_2=docs\for-each-project\ANNUNCIATOR.md"
set "MUST_NOT_EXIST_1=WORK_ORDER.md"
set "MUST_NOT_EXIST_2=SHACK_FACTS.md"
set "GATE_NAME=Hamlet"

set "ZIP=%~1"
set "REPO=%~2"

if "%ZIP%"=="" goto :usage
if "%REPO%"=="" set "REPO=C:\Source\HamLet"
if "%REPO:~-1%"=="\" set "REPO=%REPO:~0,-1%"

if not exist "%ZIP%" (
  echo ERROR: zip not found: %ZIP%
  set "RC=2"
  goto :end
)
if not exist "%REPO%\" (
  echo ERROR: repo root not found: %REPO%
  set "RC=2"
  goto :end
)

echo.
echo ============================================================
echo  extract-verify
echo    zip  : %ZIP%
echo    root : %REPO%
echo    gate : %GATE_NAME%
echo ============================================================

rem ============================================================
rem  THE GATE - resolved before the zip is touched
rem ============================================================
echo.
echo Gate checks, against the root, before anything is extracted:
echo   MUST EXIST      %MUST_EXIST_1%
echo   MUST EXIST      %MUST_EXIST_2%
echo   MUST NOT EXIST  %MUST_NOT_EXIST_1%
echo   MUST NOT EXIST  %MUST_NOT_EXIST_2%
echo.

set /a GATEFAIL=0
call :needs    "%MUST_EXIST_1%"
call :needs    "%MUST_EXIST_2%"
call :forbids  "%MUST_NOT_EXIST_1%"
call :forbids  "%MUST_NOT_EXIST_2%"

if %GATEFAIL%==0 goto :gateok
echo.
echo REFUSED: %GATEFAIL% of 4 gate checks failed against %REPO%
echo This is not the repository this delivery is for.
echo The zip was NOT touched.
set "RC=1"
goto :end

:gateok
echo.
echo Gate: 4 of 4 hold. %GATE_NAME% confirmed.

rem ============================================================
rem  THE LOCK - one writer at a time, CLAUDE_CODE.md 5
rem ============================================================
echo.
echo Taking the session lock...
call "%~dp0lock.bat" take "%REPO%"
if not errorlevel 1 goto :gotlock
echo.
echo REFUSED: could not take the session lock. Nothing was extracted.
echo A second writer in one tree is the failure this lock exists to stop.
set "RC=3"
goto :end

:gotlock
set "TOOK=1"

rem ============================================================
rem  EXTRACT
rem ============================================================
echo.
echo Extracting over the root...
powershell -NoProfile -Command "try{Expand-Archive -LiteralPath '%ZIP%' -DestinationPath '%REPO%' -Force; exit 0}catch{Write-Host $_.Exception.Message; exit 1}"
if not errorlevel 1 goto :extracted
echo.
echo ERROR: extraction failed. The tree may be half-written - check it by hand.
set "RC=4"
goto :end

:extracted
set "MAN=%REPO%\MANIFEST.txt"
if exist "%MAN%" goto :verify
echo.
echo ERROR: no MANIFEST.txt at the root after extraction.
echo Nothing can be verified. A delivery with no declaration is not verified
echo by finding nothing wrong with it.
set "RC=5"
goto :end

rem ============================================================
rem  VERIFY - one line per manifest entry
rem ============================================================
:verify
echo.
echo Verifying against MANIFEST.txt:
echo.
set /a MISSING=0
set /a PRESENT=0
for /f "usebackq delims=" %%L in ("%MAN%") do call :checkline "%%L"

echo.
echo   present : %PRESENT%
echo   missing : %MISSING%
if %MISSING%==0 goto :allthere
echo.
echo ABORTING: %MISSING% manifest entries did not land.
echo A MISSING line is loud on purpose and this one is not advisory.
set "RC=6"
goto :end

:allthere
echo.
echo All %PRESENT% manifest entries are present. Delivery verified.
set "RC=0"
goto :end

rem ============================================================
:needs
if exist "%REPO%\%~1" (
  echo   ok        MUST EXIST      %~1
  goto :eof
)
if exist "%REPO%\%~1\" (
  echo   ok        MUST EXIST      %~1
  goto :eof
)
echo   FAILED    MUST EXIST      %~1  - not found
set /a GATEFAIL+=1
goto :eof

:forbids
if not exist "%REPO%\%~1" (
  echo   ok        MUST NOT EXIST  %~1
  goto :eof
)
echo   FAILED    MUST NOT EXIST  %~1  - it is there
set /a GATEFAIL+=1
goto :eof

rem ============================================================
:checkline
set "LINE=%~1"
if not defined LINE goto :eof
if /i "%LINE:~0,4%"=="rem " goto :eof
if /i "%LINE%"=="rem" goto :eof
if exist "%REPO%\%LINE%" (
  echo   ok       %LINE%
  set /a PRESENT+=1
  goto :eof
)
if exist "%REPO%\%LINE%\" (
  echo   ok       %LINE%
  set /a PRESENT+=1
  goto :eof
)
echo   MISSING  %LINE%
set /a MISSING+=1
goto :eof

rem ============================================================
:usage
echo.
echo   extract-verify.bat ^<zip^> [root]
echo.
echo   root defaults to C:\Source\HamLet
echo   0 verified, 1 gate refused, 2 usage, 3 lock held,
echo   4 extraction failed, 5 no manifest, 6 entries missing
echo.
set "RC=2"
goto :end

rem ============================================================
:end
if not "%TOOK%"=="1" goto :endout
echo.
echo Releasing the session lock...
call "%~dp0lock.bat" release "%REPO%" >nul
:endout
rem  THE FOUR CHECKS THIS RUN USED, REPORTED BESIDE THE EXIT CODE.
rem  They are also printed before the gate resolves, at the top of the run. They
rem  are repeated here because the top of a long run scrolls away and the exit
rem  code is what a reader and a loop both look at.
rem
rem  NOTHING ELSE COMPARES THE TWO COPIES. This script keeps its own four and
rem  does NOT parse them out of the delivered WORK_INSTRUCTIONS.md: two
rem  independent checks that must agree is what CLAUDE_CODE.md 5 already
rem  describes when it says the same gate runs twice, once against the owner's
rem  hands before extraction and once against the session's attention after.
rem  Parsing one out of the other collapses them into one check wearing two
rem  coats, and a reformatted gate block would silently disable it. PRINTING
rem  THEM IS WHAT MAKES TWO COPIES SAFE - a divergence is visible in this output
rem  rather than assumed away. CPS-DEC-066.
echo.
echo Gate checks used by this run ^(this script's own copy, not parsed from the
echo delivered instruction - compare them against its gate block by eye^):
echo   MUST EXIST      %MUST_EXIST_1%
echo   MUST EXIST      %MUST_EXIST_2%
echo   MUST NOT EXIST  %MUST_NOT_EXIST_1%
echo   MUST NOT EXIST  %MUST_NOT_EXIST_2%
echo   gate name       %GATE_NAME%
echo.
echo extract-verify exit %RC%
endlocal & exit /b %RC%
