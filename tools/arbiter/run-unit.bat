@echo off
rem ============================================================
rem  run-unit.bat  -  one command runs a work instruction, end to
rem                   end, unattended
rem
rem      run-unit.bat <unit> <root> [--tools-file F] [--dry-run]
rem
rem      0  the unit ran, the report validated, the ledger line
rem         was appended
rem      1  THE LOCK IS HELD - nothing was launched
rem      2  usage, bad root, or claude is not on PATH
rem      3  the run itself failed - is_error true, or a non-zero
rem         exit from claude
rem      4  A DENIAL THE UNIT COULD NOT WORK AROUND - it was
rem         refused AND could not complete because of it. The
rem         denied calls are listed, and written in full to
rem         <root>\.run-unit\denials.txt. Redefined by the owner's
rem         ruling of 2026-08-29; it was "any non-empty
rem         permission_denials", which failed units that had
rem         succeeded. A unit that was refused and completed
rem         anyway now exits 0 with the count in its ledger line.
rem      5  the report failed validate-output.bat
rem      6  the ledger line could not be appended
rem      7  THE ROOT IS NOT A GIT REPOSITORY - nothing was
rem         launched and no lock was taken
rem
rem  THE LOCK IS RELEASED ON EVERY PATH OUT, including every
rem  failure above. CLAUDE_CODE.md section 5's "one writer at a
rem  time", made mechanical.
rem
rem  ---------------------------------------------------------------
rem  PERMISSION POSTURE, ruled by the owner 2026-08-28.
rem
rem  --restricted with an explicit list. The guard does not depend
rem  on the model's judgment.
rem
rem  REJECTED: auto mode, Claude Code's default, in which the model
rem  assesses each tool call for risk. It may well be right and
rem  nothing here has measured it; adopting an unmeasured guard for
rem  unattended runs is the mistake this project has spent two
rem  evenings avoiding.
rem
rem  REJECTED: --restricted with Bash allowed unscoped. It keeps the
rem  convenient half and drops the half that matters.
rem
rem  NOT USED, in any spelling: --dangerously-skip-permissions,
rem  --permission-mode bypassPermissions, auto mode.
rem  ---------------------------------------------------------------
rem
rem  TWO TRAPS, both measured 2026-08-28 and both worth the ink.
rem
rem  1. A NON-EMPTY permission_denials IS NOT, BY ITSELF, A FAILURE.
rem     Superseded by the owner's ruling of 2026-08-29. It used to
rem     read that a refused unit did not do the work it says it did,
rem     and that was measured false: 045's unit was denied eight
rem     times, worked around every one, and committed five times
rem     with is_error False. A healthy unit is refused as a matter
rem     of course - it tries a shape the rule does not match, is
rem     refused, and adapts. THE DENIALS ARE STILL NAMED AND STILL
rem     WRITTEN OUT in every case; what is judged is whether the
rem     unit could complete. See the policy block further down.
rem
rem  2. AN EMPTY permission_denials DOES NOT MEAN THE SCOPE FIT. If
rem     a tool is not in --tools at all there is nothing to deny, so
rem     the array stays empty while the work silently does not
rem     happen. So this script PRINTS BOTH LISTS on every run - the
rem     --tools it passed and the --allowedTools it read - and the
rem     two failure shapes stay distinguishable.
rem
rem  THE SCOPE IS A DATA FILE. tools\arbiter\run-unit-tools.txt, one
rem  rule per line. Widening it is an edit to that file, visible in
rem  a diff on its own, not a change to this logic.
rem
rem  THE PROMPT IS BUILT FROM A FILE, never composed inline -
rem  CPS-DEC-021, four silent corruptions in three runs, three of
rem  them landing in files this repository treats as the record.
rem
rem  NO WATCHDOG HERE. claude -p blocks until the run ends, so
rem  there is nothing to poll from inside this script. Watchdogging
rem  a run means a second process and that is run-unit-watched.bat.
rem
rem  Generated 2026-08-28 for: work instructions 039 task 4
rem ============================================================

setlocal

rem  CAPTURED BEFORE ANY shift. `shift` moves %0 as well as the
rem  numbered arguments, so after the two shifts below %~dp0 resolves
rem  to the CALLER's directory and every sibling script this launcher
rem  calls goes missing. Measured: the first dry run reported
rem  '"C:\Source\HamLet\lock.bat" is not recognized',
rem  the tools\grok\ having fallen off the path, and the launcher then
rem  read that failure as "the lock is held" and refused. A guard that
rem  cannot find the guard reports the wrong refusal.
set "HERE=%~dp0"

