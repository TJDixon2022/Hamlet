#!/bin/sh
# unit 442 - write the red rows' before-and-after file and the per-recording V-11 beside it.
cd /c/Source/HamLet || exit 1
{
  echo "# Unit 442 task 1 - the floor rows red at 0439a8e7, before (f14b2453, 441's entry) and at HEAD"
  echo
  sh .run-unit/unit442-rows.sh .run-unit/unit442-floortext-before.txt .run-unit/unit442-floortext-head.txt
  echo
  echo "# V-11 per keyed recording across 441's two changes (unit441-metrics-r82.txt to unit442-metrics-entry.txt)"
  sh .run-unit/unit442-v11.sh .run-unit/unit441-metrics-r82.txt .run-unit/unit442-metrics-entry.txt
} > .run-unit/unit442-floorrows.txt
wc -l .run-unit/unit442-floorrows.txt
