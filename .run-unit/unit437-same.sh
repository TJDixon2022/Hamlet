#!/bin/sh
# unit 437 - compare a unit437 output against another with timings stripped.
# Usage: sh .run-unit/unit437-same.sh <fileA> <fileB>
cd /c/Source/HamLet/.run-unit || exit 1
F='RC=|WALL|elapsed|Elapsed|Duration|Time:|Total time|\[xUnit|[0-9] ms\)|[0-9] s\)|Build started|Test run for|VSTest|Starting test|A total of|Results File|Determining|up-to-date|->|Microsoft'
grep -vE "$F" "$1" | sed -E 's/\[[0-9.]+ ?m?s\]//g' > unit437-cmp-a.tmp
grep -vE "$F" "$2" | sed -E 's/\[[0-9.]+ ?m?s\]//g' > unit437-cmp-b.tmp
if diff -q unit437-cmp-a.tmp unit437-cmp-b.tmp > /dev/null
then
  echo "SAME $1 $2"
else
  echo "DIFFER $1 $2"
  diff unit437-cmp-a.tmp unit437-cmp-b.tmp | head -${3:-30}
fi