set "RC=0"
set "TOOK="
set "UNIT=%~1"
set "ROOT=%~2"
set "TOOLSFILE=%HERE%run-unit-tools.txt"
set "DRYRUN="

if "%UNIT%"=="" goto :usage
if "%ROOT%"=="" goto :usage
shift
shift

:parse
if "%~1"=="" goto :parsed
if /i "%~1"=="--tools-file" set "TOOLSFILE=%~2" & shift & shift & goto :parse
if /i "%~1"=="--dry-run"    set "DRYRUN=1" & shift & goto :parse
if /i "%~1"=="--phase-step"     set "PHASE_STEP=%~2" & shift & shift & goto :parse
if /i "%~1"=="--phase-state"    set "PHASE_STATE=%~2" & shift & shift & goto :parse
if /i "%~1"=="--phase-approach" set "PHASE_APPROACH=%~2" & shift & shift & goto :parse
if /i "%~1"=="--phase-hit"      set "PHASE_HIT=%~2" & shift & shift & goto :parse
if /i "%~1"=="--phase-move"     set "PHASE_MOVE=%~2" & shift & shift & goto :parse
if /i "%~1"=="--phase-why"      set "PHASE_WHY=%~2" & shift & shift & goto :parse
if /i "%~1"=="--phase-decided"  set "PHASE_DECIDED=%~2" & shift & shift & goto :parse
if /i "%~1"=="--phase-licence"  set "PHASE_LICENCE=%~2" & shift & shift & goto :parse
if /i "%~1"=="--phase-did"      set "PHASE_DID=%~2" & shift & shift & goto :parse
echo ERROR: unexpected argument: %~1
goto :usage

:parsed
if "%ROOT:~-1%"=="\" set "ROOT=%ROOT:~0,-1%"
if not exist "%ROOT%\" (
  echo ERROR: repo root not found: %ROOT%
  set "RC=2"
  goto :end
)
if not exist "%TOOLSFILE%" (
  echo ERROR: no permission scope file: %TOOLSFILE%
  echo Refusing to launch with no scope. An unscoped unattended run is
  echo the blast radius the restriction exists for.
  set "RC=2"
  goto :end
)

where claude >nul 2>&1
if errorlevel 1 (
  echo ERROR: claude is not on PATH.
  set "RC=2"
  goto :end
)

rem --- the root must be a git repository, checked BEFORE the lock -
rem  A WORK INSTRUCTION ALWAYS TARGETS A REPOSITORY. CLAUDE_CODE.md
rem  section 4.9 requires every session to commit and push, so an
rem  instruction aimed at a root with no repository is a MALFORMED
rem  RUN rather than a case to accommodate. Ruled by the owner
rem  2026-08-28.
rem
rem  Unit 039 measured what leaving it costs: the launched session
rem  hit the contradiction between the prompt's commit-and-push
rem  cadence and a fixture that was not a repository, spent turns
rem  resolving it, and resolved in favour of the work instruction.
rem  A session resolving the other way tries to commit where there
rem  is no repository.
rem
rem  REJECTED by the owner: having the launcher drop the
rem  commit-and-push line when there is no repository. It makes the
rem  prompt something the launcher COMPOSES rather than a block it
rem  copies. Today it is one conditional line; the argument for the
rem  second is identical, and section 6's "nothing but the prompt"
rem  stops being true. That is the drift CPS-DEC-021 and section 6
rem  both guard against, arriving as a reasonable convenience.
rem
rem  BEFORE THE LOCK, deliberately: refusing after taking it leaves
rem  a lock behind for someone to clean up, and a lock left behind
rem  blocks the queue this whole phase exists to enable.
rem
rem  git rev-parse ran here already, for HEAD. It reported failure
rem  by leaving HEADBEFORE unset and the launcher carried on
rem  regardless - the detection existed and the refusal did not.
set "ISREPO="
pushd "%ROOT%"
git rev-parse --is-inside-work-tree >nul 2>&1
if not errorlevel 1 set "ISREPO=1"
popd
if not defined ISREPO (
  echo.
  echo REFUSED: %ROOT% is not a git repository.
  echo.
  echo A work instruction always targets one. CLAUDE_CODE.md section 4.9
  echo requires every session to commit and push, so a unit aimed at a
  echo root with no repository is a malformed run rather than a case to
  echo accommodate - the prompt would tell the session to push and the
  echo tree would have nowhere to push to.
  echo.
  echo NOTHING WAS LAUNCHED and NO LOCK WAS TAKEN.
  echo If this is a scratch fixture, run: git init "%ROOT%"
  set "RC=7"
  goto :end
)

