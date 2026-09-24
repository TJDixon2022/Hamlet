#!/bin/sh
# unit 421 - src/Hamlet.RadioEngine/Cw against entry 23457c4b, the transmit files against 7e209cb4, all of src against entry.
cd /c/Source/HamLet || exit 1
echo "== src/Hamlet.RadioEngine/Cw against 23457c4b"
git diff --stat 23457c4b -- src/Hamlet.RadioEngine/Cw
echo "== end Cw"
sh .run-unit/unit421-transmit.sh 23457c4b
echo "== all of src against 23457c4b"
git diff --stat 23457c4b -- src
echo "== end src"
echo "== data against 23457c4b"
git diff --stat 23457c4b -- data
echo "== end data"
