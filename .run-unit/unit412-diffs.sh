#!/bin/sh
# unit 412 - what each capture's text adds to the previous capture's, bracketed so spaces show.
cd /c/Source/HamLet || exit 1
D=tests/fixtures/cw/captured/unadjudicated
PREV=""
for f in $D/cw-2026-09-24-*.txt
do
  n=$(basename "$f" .txt)
  T=$(sed -n "s/^text       //p" "$f")
  ADD="${T#"$PREV"}"
  echo "$n [${ADD}]"
  PREV="$T"
done
