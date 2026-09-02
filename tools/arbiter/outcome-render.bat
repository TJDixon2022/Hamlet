@echo off
rem ============================================================
rem  outcome-render.bat  -  the morning view
rem
rem      outcome-render.bat [file] [--out PATH] [--no-open]
rem
rem      0  rendered
rem      1  the file is malformed
rem      2  the file is absent
rem      3  the HTML could not be written
rem
rem  THE OWNER'S RULING, 2026-08-28: two or three sentences, then
rem  ONE CARD PER PHASE STEP coloured by status, each carrying
rem  EXACTLY FOUR THINGS - which step, what it was, what actually
rem  happened, what it cost - then a strip naming decisions made
rem  for him, decisions waiting on him, and where to look.
rem
rem  No narrative. Detail comes when he asks.
rem
rem  Rejected by the owner: a list or table of steps. He reviewed
rem  both and the cards gave a complete understanding in about ten
rem  seconds where the list did not.
rem
rem  Rejected: leading with what went wrong. Success is validated
rem  first; decisions are walked through afterwards, at his pace.
rem
rem  HTML, NOT A CONSOLE TABLE, and that is the ruling rather than
rem  a preference: a batch script cannot draw a card in a terminal
rem  and the thing that beat the list was the card.
rem
rem  SELF-CONTAINED. No framework, no external stylesheet, no
rem  network, no font fetch - the same constraint CPS-DEC-017 puts
rem  on the panel, which opens from file:// with nothing
rem  installed. Everything is inline in one file.
rem
rem  COLOUR BY STATUS, AND BLUE IS TAKEN. Green done, amber
rem  partial, red blocked, grey not started, slate in progress.
rem  Blue means IN FLIGHT on the annunciator, and two meanings for
rem  one colour across two screens the owner reads in the same
rem  morning is worse than a fifth hue.
rem
rem  THE CARD'S ONE-LINE NOTE IS WHAT THE STEP ACCOMPLISHED, NOT
rem  WHAT IT DID. "Card is 40 percent shorter, nothing lost", not
rem  "modified three functions". This renders the ACCOMPLISHED
rem  field; the writer supplies it, and if the owner cannot tell
rem  from that line whether he got what he wanted, the line is
rem  wrong and the fix is in the writing.
rem
rem  NO .ps1, AND THAT WAS MEASURED RATHER THAN ASSUMED. The first
rem  cut called a sibling outcome-render.ps1 with
rem  -ExecutionPolicy Bypass. Two things were wrong with it: this
rem  repository's standing note is that an unsigned .ps1 does not
rem  run here, and a batch that depends on a sibling file fails at
rem  the moment the sibling is the thing that did not arrive -
rem  039 recorded that shape exactly. A test appeared to show a
rem  .ps1 running, but the effective policy in that shell was
rem  Bypass INHERITED from the parent, so it proved nothing about
rem  a shipped invocation. The page is therefore built by ONE
rem  inline -Command, which is not a script file.
rem
rem  ANGLE BRACKETS SURVIVE, and that was also measured: inside a
rem  double-quoted cmd argument, < > & and | are literal, so the
rem  HTML can be written straight into the PowerShell string. No
rem  cmd-side string assembly happens anywhere in this file, which
rem  is what CPS-DEC-021 asks for.
rem
rem  Generated 2026-08-29 for: work instructions 041 task 5
rem ============================================================

setlocal

rem  THIS SCRIPT'S OWN DIRECTORY, CAPTURED BEFORE ANY shift.
rem  PHASE_UPLIFT.md section 12: `shift` moves %0 too, so afterwards
rem  `%~dp0` resolves to the CALLER's directory and the sibling goes
rem  missing.
set "HERE=%~dp0"


set "RC=0"
set "FILE="
set "OUT="
set "NOOPEN="

:parse
if "%~1"=="" goto :parsed
if /i "%~1"=="--out"     set "OUT=%~2" & shift & shift & goto :parse
if /i "%~1"=="--no-open" set "NOOPEN=1" & shift & goto :parse
if not defined FILE set "FILE=%~1"
shift
goto :parse

:parsed
if "%FILE%"=="" set "FILE=C:\Source\HamLet\PHASE_OUTCOME.md"
if "%OUT%"=="" set "OUT=%TEMP%\phase-outcome.html"

echo.
echo ============================================================
echo  outcome-render
echo    from : %FILE%
echo    to   : %OUT%
echo ============================================================
echo.

if not exist "%FILE%" (
  echo   ABSENT - no phase outcome file at that path.
  echo   A phase that has not started has nothing to render, which is
  echo   not an error. Absent and malformed are different facts.
  set "RC=2"
  goto :end
)

