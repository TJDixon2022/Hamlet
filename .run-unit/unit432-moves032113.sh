#!/bin/sh
# unit 432 task 1 - 032113's mix moves in each WhyTheMixMoved print on record: unit 430 entry, change, variant, and this unit's entry.
cd /c/Source/HamLet/.run-unit || exit 1
for f in unit430-t1-why unit430-t2-why-change unit430-t2v-why unit432-type-entry-WhatTheOpeningHeardTests
do
  echo "== $f"
  awk '/check \| run cw-2026-08-22-032113/{p=1} /check \| run (stream|cw-2026-09)/{p=0} p' $f.txt | grep -E "check \||mix moved|survey \| 2[0-9]\.|survey \| 3[0-9]\." | cut -c1-230 | head -40
done
