cd /c/Source/HamLet
for f in entry rule
do
  grep -E "characters, [0-9]+ %|adjudicated characters|decoded |text " .run-unit/unit436-adjudicated-$f.txt | cut -c1-220 > .run-unit/unit436-adj-$f-lines.txt
done
echo "=== diff entry vs rule"
diff .run-unit/unit436-adj-entry-lines.txt .run-unit/unit436-adj-rule-lines.txt
echo "=== the three, under the rule"
grep -E "013347|003758|012403" .run-unit/unit436-adjudicated-rule.txt | grep -v Passed | cut -c1-240
