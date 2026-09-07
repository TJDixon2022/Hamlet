@echo off
call "C:\Source\HamLet\tools\arbiter\run-unit.bat" 7 "C:\Source\HamLet" 
(echo %ERRORLEVEL%)> "C:\Source\HamLet\.run-unit\watched.rc"
