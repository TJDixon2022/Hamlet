@echo off
rem ============================================================
rem  FIXTURE - NOT CLAUDE. A stand-in the launcher runs as if it were
rem  claude -p, found first on PATH by run-fixture.bat.
rem
rem  BUSY AND SILENT. Computes for 25 minutes and writes no file at
rem  all - not PROJECT_STATUS.md, not anything - then writes a valid
rem  output.md and exits. Under the old rule, twelve minutes without a
rem  status write, this is killed at minute twelve. Under the new one
rem  it MUST SURVIVE and be judged.
rem
rem  THE WORK IS IN A CHILD PROCESS ON PURPOSE. This batch file sits
rem  idle while a PowerShell it started runs flat out, which is the
rem  shape of claude -p waiting on dotnet: a watchdog that measured
rem  only the parent would call this run dead.
rem
rem  Generated 2026-09-12 for: work instructions 061 task 1
rem ============================================================
powershell -NoProfile -Command "$sw=[Diagnostics.Stopwatch]::StartNew(); $n=0; while($sw.Elapsed.TotalMinutes -lt 25){ $n++ }"
copy /y "%~dp0..\fixture-output.md" "%CD%\output.md" >nul
echo {"type":"result","subtype":"success","is_error":false,"terminal_reason":"completed","num_turns":1,"permission_denials":[],"result":"fixture busy-silent: 25 minutes of CPU in a child, no file written until the report"}
exit /b 0
