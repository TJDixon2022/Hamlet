@echo off
setlocal
rem ============================================================
rem  attempt-read.bat  -  every attempt recorded against one criterion
rem
rem      attempt-read.bat <criterion N.k> [outcome file]
rem
rem  Prints, in the order the attempts were made, lines a caller reads
rem  with for /f "tokens=1,* delims==":
rem
rem      CRITERION=<N.k>
rem      ATTEMPTS=<n>
rem      ATTEMPT_1=unit <u> | <verdict> | <fate> | <approach>
rem      REASON_1=<why that route is closed, or empty>
rem      ATTEMPT_2=...
rem      REASON_2=...
rem
rem  071 task 3. A REASON_n LINE IS EMITTED ONLY WHERE THE RECORD CARRIES ONE,
rem  immediately after its own ATTEMPT_n. AN ATTEMPT WITH NO REASON EMITS NO
rem  REASON LINE AT ALL, so a record written before unit 071 reads back BYTE
rem  FOR BYTE as it did before.
rem
rem  THE FIRST CUT EMITTED AN EMPTY REASON_n FOR EVERY ATTEMPT, and 066-s
rem  attempt-blocker-crit arm caught it within four arms of the suite starting:
rem  the arm asserts what this reader returns, and an extra line is a change to
rem  what it returns for a record nobody touched. It also made the sentence
rem  directly above FALSE, which is how it was recognised as a defect rather
rem  than an expectation to reverse. Reported rather than quietly reversed.
rem
rem  The reason is its OWN LINE in the record, never a field of the attempt:
rem
rem      REASON: <N.k> | unit <u> launched <id> | <why it is closed>
rem
rem  because the approach is the REMAINDER of the attempt line - 068's matcher
rem  splits it into at most four - so a field appended after the approach
rem  lands inside it. outcome-append.bat carries the whole reasoning.
rem  THE ATTEMPT LINE IS PARSED EXACTLY AS IT WAS, and a record with no
rem  REASON lines reads back exactly as it did before this unit.
rem
rem      0  read - n may be 0, and 0 is an answer, not a failure
rem      2  usage, or the record is absent or could not be read
rem
rem  THE RECORD. 066 task 2, author's, overrulable. An attempt is ONE LINE
rem  in PHASE_OUTCOME.md, inside the entry of the unit that made it:
rem
rem      ATTEMPT: 2.3 | unit 7 launched 2026-09-19T10:59:33.412Z | no | executed | <APPROACH>
rem
rem  THE UNIT FIELD GAINED "launched <id>" IN 068 task 3, and NOTHING HERE
rem  CHANGED. This reader returns everything after the criterion verbatim
rem  and splits no further, so a pre-068 line reading "unit 7" comes back
rem  exactly as it always did and a 068 line carries its identity through.
rem  The identity is the launch stamp's own write time in UTC, because the
rem  unit number counts from 1 again after a restart and two different
rem  units are otherwise both "unit 1".
rem
rem  criterion, unit, verdict - the entry's ADVANCED - fate, and approach.
rem  THE APPROACH IS LAST, so a pipe inside it cannot move a field.
rem
rem  WHY PHASE_OUTCOME.md AND NOT A NEW FILE. It already holds one entry
rem  per unit, append-only, on disk, written by the launcher, and it is the
rem  file the arbiter reads as its memory. A second file of attempts beside
rem  it would be two records of one run, and two records diverge -
rem  CLAUDE.md gives the same reason for keeping one decision log.
rem
rem  WRITTEN BY THE LAUNCHER, NEVER BY A SESSION. run-phase.bat hands
rem  outcome-append.bat the criterion from ADVANCES, the approach from the
rem  decision block and the verdict it computed itself. Nothing a unit says
rem  about its own work enters the line.
rem
rem  NEVER TRUNCATED. The approach is written and read whole however long
rem  it is, because the next step compares approaches and a cut one
rem  compares wrong.
rem
rem  TRANSPORT. Read as bytes, a byte-order mark stripped, lines split on
rem  CRLF, LF or CR alone, decoded as UTF-8 - the tolerance the heartbeat
rem  writer carries and criteria-count.bat measured, because a CR-only file
rem  and a BOM'd one are measured faults in this layer, twice. Lines inside
rem  a fenced block are not read, so a format example is never an attempt.
rem
rem  Batch, not PowerShell: a .ps1 will not run on this machine.
rem
rem  Generated 2026-09-14 for: work instructions 066 task 2
rem ============================================================

set "RC=0"
set "CRIT=%~1"
set "FILE=%~2"
if "%CRIT%"=="" goto :usage
echo %CRIT%| findstr /r "^[0-9][0-9]*\.[0-9][0-9]*$" >nul
if errorlevel 1 goto :usage
if "%FILE%"=="" set "FILE=C:\Source\ClaudeProjectStatus\PHASE_OUTCOME.md"
if exist "%FILE%" goto :fileok
echo ERROR: no attempt record - %FILE% does not exist
set "RC=2"
goto :end
:fileok

powershell -NoProfile -Command "$f='%FILE%'; $w='%CRIT%'.Split('.'); $ws=[int]$w[0]; $wk=[int]$w[1]; try{ $bytes=[IO.File]::ReadAllBytes($f) }catch{ 'ERROR: the attempt record could not be read - ' + $f; exit 2 }; $raw=[Text.Encoding]::UTF8.GetString($bytes); if(($raw.Length -gt 0) -and ($raw[0] -eq [char]0xFEFF)){ $raw=$raw.Substring(1) }; $CRc=[string][char]13; $LFc=[string][char]10; $lines=[regex]::Split($raw, $CRc+$LFc+'|'+$LFc+'|'+$CRc); $inf=$false; $n=0; $out=@(); foreach($ln in $lines){ if($ln -match '^\s*(```|~~~)'){ $inf=-not $inf; continue }; if($inf){ continue }; if($ln -match '^ATTEMPT: *([0-9]+)\.([0-9]+) *\| *(.*)$'){ if(([int]$Matches[1] -eq $ws) -and ([int]$Matches[2] -eq $wk)){ $n++; $out+=('ATTEMPT_' + $n + '=' + $Matches[3]) } continue }; if($ln -match '^REASON: *([0-9]+)\.([0-9]+) *\| *[^|]*\| *(.*)$'){ if(([int]$Matches[1] -eq $ws) -and ([int]$Matches[2] -eq $wk) -and ($n -gt 0)){ $out+=('REASON_' + $n + '=' + $Matches[3].Trim()) } } }; 'CRITERION=' + $ws + '.' + $wk; 'ATTEMPTS=' + $n; $out; exit 0"
set "RC=%ERRORLEVEL%"
goto :end

:usage
echo.
echo   attempt-read.bat ^<criterion N.k^> [outcome file]
echo.
echo   Every ATTEMPT: line recorded against that criterion, in order.
echo   0 read - ATTEMPTS may be 0; 2 usage, or the record absent or unreadable
echo.
set "RC=2"

:end
endlocal & exit /b %RC%
