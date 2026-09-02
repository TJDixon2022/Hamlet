@echo off
rem ============================================================
rem  run-phase.bat  -  the loop
rem
rem      run-phase.bat <root> [--max-iterations N] [--budget USD]
rem                    [--minutes N] [--poll SECONDS]
rem
rem      0  the phase plan is satisfied
rem      1  a stop condition fired - WHICH ONE is named on screen
rem         and in the ledger
rem      2  usage, or bad root
rem      3  the lock is held - nothing was started
rem
rem  Reload, author, run, record, judge, repeat. Every part of the
rem  cycle existed before this file and none of it is reimplemented
rem  here: this composes lock.bat, reload.bat, a headless arbiter
rem  session, run-unit-watched.bat, outcome-append.bat and
rem  outcome-read.bat.
rem
rem  ---------------------------------------------------------------
rem  WHAT run-unit.bat ALREADY DOES, so this does not do it twice.
rem  It calls validate-output.bat, it writes the ledger line for the
rem  run, and it calls outcome-append.bat when --phase-step is set.
rem  THIS FILE THEREFORE WRITES A LEDGER LINE ONLY FOR AN ITERATION
rem  THAT NEVER REACHED THE LAUNCHER - a lock refusal, an arbiter
rem  failure, a stop before running. An iteration counted twice in
rem  the ledger is a night the owner cannot reconstruct.
rem
rem  AND IT CALLS outcome-append.bat ITSELF, because
rem  run-unit-watched.bat passes only --tools-file through to
rem  run-unit.bat - not --phase-step or its neighbours - so the
rem  watched path cannot carry the arbiter's judgment fields. Since
rem  PHASE_STEP is therefore unset inside run-unit.bat on that path,
rem  there is no double entry. That gap is reported by unit 043 and
rem  NOT repaired: every script in tools\arbiter\ carries a proof from
rem  an earlier unit.
rem  ---------------------------------------------------------------
rem
rem  THE TEN STOP CONDITIONS. The loop halts on any of them and
rem  NAMES WHICH, on screen and in the ledger.
rem
rem     1  the phase plan is satisfied - every step done or
rem        declared unachievable
rem     2  the budget is exhausted
rem     3  output.md section 4 is non-empty and the arbiter judges
rem        the ruling blocking
rem     4  THE ARBITER DECLARES A DECISION THE OWNER'S
rem     5  the watchdog fired
rem     6  A DENIAL THE UNIT COULD NOT WORK AROUND - it was refused
rem        AND could not complete because of it. Redefined by the
rem        owner's ruling of 2026-08-29; it was "permission_denials
rem        is non-empty", which halted units that had succeeded.
rem     7  validate-output.bat refused the report
rem     8  the section 4.1 gate refused
rem     9  the loop test - the arbiter proposes what it has tried
rem    10  no progress - two consecutive units in which the phase
rem        position did not move
rem
rem  CONDITIONS 3 AND 4 ARE WHAT KEEP THE OWNER THE ARCHITECT. The
rem  rest is plumbing. Those two are printed loud.
rem
rem  THE ITERATION BACKSTOP IS NOT A STOP CONDITION. --max-iterations
rem  exists to save the night when one of the ten fails to fire, and
rem  it says so when it trips. A backstop counted among the reasons
rem  would let a broken condition hide behind a number.
rem
rem  THE LOCK IS RELEASED ON EVERY PATH OUT, including every stop and
rem  every failure. 040 made that a demonstrated arm and it stays one.
rem
rem  NOT AGAINST THIS REPOSITORY. Fixtures only - a loop tested on
rem  the tree it lives in is a loop tested once, and 042's own
rem  script deleted five tracked files here.
rem
rem  PIPES INSIDE AN INLINE -Command ARE BARE, NOT ^|. Unit 041
rem  measured that inside a double-quoted cmd argument < > & and
rem  | are LITERAL, so escaping them hands PowerShell a caret it
rem  cannot parse: "Unexpected token '^' in expression". The
rem  first run of this file died that way in three places at
rem  once. cmd `echo` lines still need ^| - the difference is
rem  whether cmd is parsing the line or passing it through.
rem
rem  Generated 2026-08-29 for: work instructions 043 tasks 3 and 4
rem ============================================================

setlocal

set "HERE=%~dp0"
set "RC=0"
set "TOOK="
set "ROOT=%~1"
set "MAXITER=10"
set "BUDGET=25.00"
set "MINUTES=12"
set "POLL=30"
if "%ROOT%"=="" goto :usage
shift

:parse
if "%~1"=="" goto :parsed
if /i "%~1"=="--max-iterations" set "MAXITER=%~2" & shift & shift & goto :parse
if /i "%~1"=="--budget"         set "BUDGET=%~2" & shift & shift & goto :parse
if /i "%~1"=="--minutes"        set "MINUTES=%~2" & shift & shift & goto :parse
if /i "%~1"=="--poll"           set "POLL=%~2" & shift & shift & goto :parse
echo ERROR: unexpected argument: %~1
goto :usage

:parsed
if "%ROOT:~-1%"=="\" set "ROOT=%ROOT:~0,-1%"
if not exist "%ROOT%\" (
  echo ERROR: repository root not found: %ROOT%
  set "RC=2"
  goto :end
)
if not exist "%ROOT%\PHASE_PLAN.md" (
  echo ERROR: no PHASE_PLAN.md at %ROOT%
  echo A phase with no plan has no steps, and a loop with no steps has
  echo nothing to be finished. Refusing to start.
  set "RC=2"
  goto :end
)

set "WORK=%ROOT%\.run-unit"
if not exist "%WORK%" mkdir "%WORK%"
set "SCRATCH=%WORK%\scratch"
set "SPENT=0"
set "ITER=0"
set "STOPWHY="
set "LASTPOS="
set "NOPROGRESS=0"

echo.
echo ============================================================
echo  run-phase
echo    root      : %ROOT%
echo    budget    : %BUDGET% USD
echo    backstop  : %MAXITER% iterations ^(NOT a stop condition^)
echo    threshold : %MINUTES% min   poll every %POLL%s
echo ============================================================

rem --- the lock is CHECKED, NOT HELD, and that was measured -----
rem  The first cut took the lock for the whole phase. run-unit.bat
rem  takes its own lock per run - so the phase held the lock the run
rem  needed, run-unit refused with exit 1, and run-phase read that 1
rem  as "the watchdog fired" and stopped the loop on a stall that
rem  never happened. A loop that guarantees its own units cannot run
rem  is worse than no loop, because it fails in a way that looks
rem  like a finding.
rem
rem  So this asks lock.bat status and refuses if it is HELD, then
rem  leaves it alone. One writer at a time is enforced where the
rem  writing happens - inside run-unit.bat - which is where 023 put
rem  it and where it belongs.
echo.
echo Checking the session lock is free...
call "%HERE%lock.bat" status "%ROOT%" >nul
if errorlevel 1 (
  echo.
  echo REFUSED: the session lock is held. NOTHING WAS STARTED.
  echo run-unit.bat takes its own lock per run; this only checks that
  echo nothing else is already writing in that tree.
  set "RC=3"
  goto :end
)

rem ============================================================
rem  THE LOOP
rem ============================================================
:iterate
set /a ITER+=1
echo.
echo ------------------------------------------------------------
echo  iteration %ITER%
echo ------------------------------------------------------------

if %ITER% GTR %MAXITER% (
  echo.
  echo BACKSTOP: %MAXITER% iterations reached.
  echo THIS IS NOT A STOP CONDITION. It is the thing that saves the night
  echo when one of the ten fails to fire. If you are seeing this, a stop
  echo condition is broken and that is the finding.
  set "STOPWHY=backstop: %MAXITER% iterations, no stop condition fired"
  goto :stopped
)

rem --- 1. the scratch, emptied ---------------------------------
rem  PHASE_PLAN.md, 2026-08-31: .run-unit\scratch\ is the permitted
rem  scratch path, and the launcher clears it at the start of each
rem  iteration. That is what lets a unit rely on finding it empty,
rem  and it is what stops a growing set of scratch-NNN directories
rem  in the repository root that no later unit is permitted to clean.
call :scratch

rem --- 2. the reload -------------------------------------------
echo.
echo   [2] reload - measuring the picture
call "%HERE%reload.bat" "%ROOT%" --out "%WORK%\reload.txt" >nul
if not exist "%WORK%\reload.txt" (
  set "STOPWHY=the reload produced nothing - the arbiter would be authoring blind"
  goto :stopped
)
call :heartbeat

rem --- is the plan already satisfied? condition 1 ---------------
call :position
if "%OPENSTEPS%"=="0" (
  echo.
  echo   STOP 1: THE PHASE PLAN IS SATISFIED.
  echo   Every step is done or declared unachievable.
  set "STOPWHY=stop 1: the phase plan is satisfied"
  set "RC=0"
  goto :stopped
)
echo       open steps: %OPENSTEPS%   position: %POSITION%

rem --- condition 10: no progress -------------------------------
if "%POSITION%"=="%LASTPOS%" (
  set /a NOPROGRESS+=1
) else (
  set "NOPROGRESS=0"
)
set "LASTPOS=%POSITION%"
if %NOPROGRESS% GEQ 2 (
  echo.
  echo   STOP 10: NO PROGRESS. Two consecutive units and the phase
  echo   position did not move.
  set "STOPWHY=stop 10: no progress in two consecutive units"
  goto :stopped
)

rem --- 3. the arbiter ------------------------------------------
echo.
echo   [3] arbiter - authoring the next unit, restricted
call :heartbeat
call :arbiter
call :heartbeat
if not "%ARBRC%"=="0" (
  set "STOPWHY=the arbiter session failed - exit %ARBRC%"
  goto :stopped
)

