@echo off
rem ============================================================
rem  outcome-append.bat  -  one unit's contribution to the phase
rem                         record
rem
rem      outcome-append.bat <unit> <step> <state> <approach> <hit>
rem                         <move> <why> <decided> <licence> <cost>
rem                         <accomplished> [file]
rem
rem      0  appended, and the step's state updated in the header
rem      2  a required argument is missing
rem      3  the file could not be written
rem      4  <state> is not one of the five
rem      5  <fate> is not one of the three
rem
rem  THE FOURTEENTH ARGUMENT IS THE STATE'S REASON, and it is
rem  optional. Added 2026-08-30 by 048, whose task 1 found that this
rem  script could record a state and nowhere to say why - the entry
rem  wrote STATE_AFTER and nothing beside it, and the header's third
rem  column is carried over from the existing line rather than
rem  supplied. From 048 the state after a run is a JUDGMENT made by
rem  a session reading the report against the plan's exit criteria,
rem  and a judgment recorded without its reason is a verdict with no
rem  evidence - 045 made the same argument when it printed stop 6's
rem  count and its verdict on separate lines.
rem
rem  It defaults to "not recorded", so every caller written before
rem  048 keeps working unchanged and says so on its face.
rem
rem  Eleven arguments and none of them invented. PHASE_CONTROL.md
rem  section 5 names what a unit records: the step, the approach
rem  taken, what it hit, the move chosen and its reasoning, any
rem  decision made on the arbiter's authority and what licensed
rem  it, and the cost. ACCOMPLISHED is the twelfth thing section 5
rem  asks for in its own words - "what the step accomplished, not
rem  what it did" - and it is the line the card carries.
rem
rem  NO CLOCK. The caller knows when its unit began and this
rem  script does not. A timestamp invented at append time is a
rem  composed timestamp, and CLAUDE_CODE.md section 11 records two
rem  of those, one of them thirty-nine seconds into the future.
rem
rem  TWO REGIONS, TWO RULES, and this script obeys both.
rem
rem    THE ENTRY IS APPENDED. It is never rewritten. A unit that
rem    went wrong gets ANOTHER entry, not a corrected one - the
rem    same argument CLAUDE.md section 1 makes about a ruling row
rem    being superseded rather than amended.
rem
rem    THE STEP'S STATE IN THE HEADER IS UPDATED IN PLACE. That is
rem    the file's one mutable region and PHASE_OUTCOME.md argues
rem    why: a step state is a RUNNING POSITION, not a record of an
rem    event. Step 3 blocked this morning and done this evening is
rem    one fact changing, not two.
rem
rem  Both happen in the same call, because a header that lags the
rem  entries is a position nobody can trust, and a caller that has
rem  to remember a second command is a caller that will forget.
rem
rem  IT CREATES THE FILE ON FIRST APPEND, header and all, the way
rem  ledger.bat does. A record that depends on somebody having set
rem  it up first is a record with a gap at the beginning.
rem
rem  COST IS NEVER 0. Unknown is the non-claim; a zero claims the
rem  run was free. 040's ruling, one file over.
rem
rem  Batch, not PowerShell: a .ps1 will not run on this machine,
rem  unsigned scripts are blocked by execution policy. The inline
rem  powershell -NoProfile -Command calls are not script files and
rem  are used where cmd cannot rewrite one line of a file in place.
rem
rem  Generated 2026-08-29 for: work instructions 041 task 4
rem ============================================================

setlocal

rem  THIS SCRIPT'S OWN DIRECTORY, TAKEN BEFORE ANY `shift`.
rem  `shift` shifts %%0 along with the rest, and there are five of them
rem  below, so `%%~dp0` further down resolves to the CALLER's directory
rem  and not to this file's. Measured: it produced
rem  C:\Source\HamLet\outcome-entry.py, which does not exist.
set "HERE=%~dp0"

set "RC=0"
set "UNIT=%~1"
set "STEP=%~2"
set "STATE=%~3"
set "APPROACH=%~4"
set "HIT=%~5"
set "MOVE=%~6"
set "WHY=%~7"
set "DECIDED=%~8"
set "LICENCE=%~9"
shift
shift
set "COST=%~8"
set "ACCOMPLISHED=%~9"
shift
set "FILE=%~9"
shift
set "FATE=%~9"
shift
set "STATEWHY=%~9"

if "%UNIT%"==""         goto :usage
if "%STEP%"==""         goto :usage
if "%STATE%"==""        goto :usage
if "%APPROACH%"==""     goto :usage
if "%HIT%"==""          goto :usage
if "%MOVE%"==""         goto :usage
if "%WHY%"==""          goto :usage
if "%DECIDED%"==""      goto :usage
if "%LICENCE%"==""      goto :usage
if "%ACCOMPLISHED%"=="" goto :usage
if "%COST%"=="" set "COST=unknown"
if "%FILE%"=="" set "FILE=C:\Source\HamLet\PHASE_OUTCOME.md"
if "%FATE%"=="" set "FATE=not recorded"
if "%STATEWHY%"=="" set "STATEWHY=not recorded"

