#!/bin/sh
# unit 471 - dim precision and the 7.052 opening from 470's printer, before and after.
# Usage: sh .run-unit/unit471-printed.sh <suffix>
cd /c/Source/HamLet || exit 1
export LC_ALL=C
F=.run-unit/unit471-printed-$1.txt
echo "== $1 dim precision"
grep -a -E "^ *dim \|" "$F" | cut -c1-200
echo "== $1 opening"
grep -a -E "^ *(opening|hm-req-102) \|" "$F" | cut -c1-900
echo "== $1 acquiring totals"
grep -a -E "^ *total \|" "$F" | cut -c1-400
