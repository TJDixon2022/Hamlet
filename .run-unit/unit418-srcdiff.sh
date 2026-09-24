#!/bin/sh
# unit 418 - the transmit files against 7e209cb4 and all of src against entry HEAD.
cd /c/Source/HamLet || exit 1
sh .run-unit/unit418-transmit.sh 3211874b
echo "== all of src against 3211874b"
git diff --stat 3211874b -- src
echo "== end src"
