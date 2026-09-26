#!/bin/sh
# unit 444 - build, then run the 17:37 gap printer alone.
# Usage: sh .run-unit/unit444-gaps.sh <suffix> "<n of 3>" "<note>"
cd /c/Source/HamLet || exit 1
sh .run-unit/unit444-build.sh "gaps-$1" "$2" "$3: building"
sh .run-unit/unit444-run.sh "gaps-$1" engine 600 "$2" "$3: printing every gap in 17:37's CQ" "FullyQualifiedName~.HowSeventeenThirtySevensGapsAreCalledTests." --no-build
grep -E "^\s*(text|marks|gap|total|read) \| " .run-unit/unit444-gaps-$1.txt | sed -E "s/^\s+//" > .run-unit/unit444-gaps-$1.table.txt
wc -l .run-unit/unit444-gaps-$1.table.txt
