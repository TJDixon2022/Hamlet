cd /c/Source/HamLet
echo "src vs 5688a8a5:"; git diff --stat 5688a8a5 HEAD -- src; echo end
sh .run-unit/unit402-transmit.sh
sh .run-unit/unit402-commit.sh "unit402 task 1: the eight reds traced, asserting nothing - #24 and #41 held by 25 Hz follows restarting the 12 s speed hold at CwDecoder 682; #42 stream fed while the guard blocks at CwDecoder 590, 0 characters reach neither event; #43 to #45 leading edge re-raised at every revision, none green on the settled transcript either; #6 first character and tail; #15 fits 23 wpm to a 12 wpm sender on two seeds" "TASK 2 of 5" "task 1 committed; task 2 group A: follows under the 60 Hz passband stop counting as a discontinuity, CwDecoder 682" tests/Hamlet.RadioEngine.Tests/Cw/TheEightRedsTests.cs docs/phase-cw/unit402-reds.md
printf 'TASK 2 of 5\n' > .run-unit/unit402-task.txt
date +%H:%M:%S
