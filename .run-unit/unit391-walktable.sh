#!/bin/sh
# Unit 391: one row per walked commit - index, hash, date, clean-12wpm reading, result.
cd /c/Source/HamLet || exit 1
git log --format=%h --since=2026-08-01 -- src/Hamlet.RadioEngine/Cw > .run-unit/unit391-candidates-all.txt
i=0
for h in $(cat .run-unit/unit391-candidates-all.txt); do
  i=$((i+1))
  f=.run-unit/unit391-floors-$h-3.txt
  [ -f $f ] || break
  d=$(git log --format="%ad" --date=short -1 $h)
  s=$(git log --format="%s" -1 $h | sed 's/|/-/g')
  p=$(grep -cE "^\s+Passed " $f)
  r=$(grep -cE "^\s+Failed " $f)
  a=$(grep -E "^Actual:" $f | head -1 | sed 's/^Actual: *//')
  [ "$r" = "0" ] && a="CQ DE W1AW K (exact)"
  echo "| $i | \`$h\` | $d | $p of 2 | \`$a\` | $s |"
done
