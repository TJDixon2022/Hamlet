cd /c/Source/HamLet
# Usage: sh .run-unit/unit406-putback.sh <path> [<path>...]
for p in "$@"
do
  git checkout -- "$p"
  echo "put back $p rc $?"
done
echo "== git diff --stat HEAD -- src tests"
git diff --stat HEAD -- src tests
echo "== status src tests"
git status --short -- src tests
