cd /c/Source/HamLet
# Usage: sh .run-unit/unit401-cmp.sh <before captures output> <after captures output>
sh .run-unit/unit401-nums.sh $1 > .run-unit/unit401-cmp-a.txt
sh .run-unit/unit401-nums.sh $2 > .run-unit/unit401-cmp-b.txt
echo "rows before $(wc -l < .run-unit/unit401-cmp-a.txt) after $(wc -l < .run-unit/unit401-cmp-b.txt)"
diff .run-unit/unit401-cmp-a.txt .run-unit/unit401-cmp-b.txt
echo "diff rc $?"
