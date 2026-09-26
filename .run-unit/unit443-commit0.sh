#!/bin/sh
# unit 443 - task 0 commit: ENTRY line into both outcome copies, then the named paths.
cd /c/Source/HamLet || exit 1
cat .run-unit/unit443-entry-line.md >> PHASE_OUTCOME.md
cat .run-unit/unit443-entry-line.md >> docs/phase-requirements/PHASE_OUTCOME.md
sh tools/status.sh EXECUTING "0 of 3" code none "Entry round done: 4 sure added at HEAD, 17:37 red as recorded; committing the entry"
sh .run-unit/unit443-commit.sh .run-unit/unit443-msg0.txt Directory.Build.props PHASE_OUTCOME.md PHASE_STATUS.md RUN_LEDGER.md PARKED.md WORK_INSTRUCTIONS.md PROJECT_STATUS.md docs/phase-requirements/PARKED.md docs/phase-requirements/PHASE_OUTCOME.md docs/phase-requirements/PHASE_STATUS.md .run-unit/unit443-*
