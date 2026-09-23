cd /c/Source/HamLet
# Usage: sh .run-unit/unit395-cmp.sh <before captures output> <after captures output>
sh .run-unit/unit395-nums.sh $1 > .run-unit/unit395-cmp-a.txt
sh .run-unit/unit395-nums.sh $2 > .run-unit/unit395-cmp-b.txt
echo "rows before $(wc -l < .run-unit/unit395-cmp-a.txt) after $(wc -l < .run-unit/unit395-cmp-b.txt)"
diff .run-unit/unit395-cmp-a.txt .run-unit/unit395-cmp-b.txt
echo "diff rc $?"
