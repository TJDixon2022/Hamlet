@echo off
rem ============================================================
rem  reload.bat  -  the arbiter's first act, measured
rem
rem      reload.bat [root] [--out PATH] [--bundle [ZIP]]
rem
rem      0  gathered
rem      1  the repository root is wrong
rem      2  the output file could not be written
rem      3  --bundle was asked for and no zip was produced
rem
rem  --bundle ALSO PACKAGES THE SOURCES. The reload output is one
rem  text file and answers "what is the position". A cold arbiter
rem  frequently wants the documents themselves next - the plan, the
rem  accumulated outcome, the card, the status file - and asking
rem  for them is a round trip through the owner. So --bundle puts
rem  the reload and those sources in one zip with a MANIFEST.txt in
rem  the format extract-verify.bat reads, which means the receiver
rem  can detect a truncated bundle rather than assume it away.
rem
rem  IT DECLARES ONLY WHAT IT PACKED. A source that is absent is
rem  left out of the manifest AND named in the reload output as
rem  absent - never listed as though it shipped, because
rem  extract-verify.bat would then refuse the whole bundle at exit
rem  6 for a file that was never going to be there.
rem
rem  Gathers the eight things PHASE_CONTROL.md section 3 requires -
rem  the phase plan, the phase status, the accumulated outcome,
rem  CLAUDE.md including the rulings in force, CLAUDE_CODE.md, the
rem  project card, the project status, and the tree - into ONE FILE
rem  A COLD READER CAN READ TOP TO BOTTOM.
rem
rem  ---------------------------------------------------------------
rem  WHY IT EXISTS, measured twice.
rem
rem  Unit 037 was authored from a reading four units stale and
rem  shipped six claims the tree contradicted, including a red suite
rem  that had been green for three units and an open question that
rem  had already been ruled.
rem
rem  Unit 041's trace found PHASE_CONTROL.md untracked and
rem  PHASE_PLAN.md absent - a delivery the arbiter believed had
rem  landed. It had not, and nothing told the arbiter until a
rem  session did.
rem
rem  Both are the same failure: ACTING ON A PICTURE RATHER THAN A
rem  MEASUREMENT. Being cold is the advantage - a cold arbiter
rem  cannot feel current, so it has no choice but to measure.
rem  ---------------------------------------------------------------
rem
rem  IT READS. IT WRITES ONLY ITS OWN OUTPUT FILE. No commit, no
rem  checkout, no fetch, no repair of anything it finds wrong.
rem  PREFLIGHT.md section 2's scope: an agent that repairs what it
rem  finds has stopped being a witness.
rem
rem  EVERY ITEM IS MEASURED AT RUN TIME, NEVER REMEMBERED. The
rem  highest ruling id is read out of CLAUDE.md section 1, not out
rem  of RULES_AT - RULES_AT is the field that was wrong. HEAD comes
rem  from git.
rem
rem  THE SUITE IS NOT RUN. BASELINE_RED is reported as THE CARD'S
rem  CLAIM and labelled as one. Running the suite is minutes and
rem  this command has to be cheap enough to run every unit; a
rem  reload nobody runs because it is slow is the reload that was
rem  skipped.
rem
rem  ANYTHING ABSENT IS REPORTED AS ABSENT, BY NAME. A missing
rem  PHASE_PLAN.md is the single most important line in the output
rem  and must never be a blank section - a blank section reads as
rem  nothing to say.
rem
rem  THE DISAGREEMENTS COME FIRST, before the gathered content. A
rem  reader who has to scroll to find out his picture is wrong will
rem  not scroll.
rem
rem  EACH DISAGREEMENT NAMES BOTH SIDES - the card says X, the tree
rem  says Y. A line saying only that something is wrong sends the
rem  reader to look for himself, which is the work this command
rem  exists to have already done.
rem
rem  NEVER NAME A HELPER Rd. THIS DELETED FIVE TRACKED FILES.
rem  `rd` is a BUILT-IN POWERSHELL ALIAS FOR Remove-Item, and
rem  ALIASES BEAT FUNCTIONS in command resolution - so a
rem  `function Rd($p){ Get-Content $p }` is defined, ignored, and
rem  every `Rd <file>` DELETES that file instead of reading it.
rem  Get-Command Rd returns Alias -> Remove-Item even with the
rem  function in scope.
rem
rem  On 2026-08-29 this removed CLAUDE.md, CLAUDE_CODE.md,
rem  PHASE_OUTCOME.md, PROJECT_CARD.md and PROJECT_STATUS.md from
rem  the working tree - the repository's constitution among them -
rem  and PHASE_PLAN.md, which was untracked and is unrecoverable.
rem  The deleted set was exactly the set of files passed to Rd,
rem  which is what identified it. The helper is ReadLines now.
rem
rem  THE LESSON IS WIDER THAN THIS FILE: a two-letter helper name
rem  in PowerShell is a coin toss against the alias table. rd, rm,
rem  ri, del, mv, cp, ls, cd, gc, sc and many more are aliases,
rem  and several of them destroy.
rem
rem  POWERSHELL VARIABLES ARE CASE-INSENSITIVE, and that is a real
rem  trap rather than a style note. The first cut used $D for the
rem  disagreement list and $d as a foreach variable, and $L for the
rem  output lines and $l for a line - so each accumulator was
rem  OVERWRITTEN BY ITS OWN LOOP, and the run died with
rem  "[System.String] does not contain a method named 'Add'". The
rem  names are $DISAG and $LINES now. Found by running it.
rem
rem  Batch, not PowerShell: a .ps1 will not run on this machine,
rem  unsigned scripts are blocked by execution policy. The inline
rem  powershell -NoProfile -Command calls are not script files.
rem
rem  Generated 2026-08-29 for: work instructions 042 tasks 2 and 3
rem ============================================================