rem  conditions 4 and 9 and the satisfied-plan case all arrive as
rem  MOVE: stop in the decision block. The arbiter is the only thing
rem  that can see them, which is why they are its call and not a
rem  test in this file.
rem  FLAT, WITH A LABEL, AND NOT A PARENTHESISED BLOCK - because
rem  %A_WHY% IS THE ARBITER'S PROSE AND CAN CONTAIN A CLOSING
rem  PARENTHESIS. cmd expands the variables in a ( ... ) body when it
rem  PARSES the if, before it decides whether to run it, so a ) in
rem  the value closes the block early and the words after it are run
rem  as commands. 045 lost a whole iteration to a decision block
rem  reading "its entry criterion (the root exists) is met": the loop
rem  died with "is was unexpected at this time." - the word straight
rem  after the parenthesis - having already paid for the arbiter and
rem  written a perfectly good WORK_INSTRUCTIONS.md.
rem  044 survived only because that arbiter happened to write no
rem  parentheses. The header above already recorded this idiom for
rem  the RUNRC checks; the arbiter's own text is where it bites.
rem  THE REQUIRED FIELD. The owner's ruling of 2026-08-30: the
rem  decision block names which step the unit advances and which exit
rem  criterion it moves - and "none - this unit clears a blocker" is a
rem  permitted and often correct answer, as CLAUDE_CODE.md 4.2 has it.
rem  What is refused is leaving it out.
rem
rem  WHY A FIELD AND NOT ONLY THE ORDERING. The ordering changes what
rem  the proposal is formed FROM; this makes it visible when the
rem  ordering was ignored. Neither alone is enough, and this half is
rem  the mechanical one: it catches the ABSENT case. A field filled in
rem  plausibly rather than truly is not something a script can see.
if not defined A_ADV goto :noadvances
if /i "%A_MOVE%"=="stop" goto :arbstop
if /i "%A_MOVE%"=="unachievable" echo       the arbiter declared step %A_STEP% unachievable
echo       step %A_STEP%, move %A_MOVE%
call :echosafe "%A_APPROACH%"
echo       approach: %ES%

rem --- 4. the run, watched -------------------------------------
echo.
echo   [4] run-unit-watched - launch, watch, kill on stall
call :heartbeat
call "%HERE%run-unit-watched.bat" %ITER% "%ROOT%" --minutes %MINUTES% --poll %POLL%
set "RUNRC=%ERRORLEVEL%"
echo       run-unit-watched exit %RUNRC%
call :heartbeat

rem --- 4a. the run's fate, which is not the step's state --------
rem  THE OWNER'S RULING OF 2026-08-29: a run that fails ends that
rem  unit; it does not halt the phase. The fact is recorded and
rem  handed to the arbiter, which judges what it means.
rem
rem  Exit 3 is "the run itself failed" - is_error, a non-zero exit
rem  from claude, or a record that cannot be read. Until 047 this
rem  file had branches for 1, 4, 5 and 7 and NONE for 3, so a failed
rem  run fell through in silence and the loop authored the next unit
rem  against a phase in which nothing had happened. It still
rem  continues - that is the ruling - but it now says so and the
rem  entry carries it.
set "RUNFATE=executed"
if "%RUNRC%"=="3" set "RUNFATE=never ran"

rem --- 4b. what state did the run leave the step in? ------------
call :judgestate

rem --- 4c. does section 4 actually want a ruling? ---------------
rem  BEFORE the record, so the verdict lands in PHASE_OUTCOME.md
rem  rather than only on screen.
call :judges4

rem --- 4d. are the two status files telling the truth? ----------
rem  PHASE_UPLIFT.md section 11. AFTER the unit and BEFORE the record,
rem  so the verdict rides into PHASE_OUTCOME.md attached to the unit
rem  that caused it rather than onto a screen nobody was watching.
rem
rem  IT DOES NOT HALT THE PHASE. A status file is a report about work
rem  and not the work; throwing away a good unit because its caption
rem  carried a bad timestamp is the worse outcome. The fault is carried
rem  forward with its name on it instead.
rem
rem  THE VERDICT RIDES ON FATE, which section 11 names. FATE already
rem  says what happened to the RUN as opposed to the step - `executed`
rem  or `never ran` - so a status file that failed validation is the
rem  same kind of fact about the same run, and it appends rather than
rem  replaces so the run's own fate is not lost behind it.
call "%HERE%status-check.bat" "%ROOT%"
set "SCRC=%ERRORLEVEL%"
if "%SCRC%"=="0" goto :statusok
echo.
echo       *** status-check exit %SCRC% - a status file is wrong. The phase
echo       *** is NOT halted. The fault is recorded against unit %ITER%.
set "RUNFATE=%RUNFATE%; status-check exit %SCRC% - a status file failed validation"
:statusok

rem --- 5. the record, with the arbiter's judgment ---------------
echo.
echo   [5] outcome-append - the record
call :heartbeat
call :cost
rem  SANITISE BEFORE THE CALL, AND DO NOT SWALLOW THE ERROR.
rem  Measured 2026-08-31 in Hamlet: four consecutive appends reported
rem  success and wrote nothing. Called by hand with the same arguments
rem  but plain prose, the same script exited 0 and updated the header.
rem  A double quote inside a forwarded field ends its quoted argument
rem  early and shifts every argument after it, so FILE stops being the
rem  outcome path. PHASE_UPLIFT_ADDENDUM section 5: any field forwarded
rem  from the decision block will eventually carry every shell
rem  metacharacter. The >nul is why it stayed hidden through four units.
call :scrub A_APPROACH
call :scrub A_HIT
call :scrub A_MOVE
call :scrub A_WHY
call :scrub A_DECIDED
call :scrub A_LICENCE
call :scrub A_DID
call :scrub J_WHY
call "%HERE%outcome-append.bat" "%ITER%" "%A_STEP%" "%J_STATE%" "%A_APPROACH%" "%A_HIT%" "%A_MOVE%" "%A_WHY%" "%A_DECIDED%" "%A_LICENCE%" "%RUNCOST%" "%A_DID%" "%ROOT%\PHASE_OUTCOME.md" "%RUNFATE%" "%J_WHY%"
set "OARC=%ERRORLEVEL%"
if not "%OARC%"=="0" goto :appendfailed

rem --- 5a. the card follows the record ---------------------------
rem  PHASE_UPLIFT.md section 5. Guarded on the append having succeeded,
rem  because PHASE_OUTCOME.md's header is the authority on where the
rem  phase stands and a failed append means the authority did not move.
rem  Copying from an unchanged header would paint the previous unit's
rem  position as though this unit had confirmed it.
call :phasesteps
goto :appended

:appendfailed
echo.
echo   *** outcome-append FAILED, exit %OARC%. The phase position
echo   *** did not move. Stop 10 will fire on the next reload and it
echo   *** will not be the truth about the work.
echo   *** The card was NOT updated: the record is the authority and
echo   *** it did not change.
echo.

:appended
echo       recorded step %A_STEP% as %J_STATE%, fate %RUNFATE%, cost %RUNCOST%

rem --- :scrub - one forwarded field, made safe for a command line
rem  Replaces the double quote with an apostrophe, which reads the same
rem  in a report and cannot end an argument. Delayed expansion is local
rem  to the subroutine; the endlocal line is parsed before it runs, so
rem  %V% carries the scrubbed value out.
:scrub
setlocal enabledelayedexpansion
set "V=!%~1!"
if not defined V ( endlocal & goto :eof )
set "V=!V:"='!"
rem  AND CAP IT. cmd.exe refuses a command line over 8191 characters
rem  with "The input line is too long." Measured 2026-09-01 in Hamlet:
rem  outcome-append failed with exactly that on unit 211, so the phase
rem  memory recorded nothing while the unit had succeeded. Eight prose
rem  fields go into one call and the arbiter writes at length, so any
rem  one of them left uncapped can sink the whole record. 900 each
rem  keeps the worst case near 7200 with the paths and the flags.
set "V=!V:~0,900!"
endlocal & set "%~1=%V%"
goto :eof

rem --- 6. the stop conditions the run produced -----------------
rem  EXIT 1 IS AMBIGUOUS AND HAS TO BE DISAMBIGUATED BY EVIDENCE.
rem  run-unit-watched.bat returns 1 for "the watchdog killed it" and
rem  passes run-unit.bat's codes through - and run-unit.bat's 1 means
rem  "the lock was held". Both arrive here as 1. Neither script may
rem  be modified, so the log is read: a kill writes "Terminating pid".
rem  Guessing between them would have this loop report a stall that
rem  never happened, which is exactly what the first run did.
rem  Flat, with a label, because a variable set inside a
rem  parenthesised block cannot be read inside the same block
rem  without delayed expansion - and turning that on in a file that
rem  does not have it changes how every other line here parses. 037
rem  recorded why.
if "%RUNRC%"=="1" goto :ambiguous1
if "%RUNRC%"=="3" goto :runnever
if "%RUNRC%"=="4" goto :judgedenials
if "%RUNRC%"=="5" (
  echo.
  echo   STOP 7: THE REPORT WAS REFUSED by validate-output.bat.
  set "STOPWHY=stop 7: validate-output refused the report"
  goto :stopped
)
if "%RUNRC%"=="7" (
  echo.
  echo   STOP 8: THE GATE REFUSED. The root is not what the instruction
  echo   is for.
  set "STOPWHY=stop 8: the section 4.1 gate refused"
  goto :stopped
)

:afterrunrc
rem --- condition 3: a ruling is wanted -------------------------
rem  JUDGED, NOT COUNTED. The owner's ruling of 2026-08-29, which is
rem  the stop 6 ruling applied to the condition with the same flaw:
rem  judge the thing, do not count the artifact. The verdict was
rem  taken at 4a, before the record, and is already in
rem  PHASE_OUTCOME.md whichever way it went.
rem  Flat, not a parenthesised block: %S4WHY% is a model's prose.
rem  STOP 3 IS NO LONGER COUNTED. The judge decides whether the ruling
rem  forecloses the step, not whether one exists. A banked ruling is
rem  recorded and the loop keeps working - measured 2026-08-31 in
rem  Hamlet, where two rulings blocking only the closing criterion
rem  halted a night with the table converter and the parity
rem  verification untouched and needing no ruling at all.
if "%S4WANTS%"=="yes" goto :s4stop
if "%S4WANTS%"=="banked" (
  echo.
  echo   A ruling is wanted and it forecloses nothing on this step.
  echo   Banked for the owner, and the loop continues.
  echo       %S4WHY%
  echo.
)
if "%S4WANTS%"=="unknown" goto :s4unknown

