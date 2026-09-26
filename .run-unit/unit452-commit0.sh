#!/bin/sh
# unit 452 - task 0 commit: ENTRY line into both outcome copies, then the named paths.
cd /c/Source/HamLet || exit 1
cat .run-unit/unit452-entry-line.md >> PHASE_OUTCOME.md
cat .run-unit/unit452-entry-line.md >> docs/phase-requirements/PHASE_OUTCOME.md
sh tools/status.sh COMPLETED "0 of 3" code none "Entry done: WBE 46 over 113 real and 48 over 84 synthetic, floors as 451 left them; next the per-condition split and the trace of every wrong boundary"
sh .run-unit/unit452-commit.sh .run-unit/unit452-msg0.txt Directory.Build.props PARKED.md PHASE_OUTCOME.md PHASE_STATUS.md RUN_LEDGER.md WORK_INSTRUCTIONS.md PROJECT_STATUS.md docs/phase-requirements/PHASE_OUTCOME.md docs/phase-requirements/PHASE_STATUS.md .run-unit/unit452-*
