@echo off
rem ============================================================
rem  run-phase.bat  -  the loop
rem
rem      run-phase.bat <root> [--max-iterations N] [--budget USD]
rem                    [--minutes N]
rem      run-phase.bat <root> --fixture <judge-section4 | record>
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
rem     3  output.md section 4 asks the owner to decide one of the
rem        THREE THINGS THE PHASE STOPS FOR, as the section 4 judge
rem        reads it: keying, transmit or the radio's safety; money
rem        past the budget; what the product promises the operator.
rem        A question outside the three is no ruling wanted (061).
rem     4  THE ARBITER DECLARES A DECISION THE OWNER'S - one of the
rem        same three, ARBITER.md section 6
rem     5  the watchdog fired - the run's process tree accrued no CPU
rem        time for ten minutes, or the owner's --minutes ceiling was
rem        reached. The watchdog has no clock of its own (061).
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
rem  AND SINCE 061 THEY FIRE FOR THREE THINGS ONLY. The owner, 2026-09-12:
rem  "How do we make the arbiter tougher? It just gives up so easily."
rem  Twice in one evening on HamLet this loop halted at STOP 3 on a
rem  question the report had already answered with a recommendation - a
rem  plan line a later ruling contradicted, and a layout split arithmetic
rem  would not allow. Everything outside the three is taken on the
rem  author's own recommendation, marked author's, overrulable, applied,
rem  and the loop goes on. The owner overrules later with one word.
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
rem  NO --minutes DEFAULT, AND THAT IS THE RULE RATHER THAN AN OMISSION.
rem  Until 061 this read 12 and meant twelve minutes without a status
rem  write - the watchdog's own threshold, passed down on every run. The
rem  watchdog now has no clock of its own (run-unit-watched.bat) and
rem  --minutes is the owner's wall-clock ceiling on one run, so a default
rem  here would be a clock the owner never set. --poll went with the old
rem  rule; the look interval is a constant in run-unit-watched.bat.
set "MINUTES="
set "FIXTURE="
if "%ROOT%"=="" goto :usage
shift

:parse
if "%~1"=="" goto :parsed
if /i "%~1"=="--max-iterations" set "MAXITER=%~2" & shift & shift & goto :parse
if /i "%~1"=="--budget"         set "BUDGET=%~2" & shift & shift & goto :parse
if /i "%~1"=="--minutes"        set "MINUTES=%~2" & shift & shift & goto :parse
if /i "%~1"=="--fixture"        set "FIXTURE=%~2" & shift & shift & goto :parse
if /i "%~1"=="--poll"           goto :pollgone
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

if defined FIXTURE goto :fixture

echo.
echo ============================================================
echo  run-phase
echo    root      : %ROOT%
echo    budget    : %BUDGET% USD
echo    backstop  : %MAXITER% iterations ^(NOT a stop condition^)
if defined MINUTES echo    ceiling   : %MINUTES% min per run - the owner's --minutes
if not defined MINUTES echo    ceiling   : no clock on a run - no --minutes was given
echo    watchdog  : kills a run only after ten minutes with no CPU anywhere
echo                in its process tree. It has no clock of its own.
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
echo   [4] run-unit-watched - launch, watch, kill a run whose process tree is dead
call :heartbeat
rem  --minutes IS PASSED ONLY WHEN THE OWNER GAVE IT. Without it the run
rem  has no clock at all, which is the ruling (061), not a gap.
set "MINARG="
if defined MINUTES set "MINARG=--minutes %MINUTES%"
call "%HERE%run-unit-watched.bat" %ITER% "%ROOT%" %MINARG%
set "RUNRC=%ERRORLEVEL%"
echo       run-unit-watched exit %RUNRC%
call :heartbeat

rem --- 4a to 5, ONE SUBROUTINE ------------------------------------
rem  CALLED, NOT FALLEN INTO, so `run-phase.bat <root> --fixture record`
rem  runs these exact lines and its fixture proves what the loop runs
rem  rather than a copy of it. 061 task 3.
call :record
goto :afterrecord

:record
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

