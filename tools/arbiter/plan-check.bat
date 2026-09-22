@echo off
setlocal
rem ============================================================
rem  plan-check.bat  -  refuse a PHASE_PLAN.md the loop cannot run
rem
rem      plan-check.bat [plan]
rem
rem      0  the plan's form is runnable - every check below passed
rem      1  REFUSED - each failing check is named, with its line
rem      2  usage, or the plan could not be read
rem
rem  THE CHECKS, 065 task 5:
rem    1  every criterion line carries N.k, a box - [ ] or - [x] - and text
rem    2  no *must-pass* line sits outside that form, where the count
rem       cannot see it - HamLet's plan was written that way and nothing
rem       could be counted
rem    3  criterion ids are unique within a step
rem    4  a criterion's N matches the step heading it sits under
rem    5  every step not done has at least one criterion
rem    6  at least one criterion anywhere is not the owner's verdict - a plan
rem       of nothing but owner's verdicts is a phase with no work in it
rem
rem  WHY THIS EXISTS. The owner does not hand-write or hand-correct a plan:
rem  this layer is intended for unaided work, so a plan is written by a unit,
rem  and the guard against a bad one cannot be the owner reading it. It is a
rem  form a script checks, and the owner's verdict arriving once, at the end.
rem
rem  WHAT THIS CANNOT CHECK, SAID ON EVERY RUN. Whether a criterion is
rem  DECIDABLE. A line reading - [ ] 3.1 the card looks right, unmarked, passes
rem  every check above and stalls the loop for ever, because no unit can meet
rem  it and nothing marks it as the owner's. That is a rule for whoever writes
rem  the plan. No script can measure it, and a check that implied it could
rem  would be worse than one that says so.
rem
rem  Step states are read from PHASE_OUTCOME.md beside the plan, the file the
rem  loop reads; where there is none, every step counts as not done. The plan
rem  is read as UTF-8, for the reason criteria-count.bat gives.
rem
rem  Batch, not PowerShell: a .ps1 will not run on this machine.
rem
rem  Generated 2026-09-14 for: work instructions 065 task 5
rem ============================================================

set "RC=0"
set "PLAN=%~1"
if "%PLAN%"=="" set "PLAN=C:\Source\ClaudeProjectStatus\PHASE_PLAN.md"
if exist "%PLAN%" goto :planok
echo ERROR: no such plan: %PLAN%
set "RC=2"
goto :end
:planok

echo.
echo ============================================================
echo  plan-check
echo    plan : %PLAN%
echo ============================================================
echo.
powershell -NoProfile -Command "$p='%PLAN%'; try{ $t=@(Get-Content -LiteralPath $p -Encoding UTF8 -ErrorAction Stop) }catch{ 'ERROR: the plan could not be read'; exit 2 }; $pd=Split-Path -Parent $p; if([string]::IsNullOrEmpty($pd)){ $pd='.' }; $o=Join-Path $pd 'PHASE_OUTCOME.md'; $state=@{}; if(Test-Path -LiteralPath $o){ foreach($ln in (Get-Content -LiteralPath $o -Encoding UTF8)){ if($ln -cmatch '^STEP: *([0-9]+) *\| *([a-z ]+?) *\|'){ if(-not $state.ContainsKey([int]$Matches[1])){ $state[[int]$Matches[1]]=$Matches[2] } } } }; $bad=@(); $steps=@(); $seen=@{}; $count=@{}; $work=0; $owner=0; $head=-1; for($i=0;$i -lt $t.Count;$i++){ $ln=([string]$t[$i]).TrimStart([char]0xFEFF); $no=$i+1; if($ln -cmatch '^STEP: *([0-9]+) *\|'){ $steps+=[int]$Matches[1]; continue }; if($ln -match '^#{1,6}\s+Step\s+([0-9]+)\b'){ $head=[int]$Matches[1]; continue }; if($ln -match '^\s*-\s\['){ if($ln -match '^\s*-\s\[( |x|X)\]\s+([0-9]+)\.([0-9]+)\s+\S'){ $sn=[int]$Matches[2]; $id=$Matches[2] + '.' + $Matches[3]; if(($head -ge 0) -and ($sn -ne $head)){ $bad+=('check 4  line ' + $no + ': criterion ' + $id + ' sits under the heading of step ' + $head) }; if($seen.ContainsKey($id)){ $bad+=('check 3  line ' + $no + ': criterion ' + $id + ' appears again - first at line ' + $seen[$id]) } else { $seen[$id]=$no }; if(-not $count.ContainsKey($sn)){ $count[$sn]=0 }; $count[$sn]++; if($ln -match '(?i)\*owner.s verdict\*\s*$'){ $owner++ } else { $work++ } } else { $bad+=('check 1  line ' + $no + ': a criterion line without N.k and text: ' + $ln.Trim()) }; continue }; if($ln -match '(?i)\*must-pass\*'){ $bad+=('check 2  line ' + $no + ': a must-pass line outside the - [ ] N.k form, which nothing can count: ' + $ln.Trim()) } }; foreach($s in $steps){ $st='none'; if($state.ContainsKey($s)){ $st=$state[$s] }; if($st -eq 'done'){ continue }; if((-not $count.ContainsKey($s)) -or ($count[$s] -eq 0)){ $bad+=('check 5  step ' + $s + ' is ' + $st + ' and has no criterion in the form') } }; if(($work + $owner) -gt 0 -and $work -eq 0){ $bad+='check 6  every criterion is the owner''s verdict - a phase with no work in it' }; if(($work + $owner) -eq 0){ $bad+='check 5/6  the plan carries no criterion in the form at all' }; '  steps          : ' + ($steps -join ', '); '  criteria       : ' + ($work + $owner) + ' - ' + $work + ' work, ' + $owner + ' the owner''s verdict'; ''; if($bad.Count -eq 0){ '  every check passed: the form is runnable.' } else { foreach($b in $bad){ '  FAILED  ' + $b } }; ''; '  CANNOT CHECK: whether a criterion is decidable. A line such as'; '  - [ ] 3.1 the card looks right, unmarked, passes every check here and'; '  stalls the loop for ever. That is a rule for whoever writes the plan.'; if($bad.Count -gt 0){ exit 1 } else { exit 0 }"
set "RC=%ERRORLEVEL%"
echo.
if "%RC%"=="0" echo   RUNNABLE - the plan's form passes.
if "%RC%"=="1" echo   REFUSED - the loop cannot run this plan as written.

:end
echo.
echo plan-check exit %RC%
endlocal & exit /b %RC%
