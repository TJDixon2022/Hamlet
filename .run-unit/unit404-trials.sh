#!/bin/sh
# unit 404 task 1 - run each line of a trial list: "<target> [chain ...]"; output appended to a log.
# Usage: sh .run-unit/unit404-trials.sh <listfile> <logname>
cd /c/Source/HamLet || exit 1
LOG=.run-unit/unit404-$2.txt
: > "$LOG"
while read line; do
  [ -z "$line" ] && continue
  sh .run-unit/unit404-trial.sh $line < /dev/null | tee -a "$LOG"
done < "$1"
