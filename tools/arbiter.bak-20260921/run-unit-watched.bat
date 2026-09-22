@echo off
setlocal

rem  POLLSEC - one look at the process tree every sixty seconds. A look
rem  costs a PowerShell start and a full read of the process table, about
rem  two seconds, and a verdict that needs ten minutes of evidence gains
rem  nothing from looking more often than once a minute.
set "POLLSEC=60"

rem  DEADPOLLS - ten quiet looks in a row - each under FLOORMS of CPU time
rem  across the whole tree - before the run is called hung: the owner's ten
rem  minutes (work instructions 061). A build, a test run, a git call or a
rem  model call all accrue CPU somewhere in the tree while they run; ten
rem  minutes of next to none is a tree doing nothing at all.
set "DEADPOLLS=10"

rem  FLOORMS - a look is WORKING only when the whole tree accrued at least
rem  this many milliseconds of CPU time since the last look; anything less
rem  is QUIET. 100, from unit 062, the arbiter's number and overrulable:
rem  three times the 31 ms tick that reset 061's hung proof when the machine
rem  was busy - that and a 15 ms tick are one and two ticks of the 15.6 ms
rem  Windows clock - and below the 234 ms of the lightest real work 061
rem  measured, a PowerShell starting. Not an argument, for DEADPOLLS' reason.
set "FLOORMS=100"

