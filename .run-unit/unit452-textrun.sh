#!/bin/sh
# unit 452 - without building, print every recording's text, sort it, and diff it against the before.
# Usage: sh .run-unit/unit452-textrun.sh <suffix> "<n of 3>" "<note>"
cd /c/Source/HamLet || exit 1
export LC_ALL=C
sh .run-unit/unit452-run.sh "text-$1" engine 600 "$2" "$3" "FullyQualifiedName~.WhereTheSureAddedLettersComeFromTests.EveryRecordingAsTheOperatorReadsIt" --no-build
grep -aE "^\s*(text|callsigns) \| " .run-unit/unit452-text-$1.txt | sort > .run-unit/unit452-text-$1.sorted.txt
wc -l .run-unit/unit452-text-$1.sorted.txt
diff .run-unit/unit452-text-before.sorted.txt .run-unit/unit452-text-$1.sorted.txt
