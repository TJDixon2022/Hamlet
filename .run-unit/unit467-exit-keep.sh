#!/bin/sh
# unit 466 task 4 - keep the exit prints, and print the three-way rows compactly for the report.
cd /c/Source/HamLet || exit 1
sh .run-unit/unit467-exit-print.sh > .run-unit/unit467-exit-print.txt 2>&1
grep -c "" .run-unit/unit467-exit-print.txt
echo "== compact three-way, figures only"
grep "^threeway | \(harness\|live\) |" .run-unit/unit467-threeway.txt | grep -v "| path |" | awk -F' [|] ' '{ r=$3; sub(/real HF, no CH-\* profile, SNR_2500 not measured, /, "", r); sub(/synthetic, no fading, shaped noise band \(not shown to be CH-AWGN\), /, "syn ", r); sub(/ in the passband \(not restated in the 2500 Hz reference\)/, "", r); sub(/ \(CW_SPEC.*$/, "", r); sub(/character gap 5 units, inside TX-FARNS.s 3 to 7/, "char5", r); gsub(/ \([0-9.]+\)/, "", $7); gsub(/ \([0-9.]+\)/, "", $8); gsub(/ \([0-9.]+\)/, "", $9); print $2 " | " r " | " $6 " | " $7 " | " $8 " | " $9 }'
echo "== req lines"
grep "^req |" .run-unit/unit467-threeway.txt | grep -v "014 no dim emitted\|014 not defined (emits no dim)$" | cut -c1-260
