#!/bin/sh
# unit 468 - tools/status.sh always from the root.
# Usage: sh .run-unit/unit470-status.sh "<n of 3>" "<note>" [STATE] [BALL]
cd /c/Source/HamLet || exit 1
sh tools/status.sh "${3:-EXECUTING}" "$1" "${4:-code}" none "$2"
grep -E "^(STATE|TASK|UPDATED|NOTE):" PROJECT_STATUS.md
