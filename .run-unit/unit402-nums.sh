cd /c/Source/HamLet
# Usage: sh .run-unit/unit402-nums.sh <captures output file>
# Prints one table row per capture: name, characters, elements, unsure, tone, pass or fail.
grep -E "^ [^ ].*: [0-9]+ characters against a floor of" $1 | sed -E "s/^ //" | sed -E "s/: ([0-9]+) characters against a floor of ([0-9]+), ([0-9]+) elements against ([0-9]+), ([0-9]+) unsure.* at ([0-9.]+) Hz.*/ | \1 | \3 | \5 | \6 |/" | sed -E "s/^/| /" | sort