rem  READ THROUGH readkey.bat, NOT findstr. PHASE_UPLIFT.md section 12.
set "PHASEHDR="
call "%HERE%readkey.bat" "%FILE%" "PHASE" PHASEHDR
if not defined PHASEHDR (
  echo   MALFORMED - no PHASE: line. Nothing below it can be trusted.
  set "RC=1"
  goto :end
)

powershell -NoProfile -Command "$src='%FILE%'; $dst='%OUT%'; $t = Get-Content -LiteralPath $src; $ok = 'not started','in progress','partial','blocked','done'; $phase = ($t | Where-Object { $_ -match '^PHASE: ' } | Select-Object -First 1); if($phase){ $phase = $phase.Substring(7).Trim() } else { $phase = 'unnamed' }; $steps = @(); foreach($l in $t){ if($l -match '^STEP: '){ $p = $l.Substring(6).Split('|'); if($p.Count -ge 2 -and $ok -contains $p[1].Trim()){ $steps += [pscustomobject]@{ N=$p[0].Trim(); S=$p[1].Trim(); W=$(if($p.Count -gt 2){$p[2].Trim()}else{''}); Did=''; Cost=''; Units=0 } } } }; $cur=''; foreach($l in $t){ if($l -match '^## UNIT (\S+) . STEP (\S+)\s*$'){ $cur = $matches[2] ; foreach($s in $steps){ if($s.N -eq $cur){ $s.Units = $s.Units + 1 } } }; if($l -match '^ACCOMPLISHED: (.*)$'){ foreach($s in $steps){ if($s.N -eq $cur){ $s.Did = $matches[1] } } }; if($l -match '^COST: (.*)$'){ foreach($s in $steps){ if($s.N -eq $cur){ $s.Cost = $matches[1] } } } }; $decided = @($t | Where-Object { $_ -match '^DECIDED: ' } | ForEach-Object { $_.Substring(9).Trim() } | Where-Object { $_ -ne 'none' -and $_ -ne 'not recorded' }); $done = @($steps | Where-Object { $_.S -eq 'done' }).Count; $part = @($steps | Where-Object { $_.S -eq 'partial' }).Count; $blk = @($steps | Where-Object { $_.S -eq 'blocked' }).Count; $ns = @($steps | Where-Object { $_.S -eq 'not started' }).Count; $tot = $steps.Count; $lead = \"$done of $tot steps done\"; if($part){ $lead = $lead + \", $part partial\" }; if($blk){ $lead = $lead + \", $blk blocked\" }; if($ns){ $lead = $lead + \", $ns not started\" }; $lead = $lead + '.'; $second = if($blk){ 'A blocked step is the thing to look at first.' } elseif($part){ 'The partial step is the one outstanding.' } elseif($ns){ 'What remains has not been started.' } else { 'Nothing is outstanding.' }; $col = @{ 'done'='#1d6b32'; 'partial'='#8a6100'; 'blocked'='#8f2020'; 'not started'='#5a5a5a'; 'in progress'='#3f4a55' }; $bg = @{ 'done'='#e8f4ea'; 'partial'='#fbf1dc'; 'blocked'='#f8e7e7'; 'not started'='#f0f0f0'; 'in progress'='#eceff2' }; $h = New-Object System.Collections.ArrayList; [void]$h.Add('<!doctype html><html><head><meta charset=\"utf-8\"><title>Phase outcome</title><style>'); [void]$h.Add('body{font:15px/1.5 -apple-system,Segoe UI,Roboto,sans-serif;margin:0;padding:32px;background:#fafaf8;color:#1a1a1a}'); [void]$h.Add('h1{font-size:19px;margin:0 0 4px}.ph{color:#666;font-size:13px;margin:0 0 20px}'); [void]$h.Add('.lead{font-size:17px;line-height:1.45;max-width:52em;margin:0 0 26px}'); [void]$h.Add('.cards{display:flex;flex-wrap:wrap;gap:14px;margin-bottom:28px}'); [void]$h.Add('.card{flex:1 1 260px;max-width:340px;border-radius:8px;padding:14px 16px;border:1px solid rgba(0,0,0,.10)}'); [void]$h.Add('.step{font-size:11px;letter-spacing:.09em;text-transform:uppercase;font-weight:700;opacity:.85}'); [void]$h.Add('.state{float:right;font-size:11px;letter-spacing:.06em;text-transform:uppercase;font-weight:700}'); [void]$h.Add('.what{font-weight:600;margin:8px 0 6px}.did{font-size:14px;margin:0 0 10px}'); [void]$h.Add('.cost{font-size:12px;opacity:.75;font-variant-numeric:tabular-nums}'); [void]$h.Add('.strip{border-top:1px solid #ddd;padding-top:16px;display:flex;flex-wrap:wrap;gap:26px;font-size:13px}'); [void]$h.Add('.strip h2{font-size:11px;letter-spacing:.09em;text-transform:uppercase;color:#666;margin:0 0 6px}'); [void]$h.Add('.strip ul{margin:0;padding-left:18px}.strip div{flex:1 1 240px}'); [void]$h.Add('</style></head><body>'); [void]$h.Add('<h1>Phase outcome</h1>'); [void]$h.Add('<p class=\"ph\">' + ($phase -replace '&','&amp;' -replace '<','&lt;' -replace '>','&gt;') + '</p>'); [void]$h.Add('<p class=\"lead\">' + $lead + ' ' + $second + '</p>'); [void]$h.Add('<div class=\"cards\">'); foreach($s in $steps){ $c = $col[$s.S]; $b = $bg[$s.S]; $did = if($s.Did){ $s.Did } else { 'nothing recorded yet' }; $did = ($did -replace '&','&amp;' -replace '<','&lt;' -replace '>','&gt;'); $w = ($s.W -replace '&','&amp;' -replace '<','&lt;' -replace '>','&gt;'); $cost = if($s.Cost){ $s.Cost } else { 'unknown' }; $u = if($s.Units -eq 1){ '1 unit' } else { \"$($s.Units) units\" }; [void]$h.Add('<div class=\"card\" style=\"background:' + $b + ';border-left:5px solid ' + $c + '\">'); [void]$h.Add('<span class=\"step\">Step ' + $s.N + '</span><span class=\"state\" style=\"color:' + $c + '\">' + $s.S + '</span>'); [void]$h.Add('<div class=\"what\">' + $w + '</div>'); [void]$h.Add('<div class=\"did\">' + $did + '</div>'); [void]$h.Add('<div class=\"cost\">' + $cost + ' &middot; ' + $u + '</div></div>') }; [void]$h.Add('</div>'); [void]$h.Add('<div class=\"strip\">'); [void]$h.Add('<div><h2>Decided for you</h2><ul>'); if($decided.Count){ foreach($d in $decided){ [void]$h.Add('<li>' + ($d -replace '&','&amp;' -replace '<','&lt;' -replace '>','&gt;') + '</li>') } } else { [void]$h.Add('<li>Nothing was decided on the arbiter&rsquo;s authority.</li>') }; [void]$h.Add('</ul></div>'); [void]$h.Add('<div><h2>Waiting on you</h2><ul>'); if($blk){ [void]$h.Add('<li>' + $blk + ' blocked step(s) need a ruling.</li>') } else { [void]$h.Add('<li>Nothing.</li>') }; [void]$h.Add('</ul></div>'); [void]$h.Add('<div><h2>Where to look</h2><ul><li>' + $src + '</li><li>RUN_LEDGER.md</li><li>tools\arbiter\outcome-read.bat</li></ul></div>'); [void]$h.Add('</div></body></html>'); Set-Content -LiteralPath $dst -Value ($h -join [Environment]::NewLine) -Encoding utf8; Write-Host ('CARDS=' + $steps.Count)"

