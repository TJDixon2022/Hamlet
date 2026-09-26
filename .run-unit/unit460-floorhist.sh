#!/bin/sh
# unit 460 task 1 - the three red floors' named counts across every committed named printout, in unit order.
cd /c/Source/HamLet || exit 1
export LC_ALL=C
for f in $(git ls-files .run-unit | grep -E "unit4(2[0-9]|3[0-9]|4[0-9]|5[0-9]|60)-named-[a-z0-9]+\.txt$" | sort)
do
  grep -a -E "^ named \| (unadjudicated/cw-2026-08-22-0321(13|29)|unadjudicated/cw-2026-09-23-173723) " "$f" | awk -F'|' -v f="$f" '{gsub(/^ +| +$/, "", $2); split($3, a, " "); print f " | " $2 " | " a[1] " named of floor " $3}' | sed 's/at or above the span bar against a floor of //'
done
