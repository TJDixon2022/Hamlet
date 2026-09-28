#!/bin/sh
# unit 478 task 2 - the CW tab: lines 1756 to 1850 (light, buttons, strip, scope) replaced by the scope with the buttons beside it.
cd /c/Source/HamLet || exit 1
F=src/Hamlet.App/Views/MainWindow.axaml
sed -n '1756p;1850p;1851p' $F
head -n 1755 $F > $F.new
sed 's/$/\r/' .run-unit/unit478-axaml-scope.txt >> $F.new
tail -n +1851 $F >> $F.new
mv $F.new $F
grep -n "CwHearingLight\|CwHearingDark\|CwPitchStrip\|IsLit\|CwScopeRow\|I agree" $F
file $F
