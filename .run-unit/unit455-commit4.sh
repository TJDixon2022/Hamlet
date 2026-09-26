#!/bin/sh
# unit 455 - the exit commit: printouts, report, status.
cd /c/Source/HamLet || exit 1
sh tools/status.sh COMPLETED "4 of 4" code none "Unit 455 done: 6.2 ticked, 071 met on 5 of 9 prosigns (4 not in the table), 072 met by a naming switch, text byte-identical - output.md written"
sh .run-unit/unit455-commit.sh .run-unit/unit455-msg4.txt output.md PROJECT_STATUS.md .run-unit/unit455-*
git status -sb | head -1