rem --- 4d. the status file the unit just wrote --------------------
rem  THE OWNER'S RULING OF 2026-09-01: CATCH IT WHERE IT IS WRITTEN.
rem  A bad field is checked in the unit that WROTE it, while that
rem  unit's report is still being judged - not at the panel an hour
rem  later. The panel stays the LAST line of defence rather than the
rem  only one.
rem
rem  IT RUNS BEFORE outcome-append SO THE VERDICT CAN BE RECORDED
rem  AGAINST THE UNIT THAT CAUSED IT. A fault that reaches the owner
rem  detached from its unit is a fault he has to go looking for.
rem
rem  A FAILING CHECK DOES NOT HALT THE PHASE. The ruling is explicit.
rem  The loop is not stopped by a bad timestamp: STATUSRC is recorded
rem  and named, and the night's work goes on. This is the same shape
rem  as RUNFATE - a fact handed to the record rather than a stop.
echo.
echo   [4d] status-check - the file the unit just wrote
call "%HERE%status-check.bat" "%ROOT%"
set "STATUSRC=%ERRORLEVEL%"
set "STATUSNOTE=status-check clean"
if "%STATUSRC%"=="1" set "STATUSNOTE=STATUS-CHECK FAILED - a field this unit wrote is wrong, see the run output"
if "%STATUSRC%"=="2" set "STATUSNOTE=STATUS-CHECK: PROJECT_STATUS.md absent or nothing readable"
if not "%STATUSRC%"=="0" echo       %STATUSNOTE%
if not "%STATUSRC%"=="0" echo       THE PHASE IS NOT HALTED. Recorded against this unit and carried on.

rem  THE STATUS VERDICT RIDES IN ON HIT, NEVER ON THE FATE. 061 task 3.
rem  Until 061 it was appended to the fate - "executed - STATUS-CHECK
rem  FAILED ..." - and outcome-append.bat refuses any fate that is not one
rem  of its three, at exit 5, so EVERY unit whose status file failed the
rem  check lost its whole outcome entry, the phase position did not move,
rem  and stop 10 was one iteration away. Measured by 061 with the call line
rem  exactly as it stood. HIT is what the unit hit, and a bad status file
rem  is something it hit.
if not "%STATUSRC%"=="0" set "A_HIT=%A_HIT% - %STATUSNOTE%"

rem --- 5. the record, with the arbiter's judgment ---------------
echo.
echo   [5] outcome-append - the record
call :heartbeat
call :cost

rem  THE FATE IS ONE OF THREE, AND THE LAUNCHER CHECKS BEFORE IT CALLS.
rem  The owner's words were "one word"; two of outcome-append.bat's three
rem  are two words - never ran, not recorded - so the guard is the
rem  vocabulary rather than a word count, which would refuse a correct
rem  never ran. The fate comes from the run's exit code at 4a and nowhere
rem  else. The judge's prose goes to STATE_WHY, the fourteenth argument,
rem  and never to the fate.
rem
rem  A FATE OUTSIDE THE THREE IS A LAUNCHER BUG, SAID SO BEFORE THE CALL.
rem  It is recorded as not recorded - the vocabulary's own word for "the
rem  launcher could not say" - with the bug named in HIT, so the entry
rem  still lands and the position still moves. Refusing the append
rem  instead is what held HamLet's step 6 still on 2026-09-12.
set "FATEOK="
if "%RUNFATE%"=="executed" set "FATEOK=1"
if "%RUNFATE%"=="never ran" set "FATEOK=1"
if "%RUNFATE%"=="not recorded" set "FATEOK=1"
if defined FATEOK goto :recfateok
echo.
echo   LAUNCHER BUG: the fate about to be recorded is not one of the three.
echo   It was: %RUNFATE%
echo   This is run-phase.bat's fault - not the unit's, not outcome-append's.
echo   Recording it as "not recorded" with the bug named in HIT.
set "A_HIT=%A_HIT% - LAUNCHER BUG: the fate was not one of the three and is recorded as not recorded"
set "RUNFATE=not recorded"
:recfateok

