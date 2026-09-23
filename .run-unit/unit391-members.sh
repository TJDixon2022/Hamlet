#!/bin/sh
# Unit 391 task 2 members column: for each seam file (one that has using Hamlet.RadioEngine.Cw),
# the Cw types it names and the members of those types it touches, by grep.
# A member is counted when the seam file has Type.Member, or .Member where Member is declared
# public in the file that declares Type. Approximate by construction; said so in the report.
cd /c/Source/HamLet || exit 1
out=.run-unit/unit391-members.txt
: > $out
grep '| using=[1-9]' .run-unit/unit391-seams.txt | while IFS='|' read -r f u types; do
  f=$(echo $f)
  for t in $types; do
    decl=$(grep -lE "(class|struct|record|enum|interface|record struct) $t\b" $(cat .run-unit/unit391-cwfiles.txt) | head -1)
    [ -z "$decl" ] && continue
    # Public member names declared in that file (properties, methods, fields, enum values).
    grep -oE 'public [^=;(]*[ ]([A-Z][A-Za-z0-9_]*)[ ]*(\(|\{|=|;|$)' "$decl" | sed -E 's/[ ]*(\(|\{|=|;)$//' | awk '{print $NF}' | sort -u > .run-unit/unit391-m.tmp
    grep -oE '^[ ]+[A-Z][A-Za-z0-9_]*,?$' "$decl" | tr -d ' ,' >> .run-unit/unit391-m.tmp
    used=""
    for m in $(sort -u .run-unit/unit391-m.tmp); do
      [ "$m" = "$t" ] && continue
      if grep -qE "\.$m\b" "$f"; then used="$used $m"; fi
    done
    echo "$f | $t | $(basename $decl) |$used" >> $out
  done
done
