#!/bin/sh
# unit 457 - count the fidelity file's audited rows, by mark and by block.
cd /c/Source/HamLet || exit 1
F=.run-unit/unit457-fidelity.txt
echo "rows:"
grep -cE "^  [ABC][0-9]+\.[0-9]+ +\|" $F
echo "by mark (last field):"
grep -E "^  [ABC][0-9]+\.[0-9]+ +\|" $F | sed -E "s/.*\| *//" | cut -c1-9 | sort | uniq -c
echo "by block:"
grep -E "^  [ABC][0-9]+\.[0-9]+ +\|" $F | sed -E "s/^  ([ABC][0-9]+)\..*/\1/" | sort | uniq -c
