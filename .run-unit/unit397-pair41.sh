cd /c/Source/HamLet
git status --short -- src
git apply .run-unit/unit397-piece-39-a09b36a7.patch
echo "39 apply rc $?"
git apply .run-unit/unit397-piece-41-2828ab69.patch
echo "41 apply rc $?"
git status --short -- src
git diff --stat HEAD -- src
wc -l src/Hamlet.RadioEngine/Cw/CwStreamSplit.cs
