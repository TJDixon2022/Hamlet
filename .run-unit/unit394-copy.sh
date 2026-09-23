cd /c/Source/HamLet
for f in one carry floors transmit commit; do
  sed "s/unit393/unit394/g" .run-unit/unit393-$f.sh > .run-unit/unit394-$f.sh
done
ls .run-unit/unit394-*