set "WORK=%ROOT%\.run-unit"
if not exist "%WORK%" mkdir "%WORK%"
set "JSON=%WORK%\last-run.json"
set "PROMPT=%WORK%\prompt.txt"
set "ALLOWED=%WORK%\allowed.txt"
set "DENIED=%WORK%\denials.txt"

rem  THE BUILT-IN TOOLS THIS SESSION GETS AT ALL. Not the scope -
rem  see run-unit-tools.txt's header for why the two are different.
set "TOOLS=Read,Write,Edit,Bash"

echo.
echo ============================================================
echo  run-unit
echo    unit  : %UNIT%
echo    root  : %ROOT%
echo    scope : %TOOLSFILE%
echo ============================================================

rem --- the scope, read from the data file ------------------------
set "ALLOWLIST="
if exist "%ALLOWED%" del /q "%ALLOWED%"
for /f "usebackq tokens=* delims=" %%R in (`powershell -NoProfile -Command "Get-Content -LiteralPath '%TOOLSFILE%' | Where-Object { $_.Trim() -ne '' -and $_ -notmatch '^\s*rem\b' } | ForEach-Object { $_.Trim() }"`) do (
  >>"%ALLOWED%" echo %%R
)
if not exist "%ALLOWED%" (
  echo ERROR: the scope file contained no rules: %TOOLSFILE%
  set "RC=2"
  goto :end
)

echo.
echo  --tools passed ^(which built-ins exist at all^):
echo    %TOOLS%
echo.
echo  --allowedTools read from the scope file ^(what they may do^):
for /f "usebackq tokens=* delims=" %%R in ("%ALLOWED%") do echo    %%R
echo.
echo  BOTH lists are printed because an empty permission_denials
echo  proves nothing on its own: a tool absent from --tools has
echo  nothing to deny.
echo.

rem --- the lock, before anything is launched ---------------------
echo Taking the session lock...
call "%HERE%lock.bat" take "%ROOT%"
if not errorlevel 1 goto :gotlock
echo.
echo REFUSED: the session lock is held. NOTHING WAS LAUNCHED.
echo A second writer in one tree is the failure this lock exists to stop.
set "RC=1"
goto :end

:gotlock
set "TOOK=1"

rem --- HEAD before the run, for the return package and the ledger -
set "HEADBEFORE="
pushd "%ROOT%"
for /f "usebackq delims=" %%H in (`git rev-parse --short HEAD 2^>nul`) do set "HEADBEFORE=%%H"
popd
rem  This arm should no longer fire: the repository check above
rem  refused a non-repository before reaching here. It is kept
rem  because rev-parse can fail for other reasons - a corrupt
rem  .git, a permission lapse - and "unknown" is the honest word
rem  for a reading that was not taken.
if not defined HEADBEFORE set "HEADBEFORE=unknown"
echo   HEAD before the run : %HEADBEFORE%

rem --- the project name, read from the target root's own card ----
rem  The gate line is the one thing standing between a unit and the
rem  wrong repository, so it is READ from the tree the unit will run
rem  in - never passed in, never assumed from the folder name. A
rem  window that labels itself from its folder name is the specific
rem  failure this whole repository exists to prevent.
set "UNIT_PROJECT="
if exist "%ROOT%\PROJECT_CARD.md" (
  for /f "usebackq tokens=1,* delims=:" %%A in (`findstr /b /c:"PROJECT:" "%ROOT%\PROJECT_CARD.md"`) do set "UNIT_PROJECT=%%B"
)
if defined UNIT_PROJECT call :trimproj
if not defined UNIT_PROJECT (
  echo ERROR: no PROJECT field on %ROOT%\PROJECT_CARD.md
  echo Refusing to build a prompt with no gate line. A unit with no
  echo project named is a unit that cannot verify it is in the right tree.
  set "RC=2"
  goto :end
)
echo   project ^(from card^) : %UNIT_PROJECT%

rem --- the prompt, built from a file -----------------------------
call :writeprompt
if not exist "%PROMPT%" (
  echo ERROR: could not write the prompt file.
  set "RC=2"
  goto :end
)
echo   prompt written      : %PROMPT%

