#!/bin/sh
# unit 458 task 3 - the where-it-wins file: the written head, then the printer's lines.
cd /c/Source/HamLet/.run-unit || exit 1
OUT=unit458-where-it-wins.txt
cat unit458-where-it-wins-head.txt > $OUT
grep -a -E "^ *(class codes|win|wins|nothing) " unit458-wins.txt | sed -E 's/^ +//' >> $OUT
wc -l $OUT
