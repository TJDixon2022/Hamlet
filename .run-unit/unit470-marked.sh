#!/bin/sh
# unit 470 - one recording's saved text, sure as itself, dim in parentheses, placeholders as they are.
# Usage: sh .run-unit/unit470-marked.sh <suffix> <recording>
cd /c/Source/HamLet || exit 1
export LC_ALL=C
grep -a -F "| ours | $2 |" .run-unit/unit470-text-$1.txt | awk -F" [|] " '
{
  t = $5; c = $6
  if (t == "(space)") { printf " "; next }
  if (c == "dim") { printf "(%s)", t; next }
  printf "%s", t
}
END { printf "\n" }'
