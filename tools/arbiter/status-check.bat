@echo off
rem ============================================================
rem  status-check.bat  -  is that status file shaped like one?
rem
rem      status-check.bat [root]
rem
rem      0  clean - every check passed
rem      1  A CHECK FAILED - the failing checks are named above
rem      2  the file is absent, or nothing could be read from it
rem
rem  CATCH IT WHERE IT IS WRITTEN. The owner's ruling of
rem  2026-09-01: a bad field is checked in the unit that wrote
rem  it, while that unit's report is still being judged - not
rem  at the panel an hour later. The panel stays the LAST line
rem  of defence rather than the only one.
rem
rem  WHY NOT HARDEN THE WRITERS. There is no single writer.
rem  Sessions write these files by hand, four faults in three
rem  days came from four different sessions, and three days of
rem  fixing writers one at a time is the evidence. The ruling
rem  rejects it explicitly.
rem
rem  IT HOLDS ITS OWN COPY OF THE REQUIRED SET AND PRINTS IT,
rem  exactly as validate-output.bat does and for CPS-DEC-066's
rem  reason: two independent checks that must agree is what
rem  CLAUDE_CODE.md section 5 already describes, and parsing one
rem  out of the other collapses them into one check wearing two
rem  coats. Printing the set is what makes two copies safe,
rem  because nothing else compares them.
rem
rem  THE REQUIRED SET, from STATUS_PROTOCOL.md's status table:
rem    STATE TASK BALL NEXT_PASTE WORK_INSTRUCTION UPDATED NOTE
rem  The panel's own REQUIRED_STATUS agrees with it field for
rem  field - measured by 059 task 1, not assumed.
rem
rem  IT READS THROUGH POWERSHELL AND NEVER findstr. 058 measured
rem  that findstr /b misses every field below line 1 in a
rem  CR-only file and the field ON line 1 in a BOM'd one. A
rem  checker that reports a missing field about a file that has
rem  one is worse than no checker.
rem
rem  ONE EXIT POINT, as lock.bat and validate-output.bat: every
rem  path sets RC and jumps to :end. lock.bat's header records
rem  why - exit /b from inside a nested block is lost under
rem  cmd /c script.bat, the form a double-click uses.
rem
rem  %~dp0 IS CAPTURED BEFORE ANY shift. 058 measured that shift
rem  moves %0 too, so afterwards %~dp0 resolves to the CALLER'S
rem  directory and every sibling script goes missing.
rem ============================================================
setlocal
set "SCHERE=%~dp0"
set "RC=0"
set /a FAILED=0

set "ROOT=%~1"
if "%ROOT%"=="" set "ROOT=C:\Source\ClaudeProjectStatus"
if "%ROOT:~-1%"=="\" set "ROOT=%ROOT:~0,-1%"

set "STATUS=%ROOT%\PROJECT_STATUS.md"
set "PHASE=%ROOT%\PHASE_STATUS.md"

echo.
echo ============================================================
echo  status-check
echo    root   : %ROOT%
echo ============================================================
echo.
echo Checks applied, held by this script and NOT parsed from
echo STATUS_PROTOCOL.md - two copies that must agree, CPS-DEC-066:
echo    1  UPDATED present
echo    2  UPDATED parseable as a timestamp
echo    3  UPDATED not ahead of this machine's clock
echo    4  every required field present:
echo          STATE TASK BALL NEXT_PASTE WORK_INSTRUCTION UPDATED NOTE
echo    5  no mojibake and no replacement character in any field
echo    6  transport clean - and named where it was normalized to read
echo    7  the same checks on PHASE_STATUS.md where one exists
echo.

if not exist "%STATUS%" (
  echo   ABSENT  no PROJECT_STATUS.md at %ROOT%
  echo           Absent means NOT WRITTEN. That is a different fact from
  echo           written wrong, and this exits 2 rather than 1.
  set "RC=2"
  goto :end
)

call :checkfile "%STATUS%" "PROJECT_STATUS.md" required
if "%CFRC%"=="2" set "RC=2" & goto :end

