#!/bin/sh
# unit 468 - copy 467's generic helpers under 468's name, and write the start status.
cd /c/Source/HamLet/.run-unit || exit 1
for n in run build round cf commitrun paritykeep tx gitstate validate v11 alone save live meetsave verify portdiff stamp
do
  sed 's/unit467/unit468/g' unit467-$n.sh > unit468-$n.sh
done
ls unit468-*
cd /c/Source/HamLet || exit 1
sh tools/status.sh EXECUTING "0 of 3" code none "Unit 468 starting - project gate passed; checking HEAD, both plan copies, the channel layer and the port diff before the record is written"
head -12 PROJECT_STATUS.md
