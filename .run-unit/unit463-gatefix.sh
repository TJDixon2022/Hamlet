#!/bin/sh
# unit 463 - the gate room's before lines, measured after the A1 revert on the committed tree, appended to A1's numbers.
cd /c/Source/HamLet || exit 1
{
  echo ""
  echo "== the gate room through the stream, before (measured after the revert, on the tree as committed) then with A1 in"
  grep -a "gate-stream |" .run-unit/unit463-gate-before.txt
  grep -a "gate-stream |" .run-unit/unit463-gate-screen-A1.txt
} >> .run-unit/unit463-A1.txt
tail -4 .run-unit/unit463-A1.txt | cut -c1-200
