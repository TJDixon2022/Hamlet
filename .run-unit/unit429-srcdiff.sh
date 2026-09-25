#!/bin/sh
# unit 429 - the transmit files against 7e209cb4, src/Hamlet.RadioEngine and all of src against entry f94886a1.
cd /c/Source/HamLet || exit 1
sh .run-unit/unit429-transmit.sh f94886a1
echo "== src/Hamlet.RadioEngine against f94886a1"
git diff --stat f94886a1 -- src/Hamlet.RadioEngine
echo "== end engine"
echo "== all of src against f94886a1"
git diff --stat f94886a1 -- src
echo "== end src"
echo "== data against f94886a1"
git diff --stat f94886a1 -- data
echo "== end data"