rem --- the fate must be one of the three -------------------------
rem  WHAT THE STEP STATE CANNOT SAY. STATE is where the step now
rem  stands; FATE is what happened to the RUN. They are different
rem  facts and 046 proved it the hard way: both its units were
rem  thrown away before they read their instructions, and the
rem  entries recorded "not started" - which is true of the step and
rem  says nothing about the night. Its second arbiter had to INFER
rem  from "no output.md and no tree change" what an entry should
rem  have told it.
rem
rem  THE TWO LEAD TO OPPOSITE MOVES. An approach that was executed
rem  and did not work is evidence about the approach - try another.
rem  A run that never reached its instruction is evidence about the
rem  harness - the approach is untested and repeating it is not a
rem  loop.
rem
rem  A VOCABULARY, NOT FREE TEXT, for the same reason the five step
rem  states are: the arbiter reads this field and a field it has to
rem  interpret is a field it can interpret wrongly.
set "FOK=0"
if /i "%FATE%"=="executed"     set "FOK=1"
if /i "%FATE%"=="never ran"    set "FOK=1"
if /i "%FATE%"=="not recorded" set "FOK=1"
if "%FOK%"=="1" goto :fateok
echo ERROR: fate not recognised: %FATE%
echo The three are: executed, never ran, not recorded.
echo A fourth word would be one the arbiter cannot act on, so it is
echo refused here rather than written - the same argument the five
echo states are refused on, one field over.
set "RC=5"
goto :end
:fateok

rem --- the state must be one of the five -------------------------
set "OK="
if /i "%STATE%"=="not started" set "OK=1"
if /i "%STATE%"=="in progress" set "OK=1"
if /i "%STATE%"=="partial"     set "OK=1"
if /i "%STATE%"=="blocked"     set "OK=1"
if /i "%STATE%"=="done"        set "OK=1"
if not defined OK (
  echo.
  echo ERROR: "%STATE%" is not a step state.
  echo The five are: not started, in progress, partial, blocked, done.
  echo A sixth word would be one the render cannot colour and the
  echo reader cannot count, so it is refused here rather than written.
  set "RC=4"
  goto :end
)

echo.
echo ============================================================
echo  outcome-append
echo    file : %FILE%
echo    unit : %UNIT%   step : %STEP%   state : %STATE%
echo ============================================================
echo.

rem --- create on first append ------------------------------------
rem  NO STEP LINE IS WRITTEN BY THIS BRANCH, and there is no
rem  "goto :entry" out of it. The header-update block below is the ONE
rem  place step lines are written, and on a file this branch has just
rem  created it finds none and initialises the whole list from
rem  PHASE_PLAN.md. Writing a single step line here was the same defect
rem  in miniature: a header listing one step of five is a planned count
rem  of one, which is how a five-step phase read "satisfied" with four
rem  steps untouched.
if not exist "%FILE%" (
  echo   No phase outcome file yet. Creating it with its header.
  >"%FILE%" echo # PHASE_OUTCOME.md
  >>"%FILE%" echo.
  >>"%FILE%" echo **The phase's memory. Accumulated as the phase runs, never assembled at
  >>"%FILE%" echo the end** - `output.md` is overwritten every unit, so anything not captured
  >>"%FILE%" echo while it runs is gone. `PHASE_CONTROL.md` section 5.
  >>"%FILE%" echo.
  >>"%FILE%" echo The phase header below is UPDATED IN PLACE - a step state is a running
  >>"%FILE%" echo position, not a record of an event. Everything under `## UNIT` is
  >>"%FILE%" echo APPEND-ONLY and no recorded fact is ever changed.
  >>"%FILE%" echo.
  >>"%FILE%" echo Read it with `tools\arbiter\outcome-read.bat`, render it with
  >>"%FILE%" echo `tools\arbiter\outcome-render.bat`.
  >>"%FILE%" echo.
  >>"%FILE%" echo ## PHASE
  >>"%FILE%" echo.
  >>"%FILE%" echo PHASE: unnamed - created by outcome-append.bat on first append
  >>"%FILE%" echo PHASE_SET: unknown
  >>"%FILE%" echo.
  >>"%FILE%" echo ---
  if not exist "%FILE%" (
    echo   ERROR: could not create %FILE%
    set "RC=3"
    goto :end
  )
)

