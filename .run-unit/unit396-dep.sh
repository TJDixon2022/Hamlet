cd /c/Source/HamLet
# Usage: sh .run-unit/unit396-dep.sh <target patch> <prior patch>...
# Restores the tree, applies the priors in order, checks whether the target applies, restores again.
T=$1
shift
git checkout HEAD -- src/Hamlet.RadioEngine/Cw
for p in "$@"; do
  git apply $p
  echo "prior $p rc $?"
done
git apply --check $T
echo "target check rc $?"
git checkout HEAD -- src/Hamlet.RadioEngine/Cw
git status --short -- src
