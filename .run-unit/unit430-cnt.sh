#!/bin/sh
# unit 430 - the stray trace's per-recording count rows, entry against a suffix, differences only.
# Usage: sh .run-unit/unit430-cnt.sh <suffix>
cd /c/Source/HamLet/.run-unit || exit 1
grep -m1 -E "^ count \| recording|^ count \| [a-z]+ \|" unit430-strays-entry.txt
grep -E "^ count \| (unadj|cw-)" unit430-strays-entry.txt > unit430-cnt-a.tmp
grep -E "^ count \| (unadj|cw-)" unit430-strays-$1.txt > unit430-cnt-b.tmp
diff unit430-cnt-a.tmp unit430-cnt-b.tmp
