#!/bin/sh
# unit 472 - one recording's text from a save, sure letters as they are, dim letters in [brackets], gaps as spaces.
# Usage: sh .run-unit/unit472-render.sh <suffix> <recording>
cd /c/Source/HamLet/.run-unit || exit 1
export LC_ALL=C
OUT=""
grep -a "^save | ours | $2 | " unit472-text-$1.txt | cut -d'|' -f5,6 | while IFS='|' read -r TEXT CLASS
do
  TEXT=$(echo "$TEXT" | sed 's/^ *//;s/ *$//')
  CLASS=$(echo "$CLASS" | sed 's/^ *//;s/ *$//')
  case "$CLASS" in
    gap) printf ' ' ;;
    sure) printf '%s' "$TEXT" ;;
    dim) printf '[%s]' "$TEXT" ;;
    *) printf '%s' "$TEXT" ;;
  esac
done
echo ""
