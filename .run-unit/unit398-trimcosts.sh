cd /c/Source/HamLet
F=tests/Hamlet.RadioEngine.Tests/Cw/WhatDecodeScoringCostsTests.cs
sed -n 282,284p $F
sed -n 382,384p $F
sed -i "283,383d" $F
echo "after:"
tail -n 5 $F
grep -c "\[Fact\]" $F
grep -n "CoarseSpacingHz" $F
