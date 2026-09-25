#!/bin/sh
# unit 431 - the single-element characters on 17:37 and 004133 at entry and at a suffix, with label, time, text and raw span.
# Usage: sh .run-unit/unit431-left.sh <suffix>
cd /c/Source/HamLet/.run-unit || exit 1
for f in entry $1
do
  echo "== $f"
  grep -hE "^ single \| unadjudicated/cw-2026-09-2[34]-(173723|004133) " unit431-strays-$f.txt | awk -F"|" '{print $2, $3, $4, $5, $7}'
done
