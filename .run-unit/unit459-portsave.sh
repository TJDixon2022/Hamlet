#!/bin/sh
# unit 459 - keep the port's texts from one parity run (the port's per-stretch text and all it printed), sorted.
# Usage: sh .run-unit/unit459-portsave.sh <parity-suffix> <out-suffix> [compare-suffix]
cd /c/Source/HamLet || exit 1
export LC_ALL=C
grep -aE "^ text \| [^|]+ \| [^|]+ \| (port |port printed )" .run-unit/unit459-parity-$1.txt | sort > .run-unit/unit459-port-$2.txt
wc -l .run-unit/unit459-port-$2.txt
if [ -n "$3" ]; then
  cmp .run-unit/unit459-port-$3.txt .run-unit/unit459-port-$2.txt && echo "BYTE-IDENTICAL to $3"
fi
