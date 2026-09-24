#!/bin/sh
# unit 422 - src/Hamlet.RadioEngine/Cw against entry bdd0070b, the transmit files against 7e209cb4, all of src against entry.
cd /c/Source/HamLet || exit 1
echo "== src/Hamlet.RadioEngine/Cw against bdd0070b"
git diff --stat bdd0070b -- src/Hamlet.RadioEngine/Cw
echo "== end Cw"
sh .run-unit/unit422-transmit.sh bdd0070b
echo "== all of src against bdd0070b"
git diff --stat bdd0070b -- src
echo "== end src"
echo "== data against bdd0070b"
git diff --stat bdd0070b -- data
echo "== end data"
