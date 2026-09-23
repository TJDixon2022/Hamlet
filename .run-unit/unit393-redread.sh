cd /c/Source/HamLet
echo "== the five that stayed green, their Retired field"
grep -E "^\s+Passed .*(134712|031905|032050|032113|032129)" .run-unit/unit393-guard-red.txt | sed -E "s/.*Name = //" | sed -E "s/Adjudicated = .*Retired =/Retired =/" | cut -c1-230
echo "== failure messages"
grep -A2 "Error Message" .run-unit/unit393-guard-red.txt | grep -vE "Error Message|^--" | sed -E "s/^\s+//" | cut -c1-220
echo "== retired rule in the test"
grep -n "Retired" tests/Hamlet.RadioEngine.Tests/Cw/TheAdjudicatedReadingsKeepReadingTests.cs | head -20
