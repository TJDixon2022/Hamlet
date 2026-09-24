#!/bin/sh
# unit 411 - the exit diffs: transmit files against 7e209cb4, Cw and src against the entry HEAD.
cd /c/Source/HamLet || exit 1
sh .run-unit/unit411-transmit.sh bcebdfea
echo "== src/Hamlet.RadioEngine/Cw against bcebdfea"
git diff --stat bcebdfea -- src/Hamlet.RadioEngine/Cw
echo "== end Cw"
echo "== src against bcebdfea"
git diff --stat bcebdfea -- src
echo "== end src"
echo "== Ic7300Rig hunks"
git diff bcebdfea -- src/Hamlet.RadioEngine/Rig/Ic7300Rig.cs