setlocal

set "HERE=%~dp0"
set "RC=0"
set "ROOT="
set "OUT="
set "BUNDLE="
set "BUNDLEZIP="

:parse
if "%~1"=="" goto :parsed
if /i "%~1"=="--out" set "OUT=%~2" & shift & shift & goto :parse
if /i "%~1"=="--bundle" goto :parsebundle
if not defined ROOT set "ROOT=%~1"
shift
goto :parse

:parsebundle
rem  --bundle takes an OPTIONAL zip path. A substring test cannot be
rem  applied to %~1 directly - `if "%~1:~0,2%"=="--"` is a syntax
rem  error, not a comparison, and cmd rejected the whole file with
rem  "The syntax of the command is incorrect." So the argument goes
rem  into a variable first and the variable is sliced.
set "BUNDLE=1"
shift
if "%~1"=="" goto :parse
set "NEXTARG=%~1"
if "%NEXTARG:~0,2%"=="--" goto :parse
set "BUNDLEZIP=%~1"
shift
goto :parse

:parsed
if "%ROOT%"=="" set "ROOT=C:\Source\HamLet"
if "%ROOT:~-1%"=="\" set "ROOT=%ROOT:~0,-1%"

if not exist "%ROOT%\" (
  echo ERROR: repository root not found: %ROOT%
  set "RC=1"
  goto :end
)
if not exist "%ROOT%\CLAUDE.md" (
  echo ERROR: no CLAUDE.md at %ROOT% - that is not this repository.
  echo Refusing to gather a picture of a tree this is not.
  set "RC=1"
  goto :end
)

if "%OUT%"=="" set "OUT=%ROOT%\.run-unit\reload.txt"
for %%D in ("%OUT%") do if not exist "%%~dpD" mkdir "%%~dpD"

echo.
echo ============================================================
echo  reload
echo    root : %ROOT%
echo    out  : %OUT%
echo ============================================================
echo.

