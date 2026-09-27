#!/bin/sh
# unit 471 - put the clock's time on the UNIT: line of output.md, keep a copy, and validate.
cd /c/Source/HamLet || exit 1
NOW=$(date "+%Y-%m-%d %H:%M")
sed -i "s/@@NOW@@/$NOW/" output.md
grep -n "^UNIT:" output.md
grep -n "^## " output.md
cp output.md .run-unit/unit471-output.md
sh .run-unit/unit471-validate.sh
