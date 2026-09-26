#!/bin/sh
# unit 444 - task 0 commit: ENTRY line into both outcome copies, then the named paths.
cd /c/Source/HamLet || exit 1
cat .run-unit/unit444-entry-line.md >> PHASE_OUTCOME.md
cat .run-unit/unit444-entry-line.md >> docs/phase-requirements/PHASE_OUTCOME.md
sh tools/status.sh EXECUTING "0 of 3" code none "Entry round done, 17:37 boundaries wrong 7 and floor red at 38 as recorded; committing the entry"
sh .run-unit/unit444-commit.sh .run-unit/unit444-msg0.txt Directory.Build.props PHASE_OUTCOME.md PHASE_STATUS.md RUN_LEDGER.md WORK_INSTRUCTIONS.md PROJECT_STATUS.md docs/phase-requirements/PHASE_OUTCOME.md docs/phase-requirements/PHASE_STATUS.md .run-unit/unit444-*
