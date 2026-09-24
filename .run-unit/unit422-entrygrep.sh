#!/bin/sh
# unit 422 - where TurnRingCountText occurs in the entry tree bdd0070b.
cd /c/Source/HamLet || exit 1
echo "== src at bdd0070b"
git grep -n "TurnRingCountText" bdd0070b -- src
echo "== tests at bdd0070b"
git grep -n "TurnRingCountText" bdd0070b -- tests
echo "== end"
