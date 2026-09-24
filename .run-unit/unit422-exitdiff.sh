#!/bin/sh
# unit 422 - the transmit files against 7e209cb4, src/Hamlet.RadioEngine against entry bdd0070b, and all of src against entry.
cd /c/Source/HamLet || exit 1
sh .run-unit/unit422-transmit.sh bdd0070b
echo "== src/Hamlet.RadioEngine against bdd0070b"
git diff --stat bdd0070b -- src/Hamlet.RadioEngine
echo "== end engine"
echo "== all of src against bdd0070b"
git diff --stat bdd0070b -- src
echo "== end src"
