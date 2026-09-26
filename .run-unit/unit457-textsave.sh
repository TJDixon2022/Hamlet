#!/bin/sh
# unit 457 - without building, print every recording's text and keep the sorted lines as <suffix>.
# Usage: sh .run-unit/unit457-textsave.sh <suffix> "<n of 5>" "<note>" [compare-suffix]
cd /c/Source/HamLet || exit 1
export LC_ALL=C
sh .run-unit/unit457-run.sh "text-$1" engine 600 "$2" "$3" "FullyQualifiedName~.WhereTheSureAddedLettersComeFromTests.EveryRecordingAsTheOperatorReadsIt" --no-build
grep -aE "^\s*(text|callsigns) \| " .run-unit/unit457-text-$1.txt | sort > .run-unit/unit457-text-$1.sorted.txt
wc -l .run-unit/unit457-text-$1.sorted.txt
if [ -n "$4" ]; then
  diff .run-unit/unit457-text-$4.sorted.txt .run-unit/unit457-text-$1.sorted.txt && echo "IDENTICAL to $4"
fi
diff .run-unit/unit453-text-before.sorted.txt .run-unit/unit457-text-$1.sorted.txt > /dev/null && echo "identical to 453's before"
