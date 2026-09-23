cd /c/Source/HamLet
# Usage: sh .run-unit/unit398-re.sh <Type> "<note>"
# Takes the type's Compile Remove line out of the engine csproj, builds, and runs the type alone if it built.
# If it did not build, the csproj is put back from HEAD and the build is run again to prove the tree.
T=$1
NOTE=$2
P=tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj
sh tools/status.sh EXECUTING "TASK 3 of 5" code none "$NOTE"
grep -n "Compile Remove=.*[\\]$T\.cs" $P
sed -i "/Compile Remove=.*[\\]$T\.cs/d" $P
echo "Compile Remove lines now $(grep -c "Compile Remove" $P)"
sh .run-unit/unit398-build.sh re-$T "$NOTE - build"
if grep -q "Build succeeded" .run-unit/unit398-build-re-$T.txt; then
  sh .run-unit/unit398-floors.sh re-$T 600 "FullyQualifiedName~$T" "TASK 3 of 5" "$NOTE - running the type" --no-build
  grep -E "^\s+(Passed|Failed) " .run-unit/unit398-re-$T.txt | sed -E "s/^\s+//" | sed -E "s/Hamlet.RadioEngine.Tests.(Cw|Audio).//" | cut -c1-200
else
  echo "REFUSED - first error:"
  grep -m1 -E " error " .run-unit/unit398-build-re-$T.txt | cut -c1-400
  git checkout -- $P
  echo "csproj put back, Compile Remove lines $(grep -c "Compile Remove" $P)"
  sh .run-unit/unit398-build.sh re-$T-back "$NOTE - refused, rebuilding the tree"
fi
date +%H:%M:%S
