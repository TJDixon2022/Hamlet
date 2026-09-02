@echo off
rem ============================================================
rem  layer-check.bat  -  what does this project's copy of the
rem                      layer actually DO?
rem
rem      layer-check.bat [root]
rem
rem      0  every capability PRESENT
rem      1  one or more ABSENT
rem      2  the root is wrong, or nothing could be read
rem
rem  THE OWNER'S RULING, 2026-09-01: A PROJECT MUST BE ABLE TO
rem  TELL IT IS BEHIND. The question "is my copy current" becomes
rem  a command. The check names the gap; PHASE_UPLIFT.md carries
rem  the fix for each gap; the project applies it. NOTHING IN
rem  THAT LOOP REQUIRES THE REPOSITORY THIS SHIPPED FROM.
rem
rem  WHY IT REPORTS CAPABILITIES AND NOT A VERSION. A version says
rem  a file CHANGED, not what it can DO. The peer project's
rem  run-phase.bat had been edited many times and was missing
rem  exactly one capability; a version number would have said
rem  "different" and told nobody which. Measured 2026-09-01:
rem  eight work instructions ran on that launcher and its card
rem  read `step 2 of 7` on a phase further along - plausible, and
rem  stale, and nothing said so.
rem
rem  THREE VERDICTS AND NO OTHERS.
rem    PRESENT      the detection matched
rem    ABSENT       the file was read and the detection did not
rem                 match - a real gap, with a fix to apply
rem    UNCHECKABLE  the file it lives in is not in this project,
rem                 so nothing was read and nothing is claimed
rem
rem  UNCHECKABLE IS NOT A FAILURE AND IS NEVER SILENTLY UPGRADED.
rem  It is the honest answer for a monitored project that has no
rem  app\PROJECT_ANNUNCIATOR.html: the panel's capabilities are
rem  not missing there, they are not that project's to have.
rem  Reporting five false gaps on every peer is how a checker
rem  gets switched off - 059 measured its own duplicate-key check
rem  calling five legitimate STEP: lines a fault.
rem
rem  IT READS AND REPAIRS NOTHING, and it NEVER EXECUTES THE
rem  LAUNCHER. A label can be present while the call site is
rem  wrong - which is exactly this peer's status-check case - and
rem  that limit is stated in the output rather than hidden.
rem
rem  IT DEPENDS ON NO SIBLING SCRIPT. A project running this may
rem  be missing readkey.bat, which is one of the things it
rem  checks for. Everything here is inline.
rem
rem  ONE EXIT POINT, as lock.bat and validate-output.bat.
rem  %~dp0 is captured before any shift.
rem ============================================================
setlocal EnableExtensions
set "LCHERE=%~dp0"

rem ============================================================
rem  HOW CURRENT THIS SCRIPT IS. Task 3.
rem  THIS IS THE FAILURE MODE OF THIS SCRIPT: it goes stale and
rem  reports green on a project that is behind. It cannot detect
rem  that on its own - see the limit printed below.
rem ============================================================
set "LIST_UPDATED=2026-09-01"
set "LIST_UNIT=060"

set "RC=0"
set /a NPRESENT=0
set /a NABSENT=0
set /a NUNCHECK=0

set "ROOT=%~1"
if "%ROOT%"=="" set "ROOT=%CD%"
if "%ROOT:~-1%"=="\" set "ROOT=%ROOT:~0,-1%"

if not exist "%ROOT%\" (
  echo ERROR: no such directory: %ROOT%
  set "RC=2"
  goto :end
)

echo.
echo ============================================================
echo  layer-check
echo    root          : %ROOT%
echo    capability list: updated %LIST_UPDATED%, written by unit %LIST_UNIT%
echo ============================================================

rem  IT REFUSES A DIRECTORY THAT IS NOT A REPOSITORY OF THIS LAYER
rem  rather than reporting sixteen ABSENTs about a folder that was
rem  never meant to have any of them. CLAUDE.md is the one file
rem  every project of this layer has.
if not exist "%ROOT%\CLAUDE.md" (
  echo.
  echo   NOT A PROJECT OF THIS LAYER - no CLAUDE.md at %ROOT%
  echo   Nothing was checked. Sixteen ABSENT lines about a folder that
  echo   was never meant to have any of them would be noise, not a finding.
  set "RC=2"
  goto :end
)