rem ============================================================
rem  run-unit-watched.bat  -  a run you can walk away from
rem
rem      run-unit-watched.bat <unit> <root> [--minutes N]
rem                          [--tools-file F]
rem
rem      0  the run finished on its own and run-unit.bat said 0
rem      1  KILLED - by the watchdog, because the run's whole process
rem         tree accrued under FLOORMS of CPU time a look for ten
rem         minutes; or at the owner's --minutes ceiling. The ledger
rem         line says which.
rem      2  usage, bad root, or the run did not start
rem      other  whatever run-unit.bat exited with, passed through
rem
rem  Launches run-unit.bat in the background, looks at the CPU time its
rem  process tree has accrued once a minute, and TERMINATES THE RUN BY
rem  PID when ten looks in a row find less than FLOORMS - 100 ms.
rem
rem  THE FLOOR, AND WHY IT EXISTS. Unit 062. As first built, any growth at
rem  all reset the quiet count. 061's proof ran three fixtures at once and
rem  the hung tree accrued 31 ms at look 7 - one or two ticks of the 15.6
rem  ms Windows clock - so the count went back to zero and a hung run was
rem  never killed. Run alone, the same tree accrued nothing and was killed.
rem  This machine is never idle, so the condition that failed is the one
rem  the watchdog lives in.
rem
rem  ---------------------------------------------------------------
rem  THE RULE MEASURES DEAD, NOT QUIET. Work instructions 061, 2026-09-12.
rem  Until then this script killed a run after twelve minutes with no
rem  write to PROJECT_STATUS.md. That rule was built for a real problem -
rem  the ledger holds eight sessions that hung for twenty to sixty
rem  minutes producing nothing - and on HamLet it killed three productive
rem  units in two days, each inside a long build or between a green test
rem  run and its commit. A working session is often quiet. The owner:
rem  "If it's working, then it should be working."
rem
rem  PROJECT_STATUS.md IS NO LONGER A LIVENESS SIGNAL. It is read once a
rem  look for the status line printed to this console, for whoever is
rem  watching it, and nothing it says or fails to say kills anything.
rem  watchdog.bat, which judged that file, is no longer called from here.
rem
rem  THE WHOLE TREE, NOT THE PARENT. claude -p spawns dotnet, git and
rem  shells, and the work happens in the children: a parent blocked on a
rem  two-minute build accrues nothing while its child runs flat out. The
rem  tree is the launched pid and every descendant, walked from
rem  Win32_Process by ParentProcessId. Kernel plus user time, which is
rem  what Get-Process calls TotalProcessorTime.
rem
rem  PER PROCESS, NOT ONE BARE SUM. A child that exits between two looks
rem  takes its CPU time out of the sum, so two totals compared directly
rem  can go DOWN while the tree is busy. Each process is compared with its
rem  own previous reading, keyed by pid AND creation time, and a process
rem  not seen before counts all of its time. Growth anywhere is growth.
rem
rem  A CHILD IS ONLY A CHILD IF IT WAS CREATED AFTER ITS PARENT. Windows
rem  reuses pids, and a process whose recorded parent pid now belongs to
rem  a newer process is not that process's child. The launched pid is
rem  held to its own creation time the same way, so a reused pid reads as
rem  the tree gone rather than as somebody else's tree being watched.
rem
rem  A LOOK THAT CANNOT BE TAKEN IS NOT A ZERO. If the process table
rem  cannot be read, the look is reported as unknown, the count of quiet
rem  looks neither advances nor resets, and polling continues. Killing a
rem  process on a reading you could not take is the worst thing in this
rem  file, and calling it working would be the second worst.
rem
rem  WHAT THIS CANNOT SEE, stated rather than buried. A hang whose event
rem  loop still ticks accrues CPU and is not killed by this rule. Unit 061
rem  measured two interactive claude.exe processes on this machine
rem  accruing 2.9 s and 7.6 s of CPU in 175 s, in states it could not
rem  see; whether a HEADLESS claude -p that has hung reads as zero was not
rem  measured, because no such hang could be produced to order. The
rem  owner's --minutes is the ceiling for that, and this file does not
rem  invent one of its own.
rem
rem  NO CLOCK OF ITS OWN. A run that accrues CPU is left alone however
rem  long it runs. The only ceilings are the owner's, set at launch:
rem  --minutes here, passed down by run-phase.bat, and run-phase.bat's
rem  --budget. Neither has a default that fires.
rem
rem  BY PID, NEVER BY NAME. taskkill /IM claude.exe would take the owner's
rem  own interactive sessions with it - unit 039's listing showed five
rem  running on this machine at the time. This tracks the process it
rem  started and kills that one, with its children, and nothing else.
rem
rem  THE LOCK IS RELEASED ON EVERY PATH OUT, INCLUDING THE KILL.
rem  run-unit.bat takes and releases its own lock; where this script
rem  kills that process the release never runs, so this script releases
rem  it instead. A lock left behind after a kill blocks the queue.
rem
rem  A KILLED RUN GETS ITS OWN LEDGER LINE, and the sentence changed with
rem  the rule, twice. The twelve-minute rule wrote "no status write within
rem  N min of the launch clock". 061's first cut wrote "no CPU time in 10
rem  min". From 062 it writes "under 100 ms of CPU time in the process tree
rem  for 10 min", built from FLOORMS and DEADPOLLS, and the owner's ceiling
rem  writes its own sentence. A reader of the ledger can tell which rule
rem  killed which run from the line alone.
rem
rem  THE KILL IS ALSO WRITTEN INTO THE RUN'S LOG, .run-unit\watched.log.
rem  run-phase.bat tells a kill from a held lock - both arrive as exit 1 -
rem  by finding "Terminating pid" in that file. This script used to print
rem  that line only to its own console, which is not that file, so every
rem  kill reached the phase loop as "the run could not take the session
rem  lock". HamLet's RUN_LEDGER.md shows it: a watchdog kill at 11:41 on
rem  2026-09-12 followed by a phase halt at 11:41 reading exactly that.
rem
rem  THE BACKGROUND RUNNER IS WRITTEN TO A FILE and then executed -
rem  CPS-DEC-021; the history is at the runner below.
rem
rem  Batch, not PowerShell: a .ps1 will not run on this machine. The
rem  inline powershell -NoProfile -Command calls are not script files,
rem  and are used where cmd cannot start a process and keep its pid,
rem  sleep, read a clock, or read the process table.
rem
rem  Generated 2026-08-28 for: work instructions 040 task 4
rem  Rewritten 2026-09-12 for: work instructions 061 task 1 - the
rem           watchdog watches the process tree, not the status file
rem ============================================================

set "HERE=%~dp0"
set "RC=0"
set "UNIT=%~1"
set "ROOT=%~2"
set "MINUTES="
set "TOOLSARG="
set /a DEADMIN=POLLSEC*DEADPOLLS/60

if "%UNIT%"=="" goto :usage
if "%ROOT%"=="" goto :usage
shift
shift

