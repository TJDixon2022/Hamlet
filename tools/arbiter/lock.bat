@echo off
rem ============================================================
rem  lock.bat  -  the session lock, one writer at a time
rem
rem  CLAUDE_CODE.md 5's "one writer at a time" made mechanical.
rem  GROKBOT.md 3 names SESSION.lock at the repository root,
rem  holding PID and start time.
rem
rem      lock.bat take    [--force] [--pid N] [root]
rem      lock.bat release                     [root]
rem      lock.bat status                      [root]
rem
rem  THE EXIT CODE IS THE POINT. A loop reads exit codes, not
rem  prose. Every path below sets one deliberately:
rem
rem      take     0 = taken            1 = already held
rem                                    2 = usage / bad root
rem                                    3 = could not write
rem                                    4 = lost a race
rem      release  0 = removed          1 = was not there
rem                                    2 = usage / bad root
rem                                    3 = could not remove
rem      status   0 = free             1 = held (live holder)
rem                                    2 = usage / bad root
rem                                    5 = held by a stale lock
rem
rem  ONE EXIT POINT, AND WHY. Every path sets RC and jumps to
rem  :end. Nothing calls exit /b from inside a parenthesised
rem  block. The first cut of this script did, and
rem      cmd /c lock.bat take        on a held lock returned 0
rem      cmd /c call lock.bat take   on a held lock returned 1
rem  - the refusal printed either way and the code was lost on
rem  the form a double-click and a naive loop both use. A guard
rem  whose exit code depends on how it was invoked is not a
rem  guard. Flat control flow to a single exit is the fix, and
rem  it is also why there is no delayed-expansion read of a
rem  variable set inside a block anywhere below.
rem
rem  Repo root defaults to C:\Source\HamLet.
rem  A trailing backslash on the argument is accepted, as in
rem  tools\get-files\get-files.template.bat.
rem
rem  Batch, not PowerShell: a .ps1 will not run on this machine,
rem  unsigned scripts are blocked by execution policy. The three
rem  inline powershell -NoProfile -Command calls below are not
rem  script files and are not affected. They are used only where
rem  cmd genuinely cannot do the job - reading an ISO timestamp
rem  from the clock, differencing two of them, and resolving the
rem  calling process. wmic is NOT used: it is gone on current
rem  Windows and a dead call there silently named every backup
rem  the same thing once already.
rem
rem  ON THE tasklist REDIRECTION. Inside for /f "usebackq" ...
rem  (`...`) the redirection MUST be written 2^>nul. Without the
rem  caret cmd parses the > while parsing the for statement and
rem  the whole line dies with "2> was unexpected at this time";
rem  with too many carets tasklist is handed 2>nul as a literal
rem  argument and errors with "Invalid argument/option". Both
rem  were seen. One caret, and liveness was verified against a
rem  pid held open for the duration rather than one that had
rem  already exited.
rem
rem  ON THE PID. The batch process that runs this script exits
rem  the moment it finishes, so recording ITS pid would make
rem  every lock stale on creation and --force would always
rem  succeed - a lock that is always breakable is no lock. The
rem  holder is therefore either
rem      --pid N   the caller says who holds it, or
rem      (default) the process that INVOKED this script, found
rem                as the grandparent of the inline powershell.
rem  Which one was used is written into the file as PID_SOURCE,
rem  because a reader must be able to tell a measured pid from
rem  an assumed one. Where neither can be established the file
rem  records PID: unknown, and status reports the holder as
rem  UNKNOWN rather than STALE - not running and could not be
rem  determined are different facts and only the first licenses
rem  a break. An unknown-holder lock is cleared with release,
rem  which is explicit and is always the owner's own hand.
rem
rem  ON THE RACE. cmd has no atomic create-if-absent for a file.
rem  take tests, writes, and then READS BACK its own one-shot
rem  token; a taker whose token is not the one on disk lost and
rem  exits 4. That narrows the window to the span between one
rem  taker's write and its own read-back rather than removing
rem  it. Recorded rather than hidden: two takers landing inside
rem  that span both succeed. The write scopes in GROKBOT.md 3
rem  put a single loop on this side of the lock, which is what
rem  makes the residual window acceptable and not what makes it
rem  absent.
rem
rem  Generated 2026-08-27 for: work instructions 023 task 3
rem ============================================================

setlocal

set "RC=0"
set "VERB=%~1"
if "%VERB%"=="" goto :usage

set "FORCE="
set "WANTPID="
set "REPO="

