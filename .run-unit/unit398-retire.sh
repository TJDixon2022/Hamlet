cd /c/Source/HamLet
# Usage: sh .run-unit/unit398-retire.sh <delete|trim> <path under the engine test project> <lines file> "<commit message>" "<note>"
# delete: git rm the file and take its Compile Remove line out of the csproj.
# trim: the file was already edited; the csproj is not touched until task 3.
MODE=$1
F=$2
L=$3
MSG=$4
NOTE=$5
P=tests/Hamlet.RadioEngine.Tests
R=docs/cw-retired-tests.txt
B=$(basename "$F" .cs)
sh tools/status.sh EXECUTING "TASK 2 of 5" code none "$NOTE"
if [ ! -f $R ]; then cp .run-unit/unit398-retired-header.txt $R; echo "created $R"; fi
cat $L >> $R
if [ "$MODE" = delete ]; then
  git rm -q -- "$P/$F"
  echo "git rm rc $?"
  sed -i "/Compile Remove=.*[\\]$B\.cs/d" $P/Hamlet.RadioEngine.Tests.csproj
  echo "csproj Compile Remove lines now $(grep -c "Compile Remove" $P/Hamlet.RadioEngine.Tests.csproj)"
  sh .run-unit/unit398-commit.sh "$MSG" "TASK 2 of 5" "$NOTE" $R $P/Hamlet.RadioEngine.Tests.csproj PROJECT_STATUS.md
else
  sh .run-unit/unit398-commit.sh "$MSG" "TASK 2 of 5" "$NOTE" $R "$P/$F" PROJECT_STATUS.md
fi
echo "retired lines now $(grep -vc "^#" $R)"
