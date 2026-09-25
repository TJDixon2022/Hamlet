cd /c/Source/HamLet || exit 1
git restore --source=998de3fe -- src/Hamlet.RadioEngine/Cw/CwProbabilisticStream.cs
echo "=== src against 998de3fe (task 1, entry src)"
git diff --stat 998de3fe -- src data
echo "=== src against 3980e2a8 (unit 436 exit)"
git diff --stat 3980e2a8 -- src data
echo "end"