rem --- parse the tail: --force, --pid N, and at most one root --
shift
:parse
if "%~1"=="" goto :parsed
if /i "%~1"=="--force" set "FORCE=1" & shift & goto :parse
if /i "%~1"=="--pid"   goto :parsepid
if defined REPO goto :badarg
set "REPO=%~1"
shift
goto :parse

:parsepid
if "%~2"=="" echo ERROR: --pid needs a number. & goto :usage
set "WANTPID=%~2"
shift
shift
goto :parse

:badarg
echo ERROR: unexpected argument: %~1
goto :usage

:parsed
if "%REPO%"=="" set "REPO=C:\Source\HamLet"
if "%REPO:~-1%"=="\" set "REPO=%REPO:~0,-1%"

if not exist "%REPO%\" (
  echo ERROR: repo root not found: %REPO%
  echo Pass the correct path as the last argument.
  set "RC=2"
  goto :end
)

set "LOCK=%REPO%\SESSION.lock"

if /i "%VERB%"=="take"    goto :take
if /i "%VERB%"=="release" goto :release
if /i "%VERB%"=="status"  goto :status
echo ERROR: unknown verb: %VERB%
goto :usage

rem ============================================================
:take
call :readlock
if not "%HELD%"=="1" goto :dotake

call :liveness
echo.
echo Lock is already held.
call :printholder
if not "%FORCE%"=="1" goto :refuse_held
if "%ALIVE%"=="1" goto :refuse_live
if "%ALIVE%"=="?" goto :refuse_unknown

echo.
echo BREAKING A STALE LOCK: pid %HPID% is not running.
echo   it was taken   : %HSTART%
echo   by             : %HHOST%
echo A stale lock is never broken silently. This line is that.
del /q "%LOCK%" >nul 2>&1
if exist "%LOCK%" (
  echo ERROR: could not remove the stale lock: %LOCK%
  set "RC=3"
  goto :end
)

:dotake
call :now
if not defined NOW (
  echo ERROR: could not read the clock. Refusing to compose a timestamp.
  set "RC=3"
  goto :end
)

call :resolvepid
set "TOKEN=%RANDOM%%RANDOM%-%NEWPID%"

>"%LOCK%" echo PID: %NEWPID%
>>"%LOCK%" echo PID_SOURCE: %PIDSOURCE%
>>"%LOCK%" echo STARTED: %NOW%
>>"%LOCK%" echo HOST: %COMPUTERNAME%
>>"%LOCK%" echo ROOT: %REPO%
>>"%LOCK%" echo TOKEN: %TOKEN%

if not exist "%LOCK%" (
  echo ERROR: could not write the lock: %LOCK%
  set "RC=3"
  goto :end
)

rem --- read back our own token; a taker that lost, lost --------
set "SEEN="
for /f "usebackq tokens=1,* delims= " %%A in ("%LOCK%") do if /i "%%A"=="TOKEN:" set "SEEN=%%B"
if "%SEEN%"=="%TOKEN%" goto :took
echo.
echo ERROR: lost a race for the lock. The token on disk is not ours.
echo   ours : %TOKEN%
echo   disk : %SEEN%
echo Not removing it - the winner holds it.
set "RC=4"
goto :end

:took
echo.
echo Lock taken.
echo   file    : %LOCK%
echo   pid     : %NEWPID%  ^(%PIDSOURCE%^)
echo   started : %NOW%
echo   host    : %COMPUTERNAME%
set "RC=0"
goto :end

:refuse_held
echo.
echo Refusing. Use release, or take --force if it is stale.
set "RC=1"
goto :end

:refuse_live
echo.
echo Refusing --force: holder pid %HPID% IS RUNNING. A live lock is not stale.
echo Use release if you are certain, and know that you are certain by hand.
set "RC=1"
goto :end

:refuse_unknown
echo.
echo Refusing --force: holder pid could not be determined. Unknown is not stale.
echo Use release if you are certain, and know that you are certain by hand.
set "RC=1"
goto :end

rem ============================================================
:release
call :readlock
if "%HELD%"=="1" goto :dorelease
echo.
echo Nothing to release. No lock at: %LOCK%
set "RC=1"
goto :end

:dorelease
call :liveness
del /q "%LOCK%" >nul 2>&1
if exist "%LOCK%" (
  echo.
  echo ERROR: could not remove the lock: %LOCK%
  set "RC=3"
  goto :end
)
echo.
echo Lock released.
call :printholder
set "RC=0"
goto :end

rem ============================================================
:status
call :readlock
if "%HELD%"=="1" goto :dostatus
echo.
echo FREE - no lock at: %LOCK%
set "RC=0"
goto :end

