#!/bin/sh
# unit 423 - the final after-trace: panel bounds at the report's sizes, and every difference line.
cd /c/Source/HamLet/.run-unit || exit 1
f=unit423-type-t3f-WhereThePanelsStandOutsideHisPrivilegesTests.txt
for s in "CW 1100 x 780," "CW 1280 x 720," "CW 1366 x 728," "CW 1400 x 1040," "CW 1536 x 824," "CW 1920 x 1040,"
do
  awk -v s="==== $s" 'index($0, s) { on = 1; print; next } on && /====/ { on = 0 } on && (/-- covered|-- not covered|-- back|card left|map  left|rig  left/) { print }' "$f" | cut -c1-200
done
grep -E "^ (CW|Digital|connected) " "$f" | cut -c1-330
