#!/bin/sh
# unit 409 - the table's row lines as markdown rows, into .run-unit/unit409-<outname>-md.txt.
# Usage: sh .run-unit/unit409-md.sh <outname>
cd /c/Source/HamLet || exit 1
IN=.run-unit/unit409-$1-rows.txt
OUT=.run-unit/unit409-$1-md.txt
grep "^row | case" "$IN" | sed -E "s/^row //; s/$/ |/" > "$OUT"
echo "|---|---|---|---|---|---|---|---|---|---|---|---|---|---|" >> "$OUT"
grep "^row | " "$IN" | grep -v "^row | case" | sed -E "s/^row //; s/$/ |/; s#unadjudicated/##" >> "$OUT"
cat "$OUT"
