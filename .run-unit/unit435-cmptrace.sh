#!/bin/sh
# unit 435 - the trace's per-recording counts at two suffixes, differences only.
# Usage: sh .run-unit/unit435-cmptrace.sh <before> <after>
cd /c/Source/HamLet || exit 1
R=.run-unit/unit435
grep -hE "^\s*(check|count) \|" $R-$1.txt > $R-cmptrace-a.tmp
grep -hE "^\s*(check|count) \|" $R-$2.txt > $R-cmptrace-b.tmp
diff $R-cmptrace-a.tmp $R-cmptrace-b.tmp
echo "== after"
grep -E "^\s*count \| all keyed|nan \| all" $R-$2.txt
