#!/bin/sh
# unit 478 task 2 - splice the view model's head: lines 114 to 297 (the light, the strip) replaced.
cd /c/Source/HamLet || exit 1
F=src/Hamlet.App/ViewModels/CwHearingViewModel.cs
sed -n '114p;297p' $F
head -n 113 $F > $F.new
cat .run-unit/unit478-vm-head.cs.txt >> $F.new
tail -n +298 $F >> $F.new
mv $F.new $F
grep -n "Strip\|IsLit\|LitWords\|DarkWords\|LightTip" $F