echo.
echo   CAPABILITY                        VERDICT      WHERE THE FIX IS
echo   --------------------------------  -----------  --------------------

rem ============================================================
rem  THE TABLE. ONE ROW PER CAPABILITY. Adding one next month is
rem  one line here and nothing else.
rem
rem    call :cap "<name>" "<file, relative to root>" "<pattern>" "<where>"
rem
rem  A pattern of * means "the file itself is the capability".
rem
rem  A FIFTH FIELD OF `req` MEANS: IF THE HOST FILE IS ABSENT, THAT IS
rem  ITSELF THE GAP - report ABSENT, not UNCHECKABLE. It is set on
rem  .gitattributes and nowhere else: EVERY project of this layer should
rem  pin *.bat to CRLF, so having no .gitattributes at all is not `nothing
rem  could be read`, it is `this project does not do that`.
rem
rem  Without the flag the default is UNCHECKABLE, which is right for the
rem  panel - a monitored project is not MISSING app\PROJECT_ANNUNCIATOR.html,
rem  it was never meant to have one - and right for the launcher-hosted rows,
rem  where `launcher present` already names the real gap and nine more
rem  ABSENTs would be the same finding nine times.
rem
rem  Found by running: removing .gitattributes reported UNCHECKABLE, which
rem  read as `not your problem` about a real one.
rem  Anything else is a case-sensitive regex read through
rem  PowerShell - never findstr, which 058 measured missing every
rem  field below line 1 in a CR-only file and the field ON line 1
rem  in a BOM'd one.
rem ============================================================

call :cap "launcher present"         "tools\arbiter\run-phase.bat"   "*"                      "PHASE_UPLIFT.md 4"
call :cap "step-state writer"        "tools\arbiter\run-phase.bat"   "^:phasesteps"           "PHASE_UPLIFT.md 5"
call :cap "heartbeat write"          "tools\arbiter\run-phase.bat"   "^:heartbeat$"           "PHASE_UPLIFT.md 6"
call :cap "beat cleared at halt"     "tools\arbiter\run-phase.bat"   "^:heartbeatclear"       "PHASE_UPLIFT.md 6"
call :cap "scratch cleared"          "tools\arbiter\run-phase.bat"   "^:scratch"              "PHASE_UPLIFT.md 7"
call :cap "state judge"              "tools\arbiter\run-phase.bat"   "^:judgestate"           "PHASE_UPLIFT.md 8"
call :cap "section-4 judge"          "tools\arbiter\run-phase.bat"   "^:judges4"              "PHASE_UPLIFT.md 8"
call :cap "ADVANCES required"        "tools\arbiter\run-phase.bat"   "^:noadvances"           "PHASE_UPLIFT.md 9"
call :cap "step-list form matched"   "tools\arbiter\run-phase.bat"   "CaseSensitive"          "PHASE_UPLIFT.md 10"
call :cap "status-check wired"       "tools\arbiter\run-phase.bat"   "status-check\.bat"      "PHASE_UPLIFT.md 11"
call :cap "status-check script"      "tools\arbiter\status-check.bat" "*"                     "PHASE_UPLIFT.md 11"
call :cap "readkey script"           "tools\arbiter\readkey.bat"     "*"                      "PHASE_UPLIFT.md 12"
call :cap "watchdog reads tolerant"  "tools\arbiter\watchdog.bat"    "readkey\.bat"           "PHASE_UPLIFT.md 12"
call :cap "reload present"           "tools\arbiter\reload.bat"      "*"                      "PHASE_UPLIFT.md 3"
call :cap "CRLF pinned for .bat"     ".gitattributes"                "eol=crlf"               "PHASE_UPLIFT.md 13" req
call :cap "panel per-field degrade"  "app\PROJECT_ANNUNCIATOR.html"  "function deriveStatus"  "PHASE_UPLIFT.md 14"
call :cap "panel transport tolerant" "app\PROJECT_ANNUNCIATOR.html"  "function normalizeText" "PHASE_UPLIFT.md 14"
call :cap "panel no blink when done" "app\PROJECT_ANNUNCIATOR.html"  "blink: allDone"         "PHASE_UPLIFT.md 15"
call :cap "panel loop reading"       "app\PROJECT_ANNUNCIATOR.html"  "function loopBeatView"  "PHASE_UPLIFT.md 15"
call :cap "panel phase parser"       "app\PROJECT_ANNUNCIATOR.html"  "function parsePhaseStatus" "PHASE_UPLIFT.md 14"

