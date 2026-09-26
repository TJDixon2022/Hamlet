#!/bin/sh
# unit 452 - task 2's commit: the kept figures and the judging printouts.
cd /c/Source/HamLet || exit 1
export LC_ALL=C
diff .run-unit/unit452-text-before.sorted.txt .run-unit/unit452-text-change.sorted.txt > .run-unit/unit452-text-diff.txt
sh .run-unit/unit452-v11.sh .run-unit/unit452-metrics-entry.txt .run-unit/unit452-metrics-change.txt > .run-unit/unit452-v11-change.txt
sh tools/status.sh COMPLETED "2 of 3" code none "Change kept: WBE 46 to 37 real, 48 to 44 synthetic, letters identical, V-11 0 worse, 17:37 at 7; next the exit round"
sh .run-unit/unit452-commit.sh .run-unit/unit452-msg2.txt docs/phase-requirements/metrics.md PROJECT_STATUS.md .run-unit/unit452-*.txt .run-unit/unit452-*.sh .run-unit/unit452-*.md
