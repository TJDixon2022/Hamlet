#!/bin/sh
# unit 477 - make this unit's helper scripts from unit 476's: names, work instruction, rules line, entry commit.
cd /c/Source/HamLet/.run-unit || exit 1
for f in status commit build cf run exit-print
do
  sed -e 's/unit476/unit477/g' -e 's/unit 476/unit 477/g' \
      -e "s/476 - the oscilloscope: a mark is the envelope over a threshold, at any pitch/477 - bars, not waves/" \
      -e 's/HM-DEC-185 (2026-09-28)/HM-DEC-186 (2026-09-28)/' \
      -e 's/ba7ef00a/4d1d24cf/g' \
      unit476-$f.sh > unit477-$f.sh
done
grep -n "477\|186\|4d1d24cf" unit477-status.sh unit477-exit-print.sh | cut -c1-140 | head
git -C /c/Source/HamLet rev-parse --short=8 HEAD
git -C /c/Source/HamLet status --short
git -C /c/Source/HamLet diff --stat