set /a NTOTAL=%NPRESENT%+%NABSENT%+%NUNCHECK%

echo.
echo   %NTOTAL% capabilities checked: %NPRESENT% present, %NABSENT% absent, %NUNCHECK% uncheckable.

rem ============================================================
rem  TASK 3 - THE LIMIT, STATED AS A LIMIT AND NOT AS COMFORT.
rem ============================================================
rem ============================================================
rem  TASK 3 - CAN IT DETECT ITS OWN STALENESS? PARTLY, AND ONLY
rem  FROM WHAT IS ALREADY IN THIS PROJECT.
rem
rem  WHAT I CHOSE: it compares the date its capability list was
rem  written against THE NEWEST DATED RULING IN THIS PROJECT'S OWN
rem  CLAUDE.md section 1. Rulings are what add capabilities, so a
rem  project carrying a ruling NEWER than the list is a project
rem  whose list may not know about it.
rem
rem  WHY THAT AND NOT A VERSION COMPARISON: nothing in this loop
rem  may reach another repository. The ruling is explicit and it is
rem  the whole point - a project must be able to run this ALONE.
rem  Its own CLAUDE.md is already here.
rem
rem  IT IS A HINT AND IS WORDED AS ONE. A ruling can be recorded
rem  without adding a capability, so a newer ruling proves nothing
rem  on its own - it says LOOK, not YOU ARE BEHIND. Stating it as a
rem  verdict would be the guess this script refuses everywhere else.
rem  And it is one-directional: it cannot see a capability added
rem  with no ruling at all, which is why the limit below is printed
rem  every run whatever this comparison says.
rem ============================================================
set "NEWESTRULE="
for /f "usebackq delims=" %%R in (`powershell -NoProfile -Command "$p='%ROOT%\CLAUDE.md'; if(-not (Test-Path -LiteralPath $p)){ exit }; $d=@(Select-String -Path $p -Pattern '^\| *([0-9]{4}-[0-9]{2}-[0-9]{2}) *\|' -AllMatches | ForEach-Object { $_.Matches[0].Groups[1].Value }); if($d.Count -gt 0){ $srt=@($d | Sort-Object); $srt[$srt.Count-1] }"`) do set "NEWESTRULE=%%R"
echo.
if not defined NEWESTRULE (
  echo   THIS PROJECT'S CLAUDE.md CARRIES NO DATED RULING ROWS, so this script
  echo   cannot compare its list against anything. That is not a fault; it is
  echo   one fewer signal.
) else if "%NEWESTRULE%" GTR "%LIST_UPDATED%" (
  echo   LOOK: this project's newest ruling is %NEWESTRULE% and this capability
  echo   list was written %LIST_UPDATED%. Rulings are what add capabilities, so
  echo   this list MAY not know about one. It is a hint and not a verdict - a
  echo   ruling can be recorded without adding a capability. Check the newer
  echo   rulings against the table above.
) else (
  echo   This project's newest ruling is %NEWESTRULE% and this list was written
  echo   %LIST_UPDATED%, so nothing in this project's own record is newer than
  echo   the list. THIS IS NOT PROOF THE LIST IS CURRENT - see the limit below.
)

echo.
echo   THIS LIST WAS LAST UPDATED %LIST_UPDATED% AND KNOWS %NTOTAL% CAPABILITIES.
echo   If the layer has gained one since, THIS SCRIPT CANNOT KNOW THAT and will
echo   report green while you are behind. A copy of this script is only as
echo   current as the day it was copied. Compare %LIST_UPDATED% against the
echo   newest ruling in your CLAUDE.md; if that is later, get a newer copy.
echo   NOTHING HERE REACHES ANOTHER REPOSITORY TO FIND OUT - by design.
echo.
echo   It reads files. It does not run the launcher, so a label present with a
echo   wrong call site reads PRESENT here. UNCHECKABLE means the file it lives
echo   in is not in this project - not that anything is wrong.

