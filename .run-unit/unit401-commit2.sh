cd /c/Source/HamLet
echo "src vs 5688a8a5:"; git diff --stat 5688a8a5 HEAD -- src; echo end
sh .run-unit/unit401-transmit.sh
sh .run-unit/unit401-commit.sh "step 3 repair: CwDisplacementFloorTests reads the settled transcript, 1 of 6, R12 - NothingIsRefusedBeforeAnythingIsBeingRead red to green, no case green to red" "TASK 2 of 5" "task 2: change a kept 1 of 6; change b next, five silent call sites to a band of 0.005" tests/Hamlet.RadioEngine.Tests/Cw/CwDisplacementFloorTests.cs
date +%H:%M:%S
