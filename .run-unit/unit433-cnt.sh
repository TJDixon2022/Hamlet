#!/bin/sh
# unit 433 - the stray trace's per-recording count rows, entry against a suffix, differences only.
# Usage: sh .run-unit/unit433-cnt.sh <suffix>
cd /c/Source/HamLet/.run-unit || exit 1
grep -m1 -E "^ count \| recording|^ count \| [a-z]+ \|" unit433-strays-entry.txt
grep -E "^ count \| (unadj|cw-)" unit433-strays-entry.txt > unit433-cnt-a.tmp
grep -E "^ count \| (unadj|cw-)" unit433-strays-$1.txt > unit433-cnt-b.tmp
diff unit433-cnt-a.tmp unit433-cnt-b.tmp
