cd /c/Source/HamLet
# Usage: sh .run-unit/unit395-restore.sh <file under Cw> - back to HEAD, clears a conflict
git checkout HEAD -- src/Hamlet.RadioEngine/Cw/$1
echo "restore rc $?"
git status --short -- src
git diff --stat 5688a8a5 HEAD -- src
echo "src against 5688a8a5 end"
