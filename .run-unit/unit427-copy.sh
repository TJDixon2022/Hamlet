#!/bin/sh
# unit 427 - carry unit 426's runner scripts forward under unit 427's name.
cd /c/Source/HamLet || exit 1
for f in run round cf build fullbuild types commit transmit
do
  sed "s/unit426/unit427/g; s/unit 426/unit 427/g" .run-unit/unit426-$f.sh > .run-unit/unit427-$f.sh
  echo "unit427-$f.sh"
done