:dostatus
call :liveness
echo.
if "%ALIVE%"=="1" echo HELD - the holder is running.
if "%ALIVE%"=="0" echo STALE - the holder is not running. Break it with: lock.bat take --force
if "%ALIVE%"=="?" echo HELD - holder liveness UNKNOWN. Not reported as stale; unknown is not stale.
call :printholder
set "RC=1"
if "%ALIVE%"=="0" set "RC=5"
goto :end

rem ============================================================
rem  read the lock file into HPID / HSTART / HHOST / HSOURCE
:readlock
set "HELD="
set "HPID="
set "HSTART="
set "HHOST="
set "HSOURCE="
if not exist "%LOCK%" goto :eof
set "HELD=1"
for /f "usebackq tokens=1,* delims= " %%A in ("%LOCK%") do call :readlockline "%%A" "%%B"
if not defined HPID    set "HPID=unknown"
if not defined HSTART  set "HSTART=unknown"
if not defined HHOST   set "HHOST=unknown"
if not defined HSOURCE set "HSOURCE=unknown"
goto :eof

:readlockline
if /i "%~1"=="PID:"        set "HPID=%~2"
if /i "%~1"=="PID_SOURCE:" set "HSOURCE=%~2"
if /i "%~1"=="STARTED:"    set "HSTART=%~2"
if /i "%~1"=="HOST:"       set "HHOST=%~2"
goto :eof

rem ============================================================
rem  ALIVE: 1 running, 0 not running, ? could not be determined
:liveness
set "ALIVE=?"
if "%HPID%"=="unknown" goto :eof
echo %HPID%| findstr /r "^[0-9][0-9]*$" >nul
if errorlevel 1 goto :eof
set "ALIVE=0"
for /f "usebackq tokens=1,2 delims=," %%A in (`tasklist /NH /FO CSV /FI "PID eq %HPID%" 2^>nul`) do if "%%~B"=="%HPID%" set "ALIVE=1"
goto :eof

rem ============================================================
:printholder
set "AGE="
if "%HSTART%"=="unknown" goto :printholder2
for /f "usebackq delims=" %%T in (`powershell -NoProfile -Command "try{[int]([datetime]::Now - [datetime]::Parse('%HSTART%')).TotalMinutes}catch{}"`) do set "AGE=%%T"
:printholder2
echo   pid     : %HPID%  ^(%HSOURCE%^)
echo   started : %HSTART%
if defined AGE     echo   age     : %AGE% min
if not defined AGE echo   age     : unknown - STARTED could not be parsed
echo   host    : %HHOST%
if "%ALIVE%"=="1" echo   holder  : RUNNING
if "%ALIVE%"=="0" echo   holder  : NOT RUNNING - stale
if "%ALIVE%"=="?" echo   holder  : UNKNOWN
goto :eof

rem ============================================================
:now
set "NOW="
for /f "usebackq delims=" %%D in (`powershell -NoProfile -Command "Get-Date -Format yyyy-MM-ddTHH:mm:sszzz"`) do set "NOW=%%D"
goto :eof

rem ============================================================
rem  NEWPID / PIDSOURCE - see ON THE PID in the header
:resolvepid
set "NEWPID="
set "PIDSOURCE="
if not defined WANTPID goto :pidfromcaller
echo %WANTPID%| findstr /r "^[0-9][0-9]*$" >nul
if errorlevel 1 goto :pidbadarg
set "NEWPID=%WANTPID%"
set "PIDSOURCE=argument"
goto :eof

:pidbadarg
echo WARNING: --pid %WANTPID% is not a number. Falling back to the caller.

:pidfromcaller
for /f "usebackq delims=" %%P in (`powershell -NoProfile -Command "try{$p=(Get-CimInstance Win32_Process -Filter ('ProcessId='+$PID)).ParentProcessId; (Get-CimInstance Win32_Process -Filter ('ProcessId='+$p)).ParentProcessId}catch{}"`) do set "NEWPID=%%P"
if defined NEWPID set "PIDSOURCE=caller" & goto :eof
set "NEWPID=unknown"
set "PIDSOURCE=undetermined"
goto :eof

rem ============================================================
:usage
echo.
echo   lock.bat take    [--force] [--pid N] [root]
echo   lock.bat release                     [root]
echo   lock.bat status                      [root]
echo.
echo   root defaults to C:\Source\HamLet
echo   take    : 0 taken, 1 held, 2 usage, 3 write failed, 4 lost a race
echo   release : 0 removed, 1 not there, 2 usage, 3 remove failed
echo   status  : 0 free, 1 held, 2 usage, 5 stale
echo.
set "RC=2"
goto :end

rem ============================================================
:end
endlocal & exit /b %RC%