rem --- condition 2: the budget ---------------------------------
call :budget
if "%OVER%"=="1" (
  echo.
  echo   STOP 2: THE BUDGET IS EXHAUSTED. Spent %SPENT% of %BUDGET%.
  set "STOPWHY=stop 2: budget exhausted - spent %SPENT% of %BUDGET%"
  goto :stopped
)
echo       spent so far: %SPENT% of %BUDGET%

goto :iterate

rem ============================================================
rem  A FAILED RUN IS NOT A STOP CONDITION. The owner's ruling of
rem  2026-08-29. It ends the unit, it is recorded as a fact, and the
rem  loop goes on to let the ARBITER judge what it means - the step
rem  may be reachable another way, another step may be worth the
rem  night, or the cause may be the owner's under ARBITER.md 6.
rem
rem  046's iteration 3 reached exactly that conclusion with no such
rem  condition to help it, which is the evidence for not adding one.
rem  What it could not do was READ the fact; it had to infer it from
rem  an absent output.md. Now the entry says it.
:runnever
echo.
echo   THE RUN FAILED - it never reached its instruction. Exit 3.
echo   Nothing in the tree changed. This ENDS THE UNIT and is
echo   recorded as a fact; it does not halt the phase.
echo   Whatever claude said is in .run-unit\last-run.json.
goto :afterrunrc

rem ============================================================
rem  AN INSTRUCTION THAT NAMES NEITHER A STEP NOR A CRITERION DOES
rem  NOT RUN. The owner's ruling of 2026-08-30.
:noadvances
echo.
echo   REFUSED: the arbiter's decision block has no ADVANCES field.
echo   It must name the step and the exit criterion this unit moves,
echo   or say "none - this unit clears a blocker" and what it clears.
echo   Nothing was launched.
set "STOPWHY=refused: the decision block named no step and no criterion"
goto :stopped

rem ============================================================
rem  STOP 3, AS THE OWNER REDEFINED IT ON 2026-08-29.
rem  Flat, because %S4WHY% is a model's prose - see :arbstop.
:s4stop
echo.
echo   ****************************************************
echo   STOP 3: THE ARBITER JUDGES THAT A RULING IS WANTED.
echo   ****************************************************
echo   why : %S4WHY%
echo.
echo   Rulings are the owner's. This is one of the two conditions
echo   that keep him the architect.
set "STOPWHY=stop 3: a ruling is wanted - judged, not counted"
goto :stopped

rem  A JUDGE THAT COULD NOT BE READ IS NOT A NO. 0.0: absent,
rem  unparseable or refused renders as unknown, never as healthy.
rem  Halting names what happened; carrying on would be the loop
rem  deciding a question it could not read was not a question.
:s4unknown
echo.
echo   STOP 3: THE SECTION 4 JUDGE COULD NOT BE READ.
echo   %S4WHY%
echo   Section 4 has text in it and nothing established whether it
echo   wants a ruling, so this halts rather than assume it does not.
set "STOPWHY=stop 3: the section 4 judge could not be read - halted rather than assume"
goto :stopped

rem ============================================================
rem  THE STATE JUDGE. The owner's ruling of 2026-08-29.
rem
rem  THE STATE A STEP IS LEFT IN IS A JUDGMENT, AND IT IS MADE WITH
rem  THE REPORT IN HAND. Until 048 this loop recorded the arbiter's
rem  pre-run STATE: field, which the arbiter writes while AUTHORING -
rem  before the unit has run. So every step was recorded as it stood
rem  before the work, "done" was permanently zero, open steps was
rem  permanently the whole plan, and STOP CONDITION 1 COULD NEVER
rem  FIRE. 047 ran two units that completed both fixture steps and
rem  the header still read "not started" twice; the phase ran out on
rem  the backstop, whose own text says that seeing it means a stop
rem  condition is broken. It was right.
rem
rem  THE ARBITER'S PRE-RUN STATE KEEPS ITS MEANING and is not
rem  touched. Overloading it would put two meanings in one field,
rem  which is how PHASE came to mean two things in CPS-DEC-024.
rem
rem  IT JUDGES AGAINST THE PLAN'S EXIT CRITERIA, NOT AGAINST WHETHER
rem  THE UNIT FINISHED. A unit can complete and not achieve its step,
rem  which is exactly what partial and blocked exist to say, and
rem  inferring done from a clean exit is the declaring-victory
rem  failure PHASE_CONTROL.md 3 names.
rem
rem  A JUDGE THAT CANNOT BE READ LEAVES THE STEP WHERE IT WAS, at
rem  "in progress", and says so. That is the safe direction: it does
rem  not advance the phase toward satisfied on evidence nobody read.
rem  Section 0.0 - unknown is never rendered as healthy.
rem
rem  Same shape as the section 4 judge: prompt built to a file, the
rem  material appended with type, handed over on STDIN because
rem  PowerShell 5.1 mangles an argument containing a quote, no write
rem  and no shell, and the JSON read from the first brace because
rem  2>&1 can put a warning in front of it.
:judgestate
set "J_STATE=in progress"
set "J_WHY=the state judge could not be read, so the step is left where it was"
if not exist "%ROOT%\output.md" goto :jsnoreport
set "JSPLAN=%WORK%\step-plan.txt"
set "JSPROMPT=%WORK%\state-prompt.txt"
set "JSJSON=%WORK%\state-verdict.json"
powershell -NoProfile -Command "$f='%ROOT%\PHASE_PLAN.md'; $n='%A_STEP%'; $t=Get-Content -LiteralPath $f; $out=@(); foreach($ln in $t){ if($ln -cmatch ('^STEP: ' + [regex]::Escape($n) + ' \|')){ $out+=$ln } }; $on=$false; foreach($ln in $t){ if($ln -match ('^#{1,6}\s+Step\s+' + [regex]::Escape($n) + '\b')){ $on=$true; $out+=$ln; continue }; if($on -and ($ln -match '^#{1,6}\s+Step\s+\d' -or $ln -match '^-{3,}\s*$')){ break }; if($on){ $out+=$ln } }; if($out.Count -eq 0){ $out=@('(the plan has no STEP ' + $n + ' - judge on the report alone)') }; $out | Set-Content -LiteralPath '%JSPLAN%' -Encoding utf8"
if not exist "%JSPLAN%" goto :jsdone
call :writejsprompt
type "%JSPLAN%" >> "%JSPROMPT%"
>>"%JSPROMPT%" echo.
>>"%JSPROMPT%" echo --- the unit's report follows ---
>>"%JSPROMPT%" echo.
type "%ROOT%\output.md" >> "%JSPROMPT%"
powershell -NoProfile -Command "$a = @('-p', '--output-format', 'json', '--restricted', '--tools', 'Read', '--allowedTools', 'Read'); Push-Location '%ROOT%'; $ErrorActionPreference='Continue'; Get-Content -LiteralPath '%JSPROMPT%' -Raw | & claude @a 2>&1 | Set-Content -LiteralPath '%JSJSON%' -Encoding utf8; Pop-Location"
if not exist "%JSJSON%" goto :jsdone
for /f "usebackq tokens=1,* delims==" %%A in (`powershell -NoProfile -Command "try{ $raw=Get-Content -LiteralPath '%JSJSON%' -Raw; $k=$raw.IndexOf([char]123); if($k -lt 0){ exit }; $j=$raw.Substring($k) | ConvertFrom-Json }catch{ exit }; $r=[string]$j.result; $v=''; $w=''; foreach($ln in ($r -split [char]10)){ $s=$ln.Trim(); if($s -match '^STATE:\s*(.+?)\s*$'){ $v=$Matches[1] }; if($s -match '^WHY:\s*(.+)$'){ $w=$Matches[1] } }; $ok='not started','in progress','partial','blocked','done'; if($ok -contains $v.ToLower()){ 'J_STATE=' + $v.ToLower() }; if($w){ 'J_WHY=' + (((($w -replace '[&|<>^%%]','') -replace [char]96,'') -replace [char]34,'') -replace '\s+',' ').Trim() }"`) do set "%%A=%%B"
goto :jsdone

:jsnoreport
set "J_STATE=in progress"
set "J_WHY=no output.md, so there is no report to judge the step against"

:jsdone
echo       state judge : step %A_STEP% is %J_STATE%
echo                     %J_WHY%
goto :eof

