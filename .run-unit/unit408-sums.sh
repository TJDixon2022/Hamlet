#!/bin/sh
# unit 408 - column totals of a captures table: emitted, placeholders, old floor, named.
# Usage: sh .run-unit/unit408-sums.sh <table-file>
cd /c/Source/HamLet || exit 1
awk '{ e += $2; p += $4; sub("floor=", "", $6); f += $6 } END { print "emitted " e " placeholders " p " oldfloor " f " named " e - p }' "$1"
