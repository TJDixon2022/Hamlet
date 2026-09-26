@echo off
setlocal
rem ============================================================
rem  criteria-count.bat  -  how many of a step's criteria are met
rem
rem      criteria-count.bat <plan> <step> [criterion]
rem
rem  Prints three lines a caller reads with for /f "tokens=1,* delims==":
rem
rem      MET=<n>       that step's criteria marked - [x]
rem      TOTAL=<n>     that step's criteria, - [ ] and - [x] together
rem      UNMET=<n>     that step's unmet WORK criteria - - [ ] and not the owner's
rem      OWNER=<n>     that step's unmet OWNER'S-VERDICT criteria
rem      CRIT=<state>  met, unmet, owner or absent, for [criterion]; owner for a
rem                    line carrying the marker, whichever box it shows; absent
rem                    when no criterion was asked for or the plan has no such line
rem
rem  THE OWNER'S-VERDICT MARKER. Unit 065 task 2, author's, overrulable. A
rem  criterion only the owner can judge ends with *owner's verdict*, straight
rem  or curly apostrophe:
rem
rem      - [ ] 4.2 the operator agrees the card reads correctly   *owner's verdict*
rem
rem  The form is otherwise unchanged, so every 064 line still counts the same.
rem  A step whose UNMET is 0 and OWNER is above 0 has no work left, only the
rem  owner's verdict - which is what run-phase.bat halts on. Nothing in the loop
rem  turns a marked line to - [x]; that is the owner's hand alone.
rem
rem  READ AS UTF-8. Windows PowerShell 5.1 reads a file without a byte-order
rem  mark as ANSI, so a curly apostrophe in a UTF-8 plan arrives as three
rem  characters and the marker would not match. Measured by 065: the reader now
rem  names the encoding, and the same plan counts the same as LF without a BOM,
rem  as CRLF and as CR-only with one.
rem
rem      0  counted - a step with no criteria in the form is 0 of 0, and
rem         that is an answer, not a failure
rem      2  usage, or the plan could not be read
rem
rem  ---------------------------------------------------------------
rem  THE FORM. Unit 064 task 2. Neither this repository's PHASE_PLAN.md
rem  nor HamLet's carried a countable criterion: HamLet's must-pass lines
rem  are bullets ending *must-pass*, with no identifier and no marker. So
rem  a criterion is a line of exactly this shape:
rem
rem      - [ ] 2.3 the fixture is killed at ten quiet looks
rem      - [x] 2.3 the fixture is killed at ten quiet looks
rem
rem  step.criterion as whole numbers, a space or an x between brackets,
rem  and text that does not change between the two states. Nothing else
rem  in the line is load-bearing. An X counts as met as well as an x.
rem  A line anywhere in the plan counts for the step its number names,
rem  wherever it sits, so a criterion cannot be lost by moving a heading.
rem
rem  A COUNT, NOT A JUDGMENT. The owner's ruling of 2026-09-14: progress
rem  is counted in criteria, and the count belongs to a script because a
rem  judge can be talked into one and a pattern cannot. This script never
rem  decides whether a criterion was HONESTLY met - run-phase.bat asks the
rem  state judge that, and only about a criterion this script saw flip.
rem
rem  TRANSPORT. The plan is read through PowerShell's Get-Content, the
rem  reader readkey.bat uses, which 058 measured returning the same lines
rem  from LF, CRLF, CR-only and a byte-order mark. findstr is not used:
rem  PHASE_UPLIFT.md section 12 records it finding only the first line of
rem  a CR-only file and missing the first line of a BOM'd one.
rem
rem  Batch, not PowerShell: a .ps1 will not run on this machine.
rem
rem  Generated 2026-09-14 for: work instructions 064 task 2
rem ============================================================

set "RC=0"
set "PLAN=%~1"
set "STEPN=%~2"
set "CRITK=%~3"
if "%PLAN%"=="" goto :usage
if "%STEPN%"=="" goto :usage
echo %STEPN%| findstr /r "^[0-9][0-9]*$" >nul
if errorlevel 1 goto :usage
if "%CRITK%"=="" goto :argsok
echo %CRITK%| findstr /r "^[0-9][0-9]*$" >nul
if errorlevel 1 goto :usage
:argsok
if exist "%PLAN%" goto :planok
echo ERROR: no such plan: %PLAN%
set "RC=2"
goto :end
:planok

set "MET="
set "TOTAL="
set "CRIT="
for /f "usebackq tokens=1,* delims==" %%A in (`powershell -NoProfile -Command "try{ $t=@(Get-Content -LiteralPath '%PLAN%' -Encoding UTF8 -ErrorAction Stop) }catch{ exit }; $n=[int]'%STEPN%'; $k='%CRITK%'; $met=0; $tot=0; $unm=0; $ownu=0; $crit='absent'; foreach($raw in $t){ $ln=([string]$raw).TrimStart([char]0xFEFF); if($ln -match '^\s*-\s\[( |x|X)\]\s+([0-9]+)\.([0-9]+)(\s|$)'){ $mk=$Matches[1]; $sn=[int]$Matches[2]; $cn=[int]$Matches[3]; $own=($ln -match '(?i)\*owner.s verdict\*\s*$'); if($sn -eq $n){ $tot++; $on=($mk -ne ' '); if($on){ $met++ } elseif($own){ $ownu++ } else { $unm++ }; if(($k -ne '') -and ($cn -eq [int]$k)){ if($own){ $crit='owner' } elseif($on){ $crit='met' } else { $crit='unmet' } } } } }; 'MET=' + $met; 'TOTAL=' + $tot; 'UNMET=' + $unm; 'OWNER=' + $ownu; 'CRIT=' + $crit"`) do set "%%A=%%B"

if defined MET goto :counted
echo ERROR: the plan could not be read: %PLAN%
set "RC=2"
goto :end

:counted
echo MET=%MET%
echo TOTAL=%TOTAL%
echo UNMET=%UNMET%
echo OWNER=%OWNER%
echo CRIT=%CRIT%
set "RC=0"
goto :end

:usage
echo.
echo   criteria-count.bat ^<plan^> ^<step^> [criterion]
echo.
echo   Counts a step's criteria in the form  - [ ] N.k text  /  - [x] N.k text
echo   and prints MET=, TOTAL= and CRIT= ^(met, unmet or absent^).
echo   0 counted, 2 usage or the plan could not be read
echo.
set "RC=2"

:end
endlocal & exit /b %RC%
