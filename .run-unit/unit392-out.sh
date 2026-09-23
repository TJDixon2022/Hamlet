cd /c/Source/HamLet
# Assembles output.md: this unit's sections, then unit 391's section 4 items 2 to 6 and its
# carried queue from unit 390, verbatim, from the output.md at HEAD.
NOW=$(date "+%Y-%m-%d %H:%M")
new=.run-unit/unit392-output.md
git show HEAD:output.md > .run-unit/unit391-output.md
awk -v now="$NOW" -v tb=.run-unit/unit392-captures-table.md '
/@@NOW@@/ { sub(/@@NOW@@/, now) }
/^@@CAPTURES@@$/ { while ((getline l < tb) > 0) print l; next }
{ print }' .run-unit/unit392-out-top.md > $new
sed -n "374,406p" .run-unit/unit391-output.md >> $new
printf "\n**Unit 390's queue as unit 391 carried it, verbatim:**\n\n" >> $new
sed -n "410,463p" .run-unit/unit391-output.md >> $new
head -c 0 $new
echo "first carried line: $(sed -n 374p .run-unit/unit391-output.md | cut -c1-60)"
echo "last 391 line: $(sed -n 406p .run-unit/unit391-output.md | cut -c1-60)"
echo "first 390 line: $(sed -n 410p .run-unit/unit391-output.md | cut -c1-60)"
grep -n "^UNIT:" $new
grep -n "^## " $new
grep -c "@@" $new
cp $new output.md
wc -l output.md
