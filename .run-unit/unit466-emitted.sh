#!/bin/sh
# unit 466 task 3 - save the harness output emitted under the switch, compare it with task 0's arbitrated save, and name every recording that differs.
# Usage: sh .run-unit/unit466-emitted.sh <suffix> "<n of 4>"
cd /c/Source/HamLet || exit 1
export LC_ALL=C
sh .run-unit/unit466-run.sh "save-emitted-$1" engine 600 "$2" "Saving the harness output emitted under the switch table, all 35, with class and p" "FullyQualifiedName~.TheArbitrationEarnsItsPlaceFact.TheEmittedTranscriptWithClassAndP" --no-build
grep -a "^ *save | ours | " .run-unit/unit466-save-emitted-$1.txt | sed 's/^ *//' > .run-unit/unit466-emitted-$1.txt
grep -a "^ *switch in force | " .run-unit/unit466-save-emitted-$1.txt | sed 's/^ *//' | grep -v "| arbitrate$"
wc -l .run-unit/unit466-emitted-$1.txt
cmp .run-unit/unit466-arb-before.txt .run-unit/unit466-emitted-$1.txt && echo "emitted harness BYTE-IDENTICAL to task 0's arbitrated save"
echo "== recordings whose emitted text differs from task 0's arbitrated save"
diff .run-unit/unit466-arb-before.txt .run-unit/unit466-emitted-$1.txt | grep "^[<>]" | cut -d"|" -f3 | sort | uniq -c
echo "== every other recording's lines, byte-identical?"
grep -v "| cq-18wpm-5db-char5 |" .run-unit/unit466-arb-before.txt > .run-unit/unit466-arb-before-others.tmp
grep -v "| cq-18wpm-5db-char5 |" .run-unit/unit466-emitted-$1.txt > .run-unit/unit466-emitted-others.tmp
cmp .run-unit/unit466-arb-before-others.tmp .run-unit/unit466-emitted-others.tmp && echo "all 34 other recordings BYTE-IDENTICAL to task 0's arbitrated save"
echo "== cq-18wpm-5db-char5, before and after"
grep "| cq-18wpm-5db-char5 |" .run-unit/unit466-arb-before.txt | cut -d"|" -f5 | tr -d "\n"
echo ""
grep "| cq-18wpm-5db-char5 |" .run-unit/unit466-emitted-$1.txt | cut -d"|" -f5 | tr -d "\n"
echo ""
