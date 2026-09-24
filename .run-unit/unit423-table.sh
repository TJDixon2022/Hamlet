#!/bin/sh
# unit 423 - the three panels' bounds at covered and not covered, before (t1-b) and after, for the report.
cd /c/Source/HamLet/.run-unit || exit 1
for f in unit423-trace-t1-b.txt unit423-trace-after.txt
do
  echo "######## $f"
  for s in "CW 1100 x 780," "CW 1280 x 720," "CW 1366 x 728," "CW 1400 x 1040," "CW 1536 x 824," "CW 1920 x 1040,"
  do
    awk -v s="==== $s" 'index($0, s) { on = 1; print; next } on && /====/ { on = 0 } on && (/-- covered|-- not covered|card left|map  left|rig  left|row: /) { print }' "$f" | cut -c1-260
  done
done
