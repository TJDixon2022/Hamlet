cd /c/Source/HamLet
# Usage: sh .run-unit/unit402-adjcmp.sh <before> <after> - the adjudicated type's printed lines, timings stripped
grep -E "^\s+Passed |^\s+Failed |^ [^ ]" $1 | sed -E "s/ \[[0-9.]+ m?s\]//" | sed -E "s/^\s+//" | sort > .run-unit/unit402-adj-a.txt
grep -E "^\s+Passed |^\s+Failed |^ [^ ]" $2 | sed -E "s/ \[[0-9.]+ m?s\]//" | sed -E "s/^\s+//" | sort > .run-unit/unit402-adj-b.txt
echo "lines before $(wc -l < .run-unit/unit402-adj-a.txt) after $(wc -l < .run-unit/unit402-adj-b.txt)"
diff .run-unit/unit402-adj-a.txt .run-unit/unit402-adj-b.txt | cut -c1-260
echo "diff rc $?"
