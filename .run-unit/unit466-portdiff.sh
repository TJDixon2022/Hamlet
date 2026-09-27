#!/bin/sh
# unit 466 - the port against 19109b51, and src this unit against 9cc2c74b.
cd /c/Source/HamLet || exit 1
echo "== git diff 19109b51 -- src/Hamlet.RadioEngine/Cw/Second/"
git diff 19109b51 -- src/Hamlet.RadioEngine/Cw/Second/
echo "== (end port diff)"
echo "== src changed this unit, against 9cc2c74b, working tree included"
git diff --stat 9cc2c74b -- src
git status --short -- src
