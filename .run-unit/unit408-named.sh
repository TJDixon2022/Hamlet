#!/bin/sh
# unit 408 - the adjudicated readings with placeholders and spaces stripped, entry against another run.
# Usage: sh .run-unit/unit408-named.sh <outname> <against-outname>
cd /c/Source/HamLet || exit 1
for r in "$1" "$2"
do
  grep -E "^ [a-z/0-9-]+ \(" ".run-unit/unit408-$r.txt" | grep -oE "^ [a-z/0-9-]+" | sed 's/^ //' > ".run-unit/unit408-$r-names.txt"
  grep -E "^\s+(read|it reads): " ".run-unit/unit408-$r.txt" | sed -E 's/^\s+(read|it reads): //' | sed 's/■//g' | tr -d ' ' > ".run-unit/unit408-$r-stripped.txt"
  paste -d'|' ".run-unit/unit408-$r-names.txt" ".run-unit/unit408-$r-stripped.txt" | sort > ".run-unit/unit408-$r-named.txt"
done
echo "== named characters, $2 against $1"
diff ".run-unit/unit408-$2-named.txt" ".run-unit/unit408-$1-named.txt" && echo "IDENTICAL"
