#!/bin/sh
# unit 467 - task 1 commit: arbitration.md, the fact and the test in one commit, with this unit's .run-unit files and the status; never src.
cd /c/Source/HamLet || exit 1
git diff --quiet -- src || { echo "src is not clean - not committing"; exit 1; }
git status --short | grep -v "^?? .run-unit/unit467-"
sh tools/status.sh EXECUTING "1 of 2" code none "Task 1 done - part (b) narrowed to 10 condition rows and the live row, re-watched red on the 4 losing rows then green; committing"
git add -- docs/phase-requirements/arbitration.md tests/Hamlet.RadioEngine.Tests/Cw/TheArbitrationEarnsItsPlaceFact.cs tests/Hamlet.RadioEngine.Tests/Cw/TheArbitrationEarnsItsPlaceTests.cs PROJECT_STATUS.md || exit 1
for f in .run-unit/unit467-*
do
  case "$f" in
    *.tmp) ;;
    *) git add -- "$f" ;;
  esac
done
echo "== staged, not this unit's run files"
git diff --cached --name-status | grep -v "\.run-unit/unit467-"
git commit -q -F .run-unit/unit467-msg1.txt || exit 1
git log --oneline -1
git push -q origin main 2>&1 | tail -3
git status -sb | head -1
