#!/bin/sh
# unit 447 - task 0 commit: ENTRY line into both outcome copies, then the named paths.
cd /c/Source/HamLet || exit 1
cat .run-unit/unit447-entry-line.md >> PHASE_OUTCOME.md
cat .run-unit/unit447-entry-line.md >> docs/phase-requirements/PHASE_OUTCOME.md
sh tools/status.sh COMPLETED "0 of 4" code none "Entry round done: metrics as 446 left them, 17:37 red as recorded; committing before the instrument"
sh .run-unit/unit447-commit.sh .run-unit/unit447-msg0.txt Directory.Build.props PHASE_OUTCOME.md PHASE_STATUS.md RUN_LEDGER.md WORK_INSTRUCTIONS.md PROJECT_STATUS.md docs/phase-requirements/PHASE_OUTCOME.md docs/phase-requirements/PHASE_STATUS.md .run-unit/unit447-*
