cd /c/Source/HamLet
for f in SHACK_FACTS.md src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs CoreHMI.sln MURC.sln
do
  if [ -e "$f" ]; then echo "EXISTS  $f"; else echo "ABSENT  $f"; fi
done
pwd -W
echo "=== ps"
ps -ef | grep -Ei "unit435|dotnet|testhost" | grep -v grep
echo "=== tasklist"
tasklist 2>/dev/null | grep -Ei "dotnet|testhost"
echo "=== date"
date
echo "=== log"
git log --oneline -8
