#!/bin/sh
# unit 478 - make this unit's helper scripts from unit 477's: names, work instruction, entry commit.
cd /c/Source/HamLet/.run-unit || exit 1
for f in status commit build cf run exit-print
do
  sed -e 's/unit477/unit478/g' -e 's/unit 477/unit 478/g' \
      -e "s/477 - bars, not waves/478 - the scope is the middle picture/" \
      -e 's/4d1d24cf/1b738a76/g' \
      -e 's/of 4>/of 3>/g' \
      unit477-$f.sh > unit478-$f.sh
done
grep -n "478\|1b738a76" unit478-status.sh unit478-exit-print.sh | cut -c1-140 | head
cat unit478-exit-print.sh
git -C /c/Source/HamLet rev-parse --short=8 HEAD
grep -n "<Version>" /c/Source/HamLet/Directory.Build.props
