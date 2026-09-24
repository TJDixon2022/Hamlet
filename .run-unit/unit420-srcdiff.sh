#!/bin/sh
# unit 420 - src/Hamlet.RadioEngine/Cw against entry b378abeb, the transmit files against 7e209cb4, all of src against entry.
cd /c/Source/HamLet || exit 1
echo "== src/Hamlet.RadioEngine/Cw against b378abeb"
git diff --stat b378abeb -- src/Hamlet.RadioEngine/Cw
echo "== end Cw"
sh .run-unit/unit420-transmit.sh b378abeb
echo "== all of src against b378abeb"
git diff --stat b378abeb -- src
echo "== end src"
echo "== data against b378abeb"
git diff --stat b378abeb -- data
echo "== end data"
