#!/bin/sh
# unit 421 - each capture row under the span bar: named, above, below, the floor, elements and elements above, from a captures output.
# Usage: sh .run-unit/unit421-bar.sh <outname>
cd /c/Source/HamLet || exit 1
grep -oE "[a-z/0-9-]+: [0-9]+ named, [0-9]+ at or above the span bar against a floor of [0-9]+, [0-9]+ below it; [0-9]+ named elements, [0-9]+ above the bar against [0-9]+; [0-9]+ placeholders" .run-unit/unit421-$1.txt \
  | sort -u \
  | awk '{ n=$2+0; a=$4+0; f=$15+0; b=$16+0; e=$19+0; ea=$22+0; ef=$27+0; p=$28+0;
           s = (b > 0) ? "BELOW-BAR" : "clean"; if (a < f) s = s "-UNDER-FLOOR";
           print s, "|", $1, "| named", n, "| above", a, "| below", b, "| floor", f, "| elements", e, "| above", ea, "| floor", ef, "| placeholders", p;
           c[s]++; tb += b; tn += n; ta += a } END { for (k in c) print "count", k, c[k]; print "total named", tn, "above", ta, "below", tb }'
