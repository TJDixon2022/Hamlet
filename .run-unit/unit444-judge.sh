#!/bin/sh
# unit 444 - task 2's judgement under the change: metrics and adjudicated, then per-recording V-11 against entry.
cd /c/Source/HamLet || exit 1
sh .run-unit/unit444-round.sh metrics change "2 of 3"
sh .run-unit/unit444-round.sh adjudicated change "2 of 3"
grep "^ *total | real\|^ *total | synthetic\|^ *row | unadjudicated/cw-2026-09-23-173723" .run-unit/unit444-metrics-change.txt | cut -c1-330
sh .run-unit/unit444-v11.sh .run-unit/unit444-metrics-entry.txt .run-unit/unit444-metrics-change.txt > .run-unit/unit444-v11-change.txt
awk '{ split($0, a, "boundaries wrong "); split(a[2], b, " "); if (b[1] != b[3] || $0 ~ /WORSE/ || $0 ~ /recordings compared/) print }' .run-unit/unit444-v11-change.txt
grep -E "Failed |Passed!|Failed!" .run-unit/unit444-adjudicated-change.txt | head -5
