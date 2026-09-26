#!/bin/sh
# unit 449 - keep one printout under another name.
# Usage: sh .run-unit/unit449-keep.sh <from> <to>
cd /c/Source/HamLet || exit 1
cp "$1" "$2"
wc -l "$2"