rem  THE PROSE GOES BY ENVIRONMENT, NOT ON THE call LINE. 061 task 3, and
rem  this is the fault HamLet hit at 13:54 on 2026-09-12, REPRODUCED.
rem
rem  call expands percent signs a SECOND time. In a batch file, a percent
rem  sign followed by a name and a colon, with that name undefined, is
rem  REMOVED - from the percent sign up to the colon. Unit 331's APPROACH
rem  carried one percent sign, in "rather than 62 percent of it", and HIT
rem  always carries a colon, in "section 4 wants a ruling:". Everything
rem  between the two vanished, both closing quotes with it, so APPROACH
rem  and HIT fused into one argument, every later argument moved up one
rem  place, the file went where the cost goes, the fate where the file
rem  goes, and the state judge's sentence where the fate goes. outcome-
rem  append.bat refused that sentence as a fate at exit 5 and the phase
rem  position did not move. The earlier caller here could never have
rem  shown it: the call ends in a redirect to nul.
rem
rem  061 measured it with unit 331's decision block verbatim and a shim
rem  that recorded the arguments as cmd split them: thirteen, not
rem  fourteen, the twelfth reading executed and the thirteenth the
rem  sentence. A probe with a percent sign and no later colon did NOT
rem  shift, which is why the first probe missed it.
rem
rem  A set line expands ONCE, and a percent sign inside a value it
rem  expands is inert. So the values are set here and outcome-append
rem  reads them with --from-env. The call line carries no prose at all.
set "OA_UNIT=%ITER%"
set "OA_STEP=%A_STEP%"
set "OA_STATE=%J_STATE%"
set "OA_APPROACH=%A_APPROACH%"
set "OA_HIT=%A_HIT%"
set "OA_MOVE=%A_MOVE%"
set "OA_WHY=%A_WHY%"
set "OA_DECIDED=%A_DECIDED%"
set "OA_LICENCE=%A_LICENCE%"
set "OA_COST=%RUNCOST%"
set "OA_ACCOMPLISHED=%A_DID%"
set "OA_FILE=%ROOT%\PHASE_OUTCOME.md"
set "OA_FATE=%RUNFATE%"
set "OA_STATEWHY=%J_WHY%"
call "%HERE%outcome-append.bat" --from-env >nul
set "APPRC=%ERRORLEVEL%"
if "%APPRC%"=="0" echo       recorded step %A_STEP% as %J_STATE%, fate %RUNFATE%, cost %RUNCOST%
if not "%APPRC%"=="0" echo       NOT RECORDED - outcome-append exit %APPRC%
goto :eof

:afterrecord

