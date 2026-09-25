@echo off
rem ============================================================
rem  outcome-append.bat  -  one unit's contribution to the phase
rem                         record
rem
rem      outcome-append.bat <unit> <step> <state> <approach> <hit>
rem                         <move> <why> <decided> <licence> <cost>
rem                         <accomplished> [file] [fate] [state-why]
rem
rem  THE FILE IS THE TWELFTH ARGUMENT AND THE FATE THE THIRTEENTH, and
rem  anything that moves an argument up one place puts the reason where
rem  the fate goes - which this script then refuses at exit 5. That is
rem  the refusal reported on HamLet on 2026-09-12, "fate not recognised"
rem  followed by the state judge's sentence, and unit 061 REPRODUCED IT
rem  from run-phase.bat: a percent sign in the arbiter's APPROACH and a
rem  colon later on the call line made call delete the text between them,
rem  quotes and all. See --from-env below, which is how run-phase.bat now
rem  passes its values. UNTIL 061 NEITHER THIS LINE NOR THE USAGE TEXT
rem  NAMED THE LAST TWO ARGUMENTS either.
rem
rem      0  appended, and the step's state updated in the header
rem      2  a required argument is missing
rem      3  the file could not be written
rem      4  <state> is not one of the five
rem      5  <fate> is not one of the three
rem      6  ADVANCED is not one of the four - --from-env only, since 064
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

set "RC=0"
rem  THE ATTEMPT LINE'S TWO VALUES, 066 task 3, are empty for every caller
rem  but --from-env, which sets them below. A positional caller never writes
rem  an attempt, even with OA_CRITERION left over in its environment.
set "ATCRIT="
set "ATAPPF="
if /i "%~1"=="--from-env" goto :fromenv
if /i "%~1"=="--open" goto :open
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
goto :argsread

rem  --open: OPEN A PHASE'S RECORD AND APPEND NOTHING. 066 task 1. Until
rem  066 this header was written only on a unit's first append, so a phase
rem  could not be opened before its first unit except by a hand writing a
rem  file this script owns, or by a false entry. --open runs the same
rem  create branch and the same header-initialisation block below - every
rem  step line from PHASE_PLAN.md at not started - and writes no UNIT
rem  entry. It refuses a file that already exists: a record is never
rem  replaced. Author's, overrulable.
rem
rem      outcome-append.bat --open [phase] [phase-set] [file]
rem
rem  No parentheses in the phase text: it is echoed inside the create
rem  branch's parenthesised block.
:open
set "OPEN=1"
set "OPHASE=%~2"
set "OPHASESET=%~3"
set "FILE=%~4"
if "%FILE%"=="" set "FILE=C:\Source\ClaudeProjectStatus\PHASE_OUTCOME.md"
if not exist "%FILE%" goto :openok
echo ERROR: %FILE% already exists - a phase record is never replaced.
set "RC=7"
goto :end
:openok
set "STEP=1"
set "STATE=not started"
goto :openfile

rem  --from-env: THE FOURTEEN VALUES COME FROM OA_ ENVIRONMENT VARIABLES,
rem  NOT FROM THE COMMAND LINE. 061 task 3. run-phase.bat passes them this
rem  way because call expands percent signs a second time, and a percent
rem  sign in one value followed later on the line by a colon deletes
rem  everything between the two, closing quotes included. On HamLet that
rem  fused APPROACH with HIT, moved every later argument up one place, and
rem  put the state judge's sentence where the fate goes. A set line
rem  expands once, so a value read here arrives whole whatever is in it.
rem  The positional form above is unchanged for every caller that uses it.
:fromenv
set "UNIT=%OA_UNIT%"
set "STEP=%OA_STEP%"
set "STATE=%OA_STATE%"
set "APPROACH=%OA_APPROACH%"
set "HIT=%OA_HIT%"
set "MOVE=%OA_MOVE%"
set "WHY=%OA_WHY%"
set "DECIDED=%OA_DECIDED%"
set "LICENCE=%OA_LICENCE%"
set "COST=%OA_COST%"
set "ACCOMPLISHED=%OA_ACCOMPLISHED%"
set "FILE=%OA_FILE%"
set "FATE=%OA_FATE%"
set "STATEWHY=%OA_STATEWHY%"
set "ADVANCED=%OA_ADVANCED%"
set "ATCRIT=%OA_CRITERION%"
set "ATAPPF=%OA_APPROACHFILE%"
set "ATID=%OA_ATTEMPTID%"

