#!/bin/sh
# unit 425 - the transmit files against 7e209cb4, src/Hamlet.RadioEngine and all of src against entry b12cbbe4.
cd /c/Source/HamLet || exit 1
sh .run-unit/unit425-transmit.sh b12cbbe4
echo "== src/Hamlet.RadioEngine against b12cbbe4"
git diff --stat b12cbbe4 -- src/Hamlet.RadioEngine
echo "== end engine"
echo "== all of src against b12cbbe4"
git diff --stat b12cbbe4 -- src
echo "== end src"
echo "== data against b12cbbe4"
git diff --stat b12cbbe4 -- data
echo "== end data"
