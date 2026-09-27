#!/bin/sh
# unit 464 - the exit prints: the port against 19109b51, the eleven transmit files against 7e209cb4, src this unit, git state.
cd /c/Source/HamLet || exit 1
{
  echo "== git diff 19109b51 -- src/Hamlet.RadioEngine/Cw/Second/ (empty):"
  git diff 19109b51 -- src/Hamlet.RadioEngine/Cw/Second/
  echo "== end port"
  sh .run-unit/unit464-tx.sh
  echo "== src this unit against 99c90b69:"
  git diff --stat 99c90b69 HEAD -- src
  sh .run-unit/unit464-gitstate.sh
} > .run-unit/unit464-exit-print.txt 2>&1
cat .run-unit/unit464-exit-print.txt
