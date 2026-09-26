#!/bin/sh
# unit 460 task 1 - the committed lines task 1's table is read from, printed with their file.
cd /c/Source/HamLet || exit 1
export LC_ALL=C
C="real HF, no CH-\* profile, SNR_2500 not measured, sender not stated in CW_SPEC.md"
for f in .run-unit/unit441-metrics-r82.txt .run-unit/unit442-metrics-entry.txt .run-unit/unit460-metrics-entry.txt
do
  echo "== $f"
  grep -a -E "^ (condition \| real \| $C|total \| real)" "$f" | cut -c1-400
  grep -a -E "^ row \| (unadjudicated/cw-2026-08-22-0321(13|29)|unadjudicated/cw-2026-09-23-173723) " "$f" | cut -c1-300
done
for f in .run-unit/unit421-named-exit.txt .run-unit/unit441-named-entry.txt .run-unit/unit442-named-entry.txt .run-unit/unit442-named-rebank.txt .run-unit/unit447-named-entry.txt .run-unit/unit447-named-change.txt .run-unit/unit460-named-entry.txt
do
  echo "== $f"
  grep -a -E "^ named \| (unadjudicated/cw-2026-08-22-0321(13|29)|unadjudicated/cw-2026-09-23-173723) " "$f" | cut -c1-400
done
echo "== HEAD text, 032113 and 032129"
grep -a -E "^ text \| unadjudicated/cw-2026-08-22-0321(13|29) " .run-unit/unit460-text-before.sorted.txt
echo "== what each fall commit touched"
git show --stat --format="%h %ad %s" --date=iso 42d5dbb9 | head -12
git show --stat --format="%h %ad %s" --date=iso b4884813 | head -12
git show --stat --format="%h %s" 83a652d9 -- src | head -5
git show --stat --format="%h %s" 3cf9f9ea | head -8
