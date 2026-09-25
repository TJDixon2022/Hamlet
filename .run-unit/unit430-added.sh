#!/bin/sh
# unit 430 - sum the stray trace's per-recording count rows: named, right, wrong, added, added single-element.
# Usage: sh .run-unit/unit430-added.sh <entry|exit>
cd /c/Source/HamLet/.run-unit || exit 1
grep -E "^ count \| (unadj|cw-)" unit430-strays-$1.txt | awk -F"|" '{n+=$4; r+=$5; w+=$6; a+=$8; s+=$9} END {print "named",n,"right",r,"wrong",w,"added",a,"added single",s,"rows",NR}'
