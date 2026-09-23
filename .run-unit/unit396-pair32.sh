cd /c/Source/HamLet
git status --short -- src
git apply .run-unit/unit396-piece-28-ade52536.patch
echo "piece 28 apply rc $?"
git apply .run-unit/unit396-piece-32-c8685e4d.patch
echo "piece 32 apply rc $?"
git status --short -- src
