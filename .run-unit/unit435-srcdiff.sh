#!/bin/sh
# unit 435 - the transmit files against 7e209cb4, src/Hamlet.RadioEngine and all of src against entry 8555034d.
cd /c/Source/HamLet || exit 1
sh .run-unit/unit435-transmit.sh 8555034d
echo "== src/Hamlet.RadioEngine against 8555034d"
git diff --stat 8555034d -- src/Hamlet.RadioEngine
echo "== end engine"
echo "== all of src against 8555034d"
git diff --stat 8555034d -- src
echo "== end src"
echo "== data against 8555034d"
git diff --stat 8555034d -- data
echo "== end data"
