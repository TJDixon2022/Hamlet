@echo off
rem ============================================================
rem  FIXTURE - NOT CLAUDE. Stands in for every claude -p call
rem  run-phase.bat makes on the progress fixture, first on PATH, and
rem  records in calls.txt which kind it was, told apart by the first line
rem  of the prompt argument as the seed fixture's stand-in is:
rem
rem    ARBITER  writes nothing, so WORK_INSTRUCTIONS.md stays the one shipped -
rem             unless the root has a .vary file, in which case it rewrites the
rem             APPROACH line from a fixed list indexed by the call number, as a
rem             real arbiter authoring a fresh instruction would. 068
rem             - unless the root has a .leftover file, in which case the
rem             arbiter call NAMED IN THAT FILE leaks a session lock held by
rem             a LIVE pid
rem             and drops a previous unit's report at the root. That is
rem             HamLet on 2026-09-14 reproduced: it happens BEFORE the launch
rem             stamp is written and before the unit is launched, which is
rem             exactly where a leaked lock and a stale report sit. 067
rem    UNIT     writes a valid output.md, and - where the fixture root has a
rem             .flip file naming a criterion such as 2.3 - turns that line of
rem             PHASE_PLAN.md from - [ ] to - [x], as a unit meeting it would
rem    JUDGE    reads its prompt on stdin and answers a state, and HONEST: yes
rem
rem  Generated 2026-09-14 for: work instructions 064 task 5
rem ============================================================
rem  067: WINDOWS FIND, NAMED BY ITS PATH AND HELD IN A VARIABLE.
rem  A bare find takes whichever find is first on PATH, and launched from a
rem  shell carrying Git's usr/bin - which is how this fixture was run on
rem  2026-09-18 - that is the UNIX find, which with no path argument walks
rem  the CURRENT DIRECTORY recursively and never returns. The stand-in hung,
rem  the watchdog read a full core of CPU and correctly called it WORKING,
rem  and the arm spent ten looks doing nothing. Measured alone, the same
rem  line printed: find: /c/$Recycle.Bin: Permission denied.
rem
rem  A VARIABLE, AND UNQUOTED WHERE IT IS USED. Inside for /f a command
rem  line that STARTS with a quote is parsed as a quoted string, and cmd
rem  answers "The filename, directory name, or volume label syntax is
rem  incorrect" - measured, both with and without usebackq. Expanding a
rem  variable that holds the path puts no quote first, and there is no
rem  space in the System32 path to need one.
set "FIND=%SystemRoot%\System32\find.exe"
set "ARGS=%CD%\.stand-in-args.txt"
>"%ARGS%" echo %*
findstr /c:"Read ARBITER" "%ARGS%" >nul
if not errorlevel 1 goto :arbiter
findstr /c:"PROJECT:" "%ARGS%" >nul
if not errorlevel 1 goto :unit
goto :judge