rem --- the clock, measured -------------------------------------
set "NOW="
for /f "usebackq delims=" %%D in (`powershell -NoProfile -Command "(Get-Date).ToString('yyyy-MM-ddTHH:mm:sszzz')"`) do set "NOW=%%D"

rem  Everything below is written by ONE inline PowerShell that
rem  reads the tree and writes the file. No cmd-side string
rem  assembly anywhere - CPS-DEC-021, four silent corruptions in
rem  three runs, three of them landing in files this repository
rem  treats as the record.
powershell -NoProfile -Command "$root='%ROOT%'; $out='%OUT%'; $now='%NOW%'; $LINES=New-Object System.Collections.ArrayList; function W($s){[void]$LINES.Add($s)}; function ReadLines($p){ if(Test-Path -LiteralPath $p){ Get-Content -LiteralPath $p } else { $null } }; $DISAG=New-Object System.Collections.ArrayList; function Dis($s){[void]$DISAG.Add($s)}; $req=@('PHASE_PLAN.md','PHASE_STATUS.md','PHASE_OUTCOME.md','CLAUDE.md','CLAUDE_CODE.md','PROJECT_CARD.md','PROJECT_STATUS.md'); $present=@{}; foreach($f in $req){ $present[$f] = Test-Path -LiteralPath (Join-Path $root $f) }; foreach($f in $req){ if(-not $present[$f]){ Dis(('ABSENT           ' + $f + ' is required by PHASE_CONTROL.md section 3 and is not at the root.')) } }; $ids=@(); foreach($m in (Select-String -Path (Join-Path $root 'CLAUDE.md') -Pattern '\| ([A-Z]+-DEC-)(\d+) \|$' -AllMatches)){ foreach($mm in $m.Matches){ $ids += [int]$mm.Groups[2].Value } }; $high=''; $highDate=''; if($ids.Count){ $n=($ids | Measure-Object -Maximum).Maximum; $row=(Select-String -Path (Join-Path $root 'CLAUDE.md') -Pattern ('\| CPS-DEC-0*' + $n + ' \|$') | Select-Object -First 1); $high='CPS-DEC-0' + $n; if($row -and $row.Line -match '^\| (\d{4}-\d{2}-\d{2}) \|'){ $highDate=$matches[1] } }; $ra=''; $st=ReadLines (Join-Path $root 'PROJECT_STATUS.md'); if($st){ $l=($st | Where-Object { $_ -match '^RULES_AT:' } | Select-Object -First 1); if($l){ $ra=$l.Substring(9).Trim() } }; $want = if($highDate){ $high + ' (' + $highDate + ')' } else { $high }; if($ra -ne $want){ Dis(('RULES_AT         PROJECT_STATUS.md says ' + $ra + '; CLAUDE.md section 1 holds ' + $want + '.')) }; Push-Location $root; $head=(& git rev-parse --short HEAD 2>$null); $branch=(& git rev-parse --abbrev-ref HEAD 2>$null); $dirty=@(& git status --porcelain 2>$null); Pop-Location; $rootDirty=@($dirty | Where-Object { $_ -notmatch '/' }); foreach($d in $rootDirty){ $code=$d.Substring(0,2).Trim(); $name=$d.Substring(3); $what = if($code -eq '??'){ 'is UNTRACKED - a fresh clone does not have it' } else { 'is modified and uncommitted' }; Dis(('UNCOMMITTED      ' + $name + ' ' + $what + '.')) }; $ccTree=''; $ccHead=''; $cc=ReadLines (Join-Path $root 'CLAUDE_CODE.md'); if($cc){ $l=($cc | Where-Object { $_ -match 'Version [0-9.]+' } | Select-Object -First 1); if($l -match 'Version ([0-9.]+)'){ $ccTree=$matches[1] } }; Push-Location $root; $ccH=(& git show HEAD:CLAUDE_CODE.md 2>$null); Pop-Location; if($ccH){ $l=($ccH | Where-Object { $_ -match 'Version [0-9.]+' } | Select-Object -First 1); if($l -match 'Version ([0-9.]+)'){ $ccHead=$matches[1] } }; if($ccTree -and $ccHead -and $ccTree -ne $ccHead){ Dis(('CLAUDE_CODE.md   the working tree is ' + $ccTree + '; HEAD is ' + $ccHead + '.')) }; $po=ReadLines (Join-Path $root 'PHASE_OUTCOME.md'); if($po){ $ok='not started','in progress','partial','blocked','done'; $hdr=@{}; foreach($l in $po){ if($l -match '^STEP: '){ $p=$l.Substring(6).Split('|'); if($p.Count -ge 2 -and $ok -contains $p[1].Trim()){ $hdr[$p[0].Trim()]=$p[1].Trim() } } }; $last=@{}; $cur=''; foreach($l in $po){ if($l -match '^## UNIT \S+ . STEP (\S+)\s*$'){ $cur=$matches[1] }; if($l -match '^STATE_AFTER: (.*)$' -and $cur){ $last[$cur]=$matches[1].Trim() } }; foreach($k in $last.Keys){ if($hdr.ContainsKey($k) -and $hdr[$k] -ne $last[$k]){ Dis(('PHASE_OUTCOME    header says step ' + $k + ' is ' + $hdr[$k] + '; the last entry for it says ' + $last[$k] + '. The entries win.')) } } }; W('RELOAD - PHASE_CONTROL.md section 3'); W(('measured ' + $now + '  root ' + $root)); W('Every line below was read from the tree at that moment. Nothing is remembered.'); W(''); W('=============================================================='); W(' DISAGREEMENTS - what does not agree with itself'); W('=============================================================='); W(''); if($DISAG.Count -eq 0){ W('  none. Everything checked agrees.') } else { foreach($d in $DISAG){ W('  ' + $d) } }; W(''); W('  Checked: RULES_AT against CLAUDE.md section 1; the working tree'); W('  against HEAD at the root; CLAUDE_CODE.md tree against HEAD;'); W('  PHASE_OUTCOME.md header against its entries; and the presence of'); W('  every file PHASE_CONTROL.md section 3 requires.'); W(''); W('=============================================================='); W(' THE TREE'); W('=============================================================='); W(''); W(('  branch : ' + $branch)); W(('  HEAD   : ' + $head)); W(('  uncommitted at the root : ' + $rootDirty.Count)); foreach($d in $rootDirty){ W('     ' + $d) }; W(''); W('=============================================================='); W(' THE RULINGS IN FORCE'); W('=============================================================='); W(''); W(('  highest in CLAUDE.md section 1 : ' + $want)); W(('  PROJECT_STATUS.md RULES_AT     : ' + $ra)); W(''); W('=============================================================='); W(' THE EIGHT'); W('=============================================================='); foreach($f in $req){ W(''); W(('  --- ' + $f + ' ---')); if(-not $present[$f]){ W(('  ABSENT. PHASE_CONTROL.md section 3 requires it and it is not at the root.')); continue }; $c=ReadLines (Join-Path $root $f); W(('  ' + $c.Count + ' lines, ' + (Get-Item (Join-Path $root $f)).Length + ' bytes')); if($f -eq 'CLAUDE.md'){ W('  (not inlined - the rulings in force are above; read the file for the rest)'); continue }; if($f -eq 'CLAUDE_CODE.md'){ W(('  version in the tree: ' + $ccTree + '   at HEAD: ' + $ccHead)); W('  (not inlined - it is the standard and it is long)'); continue }; if($f -eq 'PROJECT_CARD.md'){ foreach($l in $c){ if($l -match '^(PROJECT|PHASE|PHASE_SET|TEST_CMD|BASELINE_RED|BRANCH_POLICY|TRUNK):'){ W('  ' + $l) } }; W('  NOTE: BASELINE_RED above is THE CARD S CLAIM about the suite.'); W('  This command does not run the suite - it has to be cheap enough'); W('  to run every unit. Run TEST_CMD yourself if the number matters.'); continue }; foreach($l in $c){ W('  ' + $l) } }; W(''); W('=============================================================='); W(' END OF RELOAD'); W('=============================================================='); Set-Content -LiteralPath $out -Value $LINES -Encoding utf8"

