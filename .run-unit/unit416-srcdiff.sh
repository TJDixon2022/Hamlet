#!/bin/sh
# unit 415 - the transmit files against 7e209cb4 and all of src against entry HEAD.
cd /c/Source/HamLet || exit 1
sh .run-unit/unit416-transmit.sh 61d8b58f
echo "== all of src against 61d8b58f"
git diff --stat 61d8b58f -- src
echo "== end src"
