#!/bin/sh
# unit 465 - live path totals per set from the live printout: builds, retunes, and spans per case.
cd /c/Source/HamLet || exit 1
grep -a "^ *live | " .run-unit/unit465-live-$1.txt | grep -v "| recording |" | sed 's/^ *//' | awk -F' [|] ' '
{ set = ($2 ~ /^cq-/) ? "synthetic" : "real"; n[set]++; c[set]+=$3; r[set]+=$4; s[set]+=$5; m[set]+=$6; a[set]+=$7; d[set]+=$8; t[set]+=$9; o[set]+=$10; p[set]+=$11 }
END { for (k in n) printf "livesum | %s | recordings %d | constructions %d | retunes %d | after skips %d | memory rebuilds %d | agree %d | disagree %d | tie %d | one-sided ours %d | one-sided port %d\n", k, n[k], c[k], r[k], s[k], m[k], a[k], d[k], t[k], o[k], p[k] }'
