cd /c/Source/HamLet
git apply -C0 .run-unit/unit396-piece-30-95a5e063.patch
echo "apply -C0 rc $?"
git diff --stat -- src
grep -n "Squelched\|SquelchWithoutAdmission" src/Hamlet.RadioEngine/Cw/CwDecoder.cs
