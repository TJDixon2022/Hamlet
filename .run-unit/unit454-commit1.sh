#!/bin/sh
# unit 454 - stop commit: status BLOCKED, the report, the scripts.
cd /c/Source/HamLet || exit 1
sh tools/status.sh BLOCKED "1 of 4" tim none "Stopped at task 1: fldigi clone refused in the session - place its source under .run-unit/fldigi, then rerun 454; output.md written"
sh .run-unit/unit454-commit.sh .run-unit/unit454-msg1.txt output.md PROJECT_STATUS.md .run-unit/unit454-*
