cd /c/Source/HamLet
echo "src vs 5688a8a5:"; git diff --stat 5688a8a5 HEAD -- src; echo end
sh .run-unit/unit401-transmit.sh
sh .run-unit/unit401-commit.sh "unit401 task 1: the six displacement cases measured four ways, asserting nothing; #24's speed properties printed - settled event alone turns the 0.06 case, band 0.005 the smallest at which all five silent cases end CQ DE W1AW K with the image at 1 retune; #24 has 9 characters and 18.46 wpm but SpeedIsReacquiring true" "TASK 2 of 5" "task 1 committed; task 2: change a, CwDisplacementFloorTests line 45 to CharacterSettled, building" tests/Hamlet.RadioEngine.Tests/Cw/TheDisplacementFloorFourWaysTests.cs docs/phase-cw/unit401-closeout.md
printf 'TASK 2 of 5\n' > .run-unit/unit401-task.txt
date +%H:%M:%S
