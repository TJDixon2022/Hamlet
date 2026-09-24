#!/bin/sh
# unit 418 - the signal lines of each regenerated sheet, old (the task 1 trace, before any fix) above new (unit418-sidecar), only where they differ.
cd /c/Source/HamLet || exit 1
OUT=.run-unit/unit418-oldnew.txt
: > $OUT
for s in cw-2026-09-23-173723 cw-2026-08-22-014113 cw-2026-08-17-013347
do
  awk -v s="$s" '
    index($0, "== " s ", the sheet as") { on = 1; next }
    on && /^ ?== / { on = 0 }
    on && /^ *\| / { sub(/^ *\| /, ""); print }
  ' .run-unit/unit418-trace-t1.txt | head -40 > .run-unit/unit418-old-$s.txt
  tail -n +2 .run-unit/unit418-sidecar-$s.txt | head -40 > .run-unit/unit418-new-$s.txt
  echo "== $s" >> $OUT
  paste -d '\n' .run-unit/unit418-old-$s.txt .run-unit/unit418-new-$s.txt | awk 'NR % 2 == 1 { old = $0; next } { if (old != $0) { print "old: " old; print "new: " $0 } }' >> $OUT
done
cat $OUT
