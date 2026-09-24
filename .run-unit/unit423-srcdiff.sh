#!/bin/sh
# unit 423 - the transmit files against 7e209cb4, src/Hamlet.RadioEngine and all of src against entry 0456cad7.
cd /c/Source/HamLet || exit 1
sh .run-unit/unit423-transmit.sh 0456cad7
echo "== src/Hamlet.RadioEngine against 0456cad7"
git diff --stat 0456cad7 -- src/Hamlet.RadioEngine
echo "== end engine"
echo "== all of src against 0456cad7"
git diff --stat 0456cad7 -- src
echo "== end src"
echo "== data against 0456cad7"
git diff --stat 0456cad7 -- data
echo "== end data"