set "STARTED="
for /f "usebackq delims=" %%D in (`powershell -NoProfile -Command "(Get-Date).ToString('yyyy-MM-ddTHH:mm')"`) do set "STARTED=%%D"

if defined DRYRUN (
  echo.
  echo   DRY RUN: everything above was resolved and nothing was launched.
  echo   The prompt, both tool lists and the lock were all exercised.
  set "RC=0"
  goto :end
)

rem --- launch ----------------------------------------------------
echo.
echo Launching claude -p, restricted...
rem  ---------------------------------------------------------------
rem  EAP IS LIFTED ACROSS THE NATIVE CALL, AND THAT IS THE WHOLE
rem  POINT OF THIS LINE. MEASURED 2026-08-29, claude 2.1.251.
rem
rem  Windows PowerShell 5.1 wraps EVERY stderr line from a native
rem  command in a NativeCommandError record when the stream is
rem  redirected. Under $ErrorActionPreference='Stop' that record is
rem  TERMINATING: the pipeline dies before Set-Content runs, no JSON
rem  is written, PowerShell exits 1, and this script reports "no JSON
rem  was produced" and returns 3.
rem
rem  SO THE RUN WAS RECORDED AS FAILED WHILE claude HAD EXITED 0 AND
rem  DONE THE WORK. The trigger is any line on stderr at all - in
rem  046's chain it was "Warning: no stdin data received in 3s",
rem  which fires intermittently when this launcher is started in the
rem  background by run-unit-watched.bat. Both units of that chain
rem  were thrown away this way.
rem
rem  Three probes, each predicted before it ran, against a stand-in
rem  writing one stderr line and valid JSON and exiting 0:
rem      EAP=Stop + 2>&1     exit 1, NOTHING written
rem      no EAP=Stop         exit 0, written
rem      EAP=Stop, no 2>&1   exit 0, written
rem
rem  EAP=Stop STAYS FOR THE TWO Get-Content CALLS above - a missing
rem  scope file or prompt must still be fatal - and is lifted only
rem  across the native call. The merge stays too, so whatever claude
rem  said survives in the JSON file even when it said nothing else;
rem  the parse below starts at the first brace and steps over it.
rem
rem  $LASTEXITCODE IS CAPTURED BEFORE Pop-Location. Pop-Location is
rem  a cmdlet and does not clear it, but it is read after another
rem  statement either way, and a launcher whose exit code depends on
rem  what a later cmdlet leaves behind is a launcher nobody can
rem  reason about.
rem  ---------------------------------------------------------------
powershell -NoProfile -Command "$ErrorActionPreference='Stop'; $allow = Get-Content -LiteralPath '%ALLOWED%'; $p = Get-Content -LiteralPath '%PROMPT%' -Raw; $a = @('-p', $p, '--output-format', 'json', '--restricted', '--tools', '%TOOLS%'); foreach($r in $allow){ $a += '--allowedTools'; $a += $r }; Push-Location '%ROOT%'; $ErrorActionPreference='Continue'; & claude @a 2>&1 | Set-Content -LiteralPath '%JSON%' -Encoding utf8; $rc=$LASTEXITCODE; Pop-Location; exit $rc"
set "CLAUDERC=%ERRORLEVEL%"

set "ENDED="
for /f "usebackq delims=" %%D in (`powershell -NoProfile -Command "(Get-Date).ToString('yyyy-MM-ddTHH:mm')"`) do set "ENDED=%%D"

echo   claude exit         : %CLAUDERC%
echo   json                : %JSON%

if not exist "%JSON%" (
  echo.
  echo ERROR: no JSON was produced. The run cannot be judged.
  set "RC=3"
  goto :ledgerfail
)

