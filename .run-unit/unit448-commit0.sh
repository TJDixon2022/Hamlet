#!/bin/sh
# unit 448 - task 0 commit: ENTRY line into both outcome copies, then the named paths.
cd /c/Source/HamLet || exit 1
cat .run-unit/unit448-entry-line.md >> PHASE_OUTCOME.md
cat .run-unit/unit448-entry-line.md >> docs/phase-requirements/PHASE_OUTCOME.md
sh tools/status.sh COMPLETED "0 of 3" code none "Entry done: 4.3 ticked, metrics as 447 left them, 9 files over 25 Hz; next the acquiring trace"
sh .run-unit/unit448-commit.sh .run-unit/unit448-msg0.txt Directory.Build.props PHASE_OUTCOME.md PHASE_STATUS.md RUN_LEDGER.md WORK_INSTRUCTIONS.md PROJECT_STATUS.md output.md PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md docs/phase-requirements/PHASE_OUTCOME.md docs/phase-requirements/PHASE_STATUS.md .run-unit/unit448-*
