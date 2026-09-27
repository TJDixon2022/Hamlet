#!/bin/sh
# unit 471 - keep a saved text as the named texts file and compare it with another.
# Usage: sh .run-unit/unit471-texts.sh <suffix> <compare-file>
cd /c/Source/HamLet/.run-unit || exit 1
cp unit471-text-$1.txt unit471-texts-$1.txt
cmp "$2" unit471-texts-$1.txt && echo "unit471-texts-$1.txt BYTE-IDENTICAL to $2"
