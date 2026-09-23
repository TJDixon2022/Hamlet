cd /c/Source/HamLet
# Writes the generated tables of docs/phase-cw/unit392-seams.md task 1 to .run-unit files.
out=.run-unit/unit392-seams-table.md
echo "| File | Type | At 7e209cb4 | Members absent there (grep) |" > $out
echo "|---|---|---|---|" >> $out
sed -E "s/^([^|]*) \| ([^|]*) \| ([^|]*) \| missing: (.*)$/| \`\1\` | \2 | \3 | \4 |/" .run-unit/unit392-seams-at-7e209cb4.txt >> $out
wc -l $out
out2=.run-unit/unit392-namestatus.md
git diff --name-status 7e209cb4 HEAD -- src/Hamlet.RadioEngine/Cw | sed -E "s/^([AMD])\t(.*)$/| \1 | \`\2\` |/" > $out2
cat $out2
