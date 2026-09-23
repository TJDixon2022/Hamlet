#!/bin/sh
# unit 404 task 2 - apply pieces to the real tree, in order, with git apply --3way, Cw only.
# Usage: sh .run-unit/unit404-apply.sh <n> [n ...]
# Stops at the first piece that leaves a conflict or refuses, and says which files.
# Extra excludes per piece come from .run-unit/unit404-exclude.txt: "<n> <path>" lines.
cd /c/Source/HamLet || exit 1
P=.run-unit/unit404-patches
for n in "$@"; do
  f=$(ls $P/$(printf "%02d" "$n")-*.patch)
  EX=""
  for x in $(awk -v n="$n" '$1 == n {print $2}' .run-unit/unit404-exclude.txt 2>/dev/null); do EX="$EX --exclude=$x"; done
  if git apply --3way --whitespace=nowarn $EX "$f" > .run-unit/unit404-apply-last.txt 2>&1; then
    echo "piece $n $(basename $f .patch): applied$( [ -n "$EX" ] && echo " (excluded:$EX)")"
  else
    echo "piece $n $(basename $f .patch): STOPPED"
    cat .run-unit/unit404-apply-last.txt
    git diff --name-only --diff-filter=U
    exit 1
  fi
  grep -i "conflict" .run-unit/unit404-apply-last.txt && { echo "CONFLICT in piece $n"; exit 1; }
done
git diff --stat HEAD -- src | tail -3
