cd /c/Source/HamLet
# Usage: sh .run-unit/unit397-append.sh <line file> - appends one line to PHASE_OUTCOME.md
cat $1 >> PHASE_OUTCOME.md
tail -c 300 PHASE_OUTCOME.md
echo
wc -l < PHASE_OUTCOME.md
