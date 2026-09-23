#!/bin/sh
# unit 408 - the adjudicated type's printed readings, sorted, and diffed against another run's.
# Usage: sh .run-unit/unit408-reads.sh <outname> [<against-outname>]
cd /c/Source/HamLet || exit 1
grep -E "^\s+(read|it reads): " ".run-unit/unit408-$1.txt" | sort > ".run-unit/unit408-$1-reads.txt"
wc -l < ".run-unit/unit408-$1-reads.txt"
if [ -n "$2" ]; then
  echo "== diff against $2"
  diff ".run-unit/unit408-$2-reads.txt" ".run-unit/unit408-$1-reads.txt" && echo "IDENTICAL"
fi
