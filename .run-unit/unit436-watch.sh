cd /c/Source/HamLet
log=.run-unit/unit436-wait2.txt
i=0
while [ $i -lt 54 ]
do
  if grep -q "ENDED\|STILL LIVE" $log; then break; fi
  i=$((i+1))
  sleep 10
done
tail -3 $log
sed -n "s/^UPDATED: //p;s/^NOTE: //p;s/^TASK: //p" PROJECT_STATUS.md
git log --oneline -3
