#!/bin/sh
# unit 428 - where are the 2026-09-25 7.052 captures, if anywhere on this machine.
cd /c/Source/HamLet || exit 1
echo "== tracked files naming 2026-09-25"
git ls-files | grep -c "2026-09-25"
echo "== any cw-2026-09-25 file under the repo or the user profile"
find /c/Source/HamLet /c/Users/TimDi -name "cw-2026-09-25*" 2>/dev/null | head -40
echo "== newest cw captures anywhere under the user profile"
find /c/Users/TimDi -name "cw-2026-09-*.wav" 2>/dev/null | head -40
echo "== end"
