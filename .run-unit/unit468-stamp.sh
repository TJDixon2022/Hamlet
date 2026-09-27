#!/bin/sh
# unit 466 - put the clock's time on the UNIT: line of output.md, keep a copy, and validate.
cd /c/Source/HamLet || exit 1
NOW=$(date "+%Y-%m-%d %H:%M")
sed -i "s/@@NOW@@/$NOW/" output.md
grep -n "^UNIT:" output.md
grep -n "^## " output.md
cp output.md .run-unit/unit468-output.md
sh .run-unit/unit468-validate.sh
