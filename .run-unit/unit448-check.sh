#!/bin/sh
# unit 448 - counts quoted in output.md: recordings whose text changed, keyed files with sure letters while acquiring.
cd /c/Source/HamLet || exit 1
export LC_ALL=C
echo "recordings: $(grep -c '^ *text | ' .run-unit/unit448-text-before.sorted.txt)"
echo "text lines changed: $(diff .run-unit/unit448-text-before.sorted.txt .run-unit/unit448-text-change.sorted.txt | grep -c '^<  *text | ')"
echo "callsign lines changed: $(diff .run-unit/unit448-text-before.sorted.txt .run-unit/unit448-text-change.sorted.txt | grep -c '^<  *callsigns | ')"
grep -a "^ *hm-req-102 | " .run-unit/unit448-acq-keyed-head.txt | sed -E "s/^ +//" | awk '!seen[$0]++' | awk -F' [|] ' '{ split($3, w, " "); n = w[length(w)]; if (n + 0 > 0) k++; t++ } END { print "keyed files with sure while acquiring: " k " of " t }'
date "+%Y-%m-%d %H:%M"
