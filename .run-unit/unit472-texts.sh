#!/bin/sh
# unit 471 - keep a saved text as the named texts file and compare it with another.
# Usage: sh .run-unit/unit472-texts.sh <suffix> <compare-file>
cd /c/Source/HamLet/.run-unit || exit 1
cp unit472-text-$1.txt unit472-texts-$1.txt
cmp "$2" unit472-texts-$1.txt && echo "unit472-texts-$1.txt BYTE-IDENTICAL to $2"
