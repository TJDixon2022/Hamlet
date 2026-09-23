cd /c/Source/HamLet
# Builds the captures table rows: measured today beside unit 391's HEAD numbers.
out=.run-unit/unit392-captures-table.md
grep -E "^ ?(unadjudicated/)?cw-2026[^:]*: [0-9]+ characters" .run-unit/unit392-floors-1.txt | sed "s/^ //" | sort > .run-unit/unit392-captures-lines.txt
wc -l < .run-unit/unit392-captures-lines.txt
: > $out
while IFS= read -r line; do
  name=$(echo "$line" | sed -E "s/^([^:]*): .*/\1/")
  c=$(echo "$line" | sed -E "s/.*: ([0-9]+) characters against a floor of ([0-9]+|[a-z ]+), .*/\1/")
  cf=$(echo "$line" | sed -E "s/.*characters against a floor of ([0-9]+), .*/\1/")
  e=$(echo "$line" | sed -E "s/.*, ([0-9]+) elements against ([0-9]+), .*/\1/")
  ef=$(echo "$line" | sed -E "s/.*, ([0-9]+) elements against ([0-9]+), .*/\2/")
  u=$(echo "$line" | sed -E "s/.*, ([0-9]+) unsure where ([0-9]+) were.*/\1 (\2)/")
  hz=$(echo "$line" | sed -E "s/.* at ([0-9.]+) Hz.*/\1/")
  h=$(grep -F "| \`$name\` |" docs/phase-cw/unit391-floors-head.md | head -1)
  hr=$(echo "$h" | awk -F"|" "{print \$3}" | sed "s/^ *//; s/ *$//")
  hc=$(echo "$h" | awk -F"|" "{print \$4}" | tr -d " ")
  he=$(echo "$h" | awk -F"|" "{print \$7}" | tr -d " ")
  echo "| \`$name\` | green | $c | $cf | $((c - cf)) | $e | $ef | $((e - ef)) | $u | $hz | $hc / $he, $hr |" >> $out
done < .run-unit/unit392-captures-lines.txt
cat $out
echo "== lines not matching the plain form"
grep -E "^ ?(unadjudicated/)?cw-2026[^:]*: " .run-unit/unit392-floors-1.txt | grep -vE ": [0-9]+ characters against a floor of [0-9]+, [0-9]+ elements against [0-9]+, " | head
