#!/bin/sh
# unit 416 - unit 415's report, section 4 onward, for the carried asks.
cd /c/Source/HamLet || exit 1
git show 580d1b24:output.md | sed -n "/^## 4/,\$p"
echo "== head of 415 report"
git show 580d1b24:output.md | sed -n 1,20p
