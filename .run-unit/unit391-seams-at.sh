#!/bin/sh
# Unit 391 task 3, 0.4: for every seam row (file | type | declaring file | members), whether the type
# and each member exist under src/Hamlet.RadioEngine/Cw at the named commit. By git grep; approximate.
# Usage: sh .run-unit/unit391-seams-at.sh <hash>
h=$1
cd /c/Source/HamLet || exit 1
out=.run-unit/unit391-seams-at-$h.txt
: > $out
while IFS='|' read -r f t decl members; do
  f=$(echo $f); t=$(echo $t); decl=$(echo $decl)
  if git grep -qE "(class|struct|record|enum|interface) $t\b" $h -- src/Hamlet.RadioEngine/Cw; then
    tstate="type there"
    where=$(git grep -lE "(class|struct|record|enum|interface) $t\b" $h -- src/Hamlet.RadioEngine/Cw | head -1 | sed "s/^$h://")
  else
    tstate="TYPE ABSENT"
    where=""
  fi
  missing=""
  for m in $members; do
    if [ -n "$where" ] && git grep -qw "$m" $h -- "$where"; then :; else missing="$missing $m"; fi
  done
  echo "$f | $t | $tstate | missing:${missing:- none}" >> $out
done < .run-unit/unit391-members.txt
grep -c "TYPE ABSENT" $out
grep -vc "missing: none" $out
