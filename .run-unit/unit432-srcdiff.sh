#!/bin/sh
# unit 432 - the transmit files against 7e209cb4, src/Hamlet.RadioEngine and all of src against entry cec344ad.
cd /c/Source/HamLet || exit 1
sh .run-unit/unit432-transmit.sh cec344ad
echo "== src/Hamlet.RadioEngine against cec344ad"
git diff --stat cec344ad -- src/Hamlet.RadioEngine
echo "== end engine"
echo "== all of src against cec344ad"
git diff --stat cec344ad -- src
echo "== end src"
echo "== data against cec344ad"
git diff --stat cec344ad -- data
echo "== end data"
