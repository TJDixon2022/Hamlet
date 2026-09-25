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
rem  071 task 3: the exhaustion bar. RULING 3, the arbiter-s of 2026-09-14,
rem  overrulable - at least three distinct failed approaches, each with a
rem  reason it is closed. Named here rather than buried in the test so a
rem  reader can find the number that decides when a night may end.
set "EXBAR=3"
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
rem  --seed, RESTORED BY 063 task 3. HamLet's run-phase.bat carried it from
rem  2026-09-04 (commit 8848311); installing this repository's copy over
rem  HamLet's on 2026-09-13 (commit 230e6c0) dropped it, because this copy
rem  never had it. It is restored here as HamLet's pre-install copy wrote it.
set "SEED=0"
if "%ROOT%"=="" goto :usage
shift

:parse
if "%~1"=="" goto :parsed
if /i "%~1"=="--max-iterations" set "MAXITER=%~2" & shift & shift & goto :parse
if /i "%~1"=="--budget"         set "BUDGET=%~2" & shift & shift & goto :parse
if /i "%~1"=="--minutes"        set "MINUTES=%~2" & shift & shift & goto :parse
if /i "%~1"=="--fixture"        set "FIXTURE=%~2" & shift & shift & goto :parse
if /i "%~1"=="--seed"           set "SEED=1" & shift & goto :parse
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
  rem  072 task 3: THIS WROTE NOTHING BEFORE. The root exists, so there is a
  rem  ledger to write into, and a morning that finds no line at all cannot
  rem  tell a refusal from a launcher that was never started.
  set "STOPWHY=refused before the loop started: there is no PHASE_PLAN.md at this root"
  call :ledgerstop
  goto :end
)

set "WORK=%ROOT%\.run-unit"
if not exist "%WORK%" mkdir "%WORK%"
set "SCRATCH=%WORK%\scratch"
set "SPENT=0"
rem  070: how many iterations launched a unit, and how many were spent on a
rem  redirect that launched none. Console only - the durable record is the
rem  ledger note each redirect writes.
set "LAUNCHEDN=0"
set "REDIRECTED=0"
set "ITER=0"
set "STOPWHY="
rem  LASTPOS and NOPROGRESS are gone with the in-memory stop 10 - 064 task 3.
rem  Stop 10 is read from PHASE_OUTCOME.md by :lastadvances.

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
rem  A STALE LOCK IS NOT A HELD LOCK, AND IT NO LONGER ENDS THE NIGHT
rem  HERE. 067 task 4. This check was `if errorlevel 1`, which is true
rem  for status 5 as well - so a lock left behind by a process that no
rem  longer exists refused the whole phase at the door, and run-unit.bat
rem  never got as far as the orphan clearing that is now its job. The
rem  exit code is compared exactly rather than with errorlevel, which is
rem  a greater-or-equal test and is what fused 5 with 1 in the first
rem  place.
rem
rem  0 free and 5 stale both go on. 1 live and 6 undetermined both
rem  refuse - unknown is not dead, and a phase started against a tree
rem  something else may be writing in is the thing this asks about.
echo Checking the session lock is free...
call "%HERE%lock.bat" status "%ROOT%" >nul
set "LSTAT=%ERRORLEVEL%"
if "%LSTAT%"=="0" goto :lockfree
if "%LSTAT%"=="5" goto :lockstale
echo.
echo REFUSED: the session lock is held. NOTHING WAS STARTED.
if "%LSTAT%"=="6" echo Its holder could not be determined, and unknown is not dead.
if not "%LSTAT%"=="6" echo Its holder is running.
echo run-unit.bat takes its own lock per run; this only checks that
echo nothing else is already writing in that tree.
set "RC=3"
rem  072 task 3: THIS WROTE NOTHING BEFORE, and it is the one the owner is most
rem  likely to meet - a second launcher started by hand over a tree that is
rem  already running. A line is written; the HEARTBEAT is still not touched,
rem  because the beat belongs to the launcher that holds the lock and clearing
rem  it would assert that ITS loop had stopped. :ledgerstop only appends.
set "STOPWHY=refused before the loop started: the session lock is held and nothing was started"
call :ledgerstop
goto :end

:lockstale
echo   A STALE LOCK IS AT THAT ROOT - its owning process is not running.
echo   Going on. run-unit.bat clears an orphaned lock, takes it, and
echo   writes a ledger line saying it did. Nothing is cleared here: the
echo   lock belongs where the writing happens.

:lockfree

rem  073: THE HEADER IS RECONCILED AGAINST THE PLAN, ONCE, HERE.
rem  The last point before the first iteration: the lock is settled, ITER is
rem  still 0, and nothing has been written. Once per run rather than per
rem  iteration, because the plan does not change mid-run and a reconciliation
rem  that repeated would print the same line every iteration and teach the
rem  owner to skim it.
call :reconcile

rem  074: AND THE CARD, IMMEDIATELY AFTER THE RECORD. The owner's ruling of
rem  2026-09-20. Reconciling the record ALONE makes :phasesteps refuse for the
rem  rest of the run - the two headers then name different step sets, so it
rem  writes NOTHING and prints its FINDING every iteration, and the card's step
rem  states freeze at their last values while the panel goes on showing them as
rem  current. That is CLAUDE.md section 0.0's own case, in the file the panel
rem  reads, caused by the fix for the file it does not.
rem
rem  IT SITS HERE, BESIDE THE RECORD'S, AND NOT INSIDE IT - author's,
rem  overrulable. Both reconciliations answer to THE PLAN, so neither needs the
rem  other to have run: a card whose record could not be reconciled is still
rem  brought up to the plan. The console reads record then card, which is the
rem  direction :phasesteps later copies in.
rem
rem  IT IS A SECOND FORM OF THE SPLICE, WRITTEN AS ONE, and not :reconcile
rem  pointed at another path. PHASE_STATUS.md is LF-ONLY WITH NO BOM where
rem  PHASE_OUTCOME.md is CRLF WITH ONE. Unit 073 shipped a doubled BOM and an
rem  orphaned LF into the record and caught both only by comparing bytes; a
rem  routine carrying the record's conventions into this file would reproduce
rem  that class of defect in the other direction, and no reading would show it.
rem
rem  IT CANNOT END A NIGHT. Ruling 3 rejected a halt here BY NAME. Where the
rem  card cannot be written - missing, locked, unreadable, a shape this does
rem  not recognise - it prints what it could not do and the run goes on. No
rem  exit code, no stop number, no twenty-fourth halt. A stale card is a stop,
rem  and under the owner's ruling of 2026-09-14 a stop is failure.
call :reconcilecard

rem ============================================================
rem  THE LOOP
rem ============================================================
:iterate
set /a ITER+=1
echo.
echo ------------------------------------------------------------
echo  iteration %ITER%
if exist "%WORK%\redirect.txt" echo  A REDIRECT IS IN FORCE - this iteration authors under it
echo ------------------------------------------------------------

