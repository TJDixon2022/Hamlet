#!/bin/sh
# unit 456 - without building, print every recording's text and keep the sorted lines as <suffix>.
# Usage: sh .run-unit/unit456-textsave.sh <suffix> "<n of 5>" "<note>" [compare-suffix]
cd /c/Source/HamLet || exit 1
export LC_ALL=C
sh .run-unit/unit456-run.sh "text-$1" engine 600 "$2" "$3" "FullyQualifiedName~.WhereTheSureAddedLettersComeFromTests.EveryRecordingAsTheOperatorReadsIt" --no-build
grep -aE "^\s*(text|callsigns) \| " .run-unit/unit456-text-$1.txt | sort > .run-unit/unit456-text-$1.sorted.txt
wc -l .run-unit/unit456-text-$1.sorted.txt
if [ -n "$4" ]; then
  diff .run-unit/unit456-text-$4.sorted.txt .run-unit/unit456-text-$1.sorted.txt && echo "IDENTICAL to $4"
fi
diff .run-unit/unit453-text-before.sorted.txt .run-unit/unit456-text-$1.sorted.txt > /dev/null && echo "identical to 453's before"