rem --- read the JSON ---------------------------------------------
set "ISERR="
set "TERM="
set "TURNS="
set "COST="
set "NDENIED=0"
rem  THE DENIAL TEXT NEVER ENTERS A cmd VARIABLE. A denied call is
rem  the exact command string the unit tried, so it is full of the
rem  characters cmd acts on - quotes, ampersands, redirects, pipes.
rem  The first cut read the joined text into a variable and the
rem  launcher died at exit 255 with '"C:/Users/..." was unexpected
rem  at this time', LOSING THE EXIT 4 IT HAD CORRECTLY DECIDED ON.
rem  So PowerShell writes the detail to a FILE and hands back only a
rem  COUNT, which is a number and cannot be executed. Measured on
rem  2026-08-28 by removing Write from the scope and running it.
rem  NOT NAMED TERM. TERM is a standard environment variable and
rem  this shell inherits it as xterm-256color, so where the JSON
rem  parse produced nothing the check read the TERMINAL TYPE and
rem  called it the terminal_reason. Measured 2026-08-29 in 046's own
rem  chain, whose ledger lines read "terminal=xterm-256color".
rem  A judgment resting on an inherited variable is not a judgment.
rem  THE JSON MAY NOT START AT BYTE ZERO. 2>&1 captures stderr into
rem  the same file, and claude prints "Warning: no stdin data
rem  received in 3s" whenever stdin is not ready - which happens
rem  intermittently when the launcher is started in the background.
rem  ConvertFrom-Json then fails, ISERR reads unreadable, and a run
rem  that may have gone perfectly well is judged exit 3. MEASURED
rem  2026-08-29: it cost a paid unit run in 046's chain. So the
rem  parse starts at the first { and ignores whatever preceded it.
rem  run-phase.bat carries the same guard in both of its parses.
for /f "usebackq tokens=1,* delims==" %%A in (`powershell -NoProfile -Command "try{ $raw = Get-Content -LiteralPath '%JSON%' -Raw; $k = $raw.IndexOf([char]123); if($k -lt 0){ throw }; $j = $raw.Substring($k) | ConvertFrom-Json } catch { 'ISERR=unreadable'; exit }; 'ISERR=' + $j.is_error; 'TERMREASON=' + $j.terminal_reason; 'TURNS=' + $j.num_turns; 'COST=' + $j.total_cost_usd; $d = @($j.permission_denials); 'NDENIED=' + $d.Count; if($d.Count){ $d | ForEach-Object { $n = $_.tool_name; $i = ($_.tool_input | ConvertTo-Json -Compress); if($i.Length -gt 240){ $i = $i.Substring(0,240) + ' ...(truncated)' }; '   ' + $n + '  ' + $i } | Set-Content -LiteralPath '%DENIED%' -Encoding utf8 } else { if(Test-Path -LiteralPath '%DENIED%'){ Remove-Item -LiteralPath '%DENIED%' } }"`) do set "%%A=%%B"

echo.
echo   is_error            : %ISERR%
echo   terminal_reason     : %TERMREASON%
echo   num_turns           : %TURNS%
echo   total_cost_usd      : %COST%
echo   denied calls        : %NDENIED%
echo.

if "%ISERR%"=="unreadable" (
  echo   ERROR: the JSON could not be parsed. The run cannot be judged.
  set "RC=3"
  goto :ledgerfail
)

rem ---------------------------------------------------------------
rem  ONE DENIAL POLICY, ruled by the owner 2026-08-29.
rem
rem  A DENIAL IS FATAL ONLY WHERE THE UNIT COULD NOT COMPLETE
rem  BECAUSE OF IT. run-phase.bat makes exactly this judgment; this
rem  is the same policy in the other caller, so the two cannot
rem  disagree about the same run.
rem
rem  WHAT CHANGED, AND WHY THE ORDER MOVED. Until 046 a non-empty
rem  permission_denials exited 4 here and jumped straight to the
rem  failed ledger line - BEFORE validate-output.bat was reached -
rem  so the ledger read "failed" against units that had passed their
rem  gate, done their work, committed and written a valid report.
rem  045 measured one: eight denials, every one a compound
rem  cd "<root>" and-and ... into a shell already standing in that
rem  root, is_error False, five commits. RUN_LEDGER.md is what the
rem  owner reads instead of watching, and a "failed" he cannot trust
rem  is how a column becomes noise.
rem
rem  THE JUDGMENT NEEDS THE REPORT, SO VALIDATION RUNS FIRST. The
rem  alternative was to judge on is_error and terminal_reason alone,
rem  which are parsed above and were already in hand. Rejected:
rem  run-phase.bat also weighs the report, and a policy that is "the
rem  same" but weighs less evidence is two policies.
rem
rem  THE DENIALS STAY LOUD WHATEVER THE VERDICT. They are printed
rem  here, written in full to .run-unit\denials.txt above, and
rem  counted in the ledger line either way. What changed is the
rem  state word, not the record.
rem
rem  FLAT, WITH LABELS. Not parenthesised blocks - 045 lost an
rem  iteration to a ) inside one.
rem ---------------------------------------------------------------