:parse
if "%~1"=="" goto :parsed
if /i "%~1"=="--minutes"    set "MINUTES=%~2" & shift & shift & goto :parse
if /i "%~1"=="--tools-file" set "TOOLSARG=--tools-file "%~2"" & shift & shift & goto :parse
if /i "%~1"=="--poll" goto :pollgone
echo ERROR: unexpected argument: %~1
goto :usage

:pollgone
echo ERROR: --poll is gone. The look interval is POLLSEC at the top of
echo this file, and the watchdog has no clock a caller sets. Unit 061.
goto :usage

:parsed
if not defined MINUTES goto :minok
echo %MINUTES%| findstr /r "^[1-9][0-9]*$" >nul
if not errorlevel 1 goto :minok
echo ERROR: --minutes must be a whole number of minutes, at least 1: %MINUTES%
goto :usage
:minok

if "%ROOT:~-1%"=="\" set "ROOT=%ROOT:~0,-1%"
if not exist "%ROOT%\" (
  echo ERROR: repo root not found: %ROOT%
  set "RC=2"
  goto :end
)

set "WORK=%ROOT%\.run-unit"
if not exist "%WORK%" mkdir "%WORK%"
set "PIDFILE=%WORK%\watched.pid"
set "BORNFILE=%WORK%\watched.born"
set "CPUFILE=%WORK%\watched.cpu"
set "RCFILE=%WORK%\watched.rc"
set "LOGFILE=%WORK%\watched.log"
set "RUNNER=%WORK%\watched-runner.bat"

rem --- the launch clock, read from the system, never composed ----
set "STARTED="
for /f "usebackq delims=" %%D in (`powershell -NoProfile -Command "(Get-Date).ToString('yyyy-MM-ddTHH:mm')"`) do set "STARTED=%%D"

echo.
echo ============================================================
echo  run-unit-watched
echo    unit      : %UNIT%
echo    root      : %ROOT%
echo    started   : %STARTED%
echo    watchdog  : kills after %DEADPOLLS% looks in a row, %POLLSEC%s apart, in which
echo                the run's whole process tree accrued under %FLOORMS% ms of CPU.
echo                It has no clock of its own.
if defined MINUTES echo    ceiling   : %MINUTES% min, set by the owner with --minutes
if not defined MINUTES echo    ceiling   : none - no --minutes was given
echo ============================================================
echo.

if exist "%PIDFILE%" del /q "%PIDFILE%"
if exist "%BORNFILE%" del /q "%BORNFILE%"
if exist "%CPUFILE%" del /q "%CPUFILE%"
if exist "%RCFILE%" del /q "%RCFILE%"

rem --- the background runner, WRITTEN TO A FILE and then executed -
rem  CPS-DEC-021. The first cut built this as a cmd /c command line
rem  inside a PowerShell -ArgumentList, three layers of quoting deep,
rem  with a redirect and an ampersand in it. It launched NOTHING -
rem  no log, no exit code, no work done - and the poll loop then
rem  reported "the run finished on its own" because the pid it had
rem  captured was already gone. A launcher that silently launches
rem  nothing and calls it finished is worse than one that fails.
>"%RUNNER%" echo @echo off
>>"%RUNNER%" echo call "%HERE%run-unit.bat" %UNIT% "%ROOT%" %TOOLSARG%
rem  PARENTHESISED, and this is not style. `echo %ERRORLEVEL%> f`
rem  with an exit code of 0 reads as `echo 0> f` - a DIGIT before a
rem  redirect is a FILE HANDLE. Measured 2026-08-28.
>>"%RUNNER%" echo ^(echo %%ERRORLEVEL%%^)^> "%RCFILE%"

rem  THE CREATION TIME IS TAKEN WITH THE PID. It is what lets every later
rem  look tell this process from a stranger that has been handed its pid.
echo Launching run-unit.bat in the background...
powershell -NoProfile -Command "$p = Start-Process -FilePath '%RUNNER%' -RedirectStandardOutput '%LOGFILE%' -PassThru -WindowStyle Hidden; $p.Id | Set-Content -LiteralPath '%PIDFILE%'; $c = Get-CimInstance Win32_Process -Filter ('ProcessId=' + $p.Id); if($c){ $c.CreationDate.ToFileTimeUtc() | Set-Content -LiteralPath '%BORNFILE%' }"

