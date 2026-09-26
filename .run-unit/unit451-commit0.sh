#!/bin/sh
# unit 451 - task 0 commit: ENTRY line into both outcome copies, then the named paths.
cd /c/Source/HamLet || exit 1
cat .run-unit/unit451-entry-line.md >> PHASE_OUTCOME.md
cat .run-unit/unit451-entry-line.md >> docs/phase-requirements/PHASE_OUTCOME.md
sh tools/status.sh COMPLETED "0 of 3" code none "Entry done: metrics and floors as 450 left them, app line lost 3 all green alone; next the trace of every path that reports a speed"
sh .run-unit/unit451-commit.sh .run-unit/unit451-msg0.txt Directory.Build.props PARKED.md PHASE_OUTCOME.md PHASE_STATUS.md RUN_LEDGER.md WORK_INSTRUCTIONS.md PROJECT_STATUS.md docs/phase-requirements/PHASE_OUTCOME.md docs/phase-requirements/PHASE_STATUS.md .run-unit/unit451-*
