#!/bin/sh
# unit 455 - build, then print every capture's and synthetic case's text as the operator reads it.
# Usage: sh .run-unit/unit455-text.sh <suffix> "<n of 4>" "<note>"
cd /c/Source/HamLet || exit 1
sh .run-unit/unit455-build.sh "text-$1" "$2" "$3: building"
sh .run-unit/unit455-run.sh "text-$1" engine 600 "$2" "$3: printing every recording's text" "FullyQualifiedName~.WhereTheSureAddedLettersComeFromTests.EveryRecordingAsTheOperatorReadsIt" --no-build
grep -E "^\s*(text|callsigns) \| " .run-unit/unit455-text-$1.txt | sort > .run-unit/unit455-text-$1.sorted.txt
wc -l .run-unit/unit455-text-$1.sorted.txt