:arbiter
>>"%CD%\calls.txt" echo ARBITER
rem  068: A REAL ARBITER AUTHORS A NEW INSTRUCTION EVERY ITERATION, and its
rem  APPROACH is not the one it wrote last time. The stand-in wrote nothing at
rem  all, which was harmless until 068 made a repeated approach a refusal -
rem  every arm that runs two units at one criterion would now be refused on
rem  its second iteration, not because the loop is wrong but because the
rem  stand-in was modelling the arbiter badly. Same class of fault as 067-s
rem  copy preserving an mtime.
rem
rem  A .vary file in the root turns it on, and the approach comes from a fixed
rem  list indexed by the arbiter call number, so it is different every time and
rem  the same on every run.
if not exist "%CD%\.vary" goto :varydone
set "ARBN="
for /f %%N in ('%FIND% /c "ARBITER" ^< "%CD%\calls.txt"') do set "ARBN=%%N"
if not defined ARBN goto :varydone
powershell -NoProfile -Command "$n=[int]'%ARBN%'; $v=@('read the watchdog log rather than polling any process tree','compare each process creation time against the launch stamp on disk','count quiet looks from a file so a restarted watcher resumes them','ask the operating system which handles the run still holds open'); $i=$n - 1; if($i -lt 0){ $i=0 }; if($i -ge $v.Count){ $i=$v.Count - 1 }; $a=$v[$i]; $p='%CD%\WORK_INSTRUCTIONS.md'; $t=[IO.File]::ReadAllText($p, [Text.Encoding]::UTF8); $t=[regex]::Replace($t, '(?m)^APPROACH: .*$', ('APPROACH: ' + $a)); [IO.File]::WriteAllText($p, $t, (New-Object Text.UTF8Encoding($false)))"
echo FIXTURE: the arbiter authored approach variant %ARBN%
:varydone
rem  071: A .exhaust FILE MAKES THE STAND-IN DECLARE THE CRITERION EXHAUSTED.
rem  It rewrites MOVE: in the instruction it authored to `exhausted`, which is
rem  the declaration unit 071 added - before this unit there was no way for an
rem  arbiter to say the routes had run out at all, and 070's redirect block
rem  tells it in terms that it may not assert one. The arms seed the RECORD by
rem  hand and let the loop decide whether the declaration is true, which is the
rem  whole question: the test reads the record, never the claim.
rem
rem  An empty .exhaust rewrites MOVE only. A .exhaust naming a criterion, such
rem  as 2.3, also rewrites ADVANCES, so an arm can claim a criterion other than
rem  the one its instruction names.
if not exist "%CD%\.exhaust" goto :exhaustdone
set "EXCRIT="
for /f "usebackq delims=" %%F in ("%CD%\.exhaust") do set "EXCRIT=%%F"
powershell -NoProfile -Command "$p='%CD%\WORK_INSTRUCTIONS.md'; $t=[IO.File]::ReadAllText($p, [Text.Encoding]::UTF8); $t=[regex]::Replace($t, '(?m)^MOVE: .*$', 'MOVE: exhausted'); $c='%EXCRIT%'.Trim(); if($c -match '^([0-9]+)\.([0-9]+)$'){ $t=[regex]::Replace($t, '(?m)^ADVANCES: .*$', ('ADVANCES: step ' + $Matches[1] + ' criterion ' + $Matches[2])) }; [IO.File]::WriteAllText($p, $t, (New-Object Text.UTF8Encoding($false)))"
echo FIXTURE: the arbiter declared MOVE: exhausted
:exhaustdone
if not exist "%CD%\.leftover" goto :arbitersay
set "ARBN="
for /f %%N in ('%FIND% /c "ARBITER" ^< "%CD%\calls.txt"') do set "ARBN=%%N"
rem  067: THE NUMBER COMES OUT OF THE FILE, as .flipat-s does. The first
rem  cut hard-coded 2 on the reasoning that the leak belongs to iteration
rem  2 - and with --seed the arbiter is NOT called on iteration 1 at all,
rem  so iteration 2-s call is the FIRST. The condition never fired, the
rem  arm ran two ordinary units and proved nothing. A number the arm sets
rem  cannot drift from how the arm is seeded.
set "LEAKAT="
for /f "usebackq delims=" %%F in ("%CD%\.leftover") do set "LEAKAT=%%F"
if not "%ARBN%"=="%LEAKAT%" goto :arbitersay
rem  PID 4 is the System process. It exists on every Windows machine for
rem  the life of the boot, so the lock this leaks is one whose owner is
rem  ALIVE - the half that must still refuse. A dead owner is the lock-dead
rem  arm, and the two must not be proved by one fixture.
>"%CD%\SESSION.lock" echo PID: 4
>>"%CD%\SESSION.lock" echo PID_SOURCE: argument
>>"%CD%\SESSION.lock" echo STARTED: 2026-09-14T02:05:00-04:00
>>"%CD%\SESSION.lock" echo HOST: fixture
>>"%CD%\SESSION.lock" echo ROOT: %CD%
>>"%CD%\SESSION.lock" echo TOKEN: fixture-leak
copy /y "%~dp0output-leftover.md" "%CD%\output.md" >nul
rem  067: written NOW, before this iteration-s launch stamp - which is what
rem  a previous unit-s report lying at the root actually is. copy alone
rem  would give it this repository-s checkout time instead.
powershell -NoProfile -Command "(Get-Item -LiteralPath '%CD%\output.md').LastWriteTimeUtc = [datetime]::UtcNow"
echo FIXTURE: leaked a live-owner session lock and left unit 358-s report at the root
:arbitersay
echo {"type":"result","subtype":"success","is_error":false,"terminal_reason":"completed","num_turns":1,"permission_denials":[],"result":"fixture arbiter - wrote nothing"}
exit /b 0

