#!/bin/sh
# unit 431 - the transmit files against 7e209cb4, src/Hamlet.RadioEngine and all of src against entry 242168fc.
cd /c/Source/HamLet || exit 1
sh .run-unit/unit431-transmit.sh 242168fc
echo "== src/Hamlet.RadioEngine against 242168fc"
git diff --stat 242168fc -- src/Hamlet.RadioEngine
echo "== end engine"
echo "== all of src against 242168fc"
git diff --stat 242168fc -- src
echo "== end src"
echo "== data against 242168fc"
git diff --stat 242168fc -- data
echo "== end data"
