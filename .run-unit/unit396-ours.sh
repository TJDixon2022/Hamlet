cd /c/Source/HamLet
# Usage: sh .run-unit/unit396-ours.sh <file under Cw> - resolves every conflict block to ours, drops theirs, marks resolved
F=src/Hamlet.RadioEngine/Cw/$1
awk '/^<<<<<<< /{s=1; next} /^=======$/{if(s==1){s=2; next}} /^>>>>>>> /{if(s==2){s=0; next}} s!=2{print}' $F > $F.tmp
mv $F.tmp $F
grep -c "^<<<<<<<\|^>>>>>>>" $F
git add -- $F
git reset -q -- $F
git status --short -- src
git diff --stat -- src
