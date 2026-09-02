@echo off
rem ============================================================
rem  rules-at.bat  -  what RULES_AT should say, measured
rem
rem  Prints the highest ruling id in CLAUDE.md section 1 and its
rem  date, in the exact form PROJECT_STATUS.md's RULES_AT field
rem  takes, and compares it against what that file currently says.
rem
rem      rules-at.bat [root]
rem
rem      0  RULES_AT matches the highest ruling in force
rem      1  RULES_AT IS BEHIND (or ahead of) the log - the value
rem         to write is printed above the verdict
rem      2  usage, bad root, or neither file could be read
rem
rem  WHY THIS EXISTS. STATUS_PROTOCOL.md section 3 defines
rem  RULES_AT as "Newest ruling id and date" - the NEWEST, not the
rem  one the writing session happened to record. Its stated reason
rem  is that the owner can see at a glance when a chat session is
rem  running on stale rules; a field carrying the writing session's
rem  own id would read current every time and could never show
rem  that.
rem
rem  ON 2026-08-28 IT DID EXACTLY THAT. Unit 037 set the field to
rem  the one ruling it had recorded, CPS-DEC-058, moving it
rem  BACKWARDS past nine rulings the other thread had already
rem  written. A web session then read the field, concluded its
rem  picture was current, and wrote a work instruction against
rem  superseded rulings: six of that unit's claims did not match
rem  the tree - a red suite that CPS-DEC-064 had fixed, a stale
rem  BASELINE_RED that CPS-DEC-065 had ruled, and an open question
rem  that CPS-DEC-066 had answered.
rem
rem  So the value is MEASURED rather than remembered. A session
rem  runs this before it writes its status file and copies what it
rem  prints. That is the whole mechanism: the number nobody can
rem  get wrong is the one nobody types.
rem
rem  HIGHEST BY ID, NOT BY POSITION. CLAUDE.md section 1 is
rem  newest-first by convention, but the convention is what failed
rem  here, so this reads every row and takes the largest number
rem  rather than trusting the top row to be the newest.
rem
rem  Batch, not PowerShell: a .ps1 will not run on this machine,
rem  unsigned scripts are blocked by execution policy. The inline
rem  powershell -NoProfile -Command below is not a script file and
rem  is used only where cmd genuinely cannot do the job - reading
rem  the largest id out of a table.
rem
rem  Generated 2026-08-28 for: work instructions 038 task 2
rem ============================================================

setlocal

rem  THIS SCRIPT'S OWN DIRECTORY, CAPTURED BEFORE ANY shift.
rem  PHASE_UPLIFT.md section 12: `shift` moves %0 along with the numbered
rem  arguments, so afterwards `%~dp0` resolves to the CALLER's directory
rem  and the sibling script goes missing - and this script would then
rem  report a missing field about a file that has one, which is the exact
rem  fault readkey.bat was introduced to stop.
set "HERE=%~dp0"


set "RC=0"
set "REPO=%~1"
if "%REPO%"=="" set "REPO=C:\Source\HamLet"
if "%REPO:~-1%"=="\" set "REPO=%REPO:~0,-1%"

if not exist "%REPO%\" (
  echo ERROR: repo root not found: %REPO%
  set "RC=2"
  goto :end
)

set "LOG=%REPO%\CLAUDE.md"
set "STATUS=%REPO%\PROJECT_STATUS.md"

if not exist "%LOG%" (
  echo ERROR: no CLAUDE.md at %REPO%
  set "RC=2"
  goto :end
)
if not exist "%STATUS%" (
  echo ERROR: no PROJECT_STATUS.md at %REPO%
  set "RC=2"
  goto :end
)

echo.
echo ============================================================
echo  rules-at
echo    log    : %LOG%
echo    status : %STATUS%
echo ============================================================

rem --- the highest id in the log, and the date on its row -------
set "HIGH="
set "HIGHDATE="
for /f "usebackq tokens=1,2 delims= " %%A in (`powershell -NoProfile -Command "$rows = Select-String -Path '%LOG%' -Pattern '\| ([A-Z]+-DEC-\d+) \|$' -AllMatches; $best=$null; foreach($r in $rows){ $id=$r.Matches[0].Groups[1].Value; $n=[int]($id -split '-')[-1]; if(-not $best -or $n -gt $best.N){ $d=''; if($r.Line -match '^\| (\d{4}-\d{2}-\d{2}) \|'){$d=$Matches[1]}; $best=[pscustomobject]@{N=$n;Id=$id;D=$d} } }; if($best){ $best.Id + ' ' + $best.D }"`) do (
  set "HIGH=%%A"
  set "HIGHDATE=%%B"
)

if not defined HIGH (
  echo ERROR: no ruling rows found in CLAUDE.md section 1.
  echo Nothing to measure against - refusing to guess a value.
  set "RC=2"
  goto :end
)

rem --- what the status file currently says -----------------------
set "CURRENT="
rem  READ THROUGH readkey.bat, NOT findstr. PHASE_UPLIFT.md section 12:
rem  findstr /b finds only the first field in a CR-only file and fails on
rem  the first field in a BOM'd one, which between them covers every line.
call "%HERE%readkey.bat" "%STATUS%" "RULES_AT" CURRENT
if defined CURRENT call :trim "%CURRENT%"

set "WANT=%HIGH% (%HIGHDATE%)"

echo.
echo   highest ruling in CLAUDE.md : %HIGH%
echo   the date on its row         : %HIGHDATE%
echo.
echo   RULES_AT should read        : %WANT%
echo   RULES_AT currently reads    : %CURRENT%
echo.

if "%CURRENT%"=="%WANT%" (
  echo   MATCHES. RULES_AT is at the newest ruling in force.
  set "RC=0"
  goto :end
)

echo   BEHIND. RULES_AT does not name the newest ruling in force.
echo.
echo   Write this into PROJECT_STATUS.md:
echo.
echo       RULES_AT: %WANT%
echo.
echo   STATUS_PROTOCOL.md section 3: "Newest ruling id and date."
echo   Not the ruling this session recorded - the newest one there
echo   is. A session that writes its own id makes this field read
echo   current for ever, which is what it exists to prevent.
set "RC=1"
goto :end

rem ============================================================
:trim
set "CURRENT=%~1"
if "%CURRENT:~0,1%"==" " set "CURRENT=%CURRENT:~1%"
if "%CURRENT:~0,1%"==" " goto :trim
goto :eof

rem ============================================================
:end
echo.
echo rules-at exit %RC%
endlocal & exit /b %RC%
