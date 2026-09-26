#!/bin/sh
# unit 452 - task 1's two commits: the printer and its printout, then the measurement and the tick on their own.
cd /c/Source/HamLet || exit 1
sh tools/status.sh EXECUTING "1 of 3" code none "Committing the trace printer, then the measurement in metrics.md with 6.3 ticked"
sh .run-unit/unit452-commit.sh .run-unit/unit452-msg1a.txt tests/Hamlet.RadioEngine.Tests/Cw/WhereTheWordBoundariesGoWrongTests.cs PROJECT_STATUS.md .run-unit/unit452-*
sh tools/status.sh COMPLETED "1 of 3" code none "Measured: WBE 46 over 113 real, 48 over 84 synthetic, 080 and 081 not met where measurable, 20 conditions not measurable; 6.3 ticked; next one change against G1"
sh .run-unit/unit452-commit.sh .run-unit/unit452-msg1b.txt docs/phase-requirements/metrics.md PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md PROJECT_STATUS.md