rem --- 5a. the card catches up with the record ------------------
rem  THE OWNER'S RULING OF 2026-08-31. Nothing wrote PHASE_STATUS.md's
rem  step states or CURRENT_STEP by machine: the executor wrote them by
rem  hand mid-unit, ARBITER.md section 5 forbids the arbiter, and the
rem  state judge produced the verdict and wrote nowhere. So the card
rem  was one judgment stale at every step, and at the END of a phase it
rem  was PERMANENTLY stale, because the last unit's judgment happens
rem  after that unit has exited and nobody ever copied it in.
rem
rem  IT RUNS ONLY ON A SUCCESSFUL APPEND, because the outcome header is
rem  the authority and a failed append means the authority did not move.
rem  Copying from it then would put this unit's judgment on the card
rem  while the record does not carry it.
if not "%APPRC%"=="0" echo       outcome-append exit %APPRC% - step states NOT copied
if "%APPRC%"=="0" call :phasesteps

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
if "%S4WANTS%"=="yes" goto :s4stop
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
echo   It asks about one of the three things the phase stops for -
echo   keying, transmit or the radio's safety; money past the budget;
echo   what the product promises the operator. Those are the owner's,
echo   and this is one of the two conditions that keep him the architect.
echo   A question about anything else would not have stopped here: 061.
set "STOPWHY=stop 3: a ruling is wanted on one of the three - judged, not counted"
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
rem  THE WHY RUNS TO ITS END, NOT TO ITS FIRST LINE BREAK. 061 task 3. A
rem  judge that writes its reason over two lines used to have the second
rem  dropped, so STATE_WHY kept half a verdict. Lines after WHY: are
rem  joined onto it until a blank line or the next KEY: line. The judge's
rem  prose goes to STATE_WHY and nowhere else; the fate is not read here.
for /f "usebackq tokens=1,* delims==" %%A in (`powershell -NoProfile -Command "try{ $raw=Get-Content -LiteralPath '%JSJSON%' -Raw; $k=$raw.IndexOf([char]123); if($k -lt 0){ exit }; $j=$raw.Substring($k) | ConvertFrom-Json }catch{ exit }; $r=[string]$j.result; $v=''; $w=''; $inw=$false; foreach($ln in ($r -split [char]10)){ $s=$ln.Trim(); if($s -match '^STATE:\s*(.+?)\s*$'){ $v=$Matches[1]; $inw=$false; continue }; if($s -match '^WHY:\s*(.+)$'){ $w=$Matches[1]; $inw=$true; continue }; if($inw){ if(($s -eq '') -or ($s -match '^[A-Z_]+:')){ $inw=$false } else { $w=$w + ' ' + $s } } }; $ok='not started','in progress','partial','blocked','done'; if($ok -contains $v.ToLower()){ 'J_STATE=' + $v.ToLower() }; if($w){ 'J_WHY=' + (((($w -replace '[&|<>^%%]','') -replace [char]96,'') -replace [char]34,'') -replace '\s+',' ').Trim() }"`) do set "%%A=%%B"
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
>>"%JSPROMPT%" echo   blocked       it cannot proceed without an outside change, or
>>"%JSPROMPT%" echo                 without the owner's decision on one of the three
>>"%JSPROMPT%" echo                 things the phase stops for, and more effort will
>>"%JSPROMPT%" echo                 not help
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
>>"%JSPROMPT%" echo THE PHASE STOPS FOR THREE THINGS ONLY: anything that touches keying,
>>"%JSPROMPT%" echo transmit or the radio's safety; money past the budget; a decision that
>>"%JSPROMPT%" echo changes what the product promises the operator - what a card asserts,
>>"%JSPROMPT%" echo what a click does, what is logged as true. A question about anything
>>"%JSPROMPT%" echo else - layout, wording, a number, a test's shape, a plan line a later
>>"%JSPROMPT%" echo ruling contradicts - does not make a step blocked. The unit was to take
>>"%JSPROMPT%" echo its own recommendation on it, and a step waiting on such a question is
>>"%JSPROMPT%" echo in progress or partial by its criteria, not blocked.
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
for /f "usebackq tokens=1,* delims==" %%A in (`powershell -NoProfile -Command "try{ $raw=Get-Content -LiteralPath '%S4JSON%' -Raw; $k=$raw.IndexOf([char]123); if($k -lt 0){ exit }; $j=$raw.Substring($k) | ConvertFrom-Json }catch{ exit }; $r=[string]$j.result; $v=''; $w=''; foreach($ln in ($r -split [char]10)){ $s=$ln.Trim(); if($s -match '^VERDICT:\s*(\S+)'){ $v=$Matches[1] }; if($s -match '^WHY:\s*(.+)$'){ $w=$Matches[1] } }; if($v -match '^(?i)ruling'){ 'S4WANTS=yes' } elseif($v -match '^(?i)none'){ 'S4WANTS=no' }; if($w){ 'S4WHY=' + (((($w -replace '[&|<>^%%]','') -replace [char]96,'') -replace [char]34,'') -replace '\s+',' ').Trim() }"`) do set "%%A=%%B"
:s4done
echo       section 4 : wants a ruling = %S4WANTS%
echo                   %S4WHY%
set "A_HIT=section 4 wants a ruling: %S4WANTS% - %S4WHY%"
rem  A QUESTION OUTSIDE THE THREE IS MARKED IN THE RECORD, NOT DROPPED. 061
rem  task 5: the author's recommendation stands, marked author's,
rem  overrulable, and the loop goes on - so the entry says so, and the
rem  owner reading PHASE_OUTCOME.md can find what was decided without him.
if "%S4WANTS%"=="no" if "%S4EMPTY%"=="0" set "A_HIT=section 4 asked nothing inside the three stops - author's, overrulable, the loop continued - %S4WHY%"
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
>>"%S4PROMPT%" echo THE OWNER IS STOPPED FOR EXACTLY THREE THINGS - his ruling of
>>"%S4PROMPT%" echo 2026-09-12:
>>"%S4PROMPT%" echo.
>>"%S4PROMPT%" echo   1  anything that touches keying, transmit, or the radio's safety -
>>"%S4PROMPT%" echo      in a project that is not a radio, what the project can make
>>"%S4PROMPT%" echo      happen outside the machine it runs on
>>"%S4PROMPT%" echo   2  money past the budget
>>"%S4PROMPT%" echo   3  a decision that changes what the product promises the operator -
>>"%S4PROMPT%" echo      what a card asserts, what a click does, what is logged as true
>>"%S4PROMPT%" echo.
>>"%S4PROMPT%" echo A QUESTION ABOUT ANYTHING ELSE IS NOT A RULING REQUEST, however it is
>>"%S4PROMPT%" echo worded - even when it offers options, and even when it says the work
>>"%S4PROMPT%" echo is waiting on the owner. Layout, wording, a test's shape, a number, a
>>"%S4PROMPT%" echo mechanism in the plan that arithmetic will not allow, a plan line a
>>"%S4PROMPT%" echo later ruling contradicts: the unit's own recommendation is taken on
>>"%S4PROMPT%" echo those, marked author's, overrulable, and the work goes on.
>>"%S4PROMPT%" echo.
>>"%S4PROMPT%" echo Many units write a sentence SAYING nothing is blocking rather
>>"%S4PROMPT%" echo than leaving it blank. THAT IS NOT A RULING REQUEST. Neither is
>>"%S4PROMPT%" echo a note, an observation, a thing reported for the record, or a
>>"%S4PROMPT%" echo recommendation the unit has already acted on.
>>"%S4PROMPT%" echo.
>>"%S4PROMPT%" echo A RULING IS WANTED only where the text asks the owner to decide
>>"%S4PROMPT%" echo something INSIDE ONE OF THE THREE, or says work is stopped until he
>>"%S4PROMPT%" echo decides something inside one of the three. Where one question is
>>"%S4PROMPT%" echo inside the three and others are outside, a ruling is wanted.
>>"%S4PROMPT%" echo.
>>"%S4PROMPT%" echo Answer with exactly two lines and nothing else:
>>"%S4PROMPT%" echo.
>>"%S4PROMPT%" echo VERDICT: ruling
>>"%S4PROMPT%" echo WHY: one sentence, plain text, no punctuation beyond commas and full stops
>>"%S4PROMPT%" echo.
>>"%S4PROMPT%" echo or
>>"%S4PROMPT%" echo.
>>"%S4PROMPT%" echo VERDICT: none
>>"%S4PROMPT%" echo WHY: one sentence, plain text, no punctuation beyond commas and full stops
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
echo   It stopped rather than resolving. Since 061 it may do so only for
echo   one of the three things the phase stops for - keying, transmit or
echo   the radio's safety; money past the budget; what the product promises
echo   the operator. This is one of the two conditions that keep the owner
echo   the architect.
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
:heartbeat
powershell -NoProfile -Command "$p='%ROOT%\PHASE_STATUS.md'; if(-not (Test-Path -LiteralPath $p)){ '      no PHASE_STATUS.md - no beat written'; exit }; $bytes=[System.IO.File]::ReadAllBytes($p); $bom=($bytes.Length -ge 3 -and $bytes[0] -eq 239 -and $bytes[1] -eq 187 -and $bytes[2] -eq 191); $raw=[System.Text.Encoding]::UTF8.GetString($bytes); if($bom){ $raw=$raw.Substring(1) }; $CRc=[string][char]13; $LFc=[string][char]10; $nl=$LFc; if($raw.Contains($CRc+$LFc)){ $nl=$CRc+$LFc } elseif($raw.Contains($CRc)){ $nl=$CRc }; $lines=@([regex]::Split($raw, $CRc+$LFc+'|'+$LFc+'|'+$CRc)); if($nl -ne $LFc){ '      normalized: ' + $p + ' is ' + $(if($nl -eq $CRc){'cr'}else{'crlf'}) + $(if($bom){'+bom'}else{''}) + ' - read as line breaks, and written back in its own shape' }; $end=$lines.Count; for($i=0;$i -lt $lines.Count;$i++){ if($lines[$i] -notmatch '^[A-Za-z][A-Za-z0-9_]*:'){ $end=$i; break } }; $hb=-1; $st=-1; for($i=0;$i -lt $end;$i++){ if($hb -lt 0 -and $lines[$i] -cmatch '^HEARTBEAT:'){ $hb=$i }; if($st -lt 0 -and $lines[$i] -cmatch '^STEP:'){ $st=$i } }; $beat=Get-Date -Format 'yyyy-MM-dd HH:mm:ss'; if($hb -ge 0){ $lines[$hb]='HEARTBEAT: '+$beat } elseif($st -ge 0){ $pre=@(); if($st -gt 0){ $pre=@($lines[0..($st-1)]) }; $lines=$pre + @('HEARTBEAT: '+$beat) + @($lines[$st..($lines.Count-1)]) } else { '      REFUSED: the header carries no HEARTBEAT: and no ^STEP: line, and a beat is never appended'; exit }; [System.IO.File]::WriteAllText($p, ($lines -join $nl), (New-Object System.Text.UTF8Encoding($bom))); '      beat ' + $beat"
goto :eof

