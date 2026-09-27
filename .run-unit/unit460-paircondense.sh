#!/bin/sh
# unit 460 task 2 - one line per moved window: recording, read, units, direction, pairs, first pair and its slots, and what changed at that read.
cd /c/Source/HamLet || exit 1
awk '
/^window \|/ { if (w != "") print w " || " ch; split($0, a, " [|] "); w = a[2] " r" substr(a[3], 6); ch = ""; next }
/^  unit  \|/ { split($0, a, " [|] "); w = w " | " a[2] " -> " a[3] " | marks " a[4] " | key " a[6] " | " a[7]; next }
/^  pairs \|/ { split($0, a, " [|] "); w = w " | " a[2] " | first " a[4]; next }
/^      (HEAD|DIFF)/ { if ($0 ~ /stretch [0-9]/) { t = $0; sub(/^ +/, "", t); ch = ch "; " t } next }
END { print w " || " ch }
' .run-unit/unit460-pair-joined.txt | sed -e 's/before //' -e 's/after //' -e 's/ over [0-9]* letters//' -e 's/, holding/ holding/' -e 's/ of the 16 slots at the end/ slots/' -e 's/ spikes passed over//' -e 's/unadjudicated\/cw-2026-08-..-//' -e 's/cw-2026-08-18-//'