:writejsprompt
>"%JSPROMPT%" echo You are judging one thing and nothing else.
>>"%JSPROMPT%" echo.
>>"%JSPROMPT%" echo Below is one step of a phase plan, then the report of a unit that
>>"%JSPROMPT%" echo has just run against it. Say what state that STEP is now in.
>>"%JSPROMPT%" echo.
>>"%JSPROMPT%" echo Answer with exactly one of these five words:
>>"%JSPROMPT%" echo.
>>"%JSPROMPT%" echo   not started   nothing has been done toward the step
>>"%JSPROMPT%" echo   in progress   work is under way and more is needed
>>"%JSPROMPT%" echo   partial       some of the exit criteria are met and not all
>>"%JSPROMPT%" echo   blocked       it cannot proceed without a decision or an
>>"%JSPROMPT%" echo                 outside change, and more effort will not help
>>"%JSPROMPT%" echo   done          every exit criterion the step states is met
>>"%JSPROMPT%" echo.
>>"%JSPROMPT%" echo JUDGE AGAINST THE STEP'S EXIT CRITERIA, NOT AGAINST WHETHER THE
>>"%JSPROMPT%" echo UNIT FINISHED ITS TASKS. A UNIT CAN COMPLETE EVERY TASK IT WAS
>>"%JSPROMPT%" echo GIVEN AND NOT ACHIEVE ITS STEP. A tidy report, a clean exit and a
>>"%JSPROMPT%" echo full set of commits are not the criteria; the criteria are what
>>"%JSPROMPT%" echo the plan says the step must leave behind.
>>"%JSPROMPT%" echo.
>>"%JSPROMPT%" echo If the report says a criterion was met, look for what it quotes or
>>"%JSPROMPT%" echo measures in support. If it claims the step is done and shows
>>"%JSPROMPT%" echo nothing, say partial and say that in your reason.
>>"%JSPROMPT%" echo.
>>"%JSPROMPT%" echo Answer with exactly two lines and nothing else:
>>"%JSPROMPT%" echo.
>>"%JSPROMPT%" echo STATE: one of the five words above
>>"%JSPROMPT%" echo WHY: one sentence, plain text, no punctuation beyond commas and full stops
>>"%JSPROMPT%" echo.
>>"%JSPROMPT%" echo --- the step from the phase plan follows ---
>>"%JSPROMPT%" echo.
goto :eof


