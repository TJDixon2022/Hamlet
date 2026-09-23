#!/bin/sh
# Unit 392 task 1, items 3 and 4: the types declared in HEAD-only Cw files, who outside Cw
# names them, and which engine tests name them. By grep.
cd /c/Source/HamLet || exit 1
out=.run-unit/unit392-headonly.txt
: > $out
A=$(git diff --name-status 7e209cb4 HEAD -- src/Hamlet.RadioEngine/Cw | sed -n "s/^A\t//p")
for f in $A; do
  types=$(grep -oE "(class|struct|record|enum|interface) [A-Z][A-Za-z0-9_]*" "$f" | awk "{print \$2}" | sort -u | tr "\n" " ")
  echo "FILE $f | types: $types" >> $out
  for t in $types; do
    # skip types that also exist at 7e209cb4 anywhere under Cw
    if git grep -qE "(class|struct|record|enum|interface) $t\b" 7e209cb4 -- src/Hamlet.RadioEngine/Cw; then
      echo "  $t also declared at 7e209cb4 - not HEAD-only" >> $out
      continue
    fi
    users=$(git grep -lw "$t" HEAD -- src tools tests ":!src/Hamlet.RadioEngine/Cw" ":!tests/Hamlet.RadioEngine.Tests/Cw" | sed "s/^HEAD://" | tr "\n" " ")
    eng=$(git grep -lw "$t" HEAD -- tests/Hamlet.RadioEngine.Tests/Cw | sed "s/^HEAD://" | tr "\n" " ")
    incw=$(git grep -lw "$t" HEAD -- src/Hamlet.RadioEngine/Cw | sed "s/^HEAD://" | tr "\n" " ")
    echo "  TYPE $t" >> $out
    echo "    outside Cw: ${users:-none}" >> $out
    echo "    engine Cw tests: ${eng:-none}" >> $out
    echo "    inside Cw at HEAD: ${incw:-none}" >> $out
  done
done
cat $out
