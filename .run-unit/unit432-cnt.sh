#!/bin/sh
# unit 432 - the stray trace's per-recording count rows, entry against a suffix, differences only.
# Usage: sh .run-unit/unit432-cnt.sh <suffix>
cd /c/Source/HamLet/.run-unit || exit 1
grep -m1 -E "^ count \| recording|^ count \| [a-z]+ \|" unit432-strays-entry.txt
grep -E "^ count \| (unadj|cw-)" unit432-strays-entry.txt > unit432-cnt-a.tmp
grep -E "^ count \| (unadj|cw-)" unit432-strays-$1.txt > unit432-cnt-b.tmp
diff unit432-cnt-a.tmp unit432-cnt-b.tmp
