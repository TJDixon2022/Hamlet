@echo off
rem ============================================================
rem  gate-set.bat  -  run EXACTLY the gate set and nothing else.
rem
rem      gate-set.bat [repository root]
rem
rem      0  every gate passed
rem      1  A GATE FAILED - the project is named above
rem      2  the root is wrong, or a project is missing
rem
rem  WHAT THIS IS. docs\gate-set.md is the list and the reasons;
rem  this is the command. The list lives in ONE place and this
rem  script is the other half of it - if you add an entry there
rem  without adding it to a filter here, the document describes a
rem  gate that does not run. There is no way to check that
rem  mechanically and the rule is stated instead: BOTH FILES OR
rem  NEITHER.
rem
rem  WHY IT IS FOUR INVOCATIONS AND NOT ONE. The gate set spans
rem  four test projects and this loop's standing rule is ONE
rem  PROJECT AT A TIME, NEVER CONCURRENTLY. Contention once turned
rem  one standing failure into five, on 2026-08-31, and the
rem  measurement it poisoned is still labelled as unreliable in
rem  docs\test-baseline.md. Four builds is the price of a number
rem  anyone can believe.
rem
rem  IT NEVER RUNS Hamlet.App.Tests UNFILTERED. That project stops
rem  partway when run whole; docs\full-suite-run.md carries the
rem  four filtered commands for it. The filter below is two tests.
rem
rem  IT WRITES A TRX PER PROJECT into .run-unit\trx\, because the
rem  console logger prints a total and nothing else, and the
rem  console logs in this tree are UTF-16 - grepping them as UTF-8
rem  finds nothing and reports zero, which is how a suite came to
rem  have no total in four consecutive reports. A red gate is read
rem  afterwards with:
rem
rem      python tools\arbiter\trx-rank.py .run-unit\trx\gate-<n>.trx
rem
rem  THE TARGET IS UNDER THREE MINUTES and it is a WAYPOINT, not a
rem  gate on this script. A slow gate set that guards the right
rem  things beats a fast one that does not, and no entry was
rem  dropped to reach a number. The elapsed time is printed every
rem  run so a later unit can see it drift.
rem
rem  ONE EXIT POINT, as lock.bat, layer-check.bat and
rem  validate-output.bat. %~dp0 is captured before any shift.
rem
rem  Written by work instruction 250.
rem ============================================================
setlocal EnableExtensions EnableDelayedExpansion
set "GSHERE=%~dp0"

set "ROOT=%~1"
if "%ROOT%"=="" for %%R in ("%GSHERE%..\..") do set "ROOT=%%~fR"
if "%ROOT:~-1%"=="\" set "ROOT=%ROOT:~0,-1%"

set "RC=0"
set "NRUN=0"
set "NRED=0"
set "REDLIST="
set "TRX=%ROOT%\.run-unit\trx"

if not exist "%ROOT%\Hamlet.sln" (
  echo ERROR: no Hamlet.sln at %ROOT% - this is not the Hamlet repository.
  set "RC=2"
  goto :end
)
if not exist "%TRX%\" mkdir "%TRX%" >nul 2>&1

echo.
echo ============================================================
echo  THE GATE SET - docs\gate-set.md names every entry and the
echo  breakage it would have caught. An entry that cannot name one
echo  does not belong in it.
echo    root   : %ROOT%
echo    trx    : %TRX%
echo    target : under 3 minutes, a waypoint and not a gate
echo ============================================================

call :clock START

rem ============================================================
rem  1  Ft8Sharp.Tests
rem     Ft8SharpBoundaryTests   the port references nothing outside itself
rem     Ft8DeepIdentityTests    Deep is a superset - whole-result identity,
rem                             69 reference recordings, 801 messages
rem     Unit222TraceTests       the ladder reports ZERO WRONG at 306 trials
rem ============================================================
call :gate "1" "tests\Ft8Sharp.Tests" ^
  "FullyQualifiedName~Ft8SharpBoundaryTests|FullyQualifiedName~Ft8DeepIdentityTests|FullyQualifiedName~Unit222TraceTests"

rem ============================================================
rem  2  Ft8Sharp.Deep.Tests
rem     the boundary in the breaching direction; both sides of the
rem     codeword seam; the two submission bounds that make ZERO
rem     WRONG an arithmetic rather than a hope; and OSD off by
rem     default, which is unit 246's ruling 4.
rem
rem     NAMED ONE BY ONE, not by class. Running these classes whole
rem     would drag in the whole-type-list tripwire, which is
rem     known-red by design whenever a type is added to Deep.
rem ============================================================
call :gate "2" "tests\Ft8Sharp.Deep.Tests" ^
  "FullyQualifiedName~Ft8DeepBoundaryTests.ThePortsBuiltAssemblyDoesNotReferenceTheSibling|FullyQualifiedName~Ft8DeepBoundaryTests.NoHamletAssemblyArrivesInEitherAssembly|FullyQualifiedName~Ft8DeepGateTests.ARightOsdCodewordComesBackThroughThePortAsTheMessage|FullyQualifiedName~Ft8DeepSeamProbeTests.AWrongCodewordHandedBackIsStillRefused|FullyQualifiedName~Ft8DeepCombineGateTests.ADeliberatelyWrongPairingIsRefusedByThePortsOwnGates|FullyQualifiedName~Ft8DeepFineSyncGateTests.WithEverythingOffTheWholeResultIsThePortsWholeResult|FullyQualifiedName~Ft8DeepFineSyncGateTests.EveryMessageTheOrdinaryPathReturnedIsStillThere|FullyQualifiedName~Ft8DeepFineSyncGateTests.TheSubmissionArithmeticIsBoundedAtOnePerRefusedCandidate|FullyQualifiedName~Ft8DeepRepeatDecoderTests.TheSubmissionsSpentNeverExceedTheBudgetTheSettingsBound|FullyQualifiedName~Ft8DeepSlotDecoderTests.OrderedStatisticsIsOffUnlessItIsAskedFor"

