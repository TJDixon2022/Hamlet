#!/bin/sh
# unit 455 - task 0 commit: ENTRY line into both outcome copies, then the named paths.
cd /c/Source/HamLet || exit 1
cat .run-unit/unit455-entry-line.md >> PHASE_OUTCOME.md
cat .run-unit/unit455-entry-line.md >> docs/phase-requirements/PHASE_OUTCOME.md
sh tools/status.sh EXECUTING "1 of 4" code none "Task 0 done, numbers as 454 left them; task 1 tracing each prosign's pattern, table entry and decoded text on prosigns-18wpm"
sh .run-unit/unit455-commit.sh .run-unit/unit455-msg0.txt Directory.Build.props PHASE_OUTCOME.md PHASE_STATUS.md RUN_LEDGER.md WORK_INSTRUCTIONS.md PROJECT_STATUS.md docs/phase-requirements/PHASE_OUTCOME.md docs/phase-requirements/PHASE_STATUS.md .run-unit/unit455-*
