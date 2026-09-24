#!/bin/sh
# unit 419 - unit 418's report, from its exit commit, for the carried asks.
cd /c/Source/HamLet || exit 1
git show cb526e01:output.md > .run-unit/unit419-prev-output.txt
grep -n "^## \|^### \|Asks still outstanding\|^UNIT:" .run-unit/unit419-prev-output.txt
