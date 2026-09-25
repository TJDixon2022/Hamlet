#!/bin/sh
# unit 430 - each row's named, element and placeholder counts at a suffix against entry; the below-bar characters listed.
# Usage: sh .run-unit/unit430-rows.sh <suffix>
cd /c/Source/HamLet || exit 1
R=.run-unit/unit430
grep -oE "[a-z/0-9-]+: [0-9]+ named against a floor of [0-9]+, [0-9]+ named elements against [0-9]+, [0-9]+ placeholders" $R-captures-entry.txt \
  | sort -u | awk '{ print $1, $2+0, $9+0, $14+0 }' > $R-rows-entry.tmp
grep -oE "[a-z/0-9-]+: [0-9]+ named, [0-9]+ at or above the span bar against a floor of [0-9]+, [0-9]+ below it; [0-9]+ named elements, [0-9]+ above the bar against [0-9]+; [0-9]+ placeholders" $R-captures-$1.txt \
  | sort -u | awk '{ print $1, $2+0, $19+0, $28+0 }' > $R-rows-$1.tmp
if cmp -s $R-rows-entry.tmp $R-rows-$1.tmp
then
  echo "named, elements and placeholders identical to entry on all $(wc -l < $R-rows-entry.tmp) rows"
else
  echo "rows DIFFER from entry"
  diff $R-rows-entry.tmp $R-rows-$1.tmp
fi
echo "== below the bar, per row"
grep -E "^\s+[a-z/0-9-]+: [0-9]+ named, |below the bar:" $R-captures-$1.txt | sed -E "s/^\s+//; s/ named, .*//"
