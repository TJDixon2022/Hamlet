#!/bin/sh
# unit 449 - the summary lines of one trace printout, then optionally the trace rows.
# Usage: sh .run-unit/unit449-traceshow.sh <suffix> [rows]
cd /c/Source/HamLet || exit 1
F=.run-unit/unit449-trace449-$1.txt
grep -a "^ *\(dim\|route\|group\|pitch\|edge\|total\) | " "$F" | sed -E "s/^ +//" | cut -c1-600
if [ "$2" = "rows" ]; then
  grep -a "^ *trace | " "$F" | sed -E "s/^ +//"
fi
