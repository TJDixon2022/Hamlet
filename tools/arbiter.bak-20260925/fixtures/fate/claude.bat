@echo off
rem ============================================================
rem  FIXTURE - NOT CLAUDE. Stands in for the STATE JUDGE that
rem  run-phase.bat's :judgestate calls, found first on PATH by
rem  run-fixture.bat. It reads its prompt from stdin, as the judge
rem  does, and answers with a MULTI-SENTENCE verdict whose WHY runs
rem  onto a second line - the shape of the answer HamLet's judge gave
rem  unit 331, whose first sentence ended up in the fate field.
rem
rem  Generated 2026-09-12 for: work instructions 061 task 3
rem ============================================================
findstr "^" >nul
echo {"type":"result","subtype":"success","is_error":false,"terminal_reason":"completed","num_turns":1,"permission_denials":[],"total_cost_usd":0.01,"result":"STATE: blocked\nWHY: The step's only exit criterion is Tim's own verdict at the radio, and the report states that he has not given it.\nNo unit can close it without him, so more effort will not help."}
exit /b 0
