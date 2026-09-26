#!/bin/sh
# unit 458 task 1 - the scoring-path file: the written head, then the trace's measured lines.
cd /c/Source/HamLet/.run-unit || exit 1
OUT=unit458-scoring-path.txt
cat unit458-scoring-path-head.txt > $OUT
grep -a -E "^ *(corpus|span|pitch|latency) \|" unit458-trace-t1.txt | sed -E 's/^ +//' >> $OUT
wc -l $OUT
