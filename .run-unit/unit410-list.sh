#!/bin/sh
# unit 410 - sorted Passed/Failed list per test from a detailed run, optionally diffed against another list.
# Usage: sh .run-unit/unit410-list.sh <outname> [<against-outname>]
cd /c/Source/HamLet || exit 1
grep -E "^\s+(Passed|Failed) Hamlet" ".run-unit/unit410-$1.txt" | sed -E "s/^\s+//" | sed -E "s/ \[[^]]*\]$//" | sort > ".run-unit/unit410-$1-list.txt"
echo "passed $(grep -c "^Passed" .run-unit/unit410-$1-list.txt) failed $(grep -c "^Failed" .run-unit/unit410-$1-list.txt)"
if [ -n "$2" ]; then
  echo "== diff against $2"
  diff ".run-unit/unit410-$2-list.txt" ".run-unit/unit410-$1-list.txt" && echo "IDENTICAL"
fi
