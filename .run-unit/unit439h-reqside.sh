# requirement side of the trace: for every HM-REQ id, proving tests and related tests
cd /c/Source/HamLet || exit 1
grep -oE "^\| HM-REQ-[0-9]{3} \|" CW_REQUIREMENTS.md | grep -oE "HM-REQ-[0-9]{3}" > .run-unit/unit439h-reqids.txt
RAW=.run-unit/unit439h-trace-raw.txt
while read id; do
  p=$(awk -F'|' -v I="$id" '$4 ~ I {print $2"."$3}' $RAW | tr '\n' ',' | sed 's/,$//')
  r=$(awk -F'|' -v I="$id" '$5 ~ I {print $2"."$3}' $RAW | tr '\n' ',' | sed 's/,$//')
  pc=$(awk -F'|' -v I="$id" '$4 ~ I' $RAW | wc -l); rc=$(awk -F'|' -v I="$id" '$5 ~ I' $RAW | wc -l)
  echo "$id|$pc|$rc|$p|$r"
done < .run-unit/unit439h-reqids.txt > .run-unit/unit439h-reqside.txt
echo "ids $(wc -l < .run-unit/unit439h-reqids.txt)"
echo "with proving test $(awk -F'|' '$2>0' .run-unit/unit439h-reqside.txt | wc -l)"
echo "with none $(awk -F'|' '$2==0' .run-unit/unit439h-reqside.txt | wc -l)"
echo "with a related test (measures something else) $(awk -F'|' '$3>0' .run-unit/unit439h-reqside.txt | wc -l)"
echo "  of which no proving test $(awk -F'|' '$3>0 && $2==0' .run-unit/unit439h-reqside.txt | wc -l)"
echo "no test of either kind $(awk -F'|' '$3==0 && $2==0' .run-unit/unit439h-reqside.txt | wc -l)"
echo "methods: proves $(awk -F'|' '$4 ~ /HM-REQ/' $RAW | wc -l), none $(awk -F'|' '$4=="none"' $RAW | wc -l), unclear $(awk -F'|' '$4=="unclear"' $RAW | wc -l), related $(awk -F'|' '$5!="-"' $RAW | wc -l), not compiled $(awk -F'|' '$6 ~ /^NOT COMPILED/' $RAW | wc -l)"
