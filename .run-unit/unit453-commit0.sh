#!/bin/sh
# unit 453 - task 0 commit: ENTRY line into both outcome copies, then the named paths.
cd /c/Source/HamLet || exit 1
cat .run-unit/unit453-entry-line.md >> PHASE_OUTCOME.md
cat .run-unit/unit453-entry-line.md >> docs/phase-requirements/PHASE_OUTCOME.md
sh tools/status.sh COMPLETED "0 of 3" code none "Entry done: numbers as 452 left them, text identical; next each named span measured and every mark and gap under it traced"
sh .run-unit/unit453-commit.sh .run-unit/unit453-msg0.txt Directory.Build.props PHASE_OUTCOME.md PHASE_STATUS.md RUN_LEDGER.md WORK_INSTRUCTIONS.md PROJECT_STATUS.md docs/phase-requirements/PHASE_OUTCOME.md docs/phase-requirements/PHASE_STATUS.md .run-unit/unit453-*
