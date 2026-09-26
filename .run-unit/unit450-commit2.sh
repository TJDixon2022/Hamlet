#!/bin/sh
# unit 450 - task 2 commit: tick 4.6 in both copies, the metrics line, the runs.
cd /c/Source/HamLet || exit 1
sh .run-unit/unit450-tick.sh 4.6
sh tools/status.sh COMPLETED "2 of 3" code none "State built and kept: text 63 of 63 identical, HM-REQ-093 green after red, 4.6 ticked; next the exit round"
sh .run-unit/unit450-commit.sh .run-unit/unit450-msg2.txt PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md docs/phase-requirements/metrics.md PROJECT_STATUS.md .run-unit/unit450-*
