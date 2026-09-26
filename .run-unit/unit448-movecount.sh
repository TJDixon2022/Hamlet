#!/bin/sh
# unit 448 - per file, cold moves and those by level alone, from one acquiring trace; and each file's first move.
# Usage: sh .run-unit/unit448-movecount.sh <outname>
cd /c/Source/HamLet || exit 1
export LC_ALL=C
grep -a "^ *cold move | " .run-unit/unit448-$1.txt | sed -E "s/^ +//" | awk '!seen[$0]++' | awk -F' [|] ' '{ n[$2]++; if ($NF ~ /level alone/) a[$2]++; if (!($2 in f)) f[$2] = $3 " " $4 " | " $5 " | " $6 " | " $7 } END { for (k in n) printf "%s | %d moves, %d by level alone | first: %s\n", k, n[k], a[k] + 0, f[k] }' | sort
