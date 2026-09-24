#!/bin/sh
# unit 424 task 3 - run HowMuchTheApplicationSaysTests on the task 2 tree (task 3's source edits stashed), then restore them.
cd /c/Source/HamLet || exit 1
git stash push -u -q -m unit424-t3-src -- src/Hamlet.App/ViewModels/MainWindowViewModel.cs src/Hamlet.RadioEngine/Rig/ReceiveAdvice.cs src/Hamlet.RadioEngine/Rig/ReceiverSetupVoice.cs tests/Hamlet.App.Tests/ViewModels/OneVoiceOnThePreampTests.cs || exit 1
git stash list | head -1
sh .run-unit/unit424-build.sh t3-bisect "TASK 3 of 4" "Task 3: rebuilding the task 2 tree to see whether the length test fails there too"
sh .run-unit/unit424-types.sh t3-bisect "TASK 3 of 4" HowMuchTheApplicationSaysTests
git stash pop -q
git status --short src tests
