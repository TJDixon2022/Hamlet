#!/bin/sh
# unit 431 task 4 - the rest of the exit round, one type per invocation: adjudicated, keyed floors, keyed totals, baseline, and the dispatcher-loop loss alone.
cd /c/Source/HamLet || exit 1
T="TASK 4 of 4"
for R in adjudicated named keyed baseline
do
  echo "== $R"
  sh .run-unit/unit431-round.sh $R exit "$T"
done
echo "== alone"
grep -q "You've caused dispatcher loop" .run-unit/unit431-cf-app-exit.txt && echo "the app loss is the dispatcher loop"
sh .run-unit/unit431-alone.sh exit "$T" TheCarrierHoldsTheButtonsTests
