#!/bin/sh
# unit 452 - totals, groups and any condition where the printer and the metric disagree, from one trace printout.
# Usage: sh .run-unit/unit452-groups.sh <trace.txt>
cd /c/Source/HamLet || exit 1
grep -a "^ *total | " "$1" | cut -c1-200
grep -a "^ *group | " "$1" | cut -c1-260
echo "conditions where the printer and CwMetrics disagree: $(grep -ac " | NO\$" "$1")"
