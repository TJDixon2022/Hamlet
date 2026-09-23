#!/bin/sh
# unit 404 task 1 - for each of the fourteen, trial the whole prefix: every earlier piece, oldest first.
cd /c/Source/HamLet || exit 1
: > .run-unit/unit404-prefix.list
for t in 10 11 13 15 19 21 24 26 27 29 37 38 40 44; do
  line="$t"
  i=1
  while [ $i -lt $t ]; do line="$line $i"; i=$((i + 1)); done
  echo "$line" >> .run-unit/unit404-prefix.list
done
date +%T
sh .run-unit/unit404-trials.sh .run-unit/unit404-prefix.list trace-prefix
date +%T