rem ============================================================
rem  The beat, removed. Called once, on the way out of the loop.
rem ============================================================
rem  See the comment at :stopped for why this exists.
:heartbeatclear
powershell -NoProfile -Command "$p='%ROOT%\PHASE_STATUS.md'; if(-not (Test-Path -LiteralPath $p)){ exit }; $bytes=[System.IO.File]::ReadAllBytes($p); $bom=($bytes.Length -ge 3 -and $bytes[0] -eq 239 -and $bytes[1] -eq 187 -and $bytes[2] -eq 191); $raw=[System.Text.Encoding]::UTF8.GetString($bytes); if($bom){ $raw=$raw.Substring(1) }; $CRc=[string][char]13; $LFc=[string][char]10; $nl=$LFc; if($raw.Contains($CRc+$LFc)){ $nl=$CRc+$LFc } elseif($raw.Contains($CRc)){ $nl=$CRc }; $lines=@([regex]::Split($raw, $CRc+$LFc+'|'+$LFc+'|'+$CRc)); if($nl -ne $LFc){ '      normalized: ' + $p + ' is ' + $(if($nl -eq $CRc){'cr'}else{'crlf'}) + $(if($bom){'+bom'}else{''}) + ' - read as line breaks, and written back in its own shape' }; $end=$lines.Count; for($i=0;$i -lt $lines.Count;$i++){ if($lines[$i] -notmatch '^[A-Za-z][A-Za-z0-9_]*:'){ $end=$i; break } }; $keep=@(); $gone=0; for($i=0;$i -lt $lines.Count;$i++){ if($i -lt $end -and $lines[$i] -cmatch '^HEARTBEAT:'){ $gone++ } else { $keep+=$lines[$i] } }; if($gone -eq 0){ '      no beat to clear - the card already reads stopped'; exit }; [System.IO.File]::WriteAllText($p, ($keep -join $nl), (New-Object System.Text.UTF8Encoding($bom))); '      beat cleared - the launcher has halted and nothing is turning'"
goto :eof

