#!/bin/sh
# unit 462 - a refused screen: its patch and full numbers saved under .run-unit, the working tree reverted, src confirmed clean, rebuilt.
# Usage: sh .run-unit/unit462-refuse.sh <B|C|W> "<n of 4>" "<why, one line>"
cd /c/Source/HamLet || exit 1
T=$1
P=.run-unit/unit462-$T-refused.patch
N=.run-unit/unit462-$T-refused.txt
git diff -- src > $P
wc -l $P
{
  echo "unit 462 - technique $T, screened in the working tree and refused under R78 (DECIDED (4)): $3"
  echo ""
  echo "== the four metrics per condition, task 0's entry then with $T in"
  grep -a -E "^ total \|" .run-unit/unit462-metrics-entry.txt
  grep -a -E "^ total \|" .run-unit/unit462-metrics-screen-$T.txt
  echo ""
  echo "== per condition, as parity.md tables them, with $T in"
  grep -E "^\| \*\*(real HF|synthetic), all\*\*" .run-unit/unit462-parity-screen-$T.md
  echo ""
  echo "== adjudicated readings with $T in"
  grep -a -E "Total tests|Passed:|Failed:" .run-unit/unit462-adjudicated-screen-$T.txt
  grep -a -E "^\s+Failed " .run-unit/unit462-adjudicated-screen-$T.txt
  echo ""
  echo "== decode time, entry then with $T in (ours s over the audio s)"
  grep -a "^| real HF, inferred keys | 23" .run-unit/unit462-parity-entry.md .run-unit/unit462-parity-screen-$T.md
  grep -a "^| synthetic, exact keys | 12" .run-unit/unit462-parity-entry.md .run-unit/unit462-parity-screen-$T.md
  echo ""
  echo "== the port's texts with $T in against task 0's"
  cmp .run-unit/unit462-port-before.txt .run-unit/unit462-port-screen-$T.txt && echo "byte-identical"
  echo ""
  echo "== V-11: every recording's row, task 0's (<) against $T in (>); rows not printed did not move"
  diff .run-unit/unit462-v11-before.txt .run-unit/unit462-v11-screen-$T.txt
  echo ""
  echo "== V-11: the full table with $T in"
  cat .run-unit/unit462-v11-screen-$T.txt
  echo ""
  echo "== our texts, task 0's (<) against $T in (>)"
  diff .run-unit/unit462-text-before.sorted.txt .run-unit/unit462-text-screen-$T.sorted.txt
} > $N 2>&1
wc -l $N
git checkout -- src
echo "== git diff -- src (empty):"
git diff -- src
echo "== end"
git status --short -- src
sh .run-unit/unit462-build.sh "revert-$T" "$2" "Screen $T refused and reverted; rebuilding the tree as committed"
tail -3 .run-unit/unit462-build-revert-$T.txt
