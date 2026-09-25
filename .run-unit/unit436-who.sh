cd /c/Source/HamLet
tasklist //FI "PID eq 48584" //V
echo "=== claude processes"
tasklist //V | grep -i claude | cut -c1-220
echo "=== lock"
cat SESSION.lock
echo "=== status"
head -12 PROJECT_STATUS.md
echo "=== launch files"
cat .run-unit/launch.stamp .run-unit/watched.pid .run-unit/watched.born
