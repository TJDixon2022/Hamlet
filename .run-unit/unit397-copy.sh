cd /c/Source/HamLet/.run-unit
for n in piece measure out deps pair clean floors carry build commit cmp nums transmit log entry exit; do
  sed -e "s/unit396/unit397/g" -e "s/of 4\"/of 3\"/g" -e "s/task 2: piece/task 1: piece/g" -e "s/TASK 3 of/TASK 2 of/g" -e "s/task 3: exit/task 2: exit/g" -e "s/TASK n of 4/TASK n of 3/g" unit396-$n.sh > unit397-$n.sh
done
echo "TASK 0 of 3" > unit397-task.txt
ls unit397-*
