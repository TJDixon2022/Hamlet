#!/bin/sh
# unit 480 - derive this unit's helper scripts from unit 479's, with the unit's number, title, ruling and version.
cd /c/Source/HamLet || exit 1
for n in record0 cf build commit status type
do
  sed -e 's/unit479/unit480/g' -e 's/UNIT 479/UNIT 480/g' \
      -e 's/1\.13\.165/1.13.166/g' -e 's/1\.13\.164/1.13.165/g' \
      -e 's/479 - the tolerance follows the signal/480 - the radio points, the bars show, the letters ride on top/g' \
      -e 's/unit 479/unit 480/g' -e 's/HM-DEC-187/HM-DEC-188/g' \
      -e 's/of 3>/of 4>/g' \
      .run-unit/unit479-$n.sh > .run-unit/unit480-$n.sh || exit 1
done
grep -n "479\|480\|1\.13\|HM-DEC" .run-unit/unit480-*.sh | cut -c1-160