rem ============================================================
rem  The step states and CURRENT_STEP, copied from the record.
rem  The owner's ruling of 2026-08-31.
rem ============================================================
rem  PHASE_OUTCOME.md's HEADER IS THE AUTHORITY AND THIS ONLY COPIES.
rem  It never infers a state, never upgrades one, and never invents a
rem  step line the outcome does not carry. PHASE_OUTCOME.md says so
rem  itself - "a state in the header is always derivable from the
rem  entries below it" - and this is the derivation being carried one
rem  file further rather than a second opinion about it.
rem
rem  THE WRITE SCOPES DO NOT OVERLAP, and that is the point of the
rem  ruling. The launcher owns HEARTBEAT:, the STEP: lines and
rem  CURRENT_STEP:. The executor owns PHASE:, PHASE_SET:, DESCRIPTION:
rem  and WORK_INSTRUCTION:. This routine touches nothing but its own
rem  three, and only the STATE field of a STEP: line - the number and
rem  the delivers text are left byte for byte as they were.
rem
rem  A DISAGREEMENT ABOUT HOW MANY STEPS EXIST IS A FINDING, NOT
rem  SOMETHING TO RECONCILE. If the two headers do not name the same
rem  set of step numbers this writes NOTHING and says so. One of them
rem  is wrong and a launcher cannot know which; quietly making them
rem  agree would destroy the evidence of which.
rem
rem  CURRENT_STEP IS THE LOWEST STEP THAT IS NOT done. Where every step
rem  is done there is no such step, and it is set to the HIGHEST step
rem  number instead - the position a finished phase is actually in.
rem  Nothing reads as in progress, because the states carry that and
rem  they are all done. The alternatives were measured against
rem  phaseView: 0, or the field absent, matches no step, finds no
rem  `in progress` step to fall back to, and the face reads `current
rem  step not identified` about the one phase whose position is not in
rem  any doubt.
rem
rem  IT NEVER APPENDS BELOW THE TERMINATOR, for the reason :heartbeat
rem  carries: parsePhaseStatus collects this format's own keys found
rem  beneath the rule into strandedNames and returns the whole file
rem  NOT READABLE, which takes the entire phase region off the card.
rem  A CURRENT_STEP: that is absent is inserted immediately above the
rem  first ^STEP: line, exactly as a beat is.
rem
rem  NOTHING TO DO MEANS NOTHING WRITTEN. Where the states already
rem  match and CURRENT_STEP already reads what it should, the file is
rem  not rewritten at all - no mtime touched, nothing for the panel's
rem  activity walk to see.
rem
rem  BYTES OUTSIDE THE LINES IT OWNS DO NOT MOVE: read as bytes, BOM
rem  and newline detected and reproduced, as :heartbeat.
:phasesteps
powershell -NoProfile -Command "$p='%ROOT%\PHASE_STATUS.md'; $o='%ROOT%\PHASE_OUTCOME.md'; if(-not (Test-Path -LiteralPath $p)){ '      no PHASE_STATUS.md - no step states written'; exit }; if(-not (Test-Path -LiteralPath $o)){ '      no PHASE_OUTCOME.md - nothing to copy from'; exit }; $ok='not started','in progress','partial','blocked','done'; $bar=[char]124; $src=@{}; $sord=@(); foreach($ln in (Get-Content -LiteralPath $o)){ if($ln -cmatch '^STEP: [0-9]+ \|'){ $q=$ln.Substring(6).Split($bar); if($q.Count -ge 2){ $st=$q[1].Trim(); if($ok -contains $st){ $n=[int]$q[0].Trim(); if(-not $src.ContainsKey($n)){ $src[$n]=$st; $sord+=$n } } } } }; $bytes=[System.IO.File]::ReadAllBytes($p); $bom=($bytes.Length -ge 3 -and $bytes[0] -eq 239 -and $bytes[1] -eq 187 -and $bytes[2] -eq 191); $raw=[System.Text.Encoding]::UTF8.GetString($bytes); if($bom){ $raw=$raw.Substring(1) }; $CRc=[string][char]13; $LFc=[string][char]10; $nl=$LFc; if($raw.Contains($CRc+$LFc)){ $nl=$CRc+$LFc } elseif($raw.Contains($CRc)){ $nl=$CRc }; $lines=@([regex]::Split($raw, $CRc+$LFc+'|'+$LFc+'|'+$CRc)); if($nl -ne $LFc){ '      normalized: ' + $p + ' is ' + $(if($nl -eq $CRc){'cr'}else{'crlf'}) + $(if($bom){'+bom'}else{''}) + ' - read as line breaks, and written back in its own shape' }; $end=$lines.Count; for($i=0;$i -lt $lines.Count;$i++){ if($lines[$i] -notmatch '^[A-Za-z][A-Za-z0-9_]*:'){ $end=$i; break } }; $dst=@{}; $dord=@(); $idx=@{}; $first=-1; for($i=0;$i -lt $end;$i++){ if($lines[$i] -cmatch '^STEP: [0-9]+ \|'){ if($first -lt 0){ $first=$i }; $q=$lines[$i].Substring(6).Split($bar); if($q.Count -ge 2 -and ($ok -contains $q[1].Trim())){ $n=[int]$q[0].Trim(); if(-not $dst.ContainsKey($n)){ $dst[$n]=$q[1].Trim(); $dord+=$n; $idx[$n]=$i } } } }; $a=(@($sord | Sort-Object) -join ','); $b=(@($dord | Sort-Object) -join ','); if($a -ne $b){ '      FINDING: the two headers do not name the same steps. PHASE_OUTCOME.md has [' + $a + '] and PHASE_STATUS.md has [' + $b + ']. NOTHING WAS WRITTEN - one of them is wrong and this cannot know which.'; exit }; if($sord.Count -eq 0){ '      no step lines in the outcome header - nothing written'; exit }; $changed=0; foreach($n in $dord){ if($src[$n] -ne $dst[$n]){ $i=$idx[$n]; $q=$lines[$i].Substring(6).Split($bar); $q[1]=' ' + $src[$n] + ' '; $lines[$i]='STEP: ' + ($q -join $bar); $changed++ } }; $sorted=@($sord | Sort-Object); $open=@($sorted | Where-Object { $src[$_] -ne 'done' }); if($open.Count -gt 0){ $cs=$open[0] } else { $cs=$sorted[$sorted.Count-1] }; $want='CURRENT_STEP: ' + $cs; $ci=-1; for($i=0;$i -lt $end;$i++){ if($lines[$i] -cmatch '^CURRENT_STEP:'){ $ci=$i; break } }; if($ci -ge 0){ if($lines[$ci] -cne $want){ $lines[$ci]=$want; $changed++ } } elseif($first -ge 0){ $pre=@(); if($first -gt 0){ $pre=@($lines[0..($first-1)]) }; $lines=$pre + @($want) + @($lines[$first..($lines.Count-1)]); $changed++ } else { '      REFUSED: no CURRENT_STEP: and no ^STEP: line in the header - never appended below the rule'; exit }; if($changed -eq 0){ '      step states already match the record - nothing written'; exit }; [System.IO.File]::WriteAllText($p, ($lines -join $nl), (New-Object System.Text.UTF8Encoding($bom))); '      card caught up: ' + $changed + ' line(s) from the outcome header, CURRENT_STEP ' + $cs"
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
>>"%ARBPROMPT%" echo You stop the phase - MOVE: stop - for exactly three things, ARBITER.md
>>"%ARBPROMPT%" echo section 6: anything that touches keying, transmit or the radio's
>>"%ARBPROMPT%" echo safety; money past the budget; a decision that changes what the
>>"%ARBPROMPT%" echo product promises the operator. On everything else - including a
>>"%ARBPROMPT%" echo question the last report left in its section 4 - take your own
>>"%ARBPROMPT%" echo recommendation, put it in DECIDED marked author's, overrulable, and
>>"%ARBPROMPT%" echo author the unit on it.
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
rem  A HEADING ON THE LAST LINE IS AN EMPTY SECTION, NOT ITS OWN BODY. 061.
rem  LineNumber is one-based, so the body starts at index i; where the
rem  heading is the file's last line i equals the line count, and the range
rem  from i to count-1 then COUNTS DOWN and hands back the heading itself.
rem  A blank section 4 at the end of a report read as non-empty, the judge
rem  was asked about a heading, and where it answered in a shape the parse
rem  could not read the loop halted at STOP 3 as unknown. Found by 061's
rem  fate fixture, whose report ends on the heading.
for /f "usebackq delims=" %%E in (`powershell -NoProfile -Command "$f='%ROOT%\output.md'; if(-not (Test-Path -LiteralPath $f)){ '1'; exit }; $t=Get-Content -LiteralPath $f; $i=($t | Select-String -Pattern '^## 4\. ' | Select-Object -First 1).LineNumber; if(-not $i){ '1'; exit }; if($i -ge $t.Count){ '1'; exit }; $body=@($t[$i..($t.Count-1)] | Where-Object { $_.Trim() -ne '' }); if($body.Count -eq 0){ '1' } else { '0' }"`) do set "S4EMPTY=%%E"
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
rem  --fixture: ONE PIECE OF THE LOOP, RUN ALONE. 061 tasks 3 and 5.
rem
rem  Each arm calls the SAME subroutine the loop calls, so a fixture
rem  proves the lines that run rather than a copy of them:
rem
rem    judge-section4  :judges4 on the root's output.md - a real,
rem                    restricted, read-only claude call. Exit 1 where
rem                    the loop would halt at STOP 3, 0 where it would
rem                    continue, 2 where the judge could not be read.
rem    record          :readdecision, then :record - steps 4a to 5 for a
rem                    run that exited 0: the state judge, the section 4
rem                    judge, status-check and outcome-append. The exit
rem                    is outcome-append's.
rem
rem  NOTHING ELSE RUNS. No lock is checked or taken, no reload, no
rem  arbiter, nothing is launched and no ledger line is written.
rem
rem  NOT AGAINST THIS REPOSITORY. The header says fixtures only; here it
rem  is mechanical, because a record fixture appends to PHASE_OUTCOME.md
rem  and this repository's own is a phase record.
:fixture
for %%I in ("%HERE%..\..") do set "SELFROOT=%%~fI"
if /i "%ROOT%"=="%SELFROOT%" goto :fixtureself
if /i "%FIXTURE%"=="judge-section4" goto :fxs4
if /i "%FIXTURE%"=="record" goto :fxrecord
echo ERROR: no such fixture: %FIXTURE%
goto :usage

