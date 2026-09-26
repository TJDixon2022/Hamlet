#!/bin/sh
# unit 453 - the exit commit: printouts, report, status.
cd /c/Source/HamLet || exit 1
sh tools/status.sh COMPLETED "3 of 3" code none "Unit 453 done: 6.5 ticked, 0 of 3 measurable named spans met, WEEKEND and THINKING not in the audio, nothing built - output.md written"
sh .run-unit/unit453-commit.sh .run-unit/unit453-msg3.txt output.md PROJECT_STATUS.md docs/phase-requirements/metrics.md .run-unit/unit453-*.txt .run-unit/unit453-*.sh .run-unit/unit453-*.md
git status -sb | head -1
