#!/bin/sh
# unit 422 - write the root status file from the root.
# Usage: sh .run-unit/unit422-status.sh STATE "TASK n of m" BALL "NEXT_PASTE" "NOTE"
cd /c/Source/HamLet || exit 1
sh tools/status.sh "$1" "$2" "$3" "$4" "$5"
sed -n 1,10p PROJECT_STATUS.md
