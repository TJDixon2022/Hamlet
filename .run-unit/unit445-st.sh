#!/bin/sh
# unit 445 - status from the root. Usage: sh .run-unit/unit445-st.sh <STATE> "<n of 3>" "<note>"
cd /c/Source/HamLet || exit 1
sh tools/status.sh "$1" "$2" code none "$3"
grep "^STATE\|^TASK\|^UPDATED" PROJECT_STATUS.md
