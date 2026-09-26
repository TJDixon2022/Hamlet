#!/bin/sh
# unit 445 - copy 444's helper scripts under 445's names, and check the tree's claims.
cd /c/Source/HamLet || exit 1
for n in build cf round run cmp commit tick v11 text percond; do
  sed "s/unit444/unit445/g; s/unit 444/unit 445/g; s/step 2 in both/step 3 in both/" .run-unit/unit444-$n.sh > .run-unit/unit445-$n.sh
done
ls .run-unit/unit445-*
date "+%Y-%m-%dT%H:%M:%S%:z"
git rev-parse HEAD
echo "--- stream near 685"
sed -n 670,700p src/Hamlet.RadioEngine/Cw/CwProbabilisticStream.cs
echo "--- CwConfidence"
grep -n "Low\|High\|Unreadable\|dim" src/Hamlet.RadioEngine/Cw/CwCharacter.cs
echo "--- resolver and tracker on High"
grep -rn "CwConfidence\.\|Confidence ==\|Confidence !=" src --include=*.cs | grep -i "resolver\|tracker"
echo "--- trace exit"
ls -la .run-unit/unit441-trace-exit.txt
grep -n "442" docs/phase-requirements/metrics.md | head -20
