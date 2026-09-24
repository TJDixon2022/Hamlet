#!/bin/sh
# unit 424 task 2 - rebuild, then the three types the new row turned red, one per invocation.
cd /c/Source/HamLet || exit 1
sh .run-unit/unit424-build.sh t2-b "TASK 2 of 4" "Task 2: rebuilding after moving the three 40 m tests onto the manual's row" || exit 1
for T in EveryMorseBlockSetsWhatCwSetsTests TheOperatorsHandCrossesTheMorseBlocksTests ThePreampFollowsItsOwnTextTests ThePreampIsWhatTheManualSaysTests
do
  sh .run-unit/unit424-run.sh "type-t2b-$T" engine 180 "TASK 2 of 4" "Task 2: $T alone after the row change" "FullyQualifiedName~Hamlet.RadioEngine.Tests.Rig.$T." --no-build
done
