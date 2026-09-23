cd /c/Source/HamLet
# Usage: sh .run-unit/unit396-code.sh <patch> - the changed lines that are neither comment nor blank
grep -E "^[+-]" $1 | grep -vE "^(\+\+\+|---)" | grep -vE "^[+-][[:space:]]*//" | grep -vE "^[+-][[:space:]]*$" | cut -c1-160
echo "code lines: $(grep -E "^[+-]" $1 | grep -vE "^(\+\+\+|---)" | grep -vE "^[+-][[:space:]]*//" | grep -vcE "^[+-][[:space:]]*$")"
