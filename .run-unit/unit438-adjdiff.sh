cd /c/Source/HamLet
for f in entry local
do
  grep -E "characters, [0-9]+ %|adjudicated characters|decoded |text " .run-unit/unit438-adjudicated-$f.txt | cut -c1-220 > .run-unit/unit438-adj-$f-lines.txt
done
echo "=== diff entry vs local"
diff .run-unit/unit438-adj-entry-lines.txt .run-unit/unit438-adj-remix-lines.txt
echo "=== the three, entry"
grep -E "013347|003758|012403" .run-unit/unit438-adjudicated-entry.txt | grep -v Passed | cut -c1-240
echo "=== the three, local"
grep -E "013347|003758|012403" .run-unit/unit438-adjudicated-local.txt | grep -v Passed | cut -c1-240
