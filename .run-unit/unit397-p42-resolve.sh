cd /c/Source/HamLet
F=src/Hamlet.RadioEngine/Cw/CwDecoder.cs
sed -n 784p $F
sed -n 786p $F
sed -n 1365p $F
sed -i -e "784,1365d" $F
sed -i -e "783r .run-unit/unit397-p42-resolve.txt" $F
grep -c "^<<<<<<<\|^=======\|^>>>>>>>" $F
git add -- $F
git status --short -- src
git diff --cached --stat HEAD -- src
grep -n "DecodeQueue" $F
