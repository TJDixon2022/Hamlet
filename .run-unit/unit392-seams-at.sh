#!/bin/sh
# Unit 392 task 1, 0.4 at 7e209cb4: unit 391's seams-at logic, copied. For every seam row
# (file | type | declaring file | members), whether the type and each member exist under
# src/Hamlet.RadioEngine/Cw at the named commit. By git grep; approximate.
# Usage: sh .run-unit/unit392-seams-at.sh <hash>
h=$1
cd /c/Source/HamLet || exit 1
out=.run-unit/unit392-seams-at-$h.txt
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
echo "rows: $(wc -l < $out)"
echo "type absent: $(grep -c "TYPE ABSENT" $out)"
echo "rows with something missing: $(grep -vc "missing: none" $out)"
grep -v "missing: none" $out