rem --- 7. PHASE_STATUS.md, where one exists ---------------------
rem  ITS ABSENCE IS NOT A FAULT. PHASE_CONTROL.md section 4 is
rem  explicit: a project that has not adopted phase control simply
rem  has no PHASE_STATUS.md, and the degraded case is a missing
rem  file rather than a missing field.
echo.
if not exist "%PHASE%" (
  echo   n/a     no PHASE_STATUS.md - this project is not under phase
  echo           control, which PHASE_CONTROL.md section 4 says is not a fault
) else (
  call :checkfile "%PHASE%" "PHASE_STATUS.md" phase
)

echo.
if %FAILED%==0 (
  echo   CLEAN - every check passed.
  set "RC=0"
) else (
  echo   %FAILED% check^(s^) FAILED, named above.
  echo.
  echo   THIS DOES NOT HALT THE PHASE. The owner's ruling of 2026-09-01:
  echo   the fault is recorded and named against the unit that caused it,
  echo   and the loop goes on. A bad timestamp is not a reason to stop
  echo   the night's work.
  set "RC=1"
)
goto :end

rem ============================================================
rem  One file, every check. %~1 path, %~2 display name, %~3 mode.
rem  `required` applies the seven-field set; `phase` applies the
rem  fields PHASE_CONTROL.md section 4 requires of a phase file.
rem ============================================================
:checkfile
rem  EVERY CF_ VARIABLE IS CLEARED FIRST. Without this the second file
rem  inherits the first's readings - measured: PHASE_STATUS.md, which
rem  carries no UPDATED at all, reported PROJECT_STATUS.md's timestamp as
rem  its own and passed. A checker that reports a field a file does not
rem  have is worse than no checker.
for /f "delims==" %%V in ('set CF_ 2^>nul') do set "%%V="
set "CFRC=0"
set "CFPATH=%~1"
set "CFNAME=%~2"
set "CFMODE=%~3"
echo   %CFNAME%
set "CFOUT="
for /f "usebackq tokens=1,* delims=|" %%A in (`powershell -NoProfile -Command "$p='%CFPATH%'; $b=[System.IO.File]::ReadAllBytes($p); $bom=($b.Length -ge 3 -and $b[0] -eq 239 -and $b[1] -eq 187 -and $b[2] -eq 191); $s=[System.Text.Encoding]::UTF8.GetString($b); if($bom){ $s=$s.Substring(1) }; $CRc=[string][char]13; $LFc=[string][char]10; $crlf=$s.Contains($CRc+$LFc); $cr=[regex]::IsMatch($s,$CRc+'(?!'+$LFc+')'); $shape=@(); if($bom){$shape+='bom'}; if($cr){$shape+='cr'} elseif($crlf){$shape+='crlf'}; if($shape.Count -eq 0){$shape=@('lf')}; 'SHAPE|'+($shape -join '+'); $lines=[regex]::Split($s,$CRc+$LFc+'|'+$LFc+'|'+$CRc); $fields=@{}; foreach($ln in $lines){ $t=$ln.Trim(); if($t -eq '' -or $t -match '^-{3,}$' -or $t.StartsWith('#')){ break }; $m=[regex]::Match($t,'^([A-Za-z_][A-Za-z0-9_]*)\s*:\s*(.*)$'); if($m.Success){ $k=$m.Groups[1].Value.ToUpper(); if($fields.ContainsKey($k)){ 'DUP|'+$k } else { $fields[$k]=$m.Groups[2].Value.Trim() } } }; 'COUNT|'+$fields.Count; foreach($k in $fields.Keys){ 'FIELD|'+$k; if($fields[$k] -eq ''){ 'EMPTY|'+$k } }; if($fields.ContainsKey('STATE')){ 'STATEVAL|'+$fields['STATE'] }; if($fields.ContainsKey('UPDATED')){ $u=$fields['UPDATED']; 'UPDRAW|'+$u; $d=$null; try { $d=[DateTime]::Parse($u) } catch { $d=$null }; if($d -ne $null){ 'UPDPARSED|'+$d.ToString('yyyy-MM-dd HH:mm:ss'); $ahead=($d-(Get-Date)).TotalSeconds; if($ahead -gt 30){ 'AHEAD|'+[Math]::Round($ahead) } } else { 'UPDBAD|1' } }; $moji=@(); foreach($k in $fields.Keys){ $v=$fields[$k]; if($v.IndexOf([char]0xFFFD) -ge 0 -or [regex]::IsMatch($v,[char]0x00E2+[char]0x0080+'['+[char]0x0080+'-'+[char]0x00BF+']|'+[char]0x00C3+'['+[char]0x0080+'-'+[char]0x00BF+']'+[char]0x00C2)){ $moji+=$k } }; foreach($k in $moji){ 'MOJI|'+$k }"`) do call :cfline "%%A" "%%B"

