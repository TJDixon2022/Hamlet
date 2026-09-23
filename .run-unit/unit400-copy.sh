cd /c/Source/HamLet/.run-unit
for n in floors carry build commit transmit cmp nums readers adjcmp; do
  sed -e "s/unit399/unit400/g" unit399-$n.sh > unit400-$n.sh
done
echo "TASK 0 of 5" > unit400-task.txt
ls unit400-*
cd /c/Source/HamLet
sh tools/status.sh EXECUTING "TASK 0 of 5" code none "task 0: gate passed, scripts copied, verifying the tree against section 5"
head -6 PROJECT_STATUS.md
date +%H:%M:%S
