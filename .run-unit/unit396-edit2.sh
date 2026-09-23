cd /c/Source/HamLet
# The task label comes from .run-unit/unit396-task.txt, one line, e.g. TASK 1 of 4.
for f in piece measure build drop; do
  sed -i -e "s/\${TL:-TASK 2 of 4}/\$(cat .run-unit\/unit396-task.txt)/g" .run-unit/unit396-$f.sh
done
grep -n "unit396-task.txt" .run-unit/unit396-*.sh | cut -c1-140
