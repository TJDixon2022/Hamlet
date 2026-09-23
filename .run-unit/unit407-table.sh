#!/bin/sh
# unit 407 - one sorted row per capture from a detailed captures run: name chars elements unsure tone
# Usage: sh .run-unit/unit407-table.sh <outname> [<against-table-file>]
cd /c/Source/HamLet || exit 1
grep -E "^ [a-z/0-9-]+: [0-9]+ characters against a floor" ".run-unit/unit407-$1.txt" \
  | sed -E 's/^ ([^:]+): ([0-9]+) characters against a floor of ([0-9]+), ([0-9]+) elements against [0-9]+, ([0-9]+) unsure .* at ([0-9]+) Hz.*/\1 \2 \4 \5 \6 floor=\3/' \
  | sed 's#unadjudicated/##' | sort > ".run-unit/unit407-$1-table.txt"
wc -l < ".run-unit/unit407-$1-table.txt"
if [ -n "$2" ]; then
  echo "== diff against $2"
  diff "$2" ".run-unit/unit407-$1-table.txt" && echo "IDENTICAL"
fi
