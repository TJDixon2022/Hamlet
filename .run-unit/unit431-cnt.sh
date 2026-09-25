#!/bin/sh
# unit 431 - the stray trace's per-recording count rows, entry against a suffix, differences only.
# Usage: sh .run-unit/unit431-cnt.sh <suffix>
cd /c/Source/HamLet/.run-unit || exit 1
grep -m1 -E "^ count \| recording|^ count \| [a-z]+ \|" unit431-strays-entry.txt
grep -E "^ count \| (unadj|cw-)" unit431-strays-entry.txt > unit431-cnt-a.tmp
grep -E "^ count \| (unadj|cw-)" unit431-strays-$1.txt > unit431-cnt-b.tmp
diff unit431-cnt-a.tmp unit431-cnt-b.tmp
