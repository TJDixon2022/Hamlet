#!/bin/sh
# unit 433 - the transmit files against 7e209cb4, src/Hamlet.RadioEngine and all of src against entry f1e751ea.
cd /c/Source/HamLet || exit 1
sh .run-unit/unit433-transmit.sh f1e751ea
echo "== src/Hamlet.RadioEngine against f1e751ea"
git diff --stat f1e751ea -- src/Hamlet.RadioEngine
echo "== end engine"
echo "== all of src against f1e751ea"
git diff --stat f1e751ea -- src
echo "== end src"
echo "== data against f1e751ea"
git diff --stat f1e751ea -- data
echo "== end data"
