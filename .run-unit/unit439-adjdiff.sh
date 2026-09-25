cd /c/Source/HamLet
for f in entry bridge
do
  grep -E "characters, [0-9]+ %|adjudicated characters|decoded |text " .run-unit/unit439-adjudicated-$f.txt | cut -c1-220 > .run-unit/unit439-adj-$f-lines.txt
done
echo "=== diff entry vs bridge"
diff .run-unit/unit439-adj-entry-lines.txt .run-unit/unit439-adj-remix-lines.txt
echo "=== the three, entry"
grep -E "013347|003758|012403" .run-unit/unit439-adjudicated-entry.txt | grep -v Passed | cut -c1-240
echo "=== the three, bridge"
grep -E "013347|003758|012403" .run-unit/unit439-adjudicated-bridge.txt | grep -v Passed | cut -c1-240