if "%CF_COUNT%"=="0" (
  echo     ABSENT  nothing could be read - not one KEY: value line
  echo             That is `nothing at all can be extracted`, so exit 2.
  set "CFRC=2"
  goto :eof
)
if not defined CF_COUNT (
  echo     ABSENT  the file could not be read at all
  set "CFRC=2"
  goto :eof
)

rem --- 6. transport ---------------------------------------------
if "%CF_SHAPE%"=="lf" (
  echo     ok      transport   plain LF, nothing normalized
) else (
  echo     ok      transport   %CF_SHAPE% - normalized to read it, nothing else changed
)

rem --- 1,2,3. UPDATED -------------------------------------------
rem  PHASE_CONTROL.md section 4 does NOT require UPDATED of a phase file -
rem  the launcher's HEARTBEAT is that file's timestamp. So an absent
rem  UPDATED is a failure for PROJECT_STATUS.md and n/a for the phase file.
rem  Saying n/a is a real answer where saying FAILED would be a finding
rem  this script invented.
if /i not "%CFMODE%"=="required" if not defined CF_UPDRAW (
  echo     n/a     UPDATED     not required of a phase file - PHASE_CONTROL.md 4
  goto :cfneed
)
if not defined CF_UPDRAW (
  echo     FAILED  UPDATED     absent
  echo             STATUS_PROTOCOL.md requires it. Without it the panel falls
  echo             back to the file mtime and says so, but the field is a gap.
  set /a FAILED+=1
) else if defined CF_UPDBAD (
  echo     FAILED  UPDATED     "%CF_UPDRAW%" is not a timestamp this can parse
  echo             Absent and unreadable are different facts. This is written wrong.
  set /a FAILED+=1
) else if defined CF_AHEAD (
  echo     FAILED  UPDATED     "%CF_UPDRAW%" is %CF_AHEAD%s AHEAD of this clock
  echo             Every file is on the machine that reads it, so there is no
  echo             second clock to be skewed against - a future time was typed
  echo             rather than measured. The panel names it and the age cannot
  echo             be measured, so the staleness reading is lost.
  set /a FAILED+=1
) else (
  echo     ok      UPDATED     %CF_UPDRAW% - parses, and not ahead of this clock
)

:cfneed
rem --- 4. the required fields -----------------------------------
if /i "%CFMODE%"=="required" (
  call :need STATE
  call :need TASK
  call :need BALL
  call :need NEXT_PASTE
  call :need WORK_INSTRUCTION
  call :need NOTE
) else (
  call :need PHASE
  call :need CURRENT_STEP
)

rem --- 4b. STATE is one of the five terms -----------------------
rem  ADDED BY WHAT TASK 1 FOUND AND TASK 6 CAUGHT. Presence was not
rem  enough: HamLet's file carries STATE: IN_PROGRESS, which is present,
rem  parses, and is NOT one of the five terms - and an unrecognised STATE
rem  is exactly the fault that used to cost the whole card. This check
rem  passed that file until it was run against it.
rem
rem  THE FIVE ARE STATUS_PROTOCOL.md section 4's, held here and printed
rem  above rather than parsed out of it - CPS-DEC-066, as the rest.
if /i "%CFMODE%"=="required" call :statevocab