if not exist "%OUT%" (
  echo   ERROR: the reload file could not be written: %OUT%
  set "RC=2"
  goto :end
)

rem  The disagreements are echoed to the console as well as written,
rem  so a caller that never opens the file still sees them.
echo   Disagreements found:
echo.
powershell -NoProfile -Command "$t=Get-Content -LiteralPath '%OUT%'; $i=[array]::IndexOf($t,' DISAGREEMENTS - what does not agree with itself'); $j=[array]::IndexOf($t,' THE TREE'); if($i -ge 0 -and $j -gt $i){ $t[($i+3)..($j-3)] | Where-Object { $_.Trim() -ne '' } | ForEach-Object { '   ' + $_.Trim() } }"
echo.
for %%Z in ("%OUT%") do echo   Written : %OUT%  ^(%%~zZ bytes^)

if not defined BUNDLE goto :nobundle

rem --- the bundle: the reload plus the sources a cold reader wants -
set "BSTAGE=%ROOT%\.run-unit\_bundle"
if exist "%BSTAGE%" rd /s /q "%BSTAGE%"
mkdir "%BSTAGE%"
if "%BUNDLEZIP%"=="" set "BUNDLEZIP=%USERPROFILE%\Downloads\reload-bundle.zip"
if exist "%BUNDLEZIP%" del /q "%BUNDLEZIP%"

