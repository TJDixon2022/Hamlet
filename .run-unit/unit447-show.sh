#!/bin/sh
# unit 447 - print the speed trace and cases from one run, each line once, and the assertion messages.
# Usage: sh .run-unit/unit447-show.sh <suffix> [width] [case]
cd /c/Source/HamLet || exit 1
export LC_ALL=C
W=${2:-600}
P="needs|bound|grid|case|decode time"
if [ -n "$3" ]; then P="case"; fi
grep -aE "^\s*($P)[^|]*\|" .run-unit/unit447-speed-$1.txt | sed -E "s/^\s+//" | awk '!seen[$0]++' | cut -c1-$W
grep -aE "Assert\.|Actual|Failed " .run-unit/unit447-speed-$1.txt | head -16
