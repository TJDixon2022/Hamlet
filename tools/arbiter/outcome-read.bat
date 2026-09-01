@echo off
rem ============================================================
rem  outcome-read.bat  -  the arbiter's memory, made queryable
rem
rem      outcome-read.bat [file] [--approach TEXT]
rem
rem      0  read - the position is printed above
rem      1  MALFORMED - the file exists and is not a phase outcome
rem      2  ABSENT - there is no such file
rem
rem  2 IS NOT 1, AND THAT IS THE POINT. A phase that has not
rem  started yet has no outcome file, and that is not an error -
rem  it is the ordinary state of a repository between phases. A
rem  reader that called absence malformed would make every arbiter
rem  starting a phase read a failure. The same line the panel
rem  holds: absent and unreadable are different facts.
rem
rem  WHAT IT PRINTS. Each step, its state, how many units have been
rem  spent against it, and THE APPROACHES ALREADY TRIED. That last
rem  one is the whole reason this script exists.
rem
rem  PHASE_CONTROL.md section 3:
rem
rem    "Forward progress continues, however small. Repetition
rem     stops. Approach A fails, B fails, C fails, and the next
rem     thing proposed resembles A - that is a loop. This requires
rem     memory of what has been tried."
rem
rem  An arbiter starting cold on unit two of a phase knows a unit
rem  ran and cannot know what it tried. This is how it finds out
rem  without reading the whole file.
rem
rem  --approach TEXT answers the loop test directly: it reports
rem  whether that approach appears in any entry, and exits 0
rem  either way. IT IS A READING, NOT A VERDICT - whether a
rem  resemblance amounts to a loop is the arbiter's judgment and
rem  this script does not make it. A script that returned "you are
rem  looping" would be deciding something it cannot see.
rem
rem  A STEP LINE MUST CARRY ONE OF THE FIVE STATES. The format
rem  is documented inside PHASE_OUTCOME.md itself, and that
rem  documentation contains a TEMPLATE line -
rem      STEP: <n> | <state> | <what the step delivers ...>
rem  - which the first cut of this reader parsed as a real step and
rem  printed as the phase's position. A reader that reports a
rem  file's own example as data is worse than one that reports
rem  nothing, because it looks like an answer. So a line is only a
rem  step if its second field is not started, in progress, partial,
rem  blocked or done. The template's <state> is none of those and
rem  falls out; so does a genuinely malformed step line, which is
rem  what MALFORMED is for.
rem
rem  Batch, not PowerShell: a .ps1 will not run on this machine,
rem  unsigned scripts are blocked by execution policy. The inline
rem  powershell -NoProfile -Command calls are not script files and
rem  are used where cmd cannot group and count lines.
rem
rem  Generated 2026-08-29 for: work instructions 041 task 3
rem ============================================================

setlocal

set "RC=0"
set "FILE=%~1"
set "WANTED="

if /i "%~1"=="--approach" set "FILE=" & set "WANTED=%~2" & goto :defaults
if "%~2"=="" goto :defaults
if /i "%~2"=="--approach" set "WANTED=%~3"

:defaults
if "%FILE%"=="" set "FILE=C:\Source\HamLet\PHASE_OUTCOME.md"

echo.
echo ============================================================
echo  outcome-read
echo    file : %FILE%
echo ============================================================
echo.

if not exist "%FILE%" (
  echo   ABSENT - there is no phase outcome file at that path.
  echo.
  echo   This is NOT an error. A phase that has not started has no
  echo   outcome file yet, which is the ordinary state of a
  echo   repository between phases. Absent and malformed are
  echo   different facts and only one of them is a fault.
  set "RC=2"
  goto :end
)

rem --- is it a phase outcome file at all? ------------------------
findstr /b /c:"PHASE:" "%FILE%" >nul 2>&1
if errorlevel 1 (
  echo   MALFORMED - the file exists but carries no PHASE: line.
  echo   A phase outcome file has a phase header; this one does not,
  echo   so nothing below can be trusted to mean what it looks like.
  set "RC=1"
  goto :end
)

rem --- the phase header -----------------------------------------
for /f "usebackq tokens=1,* delims=:" %%A in (`findstr /b /c:"PHASE:" "%FILE%"`) do echo   PHASE     :%%B
for /f "usebackq tokens=1,* delims=:" %%A in (`findstr /b /c:"PHASE_SET:" "%FILE%"`) do echo   PHASE_SET :%%B
echo.

rem --- the position, step by step --------------------------------
echo   THE POSITION
echo   ------------
powershell -NoProfile -Command "$ok='not started','in progress','partial','blocked','done'; $f='%FILE%'; $steps = @(Select-String -Path $f -Pattern '^STEP: ' | ForEach-Object { $_.Line } | Where-Object { $p=$_.Substring(6).Split('|'); $p.Count -ge 2 -and $ok -contains $p[1].Trim() }); if($steps.Count -eq 0){ '   (no steps listed yet - this phase has no plan instantiated)'; exit }; foreach($s in $steps){ $p = $s.Substring(6).Split('|'); $n = $p[0].Trim(); $st = $p[1].Trim(); $what = if($p.Count -gt 2){ $p[2].Trim() } else { '' }; $units = @(Select-String -Path $f -Pattern ('^## UNIT .* STEP ' + [regex]::Escape($n) + '\s*$')).Count; '   step {0}  [{1}]  {2}' -f $n, $st.PadRight(11), $what; '        units spent: {0}' -f $units }"
echo.

rem --- the approaches already tried ------------------------------
echo   APPROACHES ALREADY TRIED
echo   ------------------------
powershell -NoProfile -Command "$f='%FILE%'; $t = Get-Content -LiteralPath $f; $cur=''; $out=@(); for($i=0;$i -lt $t.Count;$i++){ if($t[$i] -match '^## UNIT (\S+) . STEP (\S+)\s*$'){ $cur = 'unit ' + $matches[1] + ', step ' + $matches[2] }; if($t[$i] -match '^APPROACH: (.*)$'){ $out += ('   ' + $cur.PadRight(22) + '  ' + $matches[1]) } }; if($out.Count){ $out } else { '   (none recorded yet)' }"

if not defined WANTED goto :done

echo.
echo   THE LOOP TEST: has this approach been tried?
echo   --------------------------------------------
echo   asking about : %WANTED%
echo.
powershell -NoProfile -Command "$f='%FILE%'; $w='%WANTED%'; $hits = Select-String -Path $f -Pattern ('^APPROACH: .*' + [regex]::Escape($w)) -AllMatches; if($hits){ '   TRIED BEFORE - ' + $hits.Count + ' entr(y/ies) match:'; $hits | ForEach-Object { '     ' + $_.Line.Trim() } } else { '   NOT FOUND in any entry.' }"
echo.
echo   This is a READING, not a verdict. Whether a resemblance
echo   amounts to a loop is the arbiter's judgment, and a script
echo   that answered "you are looping" would be deciding something
echo   it cannot see.

:done
echo.
echo   Read complete.
set "RC=0"
goto :end

rem ============================================================
:end
echo.
echo outcome-read exit %RC%
endlocal & exit /b %RC%
