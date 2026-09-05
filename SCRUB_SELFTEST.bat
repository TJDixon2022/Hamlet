@echo off
rem  Proves :scrub before the loop depends on it.
rem  Run from anywhere:  SCRUB_SELFTEST.bat
setlocal
set "T=he said "hello" and left"
echo   before : %T%
call :scrub T
echo   after  : %T%
echo(%T%| findstr /c:"\"" >nul && (echo   FAIL - a double quote survived & exit /b 1)
echo(%T%| findstr /c:"hello" >nul || (echo   FAIL - the text was lost & exit /b 1)
echo   PASS - quotes replaced, text intact
set "T="
call :scrub T
echo   empty  : [%T%]  PASS if this line printed at all
exit /b 0

:scrub
setlocal enabledelayedexpansion
set "V=!%~1!"
if not defined V ( endlocal & goto :eof )
set "V=!V:"='!"
endlocal & set "%~1=%V%"
goto :eof
