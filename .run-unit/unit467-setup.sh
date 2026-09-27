#!/bin/sh
# unit 466 - copy 465's generic helpers under 466's name, and write the start status.
cd /c/Source/HamLet/.run-unit || exit 1
for n in run build round cf commitrun paritykeep tx gitstate validate v11 alone save live meetsave verify
do
  sed 's/unit465/unit466/g' unit465-$n.sh > unit467-$n.sh
done
ls unit467-*
cd /c/Source/HamLet || exit 1
sh tools/status.sh EXECUTING "0 of 4" code none "Unit 466 starting - project gate passed; reading the tree against section 3 before the record is written"
head -12 PROJECT_STATUS.md
