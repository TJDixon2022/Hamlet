#!/bin/sh
# unit 479 - derive this unit's helper scripts from unit 478's, with the unit's number, title, ruling and version.
cd /c/Source/HamLet || exit 1
for n in record0 cf build commit status
do
  sed -e 's/unit478/unit479/g' -e 's/UNIT 478/UNIT 479/g' \
      -e 's/1\.13\.164/1.13.165/g' -e 's/1\.13\.163/1.13.164/g' \
      -e 's/478 - the scope is the middle picture/479 - the tolerance follows the signal/g' \
      -e 's/unit 478/unit 479/g' -e 's/HM-DEC-186/HM-DEC-187/g' \
      .run-unit/unit478-$n.sh > .run-unit/unit479-$n.sh || exit 1
done
grep -n "47[89]\|1\.13\|HM-DEC" .run-unit/unit479-*.sh | cut -c1-160
