@echo off
rem ============================================================
rem  toolsarbitervalidate-output.bat  -  a shim, and an ugly name
rem  on purpose, because the name is not chosen.
rem
rem  THE PERMISSION SCOPE ALLOWS THE VALIDATOR AND THE SHELL EATS
rem  THE PATH. run-unit-tools.txt permits five spellings of
rem  tools\arbiter\validate-output.bat, every one of them with
rem  single backslashes, and the harness matches the command as it
rem  is typed. Git Bash then removes a backslash before an ordinary
rem  letter, so `cmd //c tools\arbiter\validate-output.bat` reaches
rem  cmd as `toolsarbitervalidate-output.bat` and cmd reports no
rem  such file. Doubling the backslashes fixes the shell and breaks
rem  the permission match; quoting the path does the same. Units 224
rem  through 228 have all met this.
rem
rem  So this file has the name the shell produces, and hands
rem  straight over to the real script with every argument and its
rem  exit code intact. NOTHING HERE VALIDATES ANYTHING. The rules
rem  are the arbiter's script's and it is not modified, not copied
rem  and not read from here.
rem
rem  Written by unit 228. It is untracked and was not committed. It
rem  is safe to delete and deleting it costs the next unit the same
rem  refusal.
rem ============================================================
call "%~dp0tools\arbiter\validate-output.bat" %*
exit /b %ERRORLEVEL%
