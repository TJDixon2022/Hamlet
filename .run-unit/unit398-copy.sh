cd /c/Source/HamLet/.run-unit
for n in floors carry build commit transmit cmp nums; do
  sed -e "s/unit397/unit398/g" -e "s/TASK n of 3/TASK n of 5/g" unit397-$n.sh > unit398-$n.sh
done
echo "TASK 0 of 5" > unit398-task.txt
ls unit398-*
cat unit398-nums.sh
