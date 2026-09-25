cd /c/Source/HamLet
echo "=== dotnet/testhost"
tasklist //NH | grep -Ei "dotnet|testhost" | cut -c1-120
echo "=== unit435/436 scripts"
ps -ef | grep -Ei "unit435-|unit436-" | grep -v grep
echo "=== claude processes"
tasklist //V //NH | grep -i claude | cut -c1-200
echo "=== watched pid"
tasklist //FI "PID eq 53948" //NH
