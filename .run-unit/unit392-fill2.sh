cd /c/Source/HamLet
d=docs/phase-cw/unit392-floors.md
awk -v tb=.run-unit/unit392-captures-table.md '
/^@@CAPTURES@@$/ { while ((getline l < tb) > 0) print l; next }
{ print }' $d > $d.tmp
mv $d.tmp $d
grep -c "^| \`" $d
grep -n "@@" $d
echo done
