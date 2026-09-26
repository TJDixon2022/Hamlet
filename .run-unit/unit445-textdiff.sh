#!/bin/sh
# unit 445 - print every recording's text and callsigns (no build), sort, and diff against a before file.
# Usage: sh .run-unit/unit445-textdiff.sh <suffix> <before-suffix> "<n of 3>" "<note>"
cd /c/Source/HamLet || exit 1
sh .run-unit/unit445-run.sh "text-$1" engine 600 "$3" "$4" "FullyQualifiedName~.WhereTheSureAddedLettersComeFromTests.EveryRecordingAsTheOperatorReadsIt" --no-build
grep -E "^\s*(text|callsigns) \| " .run-unit/unit445-text-$1.txt | sort > .run-unit/unit445-text-$1.sorted.txt
wc -l .run-unit/unit445-text-$1.sorted.txt
diff .run-unit/unit445-text-$2.sorted.txt .run-unit/unit445-text-$1.sorted.txt > .run-unit/unit445-text-diff-$1.txt
cat .run-unit/unit445-text-diff-$1.txt
