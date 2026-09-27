#!/bin/sh
# unit 469 - copy 468's round extras and exit printer under 469's name.
cd /c/Source/HamLet/.run-unit || exit 1
for n in extra exit-print
do
  sed 's/unit468/unit469/g' unit468-$n.sh > unit469-$n.sh
done
cat unit469-exit-print.sh
