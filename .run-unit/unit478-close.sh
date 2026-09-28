#!/bin/sh
# unit 478 task 3 - stamp output.md's UNIT line from the clock, keep a copy, write the final status.
cd /c/Source/HamLet || exit 1
NOW=$(date '+%Y-%m-%d %H:%M')
sed -i "s/^\(UNIT:       478 - .*\) - DATETIME$/\1 - $NOW/" output.md
grep -n "^UNIT:" output.md
grep -c "DATETIME" output.md
cp output.md .run-unit/unit478-output.md
sh .run-unit/unit478-status.sh COMPLETED "3 of 3" tim "output.md -> Claude Web" "Unit 478 done: the scope is a level trace with bars under the marks and tone/mixing words, no floor or threshold; light and strip off the CW tab, buttons beside the scope, row keys unchanged; three retired files emptied because git rm was refused - owner deletes; no recording read"
