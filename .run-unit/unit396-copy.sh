cd /c/Source/HamLet
# Copies unit 395's tooling to unit396-*, the unit number edited; the doc name unit395-rework.md is kept where the rows go.
for n in piece measure out pair dep floors carry build commit cmp nums drop restore verify; do
  sed -e "s/unit395-/unit396-/g" -e "s/unit396-rework.md/unit395-rework.md/g" .run-unit/unit395-$n.sh > .run-unit/unit396-$n.sh
  echo "copied $n"
done
cp .run-unit/unit394-transmit.sh .run-unit/unit396-transmit.sh
grep -n "unit39" .run-unit/unit396-*.sh | grep -v "unit396-" | head
grep -n "rework" .run-unit/unit396-*.sh