rem --- update this step's state in the header, in place ----------
rem  The ONE mutable region. If no STEP line for this step exists
rem  yet, one is added to the header rather than silently skipped:
rem  a step being worked that the header does not list is a
rem  position the header is wrong about.
rem
rem  IT ANCHORS ON THE HEADER REGION, WHICH IS THE WHOLE OF THE FIX.
rem  The region is the lines BELOW `PHASE_SET:` and ABOVE the first
rem  `---` rule or `##` heading after it, and never a line inside a
rem  fenced block. Nothing outside that window is read and nothing
rem  outside it is written.
rem
rem  WHAT IT USED TO DO, AND WHY IT WAS WRONG. It searched the WHOLE
rem  file for the last `^STEP: ` line and wrote beneath it. Measured
rem  on 2026-08-31 against the live file, that last line is line 96,
rem  inside the `## UNIT 1 - STEP 1` entry - so the writer would have
rem  written into the append-only region, which is the one region this
rem  script's own header says is never rewritten. Measured against the
rem  file as it stood when the fault fired, with no entries yet, the
rem  last line was the `STEP: <n> | <state> | ...` example inside the
rem  fenced FORMAT DOCUMENTATION - which is where it actually landed,
rem  and which the owner's ruling of 2026-08-30 names. BOTH LANDING
rem  PLACES ARE REAL and which one fires depends only on whether any
rem  unit has run yet. Both are wrong the same way and this is the one
rem  fix for both.
rem
rem  AND IT INITIALISES FROM PHASE_PLAN.md. Where the header region
rem  carries no step lines at all, every step line is written from the
rem  plan's machine-readable block - all of them, states `not started`,
rem  except this call's own step, which takes this call's state. A
rem  header that was never initialised is what let a five-step phase
rem  report planned 1, done 1, open 0 and halt as `satisfied` with four
rem  steps untouched. The plan's form is `STEP: <n> | <what it
rem  delivers>` and it is the same anchored, colon-bearing form this
rem  file's header uses - one form across the plan, the outcome and
rem  PHASE_STATUS.md.
powershell -NoProfile -Command "$f='%FILE%'; $n='%STEP%'; $s='%STATE%'; $t=@(Get-Content -LiteralPath $f); $fen=@(); $inf=$false; for($i=0;$i -lt $t.Count;$i++){ if($t[$i] -match '^\s*(```|~~~)'){ $inf=-not $inf; $fen+=$true } else { $fen+=$inf } }; $hs=-1; for($i=0;$i -lt $t.Count;$i++){ if(-not $fen[$i] -and $t[$i] -match '^PHASE_SET:'){ $hs=$i; break } }; if($hs -lt 0){ exit }; $he=$t.Count; for($i=$hs+1;$i -lt $t.Count;$i++){ if(-not $fen[$i] -and ($t[$i] -match '^-{3,}\s*$' -or $t[$i] -match '^#{1,6}\s')){ $he=$i; break } }; $found=-1; $last=-1; for($i=$hs+1;$i -lt $he;$i++){ if(-not $fen[$i] -and $t[$i] -match '^STEP: *[0-9]+ *\|'){ $last=$i; if($t[$i] -match ('^STEP: *' + [regex]::Escape($n) + ' *\|')){ $found=$i } } }; if($found -ge 0){ $p=$t[$found].Substring(5).Split('|'); $what=if($p.Count -gt 2){ $p[2].Trim() } else { '' }; $t[$found]='STEP: '+$n+' | '+$s+' | '+$what } else { $new=@(); $at=$hs; if($last -ge 0){ $new=@('STEP: '+$n+' | '+$s+' | (described by the plan)'); $at=$last } else { $plan=Join-Path (Split-Path -Parent $f) 'PHASE_PLAN.md'; if(Test-Path -LiteralPath $plan){ foreach($ln in (Get-Content -LiteralPath $plan)){ if($ln -match '^STEP: *([0-9]+) *\| *(.*)$'){ $sn=$Matches[1]; $sw=$Matches[2].Trim(); $st=if($sn -eq $n){ $s } else { 'not started' }; $new+=('STEP: '+$sn+' | '+$st+' | '+$sw) } } }; if($new.Count -eq 0){ $new=@('STEP: '+$n+' | '+$s+' | (described by the plan)') } }; $out=@(); for($i=0;$i -lt $t.Count;$i++){ $out+=$t[$i]; if($i -eq $at){ $out+=$new } }; $t=$out }; [IO.File]::WriteAllText($f, (($t -join [char]13 + [char]10) + [char]13 + [char]10), (New-Object Text.UTF8Encoding $false))"

