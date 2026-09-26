#!/bin/sh
# unit 454 - task 0 commit: ENTRY line into both outcome copies, then the named paths.
cd /c/Source/HamLet || exit 1
cat .run-unit/unit454-entry-line.md >> PHASE_OUTCOME.md
cat .run-unit/unit454-entry-line.md >> docs/phase-requirements/PHASE_OUTCOME.md
sh tools/status.sh EXECUTING "1 of 4" code none "Task 0 done, numbers as 453 left them; task 1 blocked - the fldigi clone was refused, recording the request for the four files"
sh .run-unit/unit454-commit.sh .run-unit/unit454-msg0.txt Directory.Build.props PHASE_OUTCOME.md PHASE_STATUS.md RUN_LEDGER.md WORK_INSTRUCTIONS.md PROJECT_STATUS.md docs/phase-requirements/PHASE_OUTCOME.md docs/phase-requirements/PHASE_STATUS.md .run-unit/unit454-*
