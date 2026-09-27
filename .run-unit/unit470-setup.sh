#!/bin/sh
# unit 470 - copy 469's generic helpers under 470's name, and write the start status.
cd /c/Source/HamLet/.run-unit || exit 1
for n in run build round cf commitrun paritykeep gitstate validate v11 alone save meetsave portdiff stamp status extra
do
  sed 's/unit469/unit470/g' unit469-$n.sh > unit470-$n.sh
done
ls unit470-*
cd /c/Source/HamLet || exit 1
sh tools/status.sh EXECUTING "0 of 3" code none "Unit 470 starting - project gate passed; checking HEAD, both plan copies, the RESOLVED lines and both proof states in src before the record is written"
head -12 PROJECT_STATUS.md
