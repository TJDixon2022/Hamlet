#!/bin/sh
# unit 460 task 2 - join the pair trace at HEAD with the same trace taken with 6a0b65a1's diff in the working tree, read by read.
# For every read the rule moved: its pairs, the unit before (HEAD) and after (diff), the marks' unit, the key's unit
# (median span per unit of the letters either run settled sure and right inside the window), the direction, the
# sameness of the copied rule against the stream, and the letters that settled at that read in each run.
cd /c/Source/HamLet || exit 1
H=.run-unit/unit460-pair-head.txt
D=.run-unit/unit460-pair-diff.txt
OUT=.run-unit/unit460-pair-joined.txt
grep -a -E "^ (read|letter|marks) \|" $H | sed 's/^ /H|/' > .run-unit/unit460-pair-h.tmp
grep -a -E "^ (read|letter) \|" $D | sed 's/^ /D|/' > .run-unit/unit460-pair-d.tmp
cat .run-unit/unit460-pair-h.tmp .run-unit/unit460-pair-d.tmp | awk -F' *[|] *' '
function trim(s) { gsub(/^ +| +$/, "", s); return s }
function median(arr, n,   i, j, t) {
  for (i = 1; i <= n; i++) for (j = i + 1; j <= n; j++) if (arr[j] < arr[i]) { t = arr[i]; arr[i] = arr[j]; arr[j] = t }
  if (n == 0) return -1
  if (n % 2) return arr[(n + 1) / 2]
  return (arr[n / 2] + arr[n / 2 + 1]) / 2
}
{
  run = $1; kind = $2; rec = $3
  if (kind == "read") {
    k = rec SUBSEP $4
    if (run == "H") { order[++nr] = k; hline[k] = $0; hfrom[k] = $5; hto[k] = $6; hwpm[k] = $7; hunit[k] = $8; mwpm[k] = $9; munit[k] = $10;
      nmarks[k] = $11; nspikes[k] = $12; npairs[k] = $13; punit[k] = $14; pwpm[k] = $15; moves[k] = $16; first[k] = $17; pairs[k] = $18 }
    else { dwpm[k] = $7 }
  } else if (kind == "marks" && run == "H") {
    marks[rec SUBSEP $4] = $5 " | " $6
  } else if (kind == "letter") {
    k = rec SUBSEP $4
    line = $6 " " $7 " @" $5 "s " $8 " key=" $10 " " $11
    if ($9 != "-") line = line " (stretch " $9 ")"
    if (run == "H") hl[k] = hl[k] "\n      HEAD  " line; else dl[k] = dl[k] "\n      DIFF  " line
    if ($9 != "-") { t = $5 + 0; if (!(rec in s0) || t < s0[rec]) s0[rec] = t; if (!(rec in s1) || t > s1[rec]) s1[rec] = t }
    if ($8 == "Sure" && $11 == "right" && $14 != "") { tag = rec SUBSEP $5 SUBSEP $6
      if (!(tag in seen)) { seen[tag] = 1; na[rec]++; at[rec, na[rec]] = $5; au[rec, na[rec]] = $14 } }
  }
}
END {
  same = 0; checked = 0; toward = 0; away = 0; noanchor = 0
  for (i = 1; i <= nr; i++) {
    k = order[i]; split(k, kk, SUBSEP); rec = kk[1]; idx = kk[2]
    checked++
    expect = (moves[k] == "yes") ? pwpm[k] : hwpm[k]
    if ((dwpm[k] - expect) < 0.002 && (expect - dwpm[k]) < 0.002) same++
    else print "SAMENESS MISS | " rec " | read " idx " | head " hwpm[k] " | predicted " expect " | diff " dwpm[k]
    if (moves[k] != "yes") continue
    if (!(rec in s0) || hto[k] + 0 < s0[rec] || hfrom[k] + 0 > s1[rec]) { outside[rec] = outside[rec] " " idx; nout++; continue }
    nin++
    n = 0; delete arr
    for (j = 1; j <= na[rec]; j++) if (at[rec, j] + 0 >= hfrom[k] + 0 && at[rec, j] + 0 <= hto[k] + 0) arr[++n] = au[rec, j] + 0
    key = median(arr, n)
    before = 1200 / hwpm[k]; after = 1200 / dwpm[k]
    if (key < 0) { dir = "no key letter in the window"; noanchor++ }
    else if ((after - key) ^ 2 < (before - key) ^ 2) { dir = "TOWARD the key"; toward++ }
    else { dir = "AWAY from the key"; away++ }
    printf "window | %s | read %s | %s to %s s\n", rec, idx, hfrom[k], hto[k]
    printf "  unit  | before %.1f ms (%.1f WPM) | after %.1f ms (%.1f WPM) | marks imply %s ms | window measured %s WPM | key %s ms over %d letters | %s\n", before, hwpm[k], after, dwpm[k], munit[k], mwpm[k], (key < 0 ? "-" : sprintf("%.1f", key)), n, dir
    w = 16 - (npairs[k] - 1); if (w < 0) w = 0
    printf "  pairs | %s pairs of %s marks, %s spikes passed over | pair unit %s ms | first pair %s ms, holding %d of the 16 slots at the end\n", npairs[k], nmarks[k], nspikes[k], punit[k], first[k], w
    printf "  each  | %s\n", pairs[k]
    printf "  marks | %s\n", marks[k]
    printf "  settled at this read:%s%s\n", (k in hl ? hl[k] : "\n      HEAD  (none)"), (k in dl ? dl[k] : "\n      DIFF  (none)")
  }
  for (r in outside) printf "outside the stretch | %s | moved reads%s\n", r, outside[r]
  for (r in s0) printf "stretch | %s | letters %.3f to %.3f s\n", r, s0[r], s1[r]
  printf "total | reads checked %d | sameness %d of %d | moved reads %d, %d over a stretch and %d outside | over a stretch toward the key %d, away %d, no key letter in the window %d\n", checked, same, checked, nin + nout, nin, nout, toward, away, noanchor
}' > $OUT
tail -1 $OUT
grep -c "^window |" $OUT
