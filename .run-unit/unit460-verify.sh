#!/bin/sh
# unit 460 task 0 - section 3's checks against the tree.
cd /c/Source/HamLet || exit 1
echo "== HEAD"
git log --oneline -1
echo "== PARKED.md header and the 17:37 line, and which commit added it"
head -12 PARKED.md
grep -n "re-bank at 38" PARKED.md
git log --oneline -1 -S "re-bank at 38" -- PARKED.md
echo "== NamedFloors rows"
F=$(grep -rl "class TheNumberCannotBeGamedTests" tests)
echo "$F"
grep -n -E "17:37|032113|032129|RE-BANKED|re-banked" "$F"
echo "== the pair-speed red test"
grep -rl "class TheSpeedFollowsTheSendersMarkPairsTests" tests
echo "== 6a0b65a1 and 796f9af4"
git log --oneline -1 6a0b65a1
git log --oneline -1 796f9af4
git show --stat 6a0b65a1 | tail -5
echo "== watched.rc"
ls .run-unit/watched.rc 2>&1
echo "== carry-forward lines"
grep -n "timeout 480 dotnet test" docs/carry-forward-tests.txt | cut -c1-200
