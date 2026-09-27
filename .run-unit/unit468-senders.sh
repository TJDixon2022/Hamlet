#!/bin/sh
# unit 468 - the sender proof, red against a nominal sender or green against CwSender; keeps the printout.
# Usage: sh .run-unit/unit468-senders.sh <red|green|suffix> "<n of 3>"
cd /c/Source/HamLet || exit 1
export LC_ALL=C
if [ "$1" = red ]; then
  export HAMLET_UNIT468_SENDER=nominal
  NOTE="Task 1 - watching the sender proof fail: every case keyed 1:3:1:3:7 whatever its profile"
else
  unset HAMLET_UNIT468_SENDER
  NOTE="Task 1 - the sender proof against CwSender"
fi
sh .run-unit/unit468-extra.sh senders "$1" "$2"
OUT=.run-unit/unit468-senders-$1.txt
echo "== FAIL checks by profile and speed"
grep -a "^ *proof | " "$OUT" | grep -a "| FAIL$" | awk -F'|' '{print $2 "|" $3 "|" $6 "|" $7}' | sed 's/  */ /g' | sort | uniq | head -80
echo "== cases passed / failed"
grep -a -E "^\s+(Passed|Failed) .*Sender" "$OUT" | sed -E 's/^\s+//' | cut -c1-160