if not exist "%OUT%" (
  echo   ERROR: no page was produced at %OUT%
  set "RC=3"
  goto :end
)

rem  A PARTIAL PAGE IS A FAILED RENDER, NOT A RENDER. The first cut
rem  called [System.Web.HttpUtility]::HtmlEncode, which is not
rem  loaded by default in Windows PowerShell; the call threw, the
rem  phase name was silently dropped from the page, and this script
rem  still printed "Rendered" and exited 0. A renderer that reports
rem  success over a page missing an element is worse than one that
rem  fails, because the owner reads the page and not the exit code.
rem  So the card count is checked against the step count.
set "STEPCOUNT=0"
for /f "usebackq delims=" %%N in (`powershell -NoProfile -Command "$ok='not started','in progress','partial','blocked','done'; @(Select-String -Path '%FILE%' -Pattern '^STEP: ' | ForEach-Object { $_.Line } | Where-Object { $p=$_.Substring(6).Split('|'); $p.Count -ge 2 -and $ok -contains $p[1].Trim() }).Count"`) do set "STEPCOUNT=%%N"
set "CARDCOUNT=0"
for /f "usebackq delims=" %%N in (`powershell -NoProfile -Command "@(Select-String -Path '%OUT%' -Pattern 'class=.card. style=' -AllMatches).Count"`) do set "CARDCOUNT=%%N"
echo   steps   : %STEPCOUNT%
echo   cards   : %CARDCOUNT%
if not "%STEPCOUNT%"=="%CARDCOUNT%" (
  echo.
  echo   ERROR: the page has %CARDCOUNT% cards for %STEPCOUNT% steps.
  echo   A partial page is a failed render. Not reporting success over it.
  set "RC=3"
  goto :end
)

echo   Rendered: %OUT%
for %%Z in ("%OUT%") do echo   Size    : %%~zZ bytes
if not defined NOOPEN (
  echo   Opening it...
  start "" "%OUT%"
)
set "RC=0"
goto :end

rem ============================================================
:end
echo.
echo outcome-render exit %RC%
endlocal & exit /b %RC%