rem ============================================================
rem  THE SECTION 4 JUDGE. One cheap call, asked one question.
rem
rem  WHY A SECOND CALL RATHER THAN A FIELD IN THE DECISION BLOCK.
rem  The arbiter runs at step 3, at the TOP of the iteration, and
rem  reads the PREVIOUS unit's report - so the report stop 3 halts
rem  on is one the arbiter has never seen. Carrying the verdict in
rem  the decision block therefore delays every halt by a whole
rem  iteration: a ruling wanted by unit 1 would not stop the loop
rem  until after unit 2 had run. That costs a full unit - 045
rem  measured one at $1.31 - to save a call measured at cents, and
rem  it leaves the LAST report of any phase never judged at all,
rem  which is the report most likely to want a ruling. A condition
rem  whose whole job is to fetch the owner must not be a unit late.
rem
rem  THE JUDGE GETS NO WRITE AND NO SHELL. --restricted with
rem  --tools Read; the section 4 text is handed to it in the prompt,
rem  so it needs nothing else.
rem
rem  AN EMPTY SECTION 4 COSTS NOTHING. No call is made -
rem  CLAUDE_CODE.md section 8's "empty is a real answer" is answered
rem  here without asking anybody.
:judges4
call :section4
set "S4WANTS=no"
set "S4WHY=section 4 is blank, which is CLAUDE_CODE.md section 8's empty-is-a-real-answer"
if "%S4EMPTY%"=="1" goto :s4done
set "S4WANTS=unknown"
set "S4WHY=the judge produced nothing that could be parsed"
set "S4TXT=%WORK%\section4.txt"
set "S4PROMPT=%WORK%\s4-prompt.txt"
set "S4JSON=%WORK%\s4-verdict.json"
powershell -NoProfile -Command "$f='%ROOT%\output.md'; $t=Get-Content -LiteralPath $f; $i=($t | Select-String -Pattern '^## 4\. ' | Select-Object -First 1).LineNumber; $t[$i..($t.Count-1)] | Set-Content -LiteralPath '%S4TXT%' -Encoding utf8"
if not exist "%S4TXT%" goto :s4done
call :writes4prompt
type "%S4TXT%" >> "%S4PROMPT%"
rem  THE PROMPT GOES DOWN STDIN, NOT INTO AN ARGUMENT. Windows
rem  PowerShell 5.1 mangles a native argument containing a double
rem  quote, and this prompt quotes a section heading while the
rem  report text it carries can contain anything at all. Passed as
rem  -p <text> the judge received the prompt CUT OFF at the first
rem  quote and replied asking what it was supposed to judge - a
rem  failure that costs a call and looks like a bad answer.
rem  Piping also feeds the stdin claude waits three seconds for.
powershell -NoProfile -Command "$a = @('-p', '--output-format', 'json', '--restricted', '--tools', 'Read', '--allowedTools', 'Read'); Push-Location '%ROOT%'; Get-Content -LiteralPath '%S4PROMPT%' -Raw | & claude @a 2>&1 | Set-Content -LiteralPath '%S4JSON%' -Encoding utf8; Pop-Location"
if not exist "%S4JSON%" goto :s4done
rem  NO BACKTICK MAY APPEAR INSIDE THIS COMMAND. for /f "usebackq"
rem  delimits with backticks, so a backtick in the PowerShell - the
rem  obvious `n for a newline, or one inside a character class -
rem  ENDS THE COMMAND EARLY and cmd runs the remainder. The first
rem  draft died with ".Trim() was unexpected at this time." That is
rem  the same family as the parenthesis 045 found: a character in
rem  the payload that the shell reads as structure.
rem  [char]10 is the newline, [char]96 the backtick, [char]34 the
rem  double quote. The strip exists because %S4WHY% is echoed and
rem  put in STOPWHY, where & | < > ^ are live.
for /f "usebackq tokens=1,* delims==" %%A in (`powershell -NoProfile -Command "try{ $raw=Get-Content -LiteralPath '%S4JSON%' -Raw; $k=$raw.IndexOf([char]123); if($k -lt 0){ exit }; $j=$raw.Substring($k) | ConvertFrom-Json }catch{ exit }; $r=[string]$j.result; $v=''; $w=''; foreach($ln in ($r -split [char]10)){ $s=$ln.Trim(); if($s -match '^VERDICT:\s*(\S+)'){ $v=$Matches[1] }; if($s -match '^WHY:\s*(.+)$'){ $w=$Matches[1] } }; if($v -match '^(?i)blocking'){ 'S4WANTS=yes' } elseif($v -match '^(?i)banked'){ 'S4WANTS=banked' } elseif($v -match '^(?i)none'){ 'S4WANTS=no' }; if($w){ 'S4WHY=' + (((($w -replace '[&|<>^%%]','') -replace [char]96,'') -replace [char]34,'') -replace '\s+',' ').Trim() }"`) do set "%%A=%%B"
:s4done
echo       section 4 : wants a ruling = %S4WANTS%
echo                   %S4WHY%
set "A_HIT=section 4 wants a ruling: %S4WANTS% - %S4WHY%"
goto :eof

rem ============================================================
rem  A MODEL-AUTHORED STRING IS NOT SAFE TO echo. Two different
rem  hazards, and they need two different fixes:
rem
rem    & | < > ^   break a BARE echo line - cmd parses them as
rem                structure wherever the line is. This routine
rem                substitutes them in a COPY for display.
rem    )           breaks only INSIDE a ( ... ) block, because cmd
rem                expands a block's variables when it parses it.
rem                The fix for that is the flat-label idiom, not
rem                substitution - a ruling's prose is full of
rem                legitimate parentheses and mangling them to make
rem                a display safe would corrupt what the owner reads.
rem
rem  THE ORIGINAL IS NEVER TOUCHED. %A_WHY% and its neighbours go to
rem  outcome-append.bat and the ledger as quoted arguments, where
rem  every one of these characters is harmless, so the record keeps
rem  the arbiter's words exactly as written and only the screen copy
rem  is substituted.
rem
rem  Proved 2026-08-29 against
rem    criterion (the root exists) is met & rm -rf / | echo pwned
rem    > x < y ^caret^ and 100%% done
:echosafe
set "ES=%~1"
set "ES=%ES:&=+%"
set "ES=%ES:|=/%"
set "ES=%ES:<=[%"
set "ES=%ES:>=]%"
set "ES=%ES:^=~%"
goto :eof

:writes4prompt
>"%S4PROMPT%" echo You are judging one thing and nothing else.
>>"%S4PROMPT%" echo.
>>"%S4PROMPT%" echo Below is section 4 of a work unit's report. Its heading is
>>"%S4PROMPT%" echo "What's blocking us". The convention is that a unit writes here
>>"%S4PROMPT%" echo any question that needs a ruling from the owner - and that an
>>"%S4PROMPT%" echo empty section 4 is a real answer meaning nothing is blocked.
>>"%S4PROMPT%" echo.
>>"%S4PROMPT%" echo Many units write a sentence SAYING nothing is blocking rather
>>"%S4PROMPT%" echo than leaving it blank. THAT IS NOT A RULING REQUEST. Neither is
>>"%S4PROMPT%" echo a note, an observation, a thing reported for the record, or a
>>"%S4PROMPT%" echo recommendation the unit has already acted on.
>>"%S4PROMPT%" echo.
>>"%S4PROMPT%" echo A RULING IS WANTED only where the text asks the owner to decide
>>"%S4PROMPT%" echo something, or says work is stopped until he does.
>>"%S4PROMPT%" echo.
>>"%S4PROMPT%" echo WHERE A RULING IS WANTED, JUDGE ONE MORE THING, AND IT IS THE
>>"%S4PROMPT%" echo POINT OF THIS CALL. A ruling that forecloses the whole step is
>>"%S4PROMPT%" echo different from one that stops a single exit criterion while other
>>"%S4PROMPT%" echo work on the same step remains open.
>>"%S4PROMPT%" echo.
>>"%S4PROMPT%" echo Read PHASE_PLAN.md for this step exit criteria and
>>"%S4PROMPT%" echo PHASE_OUTCOME.md for what has already been done. Then decide
>>"%S4PROMPT%" echo whether ANY work on this step could still proceed without the
>>"%S4PROMPT%" echo owner answer. The step being worked is step %A_STEP%.
>>"%S4PROMPT%" echo.
>>"%S4PROMPT%" echo Halting a night for a question that forecloses nothing is the
>>"%S4PROMPT%" echo failure this judgment exists to prevent. Building on an assumption
>>"%S4PROMPT%" echo the owner would have rejected is the other one. Weigh both and
>>"%S4PROMPT%" echo reason it out. Do not count the rulings - judge what they block.
>>"%S4PROMPT%" echo.
>>"%S4PROMPT%" echo Answer with exactly two lines and nothing else:
>>"%S4PROMPT%" echo.
>>"%S4PROMPT%" echo VERDICT: blocking
>>"%S4PROMPT%" echo WHY: one sentence, plain text, no punctuation beyond commas and full stops
>>"%S4PROMPT%" echo.
>>"%S4PROMPT%" echo or
>>"%S4PROMPT%" echo.
>>"%S4PROMPT%" echo VERDICT: banked
>>"%S4PROMPT%" echo WHY: one sentence, plain text, no punctuation beyond commas and full stops
>>"%S4PROMPT%" echo.
>>"%S4PROMPT%" echo or
>>"%S4PROMPT%" echo.
>>"%S4PROMPT%" echo VERDICT: none
>>"%S4PROMPT%" echo WHY: one sentence, plain text, no punctuation beyond commas and full stops
>>"%S4PROMPT%" echo.
>>"%S4PROMPT%" echo blocking means a ruling is wanted and NO work on this step can
>>"%S4PROMPT%" echo proceed without it. banked means a ruling is wanted and work on
>>"%S4PROMPT%" echo this step remains open. none means no ruling is wanted.
>>"%S4PROMPT%" echo.
>>"%S4PROMPT%" echo --- the section 4 text follows ---
>>"%S4PROMPT%" echo.
goto :eof

rem ============================================================
rem  Conditions 4, 9 and the satisfied-plan case all arrive as
rem  MOVE: stop. Flat, for the reason recorded where it is branched
rem  to: %A_WHY% is prose and prose contains parentheses.
:arbstop
echo.
echo   ****************************************************
echo   STOP 4: THE ARBITER DECLARED A DECISION THE OWNER'S.
echo   ****************************************************
call :echosafe "%A_WHY%"
echo   why : %ES%
echo.
echo   It stopped rather than resolving. This is one of the two
echo   conditions that keep the owner the architect.
set "STOPWHY=stop 4: the arbiter declared a decision the owner's"
goto :stopped

rem ============================================================
rem  STOP 6, AS THE OWNER REDEFINED IT ON 2026-08-29.
rem
rem  It fires when a unit was refused a tool AND COULD NOT COMPLETE
rem  BECAUSE OF IT - not on the mere presence of entries in
rem  permission_denials.
rem
rem  WHY. A healthy unit produces denials as a matter of course: the
rem  model tries a shape the rule does not match, is refused, adapts
rem  and proceeds. 044's unit 001 was denied EIGHT times, every one a
rem  compound cd "<root>" && ... into a shell already standing in
rem  that root, and it then passed its gate, wrote its files, wrote
rem  its report and made five commits with is_error False. The old
rem  condition stopped it and printed "it did not do the work it says
rem  it did", which was FALSE ABOUT THE RUN IT STOPPED. As written it
rem  halted the chain after every first unit and no widening of the
rem  scope files could prevent it - matching those forms means
rem  granting compound shell, which is already ruled against.
rem
rem  WHAT IS JUDGED, and why these and not others:
rem
rem    is_error        - the run's own verdict on itself.
rem    terminal_reason - how the session ended. Anything other than
rem                      completed means it stopped rather than
rem                      finished, so a refusal plausibly caused it.
rem    output.md exists- a unit that could not complete has nothing
rem                      to report. Presence is measured here;
rem                      SHAPE is not, because that is stop 7's job.
rem
rem  REPORT SHAPE IS CHECKED HERE TOO, AND ROUTED TO STOP 7. It has
rem  to be: run-unit.bat sets RC=4 on denials and jumps straight to
rem  its failed ledger line, so validate-output.bat IS NEVER REACHED
rem  on this path and an unshaped report would otherwise ride out
rem  behind a denial. The validator is CALLED, never edited.
rem
rem  THE DENIALS STAY LOUD WHATEVER THE JUDGMENT. run-unit.bat has
rem  already written them to .run-unit\denials.txt and its own ledger
rem  line already carries the count. Nothing here suppresses them,
rem  and the count and the judgment are printed as SEPARATE LINES so
rem  a reader sees "8 denied, unit completed, not fatal" rather than
rem  a verdict with no evidence.
:judgedenials
set "NDEN=?"
set "JISERR=?"
set "JTERM=?"
rem  THE FIRST-BRACE HARDENING, WHICH THIS PARSE WAS MISSING. 047
rem  kept claude's stderr in last-run.json so a failed run leaves
rem  evidence, and hardened the parses it knew about; this one was
rem  written by 046 and was not among them. MEASURED 2026-08-30: a
rem  fixture unit completed, wrote its files and committed, and this
rem  parse threw on the warning line - so NDEN, is_error and
rem  terminal_reason all read "unreadable", :denfatal fired, and
rem  STOP 6 halted the phase saying the unit could not complete. The
rem  same file parsed cleanly one line later in :cost, which 049 had
rem  hardened, and run-unit.bat's own ledger line carried the true
rem  values. A guard reading its evidence as unreadable refuses
rem  everything, which is the shape of a guard nobody can trust.
for /f "usebackq tokens=1,* delims==" %%A in (`powershell -NoProfile -Command "try{ $raw=Get-Content -LiteralPath '%WORK%\last-run.json' -Raw; $k=$raw.IndexOf([char]123); if($k -lt 0){ throw }; $j=$raw.Substring($k) | ConvertFrom-Json }catch{ 'NDEN=unreadable'; 'JISERR=unreadable'; 'JTERM=unreadable'; exit }; 'NDEN=' + @($j.permission_denials).Count; 'JISERR=' + $j.is_error; 'JTERM=' + $j.terminal_reason"`) do set "%%A=%%B"
echo.
echo       denied calls    : %NDEN%
echo       is_error        : %JISERR%
echo       terminal_reason : %JTERM%

if "%NDEN%"=="unreadable" goto :denfatal
if /i "%JISERR%"=="True" goto :denfatal
if not "%JTERM%"=="completed" goto :denfatal
if not exist "%ROOT%\output.md" goto :denfatal

call "%HERE%validate-output.bat" "%ROOT%\output.md" >nul
if errorlevel 1 goto :denunshaped

echo       judgment        : the unit completed. NOT FATAL - the loop goes on.
echo       The refusals are in .run-unit\denials.txt and in the ledger line.
goto :afterrunrc

:denunshaped
echo.
echo   STOP 7: THE REPORT WAS REFUSED by validate-output.bat.
echo   The unit was also denied %NDEN% call^(s^), but the report is the
echo   reason this stops - run-unit.bat returns 4 before it validates,
echo   so nothing else would have looked.
set "STOPWHY=stop 7: validate-output refused the report (after %NDEN% denied calls)"
goto :stopped

:denfatal
echo.
echo   STOP 6: PERMISSION DENIALS, AND THE UNIT COULD NOT COMPLETE.
echo   %NDEN% denied call^(s^), is_error %JISERR%, terminal %JTERM%.
echo   See .run-unit\denials.txt for what was refused.
set "STOPWHY=stop 6: denied %NDEN% and could not complete - is_error %JISERR%, terminal %JTERM%"
goto :stopped

rem ============================================================
:ambiguous1
set "KILLED=0"
for /f "usebackq delims=" %%K in (`powershell -NoProfile -Command "$f='%WORK%\watched.log'; if(Test-Path -LiteralPath $f){ @(Select-String -Path $f -Pattern 'Terminating pid').Count } else { 0 }"`) do set "KILLED=%%K"
if "%KILLED%"=="0" goto :lockheld
echo.
echo   STOP 5: THE WATCHDOG FIRED. The run stalled and was killed.
set "STOPWHY=stop 5: the watchdog fired"
goto :stopped

:lockheld
echo.
echo   STOP: the run could not take the session lock. Nothing ran.
set "STOPWHY=the run could not take the session lock"
goto :stopped

rem ============================================================
:stopped
echo.
echo ============================================================
echo  THE LOOP HALTED
echo    after     : %ITER% iteration^(s^)
echo    because   : %STOPWHY%
echo    spent     : %SPENT% of %BUDGET%
echo ============================================================
if not "%STOPWHY%"=="stop 1: the phase plan is satisfied" set "RC=1"
call :ledgerstop
rem  THE LAST ACT IS TO STOP CLAIMING TO BE TURNING. Leaving the final
rem  beat in place would have the card read `loop turning` for the whole
rem  of CFG.loopBeatMin after the loop halted - up to an hour of the
rem  panel asserting a loop that is not running, which is the one lie
rem  section 0.0 exists to prevent. The beat is REMOVED rather than
rem  back-dated, because absent renders as stopped and never as turning
rem  - PHASE_PLAN.md step 4's own must - and a composed older timestamp
rem  would be a value nobody read off a clock.
rem
rem  This is reached only from inside the loop, so it can only clear a
rem  beat this launcher wrote. The pre-loop refusals go straight to
rem  :end and never touch the file, which matters most for the held
rem  lock: another launcher is running there and its beat is true.
call :heartbeatclear
goto :end

rem ============================================================
rem  The position, as a single comparable string, and how many
rem  steps are still open. `not started`, `in progress`, `partial`
rem  and `blocked` are open; `done` is not. An unachievable step is
rem  recorded as done by the arbiter with its reasoning, per the
rem  three moves.
:position
set "OPENSTEPS=0"
set "POSITION="
for /f "usebackq delims=" %%P in (`powershell -NoProfile -Command "$f='%ROOT%\PHASE_OUTCOME.md'; if(-not (Test-Path -LiteralPath $f)){ 'none'; exit }; $ok='not started','in progress','partial','blocked','done'; $s=@(Select-String -Path $f -Pattern '^STEP: ' | ForEach-Object { $_.Line } | Where-Object { $p=$_.Substring(6).Split('|'); $p.Count -ge 2 -and $ok -contains $p[1].Trim() }); if($s.Count -eq 0){ 'none'; exit }; ($s | ForEach-Object { $p=$_.Substring(6).Split('|'); $p[0].Trim() + '=' + $p[1].Trim() }) -join ','"`) do set "POSITION=%%P"
rem  @(pipeline | Measure-Object).Count COUNTS THE WRAPPER, NOT THE
rem  MATCHES. Measure-Object emits ONE object; @() wraps that one
rem  object; .Count on it is 1 - and 1 when nothing matched at all.
rem  Until 045 both halves of this line were written that way, so
rem  planned was 1 for a plan of any size and done was 1 whenever
rem  PHASE_OUTCOME.md merely existed. Open computed 1 - 1 = 0 on
rem  every iteration after the first and STOP 1 fired: 044 measured
rem  it printing THE PHASE PLAN IS SATISFIED and exiting 0 against a
rem  three-step plan with nothing done and LEDGER_B.txt absent.
rem  THE LOOP COULD NEVER RUN MORE THAN ONE UNIT, and it said the
rem  opposite of the truth while stopping.
rem  @(pipeline).Count is the correct form and this file already used
rem  it at lines 324, 359 and 452 - which is what makes the old line
rem  a slip rather than a misunderstanding.
for /f "usebackq delims=" %%N in (`powershell -NoProfile -Command "$f='%ROOT%\PHASE_PLAN.md'; $planned=@(Select-String -Path $f -Pattern '^STEP: [0-9]+ \|' -CaseSensitive).Count; $o='%ROOT%\PHASE_OUTCOME.md'; $done=0; if(Test-Path -LiteralPath $o){ $done=@(Select-String -Path $o -Pattern '^STEP: [0-9]+ \| *done *\|' -CaseSensitive).Count }; if($planned -eq 0){ 1 } else { [Math]::Max(0, $planned - $done) }"`) do set "OPENSTEPS=%%N"
goto :eof

rem ============================================================
rem  The scratch, emptied at the start of every iteration.
rem ============================================================
rem  PHASE_PLAN.md, 2026-08-31, ".run-unit\scratch\ is the permitted
rem  scratch path". No scope is widened by this: the unit tool scope
rem  already permits writes under .run-unit\, which is the loop's own
rem  working state.
rem
rem  IT IS CLEARED, NOT DELETED. The directory exists on the way out
rem  of this routine whatever happened, because a unit that is told
rem  the path exists has to find it. Where it could not be emptied
rem  the run SAYS SO on screen rather than letting the next session
rem  discover a predecessor's files and have to reason about them -
rem  which is the cost the ruling exists to remove.
:scratch
if exist "%SCRATCH%" rd /s /q "%SCRATCH%" 2>nul
if not exist "%SCRATCH%" mkdir "%SCRATCH%" 2>nul
if not exist "%SCRATCH%" (
  echo       WARNING: the scratch path could not be created: %SCRATCH%
  goto :eof
)
set "SCRATCHLEFT="
for /f "delims=" %%F in ('dir /b "%SCRATCH%" 2^>nul') do set "SCRATCHLEFT=%%F"
if defined SCRATCHLEFT echo       WARNING: %SCRATCH% could not be emptied - a file in it is held open
goto :eof

rem ============================================================
rem  The heartbeat. PHASE_PLAN.md step 4, and 054's dropped task 7.
rem ============================================================
rem  THE CARD READS THIS AND NOTHING ELSE SAYS THE LOOP IS TURNING.
rem  loopBeatView in PROJECT_ANNUNCIATOR.html reads HEARTBEAT: out of
rem  PHASE_STATUS.md: fresh within CFG.loopBeatMin is `loop turning`,
rem  and absent, unparseable, ahead of the clock or older than the
rem  threshold are all `loop stopped`. Absent is read as stopped and
rem  NEVER as turning, which is what makes removing the beat on the
rem  way out an honest act rather than a hole.
rem
rem  THE CLOCK IS READ, NEVER COMPOSED. Get-Date -Format is the
rem  reading; CLAUDE_CODE.md section 11 records seven consecutive
rem  composed timestamps in this repository, and CPS-DEC-029 is what
rem  the panel does to one that lands ahead of its own clock.
rem
rem  THE INSERT RULE, proved by 054 against this file's actual bytes
rem  in tools\tests\heartbeat.test.js: replace an existing HEARTBEAT:
rem  line in the header IN PLACE, otherwise insert immediately above
rem  the FIRST ^STEP: line, and NEVER APPEND. Appended below the
rem  terminator the key is collected into strandedNames and the whole
rem  file returns not readable, which takes the entire phase region
rem  off the card.
rem
rem  THE ANCHOR IS ^STEP: AND NOT THE SUBSTRING. `CURRENT_STEP: 4`
rem  contains `STEP:` and comes FIRST in the real file, so what
rem  findstr "STEP:" would find is the middle of the CURRENT_STEP
rem  line - splitting it into a dangling `CURRENT_` and a bogus sixth
rem  step, and leaving the card unable to say which step is current.
rem  054's suite asserts that trap; this is the code it was asserted
rem  against.
rem
rem  THE HEADER IS THE LEADING RUN OF KEY: VALUE LINES, ending at the
rem  first line that is not one - STATUS_PROTOCOL.md 2.1's parse rule,
rem  which is the same rule the panel's parser uses. Only that region
rem  is searched, so a HEARTBEAT: written in the prose below the rule
rem  is neither replaced nor trusted.
rem
rem  BYTES OUTSIDE THE ONE LINE DO NOT MOVE. The file is read as
rem  bytes, its BOM and its newline are detected and reproduced, and
rem  it is written back with the same encoding - because Get-Content
rem  piped to Set-Content would rewrite every line ending in the file
rem  and 054 asserted that nothing else moved.
rem
rem  NO PHASE_STATUS.md, NO BEAT. PHASE_CONTROL.md section 4 gives
rem  that file to the arbiter and the launcher does not invent one:
rem  a file this routine composed would carry a phase, a current step
rem  and a step list that nobody read off anything.
rem ============================================================
rem  The heartbeat. PHASE_PLAN.md step 4, and 054's dropped task 7.
rem ============================================================
rem  THE CARD READS THIS AND NOTHING ELSE SAYS THE LOOP IS TURNING.
rem  loopBeatView in PROJECT_ANNUNCIATOR.html reads HEARTBEAT: out of
rem  PHASE_STATUS.md: fresh within CFG.loopBeatMin is `loop turning`,
rem  and absent, unparseable, ahead of the clock or older than the
rem  threshold are all `loop stopped`. Absent is read as stopped and
rem  NEVER as turning, which is what makes removing the beat on the
rem  way out an honest act rather than a hole.
rem
rem  THE CLOCK IS READ, NEVER COMPOSED. Get-Date -Format is the
rem  reading; CLAUDE_CODE.md section 11 records seven consecutive
rem  composed timestamps in this repository, and CPS-DEC-029 is what
rem  the panel does to one that lands ahead of its own clock.
rem
rem  THE INSERT RULE, proved by 054 against this file's actual bytes
rem  in tools\tests\heartbeat.test.js: replace an existing HEARTBEAT:
rem  line in the header IN PLACE, otherwise insert immediately above
rem  the FIRST ^STEP: line, and NEVER APPEND. Appended below the
rem  terminator the key is collected into strandedNames and the whole
rem  file returns not readable, which takes the entire phase region
rem  off the card.
rem
rem  THE ANCHOR IS ^STEP: AND NOT THE SUBSTRING. `CURRENT_STEP: 4`
rem  contains `STEP:` and comes FIRST in the real file, so what
rem  findstr "STEP:" would find is the middle of the CURRENT_STEP
rem  line - splitting it into a dangling `CURRENT_` and a bogus sixth
rem  step, and leaving the card unable to say which step is current.
rem  054's suite asserts that trap; this is the code it was asserted
rem  against.
rem
rem  THE HEADER IS THE LEADING RUN OF KEY: VALUE LINES, ending at the
rem  first line that is not one - STATUS_PROTOCOL.md 2.1's parse rule,
rem  which is the same rule the panel's parser uses. Only that region
rem  is searched, so a HEARTBEAT: written in the prose below the rule
rem  is neither replaced nor trusted.
rem
rem  BYTES OUTSIDE THE ONE LINE DO NOT MOVE. The file is read as
rem  bytes, its BOM and its newline are detected and reproduced, and
rem  it is written back with the same encoding - because Get-Content
rem  piped to Set-Content would rewrite every line ending in the file
rem  and 054 asserted that nothing else moved.
rem
rem  NO PHASE_STATUS.md, NO BEAT. PHASE_CONTROL.md section 4 gives
rem  that file to the arbiter and the launcher does not invent one:
rem  a file this routine composed would carry a phase, a current step
rem  and a step list that nobody read off anything.
rem ============================================================
rem  The card, copied from the record. Called from stage 5 and ONLY
rem  after a successful outcome-append.
rem ============================================================
rem  WHAT THIS FIXES. PHASE_OUTCOME.md read step 1 partial, step 2 done,
rem  step 3 blocked while PHASE_STATUS.md still read step 2 not started
rem  with CURRENT_STEP 2. The card had been stale for the whole phase,
rem  and the panel paints the card.
rem
rem  THE ROOT CAUSE WAS A WRITER NOBODY OWNED. PHASE_CONTROL.md section 4
rem  assigned CURRENT_STEP and the STEP: lines to the ARBITER, and
rem  ARBITER.md section 5 says the arbiter writes WORK_INSTRUCTIONS.md and
rem  its decision block and THAT IS ALL - it never touches tools\ and
rem  never commits. Both cannot hold, so nobody wrote them and the lines
rem  never moved. The launcher owns them now: it is the participant that
rem  already writes this file, on the beat.
rem
rem  WRITE SCOPES DO NOT OVERLAP. The launcher owns HEARTBEAT:, the
rem  STEP: lines and CURRENT_STEP:. The executor owns PHASE:, PHASE_SET:,
rem  DESCRIPTION: and WORK_INSTRUCTION:. Nothing below rewrites an
rem  executor-owned line, and a test hashes those four before and after.
rem
rem  IT FOLLOWS :heartbeat EXACTLY on bytes, BOM, newline and the header
rem  boundary, because two writers of one file that disagree about any of
rem  those will corrupt it between them. The header is the LEADING run of
rem  KEY: lines; nothing is ever read or written below it, so the ---
rem  terminator and the prose under it cannot be reached.
rem
rem  ONLY THE STATE FIELD MOVES. The regex keeps the line prefix and
rem  everything from the second pipe onward as captured groups, so the
rem  step number, the spacing and the description are the original bytes.
rem  When no line would change it writes NOTHING - not an identical file,
rem  which would still move the mtime and read as a fresh write.
rem
rem  CURRENT_STEP IS THE LOWEST STEP NOT done, and when every step is done
rem  it is the HIGHEST step number rather than 0 or absent - the panel
rem  renders both of those as `current step not identified`, which is
rem  measured in the test, not assumed.
:phasesteps
powershell -NoProfile -Command "$o='%ROOT%\PHASE_OUTCOME.md'; $p='%ROOT%\PHASE_STATUS.md'; if(-not (Test-Path -LiteralPath $o)){ ' no PHASE_OUTCOME.md - the card was not touched'; exit }; if(-not (Test-Path -LiteralPath $p)){ ' no PHASE_STATUS.md - the card was not touched'; exit }; $ob=[System.IO.File]::ReadAllBytes($o); $obom=($ob.Length -ge 3 -and $ob[0] -eq 239 -and $ob[1] -eq 187 -and $ob[2] -eq 191); $oraw=[System.Text.Encoding]::UTF8.GetString($ob); if($obom){ $oraw=$oraw.Substring(1) }; $onl=[string][char]10; if($oraw.IndexOf([char]13) -ge 0){ $onl=[string][char]13+[string][char]10 }; $ol=@($oraw -split $onl); $oend=$ol.Count; for($i=0;$i -lt $ol.Count;$i++){ if($ol[$i] -notmatch '^[A-Za-z][A-Za-z0-9_]*:'){ $oend=$i; break } }; $state=@{}; for($i=0;$i -lt $oend;$i++){ if($ol[$i] -cmatch '^STEP:\s*([0-9]+)\s*\|\s*([^|]*?)\s*\|'){ $state[[int]$Matches[1]]=$Matches[2] } }; if($state.Count -eq 0){ ' REFUSED: PHASE_OUTCOME.md header carries no STEP: line, so there is nothing to copy'; exit }; $bytes=[System.IO.File]::ReadAllBytes($p); $bom=($bytes.Length -ge 3 -and $bytes[0] -eq 239 -and $bytes[1] -eq 187 -and $bytes[2] -eq 191); $raw=[System.Text.Encoding]::UTF8.GetString($bytes); if($bom){ $raw=$raw.Substring(1) }; $nl=[string][char]10; if($raw.IndexOf([char]13) -ge 0){ $nl=[string][char]13+[string][char]10 }; $lines=@($raw -split $nl); $end=$lines.Count; for($i=0;$i -lt $lines.Count;$i++){ if($lines[$i] -notmatch '^[A-Za-z][A-Za-z0-9_]*:'){ $end=$i; break } }; $first=-1; $cur=-1; for($i=0;$i -lt $end;$i++){ if($first -lt 0 -and $lines[$i] -cmatch '^STEP:'){ $first=$i }; if($cur -lt 0 -and $lines[$i] -cmatch '^CURRENT_STEP:'){ $cur=$i } }; if($first -lt 0){ ' REFUSED: PHASE_STATUS.md carries no STEP: line in its header, and a step line is never appended'; exit }; $moved=0; for($i=0;$i -lt $end;$i++){ if($lines[$i] -cmatch '^(STEP:\s*([0-9]+)\s*\|\s*)([^|]*?)(\s*\|.*)$'){ $n=[int]$Matches[2]; if($state.ContainsKey($n)){ $w=$Matches[1]+$state[$n]+$Matches[4]; if($w -cne $lines[$i]){ $lines[$i]=$w; $moved++ } } } }; $nums=@($state.Keys | Sort-Object); $open=@($nums | Where-Object { $state[$_] -cne 'done' }); if($open.Count -gt 0){ $target=[string]$open[0] } else { $target=[string]$nums[$nums.Count-1] }; if($cur -ge 0){ if($lines[$cur] -cmatch '^(CURRENT_STEP:\s*)(.*)$'){ $w=$Matches[1]+$target; if($w -cne $lines[$cur]){ $lines[$cur]=$w; $moved++ } } } else { $pre=@(); if($first -gt 0){ $pre=@($lines[0..($first-1)]) }; $lines=$pre + @('CURRENT_STEP: '+$target) + @($lines[$first..($lines.Count-1)]); $moved++ }; if($moved -eq 0){ ' the card already matches the record - nothing written'; exit }; [System.IO.File]::WriteAllText($p, ($lines -join $nl), (New-Object System.Text.UTF8Encoding($bom))); ' card updated from the record: ' + $moved + ' line(s), current step ' + $target"
goto :eof

:heartbeat
powershell -NoProfile -Command "$p='%ROOT%\PHASE_STATUS.md'; if(-not (Test-Path -LiteralPath $p)){ '      no PHASE_STATUS.md - no beat written'; exit }; $bytes=[System.IO.File]::ReadAllBytes($p); $bom=($bytes.Length -ge 3 -and $bytes[0] -eq 239 -and $bytes[1] -eq 187 -and $bytes[2] -eq 191); $raw=[System.Text.Encoding]::UTF8.GetString($bytes); if($bom){ $raw=$raw.Substring(1) }; $nl=[string][char]10; if($raw.IndexOf([char]13) -ge 0){ $nl=[string][char]13+[string][char]10 }; $lines=@($raw -split $nl); $end=$lines.Count; for($i=0;$i -lt $lines.Count;$i++){ if($lines[$i] -notmatch '^[A-Za-z][A-Za-z0-9_]*:'){ $end=$i; break } }; $hb=-1; $st=-1; for($i=0;$i -lt $end;$i++){ if($hb -lt 0 -and $lines[$i] -cmatch '^HEARTBEAT:'){ $hb=$i }; if($st -lt 0 -and $lines[$i] -cmatch '^STEP:'){ $st=$i } }; $beat=Get-Date -Format 'yyyy-MM-dd HH:mm:ss'; if($hb -ge 0){ $lines[$hb]='HEARTBEAT: '+$beat } elseif($st -ge 0){ $pre=@(); if($st -gt 0){ $pre=@($lines[0..($st-1)]) }; $lines=$pre + @('HEARTBEAT: '+$beat) + @($lines[$st..($lines.Count-1)]) } else { '      REFUSED: the header carries no HEARTBEAT: and no ^STEP: line, and a beat is never appended'; exit }; [System.IO.File]::WriteAllText($p, ($lines -join $nl), (New-Object System.Text.UTF8Encoding($bom))); '      beat ' + $beat"
goto :eof

rem ============================================================
rem  The beat, removed. Called once, on the way out of the loop.
rem ============================================================
rem  See the comment at :stopped for why this exists.
:heartbeatclear
powershell -NoProfile -Command "$p='%ROOT%\PHASE_STATUS.md'; if(-not (Test-Path -LiteralPath $p)){ exit }; $bytes=[System.IO.File]::ReadAllBytes($p); $bom=($bytes.Length -ge 3 -and $bytes[0] -eq 239 -and $bytes[1] -eq 187 -and $bytes[2] -eq 191); $raw=[System.Text.Encoding]::UTF8.GetString($bytes); if($bom){ $raw=$raw.Substring(1) }; $nl=[string][char]10; if($raw.IndexOf([char]13) -ge 0){ $nl=[string][char]13+[string][char]10 }; $lines=@($raw -split $nl); $end=$lines.Count; for($i=0;$i -lt $lines.Count;$i++){ if($lines[$i] -notmatch '^[A-Za-z][A-Za-z0-9_]*:'){ $end=$i; break } }; $keep=@(); $gone=0; for($i=0;$i -lt $lines.Count;$i++){ if($i -lt $end -and $lines[$i] -cmatch '^HEARTBEAT:'){ $gone++ } else { $keep+=$lines[$i] } }; if($gone -eq 0){ '      no beat to clear - the card already reads stopped'; exit }; [System.IO.File]::WriteAllText($p, ($keep -join $nl), (New-Object System.Text.UTF8Encoding($bom))); '      beat cleared - the launcher has halted and nothing is turning'"
goto :eof

rem ============================================================
rem  The arbiter session. Restricted, with the write scope from a
rem  data file, and the decision block read back out of what it
rem  wrote. It cannot commit and it cannot touch tools\ - see
rem  arbiter-tools.txt.
:arbiter
set "ARBRC=1"
set "ARBTOOLS=%HERE%arbiter-tools.txt"
if not exist "%ARBTOOLS%" (
  echo       ERROR: no arbiter scope file at %ARBTOOLS%
  goto :eof
)
if not exist "%ROOT%\ARBITER.md" (
  echo       ERROR: no ARBITER.md at %ROOT%
  goto :eof
)
set "ARBPROMPT=%WORK%\arbiter-prompt.txt"
set "ARBJSON=%WORK%\arbiter.json"
call :writearbprompt
powershell -NoProfile -Command "$allow = Get-Content -LiteralPath '%ARBTOOLS%' | Where-Object { $_.Trim() -ne '' -and $_ -notmatch '^\s*rem\b' }; $p = Get-Content -LiteralPath '%ARBPROMPT%' -Raw; $a = @('-p', $p, '--output-format', 'json', '--restricted', '--tools', 'Read,Write,Bash'); foreach($r in $allow){ $a += '--allowedTools'; $a += $r.Trim() }; Push-Location '%ROOT%'; & claude @a 2>&1 | Set-Content -LiteralPath '%ARBJSON%' -Encoding utf8; Pop-Location"
if not exist "%ARBJSON%" goto :eof
for /f "usebackq tokens=1,* delims==" %%A in (`powershell -NoProfile -Command "try{ $raw=Get-Content -LiteralPath '%ARBJSON%' -Raw; $k=$raw.IndexOf([char]123); if($k -lt 0){ throw }; $j=$raw.Substring($k) | ConvertFrom-Json }catch{ 'ARBRC=1'; exit }; if($j.is_error){ 'ARBRC=1' } else { 'ARBRC=0' }; $d=@($j.permission_denials); 'ARBDENIED=' + $d.Count"`) do set "%%A=%%B"
if not "%ARBDENIED%"=="0" echo       NOTE: the arbiter was denied %ARBDENIED% call^(s^) - its scope held
call :readdecision
goto :eof

:writearbprompt
>"%ARBPROMPT%" echo Read ARBITER.md at this repository root and act as it says.
>>"%ARBPROMPT%" echo.
>>"%ARBPROMPT%" echo You are the arbiter. Author the next WORK_INSTRUCTIONS.md.
>>"%ARBPROMPT%" echo.
>>"%ARBPROMPT%" echo Read first, in this order:
>>"%ARBPROMPT%" echo   .run-unit\reload.txt   - the measured picture, disagreements first
>>"%ARBPROMPT%" echo   PHASE_PLAN.md          - what the phase is for
>>"%ARBPROMPT%" echo   PHASE_OUTCOME.md       - what has been tried and what it hit
>>"%ARBPROMPT%" echo   output.md              - the last unit's report, if there is one
>>"%ARBPROMPT%" echo.
>>"%ARBPROMPT%" echo Run the loop test before you propose an approach.
>>"%ARBPROMPT%" echo.
>>"%ARBPROMPT%" echo Write WORK_INSTRUCTIONS.md and end it with the ARBITER-DECISION
>>"%ARBPROMPT%" echo block exactly as ARBITER.md section 7 specifies. Write nothing else.
goto :eof

:readdecision
set "A_STEP=1"
set "A_APPROACH=not recorded"
set "A_MOVE=continue"
set "A_WHY=not recorded"
set "A_STATE=in progress"
set "A_DECIDED=none"
set "A_LICENCE=none"
set "A_HIT=not recorded"
set "A_DID=not recorded"
set "A_ADV="
if not exist "%ROOT%\WORK_INSTRUCTIONS.md" goto :eof
rem  PARSED IN POWERSHELL, AND EVERY DOUBLE QUOTE IS TURNED INTO A
rem  SINGLE ONE. MEASURED 2026-08-30: an arbiter wrote
rem  ACCOMPLISHED: ... "two units back to back with nobody between
rem  them" ..., that value was passed to outcome-append.bat as a
rem  QUOTED argument, the embedded quotes ended the argument early,
rem  and cmd answered "The syntax of the command is incorrect." and
rem  KILLED THE WHOLE PHASE - after the unit had run and been judged.
rem
rem  Latent since these fields were first forwarded. It had never
rem  fired because no arbiter had used a quotation mark, which is
rem  the worst shape of defect: dormant until the prose gets good.
rem
rem  A single quote rather than deletion, so the record still reads
rem  as the arbiter wrote it. The other shell-special characters are
rem  left alone deliberately - they are harmless inside a quoted
rem  argument, :echosafe already handles them for display, and
rem  mangling a ruling's prose to suit a shell is what 046 refused
rem  to do for parentheses.
for /f "usebackq tokens=1,* delims==" %%A in (`powershell -NoProfile -Command "$f='%ROOT%\WORK_INSTRUCTIONS.md'; if(-not (Test-Path -LiteralPath $f)){ exit }; $keys='STEP','APPROACH','MOVE','WHY','STATE','DECIDED','LICENCE','ACCOMPLISHED','ADVANCES'; $map=@{ 'STEP'='A_STEP'; 'APPROACH'='A_APPROACH'; 'MOVE'='A_MOVE'; 'WHY'='A_WHY'; 'STATE'='A_STATE'; 'DECIDED'='A_DECIDED'; 'LICENCE'='A_LICENCE'; 'ACCOMPLISHED'='A_DID'; 'ADVANCES'='A_ADV' }; foreach($ln in (Get-Content -LiteralPath $f)){ foreach($k in $keys){ if($ln -match ('^' + $k + ':\s*(.+?)\s*$')){ $v = $Matches[1] -replace [char]34, [char]39; $map[$k] + '=' + $v } } }"`) do set "%%A=%%B"
goto :eof

:setfield
set "K=%~1"
set "V=%~2"
if "%V:~0,1%"==" " set "V=%V:~1%"
if /i "%K%"=="STEP" set "A_STEP=%V%"
if /i "%K%"=="APPROACH" set "A_APPROACH=%V%"
if /i "%K%"=="MOVE" set "A_MOVE=%V%"
if /i "%K%"=="WHY" set "A_WHY=%V%"
if /i "%K%"=="STATE" set "A_STATE=%V%"
if /i "%K%"=="DECIDED" set "A_DECIDED=%V%"
if /i "%K%"=="LICENCE" set "A_LICENCE=%V%"
if /i "%K%"=="ACCOMPLISHED" set "A_DID=%V%"
goto :eof

rem ============================================================
:cost
set "RUNCOST=unknown"
rem  THE JSON DOES NOT START AT BYTE ZERO. 047 deliberately kept
rem  claude's stderr in last-run.json so a failed run leaves
rem  evidence, and the warning line now in front of the JSON made
rem  this plain ConvertFrom-Json throw. Measured in 048's phase:
rem  every iteration recorded "cost unknown" and the phase line read
rem  "spent 0.0000 of 8.00", while run-unit.bat's own ledger lines -
rem  which already start at the first brace - read 0.76250 and
rem  0.84392 for the same two runs.
rem
rem  THE BUDGET CEILING IS THE GUARD AGAINST AN EXPENSIVE NIGHT, and
rem  a loop that reads its spend as zero never stops on condition 2.
rem  :budget does not parse JSON at all - it adds this value to the
rem  running total - so this one line is the whole repair.
rem  The same first-brace hardening is used three times elsewhere in
rem  this file and once in run-unit.bat.
for /f "usebackq delims=" %%C in (`powershell -NoProfile -Command "try{ $raw=Get-Content -LiteralPath '%WORK%\last-run.json' -Raw; $k=$raw.IndexOf([char]123); if($k -lt 0){ throw }; $j=$raw.Substring($k) | ConvertFrom-Json; if($j.total_cost_usd){ $j.total_cost_usd } else { 'unknown' } }catch{ 'unknown' }"`) do set "RUNCOST=%%C"
goto :eof

:budget
set "OVER=0"
for /f "usebackq tokens=1,* delims==" %%A in (`powershell -NoProfile -Command "$s=0.0; try{ $s=[double]'%SPENT%' }catch{}; $c=0.0; try{ $c=[double]'%RUNCOST%' }catch{}; $t=$s+$c; 'SPENT=' + ('{0:N4}' -f $t); if($t -ge [double]'%BUDGET%'){ 'OVER=1' } else { 'OVER=0' }"`) do set "%%A=%%B"
goto :eof

rem ============================================================
rem  Section 4 empty or not. "Empty is a real answer" - so an
rem  absent section 4 is NOT the same as an empty one, and a report
rem  with no section 4 at all has already been refused by
rem  validate-output.bat inside run-unit.bat before this runs.
:section4
set "S4EMPTY=1"
for /f "usebackq delims=" %%E in (`powershell -NoProfile -Command "$f='%ROOT%\output.md'; if(-not (Test-Path -LiteralPath $f)){ '1'; exit }; $t=Get-Content -LiteralPath $f; $i=($t | Select-String -Pattern '^## 4\. ' | Select-Object -First 1).LineNumber; if(-not $i){ '1'; exit }; $body=@($t[$i..($t.Count-1)] | Where-Object { $_.Trim() -ne '' }); if($body.Count -eq 0){ '1' } else { '0' }"`) do set "S4EMPTY=%%E"
goto :eof

rem ============================================================
rem  ONE LEDGER LINE FOR THE STOP ITSELF. run-unit.bat has already
rem  written one per run that reached it; this is the line that says
rem  why the phase ended, which no run can know.
:ledgerstop
set "NOWSTAMP="
for /f "usebackq delims=" %%D in (`powershell -NoProfile -Command "(Get-Date).ToString('yyyy-MM-ddTHH:mm')"`) do set "NOWSTAMP=%%D"
call "%HERE%ledger.bat" "phase" "%NOWSTAMP%" "%NOWSTAMP%" "halted" "%STOPWHY%" "%SPENT%" "%ROOT%" >nul
goto :eof

rem ============================================================
:usage
echo.
echo   run-phase.bat ^<root^> [--max-iterations N] [--budget USD]
echo                 [--minutes N] [--poll SECONDS]
echo.
echo   --max-iterations defaults to 10. IT IS A BACKSTOP, NOT A STOP
echo                    CONDITION - it saves the night when one of the
echo                    ten fails to fire.
echo   --budget         defaults to 25.00 USD
echo.
echo   0 the plan is satisfied, 1 a stop condition fired,
echo   2 usage or bad root, 3 the lock is held
echo.
set "RC=2"
goto :end

rem ============================================================
:end
rem  THE LOCK IS RELEASED ON EVERY PATH OUT - by run-unit.bat, which
rem  is the only thing that takes it. This file never holds one, so
rem  there is none to leak. Asserted after every arm in task 5 all
rem  the same, because 040 made that a demonstrated arm.
echo.
echo run-phase exit %RC%
endlocal & exit /b %RC%
