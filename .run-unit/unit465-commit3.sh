#!/bin/sh
# unit 465 - task 3 commit: the live wiring, the sheet line, the two tests, the live printer; then push.
cd /c/Source/HamLet || exit 1
sh tools/status.sh EXECUTING "3 of 4" code none "Task 3 - live wiring in; 120 and 121 green; live corpus identical to ours on all 35; decode 52 s to 65 s; committing"
C=src/Hamlet.RadioEngine/Cw
git add -- PROJECT_STATUS.md $C/CwSecondReader.cs $C/CwDecoder.cs $C/CwArbiter.cs src/Hamlet.App/ViewModels/MainWindowViewModel.cs tests/Hamlet.RadioEngine.Tests/Cw/BothDecodersReadTheSameSamplesTests.cs tests/Hamlet.App.Tests/ViewModels/TheOperatorSeesOneTranscriptTests.cs tests/Hamlet.RadioEngine.Tests/Cw/WhereTheTwoReadingsMeetFact.cs || exit 1
for f in .run-unit/unit465-*
do
  case "$f" in
    *.tmp) ;;
    *) git add -- "$f" ;;
  esac
done
git status --short | grep -v "^??"
git status --short -- src tests | grep "^??"
git commit -q -F .run-unit/unit465-msg3.txt || exit 1
git log --oneline -1
git push -q origin main 2>&1 | tail -3
git status -sb | head -1
