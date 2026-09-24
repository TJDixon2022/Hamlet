#!/bin/sh
# unit 412 - the exit diffs: the transmit files against 7e209cb4, and Cw and src against the entry HEAD 0483369d.
cd /c/Source/HamLet || exit 1
sh .run-unit/unit412-transmit.sh 0483369d
echo "== src/Hamlet.RadioEngine/Cw against 0483369d, committed and working tree"
git diff --stat 0483369d -- src/Hamlet.RadioEngine/Cw
echo "== end Cw"
echo "== src against 0483369d"
git diff --stat 0483369d -- src
echo "== end src"