:argsread
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
if "%FILE%"=="" set "FILE=C:\Source\ClaudeProjectStatus\PHASE_OUTCOME.md"
if "%FATE%"=="" set "FATE=not recorded"
if "%STATEWHY%"=="" set "STATEWHY=not recorded"
if "%ADVANCED%"=="" set "ADVANCED=not recorded"

rem --- ADVANCED must be one of four ------------------------------
rem  WHETHER THE UNIT MOVED THE PLAN. 064 task 3. The owner's ruling of
rem  2026-09-14: a unit may claim only criterion k of step N, unmet to
rem  met, and run-phase.bat counts that with criteria-count.bat.
rem    yes           the criterion ADVANCES named flipped, and the state
rem                  judge found it honestly met
rem    no            it did not flip, or the judge found it not honestly met
rem    blocker       ADVANCES was the blocker form - no criterion claimed
rem    not recorded  nobody said; every positional caller, and any entry
rem                  written before 064
rem
rem  ITS OWN FIELD, NOT THE FATE. The instruction asked for no advance to
rem  go in the fate field. FATE's three words say what happened to the RUN
rem  and ARBITER.md section 8 reads them that way - executed is evidence
rem  about an approach, never ran is evidence about the harness. A unit can
rem  be executed AND move nothing, and those are two facts. Author's,
rem  overrulable. One word per value, as FATE is, and refused otherwise.
set "AOK="
if /i "%ADVANCED%"=="yes"          set "AOK=1"
if /i "%ADVANCED%"=="no"           set "AOK=1"
if /i "%ADVANCED%"=="blocker"      set "AOK=1"
if /i "%ADVANCED%"=="not recorded" set "AOK=1"
if defined AOK goto :advancedok
echo ERROR: advanced not recognised: %ADVANCED%
echo The four are: yes, no, blocker, not recorded.
set "RC=6"
goto :end
:advancedok

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
rem
rem  --open ENTERS HERE with its own phase text and date; every other
rem  caller falls through and gets the defaults this branch always wrote.
:openfile
if not defined OPHASE set "OPHASE=unnamed - created by outcome-append.bat on first append"
if not defined OPHASESET set "OPHASESET=unknown"
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
  >>"%FILE%" echo PHASE: %OPHASE%
  >>"%FILE%" echo PHASE_SET: %OPHASESET%
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
powershell -NoProfile -Command "$f='%FILE%'; $n='%STEP%'; $s='%STATE%'; $t=@(Get-Content -LiteralPath $f); $fen=@(); $inf=$false; for($i=0;$i -lt $t.Count;$i++){ if($t[$i] -match '^\s*(```|~~~)'){ $inf=-not $inf; $fen+=$true } else { $fen+=$inf } }; $hs=-1; for($i=0;$i -lt $t.Count;$i++){ if(-not $fen[$i] -and $t[$i] -match '^PHASE_SET:'){ $hs=$i; break } }; if($hs -lt 0){ exit }; $he=$t.Count; for($i=$hs+1;$i -lt $t.Count;$i++){ if(-not $fen[$i] -and ($t[$i] -match '^-{3,}\s*$' -or $t[$i] -match '^#{1,6}\s')){ $he=$i; break } }; $found=-1; $last=-1; for($i=$hs+1;$i -lt $he;$i++){ if(-not $fen[$i] -and $t[$i] -match '^STEP: *[0-9]+ *\|'){ $last=$i; if($t[$i] -match ('^STEP: *' + [regex]::Escape($n) + ' *\|')){ $found=$i } } }; if($found -ge 0){ $p=$t[$found].Substring(5).Split('|'); $what=if($p.Count -gt 2){ $p[2].Trim() } else { '' }; $t[$found]='STEP: '+$n+' | '+$s+' | '+$what } else { $new=@(); $at=$hs; if($last -ge 0){ $new=@('STEP: '+$n+' | '+$s+' | (described by the plan)'); $at=$last } else { $plan=Join-Path (Split-Path -Parent $f) 'PHASE_PLAN.md'; if(Test-Path -LiteralPath $plan){ foreach($ln in (Get-Content -LiteralPath $plan)){ if($ln -match '^STEP: *([0-9]+) *\| *(.*)$'){ $sn=$Matches[1]; $sw=$Matches[2].Trim(); $st=if($sn -eq $n){ $s } else { 'not started' }; $new+=('STEP: '+$sn+' | '+$st+' | '+$sw) } } }; if($new.Count -eq 0){ $new=@('STEP: '+$n+' | '+$s+' | (described by the plan)') } }; $out=@(); for($i=0;$i -lt $t.Count;$i++){ $out+=$t[$i]; if($i -eq $at){ $out+=$new } }; $t=$out }; Set-Content -LiteralPath $f -Value $t -Encoding utf8"

