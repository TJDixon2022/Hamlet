#!/bin/sh
# unit 460 task 2 - assemble .run-unit/unit460-pair-trace.txt: the head, one line per moved window, then every moved window in full.
cd /c/Source/HamLet || exit 1
OUT=.run-unit/unit460-pair-trace.txt
cp .run-unit/unit460-pair-trace-head.txt $OUT
echo "ONE LINE PER MOVED WINDOW (recording rREAD | unit before -> after | marks' unit | key's unit | direction | pairs | first pair || letters that changed there)" >> $OUT
sh .run-unit/unit460-paircondense.sh >> $OUT
echo "" >> $OUT
echo "================================================================================================" >> $OUT
echo "EVERY MOVED WINDOW IN FULL (.run-unit/unit460-pair-joined.txt)" >> $OUT
cat .run-unit/unit460-pair-joined.txt >> $OUT
wc -l $OUT
