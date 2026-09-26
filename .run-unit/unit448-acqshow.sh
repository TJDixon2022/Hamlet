#!/bin/sh
# unit 448 - print the acquiring trace lines from one run, each line once.
# Usage: sh .run-unit/unit448-acqshow.sh <outname> [width]
cd /c/Source/HamLet || exit 1
export LC_ALL=C
W=${2:-400}
grep -aE "^\s*(join|span|cold move|instrument|alone|hm-req-10[23]|opening|total) \|" .run-unit/unit448-$1.txt | sed -E "s/^\s+//" | awk '!seen[$0]++' | cut -c1-$W
grep -aE "error|Exception" .run-unit/unit448-$1.txt | head -5
