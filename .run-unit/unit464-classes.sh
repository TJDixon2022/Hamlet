#!/bin/sh
# unit 463 - each of our characters with its class and the scorer's verdict, from 462's committed fact, sorted.
# Usage: sh .run-unit/unit464-classes.sh <suffix> "<n of 4>"
cd /c/Source/HamLet || exit 1
export LC_ALL=C
sh .run-unit/unit464-run.sh "classes-$1" engine 600 "$2" "Saving each of our characters with its class and the scorer verdict, from 462s fact" "FullyQualifiedName~.WhatFldigisEdgesWouldMoveFact." --no-build
grep -a "^ text-class | " .run-unit/unit464-classes-$1.txt | sort > .run-unit/unit464-text-$1-classes.txt
wc -l .run-unit/unit464-text-$1-classes.txt
cmp .run-unit/unit462-text-before-classes.txt .run-unit/unit464-text-$1-classes.txt && echo "classes IDENTICAL to 462s before"
