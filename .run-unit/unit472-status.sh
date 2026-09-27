#!/bin/sh
# unit 471 - tools/status.sh always from the root.
# Usage: sh .run-unit/unit472-status.sh "<n of 4>" "<note>" [STATE] [BALL] [NEXT_PASTE]
cd /c/Source/HamLet || exit 1
sh tools/status.sh "${3:-EXECUTING}" "$1" "${4:-code}" "${5:-none}" "$2"
grep -E "^(STATE|TASK|WORK_INSTRUCTION|UPDATED|NOTE):" PROJECT_STATUS.md
