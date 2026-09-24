#!/bin/sh
# unit 416 - how unit 415's report printed the 09-24 text, and where the settled text is printed.
cd /c/Source/HamLet || exit 1
git show 580d1b24:output.md | grep -n -B3 -A6 "P O N S ORED"
echo "== captures type, 004322 and 004405"
grep -a -E "004322|004405" .run-unit/unit416-captures-c1.txt | cut -c1-300 | head
echo "== tests printing settled text"
grep -l -E "settled text|Settled" tests/Hamlet.RadioEngine.Tests/Cw/*.cs | head
