#!/bin/sh
# unit 414 - copy unit 413's runner scripts under the unit 414 name, name the unit in PHASE_STATUS.md.
cd /c/Source/HamLet || exit 1
for n in run cf commit build transmit round
do
  sed "s/unit413/unit414/g; s/unit 413/unit 414/g; s/of 4>/of 5>/g" .run-unit/unit413-$n.sh > .run-unit/unit414-$n.sh
done
ls .run-unit/unit414*
sed -i "s/^WORK_INSTRUCTION: 413 .*/WORK_INSTRUCTION: 414 - keys nobody has to guess/" PHASE_STATUS.md
grep -E "WORK_INSTRUCTION|CURRENT_STEP" PHASE_STATUS.md
sh tools/status.sh EXECUTING "TASK 0 of 5" code none "Gate passed; version 1.13.101; verifying section 5 against the tree before the entry round"
head -11 PROJECT_STATUS.md
git rev-parse HEAD
