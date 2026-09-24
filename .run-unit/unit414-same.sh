#!/bin/sh
# unit 414 - compare a type's printed rows at entry and at exit, timings stripped.
# Usage: sh .run-unit/unit414-same.sh <type-prefix, e.g. captures>
cd /c/Source/HamLet || exit 1
strip() {
  grep -E "^\s+(Passed|Failed) |^\s*[a-z0-9 ]+ \||named|elements" "$1" | sed -E "s/ \[[0-9.]+ ?m?s\]//; s/[0-9]+ ms//g"
}
strip .run-unit/unit414-$1-entry.txt > .run-unit/unit414-$1-entry.norm
strip .run-unit/unit414-$1-exit.txt > .run-unit/unit414-$1-exit.norm
wc -l .run-unit/unit414-$1-entry.norm .run-unit/unit414-$1-exit.norm
diff .run-unit/unit414-$1-entry.norm .run-unit/unit414-$1-exit.norm
echo "diff rc $?"
