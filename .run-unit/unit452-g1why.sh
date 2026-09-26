#!/bin/sh
# unit 452 - for one group, each boundary's recording, time, gap and why the character gap was refused, then the reasons counted.
# Usage: sh .run-unit/unit452-g1why.sh <trace.txt> <G1|G2|...>
cd /c/Source/HamLet || exit 1
grep -a "^ *boundary | .* | $2 | " "$1" | awk -F' [|] ' '{ n = split($0, p, " [|] "); why = ""; for (i = 1; i <= n; i++) if (p[i] ~ /^character gap /) why = p[i]; printf "%s | %s | %s | %s | %s\n", $3, $6, $10, $2, why }' | cut -c1-300 > .run-unit/unit452-g1why.tmp
cat .run-unit/unit452-g1why.tmp
echo "--- reasons"
sed -E "s/.*character gap (refused[^:]*|measured).*/\\1/" .run-unit/unit452-g1why.tmp | sed -E "s/boundary [0-9.]+ u/boundary under 1.3 u/" | sort | uniq -c
echo "--- with a key-up under half a unit in the window"
grep -c "[1-9][0-9]* under half a unit" .run-unit/unit452-g1why.tmp
