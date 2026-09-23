cd /c/Source/HamLet
# Usage: sh .run-unit/unit397-clean.sh [new file under Cw ...] - drops files a failed --3way added, restores Cw to HEAD
for f in "$@"; do
  git rm -q -f --cached -- src/Hamlet.RadioEngine/Cw/$f 2>/dev/null
  git clean -q -f -- src/Hamlet.RadioEngine/Cw/$f
done
git checkout HEAD -- src/Hamlet.RadioEngine/Cw
git status --short -- src
git diff --stat 5688a8a5 HEAD -- src
echo "clean end"