rem ============================================================
rem  3  Hamlet.RadioEngine.Tests
rem     the port's gates in HAMLET's path, not just the sibling's;
rem     the decoder's identity on the slot and on the sidecar;
rem     the five-count census on the sheet and the census line;
rem     and one slot inside the 15-second budget.
rem ============================================================
call :gate "3" "tests\Hamlet.RadioEngine.Tests" ^
  "FullyQualifiedName~HamletDecodesThroughDeepTests|FullyQualifiedName~ACaptureSaysWhichDecoderReadItTests|FullyQualifiedName~TheSheetSaysWhichAudioPathItRanOnTests.TheCensusNamesTheStageEachSlotReached|FullyQualifiedName~TheSheetSaysWhichAudioPathItRanOnTests.ASheetWithNoDecodeBehindItSaysTheCensusWasNotRead|FullyQualifiedName~ACapturedFileDiagnosesItselfTests.AFileOnDiskComesBackWithACensusThatNamesEveryStage"

rem ============================================================
rem  4  Hamlet.App.Tests - THE THIRD SURFACE, and the only reason
rem     this project is in the gate set at all. The census reaches
rem     the slot telemetry line here and nowhere else.
rem     TWO TESTS. NEVER UNFILTERED.
rem ============================================================
call :gate "4" "tests\Hamlet.App.Tests" ^
  "FullyQualifiedName~EverySlotLeavesALineTests.EverySlotInAReadingGetsItsOwnLine|FullyQualifiedName~EverySlotLeavesALineTests.ASlotThatDecodedNothingStillWritesItsCensus"

call :clock STOP
call :elapsed

echo.
echo ============================================================
echo  %NRUN% of 4 projects run, %NRED% red.
if %NRED% GTR 0 (
  echo.
  echo  A GATE FAILED:%REDLIST%
  echo.
  echo  Read it per test:
  echo    python tools\arbiter\trx-rank.py .run-unit\trx\gate-^<n^>.trx
  echo.
  echo  THE GATE SET IS NOT ADVISORY. Every entry names a breakage
  echo  that actually happened - docs\gate-set.md says which. A red
  echo  one is that breakage coming back, not noise to be re-run.
  set "RC=1"
) else (
  echo.
  echo  EVERY GATE PASSED.
  echo.
  echo  THIS IS NOT "THE SUITE IS GREEN". It is the short list of
  echo  properties this phase must not break. A unit still runs the
  echo  channels it touched, whole, one project at a time.
)
echo ============================================================
goto :end

rem ============================================================
rem  One project. %~1 ordinal, %~2 project path, %~3 filter.
rem ============================================================
:gate
set "GN=%~1"
set "GPROJ=%~2"
set "GFILTER=%~3"

if not exist "%ROOT%\%GPROJ%\" (
  echo.
  echo   gate %GN%: %GPROJ% IS NOT IN THIS TREE. Nothing was run and
  echo   nothing is claimed - this is not a pass.
  set "RC=2"
  goto :eof
)

echo.
echo ------------------------------------------------------------
echo  gate %GN% : %GPROJ%
echo ------------------------------------------------------------

set /a NRUN+=1

dotnet test "%ROOT%\%GPROJ%" --filter "%GFILTER%" --nologo -v:q ^
  --logger "trx;LogFileName=gate-%GN%.trx" --results-directory "%TRX%"

if errorlevel 1 (
  set /a NRED+=1
  set "REDLIST=!REDLIST! %GPROJ%"
  echo   gate %GN% RED
) else (
  echo   gate %GN% green
)
goto :eof

rem ============================================================
rem  The clock. Centiseconds since midnight, in pure cmd - no
rem  PowerShell, because this script has to run on a session whose
rem  shell refuses it and the elapsed time is the point of task 3.
rem  %TIME% is locale-shaped; the tokens are taken positionally and
rem  a leading space on the hour is tolerated.
rem ============================================================
:clock
set "T=%TIME: =0%"
set /a "CS=(1%T:~0,2%-100)*360000 + (1%T:~3,2%-100)*6000 + (1%T:~6,2%-100)*100 + (1%T:~9,2%-100)"
set "%~1=%CS%"
goto :eof

:elapsed
set /a "D=%STOP%-%START%"
if %D% LSS 0 set /a "D=%D%+8640000"
set /a "MM=%D%/6000"
set /a "SS=(%D%%%6000)/100"
if %SS% LSS 10 set "SS=0%SS%"
echo.
echo   WALL CLOCK, WHOLE COMMAND : %MM% m %SS% s
echo   the target is under 3 minutes and it is a waypoint. If this
echo   has drifted, say what it is now rather than dropping a test
echo   that earned its place.
goto :eof

:end
echo.
echo gate-set exit %RC%
endlocal & exit /b %RC%
