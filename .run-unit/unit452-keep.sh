#!/bin/sh
# unit 452 - keep one printout under another name.
# Usage: sh .run-unit/unit452-keep.sh <from> <to>
cd /c/Source/HamLet || exit 1
cp "$1" "$2"
wc -l "$2"
