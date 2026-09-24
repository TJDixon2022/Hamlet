#!/bin/sh
# unit 412 - the captures type's printed row lines at entry against a later run, the 37 original rows only.
# Usage: sh .run-unit/unit412-same37.sh <later-suffix>
cd /c/Source/HamLet || exit 1
grep -a "named against a floor" .run-unit/unit412-captures-entry.txt | sed "s/^ *//" | sort > .run-unit/unit412-rows-entry.txt
grep -a "named against a floor" ".run-unit/unit412-captures-$1.txt" | sed "s/^ *//" | grep -v "cw-2026-09-24-" | sort > ".run-unit/unit412-rows37-$1.txt"
grep -a "named against a floor" ".run-unit/unit412-captures-$1.txt" | sed "s/^ *//" | sort -u | wc -l
wc -l < .run-unit/unit412-rows-entry.txt
if diff .run-unit/unit412-rows-entry.txt ".run-unit/unit412-rows37-$1.txt" > /dev/null
then
  echo "the 37 original rows print identically to entry"
else
  echo "the 37 original rows DIFFER from entry:"
  diff .run-unit/unit412-rows-entry.txt ".run-unit/unit412-rows37-$1.txt"
fi
