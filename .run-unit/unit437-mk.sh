cd /c/Source/HamLet/.run-unit || exit 1
for n in run round cf build commit
do
  sed 's/unit436/unit437/g; s/unit 436/unit 437/g' unit436-$n.sh > unit437-$n.sh
done
sed -i 's/--no-build/--no-build/' unit437-round.sh
grep -c unit437 unit437-*.sh
