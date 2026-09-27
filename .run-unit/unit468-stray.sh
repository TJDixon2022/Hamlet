#!/bin/sh
# unit 468 - move the PROJECT_STATUS.md that status.sh wrote into the fixtures folder (run from the wrong directory) out of tests.
cd /c/Source/HamLet || exit 1
mv tests/Hamlet.RadioEngine.Tests/Cw/Fixtures/PROJECT_STATUS.md .run-unit/unit468-stray-status.tmp
git status --short tests
sh .run-unit/unit468-status.sh "0 of 3" "Task 0 - entry round all as at 467's exit (178, 278, 13, 51, 13, 71, arbitration green); now tracing what timing the fixtures can already vary"