if /i "%ISERR%"=="True" goto :runfailed
if not "%CLAUDERC%"=="0" goto :runfailed

rem --- validate the report, BEFORE the denials are judged ---------
echo Validating the report...
set "VRC=2"
if not exist "%ROOT%\output.md" goto :novrc
call "%HERE%validate-output.bat" "%ROOT%\output.md" >nul
set "VRC=%ERRORLEVEL%"
:novrc
echo   validate-output     : exit %VRC%

if not "%VRC%"=="0" goto :badreport
if not "%TERMREASON%"=="completed" goto :notcompleted

rem  It completed. Denials, if any, were worked around.
if "%NDENIED%"=="0" goto :ledgerok
echo.
echo   %NDENIED% DENIED CALL^(S^), AND THE UNIT COMPLETED ANYWAY.
echo   is_error %ISERR%, terminal %TERMREASON%, report valid.
echo   NOT FATAL - the owner's ruling of 2026-08-29.
echo.
if exist "%DENIED%" type "%DENIED%"
echo.
set "LEDGERWHY=ran unattended, %TURNS% turns, %NDENIED% denied call(s) worked around, report valid"
goto :ledgerline

:ledgerok
set "LEDGERWHY=ran unattended, %TURNS% turns, no denials, report valid"
goto :ledgerline

:notcompleted
echo   THE RUN DID NOT COMPLETE - terminal_reason is %TERMREASON%.
if "%NDENIED%"=="0" set "RC=3"
if not "%NDENIED%"=="0" set "RC=4"
if not "%NDENIED%"=="0" echo   %NDENIED% denied call^(s^) - the refusal is the likely cause.
if exist "%DENIED%" type "%DENIED%"
goto :ledgerfail

:badreport
echo   THE REPORT IS NOT SHAPED LIKE A REPORT, or is absent. Run
echo   validate-output.bat on it to see which rule failed.
if "%NDENIED%"=="0" set "RC=5"
if not "%NDENIED%"=="0" set "RC=4"
if not "%NDENIED%"=="0" echo   %NDENIED% denied call^(s^) - the refusal is the likely cause.
if exist "%DENIED%" type "%DENIED%"
goto :ledgerfail

:runfailed
if /i "%ISERR%"=="True" echo   THE RUN FAILED - is_error is true.
if not "%CLAUDERC%"=="0" echo   THE RUN FAILED - claude exited %CLAUDERC%.
if "%NDENIED%"=="0" set "RC=3"
if not "%NDENIED%"=="0" set "RC=4"
if not "%NDENIED%"=="0" echo   %NDENIED% denied call^(s^) - the refusal is the likely cause.
goto :ledgerfail

:ledgerline
rem --- the ledger ------------------------------------------------
echo Appending the ledger line...
call "%HERE%ledger.bat" "%UNIT%" "%STARTED%" "%ENDED%" "complete" "%LEDGERWHY%" "%COST%" "%ROOT%"
if errorlevel 1 (
  echo   ERROR: the ledger line was not appended.
  set "RC=6"
  goto :end
)

rem --- the phase record, contributed without being asked ---------
rem  A RECORD THAT DEPENDS ON SOMEBODY REMEMBERING TO WRITE IT IS
rem  NOT A RECORD. 041 task 4. The ledger line says a run happened;
rem  this says what it attempted, which is what PHASE_CONTROL.md
rem  section 3's loop test needs and what an arbiter starting cold
rem  on unit two cannot otherwise know.
rem
rem  WHAT THE LAUNCHER KNOWS AND WHAT IT DOES NOT. It knows the
rem  cost, the turns and whether the report validated - those come
rem  from last-run.json and validate-output.bat. It does NOT know
rem  the approach, what was hit, or the move chosen: those are
rem  judgments made inside the run. So they are passed in by
rem  whoever authored the unit, via --phase-step and its
rem  neighbours, and where they are absent this writes `not
rem  recorded` rather than inventing a sentence. A fabricated
rem  APPROACH is worse than an absent one, because the loop test
rem  reads that field.
if not defined PHASE_STEP goto :nophase
if not defined PHASE_APPROACH set "PHASE_APPROACH=not recorded"
if not defined PHASE_HIT set "PHASE_HIT=not recorded"
if not defined PHASE_MOVE set "PHASE_MOVE=not recorded"
if not defined PHASE_WHY set "PHASE_WHY=not recorded"
if not defined PHASE_DECIDED set "PHASE_DECIDED=none"
if not defined PHASE_LICENCE set "PHASE_LICENCE=none"
if not defined PHASE_STATE set "PHASE_STATE=in progress"
if not defined PHASE_DID set "PHASE_DID=not recorded"
echo Contributing to the phase record...
call "%HERE%outcome-append.bat" "%UNIT%" "%PHASE_STEP%" "%PHASE_STATE%" "%PHASE_APPROACH%" "%PHASE_HIT%" "%PHASE_MOVE%" "%PHASE_WHY%" "%PHASE_DECIDED%" "%PHASE_LICENCE%" "%COST%" "%PHASE_DID%" "%ROOT%\PHASE_OUTCOME.md" >nul
if errorlevel 1 echo   NOTE: the phase record was not written. The run stands; the memory does not.
goto :ranok

