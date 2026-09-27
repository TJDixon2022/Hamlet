#!/bin/sh
# unit 462 - judge one screened technique in the working tree: build, adjudicated, metrics, V-11 against task 0, parity (decode time, the port).
# Usage: sh .run-unit/unit463-screen.sh <B|C|W> "<n of 4>" [parity]
cd /c/Source/HamLet || exit 1
T=$1
if [ "$3" = "parity" ]; then
  sh .run-unit/unit463-round.sh parity "screen-$T" "$2"
  sh .run-unit/unit463-paritykeep.sh "screen-$T"
  sh .run-unit/unit463-portsave.sh "screen-$T" "screen-$T" before
  exit 0
fi
sh .run-unit/unit463-build.sh "screen-$T" "$2" "Screen $T: building with the technique in the working tree" || exit 1
tail -3 .run-unit/unit463-build-screen-$T.txt
sh .run-unit/unit463-round.sh adjudicated "screen-$T" "$2"
sh .run-unit/unit463-round.sh metrics "screen-$T" "$2"
echo "== totals, entry then with $T"
grep -a -E "^ total \| (real|synthetic) \| MET" .run-unit/unit463-metrics-entry.txt | cut -c1-260
grep -a -E "^ total \| (real|synthetic) \| MET" .run-unit/unit463-metrics-screen-$T.txt | cut -c1-260
sh .run-unit/unit463-v11.sh entry "screen-$T"
