#!/bin/sh
# unit 458 - status through the project's own writer. Args: STATE TASK BALL NEXT_PASTE NOTE
cd /c/Source/HamLet || exit 1
sh tools/status.sh "$1" "$2" "$3" "$4" "$5"
sed -n '3,10p' PROJECT_STATUS.md
