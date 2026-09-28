#!/bin/sh
# unit 483 - derive this unit's helper scripts from unit 482's (number and title only).
cd /c/Source/HamLet/.run-unit || exit 1
for n in build cf type commit status record0
do
  sed -e 's/unit482/unit483/g' -e 's/unit 482/unit 483/g' -e 's/UNIT 482/UNIT 483/g' -e 's/instruction 482/instruction 483/g' -e 's/1\.13\.169/1.13.170/g' -e 's/1\.13\.168/1.13.169/g' -e 's/482 - the owner'"'"'s press lands in the file/483 - the owner'"'"'s rows name the gate/' -e 's/(the owner'"'"'s press lands in the file)/(the owner'"'"'s rows name the gate)/g' unit482-$n.sh > unit483-$n.sh
done
grep -n "WORK_INSTRUCTION\|Version\|UNIT 483\|instruction 483" unit483-status.sh unit483-record0.sh
grep -c unit482 unit483-*.sh