:nophase
echo   No --phase-step given, so nothing was written to the phase record.
echo   The ledger line stands on its own. A unit outside a phase has no
echo   step to contribute to, and inventing one would put a fact in the
echo   arbiter's memory that never happened.

:ranok
echo.
echo   THE UNIT RAN END TO END. Report valid, ledger written.
set "RC=0"
goto :end

rem ============================================================
rem  A failed run still gets a ledger line. The ledger is what the
rem  owner reads instead of watching, and a failure that leaves no
rem  line is a run he cannot see happened at all.
:ledgerfail
if not defined STARTED goto :end
if not defined ENDED set "ENDED=%STARTED%"
if not defined COST set "COST=unknown"
call "%HERE%ledger.bat" "%UNIT%" "%STARTED%" "%ENDED%" "failed" "run-unit exit %RC%: %NDENIED% denied call(s), is_error=%ISERR%, terminal=%TERMREASON%" "%COST%" "%ROOT%" >nul
goto :end

rem ============================================================
rem  CLAUDE_CODE.md section 6's block, written to a file rather
rem  than composed inline. CPS-DEC-021.
:trimproj
if "%UNIT_PROJECT:~0,1%"==" " set "UNIT_PROJECT=%UNIT_PROJECT:~1%" & goto :trimproj
goto :eof

