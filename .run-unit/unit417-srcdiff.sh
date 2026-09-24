#!/bin/sh
# unit 417 - the transmit files against 7e209cb4 and all of src against entry HEAD.
cd /c/Source/HamLet || exit 1
sh .run-unit/unit417-transmit.sh 12e2d442
echo "== all of src against 12e2d442"
git diff --stat 12e2d442 -- src
echo "== end src"
