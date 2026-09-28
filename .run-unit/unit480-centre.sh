#!/bin/sh
# unit 480 task 3 - does this unit add "centre" to anything under src/Hamlet.App, and is VoiceTests in the app carry-forward filter.
cd /c/Source/HamLet || exit 1
echo "== added lines under src/Hamlet.App carrying centre"
git diff 8226ccc4 -- src/Hamlet.App | grep -c "^+.*centre"
git diff 8226ccc4 -- src/Hamlet.App | grep "^+.*centre" | cut -c1-160
echo "== VoiceTests named in docs/carry-forward-tests.txt"
grep -c "VoiceTests" docs/carry-forward-tests.txt
