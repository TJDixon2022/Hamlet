cd /c/Source/HamLet
d=docs/phase-cw/unit392-seams.md
awk -v ns=.run-unit/unit392-namestatus.md -v tb=.run-unit/unit392-seams-table.md '
/^@@NAMESTATUS@@$/ { while ((getline l < ns) > 0) print l; next }
/^@@TABLE@@$/ { while ((getline l < tb) > 0) print l; next }
{ print }' $d > $d.tmp
mv $d.tmp $d
grep -c "^|" $d
grep -n "@@" $d
echo done
