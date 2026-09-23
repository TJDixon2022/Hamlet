#!/bin/sh
# unit 406 - the six reds' printed numbers from an acq run and a recv run.
# Usage: sh .run-unit/unit406-nums.sh <acq-outname> <recv-outname>
cd /c/Source/HamLet || exit 1
A=.run-unit/unit406-$1.txt
R=.run-unit/unit406-$2.txt
echo "#6  $(grep -m1 -E '^ 25 wpm at 18 dB, bare:' "$A")"
echo "#15 $(grep -m1 -E '^ 12 wpm at 18 dB, run-up:' "$A")"
echo "#42 $(grep -A5 'Failed Hamlet.*NothingIsEmittedDuring' "$R" | grep -m1 '^Actual' ) $(grep -c 'Passed Hamlet.*NothingIsEmittedDuring' "$R") passed"
for n in coverage-easy exchange-easy tightfist-easy
do
  M=$(grep -A2 "Failed Hamlet.*TheEasyTierIsReadWhole(name: \"$n\")" "$R" | grep -oE "with [0-9]+ characters Hamlet could not read, [0-9]+ that" | sed -E 's/with ([0-9]+) characters Hamlet could not read, ([0-9]+) that/\1 + \2/')
  P=$(grep -c "Passed Hamlet.*TheEasyTierIsReadWhole(name: \"$n\")" "$R")
  echo "$n $M passed=$P"
done