rem --- --open stops here: the header is written and no entry is ----
if not defined OPEN goto :appendentry
echo   Opened %FILE%: the phase header, and every step line from
echo   PHASE_PLAN.md at not started. No UNIT entry was written.
set "RC=0"
goto :end
:appendentry

rem --- the entry, appended and never rewritten -------------------
rem  The `:entry` label that stood here is gone with the `goto :entry`
rem  that reached it. A freshly created file now falls through the
rem  header-update block above, which is what initialises its step
rem  lines from PHASE_PLAN.md.
>>"%FILE%" echo.
>>"%FILE%" echo ## UNIT %UNIT% - STEP %STEP%
>>"%FILE%" echo.
>>"%FILE%" echo STEP: %STEP%
>>"%FILE%" echo APPROACH: %APPROACH%
>>"%FILE%" echo HIT: %HIT%
>>"%FILE%" echo MOVE: %MOVE%
>>"%FILE%" echo WHY: %WHY%
>>"%FILE%" echo DECIDED: %DECIDED%
>>"%FILE%" echo LICENCE: %LICENCE%
>>"%FILE%" echo COST: %COST%
>>"%FILE%" echo ACCOMPLISHED: %ACCOMPLISHED%
>>"%FILE%" echo FATE: %FATE%
>>"%FILE%" echo STATE_AFTER: %STATE%
>>"%FILE%" echo STATE_WHY: %STATEWHY%
>>"%FILE%" echo ADVANCED: %ADVANCED%