set "CHILD="
set "BORN="
if exist "%PIDFILE%" for /f "usebackq delims=" %%P in ("%PIDFILE%") do set "CHILD=%%P"
if exist "%BORNFILE%" for /f "usebackq delims=" %%P in ("%BORNFILE%") do set "BORN=%%P"
if not defined CHILD (
  echo ERROR: the run did not start - no pid was captured.
  set "RC=2"
  goto :end
)
echo   pid       : %CHILD%
echo   log       : %LOGFILE%
if not defined BORN (
  echo   The process was gone before its creation time could be read -
  echo   it finished at once. Reading its exit code.
  goto :finished
)
echo.

rem ============================================================
rem  THE POLL LOOP
rem  Flat, with labels, and no parenthesised blocks: a variable set
rem  inside a block cannot be read inside the same block without
rem  delayed expansion, and this file does not turn that on.
rem ============================================================
set "QUIET=0"
set "LOOKS=0"

:poll
powershell -NoProfile -Command "Start-Sleep -Seconds %POLLSEC%"
set /a LOOKS+=1
call :sample
if "%TREE%"=="gone" goto :finished
if "%TREE%"=="unknown" goto :unknownlook
if "%ACTIVE%"=="1" goto :working

set /a QUIET+=1
echo   [look %LOOKS%] quiet - %GREWMS% ms across %NPROC% process^(es^), under the %FLOORMS% ms floor - quiet look %QUIET% of %DEADPOLLS% - most from: %TOP%
call :statusline
if %QUIET% GEQ %DEADPOLLS% goto :killhung
goto :ceiling

:working
set "QUIET=0"
echo   [look %LOOKS%] working - %GREWMS% ms across %NPROC% process^(es^), at or over the %FLOORMS% ms floor - most from: %TOP%
call :statusline
goto :ceiling

:unknownlook
echo   [look %LOOKS%] UNKNOWN - the process table could not be read. Not a quiet
echo                 look and not a working one: the quiet count stays at %QUIET%.
goto :ceiling

:ceiling
if not defined MINUTES goto :poll
if not defined AGEMIN goto :poll
if %AGEMIN% GEQ %MINUTES% goto :killceiling
goto :poll

