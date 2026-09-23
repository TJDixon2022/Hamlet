#!/bin/sh
# unit 407 - each entry list against unit 406's exit list of the same key.
cd /c/Source/HamLet/.run-unit || exit 1
for k in cap adj syn acq recv fix adjt gate disp capsig silence whygate twostation held pin survey switch gatewin2 witness second short bw scoring
do
  if [ -f unit406-$k-exit-list.txt ]; then
    if diff -q unit406-$k-exit-list.txt unit407-$k-entry-list.txt > /dev/null; then echo "$k identical"; else echo "$k DIFFERS"; diff unit406-$k-exit-list.txt unit407-$k-entry-list.txt; fi
  else
    echo "$k no 406 exit list"
  fi
done
