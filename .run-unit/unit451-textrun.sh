#!/bin/sh
# unit 451 - without building, print every recording's text, sort it, and diff it against the before.
# Usage: sh .run-unit/unit451-textrun.sh <suffix> "<n of 3>" "<note>"
cd /c/Source/HamLet || exit 1
export LC_ALL=C
sh .run-unit/unit451-run.sh "text-$1" engine 600 "$2" "$3" "FullyQualifiedName~.WhereTheSureAddedLettersComeFromTests.EveryRecordingAsTheOperatorReadsIt" --no-build
grep -aE "^\s*(text|callsigns) \| " .run-unit/unit451-text-$1.txt | sort > .run-unit/unit451-text-$1.sorted.txt
wc -l .run-unit/unit451-text-$1.sorted.txt
diff .run-unit/unit451-text-before.sorted.txt .run-unit/unit451-text-$1.sorted.txt
