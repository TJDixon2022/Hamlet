#!/bin/sh
# unit 445 - task 0 commit: ENTRY line into both outcome copies, then the named paths.
cd /c/Source/HamLet || exit 1
cat .run-unit/unit445-entry-line.md >> PHASE_OUTCOME.md
cat .run-unit/unit445-entry-line.md >> docs/phase-requirements/PHASE_OUTCOME.md
sh tools/status.sh EXECUTING "0 of 3" code none "Entry round done, MET-INVENTED 47 over 473 and floors as recorded; committing the entry"
sh .run-unit/unit445-commit.sh .run-unit/unit445-msg0.txt Directory.Build.props PHASE_OUTCOME.md PHASE_STATUS.md RUN_LEDGER.md WORK_INSTRUCTIONS.md PROJECT_STATUS.md docs/phase-requirements/PHASE_OUTCOME.md docs/phase-requirements/PHASE_STATUS.md .run-unit/unit445-*
