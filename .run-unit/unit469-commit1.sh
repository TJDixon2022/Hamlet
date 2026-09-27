#!/bin/sh
# unit 469 - task 1 commit: CwInterference, the proof, interference.md, CwChannel.Envelope internal, the printouts; then push.
cd /c/Source/HamLet || exit 1
git diff --quiet -- src || { echo "src is not clean - not committing"; exit 1; }
sh tools/status.sh EXECUTING "1 of 3" code none "Task 1 - the INT-* layer proved red then green, round all green; committing CwInterference, the proof and interference.md"
F=tests/Hamlet.RadioEngine.Tests/Cw/Fixtures
git add -- $F/CwInterference.cs $F/TheInterferenceProfilesAreWhatTheySayTests.cs $F/CwChannel.cs docs/phase-requirements/interference.md PROJECT_STATUS.md || exit 1
for f in .run-unit/unit469-*
do
  case "$f" in
    *.tmp) ;;
    *) git add -- "$f" ;;
  esac
done
echo "== staged"
git status --short | grep -v "^??"
echo "== untracked"
git status --short | grep "^??"
git commit -q -F .run-unit/unit469-msg1.txt || exit 1
git log --oneline -1
git push -q origin main 2>&1 | tail -3
git status -sb | head -1
