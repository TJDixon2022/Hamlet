#!/bin/sh
# unit 404 task 1 - which new Cw files each piece creates, and which pieces name them in added lines.
cd /c/Source/HamLet || exit 1
P=.run-unit/unit404-patches
echo "== files created (new file mode) per piece"
for f in $P/*.patch; do
  n=$(basename $f | cut -c1-2)
  c=$(grep -B1 "^new file mode" $f | grep "^diff --git" | sed 's#.*b/src/Hamlet.RadioEngine/Cw/##' | tr '\n' ' ')
  d=$(grep -A1 "^diff --git" $f | grep -B1 "^deleted file mode" | grep "^diff --git" | sed 's#.*b/src/Hamlet.RadioEngine/Cw/##' | tr '\n' ' ')
  [ -n "$c$d" ] && echo "$n created: $c deleted: $d"
done
echo "== added lines naming a type some piece creates, and whether HEAD has it"
for t in CwJointCutter CwPitchRanking CwReferenceDecoder CwAccuracy CwSpectralPeak CwSwingSurvey CwElementPitch CwElement CwStreamSplit CwCounterTrail Posterior; do
  inhead=$(grep -rl "\b$t\b" src/Hamlet.RadioEngine/Cw | wc -l)
  users=$(grep -l "^+.*\b$t\b" $P/*.patch | xargs -n1 basename 2>/dev/null | cut -c1-2 | tr '\n' ' ')
  echo "$t head-files=$inhead added-by: $users"
done
