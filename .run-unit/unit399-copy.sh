cd /c/Source/HamLet/.run-unit
for n in floors carry build commit transmit cmp nums; do
  sed -e "s/unit398/unit399/g" unit398-$n.sh > unit399-$n.sh
done
echo "TASK 0 of 5" > unit399-task.txt
ls unit399-*
cd /c/Source/HamLet
sh tools/status.sh EXECUTING "TASK 0 of 5" code none "task 0: gate passed, PHASE_STATUS set to 399 step 3, scripts copied, verifying the tree"
head -6 PROJECT_STATUS.md
date +%H:%M:%S