copy /y "%OUT%" "%BSTAGE%\reload.txt" >nul
>"%BSTAGE%\MANIFEST.txt" echo rem  MANIFEST.txt - a reload bundle for a cold arbiter
>>"%BSTAGE%\MANIFEST.txt" echo MANIFEST.txt
>>"%BSTAGE%\MANIFEST.txt" echo reload.txt

echo.
echo   Bundling the sources a cold reader wants next:
call :pack "PHASE_PLAN.md"
call :pack "PHASE_STATUS.md"
call :pack "PHASE_OUTCOME.md"
call :pack "PHASE_CONTROL.md"
call :pack "PROJECT_CARD.md"
call :pack "PROJECT_STATUS.md"

echo.
powershell -NoProfile -Command "Compress-Archive -Path '%BSTAGE%\*' -DestinationPath '%BUNDLEZIP%' -Force"
if not exist "%BUNDLEZIP%" (
  echo   ERROR: no bundle was produced. The reload above still stands.
  set "RC=3"
  goto :end
)
rd /s /q "%BSTAGE%"
for %%Z in ("%BUNDLEZIP%") do echo   Bundle  : %BUNDLEZIP%  ^(%%~zZ bytes^)

:nobundle
set "RC=0"
goto :end

rem ============================================================
rem  One source into the bundle. AN ABSENT ONE IS NOT MANIFESTED:
rem  extract-verify.bat refuses a whole delivery at exit 6 for a
rem  manifest entry that did not land, so declaring a file that was
rem  never going to be there would make every bundle unverifiable.
rem  Its absence is already named in the reload output above.
:pack
if not exist "%ROOT%\%~1" (
  echo     absent   %~1  ^(named in the reload, not manifested^)
  goto :eof
)
copy /y "%ROOT%\%~1" "%BSTAGE%\%~1" >nul
>>"%BSTAGE%\MANIFEST.txt" echo %~1
echo     packed   %~1
goto :eof

rem ============================================================
:end
echo.
echo reload exit %RC%
endlocal & exit /b %RC%
