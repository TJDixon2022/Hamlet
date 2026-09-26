#!/bin/sh
# unit 457 - without building, print every recording's text, sort it, and diff it against the before.
# Usage: sh .run-unit/unit457-textrun.sh <suffix> "<n of 5>" "<note>"
cd /c/Source/HamLet || exit 1
export LC_ALL=C
sh .run-unit/unit457-run.sh "text-$1" engine 600 "$2" "$3" "FullyQualifiedName~.WhereTheSureAddedLettersComeFromTests.EveryRecordingAsTheOperatorReadsIt" --no-build
grep -aE "^\s*(text|callsigns) \| " .run-unit/unit457-text-$1.txt | sort > .run-unit/unit457-text-$1.sorted.txt
wc -l .run-unit/unit457-text-$1.sorted.txt
diff .run-unit/unit457-text-before.sorted.txt .run-unit/unit457-text-$1.sorted.txt
