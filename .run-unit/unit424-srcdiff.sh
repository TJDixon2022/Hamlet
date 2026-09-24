#!/bin/sh
# unit 424 - the transmit files against 7e209cb4, src/Hamlet.RadioEngine and all of src against entry e4085d43.
cd /c/Source/HamLet || exit 1
sh .run-unit/unit424-transmit.sh e4085d43
echo "== src/Hamlet.RadioEngine against e4085d43"
git diff --stat e4085d43 -- src/Hamlet.RadioEngine
echo "== end engine"
echo "== all of src against e4085d43"
git diff --stat e4085d43 -- src
echo "== end src"
echo "== data against e4085d43"
git diff --stat e4085d43 -- data
echo "== end data"