rem --- the entry, appended and never rewritten -------------------
rem  The `:entry` label that stood here is gone with the `goto :entry`
rem  that reached it. A freshly created file now falls through the
rem  header-update block above, which is what initialises its step
rem  lines from PHASE_PLAN.md.
rem  The values are handed to PowerShell through the environment and
rem  never on its command line. A value carrying a quote, a caret, a
rem  percent or an ampersand is ordinary English in these fields, and
rem  every one of those is a metacharacter to cmd's parser on the way
rem  into a command line. The environment is the one channel that
rem  carries a string across the process boundary untouched.
set "OA_UNIT=%UNIT%"
set "OA_STEP=%STEP%"
set "OA_STATE=%STATE%"
set "OA_APPROACH=%APPROACH%"
set "OA_HIT=%HIT%"
set "OA_MOVE=%MOVE%"
set "OA_WHY=%WHY%"
set "OA_DECIDED=%DECIDED%"
set "OA_LICENCE=%LICENCE%"
set "OA_COST=%COST%"
set "OA_ACCOMPLISHED=%ACCOMPLISHED%"
set "OA_FATE=%FATE%"
set "OA_STATEWHY=%STATEWHY%"

rem  WRITTEN THROUGH POWERSHELL AND TRANSLITERATED TO ASCII, AND THE
rem  REASON IS A MEASUREMENT. The lines below used to be
rem      >>"%FILE%" echo WHY: %WHY%
rem  and cmd's `echo` emits bytes in the CONSOLE's active codepage, not
rem  in the file's. On 2026-08-31 this file held SEVEN copies of the
rem  byte run 83 3F 27 where an em-dash belonged - `0x83` is `a` with a
rem  circumflex in CP437/CP850, `3F` is the `?` an unmappable character
rem  becomes, and `27` is a best-fit apostrophe. That is a UTF-8
rem  punctuation character decoded as CP1252 and re-encoded through the
rem  OEM codepage, and it is IRREVERSIBLE: the three surviving bytes do
rem  not say which character they came from.
rem
rem  SO THE FIX IS NOT A BETTER CODEPAGE, IT IS NO NON-ASCII AT ALL.
rem  A record does not need a typographic dash, and `chcp 65001` would
rem  only move the mangling to whoever fed the variable - the value has
rem  already crossed a codepage boundary by the time this script sees
rem  it. Transliterating here makes the corruption impossible instead
rem  of less likely, and a run of non-ASCII is replaced by the nearest
rem  ASCII rather than dropped, so nothing silently disappears.
rem
rem  AND IT WRITES NO BOM. `Set-Content -Encoding utf8` on Windows
rem  PowerShell 5.1 writes UTF-8 WITH a byte-order mark, which put
rem  EF BB BF in front of `PHASE:` on line 1 - and a header parser
rem  anchored on `^PHASE:` or on `^[A-Za-z_]` does not match a line
rem  that starts with a BOM. Both writes in this file now go through
rem  UTF8Encoding($false).
rem  THE ENTRY IS WRITTEN BY PYTHON, AND THE REASON IS QUOTING.
rem  These values are ordinary English and carry quotes, ampersands,
rem  percent signs and carets, every one of which is a metacharacter on
rem  the way into a `powershell -Command` line. Two attempts at escaping
rem  them here failed, and CLAUDE_CODE.md 11 names composing file content
rem  inside nested shell quoting as a recurring corruption in this repo.
rem  The values go in the ENVIRONMENT, which carries a string across a
rem  process boundary untouched, and outcome-entry.py states the encoding
rem  it writes. See that file for the byte evidence.
python "%HERE%outcome-entry.py" "%FILE%"
if errorlevel 1 (
  echo   ERROR: could not append the entry to %FILE%
  set "RC=3"
  goto :end
)

echo   Appended:
echo     ## UNIT %UNIT% - STEP %STEP%
echo     APPROACH: %APPROACH%
echo     ACCOMPLISHED: %ACCOMPLISHED%
echo     COST: %COST%
echo     FATE: %FATE%
echo.
echo   Step %STEP% is now [%STATE%] in the phase header.
echo   Nothing above the new entry was touched.
set "RC=0"
goto :end

rem ============================================================
:usage
echo.
echo   outcome-append.bat ^<unit^> ^<step^> ^<state^> ^<approach^> ^<hit^>
echo                      ^<move^> ^<why^> ^<decided^> ^<licence^> ^<cost^>
echo                      ^<accomplished^> [file]
echo.
echo   state: not started ^| in progress ^| partial ^| blocked ^| done
echo   cost : total_cost_usd, or unknown. NEVER 0.
echo   Quote every argument.
echo.
echo   0 appended, 2 a required argument missing, 3 write failed,
echo   4 the state is not one of the five
echo.
set "RC=2"
goto :end

rem ============================================================
:end
echo.
echo outcome-append exit %RC%
endlocal & exit /b %RC%
