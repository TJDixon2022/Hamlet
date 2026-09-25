#!/bin/sh
# unit 432 - write PROJECT_STATUS.md from the repository root.
# Usage: sh .run-unit/unit432-st.sh STATE "TASK n of 3" BALL "NEXT_PASTE" "NOTE"
cd /c/Source/HamLet || exit 1
sh tools/status.sh "$1" "$2" "$3" "$4" "$5"
grep -E "^(STATE|TASK|UPDATED|NOTE):" PROJECT_STATUS.md
