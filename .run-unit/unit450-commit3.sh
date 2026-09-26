#!/bin/sh
# unit 450 - exit commit: the report, the exit runs, the status.
cd /c/Source/HamLet || exit 1
sh tools/status.sh COMPLETED "3 of 3" code none "Unit 450 done: pitch proof state built, 4.6 ticked, text 63 of 63 identical - output.md written"
sh .run-unit/unit450-commit.sh .run-unit/unit450-msg3.txt output.md PROJECT_STATUS.md .run-unit/unit450-*.txt .run-unit/unit450-*.sh .run-unit/unit450-*.md
git status --short | grep -v "^??" | grep -v ".run-unit/" | head
