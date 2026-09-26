#!/bin/sh
# unit 456 - print the sample rate in the header of every captured and synthetic-cq WAV, counted by rate.
cd /c/Source/HamLet || exit 1
for d in tests/fixtures/cw/captured tests/fixtures/cw/synthetic-cq; do
  echo "== $d"
  for f in "$d"/*.wav "$d"/*/*.wav; do
    [ -f "$f" ] || continue
    od -An -t u4 -j 24 -N 4 "$f" | tr -d " "
  done | sort | uniq -c
done
