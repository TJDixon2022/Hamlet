#!/bin/sh
# unit 428 - the transmit files against 7e209cb4, src/Hamlet.RadioEngine and all of src against entry 9db61107.
cd /c/Source/HamLet || exit 1
sh .run-unit/unit428-transmit.sh 9db61107
echo "== src/Hamlet.RadioEngine against 9db61107"
git diff --stat 9db61107 -- src/Hamlet.RadioEngine
echo "== end engine"
echo "== all of src against 9db61107"
git diff --stat 9db61107 -- src
echo "== end src"
echo "== data against 9db61107"
git diff --stat 9db61107 -- data
echo "== end data"
