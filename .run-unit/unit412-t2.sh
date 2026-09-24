#!/bin/sh
# unit 412 - task 2's types, one per invocation: the scorer's hand counts, the baseline printer, the named floors.
# Usage: sh .run-unit/unit412-t2.sh <suffix> "<TASK n of 4>" "<what is in the tree>" [--no-build]
cd /c/Source/HamLet || exit 1
R=.run-unit/unit412-run.sh
sh $R "scorer-$1" engine 300 "$2" "Task 2, $3: running TheScorerCountsWhatAHandCountsTests alone" "FullyQualifiedName~.TheScorerCountsWhatAHandCountsTests." $4
sh $R "baseline-$1" engine 300 "$2" "Task 2, $3: running TheBaselineIsScoredTests alone, edits and unsure per named" "FullyQualifiedName~.TheBaselineIsScoredTests." --no-build
sh $R "named-$1" engine 300 "$2" "Task 2, $3: running TheNumberCannotBeGamedTests alone, the named floors" "FullyQualifiedName~.TheNumberCannotBeGamedTests." --no-build
grep -a -E "^ *(row|total|outside|guard|named) \|" .run-unit/unit412-baseline-$1.txt .run-unit/unit412-named-$1.txt | sed "s/^\.run-unit\/unit412-//" | cut -c1-230
