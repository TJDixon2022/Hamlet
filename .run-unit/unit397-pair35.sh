cd /c/Source/HamLet
git status --short -- src
git add -- src/Hamlet.RadioEngine/Cw/CwSpectralPeak.cs
git apply --3way .run-unit/unit397-piece-35-dfb357ef.patch
echo "35 3way rc $?"
grep -c "^<<<<<<<\|^>>>>>>>" src/Hamlet.RadioEngine/Cw/CwSpectralPeak.cs
grep -n "AverageSpectrum\|static double\[\] Average" src/Hamlet.RadioEngine/Cw/CwSpectralPeak.cs
git status --short -- src
git diff --stat HEAD -- src
