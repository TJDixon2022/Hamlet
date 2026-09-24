#!/bin/sh
# unit 424 - the survey: is the manual in the tree, and every line that names the preamp.
cd /c/Source/HamLet || exit 1
echo "== manual in the tree"
git ls-files | grep -i -E "7300|manual|\.pdf$" | head -20
find . -iname "*7300*" -not -path "./.git/*" 2>/dev/null | head -20
echo "== preamp in src"
grep -rn -i "preamp\|RigField.Preamp" src --include=*.cs | cut -c1-240
echo "== overflow in src"
grep -rn "RigField.Overflow\|Overflow" src/Hamlet.RadioEngine --include=*.cs | cut -c1-200 | head -60
echo "== 6 m and 50 MHz in data"
grep -rn -i "\"6m\"\|6 m\|50000000\|50\.0" data --include=*.json | cut -c1-200 | head -30
