#!/bin/sh
# unit 413 - copy unit 412's runner scripts under the unit 413 name, name the unit in PHASE_STATUS.md.
cd /c/Source/HamLet || exit 1
for n in run cf commit build transmit round
do
  sed "s/unit412/unit413/g; s/unit 412/unit 413/g" .run-unit/unit412-$n.sh > .run-unit/unit413-$n.sh
done
ls .run-unit/unit413*
sed -i "s/^WORK_INSTRUCTION: 412 .*/WORK_INSTRUCTION: 413 - the words stop shattering/; s/^CURRENT_STEP: 1$/CURRENT_STEP: 3/" PHASE_STATUS.md
grep -E "WORK_INSTRUCTION|CURRENT_STEP" PHASE_STATUS.md
sh tools/status.sh EXECUTING "TASK 0 of 4" code none "Gate passed; runner scripts copied from 412; verifying the instruction against the tree before the record"
head -11 PROJECT_STATUS.md
git rev-parse HEAD
