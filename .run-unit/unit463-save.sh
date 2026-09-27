#!/bin/sh
# unit 463 - one screened form: its patch and full numbers saved under .run-unit, then the working tree's src reverted, confirmed clean, rebuilt.
# Usage: sh .run-unit/unit463-save.sh <A1|A2> "<n of 4>" "<verdict, one line>"
cd /c/Source/HamLet || exit 1
T=$1
P=.run-unit/unit463-$T.patch
N=.run-unit/unit463-$T.txt
git diff -- src > $P
wc -l $P
{
  echo "unit 463 - form $T, screened in the working tree under R78 (DECIDED (3)): $3"
  echo ""
  echo "== the four metrics per condition, task 0's entry then with $T in"
  grep -a -E "^ total \|" .run-unit/unit463-metrics-entry.txt
  grep -a -E "^ total \|" .run-unit/unit463-metrics-screen-$T.txt
  echo ""
  echo "== per condition, as parity.md tables them, entry then with $T in"
  grep -E "^\| \*\*(real HF|synthetic), all\*\*" .run-unit/unit463-parity-entry.md
  grep -E "^\| \*\*(real HF|synthetic), all\*\*" .run-unit/unit463-parity-screen-$T.md
  echo ""
  echo "== adjudicated readings with $T in"
  grep -a -E "Total tests|Passed:|Failed:" .run-unit/unit463-adjudicated-screen-$T.txt
  grep -a -E "^\s+Failed " .run-unit/unit463-adjudicated-screen-$T.txt | cut -c1-220
  echo ""
  echo "== silence with $T in: TheSilencePropertyIsLockedTests, NothingIsReadFromAudioWithNoKeyingTests, the captures floor"
  grep -a -E "Total tests|Passed:|Failed:" .run-unit/unit463-silence-screen-$T.txt
  grep -a -E "Total tests|Passed:|Failed:" .run-unit/unit463-nokeying-screen-$T.txt
  grep -a -E "Total tests|Passed:|Failed:" .run-unit/unit463-captures-screen-$T.txt
  grep -a -E "^\s+Failed .*(014854|014935)" .run-unit/unit463-captures-screen-$T.txt
  echo ""
  echo "== the gate room through the stream, before then with $T in"
  grep -a "gate-stream |" .run-unit/unit463-gate-before.txt
  grep -a "gate-stream |" .run-unit/unit463-gate-screen-$T.txt
  echo ""
  echo "== decode time, entry then with $T in (ours s over the audio s)"
  grep -a "^| real HF, inferred keys | 23" .run-unit/unit463-parity-entry.md .run-unit/unit463-parity-screen-$T.md
  grep -a "^| synthetic, exact keys | 12" .run-unit/unit463-parity-entry.md .run-unit/unit463-parity-screen-$T.md
  echo ""
  echo "== the port's texts with $T in against task 0's"
  cmp .run-unit/unit463-port-before.txt .run-unit/unit463-port-screen-$T.txt && echo "byte-identical"
  diff .run-unit/unit463-port-before.txt .run-unit/unit463-port-screen-$T.txt
  echo ""
  echo "== V-11: every recording's row, task 0's (<) against $T in (>); rows not printed did not move"
  diff .run-unit/unit463-v11-before.txt .run-unit/unit463-v11-screen-$T.txt
  echo ""
  echo "== V-11: the full table with $T in"
  cat .run-unit/unit463-v11-screen-$T.txt
} > $N 2>&1
wc -l $N
git checkout -- src
echo "== git diff -- src (empty):"
git diff -- src
echo "== end"
git status --short -- src
sh .run-unit/unit463-build.sh "revert-$T" "$2" "Screen $T saved and reverted; rebuilding the tree as committed"
tail -3 .run-unit/unit463-build-revert-$T.txt