:fixtureself
echo.
echo REFUSED: --fixture against this repository. Fixtures only.
set "RC=2"
goto :end

:fxs4
call :judges4
echo.
if "%S4WANTS%"=="yes" goto :fxs4stop
if "%S4WANTS%"=="unknown" goto :fxs4unknown
echo   FIXTURE judge-section4: THE LOOP CONTINUES - no ruling wanted.
set "RC=0"
goto :end
:fxs4stop
echo   FIXTURE judge-section4: THE LOOP WOULD HALT - STOP 3.
set "RC=1"
goto :end
:fxs4unknown
echo   FIXTURE judge-section4: UNKNOWN - the loop would halt rather than assume.
set "RC=2"
goto :end

:fxrecord
set "ITER=fixture"
set "RUNRC=0"
call :readdecision
call :record
set "RC=%APPRC%"
goto :end

rem ============================================================
:pollgone
echo ERROR: --poll is gone. The watchdog's look interval is a constant in
echo run-unit-watched.bat, and the watchdog has no clock a caller sets. 061.
goto :usage

rem ============================================================
:usage
echo.
echo   run-phase.bat ^<root^> [--max-iterations N] [--budget USD] [--minutes N]
echo   run-phase.bat ^<root^> --fixture ^<judge-section4 ^| record^>
echo.
echo   THE WATCHDOG HAS NO CLOCK OF ITS OWN. A run is killed only after ten
echo   minutes in which its whole process tree accrued no CPU time. A run
echo   that is working is left alone however long it takes.
echo.
echo   THE ONLY CEILINGS ARE --minutes AND --budget, both the owner's:
echo   --minutes N      a wall-clock ceiling on each run. NO DEFAULT - without
echo                    it, no run is ever killed on time.
echo   --budget USD     the phase's spend. Defaults to 25.00.
echo.
echo   --max-iterations defaults to 10. IT IS A BACKSTOP, NOT A STOP
echo                    CONDITION - it saves the night when one of the
echo                    ten fails to fire.
echo   --fixture        one piece of the loop alone, against a fixture root,
echo                    never this repository.
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