if %ITER% GTR %MAXITER% (
  echo.
  echo BACKSTOP: %MAXITER% iterations reached.
  echo THIS IS NOT A STOP CONDITION. It is the thing that saves the night
  echo when a stop condition fails to fire - AND, since unit 070, it is also
  echo the ordinary end of a loop that kept redirecting. Read the line above:
  echo if iterations were spent on redirects, this is the brake doing its job
  echo and the owner-s ruling that the bound is --budget, --minutes and
  echo --max-iterations. If none were, a stop condition is broken and THAT is
  echo the finding.
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

rem --- stop 1, extended: IS THE OWNER'S VERDICT ALL THAT IS LEFT? --------
rem  THE OWNER'S RULING OF 2026-09-14: when every remaining step's must-pass
rem  is the owner's own verdict, the loop writes the review sheet once - if
rem  the plan asks for one - and halts. It does not author units toward a
rem  verdict it cannot give. 065 task 3.
rem
rem  WHY. On HamLet overnight on 2026-09-13, once units 345 to 348 had
rem  finished every work step, the only thing left was the owner saying the
rem  phase passed. The loop authored six more units anyway - a review sheet,
rem  window measurements, two re-recordings of a finished step - for 66
rem  dollars, each permitted by a ruling it wrote for itself.
rem
rem  STOP 1, NOT A NEW STOP NUMBER. Stop 1 is the phase having nothing left
rem  the loop can do, and it exits 0; a phase waiting only on the owner is
rem  that case, so it is stop 1 with its own message, and exits 0.
rem
rem  DECIDED FROM THE FILES, BEFORE THE ARBITER IS CALLED, so the halt costs
rem  no prompt. The halt fires only where at least one step is not done, no
rem  open step carries an unmet work criterion, every open step carries
rem  criteria in the form, and at least one unmet criterion is the owner's.
rem  A phase with one work criterion left runs, normally. A step with no
rem  criteria in the form is not read as finished - that is unknown.
call :ownerwait
if "%OW_HALT%"=="1" goto :ownerhalt

rem --- condition 10: no progress, COUNTED IN CRITERIA ----------------
rem  THE OWNER'S RULING OF 2026-09-14: a unit may claim only criterion k
rem  of step N, unmet to met, and two consecutive units that move no
rem  criterion are stop 10. 064 task 3.
rem
rem  WHAT THIS REPLACED, AND WHY IT FAILED. Stop 10 compared a position
rem  string - every step's state - with the previous iteration's and
rem  halted after two unchanged. On HamLet overnight on 2026-09-13 the
rem  loop ran ten units for 111 dollars, the last six moved no step, and
rem  the position still changed almost every time: unit 350 was authored
rem  into done step 0 and the state judge re-recorded it partial, 351 set
rem  it back to done, 352 moved step 3 to blocked. A state flipping is not
rem  progress, and a rule that reads states can be walked past by one.
rem
rem  READ FROM THE PHASE RECORD, NOT HELD IN A VARIABLE. The counter was
rem  NOPROGRESS, in memory, so a loop that restarted began again at zero.
rem  :lastadvances reads the last two entries' ADVANCED lines out of
rem  PHASE_OUTCOME.md, which survives any restart.
rem
rem  A RUN THAT NEVER RAN IS NOT COUNTED. The owner's ruling of 2026-08-29
rem  is that a failed run ends its unit and does not halt the phase, so
rem  both entries must carry FATE: executed for stop 10 to fire.
rem
rem  TWO BLOCKER-CLEARS IN A ROW HALT TOO, AND SAY SO IN THOSE WORDS. The
rem  blocker form is how the layer repairs itself, so it stays permitted;
rem  bounded, or it becomes the hole every unit climbs through. It is not
rem  stop 10 - a ledger reader must be able to tell which rule fired.
call :lastadvances
rem  070: NO ADVANCE REDIRECTS, IT DOES NOT HALT. The owner-s ruling of
rem  2026-09-14: stopping is failure, and a unit that blocks produces the next
rem  attempt at the same criterion by a different approach. Stop 10 halted the
rem  night the second time a criterion resisted, with routes in the record it
rem  had never tried - units 066, 068 and 069 built that record, put it in
rem  front of the arbiter and made the prompt open on the plan, and nothing
rem  used it.
rem
rem  THE BLOCKER-TWICE HALT STAYS A HALT, and 070 task 1 gives the reason: the
rem  redirect-s requirement is an approach the record does not show failing AT
rem  THAT CRITERION, and a blocker-clear names no criterion. There is nothing
rem  to send it back to. 064-s bound stands.
if "%LA_KIND%"=="blocker" goto :blockertwice
if not "%LA_KIND%"=="no" goto :noredirect
if "%LA_SAME%"=="yes" goto :redirectsame
goto :redirectwander
:noredirect

rem --- 3. the arbiter ------------------------------------------
rem  A SEEDED FIRST ITERATION SKIPS AUTHORING. --seed says the owner
rem  has put a WORK_INSTRUCTIONS.md in the tree himself and wants it
rem  executed as written, so iteration 1 runs it and the arbiter takes
rem  over from iteration 2. It is a flag and not a detected condition
rem  on purpose: an extract for any other reason also leaves that file
rem  newer than output.md, and guessing from timestamps would silently
rem  skip authoring on a run nobody meant to seed.
rem
rem  The decision block is still read out of the seed instruction, so
rem  a seed carries STEP:, APPROACH:, MOVE: and the rest exactly as an
rem  authored one does. Without them stage 5 records "not recorded".
rem
rem  Restored by 063 task 3 as HamLet's run-phase.bat carried it before
rem  commit 230e6c0 - the block, its comment and its stop reason verbatim.
rem  ADVANCES is still required of a seed, because :noadvances below reads
rem  the same decision block whichever way it was reached.
if "%ITER%"=="1" if "%SEED%"=="1" (
  echo.
  echo   [3] arbiter - SKIPPED, this iteration runs the seed instruction
  if not exist "%ROOT%\WORK_INSTRUCTIONS.md" (
    set "STOPWHY=--seed was given and there is no WORK_INSTRUCTIONS.md to run"
    goto :stopped
  )
  call :heartbeat
  call :readdecision
  set "ARBRC=0"
  goto :seeded
)
echo.
echo   [3] arbiter - authoring the next unit, restricted
call :attemptline
call :heartbeat
call :arbiter
call :heartbeat
:seeded
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

rem  ADVANCES NAMES A CRITERION OR IS THE BLOCKER FORM. 064 task 2. The
rem  owner's ruling of 2026-09-14: a unit may claim only criterion k of
rem  step N. Presence was all that was checked before; now the field must
rem  read `step N criterion k`, or `none - clears a blocker:` naming a
rem  unit number or a criterion it unblocks. Anything else is refused
rem  here, where the absent field is refused, and nothing is launched.
rem
rem  NOT A WORD LIST. The request was to reject ground, record, readiness
rem  and similar as advances. "And similar" cannot be measured, and a unit
rem  that wants the work finds a synonym. A named criterion whose marker
rem  must flip is stronger: vocabulary stops mattering. Author's, overrulable.
call :advparse
if "%ADV_KIND%"=="bad" goto :badadvances

rem  A DONE STEP IS CLOSED. The owner's ruling of 2026-09-14: the arbiter
rem  does not author into a step whose state is done, and only the owner
rem  reopens one, by a ruling in the plan. Refused BEFORE THE UNIT RUNS -
rem  the unit is the expensive prompt, and under --seed no prompt at all
rem  has been spent by this point. The state is read from PHASE_OUTCOME.md's
rem  header, the file :position and stop 1 already read, for both the
rem  step the decision block names and the step ADVANCES names.
call :stepstate "%A_STEP%"
if "%SS_STATE%"=="done" goto :stepclosed
if not "%ADV_KIND%"=="criterion" goto :advchecked
if "%ADV_STEP%"=="%A_STEP%" goto :advsamestep
call :stepstate "%ADV_STEP%"
if "%SS_STATE%"=="done" goto :stepclosed
rem  071: BOTH PATHS MUST REACH THE EXHAUSTION DISPATCH BELOW. The first
rem  cut jumped straight to :advcount where the decision block and ADVANCES
rem  name the SAME step - which is the ordinary case - so MOVE: exhausted was
rem  parsed, written and never looked at, and the fixture launched a unit
rem  instead. Found by running the arm, not by reading the file.
:advsamestep

rem  071 task 3. AN ENDING MUST BE DEMONSTRATED, NOT DECLARED.
rem
rem  MOVE: exhausted is the arbiter saying the routes to this criterion have
rem  run out. Before this unit there was NO WAY TO SAY IT - the only
rem  arbiter-driven ending was MOVE: stop, which is STOP 4 and is reserved for
rem  the three things the phase stops for, and 070's redirect block tells the
rem  arbiter in terms that running out of routes is not something it may
rem  assert. So this unit builds the declaration AND the test that it is true.
rem
rem  WHERE IT SITS, AND WHY EACH SIDE OF IT MATTERS.
rem    AFTER :arbstop        MOVE: stop keeps priority. The three stops halt
rem                          on sight and are never subject to this test.
rem    AFTER the done-step   a closed step is still closed.
rem    AFTER :advparse       so the criterion is validated, not taken on
rem                          trust from the decision block's prose.
rem    BEFORE 068's matcher  an exhaustion claim launches no unit, so there
rem                          is no approach for the matcher to judge.
rem    LONG AFTER :ownerwait which runs at the TOP of the iteration, before
rem                          the arbiter is called at all - so criterion 4.5
rem                          holds BY CONSTRUCTION and not by a guard here.
rem
rem  A CLAIM THAT IS NOT THE CRITERION FORM IS REFUSED. An exhaustion claim
rem  names the criterion it says is exhausted; a blocker-clear names none.
if /i not "%A_MOVE%"=="exhausted" goto :notexhaust
if not "%ADV_KIND%"=="criterion" goto :exhaustnocrit
call :exhausttest "%ADV_STEP%.%ADV_CRIT%"
if "%EX_OK%"=="yes" goto :exhaustok
goto :exhaustthin
:notexhaust
:advcount
rem  AN APPROACH ALREADY RECORDED AS FAILED IS REFUSED. 068 tasks 4 and 5,
rem  step 2 of the plan, under the owner's ruling of 2026-09-14 that
rem  stopping is failure. Unit 066 gave the arbiter a memory and NOTHING READ
rem  IT; this is the first mechanism in this layer that pushes rather than
rem  stops.
rem
rem  REFUSED BEFORE THE UNIT RUNS, where every other bad ADVANCES is refused,
rem  so nothing is spent on a route already known to fail.
rem
rem  ONLY A VERDICT OF no BLOCKS, AND ONLY WHERE THE FATE IS executed.
rem  ADVANCED carries yes, no, blocker or not recorded. yes SUCCEEDED and never
rem  blocks a later attempt - criterion 2.3. blocker moved no criterion but was
rem  not an attempt at one. not recorded is the launcher saying it could not
rem  tell, and UNKNOWN IS NOT FAILURE - the same principle 067 applied to a lock
rem  whose holder could not be determined.
rem
rem  AND THE FATE, WHICH THE FIRST CUT OF THIS CHECK IGNORED. run-unit exit 3 -
rem  the run itself failed, claude exited non-zero or its record could not be
rem  read - records FATE: never ran, and the criterion cannot have flipped, so
rem  ADVANCED is no. The approach was therefore written into the record as
rem  FAILED WHEN IT WAS NEVER TRIED, and the next arbiter naming it would have
rem  been refused for a run that never happened. THAT IS EXACTLY THE FAILURE
rem  task 5 names - a false match blocking a route the arbiter has never tried -
rem  so a fate that is not executed does not block. Found by reading this check
rem  back against its own instruction, not by a fixture. Author's, overrulable;
rem  it narrows CPS-DEC-075 and CPS-DEC-076 records the narrowing.
rem
rem  ONLY THE CRITERION ADVANCES NAMES. An attempt recorded against a
rem  DIFFERENT criterion never blocks this one - 2.3 again - because the
rem  comparison starts by matching N.k exactly. A blocker-clear is not checked
rem  at all: the criterion it unblocks is derived rather than named, and two
rem  blocker-clears in a row already halt under their own rule. Author's,
rem  overrulable.
rem
rem  HOW A MATCH IS JUDGED, IN ONE SENTENCE FOR THE OWNER: two approaches
rem  match when the shorter one has AT LEAST FOUR DISTINCTIVE WORDS - four or
rem  more letters, not counting a short list of connectives - and EVERY ONE OF
rem  THEM also appears in the longer one.
rem
rem  IT ERRS TOWARD LETTING AN APPROACH THROUGH, and that is the ruling rather
rem  than a preference. A false match blocks a route the arbiter has never
rem  tried, which is the failure this whole phase exists to prevent; a missed
rem  match costs one repeated unit, which the no-advance path already catches.
rem  The real arbiter-written approaches of units 066, 067 and 068 carry 21, 17
rem  and 24 distinctive words - measured, not estimated - and containment of one
rem  such set inside another
rem  is vanishingly unlikely unless they really are the same approach.
rem
rem  THE FLOOR OF FOUR IS WHY IT IS SAFE, and it applies to BOTH SIDES. An
rem  approach with fewer than four distinctive words is a label rather than an
rem  approach, and is let through rather than judged.
rem
rem  REJECTED: a similarity score. The instruction forbids one that cannot be
rem  explained in a sentence, and every score available here - Jaccard, edit
rem  distance, a token ratio - needs a threshold nobody measured, which is how
rem  an unmeasured number becomes a fact nobody remembers choosing.
rem  REJECTED: a list of forbidden words, for the reason 064 already recorded -
rem  a unit that wants the work finds a synonym.
for /f "usebackq tokens=1,* delims==" %%A in (`powershell -NoProfile -Command "$o='%ROOT%\PHASE_OUTCOME.md'; $af='%WORK%\approach.txt'; $want='%ADV_STEP%.%ADV_CRIT%'; if(-not (Test-Path -LiteralPath $o)){ exit }; if(-not (Test-Path -LiteralPath $af)){ exit }; $stop=@('that this with from than then when which into over rather instead before after every each been were will would should could must have does such also only more most same other because while they them their there' -split ' '); function W($t){ $w=@([regex]::Matches($t.ToLower(), '[a-z0-9]+') | ForEach-Object { $_.Value } | Where-Object { ($_.Length -ge 4) -and ($stop -notcontains $_) }); ,@($w | Select-Object -Unique) }; $mine=W([IO.File]::ReadAllText($af, [Text.Encoding]::UTF8)); if($mine.Count -lt 4){ exit }; $raw=[Text.Encoding]::UTF8.GetString([IO.File]::ReadAllBytes($o)); if(($raw.Length -gt 0) -and ($raw[0] -eq [char]0xFEFF)){ $raw=$raw.Substring(1) }; $inf=$false; foreach($ln in [regex]::Split($raw, [char]13+[string][char]10+'|'+[string][char]10+'|'+[string][char]13)){ if($ln -match '^\s*(```|~~~)'){ $inf=-not $inf; continue }; if($inf){ continue }; if($ln -notmatch '^ATTEMPT: *([0-9]+\.[0-9]+) *\| *(.*)$'){ continue }; if($Matches[1] -ne $want){ continue }; $f=$Matches[2] -split '\|', 4; if($f.Count -lt 4){ continue }; $who=$f[0].Trim(); $verdict=$f[1].Trim(); $fate=$f[2].Trim(); if($verdict -ne 'no'){ continue }; if($fate -ne 'executed'){ continue }; $their=W($f[3]); if($their.Count -lt 4){ continue }; $short=$mine; $long=$their; if($their.Count -lt $mine.Count){ $short=$their; $long=$mine }; $hit=$true; foreach($w in $short){ if($long -notcontains $w){ $hit=$false; break } }; if(-not $hit){ continue }; 'AP_MATCH=yes'; 'AP_WHO=' + $who; 'AP_NAMED=' + $(if($who -match 'launched'){ 'yes' } else { 'no' }); 'AP_WORDS=' + (($short | Sort-Object) -join ' '); 'AP_THEIRS=' + ((($f[3] -replace '[&|<>^%%]','') -replace '\s+',' ').Trim()); break }"`) do set "%%A=%%B"
rem  070 task 2: A REDIRECTED INSTRUCTION MUST NAME ONE OF THE CRITERIA IT WAS
rem  SENT BACK TO. Checked here, after the arbiter has authored and before the
rem  unit is launched, beside every other thing that is refused at this point.
rem  An instruction that names something else is redirected AGAIN with the
rem  requirement restated - it is not halted, and it is not let through.
rem
rem  THE APPROACH HALF OF THE REQUIREMENT IS 068-S MATCHER, unchanged and not
rem  reimplemented. It runs just above and now redirects instead of halting.
call :redirectreq
if "%RQ_ON%"=="yes" echo       redirect   : in force - must name one of %RQ_WANT%, this names %RQ_MINE%
if "%RQ_MET%"=="no" goto :wrongcriterion

if "%AP_MATCH%"=="yes" goto :approachrepeat

rem  A WHY THAT CITES NO LINE OF THE PLAN IS REFUSED. 069 task 4, criterion
rem  6.4, on the owner's ruling of 2026-09-19 that the plan leads and the
rem  report follows. Unit 064 made the CLAIM honest - ADVANCES must name a
rem  criterion - and nothing ever looked at the REASONING. An arbiter fixated
rem  on the last report picks whichever criterion sits nearest its complaints
rem  and the instruction reads valid, because the refusal is downstream of the
rem  fixation.
rem
rem  WHAT COUNTS AS CITING THE PLAN, and any one of the three is enough:
rem    a criterion id N.k that EXISTS in the plan,
rem    the word step followed by a step number that exists, or
rem    AT LEAST THREE DISTINCTIVE WORDS of the WHY appearing in ONE SINGLE
rem    line of the plan - the same vocabulary 068's matcher uses, four or
rem    more letters and not a connective.
rem
rem  THREE IS MEASURED, NOT CHOSEN. The four real arbiter-written WHY lines
rem  that exist - units 066, 067, 068 and 069 - score 4, 4, 3 and 4 against
rem  their closest plan line. THE FLOOR IS UNIT 068'S, AT THREE. Built at four
rem  first, this check REFUSED 068'S OWN INSTRUCTION - a unit that reasoned
rem  correctly from step 2 and shipped - which is exactly the false refusal task
rem  4 names as the worse failure. A threshold above the lowest honest example
rem  on record is a threshold nobody measured.
rem
rem  WHAT THREE COSTS, said plainly: a WHY reasoning purely from the last
rem  report that happens to share three plan words now passes. Measured on a
rem  written example - output.md raised three mismatches and its author asked
rem  whether the panel should be taught where things live - which scores three
rem  and runs. A WHY with nothing of the plan in it at all still scores two and
rem  is still refused. Criterion 6.5 is the guard for what three lets through.
rem
rem  IT ERRS TOWARD LETTING A WHY THROUGH, and that is the instruction, not a
rem  preference: a false refusal costs an iteration at a criterion the arbiter
rem  may have reasoned about correctly, and under ruling 2 blocking a route
rem  reached honestly is the worse failure. A missed one costs one unit that
rem  criterion 6.5's judge question can still catch.
rem
rem  SO IT IS DELIBERATELY WEAK, AND THE REPORT SAYS SO. A WHY that quotes the
rem  plan and then reasons entirely from the last report PASSES THIS CHECK.
rem  6.5 is the guard for that case; this one catches only a WHY with nothing
rem  of the plan in it at all.
rem
rem  IT CHECKS WHAT WAS CITED, NEVER INTENT. The same shape as 067's reversal
rem  question: a fact about the text, not a reading of the author.
rem
rem  WHERE THE WHY COULD NOT BE READ - no snapshot, no plan - WY_SEEN stays
rem  empty and NOTHING IS REFUSED. Unknown is not a citation failure.
for /f "usebackq tokens=1,* delims==" %%A in (`powershell -NoProfile -Command "$wf='%WORK%\why.txt'; $pl='%ROOT%\PHASE_PLAN.md'; if(-not (Test-Path -LiteralPath $wf)){ exit }; if(-not (Test-Path -LiteralPath $pl)){ exit }; $why=[IO.File]::ReadAllText($wf, [Text.Encoding]::UTF8); $raw=[Text.Encoding]::UTF8.GetString([IO.File]::ReadAllBytes($pl)); if(($raw.Length -gt 0) -and ($raw[0] -eq [char]0xFEFF)){ $raw=$raw.Substring(1) }; $lines=[regex]::Split($raw, [char]13+[string][char]10+'|'+[string][char]10+'|'+[string][char]13); $ids=@(); $steps=@(); foreach($ln in $lines){ if($ln -match '^- \[( |x)\] ([0-9]+)\.([0-9]+) '){ $ids += ($Matches[2] + '.' + $Matches[3]); $steps += $Matches[2] } }; $steps=@($steps | Select-Object -Unique); $cid=@(); foreach($i in $ids){ if($why -match ([regex]::Escape($i) + '(\D|$)')){ $cid += $i } }; $cst=@(); foreach($n in $steps){ if($why -match ('(?i)step\s+' + $n + '(\D|$)')){ $cst += $n } }; $stop=@('that this with from than then when which into over rather instead before after every each been were will would should could must have does such also only more most same other because while they them their there' -split ' '); function W($t){ $w=@([regex]::Matches($t.ToLower(), '[a-z0-9]+') | ForEach-Object { $_.Value } | Where-Object { ($_.Length -ge 4) -and ($stop -notcontains $_) }); ,@($w | Select-Object -Unique) }; $ww=W($why); $best=0; $bl=''; foreach($ln in $lines){ if($ln.Trim().Length -lt 12){ continue }; $lw=W($ln); $n=0; foreach($x in $ww){ if($lw -contains $x){ $n++ } }; if($n -gt $best){ $best=$n; $bl=$ln.Trim() } }; $ok=(($cid.Count -gt 0) -or ($cst.Count -gt 0) -or ($best -ge 3)); 'WY_SEEN=yes'; 'WY_CITES=' + $(if($ok){ 'yes' } else { 'no' }); 'WY_IDS=' + $(if($cid.Count){ $cid -join ' ' } else { 'none' }); 'WY_STEPS=' + $(if($cst.Count){ $cst -join ' ' } else { 'none' }); 'WY_HITS=' + $best; 'WY_WORDS=' + $(if($ww.Count){ (($ww | Sort-Object) -join ' ') } else { 'none' }); 'WY_BEST=' + ((($bl -replace '[&|<>^%%]','') -replace '\s+',' ').Trim())"`) do set "%%A=%%B"
if "%WY_SEEN%"=="yes" echo       why        : cites ids %WY_IDS%, steps %WY_STEPS%, and shares %WY_HITS% words with its closest plan line - three are needed
if "%WY_CITES%"=="no" goto :whynoplan

rem  THE COUNT BEFORE THE UNIT RUNS, by criteria-count.bat - a script.
call :critcount "%ADV_STEP%" "%ADV_CRIT%"
set "CB_MET=%CC_MET%"
set "CB_TOTAL=%CC_TOTAL%"
set "CB_CRIT=%CC_CRIT%"
rem  A UNIT CANNOT CLAIM THE OWNER'S VERDICT. 065 task 2. A criterion marked
rem  *owner's verdict* is judged by the owner alone, and nothing in the loop -
rem  not a unit, not the judge, not this launcher - turns it to - [x]. So an
rem  ADVANCES naming one is refused here, before the unit runs, where a bad
rem  ADVANCES is refused, with its own message.
if "%CB_CRIT%"=="owner" goto :ownercrit
echo       advances   : step %ADV_STEP% criterion %ADV_CRIT% - before the unit it is %CB_CRIT%, step %ADV_STEP% has %CB_MET% of %CB_TOTAL% met
:advchecked
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
rem  THE MOMENT THIS UNIT WAS LAUNCHED, WRITTEN LAST BEFORE THE LAUNCH.
rem  067 task 2. It is what makes output.md at the root THIS unit's report
rem  rather than any report. Nothing between this line and the launch writes
rem  to the tree, so a report older than the stamp cannot be this run's.
rem  070: THE REDIRECT IS SPENT. This instruction named a criterion it was sent
rem  to with an approach the record does not show failing, so the requirement is
rem  met and the state is cleared. If this unit moves nothing, :lastadvances
rem  writes a fresh one from the record next iteration - the redirect is never
rem  carried forward on memory.
if exist "%WORK%\redirect.txt" del /q "%WORK%\redirect.txt" 2>nul
set /a LAUNCHEDN+=1
call :stamp
call "%HERE%run-unit-watched.bat" %ITER% "%ROOT%" %MINARG%
set "RUNRC=%ERRORLEVEL%"
echo       run-unit-watched exit %RUNRC%
call :heartbeat

rem --- 4pre. IS THERE A REPORT OF THIS UNIT'S TO JUDGE AT ALL? ----
rem  067 task 2, AND IT RUNS BEFORE :record BECAUSE :record IS WHAT APPENDS.
rem
rem  HamLet, 2026-09-14, the first real run of this loop. Iteration 1 ran and
rem  was judged. Iteration 2's unit could not take the session lock, so
rem  NOTHING RAN - and this file called :record anyway, handed the judge
rem  ITERATION 1'S output.md, which was still lying at the root, and appended
rem  a UNIT 2 - STEP 1 entry carrying iteration 1's cost and iteration 1's
rem  prose. PHASE_OUTCOME.md is append-only, so that entry cannot be taken
rem  back; and it is the file stop 10 reads, the file the attempt record
rem  lives in, and the file an ending is to be demonstrated from.
rem
rem  THE ORDER WAS THE WHOLE FAULT. The exit code WAS read - below, at
rem  :ambiguous1, which correctly said "the run could not take the session
rem  lock. Nothing ran." It was read AFTER the entry had been written.
rem
rem  TWO CHECKS, IN THIS ORDER, AND THE SECOND IS THE LOAD-BEARING ONE.
rem    1. did anything launch? The exit codes that mean NOTHING WAS LAUNCHED
rem       are run-unit.bat's own, from its header: 1 with no kill in
rem       watched.log (the session lock was held), 2 (usage or bad root),
rem       7 (the root is not a git repository).
rem    2. is output.md newer than the stamp written just before the launch?
rem
rem  THE FIRST ALONE WOULD HAVE STOPPED HamLet'S ENTRY, AND IS NOT ENOUGH.
rem  A lock held by a LIVE session means another session is writing in that
rem  tree and will drop its own output.md at that root, AFTER our stamp; a
rem  check that read only an exit code would judge that one the next time
rem  round. The stamp is what makes a report this unit's rather than
rem  merely recent.
rem
rem  NOTHING IS APPENDED ON EITHER PATH. Not an entry, not a placeholder,
rem  not a `not recorded` row. An entry for a unit that did not run is the
rem  fault, and a quieter spelling of it is still it.
set "LAUNCHED=yes"
if "%RUNRC%"=="2" set "LAUNCHED=no"
if "%RUNRC%"=="7" set "LAUNCHED=no"
if not "%RUNRC%"=="1" goto :launchknown
call :killcount
if "%KILLED%"=="0" set "LAUNCHED=no"
:launchknown
call :ownreport
echo       report     : %OWNWHY%
if "%LAUNCHED%"=="no" goto :nothingran
if not "%OWNREPORT%"=="yes" goto :noreport

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

rem --- 4a2. did the named criterion flip? THE SCRIPT COUNTS. -------
rem  064 task 3. Counted again now the unit has reported, against the count
rem  taken before it ran. Before the judge, so the judge can be told what
rem  the script saw flip and asked only whether it was honestly met.
call :critafter

rem --- 4b. what state did the run leave the step in? ------------
call :judgestate
call :advverdict
call :attemptid

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
set "OA_ADVANCED=%ADVANCED_OUT%"
rem  066 task 3: the criterion this unit was run against, from :attemptid -
rem  empty, and so undefined, where there is none - and the approach
rem  snapshot :readdecision took before the unit ran. outcome-append.bat
rem  writes the ATTEMPT line from these; nothing a session wrote enters it.
set "OA_CRITERION=%AT_CRIT%"
set "OA_APPROACHFILE=%WORK%\approach.txt"
rem  068 task 3: the identity that does not repeat across a restart, read
rem  from the launch stamp at :stamp. Empty only where the stamp could not
rem  be read, and then the line is written in the pre-068 form rather than
rem  carrying a composed value.
set "OA_ATTEMPTID=%ATTEMPTID%"
call "%HERE%outcome-append.bat" --from-env >nul
set "APPRC=%ERRORLEVEL%"
if "%APPRC%"=="0" echo       recorded step %A_STEP% as %J_STATE%, fate %RUNFATE%, cost %RUNCOST%
if not "%APPRC%"=="0" echo       NOT RECORDED - outcome-append exit %APPRC%

rem  NO ADVANCE AND A BLOCKER-CLEAR GO IN THE LEDGER TOO. 064 task 3. The
rem  ledger is what the owner reads instead of watching, and ten complete
rem  lines with ten valid reports is exactly what the overnight run on
rem  HamLet showed him while six of those units moved nothing. A note row,
rem  not a second run line, so an iteration is still counted once. The
rem  record fixture writes no ledger lines, and does not start here.
if "%ITER%"=="fixture" goto :eof
if "%ADVANCED_OUT%"=="no" call :ledgeradv "no advance"
if "%ADVANCED_OUT%"=="blocker" call :ledgeradv "blocker-clear"
goto :eof

:ledgeradv
set "NOWSTAMP="
for /f "usebackq delims=" %%D in (`powershell -NoProfile -Command "(Get-Date).ToString('yyyy-MM-ddTHH:mm')"`) do set "NOWSTAMP=%%D"
call "%HERE%ledger.bat" "%ITER%" "%NOWSTAMP%" "%NOWSTAMP%" "note" "%~1 - %ADVNOTE%" "none - not a run" "%ROOT%" >nul
echo       ledger     : %~1 noted
goto :eof

rem ============================================================
rem  THE LAUNCH STAMP. 067 task 2.
rem
rem  A FILE, NOT A TIMESTAMP IN A VARIABLE, AND THAT IS THE CHOICE.
rem  The instruction offered two: compare output.md's write time against a
rem  moment read from the clock, or write a marker the report must be newer
rem  than. THE MARKER WAS CHOSEN, because the two sides are then the same
rem  kind of reading taken by the same subsystem - two LastWriteTimeUtc
rem  values off one volume - and nothing has to agree about a format, a
rem  time zone, or how many digits of a second survive being printed.
rem
rem  THE CLOCK SIDE WAS MEASURED BEFORE IT WAS REJECTED. The only launch
rem  moment the layer already held is run-unit.bat's STARTED, which is
rem  (Get-Date).ToString(yyyy-MM-ddTHH:mm) - MINUTE precision, no seconds.
rem  Comparing a file write against that would call a report written up to
rem  59 seconds BEFORE the launch this unit's. That is the defect wearing
rem  a fix's coat. Nothing else in .run-unit\ records a launch moment:
rem  last-run.json carries duration_api_ms and no start.
rem
rem  IT IS REWRITTEN EVERY ITERATION, and the comparison is STRICTLY newer.
rem  Equal is refused, deliberately: refusing a real report is a halt the
rem  owner can see and undo, and accepting a stale one is an entry in an
rem  append-only record that he cannot.
:stamp
if exist "%WORK%\launch.stamp" del /q "%WORK%\launch.stamp" >nul 2>&1
>"%WORK%\launch.stamp" echo iteration %ITER% of this run-phase process
powershell -NoProfile -Command "$s='%WORK%\launch.stamp'; if(Test-Path -LiteralPath $s){ '      launch stamp: ' + (Get-Item -LiteralPath $s).LastWriteTimeUtc.ToString('yyyy-MM-ddTHH:mm:ss.fff') + 'Z - output.md must be newer than this to be judged' } else { '      THE LAUNCH STAMP WAS NOT WRITTEN - no report can be shown to be this unit-s, and the loop halts rather than judge one' }"
rem  AND THE STAMP IS ALSO THIS UNIT'S IDENTITY. 068 task 3.
rem
rem  THE ATTEMPT RECORD IDENTIFIES A UNIT BY THE LOOP'S ITERATION, and that
rem  counts from 1 again after a restart - unit 066 measured it. So two
rem  different units are both "unit 1", and 068 criterion 2.1 asks the
rem  refusal to NAME the earlier unit. A message naming unit 1 when two
rem  units have been unit 1 is worse than no message.
rem
rem  THE IDENTITY IS THE LAUNCH STAMP'S OWN WRITE TIME, IN UTC. Author's,
rem  overrulable. It is MEASURED rather than composed - the same file
rem  reading 067's ownership check already compares against - it is unique
rem  per launch because the file is rewritten immediately before each one,
rem  it survives a restart because it is a wall clock and not a counter,
rem  and it needs no new file, no new state and nothing to keep in step.
rem
rem  REJECTED: a sequence counted from the record. That is renumbering in
rem  disguise, it collides on a fresh record, and ruling 4 forbids touching
rem  what is already written. REJECTED: the phase-set date with the unit
rem  number - it collides on exactly the case this exists for, a restart
rem  inside one phase, which is what HamLet did. REJECTED: a hash of the
rem  record's length - it changes meaning as the file grows and says
rem  nothing a reader can act on.
rem
rem  MILLISECONDS ARE KEPT even though a unit run cannot finish inside one
rem  second - the watchdog's first look is at sixty - because they cost
rem  nothing and remove the argument.
for /f "usebackq delims=" %%S in (`powershell -NoProfile -Command "$s='%WORK%\launch.stamp'; if(Test-Path -LiteralPath $s){ (Get-Item -LiteralPath $s).LastWriteTimeUtc.ToString('yyyy-MM-ddTHH:mm:ss.fff') + 'Z' }"`) do set "ATTEMPTID=%%S"
if not defined ATTEMPTID set "ATTEMPTID=unknown"
goto :eof

rem ============================================================
rem  THE PHASE GOAL AND THE CRITERIA, WRITTEN FIRST. 069 criterion 6.1.
rem
rem  The goal is the PHASE: line of PHASE_OUTCOME.md's header - the phase as it
rem  is actually running, not as the plan proposed it. Step states come from the
rem  same header, criteria and titles from PHASE_PLAN.md, and a step the header
rem  does not mention is treated as not started rather than skipped.
rem
rem  THE TARGET STEP IS THE LOWEST-NUMBERED STEP THAT IS NOT done, and its unmet
rem  criteria are listed first and labelled THE WORK. The other open steps follow
rem  with their unmet criteria only. Author's, overrulable: the arbiter may
rem  legitimately author against another open step, and a block that showed only
rem  one would push it toward a criterion the plan may not put first.
rem
rem  WRITTEN IN POWERSHELL, NOT BY echo. Criterion text is prose and carries
rem  backticks, pipes and parentheses; echo would execute half of it.
:promptredirect
powershell -NoProfile -Command "$f='%WORK%\redirect.txt'; $p='%ARBPROMPT%'; if(-not (Test-Path -LiteralPath $f)){ exit }; $r=@{}; foreach($ln in (Get-Content -LiteralPath $f)){ if($ln -match '^([A-Z]+):\s*(.*)$'){ $r[$Matches[1]]=$Matches[2] } }; $out=@('', '============================================================', ' YOU HAVE BEEN REDIRECTED. READ THIS BEFORE YOU CHOOSE.', '============================================================', '', 'The loop did NOT halt. It sent you back, because:', ''); $out += '  ' + [string]$r['DETAIL']; $out += @('', 'YOUR NEXT INSTRUCTION MUST NAME ONE OF THESE CRITERIA:'); $out += '  ' + [string]$r['CRITERIA']; $out += @('', 'and must name an APPROACH THE RECORD DOES NOT ALREADY SHOW FAILING at it.', 'The attempts are listed further down this prompt. An instruction that names', 'another criterion, or repeats an approach recorded as failing, is refused', 'and you are redirected again - it costs an iteration and buys nothing.', '', 'THIS IS NOT A COMPLAINT TO ANSWER. It is the plan telling you that the route', 'you took did not move the criterion. Find another route TO THE SAME', 'CRITERION. Do not go looking in the last report for something else to do.', ''); $out += '(redirect ' + [string]$r['COUNT'] + ' of this run - there is no limit on these, and'; $out += @('running out of routes is not something you may assert. If every approach you', 'can see is recorded as failing, say so in WHY and name the one you judge', 'least exhausted.)', ''); [IO.File]::AppendAllText($p, ($out -join ([char]13 + [string][char]10)) + [char]13 + [string][char]10, (New-Object Text.UTF8Encoding($false)))"
goto :eof

:promptplan
powershell -NoProfile -Command "$o='%ROOT%\PHASE_OUTCOME.md'; $pl='%ROOT%\PHASE_PLAN.md'; $p='%ARBPROMPT%'; $goal=''; $states=@{}; $titles=@{}; if(Test-Path -LiteralPath $o){ $raw=[Text.Encoding]::UTF8.GetString([IO.File]::ReadAllBytes($o)); if(($raw.Length -gt 0) -and ($raw[0] -eq [char]0xFEFF)){ $raw=$raw.Substring(1) }; foreach($ln in [regex]::Split($raw, [char]13+[string][char]10+'|'+[string][char]10+'|'+[string][char]13)){ if($ln -match '^PHASE:\s*(.+?)\s*$'){ if($goal -eq ''){ $goal=$Matches[1] } }; if($ln -match '^STEP:\s*([0-9]+)\s*\|\s*([^|]+?)\s*\|\s*(.*)$'){ $states[[int]$Matches[1]]=$Matches[2]; $titles[[int]$Matches[1]]=$Matches[3] } } }; $crit=@{}; $raw=[Text.Encoding]::UTF8.GetString([IO.File]::ReadAllBytes($pl)); if(($raw.Length -gt 0) -and ($raw[0] -eq [char]0xFEFF)){ $raw=$raw.Substring(1) }; foreach($ln in [regex]::Split($raw, [char]13+[string][char]10+'|'+[string][char]10+'|'+[string][char]13)){ if($ln -match '^- \[( |x)\] ([0-9]+)\.([0-9]+) (.+?)\s*$'){ $n=[int]$Matches[2]; if(-not $crit.ContainsKey($n)){ $crit[$n]=@() }; $crit[$n] += @{ met=($Matches[1] -eq 'x'); k=[int]$Matches[3]; t=$Matches[4] } }; if($ln -match '^STEP:\s*([0-9]+)\s*\|\s*(.+?)\s*$'){ $n=[int]$Matches[1]; if(-not $titles.ContainsKey($n)){ $titles[$n]=$Matches[2] }; if(-not $states.ContainsKey($n)){ $states[$n]='not started' } } }; $open=@($states.Keys | Where-Object { $states[$_] -ne 'done' } | Sort-Object); $target=$null; if($open.Count -gt 0){ $target=$open[0] }; $out=@('============================================================', ' THE PHASE, AND THE STEP YOU ARE AUTHORING AGAINST', '============================================================', ''); if($goal -ne ''){ $out += 'PHASE GOAL: ' + $goal } else { $out += 'PHASE GOAL: not recorded - PHASE_OUTCOME.md carries no PHASE: line' }; $out += ''; if($null -eq $target){ $out += 'EVERY STEP IS done. There is nothing to author against.' } else { $out += 'THE STEP TO WORK: step ' + $target + ' - ' + $titles[$target] + '  [' + $states[$target] + ']'; $out += ''; $cs=@(); if($crit.ContainsKey($target)){ $cs=$crit[$target] }; $un=@($cs | Where-Object { -not $_.met } | Sort-Object { $_.k }); $me=@($cs | Where-Object { $_.met } | Sort-Object { $_.k }); $out += 'ITS UNMET CRITERIA. THESE ARE THE WORK:'; if($un.Count -eq 0){ $out += '  none - every criterion of this step is met' } else { foreach($c in $un){ $out += '  - [ ] ' + $target + '.' + $c.k + ' ' + $c.t } }; if($me.Count -gt 0){ $out += ''; $out += 'already met, for context only:'; foreach($c in $me){ $out += '  - [x] ' + $target + '.' + $c.k + ' ' + $c.t } }; $rest=@($open | Where-Object { $_ -ne $target }); if($rest.Count -gt 0){ $out += ''; $out += 'THE OTHER OPEN STEPS, AND THEIR UNMET CRITERIA:'; foreach($n in $rest){ $out += '  step ' + $n + ' - ' + $titles[$n] + '  [' + $states[$n] + ']'; $cs2=@(); if($crit.ContainsKey($n)){ $cs2=@($crit[$n] | Where-Object { -not $_.met } | Sort-Object { $_.k }) }; foreach($c in $cs2){ $out += '    - [ ] ' + $n + '.' + $c.k + ' ' + $c.t } } } }; $out += @('', 'ADVANCES MUST NAME ONE OF THE CRITERIA ABOVE. Everything below this block is', 'context for CHOOSING among them - what has been tried, and what the last unit', 'reported. None of it is a list of work to do.', ''); [IO.File]::AppendAllText($p, ($out -join ([char]13 + [string][char]10)) + [char]13 + [string][char]10, (New-Object Text.UTF8Encoding($false)))"
goto :eof

rem ============================================================
rem  THE PREVIOUS REPORT, LAST AND BOUNDED. 069 criteria 6.2 and 6.3.
rem
rem  THE SHARE IS ONE THIRD, AND THE ARITHMETIC IS WHY. The budget is HALF the
rem  prompt as it stands before the report is added, so a report at its cap is
rem  exactly one third of the finished prompt and the plan side outweighs it two
rem  to one. That is what "informs but does not dominate" is as a number, and it
rem  is a fraction the owner can check against the printed figures. Author's,
rem  overrulable.
rem
rem  MEASURED, NOT GUESSED, AND THIS IS WHY A BOUND EXISTS AT ALL: unit 068's
rem  own output.md is 22580 bytes against a 3426-byte prompt - 6.6 times the
rem  whole prompt, and 86.8 per cent of it if pasted in whole.
rem
rem  WHAT IS KEPT WHEN IT WILL NOT FIT, in order: SECTION 4, then any line
rem  naming a MISMATCH, then SECTION 3. Those carry evidence about a criterion -
rem  what is blocked, what disagreed with the tree, what the owner should see.
rem  Section 1 is the narrative of what a unit did, which is the part that reads
rem  as a list of work, and it is the first thing dropped. THE REPORT IS NEVER
rem  REMOVED - ruling 3 - and the whole file stays at the root to be read.
:promptreport
set "PR_BYTES=0"
set "PR_KEPT=0"
set "PR_LEFT=0"
set "PR_HOW=not run"
set "PR_TOTAL=0"
set "PR_SHARE=0.0"
set "PR_BASE=0"
set "PR_BUDGET=0"
for /f "usebackq tokens=1,* delims==" %%A in (`powershell -NoProfile -Command "$p='%ARBPROMPT%'; $r='%ROOT%\output.md'; $base=(Get-Item -LiteralPath $p).Length; $budget=[int][Math]::Floor($base / 2); 'PR_BASE=' + $base; 'PR_BUDGET=' + $budget; if(-not (Test-Path -LiteralPath $r)){ [IO.File]::AppendAllText($p, 'THERE IS NO PREVIOUS REPORT AT THIS ROOT. Author from the criteria above.' + [char]13 + [string][char]10, (New-Object Text.UTF8Encoding($false))); 'PR_BYTES=0'; 'PR_HOW=absent'; 'PR_SHARE=0.0'; 'PR_KEPT=0'; 'PR_TOTAL=' + (Get-Item -LiteralPath $p).Length; exit }; $raw=[Text.Encoding]::UTF8.GetString([IO.File]::ReadAllBytes($r)); if(($raw.Length -gt 0) -and ($raw[0] -eq [char]0xFEFF)){ $raw=$raw.Substring(1) }; $lines=[regex]::Split($raw, [char]13+[string][char]10+'|'+[string][char]10+'|'+[string][char]13); $whole=($raw.Length -le $budget); $body=$raw; $how='whole'; $left=0; if(-not $whole){ $s4=@(); $ms=@(); $s3=@(); $cur=0; foreach($ln in $lines){ if($ln -match '^## 4\.'){ $cur=4; $s4 += $ln; continue }; if($ln -match '^## 3\.'){ $cur=3; $s3 += $ln; continue }; if($ln -match '^## [0-9]'){ $cur=0; continue }; if($cur -eq 4){ $s4 += $ln } elseif($cur -eq 3){ $s3 += $ln } elseif($ln -match '(?i)mismatch'){ $ms += $ln } }; $pick=@(); $used=0; foreach($grp in @($s4, $ms, $s3)){ foreach($ln in $grp){ if(($used + $ln.Length + 2) -gt $budget){ break }; $pick += $ln; $used += $ln.Length + 2 } }; $body=($pick -join ([char]13 + [string][char]10)); $left=$raw.Length - $body.Length; $how='selected' }; $head=@('', '============================================================', ' THE PREVIOUS UNIT-S REPORT - EVIDENCE, NOT A LIST OF WORK', '============================================================', '', 'Read this as EVIDENCE ABOUT A CRITERION: what was measured, what was found, what', 'a check missed. It is NOT the next thing to work on, and a question it raises is', 'not a task. THE WORK IS THE UNMET CRITERIA AT THE TOP OF THIS PROMPT. If nothing', 'here bears on one of them, author from them anyway.', ''); if($how -eq 'selected'){ $head += @('THIS REPORT WAS TOO LONG FOR ITS SHARE OF THE PROMPT AND WAS SELECTED FROM.', 'Kept, in this order: section 4, then any line naming a mismatch, then section 3.', 'The whole file is at the repository root if you need the rest.', '') }; [IO.File]::AppendAllText($p, (($head -join ([char]13 + [string][char]10)) + [char]13 + [string][char]10 + $body + [char]13 + [string][char]10), (New-Object Text.UTF8Encoding($false))); $tot=(Get-Item -LiteralPath $p).Length; 'PR_BYTES=' + $raw.Length; 'PR_KEPT=' + $body.Length; 'PR_LEFT=' + $left; 'PR_HOW=' + $how; 'PR_TOTAL=' + $tot; 'PR_SHARE=' + [Math]::Round(100.0 * $body.Length / $tot, 1)"`) do set "%%A=%%B"
echo       prompt     : %PR_TOTAL% bytes, of which the previous report is %PR_KEPT% - %PR_SHARE% pct
if "%PR_HOW%"=="absent" echo       report     : none at this root
if "%PR_HOW%"=="whole"  echo       report     : %PR_BYTES% bytes, included whole, budget was %PR_BUDGET%
if "%PR_HOW%"=="selected" echo       report     : %PR_BYTES% bytes, OVER the %PR_BUDGET% budget - kept %PR_KEPT%, left out %PR_LEFT%
goto :eof

rem ============================================================
rem  IS output.md THIS UNIT'S REPORT? 067 task 2.

 Sets OWNREPORT yes or no,
rem  OWNWHY saying which, OWNSTAMP and OWNWRITE - the two times compared -
rem  and OWNFOUND, the UNIT: line of the file it found when the answer is
rem  no, so the halt names what was there instead of a report.
rem
rem  READ-ONLY. It moves nothing and deletes nothing: task 3 moves a report
rem  that was judged, and a report this refuses was never this unit's to
rem  move. The UNIT: line is stripped of the characters cmd acts on before
rem  it enters a variable, as every other model-written string here is.
:ownreport
set "OWNREPORT=no"
set "OWNWHY=the check did not run"
set "OWNFOUND="
set "OWNSTAMP="
set "OWNWRITE="
rem  IS THERE A FILE AT ALL? Set here rather than inferred from OWNFOUND,
rem  because "no output.md" and "an output.md with no UNIT: line" are two
rem  different things to say and the first run of this fixture said the
rem  second when it meant the first.
set "OWNTHERE=no"
if exist "%ROOT%\output.md" set "OWNTHERE=yes"
for /f "usebackq tokens=1,* delims==" %%A in (`powershell -NoProfile -Command "$s='%WORK%\launch.stamp'; $o='%ROOT%\output.md'; if(-not (Test-Path -LiteralPath $o)){ 'OWNREPORT=no'; 'OWNWHY=there is no output.md at the root at all'; exit }; if(-not (Test-Path -LiteralPath $s)){ 'OWNREPORT=no'; 'OWNWHY=no launch stamp was written, so no report can be shown to be this unit-s'; exit }; $st=(Get-Item -LiteralPath $s).LastWriteTimeUtc; $ot=(Get-Item -LiteralPath $o).LastWriteTimeUtc; 'OWNSTAMP=' + $st.ToString('HH:mm:ss.fff'); 'OWNWRITE=' + $ot.ToString('HH:mm:ss.fff'); if($ot -gt $st){ 'OWNREPORT=yes'; 'OWNWHY=output.md was written after this unit was launched'; exit }; 'OWNREPORT=no'; 'OWNWHY=output.md at the root was written BEFORE this unit was launched - it is not this unit-s report'; $u=''; foreach($ln in (Get-Content -LiteralPath $o)){ if($ln -match '^UNIT:\s*(.+?)\s*$'){ $u=$Matches[1]; break } }; if($u){ 'OWNFOUND=' + (((($u -replace '[&|<>^%%]','') -replace [char]96,'') -replace [char]34,'') -replace '\s+',' ').Trim() }"`) do set "%%A=%%B"
goto :eof

rem ============================================================
rem  DID THE WATCHDOG KILL THIS RUN? ONE READER, because :ambiguous1 and
rem  the launched check at 4pre both have to tell run-unit.bat's exit 1 -
rem  the lock was held, nothing ran - from run-unit-watched.bat's exit 1 -
rem  it ran and was killed. Two copies of this read would drift, and the
rem  whole of 4pre turns on which of the two it was. watched.log is
rem  truncated by -RedirectStandardOutput on every run, so the count is
rem  this run's and not the night's.
:killcount
set "KILLED=0"
for /f "usebackq delims=" %%K in (`powershell -NoProfile -Command "$f='%WORK%\watched.log'; if(Test-Path -LiteralPath $f){ @(Select-String -Path $f -Pattern 'Terminating pid').Count } else { 0 }"`) do set "KILLED=%%K"
goto :eof

rem ============================================================
rem  A JUDGED REPORT IS KEPT PER UNIT, AND THE ROOT COPY STAYS.
rem  067 task 3 built this and REMOVED the root copy. 068 task 1 stops
rem  removing it, on the owner's ruling of 2026-09-19: KEEP THE COPY, DROP
rem  THE REMOVAL.
rem
rem  WHY THE REMOVAL WENT, AND IT IS NOT A PREFERENCE. The panel ends a
rem  cycle at unitReported(slot), which returns o.mtime > i.mtime and FALSE
rem  where no OUTPUT.md exists - CPS-DEC-044. With the report moved away, a
rem  healthy loop that had just filed its report looked identical to a
rem  session that died without writing one: the delivered lamp dark, the
rem  review button grey, and the forward-only latch stuck at CODE for ever.
rem  CPS-DEC-044's own comment says a card in that state is stuck and should
rem  look it. A healthy card looking stuck is the failure this whole
rem  repository exists to prevent.
rem
rem  WHAT THE REMOVAL WAS BUYING: the next unit starting with nothing to
rem  inherit. That is tidiness, not safety. 067 task 2's launch stamp is what
rem  protects the record - a report older than the stamp is refused, named,
rem  and never judged, whether or not it was removed - so the removal was the
rem  belt over a brace that already holds, and the brace is now the only
rem  thing standing between a leftover report and the record. IT IS NOT
rem  WEAKENED HERE and nothing 067 built is touched.
rem
rem  THE ROOT FILE IS NOW ALWAYS THE NEWEST REPORT, which is exactly what
rem  unitReported measures. The per-unit copy is the archive; the root copy
rem  is what the panel reads.
rem
rem  TEACHING THE PANEL WHERE REPORTS LIVE IS ITS OWN UNIT, ruled 2026-09-19.
rem  It amends CPS-DEC-044 and is not bolted on here.
rem
rem  IT IS THE ONLY RECORD OF WHAT A UNIT SAID, and four reports in the week
rem  to 2026-09-14 were the evidence for a ruling. That is why the copy is
rem  kept at all.
rem
rem  COPY AND HASH. Never Move-Item, and now never a remove either. The
rem  destination is compared to the source byte for byte before the keep is
rem  called a keep; if the hashes differ the console says so and the keep is
rem  reported as not done.
rem
rem  THE DESTINATION IS NEVER OVERWRITTEN. .run-unit\reports\unit-N-output.md
rem  where N is the iteration - and THE ITERATION NUMBER REPEATS AFTER A
rem  RESTART, which unit 066 measured and reported: a second loop over the
rem  same root counts from 1 again. So an occupied name takes -2, -3 and on
rem  to -99, and nothing that is already there is written over. The suffix
rem  is a form and it is the arbiter's - author's, overrulable. Fixing
rem  the repeating number is step 2 of the plan, not this unit's.
rem
rem  A FAILURE HERE DOES NOT STOP THE LOOP, and deletes nothing - there is
rem  now nothing here that could. It is reported and the night goes on.
rem
rem  WHAT HAPPENS ON THE SECOND UNIT, now that the root file is never
rem  cleared between units. Within one loop process N is the iteration, so
rem  unit 2 writes unit-2-output.md and cannot collide with unit 1. Across a
rem  RESTART the iteration counts from 1 again - unit 066 measured it - so
rem  the second loop finds unit-1-output.md occupied and takes
rem  unit-1-output-2.md. Nothing is overwritten either way.
rem
rem  AND THE ROOT FILE IS NOW A PREVIOUS UNIT'S REPORT ON EVERY ITERATION,
rem  not only after a leak. A unit that runs and writes its own report
rem  overwrites it with a newer one and is judged. A unit that runs and
rem  writes NOTHING leaves the previous report there, and 067's stamp
rem  refuses it at STOP 11 naming whose it is. That path is exercised by the
rem  leftover fixture and is now the ordinary case rather than the rare one.
rem
rem  IT RUNS ONLY WHERE :record RAN. The two stop 11 arms jump straight to
rem  :stopped, so a report that was never judged is never copied either -
rem  the archive holds only reports that were actually judged.
:keepreport
set "KEPT=no"
set "KEPTWHY=the keep did not run"
set "KEPTAT="
set "KEPTHASH="
for /f "usebackq tokens=1,* delims==" %%A in (`powershell -NoProfile -Command "$o='%ROOT%\output.md'; $d='%WORK%\reports'; $n='%ITER%'; if(-not (Test-Path -LiteralPath $o)){ 'KEPT=no'; 'KEPTWHY=there was no output.md at the root to keep'; exit }; try{ if(-not (Test-Path -LiteralPath $d)){ New-Item -ItemType Directory -Path $d -Force | Out-Null }; $base='unit-' + $n + '-output'; $t=$null; for($i=1; $i -le 99; $i++){ $c=Join-Path $d ($base + $(if($i -eq 1){ '' } else { '-' + $i }) + '.md'); if(-not (Test-Path -LiteralPath $c)){ $t=$c; break } }; if($t -eq $null){ 'KEPT=no'; 'KEPTWHY=99 reports are already kept under that unit number and none was overwritten'; exit }; $h=(Get-FileHash -LiteralPath $o -Algorithm SHA256).Hash; Copy-Item -LiteralPath $o -Destination $t -Force; $g=(Get-FileHash -LiteralPath $t -Algorithm SHA256).Hash; if($g -ne $h){ 'KEPT=no'; 'KEPTWHY=the copy did not match the report byte for byte, so the root copy was left where it is'; exit }; 'KEPT=yes'; 'KEPTWHY=the report was kept and the root copy left where the panel can see it'; 'KEPTAT=' + $t; 'KEPTHASH=' + $h.Substring(0,12) } catch { 'KEPT=no'; 'KEPTWHY=the keep failed and the report was left at the root - ' + (($_.Exception.Message -replace '[&|<>^%%]','') -replace '\s+',' ') }"`) do set "%%A=%%B"
if "%KEPT%"=="yes" echo       report kept: %KEPTAT%  sha256 %KEPTHASH%... - the root output.md stays, so the panel keeps its cycle end
if "%KEPT%"=="no" echo       REPORT NOT KEPT: %KEPTWHY%
goto :eof

:afterrecord

rem --- 5keep. the judged report, moved out of the way ----------
call :keepreport

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

rem --- 5b. did the unit claim to reverse an earlier ruling? ------
rem  THE OWNER'S RULING OF 2026-09-14: no self-ruling may overrule an earlier
rem  arbiter ruling - only the owner does that. 065 task 4. The state judge
rem  was asked one question about it, answered in one word: does the report
rem  CLAIM a ruling that reverses an earlier one? Intent is not judged; a
rem  claimed reversal is. A yes halts here, after the record is written so
rem  the entry survives, with the ruling quoted. unread is never a yes.
if /i "%J_REVERSES%"=="yes" goto :reversal

rem --- 5c. did the criterion follow from the plan? 069 criterion 6.5 ------
rem  The owner's ruling of 2026-09-19: the plan leads and the report follows.
rem  A yes halts AFTER the record is written, so the entry survives - the same
rem  order the reversal halt uses. unread is never report.
if /i "%J_FOLLOWS%"=="report" goto :planonly

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
rem  PROGRESS COUNTED IN CRITERIA. 064 tasks 2 to 4, the owner's ruling
rem  of 2026-09-14. Every routine below reads a FILE - the instruction,
rem  the plan, the outcome - and none holds state across iterations.
rem ============================================================

rem  ADVANCES, read from WORK_INSTRUCTIONS.md itself rather than from the
rem  cmd variable, so an em dash or a quote cannot bend the parse. Sets
rem  ADV_KIND to criterion, blocker or bad; ADV_STEP and ADV_CRIT for a
rem  criterion; ADV_WHAT, what a blocker-clear unblocks, for display.
rem  A blocker form that names neither a unit number nor a criterion is
rem  bad: unbounded, it is the hole every future unit climbs through.
:advparse
set "ADV_KIND=bad"
rem  068: cleared every iteration, so a match found once cannot be carried
rem  into the next unit by a variable nobody reset.
set "AP_MATCH="
set "AP_WHO="
set "AP_NAMED="
set "AP_WORDS="
set "AP_THEIRS="
set "WY_SEEN="
set "WY_CITES="
set "WY_IDS="
set "WY_STEPS="
set "WY_HITS="
set "WY_WORDS="
set "WY_BEST="
set "RQ_ON="
set "RQ_WANT="
set "RQ_RULE="
set "RQ_DETAIL="
set "RQ_COUNT="
set "RQ_MINE="
set "RQ_MET="
set "ADV_STEP="
set "ADV_CRIT="
set "ADV_WHAT="
for /f "usebackq tokens=1,* delims==" %%A in (`powershell -NoProfile -Command "$f='%ROOT%\WORK_INSTRUCTIONS.md'; if(-not (Test-Path -LiteralPath $f)){ exit }; $v=''; foreach($ln in (Get-Content -LiteralPath $f)){ if($ln -match '^ADVANCES:\s*(.+?)\s*$'){ $v=$Matches[1] } }; $d='[-' + [char]0x2014 + [char]0x2013 + ']+'; if($v -match '(?i)^step\s+([0-9]+)\s*,?\s*criterion\s+([0-9]+)(\D|$)'){ 'ADV_KIND=criterion'; 'ADV_STEP=' + [int]$Matches[1]; 'ADV_CRIT=' + [int]$Matches[2]; exit }; if($v -match ('(?i)^none\s*' + $d + '\s*(this unit\s+)?clears\s+(a|the)\s+blocker\b(.*)$')){ $w=$Matches[3]; if(($w -match '(?i)\bunit\s+[0-9]+') -or ($w -match '(?i)\bcriterion\s+[0-9]+') -or ($w -match '\b[0-9]+\.[0-9]+\b')){ 'ADV_KIND=blocker'; 'ADV_WHAT=' + ((($w -replace '[&|<>^%%]','') -replace [char]34,'') -replace '\s+',' ').Trim([char[]]' :.,-') } }"`) do set "%%A=%%B"
goto :eof

rem  A step's state, from PHASE_OUTCOME.md's header - the file :position
rem  and stop 1 read. none where there is no outcome file or no such step.
:stepstate
set "SS_STEP=%~1"
set "SS_STATE=none"
for /f "usebackq delims=" %%S in (`powershell -NoProfile -Command "$o='%ROOT%\PHASE_OUTCOME.md'; if(-not (Test-Path -LiteralPath $o)){ 'none'; exit }; $n='%~1'; $s='none'; foreach($ln in (Get-Content -LiteralPath $o)){ if($ln -cmatch ('^STEP: *' + [regex]::Escape($n) + ' *\| *([a-z ]+?) *\|')){ $s=$Matches[1]; break } }; $s"`) do set "SS_STATE=%%S"
goto :eof

rem  The count, from criteria-count.bat - a script, never a judgment.
rem  CC_CRIT reads unreadable when the plan could not be read.
:critcount
set "CC_MET=?"
set "CC_TOTAL=?"
set "CC_CRIT=unreadable"
for /f "usebackq tokens=1,* delims==" %%A in (`call "%HERE%criteria-count.bat" "%ROOT%\PHASE_PLAN.md" "%~1" "%~2" 2^>nul`) do set "CC_%%A=%%B"
goto :eof

rem  The count after the unit reports. FLIPPED is 1 only where the named
rem  criterion read unmet before the unit and met after it.
:critafter
set "FLIPPED=0"
set "CA_MET="
set "CA_TOTAL="
set "CA_CRIT="
if not "%ADV_KIND%"=="criterion" goto :eof
call :critcount "%ADV_STEP%" "%ADV_CRIT%"
set "CA_MET=%CC_MET%"
set "CA_TOTAL=%CC_TOTAL%"
set "CA_CRIT=%CC_CRIT%"
if "%CB_CRIT%"=="unmet" if "%CA_CRIT%"=="met" set "FLIPPED=1"
echo       count      : step %ADV_STEP% criterion %ADV_CRIT% was %CB_CRIT% and is %CA_CRIT% - step %ADV_STEP% had %CB_MET% of %CB_TOTAL% met and has %CA_MET% of %CA_TOTAL%
goto :eof

rem  THE VERDICT ON WHETHER THE UNIT ADVANCED. The script decides the flip.
rem  The state judge rules only on whether a flip the script saw was
rem  honestly met, and a judge that could not be read is not a yes - an
rem  advance is recorded only on J_HONEST yes. A run that never ran moved
rem  nothing. The blocker form is recorded as blocker, never as yes.
:advverdict
set "ADVANCED_OUT=not recorded"
set "ADVNOTE=the decision block's ADVANCES was not read"
if "%ADV_KIND%"=="blocker" set "ADVANCED_OUT=blocker"
if "%ADV_KIND%"=="blocker" set "ADVNOTE=cleared a blocker: %ADV_WHAT%"
if not "%ADV_KIND%"=="criterion" goto :advverdictsay
set "ADVANCED_OUT=no"
set "ADVNOTE=step %ADV_STEP% criterion %ADV_CRIT% was %CB_CRIT% and is %CA_CRIT%"
if "%RUNFATE%"=="never ran" goto :advverdictsay
if not "%FLIPPED%"=="1" goto :advverdictsay
if /i "%J_HONEST%"=="yes" goto :advverdictyes
set "ADVNOTE=step %ADV_STEP% criterion %ADV_CRIT% flipped, and the state judge did not find it honestly met: %J_HONEST%"
goto :advverdictsay
:advverdictyes
set "ADVANCED_OUT=yes"
set "ADVNOTE=step %ADV_STEP% criterion %ADV_CRIT% flipped from unmet to met, honestly by the state judge"
:advverdictsay
echo       advanced   : %ADVANCED_OUT% - %ADVNOTE%
goto :eof

rem  THE CRITERION THIS UNIT WAS RUN AGAINST, FOR THE ATTEMPT RECORD. 066
rem  task 3. AT_CRIT is N.k, or empty where the unit was run against none:
rem    ADVANCES: step N criterion k        ->  N.k
rem    a blocker-clear naming a criterion  ->  the criterion it names as
rem        unblocked - N.k as written, step N criterion k, or criterion k
rem        alone taken with the decision block's STEP. Author's, overrulable.
rem    a blocker-clear naming only a unit  ->  none, and the console says so
rem  Read from ADV_WHAT and A_STEP, which this launcher parsed out of the
rem  instruction before the unit ran. Nothing the unit wrote enters it.
:attemptid
set "AT_CRIT="
set "AT_NOTE=no attempt recorded - ADVANCES was not read"
if not "%ADV_KIND%"=="criterion" goto :attemptblocker
set "AT_CRIT=%ADV_STEP%.%ADV_CRIT%"
set "AT_NOTE=recorded against criterion %ADV_STEP%.%ADV_CRIT%"
goto :attemptsay
:attemptblocker
if not "%ADV_KIND%"=="blocker" goto :attemptsay
for /f "usebackq delims=" %%C in (`powershell -NoProfile -Command "$w=[string]$env:ADV_WHAT; $s=[string]$env:A_STEP; if($w -match '\b([0-9]+)\.([0-9]+)\b'){ ([string][int]$Matches[1]) + '.' + ([string][int]$Matches[2]); exit }; if($w -match '(?i)\bstep\s+([0-9]+)\s*,?\s*criterion\s+([0-9]+)'){ ([string][int]$Matches[1]) + '.' + ([string][int]$Matches[2]); exit }; if(($w -match '(?i)\bcriterion\s+([0-9]+)') -and ($s -match '^[0-9]+$')){ ([string][int]$s) + '.' + ([string][int]$Matches[1]) }"`) do set "AT_CRIT=%%C"
if defined AT_CRIT set "AT_NOTE=recorded against criterion %AT_CRIT%, the one the blocker-clear names as unblocked"
if not defined AT_CRIT set "AT_NOTE=no attempt recorded - the blocker-clear names no criterion, only: %ADV_WHAT%"
:attemptsay
echo       attempt    : %AT_NOTE%
goto :eof

rem  The last two entries in PHASE_OUTCOME.md, in append order. LA_KIND is
rem  no when both were executed and neither advanced, blocker when both
rem  cleared a blocker, none otherwise - including where either entry was
rem  written before 064 and carries no ADVANCED line at all.
:lastadvances
set "LA_KIND=none"
set "LA_UNITS="
set "LA_STEP="
rem  070: AND THE CRITERION EACH ENTRY WAS RUN AGAINST, from the ATTEMPT line
rem  inside it - 066 put it there. Until now this routine read the STEP and
rem  never the criterion, so it could not tell a loop grinding at ONE
rem  criterion from one wandering between TWO. Criterion 3.3 turns on exactly
rem  that distinction. An entry with no ATTEMPT line - a blocker-clear naming
rem  no criterion - reports none, and none never equals none.
set "LA_CRIT1=none"
set "LA_CRIT2=none"
set "LA_SAME=no"
for /f "usebackq tokens=1,* delims==" %%A in (`powershell -NoProfile -Command "$o='%ROOT%\PHASE_OUTCOME.md'; if(-not (Test-Path -LiteralPath $o)){ exit }; $e=@(); $cur=$null; foreach($ln in (Get-Content -LiteralPath $o)){ if($ln -match '^## UNIT (\S+) - STEP (\S+)'){ if($cur){ $e+=$cur }; $cur=@{ u=$Matches[1]; s=$Matches[2]; f=''; a=''; c='' } } elseif($cur){ if($ln -match '^FATE:\s*(.+?)\s*$'){ $cur.f=$Matches[1] }; if($ln -match '^ADVANCED:\s*(.+?)\s*$'){ $cur.a=$Matches[1] }; if($ln -match '^ATTEMPT: *([0-9]+\.[0-9]+) *\|'){ $cur.c=$Matches[1] } } }; if($cur){ $e+=$cur }; if($e.Count -lt 2){ exit }; $p=$e[$e.Count-2]; $q=$e[$e.Count-1]; $steps=$p.s; if($q.s -ne $p.s){ $steps=$p.s + ' and ' + $q.s }; if(($p.a -eq 'no') -and ($q.a -eq 'no') -and ($p.f -eq 'executed') -and ($q.f -eq 'executed')){ 'LA_KIND=no'; 'LA_UNITS=' + $p.u + ' and ' + $q.u; 'LA_STEP=' + $steps; 'LA_CRIT1=' + $(if($p.c -ne ''){ $p.c } else { 'none' }); 'LA_CRIT2=' + $(if($q.c -ne ''){ $q.c } else { 'none' }); 'LA_SAME=' + $(if(($p.c -ne '') -and ($p.c -eq $q.c)){ 'yes' } else { 'no' }) } elseif(($p.a -eq 'blocker') -and ($q.a -eq 'blocker')){ 'LA_KIND=blocker'; 'LA_UNITS=' + $p.u + ' and ' + $q.u; 'LA_STEP=' + $steps }"`) do set "%%A=%%B"
goto :eof

rem  IS THE OWNER'S VERDICT ALL THAT IS LEFT? Read from PHASE_PLAN.md and
rem  PHASE_OUTCOME.md's header, never from a variable. Sets OW_HALT to 1 or 0;
rem  when 1, OW_IDS lists the owner's criteria by id, OW_SHEET is written,
rem  exists or none, and OW_SHEETPATH is the plan's REVIEW_SHEET path.
rem
rem  THE REVIEW SHEET, AND ONCE. 065 task 3. Nothing in any plan could ask for
rem  a sheet, so the ask is one line of the plan's own KEY: VALUE shape,
rem      REVIEW_SHEET: <path, relative to the root>
rem  and a plan without it asks for none. ONCE IS DECIDED FROM THE DISK: a sheet
rem  already at that path is not written again, however many passes the phase
rem  makes - for the reason stop 10's counter left memory in 064.
rem
rem  THE CRITERIA ARE PRINTED BY POWERSHELL, NOT ECHOED BY cmd, because they
rem  are plan prose and & | < > ^ are live on a bare echo line.
:ownerwait
set "OW_HALT=0"
set "OW_IDS="
set "OW_SHEET=none"
set "OW_SHEETPATH="
set "OWENV=%WORK%\owner-wait.txt"
if exist "%OWENV%" del /q "%OWENV%"
powershell -NoProfile -Command "$p='%ROOT%\PHASE_PLAN.md'; $o='%ROOT%\PHASE_OUTCOME.md'; $envf='%OWENV%'; if(-not (Test-Path -LiteralPath $p)){ exit }; $plan=@(Get-Content -LiteralPath $p -Encoding UTF8); $state=@{}; if(Test-Path -LiteralPath $o){ foreach($ln in (Get-Content -LiteralPath $o -Encoding UTF8)){ if($ln -cmatch '^STEP: *([0-9]+) *\| *([a-z ]+?) *\|'){ if(-not $state.ContainsKey([int]$Matches[1])){ $state[[int]$Matches[1]]=$Matches[2] } } } }; $steps=@(); $sheet=''; foreach($raw in $plan){ $ln=([string]$raw).TrimStart([char]0xFEFF); if($ln -cmatch '^STEP: *([0-9]+) *\|'){ $steps+=[int]$Matches[1] }; if($ln -cmatch '^REVIEW_SHEET:\s*(.+?)\s*$'){ $sheet=$Matches[1] } }; $work=0; $owners=@(); $nocrit=0; $open=0; foreach($s in $steps){ $st='none'; if($state.ContainsKey($s)){ $st=$state[$s] }; if($st -eq 'done'){ continue }; $open++; $n=0; foreach($raw in $plan){ $ln=([string]$raw).TrimStart([char]0xFEFF); if($ln -match '^\s*-\s\[( |x|X)\]\s+([0-9]+)\.([0-9]+)\s+(.*)$'){ $mk=$Matches[1]; $sn=[int]$Matches[2]; $cid=$Matches[2] + '.' + $Matches[3]; $txt=$Matches[4]; if($sn -ne $s){ continue }; $n++; if($mk -ne ' '){ continue }; if($txt -match '(?i)\*owner.s verdict\*\s*$'){ $owners+=($cid + ' ' + ($txt -replace '(?i)\s*\*owner.s verdict\*\s*$','')) } else { $work++ } } }; if($n -eq 0){ $nocrit++ } }; $halt=($open -gt 0) -and ($work -eq 0) -and ($nocrit -eq 0) -and ($owners.Count -gt 0); $out=@(); if(-not $halt){ $out+='HALT=0'; Set-Content -LiteralPath $envf -Value $out -Encoding ascii; exit }; $out+='HALT=1'; $out+=('IDS=' + (($owners | ForEach-Object { $_.Split(' ')[0] }) -join ', ')); '      waiting on the owner:'; foreach($w in $owners){ '        ' + $w }; if($sheet -eq ''){ $out+='SHEET=none' } else { $sp=Join-Path '%ROOT%' $sheet; $out+=('SHEETPATH=' + $sheet); if(Test-Path -LiteralPath $sp){ $out+='SHEET=exists' } else { $out+='SHEET=absent' } }; Set-Content -LiteralPath $envf -Value $out -Encoding ascii"
if not exist "%OWENV%" goto :eof
for /f "usebackq tokens=1,* delims==" %%A in ("%OWENV%") do set "OW_%%A=%%B"
goto :eof

:ownerhalt
echo.
echo   STOP 1: THE PHASE IS WAITING ON THE OWNER'S VERDICT.
echo   Every unmet criterion of every step not done is marked *owner's verdict*, and
echo   nothing in the loop can give one. No arbiter was called, and no unit was run.
if "%OW_SHEET%"=="exists" echo   Review sheet already on disk, not written again: %OW_SHEETPATH%
if "%OW_SHEET%"=="none" echo   The plan asks for no review sheet, so none was written.
set "STOPWHY=stop 1: the phase is waiting on the owner's verdict - criteria %OW_IDS%"
set "RC=0"
goto :stopped

rem ============================================================
rem  069 criterion 6.5. The judge read the plan, the report and the
rem  instruction's own WHY, and said the criterion was chosen from the report
rem  rather than from the plan. Its sentence is quoted rather than summarised,
rem  because the owner has to be able to disagree with the judge and not only
rem  with the launcher.
:planonly
echo.
echo   HALTED: THE CRITERION WAS CHOSEN FROM THE LAST REPORT, NOT FROM THE PLAN.
echo   The state judge read the step, this unit-s report and the instruction-s own
echo   WHY, and answered FOLLOWS: report. It said:
powershell -NoProfile -Command "'     ' + $env:J_FOLLOWED"
echo   The report is evidence about a criterion, never the next thing to work on.
echo   The owner-s ruling of 2026-09-19. The unit-s entry is recorded above.
set "STOPWHY=halted: the criterion was chosen from the last report rather than the plan - %J_FOLLOWED%"
goto :stopped

rem ============================================================
:reversal
echo.
echo   HALTED: A UNIT REPORT CLAIMS A RULING THAT REVERSES AN EARLIER ARBITER RULING.
echo   The state judge read this unit's report and answered REVERSES: yes. It quoted:
powershell -NoProfile -Command "'     ' + $env:J_REVERSED"
echo   No self-ruling may overrule an earlier arbiter ruling; only the owner does
echo   that. The owner's ruling of 2026-09-14. The unit's entry is recorded above.
set "STOPWHY=halted: a unit report claims a ruling that reverses an earlier arbiter ruling - %J_REVERSED%"
goto :stopped

:ownercrit
echo.
echo   REFUSED: ADVANCES NAMES AN OWNER'S-VERDICT CRITERION.
echo   Step %ADV_STEP% criterion %ADV_CRIT% is marked *owner's verdict* in PHASE_PLAN.md. A unit
echo   cannot flip a criterion only the owner can judge, and nothing in the loop turns
echo   one to - [x]. The owner's ruling of 2026-09-14. Nothing was launched.
set "STOPWHY=refused: ADVANCES names step %ADV_STEP% criterion %ADV_CRIT%, which is the owner's verdict"
goto :stopped

rem ============================================================
rem  068 tasks 4 and 5. NAMED, NOT NUMBERED: it is a refusal before the unit
rem  runs, the shape :stepclosed and :badadvances already use, and not one of
rem  the stop conditions. The message carries what the next author needs in
rem  order to avoid it WITHOUT READING THE FILE - the criterion, the earlier
rem  attempt, what that attempt recorded, and the words the two share.
rem ============================================================
rem  069 task 4. Named, not numbered, the shape :stepclosed and :badadvances
rem  already use. The message says WHAT IT DID CITE - the words the WHY
rem  carries, and the plan line that came closest with how many words it
rem  shared - so the next author sees the difference rather than guessing.
:whynoplan
echo.
echo   REFUSED: THIS INSTRUCTION-S WHY CITES NO LINE OF THE PLAN.
echo   NOTHING WAS LAUNCHED.
echo.
echo   criterion ids cited : %WY_IDS%
echo   steps cited         : %WY_STEPS%
echo   its own words       : %WY_WORDS%
echo   closest plan line   : %WY_BEST%
echo   which shares        : %WY_HITS% of those words - three are needed
echo.
echo   A WHY names a criterion id, a step, or enough of one plan line to show
echo   the reasoning started there. Reasoning only from the last report is what
echo   this refuses: the report is evidence about a criterion, never the next
echo   thing to work on. Author the WHY from the criterion you are advancing.
set "STOPWHY=refused: the instruction-s WHY cites no line of the plan - closest shared %WY_HITS% words, three are needed"
goto :stopped

rem ============================================================
rem  070 task 2. The redirected instruction named a criterion it was not sent
rem  back to. Redirected again, with the same requirement restated - never
rem  halted, and never let through. The iteration is spent, which is the cost
rem  of ignoring the requirement and is said on the console.
:wrongcriterion
echo.
echo   REDIRECTED AGAIN: THE INSTRUCTION NAMED A CRITERION IT WAS NOT SENT TO.
echo     it named       : %RQ_MINE%
echo     it was sent to : %RQ_WANT%
echo     %RQ_DETAIL%
echo   NOTHING WAS LAUNCHED AND NOTHING WAS SPENT ON A UNIT. The iteration is
echo   spent on the refusal, and the loop carries on.
set "RD_RULE=%RQ_RULE%"
set "RD_CRITERIA=%RQ_WANT%"
set "RD_DETAIL=%RQ_DETAIL%; the instruction then named %RQ_MINE%, which it was not sent to"
goto :redirectnext

rem ============================================================
rem  070 task 3. 068-S REFUSAL REDIRECTS RATHER THAN HALTING, and the reason
rem  is in its own message: NOTHING WAS LAUNCHED AND NOTHING WAS SPENT. That
rem  is exactly why the loop should carry on. The arbiter already receives
rem  every attempt in the phase - 068 built that - so it needs nothing more
rem  than to be told which criterion it is now pinned to, which the redirect
rem  block gives it.
rem
rem  THE MATCHER IS NOT WEAKENED to make this rarer. If it is wrong it is
rem  wrong in its own right, and 068-s report names what it misses and what it
rem  catches wrongly.
:approachrepeat
echo.
echo   REFUSED: THIS APPROACH IS ALREADY RECORDED AS FAILED AT CRITERION %ADV_STEP%.%ADV_CRIT%.
echo   NOTHING WAS LAUNCHED - AND NOTHING WAS SPENT, WHICH IS WHY THIS CARRIES ON.
echo.
echo   the earlier attempt : %AP_WHO%
if "%AP_NAMED%"=="no" echo   THAT ATTEMPT CARRIES NO LAUNCH IDENTITY. It was written before unit
if "%AP_NAMED%"=="no" echo   068, and the unit number repeats after a restart, so it may be any of
if "%AP_NAMED%"=="no" echo   the units that have carried that number. It is refused anyway: THAT
if "%AP_NAMED%"=="no" echo   the approach failed at this criterion is recorded and certain, and only
if "%AP_NAMED%"=="no" echo   WHICH unit made it is not.
echo   it recorded         : %AP_THEIRS%
echo   shared words        : %AP_WORDS%
echo.
echo   An approach recorded as failed against this criterion is not attempted
echo   again. The loop is REDIRECTING rather than halting: author a DIFFERENT
echo   route to the SAME criterion. The attempt record for every criterion is
echo   handed to the arbiter in its prompt.
set "RD_RULE=approach already recorded as failed"
set "RD_CRITERIA=%ADV_STEP%.%ADV_CRIT%"
set "RD_DETAIL=the approach named was already recorded as failed at criterion %ADV_STEP%.%ADV_CRIT% by %AP_WHO%"
goto :redirectnext

:badadvances
echo.
echo   REFUSED: ADVANCES names no criterion and is not the blocker form.
echo   It must read  step N criterion k  - a line of PHASE_PLAN.md in the form
echo   - [ ] N.k  - or  none - clears a blocker:  naming a unit number or a
echo   criterion it unblocks. The owner's ruling of 2026-09-14. Nothing was launched.
set "STOPWHY=refused: ADVANCES named no step and criterion, and no unit or criterion it unblocks"
goto :stopped

:stepclosed
echo.
echo   REFUSED: STEP %SS_STEP% IS DONE, AND A DONE STEP IS CLOSED.
echo   The instruction was authored into step %SS_STEP%, whose state in PHASE_OUTCOME.md
echo   is %SS_STATE%. Only the owner reopens a step, by a ruling in the plan - no arbiter
echo   ruling can. Nothing was launched; the unit's prompt was not spent.
set "STOPWHY=refused: step %SS_STEP% is done and closed - only the owner reopens a step"
goto :stopped

rem ============================================================
rem  070 task 2. THE TWO NO-ADVANCE REDIRECTS. Both author again; neither
rem  halts; neither is capped - ruling 2 rejects an attempt ceiling and a cap
rem  is that ceiling under another name. The bound is --budget, --minutes,
rem  --max-iterations and step 4 when it exists.
rem
rem  SAME CRITERION: the next instruction must name THAT criterion again with
rem  an approach the record does not show failing at it. That is ruling 2
rem  said literally - the next attempt at the same criterion by a different
rem  approach.
rem
rem  DIFFERENT CRITERIA: the next instruction must name ONE OF THE TWO. This is
rem  the arbiter-s answer to criterion 3.3 - author-s, overrulable - and the
rem  reasoning is in the report. In short: halting is refused by ruling 2, and
rem  letting it pick a THIRD criterion would let an arbiter that moved nothing
rem  at two criteria wander to a third, which is a different failure from a
rem  grind and the one ruling 3 warns about. Sending it back to one of the two
rem  it just failed at is ruling 2 applied to both of them at once. The
rem  constraint comes from the RECORD - which criteria failed - and never from
rem  what the last report complained about.

rem ============================================================
rem  071 task 3. THE TEST. Does the record SHOW the routes are closed?
rem
rem  A CLOSED ROUTE IS AN ATTEMPT WITH A VERDICT OF no AND A FATE OF
rem  executed. Both halves, and the second is the one a first cut would
rem  drop: CPS-DEC-076 already ruled that a fate of never ran means the
rem  approach WAS NEVER TRIED, so it cannot block a later attempt - and it
rem  cannot count as a route that has been closed either. The same pair,
rem  for the same reason, in both directions.
rem
rem  DISTINCTNESS IS 068'S MATCHER RULE, NOT A SECOND MECHANISM. Two
rem  approaches are the same when the shorter has four or more distinctive
rem  words - four letters or longer, minus the stop list, which is copied
rem  here word for word - and every one of them appears in the longer.
rem  Criterion 4.3's 'approaches that repeat one another' is exactly that
rem  question, and the instruction says to reuse it. Repeats fold into the
rem  route they repeat and are counted in EX_DUPES so the console can say
rem  so.
rem
rem  THREE, AND IT IS RULING 3'S NUMBER, NOT THIS UNIT'S. The arbiter's,
rem  2026-09-14, overrulable: too low a bar quits while a route exists, too
rem  high grinds on the impossible, and only the second side has a brake.
rem
rem  ATTEMPTS WRITTEN BEFORE THIS UNIT CARRY NO REASON AND ARE COUNTED.
rem  The instruction is explicit - prefer counting them to discarding them -
rem  and the console names each one as carrying no recorded reason, so the
rem  owner reading an ending can see which routes were demonstrated and
rem  which were merely recorded. Discarding them would make the record the
rem  loop kept since unit 066 worth nothing.
:exhausttest
set "EX_TOTAL=0"
set "EX_CLOSED=0"
set "EX_DUPES=0"
set "EX_NOREASON=0"
set "EX_OK=no"
set "EX_WHY="
if exist "%WORK%\exhaust.txt" del /q "%WORK%\exhaust.txt" 2>nul
for /f "usebackq tokens=1,* delims==" %%A in (`powershell -NoProfile -Command "$o='%ROOT%\PHASE_OUTCOME.md'; $lf='%WORK%\exhaust.txt'; $want='%~1'; $stop=@('that this with from than then when which into over rather instead before after every each been were will would should could must have does such also only more most same other because while they them their there' -split ' '); function W($t){ $w=@([regex]::Matches($t.ToLower(), '[a-z0-9]+') | ForEach-Object { $_.Value } | Where-Object { ($_.Length -ge 4) -and ($stop -notcontains $_) }); ,@($w | Select-Object -Unique) } function SAME($a,$b){ $x=W $a; $y=W $b; if($x.Count -lt $y.Count){ $s=$x; $l=$y } else { $s=$y; $l=$x }; if($s.Count -lt 4){ return $false }; foreach($w in $s){ if($l -notcontains $w){ return $false } }; return $true } if(-not (Test-Path -LiteralPath $o)){ 'EX_TOTAL=0'; 'EX_CLOSED=0'; 'EX_OK=no'; 'EX_WHY=there is no record at this root at all'; exit }; $raw=[Text.Encoding]::UTF8.GetString([IO.File]::ReadAllBytes($o)); if(($raw.Length -gt 0) -and ($raw[0] -eq [char]0xFEFF)){ $raw=$raw.Substring(1) }; $inf=$false; $at=@(); $n=-1; foreach($ln in [regex]::Split($raw, [char]13+[string][char]10+'|'+[string][char]10+'|'+[string][char]13)){ if($ln -match '^\s*(``````|~~~)'){ $inf=-not $inf; continue }; if($inf){ continue }; if($ln -match '^ATTEMPT: *([0-9]+\.[0-9]+) *\| *(.*)$'){ if($Matches[1] -ne $want){ continue }; $f=$Matches[2] -split '\|', 4; if($f.Count -lt 4){ continue }; $at+=@{ who=$f[0].Trim(); verdict=$f[1].Trim(); fate=$f[2].Trim(); approach=$f[3].Trim(); reason='' }; $n=$at.Count-1; continue }; if($ln -match '^REASON: *([0-9]+\.[0-9]+) *\| *[^|]*\| *(.*)$'){ if(($Matches[1] -eq $want) -and ($n -ge 0)){ $at[$n].reason=$Matches[2].Trim() } } }; 'EX_TOTAL=' + $at.Count; $closed=@(); $dupes=0; $nr=0; foreach($a in $at){ if($a.verdict -ne 'no'){ continue }; if($a.fate -ne 'executed'){ continue }; $seen=$false; foreach($c in $closed){ if(SAME $c.approach $a.approach){ $seen=$true; break } }; if($seen){ $dupes++; continue }; if($a.reason -eq ''){ $nr++ }; $closed+=$a }; 'EX_CLOSED=' + $closed.Count; 'EX_DUPES=' + $dupes; 'EX_NOREASON=' + $nr; $ok=($closed.Count -ge 3); 'EX_OK=' + $(if($ok){ 'yes' } else { 'no' }); $w=@(); $i=0; foreach($c in $closed){ $i++; $w+=('  route ' + $i + ' of ' + $closed.Count + ' - ' + $c.who); $w+=('    approach : ' + $c.approach); $w+=('    closed   : ' + $(if($c.reason -ne ''){ $c.reason } else { 'no reason recorded - this attempt was written before unit 071, and it is COUNTED rather than discarded' })); $w+='' }; [IO.File]::WriteAllText($lf, ($w -join ([char]13 + [string][char]10)) + [char]13 + [string][char]10, (New-Object Text.UTF8Encoding($false)))"`) do set "%%A=%%B"
goto :eof

rem ============================================================
rem  THE PERMITTED ENDING. Criterion 4.6: exit 0, and the criterion and
rem  every closed route on the console, so the owner can read three routes
rem  and three reasons without opening a file.
:exhaustok
echo.
echo   ================================================================
echo   THE CRITERION IS EXHAUSTED, AND THE RECORD SHOWS IT.
echo   ================================================================
echo   criterion : %ADV_STEP%.%ADV_CRIT%
echo   the record holds %EX_CLOSED% distinct closed route^(s^) out of %EX_TOTAL% attempt^(s^).
if not "%EX_DUPES%"=="0" echo   %EX_DUPES% further attempt^(s^) repeated a route already listed and were folded into it.
if not "%EX_NOREASON%"=="0" echo   %EX_NOREASON% of them carry no recorded reason - written before unit 071, counted not discarded.
echo.
if exist "%WORK%\exhaust.txt" type "%WORK%\exhaust.txt"
echo   THIS IS AN ENDING, NOT A STOP. Ruling 2: an ending is a criterion
echo   exhausted on the record. Nothing was launched and nothing was spent.
set "STOPWHY=ending: criterion %ADV_STEP%.%ADV_CRIT% is exhausted - %EX_CLOSED% distinct closed routes on the record"
set "RC=0"
goto :stopped

rem ============================================================
rem  A THIN OR REPETITIVE RECORD. Criteria 4.2 and 4.3.
rem
rem  IT IS REFUSED AND THE LOOP CARRIES ON. NOT A HALT. Ruling 2 and the
rem  instruction say so in terms, and it is the whole point: an arbiter that
rem  wants to stop will assert exhaustion for the same reason HamLet's units
rem  asserted a ruling whenever they wanted work. Refusing it and halting
rem  would give it what it asked for.
rem
rem  IT REDIRECTS, reusing 070's mechanism rather than a second one, so the
rem  next instruction is pinned to the same criterion with an approach the
rem  record does not show failing - which is exactly what an arbiter that is
rem  wrong about being out of routes should be made to do.
:exhaustthin
echo.
echo   REFUSED: THE RECORD DOES NOT SHOW THIS CRITERION IS EXHAUSTED.
echo   criterion : %ADV_STEP%.%ADV_CRIT%
echo   the record holds %EX_CLOSED% distinct closed route^(s^); %EXBAR% are needed.
if not "%EX_DUPES%"=="0" echo   %EX_DUPES% attempt^(s^) repeated a route already counted and were NOT counted twice.
if "%EX_TOTAL%"=="0" echo   THE RECORD FOR THIS CRITERION IS EMPTY. Nothing has been tried.
echo.
echo   An ending is DEMONSTRATED, never declared - the owner's ruling of
echo   2026-09-14. Nothing is halted here: you are REDIRECTED, and the loop
echo   carries on. Take a route to this criterion that the record does not
echo   already show failing.
set "RD_RULE=exhaustion claimed against a record that does not show it"
set "RD_CRITERIA=%ADV_STEP%.%ADV_CRIT%"
set "RD_DETAIL=you declared criterion %ADV_STEP%.%ADV_CRIT% exhausted, and the record holds %EX_CLOSED% distinct closed route(s) where %EXBAR% are needed. An ending is demonstrated from the record, never asserted"
goto :redirectnext

rem ============================================================
rem  AN EXHAUSTION CLAIM THAT NAMES NO CRITERION. The blocker form names
rem  none, and there is nothing to test a claim against. Refused the same
rem  way - redirected, never halted.
:exhaustnocrit
echo.
echo   REFUSED: MOVE: exhausted WITHOUT A CRITERION.
echo   An exhaustion claim must name the criterion whose routes have run out,
echo   in the ADVANCES form 'step N criterion k'. A blocker-clear names none,
echo   so there is nothing the record could be asked to show.
set "RD_RULE=exhaustion claimed without naming a criterion"
set "RD_CRITERIA=%A_STEP%.1"
set "RD_DETAIL=you declared exhaustion without naming a criterion, so nothing could be tested against the record"
goto :redirectnext

rem ============================================================
rem  070 task 2. THE REDIRECT ITSELF. It writes .run-unit\redirect.txt and
rem  falls through to the arbiter, which is called ONCE PER ITERATION as it
rem  always was - the redirect narrows what the arbiter may name, it does not
rem  author a second time inside one iteration.
rem
rem  A REDIRECTED ITERATION COSTS AN ITERATION, and that is the arbiter-s
rem  decision - author-s, overrulable - taken on a measurement rather than a
rem  preference. 070 task 1 found that :cost reads total_cost_usd out of
rem  last-run.json, the UNIT-s JSON, and that the arbiter-s own arbiter.json
rem  is parsed for is_error and denials and NEVER for its cost. So --budget
rem  cannot see an arbiter call at all, and a redirect that did not cost an
rem  iteration would be bounded by NOTHING. --max-iterations is the only brake
rem  that reaches it.
rem
rem  COUNT IS FOR THE CONSOLE AND IS NOT A CAP. Nothing reads it to decide
rem  anything. Ruling 2 rejects an attempt ceiling, and a cap here would be
rem  that ceiling wearing another name.
rem ============================================================
rem  070 task 2. Is a redirect in force, and does this instruction satisfy it?
rem  RQ_ON stays empty where there is no redirect.txt, and then nothing is
rem  required and nothing is refused.
rem
rem  A BLOCKER-CLEAR SATISFIES A REDIRECT, AND THIS WAS FOUND BY READING THE
rem  REDIRECT BACK AGAINST RULING 2 - task 6 asks for exactly that, and it is
rem  what 068 and 069 each learned to do. The first cut compared
rem  ADV_STEP.ADV_CRIT against the required criteria, and :advparse leaves both
rem  EMPTY for the blocker form - so a blocker-clear read as "." , matched
rem  nothing, and was redirected again. FOR EVER, until the backstop.
rem
rem  That refuses the layer-s own escape hatch at the one moment it is most
rem  needed. Ruling 2 says find a way through, and clearing what stops a
rem  criterion being counted IS a way through - it is the route 064 built and
rem  062 and 063 used. It is already bounded by the two-consecutive-blocker-
rem  clears halt, which 070 deliberately leaves in place, so permitting it here
rem  adds no unbounded path. Author-s, overrulable.
:redirectreq
set "RQ_ON="
set "RQ_MET=yes"
for /f "usebackq tokens=1,* delims==" %%A in (`powershell -NoProfile -Command "$f='%WORK%\redirect.txt'; if(-not (Test-Path -LiteralPath $f)){ exit }; $want=''; $rule=''; $detail=''; $count='1'; foreach($ln in (Get-Content -LiteralPath $f)){ if($ln -match '^CRITERIA:\s*(.*)$'){ $want=$Matches[1].Trim() }; if($ln -match '^RULE:\s*(.*)$'){ $rule=$Matches[1].Trim() }; if($ln -match '^DETAIL:\s*(.*)$'){ $detail=$Matches[1].Trim() }; if($ln -match '^COUNT:\s*(.*)$'){ $count=$Matches[1].Trim() } }; 'RQ_ON=yes'; 'RQ_WANT=' + $want; 'RQ_RULE=' + $rule; 'RQ_DETAIL=' + $detail; 'RQ_COUNT=' + $count; $mine='%ADV_STEP%.%ADV_CRIT%'; $kind='%ADV_KIND%'; if($kind -eq 'blocker'){ $mine='a blocker-clear' }; 'RQ_MINE=' + $mine; $ok=($kind -eq 'blocker'); foreach($w in ($want -split ' ')){ if($w.Trim() -eq $mine){ $ok=$true } }; 'RQ_MET=' + $(if($ok){ 'yes' } else { 'no' })"`) do set "%%A=%%B"
goto :eof

:doredirect
set "RD_COUNT=1"
for /f "usebackq tokens=1,* delims==" %%A in (`powershell -NoProfile -Command "$f='%WORK%\redirect.txt'; $n=1; if(Test-Path -LiteralPath $f){ foreach($ln in (Get-Content -LiteralPath $f)){ if($ln -match '^COUNT:\s*([0-9]+)'){ $n=[int]$Matches[1] + 1 } } }; $o=@('RULE: %RD_RULE%', 'CRITERIA: %RD_CRITERIA%', 'DETAIL: %RD_DETAIL%', ('COUNT: ' + $n)); [IO.File]::WriteAllText($f, ($o -join ([char]13 + [string][char]10)) + [char]13 + [string][char]10, (New-Object Text.UTF8Encoding($false))); 'RD_COUNT=' + $n"`) do set "%%A=%%B"
echo.
echo   REDIRECTED, NOT HALTED: %RD_RULE%.
echo     %RD_DETAIL%
echo     the next instruction must name one of: %RD_CRITERIA%
echo     with an approach the record does not show failing at it.
echo     redirect %RD_COUNT% in this run. THIS IS NOT A CAP - the bound is
echo     --budget, --minutes and --max-iterations.
call :ledgerredirect "%RD_RULE%"
goto :eof

rem  TWO WAYS IN, AND THEY GO DIFFERENT PLACES, WHICH IS THE WHOLE ACCOUNTING.
rem
rem  :redirectnow is entered from the TOP of an iteration, before the arbiter
rem  has been called - that is where no-advance is read from the record. The
rem  redirect applies to THIS iteration-s authoring, so it falls through into
rem  the arbiter and the iteration proceeds normally. One iteration, one
rem  arbiter call, one unit.
rem
rem  :redirectnext is entered AFTER the arbiter has authored - a repeated
rem  approach, or a criterion it was not sent to. Authoring again means another
rem  arbiter call, and THAT COSTS AN ITERATION: it goes to :iterate, ITER
rem  increments, and --max-iterations can see it. Falling back into the arbiter
rem  inside the same iteration would author twice on one iteration number and
rem  leave the only brake that reaches a redirect unable to count it.
:redirectnow
call :doredirect
goto :noredirect

:redirectnext
set /a REDIRECTED+=1
call :doredirect
echo       this iteration launched no unit. The loop carries on.
goto :iterate

rem  ONE LEDGER NOTE PER REDIRECT, so a reader can tell WHICH RULE FIRED
rem  without the console. A note row, not a run line, so an iteration is still
rem  counted once - the shape :ledgeradv already uses.
:ledgerredirect
set "NOWSTAMP="
for /f "usebackq delims=" %%D in (`powershell -NoProfile -Command "(Get-Date).ToString('yyyy-MM-ddTHH:mm')"`) do set "NOWSTAMP=%%D"
call "%HERE%ledger.bat" "%ITER%" "%NOWSTAMP%" "%NOWSTAMP%" "note" "redirected - %~1: %RD_DETAIL%. The next instruction must name one of %RD_CRITERIA% with an approach the record does not show failing at it. Redirect %RD_COUNT% of this run, which is not a cap." "none - not a run" "%ROOT%" >nul
echo       ledger     : redirect noted - %~1
goto :eof

:redirectsame
set "RD_RULE=no-advance at one criterion"
set "RD_CRITERIA=%LA_CRIT1%"
set "RD_DETAIL=units %LA_UNITS% both ran against criterion %LA_CRIT1% and neither moved it"
goto :redirectnow

:redirectwander
set "RD_RULE=no-advance at two different criteria"
set "RD_CRITERIA=%LA_CRIT1% %LA_CRIT2%"
set "RD_DETAIL=units %LA_UNITS% ran against criteria %LA_CRIT1% and %LA_CRIT2% and moved neither"
goto :redirectnow

rem  THE STOP 10 ARM IS KEPT AND IS NOW UNREACHABLE FROM :lastadvances.
rem  Left in place deliberately rather than deleted: a reader looking for the
rem  halt 064 built should find it and find this note, and nothing else in the
rem  file jumps here. 070 task 5 quotes its wording where the fixtures moved.
:stop10
echo.
echo   STOP 10: NO PROGRESS. Units %LA_UNITS% both ran and neither moved a
echo   criterion of step %LA_STEP% from unmet to met, as criteria-count.bat counts
echo   them in PHASE_PLAN.md. Two in a row is stop 10 - the owner's ruling of 2026-09-14.
set "STOPWHY=stop 10: no progress - units %LA_UNITS% moved no criterion of step %LA_STEP%"
goto :stopped

:blockertwice
echo.
echo   HALTED: TWO CONSECUTIVE BLOCKER-CLEARING UNITS. Units %LA_UNITS% each
echo   cleared a blocker and moved no criterion of step %LA_STEP%. The blocker form is
echo   permitted, and bounded: twice in a row halts. This is not stop 10.
set "STOPWHY=halted: two consecutive blocker-clearing units - units %LA_UNITS% moved no criterion"
goto :stopped

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
rem  J_HONEST: yes or no from the judge, asked ONLY when the script saw the
rem  named criterion flip. unread otherwise, and unread is never a yes.
set "J_HONEST=unread"
rem  J_REVERSES: yes or no from the judge - does the report CLAIM a ruling that
rem  reverses an earlier arbiter ruling? 065 task 4. unread is never a yes, and
rem  J_REVERSED carries the ruling as the judge quoted it.
set "J_REVERSES=unread"
rem  J_FOLLOWS: plan or report from the judge - 069 criterion 6.5. Does the
rem  criterion this instruction chose follow from the plan, or only from the
rem  previous report? unread is never report, the same way unread is never yes.
set "J_FOLLOWS=unread"
set "J_FOLLOWED="
set "J_REVERSED="
if not exist "%ROOT%\output.md" goto :jsnoreport
set "JSPLAN=%WORK%\step-plan.txt"
set "JSPROMPT=%WORK%\state-prompt.txt"
set "JSJSON=%WORK%\state-verdict.json"
powershell -NoProfile -Command "$f='%ROOT%\PHASE_PLAN.md'; $n='%A_STEP%'; $t=Get-Content -LiteralPath $f; $out=@(); foreach($ln in $t){ if($ln -cmatch ('^STEP: ' + [regex]::Escape($n) + ' \|')){ $out+=$ln } }; $on=$false; foreach($ln in $t){ if($ln -match ('^#{1,6}\s+Step\s+' + [regex]::Escape($n) + '\b')){ $on=$true; $out+=$ln; continue }; if($on -and ($ln -match '^#{1,6}\s+Step\s+\d' -or $ln -match '^-{3,}\s*$')){ break }; if($on){ $out+=$ln } }; if($out.Count -eq 0){ $out=@('(the plan has no STEP ' + $n + ' - judge on the report alone)') }; $out | Set-Content -LiteralPath '%JSPLAN%' -Encoding utf8"
if not exist "%JSPLAN%" goto :jsdone
call :writejsprompt
type "%JSPLAN%" >> "%JSPROMPT%"
rem  THE JUDGE'S NARROWER QUESTION. 064 task 4. The script has already
rem  counted; where it saw the named criterion flip, the judge is told so
rem  and asked only whether it was honestly met. Where nothing flipped
rem  there is no honesty question to ask, and none is asked.
if not "%FLIPPED%"=="1" goto :jsnoflip
>>"%JSPROMPT%" echo.
>>"%JSPROMPT%" echo --- what the script saw ---
>>"%JSPROMPT%" echo criteria-count.bat saw step %ADV_STEP% criterion %ADV_CRIT% flip from unmet to met in
>>"%JSPROMPT%" echo PHASE_PLAN.md during this unit. THE COUNT IS THE SCRIPT'S; you do not count.
>>"%JSPROMPT%" echo Answer one more line after WHY:
>>"%JSPROMPT%" echo HONEST: yes   if the report shows that criterion honestly met
>>"%JSPROMPT%" echo HONEST: no    if the marker was turned without the criterion being met
:jsnoflip
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
for /f "usebackq tokens=1,* delims==" %%A in (`powershell -NoProfile -Command "try{ $raw=Get-Content -LiteralPath '%JSJSON%' -Raw; $k=$raw.IndexOf([char]123); if($k -lt 0){ exit }; $j=$raw.Substring($k) | ConvertFrom-Json }catch{ exit }; $r=[string]$j.result; $v=''; $w=''; $h=''; $rv=''; $rq=''; $fv=''; $fq=''; $inw=$false; foreach($ln in ($r -split [char]10)){ $s=$ln.Trim(); if($s -match '^STATE:\s*(.+?)\s*$'){ $v=$Matches[1]; $inw=$false; continue }; if($s -match '(?i)^HONEST:\s*(yes|no)\b'){ $h=$Matches[1].ToLower(); $inw=$false; continue }; if($s -match '(?i)^REVERSES:\s*(yes|no)\b'){ $rv=$Matches[1].ToLower(); $inw=$false; continue }; if($s -match '(?i)^REVERSED:\s*(.+)$'){ $rq=$Matches[1]; $inw=$false; continue }; if($s -match '(?i)^FOLLOWS:\s*(plan|report)\b'){ $fv=$Matches[1].ToLower(); $inw=$false; continue }; if($s -match '(?i)^FOLLOWED:\s*(.+)$'){ $fq=$Matches[1]; $inw=$false; continue }; if($s -match '^WHY:\s*(.+)$'){ $w=$Matches[1]; $inw=$true; continue }; if($inw){ if(($s -eq '') -or ($s -match '^[A-Z_]+:')){ $inw=$false } else { $w=$w + ' ' + $s } } }; $ok='not started','in progress','partial','blocked','done'; if($ok -contains $v.ToLower()){ 'J_STATE=' + $v.ToLower() }; if($h){ 'J_HONEST=' + $h }; if($rv){ 'J_REVERSES=' + $rv }; if($fv){ 'J_FOLLOWS=' + $fv }; if($fq){ 'J_FOLLOWED=' + (((($fq -replace '[&|<>^%%]','') -replace [char]96,'') -replace [char]34,'') -replace '\s+',' ').Trim() }; if($rq){ 'J_REVERSED=' + (((($rq -replace '[&|<>^%%]','') -replace [char]96,'') -replace [char]34,'') -replace '\s+',' ').Trim() }; if($w){ 'J_WHY=' + (((($w -replace '[&|<>^%%]','') -replace [char]96,'') -replace [char]34,'') -replace '\s+',' ').Trim() }"`) do set "%%A=%%B"
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
>>"%JSPROMPT%" echo WHAT THE THIRD MEANS - the owner's ruling of 2026-09-13, which defines it
>>"%JSPROMPT%" echo and does not widen it: A promise is a fact the product states to the
>>"%JSPROMPT%" echo operator about the radio, a contact, or a send - what was logged, what was
>>"%JSPROMPT%" echo heard, what went out. THE WORDING OF A HINT, A LABEL, A TARGET, A CARD'S
>>"%JSPROMPT%" echo NEXT LINE, OR A NAME IS NEVER A PROMISE AND NEVER A STOP, so a step
>>"%JSPROMPT%" echo waiting only on such wording is not blocked.
>>"%JSPROMPT%" echo.
>>"%JSPROMPT%" echo THE COUNT IS THE SCRIPT'S. Whether this unit advanced the plan is decided by
>>"%JSPROMPT%" echo criteria-count.bat, which reads the - [ ] and - [x] criterion lines of
>>"%JSPROMPT%" echo PHASE_PLAN.md before and after the unit - not by you, and not by the report.
>>"%JSPROMPT%" echo You judge the step's state, and, only where you are told a criterion
>>"%JSPROMPT%" echo flipped, whether it was honestly met.
>>"%JSPROMPT%" echo.
>>"%JSPROMPT%" echo SELF-RULINGS ARE BOUNDED - the owner's ruling of 2026-09-14. A unit may make
>>"%JSPROMPT%" echo at most two self-rulings that authorize work outside its instruction's
>>"%JSPROMPT%" echo tasks, each citing the plan line it applies; a third needed means the plan
>>"%JSPROMPT%" echo is unclear, and the unit reports it as a mismatch instead of ruling. Its
>>"%JSPROMPT%" echo decisions about HOW to carry out an assigned task are uncapped, and all of
>>"%JSPROMPT%" echo them are reported. NO SELF-RULING MAY OVERRULE AN EARLIER ARBITER RULING -
>>"%JSPROMPT%" echo only the owner does that.
>>"%JSPROMPT%" echo.
>>"%JSPROMPT%" echo ONE QUESTION ABOUT IT, answered in one word. Does this unit's report claim a
>>"%JSPROMPT%" echo ruling that reverses, withdraws or overrules an earlier arbiter ruling? Do
>>"%JSPROMPT%" echo not judge intent and do not count the unit's decisions - only whether a
>>"%JSPROMPT%" echo reversal of an earlier ruling is CLAIMED. Answer one more line:
>>"%JSPROMPT%" echo REVERSES: yes   or   REVERSES: no
>>"%JSPROMPT%" echo and where it is yes, one more line quoting that ruling from the report:
>>"%JSPROMPT%" echo REVERSED: the ruling as the report states it
>>"%JSPROMPT%" echo.
rem  069 criterion 6.5. ONE QUESTION, ONE WORD, AND IT ASKS WHAT WAS CITED.
rem  The state judge is the one positioned for it: it already has the step's own
rem  section of the plan and the unit's report side by side, which is exactly
rem  the comparison. The section-4 judge sees only section 4 and has no plan.
rem
rem  IT IS NOT A JUDGMENT OF INTENT. The shape is 067's reversal question: a
rem  fact about what the instruction CITES for its choice, answered in one word,
rem  with the judge's own sentence quoted where it says report.
rem
rem  ASKED ONLY WHERE A CRITERION WAS CHOSEN. A blocker-clear names none, so
rem  there is nothing to have followed from and the question is not put.
if not "%ADV_KIND%"=="criterion" goto :jsnofollows
>>"%JSPROMPT%" echo A SECOND QUESTION, ALSO IN ONE WORD. The instruction that produced this
>>"%JSPROMPT%" echo report chose one criterion to advance. Does that choice FOLLOW FROM THE
>>"%JSPROMPT%" echo PLAN - the step and criteria above - or ONLY FROM THE PREVIOUS REPORT,
>>"%JSPROMPT%" echo which is to say from what the last unit raised rather than from what the
>>"%JSPROMPT%" echo phase is for? Do not judge whether the choice was wise, and do not judge
>>"%JSPROMPT%" echo the author. Judge only what the instruction CITES for it. Answer:
>>"%JSPROMPT%" echo FOLLOWS: plan     the criterion and its reasoning come from the plan
>>"%JSPROMPT%" echo FOLLOWS: report   the criterion was chosen because the last report
>>"%JSPROMPT%" echo                   raised it, and the plan is cited for form only
>>"%JSPROMPT%" echo and where it is report, one more line saying what it cited instead:
>>"%JSPROMPT%" echo FOLLOWED: one sentence, plain text
>>"%JSPROMPT%" echo.
>>"%JSPROMPT%" echo The instruction it produced named  step %ADV_STEP% criterion %ADV_CRIT%  and gave this WHY:
>>"%JSPROMPT%" echo.
if exist "%WORK%\why.txt" type "%WORK%\why.txt" >> "%JSPROMPT%"
if not exist "%WORK%\why.txt" >>"%JSPROMPT%" echo   (the launcher could not read a WHY from the instruction)
>>"%JSPROMPT%" echo.
:jsnofollows
>>"%JSPROMPT%" echo.
>>"%JSPROMPT%" echo Answer with the lines asked for above and nothing else, beginning with:
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
>>"%S4PROMPT%" echo WHAT THE THIRD MEANS - the owner's ruling of 2026-09-13, which defines it
>>"%S4PROMPT%" echo and does not widen it: A promise is a fact the product states to the
>>"%S4PROMPT%" echo operator about the radio, a contact, or a send - what was logged, what was
>>"%S4PROMPT%" echo heard, what went out. THE WORDING OF A HINT, A LABEL, A TARGET, A CARD'S
>>"%S4PROMPT%" echo NEXT LINE, OR A NAME IS NEVER A PROMISE AND NEVER A STOP.
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
rem  ONE READER, at :killcount - 067 task 2. The launched check at 4pre asks
rem  the same question before the record is touched, and two copies of one
rem  read drift. This arm should now be reached only after a kill, because
rem  a held lock halts at stop 11 above; it is kept because a reader here
rem  must be able to see which of the two an exit 1 was.
call :killcount
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
rem  STOP 11, BOTH HALVES. 067 task 2.
rem
rem  THE NUMBER IS THE ARBITER'S - author's, overrulable - under the
rem  owner's ruling of 2026-09-12 that the arbiter stops for three things
rem  only, none of which is a number. Eleven because one to ten are taken.
rem
rem  ONE NUMBER FOR TWO SENTENCES, because it is one condition: there is no
rem  report of this unit's to judge. Which of the two it was is in the
rem  reason, and the reason is what the ledger line carries.
rem
rem  NOTHING IS APPENDED ON EITHER PATH, AND THE CONSOLE SAYS SO, because a
rem  reader who sees a halt and no entry must be able to tell that from a
rem  halt whose entry failed to write.
:nothingran
echo.
echo   STOP 11: NOTHING WAS LAUNCHED, SO THERE IS NO UNIT TO JUDGE.
echo   run-unit-watched exit %RUNRC% - run-unit.bat records that as having
echo   launched nothing. An exit 1 with no kill in watched.log is the
echo   session lock, held by something else in this tree.
call :saywhatitfound
echo   NOTHING WAS APPENDED TO THE RECORD. An entry for a unit that did not
echo   run is the fault this check exists to prevent, and a quieter spelling
echo   of it is still it.
set "STOPWHY=stop 11: nothing was launched - run exit %RUNRC%, and %OWNWHY%"
rem  WHAT IT FOUND GOES IN THE REASON, not only on the console. The ledger
rem  is what the owner reads instead of watching, and the one fact that
rem  tells him whose report was lying at that root is the UNIT: line.
if defined OWNFOUND set "STOPWHY=%STOPWHY% - the file at the root says UNIT: %OWNFOUND%"
goto :stopped

:noreport
echo.
echo   STOP 11: NO REPORT WAS WRITTEN BY THIS UNIT.
echo   The run launched and exited %RUNRC%, and nothing at the root can be
echo   shown to be its report.
call :saywhatitfound
echo   NOTHING WAS APPENDED TO THE RECORD. The state judge was not called.
set "STOPWHY=stop 11: no report was written by this unit - %OWNWHY%"
if defined OWNFOUND set "STOPWHY=%STOPWHY% - the file at the root says UNIT: %OWNFOUND%"
goto :stopped

rem  WHAT IT FOUND INSTEAD, NAMED RATHER THAN DESCRIBED. The other file's
rem  own UNIT: line is the one thing that tells the owner whose report has
rem  been lying at that root, and it is what HamLet's console never said.
:saywhatitfound
echo   at the root : %OWNWHY%
if defined OWNSTAMP echo   stamped     : %OWNSTAMP%   output.md written: %OWNWRITE%
if defined OWNFOUND echo   THAT FILE SAYS  UNIT: %OWNFOUND%
if not defined OWNFOUND if "%OWNTHERE%"=="yes" echo   that file carries no UNIT: line to name it by
goto :eof

rem ============================================================
:stopped
echo.
echo ============================================================
echo  THE LOOP HALTED
echo    after     : %ITER% iteration^(s^) - %LAUNCHEDN% launched a unit, %REDIRECTED% spent on a redirect
echo    because   : %STOPWHY%
echo    spent     : %SPENT% of %BUDGET%
echo ============================================================
rem  EVERY STOP 1 EXITS 0. 065 extended stop 1 to the phase waiting only on the
rem  owner's verdict, so the test is the reason's prefix, not one exact sentence.
rem  071: AN EXHAUSTION ENDING EXITS 0, LIKE stop 1 AND FOR THE SAME REASON.
rem  Criterion 4.6 asks for it in terms, and ruling 2 is why: an ending is a
rem  criterion exhausted on the record, and an ending is NOT A STOP. Exit 1
rem  here would record the one outcome the phase is working toward as a
rem  failure. Every other STOPWHY still exits 1, unchanged.
set "RC=1"
if "%STOPWHY:~0,7%"=="stop 1:" set "RC=0"
if "%STOPWHY:~0,8%"=="ending: " set "RC=0"
call :ledgerstop
rem  072 task 4: THE REVIEW SHEET IS WRITTEN AT AN ENDING, not only at the
rem  owner-s-verdict one. Criterion 5.3 says at an ending, and after 071 there
rem  are five of them - a criterion exhausted, the plan satisfied, a ruling
rem  wanted on one of the three, the arbiter raising one of the three, and the
rem  owner-s verdict. A night that ended because the routes ran out is exactly
rem  the night he most needs the sheet for, and before this it wrote none.
rem  :verdictof has already run, so LEDVERDICT says which kind this was.
if "%LEDVERDICT%"=="ending" call :reviewsheet
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
rem  THE PLAN LEADS, THE REPORT FOLLOWS. 069 tasks 3, criteria 6.1 to 6.3, on
rem  the owner's ruling of 2026-09-19: the report must inform the arbiter and
rem  must not dominate it.
rem
rem  WHAT WAS HERE BEFORE, measured by 069 task 2 rather than assumed: the
rem  prompt NAMED four files to go and read and put none of their contents in
rem  front of the arbiter except 068's attempt lines. So the previous report
rem  was not dominant BY LENGTH - it was UNBOUNDED AND UNLABELLED, fetched by
rem  the arbiter's own Read tool with nothing saying what it was for. The phase
rem  goal did not appear at all, and neither did one criterion: measured 0
rem  occurrences of a - [ ] line in a real prompt file.
rem
rem  SO THE ORDER IS NOW FIXED IN THE FILE. The goal and the criteria are
rem  written first, the rules next, the attempt record after them, and the
rem  previous report LAST and BOUNDED. Putting the report in the prompt at all
rem  is stricter than naming it, not looser: a bounded, labelled block replaces
rem  an unbounded read.
>"%ARBPROMPT%" echo Read ARBITER.md at this repository root and act as it says.
>>"%ARBPROMPT%" echo.
>>"%ARBPROMPT%" echo You are the arbiter. Author the next WORK_INSTRUCTIONS.md.
call :promptplan
rem  070: AND THE REDIRECT, IMMEDIATELY AFTER THE PLAN. It belongs there and
rem  nowhere else: it narrows WHICH of the criteria just listed the arbiter may
rem  name, so it is part of the plan block-s meaning rather than context for it.
rem  Ruling 3 keeps the plan leading; this does not displace it.
call :promptredirect
rem  THE FILE LIST, REWRITTEN. FOUND BY READING THIS PROMPT BACK AGAINST THE
rem  OWNER'S RULING, which is what 069 task 6 asks for and what 068 learned to
rem  do. The list used to read:
rem
rem      Read first, in this order:
rem        .run-unit\reload.txt   - the measured picture, disagreements first
rem        PHASE_PLAN.md          - what the phase is for
rem        PHASE_OUTCOME.md       - what has been tried and what it hit
rem        output.md              - the last unit's report, if there is one
rem
rem  WITH THE REPORT NOW BOUNDED IN THE PROMPT, THAT LAST LINE UNDID THE BOUND.
rem  It sent the arbiter to read the whole unbounded file as the FOURTH thing it
rem  did, before it had chosen a criterion - which is the fixation this step
rem  exists to stop, arriving through the very block that was meant to order the
rem  reading. A cap on a copy is worth nothing beside an instruction to go and
rem  fetch the original. The first three lines were also redundant: the plan and
rem  the record are now composed into this prompt above.
rem
rem  SO THE LIST NAMES ONLY WHAT IS NOT ALREADY HERE, and says where the rest
rem  is. The whole report is still at the root and the arbiter is still free to
rem  open it - ruling 3 keeps it - but it is no longer TOLD to, and it is told
rem  what to do first instead.
>>"%ARBPROMPT%" echo.
>>"%ARBPROMPT%" echo THE PLAN, THE ATTEMPT RECORD AND THE LAST REPORT ARE ALL IN THIS PROMPT -
>>"%ARBPROMPT%" echo the criteria above, the rest below. One thing is not:
>>"%ARBPROMPT%" echo.
>>"%ARBPROMPT%" echo   .run-unit\reload.txt   - the measured picture, disagreements first.
>>"%ARBPROMPT%" echo                            READ THIS. Nothing else here carries it.
>>"%ARBPROMPT%" echo.
>>"%ARBPROMPT%" echo   PHASE_PLAN.md          - open it if you need a step's full text. The
>>"%ARBPROMPT%" echo                            criteria above are quoted from it verbatim.
>>"%ARBPROMPT%" echo.
>>"%ARBPROMPT%" echo   output.md              - the last report. A bounded extract is at the
>>"%ARBPROMPT%" echo                            BOTTOM of this prompt. Open the whole file only
>>"%ARBPROMPT%" echo                            to check something the extract raised about a
>>"%ARBPROMPT%" echo                            criterion - never as your starting point, and
>>"%ARBPROMPT%" echo                            never to find work in it.
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
>>"%ARBPROMPT%" echo PROGRESS IS COUNTED IN CRITERIA, AND THE COUNT IS THE SCRIPT'S - ARBITER.md
>>"%ARBPROMPT%" echo section 7. ADVANCES must read  step N criterion k  - one line of PHASE_PLAN.md
>>"%ARBPROMPT%" echo in the form  - [ ] N.k  - or  none - clears a blocker:  naming the unit number
>>"%ARBPROMPT%" echo or criterion it unblocks. The launcher counts that criterion before and after
>>"%ARBPROMPT%" echo the unit; if it did not flip, the unit did not advance, and two such units in a
>>"%ARBPROMPT%" echo row are stop 10. A STEP WHOSE STATE IN PHASE_OUTCOME.md IS done IS CLOSED: do
>>"%ARBPROMPT%" echo not author into it. The launcher refuses it before the unit runs, and only the
>>"%ARBPROMPT%" echo owner reopens a step, by a ruling in the plan.
>>"%ARBPROMPT%" echo.
>>"%ARBPROMPT%" echo THE OWNER'S VERDICT IS MARKED AND ENDS THE RUN - ARBITER.md section 7. A
>>"%ARBPROMPT%" echo criterion ending *owner's verdict* is the owner's alone: never name one in
>>"%ARBPROMPT%" echo ADVANCES, and never author a unit toward a verdict the loop cannot give.
>>"%ARBPROMPT%" echo.
>>"%ARBPROMPT%" echo YOUR SELF-RULINGS ARE BOUNDED - the owner's ruling of 2026-09-14. At most two
>>"%ARBPROMPT%" echo self-rulings that authorize work outside the instruction's tasks, each citing
>>"%ARBPROMPT%" echo the plan line it applies; if a third is needed, the plan or the instruction
>>"%ARBPROMPT%" echo is unclear - report it as a mismatch instead of ruling a third time.
>>"%ARBPROMPT%" echo Decisions about how to carry out an assigned task are uncapped, and every
>>"%ARBPROMPT%" echo one is reported. NO SELF-RULING MAY OVERRULE AN EARLIER ARBITER RULING -
>>"%ARBPROMPT%" echo only the owner does that, and a report claiming one halts the loop.
>>"%ARBPROMPT%" echo.
>>"%ARBPROMPT%" echo Write WORK_INSTRUCTIONS.md and end it with the ARBITER-DECISION
>>"%ARBPROMPT%" echo block exactly as ARBITER.md section 7 specifies. Write nothing else.

rem  AND THE ATTEMPT RECORD ITSELF, APPENDED. 068 task 4, criterion 2.5.
rem
rem  THIS IS THE HALF THAT MAKES THE REFUSAL FAIR. An arbiter that can see
rem  what failed can choose something else; one that cannot is being punished
rem  for a record it was never shown. Until now the prompt only NAMED
rem  PHASE_OUTCOME.md among four files to read, and no attempt line was ever
rem  extracted or put in front of it.
rem
rem  EVERY CRITERION, NOT ONE. 2.5 asks for the record for the criterion it is
rem  authoring against - and THE ARBITER CHOOSES THAT CRITERION, so it is not
rem  known until after it has authored. The only way to hand it the right
rem  record BEFORE it authors is to hand it every criterion's; the superset
rem  necessarily contains the one it picks. Author's, overrulable.
rem
rem  WRITTEN IN POWERSHELL, NOT BY echo. An approach is prose and carries
rem  pipes, ampersands and parentheses, and echo would execute half of it.
rem  Appended as UTF-8, sorted so one criterion's attempts sit together.
rem  IT COMES AFTER THE CRITERIA AND BEFORE THE REPORT. 069 criterion 6.1.
powershell -NoProfile -Command "$o='%ROOT%\PHASE_OUTCOME.md'; $p='%ARBPROMPT%'; if(-not (Test-Path -LiteralPath $o)){ exit }; $raw=[Text.Encoding]::UTF8.GetString([IO.File]::ReadAllBytes($o)); if(($raw.Length -gt 0) -and ($raw[0] -eq [char]0xFEFF)){ $raw=$raw.Substring(1) }; $inf=$false; $rows=@(); foreach($ln in [regex]::Split($raw, [char]13+[string][char]10+'|'+[string][char]10+'|'+[string][char]13)){ if($ln -match '^\s*(```|~~~)'){ $inf=-not $inf; continue }; if($inf){ continue }; if($ln -match '^ATTEMPT: *(.*)$'){ $rows += $Matches[1] } }; if($rows.Count -eq 0){ exit }; $out=@('', '--- WHAT HAS ALREADY BEEN TRIED, BY CRITERION ---', '', 'Every attempt this phase has recorded, criterion first. READ THIS BEFORE', 'YOU CHOOSE AN APPROACH. An approach recorded as no against a criterion is', 'REFUSED BEFORE THE UNIT RUNS if you name that criterion again, and the', 'iteration is spent on the refusal. yes means it succeeded and does not', 'block; blocker means it cleared something and moved no criterion.', ''); foreach($r in ($rows | Sort-Object)){ $out += '  ' + $r }; $out += @('', 'If every route you can see at a criterion is recorded as no, say so in WHY', 'and choose another criterion or another step. Do not restate a failed', 'approach in different words: the refusal matches on the words that carry', 'meaning, not on the phrasing.'); [IO.File]::AppendAllText($p, ($out -join ([char]13 + [string][char]10)) + [char]13 + [string][char]10, (New-Object Text.UTF8Encoding($false)))"

rem  AND THE PREVIOUS REPORT, LAST AND BOUNDED. 069 criteria 6.2 and 6.3.
call :promptreport
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
rem  THE APPROACH SNAPSHOT, 066 task 3, is removed first, so an iteration
rem  whose instruction is missing can never record the last one's approach.
if exist "%WORK%\approach.txt" del /q "%WORK%\approach.txt" 2>nul
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
rem  THE APPROACH, SNAPSHOTTED WHOLE FOR THE ATTEMPT RECORD. 066 task 3.
rem  A_APPROACH above has crossed cmd: its double quotes are turned to
rem  single ones, the file is read in the system code page, and a set line
rem  is bounded at cmd's 8191 characters. The attempt record is what step 2
rem  COMPARES approaches against, so it takes this copy instead - read as
rem  UTF-8, written byte for byte to .run-unit\approach.txt, never
rem  truncated and never carried by a cmd variable. Taken HERE, before the
rem  unit runs, so it is the launcher's reading of the instruction and not
rem  anything a session wrote afterwards. The last APPROACH: line wins, as
rem  in the parse above.
powershell -NoProfile -Command "$f='%ROOT%\WORK_INSTRUCTIONS.md'; $v=$null; foreach($ln in (Get-Content -LiteralPath $f -Encoding UTF8)){ if($ln -match '^APPROACH:\s*(.+?)\s*$'){ $v=$Matches[1] } }; if($null -ne $v){ [IO.File]::WriteAllText('%WORK%\approach.txt', $v, (New-Object Text.UTF8Encoding($false))) }"
rem  069 task 4: WHY is snapshotted the same way and for the same reason -
rem  it is prose, and a cmd variable mangles the characters cmd acts on. The
rem  check below reads the file, never the variable.
if exist "%WORK%\why.txt" del /q "%WORK%\why.txt" 2>nul
powershell -NoProfile -Command "$f='%ROOT%\WORK_INSTRUCTIONS.md'; $v=$null; foreach($ln in (Get-Content -LiteralPath $f -Encoding UTF8)){ if($ln -match '^WHY:\s*(.+?)\s*$'){ $v=$Matches[1] } }; if($null -ne $v){ [IO.File]::WriteAllText('%WORK%\why.txt', $v, (New-Object Text.UTF8Encoding($false))) }"
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
rem ============================================================
rem  072 task 3. WHICH ENDING, OR WHICH STOP - CRITERIA 5.1 AND 5.2.
rem
rem  Every halt already wrote a ledger line. What it wrote in the EXIT-STATE
rem  column was the word `halted`, hard-coded, for all twenty-eight of them: the
rem  exhaustion ending, the owner's verdict, a refused instruction and a watchdog
rem  kill all read the same at breakfast. The prose column told them apart; the
rem  column whose job is the verdict did not.
rem
rem  NO NEW COLUMN. ledger.bat's fourth argument is the exit state and it takes
rem  whatever it is given - task 2 looked before building. A column added here
rem  would also change how every line ALREADY WRITTEN is read, which ruling 5
rem  forbids.
rem
rem  THE CLASSIFICATION IS THE ARBITER'S - author's, overrulable - under the
rem  owner's ruling of 2026-09-12 that it stops for three things only, none of
rem  which is a form or a word. It is made from HIS rule of 2026-09-14:
rem
rem    An ending is a criterion exhausted on the record, one of the three stops,
rem    or the owner's verdict being all that remains. EVERYTHING ELSE is a stop,
rem    and a stop is recorded as FAILURE, IN THAT WORD.
rem
rem  FIVE ENDINGS. Read from STOPWHY's own prefix, so a halt cannot be
rem  classified one way in the code and another in the file:
rem
rem    ending: ...        a criterion exhausted on the record        (071)
rem    stop 1: ... owner  the owner's verdict is all that remains    (065)
rem    stop 1: satisfied  THE PHASE PLAN IS SATISFIED - see below
rem    stop 3: ... three  a ruling is wanted on one of the three
rem    stop 4: ...        the arbiter declared a decision the owner's,
rem                       which ARBITER.md section 6 permits ONLY for one of
rem                       the three - so it is one of the three, arriving by
rem                       the arbiter's hand rather than the judge's
rem
rem  THE ONE I COULD NOT READ STRAIGHT OFF THE RULING, reported rather than
rem  forced: `stop 1: the phase plan is satisfied`. The owner's three-item list
rem  does not name it. It is not an exhausted criterion, not one of the three,
rem  and not his verdict - it is EVERY CRITERION MET, the phase finished. Read
rem  literally, ruling 2's `everything else is a stop` would record a completed
rem  phase as a FAILURE. I judge that the list exists to separate honest ends
rem  from giving up, not to exclude succeeding, and classify it an ending. It
rem  already exits 0, which is the same judgement made by whoever wrote it.
rem  Author's, overrulable, and named in the report as the one that needed
rem  reasoning past the words.
rem
rem  TWO SPLITS WORTH SEEING, because each pair shares a number and parts here:
rem    stop 3  `a ruling is wanted on one of the three` is an ENDING.
rem            `the section 4 judge could not be read` is a STOP - nothing was
rem            judged, the loop halted rather than assume, and that is a run
rem            that failed to finish.
rem    stop 1  both are endings, but for different reasons - one is the plan
rem            done, the other is the owner's verdict.
rem
rem  THE BUDGET AND THE WATCHDOG ARE STOPS, and this is the classification most
rem  likely to be argued with, so the reasoning is here rather than in a report
rem  nobody re-reads. The three stops include `money past the budget` - but that
rem  is the ARBITER stopping to ask the owner a question about money. `stop 2`
rem  is the MACHINE'S BRAKE firing, and CPS-DEC-072 says in terms: rejected, a
rem  dollar target as the thing that ends a run; --budget and --minutes remain
rem  the machine's brake AND ARE NOT A TARGET. A night the brake stopped did not
rem  finish. Same for stop 5, which is where --minutes lands.
rem ============================================================
rem  072 task 4. THE REVIEW SHEET - CRITERION 5.3.
rem
rem  065 wrote a sheet that listed the owner-s criteria and nothing else. It
rem  answered WHICH LINES ARE YOURS. It did not answer the question criterion
rem  5.5 will be judged on, which is DID THE ARBITER TRY BEFORE IT STOPPED.
rem
rem  This extends 065 rather than replacing it: the write-once decision is
rem  still made FROM THE DISK, not from a variable, for the reason 065 gave -
rem  stop 10-s counter kept its state in memory in 064 and was walked past.
rem  :ownerwait no longer writes the file; it still decides the path and
rem  reports whether one is already there, and this routine writes it.
rem
rem  WHAT CHANGED BESIDES THE CONTENT: the sheet is now written at ANY of the
rem  five endings, not only at the owner-s-verdict one. A night that ended
rem  because the routes to a criterion ran out is the night the sheet is most
rem  worth having, and before this it wrote none.
rem
rem  IT IS WRITTEN FOR A MAN READING IT ONCE, TIRED. Every criterion of every
rem  step, its state in three words, and under it every attempt with the
rem  approach the arbiter chose, the verdict the launcher computed and the
rem  reason the route is closed where 071 recorded one. A criterion nothing
rem  was tried against says so in those words rather than being left blank,
rem  because a blank reads as an oversight and this is the one document where
rem  a silence would be read as the loop not having bothered.
:reviewsheet
powershell -NoProfile -Command "$p='%ROOT%\PHASE_PLAN.md'; $o='%ROOT%\PHASE_OUTCOME.md'; if(-not (Test-Path -LiteralPath $p)){ exit }; $plan=@(Get-Content -LiteralPath $p -Encoding UTF8); $sheet=''; foreach($raw in $plan){ $ln=([string]$raw).TrimStart([char]0xFEFF); if($ln -cmatch '^REVIEW_SHEET:\s*(.+?)\s*$'){ $sheet=$Matches[1] } }; if($sheet -eq ''){ '      the plan asks for no review sheet'; exit }; $sp=Join-Path '%ROOT%' $sheet; if(Test-Path -LiteralPath $sp){ '      review sheet already on disk, not written again: ' + $sheet; exit }; $at=@{}; $cur=$null; if(Test-Path -LiteralPath $o){ $raw=[Text.Encoding]::UTF8.GetString([IO.File]::ReadAllBytes($o)); if(($raw.Length -gt 0) -and ($raw[0] -eq [char]0xFEFF)){ $raw=$raw.Substring(1) }; foreach($ln in [regex]::Split($raw, [char]13+[string][char]10+'|'+[string][char]10+'|'+[string][char]13)){ if($ln -match '^ATTEMPT: *([0-9]+\.[0-9]+) *\| *(.*)$'){ $k=$Matches[1]; $f=$Matches[2] -split '\|', 4; if($f.Count -lt 4){ continue }; if(-not $at.ContainsKey($k)){ $at[$k]=@() }; $at[$k] += @{ who=$f[0].Trim(); verdict=$f[1].Trim(); fate=$f[2].Trim(); approach=$f[3].Trim(); reason='' }; $cur=$k; continue }; if($ln -match '^REASON: *([0-9]+\.[0-9]+) *\| *[^|]*\| *(.*)$'){ $k=$Matches[1]; if($at.ContainsKey($k) -and ($at[$k].Count -gt 0)){ $at[$k][$at[$k].Count-1].reason=$Matches[2].Trim() } } } }; $c=@(); $c+='# Review sheet'; $c+=''; $c+=('Written by run-phase.bat at ' + (Get-Date).ToString('yyyy-MM-dd HH:mm') + '. The run ENDED - ' + '%LEDKIND%' + '.'); $c+=''; $c+='**This is here to answer one question: did the arbiter try before it stopped?**'; $c+='Every criterion of the phase is below, with its state and every approach'; $c+='recorded against it. Nothing here is a summary - the approaches are the ones'; $c+='the launcher wrote as each unit ran.'; $c+=''; $c+='Where a line is yours to judge it is marked, and turning it to - [x] in'; $c+='PHASE_PLAN.md is yours alone. Nothing in the loop does that.'; $c+=''; $c+='---'; $c+=''; $met=0; $unmet=0; $own=0; $tried=0; $step=0; foreach($raw in $plan){ $ln=([string]$raw).TrimStart([char]0xFEFF); if($ln -cmatch '^STEP: *([0-9]+) *\| *(.*)$'){ $c+=('## Step ' + $Matches[1] + ' - ' + $Matches[2]); $c+=''; continue }; if($ln -match '^\s*-\s\[( |x|X)\]\s+([0-9]+\.[0-9]+)\s+(.*)$'){ $mk=$Matches[1]; $cid=$Matches[2]; $txt=$Matches[3]; $isown=($txt -match '(?i)\*owner.s verdict\*\s*$'); $txt=($txt -replace '(?i)\s*\*owner.s verdict\*\s*$',''); $state='NOT MET'; if($mk -ne ' '){ $state='met'; $met++ } elseif($isown){ $state='YOURS TO JUDGE'; $own++ } else { $unmet++ }; $c+=('- ' + $cid + ' ' + $txt + '  -- ' + $state); $a=@(); if($at.ContainsKey($cid)){ $a=$at[$cid] }; if($a.Count -eq 0){ $c+='    nothing was attempted against this criterion' } else { $tried++; $c+=('    ' + $a.Count + ' attempt(s):'); $i=0; foreach($x in $a){ $i++; $c+=('    ' + $i + '. ' + $x.who + ' - ' + $x.verdict + ', ' + $x.fate); $c+=('       approach : ' + $x.approach); if($x.reason -ne ''){ $c+=('       closed   : ' + $x.reason) } else { $c+='       closed   : no reason recorded against this attempt' } } }; $c+='' } }; $c+='---'; $c+=''; $c+=('**' + $met + ' met, ' + $unmet + ' not met, ' + $own + ' yours to judge. ' + $tried + ' criteria were attempted at all.**'); $dir=Split-Path -Parent $sp; if($dir -and -not (Test-Path -LiteralPath $dir)){ New-Item -ItemType Directory -Force -Path $dir | Out-Null }; [IO.File]::WriteAllText($sp, (($c -join ([char]13 + [string][char]10)) + [char]13 + [string][char]10), (New-Object Text.UTF8Encoding($false))); '      review sheet written: ' + $sheet"
goto :eof

rem ============================================================
rem  072 task 4. THE ATTEMPT COUNT, BEFORE THE ARBITER AUTHORS - CRITERION 5.4.
rem
rem  ONE LINE, and the only console line about attempts that is printed BEFORE
rem  a unit runs rather than after it. :record already prints one afterwards.
rem
rem  WHICH CRITERION, and this needed deciding because at this moment the loop
rem  does not yet know: the arbiter has not authored, so nothing has named a
rem  criterion for this iteration. Author-s, overrulable. In order:
rem    1  the criterion a redirect pins this iteration to, if one is in force -
rem       070 wrote it to redirect.txt and it is the nearest thing to an
rem       instruction the loop has at this point
rem    2  otherwise the criterion the LAST unit ran against, which :lastadvances
rem       has already read out of the record
rem    3  otherwise it says plainly that none is in hand, rather than printing a
rem       count of nothing and letting it read as zero attempts at a criterion
:attemptline
set "ATL_CRIT="
if defined LA_CRIT1 set "ATL_CRIT=%LA_CRIT1%"
if exist "%WORK%edirect.txt" for /f "usebackq tokens=2 delims=: " %%C in (`findstr /b /c:"CRITERIA:" "%WORK%edirect.txt"`) do set "ATL_CRIT=%%C"
powershell -NoProfile -Command "$c='%ATL_CRIT%'.Trim(); if($c -eq ''){ '      attempts   : no criterion is in hand yet - the arbiter chooses one from the plan'; exit }; $o='%ROOT%\PHASE_OUTCOME.md'; $n=0; $no=0; if(Test-Path -LiteralPath $o){ foreach($ln in (Get-Content -LiteralPath $o)){ if($ln -match ('^ATTEMPT: *' + [regex]::Escape($c) + ' *\|')){ $n++; if($ln -match '\| *no *\|'){ $no++ } } } }; '      attempts   : criterion ' + $c + ' has ' + $n + ' attempt(s) on the record, ' + $no + ' of them failed'"
goto :eof

rem ============================================================
rem  073 task 2. RECONCILE THE OUTCOME HEADER AGAINST THE PLAN - FORWARD ONLY.
rem
rem  The owner's ruling of 2026-09-20. Unit 069 appended step 6 to
rem  PHASE_PLAN.md and no path in this loop ever wrote a step line into
rem  PHASE_OUTCOME.md's header, so the record and the plan disagreed about how
rem  many steps the phase has.
rem
rem  WHAT THAT ACTUALLY COSTS, measured by task 1 rather than assumed. The
rem  owner's-verdict halt was NOT blind to step 6 - :ownerwait takes its step
rem  list from the PLAN and defaults a missing state to 'none'. What breaks is
rem  outcome-append.bat: its header-init has a branch for a step it cannot find
rem  beside steps it can, and that branch writes
rem
rem      STEP: 6 | in progress | (described by the plan)
rem
rem  - a PLACEHOLDER where the title belongs, permanently, in an append-only
rem  record. Reproduced on copies of the real files. This routine gets there
rem  first and writes the plan's own words.
rem
rem  WHY NOT outcome-append.bat --open, which the instruction prefers: it
rem  REFUSES A FILE THAT ALREADY EXISTS, at exit 7, because a record is never
rem  replaced. Every root this will ever run in has a record by its second
rem  unit. It cannot be used and this says so rather than leaving it unasked.
rem
rem  IT SPLICES BYTES, IT DOES NOT REWRITE LINES. The file is read with
rem  ReadAllBytes, the insertion point is the END OF THE LAST STEP LINE, and
rem  everything before and after that offset is carried through untouched -
rem  so no existing line can be altered, renumbered, reordered or re-encoded,
rem  whatever it contains. The BOM is detected and written back as found.
rem  Ruling 3, and it is a stronger guarantee than 'the code does not mean to'.
rem
rem  A STEP THE HEADER HAS AND THE PLAN DOES NOT IS REPORTED AND LEFT ALONE.
rem  What it would mean: either the plan was amended to drop a step - and its
rem  attempt history still belongs to the phase - or this run is pointed at
rem  the wrong plan. Neither is repaired by deleting a line.
:reconcile
powershell -NoProfile -Command "$o='%ROOT%\PHASE_OUTCOME.md'; $p='%ROOT%\PHASE_PLAN.md'; if(-not (Test-Path -LiteralPath $o)){ '      no PHASE_OUTCOME.md - nothing to reconcile'; exit }; if(-not (Test-Path -LiteralPath $p)){ '      no PHASE_PLAN.md - nothing to reconcile against'; exit }; $plan=@(); foreach($ln in (Get-Content -LiteralPath $p -Encoding UTF8)){ $t=([string]$ln).TrimStart([char]0xFEFF); if($t -cmatch '^STEP: *([0-9]+) *\| *(.*)$'){ $plan += @{ n=$Matches[1]; what=$Matches[2].Trim() } } }; if($plan.Count -eq 0){ '      the plan names no steps - nothing to reconcile'; exit }; $bytes=[IO.File]::ReadAllBytes($o); $bom=($bytes.Length -ge 3 -and $bytes[0] -eq 239 -and $bytes[1] -eq 187 -and $bytes[2] -eq 191); $text=[Text.Encoding]::UTF8.GetString($bytes); if(($text.Length -gt 0) -and ($text[0] -eq [char]0xFEFF)){ $text=$text.Substring(1) }; $head=$text; $cut=$text.Length; $m=[regex]::Match($text, '(?m)^-{3,}\s*$'); if($m.Success){ $cut=$m.Index; $head=$text.Substring(0,$cut) }; $have=@{}; $last=-1; $lastEnd=-1; foreach($x in [regex]::Matches($head, '(?m)^STEP: *([0-9]+) *\|.*$')){ $have[$x.Groups[1].Value]=$true; $last=$x.Index; $lastEnd=$x.Index + $x.Length }; if($last -lt 0){ '      FINDING: the header names no steps at all - nothing was appended, because'; '      there is no step line to append after and this will not invent a header'; exit }; $add=@(); foreach($st in $plan){ if(-not $have.ContainsKey($st.n)){ $add += $st } }; $extra=@(); foreach($k in $have.Keys){ $in=$false; foreach($st in $plan){ if($st.n -eq $k){ $in=$true } }; if(-not $in){ $extra += $k } }; foreach($k in ($extra | Sort-Object)){ '      FINDING: the header carries step ' + $k + ' and the plan does not.'; '        LEFT ALONE. Append-only: a step removed would take its attempt'; '        history with it, and the plan may have been amended by hand.' }; if($add.Count -eq 0){ '      header already matches the plan - ' + $plan.Count + ' step(s), nothing appended'; exit }; $nl=[string][char]13 + [string][char]10; $at=$text.IndexOf([char]10, $last); if($at -lt 0){ $at=$text.Length } else { $at=$at + 1 }; $ins=''; foreach($st in $add){ $ins += 'STEP: ' + $st.n + ' | not started | ' + $st.what + $nl }; $out=$text.Substring(0,$at) + $ins + $text.Substring($at); $enc=New-Object Text.UTF8Encoding($bom); [IO.File]::WriteAllText($o, $out, $enc); foreach($st in $add){ '      appended: STEP: ' + $st.n + ' | not started | ' + $st.what }; '      the header now carries every step the plan does'"
goto :eof

rem ============================================================
rem  The CARD's step header, reconciled against the plan. Once,
rem  from :lockfree, beside the record's. The owner's ruling of
rem  2026-09-20: the card keeps moving, and a card that cannot be
rem  written never ends a night.
rem ============================================================
rem  WHAT IT IS FOR. :phasesteps copies step states from
rem  PHASE_OUTCOME.md into PHASE_STATUS.md and REFUSES where the
rem  two headers do not name the same steps - correctly, because
rem  one of them is wrong and a launcher cannot know which. Unit
rem  073 taught the record to gain the steps its plan carries, so
rem  from that unit on the record could move and the card could
rem  not. This brings the card up to the same plan, so the two
rem  agree and :phasesteps writes as it always did.
rem
rem  THE PLAN IS THE AUTHORITY, NOT THE RECORD. It makes the same
rem  comparison :reconcile makes, against the same file, so the
rem  card is never derived from the record's header - which would
rem  make a record that failed to reconcile silently freeze the
rem  card again, one layer further down.
rem
rem  IT APPENDS AND DOES NOTHING ELSE. Nothing is removed,
rem  renumbered, reordered or restated; a step the card carries
rem  and the plan does not is REPORTED AND LEFT WHERE IT IS, for
rem  the reason :reconcile gives - the plan may have been amended
rem  by hand, and neither case is repaired by deleting a line. A
rem  new step therefore lands AFTER it and reads out of numeric
rem  order, which is what append-only costs and is deliberate.
rem
rem  IT SPLICES BYTES AND WRITES BYTES. ReadAllBytes, an insertion
rem  point at the END OF THE LAST STEP LINE'S TERMINATOR, and
rem  WriteAllBytes - so every byte outside the insertion is carried
rem  through untouched and NO ENCODER RUNS OVER THE FILE AT ALL.
rem  That is what keeps a BOM out of a file that has none: not a
rem  flag passed correctly, but nothing that could write one. The
rem  search runs over a latin-1 view of the bytes, where one char
rem  is one byte, so a char offset IS a byte offset even though the
rem  file's prose carries multi-byte characters.
rem
rem  THE NEW LINE'S TERMINATOR IS THE ONE ENDING THE LAST STEP
rem  LINE - measured at the insertion point rather than assumed.
rem  This file is LF-only here; a card written CRLF elsewhere gains
rem  a CRLF line and no bare LF. Unit 073's second defect was a
rem  splice landing INSIDE a CRLF terminator, and this cannot: the
rem  offset is past the terminator, never within it.
rem
rem  IT REFUSES TO INVENT A HEADER. Where the card's leading run of
rem  KEY: value lines carries no STEP: line at all there is nothing
rem  to append after, and appending below the terminator would put
rem  this format's own keys beneath the rule - which parsePhaseStatus
rem  reads as strandedNames and returns the WHOLE FILE NOT READABLE,
rem  taking the phase region off the card. It says so and stops.
rem
rem  EVERY FAILURE PRINTS AND CONTINUES. There is no exit code here
rem  and nothing downstream reads one.
:reconcilecard
powershell -NoProfile -Command "try { $p='%ROOT%\PHASE_STATUS.md'; $pl='%ROOT%\PHASE_PLAN.md'; if(-not (Test-Path -LiteralPath $p)){ '      CARD NOT RECONCILED: no PHASE_STATUS.md at this root - THE RUN GOES ON'; exit }; if(-not (Test-Path -LiteralPath $pl)){ '      CARD NOT RECONCILED: no PHASE_PLAN.md to reconcile against - THE RUN GOES ON'; exit }; $plan=@(); foreach($ln in (Get-Content -LiteralPath $pl -Encoding UTF8)){ $t=([string]$ln).TrimStart([char]0xFEFF); if($t -cmatch '^STEP: *([0-9]+) *\| *(.*)$'){ $plan += @{ n=$Matches[1]; what=$Matches[2].Trim() } } }; if($plan.Count -eq 0){ '      CARD NOT RECONCILED: the plan names no steps - THE RUN GOES ON'; exit }; $bytes=[IO.File]::ReadAllBytes($p); $view=[Text.Encoding]::GetEncoding(28591).GetString($bytes); $i=0; if($bytes.Length -ge 3 -and $bytes[0] -eq 239 -and $bytes[1] -eq 187 -and $bytes[2] -eq 191){ $i=3 }; $have=@{}; $hord=@(); $lastEnd=-1; $lastNl=''; while($i -lt $view.Length){ $j=$view.IndexOfAny([char[]]@([char]13,[char]10), $i); if($j -lt 0){ $lineEnd=$view.Length; $nl=''; $next=$view.Length } elseif($view[$j] -eq [char]13 -and ($j+1) -lt $view.Length -and $view[$j+1] -eq [char]10){ $lineEnd=$j; $nl=([string][char]13 + [string][char]10); $next=$j+2 } else { $lineEnd=$j; $nl=[string]$view[$j]; $next=$j+1 }; $line=$view.Substring($i, $lineEnd-$i); if($line -notmatch '^[A-Za-z][A-Za-z0-9_]*:'){ break }; if($line -cmatch '^STEP: *([0-9]+) *\|'){ $k=$Matches[1]; if(-not $have.ContainsKey($k)){ $have[$k]=$true; $hord+=$k }; $lastEnd=$next; $lastNl=$nl }; $i=$next }; if($lastEnd -lt 0){ '      CARD NOT RECONCILED: the card header names no steps at all - nothing was appended,'; '      because there is no step line to append after and this will not invent a header'; exit }; $extra=@(); foreach($k in $hord){ $in=$false; foreach($st in $plan){ if($st.n -eq $k){ $in=$true } }; if(-not $in){ $extra+=$k } }; foreach($k in ($extra | Sort-Object)){ '      FINDING: the card carries step ' + $k + ' and the plan does not.'; '        LEFT ALONE. Append-only: the card is not the place to decide a step was'; '        dropped, and the plan may have been amended by hand.' }; $add=@(); foreach($st in $plan){ if(-not $have.ContainsKey($st.n)){ $add+=$st } }; if($add.Count -eq 0){ '      the card already carries every step the plan does - ' + $plan.Count + ' step(s), nothing appended'; exit }; $term=$lastNl; $pre=''; if($term -eq ''){ $term=[string][char]10; $pre=$term }; $ins=$pre; foreach($st in $add){ $ins += 'STEP: ' + $st.n + ' | not started | ' + $st.what + $term }; $ib=[Text.Encoding]::UTF8.GetBytes($ins); $out=New-Object byte[] ($bytes.Length + $ib.Length); [Array]::Copy($bytes, 0, $out, 0, $lastEnd); [Array]::Copy($ib, 0, $out, $lastEnd, $ib.Length); [Array]::Copy($bytes, $lastEnd, $out, $lastEnd + $ib.Length, $bytes.Length - $lastEnd); [IO.File]::WriteAllBytes($p, $out); foreach($st in $add){ '      appended to the card: STEP: ' + $st.n + ' | not started | ' + $st.what }; '      the card now carries every step the plan does' } catch { '      CARD NOT RECONCILED: ' + $_.Exception.Message; '      THE RUN GOES ON - a card that cannot be written never ends a night' }"
goto :eof

:ledgerstop
set "NOWSTAMP="
for /f "usebackq delims=" %%D in (`powershell -NoProfile -Command "(Get-Date).ToString('yyyy-MM-ddTHH:mm')"`) do set "NOWSTAMP=%%D"
call :verdictof
call "%HERE%ledger.bat" "phase" "%NOWSTAMP%" "%NOWSTAMP%" "%LEDVERDICT%" "%LEDANSWER%" "%SPENT%" "%ROOT%" >nul
rem  ONE PLACE BUILDS THE PROSE, after the tests, so %STOPWHY% is expanded on a
rem  plain line and never inside a block.
goto :eof

rem ------------------------------------------------------------
rem  The classification itself, in one place, read from STOPWHY's prefix.
rem  LEDVERDICT is the exit-state column - what the owner reads first.
rem  LEDANSWER is the prose column, which gains a leading phrase saying which
rem  of the three kinds of ending it was, and is otherwise the STOPWHY that
rem  was always there.
:verdictof
rem  FLAT, NEVER A PARENTHESISED BLOCK, and the file already says why at
rem  :arbstop: cmd expands the variables in a ( ... ) body when it PARSES the
rem  if, before deciding whether to run it, so a closing parenthesis inside the
rem  value ends the block early and the words after it are run as commands.
rem  %STOPWHY% carries model prose here - stop 11 embeds %OWNWHY%, stop 6 the
rem  judge-s terminal reason - and unit 045 lost a whole iteration to exactly
rem  that. The first cut of this routine used blocks; it was rewritten flat
rem  after reading the warning eight hundred lines above it.
set "LEDKIND="
set "LEDVERDICT=failure"
if "%STOPWHY:~0,8%"=="ending: " set "LEDKIND=the criterion is exhausted on the record"
if "%STOPWHY:~0,33%"=="stop 1: the phase plan is satisfi" set "LEDKIND=the phase plan is satisfied, every criterion met"
if "%STOPWHY:~0,38%"=="stop 1: the phase is waiting on the ow" set "LEDKIND=nothing is left but the owner-s verdict"
rem  ~0,41 AND NOT ~0,40: the first cut was one character short, so the test
rem  literal carried a trailing space the substring did not, it never matched,
rem  and a night that ended because a ruling was wanted on one of the three was
rem  recorded as a FAILURE. Found by driving this routine with all thirty real
rem  reasons rather than by reading it - which is the reading-back the
rem  instruction asks for, and it found an ENDING dressed as a stop.
if "%STOPWHY:~0,41%"=="stop 3: a ruling is wanted on one of the " set "LEDKIND=a ruling is wanted on one of the three"
if "%STOPWHY:~0,7%"=="stop 4:" set "LEDKIND=the arbiter raised one of the three for the owner"
rem  ONE PLACE BUILDS THE PROSE, after the tests, so %STOPWHY% is expanded on a
rem  plain line and never inside a block.
if defined LEDKIND set "LEDVERDICT=ending"
set "LEDANSWER=STOPPED, AND A STOP IS FAILURE - %STOPWHY%"
if defined LEDKIND set "LEDANSWER=ENDED - %LEDKIND%. %STOPWHY%"
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
if /i "%FIXTURE%"=="promptonly" goto :fxpromptonly
if /i "%FIXTURE%"=="reconcile" goto :fxreconcile
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

rem  069 task 3: the arbiter prompt, composed and nothing else. The same shape
rem  as --fixture record - CALLED, not copied, so what a fixture proves is what
rem  the loop runs. It launches no unit and calls no claude, so the prompt arms
rem  cost seconds instead of the seventy a watched run takes.
rem  074 task 4: THE TWO RECONCILIATIONS, AND NOTHING ELSE. The same shape as
rem  --fixture promptonly and --fixture record - the real routines are CALLED,
rem  not copied, so what a fixture proves is what the loop runs.
rem
rem  WHY THE BYTE PROOF NEEDS IT. In a real iteration :heartbeat writes a
rem  HEARTBEAT: line into the card and :phasesteps rewrites its step states, so
rem  a card compared before and after a full run carries three routines' work
rem  and the splice cannot be isolated in it. Here nothing else touches the
rem  file, so the card AFTER is the card BEFORE plus exactly the insertion -
rem  which is the only condition under which 'remove the insertion and it is
rem  byte-identical' means anything. It also costs seconds rather than the
rem  seventy a watched unit takes.
:fxreconcile
echo.
echo   FIXTURE reconcile: the record's header, then the card's. Nothing else runs.
echo.
echo   PHASE_OUTCOME.md
call :reconcile
echo.
echo   PHASE_STATUS.md
call :reconcilecard
echo.
echo   FIXTURE reconcile: done. Neither reconciliation can set an exit code.
set "RC=0"
goto :end

:fxpromptonly
set "ARBPROMPT=%WORK%\arbiter-prompt.txt"
if exist "%ARBPROMPT%" del /q "%ARBPROMPT%"
call :writearbprompt
echo.
echo   FIXTURE promptonly: the prompt is at %ARBPROMPT%
set "RC=0"
if not exist "%ARBPROMPT%" set "RC=2"
goto :end

:fxrecord
set "ITER=fixture"
set "RUNRC=0"
call :readdecision
rem  064: the record now carries ADVANCED, so the fixture parses ADVANCES as
rem  the loop does. No count is taken before - there is no unit run here -
rem  so a criterion cannot be seen to flip, and a fixture decision block that
rem  names none records not recorded rather than a verdict it did not earn.
call :advparse
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
echo   --seed           iteration 1 executes the WORK_INSTRUCTIONS.md that
echo                    shipped, as written - the arbiter does not author it
echo                    and takes over from iteration 2. The seed still needs
echo                    its ARBITER-DECISION block, ADVANCES included. Without
echo                    --seed, iteration 1 is authored like every other.
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
