#!/bin/sh
# unit 442 - the six rules validate-output.bat applies, read with grep over the same patterns.
# Not the validator: it could not be run from this session (the call was refused for approval).
cd /c/Source/HamLet || exit 1
F=output.md
head -60 "$F" | grep -c "^UNIT:" | sed 's/^/rule 1 UNIT: lines in first 60: /'
echo "rule 2/3 headings:"
grep "^## " "$F"
grep -c "^## 4\. What's blocking us" "$F" | sed 's/^/rule 4 section 4 headings: /'
awk '/^## 3\. /{on=1; next} /^## 4\. /{on=0} on && NF {n++} END {print "rule 5 section 3 non-blank lines: " n+0}' "$F"
head -60 "$F" | grep -c "READ IN THIS ORDER" | sed 's/^/rule 6 header: /'
head -60 "$F" | grep -c "^A\." | sed 's/^/rule 6 A.: /'
head -60 "$F" | grep -c "^B\." | sed 's/^/rule 6 B.: /'
head -60 "$F" | grep -c "^C\." | sed 's/^/rule 6 C.: /'
head -60 "$F" | grep -cE "raises [0-9]+ item" | sed 's/^/rule 6 raises N items: /'
