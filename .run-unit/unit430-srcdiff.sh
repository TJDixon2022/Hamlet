#!/bin/sh
# unit 430 - the transmit files against 7e209cb4, src/Hamlet.RadioEngine and all of src against entry 003f2c98.
cd /c/Source/HamLet || exit 1
sh .run-unit/unit430-transmit.sh 003f2c98
echo "== src/Hamlet.RadioEngine against 003f2c98"
git diff --stat 003f2c98 -- src/Hamlet.RadioEngine
echo "== end engine"
echo "== all of src against 003f2c98"
git diff --stat 003f2c98 -- src
echo "== end src"
echo "== data against 003f2c98"
git diff --stat 003f2c98 -- data
echo "== end data"
