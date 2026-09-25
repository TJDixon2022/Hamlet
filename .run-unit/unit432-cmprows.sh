#!/bin/sh
# unit 432 - each row at a change suffix against task 2's: the change's named count against task 2's above-bar count.
# Usage: sh .run-unit/unit432-cmprows.sh <before-suffix> <after-suffix>
cd /c/Source/HamLet || exit 1
R=.run-unit/unit432
P="[a-z/0-9-]+: [0-9]+ named, [0-9]+ at or above the span bar against a floor of [0-9]+, [0-9]+ below it; [0-9]+ named elements, [0-9]+ above the bar against [0-9]+; [0-9]+ placeholders"
grep -oE "$P" $R-captures-$1.txt | sort -u | awk '{ print $1, $2+0, $4+0, $16+0, $19+0, $22+0, $28+0 }' > $R-cmprows-a.tmp
grep -oE "$P" $R-captures-$2.txt | sort -u | awk '{ print $1, $2+0, $4+0, $16+0, $19+0, $22+0, $28+0 }' > $R-cmprows-b.tmp
join $R-cmprows-a.tmp $R-cmprows-b.tmp | awk '{
  s = "same";
  if ($8 != $3 || $9 != $3 || $10 != 0 || $12 != $6) s = "CHECK";
  if ($9 < $3) s = "ABOVE-BAR-FELL";
  print s, $1, "before named", $2, "above", $3, "below", $4, "| after named", $8, "above", $9, "below", $10, "| above elements", $6, "to", $12, "| placeholders", $7, "to", $13;
  c[s]++; r += $4 } END { for (k in c) print "count", k, c[k]; print "below-bar before", r }'