rem --- 4c. duplicate keys ---------------------------------------
rem  059 TASK 5, FOUND BY SWEEPING. A second `STATE:` line silently
rem  overrode the first: the card showed BLOCKED for a file whose first
rem  STATE said EXECUTING, with nothing saying a choice had been made.
rem  LAST ONE WINS in every reader here and that is not changed - picking
rem  a different one would be picking a different answer, and nothing can
rem  know which the session meant. What is fixed is the SILENCE.
rem  STEP: IS THE ONE KEY THAT LEGITIMATELY REPEATS. PHASE_CONTROL.md
rem  section 4's format is one STEP: line per step - `STEP: x n, repeated,
rem  at least one is required`. The first run of this check called a correct
rem  PHASE_STATUS.md faulty four times over, which is the kind of false
rem  positive that gets a checker switched off.
set "CF_DUPR=%CF_DUP%"
if /i not "%CFMODE%"=="required" set "CF_DUPR=%CF_DUP:STEP=%"
for /f "tokens=*" %%D in ("%CF_DUPR%") do set "CF_DUPR=%%D"
if defined CF_DUPR (
  echo     FAILED  duplicate   the same key appears twice:%CF_DUPR%
  echo             The LAST one wins in every reader. Which one you meant
  echo             cannot be known from the file - delete one.
  set /a FAILED+=1
) else (
  echo     ok      duplicate   no key appears twice
)

rem --- 5. mojibake ----------------------------------------------
if defined CF_MOJI (
  echo     FAILED  content     mojibake or a replacement character in: %CF_MOJI%
  echo             That is CONTENT corruption, not transport. Nothing has been
  echo             normalized away and nothing has been repaired. Read the file.
  set /a FAILED+=1
) else (
  echo     ok      content     no mojibake and no replacement character
)
goto :eof

:statevocab
if not defined CF_STATE goto :eof
set "SVOK="
for %%S in (PREPARING_PROMPT ANSWERING_QUESTIONS EXECUTING COMPLETED BLOCKED) do if /i "%CF_STATE%"=="%%S" set "SVOK=1"
if defined SVOK (
  echo     ok      STATE       %CF_STATE% is one of the five terms
) else (
  echo     FAILED  STATE       %CF_STATE% is NOT one of the five terms
  echo             PREPARING_PROMPT ANSWERING_QUESTIONS EXECUTING COMPLETED BLOCKED
  echo             The panel reads it as unreadable: it costs the state lamp
  echo             and the cycle position. Every other field still shows.
  set /a FAILED+=1
)
goto :eof

rem  One required field. Present is `ok`; absent is a named failure.
:need
call set "HAVE=%%CF_F_%~1%%"
call set "MT=%%CF_E_%~1%%"
rem  PRESENT WITH NO VALUE IS NOT PRESENT. `BALL:` with nothing after it
rem  passes a presence test and gives the panel nothing to render - so a
rem  checker that only asks whether the key exists reports a field the
rem  owner does not actually have. 059 task 5.
if defined MT (
  echo     FAILED  field       %~1 present but EMPTY - a key with no value
  set /a FAILED+=1
  goto :eof
)
if defined HAVE (
  echo     ok      field       %~1 present
) else (
  echo     FAILED  field       %~1 ABSENT - STATUS_PROTOCOL.md requires it
  set /a FAILED+=1
)
goto :eof

rem  One tagged line back from the reader.
:cfline
set "K=%~1"
set "V=%~2"
if "%K%"=="SHAPE"     set "CF_SHAPE=%V%"
if "%K%"=="COUNT"     set "CF_COUNT=%V%"
if "%K%"=="FIELD"     set "CF_F_%V%=1"
if "%K%"=="DUP"       set "CF_DUP=%CF_DUP% %V%"
if "%K%"=="EMPTY"     set "CF_E_%V%=1"
if "%K%"=="STATEVAL"  set "CF_STATE=%V%"
if "%K%"=="UPDRAW"    set "CF_UPDRAW=%V%"
if "%K%"=="UPDPARSED" set "CF_UPDPARSED=%V%"
if "%K%"=="UPDBAD"    set "CF_UPDBAD=1"
if "%K%"=="AHEAD"     set "CF_AHEAD=%V%"
if "%K%"=="MOJI"      set "CF_MOJI=%CF_MOJI% %V%"
goto :eof

:end
echo.
echo status-check exit %RC%
endlocal & exit /b %RC%
