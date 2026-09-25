#!/bin/sh
# unit 426 - the transmit files against 7e209cb4, src/Hamlet.RadioEngine and all of src against entry 972510e6.
cd /c/Source/HamLet || exit 1
sh .run-unit/unit426-transmit.sh 972510e6
echo "== src/Hamlet.RadioEngine against 972510e6"
git diff --stat 972510e6 -- src/Hamlet.RadioEngine
echo "== end engine"
echo "== all of src against 972510e6"
git diff --stat 972510e6 -- src
echo "== end src"
echo "== data against 972510e6"
git diff --stat 972510e6 -- data
echo "== end data"
