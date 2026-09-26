#!/bin/sh
# unit 450 - task 0 commit: ENTRY line into both outcome copies, then the named paths.
cd /c/Source/HamLet || exit 1
cat .run-unit/unit450-entry-line.md >> PHASE_OUTCOME.md
cat .run-unit/unit450-entry-line.md >> docs/phase-requirements/PHASE_OUTCOME.md
sh tools/status.sh COMPLETED "0 of 3" code none "Entry done: metrics and floors as 449 left them, app line lost 2 both green alone; next the trace of every path that holds a pitch"
sh .run-unit/unit450-commit.sh .run-unit/unit450-msg0.txt Directory.Build.props PHASE_OUTCOME.md PHASE_STATUS.md RUN_LEDGER.md WORK_INSTRUCTIONS.md PROJECT_STATUS.md docs/phase-requirements/PHASE_OUTCOME.md docs/phase-requirements/PHASE_STATUS.md .run-unit/unit450-*
