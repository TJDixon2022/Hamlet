#!/bin/sh
# unit 419 - src/Hamlet.RadioEngine/Cw against entry, the transmit files against 7e209cb4, all of src against entry.
cd /c/Source/HamLet || exit 1
echo "== src/Hamlet.RadioEngine/Cw against 4bd85b35"
git diff --stat 4bd85b35 -- src/Hamlet.RadioEngine/Cw
echo "== end Cw"
sh .run-unit/unit419-transmit.sh 4bd85b35
echo "== all of src against 4bd85b35"
git diff --stat 4bd85b35 -- src
echo "== end src"
