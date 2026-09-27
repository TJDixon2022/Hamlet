#!/bin/sh
# unit 468 - task 1 commit: CwSender, CwChannel's RenderKeyed, the proof, senders.md, the printouts; then push.
cd /c/Source/HamLet || exit 1
git diff --quiet -- src || { echo "src is not clean - not committing"; exit 1; }
sh tools/status.sh EXECUTING "1 of 3" code none "Task 1 done - five profiles produced, two refused, proof red then green 16 of 16, floors and both lines green; committing"
F=tests/Hamlet.RadioEngine.Tests/Cw/Fixtures
git add -- $F/CwSender.cs $F/TheSenderProfilesAreWhatTheySayTests.cs $F/CwChannel.cs docs/phase-requirements/senders.md PROJECT_STATUS.md || exit 1
for f in .run-unit/unit468-*
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
git commit -q -F .run-unit/unit468-msg1.txt || exit 1
git log --oneline -1
git push -q origin main 2>&1 | tail -3
git status -sb | head -1