rem --- the attempt, one line, in this entry ----------------------
rem  066 task 3. Only --from-env reaches here with ATCRIT set, and
rem  run-phase.bat sets it only for a unit run against a criterion - the
rem  one ADVANCES names, or the one a blocker-clear names as unblocked.
rem  The form tools\arbiter\attempt-read.bat reads:
rem
rem      ATTEMPT: <N.k> | unit <u> launched <id> | <advanced> | <fate> | <approach>
rem
rem  THE IDENTITY GOES INSIDE THE UNIT FIELD, NOT IN A FIELD OF ITS OWN.
rem  068 task 3, author's, overrulable. A sixth field would shift the
rem  approach, which must stay LAST so a pipe inside it moves nothing, and
rem  it would change how attempt-read.bat splits every line INCLUDING the
rem  ones already written. Ruling 4: what is written is not rewritten. So
rem  the field still begins "unit <u>", every pre-068 line parses exactly
rem  as it did, attempt-read.bat is not touched at all, and a reader sees
rem  "unit 1 launched 2026-09-19T10:59:33.412Z".
rem
rem  WHERE THERE IS NO IDENTITY the field is written "unit <u>" as before -
rem  never "unit <u> launched unknown", because a composed identity is one
rem  a reader could mistake for a measurement.
rem
rem  WRITTEN IN POWERSHELL, NOT BY echo, because the approach is prose and
rem  may carry a pipe, an ampersand or a closing parenthesis, and because it
rem  is read from the launcher's UTF-8 snapshot rather than a cmd variable -
rem  whole, never truncated. Where there is no snapshot the entry's own
rem  APPROACH value is used. The approach is LAST, so a pipe in it moves no
rem  field. Appended as UTF-8, and the line is never rewritten.
rem
rem  071 task 3: THE REASON A ROUTE IS CLOSED GOES ON ITS OWN LINE, AND THE
rem  INSTRUCTION ASKED FOR IT APPENDED TO THE ATTEMPT LINE. Both are reported.
rem  Why it could not be appended, measured rather than argued: 068's matcher
rem  reads an attempt with
rem
rem      $f = $Matches[2] -split '|', 4
rem
rem  - a split into AT MOST FOUR parts, so the approach is THE REMAINDER OF
rem  THE LINE. That is exactly why the approach is last and why 068 put its
rem  identity inside the unit field: a pipe inside the approach moves no
rem  field. It also means ANY FIELD APPENDED AFTER THE APPROACH LANDS INSIDE
rem  THE APPROACH, and the matcher would count the reason's words as the
rem  approach's - a false match blocks a route the arbiter never tried, which
rem  ruling 2 names as the failure this phase exists to prevent, and the
rem  instruction forbids weakening the matcher in the same breath as it asks
rem  for the append. Author's, overrulable, under ruling 1: a form is the
rem  arbiter's.
rem
rem  SO: a companion line, written immediately after the attempt, carrying the
rem  SAME criterion and the SAME unit identity so it binds to its attempt
rem  without depending on being adjacent:
rem
rem      REASON: <N.k> | unit <u> launched <id> | <why it is closed>
rem
rem  Nothing that was written before is touched, no line is split differently,
rem  the approach stays last, and 068's matcher - which reads only ^ATTEMPT: -
rem  never sees it.
rem
rem  THE REASON IS MEASURED, NEVER TYPED. It is derived from ADVANCED and
rem  FATE, which the launcher computed itself. Ruling 2 rejects an assertion
rem  of no path forward, and a reason a session wrote about itself is the
rem  thing this week's work stopped trusting. Three readings, and two of them
rem  say the route is NOT closed:
rem
rem      no + executed     the unit ran and the criterion did not flip
rem                        -> A CLOSED ROUTE
rem      no + never ran    the run never started - NOT a closed route,
rem                        CPS-DEC-076's reasoning exactly: an approach that
rem                        was never tried has not failed
rem      no + not recorded nothing was measured - NOT a closed route
rem
rem  A verdict of yes or blocker writes no REASON line at all, because
rem  neither is a failed approach.
if not defined ATCRIT goto :noattempt
powershell -NoProfile -Command "$f=[string]$env:FILE; $ap=$null; $af=[string]$env:ATAPPF; if(($af -ne '') -and (Test-Path -LiteralPath $af)){ $ap=[IO.File]::ReadAllText($af, [Text.Encoding]::UTF8) }; if($null -eq $ap){ $ap=[string]$env:APPROACH }; $ap=($ap -replace '[\r\n]+',' ').Trim(); $u='unit ' + $env:UNIT; $id=[string]$env:ATID; if(($id -ne '') -and ($id -ne 'unknown')){ $u=$u + ' launched ' + $id }; $adv=[string]$env:ADVANCED; $fate=[string]$env:FATE; $ln='ATTEMPT: ' + $env:ATCRIT + ' | ' + $u + ' | ' + $adv + ' | ' + $fate + ' | ' + $ap; $why=''; if($adv -eq 'no'){ if($fate -eq 'executed'){ $why='the unit ran to completion and the criterion did not flip from unmet to met' } elseif($fate -eq 'never ran'){ $why='NOT A CLOSED ROUTE - the run never started, so this approach was never tried' } else { $why='NOT A CLOSED ROUTE - the fate was not recorded, so nothing was measured' } }; $out=$ln + [char]13 + [char]10; if($why -ne ''){ $out=$out + 'REASON: ' + $env:ATCRIT + ' | ' + $u + ' | ' + $why + [char]13 + [char]10 }; [IO.File]::AppendAllText($f, $out, (New-Object Text.UTF8Encoding($false)))"
echo   Attempt recorded against criterion %ATCRIT%.
:noattempt

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
echo                      ^<accomplished^> [file] [fate] [state-why]
echo   outcome-append.bat --from-env
echo   outcome-append.bat --open [phase] [phase-set] [file]
echo.
echo   --open writes the phase header and every step from PHASE_PLAN.md at
echo   not started, appends no entry, and refuses a file that exists - exit 7.
echo.
echo   state: not started ^| in progress ^| partial ^| blocked ^| done
echo   fate : executed ^| never ran ^| not recorded - what happened to the RUN
echo   cost : total_cost_usd, or unknown. NEVER 0.
echo   Quote every argument.
echo.
echo   THE FILE COMES BEFORE THE FATE. To give a fate, give the file. Left
echo   out, the fate lands in the file's place and the reason in the fate's.
echo   The judge's prose is state-why - never the fate.
echo.
echo   0 appended, 2 a required argument missing, 3 write failed,
echo   4 the state is not one of the five, 5 the fate is not one of the three,
echo   6 advanced is not one of the four - yes, no, blocker, not recorded
echo.
set "RC=2"
goto :end

rem ============================================================
:end
echo.
echo outcome-append exit %RC%
endlocal & exit /b %RC%
