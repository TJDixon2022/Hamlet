#!/bin/sh
# unit 465 - byte-identical checks: both decoders alone against task 0's saves, the four metrics row by row against entry, the port untouched.
# Usage: sh .run-unit/unit465-ident.sh <suffix> "<n of 4>"
cd /c/Source/HamLet || exit 1
sh .run-unit/unit465-save.sh "$1" "$2" before
sh .run-unit/unit465-round.sh metrics "$1" "$2"
grep -a -E "^ total \| (real|synthetic) \|" .run-unit/unit465-metrics-$1.txt | cut -c1-260
sh .run-unit/unit465-v11.sh entry "$1" | tail -3
echo "== git diff 19109b51 over Cw/Second:"
git diff --stat 19109b51 -- src/Hamlet.RadioEngine/Cw/Second/
echo "(end Second diff)"
