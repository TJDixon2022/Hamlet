#!/bin/sh
# unit 482 - derive this unit's helper scripts from unit 481's (number and title only).
cd /c/Source/HamLet/.run-unit || exit 1
for n in build cf type commit status
do
  sed -e 's/unit481/unit482/g' -e 's/unit 481/unit 482/g' -e 's/WORK_INSTRUCTION: 481 - the window holds still when the radio moves the dial/WORK_INSTRUCTION: 482 - the owner'"'"'s press lands in the file/' unit481-$n.sh > unit482-$n.sh
done
grep -n "WORK_INSTRUCTION" unit482-status.sh
grep -c unit481 unit482-*.sh
