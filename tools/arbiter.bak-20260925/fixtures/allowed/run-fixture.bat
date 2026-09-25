@echo off
setlocal
rem ============================================================
rem  run-fixture.bat  -  prove the allowed list belongs to the project
rem
rem      run-fixture.bat <keeps | creates>
rem
rem      0  the launcher treated the project's list the way it must
rem      1  it did not - the verdict line says what differed
rem      2  usage
rem
rem  Runs run-unit.bat --dry-run against a fresh git root under %TEMP%.
rem  A dry run resolves the scope, both tool lists, the prompt and the
rem  lock, and launches nothing - so no model is called and no money is
rem  spent. It does need claude on PATH, because the launcher checks.
rem
rem    keeps    the root already has .run-unit\allowed.txt, a project's own
rem             list. It MUST BE BYTE-IDENTICAL afterwards. UNTIL 063 THIS
rem             ARM WAS RED: run-unit.bat deleted the file on every run and
rem             rewrote it from its own folder's run-unit-tools.txt, which
rem             is how HamLet came to be handed this repository's list.
rem    creates  the root has none. The launcher MUST CREATE IT, and it must
rem             be byte-identical to the template, run-unit-tools.txt.
rem
rem  Generated 2026-09-13 for: work instructions 063 task 1
rem ============================================================

set "HERE=%~dp0"
set "ARM=%~1"
set "OK="
if /i "%ARM%"=="keeps" set "OK=1"
if /i "%ARM%"=="creates" set "OK=1"
if not defined OK goto :usage
for %%I in ("%HERE%..\..") do set "ARB=%%~fI\"

set "FROOT=%TEMP%\cps-allowed-fixture-%ARM%"
if exist "%FROOT%" rd /s /q "%FROOT%"
mkdir "%FROOT%"
git init -q "%FROOT%"
>"%FROOT%\PROJECT_CARD.md" echo PROJECT: allowed-fixture-%ARM%
if /i "%ARM%"=="keeps" mkdir "%FROOT%\.run-unit"
if /i "%ARM%"=="keeps" copy /y "%HERE%project-allowed.txt" "%FROOT%\.run-unit\allowed.txt" >nul

set "BEFORE=absent"
for /f "usebackq delims=" %%H in (`powershell -NoProfile -Command "$f='%FROOT%\.run-unit\allowed.txt'; if(Test-Path -LiteralPath $f){ (Get-FileHash -LiteralPath $f -Algorithm SHA256).Hash } else { 'absent' }"`) do set "BEFORE=%%H"

echo.
echo ============================================================
echo  allowed-list fixture : %ARM%
echo  root                 : %FROOT%
echo  allowed.txt before   : %BEFORE%
echo ============================================================

call "%ARB%run-unit.bat" fixture-%ARM% "%FROOT%" --dry-run
set "RRC=%ERRORLEVEL%"

echo.
echo ============================================================
echo  RESULT : %ARM%
echo ============================================================
powershell -NoProfile -Command "$f='%FROOT%\.run-unit\allowed.txt'; $tpl='%ARB%run-unit-tools.txt'; $before='%BEFORE%'; $after='absent'; if(Test-Path -LiteralPath $f){ $after=(Get-FileHash -LiteralPath $f -Algorithm SHA256).Hash }; $th=(Get-FileHash -LiteralPath $tpl -Algorithm SHA256).Hash; 'run-unit exit : %RRC%   (must be 0)'; 'before        : ' + $before; 'after         : ' + $after; 'template      : ' + $th; if('%ARM%' -eq 'keeps'){ $ok=('%RRC%' -eq '0') -and ($before -ne 'absent') -and ($after -eq $before); 'must          : after equals before - the project list left byte-identical' } else { $ok=('%RRC%' -eq '0') -and ($before -eq 'absent') -and ($after -eq $th); 'must          : created, and equal to the template' }; ''; 'verdict       : ' + $(if($ok){ 'PASS' } else { 'FAIL' }); if($ok){ exit 0 } else { exit 1 }"
set "RC=%ERRORLEVEL%"
goto :end

:usage
echo.
echo   run-fixture.bat ^<keeps ^| creates^>
echo.
set "RC=2"

:end
endlocal & exit /b %RC%
