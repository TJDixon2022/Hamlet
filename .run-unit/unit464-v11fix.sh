#!/bin/sh
# unit 464 - V-11 against task 0: regenerate the entry rows, restore v11-before as task 0 made it (a copy of entry), compare a suffix.
# Usage: sh .run-unit/unit464-v11fix.sh <after-suffix>
cd /c/Source/HamLet || exit 1
sh .run-unit/unit464-v11.sh entry "$1"
cp .run-unit/unit464-v11-entry.txt .run-unit/unit464-v11-before.txt
git diff --stat -- .run-unit/unit464-v11-before.txt
echo "== git diff above is empty when v11-before is as committed at task 0"
cmp .run-unit/unit464-v11-before.txt .run-unit/unit464-v11-$1.txt && echo "V-11: every row IDENTICAL to task 0"
