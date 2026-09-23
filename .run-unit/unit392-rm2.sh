cd /c/Source/HamLet
# Task 2: after the app hunk to ElementPitchLine nothing outside Cw uses these three; decision 4 deletes them.
C=src/Hamlet.RadioEngine/Cw
git rm -q -f $C/CwElementPitch.cs $C/CwStreamSplit.cs $C/CwJointCutter.cs
git status --short -- src tests
echo "== diff --stat 7e209cb4 -- Cw"
git diff --stat 7e209cb4 -- $C
