cd /c/Source/HamLet
# For each line of the failing set, find it as Passed or Failed in this unit's outputs
cat .run-unit/unit394-set-*.txt .run-unit/unit394-floors-1.txt | grep -E "^\s+(Passed|Failed) Hamlet" | sed -E "s/^\s+//" | sed -E "s/ \[[^]]*\]\s*$//" | tr -d "\r" > .run-unit/unit394-results.txt
n=0
tr -d "\r" < docs/unit239-failing-set.txt | while IFS= read -r line; do
  n=$((n+1))
  r=$(grep -F -x -e "Passed $line" -e "Failed $line" .run-unit/unit394-results.txt | head -1 | cut -d" " -f1)
  [ -z "$r" ] && r=NOTFOUND
  echo "$n|$r|$line"
done