:unit
>>"%CD%\calls.txt" echo UNIT
rem  080: A .slow FILE MAKES THE UNIT CALL - AND ONLY THE UNIT CALL - WAIT.
rem  Criterion 3.4 names --minutes, and the owner's ceiling cannot be reached
rem  from this fixture without it: run-unit-watched.bat computes AGEMIN only
rem  when its look finds the tree ALIVE, and this stand-in normally answers in
rem  milliseconds, so by the first look at POLLSEC=60 the child is long gone,
rem  the sample reports TREE=gone, and :ceiling returns at
rem  `if not defined AGEMIN goto :poll` without ever comparing the clock.
rem
rem  BEHIND A FLAG FILE, as .vary and .flip are, and for a harder reason than
rem  tidiness: every other arm's timing is load-bearing for three nights of
rem  recorded results, and an unconditional wait here would add a minute and a
rem  half to each of about sixty arms. No file, no wait, not one second moved.
rem
rem  THE ARBITER AND JUDGE CALLS ARE NOT SLOWED. The watchdog watches the
rem  UNIT's process tree, so the unit is the only call where waiting proves
rem  anything, and slowing the other two would only cost time.
if not exist "%CD%\.slow" goto :unitnotslow
set "SLOWSEC="
for /f "usebackq delims=" %%F in ("%CD%\.slow") do set "SLOWSEC=%%F"
echo FIXTURE: the unit stand-in is waiting %SLOWSEC%s so the watchdog's first look finds it ALIVE
powershell -NoProfile -Command "Start-Sleep -Seconds %SLOWSEC%"
:unitnotslow
rem  067: a .noreport file in the root makes the unit write NO output.md at
rem  all. run-unit.bat then refuses its own report at exit 5, and the loop
rem  must halt with nothing appended rather than judge whatever is lying
rem  there. The stand-in still answers with valid JSON: a unit that dies
rem  without writing a report is not the same as a unit that never spoke.
if not exist "%CD%\.noreport" goto :unitflip
echo {"type":"result","subtype":"success","is_error":false,"terminal_reason":"completed","num_turns":1,"permission_denials":[],"result":"fixture unit - wrote no report at all"}
exit /b 0
:unitflip
if not exist "%CD%\.flip" goto :unitreport
rem  066: a .flipat file names WHICH unit call flips it - the Nth UNIT line in
rem  calls.txt - so one root can hold a no, then a yes, then a no. Without one
rem  every unit call flips, as it did for 064.
if not exist "%CD%\.flipat" goto :flipnow
set "FLIPAT="
set "UNITN="
for /f "usebackq delims=" %%F in ("%CD%\.flipat") do set "FLIPAT=%%F"
for /f %%N in ('%FIND% /c "UNIT" ^< "%CD%\calls.txt"') do set "UNITN=%%N"
if not "%UNITN%"=="%FLIPAT%" goto :unitreport
:flipnow
set "FLIPK="
for /f "usebackq delims=" %%F in ("%CD%\.flip") do set "FLIPK=%%F"
if defined FLIPK powershell -NoProfile -Command "$p='%CD%\PHASE_PLAN.md'; $t=[IO.File]::ReadAllText($p); [IO.File]::WriteAllText($p, $t.Replace('- [ ] %FLIPK% ', '- [x] %FLIPK% '), (New-Object System.Text.UTF8Encoding($false)))"
:unitreport
rem  065: a .report file in the root names which report the unit writes -
rem  output-reversal.md or output-nine.md from this folder. Without one the
rem  unit writes the plain valid report, as it did for 064.
set "REPORTF=%~dp0..\watchdog\fixture-output.md"
if exist "%CD%\.report" for /f "usebackq delims=" %%R in ("%CD%\.report") do set "REPORTF=%~dp0%%R"
copy /y "%REPORTF%" "%CD%\output.md" >nul
rem  067: AND THEN ITS WRITE TIME IS SET TO NOW, because copy PRESERVES THE
rem  SOURCE FILE-S LAST-WRITE TIME and a session writing a report does not.
rem  Measured on the kept arm-s first run: the report landed carrying
rem  fixture-output.md-s own mtime of 2026-09-12, the launcher-s stamp check
rem  correctly read it as older than the launch, and a VALID report was
rem  refused. The check was right and the stand-in was modelling a session
rem  badly. The bytes are untouched - the kept arm compares SHA256.
powershell -NoProfile -Command "(Get-Item -LiteralPath '%CD%\output.md').LastWriteTimeUtc = [datetime]::UtcNow"
echo {"type":"result","subtype":"success","is_error":false,"terminal_reason":"completed","num_turns":1,"permission_denials":[],"result":"fixture unit - ran the instruction in the tree"}
exit /b 0

:judge
if exist "%CD%\.realjudge" goto :realjudge
findstr "^" >nul
>>"%CD%\calls.txt" echo JUDGE
echo {"type":"result","subtype":"success","is_error":false,"terminal_reason":"completed","num_turns":1,"permission_denials":[],"result":"STATE: in progress\nWHY: fixture judge - the progress fixture proves the count, not the step.\nHONEST: yes"}
exit /b 0

rem  065: THE REAL JUDGE, for the reversal and nine arms. A .realjudge file in
rem  the root hands this call - its arguments and its stdin, the prompt the
rem  launcher built with the unit's report inside - to the real claude.exe, so
rem  what is proved is how the judge actually reads a claimed reversal, not what
rem  a stand-in was written to say. Restricted and read-only, as the launcher
rem  asks for it.
:realjudge
>>"%CD%\calls.txt" echo JUDGE
"%USERPROFILE%\.local\bin\claude.exe" %*
exit /b %ERRORLEVEL%
