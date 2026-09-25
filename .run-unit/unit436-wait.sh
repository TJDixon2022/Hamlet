cd /c/Source/HamLet
log=.run-unit/unit436-wait.txt
echo "wait began $(date) - watching cmd 32764 and unit435 test processes, deadline 10:24" >> $log
n=0
while [ $n -lt 35 ]
do
  tests=$(ps -ef | grep -Ei "unit435-|testhost|dotnet test" | grep -v grep | wc -l)
  host=$(tasklist //FI "PID eq 32764" //NH | grep -c 32764)
  echo "$(date +%H:%M:%S) tests=$tests session=$host" >> $log
  if [ "$tests" -eq 0 ] && [ "$host" -eq 0 ]; then
    echo "ENDED $(date)" >> $log
    tail -4 $log
    exit 0
  fi
  n=$((n+1))
  sleep 150
done
echo "STILL LIVE AT DEADLINE $(date)" >> $log
tail -4 $log
exit 1
