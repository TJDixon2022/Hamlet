#!/bin/sh
# unit 459 - each departure's scored stretch: key, ours before (task 0), ours with the refused change, the port.
cd /c/Source/HamLet || exit 1
OUT=.run-unit/unit459-stretches.txt
: > $OUT
for n in unadjudicated/cw-2026-09-23-173723 unadjudicated/cw-2026-08-18-003758 unadjudicated/cw-2026-08-22-031838 unadjudicated/cw-2026-08-22-032012 unadjudicated/cw-2026-08-22-032050 unadjudicated/cw-2026-08-22-032113 unadjudicated/cw-2026-09-24-004234 cq-18wpm-15db-char5 cq-18wpm-5db-char5 cw-2026-08-18-004507 unadjudicated/cw-2026-08-22-031948
do
  echo "== $n" >> $OUT
  grep -a "^ text | $n | 1 | key " .run-unit/unit459-parity-entry.txt | sed 's/^ text | [^|]*| 1 | /  /' >> $OUT
  grep -a "^ text | $n | 1 | ours " .run-unit/unit459-parity-entry.txt | sed 's/^ text | [^|]*| 1 | ours /  ours before /' >> $OUT
  grep -a "^ text | $n | 1 | ours " .run-unit/unit459-parity-after.txt | sed 's/^ text | [^|]*| 1 | ours /  ours after  /' >> $OUT
  grep -a "^ text | $n | 1 | port " .run-unit/unit459-parity-entry.txt | sed 's/^ text | [^|]*| 1 | port /  port        /' >> $OUT
done
cat $OUT
