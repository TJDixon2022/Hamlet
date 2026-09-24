#!/bin/sh
# unit 421 - each capture row's named and element counts against its floors, from a captures output.
# Usage: sh .run-unit/unit421-atfloor.sh <outname>
cd /c/Source/HamLet || exit 1
grep -oE "[a-z/0-9-]+: [0-9]+ named against a floor of [0-9]+, [0-9]+ named elements against [0-9]+, [0-9]+ placeholders" .run-unit/unit421-$1.txt \
  | sort -u \
  | awk '{ n=$2+0; f=$8+0; e=$9+0; ef=$13+0; p=$14+0; s=(n==f)?"AT":"ABOVE"; if (n<f) s="BELOW"; print s, $1, "named", n, "floor", f, "elements", e, "floor", ef, "placeholders", p; c[s]++ } END { for (k in c) print "count", k, c[k] }'