rem ============================================================
rem  ONE LOOK. Sets TREE (alive, gone, unknown), and when alive NPROC,
rem  GREWMS (the CPU the whole tree accrued since the last look, in ms to
rem  a tenth), TOP (the process that accrued the most of it, with its pid
rem  and its share), ACTIVE (1 when GREWMS reached FLOORMS, 0 when it did
rem  not) and AGEMIN (how old the launched process is).
rem
rem  THE DELTA AND ITS BIGGEST CONTRIBUTOR ARE PRINTED ON EVERY LOOK, NOT
rem  ONLY THE VERDICT. Unit 062. 061's proof run was reset by a 31 ms tick
rem  that nothing could attribute, because a look printed "NO CPU" or a
rem  whole-millisecond total and never said where it came from. A console
rem  line carrying the number and the process makes the next reset
rem  somebody's rather than nobody's.
rem
rem  THE FLOOR IS COMPARED IN 100 ns UNITS, the unit the process table
rem  reports, so a delta just under the floor is not rounded up to it.
rem
rem  The previous reading lives in .run-unit\watched.cpu as
rem  pid,creation,cpu lines and is replaced on every look. Deleted at
rem  launch, so the first look counts every process as new - growth -
rem  and a run is never judged quiet on a comparison with nothing.
rem
rem  NO BACKTICK, NO PERCENT SIGN AND NO EXCLAMATION MARK inside the
rem  PowerShell: for /f "usebackq" ends the command at a backtick, and
rem  cmd owns the other two. Pipes are not needed and not used.
rem ============================================================
:sample
set "TREE=unknown"
set "ACTIVE="
set "NPROC="
set "GREWMS="
set "TOP="
set "AGEMIN="
for /f "usebackq tokens=1,* delims==" %%A in (`powershell -NoProfile -Command "try{ $root=[int]'%CHILD%'; $born=[int64]'%BORN%'; $sf='%CPUFILE%'; $floor=[int64]'%FLOORMS%' * 10000; $inv=[Globalization.CultureInfo]::InvariantCulture; $all=@(Get-CimInstance Win32_Process -ErrorAction Stop); $by=@{}; $kids=@{}; foreach($p in $all){ $i=[int]$p.ProcessId; $by[$i]=$p; $pp=[int]$p.ParentProcessId; if($pp -ne $i){ if(-not $kids.ContainsKey($pp)){ $kids[$pp]=New-Object System.Collections.ArrayList }; [void]$kids[$pp].Add($p) } }; $r=$by[$root]; if((-not $r) -or ($r.CreationDate.ToFileTimeUtc() -ne $born)){ 'TREE=gone'; exit }; $tree=New-Object System.Collections.ArrayList; $seen=@{}; $q=New-Object System.Collections.Queue; $q.Enqueue($r); while($q.Count -gt 0){ $n=$q.Dequeue(); $i=[int]$n.ProcessId; if($seen.ContainsKey($i)){ continue }; $seen[$i]=1; [void]$tree.Add($n); if($kids.ContainsKey($i)){ foreach($c in $kids[$i]){ if($c.CreationDate -ge $n.CreationDate){ $q.Enqueue($c) } } } }; $prev=@{}; if(Test-Path -LiteralPath $sf){ foreach($ln in (Get-Content -LiteralPath $sf)){ $f=$ln.Split(','); if($f.Count -eq 3){ $prev[$f[0] + ',' + $f[1]]=[int64]$f[2] } } }; $grew=[int64]0; $top='nothing - no process in the tree accrued any'; $topd=[int64]0; $out=@(); foreach($n in $tree){ $k=[string]$n.ProcessId + ',' + [string]$n.CreationDate.ToFileTimeUtc(); $cpu=[int64]$n.KernelModeTime + [int64]$n.UserModeTime; $share=[int64]0; if($prev.ContainsKey($k)){ $d=$cpu - $prev[$k]; if($d -gt 0){ $share=$d } } else { $share=$cpu }; $grew+=$share; if($share -gt $topd){ $topd=$share; $top=[string]$n.Name + ' pid ' + [string]$n.ProcessId + ' at ' + ([math]::Round($topd / 10000.0, 1)).ToString($inv) + ' ms' }; $out+=($k + ',' + $cpu) }; Set-Content -LiteralPath $sf -Value $out -Encoding ascii; 'TREE=alive'; 'NPROC=' + $tree.Count; 'GREWMS=' + ([math]::Round($grew / 10000.0, 1)).ToString($inv); 'TOP=' + $top; if($grew -ge $floor){ 'ACTIVE=1' } else { 'ACTIVE=0' }; 'AGEMIN=' + [int][math]::Floor(((Get-Date) - $r.CreationDate).TotalMinutes) }catch{ exit }"`) do set "%%A=%%B"
if "%TREE%"=="alive" if not defined ACTIVE set "TREE=unknown"
goto :eof

rem ============================================================
rem  THE STATUS LINE. Read for whoever is watching this console. It
rem  decides nothing. The two values are copied through a substitution
rem  before they are echoed, because they are text a session wrote and
rem  & | < > ^ are live on a bare echo line - :echosafe in run-phase.bat.
rem ============================================================
:statusline
if exist "%ROOT%\PROJECT_STATUS.md" goto :statusread
echo                 status file: none. Shown for you; it kills nothing.
goto :eof
:statusread
set "ST_TASK="
set "ST_UPD="
call "%HERE%readkey.bat" "%ROOT%\PROJECT_STATUS.md" "TASK" ST_TASK >nul
call "%HERE%readkey.bat" "%ROOT%\PROJECT_STATUS.md" "UPDATED" ST_UPD >nul
if not defined ST_TASK set "ST_TASK=unread"
if not defined ST_UPD set "ST_UPD=unread"
set "ST_TASK=%ST_TASK:&=+%"
set "ST_TASK=%ST_TASK:|=/%"
set "ST_TASK=%ST_TASK:<=[%"
set "ST_TASK=%ST_TASK:>=]%"
set "ST_TASK=%ST_TASK:^=~%"
set "ST_UPD=%ST_UPD:&=+%"
set "ST_UPD=%ST_UPD:|=/%"
set "ST_UPD=%ST_UPD:<=[%"
set "ST_UPD=%ST_UPD:>=]%"
set "ST_UPD=%ST_UPD:^=~%"
echo                 status file says TASK %ST_TASK%, UPDATED %ST_UPD% - shown for you; it kills nothing.
goto :eof

