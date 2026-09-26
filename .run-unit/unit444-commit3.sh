#!/bin/sh
# unit 444 - closing commit: exit printouts, output.md, status COMPLETED.
cd /c/Source/HamLet || exit 1
sh tools/status.sh COMPLETED "3 of 3" tim "output.md -> Claude Web" "Unit 444 complete: G1's two lost spaces traced to a dropout; the dropout rule was not kept (17:37 boundaries 7 to 8), so 2.5 stays open"
sh .run-unit/unit444-commit.sh .run-unit/unit444-msg3.txt output.md PROJECT_STATUS.md .run-unit/unit444-*.txt .run-unit/unit444-*.sh .run-unit/unit444-*.md
git status -sb | head -1
