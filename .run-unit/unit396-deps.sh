cd /c/Source/HamLet
# Usage: sh .run-unit/unit396-deps.sh <target patch> "<chain>" "<chain>" ...
# Each chain is a space-separated list of prior patches applied in order to a clean Cw before the target is checked.
# Decision 16: never committed; Cw restored to HEAD after each.
T=$1
shift
for chain in "$@"; do
  echo "== with: $chain"
  git checkout HEAD -- src/Hamlet.RadioEngine/Cw
  git clean -q -f -- src/Hamlet.RadioEngine/Cw
  for p in $chain; do
    git apply $p 2>/dev/null
    RC=$?
    echo "  prior $(basename $p) rc $RC"
  done
  git apply --check $T 2>&1 | grep -E "patch failed|already exists|does not exist"
  git apply --check $T 2>/dev/null
  echo "  target check rc $?"
done
git checkout HEAD -- src/Hamlet.RadioEngine/Cw
git clean -q -f -- src/Hamlet.RadioEngine/Cw
echo "== tree after"
git status --short -- src
