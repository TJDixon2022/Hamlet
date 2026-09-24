#!/bin/sh
# unit 415 - copy unit 414's runner scripts under the unit 415 name, name the unit in PHASE_STATUS.md.
cd /c/Source/HamLet || exit 1
for n in run cf commit build transmit round same
do
  sed "s/unit414/unit415/g; s/unit 414/unit 415/g; s/of 5>/of 4>/g" .run-unit/unit414-$n.sh > .run-unit/unit415-$n.sh
done
sed "s/unit413/unit415/g; s/unit 413/unit 415/g" .run-unit/unit413-rows.sh > .run-unit/unit415-rows.sh
ls .run-unit/unit415*
sed -i "s/^WORK_INSTRUCTION: 414 .*/WORK_INSTRUCTION: 415 - the space is decided after the letters/" PHASE_STATUS.md
grep -E "WORK_INSTRUCTION|CURRENT_STEP" PHASE_STATUS.md
sh tools/status.sh EXECUTING "TASK 0 of 4" code none "Gate passed; version 1.13.101 to 1.13.102; verifying section 5 against the tree before the entry round"
head -11 PROJECT_STATUS.md
git rev-parse HEAD
