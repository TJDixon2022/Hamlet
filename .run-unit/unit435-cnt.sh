#!/bin/sh
# unit 435 - the stray trace's per-recording count rows, entry against a suffix, differences only.
# Usage: sh .run-unit/unit435-cnt.sh <suffix>
cd /c/Source/HamLet/.run-unit || exit 1
grep -m1 -E "^ count \| recording|^ count \| [a-z]+ \|" unit435-strays-entry.txt
grep -E "^ count \| (unadj|cw-)" unit435-strays-entry.txt > unit435-cnt-a.tmp
grep -E "^ count \| (unadj|cw-)" unit435-strays-$1.txt > unit435-cnt-b.tmp
diff unit435-cnt-a.tmp unit435-cnt-b.tmp
