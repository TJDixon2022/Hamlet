cd /c/Source/HamLet
# Usage: sh .run-unit/unit396-pair.sh <first patch> <second patch> <file to restore first>
git checkout HEAD -- src/Hamlet.RadioEngine/Cw/$3
echo "restore rc $?"
git apply $1
echo "first apply rc $?"
git apply $2
echo "second apply rc $?"
git status --short -- src
