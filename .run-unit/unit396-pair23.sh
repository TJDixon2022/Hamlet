cd /c/Source/HamLet
git status --short -- src
git apply .run-unit/unit396-piece-22-0f48c33e.patch
echo "piece 22 apply rc $?"
git apply .run-unit/unit396-piece-23-b8cad1f9.patch
echo "piece 23 apply rc $?"
git status --short -- src
