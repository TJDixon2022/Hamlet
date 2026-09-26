#!/bin/sh
# unit 456 - task 0 commit: ENTRY line into both outcome copies, then the named paths.
cd /c/Source/HamLet || exit 1
cat .run-unit/unit456-entry-line.md >> PHASE_OUTCOME.md
cat .run-unit/unit456-entry-line.md >> docs/phase-requirements/PHASE_OUTCOME.md
sh tools/status.sh EXECUTING "1 of 5" code none "Task 0 done, numbers as 455 left them; task 1 tracing fldigi's receive call graph from cw::rx_process down"
git status --short | grep -v "^ M .run-unit/[a-z]" | head -30
sh .run-unit/unit456-commit.sh .run-unit/unit456-msg0.txt Directory.Build.props PHASE_OUTCOME.md PHASE_STATUS.md RUN_LEDGER.md WORK_INSTRUCTIONS.md PROJECT_STATUS.md docs/phase-requirements/PHASE_OUTCOME.md docs/phase-requirements/PHASE_STATUS.md .run-unit/unit456-*
git status --short -- .run-unit/fldigi
