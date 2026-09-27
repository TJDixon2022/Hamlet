#!/bin/sh
# unit 469 - copy 468's generic helpers under 469's name, and write the start status.
cd /c/Source/HamLet/.run-unit || exit 1
for n in run build round cf commitrun paritykeep tx gitstate validate v11 alone save live meetsave verify portdiff stamp status
do
  sed 's/unit468/unit469/g' unit468-$n.sh > unit469-$n.sh
done
ls unit469-*
cd /c/Source/HamLet || exit 1
sh tools/status.sh EXECUTING "0 of 3" code none "Unit 469 starting - project gate passed; checking HEAD, both plan copies, the channel and sender layers and the port diff before the record is written"
head -12 PROJECT_STATUS.md
