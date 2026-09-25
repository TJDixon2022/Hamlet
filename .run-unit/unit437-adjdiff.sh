cd /c/Source/HamLet
for f in entry remix
do
  grep -E "characters, [0-9]+ %|adjudicated characters|decoded |text " .run-unit/unit437-adjudicated-$f.txt | cut -c1-220 > .run-unit/unit437-adj-$f-lines.txt
done
echo "=== diff entry vs remix"
diff .run-unit/unit437-adj-entry-lines.txt .run-unit/unit437-adj-remix-lines.txt
echo "=== the three, entry"
grep -E "013347|003758|012403" .run-unit/unit437-adjudicated-entry.txt | grep -v Passed | cut -c1-240
echo "=== the three, remix"
grep -E "013347|003758|012403" .run-unit/unit437-adjudicated-remix.txt | grep -v Passed | cut -c1-240
