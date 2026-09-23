cd /c/Source/HamLet
# Usage: sh .run-unit/unit402-putback.sh <path>
git checkout -- "$1"
echo "put back rc $?"
git status --short -- src tests
