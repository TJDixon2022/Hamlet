#!/bin/sh
# unit 450 - per capture, MET-PITCH-ERR and windows more than 25 Hz off, before and after; changed rows only.
# Usage: sh .run-unit/unit450-pitchcmp.sh <before-suffix> <after-suffix>
cd /c/Source/HamLet || exit 1
export LC_ALL=C
pick() {
  grep -a "^ *capture | " ".run-unit/unit450-pitch-$1.txt" | grep -v "capture | file" | awk -F' [|] ' '{ e = $5; sub(/MET-PITCH-ERR /, "", e); w = $7; sub(/ windows >25/, "", w); t = $3; sub(/tracker /, "", t); i = $4; sub(/instrument /, "", i); print $2 "\t" t "\t" i "\t" e "\t" w }' | sort
}
pick "$1" > .run-unit/unit450-pc-a.tmp
pick "$2" > .run-unit/unit450-pc-b.tmp
join -t "	" .run-unit/unit450-pc-a.tmp .run-unit/unit450-pc-b.tmp | awk -F'\t' '
  { n++; a = $4 + 0; b = $8 + 0; if (a < 0) a = -a; if (b < 0) b = -b;
    split($5, wa, " of "); split($9, wb, " of ");
    if ($4 != $8 || $5 != $9) {
      tag = (b > a || wb[1] > wa[1]) ? "  ROSE" : "";
      printf "%s  tracker %s -> %s  instrument %s -> %s  MET-PITCH-ERR %s -> %s  windows >25 %s -> %s%s\n", $1, $2, $6, $3, $7, $4, $8, $5, $9, tag; ch++ }
    sa += wa[1]; sb += wb[1] }
  END { printf "captures %d, changed %d, windows more than 25 Hz off %d -> %d\n", n, ch + 0, sa, sb }'
