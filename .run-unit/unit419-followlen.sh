#!/bin/sh
# unit 419 - the follow-path region's length at entry and now, and whether this unit's diff touches it.
cd /c/Source/HamLet || exit 1
F=src/Hamlet.App/ViewModels/MainWindowViewModel.cs
for REV in 4bd85b35 cb526e01 HEAD
do
  git show "$REV:$F" > .run-unit/unit419-mwvm-at.txt
  S=$(grep -n "private async Task FollowTheMapAsync()" .run-unit/unit419-mwvm-at.txt | cut -d: -f1)
  E=$(grep -n "private async Task EstablishReceiveConditionsAsync(" .run-unit/unit419-mwvm-at.txt | cut -d: -f1)
  C=$(sed -n "${S},${E}p" .run-unit/unit419-mwvm-at.txt | wc -c)
  echo "$REV lines $S to $E, about $C bytes"
done
echo "== this unit's hunks in the file"
git diff 4bd85b35 -- $F | grep "^@@"
echo "== last commits touching the file"
git log --oneline -5 -- $F
