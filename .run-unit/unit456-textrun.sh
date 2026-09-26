#!/bin/sh
# unit 456 - without building, print every recording's text, sort it, and diff it against the before.
# Usage: sh .run-unit/unit456-textrun.sh <suffix> "<n of 5>" "<note>"
cd /c/Source/HamLet || exit 1
export LC_ALL=C
sh .run-unit/unit456-run.sh "text-$1" engine 600 "$2" "$3" "FullyQualifiedName~.WhereTheSureAddedLettersComeFromTests.EveryRecordingAsTheOperatorReadsIt" --no-build
grep -aE "^\s*(text|callsigns) \| " .run-unit/unit456-text-$1.txt | sort > .run-unit/unit456-text-$1.sorted.txt
wc -l .run-unit/unit456-text-$1.sorted.txt
diff .run-unit/unit456-text-before.sorted.txt .run-unit/unit456-text-$1.sorted.txt
