#!/bin/sh
# unit 460 task 2 - how many settled letters are sure, sure and right, and sure and right with their own span found.
cd /c/Source/HamLet || exit 1
F=.run-unit/unit460-pair-$1.txt
grep -a "^ letter |" $F | awk -F' *[|] *' '$7 == "Sure" { s++ } $7 == "Sure" && $10 == "right" { r++ } $7 == "Sure" && $10 == "right" && $13 != "" { u++ } $7 == "Sure" && $13 != "" { su++ } END { print "sure " s " | sure and right " r " | sure and right with a span " u " | sure with a span " su }'
grep -a "^ letter |" $F | awk -F' *[|] *' '$7 == "Sure" && $10 == "right"' | head -8
