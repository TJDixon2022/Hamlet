#!/bin/sh
# unit 464 - put the clock's time on the UNIT: line of part a, then reassemble output.md.
cd /c/Source/HamLet || exit 1
NOW=$(date "+%Y-%m-%d %H:%M")
sed -i "s/^UNIT:       464 - complete at task 4 of 4, none dropped - .*/UNIT:       464 - complete at task 4 of 4, none dropped - $NOW/" .run-unit/unit464-output-a.md
sh .run-unit/unit464-output.sh | grep -E "UNIT:|VALID|rule 6"