if %NABSENT% GTR 0 (
  echo.
  echo   ONE OR MORE CAPABILITIES ARE ABSENT. Each line above names where the
  echo   fix is written. Apply them in the order listed.
  set "RC=1"
)
goto :end

rem ============================================================
rem  One capability. %~1 name, %~2 file, %~3 pattern, %~4 where.
rem ============================================================
:cap
set "CAPNAME=%~1"
set "CAPFILE=%~2"
set "CAPPAT=%~3"
set "CAPWHERE=%~4"
set "CAPREQ=%~5"
set "CAPV="

if not exist "%ROOT%\%CAPFILE%" (
  if "%CAPPAT%"=="*" (
    rem  THE FILE IS THE CAPABILITY, so its absence is a real gap.
    set "CAPV=ABSENT"
  ) else if /i "%CAPREQ%"=="req" (
    rem  THE HOST FILE IS ABSENT AND EVERY PROJECT SHOULD HAVE IT, so its
    rem  absence IS the gap rather than a thing that could not be read.
    set "CAPV=ABSENT"
  ) else (
    rem  NOTHING WAS READ, SO NOTHING IS CLAIMED. This is the honest
    rem  verdict for a monitored project with no panel, and it is never
    rem  upgraded to PRESENT.
    set "CAPV=UNCHECKABLE"
  )
  goto :capsay
)
if "%CAPPAT%"=="*" set "CAPV=PRESENT" & goto :capsay

rem  READ THROUGH POWERSHELL, CASE-SENSITIVE, AND TOLERANT OF EVERY
rem  TRANSPORT - a project being checked may be exactly the project
rem  whose files are CR-only or carry a BOM, and a checker that
rem  reported ABSENT about a capability that is there would send
rem  someone to fix what is not broken. 058.
for /f "usebackq delims=" %%H in (`powershell -NoProfile -Command "$p='%ROOT%\%CAPFILE%'; $b=[System.IO.File]::ReadAllBytes($p); $s=[System.Text.Encoding]::UTF8.GetString($b); if($s.Length -ge 1 -and [int][char]$s[0] -eq 65279){ $s=$s.Substring(1) }; $CRc=[string][char]13; $LFc=[string][char]10; $lines=[regex]::Split($s,$CRc+$LFc+'|'+$LFc+'|'+$CRc); $n=0; foreach($ln in $lines){ if([regex]::IsMatch($ln,'%CAPPAT%')){ $n++ } }; if($n -gt 0){ 'yes' } else { 'no' }"`) do set "CAPHIT=%%H"
if "%CAPHIT%"=="yes" (set "CAPV=PRESENT") else (set "CAPV=ABSENT")

:capsay
if "%CAPV%"=="PRESENT"     set /a NPRESENT+=1
if "%CAPV%"=="ABSENT"      set /a NABSENT+=1
if "%CAPV%"=="UNCHECKABLE" set /a NUNCHECK+=1
rem  AN ABSENT LINE CARRIES ITS FIX. A project reading ABSENT must know
rem  where to look without asking anyone - that is the whole ruling.
rem  PADDED SO THE TABLE LINES UP. A verdict column a reader has to hunt
rem  for down a ragged edge is one that gets skimmed.
set "CAPPAD=%CAPNAME%                                  "
set "CAPPAD=%CAPPAD:~0,34%"
set "CAPVPAD=%CAPV%             "
set "CAPVPAD=%CAPVPAD:~0,13%"
if "%CAPV%"=="ABSENT" (
  echo   %CAPPAD%%CAPVPAD%%CAPWHERE%
) else if "%CAPV%"=="UNCHECKABLE" (
  echo   %CAPPAD%%CAPVPAD%no %CAPFILE% here
) else (
  echo   %CAPPAD%%CAPVPAD%
)
set "CAPHIT="
set "CAPREQ="
goto :eof

:end
echo.
echo layer-check exit %RC%
endlocal & exit /b %RC%
