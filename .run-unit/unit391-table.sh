#!/bin/sh
# Unit 391: the HEAD floor table rows from the captures output, in floor-table order.
cd /c/Source/HamLet || exit 1
f=.run-unit/unit391-floors-head-1.txt
t=tests/Hamlet.RadioEngine.Tests/Cw/TheCapturesThatDecodeKeepDecodingTests.cs
for n in $(grep -o '^        { "[^"]*"' $t | sed 's/.*"\(.*\)"/\1/'); do
  line=$(grep -m1 -F "$n: " $f | sed 's/^\[xUnit[^]]*\] *//; s/^ *//')
  c=$(echo "$line" | sed -E 's/.*: ([0-9]+) characters against a floor of ([0-9]+), ([0-9]+) elements against ([0-9]+), ([0-9]+) unsure where ([0-9]+).* at ([0-9]+) Hz/\1 \2 \3 \4 \5 \6 \7/')
  set -- $c
  if grep -qE "^\s+Failed .*name: \"$n\"" $f; then v=red; elif grep -qE "^\s+Passed .*name: \"$n\"" $f; then v=green; else v=missing; fi
  retired=""
  grep -A1 -F "$n: $1 characters" $f | grep -q "count floor has retired" && retired=" (count floor retired, anchor)"
  echo "| \`$n\` | $v$retired | $1 | $2 | $(( $1 - $2 )) | $3 | $4 | $(( $3 - $4 )) | $5 ($6) | $7 |"
done
