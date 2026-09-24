#!/bin/sh
# unit 415 - a type's printed rows at entry against a later run, timings stripped.
# Usage: sh .run-unit/unit415-diff.sh <type-prefix> <later-suffix>
cd /c/Source/HamLet || exit 1
strip() {
  grep -a -E "^\s+(Passed|Failed) |^ ?[a-z0-9/ :-]+ \||named|elements|placeholders" "$1" | sed -E "s/ \[[0-9.]+ ?m?s\]//; s/[0-9]+ ms//g; s/in [0-9.]+ s//g"
}
strip .run-unit/unit415-$1-entry.txt > .run-unit/unit415-$1-entry.norm
strip .run-unit/unit415-$1-$2.txt > .run-unit/unit415-$1-$2.norm
wc -l .run-unit/unit415-$1-entry.norm .run-unit/unit415-$1-$2.norm
diff .run-unit/unit415-$1-entry.norm .run-unit/unit415-$1-$2.norm
echo "diff rc $?"
