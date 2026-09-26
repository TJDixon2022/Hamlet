#!/bin/sh
# unit 452 - per wrong boundary: kind, recording, time, gap in units, what the relabel did, and why the character gap was or was not measured.
# Usage: sh .run-unit/unit452-why.sh <trace.txt>
cd /c/Source/HamLet || exit 1
grep -a "^ *boundary | \(inserted\|deleted\) " "$1" | awk -F' [|] ' '{ n = split($0, p, " [|] "); why = ""; for (i = 1; i <= n; i++) if (p[i] ~ /^character gap /) why = p[i]; printf "%s | %s | %s | %s | %s | %s | %s\n", $2, $3, $5, $9, $10, $13, why }' | cut -c1-420