rem ============================================================
:writeprompt
>"%PROMPT%" echo PROJECT: %UNIT_PROJECT%
>>"%PROMPT%" echo Execute WORK_INSTRUCTIONS.md.
>>"%PROMPT%" echo.
>>"%PROMPT%" echo Status cadence, for this and every session:
>>"%PROMPT%" echo.
>>"%PROMPT%" echo After each task, before starting the next, update PROJECT_STATUS.md per
>>"%PROMPT%" echo CLAUDE.md - STATE, TASK: n of m, BALL, UPDATED from the clock, and
>>"%PROMPT%" echo NOTE saying what is moving inside the task, not restating the task name.
>>"%PROMPT%" echo.
>>"%PROMPT%" echo Do the same every ten minutes while a task is running.
>>"%PROMPT%" echo.
>>"%PROMPT%" echo Commit and push each task before starting the next.
>>"%PROMPT%" echo.
rem  PHASE_STATUS.md's WORK_INSTRUCTION IS THE EXECUTOR'S AND NOBODY WAS
rem  ASKING FOR IT. Measured 2026-09-01: the field read
rem  `001 - the Ft8Sharp vessel, its licence and its boundary` while the
rem  project was on 210. PHASE_CONTROL.md section 4 assigns the field to the
rem  executor, and this prompt - the only thing that ever asks an executor
rem  for anything - did not mention PHASE_STATUS.md at all. No script wrote
rem  it either, so it still held the value typed by hand when the file was
rem  created. Fixing the file by hand would have gone stale again on the
rem  next instruction; this is the writer.
rem
rem  THE SCOPE IS NAMED IN THE PROMPT because the launcher writes the same
rem  file on the beat and at :phasesteps. Two writers with overlapping
rem  scopes corrupt a file between them, so the executor is told exactly
rem  which line is its own.
>>"%PROMPT%" echo Also update PHASE_STATUS.md at the repository root: set its
>>"%PROMPT%" echo WORK_INSTRUCTION: line to the instruction you are executing, taken from
>>"%PROMPT%" echo the `# Work instruction ^<n^> - ^<title^>` heading in WORK_INSTRUCTIONS.md,
>>"%PROMPT%" echo in the form `^<n^> - ^<title^>`.
>>"%PROMPT%" echo.
>>"%PROMPT%" echo That line and PHASE:, PHASE_SET: and DESCRIPTION: are yours. HEARTBEAT:,
>>"%PROMPT%" echo CURRENT_STEP: and the STEP: lines belong to the launcher - do not write
>>"%PROMPT%" echo them, and do not reformat the file. Leave everything below the --- alone.
>>"%PROMPT%" echo.
rem  THE ORDERING LINE IS IN THE DELIVERY, NOT IN CLAUDE_CODE.md.
rem  050's ruling and section 7's argument: a standard read at minute
rem  zero is not what a session an hour into a run is looking at, and
rem  the prompt is the only copy still in front of it at the end.
rem  The panel thread owns CLAUDE_CODE.md; this is a prototype for
rem  them, carried where it works rather than edited into their file.
>>"%PROMPT%" echo Before you stop, for any reason at all, write output.md per
>>"%PROMPT%" echo CLAUDE_CODE.md section 8, opening with the ordering block: A the phase
>>"%PROMPT%" echo goal, B this step and its exit criteria, C what this report adds and
>>"%PROMPT%" echo whether any of it bears on A or B. Complete, blocked, failed or stopped early are
>>"%PROMPT%" echo all reported the same way and there is no exit that leaves the file
>>"%PROMPT%" echo unwritten. If you are stopping with tasks remaining, name them and
>>"%PROMPT%" echo say why in section 1.
>>"%PROMPT%" echo.
rem  THE FOUR HEADINGS, VERBATIM, IN THE PROMPT. Measured 2026-09-01
rem  in Hamlet: six consecutive units did their work, wrote valid
rem  prose and were failed at exit 4 because the report carried
rem  fifteen top-level sections of its own naming instead of the four.
rem  The prompt cited section 8 and never quoted it, so a session an
rem  hour into a run wrote the shape that felt right. And the exit 4
rem  message blames the denials, which sent the owner chasing
rem  permissions for six launches while the report was the fault.
>>"%PROMPT%" echo THE REPORT HAS EXACTLY FOUR TOP-LEVEL SECTIONS. These headings,
>>"%PROMPT%" echo spelled and ordered exactly like this, at ## level, and NO OTHERS
>>"%PROMPT%" echo at ## level anywhere in the file:
>>"%PROMPT%" echo.
>>"%PROMPT%" echo   ## 1. What Claude did
>>"%PROMPT%" echo   ## 2. What the owner should expect
>>"%PROMPT%" echo   ## 3. What you should see
>>"%PROMPT%" echo   ## 4. What's blocking us
>>"%PROMPT%" echo.
>>"%PROMPT%" echo Everything you want to say goes UNDER one of those four. Use ###
>>"%PROMPT%" echo and deeper for your own headings - those are ignored by the
>>"%PROMPT%" echo validator. Section 4 is present even when empty. Section 3 is
>>"%PROMPT%" echo never empty.
>>"%PROMPT%" echo.
>>"%PROMPT%" echo THEN VALIDATE IT YOURSELF BEFORE YOU STOP:
>>"%PROMPT%" echo   tools\arbiter\validate-output.bat output.md
>>"%PROMPT%" echo It names the rule that failed. Exit 0 or the unit is failed
>>"%PROMPT%" echo whatever else it achieved. Fix the report and run it again.
goto :eof

rem ============================================================
:usage
echo.
echo   run-unit.bat ^<unit^> ^<root^> [--tools-file F] [--dry-run]
echo.
echo   unit  the work instruction number, e.g. 040
echo   root  the repository the unit runs against
echo.
echo   --phase-step N and its neighbours contribute to PHASE_OUTCOME.md
echo   ^(--phase-state --phase-approach --phase-hit --phase-move
echo    --phase-why --phase-decided --phase-licence --phase-did^)
echo.
echo   0 ran and validated, 1 lock held, 2 usage, 3 run failed,
echo   4 permission denials, 5 report invalid, 6 ledger failed,
echo   7 not a git repository
echo.
set "RC=2"
goto :end

rem ============================================================
:end
if not "%TOOK%"=="1" goto :endout
echo.
echo Releasing the session lock...
call "%HERE%lock.bat" release "%ROOT%" >nul
:endout
echo.
echo run-unit exit %RC%
endlocal & exit /b %RC%