rem ============================================================
rem  THE KILLS
rem ============================================================
:killhung
echo.
echo   UNDER %FLOORMS% MS OF CPU A LOOK FOR %DEADMIN% MIN. %DEADPOLLS% looks in a row, %POLLSEC%s apart,
echo   and the %NPROC% process^(es^) in this run's tree accrued less than %FLOORMS% ms between
echo   each. The last look read %GREWMS% ms. That is a dead run, not a quiet one. Killing it.
rem  THE SENTENCE SAYS UNDER THE FLOOR, NOT NO CPU. Unit 062, the arbiter's
rem  wording, overrulable. Once a tree may accrue 80 ms a look and still be
rem  killed, "no CPU time" is a line nobody measured. The floor and the
rem  minutes come from the constants, so the sentence cannot drift from them.
set "KILLWHY=killed by the watchdog: under %FLOORMS% ms of CPU time in the process tree for %DEADMIN% min"
goto :kill

:killceiling
echo.
echo   THE OWNER'S CEILING. This run is %AGEMIN% min old and --minutes is %MINUTES%.
echo   The ceiling is a clock and it is the owner's; whether the run was
echo   still accruing CPU does not enter into it.
set "KILLWHY=killed at the owner's --minutes ceiling: %MINUTES% min"
goto :kill

:kill
echo   Terminating pid %CHILD% and its children, BY PID.
taskkill /PID %CHILD% /T /F
echo.

set "ENDED="
for /f "usebackq delims=" %%D in (`powershell -NoProfile -Command "(Get-Date).ToString('yyyy-MM-ddTHH:mm')"`) do set "ENDED=%%D"

rem  Into the run's own log, for run-phase.bat's exit-1 disambiguation -
rem  see the header. After the kill, and after a pause, because the killed
rem  runner held that file open for its redirect until it died.
powershell -NoProfile -Command "Start-Sleep -Seconds 2"
>>"%LOGFILE%" echo Terminating pid %CHILD% - %KILLWHY%

rem  run-unit.bat's own release never ran, so release its lock here
echo   Releasing the lock the killed run was holding...
call "%HERE%lock.bat" release "%ROOT%"

rem  A KILLED RUN'S COST IS unknown, not 0. The JSON is never written
rem  because the process was terminated before claude could emit it,
rem  so the money spent is real and unmeasured - and a 0 there would be
rem  a claim that the run was free.
call "%HERE%ledger.bat" "%UNIT%" "%STARTED%" "%ENDED%" "killed" "%KILLWHY%" "unknown" "%ROOT%" >nul
echo   Ledger line written: %KILLWHY%
set "RC=1"
goto :end

rem ============================================================
:finished
set "URC="
if exist "%RCFILE%" for /f "usebackq tokens=* delims=" %%R in ("%RCFILE%") do set "URC=%%R"
if not defined URC set "URC=unknown"
echo.
echo   The run finished on its own - its process tree is gone.
echo   run-unit.bat exit : %URC%
echo   its output is in  : %LOGFILE%
echo.
if "%URC%"=="unknown" (
  set "RC=2"
  goto :end
)
set "RC=%URC%"
goto :end

rem ============================================================
:usage
echo.
echo   run-unit-watched.bat ^<unit^> ^<root^> [--minutes N] [--tools-file F]
echo.
echo   THE WATCHDOG HAS NO CLOCK OF ITS OWN. It kills a run only after
echo   %DEADPOLLS% looks in a row, %POLLSEC%s apart, in which the run's whole process
echo   tree accrued under %FLOORMS% ms of CPU time. A run that is working is left alone
echo   however long it takes. PROJECT_STATUS.md is not a liveness signal.
echo.
echo   --minutes N  the owner's wall-clock ceiling for this run. No default:
echo                without it, nothing here kills on time. With run-phase.bat's
echo                --budget, it is one of the only two ceilings there are.
echo.
echo   0 finished and run-unit said 0, 1 KILLED - by the watchdog or at
echo   --minutes, the ledger says which - 2 usage or bad root,
echo   other = run-unit.bat's own code
echo.
set "RC=2"
goto :end

rem ============================================================
:end
echo.
echo run-unit-watched exit %RC%
endlocal & exit /b %RC%
