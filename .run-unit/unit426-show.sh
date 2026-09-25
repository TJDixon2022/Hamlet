#!/bin/sh
# unit 426 - print the trace's count tables and, per point, the writes, read-back and narration.
# Usage: sh .run-unit/unit426-show.sh <trace output file>
cd /c/Source/HamLet || exit 1
F=$1
grep -n "SENTENCES ASKING" -A12 "$F" | cut -c1-120
grep -n "=================== \|--- point\|asked for:\|writes to the radio\|preamp read back\|narrated since\|then clear 40" "$F" | cut -c1-600
