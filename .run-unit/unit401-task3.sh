cd /c/Source/HamLet
cat .run-unit/unit401-closing.txt >> docs/unit239-failing-set.txt
echo "set diff:"; git diff -- docs/unit239-failing-set.txt | cut -c1-120
wc -l docs/unit239-failing-set.txt
echo "carry diff stat:"; git diff --stat -- docs/carry-forward-tests.txt
file docs/carry-forward-tests.txt
echo "grep Cw CW carry:"; grep -n "Cw\|CW" docs/carry-forward-tests.txt | cut -c1-200
echo "line 9 tail:"; sed -n 9p docs/carry-forward-tests.txt | tr -d "\r" | tail -c 200; echo
echo "line 9 terms:"; sed -n 9p docs/carry-forward-tests.txt | grep -o "FullyQualifiedName~" | wc -l
sh .run-unit/unit401-carry.sh eng t3 "TASK 3 of 5" "task 3: block and closing line written; engine line with CwFixtureTests.TheCleanRecordingsDecodeExactly added running, one build, timeout 480"
grep -E "TheCleanRecordingsDecodeExactly" .run-unit/unit401-carry-t3-eng.txt | head -4
date +%H:%M:%S
