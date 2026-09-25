#!/bin/sh
# unit 439 - task 1's per-read rows for the report, compacted: s, mix, whole cut, lowest-highest block, fell back, entry marks/mark/gap/unit/wpm/from, local the same, settles.
cd /c/Source/HamLet/.run-unit || exit 1
F=unit439-trace.txt
for L in "opening" "locked, 004108 13.8 to 30.0 s" "cold"
do
  echo "== $L"
  grep -F " cut $L | " $F | grep -v " | s | " | awk -F' [|] ' '{ printf "%s | %s | %s | %s to %s | %s | %s, %s, %s, %s, %s %s | %s, %s, %s, %s, %s %s | %s | %s | %s\n", $2, $3, $4, $5, $6, $7, $8, $9, $10, $11, $12, $13, $14, $15, $16, $17, $18, $19, $20, $21, $22 }'
done
echo "== pieces by verdict"
grep -E "^ piece \| [0-9]" $F | awk -F' [|] ' '{ print $10 }' | sort | uniq -c
echo "== pieces by length ms"
grep -E "^ piece \| [0-9]" $F | awk -F' [|] ' '{ print $4 }' | sort -n | uniq -c
echo "== pieces, distinct marks by time"
grep -E "^ piece \| [0-9]" $F | awk -F' [|] ' '{ print $3 }' | sort -u | wc -l
echo "== first read's pieces, 34.5 s"
grep -E "^ piece \| 34.5 \|" $F | cut -c1-160
